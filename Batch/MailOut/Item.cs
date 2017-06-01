using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SBM.MailOut
{
    internal class Item
    {
        public MailMessage Message { get; private set; }
        public DataRow Row { get; private set; }

        public Item(MailMessage Message)
        {
            this.Message = Message;
            this.Row = null;
        }

        public Item(MailMessage Message, DataRow Row)
        {
            this.Message = Message;
            this.Row = Row;
        }

    }
}
