// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Web;
using System.Linq;
using System.Threading;

namespace AspDotNetStorefrontCore
{
    public class AppConfig
    {
        #region private variables
        private int m_Appconfigid;
        private Guid m_Appconfigguid;
        private string m_Name;
        private string m_Description;
        private string m_Configvalue;
        private string m_Groupname;
        private bool m_Superonly;
        private DateTime m_Createdon;
        private string _valueType = "string";
        private List<string> _allowableValues = new List<string>();

        #endregion

        #region contructors
        public AppConfig() 
        {
            this.StoreId = 1; // force default
        }


        public AppConfig(int AppConfigID) : this() // chain the default constructor to initialize field defaults
        {
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getAppconfig " + AppConfigID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_Appconfigid = DB.RSFieldInt(rs, "AppConfigID");
                        m_Appconfigguid = DB.RSFieldGUID2(rs, "AppConfigGUID");
                        m_Name = DB.RSField(rs, "Name");
                        m_Description = DB.RSField(rs, "Description");
                        m_Configvalue = DB.RSField(rs, "ConfigValue").Trim();
                        m_Groupname = DB.RSField(rs, "GroupName");
                        m_Superonly = DB.RSFieldBool(rs, "SuperOnly");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                        StoreId = rs.FieldInt("StoreId");
                        _valueType = rs.Field("ValueType");
                        AllowableValues.AddCommaDelimited(rs.Field("AllowableValues"));
                    }
                }
            }            
        }

        public AppConfig(int AppConfigID, Guid Appconfigguid, string Name, string Description, string Configvalue, string Groupname, bool Superonly, DateTime Createdon)
            : this() // chain the default constructor to initialize field defaults
        {
            m_Appconfigid = AppConfigID;
            m_Appconfigguid = Appconfigguid;
            m_Name = Name;
            m_Description = Description;
            m_Configvalue = Configvalue;
            m_Groupname = Groupname;
            m_Superonly = Superonly;
            m_Createdon = Createdon;
            _valueType = "string"; // default
        }

        public AppConfig(int AppConfigID, Guid Appconfigguid, string Name, string Description, string Configvalue, string Groupname, bool Superonly, DateTime Createdon, string valueType)
            : this() // chain the default constructor to initialize field defaults
        {
            m_Appconfigid = AppConfigID;
            m_Appconfigguid = Appconfigguid;
            m_Name = Name;
            m_Description = Description;
            m_Configvalue = Configvalue;
            m_Groupname = Groupname;
            m_Superonly = Superonly;
            m_Createdon = Createdon;
            _valueType = valueType; // default
        }

        public AppConfig(int AppConfigID, Guid Appconfigguid, string Name, string Description, string Configvalue, string Groupname, bool Superonly, DateTime Createdon, string valueType, List<string> AllowedValues)
            : this() // chain the default constructor to initialize field defaults
        {
            m_Appconfigid = AppConfigID;
            m_Appconfigguid = Appconfigguid;
            m_Name = Name;
            m_Description = Description;
            m_Configvalue = Configvalue;
            m_Groupname = Groupname;
            m_Superonly = Superonly;
            m_Createdon = Createdon;
            _valueType = valueType; // default
            _allowableValues = AllowedValues;
        }
        
        #endregion

        #region static methods

        public static AppConfig Create(string Name, string Description, string ConfigValue, string GroupName, bool SuperOnly)
        {
			return Create(Name, Description, ConfigValue, null, null, GroupName, SuperOnly, AppLogic.StoreID());
        }

		public static AppConfig Create(string Name, string Description, string ConfigValue, string GroupName, bool SuperOnly, int storeId)
		{
			return Create(Name, Description, ConfigValue, null, null, GroupName, SuperOnly, storeId);
		}

		public static AppConfig Create(string Name, string Description, string ConfigValue, string valueType, string allowableValues, string GroupName, bool SuperOnly, int storeId)
        {
            int AppConfigID = 0;

            ConfigValue = ConfigValue.Trim();

            if (Name.Trim().Length == 0)
            {
                return null;
            }

            if (GroupName.Trim().Length == 0)
            {
                GroupName = "Custom";
            }

			if(valueType == null || valueType.Trim().Length == 0)
				valueType = "string";

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insAppconfig";

            cmd.Parameters.Add(new SqlParameter("@Name",			SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Description",		SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ConfigValue",		SqlDbType.NVarChar, 4000));
            cmd.Parameters.Add(new SqlParameter("@GroupName",		SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@SuperOnly",		SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@StoreId",			SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@ValueType",		SqlDbType.NVarChar, 100));
			cmd.Parameters.Add(new SqlParameter("@AllowableValues", SqlDbType.NVarChar, -1));
			cmd.Parameters.Add(new SqlParameter("@AppConfigID",		SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@Name"].Value = Name;
            cmd.Parameters["@Description"].Value = Description;
            cmd.Parameters["@ConfigValue"].Value = ConfigValue;
            cmd.Parameters["@GroupName"].Value = GroupName;
            cmd.Parameters["@SuperOnly"].Value = SuperOnly;
            cmd.Parameters["@StoreId"].Value = storeId;
			cmd.Parameters["@ValueType"].Value = valueType;
			cmd.Parameters["@AllowableValues"].Value = allowableValues;

            try
            {
                cmd.ExecuteNonQuery();
                AppConfigID = Int32.Parse(cmd.Parameters["@AppConfigID"].Value.ToString());
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            if (AppConfigID > 0)
            {
                AppConfig a = new AppConfig(AppConfigID);
                return a;
            }
            return null;

        }

        public static string Update(int AppConfigID, SqlParameter[] spa)
        {
            string err = String.Empty;

            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updAppconfig";

            SqlParameter sqlparam = new SqlParameter("@AppConfigID", SqlDbType.Int, 4);
            sqlparam.Value = AppConfigID;
            cmd.Parameters.Add(sqlparam);
            foreach (SqlParameter sp in spa)
            {
                cmd.Parameters.Add(sp);
            }
            try
            {

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();
            return err;

        }

        public static string Update(int AppConfigID, string Description, string ConfigValue, string GroupName, object SuperOnly)
        {
            return Update(AppConfigID, Description, ConfigValue, GroupName, SuperOnly, AppLogic.StoreID());
        }

        public static string Update(int AppConfigID, string Description, string ConfigValue, string GroupName, object SuperOnly, int storeId)
        {
            return Update(AppConfigID, Description, ConfigValue, GroupName, SuperOnly, storeId, "string", string.Empty);
        }

        public static string Update(int AppConfigID, string Description, string ConfigValue, string GroupName, object SuperOnly, int storeId, string valueType, string allowableValuesCommaDelimited)
        {
            Customer c = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
            ConfigValue = ConfigValue.Trim();
            AppConfig a = AppLogic.GetAppConfig(storeId, AppConfigID);
            if (a.SuperOnly)
            {
                Security.LogEvent("AppConfig Updated", "Parameter Changed: " + a.Name, 0, c.CustomerID, Convert.ToInt32(c.CurrentSessionID));
            }
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updAppconfig";

            cmd.Parameters.Add(new SqlParameter("@AppConfigID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@ConfigValue", SqlDbType.NVarChar, 4000));
            cmd.Parameters.Add(new SqlParameter("@GroupName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@SuperOnly", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ValueType", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@AllowableValues", SqlDbType.NVarChar, 10000));
            
            cmd.Parameters["@AppConfigID"].Value = AppConfigID;

            if (Description == null) cmd.Parameters["@Description"].Value = DBNull.Value;
            else cmd.Parameters["@Description"].Value = Description;

            if (ConfigValue == null) cmd.Parameters["@ConfigValue"].Value = DBNull.Value;
            else cmd.Parameters["@ConfigValue"].Value = ConfigValue;

            if (GroupName == null) cmd.Parameters["@GroupName"].Value = DBNull.Value;
            else cmd.Parameters["@GroupName"].Value = GroupName;

            if (SuperOnly == null) cmd.Parameters["@SuperOnly"].Value = DBNull.Value;
            else cmd.Parameters["@SuperOnly"].Value = SuperOnly;

            if (SuperOnly == null) cmd.Parameters["@StoreID"].Value = DBNull.Value;
            else cmd.Parameters["@StoreID"].Value = storeId;

            if (SuperOnly == null) cmd.Parameters["@ValueType"].Value = DBNull.Value;
            else cmd.Parameters["@ValueType"].Value = valueType;

            if (SuperOnly == null) cmd.Parameters["@AllowableValues"].Value = DBNull.Value;
            else cmd.Parameters["@AllowableValues"].Value = allowableValuesCommaDelimited;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();
            return err;

        }

        public static AppConfigType ParseTypeFromString(String type)
        {
            foreach (AppConfigType t in Enum.GetValues(typeof(AppConfigType)))
            {
                if (type.EqualsIgnoreCase(t.ToString()))
                    return t;
            }

            return AppConfigType.@string;
        }

        #endregion

        #region Public Methods
        public string Update(SqlParameter[] spa)
        {
            return AppConfig.Update(this.AppConfigID, spa);
        }

        public string Update(string Description, string ConfigValue, string GroupName, object SuperOnly)
        {
            return Update(Description, ConfigValue, GroupName, SuperOnly, AppLogic.StoreID());
        }

        public string Update(string Description, string ConfigValue, string GroupName, object SuperOnly, int storeId)
        {
            string err = String.Empty;
            ConfigValue = ConfigValue.Trim();
            try
            {
                err = Update(this.m_Appconfigid, Description, ConfigValue, GroupName, SuperOnly, storeId);
                if (err == "")
                {
                    m_Description = CommonLogic.IIF(Description != null, Description, m_Description);
                    m_Configvalue = CommonLogic.IIF(ConfigValue != null, ConfigValue, m_Configvalue);
                    m_Groupname = CommonLogic.IIF(GroupName != null, GroupName, m_Groupname);
                    if (SuperOnly != null)
                    {
                        m_Superonly = (bool)SuperOnly;
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            return err;

        }

        public int ToUSInt()
        {
            return Localization.ParseUSInt(this.ConfigValue);
        }


        public int ToNativeInt()
        {
            return Localization.ParseNativeInt(this.ConfigValue);
        }


        public bool ToBool()
        {
            String tmp = this.ConfigValue;
            return (tmp.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
                tmp.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
                tmp.Equals("1", StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion

        #region public properties
        public int AppConfigID
        {
            get { return m_Appconfigid; }
        }

        public Guid AppConfigGUID
        {
            get { return m_Appconfigguid; }
        }

        public string Name
        {
            get { return m_Name; }
        }

		/// <summary>
		/// Warning: Setting this property saves to the database.
		/// </summary>
        public string Description
        {
            get { return m_Description; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@Description", SqlDbType.NText);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_Description = value;
                }
            }
        }

		/// <summary>
		/// Warning: Setting this property saves to the database.
		/// </summary>
        public string ConfigValue
        {
            get { return m_Configvalue.Trim(); }
            set
            {

                SqlParameter sp1 = new SqlParameter("@ConfigValue", SqlDbType.NVarChar, 4000);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_Configvalue = value;
                }
            }
        }

		/// <summary>
		/// Warning: Setting this property saves to the database.
		/// </summary>
        public string GroupName
        {
            get { return m_Groupname; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@GroupName", SqlDbType.NVarChar, 200);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_Groupname = value;
                }
            }
        }

		/// <summary>
		/// Warning: Setting this property saves to the database.
		/// </summary>
        public bool SuperOnly
        {
            get { return m_Superonly; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@SuperOnly", SqlDbType.TinyInt);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_Superonly = value;
                }
            }
        }

        public DateTime CreatedOn
        {
            get { return m_Createdon; }
        }

        private int m_storeid;

        /// <summary>
        /// Gets or sets the StoreId
        /// </summary>
        public int StoreId
        {
            get { return m_storeid; }
            set { m_storeid = value; }
        }

		/// <summary>
		/// Warning: Setting this property saves to the database.
		/// </summary>
        public string ValueType 
        {
            get { return _valueType; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@ValueType", SqlDbType.NVarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    _valueType = value;
                }
            }
        }

		/// <summary>
		/// Warning: Setting this property saves to the database.
		/// </summary>
        public List<string> AllowableValues 
        {
            get { return _allowableValues; } 
            set
            {
                // for collection types, we wouldn't have added a set accessor
                // but for appconfigs which directly update the database whenever you touch
                // the property, let's do it here

                SqlParameter sp1 = new SqlParameter("@AllowableValues", SqlDbType.NVarChar, 10000);
                sp1.Value = value.ToCommaDelimitedString(); // parse into comma delimited string to save to db
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    _allowableValues = value;
                }
            }
        }

        private AppConfigs m_owner;
        public AppConfigs Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }

        #endregion
    }

    public class AppConfigs : IEnumerable, IEnumerable<AppConfig>
    {
        public AppConfigs(Dictionary<string, AppConfig> Data): base() 
        {
            this.data = Data;
        }
        public AppConfigs()
        {}

        // Linq cannot recognize if we both inherit from dictionary and 
        // directly implement IEnumerable<AppConfig> (so we can enumerate through the values instead of the keys)
        // so we have to wrap the data container instead and expose it's methods through public members
        protected Dictionary<string, AppConfig> data = new Dictionary<string, AppConfig>();

        public void Add(AppConfig config)
        {
            string key = config.Name.ToLowerInvariant();

            if (data.ContainsKey(key))
            {
                // if duplicate appconfig was found, remove the old one in favor of the newer one
                AppConfig oldConfig = data[key];
                string header = "Duplicate AppConfig found: {0}".FormatWith(key);
                string details = @"
                                    Overwriting old appconfig with the new
                                    Old Value: {0}
                                    New Value: {1}".FormatWith(oldConfig.ConfigValue, config.ConfigValue);
                SysLog.LogMessage(header, details, MessageTypeEnum.Informational, MessageSeverityEnum.Message);
                data.Remove(key);
            }

            data.Add(key, config);
            config.Owner = this;
        }

        public AppConfig this[string name]
        {
            get 
            {
                name = name.ToLowerInvariant();

                AppConfig config = null;
                // perform checking first before directly trying to get the value
                // so as not to throw error if key is not existing
                if (data.ContainsKey(name))
                {
                    config = data[name];
                }

                // allow for null values which means it's not present in this collection
                return config;
            }
        }

        public AppConfig this[int appConfigId]
        {
            get
            {
                var found = data.Values.FirstOrDefault(config => config.AppConfigID == appConfigId);
                return found;
            }
        }

        public int Count
        {
            get { return data.Count; }
        }

        /// <summary>
        /// Override the default behavior to enumerate through the values instead of the keys
        /// </summary>
        /// <returns></returns>
        public IEnumerator<AppConfig> GetEnumerator()
        {
            return data.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public AppConfig Add(string Name, string Description, string ConfigValue, string GroupName, bool SuperOnly)
        {
            return Add(Name, Description, ConfigValue, null, null, GroupName, SuperOnly, AppLogic.StoreID());
        }

        /// <summary>
        /// Creates a new AppConfig record and adds it to thepp collection
        /// </summary>
        public AppConfig Add(string Name, string Description, string ConfigValue, string GroupName, bool SuperOnly, int storeId)
        {
			return Add(Name, Description, ConfigValue, null, null, GroupName, SuperOnly, storeId);
		}

		public AppConfig Add(string Name, string Description, string ConfigValue, string valueType, string allowableValues, string GroupName, bool SuperOnly, int storeId)
		{
			var config = AppConfig.Create(Name, Description, ConfigValue.Trim(), valueType, allowableValues, GroupName, SuperOnly, storeId);
			this.Add(config);

			return config;
		}

        public void Remove(AppConfig config)
        {
            this.Remove(config.Name);
        }

        /// <summary>
        /// Deletes the AppConfig record and removes the item from the collection
        /// </summary>
        public void Remove(string name)
        {
            try
            {
                name = name.ToLowerInvariant();

                DB.ExecuteSQL("delete dbo.appconfig where appconfigid = " + this[name].AppConfigID.ToString());
                data.Remove(name);
            }
            catch { }
        }
        
    }

    public static class AppConfigManager
    {
        private static Dictionary<int, AppConfigs> storeConfigs = new Dictionary<int, AppConfigs>();

        /// <summary>
        /// Ensures that we have a appconfig collection, even an empty one, for a given store
        /// </summary>
        /// <param name="data"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        private static AppConfigs EnsureStoreConfigsExists(int storeId)
        {
            return EnsureStoreConfigsExists(storeConfigs, storeId);
        }

        /// <summary>
        /// Ensures that we have a appconfig collection, even an empty one, for a given store
        /// </summary>
        /// <param name="data"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        private static AppConfigs EnsureStoreConfigsExists(Dictionary<int, AppConfigs> data, int storeId)
        {
            if (!data.ContainsKey(storeId))
            {
                data.Add(storeId, new AppConfigs());
            }

            return data[storeId];
        }

        public static AppConfig CreateDBAndCacheAppConfig(AppConfig config)
        {
            StringBuilder allowableValues = new StringBuilder();
            for (int i = 0; i < config.AllowableValues.Count; i++)
            {
                if (i != 0)
                    allowableValues.Append(",");
                allowableValues.Append(config.AllowableValues[i]);
            }
            return CreateDBAndCacheAppConfig(config.Name, config.Description, config.ConfigValue, config.ConfigValue, allowableValues.ToString(), config.GroupName, config.SuperOnly, config.StoreId);
        }

        public static AppConfig CreateDBAndCacheAppConfig(string Name, string Description, string ConfigValue, string valueType, string allowableValues, string GroupName, bool SuperOnly, int storeId)
        {
            if (AppLogic.AppConfigExists(Name, storeId))
                return null;

            EnsureStoreConfigsExists(storeId);

            AppConfig newConfig = AspDotNetStorefrontCore.AppConfig.Create(Name, Description, ConfigValue, valueType, allowableValues, GroupName, SuperOnly, storeId);

            storeConfigs[storeId].Add(newConfig);
            return newConfig;
        }

        /// <summary>
        /// Gets if any of the configs has loaded
        /// </summary>
        /// <returns></returns>
        public static bool HasConfigsLoaded()
        {
            return storeConfigs.Any(store => store.Value.Count > 0);
        }

        /// <summary>
        /// Thread safe lock object
        /// </summary>
        private static object syncLock = new object();

        public static void LoadAllConfigs()
        {
            lock (syncLock)
            {
                // make a temporary copy during loading, this way we don't touch the main config files
                // for other threads trying to access it during loading/reloading time
                var loadConfigs = new Dictionary<int, AppConfigs>();                

                Action<IDataReader> readAction = (rs) =>
                {
                    while (rs.Read())
                    {
                        AppConfig config = new AppConfig(rs.FieldInt("AppConfigID"),
                                        rs.FieldGuid("AppConfigGUID"),
                                        rs.Field("Name"),
                                        rs.Field("Description"),
                                        rs.Field("ConfigValue"),
                                        rs.Field("GroupName"),
                                        rs.FieldBool("SuperOnly"),
                                        rs.FieldDateTime("CreatedOn"), 
                                        rs.Field("ValueType"));

                        // these 2 fields are guaranteed to not automatically update the database upon setting property values
                        config.StoreId = rs.FieldInt("StoreId");
                        config.AllowableValues.AddCommaDelimited(rs.Field("AllowableValues"));
                        
                        AppConfigs configs = EnsureStoreConfigsExists(loadConfigs, config.StoreId);
                        configs.Add(config);
                    }
                };

                DB.UseDataReader("dbo.aspdnsf_getAppconfig", readAction);

                // clear the master data repository
                storeConfigs.Clear();
                
                // push the config data to the main repository
                foreach (int storeId in loadConfigs.Keys)
                {
                    AppConfigs configs = loadConfigs[storeId];
                    storeConfigs.Add(storeId, configs);
                }
            }
        }

        public static AppConfigs GetAppConfigCollection(int storeId, Boolean IncludeSuperConfigs)
        {
            if (storeConfigs.ContainsKey(storeId))
            {
                if (IncludeSuperConfigs)
                {
                    return storeConfigs[storeId];
                }
                else
                {
                    AppConfigs nonSuperAppConfigs = new AppConfigs(storeConfigs[storeId].Where(c => c.SuperOnly == false).ToDictionary(item => item.Name, item => item));
                    return nonSuperAppConfigs;
                }
            }

            var empty = new AppConfigs();
            return empty;
        }

        public static string AppConfig(int storeId, String paramName, Boolean cascadeToDefault)
        {
            var configs = EnsureStoreConfigsExists(storeId);
            var config = configs[paramName.ToLowerInvariant()];
            if (config != null)
            {
                return config.ConfigValue;
            }
            else
            {
                if (!cascadeToDefault || storeId == 0)
                    return string.Empty;
                else
                {
                    return AppConfig(0, paramName, false);
                }
            }
        }

        public static AppConfig GetAppConfig(int storeId, string paramName)
        {
            var configs = EnsureStoreConfigsExists(storeId);
            return configs[paramName];
        }

        public static Boolean SetAppConfigDBAndCache(int storeId, String paramName, String value)
        {
            return SetAppConfigDBAndCache(storeId, paramName, value, new List<string>());
        }
        public static Boolean SetAppConfigDBAndCache(int storeId, String paramName, String value, List<string> dontDupValues)
        {
            AppConfigs configs = EnsureStoreConfigsExists(storeId);
            try 
	        {
                if (configs[paramName] != null)
                    configs[paramName].ConfigValue = value;
                else
                {
                    if (dontDupValues.Contains(value))
                        return false;

                    AppConfig newConfig = DuplicateAppConfigForStore(storeId, paramName, value);
                    if (newConfig == null)
                        return false;
                    configs.Add(newConfig);
                }
                return true;
	        }
	        catch (Exception)
	        {
		        return false;
	        }
        }

        public static AppConfig DuplicateAppConfigForStore(int storeId, string paramName, string value)
        {
            AppConfig DefaultAppConfig = AppLogic.GetAppConfigRouted(paramName, storeId);
            if (DefaultAppConfig == null)
                return null;

            string defaultValue = DefaultAppConfig.ConfigValue.ToLower();
            string newValue = value.ToLower();
            if (DefaultAppConfig.ValueType.EqualsIgnoreCase("multiselect"))
            {
                defaultValue = defaultValue.Replace(" ", "").Trim();
                newValue = newValue.Replace(" ", "").Trim();
            }

            if (DefaultAppConfig.ValueType.EqualsIgnoreCase("boolean") || newValue != defaultValue)
            {
                AppConfig newConfig = AspDotNetStorefrontCore.AppConfig.Create(DefaultAppConfig.Name, DefaultAppConfig.Description, value, DefaultAppConfig.GroupName, DefaultAppConfig.SuperOnly, storeId);
                newConfig.ValueType = DefaultAppConfig.ValueType;
                newConfig.AllowableValues = DefaultAppConfig.AllowableValues;
                return newConfig;
            }
            return null;
        }

        public static AppConfig GetAppConfig(int storeId, int id)
        {
            var configs = EnsureStoreConfigsExists(storeId);
            return configs[id];
        }


        /// <summary>
        /// Determines if a certain appconfig exists on all stores
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static bool AppConfigExists(string paramName)
        {
            foreach (var storeId in storeConfigs.Keys)
            {
                if (AppConfigExists(storeId, paramName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if a certain appconfig exists per store
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static bool AppConfigExists(int storeId, string paramName)
        {
            var config = GetAppConfig(storeId, paramName);
            return config != null;
        }

        public static int ALL_STORES = -1;

        public static IEnumerable<string> GetDistinctAppConfigGroups(int storeId)
        {
            var distinctGroupNames = new List<string>();

            var configs = GetAppConfigCollection(storeId, true);
            if (configs != null)
            {
                distinctGroupNames = configs.OrderBy(config => config.GroupName).Select(config => config.GroupName).Distinct().ToList();
            }

            return distinctGroupNames;
        }



        internal static IEnumerable<AppConfig> SearchAppConfigs(string SearchTerm, int StoreId)
        {
            AppConfigs data = EnsureStoreConfigsExists(StoreId);
            return data.Where(d => d.Name.ContainsIgnoreCase(SearchTerm));
        }
    }

    public enum AppConfigType
    {
        @string,
        boolean,
        integer,
        @decimal,
        @double,
        @enum,
        multiselect,
        invoke
    }

}
