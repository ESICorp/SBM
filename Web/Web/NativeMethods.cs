using System;
using System.Runtime.InteropServices;

namespace SBM.Web
{
    internal static class NativeMethods
    {

        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [Flags]
        internal enum PageProtection : uint
        {
            NoAccess = 0x01,
            Readonly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Guard = 0x100,
            NoCache = 0x200,
            WriteCombine = 0x400,
        }


        internal const int WAIT_OBJECT_0 = 0;
        internal const uint INFINITE = 0xFFFFFFFF;
        internal const int ERROR_ALREADY_EXISTS = 183;

        internal const uint SECURITY_DESCRIPTOR_REVISION = 1;

        internal const uint SECTION_MAP_READ = 0x0004;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject,
            uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool InitializeSecurityDescriptor(ref SECURITY_DESCRIPTOR sd, uint dwRevision);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool SetSecurityDescriptorDacl(ref SECURITY_DESCRIPTOR sd, bool daclPresent, IntPtr dacl, bool daclDefaulted);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateEvent(ref SECURITY_ATTRIBUTES sa, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        internal static extern bool PulseEvent(IntPtr hEvent);

        [DllImport("kernel32.dll")]
        internal static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateFileMapping(IntPtr hFile,
            ref SECURITY_ATTRIBUTES lpFileMappingAttributes, PageProtection flProtect, uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        internal static extern Int32 WaitForSingleObject(IntPtr handle, uint milliseconds);

        [DllImport("kernel32.dll")]
        internal static extern uint GetLastError();
    }
}