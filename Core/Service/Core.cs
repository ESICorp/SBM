using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SBM.Service
{
    internal class Core
    {
        /// <summary>
        /// Dictionary with callback execution list
        /// </summary>
        public IDictionary<Int32, Process> Running { get; private set; }

        /// <summary>
        /// Unique
        /// </summary>
        public ISet<Int32> UniqueRunning { get; private set; }

        /// <summary>
        /// Synchronize running/pending executions
        /// </summary>
        public object SyncList { get; private set; }

        private Core()
        {
            Log.Debug("SBM.Service [Core.Ctor]");

            this.Running =
                new Dictionary<Int32, Process>();

            this.UniqueRunning =
                new HashSet<Int32>();

            this.SyncList = new object();
        }

        private static Core instance = null;

        /// <summary>
        /// Return the unique core
        /// </summary>
        /// <returns>Core</returns>
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public static Core GetInstance()
        {
            if (instance == null) instance = new Core();

            return instance;
        }
    }
}
