using LittleWeebLibrary.Models;
using System;

namespace LittleWeebLibrary.EventArguments
{
    public class DownloadUpdateEventArgs
    {
        public JsonDownloadInfo downloadInfo { get; set; } = new JsonDownloadInfo();

        public override string ToString()
        {
            return downloadInfo.ToString();
        }

    }
}
