using Microsoft.WindowsAzure.Mobile.Service;

namespace SongSwap81MobileService.DataObjects
{
    public class Track : EntityData
    {
        public string Artist { get; set; }

        public string Title { get; set; }

        public string Album { get; set; }

        public string AlbumCoverUri { get; set; }

        public string SongId { get; set; }

        public songCategory Category { get; set; }
    }
    public enum songCategory
    {
        happy, sad, angry, calm, nostalgic
    }
}
