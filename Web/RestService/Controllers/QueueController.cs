using SBM.Component;
using SBM.Model;
using System;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Results;

namespace SBM.RestServices
{
    public class QueueController : ApiController
    {
        [HttpGet]
        public JsonResult<DispatcherResponse> Index()
        {
            return Json(new DispatcherResponse() { Fault = RestServiceFault.Custom(new NotImplementedException(), "execute") });
        }

        [HttpPost]
        public JsonResult<NewServiceResponse> NewServiceEnqueue([FromUri]NewServiceRequest request) //, [FromBody] string parameters)
        {
            var response = new NewServiceResponse();

            string step = null;
            try
            {
                step = "connect to database";

                using (var dbHelper = new DbHelper())
                {
                    step = "check owner";

                    //valida OWNER/TOKEN
                    var owner = dbHelper.GetOwner(request.ID_OWNER);

                    if (owner == null || !owner.ENABLED || owner.TOKEN != request.TOKEN)
                    {
                        response.Fault = RestServiceFault.AccessDenied;
                    }
                    else
                    {
                        step = "check service";

                        //validar SERVICE disponible y habilitado
                        var service = dbHelper.GetService(request.ID_SERVICE);

                        if (service == null)
                        {
                            response.Fault = RestServiceFault.ServiceDoesntExist;
                        }
                        else if (!service.ENABLED)
                        {
                            response.Fault = RestServiceFault.ServiceDisabled;
                        }
                        else
                        {
                            step = "check permission";

                            //valida SERVICE/OWNER
                            var permission = dbHelper.GetPermission(request.ID_SERVICE, request.ID_OWNER);

                            if (!permission)
                            {
                                response.Fault = RestServiceFault.IsntOwnerService;
                            }
                            else
                            {
                                step = "enqueue";

                                //Agrega el dispatcher
                                var d = dbHelper.Insert(new SBM_DISPATCHER()
                                {
                                    ID_OWNER = request.ID_OWNER,
                                    ID_PRIVATE = request.ID_PRIVATE,
                                    ID_SERVICE = request.ID_SERVICE,
                                    //PARAMETERS = request.PARAMETERS,
                                    //PARAMETERS = parameters,
                                    PARAMETERS = Request.Content.ReadAsStringAsync().Result,
                                    REQUESTED = DateTimeOffset.UtcNow

                                }) as SBM_DISPATCHER;

                                step = "save changes";

                                response.ID_DISPATCHER = d.ID_DISPATCHER;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                response.Fault = RestServiceFault.Custom(e, step);
            }

            return Json(response);
        }

        [HttpPost]
        public JsonResult<DispatcherResponse> Cancel([FromUri]CancelRequest request)
        {
            var response = new DispatcherResponse();

            string step = null;

            try
            {
                step = "connect to database";

                bool force = false;

                using (var dbHelper = new DbHelper())
                {
                    step = "check owner";

                    //valida OWNER/TOKEN
                    var owner = dbHelper.GetOwner(request.ID_OWNER);

                    if (owner == null || !owner.ENABLED || owner.TOKEN != request.TOKEN)
                    {
                        response.Fault = RestServiceFault.AccessDenied;
                    }
                    else
                    {
                        step = "check dispatched";

                        //validar DISPATCHER (pendiente)
                        var pending = dbHelper.GetPending(request.ID_DISPATCHER);

                        if (pending == null)
                        {
                            step = "check running";

                            //validar OBJ_POOL (corriendo)
                            var running = dbHelper.GetRunning(request.ID_DISPATCHER);

                            if (running == null)
                            {
                                response.Fault = RestServiceFault.WasAlreadyCompleted;
                            }
                            else
                            {
                                //set cancel order
                                running.MAX_TIME_RUN = 0;

                                dbHelper.Save(running);

                                force = true;
                            }
                        }
                        else
                        {
                            step = "prevent run";

                            //pendiente
                            dbHelper.SaveOrInsert(new SBM_DONE()
                            {
                                ID_DISPATCHER = pending.ID_DISPATCHER,
                                ID_SERVICE = pending.ID_SERVICE,
                                ID_OWNER = pending.ID_OWNER,
                                ID_PRIVATE = pending.ID_PRIVATE,
                                PARAMETERS = pending.PARAMETERS,
                                REQUESTED = pending.REQUESTED,
                                RESULT = null,
                                ID_DONE_STATUS = Consts.STATUS_CANCELLED_BY_USER
                            });

                            //trigger
                            //ctxt.SBM_DISPATCHER.Remove(pending);
                        }

                        step = "save changes";
                    }
                }

                if (force)
                {
                    step = "notify service";

                    Semaphore sempahore = null;

                    if (Semaphore.TryOpenExisting("Global\\SBM_CANCEL", out sempahore))
                    {
                        sempahore.Release();
                    }
                }
            }
            catch (Exception e)
            {
                response.Fault = RestServiceFault.Custom(e, step);
            }

            return Json(response);
        }
    }
}