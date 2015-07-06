// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class viewshipment : AdminPageBase
    {
        string sql;
        protected void Page_Load(object sender, EventArgs e)
        {
            litImportErrors.Text = "";
            if (ViewState["SortOrder"] == null)
            {
                ViewState["SortOrder"] = "ASC";
            }
            if (ViewState["Sort"] == null)
            {
                ViewState["Sort"] = "OrderNumber";
            }
            String dashSeperatedImportedOrderNumbers = "";
            if (Session["dashSeperatedImportedOrderNumbers"] != null)
            {
                dashSeperatedImportedOrderNumbers = Session["dashSeperatedImportedOrderNumbers"] as string;
            }
            Dictionary<Int32, Boolean> OrderNumbers = OrderNumbersFromDashSeperatedString(dashSeperatedImportedOrderNumbers);
            if (OrderNumbers.Count == 0)
            {
                litImportErrors.Text = "<strong style=\"color:red;\">No orders imported.</strong>";
                return;
            }

            sql = "SELECT o.OrderNumber, o.ShippingTrackingNumber, o.ShippedOn, o.CustomerID, o.FirstName + ' ' + o.LastName AS Name, o.Email, o.ShippingFirstName + ' ' + o.ShippingLastName AS ShippingName, o.ShippingCompany, o.ShippingAddress1, o.ShippingCity, o.ShippingState, o.ShippingZip, o.ShippingCountry, o.ShippingPhone, o.OrderSubtotal, o.OrderTax, o.OrderShippingCosts, o.OrderTotal, o.OrderDate, o.OrderWeight  ";
            sql += "FROM Orders o left join ( select ordernumber, count(distinct shippingaddressid) addresses from orders_shoppingcart group by ordernumber having count(distinct shippingaddressid) > 1) a on o.ordernumber = a.ordernumber ";
            sql += "WHERE o.OrderNumber in ("+OrderNumbersToCSV(OrderNumbers)+") AND ReadyToShip = 1 AND ShippedOn IS NOT NULL and TransactionState in (" + DB.SQuote(AppLogic.ro_TXStateAuthorized) + "," + DB.SQuote(AppLogic.ro_TXStateCaptured) + ") and a.ordernumber is null";

            using (DataTable dt = new DataTable())
            {
                using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(sql, conn))
                    {
                        dt.Load(rs);
                        dview.DataSource = dt;
                        dview.DataBind();
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (dr["OrderNumber"] == null)
                                break;

                            Int32 rowOrderNumber;
                            string OrderNumberString = dr["OrderNumber"].ToString();
                            if (Int32.TryParse(OrderNumberString, out rowOrderNumber) && OrderNumbers.ContainsKey(rowOrderNumber))
                                OrderNumbers[rowOrderNumber] = true;
                            
                        }
                    }
                }
            }
            SetImportErrors(OrderNumbers);
            Session["dashSeperatedImportedOrderNumbers"] = null;
        }

        private void SetImportErrors(Dictionary<Int32, Boolean> OrderNumbers)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<Int32, Boolean> kvp in OrderNumbers)
            {
                if (!kvp.Value)
                {
                    sb.Append(kvp.Key.ToString() + ",");
                }
            }
            if (sb.Length > 0)
            {
                string missedOrderNumbers = sb.ToString();
                litImportErrors.Text = "<strong style=\"color:red;\">Orders not imported: "+missedOrderNumbers.Substring(0, missedOrderNumbers.Length - 1) +".</strong>";
            }
        }

        private string OrderNumbersToCSV(Dictionary<Int32, Boolean> orderNumbers)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<Int32, Boolean> on in orderNumbers)
            {
                sb.Append(on.Key.ToString() + ",");
            }
            sb.Append("0");
            return sb.ToString();
        }

        private Dictionary<Int32, Boolean> OrderNumbersFromDashSeperatedString(string orders)
        {
            Dictionary<Int32, Boolean> ret = new Dictionary<Int32, Boolean>();
            string[] ordernumbers = orders.Split(new char[]{'-'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in ordernumbers)
            {
                Int32 OrderNumber;
                if (Int32.TryParse(s, out OrderNumber))
                {
                    ret.Add(OrderNumber, false);
                }
            }
            return ret;
        }
    }
}
