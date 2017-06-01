using SBM.Component;
using SBM.Model;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBM.Web
{
    public partial class TimerEdit : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            id_service_timer.Value = Context.Items["id_service_timer"] as string;
            var isNew = string.IsNullOrEmpty(id_service_timer.Value);

            using (var dbHelper = new DbHelper())
            {
                service.Items.Add(new ListItem(string.Empty, string.Empty));
                dbHelper.GetServices().ToList().ForEach(x => service.Items.Add(
                    new ListItem(x.DESCRIPTION, x.ID_SERVICE.ToString(), x.ENABLED)));

                owner.Items.Add(new ListItem(string.Empty, string.Empty));
                dbHelper.GetOwners().ToList().ForEach(x => owner.Items.Add(
                    new ListItem(x.DESCRIPTION, x.ID_OWNER.ToString(), x.ENABLED)));

                if (!isNew)
                {
                    var timer = dbHelper.GetTimer(Convert.ToInt32(id_service_timer.Value));

                    service.SelectedValue = timer.ID_SERVICE.ToString();
                    owner.SelectedValue = timer.ID_OWNER.ToString();

                    id_private.Text = timer.ID_PRIVATE;
                    parameters.Text = timer.PARAMETERS;
                    run_interval.Text = timer.RUN_INTERVAL.ToString();
                    crontab.Text = timer.CRONTAB;
                }
            }
        }

        protected void Save_Click(object sender, ImageClickEventArgs e)
        {
            var isNew = string.IsNullOrEmpty(id_service_timer.Value);

            using (var dbHelper = new DbHelper())
            {
                SBM_SERVICE_TIMER timer = null;

                if (isNew)
                {
                    timer = new SBM_SERVICE_TIMER();
                    timer.ENABLED = true;
                }
                else
                {
                    timer = dbHelper.GetTimer(Convert.ToInt32(id_service_timer.Value));
                }

                timer.ID_SERVICE = Convert.ToInt16(service.SelectedValue);
                timer.ID_OWNER = Convert.ToInt16(owner.SelectedValue);
                timer.ID_PRIVATE = id_private.Text;
                timer.PARAMETERS = parameters.Text;

                timer.RUN_INTERVAL = Convert.ToInt16(run_interval.Text);
                timer.CRONTAB = crontab.Text;

                if (isNew)
                {
                    dbHelper.Insert(timer);
                }
                else
                {
                    dbHelper.Save(timer);
                }
            }

            Server.Transfer("Timer.aspx");
        }

        protected void Back_Click(object sender, ImageClickEventArgs e)
        {
            Server.Transfer("Timer.aspx");
        }
    }
}