using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LittleWeebLibrary.Handlers
{

    public interface IDebugHandler
    {
        void UpdateDebugEvents(List<IDebugEvent> debugEvents);
        void SetDebugEvents(List<IDebugEvent> debugEvents);
    }

    public class DebugHandler : IDebugHandler, ISettingsInterface
    {
        private string currentLog;

        private bool DebugWriteAble = false;
        private string DebugFileName = "littleweeb_debug_log.log";
        private string DebugPath = "";
        private string[] DebugTypes = new string[] { "ENTRY", "PARAMETERS", "INFO", "WARNING", "ERROR", "SEVERE" };
        private string[] DebugSourceTypes = new string[] { "CONSTRUCTOR", "METHOD", "EVENT", "TASK", "EXTERNAL(LIBRARY)" };
        private LittleWeebSettings LittleWeebSettings;


        public DebugHandler(ISettingsHandler settingsHandler)
        {

            LittleWeebSettings = settingsHandler.GetLittleWeebSettings();


            foreach (int level in LittleWeebSettings.DebugLevel)
            {
                WriteTrace("DEBUG LEVEL: " + level);
            }

            currentLog = "";
           

            CreateFile();
            WriteTrace("Succesfully initiated debug handler!");

        }

        public void SetDebugEvents(List<IDebugEvent> debugEvents)
        {
            foreach (IDebugEvent debugEvent in debugEvents)
            {
                try
                {

                    debugEvent.OnDebugEvent += OnDebugEvent;
                }
                catch (Exception e)
                {
                    DebugFileWriter(e.ToString(), this.GetType().ToString(), 0, 4);
                }
            }
        }

        public void UpdateDebugEvents(List<IDebugEvent> debugEvents)
        {
            foreach (IDebugEvent debugEvent in debugEvents)
            {
                debugEvent.OnDebugEvent += OnDebugEvent;
            }
        }

        public void SetIrcSettings(IrcSettings settings)
        {
            
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            LittleWeebSettings = settings;
        }

        private void OnDebugEvent(object sender, BaseDebugArgs args)
        {
            DebugFileWriter(args.DebugMessage, args.DebugSource, args.DebugSourceType, args.DebugType);
        }

        private async void CreateFile()
        {
            try
            {

#if __ANDROID__
                DebugPath = Path.Combine(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LittleWeeb"), "DebugLog");
#else
                DebugPath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LittleWeeb"), "DebugLog");
#endif
                if (!Directory.Exists(DebugPath))
                {
                    Directory.CreateDirectory(DebugPath);
                }
                if (!File.Exists(Path.Combine(DebugPath, DebugFileName)))
                {
                    WriteTrace("Debug file does not exist, creating file.");
                    using (var fileStream = File.Open(Path.Combine(DebugPath, DebugFileName), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            await streamWriter.WriteLineAsync("Starting Log AT: " + DateTime.UtcNow + Environment.NewLine);
                        }
                    }

                    WriteTrace("Debug file has been created.");
                }
                else
                {
                    WriteTrace("Debug file exists, reading content.");
                    using (var fileStream = File.Open(Path.Combine(DebugPath, DebugFileName), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            string readLine = "";
                            while ((readLine = streamReader.ReadLine()) != null)
                            {
                                currentLog += readLine;
                            }
                        }
                    }

                    WriteTrace("Read debug log with : " + currentLog.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Count().ToString());
                    DebugWriteAble = true;
                }
            }
            catch (Exception e)
            {
                DebugWriteAble = false;
                WriteTrace(e.ToString());
            }
        }

        private async void DebugFileWriter(string toWrite, string source, int sourceType, int debugType)
        {
           
            try
            {

                if (DebugWriteAble && LittleWeebSettings.DebugLevel.Contains(debugType) && LittleWeebSettings.DebugType.Contains(sourceType))
                {
                    string debugSourceType = "";

                    if (sourceType == 99)
                    {
                        debugSourceType = "UNDEFINED";
                    }
                    else
                    {
                        debugSourceType = DebugSourceTypes[sourceType];
                    }

                    string toWriteString = DebugTypes[debugType] + "|" + source + "|" + debugSourceType + "|" + toWrite + "|" + DateTime.UtcNow.ToShortTimeString();
                    if (currentLog.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length >= LittleWeebSettings.MaxDebugLogSize)
                    {

                        string[] currentLogArray = currentLog.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip((LittleWeebSettings.MaxDebugLogSize / 2)).ToArray();

                        string fullLog = "";

                        foreach (string line in currentLogArray)
                        {
                            fullLog += line + Environment.NewLine;
                        }

                        fullLog += toWriteString;

                        using (var fileStream = File.Open(Path.Combine(DebugPath, DebugFileName), FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            using (var streamWriter = new StreamWriter(fileStream))
                            {
                                currentLog = fullLog;
                                await streamWriter.WriteAsync(fullLog);
                            }
                        }
                    }
                    else
                    {
                        using (var fileStream = File.Open(Path.Combine(DebugPath, DebugFileName), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (var streamWriter = new StreamWriter(fileStream))
                            {
                                currentLog += toWriteString;
                                await streamWriter.WriteLineAsync(toWriteString);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteTrace(e.ToString());
            }
           
           
        }

        [Conditional("DEBUG")]
        private void WriteTrace(string toWrite)
        {
            Trace.WriteLine(toWrite);
        }
    }

}
