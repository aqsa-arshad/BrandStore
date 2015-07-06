// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class orderOption : AdminPageBase
    {
        protected bool bUseHtmlEditor;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            lblMsg.Text = string.Empty;

            if (!IsPostBack)
            {
                ViewState["EditingOrderOptions"] = false;
                ViewState["EditingOrderOptionsID"] = "0";

                loadDD();
                loadDDLocales();
                loadDL();
                loadScript(false);
                resetForm();
                phMain.Visible = false;

                btnDelete.Attributes.Add("onClick", "return confirm('Are you sure you want to delete this Order Option?');");
            }

            // Determine HTML editor configuration
            bUseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
            radDescription.Visible = bUseHtmlEditor;
            radDescription.DisableFilter(Telerik.Web.UI.EditorFilters.RemoveScripts);
        }

        private void loadDD()
        {
            foreach (XmlNode xn in Localization.LocalesDoc.SelectNodes("//Locales"))
            {
                ddlPageLocales.Items.Add(new ListItem(xn.Attributes["Name"].InnerText, xn.Attributes["Name"].InnerText));
            }
            ddlPageLocales.Items.FindByValue(Localization.GetDefaultLocale()).Selected = true;

            if (ddlPageLocales.Items.Count < 2)
            {
                divlblLocale.Visible = false;
                divPageLocale.Visible = false;
            }
        }

        private void HookUpClientSideValidation()
        {
            StringBuilder script = new StringBuilder();
            script.Append("<script type=\"text/javascript\"  language=\"javascript\" >\n");
            script.Append("function validate(){\n");
            script.Append("var exp = new RegExp(/^\\d*(\\.\\d*)?$/);\n");
            script.Append("var cost = document.getElementById('" + txtCost.ClientID + "');\n");
            script.Append("if(cost.value == null || cost.value == '') cost.value = 0.0;\n");
            script.Append("if(!cost.value.match(exp))\n");
            script.Append("{\n alert('" + AppLogic.GetString("admin.orderOption.InvalidNumberFormat", SkinID, LocaleSetting) + "'); \n return false;\n}");

            // validate cost format..

            script.Append("return true;\n");
            script.Append("}\n");
            script.Append("</script>\n");

            Page.ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), script.ToString());
        }

        protected void getOrderOptionsDetails()
        {
            HookUpClientSideValidation();

            string pageLocale = ddlPageLocales.SelectedValue;
            if (pageLocale.Equals(string.Empty))
            {
                pageLocale = Localization.GetDefaultLocale();
            }

            string iden = ViewState["EditingOrderOptionsID"].ToString();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("select * from OrderOption   with (NOLOCK)  where OrderOptionID=" + iden, dbconn))
                {
                    if (!rs.Read())
                    {
                        rs.Close();
                        resetError(AppLogic.GetString("admin.common.UnableToRetrieveData", SkinID, LocaleSetting), true);
                        return;
                    }

                    //editing OrderOptions
                    bool Editing = Localization.ParseBoolean(ViewState["EditingOrderOptions"].ToString());
                    ltMode.Text = AppLogic.GetString("admin.common.Editing", SkinID, LocaleSetting);
                    btnSubmit.Text =AppLogic.GetString("admin.orderOption.UpdateOrderOption", SkinID, LocaleSetting);
                    btnDelete.Visible = true;

                    ltName.Text = DB.RSFieldByLocale(rs, "Name", pageLocale);
                    if (bUseHtmlEditor)
                    {
                        radDescription.Content = DB.RSFieldByLocale(rs, "Description", pageLocale);
                    }
                    else
                    {
                        ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                        ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\">" + XmlCommon.GetLocaleEntry(HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Description")), pageLocale, false) + "</textarea>\n");
                        ltDescription.Text += ("</div>");
                    }

                    rbIsChecked.SelectedIndex = CommonLogic.IIF(DB.RSFieldBool(rs, "DefaultIsChecked"), 1, 0);

                    ddTaxClass.Items.Clear();

                    using (SqlConnection dbconn2 = DB.dbConn())
                    {
                        dbconn2.Open();
                        using (IDataReader rsst = DB.GetRS("select * from TaxClass   with (NOLOCK)  order by DisplayOrder,Name", dbconn2))
                        {
                            while (rsst.Read())
                            {
                                ddTaxClass.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", LocaleSetting), DB.RSFieldInt(rsst, "TaxClassID").ToString()));
                            }
                        }
                    }
                    ddTaxClass.Items.FindByValue(DB.RSFieldInt(rs, "TaxClassID").ToString()).Selected = true;
                    txtCost.Text = CommonLogic.IIF(DB.RSFieldDecimal(rs, "Cost") != System.Decimal.Zero, Localization.CurrencyStringForDBWithoutExchangeRate(DB.RSFieldDecimal(rs, "Cost")), "");

                    //IMAGE
                    ltIcon.Text = "";
                    //String Image1URL = AppLogic.LookupImage("OrderOption", Localization.ParseNativeInt(iden), "icon", 1, LocaleSetting);
                    String Image1URL = AppLogic.LookupImage("OrderOption", Localization.ParseNativeInt(iden), "icon", 1, ThisCustomer.LocaleSetting);

                    if (Image1URL.Length != 0)
                    {
                        if (Image1URL.IndexOf("nopicture") == -1)
                        {
                            ltIcon.Text = String.Format(AppLogic.GetString("admin.common.Clickheretodeleteimage", SkinID, LocaleSetting),"<a href=\"javascript:void(0);\" onClick=\"DeleteImage('~/" + Image1URL + "','OrderPic');\">","</a>") + "<br/>\n";
                        }
                        ltIcon.Text += ("<br/><img id=\"OrderPic\" name=\"OrderPic\" border=\"0\" src=\"" + Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\">\n");
                    }
                }
            }

        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            ltValid.Text = "";

            ViewState["EditingOrderOptions"] = false;
            ViewState["EditingOrderOptionsID"] = "0";
            loadScript(true);
            resetForm();
            phMain.Visible = true;
            btnDelete.Visible = false;
            loadDL();
            //new OrderOptions
            ltMode.Text = "Adding an";
            btnSubmit.Text = "Add Order Option";
        }

        protected void loadScript(bool load)
        {
            if (load)
            {
                if (AppLogic.NumLocaleSettingsInstalled() > 1)
                {
                    ltScript.Text += "<script type='text/javascript' src='Scripts/tabs.js'></script>";
                }
            }
            else
            {
                ltScript.Text = "";
                ltStyles.Text = "";
            }

            ltScript2.Text = ("<script type=\"text/javascript\">\n");
            ltScript2.Text += ("function DeleteImage(imgurl,name)\n");
            ltScript2.Text += ("{\n");
            ltScript2.Text += ("window.open('" + AppLogic.AdminLinkUrl("deleteimage.aspx") + "?imgurl=' + imgurl + '&FormImageName=' + name,\"AspDotNetStorefrontAdmin_ML\",\"height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
            ltScript2.Text += ("}\n");
            ltScript2.Text += ("</SCRIPT>\n");

            HookUpClientSideValidation();
        }

        protected bool validateForm()
        {
            bool valid = true;
            string temp = "";
            //string focus = "";

            if (ltName.Text.Length == 0)
            {
                valid = false;
                temp += AppLogic.GetString("admin.common.FillOutNamePrompt", SkinID, LocaleSetting);
                //focus = "Name";
            }

            return valid;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            updateOrderOptions();
        }

        protected void updateOrderOptions()
        {
            ltValid.Text = "";

            string pageLocale = ddlPageLocales.SelectedValue;
            if (pageLocale.Equals(string.Empty))
            {
                pageLocale = Localization.GetDefaultLocale();
            }

            if (validateForm())
            {
                StringBuilder sql = new StringBuilder();
                decimal Cost = System.Decimal.Zero;
                if (txtCost.Text.Length != 0)
                {
                    Cost = Localization.ParseNativeDecimal(txtCost.Text);
                }

                bool Editing = Localization.ParseBoolean(ViewState["EditingOrderOptions"].ToString());
                int OrderOptionID = Localization.ParseNativeInt(ViewState["EditingOrderOptionsID"].ToString());


                if (!Editing)
                {
                    // ok to add them:
                    String NewGUID = DB.GetNewGUID();
                    sql.Append("insert into OrderOption(OrderOptionGUID,Name,Description,DefaultIsChecked,TaxClassID,Cost) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(AppLogic.FormLocaleXml(ltName.Text, pageLocale)) + ",");
                    String desc = String.Empty;
                    if (bUseHtmlEditor)
                    {
                        desc = AppLogic.FormLocaleXml(radDescription.Content, pageLocale);
                    }
                    else
                    {
                        desc = AppLogic.FormLocaleXmlEditor("Description", "Description", pageLocale, "orderoption", OrderOptionID);
                    }
                    if (desc.Length != 0)
                    {
                        sql.Append(DB.SQuote(desc) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    sql.Append(rbIsChecked.SelectedValue + ",");
                    sql.Append(ddTaxClass.SelectedValue + ",");
                    sql.Append(CommonLogic.IIF(Cost != System.Decimal.Zero, Localization.DecimalStringForDB(Cost), "0.0"));
                    sql.Append(")");
                    DB.ExecuteSQL(sql.ToString());

                    resetError("Order Option added.", false);

                    using (SqlConnection dbconn = DB.dbConn())
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select OrderOptionID from OrderOption   with (NOLOCK)  where OrderOptionGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            OrderOptionID = DB.RSFieldInt(rs, "OrderOptionID");
                            ViewState["EditingOrderOptions"] = true;
                            ViewState["EditingOrderOptionsID"] = OrderOptionID.ToString();
                        }
                    }
                }
                else
                {
                    // ok to update:
                    sql.Append("update OrderOption set ");
                    sql.Append("Name=" + DB.SQuote(AppLogic.FormLocaleXml("Name", ltName.Text, pageLocale, "OrderOption", OrderOptionID)) + ",");
                    String desc = String.Empty;
                    if (bUseHtmlEditor)
                    {
                        desc = AppLogic.FormLocaleXml("Description", radDescription.Content, pageLocale, "OrderOption", OrderOptionID);
                    }
                    else
                    {
                        desc = AppLogic.FormLocaleXmlEditor("Description", "Description", pageLocale, "OrderOption", OrderOptionID);
                    }
                    if (desc.Length != 0)
                    {
                        sql.Append("Description=" + DB.SQuote(desc) + ",");
                    }
                    else
                    {
                        sql.Append("Description=NULL,");
                    }
                    sql.Append("DefaultIsChecked=" + rbIsChecked.SelectedValue + ",");
                    sql.Append("TaxClassID=" + ddTaxClass.SelectedValue + ",");
                    sql.Append("Cost=" + CommonLogic.IIF(Cost != System.Decimal.Zero, Localization.DecimalStringForDB(Cost), "0.0"));
                    sql.Append(" where OrderOptionID=" + OrderOptionID.ToString());
                    DB.ExecuteSQL(sql.ToString());

                    resetError(AppLogic.GetString("admin.orderOption.OrderOptionUpdated", SkinID, LocaleSetting), false);
                }
                StoresMapping.ObjectID = OrderOptionID;
                StoresMapping.Save();

                loadDL();
                // handle image uploaded:
                try
                {
                    String Image1 = String.Empty;
                    HttpPostedFile Image1File = fuIcon.PostedFile;
                    if (Image1File.ContentLength != 0)
                    {
                        // delete any current image file first
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath("OrderOption", "icon", true) + OrderOptionID.ToString() + ".jpg");
                            System.IO.File.Delete(AppLogic.GetImagePath("OrderOption", "icon", true) + OrderOptionID.ToString() + ".gif");
                            System.IO.File.Delete(AppLogic.GetImagePath("OrderOption", "icon", true) + OrderOptionID.ToString() + ".png");
                        }
                        catch
                        { }

                        String s = Image1File.ContentType;
                        switch (Image1File.ContentType)
                        {
                            case "image/gif":
                                Image1 = AppLogic.GetImagePath("OrderOption", "icon", true) + OrderOptionID.ToString() + ".gif";
                                Image1File.SaveAs(Image1);
                                break;
                            case "image/x-png":
                                Image1 = AppLogic.GetImagePath("OrderOption", "icon", true) + OrderOptionID.ToString() + ".png";
                                Image1File.SaveAs(Image1);
                                break;
                            case "image/jpg":
                            case "image/jpeg":
                            case "image/pjpeg":
                                Image1 = AppLogic.GetImagePath("OrderOption", "icon", true) + OrderOptionID.ToString() + ".jpg";
                                Image1File.SaveAs(Image1);
                                break;
                        }
                    }

                    getOrderOptionsDetails();
                }
                catch (Exception ex)
                {
                    ltError.Text = CommonLogic.GetExceptionDetail(ex, "<br/>");
                }
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            deleteOrderOptions();
        }

        protected void deleteOrderOptions()
        {
            ltValid.Text = "";

            string OrderOptionsID = ViewState["EditingOrderOptionsID"].ToString();
            // delete the mfg:
            DB.ExecuteSQL("delete from orderoption where OrderOptionID=" + OrderOptionsID);
            phMain.Visible = false;
            loadDL();
            loadScript(false);
            ViewState["EditingOrderOptions"] = false;
            ViewState["EditingOrderOptionsID"] = "0";
            resetError(AppLogic.GetString("admin.orderOption.OrderOptionDeleted", SkinID, LocaleSetting), false);
        }

        protected void resetForm()
        {
            ltName.Text = "";
            if (bUseHtmlEditor)
            {
                radDescription.Content = "";
                ltDescription.Text = "";
            }
            else
            {
                ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\"></textarea>\n");
                ltDescription.Text += ("</div>");
            }
            txtCost.Text = "";
            rbIsChecked.SelectedIndex = 1;
            ltIcon.Text = "";
            //load Tax Class
            ddTaxClass.Items.Clear();

            if (ddlPageLocales.Items.Count < 2)
            {
                ddlPageLocales.SelectedValue = Localization.GetDefaultLocale();
            }

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rsst = DB.GetRS("select * from TaxClass   with (NOLOCK)  order by DisplayOrder,Name", dbconn))
                {
                    while (rsst.Read())
                    {
                        ddTaxClass.Items.Add(new ListItem(DB.RSFieldByLocale(rsst, "Name", LocaleSetting), DB.RSFieldInt(rsst, "TaxClassID").ToString()));
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {

            int OrderOptionID = 0;

            for (int i = 0; i < dlOrderOptions.Items.Count; i++)
            {
                OrderOptionID = int.Parse(dlOrderOptions.DataKeys[i].ToString());
                TextBox txtDispOrder = dlOrderOptions.Items[i].FindControl("txtDisplayOrder") as TextBox;

                DB.ExecuteSQL("update OrderOption set DisplayOrder=" + txtDispOrder.Text + " where OrderOptionID=" + OrderOptionID);
                resetError(AppLogic.GetString("admin.orderOption.DisplayOrderUpdated", SkinID, LocaleSetting), false);
            }
            loadDL();
        }

        protected void ddlPageLocales_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ViewState["EditingOrderOptions"].ToString() != "")
            {
                getOrderOptionsDetails();
            }
        }

        protected void ViewOrderOption(object source, DataListCommandEventArgs e)
        {
            string index = dlOrderOptions.DataKeys[e.Item.ItemIndex].ToString();
            ltValid.Text = "";

            resetForm();
            ViewState["EditingOrderOptions"] = true;
            loadScript(true);

            resetError("", false);
            phMain.Visible = true;

            ViewState["EditingOrderOptionsID"] = index;

            getOrderOptionsDetails();
            loadDL();
            return;


        }

        private void loadDL()
        {
            dlOrderOptions.DataSource = OrderOptionCollection.GetOrderOptionList(ddLocales.SelectedIndex, ddLocales.SelectedItem.Text);
            dlOrderOptions.DataBind();
        }

        private void loadDDLocales()
        {
            ddLocales.Items.Clear();
            ddLocales.Items.Add(new ListItem(AppLogic.GetString("admin.common.AllLocalesUC", SkinID, LocaleSetting), "0"));

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader dr = DB.GetRS("select Name from LocaleSetting with (NOLOCK) order by DisplayOrder,Description", conn))
                {
                    while (dr.Read())
                    {
                        ddLocales.Items.Add(new ListItem(DB.RSField(dr, "Name"), DB.RSField(dr, "Name")));
                    }
                }
            }

            if (ddLocales.Items.Count < 3)
            {
                divLocale.Visible = false;
            }
        }

        protected void ddLocales_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadDL();
        }
    }



    public class OrderOptionCollection
    {
        private string _name = string.Empty;
        private int _id = 0;
        private int _displayOrder = 0;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public int DisplayOrder
        {
            get { return _displayOrder; }
            set { _displayOrder = value; }
        }



        public OrderOptionCollection() { }

        public OrderOptionCollection(string name, int id, int displayOrder)
        {
            _name = name;
            _id = id;
            _displayOrder = displayOrder;
        }

        public static List<OrderOptionCollection> GetOrderOptionList(int LocaleID, string LocaleName)
        {
            List<OrderOptionCollection> OrderOptionList = new List<OrderOptionCollection>();

            string sql = "select * from OrderOption  with (NOLOCK) order by DisplayOrder,Name";

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader dr = DB.GetRS(sql, conn))
                {
                    while (dr.Read())
                    {
                        string name = string.Empty;
                        name = DB.RSFieldByLocale(dr, "Name", CommonLogic.IIF(LocaleID == 0, Localization.GetDefaultLocale(), LocaleName));
                        OrderOptionList.Add(new OrderOptionCollection(CommonLogic.IIF(name.Equals(string.Empty), "[Not Set for this Locale]", name), DB.RSFieldInt(dr, "OrderOptionId"), DB.RSFieldInt(dr, "DisplayOrder")));
                    }
                }
            }

            return OrderOptionList;
        }



    }
}
