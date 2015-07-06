// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Xml;
using System.IO;
using System.Text;
using System.Globalization;
using System.Xml.Serialization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for orderXML.
	/// </summary>
	public partial class orderXML : AspDotNetStorefront.SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            // dump the order & customer info:
			int OrderNumber = CommonLogic.QueryStringUSInt("OrderNumber");
			try
			{
				String OrderXml = AppLogic.RunXmlPackage("DumpOrder",null,ThisCustomer,ThisCustomer.SkinID,"","ordernumber=" + OrderNumber.ToString(),false,true);
				if(OrderXml.IndexOf("XmlPackage Exception") == -1 && !AppLogic.AppConfigBool("XmlPackage.DumpTransform"))
				{
					Response.ContentType = "text/xml";
					Response.ContentEncoding = Encoding.UTF8;
				}
				Response.Write(OrderXml.Replace("utf-16","utf-8")); // NOT SURE WHY WE NEED THIS!!
			}
			catch(Exception ex)
			{
				Response.Write(CommonLogic.GetExceptionDetail(ex,"<br/>"));
			}
		}

	}
}
