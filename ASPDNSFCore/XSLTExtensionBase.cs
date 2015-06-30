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
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontCore
{
    public class InputValidator
    {
        private String m_RoutineName = String.Empty;

        public InputValidator(String RoutineName)
        {
            m_RoutineName = RoutineName;
        }

        private void ReportError(String ParamName, String ParamValue)
        {
            ArgumentException ex = new ArgumentException("Error Calling XSLTExtension Function " + m_RoutineName + ": Invalid value specified for " + ParamName + " (" + CommonLogic.IIF(ParamValue == null, "null", ParamValue) + ")");
            SysLog.LogException(ex, MessageTypeEnum.XmlPackageException, MessageSeverityEnum.Error);
            throw ex;
        }

        public String ValidateString(String ParamName, String ParamValue)
        {
            if (ParamValue == null)
            {
                ReportError(ParamName, ParamValue);
            }
            return ParamValue;
        }

        public int ValidateInt(String ParamName, String ParamValue)
        {
            if (ParamValue == null ||
                !CommonLogic.IsInteger(ParamValue))
            {
                ReportError(ParamName, ParamValue);
            }
            return Int32.Parse(ParamValue);
        }

        public Decimal ValidateDecimal(String ParamName, String ParamValue)
        {
            if (ParamValue == null ||
                !CommonLogic.IsNumber(ParamValue))
            {
                ReportError(ParamName, ParamValue);
            }
            return Localization.ParseDBDecimal(ParamValue);
        }

        public Double ValidateDouble(String ParamName, String ParamValue)
        {
            if (ParamValue == null ||
                !CommonLogic.IsNumber(ParamValue))
            {
                ReportError(ParamName, ParamValue);
            }
            return Localization.ParseDBDouble(ParamValue);
        }

        public bool ValidateBool(String ParamName, String ParamValue)
        {
            if (ParamValue == null)
            {
                ReportError(ParamName, ParamValue);
            }

            if (ParamValue.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
                ParamValue.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
                ParamValue.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public DateTime ValidateDateTime(String ParamName, String ParamValue)
        {
            DateTime dt = DateTime.MinValue;
            if (ParamValue == null)
            {
                ReportError(ParamName, ParamValue);
            }
            try
            {
                dt = Localization.ParseDBDateTime(ParamValue);
            }
            catch
            {
                ReportError(ParamName, ParamValue);
            }
            return dt;
        }
    }

    /// <summary>
    /// Summary description for XSLTExtensions.
    /// </summary>
    public class XSLTExtensionBase
    {
        /// <summary>
        /// Javascript items
        /// </summary>
        protected static class Scripts
        {
            public static string PopupImage(Size picSize)
            {
                string _script =
                #region PopupImg Script
 @"

<script type='text/javascript'>
function setImageURL(url)
{{
    document.getElementById('popupImageURL').value = url;
}}
function popupimg(url)
{{
window.open(
    '{0}?src=' + document.getElementById('popupImageURL').value,'LargerImage{1}',
    'toolbar=no,location=no,directories=no,status=no, menubar=no,scrollbars={2}, resizable={2},copyhistory=no,width={3},height={4},left=0,top=0');
return (true);
}}
</script>
	                    ";
                #endregion

                return string.Format(_script,
                    new object[]
		{
			AppLogic.ResolveUrl("~/popup.aspx"),
			CommonLogic.GetRandomNumber(1, 100000),
			AppLogic.AppConfigBool("ResizableLargeImagePopup")? "yes": "no",
			picSize.Width,
			picSize.Height,
            ""
		});
            }

            public static string Zoomify()
            {
                string _script =
                #region popupZoom Script
 @"
<script type='text/javascript'>
function popupzoom(url,alturl)
{{
    window.open(
        '{0}?src=' + url + '&altsrc=' + alturl,'LargerImage{1}',
        'toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars={2},resizable={2},copyhistory=no,width={3},height={4},left=0,top=0'
    );
    return (true);
}}
</script>
";
                #endregion
                return string.Format(_script,
                    new object[]{
                            AppLogic.ResolveUrl("~/zoomify.aspx"),
                            CommonLogic.GetRandomNumber(1, 100000).ToString(),
                            AppLogic.AppConfigBool("ResizableLargeImagePopup")? "yes": "no",
                            35 + AppLogic.AppConfigNativeInt("Zoomify.Large.Width"),
                            35 + AppLogic.AppConfigNativeInt("Zoomify.Large.Height")
                        });

            }

        }

        protected bool m_VATEnabled = false;
        protected bool m_VATOn = false;
        protected Customer m_ThisCustomer;
        protected Dictionary<string, EntityHelper> m_EntityHelpers;

        public XSLTExtensionBase(Customer cust, int SkinID)
        {
            m_ThisCustomer = cust;
            if (m_ThisCustomer == null)
            {
                try
                {
                    m_ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;
                }
                catch
                {
                }
                if (m_ThisCustomer == null)
                {
                    m_ThisCustomer = new Customer(true);
                }
            }

            m_VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            m_VATOn = (m_VATEnabled && m_ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);
        }

        //Public Methods
        public Customer ThisCustomer
        {
            get { return m_ThisCustomer; }
        }

        public virtual string SetTrace(string sTraceName)
        {
            InputValidator IV = new InputValidator("SetTrace");
            String TraceName = IV.ValidateString("TraceName", sTraceName);
            if (!HttpContext.Current.Items.Contains("XmlPackageTracePoint"))
            {
                HttpContext.Current.Items.Add("XmlPackageTracePoint", TraceName);
            }
            else
            {
                HttpContext.Current.Items["XmlPackageTracePoint"] = TraceName;
            }
            return String.Empty;
        }

        public virtual String GetRootEntityContextOfPage(String sEntityName)
        {
            InputValidator IV = new InputValidator("GetRootEntityContextOfPage");
            String EntityName = IV.ValidateString("VariantID", sEntityName);

            EntityHelper catHelper = AppLogic.LookupHelper(EntityName, 0);
            int ProductIDQS = CommonLogic.QueryStringUSInt("ProductID");
            int EntityIDQS = CommonLogic.QueryStringUSInt(EntityName + "ID");
            if (CommonLogic.QueryStringCanBeDangerousContent("EntityID").Length != 0)
            {
                EntityIDQS = CommonLogic.QueryStringUSInt("EntityID");
            }
            int EntityIDCookie = CommonLogic.IIF(CommonLogic.CookieCanBeDangerousContent("LastViewedEntityName", true) == EntityName, CommonLogic.CookieUSInt("LastViewedEntityInstanceID"), 0);
            int EntityIDActual = 0;
            int EntityIDRoot = 0;
            if (ProductIDQS != 0)
            {
                // we have a product context, did they get there from a Entity/subcat page or not. cookie is set if so.
                if (EntityIDCookie != 0)
                {
                    EntityIDActual = EntityIDCookie;
                }
                else
                {
                    EntityIDActual = AppLogic.GetFirstProductEntityID(catHelper, ProductIDQS, false);
                }
            }
            else
            {
                if (EntityIDQS != 0)
                {
                    EntityIDActual = EntityIDQS;
                }
            }
            EntityIDRoot = catHelper.GetRootEntity(EntityIDActual);
            return EntityIDRoot.ToString();
        }

        public virtual String AjaxShippingEstimator(String sVariantID)
        {
            InputValidator IV = new InputValidator("SetTrace");
            int VariantID = IV.ValidateInt("VariantID", sVariantID);

            StringBuilder tmpS = new StringBuilder(1024);
            tmpS.Append("<div class=\"form ajax-shipping-wrap\" id=\"AjaxShipping\">");

            tmpS.Append("<div class=\"form-group\" id=\"AjaxShippingCountry\">");
            tmpS.Append("<label>");
            tmpS.Append(AppLogic.GetString("order.cs.66", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
            tmpS.Append("</label>");

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select count(*) as N from country  with (NOLOCK)  where Published = 1; select * from country  with (NOLOCK)  where Published = 1 order by DisplayOrder,Name", con))
                {
                    if (rs.Read() && DB.RSFieldInt(rs, "N") > 0)
                    {
                        int count = DB.RSFieldInt(rs, "N");

                        if (count == 1)
                        {
                            if (rs.NextResult() && rs.Read())
                            {
                                tmpS.Append("<span id=\"AjaxShippingCountrySingleValue\">");
                                tmpS.Append("<input type=\"hidden\" name=\"Country\" id=\"Country\" value=\"" + DB.RSField(rs, "Name").Replace("\"", "") + "\">");
                                tmpS.Append(DB.RSField(rs, "Name"));
                                tmpS.Append("</span>");
                            }
                        }
                        else
                        {
                            if (rs.NextResult())
                            {
                                tmpS.Append("<select name=\"Country\" id=\"Country\" class=\"form-control\" onchange=\"javascript:getShipping();\">");
                                while (rs.Read())
                                {
                                    tmpS.Append("<option value=\"" + DB.RSField(rs, "Name").Replace("\"", "") + "\">" + DB.RSField(rs, "Name") + "</option>");
                                }
                                tmpS.Append("</select>");
                            }
                        }
                    }
                }
            }

            tmpS.Append("</div>");

            tmpS.Append("<div class=\"form-group\" id=\"AjaxShippingState\">");
            tmpS.Append("<label>");
            tmpS.Append(AppLogic.GetString("order.cs.64", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
            tmpS.Append("</label>");

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select count(*) as N from state  with (NOLOCK); select * from state  with (NOLOCK)  order by DisplayOrder,Name", con))
                {
                    if (rs.Read() && DB.RSFieldInt(rs, "N") > 0)
                    {
                        int count = DB.RSFieldInt(rs, "N");

                        if (count == 1)
                        {
                            if (rs.NextResult() && rs.Read())
                            {
                                tmpS.Append("<input type=\"hidden\" name=\"State\" id=\"State\" value=\"" + DB.RSField(rs, "Name").Replace("\"", "") + "\">");
                                tmpS.Append("<span id=\"AjaxShippingStateSingleValue\">");
                                tmpS.Append(DB.RSField(rs, "Name"));
                                tmpS.Append("</span>");
                            }
                        }
                        else
                        {
                            if (rs.NextResult())
                            {
                                tmpS.Append("<select name=\"State\" class=\"form-control\" id=\"State\" onchange=\"javascript:getShipping();\">");
                                while (rs.Read())
                                {
                                    tmpS.Append("<option value=\"" + DB.RSField(rs, "Abbreviation") + "\">" + DB.RSField(rs, "Name") + "</option>");
                                }
                                tmpS.Append("</select>");
                            }
                        }
                    }
                }
            }

            tmpS.Append("</div>");

            tmpS.Append("<div class=\"form-group\" id=\"AjaxShippingZip\">");
            tmpS.Append("<label>");
            tmpS.Append(AppLogic.GetString("order.cs.65", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
            tmpS.Append("</div>");
            tmpS.Append("<label class=\"AjaxShippingZip\" type=\"text\" class=\"form-control\" maxlength=\"6\" id=\"PostalCode\" name=\"PostalCode\" onkeyup=\"javascript:getShipping();\"/>");
            tmpS.Append("</div>");

            tmpS.Append("<div id=\"AjaxShippingEstimate\">");
            tmpS.Append("<div class=\"AjaxShippingLabel\">Shipping Estimate:</div>");
            tmpS.Append("<p id=\"ShipQuote\"></p>");
            tmpS.Append("</div>");

            tmpS.Append("</div>");
            return tmpS.ToString();
        }

        public virtual string RemoteUrl(string sURL)
        {
            InputValidator IV = new InputValidator("RemoteUrl");
            String URL = IV.ValidateString("URL", sURL);
            return CommonLogic.AspHTTP(URL, 30);
        }

        public string StripHtml(String sTheString)
        {
            InputValidator IV = new InputValidator("StripHtml");
            String TheString = IV.ValidateString("TheString", sTheString);
            return AppLogic.StripHtml(TheString);
        }

        public virtual string PagingControl(string sBaseURL, String sPageNum, String sNumPages)
        {
            InputValidator IV = new InputValidator("PagingControl");
            String BaseURL = IV.ValidateString("BaseURL", sBaseURL);
            int PageNum = IV.ValidateInt("PageNum", sPageNum);
            int NumPages = IV.ValidateInt("NumPage", sNumPages);

            if (AppLogic.AppConfigBool("Paging.ShowAllPageLinks"))
                return Paging.GetAllPagesOldFormat(BaseURL, PageNum, NumPages, ThisCustomer);
            else
                return Paging.GetPagedPages(BaseURL, PageNum, NumPages, ThisCustomer);
        }

        public virtual string SkinID()
        {
            return ThisCustomer.SkinID.ToString();
        }

        public virtual void SendMail(String sSubject, String sBody, String sUseHtml, String sToAddress)
        {
            InputValidator IV = new InputValidator("SendMail");
            String Subject = IV.ValidateString("Subject", sSubject);
            String Body = IV.ValidateString("Body", sBody);
            bool UseHtml = IV.ValidateBool("UseHtml", sUseHtml);
            String ToAddress = IV.ValidateString("ToAddress", sToAddress);
            String Srv = AppLogic.MailServer().Trim();
            if (Srv.Length != 0 &&
                Srv != AppLogic.ro_TBD)
            {
                AppLogic.SendMail(Subject, Body, UseHtml, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromAddress"), ToAddress, ToAddress, String.Empty, Srv);
            }
        }

        public virtual string CustomerID()
        {
            return ThisCustomer.CustomerID.ToString();
        }

        public virtual string User_Name()
        {
            string result = String.Empty;
            if (!ThisCustomer.IsRegistered)
            {
                result = String.Empty;
            }
            else
            {
                if (AppLogic.IsAdminSite)
                {
                    result = ThisCustomer.FullName();
                }
                else
                {
                    result = "skinbase.cs.1".StringResource() + " <a class=\"username\" href=\"account.aspx\">" + ThisCustomer.FullName() + "</a>" + CommonLogic.IIF(ThisCustomer.CustomerLevelID != 0, "(" + ThisCustomer.CustomerLevelName + ")", "");
                }
            }
            return result;
        }

        public virtual string User_Menu_Name()
        {
            string result = String.Empty;
            if (!ThisCustomer.IsRegistered)
            {
                result = AppLogic.GetString("skinbase.cs.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
            else
            {
                result = ThisCustomer.FullName();
            }
            return result;
        }

        public virtual string Store_Version(String sNotUsed)
        {
            return StoreVersion(sNotUsed);
        }

        public virtual string StoreVersion(String sNotUsed)
        {
            return CommonLogic.GetVersion();
        }

        public virtual string OnLiveServer(String sNotUsed)
        {
            return AppLogic.OnLiveServer().ToString().ToLowerInvariant();
        }

        //The following functions are for backward compatibility with Parser functions only and 
        //should not be used in XmlPackage transforms because the output invalid XML when using the IncludeATag
        //Newer fucntions that produce well formed output are below
        public virtual string ManufacturerLink(String sManufacturerID, String sSEName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("ManufacturerLink");
            int ManufacturerID = IV.ValidateInt("ManufacturerID", sManufacturerID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeManufacturerLink(ManufacturerID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\" />";
            }
            return result;
        }

        public virtual string CategoryLink(String sCategoryID, String sSEName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("CategoryLink");
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeCategoryLink(CategoryID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\" />";
            }
            return result;
        }

        public virtual string SectionLink(String sSectionID, String sSEName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("SectionLink");
            int SectionID = IV.ValidateInt("SectionID", sSectionID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeSectionLink(SectionID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\" />";
            }
            return result;
        }

        public virtual string LibraryLink(String sLibraryID, String sSEName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("LibraryLink");
            int LibraryID = IV.ValidateInt("LibraryID", sLibraryID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeEntityLink("Library", LibraryID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\" />";
            }
            return result;
        }

        public virtual string ProductLink(String sProductID, String sSEName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("ProductLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeProductLink(ProductID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\" />";
            }
            return result;
        }

        public virtual string DocumentLink(String sDocumentID, String sSEName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("DocumentLink");
            int DocumentID = IV.ValidateInt("DocumentID", sDocumentID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeObjectLink("Document", DocumentID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">";
            }
            return result;
        }

        public virtual string ProductandCategoryLink(String sProductID, String sSEName, String sCategoryID, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("ProductandCategoryLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeProductAndCategoryLink(ProductID, CategoryID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">";
            }
            return result;
        }

        public virtual string ProductandSectionLink(String sProductID, String sSEName, String sSectionID, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("ProductandSectionLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int SectionID = IV.ValidateInt("SectionID", sSectionID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeProductAndSectionLink(ProductID, SectionID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">";
            }
            return result;
        }

        public virtual string ProductandManufacturerLink(String sProductID, String sSEName, String sManufacturerID, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("ProductandManufacturerLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int ManufacturerID = IV.ValidateInt("ManufacturerID", sManufacturerID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeProductAndEntityLink("Manufacturer", ProductID, ManufacturerID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">";
            }
            return result;
        }

        public virtual string DocumentandLibraryLink(String sDocumentID, String sSEName, String sLibraryID, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("DocumentandLibraryLink");
            int DocumentID = IV.ValidateInt("DocumentID", sDocumentID);
            int LibraryID = IV.ValidateInt("LibraryID", sLibraryID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeObjectAndEntityLink("Document", "Library", DocumentID, LibraryID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">";
            }
            return result;
        }

        public virtual string EntityLink(String sEntityID, String sSEName, String sEntityName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("EntityLink");
            String SEName = IV.ValidateString("SEName", sSEName);
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            int EntityID = IV.ValidateInt("EntityID", sEntityID);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeEntityLink(EntityName, EntityID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">";
            }
            return result;
        }

        public virtual string ObjectLink(String sObjectID, String sSEName, String sObjectName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("ObjectLink");
            int ObjectID = IV.ValidateInt("ObjectID", sObjectID);
            String ObjectName = IV.ValidateString("ObjectName", sObjectName);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeObjectLink(ObjectName, ObjectID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">";
            }
            return result;
        }

        public virtual string ProductandEntityLink(String sProductID, String sSEName, String sEntityID, String sEntityName, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("ProductandEntityLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int EntityID = sEntityID == string.Empty ? 0 : IV.ValidateInt("EntityID", sEntityID);
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            result = SE.MakeProductAndEntityLink(EntityName, ProductID, EntityID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">";
            }
            return result;
        }

        // end obsolete functions

        /// <summary>
        /// Creates a link to the Distributor page.
        /// </summary>
        /// <param name="DistributorID">ID of the Distributor</param>
        /// <param name="SEName">The Search Engine name of the distributor</param>
        /// <param name="IncludeATag">Flag to create an achor tag</param>
        /// <param name="TagInnerText">The innertext of the anchor tag</param>
        /// <returns>Returns an SE encoded page name</returns>
        public string DistributorLink(string sDistributorID, String sSEName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("DistributorLink");
            int DistributorID = IV.ValidateInt("DistributorID", sDistributorID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeEntityLink("Distributor", DistributorID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\" />" + TagInnerText + "</a>";
            }
            return result;
        }

        /// <summary>
        /// Creates a link to the Genre page.
        /// </summary>
        /// <param name="GenreID">ID of the Genre</param>
        /// <param name="SEName">The Search Engine name of the Genre</param>
        /// <param name="IncludeATag">Flag to create an achor tag</param>
        /// <param name="TagInnerText">The innertext of the anchor tag</param>
        /// <returns>Returns an SE encoded page name</returns>
        public string GenreLink(string sGenreID, String sSEName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("GenreLink");
            int GenreID = IV.ValidateInt("GenreID", sGenreID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeEntityLink("Genre", GenreID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\" />" + TagInnerText + "</a>";
            }
            return result;
        }

        /// <summary>
        /// Creates a link to the Vector page.
        /// </summary>
        /// <param name="VectorID">ID of the Vector</param>
        /// <param name="SEName">The Search Engine name of the Show</param>
        /// <param name="IncludeATag">Flag to create an achor tag</param>
        /// <param name="TagInnerText">The innertext of the anchor tag</param>
        /// <returns>Returns an SE encoded page name</returns>
        public string VectorLink(string sVectorID, String sSEName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("ShowLink");
            int VectorID = IV.ValidateInt("VectorID", sVectorID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeEntityLink("Vector", VectorID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string ManufacturerLink(String sManufacturerID, String sSEName, String sIncludeATag, String sTagInnerText)
        {
            return ManufacturerLink(sManufacturerID, sSEName, sIncludeATag, sTagInnerText, "0");
        }

        public virtual string ManufacturerLink(String sManufacturerID, String sSEName, String sIncludeATag, String sTagInnerText, String sFullURL)
        {
            InputValidator IV = new InputValidator("ManufacturerLink");
            int ManufacturerID = IV.ValidateInt("ManufacturerID", sManufacturerID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeManufacturerLink(ManufacturerID, SEName);
            bool FullURL = IV.ValidateBool("FullURL", sFullURL);

            if (FullURL)
            {
                result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, result);
            }

            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string CategoryLink(string sCategoryID, String sSEName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("CategoryLink");
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeCategoryLink(CategoryID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string SectionLink(string sSectionID, String sSEName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("SectionLink");
            int SectionID = IV.ValidateInt("SectionID", sSectionID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeSectionLink(SectionID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string LibraryLink(string sLibraryID, String sSEName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("LibraryLink");
            int LibraryID = IV.ValidateInt("LibraryID", sLibraryID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeEntityLink("Library", LibraryID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string ProductLink(String sProductID, String sSEName, String sIncludeATag, String sTagInnerText)
        {
            return ProductLink(sProductID, sSEName, sIncludeATag, sTagInnerText, "0");
        }

        public virtual string ProductLink(String sProductID, String sSEName, String sIncludeATag, String sTagInnerText, String sJustProductPage)
        {
            return ProductLink(sProductID, sSEName, sIncludeATag, sTagInnerText, sJustProductPage, "0");
        }

        public virtual string ProductLink(String sProductID, String sSEName, String sIncludeATag, String sTagInnerText, String sJustProductPage, String sFullURL)
        {
            InputValidator IV = new InputValidator("ProductLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            bool JustProductPage = IV.ValidateBool("JustProductPage", sJustProductPage);
            bool FullURL = IV.ValidateBool("FullURL", sFullURL);

            string result = String.Empty;
            result = SE.MakeProductLink(ProductID, SEName);

            if (FullURL)
            {
                result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, result);
            }
            else if (JustProductPage)
            {
                result = result.TrimStart('/');
                result = result.Substring(result.IndexOf('/') + 1);
            }

            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        [Obsolete("deprecated (7.0) in favor of the ShowUpsellProducts method")]
        public virtual string UpsellProducts(string sProductID)
        {
            return "<b>the UpsellProducts extension function has been deprecated please use the ShowUpsellProducts extension function </b>";
        }

        /// <summary>
        /// overload method that calls xmlpackage that displays related products. sets parameter value of enclosedTab to true
        /// </summary>
        /// <param name="sProductID">the product id to look for related products</param>
        /// <returns>returns string html to be rendered</returns>
        public virtual string RelatedProducts(string sProductID)
        {
            return RelatedProducts(sProductID, true.ToString());
        }

        /// <summary>
        /// calls xml package that displays related products
        /// </summary>
        /// <param name="sProductID">the product id to look for related products</param>
        /// <param name="sEncloseInTab">set to true if not to be displayed in a tabUI</param>
        /// <returns>returns string html to be rendered</returns>
        public virtual string RelatedProducts(string sProductID, string sEncloseInTab)
        {
            InputValidator IV = new InputValidator("RelatedProducts");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            bool encloseInTab = IV.ValidateBool("EncloseInTab", sEncloseInTab);
            string CustomerViewID = String.Empty;

            bool renderXMLPackage = (ThisCustomer.IsRegistered || HttpContext.Current.Items["OriginalSessionID"] != null);
            if (renderXMLPackage)
            {
                if (ThisCustomer.IsRegistered)
                {
                    CustomerViewID = ThisCustomer.CustomerID.ToString();
                }
                else
                {
                    CustomerViewID = HttpContext.Current.Items["OriginalSessionID"].ToString();
                }
                string runtimeParams = string.Format("ProductID={0}&EncloseInTab={1}&customerGuid={2}", sProductID, encloseInTab.ToString().ToLowerInvariant(), CustomerViewID);
                return AppLogic.RunXmlPackage("relatedproducts.xml.config", null, ThisCustomer, Convert.ToInt32(SkinID()), "", runtimeParams, false, false);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// calls xml package that displays recently viewed products
        /// </summary>
        /// <param name="sProductID">the product id to look for related products</param>
        /// <returns>returns string html to be rendered</returns>
        public virtual string RecentlyViewed(string sProductID)
        {
            InputValidator IV = new InputValidator("RecentlyViewed");
            int ProductID = IV.ValidateInt("ProductID", sProductID);

            return RecentlyViewed(sProductID, true.ToString());
        }

        /// <summary>
        /// calls xml package that displays recently viewed products, one overload
        /// </summary>
        /// <param name="sProductID">the product id to look for related products</param>
        /// <param name="sEncloseInTab">enclose recently viewed in a tab</param>
        /// <returns>returns string html to be rendered</returns>
        public virtual string RecentlyViewed(string sProductID, string sEncloseInTab)
        {
            InputValidator IV = new InputValidator("RecentlyViewed");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            bool encloseInTab = IV.ValidateBool("EncloseInTab", sEncloseInTab);
            string CustomerViewID = String.Empty;
            bool renderXMLPackage = (ThisCustomer.IsRegistered || HttpContext.Current.Items["OriginalSessionID"] != null);

            if (renderXMLPackage)
            {
                if (ThisCustomer.IsRegistered)
                {
                    CustomerViewID = ThisCustomer.CustomerID.ToString();
                }
                else
                {
                    CustomerViewID = HttpContext.Current.Items["OriginalSessionID"].ToString();
                }

                string runtimeParams = string.Format("ProductID={0}&custGuid={1}&EncloseInTab={2}", sProductID, CustomerViewID, encloseInTab.ToString().ToLowerInvariant());
                return AppLogic.RunXmlPackage("recentlyviewed.xml.config", null, ThisCustomer, Convert.ToInt32(SkinID()), "", runtimeParams, false, false);
            }
            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Calls xml package alsobought.xml.config that displays AlsoBought products
        /// </summary>
        /// <param name="sProductID">product id of product being viewed</param>
        /// <param name="sVariantID">variant id of product being viewed</param>
        /// <returns></returns>
        public virtual string AlsoBought(string sProductID, string sVariantID)
        {
            InputValidator IV = new InputValidator("AlsoBought");
            int StoreID = AppLogic.StoreID();
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            if (CommonLogic.IsStringNullOrEmpty(AppLogic.AppConfig("AlsoBoughtNumberToDisplay")) || AppLogic.AppConfig("AlsoBoughtNumberToDisplay") == "0")
            {
                return string.Empty;
            }
            return AppLogic.RunXmlPackage("alsobought.xml.config", null, ThisCustomer, Convert.ToInt32(SkinID()), "", string.Format("ProductID={0}&VariantID={1}&StoreID={2}", sProductID, sVariantID, StoreID), false, false);
        }

        public virtual string DocumentLink(string sDocumentID, String sSEName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("DocumentLink");
            int DocumentID = IV.ValidateInt("DocumentID", sDocumentID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeObjectLink("Document", DocumentID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string ProductandCategoryLink(string sProductID, String sSEName, string sCategoryID, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("ProductandCategoryLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeProductAndCategoryLink(ProductID, CategoryID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string ProductandSectionLink(string sProductID, String sSEName, string sSectionID, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("ProductandSectionLink");
            int SectionID = IV.ValidateInt("SectionID", sSectionID);
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeProductAndSectionLink(ProductID, SectionID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string ProductandManufacturerLink(string sProductID, String sSEName, string sManufacturerID, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("ProductandManufacturerLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int ManufacturerID = IV.ValidateInt("ManufacturerID", sManufacturerID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeProductAndEntityLink("Manufacturer", ProductID, ManufacturerID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string ProductProperName(string sProductID, string sVariantID)
        {
            InputValidator IV = new InputValidator("ProductProperName");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            string result = String.Empty;
            result = AppLogic.MakeProperProductName(ProductID, VariantID, ThisCustomer.LocaleSetting);
            return result;
        }

        public virtual string DocumentandLibraryLink(string sDocumentID, String sSEName, string sLibraryID, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("DocumentandLibraryLink");
            int DocumentID = IV.ValidateInt("DocumentID", sDocumentID);
            int LibraryID = IV.ValidateInt("LibraryID", sLibraryID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeObjectAndEntityLink("Document", "Library", DocumentID, LibraryID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string EntityLink(string sEntityID, String sSEName, String sEntityName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("EntityLink");
            int EntityID = IV.ValidateInt("EntityID", sEntityID);
            String SEName = IV.ValidateString("SEName", sSEName);
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeEntityLink(EntityName, EntityID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string ObjectLink(string sObjectID, String sSEName, String sObjectName, String sIncludeATag, string sTagInnerText)
        {
            InputValidator IV = new InputValidator("ObjectLink");
            String ObjectName = IV.ValidateString("ObjectName", sObjectName);
            int ObjectID = IV.ValidateInt("ObjectID", sObjectID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            string result = String.Empty;
            result = SE.MakeObjectLink(ObjectName, ObjectID, SEName);
            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }
            return result;
        }

        public virtual string ProductandEntityLink(String sProductID, String sSEName, String sEntityID, String sEntityName, String sIncludeATag, String sTagInnerText)
        {
            return ProductandEntityLink(sProductID, sSEName, sEntityID, sEntityName, sIncludeATag, sTagInnerText, "0");
        }

        public virtual string ProductandEntityLink(String sProductID, String sSEName, String sEntityID, String sEntityName, String sIncludeATag, String sTagInnerText, String sJustProductPage)
        {
            return ProductandEntityLink(sProductID, sSEName, sEntityID, sEntityName, sIncludeATag, sTagInnerText, sJustProductPage, "0");
        }

        public virtual string ProductandEntityLink(String sProductID, String sSEName, String sEntityID, String sEntityName, String sIncludeATag, String sTagInnerText, String sJustProductPage, String sFullURL)
        {
            InputValidator IV = new InputValidator("ProductandEntityLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int EntityID = IV.ValidateInt("EntityID", sEntityID);
            String SEName = IV.ValidateString("SEName", sSEName);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String TagInnerText = IV.ValidateString("TagInnerText", sTagInnerText);
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            bool JustProductPage = IV.ValidateBool("JustProductPage", sJustProductPage);
            bool FullURL = IV.ValidateBool("FullURL", sFullURL);

            string result = String.Empty;
            result = SE.MakeProductAndEntityLink(EntityName, ProductID, EntityID, SEName);

            if (FullURL)
            {
                result = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, result);
            }
            else if (JustProductPage)
            {
                result = result.TrimStart('/');
                result = result.Substring(result.IndexOf('/') + 1);
            }

            if (IncludeATag)
            {
                result = "<a href=\"" + result + "\">" + TagInnerText + "</a>";
            }

            return result;
        }

        public virtual string Topic(String sTopicName, string sTopicID)
        {
            return Topic(sTopicName, sTopicID, AppLogic.StoreID());
        }

        public virtual string Topic(String sTopicName, string sTopicID, int StoreId)
        {
            InputValidator IV = new InputValidator("Topic");
            String TopicName = IV.ValidateString("TopicName", sTopicName);
            int TopicID = IV.ValidateInt("TopicID", sTopicID);
            String LCL = ThisCustomer.LocaleSetting;
            string result = String.Empty;
            if (TopicID != 0)
            {
                Topic t = new Topic(TopicID, LCL, ThisCustomer.SkinID, null, StoreId);
                result = t.Contents;
            }

            if (TopicName.Length != 0)
            {
                Topic t = new Topic(TopicName, LCL, ThisCustomer.SkinID, null, StoreId);
                result = t.Contents;
            }
            Parser p = new Parser(ThisCustomer.SkinID, ThisCustomer);
            result = p.ReplaceTokens(result);
            return result;
        }

        public virtual string ReceiptTopic(String sTopicName, int OrderNumber)
        {
            int StoreID = Order.GetOrderStoreID(OrderNumber);
            InputValidator IV = new InputValidator("Topic");
            String TopicName = IV.ValidateString("TopicName", sTopicName);
            String LCL = ThisCustomer.LocaleSetting;
            string result = String.Empty;
            if (TopicName.Length != 0)
            {
                Topic t = new Topic(sTopicName, LCL, ThisCustomer.SkinID, null, StoreID);
                result = t.Contents;
                Parser p = new Parser(ThisCustomer.SkinID, ThisCustomer);
                result = p.ReplaceTokens(result);
            }

            return result;
        }

        public virtual string Topic(String sTopicName)
        {
            return Topic(sTopicName, AppLogic.StoreID());
        }

        public virtual string Topic(String sTopicName, int StoreID)
        {
            InputValidator IV = new InputValidator("Topic");
            String TopicName = IV.ValidateString("TopicName", sTopicName);
            String LCL = ThisCustomer.LocaleSetting;
            string result = String.Empty;
            if (TopicName.Length != 0)
            {
                Topic t = new Topic(TopicName, LCL, ThisCustomer.SkinID, null, StoreID);
                result = t.Contents;
                Parser p = new Parser(ThisCustomer.SkinID, ThisCustomer);
                result = p.ReplaceTokens(result);
            }
            return result;
        }

        public virtual string AppConfig(String sAppConfigName)
        {
            InputValidator IV = new InputValidator("AppConfig");
            String AppConfigName = IV.ValidateString("AppConfigName", sAppConfigName);
            string result = String.Empty;
            if (AppConfigName.Length != 0)
            {
                result = AppLogic.AppConfig(AppConfigName);
            }
            return result;
        }

        public virtual string AppConfig(string storeID, String sAppConfigName)
        {
            InputValidator IV = new InputValidator("AppConfig");
            String AppConfigName = IV.ValidateString("AppConfigName", sAppConfigName);
            string result = String.Empty;
            if (AppConfigName.Length != 0)
            {
                result = AppLogic.AppConfig(AppConfigName, Localization.ParseUSInt(storeID), true);
            }
            return result;
        }

        public virtual string AppConfigBool(String sAppConfigName)
        {
            InputValidator IV = new InputValidator("AppConfigBool");
            String AppConfigName = IV.ValidateString("AppConfigName", sAppConfigName);
            return AppLogic.AppConfigBool(AppConfigName).ToString().ToLowerInvariant();
        }

        public Boolean EvalBool(string sEvalString)
        {
            InputValidator IV = new InputValidator("EvalBool");
            String EvalString = IV.ValidateString("EvalString", sEvalString);

            if (EvalString.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) ||
                EvalString.Equals("YES", StringComparison.InvariantCultureIgnoreCase) ||
                EvalString.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual string StringResource(String sStringResourceName)
        {
            InputValidator IV = new InputValidator("StringResource");
            String StringResourceName = IV.ValidateString("StringResourceName", sStringResourceName);
            // undocumented diagnostic mode:
            if (AppLogic.AppConfigBool("ShowStringResourceKeys"))
            {
                return StringResourceName;
            }
            string result = String.Empty;
            if (StringResourceName.Length != 0)
            {
                result = AppLogic.GetString(StringResourceName, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
            return result;
        }

        public virtual string StringResource(String sStringResourceName, String sLocaleSetting)
        {
            InputValidator IV = new InputValidator("StringResource");
            String StringResourceName = IV.ValidateString("StringResourceName", sStringResourceName);
            String LocaleSetting = IV.ValidateString("LocaleSetting", sLocaleSetting);
            // undocumented diagnostic mode:
            if (AppLogic.AppConfigBool("ShowStringResourceKeys"))
            {
                return StringResourceName;
            }
            string result = String.Empty;
            if (StringResourceName.Length != 0)
            {
                result = AppLogic.GetString(StringResourceName, ThisCustomer.SkinID, LocaleSetting);
            }
            return result;
        }

        //uses a delimited list of params to format a StringResource that has format tags in it
        public virtual string StrFormatStringresource(string sStringResourceName, string sFormatParams, string sDelimiter)
        {
            InputValidator IV = new InputValidator("StrFormatStringresource");
            String StringResourceName = IV.ValidateString("StringResourceName", sStringResourceName);
            String FormatParams = IV.ValidateString("FormatParams", sFormatParams);
            String Delimiter = IV.ValidateString("Delimiter", sDelimiter);
            char[] delim = Delimiter.ToCharArray();
            string[] rParams = FormatParams.Split(delim);
            return String.Format(StringResource(StringResourceName), rParams);
        }

        //deprecated in 7.0.0.7, use StringResource
        public virtual string GetString(String sStringResourceName)
        {
            InputValidator IV = new InputValidator("GetString");
            String StringResourceName = IV.ValidateString("StringResourceName", sStringResourceName);
            return "<b>The GetString extension function is deprecated please use the StringResource extension function</b>";
        }

        //deprecated in 7.0.0.7, use StringResource
        public virtual string GetString(String sStringResourceName, String sLocaleSetting)
        {
            InputValidator IV = new InputValidator("GetString");
            String StringResourceName = IV.ValidateString("StringResourceName", sStringResourceName);
            String LocaleSetting = IV.ValidateString("LocaleSetting", sLocaleSetting);
            return "<b>The GetString extension function is deprecated please use the StringResource extension function</b>";
        }

        public virtual string LoginOutPrompt()
        {
            return AppLogic.GetLoginBox(ThisCustomer.SkinID);
        }

        public virtual string SearchBox()
        {
            return AppLogic.GetSearchBox(ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
        }

        public virtual string HelpBox()
        {
            string result = String.Empty;
            result = AppLogic.GetHelpBox(ThisCustomer.SkinID, true, ThisCustomer.LocaleSetting, null);
            return result;
        }

        public virtual string AddtoCartFormERP(string sProductID, string sVariantID, String sColorChangeProductImage, String sVariantStyleFlag)
        {
            InputValidator IV = new InputValidator("AddtoCartForm");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            bool ColorChangeProductImage = IV.ValidateBool("ColorChangeProductImage", sColorChangeProductImage);
            VariantStyleEnum VariantStyle = (VariantStyleEnum)IV.ValidateInt("VariantStyleFlag", sVariantStyleFlag);
            string result = String.Empty;
            if (VariantID == 0)
            {
                VariantID = AppLogic.GetDefaultProductVariant(ProductID);
            }
            if (ProductID != 0 &&
                VariantID != 0)
            {
                result = ShoppingCart.GetAddToCartForm(ThisCustomer, false, AppLogic.AppConfigBool("ShowWishButtons"), AppLogic.AppConfigBool("ShowGiftRegistryButtons"), ProductID, VariantID, ThisCustomer.SkinID, 0, ThisCustomer.LocaleSetting, ColorChangeProductImage, VariantStyle);
            }
            else
            {
                result = String.Empty;
            }
            return result;
        }

        public virtual string AddtoCartForm(string sProductID, string sVariantID, String sColorChangeProductImage)
        {
            InputValidator IV = new InputValidator("AddtoCartForm");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            bool ColorChangeProductImage = IV.ValidateBool("ColorChangeProductImage", sColorChangeProductImage);
            string result = String.Empty;
            if (VariantID == 0)
            {
                VariantID = AppLogic.GetDefaultProductVariant(ProductID);
            }
            if (ProductID != 0 &&
                VariantID != 0)
            {
                result = ShoppingCart.GetAddToCartForm(ThisCustomer, false, AppLogic.AppConfigBool("ShowWishButtons"), AppLogic.AppConfigBool("ShowGiftRegistryButtons"), ProductID, VariantID, ThisCustomer.SkinID, 0, ThisCustomer.LocaleSetting, ColorChangeProductImage, VariantStyleEnum.RegularVariantsWithAttributes);
            }
            else
            {
                result = String.Empty;
            }
            return result;
        }

        public virtual string AddtoCartForm(string sProductID, string sVariantID, String sColorChangeProductImage, string sDisplayFormat)
        {
            InputValidator IV = new InputValidator("AddtoCartForm");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            bool ColorChangeProductImage = IV.ValidateBool("ColorChangeProductImage", sColorChangeProductImage);
            string result = String.Empty;
            if (VariantID == 0)
            {
                VariantID = AppLogic.GetDefaultProductVariant(ProductID);
            }
            if (ProductID != 0 &&
                VariantID != 0)
            {
                result = ShoppingCart.GetAddToCartForm(ThisCustomer, false, AppLogic.AppConfigBool("ShowWishButtons"), AppLogic.AppConfigBool("ShowGiftRegistryButtons"), ProductID, VariantID, ThisCustomer.SkinID, 0, ThisCustomer.LocaleSetting, ColorChangeProductImage, VariantStyleEnum.RegularVariantsWithAttributes);
            }
            else
            {
                result = String.Empty;
            }
            return result;
        }

        public virtual string AddtoCartForm(string sProductID, string sVariantID, String sColorChangeProductImage, String sShowWishListButton, String sShowGiftRegistryButtons)
        {
            InputValidator IV = new InputValidator("AddtoCartForm");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            bool ShowWishListButton = IV.ValidateBool("ShowWishListButton", sShowWishListButton);
            bool ShowGiftRegistryButtons = IV.ValidateBool("ShowGiftRegistryButtons", sShowGiftRegistryButtons);
            bool ColorChangeProductImage = IV.ValidateBool("ColorChangeProductImage", sColorChangeProductImage);
            string result = String.Empty;
            if (VariantID == 0)
            {
                VariantID = AppLogic.GetDefaultProductVariant(ProductID);
            }
            if (ProductID != 0 &&
                VariantID != 0)
            {
                result = ShoppingCart.GetAddToCartForm(ThisCustomer, false, ShowWishListButton, ShowGiftRegistryButtons, ProductID, VariantID, ThisCustomer.SkinID, 0, ThisCustomer.LocaleSetting, ColorChangeProductImage, VariantStyleEnum.RegularVariantsWithAttributes);
            }
            else
            {
                result = String.Empty;
            }
            return result;
        }

        public virtual bool HasZoomify(string sID, String sEntityOrObjectName)
        {
            InputValidator IV = new InputValidator("HasZoomify");
            int ID = IV.ValidateInt("ID", sID);
            String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);

            return AppLogic.ZoomifyExists(EntityOrObjectName, ID);
        }

        public virtual string LookupZoomify(string sID, String sEntityOrObjectName, String sDesiredSize)
        {
            InputValidator IV = new InputValidator("HasZoomify");
            int ID = IV.ValidateInt("ID", sID);
            String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);

            return AppLogic.ZoomifyMarkup(EntityOrObjectName, ID, DesiredSize, ThisCustomer, ThisCustomer.SkinID);
        }

        //Deprecated methods,  Use the methods that accept image Alt Text
        public virtual string LookupImage(string sID, String sEntityOrObjectName, String sDesiredSize, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("LookupImage");
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            int ID = IV.ValidateInt("ID", sID);
            String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
            string result = String.Empty;

            string sku = string.Empty;
            string IFO = string.Empty;
            string AltText = string.Empty;
            String SwatchImageMap = String.Empty;

            if (EntityOrObjectName.Equals("PRODUCT", StringComparison.InvariantCultureIgnoreCase))
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    string query = "select SKU, ImageFilenameOverride, SEAltText, SwatchImageMap from product where productid = " + sID;
                    using (IDataReader dr = DB.GetRS(query, con))
                    {
                        if (dr.Read())
                        {
                            sku = DB.RSField(dr, "SKU");
                            IFO = DB.RSField(dr, "ImageFilenameOverride");
                            AltText = DB.RSFieldByLocale(dr, "SEAltText", ThisCustomer.LocaleSetting);
                            SwatchImageMap = DB.RSField(dr, "SwatchImageMap");
                        }
                    }
                }

                if (DesiredSize.Equals("MEDIUM", StringComparison.InvariantCultureIgnoreCase))
                {
                    StringBuilder tmpS = new StringBuilder(4096);
                    tmpS.Append("<div class=\"image-wrap medium-image-wrap\">");
                    String ProductPicture = String.Empty;
					ProductPicture = AppLogic.LookupImage("Product", ID, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    String LargePic = AppLogic.LookupImage("Product", ID, "large", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    bool HasLargePic = (LargePic.Length != 0);
                    String LargePicForPopup = LargePic;

                    // setup multi-image gallery:
                    String SwatchPic = AppLogic.LookupImage("Product", ID, "swatch", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);


                    ProductImageGallery ImgGal = null;
                    String ImgGalCacheName = "ImgGal_" + ID.ToString() + "_" + ThisCustomer.SkinID.ToString() + "_" + ThisCustomer.LocaleSetting;
                    if (AppLogic.CachingOn)
                    {
                        ImgGal = (ProductImageGallery)HttpContext.Current.Cache.Get(ImgGalCacheName);
                    }
                    if (ImgGal == null)
                    {
                        ImgGal = new ProductImageGallery(ID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, sku);
                    }
                    if (AppLogic.CachingOn)
                    {
                        HttpContext.Current.Cache.Insert(ImgGalCacheName, ImgGal, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                    }
                    tmpS.Append(ImgGal.ImgDHTML);

                    if (HasLargePic && !ImgGal.HasSomeLarge)
                    {
                        Size size = CommonLogic.GetImagePixelSize(LargePic);
                        tmpS.Append(Scripts.PopupImage(size));
                    }
                    tmpS.Append("<div class=\"medium-image-wrap\" id=\"divProductPic" + ID.ToString() + "\">\n");
                    if (ImgGal.ImgDHTML.Length == 0 || !ImgGal.HasSomeLarge)
                    {
                        if (HasLargePic)
                        {
                            tmpS.Append("<img id=\"ProductPic" + ID.ToString() + "\" name=\"ProductPic" + ID.ToString() + "\" class=\"product-image large-image img-responsive\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge_" + ID.ToString() + "()", "popupimg('" + LargePicForPopup + "')") + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" src=\"" + ProductPicture + "\" />");
                        }
                        else
                        {
                            tmpS.Append("<img id=\"ProductPic" + ID.ToString() + "\" name=\"ProductPic" + ID.ToString() + "\" class=\"product-image large-image img-responsive\" src=\"" + ProductPicture + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />");
                        }
                    }
                    else
                    {
                        if (ImgGal.HasSomeLarge)
                        {
                            tmpS.Append("<img id=\"ProductPic" + ID.ToString() + "\" name=\"ProductPic" + ID.ToString() + "\" class=\"actionelement\" onClick=\"popuplarge_" + ID.ToString() + "()\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" class=\"product-image large-image img-responsive\" src=\"" + ProductPicture + "\" />");
                        }
                        else
                        {
                            tmpS.Append("<img id=\"ProductPic" + ID.ToString() + "\" name=\"ProductPic" + ID.ToString() + "\" class=\"product-image large-image img-responsive\" src=\"" + ProductPicture + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />");
                        }
                    }
                    tmpS.Append("</div>");
                    tmpS.Append("<div class=\"image-controls\">");
                    if (ImgGal.ImgGalIcons.Length != 0)
                    {
                        tmpS.Append("<div class=\"image-icons\">");
                        tmpS.Append(ImgGal.ImgGalIcons);
                        tmpS.Append("</div>");
                    }
                    bool SwatchWritten = false;
                    if (AppLogic.AppConfigBool("UseColorSwatchIcons")) // from /images/product/swatch/color.{extension}. Color must be cleaned up to be valid filename!
                    {
                        tmpS.Append("<div class=\"swatch-image-wrap\">");
                        using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                        {
                            con.Open();
                            string query = "select Colors from ProductVariant  with (NOLOCK)  where ProductID=" + ID.ToString() + " and IsDefault=1 and Deleted=0 and Published=1";
                            using (IDataReader rsx = DB.GetRS(query, con))
                            {
                                if (rsx.Read())
                                {
                                    SwatchWritten = true;
                                    String[] Colors = DB.RSFieldByLocale(rsx, "Colors", ThisCustomer.LocaleSetting).Trim().Split(',');
                                    if (Colors.Length > 1)
                                    {
                                        StringBuilder sx = new StringBuilder(1024);
                                        sx.Append("<div class=\"swatch-image\">");
                                        foreach (String s in Colors)
                                        {
                                            String sCleaned = CommonLogic.MakeSafeFilesystemName(s.Trim());
                                            String fn = "images/product/swatch/" + sCleaned + ".jpg";
                                            if (!CommonLogic.FileExists(fn))
                                            {
                                                fn = "images/product/swatch/" + sCleaned + ".gif";
                                            }
                                            sx.Append("<img alt=\"Show Color " + s.Replace("\"", "") + "\" src=\"" + fn + "\" class=\"actionelement\" onClick=\"setcolorpic_" + sID.ToString() + "(" + CommonLogic.SQuote(s.Trim()) + ")\">");
                                        }
                                        sx.Append("</div>");
                                        tmpS.Append(sx);
                                    }
                                }
                            }
                        }
                        tmpS.Append("</div>");
                    }

                    if (!SwatchWritten && SwatchPic.Length != 0)
                    {
                        tmpS.Append("<div class=\"swatch-image-wrap\">");
                        tmpS.Append(SwatchImageMap);
                        tmpS.Append("<img class=\"product-image swatch-image\" src=\"" + SwatchPic + "\" usemap=\"#SwatchMap\"  alt=\"" + AltText.Replace("\"", "&quot;") + "\"  />");
                        tmpS.Append("</div>");
                    }
                    if (ImgGal.HasSomeLarge)
                    {
                        tmpS.Append("<div class=\"view-larger-wrap\"><a href=\"javascript:void(0);\" onClick=\"popuplarge_" + ID.ToString() + "();\">" + "showproduct.aspx.43".StringResource() + "</a></div>");
                    }
                    else if (HasLargePic)
                    {
                        tmpS.Append("<div class=\"view-larger-wrap\"><a href=\"javascript:void(0);\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge_" + ID.ToString() + "()", "popupimg('" + LargePicForPopup + "')") + ";\">" + "showproduct.aspx.43".StringResource() + "</a></div>");
                    }
                    tmpS.Append("</div>");
                    tmpS.Append("</div>");
                    result = tmpS.ToString();
                }
                else
                {
					result = AppLogic.LookupImage("Product", ID, IFO, sku, DesiredSize.ToLowerInvariant(), ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

                    // we must ALWAYS return an image here back to Xsl (this is a little different than the prior version logic, where large did not have a "no picture" returned!)
                    if (result.Length == 0)
                    {
                        result = AppLogic.NoPictureImageURL(DesiredSize.Equals("icon", StringComparison.InvariantCultureIgnoreCase), ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                    if (result.Length != 0)
                    {
                        result = "<img id=\"ProductPic" + ID.ToString() + "\" name=\"ProductPic" + ID.ToString() + "\" class=\"actionelement\" src=\"" + result + "\"  alt=\"" + AltText.Replace("\"", "&quot;") + "\" />";
                    }
                }
            }

            else if (EntityOrObjectName.Equals("DOCUMENT", StringComparison.InvariantCultureIgnoreCase))
            {
                result = String.Empty; // not supported yet
            }
            else
            {
                // a category, section, or manufacturer, etc...
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    string query = "select ImageFilenameOverride, SEAltText from " + EntityOrObjectName + " where " + EntityOrObjectName + "ID = " + sID;
                    using (IDataReader dr = DB.GetRS(query, con))
                    {
                        if (dr.Read())
                        {
                            IFO = DB.RSField(dr, "ImageFilenameOverride");
                            AltText = DB.RSField(dr, "SEAltText");
                        }
                    }
                }
                result = AppLogic.LookupImage(EntityOrObjectName, ID, IFO, "", DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                result = "<img id=\"EntityPic" + ID.ToString() + "\" class=\"actionelement\" src=\"" + result + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\">";
            }
            return result;
        }

        public virtual string LookupImage(String sEntityOrObjectName, string sID, String sImageFileNameOverride, String sSKU, String sImgSize)
        {
            InputValidator IV = new InputValidator("LookupImage");
            String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
            String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
            String SKU = IV.ValidateString("SKU", sSKU);
            String ImgSize = IV.ValidateString("ImgSize", sImgSize);
            int ID = IV.ValidateInt("ID", sID);
            String EONU = EntityOrObjectName.ToUpperInvariant();
            String CacheName = String.Empty;
            String seName = AppLogic.GetEntitySEName(ID, (AppLogic.EntityType)Enum.Parse(typeof(AppLogic.EntityType), EONU, true), ThisCustomer.LocaleSetting);

            String FN = ImageFileNameOverride;
            if (FN.Length == 0 && EONU == "PRODUCT" &&
                AppLogic.AppConfigBool("UseSKUForProductImageName"))
            {
                FN = SKU;
            }
            if (FN.Length == 0)
            {
                FN = ID.ToString();
            }
            String Image1 = String.Empty;
            String Image1URL = String.Empty;
            if ((FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)))
            {
                Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN;
                Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN;
            }
            else
            {
                Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".jpg";
                Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".jpg";
                if (!CommonLogic.FileExists(Image1))
                {
                    Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".gif";
                    Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".gif";
                }
                if (!CommonLogic.FileExists(Image1))
                {
                    Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".png";
                    Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".png";
                }
            }
            if (!CommonLogic.FileExists(Image1))
            {
                Image1 = String.Empty;
                Image1URL = String.Empty;
            }

            if (Image1URL.Length == 0 &&
                (ImgSize == "icon" || ImgSize == "medium"))
            {
                Image1URL = AppLogic.NoPictureImageURL(ImgSize == "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                if (Image1URL.Length != 0)
                {
                    Image1 = CommonLogic.SafeMapPath(Image1URL);
                }
            }

            return "<img id=\"" + EONU + "Pic" + ID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, EONU.ToString() + ID.ToString()) + "\" src=\"" + Image1URL + "\">";
        }

        public virtual string LookupProductImage(string sProductID, String sImageFileNameOverride, String sSKU, String sDesiredSize, String sIncludeATag)
        {

            InputValidator IV = new InputValidator("LookupProductImage");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
            String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
            String SKU = IV.ValidateString("SKU", sSKU);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            string seName = AppLogic.GetProductSEName(ProductID, ThisCustomer.LocaleSetting);

            if (DesiredSize.Equals("ICON", StringComparison.InvariantCultureIgnoreCase))
            {				
				result = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                result = "<img id=\"ProductPic" + ProductID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + ProductID.ToString()) + "\" class=\"product-image icon-image img-responsive\" src=\"" + result + "\" />";
                if (IncludeATag)
                {
                    result = ProductLink(ProductID.ToString(), "", "TRUE", result);
                }
            }
            else
            {
                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<div class=\"image-wrap product-image-wrap\">");
                String ProductPicture = String.Empty;
				ProductPicture = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                String LargePic = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "large", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                bool HasLargePic = (LargePic.Length != 0);
                String LargePicForPopup = LargePic;

                String ZoomifyPath = AppLogic.ZoomifyDirectory("PRODUCT", ProductID);
                bool ZoomifyLarge = ZoomifyPath.Length != 0 && AppLogic.AppConfigBool("Zoomify.ProductLarge");
                bool ZoomifyMedium = ZoomifyPath.Length != 0 && AppLogic.AppConfigBool("Zoomify.ProductMedium");

                // setup multi-image gallery:
                String SwatchPic = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "swatch", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                String SwatchImageMap = String.Empty;

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    string query = "select SwatchImageMap from Product  with (NOLOCK)  where ProductID=" + ProductID.ToString();
                    using (IDataReader rs = DB.GetRS(query, con))
                    {
                        if (rs.Read())
                        {
                            SwatchImageMap = DB.RSField(rs, "SwatchImageMap");
                        }
                    }
                }

                ProductImageGallery ImgGal = null;
                String ImgGalCacheName = "ImgGal_" + ProductID.ToString() + "_" + ThisCustomer.SkinID.ToString() + "_" + ThisCustomer.LocaleSetting;
                if (AppLogic.CachingOn)
                {
                    ImgGal = (ProductImageGallery)HttpContext.Current.Cache.Get(ImgGalCacheName);
                }
                if (ImgGal == null)
                {
                    ImgGal = new ProductImageGallery(ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, SKU);
                }
                if (AppLogic.CachingOn)
                {
                    HttpContext.Current.Cache.Insert(ImgGalCacheName, ImgGal, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                }
                tmpS.Append(ImgGal.ImgDHTML);

                if (ZoomifyLarge)
                {
                    tmpS.Append(Scripts.Zoomify());
                }

                if (HasLargePic)
                {
                    Size size = CommonLogic.GetImagePixelSize(LargePic);
                    tmpS.Append(Scripts.PopupImage(size));
                }

                if (ZoomifyMedium)
                {
                    // display flash
                    tmpS.Append("<div class=\"zoomify-wrap\" id=\"divProductPicZ" + ProductID.ToString() + "\">\n");
                    tmpS.Append(AppLogic.ZoomifyMarkup("PRODUCT", ProductID, "medium", ThisCustomer, ThisCustomer.SkinID));
                    tmpS.Append("</div>\n");
                    tmpS.Append("<div class=\"medium-image-wrap\" id=\"divProductPic" + ProductID.ToString() + "\" style=\"display:none\">\n");
                }
                else
                {
                    tmpS.Append("<div id=\"divProductPicZ" + ProductID.ToString() + "\" style=\"display:none\">\n");
                    tmpS.Append("</div>\n");
                    tmpS.Append("<div class=\"medium-image-wrap\" id=\"divProductPic" + ProductID.ToString() + "\">\n");
                }

                if (ZoomifyLarge)
                {
                    // img with popupzoom javascript call
                    tmpS.Append("<img id=\"ProductPic" + ProductID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + ProductID.ToString()) + "\" class=\"product-image zoomify-image img-responsive\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge" + "_" + sProductID + "()", "popupzoom('" + ZoomifyPath + "','" + LargePicForPopup + "')") + "\" alt=\"" + AppLogic.GetString("showproduct.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + ProductPicture + "\">");
                }
                else if (HasLargePic)
                {
                    tmpS.Append("<img id=\"ProductPic" + ProductID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + ProductID.ToString()) + "\" class=\"product-image medium-image img-responsive\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge" + "_" + sProductID + "()", "popupimg('" + LargePicForPopup + "')") + "\" alt=\"" + AppLogic.GetString("showproduct.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + ProductPicture + "\" />");
                }
                else
                {
                    tmpS.Append("<img id=\"ProductPic" + ProductID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + ProductID.ToString()) + "\" class=\"product-image medium-image img-responsive\" src=\"" + ProductPicture + "\" />");
                }
                tmpS.Append("</div>");
                tmpS.Append("<div class=\"image-controls\">");
                if (ImgGal.ImgGalIcons.Length != 0)
                {
                    tmpS.Append("<div class=\"image-icons\">");
                    tmpS.Append(ImgGal.ImgGalIcons);
                    tmpS.Append("</div>");
                }
                if (SwatchPic.Length != 0)
                {
                    tmpS.Append("<div class=\"swatch-image-wrap\">");
                    tmpS.Append(SwatchImageMap);
                    tmpS.Append("<img class=\"product-image swatch-image\" src=\"" + SwatchPic + "\" usemap=\"#SwatchMap\" />");
                    tmpS.Append("</div>");
                }
                if (ZoomifyLarge)
                {
                    tmpS.Append("<div class=\"view-larger-wrap\"><a href=\"javascript:void(0);\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge" + "_" + sProductID + "()", "popupzoom('" + ZoomifyPath + "','" + LargePicForPopup + "')") + ";\">" + "showproduct.aspx.43".StringResource() + "</a></div>");
                }
                else if (HasLargePic)
                {
                    tmpS.Append("<div class=\"view-larger-wrap\"><a href=\"javascript:void(0);\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge" + "_" + sProductID + "()", "popupimg('" + LargePicForPopup + "')") + ";\">" + "showproduct.aspx.43".StringResource() + "</a></div>");
                }
                tmpS.Append("</div>");
                tmpS.Append("</div>");
                result = tmpS.ToString();
            }
            return result;
        }

        public virtual string LookupEntityImage(String sID, String sEntityName, String sDesiredSize, String sIncludeATag)
        {
            InputValidator IV = new InputValidator("LookupEntityImage");
            int ID = IV.ValidateInt("ID", sID);
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            string result = String.Empty;
            string seName = AppLogic.GetEntitySEName(ID, EntityName, ThisCustomer.LocaleSetting);

            if (EntityName.Equals("DOCUMENT", StringComparison.InvariantCultureIgnoreCase))
            {
                result = String.Empty; // not supported yet
            }
            else
            {
                // a category, section, or manufacturer, etc...
                result = AppLogic.LookupImage(EntityName, ID, DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                result = "<img id=\"EntityPic" + ID.ToString() + "\"  name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "EntityPic" + ID.ToString()) + "\" class=\"actionelement\" src=\"" + result + "\">";
            }
            return result;
        }
        //end deprecated image lookup methods

        public virtual string LookupImage(String sEntityOrObjectName, string sID, String sImageFileNameOverride, String sSKU, String sImgSize, string sAltText)
        {
            InputValidator IV = new InputValidator("LookupImage");
            String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
            String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
            String SKU = IV.ValidateString("SKU", sSKU);
            String ImgSize = IV.ValidateString("ImgSize", sImgSize);
            int ID = IV.ValidateInt("ID", sID);
            String EONU = EntityOrObjectName.ToUpperInvariant();
            String AltText = IV.ValidateString("AltText", sAltText);
            String CacheName = String.Empty;
            String seName = AppLogic.GetEntitySEName(ID, EONU, ThisCustomer.LocaleSetting);

            String FN = ImageFileNameOverride;
            if (FN.Length == 0 && EONU == "PRODUCT" &&
                AppLogic.AppConfigBool("UseSKUForProductImageName"))
            {
                FN = SKU;
            }
            if (FN.Length == 0)
            {
                FN = ID.ToString();
            }
            String Image1 = String.Empty;
            String Image1URL = String.Empty;
            if ((FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)))
            {
                Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN;
                Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN;
            }
            else
            {
                Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".jpg";
                Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".jpg";
                if (!CommonLogic.FileExists(Image1))
                {
                    Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".gif";
                    Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".gif";
                }
                if (!CommonLogic.FileExists(Image1))
                {
                    Image1 = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, true) + FN + ".png";
                    Image1URL = AppLogic.GetImagePath(EntityOrObjectName, ImgSize, false) + FN + ".png";
                }
            }
            if (!CommonLogic.FileExists(Image1))
            {
                Image1 = String.Empty;
                Image1URL = String.Empty;
            }

            if (Image1URL.Length == 0 &&
                (ImgSize == "icon" || ImgSize == "medium"))
            {
                Image1URL = AppLogic.NoPictureImageURL(ImgSize == "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                if (Image1URL.Length != 0)
                {
                    Image1 = CommonLogic.SafeMapPath(Image1URL);
                }
            }

            return "<img id=\"" + EONU + "Pic" + ID.ToString() + "\" name=\"" + EONU + "Pic" + ID.ToString() + "\" src=\"" + Image1URL + "\" alt=" + AltText.Replace("\"", "&quot;") + "\" />";
        }

        public virtual string LookupProductImage(string sProductID, String sImageFileNameOverride, String sSKU, String sDesiredSize, String sIncludeATag, string sAltText)
        {
            InputValidator IV = new InputValidator("LookupProductImage");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
            String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
            String SKU = IV.ValidateString("SKU", sSKU);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String AltText = IV.ValidateString("AltText", sAltText);
            string result = String.Empty;
            string seName = AppLogic.GetProductSEName(ProductID, ThisCustomer.LocaleSetting);


            if (DesiredSize.Equals("ICON", StringComparison.InvariantCultureIgnoreCase))
            {			
				result = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                result = "<img id=\"ProductPic" + ProductID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + ProductID.ToString()) + "\" class=\"product-image icon-image img-responsive\" src=\"" + result + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />";
                if (IncludeATag)
                {
                    result = ProductLink(ProductID.ToString(), "", "TRUE", result);
                }
            }
            else
            {
                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<div class=\"image-wrap product-image-wrap\">");
                String ProductPicture = String.Empty;
				ProductPicture = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                String LargePic = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "large", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                bool HasLargePic = (LargePic.Length != 0);
                String LargePicForPopup = LargePic;

                String ZoomifyPath = AppLogic.ZoomifyDirectory("PRODUCT", ProductID);
                bool ZoomifyLarge = ZoomifyPath.Length != 0 && AppLogic.AppConfigBool("Zoomify.ProductLarge");
                bool ZoomifyMedium = ZoomifyPath.Length != 0 && AppLogic.AppConfigBool("Zoomify.ProductMedium");

                // setup multi-image gallery:
                String SwatchPic = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "swatch", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                String SwatchImageMap = String.Empty;

                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();
                    string query = "select SwatchImageMap from Product  with (NOLOCK)  where ProductID=" + ProductID.ToString();
                    using (IDataReader rs = DB.GetRS(query, con))
                    {
                        if (rs.Read())
                        {
                            SwatchImageMap = DB.RSField(rs, "SwatchImageMap");
                        }
                    }
                }

                ProductImageGallery ImgGal = null;
                String ImgGalCacheName = "ImgGal_" + ProductID.ToString() + "_" + ThisCustomer.SkinID.ToString() + "_" + ThisCustomer.LocaleSetting;
                if (AppLogic.CachingOn)
                {
                    ImgGal = (ProductImageGallery)HttpContext.Current.Cache.Get(ImgGalCacheName);
                }
                if (ImgGal == null)
                {
                    ImgGal = new ProductImageGallery(ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, SKU);
                }
                if (AppLogic.CachingOn)
                {
                    HttpContext.Current.Cache.Insert(ImgGalCacheName, ImgGal, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
                }
                tmpS.Append(ImgGal.ImgDHTML);

                if (ZoomifyLarge)
                {
                    tmpS.Append(Scripts.Zoomify());
                }

                if (HasLargePic)
                {
                    tmpS.Append(
                        Scripts.PopupImage(CommonLogic.GetImagePixelSize(LargePic))
                        );
                }

                if (ZoomifyMedium)
                {
                    // display flash
                    tmpS.Append("<div class=\"zoomify-wrap\" id=\"divProductPicZ" + ProductID.ToString() + "\">\n");
                    tmpS.Append(AppLogic.ZoomifyMarkup("PRODUCT", ProductID, "medium", ThisCustomer, ThisCustomer.SkinID));
                    tmpS.Append("</div>\n");
                    tmpS.Append("<div class=\"medium-image-wrap\" id=\"divProductPic" + ProductID.ToString() + "\" style=\"display:none\">\n");
                }
                else
                {
                    tmpS.Append("<div id=\"divProductPicZ" + ProductID.ToString() + "\" style=\"display:none\">\n");
                    tmpS.Append("</div>\n");
                    tmpS.Append("<div class=\"medium-image-wrap\" id=\"divProductPic" + ProductID.ToString() + "\">\n");
                }

                if (ZoomifyLarge)
                {
                    // img with popupzoom javascript call
                    tmpS.Append("<img id=\"ProductPic" + ProductID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + ProductID.ToString()) + "\" class=\"product-image zoomify-image img-responsive\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge" + "_" + sProductID + "()", "popupzoom('" + ZoomifyPath + "','" + LargePicForPopup + "')") + "\" title=\"" + AppLogic.GetString("showproduct.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + ProductPicture + "\"  alt=\"" + AltText.Replace("\"", "&quot;") + "\"  />");
                    tmpS.AppendFormat("<input type='hidden' id='popupImageURL' value='{0}' />", ProductPicture);
                }
                else if (HasLargePic)
                {
                    tmpS.Append("<img id=\"ProductPic" + ProductID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + ProductID.ToString()) + "\" class=\"product-image medium-image img-responsive\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge" + "_" + sProductID + "()", "popupimg('" + LargePicForPopup + "')") + "\" title=\"" + AppLogic.GetString("showproduct.aspx.19", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + ProductPicture + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />");
                    tmpS.AppendFormat("<input type='hidden' id='popupImageURL' value='{0}' />", LargePicForPopup);
                }
                else
                {
                    tmpS.Append("<img class=\"product-image medium-image img-responsive\" id=\"ProductPic" + ProductID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + ProductID.ToString()) + "\" src=\"" + ProductPicture + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />");
                    tmpS.AppendFormat("<input type='hidden' id='popupImageURL' value='{0}' />", ProductPicture);
                }
                tmpS.Append("</div>\n");
                tmpS.Append("<div class=\"image-controls\">");
                if (ImgGal.ImgGalIcons.Length != 0)
                {
                    tmpS.Append("<div class=\"image-icons\">");
                    tmpS.Append(ImgGal.ImgGalIcons);
                    tmpS.Append("</div>");
                }
                if (SwatchPic.Length != 0)
                {
                    tmpS.Append("<div class=\"swatch-image-wrap\">");
                    tmpS.Append(SwatchImageMap);
                    tmpS.Append("<img class=\"product-image swatch-image\" src=\"" + SwatchPic + "\" usemap=\"#SwatchMap\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />");
                    tmpS.AppendFormat("<input type='hidden' id='popupImageURL' value='{0}' />", SwatchPic);
                    tmpS.Append("</div>");
                }

                if (ZoomifyLarge)
                {
                    tmpS.Append("<div class=\"view-larger-wrap\"><a href=\"javascript:void(0);\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge" + "_" + sProductID + "()", "popupzoom('" + ZoomifyPath + "','" + LargePicForPopup + "')") + ";\">" + "showproduct.aspx.43".StringResource() + "</a></div>");
                }
                else if (HasLargePic)
                {
                    tmpS.Append("<div class=\"pop-large-wrap\"><a href=\"javascript:void(0);\" class=\"pop-large-link\" onClick=\"" + CommonLogic.IIF(ImgGal.HasSomeLarge, "popuplarge" + "_" + sProductID + "()", "popupimg('" + LargePicForPopup + "')") + ";\">" + "showproduct.aspx.43".StringResource() + "</a></div>");
                    tmpS.AppendFormat("<input type='hidden' id='popupImageURL' value='{0}' />", LargePicForPopup);
                }
                tmpS.Append("</div>");
                tmpS.Append("</div>");
                result = tmpS.ToString();
            }
            return result;
        }

        [Obsolete("Depricated. Please include another parameter, AltText.")]
        public virtual string LookupVariantImage(string sProductID, string sVariantID, String sImageFileNameOverride, String sSKU, String sDesiredSize, String sIncludeATag)
        {
            return LookupVariantImage(sProductID, sVariantID, sImageFileNameOverride, sSKU, sDesiredSize, sIncludeATag, "");
        }

        public virtual string LookupVariantImage(string sProductID, string sVariantID, String sImageFileNameOverride, String sSKU, String sDesiredSize, String sIncludeATag, string sAltText)
        {
            InputValidator IV = new InputValidator("LookupVariantImage");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
            String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
            String SKU = IV.ValidateString("SKU", sSKU);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String AltText = IV.ValidateString("AltText", sAltText);
            string result = String.Empty;
            string seName = AppLogic.GetVariantSEName(VariantID, ThisCustomer.LocaleSetting);

            if (DesiredSize.Equals("ICON", StringComparison.InvariantCultureIgnoreCase))
            {
				result = AppLogic.LookupImage("VARIANT", VariantID, ImageFileNameOverride, SKU, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                result = "<img id=\"ProductPic" + VariantID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + VariantID.ToString()) + "\" class=\"actionelement\" src=\"" + result + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />";
            }
            else
            {
                StringBuilder tmpS = new StringBuilder(4096);
                tmpS.Append("<div align=\"center\">");
                String ProductPicture = String.Empty;
				ProductPicture = AppLogic.LookupImage("VARIANT", VariantID, ImageFileNameOverride, SKU, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                String LargePic = AppLogic.LookupImage("VARIANT", VariantID, ImageFileNameOverride, SKU, "large", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                bool HasLargePic = (LargePic.Length != 0);
                String LargePicForPopup = LargePic;

                String scriptPopup = "";
                if (HasLargePic)
                {
                    Size s = CommonLogic.GetImagePixelSize(LargePic);
                    scriptPopup = @"window.open('{0}?src=" + LargePicForPopup + @"','LargerImage{1}','toolbar=no,location=no,directories=no,status=no, menubar=no,scrollbars={2}, resizable={2},copyhistory=no,width={3},height={4},left=0,top=0');";
                    scriptPopup = String.Format(scriptPopup,
                        AppLogic.ResolveUrl("~/popup.aspx"),
                        CommonLogic.GetRandomNumber(1, 100000),
                        AppLogic.AppConfigBool("ResizableLargeImagePopup") ? "yes" : "no",
                        s.Height,
                        s.Width,
                        "");
                }

                if (HasLargePic)
                {
                    tmpS.Append("<img id=\"ProductPic" + VariantID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + VariantID.ToString()) + "\" class=\"actionelement\" onClick=\"" + scriptPopup + "\" title=\"" + "showproduct.aspx.19".StringResource() + "\" src=\"" + ProductPicture + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />");
                }
                else
                {
                    tmpS.Append("<img id=\"ProductPic" + VariantID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "ProductPic" + VariantID.ToString()) + "\" src=\"" + ProductPicture + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />");
                }
                if (HasLargePic)
                {
                    tmpS.Append("<img src=\"" + AppLogic.LocateImageURL("images/spacer.gif") + "\" width=\"1\" height=\"4\" />");
                    tmpS.Append("<div class=\"pop-large-wrap\"><a href=\"javascript:void(0);\" class=\"pop-large-link\" onClick=\"" + scriptPopup + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/showlarger.gif") + "\" title=\"" + "showproduct.aspx.19".StringResource() + "\"></a></div>");
                }
                tmpS.Append("</div>");
                result = tmpS.ToString();
            }
            return result;
        }

        public virtual string LookupEntityImage(String sID, String sEntityName, String sDesiredSize, String sIncludeATag, string sAltText)
        {
            InputValidator IV = new InputValidator("LookupEntityImage");
            int ID = IV.ValidateInt("ID", sID);
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
            bool IncludeATag = IV.ValidateBool("IncludeATag", sIncludeATag);
            String AltText = IV.ValidateString("AltText", sAltText);
            string result = String.Empty;
            string seName = AppLogic.GetEntitySEName(ID, EntityName, ThisCustomer.LocaleSetting);

            if (EntityName.Equals("DOCUMENT", StringComparison.InvariantCultureIgnoreCase))
            {
                result = String.Empty; // not supported yet
            }
            else
            {
                // a category, section, or manufacturer, etc...
                result = AppLogic.LookupImage(EntityName, ID, DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                result = "<img id=\"EntityPic" + ID.ToString() + "\" name=\"" + CommonLogic.IIF(AppLogic.AppConfigBool("NameImagesBySEName") && !String.IsNullOrEmpty(seName), seName, "EntityPic" + ID.ToString()) + "\" class=\"actionelement\" src=\"" + result + "\" alt=\"" + AltText.Replace("\"", "&quot;") + "\" />";
            }
            return result;
        }

        public virtual string MiniCartProductImage(string intProductID, String sImageFileNameOverride, String sSKU)
        {
            InputValidator IV = new InputValidator("MiniCartProductImage");
            int ProductID = IV.ValidateInt("ProductID", intProductID);
            String ProdPic = String.Empty;

            string results = "";
			ProdPic = ProductImageUrl(intProductID, sImageFileNameOverride, sSKU, "icon", "false");

            int MaxWidth = AppLogic.AppConfigNativeInt("MiniCartMaxIconWidth");
            if (MaxWidth == 0)
            {
                MaxWidth = 125;
            }
            int MaxHeight = AppLogic.AppConfigNativeInt("MiniCartMaxIconHeight");
            if (MaxHeight == 0)
            {
                MaxHeight = 125;
            }
            if (ProdPic.Length != 0)
            {
                Size size = CommonLogic.GetImagePixelSize(ProdPic);
                if (size.Width > MaxWidth)
                {
                    results = "<img src=\"" + ProdPic + "\" width=\"" + MaxWidth.ToString() + "\" class=\"mini-cart-image\"/>";
                }
                else if (size.Height > MaxHeight)
                {
                    results = "<img src=\"" + ProdPic + "\" height=\"" + MaxHeight + "\" class=\"mini-cart-image\">";
                }
                else
                {
                    results = "<img src=\"" + ProdPic + "\" class=\"mini-cart-image\"/>";
                }
            }
            return results;
        }

        public virtual String GetStoreHTTPLocation(String sTryToUseSSL)
        {
            InputValidator IV = new InputValidator("GetStoreHTTPLocation");
            bool TryToUseSSL = IV.ValidateBool("TryToUseSSL", sTryToUseSSL);
            return AppLogic.GetStoreHTTPLocation(TryToUseSSL);
        }

        public virtual string ImageUrl(String sID, String sEntityOrObjectName, String sDesiredSize, String sFullUrl)
        {
            InputValidator IV = new InputValidator("ImageUrl");
            int ID = IV.ValidateInt("ID", sID);
            String EntityOrObjectName = IV.ValidateString("EntityOrObjectName", sEntityOrObjectName);
            bool FullUrl = IV.ValidateBool("FullUrl", sFullUrl);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
            string result = String.Empty;
            string ImgPath = String.Empty;
            string sURL = CommonLogic.IIF(FullUrl, AppLogic.GetStoreHTTPLocation(false), "");
            sURL = sURL.Replace(AppLogic.AdminDir() + "/", "");
            if (sURL.EndsWith("/"))
                sURL = sURL.Substring(0, sURL.Length - 1);
            ImgPath = AppLogic.LookupImage(EntityOrObjectName, ID, DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting).Replace("..", "");
            result = sURL + CommonLogic.IIF(ImgPath.StartsWith("/"), "", "/") + ImgPath;
            return result;
        }

        public virtual string ProductImageUrl(String sProductID, String sImageFileNameOverride, String sSKU, String sDesiredSize, String sFullUrl)
        {
            InputValidator IV = new InputValidator("ProductImageUrl");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            String ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
            bool FullUrl = IV.ValidateBool("FullUrl", sFullUrl);
            String DesiredSize = IV.ValidateString("DesiredSize", sDesiredSize);
            String SKU = IV.ValidateString("SKU", sSKU);
            string result = String.Empty;
            string ImgPath = String.Empty;
            string sURL = CommonLogic.IIF(FullUrl, AppLogic.GetStoreHTTPLocation(false), "");
            sURL = sURL.Replace(AppLogic.AdminDir() + "/", "");
            
			if (!sURL.EndsWith("/"))
			{
				sURL += "/";
			}
			ImgPath = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, DesiredSize, ThisCustomer.SkinID, ThisCustomer.LocaleSetting).Replace("..", "");

            if (FullUrl && ImgPath.StartsWithIgnoreCase(HttpContext.Current.Request.ApplicationPath))
			{
				ImgPath = ImgPath.Substring(HttpContext.Current.Request.ApplicationPath.Length);
			}                

            ImgPath = ImgPath.TrimStart('/');
            result = sURL + ImgPath;
            return result;
        }

        public bool Owns(String sProductID)
        {
            InputValidator IV = new InputValidator("Owns");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            return AppLogic.Owns(ProductID, ThisCustomer.CustomerID);
        }

        public bool Owns(String sProductID, String sCustomerID)
        {
            InputValidator IV = new InputValidator("Owns");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int CustomerID = IV.ValidateInt("CustomerID", sCustomerID);
            return AppLogic.Owns(ProductID, CustomerID);
        }

        public virtual string ProductNavLinks(String sProductID, String sCategoryID, String sSectionID)
        {
            return ProductNavLinks(sProductID, sCategoryID, sSectionID, "FALSE");
        }

        // if UseGraphics then /images/next.gif, /images/prev.gif, and /images/up.gif are used
        public virtual string ProductNavLinks(String sProductID, String sCategoryID, String sSectionID, String sUseGraphics)
        {
            return ProductNavLinks(sProductID, sCategoryID, sSectionID, sUseGraphics, "TRUE");
        }

        // adds a parameter to preven the "up" link from being displayed
        public virtual string ProductNavLinks(String sProductID, String sCategoryID, String sSectionID, String sUseGraphics, String sIncludeUpLink)
        {
            InputValidator IV = new InputValidator("ProductNavLinks");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            int SectionID = IV.ValidateInt("SectionID", sSectionID);
            bool UseGraphics = IV.ValidateBool("UseGraphics", sUseGraphics);
            bool IncludeUpLink = IV.ValidateBool("IncludeUpLink", sIncludeUpLink);
            string result = String.Empty;
            StringBuilder tmpS = new StringBuilder("");
            if (CategoryID == 0 &&
                SectionID == 0)
            {
                return string.Empty;
            }
            if (!AppLogic.AppConfigBool("HideProductNextPrevLinks"))
            {
                tmpS.Append("<div class=\"nav-links-wrap\">");
                int NumProducts = 0;
                if (CategoryID != 0)
                {
                    NumProducts = AppLogic.LookupHelper("Category", 0).GetNumEntityObjects(CategoryID, true, true);
                }
                else
                {
                    NumProducts = AppLogic.LookupHelper("Section", 0).GetNumEntityObjects(SectionID, true, true);
                }
                if (NumProducts > 1)
                {
                    int PreviousProductID = AppLogic.GetPreviousProduct(ProductID, CategoryID, SectionID, 0, 0, false, true, true);
                    if (CategoryID != 0)
                    {
                        if (UseGraphics)
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeProductAndCategoryLink(PreviousProductID, CategoryID, "") + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/prev.gif", ThisCustomer.LocaleSetting) + "\" border=\"0\"></a>");
                        }
                        else
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeProductAndCategoryLink(PreviousProductID, CategoryID, "") + "\">" + AppLogic.GetString("showproduct.aspx.5", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                        }
                    }
                    else
                    {
                        if (UseGraphics)
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeProductAndSectionLink(PreviousProductID, SectionID, "") + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/prev.gif", ThisCustomer.LocaleSetting) + "\" border=\"0\"></a>");
                        }
                        else
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeProductAndSectionLink(PreviousProductID, SectionID, "") + "\">" + AppLogic.GetString("showproduct.aspx.5", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                        }
                    }
                }
                if (IncludeUpLink)
                {
                    if (CategoryID != 0)
                    {
                        if (UseGraphics)
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeCategoryLink(CategoryID, "") + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/up.gif", ThisCustomer.LocaleSetting) + "\" alt=\"" + AppLogic.GetString("image.altText.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                        }
                        else
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeCategoryLink(CategoryID, "") + "\">" + AppLogic.GetString("showproduct.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                        }
                    }
                    else
                    {
                        if (UseGraphics)
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeSectionLink(SectionID, "") + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/up.gif", ThisCustomer.LocaleSetting) + "\" alt=\"" + AppLogic.GetString("image.altText.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                        }
                        else
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeSectionLink(SectionID, "") + "\">" + AppLogic.GetString("showproduct.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                        }
                    }
                }
                if (NumProducts > 1)
                {
                    int NextProductID = AppLogic.GetNextProduct(ProductID, CategoryID, SectionID, 0, 0, false, true, true);
                    if (CategoryID != 0)
                    {
                        if (UseGraphics)
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeProductAndCategoryLink(NextProductID, CategoryID, "") + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/next.gif", ThisCustomer.LocaleSetting) + "\" alt=\"" + AppLogic.GetString("image.altText.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                        }
                        else
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeProductAndCategoryLink(NextProductID, CategoryID, "") + "\">" + AppLogic.GetString("showproduct.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                        }
                    }
                    else
                    {
                        if (UseGraphics)
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeProductAndSectionLink(NextProductID, SectionID, "") + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/next.gif", ThisCustomer.LocaleSetting) + "\" alt=\"" + AppLogic.GetString("image.altText.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                        }
                        else
                        {
                            tmpS.Append("<a class=\"product-nav-link\" href=\"" + SE.MakeProductAndSectionLink(NextProductID, SectionID, "") + "\">" + AppLogic.GetString("showproduct.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                        }
                    }
                }
                tmpS.Append("</div>");
                result = tmpS.ToString();
            }
            return result;
        }

        public virtual string ProductNavLinks(String sProductID, String sEntityID, string sEntityName, string sEntitySEName, String sSortByLooks, String sUseGraphics, String sIncludeUpLink)
        {
            InputValidator IV = new InputValidator("ProductNavLinks");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int EntityID = IV.ValidateInt("EntityID", sEntityID);
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            String EntitySEName = IV.ValidateString("EntitySEName", sEntitySEName);
            bool SortByLooks = IV.ValidateBool("SortByLooks", sSortByLooks);
            bool UseGraphics = IV.ValidateBool("UseGraphics", sUseGraphics);
            bool IncludeUpLink = IV.ValidateBool("IncludeUpLink", sIncludeUpLink);
            string result = String.Empty;
            string SEName = String.Empty;
            StringBuilder tmpS = new StringBuilder("");
            if (EntityName.Trim() == "")
            {
                EntityName = "Category";
            }
            if (!AppLogic.AppConfigBool("HideProductNextPrevLinks"))
            {
                tmpS.Append("<div class=\"nav-links-wrap\">");
                int NumProducts = 0;
                NumProducts = AppLogic.LookupHelper(EntityName, 0).GetNumEntityObjects(EntityID, true, true);
                if (NumProducts > 1)
                {
                    int PreviousProductID = AppLogic.GetProductSequence("previous", ProductID, EntityID, EntityName, 0, SortByLooks, true, true, ThisCustomer, out SEName);
                    if (UseGraphics)
                    {
                        tmpS.Append("<a class=\"ProductNavLink\" href=\"" + SE.MakeProductAndEntityLink(EntityName, PreviousProductID, EntityID, SEName) + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/previous.gif", ThisCustomer.LocaleSetting) + "\" alt=\"" + AppLogic.GetString("image.altText.1", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                    }
                    else
                    {
                        tmpS.Append("<a class=\"ProductNavLink\" href=\"" + SE.MakeProductAndEntityLink(EntityName, PreviousProductID, EntityID, SEName) + "\">" + AppLogic.GetString("showproduct.aspx.5", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                    }
                }
                if (IncludeUpLink)
                {
                    if (UseGraphics)
                    {
                        tmpS.Append("<a class=\"ProductNavLink\" href=\"" + SE.MakeEntityLink(EntityName, EntityID, EntitySEName) + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/up.gif", ThisCustomer.LocaleSetting) + "\" alt=\"" + AppLogic.GetString("image.altText.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                    }
                    else
                    {
                        tmpS.Append("<a class=\"ProductNavLink\" href=\"" + SE.MakeEntityLink(EntityName, EntityID, EntitySEName) + "\">" + AppLogic.GetString("showproduct.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                    }
                }
                if (NumProducts > 1)
                {
                    int NextProductID = AppLogic.GetProductSequence("next", ProductID, EntityID, EntityName, 0, SortByLooks, true, true, ThisCustomer, out SEName);
                    if (UseGraphics)
                    {
                        tmpS.Append("<a class=\"ProductNavLink\" href=\"" + SE.MakeProductAndEntityLink(EntityName, NextProductID, EntityID, SEName) + "\"><img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/next.gif", ThisCustomer.LocaleSetting) + "\" alt=\"" + AppLogic.GetString("image.altText.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\"></a>");
                    }
                    else
                    {
                        tmpS.Append("<a class=\"ProductNavLink\" href=\"" + SE.MakeProductAndEntityLink(EntityName, NextProductID, EntityID, SEName) + "\">" + AppLogic.GetString("showproduct.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>");
                    }
                }
                tmpS.Append("</div>");
                result = tmpS.ToString();
            }
            return result;
        }

        public virtual string EmailProductToFriend(String sProductID, String sCategoryID)
        {
            InputValidator IV = new InputValidator("EmailProductToFriend");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            string result = String.Empty;
            if (AppLogic.AppConfigBool("ShowEMailProductToFriend"))
            {
                String S1 = String.Empty;

                result = String.Format(@"<div class='email-a-friend-wrap'>
											<a href='{0}?productid={1}' class='email-a-friend-link'>
												{2}
											</a>
										</div>", AppLogic.ResolveUrl("~/EMailproduct.aspx"), ProductID.ToString(), AppLogic.GetString("showproduct.aspx.20", ThisCustomer.LocaleSetting));
            }
            return result;
        }

        public virtual string ProductDescriptionFile(String sProductID, String sIncludeBRBefore)
        {
            InputValidator IV = new InputValidator("ProductDescriptionFile");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            bool IncludeBRBefore = IV.ValidateBool("IncludeBRBefore", sIncludeBRBefore);
            string result = String.Empty;
            String FileDescription = new ProductDescriptionFile(ProductID, ThisCustomer.LocaleSetting, ThisCustomer.SkinID).Contents;
            if (IncludeBRBefore && FileDescription.Length != 0)
            {
                result = FileDescription;
            }
            else
            {
                result = FileDescription;
            }
            return result;
        }
        /// <summary>
        /// Set whether the specs to show inline or to open in a new window
        /// </summary>
        /// <param name="sProductID">The ProductID</param>
        /// <param name="sIncludeBRBefore">Whether to clear or use br</param>
        /// <returns>html</returns>
        public virtual string ProductSpecs(String sProductID, String sIncludeBRBefore)
        {
            return ProductSpecs(sProductID, sIncludeBRBefore, "false", "", "600");
        }

        /// <summary>
        /// Set whether the specs to show inline or to open in a new window
        /// </summary>
        /// <param name="sProductID">The ProductID</param>
        /// <param name="sIncludeBRBefore">Whether to clear or use br </param>
        /// <param name="sShowSpecsInline">True or False, whether the spec to show in line</param>
        /// <param name="sSpecUrl">The path to the custom html file</param>
        /// <param name="sIFrameHeight">The frame height</param>
        /// <returns>html</returns>
        public virtual string ProductSpecs(String sProductID, String sIncludeBRBefore, String sShowSpecsInline, string sSpecUrl, string sIFrameHeight)
        {
            InputValidator IV = new InputValidator("ProductSpecs");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            bool IncludeBRBefore = IV.ValidateBool("IncludeBRBefore", sIncludeBRBefore);
            bool ShowSpecsInline = IV.ValidateBool("ShowSpecsInline", sShowSpecsInline);
            String SpecUrl = IV.ValidateString("SpecUrl", sSpecUrl);
            String IFrameHeight = IV.ValidateString("IFrameHeight", sIFrameHeight);
            string result = String.Empty;
            // Find the Specified ProductSpec content. will be in /descriptions/productspecs or some locale subdir. find by productid.
            ProductSpecFile ps = new ProductSpecFile(ProductID);
            bool ProductHasSpecs = (ps.Contents.Length != 0);

            if (IncludeBRBefore && ProductHasSpecs)
            {
                result = "<div class=\"clear\"></div><hr size=\"1\"/>";
            }
            result += "<a name=\"Specs\"></a>";

            //Show custom html in line if ShowSpecsInline is set to true in a frame
            if (SpecUrl.Trim().Length > 0 && ShowSpecsInline)
            {
                //create a frame to show the custom html in line
                result += "<iframe src=\"" + SpecUrl.Trim() + "\" SCROLLING=\"AUTO\" HEIGHT=\"" + IFrameHeight + "\" WIDTH=\"100%\"></iframe>";
            }
            //Show custom html in line if ShowSpecsInline is set to true 
            else if (ProductHasSpecs && ShowSpecsInline)
            {
                result += ps.Contents;
            }
            return result;
        }

        /// <summary>
        /// Set the link for custom html page that created for products
        /// </summary>
        /// <param name="sProductID">The Product ID</param>
        /// <param name="sShowSpecsInline">True or False, whether the spec to show in line</param>
        /// <param name="sSpecTitle">The name of the link that will appear on the product page</param>
        /// <param name="sSKU">The Product Sku</param>
        /// <returns>html link for new window</returns>
        public virtual string ProductSpecsLink(String sProductID, String sShowSpecsInline, string sSpecTitle, string sSKU)
        {
            return ProductSpecsLink(sProductID, sShowSpecsInline, sSpecTitle, sSKU, "");
        }

        /// <summary>
        /// Set the link for custom html page that created for products
        /// </summary>
        /// <param name="sProductID">The Product ID</param>
        /// <param name="sShowSpecsInline">True or False, whether the spec to show in line</param>
        /// <param name="sSpecTitle">The name of the link that will appear on the product page</param>
        /// <param name="sSKU">The Product Sku</param>
        /// <param name="sSpecUrl">The path to the custom html file</param>
        /// <returns>html link for new window</returns>
        public virtual string ProductSpecsLink(String sProductID, String sShowSpecsInline, string sSpecTitle, string sSKU, string sSpecUrl)
        {
            InputValidator IV = new InputValidator("ProductSpecsLink");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            bool ShowSpecsInline = IV.ValidateBool("ShowSpecsInline", sShowSpecsInline);
            String SpecTitle = IV.ValidateString("sSpecTitle", sSpecTitle);
            String SKU = IV.ValidateString("SKU", sSKU);
            String SpecUrl = IV.ValidateString("SpecUrl", sSpecUrl);
            StringBuilder results = new StringBuilder("");

            // Find the Specified ProductSpec content. will be in /descriptions/productspecs or some locale subdir. find by productid.
            ProductSpecFile pspec = new ProductSpecFile(ProductID, ThisCustomer.LocaleSetting, ThisCustomer.SkinID, SKU);
            bool ProductHasSpecs = (pspec.Contents.Length != 0);
            SpecUrl = CommonLogic.IIF(SpecUrl.Trim().Length == 0, pspec.URL, SpecUrl);
            String SpecLink = CommonLogic.IIF(ShowSpecsInline, "#Specs", SpecUrl);

            //If no spec title was set,the config value in AppConfig.DefaultSpecTitle will be use
            if (SpecTitle.Length == 0)
            {
                SpecTitle = AppLogic.GetString("AppConfig.DefaultSpecTitle", 1, Localization.GetDefaultLocale());
            }

            //If product has spec or url, add a link for new window for the custom html depending on the specurl
            if (ProductHasSpecs || SpecUrl.Length > 0)
            {
                results.Append("<a href=\"javascript:void(0);\" onClick=\"window.open('" + SpecUrl + "', 'null', 'height=600,width=500,scrollbars=yes,resizable=yes,status=yes,toolbar=no,menubar=no,location=no');\">" + SpecTitle + "</a>\n");
            }

            return results.ToString();
        }

        public virtual string ProductRatings(String sProductID, String sCategoryID, String sSectionID, String sManufacturerID, String sIncludeBRBefore)
        {
            bool showRating = AppLogic.AppConfigBool("RatingsEnabled");
            if (!showRating)
            {
                return string.Empty;
            }

            return ProductRatings(sProductID, sCategoryID, sSectionID, sManufacturerID, sIncludeBRBefore, true.ToString());
        }

        public virtual string ProductRatings(String sProductID, String sCategoryID, String sSectionID, String sManufacturerID, String sIncludeBRBefore, String sEncloseInTab)
        {
            InputValidator IV = new InputValidator("ProductRatings");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            int SectionID = IV.ValidateInt("SectionID", sSectionID);
            int ManufacturerID = IV.ValidateInt("ManufacturerID", sManufacturerID);
            bool IncludeBRBefore = IV.ValidateBool("IncludeBRBefore", sIncludeBRBefore);
            bool encloseInTab = IV.ValidateBool("EncloseInTab", sEncloseInTab);
            string result = String.Empty;

            String tmpS = Ratings.Display(ThisCustomer, ProductID, CategoryID, SectionID, ManufacturerID, ThisCustomer.SkinID, encloseInTab);
            if (IncludeBRBefore && tmpS.Length != 0 && encloseInTab)
            {
                result = "<div class=\"clear\"></div><hr size=\"1\"/>" + tmpS;
            }
            else
            {
                result = tmpS;
            }
            return result;
        }

        public virtual string ProductEntityList(String sProductID, string sEntityName)
        {
            InputValidator IV = new InputValidator("ProductEntityList");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            StringBuilder results = new StringBuilder("");

            EntityHelper eh = AppLogic.LookupHelper(EntityName.ToLowerInvariant(), 0);
            String Entities = eh.GetObjectEntities(ProductID, false);
            if (Entities.Length != 0)
            {
                String[] EntityIDs = Entities.Split(',');
                bool firstEntity = true;
                foreach (String s in EntityIDs)
                {
                    if (!firstEntity)
                    {
                        results.Append(", ");
                    }

                    string str = eh.GetEntityName(Localization.ParseUSInt(s), ThisCustomer.LocaleSetting).Trim();
                    if (!str.Equals(string.Empty))
                    {
                        results.Append("<a href=\"" + SE.MakeEntityLink(EntityName, Localization.ParseUSInt(s), String.Empty) + "\">" + str + "</a>");
                        firstEntity = false;
                    }
                    else
                    {
                        firstEntity = true;
                    }

                }
            }
            else
            {
                results.Append("");
            }
            return results.ToString();
        }

        // uses the DisplaySpec if provided for output display
        // else uses DisplayLocale if provided
        // input string expected in SQL Locale format
        public virtual string FormatCurrency(string sCurrencyValue)
        {
            InputValidator IV = new InputValidator("FormatCurrency");
            String CurrencyValue = IV.ValidateString("CurrencyValue", sCurrencyValue);
            return FormatCurrencyHelper(Localization.ParseDBDecimal(CurrencyValue));
        }

        // uses the DisplaySpec if provided for output display
        // else uses DisplayLocale if provided
        // input string expected in SQL Locale format
        public virtual string FormatCurrency(string sCurrencyValue, String sTargetCurrency)
        {
            InputValidator IV = new InputValidator("FormatCurrency");
            String CurrencyValue = IV.ValidateString("CurrencyValue", sCurrencyValue);
            String TargetCurrency = IV.ValidateString("TargetCurrency", sTargetCurrency);
            return FormatCurrencyHelper(Localization.ParseDBDecimal(CurrencyValue), TargetCurrency);
        }

        // internal helper function only!
        protected virtual String FormatCurrencyHelper(Decimal Amt)
        {
            InputValidator IV = new InputValidator("FormatCurrencyHelper");
            return Localization.CurrencyStringForDisplayWithExchangeRate(Amt, ThisCustomer.CurrencySetting);
        }

        // internal helper function only!
        protected virtual String FormatCurrencyHelper(Decimal Amt, String TargetCurrency)
        {
            if (TargetCurrency == null ||
                TargetCurrency.Length == 0)
            {
                TargetCurrency = ThisCustomer.CurrencySetting;
            }
            return Localization.CurrencyStringForDisplayWithExchangeRate(Amt, TargetCurrency);
        }

        public virtual string GetSpecialsBoxExpandedRandom(String sCategoryID, String sShowPics, String sIncludeFrame, String sTeaser)
        {
            InputValidator IV = new InputValidator("GetSpecialsBoxExpandedRandom");
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            bool ShowPics = IV.ValidateBool("ShowPics", sShowPics);
            bool IncludeFrame = IV.ValidateBool("IncludeFrame", sIncludeFrame);
            String Teaser = IV.ValidateString("Teaser", sTeaser);
            string result = String.Empty;
            result = AppLogic.GetSpecialsBoxExpandedRandom(CategoryID, ShowPics, IncludeFrame, Teaser, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, ThisCustomer);
            return result;
        }

        public virtual string GetSpecialsBoxExpanded(String sCategoryID, String sShowNum, String sShowPics, String sIncludeFrame, String sTeaser)
        {
            InputValidator IV = new InputValidator("GetSpecialsBoxExpanded");
            int CategoryID = IV.ValidateInt("CategoryID", sCategoryID);
            int ShowNum = IV.ValidateInt("ShowNum", sShowNum);
            bool ShowPics = IV.ValidateBool("ShowPics", sShowPics);
            bool IncludeFrame = IV.ValidateBool("IncludeFrame", sIncludeFrame);
            String Teaser = IV.ValidateString("Teaser", sTeaser);
            string result = String.Empty;
            result = AppLogic.GetSpecialsBoxExpanded(CategoryID, ShowNum, AppLogic.CachingOn, ShowPics, IncludeFrame, Teaser, ThisCustomer.SkinID, ThisCustomer.LocaleSetting, ThisCustomer);
            return result;
        }

        public virtual string GetNewsBoxExpanded(String sShowCopy, String sShowNum, String sIncludeFrame, String sTeaser)
        {
            InputValidator IV = new InputValidator("GetNewsBoxExpanded");
            bool ShowCopy = IV.ValidateBool("ShowCopy", sShowCopy);
            int ShowNum = IV.ValidateInt("ShowNum", sShowNum);
            bool IncludeFrame = IV.ValidateBool("IncludeFrame", sIncludeFrame);
            String Teaser = IV.ValidateString("Teaser", sTeaser);
            string result = String.Empty;

            result = AppLogic.GetNewsBoxExpanded((CommonLogic.GetThisPageName(false).IndexOf("NEWS.ASPX", StringComparison.InvariantCultureIgnoreCase) == -1), ShowCopy, ShowNum, IncludeFrame, AppLogic.CachingOn, Teaser, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            return result;
        }

        [Obsolete("deprecated (7.0) in favor of the HtmlDecode method")]
        public virtual string Decode(string sHtmlContent)
        {
            InputValidator IV = new InputValidator("Decode");
            String HtmlContent = IV.ValidateString("HtmlContent", sHtmlContent);
            return HttpContext.Current.Server.HtmlDecode(HtmlContent);
        }

        public virtual string HtmlDecode(string sHtmlContent)
        {
            InputValidator IV = new InputValidator("Decode");
            String HtmlContent = IV.ValidateString("HtmlContent", sHtmlContent);
            return HttpContext.Current.Server.HtmlDecode(HtmlContent);
        }

        public virtual string HtmlEncode(string sHtmlContent)
        {
            InputValidator IV = new InputValidator("Decode");
            String HtmlContent = IV.ValidateString("HtmlContent", sHtmlContent);
            return HttpContext.Current.Server.HtmlEncode(HtmlContent);
        }

        public virtual string ShowUpsellProducts(String sProductID)
        {
            InputValidator IV = new InputValidator("ShowUpsellProducts");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            string result = String.Empty;
            String UpsellXmlPackage = AppLogic.AppConfig("XmlPackage.UpsellProductPackage");
            if (UpsellXmlPackage.Length != 0)
            {
                result = AppLogic.RunXmlPackage(UpsellXmlPackage, new Parser(ThisCustomer.SkinID, ThisCustomer), ThisCustomer, ThisCustomer.SkinID, String.Empty, "ProductID=" + ProductID.ToString(), true, true);
            }
            else
            {
                try
                {
                    // people type weird things in the upsell box field, so ignore any "issues"...no other good solution at the moment:                    
                    result = AppLogic.GetUpsellProductsBoxExpanded(ProductID, 100, true, String.Empty, AppLogic.AppConfig("RelatedProductsFormat").Equals("GRID", StringComparison.InvariantCultureIgnoreCase), ThisCustomer.SkinID, ThisCustomer);
                }
                catch
                {
                    result = string.Empty;
                }
            }
            return result;
        }

        [Obsolete("deprecated (7.0) in favor of the RelatedProducts method")]
        public virtual string ShowRelatedProducts(String sProductID)
        {
            return "<b>the ShowRelatedProducts extension function has been deprecated, please use the RelatedProducts extension function </b>";
        }

        public virtual string Decrypt(string sEncryptedData)
        {
            InputValidator IV = new InputValidator("Decrypt");
            string EncryptedData = IV.ValidateString("EncryptedData", sEncryptedData);
            if (EncryptedData.Length == 0)
            {
                return "";
            }
            return Security.UnmungeString(EncryptedData);
        }

        public virtual string EncryptString(string sString2Encrypt)
        {
            InputValidator IV = new InputValidator("EncryptString");
            string String2Encrypt = IV.ValidateString("String2Encrypt", sString2Encrypt);
            return Security.MungeString(String2Encrypt);
        }

        public virtual string Decrypt(string sEncryptedData, String sSaltKey)
        {
            InputValidator IV = new InputValidator("Decrypt");
            string EncryptedData = IV.ValidateString("EncryptedData", sEncryptedData);
            string SaltKey = IV.ValidateString("SaltKey", sSaltKey);
            if (EncryptedData.Length == 0)
            {
                return "";
            }
            return Security.UnmungeString(EncryptedData, SaltKey);
        }

        public virtual string DecryptCCNumber(string sCardNumberCrypt, string sOrderNumber)
        {
            InputValidator IV = new InputValidator("DecryptCCNumber");
            String CardNumberCrypt = IV.ValidateString("CardNumberCrypt", sCardNumberCrypt);
            int OrderNumber = IV.ValidateInt("OrderNumber", sOrderNumber);
            String CardNumber = Security.UnmungeString(CardNumberCrypt, Order.StaticGetSaltKey(OrderNumber));
            if (CardNumber.StartsWith(Security.ro_DecryptFailedPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                CardNumber = CardNumberCrypt;
            }
            return CardNumber;
        }

        public virtual string EncryptString(string sString2Encrypt, String sSaltKey)
        {
            InputValidator IV = new InputValidator("EncryptString");
            string String2Encrypt = IV.ValidateString("String2Encrypt", sString2Encrypt);
            string SaltKey = IV.ValidateString("SaltKey", sSaltKey);
            return Security.MungeString(String2Encrypt, SaltKey);
        }

        public virtual string XmlPackage(String sPackageName)
        {
            InputValidator IV = new InputValidator("XmlPackage");
            string PackageName = IV.ValidateString("PackageName", sPackageName);
            string result = String.Empty;
            if (PackageName.Length != 0)
            {
                if (!PackageName.EndsWith(".xml.config", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = "Incorrect package name or package type";
                }
                else
                {
                    // WARNING YOU COULD CAUSE ENDLESS RECURSION HERE! if your XmlPackage refers to itself in some direct, or INDIRECT! way!!
                    result = AppLogic.RunXmlPackage(PackageName, new Parser(ThisCustomer.SkinID, ThisCustomer), ThisCustomer, ThisCustomer.SkinID, String.Empty, String.Empty, true, true);
                }
            }
            return result;
        }

        /// <summary>
        /// XmlPackage overload which allows a package to be loaded with specified runtime parameters.
        /// </summary>
        /// <param name="PackageName">The name of the package to load. The package name must include the xml.config extension.</param>
        /// <param name="AdditionalRuntimeParms">Querystring containing additional parameters that will be passed to the package as runtime values.</param>
        /// <returns>results of executing the specified package</returns>
        public virtual string XmlPackage(String sPackageName, String sAdditionalRuntimeParms)
        {
            InputValidator IV = new InputValidator("XmlPackage");
            string PackageName = IV.ValidateString("PackageName", sPackageName);
            string AdditionalRuntimeParms = IV.ValidateString("AdditionalRuntimeParms", sAdditionalRuntimeParms);
            string result = String.Empty;
            if (PackageName.Length != 0)
            {
                if (!PackageName.EndsWith(".xml.config", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = "Incorrect package name or package type";
                }
                else
                {
                    // WARNING YOU COULD CAUSE ENDLESS RECURSION HERE! if your XmlPackage refers to itself in some direct, or INDIRECT! way!!
                    result = AppLogic.RunXmlPackage(PackageName, new Parser(ThisCustomer.SkinID, ThisCustomer), ThisCustomer, ThisCustomer.SkinID, string.Empty, AdditionalRuntimeParms, true, true);
                }
            }
            return result;
        }

        public virtual string ImageGallery(String sProductID, string sImageFileNameOverride, string sSKU, string sSwatchImageMap)
        {
            InputValidator IV = new InputValidator("ImageGallery");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            string SwatchImageMap = IV.ValidateString("SwatchImageMap", sSwatchImageMap);
            string SKU = IV.ValidateString("SKU", sSKU);
            string ImageFileNameOverride = IV.ValidateString("ImageFileNameOverride", sImageFileNameOverride);
            StringBuilder results = new StringBuilder("");
            // setup multi-image gallery:
            String SwatchPic = AppLogic.LookupImage("Product", ProductID, ImageFileNameOverride, SKU, "swatch", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);

            ProductImageGallery ImgGal = null;
            String ImgGalCacheName = "ImgGal_" + ProductID.ToString() + "_" + ThisCustomer.SkinID.ToString() + "_" + ThisCustomer.LocaleSetting;
            if (AppLogic.CachingOn)
            {
                ImgGal = (ProductImageGallery)HttpContext.Current.Cache.Get(ImgGalCacheName);
            }
            if (ImgGal == null)
            {
                ImgGal = new ProductImageGallery(ProductID, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            }
            if (AppLogic.CachingOn)
            {
                HttpContext.Current.Cache.Insert(ImgGalCacheName, ImgGal, null, DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
            }
            results.Append(ImgGal.ImgDHTML);

            if (ImgGal.ImgGalIcons.Length != 0 ||
                SwatchPic.Length != 0)
            {
                if (ImgGal.ImgGalIcons.Length != 0)
                {
                    results.Append("");
                    results.Append(ImgGal.ImgGalIcons);
                }
                if (SwatchPic.Length != 0)
                {
                    results.Append(HttpContext.Current.Server.HtmlDecode(SwatchImageMap));
                    results.Append("<img src=\"" + AppLogic.LocateImageURL("images/spacer.gif") + "\" width=\"1\" height=\"4\"><img class=\"actionelement\" src=\"" + SwatchPic + "\" usemap=\"#SwatchMap\" border=\"0\">");
                }
            }
            return results.ToString();
        }

        public virtual string ShowQuantityDiscountTable(String sProductID)
        {
            InputValidator IV = new InputValidator("ShowQuantityDiscountTable");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            StringBuilder results = new StringBuilder("");
            bool CustomerLevelAllowsQuantityDiscounts = QuantityDiscount.CustomerLevelAllowsQuantityDiscounts(ThisCustomer.CustomerLevelID);

            String MainProductSKU = String.Empty;
            int ActiveDIDID = QuantityDiscount.LookupProductQuantityDiscountID(ProductID);
            bool ActiveDID = (ActiveDIDID != 0);
            if (!CustomerLevelAllowsQuantityDiscounts)
            {
                ActiveDID = false;
            }
            if (ActiveDID)
            {
                results.Append("<div class=\"quantity-discount-wrap\">");
                bool ShowInLine = AppLogic.AppConfigBool("ShowQuantityDiscountTablesInline");
                results.Append("<span class=\"quantity-discount-header\">" + AppLogic.GetString("showproduct.aspx.8", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</span>");

                string Key = (Guid.NewGuid()).ToString().Substring(0, 4);
                string discountTableHTML = QuantityDiscount.GetQuantityDiscountDisplayTable(ActiveDIDID, ThisCustomer.SkinID);
                discountTableHTML = discountTableHTML.Replace("\r\n", "");

                if (!ShowInLine)
                {
                    results.Append(string.Format("(<a id=\"lnkQuantityDiscount_{0}", Key) + "\" href=\"javascript:void(0);\" class=\"quantity-discount-link\" >" + AppLogic.GetString("showproduct.aspx.9", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a>)");
                    results.Append("<script type=\"text/javascript\" language=\"Javascript\" src=\"" + AppLogic.ResolveUrl("~/jscripts/tooltip.js") + "\" ></script>\n");
                    results.Append("<script type=\"text/javascript\" language=\"Javascript\">\n");
                    results.AppendFormat("    $window_addLoad(function(){{ new ToolTip('lnkQuantityDiscount_{0}', 'discount-table-tooltip', '{1}'); }});\n", Key, discountTableHTML);
                    results.Append("</script>\n");
                }
                else
                {
                    results.Append("<div class=\"quantity-discount-table-wrap\">");
                    results.Append(QuantityDiscount.GetQuantityDiscountDisplayTable(ActiveDIDID, ThisCustomer.SkinID));
                    results.Append("</div>");
                }
                results.Append("</div>");
            }
            return results.ToString();
        }

        public virtual string GetProductDiscountID(String sProductID)
        {
            InputValidator IV = new InputValidator("GetProductDiscountID");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            return QuantityDiscount.LookupProductQuantityDiscountID(ProductID).ToString();
        }

        /// <summary>
        /// Display 'out of stock' or 'in stock' message
        /// </summary>
        /// <param name="productId">The product id</param>
        /// <param name="pages">Entity or Product</param>
        /// <returns></returns>
        public string DisplayProductStockHint(string productId, string pages)
        {
            return DisplayProductStockHint(productId, AppLogic.OUT_OF_STOCK_ALL_VARIANTS.ToString(), pages);
        }

        /// <summary>
        /// Display 'out of stock' or 'in stock' message 
        /// </summary>
        /// <param name="sProductID">The product id</param>
        /// <param name="sVariantID">The variant id</param>
        /// <param name="page">Entity or Product</param>
        /// <returns>HTML message</returns>
        public virtual string DisplayProductStockHint(string sProductID, string sVariantID, string page)
        {
            return DisplayProductStockHint(sProductID, sVariantID, page, "stock-hint");
        }

        /// <summary>
        /// Display 'out of stock' or 'in stock' message 
        /// </summary>
        /// <param name="sProductID">The product id</param>
        /// <param name="sVariantID">The variant id</param>
        /// <param name="page">Entity or Product</param>
        /// <param name="className">css class name</param>
        /// <returns>HTML message</returns>
        public virtual string DisplayProductStockHint(string sProductID, string sVariantID, string page, string className)
        {
            return DisplayProductStockHint(sProductID, sVariantID, page, className, "div");
        }

        /// <summary>
        /// Display 'out of stock' or 'in stock' message 
        /// </summary>
        /// <param name="sProductID">The product id</param>
        /// <param name="sVariantID">The variant id</param>
        /// <param name="className">css class name</param>
        /// <param name="renderAsElement">Span or div</param>
        /// <returns>HTML stock message</returns>
        public virtual string DisplayProductStockHint(string sProductID, string sVariantID, string page, string className, string renderAsElement)
        {
            return Product.DisplayStockHint(sProductID, sVariantID, page, className, renderAsElement);
        }

        public virtual string ShowInventoryTable(String sProductID)
        {
            InputValidator IV = new InputValidator("ShowInventoryTable");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            StringBuilder results = new StringBuilder("");
            if (AppLogic.AppConfigBool("ShowInventoryTable"))
            {
                results.Append(AppLogic.GetInventoryTable(ProductID, AppLogic.GetDefaultProductVariant(ProductID), ThisCustomer.IsAdminUser, ThisCustomer.SkinID, true, false));
            }
            return results.ToString();
        }

        public virtual string ShowInventoryTable(String sProductID, String sVariantID)
        {
            InputValidator IV = new InputValidator("ShowInventoryTable");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            StringBuilder results = new StringBuilder("");
            if (AppLogic.AppConfigBool("ShowInventoryTable"))
            {
                results.Append(AppLogic.GetInventoryTable(ProductID, VariantID, ThisCustomer.IsAdminUser, ThisCustomer.SkinID, true, false));
            }
            return results.ToString();
        }

        public virtual string GetJSPopupRoutines()
        {
            return AppLogic.GetJSPopupRoutines();
        }

        public virtual string GetKitPrice(String sProductID, string sPrice, string sSalePrice, string sExtendedPrice, String sHidePriceUntilCart, string sColors)
        {
            InputValidator IV = new InputValidator("GetKitPrice");
            decimal Price = Localization.ParseDBDecimal(sPrice);
            Decimal SalePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
            int TaxClassID = DB.GetSqlN("select TaxClassID from dbo.product where productid = " + sProductID);
            return GetKitPrice(sProductID, sPrice, sSalePrice, sExtendedPrice, sHidePriceUntilCart, sColors, "FALSE", TaxClassID.ToString());
        }

        public virtual string GetKitPrice(String sProductID, string sPrice, string sSalePrice, string sExtendedPrice, String sHidePriceUntilCart, string sColors, String sShoppingCartRecID, String intTaxClassID)
        {
            InputValidator IV = new InputValidator("GetKitPrice");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            bool HidePriceUntilCart = IV.ValidateBool("HidePriceUntilCart", sHidePriceUntilCart);
            int ShoppingCartRecID = IV.ValidateInt("ShoppingCartRecID", sShoppingCartRecID);
            Decimal Price = IV.ValidateDecimal("Price", sPrice);
            Decimal SalePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
            Decimal ExtendedPrice = IV.ValidateDecimal("ExtendedPrice", sExtendedPrice);
            String Colors = IV.ValidateString("Colors", sColors);
            int TaxClassID = IV.ValidateInt("TaxClassID", intTaxClassID);

            Decimal TaxRate = 0.0M;
            Decimal TaxMultiplier = 1.0M;
            if (m_VATOn)
            {
                TaxRate = Prices.TaxRate(ThisCustomer, TaxClassID);
                TaxMultiplier = (1.0M + (TaxRate / 100.00M));
            }

            StringBuilder results = new StringBuilder("");
            decimal BasePrice = Decimal.Zero;
            if (SalePrice != Decimal.Zero)
            {
                BasePrice = Decimal.Round(Currency.Convert(SalePrice, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                BasePrice = Decimal.Round(Currency.Convert(Price, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
            }
            results.Append("<a name=\"KitInPostBack\"></a>");
            results.Append("<div class=\"kit-price-wrap\">");
            bool KitIsComplete = AppLogic.KitContainsAllRequiredItems(ThisCustomer.CustomerID, ProductID, ShoppingCartRecID);
            string VATString = String.Empty;
            if (m_VATEnabled)
            {
                VATString = CommonLogic.IIF(m_VATOn, AppLogic.GetString("setvatsetting.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("setvatsetting.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
            }
            if (!HidePriceUntilCart && !AppLogic.AppConfigBool("HideKitPrice"))
            {
                ExtendedPrice = Decimal.Round(Currency.Convert(ExtendedPrice, Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero);
                decimal KitPriceDelta = AppLogic.KitPriceDelta(ThisCustomer.CustomerID, ProductID, ShoppingCartRecID, ThisCustomer.CurrencySetting);
                decimal KitPrice = (BasePrice + KitPriceDelta);
                decimal ExtPrice = (ExtendedPrice + KitPriceDelta);
                if (AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
                {

                }
                else if (ThisCustomer.CustomerLevelID == 0)
                {
                    results.Append("<b>" + AppLogic.GetString("showproduct.aspx.10", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(BasePrice * TaxMultiplier, ThisCustomer.CurrencySetting) + " " + "</b> " + VATString + "");
                    results.Append("<b>" + AppLogic.GetString("showproduct.aspx.11", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(KitPrice * TaxMultiplier, ThisCustomer.CurrencySetting) + "</b> " + VATString + "");
                }
                else
                {
                    decimal CustLvlPrice = CommonLogic.IIF(ExtendedPrice == 0.0M, KitPrice, ExtPrice);
                    if (ThisCustomer.LevelDiscountPct != 0.0M && (ExtendedPrice == 0.0M || ThisCustomer.DiscountExtendedPrices))
                    {
                        CustLvlPrice = CustLvlPrice * (decimal)(1.00M - (ThisCustomer.LevelDiscountPct / 100.0M)) * CommonLogic.IIF(m_VATOn, TaxMultiplier, 1.0M);
                    }
                    results.Append("<b><strike>" + AppLogic.GetString("showproduct.aspx.10", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(BasePrice, ThisCustomer.CurrencySetting) + " " + "</strike></b> " + VATString + "");
                    results.Append("<b><strike>" + AppLogic.GetString("showproduct.aspx.11", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(KitPrice, ThisCustomer.CurrencySetting) + "</strike></b> " + VATString + "");
                    results.Append("<b>" + ThisCustomer.CustomerLevelName + " " + AppLogic.GetString("showproduct.aspx.37", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(CustLvlPrice, ThisCustomer.CurrencySetting) + "</b> " + VATString + "");
                }
            }

            if (KitIsComplete)
            {
                ProductImageGallery pig = new ProductImageGallery(ProductID, Colors, ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                results.Append("<p><b>" + AppLogic.GetString("showproduct.aspx.12", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b>");
                if (ShoppingCartRecID == 0)
                {
                    results.Append(ShoppingCart.GetAddToCartForm(ThisCustomer, false, true, AppLogic.AppConfigBool("ShowWishButtons"), AppLogic.AppConfigBool("ShowGiftRegistryButtons"), ProductID, AppLogic.GetDefaultProductVariant(ProductID), ThisCustomer.SkinID, 1, ThisCustomer.LocaleSetting, !pig.IsEmpty(), VariantStyleEnum.RegularVariantsWithAttributes));
                }
                results.Append("</p>");
            }
            else
            {
                results.Append("<p><b>" + AppLogic.GetString("showproduct.aspx.13", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</b></p>");
            }
            results.Append("<p></p>"); // spacer
            results.Append("</div>");

            return results.ToString();
        }

        public virtual string GetKitItemOptions(String sProductID, String sThisGroupID, String sGroupIsRequired, string sGroupName, string sGroupDescription, String sKitGroupTypeID, String sHidePriceUntilCart)
        {
            InputValidator IV = new InputValidator("GetKitItemOptions");
            int TaxClassID = DB.GetSqlN("select TaxClassID from dbo.product where productid = " + sProductID);
            return GetKitItemOptions(sProductID, sThisGroupID, sGroupIsRequired, sGroupName, sGroupDescription, sKitGroupTypeID, sHidePriceUntilCart, "FALSE", TaxClassID.ToString());
        }

        public virtual string GetKitItemOptions(String sProductID, String sThisGroupID, String sGroupIsRequired, string sGroupName, string sGroupDescription, String sKitGroupTypeID, String sHidePriceUntilCart, String sShoppingCartRecID, String intTaxClassID)
        {
            InputValidator IV = new InputValidator("GetKitItemOptions");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int ShoppingCartRecID = IV.ValidateInt("ShoppingCartRecID", sShoppingCartRecID);
            int ThisGroupID = IV.ValidateInt("ThisGroupID", sThisGroupID);
            int KitGroupTypeID = IV.ValidateInt("KitGroupTypeID", sKitGroupTypeID);
            bool GroupIsRequired = IV.ValidateBool("GroupIsRequired", sGroupIsRequired);
            bool HidePriceUntilCart = IV.ValidateBool("HidePriceUntilCart", sHidePriceUntilCart);
            String GroupName = IV.ValidateString("GroupName", sGroupName);
            String GroupDescription = IV.ValidateString("GroupDescription", sGroupDescription);
            string Locale = Localization.GetDefaultLocale();
            bool HidePrice = false;
            int TaxClassID = IV.ValidateInt("TaxClassID", intTaxClassID);

            decimal TaxRate = 0.0M;
            Decimal TaxMultiplier = 1.0M;
            if (m_VATOn)
            {
                TaxRate = Prices.TaxRate(ThisCustomer, TaxClassID);
                TaxMultiplier += TaxRate / 100.00M;
            }

            decimal DiscountMultiplier = TaxMultiplier * (1 - (ThisCustomer.LevelDiscountPct / 100));

            StringBuilder results = new StringBuilder("");
            results.Append("<table width=\"100%\" cellpadding=\"6\" cellspacing=\"0\" border=\"0\">");
            int groupnumber = 0;

            groupnumber++;
            String bgcolor = CommonLogic.IIF(groupnumber % 2 == 0, "bgcolor=\"#FFFFFF\"", "class=\"LightCell\"");
            results.Append("<tr " + bgcolor + " align=\"left\">");
            results.Append("<td align=\"left\" class=\"GroupName\">");
            results.Append("<b>" + CommonLogic.IIF(GroupIsRequired, "*", "") + GroupName + "</b>");
            if (GroupDescription.Length != 0)
            {
                results.Append("<a href=\"javascript:void(0);\" onClick=\"popupkitgroupwh('Kit%20Information','" + ThisGroupID.ToString() + "',300,400,'yes')\" class=\"actionelement\"><img alt=\"" + AppLogic.GetString("showproduct.aspx.38", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/helpcircle.gif") + "\" align=\"absmiddle\"></a>");
            }
            results.Append("</td>");
            results.Append("<td width=10></td>");
            results.Append("<td align=\"right\">");
            results.Append("</td>");
            results.Append("</tr>");
            // show the group items:

            bool itemsfound = false;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                string query = "select * from KitItem   with (NOLOCK)  where KitGroupID=" + ThisGroupID.ToString() + " order by DisplayOrder,Name";
                using (IDataReader rsi = DB.GetRS(query, con))
                {
                    bool anyDescs = false;
                    bool CustomerHasSelectedAnyItemsInThisGroup = AppLogic.KitContainsAnyGroupItems(ThisCustomer.CustomerID, ProductID, ShoppingCartRecID, ThisGroupID);
                    int ix = 1;
                    switch (KitGroupTypeID)
                    {
                        case 1: // Single Select Dropdown  List
                            results.Append("<tr " + bgcolor + " align=\"left\">");
                            results.Append("<td valign=\"top\" align=\"left\">");
                            while (rsi.Read())
                            {
                                if (ix == 1)
                                {
                                    HidePrice = HidePriceUntilCart || AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID);
                                    results.Append("<select class=\"selitemoption\" size=\"1\" name=\"KitGroupID_" + ThisGroupID.ToString() + "\">");
                                }
                                itemsfound = true;
                                String IName = HttpContext.Current.Server.HtmlEncode(DB.RSFieldByLocale(rsi, "Name", ThisCustomer.LocaleSetting));
                                if (!HidePriceUntilCart && !AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
                                {
                                    decimal PR = Decimal.Round(Currency.Convert(DB.RSFieldDecimal(rsi, "PriceDelta"), Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero) * DiscountMultiplier;
                                    if (PR > Decimal.Zero)
                                    {
                                        IName += ", " + AppLogic.GetString("showproduct.aspx.14", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                    else if (PR < Decimal.Zero)
                                    {
                                        IName += ", " + AppLogic.GetString("showproduct.aspx.15", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                }
                                String IsSelected = String.Empty;
                                if (!CustomerHasSelectedAnyItemsInThisGroup)
                                {
                                    if (DB.RSFieldBool(rsi, "IsDefault"))
                                    {
                                        IsSelected = " selected=\"selected\" ";
                                    }
                                }
                                else
                                {
                                    if (AppLogic.KitContainsItem(ThisCustomer.CustomerID, ProductID, ShoppingCartRecID, DB.RSFieldInt(rsi, "KitItemID")))
                                    {
                                        IsSelected = " selected=\"selected\" ";
                                    }
                                }
                                results.Append("<option value=\"" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "\" " + IsSelected + ">" + IName + "</option>");
                                if (DB.RSField(rsi, "Description").Length != 0)
                                {
                                    anyDescs = true;
                                }
                                ix++;
                            }
                            if (ix > 1)
                            {
                                results.Append("</select>");
                            }
                            results.Append("</td>");
                            results.Append("<td width=10></td>");
                            results.Append("<td valign=\"top\" align=\"right\">");
                            if (GroupDescription.Length != 0 && anyDescs)
                            {
                                results.Append("<a href=\"javascript:void(0);\" class=\"actionelement\" onClick=\"popupkitgroupwh('Kit%20Information','" + ThisGroupID.ToString() + "',300,400,'yes')\"><img alt=\"" + AppLogic.GetString("showproduct.aspx.38", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/moreinfo.gif") + "\" align=\"absmiddle\"></a>");
                            }
                            results.Append("</td>");
                            results.Append("</tr>");
                            break;
                        case 2: // Single Select Radio List
                            while (rsi.Read())
                            {
                                //todo:review this - I'm pretty sure it should be HidePriceUntilCart || AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID) -JH
                                HidePrice = HidePriceUntilCart && !AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID);
                                results.Append("<tr " + bgcolor + " align=\"left\">");
                                results.Append("<td valign=\"top\" align=\"left\">");
                                itemsfound = true;
                                String IName = HttpContext.Current.Server.HtmlEncode(DB.RSFieldByLocale(rsi, "Name", ThisCustomer.LocaleSetting));
                                if (!HidePriceUntilCart && !AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
                                {
                                    decimal PR = Decimal.Round(Currency.Convert(DB.RSFieldDecimal(rsi, "PriceDelta"), Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero) * DiscountMultiplier;
                                    if (PR > Decimal.Zero)
                                    {
                                        IName += ", " + AppLogic.GetString("showproduct.aspx.14", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                    else if (PR < Decimal.Zero)
                                    {
                                        IName += ", " + AppLogic.GetString("showproduct.aspx.15", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                }
                                String IsSelected = String.Empty;
                                if (!CustomerHasSelectedAnyItemsInThisGroup)
                                {
                                    if (DB.RSFieldBool(rsi, "IsDefault"))
                                    {
                                        IsSelected = " checked=\"checked\" ";
                                    }
                                }
                                else
                                {
                                    if (AppLogic.KitContainsItem(ThisCustomer.CustomerID, ProductID, ShoppingCartRecID, DB.RSFieldInt(rsi, "KitItemID")))
                                    {
                                        IsSelected = " checked=\"checked\" ";
                                    }
                                }
                                results.Append("<input type=\"radio\" name=\"KitGroupID_" + ThisGroupID.ToString() + "\" value=\"" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "\" " + IsSelected + "/>" + IName);
                                results.Append("</td>");
                                results.Append("<td width=10></td>");
                                results.Append("<td align=\"right\">");
                                if (DB.RSField(rsi, "Description").Length != 0)
                                {
                                    results.Append("<a href=\"javascript:void(0);\" class=\"actionelement\" onClick=\"popupkititemwh('Kit%20Information','" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "',300,400,'yes')\"><img alt=\"" + AppLogic.GetString("showproduct.aspx.38", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/moreinfo.gif") + "\" align=\"absmiddle\"></a>");
                                }
                                results.Append("</td>");
                                results.Append("</tr>");
                            }
                            break;
                        case 3: // Multi Select Checkbox
                            while (rsi.Read())
                            {
                                HidePrice = HidePriceUntilCart && !AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID);
                                results.Append("<tr " + bgcolor + " align=\"left\">");
                                results.Append("<td valign=\"top\" align=\"left\">");
                                itemsfound = true;
                                String IName = HttpContext.Current.Server.HtmlEncode(DB.RSFieldByLocale(rsi, "Name", ThisCustomer.LocaleSetting));
                                if (!HidePriceUntilCart && !AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))
                                {
                                    decimal PR = Decimal.Round(Currency.Convert(DB.RSFieldDecimal(rsi, "PriceDelta"), Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero) * DiscountMultiplier;
                                    if (PR > Decimal.Zero)
                                    {
                                        IName += ", " + AppLogic.GetString("showproduct.aspx.14", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                    else if (PR < Decimal.Zero)
                                    {
                                        IName += ", " + AppLogic.GetString("showproduct.aspx.15", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                }
                                String IsSelected = String.Empty;
                                if (CustomerHasSelectedAnyItemsInThisGroup)
                                {
                                    if (AppLogic.KitContainsItem(ThisCustomer.CustomerID, ProductID, ShoppingCartRecID, DB.RSFieldInt(rsi, "KitItemID")))
                                    {
                                        IsSelected = " checked=\"checked\" ";
                                    }
                                }
                                results.Append("<input type=\"checkbox\" name=\"KitGroupID_" + ThisGroupID.ToString() + "_KitItemID_" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "\" value=\"" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "\" " + IsSelected + "/>" + IName);
                                results.Append("</td>");
                                results.Append("<td width=10></td>");
                                results.Append("<td align=\"right\">");
                                if (DB.RSField(rsi, "Description").Length != 0)
                                {
                                    results.Append("<a href=\"javascript:void(0);\" class=\"actionelement\" onClick=\"popupkititemwh('Kit%20Information','" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "',300,400,'yes')\"><img alt=\"" + AppLogic.GetString("showproduct.aspx.38", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/moreinfo.gif") + "\" align=\"absmiddle\"></a>");
                                }
                                results.Append("</td>");
                                results.Append("</tr>");
                            }
                            break;
                        case 4: //Text Option
                        case 5: //Text Area
                            while (rsi.Read())
                            {
                                HidePrice = HidePriceUntilCart;
                                results.Append("<tr " + bgcolor + " align=\"left\">");
                                results.Append("<td valign=\"top\" align=\"left\">");
                                itemsfound = true;
                                String Iname = HttpContext.Current.Server.HtmlEncode(DB.RSFieldByLocale(rsi, "Name", ThisCustomer.LocaleSetting));
                                if (!HidePriceUntilCart)
                                {
                                    decimal PR = Decimal.Round(Currency.Convert(DB.RSFieldDecimal(rsi, "PriceDelta"), Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero) * DiscountMultiplier;
                                    if (PR > Decimal.Zero)
                                    {
                                        Iname += ", " + AppLogic.GetString("showproduct.aspx.14", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                    else if (PR < Decimal.Zero)
                                    {
                                        Iname += ", " + AppLogic.GetString("showproduct.aspx.15", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                }
                                String sTextOption = AppLogic.KitContainsText(ThisCustomer.CustomerID, ProductID, ShoppingCartRecID, DB.RSFieldInt(rsi, "KitItemID"));
                                if (KitGroupTypeID == 4)
                                {
                                    results.Append(Iname + "");
                                    results.Append(" <input type=\"text\" name=\"KitGroupID_" + ThisGroupID.ToString() + "_TextOption_" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "\" value=\"" + sTextOption + "\">");
                                }
                                else
                                {
                                    results.Append(Iname + "");
                                    results.Append("<textarea style=\"width:100%\" rows=\"" + ((DB.RSFieldInt(rsi, "TextOptionHeight") == 0) ? 5 : DB.RSFieldInt(rsi, "TextOptionHeight")).ToString() + "\" name=\"KitGroupID_" + ThisGroupID.ToString() + "_TextOption_" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "\" value=\"" + sTextOption + "\">" + sTextOption + "</textarea>");
                                }
                                results.Append("</td>");
                                results.Append("<td width=10></td>");
                                results.Append("<td align=\"right\">");
                                if (DB.RSField(rsi, "Description").Length != 0)
                                {
                                    results.Append("<a href=\"javascript:void(0);\" class=\"actionelement\" onClick=\"popupkititemwh('Kit%20Information','" + DB.RSFieldInt(rsi, "KitItemID").ToString() + "',300,400,'yes')\"><img alt=\"" + AppLogic.GetString("showproduct.aspx.38", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/moreinfo.gif") + "\" align=\"absmiddle\"></a>");
                                }
                                results.Append("</td>");
                                results.Append("</tr>");
                            }
                            break;
                        case 6: //File option
                            while (rsi.Read())
                            {
                                HidePrice = HidePriceUntilCart;
                                results.Append("<tr " + bgcolor + " align=\"left\">");
                                results.Append("<td valign=\"top\" align=\"left\">");
                                itemsfound = true;
                                String Iname = HttpContext.Current.Server.HtmlEncode(DB.RSFieldByLocale(rsi, "Name", ThisCustomer.LocaleSetting));
                                if (!HidePriceUntilCart)
                                {
                                    decimal PR = Decimal.Round(Currency.Convert(DB.RSFieldDecimal(rsi, "PriceDelta"), Localization.StoreCurrency(), ThisCustomer.CurrencySetting), 2, MidpointRounding.AwayFromZero) * DiscountMultiplier;
                                    if (PR > Decimal.Zero)
                                    {
                                        Iname += ", " + AppLogic.GetString("showproduct.aspx.14", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                    else if (PR < Decimal.Zero)
                                    {
                                        Iname += ", " + AppLogic.GetString("showproduct.aspx.15", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(PR, ThisCustomer.CurrencySetting);
                                    }
                                }
                                String useID = DB.RSFieldInt(rsi, "KitItemID").ToString();
                                results.Append("<input type=\"hidden\" name=\"KitGroupID_" + ThisGroupID.ToString() + "_FileOption_" + useID + "\" value=\"" + useID + "\">");
                                String sFileName = AppLogic.KitContainsText(ThisCustomer.CustomerID, ProductID, ShoppingCartRecID, DB.RSFieldInt(rsi, "KitItemID"));
                                results.Append(Iname + " <input type=\"file\" name=\"FileName_" + useID + "\">");
                                if (sFileName.Length > 0)
                                {
                                    results.Append("<small><a class=\"actionelement\" onclick=\"deleteimage(" + useID + ", this)\" >" + AppLogic.GetString("showproduct.aspx.1000", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</a></small>");
                                    results.Append("");
                                    results.Append("<img id=\"Img_" + useID + "\" src=\"" + sFileName + "\"  border=\"0\"/>");
                                    results.Append("<input id=\"FileNameH_" + useID + "\" type=\"hidden\" name=\"FileName_" + useID + "\" value=\"" + sFileName + "\">");
                                }
                                results.Append("</td>");
                                results.Append("<td width=10></td>");
                                results.Append("<td align=\"right\">");
                                if (DB.RSField(rsi, "Description").Length != 0)
                                {
                                    results.Append("<a href=\"javascript:void();\" class=\"actionelement\" onClick=\"popupkititemwh('Kit%20Information','" + useID + "',300,400,'yes')\"><img alt=\"" + AppLogic.GetString("showproduct.aspx.38", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/moreinfo.gif") + "\" align=\"absmiddle\"></a>");
                                }
                                results.Append("</td>");
                                results.Append("</tr>");
                            }
                            break;
                    }
                }
            }

            if (!itemsfound)
            {
                results.Append("<tr " + bgcolor + " align=\"left\">");
                results.Append("<td align=\"left\">");
                results.Append(String.Format(AppLogic.GetString("showproduct.aspx.16", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), GroupName.ToLowerInvariant()));
                results.Append("</td>");
                results.Append("<td width=10></td>");
                results.Append("<td align=\"right\">");
                results.Append("</td>");
                results.Append("</tr>");
            }

            results.Append("</table>");

            return results.ToString();
        }

        public virtual string GetCustomerLevelPrice(String sVariantID, string sPrice, String sPoints)
        {
            InputValidator IV = new InputValidator("GetCustomerLevelPrice");
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            string Locale = Localization.GetDefaultLocale();
            decimal Price = Localization.ParseDBDecimal(sPrice);
            int Points = IV.ValidateInt("Points", sPoints);

            bool IsOnSale = false;
            decimal LevelPrice = AppLogic.DetermineLevelPrice(VariantID, ThisCustomer.CustomerLevelID, out IsOnSale);
            String PriceString = "<b>" + AppLogic.GetString("showproduct.aspx.27", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + FormatCurrencyHelper(Price, ThisCustomer.CurrencySetting) + "</b>";
            PriceString += "<b><font color=" + AppLogic.AppConfig("OnSaleForTextColor") + ">" + ThisCustomer.CustomerLevelName + " " + AppLogic.GetString("showproduct.aspx.26", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</font><font color=" + AppLogic.AppConfig("OnSaleForTextColor") + ">" + FormatCurrencyHelper(LevelPrice, ThisCustomer.CurrencySetting) + "</font></b>";
            PriceString += CommonLogic.IIF(ThisCustomer.CustomerLevelID != 0 && AppLogic.AppConfigBool("MicroPay.ShowPointsWithPrices"), "(" + Points.ToString() + " Points)", "");
            return PriceString;
        }

        //this version of GetVariantPrice is used only for backward compatibility.  New packages should use one of the methods that accept TaxClassID
        public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName)
        {
            return GetVariantPrice(sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, "True", "1", "0.00");
        }

        public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, string sTaxClassID)
        {
            return GetVariantPrice(sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, "True", sTaxClassID, "0.00");
        }

        public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID)
        {
            return GetVariantPrice(sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, "0.00");
        }

        public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, String sChosenAttributesPriceDelta)
        {
            return GetVariantPrice(sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, sChosenAttributesPriceDelta, "true");
        }

        public virtual string GetVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, String sChosenAttributesPriceDelta, String sIncludeHTMLMarkup)
        {
            // validate paramters
            InputValidator IV = new InputValidator("GetVariantPrice");
            int variantID = IV.ValidateInt("VariantID", sVariantID);
            bool hidePriceUntilCart = IV.ValidateBool("HidePriceUntilCart", sHidePriceUntilCart);
            decimal regularPrice = IV.ValidateDecimal("Price", sPrice);
            decimal salePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
            decimal extPrice = IV.ValidateDecimal("ExtPrice", sExtPrice);
            int points = IV.ValidateInt("Points", sPoints);
            string salesPromptName = IV.ValidateString("SalesPromptName", sSalesPromptName);
            bool showPriceLabel = IV.ValidateBool("Showpricelabel", sShowpricelabel);
            int taxClassID = IV.ValidateInt("TaxClassID", sTaxClassID);
            decimal attributesPriceDelta = IV.ValidateDecimal("AttributesPriceDelta", sChosenAttributesPriceDelta);
            bool includeHTMLMarkup = IV.ValidateBool("IncludeHTMLMarkup", sIncludeHTMLMarkup);
            decimal discountedPrice = System.Decimal.Zero;
            decimal schemaPrice = 0;

            // instantiate return variable
            StringBuilder results = new StringBuilder(1024);
            results.Append("<div class=\"price-wrap\">");
            // short-circuit this procedure if the price will be hidden
            if (hidePriceUntilCart)
            {
                return string.Empty;
            }

            string taxSuffix = string.Empty;

            bool taxable = false;

            if (m_VATOn)
            {
                taxable = Prices.IsTaxable(variantID);

                // get suffix to display after pricing             
                if (m_VATEnabled)
                {
                    // put another validation stuff here if the product variant is taxable or not 
                    // set tax suffix to an empty string when the item is non-taxable regardless if the VAT Setting is either
                    // Inclusive or Exclusive
                    if (m_VATOn && !taxable)
                    {
                        taxSuffix = String.Empty;
                    }
                    else if (m_VATOn && taxable)
                    {
                        taxSuffix = AppLogic.GetString("setvatsetting.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                    else if (!m_VATOn && taxable)
                    {
                        taxSuffix = AppLogic.GetString("setvatsetting.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
            }

            Decimal origRegularPrice = regularPrice;

            // add inclusive tax, convert all pricing to ThisCustomer's currency, and round
            regularPrice = Prices.VariantPrice(ThisCustomer, variantID, origRegularPrice, salePrice, extPrice, attributesPriceDelta, false, taxClassID);
            discountedPrice = Prices.VariantPrice(ThisCustomer, variantID, origRegularPrice, salePrice, extPrice, attributesPriceDelta, true, taxClassID);

            // format pricing
            string regularPriceFormatted = Localization.CurrencyStringForDisplayWithExchangeRate(regularPrice, ThisCustomer.CurrencySetting);
            string discountedPriceFormatted = Localization.CurrencyStringForDisplayWithExchangeRate(discountedPrice, ThisCustomer.CurrencySetting);

            // get pricing labels
            string genericPriceLabel = string.Empty;
            string regularPriceLabel = string.Empty;
            string salePriceLabel = string.Empty;
            string customerLevelName = string.Empty;

            if (showPriceLabel)
            {
                genericPriceLabel = AppLogic.GetString("showproduct.aspx.26", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                regularPriceLabel = AppLogic.GetString("showproduct.aspx.27", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                salePriceLabel = salesPromptName + ":";
                customerLevelName = ThisCustomer.CustomerLevelName;
            }

            // format micropay points
            string pointsFormatted = string.Empty;
            if (AppLogic.AppConfigBool("MicroPay.ShowPointsWithPrices"))
            {
                pointsFormatted = "(" + points.ToString() + " Points)";
            }

            // create results string
            if (AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))  // wholesale site with default customerLevel
            {

            }
            else  // show Level 0 Pricing
            {
                if (salePrice == 0 || ThisCustomer.CustomerLevelID > 0)
                {
                    if (includeHTMLMarkup)
                    {
                        results.Append("<div class=\"variant-price\"><span>" + genericPriceLabel + "</span> " + regularPriceFormatted + "</div>");
                    }
                    else
                    {
                        results.Append(genericPriceLabel + regularPriceFormatted);
                    }
                    schemaPrice = regularPrice;
                }
                else if (includeHTMLMarkup)
                {
                    results.Append("<div class=\"price regular-price\"><span>" + regularPriceLabel + "</span> " + regularPriceFormatted + "</div>");
                    results.Append("<div class=\"price sale-price\"><span>" + salePriceLabel + "</span> " + discountedPriceFormatted + "</div>");
                    schemaPrice = discountedPrice;
                }
                else
                {
                    results.Append(" " + regularPriceLabel + regularPriceFormatted + " " + salePriceLabel + discountedPriceFormatted);
                    schemaPrice = discountedPrice;
                }

                results.Append(" ");

                results.Append(taxSuffix);
            }

            /*
            // handle non-discounted customerLevel cases
            if (ThisCustomer.CustomerLevelID == 0 || (ThisCustomer.LevelDiscountPct == 0 && extPrice == 0))
            {
                if (AppLogic.HideForWholesaleSite(ThisCustomer.CustomerLevelID))  // wholesale site with default customerLevel
                {

                }
                else  // show Level 0 Pricing
                {
                    if (salePrice == 0 || ThisCustomer.CustomerLevelID > 0)
                    {
                        if (includeHTMLMarkup)
                        {
                            results.Append("<div class=\"variant-price\"><span>" + genericPriceLabel + "</span> " + regularPriceFormatted + "</div>");
                        }
                        else
                        {
                            results.Append(genericPriceLabel + regularPriceFormatted);                            
                        }
                        schemaPrice = regularPrice;
                    }
                    else if (includeHTMLMarkup)
                    {
                        results.Append("<div class=\"price regular-price\"><span>" + regularPriceLabel + "</span> " + regularPriceFormatted + "</div>");
                        results.Append("<div class=\"price sale-price\"><span>" + salePriceLabel + "</span> " + discountedPriceFormatted + "</div>");
                        schemaPrice = discountedPrice;
                    }
                    else
                    {
                        results.Append(" " + regularPriceLabel + regularPriceFormatted + " " + salePriceLabel + discountedPriceFormatted);
                        schemaPrice = discountedPrice;
                    }

                    results.Append(" ");

                    results.Append(taxSuffix);
                }
            }

            // handle discounted customerLevels
            else
            {
                results.Append("<div class=\"price regular-price\">" + regularPriceLabel + " " + regularPriceFormatted + "</div>");
                results.Append("<div class=\"price level-price\">" + customerLevelName + " " + regularPriceLabel + " " + discountedPriceFormatted + "</div>");
                schemaPrice = discountedPrice;
                results.Append(pointsFormatted);
                results.Append(taxSuffix);
            }
            */

            if (schemaPrice > 0)
            {
                var storeDefaultCultureInfo = CultureInfo.GetCultureInfo(Localization.GetDefaultLocale());
                var formattedSchemaPrice = String.Format(storeDefaultCultureInfo, "{0:C}", schemaPrice);
                var schemaRegionInfo = new RegionInfo(storeDefaultCultureInfo.Name);

                results.AppendFormat("<meta itemprop=\"price\" content=\"{0}\"/>", formattedSchemaPrice);
                results.AppendFormat("<meta itemprop=\"priceCurrency\" content=\"{0}\"/>", schemaRegionInfo.ISOCurrencySymbol);
            }

            results.Append("</div>");
            return results.ToString();
        }

        public virtual decimal GetVariantPriceDecimal(String sVariantID, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sTaxClassID, string sChosenAttributesPriceDelta, string sConvertForCustomerCurrency, string sUseAnonymousCustomer)
        {
            // validate paramters
            InputValidator IV = new InputValidator("GetVariantPrice");
            int variantID = IV.ValidateInt("VariantID", sVariantID);
            decimal regularPrice = IV.ValidateDecimal("Price", sPrice);
            decimal salePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
            decimal extPrice = IV.ValidateDecimal("ExtPrice", sExtPrice);
            int points = IV.ValidateInt("Points", sPoints);
            int taxClassID = IV.ValidateInt("TaxClassID", sTaxClassID);
            decimal attributesPriceDelta = IV.ValidateDecimal("AttributesPriceDelta", sChosenAttributesPriceDelta);
            bool convertForCustomerCurrency = IV.ValidateBool("ConvertForCustomerCurrency", sConvertForCustomerCurrency);
            bool useAnonymousCustomer = IV.ValidateBool("UseAnonymousCustomer", sUseAnonymousCustomer);

            Customer customerToUse = ThisCustomer;
            if (useAnonymousCustomer)
                customerToUse = new Customer(true);

            decimal discountedPrice = System.Decimal.Zero;
            bool taxable = m_VATOn && Prices.IsTaxable(variantID);
            Decimal origRegularPrice = regularPrice;

            // add inclusive tax, convert all pricing to ThisCustomer's currency, and round
            regularPrice = Prices.VariantPrice(customerToUse, variantID, origRegularPrice, salePrice, extPrice, attributesPriceDelta, false, taxClassID);
            discountedPrice = Prices.VariantPrice(customerToUse, variantID, origRegularPrice, salePrice, extPrice, attributesPriceDelta, true, taxClassID);

            // format pricing
            if (convertForCustomerCurrency)
            {
                regularPrice = Currency.Convert(regularPrice, Localization.GetPrimaryCurrency(), customerToUse.CurrencySetting);
                discountedPrice = Currency.Convert(discountedPrice, Localization.GetPrimaryCurrency(), customerToUse.CurrencySetting);
            }

            // create results string
            // handle non-discounted customerLevel cases
            if (customerToUse.CustomerLevelID == 0 || (customerToUse.LevelDiscountPct == 0 && extPrice == 0))
            {
                if (salePrice == 0 || customerToUse.CustomerLevelID > 0)
                {
                    return regularPrice;
                }
                else
                {
                    return discountedPrice;
                }
            }
            // handle discounted customerLevels
            else
            {
                return discountedPrice;
            }
        }

        public virtual string GetUpsellVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, string decUpSelldiscountPct)
        {
            return Prices.GetUpsellVariantPrice(ThisCustomer, sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, decUpSelldiscountPct);
        }

        public virtual string GetUpsellVariantPrice(String sVariantID, String sHidePriceUntilCart, string sPrice, string sSalePrice, string sExtPrice, String sPoints, string sSalesPromptName, String sShowpricelabel, string sTaxClassID, string decUpSelldiscountPct, string sProductID, string sStartDate, string sEndDate)
        {
            return Prices.GetUpsellVariantPrice(ThisCustomer, sVariantID, sHidePriceUntilCart, sPrice, sSalePrice, sExtPrice, sPoints, sSalesPromptName, sShowpricelabel, sTaxClassID, decUpSelldiscountPct, sProductID, sStartDate, sEndDate);
        }

        public virtual string GetCartPrice(string intProductID, string intQuantity, string decProductPrice, string intTaxClassID)
        {
            return Prices.GetCartPrice(ThisCustomer, intProductID, intQuantity, decProductPrice, intTaxClassID);
        }

        public virtual Decimal SubTotal()
        {
            ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return Prices.SubTotal(true, false, true, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
        }

        public virtual Decimal SubTotal(bool includeDiscounts)
        {
            ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return Prices.SubTotal(includeDiscounts, false, true, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
        }

        public virtual Decimal SubTotal(bool includeDiscounts, bool onlyIncludeTaxableItems)
        {
            ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return Prices.SubTotal(includeDiscounts, onlyIncludeTaxableItems, true, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
        }

        public virtual Decimal SubTotal(bool includeDiscounts, bool onlyIncludeTaxableItems, bool includeDownloadItems)
        {
            ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return Prices.SubTotal(includeDiscounts, onlyIncludeTaxableItems, includeDownloadItems, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
        }

        public virtual Decimal SubTotal(bool includeDiscounts, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems)
        {
            ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return Prices.SubTotal(includeDiscounts, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
        }

        public virtual Decimal SubTotal(bool includeDiscounts, bool onlyIncludeTaxableItems, bool includeDownloadItems, bool includeFreeShippingItems, bool includeSystemItems)
        {
            ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return Prices.SubTotal(includeDiscounts, onlyIncludeTaxableItems, includeDownloadItems, includeFreeShippingItems, includeSystemItems, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions);
        }

        public virtual string CartSubTotal()
        {
            ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            return AppLogic.GetString("shoppingcart.cs.90", Convert.ToInt32(SkinID()), ThisCustomer.LocaleSetting) + " " + Localization.CurrencyStringForDisplayWithoutExchangeRate(Prices.SubTotal(true, false, true, true, true, true, cart.CartItems, cart.ThisCustomer, cart.OrderOptions), ThisCustomer.CurrencySetting);
        }

        public virtual string MiniCartOrderOption(string intOrderOptionID)
        {
            InputValidator IV = new InputValidator("MiniCartOrderOption");
            int orderOptionID = IV.ValidateInt("OrderOptionID", intOrderOptionID);

            ShoppingCart cart = new ShoppingCart(Convert.ToInt32(SkinID()), ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);
            bool VATEnabled = false;
            bool VATOn = false;

            decimal cost = decimal.Zero;
            StringBuilder costDisplay = new StringBuilder();
            int taxClassID = 0;
            string orderOptionName = string.Empty;

            VATEnabled = AppLogic.AppConfigBool("VAT.Enabled");
            VATOn = (VATEnabled && ThisCustomer.VATSettingReconciled == VATSettingEnum.ShowPricesInclusiveOfVAT);

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(string.Format("Select Name, TaxClassID, Cost FROM OrderOption where OrderOptionID = {0}", orderOptionID), conn))
                {
                    if (rs.Read())
                    {
                        orderOptionName = DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting);
                        taxClassID = DB.RSFieldInt(rs, "TaxClassID");
                        cost = DB.RSFieldDecimal(rs, "Cost");
                    }
                }
            }

            if (cost != decimal.Zero)
            {
                decimal TaxRate = Prices.TaxRate(ThisCustomer, taxClassID);
                decimal VAT = cost * (TaxRate / 100.0M);
                if (VATOn)
                {
                    cost += VAT;
                }
            }

            if (VATEnabled)
            {
                costDisplay.AppendFormat("{0} : {1} {2}", orderOptionName, ThisCustomer.CurrencyString(cost), CommonLogic.IIF(VATOn, AppLogic.GetString("setvatsetting.aspx.6", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), AppLogic.GetString("setvatsetting.aspx.7", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
            }
            else
            {
                costDisplay.AppendFormat("{0} : {1}", orderOptionName, ThisCustomer.CurrencyString(cost));
            }

            return costDisplay.ToString();
        }

        public virtual string EntityPageHeaderDescription(string sEntityName, String sEntityID)
        {
            InputValidator IV = new InputValidator("EntityPageHeaderDescription");
            String EntityName = IV.ValidateString("EntityName", sEntityName);
            int EntityID = IV.ValidateInt("EntityID", sEntityID);
            StringBuilder results = new StringBuilder("");

            string EntityInstancePicture = AppLogic.LookupImage(EntityName, EntityID, "medium", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
            EntityHelper m_EntityHelper = AppLogic.LookupHelper(EntityName, 0);
            XmlNode n = m_EntityHelper.m_TblMgr.SetContext(EntityID);

            // safety check to make sure someting invalid wasn't passed in

            // boolean flag indicating a supported entity
            bool entitySupported = false;

            // loop through each of the supported entities
            foreach (String eName in AppLogic.ro_SupportedEntities)
            {
                // if EntityName is equal to a supported entity
                if (eName.EqualsIgnoreCase(EntityName))
                {
                    // set EntityName to the name of a supported entity
                    EntityName = eName;

                    // flip flag indicating a supported entity
                    entitySupported = true;
                    break;
                }
            }

            // if a supported entity was not passed in, just return an empty string
            if (!entitySupported)
            {
                return String.Empty;
            }

            // end safety check

            string EntityInstanceDescription = DB.GetSqlS("select Description as S from dbo.{0} with(NOLOCK) where {0}ID=".FormatWith(EntityName) + EntityID.ToString());
            string DescrExcludeString = AppLogic.AppConfig("EntityDescrHTMLEqualsEmpty");
            bool hasImage = EntityInstanceDescription.Length != 0 && EntityInstancePicture.IndexOf("nopicture") == -1;

            if (DescrExcludeString.Length > 0)
            {
                foreach (string s in DescrExcludeString.Split(','))
                {
                    if (EntityInstanceDescription.Replace("\n", "").Trim() ==
                        s.Trim())
                    {
                        return string.Empty;
                    }
                }
            }
            if (AppLogic.ReplaceImageURLFromAssetMgr)
            {
                EntityInstanceDescription = EntityInstanceDescription.Replace("../images", "images");
            }
            String FileDescription = new DescriptionFile(EntityName, EntityID, ThisCustomer.LocaleSetting, ThisCustomer.SkinID).Contents;
            if (FileDescription.Length != 0)
            {
                EntityInstanceDescription += "<div class=\"page-row\">" + FileDescription + "</div>";
            }

            if (AppLogic.AppConfigBool("UseParserOnEntityDescriptions"))
            {
                Parser p = new Parser(ThisCustomer.SkinID, ThisCustomer);
                EntityInstanceDescription = p.ReplaceTokens(EntityInstanceDescription);
            }

            if (AppLogic.AppConfigBool("Force" + EntityName + "HeaderDisplay") ||
                EntityInstanceDescription.Length != 0)
            {
                results.Append("<div class=\"page-row entity-description-row\">\n");
                if (hasImage)
                {
                    results.Append("<div class=\"one-third entity-image-wrap\"><img class=\"grid-item-image img-responsive product-image\" src=\"" + EntityInstancePicture + "\"></div>");
                }
                results.Append(hasImage ? "<div class=\"two-thirds entity-description-wrap\">" : "<div class=\"full-width entity-description-wrap\">");
                results.Append("<div class=\"grid-column-inner\">");
                results.Append(EntityInstanceDescription);
                results.Append("</div>\n");
                results.Append("</div>\n");
                results.Append("</div>\n");
            }

            return results.ToString();
        }

        public virtual string GetMLValue(XPathNodeIterator MLContent)
        {
            InputValidator IV = new InputValidator("GetMLValue");
            return GetMLValue(MLContent, ThisCustomer.LocaleSetting);
        }

        public virtual string GetMLValue(XPathNodeIterator MLContent, string sLocale)
        {
            InputValidator IV = new InputValidator("GetMLValue");
            String Locale = IV.ValidateString("Locale", sLocale);
            if (Locale.Length == 0)
            {
                Locale = ThisCustomer.LocaleSetting;
            }
            return GetMLValue(MLContent, Locale, "FALSE");
        }

        public virtual string GetMLValue(XPathNodeIterator MLContent, string sLocale, String sXMLEncodeOutput)
        {
            InputValidator IV = new InputValidator("GetMLValue");
            String Locale = IV.ValidateString("Locale", sLocale);
            bool XMLEncodeOutput = IV.ValidateBool("XMLEncodeOutput", sXMLEncodeOutput);
            XPathNavigator xpn;
            MLContent.MoveNext();
            try
            {
                xpn = MLContent.Current;
            }
            catch
            {
                return "";
            }

            try
            {
                xpn.MoveToFirstChild();
                if (xpn.NodeType ==
                    XPathNodeType.Text)
                {
                    if (XMLEncodeOutput)
                    {
                        return xpn.OuterXml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;").Trim();
                    }
                    else
                    {
                        return HttpContext.Current.Server.HtmlDecode(xpn.OuterXml).Trim();
                    }
                }
                else
                {
                    XPathNavigator n = xpn.SelectSingleNode("./locale[@name='" + Locale + "']");
                    if (n == null)
                    {
                        if (Locale != Localization.GetDefaultLocale())
                        {
                            n = xpn.SelectSingleNode("./locale[@name='" + Localization.GetDefaultLocale() + "']");
                            if (n == null)
                            {
                                return "";
                            }
                            else
                            {
                                if (XMLEncodeOutput)
                                {
                                    return n.InnerXml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;").Trim();
                                }
                                else
                                {
                                    return HttpContext.Current.Server.HtmlDecode(n.InnerXml).Trim();
                                }
                            }
                        }
                        return "";
                    }
                    else
                    {
                        if (XMLEncodeOutput)
                        {
                            return n.InnerXml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;").Trim();
                        }
                        else
                        {
                            return HttpContext.Current.Server.HtmlDecode(n.InnerXml).Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "MLData Error: " + ex.Message;
            }
        }

        public virtual string Ellipses(string sContent, String sReturnLength, String sBreakBetweenWords)
        {
            InputValidator IV = new InputValidator("Ellipses");
            String Content = IV.ValidateString("Content", sContent);
            int ReturnLength = IV.ValidateInt("sReturnLength", sReturnLength);
            bool BreakBetweenWords = IV.ValidateBool("BreakBetweenWords", sBreakBetweenWords);
            return CommonLogic.Ellipses(Content, ReturnLength, BreakBetweenWords);
        }

        public virtual string SizeColorQtyOption(String sProductID, String sVariantID, string sColors, string sSizes)
        {
            return SizeColorQtyOption(sProductID, sVariantID, sColors, sSizes, AppLogic.GetString("AppConfig.ColorOptionPrompt", ThisCustomer.LocaleSetting), AppLogic.GetString("AppConfig.SizeOptionPrompt", ThisCustomer.LocaleSetting));
        }

        public virtual string SizeColorQtyOption(String sProductID, String sVariantID, string sColors, string sSizes, string sColorPrompt, string sSizePrompt)
        {
            return SizeColorQtyOption(sProductID, sVariantID, sColors, sSizes, AppLogic.GetString("AppConfig.ColorOptionPrompt", ThisCustomer.LocaleSetting), AppLogic.GetString("AppConfig.SizeOptionPrompt", ThisCustomer.LocaleSetting), "");
        }

        public virtual string SizeColorQtyOption(String sProductID, String sVariantID, string sColors, string sSizes, string sColorPrompt, string sSizePrompt, string sRestrictedQuantities)
        {
            return SizeColorQtyOption(sProductID, sVariantID, sColors, sSizes, AppLogic.GetString("AppConfig.ColorOptionPrompt", ThisCustomer.LocaleSetting), AppLogic.GetString("AppConfig.SizeOptionPrompt", ThisCustomer.LocaleSetting), "", "0", "", "1");
        }

        public virtual string SizeColorQtyOption(String sProductID, String sVariantID, string sColors, string sSizes, string sColorPrompt, string sSizePrompt, string sRestrictedQuantities, string boolCustomerEntersPrice, string sCustomerEntersPricePrompt, string intTaxClassID)
        {
            InputValidator IV = new InputValidator("SizeColorQtyOption");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            int VariantID = IV.ValidateInt("VariantID", sVariantID);
            String RestrictedQuantities = IV.ValidateString("RestrictedQuantities", sRestrictedQuantities);
            String Colors = IV.ValidateString("Colors", sColors);
            String Sizes = IV.ValidateString("Sizes", sSizes);
            bool CustomerEntersPrice = IV.ValidateBool("CustomerEntersPrice", boolCustomerEntersPrice);
            String CustomerEntersPricePrompt = IV.ValidateString("CustomerEntersPricePrompt", sCustomerEntersPricePrompt);
            int TaxClassID = IV.ValidateInt("TaxClassID", intTaxClassID);

            StringBuilder results = new StringBuilder("");
            String[] ColorsSplit = Colors.Split(',');
            String[] SizesSplit = Sizes.Split(',');

            sColorPrompt = CommonLogic.IIF(sColorPrompt.Length > 0, sColorPrompt, AppLogic.GetString("AppConfig.ColorOptionPrompt", ThisCustomer.LocaleSetting));
            sSizePrompt = CommonLogic.IIF(sSizePrompt.Length > 0, sSizePrompt, AppLogic.GetString("AppConfig.SizeOptionPrompt", ThisCustomer.LocaleSetting));
            results.Append("<div class=\"form-group table-order-group\">");
            if (CustomerEntersPrice)
            {
                results.Append("	<span class=\"add-to-cart-quantity\">");
                results.Append(CustomerEntersPricePrompt + " (" + ThisCustomer.CurrencySetting + ")");
                results.Append("		<input maxLength=\"10\" class=\"form-control quantity-field\" name=\"Price_" + ProductID.ToString() + "_" + VariantID.ToString() + "_" + 0.ToString() + "_" + 0.ToString() + "\" value=\"\">");
                results.Append("	</span>");
                results.Append("</div>");
                return results.ToString();
            }

            String Prompt = sColorPrompt + "/" + sSizePrompt;
            if (Sizes.Length == 0 && Colors.Length == 0)
            {
                results.Append("	<span class=\"add-to-cart-quantity\">");
                Prompt = AppLogic.GetString("common.cs.78", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                results.Append(Prompt);
                String FldName = ProductID.ToString() + "_" + VariantID.ToString() + "_" + 0.ToString() + "_" + 0.ToString();
                if (RestrictedQuantities.Trim().Length == 0)
                {
                    results.Append("<input name=\"Qty_" + FldName + "\" type=\"text\" class=\"form-control quantity-field\" maxlength=\"4\">");
                }
                else
                {
                    results.Append("<select name=\"Qty_" + FldName + "\" id=\"Qty_" + FldName + "\" onChange=\"if(typeof(getShipping) == 'function'){getShipping()};\" class=\"form-control quantity-field\">");
                    results.Append("<option value=\"\">" + AppLogic.GetString("admin.common.ddSelect", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</option>");
                    foreach (String s in RestrictedQuantities.Split(','))
                    {
                        if (s.Trim().Length != 0)
                        {
                            int Q = Localization.ParseUSInt(s.Trim());
                            results.Append("<option value=\"" + Q.ToString() + "\">" + Q.ToString() + "</option>");
                        }
                    }
                    results.Append("</select>");
                }
                results.Append("	</span>");
            }
            else
            {
                results.Append("<label>" + Prompt + "</label>\n");
                results.Append("<table class=\"table table-striped quantity-table\">\n");
                results.Append("<tr class=\"table-header\">\n");
                results.Append("<th> </th>\n");
                for (int i = SizesSplit.GetLowerBound(0); i <= SizesSplit.GetUpperBound(0); i++)
                {
                    string SizeString = SizesSplit[i].Trim();
                    if (SizeString.IndexOf("[") != -1)
                    {
                        decimal SizePriceDelta = AppLogic.GetColorAndSizePriceDelta("", SizesSplit[i].Trim(), TaxClassID, ThisCustomer, true, true);
                        SizeString = SizeString.Substring(0, SizesSplit[i].IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(SizePriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(SizePriceDelta) + "]";
                    }
                    results.Append("<th>" + SizeString + "</th>\n");
                }
                results.Append("</tr>\n");
                for (int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
                {
                    results.Append("<tr class=\"table-row\">\n");
                    string ColorString = ColorsSplit[i].Trim();
                    if (ColorString.IndexOf("[") != -1)
                    {
                        decimal ColorPriceDelta = AppLogic.GetColorAndSizePriceDelta(ColorsSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, true);
                        ColorString = ColorString.Substring(0, ColorsSplit[i].IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(ColorPriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(ColorPriceDelta) + "]";
                    }
                    results.Append("<td class=\"bold\">" + ColorString + "</td>\n");
                    for (int j = SizesSplit.GetLowerBound(0); j <= SizesSplit.GetUpperBound(0); j++)
                    {
                        results.Append("<td>");
                        String FldName = ProductID.ToString() + "_" + VariantID.ToString() + "_" + i.ToString() + "_" + j.ToString();
                        results.Append("<input name=\"Qty_" + FldName + "\" type=\"text\" class=\"form-control quantity-field\" maxlength=\"4\">");
                        results.Append("</td>\n");
                    }
                    results.Append("</tr>\n");
                }
                results.Append("</table>\n");
            }
            results.Append("</div>");

            return results.ToString();
        }

        public virtual string GetSizePriceDelta(string sSizes)
        {
            InputValidator IV = new InputValidator("SizePriceDelta");
            String Sizes = IV.ValidateString("Sizes", sSizes);
            int TaxClassID = IV.ValidateInt("TaxClassID", "1");

            StringBuilder results = new StringBuilder("");
            String[] SizesSplit = Sizes.Split(',');


            for (int i = SizesSplit.GetLowerBound(0); i <= SizesSplit.GetUpperBound(0); i++)
            {
                string SizeString = SizesSplit[i].Trim();
                if (SizeString.IndexOf("[") != -1)
                {
                    decimal SizePriceDelta = AppLogic.GetColorAndSizePriceDelta("", SizesSplit[i].Trim(), TaxClassID, ThisCustomer, true, true);
                    SizeString = SizeString.Substring(0, SizesSplit[i].IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(SizePriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(SizePriceDelta) + "]";
                }
                results.Append(SizeString);
            }


            return results.ToString();

        }

        public virtual string GetColorPriceDelta(string sColors)
        {
            InputValidator IV = new InputValidator("ColorPriceDelta");
            String Colors = IV.ValidateString("Colors", sColors);
            int TaxClassID = IV.ValidateInt("TaxClassID", "1");

            StringBuilder results = new StringBuilder("");
            String[] ColorsSplit = Colors.Split(',');

            for (int i = ColorsSplit.GetLowerBound(0); i <= ColorsSplit.GetUpperBound(0); i++)
            {
                string ColorString = ColorsSplit[i].Trim();
                if (ColorString.IndexOf("[") != -1)
                {
                    decimal ColorPriceDelta = AppLogic.GetColorAndSizePriceDelta(ColorsSplit[i].Trim(), "", TaxClassID, ThisCustomer, true, true);
                    ColorString = ColorString.Substring(0, ColorsSplit[i].IndexOf("[")).Trim() + " [" + AppLogic.AppConfig("AjaxPricingPrompt") + CommonLogic.IIF(ColorPriceDelta > 0, "+", "") + ThisCustomer.CurrencyString(ColorPriceDelta) + "]";
                }
                results.Append(ColorString);
            }


            return results.ToString();

        }

        public virtual string ReplaceNewLineWithBR(string sContent)
        {
            InputValidator IV = new InputValidator("ReplaceNewLineWithBR");
            String Content = IV.ValidateString("Content", sContent);
            return Content.Replace("\n", "");
        }

        public virtual string GetOrderReceiptCCNumber(string sLast4, string sCardType, string sCardExpirationMonth, string sCardExpirationYear)
        {
            InputValidator IV = new InputValidator("GetOrderReceiptCCNumber");
            String Last4 = IV.ValidateString("Last4", sLast4);
            String CardType = IV.ValidateString("CardType", sCardType);
            String CardExpirationMonth = IV.ValidateString("CardExpirationMonth", sCardExpirationMonth);
            String CardExpirationYear = IV.ValidateString("CardExpirationYear", sCardExpirationYear);
            StringBuilder results = new StringBuilder("");

            if (CardType.StartsWith("paypal", StringComparison.InvariantCultureIgnoreCase))
            {
                results.Append("<tr><td align=\"left\" width=\"20%\">" + AppLogic.GetString("order.cs.34", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td><td colspan=\"3\" width=\"80%\" align=\"left\">Not Available</td></tr>");
                results.Append("<tr><td align=\"left\" width=\"20%\">" + AppLogic.GetString("order.cs.35", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td><td colspan=\"3\" width=\"80%\" align=\"left\">Not Available</td></tr>");
            }
            else
            {
                results.Append("<tr><td align=\"left\" width=\"20%\">" + AppLogic.GetString("order.cs.34", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + "</td><td colspan=\"3\" width=\"80%\" align=\"left\">****" + Last4 + "</td></tr>");
            }
            return results.ToString();
        }

        public virtual string DisplayOrderOptions(string sOrderOptions, string sViewInLocaleSetting, String sUseFullPathToImages)
        {
            InputValidator IV = new InputValidator("DisplayOrderOptions");
            String OrderOptions = IV.ValidateString("OrderOptions", sOrderOptions);
            String ViewInLocaleSetting = IV.ValidateString("ViewInLocaleSetting", sViewInLocaleSetting);
            bool UseFullPathToImages = IV.ValidateBool("sUseFullPathToImages", sUseFullPathToImages);
            StringBuilder results = new StringBuilder("");
            if (OrderOptions.Length != 0)
            {
                results.Append("<div align=\"center\" width=\"100%\">");

                results.Append("<table cellpadding=\"2\" cellspacing=\"0\" border=\"0\">");
                results.Append("<tr>");
                results.Append("<td align=\"left\"><span class=\"OrderOptionsRowHeader\">" + AppLogic.GetString("order.cs.50", ThisCustomer.SkinID, ViewInLocaleSetting) + "</span></td>");
                results.Append("<td align=\"center\"><span class=\"OrderOptionsRowHeader\">" + AppLogic.GetString("order.cs.51", ThisCustomer.SkinID, ViewInLocaleSetting) + "</span></td>");
                results.Append("</tr>");
                foreach (String s in OrderOptions.Split('^'))
                {
                    String[] flds = s.Split('|');
                    results.Append("<tr>");
                    results.Append("<td align=\"left\">");
                    String ImgUrl = AppLogic.LookupImage("OrderOption", Localization.ParseUSInt(flds[0]), "icon", ThisCustomer.SkinID, ViewInLocaleSetting);
                    if (UseFullPathToImages)
                    {
                        if (ImgUrl.StartsWith("../"))
                        {
                            ImgUrl = ImgUrl.Replace("../", "");
                        }
                        ImgUrl = AppLogic.GetStoreHTTPLocation(true) + ImgUrl;
                    }
                    if (ImgUrl.Length != 0 &&
                        ImgUrl.IndexOf("nopicture") == -1)
                    {
                        results.Append("<img src=\"" + ImgUrl + "\" align=\"absmiddle\">");
                    }
                    results.Append("<span class=\"OrderOptionsName\">" + flds[1] + "</span></td>");
                    string VAT = "";
                    if (m_VATOn)
                    {
                        VAT = AppLogic.GetString("shoppingcart.aspx.17", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " 0.00";
                    }
                    if (flds.Length > 3)
                    {
                        VAT = AppLogic.GetString("shoppingcart.aspx.17", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " " + flds[3];
                    }
                    results.Append("<td width=\"150\" align=\"center\">");
                    results.Append("<span class=\"OrderOptionsPrice\">");
                    results.Append(flds[2]);
                    if (AppLogic.AppConfigBool("VAT.Enabled"))
                    {
                        results.Append("");
                        results.Append(VAT);
                    }
                    results.Append("</span>");
                    results.Append("</td>");
                    ImgUrl = AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/selected.gif");
                    if (UseFullPathToImages)
                    {
                        if (ImgUrl.StartsWith("../"))
                        {
                            ImgUrl = ImgUrl.Replace("../", "");
                        }
                        ImgUrl = AppLogic.GetStoreHTTPLocation(true) + ImgUrl;
                    }
                    results.Append("</tr>");
                }
                results.Append("</table>");
                results.Append("</div>");
            }

            return results.ToString();
        }

        public virtual string ToUpper(string sStrValue)
        {
            InputValidator IV = new InputValidator("ToUpper");
            String StrValue = IV.ValidateString("StrValue", sStrValue);
            return StrValue.ToUpperInvariant();
        }

        public virtual string ToLower(string sStrValue)
        {
            InputValidator IV = new InputValidator("ToLower");
            String StrValue = IV.ValidateString("StrValue", sStrValue);
            return StrValue.ToLowerInvariant();
        }

        public virtual string OrderShippingCalculation(string sPaymentMethod, string sShippingMethod, string sShippingTotal, String sShippingCalculationID, String sShipAddresses, String sIsAllDownloadComponents, String sIsAllFreeShippingComponents, String sIsAllSystemComponents)
        {
            InputValidator IV = new InputValidator("OrderShippingCalculation");
            String PaymentMethod = IV.ValidateString("PaymentMethod", sPaymentMethod);
            String ShippingMethod = IV.ValidateString("ShippingMethod", sShippingMethod);
            Decimal ShippingTotal = IV.ValidateDecimal("ShippingTotal", sShippingTotal);

            //this will handle if sShippingCalculationID is null or empty. For shippingcalculationid that is not set

            if (CommonLogic.IsStringNullOrEmpty(sShippingCalculationID))
            {
                sShippingCalculationID = ((int)Shipping.GetActiveShippingCalculationID()).ToString();
            }
            int ShippingCalculationID = IV.ValidateInt("ShippingCalculationID", sShippingCalculationID);
            int ShipAddresses = IV.ValidateInt("ShipAddresses", sShipAddresses);
            bool IsAllDownloadComponents = IV.ValidateBool("IsAllDownloadComponents", sIsAllDownloadComponents);
            bool IsAllFreeShippingComponents = IV.ValidateBool("sIsAllFreeShippingComponents", sIsAllFreeShippingComponents);
            bool IsAllSystemComponents = IV.ValidateBool("IsAllSystemComponents", sIsAllSystemComponents);
            string Locale = Localization.GetDefaultLocale();

            StringBuilder results = new StringBuilder("");

            if (!AppLogic.AppConfigBool("SkipShippingOnCheckout"))
            {
                results.Append("<tr>");
                String ShowShipText = CommonLogic.IIF(ShipAddresses > 1, String.Empty, ShippingMethod);
                // strip out RT shipping cost, if any:
                if (IsAllDownloadComponents || IsAllSystemComponents)
                {
                    ShowShipText = AppLogic.GetString("order.cs.1", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                else if (IsAllFreeShippingComponents && !AppLogic.AppConfigBool("FreeShippingAllowsRateSelection"))
                {
                    ShowShipText = AppLogic.GetString("order.cs.1", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }
                else if (ShowShipText.IndexOf("|") != -1)
                {
                    String[] ss2 = ShowShipText.Split('|');
                    try
                    {
                        ShowShipText = ss2[0].Trim();
                    }
                    catch
                    {
                    }
                }
                if (ShippingCalculationID == 4)
                {
                    ShowShipText = AppLogic.GetString("order.cs.2", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }

                if (ShowShipText.Length != 0)
                {
                    results.Append("<td align=\"right\" valign=\"top\" class=\"OrderSummaryLabel\">" + AppLogic.GetString("order.cs.55", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + " (" + ShowShipText + "):</td>");
                }
                else
                {
                    results.Append("<td align=\"right\" valign=\"top\" class=\"OrderSummaryLabel\">" + AppLogic.GetString("order.cs.55", ThisCustomer.SkinID, ThisCustomer.LocaleSetting) + ":</td>");
                }
                string st = AppLogic.GetString("order.cs.3", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                if (PaymentMethod.Equals("REQUEST QUOTE", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    //in case free shipping allows rate selection and they choose the free one
                    if (ShippingTotal == 0.0M)
                    {
                        ShowShipText = AppLogic.GetString("order.cs.1", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                    st = FormatCurrencyHelper(ShippingTotal, ThisCustomer.CurrencySetting);
                }
                results.Append("<td align=\"right\" valign=\"top\" class=\"OrderSummaryValue\">" + st + "</td>");
                results.Append("</tr>");
            }
            return results.ToString();
        }

        public virtual string GetMultiVariantPayPalAd(string sProductId)
        {
            int productId = 0;
            int.TryParse(sProductId, out productId);

            int variantId = AppLogic.GetDefaultProductVariant(productId);

            if (AppLogic.GetNextVariant(productId, variantId) != variantId) // multi variant product
            {
                PayPalAd productPageAd = new PayPalAd(PayPalAd.TargetPage.Product);
                if (productPageAd.Show)
                    return productPageAd.ImageScript;
            }
            return string.Empty;
        }

        public bool FileExists(String sFNOrUrl)
        {
            InputValidator IV = new InputValidator("FileExists");
            String FNOrUrl = IV.ValidateString("FNOrUrl", sFNOrUrl);
            // Name can be relative URL or physical file path!
            return CommonLogic.FileExists(CommonLogic.SafeMapPath(FNOrUrl));
        }

        // returns Payment Method cleaned (no spaces, or weird chars, all uppercased)
        public virtual string CleanPaymentMethod(String sPM)
        {
            InputValidator IV = new InputValidator("CleanPaymentMethod");
            String PM = IV.ValidateString("PM", sPM);
            return AppLogic.CleanPaymentMethod(PM);
        }

        // returns Payment Gateway cleaned (no spaces, or weird chars, all uppercased)
        public virtual string CleanPaymentGateway(String sGW)
        {
            InputValidator IV = new InputValidator("CleanPaymentGateway");
            String GW = IV.ValidateString("GW", sGW);
            return AppLogic.CleanPaymentGateway(GW);
        }

        // returns lowercase of string, invariant culture
        public virtual string StrToLower(String sS)
        {
            InputValidator IV = new InputValidator("StrToLower");
            String S = IV.ValidateString("S", sS);
            return S.ToLower(CultureInfo.InvariantCulture);
        }

        // returns uppercase of string, invariant culture
        public virtual string StrToUpper(String sS)
        {
            InputValidator IV = new InputValidator("StrToUpper");
            String S = IV.ValidateString("S", sS);
            return S.ToUpper(CultureInfo.InvariantCulture);
        }

        // returns capitalize of string, invariant culture
        public virtual string StrCapitalize(String sS)
        {
            InputValidator IV = new InputValidator("StrCapitalize");
            String S = IV.ValidateString("S", sS);
            return CommonLogic.Capitalize(S);
        }

        // returns trim of string
        public virtual string StrTrim(String sS)
        {
            InputValidator IV = new InputValidator("StrTrim");
            String S = IV.ValidateString("S", sS);
            return S.Trim();
        }

        // returns trim start of string
        public virtual string StrTrimStart(String sS)
        {
            InputValidator IV = new InputValidator("StrTrimStart");
            String S = IV.ValidateString("S", sS);
            return S.TrimStart();
        }

        // returns trim end of string
        public virtual string StrTrimEnd(String sS)
        {
            InputValidator IV = new InputValidator("StrTrimEnd");
            String S = IV.ValidateString("S", sS);
            return S.TrimEnd();
        }

        // returns string replace
        public virtual string StrReplace(String sS, String sOldValue, String sNewValue)
        {
            InputValidator IV = new InputValidator("StrReplace");
            String S = IV.ValidateString("S", sS);
            String OldValue = IV.ValidateString("OldValue", sOldValue);
            String NewValue = IV.ValidateString("NewValue", sNewValue);
            return S.Replace(OldValue, NewValue);
        }

        // returns string in "plural" form (this is almost impossible to do)!
        public virtual string StrMakePlural(String sS)
        {
            InputValidator IV = new InputValidator("StrMakePlural");
            String S = IV.ValidateString("S", sS);
            return S;
        }

        // returns string in "singular" form (this is almost impossible to do)!
        public virtual string StrMakeSingular(String sS)
        {
            InputValidator IV = new InputValidator("StrMakeSingular");
            String S = IV.ValidateString("S", sS);
            return S;
        }

        //splits a string and puts it inside tags using the specified TagName, e.g. <TagName>value1</TagName><TagName>value2</TagName>..., the valuex will be XML Encoded
        public virtual string SplitString(string sS, string sDelimiter, string sTagName)
        {
            InputValidator IV = new InputValidator("SplitString");
            String S = IV.ValidateString("S", sS);
            String Delimiter = IV.ValidateString("Delimiter", sDelimiter);
            String TagName = IV.ValidateString("TagName", sTagName);
            if (S.Trim().Length == 0)
            {
                return "";
            }

            string tagStart = string.Empty;
            string tagEnd = string.Empty;
            if (TagName.Trim().Length > 0)
            {
                tagStart = "<" + TagName.Trim() + ">";
                tagEnd = "</" + TagName.Trim() + ">";
            }
            StringBuilder tmpS = new StringBuilder();
            char[] delim = Delimiter.ToCharArray();
            foreach (string sv in S.Split(delim))
            {
                tmpS.Append(tagStart + XmlCommon.XmlEncode(sv) + tagEnd);
            }
            return tmpS.ToString();
        }

        public virtual string InStr(string strSource, string strFind)
        {
            return strSource.IndexOf(strFind).ToString();
        }

        public virtual string GiftCardKey()
        {
            InputValidator IV = new InputValidator("GiftCardKey");
            return GiftCard.s_GiftCardKey(ThisCustomer.CustomerID);
        }

        public virtual string MicroPayBalance()
        {
            Decimal mpd = AppLogic.GetMicroPayBalance(ThisCustomer.CustomerID);
            return FormatCurrencyHelper(mpd, ThisCustomer.CurrencySetting);
        }

        public virtual string StrFormat(string sSrcString, string sFormatParams, string sDelimiter)
        {
            InputValidator IV = new InputValidator("StrFormat");
            String SrcString = IV.ValidateString("SrcString", sSrcString);
            String FormatParams = IV.ValidateString("FormatParams", sFormatParams);
            String Delimiter = IV.ValidateString("Delimiter", sDelimiter);
            char[] delim = Delimiter.ToCharArray();
            string[] rParams = FormatParams.Split(delim);
            return String.Format(SrcString, FormatParams);
        }

        public virtual string ReadFile(string sFName)
        {
            InputValidator IV = new InputValidator("ReadFile");
            String FName = IV.ValidateString("FName", sFName);
            return CommonLogic.ReadFile(FName, true);
        }

        public virtual string LocateImageURL(string sImgUrl)
        {
            InputValidator IV = new InputValidator("LocateImageURL");
            String ImgUrl = IV.ValidateString("ImgUrl", sImgUrl);
            return AppLogic.LocateImageURL(ImgUrl);
        }

        public virtual string LocateImageURL(string sImgUrl, string sLocale)
        {
            InputValidator IV = new InputValidator("LocateImageURL");
            String ImgUrl = IV.ValidateString("ImgUrl", sImgUrl);
            String Locale = IV.ValidateString("Locale", sLocale);
            return AppLogic.LocateImageURL(ImgUrl, Locale);
        }

        public virtual string GetNativeShortDateString(String sDateTimeString)
        {
            InputValidator IV = new InputValidator("GetNativeShortDateString");
            DateTime dt = IV.ValidateDateTime("DateTimeString", sDateTimeString);
            return Localization.ToThreadCultureShortDateString(dt);
        }

        public virtual string GetLocaleShortDateString(String sDateTimeString)
        {
            InputValidator IV = new InputValidator("GetLocaleShortDateString");
            DateTime dt = IV.ValidateDateTime("DateTimeString", sDateTimeString);
            return Localization.ParseLocaleDateTime(sDateTimeString, ThisCustomer.LocaleSetting).ToShortDateString();
        }

        [Obsolete("Replace by user control PollControl in AspDotNetStorefrontControls.")]
        public virtual string GetPollBox(String sPollID, String sLarge, string sCategory, string sSection)
        {
            return "<!--The XSLTExtensionBase function GetPollBox no longer exists. Please replace with the user control PollControl in AspDotNetStorefrontControls.-->";
        }

        public virtual string GetRatingStarsImage(String sRating)
        {
            InputValidator IV = new InputValidator("GetRatingStarsImage");
            Decimal Rating = IV.ValidateDecimal("Rating", sRating);
            return CommonLogic.BuildStarsImage(Rating, ThisCustomer.SkinID);
        }

        public virtual string DisplayAddressString(String sAddressID)
        {
            InputValidator IV = new InputValidator("DisplayAddressString");
            int AddressID = IV.ValidateInt("AddressID", sAddressID);
            StringBuilder results = new StringBuilder("");
            Address adr = new Address();
            adr.LoadFromDB(AddressID);
            results.Append("<div style=\"margin-left: 10px;\">");
            results.Append(adr.DisplayHTML(false));
            results.Append("</div>");
            return results.ToString();
        }

        /// <summary>
        /// Parses incoming date string DateString and reformats it to a new date string according to the FormatString parameter
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="strFmt"></param>
        /// <returns></returns>
        public virtual string FormatDate(string strDate, string sSourceLocale, string strFmt)
        {
            InputValidator IV = new InputValidator("FormatDate");
            string sDate = IV.ValidateString("DateString", strDate);
            string SourceLocale = IV.ValidateString("SourceLocale", sSourceLocale);
            string sFormat = IV.ValidateString("FormatString", strFmt);
            try
            {
                DateTime dt = Localization.ParseLocaleDateTime(strDate, SourceLocale);
                return dt.ToString(sFormat);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public virtual string GetLocalizedShortDate(string strDate, string sSourceLocale, string sTargetLocale)
        {
            if (string.IsNullOrEmpty(sTargetLocale))
                sTargetLocale = Localization.GetDefaultLocale();

            InputValidator IV = new InputValidator("FormatDate");
            string sDate = IV.ValidateString("DateString", strDate);
            string SourceLocale = IV.ValidateString("SourceLocale", sSourceLocale);
            string TargetLocale = IV.ValidateString("TargetLocale", sTargetLocale);
            try
            {
                DateTime dt = Localization.ParseLocaleDateTime(strDate, SourceLocale);
                CultureInfo ci = new CultureInfo(TargetLocale);

                return dt.ToString("d", ci);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // returns:
        // if Date1 < Date2 => -1
        // if Date1 = Date2 => 0
        // if Date1 > Date2 -> 1
        // if Date1 or Date2 is empty string, they will be set to "now"
        public virtual String DateCompare(String sDate1, String sDate2)
        {
            InputValidator IV = new InputValidator("DateCompare");
            String Date1 = IV.ValidateString("Date1", sDate1);
            String Date2 = IV.ValidateString("Date2", sDate2);
            try
            {
                DateTime d1 = DateTime.Now;
                DateTime d2 = DateTime.Now;
                if (!Date1.Trim().Length.Equals(0))
                {
                    d1 = Localization.ParseNativeDateTime(Date1);
                }
                if (!Date2.Trim().Length.Equals(0))
                {
                    d2 = Localization.ParseNativeDateTime(Date2);
                }
                if (d1 < d2)
                {
                    return "-1";
                }
                if (d1.Equals(d2))
                {
                    return "0";
                }
                if (d1 > d2)
                {
                    return "1";
                }
            }
            catch
            {
            }
            return "0";
        }

        public virtual string FormatDecimal(string sDecimalValue, string intFixPlaces)
        {
            InputValidator IV = new InputValidator("FormatDecimal");
            Decimal DecimalValue = IV.ValidateDecimal("DecimalValue", sDecimalValue);
            DecimalValue = Localization.ParseDBDecimal(sDecimalValue);
            int FixPlaces = IV.ValidateInt("intFixPlaces", intFixPlaces);
            DecimalValue = Decimal.Round(DecimalValue, FixPlaces, MidpointRounding.AwayFromZero);
            return DecimalValue.ToString(new CultureInfo(ThisCustomer.LocaleSetting));
        }

        public virtual string GetSplitString(string sStringToSplit, string sSplitChar, string intReturnIndex)
        {
            InputValidator IV = new InputValidator("GetSplitString");
            String StringToSplit = IV.ValidateString("StringToSplit", sStringToSplit);
            String SplitChar = IV.ValidateString("SplitChar", sSplitChar);
            int ReturnIndex = IV.ValidateInt("ReturnIndex", intReturnIndex);
            if (SplitChar.Length == 0)
            {
                SplitChar = ",";
            }
            String[] s = StringToSplit.Split(SplitChar.ToCharArray());
            String tmp = String.Empty;
            try
            {
                tmp = s[ReturnIndex];
            }
            catch
            {
            }
            return tmp;
        }

        public virtual string ConvertToBase64(string input)
        {
            if (input == null)
            {
                input = String.Empty;
            }
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(input);
            string base64Encoded = Convert.ToBase64String(bytes);
            return base64Encoded;
        }

        public virtual Boolean IsEmailGiftCard(int ProductID)
        {
            if (GiftCard.s_IsEmailGiftCard(ProductID))
            {
                return true;
            }
            return false;
        }

        public virtual string GetNewKitPrice(String sProductID, string sPrice, string sSalePrice, string sExtendedPrice, String sHidePriceUntilCart, string sColors)
        {
            InputValidator IV = new InputValidator("GetKitPrice");
            decimal Price = Localization.ParseDBDecimal(sPrice);
            Decimal SalePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
            int TaxClassID = DB.GetSqlN("select TaxClassID from dbo.product where productid = " + sProductID);
            return GetKitPrice(sProductID, sPrice, sSalePrice, sExtendedPrice, sHidePriceUntilCart, sColors, "FALSE", TaxClassID.ToString());
        }

        public virtual string GetNewKitPrice(String sProductID, string sPrice, string sSalePrice, string sExtendedPrice, String sHidePriceUntilCart, string sColors, String sShoppingCartRecID, String intTaxClassID)
        {
            InputValidator IV = new InputValidator("GetKitPrice");
            int ProductID = IV.ValidateInt("ProductID", sProductID);
            bool HidePriceUntilCart = IV.ValidateBool("HidePriceUntilCart", sHidePriceUntilCart);
            int ShoppingCartRecID = IV.ValidateInt("ShoppingCartRecID", sShoppingCartRecID);
            Decimal Price = IV.ValidateDecimal("Price", sPrice);
            Decimal SalePrice = IV.ValidateDecimal("SalePrice", sSalePrice);
            Decimal ExtendedPrice = IV.ValidateDecimal("ExtendedPrice", sExtendedPrice);
            String Colors = IV.ValidateString("Colors", sColors);
            int TaxClassID = IV.ValidateInt("TaxClassID", intTaxClassID);

            Decimal TaxRate = 0.0M;
            Decimal TaxMultiplier = 1.0M;
            if (m_VATOn)
            {
                TaxRate = Prices.TaxRate(ThisCustomer, TaxClassID);
                TaxMultiplier = (1.0M + (TaxRate / 100.00M));

            }

            decimal BasePrice = Decimal.Zero;
            if (SalePrice != Decimal.Zero)
            {
                BasePrice = SalePrice;
            }
            else
            {
                BasePrice = Price;
            }

            StringBuilder output = new StringBuilder();

            output.AppendFormat("<div id=\"KitPriceMain_{0}\" {1}>\n", ProductID, CommonLogic.IIF(AppLogic.AppConfigBool("HideKitPrice"), string.Empty, "style=\"display:none\""));
            output.Append(AppLogic.GetString("showproduct.aspx.12", ThisCustomer.SkinID, ThisCustomer.LocaleSetting));
            output.Append("");

            if (!AppLogic.AppConfigBool("HideKitPrice"))
            {
                output.AppendFormat("<div id=\"KitPrice_{0}\" >\n", ProductID);
                output.AppendFormat("    <div><span id=\"KitPrice_{0}_RegularBasePrice\" class=\"KitRegularBasePrice\" ></span></div>\n", ProductID);
                output.AppendFormat("    <div><span id=\"KitPrice_{0}_BasePrice\" class=\"KitBasePrice\" ></span></div>\n", ProductID);
                output.AppendFormat("    <div><span id=\"KitPrice_{0}_CustomizedPrice\" class=\"KitCustomizedPrice\" ></span></div>\n", ProductID);
                output.AppendFormat("    <div><span id=\"KitPrice_{0}_LevelPrice\" style=\"display:none;\" class=\"KitLevelPrice\" ></span></div>\n", ProductID);
                output.Append("</div>\n");

                StringBuilder script = new StringBuilder();
                script.Append("<script type=\"text/javascript\" src=\"jscripts/prototype.js\" ></script>\n");
                script.Append("<script type=\"text/javascript\" src=\"jscripts/numberFormat154.js\" ></script>\n");
                script.Append("<script type=\"text/javascript\" src=\"jscripts/kitproduct.js\" ></script>\n");
                // javascript section
                script.Append("<script type=\"text/javascript\" Language=\"JavaScript\">\n");

                script.Append("Event.observe(window, 'load', \n");
                script.Append(" function() { \n");


                script.AppendFormat("    var ctrlPrice = new aspdnsf.Controls.KitPriceControl({0}, 'KitPrice_{0}', {1});\n", ProductID, BasePrice);

                if (ThisCustomer.CustomerLevelID != 0)
                {
                    script.AppendFormat("    ctrlPrice.setCustomerLevel('{0}');\n", ThisCustomer.CustomerLevelName);
                }
                if (AppLogic.AppConfigBool("VAT.Enabled"))
                {
                    script.AppendFormat("    ctrlPrice.setIsVatEnabled({0})\n", AppLogic.AppConfigBool("VAT.Enabled").ToString().ToLowerInvariant());
                    script.AppendFormat("    ctrlPrice.setIsVatInclusive({0})\n", m_VATOn.ToString().ToLowerInvariant());
                }
                script.AppendFormat("    ctrlPrice.addDisplayEventHandler(function(){{ $('KitPriceMain_{0}').style.display=\"\"; }});\n", ProductID);
                script.AppendFormat("    var kitControl = aspdnsf.Controls.KitController.getControl({0});\n", ProductID);
                script.AppendFormat("    ctrlPrice.setKitControl(kitControl);\n");

                script.Append(" }\n");
                script.Append(");\n");
                script.Append("</script>\n");

                output.Append(script.ToString());
            }

            string addToCartForm = ShoppingCart.GetAddToCartForm(
                        ThisCustomer, false, true,
                        AppLogic.AppConfigBool("ShowWishButtons"),
                        AppLogic.AppConfigBool("ShowGiftRegistryButtons"),
                        ProductID,
                        AppLogic.GetDefaultProductVariant(ProductID),
                        ThisCustomer.SkinID, 1, ThisCustomer.LocaleSetting,
                        false, VariantStyleEnum.RegularVariantsWithAttributes,
                        true, false,
                        "Price");

            output.Append("");
            output.Append(addToCartForm);
            output.Append("");
            output.Append("</div>\n");

            return output.ToString();
        }

        public virtual string GetNewKitItemOptions(String sProductID, string sPrice, string sSalePrice, string sExtendedPrice, String sHidePriceUntilCart, string sColors, String sShoppingCartRecID, String intTaxClassID)
        {
            return string.Empty;
        }

        /// <summary>
        /// Returns the content of CSS file based on skin ID
        /// </summary>
        /// <param name="SkinID">Skin ID</param>
        /// <returns></returns>
        public virtual string GetReceiptCss(int SkinID)
        {
            StringBuilder s = new StringBuilder();
            s.Append("\n<style type=\"text/css\">\n");
            s.Append(CommonLogic.ReadFile("~/App_Templates/Skin_" + SkinID + "/receipt.css", true).ToString());
            s.Append("\n</style>\n");
            return s.ToString();
        }

        public IXPathNavigable XmlPackageAsXml(string packageName)
        {
            return XmlPackageAsXml(packageName, string.Empty);
        }

        public IXPathNavigable XmlPackageAsXml(string packageName, string runtimeParams)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                string result = AppLogic.RunXmlPackage(packageName, null, ThisCustomer, ThisCustomer.SkinID, string.Empty, runtimeParams, false, false);
                doc.LoadXml(result);
                return doc;
            }
            catch (Exception e)
            {
                String errorXML = String.Format("<error>{0}</error>", HtmlEncode(e.Message));
                doc.LoadXml(errorXML);
                return doc;
            }
        }

        public IXPathNavigable XmlStringAsXml(string xml)
        {
            return XmlStringAsXml(xml, string.Empty);
        }

        public IXPathNavigable XmlStringAsXml(string xml, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            if (!string.IsNullOrEmpty(xpath))
            {
                return doc.SelectSingleNode(xpath);
            }
            else
            {
                return doc;
            }
        }

        public IXPathNavigable SelectElementsFromIDDelimitedString(string delimitedString, string delimiter, XPathNodeIterator selection, string element)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode rootNode = doc.CreateNode(XmlNodeType.Element, "root", string.Empty);

            string[] values = delimitedString.Split(delimiter.ToCharArray());

            // let's perform linear searching
            while (selection.MoveNext())
            {
                XmlNode itemNode = (selection.Current as IHasXmlNode).GetNode();
                foreach (string value in values)
                {
                    if (itemNode[element].InnerText == value)
                    {
                        XmlNode copyNode = doc.ImportNode(itemNode, true);
                        rootNode.AppendChild(copyNode);
                        break;
                    }
                }
            }

            return rootNode;
        }

        public IXPathNavigable CreateXmlFromDelimitedString(string delimitedString, string delimiter, string rootName, string elementName)
        {
            XmlDocument doc = new XmlDocument();

            XmlNode rootNode = doc.CreateNode(XmlNodeType.Element, rootName, string.Empty);
            doc.AppendChild(rootNode);

            string[] values = delimitedString.Split(delimiter.ToCharArray());

            foreach (string value in values)
            {
                XmlNode elementNode = doc.CreateNode(XmlNodeType.Element, elementName, string.Empty);
                elementNode.InnerText = value;

                rootNode.AppendChild(elementNode);
            }

            return rootNode;
        }

        public IXPathNavigable OrderOptionsAsXml(string strOrderOptions)
        {
            InputValidator validator = new InputValidator("OrderOptionsAsXml");
            string orderOptions = validator.ValidateString("OrderOptions", strOrderOptions);

            XmlDocument doc = new XmlDocument();
            XmlNode orderOptionNodes = doc.CreateNode(XmlNodeType.Element, "OrderOptions", string.Empty);

            if (!string.IsNullOrEmpty(orderOptions))
            {
                string[] orderOptionDelimitedValues = orderOptions.Split('^');
                foreach (string orderOptionDelimitedValue in orderOptionDelimitedValues)
                {
                    string[] orderOptionValues = orderOptionDelimitedValue.Split('|');
                    if (orderOptionValues != null && orderOptionValues.Length > 0)
                    {
                        int id = int.Parse(orderOptionValues[0]);
                        string name = orderOptionValues[1];

                        // NOTE :
                        //  These fields are already Currency Formatted!!!
                        string priceFormatted = orderOptionValues[2];
                        string extPriceFormatted = priceFormatted;

                        bool withVat = orderOptionValues.Length >= 3;
                        string vat = string.Empty;
                        if (withVat)
                        {
                            vat = orderOptionValues[3];
                        }

                        XmlNode orderOptionNode = doc.CreateNode(XmlNodeType.Element, "OrderOption", string.Empty);

                        // the details
                        XmlNode idNode = doc.CreateNode(XmlNodeType.Element, "ID", string.Empty);
                        XmlNode nameNode = doc.CreateNode(XmlNodeType.Element, "ProductName", string.Empty);
                        XmlNode priceNode = doc.CreateNode(XmlNodeType.Element, "Price", string.Empty);
                        XmlNode vatNode = doc.CreateNode(XmlNodeType.Element, "VAT", string.Empty);
                        XmlNode imageUrlNode = doc.CreateNode(XmlNodeType.Element, "ImageUrl", string.Empty);

                        idNode.InnerText = id.ToString();
                        nameNode.InnerXml = name; // NOTE: this value may be localized, make sure to call GetMLValue on the xml package!!!
                        priceNode.InnerText = priceFormatted;
                        vatNode.InnerText = vat;

                        // get the image info
                        string imgUrl = AppLogic.LookupImage("OrderOption", id, "icon", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        if (imgUrl.StartsWith("../"))
                        {
                            imgUrl = imgUrl.Replace("../", "");
                        }
                        imgUrl = AppLogic.GetStoreHTTPLocation(true) + imgUrl;
                        imageUrlNode.InnerText = imgUrl;

                        orderOptionNode.AppendChild(idNode);
                        orderOptionNode.AppendChild(nameNode);
                        orderOptionNode.AppendChild(priceNode);
                        orderOptionNode.AppendChild(vatNode);
                        orderOptionNode.AppendChild(imageUrlNode);

                        orderOptionNodes.AppendChild(orderOptionNode);
                    }
                }
            }

            doc.AppendChild(orderOptionNodes);

            return doc;
        }

        public string CleanShippingMethod(string shippingMethod)
        {
            string cleanedShippingMethod = shippingMethod;

            // in case of RT Shipping
            if (shippingMethod.IndexOf('|') > -1)
            {
                string[] split = shippingMethod.Split('|');
                if (split.Length > 0)
                {
                    cleanedShippingMethod = split[0];
                }
            }

            return cleanedShippingMethod;
        }

        public bool IsStringEmpty(string stringValue)
        {
            return string.IsNullOrEmpty(stringValue) || stringValue.Trim().Length == 0;
        }

        /// <summary>
        /// Gets the relative image path from the App_Themes/Skin_{current} directory for the specified image
        /// </summary>
        /// <param name="fileName">The image file name</param>
        /// <returns>The relative path from the themes/image directory</returns>
        public string SkinImage(string fileName)
        {
            return AppLogic.SkinImage(fileName);
        }

        public string ProcessXsltExtensionArgs(params object[] args)
        {
            return args[0].ToString();
        }

        public string ProcessXsltExtension(object args)
        {
            return args.ToString();
        }

        public string ProcessXsltExtensionX(IXPathNavigable navi)
        {
            var nav = navi.CreateNavigator();
            return nav.ToString();
        }

        public string GetCurrentProtocol()
        {
            string protocol = "http";
            if (HttpContext.Current.Request.IsSecureConnection)
            {
                protocol = "https";
            }
            return protocol;
        }

        #region PRODUCT HELPER METHODS

        /// <summary>
        /// Breaks up the dimensions value and returns a single desired dimension
        /// </summary>
        /// <param name="Dimensions"></param>
        /// <param name="DesiredDimension"></param>
        /// <returns></returns>
        public string RetrieveDimension(string Dimensions, string DesiredDimension)
        {
            string trimmedDimensions = Dimensions.ToLower().Trim();
            string pattern = "^(((\\+?\\d+)|(.\\+?\\d+)|(\\+?\\d+.\\+?\\d+))\\s*x\\s*((\\+?\\d+)|(.\\+?\\d+)|(\\+?\\d+.\\+?\\d+))\\s*x\\s*((\\+?\\d+)|(.\\+?\\d+)|(\\+?\\d+.\\+?\\d+)))$";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            Match isMatch = rgx.Match(trimmedDimensions);

            if (isMatch.Success)
            {
                string[] splitDimensions = trimmedDimensions.Split('x');

                switch (DesiredDimension.ToLower())
                {
                    case "width":
                        return splitDimensions[0].Trim();
                    case "height":
                        return splitDimensions[1].Trim();
                    case "depth":
                        return splitDimensions[2].Trim();
                    default:
                        return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates the weight text is either N|.N|N.N
        /// </summary>
        /// <param name="weightValue"></param>
        /// <returns></returns>
        public string ValidateWeight(string weightValue)
        {
            string trimmedWeight = weightValue.ToLower().Trim();
            string pattern = "^((\\+?\\d+)|(.\\+?\\d+)|(\\+?\\d+.\\+?\\d+))$";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            Match isMatch = rgx.Match(trimmedWeight);

            if (isMatch.Success)
            {
                return trimmedWeight;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// validates and formats GTIN
        /// </summary>
        /// <param name="GTINValue"></param>
        /// <returns></returns>
        public string ValidateGTIN(string GTINValue)
        {
            string trimmedGTIN = GTINValue.ToLower().Trim();
            string pattern = "^(\\+?\\d+)$";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            Match isMatch = rgx.Match(trimmedGTIN);

            if (isMatch.Success)
            {
                switch (trimmedGTIN.Length)
                {
                    case 8:
                        return string.Format("gtin8|{0}", trimmedGTIN);
                    case 12:
                        return string.Format("gtin13|0{0}", trimmedGTIN);
                    case 13:
                        return string.Format("gtin13|{0}", trimmedGTIN);
                    case 14:
                        return string.Format("gtin14|{0}", trimmedGTIN);
                    default:
                        return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the product condition text by it's value
        /// </summary>
        /// <param name="conditionValue"></param>
        /// <returns></returns>
        public string RetrieveProductConditionText(string conditionValue)
        {
            int conditionInt = 0;
            bool isNumber = int.TryParse(conditionValue, out conditionInt);
            if (isNumber)
            {
                return Enum.GetName(typeof(ProductCondition), conditionInt);
            }
            else
            {
                return Enum.GetName(typeof(ProductCondition), 0);
            }
        }

        /// <summary>
        /// Declares options for a product condition
        /// </summary>
        public enum ProductCondition
        {
            New = 0,
            Used = 1,
            Refurbished = 2
        }
        
        /// <summary>
        /// Gets the in stock/out of stock text
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="variantId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public string GetStockStatusText(string productId, string variantId, string page)
        {
            string stockText = string.Empty;
            int productInt = 0;
            int variantInt = 0;

            if (int.TryParse(productId, out productInt) && productInt > 0)
            {
                if (int.TryParse(variantId, out variantInt) && variantInt > 0)
                {
                    bool trackInventoryBySizeAndColor = AppLogic.ProductTracksInventoryBySizeAndColor(productInt);
                    bool outOfStock = AppLogic.ProbablyOutOfStock(productInt, variantInt, trackInventoryBySizeAndColor, page);
                    if (outOfStock)
                    {
                        stockText = "OutOfStock|Out Of Stock";
                    }
                    else
                    {
                        stockText = "InStock|In Stock";
                    }
                }                
            }
            return stockText;
        }

        /// <summary>
        /// Converts a date time formatted string to be iso 8601 compliant
        /// </summary>
        /// <param name="DateTimeInput"></param>
        /// <returns></returns>
        public string GetISODateTime(string DateTimeInput)
        {
            string ISODateTimeOutput = string.Empty;
            DateTime dateValue;

            if (DateTime.TryParse(DateTimeInput, out dateValue))
            {
                ISODateTimeOutput = string.Format("{0}{1}", dateValue.ToString("s"), dateValue.ToString("zzz"));
            }
            else
            {
                ISODateTimeOutput = string.Empty;
            }      
            return ISODateTimeOutput;
        }

        #endregion        
    }
}
