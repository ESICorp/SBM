using System;
using System.IO;
using System.Text;

namespace SBM.GenericDataQuery.Writer
{
    internal class WriterTXT : FactoryWriter
    {
        private string FieldSeprator;
        private StreamWriter writer;

        public WriterTXT(string fieldSeparator)
            : base()
        {
            this.FieldSeprator = fieldSeparator;

            string fullName = Path.Combine(Parameter.Target.Provider, Parameter.Target.Output);

            this.writer = new StreamWriter(fullName.Wilcard(), Parameter.Target.Append, Encoding.UTF8);
        }

        protected override void WriteImpl(string[] values)
        {
            for (int i=0; i<values.Length; i++) 
            {
                try
                {
                    values[i] = values[i]
                        .Replace(this.FieldSeprator, string.Empty)
                        .Replace(this.writer.NewLine, string.Empty);
                }
                catch (Exception e)
                {
                    //this.command.Parameters[i].Value = null;

                    TraceLog.AddError(string.Format("Couldn't write {0} on row {1}", base.headers[i], base.RowNum), e);
                }
            }

            try
            {
                this.writer.WriteLine(string.Join(this.FieldSeprator, values));
            }
            catch (Exception e)
            {
                TraceLog.AddError(string.Format("Couldn't write row {0}", base.RowNum), e);
            }
        }

        public override void Dispose()
        {
            try
            {
                if (this.writer != null)
                {
                    this.writer.Close();
                    this.writer.Dispose();
                }
            }
            catch { }
        }
    }
}
