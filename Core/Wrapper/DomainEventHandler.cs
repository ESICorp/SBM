using System;
using System.Reflection;

namespace SBM.Wrapper
{
    [Serializable]
    public class DomainEventHandler
    {
        private string Name { get; set; }

        public DomainEventHandler(string name)
        {
            this.Name = name;
        }

        public Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return System.Reflection.Assembly.ReflectionOnlyLoad(args.Name);
        }

        [Obsolete("Not working", true)]
        public void FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Log.Write("SBM.Wrapper [DomainEventHandler.FirstChanceException] " + Name, e.Exception);
        }

        [Obsolete("Not working", true)]
        public void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            Log.Write("SBM.Wrapper [DomainEventHandler.UnhandledExceptionEventArgs] " + Name, exception);
        }

    }
}
