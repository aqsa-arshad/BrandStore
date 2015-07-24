using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using AspDotNetStorefrontAdmin;
using AspDotNetStorefrontCore;
using System.Text.RegularExpressions;

public partial class Admin_export : AdminPageBase
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.CacheControl = "private";
        Response.Expires = 0;
        Response.AddHeader("pragma", "no-cache");

        SectionTitle = AppLogic.GetString("admin.sectiontitle.export", SkinID, LocaleSetting);

        if (!IsPostBack)
        {
            var orderNumber = Request.QueryString["ordernumber"];
            if (!String.IsNullOrEmpty(orderNumber))
            {
                var orderId = Convert.ToInt32(orderNumber);
                ExportOrders(orderId);
            }
        }
    }

    protected void SubmitButton_Click(object sender, EventArgs e)
    {
        ExportOrders(null);
    }

    private void ExportOrders(int? orderId)
    {
        var orders = new List<CustomOrder>();
        var products = new List<CustomProduct>();
        var productExtras = new List<CustomProductExtra>();

        var orderCust = AppLogic.GetString("CMD.StoreCustomerID", SkinID, LocaleSetting);
        var program = AppLogic.GetString("CMD.StoreProgramNumber", SkinID, LocaleSetting);

        var dateString = "";

        //call sproc and get list of orders
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
        {
            //string query = "exec custom_GetOrder @OrderID";
            string query = orderId.HasValue ? "exec custom_GetOrdersForExport @OrderID" : "exec custom_GetOrdersForExport";

            SqlCommand command = new SqlCommand(query, connection);
            if (orderId.HasValue)
            {
                command.Parameters.AddWithValue("@OrderID", orderId.Value);
            }
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    //Orders
                    var orderNumber = Convert.ToInt32(reader["OrderNumber"]);
                    var orderDate = Convert.ToDateTime(reader["OrderDate"]);
                    var email = reader["Email"].ToString();
                    var customerId = Convert.ToInt32(reader["CustomerID"]);
                    var billingCompany = reader["BillingCompany"] != null ? reader["BillingCompany"].ToString() : "";
                    var billingFirstName = reader["BillingFirstName"] != null ? reader["BillingFirstName"].ToString() : "";
                    var billingLastName = reader["BillingLastName"] != null ? reader["BillingLastName"].ToString() : "";
                    var billingAddress1 = reader["BillingAddress1"] != null ? reader["BillingAddress1"].ToString() : "";
                    var billingAddress2 = reader["BillingAddress2"] != null ? reader["BillingAddress2"].ToString() : "";
                    var billingSuite = reader["BillingSuite"] != null ? reader["BillingSuite"].ToString() : "";
                    var billingCity = reader["BillingCity"] != null ? reader["BillingCity"].ToString() : "";
                    var billingState = reader["BillingState"] != null ? reader["BillingState"].ToString() : "";
                    var billingZip = reader["BillingZip"] != null ? reader["BillingZip"].ToString() : "";
                    var billingCountry = reader["BillingCountry"] != null ? reader["BillingCountry"].ToString() : "";
                    var billingPhone = reader["BillingPhone"] != null ? reader["BillingPhone"].ToString() : "";
                    var cardType = reader["CardType"] != null ? reader["CardType"].ToString() : "";
                    var cardName = reader["CardName"] != null ? reader["CardName"].ToString() : "";
                    var last4 = reader["Last4"] != null ? reader["Last4"].ToString() : "";
                    var cardExpirationMonth = reader["CardExpirationMonth"] != null ? reader["CardExpirationMonth"].ToString() : "";
                    var cardExpirationYear = reader["CardExpirationYear"] != null ? reader["CardExpirationYear"].ToString().Substring(2) : "";
                    var authorizationCode = reader["AuthorizationCode"] != null ? reader["AuthorizationCode"].ToString() : "";
                    var paymentGateway = reader["PaymentGateway"] != null ? reader["PaymentGateway"].ToString() : "";
                    var transactionState = reader["TransactionState"] != null ? reader["TransactionState"].ToString() : "";
                    var shippingCompany = reader["ShippingCompany"] != null ? reader["ShippingCompany"].ToString() : "";
                    var shippingFirstName = reader["ShippingFirstName"] != null ? reader["ShippingFirstName"].ToString() : "";
                    var shippingLastName = reader["ShippingLastName"] != null ? reader["ShippingLastName"].ToString() : "";
                    var shippingAddress1 = reader["ShippingAddress1"] != null ? reader["ShippingAddress1"].ToString() : "";
                    var shippingAddress2 = reader["ShippingAddress2"] != null ? reader["ShippingAddress2"].ToString() : "";
                    var shippingSuite = reader["ShippingSuite"] != null ? reader["ShippingSuite"].ToString() : "";
                    var shippingCity = reader["ShippingCity"] != null ? reader["ShippingCity"].ToString() : "";
                    var shippingState = reader["ShippingState"] != null ? reader["ShippingState"].ToString() : "";
                    var shippingZip = reader["ShippingZip"] != null ? reader["ShippingZip"].ToString() : "";
                    var shippingCountry = reader["ShippingCountry"] != null ? reader["ShippingCountry"].ToString() : "";
                    var shippingPhone = reader["ShippingPhone"] != null ? reader["ShippingPhone"].ToString() : "";
                    var shippingMethod = reader["ShippingMethod"] != null ? reader["ShippingMethod"].ToString() : "";
                    var orderSubTotal = Convert.ToDecimal(reader["OrderSubTotal"]);
                    var orderTax = Convert.ToDecimal(reader["OrderTax"]);
                    var orderShippingCosts = Convert.ToDecimal(reader["OrderShippingCosts"]);
                    var orderTotal = Convert.ToDecimal(reader["OrderTotal"]);
                    var taxRate = Convert.ToDecimal(reader["TaxRate"]);

                    dateString = orderDate.ToShortDateString().Replace("/", "-");

                    var order = new CustomOrder
                    {
                        OrderID = orderNumber,
                        OrderDate = orderDate,
                        Email = email,
                        CustomerID = customerId,
                        BillingCompany = billingCompany,
                        BillingFirstName = billingFirstName,
                        BillingLastName = billingLastName,
                        BillingAddress1 = billingAddress1,
                        BillingAddress2 = billingAddress2,
                        BillingSuite = billingSuite,
                        BillingCity = billingCity,
                        BillingState = billingState,
                        BillingZip = billingZip,
                        BillingCountry = CustomOrder.ConvertCountryCode(billingCountry),
                        BillingPhone = billingPhone,
                        CardType = cardType,
                        CardName = cardName,
                        Last4 = last4,
                        CardExpirationMonth = cardExpirationMonth,
                        CardExpirationYear = cardExpirationYear,
                        AuthorizationCode = authorizationCode,
                        PaymentGateway = paymentGateway,
                        TransactionState = transactionState,
                        ShippingCompany = shippingCompany,
                        ShippingFirstName = shippingFirstName,
                        ShippingLastName = shippingLastName,
                        ShippingAddress1 = shippingAddress1,
                        ShippingAddress2 = shippingAddress2,
                        ShippingSuite = shippingSuite,
                        ShippingCity = shippingCity,
                        ShippingState = shippingState,
                        ShippingZip = shippingZip,
                        ShippingCountry = CustomOrder.ConvertCountryCode(shippingCountry),
                        ShippingPhone = shippingPhone,
                        ShippingMethod = shippingMethod,
                        OrderSubtotal = orderSubTotal,
                        OrderTax = orderTax,
                        OrderShippingCosts = orderShippingCosts,
                        OrderTotal = orderTotal,
                        TaxRate = taxRate
                    };

                    orders.Add(order);
                }

                reader.NextResult();

                while (reader.Read())
                {
                    //Products
                    var orderNumber = Convert.ToInt32(reader["OrderNumber"]);
                    var productId = Convert.ToInt32(reader["ProductID"]);
                    var orderedProductName = reader["OrderedProductName"].ToString();
                    var quantity = Convert.ToInt32(reader["Quantity"]);
                    var chosenColorSKUModifier = reader["ChosenColorSKUModifier"] != null ? reader["ChosenColorSKUModifier"].ToString() : "";
                    var orderedProductSKU = reader["OrderedProductSKU"] != null ? reader["OrderedProductSKU"].ToString() : "";
                    var orderedProductPrice = Convert.ToDecimal(reader["OrderedProductPrice"]);
                    var orderedProductRegularPrice = Convert.ToDecimal(reader["OrderedProductRegularPrice"]);

                    var product = new CustomProduct
                    {
                        OrderID = orderNumber,
                        ProductID = productId,
                        OrderedProductName = orderedProductName,
                        Quantity = quantity,
                        ChosenColorSKUModifier = chosenColorSKUModifier,
                        OrderedProductSKU = orderedProductSKU,
                        OrderedProductPrice = orderedProductPrice,
                        OrderedProductRegularPrice = orderedProductRegularPrice
                    };

                    products.Add(product);
                }

                reader.NextResult();

                while (reader.Read())
                {
                    //Product Extras
                    var productId = Convert.ToInt32(reader["ProductID"]);
                    var partId = Convert.ToInt32(reader["PartID"]);
                    var kitId = reader["KitID"].ToString();
                    var quantity = Convert.ToInt32(reader["Quantity"]);
                    var color = reader["Color"].ToString();
                    var sort = Convert.ToInt32(reader["Sort"]);

                    var productExtra = new CustomProductExtra
                    {
                        ProductID = productId,
                        PartID = partId,
                        KitID = kitId,
                        Quantity = quantity,
                        Color = color,
                        Sort = sort
                    };

                    productExtras.Add(productExtra);
                }
            }
            finally
            {
                reader.Close();
            }
        }

        StringWriter stringWriter = new StringWriter();
        XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
        xmlTextWriter.Formatting = Formatting.Indented;
        xmlTextWriter.WriteStartDocument();
        xmlTextWriter.WriteStartElement("orders");

        //loop thru and build xml
        foreach (var order in orders)
        {
            xmlTextWriter.WriteStartElement("order");

            xmlTextWriter.WriteElementString("version", "");
            xmlTextWriter.WriteElementString("rectype", "B");
            xmlTextWriter.WriteElementString("seqno", "1");
            xmlTextWriter.WriteElementString("ordno", order.OrderID.ToString());
            xmlTextWriter.WriteElementString("webtrack", order.OrderID.ToString());
            xmlTextWriter.WriteElementString("orddate", order.OrderDate.ToString("M/d/yy"));
            xmlTextWriter.WriteElementString("loginid", order.Email);
            xmlTextWriter.WriteElementString("email", order.Email);
            xmlTextWriter.WriteElementString("ordcust", orderCust);

            xmlTextWriter.WriteElementString("bcompany", order.BillingCompany);
            xmlTextWriter.WriteElementString("battn", (order.BillingFirstName + " " + order.BillingLastName).Trim());
            xmlTextWriter.WriteElementString("baddr1", order.BillingAddress1);
            xmlTextWriter.WriteElementString("baddr2", (order.BillingAddress2 + " " + order.BillingSuite).Trim());
            xmlTextWriter.WriteElementString("bcity", order.BillingCity);
            xmlTextWriter.WriteElementString("bstate", order.BillingState);
            xmlTextWriter.WriteElementString("bzip", order.BillingZip);
            xmlTextWriter.WriteElementString("bcountry", CustomOrder.ConvertCountryCode(order.BillingCountry));
            xmlTextWriter.WriteElementString("bphone", order.BillingPhone);

            xmlTextWriter.WriteElementString("cctype", CustomOrder.ConvertCCType(order.CardType));
            xmlTextWriter.WriteElementString("ccname", order.CardType);
            xmlTextWriter.WriteElementString("ccnum", order.Last4);
            xmlTextWriter.WriteElementString("ccexpire", String.Format("{0}{1}", order.CardExpirationMonth, order.CardExpirationYear));
            xmlTextWriter.WriteElementString("ccauthzano", order.AuthorizationCode);
            xmlTextWriter.WriteElementString("ccauthrefer", order.PaymentGateway);
            xmlTextWriter.WriteElementString("ccsettled", order.TransactionState == "CAPTURED" ? "Y" : "N");

            xmlTextWriter.WriteElementString("program", program);
            xmlTextWriter.WriteElementString("shipamt", order.OrderShippingCosts.ToString("F"));
            xmlTextWriter.WriteElementString("shipdesc", CustomOrder.ConvertShippingMethod(order.ShippingMethod));
            xmlTextWriter.WriteElementString("taxamt", order.OrderTax.ToString("F"));
            xmlTextWriter.WriteElementString("grdtotal", order.OrderTotal.ToString("F"));

            List<string> alpha1 = new List<string>();
            List<string> alpha2 = new List<string>();
            List<string> alpha3 = new List<string>();

            var items = products.Where(p => p.OrderID == order.OrderID);

            foreach (var item in items)
            {
                alpha1.Add(item.OrderedProductSKU);
                alpha2.Add(item.OrderedProductName);
                alpha3.Add(item.ChosenColorSKUModifier);
            }

            xmlTextWriter.WriteElementString("alpha1", String.Join(",", alpha1.ToArray()));
            xmlTextWriter.WriteElementString("alpha2", String.Join(",", alpha2.ToArray()));
            xmlTextWriter.WriteElementString("alpha3", String.Join(",", alpha3.ToArray()));

            xmlTextWriter.WriteElementString("num1", "");
            xmlTextWriter.WriteElementString("num2", "");
            xmlTextWriter.WriteElementString("num3", "");

            xmlTextWriter.WriteElementString("date1", "");
            xmlTextWriter.WriteElementString("date2", "");
            xmlTextWriter.WriteElementString("date3", "");

            //shiptos
            xmlTextWriter.WriteStartElement("shiptos");
            xmlTextWriter.WriteStartElement("shipto");

            xmlTextWriter.WriteElementString("rectype", "S");
            xmlTextWriter.WriteElementString("ordno", order.OrderID.ToString());
            xmlTextWriter.WriteElementString("webtrack", order.OrderID.ToString());
            xmlTextWriter.WriteElementString("seqno", "1");
            xmlTextWriter.WriteElementString("shipdate", "");
            xmlTextWriter.WriteElementString("scompany", order.ShippingCompany);
            xmlTextWriter.WriteElementString("sname", (order.ShippingFirstName + " " + order.ShippingLastName).Trim());
            xmlTextWriter.WriteElementString("saddr1", order.ShippingAddress1);
            xmlTextWriter.WriteElementString("saddr2", (order.ShippingAddress2 + " " + order.ShippingSuite).Trim());
            xmlTextWriter.WriteElementString("scity", order.ShippingCity);
            xmlTextWriter.WriteElementString("sstate", order.ShippingState);
            xmlTextWriter.WriteElementString("szip", order.ShippingZip);
            xmlTextWriter.WriteElementString("scountry", CustomOrder.ConvertCountryCode(order.ShippingCountry));
            xmlTextWriter.WriteElementString("sphone", order.ShippingPhone);
            xmlTextWriter.WriteElementString("shipdesc", CustomOrder.ConvertShippingMethod(order.ShippingMethod));
            xmlTextWriter.WriteElementString("comres", "R");
            xmlTextWriter.WriteElementString("shipamt", order.OrderShippingCosts.ToString("F"));
            xmlTextWriter.WriteElementString("taxcode1", "");
            xmlTextWriter.WriteElementString("taxcode2", "");

            xmlTextWriter.WriteEndElement();
            xmlTextWriter.WriteEndElement();

            foreach (var item in items)
            {
                xmlTextWriter.WriteStartElement("items");

                //get quantity and use as multiplier for extras
                var multiplier = item.Quantity;

                var color = item.ChosenColorSKUModifier == null ? "" : item.ChosenColorSKUModifier;
                var itemno = color == "" ? item.OrderedProductSKU : item.OrderedProductSKU.Replace(color, "");
                // Get the numerical portion of the item number 
                // The KitID might contain one or more alpha characters at the end which we will ignore)
                String partialKit = Regex.Match(itemno.ToString(), @"\d+").ToString();

                // Filter extra data by productID, Color, and Kit.  Since there could be more than one kit
                // with the same product extras, we just want to get the distinct list so we don't have 
                // duplicates.
                var extras = productExtras.Where(x => x.ProductID == item.ProductID &&
                                                 x.Color == item.ChosenColorSKUModifier &&
                                                 x.KitID.StartsWith(partialKit)).
                                                 GroupBy(x => new { x.ProductID, x.Color, x.KitID, x.Sort }).
                                                 Select(grp => grp.First());

                xmlTextWriter.WriteStartElement("item");
                xmlTextWriter.WriteElementString("rectype", "I");
                xmlTextWriter.WriteElementString("ordno", order.OrderID.ToString());
                xmlTextWriter.WriteElementString("webtrack", order.OrderID.ToString());
                xmlTextWriter.WriteElementString("seqno", "1");
                xmlTextWriter.WriteElementString("itemno", itemno);
                xmlTextWriter.WriteElementString("subno", "");
                xmlTextWriter.WriteElementString("itemcustno", "0");
                xmlTextWriter.WriteElementString("qtyord", item.Quantity.ToString());
                xmlTextWriter.WriteElementString("taxable", "");
                //xmlTextWriter.WriteElementString("itemprice", item.OrderedProductPrice.ToString("F"));
                xmlTextWriter.WriteElementString("itemprice", item.OrderedProductRegularPrice.ToString("F"));
                xmlTextWriter.WriteElementString("pointqty", "0.0000");
                xmlTextWriter.WriteElementString("personalization", "N");

                xmlTextWriter.WriteEndElement();

                foreach (var extra in extras)
                {
                    xmlTextWriter.WriteStartElement("item");
                    xmlTextWriter.WriteElementString("rectype", "I");
                    xmlTextWriter.WriteElementString("ordno", order.OrderID.ToString());
                    xmlTextWriter.WriteElementString("webtrack", order.OrderID.ToString());
                    xmlTextWriter.WriteElementString("seqno", "1");
                    xmlTextWriter.WriteElementString("itemno", extra.PartID.ToString());
                    xmlTextWriter.WriteElementString("subno", "");
                    xmlTextWriter.WriteElementString("itemcustno", "0");
                    xmlTextWriter.WriteElementString("qtyord", (extra.Quantity * multiplier).ToString());
                    xmlTextWriter.WriteElementString("taxable", "");
                    xmlTextWriter.WriteElementString("itemprice", "0.00");
                    xmlTextWriter.WriteElementString("pointqty", "0.0000");
                    xmlTextWriter.WriteElementString("personalization", "N");

                    xmlTextWriter.WriteEndElement();
                }

                xmlTextWriter.WriteEndElement();
            }

            //instructions
            xmlTextWriter.WriteStartElement("instructions");

            xmlTextWriter.WriteElementString("rectype", "C");
            xmlTextWriter.WriteElementString("ordno", order.OrderID.ToString());
            xmlTextWriter.WriteElementString("webtrack", order.OrderID.ToString());
            xmlTextWriter.WriteElementString("impinst", "");
            xmlTextWriter.WriteElementString("spclinst", "");

            xmlTextWriter.WriteEndElement();

            //taxes
            xmlTextWriter.WriteStartElement("taxes");
            xmlTextWriter.WriteStartElement("tax");

            xmlTextWriter.WriteElementString("rectype", "T");
            xmlTextWriter.WriteElementString("ordno", order.OrderID.ToString());
            xmlTextWriter.WriteElementString("webtrack", order.OrderID.ToString());
            xmlTextWriter.WriteElementString("taxcode", (order.TaxRate / 100).ToString());
            xmlTextWriter.WriteElementString("taxrate", order.TaxRate.ToString());

            xmlTextWriter.WriteEndElement();
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteEndElement();
        }

        xmlTextWriter.WriteEndElement();
        xmlTextWriter.WriteEndDocument();

        var fileName = orderId.HasValue ? dateString + "." + orderId.Value.ToString() + ".xml" : "export.orders.all.xml";

        //write to screen
        Response.Clear();
        Response.ContentType = "text/xml";
        Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
        Response.Write(stringWriter.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\"?>"));
        Response.End();
    }
}

