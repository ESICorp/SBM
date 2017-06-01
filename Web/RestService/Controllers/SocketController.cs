using SBM.Component;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;

namespace SBM.RestService
{
    public class SocketController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Index()
        {
            var context = HttpContext.Current;
            if (context.IsWebSocketRequest || context.IsWebSocketRequestUpgrading)
            {
                context.AcceptWebSocketRequest(Process);
            }

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        private async Task Process(AspNetWebSocketContext context)
        {
            string response = null;
            var buffer = new ArraySegment<byte>(new byte[1024]);
            var socket = context.WebSocket;
            int errors = 0;

            while (socket.State == WebSocketState.Open || errors < 3)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                var raw = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

                try
                {
                    var request = JsonConvert.DeserializeObject<dynamic>(raw);

                    string ACTION = request.Action;

                    if (ACTION == null)
                    {
                        errors++;
                        response = "{'Fault':'Invalid Action'}";
                    }
                    else if ("Enqueue".Equals(ACTION, StringComparison.InvariantCultureIgnoreCase))
                    {
                        int ID_SERVICE = request.ID_SERVICE;
                        short ID_OWNER = request.ID_OWNER;
                        string ID_PRIVATE = request.ID_PRIVATE;
                        string PARAMETERS = request.PARAMETERS;
                        
                        using (var dbHelper = new DbHelper())
                        {
                            if (dbHelper.GetPermission(ID_SERVICE, ID_OWNER))
                            {
                                var handle = dbHelper.Enqueue(ID_OWNER, ID_SERVICE, ID_PRIVATE, PARAMETERS);

                                response = string.Format("{{'Handle':'{0}'}}", handle);
                            }
                            else
                            {
                                errors++;

                                response = "{'Fault':'Access Denied'}";
                            }
                        }
                    }
                    else if ("Wait".Equals(ACTION, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Guid HANDLE = request.HANDLE;
                        long TIMEOUT = request.TIMEOUT;

                        using (var dbHelper = new DbHelper())
                        {
                            var done = dbHelper.Join(HANDLE, TIMEOUT);

                            response = JsonConvert.SerializeObject(done,
                                Newtonsoft.Json.Formatting.None,
                                new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore
                                });
                        }
                    }
                    else if ("Quit".Equals(ACTION, StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    errors++;

                    response = string.Format("{{'Fault':'{0}'}}", e.Message);
                }

                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(response));

                await socket.SendAsync(
                    buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }

            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Quit", CancellationToken.None);
            }
        }
    }
}