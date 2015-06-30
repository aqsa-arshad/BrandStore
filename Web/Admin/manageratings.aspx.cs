// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for manageratings.
	/// </summary>
	public partial class manageratings : AdminPageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache"); 
            SectionTitle = "Manage Ratings";

            Page.Form.DefaultButton = btnSubmit.UniqueID;
            sqlGridSource.ConnectionString = DB.GetDBConn();

            tdFilterSelect.Visible = Store.IsMultiStore;
            tdStoreFilter.Visible = Store.IsMultiStore;
            tdStoreSelect.Visible = Store.IsMultiStore;
            tdStoreTitle.Visible = Store.IsMultiStore;

            if (!IsPostBack)
            {
                txtShowDays.Text = "7";

                foreach (Store s in Store.GetStores(true))
                {
                    ddlStoreID.Items.Add(new ListItem(s.Name, s.StoreID.ToString(), true));
                }
            }
            
		}

        protected void grdRatings_RowDataBound(Object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ImageButton imgb = (ImageButton)e.Row.FindControl("btnDelete");
                string onclickScript = "return confirm('" + AppLogic.GetString("admin.manageratings.confirmdelete", SkinID, LocaleSetting) + "')";

                imgb.Attributes.Add("onclick", onclickScript);
            }

            if (e.Row.RowState == DataControlRowState.Edit)
            {
                DropDownList ddl = (DropDownList)e.Row.FindControl("ddlRating");
            }
        }

        protected void grdRatings_RowUpdating(Object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow r = grdRatings.Rows[grdRatings.EditIndex];
            
            DropDownList ddl = (DropDownList)r.FindControl("ddlRating");
            TextBox txt = (TextBox)r.FindControl("txtComments");


            e.NewValues["Rating"] = ddl.SelectedValue;
            e.NewValues["Comments"] = txt.Text;
        }

	}
}
