using SBM.Component;
using SBM.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SBM.Service
{
    internal class Meter : Healthy
    {
        private const string CATEGORY_NAME = "SBM";
        private const string CATEGORY_HELP = "Simple Batch Manager meters";

        private const string DISPATCHER_NAME = "dispatcher"; //please lower case

        private const string PROCESSOR = "% Processor Time";
        private const string MEMORY = "Memory Usage in Megabytes";

        private PerformanceCounterCategory category;

        private ConcurrentDictionary<String, Tuple<DateTimeOffset[], TimeSpan[], PerformanceCounter, PerformanceCounter>> performanceCounters =
            new ConcurrentDictionary<String, Tuple<DateTimeOffset[], TimeSpan[], PerformanceCounter, PerformanceCounter>>();

        private Timer timerCPU = null;
        private Timer timerMemory = null;

        public Meter()
            : base()
        {
            try
            {
                AppDomain.MonitoringIsEnabled = true;

                if (PerformanceCounterCategory.Exists(CATEGORY_NAME))
                {
                    category = new PerformanceCounterCategory(CATEGORY_NAME);
                }
                else
                {
                    //crea los counter
                    var counterDataCollection = new CounterCreationDataCollection();
                    counterDataCollection.Add(new CounterCreationData(PROCESSOR, PROCESSOR, PerformanceCounterType.NumberOfItems32));
                    counterDataCollection.Add(new CounterCreationData(MEMORY, MEMORY, PerformanceCounterType.NumberOfItems32));

                    category = PerformanceCounterCategory.Create(CATEGORY_NAME, CATEGORY_HELP,
                        PerformanceCounterCategoryType.MultiInstance, counterDataCollection);
                }

                //borra las instancias que no existen
                foreach (String name in category.GetInstanceNames())
                {
                    bool exist = false;
                    using (var dbHelper = new DbHelper())
                    {
                        exist = dbHelper.ServiceExists(name);
                    }

                    if (name != DISPATCHER_NAME && !exist)
                    {
                        new PerformanceCounter(CATEGORY_NAME, PROCESSOR, name, false).RemoveInstance();
                        new PerformanceCounter(CATEGORY_NAME, MEMORY, name, false).RemoveInstance();
                    }
                }

                //crea los counter
                if (!performanceCounters.ContainsKey(DISPATCHER_NAME))
                {
                    performanceCounters.TryAdd(DISPATCHER_NAME,
                        new Tuple<DateTimeOffset[], TimeSpan[], PerformanceCounter, PerformanceCounter>(
                            new DateTimeOffset[1] { DateTimeOffset.UtcNow },
                            new TimeSpan[1] { TimeSpan.Zero },
                            new PerformanceCounter(CATEGORY_NAME, PROCESSOR, DISPATCHER_NAME, false),
                            new PerformanceCounter(CATEGORY_NAME, MEMORY, DISPATCHER_NAME, false)));
                }
                SBM_SERVICE[] services = null;
                using (var dbHelper = new DbHelper())
                {
                    services = dbHelper.GetServices();
                }
                foreach (var service in services)
                {
                    if (!performanceCounters.ContainsKey(service.DESCRIPTION.ToLower()))
                    {
                        performanceCounters.TryAdd(service.DESCRIPTION.ToLower(),
                            new Tuple<DateTimeOffset[], TimeSpan[], PerformanceCounter, PerformanceCounter>(
                                new DateTimeOffset[1] { DateTimeOffset.MinValue },
                                new TimeSpan[1] { TimeSpan.Zero },
                                new PerformanceCounter(CATEGORY_NAME, PROCESSOR, service.DESCRIPTION.ToLower(), false),
                                new PerformanceCounter(CATEGORY_NAME, MEMORY, service.DESCRIPTION.ToLower(), false)));
                    }
                }

                timerCPU = new Timer(new TimerCallback(SampleCPU), null,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1));

                timerMemory = new Timer(new TimerCallback(SampleMemory), null,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1));
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Meter.Ctor]", e);
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void SampleCPU(object state)
        {
            ChangePriority(ThreadPriority.BelowNormal);

            Tuple<DateTimeOffset[], TimeSpan[], PerformanceCounter, PerformanceCounter> counter = null;

            Process[] running;
            lock (base.Core.SyncList)
            {
                running = new Process[base.Core.Running.Values.Count];
                base.Core.Running.Values.CopyTo(running, 0);
            }

            var stoped = new HashSet<String>(performanceCounters.Keys);

            if (performanceCounters.TryGetValue(DISPATCHER_NAME, out counter))
            {
                stoped.Remove(DISPATCHER_NAME);

                var now = DateTimeOffset.UtcNow;
                var delta = now.Subtract(counter.Item1[0]);
                counter.Item1[0] = now;

                var used = AppDomain.CurrentDomain.MonitoringTotalProcessorTime;
                var cpu = used.Subtract(counter.Item2[0]);
                counter.Item2[0] = used;

                if (delta.TotalMilliseconds > 0)
                {
                    counter.Item3.RawValue = Convert.ToInt64(cpu.TotalMilliseconds * 100D / delta.TotalMilliseconds);
                }
            }

            foreach (var job in running)
            {
                if (performanceCounters.TryGetValue(job.Name.ToLower(), out counter))
                {
                    stoped.Remove(job.Name.ToLower());

                    var now = DateTimeOffset.UtcNow;
                    var delta = now.Subtract(counter.Item1[0] == DateTimeOffset.MinValue ? job.Started : counter.Item1[0]);
                    counter.Item1[0] = now;

                    var used = job.MonitoringTotalProcessorTime;
                    var cpu = used.Subtract(counter.Item2[0]);
                    counter.Item2[0] = used;

                    if (delta.TotalMilliseconds > 0)
                    {
                        counter.Item3.RawValue = Convert.ToInt64(cpu.TotalMilliseconds * 100D / delta.TotalMilliseconds);
                    }
                }
            }

            foreach (var job in stoped)
            {
                if (performanceCounters.TryGetValue(job, out counter))
                {
                    counter.Item3.RawValue = 0;
                }
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void SampleMemory(object state)
        {
            ChangePriority(ThreadPriority.BelowNormal);

            Tuple<DateTimeOffset[], TimeSpan[], PerformanceCounter, PerformanceCounter> counter = null;

            Process[] running;
            lock (base.Core.SyncList)
            {
                running = new Process[base.Core.Running.Values.Count];
                base.Core.Running.Values.CopyTo(running, 0);
            }

            var stoped = new HashSet<String>(performanceCounters.Keys);

            GC.CollectionCount(2);

            if (performanceCounters.TryGetValue(DISPATCHER_NAME, out counter))
            {
                stoped.Remove(DISPATCHER_NAME);

                //GC.GetTotalMemory(false)  
                counter.Item4.RawValue = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1048576L;
            }

            foreach (var job in running)
            {
                if (performanceCounters.TryGetValue(job.Name.ToLower(), out counter))
                {
                    stoped.Remove(job.Name.ToLower());

                    counter.Item4.RawValue = job.MonitoringTotalAllocatedMemorySize / 1048576L;
                }
            }

            foreach (var job in stoped)
            {
                if (performanceCounters.TryGetValue(job, out counter))
                {
                    counter.Item4.RawValue = 0;
                }
            }
        }

        public override void Stop()
        {
            Log.Debug("SBM.Service [Meter.Stop]");

            if (timerCPU != null) timerCPU.Change(0, 0);
            if (timerMemory != null) timerMemory.Change(0, 0);
        }

        public override void Defunct()
        {
            Log.Debug("SBM.Service [Audit.Defunct]");

            if (timerCPU != null) timerCPU.Change(0, 0);
            if (timerMemory != null) timerMemory.Change(0, 0);
        }

        public override void Dispose()
        {
            try
            {
                if (timerCPU != null)
                {
                    timerCPU.Dispose();
                }
                if (timerMemory != null)
                {
                    timerMemory.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
