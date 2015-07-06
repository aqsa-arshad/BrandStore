// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class news : AdminPageBase
{
    protected Customer cust;
    protected bool bUseHtmlEditor;

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.CacheControl = "private";
        Response.Expires = 0;
        Response.AddHeader("pragma", "no-cache");

        cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

        if (!IsPostBack)
        {
            ViewState["EditingNews"] = false;
            ViewState["EditingNewsID"] = "0";

            loadDD();
            loadTree();
            resetForm();
            phMain.Visible = false;

            btnDelete.Attributes.Add("onClick", "return confirm('Confirm Delete');");
        }
        // Determine HTML editor configuration
        bUseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
        radCopy.Visible = bUseHtmlEditor;
        radCopy.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);


    }

    private void loadDD()
    {
        foreach (XmlNode xn in Localization.LocalesDoc.SelectNodes("/root/Locales"))
        {
            ddlPageLocales.Items.Add(new ListItem(xn.Attributes["Name"].InnerText, xn.Attributes["Name"].InnerText));
        }
        ddlPageLocales.Items.FindByValue(Localization.GetDefaultLocale()).Selected = true;

        if (ddlPageLocales.Items.Count < 2)
        {
            divPageLocale.Visible = false;
        }
    }
    private void loadTree()
    {
        try
        {
            treeMain.Nodes.Clear();            

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from News  with (NOLOCK)  where deleted=0 order by CreatedOn desc", dbconn))
                {
                    while (rs.Read())
                    {
                        TreeNode myNode = new TreeNode();

                        string temp = CommonLogic.Ellipses(DB.RSFieldByLocale(rs, "Headline", cust.LocaleSetting), 20, false) + " (" + Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs,"CreatedOn")) + ")";

                        myNode.Text = Server.HtmlEncode(temp);
                        myNode.Value = DB.RSFieldInt(rs, "NewsID").ToString();
                        myNode.ImageUrl = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/dot.gif");
                        treeMain.Nodes.Add(myNode);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            resetError(ex.ToString(), true);
        }
    }

    protected void treeMain_SelectedNodeChanged(object sender, EventArgs e)
    {
        resetForm();
        ViewState["EditingNews"] = true;
            
        resetError("", false);
        phMain.Visible = true;

        string index = treeMain.SelectedNode.Value;
        ViewState["EditingNewsID"] = index;

        getNewsDetails();

        etsMapper.ObjectID = Localization.ParseNativeInt(ViewState["EditingNewsID"].ToString());
        etsMapper.DataBind();
        litStoreMapper.Visible = etsMapper.StoreCount > 1;
        litStoreMapperHdr.Visible = etsMapper.StoreCount > 1;

    }

    protected void getNewsDetails()
    {
        using (SqlConnection dbconn = DB.dbConn())
        {
            dbconn.Open();
            using (IDataReader rs = DB.GetRS("select * from News  with (NOLOCK)  where NewsID=" + ViewState["EditingNewsID"].ToString() + " ORDER BY createdon DESC", dbconn))
            {
                if (!rs.Read())
                {
                    rs.Close();
                    resetError(AppLogic.GetString("admin.news.UnableToRetrieveData", SkinID, LocaleSetting), true);
                    return;
                }

                //editing News
                ltMode.Text = AppLogic.GetString("admin.common.Editing", SkinID, LocaleSetting);
                btnSubmit.Text = AppLogic.GetString("admin.news.UpdateNews", SkinID, LocaleSetting);
                btnDelete.Visible = true;

                string pageLocale = ddlPageLocales.SelectedValue;
                if (pageLocale.Equals(string.Empty))
                {
                    pageLocale = Localization.GetDefaultLocale();
                }

                ltHeadline.Text = DB.RSFieldByLocale(rs, "Headline", pageLocale);

                if (bUseHtmlEditor)
                {
                    radCopy.Content = DB.RSFieldByLocale(rs, "NewsCopy", pageLocale);
                }
                else
                {
                    ltCopy.Text = ("<div id=\"idNewsCopy\" style=\"height: 1%;\">");
                    ltCopy.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"NewsCopy\" name=\"NewsCopy\">" + XmlCommon.GetLocaleEntry(HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "NewsCopy")), pageLocale, false) + "</textarea>\n");
                    ltCopy.Text += ("</div>");
                }
                txtDate.SelectedDate = DB.RSFieldDateTime(rs, "ExpiresOn");

                rbPublished.Items.FindByValue(rs["Published"].ToString()).Selected = true;
            }
        }
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
        }
        else
        {
            str = "";
        }

        ltError.Text = str;
    }
    protected void btnAdd_Click(object sender, EventArgs e)
    {
        ViewState["EditingNews"] = false;
        ViewState["EditingNewsID"] = "0";
        phMain.Visible = true;
        btnDelete.Visible = false;
        resetForm();
        loadTree();
        //new News
        ltMode.Text = AppLogic.GetString("admin.editnews.AddingNews", SkinID, LocaleSetting);
        btnSubmit.Text = AppLogic.GetString("admin.editnews.Submit", SkinID, LocaleSetting);
        etsMapper.ObjectID = 0;
        etsMapper.DataBind();
    }

    protected bool validateInput()
    {
        string headline = ltHeadline.Text;
        if (string.IsNullOrEmpty( headline))
        {
            

            string temp = AppLogic.GetString("admin.news.EnterHeadline", SkinID, LocaleSetting);
            resetError(temp, true);
            return false;
        }
        return true;
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        bool Editing = Localization.ParseBoolean(ViewState["EditingNews"].ToString());
        string NewsID = ViewState["EditingNewsID"].ToString();
        
        string pageLocale = ddlPageLocales.SelectedValue;
        if (pageLocale.Equals(string.Empty))
        {
            pageLocale = Localization.GetDefaultLocale();
        }
                
        if (validateInput())
        {
            try
            {
				DateTime dt = txtDate.SelectedDate ?? System.DateTime.Now.AddMonths(6);
                StringBuilder sql = new StringBuilder(2500);
                if (!Editing)
                {
                    // ok to add them:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into news(NewsGUID,ExpiresOn,Headline,NewsCopy,Published) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.DateQuote(Localization.ToDBDateTimeString(dt)) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml(ltHeadline.Text, pageLocale)) + ",");
                    string copy = string.Empty;
                    if (bUseHtmlEditor)
                    {
                        copy = AppLogic.FormLocaleXml(radCopy.Content, pageLocale);
                    }
                    else
                    {
                        copy = AppLogic.FormLocaleXmlEditor("NewsCopy", "NewsCopy", pageLocale, "news", Convert.ToInt32(NewsID));
                    }
                    if (copy.Length != 0)
                    {
                        sql.Append(DB.SQuote(copy) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    sql.Append(rbPublished.SelectedValue.ToString() + "");
                    sql.Append(")");

                    DB.ExecuteSQL(sql.ToString());

                    resetError(AppLogic.GetString("admin.news.NewsAdded", SkinID, LocaleSetting), false);
                    loadTree();

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select NewsID from news  with (NOLOCK)  where deleted=0 and NewsGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            NewsID = DB.RSFieldInt(rs, "NewsID").ToString();
                            ViewState["EditingNews"] = true;
                            ViewState["EditingNewsID"] = NewsID;
                        }
                    }

                    getNewsDetails();
                }
                else
                {
                    // ok to update:
                    string copy = string.Empty;
                    sql.Append("update news set ");
                    sql.Append("Headline=" + DB.SQuote(AppLogic.FormLocaleXml("Headline", ltHeadline.Text, pageLocale, "news", Convert.ToInt32(NewsID))) + ",");
                    if (bUseHtmlEditor)
                    {
                        copy = AppLogic.FormLocaleXml("NewsCopy", radCopy.Content, pageLocale, "news", Convert.ToInt32(NewsID)); 
                    }
                    else
                    {
                        copy = AppLogic.FormLocaleXmlEditor("NewsCopy", "NewsCopy", pageLocale, "news", Convert.ToInt32(NewsID));
                    }
                    if (copy.Length != 0)
                    {
                        sql.Append("NewsCopy=" + DB.SQuote(copy) + ",");
                    }
                    else
                    {
                        sql.Append("NewsCopy=NULL,");
                    }
                    sql.Append("ExpiresOn=" + DB.DateQuote(Localization.ToDBDateTimeString(dt)) + ",");
                    sql.Append("Published=" + rbPublished.SelectedValue.ToString() + " ");
                    sql.Append("where NewsID=" + NewsID.ToString());
                    DB.ExecuteSQL(sql.ToString());
                    resetError(AppLogic.GetString("admin.news.NewsUpdated", SkinID, LocaleSetting), false);
                    loadTree();

                    getNewsDetails();
                }
            }
            catch (Exception ex)
            {
                resetError(String.Format(AppLogic.GetString("admin.news.UnexpectedError", SkinID, LocaleSetting),ex.ToString()), true);
            }
        }
        etsMapper.ObjectID = Localization.ParseNativeInt(NewsID);
        etsMapper.Save();
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string NewsID = ViewState["EditingNewsID"].ToString();
        DB.ExecuteSQL("update News set deleted=1 where NewsID=" + NewsID);
        phMain.Visible = false;
        loadTree();
        ViewState["EditingNews"] = false;
        ViewState["EditingNewsID"] = "0";
        resetError("News deleted.", false);
    }

    protected void resetForm()
    {
        ltHeadline.Text = "";
        if (bUseHtmlEditor)
        {
            radCopy.Content = "";
            ltCopy.Text = "";
        }
        else
        {
            ltCopy.Text = ("<div id=\"idNewsCopy\" style=\"height: 1%;\">");
            ltCopy.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"NewsCopy\" name=\"NewsCopy\"></textarea>\n");
            ltCopy.Text += ("</div>");
        }        

        txtDate.SelectedDate = System.DateTime.Now.AddMonths(1);

        rbPublished.SelectedIndex = 1;
    }

    protected void ddlPageLocales_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ViewState["EditingNewsID"].ToString() != "")
        {
            getNewsDetails();
        }
    }
}
}
