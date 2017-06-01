using System;
using System.Xml;

namespace SBM.Service
{
    public class ExceptionHelper
    {
        public static Exception Build(XmlElement error)
        {
            var inner = error.SelectSingleNode("Inner") as XmlElement;
            if (inner != null)
            {
                return new Exception(
                    error.GetAttribute("Message"), 
                    ExceptionHelper.Build(inner))
                {
                    Source = error.GetAttribute("Source")
                };
            }
            else
            {
                return new Exception(
                    error.GetAttribute("Message"))
                {
                    Source = error.GetAttribute("Source")
                };
            }
        }
    }
}
