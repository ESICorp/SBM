using Microsoft.AspNet.SignalR;

namespace SBM.Web
{
    public class DebugHub : Hub
    {
        public void Send(string time, int pid, string text)
        {
            Clients.All.broadcast(time, pid, text);
        }
    }
}