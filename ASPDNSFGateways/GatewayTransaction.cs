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
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontGateways
{
    #region enums

    /// <summary>
    /// Used to determine the payment gateway transaction mode
    /// 0 = unknown (this should only happen if the admin specifies an invalid transactionmode)
    /// 1 = auth - the order is authorized, but not captured
    /// 2 = authcapture - the order is authorized and captured
    /// </summary>
    public enum TransactionModeEnum : int
    {
        unknown = 0,
        auth = 1,
        authcapture = 2
    }



    #endregion
    
    /// <summary>
    /// Class for payment gateway transactions
    /// This object gets passed to the individual payment gateways responsible for processing the transaction
    /// </summary>
    public class GatewayTransaction
    {
        #region private member variables

        private string m_gateway = AppLogic.CleanPaymentGateway(AppLogic.AppConfig("PaymentGateway"));
        private string m_backupGateway = AppLogic.CleanPaymentGateway(AppLogic.AppConfig("PaymentGatewayBackup"));
        private string m_gatewayUsed;
        private int m_orderNumber = 0;
        private int m_customerID = 0;
        private decimal m_orderTotal = 0.00M;
        private bool m_useLiveTransactions = false;
        private TransactionModeEnum m_transactionMode = TransactionModeEnum.unknown;
        private Address m_billingAddress;
        private Address m_shippingAddress;
        private string m_cavv;
        private string m_cv2;
        private string m_eci;
        private string m_xid;
        private string m_avsResult;
        private string m_authorizationResult;
        private string m_authorizationCode;
        private string m_authorizationTransactionID;
        private string m_transactionCommand;
        private string m_transactionResponse;
        private string m_status;

        #endregion

        #region constructors

        /// <summary>
        /// Instantiates a new instance of the GatewayTransaction object
        /// </summary>
        /// <param name="gwToUse">The payment gateway you wish to use.
        /// This should be a cleaned value (no spaces or periods) and exactly match the name of the payment gateway class to call (case insensitive)</param>
        /// <param name="ordNo">The order number to process</param>
        /// <param name="custID">The customer id of the customer placing the order</param>
        /// <param name="ordTotal">The order total to process</param>
        /// <param name="billAddr">The billing address associated with the order</param>
        /// <param name="shipAddr">The shipping address associated with the order</param>
        /// <param name="cv2">The card CV2 code (card extra code)</param>
        /// <param name="ec">The ECI code (used for VBV/3D Secure transactions)</param>
        /// <param name="xi">The XID code (used for VBV/3D Secure transactions)</param>
        public GatewayTransaction(int ordNo, int custID, decimal ordTotal, Address billAddr, Address shipAddr, string cavv, string cv2, string ec, string xi)
        {
            m_orderNumber = ordNo;
            m_customerID = custID;
            m_orderTotal = ordTotal;
            m_useLiveTransactions = AppLogic.AppConfigBool("UseLiveTransactions");
            m_transactionMode = (TransactionModeEnum)Enum.Parse(typeof (TransactionModeEnum), AppLogic.AppConfig("TransactionMode").ToLowerInvariant().Replace(" ",""));
            m_billingAddress = billAddr;

            if (shipAddr != null)
            {
                m_shippingAddress = shipAddr;
            }
            else
            {
                m_shippingAddress = billAddr;
            }

            m_cavv = cavv;
            m_cv2 = cv2;
            m_eci = ec;
            m_xid = xi;

        }
        
        #endregion

        #region static methods

        /// <summary>
        /// Public method responsible for calling the payment gateway
        /// Calls private method CallGateway to perform the actual transaction processing
        /// </summary>
        public void Process()
        {
            // load the correct payment gateway based on appconfig settings
            // a failure here probably means that they entered an invalid appconfig, which exception logging will pick up automatically

            if (AppLogic.AppConfig("PaymentGatewayBackup").Length > 0)
            {
                m_gatewayUsed = this.GatewayToUsePrimary;
                int retryPrimary = AppLogic.AppConfigUSInt("PaymentGateway.PrimaryRetries");
                int retryBackup = AppLogic.AppConfigUSInt("PaymentGateway.BackupRetries");
                bool attemptBackup = true;
                
                // attempt the primary gateway first
                for (int i = 1; i <= retryPrimary; i++)
                {
                    try
                    {
                        // call the payment gateway
                        m_status = CallGateway(this.GatewayToUsePrimary);

                        // If there is no exception, we've contacted the gateway and received a valid response
                        // Just because we received a valid response does not mean the transaction was authorized!
                        attemptBackup = false;
                        break;
                    }
                    catch (Exception ex)
                    {
                        // calling the payment gateway failed.  Log any information we can.
                        SysLog.LogMessage(GenerateLogMessage(this.GatewayToUseBackup), SysLog.FormatExceptionForLog(ex), MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                        m_status = "ERROR CALLING PAYMENT GATEWAY";
                    }
                }

                if (attemptBackup) //we never received a response from the primary gateway
                {
                    for (int i = 1; i <= retryBackup; i++)
                    {
                        try
                        {
                            //call the payment gateway
                            m_status = CallGateway(this.GatewayToUseBackup);

                            // If there is no exception, we've contacted the gateway and received a valid response
                            // Just because we received a valid response does not mean the transaction was authorized!
                            attemptBackup = false;
                            m_gatewayUsed = this.GatewayToUseBackup;
                            break;
                        }
                        catch(Exception ex)
                        {
                            // calling the payment gateway failed.  Log any information we can.
                            SysLog.LogMessage(GenerateLogMessage(this.GatewayToUsePrimary), SysLog.FormatExceptionForLog(ex), MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                            m_status = "ERROR CALLING PAYMENT GATEWAY";
                        }
                    }
                }
            }
            else  // only a single payment gateway configured.  Bypass backup gateway logic
            {
                try
                {
                    m_status = CallGateway(GatewayToUsePrimary);
                    m_gatewayUsed = this.GatewayToUsePrimary;
                }
                catch(Exception ex)
                {
                    SysLog.LogMessage(GenerateLogMessage(this.GatewayToUsePrimary), SysLog.FormatExceptionForLog(ex), MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
                    m_status = "ERROR CALLING PAYMENT GATEWAY";
                }

            }



        }

        #endregion

        #region non-static methods

        /// <summary>
        /// Private method to call the gateway
        /// </summary>
        /// <param name="gateway">Payment gateway to call</param>
        /// <returns></returns>
        private string CallGateway(string gateway)
        {
            String result = String.Empty;
            GatewayProcessor processor = GatewayLoader.GetProcessor(gateway);
            
            result = processor.ProcessCard(this.OrderNumber,
                                        this.CustomerID,
                                        this.OrderTotal,
                                        this.UseLiveTransactions,
                                        this.TransactionMode,
                                        this.BillingAddress,
                                        this.CV2,
                                        this.ShippingAddress,
                                        this.CAVV,
                                        this.ECI,
                                        this.XID,
                                        out m_avsResult,
                                        out m_authorizationResult,
                                        out m_authorizationCode,
                                        out m_authorizationTransactionID,
                                        out m_transactionCommand,
                                        out m_transactionResponse);

            // set the gatewayused property so we know for sure which gateway was called!
            m_gatewayUsed = gateway;

            return result;
        }

        /// <summary>
        /// Formats an exception returned by the gateway and adds some additional information to it for troubleshooting later
        /// </summary>
        /// <param name="gateway">name of the gateway that threw the exception</param>
        /// <returns>Formatted log message details</returns>
        private string GenerateLogMessage(string gateway)
        {
            String messageSeperator = "  |  ";
            StringBuilder logMessage = new StringBuilder();
            logMessage.Append(AppLogic.GetStringForDefaultLocale("gatewaytransaction.cs.1") + "  " + gateway);
            logMessage.Append(messageSeperator);
            logMessage.Append(AppLogic.GetStringForDefaultLocale("gatewaytransaction.cs.2") + "  " + this.OrderNumber);
            logMessage.Append(messageSeperator);
            logMessage.Append(AppLogic.GetStringForDefaultLocale("gatewaytransaction.cs.3") + "  " + this.CustomerID);
            logMessage.Append(messageSeperator);
            logMessage.Append(AppLogic.GetStringForDefaultLocale("gatewaytransaction.cs.4"));
            logMessage.Append(messageSeperator);

            return logMessage.ToString();
        }

        #endregion

        

        #region properties

        /// <summary>
        /// The primary payment gateway as specified in AppConfigs
        /// This property is read-only
        /// </summary>
        public string GatewayToUsePrimary
        {
            get { return m_gateway; }
        }

        /// <summary>
        /// The backup payment gateway as configured in AppConfigs
        /// This property is read-only
        /// </summary>
        public string GatewayToUseBackup
        {
            get { return m_backupGateway; }
        }

        /// <summary>
        /// Read only property returning the actual payment gateway used
        /// This may differ from the payment gateway specified in the payment gateway AppConfig if the customer has chosen to use a backup gateway
        /// </summary>
        public string GatewayUsed
        {
            get { return m_gatewayUsed; }
        }

        /// <summary>
        /// Gets or sets the order number for the gateway transaction
        /// </summary>
        public int OrderNumber
        {
            get { return m_orderNumber; }
            set { m_orderNumber = value; }
        }

        /// <summary>
        /// Gets or sets the customer id the transaction is being processed for
        /// </summary>
        public int CustomerID
        {
            get { return m_customerID; }
            set { m_customerID = value; }
        }

        /// <summary>
        /// Gets or sets the order total for the transaction
        /// </summary>
        public decimal OrderTotal
        {
            get { return m_orderTotal; }
            set { m_orderTotal = value; }
        }

        /// <summary>
        /// Gets or sets a property that determines whether transactions are processed in live or test mode
        /// </summary>
        public bool UseLiveTransactions
        {
            get { return m_useLiveTransactions; }
            set { m_useLiveTransactions = value; }
        }

        /// <summary>
        /// Gets or sets a property that determines whether the transaction is authorize only or authorize and capture
        /// </summary>
        public TransactionModeEnum TransactionMode
        {
            get { return m_transactionMode; }
            set { m_transactionMode = value; }
        }

        /// <summary>
        /// Gets or sets the billing address for the order the transaction is being processed for
        /// </summary>
        public Address BillingAddress
        {
            get { return m_billingAddress; }
            set { m_billingAddress = value; }
        }

        /// <summary>
        /// Gets or sets the shipping address for the order the transaction is being processed for
        /// </summary>
        public Address ShippingAddress
        {
            get { return m_shippingAddress; }
            set { m_shippingAddress = value; }
        }

        /// <summary>
        /// Gets or sets he CV2/Card Extra code for the credit card number being used to process the transaction
        /// </summary>
        public string CV2
        {
            get { return m_cv2; }
            set { m_cv2 = value; }
        }

        /// <summary>
        /// Gets or sets the CAVV code (used for VBV/3D Secure transactions)
        /// </summary>
        public string CAVV
        {
            get { return m_cavv; }
            set { m_cavv = value; }
        }

        /// <summary>
        /// The ECI code (used for VBV/3D Secure transactions)
        /// </summary>
        public string ECI
        {
            get { return m_eci; }
            set { m_eci = value; }
        }

        /// <summary>
        /// Gets or sets the XID code (used for VBV/3D Secure transactions)
        /// </summary>
        public string XID
        {
            get { return m_xid; }
            set { m_xid = value; }
        }

        /// <summary>
        /// Gets the AVS Result returned by the gateway if supported
        /// This property is read-only
        /// </summary>
        public string AVSResult
        {
            get { return m_avsResult; }
        }

        /// <summary>
        /// Gets the authorization result returned by the gateway
        /// This property is read-only
        /// </summary>
        public string AuthorizationResult
        {
            get { return m_authorizationResult; }
        }

        /// <summary>
        /// Gets the authorization code returned by the gateway if available
        /// This property is read-only
        /// </summary>
        public string AuthorizationCode
        {
            get { return m_authorizationCode; }
        }

        /// <summary>
        /// Gets the authorization transaction id returned by the gateway if available
        /// This property is read-only
        /// </summary>
        public string AuthorizationTransactionID
        {
            get { return m_authorizationTransactionID; }
        }

        /// <summary>
        /// Gets the transaction command (request) sent to the payment gateway
        /// This property is read-only
        /// </summary>
        public string TransactionCommand
        {
            get { return m_transactionCommand; }
        }

        /// <summary>
        /// Gets the raw transaction response returned by the gateway
        /// This property is read-only
        /// </summary>
        public string TransactionResponse
        {
            get { return m_transactionResponse; }
        }


        /// <summary>
        /// Gets the status of the transaction, which can be used to determine if the transaction was successful or not
        /// This property is read-only
        /// </summary>
        public string Status
        {
            get { return m_status; }
        }

        #endregion

    }
}
