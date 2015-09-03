// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://aspdotnetstorefront.com")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.Web.Script.Services.ScriptService]
    public class NewsletterControlService : System.Web.Services.WebService
    {
        public NewsletterControlService()
        { }

        #region "Properties"
        private enum SF_DataType
        {
            AppConfig,
            StringResource,
            Topic
        }

        private string GetSFValue(SF_DataType type, string name)
        {
            switch (type)
            {
                case SF_DataType.AppConfig:
                    return AppLogic.AppConfig(name);
                case SF_DataType.StringResource:
                    return AppLogic.GetString(name, "");
            }
            return null;
        }
        private string _emailAddress;

        /// <summary>
        /// Captcha code derived from session
        /// </summary>
        private string captchaCode
        {
            get
            {
               return (string)Context.Session["SecurityCode"];
            }
        }

        private static int _skinID = 1;

        /// <summary>
        /// string Resource for first name
        /// </summary>
        private static string firstNameLabel
        {
            get
            {
                return AppLogic.GetString("Newsletter.FirstName", Customer.Current.SkinID, Customer.Current.LocaleSetting); 
            }
        }
        /// <summary>
        /// String Resource for last name
        /// </summary>
        private static string lastNameLabel
        {
            get
            {
                return AppLogic.GetString("Newsletter.LastName", Customer.Current.SkinID, Customer.Current.LocaleSetting);
            }
        }

        /// <summary>
        /// HTML for first and last name fields for if getNames is false, empty spans with apropriate IDs
        /// </summary>
        public static string firstAndLastHTML
        {
            get
            {
                string withHTML =
 @"
<!--This is a system level topic, do not delete-->
    <tr>
		<td style='width:50px'>%FirstName%</td>
		<td width='50px'><input type='text' id='txt_NL_FirstName' /></td>
	</tr>
    <tr>    
		<td>%LastName%</td>
		<td><input type='text' id='txt_NL_LastName' /></td>
	</tr>
";

                string withoutHTML =
 @"
<input type='hidden' id='txt_NL_FirstName' value='' />
<input type='hidden' id='txt_NL_LastName' value='' />
";

                if (!getName)
                {
                    return withoutHTML;
                }
                else
                {
                    return withHTML
                        .Replace("%FirstName%", firstNameLabel)
                        .Replace("%LastName%", lastNameLabel);
                }

            }
        }
        /// <summary>
        /// App.Config indicating whether or not to collect first and last name as well
        /// </summary>
        private static bool getName
        {
            get
            {
                return Localization.ParseBoolean(AppLogic.AppConfig("Newsletter.GetFirstAndLast"));
            }
        }

        /// <summary>
        /// Indicator of whether or not opt-int e-mail is requred
        /// </summary>
        private bool OptInLevel
        {
            get
            {
                return AppLogic.AppConfig("Newsletter.OptInLevel").ToLowerInvariant() != "single";
            }
        }

        /// <summary>
        /// AppConfig indicating whether or not to use captcha
        /// </summary>
        private static bool _captchaOn
        {
            get
            {
                return AppLogic.AppConfigBool("Newsletter.UseCaptcha");
            }
        }

        /// <summary>
        /// Base subscription token
        /// </summary>
        public static string GetNewsletterToken()
        {
            string html =
 @"
<div id='ptkSubscribe'>
		<table class='NewsletterBox' style='width:50px' width='50px'>
		<tr>
			<td colspan='2'>%SubscribeText%</td>
		</tr>
        %FirstAndLastHTML%
		<tr>
		<td>%EmailAddress%</td>
		<td>
            <input type='text' id='txtEmailAddress' onkeypress='enterSubmit(event);' size='13' />
        <input type='hidden' id='txtCaptcha' value='' /></td>
	</tr>
	<tr>
		<td align='left' colspan=2><input type='button' value='%Submit%'id='cmdSubmit' onclick='clickSubmit();' /></td>
	</tr>
	</table>
  </div>
             ";

            html = html
                .Replace("%SubscribeText%", new Topic("SubscriptionToken.Subscribe", string.Empty, _skinID).ContentsRAW)
                .Replace("%FirstAndLastHTML%", firstAndLastHTML)
                .Replace("%EmailAddress%", AppLogic.GetString("Newsletter.Email", Customer.Current.SkinID, Customer.Current.LocaleSetting))
                .Replace("%Submit%", AppLogic.GetString("newsletter.Submit", Customer.Current.SkinID, Customer.Current.LocaleSetting));

            string script =
 @"
            <script>
            function clickSubmit()
            {
	            var xReq = new SOAPRequest('Newsletter.Subscribe.asmx', 'Subscribe', 'http://aspdotnetstorefront.com')  
                xReq.addParameter('emailAddress', document.getElementById('txtEmailAddress').value);
                xReq.addParameter('Captcha', document.getElementById('txtCaptcha').value);
	            xReq.Execute(buildBox);
            }
            function buildBox(submitResponse)
            {
	            document.getElementById('ptkSubscribe').innerHTML = 
		            submitResponse;
            }
            </script>
            ";

            return html + "\n" + script;
        }

        
        /// <summary>
        /// URL to confirm subscription
        /// </summary>
        private string subscribeURL
        {
            get
            {
                string m_StoreURL = AppLogic.GetStoreHTTPLocation(false);
                return string.Format("{0}NewsletterOptInOut.aspx?GUID={1}&Opt=in", m_StoreURL, Guid);
            }
        }

        /// <summary>
        /// AppConfig("ReceiptEMailFrom")
        /// </summary>
        private String FromEMail
        {
            get { return AppLogic.AppConfig("ReceiptEMailFrom"); }
        }

        /// <summary>
        /// AppConfig("ReceiptEMailFromName")
        /// </summary>
        private String FromName
        {
            get { return AppLogic.AppConfig("ReceiptEMailFromName"); }
        }
        /// <summary>
        /// Guid of the email address
        /// </summary>
        private string Guid
        {
            get
            {
                string sql = string.Format(
                   "SELECT GUID FROM NewsletterMailList WHERE EmailAddress = {0}",
                   DB.SQuote(_emailAddress));

                string xString;
                SqlCommand xCmd = new SqlCommand(sql);
                using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
                {
                    xCmd.Connection = xCon;
                    xCon.Open();
                    xString = xCmd.ExecuteScalar().ToString();
                }

                return xString;
            }
        }
        private String StoreName
        {
            get { return AppLogic.AppConfig("StoreName"); }
        }
        private string OptInSubject
        {
            get
            {
                return AppLogic.GetString("Newsletter.OptInEmailSubject", Customer.Current.SkinID, Customer.Current.LocaleSetting)
                    .Replace("%StoreName%", StoreName);
            }
        }
       
        /// <summary>
        /// AppLogic.MailServer()
        /// </summary>
        private String FromServer
        {
            get { return AppLogic.MailServer(); }
        }


        private string OptInBody
        {
            get
            {
                string mailText =
                new Topic("NewsletterOptInEmail", string.Empty, 0).ContentsRAW
                    .Replace("%CompanyName%", StoreName)
                    .Replace("%SubscribeURL%", subscribeURL);
                return mailText;

            }
        }

        /// <summary>
        /// Timespan to stall for stallback
        /// </summary>
        private TimeSpan CaptchaErrorStall
        {
            get
            {
                int stallTime = int.Parse((float.Parse(AppLogic.AppConfig("Newsletter.CaptchaErrorDisplayLength")) * 1000).ToString());
                return new TimeSpan(0, 0, 0, 0, stallTime);
            }
        }

        private bool captchaValid(string Captcha)
        {
            if (AppLogic.AppConfigBool("Captcha.CaseSensitive"))
                return Captcha == captchaCode;

            return Captcha.Equals(captchaCode, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        private static string NukeCustomerSQL
        {
            get
            {
                return @"
                    DELETE FROM NewsletterMailList 
                    WHERE EXISTS(
	                    SELECT C.Email FROM Customer AS C 
	                    WHERE C.Email =EmailAddress AND
	                    C.CustomerID = @ID
	                    )
                    ";
            }
        }
        public static void NukeCustomer(int ID)
        {
            SqlParameter xParm = new SqlParameter("@ID", ID);
            DB.ExecuteSQL(NukeCustomerSQL, new SqlParameter[] { xParm });
        }

        public void Subscribe(string emailAddress, string firstName, string lastName)
        {

            _emailAddress = emailAddress;
            SqlCommand cmdSubscribe = new SqlCommand(

@"
            IF NOT EXISTS (SELECT ID FROM NewsletterMailList WHERE EmailAddress = @Address)
              INSERT INTO NewsletterMailList
              (EmailAddress, SubscriptionConfirmed, FirstName, LastName, AddedOn, SubscribedOn,StoreId)
              VALUES (@Address, @SubscriptionConfirmed, @FirstName, @LastName, GetDate(), @SubscribedOn,@StoreId)
              ELSE
              UPDATE NewsletterMailList
              SET SubscriptionConfirmed = @SubscriptionConfirmed,
              SubscribedOn = GETDATE(),
              UnSubscribedOn = null
              WHERE EmailAddress = @Address  
            "
);
            cmdSubscribe.Parameters.Add(
                new SqlParameter("@Address", emailAddress.ToLowerInvariant()));
            cmdSubscribe.Parameters.Add(
                new SqlParameter("@SubscriptionConfirmed", OptInLevel ? 0 : 1));
            cmdSubscribe.Parameters.Add(
                new SqlParameter("@FirstName", firstName));
            cmdSubscribe.Parameters.Add(
                new SqlParameter("@LastName", lastName));
            cmdSubscribe.Parameters.Add(
                new SqlParameter("@StoreId", AppLogic.StoreID()));
            
            if (OptInLevel)
            {
                cmdSubscribe.Parameters.Add(
                    new SqlParameter("@SubscribedOn", DBNull.Value));
            }
            else
            {
                cmdSubscribe.Parameters.Add(
                    new SqlParameter("@SubscribedOn", DateTime.Now));
            }

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                cmdSubscribe.Connection = conn;
                conn.Open();
                cmdSubscribe.ExecuteNonQuery();
            }
            if (OptInLevel)
                SendOptInEmail();
        }
        private void SendOptInEmail()
        {
            AppLogic.SendMail(OptInSubject, OptInBody, true, FromEMail, FromName, _emailAddress, string.Empty, string.Empty, FromServer);
        }
        
        [WebMethod(EnableSession = true)]
        public string Subscribe(string FirstName, string LastName, string emailAddress, string Captcha)
        {
            if (!(new EmailAddressValidator().IsValidEmailAddress(emailAddress)))
                return "ADDRESS_ERROR";

            if (_captchaOn && Captcha == "")
            {
                string strSubscribeCaptcha =
 @"
                    <input type='hidden' id='txt_NL_FirstName' value='%FirstName%' />
                    <input type='hidden' id='txt_NL_LastName' value='%LastName%' />
                    <table class='NewsletterBox' >
		                <tr class='captchaBox'>
			                <td colspan='2'><img src='Captcha.ashx?id=%ID%' height='50px' width='175px' /></td>
		                </tr>
		                <tr>
			                <td class='fieldHeader' style='{font-size:70%}'>%Captcha%</td>
			                <td class='fieldHeader'>&nbsp;</td>
		                </tr>
		                <tr>
			                <td><input type='text' id='txtCaptcha' size='6' maxlength='6' /><input type='hidden' id='txtEmailAddress' value='%EmailAddress%' /></td>
			                <td><input type='button' value='%Confirm%'id='cmdSubmit' onclick='clickSubmit();' /></td>
		                </tr>
	                </table>
";


                return strSubscribeCaptcha
                    .Replace("%EmailAddress%", emailAddress)
                    .Replace("%Captcha%", GetSFValue(SF_DataType.StringResource, "Global.CaptchaText"))
                    .Replace("%FirstName%", FirstName)
                    .Replace("%LastName%", LastName)
                    .Replace("%Confirm%", AppLogic.GetString("Newsletter.Confirm", Customer.Current.SkinID, Customer.Current.LocaleSetting))
                    .Replace("%ID%", new Random(DateTime.Now.Millisecond).Next(4, 100).ToString());
            }
            else
            {
                if (! _captchaOn || captchaValid(Captcha))
                {
                    Subscribe(emailAddress, FirstName, LastName);
                    if (!OptInLevel)
                    {
                        return
                          new Topic("Newsletter.SubscribeSuccessful",  _skinID).ContentsRAW
                          .Replace("%EmailAddress%", emailAddress);
                    }
                    else
                    {
                        return
                            new Topic("Newsletter.SubscribeConfirm", _skinID).ContentsRAW
                          .Replace("%EmailAddress%", emailAddress);
                    }
                }
                else
                {
                    return "CAPTCHA_ERROR";
                }

            }
        }


        /// <summary>
        /// The amount of time to delay for the stallback
        /// </summary>
        
        [WebMethod()]
        public string ErrorStallback()
        {
            System.Threading.Thread.Sleep(CaptchaErrorStall);
            return GetNewsletterToken();
        }
        
        [WebMethod()]
        public string getAddressErrorBlock()
        {
            return new Topic("Newsletter.AddressErrorBlock").ContentsRAW;
        }
        
        [WebMethod()]
        public string getCapthaErrorBlock()
        {
            return new Topic("Newsletter.CaptchaErrorBlock").ContentsRAW;
        }


        [WebMethod()]
        public void UnSubscribe(string GUID)
        {
            SqlCommand cmdUnsubscribe = new SqlCommand(
@"
              UPDATE NewsletterMailList
              SET SubscriptionConfirmed = 0, UnsubscribedOn = GETDATE()
              WHERE GUID = @GUID
             "
);

            cmdUnsubscribe.Parameters.Add(
                new SqlParameter("@GUID", new Guid(GUID)));

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                cmdUnsubscribe.Connection = conn;
                cmdUnsubscribe.ExecuteNonQuery();
            }
        }


        #region "List Manager Web Methods"
        [WebMethod()]
        public int GetSubscriberCount()
        {
            return 0;
        }
        [WebMethod()]
        public string GetSubscriberList(int firstRecord, int lastRecord)
        {
            return null;
        }
        [WebMethod()]
        public string GetSubscribersWithAddressLike(string patternString)
        {
            return null;
        }
        
        [WebMethod()]
        public string GetSubscribersWithLastNameLike(string patternString)
        {
            return null;
        }
        #endregion

      

        
    }
    [XmlRoot(Namespace="http://aspdotnetstorefront.com")]
    public class NewsletterRecipient : SelfSerializer
    {
        public NewsletterRecipient(
            string first,
            string last,
            string email,
            string userGUID
            )
        {

            _FirstName = first;
            _LastName = last;          
            _EmailAddress = email;
            _GUID = userGUID;
        }
        
        private string _EmailAddress;
        private string _FirstName;
        private string _LastName;
        private string _GUID;
        private DateTime _AddedOn;
        private DateTime _SubscribedOn;
        private DateTime _UnsubscribedOn;
        
        [XmlElement]
        public string EmailAddress
        {
            get { return _EmailAddress; }
            set { _EmailAddress = value; }
        }
        [XmlElement]
        public string FirstName
        {
            get { return _FirstName; }
            set { _FirstName = value; }
        }
        [XmlElement]
        public string LastName
        {
            get { return _LastName; }
            set { _LastName = value; }
        }
        [XmlElement]
        public string GUID
        {
            get { return _GUID; }
            set { _GUID = value; }
        }
        [XmlElement]
        public DateTime AddedOn
        {
            get { return _AddedOn; }
            set { _AddedOn = value; }
        }
        [XmlElement]
        public DateTime SubscribedOn
        {
            get { return _SubscribedOn; }
            set { _SubscribedOn = value; }
        }
        [XmlElement]
        public DateTime UnsubscribedOn
        {
            get { return _UnsubscribedOn; }
            set { _UnsubscribedOn = value; }
        }


        [XmlIgnore]
        public string Name
        {
            get {
                if (_FirstName == string.Empty || _LastName == string.Empty)
                    return _EmailAddress;
                else
                    return _FirstName + " " + _LastName;
            }
        }
        
    }

}
