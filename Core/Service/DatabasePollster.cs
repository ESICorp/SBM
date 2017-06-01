using SBM.Component;
using System;
using System.Threading;

namespace SBM.Service
{
    internal class DatabasePollster : Healthy
    {
        private static volatile short running = 0;
        private static DateTime? firstFail = null;
        private static bool shutting = false;

        public DatabasePollster()
        {
        }

        public override void Beat(object state)
        {
            try
            {
                if (DatabasePollster.running++ > 0 || shutting || Config.SBM_BEFORE_SHUTTING == 0) return;

                ChangePriority(ThreadPriority.Normal);

                TimeSpan diff = DateTime.Now - (firstFail ?? DateTime.Now);

                using (var dbHelper = new DbHelper())
                {
                    if (dbHelper.IsAlive)
                    {
                        firstFail = null;
                    }
                    else if (diff.TotalMinutes < Config.SBM_BEFORE_SHUTTING)
                    {
                        if (firstFail == null)
                        {
                            firstFail = DateTime.Now;

                            Log.WriteAsync("SBM.Service [DatabasePollster.Beat] Couldn't connect to database.");
                        }
                    }
                    else
                    {
                        Log.WriteAsync("SBM.Service [DatabasePollster.Beat] Shutdown service");

                        shutting = true;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(Bootstrap.Shutdown));
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [DatabasePollster.Beat]", e);
            }
            finally
            {
                DatabasePollster.running = 0;
            }
        }

        public override void Dispose()
        {
        }
    }
}
