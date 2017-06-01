using SBM.Model;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SBM.Service
{
    public class ConnectionPipe : IDisposable
    {
        /// <summary>
        /// Process
        /// </summary>
        private System.Diagnostics.Process process;

        private Regex regex = new Regex("<<START>>.*?<<END>>");

        private NamedPipeServerStream[] pipes;
        private IAsyncResult[] pendingConnect;

        public int PID { get { return process == null ? -1 : process.Id; } }

        public ConnectionPipe(bool x86)
        {
            try
            {
                var processInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    FileName = "SBM.Wrapper" + (x86 ? "32" : "64") + ".exe",
                    UseShellExecute = false,
                    ErrorDialog = false,
                    CreateNoWindow = true
                };
                
                this.process = System.Diagnostics.Process.Start(processInfo);
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [ConnectionPipe.Ctor] Process Start", e);

                throw;
            }

            try
            {
                var security = new PipeSecurity();

                security.AddAccessRule(new PipeAccessRule(
                    WindowsIdentity.GetCurrent().Name,
                    PipeAccessRights.FullControl,
                    AccessControlType.Allow));

                Log.Debug("SBM.Service [ConnectionPipe.Ctor] Create Pipes Server " + process.Id.ToString());

                this.pipes = new[] {
                        new NamedPipeServerStream(
                            "dispatcher_from_wrapper_response_" + process.Id.ToString(),
                            PipeDirection.In,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous,
                            1024, 1024,
                            security),
                        new NamedPipeServerStream(
                            "dispatcher_from_wrapper_cancel_" + process.Id.ToString(),
                            PipeDirection.In,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous,
                            1024, 1024,
                            security),
                        new NamedPipeServerStream(
                            "dispatcher_to_wrapper_" + process.Id.ToString(),
                            PipeDirection.Out,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous,
                            1024, 1024,
                            security)};

                Log.Debug("SBM.Service [ConnectionPipe.Ctor] Listen all channels " + process.Id.ToString());

                this.pendingConnect = new[] {
                        pipes[0].BeginWaitForConnection(null, null),
                        pipes[1].BeginWaitForConnection(null, null),
                        pipes[2].BeginWaitForConnection(null, null)};

                if (this.pendingConnect[2].AsyncWaitHandle.WaitOne(Consts.CommunicationTimeout))
                {
                    this.pipes[2].EndWaitForConnection(this.pendingConnect[2]);
                }
                else 
                {
                    throw new Exception("SBM.Service [ConnectionPipe.Ctor] Timeout connect");
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [ConnectionPipe.Ctor] Couldn't connect", e);

                throw;
            }
        }

        public void Send(string value)
        {
            //if (!pipes[2].IsConnected)
            //{
            //    Log.Debug("SBM.Service [ConnectionPipe.Send] Wait for connection on channel 2");

            //    pipes[2].EndWaitForConnection(pendingConnect[2]);
            //}

            try
            {
                Log.Debug("SBM.Service [ConnectionPipe.Send] Write on channel dispatcher_to_wrapper: <<START>>" + value + "<<END>>");

                byte[] aux = UTF8Encoding.UTF8.GetBytes("<<START>>");
                this.pipes[2].Write(aux, 0, aux.Length);

                aux = UTF8Encoding.UTF8.GetBytes(value);
                this.pipes[2].Write(aux, 0, aux.Length);

                aux = UTF8Encoding.UTF8.GetBytes("<<END>>");
                this.pipes[2].Write(aux, 0, aux.Length);
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [ConnectionPipe.Send] Couldn't send", e);
            }
        }

        public string ReadChannelResponse(TimeSpan timeout)
        {
            try {
                if (this.pendingConnect[0].AsyncWaitHandle.WaitOne(timeout.Add(Consts.ThresholdTimeout)))
                {
                    this.pipes[0].EndWaitForConnection(this.pendingConnect[0]);

                    return Read(0, timeout);
                }
                else
                {
                    //throw new Exception("Channel dispatcher_from_wrapper_response lost");
                    return String.Empty;
                }
            }
            catch (ObjectDisposedException)
            {
                Log.WriteAsync("SBM.Service [ConnectionPipe.ReadChannelResponse] Pipe closed");

                return String.Empty;
            }
        }

        public string ReadChannelCancel(TimeSpan timeout)
        {
            try {
                if (this.pendingConnect[1].AsyncWaitHandle.WaitOne(timeout.Add(Consts.ThresholdTimeout)))
                {
                    this.pipes[0].EndWaitForConnection(this.pendingConnect[1]);

                    return Read(1, timeout);
                }
                else
                {
                    //throw new Exception("Channel dispatcher_from_wrapper_cancel lost");
                    return string.Empty;
                }
            }
            catch (ObjectDisposedException)
            {
                Log.WriteAsync("SBM.Service [ConnectionPipe.ReadChannelCancel] Pipe closed");

                return String.Empty;
            }
        }

        private string Read(int channel, TimeSpan timeout)
        {
            //if (!pipes[channel].IsConnected)
            //{
            //    Log.Debug("SBM.Service [ConnectionPipe.Read] Wait for connection on channel " + channel.ToString());

            //    pipes[channel].EndWaitForConnection(pendingConnect[channel]);
            //}

            var buffer = new StringBuilder();
            try
            {
                byte[] read = new byte[1024];
                var start = DateTime.Now;

                do
                {
                    if (DateTime.Now.Subtract(start) >= timeout)
                    {
                        throw new Exception("Read timeout");
                    }

                    //int len = pipes[channel].Read (read, 0, 1024);
                    var handle = this.pipes[channel].BeginRead(read, 0, 1024, null, null);

                    if (handle.AsyncWaitHandle.WaitOne(timeout.Add(Consts.ThresholdTimeout)))
                    {
                        int len = this.pipes[channel].EndRead(handle);

                        if (len > 0)
                        {
                            var aux = UTF8Encoding.UTF8.GetString(read, 0, len);

                            buffer.Append(aux);

                            Log.Debug("SBM.Service [ConnectionPipe.Read] Read (partial): " + aux);
                        }
                    }

                } while (!regex.IsMatch(buffer.ToString()));

                Log.Debug("SBM.Service [ConnectionPipe.Read] Read channel : " + buffer.ToString());
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [ConnectionPipe.Read] Couldn't read channel", e);
            }

            //return buffer
            //        .Replace("<<START>>", string.Empty)
            //        .Replace("<<END>>", string.Empty).ToString();

            string result = buffer.ToString();
            int startIndex = result.IndexOf("<<START>>") + 9;
            int endIndex = result.IndexOf("<<END>>");

            return startIndex > endIndex ? string.Empty : result.Substring(startIndex, endIndex - startIndex);
        }

        private void Shutdown()
        {
            try
            {
                Send("<<SHUTDOWN>>");

                var timeout = DateTime.Now.Add(Consts.CommunicationTimeout);
                while (this.process != null && !this.process.HasExited && DateTime.Now < timeout)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }

                if (this.process == null || this.process.HasExited)
                {
                    Log.WriteAsync("SBM.Service [ConnectionPipe.Shutdown] : Process finished");
                }
                else
                {
                    this.process.Kill();
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [ConnectionPipe.Shutdown] ", e);
            }
        }

        public void Dispose()
        {
            this.Shutdown();

            try
            {
                this.pipes.ToList().ForEach(p => p.Dispose());
            }
            catch (Exception) { }

            try
            {
                this.process.Dispose();
            }
            catch (Exception) { }
        }
    }
}
