using System;

namespace SBM.Web
{
    public class DebugEventArgs : EventArgs
    {
        public int Pid { get; private set; }
        public string Value { get; private set; }

        public DebugEventArgs()
        {
        }

        public DebugEventArgs(int pid, string value)
        {
            this.Pid = pid;
            this.Value = value;
        }
    }
}