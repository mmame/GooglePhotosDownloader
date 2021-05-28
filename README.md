# GooglePhotosDownloader
Downloads all Photos from given Google account to a local directory

The Photos will be automatically grouped in subfolders per year and month

In 2019, Google decided to remove the possibility to sync Google Photos with Google Drive. 
There exist several cloud solutions which aim to fill that gap. But the solutions I tried are either not free or incredibly slow.

So I created this tiny utility to download all Google Photos to a local directory - which of course could refer to a folder synced with Google Drive  ;-)

usage: GooglePhotosDownloader -targetdir <directory> -username <google user name> [options]
arguments:
        -targetdir                      Directory where the downloaded files will be stored to
        -username                       google username
        -reauthenticate                 add this argument when you receive a 401 error
  
  Example: GooglePhotosDownloader.exe targetdir "G:\Google Fotos" username myemail@gmail.com
