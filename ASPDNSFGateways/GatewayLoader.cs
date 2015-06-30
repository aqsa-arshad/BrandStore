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
using AspDotNetStorefrontGateways.Processors;
using System.Reflection;
using AspDotNetStorefrontCore;
using System.IO;
using System.Web;

namespace AspDotNetStorefrontGateways
{
    /// <summary>
    /// Helper class to load the gateway processor based on a given name
    /// </summary>
    public class GatewayLoader
    {
        /// <summary>
        /// Gets the name of all the gateway classes
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAvailableGatewayNames()
        {
            return GetGatewayProcessorTypes().Select(type => type.Name).Where(s => !String.Equals(s, "Micropay", StringComparison.InvariantCultureIgnoreCase) && !String.Equals(s, "Verisign", StringComparison.InvariantCultureIgnoreCase)).OrderBy(s => s);
        }

        /// <summary>
        /// Gets all the gateway classes that inherit from base class GatewaProcessor
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetGatewayProcessorTypes()
        {
            String CacheName = "GetGatewayProcessorTypes";
            if (AppLogic.CachingOn)
            {
                IEnumerable<Type> c = (IEnumerable<Type>)HttpContext.Current.Cache.Get(CacheName);
                if (c != null)
                {
                    return c;
                }
            }

            IEnumerable<Type> ret;
            // Iterate through all the available types in this assembly
            // checking for Types that inherit from GatewayProcessor
            Assembly asm = Assembly.GetExecutingAssembly();
            ret = asm.GetTypes().Where(type => type.IsSubclassOf(typeof(GatewayProcessor)));

            string[] fileEntries = Directory.GetFiles(CommonLogic.SafeMapPath(@"~\bin\"), "Gateway*.dll");
            foreach (String dllname in fileEntries)
            {
                try
                {
                    asm = Assembly.LoadFrom(dllname);
                    ret = ret.Concat(asm.GetTypes().Where(type => type.IsSubclassOf(typeof(GatewayProcessor))));
                }
                catch (Exception e) {
                    SysLog.LogMessage(dllname + " failed to load.", e.ToString(), MessageTypeEnum.Informational, MessageSeverityEnum.Message); 
                }
            }
            

            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(CacheName, ret, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }

            return ret;   
        }

        /// <summary>
        /// Loads the gateway processor
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GatewayProcessor GetProcessor(string name)
        {
            // lowercase it for caching later
            name = name.ToLowerInvariant();

            #region Paypal Recurring
            if (name.Equals(AppLogic.ro_PMPayPalEmbeddedCheckout.ToLower()))
                name = Gateway.ro_GWPAYFLOWPRO.ToLower();
            #endregion

            GatewayProcessor processor = null;

            try
            {
                // Iterate through all the available types in this assembly
                // checking for Types that inherit from GatewayProcessor                
                var processors = GetGatewayProcessorTypes();                               // Get all the types that inherit from the GatewayProcessor
                Type processorType = processors
                                    .Where(type => type.Name.EqualsIgnoreCase(name))       // get only the one that has a name that starts with the name and perform case-insensitive matching
                                    .FirstOrDefault();                                        

                if (processorType != null)
                {
                    // instantiate the class
                    processor = Activator.CreateInstance(processorType) as GatewayProcessor;
                }
            }
            catch (Exception ex)
            {
                // error finding the type whatever it is, log what happend through the exception message
                Exception logEx = new Exception("Error loading gateway processor: {0}".FormatWith(name), ex);
                SysLog.LogException(logEx, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }

            if (processor == null)
            {
                // no processor type found, log
                SysLog.LogMessage("Gateway Processor not found {0}".FormatWith(name), string.Empty, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
            }

            return processor;
        }
    }
}
