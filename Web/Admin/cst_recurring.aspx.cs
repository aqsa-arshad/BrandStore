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
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for cst_recurring.
    /// </summary>
    public partial class cst_recurring : AdminPageBase 
    {

        private Customer TargetCustomer;
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");


            TargetCustomer = new Customer(CommonLogic.QueryStringUSInt("CustomerID"), true);
            if (TargetCustomer.CustomerID == 0)
            {
                AppLogic.AdminLinkUrl("Customers.aspx");   
            }
            if (TargetCustomer.IsAdminSuperUser && !ThisCustomer.IsAdminSuperUser)
            {
                throw new ArgumentException(AppLogic.GetString("admin.common.SecurityException", SkinID, LocaleSetting));
            } 
            if (CommonLogic.QueryStringUSInt("DeleteID") != 0)
            {
                DB.ExecuteSQL("delete from ShoppingCart where CustomerID=" + TargetCustomer.CustomerID.ToString() + " and ShoppingCartRecID=" + CommonLogic.QueryStringUSInt("DeleteID").ToString());
                DB.ExecuteSQL("delete from kitcart where CustomerID=" + TargetCustomer.CustomerID.ToString() + " and ShoppingCartRecID=" + CommonLogic.QueryStringUSInt("DeleteID").ToString());
            }

            if (CommonLogic.FormUSInt("OriginalRecurringOrderNumber") != 0)
            {
                int OriginalRecurringOrderNumber = CommonLogic.FormUSInt("OriginalRecurringOrderNumber");
                int NewRecurringInterval = CommonLogic.FormUSInt("RecurringInterval");
                DateIntervalTypeEnum NewRecurringIntervalType = (DateIntervalTypeEnum)CommonLogic.FormUSInt("RecurringIntervalType");

                DateTime SetNextShipDate = System.DateTime.MinValue;
                if (CommonLogic.FormCanBeDangerousContent("NextRecurringShipDate").Length != 0)
                {
                    try
                    {
                        SetNextShipDate = CommonLogic.FormNativeDateTime("NextRecurringShipDate");
                    }
                    catch { }
                    if (SetNextShipDate != System.DateTime.MinValue && OriginalRecurringOrderNumber != 0)
                    {
                        DB.ExecuteSQL(String.Format("update shoppingcart set NextRecurringShipDate={0} where customerid={1} and originalrecurringordernumber={2}", DB.DateQuote(Localization.ToDBShortDateString(SetNextShipDate)), TargetCustomer.CustomerID.ToString(), OriginalRecurringOrderNumber.ToString()));
                    }
                }

                if (CommonLogic.FormUSInt("RecurringInterval") != 0)
                {
                    DateTime CreatedOnDate = System.DateTime.MinValue;
                    DateTime LastRecurringShipDate = System.DateTime.MinValue;
                    int RecurringIndex = 1;
                    int CurrentRecurringInterval = 0;
                    DateIntervalTypeEnum CurrentRecurringIntervalType = DateIntervalTypeEnum.Monthly;

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs2 = DB.GetRS("select CreatedOn, NextRecurringShipDate,RecurringIndex,RecurringInterval,RecurringIntervalType from ShoppingCart   with (NOLOCK)  where CustomerID=" + TargetCustomer.CustomerID.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and OriginalRecurringOrderNumber=" + OriginalRecurringOrderNumber.ToString(), dbconn))
                        {
                            if (rs2.Read())
                            {
                                CurrentRecurringInterval = DB.RSFieldInt(rs2, "RecurringInterval");
                                CurrentRecurringIntervalType = (DateIntervalTypeEnum)DB.RSFieldInt(rs2, "RecurringIntervalType");
                                RecurringIndex = DB.RSFieldInt(rs2, "RecurringIndex");
                                CreatedOnDate = DB.RSFieldDateTime(rs2, "CreatedOn");
                                LastRecurringShipDate = DB.RSFieldDateTime(rs2, "NextRecurringShipDate"); // this must be "fixed" up below...we need the PRIOR ship date, not the date of next schedule ship
                            }
                        }
                    }

                    LastRecurringShipDate = System.DateTime.Now;

                    DateTime NewShipDate = System.DateTime.MinValue;
                    if (LastRecurringShipDate != System.DateTime.MinValue)
                    {
                        switch (CurrentRecurringIntervalType)
                        {
                            case DateIntervalTypeEnum.Day:
                                NewShipDate = LastRecurringShipDate.AddDays(NewRecurringInterval);
                                break;
                            case DateIntervalTypeEnum.Week:
                                NewShipDate = LastRecurringShipDate.AddDays(7 * NewRecurringInterval);
                                break;
                            case DateIntervalTypeEnum.Month:
                                NewShipDate = LastRecurringShipDate.AddMonths(NewRecurringInterval);
                                break;
                            case DateIntervalTypeEnum.Year:
                                NewShipDate = LastRecurringShipDate.AddYears(NewRecurringInterval);
                                break;
                            case DateIntervalTypeEnum.Weekly:
                                NewShipDate = LastRecurringShipDate.AddDays(7);
                                break;
                            case DateIntervalTypeEnum.BiWeekly:
                                NewShipDate = LastRecurringShipDate.AddDays(14);
                                break;                            
                            case DateIntervalTypeEnum.EveryFourWeeks:
                                NewShipDate = LastRecurringShipDate.AddDays(28);
                                break;
                            case DateIntervalTypeEnum.Monthly:
                                NewShipDate = LastRecurringShipDate.AddMonths(1);
                                break;
                            case DateIntervalTypeEnum.Quarterly:
                                NewShipDate = LastRecurringShipDate.AddMonths(3);
                                break;
                            case DateIntervalTypeEnum.SemiYearly:
                                NewShipDate = LastRecurringShipDate.AddMonths(6);
                                break;
                            case DateIntervalTypeEnum.Yearly:
                                NewShipDate = LastRecurringShipDate.AddYears(1);
                                break;
                            default:
                                NewShipDate = LastRecurringShipDate.AddMonths(NewRecurringInterval);
                                break;
                        }
                        DB.ExecuteSQL("update ShoppingCart set RecurringInterval=" + NewRecurringInterval.ToString() + ", RecurringIntervalType=" + ((int)NewRecurringIntervalType).ToString() + ", NextRecurringShipDate=" + DB.DateQuote(Localization.ToDBDateTimeString(NewShipDate)) + " where CustomerID=" + TargetCustomer.CustomerID.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and OriginalRecurringOrderNumber=" + OriginalRecurringOrderNumber.ToString());
                    }
                }
            }

            SectionTitle = "<a href=\"" + AppLogic.AdminLinkUrl("Customers.aspx") + "?searchfor=" + TargetCustomer.CustomerID.ToString() + "\">" + AppLogic.GetString("admin.menu.Customers", SkinID, LocaleSetting) + "</a> - <a href=\"" + AppLogic.AdminLinkUrl("cst_history.aspx") + "?customerid=" + TargetCustomer.CustomerID.ToString() + "\">" + AppLogic.GetString("admin.cst_recurring.OrderHistory", SkinID, LocaleSetting) + "</a> - " + AppLogic.GetString("admin.cst_recurring.RecurringShipmentsFor", SkinID, LocaleSetting) + " " + TargetCustomer.FullName() + " (" + TargetCustomer.EMail + ")";
            RenderMarkup();
        }


        private void RenderMarkup()
        {
            System.Text.StringBuilder output = new System.Text.StringBuilder();

            if (ShoppingCart.NumItems(TargetCustomer.CustomerID, CartTypeEnum.RecurringCart) == 0)
            {
                output.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.cst_recurring.NoActiveRecurringOrdersFound", SkinID, LocaleSetting) + "</b></p>\n");
            }
            else
            {
                output.Append("<p align=\"left\"><b>" + AppLogic.GetString("admin.common.CstMsg9", SkinID, LocaleSetting) + "</b></p>\n");

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rsr = DB.GetRS("Select distinct OriginalRecurringOrderNumber from ShoppingCart   with (NOLOCK)  where CartType=" + ((int)CartTypeEnum.RecurringCart).ToString() + " and CustomerID=" + TargetCustomer.CustomerID.ToString() + " order by OriginalRecurringOrderNumber desc", dbconn))
                    {
                        while (rsr.Read())
                        {
                            output.Append(AppLogic.GetRecurringCart(EntityHelpers, GetParser, TargetCustomer, DB.RSFieldInt(rsr, "OriginalRecurringOrderNumber"), SkinID, false));
                        }
                    }
                }
            }
            ltPageContents.Text = output.ToString();
        }

    }
}
