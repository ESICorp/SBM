
using System;
namespace SBM.Wrapper
{
    public class ActionCancel : Action
    {
        public ActionCancel(Request request, Response response)
            : base(request, response)
        {
        }

        public override void Execute()
        {
            try
            {
                if (Action.Batch != null)
                {
                    Action.Batch.Cancel(null);

                    base.Response.SetValue("OK");
                }
            }
            catch (Exception e)
            {
                Response.SetException(e);
            }
        }
    }
}
