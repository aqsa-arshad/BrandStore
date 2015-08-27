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
using AspDotNetStorefrontGateways.SecureNetDataAPIv411;
using AspDotNetStorefrontCore;
using System.ServiceModel;

namespace AspDotNetStorefrontGateways
{
    public class SecureNetDataReport
    {
        private static MERCHANT_KEY MerchantKey
        {
            get 
            {
                MERCHANT_KEY key = new MERCHANT_KEY();
                key.SECURENETID = AppLogic.AppConfigNativeInt("SecureNet.ID");
                key.SECUREKEY = AppLogic.AppConfig("SecureNet.Key");
                return key;
            }
        }

        private static SecureNetDataAPIv411.SERVICEClient ServiceClient
        {
            get
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                EndpointAddress endpoint = new EndpointAddress(CommonLogic.IIF(AppLogic.AppConfigBool("UseLiveTransactions"), AppLogic.AppConfig("SecureNetV4.DataAPI.LiveURL"), AppLogic.AppConfig("SecureNetV4.DataAPI.TestURL")));
                return new SERVICEClient(binding, endpoint);
            }
        }

        public static CustomerVault GetCustomerVault(int CustomerId)
        {
            try
            {

                SERVICEClient sc = ServiceClient;
                return new CustomerVault(sc.GetVaultAccountByCustomer(MerchantKey, CustomerId.ToString()));
            }
            catch
            {
                return null;
            }
        }
    }

    public class CustomerVault
    {
        public List<CustomerVaultPayment> SavedPayments { get; protected set; }

        public CustomerVault(ACCOUNT_VAULT[] AccountVaults)
        {
            SavedPayments = new List<CustomerVaultPayment>();
            foreach (ACCOUNT_VAULT av in AccountVaults)
                if (av.PAYMENTID != null)
                    SavedPayments.Add(new CustomerVaultPayment(av));
        }
    }

    public class CustomerVaultPayment
    {
        public String PaymentId { get; protected set; }
        public String Method { get; protected set; }
        public String CardCode { get; protected set; }
        public String CardNumber { get; protected set; }
        public String CardNumberPadded
        {
            get
            {
                return CardNumber.PadLeft(16, '*');
            }
        }
        public String ExpDateFormatted
        {
            get
            {
                return ExpDate.Insert(2, "/20");
            }
        }

        public String ExpDate { get; protected set; }

        public CustomerVaultPayment(ACCOUNT_VAULT AccountVault)
        {
            this.PaymentId = AccountVault.PAYMENTID;
            this.Method = AccountVault.METHOD;
            if (AccountVault.CARD != null)
            {
                this.CardCode = AccountVault.CARD.CARDCODE;
                this.CardNumber = AccountVault.CARD.CARDNUMBER;
                this.ExpDate = AccountVault.CARD.EXPDATE;
            }
        }
    }
}
