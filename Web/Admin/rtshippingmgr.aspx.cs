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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using System.IO;
using System.Xml;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    public partial class RTShippingMGR : AdminPageBase
    {
        #region Methods

        #region AttachEventHandlers
        private void AttachEventHandlers()
        {
            grdProviders.RowEditing += grdProviders_RowEditing;
            grdProviders.RowUpdating += grdProviders_RowUpdating;
            grdProviders.RowDeleting += grdProviders_RowDeleting;
            grdProviders.RowCancelingEdit += grdProviders_RowCancelingEdit;
            grdProviders.RowCommand += grdProviders_RowCommand;
        }
        #endregion

        #region BindData
        private void BindData()
        {
            DataSet ds = DB.GetTable("RTShippingProviderView", "Name", string.Empty, false);
            grdProviders.DataSource = ds.Tables[0];
            grdProviders.DataBind();
        }
        #endregion

        #region Scan

        #region ShowInfo
        private void ShowInfo(string msg, bool isError)
        {
            lblInfo.CssClass = isError ? "errorMsg" : "noticeMsg";
            lblInfo.Text = msg;
        }
        #endregion

        #region Scan
        private void Scan()
        {
            // check if some files were deleted and still present on the db
            Dictionary<string, bool> storedProviders = new Dictionary<string, bool>();
            // since XmlSpecificationFile also functions like a primarykey
            // though not defined as one, we'll just use it as lookup

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader reader = DB.GetRS("SELECT XmlSpecificationFile FROM RTShippingProvider", con))
                {
                    while (reader.Read())
                    {
                        // value will be a flag whether it's existing or not, by default
                        // we'll mark it as non-existent until verified by the next routine as existing
                        storedProviders.Add(DB.RSField(reader, "XmlSpecificationFile"), false);
                    }
                }
            }

            DirectoryInfo dir = new DirectoryInfo(CommonLogic.SafeMapPath("~/EntityHelper"));
            FileInfo[] files = dir.GetFiles("rtshipping.provider.*.xml");
            if (files.Length > 0)
            {
                List<string> newFiles = new List<string>();
                foreach (FileInfo file in files)
                {
                    // check if the file is already existing in the database
                    bool nonExistent = !storedProviders.ContainsKey(file.Name); 

                    // extract name from file...
                    string name = file.Name.ToUpperInvariant().Replace("RTSHIPPING.PROVIDER.", string.Empty).Replace(".XML", string.Empty);

                    // a. If non-existent
                    if (nonExistent)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(file.FullName);

                        // extract the values from the UpgradeMigration elements
                        XmlNodeList tobeMigratedList = doc.SelectNodes("descendant::UpgradeMigration/AppConfig");
                        foreach (XmlNode appConfigNode in tobeMigratedList)
                        {
                            // squeeze the value to the Element list..
                            string appConfig = appConfigNode.Attributes["Name"].Value;
                            string moveTo = appConfigNode.Attributes["MoveTo"].Value;

                            // NOTE : The element specified in the MoveTo attribute
                            //  can be present either in one of the Top-Level Nodes
                            //  namely RequiredElements and CarrierSpecificElements
                            //  so we will have to scan on both

                            // First try the RequiredElements
                            XmlNode elementNode = doc.SelectSingleNode("descendant::RequiredElements/" + moveTo);
                            if (elementNode == null)
                            {
                                // it's not present in the RequiredElements
                                // now let's try CarrierSpecificElements
                                elementNode = doc.SelectSingleNode(string.Format("descendant::CarrierSpecificElements/Carrier[@Name='{0}']", moveTo));
                            }

                            // if it's present, extract the value
                            // then dump the App config
                            if (elementNode != null)
                            {
                                bool appConfigPresent = true;

                                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                                {
                                    con.Open();
                                    using (IDataReader reader = DB.GetRS(string.Format("SELECT ConfigValue FROM AppConfig WHERE [Name] = {0}", DB.SQuote(appConfig)), con))
                                    {
                                        appConfigPresent = reader.Read();
                                        if (appConfigPresent)
                                        {
                                            elementNode.Attributes["Default"].Value = DB.RSField(reader, "ConfigValue");
                                        }
                                    }
                                }

                                // Nuke the AppConfig
                                if (appConfigPresent)
                                {
                                    DB.ExecuteSQL(string.Format("DELETE AppConfig WHERE [Name] = {0}", DB.SQuote(appConfig)));
                                }
                            }
                        }


                        DB.ExecuteSQL(
                            string.Format(
                                "INSERT INTO RTShippingProvider(RTShippingProviderGUID, [Name],Enabled, UseLiveMode,XmlSpecificationFile,XmlSpecificationUserData,CreatedOn) " +
                                "VALUES(NEWID(), {0}, 1, 1, {1}, {2}, GETDATE())",
                                DB.SQuote(name),
                                DB.SQuote(file.Name),
                                DB.SQuote(doc.OuterXml)
                            )
                        );
                    }

                    // b. If file is still present, Mark it
                    storedProviders[file.Name] = true;
                }
            }

            // now check if the record is still present
            bool hasProviderCleared = false;
            foreach (string providerSpecFile in storedProviders.Keys)
            {
                if (storedProviders[providerSpecFile] == false)
                {
                    // delete the records...
                    DB.ExecuteSQL(
                        string.Format(
                            "DELETE RTShippingProviderToCountryMap rtm INNER JOIN RTShippingProvider rt ON rt.RTShippingProviderID = rtm.RTShippingProviderID WHERE rt.XmlSpecificationFile = {0}",
                            DB.SQuote(providerSpecFile)
                        )
                    );

                    DB.ExecuteSQL(
                        string.Format(
                            "DELETE RTShippingProvider WHERE XmlSpecificationFile = {0}", 
                            DB.SQuote(providerSpecFile)
                        )
                    );

                    hasProviderCleared = true;
                }
            }

            if (hasProviderCleared)
            {
                ShowInfo("Some providers where deleted because the file was removed", false);
            }
        }
        #endregion

        #endregion

        #endregion

        #region Events

        #region Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            litSectionTitle.Text = "<b>Now In : </b> <a href=\"" + AppLogic.AdminLinkUrl("shipping.aspx") + "\">Shipping Method</a> - Manage Real-Time Shipping Providers";

            AttachEventHandlers();

            if (!Page.IsPostBack)
            {
                Scan();
                BindData();
            }
        }
        #endregion

        #region grdProviders_RowUpdating
        protected void grdProviders_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = grdProviders.Rows[e.RowIndex];
            if (row != null)
            {
                CheckBox chkEnabled = row.FindControl("chkEnabled") as CheckBox;
                CheckBox chkUseLiveMode = row.FindControl("chkUseLiveMode") as CheckBox;
                ITextControl colName = row.FindControl("colName") as ITextControl;
                Label lblId = row.FindControl("lblID") as Label;

                DB.ExecuteSQL(
                    string.Format(
                        "UPDATE RTShippingProvider SET [Name] = {0}, Enabled = {1}, UseLiveMode =  {2} WHERE RtShippingProviderID = {3}",
                        DB.SQuote(colName.Text),
                        Convert.ToInt16(chkEnabled.Checked),
                        Convert.ToInt16(chkUseLiveMode.Checked),
                        lblId.Text
                    )
                );

                grdProviders.EditIndex = -1;
                BindData();
            }
        }
        #endregion

        #region grdProviders_RowCancelingEdit
        protected void grdProviders_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            grdProviders.EditIndex = -1;
            BindData();
        }
        #endregion

        #region grdProviders_RowEditing
        protected void grdProviders_RowEditing(object sender, GridViewEditEventArgs e)
        {
            grdProviders.EditIndex = e.NewEditIndex;
            BindData();
        }
        #endregion

        #region grdProviders_RowCommand        
        protected void grdProviders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName.ToLowerInvariant())
            {
                case "map":
                    Response.Redirect(AppLogic.AdminLinkUrl("rtshippingcountries.aspx") +"?RtShippingProviderID=" + e.CommandArgument);
                    break;
                case "userdata":
                    Response.Redirect(AppLogic.AdminLinkUrl("rtshippingproviderspecuserdata.aspx") +"?specfile=" + e.CommandArgument);
                    break;
            }
        }
        #endregion                

        #region grdProviders_RowDeleting
        protected void grdProviders_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            GridViewRow row = grdProviders.Rows[e.RowIndex];
            if (row != null)
            {
                Label lblId = row.FindControl("lblID") as Label;
                DB.ExecuteSQL(string.Format("DELETE RTShippingProvider WHERE RtShippingProviderID = {0}", lblId.Text));
                DB.ExecuteSQL(string.Format("DELETE RTShippingProviderToCountryMap WHERE RtShippingProviderID = {0}", lblId.Text));
            }

            BindData();
        }
        #endregion

        #region btnScan_Click
        protected void btnScan_Click(object sender, EventArgs e)
        {
            Scan();
            BindData();
        }
        #endregion

        #endregion
    }

}
