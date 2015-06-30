// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for autofill.
    /// </summary>
    public partial class autofill : System.Web.UI.Page
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
            StringBuilder writer = new StringBuilder();
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");
            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
            String FormImageName = CommonLogic.QueryStringCanBeDangerousContent("FormImageName");
            if (ThisCustomer.IsAdminUser)
            {
                int ProductID = CommonLogic.QueryStringUSInt("ProductID");
                if (ProductID != 0)
                {
                    String MiscText = String.Empty;

                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("Select MiscText from product   with (NOLOCK)  where ProductID=" + ProductID.ToString(), dbconn))
                        {
                            if (rs.Read())
                            {
                                MiscText = DB.RSField(rs, "MiscText");
                            }
                        }
                    }
                    if (MiscText.Length != 0)
                    {
                        // MiscText must be in format: name skusuffix price cols
                        String MiscText2 = MiscText.Replace("\n", "|").Replace("\r", "|").Replace("||", "|");
                        foreach (String s in MiscText2.Split('|'))
                        {
                            try
                            {
                                String stmp = s.Trim();
                                while (stmp.IndexOf("  ") != -1)
                                {
                                    stmp = stmp.Replace("  ", " ");
                                }
                                stmp = stmp.Trim();
                                if (stmp.Length != 0)
                                {
                                    String[] sarray = stmp.Split(' ');
                                    String Price = sarray[sarray.GetUpperBound(0)];
                                    String SKUSuffix = sarray[sarray.GetUpperBound(0) - 1];
                                    String Name = String.Empty;
                                    for (int i = 0; i <= sarray.GetUpperBound(0) - 2; i++)
                                    {
                                        Name += sarray[i] + " ";
                                    }
                                    Name = Name.Trim();
                                    if (Price.Length != 0 && Name.Length != 0)
                                    {
                                        String sql = "insert into productvariant(ProductID,Name,SKUSuffix,Price,Inventory,Published) values(" + ProductID.ToString() + "," + DB.SQuote(Name) + "," + DB.SQuote(SKUSuffix) + "," + Price + ",1000000,1)";
                                        Response.Write(String.Format(AppLogic.GetString("admin.autofill.Executing", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),sql) +  "<br/>");
                                        DB.ExecuteSQL(sql);
                                    }
                                    else
                                    {
                                        writer.Append("<p><b>"+String.Format(AppLogic.GetString("admin.autofill.BadLineFormat", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), s) + "</b></p>");
                                    }
                                }
                            }
                            catch
                            {
                                writer.Append("<p><b>" + String.Format(AppLogic.GetString("admin.autofill.ErrorOnLine", ThisCustomer.SkinID, ThisCustomer.LocaleSetting),s) + "</b></p>");
                            }
                        }
                    }
                    else
                    {
                        writer.Append("<p><b>" + AppLogic.GetString("admin.autofill.ProductMiscTextEmpty", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></p>");
                    }
                }
                else
                {
                    writer.Append("<p><b>" + AppLogic.GetString("admin.autofill.NoProductIdSpecified", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></p>");
                }
            }
            writer.Append("<a href=\"javascript:self.close();\">" + AppLogic.GetString("admin.common.Close", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
            ltContent.Text = writer.ToString();
        }
    }
}
