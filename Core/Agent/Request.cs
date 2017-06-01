using System.Xml;
using System;

namespace SBM.Agent
{
    public class Request
    {
        public int Dispatcher { get; private set; }
        public string Action { get; private set; }
        public string FileFullName { get; private set; }
        public string Class { get; private set; }
        public string Parameters { get; private set; }
        public string Domain { get; private set; }
        public string User { get; private set; }
        public byte[] Password { get; private set; }
        public string Raw { get; private set; }
        public bool x86 { get; private set; }
        public DateTimeOffset Last { get; private set; }
        public string Zip { get; private set; }

        public string Private {get ; private set;}
        public short Owner {get; private set; }
        public short Service { get; private set; }
        public int Timeout { get; private set; }

        public Request(string raw)
        {
            this.Raw = raw;

            var doc = new XmlDocument();

            doc.LoadXml(raw);

            this.Dispatcher = int.Parse(doc.DocumentElement.GetAttribute("Dispatcher"));
            this.Action = doc.DocumentElement.GetAttribute("Action");
            this.FileFullName = doc.DocumentElement.GetAttribute("FileFullName");
            this.Class = doc.DocumentElement.GetAttribute("Class");
            this.Parameters = doc.DocumentElement.GetAttribute("Parameter");
            this.x86 = bool.Parse(doc.DocumentElement.GetAttribute("x86"));
            this.Private = doc.DocumentElement.GetAttribute("Private");
            this.Owner = short.Parse(doc.DocumentElement.GetAttribute("Owner"));
            this.Service = short.Parse(doc.DocumentElement.GetAttribute("Service"));

            this.Timeout = int.MaxValue;
            if (!string.IsNullOrEmpty(doc.DocumentElement.GetAttribute("Timeout")))
            {
                this.Timeout = int.Parse(doc.DocumentElement.GetAttribute("Timeout"));
            }

            var security = doc.DocumentElement.SelectSingleNode("Security") as XmlElement;
            if (security != null)
            {
                this.Domain = security.GetAttribute("Domain");
                this.User = security.GetAttribute("User");
                this.Password = Convert.FromBase64String(security.GetAttribute("Password"));
            }

            var zip = doc.DocumentElement.SelectSingleNode("Zip") as XmlElement;
            if (zip != null)
            {
                this.Last = XmlConvert.ToDateTimeOffset(zip.GetAttribute("Last"));
                this.Zip = zip.InnerText;
            }
        }
    }
}
