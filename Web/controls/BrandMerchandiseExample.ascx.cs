﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using AspDotNetStorefrontCore;

public partial class controls_BrandMerchandiseExample : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
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
                    productdescription.InnerHtml=PDescription;
                    productimmage.Attributes["src"] = "../images/product/icon/" + PImagename;

                    productdescriptionAfterLogin.InnerHtml = PDescription;
                    productimmageAfterLogin.Attributes["src"] = "../images/product/icon/" + PImagename;
                }
            }
        }
    }
}