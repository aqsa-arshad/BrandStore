// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Xml;

namespace AspDotNetStorefrontAdmin
{
    public enum ImportOption
    {
        Default = 0,
        LeaveModified = 1,
        OverWrite = 2
    }
    /// <summary>
    /// Summary description for AdminLogic.
    /// </summary>
    public class AdminLogic
    {

        public AdminLogic()
        { }

        static public String HandleImageSubmits(EntitySpecs specs, int EntityID)
        {
            // handle image uploaded:
            String FN = String.Empty;
            if (AppLogic.AppConfigBool("UseSKUFor" + specs.m_ObjectName + "ImageName"))
            {
                FN = CommonLogic.FormCanBeDangerousContent("SKU").Trim();
            }
            if (CommonLogic.FormCanBeDangerousContent("ImageFilenameOverride").Trim().Length != 0)
            {
                FN = CommonLogic.FormCanBeDangerousContent("ImageFilenameOverride").Trim();
            }
            if (FN.Length == 0)
            {
                FN = EntityID.ToString();
            }
            String ErrorMsg = String.Empty;

            if (specs.m_HasIconPic)
            {
                String Image1 = String.Empty;
                HttpPostedFile Image1File = HttpContext.Current.Request.Files["Image1"];
                if (Image1File != null && Image1File.ContentLength != 0)
                {
                    // delete any current image file first
                    try
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                            {
                                System.IO.File.Delete(AppLogic.GetImagePath(specs.m_ObjectName, "icon", true) + FN);
                            }
                            else
                            {
                                System.IO.File.Delete(AppLogic.GetImagePath(specs.m_EntityName, "icon", true) + FN + ss);
                            }
                        }
                    }
                    catch
                    { }

                    String s = Image1File.ContentType;
                    switch (Image1File.ContentType)
                    {
                        case "image/gif":
                            Image1 = AppLogic.GetImagePath(specs.m_EntityName, "icon", true) + FN + ".gif";
                            Image1File.SaveAs(Image1);
                            break;
                        case "image/x-png":
                            Image1 = AppLogic.GetImagePath(specs.m_EntityName, "icon", true) + FN + ".png";
                            Image1File.SaveAs(Image1);
                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/pjpeg":
                            Image1 = AppLogic.GetImagePath(specs.m_EntityName, "icon", true) + FN + ".jpg";
                            Image1File.SaveAs(Image1);
                            break;
                    }
                }
            }

