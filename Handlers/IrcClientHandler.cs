using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.Settings;
using System;

using SimpleIRCLib;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.GlobalInterfaces;
using System.IO;
using System.Threading;

namespace LittleWeebLibrary.Handlers
{

    public interface IIrcClientHandler
    {
        event EventHandler<IrcClientMessageEventArgs> OnIrcClientMessageEvent;
        event EventHandler<IrcClientDownloadEventArgs> OnIrcClientDownloadEvent;
        event EventHandler<IrcClientConnectionStatusArgs> OnIrcClientConnectionStatusEvent;
        void SendMessage(string message);
        void StartDownload(JsonDownloadInfo download);
        bool StartConnection(IrcSettings settings);
        bool StopConnection();
        bool StopDownload();
        void Setfullfilepath(string path);
        bool IsDownloading();
        bool IsConnected();
        IrcSettings CurrentSettings();
    }

    public class IrcClientHandler : IIrcClientHandler, ISettingsInterface
    {


        public event EventHandler<IrcClientMessageEventArgs> OnIrcClientMessageEvent;
        public event EventHandler<IrcClientDownloadEventArgs> OnIrcClientDownloadEvent;
        public event EventHandler<IrcClientConnectionStatusArgs> OnIrcClientConnectionStatusEvent;
       

        private readonly ISettingsHandler SettingsHandler;
        private readonly IDebugHandler DebugHandler;

        private SimpleIRC IrcClient;
        private IrcSettings IrcSettings;
        private LittleWeebSettings LittleWeebSettings;

        private bool IsConnectedBool = false;

        public IrcClientHandler(ISettingsHandler settingsHandler, IDebugHandler debugHandler)
        {

            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            SettingsHandler = settingsHandler;
            DebugHandler = debugHandler;

            IrcSettings = SettingsHandler.GetIrcSettings();
            LittleWeebSettings = SettingsHandler.GetLittleWeebSettings();

            IrcClient = new SimpleIRC();
            IrcClient.SetCustomDownloadDir( IrcSettings.fullfilepath);
            IrcClient.IrcClient.OnUserListReceived += OnUserListUpdate;
            IrcClient.IrcClient.OnMessageReceived += OnMessage;
            IrcClient.IrcClient.OnDebugMessage += OnMessageDebug;
            IrcClient.DccClient.OnDccEvent += OnDownloadUpdate;
            IrcClient.DccClient.OnDccDebugMessage += OnDownloadUpdateDebug;            

        }

