// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
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
        private void CanadaPostGetRates(Packages Shipment, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)
        {
            Shipments AllShipments = new Shipments();
            AllShipments.AddPackages(Shipment);

            CanadaPostGetRates(AllShipments, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);

            foreach (ShipMethod method in SM)
            {
                ratesText.Add(method.ServiceName + " $" + method.ServiceRate.ToString());
                ratesValues.Add(method.ServiceName + "|" + method.ServiceRate.ToString() + "|" + method.VatRate.ToString());
            }
        }

        private void CanadaPostGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)
        {
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;
            System.Collections.Specialized.NameValueCollection RatesList = new System.Collections.Specialized.NameValueCollection();

            if (!AppLogic.AppConfig("Localization.StoreCurrency").Trim().Equals("cad", StringComparison.InvariantCultureIgnoreCase))
            {
                RTShipResponse += "Localization.StoreCurrency == CAD required to use Canada Post as a carrier.";
                return;
            }

            if (!AppLogic.AppConfig("Localization.WeightUnits").Equals("kg", StringComparison.InvariantCultureIgnoreCase))
            {
                RTShipResponse += "Localization.WeightUnits == kg required to use Canada Post as a carrier.";
                return;
            }

            foreach (Packages Shipment in AllShipments)
            {
                HasFreeItems = false;
                PackageQuantity = 1;

                if (Shipment.Weight > AppLogic.AppConfigUSDecimal("RTShipping.CanadaPost.MaxWeight"))
                {
                    SM.ErrorMsg = "Canada Post " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                    return;
                }

                // create a rate request
                CanadaPost.ratesAndServicesRequest rateRequest = new CanadaPost.ratesAndServicesRequest();
                rateRequest.merchantCPCID = AppLogic.AppConfig("RTShipping.CanadaPost.MerchantID");  // Canada Post merchant credentials

                // ship-to address
                rateRequest.city = Shipment.DestinationCity;
                rateRequest.provOrState = Shipment.DestinationStateProvince;
                rateRequest.country = Shipment.DestinationCountryCode;
                rateRequest.postalCode = Shipment.DestinationZipPostalCode;

                // create one lineitem request per package in shipment
                rateRequest.lineItems.item = new CanadaPost.item[Shipment.Count];

                int packageIndex = 0;
                foreach (Package p in Shipment)
                {
                    if (p.IsFreeShipping) HasFreeItems = true;
                    if (p.IsShipSeparately) PackageQuantity = p.Quantity;

                    // shipment details
                    rateRequest.itemsPrice = p.InsuredValue.ToString("#####.00");
                    rateRequest.lineItems.item[packageIndex] = new CanadaPost.item();
                    rateRequest.lineItems.item[packageIndex].weight = p.Weight.ToString("####.0");

                    // dimensions
                    if (p.Length + p.Width + p.Height == 0)  // if package has no dimensions, we use default
                    {
                        string dimensions = AppLogic.AppConfig("RTShipping.CanadaPost.DefaultPackageSize");
                        p.Width = Convert.ToDecimal(dimensions.Split('x')[0].Trim());
                        p.Height = Convert.ToDecimal(dimensions.Split('x')[1].Trim());
                        p.Length = Convert.ToDecimal(dimensions.Split('x')[2].Trim());
                    }
                    rateRequest.lineItems.item[packageIndex].length = p.Length.ToString("###.0");
                    rateRequest.lineItems.item[packageIndex].width = p.Width.ToString("###.0");
                    rateRequest.lineItems.item[packageIndex].height = p.Height.ToString("###.0");

                    packageIndex++;
                }

                // initialize eParcel request
                CanadaPost.eparcel request = new CanadaPost.eparcel();

                // choose language for reply text
                request.language = AppLogic.AppConfig("RTShipping.CanadaPost.Language").Trim();
                if (request.language.Equals("auto", StringComparison.InvariantCultureIgnoreCase))  // set the language based on the customers locale
                {
                    Customer ThisCustomer = ((AspDotNetStorefrontPrincipal) System.Web.HttpContext.Current.User).ThisCustomer;
                    if (ThisCustomer.LocaleSetting.Trim().StartsWith("fr", StringComparison.InvariantCultureIgnoreCase))
                        request.language = "fr";
                    else
                        request.language = "en";
                }

                request.Items = new CanadaPost.ratesAndServicesRequest[1];
                request.Items[0] = rateRequest;

                // serialize eParcel request class
                XmlSerializer serRequest = new XmlSerializer(typeof(CanadaPost.eparcel));
                StringWriter swRequest = new StringWriter();

                serRequest.Serialize(swRequest, request);
                string req = swRequest.ToString().Replace(@" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "");

                // open a TCP socket with Canada Post server
                Socket socCanadaPost = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint remoteEndPoint;

                socCanadaPost.ReceiveTimeout = 10000; // milliseconds to wait for a response

                try
                {
                    remoteEndPoint = new IPEndPoint(Dns.GetHostAddresses(AppLogic.AppConfig("RTShipping.CanadaPost.Server"))[0],
                        AppLogic.AppConfigNativeInt("RTShipping.CanadaPost.ServerPort"));
                    socCanadaPost.Connect(remoteEndPoint);
                }
                catch (SocketException e)
                {
                    RTShipResponse += "Tried to reach Canada Post Server (" + AppLogic.AppConfig("RTShipping.CanadaPost.Server") +
                        ":" + AppLogic.AppConfigNativeInt("RTShipping.CanadaPost.ServerPort") + "): " + e.Message;
                    return;
                }


                // send request to Canada Post
                byte[] data = System.Text.Encoding.ASCII.GetBytes(req);
                socCanadaPost.Send(data);

                //receive response from Canada Post
                string resp = String.Empty;
                byte[] buffer = new byte[8192];
                int iRx = 0;

                while (!resp.Contains("<!--END_OF_EPARCEL-->"))
                {
                    try
                    {
                        iRx += socCanadaPost.Receive(buffer, iRx, 8192-iRx, SocketFlags.None);
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode == SocketError.TimedOut)
                            break;
                        else
                            throw e;
                    }
                    resp = new string((System.Text.Encoding.UTF8.GetChars(buffer, 0, iRx)));  // decode byte array to string
                }

                // close socket
                socCanadaPost.Close();

                // create an eParcel response class
                CanadaPost.eparcel response = new CanadaPost.eparcel();

                // deserialize the xml response into the eParcel response
                XmlSerializer serResponse = new XmlSerializer(typeof(CanadaPost.eparcel));
                StringReader srResponse = new StringReader(resp);

                try
                {
                    response = (CanadaPost.eparcel) serResponse.Deserialize(srResponse);
                }
                catch (InvalidOperationException e)  // invalid xml, or no reply received from Canada Post
                {
                    RTShipResponse += "Canada Post error: Could not parse response from Canada Post server: " + e.Message
                        + " Response received: " + resp;
                    return;
                }
                
                srResponse.Close();

                // Check the response object for Faults
                if (response.Items[0] is CanadaPost.error)
                {
                    CanadaPost.error respError = (CanadaPost.error)response.Items[0];
                    RTShipResponse += respError.statusMessage[0];
                    return;
                }

                // Check the response object for Ratings
                if (!(response.Items[0] is CanadaPost.ratesAndServicesResponse))
                {
                    RTShipResponse += "Canada Post Error: No rating responses returned from Canada Post";
                    return;
                }

                // no faults, so extract rate information
                CanadaPost.ratesAndServicesResponse ratesResp = (CanadaPost.ratesAndServicesResponse)response.Items[0];
                foreach (CanadaPost.product product in ratesResp.product)
                {
                    decimal total = Localization.ParseUSDecimal(product.rate);

                    // ignore zero-cost methods, and methods not allowed
                    if (total != 0 && ShippingMethodIsAllowed(product.name, "Canada Post"))
                    {
                        total = total * PackageQuantity * (1.00M + (MarkupPercent / 100.0M));

                        decimal vat = Decimal.Round(total * ShippingTaxRate);

                        if (!SM.MethodExists(product.name))
                        {
                            ShipMethod s_method = new ShipMethod();
                            s_method.Carrier = "Canada Post";
                            s_method.ServiceName = product.name;
                            s_method.ServiceRate = total;
                            s_method.VatRate = vat;

                            if (HasFreeItems) s_method.FreeItemsRate = total;

                            SM.AddMethod(s_method);
                        }
                        else
                        {
                            int IndexOf = SM.GetIndex(product.name);
                            ShipMethod s_method = SM[IndexOf];
                            s_method.ServiceRate += total;
                            s_method.VatRate += vat;

                            if (HasFreeItems) s_method.FreeItemsRate += total;

                            SM[IndexOf] = s_method;
                        }
                    }
                }
                RTShipRequest += req; // stash request & response for this shipment
                RTShipResponse += resp;
            }

            // Handling fee should only be added per shipping address not per package
            // let's just compute it here after we've gone through all the packages.
            // Also, since we can't be sure about the ordering of the method call here
            // and that the collection SM includes shipping methods from all possible carriers
            // we'll need to filter out the methods per this carrier to avoid side effects on the main collection
            foreach (ShipMethod shipMethod in SM.PerCarrier("Canada Post"))
            {
                shipMethod.ServiceRate += ExtraFee;
            }
        }
    }

    namespace CanadaPost
    {
        [XmlRoot("eparcel")] public partial class eparcel
        {
            [XmlElement("language")] public string language;
            [XmlElement("ratesAndServicesRequest", typeof(ratesAndServicesRequest))]
            [XmlElement("ratesAndServicesResponse", typeof(ratesAndServicesResponse))]
            [XmlElement("error", typeof(CanadaPost.error))]
            public object[] Items;
        }

        [XmlRoot("ratesAndServicesRequest")] public partial class ratesAndServicesRequest
        {
            [XmlElement("merchantCPCID")] public string merchantCPCID;
            [XmlElement("fromPostalCode")] public string fromPostalCode;
            [XmlElement("turnAroundTime")] public string turnAroundTime;
            [XmlElement("itemsPrice")] public string itemsPrice;
            [XmlElement("lineItems")] public lineItems lineItems  = new lineItems();
            [XmlElement("city")] public string city;
            [XmlElement("provOrState")] public string provOrState;
            [XmlElement("country")] public string country;
            [XmlElement("postalCode")] public string postalCode;
        }

        [XmlRoot("ratesAndServicesResponse")] public partial class ratesAndServicesResponse
        {
            [XmlElement("statusCode")] public string statusCode;
            [XmlElement("statusMessage")] public string[] statusMessage;
            [XmlElement("requestID")] public string requestID;
            [XmlElement("handling")] public string handling;
            [XmlElement("language")] public string language;
            [XmlElement("product")] public product[] product;
            [XmlElement("packing")] public packing[] packing;
            [XmlElement("emptySpace")] public emptySpace[] emptySpace;
            [XmlElement("shippingOptions")] public shippingOptions shippingOptions;
            [XmlElement("comment")] public string comment;
            [XmlElement("nearestPostalOutlet")] public nearestPostalOutlet[] nearestPostalOutlet;
        }

        [XmlRoot("error")] public partial class error
        {
            [XmlElement("statusCode")] public string statusCode;
            [XmlElement("statusMessage")] public string[] statusMessage;
        }

        [XmlRoot("lineItems")] public partial class lineItems
        {
            [XmlElement("item")] public item[] item;
        }

        [XmlRoot("item")] public partial class item
        {
            [XmlElement("quantity")] public string quantity = "1";
            [XmlElement("weight")] public string weight;
            [XmlElement("length")] public string length;
            [XmlElement("width")] public string width;
            [XmlElement("height")] public string height;
            [XmlElement("description")] public string description = "shipment";
            [XmlElement("imageURL")] public string imageURL;
            [XmlElement("readyToShip")] public string readyToShip = "1";
        }

        [XmlRoot("product")] public partial class product
        {
            [XmlAttribute("id")] public string id;
            [XmlAttribute("sequence")] public string sequence;
            [XmlElement("name")] public string name;
            [XmlElement("rate")] public string rate;
            [XmlElement("shippingDate")] public string shippingDate;
            [XmlElement("deliveryDate")] public string deliveryDate;
            [XmlElement("deliveryDayOfWeek")] public string deliveryDayOfWeek;
            [XmlElement("nextDayAM")] public string nextDayAM;
            [XmlElement("packingID")] public string packingID;
        }

        [XmlRoot("packing")] public partial class packing
        {
            [XmlElement("packingID")] public string packingID;
            [XmlElement("box")] public box[] box;
        }

        [XmlRoot("box")] public partial class box
        {
            [XmlElement("name")] public string name;
            [XmlElement("weight")] public string weight;
            [XmlElement("expediterWeight")] public string expediterWeight;
            [XmlElement("length")] public string length;
            [XmlElement("width")] public string width;
            [XmlElement("height")] public string height;
            [XmlElement("packedItem")] public packedItem[] packedItem;
        }

        [XmlRoot("packedItem")] public partial class packedItem
        {
            [XmlElement("quantity")] public string quantity;
            [XmlElement("description")] public string description;
        }

        [XmlRoot("emptySpace")] public partial class emptySpace
        {
            [XmlElement("length")] public string length;
            [XmlElement("width")] public string width;
            [XmlElement("height")] public string height;
            [XmlElement("weight")] public string weight;
        }

        [XmlRoot("shippingOptions")] public partial class shippingOptions
        {
            [XmlElement("insurance")] public string insurance;
            [XmlElement("deliveryConfirmation")] public string deliveryConfirmation;
            [XmlElement("signature")] public string signature;
        }

        [XmlRoot("nearestPostalOutlet")] public partial class nearestPostalOutlet
        {
            [XmlElement("postalOutletSequenceNo")] public string postalOutletSequenceNo;
            [XmlElement("distance")] public string distance;
            [XmlElement("outletName")] public string outletName;
            [XmlElement("businessName")] public string businessName;
            [XmlElement("postalAddress")] public postalAddress postalAddress;
            [XmlElement("phoneNumber")] public string phoneNumber;
            [XmlElement("businessHours")] public businessHours[] businessHours;
        }

        [XmlRoot("postalAddress")] public partial class postalAddress
        {
            [XmlElement("addressLine")] public string[] addressLine;
            [XmlElement("postalCode")] public string postalCode;
            [XmlElement("municipality")] public string municipality;
        }

        [XmlRoot("businessHours")] public partial class businessHours
        {
            [XmlElement("dayId")] public string dayId;
            [XmlElement("dayOfWeek")] public string dayOfWeek;
            [XmlElement("time")] public string time;
        }
    }
}
