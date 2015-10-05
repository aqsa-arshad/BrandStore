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

namespace AspDotNetStorefront
{
    public static class AuthenticationSSO
    {
        private static Customer ThisCustomer;

        /// <summary>
        /// Initialize Customer Object after OKTA Authentication
        /// </summary>
        /// <param name="userName">userName</param>
        /// <param name="password">password</param>
        /// <returns>Status</returns>
        public static Customer InitializeCustomerObject(string userName, string password)
        {
            if (UserAuthentication(userName, password))
            {
                var userModel = GetUserModel(userName);

                if (userModel == null)
                    return new Customer(userName, true);

                if (!IsCustomerAvailable(userName))
                {
                    // Add New Customer in local DB
                    InsertCustomer(userName);
                }
                // Update Customer in local DB w.r.t Okta UserModel
                UpdateCustomer(userModel.profile, userName, password);
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
                var authorization = ConfigurationManager.AppSettings["Authorization"] + " " + ConfigurationManager.AppSettings["AccessToken"];
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["DefaultURL"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", authorization);

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
                var authorization = ConfigurationManager.AppSettings["Authorization"] + " " + ConfigurationManager.AppSettings["AccessToken"];
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["DefaultURL"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", authorization);

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
        /// Validate if userName is available in DB
        /// </summary>
        /// <param name="userName">userName</param>
        /// <returns>Status</returns>
        private static bool IsCustomerAvailable(string userName)
        {
            ThisCustomer = new Customer(userName);
            return ThisCustomer.IsRegistered;
        }

        /// <summary>
        /// Update Customer in local DB w.r.t Okta UserModel
        /// </summary>
        /// <param name="profile">profile</param>
        /// <param name="userName">userName</param>
        /// <param name="password">password</param>
        private static void UpdateCustomer(Profile profile, string userName, string password)
        {
            Password p = new Password(password);
            int customerLevelID = GetCustomerLevelID(profile.userType);

            ThisCustomer = new Customer(userName);
            SqlParameter[] sqlParameter = {
                                        new SqlParameter("@Password", p.SaltedPassword),
                                        new SqlParameter("@SaltKey", p.Salt),
                                        new SqlParameter("@SkinID", AppLogic.DefaultSkinID()), 
                                        new SqlParameter("@IsRegistered", 1), 
                                        new SqlParameter("@FirstName", profile.firstName),
                                        new SqlParameter("@LastName", profile.lastName),
                                        new SqlParameter("@Phone", profile.primaryPhone),
                                        new SqlParameter("@CustomerLevelID", customerLevelID),
                                        new SqlParameter("@BillingAddressID", AddUpdateAddress(profile, ThisCustomer.CustomerID, ThisCustomer.PrimaryBillingAddressID == null ? 0 : ThisCustomer.PrimaryBillingAddressID)),
                                        new SqlParameter("@ShippingAddressID", AddUpdateAddress(profile, ThisCustomer.CustomerID, ThisCustomer.PrimaryShippingAddressID == null ? 0 : ThisCustomer.PrimaryShippingAddressID)),
                                        new SqlParameter("@IsAdmin", 0)
                                       };

            ThisCustomer.UpdateCustomer(sqlParameter);
        }

        /// <summary>
        /// Add New Customer in local DB
        /// </summary>
        /// <param name="userName">userName</param>
        private static void InsertCustomer(string userName)
        {
            Customer.CreateCustomerRecord(userName, null, ThisCustomer.SkinID,
                        null, null, 0, null, ThisCustomer.LocaleSetting, 0, ThisCustomer.CurrencySetting,
                        ThisCustomer.VATSettingRAW, ThisCustomer.VATRegistrationID, 0);
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

            return anyAddress.AddressID; ;
        }
    
        /// <summary>
        /// Get CustomerLevelID w.r.t Okta UserType
        /// </summary>
        /// <param name="userType">userType</param>
        /// <returns>CustomerLevelID</returns>
        private static int GetCustomerLevelID(string userType)
        {
            string enumUserType = userType.Replace(" ", "").ToUpper();
            if (Enum.IsDefined(typeof(UserType), enumUserType))
            {
                return (int)Enum.Parse(typeof(UserType), enumUserType);
            }
            return (int)UserType.PUBLIC;
        }

        /// <summary>
        /// Send Email to User for Change Password Request
        /// </summary>
        /// <param name="userName">userName</param>
        /// <returns>Status</returns>
        public static bool ChangePasswordRequest(string UserModelID)
        {
            using (var client = new HttpClient())
            {
                var url = ConfigurationManager.AppSettings["UserURL"] + UserModelID + ConfigurationManager.AppSettings["ResetPassword"];
                var authorization = ConfigurationManager.AppSettings["Authorization"] + " " + ConfigurationManager.AppSettings["AccessToken"];
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["DefaultURL"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", authorization);

                return client.PostAsync(url, new StringContent("")).Result.IsSuccessStatusCode;
            }
        }
    
    }
}