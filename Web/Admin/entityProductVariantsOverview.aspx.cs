// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

public partial class entityProductVariantsOverview : System.Web.UI.Page
{
    private Customer ThisCustomer;
    private EntityHelper entity;
    private string eName;
    private int eID;
    private EntitySpecs eSpecs;
    private int SiteID = 1;
    private int pID;
    private string ProductSKU;
    protected string selectSQL = "select * from ProductVariant";
        
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.CacheControl = "private";
        Response.Expires = 0;
        Response.AddHeader("pragma", "no-cache");

        ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

        eID = CommonLogic.QueryStringNativeInt("EntityID");
        eName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
        eSpecs = EntityDefinitions.LookupSpecs(eName);
        entity = new EntityHelper(eSpecs, 0);     
        pID = CommonLogic.QueryStringNativeInt("ProductID");

        ProductSKU = AppLogic.GetProductSKU(pID);

        if (!IsPostBack)
        {
            ltEntity.Text = entity.GetEntityBreadcrumb6(eID, ThisCustomer.LocaleSetting);
            ltProduct.Text = "<a href=\"" + AppLogic.AdminLinkUrl("entityEditProducts.aspx") + "?iden=" + pID + "&entityName=" + eName + "&entityFilterID=" + eID + "\">" + AppLogic.GetProductName(pID, ThisCustomer.LocaleSetting) + " (" + pID + ")</a>";

            string query = CommonLogic.QueryStringCanBeDangerousContent("searchfor");

            ViewState["SQLString"] = selectSQL;

            //set page settings
            if (ThisCustomer.ThisCustomerSession.Session("productVariantsSort").Length == 0)
            {
                ViewState["Sort"] = "DisplayOrder, Name";
            }
            else
            {
                ViewState["Sort"] = ThisCustomer.ThisCustomerSession.Session("productVariantsSort");
            }
            if (ThisCustomer.ThisCustomerSession.Session("productVariantsOrder").Length == 0)
            {
                ViewState["SortOrder"] = "ASC";
            }
            else
            {
                ViewState["SortOrder"] = ThisCustomer.ThisCustomerSession.Session("productVariantsOrder"); 
            }
            if (ThisCustomer.ThisCustomerSession.Session("productVariantsSearch").Length != 0)
            {
                query = ThisCustomer.ThisCustomerSession.Session("productVariantsSearch");
            }

            resultFilter(query);
            
            btnDeleteVariants.Attributes.Add("onclick", string.Format(
                "return confirm('{0}');", 
                DeleteAllPrompt));
            
            if (0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where ProductID=" + pID.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
            {
                btnDeleteVariants.Enabled = false;
                btnDeleteVariants.Visible = false;
            }

            AppLogic.MakeSureProductHasAtLeastOneVariant(pID);
        }

        AppLogic.EnsureProductHasADefaultVariantSet(pID);
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        ViewState["IsInsert"] = false;
        gMain.EditIndex = -1;
        resetError("", false);
        gMain.PageIndex = 0;
        resultFilter("");
    }

    protected void resultFilter(string SearchFor)
    {
        String sql = selectSQL + " with (NOLOCK) ";
        String WhereClause = " Deleted=0 AND ProductID=" + pID + " ";

        //search
        if (SearchFor.Length != 0)
        {
            if (WhereClause.Length != 0)
            {
                WhereClause += " and ";
            }
            WhereClause += " (Name like " + DB.SQuote("%" + SearchFor + "%") + " or Description like " + DB.SQuote("%" + SearchFor + "%") + " or SKUSuffix like " + DB.SQuote("%" + SearchFor + "%") + ")";
        }

        if (WhereClause.Length != 0)
        {
            sql += " where " + WhereClause;
        }

        //set states
        ViewState["SQLString"] = sql.ToString();
        sql += " order by " + ViewState["Sort"].ToString() + " " + ViewState["SortOrder"].ToString();

        ThisCustomer.ThisCustomerSession.SetVal("productVariantsSort", ViewState["Sort"].ToString());
        ThisCustomer.ThisCustomerSession.SetVal("productVariantsOrder", ViewState["SortOrder"].ToString());

        //build grid
        buildGridData(sql);
        
        //remember page
        if (ThisCustomer.ThisCustomerSession.SessionNativeInt("productVariantsPage") > 0)
        {
            gMain.PageIndex = ThisCustomer.ThisCustomerSession.SessionNativeInt("productVariantsPage");
        }

    }

    protected void treeMain_SelectedNodeChanged(object sender, EventArgs e)
    {
        ViewState["IsInsert"] = false;
        gMain.EditIndex = -1;
        resetError("", false);
        gMain.PageIndex = 0;
        resultFilter("");
    }

    protected void ddTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        ViewState["IsInsert"] = false;
        gMain.EditIndex = -1;
        resetError("", false);
        gMain.PageIndex = 0;