        public void SetIrcSettings(IrcSettings settings)
        {
            DebugHandler.TraceMessage("SetIrcSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            IrcSettings = settings;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            DebugHandler.TraceMessage("SetLittleWeebSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            LittleWeebSettings = settings;
        }

        public void Setfullfilepath(string path)
        {
            DebugHandler.TraceMessage("Setfullfilepath Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(path, DebugSource.TASK, DebugType.PARAMETERS);
            IrcClient.SetCustomDownloadDir(path);
            IrcSettings.fullfilepath= path;
        }

        public void SendMessage(string message)
        {
            DebugHandler.TraceMessage("SendMessage Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(message, DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                if (IrcClient != null)
                {
                    if (IrcClient.IsClientRunning())
                    {
                        IrcClient.SendMessageToAll(message);
                    }
                    else
                    {
                       DebugHandler.TraceMessage("Irc client is not connected!", DebugSource.TASK, DebugType.WARNING);
                    }
                }
                else
                {
                    DebugHandler.TraceMessage("Irc client is not set!", DebugSource.TASK, DebugType.WARNING);
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
        }

        public void StartDownload(JsonDownloadInfo download)
        {
            DebugHandler.TraceMessage("StartDownload Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(download.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            try
            {
                string xdccMessage = "/msg " + download.bot + " xdcc send #" + download.pack;
                IrcClient.SetCustomDownloadDir(Path.Combine(IrcSettings.fullfilepath, download.fullfilepath));
                SendMessage(xdccMessage);
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
        }

        public bool StartConnection(IrcSettings settings = null)
        {
            DebugHandler.TraceMessage("StartConnection Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            try
            {
                if (settings != null)
                {

                    DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
                    IrcSettings = settings;
                }
                IrcClient.SetupIrc(IrcSettings.ServerAddress, IrcSettings.UserName, IrcSettings.Channels, IrcSettings.Port, "", 3000, IrcSettings.Secure);

                Thread.Sleep(500);

                if (!IrcClient.StartClient())
                {
                    OnIrcClientConnectionStatusEvent?.Invoke(this, new IrcClientConnectionStatusArgs()
                    {
                        Connected = false,
                        ChannelsAndUsers = null
                    });

                    DebugHandler.TraceMessage("Irc client is could not connect!", DebugSource.TASK, DebugType.WARNING);
                    return false;
                }
                else
                {
                    DebugHandler.TraceMessage("Irc client is succesfully connected!", DebugSource.TASK, DebugType.INFO);
                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
                return false;
            }
        }

        public bool StopConnection()
        {

            DebugHandler.TraceMessage("StopConnection Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            try
            {
                if (IrcClient.IsClientRunning())
                {
                    if (IrcClient.StopClient())
                    {
                        if (IrcClient.StopXDCCDownload())
                        {

                            DebugHandler.TraceMessage("Succesfully stopped download before stopping IRC Client!", DebugSource.TASK, DebugType.INFO);
                        }

                        OnIrcClientConnectionStatusEvent?.Invoke(this, new IrcClientConnectionStatusArgs()
                        {
                            Connected = false,
                            CurrentIrcSettings = IrcSettings
                        });


                        DebugHandler.TraceMessage("Succesfully stopped IRC Client!", DebugSource.TASK, DebugType.INFO);

                        return true;
                    }
                    else
                    {
                        DebugHandler.TraceMessage("Could not stop connection with IRC server!", DebugSource.TASK, DebugType.WARNING);
                        return false;
                    }
                }
                else
                {
                    DebugHandler.TraceMessage("Irc client is is not connected!", DebugSource.TASK, DebugType.WARNING);
                    return true;
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
                return false;
            }

        }

        public bool StopDownload()
        {
            DebugHandler.TraceMessage("StopDownload Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            if (!IrcClient.StopXDCCDownload())
            {
                DebugHandler.TraceMessage("Could not stop download!", DebugSource.TASK, DebugType.WARNING);
                return false;
            }
            else
            {
                DebugHandler.TraceMessage("Succesfully stopped download!", DebugSource.TASK, DebugType.INFO);
                return true;
            }
        }

        public bool IsDownloading()
        {
            DebugHandler.TraceMessage("IsDownloading Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            return IrcClient.CheckIfDownload();
        }

        public bool IsConnected()
        {
            DebugHandler.TraceMessage("IsConnected Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            try
            {
                if (IrcClient.IsClientRunning())
                {
                    IsConnectedBool = true;
                    return true;
                }
                else
                {
                    IsConnectedBool = false;
                    return false;
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
                IsConnectedBool = false;
                return false;
            }
        }

        public IrcSettings CurrentSettings()
        {
            DebugHandler.TraceMessage("CurrentSettings Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            return IrcSettings;
        }

        private void OnMessage(object sender, IrcReceivedEventArgs args)
        {
            DebugHandler.TraceMessage("OnMessage Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            IrcClientMessageEventArgs eventArgs = new IrcClientMessageEventArgs()
            {
                Channel = args.Channel,
                User = args.User,
                Message = args.Message
            };

            DebugHandler.TraceMessage(eventArgs.ToString(), DebugSource.TASK, DebugType.ENTRY_EXIT);

            OnIrcClientMessageEvent?.Invoke(this, eventArgs);
        }

        private void OnMessageDebug(object sender, IrcDebugMessageEventArgs args)
        {
            DebugHandler.TraceMessage("OnMessageDebug Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.Type + " || " + args.Message, DebugSource.TASK, DebugType.INFO);
        }

        private void OnUserListUpdate(object sender, IrcUserListReceivedEventArgs args)
        {

            DebugHandler.TraceMessage("OnUserListUpdate Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            try
            {
                if (args.UsersPerChannel.Count > 0)
                {

                    IrcClientConnectionStatusArgs eventArgs = new IrcClientConnectionStatusArgs()
                    {
                        ChannelsAndUsers = args.UsersPerChannel,
                        Connected = true,
                        CurrentIrcSettings = IrcSettings
                    };

                    DebugHandler.TraceMessage(eventArgs.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
                   
                    if (!IsConnectedBool)
                    {
                        OnIrcClientConnectionStatusEvent?.Invoke(this, eventArgs);
                    }
                    IsConnectedBool = true;
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
           
        }

        private void OnDownloadUpdate(object sender, DCCEventArgs args)
        {
            DebugHandler.TraceMessage("OnDownloadUpdate Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            IrcClientDownloadEventArgs eventArgs = new IrcClientDownloadEventArgs()
            {
                FileName = args.FileName,
                FileLocation = args.FilePath,
                DownloadSpeed = args.KBytesPerSecond.ToString(),
                FileSize = (args.FileSize / 1048576),
                DownloadProgress = args.Progress,
                DownloadStatus = args.Status
            };

            DebugHandler.TraceMessage(eventArgs.ToString(), DebugSource.TASK, DebugType.PARAMETERS);

            OnIrcClientDownloadEvent?.Invoke(this, eventArgs);
        }

        private void OnDownloadUpdateDebug(object sender, DCCDebugMessageArgs args)
        {

            DebugHandler.TraceMessage("OnDownloadUpdateDebug Called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(args.Type + " || " + args.Message, DebugSource.TASK, DebugType.INFO);         
        }
    }
}
