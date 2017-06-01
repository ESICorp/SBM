using System;
using System.Configuration;
using System.IO;

namespace SBM.Service
{
    internal class Config
    {
        /// <summary>
        /// Expresado en segundos. Indica la frecuencia de ejecución del Timer del servicio. Default: 60. Rango: 10 - 3600.
        /// </summary>
        public static Int16 SBM_TIMER_INTERVAL { get; private set; }

        /// <summary>
        /// Cantidad. Número de objetos que se pueden ejecutar en forma simultánea. Default 0 (sin límite). Rango: 0 – 9999.
        /// </summary>
        public static Int16 SBM_MAX_OBJ_POOL { get; private set; }

        /// <summary>
        /// En Mb. Indica la memoria mínima libre que debe tener el sistema antes de permitir la ejecución de un nuevo objeto. Default: 1024. Rango: 100 – 9999.
        /// </summary>
        public static Int16 SBM_MIN_MEMORY { get; private set; }

        /// <summary>
        /// Mintuos de demora acepatos para arrancar el proceso
        /// </summary>
        public static Int16 SBM_ACCEPTED_DELAY { get; private set; }

        /// <summary>
        /// Phrase para dencrypt function de SQLServer
        /// </summary>
        public static string SBM_PHRASE { get; private set; }

        /// <summary>
        /// Minutes before shutting off (when SQLServer is disconnected)
        /// </summary>
        public static Int16 SBM_BEFORE_SHUTTING { get; private set; }

        /// <summary>
        /// Health period
        /// </summary>
        public static Int16 SBM_AUDIT_HEALTH_SECS { get; private set; }

        /// <summary>
        /// Max size from log file in megabytes
        /// </summary>
        public static Int16 SBM_LOG_SIZE { get; private set; }

        private Config()
        {
        }

#if RELOADCONFIG
        private static volatile bool refreshing = false;
#endif
        private static object syncWatcher = new object();

        public static string Initialize(ref string[] problems)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string file = Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            string msg = LoadValues(ref problems);

#if RELOADCONFIG
            Log.Debug(string.Format("SBM.Service [Config.Initialize] Watching {0} on {1}", file, path));

            var watcher = new FileSystemWatcher(path, file);
            watcher.Changed += Config_Changed;
            watcher.EnableRaisingEvents = true;
#endif
            return msg;
        }

#if RELOADCONFIG
        private static void Config_Changed(object sender, FileSystemEventArgs e)
        {
            Log.Write("SBM.Service [Config.Config_Changed]");

            if (!refreshing)
            {
                Log.Write("SBM.Service [Config.Config_Changed] Refreshing");

                lock (syncWatcher)
                {
                    refreshing = true;

                    try
                    {
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                    catch (Exception ex)
                    {
                        Log.Write("SBM.Service [Config.Config_Changed] RefreshSection", ex);
                    }

                    try
                    {
                        LoadValues();
                    }
                    catch (Exception ex)
                    {
                        Log.Write("SBM.Service [Config.Config_Changed] LoadValues", ex);
                    }

                    try
                    {
                        Trace.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Log.Write("SBM.Service [Config.Config_Changed] TraceRefresh", ex);
                    }


                    refreshing = false;
                }
            }
        }
#endif
        private static short GetAndCheck(string name, short @default, ref string[] problems)
        {
            return GetAndCheck(name, @default, short.MinValue, short.MaxValue, ref problems);
        }

        private static short GetAndCheck(string name, short @default, short min, short max, ref string[] problems)
        {
            short @short = 0;
            bool valid = true;

            string value = ConfigurationManager.AppSettings.Get(name) ?? "NOT FOUND";

            if (!Int16.TryParse(value, out @short))
            {
                Log.WriteAsync("SBM.Service [Config.LoadValues] Couldn't parse " + name + " <<" + value + ">>");

                valid = false;
            }

            if (@short > max || @short < min)
            {
                Log.WriteAsync("SBM.Service [Config.LoadValues] " + name + " invalid, use default <<" + @default + ">>");

                valid = false;
            }

            if (!valid)
            {
                @short = @default;

                AddProblem(ref problems, string.Format("Invalid parameter [{0}] = '{1}'. Setting default '{2}'", name, value, @default));
            }

            return @short;
        }

        private static void AddProblem(ref string[] problems, string problem)
        {
            if (problems == null)
            {
                problems = new string[1];
            }
            else
            {
                Array.Resize(ref problems, problems.Length + 1);
            }
            problems[problems.Length - 1] = problem;
        }

        public static string LoadValues(ref string[] problems)
        {
            Config.SBM_TIMER_INTERVAL = GetAndCheck("SBM_TIMER_INTERVAL", 60, 10, 3600, ref problems);
            Config.SBM_MAX_OBJ_POOL = GetAndCheck("SBM_MAX_OBJ_POOL", 0, 0, 9999, ref problems);
            Config.SBM_MIN_MEMORY = GetAndCheck("SBM_MIN_MEMORY", 1024, 100, 9999, ref problems);
            Config.SBM_ACCEPTED_DELAY = GetAndCheck("SBM_ACCEPTED_DELAY", 60, ref problems);
            Config.SBM_BEFORE_SHUTTING = GetAndCheck("SBM_BEFORE_SHUTTING", 0, 0, 1440, ref problems);
            Config.SBM_AUDIT_HEALTH_SECS = GetAndCheck("SBM_AUDIT_HEALTH_SECS", 0, 0, 32767, ref problems);
            Config.SBM_LOG_SIZE = GetAndCheck("SBM_LOG_SIZE", 8, 1, 1024, ref problems);

            Config.SBM_PHRASE = ConfigurationManager.AppSettings.Get("SBM_PHRASE");
            if (string.IsNullOrEmpty(Config.SBM_PHRASE))
            {
                AddProblem(ref problems, "Invalid parameter [SBM_PHRASE] = ''. Not setting default ''");
            }

            string msg = string.Format("SBM_VERSION={0},SBM_TIMER_INTERVAL={1} SECS,SBM_MAX_OBJ_POOL={2} THREAD,SBM_MIN_MEMORY={3} MB,SBM_ACCEPTED_DELAY={4} MIN,SBM_BEFORE_SHUTTING={5} MIN,SBM_LOG_SIZE={6} MB,SBM_AUDIT_HEALTH_SECS={7} SEC",
                typeof(Config).Assembly.GetName().Version,
                Config.SBM_TIMER_INTERVAL,
                Config.SBM_MAX_OBJ_POOL,
                Config.SBM_MIN_MEMORY,
                Config.SBM_ACCEPTED_DELAY,
                Config.SBM_BEFORE_SHUTTING,
                Config.SBM_LOG_SIZE,
                Config.SBM_AUDIT_HEALTH_SECS);

            Log.Debug("SBM.Service [Config.LoadValues] " + msg);

            return msg;
        }
    }
}
