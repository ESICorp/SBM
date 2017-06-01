using SBM.Service;
using System;

namespace SBM.Agent
{
    public abstract class Action 
    {
        public static readonly string Submit = "Submit";
        public static readonly string Cancel = "Cancel";

        protected Request Request;
        protected Response Response;
        protected BatchHandler Batch;

        public Action(Request request, Response response)
        {
            this.Request = request;
            this.Response = response;
        }

        public abstract void Execute();
    }
}
