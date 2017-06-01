using SBM.Service;
using System;

namespace SBM.Wrapper
{
    public class ActionSubmit : Action
    {
        //private ManualResetEvent manual = new ManualResetEvent(false);

        public ActionSubmit(Request request, Response response)
            : base(request, response)
        {
            string assemblyName = typeof(BatchHandler).Assembly.FullName;
            string typeName = typeof(BatchHandler).FullName;

            Action.Batch = (BatchHandler)
                Action.AppDomain.CreateInstanceAndUnwrap(assemblyName, typeName);

            Action.Batch.Initialize(base.Request.FileFullName, base.Request.Class);
        }

        public override void Execute()
        {
            try
            {
                //ThreadPool.QueueUserWorkItem(new WaitCallback(Join));

                Log.Debug("SBM.Wrapper [ActionSubmit.Execute] Request : " + base.Request.Raw);

                Action.Batch.BatchEventArgs = new BatchEventArgs(
                    Request.Dispatcher,
                    Request.x86,
                    Request.Parameters,
                    Request.Domain,
                    Request.User,
                    Request.Password,
                    Request.Private,
                    Request.Owner,
                    Request.Service,
                    Request.Timeout);

                var aux = Action.Batch.Submit();

                Log.Debug("SBM.Wrapper [ActionSubmit.Execute] Response : " + aux);

                base.Response.SetValue(aux);
            }
            catch (Exception e)
            {
                base.Response.SetException(e);
            }
            //finally
            //{
            //    manual.Set();
            //}
        }

        //private void Join(object state)
        //{
        //    Log.Debug("SBM.Wrapper [ActionSubmit.Join] : Wait");

        //    ILease lease = (ILease)Program.Batch.InitializeLifetimeService();

        //    while (!manual.WaitOne(TimeSpan.FromMinutes(1)))
        //    {
        //        //Keep Alive
        //        lease.Renew(TimeSpan.FromMinutes(5));
        //    }

        //    Log.Debug("SBM.Wrapper [ActionSubmit.Join] : Release");
        //}
    }
}
