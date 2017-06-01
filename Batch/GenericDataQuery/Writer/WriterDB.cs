using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace SBM.GenericDataQuery.Writer
{
    internal class WriterDB : FactoryWriter
    {
        private SqlConnection connection = null;
        private SqlCommand command = null;
        private int[] lengths = null;

        public WriterDB() : base()
        {
            this.connection = new SqlConnection(Parameter.Target.Provider);
            this.connection.Open();

            this.command = new SqlCommand();
            this.command.CommandTimeout = 0;
            this.command.Connection = this.connection;

            //this.command.CommandText = "SET ANSI_WARNINGS OFF";
            //this.command.ExecuteNonQuery();
        }

        public override void Header(string[] headers)
        {
            base.Header(headers);

            var insert = new StringBuilder();
            insert.AppendFormat("INSERT INTO [{0}] (", Parameter.Target.Output);

            if (!Parameter.Target.Append)
            { 
                var drop = string.Format("IF OBJECT_ID('[{0}_stage]', 'U') IS NOT NULL DROP TABLE [{0}_stage]", Parameter.Target.Output);
                using (var cmd = new SqlCommand(drop, this.connection))
                {
                    cmd.ExecuteNonQuery();
                }

                var createStage = ConfigurationManager.AppSettings["CREATE_TABLE"];
                using (var cmd = new SqlCommand(createStage.Trim(), this.connection))
                {
                    cmd.Parameters.AddWithValue("@tabla", Parameter.Target.Output);
                    createStage = (string)cmd.ExecuteScalar();
                }

                createStage = createStage.Replace(string.Format("[{0}]", Parameter.Target.Output), string.Format("[{0}_stage]", Parameter.Target.Output));

                var regexPK = new Regex(@"CONSTRAINT\ \[.*\]\ PRIMARY\ KEY");
                createStage = regexPK.Replace(createStage, string.Format("CONSTRAINT [pk_{0}_{1}] PRIMARY KEY", Parameter.Target.Output, new Random().Next(9999)));
                
                using (var cmd = new SqlCommand(createStage, this.connection))
                {
                    cmd.ExecuteNonQuery();
                }

                var @switch = string.Format("ALTER TABLE [dbo].[{0}] SWITCH TO [dbo].[{0}_stage]", Parameter.Target.Output);
                using (var cmd = new SqlCommand(@switch, this.connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            foreach (var header in headers)
            {
                insert.AppendFormat("[{0}],", header);
            }

            insert.Remove(insert.Length - 1, 1);
            insert.Append(") values (");

            for (int i = 0; i < headers.Length; i++)
            {
                insert.AppendFormat("@p{0,2:00},", i);
            }

            insert.Remove(insert.Length - 1, 1);
            insert.Append(')');

            this.command.CommandText = insert.ToString();

            var sqlSchema = new StringBuilder();
            sqlSchema.Append("select ");
            foreach (var header in headers)
            {
                sqlSchema.AppendFormat("[{0}],", header);
            }
            sqlSchema.Remove(sqlSchema.Length - 1, 1);
            sqlSchema.AppendFormat(" from [{0}]", Parameter.Target.Output);

            using (var cmd = new SqlCommand(sqlSchema.ToString(), this.connection))
            {
                using (var datareader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    DataTable schema = datareader.GetSchemaTable();

                    this.lengths = new int[headers.Length];

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var providerType = (SqlDbType)schema.Rows[i]["ProviderType"];

                        this.command.Parameters.Add(string.Format("@p{0,2:00}", i), providerType);

                        this.lengths[i] = Convert.ToInt32(schema.Rows[i]["ColumnSize"]);
                    }
                }
            }
        }

        protected override void WriteImpl(string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                try
                {
                    if (this.command.Parameters[i].SqlDbType == SqlDbType.BigInt)
                    {
                        this.command.Parameters[i].Value = values[i].ToInt64OrDBNull();
                    }
                    else if (this.command.Parameters[i].SqlDbType == SqlDbType.Bit)
                    {
                        this.command.Parameters[i].Value = values[i].ToBoolOrDBNull();
                    }
                    else if (this.command.Parameters[i].SqlDbType == SqlDbType.VarChar ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.NVarChar ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.Text ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.NText)
                    {
                        this.command.Parameters[i].Value = values[i].ToStringOrDBNull(lengths[i]);
                    }
                    else if (this.command.Parameters[i].SqlDbType == SqlDbType.Decimal ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.Money ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.SmallMoney)
                    {
                        this.command.Parameters[i].Value = values[i].ToDecimalOrDBNull();
                    }
                    else if (this.command.Parameters[i].SqlDbType == SqlDbType.Date ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.DateTime ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.DateTime2 ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.DateTimeOffset ||
                        this.command.Parameters[i].SqlDbType == SqlDbType.SmallDateTime)
                    {
                        this.command.Parameters[i].Value = values[i].ToDateOrDBNull();
                    }
                    else if (this.command.Parameters[i].SqlDbType == SqlDbType.Float)
                    {
                        this.command.Parameters[i].Value = values[i].ToDoubleOrDBNull();
                    }
                    else if (this.command.Parameters[i].SqlDbType == SqlDbType.UniqueIdentifier)
                    {
                        this.command.Parameters[i].Value = values[i].ToGuidOrDBNull();
                    }
                    else if (this.command.Parameters[i].SqlDbType == SqlDbType.Int)
                    {
                        this.command.Parameters[i].Value = values[i].ToInt32OrDBNull();
                    }
                    else if (this.command.Parameters[i].SqlDbType == SqlDbType.SmallInt)
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

        public override string GetResult()
        {
            if (!Parameter.Target.Append)
            {
                var drop = string.Format("DROP TABLE [dbo].[{0}_stage]", Parameter.Target.Output);
                using (var cmd = new SqlCommand(drop, this.connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            return base.GetResult();
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
