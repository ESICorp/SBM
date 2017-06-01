using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Threading;

namespace SBM.Transfer
{
    public class ThreadParameter : IDisposable
    {
        private IDbConnection SourceConnection { get; set; }
        public Item ItemConfig { get; private set; }
        public IDbCommand SourceCommand { get; private set; }
        public SqlBulkCopy BulkCopy { get; private set; }
        public IDataReader SourceDataReader { get; set; }
        public long Rows { get; private set; }
        public string Step { get; set; }
        public ManualResetEvent ManualEvent { get; private set; }

        public ThreadParameter(Item config, ManualResetEvent manualEvent)
        {
            this.ItemConfig = config;
            this.ManualEvent = manualEvent;

            this.SourceConnection = new OleDbConnection(Config.Source.Connection);
            this.SourceConnection.Open();

            this.SourceCommand = new OleDbCommand(ItemConfig.SourceSQL);
            this.SourceCommand.Connection = SourceConnection;
            this.SourceCommand.CommandTimeout = Config.CommandTimeout;

            this.BulkCopy = new SqlBulkCopy(Config.Target.Connection);
            this.BulkCopy.SqlRowsCopied += (object sender, SqlRowsCopiedEventArgs e) => this.Rows = e.RowsCopied;
            this.BulkCopy.EnableStreaming = true;
            this.BulkCopy.BatchSize = Config.BatchSize;
            this.BulkCopy.NotifyAfter = Config.BatchSize;
            this.BulkCopy.BulkCopyTimeout = Config.CommandTimeout;
        }

        public void Dispose()
        {
            if (BulkCopy != null)
            {
                BulkCopy.Close();
            }

            if (SourceDataReader != null)
            {
                SourceDataReader.Dispose();
            }

            if (SourceCommand != null)
            {
                SourceCommand.Dispose();
            }

            if (SourceConnection != null)
            {
                SourceConnection.Dispose();
            }

            if (ManualEvent != null)
            {
                ManualEvent.Set();
            }
        }
    }
}
