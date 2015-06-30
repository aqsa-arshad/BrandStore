// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Xml;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for AddressValidation
    /// </summary>
    public partial class AddressValidation
    {

        /// <summary>
        /// Validate Address using US Postal Service API.
        /// </summary>
        /// <param name="EnteredAddress">The address as entered by a customer</param>
        /// <param name="ResultAddress">The resulting validated address</param>
        /// <returns>String,
        /// ro_OK => ResultAddress = EnteredAddress proceed with no further user review,
        /// 'some message' => address requires edit or verification by customer
        /// </returns>
        public String uspsValidate(Address EnteredAddress, out Address ResultAddress)
        {
            string result = AppLogic.ro_OK;
            ResultAddress = new Address();
            ResultAddress.LoadFromDB(EnteredAddress.AddressID);

            if (EnteredAddress.Country != "United States")
            { // USPS doesn't like other contries
                return AppLogic.ro_OK;
            }

            // Build USPS request XML
            string USPSRequest = "API=Verify&Xml=<AddressValidateRequest USERID=\"{0}\">"
                + "<Address ID=\"0\">"
                + "<Address1>" + EnteredAddress.Address2 + "</Address1>"
                + "<Address2>" + EnteredAddress.Address1 + "</Address2>"
                + "<City>" + EnteredAddress.City + "</City>"
                + "<State>" + EnteredAddress.State + "</State>"
                + "<Zip5>" + EnteredAddress.Zip + "</Zip5>"
                + "<Zip4></Zip4>"
                + "</Address>"
                + "</AddressValidateRequest>";

            // Replace userid with config value
            USPSRequest = string.Format(USPSRequest, AppLogic.AppConfig("VerifyAddressesProvider.USPS.UserID"));

            // Send request & capture response
            // Possible USPS server values:
            //        http://production.shippingapis.com/shippingapi.dll 
            //        http://testing.shippingapis.com/ShippingAPITest.dll
            string received = XmlCommon.GETandReceiveData(USPSRequest, AppLogic.AppConfig("VerifyAddressesProvider.USPS.Server"));

            // Load Xml into a XmlDocument object
            XmlDocument USPSResponse = new XmlDocument();
            try
            {
                USPSResponse.LoadXml(received);
            }
            catch
            {
                return AppLogic.ro_OK; // we don't want to bug the customer if the server did not respond.
            }

            // Check for error response from server
            XmlNodeList USPSErrors = USPSResponse.GetElementsByTagName("Error");
            if (USPSErrors.Count > 0) // Error has occurred
            {
                XmlNodeList USPSError = USPSResponse.GetElementsByTagName("Error");
                XmlNode USPSErrorMessage = USPSError.Item(0);
                result = AppLogic.GetString("uspsValidate" + USPSErrorMessage["Number"].InnerText, EnteredAddress.SkinID, EnteredAddress.LocaleSetting);
                if (result == "uspsValidate" + USPSErrorMessage["Number"].InnerText)
                { // Use the USPS Error Description for error messages we don't have String values for.
                    result = "Address Verification Error: " + USPSErrorMessage["Description"].InnerText;
                }
                return result;
            }
            else  // No error, proceed looking for returned address
            {
                XmlNodeList USPSAddresses = USPSResponse.GetElementsByTagName("Address");
                if (USPSAddresses.Count > 0)
                {
                    XmlNode USPSAddress = USPSAddresses.Item(0);
                    // our address1 is their address2
                    ResultAddress.Address1 = USPSAddress["Address2"].InnerText;
					ResultAddress.Address2 = USPSAddress["Address1"] != null ? USPSAddress["Address1"].InnerText : String.Empty;
                    ResultAddress.City = USPSAddress["City"].InnerText;
                    ResultAddress.State = USPSAddress["State"].InnerText;
                    ResultAddress.Zip = USPSAddress["Zip5"].InnerText;
                    // add Zip+4 if it exists
                    ResultAddress.Zip += USPSAddress["Zip4"].InnerText.Length == 4 ? "-" + USPSAddress["Zip4"].InnerText : String.Empty;
                    ResultAddress.FirstName = ResultAddress.FirstName.ToUpperInvariant();
                    ResultAddress.LastName = ResultAddress.LastName.ToUpperInvariant();
                    ResultAddress.Company = ResultAddress.Company.ToUpperInvariant();
                    result = AppLogic.GetString("uspsValidateStandardized", EnteredAddress.SkinID, EnteredAddress.LocaleSetting);

                    // Does the resulting address matches the entered address?
                    bool IsMatch = true;
                    if (ResultAddress.Address1 != EnteredAddress.Address1)
                    {
                        IsMatch = false;
                    }
                    else if (ResultAddress.Address2 != EnteredAddress.Address2)
                    {
                        IsMatch = false;
                    }
                    else if (ResultAddress.City != EnteredAddress.City)
                    {
                        IsMatch = false;
                    }
                    else if (ResultAddress.State != EnteredAddress.State)
                    {
                        IsMatch = false;
                    }
                    else if (ResultAddress.Country != EnteredAddress.Country)
                    {
                        IsMatch = false;
                    }
                    else if (ResultAddress.Zip != EnteredAddress.Zip)
                    {
                        IsMatch = false;
                    }

                    // If the resulting address matches the entered address, then return ro_OK
                    if (IsMatch)
                    {
                        result = AppLogic.ro_OK;
                    }
                }
                else
                { // unknown response from server
                    return AppLogic.ro_OK; // we don't want to bug the customer if we don't recognize the response.
                }
            }
            return result;
        }
    }
}
