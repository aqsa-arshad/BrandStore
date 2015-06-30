// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontCore.Validation;

namespace AspDotNetStorefront
{
    public partial class EmailProduct : System.Web.UI.UserControl
    {
        string SourceEntity = "Category";
        int SourceEntityID = 0;

        String ProductName = string.Empty;
        String VariantName = string.Empty;
        String SEName = string.Empty;
        String ProductDescription = string.Empty;
        bool RequiresReg;

        private int m_productid;
        public int ProductID
        {
            get { return m_productid; }
            set { m_productid = value; }
        }

        int CategoryID;
        int SectionID;
        int ManufacturerID;

        String CategoryName;
        String SectionName;
        String ManufacturerName;

        Customer ThisCustomer;

        int baseSkinID = 1;

        protected void Page_Load(object sender, EventArgs e)
        {
            EntityHelper CategoryHelper = AppLogic.LookupHelper("Category", 0);

            ThisCustomer = (Page as SkinBase).ThisCustomer;
            baseSkinID = (Page as SkinBase).SkinID;

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS("select p.*, pv.name variantname from product p  with (NOLOCK)  join productvariant pv  with (NOLOCK)  on p.ProductID = pv.ProductID and pv.isdefault = 1 where p.ProductID=" + ProductID.ToString(), conn))
                {
                    if (!rs.Read())
                    {
                        Response.Redirect("default.aspx");
                    }
                    SEName = DB.RSField(rs, "SEName");
                    ProductName = DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting);
                    VariantName = DB.RSFieldByLocale(rs, "VariantName", ThisCustomer.LocaleSetting);

