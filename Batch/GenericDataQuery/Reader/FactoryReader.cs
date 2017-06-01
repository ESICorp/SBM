using System;

namespace SBM.GenericDataQuery.Reader
{
    internal abstract class FactoryReader : AbstractFactory
    {
        public FactoryReader() : base()
        {
        }

        public static FactoryReader GetInstance()
        {
            FactoryReader reader = null;

            if (string.Equals(Parameter.Source.Type, "DB", StringComparison.OrdinalIgnoreCase)||
                string.Equals(Parameter.Source.Type, "MSSQL", StringComparison.OrdinalIgnoreCase)) 
            {
                reader = new ReaderDB();
            }
            else if (string.Equals(Parameter.Source.Type, "TXT", StringComparison.OrdinalIgnoreCase)) 
            {
                reader = new ReaderTXT();
            }
            else if (string.Equals(Parameter.Source.Type, "xBase", StringComparison.OrdinalIgnoreCase))
            {
                reader = new ReaderXBase();
            }
            else if (string.Equals(Parameter.Source.Type, "XLS", StringComparison.OrdinalIgnoreCase))
            {
                reader = new ReaderXLS();
            }
            else //if (string.Equals(Parameter.Source.Type, "XML", StringComparison.OrdinalIgnoreCase)) 
            {
                reader = new ReaderXML();
            }
            return reader;
        }

        public abstract string[] Header();

        public string[] Next()
        {
            var aux = this.NextImpl();

            if (aux != null)
            {
                base.RowNum++;
            }

            return aux;
        }

        protected abstract string[] NextImpl();
    }
}
