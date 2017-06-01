using System;

namespace SBM.GenericDataQuery
{
    internal abstract class AbstractFactory : IDisposable
    {
        public int RowNum { get; protected set; }

        public AbstractFactory()
        {
            this.RowNum = 0;
        }

        public abstract void Dispose();
    }
}
