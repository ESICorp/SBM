using SBM.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Lifetime;
using System.Threading;

namespace SBM.Service
{
    /// <summary>
    /// Process in separate memory
    /// </summary>
    /// <remarks>
    /// Don't forget Dispose o Using please
    /// </remarks>
    /// <example>
    /// <code>
    ///     using (Process p = new Process(processName)) {
    ///
    ///     }
    /// </code>
    /// </example>
    public class Process : IDisposable
    {
        /// <summary>
        /// AppDomain who contains the execution
        /// </summary>
        //private AppDomain Domain { get; set; }

        public string PID { get; set; }

        /// <summary>
        /// Info of process
        /// </summary>
        private Context Context { get; set; }

        /// <summary>
        /// List of Batch
        /// </summary>
        private BatchHandler Batch { get; set; }

        /// <summary>
        /// Dictionary with callback execution list
        /// </summary>
        private WaitCallback Running = null;

        /// <summary>
        /// Reset Event
        /// </summary>
        private AutoResetEvent ResetEvent = null;

        /// <summary>
        /// Timer timeout
        /// </summary>
        private Timer TimeoutTimer;

        /// <summary>
        /// Timeout indicator (Warning: ProcessInfo.Timeout == TimeSpan.Zero is User Canceled)
        /// </summary>
        public Boolean IsTimeout = false;

        public String Result { get; private set; }

        public IList<Problem> Exceptions { get; private set; }

        /// <summary>
        /// Create process in separate memory
        /// </summary>
        /// <param name="ProcessInfo">Info of Process</param>
        public Process(Context context)
        {
            AppDomain.MonitoringIsEnabled = true;

            this.ResetEvent = new AutoResetEvent(false);

            //conserva los parametros
            this.Context = context;

            Log.Debug("SBM.Service [Process.Ctror] " + this.Context.ProcessName);

            //
            //crea el appdomain
            //            
            //this.Domain = AppDomain.CreateDomain(
            //    this.Context.ProcessName,
            //    this.Context.Evidence,
            //    DomainSetup.GetDefaults(this.Context),
            //    Permissions.GetDefault(this.Context));

            //var handler = new DomainEventHandler(this.ProcessInfo.ProcessName);

            //para descomentar la siguiente línea, ajuste los valores del .config
            //
            // <system.runtime.remoting>
            //  <application>
            //    <lifetime leaseTime="30M"
            //              sponsorshipTimeout="10M"
            //              renewOnCallTime="15M"
            //              leaseManagerPollTime="1M" />
            //  </application>
            //</system.runtime.remoting>
            //
            // de lo contrario podría obtener el siguiente error:
            //
            //SBM.Service [DomainEventHandler.FirstChanceException] Object '/xxx/xxx.rem' has been disconnected or does not exist at the server.
            //this.Domain.FirstChanceException += new EventHandler<FirstChanceExceptionEventArgs>(handler.FirstChanceException);

            //this.Domain.UnhandledException += new UnhandledExceptionEventHandler(handler.UnhandledException);
            //this.Domain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            //
            // Load Batch
            //
            this.Batch = AssemblyHelper.LoadAssembly(this.Context);

            if (this.Batch != null)
            {
                this.PID = this.Batch.PID;
            }
        }

        public bool IsLoaded
        {
            get { return this.Batch != null; }
        }

        public string Name
        {
            get { return this.Context.ProcessName; }
        }

        public DateTimeOffset Started
        {
            get { return this.Context.Started; }
        }

        public Int32 Dispatcher
        {
            get { return this.Context.Dispatcher; }
        }

        public Int16 Service
        {
            get { return this.Context.Service; }
        }

        public Boolean SingleExec
        {
            get { return this.Context.SingleExec; }
        }

        public Int32 Timeout
        {
            get { return this.Context.Timeout; }
        }

        //private void UnhandledException(object sender, UnhandledExceptionEventArgs unhandledException)
        //{
        //    this.Batch.BatchEventArgs.Exception = unhandledException.ExceptionObject as Exception;

