// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web.Profile;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Web;
using System.Collections;
using System.Web.UI;
using AspDotNetStorefrontCore;

// Aspdnsf custom Profile Provider.
namespace AspDotNetStorefront
{
    /// <summary>
    /// Aspdnsf custom Profile Provider storage system, whereby customer-specific preferences can be stored in SQL Server, rather than in cookies.
    /// </summary>
    public class ASPDNSFProfileProvider : System.Web.Profile.ProfileProvider
    {
        #region Class Variables

        private string applicationName;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the application.
        /// </summary>
        /// <returns>String</returns>
        public override string ApplicationName
        {
            get
            {
                return applicationName;
            }

            set
            {
                applicationName = value;
            }
        }
        #endregion

        #region Implemented Abstract Methods from ProfileProvider

        /// <summary>
        /// Initialize the provider.
        /// </summary>
        /// <param name="name">Name of the provider.</param>
        /// <param name="config">Configuration settings.</param>
        /// <remarks></remarks>
        public override void Initialize(
         string name,
         System.Collections.Specialized.NameValueCollection config
        )
        {
            // Initialize values from web.config.
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if ((name == null) || (name.Length == 0))
            {
                name = "AspdnsfProfileProvider";
            }

            if ((config["applicationName"] == null) || String.IsNullOrEmpty(config["applicationName"]))
            {
                applicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                applicationName = config["applicationName"];
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);
        }


        /// <summary>
        /// Gets the profile property values.
        /// </summary>
        /// <param name="context">Profile context.</param>
        /// <param name="settingsProperties">The profile settings properties.</param>
        /// <returns></returns>
        public override SettingsPropertyValueCollection GetPropertyValues(System.Configuration.SettingsContext context, SettingsPropertyCollection settingsProperties)
        {
            string username = (string)context["UserName"];
            SettingsPropertyValueCollection settingsValues = new SettingsPropertyValueCollection();


            // let's just do 1 read to the database and match up the relevant fields
            Dictionary<string, object> pairs = GetPropertValuePairs(username);

            foreach (SettingsProperty property in settingsProperties)
            {
                SettingsPropertyValue pv = new SettingsPropertyValue(property);

                if (pairs.ContainsKey(pv.Name))
                {
                    pv.PropertyValue = pairs[pv.Name];
                    // just read from the db, assume default value
                    pv.IsDirty = false;
                }

                settingsValues.Add(pv);
            }

            return settingsValues;
        }


        /// <summary>
        /// Sets Profile the property values.
        /// </summary>
        /// <param name="context">Profile context.</param>
        /// <param name="settingsPropertyValues">The settings profile property values.</param>
        public override void SetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyValueCollection settingsPropertyValues)
        {
            string username = (string)context["UserName"];
            bool isAuthenticated = (bool)context["IsAuthenticated"];
            int userID = GetUniqueID(username, isAuthenticated);

            foreach (SettingsPropertyValue pv in settingsPropertyValues)
            {
                if (pv.IsDirty)
                {
                    SetProperty(username, userID, isAuthenticated, (string)pv.Name, (string)pv.PropertyValue);
                }
            }

        }

