// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Text;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
namespace AspDotNetStorefrontAdmin
{
    public partial class editpolls : AdminPageBase
    {
        int PollID;
        String PollCategories;
        String PollSections;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            PollID = 0;

            if (CommonLogic.QueryStringCanBeDangerousContent("PollID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("PollID") != "0")
            {
                Editing = true;
                PollID = Localization.ParseUSInt(CommonLogic.QueryStringCanBeDangerousContent("PollID"));
            }
            else
            {
                Editing = false;
            }

            PollCategories = AppLogic.GetPollCategories(PollID);
            PollSections = AppLogic.GetPollSections(PollID);
            loadScript();
            if (!IsPostBack)
            {
                InitializePageContent();
            }
            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("polls.aspx") + "\">" + AppLogic.GetString("admin.editpolls.Polls", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.menu.Polls", SkinID, LocaleSetting) + "";
        }

        private void InitializePageContent()
        {
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from Poll   with (NOLOCK)  where PollID=" + PollID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    if (ErrorMsg.Length != 0)
                    {
                        resetError(ErrorMsg, true);
                    }
                    else
                    {
                        if (Editing)
                        {
                            ltInfo.Text = "<p align=\"left\"><b>" + String.Format(AppLogic.GetString("admin.editpolls.EditingPoll", SkinID, LocaleSetting), DB.RSFieldByLocale(rs, "Name", LocaleSetting), DB.RSField(rs, "SKU"), DB.RSFieldInt(rs, "PollID").ToString()) + "&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"" + AppLogic.AdminLinkUrl("pollanswers.aspx") + "?Pollid=" + PollID.ToString() + "\">" + AppLogic.GetString("admin.editpolls.AddEditPollAnswer", SkinID, LocaleSetting) + "</a>" +
                                          "&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"" + AppLogic.AdminLinkUrl("managepoll.aspx") + "?Pollid=" + PollID.ToString() + "\">Review Votes</a></b>&nbsp;&nbsp;&nbsp;&nbsp;</p>";
                            etsMapper.ObjectID = Localization.ParseNativeInt(PollID.ToString());
                            etsMapper.DataBind();
                            litStoreMapper.Visible = etsMapper.StoreCount > 1;
                            litStoreMapperHdr.Visible = etsMapper.StoreCount > 1;
                            btnSubmit.Text = AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting);
                            btnSubmit1.Text = AppLogic.GetString("admin.common.Update", SkinID, LocaleSetting);
                            btnReset.Visible = true;
                            btnReset1.Visible = true;
                        }
                        else
                        {
                            ltInfo.Text = "<div style=\"height:17;padding-top:3px;\" class=\"tablenormal\">" + AppLogic.GetString("admin.editpolls.AddingNewPoll", SkinID, LocaleSetting) + ":</div>\n";
                            etsMapper.ObjectID = 0;
                            etsMapper.DataBind();
                            btnSubmit.Text = AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting);
                            btnSubmit1.Text = AppLogic.GetString("admin.common.AddNew", SkinID, LocaleSetting);
                            btnReset.Visible = false;
                            btnReset1.Visible = false;
                        }
                        ltName.Text = AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, AppLogic.GetString("admin.editpolls.EnterPollName", SkinID, LocaleSetting), 100, 50, 0, 0, false);
						txtDate.SelectedDate = Editing ? DB.RSFieldDateTime(rs, "ExpiresOn") : System.DateTime.Now.AddMonths(1);
                        if (Editing)
                        {
                            rbPublished.Items.FindByValue(rs["Published"].ToString()).Selected = true;
                        }
                        else
                        {
                            rbPublished.SelectedValue = "1";
                        }
                        if (Editing)
                        {
                            rbAnon.Items.FindByValue(rs["AnonsCanVote"].ToString()).Selected = true;
                        }
                        else
                        {
                            rbAnon.SelectedValue = "0";
                        }
                        ddlSortOrder.Items.Clear();
                        using (SqlConnection dbconn2 = DB.dbConn())
                        {
                            dbconn2.Open();
                            using (IDataReader rsst = DB.GetRS("select * from PollSortOrder  with (NOLOCK)  order by DisplayOrder,Name", dbconn2))
                            {
                                while (rsst.Read())
                                {
                                    ddlSortOrder.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", LocaleSetting), DB.RSFieldInt(rsst, "PollSortOrderID").ToString()));
                                }
                            }
                        }
                        ddlSortOrder.SelectedValue = DB.RSFieldTinyInt(rs, "PollSortOrderID").ToString();
                    }
                }
            }
            ltCategoryList.Text = GetCategoryList(PollID, PollCategories, 0, 1, LocaleSetting, EntityHelpers);
            ltSectionList.Text = GetSectionList(PollID, PollSections, 0, 1, LocaleSetting, EntityHelpers);
        }

        protected void loadScript()
        {
            StringBuilder tmpS = new StringBuilder();

            tmpS.Append("<script type=\"text/javascript\">\n");
            tmpS.Append("function Form_Validator(theForm)\n");
            tmpS.Append("{\n");
            tmpS.Append("submitonce(theForm);\n");
            tmpS.Append("if (theForm.PollSortOrderID.selectedIndex < 1)\n");
            tmpS.Append("{\n");
            tmpS.Append("alert(\"" + AppLogic.GetString("admin.editpolls.SelectPollSortOrder", SkinID, LocaleSetting) + "\");\n");
            tmpS.Append("theForm.PollSortOrderID.focus();\n");
            tmpS.Append("submitenabled(theForm);\n");
            tmpS.Append("return (false);\n");
            tmpS.Append("    }\n");
            tmpS.Append("return (true);\n");
            tmpS.Append("}\n");
            tmpS.Append("</script>\n");
            if (AppLogic.NumLocaleSettingsInstalled() > 1)
            {
                tmpS.Append("<script type='text/javascript' src='Scripts/tabs.js'></script>");
            }

            ltScript.Text = tmpS.ToString();
        }

        protected bool validateInput()
        {
            string temp;
            string frmName = AppLogic.FormLocaleXml("Name");
            if (frmName.Equals("<ml></ml>") || string.IsNullOrEmpty(frmName))
            {
                temp = AppLogic.GetString("admin.editpolls.EnterPollName", SkinID, LocaleSetting) + " <script type=\"text/javascript\">alert('" + AppLogic.GetString("admin.editpolls.EnterPollName", SkinID, LocaleSetting) + "');</script>";
                resetError(temp, true);
                return false;
            }
            return true;
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
            {
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            }

            if (error.Length > 0)
            {
                str += error + "";
                plnError.Visible = true;
            }
            else
            {
                str = "";
                plnError.Visible = false;
            }

            ltError.Text = str;
        }

        static public String GetCategoryList(int PollID, String PollCategories, int ForParentCategoryID, int level, string LocaleSetting, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            String sql = String.Empty;
            EntityHelper CategoryHelper = AppLogic.LookupHelper(EntityHelpers, "Category");
            if (ForParentCategoryID == 0)
            {
                sql = "select * from category   with (NOLOCK)  where (parentcategoryid=0 or ParentCategoryID IS NULL) and published=1 and deleted=0 order by DisplayOrder,Name";
            }
            else
            {
                sql = "select * from category   with (NOLOCK)  where parentcategoryid=" + ForParentCategoryID.ToString() + " and published=1 and deleted=0 order by DisplayOrder,Name";
            }

            String Indent = String.Empty;
            for (int i = 1; i < level; i++)
            {
                Indent += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            }

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        bool PollIsMappedToThisCategory = (("," + PollCategories + ",").IndexOf("," + DB.RSFieldInt(rs, "CategoryID").ToString() + ",") != -1);
                        tmpS.Append("<input type=\"checkbox\" name=\"CategoryMap\" value=\"" + DB.RSFieldInt(rs, "CategoryID").ToString() + "\" " + CommonLogic.IIF(PollIsMappedToThisCategory, " checked ", "") + ">" + CommonLogic.IIF(level == 1, "<b>", "") + Indent + DB.RSFieldByLocale(rs, "name", LocaleSetting) + CommonLogic.IIF(level == 1, "</b>", "") + "<br/>\n");
                        if (CategoryHelper.EntityHasSubs(DB.RSFieldInt(rs, "CategoryID")))
                        {
                            tmpS.Append(GetCategoryList(PollID, PollCategories, DB.RSFieldInt(rs, "CategoryID"), level + 1, LocaleSetting, EntityHelpers));
                        }
                    }
                }
            }
            return tmpS.ToString();
        }

        static public String GetSectionList(int PollID, String PollSections, int ForParentSectionID, int level, string LocaleSetting, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers)
        {
            EntityHelper SectionHelper = AppLogic.LookupHelper(EntityHelpers, "Section");
            StringBuilder tmpS = new StringBuilder(4096);
            String sql = String.Empty;
            if (ForParentSectionID == 0)
            {
                sql = "select * from [Section]  with (NOLOCK)  where (ParentSectionID=0 or ParentSectionID IS NULL) and Published=1 and Deleted=0 order by DisplayOrder,Name";
            }
            else
            {
                sql = "select * from [Section]  with (NOLOCK)  where ParentSectionID=" + ForParentSectionID.ToString() + " and Published=1 and Deleted=0 order by DisplayOrder,Name";
            }

            String Indent = String.Empty;
            for (int i = 1; i < level; i++)
            {
                Indent += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            }

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        bool PollIsMappedToThisSection = (("," + PollSections + ",").IndexOf("," + DB.RSFieldInt(rs, "SectionID").ToString() + ",") != -1);
                        tmpS.Append("<input type=\"checkbox\" name=\"SectionMap\" value=\"" + DB.RSFieldInt(rs, "SectionID").ToString() + "\" " + CommonLogic.IIF(PollIsMappedToThisSection, " checked ", "") + ">" + CommonLogic.IIF(level == 1, "<b>", "") + Indent + DB.RSFieldByLocale(rs, "name", LocaleSetting) + CommonLogic.IIF(level == 1, "</b>", "") + "<br/>\n");
                        if (SectionHelper.EntityHasSubs(DB.RSFieldInt(rs, "SectionID")))
                        {
                            tmpS.Append(GetSectionList(PollID, PollSections, DB.RSFieldInt(rs, "SectionID"), level + 1, LocaleSetting, EntityHelpers));
                        }
                    }
                }
            }
            return tmpS.ToString();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!validateInput())
            {
                return;
            }
            StringBuilder sql = new StringBuilder(2500);
			DateTime dt = txtDate.SelectedDate ?? System.DateTime.Now.AddMonths(1);

            if (!Editing)
            {
                String NewGUID = DB.GetNewGUID();
                sql.Append("insert into Poll(PollGUID,Name,PollSortOrderID,Published,AnonsCanVote,ExpiresOn) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                sql.Append(ddlSortOrder.SelectedValue + ",");
                sql.Append(rbPublished.SelectedValue + ",");
                sql.Append(rbAnon.SelectedValue + ",");
                sql.Append(DB.DateQuote(Localization.ToDBDateTimeString(dt)));
                sql.Append(")");
                DB.ExecuteSQL(sql.ToString());
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select PollID from Poll with (NOLOCK) where deleted=0 and PollGUID=" + DB.SQuote(NewGUID), dbconn))
                    {
                        rs.Read();
                        PollID = DB.RSFieldInt(rs, "PollID");
                    }
                }

                etsMapper.ObjectID = Localization.ParseNativeInt(PollID.ToString());
                etsMapper.Save();

                Editing = true;
                InitializePageContent();
            }
            else
            {
                sql.Append("update Poll set ");
                sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name")) + ",");
                sql.Append("PollSortOrderID=" + ddlSortOrder.SelectedValue + ",");
                sql.Append("Published=" + rbPublished.SelectedValue + ",");
                sql.Append("AnonsCanVote=" + rbAnon.SelectedValue + ",");
                sql.Append("ExpiresOn=" + DB.DateQuote(Localization.ToDBDateTimeString(dt)));
                sql.Append(" where PollID=" + PollID.ToString());
                DB.ExecuteSQL(sql.ToString());
            }

            // Update Category Mappings
            DB.ExecuteSQL("delete from Pollcategory where Pollid=" + PollID.ToString());
            String CMap = CommonLogic.FormCanBeDangerousContent("CategoryMap");
            if (CMap.Length != 0)
            {
                String[] CMapArray = CMap.Split(',');
                foreach (String s in CMapArray)
                {
                    DB.ExecuteSQL("insert into Pollcategory(Pollid,categoryid) values(" + PollID.ToString() + "," + s + ")");
                }
            }

            // Update Section Mappings
            DB.ExecuteSQL("delete from Pollsection where Pollid=" + PollID.ToString());
            String SMap = CommonLogic.FormCanBeDangerousContent("SectionMap");
            if (SMap.Length != 0)
            {
                String[] SMapArray = SMap.Split(',');
                foreach (String s in SMapArray)
                {
                    DB.ExecuteSQL("insert into Pollsection(Pollid,sectionid) values(" + PollID.ToString() + "," + s + ")");
                }
            }
            PollCategories = AppLogic.GetPollCategories(PollID);
            PollSections = AppLogic.GetPollSections(PollID);
            ltCategoryList.Text = GetCategoryList(PollID, PollCategories, 0, 1, LocaleSetting, EntityHelpers);
            ltSectionList.Text = GetSectionList(PollID, PollSections, 0, 1, LocaleSetting, EntityHelpers);

            // Update Multi Store
            etsMapper.ObjectID = Localization.ParseNativeInt(PollID.ToString());
            etsMapper.Save();

            resetError(AppLogic.GetString("admin.editCreditCard.Updated", SkinID, LocaleSetting), false);
            Response.Redirect("editpolls.aspx?Pollid=" + PollID);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            PollCategories = AppLogic.GetPollCategories(PollID);
            PollSections = AppLogic.GetPollSections(PollID);
            InitializePageContent();
            resetError("", false);
        }
    }
}
