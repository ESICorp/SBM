using SBM.Component;
using SBM.Model;
using System;

namespace SBM.Service
{
    public class CheckEnable : Check
    {
        public CheckEnable(SBM_DISPATCHER dispatcher)
            : base(dispatcher)
        {
        }

        protected override bool IsValid()
        {
            //si el proceso está habilitado
            if (!base.dispatcher.SBM_SERVICE.ENABLED)
            {
                base.Step = "service " + dispatcher.SBM_SERVICE.DESCRIPTION + " disabled";
                Log.Debug("SBM.Service [CheckEnable.IsValid] " + base.Step);

                using (var dbHelper = new DbHelper())
                {
                    dbHelper.SaveOrInsert(new SBM_DONE()
                    {
                        ID_DISPATCHER = base.dispatcher.ID_DISPATCHER,
                        ENDED = DateTimeOffset.UtcNow,
                        ID_DONE_STATUS = Consts.STATUS_SERVICE_DISABLED,
                        RESULT = "Service disabled"
                    });
                }
            }
            //si el proceso está habilitado, el owner
            if (!base.dispatcher.SBM_OWNER.ENABLED)
            {
                base.Step = "owner " + dispatcher.SBM_OWNER.DESCRIPTION + " disabled";
                Log.Debug("SBM.Service [CheckEnable.IsValid] " + base.Step);

                using (var dbHelper = new DbHelper())
                {
                    dbHelper.SaveOrInsert(new SBM_DONE()
                    {
                        ID_DISPATCHER = base.dispatcher.ID_DISPATCHER,
                        ENDED = DateTimeOffset.UtcNow,
                        ID_DONE_STATUS = Consts.STATUS_OWNER_DISABLED,
                        RESULT = "Owner disabled"
                    });
                }
            }

            return base.dispatcher.SBM_SERVICE.ENABLED && base.dispatcher.SBM_OWNER.ENABLED;
        }
    }
}