        //    Log.Write("SBM.Service [Process.UnhandledException]", this.Batch.BatchEventArgs.Exception);

        //    if (this.ResetEvent != null)
        //    {
        //        try
        //        {
        //            this.ResetEvent.Set();
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Write("SBM.Service [Process.UnhandledException] : Reset", e);
        //        }
        //    }
        //}

        public void Dispose()
        {
            if (this.TimeoutTimer != null)
            {
                try
                {
                    this.TimeoutTimer.Dispose();
                }
                catch { }
                this.TimeoutTimer = null;
            }

            if (this.ResetEvent != null)
            {
                try
                {
                    this.ResetEvent.Dispose();
                }
                catch { }
                this.ResetEvent = null;
            }
            if ( this.Batch != null)
            {
                try
                {
                    this.Batch.Close();
                }
                catch { }
                this.Batch = null;
            }
            //if (this.Domain != null)
            //{
            //    try
            //    {
            //        AppDomain.Unload(this.Domain);
            //    }
            //    catch { }
            //    this.Domain = null;
            //}
        }

        /// <summary>
        /// Execute asynchronic batch 
        /// </summary>
        public void ExecuteAsync()
        {
            Log.Debug("SBM.Service [Process.ExecuteAsync] " + this.Context.ProcessName);

            this.Batch.BatchEventArgs = new BatchEventArgs(
                this.Context.Dispatcher,
                this.Context.x86,
                this.Context.Parameter,
                this.Context.Domain,
                this.Context.User,
                this.Context.Password,
                this.Context.Private,
                this.Context.Owner,
                this.Context.Service,
                this.Context.Timeout);

            ThreadPool.QueueUserWorkItem(
                this.Running = new WaitCallback(this.Batch.SubmitHandled), this.ResetEvent);

            this.TimeoutTimer = new Timer(this.TimeoutEvent, null,
                TimeSpan.FromSeconds(this.Context.Timeout).Add(Consts.ThresholdTimeout),
                System.Threading.Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Join threads
        /// </summary>
        public void Join()
        {
            Log.Debug("SBM.Service [Process.Join] " + this.Context.ProcessName);

            this.Result = string.Empty;
            this.Exceptions = new List<Problem>();

            try
            {
                var lease = (ILease)this.ResetEvent.InitializeLifetimeService();

                while (!this.ResetEvent.WaitOne(Consts.ThresholdTimeout))
                {
                    lease.Renew(Consts.CommunicationTimeout);
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Process.Join] Failed WaitOne " + this.Context.ProcessName, e);
            }

            try
            {
                this.Result += this.Batch.BatchEventArgs.Result ?? string.Empty;
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Process.Join] Failed Retrive Result " + this.Context.ProcessName, e);
            }

            try
            {
                if (this.Batch.BatchEventArgs.Exception != null)
                {
                    this.Exceptions.Add(this.Batch.BatchEventArgs.Exception);
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Process.Join] Failed Retrive Exception " + this.Context.ProcessName, e);
            }
        }

        /// <summary>
        /// Timeout unload appdomain
        /// </summary>
        /// <param name="state">AppDomain</param>
        private void TimeoutEvent(object state)
        {
            try
            {
                Log.Debug("SBM.Service [Process.TimeoutEvent]");

                this.IsTimeout = true;

                if (this.ResetEvent != null)
                {
                    this.ResetEvent.Set();
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [Process.TimeoutEvent] Couldn't reset event", e);
            }
        }

        public void Cancel()
        {
            this.Batch.Cancel(null);

            this.Context.Timeout = 0;

            this.TimeoutEvent(null);
        }

        public TimeSpan MonitoringTotalProcessorTime
        {
            get
            {
                try
                {
                    //return this.Domain.MonitoringTotalProcessorTime
                    return TimeSpan.Zero;
                }
                catch
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public long MonitoringTotalAllocatedMemorySize
        {
            get
            {
                try
                {
                    //GC.CollectionCount(2);
                    //return this.Domain.MonitoringTotalAllocatedMemorySize;
                    return 0L;
                }
                catch
                {
                    return 0L;
                }
            }
        }
    }
}
