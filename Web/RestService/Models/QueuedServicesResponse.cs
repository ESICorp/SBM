using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace SBM.RestServices
{
    public class QueuedServicesResponse : DispatcherResponse
    {
        public List<Service> Services { get; set; }

        public class Service
        {
            public int ID_DISPATCHER { get; set; }

            public short ID_SERVICE { get; set; }

            public string DESCRIPTION { get; set; }

            public ServiceType SERVICE_TYPE { get; set; }

            public byte SECURITY_LEVEL { get; set; }

            public string ID_PRIVATE { get; set; }
            public string PARAMETERS { get; set; }

            [JsonProperty(ItemConverterType = typeof(IsoDateTimeConverter))]
            public DateTimeOffset? REQUESTED { get; set; }

            public DoneStatus DONE_STATUS { get; set; }

            [JsonProperty(ItemConverterType = typeof(IsoDateTimeConverter))]
            public DateTimeOffset? STARTED { get; set; }

            public class ServiceType
            {
                public short ID_SERVICE_TYPE { get; set; }

                public string DESCRIPTION { get; set; }
            }

            public class DoneStatus
            {
                public byte ID_DONE_STATUS { get; set; }

                public string DESCRIPTION { get; set; }
            }
        }
    }
}