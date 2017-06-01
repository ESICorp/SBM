using SBM.Component;
using System;

namespace SBM.Web
{
    public partial class Default : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            using (var dbHelper = new DbHelper())
            {
                Repeater1.DataSource = dbHelper.GetServices();

                Repeater1.DataBind();
            }
        }
    }
}