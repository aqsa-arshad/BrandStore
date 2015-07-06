// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Xml;

namespace AspDotNetStorefrontAdmin
{

    public class ThubService : Page
    {
        private string myConnectionString = "Provider=SQLOLEDB;" + DB.GetDBConn();
        private bool enableLogging = false; // You can change it to make logging on/off

        private void ShowError(string str, int code, Hashtable ht)
        {
            StringWriter streamw = new StringWriter();
            XmlTextWriter w = new XmlTextWriter(streamw);
            w.WriteStartDocument();
            w.WriteStartElement("RESPONSE");
            w.WriteAttributeString("version", "", "3.92");
            w.WriteStartElement("Envelope");

            AddXmlEl(w, "Command", ht["Command"].ToString());
            AddXmlEl(w, "StatusCode", code.ToString());
            AddXmlEl(w, "StatusMessage", str);
            AddXmlEl(w, "RequestID", ht["RequestID"].ToString());
            string provider = ht["Provider"] == null ? "" : ht["Provider"].ToString();
            AddXmlEl(w, "Provider", provider);

            w.WriteEndElement();
            w.WriteEndDocument();
            w.Close();
            Response.Clear();
            Response.ContentType = "text/xml";
            String tmpstr = streamw.ToString().Replace("encoding=\"utf-16\"?>", "encoding=\"utf-8\"?>");
            Response.Write(tmpstr);
            logging("Response " + '\r' + '\r' + tmpstr.Replace("><", ">" + '\r' + "<") + '\r' + '\r');
        }

        private void logging(string str)
        {
            if (enableLogging == true)
            {
                StreamWriter myWriter = null;
                string scriptname = Request.ServerVariables["PATH_TRANSLATED"].Replace("THubService.aspx", "");
                string filename = Localization.ToNativeDateTimeString(System.DateTime.Now);
                filename = filename.Replace(" ", "_");
                filename = filename.Replace(":", "_");
                filename = filename.Replace("/", "_");
                myWriter = File.CreateText(scriptname + "logs/" + filename + ".txt");
                myWriter.WriteLine("Request" + '\r' + '\r' + Request.Form["request"].Replace("><", ">" + '\r' + "<") + '\r' + '\r');
                myWriter.WriteLine(str);
                myWriter.Close();
            }
        }
        private void AddXmlEl(XmlTextWriter w, string name, string value)
        {
            w.WriteStartElement(name);
            w.WriteString(value);
            w.WriteEndElement();
        }
        private String AddZero(int a)
        {
            String tempAddZero = null;
            if (a < 10)
                tempAddZero = "0" + a;
            else
                tempAddZero = a.ToString();

            return tempAddZero;
        }


