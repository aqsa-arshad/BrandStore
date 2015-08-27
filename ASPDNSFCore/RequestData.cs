// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Text;
using System.Web.Routing;
using System;
using System.Web.UI;
using System.Web;
using AspDotNetStorefront;


namespace AspDotNetStorefrontCore
{
    public enum ParseType
    {
        OnCulture,
        Native
    }

    public static class RoutedRequest
    {
        private static string DATA_KEY = "RoutedRequest";

        /// <summary>
        /// Gets the associated routed request for the current request context
        /// </summary>
        /// <returns></returns>
        public static RequestData GetCurrentRequestData()
        {
            return HttpContext.Current.Items[DATA_KEY] as RequestData;
        }

        /// <summary>
        /// Sets the associated routed request for the current request context
        /// </summary>
        /// <param name="data"></param>
        public static void SetCurrentRequestData(RequestData data)
        {
            var ds = HttpContext.Current.Items;
            if (ds.Contains(DATA_KEY))
            {
                ds[DATA_KEY] = data;
            }
            else
            {
                ds.Add(DATA_KEY, data);
            }
        }
    }


    /// <summary>
    /// Either the QueryString or in the case of a routed page, the route data
    /// </summary>
    public class RequestData
    {
        public RequestData(RequestContext context)
        {
            routedQueryString = context.RouteData.Values;
        }

        public RequestData()
        {

        }
       
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <returns></returns>
        private object this[string key]
        {
            get{
                if (HttpContext.Current.Request.QueryString[key] != null)
                    return HttpContext.Current.Request.QueryString[key];
                else if (routedQueryString != null && routedQueryString[key] != null)
                    return routedQueryString[key];
                else
                    return null;
            }
        }
        public object this[int index]
        {
            get
            {
                if (HttpContext.Current.Request.QueryString.Count != 0)
                {
                    return HttpContext.Current.Request.QueryString[index];
                }
                else if (routedQueryString.Count > 0)
                {
                    IDictionary<string, object> xDict = (IDictionary<string, object>)routedQueryString;
                    string[] keys = new string[] { };
                    xDict.Keys.CopyTo(keys, 0);
                    return xDict[keys[index]];
                }
                else
                {
                    return null;
                }
            }
        }
        public string GetKey(int index)
        {
            if (HttpContext.Current.Request.QueryString.GetKey(index) != null)
                return HttpContext.Current.Request.QueryString.GetKey(index);
            else
            {
                IDictionary<string, object> xDict = (IDictionary<string, object>)routedQueryString;
                string[] keys = new string[] { };
                xDict.Keys.CopyTo(keys, 0);
                return keys[index];
            }
        }
       

        #region USValues

        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <returns>bool</returns>
        public bool BoolValue(string key)
        {
            string qsValue = this[key].ToString().ToUpperInvariant();
            return (qsValue == "TRUE" || qsValue == "YES" || qsValue == "1");
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <returns>USCulture integer</returns>
        public int IntValue(string key)
        {
            return IntValue(key, ParseType.OnCulture);
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <returns>long integer</returns>
        public long LongValue(string key)
        {
            return LongValue(key, ParseType.OnCulture);
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <returns>USCulture double</returns>
        public double DoubleValue(string key)
        {
            return DoubleValue(key, ParseType.OnCulture);
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <returns>USCulture decimal</returns>
        public decimal DecimalValue(string key)
        {
            return DecimalValue(key, ParseType.OnCulture);
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <returns>DateTime</returns>
        public DateTime DateTimeValue(string key)
        {
            if (this[key].ToString() == string.Empty)
            {
                return new DateTime(0);
            }
            else
            {
                return Localization.ParseUSDateTime(this[key].ToString());
            }
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <returns>string</returns>
        public string StringValue(string key)
        {
            if (this[key] != null)
                return this[key].ToString();
            else if (key == "SEName")
                return SEName();
            else
                return string.Empty;
        }
        #endregion

        #region NativeValues

        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <param name="native">Parse native or parse based on culture</param>
        /// <returns></returns>
        public int IntValue(string key, ParseType parse)
        {
            if (this[key] == null)
                return 0;
            else
                return parse == ParseType.Native ?
                    Localization.ParseNativeInt(this[key].ToString()):
                    Localization.ParseUSInt(this[key].ToString());
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <param name="native">Parse native or parse based on culture</param>
        /// <returns>long integer</returns>
        public long LongValue(string key, ParseType parse)
        {
            if (this[key] == null)
                return 0;
            else
                return parse == ParseType.Native ?
                    Localization.ParseNativeLong(this[key].ToString()) :
                    Localization.ParseUSLong(this[key].ToString());
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <param name="native">Parse native or parse based on culture</param>
        /// <returns>double</returns>
        public double DoubleValue(string key, ParseType parse)
        {
            if (this[key] == null)
                return 0.0;
            else
                return parse == ParseType.Native ?
                    Localization.ParseNativeDouble(this[key].ToString()) :
                    Localization.ParseUSDouble(this[key].ToString());
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <param name="native">Parse native or parse based on culture</param>
        /// <returns>decimal</returns>
        public decimal DecimalValue(string key, ParseType parse)
        {
            if (this[key] == null)
                return 0.0M;
            else
                return parse == ParseType.Native ?
                    Localization.ParseNativeDecimal(this[key].ToString()) :
                    Localization.ParseUSDecimal(this[key].ToString());
        }
        /// <summary>
        /// The QueryString of the request
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <param name="native">Parse native or parse based on culture</param>
        /// <returns>DateTime</returns>
        public DateTime DateTimeValue(string key, ParseType parse)
        {
            if (this[key].ToString() == string.Empty)
            {
                return new DateTime(0);
            }
            else
            {

                return parse == ParseType.Native ?
                    Localization.ParseUSDateTime(this[key].ToString()) :
                    Localization.ParseNativeDateTime(this[key].ToString());
            }
        }
        #endregion

        /// <summary>
        /// Outputs the data in standard querystring format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (HttpContext.Current.Request.QueryString.ToString() != string.Empty)
                return HttpContext.Current.Request.QueryString.ToString();

            StringBuilder xBldr = new StringBuilder();
            foreach (string key in routedQueryString.Keys)
            {
                if (key.Contains("SEPart")) continue;
                xBldr.Append(key + "=" + routedQueryString[key].ToString() + "&");
            }
            if (routedQueryString.ContainsKey("SEPart1"))
                xBldr.AppendFormat("SEName={0}", SEName());
            if (xBldr.Length > 0 && xBldr[xBldr.Length -1] == '&')
                xBldr.Remove(xBldr.Length - 1, 1);
            return xBldr.ToString();
        }

        private string SEName()
        {
            System.Text.StringBuilder xBldr = new System.Text.StringBuilder();
            for (int xCtr = 1; ; xCtr++)
            {
                if (StringValue("SEPart" + xCtr.ToString()) != string.Empty)
                    xBldr.Append(StringValue("SEPart" + xCtr.ToString()) + "-");
                else
                {
                    if (xBldr.Length == 0)
                        return string.Empty;
                    xBldr.Remove(xBldr.Length - 1, 1);
                    return xBldr.ToString();
                }
            }
        }


        private RouteValueDictionary routedQueryString
        {get;set;}
       
    }
}
