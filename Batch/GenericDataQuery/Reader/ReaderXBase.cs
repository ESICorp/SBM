using System;
using System.Data;
using System.Data.OleDb;
using System.Globalization;

namespace SBM.GenericDataQuery.Reader
{
    internal class ReaderXBase : FactoryReader
    {
        private OleDbConnection connection = null;
        private OleDbCommand command = null;
        private OleDbDataReader datareader = null;
        private string[] buffer = null;

        public ReaderXBase() : base()
        {
            if (Environment.Is64BitProcess)
            {
                throw new Exception("xBase must be 32 bit, please set DSP_SERVICE.X86 = 1");
            }
            if (!Parameter.Source.Provider.Contains(".dbc"))
            {
                throw new Exception("/Parameters/Source/Provider must be DBC file");
            }

            string connectionString = string.Format("Provider=vfpoledb;Data Source={0}", Parameter.Source.Provider);

            this.connection = new OleDbConnection(connectionString);
            this.connection.Open();

            this.command = new OleDbCommand(Parameter.Source.Input, this.connection);
            this.command.CommandTimeout = 0;
            this.datareader = this.command.ExecuteReader(CommandBehavior.SingleResult);
        }

        public override string[] Header()
        {
            var headers = new string[this.datareader.FieldCount];

            for (int i = 0; i < this.datareader.FieldCount; i++)
            {
                headers[i] = this.datareader.GetName(i);
            }

            return headers;
        }

        protected override string[] NextImpl()
        {
            if (this.datareader.Read())
            {
                if (this.buffer == null)
                {
                    this.buffer = new string[this.datareader.FieldCount];
                }

                for (int i = 0; i < this.datareader.FieldCount; i++)
                {
                    try
                    {
                        if (this.datareader.IsDBNull(i))
                        {
                            this.buffer[i] = string.Empty;
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(Int64))
                        {
                            this.buffer[i] = Convert.ToString(this.datareader.GetInt64(i));
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(Boolean))
                        {
                            this.buffer[i] = Convert.ToString(this.datareader.GetBoolean(i));
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(String))
                        {
                            this.buffer[i] = this.datareader.GetString(i);
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(Decimal))
                        {
                            this.buffer[i] = Convert.ToString(this.datareader.GetDecimal(i), CultureInfo.InvariantCulture);
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(DateTime))
                        {
                            this.buffer[i] = this.datareader.GetDateTime(i).ToStringFormatted();
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(Double))
                        {
                            this.buffer[i] = Convert.ToString(this.datareader.GetDouble(i), CultureInfo.InvariantCulture);
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(Guid))
                        {
                            this.buffer[i] = this.datareader.GetGuid(i).ToString();
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(Int32))
                        {
                            this.buffer[i] = Convert.ToString(this.datareader.GetInt32(i));
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(Int16))
                        {
                            this.buffer[i] = Convert.ToString(this.datareader.GetInt16(i));
                        }
                        else if (this.datareader.GetFieldType(i) == typeof(Byte))
                        {
                            this.buffer[i] = Convert.ToString(this.datareader.GetByte(i));
                        }
                        else
                        {
                            this.buffer[i] = Convert.ToString(this.datareader.GetValue(i));
                        }
                    }
                    catch (Exception e)
                    {
                        this.buffer[i] = string.Empty;

                        TraceLog.AddError(string.Format("Couldn't read {0} on row {1}", this.datareader.GetName(i), base.RowNum), e);
                    }
                }

            }
            else
            {
                this.buffer = null;
            }

            return this.buffer;
        }

        public override void Dispose()
        {
            if (this.datareader != null)
            {
                if (!this.datareader.IsClosed)
                {
                    this.datareader.Close();
                }
                this.datareader.Dispose();
            }
            if (this.command != null)
            {
                this.command.Dispose();
            }
            if (connection != null)
            {
                this.connection.Close();
                this.connection.Dispose();
            }
        }
    }
}
