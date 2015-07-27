using MixRadio;
using MixRadio.Types;
using SongSwap81.DataModel;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using static SongSwap81.Keys.APIKeys;

namespace SongSwap81
{
    sealed partial class MainPage : Page
    {
        MusicClient cli;
        int searchCalls = 0;

        public MainPage()
        {
            this.InitializeComponent();
            cli = new MusicClient(MixRadioKey);
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
                int numResults = 20, numProcessed = 0;
                ListResponse<MusicItem> search = await cli.SearchAsync(searchText, Category.Track, null, null, null, 0, numResults);
                ObservableCollection<Track> tracks = new ObservableCollection<Track>();
                listView.ItemsSource = tracks;
                if (search.Succeeded)
                {
                    foreach (MusicItem track in search)
                    {
                        if (myCallNum != searchCalls)
                        {
                            break;
                        }
                        numProcessed++;
                        Response<Product> prodRes = await cli.GetProductAsync(track.Id);
                        if (prodRes.Succeeded && prodRes.Result.Category == Category.Track)
                        {
                            tracks.Add(new Track(prodRes.Result.Performers[0].Name, prodRes.Result.Name, prodRes.Result.TakenFrom.Name, prodRes.Result.Thumb200Uri.OriginalString, prodRes.Result.Id, songCategory.nostalgic));
                        }
                    }
                    if (myCallNum == searchCalls && searchCalls > 200)
                    {
                        searchCalls = 0;
                    }
                }
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

        private async void Grid_Tapped(object sender, object e)
        {
            Grid selected = sender as Grid;
            if (selected != null)
            {
                var track = selected.DataContext as Track;
                playSong(track.SongId);
                var table = App.SongSwap81Client.GetTable<Track>();
                await table.InsertAsync(track);
                Debug.WriteLine("Inserted: {0}", track.Id);
                var x = await table.ToListAsync();
                Debug.WriteLine("First ele is {0}", x[0]);
            }
        }
    }
}
