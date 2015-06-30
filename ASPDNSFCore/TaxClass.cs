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

namespace AspDotNetStorefrontCore
{
    public class TaxClass
    {
        #region Private Variables

        private int m_Taxclassid;
        private Guid m_Taxclassguid;
        private string m_Name;
        private string m_Taxcode;
        private int m_Displayorder;
        private DateTime m_Createdon;
        
        #endregion


        #region Constructors

        public TaxClass(int TaxClassID)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getTaxclass " + TaxClassID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        m_Taxclassid = DB.RSFieldInt(rs, "TaxClassID");
                        m_Taxclassguid = DB.RSFieldGUID2(rs, "TaxClassGUID");
                        m_Name = DB.RSField(rs, "Name");
                        m_Taxcode = DB.RSField(rs, "TaxCode");
                        m_Displayorder = DB.RSFieldInt(rs, "DisplayOrder");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");

                    }
                    else
                    {
                        m_Taxclassid = 0;
                        m_Taxclassguid = new Guid("00000000000000000000000000000000");
                        m_Name = string.Empty;
                        m_Taxcode = string.Empty;
                        m_Displayorder = 1;
                        m_Createdon = DateTime.MinValue;
                    }
                }
            }
        }

        public TaxClass(int TaxClassID, Guid TaxClassGUID, string Name, string TaxCode, int DisplayOrder, DateTime CreatedOn)
        {
            m_Taxclassid = TaxClassID;
            m_Taxclassguid = TaxClassGUID;
            m_Name = Name;
            m_Taxcode = TaxCode;
            m_Displayorder = DisplayOrder;
            m_Createdon = CreatedOn;
        }

        #endregion


        #region Static Methods

        static public TaxClass Create(string Name, string TaxCode, int DisplayOrder)
        {
            int TaxClassID = 0;
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insTaxclass";

            cmd.Parameters.Add(new SqlParameter("@Name",         SqlDbType.NVarChar, 800));
            cmd.Parameters.Add(new SqlParameter("@TaxCode",      SqlDbType.NVarChar, 800));
            cmd.Parameters.Add(new SqlParameter("@DisplayOrder", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxClassID",   SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@Name"].Value = Name;
            cmd.Parameters["@TaxCode"].Value = TaxCode;
            cmd.Parameters["@DisplayOrder"].Value = DisplayOrder;


            try
            {
                cmd.ExecuteNonQuery();
                TaxClassID = Int32.Parse(cmd.Parameters["@TaxClassID"].Value.ToString());
            }
            catch (Exception ex)
            {
                err = ex.Message;

            }

            if (TaxClassID > 0)
            {
                TaxClass t = new TaxClass(TaxClassID);
                return t;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            return null;
        }

        static public string UpdateTaxClass(int TaxClassID, string TaxCode, object DisplayOrder)
        {
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updTaxclass";

            cmd.Parameters.Add(new SqlParameter("@TaxClassID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxCode", SqlDbType.NVarChar, 800));
            cmd.Parameters.Add(new SqlParameter("@DisplayOrder", SqlDbType.Int, 4));

            cmd.Parameters["@TaxClassID"].Value = TaxClassID;

            if (TaxCode == null) cmd.Parameters["@TaxCode"].Value = DBNull.Value;
            else cmd.Parameters["@TaxCode"].Value = TaxCode;

            if (DisplayOrder == null) cmd.Parameters["@DisplayOrder"].Value = DBNull.Value;
            else cmd.Parameters["@DisplayOrder"].Value = DisplayOrder;


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

        public string Update(string TaxCode, object DisplayOrder)
        {
            string err = string.Empty;
            err = UpdateTaxClass(m_Taxclassid, TaxCode, DisplayOrder);
            if (err == string.Empty)
            {
                m_Taxcode = TaxCode;
                m_Displayorder = (int)DisplayOrder;
            }

            return err;
        }

        #endregion


        #region Public Properties

        public int TaxClassID
        {
            get { return m_Taxclassid; }
        }

        public Guid TaxClassGUID
        {
            get { return m_Taxclassguid; }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public string TaxCode
        {
            get { return m_Taxcode; }
        }

        public int DisplayOrder
        {
            get { return m_Displayorder; }
        }

        public DateTime CreatedOn
        {
            get { return m_Createdon; }
        }

        #endregion




    }


    public class TaxClasses : IEnumerable
    {
        public SortedList m_TaxClasses;

        public TaxClasses()
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getTaxClass", con))
                {
                    while (rs.Read())
                    {
                        m_TaxClasses.Add(DB.RSFieldInt(rs, "TaxClassID"), new TaxClass(DB.RSFieldInt(rs, "TaxClassID"), DB.RSFieldGUID2(rs, "TaxClassGUID"), DB.RSField(rs, "Name"), DB.RSField(rs, "TaxCode"), DB.RSFieldInt(rs, "DisplayOrder"), DB.RSFieldDateTime(rs, "CreatedOn")));
                    }
                }
            }
        }


        public TaxClass this[int taxclassid]
        {
            get
            {
                return (TaxClass)m_TaxClasses[taxclassid];
            }
        }

        public TaxClass this[string TaxClassName]
        {
            get
            {
                for (int i = 0; i < m_TaxClasses.Count; i++)
                {
                    TaxClass t = (TaxClass)m_TaxClasses.GetByIndex(i);
                    if (t.Name == TaxClassName)
                    {
                        return t;
                    }
                }
                return null;
            }
        }


        /// <summary>
        /// Adds an existing TaxClass object to the collection
        /// </summary>
        public void Add(TaxClass taxclass)
        {
            m_TaxClasses.Add(taxclass.TaxClassID, taxclass);
        }

        /// <summary>
        /// Creates a new TaxClass record and adds it to the collection
        /// </summary>
        public void Add(string Name, string TaxCode, int DisplayOrder)
        {
            this.Add(TaxClass.Create(Name, TaxCode, DisplayOrder));
        }

        /// <summary>
        /// Deletes the AppConfig record and removes the item from the collection
        /// </summary>
        public void Remove(int taxclassid)
        {
            try
            {
                DB.ExecuteSQL("aspdnsf_delTaxClass " + taxclassid.ToString());
                m_TaxClasses.Remove(taxclassid);
            }
            catch { }
        }

        public int Count
        {
            get { return m_TaxClasses.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return new TaxClassEnumerator(this);
        }

    }

    public class TaxClassEnumerator : IEnumerator
    {
        private int position = -1;
        private TaxClasses m_taxclasses;

        public TaxClassEnumerator(TaxClasses taxclassescol)
        {
            this.m_taxclasses = taxclassescol;
        }

        public bool MoveNext()
        {
            if (position < m_taxclasses.m_TaxClasses.Count - 1)
            {
                position++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            position = -1;
        }

        public object Current
        {
            get
            {
                return m_taxclasses.m_TaxClasses[position];
            }
        }
    }


}
