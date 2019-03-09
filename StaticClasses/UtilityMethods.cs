using PCLExt.FileStorage;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LittleWeebLibrary.StaticClasses
{
    public class UtilityMethods
    {

        public enum OperatingSystems
        {
            Windows,
            OsX,
            Linux,
            Unknown
        }

        public static string RemoveSpecialCharacters(string str)
        {           
            return Regex.Replace(str, "[^0-9A-Za-z ,]", " ");
        }


        public static long GetEpoch()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan mili = DateTime.Now - epoch;
            return (long)mili.TotalMilliseconds;
        }

        public static string IntToRoman(int num)
        {
            string[] thou = { "", "M", "MM", "MMM" };
            string[] hun = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
            string[] ten = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
            string[] ones = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
            string roman = "";
            roman += thou[(int)(num / 1000) % 10];
            roman += hun[(int)(num / 100) % 10];
            roman += ten[(int)(num / 10) % 10];
            roman += ones[num % 10];

            return roman;
        }

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

        public static long GetFreeSpaceKbits(string path)
        {
            DriveInfo[] systemDrives = DriveInfo.GetDrives();
            foreach (DriveInfo i in systemDrives)
            {
                if (path.IndexOf(i.Name) > -1)
                {
                    return (i.TotalFreeSpace / 128); //bytes -> kbits
                }
            }

            return 0;

        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                bytes = mso.ToArray();
            }

          
            return bytes;
        }

        public static string Unzip(byte[] bytes)
        {
            string toreturn = string.Empty;
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                toreturn = Encoding.UTF8.GetString(mso.ToArray());
            }

            return toreturn;
        }


        public static bool WriteBinaryFile(string path, byte[] value)
        {
            bool toreturn = false;
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite)))
                {
                    writer.Write(value);
                }
                toreturn = true;
            }
            catch
            {
                toreturn = false;
            }
            return toreturn;
        }

        public static byte[] ReadBinaryFile(string path)
        {
            byte[] value = new byte[0];
          
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite)))
            {
                const int bufferSize = 4096;
                using (var ms = new MemoryStream())
                {
                    byte[] buffer = new byte[bufferSize];
                    int count;
                    while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                        ms.Write(buffer, 0, count);
                    value = ms.ToArray();
                }
            }
            return value;
        }


        public static OperatingSystems CheckOperatingSystems()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                return OperatingSystems.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OperatingSystems.OsX;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OperatingSystems.Windows;
            }

            return OperatingSystems.Unknown;

        }


        public static string BasePath()
        {
            string path = "";
#if __ANDROID__
             path = PortablePath.Combine(PortablePath.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LittleWeeb"), "DataBase");
#else
            switch (UtilityMethods.CheckOperatingSystems())
            {
                case UtilityMethods.OperatingSystems.Linux:
                    path = PortablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".LittleWeeb");
                    break;
                case UtilityMethods.OperatingSystems.Windows:
                    path = PortablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LittleWeeb");
                    break;
                case UtilityMethods.OperatingSystems.OsX:
                    path = PortablePath.Combine(PortablePath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Library"), ".LittleWeeb");
                    break;
            }
#endif

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// https://stackoverflow.com/a/40775015/4564466
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static int LevenshteinDistance(string source, string target)
        {
            // degenerate cases
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[target.Length + 1];
            int[] v1 = new int[target.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < source.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (source[i] == target[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[target.Length];
        }

        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = LevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        

    }
}
