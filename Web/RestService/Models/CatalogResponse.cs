using System.Collections.Generic;

namespace SBM.RestServices
{
    public class CatalogResponse : DispatcherResponse
    {
        public List<Service> Services { get; set; }

        public class Service
        {
            public short ID_SERVICE { get; set; }

            public string DESCRIPTION { get; set; }

            public ServiceType SERVICE_TYPE { get; set; }

            public byte SECURITY_LEVEL { get; set; }

            public bool ENABLED { get; set; }

            public class ServiceType
            {
                public short ID_SERVICE_TYPE { get; set; }

                public string DESCRIPTION { get; set; }
            }
        }
    }
}