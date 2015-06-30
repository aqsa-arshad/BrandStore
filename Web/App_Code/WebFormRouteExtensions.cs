// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace AspDotNetStorefront
{
    public static class WebFormRouteExtensions
    {
        public static WebFormRouteMapping Map(this RouteCollection routes, string name, string urlPattern)
        {
            return new WebFormRouteMapping(routes, name, urlPattern);
        }

        //Uses the url pattern as the name by default.
        public static WebFormRouteMapping Map(this RouteCollection routes, string urlPattern)
        {
            return new WebFormRouteMapping(routes, urlPattern, urlPattern);
        }

        public static string GetUrl(this IRoutablePage page, string name)
        {
            var pathInfo = RouteTable.Routes.GetVirtualPath(page.RequestContext, name, new RouteValueDictionary());
            if (pathInfo != null)
                return pathInfo.VirtualPath;

            return null;
        }

        public static string RouteLink(this IRoutablePage page, string name, string text)
        {
            var pathInfo = RouteTable.Routes.GetVirtualPath(page.RequestContext, name, new RouteValueDictionary());

            if (pathInfo != null)
            {
                //I do something funky here with the link text for demo purposes
                return string.Format("<a href=\"{0}\">{1}</a>", pathInfo.VirtualPath, text ?? pathInfo.VirtualPath);
            }

            return null;
        }

        public static string RouteLink(this IRoutablePage page, string name)
        {
            return RouteLink(page, name, null);
        }
    }

    public class WebFormRouteMapping
    {
        RouteCollection routes;
        string url;
        string name;

        public WebFormRouteMapping(RouteCollection routes, string name, string url)
        {
            this.url = url;
            this.name = name;
            this.routes = routes;
        }

        public void To(string virtualPath)
        {
            To(virtualPath, true);
        }

        public void To(string virtualPath, bool checkPhysicalUrlAccess)
        {
            this.routes.Add(name, new Route(url, new WebFormRouteHandler(virtualPath, checkPhysicalUrlAccess)));
        }
    }
}