                    RequiresReg = DB.RSFieldBool(rs, "RequiresRegistration");
                    ProductDescription = DB.RSFieldByLocale(rs, "Description", ThisCustomer.LocaleSetting);
                    if (AppLogic.ReplaceImageURLFromAssetMgr)
                    {
                        ProductDescription = ProductDescription.Replace("../images", "images");
                    }
                    String FileDescription = new ProductDescriptionFile(ProductID, ThisCustomer.LocaleSetting, baseSkinID).Contents;
                    if (FileDescription.Length != 0)
                    {
                        ProductDescription += "<div align=\"left\">" + FileDescription + "</div>";
                    }
                }
            }

            String SourceEntityInstanceName = String.Empty;

            SourceEntity = Profile.LastViewedEntityName;
            SourceEntityInstanceName = Profile.LastViewedEntityInstanceName;
            SourceEntityID = int.Parse(CommonLogic.IIF(CommonLogic.IsInteger(Profile.LastViewedEntityInstanceID), Profile.LastViewedEntityInstanceID, "0")); ;

            // validate that source entity id is actually valid for this product:
            if (SourceEntityID != 0)
            {
                String sqlx = "select count(*) as N from dbo.productentity  with (NOLOCK)  where ProductID=" + ProductID.ToString() + " and EntityID=" + SourceEntityID.ToString() + " and EntityType = " + DB.SQuote(SourceEntity);
                if (DB.GetSqlN(sqlx) == 0)
                {
                    SourceEntityID = 0;
                }
            }

            // we had no entity context coming in, try to find a category context for this product, so they have some context if possible:
            if (SourceEntityID == 0)
            {
                SourceEntityID = EntityHelper.GetProductsFirstEntity(ProductID, EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName);
                if (SourceEntityID > 0)
                {
                    CategoryID = SourceEntityID;
                    CategoryName = CategoryHelper.GetEntityName(CategoryID, ThisCustomer.LocaleSetting);
                    Profile.LastViewedEntityName = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
                    Profile.LastViewedEntityInstanceID = CategoryID.ToString();
                    Profile.LastViewedEntityInstanceName = CategoryName;
                    SourceEntity = EntityDefinitions.readonly_CategoryEntitySpecs.m_EntityName;
                    SourceEntityInstanceName = CategoryName;
                }
            }

            // we had no entity context coming in, try to find a section context for this product, so they have some context if possible:
            if (SourceEntityID == 0)
            {
                SourceEntityID = EntityHelper.GetProductsFirstEntity(ProductID, EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName);
                if (SourceEntityID > 0)
                {
                    SectionID = SourceEntityID;
                    SectionName = CategoryHelper.GetEntityName(SectionID, ThisCustomer.LocaleSetting);
                    Profile.LastViewedEntityName = EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName;
                    Profile.LastViewedEntityInstanceID = SectionID.ToString();
                    Profile.LastViewedEntityInstanceName = SectionName;
                    SourceEntity = EntityDefinitions.readonly_SectionEntitySpecs.m_EntityName;
                    SourceEntityInstanceName = SectionName;
                }
            }

            // we had no entity context coming in, try to find a Manufacturer context for this product, so they have some context if possible:
            if (SourceEntityID == 0)
            {
                SourceEntityID = EntityHelper.GetProductsFirstEntity(ProductID, EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName);
                if (SourceEntityID > 0)
                {
                    ManufacturerID = SourceEntityID;
                    ManufacturerName = CategoryHelper.GetEntityName(ManufacturerID, ThisCustomer.LocaleSetting);
                    Profile.LastViewedEntityName = EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName;
                    Profile.LastViewedEntityInstanceID = ManufacturerID.ToString();
                    Profile.LastViewedEntityInstanceName = ManufacturerName;
                    SourceEntity = EntityDefinitions.readonly_ManufacturerEntitySpecs.m_EntityName;
                    SourceEntityInstanceName = ManufacturerName;
                }
            }

            // build up breadcrumb if we need:
            (Page as SkinBase).SectionTitle = Breadcrumb.GetProductBreadcrumb(ProductID, ProductName, SourceEntity, SourceEntityID, ThisCustomer);

            reqToAddress.ErrorMessage = AppLogic.GetString("emailproduct.aspx.13", baseSkinID, ThisCustomer.LocaleSetting);
            regexToAddress.ErrorMessage = AppLogic.GetString("emailproduct.aspx.14", baseSkinID, ThisCustomer.LocaleSetting);
            reqFromAddress.ErrorMessage = AppLogic.GetString("emailproduct.aspx.16", baseSkinID, ThisCustomer.LocaleSetting);
            regexFromAddress.ErrorMessage = AppLogic.GetString("emailproduct.aspx.17", baseSkinID, ThisCustomer.LocaleSetting);

            if (!this.IsPostBack)
            {
                InitializePageContent();
            }
        }

        private void InitializePageContent()
        {
            pnlRequireReg.Visible = (RequiresReg && !ThisCustomer.IsRegistered);
            this.pnlEmailToFriend.Visible = !(RequiresReg && !ThisCustomer.IsRegistered);

            emailproduct_aspx_1.Text = "<b>" + AppLogic.GetString("emailproduct.aspx.1", baseSkinID, ThisCustomer.LocaleSetting) + "</b><a href=\"signin.aspx?returnurl=showproduct.aspx?" + Server.HtmlEncode(Server.UrlEncode(CommonLogic.ServerVariables("QUERY_STRING"))) + "\">" + AppLogic.GetString("emailproduct.aspx.2", baseSkinID, ThisCustomer.LocaleSetting) + "</a> " + AppLogic.GetString("emailproduct.aspx.3", baseSkinID, ThisCustomer.LocaleSetting);

            String ProdPic = String.Empty;
			ProdPic = AppLogic.LookupImage("Product", ProductID, "medium", baseSkinID, ThisCustomer.LocaleSetting);

            imgProduct.ImageUrl = ProdPic;
            ProductNavLink.NavigateUrl = SE.MakeProductAndEntityLink(this.SourceEntity, ProductID, SourceEntityID, SEName);
            ProductNavLink.Text = AppLogic.GetString("emailproduct.aspx.24", baseSkinID, ThisCustomer.LocaleSetting);
            emailproduct_aspx_4.Text = AppLogic.GetString("emailproduct.aspx.4", baseSkinID, ThisCustomer.LocaleSetting) + " " + ProductName + CommonLogic.IIF(VariantName.Length > 0, " - " + VariantName, "");
            emailproduct_aspx_11.Text = AppLogic.GetString("emailproduct.aspx.11", baseSkinID, ThisCustomer.LocaleSetting);
            emailproduct_aspx_22.Text = AppLogic.GetString("emailproduct.aspx.22", baseSkinID, ThisCustomer.LocaleSetting);
            emailproduct_aspx_15.Text = AppLogic.GetString("emailproduct.aspx.15", baseSkinID, ThisCustomer.LocaleSetting);
            emailproduct_aspx_18.Text = AppLogic.GetString("emailproduct.aspx.18", baseSkinID, ThisCustomer.LocaleSetting);
            emailproduct_aspx_19.Text = AppLogic.GetString("emailproduct.aspx.19", baseSkinID, ThisCustomer.LocaleSetting);
            btnSubmit.Text = AppLogic.GetString("emailproduct.aspx.20", baseSkinID, ThisCustomer.LocaleSetting);
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (Page.IsValid)
            {
                String FromAddress = txtFromAddress.Text;
                String ToAddress = txtToAddress.Text;
                String Message = txtMessage.Text;

                String BotAddress = AppLogic.AppConfig("ReceiptEMailFrom");
                String Subject = AppLogic.AppConfig("StoreName") + " - " + ProductName;
                StringBuilder Body = new StringBuilder(4096);

                Body.Append(AppLogic.RunXmlPackage("notification.emailproduct.xml.config", null, ThisCustomer, baseSkinID, "", "message=" + HttpUtility.UrlEncode(Message) + "&fromaddress=" + HttpUtility.UrlEncode(FromAddress), false, false, AppLogic.MakeEntityHelpers()));

                EMail em = new EMail();

                em.FromAddress = FromAddress;
                em.EmailAddress = ToAddress;
                em.IncludeFooter = false;
                em.MailServer = AppLogic.MailServer();
                em.MailSubject = Subject;
                em.MailContents = Body.ToString();
                em.MailType = BulkMailTypeEnum.None;
                em.UseHTML = true;

                em.Send();

                //AppLogic.SendMail(Subject, Body.ToString(), true, BotAddress, BotAddress, ToAddress, ToAddress, "", FromAddress, AppLogic.MailServer());

                emailproduct_aspx_8.Text = AppLogic.GetString("emailproduct.aspx.8", baseSkinID, ThisCustomer.LocaleSetting);
                pnlSuccess.Visible = true;
                pnlRequireReg.Visible = false;
                pnlEmailToFriend.Visible = false;
                ReturnToProduct.Text = AppLogic.GetString("emailproduct.aspx.10", baseSkinID, ThisCustomer.LocaleSetting);
                ReturnToProduct.NavigateUrl = SE.MakeProductAndEntityLink(this.SourceEntity, ProductID, SourceEntityID, SEName);
            }
            else
            {
                InitializePageContent();
            }
        }

    }
}
