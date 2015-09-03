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

namespace AspDotNetStorefrontCore
{
    public class Affiliate
    {

        #region private variables

        private int m_Affiliateid = 0;
        private Guid m_Affiliateguid;
        private string m_Email;
        private string m_Password;
        private DateTime m_Dateofbirth;
        private string m_Gender;
        private string m_Notes;
        private bool m_Isonline;
        private string m_Firstname;
        private string m_Lastname;
        private string m_Name;
        private string m_Company;
        private string m_Address1;
        private string m_Address2;
        private string m_Suite;
        private string m_City;
        private string m_State;
        private string m_Zip;
        private string m_Country;
        private string m_Phone;
        private string m_Websitename;
        private string m_Websitedescription;
        private string m_Url;
        private bool m_Trackingonly;
        private int m_Defaultskinid;
        private int m_Parentaffiliateid;
        private int m_Displayorder;
        private string m_Extensiondata;
        private string m_Sename;
        private string m_Setitle;
        private string m_Senoscript;
        private string m_Sealttext;
        private string m_Sekeywords;
        private string m_Sedescription;
        private bool m_Published;
        private bool m_Wholesale;
        private bool m_Deleted;
        private DateTime m_Createdon;
        private int m_Saltkey = -1;

        #endregion

        #region Constructors
 
        public Affiliate() { }

