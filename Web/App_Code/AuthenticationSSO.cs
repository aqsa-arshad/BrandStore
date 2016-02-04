using AspDotNetStorefrontCore;
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
using System.Text.RegularExpressions;


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
            var thisCustomer = new Customer(userName, true);
            var isCustomerAvailable = thisCustomer.HasCustomerRecord;
            var userModel = GetUserModel(userName);

            try
            {
                if (userModel == null) // Execute Local Authentication if User is not Exist in Okta
                    return thisCustomer;

                if (UserAuthentication(userName, password)) // If User is Authenticated by Okta then Add / Update local Customer object
                {
                    if (!isCustomerAvailable)
                    {
                        // Add New Customer in local DB
                        InsertCustomer(userName);
                    }
                    // Update Customer in local DB w.r.t Okta UserModel
                    UpdateCustomer(userModel.profile, userName, password, isCustomerAvailable);
                    UpdateCustomerFundFromSFDC(thisCustomer.CustomerID == 0 ? new Customer(userName).CustomerID : thisCustomer.CustomerID);

                    ////// START - TEST CommitCustomerFund Functionality

                    //// 1. Get Funds
                    //List<CustomerFund> lstCustomerFund = GetCustomerFund(thisCustomer.CustomerID == 0 ? new Customer(userName).CustomerID : thisCustomer.CustomerID, false);
                    //// 2. Update Amount Used
                    //lstCustomerFund.ForEach(x => x.AmountUsed = 100);
                    //// 3. Update Customer Fund AmountUsed
                    //UpdateCustomerFundAmountUsed(lstCustomerFund);
                    //// 4. CommitCustomerFund
                    //CommitCustomerFund(thisCustomer.CustomerID == 0 ? new Customer(userName).CustomerID : thisCustomer.CustomerID);

                    ////// EMD - TEST CommitCustomerFund Functionlaity

                    ////// START - TEST GetSubordinateUsers Functionality
                    //GetSubordinateUsers("davewe=jeld-wen.com@example.com");
                    ////// START - TEST GetSubordinateUsers Functionality
                }
                else if (isCustomerAvailable) // If User is not Authenticated by Okta then Update local Customer object if exist
                {
                    //SqlParameter sp = new SqlParameter("@IsRegistered", System.Data.SqlDbType.TinyInt);
                    //sp.Value = 0;
                    //SqlParameter[] spa = { sp };
                    //thisCustomer.UpdateCustomer(spa);
                    return new Customer("");
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
                                        new SqlParameter("@HasSubordinates", HasSubordinates(customerLevelID, profile.sfid)),
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
                                        new SqlParameter("@HasSubordinates", HasSubordinates(customerLevelID, profile.sfid)),
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
            bool flag = false;
            try
            {
                var dealerUserQuery = AppLogic.AppConfig("SFDCDealerUserQuery").Replace(AppLogic.AppConfig("SFDCQueryParam"), sfid);
                QueryResult queryResult = new QueryResult();

                if (QuerySFDC(dealerUserQuery, ref queryResult) != false)
                {
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

                    flag = true;
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return flag;
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
            bool flag = false;
            try
            {
                var internalUserQuery = AppLogic.AppConfig("SFDCInternalUserQuery").Replace(AppLogic.AppConfig("SFDCQueryParam"), email);
                QueryResult queryResult = new QueryResult();

                if (QuerySFDC(internalUserQuery, ref queryResult))
                {
                    User user = (User)queryResult.records.FirstOrDefault();

                    profile.firstName = user.FirstName;
                    profile.lastName = user.LastName;
                    profile.userType = UserType.SALESREPS.ToString();
                    profile.sfid = user.Id;
                    flag = true;
                }
                else
                {
                    profile.sfid = email;
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return flag;
        }

        /// <summary>
        /// Query SFDC
        /// </summary>
        /// <param name="query">query</param>
        /// <param name="queryResult">queryResult</param>
        /// <returns>queryResult</returns>
        private static bool QuerySFDC(string query, ref QueryResult queryResult)
        {
            var flag = false;
            try
            {
                SoapClient client = new SoapClient();
                SessionHeader header = new SessionHeader();

                if (GetSFDCSoapClient(ref client, ref header))
                {
                    client.query(header, null, null, null, query, out queryResult);

                    if (queryResult.size > 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                            "Record not found for query: " + query, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                        flag = false;
                    }

                    // SFDC Logout
                    client.logout(header);
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return flag;
        }

        /// <summary>
        /// Get SFDC SoapClient After Login
        /// </summary>
        /// <param name="client">By Ref SoapClient</param>
        /// <param name="header">By Ref SessionHeader</param>
        /// <returns></returns>
        private static bool GetSFDCSoapClient(ref SoapClient client, ref SessionHeader header)
        {
            var username = AppLogic.AppConfig("SFDCUsername");
            var password = AppLogic.AppConfig("SFDCPassword");
            var securityToken = AppLogic.AppConfig("SFDCSecurityToken");

            SoapClient loginClient; // for login endpoint
            EndpointAddress endpoint;

            // Create a SoapClient specifically for logging in
            loginClient = new SoapClient();

            // SFDC Login
            LoginResult lr = loginClient.login(null, username, password + securityToken);

            if (lr.passwordExpired)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "SFDC Login Password Is Expired", MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                return false;
            }

            endpoint = new EndpointAddress(lr.serverUrl);
            header.sessionId = lr.sessionId;

            // Create and cache an API endpoint client
            client = new SoapClient("Soap", endpoint);
            return true;
        }

        /// <summary>
        /// Get Customer Fund
        /// 1. Get Customer Fund from SFDC.
        /// 2. Update Local DB.
        /// 3. Return lstCustomerFund.
        /// </summary>
        /// <param name="CustomerID">CustomerID</param>
        /// <returns>lstCustomerFund</returns>
        public static List<CustomerFund> GetCustomerFund(int customerID, bool IsUpdateSFDC = false)
        {
            if (customerID == 0)
                return new List<CustomerFund>();

            if (IsUpdateSFDC)
                UpdateCustomerFundFromSFDC(customerID);

            List<CustomerFund> lstCustomerFund = new List<CustomerFund>();
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerFundSelectByCustomerID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);

                        IDataReader idr = cmd.ExecuteReader();

                        while (idr.Read())
                        {
                            lstCustomerFund.Add(new CustomerFund()
                            {
                                CustomerID = idr.GetInt32(idr.GetOrdinal("CustomerID")),
                                FundID = idr.GetInt32(idr.GetOrdinal("FundID")),
                                FundName = idr.GetString(idr.GetOrdinal("FundName")),
                                Amount = idr.GetDecimal(idr.GetOrdinal("Amount")),
                                AmountUsed = idr.GetDecimal(idr.GetOrdinal("AmountUsed"))
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
        /// <param name="IsUpdateSFDC">If True then Funds will be updated from SFDC</param>
        /// <returns>customerFund</returns>
        public static CustomerFund GetCustomerFund(int customerID, int fundID, bool IsUpdateSFDC = false)
        {
            if (customerID == 0 || fundID == 0)
                return new CustomerFund();

            if (IsUpdateSFDC)
                UpdateCustomerFundFromSFDC(customerID);

            CustomerFund customerFund = new CustomerFund();
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerFundSelectByCustomerIDFundID", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
                        cmd.Parameters.AddWithValue("@FundID", fundID);

                        IDataReader idr = cmd.ExecuteReader();

                        if (idr.Read())
                        {
                            customerFund.CustomerID = idr.GetInt32(idr.GetOrdinal("CustomerID"));
                            customerFund.FundID = idr.GetInt32(idr.GetOrdinal("FundID"));
                            customerFund.FundName = idr.GetString(idr.GetOrdinal("FundName"));
                            customerFund.Amount = idr.GetDecimal(idr.GetOrdinal("Amount"));
                            customerFund.AmountUsed = idr.GetDecimal(idr.GetOrdinal("AmountUsed"));
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
        /// Add Update Customer Fund From SFDC
        /// </summary>
        /// <param name="customerID">customerID</param>
        /// <returns></returns>
        private static void UpdateCustomerFundFromSFDC(int customerID)
        {
            if (!AppLogic.AppConfig("UseSFDCBudget").ToBool())
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "AppConfig UseSFDCBudget is set to false.", MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                return;
            }

            try
            {
                Customer customer = new Customer(customerID);

                if (IsDealerUser(customer.CustomerLevelID))
                {
                    GetDealerUserFundFromSFDC(customer.CustomerID, customer.SFDCQueryParam, IsTrueBluDealerUser(customer.CustomerLevelID));
                }
                else if (IsInternalUser(customer.CustomerLevelID))
                {
                    GetInternalUserFundFromSFDC(customer.CustomerID, customer.SFDCQueryParam);
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
        /// Update Customer Fund To SFDC
        /// </summary>
        /// <param name="lstCustomerFund">lstCustomerFund</param>
        private static void UpdateCustomerFundToSFDC(List<CustomerFund> lstCustomerFund)
        {
            if (!AppLogic.AppConfig("UseSFDCBudget").ToBool())
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "AppConfig UseSFDCBudget is set to false.", MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                return;
            }
            try
            {
                Customer customer = new Customer(lstCustomerFund[0].CustomerID);
                if (IsDealerUser(customer.CustomerLevelID))
                {
                    UpdateDealerUserFundToSFDC(customer.SFDCQueryParam, lstCustomerFund);
                }
                else if (IsInternalUser(customer.CustomerLevelID))
                {
                    UpdateInternalUserFundToSFDC(customer.SFDCQueryParam, lstCustomerFund);
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
        /// Update Dealer User Fund To SFDC
        /// </summary>
        /// <param name="SFDCQueryParam">SFDCQueryParam</param>
        /// <param name="lstCustomerFund">SFDCQueryParam</param>
        private static void UpdateDealerUserFundToSFDC(string SFDCQueryParam, List<CustomerFund> lstCustomerFund)
        {
            try
            {
                var query = AppLogic.AppConfig("SFDCDealerUserQuery").Replace(AppLogic.AppConfig("SFDCQueryParam"), SFDCQueryParam);
                QueryResult queryResult = new QueryResult();

                if (QuerySFDC(query, ref queryResult))
                {
                    Contact contact = (Contact)queryResult.records.FirstOrDefault();

                    //Contact updateContact = new Contact();
                    Account updateAccount = new Account();
                    updateAccount.Id = contact.Account.Id;

                    foreach (CustomerFund customerFund in lstCustomerFund)
                    {
                        if (customerFund.FundID == (int)FundType.BLUBucks)
                        {
                            updateAccount.Co_op_budget__c = Convert.ToDouble(customerFund.Amount);
                            updateAccount.Co_op_budget__cSpecified = true;
                        }
                        else if (customerFund.FundID == (int)FundType.DirectMailFunds)
                        {
                            updateAccount.Direct_Marketing_Funds__c = Convert.ToDouble(customerFund.Amount);
                            updateAccount.Direct_Marketing_Funds__cSpecified = true;
                        }
                        else if (customerFund.FundID == (int)FundType.DisplayFunds)
                        {
                            updateAccount.Display_Funds__c = Convert.ToDouble(customerFund.Amount);
                            updateAccount.Display_Funds__cSpecified = true;
                        }
                        else if (customerFund.FundID == (int)FundType.LiteratureFunds)
                        {
                            updateAccount.Literature_Funds__c = Convert.ToDouble(customerFund.Amount);
                            updateAccount.Literature_Funds__cSpecified = true;
                        }
                        else if (customerFund.FundID == (int)FundType.POPFunds)
                        {
                            updateAccount.POP_Funds__c = Convert.ToDouble(customerFund.Amount);
                            updateAccount.POP_Funds__cSpecified = true;
                        }
                    }

                    SoapClient client = new SoapClient();
                    SessionHeader header = new SessionHeader();

                    if (GetSFDCSoapClient(ref client, ref header))
                    {
                        SaveResult[] saveResult = new SaveResult[1];
                        LimitInfo[] limitInfo = new LimitInfo[1];

                        client.update(header, null, null, null, null, null, null, null, null, null, null, null, null, new sObject[] { updateAccount }, out limitInfo, out saveResult);

                        if (!saveResult[0].success)
                        {
                            SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                                saveResult.FirstOrDefault().errors.FirstOrDefault().message, MessageTypeEnum.Informational, MessageSeverityEnum.Error);
                        }

                        // SFDC Logout
                        client.logout(header);
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
        /// Update Internal User Fund To SFDC
        /// </summary>
        /// <param name="SFDCQueryParam">SFDCQueryParam</param>
        /// <param name="lstCustomerFund">lstCustomerFund</param>
        private static void UpdateInternalUserFundToSFDC(string SFDCQueryParam, List<CustomerFund> lstCustomerFund)
        {
            try
            {
                Regex rgx = new Regex(@"^[a-zA-Z0-9][-\w\.\+]*@([a-zA-Z0-9][\w\-]*\.)+[a-zA-Z]{2,4}$");
                string query = string.Empty;

                if (rgx.IsMatch(SFDCQueryParam))
                    query = AppLogic.AppConfig("SFDCBudgetQueryByEmail").Replace(AppLogic.AppConfig("SFDCQueryParam"), SFDCQueryParam);
                else
                    query = AppLogic.AppConfig("SFDCBudgetQueryById").Replace(AppLogic.AppConfig("SFDCQueryParam"), SFDCQueryParam);

                QueryResult queryResult = new QueryResult();
                if (QuerySFDC(query, ref queryResult))
                {
                    Brand_Store_Budget__c updateEmployeeBudget = new Brand_Store_Budget__c();

                    foreach (CustomerFund customerFund in lstCustomerFund)
                    {
                        if (customerFund.FundID == (int)FundType.SOFFunds)
                        {
                            Brand_Store_Budget__c employeeBudget = (Brand_Store_Budget__c)queryResult.records.FirstOrDefault();
                            updateEmployeeBudget.Id = employeeBudget.Id;
                            updateEmployeeBudget.Current_Balance__c = Convert.ToDouble(customerFund.Amount);
                            updateEmployeeBudget.Current_Balance__cSpecified = true;
                        }
                    }

                    SoapClient client = new SoapClient();
                    SessionHeader header = new SessionHeader();

                    if (GetSFDCSoapClient(ref client, ref header))
                    {
                        SaveResult[] saveResult = new SaveResult[1];
                        LimitInfo[] limitInfo = new LimitInfo[1];

                        client.update(header, null, null, null, null, null, null, null, null, null, null, null, null, new sObject[] { updateEmployeeBudget }, out limitInfo, out saveResult);

                        if (!saveResult[0].success)
                        {
                            SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                                saveResult.FirstOrDefault().errors.FirstOrDefault().message, MessageTypeEnum.Informational, MessageSeverityEnum.Error);
                        }

                        // SFDC Logout
                        client.logout(header);
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
        /// Get & Set Dealer User Fund From SFDC
        /// </summary>
        /// <param name="customerID">CustomerID</param>
        /// <param name="SFDCQueryParam">SFDCQueryParam</param>
        private static void GetDealerUserFundFromSFDC(int customerID, string SFDCQueryParam, bool IsTrueBluDealerUser)
        {
            try
            {
                var query = AppLogic.AppConfig("SFDCDealerUserQuery").Replace(AppLogic.AppConfig("SFDCQueryParam"), SFDCQueryParam);
                QueryResult queryResult = new QueryResult();

                if (QuerySFDC(query, ref queryResult))
                {
                    Contact contact = (Contact)queryResult.records.FirstOrDefault();

                    for (int i = 1; i <= 6; i++)
                    {
                        if (i == (int)FundType.BLUBucks && IsTrueBluDealerUser)
                        {
                            CustomerFund customerFund = GetCustomerFund(customerID, i, false);
                            if (customerFund.CustomerID == 0)
                                AddCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.Co_op_budget__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.Co_op_budget__c));
                            else
                                UpdateCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.Co_op_budget__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.Co_op_budget__c));
                        }
                        else if (i == (int)FundType.DirectMailFunds)
                        {
                            CustomerFund customerFund = GetCustomerFund(customerID, i, false);
                            if (customerFund.CustomerID == 0)
                                AddCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.Direct_Marketing_Funds__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.Direct_Marketing_Funds__c));
                            else
                                UpdateCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.Direct_Marketing_Funds__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.Direct_Marketing_Funds__c));
                        }
                        else if (i == (int)FundType.DisplayFunds)
                        {
                            CustomerFund customerFund = GetCustomerFund(customerID, i, false);
                            if (customerFund.CustomerID == 0)
                                AddCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.Display_Funds__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.Display_Funds__c));
                            else
                                UpdateCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.Display_Funds__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.Display_Funds__c));
                        }
                        else if (i == (int)FundType.LiteratureFunds)
                        {
                            CustomerFund customerFund = GetCustomerFund(customerID, i, false);
                            if (customerFund.CustomerID == 0)
                                AddCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.Literature_Funds__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.Literature_Funds__c));
                            else
                                UpdateCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.Literature_Funds__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.Literature_Funds__c));
                        }
                        else if (i == (int)FundType.POPFunds)
                        {
                            CustomerFund customerFund = GetCustomerFund(customerID, i, false);
                            if (customerFund.CustomerID == 0)
                                AddCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.POP_Funds__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.POP_Funds__c));
                            else
                                UpdateCustomerFundAmount(customerID, i, Convert.ToDecimal(contact.Account.POP_Funds__c) < 0 ? 0 : Convert.ToDecimal(contact.Account.POP_Funds__c));
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
        }

        /// <summary>
        /// Get & Set Internal User Fund From SFDC
        /// </summary>
        /// <param name="SFDCQueryParam">SFDCQueryParam</param>
        private static void GetInternalUserFundFromSFDC(int customerID, string SFDCQueryParam)
        {
            try
            {
                Regex rgx = new Regex(@"^[a-zA-Z0-9][-\w\.\+]*@([a-zA-Z0-9][\w\-]*\.)+[a-zA-Z]{2,4}$");
                string query = string.Empty;

                if (rgx.IsMatch(SFDCQueryParam))
                    query = AppLogic.AppConfig("SFDCBudgetQueryByEmail").Replace(AppLogic.AppConfig("SFDCQueryParam"), SFDCQueryParam);
                else
                    query = AppLogic.AppConfig("SFDCBudgetQueryById").Replace(AppLogic.AppConfig("SFDCQueryParam"), SFDCQueryParam);

                QueryResult queryResult = new QueryResult();
                if (QuerySFDC(query, ref queryResult))
                {
                    Brand_Store_Budget__c employeeBudget = (Brand_Store_Budget__c)queryResult.records.FirstOrDefault();
                    CustomerFund customerFund = GetCustomerFund(customerID, (int)FundType.SOFFunds, false);
                    if (customerFund.CustomerID == 0)
                        AddCustomerFundAmount(customerID, (int)FundType.SOFFunds, Convert.ToDecimal(employeeBudget.Current_Balance__c) < 0 ? 0 : Convert.ToDecimal(employeeBudget.Current_Balance__c));
                    else
                        UpdateCustomerFundAmount(customerID, (int)FundType.SOFFunds, Convert.ToDecimal(employeeBudget.Current_Balance__c) < 0 ? 0 : Convert.ToDecimal(employeeBudget.Current_Balance__c));
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
        /// Validate if Dealer User
        /// </summary>
        /// <param name="customerLevelID">customerLevelID</param>        
        public static bool IsDealerUser(int customerLevelID)
        {
            if (customerLevelID == (int)UserType.BLUAUTHORIZED ||
                customerLevelID == (int)UserType.BLUELITE ||
                customerLevelID == (int)UserType.BLUPREMIER ||
                customerLevelID == (int)UserType.BLUUNLIMITED ||
                customerLevelID == (int)UserType.HOMEDEPOT ||
                customerLevelID == (int)UserType.LOWES ||
                customerLevelID == (int)UserType.MENARDS ||
                customerLevelID == (int)UserType.POTENTIAL)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Validate if Dealer User is True Blu
        /// </summary>
        /// <param name="customerLevelID">customerLevelID</param>        
        public static bool IsTrueBluDealerUser(int customerLevelID)
        {
            if (customerLevelID == (int)UserType.BLUAUTHORIZED ||
                customerLevelID == (int)UserType.BLUELITE ||
                customerLevelID == (int)UserType.BLUPREMIER ||
                customerLevelID == (int)UserType.BLUUNLIMITED)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Validate if Internal User
        /// </summary>
        /// <param name="customerLevelID">customerLevelID</param>
        public static bool IsInternalUser(int customerLevelID)
        {
            if (customerLevelID == (int)UserType.INTERNAL ||
                customerLevelID == (int)UserType.SALESREPS)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Add Customer Fund
        /// </summary>
        /// <param name="customerID">CustomerID</param>
        /// <param name="fundID">FundID</param>
        /// <param name="amount">Amount</param>
        /// <param name="amountUsed">AmountUsed</param>
        private static void AddCustomerFundAmount(int customerID, int fundID, decimal amount)
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerFundInsert", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
                        cmd.Parameters.AddWithValue("@FundID", fundID);
                        cmd.Parameters.AddWithValue("@Amount", amount);
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
        /// Update Customer Fund Amount
        /// </summary>
        /// <param name="lstCustomerFund">lstCustomerFund</param>
        private static void UpdateCustomerFundAmount(List<CustomerFund> lstCustomerFund)
        {
            if (lstCustomerFund == null || lstCustomerFund.Count == 0)
                return;

            foreach (CustomerFund customerFund in lstCustomerFund)
                UpdateCustomerFundAmount(customerFund.CustomerID, customerFund.FundID, customerFund.Amount);
        }

        /// <summary>
        /// Update Customer Fund Amount By CustomerID & FundID
        /// </summary>
        /// <param name="CustomerID">CustomerID</param>
        /// <param name="FundID">FundID</param>
        /// <param name="Amount">Amount</param>
        private static void UpdateCustomerFundAmount(int customerID, int fundID, decimal amount)
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerFundUpdateAmount", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
                        cmd.Parameters.AddWithValue("@FundID", fundID);
                        cmd.Parameters.AddWithValue("@Amount", amount);
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
        /// Update Customer Fund AmountUsed
        /// </summary>
        /// <param name="lstCustomerFund">lstCustomerFund</param>
        public static void UpdateCustomerFundAmountUsed(List<CustomerFund> lstCustomerFund)
        {
            if (lstCustomerFund == null || lstCustomerFund.Count == 0)
                return;

            foreach (CustomerFund customerFund in lstCustomerFund)
                UpdateCustomerFundAmountUsed(customerFund.CustomerID, customerFund.FundID, customerFund.AmountUsed);
        }

        /// <summary>
        /// Update Customer Fund AmountUsed By CustomerID & FundID
        /// </summary>
        /// <param name="CustomerID">CustomerID</param>
        /// <param name="FundID">FundID</param>
        /// <param name="Amount">AmountUsed</param>
        public static void UpdateCustomerFundAmountUsed(int customerID, int fundID, decimal amountUsed)
        {
            try
            {
                using (var conn = DB.dbConn())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("aspdnsf_CustomerFundUpdateAmountUsed", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CustomerID", customerID);
                        cmd.Parameters.AddWithValue("@FundID", fundID);
                        cmd.Parameters.AddWithValue("@AmountUsed", amountUsed);
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
        /// Commit Customer Fund After Placing Successful Order to Update SFDC Data.
        /// </summary>
        /// <param name="customerID">CustomerID</param>
        public static bool CommitCustomerFund(int customerID)
        {

            if (customerID == 0)
                return false;
            else if (!AppLogic.AppConfig("UseSFDCBudget").ToBool())
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "AppConfig UseSFDCBudget is set to false.", MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                return false;
            }

            bool flag = false;
            try
            {
                List<CustomerFund> lstCustomerFund = GetCustomerFund(customerID, true);

                if (!ValidateCustomerFund(lstCustomerFund))
                {
                    flag = false;
                }
                else
                {
                    lstCustomerFund = CalculateCustomerFundAmount(lstCustomerFund);
                    UpdateCustomerFundToSFDC(lstCustomerFund);
                    UpdateCustomerFundAmount(lstCustomerFund);
                    UpdateCustomerFundAmountUsed(lstCustomerFund);

                    flag = true;
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                  ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                  MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return flag;
        }

        /// <summary>
        /// Validate Customer Fund if Amount is Less than AmountUsed
        /// </summary>
        /// <param name="lstCustomerFund">lstCustomerFund</param>
        /// <returns>Status</returns>
        public static bool ValidateCustomerFund(List<CustomerFund> lstCustomerFund)
        {
            if (lstCustomerFund.Count == 0 || lstCustomerFund.Where(x => x.Amount - x.AmountUsed < 0).Count() > 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Validate Customer Fund By CustomerID
        /// </summary>
        /// <param name="customerID">CustomerID</param>
        /// <returns>Status</returns>
        public static bool ValidateCustomerFund(int customerID)
        {
            return ValidateCustomerFund(GetCustomerFund(customerID, false));
        }

        /// <summary>
        /// Calculate Customer Fund Amount of List<CustomerFund>
        /// </summary>
        /// <param name="lstCustomerFund">lstCustomerFund</param>
        /// <returns>Calculated lstCustomerFund</returns>
        private static List<CustomerFund> CalculateCustomerFundAmount(List<CustomerFund> lstCustomerFund)
        {
            lstCustomerFund.ForEach(x => x.Amount = x.Amount - x.AmountUsed);
            lstCustomerFund.ForEach(x => x.AmountUsed = 0);
            return lstCustomerFund;
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

        /// <summary>
        /// Has Subordinates?
        /// </summary>
        /// <param name="customerLevelID">customerLevelID</param>
        /// <param name="SFDCQueryParam">SFDCQueryParam</param>
        /// <returns></returns>
        private static bool HasSubordinates(int customerLevelID, string SFDCQueryParam)
        {
            if (customerLevelID != (int)UserType.SALESREPS)
                return false;

            List<SFDCSoapClient.Account> lstAccount = GetSubordinateAccounts(SFDCQueryParam);
            if (lstAccount.Count == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Get User's Subordinate
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>lstAccount<SFDCSoapClient.User></returns>
        public static List<SFDCSoapClient.Account> GetSubordinateAccounts(string SFDCQueryParam)
        {
            List<SFDCSoapClient.Account> lstAccount = new List<SFDCSoapClient.Account>();
            Regex rgx = new Regex(@"^[a-zA-Z0-9][-\w\.\+]*@([a-zA-Z0-9][\w\-]*\.)+[a-zA-Z]{2,4}$");
            if (string.IsNullOrEmpty(SFDCQueryParam) || rgx.IsMatch(SFDCQueryParam))
                return lstAccount;

            try
            {
                string query = string.Empty;
                QueryResult queryResult = new QueryResult();
                query = AppLogic.AppConfig("SFDCSubordinateUsersQuery").Replace(AppLogic.AppConfig("SFDCQueryParam"), SFDCQueryParam);

                if (QuerySFDC(query, ref queryResult))
                {
                    sObject[] accounts = queryResult.records;
                    foreach (sObject account in accounts)
                    {
                        lstAccount.Add((SFDCSoapClient.Account)account);
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return lstAccount;
        }

        /// <summary>
        /// Get Subordinate Dealers available in local DB
        /// </summary>
        /// <param name="accountId">SFDC AccountId</param>
        /// <returns>lstContact</returns>
        public static List<SFDCSoapClient.Contact> GetSubordinateDealers(string accountId)
        {
            List<SFDCSoapClient.Contact> lstContact = new List<SFDCSoapClient.Contact>();

            if (!string.IsNullOrEmpty(accountId))
            {
                try
                {
                    string query = string.Empty;
                    QueryResult queryResult = new QueryResult();
                    query = AppLogic.AppConfig("SFDCAccountDealersQuery").Replace(AppLogic.AppConfig("SFDCQueryParam"), accountId);

                    if (QuerySFDC(query, ref queryResult))
                    {
                        sObject[] contacts = queryResult.records;
                        foreach (sObject contact in contacts)
                        {
                            lstContact.Add((SFDCSoapClient.Contact)contact);
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

            return lstContact;
        }

    }
}