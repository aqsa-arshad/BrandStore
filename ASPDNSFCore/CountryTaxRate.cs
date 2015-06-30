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
    public class CountryTaxRate
    {
        #region Private Variables
        private int m_Countrytaxid;
        private int m_Countryid;
        private int m_Taxclassid;
        private string m_Taxclass;
        private string m_Country;
        private decimal m_Taxrate;
        private DateTime m_Createdon;
        #endregion


        #region Constructors

        public CountryTaxRate(int CountryTaxID)
        {
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getCountryTaxRateByID " + CountryTaxID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_Countrytaxid = DB.RSFieldInt(rs, "CountryTaxID");
                        m_Countryid = DB.RSFieldInt(rs, "CountryID");
                        m_Country = DB.RSField(rs, "Country");
                        m_Taxclassid = DB.RSFieldInt(rs, "TaxClassID");
                        m_Taxclass = DB.RSField(rs, "TaxClass");
                        m_Taxrate = DB.RSFieldDecimal(rs, "TaxRate");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                    }
                    else
                    {
                        m_Countrytaxid = 0;
                        m_Countryid = 0;
                        m_Country = "";
                        m_Taxclassid = 0;
                        m_Taxclass = "";
                        m_Taxrate = System.Decimal.Zero;
                        m_Createdon = DateTime.MinValue;
                    }
                }
            }           
        }

        public CountryTaxRate(int CountryID, int TaxClassID)
        {
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getCountryTaxRate " + CountryID.ToString() + "," + TaxClassID.ToString(), conn))
                {
                    if (rs.Read())
                    {
                        m_Countrytaxid = DB.RSFieldInt(rs, "CountryTaxID");
                        m_Countryid = DB.RSFieldInt(rs, "CountryID");
                        m_Country = DB.RSField(rs, "Country");
                        m_Taxclassid = DB.RSFieldInt(rs, "TaxClassID");
                        m_Taxclass = DB.RSField(rs, "TaxClass");
                        m_Taxrate = DB.RSFieldDecimal(rs, "TaxRate");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                    }
                    else
                    {
                        m_Countryid = 0;
                        m_Country = "";
                        m_Taxclassid = 0;
                        m_Taxclass = "";
                        m_Taxrate = System.Decimal.Zero;
                        m_Createdon = DateTime.MinValue;
                    }
                }
            }
        }

        public CountryTaxRate(int CountryTaxID, int CountryID, int TaxClassID, string CountryName, string TaxClass, decimal TaxRate, DateTime CreatedOn)
        {
            m_Countrytaxid = CountryTaxID;
            m_Countryid = CountryID;
            m_Country = CountryName;
            m_Taxclassid = TaxClassID;
            m_Taxclass = TaxClass;
            m_Taxrate = TaxRate;
            m_Createdon = CreatedOn;
        }

        #endregion


        #region Static Methods

        static public CountryTaxRate Create(int CountryID, int TaxClassID, decimal TaxRate)
        {
            int CountryTaxID = 0;
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insCountryTaxRate";

            cmd.Parameters.Add(new SqlParameter("@CountryID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxClassID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxRate", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@CountryTaxID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@CountryID"].Value = CountryID;
            cmd.Parameters["@TaxClassID"].Value = TaxClassID;
            cmd.Parameters["@TaxRate"].Value = TaxRate;


            try
            {
                cmd.ExecuteNonQuery();
                CountryTaxID = Int32.Parse(cmd.Parameters["@CountryTaxID"].Value.ToString());
            }
            catch (Exception ex)
            {
                err = ex.Message;
                
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            if (CountryTaxID > 0)
            {
                CountryTaxRate ctr = new CountryTaxRate(CountryTaxID);
                return ctr;
            }

            return null;
        }

        static public string UpdateCountryTaxRate(int CountryTaxID, object TaxRate)
        {
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updCountryTaxRate";

            cmd.Parameters.Add(new SqlParameter("@CountryTaxID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxRate", SqlDbType.Decimal, 8));

            cmd.Parameters["@CountryTaxID"].Value = CountryTaxID;

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

        public string Update(object TaxRate)
        {
            string err = string.Empty;            
            err = UpdateCountryTaxRate(m_Countrytaxid, TaxRate);
            if (err == string.Empty)
            {
                m_Taxrate = (decimal)TaxRate;
            }

            return err;
        }
        
        #endregion


        #region Public Properties

        public int CountryTaxID
        {
            get { return m_Countrytaxid; }
        }

        public int CountryID
        {
            get { return m_Countryid; }
        }

        public string Country
        {
            get { return m_Country; }
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
        
        #endregion



    }

    public class CountryTaxRates : IEnumerable
    {
        public SortedList m_CountryTaxRates;

        public CountryTaxRates()
        {
            m_CountryTaxRates = new SortedList();
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getCountryTaxRate", conn))
                {
                    while (rs.Read())
                    {
                        m_CountryTaxRates.Add(DB.RSFieldInt(rs, "CountryTaxID"), new CountryTaxRate(DB.RSFieldInt(rs, "CountryTaxID"), DB.RSFieldInt(rs, "CountryID"), DB.RSFieldInt(rs, "TaxClassID"), DB.RSField(rs, "Country"), DB.RSField(rs, "TaxClass"), DB.RSFieldDecimal(rs, "TaxRate"), DB.RSFieldDateTime(rs, "CreatedOn")));
                    }
                }
            }
        }


        public CountryTaxRate this[int countrytaxid]
        {
            get
            {
                return (CountryTaxRate)m_CountryTaxRates[countrytaxid.ToString()];
            }
        }

        public CountryTaxRate this[int CountryID, int TaxClassID]
        {
            get
            {
                for (int i = 0; i < m_CountryTaxRates.Count; i++)
                {
                    CountryTaxRate str = (CountryTaxRate)m_CountryTaxRates.GetByIndex(i);
                    if (str.CountryID == CountryID && str.TaxClassID == TaxClassID)
                    {
                        return str;
                    }
                }
                return null;
            }
        }

        public decimal GetTaxRate(int CountryID, int TaxClassID)
        {
            if (CountryID == 0)
            {
                return System.Decimal.Zero;
            }
            for (int i = 0; i < m_CountryTaxRates.Count; i++)
            {
                CountryTaxRate ctr = (CountryTaxRate)m_CountryTaxRates.GetByIndex(i);
                if (ctr.CountryID == CountryID && ctr.TaxClassID == TaxClassID)
                {
                    return ctr.TaxRate;
                }
            }
            return System.Decimal.Zero;
        }

        /// <summary>
        /// Adds an existing CountryTaxRate object to the collection
        /// </summary>
        public void Add(CountryTaxRate countrytaxrate)
        {
            m_CountryTaxRates.Add(countrytaxrate.CountryTaxID, countrytaxrate);
        }

        /// <summary>
        /// Creates a new CountryTaxRate record and adds it to the collection
        /// </summary>
        public void Add(int CountryID, int TaxClassID, decimal TaxRate)
        {
            this.Add(CountryTaxRate.Create(CountryID, TaxClassID, TaxRate));
        }

        /// <summary>
        /// Deletes the AppConfig record and removes the item from the collection
        /// </summary>
        public void Remove(int countrytaxid)
        {
            try
            {
                DB.ExecuteSQL("delete dbo.CountryTaxRate where CountryTaxRateID = " + countrytaxid.ToString());
                m_CountryTaxRates.Remove(countrytaxid);
            }
            catch { }
        }

        public int Count
        {
            get { return m_CountryTaxRates.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return new CountryTaxRatesEnumerator(this);
        }

    }

    public class CountryTaxRatesEnumerator : IEnumerator
    {
        private int position = -1;
        private CountryTaxRates m_countrytaxrates;

        public CountryTaxRatesEnumerator(CountryTaxRates countrytaxratescol)
        {
            this.m_countrytaxrates = countrytaxratescol;
        }

        public bool MoveNext()
        {
            if (position < m_countrytaxrates.m_CountryTaxRates.Count - 1)
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
                return m_countrytaxrates.m_CountryTaxRates[position];
            }
        }
    }

}
