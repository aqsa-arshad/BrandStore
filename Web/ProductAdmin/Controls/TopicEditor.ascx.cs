// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontAdmin.Controls;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    public partial class TopicEditor : System.Web.UI.UserControl
    {
        #region Public Properties
        public int EditingTopicId
        {
            get
            {
                object intValue = ViewState["EditingTopicId"];
                if (null == intValue) { return 0; }

                return (int)intValue;
            }
            set
            {
                ViewState["EditingTopicId"] = value;
            }
        }
        public string LocaleSetting
        {
            get
            {
                object intValue = ViewState["LocaleSetting"];
                if (null == intValue) { return Localization.GetDefaultLocale(); }

                return (String)intValue;
            }
            set
            {
                ViewState["LocaleSetting"] = value;
                LoadTopic(this.EditingTopicId);
            }
        }
        public Boolean Editing
        {
            get
            {
                return EditingTopicId > 0;
            }
        }
        private Boolean _showSkinIDField = false;
        public Boolean ShowSkinIDField
        {
            get { return _showSkinIDField; }
            set { _showSkinIDField = value; }
        }
        #endregion

        #region Private Properties
        protected Customer ThisCustomer;
        protected bool bUseHtmlEditor;
        protected String TopicName
        {
            get
            {
                return ltName.Text.Trim();
            }
            set
            {
                ltName.Text = value.Trim();
            }
        }
        #endregion

        #region Events
        public event EventHandler<TopicEditEventArgs> TopicCopiedToStore;
        public event EventHandler<TopicEditEventArgs> TopicAdded;
        public event EventHandler<TopicEditEventArgs> TopicSaved;
        public event EventHandler<TopicEditEventArgs> TopicDeleted;
        public event EventHandler<TopicEditEventArgs> TopicNuked;
        #endregion

        #region Public Methods
        public void LoadTopic() { LoadTopic(this.EditingTopicId); }
        public void LoadTopic(int topicId)
        {
            pnlTopicEditor.Visible = true;
            this.EditingTopicId = topicId;
            TopicMapping tm = pnlMapTopic.FindControl<TopicMapping>("ctrlMapTopic");
            tm.ThisTopicID = EditingTopicId.ToString();
            tm.BindData();

            trMapTopics.Visible = Editing;
            btnDelete.Visible = btnNuke.Visible = Editing;

            Literal28.Visible = !Editing && Store.StoreCount > 1;
            ssOne.Visible = !Editing;
            trCopyToStore.Visible = Editing && Store.StoreCount > 1 && AppLogic.GlobalConfigBool("AllowTopicFiltering");

            if (!Editing)
                BindNewTopic();
            else
                BindTopic();
        }
        #endregion

        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            pnlMapTopic.Style["display"] = "none";
            base.OnInit(e);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            trSkinId.Visible = this.ShowSkinIDField;
            ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            bUseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
            resetError("", false);

            radDescription.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);
        }
        protected void btnCopyToStore_Click(object sender, EventArgs e)
        {
            Boolean success = false;
            int sId = int.Parse(ddCopyToStore.SelectedValue);
            if (sId > 0 && EditingTopicId > 0)
            {
                Topic t = new Topic(EditingTopicId);
                Topic duplicate = t.CopyForStore(sId);
                success = duplicate != null;
                loadStoreDuplication(t.TopicName);
                UnloadTopic();
                if (success && TopicCopiedToStore != null)
                    TopicCopiedToStore(this, new TopicEditEventArgs(duplicate.TopicID, false));
                
            }
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int StoreID = 0;
            bool bTopicNameExist = IsTopicNameExist(out StoreID);
            if (bTopicNameExist && StoreID == 0)
            {
                resetError(AppLogic.GetString("admin.topic.nameexists", LocaleSetting), true);
                return;
            }
            else if (bTopicNameExist)
            {
                resetError(" A topic with this name already exists in Store ID " + StoreID.ToString(), true);
                return;
            }

            if (validateForm())
            {
                if (Editing)
                    UpdateTopic();
                else
                    SaveNewTopic();
            }
        }
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            ltValid.Text = "";
            int deletedtopicid = EditingTopicId;
            DB.ExecuteSQL(string.Format("update Topic set deleted=1 where TopicID={0}", EditingTopicId));
            resetError(AppLogic.GetString("admin.topic.deleted", LocaleSetting), false);
            pnlTopicEditor.Visible = false;
            //todo:hide topic editor

            UnloadTopic();
            if (TopicDeleted != null)
                TopicDeleted(this, new TopicEditEventArgs(deletedtopicid, false));
        }
        protected void btnNuke_Click(object sender, EventArgs e)
        {
            ltValid.Text = "";
            int deletedtopicid = EditingTopicId;
            DB.ExecuteSQL(string.Format("delete from Topic where TopicID={0}", EditingTopicId));
            resetError(AppLogic.GetString("admin.topic.deleted", LocaleSetting), false);
            pnlTopicEditor.Visible = false;
            //todo:hide topic editor

            UnloadTopic();
            if (TopicNuked != null)
                TopicNuked(this, new TopicEditEventArgs(deletedtopicid, false));
        }
        #endregion

        #region Private Methods

        protected bool IsTopicNameExist(out int RougeStoreID)
        {
            RougeStoreID = 0;
            if (Editing)
                return false;

            bool bTopicExist = false;
            string sTopicName = TopicName;
            string gcSql;
            int CurrentStore = ssOne.SelectedStoreID;
            if (Store.StoreCount == 0)
                CurrentStore = AppLogic.StoreID();


            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                gcSql = "SELECT * FROM Topic WITH (NOLOCK) WHERE Deleted=0 AND StoreID=" + CurrentStore + " AND (Name like '%>' + " + DB.SQuote(sTopicName) + " + '<%' OR Name=" + DB.SQuote(sTopicName) + ");";
                using (IDataReader rs = DB.GetRS(gcSql, con))
                {
                    if (rs.Read())
                    {
                        RougeStoreID = rs.FieldInt("StoreID");
                        bTopicExist = true;
                    }
                }

            }
            return bTopicExist;
        }
        private void BindTopic()
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(string.Format("select * from Topic with (NOLOCK) where TopicID={0}", EditingTopicId), con))
                {
                    if (!rs.Read())
                    {
                        rs.Close();
                        resetError(AppLogic.GetString("admin.common.UnableToRetrieveData", LocaleSetting), true);
                        return;
                    }

                    //editing Topic
                    ltMode.Text = AppLogic.GetString("admin.topic.editing", this.LocaleSetting);
                    btnSubmit.Text = AppLogic.GetString("admin.topic.updatebutton", this.LocaleSetting);
                    TopicName = DB.RSFieldByLocale(rs, "Name", this.LocaleSetting);
                    ltTitle.Text = DB.RSFieldByLocale(rs, "Title", this.LocaleSetting);
                    if (bUseHtmlEditor)
                    {
                        radDescription.Content = DB.RSFieldByLocale(rs, "Description", this.LocaleSetting);
                    }
                    else
                    {
                        ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                        ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\">" + XmlCommon.GetLocaleEntry(HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Description")), this.LocaleSetting, false) + "</textarea>\n");
                        ltDescription.Text += ("</div>");
                        radDescription.Visible = false;
                    }
                    ltSETitle.Text = DB.RSFieldByLocale(rs, "SETitle", this.LocaleSetting);
                    ltSEKeywords.Text = DB.RSFieldByLocale(rs, "SEKeywords", this.LocaleSetting);
                    ltSEDescription.Text = DB.RSFieldByLocale(rs, "SEDescription", this.LocaleSetting);
                    txtContentsBG.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "ContentsBGColor"), "");
                    txtPageBG.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "PageBGColor"), "");
                    txtPassword.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "Password"), "");
                    txtSkin.Text = CommonLogic.IIF(DB.RSFieldInt(rs, "SkinID") == 0, "", DB.RSFieldInt(rs, "SkinID").ToString());
                    txtDspOrdr.Text = DB.RSFieldInt(rs, "DisplayOrder").ToString();
                    txtSkinColor.Text = CommonLogic.IIF(Editing, DB.RSField(rs, "GraphicsColor"), "");
                    rbDisclaimer.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "RequiresDisclaimer"), 1, 0);
                    rbHTML.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "HTMLOk"), 1, 0);
                    rbPublish.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "ShowInSiteMap"), 1, 0);
                    rbSubscription.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "RequiresSubscription"), 1, 0);
                    loadStoreDuplication(DB.RSFieldByLocale(rs, "Name", this.LocaleSetting));
                    chkPublished.Checked = CommonLogic.IIF(DB.RSFieldBool(rs, "Published"), true, false);
                    chkIsFrequent.Checked = CommonLogic.IIF(DB.RSFieldBool(rs, "IsFrequent"), true, false);
                }
            }
        }
        private void BindNewTopic()
        {
            //editing Topic
            ltMode.Text = AppLogic.GetString("admin.topic.adding", this.LocaleSetting);
            btnSubmit.Text = AppLogic.GetString("admin.topic.addbutton", this.LocaleSetting);
            TopicName = string.Empty;
            ltTitle.Text = string.Empty;
            if (bUseHtmlEditor)
            {
                radDescription.Content = string.Empty;
            }
            else
            {
                ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\"></textarea>\n");
                ltDescription.Text += ("</div>");
                radDescription.Visible = false;
            }
            ltSEKeywords.Text =
                ltSETitle.Text =
                ltSEDescription.Text =
                txtContentsBG.Text =
                txtPageBG.Text =
                txtPassword.Text =
                txtSkin.Text =
                txtDspOrdr.Text =
                txtSkinColor.Text = string.Empty;
            rbDisclaimer.SelectedIndex =
            rbHTML.SelectedIndex =
            rbPublish.SelectedIndex =
            rbSubscription.SelectedIndex = 0;
            chkIsFrequent.Checked = true;
        }
        private void loadStoreDuplication(string TopicName)
        {
            trCopyToStore.Visible = false;
            if (AppLogic.GlobalConfigBool("AllowTopicFiltering") == true)
            {
                trCopyToStore.Visible = true;
                int defaultTopicId = Topic.GetTopicID(TopicName, 0);
                ddCopyToStore.Items.Clear();
                foreach (Store s in Store.GetStoreList())
                {
                    int thisid = Topic.GetTopicID(TopicName, s.StoreID);
                    if (thisid == 0 || thisid == defaultTopicId)
                        ddCopyToStore.Items.Add(new ListItem(s.Name, s.StoreID.ToString()));
                }
                if (ddCopyToStore.Items.Count == 0)
                    trCopyToStore.Visible = false;
            }
        }
        protected void resetError(string error, bool isError)
        {
            string str = AppLogic.GetString("admin.topic.notice", LocaleSetting);
            if (isError)
                str = "<span style=\"color:red;font-weight:bold\">" + AppLogic.GetString("admin.common.Error", LocaleSetting);
            if (error.Length > 0)
            {
                str += error + "";
            }
            else
            {
                str = "";
            }
            if (isError)
                str += "</span>";
            ltError.Text = str;
        }
        protected bool validateForm()
        {
            bool valid = true;
            string temp = "";
            ltValid.Text = "";

            if ((string.IsNullOrEmpty(TopicName) || (AppLogic.FormLocaleXml(TopicName, LocaleSetting).Equals("<ml></ml>"))))
            {
                valid = false;
                temp += AppLogic.GetString("admin.common.FillOutNamePrompt", LocaleSetting);
            }
            else if ((string.IsNullOrEmpty(ltTitle.Text) || (AppLogic.FormLocaleXml(ltTitle.Text, LocaleSetting).Equals("<ml></ml>"))))
            {
                valid = false;
                temp += AppLogic.GetString("admin.common.TopicPageTitlePrompt", LocaleSetting);

            }
            if (!valid)
            {
                TopicName = XmlCommon.GetLocaleEntry(AppLogic.FormLocaleXml(TopicName, LocaleSetting), LocaleSetting, true);
                ltTitle.Text = XmlCommon.GetLocaleEntry(AppLogic.FormLocaleXml(ltTitle.Text, LocaleSetting), LocaleSetting, true);
                ltValid.Text = string.Format("<script type=\"text/javascript\">alert('{0}');</script>", temp);
            }
            return valid;
        }
        protected void SaveNewTopic()
        {
            int CurrentStore = ssOne.SelectedStoreID;
            StringBuilder sql = new StringBuilder(2500);
            String NewGUID = DB.GetNewGUID();
            sql.Append("insert into Topic(TopicGUID,Name,SkinID,DisplayOrder,ContentsBGColor,PageBGColor,GraphicsColor,Published,Title,IsFrequent,Description,Password,RequiresSubscription,HTMLOk,RequiresDisclaimer,ShowInSiteMap,SEKeywords,SEDescription,SETitle,StoreID) values(");
            sql.Append(DB.SQuote(NewGUID) + ",");
            sql.Append(DB.SQuote(AppLogic.FormLocaleXml(TopicName, LocaleSetting)) + ",");
            sql.Append(Localization.ParseUSInt(txtSkin.Text) + ",");
            sql.Append(Localization.ParseUSInt(txtDspOrdr.Text) + ",");
            sql.Append(DB.SQuote(txtContentsBG.Text) + ",");
            sql.Append(DB.SQuote(txtPageBG.Text) + ",");
            sql.Append(DB.SQuote(txtSkinColor.Text) + ",");
            sql.Append(DB.SQuote(CommonLogic.IIF(chkPublished.Checked, 1, 0).ToString()) + ",");
            sql.Append(DB.SQuote(AppLogic.FormLocaleXml(ltTitle.Text, LocaleSetting)) + ",");
            sql.Append(DB.SQuote(CommonLogic.IIF(chkIsFrequent.Checked, 1, 0).ToString()) + ",");
            String desc = String.Empty;
            if (bUseHtmlEditor)
            {
                desc = AppLogic.FormLocaleXml(radDescription.Content, LocaleSetting);
            }
            else
            {
                desc = AppLogic.FormLocaleXml(CommonLogic.FormCanBeDangerousContent("Description"), LocaleSetting);
            }
            if (desc.Length != 0)
            {
                sql.Append(DB.SQuote(desc) + ",");
            }
            else
            {
                sql.Append("NULL,");
            }
            if (txtPassword.Text.Trim().Length != 0)
            {
                sql.Append(DB.SQuote(txtPassword.Text) + ",");
            }
            else
            {
                sql.Append("NULL,");
            }
            sql.Append(rbSubscription.SelectedValue.ToString() + ",");
            sql.Append(rbHTML.SelectedValue.ToString() + ",");
            sql.Append(rbDisclaimer.SelectedValue.ToString() + ",");
            sql.Append(rbPublish.SelectedValue.ToString() + ",");
            if (AppLogic.FormLocaleXml(ltSEKeywords.Text, LocaleSetting).Length != 0)
            {
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml(ltSEKeywords.Text, LocaleSetting)) + ",");
            }
            else
            {
                sql.Append("NULL,");
            }
            if (AppLogic.FormLocaleXml(ltSEDescription.Text, LocaleSetting).Length != 0)
            {
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml(ltSEDescription.Text, LocaleSetting)) + ",");
            }
            else
            {
                sql.Append("NULL,");
            }
            if (AppLogic.FormLocaleXml(ltSETitle.Text, LocaleSetting).Length != 0)
            {
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml(ltSETitle.Text, LocaleSetting)) + ",");
            }
            else
            {
                sql.Append("NULL" + ",");
            }

            if (CurrentStore == 0 || Store.StoreCount == 1)
            {
                sql.Append("0");
            }
            else
            {
                sql.Append(CurrentStore.ToString());
            }

            sql.Append(")");
            try
            {
                DB.ExecuteSQL(sql.ToString());
                resetError("Topic added.", false);
            }
            catch (Exception ex)
            {
                resetError(ex.Message, true);
                return;
            }
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(string.Format("select TopicID from Topic with (NOLOCK) where deleted=0 and TopicGUID={0}", DB.SQuote(NewGUID)), con))
                {
                    rs.Read();
                    UnloadTopic();
                    if (TopicAdded != null)
                        TopicAdded(this, new TopicEditEventArgs(DB.RSFieldInt(rs, "TopicID"), false));
                }
            }
        }
        protected void UpdateTopic()
        {
            Topic originalTopic = new Topic(EditingTopicId);
            StringBuilder sql = new StringBuilder(2500);
            int StoreID = 0;
            bool bTopicNameExist = IsTopicNameExist(out StoreID);
            int ExistingTopicId = Topic.GetTopicID(TopicName, LocaleSetting, originalTopic.StoreID);
            if (TopicName != originalTopic.TopicName && ExistingTopicId != originalTopic.TopicID && ExistingTopicId > 0)
            {
                resetError("The topic name entered already exists. Please choose a unique topic name.", true);
                return;
            }
            sql.Append("update Topic set ");
            sql.Append("Published=" + (CommonLogic.IIF(chkPublished.Checked, 1, 0)).ToString() + ",");
            sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name", TopicName, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId))) + ",");
            sql.Append("SkinID=" + Localization.ParseUSInt(txtSkin.Text) + ",");
            sql.Append("DisplayOrder=" + Localization.ParseUSInt(txtDspOrdr.Text) + ",");
            sql.Append("ContentsBGColor=" + DB.SQuote(txtContentsBG.Text) + ",");
            sql.Append("PageBGColor=" + DB.SQuote(txtPageBG.Text) + ",");
            sql.Append("GraphicsColor=" + DB.SQuote(txtSkinColor.Text) + ",");
            sql.Append("Title=" + DB.SQuote(AppLogic.FormLocaleXml("Title", ltTitle.Text, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId))) + ",");
            sql.Append("IsFrequent=" + (CommonLogic.IIF(chkIsFrequent.Checked, 1, 0)).ToString() + ",");

            String desc = String.Empty;
            if (bUseHtmlEditor)
            {
                desc = AppLogic.FormLocaleXml("Description", radDescription.Content, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId));
            }
            else
            {
                desc = AppLogic.FormLocaleXmlEditor("Description", "Description", LocaleSetting, "topic", Convert.ToInt32(EditingTopicId));
            }
            if (desc.Length != 0)
            {
                sql.Append("Description=" + DB.SQuote(desc) + ",");
            }
            else
            {
                sql.Append("Description=NULL,");
            }
            if (txtPassword.Text.Trim().Length != 0)
            {
                sql.Append("Password=" + DB.SQuote(txtPassword.Text) + ",");
            }
            else
            {
                sql.Append("Password=NULL,");
            }
            sql.Append("RequiresSubscription=" + rbSubscription.SelectedValue.ToString() + ",");
            sql.Append("HTMLOk=" + rbHTML.SelectedValue.ToString() + ",");
            sql.Append("RequiresDisclaimer=" + rbDisclaimer.SelectedValue.ToString() + ",");
            sql.Append("ShowInSiteMap=" + rbPublish.SelectedValue.ToString() + ",");
            if (AppLogic.FormLocaleXml("SEKeywords", ltSEKeywords.Text, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId)).Length != 0)
            {
                sql.Append("SEKeywords=" + DB.SQuote(AppLogic.FormLocaleXml("SEKeywords", ltSEKeywords.Text, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId))) + ",");
            }
            else
            {
                sql.Append("SEKeywords=NULL,");
            }
            if (AppLogic.FormLocaleXml("SEDescription", ltSEDescription.Text, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId)).Length != 0)
            {
                sql.Append("SEDescription=" + DB.SQuote(AppLogic.FormLocaleXml("SEDescription", ltSEDescription.Text, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId))) + ",");
            }
            else
            {
                sql.Append("SEDescription=NULL,");
            }
            if (AppLogic.FormLocaleXml("SETitle", ltSETitle.Text, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId)).Length != 0)
            {
                sql.Append("SETitle=" + DB.SQuote(AppLogic.FormLocaleXml("SETitle", ltSETitle.Text, LocaleSetting, "topic", Convert.ToInt32(EditingTopicId))));
            }
            else
            {
                sql.Append("SETitle=NULL");
            }
            sql.Append(" where TopicID=" + EditingTopicId.ToString());

            DB.ExecuteSQL(sql.ToString());
            resetError("Topic updated.", false);

            int EditedTopic = EditingTopicId;

            UnloadTopic();
            if (TopicSaved != null)
                TopicSaved(this, new TopicEditEventArgs(EditedTopic, originalTopic.TopicName != TopicName));
        }
        protected void UnloadTopic()
        {
            this.EditingTopicId = 0;
            this.LoadTopic();
            pnlTopicEditor.Visible = false;
        }
        #endregion
    }
}
