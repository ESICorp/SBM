using SBM.Common;
using SBM.Component;
using SBM.Model;
using System;
using System.Threading;

namespace SBM.Service
{
    internal class Canceller : Healthy
    {
        private static volatile short running = 0;

        public Canceller()
        {
            Semaphore semaphore = null;
            try
            {
                semaphore = new Semaphore(0, 1, "Global\\SBM_CANCEL");
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Canceller.Ctor] NEW : " + e);
            }
            try
            {
                semaphore = Semaphore.OpenExisting("Global\\SBM_CANCEL");
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Canceller.Ctor] OPEN : " + e);
            }

            try
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(WaitSemaphore), semaphore);
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Canceller.Ctor] THREAD : " + e);
            }
        }

        private void WaitSemaphore(object state)
        {
            try
            {
                ChangePriority(ThreadPriority.AboveNormal);

                var semaphore = state as Semaphore;

                while (!stop)
                {
                    if (semaphore.WaitOne(Consts.ThresholdTimeout))
                    {
                        Log.Debug("SBM.Service [Canceller.WaitSemaphore]");

                        Beat(null);
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Canceller.WaitSemaphore] " + e);
            }
        }

        public override void Beat(object state)
        {

#if TRACE_BEAT
            Log.Write("SBM.Service [Canceller.Beat]");
#endif
            try
            {
                if (Canceller.running++ > 0) return;

                ChangePriority(ThreadPriority.AboveNormal);

                //recorre los procesos a cancelar
                SBM_OBJ_POOL[] jobs = null;

                using (var dbHelper = new DbHelper())
                {
                    jobs = dbHelper.GetMakedToCancel();
                }

                foreach (var job in jobs)
                {
                    try
                    {
                        lock (base.Core.SyncList)
                        {
                            Process process;

                            //si está en ejecución
                            if (base.Core.Running.TryGetValue(
                                job.ID_DISPATCHER, out process))
                            {
                                process.Cancel();

                                Log.Debug("SBM.Service [Canceller.Beat] Cancel : " + job.ID_DISPATCHER);
                            }
                            else
                            {
                                Log.WriteAsync("SBM.Service [Canceller.Beat] Couldn't find Dispatcher ID " + job.ID_DISPATCHER + " to cancel");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.WriteAsync("SBM.Service [Canceller.Beat] Couldn't cancel Dispatcher ID " + job.ID_DISPATCHER, e);

                        //    using (var dbHelper = new DbHelper())
                        //    {
                        //        dbHelper.AddEventLog(new SBM_EVENT_LOG()
                        //            {
                        //                ID_EVENT = Consts.LOG_ERROR_KILLING_SERVICE,
                        //                DESCRIPTION = "Couldn't kill " +
                        //                    " (ID_DISPATCHER=" + job.ID_DISPATCHER + ", " +
                        //                    "ID_SERVICE=" + job.ID_SERVICE + ", " +
                        //                    "STARTED=" + job.STARTED.ToString("u") + ", " +
                        //                    "MAX_TIME_RUN=" + job.MAX_TIME_RUN + ") "
                        //            });
                        //    }
                    }

                } //foreach list
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Canceller.Beat]", e);
            }
            finally
            {
                Canceller.running = 0;
            }

        } //method

        public override void Dispose()
        {
        }

    } //class

} //namespace
