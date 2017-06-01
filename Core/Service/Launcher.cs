using SBM.Component;
using SBM.Model;
using System;
using System.Linq;
using System.Threading;

namespace SBM.Service
{
    internal class Launcher : Healthy
    {
        private static volatile short running = 0;

        public Launcher()
            : base()
        {
        }

        public override void Beat(object state)
        {
            string step = string.Empty;

            try
            {
                if (Launcher.running++ > 0) return;

                ChangePriority(ThreadPriority.Normal);

                step = "quering database";
#if TRACE_BEAT
                Log.Write("SBM.Service [Launcher.Beat] " + step);
#endif
                //recorre las ejecuciones pendientes
                SBM_DISPATCHER[] dispatchers = null;
                using (var dbHelper = new DbHelper())
                {
                    dispatchers = dbHelper.TakePending();
                }

                foreach (var dispatcher in dispatchers)
                {
                    lock (base.Core.SyncList)
                    {
                        step = "check that can run ";
                        Log.Debug("SBM.Service [Launcher.Beat] " + step);

                        if (Check.CheckAll(dispatcher))
                        {
                            #region LAUNCHER
                            var context = new Context()
                                {
                                    Dispatcher = dispatcher.ID_DISPATCHER,
                                    Service = dispatcher.ID_SERVICE,
                                    ProcessName = dispatcher.SBM_SERVICE.DESCRIPTION,
                                    AssemblyDirectory = dispatcher.SBM_SERVICE.ASSEMBLY_PATH,
                                    AssemblyFullName = dispatcher.SBM_SERVICE.ASSEMBLY_FILE,
                                    Parameter = dispatcher.PARAMETERS,
                                    SingleExec = dispatcher.SBM_SERVICE.SINGLE_EXEC,
                                    Timeout = dispatcher.SBM_SERVICE.MAX_TIME_RUN == 0 ? Consts.WithoutTimeout :
                                        dispatcher.SBM_SERVICE.MAX_TIME_RUN,
                                    Evidence = Consts.Evidence,
                                    MaxTimeRun = dispatcher.SBM_SERVICE.MAX_TIME_RUN,
                                    Owner = dispatcher.ID_OWNER,
                                    Private = dispatcher.ID_PRIVATE,
                                    Started = DateTimeOffset.UtcNow,
                                    Domain = dispatcher.SBM_SERVICE.DOMAIN,
                                    User = dispatcher.SBM_SERVICE.USER,
                                    x86 = dispatcher.SBM_SERVICE.x86,
                                    Server = dispatcher.SBM_SERVICE.SBM_REMOTING.FirstOrDefault() == null ? string.Empty :
                                                dispatcher.SBM_SERVICE.SBM_REMOTING.FirstOrDefault().ID_REMOTING == 0 ? string.Empty :
                                                dispatcher.SBM_SERVICE.SBM_REMOTING.FirstOrDefault().TARGET_SERVER
                                };

                            if (dispatcher.SBM_SERVICE.PASSWORD != null)
                            {
                                step = "decrypt password ";
                                using (var dbHelper = new DbHelper())
                                {
                                    context.Password = dbHelper.Decrypt(
                                        Config.SBM_PHRASE, dispatcher.SBM_SERVICE.PASSWORD);
                                }
                            }

                            step = "prepare new process ";
                            Log.Debug("SBM.Service [Launcher.Beat] " + step + dispatcher.SBM_SERVICE.DESCRIPTION);

                            Process process = new Process(context);

                            if (!process.IsLoaded)
                            {
                                #region Assembly problem
                                step = "batch empty ";
                                Log.Debug("SBM.Service [Launcher.Beat] : " + step + dispatcher.SBM_SERVICE.DESCRIPTION);

                                using (var dbHelper = new DbHelper())
                                {
                                    dbHelper.SaveOrInsert(new SBM_DONE()
                                    {
                                        ID_DISPATCHER = dispatcher.ID_DISPATCHER,
                                        ENDED = DateTimeOffset.UtcNow,
                                        ID_DONE_STATUS = Consts.STATUS_SERVICE_UNKNOWN,
                                        RESULT = "Missing Batch implement"
                                    });
                                }

                                process.Dispose();
                                #endregion
                            }
                            else
                            {
                                #region Exec Async
                                step = "enqueue info job to controller ";
                                Log.Debug("SBM.Service [Launcher.Beat] " + step + dispatcher.SBM_SERVICE.DESCRIPTION);

                                base.Core.Running.Add(dispatcher.ID_DISPATCHER, process);

                                if (dispatcher.SBM_SERVICE.SINGLE_EXEC)
                                {
                                    base.Core.UniqueRunning.Add(dispatcher.ID_SERVICE);
                                }

                                step = "notify start ";
                                Log.Debug("SBM.Service [Launcher.Beat] " + step + dispatcher.SBM_SERVICE.DESCRIPTION);

                                using (var dbHelper = new DbHelper())
                                {
                                    //por el trigger se elimina de DISPATCHER
                                    dbHelper.Insert(new SBM_OBJ_POOL()
                                    {
                                        ID_DISPATCHER = dispatcher.ID_DISPATCHER,
                                        ID_SERVICE = dispatcher.ID_SERVICE,
                                        MAX_TIME_RUN = dispatcher.SBM_SERVICE.MAX_TIME_RUN == 0 ? (short?)null : dispatcher.SBM_SERVICE.MAX_TIME_RUN,
                                        STARTED = context.Started,
                                        PID = process.PID
                                    });

                                    //trick, cuando se fuerza la ejecución con el parent not success
                                    if (dispatcher.SBM_SERVICE.ID_PARENT_SERVICE.GetValueOrDefault(1) == 0)
                                    {
                                        //dbHelper.SetForceOff(dispatcher.ID_SERVICE);
                                        dispatcher.SBM_SERVICE.ID_PARENT_SERVICE = dispatcher.SBM_SERVICE.ID_SERVICE;
                                        dispatcher.SBM_SERVICE.SBM_SERVICE_PARENT = dispatcher.SBM_SERVICE;
                                        dbHelper.Save(dispatcher.SBM_SERVICE);
                                    }
                                }

                                step = "async execute ";
                                Log.Debug("SBM.Service [Launcher.Beat] " + step + dispatcher.SBM_SERVICE.DESCRIPTION);

                                //ejecuta en forma asincronica
                                process.ExecuteAsync();

                                step = "launch wait to finish ";
                                Log.Debug("SBM.Service [Launcher.Beat] " + step + dispatcher.SBM_SERVICE.DESCRIPTION);

                                ThreadPool.QueueUserWorkItem(
                                    new WaitCallback(this.WaitFinish), process);
                                #endregion
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Launcher.Beat] " + step, e);
            }
            finally
            {
                Launcher.running = 0;
            }
        }

        private void WaitFinish(object state)
        {
            var process = (Process)state;

            try
            {
                process.Join();
            }
            catch { }

            try
            {
                Log.Debug("SBM.Service [Launcher.WaitFinish] Cleaning controllers " + process.Name);

                lock (base.Core.SyncList)
                {
                    base.Core.Running.Remove(process.Dispatcher);

                    if (process.SingleExec)
                    {
                        base.Core.UniqueRunning.Remove(process.Service);
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Process.WaitFinish] Cleaning controllers", e);
            }

            for (int i = 1; i <= 3; i++)
            {
                try
                {
                    Log.Debug("SBM.Service [Launcher.WaitFinish] Notify database (" + i + ") " + process.Name);

                    using (var dbHelper = new DbHelper())
                    {
                        dbHelper.SaveOrInsert(new SBM_DONE()
                        {
                            ID_DISPATCHER = process.Dispatcher,
                            ENDED = DateTimeOffset.UtcNow,

                            ID_DONE_STATUS =
                                    process.Timeout == 0 ? Consts.STATUS_CANCELLED_BY_USER :
                                    process.IsTimeout ? Consts.STATUS_TIMEOUT :
                                    process.Exceptions.Count == 0 ? Consts.STATUS_SUCCESS :
                                    Consts.STATUS_WITH_ERRORS,

                            RESULT = !string.IsNullOrEmpty(process.Result) ? process.Result.Left(4000) :
                                process.Exceptions != null && process.Exceptions.Count > 0 ? Log.ToString(process.Exceptions, 4000) :
                                process.IsTimeout && process.Timeout == 0 ? "User canceled" :
                                process.IsTimeout ? string.Format("Maximum running time exceeded, threshold: {0} seconds", process.Timeout) :
                                string.Empty
                        });
                    }

                    break;
                }
                catch (Exception e)
                {
                    Log.WriteAsync("SBM.Service [Process.WaitFinish]  Notify database (" + i + ") " + process.Name, e);

                    Thread.Sleep(3000);
                }
            }

            process.Dispose();
        }

        /// <summary>
        /// Stop process
        /// </summary>
        public override void Stop()
        {
            lock (base.Core.SyncList)
            {
                base.Core.Running.Values.ToList<Process>().ForEach(x => x.Cancel());
            }
        }

        /// <summary>
        /// Stop process immediate
        /// </summary>
        public override void Defunct()
        {
            lock (base.Core.SyncList)
            {
                base.Core.Running.Values.ToList<Process>().ForEach(x => x.Cancel());
            }
        }

        public override void Dispose()
        {
        }
    }
}
