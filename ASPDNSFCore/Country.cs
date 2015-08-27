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
using System.Data.SqlClient;
using System.Data;

namespace AspDotNetStorefrontCore
{
    public class Country
    {
        private int m_id;
        private string m_name;

        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public static List<Country> GetAll()
        {
            List<Country> listCountry = new List<Country>();

            using (SqlConnection conn = DB.dbConn())
            {
                try
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("SELECT CountryID,Name FROM Country WHERE Published={0} ORDER BY DisplayOrder, Name", 1),conn))
                    {
                        while (rs.Read())
                        {
                            listCountry.Add(new Country { ID = DB.RSFieldInt(rs, "CountryID"), Name = DB.RSField(rs, "Name") });
                        }
                    }
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return listCountry;
        }

    }

    public class State
    {
        private string m_abbreviation;
        private string m_name;

        public string Abbreviation
        {
            get { return m_abbreviation; }
            set { m_abbreviation = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public static List<State> GetAllStateForCountry(int CountryID, String LocaleSetting)
        {
            List<State> listState = new List<State>();

            using (SqlConnection conn = DB.dbConn())
            {
                try
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("SELECT Abbreviation,Name FROM State WHERE Published={0} and CountryID={1}", 1, CountryID),conn))
                    {
                        while (rs.Read())
                        {
                            listState.Add(new State { Abbreviation = DB.RSField(rs, "Abbreviation"), Name = DB.RSField(rs, "Name") });
                        }
                    }
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }

                if (listState.Count == 0)
                {
                    listState.Add(new State { Abbreviation = "--", Name = AppLogic.GetString("state.countrywithoutstates", LocaleSetting) });
                }
            }

            return listState;
        }

    }
}
