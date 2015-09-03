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
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontExcelWrapper;
using System.Xml;
using System.IO;

namespace AspDotNetStorefrontCore
{
    public class StringResource
    {

        #region private variables

            private int m_Stringresourceid = -1;
            private Guid m_Stringresourceguid = new Guid("00000000000000000000000000000000");
            private string m_Name = string.Empty;
            private string m_Localesetting = string.Empty;
            private string m_Configvalue = string.Empty;
            private DateTime m_Createdon = DateTime.MinValue;
            private bool m_Modified = false;
        
        #endregion

        #region contructors

            public StringResource() 
            {
                StoreId = 1; // force default
            }

            public StringResource(int StringResourceID) : this()
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    using (IDataReader dr = DB.GetRS("aspdnsf_getStringresource " + StringResourceID.ToString(), con))
                    {
                        if (dr.Read())
                        {
                            m_Stringresourceid = DB.RSFieldInt(dr, "StringResourceID");
                            m_Stringresourceguid = DB.RSFieldGUID2(dr, "StringResourceGUID");
                            m_Name = DB.RSField(dr, "Name");
                            m_Localesetting = DB.RSField(dr, "LocaleSetting");
                            m_Configvalue = DB.RSField(dr, "ConfigValue");
                            m_Createdon = DB.RSFieldDateTime(dr, "CreatedOn");
                            m_Modified = DB.RSFieldBool(dr, "Modified");
                            StoreId = dr.FieldInt("StoreId");
                        }
                    }
                }

            }

            public StringResource(int StringResourceID, Guid StringResourceGUID, string Name, string LocaleSetting, string ConfigValue, DateTime CreatedOn, bool Modified) : this()
            {
                m_Stringresourceid = StringResourceID;
                m_Stringresourceguid = StringResourceGUID;
                m_Name = Name;
                m_Localesetting = LocaleSetting;
                m_Configvalue = ConfigValue;
                m_Createdon = CreatedOn;
                m_Modified = Modified;

            }
        
        #endregion

        #region static methods

            /// <summary>
            /// Creates a new StringResource record and returns a StringResource Object.  If the StringResource record cannot be created the returned StringResource object has the error in it's ConfigValue parameter
            /// </summary>
            /// <param name="Name"></param>
            /// <param name="LocaleSetting"></param>
            /// <param name="ConfigValue"></param>
            /// <returns></returns>
            public static StringResource Create(int storeId, string Name, string LocaleSetting, string ConfigValue)
            {
                int StringResourceID = -1;

                string err = String.Empty;
                SqlConnection cn = new SqlConnection(DB.GetDBConn());
                cn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.aspdnsf_insStringresource";

                cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int, 4));
                cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200));
                cmd.Parameters.Add(new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 20));
                cmd.Parameters.Add(new SqlParameter("@ConfigValue", SqlDbType.NVarChar, 4000));
                cmd.Parameters.Add(new SqlParameter("@StringResourceID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

                cmd.Parameters["@StoreId"].Value = storeId;
                cmd.Parameters["@Name"].Value = Name;
                cmd.Parameters["@LocaleSetting"].Value = LocaleSetting;
                cmd.Parameters["@ConfigValue"].Value = ConfigValue;

                try
                {
                    cmd.ExecuteNonQuery();
                    StringResourceID = Int32.Parse(cmd.Parameters["@StringResourceID"].Value.ToString());
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }

                cn.Close();
                cmd.Dispose();
                cn.Dispose();

                if (StringResourceID > 0)
                {
                    StringResource sr = new StringResource(StringResourceID);
                    return sr;
                }
                else
                {
                    return new StringResource(-1, new Guid("00000000000000000000000000000000"), "", "", err, DateTime.MinValue, false);
                }

            }
            
            public static string Update(int storeId, int StringResourceID, string Name, string LocaleSetting, string ConfigValue)
            {
                string err = String.Empty;
                SqlConnection cn = new SqlConnection(DB.GetDBConn());
                cn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.aspdnsf_updStringresource";

                cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int, 4));
                cmd.Parameters.Add(new SqlParameter("@StringResourceID", SqlDbType.Int, 4));
                cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200));
                cmd.Parameters.Add(new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 20));
                cmd.Parameters.Add(new SqlParameter("@ConfigValue", SqlDbType.NVarChar, 4000));

                cmd.Parameters["@StoreId"].Value = storeId;
                cmd.Parameters["@StringResourceID"].Value = StringResourceID;

                if (Name == null) cmd.Parameters["@Name"].Value = DBNull.Value;
                else cmd.Parameters["@Name"].Value = Name;

                if (LocaleSetting == null) cmd.Parameters["@LocaleSetting"].Value = DBNull.Value;
                else cmd.Parameters["@LocaleSetting"].Value = LocaleSetting;

                if (ConfigValue == null) cmd.Parameters["@ConfigValue"].Value = DBNull.Value;
                else cmd.Parameters["@ConfigValue"].Value = ConfigValue;

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

        #endregion

        #region Public Methods

            public string Update(string Name, string LocaleSetting, string ConfigValue)
            {
                return Update(this.StoreId, Name, LocaleSetting, ConfigValue);
            }

            public string Update(int storeId, string Name, string LocaleSetting, string ConfigValue)
            {
                string err = String.Empty;
                try
                {
                    err = Update(storeId, this.m_Stringresourceid, Name, LocaleSetting, ConfigValue);
                    if (err == "")
                    {
                        this.StoreId = storeId;
                        m_Name = CommonLogic.IIF(Name != null, Name, m_Name);
                        m_Configvalue = CommonLogic.IIF(LocaleSetting != null, ConfigValue, m_Configvalue);
                        m_Localesetting = CommonLogic.IIF(LocaleSetting != null, LocaleSetting, m_Localesetting);
                        m_Modified = true;
                    }
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }

                return err;

            }
            
        #endregion

        #region public properties

            public int StringResourceID
            {
                get { return m_Stringresourceid; }
            }

            public Guid StringResourceGUID
            {
                get { return m_Stringresourceguid; }
            }

            public string Name
            {
                get { return m_Name; }
            }

            public string LocaleSetting
            {
                get { return m_Localesetting; }
            }

            public string ConfigValue
            {
                get { return m_Configvalue; }
            }

            public DateTime CreatedOn
            {
                get { return m_Createdon; }
            }

            public bool Modified
            {
                get { return m_Modified; }
            }

            private int m_storeid;
            public int StoreId
            {
                get { return m_storeid; }
                set { m_storeid = value; }
            }

            private StringResources m_owner;
            public StringResources Owner 
            {
                get { return m_owner; }
                set { m_owner = value; }
            }

        #endregion

    }

    public class StringResources : IEnumerable, IEnumerable<StringResource>
    {
        private Dictionary<string, StringResource> data = new Dictionary<string, StringResource>();

        public void Add(StringResource str)
        {
            string key = string.Format("{0}_{1}", str.LocaleSetting.ToLowerInvariant(), str.Name.ToLowerInvariant());
            if (data.ContainsKey(key))
            {
                // if it's already existing, remove the previous one so that we can overwriteit
                data.Remove(key);
            }
            data.Add(key, str);

            str.Owner = this;
        }

        public StringResource this[string localesetting, string name]
        {
            get
            {
                string key = string.Format("{0}_{1}", localesetting.ToLowerInvariant(), name.ToLowerInvariant());

                StringResource str = null;
                // perform checking first before directly trying to get the value
                // so as not to throw error if key is not existing
                if (data.ContainsKey(key))
                {
                    str = data[key];
                }

                // allow for null values which means it's not present in this collection
                return str;
            }
        }

        public StringResource this[int id]
        {
            get
            {
                var found = data.Values.FirstOrDefault(config => config.StringResourceID == id);
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
        public IEnumerator<StringResource> GetEnumerator()
        {
            return data.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public string Add(int storeId, string Name, string LocaleSetting, string ConfigValue)
        {
            var sr = StringResource.Create(storeId, Name, LocaleSetting, ConfigValue);
            if (sr.StringResourceID == -1)
            {
                return sr.ConfigValue;
            }
            else
            {
                this.Add(sr);
                return string.Empty;
            }
        }

        public void Remove(StringResource str)
        {
            Remove(str.LocaleSetting, str.Name);
        }

        public void Remove(string localesetting, string name)
        {
            try
            {
                var str = this[localesetting, name];
                if (str != null)
                {
                    DB.ExecuteSQL("delete dbo.stringresource where StringResourceID= " + str.StringResourceID.ToString());
                }

                string key = string.Format("{0}_{1}", localesetting.ToLowerInvariant(), name.ToLowerInvariant());

                data.Remove(key);
            }
            catch { }
        }
    }

    public static class StringResourceManager
    {
        private static Dictionary<int, StringResources> storeStrings = new Dictionary<int, StringResources>();

        private static StringResources EnsureStoreStringResourcesExists(int storeId)
        {
            return EnsureStoreStringResourcesExists(storeStrings, storeId);
        }

        private static StringResources EnsureStoreStringResourcesExists(Dictionary<int, StringResources> data, int storeId)
        {
            if (!data.ContainsKey(storeId))
            {
                data.Add(storeId, new StringResources());
            }

            return data[storeId];
        }

        /// <summary>
        /// Thread safe lock object
        /// </summary>
        private static object syncLock = new object();

        public static void LoadAllStrings(bool tryToReload)
        {
            lock(syncLock)
            {
                // temporary data holder upon loading strings
                var data = new Dictionary<int, StringResources>();

                LoadAllStringsFromDB(data);

                if (HasAnyStrings() == false && tryToReload)
                {
                    // no content from db
                    var locales = GetLocales();

                    // get all available locales and check if there's an uploaded excel file for that locale
                    // and then populate the db from the extracted excel file per locale
                    foreach (var locale in locales)
                    {
                        if (tryToReload && HasNoStringResourceInDB(locale))
                        {
                            // there doesn't seem to be any string resources for this locale in the db table, so try to load them from the 
                            // excel spreadsheet for this locale:
                            LoadStringResourceExcelFile(locale);
                        }
                    }

                    // re-read from DB
                    LoadAllStringsFromDB(data);
                }

                // clear the main repository
                storeStrings.Clear();

                // push the read data into the main repository                
                foreach (var storeId in data.Keys)
                {
                    var strings = data[storeId];
                    storeStrings.Add(storeId, strings);
                }
            }
        }

        private static void LoadAllStringsFromDB(Dictionary<int, StringResources> data)
        {
            Action<IDataReader> readAction = (rs) =>
            {
                while (rs.Read())
                {
                    var key = DB.RSField(rs, "LocaleSetting").ToLowerInvariant() + "_" + DB.RSField(rs, "Name").ToLowerInvariant();
                    var str = new StringResource(DB.RSFieldInt(rs, "StringResourceID"), DB.RSFieldGUID2(rs, "StringResourceGUID"), DB.RSField(rs, "Name"), DB.RSField(rs, "LocaleSetting"), DB.RSField(rs, "ConfigValue"), DB.RSFieldDateTime(rs, "CreatedOn"), DB.RSFieldBool(rs, "Modified"));
                    str.StoreId = rs.FieldInt("StoreId");

                    var strings = EnsureStoreStringResourcesExists(data, str.StoreId);
                    strings.Add(str);
                }
            };


            DB.UseDataReader("aspdnsf_getStringresource", readAction);
        }

        public static List<string> GetLocales()
        {
            var locales = new List<string>();            
            Action<IDataReader> readAction = (rs) =>
            {
                while (rs.Read())
                {
                    locales.Add(rs.Field("Name"));
                }
            };

            string query = "select * from LocaleSetting  with (NOLOCK)  order by displayorder,description";
            DB.UseDataReader(query, readAction);

            return locales;
        }

        /// <summary>
        /// Determines if a given locale contains any string resource files in the
        /// [appRoot]/StringResources folder
        /// </summary>
        /// <param name="locale">The locale to look for string resource files for</param>
        /// <returns>Returns true if ANY string resource files exist for a given locale.
        /// Else, returns false.</returns>
        public static bool CheckStringResourceExcelFileExists(string locale)
        {
            return GetStringResourceFilesForLocale(locale).Count > 0;
        }

        /// <summary>
        /// Returns a string array of filenames containing all string resource excel files for a given locale
        /// </summary>
        /// <param name="locale">Locale to retrieve string resource files for</param>
        /// <returns></returns>
        public static List<String> GetStringResourceFilesForLocale(string locale)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(CommonLogic.SafeMapPath("~/stringresources"), "*." + locale + ".xls", SearchOption.TopDirectoryOnly));
            return files;
        }

        static public void LoadStringResourceExcelFile(String LocaleSetting)
        {
            //Find all string resource excel files in <appRoot>/StringResources pertaining to the specified locale
            String[] files = GetStringResourceFilesForLocale(LocaleSetting).ToArray();

            bool fileExists = CheckStringResourceExcelFileExists(LocaleSetting);

            if (fileExists)
            {
                ExcelToXml exf = new ExcelToXml(files);
                XmlDocument xmlDoc = exf.LoadSheet("Sheet1", "C", 5000, "A");
                foreach (XmlNode row in xmlDoc.SelectNodes("/excel/sheet/row"))
                {
                    String StrKey = exf.GetCell(row, "A");
                    String StrVal = exf.GetCell(row, "B");
                    String StrStoreId = AppLogic.DefaultStoreID().ToString();

                    if (StrKey.Length != 0)
                    {
                        if(!String.IsNullOrEmpty(exf.GetCell(row, "C")))
                        {
                            StrStoreId = exf.GetCell(row, "C");
                        }

                        String sql = String.Format("insert StringResource(StoreId, Name,LocaleSetting,ConfigValue) values({0},{1},{2}, {3})", DB.SQuote(StrStoreId), DB.SQuote(StrKey), DB.SQuote(LocaleSetting), DB.SQuote(StrVal));
                        try
                        {
                            DB.ExecuteSQL(sql);
                        }
                        catch (Exception ex)
                        {
                            SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Alert);
                        }
                    }
                }
            }
        }

        public static bool HasAnyStrings()
        {
            return storeStrings.Values.Any(strings => strings.Count > 0);
        }

        private static bool HasNoStringResourceInDB(string locale)
        {
            var count = DB.GetSqlN(String.Format("select count(*) as N from StringResource  with (NOLOCK)  where LocaleSetting={0}", locale.DBQuote()));
            return count == 0;
        }

        public static StringResources GetStringResources(int storeId)
        {
            if (storeStrings.ContainsKey(storeId))
            {
                return storeStrings[storeId];
            }

            var empty = new StringResources();
            return empty;
        }

        public static StringResource GetStringResource(int storeId, int id)
        {
            var strings = EnsureStoreStringResourcesExists(storeId);
            return strings[id];
        }

        public static StringResource GetStringResource(int storeId, string localeSetting, string name)
        {
            var strings = EnsureStoreStringResourcesExists(storeId);
            return strings[localeSetting, name];
        }

        public static bool StringResourceExists(int storeId, string localeSetting, string name)
        {
            var str = GetStringResource(storeId, name, localeSetting);
            return str != null;
        }

    }

}
