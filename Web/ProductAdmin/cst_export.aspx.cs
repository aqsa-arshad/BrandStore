// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Text;
using System.Web;
using System.Xml;
using System.Data;
using System.Xml.Serialization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
	/// <summary>
	/// Summary description for cst_export.
	/// </summary>
	public partial class cst_export : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			if(ThisCustomer.IsAdminUser)
			{
				Response.Expires = -1;
				Response.ContentType = "text/xml";
				// Create a new XmlTextWriter instance
				XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, Encoding.UTF8);
    
				// start writing!
				writer.WriteStartDocument();
				writer.WriteStartElement("CustomerList");

                string SuperuserFilter = string.Empty;
                IntegerCollection superUserIds = AppLogic.GetSuperAdminCustomerIDs();
                if (superUserIds.Count > 0)
                {
                    String.Format(" Customer.CustomerID not in ({0}) and ", superUserIds.ToString());
                }

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select * from customer   with (NOLOCK)  where " + SuperuserFilter.ToString() + " deleted=0 and EMail<> '' order by createdon desc", dbconn))
                    {
                        while (rs.Read())
                        {
                            writer.WriteStartElement("Customer");
                            writer.WriteElementString("FirstName", DB.RSField(rs, "FirstName"));
                            writer.WriteElementString("LastName", DB.RSField(rs, "LastName"));
                            writer.WriteElementString("EMail", DB.RSField(rs, "EMail"));
                            writer.WriteElementString("OKToEMail", DB.RSFieldBool(rs, "OKToEMail").ToString());
                            writer.WriteElementString("CreatedOn", Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs, "CreatedOn")));
                            writer.WriteElementString("RegisteredOn", Localization.ToNativeDateTimeString(DB.RSFieldDateTime(rs, "RegisteredOn")));
                            writer.WriteEndElement();
                        }
                    }
                }
	    
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Close();    
			}
			else
			{
				Response.Expires = -1;
				Response.Write("<html><body>");
                Response.Write(AppLogic.GetString("admin.common.InsufficientPrivilege", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
				Response.Write("</body></html>");
			}
		}

	}
}
