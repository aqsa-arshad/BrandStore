﻿using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Web;
using System.Text;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Runtime;
using SFDCSoapClient;
using System.ServiceModel;
using System.Data;


namespace AspDotNetStorefront
{
    /// <summary>
    /// OKTA & SFDC SSO Authentication & Integration class 
    /// </summary>
    public static class AuthenticationSSO
    {

        /// <summary>
        /// Initialize Customer Object after OKTA Authentication
        /// </summary>
        /// <param name="userName">userName</param>
        /// <param name="password">password</param>
        /// <returns>Status</returns>
        public static Customer InitializeCustomerObject(string userName, string password)
        {
            var ThisCustomer = new Customer(userName, true);
            var IsCustomerAvailable = ThisCustomer.HasCustomerRecord;
            var userModel = GetUserModel(userName);

            try
            {
                if (userModel == null) // Execute Local Authentication if User is not Exist in Okta
                    return ThisCustomer;

                if (UserAuthentication(userName, password)) // If User is Authenticated by Okta then Add / Update local Customer object
                {
                    if (!IsCustomerAvailable)
                    {
                        // Add New Customer in local DB
                        InsertCustomer(userName);
                    }
                    // Update Customer in local DB w.r.t Okta UserModel
                    UpdateCustomer(userModel.profile, userName, password, IsCustomerAvailable);
                }
                else if (IsCustomerAvailable) // If User is not Authenticated by Okta then Update local Customer object if exist
                {
                    SqlParameter sp = new SqlParameter("@IsRegistered", System.Data.SqlDbType.TinyInt);
                    sp.Value = 0;
                    SqlParameter[] spa = { sp };
                    ThisCustomer.UpdateCustomer(spa);
                }

            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return new Customer(userName, true);
        }

        /// <summary>
        /// Create Session after sucessfull Login
        /// </summary>
        /// <param name="userName">userName</param>
        /// <param name="password">password</param>
        /// <returns>Status</returns>
        private static bool UserAuthentication(string userName, string password)
        {
            UserAuthenticationModel userAuthenticationModel = new UserAuthenticationModel();
            userAuthenticationModel.username = userName;
            userAuthenticationModel.password = password;

            using (var client = new HttpClient())
            {
                var url = AppLogic.AppConfig("OKTASessionURL");
                try
                {
                    var authorization = AppLogic.AppConfig("OKTAAuthorization") + " " + AppLogic.AppConfig("OKTAAccessToken");
                    client.BaseAddress = new Uri(AppLogic.AppConfig("OKTADefaultURL"));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authorization", authorization);
                }
                catch (Exception ex)
                {
                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }

                var str = JsonConvert.SerializeObject(userAuthenticationModel);
                HttpContent content = new StringContent(str, Encoding.UTF8, "application/json");
                return client.PostAsync(url, content).Result.IsSuccessStatusCode;
            }

        }

        /// <summary>
        /// Get User from Okta by Username
        /// </summary>
        /// <param name="userName">userName</param>
        /// <returns>UserModel</returns>
        public static UserModel GetUserModel(string userName)
        {


            using (var client = new HttpClient())
            {
                var url = AppLogic.AppConfig("OKTAUserURL") + userName;
                try
                {

                    var authorization = AppLogic.AppConfig("OKTAAuthorization") + " " + AppLogic.AppConfig("OKTAAccessToken");
                    client.BaseAddress = new Uri(AppLogic.AppConfig("OKTADefaultURL"));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authorization", authorization);

                }
                catch (Exception ex)
                {
                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }

                var response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var userModel = JsonConvert.DeserializeObject<UserModel>(json);
                    return userModel;
                }
                else
                    return null;


            }
        }

