using System;

namespace SBM.Service
{
    /// <summary>
    /// Events Start/Finish
    /// </summary>
    [Serializable]
    public class BatchEventArgs 
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BatchEventArgs(int dispatcher, bool x86, string parameters, string domain, string username, byte[] password,
            string @private, short owner, short service, int timeout)
        {
            this.Dispatcher = dispatcher;
            this.x86 = x86;
            this.Parameters = parameters;
            this.Domain = domain;
            this.UserName = username;
            this.Password = password;

            this.Private = @private;
            this.Owner = owner;
            this.Service = service;

            this.Timeout = timeout;
        }

        /// <summary>
        /// ID_DISPATCHER
        /// </summary>
        public int Dispatcher { get; set; }

        /// <summary>
        /// Start Batch Execution
        /// </summary>
        public DateTimeOffset StartedDatetime { get; set; }

        /// <summary>
        /// Finish Batch Execution
        /// </summary>
        public DateTimeOffset FinishedDatetime { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        public string Parameters { get; private set; }

        /// <summary>
        /// Network Credentials
        /// </summary>
        public string Domain { get; private set; }

        /// <summary>
        /// Network Credentials
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Network Credentials
        /// </summary>
        public byte[] Password { get; private set; }

        /// <summary>
        /// Result
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Problem (Serializable Exception)
        /// </summary>
        public Problem Exception { get; set; }

        /// <summary>
        /// Thread Name
        /// </summary>
        public string ThreadName { get; set; }

        /// <summary>
        /// x86 Indicator (else 64)
        /// </summary>
        public bool x86 { get; set; }

        /// <summary>
        /// Internal Use
        /// </summary>
        public string Private { get; set; }

        /// <summary>
        /// ID_OWNER
        /// </summary>
        public short Owner { get; set; }

        /// <summary>
        /// ID_SERVICE
        /// </summary>
        public short Service { get; set; }

        /// <summary>
        /// Timeout
        /// </summary>
        public int Timeout { get; set; }
    }
}
