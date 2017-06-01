using System;
using System.Threading.Tasks;

namespace SBM.PowerShell
{
#if DEBUG
    internal static class EntryPoint
    {
        public static void Main(string[] args)
        {
            var tasks = new Task<Guid>[3];
            var guids = new Guid[3];
    
            for (int i = 0; i < 3; i++)
            {
                tasks[i] = Task.Factory.StartNew( () =>
                        new Enqueue()
                        {
                            Url = "ws://localhost:52717",
                            Owner = 2,
                            Token = "20130820183617",
                            Service = 1,
                            Parameters = "10"
                        }.Debug());
            }

            Task.WaitAll(tasks);

            for (int i = 0; i < 3; i++)
            {
                guids[i] = tasks[i].Result;
            }

            var result = new Wait()
                {
                    Url = "ws://localhost:52717",
                    Handle = guids,
                    Timeout = 60
                }.Debug();

            return;
        }
    }
#endif
}