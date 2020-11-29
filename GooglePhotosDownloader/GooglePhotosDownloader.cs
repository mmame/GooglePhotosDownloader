using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace GooglePhotosDownloader
{
    public class GooglePhotosDownloader
    {
        public UserCredential UserCredential { get; private set; }
        public string GoogleUserName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public int PageSize { get; set; }

        public string TargetRootDirectory { get; set; }

        public GooglePhotosDownloader(string googleUserName, string clientId, string clientSecret, string targetRootDirectory)
        {
            this.GoogleUserName = googleUserName;
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.TargetRootDirectory = targetRootDirectory;
            PageSize = 100;
        }

        public void Authenticate(string clientSecrets)
        {
            string[] scopes = { "https://www.googleapis.com/auth/photoslibrary.readonly" };

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(clientSecrets)))
            {
                UserCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    GoogleUserName,
                    CancellationToken.None,
                    new FileDataStore("GooglePhotosDownloader", false)).Result;
            }
        }


        public MediaList GetMediaList(string nextPageToken)
        {
            if (UserCredential == null)
            {
                throw new InvalidOperationException();
            }

            string url;
            
            if (string.IsNullOrEmpty(nextPageToken))
            {
                url = string.Format("https://photoslibrary.googleapis.com/v1/mediaItems?pageSize={0}", PageSize);
            }
            else
            {
                url = string.Format("https://photoslibrary.googleapis.com/v1/mediaItems?pageSize={0}&pageToken={1}", PageSize, nextPageToken);
            }

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add("client_id", ClientId);
            httpWebRequest.Headers.Add("client_secret", ClientSecret);
            httpWebRequest.Headers.Add("Authorization:" + UserCredential.Token.TokenType + " " + UserCredential.Token.AccessToken);
            httpWebRequest.Method = "GET";

            HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    MediaList responseObject = JsonConvert.DeserializeObject<MediaList>(reader.ReadToEnd());
                    return responseObject;
                }
            }
        }

        public string GetFileNameFromMediaItem(MediaItem mediaItem, bool createDirectory)
        {
            //group media files by YYYY/MM
            string subDirectory = string.Format("{0:yyyy}/{0:MM}", mediaItem.mediaMetadata.creationTime);
            string fullDirectory = Path.Combine(TargetRootDirectory, subDirectory);
            if (createDirectory)
            {
                Directory.CreateDirectory(fullDirectory);
            }
            string pathAndFileName = Path.Combine(fullDirectory, mediaItem.filename);

            return pathAndFileName;
        }

        public void DownloadMediaItem(MediaItem mediaItem)
        {
            //download mediaItem
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}=d", mediaItem.baseUrl));
            httpWebRequest.Method = "GET";

            HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;

            using (Stream responseStream = response.GetResponseStream())
            {
                var fileName = GetFileNameFromMediaItem(mediaItem, true);
                using (FileStream fs = File.OpenWrite(fileName))
                {
                    responseStream.CopyTo(fs);
                }
            }
        }
    }
}
