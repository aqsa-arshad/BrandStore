// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
    public partial class RTShipping
    {
        // legacy overload for AjaxShipping and Google Checkout
        private void UPS2GetRates(Packages Shipment, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)
        {
            Shipments AllShipments = new Shipments();
            AllShipments.AddPackages(Shipment);

            UPS2GetRates(AllShipments, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);

            foreach (ShipMethod method in SM)
            {
                ratesText.Add(method.ServiceName + " $" + method.ServiceRate.ToString());
                ratesValues.Add(method.ServiceName + "|" + method.ServiceRate.ToString() + "|" + method.VatRate.ToString());
            }
        }

        // v7.x legacy overload
        public void UPS2GetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)
        {
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            ups2.ShippingSection ShippingConfig = new ups2.ShippingSection();
            ShippingConfig.CallForShippingPrompt = AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
            ShippingConfig.Currency = AppLogic.AppConfig("Localization.StoreCurrency");
            ShippingConfig.ExtraFee = AppLogic.AppConfigUSDecimal("ShippingHandlingExtraFee");
            ShippingConfig.FilterOutShippingMethodsThatHave0Cost = AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost");
            ShippingConfig.MarkupPercent = AppLogic.AppConfigUSDecimal("RTShipping.MarkupPercent");
            ShippingConfig.OriginAddress = AppLogic.AppConfig("RTShipping.OriginAddress");
            ShippingConfig.OriginAddress2 = AppLogic.AppConfig("RTShipping.OriginAddress2");
            ShippingConfig.OriginCity = AppLogic.AppConfig("RTShipping.OriginCity");
            ShippingConfig.OriginCountry = AppLogic.AppConfig("RTShipping.OriginCountry");
            ShippingConfig.OriginState = AppLogic.AppConfig("RTShipping.OriginState");
            ShippingConfig.OriginZip = AppLogic.AppConfig("RTShipping.OriginZip");
            ShippingConfig.UseTestRates = AppLogic.AppConfigBool("RTShipping.UseTestRates");
            ShippingConfig.WeightUnits = AppLogic.AppConfig("Localization.WeightUnits");

            ups2.UPSSection UPSConfig = new ups2.UPSSection();
            UPSConfig.AccountNumber = AppLogic.AppConfig("RTShipping.UPS.AccountNumber");
            UPSConfig.AddressTypeBehavior = AppLogic.AppConfig("RTShipping.UPS.AddressTypeBehavior");
            UPSConfig.CustomerClassification = AppLogic.AppConfig("RTShipping.UPS.CustomerClassification");
            if (!AllShipments.IsInternational)
                UPSConfig.DeliveryConfirmation = AppLogic.AppConfig("RTShipping.UPS.DeliveryConfirmation");
            UPSConfig.GetNegotiatedRates = AppLogic.AppConfigBool("RTShipping.UPS.GetNegotiatedRates");
            UPSConfig.License = AppLogic.AppConfig("RTShipping.UPS.License");
            UPSConfig.MaxWeight = AppLogic.AppConfigUSDecimal("RTShipping.UPS.MaxWeight");
            UPSConfig.PackagingType = AppLogic.AppConfig("RTShipping.UPS.PackagingType");
            UPSConfig.Password = AppLogic.AppConfig("RTShipping.UPS.Password");
            UPSConfig.PickupType = AppLogic.AppConfig("RTShipping.UPS.UPSPickupType");
            UPSConfig.Server = AppLogic.AppConfig("RTShipping.UPS.Server");
            UPSConfig.Services = AppLogic.AppConfig("RTShipping.UPS.Services");
            UPSConfig.TestServer = AppLogic.AppConfig("RTShipping.UPS.TestServer");
            UPSConfig.UserName = AppLogic.AppConfig("RTShipping.UPS.Username");

            ShippingMethods UPS2Methods = UPS2Rates.GetRates(AllShipments, ShipmentWeight, ShipmentValue, ShippingTaxRate, 
                out RTShipRequest, out RTShipResponse, ref ShippingConfig, ref UPSConfig);

            foreach (ShipMethod UPS2Method in UPS2Methods)
            {
                SM.AddMethod(UPS2Method);
            }
            SM.ErrorMsg = UPS2Methods.ErrorMsg;
        }
    }

    public partial class UPS2Rates
    {
        static public ShippingMethods GetRates(Shipments AllShipments, decimal ShipmentWeight, decimal ShipmentValue, 
            decimal ShippingTaxRate, out string RTShipRequest, out string RTShipResponse, ref ups2.ShippingSection ShippingConfig, ref ups2.UPSSection UPSConfig)
        {
            // instantiate return variables
            ShippingMethods UPS2ShipMethods = new ShippingMethods();
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            // is weight within shippable limit?
            if (ShipmentWeight > UPSConfig.MaxWeight)
            {
                UPS2ShipMethods.ErrorMsg = "UPS " + ShippingConfig.CallForShippingPrompt;
                return UPS2ShipMethods;
            }

            // check for required configuration variables
            if (UPSConfig.UserName.Length < 3)
                throw new Exception("UPS Error: RTShipping.UPS.UserName must contain the User ID used to signin to ups.com.");

            if (UPSConfig.Password.Length < 3)
                throw new Exception("UPS Error: RTShipping.UPS.Password must contain the Password used to signin to ups.com.");

            if (UPSConfig.License.Length < 7)
                throw new Exception("UPS Error: RTShipping.UPS.License must contain the Access Key assigned by UPS to access UPS OnLine Tools API's.");

            // for negotiated rates, do we have the UPS account number?
            if (UPSConfig.GetNegotiatedRates && UPSConfig.AccountNumber.Length != 6)
                throw new Exception("UPS Error: RTShipping.UPS.GetNegotiatedRates is 'true', but a six-character UPS account number " +
                    "is not specified in RTShipping.UPS.AccountNumber. The account number is required to retrieve negotiated rates.");

            // retrieve correct UPS Server URL
            string UPSServer;
            if (ShippingConfig.UseTestRates)
                UPSServer = UPSConfig.TestServer;
            else
                UPSServer = UPSConfig.Server;


            foreach (Packages Shipment in AllShipments)
            {
                // createUPS OnLine Tools access credentials
                ups2.AccessRequest accessCredentials = new ups2.AccessRequest();
                accessCredentials.AccessLicenseNumber = UPSConfig.License;
                accessCredentials.UserId = UPSConfig.UserName;
                accessCredentials.Password = UPSConfig.Password;

                // create a rate request
                ups2.RatingRequest rateRequest = new ups2.RatingRequest();

                // shipment details
                rateRequest.Shipment.Shipper = new ups2.RequestShipmentShipper();
                rateRequest.Shipment.Shipper.ShipperNumber = UPSConfig.AccountNumber;
                rateRequest.Shipment.RateInformation = new ups2.ShipmentRateInformation();
                rateRequest.Shipment.RateInformation.NegotiatedRatesIndicator = String.Empty;

                // UPS PickupType
                try
                {
                    rateRequest.PickupType.Code = Enum.Format(typeof(ups2.RequestPickupTypeCode),
                        Enum.Parse(typeof(ups2.RequestPickupTypeCode), UPSConfig.PickupType, true), "d").PadLeft(2, '0');
                }
                catch (ArgumentException)
                {
                    throw new Exception("UPS Error: RTShipping.UPS.UPSPickupType == '" + UPSConfig.PickupType + "'. Legal values are UPSDailyPickup, " +
                        "UPSCustomerCounter, UPSOneTimePickup, UPSOnCallAir, UPSSuggestedRetailRates, UPSLetterCenter, or UPSAirServiceCenter.");
                }

                // UPS CustomerClassification
                if (UPSConfig.CustomerClassification != string.Empty)
                    rateRequest.CustomerClassification.Code = UPSConfig.CustomerClassification;  // default is Wholesale, '01'

                // shipment origin
                ups2.UPSAddress Origin = new ups2.UPSAddress();

                // check Shipment for Origin address
                if (Shipment.OriginZipPostalCode != string.Empty)
                {
                    Origin.AddressLine1 = Shipment.OriginAddress1;
                    Origin.AddressLine2 = Shipment.OriginAddress2;
                    Origin.City = Shipment.OriginCity;
                    Origin.StateProvinceCode = Shipment.OriginStateProvince;
                    Origin.PostalCode = Shipment.OriginZipPostalCode;
                    Origin.CountryCode = Shipment.OriginCountryCode;
                }
                else  // shipment didn't have origin address, so use default address from appConfigs.
                {
                    Origin.AddressLine1 = ShippingConfig.OriginAddress;
                    Origin.AddressLine2 = ShippingConfig.OriginAddress2;
                    Origin.City = ShippingConfig.OriginCity;
                    Origin.StateProvinceCode = ShippingConfig.OriginState;
                    Origin.PostalCode = ShippingConfig.OriginZip;
                    Origin.CountryCode = ShippingConfig.OriginCountry;
                }

                // for UK, verify the Origin Country is set to GB
                if (Origin.CountryCode == "UK")
                    Origin.CountryCode = "GB";

                rateRequest.Shipment.Shipper.Address = Origin;

                // ship-to address
                ups2.UPSAddress Dest = new ups2.UPSAddress();

                Dest.AddressLine1 = Shipment.DestinationAddress1;
                Dest.AddressLine2 = Shipment.DestinationAddress2;
                Dest.City = Shipment.DestinationCity;
                Dest.StateProvinceCode = Shipment.DestinationStateProvince;
                Dest.PostalCode = Shipment.DestinationZipPostalCode;
                Dest.CountryCode = Shipment.DestinationCountryCode;

                // residential address indicator
                if ((UPSConfig.AddressTypeBehavior.ToLower() == "forceallresidential" ||
                        Shipment.DestinationResidenceType == ResidenceTypes.Residential ||
                        Shipment.DestinationResidenceType == ResidenceTypes.Unknown && UPSConfig.AddressTypeBehavior.ToLower() != "unknownsarecommercial") &&
                        UPSConfig.AddressTypeBehavior.ToLower() != "forceallcommercial")
                    Dest.ResidentialAddressIndicator = "";  // the mere presence of this tag indicates Residential

                // for UK, verify the Destination Country is set to GB
                if (Dest.CountryCode == "UK")
                    Dest.CountryCode = "GB";

                rateRequest.Shipment.ShipTo.Address = Dest;  // add destination address to request

                // create one package per package in shipment
                rateRequest.Shipment.Package = new ups2.ShipmentPackage[Shipment.Count];

                int pIndex = 0;

                int PackageQuantity = 1;
                bool HasFreeItems = false;

                foreach (Package p in Shipment)
                {
                    rateRequest.Shipment.Package[pIndex] = new ups2.ShipmentPackage();
                    ups2.ShipmentPackage upsPackage = rateRequest.Shipment.Package[pIndex];

                    // shipment details
                    // UPS packaging type
                    if (UPSConfig.PackagingType != string.Empty)
                        upsPackage.PackagingType.Code = UPSConfig.PackagingType;  // default is CustomerSuppliedPackage, '02'

                    // package weight
                    upsPackage.PackageWeight.Weight = p.Weight;
                    upsPackage.PackageWeight.UnitOfMeasurement = new ups2.UPSShipmentWeightUnitOfMeasurement();
                    if (ShippingConfig.WeightUnits.ToLower().Contains("kg") || ShippingConfig.WeightUnits.ToLower().Contains("kilo"))  // default is pounds
                        upsPackage.PackageWeight.UnitOfMeasurement.Code = ups2.UPSShipmentWeightUnitOfMeasurementCode.Kilograms;

                    // insurance
                    if (p.Insured && p.InsuredValue != 0)
                    {
                        upsPackage.PackageServiceOptions = new ups2.ShipmentPackageServiceOptions();
                        upsPackage.PackageServiceOptions.InsuredValue = new ups2.UPSMoney();
                        upsPackage.PackageServiceOptions.InsuredValue.MonetaryValue = p.InsuredValue.ToString();
                        upsPackage.PackageServiceOptions.InsuredValue.CurrencyCode = ShippingConfig.Currency.ToUpper();
                    }

                    // delivery confirmation
                    if (UPSConfig.DeliveryConfirmation != string.Empty)
                    {
                        if (upsPackage.PackageServiceOptions == null)
                            upsPackage.PackageServiceOptions = new ups2.ShipmentPackageServiceOptions();
                        upsPackage.PackageServiceOptions.DeliveryConfirmation = new ups2.ShipmentDeliveryConfirmation();

                        try
                        {
                            upsPackage.PackageServiceOptions.DeliveryConfirmation.DCISType = Enum.Format(typeof(ups2.DCISTypeCode),
                                Enum.Parse(typeof(ups2.DCISTypeCode), UPSConfig.DeliveryConfirmation, true), "d");
                        }
                        catch (ArgumentException)
                        {
                            throw new Exception("UPS Error: RTShipping.UPS.DeliveryConfirmation == '" + UPSConfig.DeliveryConfirmation + "'. " +
                                "Legal values are DeliveryConfirmation, SignatureRequired, or AdultSignatureRequired.");
                        }
                    }

                    // dimensions
                    if (p.Length + p.Width + p.Height != 0)  // if package has no dimensions, do not include them
                    {
                        upsPackage.Dimensions = new ups2.ShipmentPackageDimensions();
                        upsPackage.Dimensions.Length = p.Length;
                        upsPackage.Dimensions.Width = p.Width;
                        upsPackage.Dimensions.Height = p.Height;

                        // if weight is kg, UPS requires size to be cm.
                        if (upsPackage.PackageWeight.UnitOfMeasurement.Code == ups2.UPSShipmentWeightUnitOfMeasurementCode.Kilograms)
                            upsPackage.Dimensions.UnitOfMeasurement.Code = ups2.DimensionsUnitOfMeasurementCode.Centimeters; // default is inches
                    }

                    if (p.IsShipSeparately)
                        PackageQuantity = p.Quantity;

                    if (p.IsFreeShipping)
                        HasFreeItems = true;

                    pIndex++;
                }

                // serialize AccessRequest class
                XmlSerializer serAccessRequest = new XmlSerializer(typeof(ups2.AccessRequest));
                StringWriter swAccessRequest = new StringWriter();

                serAccessRequest.Serialize(swAccessRequest, accessCredentials);
                string req = swAccessRequest.ToString();

                // serialize RatingRequest class
                XmlSerializer serRatingRequest = new XmlSerializer(typeof(ups2.RatingRequest));
                StringWriter swRatingRequest = new StringWriter();

                serRatingRequest.Serialize(swRatingRequest, rateRequest);
                req += swRatingRequest.ToString(); // append RatingRequest to AccessRequest

                // Send xml rate request to UPS server
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(UPSServer);
                webRequest.Method = "POST";

                // Transmit the request to UPS
                byte[] data = System.Text.Encoding.ASCII.GetBytes(req);
                webRequest.ContentLength = data.Length;
                Stream requestStream;

                try
                {
                    requestStream = webRequest.GetRequestStream();
                }
                catch (WebException e)  // could not connect to UPS endpoint
                {
                    RTShipResponse += "Tried to reach UPS Server (" + UPSServer + "): " + e.Message;
                    return UPS2ShipMethods;
                }

                requestStream.Write(data, 0, data.Length);
                requestStream.Close();

                // get the response from UPS
                WebResponse webResponse = null;
                string resp;
                try
                {
                    webResponse = webRequest.GetResponse();
                }
                catch (WebException e)  // could not receive a response from UPS endpoint
                {
                    RTShipResponse += "No response from UPS Server (" + UPSServer + "): " + e.Message;
                    return UPS2ShipMethods;
                }

                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    resp = sr.ReadToEnd();
                    sr.Close();
                }
                webResponse.Close();

                // deserialize the xml response into a RatingResponse object
                ups2.RatingResponse response = new ups2.RatingResponse();
                XmlSerializer serResponse = new XmlSerializer(typeof(ups2.RatingResponse));
                StringReader srResponse = new StringReader(resp);

                try
                {
                    response = (ups2.RatingResponse)serResponse.Deserialize(srResponse);
                }
                catch (InvalidOperationException e)  // invalid xml, or no reply received from UPS
                {
                    RTShipResponse += "Could not parse response from UPS server: " + e.Message + " Response received: " + resp;
                    return UPS2ShipMethods;
                }

                srResponse.Close();

                // Check the response object for Faults
                if (response.Response.Error != null)
                {
                    RTShipResponse += "UPS Error: " + response.Response.Error.ErrorDescription;
                    if (response.Response.Error.ErrorLocation != null)
                        RTShipResponse += " " + response.Response.Error.ErrorLocation.ErrorLocationElementName;
                    return UPS2ShipMethods;
                }

                // Check the response object for RatedShipments
                if (response.RatedShipment == null)
                {
                    RTShipResponse += "UPS Error: No rating responses returned from UPS.";
                    return UPS2ShipMethods;
                }

                // walk the services list, looking for matches in the returned rates
                foreach (string service in UPSConfig.Services.Split(','))
                {
                    // interate the returned rates, matching with the service from the UPSServices list
                    foreach (ups2.ResponseRatedShipment ratedShipment in response.RatedShipment)
                    {
                        string serviceDescription = string.Empty;

                        // check for a match between the returned rate and the current service
                        if (ratedShipment.Service.Code == service.Split(';')[0])
                            serviceDescription = service.Split(';')[1];

                        // verify the quoted rate is in the same currency as the store
                        if (ratedShipment.TotalCharges.CurrencyCode != ShippingConfig.Currency.ToUpper())
                        {
                            RTShipResponse += "UPS Error: Received rates with currency code " + ratedShipment.TotalCharges.CurrencyCode +
                                ", but store is configured for " + ShippingConfig.Currency + ".";
                            return UPS2ShipMethods;
                        }

                        decimal total;

                        // use either negotiated rates or regular rates
                        if (UPSConfig.GetNegotiatedRates)
                            if (ratedShipment.NegotiatedRates != null)
                                total = Convert.ToDecimal(ratedShipment.NegotiatedRates.NetSummaryCharges.GrandTotal.MonetaryValue);
                            else  // no negotiated rates found
                                throw new Exception("UPS Error: GetNegotiatedRates is 'true', but no negotiated rates were returned. " +
                                    "Cantact UPS to ensure that you are authorized to receive negotiated rates.");
                        else
                            total = Convert.ToDecimal(ratedShipment.TotalCharges.MonetaryValue);

                        // ignore zero-cost methods, and methods not allowed
                        if (total == 0 || serviceDescription == string.Empty || !RTShipping.ShippingMethodIsAllowed(serviceDescription, "UPS"))
                            continue;

                        total = total * PackageQuantity * (1.00M + (ShippingConfig.MarkupPercent / 100.0M)) + ShippingConfig.ExtraFee;
                        decimal vat = Decimal.Round(total * ShippingTaxRate);

                        // add or update method in UPS2ShipMethods shipping methods collection
                        if (!UPS2ShipMethods.MethodExists(serviceDescription))
                        {
                            ShipMethod s_method = new ShipMethod();
                            s_method.Carrier = "UPS";
                            s_method.ServiceName = serviceDescription;
                            s_method.ServiceRate = total;
                            s_method.VatRate = vat;

                            if (HasFreeItems)
                                s_method.FreeItemsRate = total;

                            UPS2ShipMethods.AddMethod(s_method);
                        }
                        else
                        {
                            int IndexOf = UPS2ShipMethods.GetIndex(serviceDescription);
                            ShipMethod s_method = UPS2ShipMethods[IndexOf];

                            s_method.ServiceRate += total;
                            s_method.VatRate += vat;

                            if (HasFreeItems)
                                s_method.FreeItemsRate += total;

                            UPS2ShipMethods[IndexOf] = s_method;
                        }
                    }
                }
                RTShipRequest += req; // stash request & response for this shipment
                RTShipResponse += resp;
            }
            return UPS2ShipMethods;
        }
    }

    namespace ups2
    {
        public class ShippingSection : ConfigurationElement
        {
            [ConfigurationProperty("FilterOutShippingMethodsThatHave0Cost"), Description("if you want to filter out shipping methods that result in $0 costs to the customer, you can set this flag to true. Otherwise, all shipping methods are returned and displayed. This appconfig should almost always be false.")]
            public bool FilterOutShippingMethodsThatHave0Cost
            {
                get { return (bool)this["FilterOutShippingMethodsThatHave0Cost"]; }
                set { this["FilterOutShippingMethodsThatHave0Cost"] = (bool)value; }
            }

            [ConfigurationProperty("OriginAddress"), Description("The address of where you are shipping from")]
            public string OriginAddress
            {
                get { return (string)this["OriginAddress"]; }
                set { this["OriginAddress"] = (string)value; }
            }

            [ConfigurationProperty("OriginAddress2"), Description("The address of where you are shipping from")]
            public string OriginAddress2
            {
                get { return (string)this["OriginAddress2"]; }
                set { this["OriginAddress2"] = (string)value; }
            }

            [ConfigurationProperty("OriginCity"), Description("The city of where you are shipping from")]
            public string OriginCity
            {
                get { return (string)this["OriginCity"]; }
                set { this["OriginCity"] = (string)value; }
            }

            [ConfigurationProperty("OriginState"), Description("The state/province of where you are shipping from")]
            public string OriginState
            {
                get { return (string)this["OriginState"]; }
                set { this["OriginState"] = (string)value; }
            }

            [ConfigurationProperty("OriginZip"), Description("The zip code of where you are shipping from")]
            public string OriginZip
            {
                get { return (string)this["OriginZip"]; }
                set { this["OriginZip"] = (string)value; }
            }

            [ConfigurationProperty("OriginCountry"), Description("The Country of where you are shipping from")]
            public string OriginCountry
            {
                get { return (string)this["OriginCountry"]; }
                set { this["OriginCountry"] = (string)value; }
            }

            [ConfigurationProperty("MarkupPercent"), Description("A markup percentage to the rates returned by the carrier")]
            public decimal MarkupPercent
            {
                get { return (decimal)this["MarkupPercent"]; }
                set { this["MarkupPercent"] = (decimal)value; }
            }

            [ConfigurationProperty("ExtraFee"), Description("A fee that is added to the rates returned by the carrier")]
            public decimal ExtraFee
            {
                get { return (decimal)this["ExtraFee"]; }
                set { this["ExtraFee"] = (decimal)value; }
            }

            [ConfigurationProperty("CallForShippingPrompt"), Description("Customer prompt, if the order exceeds maximum shipping weight")]
            public string CallForShippingPrompt
            {
                get { return (string)this["CallForShippingPrompt"]; }
                set { this["CallForShippingPrompt"] = (string)value; }
            }

            [ConfigurationProperty("UseTestRates"), Description("Contacts the live or test servers")]
            public bool UseTestRates
            {
                get { return (bool)this["UseTestRates"]; }
                set { this["UseTestRates"] = (bool)value; }
            }

            [ConfigurationProperty("Currency"), Description("Currency Code used by the store")]
            public string Currency
            {
                get { return (string)this["Currency"]; }
                set { this["Currency"] = (string)value; }
            }

            [ConfigurationProperty("WeightUnits"), Description("Weight Units used by products")]
            public string WeightUnits
            {
                get { return (string)this["WeightUnits"]; }
                set { this["WeightUnits"] = (string)value; }
            }

        }

        public class UPSSection : ConfigurationElement
        {
            [ConfigurationProperty("UserName"), Description("Your UPS Account UserName, assigned by UPS. This is NOT the general UPS web site account. You must get a UPS account for Real Time Shipping Rates")]
            public string UserName
            {
                get { return (string)this["UserName"]; }
                set { this["UserName"] = (string)value; }
            }

            [ConfigurationProperty("Password"), Description("Your UPS Account Password, assigned by UPS. This is NOT the general UPS web site account. You must get a UPS account for Real Time Shipping Rates")]
            public string Password
            {
                get { return (string)this["Password"]; }
                set { this["Password"] = (string)value; }
            }

            [ConfigurationProperty("License"), Description("Your UPS Account License, or Access Key they may call this. You get this on the UPS web site, after logging in, and then entering your \"developers license\" which was assigned by UPS.")]
            public string License
            {
                get { return (string)this["License"]; }
                set { this["License"] = (string)value; }
            }

            [ConfigurationProperty("Server"), Description("The UPS live shipping rates server")]
            public string Server
            {
                get { return (string)this["Server"]; }
                set { this["Server"] = (string)value; }
            }

            [ConfigurationProperty("TestServer"), Description("The UPS test shipping rates server")]
            public string TestServer
            {
                get { return (string)this["TestServer"]; }
                set { this["TestServer"] = (string)value; }
            }

            [ConfigurationProperty("PickupType"), Description("The type of UPS pickup being used. Allowed values are: UPSDailyPickup, UPSCustomerCounter, UPSOneTimePickup, UPSOnCallAir, UPSSuggestedRetailRates, UPSLetterCenter, UPSAirServiceCenter")]
            public string PickupType
            {
                get { return (string)this["PickupType"]; }
                set { this["PickupType"] = (string)value; }
            }

            [ConfigurationProperty("MaxWeight"), Description("The maximum allowed weight for a UPS shipment, in AppConfig:RTSHipping.WeightUnits. If an order weight exceeds this, then the AppConfig:CallForShippingPrompt will be displayed as the shipping method, with a $0 cost")]
            public decimal MaxWeight
            {
                get { return (decimal)this["MaxWeight"]; }
                set { this["MaxWeight"] = (decimal)value; }
            }

            [ConfigurationProperty("AccountNumber"), Description("The six-digit account number used for negotiated rates")]
            public string AccountNumber
            {
                get { return (string)this["AccountNumber"]; }
                set { this["AccountNumber"] = (string)value; }
            }

            [ConfigurationProperty("AddressTypeBehavior"), Description("Defines how the AddressType field of the Ship-To address is handled")]
            public string AddressTypeBehavior
            {
                get { return (string)this["AddressTypeBehavior"]; }
                set { this["AddressTypeBehavior"] = (string)value; }
            }

            [ConfigurationProperty("CustomerClassification"), Description("UPS Customer Classification, default is Wholesale")]
            public string CustomerClassification
            {
                get { return (string)this["CustomerClassification"]; }
                set { this["CustomerClassification"] = (string)value; }
            }

            [ConfigurationProperty("DeliveryConfirmation"), Description("UPS Delivery Confirmation")]
            public string DeliveryConfirmation
            {
                get { return (string)this["DeliveryConfirmation"]; }
                set { this["DeliveryConfirmation"] = (string)value; }
            }

            [ConfigurationProperty("GetNegotiatedRates"), Description("If true, negotiated rates are retrieved for the UPS AccountNumber")]
            public bool GetNegotiatedRates
            {
                get { return (bool)this["GetNegotiatedRates"]; }
                set { this["GetNegotiatedRates"] = (bool)value; }
            }

            [ConfigurationProperty("PackagingType"), Description("2-char code for the UPS Packaging Type, default is Customer Provided Packaging")]
            public string PackagingType
            {
                get { return (string)this["PackagingType"]; }
                set { this["PackagingType"] = (string)value; }
            }

            [ConfigurationProperty("Services"), Description("Comma-separated list of UPS Services, in the format CODE;Display Name")]
            public string Services
            {
                get { return (string)this["Services"]; }
                set { this["Services"] = (string)value; }
            }

        }

        public class AccessRequest
        {
            [XmlAttribute()]
            public string lang;

            public string AccessLicenseNumber;
            public string UserId;
            public string Password;
        }

        public class Address
        {
            public string AddressLine1;
            public string City;
            public string StateProvinceCode;
            public string PostalCode;
            public string CountryCode;
        }

        [XmlRoot("RatingServiceSelectionRequest")]
        public class RatingRequest
        {
            [XmlAttribute()]
            public string lang = "en-US";

            public RequestRequest Request = new RequestRequest();
            public RequestPickupType PickupType = new RequestPickupType();
            public RequestCustomerClassification CustomerClassification = new RequestCustomerClassification();
            public RequestShipment Shipment = new RequestShipment();
        }

        public class RequestRequest
        {
            public string RequestAction = "Rate";
            public string RequestOption = "Shop";
            public TransactionReference TransactionReference = new TransactionReference();
        }

        public class RequestPickupType
        {
            public string Code;
        }

        public enum RequestPickupTypeCode
        {
            UPSDailyPickup = 1,
            UPSCustomerCounter = 3,
            UPSOneTimePickup = 6,
            UPSOnCallAir = 7,
            UPSSuggestedRetailRates = 11,
            UPSLetterCenter = 19,
            UPSAirServiceCenter = 20
        }

        public class RequestCustomerClassification
        {
            public string Code = RequestCustomerClassificationCode.Wholesale;
        }

        public class RequestCustomerClassificationCode
        {
            public const string Wholesale = "01";
            public const string Occasional = "03";
            public const string Retail = "04";
        }

        public class TransactionReference
        {
            public string CustomerContext;
            public string XpciVersion = "1.0";
            public string ToolVersion;
        }

        public class RequestShipment
        {
            public string Description;
            public RequestShipmentShipper Shipper;
            public RequestShipmentShipTo ShipTo = new RequestShipmentShipTo();
            public RequestShipmentShipFrom ShipFrom;
            public UPSShipmentWeight ShipmentWeight;
            public UPSService Service;
            public string DocumentsOnly;

            [XmlElement("Package")]
            public ShipmentPackage[] Package;
            public ShipmentServiceOptions ShipmentServiceOptions;
            public ShipmentRateInformation RateInformation;
        }

        public class RequestShipmentShipper
        {
            public string ShipperNumber;
            public UPSAddress Address = new UPSAddress();
        }

        public class RequestShipmentShipTo
        {
            public string ShipperAssignedIdentificationNumber;
            public string CompanyName;
            public string AttentionName;
            public string PhoneNumber;
            public string TaxIdentificationNumber;
            public string FaxNumber;
            public UPSAddress Address = new UPSAddress();
            public string LocationID;
        }

        public class RequestShipmentShipFrom
        {
            public string CompanyName;
            public string AttentionName;
            public string PhoneNumber;
            public string FaxNumber;
            public UPSAddress Address = new UPSAddress();
        }

        public class UPSAddress
        {
            public string AddressLine1;
            public string AddressLine2;
            public string AddressLine3;
            public string City;
            public string StateProvinceCode;
            public string PostalCode;
            public string CountryCode;
            public string ResidentialAddressIndicator;
        }

        public class UPSShipmentWeight
        {
            public UPSShipmentWeightUnitOfMeasurement UnitOfMeasurement = new UPSShipmentWeightUnitOfMeasurement();
            public string Weight;
        }

        public class UPSShipmentWeightUnitOfMeasurement
        {
            public string Code = UPSShipmentWeightUnitOfMeasurementCode.Pounds;
            public string Description;
        }

        public class UPSShipmentWeightUnitOfMeasurementCode
        {
            public const string Pounds = "LBS";
            public const string Kilograms = "KGS";
        }

        public class UPSService
        {
            public string Code;
            public string Description;
        }

        public class ShipmentPackage
        {
            public ShipmentPackagingType PackagingType = new ShipmentPackagingType();
            public string Description;
            public ShipmentPackageDimensions Dimensions;
            public ShipmentPackageWeight DimensionalWeight;
            public ShipmentPackageWeight PackageWeight = new ShipmentPackageWeight();
            public string LargePackageIndicator;
            public ShipmentPackageServiceOptions PackageServiceOptions;
            public string AdditionalHandling;
        }

        public class ShipmentPackagingType
        {
            public string Code = ShipmentPackagingTypeCode.CustomerSuppliedPackage;
            public string Description;
        }

        public class ShipmentPackagingTypeCode
        {
            public const string Unknown = "00";
            public const string UPSLetter = "01";
            public const string CustomerSuppliedPackage = "02";
            public const string Tube = "03";
            public const string PAK = "04";
            public const string UPSExpressBox = "21";
            public const string UPSSmallExpressBox = "2a";
            public const string UPSMediumExpressBox = "2b";
            public const string UPSLargeExpressBox = "2c";
            public const string UPS25KgBox = "24";
            public const string UPS10KgBox = "25";
            public const string Pallet = "30";
        }

        public class ShipmentPackageDimensions
        {
            public DimensionsUnitOfMeasurement UnitOfMeasurement = new DimensionsUnitOfMeasurement();
            public decimal Length;
            public decimal Width;
            public decimal Height;
        }

        public class DimensionsUnitOfMeasurement
        {
            public string Code = DimensionsUnitOfMeasurementCode.Inches;
            public string Description;
        }

        public class DimensionsUnitOfMeasurementCode
        {
            public const string Inches = "IN";
            public const string Centimeters = "CM";
        }

        public class ShipmentPackageWeight
        {
            public UPSShipmentWeightUnitOfMeasurement UnitOfMeasurement;
            public decimal Weight;
        }

        public class ShipmentPackageServiceOptions
        {
            public UPSMoney InsuredValue;
            public ShipmentDeliveryConfirmation DeliveryConfirmation;
        }

        public class ShipmentDeliveryConfirmation
        {
            public string DCISType;
        }

        public enum DCISTypeCode
        {
            DeliveryConfirmation = 1,
            SignatureRequired = 2,
            AdultSignatureRequired = 3
        }

        public class ShipmentServiceOptions
        {
            public string SaturdayPickupIndicator;
        }

        [XmlRoot("RateInformation")]
        public class ShipmentRateInformation
        {
            [XmlElement("NegotiatedRatesIndicator")]
            public string NegotiatedRatesIndicator;
        }

        [XmlRoot("RatingServiceSelectionResponse")]
        public class RatingResponse
        {
            public ResponseResponse Response;

            [XmlElement("RatedShipment")]
            public ResponseRatedShipment[] RatedShipment;
        }

        public class ResponseResponse
        {
            public TransactionReference TransactionReference;
            public string ResponseStatusCode;
            public string ResponseStatusDescription;
            public ResponseError Error;
        }

        public class ResponseError
        {
            public string ErrorSeverity;
            public string ErrorCode;
            public string ErrorDescription;
            public string MinimumRetrySeconds;
            public ResponseErrorLocation ErrorLocation;
            public string ErrorDigest;
        }

        public class ResponseErrorLocation
        {
            public string ErrorLocationElementName;
            public string ErrorLocationElementReference;
            public string ErrorLocationAttributeName;
        }

        public class ResponseRatedShipment
        {
            public UPSService Service;

            [XmlElement("RatedShipmentWarning")]
            public string[] RatedShipmentWarning;

            public UPSShipmentWeight BillingWeight;
            public UPSMoney TransportationCharges;
            public UPSMoney ServiceOptionsCharges;
            public UPSMoney HandlingChargeAmount;
            public UPSMoney TotalCharges;
            public string GuaranteedDaysToDelivery;
            public string ScheduledDeliveryTime;

            [XmlElement("RatedPackage")]
            public RatedShipmentRatedPackage[] RatedPackage;

            public RatedShipmentNegotiatedRates NegotiatedRates;
        }

        public class UPSMoney
        {
            public string CurrencyCode;
            public string MonetaryValue;
        }

        public class RatedShipmentRatedPackage
        {
            public UPSMoney TransportationCharges;
            public UPSMoney ServiceOptionsCharges;
            public UPSMoney TotalCharges;
            public string Weight;
            public UPSShipmentWeight BillingWeight;
        }

        public class RatedShipmentNegotiatedRates
        {
            public RatedShipmentNetSummaryCharges NetSummaryCharges;
        }

        public class RatedShipmentNetSummaryCharges
        {
            public UPSMoney GrandTotal;
        }
    }
}
