namespace SBM.RestServices
{
    public class NewServiceRequest
    {
        public short ID_OWNER { get; set; }

        public string TOKEN { get; set; }

        public short ID_SERVICE { get; set; }

        public string PARAMETERS { get; set; }

        public string ID_PRIVATE { get; set; }
    }
}