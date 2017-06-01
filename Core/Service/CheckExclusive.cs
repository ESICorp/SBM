using SBM.Component;
using SBM.Model;
using System;

namespace SBM.Service
{
    public class CheckExclusive : Check
    {
        private Core Core { get; set; }

        public CheckExclusive(SBM_DISPATCHER dispatcher)
            : base(dispatcher)
        {
            Core = Core.GetInstance();
        }

        protected override bool IsValid()
        {
            if (base.dispatcher.SBM_SERVICE.SINGLE_EXEC && Core.UniqueRunning.Contains(dispatcher.ID_SERVICE))
            {
                base.Step = "break exclusivity ";
                Log.Debug("SBM.Service [CheckExclusive.IsValid] " + base.Step + base.dispatcher.SBM_SERVICE.DESCRIPTION);

                using (var dbHelper = new DbHelper())
                {
                    dbHelper.SaveOrInsert(new SBM_DONE()
                    {
                        ID_DISPATCHER = base.dispatcher.ID_DISPATCHER,
                        ENDED = DateTimeOffset.UtcNow,
                        ID_DONE_STATUS = Consts.STATUS_CANCELLED_SERVICE_ALREADY_RUNNING,
                        RESULT = "Service already running"
                    });
                }

                return false;
            }
            return true;
        }
    }
}
