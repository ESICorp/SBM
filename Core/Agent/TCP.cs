using System;
using System.Net;
using System.Net.Sockets;

namespace SBM.Agent
{
    public class TCP : IDisposable
    {
        private static TCP instance = null;
        private Socket listener;
        public TCP()
        {
            try
            {
                Log.Debug("SBM.Agent [TCP.Ctror] Listen port " + Config.SBM_LISTEN_PORT);

                listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(new IPEndPoint(IPAddress.Any, Config.SBM_LISTEN_PORT));
                listener.Listen(25);
            }
            catch (Exception e)
            {
                Log.Write("SBM.Agent [TCP.Ctror] Cound't listen", e);
            }
        }

        public static void Start()
        {
            if ( instance == null )
            {
                instance = new TCP();
            }

            instance.listener.BeginAccept(new AsyncCallback(instance.AcceptCallback), null);
        }

        public static void Stop()
        {
            if (instance != null)
            {
                instance.Close();
                instance = null;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var socket = listener.EndAccept(ar);

            listener.BeginAccept(new AsyncCallback(AcceptCallback), null);

            var reader = new Reader(socket);

            socket.BeginReceive(reader.buffer, 0, Reader.BUFFER_SIZE, 0,
                new AsyncCallback(reader.ReadCallback), null);
        }

        public void Close()
        {
            try
            {
                if (listener != null)
                {
                    listener.Shutdown(SocketShutdown.Both);
                    listener.Close();
                }
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            if ( this.listener != null )
            {
                listener.Dispose();
            }
        }
    }
}
