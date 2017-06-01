using SBM.Component;
using SBM.Model;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace SBM.Service
{
    /// <summary>
    /// Bootstrap Service
    /// </summary>
    public class Bootstrap : ServiceBase
    {
        /// <summary>
        /// Execution Timer
        /// </summary>
        private Timer Timer = null;

        /// <summary>
        /// Heart
        /// </summary>
        private Healthy Hearth;

        /// <summary>
        /// Tracer
        /// </summary>
        //private TraceListener traceListener;

        private String Init;

        private String[] Problems;

        /// <summary>
        /// Entry Point
        /// </summary>
        /// <param name="args">not used</param>
        public static void Main(string[] args)
        {
            Bootstrap bootstrap = new Bootstrap();

            if (Environment.UserInteractive)
            {
                TraceListener consoleTraceListener = new ConsoleTraceListener();
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

        /// <summary>
        /// Constructor
        /// </summary>
        public Bootstrap()
        {
            try
            {
                this.Init = Config.Initialize(ref this.Problems);
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.Ctor] Init", e);
            }

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.PowerModeChange] Suspend");

                try
                {
                    using (var dbHelper = new DbHelper())
                    {
                        dbHelper.AddEventLog(
                            new SBM_EVENT_LOG()
                            {
                                ID_EVENT = Consts.LOG_APPLICATION_STOPPED,
                                DESCRIPTION = "Power Suspend"
                            });
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteAsync("SBM.Service [Bootstrap.PowerModeChange]", ex);
                }
            }
            else if (e.Mode == PowerModes.Resume)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.PowerModeChange] Resume");

                try
                {
                    using (var dbHelper = new DbHelper())
                    {
                        dbHelper.AddEventLog(
                            new SBM_EVENT_LOG()
                            {
                                ID_EVENT = Consts.LOG_APPLICATION_STARTED,
                                DESCRIPTION = "Power Resume"
                            });
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteAsync("SBM.Service [Bootstrap.PowerModeChange]", ex);
                }
            }
        }

        /// <summary>
        /// Service Start Event
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Log.WriteAsync("SBM.Service [Bootstrap.OnStart] Start");

            try
            {
                if (this.Hearth == null)
                {
                    BeforeStart.Perform();

                    this.Hearth = new Health();
                }

                this.Timer = new Timer(this.Hearth.Beat, null,
                    TimeSpan.Zero, TimeSpan.FromSeconds(Config.SBM_TIMER_INTERVAL));

                using (var dbHelper = new DbHelper())
                {
                    if (this.Problems != null)
                    {
                        foreach (var problem in this.Problems)
                        {
                            dbHelper.AddEventLog(
                                new SBM_EVENT_LOG()
                                {
                                    ID_EVENT = Consts.LOG_AUDIT,
                                    DESCRIPTION = problem
                                });
                        }
                    }

                    dbHelper.AddEventLog(
                        new SBM_EVENT_LOG()
                        {
                            ID_EVENT = Consts.LOG_APPLICATION_STARTED,
                            DESCRIPTION = this.Init
                        });
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.OnStart]", e);
            }
        }

        /// <summary>
        /// Service Pause Event
        /// </summary>
        protected override void OnPause()
        {
            Log.WriteAsync("SBM.Service [Bootstrap.OnPause] Pause");

            this.Timer.Change(
                Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            try
            {
                using (var dbHelper = new DbHelper())
                {
                    dbHelper.AddEventLog(
                        new SBM_EVENT_LOG()
                        {
                            ID_EVENT = Consts.LOG_APPLICATION_STOPPED,
                            DESCRIPTION = "Service Paused"
                        });
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.OnPause]", e);
            }

        }

        /// <summary>
        /// Service Continue Event
        /// </summary>
        protected override void OnContinue()
        {
            Log.WriteAsync("SBM.Service [Bootstrap.OnContinue] Continue");

            this.Timer.Change(
                TimeSpan.Zero, TimeSpan.FromSeconds(Config.SBM_TIMER_INTERVAL));

            try
            {
                using (var dbHelper = new DbHelper())
                {
                    dbHelper.AddEventLog(
                        new SBM_EVENT_LOG()
                        {
                            ID_EVENT = Consts.LOG_APPLICATION_STARTED,
                            DESCRIPTION = "Service Released"
                        });
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.OnContinue]", e);
            }
        }

        /// <summary>
        /// Service Stop Event
        /// </summary>
        protected override void OnStop()
        {
            Log.WriteAsync("SBM.Service [Bootstrap.OnStop] Stop");

            this.Timer.Dispose();
            this.Timer = null;

            this.Hearth.Stop();
            this.Hearth.Dispose();

            try
            {
                using (var dbHelper = new DbHelper())
                {
                    dbHelper.AddEventLog(
                        new SBM_EVENT_LOG()
                        {
                            ID_EVENT = Consts.LOG_APPLICATION_STOPPED,
                            DESCRIPTION = "Service Stopped"
                        });
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.OnStop]", e);
            }
        }

        /// <summary>
        /// Service Shutdown Machine Event
        /// </summary>
        protected override void OnShutdown()
        {
            Log.WriteAsync("SBM.Service [Bootstrap.OnShutdown] Shutdown");

            this.Timer.Dispose();
            this.Timer = null;

            this.Hearth.Defunct();
            this.Hearth.Dispose();

            try
            {
                using (var dbHelper = new DbHelper())
                {
                    dbHelper.AddEventLog(
                        new SBM_EVENT_LOG()
                        {
                            ID_EVENT = Consts.LOG_APPLICATION_STOPPED,
                            DESCRIPTION = "Service Stopped on Shutdown"
                        });
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.OnShutdown]", e);
            }
        }

        private void InitializeComponent()
        {
            // 
            // Bootstrap
            // 
            this.CanHandlePowerEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.ServiceName = "SBM.Service";
        }

        public static string PickShutdown()
        {
            string time = null;
            try
            {
                var path = Path.Combine(
                    AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "suicide");

                if (File.Exists(path))
                {
                    time = File.ReadAllText(path, Encoding.UTF8);

                    File.Delete(path);

                    Log.WriteAsync("SBM.Service [Bootstrap.PickShutdown] Committed suicide at " + time);
                }
                else
                {
                    Log.Debug("SBM.Service [Bootstrap.PickShutdown] Wasn't suicide");
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.PickShutdown]", e);
            }
            return time;
        }

        public static void Shutdown(object state)
        {
            try
            {
                string time = DateTime.Now.ToString("s");

                string path = Path.Combine(
                    AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "suicide");

                Log.Debug("SBM.Service [Bootstrap.Shutdown] " + path + " " + time);

                File.WriteAllText(path, time, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Bootstrap.Shutdown]", e);
            }

            using (var sc = new ServiceController("SBM.Service"))
            {
                sc.Stop();
            }
        }
    }
}
