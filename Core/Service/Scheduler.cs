using SBM.Component;
using SBM.Model;
using System;
using System.Threading;

namespace SBM.Service
{
    internal class Scheduler : Healthy
    {
        private static volatile short running = 0;

        public Scheduler()
            : base()
        {
        }

        public override void Beat(object state)
        {

#if TRACE_BEAT
            Log.Write("SBM.Service [Scheduler.Beat]");
#endif
            try
            {
                if (Scheduler.running++ > 0) return;

                ChangePriority(ThreadPriority.Normal);

                using (var dbHelper = new DbHelper())
                {
                    //recupera los jobs agendados
                    foreach (var job in dbHelper.GetScheduled())
                    {
                        bool too_late = job.NEXT_TIME_RUN.Value.AddMinutes(Config.SBM_ACCEPTED_DELAY) < DateTimeOffset.UtcNow;

                        if (!string.IsNullOrWhiteSpace(job.CRONTAB))
                        {
                            var schedule = Schedule.Scheduler.TryParse(job.CRONTAB);

                            if (schedule.HasValue)
                            {
                                int tries = 0;
                                while (job.NEXT_TIME_RUN < DateTimeOffset.UtcNow)
                                {
                                    job.NEXT_TIME_RUN = schedule.Value.GetNextOccurrence(
                                        job.NEXT_TIME_RUN.GetValueOrDefault(DateTimeOffset.UtcNow));

                                    if (++tries == Int32.MaxValue) break;
                                }

                                if (tries == Int32.MaxValue)
                                {
                                    Log.WriteAsync("SBM.Service [Scheduler.Beat] Couldn't determine next run " + job.CRONTAB);
                                }
                            }
                            else
                            {
                                Log.WriteAsync("SBM.Service [Scheduler.Beat] " + job.CRONTAB, schedule.Error);
                            }
                        }
                        else if (job.RUN_INTERVAL != 0)
                        {
                            int tries = 0;
                            while (job.NEXT_TIME_RUN < DateTimeOffset.UtcNow)
                            {
                                //agenda para la siguiente ejecucion
                                job.NEXT_TIME_RUN = job.NEXT_TIME_RUN.GetValueOrDefault(DateTimeOffset.UtcNow)
                                    .Add(TimeSpan.FromMinutes(job.RUN_INTERVAL));

                                if (++tries == Int32.MaxValue) break;
                            }

                            if (tries == Int32.MaxValue)
                            {
                                Log.WriteAsync("SBM.Service [Scheduler.Beat] Couldn't determine next run");
                            }
                        }
                        else
                        {
                            //once
                            job.NEXT_TIME_RUN = null;
                        }

                        if (too_late)
                        {
                            dbHelper.AddEventLog(new SBM_EVENT_LOG()
                            {
                                ID_EVENT = Consts.LOG_AUDIT,
                                DESCRIPTION = "Delayed start from Schedule " + job.SBM_SERVICE.DESCRIPTION +
                                        " (ID_SERVICE=" + job.ID_SERVICE + ", " +
                                        " PARAMETERS=" + job.PARAMETERS + ", " +
                                        " ID_SERVICE =" + job.ID_SERVICE + ", " +
                                        " ID_PRIVATE =" + job.ID_PRIVATE + ", " +
                                        " ID_OWNER=" + job.ID_OWNER + ")"
                            });
                        }
                        else
                        {
                            job.LAST_TIME_RUN = DateTimeOffset.UtcNow;

                            //crea el dispatcher
                            var dispatcher = new SBM_DISPATCHER()
                            {
                                ID_OWNER = job.ID_OWNER,
                                ID_PRIVATE = job.ID_PRIVATE,
                                ID_SERVICE = job.ID_SERVICE,
                                PARAMETERS = job.PARAMETERS,
                                REQUESTED = job.LAST_TIME_RUN
                            };

                            dbHelper.Insert(dispatcher);

                            dbHelper.AddEventLog(new SBM_EVENT_LOG()
                            {
                                ID_EVENT = Consts.LOG_SERVICE_SCHEDULED_QUEUE,
                                DESCRIPTION = "Enqueue from Schedule " + job.SBM_SERVICE.DESCRIPTION +
                                    " (ID_DISPATCHER=" + dispatcher.ID_DISPATCHER + ", " +
                                    " ID_SERVICE=" + job.ID_SERVICE + ", " +
                                    " PARAMETERS=" + job.PARAMETERS + ", " +
                                    " ID_SERVICE =" + job.ID_SERVICE + ", " +
                                    " ID_PRIVATE =" + job.ID_PRIVATE + ", " +
                                    " ID_OWNER=" + job.ID_OWNER + ")"
                            });
                        }

                        dbHelper.Save(job);
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Scheduler.Beat] ", e);
            }
            finally
            {
                Scheduler.running = 0;
            }
        }
        public override void Dispose()
        {
        }
    }
}
