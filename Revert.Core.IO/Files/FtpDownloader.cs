using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Revert.Core.IO.Files
{
    public class FtpDownloadModel
    {
        public FtpDownloadModel(string uri, string saveAsFilePath)
        {
            this.uri = uri;
            this.saveAsFilePath = saveAsFilePath;
        }

        public string uri { get; set; }
        public string saveAsFilePath { get; set; }
        public float downloadProgress { get; set; }
    }

    public static class FtpDownloader
    {
        public static async Task<FileInfo> GetFileStreamAsync(FtpDownloadModel details)
        {
            await Task.Run(() => {
                FtpWebRequest sizeRequest = (FtpWebRequest)WebRequest.Create(details.uri);
                sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                var fileSize = sizeRequest.GetResponse().ContentLength;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(details.uri);
                //request.Credentials = new NetworkCredential("username", "password");
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                using (Stream fileStream = File.Create(details.saveAsFilePath))
                {
                    byte[] buffer = new byte[10240];
                    int read;
                    while ((read = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, read);
                        details.downloadProgress = fileStream.Position / fileSize;
                    }
                    details.downloadProgress = 1.0f;
                }
            });

            var fileInfo = new FileInfo(details.saveAsFilePath);
            return fileInfo;
        }
    }
}
