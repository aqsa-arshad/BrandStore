// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Vortx.AdnsfHelpers
{
    public class ShippingHelper
    {
        public static string ConvertShippingMethodsToDropDown(string shipMethods)
        {
            Regex regEx = new Regex(@"<input type=""radio"" name=""ShippingMethodID"" id=""ShippingMethodID[0-9][0-9]?"" value=""(\d+)""([\w\d\s""=]*)>(.*?)");

            StringBuilder sb = new StringBuilder();
            sb.Append("<select name=\"ShippingMethodID\">");
            Match match = regEx.Match(shipMethods);
            while (match.Success)
            {
                // Match Group 0: whole regex match
                // Match Group 1: shipping method Id (the value of 'value' attribute)
                // Match Group 2: any data after the 'value=xx' attribute, usually 'checked' if defined
                // Match Group 3: shipping method name
                if (match.Groups[2].Value.IndexOf("checked") >= 0)
                {
                    sb.AppendFormat("<option value=\"{0}\" selected>{1}</option>", match.Groups[1].Value, match.Groups[3].Value);
                }
                else
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", match.Groups[1].Value, match.Groups[3].Value);
                }

                match = match.NextMatch();
            }
            sb.Append("</select>");

            return sb.ToString();
        }
    }
}
