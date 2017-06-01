using SBM.Service;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SBM.Agent
{
    internal class Core
    {
        /// <summary>
        /// Dictionary with callback execution list
        /// </summary>
        public IDictionary<int, BatchHandler> Running { get; private set; }

        private Core()
        {
            Log.Debug("SBM.Agent [Core.Ctor]");

            this.Running = new Dictionary<int, BatchHandler>();
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
