using SBM.Service;
using SevenZip;
using System;
using System.IO;
using System.Linq;

namespace SBM.Agent
{
    public class ActionSubmit : Action
    {
        //private ManualResetEvent manual = new ManualResetEvent(false);

        public ActionSubmit(Request request, Response response)
            : base(request, response)
        {
            base.Batch = new BatchHandlerPipe(Request.x86);
        }

        public override void Execute()
        {
            //AppDomain appDomain = null;
            try
            {
                if (!Extract()) return;

                Batch.Initialize(Request.FileFullName, Request.Class);
                
                Core.GetInstance().Running.Add(Request.Dispatcher, base.Batch);

                //ThreadPool.QueueUserWorkItem(new WaitCallback(Join), batch);
                
                Log.Debug("SBM.Agent [ActionSubmit.Execute] Request : " + Request.Raw);

                base.Batch.BatchEventArgs = new BatchEventArgs(
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

                var aux = base.Batch.Submit();

                Log.Debug("SBM.Agent [ActionSubmit.Execute] Response : " + aux);

                Response.SetValue(aux);
            }
            catch (Exception e)
            {
                Response.SetException(e);
            }
            finally
            {
                //manual.Set();

                Core.GetInstance().Running.Remove(Request.Dispatcher);
            }
        }

        //private void Join(object state)
        //{
        //    var batch = state as BatchHandler;

        //    Log.Debug("SBM.Agent [ActionSubmit.Join] : Wait");

        //    var lease = (ILease)batch.InitializeLifetimeService();

        //    while (!manual.WaitOne(TimeSpan.FromMinutes(1)))
        //    {
        //        //Keep Alive
        //        lease.Renew(TimeSpan.FromMinutes(5));
        //    }

        //    Log.Debug("SBM.Agent [ActionSubmit.Join] : Release");
        //}

        private bool Extract()
        {
            try
            {
                var fileInfo = new FileInfo(Request.FileFullName);
                if (fileInfo.Exists && fileInfo.Directory.GetFiles().Count() > 0 ) 
                {
                    var local = fileInfo.Directory.GetFiles().Max(_ => _.LastWriteTimeUtc);
                    if (Request.Last <= local)
                    {
                        return true;
                    }
                }

                if (fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.GetFiles().ToList().ForEach(_ => _.Delete());
                }
                else
                {
                    fileInfo.Directory.Create();
                }

                using (var m = new MemoryStream(Convert.FromBase64String(Request.Zip)))
                {
                    SevenZipExtractor.SetLibraryPath(
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z64.dll"));

                    using (var extract = new SevenZipExtractor(m))
                    {
                        extract.ArchiveFileNames.ToList().ForEach(_ => extract.ExtractFiles(fileInfo.Directory.FullName, _));
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Write("SBM.Agent [ActionSubmit.Extract]" + e);

                Response.SetException(e);

                return false;
            }
        }
    }
}
