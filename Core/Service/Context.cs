using System;
using System.Security.Policy;

namespace SBM.Service
{
    [Serializable]
    public sealed class Context
    {
        public Int32 Dispatcher { get; set; }
        public Int16 Service { get; set; }
        public String ProcessName { get; set; }
        public String AssemblyDirectory { get; set; }
        public String AssemblyFullName { get; set; }
        public String Parameter { get; set; }
        public Evidence Evidence { get; set; }
        public Boolean SingleExec { get; set; }
        public Int32 Timeout { get; set; }
        public Int16 MaxTimeRun { get; set; }
        public Int16 Owner { get; set; }
        public String Private { get; set; }
        public DateTimeOffset Started { get; set; }
        public String Domain { get; set; }
        public String User { get; set; }
        public Byte[] Password { get; set; }
        public Boolean x86 { get; set; }
        public String Server { get; set; }
    }
}