            if (specs.m_HasMediumPic)
            {
                String Image2 = String.Empty;
                HttpPostedFile Image2File = HttpContext.Current.Request.Files["Image2"];
                if (Image2File != null && Image2File.ContentLength != 0)
                {
                    // delete any current image file first
                    try
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath(specs.m_EntityName, "medium", true) + FN + ss);
                        }
                    }
                    catch
                    { }

                    String s = Image2File.ContentType;
                    switch (Image2File.ContentType)
                    {
                        case "image/gif":
                            Image2 = AppLogic.GetImagePath(specs.m_EntityName, "medium", true) + FN + ".gif";
                            Image2File.SaveAs(Image2);
                            break;
                        case "image/x-png":
                            Image2 = AppLogic.GetImagePath(specs.m_EntityName, "medium", true) + FN + ".png";
                            Image2File.SaveAs(Image2);
                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/pjpeg":
                            Image2 = AppLogic.GetImagePath(specs.m_EntityName, "medium", true) + FN + ".jpg";
                            Image2File.SaveAs(Image2);
                            break;
                    }
                }
            }

            if (specs.m_HasLargePic)
            {
                String Image3 = String.Empty;
                HttpPostedFile Image3File = HttpContext.Current.Request.Files["Image3"];
                if (Image3File != null && Image3File.ContentLength != 0)
                {
                    // delete any current image file first
                    try
                    {
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath(specs.m_EntityName, "large", true) + FN + ss);
                        }
                    }
                    catch
                    { }

                    String s = Image3File.ContentType;
                    switch (Image3File.ContentType)
                    {
                        case "image/gif":
                            Image3 = AppLogic.GetImagePath(specs.m_EntityName, "large", true) + FN + ".gif";
                            Image3File.SaveAs(Image3);
                            break;
                        case "image/x-png":
                            Image3 = AppLogic.GetImagePath(specs.m_EntityName, "large", true) + FN + ".png";
                            Image3File.SaveAs(Image3);
                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/pjpeg":
                            Image3 = AppLogic.GetImagePath(specs.m_EntityName, "large", true) + FN + ".jpg";
                            Image3File.SaveAs(Image3);
                            break;
                    }
                }
            }

            return ErrorMsg;
        }

        static public String EntityEditPageRender(AspDotNetStorefront.SkinBase sb, EntitySpecs specs, Customer ThisCustomer, int SkinID, String ErrorMsg)
        {
            int EntityID = 0;
            bool Editing = false;
            if (CommonLogic.QueryStringCanBeDangerousContent("EntityID").Length != 0 && CommonLogic.QueryStringCanBeDangerousContent("EntityID") != "0")
            {
                Editing = true;
                EntityID = CommonLogic.QueryStringUSInt("EntityID");
            }

            StringBuilder tmpS = new StringBuilder(50000);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select * from " + specs.m_EntityName + "  with (NOLOCK)  where " + specs.m_EntityName + "ID=" + EntityID.ToString(), con))
                {
                    if (rs.Read())
                    {
                        Editing = true;
                    }

                    tmpS.Append("<form enctype=\"multipart/form-data\" action=\"editentity.aspx?entityname=" + specs.m_EntityName + "&entityID=" + EntityID.ToString() + "&edit=" + Editing.ToString() + "\" method=\"post\" id=\"Form1\" name=\"Form1\" onsubmit=\"return (validateForm(document.forms[0]))\" onReset=\"return confirm('Do you want to reset all fields to their starting values?');\">\n");

                    if (ErrorMsg.Length != 0)
                    {
                        tmpS.Append("<p><b><font color=red>" + ErrorMsg + "</font></b></p>\n");
                    }
                    if (CommonLogic.FormCanBeDangerousContent("IsSubmit").Length != 0)
                    {
                        tmpS.Append("<p align=\"left\"><b><font color=blue>(UPDATED)</font></b></p>\n");
                    }


                    EntityHelper EntityHelper = AppLogic.LookupHelper(sb.EntityHelpers, specs.m_EntityName);

                    if (ErrorMsg.Length == 0)
                    {

                        if (Editing && EntityID != 0)
                        {
                            tmpS.Append("<p><b>Editing " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + ": " + DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting) + " (ID=" + EntityID.ToString() + ")</b>\n");
                            XmlNode n = EntityHelper.m_TblMgr.SetContext(EntityID);
                            int NumSiblings = 0;
                            if (n != null)
                            {
                                NumSiblings = EntityHelper.m_TblMgr.NumSiblings(n);
                            }
                            tmpS.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                            if (NumSiblings > 1)
                            {
                                int PreviousID = EntityHelper.GetPreviousEntity(EntityID, true);
                                tmpS.Append("<a class=\"" + specs.m_ObjectName + "NavLink\" href=\"editentity.aspx?entityname=" + specs.m_EntityName + "&entityid=" + PreviousID.ToString() + "\">&lt;&lt;</a>&nbsp;|&nbsp;");
                            }
                            tmpS.Append("<a class=\"" + specs.m_ObjectName + "NavLink\" href=\"entities.aspx?entityname=" + specs.m_EntityName + "\">up</a>");
                            if (NumSiblings > 1)
                            {
                                int NextID = EntityHelper.GetNextEntity(EntityID, true);
                                tmpS.Append("&nbsp;|&nbsp;<a class=\"" + specs.m_ObjectName + "NavLink\" href=\"editentity.aspx?entityname=" + specs.m_EntityName + "&entityid=" + NextID.ToString() + "\">&gt;&gt;</a>&nbsp");
                            }
                            tmpS.Append("&nbsp;&nbsp;&nbsp;|&nbsp;<a class=\"" + specs.m_ObjectName + "NavLink\" href=\"" + specs.m_ObjectNamePlural + ".aspx?entityname=" + specs.m_EntityName + "&entityfilterid=" + EntityID.ToString() + "\">Show " + specs.m_ObjectNamePlural + "</a>");
                            tmpS.Append("&nbsp;&nbsp;&nbsp;|&nbsp;<a class=\"" + specs.m_ObjectName + "NavLink\" href=\"displayorder.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + EntityID.ToString() + "\">Set " + specs.m_ObjectName + " Display Orders</a>");
                            int numEntityObjects = EntityHelper.GetNumEntityObjects(EntityID, true, true);
                            int MaxBulkN = AppLogic.AppConfigUSInt("MaxBulkN");
                            if (MaxBulkN == 0)
                            {
                                MaxBulkN = 100; // default it
                            }
                            if (numEntityObjects > 0 && numEntityObjects <= MaxBulkN)
                            {
                                tmpS.Append("");
                                int ThisID = EntityID;
                                if (specs.m_ObjectName == "Product")
                                {
                                    tmpS.Append("Bulk Edit (All " + specs.m_ObjectNamePlural + " In " + specs.m_EntityName + "): ");
                                    tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"Inventory\" name=\"InventoryEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditinventory.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">&nbsp;");
                                    tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"SEFields\" name=\"SearchEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditsearch.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">&nbsp;");
                                    tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"Prices\" name=\"PricesEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditprices.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">&nbsp;");
                                    if (Shipping.GetActiveShippingCalculationID() == Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts)
                                    {
                                        tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"ShipCosts\" name=\"ShippingCostsEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditshippingcosts.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">&nbsp;");
                                    }
                                    tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"DownloadFiles\" name=\"DownloadFilesEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditdownloadfiles.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">");
                                }
                            }
                            tmpS.Append("");
                            tmpS.Append("</p>\n");
                        }
                        else
                        {
                            tmpS.Append("<b>Adding New " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + ":</b>\n");
                        }

                        tmpS.Append("<script type=\"text/javascript\">\n");
                        tmpS.Append("function Form_Validator(theForm)\n");
                        tmpS.Append("{\n");
                        tmpS.Append("submitonce(theForm);\n");
                        tmpS.Append("return (true);\n");
                        tmpS.Append("}\n");
                        tmpS.Append("</script>\n");

                        tmpS.Append("<p>Please enter the following information about this " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting).ToLowerInvariant() + ". Fields marked with an asterisk (*) are required. All other fields are optional.</p>\n");
                        tmpS.Append("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\">\n");
                        tmpS.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td width=\"100%\" colspan=\"2\" align=\"left\">\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("<tr>\n");
                        tmpS.Append("<td></td><td align=\"left\" valign=\"top\">\n");
                        if (Editing)
                        {
                            tmpS.Append("<input type=\"submit\" value=\"Update\" name=\"submit\">\n");
                            tmpS.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\" class=\"CPButton\" value=\"Reset\" name=\"reset\">\n");
                        }
                        else
                        {
                            tmpS.Append("<input type=\"submit\" value=\"Add New\" name=\"submit\">\n");
                        }
                        tmpS.Append("        </td>\n");
                        tmpS.Append("      </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td width=\"25%\" align=\"right\" valign=\"top\">*" + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + " Name:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\" valign=\"top\">\n");
                        tmpS.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Name"), "Name", false, true, true, "Please enter the " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting).ToLowerInvariant() + " name", 100, 50, 0, 0, false));
                        tmpS.Append("                	</td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\" bgcolor=\"" + CommonLogic.IIF(Editing && !DB.RSFieldBool(rs, "Published"), "#FFFFCC", "FFFFFF") + "\">*Published:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" bgcolor=\"" + CommonLogic.IIF(Editing && !DB.RSFieldBool(rs, "Published"), "#FFFFCC", "FFFFFF") + "\">\n");
                        tmpS.Append("Yes&nbsp;<input type=\"radio\" name=\"Published\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "Published"), " checked=\"checked\" ", ""), " checked ") + "/>\n");
                        tmpS.Append("No&nbsp;<input type=\"radio\" name=\"Published\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "Published"), "", " checked=\"checked\" "), "") + "/>\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        if (specs.m_EntityName == "Category" || specs.m_EntityName == "Section")
                        {
                            tmpS.Append("              <tr valign=\"top\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"top\">*Show In " + specs.m_ObjectName + " Browser:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" >\n");
                            tmpS.Append("Yes&nbsp;<input type=\"radio\" name=\"ShowIn" + specs.m_ObjectName + "Browser\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "ShowIn" + specs.m_ObjectName + "Browser"), " checked=\"checked\" ", ""), " checked ") + "/>\n");
                            tmpS.Append("No&nbsp;<input type=\"radio\" name=\"ShowIn" + specs.m_ObjectName + "Browser\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "ShowIn" + specs.m_ObjectName + "Browser"), "", " checked=\"checked\" "), "") + "/>\n");
                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");
                        }
                        if (specs.m_HasParentChildRelationship)
                        {
                            tmpS.Append("              <tr valign=\"top\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"top\">Parent " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + ":&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");

                            tmpS.Append("<select size=\"1\" name=\"Parent" + specs.m_EntityName + "ID\">\n");
                            tmpS.Append(" <option value=\"0\" " + CommonLogic.IIF(!Editing, " selected=\"selected\" ", "") + ">--ROOT LEVEL--</option>\n");
                            String EntitySel = EntityHelper.GetEntitySelectList(0, String.Empty, 0, ThisCustomer.LocaleSetting, false);
                            // mark current parent:
                            EntitySel = EntitySel.Replace("<option value=\"" + DB.RSFieldInt(rs, "Parent" + specs.m_EntityName + "ID").ToString() + "\">", "<option value=\"" + DB.RSFieldInt(rs, "Parent" + specs.m_EntityName + "ID").ToString() + "\" selected>");
                            tmpS.Append(EntitySel);
                            tmpS.Append("</select>\n");

                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");
                        }

                        tmpS.Append("<tr valign=\"top\">\n");
                        tmpS.Append("<td align=\"right\" valign=\"top\">*Display Format Xml Package:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("<select size=\"1\" name=\"XmlPackage\">\n");
                        ArrayList xmlPackages = AppLogic.ReadXmlPackages("entity", 1);
                        foreach (String s in xmlPackages)
                        {
                            tmpS.Append("<option value=\"" + s + "\"");
                            if (Editing)
                            {
                                if (DB.RSField(rs, "XmlPackage").Equals(s, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    tmpS.Append(" selected");
                                }
                            }
                            tmpS.Append(">" + s + "</option>");
                        }
                        tmpS.Append("</select>\n");
                        tmpS.Append("</td>\n");
                        tmpS.Append("</tr>\n");

                        tmpS.Append("<tr valign=\"top\">\n");
                        tmpS.Append("<td align=\"right\" valign=\"top\">Quantity Discount Table:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("<td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("<select size=\"1\" name=\"QuantityDiscountID\">\n");
                        tmpS.Append("<option value=\"0\">None</option>");

                        using (SqlConnection conQtyDiscount = new SqlConnection(DB.GetDBConn()))
                        {
                            conQtyDiscount.Open();
                            using (IDataReader rsst = DB.GetRS("select * from QuantityDiscount   with (NOLOCK)  order by DisplayOrder,Name", conQtyDiscount))
                            {
                                while (rsst.Read())
                                {
                                    tmpS.Append("<option value=\"" + DB.RSFieldInt(rsst, "QuantityDiscountID").ToString() + "\"");
                                    if (Editing)
                                    {
                                        if (DB.RSFieldInt(rs, "QuantityDiscountID") == DB.RSFieldInt(rsst, "QuantityDiscountID"))
                                        {
                                            tmpS.Append(" selected");
                                        }
                                    }
                                    tmpS.Append(">" + DB.RSFieldByLocale(rsst, "Name", ThisCustomer.LocaleSetting) + "</option>");
                                }
                            }
                        }

                        tmpS.Append("</select>\n");
                        tmpS.Append("</td>\n");
                        tmpS.Append("</tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Page Size:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("                	<input maxLength=\"2\" size=\"2\" name=\"PageSize\" value=\"" + CommonLogic.IIF(Editing, DB.RSFieldInt(rs, "PageSize").ToString(), AppLogic.AppConfig("Default_" + specs.m_EntityName + "PageSize")) + "\"> (may be used by the XmlPackage displaying this page)\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Column Width:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("                	<input maxLength=\"2\" size=\"2\" name=\"ColWidth\" value=\"" + CommonLogic.IIF(Editing, DB.RSFieldInt(rs, "ColWidth").ToString(), AppLogic.AppConfig("Default_" + specs.m_EntityName + "ColWidth")) + "\"> (may be used by the XmlPackage displaying this page)\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Order " + specs.m_ObjectNamePlural + " By Looks:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("Yes&nbsp;<input type=\"radio\" name=\"SortByLooks\" value=\"1\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "SortByLooks"), " checked=\"checked\" ", ""), "") + "/>\n");
                        tmpS.Append("No&nbsp;<input type=\"radio\" name=\"SortByLooks\" value=\"0\" " + CommonLogic.IIF(Editing, CommonLogic.IIF(DB.RSFieldBool(rs, "SortByLooks"), "", " checked=\"checked\" "), " checked ") + "/>\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        if (specs.m_HasAddress)
                        {
                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">Street Address:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("                	<input maxLength=\"100\" size=\"30\" name=\"Address1\" value=\"" + CommonLogic.IIF(Editing, HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Address1")), "") + "\">\n");

                            tmpS.Append("                	&nbsp;&nbsp;\n");
                            tmpS.Append("                	Apt/Suite#:\n");
                            tmpS.Append("                	<input maxLength=\"100\" size=\"5\" name=\"Suite\" value=\"" + CommonLogic.IIF(Editing, HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Suite")), "") + "\">\n");
                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");
                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\"></td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("                	<input maxLength=\"100\" size=\"30\" name=\"Address2\" value=\"" + CommonLogic.IIF(Editing, HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "Address2")), "") + "\">\n");
                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");
                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">City:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("                	<input maxLength=\"100\" size=\"30\" name=\"City\" value=\"" + CommonLogic.IIF(Editing, HttpContext.Current.Server.HtmlEncode(DB.RSField(rs, "City")), "") + "\">\n");
                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");

                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">State:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">");
                            tmpS.Append("<select size=\"1\" name=\"State\">");
                            tmpS.Append("<option value=\"\"" + CommonLogic.IIF(DB.RSField(rs, "State").Length == 0, " selected=\"selected\"", String.Empty) + ">SELECT ONE</option>");

                            using (SqlConnection conState = new SqlConnection(DB.GetDBConn()))
                            {
                                conState.Open();
                                using (IDataReader rsState = DB.GetRS("select * from state   with (NOLOCK)  order by DisplayOrder,Name", conState))
                                {
                                    while (rsState.Read())
                                    {
                                        tmpS.Append("<option value=\"" + HttpContext.Current.Server.HtmlEncode(DB.RSField(rsState, "Abbreviation")) + "\"" + CommonLogic.SelectOption(rsState, DB.RSField(rsState, "Abbreviation"), "State") + ">" + HttpContext.Current.Server.HtmlEncode(DB.RSField(rsState, "Name")) + "</option>");
                                    }
                                }
                            }

                            tmpS.Append("</select>");
                            tmpS.Append("			</td>\n");
                            tmpS.Append("              </tr>\n");

                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">Zip Code:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("                	<input maxLength=\"16\" size=\"15\" name=\"ZipCode\" value=\"" + CommonLogic.IIF(Editing, DB.RSField(rs, "ZipCode"), "") + "\">\n");
                            tmpS.Append("                    <input type=\"hidden\" name=\"ZipCode_vldt\" value=\"[invalidalert=Please enter a valid zipcode]\">\n");
                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");

                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">Country:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("<select name=\"Country\" size=\"1\">");
                            tmpS.Append("<option value=\"0\">SELECT ONE</option>");

                            using (SqlConnection conCountry = new SqlConnection(DB.GetDBConn()))
                            {
                                conCountry.Open();
                                using (IDataReader rsCountry = DB.GetRS("select * from country   with (NOLOCK)  order by DisplayOrder,Name", conCountry))
                                {
                                    while (rsCountry.Read())
                                    {
                                        tmpS.Append("<option value=\"" + DB.RSField(rsCountry, "Name") + "\"" + CommonLogic.IIF(DB.RSField(rsCountry, "Country") == DB.RSField(rsCountry, "Name"), " selected ", "") + ">" + DB.RSField(rsCountry, "Name") + "</option>");
                                    }
                                }
                            }

                            tmpS.Append("</SELECT>");
                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");

                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">Web Site:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("                	<input maxLength=\"100\" size=\"35\" name=\"URL\" value=\"" + CommonLogic.IIF(Editing, DB.RSField(rs, "URL"), "") + "\">&nbsp;&nbsp;<small>(e.g. http://abcd.com)</small>\n");
                            tmpS.Append("               	</td>\n");
                            tmpS.Append("              </tr>\n");
                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">E-Mail Address:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("                	<input maxLength=\"100\" size=\"35\" name=\"EMail\" value=\"" + CommonLogic.IIF(Editing, DB.RSField(rs, "EMail"), CommonLogic.QueryStringCanBeDangerousContent("EMail")) + "\">\n");
                            tmpS.Append("                    <input type=\"hidden\" name=\"EMail_vldt\" value=\"" + CommonLogic.IIF(specs.m_EntityName.Equals("DISTRIBUTOR", StringComparison.InvariantCultureIgnoreCase), "[req][blankalert=EMail is a required field for a Distributor]", "") + "[invalidalert=Please enter a valid e-mail address]\">\n");
                            tmpS.Append("               	</td>\n");
                            tmpS.Append("              </tr>\n");
                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">Phone:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("                	<input maxLength=\"35\" size=\"35\" name=\"Phone\" value=\"" + CommonLogic.IIF(Editing, CommonLogic.GetPhoneDisplayFormat(DB.RSField(rs, "Phone")), "") + "\">&nbsp;&nbsp;<small>(optional, including area code)</small>\n");
                            tmpS.Append("                    <input type=\"hidden\" name=\"Phone_vldt\" value=\"[invalidalert=Please enter a valid phone number with areacode, e.g. (480) 555-1212]\">\n");
                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");
                            tmpS.Append("              <tr valign=\"middle\">\n");
                            tmpS.Append("                <td align=\"right\" valign=\"middle\">Fax:&nbsp;&nbsp;</td>\n");
                            tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                            tmpS.Append("                	<input maxLength=\"35\" size=\"35\" name=\"FAX\" value=\"" + CommonLogic.IIF(Editing, CommonLogic.GetPhoneDisplayFormat(DB.RSField(rs, "Fax")), "") + "\">&nbsp;&nbsp;<small>(optional, including area code)</small>\n");
                            tmpS.Append("                    <input type=\"hidden\" name=\"FAX_vldt\" value=\"[invalidalert=Please enter a valid FAX number with areacode, e.g. (480) 555-1212]\">\n");
                            tmpS.Append("                </td>\n");
                            tmpS.Append("              </tr>\n");
                        }

                        // BEGIN IMAGES 

                        tmpS.Append("              <tr valign=\"middle\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Image Filename Override:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("                	<input maxLength=\"100\" size=\"40\" name=\"ImageFilenameOverride\" value=\"" + CommonLogic.IIF(Editing, DB.RSField(rs, "ImageFilenameOverride"), "") + "\">\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");


                        bool disableupload = (Editing && DB.RSField(rs, "ImageFilenameOverride") != "");
                        if (specs.m_HasIconPic)
                        {
                            tmpS.Append("  <tr>\n");
                            tmpS.Append("    <td valign=\"top\" align=\"right\">Icon:\n");
                            tmpS.Append("</td>\n");
                            tmpS.Append("    <td valign=\"top\" align=\"left\">");
                            tmpS.Append("    <input type=\"file\" name=\"Image1\" size=\"50\" value=\"" + CommonLogic.IIF(Editing, "", "") + "\" " + CommonLogic.IIF(disableupload, " disabled ", "") + ">\n");
                            String Image1URL = AppLogic.LookupImage(specs.m_EntityName, EntityID, "icon", SkinID, ThisCustomer.LocaleSetting);
                            if (Image1URL.Length != 0)
                            {
                                if (Image1URL.IndexOf("nopicture") == -1)
                                {
                                    tmpS.Append("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image1URL + "','Pic1');\">Click here</a> to delete the current image\n");
                                }
                                tmpS.Append("<img id=\"Pic1\" name=\"Pic1\" border=\"0\" src=\"" + Image1URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\">\n");
                            }
                            tmpS.Append("</td>\n");
                            tmpS.Append(" </tr>\n");
                        }

                        if (specs.m_HasMediumPic)
                        {
                            tmpS.Append("  <tr>\n");
                            tmpS.Append("    <td valign=\"top\" align=\"right\">Medium Pic:\n");
                            tmpS.Append("</td>\n");
                            tmpS.Append("    <td valign=\"top\" align=\"left\">");
                            tmpS.Append("    <input type=\"file\" name=\"Image2\" size=\"50\" value=\"" + CommonLogic.IIF(Editing, "", "") + "\" " + CommonLogic.IIF(disableupload, " disabled ", "") + ">\n");
                            String Image2URL = AppLogic.LookupImage(specs.m_EntityName, EntityID, "medium", SkinID, ThisCustomer.LocaleSetting);
                            if (Image2URL.Length != 0)
                            {
                                if (Image2URL.IndexOf("nopicture") == -1)
                                {
                                    tmpS.Append("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image2URL + "','Pic2');\">Click here</a> to delete the current image\n");
                                }
                                tmpS.Append("<img id=\"Pic2\" name=\"Pic2\" border=\"0\" src=\"" + Image2URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\">\n");
                            }
                            tmpS.Append("</td>\n");
                            tmpS.Append(" </tr>\n");
                        }

                        if (specs.m_HasLargePic)
                        {
                            tmpS.Append("  <tr>\n");
                            tmpS.Append("    <td valign=\"top\" align=\"right\">Large Pic:\n");
                            tmpS.Append("</td>\n");
                            tmpS.Append("    <td valign=\"top\" align=\"left\">");
                            tmpS.Append("    <input type=\"file\" name=\"Image3\" size=\"50\" value=\"" + CommonLogic.IIF(Editing, "", "") + "\" " + CommonLogic.IIF(disableupload, " disabled ", "") + ">\n");
                            String Image3URL = AppLogic.LookupImage(specs.m_EntityName, EntityID, "large", SkinID, ThisCustomer.LocaleSetting);
                            if (Image3URL.Length == 0)
                            {
                                Image3URL = AppLogic.NoPictureImageURL(false, SkinID, ThisCustomer.LocaleSetting);
                            }
                            if (Image3URL.Length != 0)
                            {
                                if (Image3URL.IndexOf("nopicture") == -1)
                                {
                                    tmpS.Append("<a href=\"javascript:void(0);\" onClick=\"DeleteImage('" + Image3URL + "','Pic3');\">Click here</a> to delete the current image\n");
                                }
                                tmpS.Append("<img id=\"Pic3\" name=\"Pic3\" border=\"0\" src=\"" + Image3URL + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\">\n");
                            }
                            tmpS.Append("</td>\n");
                            tmpS.Append(" </tr>\n");
                        }

                        // END IMAGES

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Summary:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Summary"), "Summary", true, true, false, "", 0, 0, AppLogic.AppConfigUSInt("Admin_TextareaHeight"), AppLogic.AppConfigUSInt("Admin_TextareaWidth"), true));

                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Description:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "Description"), "Description", true, true, false, "", 0, 0, AppLogic.AppConfigUSInt("Admin_TextareaHeight"), AppLogic.AppConfigUSInt("Admin_TextareaWidth"), true));

                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Extension Data (User Defined Data):&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("                	<textarea style=\"width: 100%;\" cols=\"" + AppLogic.AppConfig("Admin_TextareaWidth") + "\" rows=\"10\" id=\"ExtensionData\" name=\"ExtensionData\">" + CommonLogic.IIF(Editing, DB.RSField(rs, "ExtensionData"), "") + "</textarea>\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Search Engine Page Title:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "SETitle"), "SETitle", false, true, false, "", 100, 100, 0, 0, false));

                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Search Engine Keywords:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "SEKeywords"), "SEKeywords", false, true, false, "", 255, 100, 0, 0, false));

                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Search Engine Description:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "SEDescription"), "SEDescription", false, true, false, "", 255, 100, 0, 0, false));

                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"top\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Search Engine NoScript:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append(AppLogic.GetLocaleEntryFields(DB.RSField(rs, "SENoScript"), "SENoScript", true, true, false, "", 50, 50, 0, 0, false));

                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"middle\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Page BG Color:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("                	<input maxLength=\"20\" size=\"10\" name=\"PageBGColor\" value=\"" + CommonLogic.IIF(Editing, DB.RSField(rs, "PageBGColor"), "") + "\">\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"middle\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Contents BG Color:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("                	<input maxLength=\"20\" size=\"10\" name=\"ContentsBGColor\" value=\"" + CommonLogic.IIF(Editing, DB.RSField(rs, "ContentsBGColor"), "") + "\">\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("              <tr valign=\"middle\">\n");
                        tmpS.Append("                <td align=\"right\" valign=\"top\">Skin Graphics Color:&nbsp;&nbsp;</td>\n");
                        tmpS.Append("                <td align=\"left\" valign=\"top\">\n");
                        tmpS.Append("                	<input maxLength=\"20\" size=\"10\" name=\"GraphicsColor\" value=\"" + CommonLogic.IIF(Editing, DB.RSField(rs, "GraphicsColor"), "") + "\">\n");
                        tmpS.Append("                </td>\n");
                        tmpS.Append("              </tr>\n");

                        tmpS.Append("<tr>\n");
                        tmpS.Append("<td></td><td align=\"left\" valign=\"top\">\n");
                        if (Editing)
                        {
                            tmpS.Append("<input type=\"submit\" value=\"Update\" name=\"submit\">\n");
                            tmpS.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input type=\"reset\" class=\"CPButton\" value=\"Reset\" name=\"reset\">\n");
                        }
                        else
                        {
                            tmpS.Append("<input type=\"submit\" value=\"Add New\" name=\"submit\">\n");
                        }
                        tmpS.Append("        </td>\n");
                        tmpS.Append("      </tr>\n");
                        tmpS.Append("  </table>\n");

                        tmpS.Append("<script type=\"text/javascript\">\n");

                        tmpS.Append("function DeleteImage(imgurl,name)\n");
                        tmpS.Append("{\n");
                        tmpS.Append("window.open('deleteimage.aspx?imgurl=' + imgurl + '&FormImageName=' + name,\"Admin_ML\",\"height=250,width=440,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no\")\n");
                        tmpS.Append("}\n");

                        tmpS.Append("</SCRIPT>\n");
                    }
                }
            }

            tmpS.Append("</form>\n");
            return tmpS.ToString();
        }

        static public void EntityListPageFormHandler(EntitySpecs specs, Customer ThisCustomer, int SkinID)
        {
            if (CommonLogic.QueryStringCanBeDangerousContent("DeleteID").Length != 0)
            {
                // delete the record:
                DB.ExecuteSQL("update " + specs.m_EntityName + " set Deleted=1 where " + specs.m_EntityName + "ID=" + CommonLogic.QueryStringCanBeDangerousContent("DeleteID"));
                HttpContext.Current.Response.Redirect("entities.aspx?entityname=" + specs.m_EntityName);
            }
            if (CommonLogic.FormBool("IsSubmit"))
            {
                for (int i = 0; i <= HttpContext.Current.Request.Form.Count - 1; i++)
                {
                    if (HttpContext.Current.Request.Form.Keys[i].IndexOf("DisplayOrder_") != -1)
                    {
                        String[] keys = HttpContext.Current.Request.Form.Keys[i].Split('_');
                        int EntityID = Localization.ParseUSInt(keys[1]);
                        int DispOrd = 1;
                        try
                        {
                            DispOrd = Localization.ParseUSInt(HttpContext.Current.Request.Form[HttpContext.Current.Request.Form.Keys[i]]);
                        }
                        catch { }
                        DB.ExecuteSQL("update " + specs.m_EntityName + " set DisplayOrder=" + DispOrd.ToString() + " where " + specs.m_EntityName + "ID=" + EntityID.ToString());
                    }
                }
                HttpContext.Current.Response.Redirect("entities.aspx?entityname=" + specs.m_EntityName);
            }
        }

        static public String EntityListPageRender(AspDotNetStorefront.SkinBase sb, EntitySpecs specs, Customer ThisCustomer, int SkinID)
        {
            StringBuilder tmpS = new StringBuilder(50000);
            EntityHelper EntityMgr = AppLogic.LookupHelper(sb.EntityHelpers, specs.m_EntityName);

            tmpS.Append("<form method=\"POST\" action=\"entities.aspx?entityname=" + specs.m_EntityName + "\">\n");
            tmpS.Append("<input type=\"hidden\" name=\"IsSubmit\" value=\"true\">\n");
            tmpS.Append("<p align=\"left\"><input type=\"button\" value=\"Add New " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + "\" name=\"AddNew\" onClick=\"self.location='editentity.aspx?entityname=" + specs.m_EntityName + "';\"><p>");
            tmpS.Append("  <table border=\"0\" cellpadding=\"2\" border=\"0\" cellspacing=\"1\" width=\"100%\">\n");
            tmpS.Append("    <tr bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\">\n");
            tmpS.Append("      <td><b>ID</b></td>\n");
            tmpS.Append("      <td><b>" + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + "</b></td>\n");
            if (specs.m_HasParentChildRelationship)
            {
                tmpS.Append("      <td align=\"center\"><b>Parent " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + "</b></td>\n");
            }
            tmpS.Append("      <td align=\"center\"><b>" + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + " Display Order</b></td>\n");
            tmpS.Append("      <td align=\"center\"><b>Edit " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + "</b></td>\n");
            tmpS.Append("      <td align=\"center\"><b>View " + specs.m_ObjectNamePlural + "</b></td>\n");
            tmpS.Append("      <td align=\"center\"><b>Set " + specs.m_ObjectNamePlural + " Display Order</b></td>\n");
            if (specs.m_ObjectName == "Product")
            {
                tmpS.Append("      <td align=\"center\"><b>Bulk " + specs.m_ObjectNamePlural + " Edit</b></td>\n");
            }
            tmpS.Append("      <td align=\"center\"><b>Delete " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + "</b></td>\n");
            tmpS.Append("    </tr>\n");

            GetEntities(specs, EntityMgr, tmpS, 0, 1, SkinID, ThisCustomer);

            tmpS.Append("    <tr>\n");
            tmpS.Append("      <td colspan=\"" + CommonLogic.IIF(specs.m_HasParentChildRelationship, "3", "2") + "\" align=\"left\"></td>\n");
            tmpS.Append("      <td align=\"center\" bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\"><input type=\"submit\" value=\"Update\" name=\"Submit\"></td>\n");
            tmpS.Append("      <td colspan=\"" + CommonLogic.IIF(specs.m_ObjectName == "Product", "5", "4") + "\"></td>\n");
            tmpS.Append("    </tr>\n");
            tmpS.Append("  </table>\n");
            tmpS.Append("<p align=\"left\"><input type=\"button\" value=\"Add New " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + "\" name=\"AddNew\" onClick=\"self.location='editentity.aspx?entityname=" + specs.m_EntityName + "';\"><p>");
            tmpS.Append("</form>\n");

            tmpS.Append("</center></b>\n");

            tmpS.Append("<script type=\"text/javascript\">\n");
            tmpS.Append("function Delete" + specs.m_EntityName + "(id)\n");
            tmpS.Append("{\n");
            tmpS.Append("if(confirm('Are you sure you want to delete " + AppLogic.GetString("AppConfig." + specs.m_EntityName + "PromptSingular", SkinID, ThisCustomer.LocaleSetting) + ": ' + id))\n");
            tmpS.Append("{\n");
            tmpS.Append("self.location = '" + CommonLogic.GetThisPageName(false) + "?entityname=" + specs.m_EntityName + "&deleteid=' + id;\n");
            tmpS.Append("}\n");
            tmpS.Append("}\n");
            tmpS.Append("</SCRIPT>\n");
            return tmpS.ToString();
        }

        private static void GetEntities(EntitySpecs specs, EntityHelper EntityMgr, StringBuilder tmpS, int ForParentEntityID, int level, int SkinID, Customer ThisCustomer)
        {
            String Indent = String.Empty;
            for (int i = 1; i < level; i++)
            {
                Indent += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            }

            XmlNode n;
            if (ForParentEntityID == 0)
            {
                n = EntityMgr.m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = EntityMgr.m_TblMgr.SetContext(ForParentEntityID);
            }

            if (n != null && EntityMgr.m_TblMgr.HasChildren(n))
            {
                n = EntityMgr.m_TblMgr.MoveFirstChild(n);
                while (n != null)
                {
                    int ThisID = EntityMgr.m_TblMgr.CurrentID(n);
                    tmpS.Append("    <tr bgcolor=\"" + AppLogic.AppConfig("LightCellColor") + "\">\n");
                    tmpS.Append("      <td >" + ThisID.ToString() + "</td>\n");
                    tmpS.Append("<td>\n");
                    String Image1URL = AppLogic.LookupImage(specs.m_EntityName, ThisID, "icon", SkinID, ThisCustomer.LocaleSetting);
                    tmpS.Append("<a href=\"editentity.aspx?entityname=" + specs.m_EntityName + "&entityid=" + ThisID.ToString() + "\">");
                    tmpS.Append("<img src=\"" + Image1URL + "\" height=\"25\" border=\"0\" align=\"absmiddle\">");
                    tmpS.Append("</a>&nbsp;\n");
                    tmpS.Append("<a href=\"editentity.aspx?entityname=" + specs.m_EntityName + "&entityid=" + ThisID.ToString() + "\">");
                    if (level == 1)
                    {
                        tmpS.Append("<b>");
                    }
                    tmpS.Append(Indent + EntityMgr.m_TblMgr.CurrentName(n, ThisCustomer.LocaleSetting));
                    if (level == 1)
                    {
                        tmpS.Append("</b>");
                    }
                    tmpS.Append("</a>");
                    tmpS.Append("</td>\n");
                    if (specs.m_HasParentChildRelationship)
                    {
                        String ParDesc = EntityMgr.m_TblMgr.CurrentFieldInt(n, "ParentEntityID").ToString();
                        if (ParDesc == "0")
                        {
                            ParDesc = "&nbsp;";
                        }
                        tmpS.Append("<td align=\"center\">" + ParDesc + "</td>\n");
                    }
                    tmpS.Append("<td align=\"center\"><input size=4 type=\"text\" name=\"DisplayOrder_" + ThisID.ToString() + "\" value=\"" + EntityMgr.m_TblMgr.CurrentFieldInt(n, "DisplayOrder").ToString() + "\"></td>\n");
                    tmpS.Append("<td align=\"center\"><input type=\"button\" style=\"font-size: 9px;\" value=\"Edit\" name=\"Edit_" + ThisID.ToString() + "\" onClick=\"self.location='editentity.aspx?entityname=" + specs.m_EntityName + "&entityid=" + ThisID.ToString() + "'\"></td>\n");
                    tmpS.Append("<td align=\"center\"><input type=\"button\" style=\"font-size: 9px;\" value=\"" + specs.m_ObjectNamePlural + "\" name=\"" + specs.m_ObjectNamePlural + "_" + ThisID.ToString() + "\" onClick=\"self.location='" + specs.m_ObjectNamePlural + ".aspx?entityname=" + specs.m_EntityName + "&EntityFilterID=" + ThisID.ToString() + "'\"></td>\n");
                    tmpS.Append("<td align=\"center\"><input type=\"button\" style=\"font-size: 9px;\" value=\"DisplayOrder\" name=\"DisplayOrder_" + ThisID.ToString() + "\" onClick=\"self.location='displayorder.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\"></td>\n");
                    if (specs.m_ObjectName == "Product")
                    {
                        int numEntityObjects = EntityMgr.GetNumEntityObjects(ThisID, true, true);
                        if (numEntityObjects == 0)
                        {
                            tmpS.Append("<td align=\"center\">(No " + specs.m_ObjectNamePlural + ")</td>\n");
                        }
                        else if (numEntityObjects > 100)
                        {
                            tmpS.Append("<td align=\"center\">(Too Many " + specs.m_ObjectNamePlural + ")</td>\n");
                        }
                        else
                        {
                            tmpS.Append("<td align=\"center\">");
                            tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"Inventory\" name=\"InventoryEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditinventory.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">");
                            tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"SEFields\" name=\"SearchEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditsearch.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">");
                            tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"Prices\" name=\"PricesEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditprices.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">");
                            if (Shipping.GetActiveShippingCalculationID() == Shipping.ShippingCalculationEnum.UseIndividualItemShippingCosts)
                            {
                                tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"ShipCosts\" name=\"ShippingCostsEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditshippingcosts.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">");
                            }
                            tmpS.Append("<input type=\"button\" style=\"font-size: 9px;\" value=\"DownloadFiles\" name=\"DownloadFilesEdit_" + ThisID.ToString() + "\" onClick=\"self.location='bulkeditdownloadfiles.aspx?entityname=" + specs.m_EntityName + "&EntityID=" + ThisID.ToString() + "'\">");
                            tmpS.Append("</td>");
                        }
                    }
                    tmpS.Append("<td align=\"center\"><input type=\"button\" style=\"font-size: 9px;\" value=\"Delete\" name=\"Delete_" + ThisID.ToString() + "\" onClick=\"Delete" + specs.m_EntityName + "(" + ThisID.ToString() + ")\"></td>\n");
                    tmpS.Append("</tr>\n");
                    if (EntityMgr.m_TblMgr.HasChildren(n))
                    {
                        GetEntities(specs, EntityMgr, tmpS, ThisID, level + 1, SkinID, ThisCustomer);
                    }
                    n = EntityMgr.m_TblMgr.MoveNextSibling(n, false);
                }
            }
        }

    }
}
