// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace AspDotNetStorefrontCore.ShippingCalculation
{
    /// <summary>
    /// Set,Get information in shipping method
    /// </summary>
    public class ShippingMethod
    {
        public ShippingMethod()
        {
            IsRealTime = false;
            IsFree = false;
            ShippingIsFree = false;
			Name = string.Empty;
			DisplayName = string.Empty;
            Freight = decimal.Zero;
            Id = 0;
        }

        private string _displayFormat = string.Empty;
        
        #region CastOperators

        public static explicit operator ShipMethod(ShippingMethod method)
        {
            ShipMethod SM = new ShipMethod();
            SM.ShippingMethodID = method.Id;
            SM.ServiceRate = method.Freight;
            SM.VatRate = method.VatRate;
			SM.ServiceName = method.Name;
			SM.DisplayName = method.DisplayName;
            return SM;
        }

        #endregion
        
        #region Properties

        public int Id {get; set;}
        public string Name {get; set;}
		public string DisplayName {get; set;}
		public decimal Freight { get; set; }
		public bool IsFree { get; set; }
		public bool ShippingIsFree { get; set; }
		public bool IsRealTime { get; set; }
		public decimal VatRate { get; set; }
        public int DisplayOrder { get; set; }
        public string DisplayFormat
        {
            get 
            {
                if (CommonLogic.IsStringNullOrEmpty(_displayFormat))
                {
					return this.GetNameForDisplay();
                }

                return _displayFormat;
            }
            set { _displayFormat = value; }
        }
		public string GetNameForDisplay()
		{
			return !String.IsNullOrEmpty(this.DisplayName) ? this.DisplayName : this.Name;
		}
        
        #endregion
    }
    
}
