using SBM.Component;
using SBM.Model;
using System;

namespace SBM.Service
{
    public class CheckParent : Check
    {
        public CheckParent(SBM_DISPATCHER dispatcher)
            : base(dispatcher)
        {
        }

        protected override bool IsValid()
        {
            if (base.dispatcher.SBM_SERVICE.ID_PARENT_SERVICE.GetValueOrDefault(0) > 0) //trick, 0 == forze
            {
                base.Step = "check parent ";
                Log.Debug("SBM.Service [CheckParent.IsValid] " + base.Step + base.dispatcher.SBM_SERVICE.DESCRIPTION);

                SBM_DONE lastDoneParent;
                using (var dbHelper = new DbHelper())
                {
                    lastDoneParent = dbHelper.GetLastDone(base.dispatcher.SBM_SERVICE.ID_PARENT_SERVICE.Value);
                }

                if (lastDoneParent != null && lastDoneParent.ID_DONE_STATUS != Consts.STATUS_SUCCESS)
                {
                    base.Step = "the parent " + base.dispatcher.SBM_SERVICE.SBM_SERVICE_PARENT.DESCRIPTION + " not successfully completed";
                    Log.Debug("SBM.Service [CheckParent.IsValid] " + base.Step);

                    using (var dbHelper = new DbHelper())
                    {
                        dbHelper.SaveOrInsert(new SBM_DONE()
                        {
                            ID_DISPATCHER = base.dispatcher.ID_DISPATCHER,
                            ENDED = DateTimeOffset.UtcNow,
                            ID_DONE_STATUS = Consts.STATUS_CANCELLED_PARENT_NOT_READY,
                            RESULT = "Parent not successfully completed"
                        });
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
