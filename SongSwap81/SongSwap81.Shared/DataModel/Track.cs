using Newtonsoft.Json;

namespace SongSwap81.DataModel
{
    class Track
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "artist")]
        public string Artist { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "album")]
        public string Album { get; set; }
        [JsonProperty(PropertyName = "albumcoveruri")]
        public string AlbumCoverUri { get; set; }
        [JsonProperty(PropertyName = "songid")]
        public string SongId { get; set; }
        [JsonProperty(PropertyName = "category")]
        public songCategory Category { get; set; }
        [JsonProperty(PropertyName ="numremainingplays")]
        public int NumRemainingPlays { get; set; }
        public Track(string artist, string title, string album, string albumCoverUri, string songId, songCategory category)
        {
            Artist = artist;
            Title = title;
            Album = album;
            AlbumCoverUri = albumCoverUri;
            SongId = songId;
            Category = category;
            NumRemainingPlays = 10;
        }
    }
    public enum songCategory
    {
        happy, sad, angry, calm, nostalgic
    }
}
