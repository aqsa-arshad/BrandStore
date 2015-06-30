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
    public class CreditCardValidator
    {
        private static bool CARD_NUMBER_VALID = true;
        private static bool CARD_NUMBER_INVALID = false;

        private string _cardNumber = string.Empty;
        private CardType _cardType;

        public CreditCardValidator(string cardNumber, CardType cardType)
        {
            _cardNumber = CleanCardNumber(cardNumber);
            _cardType = cardType;
        }

        public string CardNumber
        {
            get { return _cardNumber; }
            set { _cardNumber = value; }
        }

        public CardType CardType
        {
            get { return _cardType; }
            set { _cardType = value; }
        }

        public bool Validate()
        {
			if (_cardType != null)
			{
				bool lengthOK = CheckLength();
				if (!lengthOK)
				{
					return CARD_NUMBER_INVALID;
				}

				bool prefixOK = CheckPrefix();
				if (!prefixOK)
				{
					return CARD_NUMBER_INVALID;
				}

			}
			else
			{
				return CARD_NUMBER_INVALID;
			}

            bool luhnCheckOK = LUHNCheck();
            if(!luhnCheckOK)
            {
                return CARD_NUMBER_INVALID;
            }

            return CARD_NUMBER_VALID;
        }

        private bool CheckLength()
        {
            int cardLength = _cardNumber.Length;
            foreach (int validLength in _cardType.ValidLengths)
            {
                if (cardLength == validLength)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckPrefix()
        {
            foreach (string validPrefix in _cardType.ValidPrefixes)
            {
                if (_cardNumber.StartsWith(validPrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string CleanCardNumber(string cardNumber)
        {
            string cardNumberClean = string.Empty;

            foreach (char n in cardNumber)
            {
                if (char.IsNumber(n))
                {
                    cardNumberClean += n.ToString();
                }
            }

            return cardNumber;
        }

        private bool LUHNCheck()
        {
            int sum, checksum;
            return LUHNCheck(out sum, out checksum);
        }

        private bool LUHNCheck(out int sum, out int checksum)
        {
            sum = 0;
            checksum = 0;

            int x = 0;
            List<int> numbers = new List<int>();

            foreach (char n in _cardNumber)
            {
                if (int.TryParse(n.ToString(), out x))
                {
                    numbers.Add(x);
                }
            }

            List<int> doubles = new List<int>();
            List<int> others = new List<int>();

            for (int ctr = numbers.Count - 2; ctr >= 0; ctr -= 2)
            {
                doubles.Add(numbers[ctr]);
            }

            for (int ctr = numbers.Count - 1; ctr >= 0; ctr -= 2)
            {
                others.Add(numbers[ctr]);
            }

            int doubledSum = 0;
            int othersSum = 0;

            foreach (int doubled in doubles)
            {
                int num = doubled;
                num *= 2;

                //the number shouldn't be 2 digits
                int eval = num;
                if (eval > 9)
                {
                    // you could either add the 2 numbers up i.e. 12 = {1+2}
                    // or simply subtract 9
                    eval -= 9;
                }

                doubledSum += eval;
            }
            
            foreach (int otherNumber in others)
            {
                othersSum += otherNumber;
            }

            checksum = doubledSum;
            sum = othersSum;

            return (checksum + sum) % 10 == 0;
        }

        public bool CheckCCExpiration(int CCExpirationMonth, int CCExpirationYear)
        {
            Int32 daysInMonth = System.DateTime.DaysInMonth(CCExpirationYear, CCExpirationMonth);

            DateTime expirationDate = new DateTime(CCExpirationYear, CCExpirationMonth, daysInMonth);                           
            if (expirationDate >= System.DateTime.Today)
            {
                return true;
            }
            return false;
        }

        public bool ValidateCVV(string CCNumber, string CVV)
        {
            if (Regex.IsMatch(CCNumber, "^3[47][0-9]{13}$"))
            {
                if (Regex.IsMatch(CVV, "^[0-9]{4}$")) //AMEX
                {
                    return true;
                }
            }
            else if (Regex.IsMatch(CCNumber, "^5[1-5][0-9]{14}$") //VISA, Discover, Master Card
                || Regex.IsMatch(CCNumber, "^4[0-9]{12}(?:[0-9]{3})?$")
                || Regex.IsMatch(CCNumber, "^6(?:011|5[0-9]{2})[0-9]{12}$"))
            {
                if (Regex.IsMatch(CVV, "^[0-9]{3}$"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
