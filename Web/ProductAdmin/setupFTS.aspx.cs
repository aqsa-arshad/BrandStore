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
using System.Resources;
using AspDotNetStorefrontCore;
using System.Text;
using System.Data.SqlClient;
using System.IO;

namespace AspDotNetStorefrontAdmin
{

    public partial class Admin_setupFTS : AdminPageBase
    {
        ftsddl ddl = new ftsddl();

        protected string JSwarn1 = string.Empty;
        protected string JSwarn2 = string.Empty;
        protected string JSwarn3 = string.Empty;
        protected string JSwarn4 = string.Empty;
        protected string JSwarn5 = string.Empty;
        protected string JSwarn6 = string.Empty;
        protected string JSwarn7 = string.Empty;
        protected string JSwarn8 = string.Empty;
        protected string JSwarn9 = string.Empty;
        protected string JSwarn10 = string.Empty;


        protected void Page_Load(object sender, EventArgs e)
        {
            radioCreate.Attributes.Add("onclick", "CreateNew();");
            radioReuse.Attributes.Add("onclick", "Reuse();");
            lstCatalogNames.Attributes.Add("disabled", "true");

            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            if (!IsPostBack)
            {
                try
                {
                    int control = 0;

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader reader5 = DB.GetRS(ddl.sqlCheckFTIndex, conn))
                        {
                            if (reader5.Read())
                            {
                                control = reader5.GetInt32(0);

                                if (control > 0)
                                {
                                    lblMSFTESQL.Visible = false;
                                    lblEnableFTS.Text = AppLogic.GetString("setupFTS.aspx.27", SkinID, LocaleSetting);
                                    lblEnableFTS.Visible = true;
                                    hyperNoiseWord.Visible = true;
                                    btn_installFTS.Visible = false;
                                    lblLanguage.Visible = false;
                                    ddlLanguage.Visible = false;
                                    radioCreate.Visible = false;
                                    radioReuse.Visible = false;
                                    lblCatalogList.Visible = false;
                                    lstCatalogNames.Visible = false;
                                    lblNewCatalogName.Visible = false;
                                    txtNewCatalogName.Visible = false;
                                    lblNewCatalogPath.Visible = false;
                                    txtNewCatalogPath.Visible = false;

                                    throw new Exception(AppLogic.GetString("setupFTS.aspx.25", SkinID, LocaleSetting));
                                }
                            }
                        }
                    }

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader reader1 = DB.GetRS(ddl.sqlCheckFTSEngine, conn))
                        {
                            if (reader1.Read())
                            {
                                control = reader1.GetInt32(0);

                                if (control < 1)
                                {
                                    lblMSFTESQL.Visible = true;
                                    lblEnableFTS.Visible = false;
                                    btn_installFTS.Enabled = false;
                                    lblCatalogList.Visible = false;
                                    lstCatalogNames.Visible = false;
                                    lblNewCatalogName.Visible = false;
                                    txtNewCatalogName.Visible = false;
                                    lblNewCatalogPath.Visible = false;
                                    txtNewCatalogPath.Visible = false;
                                    radioCreate.Visible = false;
                                    radioReuse.Visible = false;
                                    btn_optimize.Visible = false;
                                    btn_uninstallFTS.Visible = false;
                                    control = 0;
                                }
                                else
                                {
                                    control = 0;
                                    lblMSFTESQL.Visible = false;
                                    btn_uninstallFTS.Visible = false;
                                    btn_optimize.Visible = false;

                                    using (SqlConnection conn2 = DB.dbConn())
                                    {
                                        conn2.Open();
                                        using (IDataReader reader2 = DB.GetRS(ddl.sqlCheckFTSEnabled, conn2))
                                        {
                                            if (reader2.Read())
                                            {
                                                control = reader2.GetInt32(0);

                                                if (control < 1)
                                                {
                                                    lblEnableFTS.Visible = true;
                                                    control = 0;

                                                    checkCatalogs();
                                                }
                                                else
                                                {
                                                    lblEnableFTS.Visible = false;
                                                    control = 0;

                                                    checkCatalogs();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string str;
                    string error = ex.Message.ToString();

                    if (error.Trim().Equals(AppLogic.GetString("setupFTS.aspx.25", SkinID, LocaleSetting)) == true)
                    {
                        str = "<font class=\"errorMsg\">" + AppLogic.GetString("setupFTS.aspx.26", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
                    }
                    else
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

                finally
                {
                    loadLocaleStrings();
                }
            }
            else // page is postback
            {
                int control = 0;

                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader reader9 = DB.GetRS(ddl.sqlCheckFTIndex, conn))
                    {
                        if (reader9.Read())
                        {
                            control = reader9.GetInt32(0);

                            if (control < 1 && btn_installFTS.Visible)
                            {
                                string query = string.Empty;
                                control = 0;

                                SqlConnection connection = new SqlConnection();
                                connection.ConnectionString = DB.GetDBConn();
                                connection.Open();

                                SqlCommand command = connection.CreateCommand();
                                SqlTransaction transaction;

                                transaction = connection.BeginTransaction();

                                command.Connection = connection;
                                command.Transaction = transaction;

                                try
                                {
                                    using (SqlConnection conn2 = DB.dbConn())
                                    {
                                        conn2.Open();
                                        using (IDataReader reader10 = DB.GetRS(ddl.sqlCheckFTSEnabled, conn2))
                                        {
                                            if (reader10.Read())
                                            {
                                                control = reader10.GetInt32(0);

                                                if (control < 1)
                                                {
                                                    DB.ExecuteSQL(ddl.query1);
                                                    control = 1;
                                                }
                                            }
                                        }
                                    }

                                    if (CommonLogic.HasInvalidChar(txtNewCatalogName.Text) && txtNewCatalogName.Text.Length != 0)
                                    {
                                        Exception invalid = new Exception(AppLogic.GetString("setupFTS.aspx.33", SkinID, LocaleSetting));
                                        throw invalid;
                                    }
                                    string language = ddlLanguage.SelectedValue.Trim();

                                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                                    {
                                        con.Open();
                                        string collationquery = "SELECT databasepropertyex(db_name(),'collation') AS collation_name";
                                        using (IDataReader rscoll = DB.GetRS(collationquery, con))
                                        {
                                            if (rscoll.Read())
                                            {
                                                if (DB.RSField(rscoll, "collation_name") == "SQL_Latin1_General_CP1_CI_AS")
                                                {
                                                    if (language.Equals("Chinese-Simplified") || language.Equals("Chinese-Traditional") | language.Equals("Russian") || language.Equals("Japanese"))
                                                    {
                                                        Exception invalid = new Exception(AppLogic.GetString("setupFTS.aspx.34", SkinID, LocaleSetting) + " " + language);
                                                        throw invalid;
                                                    }
                                                }
                                            }
                                        }
                                    }


                                    if (radioCreate.Checked.Equals(true) && radioReuse.Checked.Equals(false))
                                    {
                                        if (txtNewCatalogName.Text.Trim().Equals(string.Empty) == false && txtNewCatalogPath.Text.Trim().Equals(string.Empty) == false)
                                        {
                                            query = query + ddl.query2 + txtNewCatalogName.Text.Trim().Replace(" ", "_") + " IN PATH " + DB.SQuote(txtNewCatalogPath.Text.Trim());
                                            DB.ExecuteSQL(query);
                                            query = string.Empty;
                                            query = query + ddl.query4 + txtNewCatalogName.Text.Trim() + ddl.query5;
                                            DB.ExecuteSQL(query);
                                            query = string.Empty;
                                            query = query + ddl.query6;
                                            command.CommandText = query;
                                            command.ExecuteNonQuery();
                                            query = string.Empty;
                                        }
                                        else if (txtNewCatalogName.Text.Trim().Equals(string.Empty) == false && txtNewCatalogPath.Text.Trim().Equals(string.Empty) == true)
                                        {
                                            query = query + ddl.query2 + txtNewCatalogName.Text.Trim().Replace(" ", "_");
                                            DB.ExecuteSQL(query);
                                            query = string.Empty;
                                            query = query + ddl.query4 + txtNewCatalogName.Text.Trim().Replace(" ", "_") + ddl.query5;
                                            DB.ExecuteSQL(query);
                                            query = string.Empty;
                                            query = query + ddl.query6;
                                            command.CommandText = query;
                                            command.ExecuteNonQuery();
                                            query = string.Empty;
                                        }
                                        else if (txtNewCatalogName.Text.Trim().Equals(string.Empty) == true && txtNewCatalogPath.Text.Trim().Equals(string.Empty) == false)
                                        {
                                            throw new Exception(JSwarn4);
                                        }
                                        else
                                        {
                                            throw new Exception(JSwarn5);
                                        }
                                    }
                                    else if (radioCreate.Checked.Equals(false) && radioReuse.Checked.Equals(true))
                                    {
                                        if (txtNewCatalogName.Text.Trim().Equals(string.Empty) == true && txtNewCatalogPath.Text.Trim().Equals(string.Empty) == true)
                                        {
                                            if (lstCatalogNames.SelectedValue.Trim().Equals(string.Empty) == false)
                                            {
                                                query = query + ddl.query3 + lstCatalogNames.SelectedValue.Trim().Replace(" ", "_") + " REORGANIZE";
                                                DB.ExecuteSQL(query);
                                                query = string.Empty;
                                                query = query + ddl.query4 + lstCatalogNames.SelectedValue.Trim().Replace(" ", "_") + ddl.query5;
                                                DB.ExecuteSQL(query);
                                                query = string.Empty;
                                                query = query + ddl.query6;
                                                command.CommandText = query;
                                                command.ExecuteNonQuery();
                                                query = string.Empty;
                                            }
                                            else
                                            {
                                                throw new Exception(JSwarn6);
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception(JSwarn8);
                                        }
                                    }



                                    if (language.Equals("Chinese-Simplified"))
                                    {
                                        query = query + ddl.Chinese_Simplified();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Chinese-Traditional"))
                                    {
                                        query = query + ddl.Chinese_Traditional();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Danish"))
                                    {
                                        query = query + ddl.Danish();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Dutch"))
                                    {
                                        query = query + ddl.Dutch();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("English-International"))
                                    {
                                        query = query + ddl.English_International();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("English-US"))
                                    {
                                        query = query + ddl.English_US();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("French"))
                                    {
                                        query = query + ddl.French();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("German"))
                                    {
                                        query = query + ddl.German();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Italian"))
                                    {
                                        query = query + ddl.Italian();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Japanese"))
                                    {
                                        query = query + ddl.Japanese();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Korean"))
                                    {
                                        query = query + ddl.Korean();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Neutral"))
                                    {
                                        query = query + ddl.Neutral();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Polish"))
                                    {
                                        query = query + ddl.Polish();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Portuguese"))
                                    {
                                        query = query + ddl.Portugese();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Portuguese(Brazil)"))
                                    {
                                        query = query + ddl.Portugese_Brazil();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Russian"))
                                    {
                                        query = query + ddl.Russian();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Spanish"))
                                    {
                                        query = query + ddl.Spanish();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Swedish"))
                                    {
                                        query = query + ddl.Swedish();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else if (language.Equals("Thai"))
                                    {
                                        query = query + ddl.Thailand();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }
                                    else
                                    {
                                        query = query + ddl.Turkish();
                                        command.CommandText = query;
                                        command.ExecuteNonQuery();
                                        query = string.Empty;
                                    }

                                    query = query + ddl.query7;
                                    command.CommandText = query;
                                    command.ExecuteNonQuery();
                                    query = string.Empty;

                                    query = query + ddl.query8;
                                    command.CommandText = query;
                                    command.ExecuteNonQuery();
                                    query = string.Empty;

                                    transaction.Commit();

                                    Response.Redirect(AppLogic.AdminLinkUrl("setupFTS.aspx"), true);
                                }
                                catch (Exception ex)
                                {
                                    btn_installFTS.Enabled = false;
                                    string str;
                                    string error = ex.Message.ToString();
                                    str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

                                    if (error.Length > 0)
                                    {
                                        str += error + "";
                                    }
                                    else
                                    {
                                        str = "";
                                    }

                                    ltError.Text = str;

                                    try
                                    {
                                        transaction.Rollback();

                                        if (control > 0)
                                        {
                                            DB.ExecuteSQL(ddl.query1_uninstall);
                                        }

                                        control = 0;

                                        using (SqlConnection conn2 = DB.dbConn())
                                        {
                                            conn2.Open();
                                            using (IDataReader reader6 = DB.GetRS(ddl.sqlCheckFTIndex, conn2))
                                            {
                                                if (reader6.Read())
                                                {
                                                    control = reader6.GetInt32(0);

                                                    if (control > 0)
                                                    {
                                                        DB.ExecuteSQL(ddl.sqlDropFTIndex);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex2)
                                    {
                                        string error2 = ex2.Message.ToString();
                                        if (error2.Length > 0)
                                        {
                                            str += error2 + "";
                                        }
                                        ltError.Text = str;
                                    }
                                }
                                finally
                                {
                                    command.Dispose();
                                    connection.Close();
                                    connection.Dispose();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void loadLocaleStrings()
        {
            lblIntro.Text = AppLogic.GetString("setupFTS.aspx.2", SkinID, LocaleSetting);
            lblMSFTESQL.Text = AppLogic.GetString("setupFTS.aspx.3", SkinID, LocaleSetting);

            if (lblEnableFTS.Text.Trim().Equals(AppLogic.GetString("setupFTS.aspx.27", SkinID, LocaleSetting)) == false)
            {
                lblEnableFTS.Text = AppLogic.GetString("setupFTS.aspx.4", SkinID, LocaleSetting);
            }

            lblLanguage.Text = AppLogic.GetString("setupFTS.aspx.5", SkinID, LocaleSetting);
            radioCreate.Text = AppLogic.GetString("setupFTS.aspx.6", SkinID, LocaleSetting);
            radioReuse.Text = AppLogic.GetString("setupFTS.aspx.7", SkinID, LocaleSetting);
            lblNewCatalogName.Text = AppLogic.GetString("setupFTS.aspx.8", SkinID, LocaleSetting);
            lblNewCatalogPath.Text = AppLogic.GetString("setupFTS.aspx.9", SkinID, LocaleSetting);
            lblCatalogList.Text = AppLogic.GetString("setupFTS.aspx.10", SkinID, LocaleSetting);
            btn_installFTS.Text = AppLogic.GetString("setupFTS.aspx.11", SkinID, LocaleSetting);
            btn_uninstallFTS.Text = AppLogic.GetString("setupFTS.aspx.21", SkinID, LocaleSetting);
            btn_optimize.Text = AppLogic.GetString("setupFTS.aspx.22", SkinID, LocaleSetting);
            hyperNoiseWord.Text = AppLogic.GetString("setupFTS.aspx.28", SkinID, LocaleSetting);

            JSwarn1 = AppLogic.GetString("setupFTS.aspx.12", SkinID, LocaleSetting) + " ";
            JSwarn2 = " " + AppLogic.GetString("setupFTS.aspx.13", SkinID, LocaleSetting) + " ";
            JSwarn3 = " " + AppLogic.GetString("setupFTS.aspx.14", SkinID, LocaleSetting);
            JSwarn4 = AppLogic.GetString("setupFTS.aspx.15", SkinID, LocaleSetting);
            JSwarn5 = AppLogic.GetString("setupFTS.aspx.16", SkinID, LocaleSetting);
            JSwarn6 = AppLogic.GetString("setupFTS.aspx.17", SkinID, LocaleSetting);
            JSwarn7 = AppLogic.GetString("setupFTS.aspx.18", SkinID, LocaleSetting) + " ";
            JSwarn8 = AppLogic.GetString("setupFTS.aspx.19", SkinID, LocaleSetting);
            JSwarn9 = AppLogic.GetString("setupFTS.aspx.23", SkinID, LocaleSetting);
            JSwarn10 = AppLogic.GetString("setupFTS.aspx.24", SkinID, LocaleSetting);

        }

        private void checkCatalogs()
        {
            try
            {
                int control = 0;

                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader reader3 = DB.GetRS(ddl.sqlSearchFTCatalogs, conn))
                    {
                        if (reader3.Read())
                        {
                            control = reader3.GetInt32(0);

                            if (control < 1)
                            {
                                lblCatalogList.Visible = false;
                                lstCatalogNames.Visible = false;
                                radioReuse.Enabled = false;
                                txtNewCatalogName.Enabled = true;
                                txtNewCatalogPath.Enabled = true;
                                radioCreate.Checked = true;
                                control = 0;
                            }
                            else
                            {
                                using (SqlConnection conn2 = new SqlConnection(DB.GetDBConn()))
                                {
                                    conn2.Open();
                                    using (IDataReader reader4 = DB.GetRS(ddl.sqlAvailFTCatalogs, conn2))
                                    {
                                        while (reader4.Read())
                                        {
                                            lstCatalogNames.Items.Add(DB.RSField(reader4, "Name").Replace("_", " "));
                                        }
                                    }
                                }

                                lblCatalogList.Visible = true;
                                lstCatalogNames.Visible = true;
                                radioReuse.Enabled = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void btn_uninstallFTS_Click(object sender, EventArgs e)
        {

            int control = 0;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader reader11 = DB.GetRS(ddl.sqlCheckFTIndex, conn))
                {
                    if (reader11.Read())
                    {
                        control = reader11.GetInt32(0);

                        if (control > 0)
                        {
                            string query = string.Empty;
                            control = 0;
                            string catalogName = string.Empty;

                            using (SqlConnection conn2 = DB.dbConn())
                            {
                                conn2.Open();
                                using (IDataReader reader7 = DB.GetRS(ddl.sqlGetFTCatalogName, conn2))
                                {
                                    if (reader7.Read())
                                    {
                                        catalogName = reader7.GetString(0);
                                    }
                                }
                            }

                            SqlConnection connection = new SqlConnection();
                            connection.ConnectionString = DB.GetDBConn();
                            connection.Open();

                            SqlCommand command = connection.CreateCommand();
                            SqlTransaction transaction;

                            transaction = connection.BeginTransaction();

                            command.Connection = connection;
                            command.Transaction = transaction;

                            try
                            {
                                DB.ExecuteSQL(ddl.query1_uninstall);

                                DB.ExecuteSQL(ddl.sqlDropFTIndex);

                                query = query + ddl.query6_uninstall;
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                                query = string.Empty;

                                query = query + ddl.query7_uninstall;
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                                query = string.Empty;

                                query = query + ddl.query8_uninstall;
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                                query = string.Empty;

                                transaction.Commit();

                                Response.Redirect(AppLogic.AdminLinkUrl("setupFTS.aspx"), false);
                            }
                            catch (Exception ex)
                            {
                                string str;
                                string error = ex.Message.ToString();
                                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

                                if (error.Length > 0)
                                {
                                    str += error + "";
                                }
                                else
                                {
                                    str = "";
                                }

                                ltError.Text = str;

                                try
                                {
                                    transaction.Rollback();

                                    DB.ExecuteSQL(ddl.query1);

                                    using (SqlConnection conn3 = DB.dbConn())
                                    {
                                        conn3.Open();
                                        using (IDataReader reader8 = DB.GetRS(ddl.sqlCheckFTIndex, conn3))
                                        {
                                            if (reader8.Read())
                                            {
                                                control = reader8.GetInt32(0);

                                                if (control < 1)
                                                {
                                                    query = string.Empty;
                                                    query = query + ddl.query4 + catalogName + ddl.query5;
                                                    DB.ExecuteSQL(query);
                                                    query = string.Empty;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    string error2 = ex2.Message.ToString();
                                    if (error2.Length > 0)
                                    {
                                        str += error2 + "";
                                    }
                                    ltError.Text = str;
                                }
                            }
                            finally
                            {
                                command.Dispose();
                                connection.Close();
                                connection.Dispose();
                            }
                        }
                    }
                }
            }
        }


        protected void btn_optimize_Click(object sender, EventArgs e)
        {
            string query = string.Empty;
            string catalogName = string.Empty;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader reader7 = DB.GetRS(ddl.sqlGetFTCatalogName, conn))
                {
                    if (reader7.Read())
                    {
                        catalogName = reader7.GetString(0);
                    }
                }
            }

            query = query + ddl.query3 + catalogName.Trim() + " REORGANIZE";
            DB.ExecuteSQL(query);
            query = string.Empty;

            Response.Redirect(AppLogic.AdminLinkUrl("setupFTS.aspx"), false);
        }
    }//end class
}//end namespace









