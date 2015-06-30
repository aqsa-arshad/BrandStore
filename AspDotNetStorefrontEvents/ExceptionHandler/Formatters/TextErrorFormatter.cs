// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;
using System.Text;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontEventHandlers.ExceptionHandlers.Formatters
{
    public class TextErrorFormatter : IErrorFormatter
    {
        #region Variable Declaration

        private StringBuilder _content = new StringBuilder();
        private const string GROUP_WITH_DIVIDER_FORMAT = "\n\n----------- {0} -----------";

        #endregion

        #region Properties

        public string Error
        {
            get { return _content.ToString(); }
        }

        #endregion

        #region Methods

        public void AddGroup(string groupName)
        {
            _content.AppendFormat(GROUP_WITH_DIVIDER_FORMAT, groupName);
        }

        public void Add(string name, string value, bool valueAsIs)
        {
            _content.Append("\n" +
                (name + ":").PadRight(30, ' ') +
                CommonLogic.IIF(valueAsIs,
                    value,
                    value.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")));
        }

        public void Add(string name, string value)
        {
            Add(name, value, false);
        }

        public void AddFrom(NameValueCollection collection)
        {
            foreach (string name in collection.AllKeys)
            {
                Add(name, collection[name]);
            }
        }

        private void CaptureError(Exception error)
        {
            Add("Type", error.GetType().ToString());
            Add("Message", error.Message);
            Add("Source", error.Source);
            Add("TargetSite", error.TargetSite.ToString());
            Add("StackTrace", error.StackTrace, true);

            if (error.InnerException != null)
            {
                AddGroup("Inner Exception");
                CaptureError(error.InnerException);
            }
        }

        public void Prepare(string errorCode, Exception error)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx != null)
            {
                AddGroup("General");
                Add("Date", DateTime.Now.ToString("G"));
                HttpRequest request = ctx.Request;
                Add("Url", request.Url.ToString());
                if (request.UrlReferrer != null)
                {
                    Add("Referrer", request.UrlReferrer.ToString());
                }

                Customer thisCustomer = ((AspDotNetStorefrontPrincipal)ctx.User).ThisCustomer;
                if (thisCustomer.IsRegistered)
                {
                    AddGroup("Customer");
                    Add("Customer ID", thisCustomer.CustomerID.ToString());
                    Add("Email", thisCustomer.EMail);
                    Add("Phone", thisCustomer.Phone);
                }

                AddGroup("Exception");
                Add("ErrorCode", errorCode);
                CaptureError(error);

                AddGroup("Query String");
                AddFrom(request.QueryString);

                AddGroup("Cookies");
                foreach (string cookie in request.Cookies.AllKeys)
                {
                    Add(cookie, request.Cookies[cookie].Value);
                }

                AddGroup("Form");
                AddFrom(request.Form);

                AddGroup("Server Variables");
                AddFrom(request.ServerVariables);
            }
        }

        public override string ToString()
        {
            return this.Error;
        }

        #endregion
    }
}
