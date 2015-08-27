// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using System.Xml;
using System.Threading;
using System.Globalization;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
    [FlagsAttribute]
    public enum AddressTypes : int
    {
        Unknown = 0,
        Billing = 1,
        Shipping = 2,
        Account = 4
    }

    public enum ResidenceTypes : int
    {
        Unknown = 0,
        Residential = 1,
        Commercial = 2
    }


    /// <summary>
    /// Summary description for Address.
    /// </summary>
    public class Address
    {
        private int m_SkinID = 1; // caller must set this if required to be non "1"
        private String m_LocaleSetting = Thread.CurrentThread.CurrentUICulture.Name;
        private int m_CustomerID = 0;
        private int m_AddressID = 0;
        private string m_AddressGuid;
        private int m_DisplayOrder = 0;
        private String m_Separator; // used for Display and ToString() line separators

        private AddressTypes m_AddressType = AddressTypes.Unknown;
        private ResidenceTypes m_ResidenceType = ResidenceTypes.Unknown;

        private String m_NickName = String.Empty;
        private String m_FirstName = String.Empty;
        private String m_LastName = String.Empty;
        private String m_Company = String.Empty;
        private String m_Address1 = String.Empty;
        private String m_Address2 = String.Empty;
        private String m_Suite = String.Empty;
        private String m_City = String.Empty;
        private String m_State = String.Empty;
        private String m_Zip = String.Empty;
        private String m_Country = String.Empty;
        private String m_Phone = String.Empty;
        private String m_Fax = String.Empty;
        private String m_Url = String.Empty;
        private String m_EMail = String.Empty;
        private int m_CountryID = 0;
        private int m_StateID = 0;

        private String m_PaymentMethodLastUsed = String.Empty;
        private String m_CardType = String.Empty;
        private String m_CardNumber = String.Empty;
        private String m_CardName = String.Empty;
        private String m_CardExpirationMonth = String.Empty;
        private String m_CardExpirationYear = String.Empty;
        private String m_CardStartDate = String.Empty; // used in UK/EU
        private String m_CardIssueNumber = String.Empty; // used in UK/EU

        private String m_ECheckBankABACode = String.Empty;
        private String m_ECheckBankAccountNumber = String.Empty;
        private String m_ECheckBankAccountType = String.Empty;
        private String m_ECheckBankName = String.Empty;
        private String m_ECheckBankAccountName = String.Empty;
        private String m_PONumber = String.Empty;


        private DateTime m_ShippingDate = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        public Address()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        public Address(int CustomerID)
        {
            m_CustomerID = CustomerID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="AddressType">Type of the address.</param>
        public Address(int CustomerID, AddressTypes AddressType)
        {
            m_CustomerID = CustomerID;
            m_AddressType = AddressType;
        }

        /// <summary>
        /// Gets or sets the skin ID.
        /// </summary>
        /// <value>The skin ID.</value>
        public int SkinID
        {
            get
            {
                return m_SkinID;
            }
            set
            {
                m_SkinID = value;
            }
        }

        /// <summary>
        /// Gets or sets the locale setting.
        /// </summary>
        /// <value>The locale setting.</value>
        public String LocaleSetting
        {
            get
            {
                return m_LocaleSetting;
            }
            set
            {
                m_LocaleSetting = value;
            }
        }

        /// <summary>
        /// Gets or sets the separator.
        /// </summary>
        /// <value>The separator.</value>
        public String Separator
        {
            get
            {
                return (m_Separator == null ? "" : m_Separator);
            }
            set
            {
                m_Separator = value;
            }
        }


        /// <summary>
        /// Gets or sets the customer ID.
        /// </summary>
        /// <value>The customer ID.</value>
        public int CustomerID
        {
            get
            {
                return m_CustomerID;
            }
            set
            {
                m_CustomerID = value;
            }
        }

        /// <summary>
        /// Gets or sets the address ID.
        /// </summary>
        /// <value>The address ID.</value>
        public int AddressID
        {
            get
            {
                return m_AddressID;
            }
            set
            {
                m_AddressID = value;
            }
        }

        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        /// <value>The display order.</value>
        public int DisplayOrder
        {
            get
            {
                return m_DisplayOrder;
            }
            set
            {
                m_DisplayOrder = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>The name of the nick.</value>
        public String NickName
        {
            get
            {
                return m_NickName;
            }
            set
            {
                m_NickName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>The first name.</value>
        public String FirstName
        {
            get
            {
                return m_FirstName;
            }
            set
            {
                m_FirstName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public String LastName
        {
            get
            {
                return m_LastName;
            }
            set
            {
                m_LastName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>The company.</value>
        public String Company
        {
            get
            {
                return m_Company;
            }
            set
            {
                m_Company = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the first address.
        /// </summary>
        /// <value>The first address.</value>
        public String Address1
        {
            get
            {
                return m_Address1;
            }
            set
            {
                m_Address1 = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the second address.
        /// </summary>
        /// <value>The second address.</value>
        public String Address2
        {
            get
            {
                return m_Address2;
            }
            set
            {
                m_Address2 = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the suite.
        /// </summary>
        /// <value>The suite.</value>
        public String Suite
        {
            get
            {
                return m_Suite;
            }
            set
            {
                m_Suite = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        public String City
        {
            get
            {
                return m_City;
            }
            set
            {
                m_City = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public String State
        {
            get
            {
                return m_State;
            }
            set
            {
                m_State = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>The zip.</value>
        public String Zip
        {
            get
            {
                return m_Zip;
            }
            set
            {
                m_Zip = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>The country.</value>
        public String Country
        {
            get
            {
                return m_Country;
            }
            set
            {
                m_Country = value.Trim();
                if (m_Country.Length == 2 && AppLogic.GetCountryIDFromTwoLetterISOCode(m_Country) > 0)
                    m_CountryID = AppLogic.GetCountryIDFromTwoLetterISOCode(m_Country);
                else if (m_Country.Length == 3 && AppLogic.GetCountryIDFromThreeLetterISOCode(m_Country) > 0)
                    m_CountryID = AppLogic.GetCountryIDFromThreeLetterISOCode(m_Country);
            }
        }

        /// <summary>
        /// Gets the country ID.
        /// </summary>
        /// <value>The country ID.</value>
        public int CountryID
        {
            get { return m_CountryID; }
        }

        /// <summary>
        /// Gets the state ID.
        /// </summary>
        /// <value>The state ID.</value>
        public int StateID
        {
            get { return m_StateID; }
        }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>The phone.</value>
        public String Phone
        {
            get
            {
                return m_Phone;
            }
            set
            {
                m_Phone = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the fax.
        /// </summary>
        /// <value>The Fax.</value>
        public String Fax
        {
            get
            {
                return m_Fax;
            }
            set
            {
                m_Fax = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the Url.
        /// </summary>
        /// <value>The phone.</value>
        public String Url
        {
            get
            {
                return m_Url;
            }
            set
            {
                m_Url = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the E mail.
        /// </summary>
        /// <value>The E mail.</value>
        public String EMail
        {
            get
            {
                return m_EMail;
            }
            set
            {
                m_EMail = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the type of the address.
        /// </summary>
        /// <value>The type of the address.</value>
        public AddressTypes AddressType
        {
            get
            {
                return (m_AddressType);
            }
            set
            {
                m_AddressType = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the residence.
        /// </summary>
        /// <value>The type of the residence.</value>
        public ResidenceTypes ResidenceType
        {
            get
            {
                return (m_ResidenceType);
            }
            set
            {
                m_ResidenceType = value;
            }
        }

        /// <summary>
        /// Gets or sets the shipping date.
        /// </summary>
        /// <value>The shipping date.</value>
        public DateTime ShippingDate
        {
            get
            {
                return m_ShippingDate;
            }
            set
            {
                m_ShippingDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the payment method last used.
        /// </summary>
        /// <value>The payment method last used.</value>
        public String PaymentMethodLastUsed
        {
            get
            {
                return m_PaymentMethodLastUsed;
            }
            set
            {
                m_PaymentMethodLastUsed = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the type of the card.
        /// </summary>
        /// <value>The type of the card.</value>
        public String CardType
        {
            get
            {
                return m_CardType;
            }
            set
            {
                m_CardType = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the name of the card.
        /// </summary>
        /// <value>The name of the card.</value>
        public String CardName
        {
            get
            {
                if (m_CardName.Length == 0)
                {
                    m_CardName = String.Format("{0} {1}", m_FirstName, m_LastName).Trim();
                }
                return m_CardName;
            }
            set
            {
                m_CardName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the card number.
        /// </summary>
        /// <value>The card number.</value>
        public String CardNumber
        {
            get
            {
                return Security.UnmungeString(m_CardNumber, this.GetSaltKey());
            }
            set
            {
                m_CardNumber = Security.MungeString(value.Trim(), this.GetSaltKey());
            }
        }

        /// <summary>
        /// Gets or sets the card expiration month.
        /// </summary>
        /// <value>The card expiration month.</value>
        public String CardExpirationMonth
        {
            get
            {
                return m_CardExpirationMonth;
            }
            set
            {
                m_CardExpirationMonth = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the card expiration year.
        /// </summary>
        /// <value>The card expiration year.</value>
        public String CardExpirationYear
        {
            get
            {
                return m_CardExpirationYear;
            }
            set
            {
                m_CardExpirationYear = value.Trim();
            }
        }

        // must be in format MMYY
        /// <summary>
        /// Gets or sets the card start date.
        /// </summary>
        /// <value>The card start date.</value>
        public String CardStartDate
        {
            get
            {
                return m_CardStartDate;
            }
            set
            {
                m_CardStartDate = value.Trim().Replace(" ", "").Replace("/", "").Replace("\\", "");
            }
        }

        /// <summary>
        /// Gets or sets the card issue number.
        /// </summary>
        /// <value>The card issue number.</value>
        public String CardIssueNumber
        {
            get
            {
                return m_CardIssueNumber;
            }
            set
            {
                m_CardIssueNumber = value.Trim();
            }
        }

        /// <summary>
        /// Gets the E-Check bank ABA code masked.
        /// </summary>
        /// <value>The E-Check bank ABA code masked.</value>
        public string ECheckBankABACodeMasked
        {
            get
            {
                // NOTE:
                //  Here we assume that the property value 
                //  is in clear text/unencrypted. The member
                //  variable is encrypted by property accessor
                //  performs decryption on get
                string abaCodeToMask = ECheckBankABACode;
                if (!CommonLogic.IsStringNullOrEmpty(abaCodeToMask))
                {
                    return Mask(ECheckBankABACode);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the E-Check bank account number masked.
        /// </summary>
        /// <value>The E-Check bank account number masked.</value>
        public string ECheckBankAccountNumberMasked
        {
            get
            {
                // NOTE:
                //  Here we assume that the property value 
                //  is in clear text/unencrypted. The member
                //  variable is encrypted by property accessor
                //  performs decryption on get
                string bankAccountNumberToMask = ECheckBankAccountNumber;
                if (!CommonLogic.IsStringNullOrEmpty(bankAccountNumberToMask))
                {
                    return Mask(ECheckBankAccountNumber);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Masks the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private string Mask(string value)
        {
            string masked = "****";
            if (!CommonLogic.IsStringNullOrEmpty(value) &&
                value.Length > 4)
            {
                masked += value.Substring(value.Length - 4, 4);
            }

            return masked;
        }

        /// <summary>
        /// Gets or sets the E-Check bank ABA code.
        /// </summary>
        /// <value>The E-Check bank ABA code.</value>
        public String ECheckBankABACode
        {
            get
            {
                return Security.UnmungeString(m_ECheckBankABACode, this.GetSaltKey());
            }
            set
            {
                m_ECheckBankABACode = Security.MungeString(value.Trim(), this.GetSaltKey());
            }
        }

        /// <summary>
        /// Gets or sets the E-Check bank account number.
        /// </summary>
        /// <value>The E-Check bank account number.</value>
        public String ECheckBankAccountNumber
        {
            get
            {
                return Security.UnmungeString(m_ECheckBankAccountNumber, this.GetSaltKey());
            }
            set
            {
                m_ECheckBankAccountNumber = Security.MungeString(value.Trim(), this.GetSaltKey());
            }
        }

        /// <summary>
        /// Gets or sets the type of the E-Check bank account.
        /// </summary>
        /// <value>The type of the E-Check bank account.</value>
        public String ECheckBankAccountType
        {
            get
            {
                return m_ECheckBankAccountType;
            }
            set
            {
                m_ECheckBankAccountType = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the name of the E-Check bank.
        /// </summary>
        /// <value>The name of the E-Check bank.</value>
        public String ECheckBankName
        {
            get
            {
                return m_ECheckBankName;
            }
            set
            {
                m_ECheckBankName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the name of the E-Check bank account.
        /// </summary>
        /// <value>The name of the E-Check bank account.</value>
        public String ECheckBankAccountName
        {
            get
            {
                if (m_ECheckBankAccountName.Length == 0)
                {
                    m_ECheckBankAccountName = String.Format("{0} {1}", m_FirstName, m_LastName);
                }
                return m_ECheckBankAccountName;
            }
            set
            {
                m_ECheckBankAccountName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the PO number.
        /// </summary>
        /// <value>The PO number.</value>
        public String PONumber
        {
            get
            {
                return m_PONumber;
            }
            set
            {
                m_PONumber = value.Trim();
            }
        }

        public String AsHTML
        {
            get
            {
                return DisplayHTML(true);
            }
        }
        /// <summary>
        /// Displays the payment method info.
        /// </summary>
        /// <param name="ViewingCustomer">The viewing customer.</param>
        /// <param name="PaymentMethod">The payment method.</param>
        /// <returns></returns>
        public String DisplayPaymentMethodInfo(Customer ViewingCustomer, String PaymentMethod)
        {
            String PMCleaned = AppLogic.CleanPaymentMethod(PaymentMethod);
            if (PMCleaned == AppLogic.ro_PMMicropay)
            {
                return String.Format(AppLogic.GetString("account.aspx.11", m_SkinID, m_LocaleSetting) + " - {0}", ViewingCustomer.CurrencyString(AppLogic.GetMicroPayBalance(CustomerID)));
            }
            if (PMCleaned == AppLogic.ro_PMECheck)
            {
                return String.Format("ECheck - {0}: ABA:{1} Acct:{2}", ECheckBankName, this.ECheckBankABACodeMasked, this.ECheckBankAccountNumberMasked);
            }
            if (PMCleaned == AppLogic.ro_PMCreditCard)
            {
                return String.Format("{0} - {1}: {2} {3}/{4}", AppLogic.GetString("address.cs.54", m_SkinID, m_LocaleSetting), m_CardType, AppLogic.SafeDisplayCardNumber(CardNumber, "Address", m_AddressID), CardExpirationMonth, CardExpirationYear);
            }
            return String.Empty;
        }

        /// <summary>
        /// Gets the safe display of E-Check bank account number.
        /// </summary>
        /// <value>The safe display of E-Check bank account number.</value>
        public String SafeDisplayECheckBankAccountNumber
        {
            get
            {
                if (ECheckBankAccountNumber.Length > 4)
                {
                    return String.Format("***-{0}", ECheckBankAccountNumber.Substring(ECheckBankAccountNumber.Length - 4, 4));
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Clears the Credit Card info.
        /// </summary>
        public void ClearCCInfo()
        {
            CardNumber = "1111111111111111";
            CardType = "111111111111";
            CardExpirationMonth = "11";
            CardExpirationYear = "1111";
            CardName = "111111111111111111111";
            CardStartDate = "1111111";
            CardIssueNumber = "11111111";

            CardNumber = String.Empty;
            CardType = String.Empty;
            CardExpirationMonth = String.Empty;
            CardExpirationYear = String.Empty;
            CardName = String.Empty;
            CardStartDate = String.Empty;
            CardIssueNumber = String.Empty;

            ECheckBankABACode = "1111111111111111";
            ECheckBankABACode = String.Empty;
            ECheckBankAccountNumber = "1111111111111111";
            ECheckBankAccountNumber = String.Empty;
        }

        /// <summary>
        /// Gets or sets as XML.
        /// </summary>
        /// <value>As XML.</value>
        public string AsXml
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("Detail");
                doc.AppendChild(root);
                XmlElement address = doc.CreateElement("Address");
                root.AppendChild(address);

                XmlElement node = doc.CreateElement("AddressID");
                node.InnerText = m_AddressID.ToString();
                address.AppendChild(node);

                node = doc.CreateElement("NickName");
                node.InnerText = m_NickName;
                address.AppendChild(node);

                node = doc.CreateElement("FirstName");
                node.InnerText = m_FirstName;
                address.AppendChild(node);

                node = doc.CreateElement("LastName");
                node.InnerText = m_LastName;
                address.AppendChild(node);

                node = doc.CreateElement("Company");
                node.InnerText = m_Company;
                address.AppendChild(node);

                node = doc.CreateElement("ResidenceType");
                node.InnerText = ((int)m_ResidenceType).ToString();
                address.AppendChild(node);

                node = doc.CreateElement("Address1");
                node.InnerText = m_Address1;
                address.AppendChild(node);

                node = doc.CreateElement("Address2");
                node.InnerText = m_Address2;
                address.AppendChild(node);

                node = doc.CreateElement("Suite");
                node.InnerText = m_Suite;
                address.AppendChild(node);

                node = doc.CreateElement("City");
                node.InnerText = m_City;
                address.AppendChild(node);

                node = doc.CreateElement("State");
                node.InnerText = m_State;
                address.AppendChild(node);

                node = doc.CreateElement("Zip");
                node.InnerText = m_Zip;
                address.AppendChild(node);

                node = doc.CreateElement("Country");
                node.InnerText = m_Country;
                address.AppendChild(node);

                node = doc.CreateElement("Phone");
                node.InnerText = m_Phone;
                address.AppendChild(node);

                node = doc.CreateElement("EMail");
                node.InnerText = m_EMail;
                address.AppendChild(node);

                XmlElement shipping = doc.CreateElement("Shipping");
                root.AppendChild(shipping);

                node = doc.CreateElement("CustomerID");
                node.InnerText = CustomerID.ToString();
                shipping.AppendChild(node);

                node = doc.CreateElement("Date");
                node.InnerText = m_ShippingDate.ToString("s"); //(ISO 8601 sortable)
                shipping.AppendChild(node);

                return doc.OuterXml;
            }
            set
            {
                Clear();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(value);

                XmlNode node = doc.SelectSingleNode("//AddressID");
                if (node != null)
                {
                    AddressID = Int32.Parse(node.InnerText);
                }

                node = doc.SelectSingleNode("//NickName");
                if (node != null)
                {
                    NickName = node.InnerText;
                }

                node = doc.SelectSingleNode("//FirstName");
                if (node != null)
                {
                    FirstName = node.InnerText;
                }

                node = doc.SelectSingleNode("//LastName");
                if (node != null)
                {
                    LastName = node.InnerText;
                }

                node = doc.SelectSingleNode("//Company");
                if (node != null)
                {
                    Company = node.InnerText;
                }

                node = doc.SelectSingleNode("//Address1");
                if (node != null)
                {
                    Address1 = node.InnerText;
                }

                node = doc.SelectSingleNode("//Address2");
                if (node != null)
                {
                    Address2 = node.InnerText;
                }

                node = doc.SelectSingleNode("//Suite");
                if (node != null)
                {
                    Suite = node.InnerText;
                }

                node = doc.SelectSingleNode("//City");
                if (node != null)
                {
                    City = node.InnerText;
                }

                node = doc.SelectSingleNode("//State");
                if (node != null)
                {
                    State = node.InnerText;
                }

                node = doc.SelectSingleNode("//Zip");
                if (node != null)
                {
                    Zip = node.InnerText;
                }

                node = doc.SelectSingleNode("//Country");
                if (node != null)
                {
                    Country = node.InnerText;
                }

                node = doc.SelectSingleNode("//Phone");
                if (node != null)
                {
                    Phone = node.InnerText;
                }

                node = doc.SelectSingleNode("//EMail");
                if (node != null)
                {
                    EMail = node.InnerText;
                }

                node = doc.SelectSingleNode("//CustomerID");
                if (node != null)
                {
                    CustomerID = Int32.Parse(node.InnerText);
                }

            }
        }

        private String GetSaltKey()
        {
            String KY = AppLogic.AppConfig("AddressCCSaltField").Trim();
            String tmp = String.Empty;
            if (KY.Equals("ADDRESSID", StringComparison.InvariantCultureIgnoreCase))
            {
                tmp = m_AddressID.ToString();
            }
            else if (KY.Equals("CUSTOMERID", StringComparison.InvariantCultureIgnoreCase))
            {
                tmp = m_CustomerID.ToString();
            }
            else if (KY.Equals("AddressGUID", StringComparison.InvariantCultureIgnoreCase))
            {
                tmp = m_AddressGuid;
            }
            return tmp;
        }

        /// <summary>
        /// Creates an array of Address sql parameters that can be used by String.Format to build SQL statements.
        /// </summary>
        /// <returns>object[]</returns>
        private object[] AddressValues
        {
            get
            {
                // Munge 'em for security
                string cnMunged = string.Empty;
                string abaMunged = string.Empty;
                string accountMunged = string.Empty;

                if (CardNumber.Length != 0 && CardNumber != AppLogic.ro_CCNotStoredString)
                {
                    cnMunged = Security.MungeString(CardNumber, this.GetSaltKey());
                }
                if (ECheckBankABACode != string.Empty &&
                    ECheckBankAccountNumber != String.Empty &&
                    AppLogic.AppConfigBool("StoreCCinDB"))
                {
                    abaMunged = Security.MungeString(ECheckBankABACode, this.GetSaltKey());
                    accountMunged = Security.MungeString(ECheckBankAccountNumber, this.GetSaltKey());
                }
                object[] values = new object[] 
		  {
			  m_AddressID,                       //{0}
			  CustomerID,                        //{1}
			  DB.SQuote(NickName),               //{2}
			  DB.SQuote(m_FirstName),            //{3}
			  DB.SQuote(m_LastName),             //{4}
			  DB.SQuote(m_Company),              //{5}
			  DB.SQuote(m_Address1),             //{6}
			  DB.SQuote(m_Address2),             //{7}
			  DB.SQuote(m_Suite),                //{8}
			  DB.SQuote(m_City),                 //{9}
			  DB.SQuote(m_State),                //{10}
			  DB.SQuote(m_Zip),                  //{11}
			  DB.SQuote(m_Country),              //{12}
			  DB.SQuote(m_Phone),                //{13}
			  DB.SQuote(PaymentMethodLastUsed),  //{14}
			  DB.SQuote(CardType),               //{15}
			  DB.SQuote(cnMunged),               //{16}
			  DB.SQuote(CardName),               //{17}
			  DB.SQuote(CardExpirationMonth),    //{18}
			  DB.SQuote(CardExpirationYear),     //{19}
			  DB.SQuote(CardStartDate),          //{20}
			  DB.SQuote(Security.MungeString(CardIssueNumber)),        //{21}
			  DB.SQuote(abaMunged),              //{22}
			  DB.SQuote(accountMunged),          //{23}
			  DB.SQuote(ECheckBankAccountType),  //{24}
			  DB.SQuote(ECheckBankName),         //{25}
			  DB.SQuote(ECheckBankAccountName),  //{26}
			  DB.SQuote(PONumber),				  //{27}
			  DB.SQuote(EMail),                  //{28}
			  ((int)m_ResidenceType).ToString()       //{29} 
          };
                return values;
            }
        }

        public void Clear()
        {
            m_CustomerID = 0;
            m_AddressID = 0;
            m_DisplayOrder = 0;

            m_AddressType = AddressTypes.Unknown;
            m_ResidenceType = ResidenceTypes.Unknown;

            m_NickName = String.Empty;
            m_FirstName = String.Empty;
            m_LastName = String.Empty;
            m_Company = String.Empty;
            m_Address1 = String.Empty;
            m_Address2 = String.Empty;
            m_Suite = String.Empty;
            m_City = String.Empty;
            m_State = String.Empty;
            m_Zip = String.Empty;
            m_Country = String.Empty;
            m_Phone = String.Empty;
            m_Fax = String.Empty;
            m_Url = String.Empty;
            m_PaymentMethodLastUsed = String.Empty;
            m_CardType = String.Empty;
            m_CardNumber = String.Empty;
            m_CardName = String.Empty;
            m_CardExpirationMonth = String.Empty;
            m_CardExpirationYear = String.Empty;
            m_CardStartDate = String.Empty;
            m_CardIssueNumber = String.Empty;
            m_ECheckBankABACode = String.Empty;
            m_ECheckBankAccountNumber = String.Empty;
            m_ECheckBankAccountType = String.Empty;
            m_ECheckBankName = String.Empty;
            m_ECheckBankAccountName = String.Empty;
            m_PONumber = String.Empty;
            m_EMail = String.Empty;
        }

        /// <summary>
        /// Adds an address to the Address Table associated with a passed CustomerID
        /// </summary>
        public void InsertDB(int aCustomerID)
        {
            CustomerID = aCustomerID;
            InsertDB();
        }

        /// <summary>
        /// Adds an address to the Address Table
        /// </summary>
        public void InsertDB()
        {
            string AddressGUID = CommonLogic.GetNewGUID();
            string sql = String.Format("insert into Address(AddressGUID,CustomerID) values({0},{1})", DB.SQuote(AddressGUID), CustomerID);
            DB.ExecuteSQL(sql);

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select AddressID from Address with (NOLOCK) where AddressGUID={0}", DB.SQuote(AddressGUID)), dbconn))
                {
                    if (rs.Read())
                    {
                        m_AddressID = DB.RSFieldInt(rs, "AddressID");
                    }
                }
            }
            UpdateDB();
        }

        /// <summary>
        /// Updates the address on Address Table.
        /// </summary>
        public void UpdateDB()
        {
            string sql = String.Format("update Address set CustomerID={1},NickName={2},FirstName={3},LastName={4},Company={5},Address1={6},Address2={7},Suite={8},City={9},State={10},Zip={11},Country={12},Phone={13},PaymentMethodLastUsed={14},CardType={15},CardNumber={16},CardName={17},CardExpirationMonth={18},CardExpirationYear={19},CardStartDate={20},CardIssueNumber={21},ECheckBankABACode={22},ECheckBankAccountNumber={23},ECheckBankAccountType={24},ECheckBankName={25},ECheckBankAccountName={26},PONumber={27},EMail={28},ResidenceType={29} where AddressID={0}", AddressValues);

            DB.ExecuteSQL(sql);
        }

        /// <summary>
        /// Updates the address fields without overwriting credit card or echeck fields. Added to support SmartOPC.
        /// </summary>
        public void UpdateDBAddressOnly()
        {
            string sql = String.Format(
                @"update Address set 
					CustomerID={1},
					NickName={2},
					FirstName={3},
					LastName={4},
					Company={5},
					Address1={6},
					Address2={7},
					Suite={8},
					City={9},
					State={10},
					Zip={11},
					Country={12},
					Phone={13},
					EMail={28},
					ResidenceType={29}
				where 
					AddressID={0}", AddressValues);

            DB.ExecuteSQL(sql);
        }

        /// <summary>
        /// Updates the credit card fields without overwriting address or echeck fields. Added to support SmartOPC.
        /// </summary>
        public void UpdateDBCreditCardOnly()
        {
            string sql = String.Format(
                @"update Address set 
					CustomerID={1},
					CardType={15},
					CardNumber={16},
					CardName={17},
					CardExpirationMonth={18},
					CardExpirationYear={19},
					CardStartDate={20},
					CardIssueNumber={21}
				where 
					AddressID={0}", AddressValues);

            DB.ExecuteSQL(sql);
        }

        /// <summary>
        /// Return a count of number of addresses associated with this customerID
        /// </summary>
        public int Count(int CustomerID)
        {
            return DB.GetSqlN(String.Format("select count(*) as N from Address with (NOLOCK) where CustomerID={0}", CustomerID.ToString()));
        }

        /// <summary>
        /// Deletes an address from the Address Table
        /// </summary>
        static public void DeleteFromDB(int DeleteAddressID, int CustomerID)
        {
            if (DeleteAddressID != 0)
            {

                if (CustomerID == 0)
                {
                    Address addr = new Address();
                    addr.LoadFromDB(DeleteAddressID);
                    CustomerID = addr.CustomerID;
                }

                Customer thisCustomer = new Customer(CustomerID);

                // PABP required
                String sql = String.Format("update Address Set CardNumber=" + DB.SQuote("1111111111111111") + " where AddressID={0}", DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);
                sql = String.Format("update Address Set CardNumber=NULL where AddressID={0}", DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);
                // end PABP mod

                // Delete any CIM mappings
                sql = String.Format("delete from CIM_AddressPaymentProfileMap where AddressID={0}", DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);

                sql = String.Format("delete from Address where AddressID={0}", DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);

                int AlternateBillingAddressID = 0;
                // try to find ANY other customer address (that has credit card info) to use in place of the one being deleted, if required:

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CardNumber IS NOT NULL and CustomerID={0}", thisCustomer.CustomerID.ToString()), dbconn))
                    {
                        if (rs.Read())
                        {
                            AlternateBillingAddressID = DB.RSFieldInt(rs, "AddressID");
                        }
                    }
                }

                int AlternateShippingAddressID = 0;

                // try to find ANY other customer address (that does not have credit card info) to use in place of the one being deleted, if required:
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CardNumber IS NULL and CustomerID={0}", thisCustomer.CustomerID.ToString()), dbconn))
                    {
                        if (rs.Read())
                        {
                            AlternateShippingAddressID = DB.RSFieldInt(rs, "AddressID");
                        }
                    }
                }

                int BackupAddressID = 0;
                // try to find ANY other customer address as further backup, if required:
                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS(String.Format("select top 1 AddressID from Address with (NOLOCK) where CustomerID={0}", thisCustomer.CustomerID.ToString()), dbconn))
                    {
                        if (rs.Read())
                        {
                            BackupAddressID = DB.RSFieldInt(rs, "AddressID");
                        }
                    }
                }

                if (AlternateBillingAddressID == 0)
                {
                    AlternateBillingAddressID = BackupAddressID;
                }

                if (AlternateShippingAddressID == 0)
                {
                    AlternateShippingAddressID = BackupAddressID;
                }

                // now try to prevent invalid conditions
                if (thisCustomer.PrimaryBillingAddressID == DeleteAddressID)
                {
                    DB.ExecuteSQL(String.Format("update Customer set BillingAddressID={0} where CustomerID={1}", AlternateBillingAddressID.ToString(), thisCustomer.CustomerID.ToString()));
                }
                if (thisCustomer.PrimaryShippingAddressID == DeleteAddressID)
                {
                    DB.ExecuteSQL(String.Format("update Customer set ShippingAddressID={0} where CustomerID={1}", AlternateShippingAddressID.ToString(), thisCustomer.CustomerID.ToString()));
                }

                // update any cart shipping addresses (all types, regular cart, wish list, recurring cart, and gift registry) that match the one being deleted:
                sql = String.Format("update ShoppingCart set ShippingAddressID={0} where ShippingAddressID={1}", AlternateShippingAddressID.ToString(), DeleteAddressID.ToString());
                DB.ExecuteSQL(sql);
            }
        }

        /// <summary>
        /// Makes the customers primary address.
        /// </summary>
        /// <param name="aAddressType">Type of a address.</param>
        public void MakeCustomersPrimaryAddress(AddressTypes aAddressType)
        {
            //An address could be both Type Shipping and Billing save both to Customer if so.
            int CurrentPrimaryShippingAddressID = 0;
            string sql = String.Empty;

            if ((aAddressType & AddressTypes.Billing) != 0)
            {
                sql = "BillingAddressID={0}";
            }
            if ((aAddressType & AddressTypes.Shipping) != 0)
            {
                CurrentPrimaryShippingAddressID = Customer.GetCustomerPrimaryShippingAddressID(CustomerID);

                if (sql.Length != 0)
                {
                    sql += ",";
                }
                sql += "ShippingAddressID={0}";
            }
            sql = "update Customer set " + sql + " where CustomerID={1}";
            sql = String.Format(sql, AddressValues);
            DB.ExecuteSQL(sql);
            if (aAddressType == AddressTypes.Shipping)
            {
                Customer.SetPrimaryShippingAddressForShoppingCart(CustomerID, CurrentPrimaryShippingAddressID, AddressID);
            }
        }

        /// <summary>
        /// Gets the salt key.
        /// </summary>
        /// <param name="AddressID">The address ID.</param>
        /// <returns>Returns the salt key</returns>
        public static String StaticGetSaltKey(int AddressID)
        {
            String tmp = String.Empty;
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select " + AppLogic.AppConfig("AddressCCSaltField") + " from Address  with (NOLOCK)  where AddressID=" + AddressID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        tmp = rs[AppLogic.AppConfig("AddressCCSaltField")].ToString();
                    }
                }
            }
            return tmp;
        }


        /// <summary>
        /// Copies to shopping cart DataBase.
        /// </summary>
        /// <param name="ShoppingCartID">The shopping cart ID.</param>
        /// <param name="aAddressType">Type of a address.</param>
        public void CopyToShoppingCartDB(int ShoppingCartID, AddressTypes aAddressType)
        {
            //An address could be both Type Shipping and Billing save both to Customer if so.
            string sql = String.Empty;
            if ((aAddressType & AddressTypes.Billing) != 0)
            {
                sql = "BillingAddressID={0}";
            }
            if ((aAddressType & AddressTypes.Shipping) != 0)
            {
                if (sql.Length != 0)
                {
                    sql += ",";
                }
                sql += "ShippingAddressID={0}";
            }
            sql = "update ShoppingCart set " + sql + " where ShoppingCartID={1}";
            sql = String.Format(sql, m_AddressID, ShoppingCartID);
            DB.ExecuteSQL(sql);
        }
        /// <summary>
        /// Loads the Address record using the AddressID to select it.
        /// </summary>
        /// <param name="AddressID"></param>
        public void LoadFromDB(int AddressID)
        {
            Clear();
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select a.*, c.CountryID, s.StateID from dbo.Address a with (NOLOCK) left outer join dbo.Country c with (NOLOCK) on a.country = c.name left outer join dbo.State s with (NOLOCK) on a.State = s.Abbreviation and s.countryid = c.countryid where AddressID={0}", AddressID.ToString()), dbconn))
                {
                    if (rs.Read())
                    {
                        m_AddressID = DB.RSFieldInt(rs, "AddressID");
                        m_AddressGuid = DB.RSFieldGUID(rs, "AddressGuid");
                        CustomerID = DB.RSFieldInt(rs, "CustomerID");
                        m_NickName = DB.RSField(rs, "NickName");
                        m_FirstName = DB.RSField(rs, "FirstName");
                        m_LastName = DB.RSField(rs, "LastName");
                        m_Company = DB.RSField(rs, "Company");
                        m_Address1 = DB.RSField(rs, "Address1");
                        m_Address2 = DB.RSField(rs, "Address2");
                        m_Suite = DB.RSField(rs, "Suite");
                        m_City = DB.RSField(rs, "City");
                        m_State = DB.RSField(rs, "State");
                        m_Zip = DB.RSField(rs, "Zip");
                        m_Country = DB.RSField(rs, "Country");
                        m_CountryID = DB.RSFieldInt(rs, "CountryID");
                        m_StateID = DB.RSFieldInt(rs, "StateID");
                        m_Phone = DB.RSField(rs, "Phone");
                        m_ResidenceType = (ResidenceTypes)DB.RSFieldInt(rs, "ResidenceType");
                        PaymentMethodLastUsed = DB.RSField(rs, "PaymentMethodLastUsed");
                        CardType = DB.RSField(rs, "CardType");

                        CardNumber = DB.RSField(rs, "CardNumber");
                        if (CardNumber.Length != 0 && CardNumber != AppLogic.ro_CCNotStoredString)
                        {
                            CardNumber = Security.UnmungeString(CardNumber, this.GetSaltKey());
                            if (CardNumber.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                            {
                                CardNumber = DB.RSField(rs, "CardNumber");
                            }
                        }

                        CardName = DB.RSField(rs, "CardName");
                        CardExpirationMonth = DB.RSField(rs, "CardExpirationMonth");
                        CardExpirationYear = DB.RSField(rs, "CardExpirationYear");
                        CardStartDate = DB.RSField(rs, "CardStartDate");
                        CardIssueNumber = Security.UnmungeString(DB.RSField(rs, "CardIssueNumber"));

                        // NOTE:
                        // Since we are already doing the munging and unmunging
                        // on the get set of the property, let's just assign the 
                        // value directly to the member variables 

                        string saltKey = this.GetSaltKey();

                        string eCheckABACode = DB.RSField(rs, "ECheckBankABACode");
                        string eCheckABACodeUnMunged = Security.UnmungeString(eCheckABACode, saltKey);

                        if (eCheckABACodeUnMunged.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Failed decryption, must be clear text
                            ECheckBankABACode = eCheckABACode;
                        }
                        else
                        {
                            // decryption successful, must be already encrypted
                            ECheckBankABACode = eCheckABACodeUnMunged;
                        }

                        string eCheckBankAccountNumber = DB.RSField(rs, "ECheckBankAccountNumber");
                        string eCheckBankAccountNumberUnMunged = Security.UnmungeString(eCheckBankAccountNumber, saltKey);

                        if (eCheckBankAccountNumberUnMunged.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Failed decryption, must be clear text
                            ECheckBankAccountNumber = eCheckBankAccountNumber;
                        }
                        else
                        {
                            // decryption successful, must be already encrypted
                            ECheckBankAccountNumber = eCheckBankAccountNumberUnMunged;
                        }

                        ECheckBankAccountType = DB.RSField(rs, "ECheckBankAccountType");
                        ECheckBankName = DB.RSField(rs, "ECheckBankName");
                        ECheckBankAccountName = DB.RSField(rs, "ECheckBankAccountName");
                        PONumber = DB.RSField(rs, "PONumber");
                        EMail = DB.RSField(rs, "EMail");

                    }
                    else
                    {
                        Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Loads the customer.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="aAddressType">Type of a address.</param>
        public void LoadByCustomer(int CustomerID, AddressTypes aAddressType)
        {
            int AddressID = 0;
            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(String.Format("select ShippingAddressID,BillingAddressID from Customer with (NOLOCK) where CustomerID={0}", CustomerID.ToString()), dbconn))
                {
                    if (rs.Read())
                    {
                        if (aAddressType == AddressTypes.Billing)
                        {
                            AddressID = DB.RSFieldInt(rs, "BillingAddressID");
                        }
                        else
                        {
                            AddressID = DB.RSFieldInt(rs, "ShippingAddressID");
                        }
                    }
                }
            }

            LoadByCustomer(CustomerID, AddressID, aAddressType);
        }

        /// <summary>
        /// Loads the customer.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        /// <param name="AddressID">The address ID.</param>
        /// <param name="aAddressType">Type of a address.</param>
        public void LoadByCustomer(int CustomerID, int AddressID, AddressTypes aAddressType)
        {
            m_AddressID = AddressID;
            if (m_AddressID != 0)
            {
                LoadFromDB(m_AddressID);
                m_AddressType = aAddressType;
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return DisplayHTML(false);
        }

        /// <summary>
        /// Displays the string.
        /// </summary>
        /// <param name="Checkout">if set to <c>true</c> [checkout].</param>
        /// <param name="IncludePhone">if set to <c>true</c> [include phone].</param>
        /// <param name="Separator">The separator.</param>
        /// <returns>Returns the display string.</returns>
        public string DisplayHTML(bool IncludePhone)
        {
            string s = String.Empty;

            StringBuilder tmpS = new StringBuilder(1000);

            tmpS.Append(String.Format("<div>{0} {1}</div>", FirstName, LastName));
			tmpS.Append(String.IsNullOrEmpty(m_Company) ? "" : String.Format("<div>{0}</div>", Company));
			tmpS.Append(String.IsNullOrEmpty(m_Address1) ? "" : String.Format("<div>{0}</div>", Address1));
			tmpS.Append(String.IsNullOrEmpty(m_Address2) ? "" : String.Format("<div>{0}</div>", Address2));
			tmpS.Append(String.IsNullOrEmpty(m_Suite) ? "" : String.Format("<div>{0}</div>", Suite));
			tmpS.Append(String.Format("<div>{0}, {1} {2}</div>", City, State, Zip));
			tmpS.Append(String.IsNullOrEmpty(m_Country) ? "" : String.Format("<div>{0}</div>", Country));
            if (IncludePhone)
			{
				tmpS.Append(String.IsNullOrEmpty(m_Phone) ? "" : String.Format("<div>{0}</div>", Phone));
            }

            return tmpS.ToString();
        }

        /// <summary>
        /// Displays the card HTML.
        /// </summary>
        /// <returns>Returns the card's display string.</returns>
        public string DisplayCardHTML()
        {
            return DisplayCardString("");
        }

        /// <summary>
        /// Displays the card string.
        /// </summary>
        /// <param name="separator">The separator.</param>
        /// <returns>Returns the card's display string.</returns>
        public string DisplayCardString(string separator)
        {
            StringBuilder tmpS = new StringBuilder(1000);
            tmpS.Append(CardName + separator);
            tmpS.Append(String.Format("{0}: {1}{2}", CardType, AppLogic.SafeDisplayCardNumber(CardNumber, "Address", m_AddressID), separator));
            tmpS.Append(String.Format("{0:0#}/{1:000#}{2}", CardExpirationMonth, CardExpirationYear, separator));
            return tmpS.ToString();
        }

        /// <summary>
        /// Displays the E-Check HTML.
        /// </summary>
        /// <returns>Retruns the E-Check HTML.</returns>
        public string DisplayECheckHTML()
        {
            StringBuilder tmpS = new StringBuilder(1000);
            tmpS.Append("<span>Payment Method:</span>");
            tmpS.Append(PaymentMethodLastUsed + "");
            tmpS.Append(ECheckBankName + "");
            tmpS.Append(String.Format("{0} : ***-{1}", ECheckBankABACode, ECheckBankAccountNumber.Substring(ECheckBankAccountNumber.Length - 5, 5)));
            return tmpS.ToString();
        }

        /// <summary>
        /// Create an HTML address
        /// </summary>
        /// <returns></returns>
        public string InputHTML()
        {
            return InputHTML(String.Empty, true);
        }
        /// <summary>
        /// Create an HTML address
        /// </summary>
        /// <param name="IncludeFormValidation">True or False, Whether to add form validation</param>
        /// <returns>HTML address</returns>
        public string InputHTML(bool IncludeFormValidation)
        {
            return InputHTML(String.Empty, IncludeFormValidation);
        }
        /// <summary>
        /// Create an HTML address
        /// </summary>
        /// <param name="FieldPrefix">The FieldPrefix that will be added to the name of each control</param>
        /// <param name="IncludeFormValidation">True or False, Whether to add form validation</param>
        /// <returns>HTML address</returns>
        public string InputHTML(String FieldPrefix, bool IncludeFormValidation)
        {
            bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !AppLogic.AppConfigBool("SkipShippingOnCheckout");

            StringBuilder tmpS = new StringBuilder(1000);

            if (IncludeFormValidation)
            {
                tmpS.Append("<script type=\"text/javascript\">\n");
                tmpS.Append("function AddressInputForm" + FieldPrefix + "_Validator(theForm)\n");
                tmpS.Append("{\n");
                tmpS.Append("  submitonce(theForm);\n");
                if (AppLogic.AppConfigBool("DisallowShippingToPOBoxes"))
                {
                    if (FieldPrefix.Equals("SHIPPING", StringComparison.InvariantCultureIgnoreCase))
                    {
                        tmpS.Append("	var ADDR1 = theForm.ShippingAddress1.value.toLowerCase();\n");
                        tmpS.Append("  if (ADDR1.substring(0,2) == 'po' || ADDR1.substring(0,4) == 'p.o.' || ADDR1.substring(0,3) == 'po.' || ADDR1.substring(0,3) == 'box')\n");
                        tmpS.Append("	{\n");
                        tmpS.Append("    alert(\"We're sorry, but we cannot ship to a PO Box address. Please enter a street address.\");\n");
                        tmpS.Append("    theForm.ShippingAddress1.focus();\n");
                        tmpS.Append("    submitenabled(theForm);\n");
                        tmpS.Append("    return (false);\n");
                        tmpS.Append("	}\n");
                    }
                }
                tmpS.Append("  if (theForm.AddressState" + FieldPrefix + ".selectedIndex < 1)\n");
                tmpS.Append("  {\n");
                tmpS.Append("    alert(\"" + AppLogic.GetString("address.cs.1", m_SkinID, m_LocaleSetting) + "\");\n");
                tmpS.Append("    theForm.AddressState" + FieldPrefix + ".focus();\n");
                tmpS.Append("    submitenabled(theForm);\n");
                tmpS.Append("    return (false);\n");
                tmpS.Append("  }\n");
                tmpS.Append("  return (true);\n");
                tmpS.Append("}\n");
                tmpS.Append("</script>\n");
            }
            tmpS.Append("       <div class='form card-form'>\n");
            tmpS.Append("       <div class='form-group'>\n");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.49", m_SkinID, m_LocaleSetting) + "</label>");


            // AddressNickName_Post is just a hidden field to retain the 
            // value of 'AddressNickName control' due to postback.
            string nickNamePostbackValue = CommonLogic.FormCanBeDangerousContent("AddressNickName_Post");
            if (!CommonLogic.IsStringNullOrEmpty(nickNamePostbackValue))
            {
                m_NickName = nickNamePostbackValue;
            }
            // AddressNickName
            tmpS.Append("        <input type=\"text\" name=\"AddressNickName" + FieldPrefix + "\" id=\"AddressNickName" + FieldPrefix + "\" class=\"form-control\" maxlength=\"100\" value=\"" + NickName + "\"><!-- COPY FROM ABOVE -->");
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group first-name'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.2", m_SkinID, m_LocaleSetting) + "</label>");

            // AddressFirstName_Post is just a hidden field to retain the 
            // value of 'AddressFirstName control' due to postback.
            string firstNamePostbackValue = CommonLogic.FormCanBeDangerousContent("AddressFirstName_Post");
            if (!CommonLogic.IsStringNullOrEmpty(firstNamePostbackValue))
            {
                m_FirstName = firstNamePostbackValue;
            }
            // AddressFirstName
            tmpS.Append("        <input type=\"text\" name=\"AddressFirstName" + FieldPrefix + "\" id=\"AddressFirstName" + FieldPrefix + "\" class=\"form-control\" maxlength=\"50\" value=\"" + m_FirstName + "\" >");
            if (IncludeFormValidation)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"AddressFirstName" + FieldPrefix + "_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.13", m_SkinID, m_LocaleSetting) + "]\">\n");
            }
            tmpS.Append("      </div>");



            tmpS.Append("      <div class='form-group last-name'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.3", m_SkinID, m_LocaleSetting) + "</label>");

            // AddressLastName_Post is just a hidden field to retain the 
            // value of 'AddressLastName control' due to postback.
            string lastNamePostbackValue = CommonLogic.FormCanBeDangerousContent("AddressLastName_Post");
            if (!CommonLogic.IsStringNullOrEmpty(lastNamePostbackValue))
            {
                m_LastName = lastNamePostbackValue;
            }
            // AddressLastName
            tmpS.Append("        <input type=\"text\" name=\"AddressLastName" + FieldPrefix + "\" id=\"AddressLastName" + FieldPrefix + "\" class=\"form-control\" maxlength=\"50\" value=\"" + m_LastName + "\">");
            if (IncludeFormValidation)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"AddressLastName" + FieldPrefix + "_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.14", m_SkinID, m_LocaleSetting) + "]\">\n");
            }
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group phone'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.4", m_SkinID, m_LocaleSetting) + "</label>");
            // AddressPhone_Post is just a hidden field to retain the 
            // value of 'AddressPhone control' due to postback.
            string phoneNumberPostbackValue = CommonLogic.FormCanBeDangerousContent("AddressPhone_Post");
            if (!CommonLogic.IsStringNullOrEmpty(phoneNumberPostbackValue))
            {
                m_Phone = phoneNumberPostbackValue;
            }
            // AddressPhone
            tmpS.Append("        <input type=\"text\" name=\"AddressPhone" + FieldPrefix + "\" id=\"AddressPhone" + FieldPrefix + "\" class=\"form-control\" maxlength=\"25\" value=\"" + m_Phone + "\">");
            if (IncludeFormValidation)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"AddressPhone" + FieldPrefix + "_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.15", m_SkinID, m_LocaleSetting) + "]\">");
            }
            tmpS.Append("      </div>");


            tmpS.Append("      <div class='form-group company'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.5", m_SkinID, m_LocaleSetting) + "</label>");

            // AddressCompany_Post is just a hidden field to retain the 
            // value of 'AddressCompany control' due to postback.
            string addressCompanyPostbackValue = CommonLogic.FormCanBeDangerousContent("AddressCompany_Post");
            if (!CommonLogic.IsStringNullOrEmpty(addressCompanyPostbackValue))
            {
                m_Company = addressCompanyPostbackValue;
            }
            // AddressCompany
            tmpS.Append("        <input type=\"text\" name=\"AddressCompany" + FieldPrefix + "\" id=\"AddressCompany" + FieldPrefix + "\" class=\"form-control\" maxlength=\"100\" value=\"" + m_Company + "\">");
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group residence-type'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.58", m_SkinID, m_LocaleSetting) + "</label>");
            // ResidentType_Post is just a hidden field to retain the 
            // value of 'ResidenceType control' due to postback.
            string residenceTypePostbackValue = CommonLogic.FormCanBeDangerousContent("ResidentType_Post");

            if (!CommonLogic.IsStringNullOrEmpty(residenceTypePostbackValue))
            {
                if (residenceTypePostbackValue == ((int)ResidenceTypes.Unknown).ToString())
                {
                    m_ResidenceType = ResidenceTypes.Unknown;
                }
                else if (residenceTypePostbackValue == ((int)ResidenceTypes.Residential).ToString())
                {
                    m_ResidenceType = ResidenceTypes.Residential;
                }
                else if (residenceTypePostbackValue == ((int)ResidenceTypes.Commercial).ToString())
                {
                    m_ResidenceType = ResidenceTypes.Commercial;
                }

                // ResidenceType
                tmpS.Append("        <select class=\"form-control\" name=\"ResidenceType" + FieldPrefix + "\" id=\"ResidenceType" + FieldPrefix + "\">");
                tmpS.Append("        <OPTION value=\"" + ((int)ResidenceTypes.Unknown).ToString() + "\"" + CommonLogic.IIF((m_ResidenceType == ResidenceTypes.Unknown), " selected", String.Empty) + "\">" + AppLogic.GetString("address.cs.55", m_SkinID, m_LocaleSetting) + "</option>");
                tmpS.Append("        <OPTION value=\"" + ((int)ResidenceTypes.Residential).ToString() + "\"" + CommonLogic.IIF((m_ResidenceType == ResidenceTypes.Residential), " selected", String.Empty) + " >" + AppLogic.GetString("address.cs.56", m_SkinID, m_LocaleSetting) + "</option>");
                tmpS.Append("        <OPTION value=\"" + ((int)ResidenceTypes.Commercial).ToString() + "\"" + CommonLogic.IIF((m_ResidenceType == ResidenceTypes.Commercial), " selected", String.Empty) + " >" + AppLogic.GetString("address.cs.57", m_SkinID, m_LocaleSetting) + "</option>");
                tmpS.Append("        </select>");

            }
            else
            {
                // ResidenceType
                tmpS.Append("        <select class=\"form-control\" name=\"ResidenceType" + FieldPrefix + "\" id=\"ResidenceType" + FieldPrefix + "\">");
                tmpS.Append("        <OPTION value=\"" + ((int)ResidenceTypes.Unknown).ToString() + "\"" + CommonLogic.IIF((m_ResidenceType == ResidenceTypes.Unknown && AddressID > 0), " selected", String.Empty) + "\">" + AppLogic.GetString("address.cs.55", m_SkinID, m_LocaleSetting) + "</option>");
                tmpS.Append("        <OPTION value=\"" + ((int)ResidenceTypes.Residential).ToString() + "\"" + CommonLogic.IIF((m_ResidenceType == ResidenceTypes.Residential || (m_ResidenceType == ResidenceTypes.Unknown && AddressID <= 0)), " selected", String.Empty) + " >" + AppLogic.GetString("address.cs.56", m_SkinID, m_LocaleSetting) + "</option>");
                tmpS.Append("        <OPTION value=\"" + ((int)ResidenceTypes.Commercial).ToString() + "\"" + CommonLogic.IIF((m_ResidenceType == ResidenceTypes.Commercial), " selected", String.Empty) + " >" + AppLogic.GetString("address.cs.57", m_SkinID, m_LocaleSetting) + "</option>");
                tmpS.Append("        </select>");
            }
            tmpS.Append("      </div>");


            tmpS.Append("      <div class='form-group address-one'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.6", m_SkinID, m_LocaleSetting) + "</label>");
            // AddressAddress1_Post is just a hidden field to retain the 
            // value of 'AddressAddress1 control' due to postback.
            string addressAddress1PostbackValue = CommonLogic.FormCanBeDangerousContent("AddressAddress1_Post");
            if (!CommonLogic.IsStringNullOrEmpty(addressAddress1PostbackValue))
            {
                m_Address1 = addressAddress1PostbackValue;
            }
            // AddressAddress1
            tmpS.Append("        <input type=\"text\" name=\"AddressAddress1" + FieldPrefix + "\" id=\"AddressAddress1" + FieldPrefix + "\" class=\"form-control\" maxlength=\"100\" value=\"" + m_Address1 + "\">");
            if (IncludeFormValidation)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"AddressAddress1" + FieldPrefix + "_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.16", m_SkinID, m_LocaleSetting) + "]\">\n");
            }
            tmpS.Append("      </div>");


            tmpS.Append("      <div class='form-group address-two'>");
            tmpS.Append("       <label>" + AppLogic.GetString("address.cs.7", m_SkinID, m_LocaleSetting) + "</label>");
            // AddressAddress2_Post is just a hidden field to retain the 
            // value of 'AddressAddress2 control' due to postback.
            string addressAddress2PostbackValue = CommonLogic.FormCanBeDangerousContent("AddressAddress2_Post");
            if (!CommonLogic.IsStringNullOrEmpty(addressAddress2PostbackValue))
            {
                m_Address2 = addressAddress2PostbackValue;
            }
            // AddressAddress2
            tmpS.Append("        <input type=\"text\" name=\"AddressAddress2" + FieldPrefix + "\" id=\"AddressAddress2" + FieldPrefix + "\" class=\"form-control\" maxlength=\"100\" value=\"" + m_Address2 + "\">");
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group suite'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.8", m_SkinID, m_LocaleSetting) + "</label>");
            // AddressSuite_Post is just a hidden field to retain the 
            // value of 'AddressSuite control' due to postback.
            string addressSuitePostbackValue = CommonLogic.FormCanBeDangerousContent("AddressSuite_Post");
            if (!CommonLogic.IsStringNullOrEmpty(addressSuitePostbackValue))
            {
                m_Suite = addressSuitePostbackValue;
            }
            //AddressSuite
            tmpS.Append("        <input type=\"text\" name=\"AddressSuite" + FieldPrefix + "\" id=\"AddressSuite" + FieldPrefix + "\" class=\"form-control\" maxlength=\"50\" value=\"" + m_Suite + "\">");
            tmpS.Append("      </div>");


            tmpS.Append("      <div class='form-group city'>");
            tmpS.Append("       <label>" + AppLogic.GetString("address.cs.9", m_SkinID, m_LocaleSetting) + "</label>");

            // AddressCity_Post is just a hidden field to retain the 
            // value of 'AddressCity control' due to postback.
            string addressCityPostbackValue = CommonLogic.FormCanBeDangerousContent("AddressCity_Post");
            if (!CommonLogic.IsStringNullOrEmpty(addressCityPostbackValue))
            {
                m_City = addressCityPostbackValue;
            }
            // AddressCity
            tmpS.Append("        <input type=\"text\" name=\"AddressCity" + FieldPrefix + "\" id=\"AddressCity" + FieldPrefix + "\" class=\"form-control\" maxlength=\"50\" value=\"" + m_City + "\">");
            if (IncludeFormValidation)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"AddressCity" + FieldPrefix + "_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.17", m_SkinID, m_LocaleSetting) + "]\">\n");
            }
            tmpS.Append("      </div>");


            tmpS.Append("      <div class='form-group state'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.10", m_SkinID, m_LocaleSetting) + "</label>");
            // AddressState
            tmpS.Append("        <select class=\"form-control\" name=\"AddressState" + FieldPrefix + "\" id=\"AddressState" + FieldPrefix + "\">");

            string countryPostbackValue = CommonLogic.FormCanBeDangerousContent("AddressCountry_Post");

            int sIdx = 0;
            if (!CommonLogic.IsStringNullOrEmpty(countryPostbackValue))
            {
                string sql = "select s.* from State s  with (NOLOCK)  join country c  with (NOLOCK)  on s.countryid = c.countryid where c.name = " + DB.SQuote(countryPostbackValue) + " order by s.DisplayOrder,s.Name";
                bool statesFound = false;
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS(sql, conn))
                    {
                        while (rs.Read())
                        {
                            if (sIdx == 0)
                            {
                                tmpS.Append("        <option value=\"\"" + CommonLogic.IIF((m_State.Length == 0), " selected", String.Empty) + " >" + AppLogic.GetString("address.cs.11", m_SkinID, m_LocaleSetting) + "</option>");
                            }

                            statesFound = true;
                            tmpS.Append("      <option value=\"" + DB.RSField(rs, "Abbreviation") + "\"" + CommonLogic.IIF(DB.RSField(rs, "Abbreviation") == m_State, " selected", String.Empty) + ">" + DB.RSField(rs, "Name") + "</option>");
                            sIdx++;
                        }
                    }
                }

                if (!statesFound)
                {
                    tmpS.Append("      <option value=\"--\" >" + AppLogic.GetString("state.countrywithoutstates", m_SkinID, m_LocaleSetting) + "</option>");
                }
            }
            else
            {
                using (SqlConnection conn = DB.dbConn())
                {
                    conn.Open();
                    using (IDataReader rs = DB.GetRS("select * from state  with (NOLOCK)  order by DisplayOrder,Name", conn))
                    {
                        while (rs.Read())
                        {
                            if (sIdx == 0)
                            {
                                tmpS.Append("        <option value=\"\"" + CommonLogic.IIF((m_State.Length == 0), " selected", String.Empty) + " >" + AppLogic.GetString("address.cs.11", m_SkinID, m_LocaleSetting) + "</option>");
                            }

                            tmpS.Append("      <option value=\"" + DB.RSField(rs, "Abbreviation") + "\"" + CommonLogic.IIF(DB.RSField(rs, "Abbreviation") == m_State, " selected", String.Empty) + ">" + DB.RSField(rs, "Name") + "</option>");
                            sIdx++;
                        }
                    }
                }
            }

            tmpS.Append("        </select>");
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group zip-code'>");
            tmpS.Append("       <label>" + AppLogic.GetString("address.cs.12", m_SkinID, m_LocaleSetting) + "</label>");
            // AddressZip_Post is just a hidden field to retain the 
            // value of 'AddressZip control' due to postback.
            string addressZipPostbackValue = CommonLogic.FormCanBeDangerousContent("AddressZip_Post");
            if (!CommonLogic.IsStringNullOrEmpty(addressZipPostbackValue))
            {
                m_Zip = addressZipPostbackValue;
            }

            // AddressZip
            tmpS.Append("        <input type=\"text\" name=\"AddressZip" + FieldPrefix + "\" id=\"AddressZip" + FieldPrefix + "\" class=\"form-control\" maxlength=\"10\" value=\"" + m_Zip + "\">");
            if (IncludeFormValidation)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"AddressZip" + FieldPrefix + "_vldt\" value=\"[blankalert=" + AppLogic.GetString("address.cs.18", m_SkinID, m_LocaleSetting) + "]\">");
            }
            tmpS.Append("      </div>");


            tmpS.Append("      <div class='form-group'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.53", m_SkinID, m_LocaleSetting) + "</label>");
            if (!CommonLogic.IsStringNullOrEmpty(countryPostbackValue))
            {
                m_Country = countryPostbackValue;
            }

            string script = "onchange={doAddressPostBack();}";
            // Make the AddressCountry Postback onchange in the current URL so we can get the 
            // corresponding state value for each country
            tmpS.Append("        <select name=\"AddressCountry" + FieldPrefix + "\" id=\"AddressCountry" + FieldPrefix + "\" class=\"form-control\" " + script + ">");

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from country  with (NOLOCK)  where Published = 1 order by DisplayOrder,Name", conn))
                {
                    while (rs.Read())
                    {
                        tmpS.Append("      <option value=\"" + DB.RSField(rs, "Name") + "\"" + CommonLogic.IIF(DB.RSField(rs, "Name") == m_Country, " selected", String.Empty) + ">" + DB.RSField(rs, "Name") + "</option>");
                    }
                }
            }

            tmpS.Append("        </select>");
            tmpS.Append(" </div>");
            tmpS.Append(" </div>");

            string currentUrl = AppLogic.GetThisPageUrlWithQueryString();

            StringBuilder formScript = new StringBuilder();
            //Create form that will handle the postback data
            formScript.AppendFormat("<script type=\"text/javascript\" language=\"Javascript\">\n");
            formScript.AppendFormat("    function doAddressPostBack() {{\n");
            formScript.AppendFormat("        var frm = document.createElement('form');\n");
            formScript.AppendFormat("        frm.id = 'frmAddressPost';\n");
            formScript.AppendFormat("        frm.name = 'frmAddressPost';\n");
            formScript.AppendFormat("        frm.action = \"{0}\";\n", currentUrl);
            formScript.AppendFormat("        frm.method = 'post';\n");
            formScript.AppendFormat("        addInput(frm, 'AddressNickName_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressFirstName_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressLastName_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressPhone_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressCompany_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'ResidentType_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressAddress1_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressAddress2_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressSuite_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressCity_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressZip_Post');\n");
            formScript.AppendFormat("        addInput(frm, 'AddressCountry_Post');\n");
            formScript.AppendFormat("        document.body.appendChild(frm);\n");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressNickName" + FieldPrefix, "AddressNickName_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressFirstName" + FieldPrefix, "AddressFirstName_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressLastName" + FieldPrefix, "AddressLastName_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressPhone" + FieldPrefix, "AddressPhone_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressCompany" + FieldPrefix, "AddressCompany_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'dropdown');\n", "ResidenceType" + FieldPrefix, "ResidentType_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressAddress1" + FieldPrefix, "AddressAddress1_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressAddress2" + FieldPrefix, "AddressAddress2_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressSuite" + FieldPrefix, "AddressSuite_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressCity" + FieldPrefix, "AddressCity_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'text');\n", "AddressZip" + FieldPrefix, "AddressZip_Post");
            formScript.AppendFormat("        stuffValue('{0}', '{1}', 'dropdown');\n", "AddressCountry" + FieldPrefix, "AddressCountry_Post");
            formScript.AppendFormat("        frm.submit();\n");
            formScript.AppendFormat("    }}\n");
            // Create hidden field to hold the value
            formScript.AppendFormat("    function addInput(form, name) {{\n");
            formScript.AppendFormat("        var inp = document.createElement('input');\n");
            formScript.AppendFormat("        inp.type = 'hidden';\n");
            formScript.AppendFormat("        inp.id = name;\n");
            formScript.AppendFormat("        inp.name = name;\n");
            formScript.AppendFormat("        form.appendChild(inp);\n");
            formScript.AppendFormat("    }}\n");
            //Determined if its text or dropdown then assign the selected value
            formScript.AppendFormat("    function stuffValue(from, to, type) {{\n");
            formScript.AppendFormat("       var elemFrom = document.getElementById(from);\n");
            formScript.AppendFormat("       var elemTo = document.getElementById(to);\n");
            formScript.AppendFormat("       if(elemFrom && elemTo) {{\n");
            formScript.AppendFormat("          var val = '';\n");
            formScript.AppendFormat("          switch(type) {{\n");
            //If the control is text 
            formScript.AppendFormat("              case 'text':\n");
            formScript.AppendFormat("              val = elemFrom.value;\n");
            formScript.AppendFormat("              break;\n");
            //If the control is dropdown 
            formScript.AppendFormat("              case 'dropdown':\n");
            formScript.AppendFormat("              val = elemFrom.options[elemFrom.selectedIndex].value;\n");
            formScript.AppendFormat("              break;\n");
            formScript.AppendFormat("          }}\n");
            formScript.AppendFormat("          elemTo.value = val;\n");
            formScript.AppendFormat("       }}\n");
            formScript.AppendFormat("    }}\n");
            formScript.AppendFormat("</script>\n");

            tmpS.Append(formScript.ToString());

            return tmpS.ToString();

        }

        /// <summary>
        /// Inputs the card HTML.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="Validate">if set to <c>true</c> [validate].</param>
        /// <param name="CheckForTerms">if set to <c>true</c> [check for terms].</param>
        /// <returns></returns>
        public String InputCardHTML(Customer ThisCustomer, bool Validate, bool CheckForTerms)
        {
            StringBuilder tmpS = new StringBuilder(1000);

            tmpS.Append("<script type=\"text/javascript\" language=\"Javascript\" src=\"jscripts/tooltip.js\" /></script> ");
            tmpS.Append("<div class='form card-form'>");
            // Credit Card fields

            if (CardExpirationMonth == "")
            {
                CardExpirationMonth = CommonLogic.ParamsCanBeDangerousContent("CardExpirationMonth");
            }

            if (CardExpirationYear == "")
            {
                CardExpirationYear = CommonLogic.ParamsCanBeDangerousContent("CardExpirationYear");
            }

            if (CardType == "")
            {
                CardType = CommonLogic.ParamsCanBeDangerousContent("CardType");
            }

            if (CardName == "")
            {
                CardName = CommonLogic.ParamsCanBeDangerousContent("CardName");
            }

            if (CardNumber == "")
            {
                CardNumber = CommonLogic.ParamsCanBeDangerousContent("CardNumber");
            }

            if (CardExpirationMonth == "")
            {
                CardExpirationMonth = CommonLogic.ParamsCanBeDangerousContent("CardExpirationMonth");
            }

            string CardExtraCode = AppLogic.SafeDisplayCardExtraCode(AppLogic.GetCardExtraCodeFromSession(ThisCustomer));
            if (CardExtraCode == "")
            {
                CardExtraCode = CommonLogic.ParamsCanBeDangerousContent("CardExtraCode");
            }

            if (ThisCustomer.MasterShouldWeStoreCreditCardInfo || CommonLogic.GetThisPageName(false).Equals("checkoutpayment.aspx", StringComparison.InvariantCultureIgnoreCase) || CommonLogic.GetThisPageName(false).Equals("editaddressrecurring.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("       <label>" + AppLogic.GetString("address.cs.23", m_SkinID, m_LocaleSetting) + "</label>\n");
                tmpS.Append("        <input type=\"text\" name=\"CardName\" id=\"CardName\" class=\"form-control\" maxlength=\"100\" value=\"" + HttpContext.Current.Server.HtmlEncode(CardName.Trim()) + "\">\n");
                if (Validate)
                {
                    tmpS.Append("        <input type=\"hidden\" name=\"CardName_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.24", m_SkinID, m_LocaleSetting) + "]\">\n");
                }
                tmpS.Append("      </div>");
                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>" + AppLogic.GetString("address.cs.25", m_SkinID, m_LocaleSetting) + "</label>");
                tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardNumber\" id=\"CardNumber\" class=\"form-control\" maxlength=\"20\" value=\"" + AppLogic.SafeDisplayCardNumber(CardNumber, "Address", m_AddressID) + "\"> " + AppLogic.GetString("shoppingcart.cs.106", m_SkinID, m_LocaleSetting) + "\n");
                if (Validate)
                {
                    tmpS.Append("        <input type=\"hidden\" name=\"CardNumber_vldt\" value=\"[req][len=8][blankalert=" + AppLogic.GetString("address.cs.26", m_SkinID, m_LocaleSetting) + "][invalidalert=" + AppLogic.GetString("address.cs.27", m_SkinID, m_LocaleSetting) + "]\">\n");
                }
                tmpS.Append("      </div>");

                if (CommonLogic.GetThisPageName(false).Equals("checkoutpayment.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    tmpS.Append("      <div class='form-group'>");
                    tmpS.Append("        <label>" + AppLogic.GetString("address.cs.28", m_SkinID, m_LocaleSetting) + "</label>");
                    tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardExtraCode\" id=\"CardExtraCode\" class=\"form-control\" maxlength=\"10\" value=\"" + CardExtraCode + "\">\n");
                    tmpS.Append("(<a id=\"aCardCodeToolTip\" href=\"javascript:void(0);\" style=\"cursor: normal;\" >" + AppLogic.GetString("address.cs.50", m_SkinID, m_LocaleSetting) + "</a>)");
                    tmpS.Append("<script type=\"text/javascript\" language=\"Javascript\">");
                    tmpS.Append(" $window_addLoad(function() { { new ToolTip('aCardCodeToolTip', 'CardCodeTooltip', '<iframe width=400 height=370 frameborder=0 marginheight=2 marginwidth=2 scrolling=no src=" + AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/verificationnumber.gif") + "></iframe>'); } }) ");
                    tmpS.Append("</script>");
                    if (Validate)
                    {
                        tmpS.Append("        <input type=\"hidden\" name=\"CardExtraCode_vldt\" value=\"" + CommonLogic.IIF(!AppLogic.AppConfigBool("CardExtraCodeIsOptional"), "[req]", "") + "[len=3][blankalert=" + AppLogic.GetString("address.cs.29", m_SkinID, m_LocaleSetting) + "][invalidalert=" + AppLogic.GetString("address.cs.30", m_SkinID, m_LocaleSetting) + "]\">\n");
                    }
                    tmpS.Append("      </div>");
                }

                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>" + AppLogic.GetString("address.cs.31", m_SkinID, m_LocaleSetting) + "</label>");
                tmpS.Append("        <select class=\"form-control\" name=\"CardType\" id=\"CardType\">");
                tmpS.Append("				<option value=\"\">" + AppLogic.GetString("address.cs.32", m_SkinID, m_LocaleSetting));

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rsCard = DB.GetRS("select * from creditcardtype  with (NOLOCK)  where Accepted=1 order by CardType", dbconn))
                    {
                        while (rsCard.Read())
                        {
                            tmpS.Append("<option value=\"" + DB.RSField(rsCard, "CardType") + "\" " + CommonLogic.IIF(CardType == DB.RSField(rsCard, "CardType"), " selected ", "") + ">" + DB.RSField(rsCard, "CardType") + "</option>\n");
                        }
                    }
                }

                tmpS.Append("              </select>\n");
                tmpS.Append("      </div>");

                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>" + AppLogic.GetString("address.cs.33", m_SkinID, m_LocaleSetting) + "</label>");
                tmpS.Append("        <select class=\"form-control\" name=\"CardExpirationMonth\" id=\"CardExpirationMonth\">");
                tmpS.Append("<option value=\"\">" + AppLogic.GetString("address.cs.34", m_SkinID, m_LocaleSetting));
                for (int i = 1; i <= 12; i++)
                {
                    tmpS.Append("<option value=\"" + i.ToString().PadLeft(2, '0') + "\" " + CommonLogic.IIF(CardExpirationMonth == i.ToString().PadLeft(2, '0'), " selected ", "") + ">" + i.ToString().PadLeft(2, '0') + "</option>");
                }
                tmpS.Append("</select>    <select class=\"form-control\" name=\"CardExpirationYear\" id=\"CardExpirationYear\">");
                tmpS.Append("<option value=\"\" SELECTED>" + AppLogic.GetString("address.cs.35", m_SkinID, m_LocaleSetting));
                for (int y = 0; y <= 10; y++)
                {
                    tmpS.Append("<option value=\"" + (System.DateTime.Now.Year + y).ToString() + "\" " + CommonLogic.IIF(CardExpirationYear == (System.DateTime.Now.Year + y).ToString(), " selected ", "") + ">" + (System.DateTime.Now.Year + y).ToString() + "</option>");
                }
                tmpS.Append("</select>");
                tmpS.Append("      </div>");

                if (AppLogic.AppConfigBool("ShowCardStartDateFields"))
                {
                    tmpS.Append("      <div class='form-group'>");
                    tmpS.Append("        <label>*" + AppLogic.GetString("address.cs.59", m_SkinID, m_LocaleSetting) + "</label>");
                    String CardStartDateMonth = String.Empty;
                    try
                    {
                        CardStartDate.Substring(0, 2);
                    }
                    catch { }
                    String CardStartDateYear = String.Empty;
                    try
                    {
                        CardStartDateYear = CardStartDate.Substring(2, 2);
                    }
                    catch { }

                    tmpS.Append("        <select class=\"form-control\" name=\"CardStartDateMonth\" id=\"CardStartDateMonth\">");
                    tmpS.Append("<option value=\"\">" + AppLogic.GetString("address.cs.34", m_SkinID, m_LocaleSetting));
                    for (int i = 1; i <= 12; i++)
                    {
                        tmpS.Append("<option value=\"" + i.ToString().PadLeft(2, '0') + "\" " + CommonLogic.IIF(CardStartDateMonth == i.ToString().PadLeft(2, '0'), " selected ", "") + ">" + i.ToString().PadLeft(2, '0') + "</option>");
                    }
                    tmpS.Append("</select>    <select class=\"form-control\" name=\"CardStartDateYear\" id=\"CardStartDateYear\">");
                    tmpS.Append("<option value=\"\" SELECTED>" + AppLogic.GetString("address.cs.35", m_SkinID, m_LocaleSetting));
                    for (int y = -4; y <= 10; y++)
                    {
                        tmpS.Append("<option value=\"" + (System.DateTime.Now.Year + y).ToString().Substring(2, 2) + "\" " + CommonLogic.IIF(CardStartDateYear == (System.DateTime.Now.Year + y).ToString(), " selected ", "") + ">" + (System.DateTime.Now.Year + y).ToString() + "</option>");
                    }
                    tmpS.Append("</select>");
                    tmpS.Append("      </div>");


                    tmpS.Append("      <div class='form-group'>");
                    tmpS.Append("        <label>" + AppLogic.GetString("address.cs.61", m_SkinID, m_LocaleSetting) + "</label>");
                    tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardIssueNumber\" id=\"CardIssueNumber\" class=\"form-control\" maxlength=\"2\" value=\"" + CardIssueNumber + "\"> " + AppLogic.GetString("address.cs.63", m_SkinID, m_LocaleSetting) + "\n");
                    tmpS.Append("      </div>");
                }
            }
            tmpS.Append("</div>");
            return tmpS.ToString();
        }

        /// <summary>
        /// Inputs the card HTML.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="Validate">if set to <c>true</c> [validate].</param>
        /// <param name="CheckForTerms">if set to <c>true</c> [check for terms].</param>
        /// <param name="populatecardnumber">if set to <c>true</c> [populatecardnumber].</param>
        /// <returns></returns>
        public String InputCardHTML(Customer ThisCustomer, bool Validate, bool CheckForTerms, bool populatecardnumber)
        {
            StringBuilder tmpS = new StringBuilder(1000);

            tmpS.Append("<script type=\"text/javascript\" language=\"Javascript\" src=\"jscripts/tooltip.js\" /></script> ");
            tmpS.Append("<div class='form card-form'>");

            // Credit Card fields

            if (CardExpirationMonth == "")
            {
                CardExpirationMonth = CommonLogic.ParamsCanBeDangerousContent("CardExpirationMonth");
            }

            if (CardExpirationYear == "")
            {
                CardExpirationYear = CommonLogic.ParamsCanBeDangerousContent("CardExpirationYear");
            }

            if (CardType == "")
            {
                CardType = CommonLogic.ParamsCanBeDangerousContent("CardType");
            }

            if (CardName == "")
            {
                CardName = CommonLogic.ParamsCanBeDangerousContent("CardName");
            }

            if (CardExpirationMonth == "")
            {
                CardExpirationMonth = CommonLogic.ParamsCanBeDangerousContent("CardExpirationMonth");
            }

            string CardExtraCode = AppLogic.SafeDisplayCardExtraCode(AppLogic.GetCardExtraCodeFromSession(ThisCustomer));
            if (CardExtraCode == "")
            {
                CardExtraCode = CommonLogic.ParamsCanBeDangerousContent("CardExtraCode");
            }

            if (ThisCustomer.MasterShouldWeStoreCreditCardInfo || CommonLogic.GetThisPageName(false).Equals("checkoutpayment.aspx", StringComparison.InvariantCultureIgnoreCase) || CommonLogic.GetThisPageName(false).Equals("editaddressrecurring.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>" + AppLogic.GetString("address.cs.23", m_SkinID, m_LocaleSetting) + "</label>\n");

                tmpS.Append("        <input type=\"text\" name=\"CardName\" id=\"CardName\" class=\"form-control\" maxlength=\"100\" value=\"" + HttpContext.Current.Server.HtmlEncode(CardName.Trim()) + "\">\n");
                if (Validate)
                {
                    tmpS.Append("        <input type=\"hidden\" name=\"CardName_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.24", m_SkinID, m_LocaleSetting) + "]\">\n");
                }

                tmpS.Append("      </div>");

                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>" + AppLogic.GetString("address.cs.25", m_SkinID, m_LocaleSetting) + "</label>");
    
                String CardNumberForDisplay;
                if (CardNumber == "" && CommonLogic.ParamsCanBeDangerousContent("CardNumber") != "")
                {
                    // Form submitted with card number, but not saved to billing address,
                    // therefore we need to display the RAW card number again in the form.
                    CardNumberForDisplay = CommonLogic.ParamsCanBeDangerousContent("CardNumber");
                }
                else
                {
                    CardNumberForDisplay = CommonLogic.IIF(populatecardnumber, AppLogic.SafeDisplayCardNumber(CardNumber, "Address", m_AddressID), "");
                }
                tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardNumber\" id=\"CardNumber\" class=\"form-control\" maxlength=\"20\" value=\"" + CardNumberForDisplay + "\"> " + AppLogic.GetString("shoppingcart.cs.106", m_SkinID, m_LocaleSetting) + "\n");
                if (Validate)
                {
                    tmpS.Append("        <input type=\"hidden\" name=\"CardNumber_vldt\" value=\"[req][len=8][blankalert=" + AppLogic.GetString("address.cs.26", m_SkinID, m_LocaleSetting) + "][invalidalert=" + AppLogic.GetString("address.cs.27", m_SkinID, m_LocaleSetting) + "]\">\n");
                }
                tmpS.Append("      </div>");

                if (CommonLogic.GetThisPageName(false).Equals("checkoutpayment.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    tmpS.Append("      <div class='form-group'>");
                    tmpS.Append("        <label>" + AppLogic.GetString("address.cs.28", m_SkinID, m_LocaleSetting) + "</label>");
                    tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardExtraCode\" id=\"CardExtraCode\" class=\"form-control\" maxlength=\"10\" value=\"" + CommonLogic.IIF(populatecardnumber, CardExtraCode, "") + "\">\n");
                    tmpS.Append("(<a id=\"aCardCodeToolTip\" href=\"javascript:void(0);\" style=\"cursor: normal;\" >" + AppLogic.GetString("address.cs.50", m_SkinID, m_LocaleSetting) + "</a>)");


                    tmpS.Append("<script type=\"text/javascript\" language=\"Javascript\">");
                    tmpS.Append(" $window_addLoad(function() { { new ToolTip('aCardCodeToolTip', 'CardCodeTooltip', '<iframe width=400 height=370 frameborder=0 marginheight=2 marginwidth=2 scrolling=no src=" + AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/verificationnumber.gif") + "></iframe>'); } }) ");
                    tmpS.Append("</script>");
                    if (Validate)
                    {
                        tmpS.Append("        <input type=\"hidden\" name=\"CardExtraCode_vldt\" value=\"" + CommonLogic.IIF(!AppLogic.AppConfigBool("CardExtraCodeIsOptional"), "[req]", "") + "[len=3][blankalert=" + AppLogic.GetString("address.cs.29", m_SkinID, m_LocaleSetting) + "][invalidalert=" + AppLogic.GetString("address.cs.30", m_SkinID, m_LocaleSetting) + "]\">\n");
                    }
                    tmpS.Append("      </div>");
                }

                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("        <label>" + AppLogic.GetString("address.cs.31", m_SkinID, m_LocaleSetting) + "</label>");
                tmpS.Append("        <select class=\"form-control\" name=\"CardType\" id=\"CardType\">");
                tmpS.Append("				<option value=\"\">" + AppLogic.GetString("address.cs.32", m_SkinID, m_LocaleSetting));

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rsCard = DB.GetRS("select * from creditcardtype  with (NOLOCK)  where Accepted=1 order by CardType", dbconn))
                    {
                        while (rsCard.Read())
                        {
                            tmpS.Append("<option value=\"" + DB.RSField(rsCard, "CardType") + "\" " + CommonLogic.IIF(CardType == DB.RSField(rsCard, "CardType"), " selected ", "") + ">" + DB.RSField(rsCard, "CardType") + "</option>\n");
                        }
                    }
                }

                tmpS.Append("              </select>\n");
                tmpS.Append("      </div>");

                tmpS.Append("      <div class='form-group'>");
                tmpS.Append("       <label>" + AppLogic.GetString("address.cs.33", m_SkinID, m_LocaleSetting) + "</label>");
                tmpS.Append("        <select class=\"form-control\" name=\"CardExpirationMonth\" id=\"CardExpirationMonth\">");
                tmpS.Append("<option value=\"\">" + AppLogic.GetString("address.cs.34", m_SkinID, m_LocaleSetting));
                for (int i = 1; i <= 12; i++)
                {
                    tmpS.Append("<option value=\"" + i.ToString().PadLeft(2, '0') + "\" " + CommonLogic.IIF(CardExpirationMonth == i.ToString().PadLeft(2, '0'), " selected ", "") + ">" + i.ToString().PadLeft(2, '0') + "</option>");
                }
                tmpS.Append("</select>    <select class=\"form-control\" name=\"CardExpirationYear\" id=\"CardExpirationYear\">");
                tmpS.Append("<option value=\"\" SELECTED>" + AppLogic.GetString("address.cs.35", m_SkinID, m_LocaleSetting));
                for (int y = 0; y <= 10; y++)
                {
                    tmpS.Append("<option value=\"" + (System.DateTime.Now.Year + y).ToString() + "\" " + CommonLogic.IIF(CardExpirationYear == (System.DateTime.Now.Year + y).ToString(), " selected ", "") + ">" + (System.DateTime.Now.Year + y).ToString() + "</option>");
                }
                tmpS.Append("</select>");
                tmpS.Append("    </div>\n");

                if (AppLogic.AppConfigBool("ShowCardStartDateFields"))
                {
                    tmpS.Append("      <div class='form-group'>");
                    tmpS.Append("        <label>*" + AppLogic.GetString("address.cs.59", m_SkinID, m_LocaleSetting) + "</label>");
     
                    String CardStartDateMonth = String.Empty;
                    try
                    {
                        CardStartDate.Substring(0, 2);
                    }
                    catch { }
                    String CardStartDateYear = String.Empty;
                    try
                    {
                        CardStartDateYear = CardStartDate.Substring(2, 2);
                    }
                    catch { }

                    tmpS.Append("        <select class=\"form-control\" name=\"CardStartDateMonth\" id=\"CardStartDateMonth\">");
                    tmpS.Append("<option value=\"\">" + AppLogic.GetString("address.cs.34", m_SkinID, m_LocaleSetting));
                    for (int i = 1; i <= 12; i++)
                    {
                        tmpS.Append("<option value=\"" + i.ToString().PadLeft(2, '0') + "\" " + CommonLogic.IIF(CardStartDateMonth == i.ToString().PadLeft(2, '0'), " selected ", "") + ">" + i.ToString().PadLeft(2, '0') + "</option>");
                    }
                    tmpS.Append("</select>    <select class=\"form-control\" name=\"CardStartDateYear\" id=\"CardStartDateYear\">");
                    tmpS.Append("<option value=\"\" SELECTED>" + AppLogic.GetString("address.cs.35", m_SkinID, m_LocaleSetting));
                    for (int y = -4; y <= 10; y++)
                    {
                        tmpS.Append("<option value=\"" + (System.DateTime.Now.Year + y).ToString().Substring(2, 2) + "\" " + CommonLogic.IIF(CardStartDateYear == (System.DateTime.Now.Year + y).ToString(), " selected ", "") + ">" + (System.DateTime.Now.Year + y).ToString() + "</option>");
                    }
                    tmpS.Append("</select>");;
                    tmpS.Append("      </div>");

                    tmpS.Append("      <div class='form-group'>");
                    tmpS.Append("        <label>" + AppLogic.GetString("address.cs.61", m_SkinID, m_LocaleSetting) + "</label>");
                    tmpS.Append("        <input type=\"text\" autocomplete=\"off\" name=\"CardIssueNumber\" id=\"CardIssueNumber\" class=\"form-control\" maxlength=\"2\" value=\"" + CardIssueNumber + "\"> " + AppLogic.GetString("address.cs.63", m_SkinID, m_LocaleSetting) + "\n");
                    tmpS.Append("      </div>");
                }
            }
            tmpS.Append("</div>");
            return tmpS.ToString();
        }

        /// <summary>
        /// Inputs the E-Check HTML.
        /// </summary>
        /// <param name="Validate">if set to <c>true</c> [validate].</param>
        /// <returns></returns>
        public string InputECheckHTML(bool Validate)
        {
            StringBuilder tmpS = new StringBuilder(1000);

            tmpS.Append("<div class='form card-form'>");
            tmpS.Append("      <div class='form-group'>");
            tmpS.Append("    <label>" + AppLogic.GetString("address.cs.36", m_SkinID, m_LocaleSetting) + "</label>");
            tmpS.Append("    <input type=\"text\" name=\"ECheckBankAccountName\" id=\"ECheckBankAccountName\" class=\"form-control\" maxlength=\"50\" value=\"" + HttpContext.Current.Server.HtmlEncode(m_FirstName + " " + m_LastName) + "\">\n");
            if (Validate)
            {
                tmpS.Append("    <input type=\"hidden\" name=\"ECheckBankAccountName_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.37", m_SkinID, m_LocaleSetting) + "]\">\n");
            }
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group'>");
            tmpS.Append("       <label>" + AppLogic.GetString("address.cs.38", m_SkinID, m_LocaleSetting) + "</label>\n");
            tmpS.Append("        <input type=\"text\" name=\"ECheckBankName\" id=\"ECheckBankName\" class=\"form-control\" maxlength=\"50\" value=\"" + HttpContext.Current.Server.HtmlEncode(ECheckBankName) + "\"> " + AppLogic.GetString("address.cs.40", m_SkinID, m_LocaleSetting) + "\n");
            if (Validate)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"ECheckBankName_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.39", m_SkinID, m_LocaleSetting) + "]\">\n");
            }
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.41", m_SkinID, m_LocaleSetting) + "</label>");
            tmpS.Append("        <img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/check_aba.gif") + "\"><input type=\"text\" autocomplete=\"off\" name=\"ECheckBankABACode\" id=\"ECheckBankABACode\" class=\"form-control\" maxlength=\"9\" value=\"" + HttpContext.Current.Server.HtmlEncode(ECheckBankABACodeMasked) + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/check_aba.gif", m_LocaleSetting) + "\"> " + AppLogic.GetString("address.cs.42", m_SkinID, m_LocaleSetting) + "\n");
            if (Validate)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"ECheckBankABACode_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.43", m_SkinID, m_LocaleSetting) + "]\">\n");
            }
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.44", m_SkinID, m_LocaleSetting) + "</label>");
            tmpS.Append("<input type=\"text\" autocomplete=\"off\" name=\"ECheckBankAccountNumber\" id=\"ECheckBankAccountNumber\" class=\"form-control\" maxlength=\"25\" value=\"" + HttpContext.Current.Server.HtmlEncode(ECheckBankAccountNumberMasked) + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/check_account.gif") + "\"> " + AppLogic.GetString("address.cs.45", m_SkinID, m_LocaleSetting) + "\n");
            if (Validate)
            {
                tmpS.Append("        <input type=\"hidden\" name=\"ECheckBankAccountNumber_vldt\" value=\"[req][blankalert=" + AppLogic.GetString("address.cs.46", m_SkinID, m_LocaleSetting) + "]\">\n");
            }
            tmpS.Append("      </div>");

            tmpS.Append("      <div class='form-group'>");
            tmpS.Append("        <label>" + AppLogic.GetString("address.cs.47", m_SkinID, m_LocaleSetting) + "</label>");
            tmpS.Append("        <select class=\"form-control\" name=\"ECheckBankAccountType\" class=\"form-control\" id=\"ECheckBankAccountType\">");
            tmpS.Append("          <OPTION VALUE=\"CHECKING\" selected>CHECKING</OPTION>");
            tmpS.Append("		  		<OPTION VALUE=\"SAVINGS\">SAVINGS</OPTION>");
            tmpS.Append("			  	<OPTION VALUE=\"BUSINESS CHECKING\">BUSINESS CHECKING</OPTION>");
            tmpS.Append("        </select>\n");
            tmpS.Append("      </div>");

            tmpS.Append("	  <div class='form-text'>" + String.Format(AppLogic.GetString("address.cs.48", m_SkinID, m_LocaleSetting), AppLogic.LocateImageURL("App_Themes/skin_" + m_SkinID.ToString() + "/images/check_micr.gif")) + "</div>");
            tmpS.Append("</div>");
            return tmpS.ToString();
        }

        /// <summary>
        /// Gets the  address select list.
        /// </summary>
        /// <param name="ThisCustomer">The this customer.</param>
        /// <param name="RenamePrimary">if set to <c>true</c> [rename primary].</param>
        /// <param name="SelectName">Name of the select.</param>
        /// <param name="AddPrimary">if set to <c>true</c> [add primary].</param>
        /// <param name="NumNonDefaultFound">The number of non default found.</param>
        /// <returns></returns>
        static public String StaticGetAddressSelectList(Customer ThisCustomer, bool RenamePrimary, String SelectName, bool AddPrimary, out int NumNonDefaultFound)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            NumNonDefaultFound = 0;
            tmpS.Append("<select class=\"form-control\" name=\"" + SelectName + "\" id=\"" + SelectName + "\">\n");
            if (AddPrimary)
            {
                String s = String.Empty;
                if (ThisCustomer.PrimaryShippingAddressID != 0)
                {
                    String nm = AppLogic.GetString("address.cs.51", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    tmpS.Append("<option value=\"" + ThisCustomer.PrimaryShippingAddressID.ToString() + "\">" + HttpContext.Current.Server.HtmlEncode(nm) + "</option>");
                }
                if (ThisCustomer.PrimaryBillingAddressID != 0 && ThisCustomer.PrimaryBillingAddressID != ThisCustomer.PrimaryShippingAddressID)
                {
                    String nm = AppLogic.GetString("address.cs.52", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    tmpS.Append("<option value=\"" + ThisCustomer.PrimaryBillingAddressID.ToString() + "\">" + HttpContext.Current.Server.HtmlEncode(nm) + "</option>");
                }
            }
            String sql = "select * from Address  with (NOLOCK)  where CustomerID=" + ThisCustomer.CustomerID.ToString() + " and AddressID not in (" + ThisCustomer.PrimaryBillingAddressID.ToString() + "," + ThisCustomer.PrimaryShippingAddressID.ToString() + ") order by nickname, firstname, lastname";

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        int addrID = DB.RSFieldInt(rs, "AddressID");
                        NumNonDefaultFound++; // tick non-primary counter
                        String nm = DB.RSField(rs, "NickName");
                        if (nm.Length == 0)
                        {
                            nm = (DB.RSField(rs, "FirstName") + " " + DB.RSField(rs, "LastName")).Trim();
                        }
                        tmpS.Append("<option value=\"" + addrID.ToString() + "\">" + HttpContext.Current.Server.HtmlEncode(nm) + "</option>");
                    }
                }
            }

            tmpS.Append("</select>\n");
            return tmpS.ToString();
        }
    }

    public class Addresses : ArrayList
    {
        public Addresses() { }

        public new Address this[int index]
        {
            get
            {
                return (Address)base[index];
            }
            set
            {
                base[index] = value;
            }
        }

        /// <summary>
        /// Loads the customer.
        /// </summary>
        /// <param name="CustomerID">The customer ID.</param>
        public void LoadCustomer(int CustomerID)
        {
            string sql = String.Format("select AddressID from Address with (NOLOCK) where CustomerID={0}", CustomerID.ToString());

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    while (rs.Read())
                    {
                        int AddressID = DB.RSFieldInt(rs, "AddressID");
                        Address newAddress = new Address();
                        newAddress.LoadFromDB(AddressID);
                        Add(newAddress);
                    }
                }
            }
        }

    }
}
