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
using System.Data;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
    public class CardTypeDataSource
    {
        public static List<string> GetAcceptedCreditCardTypes(Customer ThisCustomer)
        {
            List<string> CCTypes = new List<string>();

            CCTypes.Add(AppLogic.GetString("address.cs.32", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));

            #region Moneybookers acquirer credit card type override

            string configuredPaymentGateway = AppLogic.AppConfig("PaymentGateway").ToUpper();
            string configuredAcquirer = AppLogic.AppConfig("Moneybookers.Acquirer").ToUpper();

            if (configuredPaymentGateway == "MONEYBOOKERS" && configuredAcquirer.ToUpper() != "NONE")
            {
                string[] acquirerCardTypes = AppLogic.AppConfig("Moneybookers.Acquirer." + configuredAcquirer + ".AcceptedCardTypes").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                CCTypes.AddRange(acquirerCardTypes);
                return CCTypes;
            }

            #endregion

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from creditcardtype  with (NOLOCK)  where Accepted=1 order by CardType", conn))
                {
                    while (rs.Read())
                    {
                        CCTypes.Add(DB.RSField(rs, "CardType"));
                    }
                }
            }
            return CCTypes;
        }
    }
}
