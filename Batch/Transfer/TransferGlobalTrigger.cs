using System;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace SBM.Transfer
{
    public partial class Transfer
    {
        private int GlobalSourceTrigger(string sql)
        {
            if (!string.IsNullOrWhiteSpace(sql))
            {
                try
                {
                    Log.Write(string.Format("SBM.Transfer [Transfer.GlobalSourceTrigger] Executing : {0}", sql));

                    using (var cnn = new OleDbConnection(Config.Source.Connection))
                    {
                        cnn.Open();

                        using (var cmd = new OleDbCommand(sql))
                        {
                            cmd.Connection = cnn;
                            cmd.CommandTimeout = Config.CommandTimeout;

                            return cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Write(string.Format("SBM.Transfer [Transfer.GlobalSourceTrigger] Couldn't {0}", this.Step), e);

                    throw;
                }
            }
            return 0;
        }

        private int GlobalTargetTrigger(string sql)
        {
            if (!string.IsNullOrWhiteSpace(sql))
            {
                try
                {
                    Log.Write(string.Format("SBM.Transfer [Transfer.GlobalTargetTrigger] Executing : {0}", sql));

                    using (var cnn = new SqlConnection(Config.Target.Connection))
                    {
                        cnn.Open();

                        using (var cmd = new SqlCommand(sql))
                        {
                            cmd.Connection = cnn;
                            cmd.CommandTimeout = Config.CommandTimeout;

                            return cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Write(string.Format("SBM.Transfer [Transfer.GlobalTargetTrigger] Couldn't {0}", this.Step), e);

                    throw;
                }
            }
            return 0;
        }
    }
}
