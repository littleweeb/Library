using LittleWeebLibrary.EventArguments;
using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Controllers
{
    public interface ISubWebSocketController
    {
        void OnWebSocketEvent(WebSocketEventArgs args);
    }
}
