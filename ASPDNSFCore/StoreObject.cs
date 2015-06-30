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
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Data;
using System.Web;

namespace AspDotNetStorefrontCore
{
    public enum StoreUrlType
    {
        Production = 0,
        Staging = 1,
        Development = 2,
        Unknown
    }

    [Serializable]
    public class Store
    {

        public Store()
        {
            EntityMaps = new List<StoreEntityMap>();
        }

        private Guid _StoreGuid;

        #region "Properties"

        public List<StoreEntityMap> EntityMaps { get; private set; }

        private int m_storeid;
        public int StoreID
        {
            get { return m_storeid; }
            set { m_storeid = value; }
        }
        public object StoreGuid
        {
            get { return _StoreGuid; }
            set
            {
                if (value.GetType() == typeof(Guid))
                    _StoreGuid = (Guid)value;
                else if (value.GetType() == typeof(string))
                    _StoreGuid = new Guid((string)value);
            }
        }

        private string m_productionuri;
        /// <summary>
        /// The URI for the production instance of this site
        /// </summary>
        public string ProductionURI
        {
            get { return m_productionuri; }
            set { m_productionuri = value; }
        }

        /// <summary>
        /// Flag indicating whether or not the Production Domain has been licensed
        /// </summary>
        public bool ProductionURILicensed
        {
            get
            {
                if (ProductionURI == null)
                    return false;
                else
                    return AspDotNetStorefront.Global.DomainIsLicensed(ProductionURI);
            }
        }

        private string m_staginguri;
        /// <summary>
        /// The URI for the staging instance of this site
        /// </summary>
        public string StagingURI
        {
            get { return m_staginguri; }
            set { m_staginguri = value; }
        }
        /// <summary>
        /// Flag indicating whether or not the staging Domain has been licensed
        /// </summary>
        public bool StagingURILicensed
        {
            get
            {
                if (StagingURI == null)
                    return false;
                else
                    return AspDotNetStorefront.Global.DomainIsLicensed(StagingURI);
            }
        }

        private string m_developmenturi;
        /// <summary>
        /// The URI for the development instance of this site
        /// </summary>
        public string DevelopmentURI
        {
            get { return m_developmenturi; }
            set { m_developmenturi = value; }
        }
        /// <summary>
        /// Flag indicating whether or not the development Domain has been licensed
        /// </summary>
        public bool DevelopmentURILicensed
        {
            get
            {
                if (DevelopmentURI == null)
                    return false;
                else
                    return AspDotNetStorefront.Global.DomainIsLicensed(DevelopmentURI);
            }
        }
        private string m_name;
        /// <summary>
        /// Name of the Store
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private string m_description;
        /// <summary>
        /// Description of the store
        /// </summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        private bool m_published;
        /// <summary>
        /// Flag indicating whether or not the site is accessible to customers
        /// </summary>
        public bool Published
        {
            get { return m_published; }
            set { m_published = value; }
        }

        private bool m_deleted;
        /// <summary>
        /// Flag indicating weather store has been delete
        /// Note: This is a soft delete, nuke for a hard delete
        /// </summary>
        public bool Deleted
        {
            get { return m_deleted; }
            set { m_deleted = value; }
        }

        private bool m_isdefault;
        /// <summary>
        /// Inidcator that site is default in the event another
        /// store cannot be parsed from the URL
        /// </summary>
        public bool IsDefault
        {
            get { return m_isdefault; }
            set { m_isdefault = value; }
        }

        private int m_skinid;
        /// <summary>
        /// The skin to be used by the store
        /// </summary>
        public int SkinID
        {
            get { return m_skinid; }
            set { m_skinid = value; }
        }

        private DateTime m_createdon;
        /// <summary>
        /// The date the store was created
        /// </summary>
        public DateTime CreatedOn
        {
            get { return m_createdon; }
            set { m_createdon = value; }
        }
        #endregion

