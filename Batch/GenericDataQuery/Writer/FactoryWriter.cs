using System;

namespace SBM.GenericDataQuery.Writer
{
    internal abstract class FactoryWriter : AbstractFactory
    {
        protected string[] headers = null;

        public FactoryWriter() : base()
        {
        }

        public static FactoryWriter GetInstance()
        {
            FactoryWriter writer = null;

            if (string.Equals(Parameter.Target.Type, "DB", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Parameter.Target.Type, "MSSQL", StringComparison.OrdinalIgnoreCase)) 
            {
                writer = new WriterDB();
            }
            else if (Parameter.Target.Type.StartsWith("TXT", StringComparison.OrdinalIgnoreCase)) 
            {
                var args = Parameter.Target.Type.Split('|');

                writer = new WriterTXT(args.Length > 1 ? args[1] : "\t");
            }
            else if (string.Equals(Parameter.Target.Type, "xBase", StringComparison.OrdinalIgnoreCase)) 
            {
                writer = new WriterXBase();
            }
            else if (string.Equals(Parameter.Target.Type, "XLS", StringComparison.OrdinalIgnoreCase))
            {
                writer = new WriterXLS();
            }
            else //if (string.Equals(Parameter.Target.Type, "XML", StringComparison.OrdinalIgnoreCase)) 
            {
                writer = new WriterXML();
            }
            return writer;
        }

        public virtual void Header(string[] headers)
        {
            this.headers = headers;
        }

        public void Write(string[] values)
        {
            this.RowNum++;

            WriteImpl(values);
        }

        protected abstract void WriteImpl(string[] values);

        public virtual string GetResult()
        {
            return null;
        }
    }
}
