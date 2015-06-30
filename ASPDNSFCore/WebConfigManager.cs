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
using System.Security.Cryptography;
using System.Web;
using System.Web.Configuration;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{


    /// <summary>
    /// Allows read/write access to common web.config settings
    /// For an explanation of possible encryption/hashing methods
    /// for the viewstate and forms auth tokens, see:
    /// http://msdn.microsoft.com/en-us/library/ms998288.aspx
    /// </summary>
    public class WebConfigManager
    {
        #region Enums

        /// <summary>
        /// Specifies the length (in bytes) of the key to generate
        /// ASP.NET will not allow an enum member to start with an integer
        /// Recommended settings (according to Microsoft) are:
        /// AES ValidationKey - 64 Byte
        /// AES DecryptionKey - 32 Byte
        /// SHA1 ValidationKey - 64 Byte
        /// 3DES - 48 Byte
        /// MD5  - 32 Byte
        /// Random - A key of random length will be generated
        /// </summary>
        public enum KeyLength : int
        {
            Byte64 = 64,
            Byte48 = 48,
            Byte40 = 40,
            Byte32 = 32,
            Byte24 = 24,
            Byte16 = 16,
            Random = 0
        }

        /// <summary>
        /// Specifies the method used to create a given key
        /// Auto-generation (RECOMMENDED!) will utilize the 
        /// RNGCryptoServiceProvider to generate a cryptographically
        /// strong set of random numbers which are then converted
        /// to Hex format.  If set to manual you MUST specify
        /// a valid key
        /// </summary>
        public enum KeyGenerationMethod : int
        {
            Auto = 1,
            Manual = 2
        }

        /// <summary>
        /// Specifies the method used to protect the web.config file
        /// AppSettings section if the web.config file is set to be
        /// encrypted
        /// </summary>
        public enum WebConfigEncryptionProvider : int
        {
            DataProtectionConfigurationProvider = 1,
            RsaProtectedConfigurationProvider = 2
        }

        #endregion


        #region Private variables

        private bool m_setEncryptKey = false;
        private bool m_setMachineKey = false;
        private bool m_setDBConn = false;
        private bool m_protectWebConfig = false;
        private string m_encryptKey = String.Empty;
        private string m_decryptKey = String.Empty;
        private string m_validationKey = String.Empty;
        private string m_oldEncryptKey = String.Empty;
        private Customer m_thisCustomer = null;
        private KeyLength m_encryptKeyLength = KeyLength.Random;
        private KeyLength m_validationKeyLength = KeyLength.Byte64;
        private KeyLength m_decryptionKeyLength = KeyLength.Byte32;
        private KeyGenerationMethod m_encryptKeyGenMethod = KeyGenerationMethod.Auto;
        private KeyGenerationMethod m_decryptKeyGenMethod = KeyGenerationMethod.Auto;
        private KeyGenerationMethod m_validationKeyGenMethod = KeyGenerationMethod.Auto;
        private MachineKeyValidation m_validationKeyType = MachineKeyValidation.SHA1;
        private MachineKeyCompatibilityMode m_compatibilityMode = MachineKeyCompatibilityMode.Framework20SP2;
        private WebConfigEncryptionProvider m_webConfigEncryptProvider;

        #endregion


        #region Constructors

        /// <summary>
        /// Instantiates an instance of the WebConfigManager class
        /// Overload that creates a customer object using the AspDotNetStorefrontPrinciple Object
        /// </summary>
        public WebConfigManager()
        {
            //Try to get the customer object from the HTTPContext if it wasn't passed into the constructor
            try
            {
                m_thisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
            }
            catch { }

            Initialize();
        }

        /// <summary>
        /// Instantiates an instance of the WebConfigManager class
        /// The customer object is required for security logging purposes
        /// </summary>
        public WebConfigManager(Customer thisCustomer)
        {
            m_thisCustomer = thisCustomer;

            Initialize();
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Sets default values for certain private member variables 
        /// </summary>
        private void Initialize()
        {
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~/");
                AppSettingsSection appSection = (AppSettingsSection)config.GetSection("appSettings");

                //We set the default protection setting here based on the current state of the web.config appSettings section
                //After instantiation, the developer can override this by setting the ProtectWebConfig property on this object
                m_protectWebConfig = appSection.SectionInformation.IsProtected;
            }
            catch
            {
                //If it fails just trying to read the file, there is nothing else we can do at all in this class
                throw new NotSupportedException("Your current hosting environment does not support this functionality.  Interacting with the web.config file requires the site run in full (unrestricted) trust.  The current trust level for your site is: " + AppLogic.TrustLevel.ToString());
            }


            //Try to get a valid web.config protection provider from AppConfig settings.  Use default provider if that fails
            try
            {
                m_webConfigEncryptProvider = (WebConfigEncryptionProvider)Enum.Parse(typeof(WebConfigEncryptionProvider), AppLogic.AppConfig("Web.Config.EncryptionProvider"), true);
            }
            catch
            {
                m_webConfigEncryptProvider = WebConfigEncryptionProvider.DataProtectionConfigurationProvider;
            }
        }


        /// <summary>
        /// Writes a PA-DSS-protected action to the system security log
        /// </summary>
        /// <param name="eventText">A short description of the event to log</param>
        /// <param name="eventDesc">A detailed description of the event to log</param>
        /// <param name="securityParameters">Security parameters used for encrypting the log</param>
        private void LogSecurityEvent(String eventText, String eventDesc, Security.SecurityParams securityParameters)
        {
            if (m_thisCustomer == null)
            {
                Security.LogEvent(eventText, eventDesc, 0, 0, 0, securityParameters);
            }
            else
            {
                Security.LogEvent(eventText, eventDesc, m_thisCustomer.CustomerID, m_thisCustomer.CustomerID, m_thisCustomer.CurrentSessionID, securityParameters);
            }
        }


        #endregion


        #region Public methods

        /// <summary>
        /// Commits specified properties (specified in the constructor) to the web.config file.
        /// WARNING - Causes an application restart!
        /// </summary>
        public List<Exception> Commit()
        {
            List<Exception> exceptions = new List<Exception>();

            Security.SecurityParams spa = Security.GetSecurityParams();

            //Wrap this all, as there inumerable things that could cause this to fail (permissions issues, medium trust, etc.)
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~/");

                AppSettingsSection appSection = (AppSettingsSection)config.GetSection("appSettings");
                appSection.SectionInformation.ForceSave = true;

                LogSecurityEvent("Web.config file opened for editing", "The web.config file has been opened for programmatic editing by the AspDotNetStorefront application.  This could be due to changing or setting an encryption key or machine key.  Additional information will be recorded in the security log.", spa);

                //We need to unprotect the web.config.  Otherwise we cannot write to it.
                if (appSection.SectionInformation.IsProtected)
                {
                    appSection.SectionInformation.UnprotectSection();
                    LogSecurityEvent("Web.config file decrypted", "The web.config appSettings section has been automatically decrypted to support editing.  It will be re-encrypted after editing if ProtectWebConfig is true.", spa);

                }

                //DO NOT SAVE UNTIL ALL ROUTINES ARE COMPLETE, AS COMMITTING WILL RESTART THE APPLICATION!

                #region Modify encryptKey
                try
                {
                    if (m_setEncryptKey)
                    {
                        string AddressSaltField = AppLogic.AppConfig("AddressCCSaltField");
                        string OrdersSaltField = AppLogic.AppConfig("OrdersCCSaltField");
                        string oldEncryptKey = spa.EncryptKey;

                        if (spa.EncryptIterations == 0)
                        {
                            spa.EncryptIterations = 1;
                        }

                        if (m_encryptKeyGenMethod == KeyGenerationMethod.Auto)
                        {
                            if (m_encryptKeyLength == KeyLength.Random)
                            {
                                m_encryptKey = Security.CreateRandomKey(4, 25);
                            }
                            else
                            {
                                Security.CreateRandomKey((int)m_encryptKeyLength);
                            }
                        }

                        spa.EncryptKey = m_encryptKey;

                        DBTransaction t = new DBTransaction();

                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();

                            string query = "SELECT AddressID,CustomerID,CardNumber, eCheckBankABACode, eCheckBankAccountNumber FROM Address WHERE " +
                                            " CardNumber IS NOT NULL AND CONVERT(NVARCHAR(4000),CardNumber)<>{0} AND CONVERT(NVARCHAR(4000),CardNumber)<>{1} OR ( " +
                                            " eCheckBankABACode IS NOT NULL AND CONVERT(NVARCHAR(4000),eCheckBankABACode)<>{0} AND CONVERT(NVARCHAR(4000),eCheckBankABACode)<>{1} AND " +
                                            " eCheckBankAccountNumber IS NOT NULL AND CONVERT(NVARCHAR(4000),eCheckBankAccountNumber)<>{0} AND CONVERT(NVARCHAR(4000),eCheckBankAccountNumber)<>{1}) " +
                                            " ORDER BY AddressID ";

                            query = string.Format(query, DB.SQuote(AppLogic.ro_CCNotStoredString), DB.SQuote(String.Empty));

                            using (IDataReader rsAddress = DB.GetRS(query, dbconn))
                            {
                                using (DataTable dtAddress = new DataTable())
                                {
                                    dtAddress.Load(rsAddress);

                                    foreach (DataRow row in dtAddress.Rows)
                                    {
                                        String CN = DB.RowField(row, "CardNumber");
                                        if (CN.Length != 0 &&
                                            CN != AppLogic.ro_CCNotStoredString)
                                        {
                                            String CNDecrypted = Security.UnmungeString(CN, row[AddressSaltField].ToString());
                                            if (CNDecrypted.StartsWith(Security.ro_DecryptFailedPrefix))
                                            {
                                                CNDecrypted = DB.RowField(row, "CardNumber");
                                            }
                                            row["CardNumber"] = CNDecrypted;
                                        }

                                        // eCheck Details...
                                        String abaCode = DB.RowField(row, "eCheckBankABACode");
                                        if (abaCode.Length != 0 &&
                                            abaCode != AppLogic.ro_CCNotStoredString)
                                        {
                                            String abaCodeDecrypted = Security.UnmungeString(abaCode, row[AddressSaltField].ToString());
                                            if (abaCodeDecrypted.StartsWith(Security.ro_DecryptFailedPrefix))
                                            {
                                                abaCodeDecrypted = DB.RowField(row, "eCheckBankABACode");
                                            }
                                            row["eCheckBankABACode"] = abaCodeDecrypted;
                                        }

                                        String bankAccountNumber = DB.RowField(row, "eCheckBankAccountNumber");
                                        if (bankAccountNumber.Length != 0 &&
                                            bankAccountNumber != AppLogic.ro_CCNotStoredString)
                                        {
                                            String bankAccountNumberDecrypted = Security.UnmungeString(bankAccountNumber, row[AddressSaltField].ToString());
                                            if (bankAccountNumberDecrypted.StartsWith(Security.ro_DecryptFailedPrefix))
                                            {
                                                bankAccountNumberDecrypted = DB.RowField(row, "eCheckBankAccountNumber");
                                            }
                                            row["eCheckBankAccountNumber"] = bankAccountNumberDecrypted;
                                        }
                                    }

                                    foreach (DataRow row in dtAddress.Rows)
                                    {
                                        String CN = DB.RowField(row, "CardNumber");
                                        if (CN.Length != 0 &&
                                            CN != AppLogic.ro_CCNotStoredString)
                                        {
                                            String CNEncrypted = Security.MungeString(CN, row[AddressSaltField].ToString(), spa);

                                            t.AddCommand("update Address set CardNumber=" + DB.SQuote(CNEncrypted) + " where AddressID=" + DB.RowFieldInt(row, "AddressID").ToString());
                                            CN = "1111111111111111";
                                            CN = null;
                                            row["CardNumber"] = "1111111111111111";
                                            row["CardNumber"] = null;
                                        }

                                        // eCheck Details...
                                        String abaCode = DB.RowField(row, "eCheckBankABACode");
                                        if (abaCode.Length != 0 &&
                                            abaCode != AppLogic.ro_CCNotStoredString)
                                        {
                                            String abaCodeEncrypted = Security.MungeString(abaCode, row[AddressSaltField].ToString(), spa);

                                            t.AddCommand("update Address set eCheckBankABACode=" + DB.SQuote(abaCodeEncrypted) + " where AddressID=" + DB.RowFieldInt(row, "AddressID").ToString());
                                            abaCode = "1111111111111111";
                                            abaCode = null;
                                            row["eCheckBankABACode"] = "1111111111111111";
                                            row["eCheckBankABACode"] = null;
                                        }

                                        String bankAccountNumber = DB.RowField(row, "eCheckBankAccountNumber");
                                        if (bankAccountNumber.Length != 0 &&
                                            bankAccountNumber != AppLogic.ro_CCNotStoredString)
                                        {
                                            String bankAccountNumberEncrypted = Security.MungeString(bankAccountNumber, row[AddressSaltField].ToString(), spa);

                                            t.AddCommand("update Address set eCheckBankAccountNumber=" + DB.SQuote(bankAccountNumberEncrypted) + " where AddressID=" + DB.RowFieldInt(row, "AddressID").ToString());
                                            bankAccountNumber = "1111111111111111";
                                            bankAccountNumber = null;
                                            row["eCheckBankAccountNumber"] = "1111111111111111";
                                            row["eCheckBankAccountNumber"] = null;
                                        }
                                    }
                                }
                            }
                        }

                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();

                            string query = "SELECT OrderNumber,OrderGUID,CustomerID,CustomerGUID,EMail,CustomerID,CardNumber, eCheckBankABACode, eCheckBankAccountNumber FROM Orders WHERE " +
                                            " CardNumber IS NOT NULL AND CONVERT(NVARCHAR(4000),CardNumber)<>{0} AND CONVERT(NVARCHAR(4000),CardNumber)<>{1} OR ( " +
                                            " eCheckBankABACode IS NOT NULL AND CONVERT(NVARCHAR(4000),eCheckBankABACode)<>{0} AND CONVERT(NVARCHAR(4000),eCheckBankABACode)<>{1} AND " +
                                            " eCheckBankAccountNumber IS NOT NULL AND CONVERT(NVARCHAR(4000),eCheckBankAccountNumber)<>{0} AND CONVERT(NVARCHAR(4000),eCheckBankAccountNumber)<>{1}) " +
                                            " ORDER BY OrderNumber ";

                            query = string.Format(query, DB.SQuote(AppLogic.ro_CCNotStoredString), DB.SQuote(String.Empty));

                            using (IDataReader rsOrders = DB.GetRS(query, dbconn))
                            {
                                using (DataTable dtOrders = new DataTable())
                                {
                                    dtOrders.Load(rsOrders);

                                    foreach (DataRow row in dtOrders.Rows)
                                    {
                                        String CN = DB.RowField(row, "CardNumber");
                                        if (CN.Length != 0 &&
                                            CN != AppLogic.ro_CCNotStoredString)
                                        {
                                            String CNDecrypted = Security.UnmungeString(CN, row[OrdersSaltField].ToString());
                                            if (CNDecrypted.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                CNDecrypted = DB.RowField(row, "CardNumber");
                                            }
                                            row["CardNumber"] = CNDecrypted;
                                        }


                                        // eCheck Details...
                                        String abaCode = DB.RowField(row, "eCheckBankABACode");
                                        if (abaCode.Length != 0 &&
                                            abaCode != AppLogic.ro_CCNotStoredString)
                                        {
                                            String abaCodeDecrypted = Security.UnmungeString(abaCode, row[OrdersSaltField].ToString());
                                            if (abaCodeDecrypted.StartsWith(Security.ro_DecryptFailedPrefix))
                                            {
                                                abaCodeDecrypted = DB.RowField(row, "eCheckBankABACode");
                                            }
                                            row["eCheckBankABACode"] = abaCodeDecrypted;
                                        }

                                        String bankAccountNumber = DB.RowField(row, "eCheckBankAccountNumber");
                                        if (bankAccountNumber.Length != 0 &&
                                            bankAccountNumber != AppLogic.ro_CCNotStoredString)
                                        {
                                            String bankAccountNumberDecrypted = Security.UnmungeString(bankAccountNumber, row[OrdersSaltField].ToString());
                                            if (bankAccountNumberDecrypted.StartsWith(Security.ro_DecryptFailedPrefix))
                                            {
                                                bankAccountNumberDecrypted = DB.RowField(row, "eCheckBankAccountNumber");
                                            }
                                            row["eCheckBankAccountNumber"] = bankAccountNumberDecrypted;
                                        }
                                    }

                                    foreach (DataRow row in dtOrders.Rows)
                                    {
                                        String CN = DB.RowField(row, "CardNumber");
                                        if (CN.Length != 0 &&
                                            CN != AppLogic.ro_CCNotStoredString)
                                        {
                                            String CNEncrypted = Security.MungeString(CN, row[OrdersSaltField].ToString(), spa);
                                            t.AddCommand("update Orders set CardNumber=" + DB.SQuote(CNEncrypted) + " where OrderNumber=" + DB.RowFieldInt(row, "OrderNumber").ToString());
                                            CN = "1111111111111111";
                                            CN = null;
                                            row["CardNumber"] = "1111111111111111";
                                            row["CardNumber"] = null;
                                        }

                                        // eCheck Details...
                                        String abaCode = DB.RowField(row, "eCheckBankABACode");
                                        if (abaCode.Length != 0 &&
                                            abaCode != AppLogic.ro_CCNotStoredString)
                                        {
                                            String abaCodeEncrypted = Security.MungeString(abaCode, row[OrdersSaltField].ToString(), spa);

                                            t.AddCommand("update Orders set eCheckBankABACode=" + DB.SQuote(abaCodeEncrypted) + " where OrderNumber=" + DB.RowFieldInt(row, "OrderNumber").ToString());
                                            abaCode = "1111111111111111";
                                            abaCode = null;
                                            row["eCheckBankABACode"] = "1111111111111111";
                                            row["eCheckBankABACode"] = null;
                                        }

                                        String bankAccountNumber = DB.RowField(row, "eCheckBankAccountNumber");
                                        if (bankAccountNumber.Length != 0 &&
                                            bankAccountNumber != AppLogic.ro_CCNotStoredString)
                                        {
                                            String bankAccountNumberEncrypted = Security.MungeString(bankAccountNumber, row[OrdersSaltField].ToString(), spa);

                                            t.AddCommand("update Orders set eCheckBankAccountNumber=" + DB.SQuote(bankAccountNumberEncrypted) + " where OrderNumber=" + DB.RowFieldInt(row, "OrderNumber").ToString());
                                            bankAccountNumber = "1111111111111111";
                                            bankAccountNumber = null;
                                            row["eCheckBankAccountNumber"] = "1111111111111111";
                                            row["eCheckBankAccountNumber"] = null;
                                        }
                                    }
                                }
                            }
                        }

                        using (SqlConnection dbconn = DB.dbConn())
                        {
                            dbconn.Open();
                            using (IDataReader rsSecurityLog = DB.GetRS("select LogID,SecurityAction,Description from SecurityLog", dbconn))
                            {
                                using (DataTable dtSecurityLog = new DataTable())
                                {
                                    dtSecurityLog.Load(rsSecurityLog);

                                    foreach (DataRow row in dtSecurityLog.Rows)
                                    {
                                        String DD = DB.RowField(row, "Description");
                                        String DDDecrypted = Security.UnmungeString(DD);
                                        if (DDDecrypted.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            DDDecrypted = DB.RowField(row, "Description");
                                        }
                                        row["Description"] = DDDecrypted;

                                        String DDA = DB.RowField(row, "SecurityAction");
                                        String DDADecrypted = Security.UnmungeString(DDA);
                                        if (DDADecrypted.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            DDADecrypted = DB.RowField(row, "SecurityAction");
                                        }
                                        row["SecurityAction"] = DDADecrypted;
                                    }

                                    // Only the table schema is enforcing the null constraint
                                    dtSecurityLog.Columns["Description"].AllowDBNull = true;
                                    dtSecurityLog.Columns["SecurityAction"].AllowDBNull = true;

                                    foreach (DataRow row in dtSecurityLog.Rows)
                                    {
                                        String DD = DB.RowField(row, "Description");
                                        String DDA = DB.RowField(row, "SecurityAction");
                                        if (DD.Length != 0 ||
                                            DDA.Length != 0)
                                        {
                                            String DDEncrypted = Security.MungeString(DD, spa);
                                            String DDAEncrypted = Security.MungeString(DDA, spa);
                                            t.AddCommand("update SecurityLog set SecurityAction=" + DB.SQuote(DDAEncrypted) + ", Description=" + DB.SQuote(DDEncrypted) + " where logid=" + DB.RowFieldInt(row, "logid").ToString());
                                            DD = "1111111111111111";
                                            DD = null;
                                            row["Description"] = "1111111111111111";
                                            row["Description"] = null;
                                            DDA = "1111111111111111";
                                            DDA = null;
                                            row["SecurityAction"] = "1111111111111111";
                                            row["SecurityAction"] = null;
                                        }
                                    }
                                }
                            }
                        }

                        if (t.Commit())
                        {
                            appSection.Settings["EncryptKey"].Value = m_encryptKey;
                            AppLogic.SetAppConfig("NextKeyChange", DateTime.Now.AddMonths(3).ToString());
                            LogSecurityEvent("EncryptKey Changed", "The encryption key was changed by a super admin using the built-in ChangeEncryptionKey Utility", spa);
                        }
                        else
                        {
                            //Set the encrypt key back, as it was not changed
                            spa.EncryptKey = oldEncryptKey;
                            LogSecurityEvent("Change EncryptKey", "A database error prevented the encryption key from being changed", spa);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                    LogSecurityEvent("Change EncryptKey Failed", "An exception prevented the encryption key from being changed.  View the system event log for more details", spa);
                    exceptions.Add(ex);
                }
                #endregion

                #region Modify machine key
                try
                {
                    if (m_setMachineKey)
                    {
                        //Make sure that if the KeyGenerationType is set to Manual, the user has specified a key
                        if ((m_decryptKeyGenMethod == KeyGenerationMethod.Manual && String.IsNullOrEmpty(m_decryptKey)) ||
                            (m_validationKeyGenMethod == KeyGenerationMethod.Manual && String.IsNullOrEmpty(m_validationKey)))
                        {
                            throw new ArgumentException("If your KeyGenerationMethod is set to manual, you MUST manually specify a valid key!");
                        }

                        //Random cannot be used for machine keys
                        if ((m_decryptKeyGenMethod == KeyGenerationMethod.Auto && m_decryptionKeyLength == KeyLength.Random) || 
                            (m_validationKeyGenMethod == KeyGenerationMethod.Auto && m_validationKeyLength == KeyLength.Random))
                        {
                            throw new ArgumentException("If your KeyGeneration Method is set to Auto, you must manually set the keylength");
                        }

                        MachineKeySection machineKeySec = (MachineKeySection)config.GetSection("system.web/machineKey");

                        machineKeySec.CompatibilityMode = m_compatibilityMode;
                        machineKeySec.Validation = m_validationKeyType;

                        //Setting to anything other than AES will likely cause authentication failures, failure to add to cart, etc.
                        machineKeySec.Decryption = "AES";

                        if (m_decryptKeyGenMethod == KeyGenerationMethod.Auto)
                        {
                            m_decryptKey = Security.CreateRandomKey((int)m_decryptionKeyLength);
                        }
                        else
                        {
                            //Turn the manual key into a hex-encoded string
                            m_decryptKey = Security.ConvertToHex(m_decryptKey);
                        }

                        if (m_validationKeyGenMethod == KeyGenerationMethod.Auto)
                        {
                            m_validationKey = Security.CreateRandomKey((int)m_validationKeyLength);
                        }
                        else
                        {
                            //Turn the manual key into a hex-encoded string
                            m_validationKey = Security.ConvertToHex(m_validationKey);
                        }

                        machineKeySec.DecryptionKey = m_decryptKey;
                        machineKeySec.ValidationKey = m_validationKey;
                        machineKeySec.SectionInformation.ForceSave = true;

                        LogSecurityEvent("Static Machine Key Set/Changed", "The static machine key was set or changed by a super admin using the built-in Machine Key Utility", spa);

                    }
                }
                catch (Exception ex)
                {
                    SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                    LogSecurityEvent("Static Machine Key Set/Change Failed", "The static machine key could not be set or changed.  See the system event log for additional details.", spa);
                    exceptions.Add(ex);
                }
                #endregion

                #region Modify DBConn
                if (m_setDBConn)
                {
                    //TODO
                }
                #endregion

                #region Protect Web.Config

                if (m_protectWebConfig && !appSection.SectionInformation.IsProtected)
                {
                    appSection.SectionInformation.ProtectSection(m_webConfigEncryptProvider.ToString());
                    LogSecurityEvent("Web.config file encrypted", "The web.config appSettings section has been encrypted.", spa);
                }
                else if (!m_protectWebConfig && appSection.SectionInformation.IsProtected)
                {
                    appSection.SectionInformation.UnprotectSection();
                    LogSecurityEvent("Web.config file decrypted", "The web.config appSettings section has been decrypted.", spa);

                }


                #endregion

                LogSecurityEvent("Web.config file saved", "The web.config file has been written to disk.  See prior security log entries for details.", spa);

                //FINALLY, NOW SAVE
                config.Save(ConfigurationSaveMode.Minimal);
            }
            catch (Exception ex)
            {
                SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                LogSecurityEvent("Web.config editing failed", "An attempt to edit the web.config file programmatically failed.  See system event log for details.", spa);
                exceptions.Add(ex);
            }

            return exceptions;
        }

        #endregion


        #region Public properties

        /// <summary>
        /// Boolean value representing whether the DBConnection string will be set/changed
        /// when the Commit() method is called
        /// </summary>
        public bool SetDBConn
        {
            get { return m_setDBConn; }
            set { m_setDBConn = value; }
        }

        /// <summary>
        /// Boolean value representing whether the machineKey will be set/changed
        /// when the Commit() method is called
        /// </summary>
        public bool SetMachineKey
        {
            get { return m_setMachineKey; }
            set { m_setMachineKey = value; }
        }

        /// <summary>
        /// Boolean value representing whether the EncryptKey string will be set/changed
        /// when the Commit() method is called
        /// </summary>
        public bool SetEncryptKey
        {
            get { return m_setEncryptKey; }
            set { m_setEncryptKey = value; }
        }

        /// <summary>
        /// String value used to populate the EncryptKey in the Web.config
        /// IF SetEncryptKey evaluates to true
        /// </summary>
        public string EncryptKey
        {
            get { return m_encryptKey; }
            set { m_encryptKey = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the decryption key
        /// (used for manual key generation)
        /// </summary>
        public string DecryptKey
        {
            get { return m_decryptKey; }
            set { m_decryptKey = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the validation key 
        /// (used for manual key generation)
        /// </summary>
        public string ValidationKey
        {
            get { return m_validationKey; }
            set { m_validationKey = value; }
        }

        /// <summary>
        /// Gets or sets a value determining if the EncryptKey is automatically
        /// or manually generated
        /// </summary>
        public KeyGenerationMethod EncryptKeyGenMethod
        {
            get { return m_encryptKeyGenMethod; }
            set { m_encryptKeyGenMethod = value; }
        }

        /// <summary>
        /// Gets or sets a value determining if the Decryption key is automatically
        /// or manually generated
        /// </summary>
        public KeyGenerationMethod DecryptKeyGenMethod
        {
            get { return m_decryptKeyGenMethod; }
            set { m_decryptKeyGenMethod = value; }
        }

        /// <summary>
        /// Gets or sets a value determining if the Validation key is automatically
        /// or manually generated
        /// </summary>
        public KeyGenerationMethod ValidationKeyGenMethod
        {
            get { return m_validationKeyGenMethod; }
            set { m_validationKeyGenMethod = value; }
        }

        /// <summary>
        /// Sets the hashing or encryption algorithm to use to encrypt/tamper-proof the viewstate
        /// SHA1 - Provides tamper-proofing but NO ENCRYPTION for the viewstate (Decrypt key is used if secure viewstate is specified)
        /// AES  - Preferred method of providing symmetric encryption for the viewstate
        /// MD5  - Not recommended - provides tamper-proofing using a smaller hash than SHA1
        /// </summary>
        public MachineKeyValidation ValidationKeyType
        {
            get { return m_validationKeyType; }
            set { m_validationKeyType = value; }
        }

        /// <summary>
        /// Determines whether stronger encryption mechanisms introduced in .NET 2.0 SP2
        /// are utilized
        /// </summary>
        public MachineKeyCompatibilityMode CompatibilityMode
        {
            get { return m_compatibilityMode; }
            set { m_compatibilityMode = value; }
        }

        /// <summary>
        /// Gets or sets a value determining whether the web.config should be protected
        /// </summary>
        public bool ProtectWebConfig
        {
            get { return m_protectWebConfig; }
            set { m_protectWebConfig = value; }
        }

        /// <summary>
        /// Read-only property indicating the WebConfigEncryptionProvider chosen in your AppConfigs
        /// </summary>
        public string WebConfigEncryptProvider
        {
            get { return m_webConfigEncryptProvider.ToString(); }
        }

        #endregion
    }
}
