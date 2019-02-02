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

    public class WebSocketHandler : IWebSocketHandler, ISettingsInterface
    {
        public event EventHandler<WebSocketEventArgs> OnWebSocketEvent;
       

        private readonly IDebugHandler DebugHandler;

        private LittleWeebSettings LittleWeebSettings;

        private SimpleWebSocketServer Server;

        private List<string> ClientIds;

        public WebSocketHandler(ISettingsHandler settingsHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            LittleWeebSettings = settingsHandler.GetLittleWeebSettings();
            DebugHandler = debugHandler;
        }

        public void SetLittleWeebSettings(LittleWeebSettings settings)
        {
            DebugHandler.TraceMessage("SetLittleWeebSettings Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(settings.ToString(), DebugSource.TASK, DebugType.PARAMETERS);
            LittleWeebSettings = settings;
        }

        public void StartServer()
        {
            DebugHandler.TraceMessage("StartServer Called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            try
            {
                ClientIds = new List<string>();
                Server = new SimpleWebSocketServer(new SimpleWebSocketServerSettings()
                {
                    port = LittleWeebSettings.Port,
                    bufferSize = 65535
                });
                Server.WebsocketServerEvent += OnWebSocketServerEvent;
                Server.StartServer();

                DebugHandler.TraceMessage("Web Socket Server Started!", DebugSource.TASK, DebugType.INFO);
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
            }
        }

        public async Task SendMessage(string message)
        {
            DebugHandler.TraceMessage("SendMessage Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage(message, DebugSource.TASK, DebugType.PARAMETERS);

            try
            {

                if (await Server.SendTextMessageAsync(message))
                {

                    DebugHandler.TraceMessage("Succesfully send message to WebSocket Client.", DebugSource.TASK, DebugType.INFO);
                }
                else
                {

                    DebugHandler.TraceMessage("Could not send message to WebSocket Client.", DebugSource.TASK, DebugType.WARNING);
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
        }

        public async Task StopServer()
        {
            DebugHandler.TraceMessage("StopServer Called", DebugSource.TASK, DebugType.ENTRY_EXIT);
            try
            {
                if (await Server.StopAllAsync())
                {
                    DebugHandler.TraceMessage("Succesfully closed websocket connection to client(s), stopped websocket server.", DebugSource.TASK, DebugType.INFO);
                }
                else
                {
                    DebugHandler.TraceMessage("Could not stop WebSocket connection with client, websocket server still running.", DebugSource.TASK, DebugType.WARNING);
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }
        }

        private async void OnWebSocketServerEvent(object sender, WebSocketEventArg args)
        {

            DebugHandler.TraceMessage("OnWebSocketServerEvent Called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            try
            {

                if (args.isOpen)
                {
                    if (!ClientIds.Contains(args.clientId))
                    {

                        DebugHandler.TraceMessage("Client with id " + args.clientId + " from " + args.clientBaseUrl + " connected!", DebugSource.TASK, DebugType.INFO);

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
                        DebugHandler.TraceMessage("Client with id " + args.clientId + " from " + args.clientBaseUrl + " disconnected!", DebugSource.TASK, DebugType.INFO);
                        ClientIds.Remove(args.clientId);
                    }
                }

                if (args.data != null && args.isText)
                {

                    string received = Encoding.ASCII.GetString(args.data);


                    DebugHandler.TraceMessage(received, DebugSource.TASK, DebugType.INFO);

                    OnWebSocketEvent?.Invoke(this, new WebSocketEventArgs() { Message = received });

                    await SendMessage(new JsonReceivedResponse()
                    {
                        received = received
                    }.ToJson());
                }

                if (args.isPing)
                {
                    DebugHandler.TraceMessage("IsPing: " + args.isPing.ToString(), DebugSource.TASK, DebugType.INFO);
                    await Server.SendPongMessageAsync();
                }

                if (args.isPong)
                {
                    DebugHandler.TraceMessage("IsPong: " + args.isPing.ToString(), DebugSource.TASK, DebugType.INFO);
                    await Server.SendPingMessageAsync();
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.WARNING);
            }

        }

        public void SetIrcSettings(IrcSettings settings)
        {
        }

    }
}
