using System;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace SBM.GenericDataQuery.Writer
{
    internal class WriterXBase : FactoryWriter
    {
        private OleDbConnection connection = null;
        private OleDbCommand command = null;

        public WriterXBase()
            : base()
        {
            if (Environment.Is64BitProcess)
            {
                throw new Exception("xBase must be 32 bit, please set DSP_SERVICE.X86 = 1");
            }
            if (!Parameter.Target.Provider.Contains(".dbc"))
            {
                throw new Exception("/Paramters/Source/Provider must be DBC file");
            }

            string connectionString = string.Format("Provider=vfpoledb;Data Source={0}", Parameter.Target.Provider);

            this.connection = new OleDbConnection(Parameter.Target.Provider);
            this.connection.Open();

            this.command = new OleDbCommand();
            this.command.Connection = this.connection;
        }

        public override void Header(string[] headers)
        {
            base.Header(headers);

            var insert = new StringBuilder();

            insert.AppendFormat("INSERT INTO [{0}] (", Parameter.Target.Output);

            foreach (var header in headers)
            {
                insert.AppendFormat("[{0}],", header);
            }

            insert.Remove(insert.Length - 1, 1);
            insert.Append(") values (");

            foreach (var header in headers)
            {
                insert.AppendFormat("@[{0}],", header);
            }

            insert.Remove(insert.Length - 1, 1);
            insert.Append(')');

            this.command.CommandText = insert.ToString();


            using (var cmd = new OleDbCommand(string.Format("select * from [{0}]", Parameter.Target.Output), this.connection))
            {
                using (var datareader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    DataTable schema = datareader.GetSchemaTable();

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var providerType = (OleDbType)schema.Rows[i]["ProviderType"];

                        this.command.Parameters.Add(string.Format("@p{0,2}", i), providerType);
                    }
                }
            }
            if (!Parameter.Target.Append)
            {
                using (var cmd = new OleDbCommand(string.Format("truncate table [{0}]", Parameter.Target.Output), this.connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override void WriteImpl(string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                try
                {
                    if (this.command.Parameters[i].OleDbType == OleDbType.BigInt)
                    {
                        this.command.Parameters[i].Value = values[i].ToInt64OrDBNull();
                    }
                    else if (this.command.Parameters[i].OleDbType == OleDbType.Boolean)
                    {
                        this.command.Parameters[i].Value = values[i].ToBoolOrDBNull();
                    }
                    else if (this.command.Parameters[i].OleDbType == OleDbType.VarChar ||
                        this.command.Parameters[i].OleDbType == OleDbType.LongVarChar ||
                        this.command.Parameters[i].OleDbType == OleDbType.LongVarWChar ||
                        this.command.Parameters[i].OleDbType == OleDbType.VarWChar ||
                        this.command.Parameters[i].OleDbType == OleDbType.BSTR ||
                        this.command.Parameters[i].OleDbType == OleDbType.WChar)
                    {
                        this.command.Parameters[i].Value = values[i];
                    }
                    else if (this.command.Parameters[i].OleDbType == OleDbType.Decimal ||
                        this.command.Parameters[i].OleDbType == OleDbType.Currency)
                    {
                        this.command.Parameters[i].Value = values[i].ToDecimalOrDBNull();
                    }
                    else if (this.command.Parameters[i].OleDbType == OleDbType.Date ||
                        this.command.Parameters[i].OleDbType == OleDbType.DBDate ||
                        this.command.Parameters[i].OleDbType == OleDbType.Filetime ||
                        this.command.Parameters[i].OleDbType == OleDbType.DBTimeStamp)
                    {
                        this.command.Parameters[i].Value = values[i].ToDateOrDBNull();
                    }
                    else if (this.command.Parameters[i].OleDbType == OleDbType.Double)
                    {
                        this.command.Parameters[i].Value = values[i].ToDoubleOrDBNull();
                    }
                    else if (this.command.Parameters[i].OleDbType == OleDbType.Guid)
                    {
                        this.command.Parameters[i].Value = values[i].ToGuidOrDBNull();
                    }
                    else if (this.command.Parameters[i].OleDbType == OleDbType.Integer)
                    {
                        this.command.Parameters[i].Value = values[i].ToInt32OrDBNull();
                    }
                    else if (this.command.Parameters[i].OleDbType == OleDbType.SmallInt)
                    {
                        this.command.Parameters[i].Value = values[i].ToInt16OrDBNull();
                    }
                    else
                    {
                        this.command.Parameters[i].Value = values[i];
                    }
                }
                catch (Exception e)
                {
                    this.command.Parameters[i].Value = DBNull.Value;

                    TraceLog.AddError(string.Format("Couldn't write {0} on row {1}", base.headers[i], base.RowNum), e);
                }
            }

            try
            {
                //this.command.Prepare();
                this.command.ExecuteNonQuery();
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
                if (this.command != null)
                {
                    this.command.Dispose();
                }
            }
            catch { }
            try
            {
                if (connection != null)
                {
                    this.connection.Close();
                    this.connection.Dispose();
                }
            }
            catch { }
        }
    }
}
