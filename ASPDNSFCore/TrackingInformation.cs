using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspDotNetStorefrontCore
{
    public class TrackingInformation
    {
        public string OrderNumber { get; set; }
        public string TrackingNumber { get; set; }
        public string CarrierCode { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingURL { get; set; }

    }
}
