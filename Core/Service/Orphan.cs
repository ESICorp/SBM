using SBM.Component;
using SBM.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBM.Service
{
    internal class Orphan : Healthy
    {
        private static volatile short running = 0;

        public Orphan()
        {
        }

        public override void Beat(object state)
        {
            try
            {
                if (Orphan.running++ > 0) return;

                ChangePriority(ThreadPriority.BelowNormal);

                SBM_OBJ_POOL[] jobs = null;

                using (var dbHelper = new DbHelper())
                {
                    jobs = dbHelper.GetRunnings();
                }

                foreach (var job in jobs)
                {
                    if (stop) break;

                    //var process = System.Diagnostics.Process.GetProcessById(int.Parse(job.PID));

                    bool isOrphan = false;

                    lock (base.Core.SyncList)
                    {
                        isOrphan = !Core.Running.ContainsKey(job.ID_DISPATCHER);
                    }

                    if (isOrphan)
                    {
                        //if ( process != null && process.ProcessName.StartsWith("SBM.Wrapper") )
                        //{
                        //    process.Kill();
                        //}

                        using (var dbHelper = new DbHelper())
                        {
                            if (dbHelper.GetRunning(job.ID_DISPATCHER) != null)
                            {
                                var dispatcher = dbHelper.GetDispatcher(job.ID_DISPATCHER);

                                if (dispatcher == null)
                                {
                                    dbHelper.DeleteObjPool(job.ID_DISPATCHER);
                                }
                                else
                                {
                                    dbHelper.SaveOrInsert(new SBM_DONE()
                                    {
                                        ID_DISPATCHER = job.ID_DISPATCHER,
                                        ID_SERVICE = job.ID_SERVICE,
                                        ENDED = DateTimeOffset.UtcNow,
                                        ID_DONE_STATUS = Consts.STATUS_FATAL_ERROR,
                                        RESULT = "Orphan process",
                                        //ID_OWNER = dispatcher.ID_OWNER, 
                                        //PARAMETERS = dispatcher.PARAMETERS,
                                        //REQUESTED = dispatcher.REQUESTED,
                                        //ID_PRIVATE = dispatcher.ID_PRIVATE
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Orphan.Beat]", e);
            }
            finally
            {
                Orphan.running = 0;
            }
        }
        public override void Dispose()
        {
        }
    }
}
