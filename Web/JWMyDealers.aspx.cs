using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public class temp
    {
        public String Name { get; set; }
        public String Id { get; set; }
    }

    /// <summary>
    /// View User Dealers from SFDC
    /// </summary>
    public partial class JWMyDealers : SkinBase
    {
        protected static int PageCount;
        protected static int CurrentPageNumber;
        protected static int PageSize;
        static List<SFDCSoapClient.Account> lstSFDCAccount = new List<SFDCSoapClient.Account>();
        /// <summary>
        /// Override JeldWen Master Template
        /// </summary>
        protected override string OverrideTemplate()
        {
            String MasterHome = AppLogic.HomeTemplate();

            if (MasterHome.Trim().Length == 0)
            {
                MasterHome = "JeldWenTemplate";
            }

            if (MasterHome.EndsWith(".ascx"))
            {
                MasterHome = MasterHome.Replace(".ascx", ".master");
            }

            if (!MasterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                MasterHome = MasterHome + ".master";
            }

            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + base.SkinID.ToString() + "/" + MasterHome)))
            {
                MasterHome = "JeldWenTemplate";
            }

            return MasterHome;
        }

        /// <summary>
        /// Page Load Event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            RequireSecurePage();
            PageSize = Convert.ToInt32(PageSizeList.SelectedValue);
            RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));
            if (!Page.IsPostBack)
            {
                lstSFDCAccount = AuthenticationSSO.GetSubordinateAccounts(ThisCustomer.SFDCQueryParam);
                LoadMyDealers(1);
            }
        }

        /// <summary>
        /// LoadMyDealers
        /// </summary>
        /// <param name="pageIndex">pageIndex</param>
        private void LoadMyDealers(int pageIndex)
        {
            if (ThisCustomer.HasSubordinates)
            {
                if (lstSFDCAccount.Count > 0)
                {
                    rptMyDealers.DataSource = lstSFDCAccount.Skip((pageIndex - 1) * PageSize).Take(PageSize);
                    rptMyDealers.DataBind();
                    lblDealerNotFound.Visible = false;
                    PopulatePager(lstSFDCAccount.Count, pageIndex);
                }
            }
        }

        /// <summary>
        /// Handles the Changed event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Changed(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty((sender as LinkButton).CommandArgument)) return;
            var pageIndex = int.Parse((sender as LinkButton).CommandArgument);
            LoadMyDealers(pageIndex);
        }

        /// <summary>
        /// Populates the pager.
        /// </summary>
        /// <param name="recordCount">The record count.</param>
        /// <param name="currentPage">The current page.</param>
        private void PopulatePager(int recordCount, int currentPage)
        {
            var dblPageCount = (double)((decimal)recordCount / Convert.ToDecimal(PageSize));
            PageCount = (int)Math.Ceiling(dblPageCount);
            var pages = new List<ListItem>();
            if (PageCount > 0)
            {
                if (CurrentPageNumber < currentPage)
                {
                    pages.Add(currentPage > 1
                        ? new ListItem("< " + "Previous", (currentPage - 1).ToString(), true)
                        : new ListItem("< " + "Previous", (currentPage - 1).ToString(), false));
                    pages.Add(new ListItem(currentPage + " of " + PageCount, string.Empty, false));
                    pages.Add(currentPage + 1 <= PageCount
                        ? new ListItem("Next" + " >", (currentPage + 1).ToString(), true)
                        : new ListItem("Next" + " >", string.Empty, false));
                }
                else
                {
                    pages.Add(currentPage - 1 < 1
                        ? new ListItem("< " + "Previous", (currentPage - 1).ToString(), false)
                        : new ListItem("< " + "Previous", (currentPage - 1).ToString(), true));
                    pages.Add(new ListItem(currentPage + " of " + PageCount, string.Empty, false));
                    pages.Add(currentPage + 1 <= PageCount
                        ? new ListItem("Next" + " >", (currentPage + 1).ToString(), true)
                        : new ListItem("Next" + " >", (currentPage + 1).ToString(), false));
                }
                CurrentPageNumber = currentPage;
            }
            rptPager.DataSource = pages;
            rptPager.DataBind();
        }
        private void LoadMyDealersInAscendingOrDesendingOrder(int pageIndex, int flag)
        {
            if (ThisCustomer.HasSubordinates)
            {
                if (flag == 1)
                {
                    lstSFDCAccount = lstSFDCAccount.OrderBy(x => x.Name).ToList();
                }
                else if (flag == 2)
                {
                    lstSFDCAccount = lstSFDCAccount.OrderByDescending(x => x.Name).ToList();
                }

                if (lstSFDCAccount.Count > 0)
                {
                    rptMyDealers.DataSource = lstSFDCAccount.Skip((pageIndex - 1) * PageSize).Take(PageSize);
                    rptMyDealers.DataBind();
                    lblDealerNotFound.Visible = false;
                    PopulatePager(lstSFDCAccount.Count, pageIndex);
                }
            }
        }
        protected void OrderBylist_SelectedIndexChanged(object sender, EventArgs e)
        {
            filterDealers();
        }
        protected void PageSizelist_SelectedIndexChanged(object sender, EventArgs e)
        {
            filterDealers();
        }
        private void filterDealers()
        {
            PageSize = Convert.ToInt32(PageSizeList.SelectedValue);
            String selectedValue = OrderBylist.SelectedValue;
            if (selectedValue.Equals("nameAsc"))
            {
                LoadMyDealersInAscendingOrDesendingOrder(1, 1);
            }
            else if (selectedValue.Equals("nameDsc"))
            {
                LoadMyDealersInAscendingOrDesendingOrder(1, 2);
            }
            else
            {
                LoadMyDealers(1);
            }
            PageSizeList.Items.FindByValue(PageSizeList.SelectedValue.ToString()).Selected = true;
            OrderBylist.ClearSelection();
            OrderBylist.Items.FindByValue(selectedValue).Selected = true;
        }
        protected void DealersHeader_Click(object sender, EventArgs e)
        {
            PageSize = Convert.ToInt32(PageSizeList.SelectedValue);
            String selectedValue = "nameAsc";
            LoadMyDealersInAscendingOrDesendingOrder(1, 1);
            PageSizeList.Items.FindByValue(PageSizeList.SelectedValue.ToString()).Selected = true;
            OrderBylist.ClearSelection();
            OrderBylist.Items.FindByValue(selectedValue).Selected = true;
        }

    }
}