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

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// This is for decorating your aspx page classes with a page  type 
	/// so that the page can be identifed by type in other places throughout 
	/// the appication or in xmlpackages via the GetPageType method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class PageTypeAttribute : Attribute
	{
		public string PageType { get; private set; }

		public PageTypeAttribute(string pageType)
		{
			if (pageType == null)
				throw new ArgumentNullException("pageType");

			PageType = pageType.ToLower();
		}
	}
}
