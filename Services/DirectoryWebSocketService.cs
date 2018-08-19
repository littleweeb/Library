using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Services
{
    public interface IDirectoryWebSocketService
    {
        void CreateDirectory(JObject directoryJson);
        void DeleteDirectory(JObject directoryJson);
        void GetDirectories(JObject directoryJson);
        void GetFreeSpace();
        void GetDrives();
    }
    public class DirectoryWebSocketService : IDirectoryWebSocketService, IDebugEvent, ISettingsInterface
    {
        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private readonly IDirectoryHandler DirectoryHandler;
        private readonly IWebSocketHandler WebSocketHandler;

        private LittleWeebSettings LittleWeebSettings;
        private IrcSettings IrcSettings;

        public DirectoryWebSocketService(IWebSocketHandler webSocketHandler, IDirectoryHandler directoryHandler)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "Constructor Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 0,
                DebugType = 0
            });

            WebSocketHandler = webSocketHandler;
            DirectoryHandler = directoryHandler;
        }

        public void CreateDirectory(JObject directoryJson)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "CreateDirectory Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = directoryJson.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

            try
            {

                string filePath = directoryJson.Value<string>("path");
                if (filePath.Contains("//"))
                {
                    filePath.Replace("//", "/");
                }

                if (filePath.Contains("\\\\"))
                {
                    filePath.Replace("\\\\", "\\");
                }
                string result = DirectoryHandler.CreateDirectory(filePath, "");
                WebSocketHandler.SendMessage(result);
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

                JsonError error = new JsonError()
                {
                    type = "create_directory_error",
                    errormessage = "Could not create directory.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public void DeleteDirectory(JObject directoryJson)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "DeleteDirectory Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = directoryJson.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

            try
            {
                string filePath = directoryJson.Value<string>("path");
                if (filePath.Contains("//"))
                {
                    filePath.Replace("//", "/");
                }

                if (filePath.Contains("\\\\"))
                {
                    filePath.Replace("\\\\", "\\");
                }
                string result = DirectoryHandler.DeleteDirectory(filePath);
                WebSocketHandler.SendMessage(result);
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

                JsonError error = new JsonError()
                {
                    type = "create_directory_error",
                    errormessage = "Could not create directory.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public void GetDrives()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetDrives Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            string result = DirectoryHandler.GetDrives();
            WebSocketHandler.SendMessage(result);
        }

        public void GetDirectories(JObject directoryJson)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetDirectories Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = directoryJson.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 1
            });

            try{

                string filePath = directoryJson.Value<string>("path");
                if (filePath.Contains("//"))
                {
                    filePath.Replace("//", "/");
                }

                if (filePath.Contains("\\\\"))
                {
                    filePath.Replace("\\\\", "\\");
                }
                string result = DirectoryHandler.GetDirectories(filePath);
                WebSocketHandler.SendMessage(result);
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

                JsonError error = new JsonError()
                {
                    type = "create_directory_error",
                    errormessage = "Could not create directory.",
                    errortype = "exception",
                    exception = e.ToString(),
                };

                WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public void GetFreeSpace()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "GetFreeSpace Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            string result = DirectoryHandler.GetFreeSpace(IrcSettings.fullfilepath);
            WebSocketHandler.SendMessage(result);
        }

        public void SetIrcSettings(IrcSettings settings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "SetIrcSettings Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = settings.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            IrcSettings = settings;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = "SetLittleWeebSettings Called.",
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugMessage = settings.ToString(),
                DebugSource = this.GetType().Name,
                DebugSourceType = 1,
                DebugType = 0
            });
            LittleWeebSettings = settings;
        }
    }
}