        public Affiliate(int AffiliateID)
        {
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS("aspdnsf_getAffiliate " + AffiliateID.ToString(), dbconn))
                {
                    if (dr.Read())
                    {
                        m_Affiliateid = DB.RSFieldInt(dr, "AffiliateID");
                        m_Affiliateguid = DB.RSFieldGUID2(dr, "AffiliateGUID");
                        m_Email = DB.RSField(dr, "EMail");
                        m_Password = DB.RSField(dr, "Password");
                        m_Dateofbirth = DB.RSFieldDateTime(dr, "DateOfBirth");
                        m_Gender = DB.RSField(dr, "Gender");
                        m_Notes = DB.RSField(dr, "Notes");
                        m_Isonline = DB.RSFieldBool(dr, "IsOnline");
                        m_Firstname = DB.RSField(dr, "FirstName");
                        m_Lastname = DB.RSField(dr, "LastName");
                        m_Name = DB.RSField(dr, "Name");
                        m_Company = DB.RSField(dr, "Company");
                        m_Address1 = DB.RSField(dr, "Address1");
                        m_Address2 = DB.RSField(dr, "Address2");
                        m_Suite = DB.RSField(dr, "Suite");
                        m_City = DB.RSField(dr, "City");
                        m_State = DB.RSField(dr, "State");
                        m_Zip = DB.RSField(dr, "Zip");
                        m_Country = DB.RSField(dr, "Country");
                        m_Phone = DB.RSField(dr, "Phone");
                        m_Websitename = DB.RSField(dr, "WebSiteName");
                        m_Websitedescription = DB.RSField(dr, "WebSiteDescription");
                        m_Url = DB.RSField(dr, "URL");
                        m_Trackingonly = DB.RSFieldBool(dr, "TrackingOnly");
                        m_Defaultskinid = DB.RSFieldInt(dr, "DefaultSkinID");
                        m_Parentaffiliateid = DB.RSFieldInt(dr, "ParentAffiliateID");
                        m_Displayorder = DB.RSFieldInt(dr, "DisplayOrder");
                        m_Extensiondata = DB.RSField(dr, "ExtensionData");
                        m_Sename = DB.RSField(dr, "SEName");
                        m_Setitle = DB.RSField(dr, "SETitle");
                        m_Senoscript = DB.RSField(dr, "SENoScript");
                        m_Sealttext = DB.RSField(dr, "SEAltText");
                        m_Sekeywords = DB.RSField(dr, "SEKeywords");
                        m_Sedescription = DB.RSField(dr, "SEDescription");
                        m_Published = DB.RSFieldBool(dr, "Published");
                        m_Wholesale = DB.RSFieldBool(dr, "Wholesale");
                        m_Deleted = DB.RSFieldBool(dr, "Deleted");
                        m_Createdon = DB.RSFieldDateTime(dr, "CreatedOn");
                        m_Saltkey = DB.RSFieldInt(dr, "SaltKey");
                        StoreID = DB.RSFieldInt(dr, "StoreID");
                    }
                }
            }           
        }

        public Affiliate(string AffiliateEmail)
        {
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader dr = DB.GetRS("aspdnsf_getAffiliateByEmail " + DB.SQuote(AffiliateEmail.ToString()), dbconn))
                {
                    if (dr.Read())
                    {
                        m_Affiliateid = DB.RSFieldInt(dr, "AffiliateID");
                        m_Affiliateguid = DB.RSFieldGUID2(dr, "AffiliateGUID");
                        m_Email = DB.RSField(dr, "EMail");
                        m_Password = DB.RSField(dr, "Password");
                        m_Dateofbirth = DB.RSFieldDateTime(dr, "DateOfBirth");
                        m_Gender = DB.RSField(dr, "Gender");
                        m_Notes = DB.RSField(dr, "Notes");
                        m_Isonline = DB.RSFieldBool(dr, "IsOnline");
                        m_Firstname = DB.RSField(dr, "FirstName");
                        m_Lastname = DB.RSField(dr, "LastName");
                        m_Name = DB.RSField(dr, "Name");
                        m_Company = DB.RSField(dr, "Company");
                        m_Address1 = DB.RSField(dr, "Address1");
                        m_Address2 = DB.RSField(dr, "Address2");
                        m_Suite = DB.RSField(dr, "Suite");
                        m_City = DB.RSField(dr, "City");
                        m_State = DB.RSField(dr, "State");
                        m_Zip = DB.RSField(dr, "Zip");
                        m_Country = DB.RSField(dr, "Country");
                        m_Phone = DB.RSField(dr, "Phone");
                        m_Websitename = DB.RSField(dr, "WebSiteName");
                        m_Websitedescription = DB.RSField(dr, "WebSiteDescription");
                        m_Url = DB.RSField(dr, "URL");
                        m_Trackingonly = DB.RSFieldBool(dr, "TrackingOnly");
                        m_Defaultskinid = DB.RSFieldInt(dr, "DefaultSkinID");
                        m_Parentaffiliateid = DB.RSFieldInt(dr, "ParentAffiliateID");
                        m_Displayorder = DB.RSFieldInt(dr, "DisplayOrder");
                        m_Extensiondata = DB.RSField(dr, "ExtensionData");
                        m_Sename = DB.RSField(dr, "SEName");
                        m_Setitle = DB.RSField(dr, "SETitle");
                        m_Senoscript = DB.RSField(dr, "SENoScript");
                        m_Sealttext = DB.RSField(dr, "SEAltText");
                        m_Sekeywords = DB.RSField(dr, "SEKeywords");
                        m_Sedescription = DB.RSField(dr, "SEDescription");
                        m_Published = DB.RSFieldBool(dr, "Published");
                        m_Wholesale = DB.RSFieldBool(dr, "Wholesale");
                        m_Deleted = DB.RSFieldBool(dr, "Deleted");
                        m_Createdon = DB.RSFieldDateTime(dr, "CreatedOn");
                        m_Saltkey = DB.RSFieldInt(dr, "SaltKey");
                        StoreID = DB.RSFieldInt(dr, "StoreID");
                    }
                }
            }            
        }

        public Affiliate(int Affiliateid, Guid Affiliateguid, string Email, string Password, DateTime Dateofbirth, string Gender, string Notes, bool Isonline, string Firstname, string Lastname, string Name, string Company, string Address1, string Address2, string Suite, string City, string State, string Zip, string Country, string Phone, string Websitename, string Websitedescription, string Url, bool Trackingonly, int Defaultskinid, int Parentaffiliateid, int Displayorder, string Extensiondata, string Sename, string Setitle, string Senoscript, string Sealttext, string Sekeywords, string Sedescription, bool Published, bool Wholesale, bool Deleted, DateTime Createdon)
        {
            m_Affiliateid = Affiliateid;
            m_Affiliateguid = Affiliateguid;
            m_Email = Email;
            m_Password = Password;
            m_Dateofbirth = Dateofbirth;
            m_Gender = Gender;
            m_Notes = Notes;
            m_Isonline = Isonline;
            m_Firstname = Firstname;
            m_Lastname = Lastname;
            m_Name = Name;
            m_Company = Company;
            m_Address1 = Address1;
            m_Address2 = Address2;
            m_Suite = Suite;
            m_City = City;
            m_State = State;
            m_Zip = Zip;
            m_Country = Country;
            m_Phone = Phone;
            m_Websitename = Websitename;
            m_Websitedescription = Websitedescription;
            m_Url = Url;
            m_Trackingonly = Trackingonly;
            m_Defaultskinid = Defaultskinid;
            m_Parentaffiliateid = Parentaffiliateid;
            m_Displayorder = Displayorder;
            m_Extensiondata = Extensiondata;
            m_Sename = Sename;
            m_Setitle = Setitle;
            m_Senoscript = Senoscript;
            m_Sealttext = Sealttext;
            m_Sekeywords = Sekeywords;
            m_Sedescription = Sedescription;
            m_Published = Published;
            m_Wholesale = Wholesale;
            m_Deleted = Deleted;
            m_Createdon = Createdon;
        }


        #endregion

        #region Static Methods

        public static Affiliate CreateAffiliate(string EMail, string Password, object DateOfBirth, string Gender, string Notes, bool IsOnline, string FirstName, string LastName, string Name, string Company, string Address1, string Address2, string Suite, string City, string State, string Zip, string Country, string Phone, string WebSiteName, string WebSiteDescription, string URL, bool TrackingOnly, int DefaultSkinID, int ParentAffiliateID, int DisplayOrder, string ExtensionData, string SEName, string SETitle, string SENoScript, string SEAltText, string SEKeywords, string SEDescription, bool Wholesale, int SaltKey)
        {
            int AffiliateID = 0;
            string err = String.Empty;

            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insAffiliate";

            cmd.Parameters.Add(new SqlParameter("@EMail", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 500));
            cmd.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@Gender", SqlDbType.NVarChar, 2));
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.Text));
            cmd.Parameters.Add(new SqlParameter("@IsOnline", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Company", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Address1", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Address2", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Suite", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@City", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Zip", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@Country", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
            cmd.Parameters.Add(new SqlParameter("@WebSiteName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@WebSiteDescription", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@URL", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@TrackingOnly", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@DefaultSkinID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ParentAffiliateID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@DisplayOrder", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SEName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@SETitle", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SENoScript", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SEAltText", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SEKeywords", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SEDescription", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@Wholesale", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@SaltKey", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@StoreID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@EMail"].Value = EMail;
            cmd.Parameters["@Password"].Value = Password;
            if (DateOfBirth == null) cmd.Parameters["@DateOfBirth"].Value = DBNull.Value;
            else cmd.Parameters["@DateOfBirth"].Value = DateOfBirth;

            if (Gender == null) cmd.Parameters["@Gender"].Value = DBNull.Value;
            else cmd.Parameters["@Gender"].Value = Gender;

            if (Notes == null) cmd.Parameters["@Notes"].Value = DBNull.Value;
            else cmd.Parameters["@Notes"].Value = Notes;

            cmd.Parameters["@IsOnline"].Value = IsOnline;

            if (FirstName == null) cmd.Parameters["@FirstName"].Value = DBNull.Value;
            else cmd.Parameters["@FirstName"].Value = FirstName;

            if (LastName == null) cmd.Parameters["@LastName"].Value = DBNull.Value;
            else cmd.Parameters["@LastName"].Value = LastName;

            if (Name == null) cmd.Parameters["@Name"].Value = DBNull.Value;
            else cmd.Parameters["@Name"].Value = Name;

            if (Company == null) cmd.Parameters["@Company"].Value = DBNull.Value;
            else cmd.Parameters["@Company"].Value = Company;

            if (Address1 == null) cmd.Parameters["@Address1"].Value = DBNull.Value;
            else cmd.Parameters["@Address1"].Value = Address1;

            if (Address2 == null) cmd.Parameters["@Address2"].Value = DBNull.Value;
            else cmd.Parameters["@Address2"].Value = Address2;

            if (Suite == null) cmd.Parameters["@Suite"].Value = DBNull.Value;
            else cmd.Parameters["@Suite"].Value = Suite;

            if (City == null) cmd.Parameters["@City"].Value = DBNull.Value;
            else cmd.Parameters["@City"].Value = City;

            if (State == null) cmd.Parameters["@State"].Value = DBNull.Value;
            else cmd.Parameters["@State"].Value = State;

            if (Zip == null) cmd.Parameters["@Zip"].Value = DBNull.Value;
            else cmd.Parameters["@Zip"].Value = Zip;

            if (Country == null) cmd.Parameters["@Country"].Value = DBNull.Value;
            else cmd.Parameters["@Country"].Value = Country;

            if (Phone == null) cmd.Parameters["@Phone"].Value = DBNull.Value;
            else cmd.Parameters["@Phone"].Value = Phone;

            if (WebSiteName == null) cmd.Parameters["@WebSiteName"].Value = DBNull.Value;
            else cmd.Parameters["@WebSiteName"].Value = WebSiteName;

            if (WebSiteDescription == null) cmd.Parameters["@WebSiteDescription"].Value = DBNull.Value;
            else cmd.Parameters["@WebSiteDescription"].Value = WebSiteDescription;

            if (URL == null) cmd.Parameters["@URL"].Value = DBNull.Value;
            else cmd.Parameters["@URL"].Value = URL;

            cmd.Parameters["@TrackingOnly"].Value = TrackingOnly;

            cmd.Parameters["@DefaultSkinID"].Value = DefaultSkinID;

            cmd.Parameters["@ParentAffiliateID"].Value = ParentAffiliateID;

            cmd.Parameters["@DisplayOrder"].Value = DisplayOrder;

            if (ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
            else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

            if (SEName == null) cmd.Parameters["@SEName"].Value = DBNull.Value;
            else cmd.Parameters["@SEName"].Value = SEName;

            if (SETitle == null) cmd.Parameters["@SETitle"].Value = DBNull.Value;
            else cmd.Parameters["@SETitle"].Value = SETitle;

            if (SENoScript == null) cmd.Parameters["@SENoScript"].Value = DBNull.Value;
            else cmd.Parameters["@SENoScript"].Value = SENoScript;

            if (SEAltText == null) cmd.Parameters["@SEAltText"].Value = DBNull.Value;
            else cmd.Parameters["@SEAltText"].Value = SEAltText;

            if (SEKeywords == null) cmd.Parameters["@SEKeywords"].Value = DBNull.Value;
            else cmd.Parameters["@SEKeywords"].Value = SEKeywords;

            if (SEDescription == null) cmd.Parameters["@SEDescription"].Value = DBNull.Value;
            else cmd.Parameters["@SEDescription"].Value = SEDescription;

            cmd.Parameters["@Wholesale"].Value = Wholesale;

            cmd.Parameters["@SaltKey"].Value = SaltKey;

            cmd.Parameters["@StoreID"].Value = AppLogic.StoreID();

            try
            {
                cmd.ExecuteNonQuery();
                AffiliateID = Int32.Parse(cmd.Parameters["@AffiliateID"].Value.ToString());
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            if (AffiliateID > 0)
            {
                Affiliate a = new Affiliate(AffiliateID);
                return a;
            }
            return null;
        }

        public static string Update(int AffiliateID, SqlParameter[] spa)
        {
            string err = String.Empty;

            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updAffiliate";

            SqlParameter sqlparam = new SqlParameter("@AffiliateID", SqlDbType.Int, 4);
            sqlparam.Value = AffiliateID;
            cmd.Parameters.Add(sqlparam);
            foreach (SqlParameter sp in spa)
            {
                cmd.Parameters.Add(sp);
            }
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

        public static string Update(int AffiliteID, string EMail, string Password, object DateOfBirth, string Gender, string Notes, object IsOnline, string FirstName, string LastName, string Name, string Company, string Address1, string Address2, string Suite, string City, string State, string Zip, string Country, string Phone, string WebSiteName, string WebSiteDescription, string URL, object TrackingOnly, object DefaultSkinID, object ParentAffiliateID, object DisplayOrder, string ExtensionData, string SEName, string SETitle, string SENoScript, string SEAltText, string SEKeywords, string SEDescription, object Published, object Wholesale, object Deleted, object SaltKey)
        {
            string err = String.Empty;

            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updAffiliate";

            cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@EMail", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 500));
            cmd.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.VarChar, 30));
            cmd.Parameters.Add(new SqlParameter("@Gender", SqlDbType.NVarChar, 2));
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.Text));
            cmd.Parameters.Add(new SqlParameter("@IsOnline", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Company", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Address1", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Address2", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Suite", SqlDbType.NVarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@City", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Zip", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@Country", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
            cmd.Parameters.Add(new SqlParameter("@WebSiteName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@WebSiteDescription", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@URL", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@TrackingOnly", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@DefaultSkinID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ParentAffiliateID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@DisplayOrder", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SEName", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@SETitle", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SENoScript", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SEAltText", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SEKeywords", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@SEDescription", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@Published", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@Wholesale", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@Deleted", SqlDbType.TinyInt, 1));
            cmd.Parameters.Add(new SqlParameter("@SaltKey", SqlDbType.Int, 4));

            cmd.Parameters["@AffiliateID"].Value = AffiliteID;

            if (EMail == null) cmd.Parameters["@EMail"].Value = DBNull.Value;
            else cmd.Parameters["@EMail"].Value = EMail;

            if (Password == null) cmd.Parameters["@Password"].Value = DBNull.Value;
            else cmd.Parameters["@Password"].Value = Password;

            if (DateOfBirth == null) cmd.Parameters["@DateOfBirth"].Value = DBNull.Value;
            else cmd.Parameters["@DateOfBirth"].Value = DateOfBirth;

            if (Gender == null) cmd.Parameters["@Gender"].Value = DBNull.Value;
            else cmd.Parameters["@Gender"].Value = Gender;

            if (Notes == null) cmd.Parameters["@Notes"].Value = DBNull.Value;
            else cmd.Parameters["@Notes"].Value = Notes;

            if (IsOnline == null) cmd.Parameters["@IsOnline"].Value = DBNull.Value;
            else cmd.Parameters["@IsOnline"].Value = IsOnline;

            if (FirstName == null) cmd.Parameters["@FirstName"].Value = DBNull.Value;
            else cmd.Parameters["@FirstName"].Value = FirstName;

            if (LastName == null) cmd.Parameters["@LastName"].Value = DBNull.Value;
            else cmd.Parameters["@LastName"].Value = LastName;

            if (Name == null) cmd.Parameters["@Name"].Value = DBNull.Value;
            else cmd.Parameters["@Name"].Value = Name;

            if (Company == null) cmd.Parameters["@Company"].Value = DBNull.Value;
            else cmd.Parameters["@Company"].Value = Company;

            if (Address1 == null) cmd.Parameters["@Address1"].Value = DBNull.Value;
            else cmd.Parameters["@Address1"].Value = Address1;

            if (Address2 == null) cmd.Parameters["@Address2"].Value = DBNull.Value;
            else cmd.Parameters["@Address2"].Value = Address2;

            if (Suite == null) cmd.Parameters["@Suite"].Value = DBNull.Value;
            else cmd.Parameters["@Suite"].Value = Suite;

            if (City == null) cmd.Parameters["@City"].Value = DBNull.Value;
            else cmd.Parameters["@City"].Value = City;

            if (State == null) cmd.Parameters["@State"].Value = DBNull.Value;
            else cmd.Parameters["@State"].Value = State;

            if (Zip == null) cmd.Parameters["@Zip"].Value = DBNull.Value;
            else cmd.Parameters["@Zip"].Value = Zip;

            if (Country == null) cmd.Parameters["@Country"].Value = DBNull.Value;
            else cmd.Parameters["@Country"].Value = Country;

            if (Phone == null) cmd.Parameters["@Phone"].Value = DBNull.Value;
            else cmd.Parameters["@Phone"].Value = Phone;

            if (WebSiteName == null) cmd.Parameters["@WebSiteName"].Value = DBNull.Value;
            else cmd.Parameters["@WebSiteName"].Value = WebSiteName;

            if (WebSiteDescription == null) cmd.Parameters["@WebSiteDescription"].Value = DBNull.Value;
            else cmd.Parameters["@WebSiteDescription"].Value = WebSiteDescription;

            if (URL == null) cmd.Parameters["@URL"].Value = DBNull.Value;
            else cmd.Parameters["@URL"].Value = URL;

            if (TrackingOnly == null) cmd.Parameters["@TrackingOnly"].Value = DBNull.Value;
            else cmd.Parameters["@TrackingOnly"].Value = TrackingOnly;

            if (DefaultSkinID == null) cmd.Parameters["@DefaultSkinID"].Value = DBNull.Value;
            else cmd.Parameters["@DefaultSkinID"].Value = DefaultSkinID;

            if (ParentAffiliateID == null) cmd.Parameters["@ParentAffiliateID"].Value = DBNull.Value;
            else cmd.Parameters["@ParentAffiliateID"].Value = ParentAffiliateID;

            if (DisplayOrder == null) cmd.Parameters["@DisplayOrder"].Value = DBNull.Value;
            else cmd.Parameters["@DisplayOrder"].Value = DisplayOrder;

            if (ExtensionData == null) cmd.Parameters["@ExtensionData"].Value = DBNull.Value;
            else cmd.Parameters["@ExtensionData"].Value = ExtensionData;

            if (SEName == null) cmd.Parameters["@SEName"].Value = DBNull.Value;
            else cmd.Parameters["@SEName"].Value = SEName;

            if (SETitle == null) cmd.Parameters["@SETitle"].Value = DBNull.Value;
            else cmd.Parameters["@SETitle"].Value = SETitle;

            if (SENoScript == null) cmd.Parameters["@SENoScript"].Value = DBNull.Value;
            else cmd.Parameters["@SENoScript"].Value = SENoScript;

            if (SEAltText == null) cmd.Parameters["@SEAltText"].Value = DBNull.Value;
            else cmd.Parameters["@SEAltText"].Value = SEAltText;

            if (SEKeywords == null) cmd.Parameters["@SEKeywords"].Value = DBNull.Value;
            else cmd.Parameters["@SEKeywords"].Value = SEKeywords;

            if (SEDescription == null) cmd.Parameters["@SEDescription"].Value = DBNull.Value;
            else cmd.Parameters["@SEDescription"].Value = SEDescription;

            if (Published == null) cmd.Parameters["@Published"].Value = DBNull.Value;
            else cmd.Parameters["@Published"].Value = Published;

            if (Wholesale == null) cmd.Parameters["@Wholesale"].Value = DBNull.Value;
            else cmd.Parameters["@Wholesale"].Value = Wholesale;

            if (Deleted == null) cmd.Parameters["@Deleted"].Value = DBNull.Value;
            else cmd.Parameters["@Deleted"].Value = Deleted;

            if (SaltKey == null) cmd.Parameters["@SaltKey"].Value = DBNull.Value;
            else cmd.Parameters["@SaltKey"].Value = SaltKey;

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

        public static bool EmailInUse(string email)
        {
            string query = string.Format("select count(*) N from dbo.affiliate a with (nolock) inner join (select a.AffiliateID from Affiliate a with (nolock) left join AffiliateStore b with (nolock) " +
                "on a.AffiliateID = b.AffiliateID where ({0} = 0 or StoreID = {1})) B ON A.AffiliateID = B.AffiliateID where email = {2}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowAffiliateFiltering") == true, 1, 0), AppLogic.StoreID(), DB.SQuote(email));
            return (DB.GetSqlN(query) > 0);
        }

        public static bool EmailInUse(string email, int ExcludeAffiliateID)
        {
            string query = string.Format("select count(*) N from dbo.affiliate a with (nolock) inner join (select a.AffiliateID from Affiliate a with (nolock) left join AffiliateStore b with (nolock) on a.AffiliateID = b.AffiliateID where ({0} = 0 or StoreID = {1})) " +
                "B ON A.AffiliateID = B.AffiliateID where email = {2} and A.AffiliateID <> {3}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowAffiliateFiltering") == true, 1, 0), AppLogic.StoreID(), DB.SQuote(email), ExcludeAffiliateID);
            return (DB.GetSqlN(query) > 0);
        }

        public static bool isValidAffiliate(int AffiliateID)
        {
            string query = string.Format("select count(*) N from dbo.affiliate a with (nolock) inner join (select a.AffiliateID from Affiliate a with (nolock) left join AffiliateStore b with (nolock) " +
               "on a.AffiliateID = b.AffiliateID where ({0} = 0 or StoreID = {1})) B ON A.AffiliateID = B.AffiliateID where A.AffiliateID = {2}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowAffiliateFiltering") == true, 1, 0), AppLogic.StoreID(), AffiliateID);
            return (DB.GetSqlN(query) > 0);
        }

        #endregion

        #region Public Methods

        public string Update(SqlParameter[] spa)
        {
            return Affiliate.Update(m_Affiliateid, spa);
        }

        public string Update(string EMail, string Password, object DateOfBirth, string Gender, string Notes, object IsOnline, string FirstName, string LastName, string Name, string Company, string Address1, string Address2, string Suite, string City, string State, string Zip, string Country, string Phone, string WebSiteName, string WebSiteDescription, string URL, object TrackingOnly, object DefaultSkinID, object ParentAffiliateID, object DisplayOrder, string ExtensionData, string SEName, string SETitle, string SENoScript, string SEAltText, string SEKeywords, string SEDescription, object Published, object Wholesale, object Deleted, object SaltKey)
        {
            string err = String.Empty;
            err = Update(m_Affiliateid, EMail, Password, DateOfBirth, Gender, Notes, IsOnline, FirstName, LastName, Name, Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL, TrackingOnly, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SENoScript, SEAltText, SEKeywords, SEDescription, Published, Wholesale, Deleted, SaltKey);
            LoadFromDB(m_Affiliateid);
            return err;
        }

        #endregion


        #region Private Methods

        private void LoadFromDB(int AffiliateID)
        {
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_getAffiliate";

            cmd.Parameters.Add(new SqlParameter("@AffiliateID", SqlDbType.Int, 4));
            cmd.Parameters["@AffiliateID"].Value = AffiliateID;


            try
            {
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    m_Affiliateid = DB.RSFieldInt(dr, "AffiliateID");
                    m_Affiliateguid = DB.RSFieldGUID2(dr, "AffiliateGUID");
                    m_Email = DB.RSField(dr, "EMail");
                    m_Password = DB.RSField(dr, "Password");
                    m_Dateofbirth = DB.RSFieldDateTime(dr, "DateOfBirth");
                    m_Gender = DB.RSField(dr, "Gender");
                    m_Notes = DB.RSField(dr, "Notes");
                    m_Isonline = DB.RSFieldBool(dr, "IsOnline");
                    m_Firstname = DB.RSField(dr, "FirstName");
                    m_Lastname = DB.RSField(dr, "LastName");
                    m_Name = DB.RSField(dr, "Name");
                    m_Company = DB.RSField(dr, "Company");
                    m_Address1 = DB.RSField(dr, "Address1");
                    m_Address2 = DB.RSField(dr, "Address2");
                    m_Suite = DB.RSField(dr, "Suite");
                    m_City = DB.RSField(dr, "City");
                    m_State = DB.RSField(dr, "State");
                    m_Zip = DB.RSField(dr, "Zip");
                    m_Country = DB.RSField(dr, "Country");
                    m_Phone = DB.RSField(dr, "Phone");
                    m_Websitename = DB.RSField(dr, "WebSiteName");
                    m_Websitedescription = DB.RSField(dr, "WebSiteDescription");
                    m_Url = DB.RSField(dr, "URL");
                    m_Trackingonly = DB.RSFieldBool(dr, "TrackingOnly");
                    m_Defaultskinid = DB.RSFieldInt(dr, "DefaultSkinID");
                    m_Parentaffiliateid = DB.RSFieldInt(dr, "ParentAffiliateID");
                    m_Displayorder = DB.RSFieldInt(dr, "DisplayOrder");
                    m_Extensiondata = DB.RSField(dr, "ExtensionData");
                    m_Sename = DB.RSField(dr, "SEName");
                    m_Setitle = DB.RSField(dr, "SETitle");
                    m_Senoscript = DB.RSField(dr, "SENoScript");
                    m_Sealttext = DB.RSField(dr, "SEAltText");
                    m_Sekeywords = DB.RSField(dr, "SEKeywords");
                    m_Sedescription = DB.RSField(dr, "SEDescription");
                    m_Published = DB.RSFieldBool(dr, "Published");
                    m_Wholesale = DB.RSFieldBool(dr, "Wholesale");
                    m_Deleted = DB.RSFieldBool(dr, "Deleted");
                    m_Createdon = DB.RSFieldDateTime(dr, "CreatedOn");
                    m_Saltkey = DB.RSFieldInt(dr, "SaltKey");
                    StoreID = DB.RSFieldInt(dr, "StoreID");
                }
                dr.Close();
            }
            catch { }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();
        }

        private void LoadFromDB(string Email)
        {
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_getAffiliateByEmail";

            cmd.Parameters.Add(new SqlParameter("@AffiliateEmail", SqlDbType.NVarChar, 100));
            cmd.Parameters["@AffiliateEmail"].Value = Email;


            try
            {
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    m_Affiliateid = DB.RSFieldInt(dr, "AffiliateID");
                    m_Affiliateguid = DB.RSFieldGUID2(dr, "AffiliateGUID");
                    m_Email = DB.RSField(dr, "EMail");
                    m_Password = DB.RSField(dr, "Password");
                    m_Dateofbirth = DB.RSFieldDateTime(dr, "DateOfBirth");
                    m_Gender = DB.RSField(dr, "Gender");
                    m_Notes = DB.RSField(dr, "Notes");
                    m_Isonline = DB.RSFieldBool(dr, "IsOnline");
                    m_Firstname = DB.RSField(dr, "FirstName");
                    m_Lastname = DB.RSField(dr, "LastName");
                    m_Name = DB.RSField(dr, "Name");
                    m_Company = DB.RSField(dr, "Company");
                    m_Address1 = DB.RSField(dr, "Address1");
                    m_Address2 = DB.RSField(dr, "Address2");
                    m_Suite = DB.RSField(dr, "Suite");
                    m_City = DB.RSField(dr, "City");
                    m_State = DB.RSField(dr, "State");
                    m_Zip = DB.RSField(dr, "Zip");
                    m_Country = DB.RSField(dr, "Country");
                    m_Phone = DB.RSField(dr, "Phone");
                    m_Websitename = DB.RSField(dr, "WebSiteName");
                    m_Websitedescription = DB.RSField(dr, "WebSiteDescription");
                    m_Url = DB.RSField(dr, "URL");
                    m_Trackingonly = DB.RSFieldBool(dr, "TrackingOnly");
                    m_Defaultskinid = DB.RSFieldInt(dr, "DefaultSkinID");
                    m_Parentaffiliateid = DB.RSFieldInt(dr, "ParentAffiliateID");
                    m_Displayorder = DB.RSFieldInt(dr, "DisplayOrder");
                    m_Extensiondata = DB.RSField(dr, "ExtensionData");
                    m_Sename = DB.RSField(dr, "SEName");
                    m_Setitle = DB.RSField(dr, "SETitle");
                    m_Senoscript = DB.RSField(dr, "SENoScript");
                    m_Sealttext = DB.RSField(dr, "SEAltText");
                    m_Sekeywords = DB.RSField(dr, "SEKeywords");
                    m_Sedescription = DB.RSField(dr, "SEDescription");
                    m_Published = DB.RSFieldBool(dr, "Published");
                    m_Wholesale = DB.RSFieldBool(dr, "Wholesale");
                    m_Deleted = DB.RSFieldBool(dr, "Deleted");
                    m_Createdon = DB.RSFieldDateTime(dr, "CreatedOn");
                    m_Saltkey = DB.RSFieldInt(dr, "SaltKey");
                    StoreID = DB.RSFieldInt(dr, "StoreID");
                }
                dr.Close();
            }
            catch { }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();
        }

        #endregion

        #region Public Properties
        public int AffiliateID
        {
            get { return m_Affiliateid; }
        }


        public Guid AffiliateGUID
        {
            get { return m_Affiliateguid; }
        }


        public string EMail
        {
            get { return m_Email; }
        }


        public string Password
        {
            get { return m_Password; }
        }


        public DateTime DateOfBirth
        {
            get { return m_Dateofbirth; }
        }


        public string Gender
        {
            get { return m_Gender; }
        }


        public string Notes
        {
            get { return m_Notes; }
        }


        public bool IsOnline
        {
            get { return m_Isonline; }
        }


        public string FirstName
        {
            get { return m_Firstname; }
        }


        public string LastName
        {
            get { return m_Lastname; }
        }


        public string Name
        {
            get { return m_Name; }
        }


        public string Company
        {
            get { return m_Company; }
        }


        public string Address1
        {
            get { return m_Address1; }
        }


        public string Address2
        {
            get { return m_Address2; }
        }


        public string Suite
        {
            get { return m_Suite; }
        }


        public string City
        {
            get { return m_City; }
        }


        public string State
        {
            get { return m_State; }
        }


        public string Zip
        {
            get { return m_Zip; }
        }


        public string Country
        {
            get { return m_Country; }
        }


        public string Phone
        {
            get { return m_Phone; }
        }


        public string WebSiteName
        {
            get { return m_Websitename; }
        }


        public string WebSiteDescription
        {
            get { return m_Websitedescription; }
        }


        public string URL
        {
            get { return m_Url; }
        }


        public bool TrackingOnly
        {
            get { return m_Trackingonly; }
        }


        public int DefaultSkinID
        {
            get { return m_Defaultskinid; }
        }


        public int ParentAffiliateID
        {
            get { return m_Parentaffiliateid; }
        }


        public int DisplayOrder
        {
            get { return m_Displayorder; }
        }


        public string ExtensionData
        {
            get { return m_Extensiondata; }
        }


        public string SEName
        {
            get { return m_Sename; }
        }


        public string SETitle
        {
            get { return m_Setitle; }
        }


        public string SENoScript
        {
            get { return m_Senoscript; }
        }


        public string SEAltText
        {
            get { return m_Sealttext; }
        }


        public string SEKeywords
        {
            get { return m_Sekeywords; }
        }


        public string SEDescription
        {
            get { return m_Sedescription; }
        }


        public bool Published
        {
            get { return m_Published; }
        }


        public bool Wholesale
        {
            get { return m_Wholesale; }
        }


        public bool Deleted
        {
            get { return m_Deleted; }
        }


        public DateTime CreatedOn
        {
            get { return m_Createdon; }
        }


        public int SaltKey
        {
            get { return m_Saltkey; }
        }

        private int m_storeid;

        public int StoreID
        {
            get { return m_storeid; }
            set { m_storeid = value; }
        }

        #endregion
    }
}
