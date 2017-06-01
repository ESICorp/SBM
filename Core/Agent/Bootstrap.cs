using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace SBM.Agent
{
    public partial class Bootstrap : ServiceBase
    {
        private String Init;

        private String[] Problems;

        public Bootstrap()
        {
            this.Init = Config.Initialize(ref this.Problems);

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            Bootstrap bootstrap = new Bootstrap();

            if (Environment.UserInteractive)
            {
                var consoleTraceListener = new ConsoleTraceListener();
                Trace.Listeners.Add(consoleTraceListener);
                Trace.AutoFlush = true;

                Console.WriteLine("Press any key to start : ");
                Console.ReadKey();
                bootstrap.OnStart(args);

                char keyPressed = '\0';
                while (keyPressed != 'S' && keyPressed != 's')
                {
                    Console.WriteLine("Press (P)ause, (C)ontinue or (S)top : ");
                    keyPressed = Console.ReadKey().KeyChar;

                    switch (keyPressed)
                    {
                        case 'P':
                        case 'p':
                            Console.WriteLine("Paused");
                            bootstrap.OnPause();
                            break;

                        case 'C':
                        case 'c':
                            Console.WriteLine("Continue");
                            bootstrap.OnContinue();
                            break;

                        case 'S':
                        case 's':
                            Console.WriteLine("Stopped");
                            bootstrap.OnStop();
                            break;
                    }
                }

                //bootstrap.OnShutdown();

                Trace.Listeners.Remove(consoleTraceListener);
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { bootstrap });
            }
        }

        protected override void OnStart(string[] args)
        {
            Log.Write("SBM.Agent [Bootstrap.OnStart] Start");

            TCP.Start();
        }

        protected override void OnStop()
        {
            Log.Write("SBM.Agent [Bootstrap.OnStop] Stop");

            TCP.Stop();
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                Log.Write("SBM.Agent [Bootstrap.PowerModeChange] Suspend");

                TCP.Stop();
            }
            else if (e.Mode == PowerModes.Resume)
            {
                Log.Write("SBM.Agent [Bootstrap.PowerModeChange] Resume");

                TCP.Start();
            }
        }
    }
}
