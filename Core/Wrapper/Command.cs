using System;

namespace SBM.Wrapper
{
    public class Command
    {
        public Request Request { get; private set; }
        public Response Response { get; private set; }

        public Command(string readed)
        {
            this.Response = new Response();
            this.Request = new Request(readed);
        }

        public string Execute()
        {
            Action action = null;

            try
            {
                if (Request.Action == Action.GetTypes)
                {
                    action = new ActionGetType(this.Request, this.Response);
                }
                else if (Request.Action == Action.Submit)
                {
                    action = new ActionSubmit(this.Request, this.Response);
                }
                else if (Request.Action == Action.Cancel)
                {
                    action = new ActionCancel(this.Request, this.Response);
                }
                else
                {
                    throw new NotImplementedException(Request.Action);
                }

                action.Execute();
            }
            catch (Exception e)
            {
                this.Response.SetException(e);
            }
            finally
            {
                if (action != null)
                {
                    action.Dispose();
                }
            }

            return this.Response.ToString();
        }
    }
}
