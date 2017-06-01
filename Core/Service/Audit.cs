using SBM.Component;
using SBM.Model;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Linq;

namespace SBM.Service
{
    internal class Audit : Healthy
    {
        private PerformanceCounter cpuPerformanceCounter = new PerformanceCounter();
        private PerformanceCounter memoryPerformanceCounter = new PerformanceCounter();

        private PerformanceCounter diskReadPerformanceCounter = new PerformanceCounter();
        private PerformanceCounter diskWrittenPerformanceCounter = new PerformanceCounter();
        private PerformanceCounter diskTransfersPerformanceCounter = new PerformanceCounter();

        private PerformanceCounter[] networkReceivedPerformanceCounter;
        private PerformanceCounter[] networkSentPerformanceCounter;
        private PerformanceCounter[] networkTransfersPerformanceCounter;

        private String[] networkInstanceName;

        private Timer timer = null;

        public Audit()
            : base()
        {
            try
            {
                this.cpuPerformanceCounter.CategoryName = "Processor";
                this.cpuPerformanceCounter.CounterName = "% Processor Time";
                this.cpuPerformanceCounter.InstanceName = "_Total";

                this.memoryPerformanceCounter.CategoryName = "Memory";
                this.memoryPerformanceCounter.CounterName = "Available MBytes";

                this.diskReadPerformanceCounter.CategoryName = "PhysicalDisk";
                this.diskReadPerformanceCounter.CounterName = "Disk Reads/sec";
                this.diskReadPerformanceCounter.InstanceName = "_Total";

                this.diskWrittenPerformanceCounter.CategoryName = "PhysicalDisk";
                this.diskWrittenPerformanceCounter.CounterName = "Disk Writes/sec";
                this.diskWrittenPerformanceCounter.InstanceName = "_Total";

                this.diskTransfersPerformanceCounter.CategoryName = "PhysicalDisk";
                this.diskTransfersPerformanceCounter.CounterName = "Disk Transfers/sec";
                this.diskTransfersPerformanceCounter.InstanceName = "_Total";

                this.networkInstanceName = new PerformanceCounterCategory("Network Interface").GetInstanceNames();

                this.networkReceivedPerformanceCounter = new PerformanceCounter[this.networkInstanceName.Length];
                this.networkSentPerformanceCounter = new PerformanceCounter[this.networkInstanceName.Length];
                this.networkTransfersPerformanceCounter = new PerformanceCounter[this.networkInstanceName.Length];

                for (int i = 0; i < this.networkInstanceName.Length; i++)
                {
                    this.networkReceivedPerformanceCounter[i] = new PerformanceCounter();
                    this.networkReceivedPerformanceCounter[i].CategoryName = "Network Interface";
                    this.networkReceivedPerformanceCounter[i].CounterName = "Bytes Received/sec";
                    this.networkReceivedPerformanceCounter[i].InstanceName = this.networkInstanceName[i];

                    this.networkSentPerformanceCounter[i] = new PerformanceCounter();
                    this.networkSentPerformanceCounter[i].CategoryName = "Network Interface";
                    this.networkSentPerformanceCounter[i].CounterName = "Bytes Sent/sec";
                    this.networkSentPerformanceCounter[i].InstanceName = this.networkInstanceName[i];

                    this.networkTransfersPerformanceCounter[i] = new PerformanceCounter();
                    this.networkTransfersPerformanceCounter[i].CategoryName = "Network Interface";
                    this.networkTransfersPerformanceCounter[i].CounterName = "Bytes Total/sec";
                    this.networkTransfersPerformanceCounter[i].InstanceName = this.networkInstanceName[i];
                }

                if (Config.SBM_AUDIT_HEALTH_SECS > 0)
                {
                    this.timer = new Timer(new TimerCallback(Sample), null,
                        TimeSpan.FromSeconds(Config.SBM_AUDIT_HEALTH_SECS),
                        TimeSpan.FromSeconds(Config.SBM_AUDIT_HEALTH_SECS));
                }

            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Audit.Ctor]", e);
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public void Sample(object state)
        {
            try
            {
                ChangePriority(ThreadPriority.BelowNormal);

                string currentCpuUsage = "CPU_PERCENT=" + this.cpuPerformanceCounter.NextValue().ToString();
                string currentMemoryFree = "FREE_MEMORY_MB=" + this.memoryPerformanceCounter.NextValue().ToString();

                string currentDiskReads = "DISK_READ_SEC=" + this.diskReadPerformanceCounter.NextValue().ToString();
                string currentDiskWrites = "DISK_WRITE_SEC=" + this.diskWrittenPerformanceCounter.NextValue().ToString();
                string currentDiskTransfers = "DISK_TRANSFER_SEC=" + this.diskTransfersPerformanceCounter.NextValue().ToString();

                float networkReceived = 0;
                float networkSent = 0;
                float networkTransfer = 0;
                for (int i = 0; i < this.networkInstanceName.Length; i++)
                {
                    networkReceived += this.networkReceivedPerformanceCounter[i].NextValue();
                    networkSent += this.networkSentPerformanceCounter[i].NextValue();
                    networkTransfer += this.networkTransfersPerformanceCounter[i].NextValue();
                }
                string currentNetworkReceived = "NET_RECEIVED_SEC=" + networkReceived.ToString();
                string currentNetworkSent = "NET_SENT_SEC=" + networkSent.ToString();
                string currentNetworkTransfers = "NET_TRANSFER_SEC:" + networkTransfer.ToString();

                using (var dbHelper = new DbHelper())
                {
                    dbHelper.AddEventLog(new SBM_EVENT_LOG()
                    {
                        ID_EVENT = Consts.LOG_HEALTHY,
                        DESCRIPTION = "Health (" +
                            currentCpuUsage + "," +
                            currentMemoryFree + "," +
                            currentDiskReads + "," +
                            currentDiskWrites + "," +
                            currentNetworkReceived + "," +
                            currentNetworkSent + ")"
                    });
                    //currentDiskTransfers
                    //networkTransfer
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Audit.Sample]", e);
            }
        }

        public override void Stop()
        {
            Log.Debug("SBM.Service [Audit.Stop]");

            if (timer != null) timer.Change(0, 0);
        }

        public override void Defunct()
        {
            Log.Debug("SBM.Service [Aduit.Defunct]");

            if (timer != null) timer.Change(0, 0);
        }

        public override void Dispose()
        {
            try
            {
                if (cpuPerformanceCounter != null)
                {
                    cpuPerformanceCounter.Dispose();
                }

                if (memoryPerformanceCounter != null)
                {
                    memoryPerformanceCounter.Dispose();
                }

                if (diskReadPerformanceCounter != null)
                {
                    diskReadPerformanceCounter.Dispose();
                }

                if (diskWrittenPerformanceCounter != null)
                {
                    diskWrittenPerformanceCounter.Dispose();
                }

                if (diskTransfersPerformanceCounter != null)
                {
                    diskTransfersPerformanceCounter.Dispose();
                }

                if (networkReceivedPerformanceCounter != null)
                {
                    networkReceivedPerformanceCounter.ToList().ForEach(p => p.Dispose());
                }

                if (networkSentPerformanceCounter != null)
                {
                    networkSentPerformanceCounter.ToList().ForEach(p => p.Dispose());
                }

                if (networkTransfersPerformanceCounter != null)
                {
                    networkTransfersPerformanceCounter.ToList().ForEach(p => p.Dispose());
                }

                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
