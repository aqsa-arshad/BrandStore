using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using AspDotNetStorefrontCore;

/// <summary>
/// Summary description for Categories
/// </summary>
public class CustomXsltExtension : XSLTExtensionBase
{
    public CustomXsltExtension()
        : this(null, 1, null)
    {
    }

    public CustomXsltExtension(Customer cust, int SkinID, Dictionary<string, EntityHelper> EntityHelpers)
        : base(cust, SkinID)
    {
    }

    private string GetString(HttpContext context, string key)
    {
        var skinId = AppLogic.GetStoreSkinID(AppLogic.StoreID());
        var customer = ((AspDotNetStorefrontPrincipal)context.User).ThisCustomer;
        return AppLogic.GetString(key, skinId, customer.LocaleSetting);
    }

    public virtual string GetPartSelectorButtonIfExists(int productId)
    {
        var context = HttpContext.Current;

        if (context.Session["PART_LOCATOR"] != null && Convert.ToBoolean(context.Session["PART_LOCATOR"]))
        { 
            Product p = new Product(productId);

            if (context.Session["PART_SKU"] != null)
            {
                var sessionSku = Convert.ToString(context.Session["PART_SKU"]);
                var productSku = p.SKU;

                if (sessionSku.ToUpper() == productSku.ToUpper())
                {
                    var buttonText = GetString(context, "Custom.BackToPartSelector");

                    return String.Format("<div class=\"back-to-partlocator\"><a href=\"/partlocator.aspx?sku={1}\" class=\"btn call-to-action\">{0}</a></div>", buttonText, sessionSku);
                }
            }
        }

        return "";
    }

