using System.Net.Mail;

namespace SBM.MailIn
{
    internal class Item
    {
        public uint UID { get; private set; }
        public MailMessage Message { get; private set; }

        public Item(uint UID, MailMessage message)
        {
            this.UID = UID;
            this.Message = message;
        }
    }
}