        //----------------------------------------------------------------------------------------------------------------------------------------
        //----------   GetOrders
        //----------------------------------------------------------------------------------------------------------------------------------------
        private void GetOrders(Hashtable ht, SqlDataReader reader1)
        {
            StringWriter streamw = new StringWriter();
            XmlTextWriter w = new XmlTextWriter(streamw);

            w.Formatting = Formatting.Indented;
            w.Indentation = 5;
            w.WriteStartDocument();
            w.WriteStartElement("RESPONSE");
            w.WriteAttributeString("version", "", "3.92");
            w.WriteStartElement("Envelope");
            AddXmlEl(w, "Command", ht["Command"].ToString());
            AddXmlEl(w, "StatusCode", "0");
            AddXmlEl(w, "StatusMessage", "All OK");
            AddXmlEl(w, "RequestID", ht["RequestID"].ToString());
            AddXmlEl(w, "Provider", "Value from Request");
            w.WriteEndElement(); //Envelop
            w.WriteStartElement("Orders");

            while (reader1.Read())
            {
                w.WriteStartElement("Order");
                AddXmlEl(w, "OrderID", reader1["OrderNumber"].ToString());
                AddXmlEl(w, "ProviderOrderRef", reader1["OrderNumber"].ToString());
                AddXmlEl(w, "CustomerID", reader1["CustomerID"].ToString().Trim());
                string dt = reader1["OrderDate"].ToString();
                AddXmlEl(w, "Date", System.Convert.ToDateTime(dt).Year + "-" + AddZero(System.Convert.ToDateTime(dt).Month) + "-" + AddZero(System.Convert.ToDateTime(dt).Day));
                AddXmlEl(w, "Time", AddZero(System.Convert.ToDateTime(dt).Hour) + ":" + AddZero(System.Convert.ToDateTime(dt).Minute) + ":" + AddZero(System.Convert.ToDateTime(dt).Second));
                AddXmlEl(w, "TimeZone", "");
                AddXmlEl(w, "StoreID", GetConfigValue("StoreName"));
                AddXmlEl(w, "StoreName", GetConfigValue("StoreName"));
                AddXmlEl(w, "Comment", HttpContext.Current.Server.HtmlEncode(reader1["OrderNotes"].ToString().Trim()));
                AddXmlEl(w, "Currency", GetConfigValue("Localization.StoreCurrency"));
                w.WriteStartElement("Bill");
                AddXmlEl(w, "PayMethod", reader1["PaymentMethod"].ToString().Trim());
                string PayStatus = "Pending";
                string TransactionType = "Sale";
                string ParentOrderNumber = "";
                
                if (reader1["TransactionState"].ToString() == AppLogic.ro_TXStateRefunded)
                {
                	PayStatus = "Refunded";
                	TransactionType = "Return";
                	ParentOrderNumber = reader1["ParentOrderNumber"].ToString();
              	} 
              	else if (reader1["TransactionState"].ToString() == AppLogic.ro_TXStateCaptured)
                {
                    PayStatus = "Cleared";
                }

                AddXmlEl(w, "PayStatus", PayStatus);
                AddXmlEl(w, "TransactionType", TransactionType);

                AddXmlEl(w, "FirstName", reader1["BillingFirstName"].ToString().Trim());
                AddXmlEl(w, "LastName", reader1["BillingLastName"].ToString().Trim());
                AddXmlEl(w, "MiddleName", "");
                AddXmlEl(w, "CompanyName", reader1["BillingCompany"].ToString().Trim());

                AddXmlEl(w, "Address1", reader1["BillingAddress1"].ToString().Trim());
                AddXmlEl(w, "Address2", (reader1["BillingAddress2"].ToString().Trim() + " " + reader1["BillingSuite"].ToString().Trim()).Trim());
                AddXmlEl(w, "City", reader1["BillingCity"].ToString().Trim());
                AddXmlEl(w, "State", reader1["BillingState"].ToString().Trim());
                AddXmlEl(w, "Zip", reader1["BillingZip"].ToString().Trim());
                AddXmlEl(w, "Country", reader1["BillingCountry"].ToString().Trim());
                AddXmlEl(w, "Email", reader1["Email"].ToString().Trim());
                AddXmlEl(w, "Phone", reader1["BillingPhone"].ToString().Trim());
                
                if (reader1["TransactionState"].ToString() == AppLogic.ro_TXStateRefunded)
                {
	              	AddXmlEl(w, "PONumber", ParentOrderNumber);
              	} else {
	              	AddXmlEl(w, "PONumber", reader1["PONumber"].ToString().Trim());
              	} 
                
                if (reader1["CardType"].ToString().Trim() != "" & reader1["CardExpirationYear"].ToString().Trim().Length != 0 & reader1["CardExpirationMonth"].ToString().Trim().Length != 0)
                {
                    w.WriteStartElement("CreditCard");
                    AddXmlEl(w, "CreditCardType", reader1["CardType"].ToString().Trim());
                    AddXmlEl(w, "CreditCardCharge", "");
                    AddXmlEl(w, "ExpirationDate", reader1["CardExpirationMonth"].ToString().Trim() + "/" + reader1["CardExpirationYear"].ToString().Trim());

                    AddXmlEl(w, "CreditCardName", reader1["CardName"].ToString().Trim());

                    string ccNum = "";
                    ccNum = Security.UnmungeString(reader1["CardNumber"].ToString().Trim(), Order.StaticGetSaltKey(DB.RSFieldInt(reader1, "OrderNumber")));
                    if (ccNum.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ccNum = reader1["CardNumber"].ToString().Trim();
                    }

                    AddXmlEl(w, "CreditCardNumber", ccNum);
                    AddXmlEl(w, "AuthDetails", "AuthCode=" + reader1["AuthorizationCode"].ToString().Trim() + ";TransId=" + reader1["AuthorizationResult"].ToString().Trim() + ";AVSCode=" + reader1["AVSResult"].ToString().Trim()); //AuthCode=Q31234;TransId=4423412312;AVSCode=P
                    AddXmlEl(w, "ReconciliationData", reader1["AuthorizationResult"].ToString().Trim());
                    w.WriteEndElement(); //CreditCard
                }
                w.WriteEndElement(); //Bill
                w.WriteStartElement("Ship");

                string shipMethodDb = reader1["ShippingMethod"].ToString().Trim();
                string shipMethodParsed = "";

                string[] arInfo = new string[1];

                // define which character is seperating fields
                char[] splitter = { '|' };

                arInfo = shipMethodDb.Split(splitter);
                shipMethodParsed = arInfo[0];
                AddXmlEl(w, "ShipMethod", shipMethodParsed);


                AddXmlEl(w, "FirstName", reader1["ShippingFirstName"].ToString().Trim());
                AddXmlEl(w, "LastName", reader1["ShippingLastName"].ToString().Trim());
                AddXmlEl(w, "CompanyName", reader1["ShippingCompany"].ToString().Trim());
                AddXmlEl(w, "Address1", reader1["ShippingAddress1"].ToString().Trim());
                AddXmlEl(w, "Address2", (reader1["ShippingAddress2"].ToString().Trim() + " " + reader1["ShippingSuite"].ToString().Trim()).Trim());
                AddXmlEl(w, "City", reader1["ShippingCity"].ToString().Trim());
                AddXmlEl(w, "State", reader1["ShippingState"].ToString().Trim());
                AddXmlEl(w, "Zip", reader1["ShippingZip"].ToString().Trim());
                AddXmlEl(w, "Country", reader1["ShippingCountry"].ToString().Trim());
                AddXmlEl(w, "Email", reader1["Email"].ToString().Trim());
                AddXmlEl(w, "Phone", reader1["ShippingPhone"].ToString().Trim());
                w.WriteEndElement();
                w.WriteStartElement("Charges");
                AddXmlEl(w, "Shipping", reader1["OrderShippingCosts"].ToString().Trim());
                AddXmlEl(w, "Handling", "");
                AddXmlEl(w, "Tax", reader1["OrderTax"].ToString().Trim());
                AddXmlEl(w, "Fee", "");
                AddXmlEl(w, "TaxOther", "0");
                w.WriteStartElement("FeeDetails");
                AddXmlEl(w, "FeeDetail", "");
                AddXmlEl(w, "FeeName", "");
                AddXmlEl(w, "FeeValue", "");
                w.WriteEndElement();
                AddXmlEl(w, "Discount", reader1["CouponDiscountAmount"].ToString().Trim());
                AddXmlEl(w, "DiscountPercent", reader1["CouponDiscountPercent"].ToString().Trim());
                AddXmlEl(w, "Total", reader1["OrderTotal"].ToString().Trim());
                w.WriteEndElement();

                if (reader1["CouponCode"].ToString().Trim() != "")
                {
                    w.WriteStartElement("Coupon");
                    AddXmlEl(w, "CouponCode", reader1["CouponCode"].ToString().Trim());
                    AddXmlEl(w, "CouponID", reader1["CouponCode"].ToString().Trim());
                    AddXmlEl(w, "CouponDescription", reader1["CouponDescription"].ToString().Trim());
                    AddXmlEl(w, "CouponPercent", reader1["CouponDiscountPercent"].ToString().Trim());
                    AddXmlEl(w, "CouponValue", reader1["CouponDiscountAmount"].ToString().Trim());
                    w.WriteEndElement();
                }
                w.WriteStartElement("Items");


                string myQuery2 = "SELECT shc.ProductID, shc.OrderedProductSKU, shc.ShoppingCartRecID, shc.OrderedProductName AS OrderedProductName, shc.OrderedProductVariantName, shc.Quantity, shc.OrderedProductRegularPrice, shc.OrderedProductPrice, shc.ChosenColor, shc.ChosenSize, shc.TextOption, pv.Name AS VariantName, pv.Weight FROM Orders_ShoppingCart shc INNER JOIN ProductVariant pv ON shc.VariantID = pv.VariantID WHERE shc.OrderNumber = " + reader1["OrderNumber"];

                try
                {
                    using (SqlConnection conn2 = DB.dbConn())
                    {
                        conn2.Open();
                        using (IDataReader reader2 = DB.GetRS(myQuery2, conn2))
                        {
                            while (reader2.Read())
                            {
                                w.WriteStartElement("Item");
                                AddXmlEl(w, "ItemCode", reader2["OrderedProductSKU"].ToString().Trim());
                                string itemD = "";
                                string itemCodeBase = "";
                                string itemCodeAlternate = "";

                                itemCodeBase = "";
                                itemCodeAlternate = "";
                                if (reader2["VariantName"].ToString().Trim() != "")
                                {
                                    itemCodeBase = reader2["VariantName"].ToString().Trim();
                                    itemCodeAlternate = reader2["VariantName"].ToString().Trim();
                                }

                                if (itemCodeAlternate != "")
                                {
                                    if (reader2["ChosenColor"].ToString().Trim() != "")
                                    {
                                        itemCodeAlternate = itemCodeAlternate + "-" + reader2["ChosenColor"].ToString().Trim();
                                    }
                                    if (reader2["ChosenSize"].ToString().Trim() != "")
                                    {
                                        itemCodeAlternate = itemCodeAlternate + "-" + reader2["ChosenSize"].ToString().Trim();
                                    }
                                }
                                AddXmlEl(w, "ItemCodeBase", itemCodeBase);
                                AddXmlEl(w, "ItemCodeAlternate", itemCodeAlternate);


                                itemD = reader2["OrderedProductName"].ToString().Trim();
                                if (reader2["OrderedProductVariantName"].ToString().Trim() != "")
                                {
                                    itemD = itemD + "-" + reader2["OrderedProductVariantName"].ToString().Trim();
                                }

                                if (reader2["TextOption"].ToString().Trim() != "")
                                {
                                    itemD = itemD + ":Customization Text=" + reader2["TextOption"].ToString().Trim();
                                }

                                AddXmlEl(w, "ItemDescription", itemD);

                                AddXmlEl(w, "Quantity", reader2["Quantity"].ToString().Trim());
                                AddXmlEl(w, "UnitPrice", "");
                                AddXmlEl(w, "ItemTotal", reader2["OrderedProductPrice"].ToString().Trim());
                                AddXmlEl(w, "ItemUnitWeight", reader2["Weight"].ToString().Trim());
                                AddXmlEl(w, "CustomField1", "");
                                AddXmlEl(w, "CustomField2", "");
                                AddXmlEl(w, "CustomField3", "");
                                AddXmlEl(w, "CustomField4", "");
                                AddXmlEl(w, "CustomField5", "");
                                w.WriteStartElement("ItemOptions");
                                w.WriteStartElement("ItemOption");
                                w.WriteAttributeString("Name", "", "ChosenColor");
                                w.WriteAttributeString("Value", "", reader2["ChosenColor"].ToString().Trim());
                                w.WriteEndElement();
                                w.WriteStartElement("ItemOption");
                                w.WriteAttributeString("Name", "", "ChosenSize");
                                w.WriteAttributeString("Value", "", reader2["ChosenSize"].ToString().Trim());
                                w.WriteEndElement();

                                string myQuery3 = "" + "select KitGroupName, KitItemName " + "from Orders_KitCart  " + "where OrderNumber = " + reader1["OrderNumber"] + " and ShoppingCartRecID=" + reader2["ShoppingCartRecID"];
                                try
                                {
                                    using (SqlConnection conn3 = DB.dbConn())
                                    {
                                        conn3.Open();
                                        using (IDataReader reader3 = DB.GetRS(myQuery3, conn3))
                                        {
                                            while (reader3.Read())
                                            {
                                                w.WriteStartElement("ItemOption");
                                                w.WriteAttributeString("Name", "", "KitItem");
                                                w.WriteAttributeString("Value", "", reader3["KitGroupName"].ToString().Trim() + "/" + reader3["KitItemName"].ToString().Trim());
                                                w.WriteEndElement();
                                            }
                                        }
                                    }
                                }
                                catch (Exception err3)
                                {
                                    ShowError("Error reading Item Options." + err3.Message, 9999, ht);
                                }                           

                                w.WriteEndElement(); //ItemOptions
                                w.WriteEndElement(); //Item                           
                            }
                        }
                    }
                    

                }
                catch (Exception err2)
                {
                    ShowError("Error reading Items of order." + err2.Message + " -- " + myQuery2, 9999, ht);
                }
                w.WriteEndElement(); //Items
                w.WriteEndElement(); //Order

            }

            w.WriteEndElement(); //Orders
            w.WriteEndElement(); //Response
            w.WriteEndDocument();
            w.Close();
            Response.Clear();
            Response.ContentType = "text/xml";
            String tmpstr = streamw.ToString().Replace("encoding=\"utf-16\"?>", "encoding=\"utf-8\"?>");
            Response.Write(tmpstr);
            logging("Response " + '\r' + '\r' + tmpstr.Replace("><", ">" + '\r' + "<") + '\r' + '\r');
        }

