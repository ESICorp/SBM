using SBM.Component;
using SBM.Model;
using System;

namespace SBM.Service
{
    public class CheckThread : Check
    {
        private Core Core { get; set; }

        public CheckThread(SBM_DISPATCHER dispatcher)
            : base(dispatcher)
        {
            Core = Core.GetInstance();
        }

        protected override bool IsValid()
        {
            if (Core.Running.Count >= Config.SBM_MAX_OBJ_POOL && Config.SBM_MAX_OBJ_POOL != 0)
            {
                base.Step = "not enough physical threads, running " + Core.Running.Count + " and configured " + Config.SBM_MAX_OBJ_POOL;
                Log.Debug("SBM.Service [CheckThread.IsValid] " + base.Step + base.dispatcher.SBM_SERVICE.DESCRIPTION);

                using (var dbHelper = new DbHelper())
                {
                    dbHelper.SaveOrInsert(new SBM_DONE()
                    {
                        ID_DISPATCHER = base.dispatcher.ID_DISPATCHER,
                        ENDED = DateTimeOffset.UtcNow,
                        ID_DONE_STATUS = Consts.STATUS_INTERNAL_ERROR,
                        RESULT = "Not enough physical threads"
                    });

                    dbHelper.AddEventLog(new SBM_EVENT_LOG()
                    {
                        ID_EVENT = Consts.LOG_APPLICATION_POOL_FULL,
                        DESCRIPTION = "Not enough thread to " + dispatcher.SBM_SERVICE.DESCRIPTION +
                            ", running " + Core.Running.Count +
                            " and configured " + Config.SBM_MAX_OBJ_POOL
                    });
                }
                return false;
            }
            return true;
        }
    }
}
