using System;
using System.Text;
using System.Web.Script.Serialization;

namespace SBM.RestServices
{
    public class RestServiceFault
    {
        public string Reason { get; private set; }
        public string Code { get; private set; }

        public RestServiceFault()
        {
        }

        public RestServiceFault(string reason, string code)
        {
            this.Reason = reason;
            this.Code = code;
        }

        [ScriptIgnore]
        public static readonly RestServiceFault AccessDenied = new RestServiceFault("Access denied", "1000");

        [ScriptIgnore]
        public static readonly RestServiceFault ServiceDoesntExist = new RestServiceFault("Service Does Not Exist", "1010");

        [ScriptIgnore]
        public static readonly RestServiceFault ServiceDisabled = new RestServiceFault("Service Disabled", "1020");

        [ScriptIgnore]
        public static readonly RestServiceFault IsntOwnerService = new RestServiceFault("Is not the owner of the service", "1030");

        [ScriptIgnore]
        public static readonly RestServiceFault DispatcherDoesntExist = new RestServiceFault("Dispatcher Does Not Exist", "1040");

        [ScriptIgnore]
        public static readonly RestServiceFault WasAlreadyCompleted = new RestServiceFault("Was Already Completed", "1050");

        public static RestServiceFault Custom(Exception exception, string step)
        {
            var text = new StringBuilder();
            text.Append("Couldn't ");
            text.Append(step);
            text.Append(": ");

            text.Append('[');
            text.Append(exception.Source);
            text.Append("] ");
            text.Append(exception.Message);

            var inner = exception.InnerException;
            while (inner != null)
            {
                text.Append(" >> [");
                text.Append(inner.Source);
                text.Append("] ");
                text.Append(inner.Message);

                inner = inner.InnerException;
            }
            
            return new RestServiceFault(text.ToString(), "1060");
        }
    }
}