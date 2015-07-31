using Microsoft.WindowsAzure.MobileServices;
using MixRadio;
using MixRadio.Types;
using SongSwap81.DataModel;
using SongSwap81.Keys;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SongSwap81
{
    sealed partial class MainPage : Page
    {
        private MusicClient cli;
        private int searchCalls = 0;

        public MainPage()
        {
            this.InitializeComponent();
            cli = new MusicClient(APIKeys.MixRadioKey);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //attempt to stop searches from happening while typing
            searchCalls++;
            updateSearch(textBox.Text, searchCalls);
        }

        async private void updateSearch(string searchText, int myCallNum)
        {
            if (searchText != "" && myCallNum == searchCalls)
            {
                int numResults = 20, startIndex = 0;
                ListResponse<MusicItem> search;
                ObservableCollection<Track> tracks = new ObservableCollection<Track>();
                listView.ItemsSource = tracks;
                do
                {
                    search = await cli.SearchAsync(searchText, Category.Track, null, null, null, startIndex, numResults);
                    if (search.Succeeded)
                    {
                        foreach (MusicItem track in search)
                        {
                            if (myCallNum != searchCalls)
                            {
                                break;
                            }
                            Response<Product> prodRes = await cli.GetProductAsync(track.Id);
                            if (prodRes.Succeeded && prodRes.Result.Category == Category.Track && !prodRes.Result.ParentalAdvisory && tracks.Count < 20)
                            {
                                tracks.Add(new Track(prodRes.Result.Performers[0].Name, prodRes.Result.Name, prodRes.Result.TakenFrom.Name, prodRes.Result.Thumb200Uri.OriginalString, prodRes.Result.Id, songCategory.nostalgic));
                            }
                        }
                        if (myCallNum == searchCalls && searchCalls > 200)
                        {
                            searchCalls = 0;
                        }
                    }
                    startIndex += 21;
                } while (myCallNum == searchCalls && search.TotalResults > numResults && tracks.Count < numResults);
            }
            else if (searchText == "")
            {
                listView.ItemsSource = null;
            }
        }

        private void playSong(string songId)
        {
            mediaControl.Stop();
            Uri previewUri = cli.GetTrackSampleUri(songId);
            mediaControl.Source = previewUri;
            mediaControl.Play();
        }

        async private void Grid_Tapped(object sender, object e)
        {
            Grid selected = sender as Grid;
            if (selected != null)
            {
                Track track = selected.DataContext as Track;
                IMobileServiceTable<Track> table = App.SongSwap81Client.GetTable<Track>();
                await startNextTrack(table);
                await addSelectedTrack(table, track);
            }
        }

        async private Task startNextTrack(IMobileServiceTable<Track> table)
        {
            List<Track> tracks = await table.CreateQuery().Where(Track => Track.Category == songCategory.nostalgic).ToListAsync();
            Random rand = new Random();
            if (tracks.Count > 0)
            {
                Track track = tracks[rand.Next(tracks.Count)];
                playSong(track.SongId);
                track.NumRemainingPlays--;
                if (track.NumRemainingPlays > 0)
                {
                    await table.UpdateAsync(track);
                    Debug.WriteLine("Updated {0}, {1} plays remaining", track.Title, track.NumRemainingPlays);
                }
                else
                {
                    await table.DeleteAsync(track);
                    Debug.WriteLine("Deleted {0}", track.Title);
                }
            }
        }

        async private Task addSelectedTrack(IMobileServiceTable<Track> table, Track track)
        {
            try
            {
                await table.InsertAsync(track);
                Debug.WriteLine("Inserted {0} as {1}", track.Title, track.Id);
            }
            catch (MobileServiceConflictException e)
            {
                Track oldTrack = await table.LookupAsync(track.Id);
                oldTrack.NumRemainingPlays = 10;
                await table.UpdateAsync(oldTrack);
                Debug.WriteLine("Inserted {0} already present, reset to 10", track.Title, track.Id);
            }

        }
    }
}