using SBM.Component;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;

namespace SBM.RestServices
{
    public class QueryController : ApiController
    {
        [HttpGet]
        public JsonResult<DispatcherResponse> Index()
        {
            return Json(new DispatcherResponse() { Fault = RestServiceFault.Custom(new NotImplementedException(), "execute") });
        }

        [HttpGet]
        public JsonResult<CatalogResponse> Catalog([FromUri]CatalogRequest request)
        {
            var response = new CatalogResponse();

            string step = null;
            try
            {
                step = "create response";

                //Arma respuesta
                response.Services = new List<CatalogResponse.Service>();

                step = "connect to database";

                using (var dbHelper = new DbHelper())
                {
                    step = "check owner/token";

                    //valida OWNER/TOKEN
                    var owner = dbHelper.GetOwner(request.ID_OWNER);

                    if (owner == null || !owner.ENABLED || owner.TOKEN != request.TOKEN)
                    {
                        response.Fault = RestServiceFault.AccessDenied;
                    }
                    else
                    {
                        step = "get services";

                        //recupera SERVICE
                        var dbServices = dbHelper.GetServicesOwner(request.ID_OWNER);

                        step = "browse services";

                        foreach (var dbService in dbServices)
                        {
                            var service = new CatalogResponse.Service()
                            {
                                ID_SERVICE = dbService.ID_SERVICE,
                                DESCRIPTION = dbService.SBM_SERVICE_TYPE.DESCRIPTION,
                                SECURITY_LEVEL = 0,
                                ENABLED = dbService.ENABLED,
                                SERVICE_TYPE = new CatalogResponse.Service.ServiceType()
                                {
                                    ID_SERVICE_TYPE = dbService.ID_SERVICE_TYPE,
                                    DESCRIPTION = dbService.SBM_SERVICE_TYPE.DESCRIPTION
                                }
                            };

                            response.Services.Add(service);
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

        [HttpGet]
        public JsonResult<QueuedServicesResponse> QueuedServices([FromUri]QueuedServicesRequest request)
        {
            var response = new QueuedServicesResponse();

            string step = null;

            try
            {
                step = "create response";

                response.Services = new List<QueuedServicesResponse.Service>();

                step = "connect to database";

                using (var dbHelper = new DbHelper())
                {
                    step = "check owner/token";

                    //valida OWNER/TOKEN
                    var owner = dbHelper.GetOwner(request.ID_OWNER);

                    if (owner == null || !owner.ENABLED || owner.TOKEN != request.TOKEN)
                    {
                        response.Fault = RestServiceFault.AccessDenied;
                    }
                    else
                    {
                        step = "get status description";

                        var encola = dbHelper.GetDoneStatus(1);

                        step = "get pendings";

                        //pending
                        var pending = dbHelper.GetPendings(request.ID_OWNER, request.ID_PRIVATE);

                        step = "browse pending";

                        foreach (var dbService in pending)
                        {
                            var service = new QueuedServicesResponse.Service()
                            {
                                ID_DISPATCHER = dbService.ID_DISPATCHER,
                                ID_SERVICE = dbService.ID_SERVICE,
                                DESCRIPTION = dbService.SBM_SERVICE.DESCRIPTION,
                                SECURITY_LEVEL = 0,
                                ID_PRIVATE = dbService.ID_PRIVATE,
                                PARAMETERS = dbService.PARAMETERS,
                                REQUESTED = dbService.REQUESTED,
                                DONE_STATUS = new QueuedServicesResponse.Service.DoneStatus()
                                {
                                    ID_DONE_STATUS = encola.ID_DONE_STATUS,
                                    DESCRIPTION = encola.DESCRIPTION
                                },
                                SERVICE_TYPE = new QueuedServicesResponse.Service.ServiceType()
                                {
                                    ID_SERVICE_TYPE = dbService.SBM_SERVICE.ID_SERVICE_TYPE,
                                    DESCRIPTION = dbService.SBM_SERVICE.SBM_SERVICE_TYPE.DESCRIPTION
                                }
                            };

                            response.Services.Add(service);
                        }

                        step = "get finished";

                        //finished
                        var finished = dbHelper.GetRunningsOwner(request.ID_OWNER, request.ID_PRIVATE);

                        step = "browse finished";

                        foreach (var dbService in finished)
                        {
                            var service = new QueuedServicesResponse.Service()
                            {
                                ID_DISPATCHER = dbService.ID_DISPATCHER,
                                ID_SERVICE = dbService.ID_SERVICE,
                                DESCRIPTION = dbService.SBM_SERVICE.DESCRIPTION,
                                SECURITY_LEVEL = 0,
                                ID_PRIVATE = dbService.ID_PRIVATE,
                                PARAMETERS = dbService.PARAMETERS,
                                REQUESTED = dbService.REQUESTED,
                                STARTED = dbService.STARTED,
                                SERVICE_TYPE = new QueuedServicesResponse.Service.ServiceType()
                                {
                                    ID_SERVICE_TYPE = dbService.SBM_SERVICE.ID_SERVICE_TYPE,
                                    DESCRIPTION = dbService.SBM_SERVICE.SBM_SERVICE_TYPE.DESCRIPTION
                                },
                                DONE_STATUS = new QueuedServicesResponse.Service.DoneStatus()
                                {
                                    ID_DONE_STATUS = dbService.ID_DONE_STATUS,
                                    DESCRIPTION = dbService.SBM_DONE_STATUS.DESCRIPTION
                                }
                            };

                            response.Services.Add(service);
                        }

                        step = "sort";

                        //ordenado por fecha de requerimiento
                        response.Services.Sort((x1, x2) => x2.REQUESTED.GetValueOrDefault().CompareTo(
                            x1.REQUESTED.GetValueOrDefault()));
                    }
                }
            }
            catch (Exception e)
            {
                response.Fault = RestServiceFault.Custom(e, step);
            }

            return Json(response);
        }

        [HttpGet]
        public JsonResult<DispatchedServicesResponse> DispatchedServices([FromUri]DispatchedServicesRequest request)
        {
            var response = new DispatchedServicesResponse();

            string step = null;
            try
            {
                step = "create response";

                response.Services = new List<DispatchedServicesResponse.Service>();

                step = "connect database";

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
                        step = "get finished";

                        var finished = dbHelper.GetFinishedRange(request.ID_OWNER, request.DATE_FROM, request.DATE_TO, request.ID_PRIVATE);

                        step = "browse finished";

                        foreach (var dbService in finished)
                        {
                            var service = new DispatchedServicesResponse.Service()
                            {
                                ID_DISPATCHER = dbService.ID_DISPATCHER,
                                ID_SERVICE = dbService.ID_SERVICE,
                                DESCRIPTION = dbService.SBM_SERVICE.DESCRIPTION,
                                SECURITY_LEVEL = 0,
                                ID_PRIVATE = dbService.ID_PRIVATE,
                                PARAMETERS = dbService.PARAMETERS,
                                REQUESTED = dbService.REQUESTED,
                                STARTED = dbService.STARTED,
                                ENDED = dbService.ENDED.GetValueOrDefault(),
                                RESULT = dbService.RESULT,
                                SERVICE_TYPE = new DispatchedServicesResponse.Service.ServiceType()
                                {
                                    ID_SERVICE_TYPE = dbService.SBM_SERVICE.ID_SERVICE_TYPE,
                                    DESCRIPTION = dbService.SBM_SERVICE.SBM_SERVICE_TYPE.DESCRIPTION
                                },
                                DONE_STATUS = new DispatchedServicesResponse.Service.DoneStatus()
                                {
                                    ID_DONE_STATUS = dbService.ID_DONE_STATUS,
                                    DESCRIPTION = dbService.SBM_DONE_STATUS.DESCRIPTION
                                }
                            };

                            response.Services.Add(service);
                        }

                        step = "sort";

                        //ordenado por fecha de requerimiento
                        response.Services.Sort((x1, x2) => x2.REQUESTED.GetValueOrDefault().CompareTo(
                            x1.REQUESTED.GetValueOrDefault()));
                    }
                }
            }
            catch (Exception e)
            {
                response.Fault = RestServiceFault.Custom(e, step);
            }

            return Json(response);
        }

        [HttpGet]
        public JsonResult<DispatchedServiceResponse> DispatchedService([FromUri]DispatchedServiceRequest request)
        {
            var response = new DispatchedServiceResponse();

            string step = null;
            try
            {                
                step = "connect database";

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
                        step = "get finished";

                        var finished = dbHelper.GetFinished(request.ID_OWNER, request.ID_DISPATCHER);

                        if (finished == null)
                        {
                            response.Fault = RestServiceFault.DispatcherDoesntExist;
                        }
                        else
                        {
                            step = "set response";

                            response.ID_DISPATCHER = finished.ID_DISPATCHER;
                            response.ID_SERVICE = finished.ID_SERVICE;
                            response.DESCRIPTION = finished.SBM_SERVICE.DESCRIPTION;
                            response.SECURITY_LEVEL = 0;
                            response.ID_PRIVATE = finished.ID_PRIVATE;
                            response.PARAMETERS = finished.PARAMETERS;
                            response.REQUESTED = finished.REQUESTED;
                            response.STARTED = finished.STARTED;
                            response.ENDED = finished.ENDED;
                            response.RESULT = finished.RESULT;
                            response.SERVICE_TYPE = new DispatchedServiceResponse.ServiceType()
                            {
                                ID_SERVICE_TYPE = finished.SBM_SERVICE.ID_SERVICE_TYPE,
                                DESCRIPTION = finished.SBM_SERVICE.SBM_SERVICE_TYPE.DESCRIPTION
                            };
                            response.DONE_STATUS = new DispatchedServiceResponse.DoneStatus()
                            {
                                ID_DONE_STATUS = finished.ID_DONE_STATUS,
                                DESCRIPTION = finished.SBM_DONE_STATUS.DESCRIPTION
                            };
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
    }
}