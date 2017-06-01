using Microsoft.AspNet.SignalR;
using System;
using System.Diagnostics;

namespace SBM.Web
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            DebugMonitor.OnOutputDebugString += Application_OnDebug;
            DebugMonitor.Start();
        }

        protected void Application_OnDebug(object sender, EventArgs e)
        {
            try
            {
                var args = e as DebugEventArgs;

                var context = GlobalHost.ConnectionManager.GetHubContext<DebugHub>();
                context.Clients.All.broadcast(DateTime.Now.ToString("HH:mm:ss"), args.Pid, args.Value);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("SBM.Web [Global.Application_OnDebug] " + ex.Message);
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
            DebugMonitor.Stop();
        }
    }
}