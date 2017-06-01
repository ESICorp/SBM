namespace SBM.RestServices
{
    public class CancelRequest
    {
        public short ID_OWNER { get; set; }

        public string TOKEN { get; set; }

        public int ID_DISPATCHER { get; set; }
    }
}