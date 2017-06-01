using System;

namespace SBM.Agent
{
    public class ActionCancel : Action
    {
        public ActionCancel(Request request, Response response) 
            : base(request, response)
        {
            base.Batch = Core.GetInstance().Running[Request.Dispatcher];
        }

        public override void Execute()
        {
            try
            {
                if (base.Batch != null)
                {
                    base.Batch.Cancel(null);

                    Response.SetValue("OK");
                }
            }
            catch (Exception e)
            {
                Response.SetException(e);
            }
        }
    }
}
