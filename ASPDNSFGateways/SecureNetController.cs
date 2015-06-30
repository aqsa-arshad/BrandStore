// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Text;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using AspDotNetStorefrontGateways.SecureNetAPIv411;
using System.ServiceModel;
using System.Collections.Generic;
using AspDotNetStorefrontGateways;

namespace AspDotNetStorefrontGateways
{
    public static class SecureNetController
    {
        public static MERCHANT_KEY GetMerchantKey()
        {
            MERCHANT_KEY key = new MERCHANT_KEY();
            key.SECURENETID = AppLogic.AppConfigNativeInt("SecureNet.ID");
            key.SECUREKEY = AppLogic.AppConfig("SecureNet.Key");
            return key;
        }

        public static GatewayClient GetGatewayClient()
        {
            BasicHttpBinding binding;
            EndpointAddress endpoint = new EndpointAddress(AppLogic.AppConfigBool("UseLiveTransactions") ? AppLogic.AppConfig("SecureNetV4.LiveURL") : AppLogic.AppConfig("SecureNetV4.TestURL"));

            if (AppLogic.AppConfigBool("UseLiveTransactions"))
                binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            else
                binding = new BasicHttpBinding();            
            
            return new GatewayClient(binding, endpoint);
        }

        public static String GetTypeCodeString(SecureNetTransactionTypeCode code)
        {
            switch (code)
            {
                case SecureNetTransactionTypeCode.AUTH_ONLY:
                    return "0000";
                case SecureNetTransactionTypeCode.PARTIAL_AUTH_ONLY:
                    return "0001";
                case SecureNetTransactionTypeCode.AUTH_CAPTURE:
                    return "0100";
                case SecureNetTransactionTypeCode.PARTIAL_AUTH_CAPTURE:
                    return "0101";
                case SecureNetTransactionTypeCode.PRIOR_AUTH_CAPTURE:
                    return "0200";
                case SecureNetTransactionTypeCode.CAPTURE_ONLY:
                    return "0300";
                case SecureNetTransactionTypeCode.VOID:
                    return "0400";
                case SecureNetTransactionTypeCode.PARTIAL_VOID:
                    return "0401";
                case SecureNetTransactionTypeCode.VOID_BY_ORDERID:
                    return "0402";
                case SecureNetTransactionTypeCode.CREDIT:
                    return "0500";
                case SecureNetTransactionTypeCode.CREDIT_AUTHONLY:
                    return "0501";
                case SecureNetTransactionTypeCode.CREDIT_PRIORAUTHCAPTURE:
                    return "0502";
                case SecureNetTransactionTypeCode.FORCE_CREDIT:
                    return "0600";
                case SecureNetTransactionTypeCode.FORCE_CREDIT_AUTHONLY:
                    return "0601";
                case SecureNetTransactionTypeCode.FORCE_CREDIT_PRIORAUTHCAPTURE:
                    return "0602";
                case SecureNetTransactionTypeCode.VERIFICATION:
                    return "0700";
                case SecureNetTransactionTypeCode.AUTH_INCREMENT:
                    return "0800";
                case SecureNetTransactionTypeCode.ISSUE:
                    return "0900";
                case SecureNetTransactionTypeCode.ACTIVATE:
                    return "0901";
                case SecureNetTransactionTypeCode.REDEEM:
                    return "0902";
                case SecureNetTransactionTypeCode.REDEEM_PARTIAL:
                    return "0903";
                case SecureNetTransactionTypeCode.DEACTIVATE:
                    return "0904";
                case SecureNetTransactionTypeCode.REACTIVATE:
                    return "0905";
                case SecureNetTransactionTypeCode.INQUERY_BALANCE:
                    return "0906";
                default:
                    return "";
            }
        }

        public static TRANSACTION GetTransactionWithDefaults()
        {
            TRANSACTION oT = new TRANSACTION();
            oT.MERCHANT_KEY = GetMerchantKey();
            oT.TEST = AppLogic.AppConfigBool("SecureNetV4.UseTestMode").ToString().ToUpper();
            return oT;
        }

        public static TRANSACTION_VAULT GetVaultTransactionWithDefaults()
        {
            TRANSACTION_VAULT tv = new TRANSACTION_VAULT();
            tv.MERCHANT_KEY = GetMerchantKey();
            return tv;
        }
    }

    public enum SecureNetTransactionTypeCode
    {
        AUTH_ONLY,
        PARTIAL_AUTH_ONLY,
        AUTH_CAPTURE,
        PARTIAL_AUTH_CAPTURE,
        PRIOR_AUTH_CAPTURE,
        CAPTURE_ONLY,
        VOID,
        PARTIAL_VOID,
        VOID_BY_ORDERID,
        CREDIT,
        CREDIT_AUTHONLY,
        CREDIT_PRIORAUTHCAPTURE,
        FORCE_CREDIT,
        FORCE_CREDIT_AUTHONLY,
        FORCE_CREDIT_PRIORAUTHCAPTURE,
        VERIFICATION,
        AUTH_INCREMENT,
        ISSUE,
        ACTIVATE,
        REDEEM,
        REDEEM_PARTIAL,
        DEACTIVATE,
        REACTIVATE,
        INQUERY_BALANCE
    }

