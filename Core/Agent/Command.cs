using SBM.Model;
using System.Net.Sockets;
using System.Threading;

namespace SBM.Agent
{
    public class Command 
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }

        private Socket Socket { get; set; }
        public Command(Socket socket, string readed)
        {
            this.Socket = socket;
            this.Response = new Response();
            this.Request = new Request(readed);
        }

        public void Execute()
        {
            Action action = null;

            if (Request.Action == Action.Submit)
            {
                ThreadPool.QueueUserWorkItem(x =>
                {
                    while (Core.GetInstance().Running.ContainsKey(Request.Dispatcher))
                    {
                        if (this.Socket.Poll(Consts.ThresholdTimeout.Milliseconds, SelectMode.SelectError))
                        {
                            new ActionCancel(
                                new Request(string.Format("<?xml version='1.0' encoding='UTF-8'?><Request Action='Cancel' Dispatcher='{0}'/>", Request.Dispatcher)),
                                new Response()).Execute();
                        }
                    }
                });

                action = new ActionSubmit(this.Request, this.Response);
            }
            else if (Request.Action == Action.Cancel)
            {
                action = new ActionCancel(this.Request, this.Response);
            }

            if (action != null)
            {
                action.Execute();
            }

            var writer = new Writer(this.Socket);

            writer.Send(this.Response.ToString());
        }
    }
}
