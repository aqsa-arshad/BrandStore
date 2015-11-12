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
        SetControlValues();
    }

    protected void SetControlValues()
    {
        using (SqlConnection conn = DB.dbConn())
        {
            conn.Open();
            String CategoryID = AppLogic.AppConfig("PromotionalItemsCategoryID");
            using (System.Data.IDataReader rs = DB.GetRS("aspdnsf_GetLatestProductByCategory " + CategoryID, conn))
            {
                if (rs.Read())
                {

                    String PGuid = rs["ProductGUID"].ToString();
                    String PID = rs["ProductID"].ToString();
                    String PDescription = rs["Description"].ToString();
                    String PImagename = rs["Imagename"].ToString();

                    /*Get immage for product if it exists and check its name/extension*/
                    String path = HttpContext.Current.Request.MapPath("~/images/product/icon");
                    string[] filePaths = System.IO.Directory.GetFiles(path, PImagename + ".*", System.IO.SearchOption.TopDirectoryOnly);
                    
                    if (filePaths.Length > 0)
                    {
                        if (!PImagename.EndsWith(".JPG", true, new CultureInfo("en-US")))
                        PImagename = PImagename + System.IO.Path.GetExtension(filePaths[0]);
                    }
                    else
                    {
                        PImagename = "nopicture.gif";
                    }

                    if (PDescription.Length > 190)
                    {
                        PDescription = PDescription.Substring(0, 190) + ".....";

                    }
                    productdescription.InnerHtml = PDescription;
                    productimmage.Attributes["src"] = "../images/product/icon/" + PImagename;
                    productimmageAfterLogin.Attributes["src"] = "../images/product/icon/" + PImagename;

                    SetLoginSpecificValues(); 

                }
            }
        }
    }

    protected void SetLoginSpecificValues()
    {
        String CustomerLevel = ThisCustomer.CustomerLevelID.ToString();
        if (CustomerLevel == "7" || CustomerLevel == "3" || CustomerLevel == "1" || CustomerLevel == "2")
        {
            //sales rep or internal user
            controlHeadingforPromotionalItems.InnerHtml = "Our brand is their brand";
            productdescriptionAfterLogin.InnerHtml = "Help your dealers express their support for JELD-WEN with a variety of quality branded merchandise. ";
        }
        else if (CustomerLevel == "4" || CustomerLevel == "5" || CustomerLevel == "6")
        {
            //dealer login
            controlHeadingforPromotionalItems.InnerHtml = "Suit up for the showroom.";
            productdescriptionAfterLogin.InnerHtml = "Inside you’ll find a variety of JELD-WEN merchandise to enhance the showroom experience, all of which can be purchased with BLU Bucks or through your regular business account. ";
        }
    }
}