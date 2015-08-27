// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml;
using System;

namespace AspDotNetStorefrontGateways.Processors
{    
    public class FirstPayTransaction
    {
        #region Instance Variables
      
        public string Type;
        public int Status;
        public bool Settled;
        public string CreditVoid;
        public string OrderId;
        public int ReferenceNumber;
        public DateTime TransactionTime;
        public string CardType;
        public decimal Amount;
        public decimal AmountSettled;
        public decimal AmountCredited;
        public string Error;

        #endregion

        public FirstPayTransaction() { }

    }
        
}
