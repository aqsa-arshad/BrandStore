﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using AspDotNetStorefrontCore;

public partial class controls_MarkeetingExample : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        using (SqlConnection conn = DB.dbConn())
        {
            conn.Open();
            String CategoryID = AppLogic.AppConfig("MarkeetingMaterialCategoryID");// ConfigurationManager.AppSettings["MarkeetingMaterial"];
            using (System.Data.IDataReader rs = DB.GetRS("aspdnsf_GetLatestProductByCategory " + CategoryID, conn))
            {
                if (rs.Read())
                {

                    String PGuid = rs["ProductGUID"].ToString();
                    String PID = rs["ProductID"].ToString(); 
                    String PDescription = rs["Description"].ToString()+"";
                    String PImagename = rs["Imagename"].ToString();

                    if (PDescription.Length > 187)
                    {
                        PDescription = PDescription.Substring(0, 187) + ".....";
                    }
                   
                    productdescription.InnerHtml=PDescription;
                    productimmage.Attributes["src"] = "../images/product/icon/" + PImagename + ".jpg";
                }
            }
        }
    }
}