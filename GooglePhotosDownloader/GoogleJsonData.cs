using System;
using System.Collections.Generic;

namespace GooglePhotosDownloader
{
    public class MediaList
    {
        public List<MediaItem> mediaItems { get; set; }
        public string nextPageToken { get; set; }
    }

    public class MediaItem
    {
        public string id { get; set; }
        public string productUrl { get; set; }
        public string baseUrl { get; set; }
        public string mimeType { get; set; }
        public MediaMetadata mediaMetadata { get; set; }
        public string filename { get; set; }
    }

    public class MediaMetadata
    {
        public DateTime creationTime { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public Photo photo { get; set; }
    }

    public class Photo
    {
        public string cameraMake { get; set; }
        public string cameraModel { get; set; }
        public double focalLength { get; set; }
        public double apertureFNumber { get; set; }
        public int isoEquivalent { get; set; }
    }
}
