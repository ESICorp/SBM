using System;
using System.Threading;

namespace SBM.Wrapper
{
    public class EntryPoint
    {
        public static int Run()
        {
            int count = 0;
            try
            {
                Log.Debug("SBM.Wrapper [Program.Main] Start");

                using (var reader = new Reader())
                {
                    if (!reader.IsConnected)
                    {
                        throw new Exception("Couldn't connect");
                    }

                    while (reader.Next())
                    {
                        ThreadPool.QueueUserWorkItem(
                              new WaitCallback(Async), reader.Text);

                        count++;
                    }
                }

                Log.Debug("SBM.Wrapper [Program.Main] End ");
            }
            catch (Exception e)
            {
                Log.Write("SBM.Wrapper [Program.Main]", e);

                count = -1;
            }
            return count;
        }

        private static void Async(object state)
        {
            var step = "create command";
            try
            {
                var command = new Command(state as string);

                step = "execute command";

                string output = command.Execute();

                step = "create writer";

                using (var writer = new Writer())
                {
                    step = "identify action";

                    if (command.Request.Action == Action.Cancel)
                    {
                        step = "write on cancel";

                        writer.WriteChannelCancel(output);
                    }
                    else
                    {
                        step = "write on response";

                        writer.WriteChannelResponse(output);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write("SBM.Wrapper [EntryPoint.Async] Couldn't " + step + ": ", e);
            }
        }
    }
}
