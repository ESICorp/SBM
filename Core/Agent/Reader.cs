using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SBM.Agent
{
    public class Reader
    {
        public const int BUFFER_SIZE = 1024;

        public byte[] buffer = new byte[BUFFER_SIZE];

        private Socket socket = null;
        private StringBuilder text = new StringBuilder();

        private Regex regexStart = new Regex("<<START>>");
        private Regex regexComplete = new Regex("<<START>>.*?<<END>>");

        public Reader(Socket socket)
        {
            this.socket = socket;
        }

        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                int len = socket.EndReceive(ar);

                if (len > 0)
                {
                    var aux = UTF8Encoding.UTF8.GetString(buffer, 0, len);

                    Log.Debug("SBM.Agent [Reader.ReadCallback] Read (partial): " + aux);

                    text.Append(aux);

                    var content = text.ToString();

                    if (regexComplete.IsMatch(content))
                    {
                        //Log.Debug("SBM.Agent [Reader.ReadCallback] Read : " + content);

                        //content = content.Replace("<<START>>", string.Empty).Replace("<<END>>", string.Empty);

                        int startIndex = content.IndexOf("<<START>>") + 9;
                        int endIndex = content.IndexOf("<<END>>");
                        content = content.Substring(startIndex, endIndex - startIndex);

                        var cmd = new Command(socket, content);
                            
                        cmd.Execute();
                    }
                    else if (regexStart.IsMatch(content))
                    {
                        socket.BeginReceive(buffer, 0, BUFFER_SIZE, 0,
                            new AsyncCallback(ReadCallback), null);
                    }
                    else
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write("SBM.Agent [Reader.ReadCallback] Coludn't read", e);
            }
        }
    }
}
