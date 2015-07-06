// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefront;
using AspDotNetStorefrontBuySafe;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    public partial class buysafesetup : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            this.Title = "buySAFE for AspDotNetStorefront";

            SetError("", false);

            if (!IsPostBack)
            {
                ChooseInitialDisplay();
            }
        }

        private void ChooseInitialDisplay()
        {
            SetPageState(BuySafeController.GetBuySafeState());
        }

        private void SetPageState(BuySafeState pageState)
        {
            toppnlEnabledFreeTrialExpired.Visible =
                toppnlWorking.Visible =
                pnlFreeTrialStatus.Visible =
                toppnlError.Visible =
                toppnlNotEnabledFreeTrialAvailable.Visible =
                toppnlNotEnabledFreeTrialExpired.Visible =
                pnlActivationForm.Visible =
                false;
            switch (pageState)
            {
                case BuySafeState.NotEnabledFreeTrialAvailable:
                    toppnlNotEnabledFreeTrialAvailable.Visible = true;
                    break;
                case BuySafeState.NotEnabledFreeTrialExpired:
                    toppnlNotEnabledFreeTrialExpired.Visible = true;
                    break;
                case BuySafeState.EnabledFreeTrialExpired:
                    toppnlEnabledFreeTrialExpired.Visible = true;
                    break;
                case BuySafeState.EnabledOnFreeTrial:
                    pnlFreeTrialStatus.Visible = true;
                    int daysremaining = BuySafeController.DaysRemainingOnTrial();
                    litFreeDaysRemaining.Text = string.Format("You have {0} days remaining on your free trial.", daysremaining);
                    toppnlWorking.Visible = true;
                    pnlActivationForm.Visible = !AppLogic.GlobalConfigBool("BuySafe.ActivationSubmitted");
                    pnlActivated.Visible = AppLogic.GlobalConfigBool("BuySafe.ActivationSubmitted");
                    SetupStoreLists();
                    break;
                case BuySafeState.EnabledFullUserAfterFreeTrial:
                    pnlFreeTrialStatus.Visible = false;
                    toppnlWorking.Visible = true;
                    pnlActivated.Visible = true;
                    SetupStoreLists();
                    break;
                case BuySafeState.Error:
                default:
                    toppnlError.Visible = true;
                    break;
            }
        }

        private void SetupStoreLists()
        {
            BuySafeStoreStatuses statuses = AspDotNetStorefrontBuySafe.BuySafeController.GetStoreStatuses();
            pnlRegisteredStores.Visible = statuses.RegisteredStores.Count > 0;
            pnlUnregisteredStores.Visible = statuses.UnRegisteredStores.Count > 0;

            if (!string.IsNullOrEmpty(AppLogic.GlobalConfig("BuySafe.UserName")))
                litBuySafeUserName.Text = string.Format("<b>Your buySAFE username is {0}</b><br /><br />", AppLogic.GlobalConfig("BuySafe.UserName"));
            else
                litBuySafeUserName.Text = string.Empty;

            repRegisteredStores.DataSource = statuses.RegisteredStores;
            repRegisteredStores.DataBind();
            repUnregisteredStores.DataSource = statuses.UnRegisteredStores;
            repUnregisteredStores.DataBind();
        }

        protected void btnRegisterStore_Click(Object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int StoreId;
            if (int.TryParse(btn.CommandArgument, out StoreId))
            {
                BuySafeController.RegisterStore(Store.GetStoreList().FirstOrDefault(s => s.StoreID == StoreId));
                SetupStoreLists();
            }
        }

        protected void btnBuySafeSignUp_Click(Object sender, EventArgs e)
        {
            BuySafeRegistrationStatus s = BuySafeController.BuySafeOneClickSignup();
            if (s.Sucessful)
            {
                pnlBuySafeSignUp.Visible = false;
                SetPageState(BuySafeState.EnabledOnFreeTrial);
                SetError("You have successfully registered with BuySafe.", false);
            }
            else if (!string.IsNullOrEmpty(s.ErrorMessage))
            {
                SetError(s.ErrorMessage, true);
            }
            else
            {
                SetPageState(BuySafeState.Error);
            }
        }

        private void SetError(string Message, Boolean isError)
        {
            pnlMessage.Visible = (Message.Length > 0);
            litErrorMessage.Text = Message;
            pnlMessage.CssClass = CommonLogic.IIF(isError, "error", "nonerror");
        }

        
    }

}
