// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Xml;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Outputs the raw package results along with setting any http headers specified in the package.
/// The package transform output method needs to match the Content-Type http header or you may not get the results you expect
/// </summary>
public class ExecXmlPackage : IHttpHandler
{
    public bool IsReusable
    {
        get { return true; }
    }

    private System.Web.Routing.RequestContext m_requestcontext;
    public System.Web.Routing.RequestContext RequestContext
    {
        get { return m_requestcontext; }
        set { m_requestcontext = value; }
    }
    private RequestData m_requestdata;
    public RequestData requestData
    {
        get { return m_requestdata; }
        set { m_requestdata = value; }
    }

    private string XMLPackage
    {
        get
        {
            return HttpContext.Current.GetRequestData().StringValue("xmlpackage");
        }
    }
    public void ProcessRequest(HttpContext context)
    {
        string pn = XMLPackage;
        Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)context.User).ThisCustomer;
        try
        {
            using (XmlPackage2 p = new XmlPackage2(pn, ThisCustomer, ThisCustomer.SkinID, "", "", "", true))
            {

                if (!p.AllowEngine)
                {
                    context.Response.Write("This XmlPackage is not allowed to be run from the engine.  Set the package element's allowengine attribute to true to enable this package to run.");
                }
                else
                {
                    if (p.HttpHeaders != null)
                    {
                        foreach (XmlNode xn in p.HttpHeaders)
                        {
                            string headername = xn.Attributes["headername"].InnerText;
                            string headervalue = xn.Attributes["headervalue"].InnerText;
                            context.Response.AddHeader(headername, headervalue);
                        }
                    }

                    StringBuilder output = new StringBuilder(10000);
                    if (p.RequiresParser)
                    {
                        Parser ps = new Parser(ThisCustomer.SkinID, ThisCustomer);
                        output.Append(ps.ReplaceTokens(p.TransformString()));
                    }
                    else
                    {
                        output.Append(p.TransformString());
                    }

                    context.Response.AddHeader("Content-Length", output.Length.ToString());
                    context.Response.Write(output);
                }
            }
        }
        catch (Exception ex)
        {
            context.Response.Write(context.Server.HtmlEncode(ex.Message) + "");
            Exception iex = ex.InnerException;
            while (iex != null)
            {
                context.Response.Write(context.Server.HtmlEncode(ex.Message) + "");
                iex = iex.InnerException;
            }
        }
    }
}
