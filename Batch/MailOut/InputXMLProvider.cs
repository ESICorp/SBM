using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Mail;
using System.Xml;

namespace SBM.MailOut
{
    internal class InputXMLProvider : InputProvider
    {
        private NameValueCollection alias = new NameValueCollection();

        public InputXMLProvider() : base()
        {
            foreach (var item in Parameter.Source.Input.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = item.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (pair.Length == 2)
                {
                    //{nodo1,From}{nodo2,To}
                    alias.Add(pair[1], pair[0]);
                }
            }
        }

        public override IEnumerator GetMails()
        {
            var xml = new XmlDocument();
            xml.Load(Parameter.Source.Provider);

            var list = new List<Item>();

            foreach (XmlElement node in xml.DocumentElement.ChildNodes)
            {
                var msg = CreateMessage();

                var aux = GetValue(node, "From");
                if (!string.IsNullOrWhiteSpace(aux))
                {
                    msg.From = new MailAddress(aux);
                }

                aux = GetValue(node, "To");
                foreach (var to in aux.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    msg.To.Add(new MailAddress(to));
                }

                aux = GetValue(node, "CC");
                if (!string.IsNullOrWhiteSpace(aux))
                {
                    foreach (var cc in aux.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        msg.CC.Add(new MailAddress(cc));
                    }
                }

                aux = GetValue(node, "BCC");
                if (!string.IsNullOrWhiteSpace(aux))
                {
                    foreach (var bcc in aux.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        msg.Bcc.Add(new MailAddress(bcc));
                    }
                }

                aux = GetValue(node, "Reply");
                if (!string.IsNullOrWhiteSpace(aux))
                {
                    foreach (var reply in aux.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        msg.ReplyToList.Add(new MailAddress(reply));
                    }
                }

                msg.Subject = GetValue(node, "Subject").ReeplaceWithContext(Context);
                msg.Body = GetValue(node, "Body").ReeplaceWithContext(Context);

                var folder = GetValue(node, "AttachFolder");
                var files = GetValue(node, "AttacheFiles");
                if (!string.IsNullOrWhiteSpace(folder) && !string.IsNullOrWhiteSpace(files))
                {
                    foreach (var fileName in files.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        msg.Attachments.Add(new Attachment(Path.Combine(folder, fileName.Trim())));
                    }
                }

                list.Add(new Item(msg));
            }

            return list.GetEnumerator();
        }

        private string GetValue(XmlElement node, string name)
        {
            var newName = alias[name];

            if (string.IsNullOrWhiteSpace(newName)) newName = name;

            var value = node.GetAttribute(newName);

            if (string.IsNullOrWhiteSpace(value))
            {
                var child = node.SelectSingleNode("./" + newName);

                if ( child != null )
                {
                    value = child.InnerText;
                }
            }

            return value;
        }
    }
}

