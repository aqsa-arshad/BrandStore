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
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.Sql;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for search.
    /// </summary>
    public partial class ajaxShipping : System.Web.UI.Page
    {

        Customer ThisCustomer;
        int ProductID = 0;
        int VariantID = 0;
        int Quantity = 1;
        String Country = String.Empty;
        String State = String.Empty;
        String PostalCode = String.Empty;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            ProductID = CommonLogic.QueryStringUSInt("ProductID");
            VariantID = CommonLogic.QueryStringUSInt("VariantID");
            Quantity = CommonLogic.QueryStringUSInt("Quantity");
            Country = CommonLogic.QueryStringCanBeDangerousContent("Country");
            State = CommonLogic.QueryStringCanBeDangerousContent("State");
            PostalCode = CommonLogic.QueryStringCanBeDangerousContent("PostalCode");

            if (ProductID == 0)
            {
                if (VariantID != 0)
                {
                    ProductID = AppLogic.GetVariantProductID(VariantID);
                }
            }
            if (VariantID == 0)
            {
                if (ProductID != 0)
                {
                    VariantID = AppLogic.GetDefaultProductVariant(ProductID);
                }
            }

            decimal Price = VariantPriceLookup();
            decimal Weight = VariantWeightLookup();
            int CountryID = AppLogic.GetCountryID(Country);
            int StateID = AppLogic.GetStateID(State);
            int ZoneID = Shipping.ZoneLookup(PostalCode);

            Price = Quantity * Price;
            Weight = Quantity * Weight;

            Response.ContentType = "application/xml";
            XmlWriter writer = new XmlTextWriter(Response.OutputStream, System.Text.Encoding.UTF8);
            writer.WriteStartDocument();
            writer.WriteStartElement("root");
            WriteShippingXML(writer, VariantID, Price, Weight, CountryID, StateID, ZoneID);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
        }

        public decimal VariantWeightLookup()
        {
            decimal tmp = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select Weight from ProductVariant  with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        tmp = DB.RSFieldDecimal(rs, "Weight");
                    }
                }
            }

            if (AppLogic.IsAKit(ProductID))
            {
                // figure out the active kit in construction's weight:
                tmp += AppLogic.KitWeightDelta(ThisCustomer.CustomerID, ProductID, 0);
            }
            if (tmp <= 0.0M)
            {
                tmp = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight");
            }
            if (tmp < AppLogic.AppConfigUSDecimal("MinOrderWeight"))
            {
                tmp = AppLogic.AppConfigUSDecimal("MinOrderWeight");
            }
            return tmp;
        }

        public decimal VariantPriceLookup()
        {
            Decimal tmp = System.Decimal.Zero;
            if (AppLogic.IsAKit(ProductID))
            {
                // figure out the active kit in construction's price is:
                tmp = AppLogic.GetKitTotalPrice(ThisCustomer.CustomerID, ThisCustomer.CustomerLevelID, ProductID, VariantID, 0);
            }
            else
            {
                tmp = AppLogic.VariantPriceLookup(ThisCustomer, VariantID);
            }
            return tmp;
        }

        public void WriteShippingXML(XmlWriter writer, int VariantID, decimal Price, decimal Weight, int CountryID, int StateID, int ZoneID)
        {
            bool AnyShippingMethodsFound = false;
            bool ShippingMethodToStateMapIsEmpty = Shipping.ShippingMethodToStateMapIsEmpty();
            bool ShippingMethodToCountryMapIsEmpty = Shipping.ShippingMethodToCountryMapIsEmpty();
            bool ShippingMethodToZoneMapIsEmpty = Shipping.ShippingMethodToZoneMapIsEmpty();

            int FreeShippingMethodID = 0;
            if (AppLogic.AppConfigUSInt("ShippingMethodIDIfFreeShippingIsOn") != 0)
            {
                FreeShippingMethodID = AppLogic.AppConfigUSInt("ShippingMethodIDIfFreeShippingIsOn");
            }

            bool ShippingIsFree = false;
            if (Price > AppLogic.AppConfigUSDecimal("FreeShippingThreshold"))
            {
                ShippingIsFree = true;
            }

            Shipping.ShippingCalculationEnum ShipCalcID = Shipping.GetActiveShippingCalculationID();
            decimal ExtraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");

            switch (ShipCalcID)
            {
                case Shipping.ShippingCalculationEnum.CalculateShippingByWeight:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "CalculateShippingByWeight");
                        StringBuilder shipsql = new StringBuilder(4096);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + StateID + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + CountryID + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + ZoneID + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS(shipsql.ToString(), con))
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && ThisID == FreeShippingMethodID)
                                    {
                                        writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + AppLogic.GetString("shoppingcart.aspx.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByWeightCharge(ThisID, Weight) + ExtraFee;
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            AnyShippingMethodsFound = true;
											writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(ThisShipCost));
                                        }
                                    }
                                }
                            }
                        }

                        if (!AnyShippingMethodsFound)
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }
                        writer.WriteEndElement();
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotal:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "CalculateShippingByTotal");
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + StateID + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + CountryID + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + ZoneID + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS(shipsql.ToString(), con))
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && ThisID == FreeShippingMethodID)
                                    {
										writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": FREE");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByTotalCharge(ThisID, Price) + ExtraFee; // exclude download items!
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            AnyShippingMethodsFound = true;
											writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(ThisShipCost));
                                        }
                                    }
                                }
                            }
                        }

                        if (!AnyShippingMethodsFound)
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }
                        writer.WriteEndElement();
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseFixedPrice:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "UseFixedPrice");
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + StateID + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + CountryID + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + ZoneID + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS(shipsql.ToString(), con))
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && ThisID == FreeShippingMethodID)
                                    {
										writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": FREE");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = (decimal)DB.RSFieldDecimal(rs, "FixedRate") + ExtraFee;
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            AnyShippingMethodsFound = true;
											writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(ThisShipCost));
                                        }
                                    }
                                }
                            }
                        }

                        if (!AnyShippingMethodsFound)
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }
                        writer.WriteEndElement();
                        break;
                    }
                case Shipping.ShippingCalculationEnum.AllOrdersHaveFreeShipping:
                    AnyShippingMethodsFound = true;
                    writer.WriteStartElement("Shipping");
                    writer.WriteAttributeString("Method", "AllOrdersHaveFreeShipping");
                    writer.WriteElementString("Method1", "All Orders Have Free Shipping");
                    writer.WriteEndElement();
                    break;
                case Shipping.ShippingCalculationEnum.UseFixedPercentageOfTotal:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "UseFixedPercentageOfTotal");
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + StateID + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + CountryID + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + ZoneID + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS(shipsql.ToString(), con))
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && ThisID == FreeShippingMethodID)
                                    {
										writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": FREE");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByFixedPercentageCharge(ThisID, Price) + ExtraFee;
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            AnyShippingMethodsFound = true;
											writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(ThisShipCost));
                                        }
                                    }
                                }
                            }
                        }

                        if (!AnyShippingMethodsFound)
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }
                        writer.WriteEndElement();
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "UseIndividualItemShippingCosts");
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + StateID + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + CountryID + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + ZoneID + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS(shipsql.ToString(), con))
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && ThisID == FreeShippingMethodID)
                                    {
										writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": FREE");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetVariantShippingCost(VariantID, ThisID) + ExtraFee;
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            AnyShippingMethodsFound = true;
											writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(ThisShipCost));
                                        }
                                    }
                                }
                            }
                        }

                        if (!AnyShippingMethodsFound)
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }
                        writer.WriteEndElement();
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByWeightAndZone:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "CalculateShippingByWeightAndZone");
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + StateID + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + CountryID + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + ZoneID + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS(shipsql.ToString(), con))
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && ThisID == FreeShippingMethodID)
                                    {
										writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": FREE");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByWeightAndZoneCharge(ThisID, Weight, ZoneID) + ExtraFee;
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            AnyShippingMethodsFound = true;
											writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(ThisShipCost));
                                        }
                                    }
                                }
                            }
                        }

                        if (!AnyShippingMethodsFound)
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }
                        writer.WriteEndElement();
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotalAndZone:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "CalculateShippingByTotalAndZone");
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + StateID + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + CountryID + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + ZoneID + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS(shipsql.ToString(), con))
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && ThisID == FreeShippingMethodID)
                                    {
										writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": FREE");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByTotalAndZoneCharge(ThisID, Price, ZoneID) + ExtraFee;
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            AnyShippingMethodsFound = true;
											writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(ThisShipCost));
                                        }
                                    }
                                }
                            }
                        }

                        if (!AnyShippingMethodsFound)
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }
                        writer.WriteEndElement();
                        break;
                    }
                case Shipping.ShippingCalculationEnum.CalculateShippingByTotalByPercent:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "CalculateShippingByTotalByPercent");
                        StringBuilder shipsql = new StringBuilder(1024);
                        shipsql.Append("select * from ShippingMethod  with (NOLOCK)  where IsRTShipping=0 ");
                        if (!ShippingMethodToStateMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToStateMap  with (NOLOCK)  where StateID=" + StateID + ")");
                        }
                        if (!ShippingMethodToCountryMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToCountryMap  with (NOLOCK)  where CountryID=" + CountryID + ")");
                        }
                        if (!ShippingMethodToZoneMapIsEmpty)
                        {
                            shipsql.Append(" and ShippingMethodID in (select ShippingMethodID from ShippingMethodToZoneMap  with (NOLOCK)  where ShippingZoneID=" + ZoneID + ")");
                        }
                        shipsql.Append(" order by Displayorder");

                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            using (IDataReader rs = DB.GetRS(shipsql.ToString(), con))
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, "ShippingMethodID");
                                    if (ShippingIsFree && ThisID == FreeShippingMethodID)
                                    {
										writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": FREE");
                                    }
                                    else
                                    {
                                        Decimal ThisShipCost = Shipping.GetShipByTotalByPercentCharge(ThisID, Price) + ExtraFee;
                                        if (ThisShipCost != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                        {
                                            AnyShippingMethodsFound = true;
											writer.WriteElementString("Method" + ThisID.ToString(), Shipping.GetShippingMethodDisplayName(ThisID, null) + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(ThisShipCost));
                                        }
                                    }
                                }
                            }
                        }

                        if (!AnyShippingMethodsFound)
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }
                        writer.WriteEndElement();
                        break;
                    }
                case Shipping.ShippingCalculationEnum.UseRealTimeRates:
                    {
                        writer.WriteStartElement("Shipping");
                        writer.WriteAttributeString("Method", "UseRealTimeRates");
                        ShippingMethods SM = GetRates();

                        if (SM.Count > 0 )
                        {
                            int ThisID = 1;
                            foreach (ShipMethod meth in SM)
                            {
                                if (meth.ServiceRate != System.Decimal.Zero || !AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost"))
                                {
                                    writer.WriteElementString("Method" + ThisID.ToString(), meth.ServiceName + ": " + Localization.CurrencyStringForDisplayWithoutExchangeRate(meth.ServiceRate));
                                    ThisID++;
                                }
                            }
                        }
                        else if (SM.ErrorMsg.Length != 0)
                        {
                            if (SM.ErrorMsg.IndexOf(AppLogic.AppConfig("RTShipping.CallForShippingPrompt")) != -1)
                            {
                                writer.WriteElementString("Method0", AppLogic.AppConfig("RTShipping.CallForShippingPrompt"));
                            }
                            else
                            {
                                writer.WriteElementString("Method0", SM.ErrorMsg);
                            }
                        }
                        else
                        {
                            writer.WriteElementString("Method0", "No Matching Shipping Methods Found For That Country/State/Postal Code");
                        }

                        writer.WriteEndElement();
                        break;
                    }
            }
        }

        public static bool GetProductIsShipSeparately(int ProductID)
        {
            bool tmpS = false;
            if (ProductID != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader rs = DB.GetRS("select IsShipSeparately from productvariant   with (NOLOCK)  where productid=" + ProductID.ToString(), con))
                    {
                        if (rs.Read())
                        {
                            tmpS = DB.RSFieldBool(rs, "IsShipSeparately");
                        }
                    }
                }
            }
            return tmpS;
        }

        static public decimal GetVariantWeight(int VariantID)
        {
            decimal pr = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select Weight from productvariant   with (NOLOCK)  where VariantID=" + VariantID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        pr = DB.RSFieldDecimal(rs, "Weight");
                    }
                }
            }

            return pr;
        }

        protected ShippingMethods GetRates()
        {
            decimal ShippingHandlingExtraFee = AppLogic.AppConfigNativeDecimal("ShippingHandlingExtraFee");
            bool IsShipSeparately = GetProductIsShipSeparately(ProductID);

            RTShipping realTimeShipping = new RTShipping();

            Decimal MarkupPercent = AppLogic.AppConfigUSDecimal("RTShipping.MarkupPercent");

            // Set shipment info
            decimal ProductWeight = GetVariantWeight(VariantID);
            decimal ProductPrice = AppLogic.GetVariantPrice(VariantID);
            realTimeShipping.ShipmentWeight = ProductWeight * Quantity;
            if (realTimeShipping.ShipmentWeight == System.Decimal.Zero)
            {
                realTimeShipping.ShipmentWeight = AppLogic.AppConfigUSDecimal("RTShipping.DefaultItemWeight"); // force a default.
            }


            //Create Shipments Collection
            Shipments shipments = new Shipments();

            // Create Packages Collection
            Packages shipment = new Packages();

            int PackageID = 1;

            shipments = new Shipments();

            shipment = new Packages();

            // Set pickup type
            shipment.PickupType = AppLogic.AppConfig("RTShipping.UPS.UPSPickupType"); // RTShipping.PickupTypes.UPSCustomerCounter.ToString();
            if (AppLogic.AppConfig("RTShipping.UPS.UPSPickupType").Length == 0)
            {
                shipment.PickupType = RTShipping.PickupTypes.UPSCustomerCounter.ToString();
            }

            // Set destination address of this package group:
            Address sa = new Address();
            sa.LoadFromDB(ThisCustomer.PrimaryShippingAddressID);
            shipment.DestinationCity = sa.City;
            shipment.DestinationStateProvince = sa.State;
            shipment.DestinationZipPostalCode = sa.Zip;
            shipment.DestinationCountryCode = AppLogic.GetCountryTwoLetterISOCode(sa.Country);
            shipment.DestinationResidenceType = sa.ResidenceType;
            realTimeShipping.DestinationResidenceType = shipment.DestinationResidenceType;

            // now override them with what is passed in here:
            shipment.DestinationCountryCode = AppLogic.GetCountryTwoLetterISOCode(Country);
            shipment.DestinationStateProvince = State;
            shipment.DestinationZipPostalCode = PostalCode;

            if (IsShipSeparately)
            {
                for (int i = 1; i <= Quantity; i++)
                {
                    Package p = new Package();
                    p.PackageId = PackageID;
                    PackageID = PackageID + 1;
                    String Dimensions = String.Empty; // not supported here
                    p.Weight = ProductWeight;
                    if (p.Weight == System.Decimal.Zero)
                    {
                        p.Weight = 0.5M; // must have SOMETHING to use!
                    }
                    p.Weight += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");
                    p.Insured = AppLogic.AppConfigBool("RTShipping.Insured");
                    p.InsuredValue = ProductPrice;
                    shipment.AddPackage(p);
                    realTimeShipping.ShipmentValue += ProductPrice;
                    p = null;
                }
            }
            else
            {
                Package p = new Package();
                p.PackageId = PackageID;
                PackageID = PackageID + 1;
                String Dimensions = String.Empty;
                p.Weight = ProductWeight * Quantity;
                if (p.Weight == System.Decimal.Zero)
                {
                    p.Weight = 0.5M; // must have SOMETHING to use!
                }
                p.Weight += AppLogic.AppConfigUSDecimal("RTShipping.PackageExtraWeight");
                p.Insured = AppLogic.AppConfigBool("RTShipping.Insured");
                p.InsuredValue = ProductPrice * Quantity;
                shipment.AddPackage(p);
                p = null;
                realTimeShipping.ShipmentValue = ProductPrice * Quantity;
            }

            shipments.AddPackages(shipment);

            // Get carriers
            string carriers = String.Empty;
            if (shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase))
            {
                carriers = AppLogic.AppConfig("RTshipping.DomesticCarriers");
            }
            else
            {
                carriers = AppLogic.AppConfig("RTshipping.InternationalCarriers");
            }
            if (carriers.Length == 0)
            {
                carriers = AppLogic.AppConfig("RTShipping.ActiveCarrier");
            }


            // Get result type
            RTShipping.ResultType format = RTShipping.ResultType.CollectionList;
            String RTShipRequest = String.Empty;
            String RTShipResponse = String.Empty;
            decimal ShippingTaxRate = System.Decimal.Zero; // NOT supported here            
            return (ShippingMethods)realTimeShipping.GetRates(shipments, carriers, format, "ShippingMethod", "ShippingMethod", ShippingTaxRate, out RTShipRequest, out RTShipResponse, ShippingHandlingExtraFee, (decimal)MarkupPercent, realTimeShipping.ShipmentValue);
        }
    }
}