        [NonSerialized]
        private SqlCommand _cmdSaveList;
        private SqlCommand sqlUpdateList
        {
            get
            {
                if (_cmdSaveList != null)
                    return _cmdSaveList;
                string SQL =
 @"
IF @GUID IS NULL 
BEGIN
	INSERT Store(StoreGUID, ProductionURI, StagingURI, DevelopmentURI, Name, Description, IsDefault, Published, SkinID, CreatedOn)
	VALUES (NewID(), @ProductionURI, @StagingURI, @DevelopmentURI, @Name, @Description, @IsDefault, @Published, @SkinID, Getdate())
END
ELSE
BEGIN
	UPDATE Store 
		SET 
        ProductionURI = ISNULL(@ProductionURI, ProductionURI), 
        StagingURI = ISNULL(@StagingURI, StagingURI), 
        DevelopmentURI = ISNULL(@DevelopmentURI, DevelopmentURI), 
		[Name] = ISNULL(@Name, [Name]),
		Description = ISNULL(@Description, Description),
        Deleted = ISNULL(@Deleted, Deleted),
        IsDefault = ISNULL(@IsDefault, IsDefault), 
        Published = ISNULL(@Published, Published), 
		SkinID = ISNULL(@SkinID, SkinID)
	WHERE
		StoreGUID = @GUID
END
"
;
                _cmdSaveList = new SqlCommand(SQL);
                _cmdSaveList.Parameters.Add(new SqlParameter("@GUID", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@Name", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@ProductionURI", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@StagingURI", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@DevelopmentURI", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@Description", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@Published", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@IsDefault", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@SkinID", DBNull.Value));
                _cmdSaveList.Parameters.Add(new SqlParameter("@Deleted", DBNull.Value));
                return _cmdSaveList;
            }
        }

        /// <summary>
        /// Commits any changes to the object to the database
        /// </summary>
        public void Save()
        {
            using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
            {
                if (StoreGuid.ToString() != new Guid().ToString())
                    sqlUpdateList.Parameters["@GUID"].Value = StoreGuid;
                sqlUpdateList.Parameters["@Name"].Value = Name;
                sqlUpdateList.Parameters["@Description"].Value = Description == null ? " " : Description;
                if (ProductionURI != null)
                {
                    sqlUpdateList.Parameters["@ProductionURI"].Value = ProductionURI;
                }
                if (StagingURI != null)
                {
                    sqlUpdateList.Parameters["@StagingURI"].Value = StagingURI;
                }
                if (DevelopmentURI != null)
                {
                    sqlUpdateList.Parameters["@DevelopmentURI"].Value = DevelopmentURI;
                }
                sqlUpdateList.Parameters["@IsDefault"].Value = IsDefault ? 1 : 0;
                sqlUpdateList.Parameters["@Published"].Value = Published ? 1 : 0;
                sqlUpdateList.Parameters["@Deleted"].Value = Deleted ? 1 : 0;
                sqlUpdateList.Parameters["@SkinID"].Value = SkinID;
                xCon.Open();
                sqlUpdateList.Connection = xCon;
                sqlUpdateList.ExecuteNonQuery();
            }
        }

        public static int StoreCount
        {
            get
            {
                if (_lstStores == null)
                {
                    return DB.GetSqlN("SELECT COUNT(*) N FROM Store WHERE Deleted = 0");
                }
                else
                {
                    return _lstStores.Count;
                }
            }
        }

        public static bool IsMultiStore
        {
            //not using license check right now
			//get { return AspDotNetStorefront.Global.LicenseInfo("multistore").EqualsIgnoreCase("true") && AspDotNetStorefront.Global.LicenseInfo("licensename").EqualsIgnoreCase("AspDotNetStorefront Multi-Store"); }
			get { return Store.StoreCount > 1; }
        }

        public static void resetStoreCache()
        {
            GetStoreList(true);
            CacheMappings();
        }

        private static List<Store> _lstStores;

        public static String GetStoreName(int StoreId)
        {
            Store st = GetStoreList().FirstOrDefault(s => s.StoreID == StoreId);
            if (st != null)
	        {
		        return st.Name;
	        }
            return string.Empty;
        }

