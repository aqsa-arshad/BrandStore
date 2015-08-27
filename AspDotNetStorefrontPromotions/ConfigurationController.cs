// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Linq;
using System.Data.Linq;

#endregion

namespace AspDotNetStorefront.Promotions.Data
{
	/// <summary>
	///  The ConfigurationController class is responsible for pulling ADNSF specific configuration.
	/// </summary>
	public static class ConfigurationController
	{
		#region Public Methods

		/// <summary>
		///   Retrieves a configuration value from the AppConfig table by Name.
		/// </summary>
		/// <param name="dataContext">Data context to the ADNSF database.</param>
		/// <param name="configName">Name of the app config to retrieve the value for.</param>
		/// <returns>The value of the app config.</returns>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="InvalidOperationException" />
		public static String GetConfigValueByName (DataContext dataContext, String configName)
		{
			if (dataContext == null)
				throw new ArgumentNullException("dataContext", "Data context cannot be null or empty.");

			if (String.IsNullOrEmpty(configName))
				throw new ArgumentNullException("configName", "Config name cannot be null or empty.");

			String retVal = String.Empty;

			AppConfig appConfig = dataContext.GetTable<AppConfig>().FirstOrDefault(ac => ac.Name == configName);
			if (appConfig != null)
				retVal = appConfig.ConfigValue;

			return retVal;
		}

		#endregion
	}
}
