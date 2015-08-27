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
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Enum used to determine if this is a normal bulk mailing or a newsletter
    /// BulkMailTypeEnum.EmailBlast = Records retrieved from the Customer Table
    /// BulkMailTypeEnum.NewsLetter = Records retrieved from the NewsLetter Table
    /// BulkMailTypeEnum.None = Used for sending single emails where properties are set manually
    /// </summary>
    public enum BulkMailTypeEnum
    {
        EmailBlast = 0,
        NewsLetter = 1,
        None = 9
    }

    #region EMail Class
    /// <summary>
    /// Class for generating mailing list objects used for sending 
    /// email blasts and newsletters
    /// </summary>
    public class EMail
    {
        #region Private variables

        private int m_RecipientID;
        private string m_EmailAddress;
        private string m_RecipientGuid;
        private string m_FirstName;
        private string m_LastName;
        private string m_MailContents;
        private string m_MailFooter;
        private string m_MailSubject;
        private string m_FromAddress;
        private string m_FromName;
        private string m_MailServer;
        private string m_StoreURL;
        private bool m_IncludeFooter;
        private bool m_LogMessage;
        private bool m_UseHTML;
        private BulkMailTypeEnum m_MailType;

        #endregion


        #region Constructors

        /// <summary>
        /// Instantiates a new intance of the EMail Class
        /// </summary>
        public EMail()
        {
            //Set default values from AppConfigs
            m_MailServer = AppLogic.AppConfig("MailMe_Server");
            m_FromAddress = AppLogic.AppConfig("MailMe_FromAddress");
            m_FromName = AppLogic.AppConfig("MailMe_FromeName");
            m_StoreURL = AppLogic.GetStoreHTTPLocation(false);

            m_UseHTML = true;
            m_IncludeFooter = true;
            m_LogMessage = false;

            m_MailType = BulkMailTypeEnum.None;
            m_MailFooter = String.Empty;
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Parses the MailContents property looking for tokens to replace
        /// </summary>
        /// <param name="contents">Contents to parse for replacement</param>
        /// <returns></returns>
        private void ReplaceTokens()
        {

            String[] replaceTokens = {@"%UnsubscribeLink%",
                                      @"%RemoveURL%",
                                      @"%FirstName%",
                                      @"%LastName%",
                                      @"%CompanyName%"};

            String newsletterUnsubscribe = String.Format("{0}NewsletterOptInOut.aspx?GUID={1}&opt=out", m_StoreURL, m_RecipientGuid);
            String mailingMgrUnsubscribe = String.Format("{0}remove.aspx?id={1}", m_StoreURL, m_RecipientGuid);


            foreach (String s in replaceTokens)
            {
                if (Regex.IsMatch(m_MailContents, s, RegexOptions.IgnoreCase)
                    || Regex.IsMatch(m_MailFooter, s, RegexOptions.IgnoreCase) 
                    || Regex.IsMatch(m_MailSubject, s, RegexOptions.IgnoreCase))
                {
                    String replacement = String.Empty;

                    if (s.EqualsIgnoreCase(@"%UnsubscribeLink%"))
                    {
                        replacement = newsletterUnsubscribe;
                    }
                    else if (s.EqualsIgnoreCase(@"%RemoveURL%"))
                    {
                        replacement = mailingMgrUnsubscribe;
                    }
                    else if (s.EqualsIgnoreCase(@"%FirstName%"))
                    {
                        replacement = m_FirstName;
                    }
                    else if (s.EqualsIgnoreCase(@"%LastName%"))
                    {
                        replacement = m_LastName;
                    }
                    else if (s.EqualsIgnoreCase(@"%CompanyName%"))
                    {
                        replacement = AppLogic.AppConfig("StoreName");
                    }

                    m_MailContents = Regex.Replace(m_MailContents, s, replacement, RegexOptions.IgnoreCase);
                    m_MailFooter = Regex.Replace(m_MailFooter, s, replacement, RegexOptions.IgnoreCase);
                    m_MailSubject = Regex.Replace(m_MailSubject, s, replacement, RegexOptions.IgnoreCase);

                }
            }
        }

        /// <summary>
        /// Writes the EMail object to the MailingMgrLog table
        /// </summary>
        private void LogEmailInMailingMgrLog()
        {
            StringBuilder contents = new StringBuilder(m_MailContents);

            if (m_IncludeFooter)
            {
                contents.AppendLine();
                contents.Append(m_MailFooter);
            }

            SqlParameter spSentOn = new SqlParameter("@SentOn", System.DateTime.Now);
            SqlParameter spToEmail = new SqlParameter("@ToEmail", m_EmailAddress);
            SqlParameter spFromEmail = new SqlParameter("@FromEmail", m_FromAddress);
            SqlParameter spSubject = new SqlParameter("@Subject", m_MailSubject);
            SqlParameter spBody = new SqlParameter("@Body", contents.ToString());

            SqlParameter[] spa = { spSentOn, spToEmail, spFromEmail, spSubject, spBody };

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                DB.ExecuteStoredProcVoid("[dbo].[aspdnsf_insMailingMgrLog]", spa, conn);

                conn.Close();
                conn.Dispose();
            }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Method that sends the EMail object once it has been properly configured
        /// </summary>
        public void Send()
        {
            if (String.IsNullOrEmpty(m_MailContents) || String.IsNullOrEmpty(m_MailSubject))
            {
                throw new ArgumentException("Error:  Cannot send broadcast emails without a subject or content.  Ensure that you have set both of these properties before calling the Send() method");
            }
            else
            {
                // Replace parser tokens before building the message body
                ReplaceTokens();

                StringBuilder messageBody = new StringBuilder(m_MailContents);

                if (m_IncludeFooter)
                {
                    messageBody.AppendLine();
                    messageBody.Append(m_MailFooter);
                }

                try
                {
                    AppLogic.SendMail(  m_MailSubject,          //Email Subject
                                        messageBody.ToString(), //Email Contents
                                        m_UseHTML,              //HTML or Text-Only
                                        m_FromAddress,          //Email From Address
                                        m_FromName,             //Email From Name
                                        m_EmailAddress,         //Email Address to send to
                                        this.FullName,          //Name to send to
                                        String.Empty,           //BCC Addresses
                                        m_MailServer);          //Mail Server

                    if (m_LogMessage)
                    {
                        LogEmailInMailingMgrLog();
                    }
                }
                catch (Exception ex)
                {
                    SysLog.LogException(ex, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                }
            }
        }

        #endregion


        #region Public properties

        /// <summary>
        /// Gets or sets the recipient's ID based on the mailing type.
        /// BulkMailTypeEnum.EmailBlast = Customer.CustomerID
        /// BulkMailTypeEnum.NewsLetter = Newsletter.ID
        /// </summary>
        public int RecipientID
        {
            get { return m_RecipientID; }
            set { m_RecipientID = value; }
        }

        /// <summary>
        /// Gets or sets the recipient's Address based on the mailing type
        /// BulkMailTypeEnum.EmailBlast = Customer.EMail
        /// BulkMailTypeEnum.NewsLetter = Newsletter.EMailAddress
        /// </summary>
        public string EmailAddress
        {
            get { return m_EmailAddress; }
            set { m_EmailAddress = value; }
        }

        /// <summary>
        /// Gets or sets the recipient's guid used to generate removal links
        /// BulkMailTypeEnum.EmailBlast = Customer.CustomerGUID
        /// BulkMailTypeEnum.NewsLetter = Newsletter.GUID
        /// </summary>
        public string RecipientGuid
        {
            get { return m_RecipientGuid; }
            set { m_RecipientGuid = value; }
        }

        /// <summary>
        /// Gets or sets the email recipient's First Name
        /// </summary>
        public string FirstName
        {
            get { return m_FirstName; }
            set { m_FirstName = value; }
        }

        /// <summary>
        /// Gets or sets the email recipient's Last Name
        /// </summary>
        public string LastName
        {
            get { return m_LastName; }
            set { m_LastName = value; }
        }

        /// <summary>
        /// Gets or sets the EMail Type
        /// EmailBlast retrieves records from the customer table
        /// NewsLetter retrieves records from the newsletter table
        /// </summary>
        public BulkMailTypeEnum MailType
        {
            get { return m_MailType; }
            set { m_MailType = value; }
        }

        /// <summary>
        /// Gets or sets the contents of the message to be sent
        /// </summary>
        public string MailContents
        {
            get { return m_MailContents; }
            set { m_MailContents = value; }
        }

        /// <summary>
        /// Gets or sets a value containing the email footer
        /// </summary>
        public string MailFooter
        {
            get { return m_MailFooter; }
            set { m_MailFooter = value; }
        }

        /// <summary>
        /// Gets or sets the subject of the message to be sent
        /// </summary>
        public string MailSubject
        {
            get { return m_MailSubject; }
            set { m_MailSubject = value; }
        }

        /// <summary>
        /// Gets a concatenated string of the recipient's FirstName and LastName
        /// </summary>
        public string FullName
        {
            get { return String.Format("{0} {1}", m_FirstName, m_LastName); }
        }

        /// <summary>
        /// Gets or sets the email address that the message is sent from
        /// </summary>
        public string FromAddress
        {
            get { return m_FromAddress; }
            set { m_FromAddress = value; }
        }

        /// <summary>
        /// Gets or sets the full name of the person or entity that the email comes from
        /// </summary>
        public string FromName
        {
            get { return m_FromName; }
            set { m_FromName = value; }
        }

        /// <summary>
        /// Gets or sets the mail server that the message is sent through
        /// </summary>
        public string MailServer
        {
            get { return m_MailServer; }
            set { m_MailServer = value; }
        }

        /// <summary>
        /// Gets a value containing the store URL.  Used to build removal links.
        /// </summary>
        public string StoreURL
        {
            get { return m_StoreURL; }
        }

        /// <summary>
        /// Gets or sets a value determining whether the footer is appended to the message contents
        /// </summary>
        public bool IncludeFooter
        {
            get { return m_IncludeFooter; }
            set {m_IncludeFooter = value;}
        }

        /// <summary>
        /// Gets or sets a value determining if the message is logged in the MailingMgrLog table
        /// </summary>
        public bool LogMessage
        {
            get { return m_LogMessage; }
            set { m_LogMessage = value; }
        }

        /// <summary>
        /// Gets or sets a property determining if the email is sent as HTML or plain text
        /// </summary>
        public bool UseHTML
        {
            get { return m_UseHTML; }
            set { m_UseHTML = value; }
        }

        #endregion


        #region Static methods

        /// <summary>
        /// Generic List of EMail objects that can be utilized to send mailings.
        /// !!!WARNING!!! Do NOT set the contents property on the entire list! This method can
        /// generate TENS OR HUNDREDS OF THOUSANDS of objects, consuming GIGABYTES OF MEMORY!
        /// Contents should be set individually in the routine that loops through the list to
        /// send the emails.
        /// </summary>
        /// <param name="listType">
        /// Determines whether the list is built off of the Customers or Newsletter table.
        /// EmailBlast = Customers, Newsletter = NewsLetter
        /// </param>
        /// <param name="withOrdersOnly">
        /// If BulkMailTypeEnum = EmailBlast, determines if only Customers with orders are returned
        /// </param>
        /// <param name="mailingSubject">
        /// The subject of the mailing that the list is being generated for.
        /// Used to check for duplicate mailings to the same customer
        /// </param>
        /// <returns>
        /// Generic List containing all mailing objects that meet the qualification parameters.
        /// </returns>
        public static List<EMail> MailingList(BulkMailTypeEnum listType, bool withOrdersOnly, string mailingSubject)
        {
            List<EMail> mailObjectList = new List<EMail>();

            SqlParameter spListType = new SqlParameter("@ListType", (int)listType);
            SqlParameter spWithOrdersOnly = new SqlParameter("@withOrdersOnly", (withOrdersOnly ? 1 : 0));
            SqlParameter spMailingSubject = new SqlParameter("@mailingSubject", mailingSubject);

            SqlParameter[] spa = { spListType, spWithOrdersOnly, spMailingSubject };

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.ExecuteStoredProcReader("[dbo].[aspdnsf_getMailingList]", spa, conn))
                {
                    while (rs.Read())
                    {
                        EMail objMailing = new EMail();
                        objMailing.RecipientID = DB.RSFieldInt(rs, "RecipientID");
                        objMailing.RecipientGuid = DB.RSFieldGUID(rs, "RecipientGuid");
                        objMailing.EmailAddress = DB.RSField(rs, "EmailAddress");
                        objMailing.FirstName = DB.RSField(rs, "FirstName");
                        objMailing.LastName = DB.RSField(rs, "LastName");

                        mailObjectList.Add(objMailing);
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return mailObjectList;
        }



        /// <summary>
        /// Accepts a mailing list object and returns a comma separated string
        /// </summary>
        /// <param name="mailingList">Mailing list object to export</param>
        /// <returns>Comma separated mailing list</returns>
        public static String ExportListToCSV(List<EMail> mailingList)
        {
            StringBuilder sb = new StringBuilder();

            //Write Headers
            sb.Append("ID,FirstName,LastName,EMailAddress");
            sb.AppendLine();

            //Write the file contents
            foreach (EMail em in mailingList)
            {
                sb.Append(em.RecipientID);
                sb.Append(",");
                sb.Append(em.FirstName);
                sb.Append(",");
                sb.Append(em.LastName);
                sb.Append(",");
                sb.Append(em.EmailAddress);
                sb.Append(",");
                sb.AppendLine();
            }

            return sb.ToString();
        }


        #endregion


    }


    public class NewsLetter
    {
        #region Private variables

        private int m_RecipientID;
        private string m_EmailAddress;
        private int m_StoreId;
        private string m_FirstName;
        private string m_LastName;
		private DateTime m_SubscribedOn;

        #endregion

        #region Public properties

		/// <summary>
		/// Gets or sets the recipient's ID based on the mailing type.
        /// BulkMailTypeEnum.EmailBlast = Customer.CustomerID
        /// BulkMailTypeEnum.NewsLetter = Newsletter.ID
        /// </summary>
        public int RecipientID
        {
            get { return m_RecipientID; }
            set { m_RecipientID = value; }
        }

        /// <summary>
        /// Gets or sets the email recipient's First Name
        /// </summary>
        public string FirstName
        {
            get { return m_FirstName; }
            set { m_FirstName = value; }
        }

        /// <summary>
        /// Gets or sets the email recipient's Last Name
        /// </summary>
        public string LastName
        {
            get { return m_LastName; }
            set { m_LastName = value; }
        }

        /// <summary>
        /// Gets or sets the recipient's Address based on the mailing type
        /// BulkMailTypeEnum.EmailBlast = Customer.EMail
        /// BulkMailTypeEnum.NewsLetter = Newsletter.EMailAddress
        /// </summary>
        public string EmailAddress
        {
            get { return m_EmailAddress; }
            set { m_EmailAddress = value; }
        }

        /// <summary>
        /// Gets or sets the StoreId based on the mailing type
        /// BulkMailTypeEnum.EmailBlast = Customer.EMail
        /// BulkMailTypeEnum.NewsLetter = Newsletter.EMailAddress
        /// </summary>
        public int StoreId
        {
            get { return m_StoreId; }
            set { m_StoreId = value; }
        }
		public DateTime SubscribedOn
		{
			get
			{ return this.m_SubscribedOn; }
			set
			{ this.m_SubscribedOn = value; }
		}
        #endregion

        public static List<NewsLetter> NewsLetterMailingList(bool withOrdersOnly)
        {
            BulkMailTypeEnum listType = BulkMailTypeEnum.NewsLetter;
            string mailingSubject = String.Empty;

            List<NewsLetter> mailObjectList = new List<NewsLetter>();

            SqlParameter spListType = new SqlParameter("@ListType", (int)listType);
            SqlParameter spWithOrdersOnly = new SqlParameter("@withOrdersOnly", (withOrdersOnly ? 1 : 0));
            SqlParameter spMailingSubject = new SqlParameter("@mailingSubject", mailingSubject);

            SqlParameter[] spa = { spListType, spWithOrdersOnly, spMailingSubject };

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.ExecuteStoredProcReader("[dbo].[aspdnsf_getMailingList]", spa, conn))
                {
                    while (rs.Read())
                    {
						var email = DB.RSField(rs, "EmailAddress");

						Customer customer = new Customer(email);

						if (!withOrdersOnly || (customer != null && customer.HasOrders()))
						{
							NewsLetter objMailing = new NewsLetter();
							objMailing.RecipientID = DB.RSFieldInt(rs, "RecipientID");
							objMailing.EmailAddress = email;
							objMailing.StoreId = DB.RSFieldInt(rs, "StoreId");
							objMailing.FirstName = DB.RSField(rs, "FirstName");
							objMailing.LastName = DB.RSField(rs, "LastName");
							objMailing.SubscribedOn = DB.RSFieldDateTime(rs, "SubscribedOn");
							mailObjectList.Add(objMailing);
						}
                    }

                    rs.Close();
                    rs.Dispose();
                }

                conn.Close();
                conn.Dispose();
            }

            return mailObjectList;
        }
    }
#endregion


    #region BulkMailing Class
    /// <summary>
    /// Public class for performing bulk mailings
    /// </summary>
    public class BulkMailing
    {
        /// <summary>
        /// Delegate method for executing asynchronous bulk mailings
        /// </summary>
        public delegate void ExecuteAsyncBulkSend();

        #region Private variables

        private List<EMail> m_mailingList;
        private string m_body;
        private string m_subject;
        private string m_sessionID;
        private string m_footer;
        private bool m_includeFooter;

        #endregion


        #region Constructors

        /// <summary>
        /// Instantiates a new instance of the bulk mailing class
        /// </summary>
        /// <param name="mailingList">List object containing email objects to send</param>
        /// <param name="body">Email body contents</param>
        /// <param name="subject">Email subject</param>
        /// <param name="includeFooter">Whether or not to include the contents of the footer topic</param>
        /// <param name="sessionID">Current session ID (used to track async progress)</param>
        public BulkMailing(List<EMail> mailingList, string body, string subject, string footer, bool includeFooter, string sessionID)
        {
            m_mailingList = mailingList;
            m_body = body;
            m_subject = subject;
            m_includeFooter = includeFooter;
            m_sessionID = sessionID;
            m_footer = footer;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Applies the contents and subjects and sends the EMail message to the recipient
        /// </summary>
        /// <returns></returns>
        public void ExecuteBulkSend()
        {
            String logDetail = String.Empty;
            int emailsSent = 0;
            DateTime startTime = System.DateTime.Now;

            //Create a blank record.  We will update it later on

            if (AsyncDataStore.RetrieveRecord(m_sessionID) == null)
            {
                AsyncDataStore.AddRecord(m_sessionID, string.Empty);
            }

            foreach (EMail em in m_mailingList)
            {
                try
                {
                    emailsSent++;

                    em.MailSubject = m_subject;
                    em.MailContents = m_body;
                    em.IncludeFooter = m_includeFooter;
                    if (m_includeFooter)
                    {
                        em.MailFooter = m_footer;
                    }
                    em.LogMessage = true;

                    em.Send();

                    //Drop the data into the async store so we can poll it later
                    AsyncDataStore.UpdateRecord(m_sessionID, String.Format("{0},{1},{2}", emailsSent.ToString(), m_mailingList.Count.ToString(), startTime.ToString()));
                }
                catch (Exception ex)
                {
                    SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Alert);
                }
            }

            logDetail = String.Format("Successfully sent {0} emails of {1}", emailsSent.ToString(), m_mailingList.Count.ToString());

            SysLog.LogMessage("Bulk Mailing Completed", logDetail, MessageTypeEnum.Informational, MessageSeverityEnum.Message);
        }

        #endregion


        #region Public properties

        /// <summary>
        /// Allows readonly access to the the underlying EMail Object List
        /// </summary>
        public List<EMail> MailingList
        {
            get { return m_mailingList; }
        }

        #endregion
    }

    #endregion

}
