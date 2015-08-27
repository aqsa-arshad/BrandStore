// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AspDotNetStorefrontBuySafe
{
    public class BuySafeController
    {
        private static readonly string SandboxPOSTURL = "https://sbws.buysafe.com/BuySafeWS/RegistrationAPI.dll";
        private static readonly string ProductionPOSTURL = "https://api.buysafe.com/BuySafeWS/RegistrationAPI.dll";

        private static readonly string LiveMSPUsername = "Vortx";
        private static readonly string LiveMSPPassword = "4EF7DEEB-4F1A-421F-AFD9-8BEC39AD34E4";
        private static readonly string LivePromoCode = "aspdotnetstorefront";
        private static readonly string SandboxMSPUsername = "Vortx";
        private static readonly string SandboxMSPPassword = "D8A75188-147D-4165-83D4-003DB5C869C2";
        private static readonly string SandboxPromoCode = "Vortx_2011_03";

        private static Boolean UseSandbox
        {
            get
            {
                return AppLogic.AppConfigBool("BuySafe.UseSandbox");
            }
        }


        public static BuySafeStatus RegisterMerchant(
            String MerchantUserName,
            String MerchantFirstName,
            String MerchantLastName,
            String MerchantPhone,
            String MerchantEMail,
            String MerchantCompany,
            String MerchantAddress1,
            String MerchantAddress2,
            String MerchantCity,
            String MerchantState,
            String MerchantZip,
            String MerchantCountry,
            ResidenceTypes MerchantResidenceType,

            DateTime MinOrderDate,
            List<Store> Stores
        )
        {
            BuySafeWebReference.BuySafeAPI api = GetAPI();

            BuySafeWebReference.AddAccountRQ reqAccount = new BuySafeWebReference.AddAccountRQ();

            reqAccount.AddStoreList = new BuySafeWebReference.StoreInformation[Stores.Count];
            for (int i = 0; i < Stores.Count; i++)
                reqAccount.AddStoreList[i] = GetStoreInformation(Stores[i]);


            reqAccount.BusinessInformation = new BuySafeWebReference.BusinessInformation();

            BuySafeWebReference.Address a = new BuySafeWebReference.Address();
            a.CityName = MerchantCity;
            a.Country = MerchantCountry;
            a.CountryCode = BuySafeWebReference.CountryCode.US;
            a.PostalCode = MerchantZip;
            a.StateProvince = MerchantState;
            a.StreetLine1 = MerchantAddress1;
            a.StreetLine2 = MerchantAddress2;
            switch (MerchantResidenceType)
            {
                case ResidenceTypes.Residential:
                    a.Type = BuySafeWebReference.AddressType.Home;
                    break;
                case ResidenceTypes.Commercial:
                case ResidenceTypes.Unknown:
                default:
                    a.Type = BuySafeWebReference.AddressType.Business;
                    break;
            }
            a.Type = BuySafeWebReference.AddressType.Business;

            reqAccount.BusinessInformation.Address = a;
            reqAccount.BusinessInformation.CompanyName = MerchantCompany;
            if (UseSandbox)
                reqAccount.BusinessInformation.PromoCode = SandboxPromoCode;
            else
                reqAccount.BusinessInformation.PromoCode = LivePromoCode;
            BuySafeWebReference.CustomDateTime sellingTime = new BuySafeWebReference.CustomDateTime();
            sellingTime.DateTimeValue = MinOrderDate;
            sellingTime.HasDateTime = true;
            reqAccount.BusinessInformation.DateBeganSelling = sellingTime;
            reqAccount.BusinessInformation.Email = MerchantEMail;
            reqAccount.BusinessInformation.FirstName = MerchantFirstName;
            reqAccount.BusinessInformation.LastName = MerchantLastName;
            BuySafeWebReference.Phone p = new BuySafeWebReference.Phone();
            p.Number = MerchantPhone;
            p.Type = BuySafeWebReference.PhoneType.Business;
            reqAccount.BusinessInformation.Phone = p;

            BuySafeWebReference.LoginInformation creds = new BuySafeWebReference.LoginInformation();
            creds.AutoGenerateOnCollision = false;
            creds.UserName = MerchantUserName;
            reqAccount.LoginInformation = creds;

            BuySafeWebReference.AddAccountRS response = api.AddAccount(reqAccount);
            BuySafeStatus status = new BuySafeStatus();
            status.HasError = !response.Successful;
            if (status.HasError)
                status.ErrorMessage = response.ErrorInformation.ToString();
            else
                status.Hash = response.AccountResponse.Hash;
            status.UserName = MerchantUserName;
            return status;
        }

        private static BuySafeWebReference.StoreInformation GetStoreInformation(Store s)
        {
            Random r = new Random();
            int randomint = r.Next(0, 10000);

            BuySafeWebReference.StoreInformation newstore;
            newstore = new BuySafeWebReference.StoreInformation();
            newstore.Name = s.ProductionURI;
            if (AppLogic.GlobalConfigBool("BuySafe.AppendRandomNumberToStoreNames"))
                newstore.Name += randomint.ToString();
            newstore.Url = GetFullURL(s.ProductionURI);
            String bsgms = AppLogic.GlobalConfig("BuySafe.GMS");
            newstore.StoreMonthlyGMS = Localization.ParseNativeDouble(bsgms);
            return newstore;
            //store1.CheckOutUrl = "http://store2/aspdotnetstorefrontmultistore9/checkout.aspx"; //optional
            //store1.TermsOfSale = "terms"; //optional
            //store1.Email = "jesse@vortx.com"; //optional
        }

        private static BuySafeWebReference.BuySafeAPI GetAPI()
        {
            BuySafeWebReference.BuySafeAPI api;
            api = new BuySafeWebReference.BuySafeAPI();
            api.MerchantServiceProviderCredentialsValue = new BuySafeWebReference.MerchantServiceProviderCredentials();
            if (UseSandbox)
            {
                api.Url = SandboxPOSTURL;
                if (!String.IsNullOrEmpty(AppLogic.GlobalConfig("BuySafe.SandBoxEndPoint")))
                    api.Url = AppLogic.GlobalConfig("BuySafe.SandBoxEndPoint");
                api.MerchantServiceProviderCredentialsValue.UserName = SandboxMSPUsername;
                api.MerchantServiceProviderCredentialsValue.Password = SandboxMSPPassword;
            }
            else
            {
                api.Url = ProductionPOSTURL;
                if (!String.IsNullOrEmpty(AppLogic.GlobalConfig("BuySafe.EndPoint")))
                    api.Url = AppLogic.GlobalConfig("BuySafe.EndPoint");
                api.MerchantServiceProviderCredentialsValue.UserName = LiveMSPUsername;
                api.MerchantServiceProviderCredentialsValue.Password = LiveMSPPassword;
            }

            api.BuySafeWSHeaderValue = new BuySafeWebReference.BuySafeWSHeader();
            api.BuySafeWSHeaderValue.Version = "610";
            return api;
        }

        public static Boolean AccountIsActive()
        {
            try
            {
                BuySafeWebReference.AccountResponse accountResponse = GetAccountRS().AccountResponse;
                return accountResponse.Status == BuySafeWebReference.AccountResponseStatus.Active;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Boolean IsWorking()
        {
            //short circuit if not enabled
            return IsEnabled() && AccountIsActive();
        }

        public static int DaysRemainingOnTrial()
        {
            DateTime regDate = Localization.ParseNativeDateTime(AppLogic.GlobalConfig("BuySafe.TrialStartDate"));
            if (regDate == DateTime.MinValue)
            {
                return -1;
            }

            int daysInTrial = regDate.AddDays(30).Subtract(DateTime.Now).Days;
            if (daysInTrial < 0)
            {
                daysInTrial = 0;
            }
            else if (daysInTrial > 30)
            {
                daysInTrial = 30;
            }
            return daysInTrial;
        }

        public static Boolean IsEnabled()
        {
            return AppLogic.GlobalConfigBool("BuySafe.Enabled");
        }

        public static BuySafeStoreStatuses GetStoreStatuses()
        {
            List<Store> registeredStores = new List<Store>();
            List<Store> unregisteredStores = new List<Store>();
            List<Store> allStores = Store.GetStoreList();

            BuySafeWebReference.GetAccountRS rs = GetAccountRS();
            if (rs.StoreResponseList == null)
	        {
                return new BuySafeStoreStatuses()
                {
                    RegisteredStores = new List<Store>(),
                    UnRegisteredStores = allStores
                };
	        }

            BuySafeWebReference.StoreResponse[] storeResponses = rs.StoreResponseList;

            foreach (Store currentStore in allStores)
            {
                if (storeResponses.FirstOrDefault(s => s.Url.ToLower() == GetFullURL(currentStore.ProductionURI).ToLower()) != null)
                    registeredStores.Add(currentStore);
                else
                    unregisteredStores.Add(currentStore);
            }

            return new BuySafeStoreStatuses(){
                RegisteredStores = registeredStores,
                UnRegisteredStores = unregisteredStores
            };
        }

        private static String GetFullURL(string StoreURI)
        {
            return "http://" + StoreURI + "/";
        }

        private static string AccountHash()
        {

            return HttpUtility.UrlDecode(AppLogic.GlobalConfig("BuySafe.Hash"));
        }

        private static BuySafeWebReference.GetAccountRS GetAccountRS()
        {
            try
            {
                BuySafeWebReference.BuySafeAPI api = GetAPI();
                BuySafeWebReference.GetAccountRQ accountRQ = new BuySafeWebReference.GetAccountRQ();
                accountRQ.AccountHash = AccountHash();
                return api.GetAccount(accountRQ);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static BuySafeStatus RegisterStore(Store store)
        {
            BuySafeWebReference.BuySafeAPI api = GetAPI();
            BuySafeWebReference.AddStoreRQ req = new BuySafeWebReference.AddStoreRQ();
            req.AccountHash = AccountHash();
            req.AddStoreList = new BuySafeWebReference.StoreInformation[1];
            req.AddStoreList[0] = GetStoreInformation(store);

            BuySafeWebReference.AddStoreRS response = api.AddStore(req);
            BuySafeStatus status = new BuySafeStatus()
            {
                HasError = response.Successful == false
            };
            if (response.ErrorInformation != null)
            {
                status.ErrorMessage = response.ErrorInformation.ToString();
            }
            return status;
        }

        public static BuySafeRegistrationStatus BuySafeOneClickSignup()
        {
            String email = GetEmailAddress();
            if (String.IsNullOrEmpty(email))
            {
                return new BuySafeRegistrationStatus()
                {
                    ErrorMessage = "No email addresses could be found. Please configure your email before signing up for buySAFE.",
                    Sucessful = false
                };
            }

            List<Store> stores = Store.GetStoreList(true);
            Store defaultStore = GetDefaultStore(stores);

            if (defaultStore == null)
            {
                return new BuySafeRegistrationStatus()
                {
                    ErrorMessage = "No valid stores have been setup. Please setup a store before signing up for buySAFE.",
                    Sucessful = false
                };
            }

            Double gms = 1000;

            AppLogic.SetGlobalConfig("BuySafe.GMS", gms.ToString());

            DateTime minOrderDate = DateTime.Now;
            using (SqlConnection dbconn5 = DB.dbConn())
            {
                dbconn5.Open();
                using (IDataReader rscc = DB.GetRS("select min(capturedon) from orders", dbconn5))
                {
                    if (rscc.Read())
                        minOrderDate = DB.RSFieldDateTime(rscc, "caputredon");
                }
            }

            if (minOrderDate == DateTime.MinValue)
                minOrderDate = DateTime.Now.AddDays(-1);


            //register with buysafe
            BuySafeStatus status = BuySafeController.RegisterMerchant(
                defaultStore.ProductionURI,             //username
                "AspDotNetStorefront",  //first name
                "Merchant",             //last name
                "5555555555",           //phone
                email,                  //email
                defaultStore.ProductionURI,      //Company Name
                "Address1",                     //address 1
                "Address2",                     //address 2
                "City",                     //city
                "Oregon",                     //state
                "12345",                     //zip
                "US",                     //country
                ResidenceTypes.Commercial, //residence type
                minOrderDate,
                stores
            );

            if (status.HasError)
            {
                SysLog.LogMessage("buySafe registration failed.", status.ErrorMessage, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                return new BuySafeRegistrationStatus()
                {
                    Sucessful = false
                };
            }
            AppLogic.SetGlobalConfig("BuySafe.Enabled", "true");
            AppLogic.SetGlobalConfig("BuySafe.UserName", status.UserName);
            AppLogic.SetGlobalConfig("BuySafe.Hash", HttpUtility.UrlEncode(status.Hash));
            AppLogic.SetGlobalConfig("BuySafe.TrialStartDate", DateTime.Now.ToShortDateString());
            AppLogic.m_RestartApp();
            return new BuySafeRegistrationStatus()
            {
                Sucessful = true
            };
        }

        public static Store GetDefaultStore(List<Store> stores)
        {
            string DefaultProduction = "www.mystore.com";
            foreach (Store s in stores)
            {
                if (!string.IsNullOrEmpty(s.ProductionURI) && s.ProductionURI.ToLower() != DefaultProduction && s.ProductionURI.Contains('.'))
                    return s;
            }
            return null;
        }

        public static string GetEmailAddress()
        {
            String DefaultAddress = "sales@yourdomain.com";
            String[] AppConfigsToCheck = { "MailMe_ToAddress", "GotOrderEMailTo", "MailMe_FromAddress", "GotOrderEMailFrom", "ReceiptEMailFrom" };
            for (int i = 0; i < AppConfigsToCheck.Length; i++)
            {
                string defaultAppConfigAddress = AppLogic.AppConfig(AppConfigsToCheck[i], 0, false);
                if (!string.IsNullOrEmpty(defaultAppConfigAddress) && defaultAppConfigAddress.ToLower() != DefaultAddress)
                    return defaultAppConfigAddress;

                foreach (Store s in Store.GetStoreList())
                {
                    string address = AppLogic.AppConfig(AppConfigsToCheck[i], s.StoreID, false);
                    if (!String.IsNullOrEmpty(address) && address.ToLower() != DefaultAddress)
                        return address;
                }
            }
            return null;
        }

        public static BuySafeState GetBuySafeState()
        {
            Boolean isEnabled = BuySafeController.IsEnabled();
            Boolean freeTrialAvailable = BuySafeController.DaysRemainingOnTrial() > 0 || BuySafeController.DaysRemainingOnTrial() == -1;
            Boolean isWorking = BuySafeController.IsWorking();//also checks AccountIsActive
            if (!isEnabled)
            {
                if (freeTrialAvailable)
                    return BuySafeState.NotEnabledFreeTrialAvailable;
                else
                    return BuySafeState.NotEnabledFreeTrialExpired;
            }
            else if (isWorking) // isEnabled && isWorking
            {
                if (freeTrialAvailable)
                    return BuySafeState.EnabledOnFreeTrial;
                else
                    return BuySafeState.EnabledFullUserAfterFreeTrial;
            }
            else //isEnabled && !working
            {
                if (!freeTrialAvailable)
                    return BuySafeState.EnabledFreeTrialExpired;
            }
            return BuySafeState.Error;
        }
    }

    public enum BuySafeState
    {
        NotEnabledFreeTrialAvailable,
        NotEnabledFreeTrialExpired,
        EnabledOnFreeTrial,
        EnabledFreeTrialExpired,
        EnabledFullUserAfterFreeTrial,
        Error
    }

    public class BuySafeStatus
    {
        public Boolean HasError
        { get;set; }
        public String ErrorMessage
        { get; set; }
        public String UserName
        { get; set; }
        public String Hash
        { get; set; }
        public BuySafeStatus()
        { }
    }

    public class BuySafeStoreStatuses
    {
        public List<Store> RegisteredStores
        { get; set; }
        public List<Store> UnRegisteredStores
        { get; set; }
        public BuySafeStoreStatuses() { }
    }

    public class BuySafeRegistrationStatus
    {
        public String ErrorMessage { get; set; }
        public Boolean Sucessful { get; set; }
    }
}
