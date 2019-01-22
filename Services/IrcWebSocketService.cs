using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Services
{
    public interface IIrcWebSocketService
    {
        Task Connect(JObject ircJson = null);
        Task Disconnect();
        Task EnableSendMessage();
        Task DisableSendMessage();
        Task GetCurrentIrcSettings();
        Task SendMessage(JObject ircMessageJson);
    }
    public class IrcWebSocketService : IIrcWebSocketService, ISettingsInterface
    {
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IIrcClientHandler IrcClientHandler;
        private readonly ISettingsHandler SettingsHandler;
        private readonly IDebugHandler DebugHandler;


        private LittleWeebSettings LittleWeebSettings;
        private IrcSettings IrcSettings;

        private bool SendMessageToWebSocketClient;
        private bool IsIrcConnected;

       

        public IrcWebSocketService(IWebSocketHandler webSocketHandler, IIrcClientHandler ircClientHandler, ISettingsHandler settingsHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("IrcWebSocketService Constructor called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            SendMessageToWebSocketClient = false;
            IsIrcConnected = false;
            SettingsHandler = settingsHandler;
            IrcClientHandler = ircClientHandler;
            WebSocketHandler = webSocketHandler;
            DebugHandler = debugHandler;

            LittleWeebSettings = SettingsHandler.GetLittleWeebSettings();
            IrcSettings = SettingsHandler.GetIrcSettings();

            IrcClientHandler.OnIrcClientMessageEvent += OnIrcClientMessageEvent;
            IrcClientHandler.OnIrcClientConnectionStatusEvent += OnIrcClientConnectionStatusEvent;
        }

        public async Task Connect(JObject ircJson = null)
        {
            DebugHandler.TraceMessage("Connect called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            bool succes = false;
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

                    DebugHandler.TraceMessage(ircJson.ToString(), DebugSource.TASK, DebugType.INFO);

                    string username = ircJson.Value<string>("username");

                    if (username == "")
                    {
                        username = UtilityMethods.GenerateUsername(LittleWeebSettings.RandomUsernameLength);
                    }


                    IrcSettings.ServerAddress = ircJson.Value<string>("address");
                    IrcSettings.Channels = ircJson.Value<string>("channels");
                    IrcSettings.UserName = username;
                }

                succes = IrcClientHandler.StartConnection(IrcSettings);

                if (succes)
                {
                    JsonIrcInfo ircInfo = new JsonIrcInfo()
                    {
                        connected = succes,
                        channel = IrcSettings.Channels,
                        server = IrcSettings.ServerAddress,
                        user = IrcSettings.UserName,
                        fullfilepath = IrcSettings.fullfilepath
                    };

                    await WebSocketHandler.SendMessage(ircInfo.ToJson());
                }
                else
                {
                    JsonIrcInfo ircInfo = new JsonIrcInfo()
                    {
                        connected = succes,
                        channel = IrcSettings.Channels,
                        server = IrcSettings.ServerAddress,
                        user = IrcSettings.UserName,
                        fullfilepath = IrcSettings.fullfilepath
                    };

                    await WebSocketHandler.SendMessage(ircInfo.ToJson());
                }

                DebugHandler.TraceMessage("Started irc connection using the following settings: " + IrcSettings.ToString(), DebugSource.TASK, DebugType.INFO);

                SettingsHandler.WriteIrcSettings(IrcSettings);

            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                JsonIrcInfo ircInfo = new JsonIrcInfo()
                {
                    connected = succes,
                    channel = IrcSettings.Channels,
                    server = IrcSettings.ServerAddress,
                    user = IrcSettings.UserName,
                    fullfilepath = IrcSettings.fullfilepath
                };

                await WebSocketHandler.SendMessage(ircInfo.ToJson());

                JsonError error = new JsonError()
                {
                    type = "irc_connect_error",
                    errormessage = "Could not start connection to irc server.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }

            IsIrcConnected = succes;
        }

        public async Task GetCurrentIrcSettings()
        {
            DebugHandler.TraceMessage("GetCurrentIrcSettings called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            JsonIrcInfo ircInfo = new JsonIrcInfo()
            {
                connected = IsIrcConnected,
                channel = IrcSettings.Channels,
                server = IrcSettings.ServerAddress,
                user = IrcSettings.UserName,
                fullfilepath= IrcSettings.fullfilepath
            };

            await WebSocketHandler.SendMessage(ircInfo.ToJson());
        }

        public async Task EnableSendMessage()
        {
            DebugHandler.TraceMessage("EnableSendMessage called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            SendMessageToWebSocketClient = true;

            JsonSuccess jsonSuccess = new JsonSuccess()
            {
                message = "Succesfully enabled sending IRC messages to client."
            };

            await WebSocketHandler.SendMessage(jsonSuccess.ToJson());
        }

        public async Task DisableSendMessage()
        {
            DebugHandler.TraceMessage("DisableSendMessage called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            SendMessageToWebSocketClient = false;

            JsonSuccess jsonSuccess = new JsonSuccess()
            {
                message = "Succesfully disabled sending IRC messages to client."
            };

            await WebSocketHandler.SendMessage(jsonSuccess.ToJson());
        }

        public async Task Disconnect()
        {
            DebugHandler.TraceMessage("Disconnect called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            bool succes = false;
            try
            {
                succes = IrcClientHandler.StopConnection();
                JsonIrcInfo ircInfo = new JsonIrcInfo()
                {
                    connected = !succes,
                    channel = IrcSettings.Channels,
                    server = IrcSettings.ServerAddress,
                    user = IrcSettings.UserName,
                    fullfilepath = IrcSettings.fullfilepath
                };

                await WebSocketHandler.SendMessage(ircInfo.ToJson());
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);

                succes = IrcClientHandler.StopConnection();
                JsonIrcInfo ircInfo = new JsonIrcInfo()
                {
                    connected = !succes,
                    channel = IrcSettings.Channels,
                    server = IrcSettings.ServerAddress,
                    user = IrcSettings.UserName,
                    fullfilepath = IrcSettings.fullfilepath
                };

                await WebSocketHandler.SendMessage(ircInfo.ToJson());

                JsonError error = new JsonError()
                {
                    type = "irc_disconnect_error",
                    errormessage = "Could not stop connection to irc server.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());

            }
        }        

        public async Task SendMessage(JObject ircMessageJson)
        {
            DebugHandler.TraceMessage("SendMessage called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(ircMessageJson.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                string message = ircMessageJson.Value<string>("message");
                IrcClientHandler.SendMessage(message);
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
                JsonError error = new JsonError()
                {
                    type = "irc_connect_error",
                    errormessage = "Could not send message to irc server.",
                    errortype = "exception",
                    exception = e.ToString()
                };

                await WebSocketHandler.SendMessage(error.ToJson());
            }

        }

        private async void OnIrcClientMessageEvent(object sender, IrcClientMessageEventArgs args)
        {

            DebugHandler.TraceMessage("OnIrcClientMessageEvent called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

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

                    await WebSocketHandler.SendMessage(messageToSend.ToJson());
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
                if (SendMessageToWebSocketClient)
                {
                    JsonError error = new JsonError()
                    {
                        type = "irc_status_error",
                        errormessage = "Error on sending irc message to client.",
                        errortype = "exception",
                        exception = e.ToString()
                    };


                    await WebSocketHandler.SendMessage(error.ToJson());
                }
            }
        }

        private async void OnIrcClientConnectionStatusEvent(object sender, IrcClientConnectionStatusArgs args)
        {
            DebugHandler.TraceMessage("OnIrcClientConnectionStatusEvent called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.ToString(), DebugSource.TASK, DebugType.PARAMETERS);


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

                await WebSocketHandler.SendMessage(update.ToJson());
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
                JsonError error = new JsonError()
                {
                    type = "irc_status_error",
                    errormessage = "Error on sending irc status update to client.",
                    errortype = "exception",
                    exception = e.ToString()
                };
                await WebSocketHandler.SendMessage(error.ToJson());
            }
        }

        public void SetIrcSettings(IrcSettings settings)
        {
            DebugHandler.TraceMessage("SetIrcSettings called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            IrcSettings = settings;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            DebugHandler.TraceMessage("SetLittleWeebSettings called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            LittleWeebSettings = settings;
        }
    }
}