/*
<?xml version="1.0" encoding="utf-8" ?>
<orders>
  <order>
    <version />
    <rectype>B</rectype>
    <seqno>1</seqno>
    <ordno>100006</ordno>
    <webtrack>100006</webtrack>
    <orddate>6/10/2014</orddate>
    <loginid>dolds@cmdagency.com</loginid>
    <email>dolds@cmdagency.com</email>
    <ordcust>58665</ordcust>
    <bcompany> </bcompany>
    <battn>Del Olds</battn>
    <baddr1>5162 SW Malsam Court</baddr1>
    <baddr2>
    </baddr2>
    <bcity>Tualatin</bcity>
    <bstate>OR</bstate>
    <bzip>97062</bzip>
    <bcountry>US</bcountry>
    <bphone>503-260-1744</bphone>
    <cctype>AMEX</cctype>
    <ccname>Del Olds</ccname>
    <ccnum>1009</ccnum>
    <ccexpire>10/2015</ccexpire>
    <ccauthzano>211582</ccauthzano>
    <ccauthrefer>6246445365</ccauthrefer>
    <ccsettled>Y</ccsettled>
    <program>91</program>
    <shipamt>0.0000</shipamt>
    <shipdesc>
    </shipdesc>
    <taxamt> </taxamt>
    <grdtotal>0.0200</grdtotal>
    <alpha1> </alpha1>
    <alpha2>
    </alpha2>
    <alpha3>
    </alpha3>
    <num1> </num1>
    <num2> </num2>
    <num3> </num3>
    <date1> </date1>
    <date2> </date2>
    <date3> </date3>
    <shiptos>
      <shipto>
        <rectype>S</rectype>
        <ordno>100006</ordno>
        <webtrack>100006</webtrack>
        <seqno>1</seqno>
        <shipdate>
        </shipdate>
        <scompany>
        </scompany>
        <sname>Del Olds</sname>
        <saddr1>5162 SW Malsam Court</saddr1>
        <saddr2>
        </saddr2>
        <scity>Tualatin</scity>
        <sstate>OR</sstate>
        <szip>97062</szip>
        <scountry>US</scountry>
        <sphone>503-260-1744</sphone>
        <shipdesc>
        </shipdesc>
        <comres>R</comres>
        <shipamt>0.0000</shipamt>
        <taxcode1>
        </taxcode1>
        <taxcode2>
        </taxcode2>
      </shipto>
    </shiptos>
    <items>
      <item>
        <rectype>I</rectype>
        <ordno>100006</ordno>
        <webtrack>100006</webtrack>
        <seqno>1</seqno>
        <itemno>CS3</itemno>
        <subno>
        </subno>
        <itemcustno>0</itemcustno>
        <qtyord>2</qtyord>
        <taxable>N</taxable>
        <itemprice>0.01</itemprice>
        <pointqty>0.0000</pointqty>
        <personalization>N</personalization>
      </item>
    </items>
    <instructions>
      <rectype>C</rectype>
      <ordno>100006</ordno>
      <webtrack>100006</webtrack>
      <impinst>
      </impinst>
      <spclinst>
      </spclinst>
    </instructions>
    <taxes>
      <tax>
        <rectype>T</rectype>
        <ordno>100006</ordno>
        <webtrack>100006</webtrack>
        <taxcode>001</taxcode>
        <taxrate>
        </taxrate>
      </tax>
    </taxes>
  </order>
</orders>
 */