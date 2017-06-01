using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace SBM.Schedule
{
    [Serializable]
    public class ParseException : Exception
    {
        public ParseException() :
            base() { }

        public ParseException(string message) :
            base(message) { }

        public ParseException(string message, params object[] args) :
            base(string.Format(CultureInfo.InvariantCulture, message, args))
        { }

        public ParseException(string message, Exception innerException) :
            base(message, innerException) { }

        public ParseException(Exception innerException, string message, params object[] args) :
        base(string.Format(CultureInfo.InvariantCulture, message, args), innerException)
        { }

        protected ParseException(SerializationInfo info, StreamingContext context) :
            base(info, context) { }
    }
}