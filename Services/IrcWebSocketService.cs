using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LittleWeebLibrary.Services
{
    public interface IIrcWebSocketService
    {
        void Connect(JObject ircJson = null);
        void Disconnect();
        void EnableSendMessage();
        void DisableSendMessage();
        void GetCurrentIrcSettings();
        void SendMessage(JObject ircMessageJson);
    }
    public class IrcWebSocketService : IIrcWebSocketService, IDebugEvent, ISettingsInterface
    {
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IIrcClientHandler IrcClientHandler;
        private readonly ISettingsHandler SettingsHandler;


        private LittleWeebSettings LittleWeebSettings;
        private IrcSettings IrcSettings;

        private bool SendMessageToWebSocketClient;
        private bool IsIrcConnected;

        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        public IrcWebSocketService(IWebSocketHandler webSocketHandler, IIrcClientHandler ircClientHandler, ISettingsHandler settingsHandler)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "IrcWebSocketService called.",
                DebugSourceType = 0,
                DebugType = 0
            });

            SendMessageToWebSocketClient = false;
            IsIrcConnected = false;
            SettingsHandler = settingsHandler;
            IrcClientHandler = ircClientHandler;
            WebSocketHandler = webSocketHandler;

            LittleWeebSettings = SettingsHandler.GetLittleWeebSettings();
            IrcSettings = SettingsHandler.GetIrcSettings();

            IrcClientHandler.OnIrcClientMessageEvent += OnIrcClientMessageEvent;
            IrcClientHandler.OnIrcClientConnectionStatusEvent += OnIrcClientConnectionStatusEvent;
        }

        public void Connect(JObject ircJson = null)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "Connect called.",
                DebugSourceType = 1,
                DebugType = 0
            });


            try
            {
                if (ircJson == null)
                {
                    string username = IrcSettings.UserName;

                    if (username == "")
                    {
                        username = UtilityMethods.GenerateUsername(LittleWeebSettings.RandomUsernameLength);
                    }
                    IrcSettings.UserName = username;
                }
                else
                {

                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name,
                        DebugMessage = ircJson.ToString(),
                        DebugSourceType = 1,
                        DebugType = 1
                    });

                    string username = ircJson.Value<string>("username");

                    if (username == "")
                    {
                        username = UtilityMethods.GenerateUsername(LittleWeebSettings.RandomUsernameLength);
                    }


                    IrcSettings.ServerAddress = ircJson.Value<string>("address");
                    IrcSettings.Channels = ircJson.Value<string>("channels");
                    IrcSettings.UserName = username;
                }

                IrcClientHandler.StartConnection(IrcSettings);

                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name,
                    DebugMessage = "Started irc connection using the following settings: " + IrcSettings.ToString(),
                    DebugSourceType = 1,
                    DebugType = 2
                });

                SettingsHandler.WriteIrcSettings(IrcSettings);

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
                    type = "irc_connect_error",
                    errormessage = "Could not start connection to irc server.",
                    errortype = "exception",
                    exception = e.ToString()
                };


                WebSocketHandler.SendMessage(error.ToJson());
            }

        }

        public void GetCurrentIrcSettings()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "GetCurrentIrcSettings called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            JsonIrcInfo ircInfo = new JsonIrcInfo()
            {
                connected = IsIrcConnected,
                channel = IrcSettings.Channels,
                server = IrcSettings.ServerAddress,
                user = IrcSettings.UserName,
                fullfilepath= IrcSettings.fullfilepath
            };

            WebSocketHandler.SendMessage(ircInfo.ToJson());
        }

        public void EnableSendMessage()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "EnableSendMessage called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            SendMessageToWebSocketClient = true;
        }

        public void DisableSendMessage()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "DisableSendMessage called.",
                DebugSourceType = 1,
                DebugType = 0
            });
            
            SendMessageToWebSocketClient = false;
        }

        public void Disconnect()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "Disconnect called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            try
            {
                IrcClientHandler.StopConnection();
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

        public void SendMessage(JObject ircMessageJson)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "SendMessage called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = ircMessageJson.ToString(),
                DebugSourceType = 1,
                DebugType = 1
            });

            try
            {
                string message = ircMessageJson.Value<string>("message");
                IrcClientHandler.SendMessage(message);
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
                    type = "irc_connect_error",
                    errormessage = "Could not send message to irc server.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                WebSocketHandler.SendMessage(error.ToJson());
            }

        }

        private void OnIrcClientMessageEvent(object sender, IrcClientMessageEventArgs args)
        {

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                DebugMessage = "OnIrcClientMessageEvent called.",
                DebugSourceType = 2,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                DebugMessage = args.ToString(),
                DebugSourceType = 2,
                DebugType = 1
            });

            try
            {
                if (SendMessageToWebSocketClient)
                {
                    JsonIrcChatMessage messageToSend = new JsonIrcChatMessage()
                    {
                        channel = args.Channel,
                        user = args.User,
                        message = args.Message
                    };

                    WebSocketHandler.SendMessage(messageToSend.ToJson());
                }
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });

                if (SendMessageToWebSocketClient)
                {
                    JsonError error = new JsonError()
                    {
                        type = "irc_status_error",
                        errormessage = "Error on sending irc message to client.",
                        errortype = "exception",
                        exception = e.ToString()
                    };


                    WebSocketHandler.SendMessage(error.ToJson());
                }
            }
        }

        private void OnIrcClientConnectionStatusEvent(object sender, IrcClientConnectionStatusArgs args)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                DebugMessage = "OnIrcClientConnectionStatusEvent called.",
                DebugSourceType = 2,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                DebugMessage = args.ToString(),
                DebugSourceType = 2,
                DebugType = 1
            });


            IsIrcConnected = args.Connected;

            try
            {
                JsonIrcInfo update = new JsonIrcInfo()
                {
                    connected = args.Connected,
                    channel = args.CurrentIrcSettings.Channels,
                    server = args.CurrentIrcSettings.ServerAddress,
                    user = args.CurrentIrcSettings.UserName,
                    fullfilepath= args.CurrentIrcSettings.fullfilepath
                };

                WebSocketHandler.SendMessage(update.ToJson());
            }
            catch (Exception e)
            {
                OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                {
                    DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                    DebugMessage = e.ToString(),
                    DebugSourceType = 1,
                    DebugType = 4
                });

                JsonError error = new JsonError()
                {
                    type = "irc_status_error",
                    errormessage = "Error on sending irc status update to client.",
                    errortype = "exception",
                    exception = e.ToString()
                };


                WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public void SetIrcSettings(IrcSettings settings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "SetIrcSettings called.",
                DebugSourceType = 1,
                DebugType = 0
            });
            IrcSettings = settings;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "SetLittleWeebSettings called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            LittleWeebSettings = settings;
        }
    }
}
