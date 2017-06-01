using SBM.Component;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBM.Web
{
    public partial class Scheduler : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //var tz = Request.Cookies["tz"];

            using (var dbHelper = new DbHelper())
            {
                GridTimer.DataSource = dbHelper.GetTimers().Select(x => new
                {
                    x.ID_SERVICE_TIMER,
                    x.ID_SERVICE,
                    x.ID_OWNER,
                    x.SBM_SERVICE,
                    x.SBM_OWNER,
                    x.NEXT_TIME_RUN,
                    x.LAST_TIME_RUN,
                    x.PARAMETERS,
                    x.RUN_INTERVAL,
                    x.CRONTAB,
                    x.ENABLED,
                    DISABLED = !x.ENABLED
                });

                GridTimer.DataBind();
            }
        }

        protected void Add_Click(object sender, ImageClickEventArgs e)
        {
            Server.Transfer("TimerEdit.aspx");
        }

        protected void Edit_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;

            Context.Items["id_service_timer"] = button.CommandArgument;

            Server.Transfer("TimerEdit.aspx");
        }

        protected void Deactivate_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;

            var id_service_timer = Convert.ToInt32(button.CommandArgument);

            using (var dbHelper = new DbHelper())
            {
                var timer = dbHelper.GetTimer(id_service_timer);

                if (timer != null)
                {
                    timer.ENABLED = false;
                    dbHelper.Save(timer);
                }
            }
            Response.Redirect("Timer.aspx");
        }

        protected void Activate_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;

            var id_service_timer = Convert.ToInt32(button.CommandArgument);

            using (var dbHelper = new DbHelper())
            {
                var timer = dbHelper.GetTimer(id_service_timer);

                if (timer != null)
                {
                    timer.ENABLED = true;
                    dbHelper.Save(timer);
                }
            }
            Response.Redirect("Timer.aspx");
        }
    }
}