using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
    public partial class JWMyAddresses : SkinBase
    {
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
            RequiresLogin(CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING"));

            if (!Page.IsPostBack)
            {
                LoadAddresses();
            }
        }

        /// <summary>
        /// Load All Customer Addresses
        /// </summary>
        private void LoadAddresses()
        {
            try
            {
                Addresses allAddresses = new Addresses();
                allAddresses.LoadCustomer(ThisCustomer.CustomerID);

                rptAddresses.DataSource = allAddresses;
                rptAddresses.DataBind();
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error); 
            }
        }

        /// <summary>
        /// Add New Address Event
        /// </summary>
        protected void btnAddAddresses_Click(object sender, EventArgs e)
        {
            Response.Redirect("JWAddAddresses.aspx");
        }

        /// <summary>
        /// Repeater ItemCommand Event
        /// </summary>
        protected void rptAddresses_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                int addressID = 0;
                if (Int32.TryParse(e.CommandArgument.ToString(), out addressID))
                {
                    if (e.CommandName == "Delete")
                    {
                        DeleteAddress(addressID);
                    }
                    else if (e.CommandName == "Edit")
                    {
                        EditAddress(addressID);
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error); 
            }
            
        }

        /// <summary>
        /// Delete Customer Address by addressID
        /// </summary>
        /// <param name="addressID">addressID</param>
        private void DeleteAddress(int addressID)
        {
            try
            {
                AspDotNetStorefrontCore.Address anyAddress = new AspDotNetStorefrontCore.Address();
                anyAddress.LoadFromDB(addressID);

                if (ThisCustomer.CustomerID == anyAddress.CustomerID || ThisCustomer.IsAdminSuperUser)
                {
                    AspDotNetStorefrontCore.Address.DeleteFromDB(anyAddress.AddressID, ThisCustomer.CustomerID);
                }

                LoadAddresses();
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error); 
            }
        }

        /// <summary>
        /// Edit Customer Address by addressID
        /// </summary>
        /// <param name="addressID">addressID</param>
        private void EditAddress(int addressID)
        {
            Response.Redirect("JWAddAddresses.aspx?AddressID=" + addressID);
        }

    }
}