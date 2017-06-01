using System;
using System.Xml;

namespace SBM.Agent
{
    public class Response
    {
        private XmlDocument Document { get; set; }

        public Response()
        {
            this.Document = new XmlDocument();

            this.Document.AppendChild(
                this.Document.CreateElement(string.Empty, "Response", string.Empty));

            this.Document.InsertBefore(
                this.Document.CreateXmlDeclaration("1.0", "UTF-8", null),
                this.Document.DocumentElement);
        }

        public XmlElement CreateElement(XmlElement parent, string localName)
        {
            var element = this.Document.CreateElement(string.Empty, localName, string.Empty);

            parent.AppendChild(element);

            return element;
        }

        public XmlElement CreateElement(string localName)
        {
            return CreateElement(this.Document.DocumentElement, localName);
        }

        public void SetValue(string value)
        {
            this.Document.DocumentElement.InnerText = value;
        }

        public void SetException(Exception e)
        {
            var root = CreateElement("Error");
            root.SetAttribute("Source", e.Source);
            root.SetAttribute("Message", e.Message);

            var inner = e.InnerException;
            while (inner != null)
            {
                var element = CreateElement(root, "Inner");
                element.SetAttribute("Source", inner.Source);
                element.SetAttribute("Message", inner.Message);

                root = element;
                inner = inner.InnerException;
            }
        }

        public override string ToString()
        {
            return this.Document.OuterXml;
        }
    }
}
