using System;
using System.Threading;

namespace SBM.Service
{
    internal abstract class Healthy : IDisposable
    {
        /// <summary>
        /// Core
        /// </summary>
        protected Core Core { get; private set; }

        /// <summary>
        /// stop indicator
        /// </summary>
        protected volatile bool stop = false;

        /// <summary>
        /// Ctor
        /// </summary>
        public Healthy()
        {
            this.Core = Core.GetInstance();
        }

        public virtual void Beat(object state)
        {
            //please override
        }

        public virtual void Stop()
        {
            stop = true;

            //override if necessary
        }

        public virtual void Defunct()
        {
            //override if necessary
        }

        protected void ChangePriority(ThreadPriority priority)
        {
            try
            {
                if (Thread.CurrentThread.Priority != priority)
                {
                    Thread.CurrentThread.Priority = priority;
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Healthy.ChangePriority]", e);
            }
        }

        public abstract void Dispose();
    }
}
