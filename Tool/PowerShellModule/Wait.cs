using System;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SBM.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "SBM")]
    public class Wait : Cmdlet
    {
        [Parameter(Position = 1, Mandatory = true, 
            HelpMessage = "Example ws://localhost/SBM.RestService/")]
        [Alias("URI")]
        public string Url { get; set; } = "ws://localhost/SBM.RestService/";

        [Parameter(Position = 0, Mandatory = true, 
            HelpMessage = "Returned by Enqueue", 
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true)]
        public Guid[] Handle { get; set; }

        [Parameter(Position = 2, Mandatory = true, HelpMessage = "Seconds")]
        public long Timeout { get; set; } = 3600;

#if DEBUG
        private ConcurrentBag<string> OUTPUT { get; set; } = new ConcurrentBag<string>();

        public string[] Debug()
        {
            this.ProcessRecord();

            return OUTPUT.ToArray();
        }
#endif
        protected override void ProcessRecord()
        {
            var uriBuilder = new UriBuilder(this.Url + (this.Url.EndsWith("/") ? "Socket/" : "/Socket/"));

            var tasks = new Task<string>[this.Handle.Length];

            for (int i=0; i< this.Handle.Length; i++)
            {
                tasks[i] = ProcessRecord(uriBuilder.Uri, this.Handle[i]);
            }

            Task.WaitAll(tasks, TimeSpan.FromSeconds(Math.Ceiling(this.Timeout * 1.1)));

            for (int i = 0; i < this.Handle.Length; i++)
            {
                if (tasks[i].Status != TaskStatus.Running)
                {
                    if (tasks[i].Exception == null)
                    {
#if DEBUG
                        this.OUTPUT.Add(tasks[i].Result);
#else
                        WriteObject(tasks[i].Result);
#endif
                    }
                    else
                    {
#if DEBUG
                        this.OUTPUT.Add(tasks[i].Exception.Message);
#else
                        WriteError(new ErrorRecord(tasks[i].Exception, "1", ErrorCategory.NotSpecified, this));
#endif

                        if (tasks[i].Exception.InnerExceptions != null)
                        {
                            foreach (var e in tasks[i].Exception.InnerExceptions)
                            {
#if DEBUG
                                this.OUTPUT.Add(e.Message);
#else
                                WriteError(new ErrorRecord(e, "2", ErrorCategory.NotSpecified, this));
#endif
                            }
                        }
                    }
                }
            }
        }

        private async Task<string> ProcessRecord(Uri uri, Guid handle)
        {
            var request = string.Format("{{'Action':'Wait','HANDLE':'{0}','TIMEOUT':'{1}'}}", handle, this.Timeout);
            string response = "";

            using (var socket = new ClientWebSocket())
            {
                Remote.Connect(socket, uri).Wait();

                Remote.Send(socket, request).Wait();

                response = await Remote.Receive(socket);

                Remote.Close(socket).Wait();
            }

            return response;
        }
    }
}
