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

namespace Vortx.MobileFramework.Utils
{
    public class TextUtil
    {
        public static string StripNonScriptHtml(String s)
        {
            return Regex.Replace(s, @"<(?!/?script)(.|\n)*?>", string.Empty, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static string StripScriptAndHTML(String s)
        {
            return StripScriptAndHTML(s, false);
        }

        public static string StripScriptAndHTML(String s, bool ShowImages)
        {
            string retstring;
            retstring = Regex.Replace(s, @"<script(.|\n)*?>(.|\n)*?</script(.|\n)*?>", string.Empty, RegexOptions.Compiled);
            return Regex.Replace(retstring, @"<(?!/?br)" + (ShowImages ? "(?!/?img)" : "") + @"(.|\n)*?>", string.Empty, RegexOptions.Compiled);
        }

        /// <summary>
        /// Clips text to given length, stopping at nearest space character.
        /// </summary>
        /// <param name="textToClip"></param>
        /// <param name="length"></param>
        /// <returns>Clipped text</returns>
        public static string ClipText(string textToClip, int length)
        {
            return ClipText(textToClip, length, ' ');
        }

        /// <summary>
        /// Clips text to given length, stopping at nearest given character.
        /// </summary>
        /// <param name="textToClip"></param>
        /// <param name="length"></param>
        /// <param name="nearestChar"></param>
        /// <returns>Clipped text</returns>
        public static string ClipText(string textToClip, int length, char nearestChar)
        {
            if (length >= textToClip.Length)
                return textToClip;

            // if we find a character near the given char, then clip to 
            // that char.
            int idx = textToClip.IndexOf(nearestChar, length);
            if (idx != -1)
            {
                return textToClip.Substring(0, idx);
            }

            // otherwisse, just clip to the given length
            return textToClip.Substring(0, length);
        }
    }
}
