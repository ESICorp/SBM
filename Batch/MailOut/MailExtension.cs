using DKIM;
using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace SBM.MailOut
{
    public static class MailExtension
    {
        public static byte[] ToEml(this MailMessage mail)
        {
            using (var stream = new MemoryStream())
            {
                var mailWriterType = mail.GetType().Assembly.GetType("System.Net.Mail.MailWriter");

                var mailWriter = Activator.CreateInstance(
                                    type: mailWriterType,
                                    bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic,
                                    binder: null,
                                    args: new object[] { stream },
                                    culture: null,
                                    activationAttributes: null);

                mail.GetType().InvokeMember(
                                    name: "Send",
                                    invokeAttr: BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                                    binder: null,
                                    target: mail,
                                    args: new object[] { mailWriter, true, true });

                //return Encoding.UTF8.GetString(stream.ToArray());
                return stream.ToArray();
            }
        }

        public static MailMessage DKim(this MailMessage msg)
        {
            if (string.IsNullOrEmpty(Parameter.Target.DKimPrivateKey) ||
                string.IsNullOrEmpty(Parameter.Target.DKimDomain))
            {
                return msg;
            }
            else
            {
                var privateKey = PrivateKeySigner.Create(Parameter.Target.DKimPrivateKey);

                var domainkey = new DomainKeySigner(privateKey, Parameter.Target.DKimDomain, "domainkey",
                    new string[] { "From", "To", "Subject" });

                msg = msg.DomainKeySign(domainkey);

                var dkim = new DkimSigner(privateKey, Parameter.Target.DKimDomain, "dkim",
                    new string[] { "From", "To", "Subject" });

                return msg.DkimSign(dkim);
            }
        }

    }
}
