using System;

namespace SBM.Service
{
    [Serializable]
    public class Problem
    {
        public string Source { get; private set; }
        public string Message { get; private set; }
        public string StackTrace { get; private set; }
        public Problem InnerProblem { get; private set; }

        public Problem(Exception e)
        {
            Source = e.Source;
            Message = e.Message;
            StackTrace = e.StackTrace;
            if (e.InnerException != null)
            {
                InnerProblem = new Problem(e.InnerException);
            }
        }

        public static implicit operator Problem(Exception e)
        {
            return new Problem(e);
        }
    }
}
