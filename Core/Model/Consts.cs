using System;
using System.Security.Policy;

namespace SBM.Model
{
    /// <summary>
    /// Constants
    /// </summary>
    public sealed class Consts
    {
        /// <summary>
        /// No timeout (1 days)
        /// </summary>
        public static int WithoutTimeout = 86400;

        /// <summary>
        /// Threshold Timeout (15 secs)
        /// </summary>
        public static TimeSpan ThresholdTimeout = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Communication Timeout (30 secs)
        /// </summary>
        public static TimeSpan CommunicationTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Default Evidence
        /// </summary>
        public static Evidence Evidence = AppDomain.CurrentDomain.Evidence;

        /// <summary>
        /// Null Owner
        /// </summary>
        public static Int16 NullOwner = 2;

        /// <summary>
        /// Null Service
        /// </summary>
        public static Int16 NullService = 0;

        /// <summary>
        /// Unspecified
        /// </summary>
        public static Byte LOG_UNSPECIFIED = 0;

        /// <summary>
        /// Id Started Log
        /// </summary>
        public static Byte LOG_APPLICATION_STARTED = 1;

        /// <summary>
        /// Application Pool Full
        /// </summary>
        public static Byte LOG_APPLICATION_POOL_FULL = 2;

        /// <summary>
        /// New Scheduled service in queue
        /// </summary>
        public static Byte LOG_SERVICE_SCHEDULED_QUEUE = 3;

        /// <summary>
        /// Process killed, timeout
        /// </summary>
        public static Byte LOG_SERVICE_TIMEOUT = 4;

        /// <summary>
        /// Error Killing
        /// </summary>
        public static Byte LOG_ERROR_KILLING_SERVICE = 5;

        /// <summary>
        /// Service already running
        /// </summary>
        public static Byte LOG_SERVICE_ALREADY_RUNNING = 6;

        /// <summary>
        /// Audit
        /// </summary>
        public static Byte LOG_AUDIT = 7;

        /// <summary>
        /// Reserved
        /// </summary>
        public static Byte LOG_HEALTHY = 8;

        /// <summary>
        /// Id Stopped Log
        /// </summary>
        public static Byte LOG_APPLICATION_STOPPED = 9;

        /// <summary>
        /// Runing
        /// </summary>
        public static Byte STATUS_RUNNING = 2;

        /// <summary>
        /// Completed Successfully
        /// </summary>
        public static Byte STATUS_SUCCESS = 3;

        /// <summary>
        /// Completed With Errors
        /// </summary>
        public static Byte STATUS_WITH_ERRORS = 4;

        /// <summary>
        /// Completed Fatal Error
        /// </summary>
        public static Byte STATUS_FATAL_ERROR = 5;

        /// <summary>
        /// Canceled, process timeout 
        /// </summary>
        public static Byte STATUS_TIMEOUT = 6;

        /// <summary>
        /// Canceled, internal error
        /// </summary>
        public static Byte STATUS_INTERNAL_ERROR = 7;

        /// <summary>
        /// Service disabled
        /// </summary>
        public static Byte STATUS_SERVICE_DISABLED = 8;

        /// <summary>
        /// Service unnknown
        /// </summary>
        public static Byte STATUS_SERVICE_UNKNOWN = 9;

        /// <summary>
        /// Canceled, user request
        /// </summary>
        public static Byte STATUS_CANCELLED_BY_USER = 10;

        /// <summary>
        /// Canceled, service already running
        /// </summary>
        public static Byte STATUS_CANCELLED_SERVICE_ALREADY_RUNNING = 11;

        /// <summary>
        /// Canceled, Parent service not ready
        /// </summary>
        public static Byte STATUS_CANCELLED_PARENT_NOT_READY = 12;

        /// <summary>
        /// Owner disabled
        /// </summary>
        public static Byte STATUS_OWNER_DISABLED = 13;

        /// <summary>
        /// Not enough resources
        /// </summary>
        public static Byte STATUS_NOT_ENOUGH_RESOURCE = 14;

        private Consts()
        {
        }
    }
}
