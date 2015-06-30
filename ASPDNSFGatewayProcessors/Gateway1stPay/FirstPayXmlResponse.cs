// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml;
using System.Globalization;
using System;

namespace AspDotNetStorefrontGateways.Processors
{    
    public class FirstPayXmlResponse
    {
        #region Instance Variables

        private XmlDocument doc;
        public Dictionary<string, string> Fields;

        #endregion

        public FirstPayXmlResponse(XmlDocument xmlDoc)
        {
            doc = xmlDoc;
            PopulateFields();
        }

        #region Helper Methods

        private void PopulateFields()
        {
            if (doc == null)
                return;

            Fields = new Dictionary<string, string>();
            XmlNodeList nodeList = doc.SelectNodes("/RESPONSE/FIELDS/FIELD");
            foreach (XmlNode node in nodeList)
            {
                Fields.Add(node.Attributes["KEY"].Value, node.InnerText);
            }
        }

        public List<FirstPayTransaction> GetTransactionsFromQuery()
        {
            List<FirstPayTransaction> transactions = new List<FirstPayTransaction>();                    
            CultureInfo provider = CultureInfo.InvariantCulture;
            string dateFormat = "M/d/yyyy h:mm:ss tt";
            int recordCount;    
            string recordsFound = Fields.ContainsKey("records_found") ? Fields["records_found"] : "";

            if (string.IsNullOrEmpty(recordsFound))
                return transactions;

            int.TryParse(recordsFound, out recordCount);

            try
            {
                for (int i = 1; i <= recordCount; i++)
                {
                    string index = i.ToString();
                    FirstPayTransaction trans = new FirstPayTransaction();
                    trans.Type = Fields["trans_type" + index];
                    trans.Status = int.Parse(Fields["trans_status" + index]);
                    trans.Settled = Fields["settled" + index] == "1" ? true : false;
                    trans.CreditVoid = Fields["credit_void" + index];
                    trans.OrderId = Fields["order_id" + index];
                    trans.ReferenceNumber = int.Parse(Fields["reference_number" + index]);
                    trans.TransactionTime = DateTime.ParseExact(Fields["trans_time" + index], dateFormat, provider);
                    trans.CardType = Fields["card_type" + index];
                    trans.Amount = decimal.Parse(Fields["amount" + index]);
                    trans.AmountSettled = decimal.Parse(Fields["amount_settled" + index]);
                    trans.AmountCredited = decimal.Parse(Fields["amount_credited" + index]);
                    trans.Error = Fields.ContainsKey("error" + index) ? Fields["error" + index] : "";
                    transactions.Add(trans);
                }
            }
            catch { }

            return transactions;
        }

        #endregion

    }
        
}
