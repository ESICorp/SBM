using SBM.Component;
using SBM.Model;
using System;
using System.Web.UI;

namespace SBM.Web
{
    public partial class OwnerEdit : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var id = Context.Items["owner_id"] as string;

            if (id != null)
            {
                owner_id.Value = id;

                using (var dbHelper = new DbHelper())
                {
                    var owner = dbHelper.GetOwner(Convert.ToInt32(id));

                    description.Text = owner.DESCRIPTION;
                    token.Text = owner.TOKEN;
                }
            }
        }

        protected void Save_Click(object sender, ImageClickEventArgs e)
        {
            using (var dbHelper = new DbHelper())
            {
                bool isNew = false;
                SBM_OWNER owner = null;

                if (!string.IsNullOrEmpty(owner_id.Value))
                {
                    owner = dbHelper.GetOwner(Convert.ToInt32(owner_id.Value));
                }
                if (owner == null)
                {
                    owner = new SBM_OWNER();
                    isNew = true;
                    owner.ENABLED = true;
                }

                owner.DESCRIPTION = description.Text;
                owner.TOKEN = token.Text;

                if (isNew)
                {
                    dbHelper.Insert(owner);
                }
                else
                {
                    dbHelper.Save(owner);
                }
            }
            Server.Transfer("Owner.aspx");
        }

        protected void Back_Click(object sender, ImageClickEventArgs e)
        {
            Server.Transfer("Owner.aspx");
        }
    }
}