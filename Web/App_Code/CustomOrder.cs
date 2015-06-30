using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class CustomOrder
{
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public string Email { get; set; }
    public int CustomerID { get; set; }
    public string BillingCompany { get; set; }
    public string BillingFirstName { get; set; }
    public string BillingLastName { get; set; }
    public string BillingAddress1 { get; set; }
    public string BillingAddress2 { get; set; }
    public string BillingSuite { get; set; }
    public string BillingCity { get; set; }
    public string BillingState { get; set; }
    public string BillingZip { get; set; }
    public string BillingCountry { get; set; }
    public string BillingPhone { get; set; }
    public string CardType { get; set; }
    public string CardName { get; set; }
    public string Last4 { get; set; }
    public string CardExpirationMonth { get; set; }
    public string CardExpirationYear { get; set; }
    public string AuthorizationCode { get; set; }
    public string PaymentGateway { get; set; }
    public string TransactionState { get; set; }
    public string ShippingCompany { get; set; }
    public string ShippingFirstName { get; set; }
    public string ShippingLastName { get; set; }
    public string ShippingAddress1 { get; set; }
    public string ShippingAddress2 { get; set; }
    public string ShippingSuite { get; set; }
    public string ShippingCity { get; set; }
    public string ShippingState { get; set; }
    public string ShippingZip { get; set; }
    public string ShippingCountry { get; set; }
    public string ShippingPhone { get; set; }
    public string ShippingMethod { get; set; }
    public decimal OrderSubtotal { get; set; }
    public decimal OrderTax { get; set; }
    public decimal OrderShippingCosts { get; set; }
    public decimal OrderTotal { get; set; }
    public decimal TaxRate { get; set; }

    public static string ConvertCountryCode(string country)
    {
        switch (country)
        {
            case "United States":
                return "US";
            case "Canada":
                return "CA";
            default:
                return "US";
        }
    }

    public static string ConvertShippingMethod(string method)
    {
        if (method.Contains("Standard Ground"))
            return "FG";

        if (method.Contains("Expedited shipping"))
            return "OT";

        return "FG";
    }

    public static string ConvertCCType(string type)
    {
        switch (type)
        {
            case "VISA":
                return "01";
            case "MasterCard":
                return "02";
            case "AMEX":
                return "03";
            case "DISCOVER":
                return "04";
            default:
                return "01";
        }
    }
}

public class CustomProduct
{
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public string OrderedProductName { get; set; }
    public int Quantity { get; set; }
    public string ChosenColorSKUModifier { get; set; }
    public string ChosenSizeSKUModifier { get; set; }
    public string OrderedProductSKU { get; set; }
    public decimal OrderedProductPrice { get; set; }
    public decimal OrderedProductRegularPrice { get; set; }
}

public class CustomProductExtra
{
    //public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int PartID { get; set; }
    public string KitID { get; set; }
    public int Quantity { get; set; }
    public string Color { get; set; }
}