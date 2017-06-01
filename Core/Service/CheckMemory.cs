using SBM.Component;
using SBM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SBM.Service
{
    public class CheckMemory : Check
    {
        public CheckMemory(SBM_DISPATCHER dispatcher)
            : base(dispatcher)
        {
        }

        protected override bool IsValid()
        {
            var memStatus = new NativeMethods.MEMORYSTATUSEX();
            NativeMethods.GlobalMemoryStatusEx(memStatus);

            if (memStatus.ullAvailPhys < (Convert.ToUInt64(Config.SBM_MIN_MEMORY) * 1024ul * 1024ul))
            {
                var availableMb = (memStatus.ullAvailPhys / 1024ul) / 1024ul;

                base.Step = "not enough physical memory to " + base.dispatcher.SBM_SERVICE.DESCRIPTION + ", current " + availableMb + "MB, min " + Config.SBM_MIN_MEMORY + "MB";
                Log.Debug("SBM.Service [CheckMemory.IsValid] " + base.Step + dispatcher.SBM_SERVICE.DESCRIPTION);

                using (var dbHelper = new DbHelper())
                {
                    dbHelper.SaveOrInsert(new SBM_DONE()
                    {
                        ID_DISPATCHER = dispatcher.ID_DISPATCHER,
                        ENDED = DateTimeOffset.UtcNow,
                        ID_DONE_STATUS = Consts.STATUS_NOT_ENOUGH_RESOURCE,
                        RESULT = "Not enough physical memory"
                    });

                    dbHelper.AddEventLog(new SBM_EVENT_LOG()
                    {
                        ID_EVENT = Consts.LOG_APPLICATION_POOL_FULL,
                        DESCRIPTION = "Not enough physical memory to ID " + dispatcher.ID_DISPATCHER.ToString() + " " + dispatcher.SBM_SERVICE.DESCRIPTION + ". Current " + availableMb + "MB, min " + Config.SBM_MIN_MEMORY + "MB"
                    });
                }
                ThreadPool.QueueUserWorkItem(new WaitCallback(Dump));

                return false;
            }
            return true;
        }

        public static void Dump(object state)
        {
            var processes = new SortedList<long, string>();
            foreach (var p in System.Diagnostics.Process.GetProcesses())
            {
                try
                {
                    if (p.ProcessName != "Idle" && !p.HasExited)
                    {
                        processes.Add(p.WorkingSet64,
                            string.Format("{0,6} {1,-50} {2,10} {3}",
                                p.Id,
                                p.ProcessName,
                                p.WorkingSet64,
                                p.TotalProcessorTime.ToString(@"hh\:mm\:ss")));
                    }
                }
                catch { }
            }

            Log.WriteSync("=== PROCESS DUMP ===");
            Log.WriteSync("PID    Process                                            WorkingSet Processor");

            processes.Reverse().ToList().ForEach(p => Log.WriteSync(p.Value));

            Log.WriteSync("=== PROCESS DUMP ===");
        }
    }
}
