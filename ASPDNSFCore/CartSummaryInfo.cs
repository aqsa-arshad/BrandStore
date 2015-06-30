// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------



namespace AspDotNetStorefrontCore
{
    public class CartSummaryInfo
    {
        private ShoppingCart m_thiscart;
        private ShoppingCart thisCart
        {
            get { return m_thiscart; }
            set { m_thiscart = value; }
        }
        private bool m_vatenabled;
        public bool VatEnabled
        {
            get { return m_vatenabled; }
            set { m_vatenabled = value; }
        }

        private bool m_vatisinclusive;
        public bool VatIsInclusive
        {
            get { return m_vatisinclusive; }
            set { m_vatisinclusive = value; }
        }

        private decimal m_lineitemtotal;
        public decimal LineItemTotal
        {
            get { return m_lineitemtotal; }
            set { m_lineitemtotal = value; }
        }

        public decimal FreightRate
        {
            get
            {
                return Prices.ShippingTotal(true, VatIsInclusive, thisCart.CartItems, Customer.Current, thisCart.OrderOptions);
            }
        }

        public decimal TaxRate
        {
            get { return Prices.TaxTotal(Customer.Current, thisCart.CartItems, FreightRate, thisCart.OrderOptions); }
        }

        public decimal TaxTotalForLineItem
        {
            get
            {
                return Prices.TaxTotal(Customer.Current, thisCart.CartItems, Prices.ShippingTotal(true, false, thisCart.CartItems, Customer.Current, thisCart.OrderOptions), thisCart.OrderOptions);
            }
        }
    }
}
