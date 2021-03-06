// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.1.
// 
#pragma warning disable 1591

namespace AspDotNetStorefrontGateways.CentralPaymentsSvc {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.ComponentModel;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="PaymentSoap", Namespace="http://pay.centralpayments.net/soap/v6")]
    public partial class Payment : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback DirectPaymentOperationCompleted;
        
        private System.Threading.SendOrPostCallback AuthorizePaymentOperationCompleted;
        
        private System.Threading.SendOrPostCallback CreditPaymentOperationCompleted;
        
        private System.Threading.SendOrPostCallback CapturePaymentOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Payment() {
            this.Url = global::AspDotNetStorefrontGateways.Properties.Settings.Default.AspDotNetStorefrontGateways_CentralPaymentsSvc_Payment;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event DirectPaymentCompletedEventHandler DirectPaymentCompleted;
        
        /// <remarks/>
        public event AuthorizePaymentCompletedEventHandler AuthorizePaymentCompleted;
        
        /// <remarks/>
        public event CreditPaymentCompletedEventHandler CreditPaymentCompleted;
        
        /// <remarks/>
        public event CapturePaymentCompletedEventHandler CapturePaymentCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://pay.centralpayments.net/soap/v6/DirectPayment", RequestNamespace="http://pay.centralpayments.net/soap/v6", ResponseNamespace="http://pay.centralpayments.net/soap/v6", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string DirectPayment(
                    string AssociateName, 
                    string AssociatePassword, 
                    decimal Amount, 
                    string UserEmail, 
                    string FullName, 
                    string StreetAddress, 
                    string City, 
                    string Zip, 
                    string State, 
                    string Country, 
                    string DayPhone, 
                    string CreditCard, 
                    string ExpirationMonth, 
                    string ExpirationYear, 
                    string UDF1, 
                    string UDF2, 
                    string TrackingId, 
                    string BrowserType, 
                    string BrowserEmail, 
                    string IPAddress, 
                    string CardVerification, 
                    string HostName) {
            object[] results = this.Invoke("DirectPayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        Amount,
                        UserEmail,
                        FullName,
                        StreetAddress,
                        City,
                        Zip,
                        State,
                        Country,
                        DayPhone,
                        CreditCard,
                        ExpirationMonth,
                        ExpirationYear,
                        UDF1,
                        UDF2,
                        TrackingId,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginDirectPayment(
                    string AssociateName, 
                    string AssociatePassword, 
                    decimal Amount, 
                    string UserEmail, 
                    string FullName, 
                    string StreetAddress, 
                    string City, 
                    string Zip, 
                    string State, 
                    string Country, 
                    string DayPhone, 
                    string CreditCard, 
                    string ExpirationMonth, 
                    string ExpirationYear, 
                    string UDF1, 
                    string UDF2, 
                    string TrackingId, 
                    string BrowserType, 
                    string BrowserEmail, 
                    string IPAddress, 
                    string CardVerification, 
                    string HostName, 
                    System.AsyncCallback callback, 
                    object asyncState) {
            return this.BeginInvoke("DirectPayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        Amount,
                        UserEmail,
                        FullName,
                        StreetAddress,
                        City,
                        Zip,
                        State,
                        Country,
                        DayPhone,
                        CreditCard,
                        ExpirationMonth,
                        ExpirationYear,
                        UDF1,
                        UDF2,
                        TrackingId,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndDirectPayment(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void DirectPaymentAsync(
                    string AssociateName, 
                    string AssociatePassword, 
                    decimal Amount, 
                    string UserEmail, 
                    string FullName, 
                    string StreetAddress, 
                    string City, 
                    string Zip, 
                    string State, 
                    string Country, 
                    string DayPhone, 
                    string CreditCard, 
                    string ExpirationMonth, 
                    string ExpirationYear, 
                    string UDF1, 
                    string UDF2, 
                    string TrackingId, 
                    string BrowserType, 
                    string BrowserEmail, 
                    string IPAddress, 
                    string CardVerification, 
                    string HostName) {
            this.DirectPaymentAsync(AssociateName, AssociatePassword, Amount, UserEmail, FullName, StreetAddress, City, Zip, State, Country, DayPhone, CreditCard, ExpirationMonth, ExpirationYear, UDF1, UDF2, TrackingId, BrowserType, BrowserEmail, IPAddress, CardVerification, HostName, null);
        }
        
        /// <remarks/>
        public void DirectPaymentAsync(
                    string AssociateName, 
                    string AssociatePassword, 
                    decimal Amount, 
                    string UserEmail, 
                    string FullName, 
                    string StreetAddress, 
                    string City, 
                    string Zip, 
                    string State, 
                    string Country, 
                    string DayPhone, 
                    string CreditCard, 
                    string ExpirationMonth, 
                    string ExpirationYear, 
                    string UDF1, 
                    string UDF2, 
                    string TrackingId, 
                    string BrowserType, 
                    string BrowserEmail, 
                    string IPAddress, 
                    string CardVerification, 
                    string HostName, 
                    object userState) {
            if ((this.DirectPaymentOperationCompleted == null)) {
                this.DirectPaymentOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDirectPaymentOperationCompleted);
            }
            this.InvokeAsync("DirectPayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        Amount,
                        UserEmail,
                        FullName,
                        StreetAddress,
                        City,
                        Zip,
                        State,
                        Country,
                        DayPhone,
                        CreditCard,
                        ExpirationMonth,
                        ExpirationYear,
                        UDF1,
                        UDF2,
                        TrackingId,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName}, this.DirectPaymentOperationCompleted, userState);
        }
        
        private void OnDirectPaymentOperationCompleted(object arg) {
            if ((this.DirectPaymentCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DirectPaymentCompleted(this, new DirectPaymentCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://pay.centralpayments.net/soap/v6/AuthorizePayment", RequestNamespace="http://pay.centralpayments.net/soap/v6", ResponseNamespace="http://pay.centralpayments.net/soap/v6", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string AuthorizePayment(
                    string AssociateName, 
                    string AssociatePassword, 
                    decimal Amount, 
                    string UserEmail, 
                    string FullName, 
                    string StreetAddress1, 
                    string City, 
                    string Zip, 
                    string State, 
                    string Country, 
                    string DayPhone, 
                    string CreditCard, 
                    string ExpirationMonth, 
                    string ExpirationYear, 
                    string UDF1, 
                    string UDF2, 
                    string BrowserType, 
                    string BrowserEmail, 
                    string IPAddress, 
                    string CardVerification, 
                    string HostName) {
            object[] results = this.Invoke("AuthorizePayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        Amount,
                        UserEmail,
                        FullName,
                        StreetAddress1,
                        City,
                        Zip,
                        State,
                        Country,
                        DayPhone,
                        CreditCard,
                        ExpirationMonth,
                        ExpirationYear,
                        UDF1,
                        UDF2,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginAuthorizePayment(
                    string AssociateName, 
                    string AssociatePassword, 
                    decimal Amount, 
                    string UserEmail, 
                    string FullName, 
                    string StreetAddress1, 
                    string City, 
                    string Zip, 
                    string State, 
                    string Country, 
                    string DayPhone, 
                    string CreditCard, 
                    string ExpirationMonth, 
                    string ExpirationYear, 
                    string UDF1, 
                    string UDF2, 
                    string BrowserType, 
                    string BrowserEmail, 
                    string IPAddress, 
                    string CardVerification, 
                    string HostName, 
                    System.AsyncCallback callback, 
                    object asyncState) {
            return this.BeginInvoke("AuthorizePayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        Amount,
                        UserEmail,
                        FullName,
                        StreetAddress1,
                        City,
                        Zip,
                        State,
                        Country,
                        DayPhone,
                        CreditCard,
                        ExpirationMonth,
                        ExpirationYear,
                        UDF1,
                        UDF2,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndAuthorizePayment(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void AuthorizePaymentAsync(
                    string AssociateName, 
                    string AssociatePassword, 
                    decimal Amount, 
                    string UserEmail, 
                    string FullName, 
                    string StreetAddress1, 
                    string City, 
                    string Zip, 
                    string State, 
                    string Country, 
                    string DayPhone, 
                    string CreditCard, 
                    string ExpirationMonth, 
                    string ExpirationYear, 
                    string UDF1, 
                    string UDF2, 
                    string BrowserType, 
                    string BrowserEmail, 
                    string IPAddress, 
                    string CardVerification, 
                    string HostName) {
            this.AuthorizePaymentAsync(AssociateName, AssociatePassword, Amount, UserEmail, FullName, StreetAddress1, City, Zip, State, Country, DayPhone, CreditCard, ExpirationMonth, ExpirationYear, UDF1, UDF2, BrowserType, BrowserEmail, IPAddress, CardVerification, HostName, null);
        }
        
        /// <remarks/>
        public void AuthorizePaymentAsync(
                    string AssociateName, 
                    string AssociatePassword, 
                    decimal Amount, 
                    string UserEmail, 
                    string FullName, 
                    string StreetAddress1, 
                    string City, 
                    string Zip, 
                    string State, 
                    string Country, 
                    string DayPhone, 
                    string CreditCard, 
                    string ExpirationMonth, 
                    string ExpirationYear, 
                    string UDF1, 
                    string UDF2, 
                    string BrowserType, 
                    string BrowserEmail, 
                    string IPAddress, 
                    string CardVerification, 
                    string HostName, 
                    object userState) {
            if ((this.AuthorizePaymentOperationCompleted == null)) {
                this.AuthorizePaymentOperationCompleted = new System.Threading.SendOrPostCallback(this.OnAuthorizePaymentOperationCompleted);
            }
            this.InvokeAsync("AuthorizePayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        Amount,
                        UserEmail,
                        FullName,
                        StreetAddress1,
                        City,
                        Zip,
                        State,
                        Country,
                        DayPhone,
                        CreditCard,
                        ExpirationMonth,
                        ExpirationYear,
                        UDF1,
                        UDF2,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName}, this.AuthorizePaymentOperationCompleted, userState);
        }
        
        private void OnAuthorizePaymentOperationCompleted(object arg) {
            if ((this.AuthorizePaymentCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.AuthorizePaymentCompleted(this, new AuthorizePaymentCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://pay.centralpayments.net/soap/v6/CreditPayment", RequestNamespace="http://pay.centralpayments.net/soap/v6", ResponseNamespace="http://pay.centralpayments.net/soap/v6", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string CreditPayment(string AssociateName, string AssociatePassword, string UserEmail, string Reference, decimal Amount, string UDF1, string UDF2) {
            object[] results = this.Invoke("CreditPayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        UserEmail,
                        Reference,
                        Amount,
                        UDF1,
                        UDF2});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginCreditPayment(string AssociateName, string AssociatePassword, string UserEmail, string Reference, decimal Amount, string UDF1, string UDF2, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("CreditPayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        UserEmail,
                        Reference,
                        Amount,
                        UDF1,
                        UDF2}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndCreditPayment(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void CreditPaymentAsync(string AssociateName, string AssociatePassword, string UserEmail, string Reference, decimal Amount, string UDF1, string UDF2) {
            this.CreditPaymentAsync(AssociateName, AssociatePassword, UserEmail, Reference, Amount, UDF1, UDF2, null);
        }
        
        /// <remarks/>
        public void CreditPaymentAsync(string AssociateName, string AssociatePassword, string UserEmail, string Reference, decimal Amount, string UDF1, string UDF2, object userState) {
            if ((this.CreditPaymentOperationCompleted == null)) {
                this.CreditPaymentOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreditPaymentOperationCompleted);
            }
            this.InvokeAsync("CreditPayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        UserEmail,
                        Reference,
                        Amount,
                        UDF1,
                        UDF2}, this.CreditPaymentOperationCompleted, userState);
        }
        
        private void OnCreditPaymentOperationCompleted(object arg) {
            if ((this.CreditPaymentCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CreditPaymentCompleted(this, new CreditPaymentCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://pay.centralpayments.net/soap/v6/CapturePayment", RequestNamespace="http://pay.centralpayments.net/soap/v6", ResponseNamespace="http://pay.centralpayments.net/soap/v6", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string CapturePayment(string AssociateName, string AssociatePassword, string UserEmail, string Authorization, decimal Amount, string UDF1, string UDF2, string BrowserType, string BrowserEmail, string IPAddress, string CardVerification, string HostName) {
            object[] results = this.Invoke("CapturePayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        UserEmail,
                        Authorization,
                        Amount,
                        UDF1,
                        UDF2,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginCapturePayment(string AssociateName, string AssociatePassword, string UserEmail, string Authorization, decimal Amount, string UDF1, string UDF2, string BrowserType, string BrowserEmail, string IPAddress, string CardVerification, string HostName, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("CapturePayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        UserEmail,
                        Authorization,
                        Amount,
                        UDF1,
                        UDF2,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndCapturePayment(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void CapturePaymentAsync(string AssociateName, string AssociatePassword, string UserEmail, string Authorization, decimal Amount, string UDF1, string UDF2, string BrowserType, string BrowserEmail, string IPAddress, string CardVerification, string HostName) {
            this.CapturePaymentAsync(AssociateName, AssociatePassword, UserEmail, Authorization, Amount, UDF1, UDF2, BrowserType, BrowserEmail, IPAddress, CardVerification, HostName, null);
        }
        
        /// <remarks/>
        public void CapturePaymentAsync(string AssociateName, string AssociatePassword, string UserEmail, string Authorization, decimal Amount, string UDF1, string UDF2, string BrowserType, string BrowserEmail, string IPAddress, string CardVerification, string HostName, object userState) {
            if ((this.CapturePaymentOperationCompleted == null)) {
                this.CapturePaymentOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCapturePaymentOperationCompleted);
            }
            this.InvokeAsync("CapturePayment", new object[] {
                        AssociateName,
                        AssociatePassword,
                        UserEmail,
                        Authorization,
                        Amount,
                        UDF1,
                        UDF2,
                        BrowserType,
                        BrowserEmail,
                        IPAddress,
                        CardVerification,
                        HostName}, this.CapturePaymentOperationCompleted, userState);
        }
        
        private void OnCapturePaymentOperationCompleted(object arg) {
            if ((this.CapturePaymentCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CapturePaymentCompleted(this, new CapturePaymentCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void DirectPaymentCompletedEventHandler(object sender, DirectPaymentCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DirectPaymentCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DirectPaymentCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void AuthorizePaymentCompletedEventHandler(object sender, AuthorizePaymentCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class AuthorizePaymentCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal AuthorizePaymentCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void CreditPaymentCompletedEventHandler(object sender, CreditPaymentCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CreditPaymentCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal CreditPaymentCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void CapturePaymentCompletedEventHandler(object sender, CapturePaymentCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CapturePaymentCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal CapturePaymentCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591