    public virtual string GetBalanceList(int categoryId)
    {
        var output = new StringBuilder();
        output.Append("<ul>");

        using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
        {
            string query = "EXEC custom_GetBalanceList @CategoryID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CategoryID", categoryId);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var name = reader["Name"];
                    var url = String.Format("c-{0}-{1}.aspx", reader["CategoryID"], reader["SEName"]);
                    var total = reader["Total"];
                    var tier = Convert.ToInt32(reader["Tier"]);
                    var isParent = (tier == 1);
                    var isSelected = Convert.ToInt32(reader["CategoryID"]) == categoryId;

                    var parentClass = isParent ? "cat-parent tier-" + tier : "cat-child tier-" + tier;
                    var selectedClass = isSelected ? " selected" : "";
                    var arrow = isSelected ? "<span class=\"arrow\">&#9658;</span>" : "";

                    output.Append(String.Format("<li class=\"{0}{1}\">{5}<a href=\"{2}\">{3}</a><span>({4})</span></li>", parentClass, selectedClass, url, name, total, arrow));
                }
            }
            finally
            {
                reader.Close();
            }
        }

        output.Append("</ul>");

        return output.ToString();
    }

    public virtual string GetCategoryList(string categoryId)
    {
        var output = new StringBuilder();
        output.Append("<ul>");

        using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
        {
            string query = "EXEC custom_GetCategoryList @CategoryID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CategoryID", categoryId);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var name = reader["Name"];
                    var url = String.Format("c-{0}-{1}.aspx", reader["CategoryID"], reader["SEName"]);
                    var total = reader["Total"];
                    var isSelected = false;

                    var selectedClass = isSelected ? " selected" : "";
                    var arrow = isSelected ? "<span class=\"arrow\">&#9658;</span>" : "";

                    output.Append(String.Format("<li class=\"{0}{1}\">{5}<a href=\"{2}\">{3}</a><span>({4})</span></li>", "", selectedClass, url, name, total, arrow));
                }
            }
            finally
            {
                reader.Close();
            }
        }

        output.Append("</ul>");

        var result = output.ToString();
        if (result == "<ul></ul>")
            return "";

        return "<div id=\"staticCategoryList\"><h3>Categories</h3>" + result + "</div>";
    }

    public virtual string GetSubCategoryListByCategory(string categoryId)
                                
    {        
        var output = new StringBuilder();
        output.Append("<ul class='dropdown-menu'>");
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
        {
            string query = "EXEC custom_GetSubCategoryListByCategory @CategoryID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CategoryID", categoryId);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var name = reader["Name"];
                    var url = String.Format("c-{0}-{1}.aspx", reader["CategoryID"], reader["SEName"]);                  
                    var isSelected = false;
                    var selectedClass = isSelected ? " selected" : "";
                    var arrow = isSelected ? "<span class=\"arrow\">&#9658;</span>" : "";
                    output.Append(String.Format("<li class=\"{0}{1}\"><a href=\"{2}\">{3}</a></li>", "", selectedClass, url, name));
                }
            }
            finally
            {
                reader.Close();
            }
        }

        output.Append("</ul>");

        var result = output.ToString();
        if (result == "<ul></ul>")
            return "";

        return result;
    }
    public virtual string GetCategoryRootLevelList(int categoryId)
    {
        var output = new StringBuilder();

        using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
        {
            string query = "EXEC custom_GetCategoryList @CategoryID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CategoryID", categoryId);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var catId = Convert.ToInt32(reader["CategoryID"]);
                    var tier = Convert.ToInt32(reader["Tier"]);

                    if (tier == 1)
                        continue;

                    string name = reader["Name"].ToString();
                    string imageFilenameOverride = reader["ImageFilenameOverride"].ToString();

                    var url = String.Format("c-{0}-{1}.aspx", reader["CategoryID"], reader["SEName"]);

                    var imageLocalPath = AppLogic.LookupImage("Category", catId, imageFilenameOverride, string.Empty, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    var imageUrl = "<img id=\"CategoryPic" + catId + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(name), name, "ProductPic" + catId) + "\" class=\"product-image icon-image img-responsive\" src=\"" + imageLocalPath + "\">";          

                    output.Append("<div class=\"col-md-6\"> <div class=\"thumbnail\">");
                    
                    output.Append("<a href=\"" + url + "\" \">");
                    output.Append("<h4>" + name + "</h4></a>");

                    output.Append("<a class=\"product-img-box\" href=\"" + url + "\" \">");
                    output.Append(imageUrl + "</a>");

                    output.Append("<a class=\"btn btn-primary btn-block\" role=\"button\" href=\"" + url + "\" \">");
                    output.Append("VIEW All" + "</a>");

                    output.Append("</div></div>");
                }
            }
            finally
            {
                reader.Close();
            }
        }

        return output.ToString();
    }

    public virtual string GetCategoryBalanceLevelList(int categoryId)
    {
        var output = new StringBuilder();

        using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
        {
            string query = "EXEC custom_GetBalanceList @CategoryID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CategoryID", categoryId);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var catId = Convert.ToInt32(reader["CategoryID"]);
                    var tier = Convert.ToInt32(reader["Tier"]);

                    if (tier == 1 | tier == 2)
                        continue;

                    var parentCatId = Convert.ToInt32(reader["ParentCategoryID"]);

                    if (parentCatId != categoryId)
                        continue;

                    var name = reader["Name"];
                    var url = String.Format("c-{0}-{1}.aspx", reader["CategoryID"], reader["SEName"]);
                    var total = reader["Total"];

                    var products = GetProductListByCategory(catId);

                    output.Append("<div class=\"top-category-container\">");
                    output.Append("<div class=\"title-container\"><h3>" + name + " <span>(" + total + ")</span></h3>");
                    output.Append("<a href=\"" + url + "\" class=\"btn call-to-action\">View All " + name + "</a></div>");
                    output.Append("<table border=\"0\" cellpadding=\"0\" id=\"ProductTable\" cellspacing=\"4\" width=\"100%\">");
                    output.Append("<tr>");
                    output.Append(products);
                    output.Append("</tr>");
                    output.Append("</table>");
                    output.Append("</div>");
                }
            }
            finally
            {
                reader.Close();
            }
        }

        return output.ToString();
    }

    public virtual string GetProductListByCategory(int categoryId)
    {
        var output = new StringBuilder();

        using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["DBConn"]))
        {
            //string query = "EXEC aspdnsf_GetProducts @categoryID, @pagesize";
            string query = "exec aspdnsf_GetProducts @categoryID = " + categoryId + ", @pagesize = 4";

            SqlCommand command = new SqlCommand(query, connection);
            //command.Parameters.AddWithValue("@categoryID", categoryId);
            //command.Parameters.AddWithValue("@pagesize", 4);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                reader.NextResult();

                var count = 0;
                while (reader.Read())
                {
                    var productId = Convert.ToInt32(reader["ProductID"]);
                    Product p = new Product(productId);

                    var image = AppLogic.LookupImage("Product", productId, p.ImageFilenameOverride, p.SKU, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    
                    var name = p.Name;
                    var seName = p.SEName;
                    var variantId = p.Variants.Count > 0 ? p.Variants[0].VariantID : 0;
                    var x = AppLogic.GetVariantPrice(variantId);
                    var price = String.Format("{0:C}", reader["Price"]);
                    var url = String.Format("p-{0}-{1}.aspx", productId, seName);

                    //price = String.Format("{0:C}", Convert.ToString(x));

                    output.Append("<td align=\"center\" valign=\"top\" class=\"guidedNavProductCell\" style=\"width:25%;\">");
                    output.Append("<div class=\"guidedNavImageWrap\"><a href=\"" + url + "\"></a>");
                    output.Append("<div class=\"image-wrap product-image-wrap\"><a href=\"" + url + "\">");
                    output.Append("<div id=\"divProductPic" + productId + "\" class=\"medium-image-wrap\">");
                    output.Append("<img id=\"ProductPic" + productId + "\" class=\"product-image medium-image img-responsive\" alt=\"" + name + "\" src=\"" + image + "\" name=\"" + seName + "\">");
                    output.Append("</div></a>");
                    output.Append("</div>");
                    output.Append("</div>");
                    output.Append("<div class=\"guidedNavNameWrap\">");
                    output.Append("<a href=\"" + url + "\">" + name + "</a>");
                    output.Append("</div>");
                    output.Append("<div class=\"guidedNavPriceWrap\">");
                    output.Append("<div class=\"price-wrap\">");
                    output.Append("<div class=\"variant-price\">");
                    output.Append("<span>Price:</span> " + price);
                    output.Append("<meta content=\"" + price + "\" itemprop=\"price\">");
                    output.Append("<meta content=\"USD\" itemprop=\"priceCurrency\">");
                    output.Append("</div>");
                    output.Append("</div>");
                    output.Append("</div>");
                    output.Append("</td>");

                    count++;
                }
                    
                if (count < 4)
                {
                    for (int i = 0; i < 4 - count; i++)
                    {
                        output.Append("<td align=\"center\" valign=\"top\" class=\"guidedNavProductCell\" style=\"width:25%;\"></td>");
                    }
                }
            }
            finally
            {
                reader.Close();
            }

            return output.ToString();
        }
    }

    public virtual string GetExtensionDataByProduct(int productId)
    {
        var output = new StringBuilder();

        Product p = new Product(productId);

        var extensionData = String.Format("<data><items>{0}</items></data>", p.ExtensionData);

        XmlSerializer serializer = new XmlSerializer(typeof(data));
        StringReader rdr = new StringReader(extensionData);
        data d = (data)serializer.Deserialize(rdr);

        if (d != null && d.items != null)
        {
            foreach (var item in d.items)
            {
                var name = item.name;
                var orientation = item.orientation;
                var quantity = item.quantity;

                foreach (var part in item.parts)
                {
                    var color_code = part.color_code;
                    var titan_number = part.titan_number;

                    //output.Append("<input type=\"hidden\" name=\"" + color_code + "[]\" value=\"" + quantity + "."  + titan_number + "\" />");
                }
            }
        }

        return output.ToString();
    }
}

