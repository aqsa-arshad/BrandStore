using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    public partial class MyDownloads : SkinBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RequireSecurePage();
            if (!Page.IsPostBack)
            {
                LoadDownloadItems();
            }
        }

        protected override string OverrideTemplate()
        {
            var masterHome = AppLogic.HomeTemplate();
            if (masterHome.Trim().Length == 0)
            {
                masterHome = "JeldWenTemplate";
            }
            if (masterHome.EndsWith(".ascx"))
            {
                masterHome = masterHome.Replace(".ascx", ".master");
            }
            if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                masterHome = masterHome + ".master";
            }
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }

        protected void LoadDownloadItems()
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_GetCustomerDownloads", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CUSTOMERID", ThisCustomer.CustomerID);
                        IDataReader reader = cmd.ExecuteReader();
                        rptDownloadableItems.DataSource = reader;
                        rptDownloadableItems.DataBind();
                        conn.Close();
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            noDownloadProductsFound.Visible = (rptDownloadableItems.Items.Count == 0);
        }

        protected void rptAddresses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                (e.Item.FindControl("hlDownload") as HyperLink).NavigateUrl = "~/images/Product/icon/" +
                    (e.Item.FindControl("hfDownloadLocation") as HiddenField).Value;
                (e.Item.FindControl("hlDownload") as HyperLink).Text = "Download";
                if ((e.Item.FindControl("hfSKU") as HiddenField).Value != null)
                {
                    (e.Item.FindControl("lblProductSKU") as Label).Text = "SKU: " +
                                                                          (e.Item.FindControl("hfSKU") as HiddenField)
                                                                              .Value;
                }
                if ((e.Item.FindControl("hfDescription") as HiddenField).Value != null)
                {
                    if ((e.Item.FindControl("hfDescription") as HiddenField).Value.Length > 60)
                        (e.Item.FindControl("lblDescription") as Label).Text = (e.Item.FindControl("hfDescription") as HiddenField)
                                                                              .Value.Take(60).Aggregate("", (x, y) => x + y) + " ...";
                    else
                    {
                        (e.Item.FindControl("lblDescription") as Label).Text =
                            (e.Item.FindControl("hfSKU") as HiddenField).Value;
                    }
                }
            }
        }
    }
}