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
using Salesforce.Common;
using Salesforce.Force;
using Newtonsoft.Json.Linq;
using System.Runtime;

namespace AspDotNetStorefront
{
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
            finally
            { 
             
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
                var url = ConfigurationManager.AppSettings["SessionURL"];
                try
                {
                    var authorization = ConfigurationManager.AppSettings["Authorization"] + " " + ConfigurationManager.AppSettings["AccessToken"];
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["DefaultURL"]);
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
                var url = ConfigurationManager.AppSettings["UserURL"] + userName;
                try
                {

                    var authorization = ConfigurationManager.AppSettings["Authorization"] + " " + ConfigurationManager.AppSettings["AccessToken"];
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["DefaultURL"]);
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
        private static void UpdateCustomer(Profile profile, string userName, string password, bool IsCustomerAvailable)
        {
            try
            {
                SFDCDealerUser dealerUser = new SFDCDealerUser();
                SFDCInternalUser internalUser = new SFDCInternalUser();

                if (!string.IsNullOrEmpty(profile.sfid))
                    GetSFDCDealerUser(profile, profile.sfid);
                else
                    GetSFDCInternalUser(profile, userName);

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
                                        // Address will be syncedd with user salesforce account
                                        //new SqlParameter("@BillingAddressID", AddUpdateAddress(profile, ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID == null ? 0 : ThisCustomer.PrimaryBillingAddressID)),
                                        //new SqlParameter("@ShippingAddressID", AddUpdateAddress(profile, ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID == null ? 0 : ThisCustomer.PrimaryShippingAddressID)),
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
        private static int AddUpdateAddress(Profile profile, int CustomerID, int AddressID)
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
            return (int)UserType.PUBLIC;
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
                var url = ConfigurationManager.AppSettings["UserURL"] + UserModelID + ConfigurationManager.AppSettings["ResetPassword"];
                try
                {
                    var authorization = ConfigurationManager.AppSettings["Authorization"] + " " + ConfigurationManager.AppSettings["AccessToken"];
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["DefaultURL"]);
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

        public static void GetSFDCDealerUser(Profile profile, string sfid)
        {
            SFDCDealerUser _dealerUser = new SFDCDealerUser();
            try
            {
                var dealerUserQuery = ConfigurationManager.AppSettings["SFDCDealerUserQuery"].Replace(ConfigurationManager.AppSettings["SFDCQueryParam"], sfid);
                var consumerkey = ConfigurationManager.AppSettings["SFDCConsumerkey"];
                var consumersecret = ConfigurationManager.AppSettings["SFDCConsumersecret"];
                var username = ConfigurationManager.AppSettings["SFDCUsername"];
                var password = ConfigurationManager.AppSettings["SFDCPassword"];
                var endPointURL = ConfigurationManager.AppSettings["SFDCEndPointURL"];

                //create auth client to retrieve token
                var auth = new AuthenticationClient();

                //get back URL and token
                auth.UsernamePasswordAsync(consumerkey, consumersecret, username, password, endPointURL).Wait();

                var instanceUrl = auth.InstanceUrl;
                var accessToken = auth.AccessToken;
                var apiVersion = auth.ApiVersion;

                var client = new ForceClient(instanceUrl, accessToken, apiVersion);
                var dealerUser = client.QueryAsync<dynamic>(dealerUserQuery);
                List<string> lstDealerUser = new List<string>();

                foreach (JProperty item in dealerUser.Result.Records.FirstOrDefault())
                {
                    if (item.Name.ToString().ToLower() == "attributes")
                        continue;

                    if (item.Name.ToString().ToLower() == "account")
                    {
                        foreach (JProperty itemaccount in item.Value)
                        {
                            if (itemaccount.Name.ToString().ToLower() == "attributes")
                                continue;

                            lstDealerUser.Add(itemaccount.Value.ToString());
                        }
                    }
                    else
                    {
                        lstDealerUser.Add(item.Value.ToString());
                    }
                }

                _dealerUser.Customer_Number__c = string.IsNullOrEmpty(lstDealerUser[0]) ? string.Empty : lstDealerUser[0];
                _dealerUser.TrueBLUStatus__c = string.IsNullOrEmpty(lstDealerUser[1]) ? string.Empty : lstDealerUser[1];
                _dealerUser.Region__c = string.IsNullOrEmpty(lstDealerUser[2]) ? string.Empty : lstDealerUser[2];
                _dealerUser.Co_op_budget__c = string.IsNullOrEmpty(lstDealerUser[3]) ? string.Empty : lstDealerUser[3];
                _dealerUser.Display_Funds__c = string.IsNullOrEmpty(lstDealerUser[4]) ? string.Empty : lstDealerUser[4];
                _dealerUser.Literature_Funds__c = string.IsNullOrEmpty(lstDealerUser[5]) ? string.Empty : lstDealerUser[5];
                _dealerUser.POP_Funds__c = string.IsNullOrEmpty(lstDealerUser[6]) ? string.Empty : lstDealerUser[6];
                _dealerUser.Direct_Marketing_Funds__c = string.IsNullOrEmpty(lstDealerUser[7]) ? string.Empty : lstDealerUser[7];
                _dealerUser.ID = string.IsNullOrEmpty(lstDealerUser[8]) ? string.Empty : lstDealerUser[8];
                _dealerUser.Name = string.IsNullOrEmpty(lstDealerUser[9]) ? string.Empty : lstDealerUser[9];
                _dealerUser.FirstName = string.IsNullOrEmpty(lstDealerUser[10]) ? string.Empty : lstDealerUser[10];
                _dealerUser.LastName = string.IsNullOrEmpty(lstDealerUser[11]) ? string.Empty : lstDealerUser[11];
                _dealerUser.Email = string.IsNullOrEmpty(lstDealerUser[12]) ? string.Empty : lstDealerUser[12];
                _dealerUser.Contact_Roles__c = string.IsNullOrEmpty(lstDealerUser[13]) ? string.Empty : lstDealerUser[13];

                profile.firstName = _dealerUser.FirstName;
                profile.lastName = _dealerUser.LastName;

                if (_dealerUser.TrueBLUStatus__c.Equals("ELITE", StringComparison.InvariantCultureIgnoreCase) ||
                        _dealerUser.TrueBLUStatus__c.Equals("PREMIER", StringComparison.InvariantCultureIgnoreCase) ||
                        _dealerUser.TrueBLUStatus__c.Equals("AUTHORIZED", StringComparison.InvariantCultureIgnoreCase))
                    profile.userType = "BLU" + _dealerUser.TrueBLUStatus__c;
                else
                    profile.userType = _dealerUser.TrueBLUStatus__c;
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }

        public static void GetSFDCInternalUser(Profile profile, string email)
        {
            SFDCInternalUser _internalUser = new SFDCInternalUser();
            try
            {
                var dealerUserQuery = ConfigurationManager.AppSettings["SFDCInternalUserQuery"].Replace(ConfigurationManager.AppSettings["SFDCQueryParam"], email);
                var consumerkey = ConfigurationManager.AppSettings["SFDCConsumerkey"];
                var consumersecret = ConfigurationManager.AppSettings["SFDCConsumersecret"];
                var username = ConfigurationManager.AppSettings["SFDCUsername"];
                var password = ConfigurationManager.AppSettings["SFDCPassword"];
                var endPointURL = ConfigurationManager.AppSettings["SFDCEndPointURL"];

                //create auth client to retrieve token
                var auth = new AuthenticationClient();

                //get back URL and token
                auth.UsernamePasswordAsync(consumerkey, consumersecret, username, password, endPointURL).Wait();

                var instanceUrl = auth.InstanceUrl;
                var accessToken = auth.AccessToken;
                var apiVersion = auth.ApiVersion;

                var client = new ForceClient(instanceUrl, accessToken, apiVersion);
                var internalUser = client.QueryAsync<dynamic>(dealerUserQuery);
                List<string> lstInternalUser = new List<string>();

                foreach (JProperty item in internalUser.Result.Records.FirstOrDefault())
                {
                    if (item.Name.ToString().ToLower() == "attributes")
                        continue;

                    lstInternalUser.Add(item.Value.ToString());
                }

                _internalUser.Id = string.IsNullOrEmpty(lstInternalUser[0]) ? string.Empty : lstInternalUser[0];
                _internalUser.Name = string.IsNullOrEmpty(lstInternalUser[1]) ? string.Empty : lstInternalUser[1];
                _internalUser.FirstName = string.IsNullOrEmpty(lstInternalUser[2]) ? string.Empty : lstInternalUser[2];
                _internalUser.LastName = string.IsNullOrEmpty(lstInternalUser[3]) ? string.Empty : lstInternalUser[3];
                _internalUser.Email = string.IsNullOrEmpty(lstInternalUser[4]) ? string.Empty : lstInternalUser[4];
                _internalUser.IsActive = string.IsNullOrEmpty(lstInternalUser[5]) ? string.Empty : lstInternalUser[5];
                _internalUser.Sales_Rep_ID__c = string.IsNullOrEmpty(lstInternalUser[6]) ? string.Empty : lstInternalUser[6];
                _internalUser.SOF__c = string.IsNullOrEmpty(lstInternalUser[7]) ? string.Empty : lstInternalUser[7];
                _internalUser.Billing_GL__c = string.IsNullOrEmpty(lstInternalUser[8]) ? string.Empty : lstInternalUser[8];

                profile.firstName = _internalUser.FirstName;
                profile.lastName = _internalUser.LastName;
                profile.userType = UserType.SALESREPS.ToString();
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
        }
    }
}