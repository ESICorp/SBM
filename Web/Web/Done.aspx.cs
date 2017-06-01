using SBM.Component;
using System;

namespace SBM.Web
{
    public partial class Done : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                using (var dbHelper = new DbHelper())
                {
                    GridDone.VirtualItemCount = dbHelper.CountDone();

                    GridDone.DataSource = dbHelper.GetDone(0, GridDone.PageSize);

                    GridDone.DataBind();
                }
            }
        }

        protected void GridDone_PageIndexChanging(object sender, System.Web.UI.WebControls.GridViewPageEventArgs e)
        {
            GridDone.PageIndex = e.NewPageIndex;

            using (var dbHelper = new DbHelper())
            {
                GridDone.DataSource = dbHelper.GetDone(e.NewPageIndex, GridDone.PageSize);

                GridDone.DataBind();
            }
        }
    }
}