// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AspDotNetStorefront.Promotions
{
	public interface IDataLookupResult
	{
		#region Properties
        
		DateTime DateTimeResult { get; set; }

		Int32 Int32Result { get; set; }

		Decimal DecimalResult { get; set; }

        String StringResult { get; set; }
		
		#endregion
	}

	public class SimpleDataLookupResult : IDataLookupResult
	{
		#region Properties

		public DateTime DateTimeResult { get; set; }

		public Int32 Int32Result { get; set; }

		public Decimal DecimalResult { get; set; }

        public String StringResult { get; set; }

		#endregion
	}
}
