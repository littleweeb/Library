using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.EventArguments
{
    public class IrcClientDownloadEventArgs
    {
        public string FileName { get; set; }
        public string FileLocation { get; set; }
        public string DownloadSpeed { get; set; }
        public long FileSize { get; set; }
        public int DownloadProgress { get; set; }
        public string DownloadStatus { get; set; }
        public override string ToString()
        {
            string toReturn = string.Empty;
            toReturn += "FileName: " + FileName + Environment.NewLine;
            toReturn += "FileLocation: " + FileLocation + Environment.NewLine;
            toReturn += "DownloadSpeed: " + DownloadSpeed.ToString() + Environment.NewLine;
            toReturn += "FileSize: " + FileSize.ToString() + Environment.NewLine;
            toReturn += "DownloadProgress: " + DownloadProgress.ToString() + Environment.NewLine;
            toReturn += "DownloadStatus " + DownloadStatus.ToString() + Environment.NewLine;
            return toReturn;
        }
    }
}
