// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for orderframe.
    /// </summary>
    public partial class orderframe : AdminPageBase
    {
        bool _shipRushEnabledAndConfigured = false;
        protected void Page_Load(object sender, System.EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
            String SubmitAction = CommonLogic.FormCanBeDangerousContent("SubmitAction").ToUpperInvariant();
            Order ord = new Order(OrderNumber, Localization.GetDefaultLocale());
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select * from orders  with (NOLOCK)  where ordernumber=" + OrderNumber.ToString(), dbconn))
                {
                    if (!rs.Read())
                    {  // if order does not exist set to zero
                        OrderNumber = 0;
                    }
                    int SkinID = 1;
                    if (AppLogic.AppConfigBool("ShipRush.Enabled"))
                    {
                        // look for status back from shiprush
                        try
                        {
                            using (SqlConnection dbconn2 = DB.dbConn())
                            {
                                dbconn2.Open();
                                using (IDataReader rsJobHistory = DB.GetRS("select * from OR_JOBHISTORY where (TrackingNumber IS NOT NULL and TrackingNumber<>'') and JobID in (select 'OrderNumber_' + convert(varchar(10),OrderNumber) as JobID from orders  with (NOLOCK)  where ShippingTrackingNumber=" + DB.SQuote("Pending From ShipRush") + ")", dbconn2))
                                {
                                    //this is just a flag if shiprush table is already configure
                                    _shipRushEnabledAndConfigured = true;
                                    while (rsJobHistory.Read())
                                    {
                                        String TN = DB.RSField(rsJobHistory, "TrackingNumber").Trim();
                                        String TNotes = DB.RSField(rsJobHistory, "Notes").Trim();
                                        String JobID = DB.RSField(rsJobHistory, "JobID").Trim();
                                        String ON = String.Empty;
                                        try
                                        {
                                            ON = JobID.Split('_')[1].Trim();
                                            if (ON.Length != 0)
                                            {
                                                DB.ExecuteSQL("update orders set CarrierReportedRate=" + DB.SQuote(TNotes) + ", ShippingTrackingNumber=" + DB.SQuote(TN) + " where ShippingTrackingNumber=" + DB.SQuote("Pending From ShipRush") + " and OrderNumber=" + ON);
                                            }
                                        }
                                        catch
                                        { }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            _shipRushEnabledAndConfigured = false;
                        }
                    }

                    if (AppLogic.AppConfigBool("FedExShipManager.Enabled"))
                    {
                        // look for status back from shipmanager

                        using (SqlConnection dbconn3 = DB.dbConn())
                        {
                            dbconn3.Open();
                            using (IDataReader rsfedex = DB.GetRS("SELECT * FROM ShippingImportExport WHERE (TrackingNumber IS NOT NULL and TrackingNumber <> '') ", dbconn3))
                            {
                                while (rsfedex.Read())
                                {
                                    string tracking = DB.RSField(rsfedex, "TrackingNumber").Trim();
                                    string shippedVia = CommonLogic.IIF(DB.RSField(rsfedex, "ServiceCarrierCode").Length != 0, DB.RSField(rsfedex, "ServiceCarrierCode"), AppLogic.GetString("order.cs.1", SkinID, LocaleSetting));
                                    decimal cost = DB.RSFieldDecimal(rsfedex, "Cost");
                                    decimal weight = DB.RSFieldDecimal(rsfedex, "Weight");
                                    int ordno = DB.RSFieldInt(rsfedex, "OrderNumber");

                                    try
                                    {
                                        //send confirmation before we put the price in shippedVia
                                        Order.MarkOrderAsShipped(ordno, shippedVia, tracking, System.DateTime.Now, false, null, new Parser(null, 1, ThisCustomer), !AppLogic.AppConfigBool("BulkImportSendsShipmentNotifications"));
                                        //Update Orders
                                        DB.ExecuteSQL("UPDATE Orders SET ShippedVia=" + DB.SQuote(shippedVia + "|" + cost) + ", CarrierReportedWeight=" + DB.SQuote(weight.ToString()) + ", CarrierReportedRate=" + DB.SQuote(cost.ToString()) + ", ShippingTrackingNumber=" + DB.SQuote(tracking) + " WHERE OrderNumber=" + ordno);
                                        //Delete from FedEx synch table
                                        DB.ExecuteSQL("DELETE FROM ShippingImportExport WHERE OrderNumber=" + ordno);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    String bgcolor = "FFFFFF";
                    String LastIPAddress = String.Empty;
                    if (OrderNumber != 0)
                    {
                        if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateFraud)
                        {
                            bgcolor = "ffbcbc";
                        }
                        if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateVoided)
                        {
                            bgcolor = "ebebeb";
                        }
                        if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateForceVoided)
                        {
                            bgcolor = "ebebeb";
                        }
                        if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateRefunded)
                        {
                            bgcolor = "fffcd0";
                        }
                        if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateAuthorized)
                        {
                            bgcolor = "C2DAFC";
                        }
                        if (DB.RSField(rs, "TransactionState") == AppLogic.ro_TXStateCaptured)
                        {
                            bgcolor = "C2DAFC";
                        }
                        LastIPAddress = DB.RSField(rs, "LastIPAddress");
                    }
                    sb.Append("<body bgcolor=\"#" + bgcolor + "\" topmargin=\"0\" marginheight=\"0\" bottommargin=\"0\" marginwidth=\"0\" rightmargin=\"0\">\n");
                    sb.Append("<script type=\"text/javascript\" src=\"./scripts/formValidate.js\"></script>\n");

                    String InitialTab = CommonLogic.QueryStringCanBeDangerousContent("InitialTab");
                    if (InitialTab.Length == 0)
                    {
                        InitialTab = "General";
                    }
                    EntityHelper AffiliateHelper = new EntityHelper(EntityDefinitions.LookupSpecs("Affiliate"), 0);

                    if (!ThisCustomer.IsAdminUser)
                    {
                        sb.Append("<p><b>" + AppLogic.GetString("admin.common.InsufficientPermissions", SkinID, LocaleSetting) + "</b></p>");
                    }
                    else
                    {
                        String Status = String.Empty;

                        if (SubmitAction.Length != 0)
                        {
                            if (SubmitAction == "GETMAXMIND")
                            {
                                // get MaxMind info if not already done (e.g. PayPal or Google checkout, etc)
                                try
                                {
                                    if (ord.OrderNumber != 0 && ord.MaxMindFraudScore == -1 && AppLogic.AppConfigBool("MaxMind.Enabled"))
                                    {
                                        String FraudDetails = String.Empty;
                                        Address billingAddress = new Address();
                                        billingAddress.LoadByCustomer(ord.CustomerID, AddressTypes.Billing);
                                        Address shippingAddress = new Address();
                                        shippingAddress.LoadByCustomer(ord.CustomerID, AddressTypes.Shipping);
                                        Customer customer = new Customer(ord.CustomerID, true);
                                        Decimal FraudScore = Gateway.MaxMindFraudCheck(ord.OrderNumber
                                        , customer
                                        , billingAddress
                                        , shippingAddress
                                        , ord.Total(true)
                                        , customer.CurrencySetting
                                        , ord.PaymentMethod
                                        , out FraudDetails);

                                        DB.ExecuteSQL(String.Format("update orders set MaxMindFraudScore={0}, MaxMindDetails={1} where OrderNumber={2}", Localization.DecimalStringForDB(FraudScore), DB.SQuote(FraudDetails), ord.OrderNumber.ToString()));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DB.ExecuteSQL(String.Format("update orders set MaxMindFraudScore={0}, MaxMindDetails={1} where OrderNumber={2}", -1.0M, DB.SQuote(ex.Message), ord.OrderNumber.ToString()));
                                }
                            }

                            if (SubmitAction == "UPDATENOTES")
                            {
                                Status = Gateway.OrderManagement_SetPrivateNotes(ord, LocaleSetting, CommonLogic.FormCanBeDangerousContent("Notes"));
                                Status = Gateway.OrderManagement_SetCustomerServiceNotes(ord, LocaleSetting, CommonLogic.FormCanBeDangerousContent("CustomerServiceNotes"));
                                InitialTab = "Notes";
                            }

                            if (SubmitAction == "CLEARNEW")
                            {
                                Status = Gateway.OrderManagement_ClearNewStatus(ord, LocaleSetting);
                            }

                            if (SubmitAction == "DOWNLOADEXPORTXML")
                            {
                                Status = Gateway.OrderManagement_MarkAsExported(ord, LocaleSetting);
                                InitialTab = "General";
                            }

                            if (SubmitAction == "MARKREADYTOSHIP")
                            {
                                Status = Gateway.OrderManagement_MarkAsReadyToShip(ord, LocaleSetting);
                                InitialTab = "General";
                            }

                            if (SubmitAction == "CLEARREADYTOSHIP")
                            {
                                Status = Gateway.OrderManagement_ClearReadyToShip(ord, LocaleSetting);
                                InitialTab = "General";
                            }

                            if (SubmitAction == "ADJUSTORDERWEIGHT")
                            {
                                Status = Gateway.OrderManagement_SetOrderWeight(ord, LocaleSetting, CommonLogic.FormNativeDecimal("OrderWeight"));
                                InitialTab = "General";
                            }

                            if (SubmitAction == "MARKASPRINTED")
                            {
                                Status = Gateway.OrderManagement_MarkAsPrinted(ord, LocaleSetting);
                            }

                            if (SubmitAction == "SENDTRACKING")
                            {
                                String ShippedVIA = DB.RSField(rs, "ShippedVIA");
                                String ShippingTrackingNumber = DB.RSField(rs, "ShippingTrackingNumber");
                                if (CommonLogic.FormCanBeDangerousContent("ShippedVIA").Trim().Length != 0)
                                {
                                    ShippedVIA = CommonLogic.FormCanBeDangerousContent("ShippedVIA").Trim(", ".ToCharArray());
                                }

                                if (CommonLogic.FormCanBeDangerousContent("ShippingTrackingNumber").Trim().Length != 0)
                                {
                                    ShippingTrackingNumber = CommonLogic.FormCanBeDangerousContent("ShippingTrackingNumber").Trim(",".ToCharArray());
                                }
                                Status = Gateway.OrderManagement_SetTracking(ord, LocaleSetting, ShippedVIA, ShippingTrackingNumber);
                            }

                            if (SubmitAction == "MARKASSHIPPED")
                            {
                                AppLogic.eventHandler("OrderShipped").CallEvent("&OrderShipped=true&OrderNumber=" + OrderNumber.ToString());

                                String ShippedVIA = DB.RSField(rs, "ShippedVIA");
                                String ShippingTrackingNumber = DB.RSField(rs, "ShippingTrackingNumber");
                                if (CommonLogic.FormCanBeDangerousContent("ShippedVIA").Trim().Length != 0)
                                {
                                    ShippedVIA = CommonLogic.FormCanBeDangerousContent("ShippedVIA").Trim(", ".ToCharArray());
                                }

                                if (CommonLogic.FormCanBeDangerousContent("ShippingTrackingNumber").Trim().Length != 0)
                                {
                                    ShippingTrackingNumber = CommonLogic.FormCanBeDangerousContent("ShippingTrackingNumber").Trim(",".ToCharArray());
                                }
                                String ShippedOnString = CommonLogic.FormCanBeDangerousContent("ShippedOn").Trim(",".ToCharArray());
                                DateTime ShippedOn = System.DateTime.Now;
                                if (ShippedOnString.Length != 0)
                                {
                                    ShippedOn = Localization.ParseNativeDateTime(ShippedOnString);
                                }
                                if (ShippedOn == System.DateTime.MinValue)
                                {
                                    ShippedOn = System.DateTime.Now;
                                }
                                Status = Gateway.OrderManagement_MarkAsShipped(ord, LocaleSetting, ShippedVIA, ShippingTrackingNumber, ShippedOn);
                                InitialTab = "Shipping";
                            }

                            if (SubmitAction == "CHANGEORDEREMAIL")
                            {
                                String NewEMail = CommonLogic.FormCanBeDangerousContent("NewEMail").Trim().ToLowerInvariant();
                                Status = Gateway.OrderManagement_ChangeOrderEMail(ord, LocaleSetting, NewEMail);
                                InitialTab = "Customer";
                            }

                            if (SubmitAction == "MARKASFRAUD")
                            {
                                Status = Gateway.OrderManagement_MarkAsFraud(ord, LocaleSetting);
                            }

                            if (SubmitAction == "CLEARASFRAUD")
                            {
                                Status = Gateway.OrderManagement_ClearFraud(ord, LocaleSetting);
                            }

                            if (SubmitAction == "BLOCKIP")
                            {
                                Status = Gateway.OrderManagement_BlockIP(ord, LocaleSetting);
                            }

                            if (SubmitAction == "ALLOWIP")
                            {
                                Status = Gateway.OrderManagement_AllowIP(ord, LocaleSetting);
                            }

                            if (_shipRushEnabledAndConfigured)
                            {
                                if (SubmitAction == "SHIPRUSH")
                                {
                                    Status = Gateway.OrderManagement_SendToShipRush(ord, LocaleSetting, ThisCustomer);
                                }
                            }

                            if (AppLogic.AppConfigBool("FedExShipManager.Enabled"))
                            {
                                if (SubmitAction == "FEDEXSHIPMANAGER")
                                {
                                    Status = Gateway.OrderManagement_SendToFedexShippingMgr(ord, LocaleSetting);
                                }
                            }

                            if (SubmitAction == "FORCEREFUND")
                            {
                                Status = Gateway.OrderManagement_DoForceFullRefund(ord, LocaleSetting);
                            }

                            if (SubmitAction == "SENDRECEIPTEMAIL")
                            {
                                Status = Gateway.OrderManagement_SendReceipt(ord, LocaleSetting);
                                InitialTab = "General";
                            }

                            if (SubmitAction == "REGENERATERECEIPT")
                            {
                                Status = ord.RegenerateReceipt(new Customer(ord.CustomerID));
                                InitialTab = "General";
                            }

                            if (SubmitAction == "SENDDISTRIBUTOREMAIL")
                            {
                                Status = Gateway.OrderManagement_SendDistributorNotification(ord, LocaleSetting);
                                if (Status != AppLogic.ro_OK)
                                {
                                    sb.Append("<p><b>");
                                    sb.Append(Status);
                                    sb.Append("<p></b>");
                                }
                                InitialTab = "General";
                            }
                        }
                        if (SubmitAction.Contains("RELEASEITEM"))
                        {
                            string idString = SubmitAction.Replace("RELEASEITEM", string.Empty);

                            if (idString.Length > 0)
                            {
                                int itemId = 0;
                                if (int.TryParse(idString, out itemId))
                                {
                                    DownloadItem downloadItem = new DownloadItem();
                                    downloadItem.Load(itemId);


                                    var locationFromQueryString = CommonLogic.FormCanBeDangerousContent(string.Format("DownloadLocation{0}", idString));
                                    if (locationFromQueryString != null && locationFromQueryString.Length > 0)
                                    {
                                        var location = (locationFromQueryString.Contains("http:") || locationFromQueryString.Contains("https:")) ? locationFromQueryString : string.Format("../{0}", locationFromQueryString.ToString());
                                        downloadItem.UpdateDownloadLocation(location);
                                    }

                                    downloadItem.Release(true);
                                    downloadItem.SendDownloadEmailNotification();
                                }
                            }
                            InitialTab = "General";
                        }

                        if (SubmitAction.Length != 0)
                        {
                            // reload order to get latest state after submit actions could have changed it
                            ord = new Order(OrderNumber, Localization.GetDefaultLocale());
                        }

                        if (ord.OrderNumber == 0 || ord.IsEmpty)
                        {
                            sb.Append("<p><b>" + AppLogic.GetString("admin.orderframe.OrderNotFoundOrOrderHasBeenDeleted", SkinID, LocaleSetting) + "</b></p>");
                        }
                        else
                        {

                            sb.Append("  <!-- calendar stylesheet -->\n");
                            sb.Append("  <link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"jscalendar/calendar-win2k-cold-1.css\" title=\"win2k-cold-1\" />\n");
                            sb.Append("\n");
                            sb.Append("  <!-- main calendar program -->\n");
                            sb.Append("  <script type=\"text/javascript\" src=\"jscalendar/calendar.js\"></script>\n");
                            sb.Append("\n");
                            sb.Append("  <!-- language for the calendar -->\n");
                            sb.Append("  <script type=\"text/javascript\" src=\"jscalendar/lang/" + Localization.JSCalendarLanguageFile() + "\"></script>\n");
                            sb.Append("\n");
                            sb.Append("  <!-- the following script defines the Calendar.setup helper function, which makes\n");
                            sb.Append("       adding a calendar a matter of 1 or 2 lines of code. -->\n");
                            sb.Append("  <script type=\"text/javascript\" src=\"jscalendar/calendar-setup.js\"></script>\n");

                            // Close and re-read Order data since we probably modified it above. 

                            String GW;
                            GatewayProcessor GWActual;
                            String PM;
                            bool IsPayPal, IsAmazonSimplePay;
                            String ShippingStatus = String.Empty;

                            using (SqlConnection dbconn4 = DB.dbConn())
                            {
                                dbconn4.Open();
                                using (IDataReader rs2 = DB.GetRS("Select * from orders  with (NOLOCK)  where ordernumber=" + OrderNumber.ToString(), dbconn4))
                                {
                                    rs2.Read();

                                    String TransactionState = DB.RSField(rs2, "TransactionState").Trim().ToUpper(CultureInfo.InvariantCulture);
                                    AppLogic.TransactionTypeEnum TransactionType = (AppLogic.TransactionTypeEnum)DB.RSFieldInt(rs2, "TransactionType");
                                    DateTime AuthorizedOn = DB.RSFieldDateTime(rs2, "AuthorizedOn");
                                    DateTime CapturedOn = DB.RSFieldDateTime(rs2, "CapturedOn");
                                    DateTime VoidedOn = DB.RSFieldDateTime(rs2, "VoidedOn");
                                    DateTime FraudedOn = DB.RSFieldDateTime(rs2, "FraudedOn");
                                    DateTime RefundedOn = DB.RSFieldDateTime(rs2, "RefundedOn");
                                    bool IsNew = DB.RSFieldBool(rs2, "IsNew");
                                    bool HasShippableComponents = AppLogic.OrderHasShippableComponents(DB.RSFieldInt(rs2, "OrderNumber"));
                                    GW = AppLogic.CleanPaymentGateway(DB.RSField(rs2, "PaymentGateway"));
                                    GWActual = GatewayLoader.GetProcessor(GW);
                                    Boolean GWSupportsPostProcessingEdits = GWActual == null || GWActual.SupportsPostProcessingEdits();
                                    Boolean GWSupportsAdHocOrders = GWActual == null || GWActual.SupportsAdHocOrders();


                                    PM = AppLogic.CleanPaymentMethod(DB.RSField(rs2, "PaymentMethod"));

                                    bool IsCard = (PM == AppLogic.ro_PMCreditCard);
                                    bool IsCheck = (PM == AppLogic.ro_PMCheckByMail || PM.IndexOf(AppLogic.ro_PMCOD) != -1);
                                    bool IsEcheck = (PM == AppLogic.ro_PMECheck);
                                    bool IsMicroPay = (PM == AppLogic.ro_PMMicropay);
                                    IsPayPal = (PM == AppLogic.ro_PMPayPal || PM == AppLogic.ro_PMPayPalExpress || GW == Gateway.ro_GWPAYPALPRO);
                                    IsAmazonSimplePay = (PM == AppLogic.ro_PMAmazonSimplePay || GW == Gateway.ro_GWAMAZONSIMPLEPAY);
                                    bool IsCOD = (PM == AppLogic.ro_PMCOD || PM == AppLogic.ro_PMCODCompanyCheck || PM == AppLogic.ro_PMCODMoneyOrder || PM == AppLogic.ro_PMCODNet30);
                                    Customer OrderCustomer = new Customer(ord.CustomerID);

                                    String ShipAddr = String.Empty;

                                    if (AppLogic.AppConfigBool("AllowMultipleShippingAddressPerOrder") && ord.HasMultipleShippingAddresses())
                                    {
                                        ShipAddr = AppLogic.GetString("checkoutreview.aspx.25", SkinID, LocaleSetting);
                                    }
                                    else
                                    {
                                        ShipAddr = (ord.ShippingAddress.m_FirstName + " " + ord.ShippingAddress.m_LastName).Trim();

                                        if (!string.IsNullOrEmpty(ord.ShippingAddress.m_Company))
                                        {
                                            ShipAddr += "<br />" + ord.ShippingAddress.m_Company;
                                        }

                                        ShipAddr += "<br />" + ord.ShippingAddress.m_Address1;
                                        if (ord.ShippingAddress.m_Address2.Length != 0)
                                        {
                                            ShipAddr += "<br/>" + ord.ShippingAddress.m_Address2;
                                        }
                                        if (ord.ShippingAddress.m_Suite.Length != 0)
                                        {
                                            ShipAddr += ", " + ord.ShippingAddress.m_Suite;
                                        }
                                        ShipAddr += "<br/>" + ord.ShippingAddress.m_City + ", " + ord.ShippingAddress.m_State + " " + ord.ShippingAddress.m_Zip;
                                        ShipAddr += "<br/>" + ord.ShippingAddress.m_Country.ToUpperInvariant();
                                        ShipAddr += "<br/>" + ord.ShippingAddress.m_Phone;
                                    }

                                    String BillAddr = (ord.BillingAddress.m_FirstName + " " + ord.BillingAddress.m_LastName).Trim();
                                    if (!string.IsNullOrEmpty(ord.BillingAddress.m_Company))
                                    {
                                        BillAddr += "<br />" + ord.BillingAddress.m_Company;
                                    }
                                    BillAddr += "<br />" + ord.BillingAddress.m_Address1;
                                    if (ord.BillingAddress.m_Address2.Length != 0)
                                    {
                                        BillAddr += "<br/>" + ord.BillingAddress.m_Address2;
                                    }
                                    if (ord.BillingAddress.m_Suite.Length != 0)
                                    {
                                        BillAddr += ", " + ord.BillingAddress.m_Suite;
                                    }
                                    BillAddr += "<br/>" + ord.BillingAddress.m_City + ", " + ord.BillingAddress.m_State + " " + ord.BillingAddress.m_Zip;
                                    BillAddr += "<br/>" + ord.BillingAddress.m_Country.ToUpperInvariant();
                                    BillAddr += "<br/>" + ord.BillingAddress.m_Phone;

                                    String ReceiptURLPrintable = "../receipt.aspx?ordernumber=" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "&customerid=" + DB.RSFieldInt(rs2, "CustomerID").ToString();

                                    sb.Append("<b>Order # " + OrderNumber.ToString());
                                    if (IsNew)
                                    {
                                        sb.Append("&nbsp;<img class=\"actionelement\" alt=\"" + AppLogic.GetString("admin.orderframe.ClearIsNewFlag", SkinID, LocaleSetting) + "\" onClick=\"ClearNew(OrderDetailForm," + OrderNumber.ToString() + "," + CommonLogic.IIF(ord.TransactionIsCaptured(), "1", "0") + ");\" src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/new.gif") + "\" align=\"absmiddle\" border=\"0\"></a>");
                                    }

                                    if (!ord.HasBeenEdited && (TransactionState != AppLogic.ro_TXStateFraud && TransactionType == AppLogic.TransactionTypeEnum.CHARGE))
                                    {
                                        sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" " + CommonLogic.IIF(Customer.StaticIsAdminSuperUser(ord.CustomerID), " disabled ", "") + " type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.MarkAsFraud", SkinID, LocaleSetting) + "\" name=\"Fraud" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"MarkAsFraudOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                    }
                                    if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateFraud && TransactionType == AppLogic.TransactionTypeEnum.CHARGE))
                                    {
                                        sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ClearFraudFlag", SkinID, LocaleSetting) + "\" name=\"ClearFraud" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ClearAsFraudOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                    }

                                    Customer TargetCustomer = new Customer(ord.CustomerID, true);

                                    if (ord.HasBeenEdited)
                                    {
                                        if (GWSupportsPostProcessingEdits)
                                        {
                                            sb.Append("&nbsp;" + AppLogic.GetString("admin.orderframe.EditedSeeOrder", SkinID, LocaleSetting) + "<a href=\"" + AppLogic.AdminLinkUrl("orderframe.aspx") + "?ordernumber=" + ord.RelatedOrderNumber.ToString() + "\">" + ord.RelatedOrderNumber.ToString() + "</a>");
                                        }
                                    }
                                    else if (AppLogic.AppConfigBool("OrderEditingEnabled") && ord.IsEditable() && !TargetCustomer.IsAdminSuperUser && !TargetCustomer.IsAdminUser)
                                    {
                                        sb.Append("&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Edit", SkinID, LocaleSetting) + "\" name=\"Edit" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"EditOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                    }
                                    else if (AppLogic.AppConfigBool("OrderEditingEnabled") && ord.IsEditable() && (TargetCustomer.IsAdminSuperUser || TargetCustomer.IsAdminUser))
                                    {
                                        sb.Append("&nbsp;&nbsp;<input disabled  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Edit", SkinID, LocaleSetting) + "\" name=\"Edit" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"EditOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                        sb.Append("<b>" + AppLogic.GetString("admin.orderframe.CustomerIsAdmin", SkinID, LocaleSetting) + "</b>&nbsp;&nbsp;");
                                    }

                                    sb.Append("&nbsp;&nbsp;" + String.Format(AppLogic.GetString("admin.orderframe.Type", SkinID, LocaleSetting), TransactionType.ToString()));
                                    sb.Append("&nbsp;&nbsp;" + String.Format(AppLogic.GetString("admin.orderframe.State", SkinID, LocaleSetting), TransactionState));

                                    //This will notify admin to set-up ShipRush server DB tables into the storefront database.
                                    if (AppLogic.AppConfigBool("ShipRush.Enabled") && !_shipRushEnabledAndConfigured)
                                    {
                                        sb.Append("&nbsp;&nbsp;<br><b><Font color= red size = 3>" + AppLogic.GetString("admin.ShipRushWarningMessage", SkinID, LocaleSetting) + "</Font></b>");
                                    }

                                    if (TransactionState == AppLogic.ro_TXStateRefunded && ord.RefundTXCommand.IndexOf("FORCED", StringComparison.InvariantCultureIgnoreCase) != -1)
                                    {
                                        sb.Append("&nbsp;(" + ord.RefundTXCommand + ")");
                                    }

                                    // NOTE: if "edited" turn off all orderframe actions. Just allow viewing of the original receipt. and that's it.

                                    sb.Append("<form method=\"POST\" action=\"" + AppLogic.AdminLinkUrl("orderframe.aspx") + "?ordernumber=" + OrderNumber.ToString() + "\" id=\"OrderDetailForm\" name=\"OrderDetailForm\" >");
                                    sb.Append("<input type=\"hidden\" name=\"SubmitAction\" value=\"\">\n");

                                    sb.Append("\n<script type=\"text/javascript\">\n");
                                    sb.Append("function PopupTX(ordernumber)\n");
                                    sb.Append("{\n");
                                    sb.Append("window.open('" + AppLogic.AdminLinkUrl("popuptx.aspx") + "?ordernumber=' + ordernumber,'PopupTX" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=500,left=0,top=0');\n");
                                    sb.Append("return (true);\n");
                                    sb.Append("}\n");
                                    sb.Append("</script>\n");

                                    sb.Append("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
                                    sb.Append("<tr>\n");
                                    sb.Append("<td id=\"GeneralTD_" + OrderNumber.ToString() + "\" width=90 height=22 align=\"center\" valign=\"bottom\"><a href=\"javascript:void(0);\" onClick=\"ShowGeneralDiv_" + OrderNumber.ToString() + "();\" >" + AppLogic.GetString("admin.orderframe.General", SkinID, LocaleSetting) + "</a><br/><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" height=\"2\" width=\"2\" border=\"0\"></td>\n");
                                    sb.Append("<td id=\"BillingTD_" + OrderNumber.ToString() + "\" width=90 height=22 align=\"center\" valign=\"bottom\"><a href=\"javascript:void(0);\" onClick=\"ShowBillingDiv_" + OrderNumber.ToString() + "();\" >" + AppLogic.GetString("admin.orderframe.Billing", SkinID, LocaleSetting) + "</a><br/><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" height=\"2\" width=\"2\" border=\"0\"></td>\n");
                                    sb.Append("<td id=\"ShippingTD_" + OrderNumber.ToString() + "\" width=90 height=22 align=\"center\" valign=\"bottom\"><a href=\"javascript:void(0);\" onClick=\"ShowShippingDiv_" + OrderNumber.ToString() + "();\" >" + AppLogic.GetString("admin.common.Shipping", SkinID, LocaleSetting) + "</a><br/><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" height=\"2\" width=\"2\" border=\"0\"></td>\n");
                                    sb.Append("<td id=\"CustomerTD_" + OrderNumber.ToString() + "\" width=90 height=22 align=\"center\" valign=\"bottom\"><a href=\"javascript:void(0);\" onClick=\"ShowCustomerDiv_" + OrderNumber.ToString() + "();\" >" + AppLogic.GetString("admin.common.Customer", SkinID, LocaleSetting) + "</a><br/><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" height=\"2\" width=\"2\" border=\"0\"></td>\n");
                                    sb.Append("<td id=\"NotesTD_" + OrderNumber.ToString() + "\" width=90 height=22 align=\"center\" valign=\"bottom\"><a href=\"javascript:void(0);\" onClick=\"ShowNotesDiv_" + OrderNumber.ToString() + "();\" >" + AppLogic.GetString("admin.orderframe.Notes", SkinID, LocaleSetting) + "</a><br/><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" height=\"2\" width=\"2\" border=\"0\"></td>\n");
                                    sb.Append("<td id=\"ReceiptTD_" + OrderNumber.ToString() + "\" width=90 height=22 align=\"center\" valign=\"bottom\"><a href=\"javascript:void(0);\" onClick=\"ShowReceiptDiv_" + OrderNumber.ToString() + "();\" >" + AppLogic.GetString("admin.common.Receipt", SkinID, LocaleSetting) + "</a><br/><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" height=\"2\" width=\"2\" border=\"0\"></td>\n");
                                    if (ThisCustomer.IsAdminSuperUser)
                                    {
                                        sb.Append("<td id=\"XMLTD_" + OrderNumber.ToString() + "\" width=90 height=22 align=\"center\" valign=\"bottom\"><a href=\"javascript:void(0);\" onClick=\"ShowXMLDiv_" + OrderNumber.ToString() + "();\" >" + AppLogic.GetString("admin.exportProductPricing.XML", SkinID, LocaleSetting) + "</a><br/><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" height=\"2\" width=\"2\" border=\"0\"></td>\n");
                                    }
                                    if (ord.HasDistributorComponents() && TransactionType == AppLogic.TransactionTypeEnum.CHARGE)
                                    {
                                        sb.Append("<td id=\"DistributorTD_" + OrderNumber.ToString() + "\" width=90 height=22 align=\"center\" valign=\"bottom\"><a href=\"javascript:void(0);\" onClick=\"ShowDistributorDiv_" + OrderNumber.ToString() + "();\" >" + AppLogic.GetString("admin.common.Distributor", SkinID, LocaleSetting) + "</a><br/><img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/spacer.gif") + "\" height=\"2\" width=\"2\" border=\"0\"></td>\n");
                                    }
                                    sb.Append("<td width=\"*\"></td>\n");
                                    sb.Append("</tr>\n");
                                    sb.Append("<td bgcolor=\"#" + bgcolor + "\" colspan=\"" + CommonLogic.IIF(ThisCustomer.IsAdminSuperUser, "7", "6") + "\" align=\"left\" valign=\"top\">\n");


                                    // --------------------------------------------------------------------------------------------------
                                    // GENERAL DIV
                                    // --------------------------------------------------------------------------------------------------
                                    sb.Append("<div id=\"GeneralDiv_" + OrderNumber.ToString() + "\" name=\"GeneralDiv_" + OrderNumber.ToString() + "\" style=\"width: 100%; display:none;\">\n");

                                    if ((ord.ParentOrderNumber == 0 || TransactionType == AppLogic.TransactionTypeEnum.RECURRING_AUTO) && ord.TransactionIsCaptured() && (IsCard || IsMicroPay || IsPayPal) && GWSupportsAdHocOrders)
                                    {
                                        sb.Append("<br/>");
                                        if (!ord.HasBeenEdited && (TransactionType != AppLogic.TransactionTypeEnum.RECURRING_AUTO || ord.AuthorizationPNREF.Length > 0))
                                        {
                                            sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.CreateAdhocChargeRefund", SkinID, LocaleSetting) + "\" name=\"AdHocChargeOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"AdHocChargeOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                        }
                                        if (!ord.HasBeenEdited && (ord.RecurringSubscriptionID.Length != 0 && ord.AuthorizationPNREF.Length > 0 && ord.RefundedOn == System.DateTime.MinValue))
                                        {
                                            sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.StopFutureBillingAndRefund", SkinID, LocaleSetting) + "\" name=\"CancelRecurringBilling" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"CancelRecurringBilling(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                        }
                                        sb.Append("<br/>");
                                    }

                                    bool HasDownload = AppLogic.OrderHasDownloadComponents(DB.RSFieldInt(rs2, "OrderNumber"), true);
                                    sb.Append("<table align=\"left\" valign=\"top\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
                                    sb.Append("<tr><td width=\"150\">&nbsp;</td><td>&nbsp;</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">\n");
                                    sb.Append(AppLogic.GetString("admin.common.OrderNumber", SkinID, LocaleSetting) + ":&nbsp;</td>\n");
                                    sb.Append("<td align=\"left\" valign=\"top\">\n");
                                    sb.Append(OrderNumber.ToString());
                                    if (ord.ParentOrderNumber != 0)
                                    {
                                        sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;" + AppLogic.GetString("admin.orderframe.ParentOrder", SkinID, LocaleSetting) + "<a href=\"" + AppLogic.AdminLinkUrl("orderframe.aspx") + "?ordernumber=" + ord.ParentOrderNumber + "\">" + ord.ParentOrderNumber.ToString() + "</a>&nbsp;&nbsp;");
                                    }
                                    if (ord.ChildOrderNumbers.Length != 0)
                                    {
                                        sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;" + AppLogic.GetString("admin.orderframe.RelatedOrder", SkinID, LocaleSetting));
                                        foreach (String s in ord.ChildOrderNumbers.Split(','))
                                        {
                                            sb.Append("<a href=\"" + AppLogic.AdminLinkUrl("orderframe.aspx") + "?ordernumber=" + s + "\">" + s + "</a>&nbsp;&nbsp;");
                                        }
                                    }
                                    sb.Append("</td>\n");

                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.common.OrderDate", SkinID, LocaleSetting) + ":&nbsp;</td><td align=\"left\" valign=\"top\">" + Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs2, "OrderDate")) + "</td></tr>\n");
                                    if (AppLogic.AppConfigBool("MaxMind.Enabled"))
                                    {
                                        sb.Append("<tr><td align='right' valign='top'>" + AppLogic.GetString("admin.orderframe.MaxMindFraudScore", SkinID, LocaleSetting) + "&nbsp;</td><td align='left' valign='top'>" + Localization.DecimalStringForDB(DB.RSFieldDecimal(rs2, "MaxMindFraudScore")) + " &nbsp;<a href='" + AppLogic.AppConfig("MaxMind.ExplanationLink") + "' target='_blank'>" + AppLogic.GetString("admin.orderframe.MaxMindFraudExplanation", SkinID, LocaleSetting) + "</a>");
                                        if (ord.MaxMindFraudScore == -1)
                                        {
                                            sb.Append("&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.GetMaxMind", SkinID, LocaleSetting) + "\" name=\"GetMaxmind" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"GetMaxmind(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                        }
                                        sb.Append("</td></tr>\n");
                                    }
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.CustomerID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSFieldInt(rs2, "CustomerID").ToString() + "</td></tr>\n");
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.IPAddress", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"middle\">");
                                    if (AppLogic.IPIsRestricted(LastIPAddress))
                                    {
                                        sb.Append("<font color=\"red\"><b>");
                                        sb.Append(LastIPAddress);
                                        sb.Append("</b></font>");
                                        sb.Append("&nbsp;&nbsp;");
                                        sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.AllowThisIP", SkinID, LocaleSetting) + "\" name=\"AllowIP" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"AllowIP(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                    }
                                    else
                                    {
                                        sb.Append(LastIPAddress);
                                        sb.Append("&nbsp;&nbsp;");
                                        sb.Append("<input  class=\"normalButtons\" " + CommonLogic.IIF(Customer.StaticIsAdminSuperUser(ord.CustomerID), " disabled ", "") + " type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.BlockThisIP", SkinID, LocaleSetting) + "\" name=\"BlockIP" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"BlockIP(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                    }
                                    sb.Append("</td></tr>\n");

                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.AffiliateID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" vali    gn=\"top\">" + DB.RSFieldInt(rs2, "AffiliateID").ToString() + "</td></tr>\n");

                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CustomerEmail", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><a href=\"mailto:" + ord.EMail + "?subject=" + "RE: " + AppLogic.AppConfig("StoreName") + " Order " + ord.OrderNumber.ToString() + "\">" + ord.EMail + "</a></td></tr>\n");
                                    int NumOrders = DB.GetSqlN("select count(ordernumber) as N from orders   with (NOLOCK)  where TransactionState in (" + DB.SQuote(AppLogic.ro_TXStateCaptured) + "," + DB.SQuote(AppLogic.ro_TXStateAuthorized) + ") and CustomerID=" + DB.RSFieldInt(rs2, "CustomerID").ToString());
                                    if (NumOrders > 0)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.OrderHistory", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");
                                        sb.Append("<a target=\"content\" href=\"" + AppLogic.AdminLinkUrl("cst_history.aspx") + "?customerid=" + DB.RSFieldInt(rs2, "CustomerID").ToString() + "\">");
                                        for (int i = 1; i <= NumOrders; i++)
                                        {
                                            sb.Append("<img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/smile.gif") + "\" border=\"0\" align=\"absmiddle\">");
                                            if (i % 25 == 0)
                                            {
                                                sb.Append("<br/>");
                                            }
                                        }
                                        sb.Append("</td></tr>\n");
                                    }
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">Order Total:&nbsp;</td><td align=\"left\" valign=\"top\">");
                                    sb.Append(CommonLogic.IIF(DB.RSFieldBool(rs2, "QuoteCheckout"), AppLogic.GetString("admin.orderframe.RequestForQuote", SkinID, LocaleSetting), ThisCustomer.CurrencyString(DB.RSFieldDecimal(rs2, "OrderTotal"))));


                                    if ((IsCard || IsMicroPay || IsPayPal || IsCOD) && GWSupportsAdHocOrders)
                                    {
                                        if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized))
                                        {
                                            sb.Append("&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.AdjustOrderTotal", SkinID, LocaleSetting) + "\" name=\"AdjustChargeOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"AdjustChargeOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                        }
                                    }

                                    sb.Append("</td></tr>\n");

                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.SubscriptionAdded", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + ord.SubscriptionTotalDays() + "</td></tr>\n");

                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.LocaleSetting", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + ord.LocaleSetting + "</td></tr>\n");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.StoreVersion", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "StoreVersion") + "</td></tr>\n");
                                    sb.Append(StoreLine(DB.RSFieldInt(rs2, "StoreID")));

                                    sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");

                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.PaymentMethod", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "PaymentMethod") + "</td></tr>\n");
                                    GatewayCheckoutByAmazon.CheckoutByAmazon checkoutByAmazon = new GatewayCheckoutByAmazon.CheckoutByAmazon();
                                    if (PM.ToLower() == GatewayCheckoutByAmazon.CheckoutByAmazon.CBA_Gateway_Identifier.ToLower())
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">Amazon Order Ids:</td><td align=\"left\" valign=\"top\">");
                                        foreach (String amazonAorderId in ord.AuthorizationPNREF.Split(new char[] { ',' }))
                                            sb.Append(amazonAorderId);
                                        sb.Append("</td></tr>\n");

                                        sb.Append("<tr><td align=\"right\" valign=\"top\">Feed Submission Ids:</td><td align=\"left\" valign=\"top\">");
                                        foreach (String feedSubmissionIds in ord.FinalizationData.Split(new char[] { ',' }))
                                            sb.Append(feedSubmissionIds);
                                        sb.Append("</td></tr>\n");
                                    }

                                    if (PM == AppLogic.ro_PMCreditCard)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardType", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "CardType") + "</td></tr>");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.default.PaymentGateway", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "PaymentGateway") + "</td></tr>");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.AVSResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "AVSResult") + "</td></tr>");
                                    }
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.order.TransactionType", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + TransactionType.ToString() + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.order.TransactionState", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");

                                    if (!ord.HasBeenEdited)
                                    {
                                        if ((TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStatePending) && TransactionType != AppLogic.TransactionTypeEnum.RECURRING_AUTO)
                                        {
                                            sb.Append(TransactionState);
                                            if (TransactionState == AppLogic.ro_TXStateRefunded && ord.RefundTXCommand.IndexOf("FORCED", StringComparison.InvariantCultureIgnoreCase) != -1)
                                            {
                                                sb.Append("&nbsp;(" + ord.RefundTXCommand + ")");
                                            }
                                            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.Void", SkinID, LocaleSetting) + "\" name=\"VoidOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"VoidOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ForceVoid", SkinID, LocaleSetting) + "\" name=\"ForceVoidOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ForceVoidOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.Capture", SkinID, LocaleSetting) + "\" name=\"CaptureOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"CaptureOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            if (IsPayPal)
                                            {
                                                sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.Reauthorize", SkinID, LocaleSetting) + "\" name=\"PayPalReauthOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"PayPalReauthOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            }
                                            if (IsAmazonSimplePay)
                                            {
                                                sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.UpdateTransaction", SkinID, LocaleSetting) + "\" name=\"AmazonTransaction" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"GetTransactionStatus(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            }
                                            if (PM.EqualsIgnoreCase(GatewayCheckoutByAmazon.CheckoutByAmazon.CBA_Gateway_Identifier))
                                            {
                                                sb.Append(String.Format("<b>{0}</b>", "gw.checkoutbyamazon.display.2".StringResource()));
                                            }
                                        }
                                        else if (TransactionState == AppLogic.ro_TXStateCaptured && (TransactionType != AppLogic.TransactionTypeEnum.RECURRING_AUTO || ord.AuthorizationPNREF.Length > 0))
                                        {
                                            sb.Append(TransactionState);

                                            // With some gateways, if already captured, you can only refund it (not void it).
                                            if (GW != Gateway.ro_GWMONEYBOOKERS && PM != AppLogic.ro_PMMoneybookersQuickCheckout)
                                            {
                                                sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.Void", SkinID, LocaleSetting) + "\" name=\"VoidOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"VoidOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                                sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ForceVoid", SkinID, LocaleSetting) + "\" name=\"ForceVoidOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ForceVoidOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            }
                                            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Refund", SkinID, LocaleSetting) + "\" name=\"RefundOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"RefundOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            if (PM == AppLogic.ro_PMCreditCard || PM == AppLogic.ro_PMMicropay || PM == AppLogic.ro_PMAmazonSimplePay)
                                            {
                                                sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ForceRefund", SkinID, LocaleSetting) + "\" name=\"ForceRefund" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ForceRefund(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            }
                                        }
                                        else
                                        {
                                            sb.Append(TransactionState);
                                            if (TransactionState == AppLogic.ro_TXStateRefunded && ord.RefundTXCommand.IndexOf("FORCED", StringComparison.InvariantCultureIgnoreCase) != -1)
                                            {
                                                sb.Append("&nbsp;(" + ord.RefundTXCommand + ")");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sb.Append(TransactionState);
                                    }
                                    sb.Append("</td></tr>");

                                    if (PM == AppLogic.ro_PMCreditCard || PM == AppLogic.ro_PMMicropay || IsPayPal || PM == AppLogic.ro_PMCardinalMyECheck)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.TransactionID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "AuthorizationPNREF") + "</td></tr>");
                                    }
                                    if (DB.RSField(rs2, "RecurringSubscriptionID").Length != 0 || TransactionType == AppLogic.TransactionTypeEnum.RECURRING_AUTO)
                                    {
                                        if (GW == AspDotNetStorefrontGateways.Gateway.ro_GWVERISIGN || GW == AspDotNetStorefrontGateways.Gateway.ro_GWPAYFLOWPRO)
                                        { // include link to recurringgatewaydetails.aspx for live gateway status 
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.GatewayAutoBillSubscriptionID", SkinID, LocaleSetting) + "</td><td align=\"left\" valign=\"top\"><a href=\"" + AppLogic.AdminLinkUrl("recurringgatewaydetails.aspx") + "?RecurringSubscriptionID=" + DB.RSField(rs2, "RecurringSubscriptionID") + "\">" + DB.RSField(rs2, "RecurringSubscriptionID") + "</a></td></tr>");
                                        }
                                        else
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.GatewayAutoBillSubscriptionID", SkinID, LocaleSetting) + "</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "RecurringSubscriptionID") + "</td></tr>");
                                        }
                                    }
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.AuthorizedOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + Localization.ToNativeDateTimeString(AuthorizedOn) + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.CapturedOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + Localization.ToNativeDateTimeString(CapturedOn) + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.VoidedOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + Localization.ToNativeDateTimeString(VoidedOn) + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.RefundedOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + Localization.ToNativeDateTimeString(RefundedOn) + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.FraudedOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + Localization.ToNativeDateTimeString(FraudedOn) + "</td></tr>");

                                    sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.ReceiptEmailSentOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"middle\">" + CommonLogic.IIF(DB.RSFieldDateTime(rs2, "ReceiptEMailSentOn") != System.DateTime.MinValue, Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs2, "ReceiptEMailSentOn")), "Not Sent"));
                                    if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                    {
                                        sb.Append("&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + CommonLogic.IIF(DB.RSFieldDateTime(rs2, "ReceiptEMailSentOn") != System.DateTime.MinValue, AppLogic.GetString("admin.orderframe.ReSendReceiptEmail", SkinID, LocaleSetting), AppLogic.GetString("admin.orderframe.SendReceiptEMail", SkinID, LocaleSetting)) + "\" name=\"Clear" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"SendReceiptEMail(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                    }
                                    sb.Append("&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"Regenerate Receipt\" name=\"Regen" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"RegenerateReceipt(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                    sb.Append("</td></tr>\n");
                                    sb.Append("</td></tr>\n");
                                    sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");

                                    if (HasShippableComponents && !ord.HasBeenEdited && (DB.RSFieldDateTime(rs2, "ShippedOn") == System.DateTime.MinValue && DB.RSFieldDateTime(rs2, "DownloadEmailSentOn") == System.DateTime.MinValue && TransactionType == AppLogic.TransactionTypeEnum.CHARGE))
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.ReadyToShip", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"middle\">");
                                        if (DB.RSFieldBool(rs2, "ReadyToShip"))
                                        {
                                            sb.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ClearReadyToShip", SkinID, LocaleSetting) + "\" name=\"ClearReadyToShip" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ClearReadyToShip(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                        }
                                        else
                                        {
                                            sb.Append(AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "&nbsp;");
                                            if ((TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                            {
                                                sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.MarkAsReadyToShip", SkinID, LocaleSetting) + "\" name=\"MarkReadyToShip" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"MarkReadyToShip(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            }
                                        }
                                        sb.Append("</td></tr>\n");
                                        if (!ord.IsAllDownloadComponents() && !ord.IsAllSystemComponents())
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.OrderWeight", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"middle\">");
                                            //JH 10.18.2010 - don't allow weight change on multiship orders. It doesn't make sense to change order weight if there are multiple shipments.
                                            if (ord.HasMultipleShippingAddresses())
                                            {
                                                sb.Append("Multiple Shipments");
                                            }
                                            else if ((TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                            {
                                                if (!ord.HasBeenEdited)
                                                {
                                                    sb.Append("<input type=\"text\" size=\"5\" name=\"OrderWeight\" value=\"" + Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.OrderWeight) + "\">&nbsp;");
                                                    sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.AdjustWeight", SkinID, LocaleSetting) + "\" name=\"AdjustOrderWeight" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"AdjustOrderWeight(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                                }
                                            }
                                            else
                                            {
                                                sb.Append(Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.OrderWeight));
                                            }
                                            sb.Append("</td></tr>\n");
                                        }
                                    }

                                    if (TransactionType == AppLogic.TransactionTypeEnum.CHARGE)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ShippingMethod", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(HasShippableComponents, DB.RSFieldByLocale(rs2, "ShippingMethod", LocaleSetting), AppLogic.GetString("admin.orderframe.AllDownloadItems", SkinID, LocaleSetting)) + "</td></tr>\n");
                                    }
                                    sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");


                                    if (TransactionType == AppLogic.TransactionTypeEnum.CHARGE)
                                    {
                                        if (HasShippableComponents)
                                        {
                                            bool bGCorder = DB.GetSqlS(
                                                "select PaymentGateway S from Orders where OrderNumber = " +
                                                OrderNumber.ToString()).StartsWith(Gateway.ro_GWGOOGLECHECKOUT);
                                            // for google checkout order we can enter tracking info after it is marked as shipped or ready to ship

                                            if (DB.RSFieldDateTime(rs2, "ShippedOn") != System.DateTime.MinValue)
                                            {
                                                ShippingStatus = "Shipped";
                                                bool bGCprompt = false;
                                                if (DB.RSField(rs2, "ShippedVIA").Length != 0)
                                                {
                                                    ShippingStatus += " via " + DB.RSField(rs2, "ShippedVIA");
                                                }
                                                else if (!ord.HasBeenEdited && bGCorder)
                                                {
                                                    ShippingStatus += AppLogic.GetString("admin.orderframe.Carrier", SkinID, LocaleSetting) + "<input maxlength=\"50\" size=\"10\" type=\"text\" name=\"ShippedVIA\">&nbsp;&nbsp;";
                                                    bGCprompt = true;
                                                }
                                                ShippingStatus += " on " + Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs2, "ShippedOn")) + ".";
                                                if (DB.RSField(rs2, "ShippingTrackingNumber").Length != 0)
                                                {
                                                    ShippingStatus += AppLogic.GetString("admin.orderframe.TrackingNumber", SkinID, LocaleSetting);

                                                    String TrackURL = Shipping.GetTrackingURL(DB.RSField(rs2, "ShippingTrackingNumber"));
                                                    if (TrackURL.Length != 0)
                                                    {
                                                        ShippingStatus += "<a href=\"" + TrackURL + "\" target=\"_blank\">" + DB.RSField(rs2, "ShippingTrackingNumber") + "</a>";
                                                    }
                                                    else
                                                    {
                                                        ShippingStatus += DB.RSField(rs2, "ShippingTrackingNumber");
                                                    }
                                                    ShippingStatus += ". ";
                                                }
                                                else if (!ord.HasBeenEdited && bGCorder)
                                                {
                                                    ShippingStatus += "<br/>" + AppLogic.GetString("admin.orderframe.TrackingNumber", SkinID, LocaleSetting) + " <input maxlength=\"50\" size=\"10\" type=\"text\" name=\"ShippingTrackingNumber\">&nbsp;&nbsp;";
                                                    bGCprompt = true;
                                                }
                                                if (!ord.HasBeenEdited && bGCprompt)
                                                {
                                                    ShippingStatus += " <input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.SendTrackingNumber", SkinID, LocaleSetting) + "\" onClick=\"document.OrderDetailForm.SubmitAction.value='SENDTRACKING';document.OrderDetailForm.submit();\"><br/>\n";
                                                }
                                                ShippingStatus += " " + AppLogic.GetString("admin.orderframe.CarrierReportedRate", SkinID, LocaleSetting) + DB.RSField(rs2, "CarrierReportedRate") + ". ";
                                                ShippingStatus += " " + AppLogic.GetString("admin.orderframe.CarrierReportedWeight", SkinID, LocaleSetting) + DB.RSField(rs2, "CarrierReportedWeight") + ". ";

                                            }
                                            else if (!AppLogic.AppConfigBool("ShipRush.Enabled") && !AppLogic.AppConfigBool("FedExShipManager.Enabled"))
                                            {
                                                if (DB.RSField(rs2, "ShippedVIA").Length != 0)
                                                {
                                                    ShippingStatus += AppLogic.GetString("admin.orderframe.Carrier", SkinID, LocaleSetting) + DB.RSField(rs2, "ShippedVIA") + ".";
                                                }
                                                else if (!ord.HasBeenEdited)
                                                {
                                                    ShippingStatus += AppLogic.GetString("admin.orderframe.Carrier", SkinID, LocaleSetting) + "<input maxlength=\"50\" size=\"10\" type=\"text\" name=\"ShippedVIA\">&nbsp;&nbsp;";
                                                }
                                                if (DB.RSField(rs2, "ShippingTrackingNumber").Length != 0)
                                                {
                                                    ShippingStatus += " " + AppLogic.GetString("admin.orderframe.TrackingNumber", SkinID, LocaleSetting);

                                                    String TrackURL = Shipping.GetTrackingURL(DB.RSField(rs2, "ShippingTrackingNumber"));
                                                    if (TrackURL.Length != 0)
                                                    {
                                                        ShippingStatus += "<a href=\"" + TrackURL + "\" target=\"_blank\">" + DB.RSField(rs2, "ShippingTrackingNumber") + "</a>";
                                                    }
                                                    else
                                                    {
                                                        ShippingStatus += DB.RSField(rs2, "ShippingTrackingNumber");
                                                    }
                                                    ShippingStatus += ". ";
                                                }
                                                else if (!ord.HasBeenEdited)
                                                {
                                                    ShippingStatus += "<br/>" + AppLogic.GetString("admin.orderframe.TrackingNumber", SkinID, LocaleSetting) + " <input maxlength=\"50\" size=\"10\" type=\"text\" name=\"ShippingTrackingNumber\">&nbsp;&nbsp;";
                                                }
                                                if (DB.RSFieldBool(rs2, "ReadyToShip") && bGCorder)
                                                {
                                                    if (!ord.HasBeenEdited && (DB.RSField(rs2, "ShippingTrackingNumber").Length == 0 || DB.RSField(rs2, "ShippedVIA").Length == 0))
                                                    {
                                                        ShippingStatus += " <input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.SendTrackingNumber", SkinID, LocaleSetting) + "\" onClick=\"document.OrderDetailForm.SubmitAction.value='SENDTRACKING';document.OrderDetailForm.submit();\"><br/>\n";
                                                    }
                                                }
                                                if (!ord.HasBeenEdited)
                                                {
                                                    //GFS - Added correct ID specification for calendar Calender.js actuation.
                                                    ShippingStatus += "<br/>Shipped On: <input maxlength=\"15\" size=\"10\" type=\"text\" id=\"ShippedOn\" name=\"ShippedOn\">&nbsp;<img src=\"" + AppLogic.LocateImageURL("App_Themes/Admin_Default/images/calendar.gif") + "\" class=\"actionelement\" align=\"absmiddle\" id=\"f_trigger_s\">&nbsp;&nbsp;(defaults to today's date)";
                                                    //End GFS
                                                    if (ord.TransactionIsCaptured())
                                                    {
                                                        ShippingStatus += "<br/><input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.MarkAsShipped", SkinID, LocaleSetting) + "\" onClick=\"document.OrderDetailForm.SubmitAction.value='MARKASSHIPPED';document.OrderDetailForm.submit();\">\n";
                                                    }
                                                    else
                                                    {
                                                        ShippingStatus += "<br/><input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.MarkAsShipped", SkinID, LocaleSetting) + "\" onClick=\"if(confirm('Are you sure you want to proceed. The payment for this order has not yet cleared, and this will close the order, and remove the IsNew status from the order!')) {document.OrderDetailForm.SubmitAction.value='MARKASSHIPPED';document.OrderDetailForm.submit();}\">\n";
                                                    }
                                                }
                                            }
                                            sb.Append("<tr><td align=\"right\" width=\"200\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ShippingStatus", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + ShippingStatus + "</td></tr>\n");
                                        }
                                    }

                                    if (TransactionType == AppLogic.TransactionTypeEnum.CHARGE)
                                    {
                                        sb.Append("<tr><td align=\"right\" width=\"200\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.WasPrinted", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(DB.RSFieldBool(rs2, "IsPrinted"), AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting), AppLogic.GetString("admin.common.No", SkinID, LocaleSetting)));
                                        if (!ord.HasBeenEdited && !DB.RSFieldBool(rs2, "IsPrinted"))
                                        {
                                            sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.MarkAsPrinted", SkinID, LocaleSetting) + "\" onClick=\"document.OrderDetailForm.SubmitAction.value='MARKASPRINTED';document.OrderDetailForm.submit();\">");
                                        }
                                        sb.Append("</td></tr>\n");
                                    }

                                    sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");

                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasDownloadItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">Yes</td></tr>\n");

                                    if (ord.HasDownloadComponents(false))
                                    {
                                        foreach (CartItem c in ord.CartItems)
                                        {
                                            if (c.IsDownload)
                                            {
                                                DownloadItem downloadItem = new DownloadItem();
                                                downloadItem.Load(c.ShoppingCartRecordID);

                                                if (downloadItem.Status == DownloadItem.DownloadItemStatus.Pending && AppLogic.AppConfigBool("MaxMind.Enabled") && ord.MaxMindFraudScore >= AppLogic.AppConfigNativeDecimal("MaxMind.DelayDownloadThreshold"))
                                                    sb.Append(string.Format("<tr><td colspan=\"2\" align=\"right\"><b>{0}</b></td></tr>", AppLogic.GetString("download.aspx.17", LocaleSetting)));

                                                sb.Append("<tr><td colspan=\"2\" align=\"right\"></td></tr>");
                                                sb.Append("<tr>");
                                                sb.Append("	<td align=\"right\">");
                                                sb.Append(string.Format("{0}: ", downloadItem.DownloadName));
                                                sb.Append("	</td>");
                                                sb.Append("	<td align=\"left\" valign=\"top\">");

                                                if (!AppLogic.AppConfig("Download.ReleaseOnAction").EqualsIgnoreCase("manual") && downloadItem.DownloadLocation.Length == 0)
                                                    sb.Append(string.Format("<input type=\"text\" value=\"{0}\" style=\"width:300px;\" name=\"DownloadLocation{2}\" /> <input  class=\"normalButtons\" type=\"button\" value=\"{1}\" name=\"DownloadRelease{2}\" onClick=\"ReleaseDownload(OrderDetailForm,{2});\">", downloadItem.DownloadLocation, AppLogic.GetString("admin.orderframe.DownloadReleaseItem", SkinID, LocaleSetting), downloadItem.ShoppingCartRecordId));
                                                else if (downloadItem.Status == DownloadItem.DownloadItemStatus.Available)
                                                    sb.Append(string.Format("{0} {1}", AppLogic.GetString("admin.orderframe.DownloadReleasedOn", SkinID, LocaleSetting), downloadItem.ReleasedOn));
                                                else if (downloadItem.Status == DownloadItem.DownloadItemStatus.Expired)
                                                    sb.Append(string.Format("{0} {1}", AppLogic.GetString("admin.orderframe.DownloadExpiredOn", SkinID, LocaleSetting), downloadItem.ExpiresOn));
                                                else
                                                    sb.Append(string.Format("<input type=\"text\" value=\"{0}\" style=\"width:300px;\" name=\"DownloadLocation{2}\" /> <input  class=\"normalButtons\" type=\"button\" value=\"{1}\" name=\"DownloadRelease{2}\" onClick=\"ReleaseDownload(OrderDetailForm,{2});\">", downloadItem.DownloadLocation, AppLogic.GetString("admin.orderframe.DownloadReleaseItem", SkinID, LocaleSetting), downloadItem.ShoppingCartRecordId));

                                                sb.Append("	</td>");
                                                sb.Append("</tr>");
                                            }
                                        }

                                    }
                                    sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");

                                    if (HasShippableComponents && TransactionType == AppLogic.TransactionTypeEnum.CHARGE)
                                    {
                                        if (ord.HasDistributorComponents())
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasDistributorDropShipItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "</td></tr>\n");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.DistributorEmailSentOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(DB.RSFieldDateTime(rs2, "DistributorEMailSentOn") != System.DateTime.MinValue, Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs2, "DistributorEMailSentOn")), AppLogic.ro_NotApplicable));
                                            if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                            {
                                                sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + CommonLogic.IIF(DB.RSFieldDateTime(rs2, "DistributorEMailSentOn") != System.DateTime.MinValue, AppLogic.GetString("admin.orderframe.ReSendDistributorEmails", SkinID, LocaleSetting), AppLogic.GetString("admin.orderframe.SendDistributorEmails", SkinID, LocaleSetting)) + "\" name=\"Distributor" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"SendDistributorEMail(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            }
                                            sb.Append("</td></tr>\n");

                                        }
                                        else
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasDistributorItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "</td></tr>\n");
                                        }
                                    }

                                    if (TransactionType == AppLogic.TransactionTypeEnum.CHARGE)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasMultipleShippingAddresses", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(ord.HasMultipleShippingAddresses(), AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting), AppLogic.GetString("admin.common.No", SkinID, LocaleSetting)) + "</td></tr>\n");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasGiftRegistryItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(ord.HasGiftRegistryComponents(), AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting), AppLogic.GetString("admin.common.No", SkinID, LocaleSetting)) + "</td></tr>\n");
                                    }
                                    if (TransactionType == AppLogic.TransactionTypeEnum.CHARGE)
                                    {
                                        if (_shipRushEnabledAndConfigured)
                                        {
                                            sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ShipRush", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");
                                            if (DB.RSFieldDateTime(rs2, "ShippedOn") == System.DateTime.MinValue)
                                            {
                                                if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                                {
                                                    sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.SendToShipRush", SkinID, LocaleSetting) + "\" name=\"ShipRush" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ShipRushOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "," + CommonLogic.IIF(ord.TransactionIsCaptured(), "1", "0") + ");\">");
                                                }
                                            }
                                            else
                                            {
                                                sb.Append(String.Format(AppLogic.GetString("admin.orderframe.SentToShipRush", SkinID, LocaleSetting), Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs2, "ShippedOn"))));
                                                if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                                {
                                                    sb.Append("&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ReSendToShipRush", SkinID, LocaleSetting) + "\" name=\"ShipRush" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ShipRushOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "," + CommonLogic.IIF(ord.TransactionIsCaptured(), "1", "0") + ");\">");
                                                }
                                            }
                                            sb.Append("</td></tr>");
                                        }
                                        if (AppLogic.AppConfigBool("FedExShipManager.Enabled"))
                                        {
                                            sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.FedExShipManager", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");
                                            if (DB.RSFieldDateTime(rs2, "ShippedOn") == System.DateTime.MinValue)
                                            {
                                                if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                                {
                                                    sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.SendToFedExShipManager", SkinID, LocaleSetting) + "\" name=\"FedExShipManager" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"FedExShipManagerOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "," + CommonLogic.IIF(ord.TransactionIsCaptured(), "1", "0") + ");\">");
                                                }
                                            }
                                            else
                                            {
                                                sb.Append(AppLogic.GetString("admin.orderframe.FraudedOn", SkinID, LocaleSetting) + Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs2, "ShippedOn")));
                                                if (!ord.HasBeenEdited)
                                                {
                                                    sb.Append("&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ReSendToFedExShipManager", SkinID, LocaleSetting) + "\" name=\"FedExShipManager" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"FedExShipManagerOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "," + CommonLogic.IIF(ord.TransactionIsCaptured(), "1", "0") + ");\">");
                                                }
                                            }
                                            sb.Append("</td></tr>");
                                        }
                                    }

                                    sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.BillTo", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + BillAddr + "</td></tr>\n");
                                    if (HasShippableComponents && !ord.HasMultipleShippingAddresses())
                                    {
                                        sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ShipTo", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + ShipAddr + "</td></tr>\n");
                                    }
                                    sb.Append("</table>\n");
                                    sb.Append("</div>\n");

                                    // --------------------------------------------------------------------------------------------------
                                    // BILLING DIV
                                    // --------------------------------------------------------------------------------------------------
                                    sb.Append("<div id=\"BillingDiv_" + OrderNumber.ToString() + "\" name=\"BillingDiv_" + OrderNumber.ToString() + "\" style=\"width: 100%; display:none;\">\n");

                                    sb.Append("<table align=\"left\" valign=\"top\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.OrderNumber", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + OrderNumber.ToString());
                                    if (ord.ParentOrderNumber != 0)
                                    {
                                        sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;" + AppLogic.GetString("admin.orderframe.ParentOrder", SkinID, LocaleSetting) + "<a href=\"" + AppLogic.AdminLinkUrl("orderframe.aspx") + "?ordernumber=" + ord.ParentOrderNumber + "\">" + ord.ParentOrderNumber.ToString() + "</a>&nbsp;&nbsp;");
                                    }
                                    if (ord.ChildOrderNumbers.Length != 0)
                                    {
                                        sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;" + AppLogic.GetString("admin.orderframe.RelatedOrder", SkinID, LocaleSetting));
                                        foreach (String s in ord.ChildOrderNumbers.Split(','))
                                        {
                                            sb.Append("<a href=\"" + AppLogic.AdminLinkUrl("orderframe.aspx") + "?ordernumber=" + s + "\">" + s + "</a>&nbsp;&nbsp;");
                                        }
                                    }
                                    sb.Append("</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.CustomerID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSFieldInt(rs2, "CustomerID").ToString() + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.AffiliateID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSFieldInt(rs2, "AffiliateID").ToString() + "</td></tr>\n");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.OrderDate", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSFieldDateTime(rs2, "OrderDate").ToString() + "</td></tr>");
                                    if (AppLogic.AppConfigBool("MaxMind.Enabled"))
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.MaxMindFraudScore", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + Localization.DecimalStringForDB(DB.RSFieldDecimal(rs2, "MaxMindFraudScore")) + " &nbsp;<a href='" + AppLogic.AppConfig("MaxMind.ExplanationLink") + "' target=\"_blank\">" + AppLogic.GetString("admin.orderframe.MaxMindFraudExplanation", SkinID, LocaleSetting) + "</a>");
                                        if (ord.MaxMindFraudScore == -1)
                                        {
                                            sb.Append("&nbsp;&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.GetMaxMind", SkinID, LocaleSetting) + "\" name=\"GetMaxmind" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"GetMaxmind(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                        }
                                        sb.Append("</td></tr>\n");
                                    }
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.OrderTotal", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");
                                    sb.Append(ThisCustomer.CurrencyString(DB.RSFieldDecimal(rs2, "OrderTotal")));

                                    if ((IsCard || IsMicroPay) && GWSupportsAdHocOrders)
                                    {
                                        if (!ord.HasBeenEdited && TransactionState == AppLogic.ro_TXStateAuthorized)
                                        {
                                            sb.Append("&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.AdjustOrderTotal", SkinID, LocaleSetting) + "\" name=\"AdjustChargeOrder" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"AdjustChargeOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                        }
                                    }

                                    sb.Append("</td></tr>");
                                    if (PM == AppLogic.ro_PMCreditCard)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardType", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "CardType") + "</td></tr>");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.default.PaymentGateway", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "PaymentGateway") + "</td></tr>");
                                    }
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.TransactionType", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + TransactionType.ToString() + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.TransactionState", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + TransactionState + "</td></tr>");
                                    if (PM == AppLogic.ro_PMCreditCard)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.AVSResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "AVSResult") + "</td></tr>");
                                    }

                                    String _cardNumber = "";
                                    if (ThisCustomer.AdminCanViewCC)
                                    {
                                        _cardNumber = AppLogic.AdminViewCardNumber(DB.RSField(rs2, "CardNumber"), "Orders", ord.OrderNumber);
                                        if (_cardNumber.Length > 0 && _cardNumber != AppLogic.ro_CCNotStoredString) //log admin viewing card number
                                        {
                                            Security.LogEvent("Viewed Credit Card", AppLogic.GetString("admin.orderframe.ViewedCardNumber", SkinID, LocaleSetting) + _cardNumber.Substring(_cardNumber.Length - 4).PadLeft(_cardNumber.Length, '*') + " " + AppLogic.GetString("admin.orderframe.ViewedCardNumberOnOrderNumber", SkinID, LocaleSetting) + ord.OrderNumber.ToString(), OrderCustomer.CustomerID, ThisCustomer.CustomerID, Convert.ToInt32(ThisCustomer.CurrentSessionID));
                                        }
                                    }
                                    else
                                    {
                                        _cardNumber = AppLogic.GetString("admin.orderframe.NoPermissionToView", SkinID, LocaleSetting);
                                    }

                                    String _last4 = DB.RSField(rs2, "Last4");
                                    String _cardType = DB.RSField(rs2, "CardType");

                                    if (IsEcheck)
                                    {
                                        string saltKey = Order.StaticGetSaltKey(OrderNumber);

                                        string realECheckABACode = string.Empty;
                                        string realCheckBankAccountNumber = string.Empty;

                                        string eCheckABACode = DB.RSField(rs, "ECheckBankABACode");
                                        string eCheckABACodeUnMunged = Security.UnmungeString(eCheckABACode, saltKey);

                                        if (eCheckABACodeUnMunged.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            // Failed decryption, must be clear text
                                            realECheckABACode = eCheckABACode;
                                        }
                                        else
                                        {
                                            // decryption successful, must be already encrypted
                                            realECheckABACode = eCheckABACodeUnMunged;
                                        }

                                        string eCheckBankAccountNumber = DB.RSField(rs, "ECheckBankAccountNumber");
                                        string eCheckBankAccountNumberUnMunged = Security.UnmungeString(eCheckBankAccountNumber, saltKey);

                                        if (eCheckBankAccountNumberUnMunged.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            // Failed decryption, must be clear text
                                            realCheckBankAccountNumber = eCheckBankAccountNumber;
                                        }
                                        else
                                        {
                                            // decryption successful, must be already encrypted
                                            realCheckBankAccountNumber = eCheckBankAccountNumberUnMunged;
                                        }

                                        // masked the account
                                        if (AppLogic.StoreCCInDB())
                                        {
                                            if (!CommonLogic.IsStringNullOrEmpty(realECheckABACode))
                                            {
                                                realECheckABACode = AppLogic.Mask(realECheckABACode);
                                            }

                                            if (!CommonLogic.IsStringNullOrEmpty(realCheckBankAccountNumber))
                                            {
                                                realCheckBankAccountNumber = AppLogic.Mask(realCheckBankAccountNumber);
                                            }
                                        }


                                        sb.Append(String.Format("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ECheckBankName", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">{0}</td></tr>", DB.RSField(rs2, "ECheckBankName")));
                                        sb.Append(String.Format("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ECheckABA", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">{0}</td></tr>", realECheckABACode));
                                        sb.Append(String.Format("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ECheckAccount", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">{0}</td></tr>", realCheckBankAccountNumber));
                                        sb.Append(String.Format("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ECheckAccountName", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">{0}</td></tr>", DB.RSField(rs2, "ECheckBankAccountName")));
                                        sb.Append(String.Format("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ECheckAccountType", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">{0}</td></tr>", DB.RSField(rs2, "ECheckBankAccountType")));
                                    }
                                    if (IsMicroPay)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("account.aspx.11", 1, LocaleSetting) + " " + AppLogic.GetString("admin.orderframe.Transaction", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"></td></tr>");
                                    }
                                    else if (PM == AppLogic.ro_PMCreditCard)
                                    {
                                        if (_cardType.StartsWith(AppLogic.ro_PMPayPal, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardNumber", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AppLogic.GetString("admin.orders.PaymentMethod.PayPal", SkinID, LocaleSetting) + "</td></tr>");
                                        }
                                        else
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardNumber", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + _cardNumber + "</td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.LastFour", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + _last4 + "</td></tr>");
                                        }
                                        if (_cardNumber.Length == 0 || _cardNumber == AppLogic.ro_CCNotStoredString)
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardExpiration", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.NotStored", SkinID, LocaleSetting) + "</td></tr>");
                                        }
                                        else
                                        {
                                            if (_cardType.StartsWith(AppLogic.ro_PMPayPal, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardExpiration", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.NotAvailable", SkinID, LocaleSetting) + "</td></tr>");
                                            }
                                            else
                                            {
                                                sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardExpiration", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "CardExpirationMonth") + "/" + DB.RSField(rs2, "cardExpirationYear") + "</td></tr>");
                                            }
                                        }
                                        if (AppLogic.AppConfigBool("ShowCardStartDateFields"))
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardStartDate", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "CardStartDate") + "</td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardIssueNumber", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + Security.UnmungeString(DB.RSField(rs2, "CardIssueNumber")) + "</td></tr>");
                                        }
                                    }

                                    if (AppLogic.AppConfigBool("MaxMind.Enabled"))
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.MaxMindDetails", SkinID, LocaleSetting) + "&nbsp;<br/><a href='" + AppLogic.AppConfig("MaxMind.ExplanationLink") + "' target=\"_blank\">" + AppLogic.GetString("admin.orderframe.MaxMindFraudExplanation", SkinID, LocaleSetting) + "</a></td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + DB.RSField(rs2, "MaxMindDetails") + "</textarea></td></tr>");
                                    }
                                    //Promotion usage info
                                    using (SqlConnection promoConn = DB.dbConn())
                                    {
                                        SqlParameter[] promoSpa = { new SqlParameter("@OrderNumber", ord.OrderNumber) };
                                        string promoSQL = @"SELECT pu.ShippingDiscountAmount, pu.LineItemDiscountAmount, pu.OrderDiscountAmount, pu.DiscountAmount, p.Code, 
												CASE  WHEN pu.ShippingDiscountAmount + pu.LineItemDiscountAmount + pu.OrderDiscountAmount != pu.DiscountAmount THEN 1
													  WHEN pu.ShippingDiscountAmount + pu.LineItemDiscountAmount + pu.OrderDiscountAmount = pu.DiscountAmount THEN 0
													END
												AS GiftWithPurchase
											FROM PromotionUsage pu
											INNER JOIN Promotions p ON pu.PromotionID = p.ID
											WHERE pu.OrderId = @OrderNumber";

                                        promoConn.Open();
                                        using (IDataReader promoRS = DB.GetRS(promoSQL, promoSpa, promoConn))
                                        {
                                            int rowCount = 0;
                                            string rowStyle;
                                            while (promoRS.Read())
                                            {
                                                if (rowCount == 0)
                                                {
                                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.PromotionUsage", SkinID, LocaleSetting)
                                                    + "&nbsp;</td><td align=\"left\" valign=\"top\">"
                                                    + "<table class=\"orderFramePromoTable\" ><tr><th align=\"left\">" + AppLogic.GetString("admin.orderframe.PromotionCode", SkinID, LocaleSetting) + "</th>"
                                                    + "<th align=\"left\">" + AppLogic.GetString("admin.orderframe.ShippingDiscount", SkinID, LocaleSetting) + "</th>"
                                                    + "<th align=\"left\">" + AppLogic.GetString("admin.orderframe.LineItemDiscount", SkinID, LocaleSetting) + "</th>"
                                                    + "<th align=\"left\">" + AppLogic.GetString("admin.orderframe.OrderLevelDiscount", SkinID, LocaleSetting) + "</th>"
                                                    + "<th align=\"left\">" + AppLogic.GetString("admin.orderframe.GiftWPurchase", SkinID, LocaleSetting) + "</th>"
                                                    + "<th align=\"left\">" + AppLogic.GetString("admin.orderframe.TotalDiscount", SkinID, LocaleSetting) + "</th>"
                                                    + "</th></tr>");
                                                }

                                                rowStyle = rowCount % 2 == 0 ? "orderFramePromoTableRow" : "orderFramePromoTableAlternatingRow";
                                                sb.Append("<tr class=\"" + rowStyle + "\"><td>" + DB.RSField(promoRS, "Code") + "</td>");
                                                sb.Append("<td>" + Localization.CurrencyStringForDisplayWithExchangeRate(DB.RSFieldDecimal(promoRS, "ShippingDiscountAmount"), ThisCustomer.CurrencySetting) + "</td>");
                                                sb.Append("<td>" + Localization.CurrencyStringForDisplayWithExchangeRate(DB.RSFieldDecimal(promoRS, "LineItemDiscountAmount"), ThisCustomer.CurrencySetting) + "</td>");
                                                sb.Append("<td>" + Localization.CurrencyStringForDisplayWithExchangeRate(DB.RSFieldDecimal(promoRS, "OrderDiscountAmount"), ThisCustomer.CurrencySetting) + "</td>");
                                                sb.Append("<td><input type=\"checkbox\" disabled =\"disabled\" name=\"cbxGiftWPurchase\"" + (DB.RSFieldBool(promoRS, "GiftWithPurchase") ? " checked=\"checked\"" : String.Empty) + "\" /></td>");
                                                sb.Append("<td>" + Localization.CurrencyStringForDisplayWithExchangeRate(DB.RSFieldDecimal(promoRS, "DiscountAmount"), ThisCustomer.CurrencySetting) + "</td></tr>");
                                                rowCount++;
                                            }

                                            if (rowCount > 0)
                                                sb.Append("</table>");
                                        }

                                    }


                                    if (PM == AppLogic.ro_PMCreditCard || PM == AppLogic.ro_PMMicropay || IsPayPal)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.TransactionID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "AuthorizationPNREF") + "</td></tr>");
                                        if (DB.RSField(rs2, "RecurringSubscriptionID").Length != 0 || TransactionType == AppLogic.TransactionTypeEnum.RECURRING_AUTO)
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.GatewayAutoBillSubscriptionID", SkinID, LocaleSetting) + "</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "RecurringSubscriptionID") + "</td></tr>");
                                        }
                                        if (ThisCustomer.AdminCanViewCC)
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.TransactionCommand", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + DB.RSField(rs2, "TransactionCommand") + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.AuthorizationResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + DB.RSField(rs2, "AuthorizationResult") + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.AuthorizationCode", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + DB.RSField(rs2, "AuthorizationCode") + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CaptureTXCommand", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "CaptureTXCommand").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "CaptureTXCommand")) + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CaptureTXResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "CaptureTXResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "CaptureTXResult")) + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.VoidTXCommand", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "VoidTXCommand").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "VoidTXCommand")) + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.VoidTXResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "VoidTXResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "VoidTXResult")) + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.RefundTXCommand", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "RefundTXCommand").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "RefundTXCommand")) + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.RefundTXResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "RefundTXResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "RefundTXResult")) + "</textarea></td></tr>");

                                            if (AppLogic.AppConfigBool("CardinalCommerce.Centinel.Enabled"))
                                            {
                                                sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardinalLookupResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "CardinalLookupResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "CardinalLookupResult")) + "</textarea></td></tr>");
                                                sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardinalAuthenticateResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "CardinalAuthenticateResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "CardinalAuthenticateResult")) + "</textarea></td></tr>");
                                                sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardinalGatewayParms", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "CardinalGatewayParms").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "CardinalGatewayParms")) + "</textarea></td></tr>");
                                            }
                                            else if (DB.RSField(rs2, "CardinalLookupResult").Length > 0)
                                            { // else if added 10/31/2006 for Cybersource and future 3D Secure data.
                                                sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.3DSecureLookupResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "CardinalLookupResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "CardinalLookupResult")) + "</textarea></td></tr>");
                                            }

                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.RecurringBillingSubscriptionCreateCommand", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + DB.RSField(rs2, "RecurringSubscriptionCommand") + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.RecurringBillingSubscriptionCreateResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + DB.RSField(rs2, "RecurringSubscriptionResult") + "</textarea></td></tr>");
                                        }
                                    }
                                    else if (PM == AppLogic.ro_PMCardinalMyECheck)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.TransactionID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "AuthorizationPNREF") + "</td></tr>");
                                        if (ThisCustomer.AdminCanViewCC)
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardinalLookupResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "CardinalLookupResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "CardinalLookupResult")) + "</textarea></td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CardinalAuthenticateResult", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><textarea READONLY rows=10 cols=60>" + CommonLogic.IIF(DB.RSField(rs2, "CardinalAuthenticateResult").Length == 0, AppLogic.ro_NotApplicable, DB.RSField(rs2, "CardinalAuthenticateResult")) + "</textarea></td></tr>");
                                        }
                                    }
                                    sb.Append("</table>\n");
                                    sb.Append("</div>\n");

                                    // --------------------------------------------------------------------------------------------------
                                    // SHIPPING DIV
                                    // --------------------------------------------------------------------------------------------------
                                    sb.Append("<div id=\"ShippingDiv_" + OrderNumber.ToString() + "\" name=\"ShippingDiv_" + OrderNumber.ToString() + "\" style=\"width: 100%; display:none;\">\n");

                                    if (TransactionType == AppLogic.TransactionTypeEnum.CHARGE || TransactionType == AppLogic.TransactionTypeEnum.RECURRING_AUTO)
                                    {
                                        sb.Append("<table align=\"left\" valign=\"top\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ShipTo", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + ShipAddr + "</td></tr>\n");
                                        sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ShippingMethod", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(HasShippableComponents, DB.RSFieldByLocale(rs2, "ShippingMethod", LocaleSetting), AppLogic.GetString("admin.orderframe.AllDownloadItems", SkinID, LocaleSetting)) + "</td></tr>\n");

                                        if (DB.RSFieldDateTime(rs2, "ShippedOn") == System.DateTime.MinValue && DB.RSFieldDateTime(rs2, "DownloadEmailSentOn") == System.DateTime.MinValue)
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.ReadyToShip", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"middle\">");
                                            if (DB.RSFieldBool(rs2, "ReadyToShip"))
                                            {
                                                sb.Append(AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting));
                                                if (!ord.HasBeenEdited)
                                                {
                                                    sb.Append("&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ClearReadyToShip", SkinID, LocaleSetting) + "\" name=\"ClearReadyToShip" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ClearReadyToShip(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                                }
                                            }
                                            else
                                            {
                                                sb.Append("No");
                                                if (HasShippableComponents && !ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                                {
                                                    sb.Append("&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.MarkAsReadyToShip", SkinID, LocaleSetting) + "\" name=\"MarkReadyToShip" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"MarkReadyToShip(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                                }
                                            }
                                            sb.Append("</td></tr>\n");
                                            sb.Append("<tr><td align=\"right\" valign=\"middle\">" + AppLogic.GetString("admin.orderframe.OrderWeight", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"middle\">");
                                            sb.Append(Localization.CurrencyStringForGatewayWithoutExchangeRate(ord.OrderWeight));
                                            sb.Append("</td></tr>\n");
                                        }

                                        if (HasShippableComponents)
                                        {
                                            if (ShippingStatus.IndexOf("<input") == -1) // don't show the form again!
                                            {
                                                sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                                sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ShippingStatus", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + ShippingStatus + "</td></tr>\n");
                                            }
                                            sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                            if (DB.RSField(rs2, "RTShipRequest").Length != 0)
                                            {
                                                sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.RTShipping", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><a target=\"_blank\" href=\"" + AppLogic.AdminLinkUrl("popuprt.aspx") + "?ordernumber=" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\">" + AppLogic.GetString("admin.orderframe.RTShippingInfo", SkinID, LocaleSetting) + "</a></td></tr>\n");
                                            }
                                        }
                                        if (ord.HasDownloadComponents(true) && ord.ThereAreDownloadFilesSpecified())
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasDownloadItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "</td></tr>\n");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.DownloadEmailSentOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(DB.RSFieldDateTime(rs2, "DownloadEMailSentOn") != System.DateTime.MinValue, Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs2, "DownloadEMailSentOn")), AppLogic.ro_NotApplicable));
                                            if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                            {
                                                sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + CommonLogic.IIF(DB.RSFieldDateTime(rs2, "DownloadEMailSentOn") != System.DateTime.MinValue, AppLogic.GetString("admin.orderframe.ReSendDownloadEmail", SkinID, LocaleSetting), AppLogic.GetString("admin.orderframe.SendDownloadEmail", SkinID, LocaleSetting)) + "\" name=\"Download" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"SendDownloadEMail(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            }
                                            sb.Append("</td></tr>\n");

                                        }
                                        else
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasDownloadItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AppLogic.GetString("admin.common.No", SkinID, LocaleSetting) + "</td></tr>\n");
                                        }
                                        if (ord.HasDistributorComponents())
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasDistributorDropShipItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + "</td></tr>\n");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.DistributorEmailSentOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(DB.RSFieldDateTime(rs2, "DistributorEMailSentOn") != System.DateTime.MinValue, Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs2, "DistributorEMailSentOn")), AppLogic.ro_NotApplicable));
                                            if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                            {
                                                sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + CommonLogic.IIF(DB.RSFieldDateTime(rs2, "DistributorEMailSentOn") != System.DateTime.MinValue, AppLogic.GetString("admin.orderframe.ReSendDistributorEmails", SkinID, LocaleSetting), AppLogic.GetString("admin.orderframe.SendDistributorEmails", SkinID, LocaleSetting)) + "\" name=\"Distributor" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"SendDistributorEMail(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                            }
                                            sb.Append("</td></tr>\n");

                                        }
                                        else
                                        {
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasDistributorItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">No</td></tr>\n");
                                        }

                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasMultipleShippingAddresses", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(ord.HasMultipleShippingAddresses(), AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting), AppLogic.GetString("admin.common.No", SkinID, LocaleSetting)) + "</td></tr>\n");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.HasGiftRegistryItems", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + CommonLogic.IIF(ord.HasGiftRegistryComponents(), AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting), AppLogic.GetString("admin.common.No", SkinID, LocaleSetting)) + "</td></tr>\n");

                                        if (_shipRushEnabledAndConfigured)
                                        {
                                            sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.ShipRush", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");
                                            if (DB.RSFieldDateTime(rs2, "ShippedOn") == System.DateTime.MinValue)
                                            {
                                                if (!ord.HasBeenEdited)
                                                {
                                                    sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ShipRushValue", SkinID, LocaleSetting) + "\" name=\"ShipRush" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ShipRushOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "," + CommonLogic.IIF(ord.TransactionIsCaptured(), "1", "0") + ");\">");
                                                }
                                            }
                                            else
                                            {
                                                sb.Append(AppLogic.GetString("admin.orderframe.SentToShipRushOn", SkinID, LocaleSetting) + " " + Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs2, "ShippedOn")));
                                            }
                                            sb.Append("</td></tr>");
                                        }
                                        if (AppLogic.AppConfigBool("FedExShipManager.Enabled"))
                                        {
                                            sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                            sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.FedExShipManager", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");
                                            if (DB.RSFieldDateTime(rs2, "ShippedOn") == System.DateTime.MinValue)
                                            {

                                                if (!ord.HasBeenEdited && (TransactionState == AppLogic.ro_TXStateAuthorized || TransactionState == AppLogic.ro_TXStateCaptured || TransactionState == AppLogic.ro_TXStatePending))
                                                {
                                                    sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.FedExShipManagerValue", SkinID, LocaleSetting) + "\" name=\"FedExShipManager" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"FedExShipManagerOrder(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "," + CommonLogic.IIF(ord.TransactionIsCaptured(), "1", "0") + ");\">");
                                                }
                                            }
                                            else
                                            {
                                                sb.Append(AppLogic.GetString("admin.orderframe.SentToFedExShipManagerOn", SkinID, LocaleSetting) + " " + Localization.ToThreadCultureShortDateString(DB.RSFieldDateTime(rs2, "ShippedOn")));
                                            }
                                            sb.Append("</td></tr>");
                                        }

                                        sb.Append("</table>\n");
                                    }
                                    else
                                    {
                                        sb.Append(AppLogic.ro_NotApplicable);
                                    }
                                    sb.Append("</div>\n");

                                    // --------------------------------------------------------------------------------------------------
                                    // CUSTOMER DIV
                                    // --------------------------------------------------------------------------------------------------
                                    sb.Append("<div id=\"CustomerDiv_" + OrderNumber.ToString() + "\" name=\"CustomerDiv_" + OrderNumber.ToString() + "\" style=\"width: 100%; display:none;\">\n");

                                    DateTime CustomerCreatedOn = System.DateTime.MinValue;

                                    using (SqlConnection dbconn5 = DB.dbConn())
                                    {
                                        dbconn5.Open();
                                        using (IDataReader rscc = DB.GetRS("Select CreatedOn from Customer with (NOLOCK) where CustomerID=" + DB.RSFieldInt(rs2, "CustomerID").ToString(), dbconn5))
                                        {
                                            if (rscc.Read())
                                            {
                                                CustomerCreatedOn = DB.RSFieldDateTime(rscc, "CreatedOn");
                                            }
                                        }
                                    }

                                    sb.Append("<table align=\"left\" valign=\"top\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\n");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.CustomerID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSFieldInt(rs2, "CustomerID").ToString() + "&nbsp;&nbsp;<a href=\"" + AppLogic.AdminLinkUrl("customerdetail.aspx") + "?customerid=" + DB.RSFieldInt(rs2, "CustomerID").ToString() + "\" target=\"content\">" + AppLogic.GetString("admin.common.Edit", SkinID, LocaleSetting) + "</a></td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.AffiliateID", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSFieldInt(rs2, "AffiliateID").ToString() + "</td></tr>\n");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.CustomerName", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + (DB.RSField(rs2, "FirstName") + " " + DB.RSField(rs2, "LastName")).Trim() + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CustomerPhone", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + DB.RSField(rs2, "Phone") + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.CreatedOn", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + Localization.ToThreadCultureShortDateString(CustomerCreatedOn) + "</td></tr>");
                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.CustomerEmail", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\"><a href=\"mailto:" + DB.RSField(rs2, "EMail") + "?subject=" + AppLogic.GetString("admin.orderframe.Re", SkinID, LocaleSetting) + " " + AppLogic.AppConfig("StoreName") + " " + AppLogic.GetString("admin.common.Order", SkinID, LocaleSetting) + " " + ord.OrderNumber.ToString() + "\">" + DB.RSField(rs2, "EMail") + "</a>");
                                    if (!ord.HasBeenEdited && TransactionState != AppLogic.ro_TXStateFraud)
                                    {
                                        sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"text\" name=\"NewEMail\" size=\"25\" maxlength=\"100\">&nbsp;<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.orderframe.ChangeOrderEmail", SkinID, LocaleSetting) + "\" name=\"ChangeOrderEMail" + DB.RSFieldInt(rs2, "OrderNumber").ToString() + "\" onClick=\"ChangeOrderEMail(OrderDetailForm," + DB.RSFieldInt(rs2, "OrderNumber").ToString() + ");\">");
                                    }
                                    sb.Append("</td></tr>");
                                    if (NumOrders > 1)
                                    {
                                        sb.Append("<tr><td colspan=2>&nbsp;</td></tr>");
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.OrderHistory", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");
                                        sb.Append("<a target=\"content\" href=\"" + AppLogic.AdminLinkUrl("cst_history.aspx") + "?customerid=" + DB.RSFieldInt(rs2, "CustomerID").ToString() + "\">");
                                        for (int i = 1; i <= NumOrders; i++)
                                        {
                                            sb.Append("<img src=\"" + AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/smile.gif") + "\" border=\"0\" align=\"absmiddle\">");
                                            if (i % 10 == 0)
                                            {
                                                sb.Append("<br/>");
                                            }
                                        }
                                        sb.Append("</a>");
                                        sb.Append("</td></tr>");
                                    }
                                    if (DB.RSField(rs2, "Referrer").Length != 0)
                                    {
                                        sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.orderframe.Referrer", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">");
                                        if (DB.RSField(rs2, "Referrer").StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            sb.Append("<a href=\"" + DB.RSField(rs2, "Referrer") + "\" target=\"_blank\">");
                                            sb.Append(DB.RSField(rs2, "Referrer"));
                                            sb.Append("</a>");
                                        }
                                        sb.Append("</td></tr>");
                                    }

                                    sb.Append("<tr><td align=\"right\" valign=\"top\">" + AppLogic.GetString("admin.order.Affiliate", SkinID, LocaleSetting) + "&nbsp;</td><td align=\"left\" valign=\"top\">" + AffiliateHelper.GetEntityName(DB.RSFieldInt(rs2, "AffiliateID"), LocaleSetting) + "</td></tr>");
                                    sb.Append("</table>");
                                    sb.Append("</div>\n");

                                    // --------------------------------------------------------------------------------------------------
                                    // NOTES DIV
                                    // --------------------------------------------------------------------------------------------------
                                    sb.Append("<div id=\"NotesDiv_" + OrderNumber.ToString() + "\" name=\"NotesDiv_" + OrderNumber.ToString() + "\" style=\"width: 100%; display:none;\">\n");
                                    sb.Append("<p></p>");

                                    sb.Append("<p><b>" + AppLogic.GetString("admin.orderframe.FinalizationData", SkinID, LocaleSetting) + "</b></p>");
                                    sb.Append("<textarea readonly rows=\"10\" name=\"FinalizationData\" cols=\"120\">" + XmlCommon.PrettyPrintXml(DB.RSField(rs2, "FinalizationData")) + "</textarea><br/>\n");

                                    sb.Append("<p><b>" + AppLogic.GetString("admin.orderframe.OrderNotes", SkinID, LocaleSetting) + "</b></p>");
                                    sb.Append("<textarea readonly rows=\"10\" name=\"OrderNotes\" cols=\"120\">" + Server.HtmlEncode(DB.RSField(rs2, "OrderNotes")) + "</textarea><br/>\n");
                                    sb.Append("<p></p>");

                                    sb.Append("<p><b>" + AppLogic.GetString("admin.orderframe.PrivateNotes", SkinID, LocaleSetting) + "</b></p>");
                                    sb.Append("<textarea rows=\"10\" name=\"Notes\" cols=\"120\">" + Server.HtmlEncode(DB.RSField(rs2, "Notes")) + "</textarea><br/>\n");
                                    sb.Append("<p></p>");

                                    sb.Append("<p><b>" + AppLogic.GetString("admin.orderframe.CustomerServiceNotes", SkinID, LocaleSetting) + " " + CommonLogic.IIF(AppLogic.AppConfigBool("ShowCustomerServiceNotesInReceipts"), AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting), AppLogic.GetString("admin.common.No", SkinID, LocaleSetting)) + ", " + AppLogic.GetString("admin.orderframe.EditableHere", SkinID, LocaleSetting) + " " + AppLogic.GetString("admin.common.Yes", SkinID, LocaleSetting) + ")</b></p>");
                                    sb.Append("<textarea rows=\"20\" name=\"CustomerServiceNotes\" cols=\"120\">" + DB.RSField(rs2, "CustomerServiceNotes") + "</textarea><br/>\n");

                                    sb.Append("<input  class=\"normalButtons\" type=\"button\" value=\"" + AppLogic.GetString("admin.common.Submit", SkinID, LocaleSetting) + "\" onClick=\"document.OrderDetailForm.SubmitAction.value='UPDATENOTES';document.OrderDetailForm.submit();\">\n");

                                    sb.Append("</div>\n");

                                    // --------------------------------------------------------------------------------------------------
                                    // RECEIPT DIV
                                    // --------------------------------------------------------------------------------------------------
                                    sb.Append("<div id=\"ReceiptDiv_" + OrderNumber.ToString() + "\" name=\"ReceiptDiv_" + OrderNumber.ToString() + "\" style=\"width: 100%; display:none;\">\n");
                                    sb.Append("<p><b><a href=\"javascript:window.print();\">" + AppLogic.GetString("admin.orderframe.PrintReceipt", SkinID, LocaleSetting) + "</a>&nbsp;&nbsp;&nbsp;&nbsp;or&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"" + ReceiptURLPrintable + "\" target=\"_blank\">" + AppLogic.GetString("admin.orderframe.OpenNewReceiptWindowHere", SkinID, LocaleSetting) + "</a></b></p>");
                                    sb.Append(CommonLogic.ExtractBody(ord.Receipt(ThisCustomer, false)));
                                    sb.Append("</div>\n");

                                    // --------------------------------------------------------------------------------------------------
                                    // XML DIV
                                    // --------------------------------------------------------------------------------------------------
                                    // We write an empty div even if not a SuperUser so that all the javascript references to the XMLDiv don't fail.
                                    sb.Append("<div id=\"XMLDiv_" + OrderNumber.ToString() + "\" name=\"XMLDiv_" + OrderNumber.ToString() + "\" style=\"width: 100%; display:none;\">\n");
                                    if (ThisCustomer.IsAdminSuperUser)
                                    {
                                        String OrderXml = AppLogic.RunXmlPackage("DumpOrder", null, ThisCustomer, SkinID, "", "OrderNumber=" + OrderNumber.ToString(), false, true);
                                        sb.Append("<textarea READONLY rows=50 cols=120>" + XmlCommon.PrettyPrintXml(OrderXml) + "</textarea>");

                                    }
                                    sb.Append("</div>\n");

                                    // --------------------------------------------------------------------------------------------------
                                    // DISTRIBUTOR DIV
                                    // --------------------------------------------------------------------------------------------------
                                    if (ord.HasDistributorComponents())
                                    {
                                        sb.Append("<div id=\"DistributorDiv_" + OrderNumber.ToString() + "\" name=\"DistributorDiv_" + OrderNumber.ToString() + "\" style=\"width: 100%; display:none;\">\n");
                                        sb.Append(AppLogic.GetAllDistributorNotifications(ord));
                                        sb.Append("</div>\n");
                                    }

                                    sb.Append("</td></tr>\n");
                                    sb.Append("</table>\n");


                                    sb.Append("<script type=\"text/javascript\">\n");

                                    sb.Append("function ShowDiv_" + OrderNumber.ToString() + "(name)\n");
                                    sb.Append("{\n");
                                    sb.Append("	document.getElementById(name + 'Div_" + OrderNumber.ToString() + "').style.display='block';\n");
                                    sb.Append("	document.getElementById(name + 'TD_" + OrderNumber.ToString() + "').className = 'LightTab';\n");
                                    sb.Append("	return (true);\n");
                                    sb.Append("}\n");

                                    sb.Append("function HideDiv_" + OrderNumber.ToString() + "(name)\n");
                                    sb.Append("{\n");
                                    sb.Append("	document.getElementById(name + 'Div_" + OrderNumber.ToString() + "').style.display='none';\n");
                                    sb.Append("	return (true);\n");
                                    sb.Append("}\n");


                                    sb.Append("function ShowGeneralDiv_" + OrderNumber.ToString() + "()\n");
                                    sb.Append("{\n");
                                    sb.Append("	ShowDiv_" + OrderNumber.ToString() + "('General');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Billing');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Shipping');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Customer');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Notes');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Receipt');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('XML');\n");
                                    if (ord.HasDistributorComponents())
                                    {
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Distributor');\n");
                                    }
                                    sb.Append("	return (true);\n");
                                    sb.Append("}\n");

                                    sb.Append("function ShowBillingDiv_" + OrderNumber.ToString() + "()\n");
                                    sb.Append("{\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('General');\n");
                                    sb.Append("	ShowDiv_" + OrderNumber.ToString() + "('Billing');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Shipping');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Customer');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Notes');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Receipt');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('XML');\n");
                                    if (ord.HasDistributorComponents())
                                    {
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Distributor');\n");
                                    }
                                    sb.Append("	return (true);\n");
                                    sb.Append("}\n");

                                    sb.Append("function ShowShippingDiv_" + OrderNumber.ToString() + "()\n");
                                    sb.Append("{\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('General');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Billing');\n");
                                    sb.Append("	ShowDiv_" + OrderNumber.ToString() + "('Shipping');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Customer');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Notes');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Receipt');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('XML');\n");
                                    if (ord.HasDistributorComponents())
                                    {
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Distributor');\n");
                                    }
                                    sb.Append("	return (true);\n");
                                    sb.Append("}\n");

                                    sb.Append("function ShowCustomerDiv_" + OrderNumber.ToString() + "()\n");
                                    sb.Append("{\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('General');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Billing');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Shipping');\n");
                                    sb.Append("	ShowDiv_" + OrderNumber.ToString() + "('Customer');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Notes');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Receipt');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('XML');\n");
                                    if (ord.HasDistributorComponents())
                                    {
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Distributor');\n");
                                    }
                                    sb.Append("	return (true);\n");
                                    sb.Append("}\n");

                                    sb.Append("function ShowNotesDiv_" + OrderNumber.ToString() + "()\n");
                                    sb.Append("{\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('General');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Billing');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Shipping');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Customer');\n");
                                    sb.Append("	ShowDiv_" + OrderNumber.ToString() + "('Notes');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Receipt');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('XML');\n");
                                    if (ord.HasDistributorComponents())
                                    {
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Distributor');\n");
                                    }
                                    sb.Append("	return (true);\n");
                                    sb.Append("}\n");

                                    sb.Append("function ShowReceiptDiv_" + OrderNumber.ToString() + "()\n");
                                    sb.Append("{\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('General');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Billing');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Shipping');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Customer');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Notes');\n");
                                    sb.Append("	ShowDiv_" + OrderNumber.ToString() + "('Receipt');\n");
                                    sb.Append("	HideDiv_" + OrderNumber.ToString() + "('XML');\n");
                                    if (ord.HasDistributorComponents())
                                    {
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Distributor');\n");
                                    }
                                    sb.Append("	return (true);\n");
                                    sb.Append("}\n");

                                    if (ThisCustomer.IsAdminSuperUser)
                                    {
                                        sb.Append("function ShowXMLDiv_" + OrderNumber.ToString() + "()\n");
                                        sb.Append("{\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('General');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Billing');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Shipping');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Customer');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Notes');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Receipt');\n");
                                        sb.Append("	ShowDiv_" + OrderNumber.ToString() + "('XML');\n");
                                        if (ord.HasDistributorComponents())
                                        {
                                            sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Distributor');\n");
                                        }
                                        sb.Append("	return (true);\n");
                                        sb.Append("}\n");
                                    }

                                    if (ord.HasDistributorComponents())
                                    {
                                        sb.Append("function ShowDistributorDiv_" + OrderNumber.ToString() + "()\n");
                                        sb.Append("{\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('General');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Billing');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Shipping');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Customer');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Notes');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('Receipt');\n");
                                        sb.Append("	HideDiv_" + OrderNumber.ToString() + "('XML');\n");
                                        sb.Append("	ShowDiv_" + OrderNumber.ToString() + "('Distributor');\n");
                                        sb.Append("	return (true);\n");
                                        sb.Append("}\n");
                                    }

                                    sb.Append("Show" + InitialTab + "Div_" + OrderNumber.ToString() + "();\n");

                                    sb.Append("</script>\n");
                                }
                            }

                            ord = null;

                            sb.Append("<script type=\"text/javascript\">\n");

                            if (IsPayPal)
                            {
                                sb.Append("function PayPalReauthOrder(theForm,id)\n");
                                sb.Append("{\n");
                                sb.Append("window.open('" + AppLogic.AdminLinkUrl("paypalreauthorder.aspx") + "?ordernumber=' + id,'PayPalReauthOrder" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=500,left=0,top=0');\n");
                                sb.Append("}\n");
                            }

                            sb.Append("function MarkAsShipped(theForm,id,transactioniscaptured)\n");
                            sb.Append("{\n");
                            sb.Append("var oktoproceed = true;\n");
                            sb.Append("if(transactioniscaptured == 0)\n");
                            sb.Append("{\n");
                            sb.Append("oktoproceed = confirm('" + AppLogic.GetString("admin.orderframe.OkToProceedMessage", SkinID, LocaleSetting) + "');\n");
                            sb.Append("}\n");
                            sb.Append("if(oktoproceed)\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'MARKASSHIPPED';\n");
                            sb.Append("theForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function MarkAsPrinted(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'MARKASPRINTED';\n");
                            sb.Append("theForm.submit();\n");
                            sb.Append("}\n");

                            sb.Append("function DownloadExportXML(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'DOWNLOADEXPORTXML';\n");
                            sb.Append("theForm.submit();\n");
                            sb.Append("}\n");

                            sb.Append("function MarkReadyToShip(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'MARKREADYTOSHIP';\n");
                            sb.Append("theForm.submit();\n");
                            sb.Append("}\n");

                            sb.Append("function ClearReadyToShip(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'CLEARREADYTOSHIP';\n");
                            sb.Append("theForm.submit();\n");
                            sb.Append("}\n");

                            sb.Append("function ClearNew(theForm,id,transactioniscaptured)\n");
                            sb.Append("{\n");
                            sb.Append("var oktoproceed = true;\n");
                            sb.Append("if(transactioniscaptured == 0)\n");
                            sb.Append("{\n");
                            sb.Append("oktoproceed = confirm('" + AppLogic.GetString("admin.orderframe.OkToProceedMessage", SkinID, LocaleSetting) + "');\n");
                            sb.Append("}\n");
                            sb.Append("if(oktoproceed)\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'CLEARNEW';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function GetTransactionStatus(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("amazontransaction.aspx") + "?ordernumber=' + id,'GetTransaction" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=500,left=0,top=0');\n");
                            sb.Append("}\n");
                            sb.Append("function RefundOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("refundorder.aspx") + "?ordernumber=' + id,'RefundOrder" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=500,left=0,top=0');\n");
                            sb.Append("}\n");

                            sb.Append("function CaptureOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("captureorder.aspx") + "?ordernumber=' + id,'CaptureOrder" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=500,left=0,top=0');\n");
                            sb.Append("}\n");

                            sb.Append("function VoidOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("voidorder.aspx") + "?ordernumber=' + id,'VoidOrder" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=500,left=0,top=0');\n");
                            sb.Append("}\n");

                            sb.Append("function ForceVoidOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("voidorder.aspx") + "?ordernumber=' + id + '&ForceVoid=1','ForceVoidOrder" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=500,left=0,top=0');\n");
                            sb.Append("}\n");

                            sb.Append("function AdHocChargeOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("adhoccharge.aspx") + "?ordernumber=' + id,'AdHocOrder" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=650,left=0,top=0');\n");
                            sb.Append("}\n");

                            sb.Append("function CancelRecurringBilling(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("recurringrefundcancel.aspx") + "?ordernumber=' + id,'CancelRecurringBilling" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=650,left=0,top=0');\n");
                            sb.Append("}\n");

                            sb.Append("function AdjustChargeOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("adjustcharge.aspx") + "?ordernumber=' + id,'AdjustChargeOrder" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,copyhistory=no,width=600,height=500,left=0,top=0');\n");
                            sb.Append("}\n");

                            if (_shipRushEnabledAndConfigured)
                            {

                                sb.Append("function ShipRushOrder(theForm,id,transactioniscaptured)\n");
                                sb.Append("{\n");
                                sb.Append("var oktoproceed = true;\n");
                                sb.Append("if(transactioniscaptured == 0)\n");
                                sb.Append("{\n");
                                sb.Append("oktoproceed = confirm('" + AppLogic.GetString("admin.orderframe.OkToProceedMessage", SkinID, LocaleSetting) + "');\n");
                                sb.Append("}\n");
                                sb.Append("if(oktoproceed)\n");
                                sb.Append("{\n");
                                sb.Append("document.OrderDetailForm.SubmitAction.value = 'SHIPRUSH';\n");
                                sb.Append("theForm.submit();\n");
                                sb.Append("}\n");
                                sb.Append("}\n");
                            }
                            if (AppLogic.AppConfigBool("FedExShipManager.Enabled"))
                            {

                                sb.Append("function FedExShipManagerOrder(theForm,id,transactioniscaptured)\n");
                                sb.Append("{\n");
                                sb.Append("var oktoproceed = true;\n");
                                sb.Append("if(transactioniscaptured == 0)\n");
                                sb.Append("{\n");
                                sb.Append("oktoproceed = confirm('" + AppLogic.GetString("admin.orderframe.OkToProceedMessage", SkinID, LocaleSetting) + "');\n");
                                sb.Append("}\n");
                                sb.Append("if(oktoproceed)\n");
                                sb.Append("{\n");
                                sb.Append("document.OrderDetailForm.SubmitAction.value = 'FEDEXSHIPMANAGER';\n");
                                sb.Append("theForm.submit();\n");
                                sb.Append("}\n");
                                sb.Append("}\n");
                            }

                            if (ShippingStatus.IndexOf("Shipped On: <input") != -1)
                            {
                                sb.Append("    Calendar.setup({\n");
                                sb.Append("        inputField     :    \"ShippedOn\",      // id of the input field\n");
                                sb.Append("        ifFormat       :    \"" + Localization.JSCalendarDateFormatSpec() + "\",       // format of the input field\n");
                                sb.Append("        showsTime      :    false,            // will display a time selector\n");
                                sb.Append("        button         :    \"f_trigger_s\",   // trigger for the calendar (button ID)\n");
                                sb.Append("        singleClick    :    true            // Single-click mode\n");
                                sb.Append("    });\n");
                            }

                            sb.Append("function EditOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.orderframe.QueryEdit", SkinID, LocaleSetting), DB.RSFieldInt(rs, "OrderNumber").ToString()) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("parent.location='" + AppLogic.AdminLinkUrl("phoneorder.aspx") + "?ordernumber=' + id");

                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function MarkAsFraudOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.orderframe.QueryMark", SkinID, LocaleSetting), DB.RSFieldInt(rs, "OrderNumber").ToString()) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'MARKASFRAUD';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function ClearAsFraudOrder(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.orderframe.QueryRemoveFraud", SkinID, LocaleSetting), DB.RSFieldInt(rs, "OrderNumber").ToString()) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'CLEARASFRAUD';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function BlockIP(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + AppLogic.GetString("admin.orderframe.QueryBlockIP", SkinID, LocaleSetting) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'BLOCKIP';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function GetMaxmind(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'GETMAXMIND';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");

                            sb.Append("function AllowIP(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + AppLogic.GetString("admin.orderframe.QueryUnblockIP", SkinID, LocaleSetting) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'ALLOWIP';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function ChangeOrderEMail(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.orderframe.QueryChange", SkinID, LocaleSetting), DB.RSFieldInt(rs, "OrderNumber").ToString()) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'CHANGEORDEREMAIL';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function AdjustOrderWeight(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'ADJUSTORDERWEIGHT';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");

                            sb.Append("function ForceRefund(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.orderframe.QueryForceRefund", SkinID, LocaleSetting), DB.RSFieldInt(rs, "OrderNumber").ToString()) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'FORCEREFUND';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function SendDownloadEMail(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.orderframe.QuerySendDownloadEmail", SkinID, LocaleSetting), DB.RSFieldInt(rs, "OrderNumber").ToString()) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'SENDDOWNLOADEMAIL';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function ReleaseDownload(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append(string.Format("if(confirm('{0}'))\n", AppLogic.GetString("admin.orderframe.QueryReleaseDownload", SkinID, LocaleSetting)));
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'RELEASEITEM' + id;\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function SendDistributorEMail(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.orderframe.QuerySendDistributorEmail", SkinID, LocaleSetting), DB.RSFieldInt(rs, "OrderNumber").ToString()) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'SENDDISTRIBUTOREMAIL';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");
                            sb.Append("function SendReceiptEMail(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('" + String.Format(AppLogic.GetString("admin.orderframe.QuerySendReceiptEmail", SkinID, LocaleSetting), DB.RSFieldInt(rs, "OrderNumber").ToString()) + "'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'SENDRECEIPTEMAIL';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function RegenerateReceipt(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("if(confirm('Are you sure you want to regenerate this receipt?'))\n");
                            sb.Append("{\n");
                            sb.Append("document.OrderDetailForm.SubmitAction.value = 'REGENERATERECEIPT';\n");
                            sb.Append("document.OrderDetailForm.submit();\n");
                            sb.Append("}\n");
                            sb.Append("}\n");

                            sb.Append("function SetOrderNotes(theForm,id)\n");
                            sb.Append("{\n");
                            sb.Append("window.open('" + AppLogic.AdminLinkUrl("editordernotes.aspx") + "?OrderNumber=' + id,\"AspDotNetStorefront_ML" + CommonLogic.GetRandomNumber(1, 10000).ToString() + "\",\"height=300,width=630,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
                            sb.Append("}\n");

                            sb.Append("</SCRIPT>\n");
                        }
                    }
                }
            }
            ltBody.Text = sb.ToString();
        }


        private string StoreLine(int StoreID)
        {
            return string.Format(

            "<tr><td align='right' valign='top'>{0}:&nbsp;</td><td align='left' valign='top'>{1}</td></tr>\n",
            "Store",
            string.Format("{0} - {1}", StoreID, AppLogic.AppConfig("StoreName", StoreID, true))
            );
        }
    }
}
