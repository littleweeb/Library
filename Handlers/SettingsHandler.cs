using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LittleWeebLibrary.Handlers
{
    public interface ISettingsHandler
    {
        IrcSettings GetIrcSettings();
        LittleWeebSettings GetLittleWeebSettings();
        void WriteIrcSettings(IrcSettings ircSettings);
        void WriteLittleWeebSettings(LittleWeebSettings littleWeebSettings);
    }

    public class SettingsHandler : ISettingsHandler, IDebugEvent
    {
        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private string BasePath;
        private string SettingsPath;

        public SettingsHandler()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "Constructor called.",
                DebugSourceType = 0,
                DebugType = 0
            });

            string littleWeebSettingsName = "LittleWeebSettings.json";
            string ircSettingsName = "IrcSettings.json";

#if __ANDROID__
            BasePath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "LittleWeeb");
#else
            BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LittleWeeb");
#endif

            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            SettingsPath = Path.Combine(BasePath, "Settings");

            if (!Directory.Exists(SettingsPath))
            {
                Directory.CreateDirectory(SettingsPath);
            }

            if (!File.Exists(Path.Combine(SettingsPath, littleWeebSettingsName))){
                WriteLittleWeebSettings(new LittleWeebSettings()
                {
                    Local = true,
                    Port = 1515,
                    DebugLevel = new List<int>() { 0, 1, 2, 3, 4, 5 },
                    DebugType = new List<int>() { 0, 1, 2, 3, 4},
                    RandomUsernameLength = 6,
                    MaxDebugLogSize = 2000,
                    Version = "v0.4.0"
                });
            }
            if (!File.Exists(Path.Combine(SettingsPath, ircSettingsName)))
            {
                WriteIrcSettings(new IrcSettings());
            }
        }

        public void WriteIrcSettings(IrcSettings ircSettings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "WriteIrcSettings called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = ircSettings.ToString(),
                DebugSourceType = 1,
                DebugType = 1
            });

            try
            {
                string settingsName = "IrcSettings.json";
                string settingsJson = JsonConvert.SerializeObject(ircSettings);

                if (!File.Exists(Path.Combine(SettingsPath, settingsName)))
                {
                    using (var fileStream = File.Open(Path.Combine(SettingsPath, settingsName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(settingsJson);
                        }
                    }
                }
                else
                {
                    using (var fileStream = File.Open(Path.Combine(SettingsPath, settingsName), FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
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
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });
            }

        }

        public void WriteLittleWeebSettings(LittleWeebSettings littleWeebSettings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "WriteLittleWeebSettings called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = littleWeebSettings.ToString(),
                DebugSourceType = 1,
                DebugType = 1
            });



            try
            {
                string settingsName = "LittleWeebSettings.json";
                string settingsJson = JsonConvert.SerializeObject(littleWeebSettings);
                if (!File.Exists(Path.Combine(SettingsPath, settingsName)))
                {
                    using (var fileStream = File.Open(Path.Combine(SettingsPath, settingsName), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.Write(settingsJson);
                        }
                    }
                }
                else
                {
                    using (var fileStream = File.Open(Path.Combine(SettingsPath, settingsName), FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite))
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
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });
            }
        }

        public LittleWeebSettings GetLittleWeebSettings()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "GetLittleWeebSettings called.",
                DebugSourceType = 1,
                DebugType = 0
            });
            try
            {
                string settingsName = "LittleWeebSettings.json";
                if (File.Exists(Path.Combine(SettingsPath, settingsName)))
                {
                    string settingsJson = "";
                    using (var fileStream = File.Open(Path.Combine(SettingsPath, settingsName), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
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

                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name,
                        DebugMessage = "Returning read littleweebsettings: " + settingsJson,
                        DebugSourceType = 1,
                        DebugType = 2
                    });
                    return JsonConvert.DeserializeObject<LittleWeebSettings>(settingsJson);
                }
                else
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name,
                        DebugMessage = "Returning new littleweebsettings.",
                        DebugSourceType = 1,
                        DebugType = 2
                    });
                    return new LittleWeebSettings();
                }
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });

                return new LittleWeebSettings();
            }

        }

        public IrcSettings GetIrcSettings()
        {
            IrcSettings toReturn = new IrcSettings();

            try
            {
                string settingsName = "IrcSettings.json";
                if (File.Exists(Path.Combine(SettingsPath, settingsName)))
                {
                    string settingsJson = "";
                    using (var fileStream = File.Open(Path.Combine(SettingsPath, settingsName), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
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
                }
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });
            }

            return toReturn;
        }

    }
}
