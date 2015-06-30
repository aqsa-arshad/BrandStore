// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#if WSE3
using System;
using System.Web;
using System.Xml;
using System.Web.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{

    [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public class ASPDNSFUsernameTokenManager : UsernameTokenManager
    {
        /// <summary>
        /// Constructs an instance of this security token manager.
        /// </summary>
        public ASPDNSFUsernameTokenManager() { }

        /// <summary>
        /// Constructs an instance of this security token manager.
        /// </summary>
        /// <param name="nodes">An XmlNodeList containing XML elements from a configuration file.</param>
        public ASPDNSFUsernameTokenManager(XmlNodeList nodes) : base(nodes) { }

        /// <summary>
        /// Returns the password or password equivalent for the username provided.
        /// Adds a principal to the token with user's roles.
        /// </summary>
        /// <param name="token">The username token</param>
        /// <returns>The password (or password equivalent) for the username</returns>
        protected override string AuthenticateToken(UsernameToken token)
        {
            Customer ThisCustomer = new Customer(token.Username, true);

            bool LoginOk = true;

            if (ThisCustomer.CustomerID <= 0)
            {
                LoginOk = false;
            }
            if (LoginOk && (ThisCustomer.BadLoginCount >= AppLogic.AppConfigNativeInt("MaxBadLogins") && ThisCustomer.LockedUntil > DateTime.Now))
            {
                LoginOk = false;
            }
            if (LoginOk && (!ThisCustomer.Active))
            {
                LoginOk = false;
            }
            if (LoginOk && (ThisCustomer.PwdChanged.AddDays(AppLogic.AppConfigUSDouble("AdminPwdChangeDays")) < DateTime.Now || ThisCustomer.PwdChangeRequired))
            {
                LoginOk = false;
            }

            if (LoginOk)
            {
                HttpContext.Current.Items.Add("WSIAuthenticateTokenReceived", "true");
                HttpContext.Current.User = new AspDotNetStorefrontPrincipal(ThisCustomer);
                return ThisCustomer.Password;
            }
            return null;
        }
    }
}

#endif
