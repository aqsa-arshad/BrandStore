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
using System.Xml.Linq;

namespace GatewayMoneybookers
{
	public static class XmlTransformExtensions
	{
		public static XElement ToXElement(this object value, XName name)
		{
			if(value == null)
				return null;

			return new XElement(name, value);
		}

		public static XAttribute ToXAttribute(this object value, XName name)
		{
			if(value == null)
				return null;

			return new XAttribute(name, value);
		}

		public static string TrimToMax(this string value, int max)
		{
			if(String.IsNullOrEmpty(value))
				return value;

			return value.Substring(0, Math.Min(value.Length, max));
		}
	}
}
