using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SBM.Agent
{
    public class Writer 
    {
        private Socket socket;

        public Writer(Socket socket) 
        {
            this.socket = socket;
        }

        public void Send(string value)
        {
            try
            {
                Log.Debug("SBM.Agent [Write.Send] Send " + value);

                byte[] aux = null;
                using (var stream = new MemoryStream())
                {
                    aux = UTF8Encoding.UTF8.GetBytes("<<START>>");
                    stream.Write(aux, 0, aux.Length);

                    aux = UTF8Encoding.UTF8.GetBytes(value);
                    stream.Write(aux, 0, aux.Length);

                    aux = UTF8Encoding.UTF8.GetBytes("<<END>>");
                    stream.Write(aux, 0, aux.Length);

                    aux = stream.ToArray();
                }

                socket.BeginSend(aux, 0, aux.Length, 0, new AsyncCallback(SendCallback), null);
            }
            catch (Exception e)
            {
                Log.Write("SBM.Agent [Writer.Send] Couldn't send", e);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                //Socket handler = (Socket)ar.AsyncState;
                int bytesSent = socket.EndSend(ar);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Log.Write("SBM.Agent [Writer.SendCallback] Couldn't send", e);
            }
        }

    }
}
