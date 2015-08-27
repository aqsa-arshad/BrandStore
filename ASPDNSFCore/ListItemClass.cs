// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace AspDotNetStorefrontCore
{
    public class ListItemClass
    {
        private string m_item;
        private int m_value;

        private string m_valueS;

        public string Item
        {
            get
            {
                return m_item;
            }
            set
            {
                m_item = value.Replace("-&gt;","->");
            }
        }

        public int Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        public string ValueS
        {
            get
            {
                return m_valueS;
            }
            set
            {
                m_valueS = value;
            }
        }
    }
}
