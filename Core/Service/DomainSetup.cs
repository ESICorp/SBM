using System;
using System.IO;

namespace SBM.Service
{
    internal class DomainSetup
    {
        private DomainSetup()
        {
        }

        public static AppDomainSetup GetTemp(Context context)
        {
            Log.Debug("SBM.Service [DomainSetup.GetTemp] " + context.ProcessName);

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationName = context.ProcessName + "_tmp_" + Guid.NewGuid().ToString("N");
            setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            setup.PrivateBinPath = context.AssemblyDirectory;
            //setup.PrivateBinPathProbe = processInfo.AssemblyDirectory;
            //setup.LoaderOptimization = LoaderOptimization.MultiDomainHost;

            return setup;
        }

        public static AppDomainSetup GetDefaults(Context context)
        {
            Log.Debug("SBM.Service [DomainSetup.GetDefaults] " + context.ProcessName);

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationName = context.ProcessName + "_" + Guid.NewGuid().ToString("N"); ;
            setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            setup.PrivateBinPath = Path.Combine("Repository", context.AssemblyDirectory);
            //setup.PrivateBinPathProbe = processInfo.AssemblyDirectory;
            //setup.LoaderOptimization = LoaderOptimization.MultiDomainHost;

            //valida exista un config
            string configFile = Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                "Repository",
                context.AssemblyDirectory,
                context.AssemblyFullName) +
                    (context.AssemblyFullName.EndsWith(".dll") ? string.Empty : ".dll") + ".config";

            if (File.Exists(configFile))
            {
                setup.ConfigurationFile = configFile;
            }
            else
            {
                Log.Debug("SBM.Service [DomainSetup.GetTemp] " + configFile + " not found");
            }

            return setup;
        }
    }
}
