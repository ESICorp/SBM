using System;
using System.IO;
using System.Text;
using System.Xml;

namespace SBM.GenericDataQuery.Writer
{
    internal class WriterXLS : FactoryWriter
    {
        private string NAMESPACE = "urn:schemas-microsoft-com:office:spreadsheet";
        private string PREFIX = "ss";
        private string ROW = "Row";
        private string CELL = "Cell";
        private string DATA = "Data";
        private string TYPE = "Type";
        private string STRING = "String";

        private XmlWriter writer;

        public WriterXLS()
            : base()
        {
            var settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;

            string fullName = Path.Combine(Parameter.Target.Provider, Parameter.Target.Output);
            this.writer = XmlWriter.Create(new StreamWriter(fullName.Wilcard(), Parameter.Target.Append), settings);

            this.writer.WriteStartDocument();
            this.writer.WriteProcessingInstruction("mso-application", "progid='Excel.Sheet'");

            this.writer.WriteStartElement("Workbook", NAMESPACE);
            this.writer.WriteAttributeString("xmlns", PREFIX, null, NAMESPACE);

            this.writer.WriteStartElement("Worksheet");
            this.writer.WriteAttributeString(PREFIX, "Name", null, "Sheet1");

            this.writer.WriteStartElement("Table");
        }

        public override void Header(string[] headers)
        {
            base.Header(headers);

            this.writer.WriteStartElement(ROW);

            for (int i = 0; i < headers.Length; i++)
            {
                this.writer.WriteStartElement(CELL);

                this.writer.WriteStartElement(DATA);
                this.writer.WriteAttributeString(PREFIX, TYPE, null, STRING);
                this.writer.WriteString(headers[i]);
                this.writer.WriteEndElement(); //DATA

                this.writer.WriteEndElement(); //CELL
            }

            this.writer.WriteEndElement(); //ROW
        }

        protected override void WriteImpl(string[] values)
        {
            this.writer.WriteStartElement(ROW);

            for (int i = 0; i < values.Length; i++)
            {
                try
                {
                    this.writer.WriteStartElement(CELL);

                    this.writer.WriteStartElement(DATA);
                    this.writer.WriteAttributeString(PREFIX, TYPE, null, STRING);
                    this.writer.WriteString(values[i]);
                    this.writer.WriteEndElement(); //DATA

                    this.writer.WriteEndElement(); //CELL
                }
                catch (Exception e)
                {
                    string message = string.Format("Couldn't write {0} on row {1}", base.headers[i], base.RowNum);

                    TraceLog.AddError(message, e);

                    if (this.writer.WriteState == WriteState.Error)
                    {
                        throw new Exception(message, e);
                    }
                }
            }

            this.writer.WriteEndElement(); //ROW
        }

        public override string GetResult()
        {
            this.writer.WriteEndElement(); //TABLE
            this.writer.WriteEndElement(); //SHEET
            this.writer.WriteEndElement(); //ROOT

            return null;
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
