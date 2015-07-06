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
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
namespace AspDotNetStorefrontAdmin
{
    public partial class Admin_NewsletterMailingListManager : AdminPageBase
    {

        private SqlCommand _cmdGetList;
        private SqlCommand cmdGetList
        {
            get
            {
                if (_cmdGetList != null)
                {
                    return _cmdGetList;
                }
                string cmdText = @"
SELECT 
    EmailAddress, 
    FirstName + ' ' + LastName AS [Name], 
    SubscriptionConfirmed, 
    AddedOn, 
    SubscribedOn, 
    UnsubscribedOn
FROM  NewsletterMailList WITH (NOLOCK) 
WHERE 
	((SubscriptionConfirmed = @Confirmed) OR ISNULL(@Confirmed, -1) =-1) AND
	((EmailAddress LIKE @EmailAddress) OR (ISNULL(@EmailAddress, 'NULL') = 'NULL')) AND
	((FirstName + ' ' + LastName LIKE @Name) OR (ISNULL(@Name, 'NULL') = 'NULL')) 
";
                SqlCommand xCmd = new SqlCommand(cmdText);

                xCmd.Parameters.Add("@Confirmed", SqlDbType.Int);
                xCmd.Parameters.Add("@EmailAddress", SqlDbType.VarChar, 50);
                xCmd.Parameters.Add("@Name", SqlDbType.VarChar, 50);
                foreach (SqlParameter xParm in xCmd.Parameters)
                {
                    xParm.Value = DBNull.Value;
                }

                _cmdGetList = xCmd;

                return xCmd;
            }
        }

        private string cmdUnsubscribe
        {
            get
            {
                return @"
UPDATE NewsletterMailList SET SubscriptionConfirmed = 0, UnsubscribedOn = GETDATE()
WHERE EmailAddress = @EmailAddress AND SubscriptionConfirmed = 1
";
            }
        }

        private static class Settings
        {
            internal static bool recordingFirstLast
            { get { return bool.Parse(AppLogic.AppConfig("Newsletter.GetFirstAndLast")); } }
        }
        private static class Labels
        {
            internal static string firstName
            { get { return AppLogic.GetString("Global.FirstName", 0, string.Empty); } }
            internal static string lastName
            { get { return AppLogic.GetString("Global.LastName", 0, string.Empty); } }
            internal static string SubscriptionConfirmed
            { get { return AppLogic.GetString("Newsletter.ListManager.SubscriptionConfirmed", 0, string.Empty); } }
            internal static string AddedOn
            { get { return AppLogic.GetString("Newsletter.ListManager.AddedOn", 0, string.Empty); } }
            internal static string ConfirmedOn
            { get { return AppLogic.GetString("Newsletter.ListManager.ConfirmedOn", 0, string.Empty); } }
            internal static string UnsubscribedOn
            { get { return AppLogic.GetString("Newsletter.ListManager.UnsubscribedOn", 0, string.Empty); } }
        }

        private string SearchText
        {
            get { return string.Format("%{0}%", txtSearch.Text); }
        }
        private enum enSearchMode
        {
            None,
            ByName,
            ByEmailAddress
        }
        private enSearchMode SearchMode
        {
            get { return enSearchMode.ByEmailAddress; }
        }



        DataTable _source;
        private DataTable NewsletterList
        {
            get
            {
                using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
                {
                    _source = new DataTable();
                    cmdGetList.Connection = xCon;
                    SqlDataAdapter xAdp = new SqlDataAdapter(cmdGetList);
                    xCon.Open();
                    xAdp.Fill(_source);
                    if (!Settings.recordingFirstLast)
                    {
                        _source.Columns.Remove("Name");
                    }
                    _source.Columns["SubscriptionConfirmed"].ColumnName = Labels.SubscriptionConfirmed;
                    _source.Columns["AddedOn"].ColumnName = Labels.AddedOn;
                    _source.Columns["SubscribedOn"].ColumnName = Labels.ConfirmedOn;
                    _source.Columns["UnsubscribedOn"].ColumnName = Labels.UnsubscribedOn;


                    return _source;
                }
            }
        }

        private void ConfirmAdmin()
        {
            Server.ScriptTimeout = 5000000;

            if (!ThisCustomer.IsAdminUser)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
            }
        }
        private void ExecuteUnsubscribe(string emailAddress)
        {
            DB.ExecuteSQL(
                cmdUnsubscribe,
                new SqlParameter[] { 
                new SqlParameter("@EmailAddress", emailAddress) });
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ConfirmAdmin();
                gridRefresh();
            }

            litMultiButtonBreak.Visible = Settings.recordingFirstLast;
            cmdSearchName.Visible = Settings.recordingFirstLast;
            Page.Form.DefaultButton = cmdSearchName.UniqueID;
            Page.Form.DefaultFocus = txtSearch.ClientID;

        }

        protected void grdData_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void gridRefresh()
        {
            grdData.DataSource = null;
            grdData.DataSource = NewsletterList;
            if (NewsletterList.Rows.Count == 0)
            {
                litNoRecords.Text = "No subscribers on Newsletter mailing list";
                grdData.Visible = false;
            }
            else
            {
                grdData.DataBind();
            }

        }
        protected void grdData_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Unsubscribe")
            {
                ExecuteUnsubscribe(e.CommandArgument.ToString());
                gridRefresh();
            }
        }
        protected void grdData_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {

        }
        protected void cmdSearch_Click(object sender, EventArgs e)
        {

            _source = null;
            _cmdGetList = null;
            if (((Button)sender).Text.Contains("Name"))
            {
                cmdGetList.Parameters["@Name"].Value = SearchText;
            }
            else
            {
                cmdGetList.Parameters["@EmailAddress"].Value = SearchText;
            }
            gridRefresh();
        }
        protected void grdData_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdData.PageIndex = e.NewPageIndex;
            grdData.DataSource = null;
            grdData.DataSource = NewsletterList;
            grdData.DataBind();
        }
    }
}
