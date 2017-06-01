using SBM.Component;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBM.Web
{
    public partial class Service : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            using (var dbHelper = new DbHelper())
            {
                GridService.DataSource = dbHelper.GetServices().Select(x => new
                {
                    x.ID_SERVICE,
                    x.DESCRIPTION,
                    x.VERSION,
                    SERVICE_TYPE = x.SBM_SERVICE_TYPE.DESCRIPTION,
                    x.ASSEMBLY_FILE,
                    x.MAX_TIME_RUN,
                    CPU = (x.x86 ? "32" : "64"),
                    ENABLED = (x.ENABLED && x.ID_SERVICE > 0),
                    DISABLED = !x.ENABLED,
                    FILES = Dir(x.ASSEMBLY_PATH)
                });

                GridService.DataBind();
            }
        }

        private String Dir(string assembly_path)
        {
            var dir_root = ConfigurationManager.AppSettings.Get("SBM_ROOT_PATH");
            var dir_app = new DirectoryInfo(Path.Combine(dir_root, "Repository", assembly_path));

            var list = new StringBuilder();
            if (dir_app.Exists)
            {
                foreach (var file in dir_app.GetFiles())
                {
                    list.AppendFormat("{0}\t{1,12:#,##0}\t{2}\n", 
                        file.LastWriteTime.ToString("dd/MM/yyyy hh:mm tt"), 
                        file.Length, file.Name);
                }
            }

            return list.ToString();
        }

        protected void Add_Click(object sender, ImageClickEventArgs e)
        {
            Server.Transfer("ServiceEdit.aspx");
        }

        protected void Edit_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;
            Context.Items["service_id"] = button.CommandArgument;

            Server.Transfer("ServiceEdit.aspx");
        }

        protected void Deactivate_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;
            var service_id = Convert.ToInt32(button.CommandArgument);

            using (var dbHelper = new DbHelper())
            {
                var service = dbHelper.GetService(service_id);

                if (service != null)
                {
                    service.ENABLED = false;
                    dbHelper.Save(service);
                }
            }
            Response.Redirect("Service.aspx");
        }

        protected void Activate_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;
            var service_id = Convert.ToInt32(button.CommandArgument);

            using (var dbHelper = new DbHelper())
            {
                var service = dbHelper.GetService(service_id);

                if (service != null)
                {
                    service.ENABLED = true;
                    dbHelper.Save(service);
                }
            }
            Response.Redirect("Service.aspx");
        }
    }
}