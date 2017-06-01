using System;
using System.IO;
using System.Text;
using System.Xml;

namespace SBM.GenericDataQuery.Writer
{
    internal class WriterXML : FactoryWriter
    {
        private XmlWriter writer;
        private StringWriter stringOutput;

        public WriterXML()
            : base()
        {
            var settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            
            string fullName = Path.Combine(Parameter.Target.Provider, Parameter.Target.Output);

            if (string.IsNullOrEmpty(fullName))
            {
                this.writer = XmlWriter.Create(this.stringOutput = new StringWriter(), settings);
            }
            else
            {
                this.writer = XmlWriter.Create(
                    new StreamWriter(fullName.Wilcard(), Parameter.Target.Append), settings);
            }

            this.writer.WriteStartDocument();
        }

        public override void Header(string[] headers)
        {
            base.Header(headers);

            this.writer.WriteStartElement("list");
        }

        protected override void WriteImpl(string[] values)
        {
            this.writer.WriteStartElement("Add");

            for (int i=0; i<headers.Length; i++)
            {
                try
                {
                    this.writer.WriteAttributeString(headers[i], values[i]);
                    //this.writer.WriteElementString(headers[i], values[i]);
                }
                catch (Exception e)
                {
                    //this.writer.WriteAttributeString(headers[i], string.Empty);

                    string message = string.Format("Couldn't write {0} on row {1}", base.headers[i], base.RowNum);

                    TraceLog.AddError(message, e);

                    if (this.writer.WriteState == WriteState.Error)
                    {
                        throw new Exception(message, e);
                    }
                }
            }

            this.writer.WriteEndElement();
        }

        public override string GetResult()
        {
            this.writer.WriteEndElement();
            this.writer.WriteEndDocument();

            if (this.writer != null)
            {
                this.writer.Close();
                this.writer.Dispose();
            }

            return this.stringOutput == null ? null : this.stringOutput.ToString();
        }

        public override void Dispose()
        {
            try
            {
                if (this.stringOutput != null)
                {
                    this.stringOutput.Close();
                    this.stringOutput.Dispose();
                }
            }
            catch { }
        }
    }
}
