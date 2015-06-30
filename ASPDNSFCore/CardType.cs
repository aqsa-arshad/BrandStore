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

namespace AspDotNetStorefrontCore
{
    public class CardType
    {
        #region Variable Declaration
        
        private string _name = string.Empty;        
        private int[] _validLengths;
        private string[] _validPrefixes;

        #endregion        

        #region Constructor

        private CardType(string name, string[] validPrefixes, int[] validLengths)
        {
            _name = name;
            _validPrefixes = validPrefixes;
            _validLengths = validLengths;
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
        }

        public int[] ValidLengths
        {
            get { return _validLengths; }
        }

        public string[] ValidPrefixes
	    {
		    get { return _validPrefixes;}
	    }
	

        #endregion


        public static readonly CardType Visa = new CardType("Visa", new string[]{"4"}, new int[]{13,16});
        public static readonly CardType MasterCard = new CardType("MasterCard", new string[] {"51", "52", "53", "54", "55" }, new int[] {16 });
        public static readonly CardType Amex = new CardType("AMEX", new string[] { "34", "37" }, new int[] { 15});
        public static readonly CardType DinersClub = new CardType("Diners Club", new string[] { "300", "301", "302", "303", "304", "305", "36", "38" }, new int[] { 14 });
        public static readonly CardType CarteBlanche = new CardType("Carte Blanche", new string[] { "300", "301", "302", "303", "304", "305", "36", "38" }, new int[] { 14 });
        public static readonly CardType Discover = new CardType("Discover", new string[] { "6011" }, new int[] { 16 });        
        public static readonly CardType EnRoute = new CardType("enRoute", new string[] { "2014", "2149" }, new int[] { 15});
        public static readonly CardType JCB = new CardType("JCB", new string[] { "3", "2131", "1800" }, new int[] {15, 16 });

        public static CardType Parse(string s)
        {
            if (s.StartsWith("Visa", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardType.Visa;
            }
            if (s.StartsWith("MasterCard", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardType.MasterCard;
            }
            if (s.StartsWith("Ame", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardType.Amex;
            }
            if (s.StartsWith("Diners", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardType.DinersClub;
            }
            if (s.StartsWith("Diners", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardType.DinersClub;
            }
            if (s.StartsWith("Discover", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardType.Discover;
            }
            if (s.StartsWith("EnRoute", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardType.EnRoute;
            }
            if (s.StartsWith("JCB", StringComparison.InvariantCultureIgnoreCase))
            {
                return CardType.JCB;
            }

            return null;
        }
	
        public static CardType ParseFromNumber(string cardNum)
        {
            if (Regex.IsMatch(cardNum, "^3[47][0-9]{13}$"))
            {
                return CardType.Amex;
            }
            if (Regex.IsMatch(cardNum, "^4[0-9]{12}(?:[0-9]{3})?$"))
            {
                return CardType.Visa;
            }
            if (Regex.IsMatch(cardNum, "^5[1-5][0-9]{14}$"))
            {
                return CardType.MasterCard;
            }
            if (Regex.IsMatch(cardNum, "^6(?:011|5[0-9]{2})[0-9]{12}$"))
            {
                return CardType.Discover;
            }
            return null;
        }	
    }
}
