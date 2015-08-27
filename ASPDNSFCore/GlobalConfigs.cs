// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AspDotNetStorefrontCore
{
    [Serializable()]
    public class GlobalConfig
    {
        private GlobalConfig(int id,
            Guid _guid, string _name,
            string _description, string _configValue,
            string _groupname, bool _superOnly,
            string _configType, DateTime _createdOn, string _enumValues)
        {
            this.ID = id;
            this.GUID = _guid;
            this.Name = _name;
            this.Description = _description;
            this.ConfigValue = _configValue;
            this.GroupName = _groupname;
            this.ValueType = _configType;
            this.SuperOnly = _superOnly;
            this.CreatedOn = _createdOn;
            this.AllowableValues = new List<string>();// = _enumValues .Split(',');
            this.AllowableValues.AddCommaDelimited(_enumValues);
        }
        
        #region properties

        [NonSerialized]
        private Guid _Guid;

        private enum ValueTypeEnum{
            Bool,
            Integer,
            Decimal,
            StringType,
            Enumeration
        }

        public object GUID
        {
            get { return _Guid; }
            set
            {
                if (value.GetType() == typeof(string))
                {
                    _Guid = new Guid(value.ToString());
                }
                if (value.GetType() == typeof(Guid))
                {
                    _Guid = (Guid)value;
                }
            }
        }

        private int m_id;
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }
        private string m_name;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private string m_description;
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private string m_configvalue;
        public string ConfigValue
        {
            get { return m_configvalue; }
            set { m_configvalue = value; }
        }
        private string m_valuetype;
        public string ValueType
        {
            get { return m_valuetype; }
            set { m_valuetype = value; }
        }

        private string m_groupname;
        public string GroupName
        {
            get { return m_groupname; }
            set { m_groupname = value; }
        }

        private bool m_superonly;
        public bool SuperOnly
        {
            get { return m_superonly; }
            set { m_superonly = value; }
        }
        private bool m_hidden;
        public bool Hidden
        {
            get { return m_hidden; }
            set { m_hidden = value; }
        }

        private DateTime m_createdon;
        public DateTime CreatedOn
        {
            get { return m_createdon; }
            set { m_createdon = value; }
        }

        private bool m_ismultistore;
        public bool IsMultiStore
        {
            get { return m_ismultistore; }
            set { m_ismultistore = value; }
        }

        private List<string> m_allowablevalues;
        public List<string> AllowableValues
        {
            get { return m_allowablevalues; }
            set { m_allowablevalues = value; }
        }

        #endregion

        #region "data methods"

        public void Save()
        {
            string SQL =
 @"
            IF EXISTS (SELECT * FROM GlobalConfig WHERE GlobalConfigGuid = @GUID)
            BEGIN
	            UPDATE GlobalConfig SET 
		            [Name] = @Name,
		            Description = @Description,
		            ConfigValue = @ConfigValue,
		            GroupName = @GroupName,
		            SuperOnly = @SuperOnly,
                    EnumValues = @EnumValues
	            WHERE [GlobalConfigID] = @ID
            END
            ELSE
            BEGIN
	            INSERT INTO GlobalConfig
		            (GlobalConfigGUID, [Name], [Description], [ConfigValue], [ValueType] ,[GroupName], [SuperOnly], [EnumValues])
		            VALUES
		            (NewID(), @Name, @Description, @ConfigValue, @ValueType, @GroupName, @SuperOnly, @EnumValues)
            END
            ";

            // CONVERSION TODO
            // because we're using shortcut syntax for these properties (eg. { get; set; }) without private variables
            // for storing data, when converting to VB you may need to ensure that AddressOf is not added as part of
            // the second parameter object.
            // These properties should be updated to follow our coding standards.
            SqlParameter[] Params = new SqlParameter[]{
              new SqlParameter("@Name", this.Name),
              new SqlParameter("@Description", this.Description),
              new SqlParameter("@ConfigValue", this.ConfigValue.ToString()),
              new SqlParameter("@ValueType", this.ValueType),
              new SqlParameter("@EnumValues", this.AllowableValues.ToCommaDelimitedString()),
              new SqlParameter("@GroupName", this.GroupName),
              new SqlParameter("@SuperOnly", this.SuperOnly ? 1: 0),
              new SqlParameter("@GUID", this.GUID),
              new SqlParameter("@ID", this.ID)
            };
            DB.ExecuteSQL(SQL, Params);
        }

        
        private static string sqlRetrieve
        {
            get
            {
                return "SELECT * FROM GlobalConfig WITH (NOLOCK) WHERE HIDDEN = 0 AND ([Name] = @Name OR @Name IS Null)";
            }
        }
        public static GlobalConfig getGlobalConfig(string setting)
        {
            using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
            {
                 xCon.Open();
                //System.Data.IDataReader xRdr= DB.GetRS(sqlRetrieve, xCon);
                 System.Data.IDataReader xRdr = DB.GetRS(sqlRetrieve, new SqlParameter[] { new SqlParameter("@Name", setting) }, xCon);
                while (xRdr.Read())
                {
                    return new GlobalConfig(xRdr.FieldInt("GlobalConfigID"),
                        xRdr.FieldGuid("GlobalConfigGUID"),
                        xRdr.Field("Name"),
                        xRdr.Field("Description"),
                        xRdr.Field("ConfigValue"),
                        xRdr.Field("GroupName"),
                        xRdr.FieldBool("SuperOnly"),
                        xRdr.Field("ValueType"),
                        xRdr.FieldDateTime("CreatedOn"),
                        xRdr.Field("EnumValues"));
                }
            }
            return null;
        }
        public static List<GlobalConfig> getGlobalConfigs()
        {
            List<GlobalConfig> xConfig = new List<GlobalConfig>();
           
            using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
            {
                xCon.Open();
                System.Data.IDataReader xRdr= DB.GetRS
                    (sqlRetrieve, 
                    new SqlParameter[]{new SqlParameter("@Name", DBNull.Value)}, 
                    xCon);
                while (xRdr.Read())
                {
                    GlobalConfig cfg = new GlobalConfig(xRdr.FieldInt("GlobalConfigID"),
                       xRdr.FieldGuid("GlobalConfigGUID"),
                       xRdr.Field("Name"),
                       xRdr.Field("Description"),
                       xRdr.Field("ConfigValue"),
                       xRdr.Field("GroupName"),
                       xRdr.FieldBool("SuperOnly"),
                       xRdr.Field("ValueType"),
                       xRdr.FieldDateTime("CreatedOn"),
                       xRdr.Field("EnumValues"));

                    xConfig.Add(cfg);
                }
            }
            return xConfig;
        }

        /// <summary>
        /// Thread safe lock object
        /// </summary>
        private static object syncLock = new object();

        public static void LoadGlobalConfigs()
        {
            lock (syncLock)
            {
                GlobalConfigCollection configs = new GlobalConfigCollection();
                List<GlobalConfig> globalList = getGlobalConfigs();
                foreach (GlobalConfig xConfig in globalList)
                {
                    configs.Add(xConfig);
                }

                AppLogic.GlobalConfigTable = configs;
            }
        }

        public static Boolean CreateGlobalConfig(string Name, string Group, string Description, string ConfigValue, string ValueType)
        {
            if (GlobalConfig.getGlobalConfig(Name) != null)
                return false;

            SqlParameter[] Params = new SqlParameter[]{
              new SqlParameter("@Name", Name),
              new SqlParameter("@Group", Group.ToUpper()),
              new SqlParameter("@Description", Description),
              new SqlParameter("@ConfigValue", ConfigValue.ToString()),
              new SqlParameter("@ValueType", ValueType),
            };

            String sql = @"INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) VALUES(@Name, @Group, @Description, @ConfigValue, @ValueType, 'false')";

            DB.ExecuteSQL(sql, Params);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Global config collection data structure that provides access via key and index
    /// </summary>
    public class GlobalConfigCollection : IEnumerable, IEnumerable<GlobalConfig>
    {
        private Dictionary<string, GlobalConfig> data = new Dictionary<string, GlobalConfig>();

        /// <summary>
        /// Adds a globalconfig to the collection
        /// </summary>
        /// <param name="config"></param>
        public void Add(GlobalConfig config)
        {
            string key = config.Name.ToLowerInvariant();
            if (data.ContainsKey(key))
            {
                data.Remove(key);
            }

            data.Add(key, config);
        }

        /// <summary>
        /// Gets the global config based on a given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GlobalConfig this[string name]
        {
            get 
            {
                string key = name.ToLowerInvariant();

                GlobalConfig config = null;
                
                if (data.ContainsKey(key))
                {
                    config = data[key];
                }

                return config;
            }
        }
        
        /// <summary>
        /// Gets or sets the globalconfig in this collection via the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GlobalConfig this[int index]
        {
            get
            {
                // since we're using a dictionary data structure
                // by design, it's indexer is based on a key
                // we therefore use Linq's Where method with the overload
                // that specifies the index and match it against the index specified
                GlobalConfig config = data.Values
                            .Where((cfg, idx) => idx == index)
                            .FirstOrDefault();

                return config;
            }
            set
            {
                // because we're using dictionary data structure
                // we need first to find the exisitng config
                // based on that id and remove that one to overwrite
                // with what's specified
                GlobalConfig config = this[index]; // use the get accessor of this indexer
                if (config != null)
                {
                    string key = config.Name.ToLowerInvariant();
                    if (data.ContainsKey(key))
                    {
                        data.Remove(key);
                    }
                }

                Add(value);
            }
        }


        /// <summary>
        /// Override the default behavior to enumerate through the values instead of the keys
        /// </summary>
        /// <returns></returns>
        public IEnumerator<GlobalConfig> GetEnumerator()
        {
            return data.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerable<string> GetDistinctGroupNames()
        {
            return this.Select(config => config.GroupName).Distinct().ToList();
        }

    }
}

