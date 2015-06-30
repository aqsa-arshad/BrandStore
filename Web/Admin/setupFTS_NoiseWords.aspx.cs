// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using AspDotNetStorefrontCore;
using System.Text;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    public partial class Admin_setupFTS_NoiseWords : AdminPageBase
    {
        protected string selectSQL = "select * from NoiseWords order by word";
        protected Customer cust;
        private int m_SkinID = 1;
        protected string JSWarnAddNewNoiseWord = string.Empty;
        protected string JSWarnDuplicateNoiseWord = string.Empty;


        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (!Page.IsPostBack)
            {
                buildGridData();
            }
            else
            {
                if (ltError.Text.Equals("", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    ltError.Text = "";
                }
            }
            loadLocaleStrings();
        }

        private void loadLocaleStrings()
        {
            btn_AddNewNoiseWord.Text = AppLogic.GetString("setupFTS.aspx.29", m_SkinID, cust.LocaleSetting);
            JSWarnAddNewNoiseWord = AppLogic.GetString("setupFTS.aspx.30", m_SkinID, cust.LocaleSetting);
            JSWarnDuplicateNoiseWord = AppLogic.GetString("setupFTS.aspx.31", m_SkinID, cust.LocaleSetting);
            hyperNoiseWord.Text = AppLogic.GetString("setupFTS.aspx.1", m_SkinID, cust.LocaleSetting);
        }

        protected void buildGridData()
        {
            try
            {
                using (DataTable dt = new DataTable())
                {
                    dt.Columns.Add("EditWord");

                    using (SqlConnection con = DB.dbConn())
                    {
                        con.Open();
                        using (IDataReader rs = DB.GetRS(selectSQL, con))
                        {
                            dt.Load(rs);
                        }
                    }

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            dr["EditWord"] = dr["word"];
                        }
                    }

                    gMain.DataSource = dt;
                    gMain.DataBind();
                }

            }
            catch (Exception ex)
            {
                string str;
                string error = ex.Message.ToString();
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", m_SkinID, cust.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

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
        }

        protected void gMain_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gMain.EditIndex = -1;
            buildGridData();
        }

        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                    ib.Attributes.Add("onClick", "javascript: return confirm('" + AppLogic.GetString("setupFTS.aspx.32", m_SkinID, cust.LocaleSetting) + "')");

                    //Click to edit
                    if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
                    {
                        e.Row.Attributes.Add("ondblclick", "javascript:__doPostBack('gMain','Edit$" + e.Row.RowIndex + "')");
                    }
                }
            }
            catch (Exception ex)
            {
                string str;
                string error = ex.Message.ToString();
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", m_SkinID, cust.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

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
        }

        protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteItem")
            {
                gMain.EditIndex = -1;
                int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());

                StringBuilder sql = new StringBuilder();
                sql.Append("delete from NoiseWords where ID=" + iden);
                try
                {
                    DB.ExecuteSQL(sql.ToString());
                    buildGridData();
                }
                catch (Exception ex)
                {
                    string str;
                    string error = ex.Message.ToString();
                    str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", m_SkinID, cust.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

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
            }
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {

            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            buildGridData();

        }

        protected void gMain_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gMain.Rows[e.RowIndex];

            try
            {
                if (row != null)
                {
                    string newWord = ((TextBox)row.FindControl("txtNewNoiseWord")).Text;
                    int newWordID = Convert.ToInt32((((Label)row.FindControl("lblNewNoiseWordID")).Text.ToString()));

                    int count = 0;
                    StringBuilder sql = new StringBuilder();

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader reader = DB.GetRS("select count(*) from NoiseWords where word = " + DB.SQuote(newWord), conn))
                        {
                            if (reader.Read())
                            {
                                count = reader.GetInt32(0);
                            }
                        }
                    }

                    if (count > 0)
                    {
                        gMain.EditIndex = -1;
                        buildGridData();
                        throw new Exception(JSWarnDuplicateNoiseWord);
                    }
                    else
                    {
                        StringBuilder sql2 = new StringBuilder();

                        sql2.Append("update NoiseWords set word =" + DB.SQuote(newWord));
                        sql2.Append(" where ID=" + newWordID);
                        DB.ExecuteSQL(sql2.ToString());
                        gMain.EditIndex = -1;
                        buildGridData();
                    }
                }
            }
            catch (Exception ex)
            {
                string str;
                string error = ex.Message.ToString();
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", m_SkinID, cust.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

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
        }

        protected void gMain_RowEditing(object sender, GridViewEditEventArgs e)
        {

            gMain.EditIndex = e.NewEditIndex;
            buildGridData();

        }
        protected void btn_AddNewNoiseWord_Click(object sender, EventArgs e)
        {
            try
            {
                int count = 0;
                StringBuilder sql = new StringBuilder();

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader reader = DB.GetRS("select count(*) from NoiseWords where word = " + DB.SQuote(txtNewNoiseWord.Text.Trim()), conn))
                    {
                        if (reader.Read())
                        {
                            count = reader.GetInt32(0);
                        }
                    }
                }

                if (count > 0)
                {
                    txtNewNoiseWord.Text = "";
                    throw new Exception(JSWarnDuplicateNoiseWord);
                }
                else
                {
                    sql.Append("insert NoiseWords values(");
                    sql.Append(DB.SQuote(txtNewNoiseWord.Text.Trim()));
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());
                    txtNewNoiseWord.Text = "";
                    buildGridData();
                }
            }
            catch (Exception ex)
            {
                string str;
                string error = ex.Message.ToString();
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", m_SkinID, cust.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

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
        }
    }
}
