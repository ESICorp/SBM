using SBM.Model;
using System;
using System.IO;
using System.Xml;

namespace SBM.Service
{
    public class AssemblyHelper 
    {
        public AssemblyHelper()
        {
        }

        /// <summary>
        /// Create instances of Batch classes from Assembly
        /// </summary>
        public static BatchHandler LoadAssembly(Context context)
        {
            #region TYPE
            var fullName = Path.Combine(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                "Repository",
                context.AssemblyDirectory,
                context.AssemblyFullName) +
                    (context.AssemblyFullName.EndsWith(".dll") ? string.Empty : ".dll");

            Log.Debug("SBM.Service [AssemblyHelper.LoadAssembly] " + fullName);

            string type = null;

            try
            {
                type = GetType(fullName, context.Dispatcher, context.x86);
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [AssemblyHelper.LoadAssembly]", e);
            }
            if (string.IsNullOrEmpty(type))
            {
                Log.Debug("SBM.Service [AssemblyHelper.LoadAssembly] Not found Batch implementations on " + fullName);
            }
            #endregion

            #region BATCH
            //create instance of batch in specified appdomain
            BatchHandler batch = null;
            
            if (!string.IsNullOrEmpty(type))
            {
                try
                {
                    Log.Debug(string.Format("SBM.Service [AssemblyHelper.LoadAssembly] Create {0} from {1}", type, fullName));

                    if (string.IsNullOrEmpty(context.Server))
                    {
                        batch = new BatchHandlerPipe(context.x86);
                    }
                    else 
                    {
                        batch = new BatchHandlerTCP(context.Server);
                    }
                    //else
                    //{
                    //   batch = (BatchHandler)domain.CreateInstanceAndUnwrap(
                    //        typeof(BatchHandler).Assembly.FullName, typeof(BatchHandler).FullName);
                    //}

                    batch.Initialize(fullName, type);
                }
                catch (Exception e)
                {
                    Log.WriteAsync(string.Format("SBM.Service [AssemblyHelper.LoadAssembly] Couldn't create {0} from {1}", type, fullName), e);
                }
            }
            #endregion

            return batch;
        }

        /// <summary>
        /// Get List of Types
        /// </summary>
        private static string GetType(string fullName, int id_dispatcher, bool x86)
        {
            Log.Debug("SBM.Service [AssemblyHelper.GetType] " + fullName);

            string type = null;

            var docRequest = new XmlDocument();
            var docResponse = new XmlDocument();

            docRequest.InsertBefore(docRequest.CreateXmlDeclaration("1.0", "UTF-8", null),
                docRequest.DocumentElement);

            var element = docRequest.CreateElement(string.Empty, "Request", string.Empty);
            docRequest.AppendChild(element);

            element.SetAttribute("Dispatcher", id_dispatcher.ToString());
            element.SetAttribute("Action", "GetType");
            element.SetAttribute("FileFullName", fullName);
            element.SetAttribute("x86", x86.ToString());

            string result = null;

            using (var external = new ConnectionPipe(x86))
            {
                external.Send(docRequest.OuterXml);

                result = external.ReadChannelResponse(Consts.CommunicationTimeout);
            }

            if (!string.IsNullOrEmpty(result))
            {
                docResponse.LoadXml(result);

                type = docResponse.DocumentElement.InnerText;

                var error = docResponse.SelectSingleNode("//Error") as XmlElement;
                if (error != null)
                {
                    throw ExceptionHelper.Build(error);
                }
            }

            return type;
        }
    }
}
