// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vortx.OnePageCheckout.Models;
using Vortx.OnePageCheckout.Views;
using Vortx.OnePageCheckout.Settings;

namespace Vortx.OnePageCheckout
{
	public class ObjectFactory
	{
        public static IModelFactory CreateModelFactory(AspDotNetStorefront.SkinBase page)
        {
            return (IModelFactory) new Vortx.OnePageCheckout.Models.Adnsf9200.AdnsfModelFactory(page.OPCCustomer, page.OPCShoppingCart);
        }

        public static IConfigurationProviderFactory CreateConfigurationFactory()
        {
            return (IConfigurationProviderFactory) new Vortx.OnePageCheckout.Models.Adnsf9200.Settings.AppConfigProviderFactory();
        }
	}
}
