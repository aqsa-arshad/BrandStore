// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for cloneproduct.
    /// </summary>
    public partial class cloneproduct : System.Web.UI.Page
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            int ProductID = CommonLogic.QueryStringUSInt("ProductID");

            if (ProductID != 0)
            {
                int NewProductID = 0;

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("aspdnsf_CloneProduct " + ProductID.ToString(), dbconn))
                    {
                        if (rs.Read())
                        {
                            NewProductID = DB.RSFieldInt(rs, "ProductID");
                        }
                    }
                }
                if (NewProductID != 0)
                {
                    Response.Redirect(AppLogic.AdminLinkUrl("editproduct.aspx") + "?productid=" + NewProductID.ToString());
                }
            }
            Response.Redirect(AppLogic.AdminLinkUrl("products.aspx"));
        }

    }
}

