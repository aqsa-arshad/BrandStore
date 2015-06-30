// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using System.Web.UI.WebControls;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for polls.
	/// </summary>
	public partial class polls : SkinBase
	{
        List<int> listOfPollID = new List<int>(); 
		protected void Page_Load(object sender, System.EventArgs e)
		{
			if(AppLogic.AppConfigBool("GoNonSecureAgain"))
			{
				GoNonSecureAgain();
			}
            if (!ThisCustomer.IsRegistered)
            {
                dtlPolls.Visible = false;
            }

            lblPoll.Text = AppLogic.GetString("polls.aspx.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

            InitializeDataSource();
        }

        /// <summary>
        /// Initializes the data source.
        /// </summary>
        /// <param name="polls">The polls.</param>
        private void InitializeDataSource()
        {
            string sql = @"SELECT PollID
                           FROM Poll WITH (NOLOCK) 
                           WHERE ExpiresOn >= getdate() and Published = 1 and Deleted = 0 
                           Order By CreatedOn desc, DisplayOrder, Name";

            int pollID = 0;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(sql, conn))
                {
                    while (rs.Read())
                    {
                        pollID = DB.RSFieldInt(rs, "PollID");
                        listOfPollID.Add(pollID);
                    }
                }
            }

            if (listOfPollID.Count > 0)
            {
                dtlPolls.DataSource = listOfPollID;
                dtlPolls.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the dtlPolls control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.DataListItemEventArgs"/> instance containing the event data.</param>
        protected void dtlPolls_ItemDataBound(object sender, System.Web.UI.WebControls.DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                PollControl pc = e.Item.FindControl("ctrlPoll") as PollControl;
                if (pc != null)
                {
                    pc.DataSource = Poll.Find(ThisCustomer.CustomerID, 0, 0, ThisCustomer.SkinID, listOfPollID[e.Item.ItemIndex], ThisCustomer.LocaleSetting, ThisCustomer.IsRegistered);
                }

                if (listOfPollID.Count == 1)
                {
                    Panel pnlPoll = e.Item.FindControl("pnlPoll") as Panel;

                    if (pnlPoll != null)
                    {
                        Unit width = new Unit("690px");
                        pnlPoll.Width = width;
                    }
                }
            }

        }

        /// <summary>
        /// Handles the PollVoteButtonClick event of the ctrlPoll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ctrlPoll_PollVoteButtonClick(object sender, EventArgs e)
        {
            PollControl ctrlPoll = sender as PollControl;
            if (ctrlPoll != null)
            {
                string pollAnswerID = HttpUtility.UrlEncode(ctrlPoll.SelectedValue);
                string pollID = HttpUtility.UrlEncode(ctrlPoll.PollID.ToString());

                string redirectUrl = string.Format("pollvote.aspx?PollID={0}&Poll_{0}={1}", pollID, pollAnswerID);
                Response.Redirect(redirectUrl);
            }
        }
	}
}
