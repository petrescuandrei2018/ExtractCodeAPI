using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtractCodeAPI.Services.Abstractions;

namespace ExtractCodeAPI.Services.Implementations
{
    public class WebSocketHandler : IWebSocketHandler
    {
        public async Task HandleWebSocket(WebSocket webSocket)
        {
            string uploadFolderPath = "C:\\Uploads";
            if (!Directory.Exists(uploadFolderPath))
            {
                Directory.CreateDirectory(uploadFolderPath); // ✅ Creează folderul dacă nu există
            }

            byte[] buffer = new byte[1024 * 64]; // ✅ 64KB buffer
            string filePath = Path.Combine(uploadFolderPath, $"upload_{Guid.NewGuid()}.bin");

            Console.WriteLine($"📥 Începem salvarea fișierului la: {filePath}");

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                WebSocketReceiveResult result;
                long totalBytesReceived = 0;
                long lastReportedBytes = 0;
                DateTime lastReportTime = DateTime.Now;

                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    await fileStream.WriteAsync(buffer, 0, result.Count);
                    totalBytesReceived += result.Count;

                    // ✅ Trimit progresul la fiecare 10 secunde
                    if ((DateTime.Now - lastReportTime).TotalSeconds >= 10)
                    {
                        lastReportTime = DateTime.Now;
                        double progress = (double)totalBytesReceived / fileStream.Length * 100;
                        string progressMessage = $"{{\"progress\": {progress:F2}, \"bytes_received\": {totalBytesReceived}}}";
                        byte[] messageBuffer = Encoding.UTF8.GetBytes(progressMessage);
                        await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        Console.WriteLine($"📊 Progres: {progress:F2}% ({totalBytesReceived} bytes primiți)");
                    }

                } while (!result.CloseStatus.HasValue);

                Console.WriteLine("✅ Fișier salvat cu succes!");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Upload complet!", CancellationToken.None);
            }
        }
    }
}
