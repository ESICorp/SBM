using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SBM.Filer
{
    internal class TraceLog
    {
        public static readonly int MAX_EXCEPTIONS = 100;

        private static TraceListener listener = null;

        private static IList<Exception> exceptions = new List<Exception>();

        public static int Count { get; private set; } = 0;

        public static Exception Exceptions { get { return new AggregateException(TraceLog.exceptions); } }

        public static void Configure()
        {
            if (!string.IsNullOrEmpty(Parameter.Log))
            {
                TraceLog.listener = new TextWriterTraceListener(Parameter.Log.Wilcard());
            }
        }

        public static void AddError(string message, Exception innerException)
        {
            TraceLog.Count++;

            string content = string.Format("Filer : {0} {1}", message, innerException == null ? string.Empty : innerException.Message);

            Trace.WriteLine(content);

            if (TraceLog.listener != null)
            {
                TraceLog.listener.WriteLine(content);
                TraceLog.listener.Flush();
            }

            if (TraceLog.Count < TraceLog.MAX_EXCEPTIONS)
            {
                TraceLog.exceptions.Add(new Exception(message, innerException));
            }
            else if (TraceLog.Count == TraceLog.MAX_EXCEPTIONS)
            {
                TraceLog.exceptions.Add(new Exception("Too many exceptions"));
            }
        }

        public static void Dispose()
        {
            try
            {
                if (TraceLog.listener != null)
                {
                    TraceLog.listener.Flush();

                    TraceLog.listener.Close();
                    TraceLog.listener.Dispose();
                }

                TraceLog.exceptions.Clear();
            }
            catch (Exception)
            {
            }
        }
    }
}
