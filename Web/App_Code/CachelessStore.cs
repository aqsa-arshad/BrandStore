// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Data;
using AspDotNetStorefrontCore;

#endregion

namespace AspDotNetStorefront
{
    // CachelessStore is a temporary copy of the same class in core. We didn't want to modify the application dll or the core dll for admin patches. Because of the insanely slow store entity mapping
    //  cachine that was going on, this particular class mimics the original but without the caching model. This class is used the the EntityObjectMap control in Admin\Controls. In our next point rev, this will be resolved in core and this file will go away.
    [Serializable]
    public class CachelessStore
    {
        #region Fields

        private Guid _StoreGuid;
        private static List<CachelessStore> _lstStores;
        [NonSerialized]
        private SqlCommand _cmdSaveList;

        #endregion

        #region Properties

        public Int32 StoreID { get; set; }

        public Object StoreGuid
        {
            get { return _StoreGuid; }
            set
            {
                if (value.GetType() == typeof(Guid))
                    _StoreGuid = (Guid)value;
                else if (value.GetType() == typeof(String))
                    _StoreGuid = new Guid((String)value);
            }
        }

        public String ProductionURI { get; set; }

        public Boolean ProductionURILicensed
        {
            get
            {
                if (this.ProductionURI == null)
                    return false;
                else
                    return AspDotNetStorefront.Global.DomainIsLicensed(this.ProductionURI);
            }
        }

        public String StagingURI { get; set; }
        
        public Boolean StagingURILicensed
        {
            get
            {
                if (this.StagingURI == null)
                    return false;
                else
                    return AspDotNetStorefront.Global.DomainIsLicensed(this.StagingURI);
            }
        }

        public String DevelopmentURI { get; set; }

        public Boolean DevelopmentURILicensed
        {
            get
            {
                if (this.DevelopmentURI == null)
                    return false;
                else
                    return AspDotNetStorefront.Global.DomainIsLicensed(this.DevelopmentURI);
            }
        }

        public String Name { get; set; }

        public String Description { get; set; }

        public Boolean Published { get; set; }

        public Boolean Deleted { get; set; }

        public Boolean IsDefault { get; set; }

        public Int32 SkinID { get; set; }

        public DateTime CreatedOn { get; set; }

        public static Int32 StoreCount
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

        public static Boolean IsMultiStore
        {
            //not using license check right now
            //get { return AspDotNetStorefront.Global.LicenseInfo("multistore").EqualsIgnoreCase("true") && AspDotNetStorefront.Global.LicenseInfo("licensename").EqualsIgnoreCase("AspDotNetStorefront Multi-Store"); }
            get { return Store.StoreCount > 1; }
        }

