using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace SBM.Transfer
{
    /// <summary>
    /// Accesing to network connection with UNC 
    /// </summary>
    /// <example>
    /// <code>
    /// var credential = new NetworkCredential("user", "password");
    /// using (new NetworkConnection(@"\\192.168.0.1\shared", credential))
    /// {
    ///     Console.WriteLine(File.ReadAllText(@"\\192.168.0.1\shared\sample.txt"));
    /// }
    /// </code>
    /// </example>
    public class NetworkConnection 
    {
        private string NetworkName { get; set; }

        /// <summary>
        /// Open UNC
        /// </summary>
        /// <remarks>
        /// Please use within a block "using (new NetworkConnection(..."
        /// </remarks>
        /// <param name="networkName">UNC</param>
        /// <param name="credentials">Credentials with user and password. Domain is optional</param>
        /// <exception cref="Win32Exception">Api error number and description</exception>   
        [SecurityCritical]
        public NetworkConnection(string networkName, NetworkCredential credentials)
        {
            this.NetworkName = networkName;

            new SecurityPermission(SecurityPermissionFlag.ControlPolicy |  SecurityPermissionFlag.ControlEvidence).Demand();

            var netResource = new NetResource()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = this.NetworkName
            };

            var userName = string.IsNullOrEmpty(credentials.Domain)
                ? credentials.UserName : string.Format(@"{0}\{1}", credentials.Domain, credentials.UserName);

            Log.Write("SBM.Transfer [NetworkConnection.Ctor] WNetAddConnection2");

            var result2 = WNetAddConnection2(
                netResource, credentials.Password, userName, ConnectionFlags.CONNECT_TEMPORARY);

            if (result2 != ConnectionResult.SUCCESS)
            {
                Log.Write("SBM.Transfer [NetworkConnection.Ctor] Wrong WNetAddConnection2 : " + result2.GetText());

                throw new Win32Exception(result2.GetCode(), result2.GetText());
            }
        }

        [SecurityCritical]
        public void Close()
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence).Demand();

                Log.Write("SBM.Transfer [NetworkConnection.Close] WNetCancelConnection2");

                var result = WNetCancelConnection2(NetworkName, 0, true);

                if (result != DisconnectionResult.SUCCESS)
                {
                    Log.Write("SBM.Transfer [NetworkConnection.Close] Wrong WNetCancelConnection2 : " + result.GetText());
                }
                //SecurityPermission.RevertAll();
            }
            catch (Exception e)
            {
                Log.Write("SBM.Transfer [NetworkConnection.Close] Couldn't dispose", e);
            }
        }

        [DllImport("mpr.dll", SetLastError = true)]
        private static extern ConnectionResult WNetAddConnection2(NetResource netResource, string password, string username, ConnectionFlags flags);

        [DllImport("mpr.dll", SetLastError = true)]
        private static extern DisconnectionResult WNetCancelConnection2(string name, int flags, bool force);
    }
}
