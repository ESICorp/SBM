using SBM.Model;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SBM.Service
{
    public class ConnectionTCP : IDisposable
    {
        private Regex regex = new Regex("<<START>>.*?<<END>>");

        private Socket socket;

        public int LocalPort { get { return socket == null ? -1 : ((IPEndPoint)socket.LocalEndPoint).Port; } }

        public ConnectionTCP(string ip, int port)
        {
            try
            {
                Log.Debug("SBM.Service [ConnectionTCP.Ctor] Create Socket to Agent Server " + ip + " " + port);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);

                var host = IPAddress.Parse(ip);

                var pendingConnect = socket.BeginConnect(new IPEndPoint(host, port), null, null);

                var connected = pendingConnect.AsyncWaitHandle.WaitOne(Consts.CommunicationTimeout);

                if (connected)
                {
                    socket.EndConnect(pendingConnect);
                }
                else
                {
                    throw new Exception("SBM.Service [ConnectionTCP.Ctor] Timeout connect");
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [ConnectionTCP.Ctor] Connect to Agent", e);

                throw;
            }
        }

        public void Send(string value)
        {
            //if (!socket.Connected)
            //{
            //    Log.Debug("SBM.Service [ConnectionTCP.Send] Wait for connection");

            //    socket.EndConnect(pendingConnect);
            //}

            try
            {
                Log.Debug("SBM.Service [ConnectionTCP.Send] Write <<START>>" + value + "<<END>>");

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

                socket.Send(aux, 0, aux.Length, SocketFlags.None);
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [ConnectionTCP.Send] Couldn't send", e);
            }
        }

        public string Read(TimeSpan timeout)
        {
            //if (!socket.Connected)
            //{
            //    Log.Debug("SBM.Service [ConnectionTCP.Read] Wait for connection");

            //    socket.EndConnect(pendingConnect);
            //}

            var buffer = new StringBuilder();
            try
            {
                byte[] read = new byte[1024];
                var start = DateTime.Now;

                SocketError error;

                do
                {
                    if (DateTime.Now.Subtract(start) >= timeout)
                    {
                        throw new Exception("Receive timeout");
                    }

                    //int len = socket.Receive(read, 0, 1024, SocketFlags.None, out error);
                    var handle = socket.BeginReceive(read, 0, 1024, SocketFlags.None, out error, null, null);
                    var received = handle.AsyncWaitHandle.WaitOne(timeout.Add(Consts.ThresholdTimeout));

                    if (error != SocketError.Success)
                    {
                        throw new SocketException();
                    }

                    if (received)
                    {
                        int len = socket.EndReceive(handle);

                        if (len > 0)
                        {
                            var aux = UTF8Encoding.UTF8.GetString(read, 0, len);

                            buffer.Append(aux);

                            Log.Debug("SBM.Service [ConnectionTCP.Read] Read (partial): " + aux);
                        }
                    }

                } while (!regex.IsMatch(buffer.ToString()));

                Log.Debug("SBM.Service [ConnectionTCP.Read] Read : " + buffer.ToString());
            }
            catch (ObjectDisposedException)
            {
                Log.WriteAsync("SBM.Service [ConnectionTCP.Read] Socket closed");
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [ConnectionTCP.Read] Couldn't read ", e);
            }

            //return buffer.Replace("<<START>>", string.Empty)
            //        .Replace("<<END>>", string.Empty).ToString();

            string result = buffer.ToString();
            int startIndex = result.IndexOf("<<START>>") + 9;
            int endIndex = result.IndexOf("<<END>>");

            return startIndex > endIndex ? string.Empty : result.Substring(startIndex, endIndex - startIndex);
        }

        public void Dispose()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception) { }
        }
    }
}
