using System;
using System.Runtime.InteropServices;

namespace SBM.Transfer
{
    //[SecuritySafeCritical]
    [Flags]
    internal enum ConnectionFlags
    {
        CONNECT_UPDATE_PROFILE = 1,
        CONNECT_UPDATE_RECENT = 2,
        CONNECT_TEMPORARY = 4,
        CONNECT_INTERACTIVE = 8,
        CONNECT_PROMPT = 16,
        CONNECT_NEED_DRIVE = 32,
        CONNECT_REFCOUNT = 64,
        CONNECT_REDIRECT = 128,
        CONNECT_LOCALDRIVE = 256,
        CONNECT_CURRENT_MEDIA = 512
    }

    //[SecuritySafeCritical]
    internal enum ConnectionResult : int
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        SUCCESS = 0x0,
        /// <summary>
        /// Access is denied
        /// </summary>
        ERROR_ACCESS_DENIED = 0x5,
        /// <summary>
        /// The local device name is already in use.
        /// </summary>
        ERROR_ALREADY_ASSIGNED = 0x55,
        /// <summary>
        /// The network resource type is not correct.
        /// </summary>
        ERROR_BAD_DEV_TYPE = 0x42,
        /// <summary>
        /// The specified device name is invalid.
        /// </summary>
        ERROR_BAD_DEVICE = 0x4B0,
        /// <summary>
        /// The network name cannot be found.
        /// </summary>
        ERROR_BAD_NET_NAME = 0x43,
        /// <summary>
        /// The network connection profile is corrupted.
        /// </summary>
        ERROR_BAD_PROFILE = 0x4B6,
        /// <summary>
        /// The specified network provider name 
        /// is invalid.
        /// </summary>
        ERROR_BAD_PROVIDER = 0x4B4,
        /// <summary>
        /// The specified username is invalid.
        /// </summary>
        ERROR_BAD_USERNAME = 0x89A,
        /// <summary>
        /// The requested resource is in use.
        /// </summary>
        ERROR_BUSY = 0xAA,
        /// <summary>
        /// The operation was canceled by the user.
        /// </summary>
        ERROR_CANCELLED = 0x4C7,
        /// <summary>
        /// Unable to open the network connection profile.
        /// </summary>
        ERROR_CANNOT_OPEN_PROFILE = 0x4B5,
        /// <summary>
        /// The local device name has a remembered connection to another network resource.
        /// </summary>
        ERROR_DEVICE_ALREADY_REMEMBERED = 0x4B2,
        /// <summary>
        /// An extended error has occurred.
        /// </summary>
        ERROR_EXTENDED_ERROR = 0x4B8,
        /// <summary>
        /// Attempt to access invalid address.
        /// </summary>
        ERROR_INVALID_ADDRESS = 0x1E7,
        /// <summary>
        /// The parameter is incorrect.
        /// </summary>
        ERROR_INVALID_PARAMETER = 0x57,
        /// <summary>
        /// The specified network password is not correct.
        /// </summary>
        ERROR_INVALID_PASSWORD = 0x56,
        /// <summary>
        /// The user name or password is incorrect.
        /// </summary>
        ERROR_LOGON_FAILURE = 0x52E,
        /// <summary>
        /// The network path was either typed incorrectly, does not exist, or the network provider is not currently available. Please try retyping the path or contact your network administrator.
        /// </summary>
        ERROR_NO_NET_OR_BAD_PATH = 0x4B3,
        /// <summary>
        /// The network is not present or not started.
        /// </summary>
        ERROR_NO_NETWORK = 0x4C6,
        /// <summary>
        /// Multiple connections to a server or shared resource by the same user, using more than one user name, are not allowed. Disconnect all previous connections to the server or shared resource and try again
        /// </summary>
        ERROR_SESSION_CREDENTIAL_CONFLICT = 0x4C3
    }

    //[SecuritySafeCritical]
    internal static class ConnectionResultExtensions
    {
        public static int GetCode(this ConnectionResult number)
        {
            return (int)number;
        }

        public static string GetText(this ConnectionResult number)
        {
            switch (number)
            {
                case ConnectionResult.SUCCESS: return "The operation completed successfully";
                case ConnectionResult.ERROR_ACCESS_DENIED: return "Access is denied";
                case ConnectionResult.ERROR_ALREADY_ASSIGNED: return "The local device name is already in use";
                case ConnectionResult.ERROR_BAD_DEV_TYPE: return "The network resource type is not correct";
                case ConnectionResult.ERROR_BAD_DEVICE: return "The specified device name is invalid";
                case ConnectionResult.ERROR_BAD_NET_NAME: return "The network name cannot be found";
                case ConnectionResult.ERROR_BAD_PROFILE: return "The network connection profile is corrupted";
                case ConnectionResult.ERROR_BAD_PROVIDER: return "The specified network provider name is invalid";
                case ConnectionResult.ERROR_BAD_USERNAME: return "The specified username is invalid";
                case ConnectionResult.ERROR_BUSY: return "The requested resource is in use";
                case ConnectionResult.ERROR_CANCELLED: return "The operation was canceled by the user";
                case ConnectionResult.ERROR_CANNOT_OPEN_PROFILE: return "Unable to open the network connection profile";
                case ConnectionResult.ERROR_DEVICE_ALREADY_REMEMBERED: return "The local device name has a remembered connection to another network resource";
                case ConnectionResult.ERROR_EXTENDED_ERROR: return "An extended error has occurred";
                case ConnectionResult.ERROR_INVALID_ADDRESS: return "Attempt to access invalid address";
                case ConnectionResult.ERROR_INVALID_PARAMETER: return "The parameter is incorrect";
                case ConnectionResult.ERROR_INVALID_PASSWORD: return "The specified network password is not correct";
                case ConnectionResult.ERROR_LOGON_FAILURE: return "The user name or password is incorrect";
                case ConnectionResult.ERROR_NO_NET_OR_BAD_PATH: return "The network path was either typed incorrectly, does not exist, or the network provider is not currently available. Please try retyping the path or contact your network administrator";
                case ConnectionResult.ERROR_NO_NETWORK: return "The network is not present or not started.";
                case ConnectionResult.ERROR_SESSION_CREDENTIAL_CONFLICT: return "Multiple connections to a server or shared resource by the same user, using more than one user name, are not allowed. Disconnect all previous connections to the server or shared resource and try again.";
                default: return "Unknown error";
            }
        }
    }

    //[SecuritySafeCritical]
    internal enum DisconnectionResult : int
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        SUCCESS = 0x0,
        /// <summary>
        /// The network connection profile is corrupted.
        /// </summary>
        ERROR_BAD_PROFILE = 0x4B6,
        /// <summary>
        /// Unable to open the network connection profile.
        /// </summary>
        ERROR_CANNOT_OPEN_PROFILE = 0x4B5,
        /// <summary>
        /// The device is in use by an active process and cannot be disconnected.
        /// </summary>
        ERROR_DEVICE_IN_USE = 0x964,
        /// <summary>
        /// An extended error has occurred.
        /// </summary>
        ERROR_EXTENDED_ERROR = 0x4B8,
        /// <summary>
        /// This network connection does not exist.
        /// </summary>
        ERROR_NOT_CONNECTED = 0x8CA,
        /// <summary>
        /// This network connection has files open or requests pending.
        /// </summary>
        ERROR_OPEN_FILES = 0x961,
    }

    //[SecuritySafeCritical]
    internal static class DisconnectionResultExtensions
    {
        public static int GetCode(this DisconnectionResult number)
        {
            return (int)number;
        }

        public static string GetText(this DisconnectionResult number)
        {
            switch (number)
            {
                case DisconnectionResult.SUCCESS: return "The operation completed successfully";
                case DisconnectionResult.ERROR_BAD_PROFILE: return "The network connection profile is corrupted";
                case DisconnectionResult.ERROR_CANNOT_OPEN_PROFILE: return "Unable to open the network connection profile";
                case DisconnectionResult.ERROR_DEVICE_IN_USE: return "The device is in use by an active process and cannot be disconnected";
                case DisconnectionResult.ERROR_EXTENDED_ERROR: return "An extended error has occurred";
                case DisconnectionResult.ERROR_NOT_CONNECTED: return "This network connection does not exist";
                case DisconnectionResult.ERROR_OPEN_FILES: return "This network connection has files open or requests pending";
                default: return "Unknown error";
            }
        }
    }

    //[SecuritySafeCritical]
    [StructLayout(LayoutKind.Sequential)]
    internal class NetResource
    {
        public ResourceScope Scope;
        public ResourceType ResourceType;
        public ResourceDisplaytype DisplayType;
        public int Usage;
        public string LocalName;
        public string RemoteName;
        public string Comment;
        public string Provider;
    }

    //[SecuritySafeCritical]
    internal enum ResourceDisplaytype : int
    {
        Generic = 0x0,
        Domain = 0x01,
        Server = 0x02,
        Share = 0x03,
        File = 0x04,
        Group = 0x05,
        Network = 0x06,
        Root = 0x07,
        Shareadmin = 0x08,
        Directory = 0x09,
        Tree = 0x0a,
        Ndscontainer = 0x0b
    }

    //[SecuritySafeCritical]
    internal enum ResourceScope : int
    {
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
    }

    //[SecuritySafeCritical]
    internal enum ResourceType : int
    {
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8,
    }
}
