using System;
using System.Xml;

namespace SBM.Filer
{
    internal class Parameter
    {
        public static TargetProperties Target { get; private set; }

        public static string Log { get; private set; }

        private Parameter()
        {
        }

        public static void Parse(string xml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var nodeTarget = doc.SelectSingleNode("//Target") as XmlElement;
                var nodeTargetCommand = doc.SelectSingleNode("//Target/Command") as XmlElement;

                Target = new TargetProperties()
                {
                    Computer = nodeTarget.GetAttribute("Computer"),
                    Username = nodeTarget.GetAttribute("Username"),
                    Password = nodeTarget.GetAttribute("Password"),
                    Command = nodeTargetCommand.InnerText,
                };

                var nodeLog = doc.SelectSingleNode("//Log") as XmlElement;
                Log = nodeLog.InnerText;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Wrong parameter", e);
            }
        }

        internal class TargetProperties
        {
            public string Computer { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Command { get; set; }
        }
    }
}
