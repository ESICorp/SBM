using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace SBM.MailOut
{
    internal class InputSQLProvider : InputProvider
    {
        public InputSQLProvider() : base()
        {
        }

        public override IEnumerator GetMails()
        {
            var table = new DataTable();

            using (var adapter = new SqlDataAdapter(Parameter.Source.Input, Parameter.Source.Provider))
            {
                adapter.Fill(table);
            }

            var list = new List<Item>();

            foreach (DataRow row in table.Rows)
            {
                var msg = CreateMessage();

                if (table.Columns.Contains("From"))
                {
                    msg.From = new MailAddress(row["From"].ToStringNotNull());
                }

                foreach (var to in row["To"].ToArrayNotNull())
                {
                    msg.To.Add(new MailAddress(to));
                }

                if (table.Columns.Contains("CC"))
                {
                    foreach (var cc in row["CC"].ToArrayNotNull())
                    {
                        msg.CC.Add(new MailAddress(cc));
                    }
                }

                if (table.Columns.Contains("BCC"))
                {
                    foreach (var bcc in row["BCC"].ToArrayNotNull())
                    {
                        msg.Bcc.Add(new MailAddress(bcc));
                    }
                }

                if (table.Columns.Contains("Reply"))
                {
                    foreach (var reply in row["Reply"].ToArrayNotNull())
                    {
                        msg.ReplyToList.Add(new MailAddress(reply));
                    }
                }

                msg.Subject = row["Subject"].ToStringNotNull().ReeplaceWithContext(Context);
                msg.Body = row["Body"].ToStringNotNull().ReeplaceWithContext(Context);

                if (table.Columns.Contains("AttachFolder") && table.Columns.Contains("AttachFiles"))
                {
                    foreach (var fileName in row["AttachFiles"].ToArrayNotNull())
                    {
                        msg.Attachments.Add(new Attachment(Path.Combine(row["AttachFolder"].ToStringNotNull(), fileName.Trim())));
                    }
                }

                list.Add(new Item(msg, row));
            }

            return list.GetEnumerator();
        }

        public override int Update(DataRow row)
        {
            var regex = new Regex(@"\@\w+");

            using (var cnn = new SqlConnection(Parameter.Source.Provider))
            {
                cnn.Open();

                using (var cmd = new SqlCommand(Parameter.Target.Update, cnn))
                {
                    foreach (Match match in regex.Matches(Parameter.Target.Update))
                    {
                        var name = match.Value.Substring(1);
                        cmd.Parameters.AddWithValue(match.Value, row[name]);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
