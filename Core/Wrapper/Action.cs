using SBM.Service;
using System;
using System.IO;

namespace SBM.Wrapper
{
    public abstract class Action : IDisposable
    {
        public static readonly string GetTypes = "GetType";
        public static readonly string Submit = "Submit";
        public static readonly string Cancel = "Cancel";

        protected Request Request;
        protected Response Response;

        protected static AppDomain AppDomain = null;
        protected static BatchHandler Batch = null;

        public Action(Request request, Response response)
        {
            this.Request = request;
            this.Response = response;

            if (Action.AppDomain == null)
            {
                var fileInfo = new FileInfo(Request.FileFullName);
                var setup = new AppDomainSetup();
                setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                setup.PrivateBinPath = fileInfo.DirectoryName;

                var configFile = Request.FileFullName + ".config";
                if (File.Exists(configFile))
                {
                    setup.ConfigurationFile = configFile;
                }
                else
                {
                    Log.Debug("SBM.Wrapper [Action.Ctor] : " + configFile + " not found");
                }

                Action.AppDomain = AppDomain.CreateDomain(fileInfo.Name, AppDomain.CurrentDomain.Evidence, setup);

                var handler = new DomainEventHandler(fileInfo.Name);

                //Program.AppDomain.FirstChanceException += handler.FirstChanceException;
                //Program.AppDomain.UnhandledException += handler.UnhandledException;
                Action.AppDomain.ReflectionOnlyAssemblyResolve += handler.ReflectionOnlyAssemblyResolve;
            }
        }

        public abstract void Execute();

        public void Dispose()
        {
            try
            {
                if (Action.AppDomain != null)
                {
                    AppDomain.Unload(Action.AppDomain);
                }
            }
            catch { }
        }
    }
}
