// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Routing;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Pages don't have to implement this interface, but the ones that do will be able to generate outgoing routing URLs.
    /// </summary>
    public interface IRoutablePage : IHttpHandler
    {
        RequestContext RequestContext { get; set; }
        //HtmlHelper Html { get; }
        //UrlHelper Url { get; }
    }
}
