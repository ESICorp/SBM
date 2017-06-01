using SBM.Model;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;

namespace SBM.Wrapper
{
    public abstract class Pipe
    {
        protected NamedPipeClientStream[] pipes;

        public bool IsConnected { get; set; }

        public Pipe(string prefix, PipeDirection direction)
        {
            Log.Debug("SBM.Wrapper [Pipe.Ctror] Pipe Name dispatcher_" + prefix + "_" + Process.GetCurrentProcess().Id.ToString());
            
            pipes = new[] {new NamedPipeClientStream(".",
                    "dispatcher_" + prefix + "_" + Process.GetCurrentProcess().Id.ToString(),
                    direction,
                    PipeOptions.None,
                    TokenImpersonationLevel.Impersonation) 
                };

            try
            {
                Log.Debug("SBM.Wrapper [Pipe.Ctror] Connect channel dispatcher_" + prefix + "_" + Process.GetCurrentProcess().Id.ToString());

                pipes[0].Connect(Consts.CommunicationTimeout.Milliseconds);

                IsConnected = pipes[0].IsConnected;
            }
            catch (Exception e)
            {
                IsConnected = false;

                Log.Write("SBM.Wrapper [Pipe.Ctror] Cound't connect channel dispatcher_" + prefix + "_" + Process.GetCurrentProcess().Id.ToString(), e);
            }
        }

        public Pipe(string prefix1, string prefix2, PipeDirection direction)
        {
            Log.Debug("SBM.Wrapper [Pipe.Ctror] Pipe Name dispatcher_" +
                prefix1 + "_" + Process.GetCurrentProcess().Id.ToString() + " and " +
                "dispatcher_" + prefix2 + "_" + Process.GetCurrentProcess().Id.ToString());

            pipes = new[] {
                new NamedPipeClientStream(".",
                    "dispatcher_" + prefix1 + "_" + Process.GetCurrentProcess().Id.ToString(),
                    direction,
                    PipeOptions.None,
                    TokenImpersonationLevel.Impersonation), 
                new NamedPipeClientStream(".",
                    "dispatcher_" + prefix2 + "_" + Process.GetCurrentProcess().Id.ToString(),
                    direction,
                    PipeOptions.None,
                    TokenImpersonationLevel.Impersonation) 
                };

            Log.Debug("SBM.Wrapper [Pipe.Ctror] Connect channels dispatcher_" +
                prefix1 + "_" + Process.GetCurrentProcess().Id.ToString() + " and " +
                "dispatcher_" + prefix2 + "_" + Process.GetCurrentProcess().Id.ToString());

            try
            {
                pipes[0].Connect(Consts.CommunicationTimeout.Milliseconds);

                IsConnected = pipes[0].IsConnected;
            }
            catch (Exception e)
            {
                IsConnected = false;

                Log.Write("SBM.Wrapper [Pipe.Ctror] Couldn't connect channels dispatcher_" +
                prefix1 + "_" + Process.GetCurrentProcess().Id.ToString(), e);
            }
            try
            {
                pipes[1].Connect(Consts.CommunicationTimeout.Milliseconds);

                IsConnected = pipes[0].IsConnected && pipes[1].IsConnected;
            }
            catch (Exception e)
            {
                IsConnected = false;

                Log.Write("SBM.Wrapper [Pipe.Ctror] Couldn't connect channels dispatcher_" +
                prefix2 + "_" + Process.GetCurrentProcess().Id.ToString(), e);
            }
        }

        public void Close()
        {
            try
            {
                pipes.ToList().ForEach(p => p.Dispose());
            }
            catch (Exception) { }
        }
    }
}
