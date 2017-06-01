using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBM.PowerShell
{
    internal static class Remote
    {
        private static string QUIT = "{'Action':'Quit'}";

        public static async Task Connect(ClientWebSocket socket, Uri uri)
        {
            await socket.ConnectAsync(uri, CancellationToken.None);
        }

        public static async Task Send(ClientWebSocket socket, string buffer)
        {
            var message = new ArraySegment<byte>(UTF8Encoding.UTF8.GetBytes(buffer));

            await socket.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<string> Receive(ClientWebSocket socket)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

            return UTF8Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
        }

        public static async Task Close(ClientWebSocket socket)
        {
            await Send(socket, QUIT);

            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal Closure", CancellationToken.None).Wait();
        }
    }
}
