// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class NewAdmin_App_Templates_AdminMaster : AdminMasterPageBase
    {
        //private string AdminMenuSource = AppLogic.AdminLinkUrl("Controls/AdminMenu.ascx", true);
        #region eventHandlers

		protected void Page_Init(object sender, EventArgs e)
		{
			if (Request.UserAgent.IndexOf("AppleWebKit") > 0)
			{
				Request.Browser.Adapters.Clear();
			}
		}

        /// <summary>
        /// Default page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (base.ThisCustomer.Notes.Trim().Length > 0 && base.ThisCustomer.Notes.Contains(".ascx"))
            {
                LoadMenu(base.ThisCustomer.Notes, base.ThisCustomer.LocaleSetting, false);
            }
            else
            {
                LoadMenu("AdminMenu.ascx", base.ThisCustomer.LocaleSetting, true);
            }
            

            if (!String.Equals(CommonLogic.GetThisPageName(false), "default.aspx", StringComparison.OrdinalIgnoreCase))
            {
                if (!String.IsNullOrEmpty(Page.Title))
                {
                    trBreadCrumbRow.Visible = true;
                    ltBreadCrumb.Text = AppLogic.GetString("admin.common.NowIn", ThisCustomer.LocaleSetting) + " " + Page.Title;
                }
            }
            AdminIconLink.HRef = AppLogic.AdminLinkUrl("customerdetail.aspx?customerid=" + ThisCustomer.CustomerID);

            bool IsBeingImpersonated = false;
            try
            {
                IsBeingImpersonated = ((String)HttpContext.Current.Items["IsBeingImpersonated"] == "true");
            }
            catch { }
            spanImpersonationWarning.Visible = IsBeingImpersonated;
        }

        protected void btnStopImpersonation_Click(object sender, EventArgs e)
        {
            ThisCustomer.ThisCustomerSession["IGD"] = "";
            ThisCustomer.ThisCustomerSession["IGDEDITINGORDER"] = "";
            spanImpersonationWarning.Visible = false;
            litUserName.Text = ThisCustomer.FirstName + " " + ThisCustomer.LastName;
        }

        private void LoadMenu(String ctrlToLoad, String localeSetting, bool isDefaultMenu)
        {
            String ctrlName = AppLogic.AdminLinkUrl("Controls/" + ctrlToLoad, true);

            Literal litError = new Literal();
            litError.Mode = LiteralMode.PassThrough;
            litError.Text = AppLogic.GetString("admin.menu.loaderror", localeSetting);

            // couldn't find the default menu...log
            try
            {
                if (!CommonLogic.FileExists(ctrlName) && isDefaultMenu)
                {
                    pnlMenu.Controls.Add(litError);

                    // no menu file found...log
                    SysLog.LogMessage("File Not Found", "Unable to load menu control {0}.".FormatWith(ctrlToLoad), MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }
                else if (CommonLogic.FileExists(ctrlName))
                {
                    try
                    {
                        pnlMenu.Controls.Add(LoadControl(ctrlName));
                    }
                    catch
                    {
                        // no menu file found...log
                        SysLog.LogMessage("File Not Found", "Unable to load menu control {0}.".FormatWith(ctrlToLoad), MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                    }
                }
                else
                {
                    // couldn't load a custom menu so display an error message to the user and log rather than showing 
                    // an entire menu to an admin user that maybe shouldn't see it

                    pnlMenu.Controls.Add(litError);

                    // no menu file found...log
                    SysLog.LogMessage("File Not Found", "Unable to load menu control {0}.".FormatWith(ctrlToLoad), MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);

                }
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                pnlMenu.Controls.Add(litError);
            }
        }

        /// <summary>
        /// Handles search functionality within the admin page header
        /// </summary>
        protected void search_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("search.aspx?searchterm=" + txtSearch.Text);
        }

        #endregion
    }
}
