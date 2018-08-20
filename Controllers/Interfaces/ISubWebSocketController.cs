using LittleWeebLibrary.EventArguments;

namespace LittleWeebLibrary.Controllers
{
    public interface ISubWebSocketController
    {
        void OnWebSocketEvent(WebSocketEventArgs args);
    }
}