        //{{{{{{{{{{{{{{{{{{{{{{{{{   UpdateOrders    {{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{
        private void UpdateOrders(Hashtable ht, string req)
        {
            SqlCommand cmd;
            StringReader stream1 = new StringReader(req);
            XmlTextReader r = new XmlTextReader(stream1);
            Hashtable myHT1 = new Hashtable();
            Hashtable hostOrderHT = new Hashtable();
            Hashtable LocalOrderIDHT = new Hashtable();
            Hashtable LocalOrderRefHT = new Hashtable();
            Hashtable resOfUpdate = new Hashtable();
            string curName = null;
            int index1 = 0;
            if (req == "")
                ShowError("Request is empty.", 9999, ht);

            // if ErrCode=0 then 
            // get variables from xml request
            index1 = -1;
            while (r.Read())
            {
                if (r.Name != "" & r.NodeType == XmlNodeType.Element)
                {
                    curName = r.Name.Trim();
                    r.Read();
                    if (r.NodeType == XmlNodeType.Text)
                        myHT1[curName] = r.Value.Trim();
                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "HOSTORDERID")
                    {
                        index1 += 1;
                        hostOrderHT[index1] = r.Value.Trim();
                    }
                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "LOCALORDERREF")
                        LocalOrderRefHT[index1] = r.Value.Trim();
                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "LOCALORDERID")
                        LocalOrderIDHT[index1] = r.Value.Trim();
                }
            }
            r.Close();

            int i = 0;
            int failCount = 0;
            failCount = 0;
            int tempFor1 = index1;
            for (i = 0; i <= tempFor1; i++)
            {
                string myQuery = "UPDATE Orders SET IsNew = 0, " + "THUB_POSTED_TO_ACCOUNTING = 'Y', thub_posted_date = getdate()" + ", thub_accounting_ref = '" + LocalOrderRefHT[i] + "' where OrderNumber=" + hostOrderHT[i];

                SqlConnection dbconn = new SqlConnection();
                dbconn.ConnectionString = DB.GetDBConn();
                cmd = new SqlCommand(myQuery, dbconn);

                try
                {
                    dbconn.Open();
                    resOfUpdate[i] = cmd.ExecuteNonQuery();
                    if (System.Int32.Parse(resOfUpdate[i].ToString()) == 0)
                        failCount += 1;
                }
                catch
                {
                    resOfUpdate[i] = -1;
                    failCount += 1;
                }
                finally
                {
                    if (dbconn != null)
                        dbconn.Close();
                }
            }

            StringWriter streamw = new StringWriter();
            XmlTextWriter w = new XmlTextWriter(streamw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 5;
            w.WriteStartDocument();
            w.WriteStartElement("RESPONSE");
            w.WriteAttributeString("version", "", "3.92");
            w.WriteStartElement("Envelope");
            AddXmlEl(w, "Command", ht["Command"].ToString());
            AddXmlEl(w, "StatusCode", "0");
            AddXmlEl(w, "StatusMessage", "Successful Count=" + (index1 - failCount + 1) + ";Failed Count=" + failCount);
            AddXmlEl(w, "RequestID", ht["RequestID"].ToString());
            string provider = ht["Provider"] == null ? "" : ht["Provider"].ToString();
            AddXmlEl(w, "Provider", provider);
            w.WriteEndElement();
            w.WriteStartElement("Orders");
            int tempFor2 = index1;
            for (i = 0; i <= tempFor2; i++)
            {
                w.WriteStartElement("Order");
                AddXmlEl(w, "HostOrderID", hostOrderHT[i].ToString());
                AddXmlEl(w, "LocalOrderID", LocalOrderIDHT[i].ToString());
                AddXmlEl(w, "LocalOrderRef", LocalOrderRefHT[i].ToString());
                if (System.Int32.Parse(resOfUpdate[i].ToString()) > 0)
                    AddXmlEl(w, "HostStatus", "Success");
                else
                    AddXmlEl(w, "HostStatus", "Fail");
                w.WriteEndElement();
            }
            w.WriteEndElement();
            w.WriteEndElement();
            w.WriteEndDocument();
            w.Close();
            Response.Clear();
            Response.ContentType = "text/xml";
            String tmpstr = streamw.ToString().Replace("encoding=\"utf-16\"?>", "encoding=\"utf-8\"?>");
            Response.Write(tmpstr);
            logging("Response " + '\r' + '\r' + tmpstr.Replace("><", ">" + '\r' + "<") + '\r' + '\r');

        } //Update Orders
        //-------------------------------------------------------------------------------------------------------------------------------

        //{{{{{{{{{{{{{{{{{{{{{{{{{   updateOrdersShippingStatus    {{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{
        private void updateOrdersShippingStatus(Hashtable ht, string req)
        {
            SqlCommand cmd;
            StringReader stream1 = new StringReader(req);
            XmlTextReader r = new XmlTextReader(stream1);
            Hashtable myHT1 = new Hashtable();
            Hashtable hostOrderHT = new Hashtable();
            Hashtable LocalOrderIDHT = new Hashtable();
            Hashtable ShippedOnHT = new Hashtable();
            Hashtable ShippedViaHT = new Hashtable();
            Hashtable TrackingNumberHT = new Hashtable();
            Hashtable resOfUpdate = new Hashtable();
            string curName = null;
            int index1 = 0;
            if (req == "")
                ShowError("Request is empty.", 9999, ht);

            // if ErrCode=0 then 
            // get variables from xml request
            index1 = -1;
            while (r.Read())
            {
                if (r.Name != "" & r.NodeType == XmlNodeType.Element)
                {
                    curName = r.Name.Trim();
                    r.Read();
                    if (r.NodeType == XmlNodeType.Text)
                    {
                        myHT1[curName] = r.Value.Trim();
                    }

                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "HOSTORDERID")
                    {
                        index1 += 1;
                        hostOrderHT[index1] = r.Value.Trim();
                    }

                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "LOCALORDERID")
                    {
                        LocalOrderIDHT[index1] = r.Value.Trim();
                    }

                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "SHIPPEDON")
                    {
                        ShippedOnHT[index1] = r.Value.Trim();
                    }

                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "SHIPPEDVIA")
                    {
                        ShippedViaHT[index1] = r.Value.Trim();
                    }

                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "TRACKINGNUMBER")
                    {
                        TrackingNumberHT[index1] = r.Value.Trim();
                    }
                }
            }
            r.Close();

            int i = 0;
            int failCount = 0;
            failCount = 0;
            int tempFor1 = index1;
            for (i = 0; i <= tempFor1; i++)
            {
                string myQuery = "UPDATE Orders SET IsNew = 0, " + "ShippedOn = '" + ShippedOnHT[i] + "', ShippedVIA = '" + ShippedViaHT[i] + "', " + "ShippingTrackingNumber = '" + TrackingNumberHT[i] + "' where OrderNumber=" + hostOrderHT[i];
                SqlConnection dbconn = new SqlConnection();
                dbconn.ConnectionString = DB.GetDBConn();
                cmd = new SqlCommand(myQuery, dbconn);
                try
                {
                    dbconn.Open();
                    resOfUpdate[i] = cmd.ExecuteNonQuery();
                    if (System.Int32.Parse(resOfUpdate[i].ToString()) == 0)
                        failCount += 1;

                    if (AppLogic.AppConfigBool("BulkImportSendsShipmentNotifications"))
                    {
                        Order.MarkOrderAsShipped(int.Parse(hostOrderHT[i].ToString()), ShippedViaHT[i].ToString(), TrackingNumberHT[i].ToString(), Localization.ParseDBDateTime(ShippedOnHT[i].ToString()), false, null, new Parser(null, 1, null), false);
                    }
                    else
                    {
                        Order.MarkOrderAsShipped(int.Parse(hostOrderHT[i].ToString()), ShippedViaHT[i].ToString(), TrackingNumberHT[i].ToString(), Localization.ParseDBDateTime(ShippedOnHT[i].ToString()), false, null, new Parser(null, 1, null), true);
                    }

                }
                catch
                {
                    resOfUpdate[i] = -1;
                    failCount += 1;
                }
                finally
                {
                    if (dbconn != null)
                        dbconn.Close();
                }
            }

            StringWriter streamw = new StringWriter();
            XmlTextWriter w = new XmlTextWriter(streamw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 5;
            w.WriteStartDocument();
            w.WriteStartElement("RESPONSE");
            w.WriteAttributeString("version", "", "3.92");
            w.WriteStartElement("Envelope");
            AddXmlEl(w, "Command", ht["Command"].ToString());
            AddXmlEl(w, "StatusCode", "0");
            AddXmlEl(w, "StatusMessage", "Successful Count=" + (index1 - failCount + 1) + ";Failed Count=" + failCount);
            AddXmlEl(w, "RequestID", ht["RequestID"].ToString());
            w.WriteEndElement();
            w.WriteStartElement("Orders");
            int tempFor2 = index1;
            for (i = 0; i <= tempFor2; i++)
            {
                w.WriteStartElement("Order");
                AddXmlEl(w, "HostOrderID", hostOrderHT[i].ToString());
                AddXmlEl(w, "LocalOrderID", (LocalOrderIDHT[i] == null) ? "" : LocalOrderIDHT[i].ToString());
                if (System.Int32.Parse(resOfUpdate[i].ToString()) > 0)
                    AddXmlEl(w, "HostStatus", "Success");
                else
                    AddXmlEl(w, "HostStatus", "Fail");
                w.WriteEndElement();
            }
            w.WriteEndElement();
            w.WriteEndElement();
            w.WriteEndDocument();
            w.Close();
            Response.Clear();
            Response.ContentType = "text/xml";
            String tmpstr = streamw.ToString().Replace("encoding=\"utf-16\"?>", "encoding=\"utf-8\"?>");
            Response.Write(tmpstr);
            logging("Response " + '\r' + '\r' + tmpstr.Replace("><", ">" + '\r' + "<") + '\r' + '\r');

        } //updateOrdersShippingStatus
        //-------------------------------------------------------------------------------------------------------------------------------

        //{{{{{{{{{{{{{{{{{{{{{{{{{   updateOrdersPaymentStatus    {{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{
        private void updateOrdersPaymentStatus(Hashtable ht, string req)
        {
            SqlCommand cmd;
            StringReader stream1 = new StringReader(req);
            XmlTextReader r = new XmlTextReader(stream1);
            Hashtable myHT1 = new Hashtable();
            Hashtable hostOrderHT = new Hashtable();
            Hashtable LocalOrderIDHT = new Hashtable();
            Hashtable PaymentStatusHT = new Hashtable();
            Hashtable ClearedOnHT = new Hashtable();
            Hashtable resOfUpdate = new Hashtable();
            string curName = null;
            int index1 = 0;
            if (req == "")
                ShowError("Request is empty.", 9999, ht);

            // if ErrCode=0 then 
            // get variables from xml request
            index1 = -1;
            while (r.Read())
            {
                if (r.Name != "" & r.NodeType == XmlNodeType.Element)
                {
                    curName = r.Name.Trim();
                    r.Read();
                    if (r.NodeType == XmlNodeType.Text)
                        myHT1[curName] = r.Value.Trim();
                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "HOSTORDERID")
                    {
                        index1 += 1;
                        hostOrderHT[index1] = r.Value.Trim();
                    }
                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "LOCALORDERID")
                        LocalOrderIDHT[index1] = r.Value.Trim();
                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "CLEAREDON")
                        ClearedOnHT[index1] = r.Value.Trim();
                    if (r.NodeType == XmlNodeType.Text & curName.ToUpper() == "PAYMENTSTATUS")
                        PaymentStatusHT[index1] = r.Value.Trim().ToUpper();
                }
            }
            r.Close();

            StringWriter streamw = new StringWriter();
            XmlTextWriter w = new XmlTextWriter(streamw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 5;
            w.WriteStartDocument();
            w.WriteStartElement("RESPONSE");
            w.WriteAttributeString("version", "", "3.92");
            w.WriteStartElement("Envelope");



            int i = 0;
            int failCount = 0;
            failCount = 0;
            int tempFor1 = index1;
            for (i = 0; i <= tempFor1; i++)
            {
                string myQuery = String.Empty;
                if (PaymentStatusHT[i].ToString() == "CLEARED")
                {
                    Gateway.ProcessOrderAsCaptured(System.Int32.Parse(hostOrderHT[i].ToString()));
                }

                if (PaymentStatusHT[i].ToString() == "DECLINED")
                {
                    myQuery = "UPDATE Orders SET TransactionState=" + DB.SQuote(AppLogic.ro_TXStateVoided) + " where OrderNumber=" + hostOrderHT[i];
                }

                if (PaymentStatusHT[i].ToString() == "FAILED")
                {
                    myQuery = "UPDATE Orders SET TransactionState=" + DB.SQuote(AppLogic.ro_TXStateVoided) + " where OrderNumber=" + hostOrderHT[i];
                }

                SqlConnection dbconn = new SqlConnection();
                dbconn.ConnectionString = DB.GetDBConn();
                cmd = new SqlCommand(myQuery, dbconn);

                try
                {
                    dbconn.Open();
                    resOfUpdate[i] = cmd.ExecuteNonQuery();
                    if (System.Int32.Parse(resOfUpdate[i].ToString()) == 0)
                        failCount += 1;
                }
                catch
                {
                    resOfUpdate[i] = -1;
                    failCount += 1;
                }
                finally
                {
                    if (dbconn != null)
                        dbconn.Close();
                }
            }

            AddXmlEl(w, "Command", ht["Command"].ToString());
            AddXmlEl(w, "StatusCode", "0");
            AddXmlEl(w, "StatusMessage", "Successful Count=" + (index1 - failCount + 1) + ";Failed Count=" + failCount);
            AddXmlEl(w, "RequestID", ht["RequestID"].ToString());
            w.WriteEndElement();
            w.WriteStartElement("Orders");
            int tempFor2 = index1;
            for (i = 0; i <= tempFor2; i++)
            {
                w.WriteStartElement("Order");
                AddXmlEl(w, "HostOrderID", hostOrderHT[i].ToString());
                AddXmlEl(w, "LocalOrderID", LocalOrderIDHT[i].ToString());
                if (System.Int32.Parse(resOfUpdate[i].ToString()) > 0)
                    AddXmlEl(w, "HostStatus", "Success");
                else
                    AddXmlEl(w, "HostStatus", "Fail");
                w.WriteEndElement();
            }
            w.WriteEndElement();
            w.WriteEndElement();
            w.WriteEndDocument();
            w.Close();
            Response.Clear();
            Response.ContentType = "text/xml";
            String tmpstr = streamw.ToString().Replace("encoding=\"utf-16\"?>", "encoding=\"utf-8\"?>");
            Response.Write(tmpstr);
            logging("Response " + '\r' + '\r' + tmpstr.Replace("><", ">" + '\r' + "<") + '\r' + '\r');

        } //updateOrdersPaymentStatus
        //-------------------------------------------------------------------------------------------------------------------------------

        //{{{{{{{{{{{{{{{{{{{{{{{{{   UpdateInventory    {{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{{
        private void UpdateInventory(Hashtable ht, string req)
        {
            SqlCommand cmd;
            //SqlDataReader reader = null;
            bool updateInventory;
            bool updatePrice;
            bool listInventory;
            XmlDocument requestDoc;
            XmlDocument responseDoc;
            XmlElement responseRootNode;
            XmlElement responseEnvelopeNode;
            XmlElement responseCommandNode;
            XmlElement responseStatusCodeNode;
            XmlElement responseStatusMessageNode;
            XmlElement responseRequestIDNode;
            XmlElement responseProviderNode;
            XmlAttribute responseAttr;
            int totalItems = 0;
            int failedItems = 0;
            Hashtable skuVsVariantId = new Hashtable();

            requestDoc = new XmlDocument();
            requestDoc.LoadXml(req);

            responseDoc = new XmlDocument();
            responseRootNode = responseDoc.CreateElement("RESPONSE");
            responseDoc.AppendChild(responseRootNode);
            responseAttr = responseDoc.CreateAttribute("version");
            responseAttr.Value = "3.92";
            responseDoc.DocumentElement.Attributes.Append(responseAttr);

            responseEnvelopeNode = responseDoc.CreateElement("Envelope");
            responseRootNode.AppendChild(responseEnvelopeNode);

            responseCommandNode = responseDoc.CreateElement("Command");
            responseCommandNode.AppendChild(responseDoc.CreateTextNode(ht["Command"].ToString()));
            responseEnvelopeNode.AppendChild(responseCommandNode);

            updateInventory = ht["UpdateInventory"] == null ? false : ht["UpdateInventory"].ToString() == "1";
            updatePrice = ht["UpdatePrice"] == null ? false : ht["UpdatePrice"].ToString() == "1";
            listInventory = ht["InventoryList"] == null ? false : ht["InventoryList"].ToString() == "1";
            if (updateInventory)
            {
                #region Update Inventory

                XmlElement requestItemsNode;
                XmlElement responseItemsNode;
                XmlElement responseItemNode;
                XmlElement responseItemNumberNode;
                XmlElement responseItemCodeNode;
                XmlElement responseItemNameNode;
                XmlElement responseItemOptionNode;
                XmlElement responseQuantityInStockWebNode;

                responseItemsNode = responseDoc.CreateElement("Items");
                responseRootNode.AppendChild(responseItemsNode);

                requestItemsNode = (XmlElement)requestDoc.DocumentElement.SelectSingleNode("./Items");
                foreach (XmlElement requestItemNode in requestItemsNode.ChildNodes)
                {
                    string requestSKU;
                    string requestSKUParent;
                    string variantId = "";
                    string productName = "";
                    string color = "";
                    string size = "";
                    string inventoryQuantity = "0";
                    string inventoryUpdateStatus = "2";
                    string inventoryUpdateStatusMsg = "";
                    string productPrice = "";
                    string productSalePrice = "";
                    totalItems++;

                    requestSKU = requestItemNode.SelectSingleNode("./ItemCode").FirstChild.InnerText;
                    requestSKUParent = "";

                    try
                    {
                        requestSKUParent = requestItemNode.SelectSingleNode("./ItemCodeParent").FirstChild.InnerText;
                    }
                    catch
                    {
                        requestSKUParent = "";
                    }

                    try
                    {
                        productPrice = requestItemNode.SelectSingleNode("./Price").FirstChild.InnerText;
                    }
                    catch
                    {
                        productPrice = "";
                    }

                    try
                    {
                        productSalePrice = requestItemNode.SelectSingleNode("./SalePrice").FirstChild.InnerText;
                    }
                    catch
                    {
                        productSalePrice = "";
                    }


                    int found = 0;
                    if (!skuVsVariantId.Contains(requestSKU))
                    {
                        string query;
                        found = 0;

                        try
                        {
                            query = "SELECT V.VariantID, P.ProductID, P.SKU + V.SKUSuffix AS SKU, P.ProductID, V.Colors, V.Sizes, P.Name + ' ' + V.Name AS ProductName FROM ProductVariant AS V INNER JOIN Product AS P ON V.ProductID=P.ProductID WHERE (P.SKU + V.SKUSuffix)='" + requestSKU + "'";
                            using (SqlConnection conn = DB.dbConn())
                            {
                                conn.Open();
                                using (IDataReader dr = DB.GetRS(query, conn))
                                {
                                    if (dr.Read())
                                    {
                                        variantId = dr["VariantID"].ToString();
                                        skuVsVariantId[requestSKU] = variantId;
                                        productName = dr["ProductName"].ToString();
                                        found = 1;
                                    }
                                }
                            }

                            if (found == 0)
                            {
                                // Handling the case when p.SKU is null                            	
                                query = "SELECT V.VariantID, P.ProductID, V.SKUSuffix AS SKU, P.ProductID, V.Colors, V.Sizes, P.Name + ' ' + V.Name AS ProductName FROM ProductVariant AS V INNER JOIN Product AS P ON V.ProductID=P.ProductID WHERE (V.SKUSuffix)='" + requestSKU + "'";
                                using (SqlConnection conn = DB.dbConn())
                                {
                                    conn.Open();
                                    using (IDataReader dr = DB.GetRS(query, conn))
                                    {
                                        if (dr.Read())
                                        {
                                            variantId = dr["VariantID"].ToString();
                                            skuVsVariantId[requestSKU] = variantId;
                                            productName = dr["ProductName"].ToString();
                                            found = 4;
                                        }
                                    }
                                }
                            }

                            if (found == 0 && requestSKUParent != "")
                            {
                                // Handling the case when Parent item code is sent to match variant SKU - from POS
                                query = "SELECT V.VariantID, P.ProductID, V.SKUSuffix AS SKU, P.ProductID, V.Colors, V.Sizes, P.Name + ' ' + V.Name AS ProductName FROM ProductVariant AS V INNER JOIN Product AS P ON V.ProductID=P.ProductID WHERE (V.SKUSuffix)='" + requestSKUParent + "' ";
                                using (SqlConnection conn = DB.dbConn())
                                {
                                    conn.Open();
                                    using (IDataReader dr = DB.GetRS(query, conn))
                                    {
                                        if (dr.Read())
                                        {
                                            variantId = dr["VariantID"].ToString();
                                            skuVsVariantId[requestSKUParent] = variantId;
                                            productName = dr["ProductName"].ToString();
                                            found = 5;
                                        }
                                    }
                                }
                            }

                            if (found == 0 && requestSKUParent != "")
                            {
                                // Handling the case when Parent item code is sent to match variant SKU - from POS
                                query = "SELECT V.VariantID, P.ProductID, V.SKUSuffix AS SKU, P.ProductID, V.Colors, V.Sizes, P.Name + ' ' + V.Name AS ProductName FROM ProductVariant AS V INNER JOIN Product AS P ON V.ProductID=P.ProductID WHERE (V.Name)='" + requestSKUParent + "'";
                                using (SqlConnection conn = DB.dbConn())
                                {
                                    conn.Open();
                                    using (IDataReader dr = DB.GetRS(query, conn))
                                    {
                                        if (dr.Read())
                                        {
                                            variantId = dr["VariantID"].ToString();
                                            skuVsVariantId[requestSKUParent] = variantId;
                                            productName = dr["ProductName"].ToString();
                                            found = 6;
                                        }
                                    }
                                }
                            }


                            if (found == 0)
                            {
                                // Handling the case when V.SKUSuffix is null                            	
                                query = "SELECT V.VariantID, P.ProductID, P.SKU AS SKU, P.ProductID, V.Colors, V.Sizes, P.Name + ' ' + V.Name AS ProductName FROM ProductVariant AS V INNER JOIN Product AS P ON V.ProductID=P.ProductID WHERE V.SKUSuffix IS NULL AND (P.SKU)='" + requestSKU + "'";
                                using (SqlConnection conn = DB.dbConn())
                                {
                                    conn.Open();
                                    using (IDataReader dr = DB.GetRS(query, conn))
                                    {
                                        if (dr.Read())
                                        {
                                            variantId = dr["VariantID"].ToString();
                                            skuVsVariantId[requestSKU] = variantId;
                                            productName = dr["ProductName"].ToString();
                                            found = 7;
                                        }
                                    }
                                }
                            }

                        }
                        catch { }
                    }
                    else
                    {
                        variantId = skuVsVariantId[requestSKU].ToString();
                    }

                    responseItemNode = responseDoc.CreateElement("Item");
                    responseItemsNode.AppendChild(responseItemNode);

                    responseItemNumberNode = responseDoc.CreateElement("ItemNumber");
                    responseItemNumberNode.AppendChild(responseDoc.CreateTextNode(variantId));
                    responseItemNode.AppendChild(responseItemNumberNode);

                    responseItemCodeNode = responseDoc.CreateElement("ItemCode");
                    responseItemCodeNode.AppendChild(responseDoc.CreateTextNode(requestSKU));
                    responseItemNode.AppendChild(responseItemCodeNode);


                    responseItemNameNode = responseDoc.CreateElement("ItemDescription");
                    responseItemNameNode.AppendChild(responseDoc.CreateTextNode(productName));
                    responseItemNode.AppendChild(responseItemNameNode);


                    foreach (XmlNode requestItemChildNode in requestItemNode.ChildNodes)
                    {
                        if (requestItemChildNode.Name == "ItemOption")
                        {
                            string optionName;

                            optionName = requestItemChildNode.Attributes["Name"] == null ? "" : requestItemChildNode.Attributes["Name"].Value;
                            if (optionName.ToUpper() == "COLOR")
                            {
                                color = requestItemChildNode.FirstChild.InnerText;

                                responseItemOptionNode = responseDoc.CreateElement("ItemOption");
                                responseItemOptionNode.AppendChild(responseDoc.CreateTextNode(color));
                                responseItemNode.AppendChild(responseItemOptionNode);
                                responseAttr = responseDoc.CreateAttribute("Name");
                                responseAttr.Value = "Color";
                                responseItemOptionNode.Attributes.Append(responseAttr);
                            }
                            else if (optionName.ToUpper() == "SIZE")
                            {
                                size = requestItemChildNode.FirstChild.InnerText;

                                responseItemOptionNode = responseDoc.CreateElement("ItemOption");
                                responseItemOptionNode.AppendChild(responseDoc.CreateTextNode(size));
                                responseItemNode.AppendChild(responseItemOptionNode);
                                responseAttr = responseDoc.CreateAttribute("Name");
                                responseAttr.Value = "Size";
                                responseItemOptionNode.Attributes.Append(responseAttr);
                            }
                        }
                        else if (requestItemChildNode.Name == "QuantityInStock")
                        {
                            inventoryQuantity = requestItemChildNode.FirstChild.InnerText;
                        }
                        else if (requestItemChildNode.Name == "Price")
                        {
                            try
                            {
                                productPrice = requestItemChildNode.FirstChild.InnerText;
                            }
                            catch
                            {
                                productPrice = "";
                            }
                        }
                    }

                    if (variantId != null && variantId.Length > 0)
                    {
                        string trackInventoryBySizeAndColor = "0";
                        try
                        {
                            string inventoryTrackquery;
                            inventoryTrackquery = "SELECT p.TrackInventoryBySizeAndColor FROM Product as p, ProductVariant as v WHERE p.ProductID = v.ProductID AND v.VariantID='" + variantId + "'";
                           
                            using (SqlConnection conn = DB.dbConn())
                            {
                                conn.Open();
                                using (IDataReader dr = DB.GetRS(inventoryTrackquery, conn))
                                {
                                    if (dr.Read())
                                    {
                                        trackInventoryBySizeAndColor = dr["TrackInventoryBySizeAndColor"].ToString();
                                    }
                                }
                            }                            
                        }
                        catch { }
                        

                        string updatePriceSql1 = " ";
                        string updateSalePriceSql1 = " ";

                        if (updatePrice == true && productPrice != "")
                        {
                            updatePriceSql1 = " , Price = " + productPrice + " ";
                        }
                        if (updatePrice == true && productSalePrice != "")
                        {
                            updateSalePriceSql1 = " , SalePrice = " + productSalePrice + " ";
                        }

                        if (updatePrice == true && productSalePrice == "")
                        {
                            updateSalePriceSql1 = " , SalePrice = NULL ";
                        }

                        SqlConnection dbconn = new SqlConnection();
                        dbconn.ConnectionString = DB.GetDBConn();
                        if (trackInventoryBySizeAndColor == "0")
                        {
                            string updateStmt;
                            try
                            {

                                dbconn.Open();
                                //ONLY Update Inventory in ProductVariant table if TrackInventoryBySizeAndColor = 0)
                                updateStmt = "UPDATE ProductVariant SET ProductVariant.Inventory='" + inventoryQuantity + "' " + updatePriceSql1 + updateSalePriceSql1 + " Where ProductVariant.VariantID='" + variantId + "'";
                                cmd = new SqlCommand(updateStmt, dbconn);
                                if (cmd.ExecuteNonQuery() >= 1)
                                {
                                    inventoryUpdateStatus = "0";
                                    inventoryUpdateStatusMsg = "updateStmt=" + updateStmt;
                                }
                                else
                                {
                                    failedItems++;
                                }
                            }
                            finally
                            {
                                if (dbconn != null)
                                    dbconn.Close();
                            }
                        }
                        else
                        {
                            // trackInventoryBySizeAndColor == 1 
                            string query;
                            string updateOrInserStmt = "";
                            try
                            {
                                dbconn.Open();
                                query = "SELECT * FROM Inventory WHERE VariantID='" + variantId + "' AND Color='" + color + "' AND [Size]='" + size + "'";
                                updateOrInserStmt = "UPDATE Inventory SET Quan='" + inventoryQuantity + "' WHERE VariantID='" + variantId + "' AND VendorFullSKU ='" + requestSKU + "'";

                                cmd = new SqlCommand(updateOrInserStmt, dbconn);
                                if (cmd.ExecuteNonQuery() >= 1)
                                {
                                    inventoryUpdateStatus = "0";
                                    inventoryUpdateStatusMsg = "updateOrInserStmt=" + updateOrInserStmt;
                                }
                                else
                                {
                                    failedItems++;
                                }

                                string updatePriceSql2 = "";
                                if (updatePrice == true && productPrice != "")
                                {
                                    updatePriceSql2 = " UPDATE ProductVariant SET Inventory = Inventory " + updatePriceSql1 + updateSalePriceSql1 + " Where VariantID='" + variantId + "'";
                                    cmd = new SqlCommand(updatePriceSql2, dbconn);
                                    if (cmd.ExecuteNonQuery() == 1)
                                    {
                                    	//do nothing...
                                    }
                                }

                            }
                            finally
                            {
                                if (dbconn != null)
                                    dbconn.Close();
                            }
                        }
                    }
                    else
                    {
                        inventoryUpdateStatus = "1";
                    }

                    responseQuantityInStockWebNode = responseDoc.CreateElement("InventoryUpdateStatus");
                    responseQuantityInStockWebNode.AppendChild(responseDoc.CreateTextNode(inventoryUpdateStatus));
                    responseItemNode.AppendChild(responseQuantityInStockWebNode);


                    responseQuantityInStockWebNode = responseDoc.CreateElement("InventoryUpdateStatusMsg");
                    responseQuantityInStockWebNode.AppendChild(responseDoc.CreateTextNode(inventoryUpdateStatusMsg));
                    responseItemNode.AppendChild(responseQuantityInStockWebNode);

                    responseQuantityInStockWebNode = responseDoc.CreateElement("QuantityInStockWEB");
                    responseQuantityInStockWebNode.AppendChild(responseDoc.CreateTextNode("-1"));
                    responseItemNode.AppendChild(responseQuantityInStockWebNode);
                }

                #endregion
            }

            responseStatusCodeNode = responseDoc.CreateElement("StatusCode");
            responseStatusCodeNode.AppendChild(responseDoc.CreateTextNode("0"));
            responseEnvelopeNode.AppendChild(responseStatusCodeNode);

            responseStatusMessageNode = responseDoc.CreateElement("StatusMessage");
            responseStatusMessageNode.AppendChild(responseDoc.CreateTextNode("Successful Count=" + (totalItems - failedItems) + ";Failed Count=" + failedItems));
            responseEnvelopeNode.AppendChild(responseStatusMessageNode);

            responseRequestIDNode = responseDoc.CreateElement("RequestID");
            responseRequestIDNode.AppendChild(responseDoc.CreateTextNode(ht["RequestID"] == null ? "" : ht["RequestID"].ToString()));
            responseEnvelopeNode.AppendChild(responseRequestIDNode);

            responseProviderNode = responseDoc.CreateElement("Provider");
            responseProviderNode.AppendChild(responseDoc.CreateTextNode(ht["Provider"] == null ? "" : ht["Provider"].ToString()));
            responseEnvelopeNode.AppendChild(responseProviderNode);

            Response.Clear();
            Response.ContentType = "text/xml";
            String tmpstr = responseDoc.OuterXml.Replace("encoding=\"utf-16\"?>", "encoding=\"utf-8\"?>");
            Response.Write(tmpstr);
            logging("Response " + '\r' + '\r' + tmpstr.Replace("><", ">" + '\r' + "<") + '\r' + '\r');
        }
        //-------------------------------------------------------------------------------------------------------------------------------



        private void parsexml()
        {
            string curName = null;
            string tmp = null;
            int ErrCode = 0;
            tmp = Request.Form["request"];
            tmp = tmp.Replace("><", "> <");
            ErrCode = 0;
            if (tmp == "")
                ErrCode = 1;
            StringReader stream1 = new StringReader(tmp);
            XmlTextReader r = new XmlTextReader(stream1);
            Hashtable myHT = new Hashtable();
            if (tmp == "")
                ShowError("Request is empty.", 9999, myHT);
            if (ErrCode == 0)
            {
                while (r.Read())
                {
                    if (r.Name != "" & r.NodeType == XmlNodeType.Element)
                        curName = r.Name.Trim();
                    r.Read();
                    if (r.NodeType == XmlNodeType.Text)
                        myHT[curName] = r.Value.Trim();
                }
                r.Close();
            }

            Customer c = new Customer(myHT["UserID"].ToString());
            Password pwd = new Password(myHT["Password"].ToString(), c.SaltKey);

            string SaltedAndHashedPassword = pwd.SaltedPassword;
            string myQuery3 = "Select Count(*) as count1 FROM Customer where Deleted=0 and IsAdmin IN (1,3) and EMail=" + DB.SQuote(myHT["UserID"].ToString()) + " and Password = " + DB.SQuote(SaltedAndHashedPassword);
            try
            {
                using (SqlConnection conn5 = DB.dbConn())
                {
                    conn5.Open();
                    using (IDataReader reader5 = DB.GetRS(myQuery3, conn5))
                    {
                        if (reader5.Read())
                        {
                            if (System.Int32.Parse(reader5["count1"].ToString()) == 0)
                            {
                                ErrCode = 9000;
                                ShowError("Login failed.", ErrCode, myHT);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                ShowError("Error. Auth Code. " + err.Message + " -- " + myQuery3 + " -- ", 9999, myHT);
            }
   
            // making sql request for Get Orders
            if (ErrCode == 0)
            {
                
              string LOC = (myHT["LimitOrderCount"] == null) ? "" : myHT["LimitOrderCount"].ToString();
              if (LOC == ""){
                  myHT["LimitOrderCount"] = "25";
                  LOC = "2";
              }
                switch (myHT["Command"].ToString().ToUpper())
                {
                    case "GETORDERS":
                        string exclList = null;
                        string orderStartNumber = (myHT["OrderStartNumber"] == null) ? "0" : myHT["OrderStartNumber"].ToString();
                        string numberOfDays = (myHT["NumberOfDays"] == null) ? "10" : myHT["NumberOfDays"].ToString();

                        string exclord = "";
                        if (exclord != "")
                        {
                            string[] excl = exclord.Split(',');
                            int k = 0;
                            int tempFor1 = excl.Length;
                            for (k = 0; k < tempFor1; k++)
                            {
                                exclList = exclList + " o.OrderNumber<>" + excl[k] + " and ";
                            }
                        }

                        string dateRangeCrit = "";
                        if (orderStartNumber == "0")
                        {
                            dateRangeCrit = " AND DATEDIFF(DAY, o.OrderDate, GETDATE()) <= " + numberOfDays;
                        }

                        string myQuery = "SELECT TOP " + LOC + " o.OrderNumber, o.CustomerID, o.ParentOrderNumber, o.OrderGUID, o.OrderDate,  o.OrderNotes, " +
                                                            " o.PaymentMethod, o.TransactionState, o.Email," + "o.BillingFirstName, o.BillingLastName, " + "o.BillingAddress1, o.BillingAddress2, " + "o.CapturedOn as PaymentClearedOn, " + "o.AVSResult, o.AuthorizationCode, o.AuthorizationResult," + "o.BillingCity, o.BillingState, o.BillingZip, o.BillingCountry, o.BillingPhone, o.BillingSuite, o.BillingCompany, " + "o.CardType, o.CardExpirationMonth, o.CardExpirationYear, o.CardName, o.CardNumber, " + "o.ShippingMethod, o.ShippingFirstName, o.ShippingLastName, o.ShippingAddress1, " + "o.ShippingAddress2, o.ShippingCity, o.ShippingState, o.ShippingZip, o.ShippingCountry, o.ShippedVIA, " + "o.ShippingPhone, o.ShippingCompany, o.ShippingSuite, o.ShippingPhone, " + "o.CouponCode, o.CouponDescription, o.CouponDiscountAmount, o.CouponDiscountPercent ,  " + "o.OrderShippingCosts, o.OrderTax, " + "o.OrderTotal, o.PONumber " + "from Orders o with (NOLOCK) " +
                                                            " WHERE o.OrderNumber > " + orderStartNumber + " " + dateRangeCrit + " ORDER BY o.OrderNumber";


                        try
                        {
                            using (SqlConnection conn = DB.dbConn())
                            {
                                conn.Open();
                                using (IDataReader reader = DB.GetRS(myQuery, conn))
                                {
                                    GetOrders(myHT, (SqlDataReader)reader);
                                }
                            }                            
                        }
                        catch (Exception err)
                        {
                            ShowError("Error Creating Response. " + err.Message + " -- " + myQuery, 9999, myHT);
                        }
                        finally
                        {

                        }

                        break;
                    case "UPDATEORDERS":
                        UpdateOrders(myHT, tmp);

                        break;
                    case "UPDATEORDERSSHIPPINGSTATUS":
                        updateOrdersShippingStatus(myHT, tmp);
                        break;
                    case "UPDATEORDERSPAYMENTSTATUS":
                        updateOrdersPaymentStatus(myHT, tmp);
                        break;
                    case "UPDATEINVENTORY":
                        UpdateInventory(myHT, tmp);
                        break;
                    default:
                        ShowError("Wrong Command value: '" + myHT["Command"] + "'", -1, myHT);
                        ErrCode = -1;
                        break;
                }

            }
        }

        //--------------------------------------------
        private String GetConfigValue(string valueName)
        {
            String tempGetConfigValue = null;
            Hashtable myHT9 = new Hashtable();
            string myQuery9 = "select ConfigValue FROM AppConfig WHERE Name = '" + valueName + "'";

            try
            {
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader reader9 = DB.GetRS(myQuery9, conn))
                    {
                        reader9.Read();
                        tempGetConfigValue = reader9["ConfigValue"].ToString();
                    }
                }
            }
            catch (Exception err9)
            {
                ShowError("Error GetConfigValue. " + err9.Message + " -- " + myQuery9, 9999, myHT9);
            }
            finally
            {

            }
            return tempGetConfigValue;
        }
        //____________________________________________

        override protected void OnInit(EventArgs e)
        {
            this.Load += new System.EventHandler(this.Page_Load);
            base.OnInit(e);
        }

        private void Page_Load(object Sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            parsexml();
        }
    }
}

