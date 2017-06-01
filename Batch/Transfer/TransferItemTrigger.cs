using System.Data.OleDb;
using System.Data.SqlClient;

namespace SBM.Transfer
{
    public partial class Transfer
    {
        private int TriggerBefore(ThreadParameter param)
        {
            var affected = 0;

            if (!string.IsNullOrEmpty(param.ItemConfig.SourceSQLBefore))
            {
                param.Step = param.ItemConfig.Name + ": source trigger before";

                Log.Write(string.Format("SBM.Transfer [TransferItemTrigger.TriggerBefore] Execute: {0}",
                    param.ItemConfig.SourceSQLBefore));

                using (var SourceConnection = new OleDbConnection(Config.Source.Connection))
                {
                    SourceConnection.Open();
                    using (var cmd = new OleDbCommand(param.ItemConfig.SourceSQLBefore, SourceConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        affected = cmd.ExecuteNonQuery();
                    }
                }
            }

            if (!string.IsNullOrEmpty(param.ItemConfig.TargetSQLBefore))
            {
                param.Step = param.ItemConfig.Name + ": target trigger before";

                Log.Write(string.Format("SBM.Transfer [TransferItemTrigger.TriggerBefore] Execute [{0}].[{1}] : {2}",
                    Config.Target.Connection.GetValue("Data Source"),
                    Config.Target.Connection.GetValue("Initial Catalog"),
                    param.ItemConfig.TargetSQLBefore));

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(param.ItemConfig.TargetSQLBefore, TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        affected += cmd.ExecuteNonQuery();
                    }
                }
            }
            return affected;
        }

        private int TriggerAfter(ThreadParameter param)
        {
            var affected = 0;

            if (!string.IsNullOrEmpty(param.ItemConfig.TargetSQLAfter))
            {
                param.Step = param.ItemConfig.Name + ": trigger after";

                Log.Write(string.Format("SBM.Transfer [TransferItemTrigger.TriggerAfter] Execute [{0}].[{1}] : {2}",
                    Config.Target.Connection.GetValue("Data Source"),
                    Config.Target.Connection.GetValue("Initial Catalog"),
                    param.ItemConfig.TargetSQLAfter));

                using (var TargetConnection = new SqlConnection(Config.Target.Connection))
                {
                    TargetConnection.Open();
                    using (var cmd = new SqlCommand(param.ItemConfig.TargetSQLAfter, TargetConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        affected  = cmd.ExecuteNonQuery();
                    }
                }
            }

            if (!string.IsNullOrEmpty(param.ItemConfig.SourceSQLAfter))
            {
                param.Step = param.ItemConfig.Name + ": source trigger after";

                Log.Write(string.Format("SBM.Transfer [TransferItemTrigger.TriggerAfter] Execute: {0}",
                    param.ItemConfig.SourceSQLAfter));

                using (var SourceConnection = new OleDbConnection(Config.Source.Connection))
                {
                    SourceConnection.Open();
                    using (var cmd = new OleDbCommand(param.ItemConfig.SourceSQLAfter, SourceConnection))
                    {
                        cmd.CommandTimeout = Config.CommandTimeout;
                        affected += cmd.ExecuteNonQuery();
                    }
                }
            }

            return affected;
        }
    }
}
