using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SBM.Web
{
    public delegate void OnOutputDebugStringHandler(object sender, EventArgs e);

    public sealed class DebugMonitor
    {
        private DebugMonitor()
        {
        }

        public static event OnOutputDebugStringHandler OnOutputDebugString;

        private static IntPtr m_AckEvent = IntPtr.Zero;

        private static IntPtr m_ReadyEvent = IntPtr.Zero;

        private static IntPtr m_SharedFile = IntPtr.Zero;

        private static IntPtr m_SharedMem = IntPtr.Zero;

        private static Thread m_Capturer = null;

        private static object m_SyncRoot = new object();

        private static Mutex m_Mutex = null;


        public static void Start()
        {
            lock (m_SyncRoot)
            {
                if (m_Capturer != null)
                {
                    throw new ApplicationException("This DebugMonitor is already started.");
                }

                bool createdNew = false;
                m_Mutex = new Mutex(false, typeof(DebugMonitor).Namespace, out createdNew);
                if (!createdNew)
                {
                    throw new ApplicationException("There is already an instance running.");
                }

                NativeMethods.SECURITY_DESCRIPTOR sd = new NativeMethods.SECURITY_DESCRIPTOR();
                int number;
                if (!NativeMethods.InitializeSecurityDescriptor(ref sd, NativeMethods.SECURITY_DESCRIPTOR_REVISION))
                {
                    number = Marshal.GetLastWin32Error();
                    throw CreateApplicationException("Failed to initializes the security descriptor.", number);
                }

                if (!NativeMethods.SetSecurityDescriptorDacl(ref sd, true, IntPtr.Zero, false))
                {
                    number = Marshal.GetLastWin32Error();
                    throw CreateApplicationException("Failed to initializes the security descriptor", number);
                }

                var sa = new NativeMethods.SECURITY_ATTRIBUTES();

                m_AckEvent = NativeMethods.CreateEvent(ref sa, false, false, "DBWIN_BUFFER_READY");
                number = Marshal.GetLastWin32Error();
                if (m_AckEvent == IntPtr.Zero)
                {
                    throw CreateApplicationException("Failed to create event 'DBWIN_BUFFER_READY'", number);
                }

                m_ReadyEvent = NativeMethods.CreateEvent(ref sa, false, false, "DBWIN_DATA_READY");
                number = Marshal.GetLastWin32Error();
                if (m_ReadyEvent == IntPtr.Zero)
                {
                    throw CreateApplicationException("Failed to create event 'DBWIN_DATA_READY'", number);
                }

                m_SharedFile = NativeMethods.CreateFileMapping(new IntPtr(-1), ref sa, NativeMethods.PageProtection.ReadWrite, 0, 4096, "DBWIN_BUFFER");
                number = Marshal.GetLastWin32Error();
                if (m_SharedFile == IntPtr.Zero)
                {
                    throw CreateApplicationException("Failed to create a file mapping to slot 'DBWIN_BUFFER'", number);
                }

                m_SharedMem = NativeMethods.MapViewOfFile(m_SharedFile, NativeMethods.SECTION_MAP_READ, 0, 0, 512);
                number = Marshal.GetLastWin32Error();
                if (m_SharedMem == IntPtr.Zero)
                {
                    throw CreateApplicationException("Failed to create a mapping view for slot 'DBWIN_BUFFER'", number);
                }

                m_Capturer = new Thread(new ThreadStart(Capture));
                m_Capturer.Start();
            }
        }

        private static void Capture()
        {
            try
            {
                IntPtr pString = new IntPtr(
                    m_SharedMem.ToInt32() + Marshal.SizeOf(typeof(int))
                );

                while (true)
                {
                    NativeMethods.SetEvent(m_AckEvent);

                    int ret = NativeMethods.WaitForSingleObject(m_ReadyEvent, NativeMethods.INFINITE);

                    if (m_Capturer == null)
                        break;

                    if (ret == NativeMethods.WAIT_OBJECT_0)
                    {
                        FireOnOutputDebugString(
                            Marshal.ReadInt32(m_SharedMem),
                                Marshal.PtrToStringAnsi(pString));
                    }
                }

            }
            catch
            {
                throw;

            }
            finally
            {
                Dispose();
            }
        }

        private static void FireOnOutputDebugString(int pid, string text)
        {
            if (OnOutputDebugString == null)
            {
                return;
            }

#if !DEBUG
            try
            {
#endif
            OnOutputDebugString(null, new DebugEventArgs(pid, text));
#if !DEBUG
            }
            catch (Exception ex)
            {
                Console.WriteLine("An 'OnOutputDebugString' handler failed to execute: " + ex.ToString());
            }
#endif
        }

        private static void Dispose()
        {
            if (m_AckEvent != IntPtr.Zero)
            {
                if (!NativeMethods.CloseHandle(m_AckEvent))
                {
                    var number = Marshal.GetLastWin32Error();
                    throw CreateApplicationException("Failed to close handle for 'AckEvent'", number);
                }
                m_AckEvent = IntPtr.Zero;
            }

            if (m_ReadyEvent != IntPtr.Zero)
            {
                if (!NativeMethods.CloseHandle(m_ReadyEvent))
                {
                    var number = Marshal.GetLastWin32Error();
                    throw CreateApplicationException("Failed to close handle for 'ReadyEvent'", number);
                }
                m_ReadyEvent = IntPtr.Zero;
            }

            if (m_SharedFile != IntPtr.Zero)
            {
                if (!NativeMethods.CloseHandle(m_SharedFile))
                {
                    var number = Marshal.GetLastWin32Error();
                    throw CreateApplicationException("Failed to close handle for 'SharedFile'", number);
                }
                m_SharedFile = IntPtr.Zero;
            }


            if (m_SharedMem != IntPtr.Zero)
            {
                if (!NativeMethods.UnmapViewOfFile(m_SharedMem))
                {
                    var number = Marshal.GetLastWin32Error();
                    throw CreateApplicationException("Failed to unmap view for slot 'DBWIN_BUFFER'", number);
                }
                m_SharedMem = IntPtr.Zero;
            }

            if (m_Mutex != null)
            {
                m_Mutex.Close();
                m_Mutex = null;
            }
        }

        public static void Stop()
        {
            lock (m_SyncRoot)
            {
                if (m_Capturer == null)
                {
                    throw new ObjectDisposedException("DebugMonitor", "This DebugMonitor is not running.");
                }
                
                m_Capturer = null;

                NativeMethods.PulseEvent(m_ReadyEvent);

                while (m_AckEvent != IntPtr.Zero)
                    ;
            }
        }

        private static ApplicationException CreateApplicationException(string text, int number)
        {
            if (text == null || text.Length < 1)
            {
                throw new ArgumentNullException("text", "'text' may not be empty or null.");
            }

            return new ApplicationException(string.Format("{0}. Last Win32 Error was {1}", text, number));
        }

    }
}