using System;
using System.Runtime.ExceptionServices;

namespace SBM.Service
{
    [Obsolete("Not working", true)]
    [Serializable]
    public class DomainEventHandler
    {
        private string Name { get; set; }

        public DomainEventHandler(string name)
        {
            this.Name = name;
        }

        public void FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            Log.WriteAsync("SBM.Service [DomainEventHandler.FirstChanceException] " + Name, e.Exception);
        }

        public void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            Log.WriteAsync("SBM.Service [DomainEventHandler.UnhandledException] " + Name, exception);
        }
    }
}
