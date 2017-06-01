using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SBM.GenericDataQuery.Reader
{
    internal class ReaderXLS : FactoryReader
    {
        private IEnumerator<XmlElement> list;

        private string SS = "urn:schemas-microsoft-com:office:spreadsheet";
        private string OF = "urn:schemas-microsoft-com:office:office";
        private XmlNamespaceManager ns;

        private IList<int> indexes = new List<int>();
        private IList<string> headers = new List<string>();

        private string[] buffer = null;

        public ReaderXLS()
            : base()
        {
            var doc = new XmlDocument();
            doc.Load(Parameter.Source.Provider);

            this.ns = new XmlNamespaceManager(doc.NameTable);
            this.ns.AddNamespace("ss", this.SS);
            this.ns.AddNamespace("o", this.OF);

            this.list = doc.SelectNodes("/ss:Workbook/ss:Worksheet/ss:Table/ss:Row", this.ns).Cast<XmlElement>().GetEnumerator();
        }

        public override string[] Header()
        {
            if (!this.list.MoveNext())
            {
                throw new Exception("Empty worksheet");
            }

            var columns = this.list.Current.SelectNodes("ss:Cell/ss:Data", this.ns);
            var parsed = Parameter.Source.Input.ParseAlias();

            for (int i = 0; i < parsed.Count; i++)
            {
                for (int j = 0; j < columns.Count; j++)
                {
                    if (string.Equals(parsed.Keys[i], columns[j].InnerText, StringComparison.OrdinalIgnoreCase))
                    {
                        this.headers.Add(parsed[i]);
                        this.indexes.Add(j);
                        break;
                    }
                }
            }

            return this.headers.ToArray();
        }

        protected override string[] NextImpl()
        {
            if (this.list.MoveNext())
            {
                if (this.buffer == null)
                {
                    this.buffer = new string[this.indexes.Count];
                }

                for (int i = 0; i < this.indexes.Count; i++)
                {
                    this.buffer[i] = string.Empty;
                }

                var bufferIndex = -1;
                for (int i = 0; i < this.indexes.Count && bufferIndex < this.indexes.Count; i++)
                {
                    try
                    {
                        //var data = this.list.Current.SelectSingleNode(string.Format("ss:Cell[{0}]/ss:Data", this.indexes[i] + 1), this.ns) as XmlElement;
                        var cell = this.list.Current.SelectSingleNode(string.Format("ss:Cell[{0}]", this.indexes[i] + 1), this.ns) as XmlElement;
                        if (cell == null) break;

                        var reindex = cell.GetAttribute("Index", this.SS);

                        if (string.IsNullOrEmpty(reindex))
                        {
                            bufferIndex++; 
                        }
                        else
                        {
                            bufferIndex = Convert.ToInt32(reindex) - 1;
                        }

                        var data = cell.SelectSingleNode("ss:Data", this.ns) as XmlElement;
                        if (data == null)
                        {
                            this.buffer[bufferIndex] = string.Empty;
                        }
                        else
                        {
                            this.buffer[bufferIndex] = data.InnerText;
                        }
                    }
                    catch (Exception e)
                    {
                        this.buffer[bufferIndex] = string.Empty;

                        TraceLog.AddError(string.Format("Couldn't read {0} on row {1}", i, base.RowNum), e);
                    }
                }

                return this.buffer;
            }
            else
            {
                return null;
            }
        }

        public override void Dispose()
        {
            if (this.list != null)
            {
                this.list.Dispose();
            }
        }
    }
}
