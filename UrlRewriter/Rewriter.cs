// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Xml;
using System.Configuration;
using System.Text.RegularExpressions;

namespace ASPDNSF.URLRewriter
{

    public class Rewriter : IConfigurationSectionHandler
    {
        protected XmlNode m_Rules = null;

        protected Rewriter() { }

        public string GetSubstitution(string sPath, out bool substituted)
		{
            substituted = false;

            sPath = HttpContext.Current.Server.UrlDecode(sPath);
			foreach(XmlNode n in m_Rules.SelectNodes("rule"))
			{
                String url = String.Empty;
                if (n.Attributes["url"] != null)
                {
                    url = n.Attributes["url"].Value;
                }
                else
				{
				    url = n.SelectSingleNode("url/text()").Value; // backwards compatibility
				}
				//Regex theReg = new Regex(url, RegexOptions.IgnoreCase);

				//Match m = theReg.Match(sPath);

                if (Regex.IsMatch(sPath, url, RegexOptions.Compiled | RegexOptions.IgnoreCase))
				{
                    String rewrite = String.Empty;
                    if (n.Attributes["rewrite"] != null)
                    {
                        rewrite = n.Attributes["rewrite"].Value;
                    }
                    else
                    {
                        rewrite = n.SelectSingleNode("rewrite/text()").Value; // backwards compatibility
                    }
                    String tmp = Regex.Replace(sPath, url, rewrite, RegexOptions.Compiled | RegexOptions.IgnoreCase).Trim();
					if(tmp.EndsWith("&"))
					{
						tmp = tmp.Substring(0,tmp.Length-1);
					}
                    substituted = true;
					return tmp;
				}
			}
			return sPath;
		}

        public static void Process()
        {
            Rewriter r = (Rewriter)System.Web.Configuration.WebConfigurationManager.GetSection("system.web/urlrewrites");

            bool substituted = false;
            string s = r.GetSubstitution(HttpContext.Current.Request.Url.PathAndQuery, out substituted);

            // if we've found a rewrite path based on the defined rules
            if (substituted)
            {
                HttpContext ctx = HttpContext.Current;

                // check if IIS 7's integrated pipeline,                
                if (HttpRuntime.UsingIntegratedPipeline)
                {
                    ctx.Server.TransferRequest(s, true);
                }
                else
                {
                    ctx.RewritePath(s);
                }
            }

            // just allow to continue as is
        }

        public object Create(object parent, object configContext, XmlNode section)
        {
            m_Rules = section;
            return this;
        }
    }
    
}
