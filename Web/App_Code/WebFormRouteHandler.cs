// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.Routing;
using System.Web.Security;
using System.Security;
using System.Text;
using AspDotNetStorefrontCore;
using System.Collections.Generic;

namespace AspDotNetStorefront
{
    public class WebFormRouteHandler : IRouteHandler
    {
        private bool m_physicalurlaccess = false;

        public WebFormRouteHandler(string virtualPath)
            : this(virtualPath, true)
        {
        }

        public WebFormRouteHandler(string virtualPath, bool checkPhysicalUrlAccess)
        {
            if (virtualPath == null) throw new ArgumentNullException("virtualPath");
            if (!virtualPath.StartsWith("~/")) throw new ArgumentException("virtualPath must start with a tilde slash: \"~/\"", "virtualPath");

            this.VirtualPath = virtualPath;
            this.CheckPhysicalUrlAccess = checkPhysicalUrlAccess;
        }

        /// <summary>
        /// This is the full virtual path (using tilde syntax) to the WebForm page.
        /// </summary>
        /// <remarks>
        /// Needs to be thread safe so this is only settable via ctor.
        /// </remarks>
        public string VirtualPath { get; private set; }

        /// <summary>
        /// Because we're not actually rewriting the URL, ASP.NET's URL Auth will apply to the incoming request URL and not the URL of the physical WebForm page.
        /// Setting this to true (default) will apply URL access rules against the physical file.
        /// </summary>
        /// <value>True by default</value>
        public bool CheckPhysicalUrlAccess
        {
            get { return m_physicalurlaccess; }
            set { m_physicalurlaccess = value; }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            string substVirtualPath = GetSubstitutedVirtualPath(requestContext);

            if (this.CheckPhysicalUrlAccess && !UrlAuthorizationModule.CheckUrlAccessForPrincipal(substVirtualPath, requestContext.HttpContext.User, requestContext.HttpContext.Request.HttpMethod))
            {
                throw new SecurityException();
            }

            // add query string parameters
            string queryString = string.Empty;
            string SEName = string.Empty;

            foreach (KeyValuePair<string, object> aux in requestContext.RouteData.Values)
            {
                if (aux.Key.Contains("SEPart"))
                {
                    // reassemble SEName from the SEParts
                    SEName = requestContext.HttpContext.Server.UrlEncode(aux.Value.ToString()) + (SEName.Length == 0 ? string.Empty : "-" + SEName);
                }
                else
                {
                    queryString += (queryString.Length == 0 ? "?" : "&") + requestContext.HttpContext.Server.UrlEncode(aux.Key) + "=" + requestContext.HttpContext.Server.UrlEncode(aux.Value.ToString());
                }
            }

            if (SEName.Length > 0) queryString += (queryString.Length == 0 ? "?" : "&") + "SEName=" + SEName;  // add reassembled SEName

            IHttpHandler handler = null;
            
            if (substVirtualPath.Contains(".aspx"))
            {
                handler = BuildManager.CreateInstanceFromVirtualPath("~/" + substVirtualPath, typeof(Page)) as IHttpHandler;
            }
                    
            else if (substVirtualPath.Contains(".ashx"))
            {
                handler = BuildManager.CreateInstanceFromVirtualPath("~/" + substVirtualPath, typeof(IHttpHandler)) as IHttpHandler;
            }                

            if (handler != null)
            {
                HttpContext.Current.SetRequestData(new RequestData(requestContext));
            }           
            

            return handler;

        }

        /// <summary>
        /// Gets the virtual path to the resource after applying substitutions based on route data.
        /// </summary>
        public string GetSubstitutedVirtualPath(RequestContext requestContext)
        {
            string substVirtualPath = VirtualPath.Substring(2); //Trim off ~/

            if (!VirtualPath.Contains("{")) return substVirtualPath;

            Route route = new Route(substVirtualPath, this);
            VirtualPathData vpd = route.GetVirtualPath(requestContext, requestContext.RouteData.Values);
            if (vpd == null) return substVirtualPath;
            return vpd.VirtualPath;
        }
    }
}
