using SBM.Component;
using SBM.Model;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBM.Web
{
    public partial class ServiceEdit : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var id = Context.Items["service_id"] as string;

            using (var dbHelper = new DbHelper())
            {
                service_type.Items.Add(
                        new ListItem(string.Empty, string.Empty));

                dbHelper.GetServiceTypes().ToList().ForEach(x => service_type.Items.Add(
                        new ListItem(x.DESCRIPTION, x.ID_SERVICE_TYPE.ToString())));

                service_parent.Items.Add(
                    new ListItem(string.Empty, string.Empty));

                dbHelper.GetServices().ToList().ForEach(x => service_parent.Items.Add(
                        new ListItem(x.ID_SERVICE == 0 ? "<< ITSELF/FORCE >>" : x.DESCRIPTION, x.ID_SERVICE.ToString())));

                if (id != null)
                {
                    var service = dbHelper.GetService(Convert.ToInt32(id));

                    service_id.Value = id;
                    description.Text = service.DESCRIPTION;
                    service_type.SelectedValue = service.ID_SERVICE_TYPE.ToString();
                    service_parent.SelectedValue = service.ID_PARENT_SERVICE == null ? string.Empty : service.ID_PARENT_SERVICE.ToString();
                    version.Text = service.VERSION;
                    assembly_file.Text = service.ASSEMBLY_FILE;
                    assembly_path.Text = service.ASSEMBLY_PATH;
                    x32.Checked = service.x86;
                    x64.Checked = !service.x86;
                    max_time_run.Text = service.MAX_TIME_RUN.ToString();
                    single_exec.Checked = service.SINGLE_EXEC;
                    domain.Text = service.DOMAIN;
                    user.Text = service.USER;
                    //password.Text = service.PASSWORD;
                }
            }
        }

        protected void Save_Click(object sender, ImageClickEventArgs e)
        {
            using (var dbHelper = new DbHelper())
            {
                bool isNew = false;
                SBM_SERVICE service = null;

                if (!string.IsNullOrEmpty(service_id.Value))
                {
                    service = dbHelper.GetService(Convert.ToInt32(service_id.Value));
                }
                if (service == null)
                {
                    service = new SBM_SERVICE();
                    isNew = true;
                    service.ENABLED = true;
                    service.PUBLISHED = DateTimeOffset.Now;
                }

                service.DESCRIPTION = description.Text;
                service.ID_SERVICE_TYPE = Convert.ToInt16(service_type.SelectedValue);
                if (string.IsNullOrEmpty(service_parent.SelectedValue))
                {
                    service.SBM_SERVICE_PARENT = null;
                    service.ID_PARENT_SERVICE = null;
                }
                else
                {
                    service.ID_PARENT_SERVICE = Convert.ToInt16(service_parent.SelectedValue);
                }
                service.VERSION = version.Text;
                service.x86 = x32.Checked;
                service.MAX_TIME_RUN = Convert.ToInt16(max_time_run.Text);
                service.SINGLE_EXEC = single_exec.Checked;
                service.DOMAIN = domain.Text;
                service.USER = user.Text;
                if (!string.IsNullOrEmpty(password.Text))
                {
                    service.PASSWORD = dbHelper.Crypt(ConfigurationManager.AppSettings.Get("SBM_PHRASE"), password.Text);
                }
                service.ASSEMBLY_PATH = assembly_path.Text;
                service.ASSEMBLY_FILE = assembly_file.Text;

                if (isNew)
                {
                    dbHelper.Insert(service);
                }
                else
                {
                    dbHelper.Save(service);
                }

                if (upload_file.HasFile)
                {
                    try
                    {
                        var dir_root = ConfigurationManager.AppSettings.Get("SBM_ROOT_PATH");
                        var dir_app = Path.Combine(dir_root, "Repository", service.ASSEMBLY_PATH);

                        if (!Directory.Exists(dir_app))
                        {
                            Directory.CreateDirectory(dir_app);
                        }

                        foreach (var f in upload_file.PostedFiles)
                        {
                            f.SaveAs(Path.Combine(dir_app, f.FileName));
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Warn("Dispatcher", "Couldn't Upload Files", ex);
                    }
                }
            }
            Server.Transfer("Service.aspx");
        }

        protected void Back_Click(object sender, ImageClickEventArgs e)
        {
            Server.Transfer("Service.aspx");
        }
    }
}