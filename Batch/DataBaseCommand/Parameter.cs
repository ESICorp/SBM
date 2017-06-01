using System;
using System.Xml;

namespace SBM.DataBaseCommand
{
    internal class Parameter
    {
        public static SourceProperties Source { get; private set; }

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

                var nodeSource = doc.SelectSingleNode("//Source") as XmlElement;
                var nodeSourceProvider = doc.SelectSingleNode("//Source/Provider") as XmlElement;
                var nodeSourceCommand = doc.SelectSingleNode("//Source/Command") as XmlElement;

                Source = new SourceProperties()
                {
                    Type = nodeSource.GetAttribute("Type"),
                    Provider = nodeSourceProvider.InnerText,
                    Command = nodeSourceCommand.InnerText,
                };

                var nodeLog = doc.SelectSingleNode("//Log") as XmlElement;
                Log = nodeLog.InnerText;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Wrong parameter", e);
            }
        }

        internal class SourceProperties
        {
            public string Type { get; set; }
            public string Provider { get; set; }
            public string Command { get; set; }
        }
    }
}
