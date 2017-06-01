using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SBM.Transfer
{
    public partial class Transfer
    {
        private int CheckTarget(ThreadParameter param)
        {
            int @return = 0;

            @return += CreateTable(param);

            @return += CreatePrimaryKey(param);

            @return += Truncate(param);

            return @return;
        }

        private int CreateTable(ThreadParameter param)
        {
            if (param.ItemConfig.TargetAutoCreate)
            {
                param.Step = param.ItemConfig.Name + ": create table";

                StringBuilder create = new StringBuilder();

                create.Append("IF OBJECT_ID('");
                create.Append(param.ItemConfig.TargetTable.Trim());
                create.Append("')IS NULL CREATE TABLE[");
                create.Append(param.ItemConfig.TargetTable.Trim());
                create.Append("](");

                if (param.ItemConfig.Mapping != null && param.ItemConfig.Mapping.Count > 0)
                {
                    foreach (Field field in param.ItemConfig.Mapping)
                    {
                        create.Append('[');
                        create.Append(field.Target.Trim());
                        create.Append(']');
                        create.Append(string.IsNullOrEmpty(field.Type) ? param.SourceDataReader.GetSqlType(field.Source) : field.Type);
                        if (param.ItemConfig.SourceUnique.HasValue(field.Source))
                        {
                            create.Append("NOT NULL");
                        }
                        create.Append(',');
                    }
                }
                else
                {
                    for (int i = 0; i < param.SourceDataReader.FieldCount; i++)
                    {
                        create.Append('[');
                        create.Append(param.SourceDataReader.GetName(i).Trim());
                        create.Append(']');
                        create.Append(param.SourceDataReader.GetSqlType(i));
                        if (param.ItemConfig.SourceUnique.HasValue(param.SourceDataReader.GetName(i)))
                        {
                            create.Append("NOT NULL");
                        }
                        create.Append(',');
                    }
                }
                create.Length--;
                create.Append(')');

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(create.ToString(), TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            return 0;
        }

        private int CreatePrimaryKey(ThreadParameter param)
        {
            if (!string.IsNullOrEmpty(param.ItemConfig.SourceUnique))
            {
                param.Step = param.ItemConfig.Name + ": create primary key";

                StringBuilder primarykey = new StringBuilder();

                primarykey.Append("IF OBJECTPROPERTY(OBJECT_ID('");
                primarykey.Append(param.ItemConfig.TargetTable.Trim());
                primarykey.Append("'),'TABLEHASPRIMARYKEY')=0 ALTER TABLE[");
                primarykey.Append(param.ItemConfig.TargetTable.Trim());
                primarykey.Append("]ADD PRIMARY KEY(");

                foreach (string key in param.ItemConfig.SourceUnique.Split(new char[] { ',', ';' }))
                {
                    primarykey.Append('[');
                    primarykey.Append((param.ItemConfig.Mapping != null && param.ItemConfig.Mapping.Count > 0 ? param.ItemConfig.Mapping[key.Trim()].Target : key).Trim());
                    primarykey.Append("],");
                }
                primarykey.Length -= 1;
                primarykey.Append(')');

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(primarykey.ToString(), TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            return 0;
        }

        private int Truncate(ThreadParameter param)
        {
            if (param.ItemConfig.Switch)
            {
                return Switch(param);
            }
            else if (param.ItemConfig.Truncate)
            {
                param.Step = param.ItemConfig.Name + ": truncate table";

                StringBuilder truncate = new StringBuilder();

                truncate.Append("TRUNCATE TABLE [");
                truncate.Append(param.ItemConfig.TargetTable.Trim());
                truncate.Append(']');

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(truncate.ToString(), TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            return 0;
        }

        private int Switch(ThreadParameter param)
        {
            if (param.ItemConfig.Truncate)
            {
                param.Step = param.ItemConfig.Name + ": drop stage";

                var drop = new StringBuilder();

                drop.Append("IF OBJECT_ID('");
                drop.Append(param.ItemConfig.TargetTable.Trim());
                drop.Append("_stage')IS NOT NULL DROP TABLE[");
                drop.Append(param.ItemConfig.TargetTable.Trim());
                drop.Append("_stage]");

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(drop.ToString(), TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        cmd.ExecuteNonQuery();
                    }
                }

                param.Step = param.ItemConfig.Name + ": create stage";

                StringBuilder create = new StringBuilder();

                create.Append("SELECT * INTO [");
                create.Append(param.ItemConfig.TargetTable.Trim());
                create.Append("_stage] FROM [");
                create.Append(param.ItemConfig.TargetTable.Trim());
                create.Append("] WHERE 1=0 UNION ALL SELECT * FROM [");
                create.Append(param.ItemConfig.TargetTable.Trim());
                create.Append("] WHERE 1=0");

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(create.ToString(), TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        cmd.ExecuteNonQuery();
                    }
                }

                param.Step = param.ItemConfig.Name + ": create pk";

                StringBuilder pk = new StringBuilder();

                pk.Append("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND TABLE_NAME = '");
                pk.Append(param.ItemConfig.TargetTable.Trim());
                pk.Append("' AND TABLE_SCHEMA = 'dbo' ORDER BY ORDINAL_POSITION");

                var pk_fields = new LinkedList<string>();

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();

                    using (var cmd = new SqlCommand(pk.ToString(), TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pk_fields.AddLast(string.Format("[{0}]", reader.GetString(0)));
                            }
                        }
                    }
                }

                if (pk_fields.Count > 0)
                {
                    pk.Clear();

                    pk.Append("ALTER TABLE [dbo].[");
                    pk.Append(param.ItemConfig.TargetTable.Trim());
                    pk.Append("_stage] ADD  CONSTRAINT [PK_");
                    pk.Append(param.ItemConfig.TargetTable.Trim());
                    pk.Append('_');
                    pk.Append(new Random().Next(1000,9999));
                    pk.Append("] PRIMARY KEY CLUSTERED (");
                    pk.Append(string.Join(",", pk_fields));
                    pk.Append(")");

                    using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                    {
                        TargetConnection.Open();

                        using (var cmd = new SqlCommand(pk.ToString(), TargetConnection))
                        {
                            cmd.CommandTimeout = Config.CommandTimeout;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                param.Step = param.ItemConfig.Name + ": switch stage";

                var @switch = new StringBuilder();

                @switch.Append("ALTER TABLE [dbo].[");
                @switch.Append(param.ItemConfig.TargetTable.Trim());
                @switch.Append("] SWITCH TO [dbo].[");
                @switch.Append(param.ItemConfig.TargetTable.Trim());
                @switch.Append("_stage]");

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(@switch.ToString(), TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        cmd.ExecuteNonQuery();
                    }
                }

                param.Step = param.ItemConfig.Name + ": drop stage";

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(drop.ToString(), TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            return 0;
        }

    }
}
