using SBM.Schedule;
using System;

namespace SBM.Service
{
    internal sealed class Cron
    {
        public object CrontabSchedule { get; private set; }

        /// <summary>
        /// Next Run
        /// </summary>
        /// <param name="chron">
        /// 
        /// * * * * *
        /// - - - - -
        /// | | | | |
        /// | | | | +---- day of week (0-6) (sunday=0)
        /// | | | +------ month (1-12)
        /// | | +-------- day of month (1-31)
        /// | +---------- hour (0-23)
        /// +------------ min (0-59)
        /// 
        /// </param>
        /// <returns>Next Run</returns>
        public DateTimeOffset Next(string chron)
        {
            var schedule = Schedule.Scheduler.Parse(chron);

            var next = schedule.GetNextOccurrence(DateTimeOffset.UtcNow);

            Log.Debug(string.Format("SBM.Service [Cron.Next] {0} -> {1}", chron, next.ToString("n")));

            return next;
        }
    }
}
