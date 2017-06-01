using SBM.Component;
using System;

namespace SBM.Web
{
    public partial class Event : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                using (var dbHelper = new DbHelper())
                {
                    GridEvent.VirtualItemCount = dbHelper.CountEventLog();

                    GridEvent.DataSource = dbHelper.GetEventLog(0, GridEvent.PageSize);

                    GridEvent.DataBind();
                }
            }
        }

        protected void GridEvent_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            GridEvent.PageIndex = e.NewPageIndex;

            using (var dbHelper = new DbHelper())
            {
                GridEvent.DataSource = dbHelper.GetEventLog(e.NewPageIndex, GridEvent.PageSize);

                GridEvent.DataBind();
            }
        }
    }
}