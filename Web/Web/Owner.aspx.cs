using SBM.Component;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBM.Web
{
    public partial class Owner : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            using (var dbHelper = new DbHelper())
            {
                GridOwner.DataSource = dbHelper.GetOwners().Select(x => new
                {
                    x.ID_OWNER,
                    x.DESCRIPTION,
                    x.TOKEN,
                    x.ENABLED,
                    DISABLED = !x.ENABLED
                });

                GridOwner.DataBind();
            }
        }

        protected void Add_Click(object sender, ImageClickEventArgs e)
        {
            Server.Transfer("OwnerEdit.aspx");
        }

        protected void Edit_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;
            Context.Items["owner_id"] = button.CommandArgument;

            Server.Transfer("OwnerEdit.aspx");
        }

        protected void Deactivate_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;
            var owner_id = Convert.ToInt32(button.CommandArgument);

            using (var dbHelper = new DbHelper())
            {
                var owner = dbHelper.GetOwner(owner_id);

                if (owner != null)
                {
                    owner.ENABLED = false;
                    dbHelper.Save(owner);
                }
            }
            Response.Redirect("Owner.aspx");
        }

        protected void Activate_Click(object sender, ImageClickEventArgs e)
        {
            var button = sender as ImageButton;
            var owner_id = Convert.ToInt32(button.CommandArgument);

            using (var dbHelper = new DbHelper())
            {
                var owner = dbHelper.GetOwner(owner_id);

                if (owner != null)
                {
                    owner.ENABLED = true;
                    dbHelper.Save(owner);
                }
            }
            Response.Redirect("Owner.aspx");
        }
    }
}