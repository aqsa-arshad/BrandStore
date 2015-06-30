// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using AspDotNetStorefrontEncrypt;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for Security: localization of password storage and comparision functions.
    /// </summary>
    public class Security
    {
        public struct SecurityParams
        {
            public String EncryptKey;
            public int KeySize;
            public int EncryptIterations;
            public String InitializationVector;
            public String HashAlgorithm;
            public SecurityParams(String p_EncryptKey, int p_KeySize, int p_EncryptIterations, String p_InitializationVector, String p_HashAlgorithm)
            {
                EncryptKey = p_EncryptKey;
                KeySize = p_KeySize;
                EncryptIterations = p_EncryptIterations;
                InitializationVector = p_InitializationVector;
                HashAlgorithm = p_HashAlgorithm;
            }
        }

        public static readonly int ro_SaltKeyIsInvalid = 0;
        public static readonly int ro_PasswordIsInClearText = -1;
        public static readonly int ro_PasswordIsEncrypted = -2;
        public static readonly String ro_PasswordDefaultTextForAnon = String.Empty;
        public static readonly String ro_DecryptFailedPrefix = "Error.";

        public enum CryptTypeEnum
        {
            V1 = 0,
            V2 = 1
        }

        public Security()
        {
        }

        /// <summary>
        /// Generates a random key of the specified length
        /// </summary>
        /// <param name="length">length of the key (in bytes) to generate</param>
        /// <returns></returns>
        public static string CreateRandomKey(int length)
        {
            //We'll convert this to hex, so the length must be double
            StringBuilder key = new StringBuilder(length * 2);
            byte[] ba = new byte[length];

            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();

            rand.GetBytes(ba);

            foreach (byte b in ba)
            {
                key.Append(String.Format("{0:X2}", b));
            }

            return key.ToString();
        }


        /// <summary>
        /// Creates a random key with a random length (in bytes)
        /// Since this is converted to Hexidecimal, the key length (in characters) will be double the specified values
        /// </summary>
        /// <param name="minLength">Minimum length of the key to generate.</param>
        /// <param name="maxLength">Maximum length of the key to generate.</param>
        /// <returns></returns>
        public static string CreateRandomKey(int minLength, int maxLength)
        {
            int minimum = minLength;
            int maximum = maxLength;

            if (minimum > maximum)
            {
                //Someone goofed up.  Swap the numbers.
                minimum = maxLength;
                maximum = minLength;
            }

            return CreateRandomKey(new Random().Next(minimum, maximum));
        }


        public static string ConvertToHex(string input)
        {
            StringBuilder converted = new StringBuilder(input.Length * 2);
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] ba = enc.GetBytes(input);

            foreach (byte b in ba)
            {
                converted.Append(String.Format("{0:X2}", b));
            }
            
            return converted.ToString();
        }

        public static void AgeSecurityLog()
        {
            DB.ExecuteSQL("delete from SecurityLog where ActionDate < dateadd(year,-1,getdate())");
        }

        public static String GetMD5Hash(String s)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            Byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(s));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static String GetEncryptParam(String ParamName)
        {
            String param = string.Empty;
            if (ParamName == "EncryptKey")
            {
                param = CommonLogic.Application(ParamName);
            }
            else
            {
                param = AppLogic.AppConfig(ParamName);
            }

            // now do validation!
            if (ParamName == "EncryptKey")
            {
                if (param.Length == 0 || param == "WIZARD" ||
                    param == AppLogic.ro_TBD)
                {
                    throw new ArgumentException("You must enter your EncryptKey in the /web.config file!!! Open that file in Notepad, and see the instructions.");
                }
            }

            if (ParamName == "EncryptIterations")
            {
                if (param.Length == 0 && !CommonLogic.IsInteger(param) && Convert.ToInt32(param) >= 1 &&
                    Convert.ToInt32(param) <= 4)
                {
                    throw new ArgumentException("The EncryptIterations parameter must be an integer value between 1 and 4.");
                }
            }

            if (ParamName == "InitializationVector")
            {
                if (param.Length == 0 || param == AppLogic.ro_TBD ||
                    param.Length != 16)
                {
                    throw new ArgumentException("You MUST set your InitializationVector in the AppConfig manager in the admin site! it MUST be exactly 16 characters/digits long. This is required for security reasons.");
                }
            }

            if (ParamName == "KeySize")
            {
                if (param.Length == 0 || param == "0" ||
                    (param != "128" && param != "192" && param != "256"))
                {
                    throw new ArgumentException("You MUST set your KeySize value in the AppConfig manager in the admin site to an allowed valid value! This is required for security reasons.");
                }
            }

            if (ParamName == "HashAlgorithm")
            {
                if (param.Length == 0 ||
                    (param != "MD5" && param != "SHA1"))
                {
                    throw new ArgumentException("You MUST set your HashAlgorithm in the AppConfig manager in the admin site to an allowed valid value! This is required for security reasons.");
                }
            }

            return param;
        }

        public static void SaveConfig(String origFN, String paramVal)
        {
            bool isReadOnly = false;
            XmlDocument xdoc = new XmlDocument();
            FileAttributes attribs = File.GetAttributes( origFN );
            XmlNode EncryptKeyNode;
            xdoc.Load(origFN);
            EncryptKeyNode = xdoc.SelectSingleNode("configuration/appSettings/add[@key='EncryptKey']");
            EncryptKeyNode.Attributes["value"].Value = paramVal;
            if ((File.GetAttributes(origFN) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                attribs &= ~FileAttributes.ReadOnly;
                File.SetAttributes(origFN, attribs);
                isReadOnly = true;
            }
            xdoc.Save(origFN);
            if( isReadOnly )
            {
                attribs |= FileAttributes.ReadOnly;
                File.SetAttributes( origFN, attribs );
            }
        }

        public static void SetEncryptParam(String ParamName, String ParamValue)
        {
            if (ParamName == "EncryptKey")
            {
                // do not use "~", seems to be a read-only accessor. Have to use the ApplicationPath to get a read-write copy.
                // Also requires that the user for the worker process (NETWORK USER, ASPNET etc..) be granted read/write to the web.config file
                System.Configuration.Configuration config =
                    WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
                AppSettingsSection appsettings = (AppSettingsSection) config.GetSection( "appSettings" );
                appsettings.Settings[ ParamName ].Value = ParamValue;
                appsettings.SectionInformation.ForceSave = true;
                config.Save( ConfigurationSaveMode.Full );

            }
            else
            {
                try
                {
                    SqlParameter sp1 = new SqlParameter("@ConfigValue", SqlDbType.NVarChar);
                    sp1.Value = ParamValue;
                    SqlParameter[] spa = {sp1};
                    AppConfig config = AppLogic.GetAppConfig(ParamName);
                    if (config != null)
                    {
                        config.Update(spa);
                    }
                }
                catch
                {
                    throw new ArgumentException("You do not have a required Security AppConfig [" + ParamName + "] parameter in your database. Please run the latest database upgrade script, and restart your website!!");
                }
            }
        }


        public static SecurityParams GetSecurityParams()
        {
            SecurityParams p;
            p.EncryptKey = GetEncryptParam("EncryptKey");
            p.HashAlgorithm = GetEncryptParam("HashAlgorithm");
            p.InitializationVector = GetEncryptParam("InitializationVector");
            p.KeySize = Int32.Parse(GetEncryptParam("KeySize"));
            p.EncryptIterations = Int32.Parse(GetEncryptParam("EncryptIterations"));
            if (p.EncryptIterations == 0)
            {
                p.EncryptIterations = 1;
            }
            return p;
        }


        public static void UpgradeEncryption()
        {
            // address table info:
            String SaltField = AppLogic.AppConfig("AddressCCSaltField");
            if (SaltField.Length == 0)
            {
                throw new ArgumentException("You MUST set AppConfig:AddressCCSaltField to a valid defined value. Please see the description of that AppConfig! This is required for security reasons.");
            }

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select " + SaltField + " ,CardNumber from Address where Crypt=" + ((int)CryptTypeEnum.V1).ToString() + " and CardNumber IS NOT NULL and convert(nvarchar(100),CardNumber)<>''", con))
                {
                    while (rs.Read())
                    {
                        String CN = DB.RSField(rs, "CardNumber").Trim();
                        if (CN.Length != 0 &&
                            CN != AppLogic.ro_CCNotStoredString)
                        {
                            CN = UnmungeStringOld(CN);
                            if (CN.StartsWith(ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                CN = DB.RSField(rs, "CardNumber");
                            }
                            if (CN.Trim().Length != 0)
                            {
                                DB.ExecuteSQL("update Address set Crypt=" + ((int)CryptTypeEnum.V2).ToString() + ", CardNumber=" + DB.SQuote(MungeString(CN, rs[SaltField].ToString())) + " where AddressID=" + DB.RSFieldInt(rs, "AddressID").ToString());
                            }
                        }
                    }
                }
            }

            DB.ExecuteSQL("update Address set Crypt=" + ((int)CryptTypeEnum.V2).ToString() + " where Crypt < " + ((int)CryptTypeEnum.V2).ToString());

            // orders table info:
            SaltField = AppLogic.AppConfig("OrdersCCSaltField");
            if (SaltField.Length == 0)
            {
                throw new ArgumentException("You MUST set AppConfig:OrdersCCSaltField to a valid defined value. Please see the description of that AppConfig! This is required for security reasons.");
            }

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select OrderNumber," + SaltField + " as SaltField,CardNumber from Orders where Crypt=" + ((int)CryptTypeEnum.V1).ToString() + " and CardNumber IS NOT NULL and convert(nvarchar(100),CardNumber)<>''", con))
                {
                    while (rs.Read())
                    {
                        String CN = DB.RSField(rs, "CardNumber").Trim();
                        if (CN.Length != 0 &&
                            CN != AppLogic.ro_CCNotStoredString)
                        {
                            CN = UnmungeStringOld(CN);
                            if (CN.StartsWith(ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                CN = DB.RSField(rs, "CardNumber");
                            }
                            if (CN.Trim().Length != 0)
                            {
                                DB.ExecuteSQL("update Orders set Crypt=" + ((int)CryptTypeEnum.V2).ToString() + ", CardNumber=" + DB.SQuote(MungeString(CN, rs["SaltField"].ToString())) + " where OrderNumber=" + DB.RSFieldInt(rs, "OrderNumber").ToString());
                            }
                        }
                    }
                }
            }

            DB.ExecuteSQL("update Orders set Crypt=" + ((int)CryptTypeEnum.V2).ToString() + " where Crypt < " + ((int)CryptTypeEnum.V2).ToString());
        }

        /// <summary>
        /// Provides a method for scrubbing string values for anything that appears to look like a card number.
        /// The string that is returned will replace any matches with "OMMITTED"
        /// </summary>
        /// <param name="s">The string to scrub</param>
        /// <returns></returns>
        public static string ScrubCCNumbers(String unclean)
        {
            String clean = String.Empty;
            String matchPattern = @"\b(?:\d[ -]*[A-z]*\W*){13,16}\b";
            Regex rgx = new Regex(matchPattern, RegexOptions.IgnoreCase);

            clean = rgx.Replace(unclean, "OMMITTED", unclean.Length);

            // Clean up any numbers that may remain in memory
            unclean = MungeString(unclean);
            unclean = String.Empty;


            return clean;
        }

        public static String MungeString(String s)
        {
            return MungeString(s, String.Empty, GetSecurityParams());
        }

        public static String MungeString(String s, SecurityParams p)
        {
            return MungeString(s, String.Empty, p);
        }

        public static String MungeString(String s, String SaltKey)
        {
            return MungeString(s, SaltKey, GetSecurityParams());
        }

        public static String MungeString(String s, String SaltKey, SecurityParams p)
        {
            if (s.Length == 0)
            {
                return s;
            }
            Encrypt e = new Encrypt(p.EncryptKey, p.InitializationVector, 4, 4, p.KeySize, p.HashAlgorithm, SaltKey, p.EncryptIterations);
            String tmpS = e.EncryptData(s);
            return tmpS;
        }

        public static String UnmungeString(String s)
        {
            return UnmungeString(s, String.Empty, GetSecurityParams());
        }

        public static String UnmungeString(String s, SecurityParams p)
        {
            return UnmungeString(s, String.Empty, p);
        }

        public static String UnmungeString(String s, String SaltKey)
        {
            return UnmungeString(s, SaltKey, GetSecurityParams());
        }

        public static String UnmungeString(String s, String SaltKey, SecurityParams p)
        {
            if (s.Length == 0)
            {
                return s;
            }
            try
            {
                Encrypt e = new Encrypt(p.EncryptKey, p.InitializationVector, 4, 4, p.KeySize, p.HashAlgorithm, SaltKey, p.EncryptIterations);
                String tmpS = e.DecryptData(s);
                return tmpS;
            }
            catch
            {
                //return "Error: Decrypt Failed";
                // to make sure when comparing the StartsWith
                return ro_DecryptFailedPrefix + " Decrypt Failed";
            }
        }

        public static void ConvertAllPasswords()
        {
            // customer table:     
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select CustomerID,Password from Customer  with (NOLOCK)  where SaltKey in (-1,-2)", con))
                {
                    while (rs.Read())
                    {
                        if (DB.RSField(rs, "Password").Length != 0 ||
                            DB.RSField(rs, "Password") != ro_PasswordDefaultTextForAnon)
                        {
                            String PWD = UnmungeStringOld(DB.RSField(rs, "Password"));
                            if (PWD.StartsWith(ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // must have been in clear text:
                                PWD = DB.RSField(rs, "Password");
                            }
                            int Salt = Encrypt.CreateRandomSalt();
                            Password p = new Password(PWD, Salt);
                            DB.ExecuteSQL("update Customer set Password=" + DB.SQuote(p.SaltedPassword) + ", SaltKey=" + Salt.ToString() + " where CustomerID=" + DB.RSFieldInt(rs, "CustomerID").ToString());
                        }
                    }
                }
            }

            // Affiliate Table:
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select AffiliateID,Password,SaltKey from Affiliate  with (NOLOCK)  where SaltKey in (-1,-2)", con))
                {
                    while (rs.Read())
                    {
                        if (DB.RSField(rs, "Password").Length != 0 ||
                            DB.RSField(rs, "Password") != ro_PasswordDefaultTextForAnon)
                        {
                            String PWD = UnmungeStringOld(DB.RSField(rs, "Password"));
                            if (PWD.StartsWith(ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                PWD = DB.RSField(rs, "Password");
                            }
                            int Salt = Encrypt.CreateRandomSalt();
                            Password p = new Password(PWD, Salt); // PWD in this call is still in clear text really
                            DB.ExecuteSQL("update Affiliate set Password=" + DB.SQuote(p.SaltedPassword) + ", SaltKey=" + Salt.ToString() + " where AffiliateID=" + DB.RSFieldInt(rs, "AffiliateID").ToString());
                        }
                    }
                }
            }
        }

        // properly handles normal and admin customers!
        public static String SetCustomerToNewRandomPassword(Customer c)
        {
            Password pwd = new RandomPassword();
            if (c.IsAdminUser)
            {
                pwd = new RandomStrongPassword();
            }
            c.UpdateCustomer(
                /*CustomerLevelID*/ null,
                                    /*EMail*/ null,
                                    /*SaltedAndHashedPassword*/ pwd.SaltedPassword,
                                    /*SaltKey*/ pwd.Salt,
                                    /*DateOfBirth*/ null,
                                    /*Gender*/ null,
                                    /*FirstName*/ null,
                                    /*LastName*/ null,
                                    /*Notes*/ null,
                                    /*SkinID*/ null,
                                    /*Phone*/ null,
                                    /*AffiliateID*/ null,
                                    /*Referrer*/ null,
                                    /*CouponCode*/ null,
                                    /*OkToEmail*/ null,
                                    /*IsAdmin*/ null,
                                    /*BillingEqualsShipping*/ null,
                                    /*LastIPAddress*/ null,
                                    /*OrderNotes*/ null,
                                    /*SubscriptionExpiresOn*/ null,
                                    /*RTShipRequest*/ null,
                                    /*RTShipResponse*/ null,
                                    /*OrderOptions*/ null,
                                    /*LocaleSetting*/ null,
                                    /*MicroPayBalance*/ null,
                                    /*RecurringShippingMethodID*/ null,
                                    /*RecurringShippingMethod*/ null,
                                    /*BillingAddressID*/ null,
                                    /*ShippingAddressID*/ null,
                                    /*GiftRegistryGUID*/ null,
                                    /*GiftRegistryIsAnonymous*/ null,
                                    /*GiftRegistryAllowSearchByOthers*/ null,
                                    /*GiftRegistryNickName*/ null,
                                    /*GiftRegistryHideShippingAddresses*/ null,
                                    /*CODCompanyCheckAllowed*/ null,
                                    /*CODNet30Allowed*/ null,
                                    /*ExtensionData*/ null,
                                    /*FinalizationData*/ null,
                                    /*Deleted*/ null,
                                    /*Over13Checked*/ null,
                                    /*CurrencySetting*/ null,
                                    /*VATSetting*/ null,
                                    /*VATRegistrationID*/ null,
                                    /*StoreCCInDB*/ null,
                                    /*IsRegistered*/ null,
                                    /*LockedUntil*/ null,
                                    /*AdminCanViewCC*/ null,
                                    /*BadLogin*/ null,
                                    /*Active*/ null,
                                    /*PwdChangeRequired*/ 1,
                                    /*RegisterDate*/ null,
                                    /*StoreId*/null
                );
            return pwd.SaltedPassword;
        }

        /// <summary>
        /// Overload for backwards compatibility.  If no security params are passed, the default values will be used.
        /// </summary>
        /// <param name="SecurityAction">Brief description of the security event</param>
        /// <param name="Description">Full description of the security event</param>
        /// <param name="CustomerUpdated">The customer ID which the event targeted (eg. password change event)</param>
        /// <param name="UpdatedBy">The ID of the initiating customer or administrator</param>
        /// <param name="CustomerSessionID">The session ID of the initiating customer or administrator</param>
        /// <returns></returns>
        public static String LogEvent(String SecurityAction, String Description, int CustomerUpdated, int UpdatedBy, int CustomerSessionID)
        {
            string err = String.Empty;

            SecurityParams spa = GetSecurityParams();

            LogEvent(SecurityAction, Description, CustomerUpdated, UpdatedBy, CustomerSessionID, spa);

            return err;
        }

        /// <summary>
        /// Writes an event to the encrypted system security log
        /// This method is used for PA-DSS compliance
        /// </summary>
        /// <param name="SecurityAction">Brief description of the security event</param>
        /// <param name="Description">Full description of the security event</param>
        /// <param name="CustomerUpdated">The customer ID which the event targeted (eg. password change event)</param>
        /// <param name="UpdatedBy">The ID of the initiating customer or administrator</param>
        /// <param name="CustomerSessionID">The session ID of the initiating customer or administrator</param>
        /// <param name="SecurityParameters">Security parameter object allowing the developer to control how strings are encrypted</param>
        /// <returns></returns>
        public static String LogEvent(String SecurityAction, String Description, int CustomerUpdated, int UpdatedBy, int CustomerSessionID, SecurityParams SecurityParameters)
        {
            String err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_SecurityLogInsert";

            cmd.Parameters.Add(new SqlParameter("@SecurityAction", SqlDbType.NVarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@CustomerUpdated", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@CustomerSessionID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@logid", SqlDbType.BigInt, 8)).Direction = ParameterDirection.Output;

            cmd.Parameters["@SecurityAction"].Value = MungeString(SecurityAction, SecurityParameters);
            cmd.Parameters["@Description"].Value = MungeString(Description, SecurityParameters);
            cmd.Parameters["@CustomerUpdated"].Value = CustomerUpdated;
            cmd.Parameters["@UpdatedBy"].Value = UpdatedBy;
            cmd.Parameters["@CustomerSessionID"].Value = CustomerSessionID;

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

        // helper routine just for compatibility with WSI.cs class from v7.1 in the future:
        public static String HtmlEncode(String s)
        {
            return HttpContext.Current.Server.HtmlEncode(s);
        }

        public static String UrlEncode(String s)
        {
            return HttpContext.Current.Server.UrlEncode(s);
        }

        #region OLDROUTINES

        // ---------------------------------------------------------------------------------------------------
        // OLD routines now below
        // do not use anymore
        // routines above offer increased security characteristics per PABP
        // ---------------------------------------------------------------------------------------------------

        public static String MungeStringOld(String s)
        {
            SecurityParams p = GetSecurityParams();
            String tmpS = new EncryptOld().EncryptData(p.EncryptKey, s); // we removed s.ToLowerInvariant() from this call in v5.8!! 
            return tmpS;
        }

        public static String UnmungeStringOld(String s)
        {
            SecurityParams p = GetSecurityParams();
            String tmpS = new EncryptOld().DecryptData(p.EncryptKey, s);
            return tmpS;
        }

        #endregion
    }

    public class Password
    {
        private String m_ClearPassword = String.Empty;
        private int m_Salt = 0;
        private String m_SaltedPassword = String.Empty;

        public static readonly int ro_RandomPasswordLength = 8;
        public static readonly int ro_RandomStrongPasswordLength = 8;

        public Password(String ClearPassword, int Salt)
        {
            m_ClearPassword = ClearPassword;
            m_Salt = Salt;
            m_SaltedPassword = Encrypt.ComputeSaltedHash(m_Salt, m_ClearPassword);
        }


        public Password(String ClearPassword)
        {
            m_ClearPassword = ClearPassword;
            m_Salt = Encrypt.CreateRandomSalt();
            m_SaltedPassword = Encrypt.ComputeSaltedHash(m_Salt, m_ClearPassword);
        }


        public Password()
        {
            m_ClearPassword = Encrypt.CreateRandomStrongPassword(8);
            m_Salt = Encrypt.CreateRandomSalt();
            m_SaltedPassword = Encrypt.ComputeSaltedHash(m_Salt, m_ClearPassword);
        }


        public String ClearPassword
        {
            get { return m_ClearPassword; }
        }

        public String SaltedPassword
        {
            get { return m_SaltedPassword; }
        }

        public int Salt
        {
            get { return m_Salt; }
        }
    }

    // creates a random password with random salt, and hashes it!
    // this is NOT guaranteed to be a strong password!!
    // if you need a Strong Password use RandomStrongPassword class!!
    public class RandomPassword : Password
    {
        public RandomPassword() : base(Encrypt.CreateRandomPassword(ro_RandomPasswordLength, CommonLogic.IIF(AppLogic.AppConfig("NewPwdAllowedChars").Length == 0, @"abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ23456789~!@#$%&*()_-={}[]\\|;:\,./?", AppLogic.AppConfig("NewPwdAllowedChars"))), Encrypt.CreateRandomSalt())
        {
        }
    }

    public class RandomStrongPassword : Password
    {
        public RandomStrongPassword() : base(Encrypt.CreateRandomStrongPassword(ro_RandomStrongPasswordLength), Encrypt.CreateRandomSalt())
        {
        }
    }
}
