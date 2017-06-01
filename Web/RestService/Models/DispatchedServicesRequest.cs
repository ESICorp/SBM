using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace SBM.RestServices
{
    public class DispatchedServicesRequest
    {
        public short ID_OWNER { get; set; }

        public string TOKEN { get; set; }

        [JsonProperty(ItemConverterType = typeof(IsoDateTimeConverter))]
        public DateTimeOffset? DATE_FROM { get; set; }

        [JsonProperty(ItemConverterType = typeof(IsoDateTimeConverter))]
        public DateTimeOffset? DATE_TO { get; set; }

        public string ID_PRIVATE { get; set; }
    }
}