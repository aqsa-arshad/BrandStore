// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for downloads.
    /// </summary>
    public partial class downloads : SkinBase
    {
        public override bool RequireScriptManager
        {
            get
            {
                return true;
            }
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            RequireSecurePage();
            RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
            SectionTitle = AppLogic.GetString("download.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            if (!Page.IsPostBack)
            {
                LoadDownloadGridViews();

                if (AppLogic.AppConfigBool("Download.ShowRelatedProducts"))
                    LoadRelatedProducts();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected void gvDownloadsAvailable_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            ClearError();
            int shoppingCartRecordId = 0;
            int.TryParse(e.CommandArgument.ToString(), out shoppingCartRecordId);
            if (e.CommandName.Equals("download") && shoppingCartRecordId > 0)
            {
                if (AppLogic.AppConfigBool("Download.StreamFile"))
                    StreamFile(shoppingCartRecordId);
            }
        }

        protected void StreamFile(int shoppingCartRecordId)
        {
            DownloadItem downloadItem = new DownloadItem();
            downloadItem.Load(shoppingCartRecordId);

            string filepath = CommonLogic.SafeMapPath(downloadItem.DownloadLocation);
            string filename = Path.GetFileName(filepath);
            try
            {
                Response.ContentType = downloadItem.ContentType;
                Response.AppendHeader("Content-Disposition", string.Format("attachment; filename={0}", filename));
                Response.TransmitFile(filepath);
                Response.End();
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                ShowError(AppLogic.GetString("download.aspx.15", ThisCustomer.LocaleSetting));
            }
        }

        protected void LoadDownloadGridViews()
        {
            List<DownloadItem> availableDownloads = new List<DownloadItem>();
            List<DownloadItem> pendingDownloads = new List<DownloadItem>();
            List<DownloadItem> expiredDownloads = new List<DownloadItem>();

            SqlParameter[] sqlParamaters = { new SqlParameter("@CustomerId", ThisCustomer.CustomerID) };

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select distinct ShoppingCartRecId from Orders_ShoppingCart where IsDownload=1 and CustomerId=@CustomerId", sqlParamaters, con))
                {
                    while (rs.Read())
                    {
                        int shoppingCartRecId = DB.RSFieldInt(rs, "ShoppingCartRecId");
                        DownloadItem item = new DownloadItem();
                        item.Load(shoppingCartRecId);

                        switch (item.Status)
                        {
                            case DownloadItem.DownloadItemStatus.Available:
                                availableDownloads.Add(item);
                                break;
                            case DownloadItem.DownloadItemStatus.Pending:
                                pendingDownloads.Add(item);
                                break;
                            case DownloadItem.DownloadItemStatus.Expired:
                                expiredDownloads.Add(item);
                                break;
                        }
                    }
                }
            }

            gvDownloadsAvailable.DataSource = availableDownloads;
            gvDownloadsAvailable.DataBind();

            gvDownloadsPending.DataSource = pendingDownloads;
            gvDownloadsPending.DataBind();

            gvDownloadsExpired.DataSource = expiredDownloads;
            gvDownloadsExpired.DataBind();
        }

        protected void LoadRelatedProducts()
        {
            //imgRelatedProductsTab
            List<Product> relatedProducts = new List<Product>();

            SqlParameter[] sqlParamaters = { new SqlParameter("@CustomerId", ThisCustomer.CustomerID) };

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(@"select ProductID from Orders_ShoppingCart where IsDownload=1 and CustomerId=@CustomerId", sqlParamaters, con))
                {
                    while (rs.Read())
                    {
                        foreach (Product p in GetRelatedProducts(DB.RSFieldInt(rs, "ProductID")))
                        {
                            if (!relatedProducts.Select(s => s.ProductID).Contains(p.ProductID))
                                relatedProducts.Add(p);
                        }
                    }
                }
            }

            imgRelatedProductsTab.ImageUrl = AppLogic.LocateImageURL(String.Format("~/App_Themes/skin_{0}/images/relatedproducts.gif", SkinID.ToString()));
            rptRelatedProducts.DataSource = relatedProducts.OrderBy(o => Guid.NewGuid()).Take(4);
            rptRelatedProducts.DataBind();

            imgRelatedProductsTab.Visible = relatedProducts.Count() > 0;
        }

        protected List<Product> GetRelatedProducts(int productId)
        {
            List<Product> relatedProducts = new List<Product>();
            SqlParameter[] sqlParamaters = 
			{ 
				new SqlParameter("@CustomerGUID", ThisCustomer.CustomerGUID), 
				new SqlParameter("@ProductID", productId), 
				new SqlParameter("@CustomerLevelID", ThisCustomer.CustomerLevelID), 
				new SqlParameter("@InvFilter", AppLogic.AppConfigNativeInt("HideProductsWithLessThanThisInventoryLevel")), 
				new SqlParameter("@affiliateID", ThisCustomer.AffiliateID), 
				new SqlParameter("@storeID", ThisCustomer.StoreID)
			};

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(@"EXEC aspdnsf_GetCustomersRelatedProducts @CustomerGUID, @ProductID, @CustomerLevelID, @InvFilter, @affiliateID, @storeID", sqlParamaters, con))
                {
                    while (rs.Read())
                    {
                        relatedProducts.Add(new Product(DB.RSFieldInt(rs, "ProductID")));
                    }
                }
            }
            return relatedProducts;
        }

        protected void ShowError(string error)
        {
            ltError.Text = error;
        }

        protected void ClearError()
        {
            ltError.Text = string.Empty;
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

        protected void gvDownloadsPending_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HyperLink hp = (HyperLink)e.Row.FindControl("hlDownloadLink");
                HiddenField hf = (HiddenField)e.Row.FindControl("hfDownloadLocation");
                hp.NavigateUrl = "~/images/Product/icon/" + hf.Value;
                hp.Text = "Download";
            }
        }
    }
}
