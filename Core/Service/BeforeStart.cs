using COMAdmin;
using SBM.Component;
using SBM.Model;
using System;
using System.EnterpriseServices;
using System.IO;

namespace SBM.Service
{
    internal class BeforeStart
    {
        public static void Perform()
        {
            #region Register COM+
            try
            {
                Log.WriteAsync("SBM.Service [BeforeStart.Perform] Register Component");

                var regComponent = new RegistrationConfig();
                regComponent.InstallationFlags = InstallationFlags.FindOrCreateTargetApplication;
                regComponent.ApplicationRootDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                regComponent.Application = "Simple Batch Manager Component";
                regComponent.AssemblyFile = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "SBM.Component.dll");

                var reg1 = new RegistrationHelper();
                reg1.InstallAssemblyFromConfig(ref regComponent);

                //var reg2 = new RegistrationHelper();
                //var regModel = new RegistrationConfig();
                //regModel.InstallationFlags = InstallationFlags.FindOrCreateTargetApplication;
                //regModel.ApplicationRootDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                //regModel.Application = "Dispatcher Model"; 
                //regModel.AssemblyFile = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "SBM.Model.dll");
                //reg2.InstallAssemblyFromConfig(ref regModel);

                COMAdminCatalog catalog = new COMAdminCatalog();
                COMAdminCatalogCollection collection = catalog.GetCollection("Applications");
                collection.Populate(); 
                for (int i=0; i<collection.Count; i++)
                {
                    COMAdminCatalogObject application = collection.Item[i];
                    if (application.Name == "Simple Batch Manager Component" &&
                        application.Value["Identity"] != @"NT AUTHORITY\NetworkService")
                    {
                        application.Value["Identity"] = @"NT AUTHORITY\NetworkService";
                        collection.SaveChanges();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BeforeStart.Perform] Register Component", e);
            }
            #endregion

            #region Running jobs
            SBM_OBJ_POOL[] jobs = null;

            try
            {
                using (var dbHelper = new DbHelper())
                {
                    jobs = dbHelper.GetRunnings();
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BeforeStart.Perform] Getting running", e);
            }
            #endregion

            if (jobs != null)
            {
                foreach (var job in jobs)
                {
                    #region Save Done / delete ObjPool
                    try
                    {
                        Log.WriteAsync("SBM.Service [BeforeStart.Perform] Fatal " + job.ID_DISPATCHER);

                        using (var dbHelper = new DbHelper())
                        {
                            var dispatcher = dbHelper.GetDispatcher(job.ID_DISPATCHER);

                            if (dispatcher == null)
                            {
                                dbHelper.DeleteObjPool(job.ID_DISPATCHER);
                            }
                            else
                            {
                                dbHelper.SaveOrInsert(new SBM_DONE()
                                {
                                    ID_DISPATCHER = job.ID_DISPATCHER,
                                    ID_DONE_STATUS = Consts.STATUS_FATAL_ERROR,
                                    RESULT = "Process information couldn't be obtained",
                                    ENDED = DateTimeOffset.UtcNow
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.WriteAsync("SBM.Service [BeforeStart.Perform] Done/ObjPool", e);
                    }
                    #endregion
                }
            }

            #region Unexpected service
            try
            {
                using (var dbHelper = new DbHelper())
                {
                    dbHelper.FixLastShutdown(Bootstrap.PickShutdown());
                }
            }
            catch (Exception e)
            {
                Log.WriteAsync("SBM.Service [BeforeStart.Perform] CheckLastShutdown", e);
            }
            #endregion
        }
    }
}
