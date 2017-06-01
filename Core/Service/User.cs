using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SBM.Service
{
    public class User : IDisposable
    {
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_PROVIDER_WINNT50 = 3;
        private const int LOGON32_TYPE_NEW_CREDENTIALS = 9;

        private readonly NativeMethods.SafeTokenHandle Handle;
        private readonly WindowsImpersonationContext Context;

        public User(NetworkCredential credential)
        {
            Log.Debug("SBM.Service [User.ctor] : Logon.User");

            var result = NativeMethods.LogonUser(
                credential.UserName,
                credential.Domain,
                credential.Password,
                LOGON32_TYPE_NEW_CREDENTIALS, //LOGON32_LOGON_INTERACTIVE, 
                LOGON32_PROVIDER_WINNT50, //LOGON32_PROVIDER_DEFAULT, 
                out Handle);

            var last = Marshal.GetLastWin32Error();

            if (!result)
            {
                Log.WriteAsync("SBM.Service [User.ctor] User or password incorrect : " + credential.UserName);

                throw new Win32Exception(last, "User or password incorrect");
            }

            this.Context = WindowsIdentity.Impersonate(Handle.DangerousGetHandle());

            Log.Debug("SBM.Service [User.Ctor] WindowsIdentity.Impersonate >> " + WindowsIdentity.GetCurrent().Name);
        }


        public void Dispose()
        {
            try
            {
                if (this.Context != null)
                {
                    this.Context.Dispose();
                }
            } catch(Exception) {}

            try 
            {
                if (this.Handle != null)
                {
                    this.Handle.Dispose();
                }
            }
            catch (Exception) { }
        }
    }
}
