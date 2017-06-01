using SBM.Common;
using System;
using System.Net;
using System.Text;
using System.Threading;

namespace SBM.Service
{
    public class BatchHandler : MarshalByRefObject
    {
        /// <summary>
        /// Process ID
        /// </summary>
        public string PID { get; protected set; }

        /// <summary>
        /// Events Args
        /// </summary>
        public BatchEventArgs BatchEventArgs { get; set; }

        /// <summary>
        /// FullName
        /// </summary>
        protected String FullName { get; set; }

        /// <summary>
        /// Class
        /// </summary>
        protected String Type { get; set; }

        /// <summary>
        /// Batch implemetation
        /// </summary>
        private Batch Batch { get; set; }

        /// <summary>
        /// Cancel indicator
        /// </summary>
        private bool IsCanceled { get; set; } = false;

        public BatchHandler()
        {
            AppDomain.MonitoringIsEnabled = true;
        }

        public BatchHandler Initialize(string fullName, string type)
        {
            this.FullName = fullName;
            this.Type = type;

            return this;
        }

        protected void CheckPriority()
        {
            try
            {
                if (Thread.CurrentThread.Priority !=
                    ThreadPriority.AboveNormal)
                {
                    Thread.CurrentThread.Priority =
                        ThreadPriority.AboveNormal;
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BatchHandler.CheckPriority] ", e);
            }
        }

        /// <summary>
        /// Internal use
        /// </summary>
        public void SubmitHandled(object state)
        {
            var resetEvent = state as AutoResetEvent;

            this.CheckPriority();

            this.BatchEventArgs.StartedDatetime = DateTimeOffset.UtcNow;
            this.BatchEventArgs.ThreadName = Thread.CurrentThread.Name;

            try
            {
                Log.Debug("SBM.Service [BatchHandler.SubmitHandled] Submit " + this.BatchEventArgs.Parameters);

                this.BatchEventArgs.Result = this.Submit();
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BatchHandler.SubmitHandled] Submit", e);

                this.BatchEventArgs.Exception = e;
            }
            finally
            {
                this.BatchEventArgs.FinishedDatetime = DateTimeOffset.UtcNow;
            }

            if (resetEvent != null)
            {
                try
                {
                    Log.Debug("SBM.Service [BatchHandler.SubmitHandled] Submit.Reset");

                    resetEvent.Set();
                }
                catch (Exception)
                {
                    //Log.Write("SBM.Service [BatchHandler.SubmitHandled] Submit.Reset", e);
                }
            }
        }

        public virtual string Submit()
        {
            User user = null;

            try
            {
                if (!string.IsNullOrEmpty(this.BatchEventArgs.UserName) && this.BatchEventArgs.Password != null)
                {
                    Log.Debug("SBM.Service [BatchHandler.Sumbit] : Login " + this.BatchEventArgs.UserName);

                    var password = Encoding.UTF8.GetString(this.BatchEventArgs.Password);

                    user = new User(new NetworkCredential(
                        this.BatchEventArgs.UserName, password, this.BatchEventArgs.Domain));
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BatchHandler.Sumbit] ", e);

                throw;
            }

            try
            {
                if (this.Batch == null)
                {
                    Log.Debug("SBM.Service [BatchHandler.Sumbit] : Create " + this.Type + " on " + this.FullName);

                    this.Batch = (Batch)AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(this.FullName, this.Type);
                }

                this.Batch.Context = new BatchContext()
                {
                    PARAMETERS = this.BatchEventArgs.Parameters,
                    ID_PRIVATE = this.BatchEventArgs.Private,
                    ID_OWNER = this.BatchEventArgs.Owner,
                    ID_SERVICE = this.BatchEventArgs.Service,
                    ID_DISPATCHER = this.BatchEventArgs.Dispatcher,
                    DOMAIN = this.BatchEventArgs.Domain,
                    USERNAME = this.BatchEventArgs.UserName,
                    PASSWORD = this.BatchEventArgs.Password,
                    TIMEOUT = this.BatchEventArgs.Timeout
                };

                Log.Debug("SBM.Service [BatchHandler.Sumbit] : Init");

                this.Batch.Init();

                Log.Debug("SBM.Service [BatchHandler.Sumbit] : Submit!");

                var readed = this.Batch.Read();

                while (readed != null && !this.IsCanceled)
                {
                    var processed = this.Batch.Process(readed);

                    if (processed == null || this.IsCanceled) break;

                    this.Batch.Write(processed);

                    if (this.IsCanceled) break;

                    readed = this.Batch.Read();
                }

                this.Batch.Destroy();

                return this.Batch.Context.RESPONSE;
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BatchHandler.Sumbit] ", e);

                throw;
            }
            finally
            {
                if (user != null)
                {
                    user.Dispose();
                }
            }
        }

        public virtual void Cancel(object state)
        {
            this.IsCanceled = true;
        }

        public virtual void Close()
        { 
        }
    }
}
