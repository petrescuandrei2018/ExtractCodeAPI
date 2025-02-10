using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ExtractCodeAPI.Services.Abstractions
{
    public interface IWebSocketHandler
    {
        Task HandleWebSocket(WebSocket webSocket);
    }
}