        /// <summary>
        /// Retrieves all stores
        /// </summary>
        public static List<Store> GetStoreList()
        {
            return GetStoreList(false);
        }

        /// <summary>
        /// Retrieves all stores
        /// </summary>
        /// <param name="refresh">Whether or not to refresh the cache</param>
        /// <returns></returns>
        public static List<Store> GetStoreList(bool refresh)
        {
            if (_lstStores != null && !refresh)
            {
                return _lstStores;
            }

            _lstStores = GetStores(false);

            return _lstStores;

        }

        /// <summary>
        /// Returns a generic list for the stores
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public static List<Store> GetStores(bool all)
        {
            var allStores = new List<Store>();

            var sql = "SELECT [StoreID],[StoreGUID], [Name], [ProductionURI], [StagingURI], [DevelopmentURI], [Description], [Published], [Deleted], [SkinID], [IsDefault], [CreatedOn] FROM Store";

            if (!all)
            {
                sql += " WHERE Deleted = 0 and Published = 1";
            }

            using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
            {
                xCon.Open();
                IDataReader xRdr = DB.GetRS(sql, xCon);
                while (xRdr.Read())
                {
                    Store xObj = new Store();
                    xObj.Initialize(xRdr);

                    allStores.Add(xObj);
                }
            }

            return allStores;
        }

		/// <summary>
		/// Retrieves a store by its id
		/// </summary>
		/// <param name="id">the store id</param>
		/// <returns></returns>
		public static Store GetStoreById(int id)
		{
			return GetStores(true).Find(s => s.StoreID == id);
		}

		/// <summary>
		/// Gets the application's default store
		/// </summary>
		/// <returns></returns>
		public static Store GetDefaultStore()
		{
			return GetStores(false).Find(s => s.IsDefault == true);
		}

        public static StoreUrlType DetermineCurrentUrlType()
        {
            string currentUrl = HttpContext.Current.Request.Url.Host;
            Store currentStore = GetStoreById(AppLogic.StoreID());

            if (currentStore.ProductionURI == currentUrl)
                return StoreUrlType.Production;
            else if (currentStore.StagingURI == currentUrl)
                return StoreUrlType.Staging;
            else if (currentStore.DevelopmentURI == currentUrl)
                return StoreUrlType.Development;
            else
                return StoreUrlType.Unknown;
        }

        public static string GetStoreUrlByType(StoreUrlType urlType, int storeId)
        {
            Store store = GetStoreById(storeId);

            switch (urlType)
            {
                case StoreUrlType.Production:
                    return store.ProductionURI;
                case StoreUrlType.Development:
                    return store.DevelopmentURI;
                case StoreUrlType.Staging:
                    return store.StagingURI;
            }

            return string.Empty;
        }

        /// <summary>
        /// Initializes the members of this store based on the datareader values
        /// </summary>
        /// <param name="xRdr"></param>
        private void Initialize(IDataReader xRdr)
        {
            this.StoreID = xRdr.FieldInt("StoreID");
            this.Name = xRdr.Field("Name");
            this.StoreGuid = xRdr.FieldGuid("StoreGUID");
            this.ProductionURI = xRdr.Field("ProductionURI");
            this.DevelopmentURI = xRdr.Field("DevelopmentURI");
            this.StagingURI = xRdr.Field("StagingURI");
            this.Description = xRdr.Field("Description");
            this.Published = ((byte)xRdr["Published"]) == 1;
            this.SkinID = (int)xRdr["SkinID"];
            this.IsDefault = (byte)xRdr["IsDefault"] == 1;
            this.CreatedOn = xRdr.FieldDateTime("CreatedOn");
            this.Deleted = xRdr.FieldBool("Deleted");
        }


