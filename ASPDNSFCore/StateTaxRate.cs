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
    public class StateTaxRate
    {

        #region Private Variables
        private int m_Statetaxid;
        private int m_Stateid;
        private string m_State;
        private int m_Taxclassid;
        private string m_Taxclass;
        private decimal m_Taxrate;
        private DateTime m_Createdon;

        #endregion


        #region Constructors

        public StateTaxRate(int StateTaxID)
        {
            using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                using (IDataReader rs = DB.GetRS("aspdnsf_getStateTaxRateByID " + StateTaxID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        m_Statetaxid = DB.RSFieldInt(rs, "StateTaxID");
                        m_Stateid = DB.RSFieldInt(rs, "StateID");
                        m_State = DB.RSField(rs, "State");
                        m_Taxclassid = DB.RSFieldInt(rs, "TaxClassID");
                        m_Taxclass = DB.RSField(rs, "TaxClass");
                        m_Taxrate = DB.RSFieldDecimal(rs, "TaxRate");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                    }
                    else
                    {
                        m_Statetaxid = 0;
                        m_Stateid = 0;
                        m_State = "";
                        m_Taxclassid = 0;
                        m_Taxclass = "";
                        m_Taxrate = 0.0M;
                        m_Createdon = DateTime.MinValue;
                    }
                }
            }
        }

        public StateTaxRate(int StateID, int TaxClassID)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                using (IDataReader rs = DB.GetRS("aspdnsf_getStateTaxRate " + StateID.ToString() + "," + TaxClassID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        m_Statetaxid = DB.RSFieldInt(rs, "StateTaxID");
                        m_Stateid = DB.RSFieldInt(rs, "StateID");
                        m_State = DB.RSField(rs, "State");
                        m_Taxclassid = DB.RSFieldInt(rs, "TaxClassID");
                        m_Taxclass = DB.RSField(rs, "TaxClass");
                        m_Taxrate = DB.RSFieldDecimal(rs, "TaxRate");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                    }
                    else
                    {
                        m_Statetaxid = 0;
                        m_Stateid = 0;
                        m_State = "";
                        m_Taxclassid = 0;
                        m_Taxclass = "";
                        m_Taxrate = 0.0M;
                        m_Createdon = DateTime.MinValue;
                    }
                }
            }
        }

        public StateTaxRate(int StateTaxID, int StateID, int TaxClassID, string StateName, string TaxClass, decimal TaxRate, DateTime CreatedOn)
        {
            m_Statetaxid = StateTaxID;
            m_Stateid = StateID;
            m_State = StateName;
            m_Taxclassid = TaxClassID;
            m_Taxclass = TaxClass;
            m_Taxrate = TaxRate;
            m_Createdon = CreatedOn;
        }

        #endregion


        #region Static Methods

        static public StateTaxRate Create(int StateID, int TaxClassID, decimal TaxRate)
        {
            int StateTaxID = 0;
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insStateTaxRate";

            cmd.Parameters.Add(new SqlParameter("@StateID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxClassID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxRate", SqlDbType.Decimal, 8));
            cmd.Parameters.Add(new SqlParameter("@StateTaxID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@StateID"].Value = StateID;
            cmd.Parameters["@TaxClassID"].Value = TaxClassID;
            cmd.Parameters["@TaxRate"].Value = TaxRate;

            try
            {
                cmd.ExecuteNonQuery();
                StateTaxID = Int32.Parse(cmd.Parameters["@StateTaxID"].Value.ToString());
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            if (StateTaxID > 0)
            {
                StateTaxRate str = new StateTaxRate(StateTaxID);
                return str;
            }

            return null;
        }

        static public string UpdateStateTaxRate(int StateTaxID, object TaxRate)
        {
            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updStateTaxRate";

            cmd.Parameters.Add(new SqlParameter("@StateTaxID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@TaxRate", SqlDbType.Money, 4));


            cmd.Parameters["@StateTaxID"].Value = StateTaxID;

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
            err = UpdateStateTaxRate(m_Statetaxid, TaxRate);
            if (err == string.Empty)
            {
                m_Taxrate = (decimal)TaxRate;
            }

            return err;
        }
        
        #endregion


        #region Public Properties

        public int StateTaxID
        {
            get { return m_Statetaxid; }
        }


        public int StateID
        {
            get { return m_Stateid; }
        }

        public string State
        {
            get { return m_State; }
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

    public class StateTaxRates : IEnumerable
    {
        public SortedList m_StateTaxRates;

        public StateTaxRates()
        {
            m_StateTaxRates = new SortedList();

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getStateTaxRate", con))
                {
                    while (rs.Read())
                    {
                        m_StateTaxRates.Add(DB.RSFieldInt(rs, "StateTaxID"), new StateTaxRate(DB.RSFieldInt(rs, "StateTaxID"), DB.RSFieldInt(rs, "StateID"), DB.RSFieldInt(rs, "TaxClassID"), DB.RSField(rs, "StateName"), DB.RSField(rs, "TaxClass"), DB.RSFieldDecimal(rs, "TaxRate"), DB.RSFieldDateTime(rs, "CreatedOn")));
                    }
                }
            }
        }


        public StateTaxRate this[int statetaxid]
        {
            get
            {
                return (StateTaxRate)m_StateTaxRates[statetaxid];
            }
        }

        public StateTaxRate this[int StateID, int TaxClassID]
        {
            get
            {
                for (int i = 0; i < m_StateTaxRates.Count; i++)
                {
                    StateTaxRate str = (StateTaxRate)m_StateTaxRates.GetByIndex(i);
                    if (str.StateID == StateID && str.TaxClassID == TaxClassID)
                    {
                        return str;
                    }
                }
                return null;
            }
        }

        public decimal GetTaxRate(int StateID, int TaxClassID)
        {
            if (StateID == 0)
            {
                return 0.0M;
            }

            for (int i = 0; i < m_StateTaxRates.Count; i++)
            {
                StateTaxRate str = (StateTaxRate)m_StateTaxRates.GetByIndex(i);
                if (str.StateID == StateID && str.TaxClassID == TaxClassID)
                {
                    return str.TaxRate;
                }
            }
            return 0.0M;
        }


        /// <summary>
        /// Adds an existing StateTaxRate object to the collection
        /// </summary>
        public void Add(StateTaxRate statetaxrate)
        {
            m_StateTaxRates.Add(statetaxrate.StateTaxID, statetaxrate);
        }

        /// <summary>
        /// Creates a new CountryTaxRate record and adds it to the collection
        /// </summary>
        public void Add(int StateID, int TaxClassID, decimal TaxRate)
        {
            StateTaxRate str = StateTaxRate.Create(StateID, TaxClassID, TaxRate);
            this.Add(str);
        }

        /// <summary>
        /// Deletes the AppConfig record and removes the item from the collection
        /// </summary>
        public void Remove(int statetaxid)
        {
            try
            {
                DB.ExecuteSQL("delete dbo.StateTaxRate where StateTaxID = " + statetaxid.ToString());
                m_StateTaxRates.Remove(statetaxid);
            }
            catch { }
        }

        public int Count
        {
            get { return m_StateTaxRates.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return new StateTaxRatesEnumerator(this);
        }

    }

    public class StateTaxRatesEnumerator : IEnumerator
    {
        private int position = -1;
        private StateTaxRates m_statetaxrates;

        public StateTaxRatesEnumerator(StateTaxRates statetaxratescol)
        {
            this.m_statetaxrates = statetaxratescol;
        }

        public bool MoveNext()
        {
            if (position < m_statetaxrates.m_StateTaxRates.Count - 1)
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
                return m_statetaxrates.m_StateTaxRates[position];
            }
        }
    }

}
