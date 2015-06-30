// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Services;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services.Protocols;
using System.Xml;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontWSI;

#if WSE3
using Microsoft.Web.Services3;
#endif

[WebService(Namespace = "http://www.aspdotnetstorefront.com/", Name = "AspDotNetStorefront Import WebService", Description = "AspDotNetStorefront Import Web Service I/F. NOTE: You may have to reset the store cache after imports to make all new items live! Also, this interface does not support any setting of IsImport flags, and imports cannot be undone.")]
public class ipx : System.Web.Services.WebService
{
    public ipx() {}

    [WebMethod(Description = "XmlDocument Input (as String). XmlDocument Output. This method requires Microsoft Web Services 3 UsernameToken authentication! When using WSE3 authentication, you must send in the full admin hashed password (you can find this in the master database record for the admin user customer record)", EnableSession = false)]
    public String DoItWSE3(String XmlInputRequestString)
    {
        if(!HttpContext.Current.Items.Contains("WSIAuthenticateTokenReceived") ||
            !(HttpContext.Current.User is AspDotNetStorefrontPrincipal))
        {
            // our username token manager did NOT set the context flag, indicating a successful authentication, so fail this request hard.
            throw new ArgumentException("Authentication Failed");
        }

        // Since we already validated the  user through the ASPDNSFUsernameTokenManager
        // We're safe to assume that it's already the current principal
        Customer thisCustomer = (HttpContext.Current.User as AspDotNetStorefrontPrincipal).ThisCustomer;
        // we'll just explitictly pass the customer object itself
        WSI IPXObject = new WSI(thisCustomer);
        return DoItHelper(IPXObject, ref XmlInputRequestString);
    }

    [WebMethod(Description = "XmlDocument Input (as String). XmlDocument Output. This method is less secure, but does not require Microsoft Web Services 3 UsernameToken authentication!. This method can be used over HTTPS to do username and password check on the call itself. Password should be clear text master password here.", EnableSession = false)]
    public String DoItUsernamePwd(String AuthenticationEMail, String AuthenticationPassword, String XmlInputRequestString)
    {

        WSI IPXObject = new WSI();
        if (!IPXObject.AuthenticateRequest(AuthenticationEMail, AuthenticationPassword))
        {
            throw new ArgumentException("Authentication Failed");
        }
        return DoItHelper(IPXObject, ref XmlInputRequestString);
    }

    [WebMethod(Description = "XmlDocument Input (as String). XmlDocument Output. This method is less secure, but does not require Microsoft Web Services 3 UsernameToken authentication!. This method can be used over HTTPS to do username and password check on the call itself. Password should be clear text master password here. This routine is now obsolete, please use DoItUsernamePassword instead", EnableSession = false)]
    public String DoIt(String AuthenticationEMail, String AuthenticationPassword, String XmlInputRequestString)
    {

        WSI IPXObject = new WSI();
        if (!IPXObject.AuthenticateRequest(AuthenticationEMail, AuthenticationPassword))
        {
            throw new ArgumentException("Authentication Failed");
        }
        return DoItHelper(IPXObject, ref XmlInputRequestString);
    }

    private String DoItHelper(WSI IPXObject, ref String XmlInputRequestString)
    {
        try
        {
            IPXObject.LoadFromString(XmlInputRequestString);
        }
        catch
        {
            throw new ArgumentException("Invalid Import XmlDocument");
        }
        IPXObject.DoIt();
        try
        {
            XmlDocument d = IPXObject.GetResults();
            String d_xml = d.InnerXml;
            return d_xml;
        }
        catch
        {
            return IPXObject.GetResultsAsString();
        }
    }

}

