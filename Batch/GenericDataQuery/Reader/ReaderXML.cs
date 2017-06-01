using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SBM.GenericDataQuery.Reader
{
    internal class ReaderXML : FactoryReader
    {
        private XmlReader reader;

        private NameValueCollection alias = new NameValueCollection();
        private NameValueCollection cache = new NameValueCollection();

        private IList<string> headers = new List<string>();

        private string[] buffer = null;

        private string nodeName = null;
        private Stream streamData = null;
        private Stream streamAux = null;

        public ReaderXML()
            : base()
        {
            if (string.IsNullOrEmpty(Parameter.Source.Provider))
            {
                this.streamData = new MemoryStream(Encoding.UTF8.GetBytes(Parameter.Source.Input));
                this.streamAux = new MemoryStream(Encoding.UTF8.GetBytes(Parameter.Source.Input));
            }
            else
            {
                this.streamData = new FileStream(Parameter.Source.Provider, FileMode.Open, FileAccess.Read);
                this.streamAux = new FileStream(Parameter.Source.Provider, FileMode.Open, FileAccess.Read);
            }

            using (var temp = XmlReader.Create(this.streamAux))
            {
                BuildAliasAndHeader(temp);
            }

            this.reader = XmlReader.Create(this.streamData);
            this.reader.ReadStartElement();
            this.reader.MoveToContent();
        }    

        public override string[] Header()
        {
            return this.headers.ToArray();
        }

        protected override string[] NextImpl()
        {
            if (this.reader.NodeType == XmlNodeType.EndElement)
            {
                this.buffer = null;
            }
            else
            {
                if (this.buffer == null)
                {
                    this.buffer = new string[this.alias.Count];
                }

                //first 
                this.cache.Clear();
                if (this.reader.HasAttributes)
                {
                    this.reader.MoveToFirstAttribute();
                    this.cache.Add(this.reader.Name, this.reader.Value);

                    while (this.reader.MoveToNextAttribute())
                    {
                        this.cache.Add(this.reader.Name, this.reader.Value);
                    }
                    this.reader.MoveToElement(); //go back
                }
                this.reader.ReadStartElement();
                this.reader.MoveToContent();

                while (this.reader.Name != this.nodeName)
                {
                    this.cache.Add(this.reader.Name, this.reader.ReadElementContentAsString());
                    this.reader.MoveToContent();
                }
                this.reader.ReadEndElement();
                this.reader.MoveToContent();
                
                //second 
                int column = -1;
                foreach (var name in this.alias.AllKeys)
                {
                    try
                    {
                        column++;

                        this.buffer[column] = this.cache[name];
                    }
                    catch (Exception e)
                    {
                        this.buffer[column] = string.Empty;

                        TraceLog.AddError(string.Format("Couldn't read {0} on row {1}", column, base.RowNum), e);
                    }
                }
            }

            return this.buffer;
        }

        public override void Dispose()
        {
            if (this.streamAux != null)
            {
                this.streamAux.Close();
                this.streamAux.Dispose();
            }
            if (this.streamData != null)
            {
                this.streamData.Close();
                this.streamData.Dispose();
            } 
            if (this.reader != null)
            {
                this.reader.Close();
                this.reader.Dispose();
            }
        }

        private void BuildAliasAndHeader(XmlReader temp)
        {
            temp.ReadStartElement();
            temp.MoveToContent();
            if (temp.NodeType == XmlNodeType.Element)
            {
                this.nodeName = temp.Name;
            }

            if (string.IsNullOrEmpty(Parameter.Source.Provider))
            {
                if (temp.HasAttributes)
                {
                    temp.MoveToFirstAttribute();

                    this.alias.Add(temp.Name, temp.Name);

                    while (temp.MoveToNextAttribute())
                    {
                        this.alias.Add(temp.Name, temp.Name);
                    }
                }

                temp.ReadStartElement();
                temp.MoveToContent();

                while (temp.Name != this.nodeName)
                {
                    this.alias.Add(temp.Name, temp.Name);

                    temp.ReadElementContentAsString();
                    temp.MoveToContent();
                }
            }
            else
            {
                this.alias.Add(Parameter.Source.Input.ParseAlias());
            }

            for (int i = 0; i < this.alias.Count; i++)
            {
                this.headers.Add(this.alias[i]);
            }
        }
    }
}
