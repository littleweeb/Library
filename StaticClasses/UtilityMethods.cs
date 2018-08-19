using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LittleWeebLibrary.StaticClasses
{
    public class UtilityMethods
    {
        public static string GenerateUsername(int requestedLength)
        {
            
            Random rnd = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
            string[] vowels = { "a", "e", "i", "o", "u" };

            string word = "";

            if (requestedLength == 1)
            {
                word = vowels[rnd.Next(0, vowels.Length - 1)];
            }
            else
            {
                for (int i = 0; i < requestedLength; i += 2)
                {
                    word += consonants[rnd.Next(0, consonants.Length - 1)] + vowels[rnd.Next(0, vowels.Length - 1)];
                }

                word = word.Replace("q", "qu").Substring(0, requestedLength); // We may generate a string longer than requested length, but it doesn't matter if cut off the excess.
            }

            return word;

        }

        public static long GetFreeSpace(string path)
        {
            DriveInfo[] systemDrives = DriveInfo.GetDrives();
            foreach (DriveInfo i in systemDrives)
            {
                if (path.IndexOf(i.Name) > -1)
                {
                    return i.TotalFreeSpace;
                }
            }

            return 0;

        }

        public static bool IsLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
    }
}
