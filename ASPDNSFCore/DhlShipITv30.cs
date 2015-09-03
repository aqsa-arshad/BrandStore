// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Extends RTShipping class to include DHL International
    /// </summary>
    public partial class RTShipping
    {
        /// <summary>
        /// Get shipping rates from DHL for International shipments
        /// </summary>
        private void DHLIntlGetRates(Shipments allShipments, out string rtShipRequest, out string rtShipResponse, decimal extraFee, decimal markupPercent, decimal shipmentValue, decimal shippingTaxRate)
        {
            rtShipRequest = String.Empty;
            rtShipResponse = String.Empty;
            System.Collections.Specialized.NameValueCollection ratesList = new System.Collections.Specialized.NameValueCollection();

            // is weight within shippable limit?
            if (ShipmentWeight > AppLogic.AppConfigUSDecimal("RTShipping.DHLIntl.MaxWeight"))
            {
                SM.ErrorMsg = "DHL " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                return;
            }

            // retrieve correct DHL Server URL
            string dhlServer;
            if (AppLogic.AppConfigBool("RTShipping.UseTestRates"))
            {
                dhlServer = AppLogic.AppConfig("RTShipping.DHL.TestServer");
            }
            else
            {
                dhlServer = AppLogic.AppConfig("RTShipping.DHL.Server");
            }

            // calculate legal ship date
            DateTime shipDate = DateTime.Now.AddDays(AppLogic.AppConfigUSDouble("RTShipping.DHL.ShipInDays"));
            if (shipDate.DayOfWeek == DayOfWeek.Saturday) shipDate = shipDate.AddDays(2);
            if (shipDate.DayOfWeek == DayOfWeek.Sunday) shipDate = shipDate.AddDays(1);

            // error 4112 is tripped by asking for a rate quote on a Sunday or a Holiday
            bool error4112 = false;

            do
            {
                if (error4112)
                {
                    error4112 = false;
                    shipDate = shipDate.AddDays(1);
                }

                foreach (Packages shipment in allShipments)
                {
                    HasFreeItems = false;
                    PackageQuantity = 1;

                    foreach (Package p in shipment)
                    {
                        if (p.IsFreeShipping) HasFreeItems = true;
                        if (p.IsShipSeparately) PackageQuantity = p.Quantity;

                        // initialize rate requests
                        dhl.InternationalRateRequest rateRequests = new dhl.InternationalRateRequest();
                        rateRequests.Requestor.ID = AppLogic.AppConfig("RTShipping.DHL.APISystemID");
                        rateRequests.Requestor.Password = AppLogic.AppConfig("RTShipping.DHL.APISystemPassword");

                        string dhlServices = AppLogic.AppConfig("RTShipping.DHLIntl.Services");

                        // create an array of individual requests, one for each service
                        rateRequests.Shipment = new dhl.InternationalRequest[dhlServices.Split(',').Length];

                        // populate the array
                        int serviceIndex = 0;
                        foreach (string service in dhlServices.Split(','))
                        {
                            rateRequests.Shipment[serviceIndex] = new dhl.InternationalRequest();
                            dhl.InternationalRequest request = rateRequests.Shipment[serviceIndex];

                            // DHL rating API credentials
                            request.ShippingCredentials.ShippingKey = AppLogic.AppConfig("RTShipping.DHLIntl.ShippingKey");
                            request.ShippingCredentials.AccountNbr = AppLogic.AppConfig("RTShipping.DHL.AccountNumber");

                            // shipment details
                            request.ShipmentDetail.ShipDate = shipDate.ToString("yyyy-MM-dd");
                            request.ShipmentDetail.ShipmentType.Code = AppLogic.AppConfig("RTShipping.DHLIntl.Packaging").ToUpperInvariant();

                            // used to allow 'O' (Other) packaging types, which are now 'P' types
                            if (request.ShipmentDetail.ShipmentType.Code == "O") request.ShipmentDetail.ShipmentType.Code = "P";

                            // package weight
                            if (request.ShipmentDetail.ShipmentType.Code == "L")
                            {
                                request.ShipmentDetail.Weight = p.Weight.ToString("#0.0");
                            }
                            else
                            {
                                request.ShipmentDetail.Weight = Math.Ceiling(p.Weight).ToString("##0");
                            }

                            request.ShipmentDetail.ContentDesc = "ContentDesc";

                            // billing details
                            request.Billing.Party.Code = AppLogic.AppConfig("RTShipping.DHLIntl.BillingParty").ToUpperInvariant();
                            if (request.Billing.Party.Code != "S") request.Billing.AccountNbr = AppLogic.AppConfig("RTShipping.DHLIntl.BillingAccountNbr").ToUpperInvariant();

                            request.Billing.DutyPaymentType = AppLogic.AppConfig("RTShipping.DHLIntl.DutyPayment").ToUpperInvariant();
                            if (request.Billing.DutyPaymentType == "3") request.Billing.DutyPaymentAccountNbr = AppLogic.AppConfig("RTShipping.DHLIntl.DutyPaymentAccountNbr").ToUpperInvariant();

                            // import duty declaration
                            request.Dutiable.DutiableFlag = AppLogic.AppConfig("RTShipping.DHLIntl.Dutiable").ToUpperInvariant();
                            request.Dutiable.CustomsValue = p.InsuredValue.ToString("######");

                            // overrides
                            string overrideCodes = AppLogic.AppConfig("RTShipping.DHLIntl.Overrides");
                            if (overrideCodes.Length > 0)
                            {
                                request.ShipmentProcessingInstructions.Overrides = new dhl.Override[overrideCodes.Split(',').Length];

                                int overrideIndex = 0;
                                foreach (string overrideCode in overrideCodes.Split(','))
                                {
                                    request.ShipmentProcessingInstructions.Overrides[overrideIndex] = new dhl.Override();
                                    request.ShipmentProcessingInstructions.Overrides[overrideIndex].Code = overrideCode;

                                    overrideIndex++;
                                }
                            }

                            // ship-to address
                            request.Receiver.Address.Street = "Street";
                            request.Receiver.Address.City = shipment.DestinationCity;
                            if (shipment.DestinationCountryCode.Equals("ca", StringComparison.OrdinalIgnoreCase) ||
                                shipment.DestinationCountryCode.Equals("us", StringComparison.OrdinalIgnoreCase))
                            {
                                request.Receiver.Address.State = shipment.DestinationStateProvince;
                            }

                            request.Receiver.Address.Country = shipment.DestinationCountryCode.ToUpperInvariant();
                            request.Receiver.Address.PostalCode = shipment.DestinationZipPostalCode.ToUpperInvariant();

                            // dimensions
                            if (p.Length + p.Width + p.Height != 0)
                            {
                                request.ShipmentDetail.Dimensions = new dhl.Dimensions();
                                request.ShipmentDetail.Dimensions.Length = p.Length.ToString("###");
                                request.ShipmentDetail.Dimensions.Width = p.Width.ToString("###");
                                request.ShipmentDetail.Dimensions.Height = p.Height.ToString("###");
                            }

                            // insurance
                            if (p.Insured && p.InsuredValue != 0)
                            {
                                request.ShipmentDetail.AdditionalProtection.Code = "AP";  // additional protection
                                request.ShipmentDetail.AdditionalProtection.Value = p.InsuredValue.ToString("######");
                            }
                            else
                            {
                                request.ShipmentDetail.AdditionalProtection.Code = "NR";  // not required
                                request.ShipmentDetail.AdditionalProtection.Value = "0";
                            }

                            // add the service code, and service name to the request
                            request.ShipmentDetail.Service.Code = service.Split(';')[0];
                            request.TransactionTrace = service.Split(';')[1];

                            // add this individual service request to the rateRequests
                            rateRequests.Shipment[serviceIndex] = request;

                            serviceIndex++;
                        }

                        // serialize rateRequests into an xml string
                        XmlWriterSettings xwSettings = new XmlWriterSettings();
                        xwSettings.OmitXmlDeclaration = true;

                        XmlSerializer serRequest = new XmlSerializer(rateRequests.GetType());
                        StringWriter swRequest = new StringWriter();
                        XmlWriter xwRequest = XmlWriter.Create(swRequest, xwSettings);
                        serRequest.Serialize(xwRequest, rateRequests);

                        string req = swRequest.ToString();

                        // Send xml rate request to DHL server
                        HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(dhlServer);
                        webRequest.Method = "POST";

                        webRequest.ProtocolVersion = HttpVersion.Version11;
                        webRequest.ContentType = "application/xml; charset=UTF-8";
                        webRequest.Accept = "application/xml; charset=UTF-8";

                        // Transmit the request to DHL
                        byte[] data = System.Text.Encoding.ASCII.GetBytes(req);
                        webRequest.ContentLength = data.Length;
                        Stream requestStream;

                        try
                        {
                            requestStream = webRequest.GetRequestStream();
                        }
                        catch (WebException e)
                        {
                            // could not connect to DHL endpoint
                            rtShipResponse += "Tried to reach DHL Server (" + dhlServer + "): " + e.Message;
                            return;
                        }

                        requestStream.Write(data, 0, data.Length);
                        requestStream.Close();

                        // get the response from DHL
                        WebResponse webResponse = null;
                        string resp;
                        try
                        {
                            webResponse = webRequest.GetResponse();
                        }
                        catch (WebException e)
                        {
                            // could not receive a response from DHL endpoint
                            rtShipResponse += "No response from DHL Server (" + dhlServer + "): " + e.Message;
                            return;
                        }

                        using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                        {
                            resp = sr.ReadToEnd();
                            sr.Close();
                        }

                        webResponse.Close();

                        // deserialize the xml response into an InternationalRateResponse object
                        dhl.InternationalRateResponse response = new dhl.InternationalRateResponse();
                        XmlSerializer serResponse = new XmlSerializer(typeof(dhl.InternationalRateResponse));
                        StringReader srResponse = new StringReader(resp);

                        try
                        {
                            response = (dhl.InternationalRateResponse)serResponse.Deserialize(srResponse);
                        }
                        catch (InvalidOperationException e)
                        {
                            // invalid xml, or no reply received from DHL
                            rtShipResponse += "Could not parse response from DHL server: " + e.Message + " Response received: " + resp;
                            return;
                        }

                        srResponse.Close();

                        // Check the response object for Ratings
                        if (response.Shipment == null)
                        {
                            rtShipResponse += "DHL Error: No rating responses returned from DHL.";
                            return;
                        }

                        // Check the response object for Faults
                        if (response.Shipment[0].Faults != null)
                        {
                            if (response.Shipment[0].Faults[0].Code == "4112")
                            {
                                error4112 = true;
                            }
                            else
                            {
                                rtShipResponse += "DHL Error: " + response.Shipment[0].Faults[0].Desc;
                                return;
                            }
                        }

                        // one response for each rate request
                        foreach (dhl.Response rateResponse in response.Shipment)
                        {
                            // check for a rate estimate
                            if (rateResponse.EstimateDetail == null) break;

                            // we have a good estimate
                            decimal total = Localization.ParseUSDecimal(rateResponse.EstimateDetail.RateEstimate.TotalChargeEstimate);

                            // ignore zero-cost methods, and methods not allowed
                            if ((total == 0 && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost")) ||
                                !ShippingMethodIsAllowed(rateResponse.TransactionTrace, string.Empty))
                            {
                                break;
                            }

                            total = total * PackageQuantity * (1.00M + (markupPercent / 100.0M));
                            decimal vat = Decimal.Round(total * shippingTaxRate);

                            if (!SM.MethodExists(rateResponse.TransactionTrace))
                            {
                                ShipMethod s_method = new ShipMethod();
                                s_method.Carrier = "DHL";
                                s_method.ServiceName = rateResponse.TransactionTrace;
                                s_method.ServiceRate = total;
                                s_method.VatRate = vat;

                                if (HasFreeItems) s_method.FreeItemsRate = total;

                                SM.AddMethod(s_method);
                            }
                            else
                            {
                                int indexOf = SM.GetIndex(rateResponse.TransactionTrace);
                                ShipMethod s_method = SM[indexOf];

                                s_method.ServiceRate += total;
                                s_method.VatRate += vat;

                                if (HasFreeItems) s_method.FreeItemsRate += total;

                                SM[indexOf] = s_method;
                            }
                        }

                        rtShipRequest += req;
                        rtShipResponse += resp;
                    }
                }
            }
            while (error4112 == true);

            // Handling fee should only be added per shipping address not per package
            // let's just compute it here after we've gone through all the packages.
            // Also, since we can't be sure about the ordering of the method call here
            // and that the collection SM includes shipping methods from all possible carriers
            // we'll need to filter out the methods per this carrier to avoid side effects on the main collection
            foreach (ShipMethod shipMethod in SM.PerCarrier("DHL"))
            {
                shipMethod.ServiceRate += extraFee;
            }
        }
    }

    namespace dhl
    {
        [XmlRoot("ECommerce")] public partial class InternationalRateRequest
        {
            [XmlAttribute("action")] public string action = "Request";
            [XmlAttribute("version")] public string version = "1.1";
            [XmlElement("Requestor")] public Requestor Requestor = new Requestor();
            [XmlElement("IntlShipment")] public InternationalRequest[] Shipment;
        }

        [XmlRoot("ECommerce")] public partial class InternationalRateResponse
        {
            [XmlAttribute("action")] public string action;
            [XmlAttribute("version")] public string version;
            [XmlElement("Requestor")] public Requestor Requestor = new Requestor();
            [XmlElement("IntlShipment")] public Response[] Shipment;
        }

        public partial class InternationalRequest
        {
            [XmlAttribute("action")] public string action = "RateEstimate";
            [XmlAttribute("version")] public string version = "3.0";
            [XmlElement("ShippingCredentials")] public ShippingCredentials ShippingCredentials = new ShippingCredentials();
            [XmlElement("ShipmentDetail")] public InternationalRequestShipmentDetail ShipmentDetail = new InternationalRequestShipmentDetail();
            [XmlElement("Dutiable")] public Dutiable Dutiable = new Dutiable();
            [XmlElement("Billing")] public InternationalBilling Billing = new InternationalBilling();
            [XmlElement("Receiver")] public InternationalReceiver Receiver = new InternationalReceiver();
            [XmlElement("ShipmentProcessingInstructions")] public ShipmentProcessingInstructions ShipmentProcessingInstructions = new ShipmentProcessingInstructions();
            [XmlElement("TransactionTrace")] public string TransactionTrace;
        }

        public partial class Response
        {
            [XmlAttribute("action")] public string action;
            [XmlAttribute("version")] public string version;
            [XmlArrayItem("Fault")] public Fault[] Faults;
            [XmlElement("Result")] public Result Result;
            [XmlElement("ShippingCredentials")] public ShippingCredentials ShippingCredentials;
            [XmlElement("EstimateDetail")] public EstimateDetail EstimateDetail;
            [XmlElement("TransactionTrace")] public string TransactionTrace;
        }

        public partial class Requestor
        {
            [XmlElement("ID")] public string ID;
            [XmlElement("Password")] public string Password;
        }

        public partial class ShippingCredentials
        {
            [XmlElement("ShippingKey")] public string ShippingKey;
            [XmlElement("AccountNbr")] public string AccountNbr;
        }

        public partial class InternationalRequestShipmentDetail
        {
            [XmlElement("ShipDate")] public string ShipDate;
            [XmlElement("Service")] public Service Service = new Service();
            [XmlElement("ShipmentType")] public ShipmentType ShipmentType = new ShipmentType();
            [XmlElement("MultiRate")] public string MultiRate = "N";
            [XmlElement("Weight")] public string Weight;
            [XmlElement("ContentDesc")] public string ContentDesc;
            [XmlElement("Dimensions")] public Dimensions Dimensions;
            [XmlElement("AdditionalProtection")] public AdditionalProtection AdditionalProtection = new AdditionalProtection();
        }

        public partial class Service
        {
            [XmlElement("Code")] public string Code;
        }

        public partial class ShipmentType
        {
            [XmlElement("Code")] public string Code;
        }

        public partial class Dimensions
        {
            [XmlElement("Length")] public string Length;
            [XmlElement("Width")] public string Width;
            [XmlElement("Height")] public string Height;
        }

        public partial class AdditionalProtection
        {
            [XmlElement("Code")] public string Code;
            [XmlElement("Value")] public string Value;
        }

        public partial class Dutiable
        {
            [XmlElement("DutiableFlag")] public string DutiableFlag;
            [XmlElement("CustomsValue")] public string CustomsValue;
        }

        public partial class InternationalBilling
        {
            [XmlElement("Party")] public BillingParty Party = new BillingParty();
            [XmlElement("AccountNbr")] public string AccountNbr;
            [XmlElement("DutyPaymentType")] public string DutyPaymentType;
            [XmlElement("DutyPaymentAccountNbr")] public string DutyPaymentAccountNbr;
        }

        public partial class BillingParty
        {
            [XmlElement("Code")] public string Code;
        }

        public partial class InternationalReceiver
        {
            [XmlElement("Address")] public InternationalReceiverAddress Address = new InternationalReceiverAddress();
        }

        public partial class InternationalReceiverAddress
        {
            [XmlElement("Street")] public string Street;
            [XmlElement("City")] public string City;
            [XmlElement("State")] public string State;
            [XmlElement("Country")] public string Country;
            [XmlElement("PostalCode")] public string PostalCode;
        }

        public partial class ShipmentProcessingInstructions
        {
            [XmlArrayItem("Override")] public Override[] Overrides;
        }

        public partial class Override
        {
            [XmlElement("Code")] public string Code;
        }

        public partial class Result
        {
            [XmlElement("Code")] public string Code;
            [XmlElement("Desc")] public string Desc;
        }

        public partial class EstimateDetail
        {
            [XmlElement("DateGenerated")] public string DateGenerated;
            [XmlElement("ShipDate")] public string ShipDate;
            [XmlElement("Service")] public Service Service;
            [XmlElement("ServiceLevelCommitment")] public ServiceLevelCommitment ServiceLevelCommitment;
            [XmlElement("RateEstimate")] public RateEstimate RateEstimate;
        }

        public partial class ServiceLevelCommitment
        {
            [XmlElement("Desc")] public string Desc;
        }

        public partial class RateEstimate
        {
            [XmlElement("TotalChargeEstimate")] public string TotalChargeEstimate;
            [XmlArrayItem("Charge")] public Charge[] Charges;
        }

        public partial class Charge
        {
            [XmlElement("Type")] public Type Type;
            [XmlElement("Value")] public string Value;
        }

        public partial class Type
        {
            [XmlElement("Code")] public string Code;
            [XmlElement("Desc")] public string Desc;
        }

        public partial class Fault
        {
            [XmlElement("Source")] public string Source;
            [XmlElement("Code")] public string Code;
            [XmlElement("Desc")] public string Desc;
            [XmlElement("Context")] public string Context;
        }
    }
}
