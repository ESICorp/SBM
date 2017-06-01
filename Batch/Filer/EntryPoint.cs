using SBM.Common;
using Microsoft.PowerShell.Commands;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace SBM.Filer
{
    public class EntryPoint : Batch
    {
        //#if DEBUG
        //        public static int Main(string[] args)
        //        {
        //            string xml = string.Format("<Parameters><Target Computer='{0}' Username='{1}' Password='{2}'><Command>{3}</Command></Target><Log>{4}</Log></Parameters>",
        //                //@"10.146.137.118",
        //                //"acastiglia",
        //                //"Sanguineti1!",
        //                @"localhost",
        //                @"corp-aa\andres.castiglia",
        //                "Axa0707+",


        //                //@"Get-ChildItem C:\
        //                //Get-Process -Name System",
        //                @"$shell = New-Object -ComObject Shell.Application
        //                $zip = $shell.NameSpace(""C:\temp\del\PipeList.zip"")
        //                foreach ($item in $zip.items())
        //                {
        //                    $shell.Namespace(""C:\temp\del"").copyhere($item)
        //                }",

        //                @"c:\temp\del\filer[##].txt");

        //            return 0;
        //        }
        //#endif

        public override void Init()
        {
            Parameter.Parse(base.PARAMETERS);

            TraceLog.Configure();
        }

        public override object Process(object source)
        {
            var step = "create session state";

            try { 

                var initial = InitialSessionState.Create();

                step = "add command";

                initial.Commands.Add(new SessionStateCmdletEntry("Invoke-Command",
                    typeof(InvokeCommandCommand), string.Empty));

                step = "create run space";

                using (var runspace = RunspaceFactory.CreateRunspace(initial))
                {
                    runspace.Open();

                    step = "init powershell";

                    using (var shell = PowerShell.Create())
                    {
                        shell.Runspace = runspace;

                        Collection<PSObject> output = null;

                        step = "build script";

                        using (var password = new SecureString())
                        {
                            foreach (var c in Parameter.Target.Password.ToCharArray())
                                password.AppendChar(c);

                            //Invoke-Command -ComputerName 10.146.137.118 -ScriptBlock { Get-ChildItem C:\ } -Credential acastiglia

                            shell.AddCommand("Invoke-Command")
                                .AddParameter("ComputerName", new[] { Parameter.Target.Computer })
                                .AddParameter("ScriptBlock", ScriptBlock.Create(Parameter.Target.Command))
                                .AddParameter("Credential", new PSCredential(Parameter.Target.Username, password));

                            step = "run script";

                            output = shell.Invoke();
                        }

                        step = "get error";

                        foreach (var item in shell.Streams.Error)
                        {
                            TraceLog.AddError(
                                item.CategoryInfo.Activity == null ? item.CategoryInfo.Category.ToString() :
                                ("Couldn't execute " + item.CategoryInfo.Activity), item.Exception);
                        }

                        step = "get output";

                        RESPONSE = string.Empty;

                        foreach (var item in output)
                        {
                            RESPONSE += item.ToString() + '\n';
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TraceLog.AddError("Couldn't " + step, e);
            }

            return null;
        }

        public override void Destroy()
        {
            try
            {
                if (TraceLog.Count > 0)
                {
                    throw TraceLog.Exceptions;
                }
            }
            finally
            {
                TraceLog.Dispose();
            }
        }
    }
}
