namespace SBM.Common
{
    public class BatchContext
    {
        public string PARAMETERS { get; set; }
        public string ID_PRIVATE { get; set; }
        public short ID_OWNER { get; set; }
        public short ID_SERVICE { get; set; }
        public int ID_DISPATCHER { get; set; }
        public string DOMAIN { get; set; }
        public string USERNAME { get; set; }
        public byte[] PASSWORD { get; set; }
        public int TIMEOUT { get; set; }
        public string RESPONSE { get; set; }
    }
}
