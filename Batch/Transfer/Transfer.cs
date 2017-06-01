using SBM.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace SBM.Transfer
{
    public partial class Transfer : Batch
    {
        private int Count { get; set; }

        public override void Init()
        {
            Initalize();

            //GlobalSourceTrigger(Config.Source.SQLBefore);
            GlobalTargetTrigger(Config.Target.SQLBefore);
        }

        public override object Read()
        {
            if ( Config.Items.MoveNext() )
            {
                this.Count++;

                return Config.Items.Current;
            }
            else
            {
                return null;
            }
        }

        public override object Process(object source)
        {
            var config = source as Item;

            var manualEvent = new ManualResetEvent(false);

            Transfer.ManualEvents.TryAdd(config.Name, manualEvent);

            if (Config.Parallel > 1)
            {
                if (this.Concurrents >= Config.Parallel)
                {
                    //Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    //WaitHandle.WaitAny(this.ManualEvents.ToArray());
                    WaitHandle.WaitAny(new List<WaitHandle>(Transfer.ManualEvents.Values).ToArray());
                }

                this.Concurrents++;

                ThreadPool.QueueUserWorkItem(
                    new WaitCallback(ProcessItem),
                    new ThreadParameter(config, manualEvent));
            }
            else
            {
                ProcessItem(new ThreadParameter(config, manualEvent));
            }

            return source;
        }

        public override void Destroy()
        {
            if (Transfer.ManualEvents.Count > 0)
            {
                WaitHandle.WaitAll(new List<WaitHandle>(Transfer.ManualEvents.Values).ToArray());
            }

            GlobalTargetTrigger(Config.Target.SQLAfter);
            //GlobalSourceTrigger(Config.Source.SQLAfter);

            Clean();

            RESPONSE = string.Format("{0} Tables, {1} Good, {2} Bad : {3}", 
                this.Count, this.Good, this.Bad, this.Message.ToString());
        }

        private void ProcessItem(object state)
        {
            string name = null;

            using (var param = state as ThreadParameter)
            {
                try
                {
                    name = param.ItemConfig.Name;

                    Log.Write("SBM.Transfer [Transfer.ProcessItem] " + param.ItemConfig.SourceSQL);
                    param.SourceDataReader = param.SourceCommand.ExecuteReader(CommandBehavior.SingleResult);

                    CheckTarget(param);

                    TriggerBefore(param);

                    Copy(param);

                    TriggerAfter(param);

                    lock (SyncObject)
                    {
                        this.Rows += param.Rows;
                        this.Message.AppendFormat("{0}:{1}|", param.ItemConfig.Name, param.Rows);
                    }

                    this.Good++;
                }
                catch (Exception e)
                {
                    Log.Write(string.Format("SBM.Transfer [Transfer.ProcessItem] Couldn't {0}", param.Step), e);

                    lock (SyncObject)
                    {
                        this.Message.AppendFormat("{0}:{1}|", param.Step, e.Message);
                    }

                    this.Bad++;
                }
            }

            this.Concurrents--;

            ManualResetEvent removed = null;

            if (Transfer.ManualEvents.TryRemove(name, out removed))
            {
                removed.Dispose();
            }
        }

#if FALSE
        public static void Main(string[] args)
        {
            new Transfer().Submit(string.Empty);
        }
#endif
    }
}
