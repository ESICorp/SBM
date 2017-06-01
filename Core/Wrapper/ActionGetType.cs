using SBM.Common;
using System;
using System.IO;
using System.Reflection;

namespace SBM.Wrapper
{
    public class ActionGetType : Action
    {
        public ActionGetType(Request request, Response response)
            : base(request, response)
        {
        }

        public override void Execute()
        {
            try
            {
                Log.Debug("SBM.Wrapper [ActionGetType.Execute] " + base.Request.FileFullName);
                
                //var assembly = Program.AppDomain.Load(File.ReadAllBytes(base.Request.FileFullName));
                
                var handler = new DomainEventHandler(string.Empty);
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += handler.ReflectionOnlyAssemblyResolve;
                
                var assembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(base.Request.FileFullName));
                
                //Si pudo levantar el assembly
                if (assembly == null)
                {
                    throw new Exception("Couldn't load assembly " + base.Request.FileFullName);
                }
                else
                {
                    //Recorre las clases
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        //Si la clase implementa Batch
                        if (string.Equals(type.BaseType.Name, typeof(Batch).Name))
                        {
                            Log.Debug("SBM.Wrapper [ActionGetType.Execute] return " + type.FullName);

                            base.Response.SetValue(type.FullName);

                            break;
                        }
                        else
                        {
                            Log.Debug("SBM.Wrapper [ActionGetType.Execute] ignoring " + type.FullName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                base.Response.SetException(e);
            }
        }
    }
}
