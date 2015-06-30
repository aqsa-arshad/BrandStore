// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System.Data;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for ShoppingCart_change.
	/// </summary>
	public partial class ShoppingCart_change : SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            int ShoppingCartRecID = CommonLogic.QueryStringUSInt("CartRecID");
            int ProductID = CommonLogic.QueryStringUSInt("ProductId");
		    string sename = CommonLogic.QueryStringCanBeDangerousContent( "SEName" );
            if (ShoppingCartRecID == 0)
			{
				Response.Redirect("ShoppingCart.aspx");
			}
           
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using(IDataReader rs = DB.GetRS("select * from ShoppingCart  with (NOLOCK)  where ShoppingCartRecID=" + ShoppingCartRecID.ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString(),dbconn))
                {
                    if (rs.Read())
                    {
                        ProductID = DB.RSFieldInt(rs, "ProductID");
                    }
                }
            }
          
            if (AppLogic.IsAKit(ProductID))
            {
                // move this item back to the kitcart

                // nuke any "temp in build state" kit for this customer for this product:
                DB.ExecuteSQL("delete from kitcart where ShoppingCartRecID=0 and ProductID=" + ProductID.ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString());

                // move the "cart" back to the constructino state in kitcart for this kit product:
                DB.ExecuteSQL("update kitcart set ShoppingCartRecID=0 where ProductID=" + ProductID.ToString() + " and ShoppingCartRecID=" + ShoppingCartRecID.ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString());

                // remove the actual cart record again now, so it's not left in the cart by accident:
                DB.ExecuteSQL("delete from ShoppingCart where ShoppingCartRecID=" + ShoppingCartRecID.ToString() + " and CustomerID=" + ThisCustomer.CustomerID.ToString());
            }

            if (AppLogic.IsAKit(ProductID))
            {
                Response.Redirect("showproduct.aspx?productid=" + ProductID.ToString() + "&edit=" + ShoppingCartRecID.ToString() + "&SEName=" + sename.ToString());
            }
            else
            {
                Response.Redirect("showproduct.aspx?productid=" + ProductID.ToString() + "&cartrecid=" + ShoppingCartRecID.ToString() + "&edit=" + ShoppingCartRecID.ToString() + "&SEName=" + sename.ToString());
            }
		}

	}
}
