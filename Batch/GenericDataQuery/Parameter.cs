using SBM.Common;
using System;
using System.Xml;

namespace SBM.GenericDataQuery
{
    internal class Parameter
    {
        public static SourceProperties Source { get; private set; }
        public static TargetProperties Target { get; private set; }

        public static void Parse(BatchContext context)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(context.PARAMETERS);

                var nodeSource = doc.SelectSingleNode("//Source") as XmlElement;
                var nodeSourceProvider = doc.SelectSingleNode("//Source/Provider") as XmlElement;
                var nodeSourceInput = doc.SelectSingleNode("//Source/Input") as XmlElement;

                var nodeTarget = doc.SelectSingleNode("//Target") as XmlElement;
                var nodeTargetProvider = doc.SelectSingleNode("//Target/Provider") as XmlElement;
                var nodeTargetOutput = doc.SelectSingleNode("//Target/Output") as XmlElement;

                Source = new SourceProperties()
                {
                    Type = nodeSource.GetAttribute("Type"),
                    Provider = ReeplaceWithContext(nodeSourceProvider.InnerText, context),
                    Input = ReeplaceWithContext(string.IsNullOrEmpty(nodeSourceProvider.InnerText) ? nodeSourceInput.OuterXml : nodeSourceInput.InnerText, context),
                    Delimiter = nodeSource.GetAttribute("Delimiter")
                };

                Target = new TargetProperties()
                {
                    Type = nodeTarget.GetAttribute("Type"),
                    Provider = ReeplaceWithContext(nodeTargetProvider.InnerText, context),
                    Output = ReeplaceWithContext(nodeTargetOutput.InnerText, context),
                    Log = ReeplaceWithContext(nodeTargetOutput.GetAttribute("Log"), context),
                    Append = Convert.ToBoolean(nodeTargetOutput.HasAttribute("Append") ? nodeTargetOutput.GetAttribute("Append") : "false")
                };
            }
            catch (Exception e)
            {
                throw new ArgumentException("Wrong parameter", e);
            }

            if ((string.Equals(Source.Type, "XML", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Target.Type, "XML", StringComparison.OrdinalIgnoreCase)) ||
                (string.Equals(Source.Type, "TXT", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Target.Type, "TXT", StringComparison.OrdinalIgnoreCase)) ||
                (string.Equals(Source.Type, "XLS", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Target.Type, "XLS", StringComparison.OrdinalIgnoreCase)))
            {
                throw new NotSupportedException("Source/Target types not supported");
            }
        }

        internal class SourceProperties
        {
            public string Type { get; set; }
            public string Provider { get; set; }
            public string Input { get; set; }
            public string Delimiter { get; set; }
        }
        internal class TargetProperties
        {
            public string Type { get; set; }
            public string Provider { get; set; }
            public string Output { get; set; }
            public string Log { get; set; }
            public bool Append { get; set; }
        }

        private static string ReeplaceWithContext(string inputText, BatchContext context)
        {
            return inputText == null ? null : inputText
                    .Replace("[DOMAIN]", context.DOMAIN)
                    .Replace("[USERNAME]", context.USERNAME)
                    .Replace("[ID_DISPATCHER]", context.ID_DISPATCHER.ToString())
                    .Replace("[ID_OWNER]", context.ID_OWNER.ToString())
                    .Replace("[ID_PRIVATE]", context.ID_PRIVATE)
                    .Replace("[ID_SERVICE]", context.ID_SERVICE.ToString())
                    .Replace("[PARAMETERS]", context.PARAMETERS);
        }

    }
}
