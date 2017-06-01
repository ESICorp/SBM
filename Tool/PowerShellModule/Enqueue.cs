using System;
using System.Management.Automation;
using System.Net.WebSockets;

namespace SBM.PowerShell
{

    [Cmdlet(VerbsCommon.Push, "SBM")]
    public class Enqueue : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Example ws://localhost/SBM.RestService/")]
        [Alias("URI")]
        public string Url { get; set; } = "ws://localhost/SBM.RestService/";

        [Parameter(Position = 1, Mandatory = true, HelpMessage = "ID_OWNER")]
        [Alias("ID_OWNER")]
        public int Owner { get; set; }

        [Parameter(Position = 2, Mandatory = true, HelpMessage = "TOKEN")]
        public string Token { get; set; }

        [Parameter(Position = 3, Mandatory = true, HelpMessage = "ID_SERVICE")]
        [Alias("ID_SERVICE")]
        public int Service { get; set; }

        [Parameter(Position = 4, Mandatory = false, HelpMessage = "ID_PRIVATE")]
        [Alias("ID_PRIVATE")]
        public string Private { get; set; }

        [Parameter(Position = 5, Mandatory = false, HelpMessage = "PARAMETERS")]
        public string Parameters { get; set; }

#if DEBUG
        private string OUTPUT { get; set; }

        public Guid Debug()
        {
            this.ProcessRecord();

            return Guid.Parse( OUTPUT.Replace("{'Handle':'", string.Empty).Replace("'}", string.Empty) );
        }
#endif

        protected override void ProcessRecord()
        {
            try
            {
                var uriBuilder = new UriBuilder(this.Url + (this.Url.EndsWith("/") ? "Socket/" : "/Socket/"));

                var request = string.Format("{{'Action':'Enqueue','ID_OWNER':'{0}','TOKEN':'{1}','ID_SERVICE':'{2}','ID_PRIVATE':'{3}','PARAMETERS':'{4}'}}",
                        this.Owner, this.Token, this.Service, this.Private, this.Parameters);

                using (var socket = new ClientWebSocket())
                {
                    Remote.Connect(socket, uriBuilder.Uri).Wait();

                    Remote.Send(socket, request).Wait();

                    var response = Remote.Receive(socket);

                    response.Wait();
#if DEBUG
                    this.OUTPUT = response.Result;
#else
                    WriteObject(response.Result);
#endif
                    Remote.Close(socket).Wait();
                }
            }
            catch (Exception e)
            {
#if DEBUG
                this.OUTPUT = e.Message;
#else
                WriteError(new ErrorRecord(e, "1", ErrorCategory.NotSpecified, this));
#endif
            }
        }

    }
}
