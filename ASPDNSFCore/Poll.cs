// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Contains the Value and ID of PollAnswer
    /// </summary>
    public class PollAnswer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PollAnswer"/> class.
        /// </summary>
        public PollAnswer()
        {
        }

        private string m_value;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        private int m_id; 

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID of the PollAnswer.</value>
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private int m_thispercent;

        /// <summary>
        /// Gets the this percent.
        /// </summary>
        /// <value>The this percent.</value>
        public int ThisPercent
        {
            get { return m_thispercent; }
            set { m_thispercent = value; }
        }
    }

    /// <summary>
    /// This Poll object is used as datasource for PollControl.
    /// </summary>
    public class Poll
    {
        private string _name;
        private string _localeSetting;
        private int _sortOrder;
        private int _pollID;
        private int _skinID;
        private bool _hasCustomerVoted;

        /// <summary>
        /// Gets the name of the poll.
        /// </summary>
        /// <value>The name of the poll.</value>
        public string Name
        {
            get { return this._name; }
        }

        /// <summary>
        /// Gets the poll ID.
        /// </summary>
        /// <value>The poll ID.</value>
        public int PollID
        {
            get { return this._pollID; }
        }

        /// <summary>
        /// Gets the skin ID.
        /// </summary>
        /// <value>The skin ID.</value>
        public int SkinID
        {
            get { return this._skinID; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has customer voted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has customer voted; otherwise, <c>false</c>.
        /// </value>
        public bool HasCustomerVoted
        {
            get { return this._hasCustomerVoted; }
        }

        private List<PollAnswer> answers = new List<PollAnswer>();

        /// <summary>
        /// Gets the poll answers.
        /// </summary>
        /// <value>The poll answers.</value>
        public List<PollAnswer> pollAnswers
        {
            get
            {
                return this.answers;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Poll"/> class.
        /// </summary>
        /// <param name="pollID">The pollID.</param>
        /// <param name="skinID">The skinID.</param>
        /// <param name="localeSetting">The localeSetting.</param>
        public Poll(int pollID, int skinID, string localeSetting)
            : this(pollID, skinID, localeSetting, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Poll"/> class.
        /// </summary>
        /// <param name="pollID">The poll ID.</param>
        /// <param name="skinID">The skin ID.</param>
        /// <param name="localeSetting">The locale setting.</param>
        /// <param name="customerID">The customer ID.</param>
        public Poll(int pollID, int skinID, string localeSetting, int customerID)
        {
            this._skinID = skinID;
            this._pollID = pollID;
            this._name = String.Empty;
            this._sortOrder = 3;
            this._localeSetting = localeSetting;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("SELECT * FROM Poll   with (NOLOCK)  where PollID=" + pollID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        this._name = DB.RSFieldByLocale(rs, "Name", this._localeSetting);
                        this._sortOrder = DB.RSFieldInt(rs, "PollSortOrderID");
                    }
                }
            }

            if (pollID != 0)
            {
                this.GetPollAnswers(customerID);
            }
        }

        /// <summary>
        /// This is use for the datasource of the PollControl
        /// </summary>
        /// <param name="customerID">The customer ID.</param>
        /// <param name="categoryID">The category ID.</param>
        /// <param name="sectionID">The section ID.</param>
        /// <param name="skinId">The skin id.</param>
        /// <param name="pollID">The poll ID.</param>
        /// <param name="localeSetting">The locale setting.</param>
        /// <param name="isRegistered">if set to <c>true</c> [is registered].</param>
        /// <returns>Return null if no poll records found.</returns>
        public static Poll Find(int customerID, int categoryID, int sectionID, int skinId, int pollID, string localeSetting, bool isRegistered)
        {
            string sql = string.Empty;

            bool isRegister = isRegistered;
            bool showPoll = AppLogic.AppConfigBool("Polls.Enabled");

            if (!showPoll)
            {
                return null;
            }

            // polls are assigned to specific categories or sections via Admin > Misc > More > Manage Polls
            // they are not displayed on non-category or non-section pages.
            if (categoryID == 0 && sectionID == 0 && pollID == 0)
            {
                return null;
            }

            if (pollID == 0)
            {
                SqlParameter[] spa = { new SqlParameter("@isRegistered", isRegistered),
                                       new SqlParameter("@CategoryID", categoryID),
                                       new SqlParameter("@SectionID", sectionID),
                                       new SqlParameter("@CustomerID", customerID),
                                       new SqlParameter("@StoreID", AppLogic.StoreID())
                                     };

                if (categoryID != 0 && sectionID == 0)
                {
                    sql = "SELECT p.PollID FROM poll p WITH (NOLOCK) LEFT JOIN pollcategory pc WITH (NOLOCK) ON p.pollid = pc.pollid WHERE (p.AnonsCanVote=1 OR @isRegistered=1) AND p.published=1 AND p.ExpiresOn>=getDate() AND p.deleted=0 AND pc.CategoryID=@CategoryID AND p.pollid NOT IN (SELECT DISTINCT pollid FROM pollvotingrecord WITH (NOLOCK) WHERE customerid=@CustomerID) AND p.pollid IN (SELECT PollID FROM PollStore WHERE StoreID=@StoreID) ORDER BY p.CreatedOn DESC, p.DisplayOrder, p.Name";
                }
                else if (sectionID != 0 && categoryID == 0)
                {
                    sql = "SELECT p.PollID FROM poll p with (NOLOCK) LEFT JOIN pollsection ps WITH (NOLOCK) ON p.pollid = ps.pollid WHERE (p.AnonsCanVote=1 OR @isRegistered=1) AND p.published=1 AND p.ExpiresOn>=getDate() AND p.deleted=0 AND ps.SectionID=@SectionID AND p.pollid NOT IN (SELECT DISTINCT pollid FROM pollvotingrecord WITH (NOLOCK) WHERE customerid=@CustomerID) AND p.pollid IN (SELECT PollID FROM PollStore WHERE StoreID=@StoreID) ORDER BY p.CreatedOn DESC, p.DisplayOrder, p.Name";
                }

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(sql, spa, con))
                    {
                        if (rs.Read())
                        {
                            pollID = DB.RSFieldInt(rs, "PollID");
                        }
                    }
                }
            }

            // now try to find ANY active poll, voted on or not!
            if (pollID == 0)
            {
                SqlParameter[] spa = { new SqlParameter("@isRegistered", isRegistered),
                                       new SqlParameter("@CategoryID", categoryID),
                                       new SqlParameter("@SectionID", sectionID),   
                                       new SqlParameter("@StoreID", AppLogic.StoreID())
                                     };

                if (categoryID != 0 && sectionID == 0)
                {
                    sql = "SELECT p.PollID FROM poll p with (NOLOCK) left join pollcategory pc with (NOLOCK) on p.pollid = pc.pollid  where (p.AnonsCanVote=1 OR @isRegistered=1) and p.published=1 and p.ExpiresOn>=getdate() and deleted=0 and pc.pollid is null and pc.CategoryID=@CategoryID AND p.pollid IN (SELECT PollID FROM PollStore WHERE StoreID=@StoreID)";
                }
                else if (sectionID != 0 &&
                          categoryID == 0)
                {
                    sql = "SELECT p.PollID FROM poll p with (NOLOCK)  left join pollsection ps with (NOLOCK) on p.pollid = ps.pollid  where (p.AnonsCanVote=1 OR @isRegistered=1) and p.published=1 and p.ExpiresOn>=getdate() and deleted=0 and ps.pollid is null and ps.SectionID=@SectionID AND p.pollid IN (SELECT PollID FROM PollStore WHERE StoreID=@StoreID)";
                }

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS(sql, spa, con))
                    {
                        if (rs.Read())
                        {
                            pollID = DB.RSFieldInt(rs, "PollID");
                        }
                    }
                }
            }

            if (pollID != 0)
            {
                Poll p = new Poll(pollID, skinId, localeSetting, customerID);
                return p;
            }

            return null;
        }

        /// <summary>
        /// Records the vote.
        /// </summary>
        /// <param name="customerID">The CustomerID.</param>
        /// <param name="pollAnswerID">The PollAnswerID.</param>
        public void RecordVote(int customerID, int pollAnswerID)
        {
            if (!this.CustomerHasVoted(customerID))
            {
                DB.ExecuteSQL("insert into PollVotingRecord(PollID,PollAnswerID,CustomerID) values(" + this._pollID.ToString() + "," + pollAnswerID.ToString() + "," + customerID.ToString() + ")");
            }
        }

        /// <summary>
        /// Determines whether this Poll is expired.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is expired; otherwise, <c>false</c>.
        /// </returns>
        public bool IsExpired()
        {
            return DB.GetSqlN("select count(*) as N from Poll   with (NOLOCK)  where PollID=" + this._pollID.ToString() + " and ExpiresOn<" + DB.DateQuote(Localization.ToDBShortDateString(System.DateTime.Now))) > 0;
        }

        [Obsolete("Replace by user control PollControl in AspDotNetStorefrontControls.")]
        public string Display(int customerID, bool showPollsLink)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            if (!this.CustomerHasVoted(customerID))
            {
                tmpS.Append("<form method=\"POST\" action=\"pollvote.aspx\" name=\"Poll" + this._pollID.ToString() + "Form\" id=\"Poll" + this._pollID.ToString() + "Form\">");
                tmpS.Append("<input type=\"hidden\" name=\"PollID\" value=\"" + this._pollID.ToString() + "\">");
                tmpS.Append("<span class=\"PollTitle\">" + this._name + CommonLogic.IIF(this.IsExpired(), " " + AppLogic.GetString("poll.cs.1", this._skinID, Thread.CurrentThread.CurrentUICulture.Name), string.Empty) + "</span>");

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select * from PollAnswer   with (NOLOCK)  where deleted=0 and PollID=" + this._pollID.ToString() + " order by DisplayOrder,Name", dbconn))
                    {
                        while (rs.Read())
                        {
                            tmpS.Append("<input class=\"PollRadio\" type=\"radio\" value=\"" + DB.RSFieldInt(rs, "PollAnswerID").ToString() + "\" name=\"Poll_" + this._pollID.ToString() + "\"><span class=\"PollAnswer\">" + DB.RSFieldByLocale(rs, "Name", this._localeSetting) + "</span>");
                        }
                    }
                }

                tmpS.Append("<div align=\"center\"><input class=\"PollSubmit\" type=\"submit\" value=\"Vote\" name=\"B1\"></div>");
                tmpS.Append("</form>");
            }
            else
            {
                tmpS.Append("<span class=\"PollTitle\">" + this._name + CommonLogic.IIF(this.IsExpired(), " (Not Active)", string.Empty) + "</span>");
                tmpS.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">");
                string sql = "SELECT TOP 100 PERCENT Poll.PollID, PollAnswer.Name, PollAnswer.PollAnswerID, COUNT(PollVotingRecord.PollAnswerID) AS NumVotes, PollAnswer.DisplayOrder FROM (PollAnswer   with (NOLOCK)  INNER JOIN Poll   with (NOLOCK)  ON PollAnswer.PollID = Poll.PollID) LEFT OUTER JOIN PollVotingRecord   with (NOLOCK)  ON PollAnswer.PollID = PollVotingRecord.PollID AND PollAnswer.PollAnswerID = PollVotingRecord.PollAnswerID GROUP BY Poll.PollID, PollAnswer.Name, PollAnswer.PollAnswerID, PollAnswer.DisplayOrder HAVING (Poll.PollID = " + this._pollID.ToString() + ") ";
                switch (this._sortOrder)
                {
                    case 1:
                        // As Written
                        sql = sql + " Order By PollAnswer.PollAnswerID";
                        break;
                    case 2:
                        // Ascending
                        sql = sql + " ORDER BY NumVotes ASC, PollAnswer.PollAnswerID";
                        break;
                    case 3:
                        // Descending
                        sql = sql + " ORDER BY NumVotes DESC, PollAnswer.PollAnswerID";
                        break;
                }

                int NV = this.NumVotes();

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(sql, dbconn))
                    {
                        while (rs.Read())
                        {
                            int answerNumVotes = DB.RSFieldInt(rs, "NumVotes");
                            int thisPercent = (int)((decimal)answerNumVotes / (decimal)NV * 100.0M);
                            tmpS.Append("<tr>");
                            tmpS.Append("<td width=\"40%\" align=\"right\" valign=\"middle\"><span class=\"PollAnswer\">" + DB.RSFieldByLocale(rs, "Name", this._localeSetting) + ":&nbsp;</span></td>");
                            tmpS.Append("<td width=\"60%\" align=\"left\" valign=\"middle\"><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/skin_" + this._skinID.ToString() + "/images/pollimage.gif") + "\" align=\"absmiddle\" width=\"" + ((int)(thisPercent * 0.9)).ToString() + "%\" height=\"10\" border=\"0\"><span class=\"PollAnswer\"> (" + thisPercent.ToString() + "%)</span></td>");
                            tmpS.Append("</tr>");
                            tmpS.Append("<tr><td colspan=\"2\"><img src=\"images/spacer.gif\" width=\"100%\" height=\"2\"></td></tr>");
                        }
                    }
                }

                tmpS.Append("</table>");
                tmpS.Append("  <div align=\"center\"><span class=\"PollLink\">" + AppLogic.GetString("poll.cs.2", this._skinID, Thread.CurrentThread.CurrentUICulture.Name) + " " + NV.ToString() + "</span></div>");
                if (showPollsLink)
                {
                    tmpS.Append("  <div align=\"center\"><a class=\"PollLink\" href=\"polls.aspx\">" + AppLogic.GetString("poll.cs.3", this._skinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a></div>");
                }
            }

            return tmpS.ToString();
        }

        /// <summary>
        /// The total number of votes.
        /// </summary>
        /// <returns>total votes.</returns>
        public int NumVotes()
        {
            return DB.GetSqlN("SELECT COUNT(*) AS N FROM PollVotingRecord   WITH (NOLOCK)  WHERE Pollanswerid IN (SELECT DISTINCT Pollanswerid FROM Pollanswer   WITH (NOLOCK)  WHERE Deleted=0) AND PollID=" + this._pollID.ToString());
        }

        /// <summary>
        /// Determine if the specified CustomerID has voted.
        /// </summary>
        /// <param name="CustomerID">The CustomerID.</param>
        /// <returns>True or False</returns>
        public bool CustomerHasVoted(int customerID)
        {
            this._hasCustomerVoted = DB.GetSqlN("SELECT COUNT(*) as N FROM PollVotingRecord   WITH (NOLOCK)  WHERE PollID=" + this._pollID.ToString() + " AND customerID=" + customerID.ToString()) != 0;
            return this._hasCustomerVoted;
        }

        /// <summary>
        /// Gets the poll answers.
        /// </summary>
        /// <param name="customerID">The customer ID.</param>
        private void GetPollAnswers(int customerID)
        {
            string sql = string.Empty;

            if (!this.CustomerHasVoted(customerID))
            {
                sql = "SELECT * FROM PollAnswer   with (NOLOCK)  where deleted=0 and PollID=" + this._pollID.ToString() + " order by DisplayOrder,Name";
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(sql, dbconn))
                    {
                        while (rs.Read())
                        {
                            PollAnswer polAns = new PollAnswer();
                            polAns.Value = DB.RSFieldByLocale(rs, "Name", this._localeSetting);
                            polAns.ID = DB.RSFieldInt(rs, "PollAnswerID");
                            this.answers.Add(polAns);
                        }
                    }
                }
            }
            else
            {
                sql = "SELECT TOP 100 PERCENT Poll.PollID, PollAnswer.Name, PollAnswer.PollAnswerID, COUNT(PollVotingRecord.PollAnswerID) AS NumVotes, PollAnswer.DisplayOrder FROM (PollAnswer   with (NOLOCK)  INNER JOIN Poll   with (NOLOCK)  ON PollAnswer.PollID = Poll.PollID) LEFT OUTER JOIN PollVotingRecord   with (NOLOCK)  ON PollAnswer.PollID = PollVotingRecord.PollID AND PollAnswer.PollAnswerID = PollVotingRecord.PollAnswerID GROUP BY Poll.PollID, PollAnswer.Name, PollAnswer.PollAnswerID, PollAnswer.DisplayOrder HAVING (Poll.PollID = " + this._pollID.ToString() + ") ";

                switch (this._sortOrder)
                {
                    case 1:
                        // As Written
                        sql = sql + " Order By PollAnswer.PollAnswerID";
                        break;
                    case 2:
                        // Ascending
                        sql = sql + " ORDER BY NumVotes ASC, PollAnswer.PollAnswerID";
                        break;
                    case 3:
                        // Descending
                        sql = sql + " ORDER BY NumVotes DESC, PollAnswer.PollAnswerID";
                        break;
                }

                int NV = this.NumVotes();

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(sql, dbconn))
                    {
                        while (rs.Read())
                        {
                            PollAnswer polAns = new PollAnswer();
                            int numvotes = DB.RSFieldInt(rs, "NumVotes");
                            polAns.ThisPercent = (int)((decimal)numvotes / (Decimal)NV * 100.0M);
                            polAns.Value = DB.RSFieldByLocale(rs, "Name", this._localeSetting);
                            polAns.ID = DB.RSFieldInt(rs, "PollAnswerID");
                            this.answers.Add(polAns);
                        }
                    }
                }
            }
        }
    }
}
