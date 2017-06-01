using System;
using System.Configuration;
using System.IO;

namespace SBM.Agent
{
    internal class Config
    {
        /// <summary>
        /// Cantidad. Número de objetos que se pueden ejecutar en forma simultánea. Default 0 (sin límite). Rango: 0 – 9999.
        /// </summary>
        public static Int16 SBM_MAX_OBJ_POOL { get; private set; }

        /// <summary>
        /// En Mb. Indica la memoria mínima libre que debe tener el sistema antes de permitir la ejecución de un nuevo objeto. Default: 1024. Rango: 100 – 9999.
        /// </summary>
        public static Int16 SBM_MIN_MEMORY { get; private set; }

        /// <summary>
        /// Max size from log file in megabytes
        /// </summary>
        public static Int16 SBM_LOG_SIZE { get; private set; }

        /// <summary>
        /// TCP/IP Listener Port
        /// </summary>
        public static Int16 SBM_LISTEN_PORT { get; private set; }

        private Config()
        {
        }

        public static string Initialize(ref string[] problems)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string file = Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            string msg = LoadValues(ref problems);

            return msg;
        }

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
                Log.Write("SBM.Agent [Config.LoadValues] Couldn't parse " + name + " <<" + value + ">>");

                valid = false;
            }

            if (@short > max || @short < min)
            {
                Log.Write("SBM.Agent [Config.LoadValues] " + name + " invalid, use default <<" + @default + ">>");

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
            Config.SBM_MAX_OBJ_POOL = GetAndCheck("SBM_MAX_OBJ_POOL", 0, 0, 9999, ref problems);
            Config.SBM_MIN_MEMORY = GetAndCheck("SBM_MIN_MEMORY", 1024, 100, 9999, ref problems);
            Config.SBM_LOG_SIZE = GetAndCheck("SBM_LOG_SIZE", 8, 1, 1024, ref problems);
            Config.SBM_LISTEN_PORT = GetAndCheck("SBM_LISTEN_PORT", 4921, 1025, 32767, ref problems);

            string msg = string.Format("[VERSION]='{0}' - [SBM_MAX_OBJ_POOL]='{1} threads' - [SBM_MIN_MEMORY]='{2} MB' - [SBM_LOG_SIZE]='{3} MB' - [SBM_LISTEN_PORT]={4}",
                typeof(Config).Assembly.GetName().Version,
                Config.SBM_MAX_OBJ_POOL,
                Config.SBM_MIN_MEMORY,
                Config.SBM_LOG_SIZE,
                Config.SBM_LISTEN_PORT);

            Log.Debug("SBM.Agent [Config.LoadValues] " + msg);

            return msg;
        }
    }
}
