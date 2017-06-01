using SBM.Model;
using SevenZip;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace SBM.Service
{
    public class BatchHandlerTCP : BatchHandler
    {
        /// <summary>
        /// External Service
        /// </summary>
        private ConnectionTCP connectionTCP;

        public BatchHandlerTCP(string server)
            : base()
        {
            var tuple = server.Split(':');
            var ip = tuple[0];
            var port = tuple.Length > 1 ? int.Parse(tuple[1]) : 4921;

            this.connectionTCP = new ConnectionTCP(ip, port);

            base.PID = string.Format("{0}", connectionTCP.LocalPort);
        }

        /// <summary>
        /// Internal use
        /// </summary>
        public override string Submit()
        {
            Log.Debug("SBM.Service [BatchHandlerTCP.Submit] Submit " + base.BatchEventArgs.Parameters);

            string result = string.Empty;
            try
            {
                var docRequest = new XmlDocument();
                var docResponse = new XmlDocument();

                var element = docRequest.CreateElement("Request");
                docRequest.AppendChild(element);

                docRequest.InsertBefore(docRequest.CreateXmlDeclaration("1.0", "UTF-8", null),
                    docRequest.DocumentElement);

                element.SetAttribute("Dispatcher", base.BatchEventArgs.Dispatcher.ToString());
                element.SetAttribute("Action", "Submit");
                element.SetAttribute("FileFullName", this.FullName);
                element.SetAttribute("Class", this.Type);
                element.SetAttribute("Parameter", base.BatchEventArgs.Parameters ?? "");
                element.SetAttribute("x86", base.BatchEventArgs.x86.ToString());

                if (!string.IsNullOrEmpty(base.BatchEventArgs.UserName) && base.BatchEventArgs.Password != null)
                {
                    var security = docRequest.CreateElement("Security");
                    element.AppendChild(security);
                    security.SetAttribute("Domain", base.BatchEventArgs.Domain ?? string.Empty);
                    security.SetAttribute("User", base.BatchEventArgs.UserName);
                    security.SetAttribute("Password", Convert.ToBase64String(base.BatchEventArgs.Password));
                }

                element.SetAttribute("Private", base.BatchEventArgs.Private ?? "");
                element.SetAttribute("Owner", base.BatchEventArgs.Owner.ToString());
                element.SetAttribute("Service", base.BatchEventArgs.Service.ToString());
                element.SetAttribute("Timeout", base.BatchEventArgs.Timeout.ToString());

                var zip = docRequest.CreateElement("Zip");
                element.AppendChild(zip);
                zip.SetAttribute("Last", LastWrite(this.FullName));
                zip.InnerText = Compress(this.FullName);

                this.connectionTCP.Send(docRequest.OuterXml);

                result = this.connectionTCP.Read(TimeSpan.FromSeconds(base.BatchEventArgs.Timeout));

                if (!string.IsNullOrEmpty(result))
                {
                    docResponse.LoadXml(result);

                    result = docResponse.DocumentElement.InnerText;

                    var error = docResponse.SelectSingleNode("//Error") as XmlElement;
                    if (error != null)
                    {
                        throw ExceptionHelper.Build(error);
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BatchHandlerTCP.Submit]", e);

                throw;
            }

            return result;
        }

        public override void Cancel(object state)
        {
            try
            {
                if (this.connectionTCP != null)
                {
                    this.connectionTCP.Send(
                        string.Format("<?xml version='1.0' encoding='UTF-8'?><Request Action='Cancel' Dispatcher='{0}'/>", base.BatchEventArgs.Dispatcher));

                    var result = this.connectionTCP.Read(Consts.CommunicationTimeout);

                    if (!string.IsNullOrEmpty(result))
                    {
                        var docResponse = new XmlDocument();
                        docResponse.LoadXml(result);

                        var error = docResponse.SelectSingleNode("//Error") as XmlElement;
                        if (error != null)
                        {
                            throw ExceptionHelper.Build(error);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BatchHandlerTCP.Cancel]", e);
            }
        }

        public override void Close()
        {
            try
            {
                if (this.connectionTCP != null)
                {
                    this.connectionTCP.Dispose();
                    this.connectionTCP = null;
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BatchHandlerTCP.Release]", e);
            }
        }

        private string LastWrite(string full)
        {
            return XmlConvert.ToString(
                new FileInfo(full).Directory.GetFiles().Max(_ => _.LastWriteTimeUtc),
                XmlDateTimeSerializationMode.Utc);
        }

        private string Compress(string full)
        {
            var directory = new FileInfo(full).Directory.FullName;

            SevenZipExtractor.SetLibraryPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "7z64.dll"));

            var compressor = new SevenZipCompressor();

            using (var m = new MemoryStream()) {

                compressor.CompressDirectory(directory, m);

                return Convert.ToBase64String(m.ToArray());
            }
        }
    }
}
