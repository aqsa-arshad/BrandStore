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

                MasterHome = "JeldWenTemplate";// "template";
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
                //Change template name to JELD-WEN template by Tayyab on 07-09-2015
                MasterHome = "JeldWenTemplate";// "template.master";
            }

            return MasterHome;
        }

        /// <summary>
        /// Page Load Event
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
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
            Addresses allAddresses = new Addresses();
            allAddresses.LoadCustomer(ThisCustomer.CustomerID);

            rptAddresses.DataSource = allAddresses;
            rptAddresses.DataBind();
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

        /// <summary>
        /// Delete Customer Address by addressID
        /// </summary>
        /// <param name="addressID">addressID</param>
        private void DeleteAddress(int addressID)
        {
            AspDotNetStorefrontCore.Address anyAddress = new AspDotNetStorefrontCore.Address();
            anyAddress.LoadFromDB(addressID);

            if (ThisCustomer.CustomerID == anyAddress.CustomerID || ThisCustomer.IsAdminSuperUser)
            {
                AspDotNetStorefrontCore.Address.DeleteFromDB(anyAddress.AddressID, ThisCustomer.CustomerID);
            }

            LoadAddresses();
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