        /// <summary>
        /// Deletes profiles that have been inactive since the specified date.
        /// </summary>
        /// <param name="authenticationOption">Current authentication option setting.</param>
        /// <param name="userInactiveSinceDate">Inactivity date for deletion.</param>
        /// <returns>Number of records deleted.</returns>
        public override int DeleteInactiveProfiles(System.Web.Profile.ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            try
            {
                DB.ExecuteSQL("Delete from Profile where UpdatedOn < '" + userInactiveSinceDate + "'");
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// *Note Currently not supported -- Delete profiles for an array of user names.
        /// </summary>        
        public override int DeleteProfiles(string[] userNames)
        {
            return 0;
        }

        /// <summary>
        /// *Note Currently not supported -- Delete profiles based upon the user names in the collection of profiles.
        /// </summary>        
        public override int DeleteProfiles(System.Web.Profile.ProfileInfoCollection profiles)
        {
            string[] userNames = new string[profiles.Count];
            return DeleteProfiles(userNames);
        }

        /// <summary>
        /// *Note Currently not supported -- Get a collection of profiles based upon a user name matching string and inactivity date.
        /// </summary>        
        public override System.Web.Profile.ProfileInfoCollection FindInactiveProfilesByUserName(System.Web.Profile.ProfileAuthenticationOption authenticationOption, string userNameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            return GetProfileInfo(
            authenticationOption,
            userNameToMatch,
            userInactiveSinceDate,
            pageIndex,
            pageSize,
            out totalRecords
            );

        }

        /// <summary>
        /// *Note Currently not supported -- Get a collection of profiles based upon a user name matching string.
        /// </summary>       
        public override System.Web.Profile.ProfileInfoCollection FindProfilesByUserName(System.Web.Profile.ProfileAuthenticationOption authenticationOption, string userNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return GetProfileInfo(
            authenticationOption,
            userNameToMatch,
            null,
            pageIndex,
            pageSize,
            out totalRecords
            );

        }

        /// <summary>
        /// *Note Currently not supported -- Get a collection of profiles based upon an inactivity date.
        /// </summary>       
        public override System.Web.Profile.ProfileInfoCollection GetAllInactiveProfiles(System.Web.Profile.ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            return GetProfileInfo(
            authenticationOption,
            null,
            userInactiveSinceDate,
            pageIndex,
            pageSize,
            out totalRecords
            );

        }

        /// <summary>
        /// *Note Currently not supported -- Get a collection of profiles.
        /// </summary>      
        public override System.Web.Profile.ProfileInfoCollection GetAllProfiles(System.Web.Profile.ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            return GetProfileInfo(
            authenticationOption,
            null,
            null,
            pageIndex,
            pageSize,
            out totalRecords
            );

        }

        /// <summary>
        /// *Note Currently not supported -- Get the number of inactive profiles based upon an inactivity date.
        /// </summary>    
        public override int GetNumberOfInactiveProfiles(System.Web.Profile.ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int inactiveProfiles = 0;

            GetProfileInfo(authenticationOption, null, userInactiveSinceDate, 0, 0, out inactiveProfiles);

            return inactiveProfiles;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Sets Profile property value.
        /// </summary>
        /// <param name="username">Authentication user name(customer Guid).</param>
        /// <param name="uniqueID">Customer Id.</param>
        /// <param name="isAuthenticated">if set to <c>false</c> [is Anonymous user].</param>
        /// <param name="pname">The profile property name.</param>
        /// <param name="pvalue">The profile property value.</param>
        private void SetProperty(string username, int uniqueID, Boolean isAuthenticated, string pname, string pvalue)
        {

            String err = String.Empty;
            using (SqlConnection cn = new SqlConnection(DB.GetDBConn()))
            {

                cn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.aspdnsf_SetProfileProperties";

                    cmd.Parameters.Add(new SqlParameter("@storeid", SqlDbType.Int));
                    cmd.Parameters.Add(new SqlParameter("@customerid", SqlDbType.Int));
                    cmd.Parameters.Add(new SqlParameter("@CustomerGUID", SqlDbType.UniqueIdentifier));
                    cmd.Parameters.Add(new SqlParameter("@isAuthenticated", SqlDbType.Bit));
                    cmd.Parameters.Add(new SqlParameter("@PropertyNames", SqlDbType.NVarChar, 256));
                    cmd.Parameters.Add(new SqlParameter("@PropertyValuesString", SqlDbType.NVarChar, 256));

                    Guid pGuid = new Guid(username);

                    cmd.Parameters["@storeid"].Value = AppLogic.StoreID();
                    cmd.Parameters["@customerid"].Value = uniqueID;
                    cmd.Parameters["@CustomerGUID"].Value = pGuid;
                    cmd.Parameters["@isAuthenticated"].Value = isAuthenticated;
                    cmd.Parameters["@PropertyNames"].Value = pname;
                    cmd.Parameters["@PropertyValuesString"].Value = pvalue;

                    cmd.ExecuteNonQuery();                    
                }
            }

         }

        /// <summary>
        /// Query profile information for the current user.
        /// </summary>
        /// <param name="userName">GUID of the current user.</param>
        /// <returns>Profile information of the current user.</returns>
        private Dictionary<string, object> GetPropertValuePairs(string userName)
        {
            string query = string.Format("select PropertyName, PropertyValueString from dbo.profile with(nolock) where CustomerGUID = {0} and StoreID = {1}", DB.SQuote(userName),AppLogic.StoreID());

            Dictionary<string, object> propertyValues = new Dictionary<string, object>();

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (IDataReader rs = DB.GetRS(query, conn))
                {
                    while (rs.Read())
                    {
                        string pName = DB.RSField(rs, "PropertyName");
                        object pValue = rs["PropertyValueString"];

                        if (propertyValues.ContainsKey(pName) == false)
                        {
                            propertyValues.Add(pName, pValue);
                        }
                    }

                    rs.Close();
                    rs.Dispose();
                }
                conn.Close();
                conn.Dispose();
            }

            return propertyValues;
        }

        /// <summary>
        /// Gets the profile property value.
        /// </summary>
        /// <param name="username">Authentication user name(customer Guid).</param>
        /// <param name="pname">The profile property name</param>
        /// <returns>profile property value</returns>
        public string GetProperty(string username, string pname)
        {
            string pvalue = "";
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                string query = string.Format("select PropertyValueString from dbo.profile with(nolock) where CustomerGUID = {0} and PropertyName = {1}", DB.SQuote(username), DB.SQuote(pname));
                using (IDataReader rs = DB.GetRS(query, con))
                {
                    if (rs.Read())
                    {
                        pvalue = DB.RSField(rs, "PropertyValueString");
                    }
                }
            }
            return pvalue;
        }

        /// <summary>
        /// Gets the customer unique ID.
        /// </summary>
        /// <param name="username">Authentication user name(customer Guid).</param>
        /// <param name="isAuthenticated">if set to <c>false</c> [is Anonymous user].</param>
        /// <returns>Customer ID</returns>
        private int GetUniqueID(string username, Boolean isAuthenticated)
        {
            int userID = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                using (IDataReader rs = DB.GetRS("select CustomerID from dbo.Customer where CustomerGUID = '" + username + "'", con))
                {
                    if (rs.Read())
                    {
                        userID = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }

            return userID;
        }

        /// <summary>
        /// *note method not supported on our custom profile provider, return empty profile
        /// </summary>     
        private ProfileInfoCollection GetProfileInfo(ProfileAuthenticationOption authenticationOption, string usernameToMatch, object userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {

            ProfileInfoCollection profiles = new ProfileInfoCollection();
            ProfileInfo profileInfo = new ProfileInfo("", false, DateTime.MinValue, DateTime.MinValue, 0);
            profiles.Add(profileInfo);

            totalRecords = 0;
            return profiles;
        }

        #endregion

    }
}




