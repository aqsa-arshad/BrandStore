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
using System.Linq;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefrontAdmin
{

    public partial class editgiftcard : AdminPageBase
    {
        protected string selectSQL = "SELECT G.*, C.FirstName, C.LastName from GiftCard G with (NOLOCK) LEFT OUTER JOIN Customer C with (NOLOCK) ON G.PurchasedByCustomerID = C.CustomerID ";
        private Customer cust;
        private int GiftCardID;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            GiftCardID = CommonLogic.QueryStringNativeInt("iden");
            lnkUsage.NavigateUrl = "" + AppLogic.AdminLinkUrl("giftcardusage.aspx") + "?iden=" + GiftCardID;

            if (GiftCardID == 0)
            {
                OrderNumberRow.Visible = false;
                RemainingBalanceRow.Visible = false;
                ltAmount.Visible = false;
                PurchasedByCustomerIDLiteralRow.Visible = false;
                GiftCardTypeDisplayRow.Visible = false;
                InitialAmountLiteralRow.Visible = false;
                PurchasedByCustomerIDTextRow.Visible = true;
                reqCustEmail.Enabled = true;
            }
            else
            {
                txtAmount.Visible = false; // cannot change after first created
                PurchasedByCustomerIDTextRow.Visible = false;
                reqCustEmail.Enabled = false;
                GiftCardTypeSelectRow.Visible = false;
                InitialAmountTextRow.Visible = false;
            }

            if (!IsPostBack)
            {
                ltScript.Text = ("\n<script type=\"text/javascript\">\n");
                ltScript.Text += ("    Calendar.setup({\n");
                ltScript.Text += ("        inputField     :    \"" + txtDate.ClientID + "\",      // id of the input field\n");
                ltScript.Text += ("        ifFormat       :    \"" + Localization.JSCalendarDateFormatSpec() + "\",       // format of the input field\n");
                ltScript.Text += ("        showsTime      :    false,            // will display a time selector\n");
                ltScript.Text += ("        button         :    \"f_trigger_s\",   // trigger for the calendar (button ID)\n");
                ltScript.Text += ("        singleClick    :    true            // double-click mode\n");
                ltScript.Text += ("    });\n");
                ltScript.Text += ("</script>\n");

                ltDate.Text = "<img class=\"actionelement\" src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/calendar.gif") + "\" align=\"absmiddle\" id=\"f_trigger_s\">&nbsp;<small>(" + Localization.ShortDateFormat() + ")</small>";

                ltStyles.Text = ("  <!-- calendar stylesheet -->\n");
                ltStyles.Text += ("  <link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"jscalendar/calendar-win2k-cold-1.css\" title=\"win2k-cold-1\" />\n");
                ltStyles.Text += ("\n");
                ltStyles.Text += ("  <!-- main calendar program -->\n");
                ltStyles.Text += ("  <script type=\"text/javascript\" src=\"jscalendar/calendar.js\"></script>\n");
                ltStyles.Text += ("\n");
                ltStyles.Text += ("  <!-- language for the calendar -->\n");
                ltStyles.Text += ("  <script type=\"text/javascript\" src=\"jscalendar/lang/" + Localization.JSCalendarLanguageFile() + "\"></script>\n");
                ltStyles.Text += ("\n");
                ltStyles.Text += ("  <!-- the following script defines the Calendar.setup helper function, which makes\n");
                ltStyles.Text += ("       adding a calendar a matter of 1 or 2 lines of code. -->\n");
                ltStyles.Text += ("  <script type=\"text/javascript\" src=\"jscalendar/calendar-setup.js\"></script>\n");

                if (GiftCardID > 0)
                {
                    trEmail.Visible = false;
                    ltCard.Text = DB.GetSqlS("SELECT SerialNumber AS S FROM GiftCard with (NOLOCK) WHERE GiftCardID=" + CommonLogic.QueryStringCanBeDangerousContent("iden"));
                    btnSubmit.Text = AppLogic.GetString("admin.editgiftcard.UpdateGiftCard", SkinID, LocaleSetting);

                    loadData();

                    if (CommonLogic.QueryStringNativeInt("added") == 1)
                    {
                        resetError(AppLogic.GetString("admin.editgiftcard.GiftCardAdded", SkinID, LocaleSetting), false);
                    }
                    else if (CommonLogic.QueryStringNativeInt("added") == 2)
                    {
                        resetError(AppLogic.GetString("admin.editgiftcard.GiftCardUpdated", SkinID, LocaleSetting), false);
                    }
                    else if (CommonLogic.QueryStringNativeInt("added") == 3)
                    {
                        resetError(AppLogic.GetString("giftcard.email.error.1", cust.SkinID, cust.LocaleSetting), true);
                    }
                    if (etsMapper.ObjectID != GiftCardID)
                    {
                        etsMapper.ObjectID = GiftCardID;
                        etsMapper.DataBind();
                    }
                }
                else
                {
                    lblAction.Visible = false;
                    rblAction.Visible = false;
                    ltCurrentBalance.Text = "NA";
                    trEmail.Visible = true;
                    ltCard.Text = AppLogic.GetString("admin.editgiftcard.NewGiftCard", SkinID, LocaleSetting);
                    btnSubmit.Text = AppLogic.GetString("admin.editgiftcard.AddGiftCard", SkinID, LocaleSetting);
                    rblAction.SelectedIndex = 0;

                    XmlPackage2 iData = new XmlPackage2("giftcardassignment.xml.config");
                    System.Xml.XmlDocument xd = iData.XmlDataDocument;
                    txtSerial.Text = xd.SelectSingleNode("/root/GiftCardAssignment/row/CardNumber").InnerText;
                    txtDate.Text = DateTime.Now.AddYears(1).ToShortDateString();
                } 
            }

            litStoreMapper.Visible = etsMapper.StoreCount > 1;
            litStoreMapperHdr.Visible = etsMapper.StoreCount > 1;
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }

        protected void loadData()
        {
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("SELECT * FROM GiftCard  with (NOLOCK)  WHERE GiftCardID=" + GiftCardID, dbconn))
                {
                    if (!rs.Read())
                    {
                        rs.Close();
                        resetError("Unable to retrieve data.", true);
                        return;
                    }

                    txtSerial.Text = Server.HtmlEncode(DB.RSField(rs, "SerialNumber"));
                    ltCustomerID.Text = DB.RSFieldInt(rs, "PurchasedByCustomerID").ToString();

                    if (DB.RSFieldInt(rs, "PurchasedByCustomerID") != 0)
                    {
                        ltCustomer2.Text = String.Format(AppLogic.GetString("admin.editgiftcard.CustomerName", SkinID, LocaleSetting), DB.GetSqlS("SELECT (LastName + ', ' + FirstName) AS S FROM Customer with (NOLOCK) WHERE CustomerID=" + DB.RSFieldInt(rs, "PurchasedByCustomerID").ToString()));
                    }
                    else
                    {
                        ltCustomer2.Text = AppLogic.GetString("admin.editgiftcard.NACustomer", SkinID, LocaleSetting);
                    }

                    txtOrder.Text = Server.HtmlEncode(DB.RSFieldInt(rs, "OrderNumber").ToString());
                    txtDate.Text = Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs, "ExpirationDate"));

                    txtAmount.Text = Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "InitialAmount"));
                    ltAmount.Text = Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "InitialAmount"));
                    ltCurrentBalance.Text = Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "Balance"));

                    ddType.Items.FindByValue(DB.RSFieldInt(rs, "GiftCardTypeID").ToString()).Selected = true;
                    ltGiftCardType.Text = ((GiftCardTypes)DB.RSFieldInt(rs, "GiftCardTypeID")).ToString();

                    rblAction.ClearSelection();
                    rblAction.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "DisabledByAdministrator"), 1, 0);
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                resetError("", false);

                StringBuilder sql = new StringBuilder(1024);

                //validate for the email type on insert for emailing
                int type = Localization.ParseNativeInt(ddType.SelectedValue);
                if ((type == 101) && (GiftCardID == 0))
                {
                    if ((txtEmailBody.Text.Length == 0) || (txtEmailName.Text.Length == 0) || (txtEmailTo.Text.Length == 0))
                    {
                        resetError(AppLogic.GetString("admin.editgiftcard.EnterEmailPreferences", SkinID, LocaleSetting), true);
                        return;
                    }
                }
				//validate customer id if creating giftcard  (ID = 0 is new giftcard)
				int customerId = Localization.ParseNativeInt(hdnCustomerId.Value);
				if (GiftCardID == 0 && customerId == 0)
				{
					resetError(AppLogic.GetString("admin.editgiftcard.InvalidEmail", cust.SkinID, cust.LocaleSetting), true);
					return;
				}

                //make sure the customer has set up their email properly
                if (type == 101 && GiftCardID == 0 && (AppLogic.AppConfig("MailMe_Server").Length == 0 || AppLogic.AppConfig("MailMe_FromAddress") == "sales@yourdomain.com"))
                {
                    //Customer has not configured their MailMe AppConfigs yet
                    resetError(AppLogic.GetString("giftcard.email.error.2", cust.SkinID, cust.LocaleSetting), true);
                    return;
                }

                if (GiftCardID == 0)
                {
                    //insert a new card

                    //check if valid SN
                    int N = DB.GetSqlN("select count(GiftCardID) as N from GiftCard   with (NOLOCK)  where lower(SerialNumber)=" + DB.SQuote(txtSerial.Text.ToLowerInvariant().Trim()));
                    if (N != 0)
                    {
                        resetError(AppLogic.GetString("admin.editgiftcard.ExistingGiftCard", SkinID, LocaleSetting), true);
                        return;
                    }

                    //ok to add them
					GiftCard card = GiftCard.CreateGiftCard(customerId,
										txtSerial.Text,
										Localization.ParseNativeInt(txtOrder.Text),
										0,
										0,
										0,
										Localization.ParseNativeDecimal(txtAmount.Text),
										txtDate.Text,
										Localization.ParseNativeDecimal(txtAmount.Text),
										ddType.SelectedValue,
										CommonLogic.Left(txtEmailName.Text, 100),
										CommonLogic.Left(txtEmailTo.Text, 100),
										txtEmailBody.Text,
										null,
										null,
										null,
										null,
										null,
										null);
					GiftCardID = card.GiftCardID;

                    try
                    {
						card.SendGiftCardEmail();
                    }
                    catch
                    {
                        //reload page, but inform the admin the the email could not be sent
                        Response.Redirect(AppLogic.AdminLinkUrl("editgiftcard.aspx") + "?iden=" + GiftCardID + "&added=3");
                    }

                    //reload page
                    etsMapper.ObjectID = GiftCardID;
                    etsMapper.Save();
                    Response.Redirect(AppLogic.AdminLinkUrl("editgiftcard.aspx") + "?iden=" + GiftCardID + "&added=1");
                }
                else
                {
                    //update existing card

                    //check if valid SN
                    int N = DB.GetSqlN("select count(GiftCardID) as N from GiftCard   with (NOLOCK)  where GiftCardID<>" + GiftCardID.ToString() + " and lower(SerialNumber)=" + DB.SQuote(txtSerial.Text.ToLowerInvariant().Trim()));
                    if (N != 0)
                    {
                        resetError(AppLogic.GetString("admin.editgiftcard.ExistingGiftCard", SkinID, LocaleSetting), true);
                        return;
                    }

                    //ok to update
                    sql.Append("UPDATE GiftCard SET ");
                    sql.Append("SerialNumber=" + DB.SQuote(txtSerial.Text) + ",");
                    sql.Append("ExpirationDate=" + DB.SQuote(Localization.ToDBShortDateString(Localization.ParseNativeDateTime(txtDate.Text))) + ",");
                    sql.Append("DisabledByAdministrator=" + Localization.ParseNativeInt(rblAction.SelectedValue));
                    sql.Append(" WHERE GiftCardID=" + GiftCardID);
                    DB.ExecuteSQL(sql.ToString());

                    etsMapper.ObjectID = GiftCardID;
                    etsMapper.Save();
                    //reload page
                    Response.Redirect(AppLogic.AdminLinkUrl("editgiftcard.aspx") + "?iden=" + GiftCardID + "&added=2");
                }
                etsMapper.Save();
            }
        }


		[System.Web.Services.WebMethodAttribute(), System.Web.Script.Services.ScriptMethodAttribute()]
		public static string[] GetCompletionList(string prefixText, int count, string contextKey)
		{
			using (SqlConnection dbconn = DB.dbConn())
			{
				dbconn.Open();
				SqlParameter[] spa = { new SqlParameter("@prefixText", prefixText + '%'), new SqlParameter("@count", count) };
				using (IDataReader rsCustomer = DB.GetRS("SELECT TOP (@count) CustomerId, Email FROM Customer with (NOLOCK) where Email <> '' AND Email like @prefixText OR FirstName like @prefixText OR LastName like @prefixText ORDER BY CustomerId", spa, dbconn))
				{
					IList<string> txtCustomers = new List<string>();
					String customerItem;

					using (DataTable dtCustomer = new DataTable())
					{
						dtCustomer.Columns.Add("CustomerId");
						dtCustomer.Columns.Add("Email");
						dtCustomer.Load(rsCustomer);
						foreach (DataRow row in dtCustomer.Rows)
						{
							customerItem = AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(DB.RowField(row, "Email"), DB.RowField(row, "CustomerID"));
							txtCustomers.Add(customerItem);
						}
						return txtCustomers.ToArray();
					}
				}
			}
		}
    }
}
