// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Data;
using System.Text;
using System.Globalization;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for displayorder.
    /// </summary>
    public partial class entityBulkDisplayOrder : AdminPageBase
    {
        private string eName;
        private int eID;
        private EntitySpecs eSpecs;
        private XmlDocument EntityXml;
        private string EntityPlural;

        protected void Page_Load(object sender, EventArgs e)
        {
            eID = CommonLogic.QueryStringNativeInt("EntityID");
            eName = CommonLogic.QueryStringCanBeDangerousContent("EntityName");
            eSpecs = EntityDefinitions.LookupSpecs(eName);

            switch (eName.ToUpperInvariant())
            {
                case "SECTION":
                    ViewState["entityname"] = "Section";
                    EntityPlural = "Sections";
                    break;
                case "MANUFACTURER":
                    ViewState["entityname"] = "Manufacturer";
                    EntityPlural = "Manufacturers";
                    break;
                case "DISTRIBUTOR":
                    ViewState["entityname"] = "Distributor";
                    EntityPlural = "Distributors";
                    break;
                case "GENRE":
                    ViewState["entityname"] = "Genre";
                    EntityPlural = "Genres";
                    break;
                case "VECTOR":
                    ViewState["entityname"] = "Vector";
                    EntityPlural = "Vectors";
                    break;
                case "LIBRARY":
                    ViewState["entityname"] = "Library";
                    EntityPlural = "Libraries";
                    break;
                default:
                    ViewState["entityname"] = "Category";
                    EntityPlural = "Categories";
                    break;
            }

            if (eID == 0)
            {
                lblpagehdr.Text = "Set " + ViewState["entityname"].ToString() + " Display Order";
                lblpagehdr.Visible = true;
            }
            else
            {
                lblpagehdr.Visible = false;
            }

            if (!IsPostBack)
            {
                EntityXml = new EntityHelper(0, eSpecs, !AppLogic.IsAdminSite, 0).m_TblMgr.XmlDoc;
                LoadBody();
            }
        }

        private void LoadBody()
        {

            XmlNodeList nodelist = EntityXml.SelectNodes("//Entity[ParentEntityID=" + eID.ToString() + "]");
            subcategories.DataSource = nodelist;
            subcategories.DataBind();

            if (nodelist.Count > 0)
            {
                pnlNoSubEntities.Visible = false;
                pnlSubEntityList.Visible = true;
            }
            else
            {
                lblError.Text = "This " + ViewState["entityname"].ToString() + " has no sub-" + EntityPlural;
            }

        }


        protected void UpdateDisplayOrder(object sender, EventArgs e)
        {
            foreach (RepeaterItem ri in subcategories.Items)
            {
                TextBox d = (TextBox)ri.FindControl("DisplayOrder");
                TextBox eid = (TextBox)ri.FindControl("entityid");

                string displayorder = CommonLogic.IIF(CommonLogic.IsInteger(d.Text), d.Text, "1");
                DB.ExecuteSQL("update " + ViewState["entityname"].ToString() + " set displayorder = " + displayorder + " where " + ViewState["entityname"].ToString() + "ID = " + eid.Text);
            }

            //refresh the static entityhelper
            switch (ViewState["entityname"].ToString().ToUpperInvariant())
            {
                case "CATEGORY":
                    AppLogic.CategoryStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), true, 0);
                    EntityXml = new EntityHelper(0, EntityDefinitions.LookupSpecs("Category"), false, 0).m_TblMgr.XmlDoc;

                    break;
                case "SECTION":
                    AppLogic.SectionStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), true, 0);
                    EntityXml = new EntityHelper(0, EntityDefinitions.LookupSpecs("Section"), false, 0).m_TblMgr.XmlDoc;
                    break;
                case "MANUFACTURER":
                    AppLogic.ManufacturerStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Manufacturer"), true, 0);
                    EntityXml = new EntityHelper(0, EntityDefinitions.LookupSpecs("Manufacturer"), false, 0).m_TblMgr.XmlDoc;
                    break;
                case "DISTRIBUTOR":
                    AppLogic.DistributorStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Distributor"), true, 0);
                    EntityXml = new EntityHelper(0, EntityDefinitions.LookupSpecs("Distributor"), false, 0).m_TblMgr.XmlDoc;
                    break;
                case "GENRE":
                    AppLogic.GenreStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), true, 0);
                    EntityXml = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), false, 0).m_TblMgr.XmlDoc;
                    break;
                case "VECTOR":
                    AppLogic.VectorStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Vector"), true, 0);
                    EntityXml = new EntityHelper(0, EntityDefinitions.LookupSpecs("Genre"), false, 0).m_TblMgr.XmlDoc;
                    break;
                case "LIBRARY":
                    AppLogic.LibraryStoreEntityHelper[0] = new EntityHelper(0, EntityDefinitions.LookupSpecs("Library"), true, 0);
                    EntityXml = new EntityHelper(0, EntityDefinitions.LookupSpecs("Library"), false, 0).m_TblMgr.XmlDoc;
                    break;
            }

            LoadBody();
        }

        public string getLocaleValue(XmlNode n, string locale)
        {
            XmlNode xn = n.SelectSingleNode(".//locale[@name='" + locale + "']");
            if (xn != null)
            {
                return xn.InnerText;
            }
            else
            {
                return n.InnerText;
            }
        }
    }
}