    public enum SecureNetMethod
    {
        CC,
        DB,
        ECHECK,
        CHECK21,
        PD,
        SV,
        EBT
    }

    public class SecureNetVault
    {
        private GatewayClient gsClient;
        private Customer ThisCustomer;

        public SecureNetVault(Customer c)
        {
            gsClient = SecureNetController.GetGatewayClient();
            ThisCustomer = c;
        }

        public ServiceResponse AddCreditCardToCustomerVault(String CardHolderName, String CardNumber, String CardCCV, String CardType, String ExpMonth, String ExpYear)
        {
            String CleanedCardNumber = ParseCardInt(CardNumber);
            if (CleanedCardNumber.Length != 16)
                return new ServiceResponse(true, "Please enter a valid credit card number.");
            if (this.CardExists(CardNumber, ExpMonth, ExpYear))
            {
                return new ServiceResponse(true, "This card has already been saved.");
            }
            else
            {
                OPERATIONPARAMETERS operationParameters = new OPERATIONPARAMETERS();
                operationParameters.ACTIONCODE = 1;
                operationParameters.ADD_IF_DECLINED = 0;
                TRANSACTION_VAULT transactionVault = SecureNetController.GetVaultTransactionWithDefaults();
                transactionVault.TRANSACTION = SecureNetController.GetTransactionWithDefaults();
                transactionVault.TRANSACTION.TRANSACTION_SERVICE = 3;
                transactionVault.TRANSACTION.CUSTOMER_BILL = GetVaultAccountCustomerBill();
                transactionVault.TRANSACTION.CODE = SecureNetController.GetTypeCodeString(SecureNetTransactionTypeCode.VERIFICATION);
                transactionVault.ACCOUNT_VAULT = GetAccountVault(ThisCustomer, CardHolderName, CardNumber, CardCCV, CardType, ExpMonth, ExpYear);
                transactionVault.CUSTOMER_VAULT = GetCustomerVault();
                transactionVault.OPERATIONPARAMETERS = operationParameters;
                GATEWAYRESPONSE gatewayResponse = gsClient.ProcessCustomerAndAccount(transactionVault);

                if (gatewayResponse.VAULTACCOUNTRESPONSE.RESPONSE_CODE != "1")
                    return new ServiceResponse(true, gatewayResponse.VAULTACCOUNTRESPONSE.RESPONSE_REASON_TEXT, gatewayResponse.VAULTACCOUNTRESPONSE.RESPONSE_REASON_CODE);

                if (gatewayResponse.VAULTCUSTOMERRESPONSE.RESPONSE_CODE != "1")
                    return new ServiceResponse(true, gatewayResponse.VAULTCUSTOMERRESPONSE.RESPONSE_REASON_TEXT, gatewayResponse.VAULTCUSTOMERRESPONSE.RESPONSE_REASON_CODE);

                return new ServiceResponse(false, "Payment Account Added");
            }
        }

        private bool CardExists(string CardNumber, string ExpMonth, string ExpYear)
        {
            if (CardNumber.Length > 4)
                CardNumber = CardNumber.Substring(CardNumber.Length - 4, 4);
            if (ExpYear.Length > 2)
                ExpYear = ExpYear.Substring(ExpYear.Length - 2, 2);
            CustomerVault v = SecureNetDataReport.GetCustomerVault(ThisCustomer.CustomerID);
            foreach (CustomerVaultPayment vp in v.SavedPayments)
                if (vp.CardNumber == CardNumber && vp.ExpDate.StartsWith(ExpMonth) && vp.ExpDate.EndsWith(ExpYear))
                    return true;
            return false;
        }

        public ServiceResponse DeletePaymentMethod(String VaultPaymentId)
        {
            OPERATIONPARAMETERS operationParameters = new OPERATIONPARAMETERS();
            operationParameters.ACTIONCODE = 3;
            TRANSACTION_VAULT transactionVault = SecureNetController.GetVaultTransactionWithDefaults();
            transactionVault.ACCOUNT_VAULT = new ACCOUNT_VAULT();
            transactionVault.ACCOUNT_VAULT.CUSTOMERID = ThisCustomer.CustomerID.ToString();
            transactionVault.ACCOUNT_VAULT.PAYMENTID = VaultPaymentId;
            transactionVault.OPERATIONPARAMETERS = operationParameters;
            GATEWAYRESPONSE gatewayResponse = gsClient.ProcessAccount(transactionVault);

            if (gatewayResponse.VAULTACCOUNTRESPONSE.RESPONSE_CODE != "1")
                return new ServiceResponse(true, gatewayResponse.VAULTACCOUNTRESPONSE.RESPONSE_REASON_TEXT, gatewayResponse.VAULTACCOUNTRESPONSE.RESPONSE_REASON_CODE);

            return new ServiceResponse(false, "Payment Account Added");
        }

