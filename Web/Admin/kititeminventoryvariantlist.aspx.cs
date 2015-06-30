// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.Linq;
using AspDotNetStorefrontControls;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace AspDotNetStorefrontAdmin
{
    public partial class Admin_kititeminventoryvariantlist : AdminPageBase
    {

        public new Customer ThisCustomer
        {
            get { return Customer.Current; }
        }

        private const string EMPTY_NULL_DISPLAY = "-";

        protected string FormatNumericDisplay(decimal number)
        {
            if (number == decimal.Zero)
            {
                return EMPTY_NULL_DISPLAY;
            }
            else
            {
                return number.ToString("0.00");
            }
        }

        protected override void OnInit(EventArgs e)
        {
            Title = AppLogic.GetString("admin.sectiontitle.kititeminventoryvariantlist", SkinID, LocaleSetting);

            ctrlSearch.PageSizes = new List<int>(new int[] { 10, 15, 20, 25, 30, 40, 50 });
            if (!IsPostBack)
            {
                ctrlSearch.CurrentPageSize = 10;
            }

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (!Page.IsPostBack)
            {
                SetupDefaultFilters();
            }
        }

        protected void ctrlSearch_Filter(object sender, FilterEventArgs e)
        {
            Search searchControl = ctrlSearch.Search;

            string searchFilter = string.Empty;
            string alphaFilter = string.Empty;

            switch (ctrlSearch.CurrentFilterType)
            {
                case FilterType.Search:
                    searchFilter = ctrlSearch.CurrentFilter;
                    break;
                case FilterType.AlphaFilter:
                    alphaFilter = ctrlSearch.CurrentFilter;
                    break;
            }

            DoSearch(searchFilter, alphaFilter, e.Page);
        }

        private void SetupDefaultFilters()
        {
            ctrlSearch.CurrentFilter = ProductForKit.NO_FILTER;
            ctrlSearch.CurrentFilterType = FilterType.Search;
            ctrlSearch.CurrentPage = 1;
        }

        private void DoSearch(string searchFilter, string alphaFilter, int page)
        {
            PaginatedList<ProductForKit> allProducts = ProductForKit.GetAll(ThisCustomer, alphaFilter, searchFilter, ctrlSearch.CurrentPageSize, page);
            BindData(allProducts);
        }

        private void BindData(PaginatedList<ProductForKit> datasource)
        {
            ctrlSearch.AllCount = datasource.TotalCount;
            ctrlSearch.StartCount = datasource.StartIndex;
            ctrlSearch.EndCount = datasource.EndIndex;

            ctrlSearch.PageCount = datasource.TotalPages;
            ctrlSearch.CurrentPage = 1;

            Repeater rptProducts = ctrlSearch.FindControl<Repeater>("rptProducts");
            rptProducts.DataSource = datasource;
            rptProducts.DataBind();
        }

        protected void rptVariants_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ProductVariant variant = e.Item.DataItemAs<ProductVariant>();
                HyperLink lnkName = e.Item.FindControl("lnkName") as HyperLink;
                ProductVariantForKit DataItem = e.Item.DataItem as ProductVariantForKit;

                JavaScriptSerializer ser = new JavaScriptSerializer();
                string serialized = ser.Serialize(e.Item.DataItem);
                lnkName.Attributes["onclick"] = string.Format("CloseAndUpdate({0})", serialized);
                lnkName.Attributes["src"] = "#";
                lnkName.Attributes["style"] = "cursor: pointer;";
                lnkName.Text = XmlCommon.GetLocaleEntry(DataItem.Name, ThisCustomer.LocaleSetting, true);
            }
        }

        private const int DEFAULT_PAGE_SIZE = 10;

        protected void ctrlSearch_ContentCreated(object sender, FilterEventArgs e)
        {
                string searchFilter = string.Empty;
                string alphaFilter = string.Empty;

                switch (e.Type)
                {
                    case FilterType.Search:
                        searchFilter = e.Filter;
                        break;
                    case FilterType.AlphaFilter:
                        alphaFilter = e.Filter;
                        break;
                }

                int navigateToPage = e.Page;

                PaginatedList<ProductForKit> products = ProductForKit.GetAll(ThisCustomer, searchFilter, alphaFilter, DEFAULT_PAGE_SIZE, navigateToPage);

                
                ctrlSearch.AllCount = products.TotalCount;
                ctrlSearch.StartCount = products.StartIndex;
                ctrlSearch.EndCount = products.EndIndex;

                ctrlSearch.PageCount = products.TotalPages;
                ctrlSearch.CurrentPage = products.CurrentPage;

                Repeater rptProducts = ctrlSearch.FindControl<Repeater>("rptProducts");
                rptProducts.DataSource = products;
                rptProducts.DataBind();     
        }

    }
}

