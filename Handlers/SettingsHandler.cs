using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using PCLExt.FileStorage;

namespace LittleWeebLibrary.Handlers
{
    public interface ISettingsHandler
    {
        IrcSettings GetIrcSettings();
        LittleWeebSettings GetLittleWeebSettings();
        void WriteIrcSettings(IrcSettings ircSettings);
        void WriteLittleWeebSettings(LittleWeebSettings littleWeebSettings);
    }

    public class SettingsHandler : ISettingsHandler
    {
       

        private readonly IDebugHandler DebugHandler;

        private readonly string SettingsPath;

        public SettingsHandler(IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            DebugHandler = debugHandler;

            string littleWeebSettingsName = "LittleWeebSettings.json";
            string ircSettingsName = "IrcSettings.json";




            SettingsPath = PortablePath.Combine(UtilityMethods.BasePath(), "Settings");

            if (!Directory.Exists(SettingsPath))
            {
                Directory.CreateDirectory(SettingsPath);
            }

            if (!File.Exists(PortablePath.Combine(SettingsPath, littleWeebSettingsName))){
                WriteLittleWeebSettings(new LittleWeebSettings()
                {
                    Local = true,
                    Port = 1515,
                    DebugLevel = new List<int>() { 0, 1, 2, 3, 4, 5 },
                    DebugType = new List<int>() { 0, 1, 2, 3, 4},
                    RandomUsernameLength = 6,
                    MaxDebugLogSize = 2000
                });
            }
            if (!File.Exists(PortablePath.Combine(SettingsPath, ircSettingsName)))
            {
                WriteIrcSettings(new IrcSettings());
            }


        }

        public void WriteIrcSettings(IrcSettings ircSettings)
        {

            DebugHandler.TraceMessage("WriteIrcSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(ircSettings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                string settingsName = "IrcSettings.json";
                string settingsJson = JsonConvert.SerializeObject(ircSettings);

                if (!File.Exists(PortablePath.Combine(SettingsPath, settingsName)))
                {
                    using (var fileStream = File.Open(PortablePath.Combine(SettingsPath, settingsName), FileMode.OpenOrCreate, System.IO.FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(settingsJson);
                        }
                    }
                }
                else
                {
                    using (var fileStream = File.Open(PortablePath.Combine(SettingsPath, settingsName), FileMode.Truncate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(settingsJson);
                        }
                    }
                }
            }
            catch (Exception e)
            {

                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }

        }

        public void WriteLittleWeebSettings(LittleWeebSettings littleWeebSettings)
        {

            DebugHandler.TraceMessage("WriteLittleWeebSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(littleWeebSettings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);


            try
            {
                string settingsName = "LittleWeebSettings.json";
                string settingsJson = JsonConvert.SerializeObject(littleWeebSettings);
                if (!File.Exists(PortablePath.Combine(SettingsPath, settingsName)))
                {
                    using (var fileStream = File.Open(PortablePath.Combine(SettingsPath, settingsName), FileMode.OpenOrCreate, System.IO.FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(settingsJson);
                        }
                    }
                }
                else
                {
                    using (var fileStream = File.Open(PortablePath.Combine(SettingsPath, settingsName), FileMode.Truncate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(settingsJson);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
        }

        public LittleWeebSettings GetLittleWeebSettings()
        {

            DebugHandler.TraceMessage("GetLittleWeebSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            try
            {
                string settingsName = "LittleWeebSettings.json";
                if (File.Exists(PortablePath.Combine(SettingsPath, settingsName)))
                {
                    string settingsJson = "";
                    using (var fileStream = File.Open(PortablePath.Combine(SettingsPath, settingsName), FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            string readLine = "";
                            while ((readLine = streamReader.ReadLine()) != null)
                            {
                                settingsJson += readLine;
                            }
                        }

                    }

                    DebugHandler.TraceMessage("Returning read littleweebsettings: " + settingsJson, DebugSource.TASK, DebugType.INFO);
                    return JsonConvert.DeserializeObject<LittleWeebSettings>(settingsJson);
                }
                else
                {
                    DebugHandler.TraceMessage("Returning new littleweebsettings.", DebugSource.TASK, DebugType.INFO);
                    return new LittleWeebSettings();
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Returning new littleweebsettings.", DebugSource.TASK, DebugType.INFO);
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                return new LittleWeebSettings();
            }

        }

        public IrcSettings GetIrcSettings()
        {
            DebugHandler.TraceMessage("GetIrcSettings called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            IrcSettings toReturn = new IrcSettings();

            try
            {
                string settingsName = "IrcSettings.json";
                if (File.Exists(PortablePath.Combine(SettingsPath, settingsName)))
                {
                    string settingsJson = "";
                    using (var fileStream = File.Open(PortablePath.Combine(SettingsPath, settingsName), FileMode.Open, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        using (var streamReader = new StreamReader(fileStream))
                        {
                            string readLine = "";
                            while ((readLine = streamReader.ReadLine()) != null)
                            {
                                settingsJson += readLine;
                            }
                        }
                    }

                    toReturn = JsonConvert.DeserializeObject<IrcSettings>(settingsJson);

                    DebugHandler.TraceMessage("Returning read ircsettings: " + toReturn, DebugSource.TASK, DebugType.INFO);
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }

            return toReturn;
        }

    }
}