        /// <summary>
        /// Switches setting on published flag
        /// </summary>
        public void PublishSwitch()
        {
            string SQL = @"
                UPDATE Store SET Published = @Published WHERE StoreGUID = @GUID
            ";
            SqlParameter[] xParams = new SqlParameter[]
            {
                new SqlParameter("@Published", Published ? 0 : 1),
                new SqlParameter("@GUID", StoreGuid)
            };
            DB.ExecuteSQL(SQL, xParams);
            Store.GetStoreList(true);
        }
        /// <summary>
        /// Sets the delete flag to true. This also will remove all store related mappings.
        /// (Logical delete)
        /// </summary>
        public void DeleteStore()
        {
            // removes mappings first then
            NukeMappings();

            string SQL = @"
                UPDATE Store SET Deleted = 1 WHERE StoreGUID = @GUID
            ";
            SqlParameter[] xParams = new SqlParameter[]
            {
                new SqlParameter("@GUID", StoreGuid)
            };
            DB.ExecuteSQL(SQL, xParams);
        }

        /// <summary>
        /// Sets the delete flag to false
        /// (Logical delete)
        /// </summary>
        public void UnDeleteStore()
        {
            string SQL = @"
                UPDATE Store SET Deleted = 0 WHERE StoreGUID = @GUID
            ";
            SqlParameter[] xParams = new SqlParameter[]
            {
                new SqlParameter("@GUID", StoreGuid)
            };
            DB.ExecuteSQL(SQL, xParams);
        }

        /// <summary>
        /// Sets this store as default turning the default flag off for all other stores
        /// </summary>
        public void SetDefault()
        {
            var makeDefSql = "[aspdnsf_MakeStoreDefault] @StoreID = {0}".FormatWith(this.StoreID);
            DB.ExecuteSQL(makeDefSql);

            // update the context cache
            var ctx = System.Web.HttpContext.Current;
            if (ctx != null)
            {
                ctx.Items["DefaultStoreId"] = this.StoreID;
            }
        }

        /// <summary>
        /// Removes all store mappings
        /// </summary>
        private void NukeMappings()
        {
            // first nuke all mappings
            var nukeMapSql = "[aspdnsf_NukeStoreMappings] @StoreID = {0}".FormatWith(this.StoreID);
            DB.ExecuteSQL(nukeMapSql);
        }

        /// <summary>
        /// Overrides the settings of this store instance and copies the settings from the passed store instance.
        /// </summary>
        /// <param name="other"></param>
        public void CopyFrom(Store other)
        {
            NukeMappings();

            // clone all mappings from the other store
            var cloneMapSql = "[aspdnsf_CloneStoreMappings] @FromStoreID = {0}, @ToStoreID = {1}".FormatWith(other.StoreID, this.StoreID);
            DB.ExecuteSQL(cloneMapSql);
            
            // reset cached entity mappings
            CacheEntityMappings();

            // now overwrite the following attributes            
            this.ProductionURI = other.ProductionURI;
            this.DevelopmentURI = other.DevelopmentURI;
            this.StagingURI = other.StagingURI;
            this.Description = other.Description;
            this.Published = other.Published;
            this.SkinID = other.SkinID;

            this.Save();
        }

        /// <summary>
        /// Creates a duplicate copy of the store
        /// </summary>
        public Store CloneStore()
        {
            Store newClonedStore = null;

            var newName = "{0} - Clone".FormatWith(this.Name);

            try
            {
                var newStoreID = DB.ExecuteStoredProcInt("aspdnsf_CloneStore",
                                new SqlParameter[] { 
                                    new SqlParameter("StoreID", SqlDbType.Int) { Value=this.StoreID, Direction= ParameterDirection.Input},
                                    new SqlParameter("NewStoreName", SqlDbType.NVarChar) { Size = 400, Value=newName, Direction= ParameterDirection.Input},
                                    new SqlParameter("NewStoreID", SqlDbType.Int) { Size = 400, Direction= ParameterDirection.Output}
                                });

                Action<IDataReader> readAction = (rs) =>
                {
                    if (rs.Read())
                    {
                        newClonedStore = new Store();
                        newClonedStore.Initialize(rs);
                    }
                };

                var sql = "SELECT * FROM Store WITH (NOLOCK) WHERE StoreID = {0}".FormatWith(newStoreID);
                DB.UseDataReader(sql, readAction);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to clone store: ".FormatWith(this.Name), ex);
            }

            return newClonedStore;
        }

