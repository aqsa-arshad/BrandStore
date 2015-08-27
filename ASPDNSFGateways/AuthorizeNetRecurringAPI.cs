// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
// this is a helper class. All calls from AspDotNetStorefront should go through AuthorizeNet.cs class Interface

// Before working with this sample code, please be sure to read the accompanying Readme.txt file.
// It contains important information regarding the appropriate use of and conditions for this
// sample code. Also, please pay particular attention to the comments included in each individual
// code file, as they will assist you in the unique and correct implementation of this code on
// your specific platform.
//
// Copyright 2007 Authorize.Net Corp.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections;
using System.Net;
using AspDotNetStorefrontCore;


// These classes are used to serialize and deserialize data sent and received
// from the API server for the Automated Recurring Billing (ARB) API.

namespace AuthorizeNetRecurringAPI
{
    public class MerchantAuthentication
    {
        public string name;
        public string transactionKey;
    }

    public class ANetApiRequest
    {
        public MerchantAuthentication merchantAuthentication;
        public string refId = null;
    }

    public class Messages
    {
        public enum MessageType
        {
            Ok, Error
        }
        public MessageType resultCode;
        public class Message
        {
            public string code;
            public string text;
        }

        [System.Xml.Serialization.XmlElementAttribute("message")]
        public Message[] Msg;
    }

    public enum SubscriptionUnitType
    {
        days, months
    }

    public class NameAndAddress
    {
        public string firstName;
        public string lastName;
        public string company;
        public string address;
        public string city;
        public string state;
        public string zip;
        public string country;
    }

    public class Order
    {
        public string invoiceNumber;
        public string description;
    }

    public class Customer
    {
        public string type;     // Either "individual" or "business"
        public string id;
        public string email;
        public string phoneNumber;
        public string faxNumber;

    }

    public class PaymentSchedule
    {
        public struct Interval
        {
            public int length;
            public SubscriptionUnitType unit;
        }
        [System.Xml.Serialization.XmlIgnore()]
        public bool intervalSpecified;

        [System.Xml.Serialization.XmlElementAttribute("interval")]
        public Interval PaymentScheduleInterval;

        public string startDate;        // Format is YYYY-MM-DD

        [System.Xml.Serialization.XmlIgnore()]
        public bool totalOccurrencesSpecified;
        public int totalOccurrences;

        [System.Xml.Serialization.XmlIgnore()]
        public bool trialOccurrencesSpecified;
        public int trialOccurrences;
    }

    public class CreditCard
    {
        public string cardNumber;       // Number must be 13 or 16 digits. Must pass LUHN check.
        public string expirationDate;   // Format must be YYYY-MM
        public string cardCode;
    }

    public class BankAccount
    {
        public string accountType;      // One of "checking", "savings", or "businessChecking"
        public string routingNumber;    // Number must be 9 digits
        public string accountNumber;    // Number should be 5 to 17 digits
        public string nameOnAccount;
        public string echeckType;       // One of "PPD", "WEB", "CCD", or "TEL"
        public string bankName;
    }

    public class Payment
    {
        // Choice of BankAccountType or CreditCardType
        [System.Xml.Serialization.XmlElementAttribute("bankAccount", typeof(BankAccount))]
        [System.Xml.Serialization.XmlElementAttribute("creditCard", typeof(CreditCard))]
        public object item;
    }

    public class ARBSubscription
    {
        public string name;
        public PaymentSchedule paymentSchedule;

        [System.Xml.Serialization.XmlIgnore()]
        public bool amountSpecified;
        public decimal amount;

        [System.Xml.Serialization.XmlIgnore()]
        public bool trialAmountSpecified;
        public decimal trialAmount;

        public Payment payment;

        [System.Xml.Serialization.XmlIgnore()]
        public bool orderSpecified;

        [System.Xml.Serialization.XmlIgnore()]
        public bool customerSpecified;

        public Order order; // RJB added
        public Customer customer; // RJB added
        public NameAndAddress billTo;
        public NameAndAddress shipTo;
    }

    public class ANetApiResponse
    {
        public string refId;
        public Messages messages;
    }

    // --------------------------------------------------------------------------------------------
    // Error
    // --------------------------------------------------------------------------------------------

