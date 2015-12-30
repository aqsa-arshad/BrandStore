using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using AspDotNetStorefrontCore;
using System.Globalization;

public partial class controls_BrandMerchandiseExample : System.Web.UI.UserControl
{
    private Customer m_ThisCustomer;
    public Customer ThisCustomer
    {
        get
        {
            if (m_ThisCustomer == null)
                m_ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            return m_ThisCustomer;
        }
        set
        {
            m_ThisCustomer = value;
        }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        SetLoginSpecificValues();
    }
    protected void SetLoginSpecificValues()
    {
        String CustomerLevel = ThisCustomer.CustomerLevelID.ToString();
        if (CustomerLevel == "7" || CustomerLevel == "3" || CustomerLevel == "1" || CustomerLevel == "2")
        {
            //sales rep or internal user
            PromotionalItemsTopicForInternalusers.Visible = true;
            PromotionalItemsTopicForDealers.Visible = false;
           
        }
        else if (CustomerLevel == "4" || CustomerLevel == "5" || CustomerLevel == "6" || CustomerLevel == "9" || CustomerLevel == "13")
        {
            //dealer and subdealers login
            PromotionalItemsTopicForInternalusers.Visible = false;
            PromotionalItemsTopicForDealers.Visible = true;
        }
    }
}