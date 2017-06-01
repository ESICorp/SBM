using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Threading;

namespace SBM.Transfer
{
    public partial class Transfer
    {
        public static ConcurrentDictionary<string, ManualResetEvent> ManualEvents = new ConcurrentDictionary<string, ManualResetEvent>();

        private NetworkConnection NetworkConnection { get; set; }
        //private IDbConnection SourceConnection { get; set; }
        public string Step { get; set; }
        public StringBuilder Message = new StringBuilder();
        public long Rows { get; set; }
        public object SyncObject = new object();

        private volatile int Good = 0;
        private volatile int Bad = 0;
        private volatile int Concurrents = 0;

        private bool IsVFPOleDB
        {
            get { 
                return (Config.Source.Connection.GetValue("Provider") ?? "").StartsWith("vfpoledb", true, CultureInfo.CurrentCulture); 
            }
        }
    }
}
