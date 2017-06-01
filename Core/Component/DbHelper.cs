using SBM.Model;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;

namespace SBM.Component
{
    [Transaction(TransactionOption.Required)]
    [JustInTimeActivation(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ObjectPooling(MinPoolSize = 5, MaxPoolSize = 10, CreationTimeout = 20)]
    public class DbHelper : ServicedComponent, IDisposable
    {
        public DbHelper()
        {
        }

        /// <summary>
        /// Obtiene todos los servicios
        /// </summary>
        /// <returns></returns>
        public SBM_SERVICE[] GetServices()
        {
            using (var ctxt = new Context())
            {
                var services = from d in ctxt.SBM_SERVICE
                                .Include(_ => _.SBM_SERVICE_TYPE)
                               select d;

                return services.ToArray();
            }
        }

        public SBM_SERVICE[] GetServicesOwner(short ID_OWNER)
        {
            using (var ctxt = new Context())
            {
                var services = from s in ctxt.SBM_SERVICE
                                .Include(_ => _.SBM_SERVICE_TYPE)
                               where s.SBM_OWNER.Any( o => o.ID_OWNER == ID_OWNER)
                               select s;

                return services.ToArray();
            }
        }

        /// <summary>
        /// Obtiene un servicio
        /// </summary>
        /// <returns></returns>
        public SBM_SERVICE GetService(int ID_SERVICE)
        {
            using (var ctxt = new Context())
            {
                return ctxt.SBM_SERVICE.Find(ID_SERVICE);
            }
        }

        /// <summary>
        /// Existe el servicio
        /// </summary>
        /// <returns></returns>
        public bool ServiceExists(string name)
        {
            using (var ctxt = new Context())
            {
                return ctxt.SBM_SERVICE.Any(x => x.DESCRIPTION.ToLower() == name.ToLower());
            }
        }

        /// <summary>
        /// Obtiene los procesos con cancelacion forzada (o sea, timeout = 0)
        /// </summary>
        /// <returns></returns>
        public SBM_OBJ_POOL[] GetMakedToCancel()
        {
            using (var ctxt = new Context())
            {
                var list = from r in ctxt.SBM_OBJ_POOL
                            .Include(_ => _.SBM_SERVICE)
                           where r.MAX_TIME_RUN == 0
                           select r;

                return list.ToArray();
            }
        }

        /// <summary>
        /// Obtiene los jobs pendientes de ejecucion
        /// </summary>
        /// <returns></returns>
        [AutoComplete]
        public SBM_DISPATCHER[] TakePending()
        {
            SBM_DISPATCHER[] dispatchers = null;

            using (var ctxt = new Context())
            {
                dispatchers = (from d in ctxt.SBM_DISPATCHER
                                    .Include(_ => _.SBM_OWNER)
                                    .Include(_ => _.SBM_SERVICE.SBM_SERVICE_PARENT)
                                    .Include(_ => _.SBM_SERVICE.SBM_REMOTING)
                               orderby d.ID_DISPATCHER
                               select d).ToArray();

                foreach (var dispatcher in dispatchers)
                {
                    var done = ctxt.SBM_DONE.Find(dispatcher.ID_DISPATCHER);

                    if (done == null)
                    {
                        done = new SBM_DONE();
                        done.ID_DISPATCHER = dispatcher.ID_DISPATCHER;

                        ctxt.SBM_DONE.Add(done);
                    }

                    done.ID_SERVICE = dispatcher.ID_SERVICE;
                    done.PARAMETERS = dispatcher.PARAMETERS;
                    done.STARTED = DateTimeOffset.UtcNow;
                    done.ID_OWNER = dispatcher.ID_OWNER;
                    done.ID_PRIVATE = dispatcher.ID_PRIVATE;
                    done.REQUESTED = dispatcher.REQUESTED;
                    done.ID_DONE_STATUS = Consts.STATUS_RUNNING;
                    done.ID_REMOTING = dispatcher.SBM_SERVICE.SBM_REMOTING.FirstOrDefault() == null ? 0 :
                        dispatcher.SBM_SERVICE.SBM_REMOTING.FirstOrDefault().ID_REMOTING;
                }

                ctxt.SaveChanges();
            }

            return dispatchers;
        }

        /// <summary>
        /// Obtiene el ultimo done
        /// </summary>
        /// <param name="service">servicio para ultimo log</param>
        /// <returns>last done</returns>
        public SBM_DONE GetLastDone(short ID_SERVICE)
        {
            using (var ctxt = new Context())
            {
                var done = from d in ctxt.SBM_DONE
                           where d.ID_SERVICE == ID_SERVICE
                                //&& d.ID_DONE_STATUS < Consts.STATUS_SERVICE_DISABLED
                           orderby d.ID_DISPATCHER descending
                           select d;

                return done.Take(1).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get Scheduled Jobs
        /// </summary>
        /// <returns>Scheudled Jobs</returns>
        public SBM_SERVICE_TIMER[] GetScheduled()
        {
            using (var ctxt = new Context())
            {
                var scheduled = from t in ctxt.SBM_SERVICE_TIMER
                                            .Include(_ => _.SBM_SERVICE)
                                where //t.SBM_OWNER.ENABLED &&
                                      //t.SBM_SERVICE.ENABLED &&
                                    t.ENABLED &&
                                    t.NEXT_TIME_RUN != null &&
                                    t.NEXT_TIME_RUN < DateTimeOffset.UtcNow
                                orderby t.NEXT_TIME_RUN
                                select t;

                return scheduled.ToArray();
            }
        }

        /// <summary>
        /// Get Running Jobs
        /// </summary>
        /// <returns>Running Jobs</returns>
        public SBM_OBJ_POOL[] GetRunnings()
        {
            using (var ctxt = new Context())
            {
                var running = from r in ctxt.SBM_OBJ_POOL
                                .Include(_ => _.SBM_SERVICE)
                              orderby r.STARTED
                              select r;

                return running.ToArray();
            }
        }

        /// <summary>
        /// Informa si el proceso se marco para cancelacion
        /// </summary>
        /// <param name="ID_DISPATCHER"></param>
        /// <returns></returns>
        public bool IsClearEnqueued(int ID_DISPATCHER)
        {
            using (Context ctxt = new Context())
            {
                return ctxt.SBM_DISPATCHER.Any(_ => _.ID_DISPATCHER == ID_DISPATCHER);
            }
        }

        [AutoComplete]
        public void SaveOrInsert(SBM_DONE done)
        {
            using (var ctxt = new Context())
            {
                var saved = (from d in ctxt.SBM_DONE
                             where d.ID_DISPATCHER == done.ID_DISPATCHER
                             select d).FirstOrDefault();

                if (saved == null)
                {
                    ctxt.SBM_DONE.Add(done);
                }
                else
                {
                    saved.ENDED = done.ENDED;
                    saved.ID_DONE_STATUS = done.ID_DONE_STATUS;
                    saved.RESULT = done.RESULT;

                    var objPool = (from p in ctxt.SBM_OBJ_POOL
                                   where p.ID_DISPATCHER == done.ID_DISPATCHER
                                   select p).FirstOrDefault();

                    if (objPool != null)
                    {
                        ctxt.SBM_OBJ_POOL.Remove(objPool);
                    }
                }

                ctxt.SaveChanges();
            }
        }

        [AutoComplete]
        public void DeleteObjPool(int ID_DISPATCHER)
        {
            using (var ctxt = new Context())
            {
                var objPool = (from p in ctxt.SBM_OBJ_POOL
                               where p.ID_DISPATCHER == ID_DISPATCHER
                               select p).FirstOrDefault();

                if (objPool != null)
                {
                    ctxt.SBM_OBJ_POOL.Remove(objPool);
                }

                ctxt.SaveChanges();
            }
        }

        /// <summary>
        /// Guarda
        /// </summary>
        /// <param name="object"></param>
        [AutoComplete]
        public void Save(SBM_TABLE @object)
        {
            using (var ctxt = new Context())
            {
                ctxt.Entry(@object).State = EntityState.Modified;

                ctxt.SaveChanges();
            }
        }

        /// <summary>
        /// Guarda
        /// </summary>
        /// <param name="object"></param>
        [AutoComplete]
        public SBM_TABLE Insert(SBM_TABLE @object)
        {
            using (var ctxt = new Context())
            {
                ctxt.Entry(@object).State = EntityState.Added;

                ctxt.SaveChanges();
            }

            return @object;
        }

        /// <summary>
        /// Save Event Log on DB
        /// </summary>
        /// <param name="event_log"></param>
        [AutoComplete]
        public void AddEventLog(SBM_EVENT_LOG event_log)
        {
            using (var ctxt = new Context())
            {
                event_log.DESCRIPTION = event_log.DESCRIPTION.Left(4000);

                event_log.TIME_STAMP = DateTimeOffset.UtcNow;

                ctxt.SBM_EVENT_LOG.Add(event_log);

                ctxt.SaveChanges();
            }
        }

        public byte[] Crypt(string phrase, string text)
        {
            using (var ctxt = new Context())
            {
                return ctxt.Crypt(phrase, text).FirstOrDefault();
            }
        }

        public byte[] Decrypt(string phrase, byte[] cipher)
        {
            using (var ctxt = new Context())
            {
                return ctxt.Decrypt(phrase, cipher).FirstOrDefault();
            }
        }

        public SBM_SERVICE_TYPE[] GetServiceTypes()
        {
            using (var ctxt = new Context())
            {
                var list = from t in ctxt.SBM_SERVICE_TYPE
                           orderby t.DESCRIPTION
                           select t;

                return list.ToArray();
            }
        }

        /// <summary>
        /// Obtiene todos los owners
        /// </summary>
        /// <returns></returns>
        public SBM_OWNER[] GetOwners()
        {
            using (var ctxt = new Context())
            {
                return ctxt.SBM_OWNER.ToArray();
            }
        }

        /// <summary>
        /// Obtiene un owner
        /// </summary>
        /// <returns></returns>
        public SBM_OWNER GetOwner(int ID_OWNER)
        {
            using (var ctxt = new Context())
            {
                return ctxt.SBM_OWNER.Find(ID_OWNER);
            }
        }

        public bool GetPermission(int ID_SERVICE, short ID_OWNER)
        {
            using (var ctxt = new Context())
            {
                var permission = ctxt.SBM_SERVICE.Any(s =>
                                 s.ID_SERVICE == ID_SERVICE && 
                                 s.SBM_OWNER.Any(o => o.ID_OWNER == ID_OWNER));

                return permission;
            }
        }

        /// <summary>
        /// Cuenta todos los event logs
        /// </summary>
        /// <returns></returns>
        public int CountEventLog()
        {
            using (var ctxt = new Context())
            {
                return ctxt.SBM_EVENT_LOG.Count();
            }
        }

        /// <summary>
        /// Obtiene todos los event logs
        /// </summary>
        /// <returns></returns>
        public SBM_EVENT_LOG[] GetEventLog(int page, int size)
        {
            using (var ctxt = new Context())
            {
                var events = from d in ctxt.SBM_EVENT_LOG
                                        .Include(_ => _.SBM_EVENT)
                             orderby d.ID_EVENT_LOG descending
                             select d;

                return events.Skip(size * page)
                              .Take(size)
                              .ToArray();
            }
        }

        /// <summary>
        /// Cuenta todos los done
        /// </summary>
        /// <returns></returns>
        public int CountDone()
        {
            using (var ctxt = new Context())
            {
                return ctxt.SBM_DONE.Count();
            }
        }

        /// <summary>
        /// Obtiene todos los done
        /// </summary>
        /// <returns></returns>
        public SBM_DONE[] GetDone(int page, int size)
        {
            using (var ctxt = new Context())
            {
                var done = from d in ctxt.SBM_DONE
                                    .Include(_ => _.SBM_SERVICE)
                                    .Include(_ => _.SBM_OWNER)
                                    .Include(_ => _.SBM_DONE_STATUS)
                           orderby d.REQUESTED descending
                           select d;

                return done.Skip(size * page)
                              .Take(size)
                              .ToArray();
            }
        }

        public SBM_DONE[] GetRunningsOwner(short ID_OWNER, string ID_PRIVATE)
        {
            using (var ctxt = new Context())
            {
                //var finished = from d in ctxt.SBM_DONE
                //                .Include(_ => _.SBM_SERVICE)
                //                .Include(_ => _.SBM_SERVICE.SBM_SERVICE_TYPE)
                //                .Include(_ => _.SBM_DONE_STATUS)
                //              where d.ID_OWNER == ID_OWNER
                //                && d.ID_DONE_STATUS == Consts.STATUS_RUNNING
                //                && (d.ID_PRIVATE == ID_PRIVATE || ID_PRIVATE == null)
                //               select d;

                if (ID_PRIVATE == null)
                {
                    return ctxt.SBM_DONE
                        .Include(_ => _.SBM_SERVICE)
                        .Include(_ => _.SBM_SERVICE.SBM_SERVICE_TYPE)
                        .Include(_ => _.SBM_DONE_STATUS)
                        .Where(d => d.ID_OWNER == ID_OWNER && 
                            d.ID_DONE_STATUS == Consts.STATUS_RUNNING).ToArray();
                }
                else
                {
                    return ctxt.SBM_DONE
                        .Include(_ => _.SBM_SERVICE)
                        .Include(_ => _.SBM_SERVICE.SBM_SERVICE_TYPE)
                        .Include(_ => _.SBM_DONE_STATUS)
                        .Where(d => d.ID_OWNER == ID_OWNER && 
                            d.ID_DONE_STATUS == Consts.STATUS_RUNNING &&
                            d.ID_PRIVATE == ID_PRIVATE).ToArray();
                }
            }
        }

        public SBM_DONE GetFinished(short ID_OWNER, int ID_DISPATCHER)
        {
            using (var ctxt = new Context())
            {
                var finished = from d in ctxt.SBM_DONE
                                .Include(_ => _.SBM_SERVICE)
                                .Include(_ => _.SBM_SERVICE.SBM_SERVICE_TYPE)
                                .Include(_ => _.SBM_DONE_STATUS)
                               where d.ID_OWNER == ID_OWNER
                                 && d.ID_DISPATCHER == ID_DISPATCHER
                               select d;

                return finished.FirstOrDefault();
            }
        }

        public SBM_DONE[] GetFinishedRange(short ID_OWNER, DateTimeOffset? start, DateTimeOffset? end, string ID_PRIVATE)
        {
            using (var ctxt = new Context())
            {
                if ( end != null && end.Value.TimeOfDay == TimeSpan.Zero)
                {
                    end = end.Value.AddDays(1).AddSeconds(-1);
                }

                return ctxt.SBM_DONE
                                .Include(_ => _.SBM_SERVICE)
                                .Include(_ => _.SBM_SERVICE.SBM_SERVICE_TYPE)
                                .Include(_ => _.SBM_DONE_STATUS)
                                .Where(d =>
                                    d.ID_OWNER == ID_OWNER &&
                                    d.ID_DONE_STATUS > Consts.STATUS_RUNNING &&
                                    (start == null || (start != null && d.REQUESTED >= start.Value)) &&
                                    (end == null || (end != null && d.REQUESTED <= end.Value)) &&
                                    (ID_PRIVATE == null || (ID_PRIVATE != null && d.ID_PRIVATE == ID_PRIVATE)))
                                .Distinct().ToArray();
            }
        }

        /// <summary>
        /// Obtiene todos los timer
        /// </summary>
        /// <returns></returns>
        public SBM_SERVICE_TIMER[] GetTimers()
        {
            using (var ctxt = new Context())
            {
                var timers = from d in ctxt.SBM_SERVICE_TIMER
                                    .Include(_ => _.SBM_SERVICE)
                                    .Include(_ => _.SBM_OWNER)
                             orderby d.NEXT_TIME_RUN descending
                             select d;

                return timers.ToArray();
            }
        }

        /// <summary>
        /// Obtiene un timer
        /// </summary>
        /// <returns></returns>
        public SBM_SERVICE_TIMER GetTimer(int ID_SERVICE_TIMER)
        {
            using (var ctxt = new Context())
            {
                var timer = from d in ctxt.SBM_SERVICE_TIMER
                                    .Include(_ => _.SBM_SERVICE)
                                    .Include(_ => _.SBM_OWNER)
                            where d.ID_SERVICE_TIMER == ID_SERVICE_TIMER
                            select d;

                return timer.FirstOrDefault();
            }
        }

        [AutoComplete]
        public void FixLastShutdown(string suicide)
        {
            var description = suicide == null ? "Service terminated unexpectedly" :
                string.Format("Service stopped itself at {0}", suicide);

            using (var ctxt = new Context())
            {
                var last = (from l in ctxt.SBM_EVENT_LOG
                            where l.ID_EVENT == Consts.LOG_APPLICATION_STOPPED 
                                || l.ID_EVENT == Consts.LOG_APPLICATION_STARTED
                                || (l.ID_EVENT == Consts.LOG_AUDIT && l.DESCRIPTION == description) 
                            orderby l.ID_EVENT_LOG descending
                            select l).FirstOrDefault();

                if (last != null && last.ID_EVENT == Consts.LOG_APPLICATION_STARTED) 
                {
                    ctxt.SBM_EVENT_LOG.Add(
                        new SBM_EVENT_LOG()
                        {
                            ID_EVENT = Consts.LOG_AUDIT, 
                            DESCRIPTION = description,
                            TIME_STAMP = DateTimeOffset.UtcNow
                        });

                    ctxt.SaveChanges();
                }
            }
        }

        public bool IsAlive
        {
            get
            {
                try
                {
                    using (Context ctxt = new Context())
                    {
                        return ctxt.Database.Exists();
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        /*
                /// <summary>
                /// Vuelve a poner el servicio en control
                /// </summary>
                /// <param name="ID_SERVICE"></param>
                /// <returns></returns>
                public int SetForceOff(short ID_SERVICE)
                {
                    using (Entities ctxt = new Entities())
                    {
                        return ctxt.Database.ExecuteSqlCommand(
                            "UPDATE [dbo].[SBM_SERVICE] SET [ID_PARENT_SERVICE] = [ID_SERVICE] WHERE [ID_SERVICE] = @ID_SERVICE",
                            new SqlParameter("@ID_SERVICE", ID_SERVICE));
                    }
                }
                */

        /// <summary>
        /// Get Running Job
        /// </summary>
        /// <returns>Running Job</returns>
        public SBM_OBJ_POOL GetRunning(int ID_DISPATCHER)
        {
            using (var ctxt = new Context())
            {
                var running = from r in ctxt.SBM_OBJ_POOL
                                .Include(_ => _.SBM_SERVICE)
                              where r.ID_DISPATCHER == ID_DISPATCHER
                              select r;
                
                return running.FirstOrDefault();
            }
        }

        public SBM_DISPATCHER GetPending(int ID_DISPATCHER)
        {
            using (var ctxt = new Context())
            {
                var pending = from d in ctxt.SBM_DISPATCHER
                                .Include(_ => _.SBM_SERVICE)
                              where d.ID_DISPATCHER == ID_DISPATCHER
                              select d;

                return pending.FirstOrDefault();
            }
        }

        public SBM_DISPATCHER[] GetPendings(short ID_OWNER, string ID_PRIVATE)
        {
            using (var ctxt = new Context())
            {
                //var pending = from d in ctxt.SBM_DISPATCHER
                //                .Include(_ => _.SBM_SERVICE)
                //                .Include(_ => _.SBM_SERVICE.SBM_SERVICE_TYPE)
                //              where d.ID_OWNER == ID_OWNER
                //                && (d.ID_PRIVATE == ID_PRIVATE || ID_PRIVATE == null)
                //              select d;

                if (ID_PRIVATE == null)
                {
                    return ctxt.SBM_DISPATCHER
                                .Include(_ => _.SBM_SERVICE)
                                .Include(_ => _.SBM_SERVICE.SBM_SERVICE_TYPE)
                                .Where(d => d.ID_OWNER == ID_OWNER).ToArray();
                }
                else
                {
                    return ctxt.SBM_DISPATCHER
                                .Include(_ => _.SBM_SERVICE)
                                .Include(_ => _.SBM_SERVICE.SBM_SERVICE_TYPE)
                                .Where(d => d.ID_OWNER == ID_OWNER && 
                                    d.ID_PRIVATE == ID_PRIVATE).ToArray();
                }
            }
        }

        public SBM_DONE_STATUS GetDoneStatus(byte ID_DONE_STATUS)
        {
            using (var ctxt = new Context())
            {
                return ctxt.SBM_DONE_STATUS.Find(ID_DONE_STATUS);
            }
        }

        public SBM_DISPATCHER GetDispatcher(int ID_DISPATCHER)
        {
            using (var ctxt = new Context())
            {
                return ctxt.SBM_DISPATCHER.Find(ID_DISPATCHER);
            }
        }

        public Guid Enqueue(int ID_OWNER, int ID_SERVICE, string ID_PRIVATE, string PARAMETERS)
        {
            var xml = new XmlDocument();
            var doc = xml.CreateElement("req");
            var node = xml.AppendChild(doc);
            doc.SetAttribute("id_owner", Convert.ToString(ID_OWNER));
            doc.SetAttribute("id_service", Convert.ToString(ID_SERVICE));
            doc.SetAttribute("id_private", ID_PRIVATE);
            doc.SetAttribute("parameters", PARAMETERS); //SecurityElement.Escape(parameters)

            var request = xml.OuterXml;

            Trace.WriteLine("Dispatcher.Component [DbHelper.Enqueue] " + request);

            using (var ctxt = new Context())
            {
                return ctxt.Enqueue(request).FirstOrDefault();
            }
        }

        public SBM_DONE Join(Guid handle, long timeout)
        {
            using (var ctxt = new Context())
            {
                var result = ctxt.Join(handle, timeout).FirstOrDefault();

                if (result == null)
                {
                    Trace.WriteLine("Dispatcher.Component [DbHelper.Join] null");

                    return null;
                }
                else
                {
                    Trace.WriteLine("Dispatcher.Component [DbHelper.Join] " + result);

                    var xml = new XmlDocument();
                    xml.LoadXml(result);

                    //<res ID_DISPATCHER="23" ID_SERVICE="1" ID_OWNER="2" ID_PRIVATE="113B74E6-B1C1-E611-94D2-F1ECA6CA2F5F" PARAMETERS="10" REQUESTED="2016-12-14T01:00:51.3878027-03:00" STARTED="2016-12-14T04:00:47.4996412Z" ENDED="2016-12-14T04:00:58.9384193Z" ID_DONE_STATUS="3" RESULT="DummySample - parameter 10 - return" ID_REMOTING="0" />
                    //return new SBM_DONE()
                    //{
                    //    ID_DISPATCHER = Convert.ToInt32(xml.DocumentElement.GetAttribute("ID_DISPATCHER")),
                    //    ID_SERVICE = Convert.ToInt16(xml.DocumentElement.GetAttribute("ID_SERVICE")),
                    //    ID_OWNER = Convert.ToInt16(xml.DocumentElement.GetAttribute("ID_OWNER")),
                    //    ID_PRIVATE = xml.DocumentElement.GetAttribute("ID_PRIVATE"),
                    //    PARAMETERS = xml.DocumentElement.GetAttribute("PARAMETERS"),
                    //    REQUESTED = DateTimeOffset.Parse(xml.DocumentElement.GetAttribute("REQUESTED")),
                    //    STARTED = DateTimeOffset.Parse(xml.DocumentElement.GetAttribute("STARTED")),
                    //    ENDED = DateTimeOffset.Parse(xml.DocumentElement.GetAttribute("ENDED")),
                    //    ID_DONE_STATUS = Convert.ToByte(xml.DocumentElement.GetAttribute("ID_DONE_STATUS")),
                    //    RESULT = xml.DocumentElement.GetAttribute("RESULT"),
                    //    ID_REMOTING = Convert.ToInt32(xml.DocumentElement.GetAttribute("ID_REMOTING"))
                    //};

                    var ID_DISPATCHER = Convert.ToInt32(xml.DocumentElement.GetAttribute("ID_DISPATCHER"));
                    return ctxt.SBM_DONE.Find(ID_DISPATCHER);
                }
            }
        }
    }
}
