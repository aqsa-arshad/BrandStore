// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for setlast4
    /// </summary>
    public partial class setlast4 : AdminPageBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            StringBuilder writer = new StringBuilder();
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Server.ScriptTimeout = 30000;

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            if (!ThisCustomer.IsAdminSuperUser)
            {
                throw new ArgumentException("Permission Denied, Must Be SuperAdmin");
            }
            writer.Append("Updating Last4 fields of all order records...<br/>");
            Response.Flush();
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select OrderNumber, CardNumber from orders  with (NOLOCK)  where CardNumber IS NOT NULL and (Last4 IS NULL or Last4='') order by ordernumber", con))
                {
                    while (rs.Read())
                    {
                        int ONX = DB.RSFieldInt(rs, "OrderNumber");

                        writer.Append("Checking order " + ONX.ToString() + "...");
                        Response.Flush();

                        String CardNumber = AppLogic.AdminViewCardNumber(DB.RSField(rs, "CardNumber"), "Orders", DB.RSFieldInt(rs, "OrderNumber"));
                        if (CardNumber.Length != 0)
                        {
                            String Last4 = AppLogic.SafeDisplayCardNumberLast4(CardNumber, "Orders", DB.RSFieldInt(rs, "OrderNumber"));
                            if (Last4.Length != 0)
                            {
                                DB.ExecuteSQL("update orders set Last4=" + DB.SQuote(Last4) + " where OrderNumber=" + ONX.ToString() + "");
                                writer.Append("update orders set Last4=" + DB.SQuote(Last4) + " where OrderNumber=" + ONX.ToString());
                            }
                            else
                            {
                                writer.Append("Last4 could not be set");
                            }
                        }
                        else
                        {
                            writer.Append("CardNumber not found");
                        }
                        writer.Append("<br/>");
                        Response.Flush();
                    }
                }
            }
            writer.Append("done<br/>");
            ltContent.Text = writer.ToString();
        }
    }
}
