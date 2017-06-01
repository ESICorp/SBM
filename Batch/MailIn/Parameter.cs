using SBM.Common;
using System;
using System.Xml;

namespace SBM.MailIn
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

                var nodeSourceProvider = doc.SelectSingleNode("//Source/Provider") as XmlElement;
                var nodeSourceInput = doc.SelectSingleNode("//Source/Input") as XmlElement;
                var nodeSourceCriteria = doc.SelectSingleNode("//Source/Input/Criteria") as XmlElement;

                var nodeTargetOutput = doc.SelectSingleNode("//Target/Output") as XmlElement;

                Source = new SourceProperties()
                {
                    Host = nodeSourceProvider.GetAttribute("Host"),
                    Port = Convert.ToInt16(nodeSourceProvider.GetAttribute("Port")),
                    SSL = Convert.ToBoolean(nodeSourceProvider.GetAttribute("SSL")),
                    Username = nodeSourceProvider.GetAttribute("Username"),
                    Password = nodeSourceProvider.GetAttribute("Password")
                };

                var tf = nodeSourceInput.GetAttribute("TimeFrame");
                Source.TimeFrame = string.IsNullOrWhiteSpace(tf) ? TIMEFRAME.ALL : (TIMEFRAME)Enum.Parse(typeof(TIMEFRAME), tf, true);

                var ct = nodeSourceInput.GetAttribute("Content");
                Source.Content = string.IsNullOrWhiteSpace(ct) ? CONTENT.ALL : (CONTENT)Enum.Parse(typeof(CONTENT), ct, true);

                if (nodeSourceCriteria != null)
                {
                    Source.CriteriaSubject = nodeSourceCriteria.GetAttribute("Subject");
                    Source.CriteriaBody = nodeSourceCriteria.GetAttribute("Body");
                    Source.CriteriaBodyRegex = nodeSourceCriteria.GetAttribute("BodyRegEx");
                    Source.CriteriaFrom = nodeSourceCriteria.GetAttribute("From");
                    Source.CriteriaAttach = nodeSourceCriteria.GetAttribute("Attach");
                }

                var action = string.IsNullOrWhiteSpace(nodeTargetOutput.GetAttribute("Action")) ? new[] { "EMPTY" } :
                    nodeTargetOutput.GetAttribute("Action").Split(new[]{':'}, StringSplitOptions.RemoveEmptyEntries);

                Target = new TargetProperties()
                {
                    Log = nodeTargetOutput.GetAttribute("Log"),
                    AttachFolder = nodeTargetOutput.GetAttribute("AttachFolder"),
                    AttachMask = nodeTargetOutput.GetAttribute("AttachMask"),
                    MailFolder = nodeTargetOutput.GetAttribute("MailFolder"),
                    MailMask = nodeTargetOutput.GetAttribute("MailMask"),

                    Action = (ACTION)Enum.Parse(typeof(ACTION), action[0], true),
                    ActionFolder = action.Length == 2 ? action[1] : string.Empty
                };
            }
            catch (Exception e)
            {
                throw new ArgumentException("Wrong parameter", e);
            }
        }
        internal class SourceProperties
        {
            public string Host { get; set; }
            public short Port { get; set; }
            public bool SSL { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public TIMEFRAME TimeFrame { get; set; }
            public CONTENT Content { get; set; }
            public string CriteriaSubject { get; set; } = null;
            public string CriteriaBody { get; set; } = null;
            public string CriteriaBodyRegex { get; set; } = null;
            public string CriteriaFrom { get; set; } = null;
            public string CriteriaAttach { get; set; } = null;
        }
        internal class TargetProperties
        {
            public string Log { get; set; }
            public string AttachFolder { get; set; }
            public string AttachMask { get; set; }
            public string MailFolder { get; set; }
            public string MailMask { get; set; }
            public ACTION Action { get; set; }
            public string ActionFolder { get; set; }
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
    internal enum TIMEFRAME
    {
        ALL,
        TODAY,
        YESTERDAY,
        LASTWEEK,
        LASTMONTH
    }
    internal enum CONTENT
    {
        BODY = 1,    //0001
        ATTACH = 2,  //0010
        ALL = 3      //0011
    }
    internal enum ACTION
    {
        EMPTY,
        DELETE,
        MOVETO
    }
}
