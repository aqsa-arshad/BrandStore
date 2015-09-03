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
	public class Skin
	{
		public int Id { get; private set; }
		public string Name { get; private set; }
		public string DisplayName { get; private set; }
		public string Description { get; private set; }
		public string PreviewUrl { get; private set; }
		public bool IsMobile { get; private set; }

		public Skin(int id, string name, string displayName, string description, string previewUrl, bool isMobile)
		{
			Id = id;
			Name = name;
			DisplayName = displayName;
			Description = description;
			PreviewUrl = previewUrl;
			IsMobile = isMobile;
		}
	}
}
