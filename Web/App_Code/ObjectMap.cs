// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Collections.Generic;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    // MappedObjectCollection is a temporary copy of the same class in core. We didn't want to modify the application dll or the core dll for admin patches. Because of the insanely slow store entity mapping
    //  cachine that was going on, this particular class calls the get map stored procedure in get objects with a switch that actually enables it. This way the default on startup routine runs through the procedure
    //  without querying anything. This class is used the the EntityObjectMap control in Admin\Controls. In our next point rev, this will be resolved in core and this file will go away.
    public class MappedObjectCollection : List<MappedObject>
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
        private PagingInfo m_pageinfo;
        public PagingInfo PageInfo
        {
            get { return m_pageinfo; }
            set { m_pageinfo = value; }
        }

        public static MappedObjectCollection GetObjects(int storeId, string entityType)
        {
            return GetObjects(storeId, entityType, string.Empty, string.Empty, -1, -1);
        }

        public static MappedObjectCollection GetObjects(int storeId, string entityType, string alphaFilter, string searchFilter, int pageSize, int page)
        {
            var mapping = new MappedObjectCollection();
            mapping.StoreID = storeId;
            mapping.EntityType = entityType;

            var cmd = new SqlCommand("aspdnsf_GetMappedObjects");
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int) { Value = storeId });
            cmd.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.NVarChar, 30) { Value = entityType });

            // nullable parameters
            var pAlphaFilter = new SqlParameter("@AlphaFilter", SqlDbType.NVarChar, 30) { Value = DBNull.Value };
            var pSearchFilter = new SqlParameter("@SearchFilter", SqlDbType.NVarChar, 30) { Value = DBNull.Value };
            var pPageSize = new SqlParameter("@PageSize", SqlDbType.Int) { Value = DBNull.Value };
            var pPage = new SqlParameter("@Page", SqlDbType.Int) { Value = DBNull.Value };

            if (!string.IsNullOrEmpty(alphaFilter))
            {
                pAlphaFilter.Value = alphaFilter;
            }

            if (!string.IsNullOrEmpty(searchFilter))
            {
                pSearchFilter.Value = searchFilter;
            }

            if (pageSize > 0 && page > 0)
            {
                pPageSize.Value = pageSize;
                pPage.Value = page;
            }

            cmd.Parameters.Add(pAlphaFilter);
            cmd.Parameters.Add(pSearchFilter);
            cmd.Parameters.Add(pPageSize);
            cmd.Parameters.Add(pPage);
            cmd.Parameters.Add(new SqlParameter("@IsLegacyCacheMechanism", SqlDbType.Bit)
            {
                Value = 0
            });

            Action<IDataReader> readAction = (rs) =>
            {
                // first result set is the paging data
                if (rs.Read())
                {
                    var nfo = new PagingInfo();
                    nfo.TotalCount = rs.FieldInt("TotalCount");
                    nfo.PageSize = rs.FieldInt("PageSize");
                    nfo.CurrentPage = rs.FieldInt("CurrentPage");
                    nfo.TotalPages = rs.FieldInt("TotalPages");
                    nfo.StartIndex = rs.FieldInt("StartIndex");
                    nfo.EndIndex = rs.FieldInt("EndIndex");

                    mapping.PageInfo = nfo;
                }

                rs.NextResult();

                // next is the actual result set
                while (rs.Read())
                {
                    var map = new MappedObject();
                    map.ID = rs.FieldInt("ID");
                    map.StoreID = storeId;
                    map.Type = rs.Field("EntityType");
                    map.Name = rs.Field("Name");
                    map.IsMapped = rs.FieldBool("Mapped");
                    mapping.Add(map);
                }
            };

            DB.UseDataReader(cmd, readAction);

            return mapping;
        }

        public void SaveMapping()
        {
            var cmd = new SqlCommand("aspdnsf_SaveMap");
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@StoreId", SqlDbType.Int) { Value = this.StoreID });
            cmd.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.NVarChar, 30) { Value = this.EntityType });

            var pEntityId = new SqlParameter("@EntityId", SqlDbType.Int);
            var pMap = new SqlParameter("@Map", SqlDbType.Bit);
            cmd.Parameters.Add(pEntityId);
            cmd.Parameters.Add(pMap);


            using (var con = DB.dbConn())
            {
                con.Open();
                using (cmd)
                {
                    cmd.Connection = con;
                    foreach (var map in this)
                    {
                        pEntityId.Value = map.ID;
                        pMap.Value = map.IsMapped;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
