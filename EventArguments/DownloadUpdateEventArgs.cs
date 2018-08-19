using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.EventArguments
{
    public class DownloadUpdateEventArgs
    {
        public string id { get; set; }
        public string animeid { get; set; }
        public string animeTitle { get; set; }
        public string animeCoverOriginal { get; set; }
        public string animeCoverSmall { get; set; }
        public string episodeNumber { get; set; }
        public string bot { get; set; }
        public string pack { get; set; }
        public string progress { get; set; }
        public string speed { get; set; }
        public string status { get; set; }
        public string filename { get; set; }
        public string filesize { get; set; }
        public int downloadIndex { get; set; }
        public string fullfilepath{ get; set; }

        public override string ToString()
        {
            string toReturn = string.Empty;
            toReturn += "id: " + id + Environment.NewLine;
            toReturn += "animeid: " + animeid + Environment.NewLine;
            toReturn += "episodeNumber: " + episodeNumber + Environment.NewLine;
            toReturn += "bot: " + bot + Environment.NewLine;
            toReturn += "pack: " + pack + Environment.NewLine;
            toReturn += "progress: " + progress + Environment.NewLine;
            toReturn += "speed: " + speed + Environment.NewLine;
            toReturn += "status: " + status + Environment.NewLine;
            toReturn += "filename: " + filename + Environment.NewLine;
            toReturn += "filesize: " + filesize + Environment.NewLine;
            toReturn += "downloadIndex: " + downloadIndex.ToString() + Environment.NewLine;
            toReturn += "fullfilepath: " + fullfilepath+ Environment.NewLine;
            return toReturn;
        }

    }
}
