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
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class eventhandler : AdminPageBase
    {
        protected string selectSQL = "select EventID, EventName, CalloutURL, XmlPackage, Active,Debug from EventHandler ";
        private Customer cust;

        protected void Page_Load(object sender, EventArgs e)
        {         
                Response.CacheControl = "private";
                Response.Expires = 0;
                Response.AddHeader("pragma", "no-cache");

                cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

                if (!IsPostBack)
                {
                    string query = CommonLogic.QueryStringCanBeDangerousContent("searchfor");

                    loadXMLPackages();

                    ViewState["Sort"] = "EventName";
                    ViewState["SortOrder"] = "ASC";
                    ViewState["SQLString"] = selectSQL;

                    if (query.Length > 0)
                    {
                        resultFilter(query);
                    }
                    else
                    {
                        BuildGridData();
                    }
                }
            
        }
       
        protected void resultFilter(string SearchFor)
        {
            String sql = selectSQL + " with (NOLOCK) ";
            String WhereClause= String.Empty;

            //search
            if (SearchFor.Length != 0)
            {
                WhereClause = " (EventName like " + DB.SQuote("%" + SearchFor + "%") + " or CalloutURL like " + DB.SQuote("%" + SearchFor + "%") + ")";
            }

            //set states
            ViewState["SQLString"] = sql.ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();
            
            BuildGridData(sql);
          
        }

        protected void BuildGridData()
        {
            string sql = ViewState["SQLString"].ToString();
            sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

            BuildGridData(sql);
        }
     
        protected void BuildGridData(string sql)
        {
            using (DataTable dt = new DataTable())
            {
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(sql, dbconn))
                    {
                        dt.Load(rs);
                        gMain.DataSource = dt;
                        gMain.DataBind();
                    }
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

        protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            btnInsert.Enabled = true;
            //if new item and cancel, must delete
            if (Localization.ParseBoolean(ViewState["IsInsert"].ToString()))
            {
                GridViewRow row = gMain.Rows[e.RowIndex];
                if (row != null)
                {
                    int iden = Localization.ParseNativeInt(row.Cells[1].Text.ToString());
                    deleteRow(iden);
                }
            }

            ViewState["IsInsert"] = false;

            gMain.EditIndex = -1;
            BuildGridData();
        }
        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                ib.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("admin.common.ConfirmDeletion", SkinID, LocaleSetting) + "')");

                //Click to edit
                if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
                {
                    e.Row.Attributes.Add("ondblclick", "javascript:__doPostBack('gMain','Edit$" + e.Row.RowIndex + "')");
                }
            }
            if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
            {
                DataRowView myrow = (DataRowView)e.Row.DataItem;


                DropDownList ddXmlPackage = (DropDownList)e.Row.FindControl("ddEditXMLPackage");
                ddXmlPackage.Items.Clear();
                ListItem myNode = new ListItem();
                myNode.Value = AppLogic.GetString("admin.eventhandler.SelectPackage", SkinID, LocaleSetting);
                ddXmlPackage.Items.Add(myNode);

                String Location = CommonLogic.SafeMapPath("~/XMLPackages");
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
                foreach (System.IO.FileInfo f in dir.GetFiles("event.*")) 
                {
                    //LOAD FILES 
                    ListItem myNode1 = new ListItem();
                    myNode1.Value = f.Name.ToUpperInvariant();
                    ddXmlPackage.Items.Add(myNode1);
                } 
                
                if (ddXmlPackage.Items.Count > 1)
                {
                    ddXmlPackage.SelectedValue = myrow["XMLPackage"].ToString().ToUpperInvariant();
                }
               try
                {
                    if (ViewState["FirstTimeEdit"].ToString() == "0")
                    {
                        TextBox txt = (TextBox)e.Row.FindControl("txtEventName");
                        txt.Visible = false;
                        Literal lt = (Literal)e.Row.FindControl("ltEventName");
                        lt.Visible = true;
                    }
                    else
                    {
                        TextBox txt = (TextBox)e.Row.FindControl("txtEventName");
                        txt.Visible = true;
                        Literal lt = (Literal)e.Row.FindControl("ltEventName");
                        lt.Visible = false;
                    }
                }
                catch
                {
                    TextBox txt = (TextBox)e.Row.FindControl("txtEventName");
                    txt.Visible = false;
                    Literal lt = (Literal)e.Row.FindControl("ltEventName");
                    lt.Visible = true;
                }
            }
        }
        protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            ViewState["Sort"] = e.SortExpression.ToString();
            ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
            BuildGridData();
        }
        protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            resetError("", false);

            if (e.CommandName == "DeleteItem")
            {
                ViewState["IsInsert"] = false;
                gMain.EditIndex = -1;
                int eventID = Localization.ParseNativeInt(e.CommandArgument.ToString());
                deleteRow(eventID);
            }
        }
        protected void deleteRow(int iden)
        {
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("DELETE FROM EventHandler WHERE EventID=" + iden.ToString());
            try
            {
                DB.ExecuteSQL(sql.ToString());

                loadXMLPackages();

                BuildGridData();
                resetError("Item Deleted", false);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrUpdateDB", SkinID, LocaleSetting),ex.ToString()));
            }
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ViewState["IsInsert"] = false;
            resetError("", false);
            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            BuildGridData();
        }
        protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            btnInsert.Enabled = true;
         
            ViewState["IsInsert"] = false;
            GridViewRow row = gMain.Rows[e.RowIndex];

            if (row != null)
            {
                string iden = row.Cells[1].Text.ToString();
                TextBox eventname = (TextBox)row.FindControl("txtEventName");
                TextBox xmlpackage = (TextBox)row.FindControl("txtXMLPackage");
                TextBox callouturl = (TextBox)row.FindControl("txtCalloutURL");
                DropDownList ddXMLPackage = (DropDownList)row.FindControl("ddEditXMLPackage");
                CheckBox Active = (CheckBox)row.FindControl("cbkActive");
                CheckBox Debug = (CheckBox)row.FindControl("cbkDebug");

                string XMLPackagename = ddXMLPackage.SelectedValue;
                if (ddXMLPackage.SelectedIndex == 0)
                {
                    XMLPackagename = xmlpackage.Text;
                }

                AspdnsfEventHandler a = AppLogic.eventHandler(eventname.Text);
                if (a == null)
                {
                    resetError(AppLogic.GetString("admin.eventhanlder.NonExistingEventHandler", SkinID, LocaleSetting), true);
                }
                else
                {
                   
                    a.Update(eventname.Text, callouturl.Text, XMLPackagename.ToUpperInvariant(), Active.Checked,Debug.Checked);   //TO_DO_MJ : How to set Active parameter on this page. Defaulted it to TRUE            

                    resetError(AppLogic.GetString("admin.common.ItemUpdated", SkinID, LocaleSetting), false);
                }
                try
                {
                    loadXMLPackages();
                    gMain.EditIndex = -1;
                    resultFilter("");
                }
                catch { }
                }
              
        }
        protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
        {
            btnInsert.Enabled = false;
            ViewState["IsInsert"] = false;
            gMain.EditIndex = e.NewEditIndex;
            BuildGridData();
        }
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            StringBuilder sql = new StringBuilder(2500);

            if (txtAddName.Text.Trim().Length > 0 && txtAddName.Text.Trim().Equals("eventhandler name", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                string name = txtAddName.Text.Trim();
                // see if this name is already there:
                if (AppLogic.eventHandler(name) != null)
                {
                    if (AppLogic.eventHandler(name).EventName.Length > 0)
                    {
                        resetError(AppLogic.GetString("admin.eventhandler.DuplicateEventHandler", SkinID, LocaleSetting), true);
                        return;
                    }
                }
                try
                {
                    AppLogic.EventHandlerTable.Add(name, txtAddURL.Text.Trim(), ddAddXmlPackage.SelectedValue, true,false);//TO_DO_MJ Active value defualted to true again here
                    ViewState["SQLString"] = selectSQL + " WHERE [EventName]=" + DB.SQuote(name);
                    ViewState["Sort"] = "EventID";
                    ViewState["SortOrder"] = "DESC";

                    ViewState["FirstTimeEdit"] = "0";//"1";

                    BuildGridData();
                    resetError("Item added.", false);
                    ViewState["IsInsert"] = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format(AppLogic.GetString("admin.common.ErrUpdateDB", SkinID, LocaleSetting),ex.ToString()));
                }
            }
            else
            {
                resetError(AppLogic.GetString("admin.eventhandler.EnterEventHandler", SkinID, LocaleSetting), true);
            }
        }


        protected void ddAddXmlPackage_SelectedIndexChanged(object sender, EventArgs e)
        {      
            ViewState["IsInsert"] = false;
            gMain.EditIndex = -1;
            resetError("", false);
            gMain.PageIndex = 0;
            resultFilter("");
      
        }


        private void loadXMLPackages()
        {
            String Location = CommonLogic.SafeMapPath("~/XMLPackages");
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
            foreach (System.IO.FileInfo f in dir.GetFiles("event.*")) 
            {
                //LOAD FILES 
                ListItem myNode = new ListItem();
                myNode.Value = f.Name.ToUpperInvariant();
                ddAddXmlPackage.Items.Add(myNode);
            } 
        }
}
}
