using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SBM.Service
{
    internal class Health : Healthy
    {
        private List<Healthy> beatMembers = null;
        private List<Healthy> noBeatMembers = null;

        public Health()
            : base()
        {
            try
            {
                this.beatMembers = new List<Healthy>(new Healthy[] {
                    new DatabasePollster()
                    ,new Canceller()
                    ,new Scheduler()
                    ,new Launcher()
                    ,new Orphan()
                });
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Health.Ctor] Beat", e);
            }

            try
            {
                this.noBeatMembers = new List<Healthy>(new Healthy[] {
                    new Meter(),
                    new Audit()
                });
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Health.Ctor] NoBeat", e);
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public override void Beat(object state)
        {
#if TRACE_BEAT
            Log.Write("SBM.Service [Health.Beat]");
#endif
            //checkPriority();
            try
            {
                //this.beatMembers.ForEach(x => Task.Run(() => x.Beat(state)));
                this.beatMembers.ForEach(x => x.Beat(state));
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Health.Beat]", e);
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public override void Stop()
        {
            Log.Debug("SBM.Service [Health.Stop]");

            //checkPriority();
            try
            {
                this.beatMembers.ForEach(x => x.Stop());
                this.noBeatMembers.ForEach(x => x.Stop());
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Health.Stop]", e);
            }
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public override void Defunct()
        {
            Log.Debug("SBM.Service [Health.Defunct]");

            //checkPriority();
            try
            {
                this.beatMembers.ForEach(x => x.Defunct());
                this.noBeatMembers.ForEach(x => x.Defunct());
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Health.Defunct]", e);
            }
        }

        public override void Dispose()
        {
            try
            {
                this.beatMembers.ForEach(x => x.Dispose());
                this.noBeatMembers.ForEach(x => x.Dispose());
            }
            catch (Exception)
            {
            }
        }
    }
}