    [System.Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = "AnetApi/xml/v1/schema/AnetApiSchema.xsd")]
    public class ErrorResponse : ANetApiResponse
    {
        // This is the response returned by any API call that cannot get past basic validation
        // of the request. For example, if any errors occur parsing the XML.
        // It does not contain any more data than that provided by ANetApiResponseType.
    }

    // --------------------------------------------------------------------------------------------
    // Create Subscription
    // --------------------------------------------------------------------------------------------

    [System.Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = "AnetApi/xml/v1/schema/AnetApiSchema.xsd")]
    public class ARBCreateSubscriptionRequest : ANetApiRequest
    {
        public ARBSubscription subscription;
    }

    [System.Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = "AnetApi/xml/v1/schema/AnetApiSchema.xsd")]
    public class ARBCreateSubscriptionResponse : ANetApiResponse
    {
        public string subscriptionId;
    }


    // --------------------------------------------------------------------------------------------
    // Update Subscription
    // --------------------------------------------------------------------------------------------

    [System.Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = "AnetApi/xml/v1/schema/AnetApiSchema.xsd")]
    public class ARBUpdateSubscriptionRequest : ANetApiRequest
    {
        public string subscriptionId;
        public ARBSubscription subscription;
    }

    [System.Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = "AnetApi/xml/v1/schema/AnetApiSchema.xsd")]
    public class ARBUpdateSubscriptionResponse : ANetApiResponse
    {
        // No extra data returned for the Update method
    }

    // --------------------------------------------------------------------------------------------
    // Cancel Subscription
    // --------------------------------------------------------------------------------------------

    [System.Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = "AnetApi/xml/v1/schema/AnetApiSchema.xsd")]
    public class ARBCancelSubscriptionRequest : ANetApiRequest
    {
        public string subscriptionId;
    }

    [System.Serializable]
    [System.Xml.Serialization.XmlRoot(Namespace = "AnetApi/xml/v1/schema/AnetApiSchema.xsd")]
    public class ARBCancelSubscriptionResponse : ANetApiResponse
    {
        // No extra data returned for the Cancel method
    }

    public class APIHelper
    {

        public APIHelper() { }

        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Send the request to the API server and load the response into an XML document.
        /// An XmlSerializer is used to form the XML used in the request to the API server. 
        /// The response from the server is also XML. An XmlReader is used to process the
        /// response stream from the API server so that it can be loaded into an XmlDocument.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        /// True if successful, false if not. If true then the specified XmlDoc will contain the
        /// response received from the API server.
        /// </returns>
        /// 
        /// NOTE:
        /// 
        /// RJB MODS: if _apiUrl is empty, we will just simulate the call (The MANUAL gateway uses this)
        ///  so you can do simulations of RecurringBilling in MANUAL gateway mode
        ///  we just return always approved properly formated XmlDocs, matching the Authorize.Net API specs
        // ----------------------------------------------------------------------------------------
        public static String PostRequest(object apiRequest, String _apiUrl, out XmlDocument xmldoc)
        {
            String result = AppLogic.ro_OK;
            XmlSerializer serializer;
            xmldoc = null;

            try
            {
                if (_apiUrl == "manualgatewaysimulation")
                {
                    // return simulation XmlDocuments for each request type:
                    // simulate a new SubscriptionID on create requests, using a GUID
                    xmldoc = new XmlDocument();
                    String SimulatedXmlResponse = String.Empty;
                    if (apiRequest.GetType() == typeof(ARBCreateSubscriptionRequest))
                    {
                        String RefID = ((ARBCreateSubscriptionRequest)apiRequest).refId;
                        SimulatedXmlResponse = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ARBCreateSubscriptionResponse xmlns=\"AnetApi/xml/v1/schema/AnetApiSchema.xsd\"><refId>" + RefID + "</refId><messages><resultCode>Ok</resultCode><message><code>I00001</code><text>Successful.</text></message></messages><subscriptionId>" + DB.GetNewGUID().ToString() + "</subscriptionId></ARBCreateSubscriptionResponse>";
                    }
                    else if (apiRequest.GetType() == typeof(ARBCancelSubscriptionRequest))
                    {
                        String RefID = ((ARBCancelSubscriptionRequest)apiRequest).refId;
                        SimulatedXmlResponse = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ARBCancelSubscriptionResponse xmlns=\"AnetApi/xml/v1/schema/AnetApiSchema.xsd\"><refId>" + RefID + "</refId><messages><resultCode>Ok</resultCode><message><code>I00001</code><text>Successful.</text></message></messages></ARBCancelSubscriptionResponse>";
                    }
                    else if (apiRequest.GetType() == typeof(ARBUpdateSubscriptionRequest))
                    {
                        String RefID = ((ARBUpdateSubscriptionRequest)apiRequest).refId;
                        SimulatedXmlResponse = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ARBUpdateSubscriptionResponse xmlns=\"AnetApi/xml/v1/schema/AnetApiSchema.xsd\"><refId>" + RefID + "</refId><messages><resultCode>Ok</resultCode><message><code>I00001</code><text>Successful.</text></message></messages></ARBUpdateSubscriptionResponse>";
                    }
                    else
                    {
                        SimulatedXmlResponse = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ErrorResponse xmlns=\"AnetApi/xml/v1/schema/AnetApiSchema.xsd\"><messages><resultCode>Error</resultCode><message><code>E00003</code><text> An error occurred while parsing the XML request.</text></message></messages></ErrorResponse>";
                    }
                    xmldoc.LoadXml(SimulatedXmlResponse);
                }
                else
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(_apiUrl);
                    webRequest.Method = "POST";
                    webRequest.ContentType = "text/xml";
                    webRequest.KeepAlive = true;

                    // Serialize the request
                    serializer = new XmlSerializer(apiRequest.GetType());
                    XmlWriter writer = new XmlTextWriter(webRequest.GetRequestStream(), Encoding.UTF8);
                    serializer.Serialize(writer, apiRequest);
                    writer.Close();

                    // Get the response
                    WebResponse webResponse = webRequest.GetResponse();

                    // Load the response from the API server into an XmlDocument.
                    xmldoc = new XmlDocument();
                    xmldoc.Load(XmlReader.Create(webResponse.GetResponseStream()));
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Deserialize the given XML document into the correct object type using the root
        /// node to determine the type of output object.
        /// 
        /// For any given API request the response can be one of two types:
        ///    ErorrResponse or [methodname]Response.
        /// For example, the ARBCreateSubscriptionRequest would normally result in a response of
        /// ARBCreateSubscriptionResponse. This is also the name of the root node of the response.
        /// This name can be used to deserialize the response into local objects. 
        /// </summary>
        /// <param name="xmldoc">
        /// This is the XML document to process. It holds the response from the API server.
        /// </param>
        /// <param name="apiResponse">
        /// This will hold the deserialized object of the appropriate type.
        /// </param>
        /// <returns>
        /// True if successful, false if not.
        /// </returns>
        // ----------------------------------------------------------------------------------------
        public static String ProcessXmlResponse(XmlDocument xmldoc, out object apiResponse)
        {
            String result = AppLogic.ro_OK;
            XmlSerializer serializer;
            apiResponse = null;

            try
            {
                // Use the root node to determine the type of response object to create.
                switch (xmldoc.DocumentElement.Name)
                {
                    case "ARBCreateSubscriptionResponse":
                        serializer = new XmlSerializer(typeof(ARBCreateSubscriptionResponse));
                        apiResponse = (ARBCreateSubscriptionResponse)serializer.Deserialize(new StringReader(xmldoc.DocumentElement.OuterXml));
                        break;

                    case "ARBUpdateSubscriptionResponse":
                        serializer = new XmlSerializer(typeof(ARBUpdateSubscriptionResponse));
                        apiResponse = (ARBUpdateSubscriptionResponse)serializer.Deserialize(new StringReader(xmldoc.DocumentElement.OuterXml));
                        break;

                    case "ARBCancelSubscriptionResponse":
                        serializer = new XmlSerializer(typeof(ARBCancelSubscriptionResponse));
                        apiResponse = (ARBCancelSubscriptionResponse)serializer.Deserialize(new StringReader(xmldoc.DocumentElement.OuterXml));
                        break;

                    case "ErrorResponse":
                        serializer = new XmlSerializer(typeof(ErrorResponse));
                        apiResponse = (ErrorResponse)serializer.Deserialize(new StringReader(xmldoc.DocumentElement.OuterXml));
                        break;

                    default:
                        result = "Unexpected type of object: " + xmldoc.DocumentElement.Name;
                        break;
                }
            }
            catch (Exception ex)
            {
                apiResponse = null;
                result = ex.Message;
            }
            return result;
        }

        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// Determine the type of the response object and process accordingly.
        /// Since this is just sample code the only processing being done here is to write a few
        /// bits of information to the console window.
        /// </summary>
        /// <param name="response"></param>
        // ----------------------------------------------------------------------------------------
        public static String ProcessResponse(object response, out String ResultCode, out String SubscriptionID)
        {
            // Every response is based on ANetApiResponse so you can always do this sort of type casting.
            ANetApiResponse br = (ANetApiResponse)response;

            String result = AppLogic.ro_OK;
            SubscriptionID = String.Empty;
            ResultCode = br.messages.resultCode.ToString();

            // If the result code is OK then the request was successfully processed.
            if (br.messages.resultCode == Messages.MessageType.Ok)
            {
                result = AppLogic.ro_OK;
                // CreateSubscription is the only method that returns additional data.
                if (response.GetType() == typeof(ARBCreateSubscriptionResponse))
                {
                    ARBCreateSubscriptionResponse createResponse = (ARBCreateSubscriptionResponse)response;
                    SubscriptionID = createResponse.subscriptionId;
                }
            }
            else
            {
                for (int i = 0; i < br.messages.Msg.Length; i++)
                {
                    result += ("[" + br.messages.Msg[i].code + "] " + br.messages.Msg[i].text);
                }
            }
            return result;
        }

    }

}
