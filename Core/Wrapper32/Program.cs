using SBM.Common;
using SBM.Model;
using SBM.Wrapper;
using System;
using System.Threading;

namespace SBM.Wrapper32
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var timeout = new Timer(new TimerCallback(Program.Shutdown), -1, TimeSpan.FromSeconds(Consts.WithoutTimeout), Timeout.InfiniteTimeSpan))
            {
                EntryPoint.Run();
            }
        }

        private static void Shutdown(object state)
        {
            Environment.FailFast("Process zombie");
        }
    }
}