        private SqlCommand sqlUpdateList
        {
            get
            {
                if (_cmdSaveList != null)
                    return _cmdSaveList;

                String SQL =
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
        
        #endregion
        
        #region Public Methods

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

        public static void resetStoreCache()
        {
			_lstStores = null;
            //GetStoreList(true);
            //CacheMappings();
        }

        public static List<CachelessStore> GetStoreList()
        {
            return GetStoreList(false);
        }

        public static List<CachelessStore> GetStoreList(Boolean refresh)
        {
            if (_lstStores == null || refresh)
                _lstStores = GetStores(false);
            
            return _lstStores;
        }

        public static List<CachelessStore> GetStores(Boolean all)
        {
            var stores = new List<CachelessStore>();

            var sql = "SELECT [StoreID],[StoreGUID], [Name], [ProductionURI], [StagingURI], [DevelopmentURI], [Description], [Published], [Deleted], [SkinID], [IsDefault], [CreatedOn] FROM Store";

            if (!all)
                sql += " WHERE Deleted = 0 and Published = 1";

            using (SqlConnection connection = new SqlConnection(DB.GetDBConn()))
            {
                connection.Open();
                IDataReader reader = DB.GetRS(sql, connection);
                while (reader.Read())
                {
                    CachelessStore store = new CachelessStore();
                    store.Initialize(reader);
                    stores.Add(store);
                }
            }

            return stores;
        }

        public void PublishSwitch()
        {
            String SQL = @"
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

        public void DeleteStore()
        {
            // removes mappings first then
            NukeMappings();

            String SQL = @"
                UPDATE Store SET Deleted = 1 WHERE StoreGUID = @GUID
            ";
            SqlParameter[] xParams = new SqlParameter[]
            {
                new SqlParameter("@GUID", StoreGuid)
            };
            DB.ExecuteSQL(SQL, xParams);
        }

        public void UnDeleteStore()
        {
            String SQL = @"
                UPDATE Store SET Deleted = 0 WHERE StoreGUID = @GUID
            ";
            SqlParameter[] xParams = new SqlParameter[]
            {
                new SqlParameter("@GUID", StoreGuid)
            };
            DB.ExecuteSQL(SQL, xParams);
        }

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

        public void CopyFrom(Store other)
        {
            NukeMappings();

            // clone all mappings from the other store
            var cloneMapSql = "[aspdnsf_CloneStoreMappings] @FromStoreID = {0}, @ToStoreID = {1}".FormatWith(other.StoreID, this.StoreID);
            DB.ExecuteSQL(cloneMapSql);

            // now overwrite the following attributes            
            this.ProductionURI = other.ProductionURI;
            this.DevelopmentURI = other.DevelopmentURI;
            this.StagingURI = other.StagingURI;
            this.Description = other.Description;
            this.Published = other.Published;
            this.SkinID = other.SkinID;

            this.Save();
        }

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
                        //newClonedStore.Initialize(rs);
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

        public Boolean IsProductMapped(Int32 _ProductID)
        {
            return IsMapped("Product", _ProductID);
        }

        public Boolean IsMapped(String entityType, Int32 id)
        {
            MappedObject map = GetMapping(entityType, id);

            return (map != null && map.IsMapped);
        }

        public MappedObject GetMapping(String entityType, Int32 id)
        {
            entityType = entityType.ToLowerInvariant();

            MappedObject map = null;

            using (SqlConnection connection = new SqlConnection(DB.GetDBConn()))
            {
                connection.Open();

                IDataReader reader = DB.GetRS("dbo.aspdnsf_GetMappedObject @StoreId = {0}, @EntityType = {1}, @EntityID = {2}".FormatWith(this.StoreID, entityType.DBQuote(), id), connection);
                while (reader.Read())
                {
                    map = new MappedObject();
                    map.ID = reader.FieldInt("ID");
                    map.StoreID = this.StoreID;
                    map.Type = reader.Field("EntityType");
                    map.Name = reader.Field("Name");
                    map.IsMapped = reader.FieldBool("Mapped");
                }
            }

            return map;
        }

        public void UpdateMapping(String entityType, Int32 entityID, Boolean isMapped)
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

        #endregion

        #region Private Methods

        private void Initialize(IDataReader reader)
        {
            this.StoreID = reader.FieldInt("StoreID");
            this.Name = reader.Field("Name");
            this.StoreGuid = reader.FieldGuid("StoreGUID");
            this.ProductionURI = reader.Field("ProductionURI");
            this.DevelopmentURI = reader.Field("DevelopmentURI");
            this.StagingURI = reader.Field("StagingURI");
            this.Description = reader.Field("Description");
            this.Published = ((byte)reader["Published"]) == 1;
            this.SkinID = (Int32)reader["SkinID"];
            this.IsDefault = (byte)reader["IsDefault"] == 1;
            this.CreatedOn = reader.FieldDateTime("CreatedOn");
            this.Deleted = reader.FieldBool("Deleted");
        }

        private void NukeMappings()
        {
            // first nuke all mappings
            var nukeMapSql = "[aspdnsf_NukeStoreMappings] @StoreID = {0}".FormatWith(this.StoreID);
            DB.ExecuteSQL(nukeMapSql);
        }

        #endregion
    }
}
