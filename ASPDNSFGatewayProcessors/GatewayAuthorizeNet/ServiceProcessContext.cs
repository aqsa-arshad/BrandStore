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
using GatewayAuthorizeNet.AuthorizeNetApi;

namespace GatewayAuthorizeNet
{
	internal class ServiceProcessContext
	{
		public ServiceSoap Service;
		public MerchantAuthenticationType MerchantAuthenticationType;

		public ServiceProcessContext()
		{
			String liveUrl = AspDotNetStorefrontCore.AppLogic.AppConfig("AUTHORIZENET_Cim_LiveServiceURL");
			String testUrl = AspDotNetStorefrontCore.AppLogic.AppConfig("AUTHORIZENET_Cim_SandboxServiceURL");
			String merchantId = AspDotNetStorefrontCore.AppLogic.AppConfig("AUTHORIZENET_X_Login");
			String transactionKey = AspDotNetStorefrontCore.AppLogic.AppConfig("AUTHORIZENET_X_Tran_Key");
            bool sandboxEnabled = AspDotNetStorefrontCore.AppLogic.AppConfigBool("AUTHORIZENET_Cim_UseSandbox");

			System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
			binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;

			string url = liveUrl;
            if (sandboxEnabled)
				url = testUrl;

			System.ServiceModel.EndpointAddress endpointAddress = new System.ServiceModel.EndpointAddress(url);
	
			Service = new ServiceSoapClient(binding, endpointAddress);

			MerchantAuthenticationType = ServiceTools.CreateMerchantAuthenticationType(merchantId, transactionKey);
		}
	}
}