        resultFilter("");
    }

    protected void buildGridData(string sql)
    {
        using (DataTable dt = new DataTable())
        {
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    dt.Load(rs);
                    gMain.DataSource = dt;
                    gMain.DataBind();
                }
            }
        }
        
        try
        {
            for (int i = 0; i < gMain.HeaderRow.Cells.Count; i++)
            {
                if (gMain.Columns[i].SortExpression.Equals(ViewState["Sort"].ToString()))
                {
                    Image arrow = new Image();
                    if (ViewState["SortOrder"].ToString().Equals("asc", StringComparison.InvariantCultureIgnoreCase))
                    {
                        arrow.ImageUrl = "icons/asc.gif";
                    }
                    else
                    {
                        arrow.ImageUrl = "icons/desc.gif";
                    }
                    gMain.HeaderRow.Cells[i].Controls.Add(arrow);
                }
            }
        }
        catch { }
    }

    protected void resetError(string error, bool isError)
    {
        string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Error", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
        if (isError)
            str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

        if (error.Length > 0)
            str += error + "";
        else
            str = "";

        ltError.Text = str;
    }
    
    protected void btnAdd_Click(object sender, EventArgs e)
    {
        Response.Redirect(AppLogic.AdminLinkUrl("entityEditProductVariant.aspx")+ "?ProductID=" + pID + "&entityname=" + eName + "&EntityID=" + eID + "&Variantid=0");
    }

    private string DeletePrompt
    {
        get { return AppLogic.GetString("EntityProductOverview.ConfirmDelete", ThisCustomer.SkinID, ThisCustomer.LocaleSetting); }
    }
    private string ClonePrompt
    {
        get { return AppLogic.GetString("EntityProductOverview.ConfirmClone", ThisCustomer.SkinID, ThisCustomer.LocaleSetting); }
    }
    private string DeleteAllPrompt
    {
        get { return AppLogic.GetString("EntityProductOverview.ConfirmDeleteAll", ThisCustomer.SkinID, ThisCustomer.LocaleSetting); }
    }

    protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DataRowView myrow = (DataRowView)e.Row.DataItem;

            //set delete and action confirms
            ImageButton iD = (ImageButton)e.Row.FindControl("imgDelete");
            iD.Attributes.Add("onClick", string.Format(
                "javascript: return confirm('{0}');",
                    string.Format(DeletePrompt, myrow["VariantID"])
                    ));
            
            LinkButton lCc = (LinkButton)e.Row.FindControl("lnkClone");
            lCc.Attributes.Add("onClick", 
                string.Format("javascript: return confirm('{0}')", ClonePrompt));

            //Name and Image
            Literal ltName = (Literal)e.Row.FindControl("ltName");
            ltName.Text += ("<a href=\"" + AppLogic.AdminLinkUrl("entityEditProductVariant.aspx") + "?ProductID=" + pID + "&entityname=" + eName + "&EntityID=" + eID + "&Variantid=" + myrow["VariantID"].ToString() + "\">" + CommonLogic.IIF(XmlCommon.GetLocaleEntry(myrow["Name"].ToString(), ThisCustomer.LocaleSetting, true).Length == 0, "(Unnamed Variant)", XmlCommon.GetLocaleEntry(myrow["Name"].ToString(), ThisCustomer.LocaleSetting, true)) + "</a>");

            Literal ltImage = (Literal)e.Row.FindControl("ltImage");
            String Image1URL = AppLogic.LookupImage("Variant", Localization.ParseNativeInt(myrow["VariantID"].ToString()), "icon", SiteID, ThisCustomer.LocaleSetting);
            if (!CommonLogic.FileExists(Image1URL))
            {
                Image1URL = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/nopictureicon.gif", ThisCustomer.LocaleSetting);
            }
            ltImage.Text = "<img src=\"" + Image1URL + "\" width=\"25\" border=\"0\" align=\"absmiddle\">";
            

            //SKU
            Literal ltSKU = (Literal)e.Row.FindControl("ltSKU");
            ltSKU.Text = ProductSKU + myrow["SKUSuffix"].ToString();

            //Price
            Literal ltPrice = (Literal)e.Row.FindControl("ltPrice");
            ltPrice.Text = Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseNativeDecimal(myrow["Price"].ToString()));
            ltPrice.Text += CommonLogic.IIF(Localization.ParseNativeDecimal(myrow["SalePrice"].ToString()) != System.Decimal.Zero, "<br/><span style=\"font-weight: bold; color: red;\">" + Localization.CurrencyStringForDisplayWithoutExchangeRate(Localization.ParseNativeDecimal(myrow["SalePrice"].ToString())) + "</span>", "&nbsp;");

            //Inventory
            Literal ltInventory = (Literal)e.Row.FindControl("ltInventory");
            if (AppLogic.ProductTracksInventoryBySizeAndColor(Localization.ParseNativeInt(myrow["ProductID"].ToString())))
            {
                ltInventory.Text = ("<a href=\"" + AppLogic.AdminLinkUrl("entityEditInventory.aspx") + "?productid=" + myrow["ProductID"].ToString() + "&Variantid=" + myrow["VariantID"].ToString() + "&entityname=" + eName + "&EntityID=" + eID + "\">" + AppLogic.GetString("admin.common.Inventory", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>\n");
            }
            else
            {
                ltInventory.Text = (myrow["Inventory"].ToString());
            }
           
            //Display Order
            Literal ltDisplayOrder = (Literal)e.Row.FindControl("ltDisplayOrder");
            ltDisplayOrder.Text = "<input size=2 type=\"text\" name=\"DisplayOrder_" + myrow["VariantID"].ToString() + "\" value=\"" + Localization.ParseNativeInt(myrow["DisplayOrder"].ToString()) + "\">";

            //Default Variant
            Literal ltDefault = (Literal)e.Row.FindControl("ltDefault");
            ltDefault.Text = "<input type=\"radio\" name=\"IsDefault\" value=\"" + myrow["VariantID"].ToString() + "\" " + CommonLogic.IIF(myrow["IsDefault"].ToString() == "1", " checked ", "") + ">";
        }
    }

    protected void gMain_Sorting(object sender, GridViewSortEventArgs e)
    {
        gMain.EditIndex = -1;
        ViewState["Sort"] = e.SortExpression.ToString();
        ViewState["SortOrder"] = (ViewState["SortOrder"].ToString() == "ASC" ? "DESC" : "ASC");
        resultFilter("");
    }
    protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        resetError("", false);

        if (e.CommandName == "DeleteItem")
        {
            gMain.EditIndex = -1;
            int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
            deleteRow(iden);
        }
        else if (e.CommandName == "CloneItem")
        {
            gMain.EditIndex = -1;
            int iden = Localization.ParseNativeInt(e.CommandArgument.ToString());
            cloneRow(iden);
        }
    }

    protected void cloneRow(int iden)
    {
        if (iden != 0)
        {
            DB.ExecuteSQL("aspdnsf_CloneVariant " + iden.ToString() + "," + ThisCustomer.CustomerID.ToString());
            resultFilter("");
            resetError(AppLogic.GetString("admin.entityProductVariantsOverview.VariantCloned", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), false);
        }
    }

    protected void deleteRow(int iden)
    {
        if (0 < DB.GetSqlN("select count(*) N from ShoppingCart  with (NOLOCK)  where VariantID=" + iden.ToString() + " and CartType=" + ((int)CartTypeEnum.RecurringCart).ToString()))
        {
            resetError(AppLogic.GetString("admin.common.DeleteNAInfoRecurring", SiteID, ThisCustomer.LocaleSetting), true);
            return;
        }

        DB.ExecuteSQL("delete from KitCart where VariantID=" + iden.ToString());
        DB.ExecuteSQL("delete from ShoppingCart where VariantID=" + iden.ToString());
        DB.ExecuteSQL("update ProductVariant set Deleted = 1 where VariantID=" + iden.ToString());

        resultFilter("");
        resetError("Product deleted.", false);
    }
    
    protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        resetError("", false);
        gMain.PageIndex = e.NewPageIndex;
        gMain.EditIndex = -1;
        ThisCustomer.ThisCustomerSession.SetVal("ProductsPage", e.NewPageIndex.ToString());
        resultFilter("");
    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        DB.ExecuteSQL("update ProductVariant set IsDefault=0 where ProductID=" + pID.ToString());
        if (CommonLogic.FormCanBeDangerousContent("IsDefault").Length == 0 || CommonLogic.FormUSInt("IsDefault") == 0)
        {
            // try to force a default variant, none was specified!
            DB.ExecuteSQL("update ProductVariant set IsDefault=1 where ProductID=" + pID.ToString() + " and VariantID in (SELECT top 1 VariantID from ProductVariant where ProductID=" + pID.ToString() + " order by DisplayOrder,Name)");
        }
        else
        {
            string temp = CommonLogic.FormUSInt("IsDefault").ToString();
            DB.ExecuteSQL("update ProductVariant set IsDefault=1 where ProductID=" + pID.ToString() + " and VariantID=" + temp);
        }
        for (int i = 0; i <= Request.Form.Count - 1; i++)
        {
            if (Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
            {
                String[] keys = Request.Form.Keys[i].Split('_');
                int VariantID = Localization.ParseUSInt(keys[1]);
                int DispOrd = 1;
                try
                {
                    DispOrd = Localization.ParseUSInt(Request.Form[Request.Form.Keys[i]]);
                }
                catch { }
                DB.ExecuteSQL("update productvariant set DisplayOrder=" + DispOrd.ToString() + " where VariantID=" + VariantID.ToString());
            }
        }

        resultFilter("");
    }

    protected void btnDeleteVariants_Click(object sender, EventArgs e)
    {
        DB.ExecuteSQL("delete from KitCart where VariantID in (select VariantID from ProductVariant where ProductID=" + pID.ToString() + ")");
        DB.ExecuteSQL("delete from ShoppingCart where VariantID in (select VariantID from ProductVariant where ProductID=" + pID.ToString() + ")");
        DB.ExecuteSQL("delete from ProductVariant where ProductID=" + pID.ToString());
    }
}
