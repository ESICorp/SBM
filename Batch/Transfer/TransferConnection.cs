using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SBM.Transfer
{
    public partial class Transfer
    {
        //[SecuritySafeCritical]
        private void Initalize()
        {
            NetworkConnect();

            //ConnectSource();

            //ConnectTarget();
        }

        //private void ConnectTarget()
        //{
        //    Step = "connect target";

        //    Log.Write(string.Format("SBM.Transfer [TransferConnection.ConnectTarget] [{0}].[{1}]",
        //        Config.Target.Connection.GetValue("Data Source"),
        //        Config.Target.Connection.GetValue("Initial Catalog")));

        //    TargetConnection = new SqlConnection(Config.Target.Connection);
        //    TargetConnection.Open();
        //}

        //private void ConnectSource() {

        //    Step = "connect source";

        //    Log.Write(string.Format("SBM.Transfer [TransferConnection.ConnectSource] [{0}].[{1}]",
        //        Config.Source.Connection.GetValue("Data Source"),
        //        Config.Source.Connection.GetValue("Initial Catalog")));

        //    SourceConnection = new OleDbConnection(Config.Source.Connection);
        //    SourceConnection.Open();
        //}

        //[SecuritySafeCritical]
        private void NetworkConnect()
        {
            Step = "check must connect";

            if (IsVFPOleDB)
            {
                Step = "get unc";

                var unc = Config.Source.Connection.GetValue("Data Source");

                Step = "is unc";

                if (unc.IsUNC())
                {
                    Step = "network connect";

                    Log.Write(string.Format("SBM.Transfer [TransferConnection.NetworkConnect] {0}\\{1} : {2}",
                        Config.Source.NetDomain, Config.Source.NetUser, unc.GetShared()));

                    NetworkConnection = new NetworkConnection(unc.GetShared(), new NetworkCredential(
                        Config.Source.NetUser, Config.Source.NetPassword, Config.Source.NetDomain));
                }
            }
        }

        //[SecuritySafeCritical]
        private void Clean()
        {
            Step = "disconnect";

            if (ManualEvents != null)
            {
                new List<WaitHandle>(ManualEvents.Values).ForEach( _ => _.Dispose() );
            }

            //if (TargetConnection != null)
            //{
            //    TargetConnection.Dispose();
            //}

            //if (SourceConnection != null)
            //{
            //    SourceConnection.Dispose();
            //}

            if (NetworkConnection != null)
            {
                NetworkConnection.Close();
            }
        }
    }
}
