using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace SBM.Wrapper
{
    internal class Log
    {
        private static object syncObject = new object();

        private static void WriteOnFile(object state)
        {
            string message = state as string;

            string sufix = Environment.Is64BitProcess ? "64" : "32";

            string path1 = Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "SBM.Wrapper" + sufix + " .log");

            string path2 = Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "SBM.Wrapper" + sufix + " .log.old");

            lock (syncObject)
            {

                var file1 = new FileInfo(path1);
                var file2 = new FileInfo(path2);

                var configSize = Convert.ToInt64(ConfigurationManager.AppSettings.Get("SBM_LOG_SIZE") ?? "8");

                try
                {
                    if (file1.Exists && file1.Length > configSize * 1024L * 1024L)
                    {
                        if (file2.Exists) file2.Delete();
                        file1.MoveTo(path2);
                    }
                }
                catch (Exception)
                {
                    //do nothing
                }
                try
                {
                    File.AppendAllText(path1, message, Encoding.UTF8);
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        public static void Write(string message)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(WriteOnFile),
                string.Format("{0}{1} {2}", Environment.NewLine, DateTime.Now.ToString("HH:mm:ss.fff"), message));
        }

        public static void Debug(string message)
        {
            Trace.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString("HH:mm:ss.fff"), message));
        }

        public static void Write(string message, Exception e)
        {
            Debug(string.Format("{0} : {1}", message, e.Message));
            Write(string.Format("{0} : {1}", message, e.Message));

            FusionLog(message, e);
            //LogEntityValidation(message, e);

            Exception inner = e.InnerException;
            while (inner != null)
            {
                Debug(string.Format("{0} : {1}", message, inner.Message));
                Write(string.Format("{0} : {1}", message, inner.Message));

                FusionLog(message, e);
                //LogEntityValidation(message, e);

                inner = inner.InnerException;
            }
        }

        //private static void LogEntityValidation(string message, Exception e)
        //{
        //    var e1 = e as DbEntityValidationException;

        //    if (e1 != null)
        //    {
        //        foreach (var msg in e1.EntityValidationErrors)
        //        {
        //            Debug(string.Format("{0} : {1} {2}", message, msg.Entry.Entity.GetType().Name, msg.Entry.State));
        //            Write(string.Format("{0} : {1} {2}", message, msg.Entry.Entity.GetType().Name, msg.Entry.State));

        //            foreach (var m in msg.ValidationErrors)
        //            {
        //                Debug(string.Format("{0} : {1} {2}", message, m.PropertyName, m.ErrorMessage));
        //                Write(string.Format("{0} : {1} {2}", message, m.PropertyName, m.ErrorMessage));
        //            }
        //        }
        //    }
        //}

        private static void FusionLog(string message, Exception e)
        {
            string fusionLog = null;

            var e1 = e as FileNotFoundException;
            var e2 = e as FileLoadException;
            var e3 = e as BadImageFormatException;

            if (e1 != null)
            {
                fusionLog = e1.FusionLog;
            }
            else if (e2 != null)
            {
                fusionLog = e2.FusionLog;
            }
            else if (e3 != null)
            {
                fusionLog = e3.FusionLog;
            }

            if (fusionLog != null)
            {
                foreach (string msg in fusionLog.Split('\n'))
                {
                    Debug(string.Format("{0} : {1}", message, msg));
                    Write(string.Format("{0} : {1}", message, msg));
                }
            }
        }
    }
}
