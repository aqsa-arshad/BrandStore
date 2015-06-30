// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using AspDotNetStorefrontAdmin;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class editfeed : AdminPageBase
    {
        int FeedID;
        Feed m_Feed;
        private List<Store> m_stores;
        public List<Store> Stores
        {
            get { return m_stores; }
            set { m_stores = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (ViewState["feedid"] != null)
            {
                FeedID = Convert.ToInt32(ViewState["feedid"]);
            }
            else
            {
                FeedID = CommonLogic.QueryStringUSInt("feedid");
            }

            if (FeedID != 0)
            {
                m_Feed = new Feed(FeedID);
            }

            if (!IsPostBack)
            {
                InitializePageData();
            }
            Page.Form.DefaultButton = btnSubmit.UniqueID;
        }

        private void InitializePageData()
        {
            string path = Server.MapPath("") + @"\XmlPackages";
            string searchpattern = "feed*";
            string[] feeds = Directory.GetFiles(path, searchpattern);

            // clear any existing items from the XMLPackage list
            XmlPackage.Items.Clear();

            foreach (string s in feeds)
            {
                XmlPackage.Items.Add(new ListItem(s.Substring(s.LastIndexOf(@"\") + 1).ToLowerInvariant(), s.Substring(s.LastIndexOf(@"\") + 1).ToLowerInvariant()));
            }
            XmlPackage.Items.Insert(0, new ListItem("Select a Package", ""));
            XmlPackage.SelectedIndex = 0;

            Stores = Store.GetStoreList();
            cboStore.Items.Clear();
            cboStore.Items.Add(new ListItem("Select a store", "0"));
            foreach (var store in Stores)
            {
                string storeName = CommonLogic.IIF(store.IsDefault, store.Name + "(Default)", store.Name);
                cboStore.Items.Add(new ListItem(storeName, store.StoreID.ToString()));
            }

            if (m_Feed != null)
            {
                txtFeedName.Text = m_Feed.Name;
                if (XmlPackage.Items.FindByText(m_Feed.XmlPackage.ToLowerInvariant()) != null)
                {
                    XmlPackage.SelectedValue = m_Feed.XmlPackage.ToLowerInvariant();
                }
                txtFtpUserName.Text = m_Feed.FTPUsername;
                txtFtpPwd.Text = m_Feed.FTPPassword;
                txtFtpServer.Text = m_Feed.FTPServer;
                txtFtpPort.Text = m_Feed.FTPPort.ToString();
                txtFtpFileName.Text = m_Feed.FTPFilename;
                CanAutoFtp.Items[0].Selected = m_Feed.CanAutoFTP;
                CanAutoFtp.Items[1].Selected = !m_Feed.CanAutoFTP;
                btnSubmit.Text = "Update Feed";
                ViewState["feedid"] = FeedID.ToString();
                Label1.Text = "Editing Feed:";
                pageheading.Text = m_Feed.Name;
                foreach (ListItem li in cboStore.Items)
                    if (li.Value.EqualsIgnoreCase(m_Feed.StoreID.ToString()))
                        li.Selected = true;

                btnExecFeed.Visible = true;
            }
            else
            {
                Label1.Text = "";
                pageheading.Text = AppLogic.GetString("admin.editfeed.CreateNewFeed", SkinID, LocaleSetting);  
                    
                btnExecFeed.Visible = false;
            }
        }

        protected void resetError(string errorx, bool isError)
        {
            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";

            if (errorx.Length > 0)
                str += errorx + "";
            else
                str = "";

            ltError.Text = str;
        }

        protected void btnSubmit_OnClick(object sender, EventArgs e)
        {
            if (m_Feed != null)
            {
                resetError(UpdateFeed(), false);
                lblMessage.Text = "Feed has been updated.";
            }
            else
            {
                String FTPPort = txtFtpPort.Text.Trim();
                if (FTPPort.Length == 0)
                {
                    FTPPort = "21";
                }

                m_Feed = Feed.CreateFeed(Convert.ToInt32(cboStore.SelectedValue),
                                         txtFeedName.Text,
                                         1,
                                         XmlPackage.SelectedValue,
                                         (CanAutoFtp.SelectedValue == "1"),
                                         txtFtpUserName.Text,
                                         txtFtpPwd.Text,
                                         txtFtpServer.Text,
                                         Convert.ToInt32(FTPPort),
                                         txtFtpFileName.Text,
                                         "");

                FeedID = m_Feed.FeedID;
            }

            InitializePageData();
        }

        protected void btnExecFeed_OnClick(object sender, EventArgs e)
        {
            ExecuteFeed();
        }

        private void ExecuteFeed()
        {
            Server.ScriptTimeout = 120;
            if (txtFtpServer.Text.Trim().Length == 0 && m_Feed.CanAutoFTP)
            {
                resetError("No ftp server specified", true);
            }
            else if (txtFtpFileName.Text.Trim().Length == 0 && m_Feed.CanAutoFTP)
            {
                resetError("No remote filename specified", true);
            }
            else
            {
                Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
                String RuntimeParams = String.Empty;
                RuntimeParams += String.Format("SID={0}&", cboStore.SelectedValue);
                UpdateFeed();
                resetError(m_Feed.ExecuteFeed(ThisCustomer, RuntimeParams), false);
            }
            InitializePageData();
        }


        private string UpdateFeed()
        {
            String FTPPort = txtFtpPort.Text.Trim();
            if (FTPPort.Length == 0)
            {
                FTPPort = "21";
            }

            return m_Feed.UpdateFeed(Convert.ToInt32(cboStore.SelectedValue), txtFeedName.Text, -1, XmlPackage.SelectedValue, Convert.ToSByte(CanAutoFtp.SelectedValue), txtFtpUserName.Text, txtFtpPwd.Text, txtFtpServer.Text, Convert.ToInt32(FTPPort), txtFtpFileName.Text.Trim(), "");
        }

        /// <summary>
        /// Check if user selected an xmlpackage before creating the feed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        protected void ValidateXmlPackage(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (XmlPackage.SelectedIndex > 0);
        }

        /// <summary>
        /// Check if user selected a store before creating the feed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        protected void ValidateStoreID(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (cboStore.SelectedIndex > 0);
        }

        protected void ValidatePort(object source, ServerValidateEventArgs args)
        {
            string port = txtFtpPort.Text.Trim();
            if (port.Length > 0)
            {
                args.IsValid = (CommonLogic.IsInteger(port));
            }
            else
            {
                args.IsValid = true;
            }
        }

        protected void ValidateServer(object source, ServerValidateEventArgs args)
        {
            if (CanAutoFtp.Items[0].Selected && txtFtpServer.Text.Trim() == "")
            {
                args.IsValid = false;
            }
            else
            {
                args.IsValid = true;
            }
        }

    }
}
