// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

	/// <summary>
	/// Summary description for mailingmgr
	/// </summary>
	public partial class mailingmgr : AdminPageBase
	{
		#region Private variables

		#endregion

		#region Events

		/// <summary>
		/// Default Page Load Event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");
			Server.ScriptTimeout = 50000;
			SectionTitle = "EMail Manager";

			//Images in mailings must have Absolute URLs
			radDescription.EnableFilter(Telerik.Web.UI.EditorFilters.MakeUrlsAbsolute);
			radDescription.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);

			if (!IsPostBack)
			{
				bool processing = false;

				//Check to see if we have any mailing manager jobs running
				if (AsyncDataStore.RetrieveRecord(Session.SessionID) != null)
				{
					//The data store DOES contain records - lets see if it is completed
					try
					{
						String[] Status = AsyncDataStore.RetrieveRecord(Session.SessionID).ToString().Split(',');

						if (Status[0] != Status[1])
						{
							//Not completed.  Show status, and disable buttons.
							ifrStatus.Visible = true;
							ifrStatus.Attributes["src"] = "asyncstatus.aspx?id=" + Session.SessionID;
							ltError.Text = AppLogic.GetString("admin.mailingmgr.BulkMailSending", ThisCustomer.LocaleSetting);

							btnRemoveEmail.Enabled = false;
							btnSend.Enabled = false;

							processing = true;
						}
					}
					catch { }
				}

				if (!processing && CommonLogic.QueryStringNativeInt("completed") == 1)
				{
					//Page reloaded due to having completed processing a bulk send

					ltError.Text = AppLogic.GetString("admin.asyncstatus.Complete", ThisCustomer.LocaleSetting);
					divStatus.Attributes["class"] = "noticeMsg";
				}

			}
		}


		/// <summary>
		/// Send one test email to admin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void btnSendTestEmailToAdmin_Click(object sender, EventArgs e)
		{
			//Send a single message to the currently logged in user
			string mailSubject = txtSubject.Text;
			string mailBody = radDescription.Content;

			EMail objEmail = new EMail();

			objEmail.RecipientID = ThisCustomer.CustomerID;
			objEmail.RecipientGuid = ThisCustomer.CustomerGUID;
			objEmail.EmailAddress = ThisCustomer.EMail;
			objEmail.MailSubject = mailSubject;
			objEmail.MailContents = mailBody;
			objEmail.MailFooter = (new Topic("mailfooter").Contents);
			objEmail.IncludeFooter = true;
			objEmail.LogMessage = true;

			objEmail.Send();
		}
		public void btnExport_Click(object sender, EventArgs e)
		{
			bool withOrdersOnly = chkCustomersWithOrdersOnly.Checked;
			bool newsLetter = rbCustomerListType.SelectedValue.Equals("Newsletter");

			Export(withOrdersOnly, newsLetter);
		}

		/// <summary>
		/// Handles the send button OnClick event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnSend_Click(object sender, EventArgs e)
		{
			bool withOrdersOnly = chkCustomersWithOrdersOnly.Checked;
			bool newsLetter = rbCustomerListType.SelectedValue.EqualsIgnoreCase("Newsletter");

			Send(withOrdersOnly, newsLetter);
		}

		private void Send(bool optionCustomersWithOrders, bool optionNewsletterSubscribers)
		{
			string mailSubject = txtSubject.Text;
			string mailBody = radDescription.Content;
			string mailFooter = new Topic("mailfooter").Contents;
			List<EMail> mailingList = new List<EMail>();

            if (optionNewsletterSubscribers)
				mailingList = EMail.MailingList(BulkMailTypeEnum.NewsLetter, optionCustomersWithOrders, mailSubject);
			else
				mailingList = EMail.MailingList(BulkMailTypeEnum.EmailBlast, optionCustomersWithOrders, mailSubject);

			BulkMailing bm = new BulkMailing(mailingList, mailBody, mailSubject, mailFooter, true, Session.SessionID);

			if (bm.MailingList.Count > 0)
			{
				BulkMailing.ExecuteAsyncBulkSend executeAsyncSend = new BulkMailing.ExecuteAsyncBulkSend(bm.ExecuteBulkSend);

				//We will check status in case the operation finishes and we need to re-enable buttons, etc.
				AsyncCallback cb = new AsyncCallback(MailingComplete);

				executeAsyncSend.BeginInvoke(cb, null);

				ifrStatus.Attributes["src"] = "asyncstatus.aspx?id=" + Session.SessionID;
				ifrStatus.Visible = true;

				ltError.Text = AppLogic.GetString("admin.mailingmgr.BulkMailSending", ThisCustomer.LocaleSetting);

				btnRemoveEmail.Enabled = false;
				btnSend.Enabled = false;
			}
			else
			{
				btnRemoveEmail.Enabled = true;
				btnSend.Enabled = true;

				ifrStatus.Visible = false;

				ltError.Text = AppLogic.GetString("admin.mailingmgr.NoEmails", ThisCustomer.LocaleSetting);
			}
		}

		private void Export(bool optionCustomersWithOrders, bool optionNewsletterSubscribers)
		{
			List<object> exportList = new List<object>();

            if (optionNewsletterSubscribers)
			{
				List<NewsLetter> newsletterMailingList = NewsLetter.NewsLetterMailingList(optionCustomersWithOrders);
				exportList = newsletterMailingList.ConvertAll<object>(delegate(NewsLetter g) { return (object)g; });
			}
			else
			{
				List<GridCustomer> customerList = new List<GridCustomer>();

				if (optionCustomersWithOrders) // filter to customers only with orders
				{
					var allCustomers = GridCustomer.GetCustomers();

					foreach (var c in allCustomers)
					{
						Customer customer = new Customer(c.CustomerID);
						if (customer.HasOrders())
							customerList.Add(c);
					}
				}
				else // return all customers
				{
					customerList = GridCustomer.GetCustomers();
				}

				exportList = customerList.ConvertAll<object>(delegate(GridCustomer g) { return (object)g; });
			}

			StringBuilder strMailingList = new StringBuilder();

			Response.Clear();
			Response.ClearHeaders();
			Response.ClearContent();
			Response.AddHeader("content-disposition", "attachment; filename=MailingList.csv");
			Response.ContentType = "text/csv";
			Response.AddHeader("Pragma", "public");

			strMailingList.Append(CSVExporter.ExportListToCSV(exportList));

			Response.Write(strMailingList.ToString());
			Response.End();

		}

		/// <summary>
		/// Sets the provided customer's OkToEmail flag to false
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnRemoveEmail_Click(object sender, EventArgs e)
		{
			DB.ExecuteSQL("update customer set OKToEMail=0 where EMail=" + DB.SQuote(txtRemoveEmail.Text));
		}

		#endregion

		public void MailingComplete(IAsyncResult ar)
		{
			BulkMailing.ExecuteAsyncBulkSend executeAsyncSend = (BulkMailing.ExecuteAsyncBulkSend)((AsyncResult)ar).AsyncDelegate;
			executeAsyncSend.EndInvoke(ar);
		}

	}
}
