using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SBM.GenericDataQuery.Reader
{
    internal class ReaderTXT : FactoryReader
    {
        private TextReader reader = null;
        private Stream streamData = null;

        private IList<string> headers = new List<string>();
        private Definition[] definitions = null;

        private int lastLength = 0;
        private string[] buffer = null;

        public ReaderTXT()
            : base()
        {
            BuildDefAndHeader();

            this.streamData = new FileStream(Parameter.Source.Provider, FileMode.Open, FileAccess.Read);
            this.reader = new StreamReader(this.streamData);
        }    

        public override string[] Header()
        {
            return this.headers.ToArray();
        }

        protected override string[] NextImpl()
        {
            var line = this.reader.ReadLine();

            if (string.IsNullOrEmpty(line))
            {
                this.buffer = null;
            }
            else
            {
                if (this.buffer == null)
                {
                    this.buffer = new string[this.headers.Count()];
                }

                string[] splited = null;

                if (this.lastLength < 1)
                {
                    splited = line.Split(string.IsNullOrEmpty(Parameter.Source.Delimiter) ? '\t' : Parameter.Source.Delimiter[0]);
                }

                int idxBuffer = 0;

                for (int idxDefinition = 0; idxDefinition < this.definitions.Length; idxDefinition++)
                {
                    try
                    {
                        if (this.definitions[idxDefinition].Ignore) continue;

                        if (this.lastLength > 0)
                        {
                            this.buffer[idxBuffer] = line.Substring(this.definitions[idxDefinition].Start, this.definitions[idxDefinition].Length).Trim();
                        }
                        else
                        {
                            this.buffer[idxBuffer] = splited[idxDefinition].Trim();
                        }
                    }
                    catch (Exception e)
                    {
                        this.buffer[idxBuffer] = string.Empty;

                        TraceLog.AddError(string.Format("Couldn't read {0} on row {1}", this.definitions[idxDefinition].Name, base.RowNum), e);
                    }
                    finally
                    {
                        idxBuffer++;
                    }
                }
            }

            return this.buffer;
        }

        public override void Dispose()
        {
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

        private void BuildDefAndHeader()
        {
            int idxStart = 0;

            var fields = Parameter.Source.Input.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);

            this.definitions = new Definition[fields.Count()];

            for (int i = 0; i < fields.Count(); i++)
            {
                var aux = fields[i].Split(':');

                bool startWithAsterisk = aux[0].Trim().StartsWith("*", StringComparison.Ordinal);

                this.definitions[i] = new Definition()
                {
                    Ignore = startWithAsterisk,
                    Name = startWithAsterisk ? aux[0].Trim().Substring(1) : aux[0].Trim(),
                    Start = idxStart,
                    Length = this.lastLength = (aux.Count() > 1 ? int.Parse(aux[1]) : 0)
                };

                idxStart += this.definitions[i].Length;

                if (!this.definitions[i].Ignore)
                {
                    this.headers.Add(this.definitions[i].Name);
                }
            }
        }

        private class Definition
        {
            public bool Ignore { get; set; }
            public string Name { get; set; }
            public int Start { get; set; }
            public int Length { get; set; }
        }
    }
}
