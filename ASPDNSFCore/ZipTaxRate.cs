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
    public class ZipTaxRate
    {
        #region Private Variables
        private int m_Ziptaxid;
        private string m_Zipcode;
        private int m_Taxclassid;
        private string m_Taxclass;
        private decimal m_Taxrate;
        private DateTime m_Createdon;
        private int m_CountryID;

        #endregion


        #region Constructors

        public ZipTaxRate(int ZipTaxID)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getZipTaxRateByID " + ZipTaxID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        m_Ziptaxid = DB.RSFieldInt(rs, "ZipTaxID");
                        m_Zipcode = DB.RSField(rs, "ZipCode");
                        m_Taxclassid = DB.RSFieldInt(rs, "TaxClassID");
                        m_Taxclass = DB.RSField(rs, "TaxClass");
                        m_Taxrate = DB.RSFieldDecimal(rs, "TaxRate");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                        m_CountryID = DB.RSFieldInt(rs, "CountryID");
                    }
                    else
                    {
                        m_Ziptaxid = 0;
                        m_Zipcode = "";
                        m_Taxclassid = 0;
                        m_Taxclass = "";
                        m_Taxrate = 0.0M;
                        m_Createdon = DateTime.MinValue;
                        m_CountryID = 0;
                    }
                }
            }
        }

        public ZipTaxRate(string ZipCode, int TaxClassID)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getStateTaxRate " + DB.SQuote(ZipCode) + "," + TaxClassID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        m_Ziptaxid = DB.RSFieldInt(rs, "ZipTaxID");
                        m_Zipcode = DB.RSField(rs, "ZipCode");
                        m_Taxclassid = DB.RSFieldInt(rs, "TaxClassID");
                        m_Taxclass = DB.RSField(rs, "TaxClass");
                        m_Taxrate = DB.RSFieldDecimal(rs, "TaxRate");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                        m_CountryID = DB.RSFieldInt(rs, "CountryID");
                    }
                    else
                    {
                        m_Ziptaxid = 0;
                        m_Zipcode = "";
                        m_Taxclassid = 0;
                        m_Taxclass = "";
                        m_Taxrate = 0.0M;
                        m_Createdon = DateTime.MinValue;
                        m_CountryID = 0;
                    }
                }
            }
        }

        public ZipTaxRate(int ZipTaxID, string ZipCode, int TaxClassID, string TaxClass, decimal TaxRate, DateTime CreatedOn)
        {
            m_Ziptaxid = ZipTaxID;
            m_Zipcode = ZipCode;
            m_Taxclassid = TaxClassID;
            m_Taxclass = TaxClass;
            m_Taxrate = TaxRate;
            m_Createdon = CreatedOn;
        }

        public ZipTaxRate(int ZipTaxID, string ZipCode, int TaxClassID, string TaxClass, decimal TaxRate, DateTime CreatedOn, int CountryID)
        {
            m_Ziptaxid = ZipTaxID;
            m_Zipcode = ZipCode;
            m_Taxclassid = TaxClassID;
            m_Taxclass = TaxClass;
            m_Taxrate = TaxRate;
            m_Createdon = CreatedOn;
            m_CountryID = CountryID;
        }

        #endregion


        #region Static Methods

        static public ZipTaxRate Create(string ZipCode, int TaxClassID, decimal TaxRate, int CountryId)
        {
            int ZipTaxID = 0;
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insZipTaxRate";

            cmd.Parameters.Add(new SqlParameter("@ZipCode", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@TaxClassID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxRate", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@CountryId", SqlDbType.Int));

            cmd.Parameters["@ZipCode"].Value = ZipCode;
            cmd.Parameters["@TaxClassID"].Value = TaxClassID;
            cmd.Parameters["@TaxRate"].Value = TaxRate;
            cmd.Parameters["@CountryId"].Value = CountryId;

            try
            {
                using (IDataReader rs = cmd.ExecuteReader())
                {
                    if (rs.Read())
                    {
                        ZipTaxID = Int32.Parse(rs.GetValue(0).ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
                
            }
    
            if (ZipTaxID > 0)
            {
                ZipTaxRate str = new ZipTaxRate(ZipTaxID);
                return str;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            return null;
        }

        static public string UpdateZipTaxRate(int ZipTaxID, object TaxRate, int CountryID)
        {
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updZipTaxRate";

            cmd.Parameters.Add(new SqlParameter("@ZipTaxID",  SqlDbType.Int       , 4));
            cmd.Parameters.Add(new SqlParameter("@TaxRate", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@CountryID", SqlDbType.Int));

            cmd.Parameters["@ZipTaxID"].Value = ZipTaxID;
            cmd.Parameters["@CountryID"].Value = CountryID;

            if (TaxRate == null) cmd.Parameters["@TaxRate"].Value = DBNull.Value;
            else cmd.Parameters["@TaxRate"].Value = TaxRate;


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

        static public string UpdateZipTaxRate(int ZipTaxID, string ZipCode, object TaxRate, int CountryID, int OriginalCountryID)
        {
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_UpdateZipTaxRateCountry";

            cmd.Parameters.Add(new SqlParameter("@ZipTaxID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ZipCode", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@TaxRate", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@CountryID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@OriginalCountryID", SqlDbType.Int));

            cmd.Parameters["@ZipTaxID"].Value = ZipTaxID;
            cmd.Parameters["@ZipCode"].Value = ZipCode;
            cmd.Parameters["@CountryID"].Value = CountryID;
            cmd.Parameters["@OriginalCountryID"].Value = OriginalCountryID;

            if (TaxRate == null) cmd.Parameters["@TaxRate"].Value = DBNull.Value;
            else cmd.Parameters["@TaxRate"].Value = TaxRate;


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

        public string Update(object TaxRate, int CountryId)
        {
            string err = string.Empty;
            err = UpdateZipTaxRate(m_Ziptaxid, TaxRate, CountryId);
            if (err == string.Empty)
            {
                m_Taxrate = (decimal)TaxRate;
            }

            return err;
        }
        
        public string Update(object TaxRate, string ZipCode, int CountryId, int OriginalCountryID)
        {
            string err = string.Empty;
            err = UpdateZipTaxRate(m_Ziptaxid, ZipCode, TaxRate, CountryId, OriginalCountryID);
            if (err == string.Empty)
            {
                m_Taxrate = (decimal)TaxRate;
            }

            return err;
        }
        #endregion


        #region Public Properties

        public int ZipTaxID
        {
            get { return m_Ziptaxid; }
        }


        public string ZipCode
        {
            get { return m_Zipcode; }
        }


        public int TaxClassID
        {
            get { return m_Taxclassid; }
        }

        public string TaxClass
        {
            get { return m_Taxclass; }
        }
        

        public decimal TaxRate
        {
            get { return m_Taxrate; }
        }


        public DateTime CreatedOn
        {
            get { return m_Createdon; }
        }

        public int CountryID
        {
            get { return m_CountryID; }
        }


        
        #endregion

    }

    public class ZipTaxRates : IEnumerable
    {
        public SortedList m_ZipTaxRates;

        public ZipTaxRates()
        {
            m_ZipTaxRates = new SortedList();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_SelectZipTaxRatesAll", con))
                {
                    while (rs.Read())
                    {
                        m_ZipTaxRates.Add(DB.RSFieldInt(rs, "ZipTaxID"), new ZipTaxRate(DB.RSFieldInt(rs, "ZipTaxID"), DB.RSField(rs, "ZipCode"), DB.RSFieldInt(rs, "TaxClassID"), DB.RSField(rs, "TaxClass"), DB.RSFieldDecimal(rs, "TaxRate"), DB.RSFieldDateTime(rs, "CreatedOn"), DB.RSFieldInt(rs, "CountryID")));
                    }
                }
            }
        }


        public ZipTaxRate this[int ziptaxid]
        {
            get
            {
                return (ZipTaxRate)m_ZipTaxRates[ziptaxid];
            }
        }

        public ZipTaxRate this[string ZipCode, int TaxClassID]
        {
            get
            {
                for (int i = 0; i < m_ZipTaxRates.Count; i++)
                {
                    ZipTaxRate ztr = (ZipTaxRate)m_ZipTaxRates.GetByIndex(i);
                    if (ztr.ZipCode == ZipCode && ztr.TaxClassID == TaxClassID)
                    {
                        return ztr;
                    }
                }
                return null;
            }
        }

        public ZipTaxRate this[string ZipCode, int TaxClassID, int CountryID]
        {
            get
            {
                for (int i = 0; i < m_ZipTaxRates.Count; i++)
                {
                    ZipTaxRate ztr = (ZipTaxRate)m_ZipTaxRates.GetByIndex(i);
                    if (ztr.ZipCode == ZipCode && ztr.TaxClassID == TaxClassID && ztr.CountryID == CountryID)
                    {
                        return ztr;
                    }
                }
                return null;
            }
        }
        public decimal GetTaxRate(string ZipCode, int TaxClassID)
        {
            if (ZipCode == string.Empty)
            {
                return 0.0M;
            }
            for (int i = 0; i < m_ZipTaxRates.Count; i++)
            {
                ZipTaxRate ztr = (ZipTaxRate)m_ZipTaxRates.GetByIndex(i);

                if (ztr.ZipCode.Equals(ZipCode, StringComparison.InvariantCultureIgnoreCase) && ztr.TaxClassID == TaxClassID)
                {
                    return ztr.TaxRate;
                }
            }
            return 0.0M;
        }

        public decimal GetTaxRate(string ZipCode, int TaxClassID, int CountryID)
        {
            if (ZipCode == string.Empty)
            {
                return 0.0M;
            }
            for (int i = 0; i < m_ZipTaxRates.Count; i++)
            {
                ZipTaxRate ztr = (ZipTaxRate)m_ZipTaxRates.GetByIndex(i);
                
                if (ztr.ZipCode.Equals(ZipCode, StringComparison.InvariantCultureIgnoreCase) && ztr.TaxClassID == TaxClassID && ztr.CountryID == CountryID)
                {
                    return ztr.TaxRate;
                }
            }
            return 0.0M;
        }

        public decimal GetTaxRateUSOnly(string ZipCode, int TaxClassID)
        {
            if (ZipCode == string.Empty)
            {
                return 0.0M;
            }
            for (int i = 0; i < m_ZipTaxRates.Count; i++)
            {
                ZipTaxRate ztr = (ZipTaxRate)m_ZipTaxRates.GetByIndex(i);

                int ziplen = ztr.ZipCode.Trim().Length;

                if (ziplen > 5)
                {
                    ziplen = 5;
                }

                if (ztr.ZipCode.Substring(0, ziplen).Equals(ZipCode.Substring(0, ziplen), StringComparison.InvariantCultureIgnoreCase) && ztr.TaxClassID == TaxClassID)
                {
                    return ztr.TaxRate;
                }
            }
            return 0.0M;
        }

        /// <summary>
        /// Adds an existing StateTaxRate object to the collection
        /// </summary>
        public void Add(ZipTaxRate ziptaxrate)
        {
            m_ZipTaxRates.Add(ziptaxrate.ZipTaxID, ziptaxrate);
        }

        /// <summary>
        /// Creates a new CountryTaxRate record and adds it to the collection
        /// </summary>
        public void Add(string ZipCode, int TaxClassID, decimal TaxRate, int CountryId)
        {
            ZipTaxRate ztr = ZipTaxRate.Create(ZipCode, TaxClassID, TaxRate, CountryId);
            this.Add(ztr);
        }

        /// <summary>
        /// Add an existing TaxRate with modified Country ID to the collection. This happens if you changed the existing country.
        /// </summary>
        public void AddNewRate(int ZipTaxID, string ZipCode, int TaxClassID, decimal TaxRate, int CountryID)
        {
            m_ZipTaxRates.Remove(ZipTaxID);
            m_ZipTaxRates.Add(ZipTaxID, new ZipTaxRate(ZipTaxID, ZipCode, TaxClassID, string.Empty, TaxRate, DateTime.Today, CountryID));
        }

        /// <summary>
        /// Deletes the AppConfig record and removes the item from the collection
        /// </summary>
        public void Remove(int ziptaxid)
        {


            DB.ExecuteSQL(string.Format("DELETE FROM ZipTaxRate WHERE ZipCode='ZIP'", ziptaxid));
            m_ZipTaxRates.Remove(ziptaxid);

            //try
            //{
            //    ZipTaxRateController.Delete(ziptaxid);
            //    m_ZipTaxRates.Remove(ziptaxid);
            //}
            //catch { }
        }

        public void RemoveAll(string zipCode)
        {
            try
            {

                List<ZipTaxRate> itemsToBeRemoved = new List<ZipTaxRate>();
                foreach (int ziptaxid in this.m_ZipTaxRates.Keys)
                {
                    ZipTaxRate taxRate = this.m_ZipTaxRates[ziptaxid] as ZipTaxRate;
                    if (taxRate.ZipCode.Equals(zipCode, StringComparison.InvariantCultureIgnoreCase))
                    {
                        itemsToBeRemoved.Add(taxRate);
                    }
                }

                foreach (ZipTaxRate taxRate in itemsToBeRemoved)
                {
                    this.m_ZipTaxRates.Remove(taxRate.ZipTaxID);
                }
            }
            catch { }
        }

        public void RemoveAll(string ZipCode, int CountryID)
        {
            try
            {

                List<ZipTaxRate> itemsToBeRemoved = new List<ZipTaxRate>();
                foreach (int ziptaxid in this.m_ZipTaxRates.Keys)
                {
                    ZipTaxRate taxRate = this.m_ZipTaxRates[ziptaxid] as ZipTaxRate;
                    if (taxRate.ZipCode.Equals(ZipCode, StringComparison.InvariantCultureIgnoreCase) && taxRate.CountryID.Equals(CountryID))
                    {
                        itemsToBeRemoved.Add(taxRate);
                    }
                }

                foreach (ZipTaxRate taxRate in itemsToBeRemoved)
                {
                    this.m_ZipTaxRates.Remove(taxRate.ZipTaxID);
                }
            }
            catch { }
        }

        public int Count
        {
            get { return m_ZipTaxRates.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return new ZipTaxRatesEnumerator(this);
        }

        public ICollection All
        {
            get { return m_ZipTaxRates.Values; }
        }

    }

    public class ZipTaxRatesEnumerator : IEnumerator
    {
        private int position = -1;
        private ZipTaxRates m_ziptaxrates;

        public ZipTaxRatesEnumerator(ZipTaxRates ziptaxratescol)
        {
            this.m_ziptaxrates = ziptaxratescol;
        }

        public bool MoveNext()
        {
            if (position < m_ziptaxrates.m_ZipTaxRates.Count - 1)
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
                return m_ziptaxrates.m_ZipTaxRates[position];
            }
        }
    }
    
}
