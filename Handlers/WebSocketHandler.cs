using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using SimpleWebSocketServerLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{

    public interface IWebSocketHandler
    {
        event EventHandler<WebSocketEventArgs> OnWebSocketEvent;
        Task SendMessage(string message);
        void StartServer();
        Task StopServer();
    }

    public class WebSocketHandler : IWebSocketHandler, IDebugEvent, ISettingsInterface
    {
        public event EventHandler<WebSocketEventArgs> OnWebSocketEvent;
        public event EventHandler<BaseDebugArgs> OnDebugEvent;

        private LittleWeebSettings LittleWeebSettings;

        private SimpleWebSocketServer Server;

        private List<string> ClientIds;

        public WebSocketHandler(ISettingsHandler settingsHandler)
        {
            LittleWeebSettings = settingsHandler.GetLittleWeebSettings();
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = settings.ToString(),
                DebugSourceType = 1,
                DebugType = 1
            });
            LittleWeebSettings = settings;

        }

        public void StartServer()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = " StartWebSocketServer Called.",
                DebugSourceType = 1,
                DebugType = 0
            });

            try
            {
                ClientIds = new List<string>();
                Server = new SimpleWebSocketServer(new SimpleWebSocketServerSettings()
                {
                    port = LittleWeebSettings.Port
                });
                Server.WebsocketServerEvent += OnWebSocketServerEvent;
                Server.StartServer();

                Debug.WriteLine("WebSocket server started.");
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

                Debug.WriteLine(e.ToString());
            }
        }

        public async Task SendMessage(string message)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "SendMessage called.",
                DebugSourceType = 3,
                DebugType = 0
            });

            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = message,
                DebugSourceType = 3,
                DebugType = 1
            });

            try
            {
                if (await Server.SendTextMessageAsync(message))
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name,
                        DebugMessage = "Succesfully send message to WebSocket Client.",
                        DebugSourceType = 3,
                        DebugType = 2
                    });
                }
                else
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name,
                        DebugMessage = "Could not send message to WebSocket Client.",
                        DebugSourceType = 3,
                        DebugType = 3
                    });
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

        public async Task StopServer()
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name,
                DebugMessage = "StopServer called.",
                DebugSourceType = 3,
                DebugType = 0
            });
            try
            {
                if (await Server.StopAllAsync())
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name,
                        DebugMessage = "Succesfully stopped WebSocket connection with client.",
                        DebugSourceType = 3,
                        DebugType = 2
                    });
                }
                else
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name,
                        DebugMessage = "Could not stop WebSocket connection with client.",
                        DebugSourceType = 3,
                        DebugType = 3
                    });
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

        private async void OnWebSocketServerEvent(object sender, WebSocketEventArg args)
        {
            OnDebugEvent?.Invoke(this, new BaseDebugArgs()
            {
                DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                DebugMessage = "OnWebSocketServerEvent called.",
                DebugSourceType = 2,
                DebugType = 0
            });

            try
            {

                if (args.isOpen)
                {
                    if (!ClientIds.Contains(args.clientId))
                    {

                        OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                        {
                            DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                            DebugMessage = "Client with id " + args.clientId + " from " + args.clientBaseUrl + " connected!",
                            DebugSourceType = 2,
                            DebugType = 2
                        });

                        ClientIds.Add(args.clientId);
                        await SendMessage(new JsonWelcome()
                        {
                            local = LittleWeebSettings.Local
                        }.ToJson());
                    }
                }

                if (args.isClosed)
                {
                    if (ClientIds.Contains(args.clientId))
                    {
                        OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                        {
                            DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                            DebugMessage = "Client with id " + args.clientId + " from " + args.clientBaseUrl + " disconnected!",
                            DebugSourceType = 2,
                            DebugType = 2
                        });
                        ClientIds.Remove(args.clientId);
                    }
                }

                if (args.data != null && args.isText)
                {

                    string received = Encoding.ASCII.GetString(args.data);
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                        DebugMessage = received,
                        DebugSourceType = 2,
                        DebugType = 1
                    });

                    OnWebSocketEvent?.Invoke(this, new WebSocketEventArgs() { Message = received });

                    await SendMessage(new JsonReceivedResponse()
                    {
                        received = received
                    }.ToJson());
                }

                if (args.isPing)
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                        DebugMessage = "IsPing: " + args.isPing.ToString(),
                        DebugSourceType = 2,
                        DebugType = 1
                    });

                    await Server.SendPongMessageAsync();
                }

                if (args.isPong)
                {
                    OnDebugEvent?.Invoke(this, new BaseDebugArgs()
                    {
                        DebugSource = this.GetType().Name + " via " + sender.GetType().Name,
                        DebugMessage = "IsPong: " + args.isPong.ToString(),
                        DebugSourceType = 2,
                        DebugType = 1
                    });
                    await Server.SendPingMessageAsync();
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
            }

        }

        public void SetIrcSettings(IrcSettings settings)
        {
        }

    }
}
