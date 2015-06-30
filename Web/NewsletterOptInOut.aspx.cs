// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    public partial class NewsletterOptInOut : SkinBase
    {
        #region "Properties"
        /// <summary>
        /// The level of opt-in or opt-out explicating the degree of verification required
        /// 
        /// ************** Single = 0 **************
        /// Submission of email is sufficient verification
        /// Not valid for opt-out
        /// 
        /// ************** Double = 1 **************
        /// Clicking of link in email servers as verification
        /// 
        /// ************** Triple = 2 **************
        /// Requires clicking of link an confirmation on the page to which it navigates
        /// </summary>
        private enum OptLevel
        {
            Single = 0,
            Double = 1,
            Triple = 2
        }

        /// <summary>
        /// Extract from AppConfig("Newsletter.OptOutLevel")
        /// </summary>
        private OptLevel OptOutLevel
        {
            get
            {
                switch (AppLogic.AppConfig("Newsletter.OptOutLevel").ToLowerInvariant())
                {
                    case ("single"):
                        return OptLevel.Single;
                    case ("double"):
                        return OptLevel.Double;
                    case ("triple"):
                        return OptLevel.Triple;
                }
                return OptLevel.Single;
            }
        }

        /// <summary>
        /// Extract from AppConfig("Newsletter.OptInLevel")
        /// </summary>
        private OptLevel OptInLevel
        {
            get
            {
                switch (AppLogic.AppConfig("Newsletter.OptInLevel").ToLowerInvariant())
                {
                    case ("single"):
                        return OptLevel.Single;
                    case ("double"):
                        return OptLevel.Double;
                    case ("triple"):
                        return OptLevel.Triple;
                }
                return OptLevel.Single;
            }
        }

        /// <summary>
        /// Extract from querystring:'GUID'
        /// </summary>
        private string Guid
        {
            get
            {
                return CommonLogic.QueryStringCanBeDangerousContent("GUID");
            }
        }
        /// <summary>
        /// Extract from querystring:'Opt'
        /// </summary>
        private bool OptingIn
        {
            get
            {
                return CommonLogic.QueryStringCanBeDangerousContent("Opt") == "in";
            }
        }

        /// <summary>
        /// Extract from querystring:'Opt'
        /// </summary>
        private bool OptingOut
        {
            get
            {
                return CommonLogic.QueryStringCanBeDangerousContent("Opt") == "out";
            }
        }

        /// <summary>
        /// Extract from string resource::SubscriptionToken.UnsubscribeSuccessful
        /// </summary>
        private string UnsubscribeSuccessful
        {
            get
            {
                return string.Format(
                    AppLogic.GetString("Newsletter.UnsubscribeSuccessful", base.SkinID, ThisCustomer.LocaleSetting),
                    EmailAddress);
            }
        }

        /// <summary>
        /// Extract from Topic::SubscriptionToken.SubscribeSuccessful
        /// </summary>
        private string SubscribeSuccessful
        {
            get
            {
                return new Topic("Newsletter.SubscribeSuccessful", string.Empty, 1).ContentsRAW
                    .Replace("%EmailAddress%", EmailAddress);
            }
        }
        /// <summary>
        /// Extract from Topic::Newsletter.ConfirmOptIn
        /// </summary>
        private string ConfirmOptIn
        {
            get { return new Topic("Newsletter.ConfirmOptIn", string.Empty, 1).ContentsRAW; }
        }
        /// <summary>
        /// Extract from string resource::Newsletter.ConfirmOptOut
        /// </summary>
        private string ConfirmOptOut
        {
            get { return new Topic("Newsletter.ConfirmOptOut", string.Empty, 1).ContentsRAW; }
        }
        private string _EmailAddress = null;
        /// <summary>
        /// 
        /// Email Address matching with querystring GUID
        /// </summary>
        private string EmailAddress
        {
            get
            {
                if (_EmailAddress == null || _EmailAddress == "")
                {
                    string sql = string.Format(
                        "SELECT EmailAddress AS S FROM NewsletterMailList WHERE GUID = {0} AND SubscriptionConfirmed = {1}",
                        DB.SQuote(Guid),
                        OptingIn ? 0 : 1);
                    /*
                    OptingIn ? 0 : 1
                    If user is opting in, we need to ignore all email address already confirmed
                    if user is opting out, we need to ignore all email address already opted out
                    */
                    _EmailAddress = DB.GetSqlS(sql);
                }
                return _EmailAddress;
            }
        }
        /// <summary>
        /// Topic -- Newsletter.OptInOutBadRequest
        /// The error message to be displayed if the request is invalid
        /// </summary>
        private string invalidGuidResponse
        {
            get
            {
                return new Topic("Newsletter.OptInOutBadRequest", string.Empty, 0).ContentsRAW;
            }
        }

        #endregion

        private void Unsubscribe()
        {
            string sql =
                "UPDATE NewsletterMailList SET SubscriptionConfirmed = 0, UnsubscribedOn = GetDate() WHERE [GUID] = @GUID";
            DB.ExecuteSQL(sql, new SqlParameter[]{
                new SqlParameter("@GUID", new Guid(Guid))});
            cmdConfirm.Visible = false;
            litOptOption.Text = UnsubscribeSuccessful;
        }

        private void Subscribe()
        {
            string sql =
                "UPDATE NewsletterMailList SET SubscriptionConfirmed = 1, SubscribedOn = GetDate() WHERE [GUID] = @GUID";
            DB.ExecuteSQL(sql, new SqlParameter[]{
                new SqlParameter("@GUID", new Guid(Guid))});
            cmdConfirm.Visible = false;
            litOptOption.Text = SubscribeSuccessful;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                return;
            }
            try
            {
                new Guid(Guid);
            }
            catch
            {
                litOptOption.Text = invalidGuidResponse;
                return;
            }

            if (Guid == String.Empty || EmailAddress == string.Empty || OptingIn == OptingOut)
            {
                litOptOption.Text = invalidGuidResponse;
                return;
            }
            if (OptingIn)
            {
                if (OptInLevel == OptLevel.Double)
                {
                    Subscribe();
                }
                if (OptInLevel == OptLevel.Triple)
                {
                    litOptOption.Text = ConfirmOptIn;
                    cmdConfirm.Visible = true;
                }
            }
            else
            {
                if (OptOutLevel == OptLevel.Double)
                {
                    Unsubscribe();
                }
                if (OptOutLevel == OptLevel.Triple)
                {
                    litOptOption.Text = ConfirmOptOut;
                    cmdConfirm.Visible = true;
                }
            }
        }
        protected void cmdConfirm_Click(object sender, EventArgs e)
        {
            if (OptingIn)
            {
                Subscribe();
            }

            if (OptingOut)
            {
                Unsubscribe();
            }
        }
    }
}