        /// <summary>
        /// Delete the store and removes all references
        /// </summary>
        public void NukeStore()
        {
            using (var con = DB.dbConn())
            {
                con.Open();
                
                using (var cmd = new SqlCommand("[aspdnsf_NukeStore]", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@StoreID", SqlDbType.Int) { Value = this.StoreID });

                    SqlTransaction trans = null;
                    try
                    {
                        trans = con.BeginTransaction();
                        cmd.Transaction = trans;

                        cmd.ExecuteNonQuery();

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }                
            }

            // at this point this store is already nuked, it's reference should be removed
        }

        /// <summary>
        /// Checks to see if a particular product is mapped to the store
        /// </summary>
        /// <param name="_ProductID">ID of the product to check for mapping</param>
        public bool IsProductMapped(int _ProductID)
        {
            return IsMapped("Product", _ProductID);
        }

        /// <summary>
        /// Checks to determine if a particular entity is mapped to the store
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        /// <param name="id">Entity ID</param>
        /// <returns></returns>
        public bool IsMapped(string entityType, int id)
        {
            MappedObject map = GetMapping(entityType, id);

            return (map != null && map.IsMapped);
        }

        public MappedObject GetMapping(string entityType, int id)
        {
            entityType = entityType.ToLowerInvariant();

            MappedObject map = null;

            if (this.EntityMaps.Count > 0)
            {
                var entity = this.EntityMaps.Find(e => e.EntityType.EqualsIgnoreCase(entityType));
                if (entity != null)
                {
                    map = entity.MappedObjects.Find(m => m.ID == id);
                }
            }
            else
            {
                map = MappedObject.Find(this.StoreID, entityType, id);
            }

            return map;
        }

        public void UpdateMapping(string entityType, int entityID, bool isMapped)
        {
            MappedObject map = GetMapping(entityType, entityID);
            if (map == null)
            {
                map = new MappedObject();
                map.Type = entityType;
                map.StoreID = StoreID;
                map.ID = entityID;
            }

            map.IsMapped = isMapped;
            map.Save();
        }

        private void CacheEntityMappings()
        {
            EntityMaps.Clear();

            // cache per entity map
            CacheEntityMappings("Product");
            CacheEntityMappings("Manufacturer");
            CacheEntityMappings("Category");
            CacheEntityMappings("Section");
            CacheEntityMappings("Coupon");
            CacheEntityMappings("OrderOption");
            CacheEntityMappings("GiftCard");
            CacheEntityMappings("ShippingMethod");
            CacheEntityMappings("Affiliate");
            CacheEntityMappings("Topic");
            CacheEntityMappings("News");
            CacheEntityMappings("Polls");
        }

        private void CacheEntityMappings(string entityType)
        {
            var entityMap = new StoreEntityMap() { StoreID = this.StoreID, EntityType = entityType };
            entityMap.MappedObjects = MappedObjectCollection.GetObjects(this.StoreID, entityType);

            EntityMaps.Add(entityMap);
        }

        private static object sl = new object();

        public static void CacheMappings()
        {
            lock (sl)
            {
                List<Store> stores = Store.GetStoreList();

                foreach (Store sto in stores)
                {
                    sto.CacheEntityMappings();
                }
            }
        }

        public void ResetEntityMappingsCache(string entityType)
        {
            var entityMap = this.EntityMaps.Find(e => e.EntityType.EqualsIgnoreCase(entityType));
            if (entityMap != null)
            {
                this.EntityMaps.Remove(entityMap);

                CacheEntityMappings(entityType);
            }
        }

    }

    public class StoreEntityMap
    {
        private int m_storeid;
        public int StoreID
        {
            get { return m_storeid; }
            set { m_storeid = value; }
        }
        private string m_entitytype;
        public string EntityType
        {
            get { return m_entitytype; }
            set { m_entitytype = value; }
        }
        private MappedObjectCollection m_mappedobjects;
        public MappedObjectCollection MappedObjects 
        {
            get { return m_mappedobjects; }
            set { m_mappedobjects = value; }
        }
    }

}
