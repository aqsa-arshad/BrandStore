// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.Routing;
using System.Configuration;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    public class RegisterRoutes : IHttpModule
    {
        private static bool first = true;

        /// <summary>
        /// Register routes contained in Web.Config to the RouteTable
        /// </summary>
        public void Init(HttpApplication application)
        {
            if (first)
            {
                // Get the Routes from Web.config
                RouteSection config = (RouteSection) ConfigurationManager.GetSection("system.web/routeTable");
                if (config == null) return;

                // Add each Route to RouteTable

                using (RouteTable.Routes.GetWriteLock())
                {
                    RouteTable.Routes.Clear();
                    foreach (RouteElement element in config.Routes)
                    {
						var dataTokens = new RouteValueDictionary
                        {
                            { "EntityType", element.EntityType },
                        };

						// handle special case of SEName. Break it into parts, so Routing parser can understand multi-dash Urls
                        if (element.Url.Contains("-{SEName}"))
                        {
                            string Url = element.Url.Replace("-{SEName}", "-{SEPart1}-{SEPart2}-{SEPart3}-{SEPart4}-{SEPart5}-{SEPart6}-{SEPart7}-{SEPart8}-{SEPart9}-{SEPart10}-{SEPart11}-{SEPart12}-{SEPart13}-{SEPart14}-{SEPart15}-{SEPart16}-{SEPart17}-{SEPart18}-{SEPart19}-{SEPart20}-{SEPart21}-{SEPart22}-{SEPart23}-{SEPart24}-{SEPart25}-{SEPart26}-{SEPart27}-{SEPart28}-{SEPart29}-{SEPart30}");
                            for (int c = 30; c >= 0; c--)
                            {
								RouteTable.Routes.Add(element.Name + c.ToString(), new Route(Url, null, null, dataTokens, new WebFormRouteHandler(element.VirtualPath, element.CheckPhysicalUrlAccess)));
                                
                                Url = Url.Replace("-{SEPart" + c.ToString() + "}", string.Empty);
                            }
                        }
						
						var route = new Route(element.Url, new WebFormRouteHandler(element.VirtualPath, element.CheckPhysicalUrlAccess));
						route.DataTokens = dataTokens;

						RouteTable.Routes.Add(element.Name, route);
                    }
                }
                   
            }

            first = false;
        }

        /// <summary>
        /// Needed for IHttpModule interface
        /// </summary>
        public void Dispose()
        {
        }
    }
}
