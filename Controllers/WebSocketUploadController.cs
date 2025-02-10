using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

[Route("api/upload")]
[ApiController]
public class WebSocketUploadController : ControllerBase
{
    [HttpGet("ws")]
    public async Task<IActionResult> WebSocketEndpoint()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            return BadRequest("Aceasta nu este o cerere WebSocket.");
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await ReceiveFile(webSocket);

        return Ok("Upload finalizat.");
    }

    private async Task ReceiveFile(WebSocket webSocket)
    {
        byte[] buffer = new byte[1024 * 64]; // ✅ Creștem buffer-ul la 64KB
        string filePath = Path.Combine("C:\\Uploads", $"upload_{Guid.NewGuid()}.bin");

        Console.WriteLine($"📥 Începem salvarea fișierului la: {filePath}");

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                await fileStream.WriteAsync(buffer, 0, result.Count);
                Console.WriteLine($"📤 Am scris {result.Count} bytes...");
            }
            while (!result.CloseStatus.HasValue);
        }

        Console.WriteLine("✅ Fișier salvat cu succes!");
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Upload complet!", CancellationToken.None);
    }
}
