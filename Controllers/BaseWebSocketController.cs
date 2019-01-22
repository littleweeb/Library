using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.EventArguments;
using System;
using System.Collections.Generic;
using LittleWeebLibrary.Handlers;

namespace LittleWeebLibrary.Controllers
{
    public interface IBaseWebSocketController
    {
        void SetSubControllers(List<ISubWebSocketController> subControllers);
    }
    public class BaseWebSocketController : IBaseWebSocketController
    {
       
        private readonly IWebSocketHandler WebSocketHandler;
        private readonly IDebugHandler DebugHandler;
        private List<ISubWebSocketController> SubControllers;

        public BaseWebSocketController(IWebSocketHandler webSocketHandler, IDebugHandler debugHandler)
        {

            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;

            WebSocketHandler = webSocketHandler;
            WebSocketHandler.OnWebSocketEvent += OnWebSocketEvent;


        }

        public void SetSubControllers(List<ISubWebSocketController> subControllers)
        {
            DebugHandler.TraceMessage("SetSubControllers called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            SubControllers = subControllers;
        }

        private void OnWebSocketEvent(object sender, WebSocketEventArgs args)
        {
            DebugHandler.TraceMessage("OnWebSocketEvent called", DebugSource.TASK, DebugType.ENTRY_EXIT);

            try{
                foreach (ISubWebSocketController controller in SubControllers)
                {
                    controller.OnWebSocketEvent(args);
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage(e.ToString(), DebugSource.TASK, DebugType.ERROR);
            }
        }
    }
}
