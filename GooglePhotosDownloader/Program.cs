using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GooglePhotosDownloader
{
    class Program
    {
        /* GooglePhotosDownloader Google API client "secrets". Well they are no longer kept "secret" ;-) */
        /* DISCLAIMER: **YOU** UNDERSTAND THE RISK OF USING THAT APPLICATION. **YOU** KNOW WHAT YOU ARE DOING */
        static string ClientID = "1071471685143-0d9geh59sod45uno09r4vam27f8l5uhi.apps.googleusercontent.com";
        static string ClientSecret = "kmPtzlP1AKAwq6BMnOZ9VEyg";
        static string ClientSecrets = @"{""installed"":{""client_id"":""1071471685143-0d9geh59sod45uno09r4vam27f8l5uhi.apps.googleusercontent.com"",""project_id"":""windows-photo-do-1585669561289"",""auth_uri"":""https://accounts.google.com/o/oauth2/auth"",""token_uri"":""https://oauth2.googleapis.com/token"",""auth_provider_x509_cert_url"":""https://www.googleapis.com/oauth2/v1/certs"",""client_secret"":""kmPtzlP1AKAwq6BMnOZ9VEyg"",""redirect_uris"":[""urn:ietf:wg:oauth:2.0:oob"",""http://localhost""]}}";


        static string targetRootDirectory = string.Empty;
        static string googleUserName = string.Empty;

        enum ExitCode : int
        {
            Success = 0,
            InvalidArguments = 1,
            UnknownError = 2
        }

        static int Main(string[] args)
        {
            try
            {
                if (!ParseArguments(args))
                {
                    ShowUsage();
                    return (int)ExitCode.InvalidArguments;
                }

                var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 20 };

                GooglePhotosDownloader googlePhotosDownloader = new GooglePhotosDownloader(googleUserName, ClientID, ClientSecret, targetRootDirectory);
                googlePhotosDownloader.Authenticate(ClientSecrets);

                MediaList mediaListResponse = null;
                do
                {
                    mediaListResponse = googlePhotosDownloader.GetMediaList(mediaListResponse != null ? mediaListResponse.nextPageToken : string.Empty);
                    if (mediaListResponse != null)
                    {
                        if (mediaListResponse.mediaItems != null && mediaListResponse.mediaItems.Count > 0)
                        {
                            Console.WriteLine("------------------------Retrieving media files--------------------------------");
                            Parallel.ForEach(mediaListResponse.mediaItems, parallelOptions, item =>
                            {
                                var targetFile = googlePhotosDownloader.GetFileNameFromMediaItem(item, false);
                                if (File.Exists(targetFile))
                                {
                                    Console.WriteLine(string.Format("File {0} already exists - skipped", targetFile));
                                }
                                else
                                {
                                    try
                                    {
                                        Console.WriteLine(string.Format("Download {0}", targetFile));
                                        googlePhotosDownloader.DownloadMediaItem(item);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(string.Format("Failed to download {0}: {1}", targetFile, ex));
                                    }
                                }
                            }
                            );
                        }
                    }
                } while (mediaListResponse != null && !string.IsNullOrEmpty(mediaListResponse.nextPageToken));

                Console.WriteLine("---------------------Finished receiving media files-----------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured: " + ex.Message);
                return (int)ExitCode.UnknownError;
            }

            return (int)ExitCode.Success;
        }

        static bool ParseArguments(string[] args)
        {
            int state = 0;

            foreach (string arg in args)
            {
                switch (state)
                {
                    case 0:
                        switch (arg.ToLower())
                        {
                            case "targetdir":
                                state = 1;
                                break;
                            case "username":
                                state = 2;
                                break;
                            default:
                                return false;
                        }
                        break;
                    
                    case 1:
                        targetRootDirectory = arg;
                        state = 0;
                        break;
                    
                    case 2:
                        googleUserName = arg;
                        state = 0;
                        break;
                    
                    default:
                        throw new ApplicationException("StupidProgrammer error");
                }
            }

            //check for mandatory arguments
            if (!string.IsNullOrEmpty(targetRootDirectory) && !string.IsNullOrEmpty(googleUserName))
            {
                return true;
            }         
            return false;
        }

        static void ShowUsage()
        {
            Console.WriteLine("Google Photos Downloader Utility");
            Console.WriteLine("This is Free Software.");
            Console.WriteLine("Downloads all Photos from given Google account to a local directory. ");
            Console.WriteLine("The Photos will be automatically grouped in subfolders per year and month");
            Console.WriteLine(string.Empty);
            Console.WriteLine("usage: GooglePhotosDownloader -targetdir <directory> -username <google user name> [options]");
            Console.WriteLine("arguments:");
            Console.WriteLine("\t-targetdir\t\t\tDirectory where the downloaded files will be stored to");
            Console.WriteLine("\t-username\t\t\tgoogle username");
        }
    }
}
