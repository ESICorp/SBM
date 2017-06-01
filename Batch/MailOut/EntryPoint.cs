using SBM.Common;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace SBM.MailOut
{
    public class EntryPoint : Batch
    {
#if FALSE
        public static void Main(string[] args)
        {
            var xml = string.Format(
            "<Parameters>" +
               "<Source>" +
               "   <Provider Type='{0}'>{1}</Provider>" +
               "   <Input {2}>" +
               "      {3}" +
               "   </Input>" +
               "</Source>" +
               "<Target>" +
               //"   <Provider Host='smtp.gmail.com' Port='465' UserName='acastiglia@gmail.com' Password='Sanguineti1' Ssl='true' />" +
               "   <Output Log='c:\\temp\\andres\\mailout.txt' MailFolder='c:\\temp\\andres' MailMask='mail[#].eml'>" +
               "      {4}" +
               "   </Output>" +
               "</Target>" +
            "</Parameters>",

            //"",
            //"",
            //"From='acastiglia@gmail.com' To='andrescastiglia@hotmail.com' CC='andres_castiglia@yahoo.com.ar' BCC='patusy@yahoo.com.ar' Subject= 'Subject Prueba 1' Body='Body Prueba 1' AttachFolder='c:\\temp\\andres' AttachFiles='dispatcher.jpg, debugviewer.txt'",
            //"",
            //""

            //"sql",
            //"data source=.\\INSTANCIA2;initial catalog=dele;user id=sa;password=password;",
            //"",
            //"select id, destino as [to], asunto as [subject], mensaje as [body] from mailout where enviado is null",
            //"update mailout set enviado = getdate() where id = @id"

            "xml",
            @"c:\temp\andres\mailout.xml",
            "",
            "{destino,To}{asunto,Subject}{mensaje,Body}",
            ""
            );

            new EntryPoint().Submit(xml);
        }
#endif
        private InputProvider InputProvider { get; set; }
        private SmtpClient SmtpClient { get; set; }
        private IEnumerator Items { get; set; }
        private int Count { get; set; }
        private int Updated { get; set; }

        public override void Init()
        {
            Parameter.Parse(Context);

            TraceLog.Configure();

            this.InputProvider = InputProvider.GetProvider(Context);

            this.SmtpClient = new SmtpClient();

            if (!string.IsNullOrWhiteSpace(Parameter.Target.Host))
            {
                this.SmtpClient.Host = Parameter.Target.Host;
            }

            if (Parameter.Target.Port.HasValue)
            {
                this.SmtpClient.Port = Parameter.Target.Port.Value;
            }

            if (Parameter.Target.SSL.HasValue)
            {
                this.SmtpClient.EnableSsl = Parameter.Target.SSL.Value;
            }

            if (!string.IsNullOrWhiteSpace(Parameter.Target.Username) &&
                !string.IsNullOrWhiteSpace(Parameter.Target.Password))
            {
                this.SmtpClient.Credentials = string.IsNullOrWhiteSpace(Parameter.Target.Domain) ?
                    new NetworkCredential(Parameter.Target.Username, Parameter.Target.Password) :
                    new NetworkCredential(Parameter.Target.Username, Parameter.Target.Password, Parameter.Target.Domain);
            }

            this.Items = this.InputProvider.GetMails();
        }

        public override object Read()
        {
            if ( this.Items.MoveNext() )
            {
                this.Count++;

                return this.Items.Current;
            }
            else
            {
                return null;
            }
        }
        public override object Process(object source)
        {
            Item item = source as Item;

            return new Item(item.Message.DKim(), item.Row);
        }

        public override void Write(object @object)
        {
            Item item = @object as Item;

            string step = "send mail";
            try
            {
                this.SmtpClient.Send(item.Message);

                step = "udpate source";

                this.Updated += this.InputProvider.Update(item.Row);

                if (!string.IsNullOrWhiteSpace(Parameter.Target.MailFolder))
                {
                    step = "save message";

                    string file = Path.Combine(Parameter.Target.MailFolder, Parameter.Target.MailMask).Wilcard();

                    File.WriteAllBytes(file, item.Message.ToEml());
                }
            }
            catch (Exception e)
            {
                TraceLog.AddError("Couldn't " + step, e);
            }
        }

        public override void Destroy()
        {
            try
            {
                RESPONSE = string.Format("{{Sent:{0},Updated:{1},Exceptions:{2}}}", this.Count, this.Updated, TraceLog.Count);

                if (TraceLog.Count > 0)
                {
                    throw new Exception(RESPONSE, TraceLog.Exceptions);
                }
            }
            finally
            {
                if (this.SmtpClient != null)
                {
                    this.SmtpClient.Dispose();
                }

                TraceLog.Dispose();
            }
        }
    }
}
