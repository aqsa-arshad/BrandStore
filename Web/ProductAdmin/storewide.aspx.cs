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
using AspDotNetStorefrontCore;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for storewide.
    /// </summary>
    public partial class storewide : AdminPageBase
    {
        private string BeforeOrderDate1;
        private string BeforeOrderDate2;
        private Customer cust;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (!IsPostBack)
            {
                LoadData();
            }
            BeforeOrderDate1 = Localization.ToThreadCultureShortDateString(System.DateTime.Now.AddMonths(-3));
            BeforeOrderDate2 = Localization.ToThreadCultureShortDateString(System.DateTime.Now.AddMonths(-3));

        }

        protected void LoadData()
        {
            ddDiscountCate.Items.Clear();
            ddDiscountDep.Items.Clear();
            ddDiscountManu.Items.Clear();
            ddDiscountTable.Items.Clear();

            ddOnSale.Items.Clear();
            ddOnSaleCat.Items.Clear();
            ddOnSaleDep.Items.Clear();
            ddOnSaleManu.Items.Clear();

            ddOnSale.Items.Add(new ListItem("- Select -", "0"));

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rsst = DB.GetRS("select * from salesprompt   with (NOLOCK)  where deleted=0", con))
                {
                    while (rsst.Read())
                    {
                        ListItem li = new ListItem();
                        li.Value = DB.RSFieldInt(rsst, "SalesPromptID").ToString();
                        li.Text = DB.RSFieldByLocale(rsst, "Name", cust.LocaleSetting);

                        ddOnSale.Items.Add(li);
                    }
                }
            }

            ddDiscountTable.Items.Add(new ListItem("- Select -", "0"));

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rsst = DB.GetRS("select * from quantitydiscount   with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    while (rsst.Read())
                    {
                        ListItem li = new ListItem();
                        li.Value = DB.RSFieldInt(rsst, "QuantityDiscountID").ToString();
                        li.Text = DB.RSFieldByLocale(rsst, "Name", cust.LocaleSetting);

                        ddDiscountTable.Items.Add(li);
                    }
                }
            }

            //Manufacturers
            ddDiscountManu.Items.Add(new ListItem("All Manufacturers", "0"));
            ddOnSaleManu.Items.Add(new ListItem("All Manufacturers", "0"));

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select * from Manufacturer   with (NOLOCK)  where deleted=0 order by DisplayOrder,Name",conn))
                {
                    while (rs.Read())
                    {
                        ListItem li = new ListItem();
                        li.Value = DB.RSFieldInt(rs, "ManufacturerID").ToString();
                        li.Text = Server.HtmlDecode(DB.RSFieldByLocale(rs, "Name", cust.LocaleSetting));

                        ddOnSaleManu.Items.Add(li);
                        ddDiscountManu.Items.Add(li);
                    }
                }
            
            }
          
            //Sections
            ddDiscountDep.Items.Add(new ListItem("All " + AppLogic.GetString("AppConfig.SectionPromptPlural", 1, cust.LocaleSetting), "0"));
            ddOnSaleDep.Items.Add(new ListItem("All " + AppLogic.GetString("AppConfig.SectionPromptPlural", 1, cust.LocaleSetting), "0"));
            EntityHelper eTemp = new EntityHelper(EntityDefinitions.readonly_SectionEntitySpecs, 0);
            ArrayList al = eTemp.GetEntityArrayList(0, "", 0, cust.LocaleSetting, false);
            for (int i = 0; i < al.Count; i++)
            {
                ListItemClass lic = (ListItemClass)al[i];
                string value = lic.Value.ToString();
                string name = Server.HtmlDecode(lic.Item);

                ddDiscountDep.Items.Add(new ListItem(name, value));
                ddOnSaleDep.Items.Add(new ListItem(name, value));
            }

            //Categories
            ddDiscountCate.Items.Add(new ListItem("All " + AppLogic.GetString("AppConfig.CategoryPromptPlural", 1, cust.LocaleSetting), "0"));
            ddOnSaleCat.Items.Add(new ListItem("All " + AppLogic.GetString("AppConfig.CategoryPromptPlural", 1, cust.LocaleSetting), "0"));
            eTemp = new EntityHelper(EntityDefinitions.readonly_CategoryEntitySpecs, 0);
            al = eTemp.GetEntityArrayList(0, "", 0, cust.LocaleSetting, false);
            for (int i = 0; i < al.Count; i++)
            {
                ListItemClass lic = (ListItemClass)al[i];
                string value = lic.Value.ToString();
                string name = Server.HtmlDecode(lic.Item);

                ddDiscountCate.Items.Add(new ListItem(name, value));
                ddOnSaleCat.Items.Add(new ListItem(name, value));
            }
        }
        protected void btnSubmit1_Click(object sender, EventArgs e)
        {
            if (ddOnSale.SelectedValue != "0")
            {
                DB.ExecuteSQL("Update product set SalesPromptID=" + ddOnSale.SelectedValue + " where 1=1 " + CommonLogic.IIF(ddOnSaleCat.SelectedValue != "0", " and productid in (select distinct productid from productcategory   with (NOLOCK)  where categoryid=" + ddOnSaleCat.SelectedValue + ")", "") + CommonLogic.IIF(ddOnSaleDep.SelectedValue != "0", " and productid in (select distinct productid from productsection   with (NOLOCK)  where SectionID=" + ddOnSaleDep.SelectedValue + ")", "") + CommonLogic.IIF(ddOnSaleManu.SelectedValue != "0", " and productid in (select distinct productid from productmanufacturer   with (NOLOCK)  where manufacturerid=" + ddOnSaleManu.SelectedValue + ")", ""));
                resetError("On Sale Prompt updated", false);
            }
        }
        protected void btnSubmit2_Click(object sender, EventArgs e)
        {
            DB.ExecuteSQL("Update product set SpecTitle=" + DB.SQuote(txtSpec.Text));
            resetError("Spec Title updated", false);
        }
        protected void btnSubmit3_Click(object sender, EventArgs e)
        {
            foreach (ListItem li in rblSpecsInline.Items)
            {
                if (li.Selected)
                {
                    DB.ExecuteSQL("Update product set SpecsInline=" + li.Value);
                    resetError("Specs Inline updated", false);
                }
            }
        }
        protected void btnSubmit4_Click(object sender, EventArgs e)
        {
            DB.ExecuteSQL("Update product set QuantityDiscountID=" + ddDiscountTable.SelectedValue);
            resetError("Quantity Tables set", false);
        }
        protected void btnSubmit5_Click(object sender, EventArgs e)
        {
            DB.ExecuteSQL("Update productvariant set SalePrice=" + CommonLogic.IIF(txtDiscountPercent.Text == "0" || txtDiscountPercent.Text == "0.0" || txtDiscountPercent.Text == "0.00", "NULL", "Round(Price*(1-(" + txtDiscountPercent.Text + "/100.0)),2)") + " where 1=1 " + CommonLogic.IIF(ddDiscountCate.SelectedValue != "0", " and productid in (select distinct productid from productcategory   with (NOLOCK)  where categoryid=" + ddDiscountCate.SelectedValue + ")", "") + CommonLogic.IIF(ddDiscountDep.SelectedValue != "0", " and productid in (select distinct productid from productsection   with (NOLOCK)  where SectionID=" + ddDiscountDep.SelectedValue + ")", "") + CommonLogic.IIF(ddDiscountManu.SelectedValue != "0", " and productid in (select distinct productid from productmanufacturer   with (NOLOCK)  where manufacturerid=" + ddDiscountManu.SelectedValue + ")", ""));
            resetError("Sales Discount percentage set", false);
        }
        protected void btnSubmit6_Click(object sender, EventArgs e)
        {
            DB.ExecuteSQL("aspdnsf_ResetAllProductVariantDefaults");
            resetError("Default Variants set", false);
        }
        protected void btnSubmit7_Click(object sender, EventArgs e)
        {
            string sql = "update product set sename = {0} where productid = {1};";
            StringBuilder SQLBatch = new StringBuilder("set nocount on;");
            int counter = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader dr = DB.GetRS("select productid, name from dbo.product", con))
                {
                    while (dr.Read())
                    {
                        SQLBatch.Append(string.Format(sql, DB.SQuote(SE.MungeName(DB.RSFieldByLocale(dr, "name", Localization.GetDefaultLocale()))), DB.RSFieldInt(dr, "productid").ToString()));
                        counter++;
                        if (counter == 500)
                        {
                            DB.ExecuteSQL(SQLBatch.ToString());
                            counter = 0;
                            SQLBatch.Length = 0;
                            SQLBatch.Append("set nocount on;");
                        }
                    }
                }
            }

            if (SQLBatch.Length > 0)
            {
                DB.ExecuteSQL(SQLBatch.ToString());
            }
            resetError("SEName Updated", false);
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }

    }
}
