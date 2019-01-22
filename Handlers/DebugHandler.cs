using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{

    public enum DebugSource { CONSTRUCTOR, METHOD, EVENT, TASK, EXTERNAL, UNDEFINED };
    public enum DebugType { NONE, ENTRY_EXIT, PARAMETERS, INFO, WARNING, ERROR };

    public interface IDebugHandler
    {
        void StopDebugger();
        void SetSettings(ISettingsHandler settingsHandler);
        void TraceMessage(string message, DebugSource sourcetype, DebugType debugtype,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0);
    }

    public class DebugHandler : IDebugHandler
    {
        private string currentLog;

        private bool DebugWriteAble = false;
        private readonly string DebugFileName = "littleweeb_debug_log.log";
        private string DebugPath = "";
        private readonly string[] DebugTypes = new string[] { "NOT DEFINED", "ENTRY/EXIT", "PARAMETERS", "INFO", "WARNING", "ERROR" };
        private readonly string[] DebugSourceTypes = new string[] { "CONSTRUCTOR", "METHOD", "EVENT", "TASK", "EXTERNAL(LIBRARY)", "NOT DEFINED" };
        private readonly ConcurrentBag<string[]> MessageQueue;
        private LittleWeebSettings LittleWeebSettings;
        private bool stop = false;

        private ISettingsHandler SettingsHandler;

        public DebugHandler()
        {

            LittleWeebSettings = new LittleWeebSettings()
            {
                Local = true,
                Port = 1515,
                DebugLevel = new List<int>() { 0, 1, 2, 3, 4, 5 },
                DebugType = new List<int>() { 0, 1, 2, 3, 4 },
                RandomUsernameLength = 6,
                MaxDebugLogSize = 2000
            };

            MessageQueue = new ConcurrentBag<string[]>();

            foreach (int level in LittleWeebSettings.DebugLevel)
            {
                WriteTrace("DEBUG LEVEL: " + level);
            }

            currentLog = "";        

            CreateFile();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            TraceWriter();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            WriteTrace("Succesfully initiated debug handler!");

        }       

        public void SetSettings(ISettingsHandler settingsHandler)
        {
            LittleWeebSettings = settingsHandler.GetLittleWeebSettings();
            SettingsHandler = settingsHandler;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private async Task TraceWriter()
        {
            while (!stop)
            {

                try
                {
                    string[] debugMessage;
                    bool succes = MessageQueue.TryTake(out debugMessage);
                    if (succes)
                    {
                        DebugFileWriter(debugMessage);
                    }

                }
                catch (Exception e)
                {
                    WriteTrace(e.ToString());
                }

                if (MessageQueue.Count != 0)
                {
                    await Task.Delay(10);
                }
                else
                {
                    await Task.Delay(500);
                }
            }
        }

        public void TraceMessage(string message, DebugSource sourcetype, DebugType debugtype,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {

            string[] debugMessage = new string[] { message, memberName, sourceFilePath, sourceLineNumber.ToString(), DebugSourceTypes[(int)sourcetype], DebugTypes[(int)debugtype] };

            WriteTrace("TRACEMESSAGE|" + debugMessage[4] + "|[" + debugMessage[1] + ", " + debugMessage[2] + "," + debugMessage[3] + "]|" + debugMessage[5] + "|" + debugMessage[0] + "|");
            MessageQueue.Add(debugMessage);
            
        }

        private async void CreateFile()
        {
            try
            {

#if __ANDROID__
                DebugPath = Path.Combine(Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LittleWeeb"), "DebugLog");
#else
                DebugPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LittleWeeb"), "DebugLog");
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

        private async void DebugFileWriter(string[] message)
        {           
            try
            {
                if (DebugWriteAble && LittleWeebSettings.DebugLevel.Contains(Array.IndexOf<string>(DebugTypes, message[4])) && LittleWeebSettings.DebugType.Contains(Array.IndexOf<string>(DebugSourceTypes, message[5])))
                {
                    string debugSourceType = "";

                    if (message[5] == "NOT DEFINED")
                    {
                        debugSourceType = "NOT DEFINED";
                    }
                    else
                    {
                        debugSourceType = message[5];
                    }

                    string toWriteString = "|" + message[4] + "|[" + message[1] + ", " + message[2] + "," + message[3] + "]|" + debugSourceType + "|" + message[0] + "|" ;

                    using (var fileStream = File.Open(Path.Combine(DebugPath, DebugFileName), FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            currentLog = streamReader.ReadToEnd();
                        }

                        if (currentLog.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length > LittleWeebSettings.MaxDebugLogSize)
                        {
                            currentLog = currentLog.Skip(toWriteString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length) + Environment.NewLine;
                        }

                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            await streamWriter.WriteAsync(currentLog);
                        }
                    }
                  
                }
            }
            catch (Exception e)
            {
                WriteTrace(e.ToString());
            }        
        }

        private void WriteTrace(string toWrite)
        {
            Console.WriteLine(toWrite);
        }

        public void StopDebugger()
        {
            stop = true;
        }
    }

}
