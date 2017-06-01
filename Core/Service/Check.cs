using SBM.Model;
using System.Collections.Generic;

namespace SBM.Service
{
    public abstract class Check
    {
        public string Step { get; protected set; }
        
        protected SBM_DISPATCHER dispatcher;
        
        protected Check(SBM_DISPATCHER dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        protected abstract bool IsValid();

        public static bool CheckAll(SBM_DISPATCHER dispatcher)
        {
            var members = new List<Check>(new Check[] {
                new CheckEnable(dispatcher), //si el proceso está habilitado, el owner
                new CheckParent(dispatcher), //si terminó bien la última ejecución del padre
                new CheckMemory(dispatcher), //si no hay disponibilidad de memoria fisica
                new CheckThread(dispatcher), //si no hay disponibilidad de threads
                new CheckExclusive(dispatcher)});//si el proceso es de ejecución unica y está corriendo

            foreach (var member in members)
            {
                if (!member.IsValid())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
