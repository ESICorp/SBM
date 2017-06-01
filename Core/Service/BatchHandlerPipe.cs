using SBM.Model;
using System;
using System.Xml;

namespace SBM.Service
{
    /// <summary>
    /// Abstract class to implement the Bussines Logic
    /// </summary>
    public class BatchHandlerPipe : BatchHandler
    {
        /// <summary>
        /// External Process
        /// </summary>
        private ConnectionPipe connectionPipe;

        public BatchHandlerPipe(bool x86)
            : base()
        {
            this.connectionPipe = new ConnectionPipe(x86);

            base.PID = string.Format("{0}", connectionPipe.PID);
        }

        /// <summary>
        /// Internal use
        /// </summary>
        public override string Submit()
        {
            Log.Debug("SBM.Service [BatchHandlerPipe.Submit] Submit " + base.BatchEventArgs.Parameters);

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

                this.connectionPipe.Send(docRequest.OuterXml);

                result = this.connectionPipe.ReadChannelResponse(TimeSpan.FromSeconds(base.BatchEventArgs.Timeout));

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
                Log.WriteAsync("SBM.Service [BatchHandlerPipe.Submit]", e);

                throw;
            }

            return result;
        }

        public override void Cancel(object state)
        {
            try
            {
                if (this.connectionPipe != null)
                {
                    this.connectionPipe.Send(
                        string.Format("<?xml version='1.0' encoding='UTF-8'?><Request Action='Cancel' Dispatcher='{0}'/>", base.BatchEventArgs.Dispatcher));

                    var result = this.connectionPipe.ReadChannelCancel(Consts.CommunicationTimeout);

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
                Log.WriteAsync("SBM.Service [BatchHandlerPipe.Cancel]", e);
            }
        }

        public override void Close()
        {
            try
            {
                if (this.connectionPipe != null)
                {
                    this.connectionPipe.Dispose();
                    this.connectionPipe = null;
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BatchHandlerPipe.Dispose]", e);
            }
        }
    }
}
