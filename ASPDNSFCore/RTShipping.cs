// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace AspDotNetStorefrontCore
{

    public partial class RTShipping
    {

        private string m_upsLogin;			// username, password, license
        private string m_upsServer;			// UPS server
        private string m_upsUsername;
        private string m_upsPassword;
        private string m_upsLicense;

        private string m_uspsLogin;			// username, password
        private string m_uspsServer;			// USPS server
        private string m_uspsUsername;
        private string m_uspsPassword;

        private string m_FedexAccountNumber;
        private string m_FedexServer;
        private string m_FedexMeter;			// Returned from Fedex after subscription
        private string m_FedexKey;
        private string m_FedexPassword;

        private string m_OriginAddress;
        private string m_OriginAddress2;
        private string m_OriginCity;
        private string m_OriginStateProvince;
        private string m_OriginZipPostalCode;
        private string m_OriginCountry;
        private string m_DestinationAddress;
        private string m_DestinationAddress2;
        private string m_DestinationCity;
        private string m_DestinationStateProvince;
        private string m_DestinationZipPostalCode;
        private string m_DestinationCountry;
        private ResidenceTypes m_DestinationResidenceType;

        private Decimal m_ShipmentWeight;
        private decimal m_ShipmentValue;
        private Decimal m_Length;	// Length of the package in inches
        private Decimal m_Width;	// Width of the package in inches
        private Decimal m_Height;	// Height of the package in inches

        private bool m_TestMode;

        private ArrayList ratesValues;
        private ArrayList ratesText;

        //new ShippingMethods Collection
        ShippingMethods SM;

        //will use for determining the cost of free shipping items to include or omit from a
        //returned rate if AppConfig.FreeShippingAllowsRateSelection is true
        Boolean HasFreeItems;
        int PackageQuantity;

		static private bool IsDomesticCountryCode(String CountryCode)
		{
			CountryCode = CountryCode.Trim().ToUpperInvariant();
			String[] DomesticCountries = {"US", "PR", "VI", "AS", "GU", "MP", "PW", "MH"};
            foreach (String s in DomesticCountries)
			{
				if(s == CountryCode)
				{
					return true;
				}
			}
			return false;
		}

        static private String MapPickupType(String s)
        {
            s = s.Trim().ToLowerInvariant();
            if (s == "upsdailypickup")
            {
                return "01";
            }
            if (s == "upscustomercounter")
            {
                return "03";
            }
            if (s == "upsonetimepickup")
            {
                return "06";
            }
            if (s == "upsoncallair")
            {
                return "07";
            }
            if (s == "upssuggestedretailrates")
            {
                return "11";
            }
            if (s == "upslettercenter")
            {
                return "19";
            }
            if (s == "upsairservicecenter")
            {
                return "20";
            }
            return "03"; // find some default
        }

        public string UPSLogin	// UPS Login infomration, "Username,Password,License" Please note: The login information is case sensitive
        {
            get { return m_upsLogin; }
            set
            {
                m_upsLogin = value;
                string[] arrUpsLogin = m_upsLogin.Split(',');
                try
                {
                    m_upsUsername = arrUpsLogin[0].Trim();
                    m_upsPassword = arrUpsLogin[1].Trim();
                    m_upsLicense = arrUpsLogin[2].Trim();
                }
                catch { }
            }
        }

        /// FedEx login information, "Username,Password"
        /// 
        public string FedexKey
        {
            get { return this.m_FedexKey; }
            set { this.m_FedexKey = value; }
        }

        public string FedexPassword
        {
            get { return this.m_FedexPassword; }
            set { this.m_FedexPassword = value; }
        }
        public string FedexAccountNumber
        {
            get { return this.m_FedexAccountNumber; }
            set { this.m_FedexAccountNumber = value; }
        }

        /// FedEx Meter Number provided by FedEx after subscription
        public string FedexMeter
        {
            get { return this.m_FedexMeter; }
            set { this.m_FedexMeter = value; }
        }

        /// URL To FedEx server
        public string FedexServer
        {
            get { return this.m_FedexServer; }
            set { this.m_FedexServer = value; }
        }


        public string UPSServer	// URL To ups server, either test or live
        {
            get { return m_upsServer; }
            set { m_upsServer = value.Trim(); }
        }

        public string UPSUsername	// URL To ups server, either test or live
        {
            get { return m_upsUsername; }
            set { m_upsUsername = value.Trim(); }
        }

        public string UPSPassword	// URL To ups server, either test or live
        {
            get { return m_upsPassword; }
            set { m_upsPassword = value.Trim(); }
        }

        public string UPSLicense	// URL To ups server, either test or live
        {
            get { return m_upsLicense; }
            set { m_upsLicense = value.Trim(); }
        }

        public string USPSLogin	// USPS Login information, "Username,Password" Please note: The login information is case sensitive
        {
            get { return m_uspsLogin; }
            set
            {
                m_uspsLogin = value.Trim();
                string[] arrUSPSLogin = m_uspsLogin.Split(',');
                try
                {
                    m_uspsUsername = arrUSPSLogin[0].Trim();
                    m_uspsPassword = arrUSPSLogin[1].Trim();
                }
                catch { }
            }
        }

        public string USPSServer	// URL To usps server, either test or live
        {
            get { return m_uspsServer; }
            set { m_uspsServer = value.Trim(); }
        }

        public string USPSUsername	// URL To usps server, either test or live
        {
            get { return m_uspsUsername; }
            set { m_uspsUsername = value.Trim(); }
        }

        public string USPSPassword	// URL To usps server, either test or live
        {
            get { return m_uspsPassword; }
            set { m_uspsPassword = value.Trim(); }
        }

        public string DestinationAddress	// Shipment destination street address
        {
            get { return m_DestinationAddress; }
            set { m_DestinationAddress = value; }
        }

        public string DestinationAddress2	// Shipment destination street address continued
        {
            get { return m_DestinationAddress2; }
            set { m_DestinationAddress2 = value; }
        }

        public string DestinationCity	// Shipment destination city
        {
            get { return m_DestinationCity; }
            set { m_DestinationCity = value; }
        }

        public string DestinationStateProvince	// Shipment destination State or Province
        {
            get
            {
                if (m_DestinationStateProvince == "-" || m_DestinationStateProvince == "--" || m_DestinationStateProvince == "ZZ")
                {
                    return String.Empty;
                }
                else
                {
                    return m_DestinationStateProvince;
                }
            }
            set { m_DestinationStateProvince = value; }
        }

        public string DestinationZipPostalCode	// Shipment Destination Zip or Postal Code
        {
            get { return m_DestinationZipPostalCode; }
            set { m_DestinationZipPostalCode = value; }
        }

        public string DestinationCountry	// Shipment Destination Country
        {
            get { return m_DestinationCountry; }
            set { m_DestinationCountry = value; }
        }

        public ResidenceTypes DestinationResidenceType	// Shipment Destination ResidenceType
        {
            get { return m_DestinationResidenceType; }
            set { m_DestinationResidenceType = value; }
        }

        public string OriginAddress	// Shipment origin street address
        {
            get { return m_OriginAddress; }
            set { m_OriginAddress = value; }
        }

        public string OriginAddress2	// Shipment origin street address continued
        {
            get { return m_OriginAddress2; }
            set { m_OriginAddress2 = value; }
        }

        public string OriginCity	// Shipment origin city
        {
            get { return m_OriginCity; }
            set { m_OriginCity = value; }
        }

        public string OriginStateProvince	// Shipment origin State or Province
        {
            get { return m_OriginStateProvince; }
            set { m_OriginStateProvince = value; }
        }

        public string OriginZipPostalCode	// Shipment Origin Zip or Postal Code
        {
            get { return m_OriginZipPostalCode; }
            set { m_OriginZipPostalCode = value; }
        }

        public string OriginCountry	// Shipment Origin Country
        {
            get { return m_OriginCountry; }
            set { m_OriginCountry = value; }
        }

        public Decimal ShipmentWeight	// Shipment shipmentWeight
        {
            get { return m_ShipmentWeight; }
            set { m_ShipmentWeight = value; }
        }

        public decimal ShipmentValue	//  Shipment value
        {
            get { return m_ShipmentValue; }
            set { m_ShipmentValue = value; }
        }

        public bool TestMode	// Boolean value to set entire class into test mode. Only test servers will be used if applicable
        {
            get { return m_TestMode; }
            set { m_TestMode = value; }
        }

        public Decimal Length	// Single value representing the lenght of the package in inches
        {
            get { return m_Length; }
            set { m_Length = value; }
        }

        public Decimal Width	// Single value representing the width of the package in inches
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        public Decimal Height	// Single value representing the height of the package in inches
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        public RTShipping()
        {
            UPSLogin = AppLogic.AppConfig("RTShipping.UPS.Username") + "," + AppLogic.AppConfig("RTShipping.UPS.Password") + "," + AppLogic.AppConfig("RTShipping.UPS.License");
            UPSServer = AppLogic.AppConfig("RTShipping.UPS.Server");
            UPSUsername = AppLogic.AppConfig("RTShipping.UPS.Username");
            UPSPassword = AppLogic.AppConfig("RTShipping.UPS.Password");
            UPSLicense = AppLogic.AppConfig("RTShipping.UPS.License");

            USPSServer = AppLogic.AppConfig("RTShipping.USPS.Server");
            USPSLogin = AppLogic.AppConfig("RTShipping.USPS.Username") + "," + AppLogic.AppConfig("RTShipping.USPS.Password");
            USPSUsername = AppLogic.AppConfig("RTShipping.USPS.Username");
            USPSPassword = AppLogic.AppConfig("RTShipping.USPS.Password");

            FedexAccountNumber = AppLogic.AppConfig("RTShipping.FEDEX.AccountNumber");
            FedexKey = AppLogic.AppConfig("RTShipping.FEDEX.Key");
            FedexPassword = AppLogic.AppConfig("RTShipping.FEDEX.Password");
            FedexServer = AppLogic.AppConfig("RTShipping.FEDEX.Server");
            FedexMeter = AppLogic.AppConfig("RTShipping.FEDEX.Meter");

            OriginAddress = AppLogic.AppConfig("RTShipping.OriginAddress");
            OriginAddress2 = AppLogic.AppConfig("RTShipping.OriginAddress2");
            OriginCity = AppLogic.AppConfig("RTShipping.OriginCity");
            OriginStateProvince = AppLogic.AppConfig("RTShipping.OriginState");
            OriginZipPostalCode = AppLogic.AppConfig("RTShipping.OriginZip");
            OriginCountry = AppLogic.AppConfig("RTShipping.OriginCountry");

            if (OriginCountry.Equals("US",StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    OriginZipPostalCode = OriginZipPostalCode.Substring(0, 5);
                }
                catch
                {
                    throw new Exception("The RTShipping.OriginZip AppConfig parameter is invalid, please update this value.");
                }
            }

            m_DestinationAddress = string.Empty;
            m_DestinationAddress2 = string.Empty;
            m_DestinationCity = string.Empty;
            m_DestinationStateProvince = string.Empty;
            m_DestinationZipPostalCode = string.Empty;
            m_DestinationCountry = string.Empty;
            m_DestinationResidenceType = ResidenceTypes.Unknown;

            m_ShipmentWeight = 0.0M;

            m_ShipmentValue = System.Decimal.Zero;

            m_TestMode = false;

            ratesValues = new ArrayList();
            ratesText = new ArrayList();

            SM = new ShippingMethods();
        }

        /// <summary>
        /// Main method which retrieves rates. Returns a dropdown list, radio button list, or multiline select box
        /// </summary>
        /// <param name="Shipment">The Packages object which contains the packages to be rated</param>
        /// <param name="Carriers">The carriers to get rates from: UPS, USPS, FedEx, DHL. Use a comma separated list</param>
        /// <param name="ListFormat">The type of list you would like back: DropDown, RadioButtonList, Multiline</param>
        /// <param name="FieldName">The name of the field when returned</param>
        /// <param name="CssClass">The CSS style class name of the field when returned</param>
        /// <param name="ShippingTaxRate">The tax rate for shipping to display in the list of rate options, send 0 to to add no tax</param>
        /// <returns>System.String</returns>
        public object GetRates(Packages Shipment, string Carriers, ResultType ListFormat, string FieldName, string CssClass, decimal ShippingTaxRate, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue)
        {
            // Get all carriers to retrieve rates for
            string[] carriersS = Carriers.Split(',');

            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            // Loop through & get rates
            foreach (string carrier in carriersS)
            {
                switch (carrier.Trim().ToUpperInvariant())
                {
                    case "UPS":
                        if (UPSServer.Length != 0)
                        {
                            UPSGetRates(Shipment, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                        }
                        break;

                    case "UPS2":
                        UPS2GetRates(Shipment, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                        break;
                    
                    case "USPS":
                        if (USPSServer.Length != 0)
                        {
                            if (IsDomesticCountryCode(Shipment.DestinationCountryCode))
                            {
                                USPSGetRates(Shipment, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                            }
                            else
                            {
                                USPSIntlGetRates(Shipment, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                            }
                        }
                        break;
                    case "FEDEX":
                        if (FedexServer.Length != 0)
                        {
                            FedExGetRates(Shipment, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                        }
                        break;

                    case "CANADAPOST":
                        CanadaPostGetRates(Shipment, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                        break;

                    case "AUSPOST":
                        AusPostGetRates(Shipment, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                        break;
                }
            }

            // Check list format type, and setup appropriate 
            StringBuilder output = new StringBuilder(1024);
            object returnObject = null;
            switch (ListFormat)
            {
                case ResultType.PlainText:
                    {
                        output.Append(string.Format("<SPAN ID=\"{0}\" CLASS=\"{1}\">", FieldName, CssClass));
                        output.Append("</SPAN>");
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.SingleDropDownList:
                    {
                        output.Append(String.Format("<SELECT SIZE=\"1\" NAME=\"{0}\" CLASS=\"{1}\">", FieldName, CssClass));
                        for (int i = 0; i < ratesText.Count; i++)
                        {
                            output.Append("<OPTION VALUE=\"");
                            output.Append((String)ratesValues[i].ToString());
                            output.Append("\">");
                            output.Append((String)ratesText[i].ToString());
                            output.Append("</OPTION>");
                        }
                        output.Append("</SELECT>");
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.MultiDropDownList:
                    {
                        output.Append(string.Format("<SELECT SIZE=\"5\" NAME=\"{0}\" CLASS=\"{1}\">", FieldName, CssClass));
                        for (int i = 0; i < ratesText.Count; i++)
                        {
                            output.Append("<OPTION VALUE=\"");
                            output.Append((string)ratesValues[i].ToString());
                            output.Append("\">");
                            output.Append((string)ratesText[i].ToString());
                            output.Append("</OPTION>");
                        }
                        output.Append("</SELECT>");
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.RadioButtonList:
                    {
                        output.Append(string.Format("<SPAN CLASS=\"{0}\">", CssClass));
                        for (int i = 0; i < ratesText.Count; i++)
                        {
                            string RadioId = FieldName + "_" + i.ToString();
                            output.Append("<INPUT TYPE=\"radio\" NAME=\"");
                            output.Append(FieldName);
                            output.Append("\" VALUE=\"");
                            output.Append((string)ratesValues[i].ToString());
                            output.Append("\" ID=\"");
                            output.Append(RadioId.ToString());
                            output.Append("\"><LABEL FOR=\"");
                            output.Append(RadioId.ToString());
                            output.Append("\">");
                            output.Append((string)ratesText[i].ToString());
                            output.Append("</LABEL>");
                        }
                        output.Append("</SPAN>");
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.RawDelimited:
                    {
                        String separator = String.Empty;
                        for (int i = 0; i < ratesValues.Count; i++)
                        {
                            output.Append(separator);
                            output.Append((string)ratesValues[i].ToString().Trim());
                            separator = ",";
                        }
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.DropDownListControl:
                    {
                        System.Web.UI.WebControls.DropDownList returnList = new System.Web.UI.WebControls.DropDownList();
                        returnList.CssClass = CssClass;
                        returnList.ID = FieldName;
                        for (int i = 0; i < ratesValues.Count; i++)
                        {
                            System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem();
                            item.Text = ratesText[i].ToString();
                            item.Value = ratesValues[i].ToString();
                            returnList.Items.Add(item);
                            item = null;
                        }
                        returnObject = (object)returnList;
                        break;
                    }
            }

            return returnObject;
        }

        public object GetRates(Shipments AllShipments, string Carriers, ResultType ListFormat, string FieldName, string CssClass, decimal ShippingTaxRate, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue)
        {
            return GetRates(AllShipments, Carriers, ListFormat, FieldName, CssClass, ShippingTaxRate, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, null);
        }

        /// <summary>
        /// Main method which retrieves rates. Returns a dropdown list, radio button list, or multiline select box
        /// </summary>
        /// <param name="AllShipments">The Shipments object which contains the shipments to be rated when RTShipping.MultiDistributorCalculation is true</param>
        /// <param name="Carriers">The carriers to get rates from: UPS, USPS, FedEx, DHL. Use a comma separated list</param>
        /// <param name="ListFormat">The type of list you would like back: DropDown, RadioButtonList, Multiline</param>
        /// <param name="FieldName">The name of the field when returned</param>
        /// <param name="CssClass">The CSS style class name of the field when returned</param>
        /// <param name="ShippingTaxRate">The tax rate for shipping to display in the list of rate options, send 0 to to add no tax</param>
        /// <param name="ExtraFee"></param>
        /// <param name="MarkupPercent"></param>
        /// <param name="RTShipRequest"></param>
        /// <param name="RTShipResponse"></param>
        /// <param name="ShipmentValue"></param>
        /// <param name="thisCart"></param>
        /// <returns>System.String</returns>
        public object GetRates(Shipments AllShipments, string Carriers, ResultType ListFormat, string FieldName, string CssClass, decimal ShippingTaxRate, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, CartItemCollection thisCart)
        {
            // Get all carriers to retrieve rates for
            string[] carriersS = Carriers.Split(',');

            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            // Loop through & get rates
            foreach (string carrier in carriersS)
            {
                switch (carrier.Trim().ToUpperInvariant())
                {
                case "UPS":
                    string UPSRTShipRequest;
                    string UPSRTShipResponse;

                    UPSGetRates(AllShipments, out UPSRTShipRequest, out UPSRTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);

                    RTShipRequest += "<UPSRequest>" + UPSRTShipRequest.Replace("<?xml version=\"1.0\"?>", "") + "</UPSRequest>";
                    RTShipResponse += "<UPSResponse>" + UPSRTShipResponse.Replace("<?xml version=\"1.0\"?>", "") + "</UPSResponse>";

                    break;

                case "UPS2":
                    string UPS2RTShipRequest;
                    string UPS2RTShipResponse;

                    UPS2GetRates(AllShipments, out UPS2RTShipRequest, out UPS2RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);

                    RTShipRequest += "<UPS2Request>" + UPS2RTShipRequest.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "") + "</UPS2Request>";
                    RTShipResponse += "<UPS2Response>" + UPS2RTShipResponse.Replace("<?xml version=\"1.0\"?>", "") + "</UPS2Response>";

                    break;
                
                case "USPS":
                    string USPSRTShipRequest = string.Empty;
                    string USPSRTShipResponse = string.Empty;
                    Shipments USPSDomesticShipments = new Shipments();
                    Shipments USPSIntlShipments = new Shipments();

                    foreach (Packages Shipment in AllShipments)
                    {
                        if (IsDomesticCountryCode(Shipment.DestinationCountryCode))
                        {
                            USPSDomesticShipments.AddPackages(Shipment);
                            foreach (Package p in Shipment)
                            {
                                if (p.IsFreeShipping)
                                {
                                    USPSDomesticShipments.HasFreeItems = true;
                                }
                            }
                        }
                        else
                        {
                            USPSIntlShipments.AddPackages(Shipment);
                            foreach (Package p in Shipment)
                            {
                                if (p.IsFreeShipping)
                                {
                                    USPSIntlShipments.HasFreeItems = true;
                                }
                            }
                        }
                    }
                    if (USPSDomesticShipments.Count > 0)
                    {
                        USPSGetRates(USPSDomesticShipments, out USPSRTShipRequest, out USPSRTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                    }
                    if (USPSIntlShipments.Count > 0)
                    {
                        USPSIntlGetRates(USPSIntlShipments, out USPSRTShipRequest, out USPSRTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                    }

                    RTShipRequest += "<USPSRequest>" + USPSRTShipRequest.Replace("API=RateV2&Xml=", "") + "</USPSRequest>";
                    RTShipResponse += "<USPSResponse>" + USPSRTShipResponse.Replace("<?xml version=\"1.0\"?>", "") + "</USPSResponse>";

                    break;

                    case "FEDEX":
                        string FedExRTShipRequest = string.Empty;
                        string FedExRTShipResponse = string.Empty;

                        if (AllShipments.Count > 0)
                        {
                            FedExGetRates(AllShipments, out FedExRTShipRequest, out FedExRTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                        }

                        RTShipRequest += "<FedExRequest>" + FedExRTShipRequest.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>", "") + "</FedExRequest>";
                        RTShipResponse += "<FedExResponse>" + FedExRTShipResponse.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "") + "</FedExResponse>";

                        break;

                    case "DHL":
                        string DHLRTShipRequest = string.Empty;
                        string DHLRTShipResponse = string.Empty;
                        Shipments DHLIntlShipments = new Shipments();

                        foreach (Packages Shipment in AllShipments)
                        {
                            if (!IsDomesticCountryCode(Shipment.DestinationCountryCode))
                            {
                                DHLIntlShipments.AddPackages(Shipment);
                                foreach (Package p in Shipment)
                                {
                                    if (p.IsFreeShipping) DHLIntlShipments.HasFreeItems = true;
                                }
                            }
                        }

                        if (DHLIntlShipments.Count > 0)
                        {
                            DHLIntlGetRates(DHLIntlShipments, out DHLRTShipRequest, out DHLRTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);
                        }

                        RTShipRequest += "<DHLRequest>" + DHLRTShipRequest + "</DHLRequest>";
                        RTShipResponse += "<DHLResponse>" + DHLRTShipResponse + "</DHLResponse>";

                        break;

                    case "CANADAPOST":
                        string CanadaPostRTShipRequest;
                        string CanadaPostRTShipResponse;

                        CanadaPostGetRates(AllShipments, out CanadaPostRTShipRequest, out CanadaPostRTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);

                        RTShipRequest += "<CanadaPostRequest>" + CanadaPostRTShipRequest.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "") + "</CanadaPostRequest>";
                        if (CanadaPostRTShipResponse.IndexOf("]>") > 0)
                        {
                            RTShipResponse += "<CanadaPostResponse>" + CanadaPostRTShipResponse.Substring(CanadaPostRTShipResponse.IndexOf("]>") + 2) + "</CanadaPostResponse>";
                        }
                        break;

                    case "AUSPOST":
                        string AusPostRTShipRequest;
                        string AusPostRTShipResponse;

                        AusPostGetRates(AllShipments, out AusPostRTShipRequest, out AusPostRTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);

                        RTShipRequest += "<AusPostRequest><![CDATA[" + AusPostRTShipRequest + "]]></AusPostRequest>";
                        RTShipResponse += "<AusPostResponse><![CDATA[" + AusPostRTShipResponse + "]]></AusPostResponse>";

                        break;
                }
            }

            // optionally sort rates by cost
            if (AppLogic.AppConfigBool("RTShipping.SortByRate"))
            {
                //Sort shipping costs low to high
                ShippingCostsSorter rtsComparer = new ShippingCostsSorter();
                SM.Sort(rtsComparer);
            }

			//Apply shipping method display names
			string sql = "select Name, DisplayName from ShippingMethod where DisplayName <> ''";
			using(SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
			{
				dbconn.Open();
				using(IDataReader rs = DB.GetRS(sql, dbconn))
				{
					while(rs.Read())
					{
						var displayName = DB.RSField(rs, "DisplayName");
						var methodName = DB.RSField(rs, "Name");
						foreach(ShipMethod method in SM)
						{
							if(method.ServiceName == methodName)
								method.DisplayName = displayName;
						}
					}
				}
			}

            // Check list format type, and setup appropriate 
            StringBuilder output = new StringBuilder(1024);
            object returnObject = null;
            switch (ListFormat)
            {
                case ResultType.PlainText:
                    {
                        output.Append(string.Format("<SPAN ID=\"{0}\" CLASS=\"{1}\">", FieldName, CssClass));
                        for (int i = 0; i < ratesText.Count; i++)
                        {
                            output.Append((string)ratesText[i].ToString());
                            output.Append("");
                        }
                        output.Append("</SPAN>");
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.SingleDropDownList:
                    {
                        output.Append(String.Format("<SELECT SIZE=\"1\" NAME=\"{0}\" CLASS=\"{1}\">", FieldName, CssClass));
                        for (int i = 0; i < ratesText.Count; i++)
                        {
                            output.Append("<OPTION VALUE=\"");
                            output.Append((String)ratesValues[i].ToString());
                            output.Append("\">");
                            output.Append((String)ratesText[i].ToString());
                            output.Append("</OPTION>");
                        }
                        output.Append("</SELECT>");
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.MultiDropDownList:
                    {
                        output.Append(string.Format("<SELECT SIZE=\"5\" NAME=\"{0}\" CLASS=\"{1}\">", FieldName, CssClass));
                        for (int i = 0; i < ratesText.Count; i++)
                        {
                            output.Append("<OPTION VALUE=\"");
                            output.Append((string)ratesValues[i].ToString());
                            output.Append("\">");
                            output.Append((string)ratesText[i].ToString());
                            output.Append("</OPTION>");
                        }
                        output.Append("</SELECT>");
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.RadioButtonList:
                    {
                        output.Append(string.Format("<SPAN CLASS=\"{0}\">", CssClass));
                        for (int i = 0; i < ratesText.Count; i++)
                        {
                            string RadioId = FieldName + "_" + i.ToString();
                            output.Append("<INPUT TYPE=\"radio\" NAME=\"");
                            output.Append(FieldName);
                            output.Append("\" VALUE=\"");
                            output.Append((string)ratesValues[i].ToString());
                            output.Append("\" ID=\"");
                            output.Append(RadioId.ToString());
                            output.Append("\"><LABEL FOR=\"");
                            output.Append(RadioId.ToString());
                            output.Append("\">");
                            output.Append((string)ratesText[i].ToString());
                            output.Append("</LABEL>");
                        }
                        output.Append("</SPAN>");
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.RawDelimited:
                    {
                        String separator = String.Empty;
                        for (int i = 0; i < ratesValues.Count; i++)
                        {
                            output.Append(separator);
                            output.Append((string)ratesValues[i].ToString().Trim());
                            separator = ",";
                        }
                        String tmpS = output.ToString();
                        returnObject = (object)tmpS;
                        break;
                    }
                case ResultType.DropDownListControl:
                    {
                        System.Web.UI.WebControls.DropDownList returnList = new System.Web.UI.WebControls.DropDownList();
                        returnList.CssClass = CssClass;
                        returnList.ID = FieldName;
                        for (int i = 0; i < ratesValues.Count; i++)
                        {
                            System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem();
                            item.Text = ratesText[i].ToString();
                            item.Value = ratesValues[i].ToString();
                            returnList.Items.Add(item);
                            item = null;
                        }
                        returnObject = (object)returnList;
                        break;
                    }
                case ResultType.CollectionList:
                    {
                        returnObject = (object)SM;
                        break;
                    }
            }

            return returnObject;
        }

        //had to leave for google checkout...google checkout does not support shipping by multiple distributors
        private void UPSGetRates(Packages Shipment, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)	// Private method to retrieve UPS rates
        {
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;
            // check all required info
            if (m_upsLogin == string.Empty || m_upsUsername == string.Empty || m_upsPassword == string.Empty || m_upsLicense == string.Empty)
            {
                SM.ErrorMsg = "Error: You must provide UPS login information";
                return;
            }

            if (Shipment.DestinationStateProvince == "AE")
            {
                SM.ErrorMsg = "UPS Does not ship to APO Boxes";
                return;
            }

            // Check for test mode
            if (m_TestMode)
            {
                m_upsServer = AppLogic.AppConfig("RTShipping.UPS.TestServer");
            }

            // Check server setting
            if (m_upsServer == string.Empty)
            {
                SM.ErrorMsg = "Error: You must provide the UPS server";
                return;
            }

            // Check for m_ShipmentWeight
            if (m_ShipmentWeight == 0.0M)
            {
                SM.ErrorMsg = "Error: Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".";
                return;
            }

            Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.UPS.MaxWeight");
            if (maxWeight == 0)
            {
                maxWeight = 150;
            }

            if (m_ShipmentWeight > maxWeight)
            {
                SM.ErrorMsg = "UPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                return;
            }

            // Set the access request Xml
            String accessRequest = string.Format("<?xml version=\"1.0\"?><AccessRequest xml:lang=\"en-us\"><AccessLicenseNumber>{0}</AccessLicenseNumber><UserId>{1}</UserId><Password>{2}</Password></AccessRequest>", this.m_upsLicense, this.m_upsUsername, this.m_upsPassword);

            // Set the rate request Xml
            StringBuilder shipmentRequest = new StringBuilder(1024);
            shipmentRequest.Append("<?xml version=\"1.0\"?>");
            shipmentRequest.Append("<RatingServiceSelectionRequest xml:lang=\"en-US\">");
            shipmentRequest.Append("<Request>");
            shipmentRequest.Append("<RequestAction>Rate</RequestAction>");
            shipmentRequest.Append("<RequestOption>Shop</RequestOption>");
            shipmentRequest.Append("<TransactionReference>");
            shipmentRequest.Append("<CustomerContext>Rating and Service</CustomerContext>");
            shipmentRequest.Append("<XpciVersion>1.0001</XpciVersion>");
            shipmentRequest.Append("</TransactionReference>");
            shipmentRequest.Append("</Request>");
            shipmentRequest.Append("<PickupType>");
            shipmentRequest.Append("<Code>");
            shipmentRequest.Append(MapPickupType(Shipment.PickupType));
            shipmentRequest.Append("</Code>");
            shipmentRequest.Append("</PickupType>");
            //Add proper elements to support SuggestedRetailRates
            if (AppLogic.AppConfig("RTShipping.UPS.UPSPickupType").Equals("UPSSUGGESTEDRETAILRATES", StringComparison.InvariantCultureIgnoreCase))
            {
                shipmentRequest.Append("<CustomerClassification>");
                shipmentRequest.Append("<Code>04</Code>");
                shipmentRequest.Append("</CustomerClassification>");
            }
            shipmentRequest.Append("<Shipment>");
            shipmentRequest.Append("<Shipper>");
            shipmentRequest.Append("<Address>");
            shipmentRequest.Append("<City>");
            shipmentRequest.Append(m_OriginCity.ToUpperInvariant());
            shipmentRequest.Append("</City>");
            shipmentRequest.Append("<StateProvinceCode>");
            shipmentRequest.Append(m_OriginStateProvince.ToUpperInvariant());
            shipmentRequest.Append("</StateProvinceCode>");
            shipmentRequest.Append("<PostalCode>");
            shipmentRequest.Append(m_OriginZipPostalCode);
            shipmentRequest.Append("</PostalCode>");
            shipmentRequest.Append("<CountryCode>");
            shipmentRequest.Append(m_OriginCountry.ToUpperInvariant());
            shipmentRequest.Append("</CountryCode>");
            shipmentRequest.Append("</Address>");
            shipmentRequest.Append("</Shipper>");
            shipmentRequest.Append("<ShipTo>");
            shipmentRequest.Append("<Address>");
            shipmentRequest.Append("<City>");
            shipmentRequest.Append(Shipment.DestinationCity.ToUpperInvariant());
            shipmentRequest.Append("</City>");
            shipmentRequest.Append("<StateProvinceCode>");
            shipmentRequest.Append(Shipment.DestinationStateProvince.ToUpperInvariant());
            shipmentRequest.Append("</StateProvinceCode>");
            shipmentRequest.Append("<PostalCode>");
            shipmentRequest.Append(Shipment.DestinationZipPostalCode);
            shipmentRequest.Append("</PostalCode>");
            shipmentRequest.Append("<CountryCode>");
            shipmentRequest.Append(Shipment.DestinationCountryCode.ToUpperInvariant());
            shipmentRequest.Append("</CountryCode>");
            shipmentRequest.Append(CommonLogic.IIF(Shipment.DestinationResidenceType == ResidenceTypes.Commercial, "", "<ResidentialAddressIndicator/>"));
            shipmentRequest.Append("</Address>");
            shipmentRequest.Append("</ShipTo>");
            shipmentRequest.Append("<ShipmentWeight>");
            shipmentRequest.Append("<UnitOfMeasurement>");
            shipmentRequest.Append("<Code>");
            shipmentRequest.Append(AppLogic.AppConfig("RTShipping.WeightUnits").Trim().ToUpperInvariant());
            shipmentRequest.Append("</Code>");
            shipmentRequest.Append("</UnitOfMeasurement>");
            shipmentRequest.Append("<Weight>");
            shipmentRequest.Append(Localization.DecimalStringForDB(Shipment.Weight));
            shipmentRequest.Append("</Weight>");
            shipmentRequest.Append("</ShipmentWeight>");


            // loop through the packages
            foreach (Package p in Shipment)
            {
                
                //Check for invalid weights and assign a new value if necessary
                if (p.Weight < AppLogic.AppConfigUSDecimal("UPS.MinimumPackageWeight"))
                {
                    p.Weight = AppLogic.AppConfigUSDecimal("UPS.MinimumPackageWeight");
                }

                shipmentRequest.Append("<Package>");
                shipmentRequest.Append("<PackagingType>");
                shipmentRequest.Append("<Code>02</Code>");
                shipmentRequest.Append("</PackagingType>");
                shipmentRequest.Append("<Dimensions>");
                shipmentRequest.Append("<UnitOfMeasurement>");

                if (AppLogic.AppConfig("RTShipping.WeightUnits").Trim().Equals("LBS", StringComparison.InvariantCultureIgnoreCase))
                    shipmentRequest.Append("<Code>IN</Code>");
                else
                    shipmentRequest.Append("<Code>CM</Code>");

                shipmentRequest.Append("</UnitOfMeasurement>");
                shipmentRequest.Append("<Length>");
                shipmentRequest.Append(p.Length.ToString());
                shipmentRequest.Append("</Length>");
                shipmentRequest.Append("<Width>");
                shipmentRequest.Append(p.Width.ToString());
                shipmentRequest.Append("</Width>");
                shipmentRequest.Append("<Height>");
                shipmentRequest.Append(p.Height.ToString());
                shipmentRequest.Append("</Height>");
                shipmentRequest.Append("</Dimensions>");
                shipmentRequest.Append("<Description>");
                shipmentRequest.Append(p.PackageId.ToString());
                shipmentRequest.Append("</Description>");
                shipmentRequest.Append("<PackageWeight>");
                shipmentRequest.Append("<UnitOfMeasure>");
                shipmentRequest.Append("<Code>");
                shipmentRequest.Append(AppLogic.AppConfig("RTShipping.WeightUnits").Trim().ToUpperInvariant());
                shipmentRequest.Append("</Code>");
                shipmentRequest.Append("</UnitOfMeasure>");
                shipmentRequest.Append("<Weight>");
                shipmentRequest.Append(Localization.DecimalStringForDB(p.Weight));
                shipmentRequest.Append("</Weight>");
                shipmentRequest.Append("</PackageWeight>");
                shipmentRequest.Append("<OversizePackage />");

                if (p.Insured && (p.InsuredValue != 0))
                {
                    shipmentRequest.Append("<PackageServiceOptions>");
                    shipmentRequest.Append("<InsuredValue>");
                    shipmentRequest.Append("<CurrencyCode>USD</CurrencyCode>");
                    shipmentRequest.Append("<MonetaryValue>");
                    shipmentRequest.Append(Localization.CurrencyStringForDBWithoutExchangeRate(p.InsuredValue));
                    shipmentRequest.Append("</MonetaryValue>");
                    shipmentRequest.Append("</InsuredValue>");
                    shipmentRequest.Append("</PackageServiceOptions>");
                }

                shipmentRequest.Append("</Package>");
            }

            shipmentRequest.Append("<ShipmentServiceOptions/></Shipment></RatingServiceSelectionRequest>");

            // Concat the requests
            String fullUPSRequest = accessRequest + shipmentRequest.ToString();

            RTShipRequest = fullUPSRequest;

            // Send request & capture response

            string result = POSTandReceiveData(fullUPSRequest, m_upsServer);

            RTShipResponse = result;

            // Load Xml into a XmlDocument object
            XmlDocument UPSResponse = new XmlDocument();
            try
            {
                UPSResponse.LoadXml(result);
            }
            catch
            {
                SM.ErrorMsg = "Error: UPS Gateway Did Not Respond";
                return;
            }

            // Get Response code: 0 = Fail, 1 = Success
            XmlNodeList UPSResponseCode = UPSResponse.GetElementsByTagName("ResponseStatusCode");

            if (UPSResponseCode[0].InnerText == "1") // Success
                    {
                        // Loop through elements & get rates
                        XmlNodeList ratedShipments = UPSResponse.GetElementsByTagName("RatedShipment");
                        string tempService = string.Empty;
                        Decimal tempRate = 0.0M;
                        for (int i = 0; i < ratedShipments.Count; i++)
                        {
                            XmlNode shipmentX = ratedShipments.Item(i);
                            tempService = UPSServiceCodeDescription(shipmentX["Service"]["Code"].InnerText);

                            if (ShippingMethodIsAllowed(tempService, "UPS"))
                            {
                                tempRate = Localization.ParseUSDecimal(shipmentX["TotalCharges"]["MonetaryValue"].InnerText);

                                if (MarkupPercent != System.Decimal.Zero)
                                {
                                    tempRate = Decimal.Round(tempRate * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
                                }
                                tempRate += ExtraFee;


                                decimal vat = Decimal.Round(tempRate * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);

                                ShipMethod s_method = new ShipMethod();

                                s_method.Carrier = "UPS";
                                s_method.ServiceName = tempService;
                                s_method.ServiceRate = tempRate;
                                s_method.VatRate = vat;
                                SM.AddMethod(s_method);
                                
                            }
                        }
                    }
            else // Error
            {
                XmlNodeList UPSError = UPSResponse.GetElementsByTagName("ErrorDescription");
                SM.ErrorMsg = "UPS Error: " + UPSError[0].InnerText;
                UPSError = null;
                return;
            }

            // Some clean up
            UPSResponseCode = null;
            UPSResponse = null;
        }

        private void UPSGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)	// Private method to retrieve UPS rates
        {
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            HasFreeItems = false;
            PackageQuantity = 1;

            // check all required info
            if (m_upsLogin == string.Empty || m_upsUsername == string.Empty || m_upsPassword == string.Empty || m_upsLicense == string.Empty)
            {
                SM.ErrorMsg = "Error: You must provide UPS login information";
                return;
            }

            foreach (Packages ps in AllShipments)
            {
                if (ps.DestinationStateProvince == "AE")
                {
                    SM.ErrorMsg = "UPS Does not ship to APO Boxes";
                    return;
                }
            }
            
            // Check for test mode
            if (m_TestMode)
            {
                m_upsServer = AppLogic.AppConfig("RTShipping.UPS.TestServer");
            }

            // Check server setting
            if (m_upsServer == string.Empty)
            {
                SM.ErrorMsg = "Error: You must provide the UPS Server";
                return;
            }

            // Check for m_ShipmentWeight
            if (m_ShipmentWeight == 0.0M)
            {
                SM.ErrorMsg = "Error: Shipment Weight must be great than 0 " + Localization.WeightUnits() + ".";
                return;
            }

            Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.UPS.MaxWeight");
            if (maxWeight == 0)
            {
                maxWeight = 150;
            }

            if (m_ShipmentWeight > maxWeight)
            {
                SM.ErrorMsg = "UPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                return;
            }

            // Set the access request Xml
            String accessRequest = String.Empty;
            StringBuilder shipmentRequest = new StringBuilder(1024);

            Boolean MultiDistributorEnabled = AppLogic.AppConfigBool("RTShipping.MultiDistributorCalculation") && AllShipments.HasDistributorItems;
                
            HasFreeItems = false;
            PackageQuantity = 1;

            foreach (Packages Shipment in AllShipments)
            {
                shipmentRequest = new StringBuilder(1024);

                accessRequest = string.Format("<?xml version=\"1.0\"?><AccessRequest xml:lang=\"en-us\"><AccessLicenseNumber>{0}</AccessLicenseNumber><UserId>{1}</UserId><Password>{2}</Password></AccessRequest>", this.m_upsLicense, this.m_upsUsername, this.m_upsPassword);

                // Set the rate request Xml

                shipmentRequest.Append("<?xml version=\"1.0\"?>");
                shipmentRequest.Append("<RatingServiceSelectionRequest xml:lang=\"en-US\">");
                shipmentRequest.Append("<Request>");
                shipmentRequest.Append("<RequestAction>Rate</RequestAction>");
                shipmentRequest.Append("<RequestOption>Shop</RequestOption>");
                shipmentRequest.Append("<TransactionReference>");
                shipmentRequest.Append("<CustomerContext>Rating and Service</CustomerContext>");
                shipmentRequest.Append("<XpciVersion>1.0001</XpciVersion>");
                shipmentRequest.Append("</TransactionReference>");
                shipmentRequest.Append("</Request>");
                shipmentRequest.Append("<PickupType>");
                shipmentRequest.Append("<Code>");
                shipmentRequest.Append(MapPickupType(Shipment.PickupType));
                shipmentRequest.Append("</Code>");
                shipmentRequest.Append("</PickupType>");
                //Add proper elements to support SuggestedRetailRates
                if (AppLogic.AppConfig("RTShipping.UPS.UPSPickupType").Equals("UPSSUGGESTEDRETAILRATES", StringComparison.InvariantCultureIgnoreCase))
                {
                    shipmentRequest.Append("<CustomerClassification>");
                    shipmentRequest.Append("<Code>04</Code>");
                    shipmentRequest.Append("</CustomerClassification>");
                }
                shipmentRequest.Append("<Shipment>");
                shipmentRequest.Append("<Shipper>");
                shipmentRequest.Append("<Address>");
                shipmentRequest.Append("<City>");
                if (MultiDistributorEnabled)
                    shipmentRequest.Append(CommonLogic.IIF(Shipment.OriginCity == "", m_OriginCity.ToUpperInvariant(), Shipment.OriginCity.ToUpperInvariant()));
                else
                    shipmentRequest.Append(m_OriginCity.ToUpperInvariant());
                shipmentRequest.Append("</City>");
                shipmentRequest.Append("<StateProvinceCode>");
                if (MultiDistributorEnabled)
                    shipmentRequest.Append(CommonLogic.IIF(Shipment.OriginStateProvince == "", m_OriginStateProvince.ToUpperInvariant(), Shipment.OriginStateProvince.ToUpperInvariant()));
                else
                    shipmentRequest.Append(m_OriginStateProvince.ToUpperInvariant());
                shipmentRequest.Append("</StateProvinceCode>");
                shipmentRequest.Append("<PostalCode>");
                if(!MultiDistributorEnabled || Shipment.OriginZipPostalCode == "")
                    shipmentRequest.Append(m_OriginZipPostalCode);
                else
                    shipmentRequest.Append(Shipment.OriginZipPostalCode);
                shipmentRequest.Append("</PostalCode>");
                shipmentRequest.Append("<CountryCode>");
                if(MultiDistributorEnabled)
                    shipmentRequest.Append(CommonLogic.IIF(Shipment.OriginCountryCode == "", m_OriginCountry.ToUpperInvariant(), Shipment.OriginCountryCode.ToUpperInvariant()));
                else
                    shipmentRequest.Append(m_OriginCountry.ToUpperInvariant());
                shipmentRequest.Append("</CountryCode>");
                shipmentRequest.Append("</Address>");
                shipmentRequest.Append("</Shipper>");
                shipmentRequest.Append("<ShipTo>");
                shipmentRequest.Append("<Address>");
                shipmentRequest.Append("<City>");
                shipmentRequest.Append(Shipment.DestinationCity.ToUpperInvariant());
                shipmentRequest.Append("</City>");
                shipmentRequest.Append("<StateProvinceCode>");
                shipmentRequest.Append(Shipment.DestinationStateProvince.ToUpperInvariant());
                shipmentRequest.Append("</StateProvinceCode>");
                shipmentRequest.Append("<PostalCode>");
                shipmentRequest.Append(Shipment.DestinationZipPostalCode);
                shipmentRequest.Append("</PostalCode>");
                shipmentRequest.Append("<CountryCode>");
                shipmentRequest.Append(Shipment.DestinationCountryCode.ToUpperInvariant());
                shipmentRequest.Append("</CountryCode>");
                shipmentRequest.Append(CommonLogic.IIF(Shipment.DestinationResidenceType == ResidenceTypes.Commercial, "", "<ResidentialAddressIndicator/>"));
                shipmentRequest.Append("</Address>");
                shipmentRequest.Append("</ShipTo>");
                shipmentRequest.Append("<ShipmentWeight>");
                shipmentRequest.Append("<UnitOfMeasurement>");
                shipmentRequest.Append("<Code>");
                shipmentRequest.Append(AppLogic.AppConfig("RTShipping.WeightUnits").Trim().ToUpperInvariant());
                shipmentRequest.Append("</Code>");
                shipmentRequest.Append("</UnitOfMeasurement>");
                shipmentRequest.Append("<Weight>");
                shipmentRequest.Append(Localization.DecimalStringForDB(Shipment.Weight));
                shipmentRequest.Append("</Weight>");
                shipmentRequest.Append("</ShipmentWeight>");


                // loop through the packages
                foreach (Package p in Shipment)
                {
                    //can do this because any Shipment that has free items will only have 1 Package p
                    if (p.IsFreeShipping)
                    {
                        HasFreeItems = true;
                    }

                    //can do this because any Shipment that has IsShipSeparately items will only have 1 Package p
                    //sanity check to make sure we don't call the server 100 times for each carrier if 100 of an
                    //IsShipSeparately item is ordered.  ShoppingCart.cs will add 1 Package p with a quantity
                    //if the item IsShipSeparately
                    if (p.IsShipSeparately)
                    {
                        PackageQuantity = p.Quantity;
                    }

                    //Check for invalid weights and assign a new value if necessary
                    if (p.Weight < AppLogic.AppConfigUSDecimal("UPS.MinimumPackageWeight"))
                    {
                        p.Weight = AppLogic.AppConfigUSDecimal("UPS.MinimumPackageWeight");
                    }

                    shipmentRequest.Append("<Package>");
                    shipmentRequest.Append("<PackagingType>");
                    shipmentRequest.Append("<Code>02</Code>");
                    shipmentRequest.Append("</PackagingType>");
                    shipmentRequest.Append("<Dimensions>");
                    shipmentRequest.Append("<UnitOfMeasurement>");

                    if (AppLogic.AppConfig("RTShipping.WeightUnits").Trim().Equals("LBS", StringComparison.InvariantCultureIgnoreCase))
                        shipmentRequest.Append("<Code>IN</Code>");
                    else
                        shipmentRequest.Append("<Code>CM</Code>");

                    shipmentRequest.Append("</UnitOfMeasurement>");
                    shipmentRequest.Append("<Length>");
                    shipmentRequest.Append(p.Length.ToString());
                    shipmentRequest.Append("</Length>");
                    shipmentRequest.Append("<Width>");
                    shipmentRequest.Append(p.Width.ToString());
                    shipmentRequest.Append("</Width>");
                    shipmentRequest.Append("<Height>");
                    shipmentRequest.Append(p.Height.ToString());
                    shipmentRequest.Append("</Height>");
                    shipmentRequest.Append("</Dimensions>");
                    shipmentRequest.Append("<Description>");
                    shipmentRequest.Append(p.PackageId.ToString());
                    shipmentRequest.Append("</Description>");
                    shipmentRequest.Append("<PackageWeight>");
                    shipmentRequest.Append("<UnitOfMeasure>");
                    shipmentRequest.Append("<Code>");
                    shipmentRequest.Append(AppLogic.AppConfig("RTShipping.WeightUnits").Trim().ToUpperInvariant());
                    shipmentRequest.Append("</Code>");
                    shipmentRequest.Append("</UnitOfMeasure>");
                    shipmentRequest.Append("<Weight>");
                    shipmentRequest.Append(Localization.DecimalStringForDB(p.Weight));
                    shipmentRequest.Append("</Weight>");
                    shipmentRequest.Append("</PackageWeight>");
                    shipmentRequest.Append("<OversizePackage />");

                    if (p.Insured && (p.InsuredValue != 0))
                    {
                        shipmentRequest.Append("<PackageServiceOptions>");
                        shipmentRequest.Append("<InsuredValue>");
                        shipmentRequest.Append("<CurrencyCode>USD</CurrencyCode>");
                        shipmentRequest.Append("<MonetaryValue>");
                        shipmentRequest.Append(Localization.CurrencyStringForDBWithoutExchangeRate(p.InsuredValue));
                        shipmentRequest.Append("</MonetaryValue>");
                        shipmentRequest.Append("</InsuredValue>");
                        shipmentRequest.Append("</PackageServiceOptions>");
                    }

                    shipmentRequest.Append("</Package>");
                }

                shipmentRequest.Append("<ShipmentServiceOptions/></Shipment></RatingServiceSelectionRequest>");

                // Concat the requests
                String fullUPSRequest = accessRequest + shipmentRequest.ToString();

                RTShipRequest = fullUPSRequest;

                // Send request & capture response

                string result = POSTandReceiveData(fullUPSRequest, m_upsServer);

                RTShipResponse = result;

                // Load Xml into a XmlDocument object
                XmlDocument UPSResponse = new XmlDocument();
                try
                {
                    UPSResponse.LoadXml(result);
                }
                catch
                {
                    SM.ErrorMsg = "Error: UPS Gateway Did Not Respond";
                    return;
                }

                // Get Response code: 0 = Fail, 1 = Success
                XmlNodeList UPSResponseCode = UPSResponse.GetElementsByTagName("ResponseStatusCode");

                if (UPSResponseCode[0].InnerText == "1") // Success
                {
                    // Loop through elements & get rates
                    XmlNodeList ratedShipments = UPSResponse.GetElementsByTagName("RatedShipment");
                    string tempService = string.Empty;
                    Decimal tempRate = 0.0M;
                    for (int i = 0; i < ratedShipments.Count; i++)
                    {
                        XmlNode shipmentX = ratedShipments.Item(i);
                        tempService = UPSServiceCodeDescription(shipmentX["Service"]["Code"].InnerText);
                            
                        if (ShippingMethodIsAllowed(tempService, "UPS"))
                        {
                            tempRate = Localization.ParseUSDecimal(shipmentX["TotalCharges"]["MonetaryValue"].InnerText);

                            //multiply the returned rate by the quantity in the package to avoid calling
                            //UPS more than necessary if there were multiple IsShipSeparately items
                            //ordered.  If there weren't, PackageQuantity is 1 and the rate is normal
                            tempRate = tempRate * PackageQuantity;

                            if (MarkupPercent != System.Decimal.Zero)
                            {
                                tempRate = Decimal.Round(tempRate * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
                            }

                            decimal vat = Decimal.Round(tempRate * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                            tempService = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(tempService));
                            if (!SM.MethodExists(tempService))
                            {
                                ShipMethod s_method = new ShipMethod();

                                s_method.Carrier = "UPS";
                                s_method.ServiceName = tempService;
                                s_method.ServiceRate = tempRate;
                                s_method.VatRate = vat;
                                if (HasFreeItems)
                                {
                                    s_method.FreeItemsRate = tempRate;
                                }
                                SM.AddMethod(s_method);
                            }
                            else
                            {
                                int IndexOf = SM.GetIndex(tempService);
                                ShipMethod s_method = SM[IndexOf];
                                    
                                s_method.ServiceRate += tempRate;
                                s_method.VatRate += vat;
                                if (HasFreeItems)
                                {
                                    s_method.FreeItemsRate += tempRate;
                                }
                                SM[IndexOf] = s_method;
                            }
                        }
                    }
                }
                else // Error
                {
                    XmlNodeList UPSError = UPSResponse.GetElementsByTagName("ErrorDescription");
                    SM.ErrorMsg = "UPS Error: " + UPSError[0].InnerText;
                    UPSError = null;
                    return;
                }

                // Some clean up
                UPSResponseCode = null;
                UPSResponse = null;
                HasFreeItems = false;
                PackageQuantity = 1;
            }

            // Handling fee should only be added per shipping address not per package
            // let's just compute it here after we've gone through all the packages.
            // Also, since we can't be sure about the ordering of the method call here
            // and that the collection SM includes shipping methods from all possible carriers
            // we'll need to filter out the methods per this carrier to avoid side effects on the main collection
            foreach (ShipMethod shipMethod in SM.PerCarrier("UPS"))
            {
                shipMethod.ServiceRate += ExtraFee;
            }
        }

        private void USPSIntlGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate) // Retrieves International rates for USPS
        {
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            foreach (Packages Shipment in AllShipments)
            {

                // check all required info
                if (Shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase))
                {
                    SM.ErrorMsg = "Error: Calling USPS International but shipping to US country";
                    return; // error
                }

                if (m_uspsLogin == string.Empty || m_uspsUsername == string.Empty)
                {
                    SM.ErrorMsg = "Error: You must provide USPS login information";
                    return;
                }

                // Check server setting
                if (m_uspsServer == string.Empty)
                {
                    SM.ErrorMsg = "Error: You must provide the USPS server";
                    return;
                }

                // Check for test mode
                if (m_TestMode)
                {
                    m_uspsServer = AppLogic.AppConfig("RTShipping.USPS.TestServer");
                }

                // Check for m_ShipmentWeight
                if (ShipmentWeight == 0.0M)
                {
                    SM.ErrorMsg = "Error: Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".";
                    return;
                }

                Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.USPS.MaxWeight");
                if (maxWeight == 0)
                {
                    maxWeight = 70;
                }

                if (ShipmentWeight > maxWeight)
                {
                    SM.ErrorMsg = "USPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                    return;
                }


                HasFreeItems = false;
                PackageQuantity = 1;

                // Create the Xml request (International)
                StringBuilder USPSRequest = new StringBuilder(1024);
                USPSRequest.Append("API=IntlRateV2&Xml=");

                StringBuilder uspsReqLoop = new StringBuilder(1024);
                uspsReqLoop.Append("<IntlRateV2Request USERID=\"{0}\">");
                foreach (Package p in Shipment)
                {
                    USPSWeight w = USPSGetWeight(p.Weight);

                    //can do this because any Shipment that has free items will only have 1 Package p
                    if (p.IsFreeShipping)
                    {
                        HasFreeItems = true;

                    }

                    //can do this because any Shipment that has IsShipSeparately items will only have 1 Package p
                    //sanity check to make sure we don't call the server 100 times for each carrier if 100 of an
                    //IsShipSeparately item is ordered.  ShoppingCart.cs will add 1 Package p with a quantity
                    //if the item IsShipSeparately
                    if (p.IsShipSeparately)
                    {
                        PackageQuantity = p.Quantity;
                    }

                    uspsReqLoop.Append("<Package ID=\""+ p.PackageId + "\">");
                    uspsReqLoop.Append("<Pounds>" + w.pounds + "</Pounds>");
                    uspsReqLoop.Append("<Ounces>" + w.ounces + "</Ounces>");
                    uspsReqLoop.Append("<Machinable>True</Machinable>");
                    uspsReqLoop.Append("<MailType>Package</MailType>");
                    uspsReqLoop.Append("<ValueOfContents>" + p.InsuredValue + "</ValueOfContents>");
                    
                    if (Shipment.DestinationCountryCode.Equals("GB", StringComparison.InvariantCultureIgnoreCase))
                    {
                        uspsReqLoop.Append("<Country>United Kingdom (Great Britain)</Country>");
                    }
                    else
                    {
                        uspsReqLoop.Append("<Country>");
                        uspsReqLoop.Append(AppLogic.GetCountryName(Shipment.DestinationCountryCode));
                        uspsReqLoop.Append("</Country>");
                    }
                    
                    uspsReqLoop.Append("<Container>RECTANGULAR</Container>");
                    uspsReqLoop.Append(USPSGetSize(p.Length, p.Width, p.Height));
                    uspsReqLoop.Append("</Package>");
                }
                USPSRequest.Append(uspsReqLoop);
                USPSRequest.Append("</IntlRateV2Request>");

                // Replace login info
                String USPSRequest2 = string.Format(USPSRequest.ToString(), USPSUsername, USPSPassword);
                RTShipRequest += USPSRequest2;

                // Send request & capture response
                string result = GETandReceiveData(USPSRequest2, USPSServer);
                RTShipResponse += result;

                // Load Xml into a XmlDocument object
                XmlDocument USPSResponse = new XmlDocument();
                try
                {
                    USPSResponse.LoadXml(result);
                }
                catch
                {
                    SM.ErrorMsg = "Error: USPS Gateway Did Not Respond";
                    return;
                }

                // Check for error
                XmlNodeList USPSErrors = USPSResponse.GetElementsByTagName("Error");
                if (USPSErrors.Count > 0) // Error has occurred
                {
                    XmlNodeList USPSError = USPSResponse.GetElementsByTagName("Error");
                    XmlNode USPSErrorMessage = USPSError.Item(0);
                    ratesText.Add("USPS Error: " + USPSErrorMessage["Description"].InnerText);
                    ratesValues.Add("USPS Error: " + USPSErrorMessage["Description"].InnerText);
                    USPSError = null;
                    return;
                }
                else
                {
                    XmlNodeList nodesPackages = USPSResponse.GetElementsByTagName("Package");
                    foreach (XmlNode nodePackage in nodesPackages)
                    {
                        XmlNodeList nodesServices = nodePackage.SelectNodes("Service");
                        foreach (XmlNode nodeService in nodesServices)
                        {
                            string rateName = nodeService.SelectSingleNode("SvcDescription").InnerText;
                            if (ShippingMethodIsAllowed("U.S. Postal " + rateName, "USPS") && rateName.IndexOf("Envelope") == -1 && rateName.IndexOf(" Document") == -1 && rateName.IndexOf("Letter") == -1)
                            {
                                decimal totalCharges = Localization.ParseUSDecimal(nodeService.SelectSingleNode("Postage").InnerText);

                                //multiply the returned rate by the quantity in the package to avoid calling
                                //more than necessary if there were multiple IsShipSeparately items
                                //ordered.  If there weren't, PackageQuantity is 1 and the rate is normal
                                totalCharges = totalCharges * PackageQuantity;

                                if (MarkupPercent != System.Decimal.Zero)
                                {
                                    totalCharges = Decimal.Round(totalCharges * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
                                }

                                decimal vat = Decimal.Round(totalCharges * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                                rateName = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(rateName).Replace("<sup>&reg;</sup>", ""));
                                if (!SM.MethodExists(rateName))
                                {
                                    ShipMethod s_method = new ShipMethod();
                                    s_method.Carrier = "USPS";
                                    s_method.ServiceName = rateName;
                                    s_method.ServiceRate = totalCharges;
                                    s_method.VatRate = vat;
                                    if (HasFreeItems)
                                    {s_method.FreeItemsRate = totalCharges;}
                                    SM.AddMethod(s_method);
                                }
                                else
                                {
                                    int IndexOf = SM.GetIndex(rateName);
                                    ShipMethod s_method = SM[IndexOf];

                                    s_method.ServiceRate += totalCharges;
                                    s_method.VatRate += vat;

                                    if (HasFreeItems)
                                    {
                                        s_method.FreeItemsRate += totalCharges;
                                    }

                                    SM[IndexOf] = s_method;
                                }
                            }
                        }
                    }

                    // Clean up
                    USPSResponse = null;
                }

                //cleanup
                HasFreeItems = false;
                PackageQuantity = 1;
            }

            // Handling fee should only be added per shipping address not per package
            // let's just compute it here after we've gone through all the packages.
            // Also, since we can't be sure about the ordering of the method call here
            // and that the collection SM includes shipping methods from all possible carriers
            // we'll need to filter out the methods per this carrier to avoid side effects on the main collection
            foreach (ShipMethod shipMethod in SM.PerCarrier("USPS"))
            {
                shipMethod.ServiceRate += ExtraFee;
            }
        }

        private void USPSGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)	// Retrieves rates for USPS
        {
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            

            // check all required info
            if (USPSLogin == string.Empty || USPSUsername == string.Empty)
            {
                SM.ErrorMsg = "Error: You must provide USPS login information";
                return;
            }

            // Check server setting
            if (USPSServer == string.Empty)
            {
                SM.ErrorMsg = "Error: You must provide the USPS server";
                return;
            }

            // Check for test mode
            if (TestMode)
            {
                SM.ErrorMsg = "Error: Test Mode not supported for USPS";
                return;
            }

            // Check for shipmentWeight
            if (ShipmentWeight == 0.0M)
            {
                SM.ErrorMsg = "Error: Shipment Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".";
                return;
            }

            Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.USPS.MaxWeight");
            if (maxWeight == 0) maxWeight = 70;

            if (ShipmentWeight > maxWeight)
            {
                SM.ErrorMsg = "USPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                return;
            }
            foreach (Packages Shipment in AllShipments)
            {
                // Create the Xml request (Domestinc)
                // 0 = Usename
                // 1 = Password
                // 2 = Service name
                // 3 = origin zip
                // 4 = dest zip
                // 5 = pounds
                // 6 = ounces (always 0)
                // 7 = Machinable? Always false
                StringBuilder USPSRequest = new StringBuilder(1024);
                USPSRequest.Append("API=RateV4&Xml=");

                String[] USPSServices = AppLogic.AppConfig("RTShipping.USPS.Services").Split(',');

                HasFreeItems = false;
                PackageQuantity = 1;

                StringBuilder uspsReqLoop = new StringBuilder(1024);
                uspsReqLoop.Append("<RateV4Request USERID=\"{0}\">");
                uspsReqLoop.Append("<Revision />");
                foreach (Package p in Shipment)
                {
                    USPSWeight w = USPSGetWeight(p.Weight);

                    //can do this because any Shipment that has free items will only have 1 Package p
                    if (p.IsFreeShipping)
                    {
                        HasFreeItems = true;
                    }

                    //can do this because any Shipment that has IsShipSeparately items will only have 1 Package p
                    //sanity check to make sure we don't call the server 100 times for each carrier if 100 of an
                    //IsShipSeparately item is ordered.  ShoppingCart.cs will add 1 Package p with a quantity
                    //if the item IsShipSeparately
                    if (p.IsShipSeparately)
                    {
                        PackageQuantity = p.Quantity;
                    }

                    for (int srvcs = 0; srvcs < USPSServices.Length; srvcs++)
                    {

                        uspsReqLoop.Append("<Package ID=\"");
                        uspsReqLoop.Append(p.PackageId.ToString());
                        uspsReqLoop.Append("-");
                        uspsReqLoop.Append(srvcs.ToString());
                        uspsReqLoop.Append("\">");
                        uspsReqLoop.Append("<Service>");
                        uspsReqLoop.Append(USPSServices[srvcs].ToString());
                        uspsReqLoop.Append("</Service>");
                        uspsReqLoop.Append("<ZipOrigination>");
                        uspsReqLoop.Append(OriginZipPostalCode);
                        uspsReqLoop.Append("</ZipOrigination>");
                        uspsReqLoop.Append("<ZipDestination>");
                        if (Shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase) && Shipment.DestinationZipPostalCode.Length > 5)
                        {
                            uspsReqLoop.Append(Shipment.DestinationZipPostalCode.Substring(0, 5));
                        }
                        else
                        {
                            uspsReqLoop.Append(Shipment.DestinationZipPostalCode);
                        }
                        uspsReqLoop.Append("</ZipDestination>");
                        uspsReqLoop.Append("<Pounds>");
                        uspsReqLoop.Append(w.pounds.ToString());
                        uspsReqLoop.Append("</Pounds>");
                        uspsReqLoop.Append("<Ounces>");
                        uspsReqLoop.Append(w.ounces.ToString());
                        uspsReqLoop.Append("</Ounces>");
                        uspsReqLoop.Append("<Container/>");
                        uspsReqLoop.Append(USPSGetSize(p.Length, p.Width, p.Height));
                        uspsReqLoop.Append("<Machinable>False</Machinable>");
                        uspsReqLoop.Append("</Package>");
                    }
                }
                USPSRequest.Append(uspsReqLoop);
                USPSRequest.Append("</RateV4Request>");

                // Replace login info
                String USPSRequest2 = String.Format(USPSRequest.ToString(), USPSUsername, USPSPassword);
                RTShipRequest += USPSRequest2;

                // Send request & capture response
                string result = GETandReceiveData(USPSRequest2, USPSServer);
                RTShipResponse += result;

                // Load Xml into a XmlDocument object
                XmlDocument USPSResponse = new XmlDocument();
                try
                {
                    USPSResponse.LoadXml(result);
                }
                catch
                {
                    SM.ErrorMsg = "Error: USPS Gateway Did Not Respond";
                    return;
                }

                string tempService = string.Empty;
                string ExpressName = string.Empty, PriorityName = string.Empty, ParcelName = string.Empty, FirstClassName = string.Empty, BPMName = string.Empty, LibraryName = string.Empty, MediaName = string.Empty;
                Decimal tempRate = 0.0M;

                XmlNodeList USPSPackage = USPSResponse.GetElementsByTagName("Postage");

                for (int i = 0; i < USPSPackage.Count; i++)
                {
                    XmlNode USPSPostage = USPSPackage.Item(i);
                    tempService = USPSPostage["MailService"].InnerText;
                    if (ShippingMethodIsAllowed("U.S. Postal " + tempService, "USPS"))
                    {
                        tempRate = Localization.ParseUSDecimal(USPSPostage["Rate"].InnerText);

                        //multiply the returned rate by the quantity in the package to avoid calling
                        //more than necessary if there were multiple IsShipSeparately items
                        //ordered.  If there weren't, PackageQuantity is 1 and the rate is normal
                        tempRate = tempRate * PackageQuantity;

                        if (MarkupPercent != System.Decimal.Zero)
                        {
                            tempRate = Decimal.Round(tempRate * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
                        }
                        
                        //strip out html encoded characters sent back from USPS
                        tempService = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(tempService));

                        decimal vat = Decimal.Round(tempRate * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);

                        if (!SM.MethodExists(tempService))
                        {
                            ShipMethod s_method = new ShipMethod();

                            s_method.Carrier = "USPS";
                            s_method.ServiceName = tempService;
                            s_method.ServiceRate = tempRate;
                            s_method.VatRate = vat;

                            if (HasFreeItems)
                            {
                                s_method.FreeItemsRate = tempRate;
                            }

                            SM.AddMethod(s_method);
                        }
                        else
                        {
                            int IndexOf = SM.GetIndex(tempService);
                            ShipMethod s_method = SM[IndexOf];

                            s_method.ServiceRate += tempRate;
                            s_method.VatRate += vat;

                            if (HasFreeItems)
                            {
                                s_method.FreeItemsRate += tempRate;
                            }

                            SM[IndexOf] = s_method;
                        }
                    }
                    USPSPostage = null;
                }

                USPSPackage = null;
                HasFreeItems = false;
                PackageQuantity = 1;
            }

            // Handling fee should only be added per shipping address not per package
            // let's just compute it here after we've gone through all the packages.
            // Also, since we can't be sure about the ordering of the method call here
            // and that the collection SM includes shipping methods from all possible carriers
            // we'll need to filter out the methods per this carrier to avoid side effects on the main collection
            foreach (ShipMethod shipMethod in SM.PerCarrier("USPS"))
            {
                shipMethod.ServiceRate += ExtraFee;
            }
        }

        private void FedExIntlGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)	// Retrieves FedEx rates
        {

            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            Encoding utf8 = new UTF8Encoding(false);
            string[] FedExCarrierCodes = { "" }; 
            Hashtable htRates = new Hashtable();

            Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.Fedex.MaxWeight");
            if (maxWeight == 0)
            {
                maxWeight = 150;
            }

            if (ShipmentWeight > maxWeight)
            {
                SM.ErrorMsg = "FedEx " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                return;
            }

            foreach (Packages Shipment in AllShipments)
            {

                HasFreeItems = false;
                PackageQuantity = 1;

                foreach (string FedExCarrierCode in FedExCarrierCodes)
                {
                    foreach (Package p in Shipment)
                    {
                        //can do because any Shipment with free shipping items will only have one Package p
                        if (p.IsFreeShipping)
                        {
                            HasFreeItems = true;
                        }

                        //can do this because any Shipment that has IsShipSeparately items will only have 1 Package p
                        //sanity check to make sure we don't call the server 100 times for each carrier if 100 of an
                        //IsShipSeparately item is ordered.  ShoppingCart.cs will add 1 Package p with a quantity
                        //if the item IsShipSeparately
                        if (p.IsShipSeparately)
                        {
                            PackageQuantity = p.Quantity;
                        }

                        StringBuilder FedExRequest = new StringBuilder(4096);
                        FedExRequest.Append("<?xml version='1.0' encoding='UTF-8' ?>");
                        FedExRequest.Append("<FDXRateAvailableServicesRequest xmlns:api='http://www.fedex.com/fsmapi' xmlns:xsi='http://www.w3.org/2001/XmlSchema-instance' xsi:noNamespaceSchemaLocation='FDXRateAvailableServicesRequest.xsd'>");
                        FedExRequest.Append("<RequestHeader>");
                        FedExRequest.Append("<CustomerTransactionIdentifier>RatesRequest</CustomerTransactionIdentifier>");
                        FedExRequest.Append("<AccountNumber>" + this.FedexAccountNumber + "</AccountNumber>");
                        FedExRequest.Append("<MeterNumber>" + this.FedexMeter + "</MeterNumber>");
                        FedExRequest.Append("<CarrierCode>" + FedExCarrierCode.ToString() + "</CarrierCode>");
                        FedExRequest.Append("</RequestHeader>");
                        System.DateTime TomorrowsDate = System.DateTime.Now.AddDays(1);
                        FedExRequest.Append("<ShipDate>" + TomorrowsDate.Year.ToString() + "-" + TomorrowsDate.Month.ToString().PadLeft(2, '0') + "-" + TomorrowsDate.Day.ToString().PadLeft(2, '0') + "</ShipDate>");
                        FedExRequest.Append("<DropoffType>REGULARPICKUP</DropoffType>");
                        FedExRequest.Append("<Packaging>YOURPACKAGING</Packaging>");
                        FedExRequest.Append("<WeightUnits>" + AppLogic.AppConfig("RTShipping.WeightUnits").Trim().ToUpperInvariant() + "</WeightUnits>");
                        FedExRequest.Append("<ListRate>false</ListRate>");
                        FedExRequest.Append("<Weight>" + Localization.DecimalStringForDB(p.Weight) + "</Weight>");
                        FedExRequest.Append("<OriginAddress>");
                        FedExRequest.Append("<StateOrProvinceCode>" + m_OriginStateProvince + "</StateOrProvinceCode>");
                        FedExRequest.Append("<PostalCode>");
                        FedExRequest.Append(m_OriginZipPostalCode);
                        FedExRequest.Append("</PostalCode>");
                        FedExRequest.Append("<CountryCode>" + m_OriginCountry + "</CountryCode>");
                        FedExRequest.Append("</OriginAddress>");
                        FedExRequest.Append("<DestinationAddress>");
                        FedExRequest.Append("<StateOrProvinceCode>" + Shipment.DestinationStateProvince + "</StateOrProvinceCode>");
                        String DCountry = Shipment.DestinationCountryCode;
                        if (DCountry.Length == 0)
                        {
                            DCountry = this.OriginCountry;
                        }
                        FedExRequest.Append("<PostalCode>");
                        if (DCountry.Equals("US", StringComparison.InvariantCultureIgnoreCase))
                        {
                            FedExRequest.Append(Shipment.DestinationZipPostalCode.Substring(0, 5));
                        }
                        else
                        {
                            FedExRequest.Append(Shipment.DestinationZipPostalCode);
                        }
                        FedExRequest.Append("</PostalCode>");
                        FedExRequest.Append("<CountryCode>" + DCountry + "</CountryCode>");
                        FedExRequest.Append("</DestinationAddress>");
                        FedExRequest.Append("<Payment>");
                        FedExRequest.Append("<PayorType>SENDER</PayorType>");
                        FedExRequest.Append("</Payment>");
                        if (p.Length + p.Width + p.Height != 0)
                        {
                            FedExRequest.Append("<Dimensions>");
                            FedExRequest.Append("<Length>");
                            FedExRequest.Append(p.Length.ToString("###"));
                            FedExRequest.Append("</Length>");
                            FedExRequest.Append("<Width>");
                            FedExRequest.Append(p.Width.ToString("###"));
                            FedExRequest.Append("</Width>");
                            FedExRequest.Append("<Height>");
                            FedExRequest.Append(p.Height.ToString("###"));
                            FedExRequest.Append("</Height>");
                            FedExRequest.Append("<Units>");

                            if (AppLogic.AppConfig("RTShipping.WeightUnits").Trim().Equals("LBS", StringComparison.InvariantCultureIgnoreCase))
                                FedExRequest.Append("IN");
                            else
                                FedExRequest.Append("CM");

                            FedExRequest.Append("</Units>");
                            FedExRequest.Append("</Dimensions>");
                        }

                        if (p.Insured && (p.InsuredValue != 0))
                        {
                            FedExRequest.Append("<DeclaredValue>");
                            FedExRequest.Append("<Value>" + Localization.CurrencyStringForDBWithoutExchangeRate(p.InsuredValue) + "</Value>");
                            FedExRequest.Append("<CurrencyCode>" + Localization.StoreCurrency() + "</CurrencyCode>");
                            FedExRequest.Append("</DeclaredValue>");
                        }

                        FedExRequest.Append("<PackageCount>1</PackageCount>");
                        FedExRequest.Append("</FDXRateAvailableServicesRequest>");

                        // Send Fedex Request
                        RTShipRequest = FedExRequest.ToString();
                        string result = POSTandReceiveData(RTShipRequest, this.FedexServer);
                        RTShipResponse = result;
                        FedExRequest = null;

                        // Load Xml into a XmlDocument object
                        XmlDocument FedExResponse = new XmlDocument();
                        try
                        {
                            FedExResponse.LoadXml(result);
                        }
                        catch
                        {
                            SM.ErrorMsg = "Error: FedEx Gateway Did Not Respond";
                            return;
                        }

                        // Parse the response

                        // Check for errors
                        XmlNodeList FedExErrors = FedExResponse.SelectNodes("/FDXRateAvailableServicesReply/Error");

                        if (FedExErrors.Count > 0)
                        {
                            XmlNode errorCode = FedExResponse.SelectSingleNode("/FDXRateAvailableServicesReply/Error/Code");
                            XmlNode errorMessage = FedExResponse.SelectSingleNode("/FDXRateAvailableServicesReply/Error/Message");

                            switch (errorCode.InnerText)
                            {
                                case "58660":
                                    {
                                        SM.ErrorMsg = AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                                        break;
                                    }
                                default:
                                    {
                                        SM.ErrorMsg = "FedEx Error: " + errorMessage.InnerText;
                                        break;
                                    }
                            }
                            errorCode = null;
                            errorMessage = null;

                            return;
                        }

                        FedExErrors = null;

                        // Get rates
                        XmlNodeList nodesEntries = FedExResponse.SelectNodes("/FDXRateAvailableServicesReply/Entry");

                        // Loop through & get rates for individual packages
                        foreach (XmlNode nodeEntry in nodesEntries)
                        {

                            string rateName = "FedEx " + FedExGetCodeDescription(nodeEntry.SelectSingleNode("Service").InnerText);
                            if (ShippingMethodIsAllowed(rateName, "FEDEX"))
                            {
                                String Sx = nodeEntry.SelectSingleNode("EstimatedCharges/DiscountedCharges/NetCharge").InnerText;
                                decimal totalCharges = Localization.ParseUSDecimal(Sx);

                                //multiply the returned rate by the quantity in the package to avoid calling
                                //more than necessary if there were multiple IsShipSeparately items
                                //ordered.  If there weren't, PackageQuantity is 1 and the rate is normal
                                totalCharges = totalCharges * PackageQuantity;

                                if (MarkupPercent != System.Decimal.Zero)
                                {
                                    totalCharges = Decimal.Round(totalCharges * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
                                }
                                
                                decimal vat = Decimal.Round(totalCharges * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);

                                if (!SM.MethodExists(rateName))
                                {
                                    ShipMethod s_method = new ShipMethod();

                                    s_method.Carrier = "FEDEX";
                                    s_method.ServiceName = rateName;
                                    s_method.ServiceRate = totalCharges;
                                    s_method.VatRate = vat;

                                    if (HasFreeItems)
                                    {
                                        s_method.FreeItemsRate = totalCharges;
                                    }

                                    SM.AddMethod(s_method);
                                }
                                else
                                {
                                    int IndexOf = SM.GetIndex(rateName);
                                    ShipMethod s_method = SM[IndexOf];

                                    s_method.ServiceRate += totalCharges;
                                    s_method.VatRate += vat;

                                    if (HasFreeItems)
                                    {
                                        s_method.FreeItemsRate += totalCharges;
                                    }

                                    SM[IndexOf] = s_method;
                                }
                            }

                        }
                        // Clean up
                        FedExResponse = null;
                    }
                }

                //cleanup
                HasFreeItems = false;
                PackageQuantity = 1;
            }

            // Handling fee should only be added per shipping address not per package
            // let's just compute it here after we've gone through all the packages.
            // Also, since we can't be sure about the ordering of the method call here
            // and that the collection SM includes shipping methods from all possible carriers
            // we'll need to filter out the methods per this carrier to avoid side effects on the main collection
            foreach (ShipMethod shipMethod in SM.PerCarrier("FEDEX"))
            {
                shipMethod.ServiceRate += ExtraFee;
            }
        }

        private void USPSIntlGetRates(Packages Shipment, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate) // Retrieves International rates for USPS
        {

            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;
            Hashtable htRates = new Hashtable();

            // check all required info
            if (Shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase))
            {
                ratesText.Add("Error: Calling USPS International but shipping to US country");
                ratesValues.Add("Error: Calling USPS International but shipping to US country");
                return; // error
            }

            if (m_uspsLogin == string.Empty || m_uspsUsername == string.Empty)
            {
                ratesText.Add("Error: You must provide USPS login information");
                ratesValues.Add("Error: You must provide USPS login information");
                return;
            }

            // Check server setting
            if (m_uspsServer == string.Empty)
            {
                ratesText.Add("Error: You must provide the USPS server");
                ratesValues.Add("Error: You must provide the USPS server");
                return;
            }

            // Check for test mode
            if (m_TestMode)
            {
                m_uspsServer = AppLogic.AppConfig("RTShipping.USPS.TestServer");
            }

            // Check for m_ShipmentWeight
            if (ShipmentWeight == 0.0M)
            {
                ratesText.Add("Error: Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".");
                ratesValues.Add("Error: Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".");
                return;
            }

            Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.USPS.MaxWeight");
            if (maxWeight == 0)
            {
                maxWeight = 70;
            }

            if (ShipmentWeight > maxWeight)
            {
                ratesText.Add("USPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt"));
                ratesValues.Add("USPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt"));
                return;
            }

            // Create the Xml request (International)
            StringBuilder USPSRequest = new StringBuilder(1024);
            USPSRequest.Append("API=IntlRateV2&Xml=");

            StringBuilder uspsReqLoop = new StringBuilder(1024);
            uspsReqLoop.Append("<IntlRateV2Request USERID=\"{0}\">");
            foreach (Package p in Shipment)
            {
                USPSWeight w = USPSGetWeight(p.Weight);
                uspsReqLoop.Append("<Package ID=\"" + p.PackageId + "\">");
                uspsReqLoop.Append("<Pounds>" + w.pounds + "</Pounds>");
                uspsReqLoop.Append("<Ounces>" + w.ounces + "</Ounces>");
                uspsReqLoop.Append("<Machinable>True</Machinable>");
                uspsReqLoop.Append("<MailType>Package</MailType>");
                uspsReqLoop.Append("<ValueOfContents>" + p.InsuredValue + "</ValueOfContents>");

                if (Shipment.DestinationCountryCode.Equals("GB", StringComparison.InvariantCultureIgnoreCase))
                {
                    uspsReqLoop.Append("<Country>United Kingdom (Great Britain)</Country>");
                }
                else
                {
                    uspsReqLoop.Append("<Country>");
                    uspsReqLoop.Append(AppLogic.GetCountryName(Shipment.DestinationCountryCode));
                    uspsReqLoop.Append("</Country>");
                }

                uspsReqLoop.Append("<Container>RECTANGULAR</Container>");
                uspsReqLoop.Append(USPSGetSize(p.Length, p.Width, p.Height));

                uspsReqLoop.Append("</Package>");
            }
            USPSRequest.Append(uspsReqLoop);
            USPSRequest.Append("</IntlRateV2Request>");

            // Replace login info
            String USPSRequest2 = string.Format(USPSRequest.ToString(), USPSUsername, USPSPassword);
            RTShipRequest = USPSRequest2;

            // Send request & capture response
            string result = GETandReceiveData(USPSRequest2, USPSServer);
            RTShipResponse = result;

            // Load Xml into a XmlDocument object
            XmlDocument USPSResponse = new XmlDocument();
            try
            {
                USPSResponse.LoadXml(result);
            }
            catch
            {
                ratesText.Add("Error: USPS Gateway Did Not Respond");
                ratesValues.Add("Error: USPS Gateway Did Not Respond");
                return;
            }

            // Check for error
            XmlNodeList USPSErrors = USPSResponse.GetElementsByTagName("Error");
            if (USPSErrors.Count > 0) // Error has occurred
            {
                XmlNodeList USPSError = USPSResponse.GetElementsByTagName("Error");
                XmlNode USPSErrorMessage = USPSError.Item(0);
                ratesText.Add("USPS Error: " + USPSErrorMessage["Description"].InnerText);
                ratesValues.Add("USPS Error: " + USPSErrorMessage["Description"].InnerText);
                USPSError = null;
                return;
            }
            else
            {
                XmlNodeList nodesPackages = USPSResponse.GetElementsByTagName("Package");
                foreach (XmlNode nodePackage in nodesPackages)
                {
                    XmlNodeList nodesServices = nodePackage.SelectNodes("Service");
                    foreach (XmlNode nodeService in nodesServices)
                    {
                        string rateName = nodeService.SelectSingleNode("SvcDescription").InnerText;
                        if (ShippingMethodIsAllowed("U.S. Postal " + rateName, "USPS") && rateName.IndexOf("Envelope") == -1 && rateName.IndexOf(" Document") == -1 && rateName.IndexOf("Letter") == -1)
                        {
                            decimal totalCharges = Localization.ParseUSDecimal(nodeService.SelectSingleNode("Postage").InnerText);

                            if (MarkupPercent != System.Decimal.Zero)
                            {
                                totalCharges = Decimal.Round(totalCharges * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
                            }
                            totalCharges += ExtraFee;
                            rateName = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(rateName).Replace("<sup>&reg;</sup>", ""));
                            if (htRates.ContainsKey(rateName))
                            {
                                // Get the sum of the rate(s)
                                decimal myTempCharge = Localization.ParseUSDecimal(htRates[rateName].ToString());
                                totalCharges += myTempCharge;
                                // Remove the old value & add the new
                                htRates.Remove(rateName);
                            }
                            decimal vat = Decimal.Round(totalCharges * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                            htRates.Add(rateName, Localization.CurrencyStringForDBWithoutExchangeRate(totalCharges) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(vat));
                        }
                    }
                }

                // Clean up
                USPSResponse = null;
            }

            // Add rates from hastable into array(s)
            IDictionaryEnumerator myEnumerator = htRates.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                ratesText.Add(myEnumerator.Key.ToString() + " $" + myEnumerator.Value.ToString());
                ratesValues.Add(myEnumerator.Key.ToString() + "|" + myEnumerator.Value.ToString());
            }
        }

        private void USPSGetRates(Packages Shipment, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)	// Retrieves rates for USPS
        {
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;
            // check all required info
            if (USPSLogin == string.Empty || USPSUsername == string.Empty)
            {
                ratesText.Add("Error: You must provide USPS login information");
                ratesValues.Add("Error: You must provide USPS login information");
                return;
            }

            // Check server setting
            if (USPSServer == string.Empty)
            {
                ratesText.Add("Error: You must provide the USPS server");
                ratesValues.Add("Error: You must provide the USPS server");
                return;
            }

            // Check for test mode
            if (TestMode)
            {
                ratesText.Add("Error: Test Mode not supported for USPS");
                ratesValues.Add("Error: Test Mode not supported for USPS");
                return;
                //USPSServer = AppLogic.AppConfig("RTShipping.USPS.TestServer");
            }

            // Check for shipmentWeight
            if (ShipmentWeight == 0.0M)
            {
                ratesText.Add("Error: Shipment Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".");
                ratesValues.Add("Error: Shipment Shipment Weight must be greater than 0 " + Localization.WeightUnits() + ".");
                return;
            }

            Decimal maxWeight = AppLogic.AppConfigUSDecimal("RTShipping.USPS.MaxWeight");
            if (maxWeight == 0) maxWeight = 70;

            if (ShipmentWeight > maxWeight)
            {
                ratesText.Add("USPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt"));
                ratesValues.Add("USPS " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt"));
                return;
            }

            // Create the Xml request (Domestinc)
            // 0 = Usename
            // 1 = Password
            // 2 = Service name
            // 3 = origin zip
            // 4 = dest zip
            // 5 = pounds
            // 6 = ounces (always 0)
            // 7 = Machinable? Always false
            StringBuilder USPSRequest = new StringBuilder(1024);
            USPSRequest.Append("API=RateV4&Xml=");

            String[] USPSServices = AppLogic.AppConfig("RTShipping.USPS.Services").Split(',');

            /*
            ArrayList USPSServices = new ArrayList();
            USPSServices.Add("Express");
            //USPSServices.Add("First Class");
            USPSServices.Add("Priority");
            USPSServices.Add("Parcel");
            //USPSServices.Add("BPM");
            USPSServices.Add("Library");
            USPSServices.Add("Media");
            */
            //USPSWeight w = USPSGetWeight(ShipmentWeight);

            StringBuilder uspsReqLoop = new StringBuilder(1024);
            uspsReqLoop.Append("<RateV4Request USERID=\"{0}\">");
            uspsReqLoop.Append("<Revision />");
            foreach (Package p in Shipment)
            {
                USPSWeight w = USPSGetWeight(p.Weight);
                for (int srvcs = 0; srvcs < USPSServices.Length; srvcs++)
                {
                    uspsReqLoop.Append("<Package ID=\"");
                    uspsReqLoop.Append(p.PackageId.ToString());
                    uspsReqLoop.Append("-");
                    uspsReqLoop.Append(srvcs.ToString());
                    uspsReqLoop.Append("\">");
                    uspsReqLoop.Append("<Service>");
                    uspsReqLoop.Append(USPSServices[srvcs].ToString());
                    uspsReqLoop.Append("</Service>");
                    uspsReqLoop.Append("<ZipOrigination>");
                    uspsReqLoop.Append(OriginZipPostalCode);
                    uspsReqLoop.Append("</ZipOrigination>");
                    uspsReqLoop.Append("<ZipDestination>");
                    if (Shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase) && Shipment.DestinationZipPostalCode.Length > 5)
                    {
                        uspsReqLoop.Append(Shipment.DestinationZipPostalCode.Substring(0, 5));
                    }
                    else
                    {
                        uspsReqLoop.Append(Shipment.DestinationZipPostalCode);
                    }
                    uspsReqLoop.Append("</ZipDestination>");
                    uspsReqLoop.Append("<Pounds>");
                    uspsReqLoop.Append(w.pounds.ToString());
                    uspsReqLoop.Append("</Pounds>");
                    uspsReqLoop.Append("<Ounces>");
                    uspsReqLoop.Append(w.ounces.ToString());
                    uspsReqLoop.Append("</Ounces>");
                    uspsReqLoop.Append("<Container/>");
                    uspsReqLoop.Append(USPSGetSize(p.Length, p.Width, p.Height));
                    uspsReqLoop.Append("<Machinable>False</Machinable>");
                    uspsReqLoop.Append("</Package>");
                }
            }
            USPSRequest.Append(uspsReqLoop);
            USPSRequest.Append("</RateV4Request>");

            // Replace login info
            String USPSRequest2 = String.Format(USPSRequest.ToString(), USPSUsername, USPSPassword);
            RTShipRequest = USPSRequest2;

            // Send request & capture response
            string result = GETandReceiveData(USPSRequest2, USPSServer);
            RTShipResponse = result;

            // Load Xml into a XmlDocument object
            XmlDocument USPSResponse = new XmlDocument();
            try
            {
                USPSResponse.LoadXml(result);
            }
            catch
            {
                ratesText.Add("Error: USPS Gateway Did Not Respond");
                ratesValues.Add("Error: USPS Gateway Did Not Respond");
                return;
            }

            string tempService = string.Empty;
            string ExpressName = string.Empty, PriorityName = string.Empty, ParcelName = string.Empty, FirstClassName = string.Empty, BPMName = string.Empty, LibraryName = string.Empty, MediaName = string.Empty;
            Decimal tempRate = 0.0M;
            Decimal FirstClassRate = 0.0M;
            Decimal BPMRate = 0.0M;
            Decimal LibraryRate = 0.0M;
            Decimal MediaRate = 0.0M;
            Decimal ExpressRate = 0.0M;
            Decimal PriorityRate = 0.0M;
            Decimal ParcelRate = 0.0M;

            Decimal FirstClassRateVat = 0.0M;
            Decimal BPMRateVat = 0.0M;
            Decimal LibraryRateVat = 0.0M;
            Decimal MediaRateVat = 0.0M;
            Decimal ExpressRateVat = 0.0M;
            Decimal PriorityRateVat = 0.0M;
            Decimal ParcelRateVat = 0.0M;

            XmlNodeList USPSPackage = USPSResponse.GetElementsByTagName("Postage");

            for (int i = 0; i < USPSPackage.Count; i++)
            {
                XmlNode USPSPostage = USPSPackage.Item(i);
                tempService = USPSPostage["MailService"].InnerText;
                if (ShippingMethodIsAllowed("U.S. Postal " + tempService, "USPS"))
                {
                    tempRate = Localization.ParseUSDecimal(USPSPostage["Rate"].InnerText);

                    if (MarkupPercent != System.Decimal.Zero)
                    {
                        tempRate = Decimal.Round(tempRate * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
                    }
                    tempRate += ExtraFee;
                    decimal vat = Decimal.Round(tempRate * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);

                    if (tempService.IndexOf("Express") != -1)
                    {
                        ExpressName = tempService;
                        ExpressRate += tempRate;
                        ExpressRateVat += vat;
                    }
                    else if (tempService.IndexOf("Priority") != -1)
                    {
                        PriorityName = tempService;
                        PriorityRate += tempRate;
                        PriorityRateVat += vat;
                    }
                    else if (tempService.IndexOf("Parcel") != -1)
                    {
                        ParcelName = tempService;
                        ParcelRate += tempRate;
                        ParcelRateVat += vat;
                    }
                    else if (tempService.IndexOf("Library") != -1)
                    {
                        LibraryName = tempService;
                        LibraryRate += tempRate;
                        LibraryRateVat += vat;
                    }
                    else if (tempService.IndexOf("Media") != -1)
                    {
                        MediaName = tempService;
                        MediaRate += tempRate;
                        MediaRateVat += vat;
                    }

                    /*
                    switch (tempService)
                    {
                        case "Express":
                            ExpressName = tempService;
                            ExpressRate += tempRate;
                            break;
                        case "Priority":
                            PriorityName = tempService;
                            PriorityRate += tempRate;
                            break;
                        case "Parcel":
                            ParcelName = tempService;
                            ParcelRate += tempRate;
                            break;
                        case "First Class":
                            FirstClassName = tempService;
                            FirstClassRate += tempRate;
                            break;
                        case "BPM":
                            BPMName = tempService;
                            BPMRate += tempRate;
                            break;
                        case "Library":
                            LibraryName = tempService;
                            LibraryRate += tempRate;
                            break;
                        case "Media":
                            MediaName = tempService;
                            MediaRate += tempRate;
                            break;
                    }
                    */
                }
                USPSPostage = null;
            }

            if (ExpressRate != 0.0M)
            {
                ratesText.Add("U.S. Postal " + ExpressName + " " + Localization.CurrencyStringForDBWithoutExchangeRate(ExpressRate));
                ratesValues.Add("U.S. Postal " + ExpressName + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(ExpressRate) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(ExpressRateVat));
            }

            if (PriorityRate != 0.0M)
            {
                ratesText.Add("U.S. Postal " + PriorityName + " " + Localization.CurrencyStringForDBWithoutExchangeRate(PriorityRate));
                ratesValues.Add("U.S. Postal " + PriorityName + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(PriorityRate) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(PriorityRateVat));
            }

            if (ParcelRate != 0.0M)
            {
                ratesText.Add("U.S. Postal " + ParcelName + " " + Localization.CurrencyStringForDBWithoutExchangeRate(ParcelRate));
                ratesValues.Add("U.S. Postal " + ParcelName + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(ParcelRate) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(ParcelRateVat));
            }

            if (FirstClassRate != 0.0M)
            {
                ratesText.Add("U.S. Postal " + FirstClassName + " " + Localization.CurrencyStringForDBWithoutExchangeRate(FirstClassRate));
                ratesValues.Add("U.S. Postal " + FirstClassName + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(FirstClassRate) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(FirstClassRateVat));
            }

            if (BPMRate != 0.0M)
            {
                ratesText.Add("U.S. Postal " + BPMName + " " + Localization.CurrencyStringForDBWithoutExchangeRate(BPMRate));
                ratesValues.Add("U.S. Postal " + BPMName + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(BPMRate) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(BPMRateVat));
            }

            if (LibraryRate != 0.0M)
            {
                ratesText.Add("U.S. Postal " + LibraryName + " " + Localization.CurrencyStringForDBWithoutExchangeRate(LibraryRate));
                ratesValues.Add("U.S. Postal " + LibraryName + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(LibraryRate) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(LibraryRateVat));
            }

            if (MediaRate != 0.0M)
            {
                ratesText.Add("U.S. Postal " + MediaName + " " + Localization.CurrencyStringForDBWithoutExchangeRate(MediaRate));
                ratesValues.Add("U.S. Postal " + MediaName + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(MediaRate) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(MediaRateVat));
            }

            USPSPackage = null;
        }

        private string USPSGetSize(Decimal length, Decimal width, Decimal height)
        {
            string Output = string.Empty;
            Decimal girth = 0;

            if (length >= width && length >= height)
                girth = (height + width) * 2;
            else if (width >= length && width >= height)
                girth = (length + height) * 2;
            else
                girth = (length + width) * 2;

            if (length > 12 || width > 12 || height > 12)
            {
                Output = "<Size>Large</Size>";
                Output += "<Width>" + width.ToString() + "</Width>";
                Output += "<Length>" + length.ToString() + "</Length>";
                Output += "<Height>" + height.ToString() + "</Height>";
                Output += "<Girth>" + girth.ToString() + "</Girth>";
            }
            else
            {
                
                Output = "<Size>Regular</Size>";
                Output += "<Width>" + width.ToString() + "</Width>";
                Output += "<Length>" + length.ToString() + "</Length>";
                Output += "<Height>" + height.ToString() + "</Height>";
                Output += "<Girth>" + girth.ToString() + "</Girth>";
            }
            return Output;
        }

        



        /// <summary>
        /// Convert the input number to the textual description of the Service Code
        /// </summary>
        /// <param name="code">The Service Code number to be converted</param>
        /// <returns></returns>
        private string UPSServiceCodeDescription(string code)
        {
            string result = string.Empty;
            switch (code)
            {
                case "01":
                    result = "UPS Next Day Air";
                    break;
                case "02":
                    result = "UPS 2nd Day Air";
                    break;
                case "03":
                    result = "UPS Ground";
                    break;
                case "07":
                    result = "UPS Worldwide Express";
                    break;
                case "08":
                    result = "UPS Worldwide Expedited";
                    break;
                case "11":
                    result = "UPS Standard";
                    break;
                case "12":
                    result = "UPS 3-Day Select";
                    break;
                case "13":
                    result = "UPS Next Day Air Saver";
                    break;
                case "14":
                    result = "UPS Next Day Air Early AM";
                    break;
                case "54":
                    result = "UPS Worldwide Express Plus";
                    break;
                case "59":
                    result = "UPS 2nd Day Air AM";
                    break;
                case "65":
                    result = "UPS Express Saver";
                    break;
            }

            return result;
        }


        /// <summary>
        /// Convert the decimal weight passed in to pounds and ounces
        /// </summary>
        /// <param name="weight">The decimal weight to be convert (in pounds only)</param>
        /// <returns></returns>
        USPSWeight USPSGetWeight(Decimal weight)
        {
            Decimal pounds = 0;
            Decimal ounces = 0;

            pounds = Convert.ToInt32(weight - weight % 1);
            decimal tempWeight = (decimal)weight * 16;
            ounces = Convert.ToInt32(Math.Ceiling((Decimal)tempWeight - (Decimal)pounds * 16.0M));

            USPSWeight w = new USPSWeight();
            w.pounds = Localization.ParseUSInt(pounds.ToString());
            w.ounces = Localization.ParseUSInt(ounces.ToString());

            return w;
        }

        public static bool ShippingMethodIsAllowed(String MethodName, String Carrier)
        {
            string SuperName = StripHtmlAndRemoveSpecialCharacters(HttpUtility.HtmlDecode(MethodName).Replace("<sup>&reg;</sup>", "").Replace("U.S. Postal", "").ToUpperInvariant());
            if (MethodName.Length == 0)
            {
                return true; // not sure how this could happen, but...
            }
            MethodName = MethodName.ToUpperInvariant();
            String tmpS = AppLogic.AppConfig("RTShipping.ShippingMethodsToPrevent").ToUpperInvariant();
            if (tmpS.Length == 0)
            {
                // nothing is prevented
                return true;
            }
            // only allow this method if does not match (exactly) any of the prevented methods:
            foreach (String s in tmpS.Split(','))
            {
                if (s.Trim() == MethodName || s.Trim() == (Carrier.ToUpperInvariant() + " " + MethodName).Trim())
                {
                    // restrict on match:
                    return false;
                }

                //Lets add some brains for those USPS Intl methods as well.
                if (Carrier.Contains("USPS"))
                {
                    SuperName = SuperName.Replace("U.S. POSTAL", "").Trim();

                    if (s.Trim() == SuperName || s.Trim() == (Carrier.ToUpperInvariant() + " " + SuperName).Trim() || s.Trim() == ("U.S. POSTAL " + SuperName))
                    {
                        // restrict on match:
                        return false;
                    }
                }
            }
            return true;
        }
        public static string StripHtmlAndRemoveSpecialCharacters(string value)
        {
            string returnValue;

            //RS 1/11
            //strip the html from the string
            returnValue = AppLogic.StripHtml(value);

            //for shipping values, there are a number of special characters we want to remove
            //if these are left in the shipping names, they will not match when compared to RTShipping.ShippingMethodsToPrevent
            returnValue = returnValue.Replace("&reg;", "");
            returnValue = returnValue.Replace("&copy;", "");

            return returnValue;
        }

        /// <summary>
        /// Send and capture data using GET
        /// </summary>
        /// <param name="Request">The Xml Request to be sent</param>
        /// <param name="Server">The server the request should be sent to</param>
        /// <returns>String</returns>
        private string GETandReceiveData(string Request, string Server)
        {
            HttpWebRequest requestX = (HttpWebRequest)WebRequest.Create(Server + "?" + Request);
            HttpWebResponse response = (HttpWebResponse)requestX.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string result = sr.ReadToEnd();
            response.Close();
            sr.Close();
            return result;
        }


        /// <summary>
        /// Send and capture data using Post
        /// </summary>
        /// <param name="Request">The Xml Request to be sent</param>
        /// <param name="Server">The server the request should be sent to</param>
        /// <returns>String</returns>
        private string POSTandReceiveData(string Request, string Server)
        {
            // check for cache hit:
            String CacheName = Server + Request;
            String s = (String)HttpContext.Current.Cache.Get(CacheName);
            if (s != null)
            {
                return s;
            }
            // Set encoding & get content Length
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(Request); // Request

            // Prepare post request
            HttpWebRequest shipRequest = (HttpWebRequest)WebRequest.Create(Server); // Server
            shipRequest.Method = "POST";
            shipRequest.ContentType = "application/x-www-form-urlencoded";
            shipRequest.ContentLength = data.Length;
            Stream requestStream = shipRequest.GetRequestStream();
            // Send the data
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            // get the response
            WebResponse shipResponse = null;
            string response = String.Empty;
            try
            {
                shipResponse = shipRequest.GetResponse();
                using (StreamReader sr = new StreamReader(shipResponse.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception exc)
            {
                response = exc.ToString();
            }
            finally
            {
                if (shipResponse != null) shipResponse.Close();
            }

            shipRequest = null;
            requestStream = null;
            shipResponse = null;

            // cache result. if there was no error in it!
            if (response.ToLowerInvariant().IndexOf("error:") != -1
                 || response.ToLowerInvariant().IndexOf("exception") != -1)
            {
                try
                {
                    HttpContext.Current.Cache.Remove(CacheName);
                }
                catch { }
            }
            else
            {
                HttpContext.Current.Cache.Insert(CacheName, response, null, System.DateTime.Now.AddMinutes(15), TimeSpan.Zero);
            }

            return response;
        }


        public void ClearRates()	// Clears all current rates in memory
        {
            ratesText.Clear();
            ratesValues.Clear();
        }
       
        public enum Shipper	// Enum Shipper: The currently available shipping companies
        {
            Unknown = 0,
            UPS,
            USPS,
            FedEx,
            DHL,
            CanadaPost,
            AusPost
        }

        public enum ResultType	// Enum ResultType: The available return types of the shipment rating(s)
        {
            Unknown = 0,
            PlainText = 1,	// ResultType.PlainText: Specifies the resulting output to be plain text with &lt;BR&gt; tags to separate them
            SingleDropDownList = 2,	// ResultType.SingleDropDownList: Specifies the resulting output to be a single line drop down list
            MultiDropDownList = 3,	// ResultType.MultiDropDownList: Specifies the resulting output to be a multi-line combo-box
            RadioButtonList = 4,	// ResultType.RadioButtonList: Specifies the resulting output to be a list of radio buttons with labels
            RawDelimited = 5,	// ResultType.RawDelimited: Specifes the resulting output to be a delimited string. Rates are delimited with a pipe character (|), rate names &amp; prices are delimited with a comma (,)
            DropDownListControl = 6,	// ResultType.DropDownListControl: Specifes the resulting output to be a System.Web.UI.WebControls.DropDownList control.
            RadioButtonListControl = 7,	// ResultType.RadioButtonListControl: Specifes the resulting output to be a System.Web.UI.WebControls.RadioButtonList control.
            
            CollectionList = 8  // ResultType.CollectionList: Returns the ShippingMethod Collection
           
        }


        public class PickupTypes
        {
            /// <summary>
            /// Specifies the pickup type as: Daily Pickup
            /// </summary>
            public static string UPSDailyPickup
            {
                get { return "01"; }
            }
            /// <summary>
            /// Specifies the pickup type as: Customer Counter
            /// </summary>
            public static string UPSCustomerCounter
            {
                get { return "03"; }
            }
            /// <summary>
            /// Specifies the pickup type as: One time pickup
            /// </summary>
            public static string UPSOneTimePickup
            {
                get { return "06"; }
            }
            /// <summary>
            /// Specifies the pickup type as: On Call Air
            /// </summary>
            public static string UPSOnCallAir
            {
                get { return "07"; }
            }
            /// <summary>
            /// Specifies the pickup type as: Suggested retail rates
            /// </summary>
            public static string UPSSuggestedRetailRates
            {
                get { return "11"; }
            }
            /// <summary>
            /// Specifies the pickup type as: Letter center
            /// </summary>
            public static string UPSLetterCenter
            {
                get { return "19"; }
            }
            /// <summary>
            /// Specifies the pickup type as: Air service center
            /// </summary>
            public static string UPSAirServiceCenter
            {
                get { return "20"; }
            }
        }


        public struct USPSWeight	// Struct USPSWeight: Used to hold shipment weight in pounds and ounces
        {
            public int pounds;	// USPSWeight.pounds: Holds shipment weight in pounds
            public int ounces;	// USPSWeight.pounds: Holds shipment weight in remaining ounces
        }

        /// <summary>
        /// Provides for ability to sort rate list string by rate.
        /// </summary>
        public class ShippingCostsSorter : IComparer
        {
            // Compare cost of shipping.
            public int Compare(object x, object y)
            {
                ShipMethod smX = (ShipMethod)x;
                ShipMethod smY = (ShipMethod)y;
                return smX.ServiceRate.CompareTo(smY.ServiceRate);
            }
        }
    }
}
