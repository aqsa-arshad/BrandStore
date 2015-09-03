// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Xml;

namespace AspDotNetStorefrontGateways.Processors
{    
    public class FirstPayXmlCommand : XmlDocument
    {
        #region Instance Variables
        private XmlDeclaration dec;
        private XmlElement root;
        private XmlElement Fields;
        
        private XmlElement TransactionCenterId;
        private XmlElement GatewayId;
        private XmlElement OperationType;        
        private XmlElement ProcessorId;

        #endregion

        public FirstPayXmlCommand(string transaction_center_id, string gateway_id, string processor_id)
        {
            
            dec = this.CreateXmlDeclaration("1.0", "UTF-8", null);
            root = this.CreateElement("TRANSACTION");
            Fields = this.CreateElement("FIELDS");

            TransactionCenterId = AddField("transaction_center_id", transaction_center_id.ToString());
            GatewayId = AddField("gateway_id", gateway_id);
            OperationType = AddField("operation_type", "");
            ProcessorId = AddField("processor_id", processor_id);

            this.AppendChild(dec);
            root.AppendChild(Fields);
            this.AppendChild(root);
            
        }

        #region Transaction Type Methods

        public void Settle(string reference_number, string settle_amount)
        {
            if(Fields.ChildNodes.Count > 6)
                InitializeFieldNode();

            OperationType.InnerText = FirstPay.TransactionType.settle.ToString();            
            AddField("total_number_transactions", "1");
            AddField("reference_number1", reference_number);
            AddField("settle_amount1", settle_amount);
        }

        public void Credit(string reference_number, string credit_amount)
        {
            if (Fields.ChildNodes.Count > 6)
                InitializeFieldNode();

            OperationType.InnerText = FirstPay.TransactionType.credit.ToString();
            AddField("total_number_transactions", "1");
            AddField("reference_number1", reference_number);
            AddField("credit_amount1", credit_amount);
        }

        public void Void(string reference_number)
        {
            if (Fields.ChildNodes.Count > 6)
                InitializeFieldNode();

            OperationType.InnerText = FirstPay.TransactionType.@void.ToString();
            AddField("total_number_transactions", "1");
            AddField("reference_number1", reference_number);
        }

        public void Query(string order_id, DateTime BeginDate, DateTime EndDate)
        {
            if (Fields.ChildNodes.Count > 6)
                InitializeFieldNode();

            OperationType.InnerText = FirstPay.TransactionType.query.ToString();
            AddField("trans_type", "ALL");
            AddField("order_id", order_id);
            AddField("begin_date", BeginDate.ToString("MMddyy"));
            AddField("end_date", EndDate.ToString("MMddyy"));
        }

        #endregion

        #region Helper Methods

        private void InitializeFieldNode()
        {
            Fields.RemoveAll();

            AddField(TransactionCenterId);
            AddField(GatewayId);
            AddField(OperationType);
            AddField(ProcessorId);            
        }

        public XmlElement AddField(string Key, string Value)
        {
            XmlElement node = this.CreateElement("FIELD");
            node.SetAttribute("KEY", Key.Trim());
            node.InnerText = Value.Trim();
            Fields.AppendChild(node);

            return node;
        }

        private void AddField(XmlElement node)
        {
            Fields.AppendChild(node);
        }

        #endregion

    }

}
