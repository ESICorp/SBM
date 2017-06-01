using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SBM.Transfer
{
    internal class Log
    {
        public static void Write(string message)
        {
            Trace.WriteLine(string.Format("{0} Thread_{1} {2}", DateTime.Now.ToString("HH:mm:ss.FFF"), Thread.CurrentThread.ManagedThreadId,  message));
        }

        public static void Write(string message, Exception e)
        {
            Write(string.Format("{0} : {1}", message, e.Message));
            FusionLog(message, e);

            Exception inner = e.InnerException;
            while (inner != null)
            {
                Write(string.Format("{0} : {1}", message, inner.Message));

                FusionLog(message, e);

                inner = inner.InnerException;
            }
        }

        private static void FusionLog(string message, Exception e)
        {
            string fusionLog = null;

            FileNotFoundException e1 = e as FileNotFoundException;
            FileLoadException e2 = e as FileLoadException;

            if (e1 != null)
            {
                fusionLog = e1.FusionLog;
            }
            else if (e2 != null)
            {
                fusionLog = e2.FusionLog;
            }

            if (fusionLog != null)
            {
                foreach (string msg in fusionLog.Split('\n'))
                {
                    Write(string.Format("{0} : {1}", message, msg));
                }
            }
        }
    }
}