[Serializable]
public sealed class data
{
    public item[] items { get; set; }
}

[Serializable]
public class item
{
    [XmlAttribute("name")]
    public string name { get; set; }

    [XmlAttribute("orientation")]
    public string orientation { get; set; }

    [XmlAttribute("quantity")]
    public string quantity { get; set; }

    [XmlElement("part", Type = typeof(part))]
    public part[] parts { get; set; }
}

[Serializable]
public class part
{
    [XmlAttribute("color_code")]
    public string color_code { get; set; }

    [XmlAttribute("titan_number")]
    public string titan_number { get; set; }
}

/*
<data>
    <parts>
        <part kit_ID="0990" item_order="1" part_ID="20990" color="WH" qty="1"  />
        <part kit_ID="0990" item_order="2" part_ID="21816"  color="WH" qty="1"  />
        <part kit_ID="0990" item_order="3" part_ID="21912" color="WH" qty="2"  />
        <part kit_ID="0990" item_order="4" part_ID="21541" color="WH" qty="2"  />
        <part kit_ID="0990" item_order="1" part_ID="20991" color="DS" qty="1" />
        <part kit_ID="0990" item_order="2" part_ID="21817"  color="DS" qty="1"  />
        <part kit_ID="0990" item_order="3" part_ID="21913" color="DS" qty="2"  />
        <part kit_ID="0990" item_order="4" part_ID="21542" color="DS" qty="2"  />
        <part kit_ID="0990" item_order="1" part_ID="21070" color="AL" qty="1" />
        <part kit_ID="0990" item_order="2" part_ID="21818"  color="AL" qty="1"  />
        <part kit_ID="0990" item_order="3" part_ID="21914" color="AL" qty="2"  />
        <part kit_ID="0990" item_order="4" part_ID="21543" color="AL" qty="2"  />
    </parts>
</data>
 */