        /// <summary>
        /// Update Customer in local DB w.r.t Okta UserModel
        /// </summary>
        /// <param name="profile">profile</param>
        /// <param name="userName">userName</param>
        /// <param name="password">password</param>
        private static void UpdateCustomer(AspDotNetStorefrontCore.Profile profile, string userName, string password, bool IsCustomerAvailable)
        {
            try
            {
                bool IsSFDCUser = false;
                if (!string.IsNullOrEmpty(profile.sfid))
                    IsSFDCUser = GetSFDCDealerUser(profile, profile.sfid);
                else
                    IsSFDCUser = GetSFDCInternalUser(profile, userName);

                if (!IsSFDCUser)
                {
                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "User '" + userName + "' is not found in SFDC. Hence Application will treat this as in Internal User without any budget.", MessageTypeEnum.Informational, MessageSeverityEnum.Message);
                    profile.userType = UserType.INTERNAL.ToString();
                }
                if (string.IsNullOrEmpty(profile.sfid))
                {
                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                        "User '" + userName + "' sfid or email is not found in SFDC.", MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                    profile.userType = UserType.INTERNAL.ToString();
                }

                Password p = new Password(password);
                int customerLevelID = GetCustomerLevelID(profile.userType);
                
                var ThisCustomer = new Customer(userName);

                if (!IsCustomerAvailable)
                {
                    SqlParameter[] sqlParameter = {
                                        new SqlParameter("@Password", p.SaltedPassword),
                                        new SqlParameter("@SaltKey", p.Salt),
                                        new SqlParameter("@SkinID", AppLogic.DefaultSkinID()), 
                                        new SqlParameter("@IsRegistered", 1), 
                                        new SqlParameter("@Over13Checked", 1),
                                        new SqlParameter("@FirstName", profile.firstName),
                                        new SqlParameter("@LastName", profile.lastName),
                                        new SqlParameter("@Phone", profile.primaryPhone),
                                        new SqlParameter("@CustomerLevelID", customerLevelID),
                                        new SqlParameter("@BillingAddressID", 0),
                                        new SqlParameter("@ShippingAddressID", 0),
                                        new SqlParameter("@SFDCQueryParam", profile.sfid),
                                        new SqlParameter("@IsAdmin", customerLevelID == (int)UserType.STOREADMINISTRATOR ? 1 : 0)
                                       };
                    ThisCustomer.UpdateCustomer(sqlParameter);
                }
                else
                {
                    SqlParameter[] sqlParameter = {
                                        new SqlParameter("@Password", p.SaltedPassword),
                                        new SqlParameter("@SaltKey", p.Salt),
                                        new SqlParameter("@CustomerLevelID", customerLevelID),
                                        new SqlParameter("@IsRegistered", 1), 
                                        new SqlParameter("@SFDCQueryParam", profile.sfid),
                                        new SqlParameter("@IsAdmin", customerLevelID == (int)UserType.STOREADMINISTRATOR ? 1 : 0)
                                       };
                    ThisCustomer.UpdateCustomer(sqlParameter);
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Add New Customer in local DB
        /// </summary>
        /// <param name="userName">userName</param>
        private static void InsertCustomer(string userName)
        {
            try
            {
                var ThisCustomer = new Customer(userName);
                Customer.CreateCustomerRecord(userName, null, ThisCustomer.SkinID,
                            null, null, 0, null, ThisCustomer.LocaleSetting, 0, ThisCustomer.CurrencySetting,
                            ThisCustomer.VATSettingRAW, ThisCustomer.VATRegistrationID, 0);
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Add / Update Customer Address w.r.t Okta UserModel
        /// </summary>
        /// <param name="profile">profile</param>
        /// <param name="CustomerID">CustomerID</param>
        /// <param name="AddressID">AddressID</param>
        /// <returns></returns>
        private static int AddUpdateAddress(AspDotNetStorefrontCore.Profile profile, int CustomerID, int AddressID)
        {

            Address anyAddress = new Address();
            try
            {

                anyAddress.AddressID = AddressID;
                anyAddress.CustomerID = CustomerID;
                anyAddress.NickName = string.IsNullOrEmpty(profile.nickName) ? string.Empty : profile.nickName;
                anyAddress.FirstName = string.IsNullOrEmpty(profile.firstName) ? string.Empty : profile.firstName;
                anyAddress.LastName = string.IsNullOrEmpty(profile.lastName) ? string.Empty : profile.lastName;
                anyAddress.Company = string.IsNullOrEmpty(profile.organization) ? string.Empty : profile.organization;
                anyAddress.Address1 = string.IsNullOrEmpty(profile.streetAddress) ? string.Empty : profile.streetAddress;
                anyAddress.Address2 = string.Empty;
                anyAddress.Suite = string.Empty;
                anyAddress.City = string.IsNullOrEmpty(profile.city) ? string.Empty : profile.city;
                anyAddress.State = string.IsNullOrEmpty(profile.state) ? string.Empty : profile.state;
                anyAddress.Zip = string.IsNullOrEmpty(profile.zipCode) ? string.Empty : profile.zipCode;
                anyAddress.Country = string.IsNullOrEmpty(profile.countryCode) ? string.Empty : profile.countryCode;
                anyAddress.Phone = string.IsNullOrEmpty(profile.primaryPhone) ? string.Empty : profile.primaryPhone;
                anyAddress.ResidenceType = ResidenceTypes.Residential;

                if (AddressID == 0)
                    anyAddress.InsertDB();
                else
                    anyAddress.UpdateDB();
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }

            return anyAddress.AddressID; ;
        }

        /// <summary>
        /// Get CustomerLevelID w.r.t Okta UserType
        /// </summary>
        /// <param name="userType">userType</param>
        /// <returns>CustomerLevelID</returns>
        private static int GetCustomerLevelID(string userType)
        {
            try
            {
                if (!string.IsNullOrEmpty(userType))
                {
                    string enumUserType = userType.Replace(" ", "").ToUpper();
                    if (Enum.IsDefined(typeof(UserType), enumUserType))
                    {
                        return (int)Enum.Parse(typeof(UserType), enumUserType);
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return (int)UserType.SALESREPS;
        }

        /// <summary>
        /// Send Email to User for Change Password Request
        /// </summary>
        /// <param name="userName">userName</param>
        /// <returns>Status</returns>
        public static bool ForgotPasswordRequest(string UserModelID)
        {
            using (var client = new HttpClient())
            {
                var url = AppLogic.AppConfig("OKTAUserURL") + UserModelID + AppLogic.AppConfig("OKTAResetPasswordURL"); //ConfigurationManager.AppSettings["ResetPassword"];
                try
                {
                    var authorization = AppLogic.AppConfig("OKTAAuthorization") + " " + AppLogic.AppConfig("OKTAAccessToken");
                    client.BaseAddress = new Uri(AppLogic.AppConfig("OKTADefaultURL"));
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authorization", authorization);
                }
                catch (Exception ex)
                {
                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                }

                return client.PostAsync(url, new StringContent("")).Result.IsSuccessStatusCode;
            }
        }

        /// <summary>
        /// Get SFDC Dealer User Information by SalesforceID
        /// </summary>
        /// <param name="profile">profile</param>
        /// <param name="sfid">sfid</param>
        private static bool GetSFDCDealerUser(AspDotNetStorefrontCore.Profile profile, string sfid)
        {
            var dealerUserQuery = AppLogic.AppConfig("SFDCDealerUserQuery").Replace(AppLogic.AppConfig("SFDCQueryParam"), sfid);
            QueryResult queryResult = new QueryResult();

            if (QuerySFDC(dealerUserQuery, ref queryResult) == false)
                return false;

            Contact contact = (Contact)queryResult.records.FirstOrDefault();

            profile.firstName = contact.FirstName;
            profile.lastName = contact.LastName;

            if (contact.Account.TrueBLUStatus__c.Equals("ELITE", StringComparison.InvariantCultureIgnoreCase) ||
                    contact.Account.TrueBLUStatus__c.Equals("PREMIER", StringComparison.InvariantCultureIgnoreCase) ||
                    contact.Account.TrueBLUStatus__c.Equals("AUTHORIZED", StringComparison.InvariantCultureIgnoreCase) ||
                    contact.Account.TrueBLUStatus__c.Equals("UNLIMITED", StringComparison.InvariantCultureIgnoreCase))
                profile.userType = "BLU" + contact.Account.TrueBLUStatus__c;
            else
                profile.userType = contact.Account.TrueBLUStatus__c;

            return true;
        }

        /// <summary>
        /// Get SFDC Internal User Information by Email
        /// -- Search User in SFDC with username (Email)
        /// ---- If User Found in SFDC: Search Budget in Employee_Budget__c in SFDC with ID
        /// ---- If User Not Found in SFDC: Search Budget in Employee_Budget__c in SFDC with Email
        /// </summary>
        /// <param name="profile">profile</param>
        /// <param name="email">email</param>
        private static bool GetSFDCInternalUser(AspDotNetStorefrontCore.Profile profile, string email)
        {
            var internalUserQuery = AppLogic.AppConfig("SFDCInternalUserQuery").Replace(AppLogic.AppConfig("SFDCQueryParam"), email);
            QueryResult queryResult = new QueryResult();

            if (QuerySFDC(internalUserQuery, ref queryResult))
            {
                User user = (User)queryResult.records.FirstOrDefault();

                profile.firstName = user.FirstName;
                profile.lastName = user.LastName;
                profile.userType = UserType.SALESREPS.ToString();
                profile.sfid = user.Sales_Rep_ID__c;

                //////// If User Found in SFDC: Search Budget in Employee_Budget__c in SFDC with ID
                //////var SFDCBudgetQueryById = AppLogic.AppConfig("SFDCBudgetQueryById").Replace(AppLogic.AppConfig("SFDCQueryParam"), user.Sales_Rep_ID__c);

                //////if (QuerySFDC(SFDCBudgetQueryById, ref queryResult) == true)
                //////{
                //////    // Set Budget
                //////}
                return true;
            }
            else
            {
                profile.sfid = email;
                
                //////var SFDCBudgetQueryByEmail = AppLogic.AppConfig("SFDCBudgetQueryByEmail").Replace(AppLogic.AppConfig("SFDCQueryParam"), email);
                
                //////if (QuerySFDC(SFDCBudgetQueryByEmail, ref queryResult) == true)
                //////{
                //////    // Set Budget
                //////}
                return false;
            }
        }

        /// <summary>
        /// Query SFDC
        /// </summary>
        /// <param name="query">query</param>
        /// <param name="queryResult">queryResult</param>
        /// <returns>queryResult</returns>
        private static bool QuerySFDC(string query, ref QueryResult queryResult)
        {
            var username = AppLogic.AppConfig("SFDCUsername");
            var password = AppLogic.AppConfig("SFDCPassword");
            var securityToken = AppLogic.AppConfig("SFDCSecurityToken");
            var flag = true;

            SoapClient loginClient; // for login endpoint
            SoapClient client; // for API endpoint
            SessionHeader header;
            EndpointAddress endpoint;

            // Create a SoapClient specifically for logging in
            loginClient = new SoapClient();

            // SFDC Login
            LoginResult lr = loginClient.login(null, username, password + securityToken);

            if (lr.passwordExpired)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "SFDC Login Password Is Expired", MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                flag = false;
            }

            endpoint = new EndpointAddress(lr.serverUrl);
            header = new SessionHeader();
            header.sessionId = lr.sessionId;

            // Create and cache an API endpoint client
            client = new SoapClient("Soap", endpoint);
            client.query(header, null, null, null, query, out queryResult);

            if (queryResult.size == 0)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "Record not found for " + username, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                flag = false;
            }

            // SFDC Logout
            client.logout(header);
            return flag;
        }

        /// <summary>
        /// Get Customer Fund
        /// 1. Get Customer Fund from SFDC.
        /// 2. Update Local DB.
        /// 3. Return lstCustomerFund.
        /// </summary>
        /// <param name="CustomerID">CustomerID</param>
        /// <returns>lstCustomerFund</returns>
        public static List<CustomerFund> GetCustomerFund(int CustomerID)
        {
            List<CustomerFund> lstCustomerFund = new List<CustomerFund>();
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerFundSelectByCustomerID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", CustomerID);

                        IDataReader idr = cmd.ExecuteReader();

                        while (idr.Read())
                        {
                            lstCustomerFund.Add(new CustomerFund()
                            {
                                CustomerID = idr.GetInt32(idr.GetOrdinal("CustomerID")),
                                FundID = idr.GetInt32(idr.GetOrdinal("FundID")),
                                FundName = idr.GetString(idr.GetOrdinal("FundName")),
                                Amount = idr.GetDecimal(idr.GetOrdinal("Amount"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return lstCustomerFund;
        }

        /// <summary>
        /// Get Customer Fund
        /// </summary>
        /// <param name="CustomerID">CustomerID</param>
        /// <param name="FundID">FundID</param>
        /// <returns>customerFund</returns>
        public static CustomerFund GetCustomerFund(int CustomerID, int FundID)
        {
            CustomerFund customerFund = new CustomerFund();
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerFundSelectByCustomerIDFundID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", CustomerID);
                        cmd.Parameters.AddWithValue("@FundID", FundID);

                        IDataReader idr = cmd.ExecuteReader();

                        if (idr.Read())
                        {
                            customerFund.CustomerID = idr.GetInt32(idr.GetOrdinal("CustomerID"));
                            customerFund.FundID = idr.GetInt32(idr.GetOrdinal("FundID"));
                            customerFund.FundName = idr.GetString(idr.GetOrdinal("FundName"));
                            customerFund.Amount = idr.GetDecimal(idr.GetOrdinal("Amount"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return customerFund;
        }

        /// <summary>
        /// Update Customer Fund
        /// </summary>
        /// <param name="lstCustomerFund">lstCustomerFund</param>
        public static void UpdateCustomerFund(List<CustomerFund> lstCustomerFund)
        {
            if (lstCustomerFund == null || lstCustomerFund.Count == 0)
                return;

            foreach (CustomerFund customerFund in lstCustomerFund)
                UpdateCustomerFund(customerFund.CustomerID, customerFund.FundID, customerFund.Amount);
        }

        /// <summary>
        /// Update Customer Fund
        /// </summary>
        /// <param name="CustomerID">CustomerID</param>
        /// <param name="FundID">FundID</param>
        /// <param name="Amount">Amount</param>
        public static void UpdateCustomerFund(int CustomerID, int FundID, decimal Amount)
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerFundUpdate", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", CustomerID);
                        cmd.Parameters.AddWithValue("@FundID", FundID);
                        cmd.Parameters.AddWithValue("@Amount", Amount);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        /// <summary>
        /// Gets the budget percentage ratio by using CustomerLevelID.
        /// </summary>
        /// <param name="CustomerLevelID">The customer level identifier.</param>
        /// <returns></returns>
        public static List<BudgetPercentageRatio> GetBudgetPercentageRatio(int CustomerLevelID)
        {
            var lstBudgetPercentageRatio = new List<BudgetPercentageRatio>();
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_BudgetPercentageRatioSelectByCustomerLevelID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerLevelID", CustomerLevelID);
                        IDataReader idr = cmd.ExecuteReader();
                        while (idr.Read())
                        {
                            lstBudgetPercentageRatio.Add(new BudgetPercentageRatio()
                            {
                                CustomerLevelID = idr.GetInt32(idr.GetOrdinal("CustomerLevelID")),
                                CategoryID = idr.GetInt32(idr.GetOrdinal("CategoryID")),
                                BudgetPercentageValue = idr.GetInt32(idr.GetOrdinal("BudgetPercentageValue")),
                                IsActive = idr.GetBoolean(idr.GetOrdinal("isActive"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return lstBudgetPercentageRatio;
        }

        /// <summary>
        /// Gets the budget percentage ratio by using CustomerLevelID and CategoryID.
        /// </summary>
        /// <param name="CustomerLevelID">The customer level identifier.</param>
        /// <param name="CategoryID">The category identifier.</param>
        /// <returns></returns>
        public static BudgetPercentageRatio GetBudgetPercentageRatio(int CustomerLevelID, int CategoryID)
        {
            var budgetPercentageRatio = new BudgetPercentageRatio();
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_BudgetPercentageRatioSelectByCustomerLevelIDCategoryID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerLevelID", CustomerLevelID);
                        cmd.Parameters.AddWithValue("@CategoryID", CategoryID);
                        IDataReader idr = cmd.ExecuteReader();
                        if (idr.Read())
                        {
                            budgetPercentageRatio.CustomerLevelID = idr.GetInt32(idr.GetOrdinal("CustomerLevelID"));
                            budgetPercentageRatio.CategoryID = idr.GetInt32(idr.GetOrdinal("CategoryID"));
                            budgetPercentageRatio.BudgetPercentageValue =
                                idr.GetInt32(idr.GetOrdinal("BudgetPercentageValue"));
                            budgetPercentageRatio.IsActive = idr.GetBoolean(idr.GetOrdinal("isActive"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return budgetPercentageRatio;
        }
    }
}