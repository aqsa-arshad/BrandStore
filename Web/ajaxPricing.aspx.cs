// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.Sql;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for search.
    /// </summary>
    public partial class ajaxPricing : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            int ProductID = CommonLogic.QueryStringUSInt("ProductID");
            int VariantID = CommonLogic.QueryStringUSInt("VariantID");
            if (ProductID == 0)
            {
                ProductID = AppLogic.GetVariantProductID(VariantID);
            }

            String st = AppLogic.AppConfig("AjaxPricingPrompt").Replace("+", " ");
            String ChosenSize = CommonLogic.QueryStringCanBeDangerousContent("Size");
            ChosenSize = ChosenSize.Replace("[" + st, "[");
            ChosenSize = Regex.Replace(ChosenSize, "\\s+", "", RegexOptions.Compiled);
            String ChosenSizeSimple = String.Empty;
            String ChosenColor = CommonLogic.QueryStringCanBeDangerousContent("Color");
            ChosenColor = ChosenColor.Replace("[" + st, "[");
            ChosenColor = Regex.Replace(ChosenColor, "\\s+", "", RegexOptions.Compiled);
            String ChosenColorSimple = String.Empty;
            Decimal SizePriceModifier = System.Decimal.Zero;
            Decimal ColorPriceModifier = System.Decimal.Zero;

            if (ChosenSize.IndexOf("[") != -1)
            {
                int i1 = ChosenSize.IndexOf("[");
                int i2 = ChosenSize.IndexOf("]");
                if (i1 != -1 && i2 != -1)
                {
                    SizePriceModifier = Localization.ParseNativeDecimal(ChosenSize.Substring(i1 + 1, i2 - i1 - 1).Replace("$", "").Replace(",", ""));
                }
                ChosenSizeSimple = ChosenSize.Substring(0, i1);
            }
            if (ChosenColor.IndexOf("[") != -1)
            {
                int i1 = ChosenColor.IndexOf("[");
                int i2 = ChosenColor.IndexOf("]");
                if (i1 != -1 && i2 != -1)
                {
                    ColorPriceModifier = Localization.ParseNativeDecimal(ChosenColor.Substring(i1 + 1, i2 - i1 - 1).Replace("$", "").Replace(",", ""));
                }
                ChosenColorSimple = ChosenColor.Substring(0, i1);
            }

            Decimal FinalPriceDelta = SizePriceModifier + ColorPriceModifier;

            String NewPRText = "Not Available";

            XSLTExtensionBase xslte = new XSLTExtensionBase(ThisCustomer, ThisCustomer.SkinID);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select p.*, pv.VariantID, pv.name VariantName, pv.Price, pv.Description VariantDescription, isnull(pv.SalePrice, 0) SalePrice, isnull(SkuSuffix, '') SkuSuffix, pv.Dimensions, pv.Weight, isnull(pv.Points, 0) Points, sp.name SalesPromptName, case when pcl.productid is null then 0 else isnull(e.Price, 0) end ExtendedPrice FROM Product p join productvariant pv on p.ProductID = pv.ProductID join SalesPrompt sp on p.SalesPromptID = sp.SalesPromptID left join ExtendedPrice e on pv.VariantID=e.VariantID and e.CustomerLevelID=" + ThisCustomer.CustomerLevelID.ToString() + " left join ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = " + ThisCustomer.CustomerLevelID.ToString() + " WHERE p.ProductID = " + ProductID.ToString() + " and p.Deleted = 0 and pv.Deleted = 0 and p.Published = 1 and pv.VariantID=" + VariantID.ToString() + " and pv.Published = 1 ORDER BY p.ProductID, pv.DisplayOrder, pv.Name", con))
                {
                    if (rs.Read())
                    {
                        NewPRText = xslte.GetVariantPrice(VariantID.ToString(),
                            CommonLogic.IIF(DB.RSFieldBool(rs, "HidePriceUntilCart"), "1", "0"),
                            Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "Price")),
                            Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "SalePrice")),
                            Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "ExtendedPrice")),
                            DB.RSFieldInt(rs, "Points").ToString(),
                            DB.RSField(rs, "SalesPromptName"),
                            "1",
                            DB.RSFieldInt(rs, "TaxClassID").ToString(),
                            Localization.DecimalStringForDB(System.Decimal.Zero)).Replace("Price:", "Base Price:");
                        NewPRText += "";
                        NewPRText += xslte.GetVariantPrice(VariantID.ToString(),
                            CommonLogic.IIF(DB.RSFieldBool(rs, "HidePriceUntilCart"), "1", "0"),
                            Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "Price")),
                            Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "SalePrice")),
                            Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "ExtendedPrice")),
                            DB.RSFieldInt(rs, "Points").ToString(),
                            DB.RSField(rs, "SalesPromptName"),
                            "1",
                            DB.RSFieldInt(rs, "TaxClassID").ToString(),
                            Localization.DecimalStringForDB(FinalPriceDelta)); //.Replace("Price:", "Price as Selected:");
                    }
                }
            }

            Response.Expires = -1;
            Response.ContentType = "application/xml";
            XmlWriter writer = new XmlTextWriter(Response.OutputStream, System.Text.Encoding.UTF8);
            writer.WriteStartDocument();
            writer.WriteStartElement("root");

            writer.WriteElementString("CustomerID", ThisCustomer.CustomerID.ToString());
            writer.WriteElementString("CustomerLevelID", ThisCustomer.CustomerLevelID.ToString());
            writer.WriteElementString("ProductID", ProductID.ToString());
            writer.WriteElementString("VariantID", VariantID.ToString());

            writer.WriteStartElement("Size");
            writer.WriteElementString("Input", ChosenSize);
            writer.WriteElementString("Text", ChosenSizeSimple);
            writer.WriteElementString("Delta", Localization.DecimalStringForDB(SizePriceModifier));
            writer.WriteEndElement();

            writer.WriteStartElement("Color");
            writer.WriteElementString("Input", ChosenColor);
            writer.WriteElementString("Text", ChosenColorSimple);
            writer.WriteElementString("Delta", Localization.DecimalStringForDB(ColorPriceModifier));
            writer.WriteEndElement();

            writer.WriteElementString("ChosenAttributesPriceDelta", Localization.DecimalStringForDB(FinalPriceDelta));
            writer.WriteElementString("PriceHTML", NewPRText);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }

    }
}
