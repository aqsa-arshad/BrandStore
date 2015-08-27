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
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Helper class containing our extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        #region String Extensions

        private const StringComparison STRING_COMPARISON_RULE = StringComparison.InvariantCultureIgnoreCase;

        /// <summary>
        /// Extension method to do case-insensitive string comparison
        /// </summary>
        /// <param name="str"></param>
        /// <param name="equalTo"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string str, string equalTo)
        {
            return str.Equals(equalTo, STRING_COMPARISON_RULE);
        }


        /// <summary>
        /// Extension method to determine whether the beginning of the string matches the specified string in a case-insensitive manner
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool StartsWithIgnoreCase(this string str, string value)
        {
            return str.StartsWith(value, STRING_COMPARISON_RULE);
        }

        public static bool StartsWithNumbers(this string str)
        {
            bool firstCharNumeric = false;
            if (!string.IsNullOrEmpty(str) && str.Length > 0)
            {
                firstCharNumeric = char.IsNumber(str[0]);
            }

            return firstCharNumeric;
        }

        /// <summary>
        /// Extension method to determine if whether the string contains the specified value in a case-insensitive manner
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string str, string value)
        {
            return str.IndexOf(value, STRING_COMPARISON_RULE) != -1;
        }

        /// <summary>
        /// Extension function to determine whether the specified string can be parsed into an integer
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInt(this string str)
        {
            int ni;
            return System.Int32.TryParse(str, NumberStyles.Integer, Thread.CurrentThread.CurrentUICulture, out ni); // use default locale setting
        }

        /// <summary>
        /// Extension method to parse the specified string into a native integer
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ToNativeInt(this string str)
        {
            return Localization.ParseNativeInt(str);
        }

        /// <summary>
        /// Extension method to parse the specified string into native decimal
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal ToNativeDecimal(this string str)
        {
            return Localization.ParseNativeDecimal(str);
        }

        public static String AppendQueryString(this string str, String query)
        {
            if (str.Contains("?"))
                return str + "&" + query;
            else
                return str + "?" + query;
        }

        public static bool ToBool(this string str)
        {
            if (str.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Extension method to safe-quote the string to use for db queries
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DBQuote(this string str)
        {
            return DB.SQuote(str);
        }

        /// <summary>
        /// Adds a range of values on a List of string from a comma delimited string
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="commaDelimited"></param>
        public static void AddCommaDelimited(this List<string> destination, string commaDelimited)
        {
            string[] values = commaDelimited.Split(',');
            foreach (string value in values)
            {
                if (!string.IsNullOrEmpty(value.Trim()))
                {
                    destination.Add(value.Trim());
                }
            }
        }

        public static string IntToCommaDelimitedString(this List<int> source)
        {
            string commaDelimited = string.Empty;
            for (int ctr = 0; ctr < source.Count; ctr++)
            {
                string value = source[ctr].ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    if (ctr > 0)
                    {
                        value = ", " + value;
                    }

                    commaDelimited += value;
                }
            }

            return commaDelimited;
        }

        /// <summary>
        /// Builds a comma delmited string from a generic string list
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToCommaDelimitedString(this List<string> source)
        {
            string commaDelimited = string.Empty;
            for(int ctr=0; ctr<source.Count; ctr++)
            {
                string value = source[ctr].Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    if (ctr > 0)
                    {
                        value = ", " + value;
                    }

                    commaDelimited += value;
                }
            }

            return commaDelimited;
        }

        /// <summary>
        /// String.Formats a given string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        /// <summary>
        /// Looks up the value of a string resource with the current customer's SkinID and LocaleSetting
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringResource(this string str)
        {
            return AppLogic.GetString(str, Customer.Current.SkinID, Customer.Current.LocaleSetting);
        }
        

        #endregion

        #region Control Extensions

        /// <summary>
        /// Extension method for data-binding operations on a Repeater control to strongly type the current binded item
        /// Sample usage:  Text='&lt;%# Container.DataItemAs&lt;KitItemData&gt;().Name %&gt;'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T DataItemAs<T>(this RepeaterItem item) where T : class
        {
            return item.DataItem as T;
        }

        public static T DataItemAs<T>(this GridViewRow item) where T : class
        {
            return item.DataItem as T;
        }

        /// <summary>
        /// Extension method for data-binding operations on a DataList control to strongly type the current binded item
        /// Sample usage:  Text='&lt;%# Container.DataItemAs&lt;KitItemData&gt;().Name %&gt;'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T DataItemAs<T>(this DataListItem item) where T : class
        {
            return item.DataItem as T;
        }

        public static T DataItemAs<T>(this IDataItemContainer item) where T : class
        {
            return item.DataItem as T;
        }

        /// <summary>
        /// Extension method to automatically define the expected type of the found control by specifying it's generic type
        /// Sample usage: FindControl&lt;TextBox&gt;("txtValue");
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T FindControl<T>(this Control container, string id) where T : class
        {
            return container.FindControl(id) as T;
        }

        /// <summary>
        /// Extension method to assign css class for a asp.net control based on a condition
        /// </summary>
        /// <param name="container"></param>
        /// <param name="condition"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string CssClassIf(this Control container, bool condition, string className)
        {
            if (condition == true)
            {
                return className;
            }

            return string.Empty;
        }

        /// <summary>
        /// Extension method to assign the css class based on a condition and fallback className if condition is invalid
        /// </summary>
        /// <param name="container"></param>
        /// <param name="condition"></param>
        /// <param name="className"></param>
        /// <param name="otherwiseClassName"></param>
        /// <returns></returns>
        public static string CssClassIf(this Control container, bool condition, string className, string otherwiseClassName)
        {
            if (condition == true)
            {
                return className;
            }
            else
            {
                return otherwiseClassName;
            }
        }


        /// <summary>
        /// Extension method to assign css class for a asp.net control based on a condition
        /// </summary>
        /// <param name="container"></param>
        /// <param name="condition"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string CssClassIfInvalid(this Control container, IValidable item, string propertyName, string errorStyle)
        {
            if (!item.IsValid())
            {
                // find the matching property
                ValidationError valError = item.ValidationErrors().Find(vError => vError.PropertyName.EqualsIgnoreCase(propertyName));
                if (valError != null)
                {
                    return errorStyle;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Helper method to execute upon iterating through the ItemTemplate in this repeater
        /// </summary>
        /// <param name="rpt"></param>
        /// <param name="itemAction"></param>
        public static void ForEachItemTemplate(this Repeater rpt, Action<RepeaterItem> itemAction)
        {
            foreach (RepeaterItem rptItem in rpt.Items)
            {
                if (rptItem.ItemType == ListItemType.Item ||
                    rptItem.ItemType == ListItemType.AlternatingItem)
                {
                    itemAction(rptItem);
                }
            }

        }

        /// <summary>
        /// Helper method to specify a delegate to execute for each item in this collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="iterator"></param>
        public static void ForEachItem<T>(this IEnumerable<T> source, Action<T> iterator)
        {
            foreach (T item in source)
            {
                iterator(item);
            }
        }

        #endregion

        #region HttpRequest Extension Methods

        /// <summary>
        /// Extension method to get the querystring value from the HttpRequest object
        /// </summary>
        /// <param name="req"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static string QueryStringCanBeDangerousContent(this HttpRequest req, string paramName)
        {
            return CommonLogic.QueryStringCanBeDangerousContent(paramName);
        }

        /// <summary>
        /// Extension method to get the querystring value as integer from the HttpRequest object
        /// </summary>
        /// <param name="req"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static int QueryStringNativeInt(this HttpRequest req, string paramName)
        {
            return CommonLogic.QueryStringNativeInt(paramName);
        }

        #endregion

        #region IDataReader Extension Methods

        public static string Field(this IDataReader rs, string fieldName)
        {
            return DB.RSField(rs, fieldName);
        }

        public static int FieldInt(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldInt(rs, fieldName);
        }

        public static bool FieldBool(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldBool(rs, fieldName);
        }

        public static string FieldByLocale(this IDataReader rs, string fieldName, string locale)
        {
            return DB.RSFieldByLocale(rs, fieldName, locale);
        }

        public static DateTime FieldDateTime(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldDateTime(rs, fieldName);
        }

        public static Decimal FieldDecimal(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldDecimal(rs, fieldName);
        }

        public static Guid FieldGuid(this IDataReader rs, string fieldName)
        {
            return DB.RSFieldGUID2(rs, fieldName);
        }

        #endregion

        #region HttpContext Extension Methods

        /// <summary>
        /// Extension method to retrieve the associated routing request data from the current httpcontext request
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static RequestData GetRequestData(this HttpContext ctx)
        {
            return RoutedRequest.GetCurrentRequestData();
        }

        /// <summary>
        /// Extension method to set the associated routing request data from the current httpcontext request
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="data"></param>
        public static void SetRequestData(this HttpContext ctx, RequestData data)
        {
            RoutedRequest.SetCurrentRequestData(data);
        }

        #endregion


    }

}

