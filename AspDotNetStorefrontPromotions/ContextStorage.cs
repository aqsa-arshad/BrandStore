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
using System.Collections;
using System.Web;

namespace AspDotNetStorefront.Promotions.Data
{
	public interface ContextStorage
	{
		Object this[String key] { get; set; }
	}

	public static class ContextStorageController
	{
		public static ContextStorage Current;
	}

	public class SimpleContextStorage : ContextStorage
	{
		#region Fields

		private IDictionary Items = new Dictionary<String, Object>();

		#endregion

		#region Properties

		public Object this[String key]
		{
			get { return Items[key]; }
			set { Items[key] = value; }
		}

		#endregion
	}

	public class HttpContextStorage : ContextStorage
	{
		public Object this[String key]
		{
			get { return HttpContext.Current.Items[key]; }
			set { HttpContext.Current.Items[key] = value; }
		}
	}
}
