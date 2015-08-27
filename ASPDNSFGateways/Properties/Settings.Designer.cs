// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18047
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AspDotNetStorefrontGateways.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://pay.centralpayments.net/soap/v6/payment.asmx")]
        public string AspDotNetStorefrontGateways_CentralPaymentsSvc_Payment {
            get {
                return ((string)(this["AspDotNetStorefrontGateways_CentralPaymentsSvc_Payment"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://api-3t.sandbox.paypal.com/2.0/")]
        public string AspDotNetStorefrontGateways_PayPalSvc_PayPalAPIInterfaceService {
            get {
                return ((string)(this["AspDotNetStorefrontGateways_PayPalSvc_PayPalAPIInterfaceService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://secure.cardia.no/Service/Card/Transaction/1.2/Transaction.asmx")]
        public string AspDotNetStorefrontGateways_CardiaAPI_Transaction {
            get {
                return ((string)(this["AspDotNetStorefrontGateways_CardiaAPI_Transaction"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://www.sagepayments.net/web_services/vterm_extensions/transaction_processing" +
            ".asmx")]
        public string AspDotNetStorefrontGateways_sagepaymentsAPI_TRANSACTION_PROCESSING {
            get {
                return ((string)(this["AspDotNetStorefrontGateways_sagepaymentsAPI_TRANSACTION_PROCESSING"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://www.ppscommerce.net/SmartPayments/transact.asmx")]
        public string AspDotNetStorefrontGateways_PPSCommerce_SmartPayments {
            get {
                return ((string)(this["AspDotNetStorefrontGateways_PPSCommerce_SmartPayments"]));
            }
        }
    }
}
