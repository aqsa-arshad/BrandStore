// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontGateways.Processors;
using AspDotNetStorefront.HostView;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontGateways
{
    public class RecurringInterval
    {
        public Int32 GatewayIntervalId { get; set; }
        public Int32 SubscriptionDays { get; set; }
        public String GatewayPassedValue { get; set; }
        public String DisplayName { get; set; }

        public RecurringInterval(String gatewayPassedValue, String displayName, Int32 gatewayIntervalId, Int32 subscriptionDays) 
        {
            this.GatewayPassedValue = gatewayPassedValue;
            this.DisplayName = displayName;
            this.GatewayIntervalId = gatewayIntervalId;
            this.SubscriptionDays = subscriptionDays;
        }

        public ListItem ToRecurringListItem()
        {
            return new ListItem(this.DisplayName, this.GatewayIntervalId.ToString());
        }

        public ListItem ToSubscriptionListItem()
        {
            return new ListItem(this.DisplayName, this.SubscriptionDays.ToString());
        }
    }

    public class RecurringIntervalCollection
    {
        public List<RecurringInterval> Intervals { get; set; }
        public Boolean AllowMultiplier { get; set; }
        public Boolean IsGatewayRecurring { get; set; }

        public RecurringIntervalCollection()
        {
            this.Intervals = new List<RecurringInterval>();
            this.AllowMultiplier = false;
            this.IsGatewayRecurring = false;
        }
    }
}
