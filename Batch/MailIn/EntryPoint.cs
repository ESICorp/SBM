using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using S22.Imap;
using SBM.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SBM.MailIn
{
    public class EntryPoint : Batch
    {
#if FALSE
        public static void Main(string[] args)
        {
            var xml =
            "<Parameters>" +
               "<Source>" +
               "   <Provider Host='imap.gmail.com' Port='993' SSL='true' Username='acastiglia@gmail.com' Password='Sanguineti1' />" +
               "   <Input TimeFrame='lastmonth' Content='all'>" +
               "      <Criteria Subject='' Body='' BodyRegEx='' From='noresponder@administracionglobal.com' Attach='' />" +
               "   </Input>" +
               "</Source>" +
               "<Target>" +
               "   <Output Log='' AttachFolder='' AttachMask='' MailFolder='' MailMask='' Action='moveTo:Lobos' />" +
               "</Target>" +
            "</Parameters>";

            new EntryPoint().Submit(xml);
        }
#endif

        private ImapClient ImapClient { get; set; }
        private IEnumerator<Item> Items { get; set; }
        private int Count { get; set; }
        private XmlDocument xml { get; set; } = new XmlDocument();

        public override void Init()
        {
            Parameter.Parse(Context);

            TraceLog.Configure();

            this.xml.AppendChild(xml.CreateElement("Emails"));

            this.ImapClient = new ImapClient(Parameter.Source.Host, Parameter.Source.Port,
                    Parameter.Source.Username, Parameter.Source.Password, AuthMethod.Login, Parameter.Source.SSL);

            #region Filter
            var condition = (Parameter.Target.Action == ACTION.EMPTY) ? SearchCondition.Unseen() : null;

            switch (Parameter.Source.TimeFrame)
            {
                case TIMEFRAME.TODAY:
                    condition = SearchCondition.SentOn(DateTime.Today);
                    break;
                case TIMEFRAME.YESTERDAY:
                    condition = SearchCondition.SentOn(DateTime.Today.AddDays(-1));
                    break;
                case TIMEFRAME.LASTWEEK:
                    condition = SearchCondition.SentSince(DateTime.Today.AddDays(-7));
                    break;
                case TIMEFRAME.LASTMONTH:
                    condition = SearchCondition.SentSince(DateTime.Today.AddMonths(-1));
                    break;
            }

            if (!string.IsNullOrWhiteSpace(Parameter.Source.CriteriaSubject))
            {
                var aux = SearchCondition.Subject(Parameter.Source.CriteriaSubject);
                condition = condition == null ? aux : condition.And(aux);
            }

            if (!string.IsNullOrWhiteSpace(Parameter.Source.CriteriaBody))
            {
                var aux = SearchCondition.Body(Parameter.Source.CriteriaBody);
                condition = condition == null ? aux : condition.And(aux);
            }

            if (!string.IsNullOrWhiteSpace(Parameter.Source.CriteriaFrom))
            {
                var aux = SearchCondition.From(Parameter.Source.CriteriaFrom);
                condition = condition == null ? aux : condition.And(aux);
            }

            var uids = this.ImapClient.Search(condition ?? SearchCondition.All())
                .Select(uid => new Item(uid, this.ImapClient.GetMessage(uid)));

            Regex bodyRegex = string.IsNullOrWhiteSpace(Parameter.Source.CriteriaBodyRegex) ? null :
                new Regex(Parameter.Source.CriteriaBodyRegex);

            this.Items = uids.Where(tuple =>
            {
                if (bodyRegex != null)
                {
                    if (!bodyRegex.IsMatch(tuple.Message.Body)) return false;
                }
                if ((Parameter.Source.Content & CONTENT.ATTACH) > 0 && !string.IsNullOrWhiteSpace(Parameter.Source.CriteriaAttach))
                {
                    if (!tuple.Message.Attachments.Any(a => Operators.LikeString(a.Name.ToLower(), Parameter.Source.CriteriaAttach.ToLower(), CompareMethod.Text))) return false;
                }
                return true;

            }).GetEnumerator();

            #endregion
        }

        public override object Read()
        {
            if (this.Items.MoveNext())
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
            var tuple = source as Item;

            var emailNode = xml.CreateElement("Email");

            try
            {
                emailNode.SetAttribute("From", tuple.Message.From.ToString());
                emailNode.SetAttribute("To", tuple.Message.To.ToString());
                emailNode.SetAttribute("CC", tuple.Message.CC.ToString());
                emailNode.SetAttribute("Subject", tuple.Message.Subject);
                emailNode.SetAttribute("Body", tuple.Message.Body);

                if ((Parameter.Source.Content & CONTENT.ATTACH) > 0 &&
                     !string.IsNullOrWhiteSpace(Parameter.Target.AttachFolder))
                {
                    var attachments = new List<string>();

                    tuple.Message.Attachments
                         .Where(a => string.IsNullOrWhiteSpace(Parameter.Source.CriteriaAttach) || Operators.LikeString(a.Name.ToLower(), Parameter.Source.CriteriaAttach.ToLower(), CompareMethod.Text)).ToList()
                         .ForEach(a =>
                         {
                             var path = string.IsNullOrWhiteSpace(Parameter.Target.AttachMask) ?
                                 Path.Combine(Parameter.Target.AttachFolder, a.Name).DontOverwrite() :
                                 Path.Combine(Parameter.Target.AttachFolder, Parameter.Target.AttachMask).Wilcard();

                             using (var f = new FileStream(path, FileMode.Create, FileAccess.Write))
                             {
                                 a.ContentStream.CopyTo(f);
                             }

                             attachments.Add(path);
                         });

                    emailNode.SetAttribute("AttachFolder", Parameter.Target.AttachFolder);
                    emailNode.SetAttribute("AttachFiles", string.Join(";", attachments.Select(_ => Path.GetFileName(_))));
                }

                switch (Parameter.Target.Action)
                {
                    case ACTION.DELETE:
                        
                        this.ImapClient.DeleteMessage(tuple.UID);
                        break;

                    case ACTION.MOVETO:
                        
                        this.ImapClient.MoveMessage(tuple.UID, Parameter.Target.ActionFolder);
                        break;

                    default:
                        this.ImapClient.SetMessageFlags(tuple.UID, null, MessageFlag.Seen);
                        break;
                }
            }
            catch (Exception e)
            {
                TraceLog.AddError("Couldn't process item", e);
            }

            return emailNode;
        }

        public override void Write(object @object)
        {
            var emailNode = @object as XmlElement;

            this.xml.DocumentElement.AppendChild(emailNode);
        }

        public override void Destroy()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Parameter.Target.MailFolder))
                {
                    var path = Path.Combine(Parameter.Target.MailFolder,
                         string.IsNullOrWhiteSpace(Parameter.Target.MailMask) ? "eMails[YYYYMMDD]-[##].xml" : Parameter.Target.MailMask).Wilcard();

                    this.xml.Save(path);
                }

                RESPONSE = (this.xml == null) ? string.Empty : this.xml.OuterXml;

                if (TraceLog.Count > 0)
                {
                    throw new Exception(string.Format("Processed {0}; {1} exceptions", this.Count, TraceLog.Count), TraceLog.Exceptions);
                }
                else if (string.IsNullOrWhiteSpace(Parameter.Target.MailFolder) && RESPONSE.Length > 4000)
                {
                    throw new OverflowException(RESPONSE.Substring(0, 4000));
                }
            }
            finally
            {
                if (this.ImapClient != null)
                {
                    this.ImapClient.Dispose();
                }

                TraceLog.Dispose();
            }
        }
    }
}
