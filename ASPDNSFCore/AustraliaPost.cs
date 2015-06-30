// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
    public partial class RTShipping
    {
        // legacy overload for AjaxShipping
        private void AusPostGetRates(Packages Shipment, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)
        {
            Shipments AllShipments = new Shipments();
            AllShipments.AddPackages(Shipment);

            AusPostGetRates(AllShipments, out RTShipRequest, out RTShipResponse, ExtraFee, MarkupPercent, ShipmentValue, ShippingTaxRate);

            foreach (ShipMethod method in SM)
            {
                ratesText.Add(method.ServiceName + " $" + method.ServiceRate.ToString());
                ratesValues.Add(method.ServiceName + "|" + method.ServiceRate.ToString() + "|" + method.VatRate.ToString());
            }
        }

        private void AusPostGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)
        {
            RTShipRequest = String.Empty;
            RTShipResponse = String.Empty;

            if (!AppLogic.AppConfig("Localization.StoreCurrency").Trim().Equals("aud", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Localization.StoreCurrency == AUD required to use Australia Post as a carrier.");
            }

            if (!AppLogic.AppConfig("Localization.WeightUnits").Equals("kg", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Localization.WeightUnits == kg required to use Australia Post as a carrier.");
            }

            // is weight within shippable limit?
            if (ShipmentWeight > AppLogic.AppConfigUSDecimal("RTShipping.AusPost.MaxWeight"))
            {
                SM.ErrorMsg = "Australia Post " + AppLogic.AppConfig("RTShipping.CallForShippingPrompt");
                return;
            }

            foreach (Packages Shipment in AllShipments)
            {
                HasFreeItems = false;
                PackageQuantity = 1;

                foreach (Package p in Shipment)
                {
                    if (p.IsFreeShipping)
                        HasFreeItems = true;

                    if (p.IsShipSeparately)
                        PackageQuantity = p.Quantity;

                    // initialize rate requests
                    ausPost.RateRequest rateRequests = new ausPost.RateRequest();

                    // get list of service classes
                    string AusPostServices;
                    if (Shipment.DestinationCountryCode == "AU")
                        AusPostServices = AppLogic.AppConfig("RTShipping.AusPost.DomesticServices");
                    else
                        AusPostServices = AppLogic.AppConfig("RTShipping.AusPost.IntlServices");

                    // create individual requests, one for each service
                    foreach (string service in AusPostServices.Split(','))
                    {
                        ausPost.Request request = new ausPost.Request();

                        // dimensions (all specified in mm)
                        if (p.Length + p.Width + p.Height == 0)  // if package has no dimensions, we use default
                        {
                            string dimensions = AppLogic.AppConfig("RTShipping.AusPost.DefaultPackageSize");
                            try
                            {
                                p.Width = Convert.ToDecimal(dimensions.Split('x')[0].Trim());
                                p.Height = Convert.ToDecimal(dimensions.Split('x')[1].Trim());
                                p.Length = Convert.ToDecimal(dimensions.Split('x')[2].Trim());
                            }
                            catch (FormatException e)
                            {
                                throw new Exception("Check the RTShipping.AusPost.DefaultPackageSize AppConfig. " +
                                    "Must be three dimensions (in cm) separated by 'x'. Example: 15x15x15. " + e.Message);
                            }
                        }
                        request.Length = Convert.ToInt32(Math.Ceiling(p.Length * 10)); // convert all from cm to mm
                        request.Width = Convert.ToInt32(Math.Ceiling(p.Width * 10));
                        request.Height = Convert.ToInt32(Math.Ceiling(p.Height * 10));

                        request.Weight = Convert.ToInt32(Math.Ceiling(p.Weight * 1000)); // convert from kg to g
                        request.Quantity = PackageQuantity;

                        // shipping addresses
                        request.Pickup_Postcode = OriginZipPostalCode.PadRight(4).Substring(0, 4);
                        request.Country = Shipment.DestinationCountryCode;
                        request.Destination_Postcode = Shipment.DestinationZipPostalCode.PadRight(4).Substring(0, 4);

                        // Service Type
                        try
                        {
                            request.Service_Type =
                                (ausPost.Request.ServiceType)Enum.Parse(typeof(ausPost.Request.ServiceType), service.Split(';')[0], true);
                        }
                        catch (ArgumentException e)
                        {
                            if (Shipment.DestinationCountryCode == "AU")
                                throw new Exception("Check the RTShipping.AusPost.DomesticServices AppConfig. " +
                                    "Legal values are STANDARD or EXPRESS, followed by a semi-colon and Name. " + e.Message);
                            else
                                throw new Exception("Check the RTShipping.AusPost.IntlServices AppConfig. " +
                                    "Legal values are AIR, SEA, ECI_D, ECI_M, or EPI, followed by a semi-colon and Name. " + e.Message);
                        }

                        // convert rateRequests into a URL
                        string req = Convert.ToString(request);

                        // Send rate request to AusPost server
                        HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(req);

                        // get the response from AusPost
                        WebResponse webResponse;
                        string resp;
                        try
                        {
                            webResponse = webRequest.GetResponse();
                        }
                        catch (WebException e)  // could not receive a response from AusPost endpoint
                        {
                            RTShipResponse += "No response from Australia Post Server: " + e.Message;
                            return;
                        }

                        using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                        {
                            resp = sr.ReadToEnd();
                            sr.Close();
                        }
                        webResponse.Close();

                        // Convert the response into a Response object
                        ausPost.Response response;

                        try
                        {
                            response = (ausPost.Response) TypeDescriptor.GetConverter(typeof(ausPost.Response)).ConvertFromString(resp);
                        }
                        catch (InvalidOperationException e)  // invalid or no reply received from AusPost
                        {
                            RTShipResponse += "Could not parse response from Australia Post server: " + e.Message
                                + " Response received: " + resp;
                            return;
                        }

                        // Check the response object for an error
                        if (response.Err_msg != "OK")
                        {
                            RTShipResponse += "Austalia Post Error: " + response.Err_msg + Environment.NewLine;
                            continue;
                        }

                        // we have a good estimate
                        decimal total = response.Charge;

                        // ignore zero-cost methods, and methods not allowed
                        if ((total == 0 && AppLogic.AppConfigBool("FilterOutShippingMethodsThatHave0Cost")) || !ShippingMethodIsAllowed(service.Split(';')[1], string.Empty))
                            continue;

                        total = total * (1.00M + (MarkupPercent / 100.0M));
                        decimal vat = Decimal.Round(total * ShippingTaxRate);

                        // add shipping method
                        if (!SM.MethodExists(service.Split(';')[1]))
                        {
                            ShipMethod s_method = new ShipMethod();
                            s_method.Carrier = "Australia Post";
                            s_method.ServiceName = service.Split(';')[1];
                            s_method.ServiceRate = total;
                            s_method.VatRate = vat;

                            if (HasFreeItems)
                                s_method.FreeItemsRate = total;

                            SM.AddMethod(s_method);
                        }
                        else
                        {
                            int IndexOf = SM.GetIndex(service.Split(';')[1]);
                            ShipMethod s_method = SM[IndexOf];
                            s_method.ServiceRate += total;
                            s_method.VatRate += vat;

                            if (HasFreeItems)
                                s_method.FreeItemsRate += total;

                            SM[IndexOf] = s_method;
                        }

                        RTShipRequest += req + Environment.NewLine;
                        RTShipResponse += resp.Replace('\n', ' ').Replace('\r', ' ') + Environment.NewLine;
                    }
                }
            }

            // Handling fee should only be added per shipping address not per package
            // let's just compute it here after we've gone through all the packages.
            // Also, since we can't be sure about the ordering of the method call here
            // and that the collection SM includes shipping methods from all possible carriers
            // we'll need to filter out the methods per this carrier to avoid side effects on the main collection
            foreach (ShipMethod shipMethod in SM.PerCarrier("Australia Post"))
            {
                shipMethod.ServiceRate += ExtraFee;
            }
        }
    }

    namespace ausPost
    {
        public partial class RateRequest
        {
            public Request[] Shipment;
        }

        public partial class Request : IConvertible
        {
            public string Pickup_Postcode;
            public string Destination_Postcode;
            public string Country;
            public int Weight;
            public ServiceType Service_Type;
            public int Length;
            public int Width;
            public int Height;
            public int Quantity;

            public enum ServiceType { STANDARD,EXPRESS,SEA,AIR,ECI_D,ECI_M,EPI };

            string IConvertible.ToString(IFormatProvider provider)
            {
                return "http://drc.edeliver.com.au/ratecalc.asp" +
                    "?Height=" + Height.ToString() +
                    "&Length=" + Length.ToString() +
                    "&Width=" + Width.ToString() +
                    "&Weight=" + Weight.ToString() +
                    "&Pickup_Postcode=" + Pickup_Postcode +
                    "&Destination_Postcode=" + Destination_Postcode +
                    "&Country=" + Country +
                    "&Service_Type=" + Service_Type.ToString() +
                    "&Quantity=" + Quantity.ToString();
                //throw new Exception("The method or operation is not implemented.");
            }

            public TypeCode GetTypeCode()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public double ToDouble(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public int ToInt32(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            [CLSCompliant(false)]
            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            [CLSCompliant(false)]
            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            [CLSCompliant(false)]
            public uint ToUInt32(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            [CLSCompliant(false)]
            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        [TypeConverter(typeof(ResponseConverter))]
        public class Response
        {
            public Response(decimal RespCharge, int RespDays, string RespErr_msg)
            {
                Charge = RespCharge;
                Days = RespDays;
                Err_msg = RespErr_msg;
            }

            public decimal Charge;
            public int Days;
            public string Err_msg;
        }

        public class ResponseConverter : TypeConverter
        {
           public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
           {
                if (sourceType == typeof(string))
                    return true;

               return base.CanConvertFrom(context, sourceType);
           }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string resp = ((string)value).Replace("\r", "").Replace("charge=", "").Replace("days=", "").Replace("err_msg=", "");
                    string[] v = resp.Split('\n');
                    return new Response(decimal.Parse(v[0]), int.Parse(v[1]), v[2]);
                }
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}

