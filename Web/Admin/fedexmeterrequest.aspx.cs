// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class FedExMeterRequest : AdminPageBase
	{
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Page.Form.DefaultButton = btnSubmitRequest.UniqueID;
		}

		protected void btnSubmitRequest_Click(object sender, System.EventArgs e)
		{
			FedExSubscription mySub = new FedExSubscription();
			string myRequest = mySub.CreateFedExRequest(AccountNumber.Text.Trim()
				,FullName.Text.Trim()
				,CompanyName.Text.Trim()
				,Department.Text.Trim()
				,PhoneNumber.Text.Trim()
				,PagerNumber.Text.Trim()
				,FaxNumber.Text.Trim()
				,EMail.Text.ToLowerInvariant().Trim()
				,Address.Text.Trim()
				,City.Text.Trim()
				,State.Text.Trim()
				,Zip.Text.Trim()
				,Country.Text.Trim());
			
			bool successfulSubmit = mySub.Send(myRequest,FedExServer.Text.Trim());
			
			if(successfulSubmit)
			{
				// Sucess the submission was successful, parse the response to fill the properties
				string myResponse = mySub.FedExResponse;
				mySub.ParseSubscrpitionResponse(myResponse);
				if(mySub.SubscriptionSuccess)
				{
					lblMeter.Text = mySub.MeterNumber;
				}
				else
				{
					lblMeter.Text = String.Format(AppLogic.GetString("admin.fedexmeterrequest.Error", SkinID, LocaleSetting), mySub.ErrorMessage);
					lblMeter.ForeColor = Color.Red;
					lblMeter.Font.Size = FontUnit.Larger;
				}
			}
			else
			{
				// The submission was not successfull
				lblMeter.Text = String.Format(AppLogic.GetString("admin.fedexmeterrequest.Error", SkinID, LocaleSetting), mySub.ErrorMessage);
				lblMeter.ForeColor = Color.Red;
				lblMeter.Font.Size = FontUnit.Larger;
			}

			responseX.Text = String.Format(AppLogic.GetString("admin.fedexmeterrequest.FedexRequest", SkinID, LocaleSetting),Server.HtmlEncode(mySub.FedExRequest));
			responseX.Text += String.Format(AppLogic.GetString("admin.fedexmeterrequest.FedexRequest", SkinID, LocaleSetting), Server.HtmlEncode(mySub.FedExResponse));
		}
	}

    public class FedExSubscription
    {
        private string fedExRequest;
        private string fedExResponse;
        private bool subscriptionSuccess;
        private string errorCode;
        private string errorMessage;
        private string meterNumber;

        public FedExSubscription()
        {
            fedExRequest = string.Empty;
            fedExResponse = string.Empty;
            subscriptionSuccess = false;
            errorCode = string.Empty;
            errorMessage = string.Empty;
            meterNumber = string.Empty;
        }

        /// <summary>
        /// Sends the subscription request
        /// </summary>
        /// <param name="Request">The FedEx XML subscription request, created with the &quot;CreateRequest()&quot; method</param>
        /// <param name="Server">The URL to the FedEx subscription server</param>
        /// <returns></returns>
        public bool Send(string Request, string Server)
        {
            bool sendSuccess = true;
            // Set encoding & get content length
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(Request); // Request

            // Prepare post request
            HttpWebRequest shipRequest = (HttpWebRequest)WebRequest.Create(Server); // Server

            // Add FedEx specific headers
            shipRequest.Method = "POST";
            shipRequest.Referer = "Avetar Interactive";
            shipRequest.Accept = "image/gif, image/jpg, image/jpeg, image/pjpeg, text/plain, text/html, */*";
            shipRequest.ContentType = "image/gif";
            shipRequest.ContentLength = data.Length;
            shipRequest.UserAgent = "Mozilla/4.0 (compatible; Win32; Avetar Interactive Shipping Module)";

            Stream requestStream = shipRequest.GetRequestStream();
            // Send the data
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            // get the response
            WebResponse shipResponse;
            string strResponse = String.Empty;

            try
            {
                shipResponse = shipRequest.GetResponse();
                using (StreamReader sr = new StreamReader(shipResponse.GetResponseStream()))
                {
                    strResponse = sr.ReadToEnd();
                    sr.Close();
                    fedExResponse = strResponse;
                }

            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                sendSuccess = false;
            }
            finally
            {
                shipRequest = null;
                requestStream = null;
                shipResponse = null;
            }

            return sendSuccess;

        }

        /// <summary>
        /// Create the FedEx XML Request
        /// </summary>
        /// <param name="AccountNumber">FedEx Account Number</param>
        /// <param name="FullName">Self Explanatory</param>
        /// <param name="CompanyName">Self Explanatory</param>
        /// <param name="Department">Self Explanatory</param>
        /// <param name="PhoneNumber">Self Explanatory</param>
        /// <param name="PagerNumber">Self Explanatory</param>
        /// <param name="FaxNumber">Self Explanatory</param>
        /// <param name="EMail">Self Explanatory</param>
        /// <param name="Address">Self Explanatory</param>
        /// <param name="City">Self Explanatory</param>
        /// <param name="State">Self Explanatory (2 character state code)</param>
        /// <param name="Zip">Self Explanatory</param>
        /// <param name="Country">Self Explanatory (2 character country code)</param>
        /// <returns>A string containing the FedEx XML Response</returns>
        public string CreateFedExRequest(string AccountNumber,
            string FullName,
            string CompanyName,
            string Department,
            string PhoneNumber,
            string PagerNumber,
            string FaxNumber,
            string EMail,
            string Address,
            string City,
            string State,
            string Zip,
            string Country)
        {
            string FedExRequest = string.Empty;
            FedExRequest = "<?xml version='1.0' encoding='UTF-8' ?>"
                + "<FDXSubscriptionRequest xmlns:api='http://www.fedex.com/fsmapi' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='FDXSubscriptionRequest.xsd'>"
                + "<RequestHeader>"
                + "<CustomerTransactionIdentifier>String</CustomerTransactionIdentifier>"
                + "<AccountNumber>" + AccountNumber.Trim() + "</AccountNumber>"
                + "</RequestHeader>"
                + "<Contact>"
                + "<PersonName>" + FullName.Trim() + "</PersonName>"
                + "<CompanyName>" + CompanyName.Trim() + "</CompanyName>"
                + "<Department>" + Department.Trim() + "</Department>"
                + "<PhoneNumber>" + PhoneNumber.Trim().Replace("-", "").Replace("(", "").Replace(")", "").Replace(",", "").Replace(".", "").Replace("+", "") + "</PhoneNumber>"
                + "<PagerNumber>" + PagerNumber.Trim().Replace("-", "").Replace("(", "").Replace(")", "").Replace(",", "").Replace(".", "").Replace("+", "") + "</PagerNumber>"
                + "<FaxNumber>" + FaxNumber.Trim().Replace("-", "").Replace("(", "").Replace(")", "").Replace(",", "").Replace(".", "").Replace("+", "") + "</FaxNumber>"
                + "<E-MailAddress>" + EMail.Trim() + "</E-MailAddress>"
                + "</Contact>"
                + "<Address>"
                + "<Line1>" + Address.Trim() + "</Line1>"
                + "<City>" + City.Trim() + "</City>"
                + "<StateOrProvinceCode>" + State.Trim() + "</StateOrProvinceCode>"
                + "<PostalCode>" + Zip.Trim() + "</PostalCode>"
                + "<CountryCode>" + Country.Trim() + "</CountryCode>"
                + "</Address>"
                + "</FDXSubscriptionRequest>";

            fedExRequest = FedExRequest;

            return FedExRequest;
        }

        /// <summary>
        /// Parses the XML Response returned from FedEx
        /// </summary>
        /// <param name="FedExResponse"></param>
        public void ParseSubscrpitionResponse(string FedExResponse)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(FedExResponse);

            // Check for errors
            XmlNodeList replyError = doc.SelectNodes("/FDXSubscriptionReply/Error");
            XmlNodeList rootError = doc.SelectNodes("/Error");

            if (replyError.Count > 0 || rootError.Count > 0)
            {
                string FedExErrorCode = string.Empty;
                string FedExErrorMessage = string.Empty;

                XmlNode code = null;
                XmlNode message = null;

                if (replyError.Count > 0)
                {
                    code = doc.SelectSingleNode("/FDXSubscriptionReply/Error/Code");
                    message = doc.SelectSingleNode("/FDXSubscriptionReply/Error/Message");
                }
                else
                {
                    code = doc.SelectSingleNode("/Error/Code");
                    message = doc.SelectSingleNode("/Error/Message");
                }

                if (code != null)
                    FedExErrorCode = code.InnerText;

                if (message != null)
                    FedExErrorMessage = message.InnerText;

                errorCode = FedExErrorCode;
                errorMessage = FedExErrorMessage;
                subscriptionSuccess = false;

            }
            else
            // Success!
            {
                XmlNode xmlMeterNumber = doc.SelectSingleNode("/FDXSubscriptionReply/MeterNumber");
                subscriptionSuccess = true;
                meterNumber = xmlMeterNumber.InnerText;
            }
        }

        /// <summary>
        /// The Response received from the FedEx subscription server
        /// </summary>
        public string FedExResponse
        {
            get { return fedExResponse; }
        }

        /// <summary>
        /// The Status of the submission once the &quot;Send()&quot; method has been called.
        /// Either &quot;Success&quot; or &quot;Error&quot;
        /// </summary>
        public bool SubscriptionSuccess
        {
            get { return subscriptionSuccess; }
        }

        /// <summary>
        /// The Meter number returned by the FedEx server
        /// </summary>
        public string MeterNumber
        {
            get { return meterNumber; }
        }

        /// <summary>
        /// The FedEx XML Subscription Request once the &quot;CreateRequest()&quot; method has been called
        /// </summary>
        public string FedExRequest
        {
            get { return fedExRequest; }
        }

        /// <summary>
        /// The Error message returned if any
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
        }

        /// <summary>
        /// The Error code returned if any
        /// </summary>
        public string ErrorCode
        {
            get { return errorCode; }
        }

    }



}