        private String ParseCardInt(string CardNumber)
        {
            String allowed = "1234567890";
            String ret = "";
            foreach (char c in CardNumber)
            {
                if (allowed.Contains(c.ToString()))
                    ret += c;
            }
            return ret;
        }

        private ACCOUNT_VAULT GetAccountVault(Customer ThisCustomer, string CardHolderName, string CardNumber, string CardCCV, string CardType, string ExpMonth, string ExpYear)
        {
            ACCOUNT_VAULT accountVault = new ACCOUNT_VAULT();
            accountVault.ACDI = 0; // other values 1,2 0r 3
            accountVault.CARD = new CARD();
            accountVault.CARD.CARDNUMBER = CardNumber;
            accountVault.CARD.EXPDATE = ExpMonth.PadLeft(2, '0') + ExpYear.Substring(2, 2); //MMYY
            accountVault.CUSTOMERID = ThisCustomer.CustomerID.ToString();
            accountVault.PAYMENTID = "AUTO";
            accountVault.METHOD = "CC";
            return accountVault;
        }

        private CUSTOMER_VAULT GetCustomerVault()
        {
            CUSTOMER_VAULT customerVault = new CUSTOMER_VAULT();
            customerVault.CSDI = 1; //do not error and skip adding customer if they already exist.
            customerVault.CUSTOMERID = ThisCustomer.CustomerID.ToString();
            customerVault.CUSTOMER_BILL = GetVaultAccountCustomerBill();
            return customerVault;
        }

        private CUSTOMER_BILL GetVaultAccountCustomerBill()
        {
            CUSTOMER_BILL CustomerBill = new CUSTOMER_BILL();
            CustomerBill.ADDRESS = ThisCustomer.PrimaryBillingAddress.Address1;
            CustomerBill.CITY = ThisCustomer.PrimaryBillingAddress.City;
            CustomerBill.COMPANY = ThisCustomer.PrimaryBillingAddress.Company;
            CustomerBill.COUNTRY = ThisCustomer.PrimaryBillingAddress.Country;
            CustomerBill.EMAIL = ThisCustomer.PrimaryBillingAddress.EMail;
            CustomerBill.FIRSTNAME = ThisCustomer.PrimaryBillingAddress.FirstName;
            CustomerBill.LASTNAME = ThisCustomer.PrimaryBillingAddress.LastName;
            CustomerBill.PHONE = ThisCustomer.PrimaryBillingAddress.Phone;
            CustomerBill.STATE = ThisCustomer.PrimaryBillingAddress.State;
            CustomerBill.ZIP = ThisCustomer.PrimaryBillingAddress.Zip;
            CustomerBill.EMAILRECEIPT = "FALSE";
            return CustomerBill;
        }

        public string ProcessTransaction(int OrderNumber, int CustomerID, decimal OrderTotal, bool useLiveTransactions, TransactionModeEnum TransactionMode, Address UseBillingAddress, string CardExtraCode, Address UseShippingAddress, string CAVV, string ECI, string XID, out string AVSResult, out string AuthorizationResult, out string AuthorizationCode, out string AuthorizationTransID, out string TransactionCommandOut, out string TransactionResponse)
        {
            Processors.GatewayProcessor sn = GatewayLoader.GetProcessor("SecureNetV4");
            if (sn == null)
                throw new ArgumentException("Gateway processor \"SecureNetV4\" must be present to process vault transactions.");

            UseBillingAddress.CardNumber = "";//set card to empty to trigger vault transaction

            return sn.ProcessCard(OrderNumber, CustomerID, OrderTotal, useLiveTransactions, TransactionMode, UseBillingAddress, CardExtraCode, UseShippingAddress, CAVV, ECI, XID, out AVSResult, out AuthorizationResult, out AuthorizationCode, out AuthorizationTransID, out TransactionCommandOut, out TransactionResponse);
        }
    }

    public class ServiceResponse
    {
        public Boolean HasError { get; protected set; }
        public String Message { get; protected set; }
        public String MessageIdentifier { get; protected set; }

        public ServiceResponse(Boolean hasError, String message) : this(hasError, message, null) { }

        public ServiceResponse(Boolean hasError, String message, String messageId)
        {
            HasError = hasError;
            Message = message;
            MessageIdentifier = messageId;
        }
    }
}
