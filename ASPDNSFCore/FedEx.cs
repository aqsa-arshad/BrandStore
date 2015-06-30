// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.IO;
using System.Linq;
using AspDotNetStorefrontCore.RateServiceWebReference;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace AspDotNetStorefrontCore
{
    public partial class RTShipping
    {
        private RateRequest CreateRateRequest(Packages Shipment)
        {
            // Build the RateRequest
            RateRequest request = new RateRequest();
            
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = FedexKey; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = FedexPassword; // Replace "XXX" with the Password
            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = FedexAccountNumber; // Replace "XXX" with client's account number
            request.ClientDetail.MeterNumber = FedexMeter; // Replace "XXX" with client's meter number
            //
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = ""; // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId(); // WSDL version information, value is automatically set from wsdl
            //
            request.ReturnTransitAndCommit = true;
            request.ReturnTransitAndCommitSpecified = true;
            //
            SetShipmentDetails(request);
            //
            SetOrigin(request);
            //
            SetDestination(request, Shipment);
            //
            SetPayment(request);
            //
            SetPackageLineItems(request, Shipment);

			bool includeSmartPost = AppLogic.AppConfigBool("RTShipping.FedEx.SmartPost.Enabled"); ;
			if (includeSmartPost)
			{
				SetSmartPostDetails(request, Shipment);
			}
            return request;
        }

		private SmartPostIndiciaType GetSmartPostIndiciaType(decimal weight)
		{
			SmartPostIndiciaType retType = SmartPostIndiciaType.PARCEL_SELECT;
			//0-1:PRESORTED STANDARD,1.01-69.99:PARCEL SELECT
			string indiciaString = AppLogic.AppConfig("RTShipping.FedEx.SmartPost.IndiciaWeights");
			string[] weightBreaks = indiciaString.Split(',');
			foreach (string weightBreak in weightBreaks)
			{
				string[] splitWeight = weightBreak.Split(':');
				if (splitWeight.Length == 2)
				{
					decimal weightLow = decimal.Zero;
					decimal weightHigh = decimal.Zero;
					string[] weightRanges = splitWeight[0].Split('-');
					if (weightRanges.Length == 2)
					{
						if (decimal.TryParse(weightRanges[0], out weightLow) &&
							decimal.TryParse(weightRanges[1], out weightHigh))
						{
							if (weight >= weightLow && weight <= weightHigh)
							{
								retType = (SmartPostIndiciaType)Enum.Parse(typeof(SmartPostIndiciaType), splitWeight[1]);
							}
						}
					}
				}
			}

			return retType;
		}

		private SmartPostAncillaryEndorsementType? GetSmartPostAncillaryEndorsement()
		{
			string endorsement = AppLogic.AppConfig("RTShipping.FedEx.SmartPost.AncillaryEndorsementType");
			switch (endorsement.ToUpper())
			{
				case "ADDRESS CORRECTION": return SmartPostAncillaryEndorsementType.ADDRESS_CORRECTION;
				case "CARRIER LEAVE IF NO RESPONSE": return SmartPostAncillaryEndorsementType.CARRIER_LEAVE_IF_NO_RESPONSE;
				case "CHANGE SERVICE": return SmartPostAncillaryEndorsementType.CHANGE_SERVICE;
				case "FORWARDING SERVICE": return SmartPostAncillaryEndorsementType.FORWARDING_SERVICE;
				case "RETURN SERVICE": return SmartPostAncillaryEndorsementType.RETURN_SERVICE;
			}
			return null;
		}

		private string GetSmartPostHubId()
		{
			#region HubIds
			//            5303 ATGA Atlanta
			//• 	5281 CHNC Charlotte
			//• 	5602 CIIL Chicago
			//• 	5929 COCA Chino
			//• 	5751 DLTX Dallas
			//• 	5802 DNCO Denver
			//• 	5481 DTMI Detroit
			//• 	5087 EDNJ Edison
			//• 	5431 GCOH Grove City
			//• 	5771 HOTX Houston
			//• 	5465 ININ Indianapolis
			//• 	5648 KCKS Kansas City
			//• 	5902 LACA Los Angeles
			//• 	5254 MAWV Martinsburg
			//• 	5379 METN Memphis
			//• 	5552 MPMN Minneapolis
			//• 	5531 NBWI New Berlin
			//• 	5110 NENY Newburgh
			//• 	5015 NOMA Northborough
			//• 	5327 ORFL Orlando
			//• 	5194 PHPA Philadelphia
			//• 	5854 PHAZ Phoenix
			//• 	5150 PTPA Pittsburgh
			//• 	5958 SACA Sacramento
			//• 	5843 SCUT Salt Lake City
			//• 	5983 SEWA Seattle
			//• 	5631 STMO St. Louis
			#endregion
			return AppLogic.AppConfig("RTShipping.FedEx.SmartPost.HubId");
		}

		private void SetSmartPostDetails(RateRequest request, Packages shipment)
		{
			request.RequestedShipment.SmartPostDetail = new SmartPostShipmentDetail()
			{
				Indicia = GetSmartPostIndiciaType(shipment.Weight),
				IndiciaSpecified = true,
				HubId = GetSmartPostHubId(),
				AncillaryEndorsementSpecified = false,
			};

			var ancillary = GetSmartPostAncillaryEndorsement();
			if (ancillary.HasValue)
			{
				request.RequestedShipment.SmartPostDetail.AncillaryEndorsement = ancillary.Value;
				request.RequestedShipment.SmartPostDetail.AncillaryEndorsementSpecified = true;				 
			}	
		}

		private string SerializeObject<T>(T obj)
		{
			string ret = string.Empty;
			try
			{
				System.IO.StringWriter sw = new System.IO.StringWriter();
				System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
				x.Serialize(sw, obj);
				ret = sw.ToString();
				sw.Close();
			}
			catch (Exception)
			{

			}

			return ret;
		}
        private void SetShipmentDetails(RateRequest request)
        {
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = DateTime.Now; // Shipping date and time
            request.RequestedShipment.ShipTimestampSpecified = true;
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP; //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
            request.RequestedShipment.DropoffTypeSpecified = true;
            request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING;
            request.RequestedShipment.PackagingTypeSpecified = true;
            //
            request.RequestedShipment.RateRequestTypes = new RateRequestType[2];
            request.RequestedShipment.RateRequestTypes[0] = RateRequestType.ACCOUNT;
            request.RequestedShipment.RateRequestTypes[1] = RateRequestType.LIST;
            request.RequestedShipment.PackageDetail = RequestedPackageDetailType.INDIVIDUAL_PACKAGES;
            request.RequestedShipment.PackageDetailSpecified = true;
        }

        private void SetOrigin(RateRequest request)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Address = new AspDotNetStorefrontCore.RateServiceWebReference.Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[1] { OriginAddress };
            request.RequestedShipment.Shipper.Address.City = OriginCity;
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = OriginStateProvince;
            request.RequestedShipment.Shipper.Address.PostalCode = OriginZipPostalCode;
            request.RequestedShipment.Shipper.Address.CountryCode = OriginCountry;
        }

        private void SetDestination(RateRequest request, Packages Shipment)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new AspDotNetStorefrontCore.RateServiceWebReference.Address();
            if (Shipment.DestinationStateProvince.Replace("-", "").Length != 0 && Shipment.DestinationStateProvince != "ZZ")
            {
                request.RequestedShipment.Recipient.Address.StreetLines = new string[1] { Shipment.DestinationAddress1 };
                request.RequestedShipment.Recipient.Address.City = Shipment.DestinationCity;
                request.RequestedShipment.Recipient.Address.StateOrProvinceCode = Shipment.DestinationStateProvince;
            }
            if (Shipment.DestinationCountryCode != String.Empty)
                request.RequestedShipment.Recipient.Address.CountryCode = Shipment.DestinationCountryCode;
            else
                request.RequestedShipment.Recipient.Address.CountryCode = OriginCountry;

            if (Shipment.DestinationCountryCode.Equals("US", StringComparison.InvariantCultureIgnoreCase) && Shipment.DestinationZipPostalCode.Length > 5)
                request.RequestedShipment.Recipient.Address.PostalCode = Shipment.DestinationZipPostalCode.Substring(0, 5);
            else
                request.RequestedShipment.Recipient.Address.PostalCode = Shipment.DestinationZipPostalCode;

			if (Shipment.DestinationResidenceType == ResidenceTypes.Residential)
			{
				request.RequestedShipment.Recipient.Address.Residential = true;
				request.RequestedShipment.Recipient.Address.ResidentialSpecified = true;
			}
        }

        private void SetPayment(RateRequest request)
        {
            request.RequestedShipment.ShippingChargesPayment = new Payment();
            request.RequestedShipment.ShippingChargesPayment.PaymentType = PaymentType.SENDER; // Payment options are RECIPIENT, SENDER, THIRD_PARTY
            request.RequestedShipment.ShippingChargesPayment.PaymentTypeSpecified = true;
            request.RequestedShipment.ShippingChargesPayment.Payor = new Payor();
            request.RequestedShipment.ShippingChargesPayment.Payor.AccountNumber = FedexAccountNumber; // Replace "XXX" with client's account number
            request.RequestedShipment.ShippingChargesPayment.Payor.CountryCode = OriginCountry;
        }

        private void SetPackageLineItems(RateRequest request, Packages Shipment)
        {

            // ------------------------------------------
            // Passing individual pieces rate request
            // ------------------------------------------
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[Shipment.Count];
            int packagecount = 0;
            foreach (Package package in Shipment)
            {
                //
                request.RequestedShipment.RequestedPackageLineItems[packagecount] = new RequestedPackageLineItem();
                request.RequestedShipment.RequestedPackageLineItems[packagecount].SequenceNumber = (packagecount + 1).ToString(); // package sequence number
                //
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Weight = new Weight(); // package weight
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions = new Dimensions(); // package dimensions

                if (AppLogic.AppConfig("RTShipping.WeightUnits").Trim().Equals("LBS", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].Weight.Units = WeightUnits.LB;
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Units = LinearUnits.IN;
                }
                else
                {
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].Weight.Units = WeightUnits.KG;
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Units = LinearUnits.CM;
                }

                request.RequestedShipment.RequestedPackageLineItems[packagecount].Weight.Value = package.Weight;
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Length = Math.Round(package.Length, 0, MidpointRounding.AwayFromZero).ToString();
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Width = Math.Round(package.Width, 0, MidpointRounding.AwayFromZero).ToString();
                request.RequestedShipment.RequestedPackageLineItems[packagecount].Dimensions.Height = Math.Round(package.Height, 0, MidpointRounding.AwayFromZero).ToString();

                if (package.Insured)
                {
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].InsuredValue = new Money(); // insured value
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].InsuredValue.Amount = package.InsuredValue;
                    request.RequestedShipment.RequestedPackageLineItems[packagecount].InsuredValue.Currency = "USD";
                }
                packagecount += 1;
            }
            request.RequestedShipment.PackageCount = packagecount.ToString();
        }

        private void FedExGetRates(Packages Shipment, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)	// Retrieves FedEx rates
        {
            RTShipRequest = string.Empty;
            RTShipResponse = string.Empty; 

            Hashtable htRates = new Hashtable();
            RateRequest request = CreateRateRequest(Shipment);
            RateService service = new RateService(); // Initialize the service
            service.Url = this.FedexServer;


            try
            {
                // Call the web service passing in a RateRequest and returning a RateReply
                RateReply reply = service.getRates(request);

                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING) // check if the call was successful
                {
                    //create list of available services

                    for (int i = 0; i < reply.RateReplyDetails.Length; i++)
                    {
                        RateReplyDetail rateReplyDetail = reply.RateReplyDetails[i];
                        RatedShipmentDetail ratedShipmentDetail = rateReplyDetail.RatedShipmentDetails[1];

                        decimal totalCharges = ratedShipmentDetail.ShipmentRateDetail.TotalNetCharge.Amount;
                        if (MarkupPercent != System.Decimal.Zero)
                        {
                            totalCharges = Decimal.Round(totalCharges * (1.00M + (MarkupPercent / 100.0M)), 2, MidpointRounding.AwayFromZero);
                        }

                        decimal vat = Decimal.Round(totalCharges * ShippingTaxRate, 2, MidpointRounding.AwayFromZero);
                        string rateName = rateReplyDetail.ServiceType.ToString();
                        if (htRates.ContainsKey(rateName))
                        {
                            // Get the sum of the rate(s)
                            decimal myTempCharge = Localization.ParseUSDecimal(htRates[rateName].ToString().Split('|')[0]);
                            totalCharges += myTempCharge;
                            vat += Localization.ParseUSDecimal(htRates[rateName].ToString().Split('|')[1]);

                            // Remove the old value & add the new
                            htRates.Remove(rateName);
                        }

                        // Temporarily add rate to hash table
                        htRates.Add(rateName, Localization.CurrencyStringForDBWithoutExchangeRate(totalCharges) + "|" + Localization.CurrencyStringForDBWithoutExchangeRate(vat));

                    }
                }
                else
                {
                    ratesText.Add("Error: Call Not Successful");
                    ratesValues.Add("Error: Call Not Successful");
                }

                RTShipRequest = Serialize(request);
                RTShipResponse = Serialize(reply); 
            }
            catch (SoapException e)
            {
                ratesText.Add("Error: " + e.Detail.InnerText);
                ratesValues.Add("Error: " + e.Detail.InnerText);
            }
            catch (Exception e)
            {
                ratesText.Add("Error: " + e.Message);
                ratesValues.Add("Error: " + e.Message);
            }



            // Add rates from hastable into array(s)
            IDictionaryEnumerator myEnumerator = htRates.GetEnumerator();
            while (myEnumerator.MoveNext())
            {
                Decimal tmp_rate = Localization.ParseUSDecimal(myEnumerator.Value.ToString().Substring(0, myEnumerator.Value.ToString().IndexOf("|"))) + ExtraFee;
                Decimal tmp_vat = Localization.ParseUSDecimal(myEnumerator.Value.ToString().Substring(myEnumerator.Value.ToString().LastIndexOf("|") + 1));
                String rateText = tmp_rate.ToString() + "|" + tmp_vat.ToString();
                ratesText.Add(myEnumerator.Key.ToString() + " $" + rateText);
                ratesValues.Add(myEnumerator.Key.ToString() + "|" + rateText);
            }
        }
        
        public string FedExGetCodeDescription(string code)
        {
            string result = string.Empty;
            switch (code.Replace("_", ""))
            {
                case "PRIORITYOVERNIGHT":
                    result = "Priority";
                    break;
                case "FEDEX2DAY":
                    result = "2nd Day";
                    break;
                case "STANDARDOVERNIGHT":
                    result = "Standard Overnight";
                    break;
                case "FIRSTOVERNIGHT":
                    result = "First Overnight";
                    break;
                case "FEDEXEXPRESSSAVER":
                    result = "Express Saver";
                    break;
                case "FEDEX1DAYFREIGHT":
                    result = "Overnight Freight";
                    break;
                case "FEDEX2DAYFREIGHT":
                    result = "2nd Day Freight";
                    break;
                case "FEDEX3DAYFREIGHT":
                    result = "Express Saver Freight";
                    break;
                case "GROUNDHOMEDELIVERY":
                    result = "Home Delivery";
                    break;
                case "FEDEXGROUND":
                    result = "Ground Service";
                    break;
                case "INTERNATIONALPRIORITY":
                    result = "International Priority";
                    break;
                case "INTERNATIONALECONOMY":
                    result = "International Economy";
                    break;
                case "INTERNATIONALPRIORITYFREIGHT":
                    result = "International Priority Freight";
                    break;
				case "SMARTPOST":
					result = "Smart Post";
					break;
            }
            return result;
        }

        private void FedExGetRates(Shipments AllShipments, out string RTShipRequest, out string RTShipResponse, decimal ExtraFee, Decimal MarkupPercent, decimal ShipmentValue, decimal ShippingTaxRate)	// Retrieves FedEx rates
        {
            RTShipRequest = string.Empty;
            RTShipResponse = string.Empty;

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
                foreach (Package p in Shipment)
                {
                    if (p.IsFreeShipping)
                    {
                        HasFreeItems = true;
                        break;
                    }
                }

                RateRequest request = CreateRateRequest(Shipment);
                RateService service = new RateService(); // Initialize the service
                service.Url = this.FedexServer;
                try
                {
                    RateReply reply = service.getRates(request);

					string rateRequest = SerializeObject(request);
					string rateReply = SerializeObject(reply);
					System.Diagnostics.Debug.WriteLine(rateRequest);
					System.Diagnostics.Debug.WriteLine(rateReply);

                    if (reply.RateReplyDetails != null && (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING))// check if the call was successful
                    {
                        //create list of available services

                        for (int i = 0; i < reply.RateReplyDetails.Length; i++)
                        {
                            RateReplyDetail rateReplyDetail = reply.RateReplyDetails[i];

							// listRatedShipmentDetail is currently unused - could be used in the future to support list or pro-rated rates
							RatedShipmentDetail listRatedShipmentDetail = rateReplyDetail.RatedShipmentDetails
								.FirstOrDefault(rsd => rsd.ShipmentRateDetail.RateType == ReturnedRateType.PAYOR_LIST_PACKAGE ||
									rsd.ShipmentRateDetail.RateType == ReturnedRateType.RATED_LIST_PACKAGE ||
									rsd.ShipmentRateDetail.RateType == ReturnedRateType.PAYOR_LIST_SHIPMENT ||
									rsd.ShipmentRateDetail.RateType == ReturnedRateType.RATED_LIST_SHIPMENT);

							RatedShipmentDetail ratedShipmentDetail = rateReplyDetail.RatedShipmentDetails
								.FirstOrDefault(rsd => rsd.ShipmentRateDetail.RateType == ReturnedRateType.PAYOR_ACCOUNT_PACKAGE ||
									rsd.ShipmentRateDetail.RateType == ReturnedRateType.PAYOR_ACCOUNT_SHIPMENT ||
									rsd.ShipmentRateDetail.RateType == ReturnedRateType.RATED_ACCOUNT_PACKAGE ||
									rsd.ShipmentRateDetail.RateType == ReturnedRateType.RATED_ACCOUNT_SHIPMENT);

                            string rateName = "FedEx " + FedExGetCodeDescription(rateReplyDetail.ServiceType.ToString());

                            if (ShippingMethodIsAllowed(rateName, "FEDEX"))
                            {
                                decimal totalCharges = ratedShipmentDetail.ShipmentRateDetail.TotalNetCharge.Amount;

                                //multiply the returned rate by the quantity in the package to avoid calling
                                //more than necessary if there were multiple IsShipSeparately items
                                //ordered.  If there weren't, Shipment.PackageCount is 1 and the rate is normal
                                totalCharges = totalCharges * Shipment.PackageCount;

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
                    else
                    {
                        RTShipResponse = "Error: Call Not Successful " + reply.Notifications[0].Message;
                    }

                    RTShipRequest = Serialize(request);
                    RTShipResponse = Serialize(reply); 

                }
                catch (SoapException e)
                {
                    RTShipResponse = "FedEx Error: " + e.Detail.InnerXml;
                }
                catch (Exception e)
                {
                    RTShipResponse = "FedEx Error: " + e.InnerException.Message;
                }
            }

        }

        private string Serialize(RateReply reply)
        {
            XmlSerializer serRateReply = new XmlSerializer(typeof(AspDotNetStorefrontCore.RateServiceWebReference.RateReply));
            StringWriter swRateReply = new StringWriter();

            serRateReply.Serialize(swRateReply, reply);
            return swRateReply.ToString();
        }

        private string Serialize(RateRequest request)
        {
            XmlSerializer serRateRequest = new XmlSerializer(typeof(AspDotNetStorefrontCore.RateServiceWebReference.RateRequest));
            StringWriter swRateRequest = new StringWriter();

            serRateRequest.Serialize(swRateRequest, request);
            return swRateRequest.ToString(); 
        }
    }
}
