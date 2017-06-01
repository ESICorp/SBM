using SBM.Common;
using System;
using System.Configuration;
using System.Xml;

namespace SBM.MailOut
{
/*
    <Parameters>
        <Source Type="{mssql|xml}">
            <Provider>{connection string|c:\archivo.xml}<Provider/>
            <Input From="" To="" CC="" BCC="" Reply="" Subject="" Body="" AttachFolder="" AttachFiles="">
                select col1 as [pk], col2 as [To] from tabla1
                {nodo1,From}{nodo2,To}
            </Input>
        </Source>
        <Target>
            <Provider Host="smtp.gmail.com" Port="465" Domain='' UserName="acastiglia@gmail.com" Password="Sanguineti1" EnableSsl="true" />
            <DKim PrivateKey="" Domain="" />
            <Output Log='' MailFolder='' MailMask=''>
                update tabla1 set col3 = true where col1 = :pk 
            </Output>
        </Target>
    </Parameters>
*/
    internal static class Parameter
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

                var nodeTargetProvider = doc.SelectSingleNode("//Target/Provider") as XmlElement;
                var nodeTargetDKim = doc.SelectSingleNode("//Target/DKim") as XmlElement;
                var nodeTargetOutput = doc.SelectSingleNode("//Target/Output") as XmlElement;

                Source = new SourceProperties();

                if (nodeSourceProvider != null)
                {
                    Source.Provider = nodeSourceProvider.InnerText;

                    var t = nodeSourceProvider.GetAttribute("Type");
                    Source.Source = string.IsNullOrWhiteSpace(t) ? SOURCE.NONE : (SOURCE)Enum.Parse(typeof(SOURCE), t, true);
                };

                if (nodeSourceInput != null)
                {
                    Source.From = nodeSourceInput.GetAttribute("From");
                    Source.To = nodeSourceInput.GetAttribute("To");
                    Source.CC = nodeSourceInput.GetAttribute("CC");
                    Source.BCC = nodeSourceInput.GetAttribute("BCC");
                    Source.Reply = nodeSourceInput.GetAttribute("Reply");
                    Source.Subject = nodeSourceInput.GetAttribute("Subject");
                    Source.Body = nodeSourceInput.GetAttribute("Body");
                    Source.AttachFolder = nodeSourceInput.GetAttribute("AttachFolder");
                    Source.AttachFiles = nodeSourceInput.GetAttribute("AttachFiles");
                    Source.Input = nodeSourceInput.InnerText;
                }

                Target = new TargetProperties();

                if (nodeTargetProvider != null)
                {
                    Target.Host = nodeTargetProvider.GetAttribute("Host");
                    Target.Domain = nodeTargetProvider.GetAttribute("Domain");
                    Target.Username = nodeTargetProvider.GetAttribute("Username");
                    Target.Password = nodeTargetProvider.GetAttribute("Password");

                    var port = nodeTargetProvider.GetAttribute("Port");
                    Target.Port = string.IsNullOrWhiteSpace(port) ? (short?)null : Convert.ToInt16(port);

                    var ssl = nodeTargetProvider.GetAttribute("SSL");
                    Target.SSL = string.IsNullOrWhiteSpace(ssl) ? (bool?)null : Convert.ToBoolean(ssl);
                }

                if ( nodeTargetOutput != null )
                {
                    Target.MailFolder = nodeTargetOutput.GetAttribute("MailFolder");
                    Target.MailMask = nodeTargetOutput.GetAttribute("MailMask");

                    Target.Log = nodeTargetOutput.GetAttribute("Log");
                    Target.Update = nodeTargetOutput.InnerText;
                }

                if (nodeTargetDKim != null)
                {
                    Target.DKimDomain = nodeTargetDKim.GetAttribute("Domain");
                    Target.DKimPrivateKey = nodeTargetDKim.GetAttribute("PrivateKey");
                }
                
                Target.DKimDomain = string.IsNullOrWhiteSpace(Target.DKimDomain) ?
                    ConfigurationManager.AppSettings["DKimDomain"] : Target.DKimDomain;

                Target.DKimPrivateKey = string.IsNullOrWhiteSpace(Target.DKimPrivateKey) ?
                    ConfigurationManager.AppSettings["DKimPrivateKey"] : Target.DKimPrivateKey;
            }
            catch (Exception e)
            {
                throw new ArgumentException("Wrong parameter", e);
            }
        }
        internal class SourceProperties
        {
            public string Provider { get; set; }
            public SOURCE Source { get; set; } = SOURCE.NONE;
            public string Input { get; set; } 
            public string From { get; set; }
            public string To { get; set; } 
            public string CC { get; set; } 
            public string BCC { get; set; }
            public string Reply { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; } 
            public string AttachFolder { get; set; }
            public string AttachFiles { get; set; } 
        }
        internal class TargetProperties
        {
            public string Host { get; set; }
            public short? Port { get; set; } = null;
            public bool? SSL { get; set; } = null;
            public string Domain { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Log { get; set; }

            public string DKimPrivateKey { get; set; }
            public string DKimDomain { get; set; }

            public string MailFolder { get; set; }
            public string MailMask { get; set; }

            public string Update { get; set; }
        }

        public static string ReeplaceWithContext(this string inputText, BatchContext context)
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

    public enum SOURCE
    {
        NONE,
        MSSQL,
        SQL,
        XML
    }
}
