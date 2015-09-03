// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace AspDotNetStorefrontGateways.Processors
{
    public abstract class GatewayProcessor
    {
        public virtual bool ShowCheckoutButton
        {
            get { return true; }
        }
        public virtual RecurringSupportType RecurringSupportType() { return Processors.RecurringSupportType.None; }
        public virtual Boolean RecurringIntervalSupportsMultiplier
        {
            get { return true; }
        }
        public virtual Boolean SupportsEChecks() { return false; }
        public virtual Boolean SupportsPostProcessingEdits() { return true; }
        public virtual Boolean SupportsAdHocOrders() { return true; }
        public virtual Boolean RequiresCCForFurtherProcessing() { return false; }
        public virtual Boolean RequiresFinalization() { return false; }
        public virtual String PromptForSetup() { return string.Empty; }
        public virtual String StringResourceName
        {
            get { return String.Format("gw.{0}.display", this.GatewayIdentifier.ToLower()); }
        }
        public virtual String TypeName
        {
            get
            {
                String type = this.GetType().ToString();
                String[] splitType = type.Split('.');
                return splitType[splitType.Length - 1];
            }
        }
        public virtual String GatewayIdentifier
        {
            get
            {
                return this.TypeName.ToUpperInvariant();
            }
        }
        public virtual String AdministratorSetupPrompt
        {
            get
            {
                return string.Empty;
            }
        }
        public virtual String Version
        {
            get
            {
                return System.Reflection.Assembly.GetAssembly(this.GetType()).GetName().Version.ToString();
            }
        }

        public abstract String CaptureOrder(Order o);
        public abstract String VoidOrder(int OrderNumber);
        public abstract String RefundOrder(int OriginalOrderNumber, int NewOrderNumber, decimal RefundAmount, String RefundReason, Address UseBillingAddress);

        public virtual String ProcessCard(int OrderNumber,
            int CustomerID, Decimal OrderTotal,
            bool useLiveTransactions,
            TransactionModeEnum TransactionMode,
            Address UseBillingAddress,
            String CardExtraCode,
            Address UseShippingAddress,
            String CAVV,
            String ECI,
            String XID,
            out String AVSResult,
            out String AuthorizationResult,
            out String AuthorizationCode,
            out String AuthorizationTransID,
            out String TransactionCommandOut,
            out String TransactionResponse)
        {
            AVSResult = string.Empty;
            AuthorizationResult = string.Empty;
            AuthorizationCode = string.Empty;
            AuthorizationTransID = string.Empty;
            TransactionCommandOut = string.Empty;
            TransactionResponse = string.Empty;

            return string.Empty;
        }

        public virtual String ProcessECheck(int OrderNumber,
            int CustomerID,
            Decimal OrderTotal,
            Address UseBillingAddress,
            Address UseShippingAddress,
            out String AVSResult,
            out String AuthorizationResult,
            out String AuthorizationCode,
            out String AuthorizationTransID,
            out String TransactionCommandOut,
            out String TransactionResponse)
        {
            AVSResult = string.Empty;
            AuthorizationResult = string.Empty;
            AuthorizationCode = string.Empty;
            AuthorizationTransID = string.Empty;
            TransactionCommandOut = string.Empty;
            TransactionResponse = string.Empty;

            return string.Empty;
        }
     
        public virtual string RecurringBillingCreateSubscription(String SubscriptionDescription,
            Customer ThisCustomer,
            Address UseBillingAddress,
            Address UseShippingAddress,
            Decimal RecurringAmount,
            DateTime StartDate,
            int RecurringInterval,
            DateIntervalTypeEnum RecurringIntervalType,
            int OriginalRecurringOrderNumber,
            string XID,
            IDictionary<string, string> TransactionContext,
            out String RecurringSubscriptionID,
            out String RecurringSubscriptionCommand,
            out String RecurringSubscriptionResult)
        {
            RecurringSubscriptionID = string.Empty;
            RecurringSubscriptionCommand = string.Empty;
            RecurringSubscriptionResult = string.Empty;

            return string.Empty;
        }

        public virtual string RecurringBillingCancelSubscription(String RecurringSubscriptionID, int OriginalRecurringOrderNumber,  IDictionary<string, string> TransactionContext)
        {
            return string.Empty;
        }

        public virtual string RecurringBillingAddressUpdate(String RecurringSubscriptionID, int OriginalRecurringOrderNumber, Address UseBillingAddress)
        {
            return string.Empty;
        }

        public virtual String RecurringBillingGetStatusFile()
        {
            return string.Empty;
        }

        public virtual String ProcessingPageRedirect()
        {
            return string.Empty;
        }

        public virtual String CreditCardPaneInfo(int SkinId, Customer ThisCustomer)
        {
            return string.Empty;
        }

        public virtual String ProcessAutoBillStatusFile(String GW, String StatusFile, out String Results, RecurringOrderMgr OrderManager)
        {
            String Status = AppLogic.ro_OK;
            Results = string.Empty;
            return Status;
        }

        public virtual String DisplayName(String LocaleSetting)
        {
            String resource = AppLogic.GetString(StringResourceName, LocaleSetting);
            if (StringResourceName != resource)
            {
                return resource.Replace(" ", " "); // The first one is actually a weird space character.
            }
            return TypeName;
        }

        public virtual List<DateIntervalTypeEnum> GetAllowedRecurringIntervals()
        {
            List<DateIntervalTypeEnum> includedTypes = new List<DateIntervalTypeEnum>();
            includedTypes.Add(DateIntervalTypeEnum.Day);
            includedTypes.Add(DateIntervalTypeEnum.Week);
            includedTypes.Add(DateIntervalTypeEnum.Month);
            includedTypes.Add(DateIntervalTypeEnum.Year);
            return includedTypes;
        }

        public virtual IntervalValidator GetIntervalValidator(DateIntervalTypeEnum intervalType)
        {
            return new IntervalRegExValidator(@"^(\d+)$", "Please enter an integer.", intervalType);
        }

        public virtual IConfigurationAtom GetConfigurationAtom()
        {
            return GetConfigurationAtom(String.Empty);
        }

        public virtual IConfigurationAtom GetConfigurationAtom(String resourceName)
        {
            try
            {
                String resourceFound = null;
                Assembly a = System.Reflection.Assembly.GetAssembly(this.GetType());
                String[] gatewayresources = a.GetManifestResourceNames();

                if (!String.IsNullOrEmpty(resourceName))
                    resourceFound = gatewayresources.FirstOrDefault(rn => rn.ToLower().EndsWith(resourceName.ToLower()));
                else
                {
                    foreach (string rn in gatewayresources)
                    {
                        if (rn.EndsWith("GatewayConfigAtom.xml"))
                        {
                            resourceFound = rn;
                            break;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(resourceFound))
                {
                    using (Stream xmlStream = a.GetManifestResourceStream(resourceFound))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(xmlStream);
                        return new ConfigurationAtom(doc);
                    }
                }
                else
                    return null;
            }
            catch (Exception)
            {
                return new SearchConfigurationAtom(this.TypeName, "", "Configure " + DisplayName(Localization.GetDefaultLocale()));
            }
        }
    }

    public class ValidatorResult
    {
        public virtual String Message { get; protected set; }
        public virtual Boolean IsValid { get; protected set; }
        public ValidatorResult(String message, Boolean isValid)
        {
            Message = message;
            IsValid = isValid;
        }
    }


    public abstract class IntervalValidator
    {
        public virtual DateIntervalTypeEnum IntervalType { get; set; }
        public IntervalValidator(DateIntervalTypeEnum intervalType)
        {
            IntervalType = intervalType;
        }
        public abstract ValidatorResult Validate(string value);
    }

    public class IntervalRegExValidator : IntervalValidator
    {
        public Regex RegEx
        {
            get;
            protected set;
        }
        public String RegExErrorMessage
        {
            get;
            protected set;
        }
        public IntervalRegExValidator(string regEx, string regExErrorMessage, DateIntervalTypeEnum intervalType)
            : base(intervalType)
        {
            RegEx = new Regex(regEx);
            RegExErrorMessage = regExErrorMessage;
        }

        public override ValidatorResult Validate(string value)
        {
            String ErrorMessage = null;
            Boolean regexmatches = RegEx.IsMatch(value);
            if (!regexmatches)
                ErrorMessage = RegExErrorMessage;

            return new ValidatorResult(ErrorMessage, regexmatches);
        }
    }

    public enum RecurringSupportType
    {
        None,
        Normal,
        Extended
    }
}
