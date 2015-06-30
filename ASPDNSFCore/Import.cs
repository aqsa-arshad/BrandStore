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
using System.Globalization;
using System.Text;
using System.Web;
using System.Xml;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontExcelWrapper;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for Import.
    /// </summary>
    public class Import
    {
        public Import()
        { }

        // in format: "ProductID~Related ProductName|ProductID~Related ProductName|ProductID~Related ProductName|etc..."
        static private StringBuilder RelatedProductsToDoList = new StringBuilder(10000);

        static private String ProcessingNode = String.Empty;

        public static String ProcessXmlImportFile(String XmlFile)
        {
            StringBuilder tmpS = new StringBuilder(100000);
            String XmlContents = CommonLogic.ReadFile(XmlFile, false);
            Import.DoIt(tmpS, XmlContents, false);
            return tmpS.ToString();
        }

        // we're going to read Excel and convert into our Xml Format and then use the same processing code.
        // this will be a little less efficient than just doing the processing on the Excel file, but it will
        // allow the actual import processing logic to be centralized.
        public static String ProcessExcelImportFile(String ExcelFile)
        {
            StringBuilder tmpS = new StringBuilder(100000);

            // get excel file and convert to Xml Import Format:
            String XmlContents = ConvertExcelToXml(ExcelFile);

            // now save the Xml File for later review if necessary:
            String XmlFileName = "ExcelImport_" + Localization.ToThreadCultureShortDateString(System.DateTime.Now).Replace(" ", "").Replace("/", "").Replace(":", "").Replace(".", "");
            String SaveToXmlFilename = CommonLogic.SafeMapPath("../images" + "/" + XmlFileName + ".xml");
            CommonLogic.WriteFile(SaveToXmlFilename, XmlContents.ToString(), true);

            // now process the Xml File:
            Import.DoIt(tmpS, XmlContents, false);
            return tmpS.ToString();
        }

        public static String ProcessExcelImportFileTrackingNumbers(String ExcelFile, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Parser GetParser)
        {
            StringBuilder tmpS = new StringBuilder(100000);

            DataSet ds = Excel.GetDS(ExcelFile, "Sheet1");

            tmpS.Append("<table cellpadding='2' cellspacing='0' border='0'><tr><td></td><td></td><td></td></tr>");

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                int orderNumber = Localization.ParseNativeInt(dr[0].ToString());
                string trackingNumber = dr[1].ToString().Trim();

                if (orderNumber > 0)
                {
                    string existing = "";

                    try
                    {
                        existing = DB.GetSqlS("SELECT ShippingTrackingNumber AS S FROM Orders WHERE OrderNumber=" + orderNumber.ToString()).Trim();

                        if (existing.Length > 0)
                        {
                            if (existing.IndexOf(trackingNumber) < 0)
                            {
                                Order.MarkOrderAsShipped(orderNumber, "", existing + "," + trackingNumber, DateTime.Now, false, EntityHelpers, GetParser, false);
                                tmpS.Append("<tr><td><b>Order</b>: " + orderNumber + " = " + trackingNumber + "</td><td><b>Existing</b>: " + existing + "</td><td><b>Updated</b>: " + existing + "," + trackingNumber + "</i></td></tr>");
                            }
                            else
                            {
                                tmpS.Append("<tr><td><b>Order</b>: " + orderNumber + " = " + trackingNumber + "</td><td><b>Existing</b>: " + CommonLogic.IIF(existing.Length == 0, AppLogic.ro_NotApplicable, existing) + "</td><td><b>Updated</b>: <i>Duplicate - ignored</i></td></tr>");
                            }
                        }
                        else
                        {
                            Order.MarkOrderAsShipped(orderNumber, "", trackingNumber, DateTime.Now, false, EntityHelpers, GetParser, false);
                            tmpS.Append("<tr><td><b>Order</b>: " + orderNumber + " = " + trackingNumber + "</td><td><b>Existing</b>: " + CommonLogic.IIF(existing.Length == 0, AppLogic.ro_NotApplicable, existing) + "</td><td><b>Updated</b>: " + trackingNumber + "</td></tr>");
                        }
                    }
                    catch
                    {
                        tmpS.Append("<tr><td><b>Order</b>: " + orderNumber + " = " + trackingNumber + "</td><td><b>Existing</b>: " + CommonLogic.IIF(existing.Length == 0, AppLogic.ro_NotApplicable, existing) + "</td><td><b>Updated</b>: ERROR UPDATING</td></tr>");
                    }
                }
            }
            ds.Dispose();
            tmpS.Append("</table>");

            return tmpS.ToString();
        }

        // ColumnNumbers are 1 based here!! but 0 based in dataset!
        private static String GetExcelColumnForXml(DataRow row, int ColumnNumber, bool XmlEncodeIt)
        {
            String tmpS = String.Empty;
            try
            {
                // the col may not exist:
                tmpS = "" + row[ColumnNumber - 1].ToString();
            }
            catch { }
            if (XmlEncodeIt)
            {
                tmpS = XmlCommon.XmlEncode(tmpS);
            }
            return tmpS;
        }

        // ExcelFile should be relative filename!
        private static String ConvertExcelToXml(String ExcelFile)
        {
            StringBuilder tmpS = new StringBuilder(100000);
            ExcelToXml exf = new ExcelToXml(ExcelFile);
            int MaxRowsInSpreadsheet = AppLogic.AppConfigUSInt("ImportMaxRowsExcel");

            if (MaxRowsInSpreadsheet == 0)
            {
                MaxRowsInSpreadsheet = 10000;
            }


            // a row without cols A or AQ having data terminates the spreadsheet!
            XmlDocument xmlDoc = exf.LoadSheet("Sheet1", "CA", MaxRowsInSpreadsheet, "A,AQ");

            tmpS.Append("<AspDotNetStorefrontImportFile>");
            int rowI = 1;
            bool first = true;
            Decimal ImportFileVersion = 2.0M;
            String LastPName = String.Empty;
            String FV = exf.GetCell(xmlDoc, 1, "E");
            if (FV.StartsWith("IMPORT FILE VERSION", StringComparison.InvariantCultureIgnoreCase))
            {
                ImportFileVersion = Localization.ParseNativeDecimal(exf.GetCell(xmlDoc, 1, "F"));
            }
            tmpS.Append("<Version>" + Localization.DecimalStringForDB(ImportFileVersion) + "</Version>");
            foreach (XmlNode row in xmlDoc.SelectNodes("/excel/sheet/row"))
            {
                rowI = XmlCommon.XmlAttributeUSInt(row, "id");
                if (rowI > 3) // skip first 3 header rows!
                {
                    // if this line has no variant price, and is not a kit group or kit item, consider this line as EOF
                    String PName = exf.GetCell(row, "A");
                    String s;
                    if (PName.ToUpper(CultureInfo.InvariantCulture) == "KITGROUPDEF")
                    {
                        // skip this row
                        if (LastPName.ToUpper(CultureInfo.InvariantCulture) == "KITITEM" || LastPName.ToUpper(CultureInfo.InvariantCulture) == "KITGROUP")
                        {
                            tmpS.Append("</KitGroup>");
                        }
                    }
                    else if (PName.ToUpper(CultureInfo.InvariantCulture) == "KITGROUP")
                    {
                        // Process Kit Group Row
                        if (LastPName.ToUpper(CultureInfo.InvariantCulture) == "KITITEM" || LastPName.ToUpper(CultureInfo.InvariantCulture) == "KITGROUP")
                        {
                            tmpS.Append("</KitGroup>");
                        }
                        tmpS.Append("<KitGroup>");
                        tmpS.Append("<Name>" + XmlCommon.XmlEncode(exf.GetCell(row, "B")) + "</Name>");
                        tmpS.Append("<Description>" + XmlCommon.XmlEncode(exf.GetCell(row, "C")) + "</Description>");
                        tmpS.Append("<DisplayOrder>" + exf.GetCell(row, "D") + "</DisplayOrder>");
                        tmpS.Append("<KitGroupTypeID>" + exf.GetCell(row, "E") + "</KitGroupTypeID>");
                        tmpS.Append("<IsRequired>" + exf.GetCell(row, "F") + "</IsRequired>");
                    }
                    else if (PName.ToUpper(CultureInfo.InvariantCulture) == "KITITEM")
                    {
                        // Process Kit Item Row
                        if (exf.GetCell(row, "B").ToUpper(CultureInfo.InvariantCulture) != "NAME" && exf.GetCell(row, "C").ToUpper(CultureInfo.InvariantCulture) != "DESCRIPTION")
                        {
                            tmpS.Append("<KitItem>");
                            tmpS.Append("<Name>" + XmlCommon.XmlEncode(exf.GetCell(row, "B")) + "</Name>");
                            tmpS.Append("<Description>" + XmlCommon.XmlEncode(exf.GetCell(row, "C")) + "</Description>");
                            tmpS.Append("<DisplayOrder>" + exf.GetCell(row, "D") + "</DisplayOrder>");
                            tmpS.Append("<PriceDelta>" + exf.GetCell(row, "E") + "</PriceDelta>");
                            tmpS.Append("<IsDefault>" + exf.GetCell(row, "F") + "</IsDefault>");
                            tmpS.Append("<TextOptionMaxLength>" + exf.GetCell(row, "G") + "</TextOptionMaxLength>");
                            tmpS.Append("<TextOptionWidth>" + exf.GetCell(row, "H") + "</TextOptionWidth>");
                            tmpS.Append("<TextOptionHeight>" + exf.GetCell(row, "I") + "</TextOptionHeight>");
                            tmpS.Append("</KitItem>");
                        }
                    }
                    else
                    {
                        if (PName.Length != 0)
                        {
                            if (!first)
                            {
                                if (LastPName.ToUpper(CultureInfo.InvariantCulture) == "KITGROUP")
                                {
                                    tmpS.Append("</KitGroup>");
                                }
                                if (LastPName.ToUpper(CultureInfo.InvariantCulture) == "KITITEM")
                                {
                                    tmpS.Append("</KitGroup>");
                                }
                                tmpS.Append("</Product>");
                            }
                            tmpS.Append("<Product>");
                            tmpS.Append("<Name>" + XmlCommon.XmlEncode(PName) + "</Name>");
                            tmpS.Append("<ProductTypeRef>" + XmlCommon.XmlEncode(exf.GetCell(row, "B")) + "</ProductTypeRef>");
                            tmpS.Append("<ManufacturerRef>" + XmlCommon.XmlEncode(exf.GetCell(row, "C")) + "</ManufacturerRef>");
                            tmpS.Append("<DistributorRef>" + XmlCommon.XmlEncode(exf.GetCell(row, "D")) + "</DistributorRef>");
                            s = exf.GetCell(row, "E");
                            if (s.Length != 0 && !s.StartsWith("/"))
                            {
                                s = "/" + s;
                            }
                            tmpS.Append("<CategoryRef>" + XmlCommon.XmlEncode(s) + "</CategoryRef>");
                            s = exf.GetCell(row, "F");
                            if (s.Length != 0 && !s.StartsWith("/"))
                            {
                                s = "/" + s;
                            }
                            tmpS.Append("<CategoryRef>" + XmlCommon.XmlEncode(s) + "</CategoryRef>");
                            if (ImportFileVersion >= 3)
                            {
                                s = exf.GetCell(row, "G");
                                if (s.Length != 0 && !s.StartsWith("/"))
                                {
                                    s = "/" + s;
                                }
                                tmpS.Append("<CategoryRef>" + XmlCommon.XmlEncode(s) + "</CategoryRef>");
                                s = exf.GetCell(row, "H");
                                if (s.Length != 0 && !s.StartsWith("/"))
                                {
                                    s = "/" + s;
                                }
                                tmpS.Append("<CategoryRef>" + XmlCommon.XmlEncode(s) + "</CategoryRef>");
                            }
                            s = exf.GetCell(row, "I");
                            if (s.Length != 0 && !s.StartsWith("/"))
                            {
                                s = "/" + s;
                            }
                            tmpS.Append("<SectionRef>" + XmlCommon.XmlEncode(s) + "</SectionRef>");
                            s = exf.GetCell(row, "J");
                            if (s.Length != 0 && !s.StartsWith("/"))
                            {
                                s = "/" + s;
                            }
                            tmpS.Append("<SectionRef>" + XmlCommon.XmlEncode(s) + "</SectionRef>");
                            if (ImportFileVersion >= 3)
                            {
                                s = exf.GetCell(row, "K");
                                if (s.Length != 0 && !s.StartsWith("/"))
                                {
                                    s = "/" + s;
                                }
                                tmpS.Append("<SectionRef>" + XmlCommon.XmlEncode(s) + "</SectionRef>");
                                s = exf.GetCell(row, "L");
                                if (s.Length != 0 && !s.StartsWith("/"))
                                {
                                    s = "/" + s;
                                }
                                tmpS.Append("<SectionRef>" + XmlCommon.XmlEncode(s) + "</SectionRef>");
                            }
                            tmpS.Append("<Summary>" + XmlCommon.XmlEncode(exf.GetCell(row, "M")) + "</Summary>");
                            tmpS.Append("<Description>" + XmlCommon.XmlEncode(exf.GetCell(row, "N")) + "</Description>");
                            tmpS.Append("<SEKeywords>" + XmlCommon.XmlEncode(exf.GetCell(row, "O")) + "</SEKeywords>");
                            tmpS.Append("<SEDescription>" + XmlCommon.XmlEncode(exf.GetCell(row, "P")) + "</SEDescription>");
                            tmpS.Append("<SETitle>" + XmlCommon.XmlEncode(exf.GetCell(row, "Q")) + "</SETitle>");
                            tmpS.Append("<SKU>" + XmlCommon.XmlEncode(exf.GetCell(row, "R")) + "</SKU>");
                            tmpS.Append("<ManufacturerPartNumber>" + XmlCommon.XmlEncode(exf.GetCell(row, "S")) + "</ManufacturerPartNumber>");
                            tmpS.Append("<XmlPackage>" + exf.GetCell(row, "T") + "</XmlPackage>");
                            tmpS.Append("<ColWidth>" + exf.GetCell(row, "U") + "</ColWidth>");
                            tmpS.Append("<SalesPromptID>" + exf.GetCell(row, "V") + "</SalesPromptID>");
                            tmpS.Append("<Published>" + exf.GetCell(row, "W") + "</Published>");
                            tmpS.Append("<RequiresRegistration>" + exf.GetCell(row, "X") + "</RequiresRegistration>");
                            s = exf.GetCell(row, "Y");
                            foreach (String s2 in s.Split(','))
                            {
                                tmpS.Append("<RelatedProducts>" + XmlCommon.XmlEncode(s2) + "</RelatedProducts>");
                            }
                            tmpS.Append("<MiscText>" + XmlCommon.XmlEncode(exf.GetCell(row, "Z")) + "</MiscText>");
                            String tmpX = exf.GetCell(row, "AA");
                            tmpS.Append("<TrackInventoryBySizeAndColor>" + tmpX + "</TrackInventoryBySizeAndColor>");
                            tmpS.Append("<TrackInventoryBySize>" + tmpX + "</TrackInventoryBySize>"); // this is the correct logic, see bug 167
                            tmpS.Append("<TrackInventoryByColor>" + tmpX + "</TrackInventoryByColor>"); // this is the correct logic, see bug 167
                            if (ImportFileVersion >= 3)
                            {
                                tmpS.Append("<IsAKit>" + exf.GetCell(row, "AB") + "</IsAKit>");
                            }
                            tmpS.Append("<ImageFilenameOverride>" + XmlCommon.XmlEncode(exf.GetCell(row, "AC")) + "</ImageFilenameOverride>");
                            tmpS.Append("<ExtensionData>" + XmlCommon.XmlEncode(exf.GetCell(row, "AD")) + "</ExtensionData>");
                            tmpS.Append("<SEAltText>" + XmlCommon.XmlEncode(exf.GetCell(row, "AE")) + "</SEAltText>");
                        }

                        // col AF is not used!

                        tmpS.Append("<ProductVariant>");
                        tmpS.Append("<Name>" + XmlCommon.XmlEncode(exf.GetCell(row, "AG")) + "</Name>");
                        tmpS.Append("<IsDefault>" + exf.GetCell(row, "AH") + "</IsDefault>");
                        tmpS.Append("<SKUSuffix>" + XmlCommon.XmlEncode(exf.GetCell(row, "AI")) + "</SKUSuffix>");
                        tmpS.Append("<ManufacturerPartNumber>" + XmlCommon.XmlEncode(exf.GetCell(row, "AJ")) + "</ManufacturerPartNumber>");
                        tmpS.Append("<Description>" + XmlCommon.XmlEncode(exf.GetCell(row, "AK")) + "</Description>");
                        tmpS.Append("<SEKeywords>" + XmlCommon.XmlEncode(exf.GetCell(row, "AL")) + "</SEKeywords>");
                        tmpS.Append("<SEDescription>" + XmlCommon.XmlEncode(exf.GetCell(row, "AM")) + "</SEDescription>");
                        tmpS.Append("<SETitle>" + XmlCommon.XmlEncode(exf.GetCell(row, "AN")) + "</SETitle>");
                        tmpS.Append("<Price>" + exf.GetCell(row, "AO") + "</Price>");
                        tmpS.Append("<SalePrice>" + exf.GetCell(row, "AP") + "</SalePrice>");
                        tmpS.Append("<MSRP>" + exf.GetCell(row, "AQ") + "</MSRP>");
                        tmpS.Append("<Cost>" + exf.GetCell(row, "AR") + "</Cost>");
                        tmpS.Append("<Weight>" + exf.GetCell(row, "AS") + "</Weight>");
                        tmpS.Append("<Dimensions>" + XmlCommon.XmlEncode(exf.GetCell(row, "AT")) + "</Dimensions>");
                        tmpS.Append("<Inventory>" + exf.GetCell(row, "AU") + "</Inventory>");
                        tmpS.Append("<DisplayOrder>" + exf.GetCell(row, "AV") + "</DisplayOrder>");
                        tmpS.Append("<Colors>" + XmlCommon.XmlEncode(exf.GetCell(row, "AW")) + "</Colors>");
                        tmpS.Append("<ColorSKUModifiers>" + XmlCommon.XmlEncode(exf.GetCell(row, "AX")) + "</ColorSKUModifiers>");
                        tmpS.Append("<Sizes>" + XmlCommon.XmlEncode(exf.GetCell(row, "AY")) + "</Sizes>");
                        tmpS.Append("<SizeSKUModifiers>" + XmlCommon.XmlEncode(exf.GetCell(row, "AZ")) + "</SizeSKUModifiers>");
                        tmpS.Append("<IsTaxable>" + exf.GetCell(row, "BA") + "</IsTaxable>");
                        tmpS.Append("<IsShipSeparately>" + exf.GetCell(row, "BB") + "</IsShipSeparately>");
                        tmpS.Append("<IsDownload>" + exf.GetCell(row, "BC") + "</IsDownload>");
                        tmpS.Append("<DownloadLocation>" + XmlCommon.XmlEncode(exf.GetCell(row, "BD")) + "</DownloadLocation>");
                        tmpS.Append("<Published>" + exf.GetCell(row, "BE") + "</Published>");
                        tmpS.Append("<ImageFilenameOverride>" + XmlCommon.XmlEncode(exf.GetCell(row, "BF")) + "</ImageFilenameOverride>");
                        tmpS.Append("<ExtensionData>" + XmlCommon.XmlEncode(exf.GetCell(row, "BG")) + "</ExtensionData>");
                        tmpS.Append("<SEAltText>" + XmlCommon.XmlEncode(exf.GetCell(row, "BH")) + "</SEAltText>");
                        tmpS.Append("<Condition>" + exf.GetCell(row, "BI") + "</Condition>");
                        tmpS.Append("<GTIN>" + exf.GetCell(row, "BK") + "</GTIN>");
                        tmpS.Append("</ProductVariant>");


                        String storeMappings = exf.GetCell(row, "BJ");
                        if (!String.IsNullOrEmpty(storeMappings.Trim()))
                        {
                            String[] storeMap = storeMappings.Split(',');
                            tmpS.Append("<StoreMappings>");
                            foreach (String mapping in storeMap)
                            {
                                int storeid;
                                if (!int.TryParse(mapping.Trim(), out storeid))
                                {//not an integer - must be a name
                                    storeid = GetStoreIdByName(mapping.Trim());
                                }
                                if (ValidateStoreId(storeid))
                                    tmpS.Append("<Store StoreId=\"" + storeid + "\" />");
                            }
                            tmpS.Append("</StoreMappings>");
                        }
                    }
                    first = false;
                    LastPName = PName;
                }
            }
            if (!first)
            {
                if (LastPName.ToUpper(CultureInfo.InvariantCulture) == "KITITEM" || LastPName.ToUpper(CultureInfo.InvariantCulture) == "KITGROUP")
                {
                    tmpS.Append("</KitGroup>");
                }
                tmpS.Append("</Product>");
            }
            tmpS.Append("</AspDotNetStorefrontImportFile>");
            return tmpS.ToString();
        }

        private static int GetStoreIdByName(string StoreName)
        {
 	 	    List<Store> stores = Store.GetStoreList();
 	 	    foreach (Store s in stores)
 	 	 	    if (s.Name.EqualsIgnoreCase(StoreName))
 	 	 	        return s.StoreID;
            return 0;
        }

        public static Boolean ValidateStoreId(int StoreId)
        {
            Boolean found = false;
 	 	 	List<Store> stores = Store.GetStoreList();
 	 	 	foreach (Store s in stores)
 	 	 	    if (s.StoreID == StoreId)
 	 	 	        found = true;
 	 	 	return found;
        }

        private static void CheckForRequiredField(XmlNode node, String fieldName)
        {
            XmlNode n = node.SelectSingleNode(@fieldName);
            if (n != null)
            {
                if (n.InnerText.Trim().Length == 0)
                {
                    throw new ArgumentException("Node: " + node.Name + ", Required Field (" + fieldName + ") Is Missing!");
                }
            }
            else
            {
                throw new ArgumentException("Node: " + node.Name + ", Required Field (" + fieldName + ") Is Missing!");
            }
        }

        private static void CheckForRequiredFields(XmlNode node, String fieldList)
        {
            String[] fields = fieldList.Split(',');
            foreach (String s in fields)
            {
                XmlNode n = node.SelectSingleNode(@s.Trim());
                if (n != null)
                {
                    if (n.InnerText.Trim().Length == 0)
                    {
                        throw new ArgumentException("Node: " + node.Name + ", Required Field (" + s.Trim() + ") Is Missing!");
                    }
                }
                else
                {
                    throw new ArgumentException("Node: " + node.Name + ", Required Field (" + s.Trim() + ") Is Missing!");
                }
            }
        }

        private static void RunCommand(StringBuilder StatusList, String cmd)
        {
            StatusList.Append("Executing SQL: " + HttpContext.Current.Server.HtmlEncode(cmd) + "");
            int TimeoutSecs = 480;
            try
            {
                TimeoutSecs = CommonLogic.ApplicationUSInt("SQLCommandTimeoutSecs");
            }
            catch { }
            DB.ExecuteLongTimeSQL(cmd, TimeoutSecs);
        }

        private static void LoadCustomer(StringBuilder StatusList, XmlNode node)
        {
            CheckForRequiredFields(node, "EMail");

            String nodeName = XmlCommon.XmlField(node, "EMail").Trim();
            ProcessingNode = "Customer: " + nodeName;
            StatusList.Append("Adding Customer(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ")");
            if (nodeName.Length == 0)
            {
                StatusList.Append("  >> No EMail...skipping");
            }
            else if (!Customer.NewEmailPassesDuplicationRules(nodeName, 0, false))
            {
                StatusList.Append("  >> Duplicate EMail...skipping");
            }
            else
            {
                try
                {
                    Customer NewCustomer = Customer.CreateNewAnonCustomerObject();

                    String EMailField = nodeName;
                    bool NewEmailAllowed = Customer.NewEmailPassesDuplicationRules(EMailField, NewCustomer.CustomerID, false);

                    string PWD = XmlCommon.XmlField(node, "Password").Trim();
                    Password p = new Password(PWD);
                    String newpwd = p.SaltedPassword;
                    System.Nullable<int> newsaltkey = p.Salt;
                    DateTime dob = XmlCommon.XmlFieldNativeDateTime(node, "DateOfBirth");
                    String dobs = Localization.ToDBDateTimeString(dob);
                    if (XmlCommon.XmlField(node, "DateOfBirth").Length == 0 || dob.Equals(System.DateTime.MinValue))
                    {
                        dobs = null;
                    }
                    bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !AppLogic.AppConfigBool("SkipShippingOnCheckout");

                    int OKM = CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "OkToEMail"), 1, 0);
                    if (XmlCommon.XmlField(node, "OkToEMail").Length == 0)
                    {
                        // something is wrong, just try to find the right case...
                        if (XmlCommon.XmlField(node, "OkToEmail").Length != 0)
                        {
                            OKM = CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "OkToEmail"), 1, 0);
                        }
                        if (XmlCommon.XmlField(node, "OKToEmail").Length != 0)
                        {
                            OKM = CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "OKToEmail"), 1, 0);
                        }
                        if (XmlCommon.XmlField(node, "OKToEMail").Length != 0)
                        {
                            OKM = CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "OKToEMail"), 1, 0);
                        }
                    }

                    NewCustomer.UpdateCustomer(
                        /*CustomerLevelID*/ XmlCommon.XmlFieldUSInt(node, "CustomerLevelID"),
                        /*EMail*/ EMailField,
                        /*SaltedAndHashedPassword*/ newpwd,
                        /*SaltKey*/ newsaltkey,
                        /*DateOfBirth*/ dobs,
                        /*Gender*/ XmlCommon.XmlField(node, "Gender"),
                        /*FirstName*/ XmlCommon.XmlField(node, "FirstName"),
                        /*LastName*/ XmlCommon.XmlField(node, "LastName"),
                        /*Notes*/ XmlCommon.XmlField(node, "Notes"),
                        /*SkinID*/ null,
                        /*Phone*/ XmlCommon.XmlField(node, "Phone"),
                        /*AffiliateID*/ XmlCommon.XmlFieldUSInt(node, "AffiliateID"),
                        /*Referrer*/ null,
                        /*CouponCode*/ null,
                        /*OkToEmail*/ OKM,
                        /*IsAdmin*/ null,
                        /*BillingEqualsShipping*/ CommonLogic.IIF(AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo"), 0, 1),
                        /*LastIPAddress*/ null,
                        /*OrderNotes*/ null,
                        /*SubscriptionExpiresOn*/ null,
                        /*RTShipRequest*/ null,
                        /*RTShipResponse*/ null,
                        /*OrderOptions*/ null,
                        /*LocaleSetting*/ null,
                        /*MicroPayBalance*/ XmlCommon.XmlFieldNativeDecimal(node, "MicropayBalance"),
                        /*RecurringShippingMethodID*/ null,
                        /*RecurringShippingMethod*/ null,
                        /*BillingAddressID*/ null,
                        /*ShippingAddressID*/ null,
                        /*GiftRegistryGUID*/ null,
                        /*GiftRegistryIsAnonymous*/ null,
                        /*GiftRegistryAllowSearchByOthers*/ null,
                        /*GiftRegistryNickName*/ null,
                        /*GiftRegistryHideShippingAddresses*/ null,
                        /*CODCompanyCheckAllowed*/ CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "CODCompanyCheckAllowed"), 1, 0),
                        /*CODNet30Allowed*/ CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "CODNet30Allowed"), 1, 0),
                        /*ExtensionData*/ XmlCommon.XmlField(node, "ExtensionData"),
                        /*FinalizationData*/ null,
                        /*Deleted*/ null,
                        /*Over13Checked*/ CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "Over13Checked"), 1, 0),
                        /*CurrencySetting*/ null,
                        /*VATSetting*/ XmlCommon.XmlFieldUSInt(node, "VATSetting"),
                        /*VATRegistrationID*/ XmlCommon.XmlField(node, "VATRegistrationID"),
                        /*StoreCCInDB*/ CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "StoreCCInDB"), 1, 0),
                        /*IsRegistered*/ CommonLogic.IIF(EMailField.Length != 0 && (PWD.Length != 0), 1, 0),
                        /*LockedUntil*/ null,
                        /*AdminCanViewCC*/ null,
                        /*BadLogin*/ null,
                        /*Active*/ null,
                        /*PwdChangeRequired*/ null,
                        /*RegisterDate*/ null,
                        /*StoreId*/AppLogic.StoreID()
                     );

                    Address BillingAddress = new Address();
                    Address ShippingAddress = new Address();

                    XmlNode BA = node.SelectSingleNode("BillingAddress");
                    XmlNode SA = node.SelectSingleNode("ShippingAddress");

                    BillingAddress.LastName = XmlCommon.XmlField(BA, "LastName");
                    BillingAddress.FirstName = XmlCommon.XmlField(BA, "FirstName");
                    BillingAddress.Phone = XmlCommon.XmlField(BA, "Phone");
                    BillingAddress.Company = XmlCommon.XmlField(BA, "Company");
                    BillingAddress.ResidenceType = (ResidenceTypes)(CommonLogic.IIF(XmlCommon.XmlField(BA, "ResidenceType").Equals("commercial", StringComparison.InvariantCultureIgnoreCase), (int)ResidenceTypes.Commercial, (int)ResidenceTypes.Residential));
                    BillingAddress.Address1 = XmlCommon.XmlField(BA, "Address1");
                    BillingAddress.Address2 = XmlCommon.XmlField(BA, "Address2");
                    BillingAddress.Suite = XmlCommon.XmlField(BA, "Suite");
                    BillingAddress.City = XmlCommon.XmlField(BA, "City");
                    BillingAddress.State = XmlCommon.XmlField(BA, "State");
                    BillingAddress.Zip = XmlCommon.XmlField(BA, "Zip");
                    BillingAddress.Country = XmlCommon.XmlField(BA, "Country");
                    BillingAddress.EMail = XmlCommon.XmlField(BA, "EMail");

                    BillingAddress.InsertDB(NewCustomer.CustomerID);
                    BillingAddress.MakeCustomersPrimaryAddress(AddressTypes.Billing);

                    if (AllowShipToDifferentThanBillTo)
                    {
                        ShippingAddress.LastName = XmlCommon.XmlField(SA, "LastName");
                        ShippingAddress.FirstName = XmlCommon.XmlField(SA, "FirstName");
                        ShippingAddress.Phone = XmlCommon.XmlField(SA, "Phone");
                        ShippingAddress.Company = XmlCommon.XmlField(SA, "Company");
                        ShippingAddress.ResidenceType = (ResidenceTypes)(CommonLogic.IIF(XmlCommon.XmlField(SA, "ResidenceType").Equals("commercial", StringComparison.InvariantCultureIgnoreCase), (int)ResidenceTypes.Commercial, (int)ResidenceTypes.Residential));
                        ShippingAddress.Address1 = XmlCommon.XmlField(SA, "Address1");
                        ShippingAddress.Address2 = XmlCommon.XmlField(SA, "Address2");
                        ShippingAddress.Suite = XmlCommon.XmlField(SA, "Suite");
                        ShippingAddress.City = XmlCommon.XmlField(SA, "City");
                        ShippingAddress.State = XmlCommon.XmlField(SA, "State");
                        ShippingAddress.Zip = XmlCommon.XmlField(SA, "Zip");
                        ShippingAddress.Country = XmlCommon.XmlField(SA, "Country");
                        ShippingAddress.EMail = XmlCommon.XmlField(SA, "EMail");

                        ShippingAddress.InsertDB(NewCustomer.CustomerID);
                        ShippingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                    }
                    else
                    {
                        BillingAddress.MakeCustomersPrimaryAddress(AddressTypes.Shipping);
                    }

                    String vtr = XmlCommon.XmlField(node, "VatRegistrationID");
					Exception vatServiceException;
					if (!AppLogic.AppConfigBool("VAT.Enabled") || !AppLogic.VATRegistrationIDIsValid(NewCustomer, vtr, out vatServiceException))
                    {
                        vtr = String.Empty;
                    }
                    NewCustomer.SetVATRegistrationID(vtr);
                    StatusList.Append("  >> OK" + "");
                }
                catch (Exception ex)
                {
                    StatusList.Append("ERROR ADDING CUSTOMER:" + CommonLogic.GetExceptionDetail(ex, String.Empty) + "");
                }
            }
        }

        private static void LoadProductType(StringBuilder StatusList, XmlNode node)
        {
            CheckForRequiredFields(node, "Name");

            String nodeName = XmlCommon.XmlField(node, "Name");
            ProcessingNode = "ProductType:" + nodeName;
            StatusList.Append("Adding ProductType(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ")" + "");
            if (nodeName.Length == 0)
            {
                StatusList.Append("  >> No product type name...skipping" + "");
            }
            else if (CheckForProductType(nodeName) != 0)
            {
                StatusList.Append("  >> Duplicate product type...skipping" + "");
            }
            else
            {
                StringBuilder sql = new StringBuilder(2500);
                String NewGUID = CommonLogic.GetNewGUID();
                sql.Append("insert producttype(producttypeGUID,Name) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(CommonLogic.Left(nodeName, 100)));
                sql.Append(")");
                RunCommand(StatusList, sql.ToString());
                StatusList.Append("  >> OK" + "");
            }
        }

        private static void LoadEntity(String EntityName, StringBuilder StatusList, XmlNode node)
        {
            CheckForRequiredFields(node, "Name");
            String nodeName = XmlCommon.XmlField(node, "Name");
            ProcessingNode = EntityName + ":" + nodeName;
            StatusList.Append("Adding " + EntityName + "(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ")" + "");
            if (nodeName.Length == 0)
            {
                StatusList.Append("  >> No " + EntityName + " name...skipping" + "");
            }
            else if (CheckForEntity(EntityName, nodeName, false) != 0)
            {
                StatusList.Append("  >> Duplicate " + EntityName + "...skipping" + "");
            }
            else
            {
                bool entityImportIsRestricted = false;
                entityImportIsRestricted = AppLogic.MaxEntitiesExceeded();
                if (!entityImportIsRestricted)
                {
                    StringBuilder sql = new StringBuilder(2500);
                    String NewGUID = CommonLogic.GetNewGUID();
                    sql.Append("insert ^(^GUID,IsImport,Name,SEName,ImageFilenameOverride,Parent^ID,Summary,Description,SEKeywords,SEDescription,SETitle,Published,ColWidth,XmlPackage");
                    if (EntityDefinitions.LookupSpecs(EntityName).m_HasAddress)
                    {
                        sql.Append(",Address1, Address2, Suite, City, State, ZipCode, Country, Notes, Phone, FAX, URL, Email) values(");
                    }
                    else
                    {
                        sql.Append(") values(");
                    }
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append("1,"); // Set IsImport Flag For later Undo of Import
                    sql.Append(DB.SQuote(CommonLogic.Left(nodeName, 400)) + ",");
                    sql.Append(DB.SQuote(CommonLogic.Left(SE.MungeName(nodeName), 100)) + ",");
                    if (XmlCommon.XmlField(node, "ImageFilenameOverride").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ImageFilenameOverride")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    int ParentEntityID = 0;
                    if (XmlCommon.XmlField(node, "Parent" + EntityName).Length != 0)
                    {
                        ParentEntityID = CheckForEntity(EntityName, XmlCommon.XmlField(node, "Parent" + EntityName), true);
                    }
                    sql.Append(CommonLogic.IIF(ParentEntityID == 0, "0", ParentEntityID.ToString()) + ",");
                    if (XmlCommon.XmlField(node, "Summary").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Summary")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "Description").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SEKeywords").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SEKeywords")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SEDescription").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SEDescription")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SETitle").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SETitle")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "Published").Length == 0, 1, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "Published"), 1, 0)) + ",");
                    sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "ColWidth").Length == 0, 4, XmlCommon.XmlFieldUSInt(node, "ColWidth")) + ",");
                    sql.Append(DB.SQuote((CommonLogic.IIF(XmlCommon.XmlField(node, "XmlPackage").Length == 0, AppLogic.ro_DefaultEntityXmlPackage, XmlCommon.XmlField(node, "XmlPackage").ToLowerInvariant()))));

                    if (EntityDefinitions.LookupSpecs(EntityName).m_HasAddress)
                    {
                        sql.Append(",");
                        if (XmlCommon.XmlField(node, "Address1").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Address1")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "Address2").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Address2")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "Suite").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Suite")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "City").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "City")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "State").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "State")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "ZipCode").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ZipCode")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "Country").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Country")) + ",");
                        }
                        else
                        {
                            sql.Append("'US',");
                        }
                        if (XmlCommon.XmlField(node, "Notes").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Notes")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "Phone").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Phone")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "FAX").Length != 0)
                        {
                            sql.Append(DB.SQuote(XmlCommon.XmlField(node, "FAX")) + ",");
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "URL").Length != 0)
                        {
                            String theUrl = CommonLogic.Left(XmlCommon.XmlField(node, "URL"), 80);
                            if (theUrl.IndexOf("http://") == -1 && theUrl.Length != 0)
                            {
                                theUrl = "http://" + theUrl;
                            }
                            if (theUrl.Length == 0)
                            {
                                sql.Append("NULL,");
                            }
                            else
                            {
                                sql.Append(DB.SQuote(theUrl) + ",");
                            }
                        }
                        else
                        {
                            sql.Append("NULL,");
                        }
                        if (XmlCommon.XmlField(node, "EMail").Length != 0)
                        {
                            sql.Append(DB.SQuote(CommonLogic.Left(XmlCommon.XmlField(node, "EMail"), 100)));
                        }
                        else
                        {
                            sql.Append("NULL");
                        }
                    }

                    if (sql.ToString().EndsWith(","))
                    {
                        sql.Length = sql.Length - 1;
                    }

                    sql.Append(")");
                    sql = sql.Replace("^", EntityName);
                    RunCommand(StatusList, sql.ToString());
                    StatusList.Append("  >> OK" + "");
                }
                else
                {
                    StatusList.Append("You cannot import any more entities, because you have reached the limit allowed with ML/Express.");
                }
            }
        }

        private static void LoadEntityTree(String EntityName, StringBuilder StatusList, XmlNode node)
        {

            String nodeName = node.InnerText;
            ProcessingNode = EntityName + " Tree:" + nodeName;
            StatusList.Append("Adding " + EntityName + "Tree(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ")" + "");
            if (nodeName.Length == 0)
            {
                StatusList.Append("  >> No " + EntityName + " tree name..." + "");
                return;
            }
            int ParentEntityID = 0;
            if (nodeName.StartsWith("//"))
            {
                nodeName = nodeName.Substring(2, nodeName.Length - 2);
            }
            if (nodeName.StartsWith("/"))
            {
                nodeName = nodeName.Substring(1, nodeName.Length - 1);
            }
            if (nodeName.EndsWith("/"))
            {
                nodeName = nodeName.Substring(0, nodeName.Length - 1);
            }
            nodeName = nodeName.Trim();
            foreach (String s in nodeName.Split('/'))
            {
                int PIC = CheckForEntity(EntityName, s, ParentEntityID);
                if (PIC != 0)
                {
                    ParentEntityID = PIC;
                }
                else
                {
                    StringBuilder sql = new StringBuilder(2500);
                    String NewGUID = CommonLogic.GetNewGUID();
                    sql.Append("insert ^(^GUID,IsImport,Name,Parent^ID,SEname) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append("1,"); // Set IsImport Flag For later Undo of Import
                    sql.Append(DB.SQuote(CommonLogic.Left(s, 100)) + ",");
                    sql.Append(ParentEntityID.ToString() + ",");
                    sql.Append(DB.SQuote(SE.MungeName(CommonLogic.Left(s, 100))));
                    sql.Append(")");
                    sql = sql.Replace("^", EntityName);
                    RunCommand(StatusList, sql.ToString());

                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select * from " + EntityName + "  with (NOLOCK)  where Deleted=0 and Name=" + DB.SQuote(s) + " and Parent" + EntityName + "ID=" + ParentEntityID.ToString(), dbconn))
                        {
                            rs.Read();
                            ParentEntityID = DB.RSFieldInt(rs, EntityName + "ID");
                        }
                    }

                    StatusList.Append("  >> OK" + "");
                }
            }
        }

        private static void LoadProduct(StringBuilder StatusList, XmlNode node)
        {
            CheckForRequiredFields(node, "Name,ProductTypeRef,ManufacturerRef");
            String nodeName = XmlCommon.XmlField(node, "Name");
            ProcessingNode = "Product:" + nodeName;
            if (nodeName.Length == 0)
            {
                StatusList.Append("  >> No product name...skipping" + "");
            }
            else
            {
                int ProductID = 0;
                // check for existing product by matching a non-blank SKU. If SKU is blank or null (it's an optional field), match by Name
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rsp = DB.GetRS("select ProductID from dbo.Product with (NOLOCK) where Deleted=0 and ((len(SKU)>0 and SKU=" + DB.SQuote(XmlCommon.XmlField(node, "SKU")) + ") or (len(isnull(SKU,''))=0 and Name=" + DB.SQuote(nodeName) + "))", dbconn))
                    {
                        if (rsp.Read())
                        {
                            ProductID = DB.RSFieldInt(rsp, "ProductID");
                        }
                    }
                }
                bool WasJustAddedProduct = true;

                bool TrackInventoryBySizeAndColor = (XmlCommon.XmlField(node, "TrackInventoryBySizeAndColor").Length != 0 && XmlCommon.XmlFieldBool(node, "TrackInventoryBySizeAndColor"));
                if (ProductID == 0)
                {
                    // add the product:
                    StatusList.Append("Adding Product(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ")" + "");

                    StringBuilder sql = new StringBuilder(2500);

                    // ok to add them:
                    String NewGUID = CommonLogic.GetNewGUID();
                    sql.Append("insert product(ProductGUID,IsImport,IsAKit,Name,SEName,ImageFilenameOverride,ProductTypeID,RelatedProducts,Summary,Description,SEKeywords,SEDescription,SETitle,SKU,ManufacturerPartNumber,ColWidth,XmlPackage,SalesPromptID,Published,TrackInventoryBySizeAndColor,TrackInventoryBySize,TrackInventoryByColor,RequiresRegistration,MiscText,ExtensionData,SEAltText) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append("1,"); // Set IsImport Flag For later Undo of Import
                    sql.Append(CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsAKit"), "1", "0") + ",");
                    sql.Append(DB.SQuote(CommonLogic.Left(nodeName, 400)) + ",");
                    sql.Append(DB.SQuote(CommonLogic.Left(SE.MungeName(nodeName), 150)) + ",");
                    if (XmlCommon.XmlField(node, "ImageFilenameOverride").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ImageFilenameOverride")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }

                    int PTypeID = 1;
                    if (XmlCommon.XmlField(node, "ProductTypeRef").Length != 0)
                    {
                        PTypeID = CheckForProductType(XmlCommon.XmlField(node, "ProductTypeRef"));
                        if (PTypeID == 0)
                        {
                            PTypeID = CheckForProductType("Type 1");
                        }
                        if (PTypeID == 0)
                        {
                            PTypeID = 8; // Pick the default
                        }
                    }
                    sql.Append(PTypeID + ",");
                    sql.Append("'',"); // empty related products for now (but not NULL)
                    if (XmlCommon.XmlField(node, "Summary").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Summary")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    String D = XmlCommon.XmlField(node, "Description").Trim();
                    if (D.Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                    }
                    else
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Summary")) + ",");
                    }
                    if (XmlCommon.XmlField(node, "SEKeywords").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SEKeywords")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SEDescription").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SEDescription")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SETitle").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SETitle")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SKU").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SKU")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "ManufacturerPartNumber").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ManufacturerPartNumber")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "ColWidth").Length == 0, 4, XmlCommon.XmlFieldUSInt(node, "ColWidth")) + ",");
                    String DefaultXmlPN = AppLogic.ro_DefaultProductXmlPackage;
                    if (XmlCommon.XmlFieldBool(node, "IsAKit"))
                    {
                        DefaultXmlPN = AppLogic.ro_DefaultProductKitXmlPackage;
                    }
                    sql.Append(DB.SQuote(CommonLogic.IIF(XmlCommon.XmlField(node, "XmlPackage").Length == 0, DefaultXmlPN, XmlCommon.XmlField(node, "XmlPackage").ToLowerInvariant())) + ",");
                    sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "SalesPromptID").Length == 0, 1, XmlCommon.XmlFieldUSInt(node, "SalesPromptID")) + ",");
                    sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "Published").Length == 0, 1, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "Published"), 1, 0)) + ",");
                    sql.Append(CommonLogic.IIF(TrackInventoryBySizeAndColor, 1, 0).ToString() + ",");
                    sql.Append(CommonLogic.IIF(TrackInventoryBySizeAndColor, 1, 0).ToString() + ","); // this is the correct logic now, see bug 167
                    sql.Append(CommonLogic.IIF(TrackInventoryBySizeAndColor, 1, 0).ToString() + ","); // this is the correct logic now, see bug 167
                    sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "RequiresRegistration").Length == 0, 0, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "RequiresRegistration"), 1, 0)) + ",");
                    if (XmlCommon.XmlField(node, "MiscText").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "MiscText")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "ExtensionData").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ExtensionData")) + ",");
                    }
                    else
                    {
                        sql.Append("NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SEAltText").Length != 0)
                    {
                        sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SEAltText")));
                    }
                    else
                    {
                        sql.Append(DB.SQuote(CommonLogic.Left(nodeName, 400)) + "");
                    }
                    sql.Append(")");
                    RunCommand(StatusList, sql.ToString());
                    StatusList.Append("  >> Added OK" + "");

                    using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                    {
                        dbconn.Open();
                        using (IDataReader rs = DB.GetRS("select ProductID from Product  with (NOLOCK)  where Deleted=0 and ProductGUID=" + DB.SQuote(NewGUID), dbconn))
                        {
                            rs.Read();
                            ProductID = DB.RSFieldInt(rs, "ProductID");
                        }
                    }

                    XmlNodeList RelatedList = node.SelectNodes(@"RelatedProducts");
                    foreach (XmlNode RelatedNode in RelatedList)
                    {
                        if (RelatedNode.InnerText.Length != 0)
                        {
                            RelatedProductsToDoList.Append(ProductID + "~" + RelatedNode.InnerText + "|");
                        }
                    }
                }
                else
                {
                    // update the product info
                    WasJustAddedProduct = false;
                    StatusList.Append("Updating Product(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ")" + "");

                    StringBuilder sql = new StringBuilder(2500);

                    sql.Append("update product set ");
                    sql.Append("Name=" + DB.SQuote(CommonLogic.Left(nodeName, 400)) + ",");
                    sql.Append("SEName=" + DB.SQuote(CommonLogic.Left(SE.MungeName(nodeName), 150)) + ",");
                    if (XmlCommon.XmlField(node, "ImageFilenameOverride").Length != 0)
                    {
                        sql.Append("ImageFilenameOverride=" + DB.SQuote(XmlCommon.XmlField(node, "ImageFilenameOverride")) + ",");
                    }
                    else
                    {
                        sql.Append("ImageFilenameOverride=NULL,");
                    }

                    int PTypeID = 1;
                    if (XmlCommon.XmlField(node, "ProductTypeRef").Length != 0)
                    {
                        PTypeID = CheckForProductType(XmlCommon.XmlField(node, "ProductTypeRef"));
                        if (PTypeID == 0)
                        {
                            PTypeID = CheckForProductType("Type 1");
                        }
                        if (PTypeID == 0)
                        {
                            PTypeID = 8; // Pick the default
                        }
                    }
                    sql.Append("ProductTypeID=" + PTypeID.ToString() + ",");
                    if (XmlCommon.XmlField(node, "Summary").Length != 0)
                    {
                        sql.Append("Summary=" + DB.SQuote(XmlCommon.XmlField(node, "Summary")) + ",");
                    }
                    else
                    {
                        sql.Append("Summary=NULL,");
                    }
                    String D = XmlCommon.XmlField(node, "Description").Trim();
                    if (D.Length != 0)
                    {
                        sql.Append("Description=" + DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                    }
                    else
                    {
                        sql.Append("Description=NULL" + ",");
                    }
                    if (XmlCommon.XmlField(node, "SEKeywords").Length != 0)
                    {
                        sql.Append("SEKeywords=" + DB.SQuote(XmlCommon.XmlField(node, "SEKeywords")) + ",");
                    }
                    else
                    {
                        sql.Append("SEKeywords=NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SEDescription").Length != 0)
                    {
                        sql.Append("SEDescription=" + DB.SQuote(XmlCommon.XmlField(node, "SEDescription")) + ",");
                    }
                    else
                    {
                        sql.Append("SEDescription=NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SETitle").Length != 0)
                    {
                        sql.Append("SETitle=" + DB.SQuote(XmlCommon.XmlField(node, "SETitle")) + ",");
                    }
                    else
                    {
                        sql.Append("SETitle=NULL,");
                    }
                    if (XmlCommon.XmlField(node, "ManufacturerPartNumber").Length != 0)
                    {
                        sql.Append("ManufacturerPartNumber=" + DB.SQuote(XmlCommon.XmlField(node, "ManufacturerPartNumber")) + ",");
                    }
                    else
                    {
                        sql.Append("ManufacturerPartNumber=NULL,");
                    }
                    sql.Append("ColWidth=" + CommonLogic.IIF(XmlCommon.XmlField(node, "ColWidth").Length == 0, 4, XmlCommon.XmlFieldUSInt(node, "ColWidth")) + ",");
                    sql.Append("XmlPackage=" + DB.SQuote(CommonLogic.IIF(XmlCommon.XmlField(node, "XmlPackage").Length == 0, AppLogic.ro_DefaultEntityXmlPackage, XmlCommon.XmlField(node, "XmlPackage").ToLowerInvariant())) + ",");
                    sql.Append("IsAKit=" + CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsAKit"), "1", "0") + ",");
                    sql.Append("SalesPromptID=" + CommonLogic.IIF(XmlCommon.XmlField(node, "SalesPromptID").Length == 0, 1, XmlCommon.XmlFieldUSInt(node, "SalesPromptID")) + ",");
                    sql.Append("Published=" + CommonLogic.IIF(XmlCommon.XmlField(node, "Published").Length == 0, 1, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "Published"), 1, 0)) + ",");
                    sql.Append("TrackInventoryBySizeAndColor=" + CommonLogic.IIF(TrackInventoryBySizeAndColor, 1, 0).ToString() + ",");
                    sql.Append("TrackInventoryBySize=" + CommonLogic.IIF(TrackInventoryBySizeAndColor, 1, 0).ToString() + ","); // this is the correct logic now, see bug 167
                    sql.Append("TrackInventoryByColor=" + CommonLogic.IIF(TrackInventoryBySizeAndColor, 1, 0).ToString() + ","); // this is the correct logic now, see bug 167
                    sql.Append("RequiresRegistration=" + CommonLogic.IIF(XmlCommon.XmlField(node, "RequiresRegistration").Length == 0, 0, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "RequiresRegistration"), 1, 0)) + ",");
                    if (XmlCommon.XmlField(node, "MiscText").Length != 0)
                    {
                        sql.Append("MiscText=" + DB.SQuote(XmlCommon.XmlField(node, "MiscText")) + ",");
                    }
                    else
                    {
                        sql.Append("MiscText=NULL,");
                    }
                    if (XmlCommon.XmlField(node, "ExtensionData").Length != 0)
                    {
                        sql.Append("ExtensionData=" + DB.SQuote(XmlCommon.XmlField(node, "ExtensionData")) + ",");
                    }
                    else
                    {
                        sql.Append("ExtensionData=NULL,");
                    }
                    if (XmlCommon.XmlField(node, "SEAltText").Length != 0)
                    {
                        sql.Append("SEAltText=" + DB.SQuote(XmlCommon.XmlField(node, "SEAltText")));
                    }
                    else
                    {
                        sql.Append("SEAltText=" + DB.SQuote(CommonLogic.Left(nodeName, 400)) + "");
                    }
                    sql.Append(" where ProductID=" + ProductID.ToString());

                    RunCommand(StatusList, sql.ToString());
                    StatusList.Append("  >> Updated OK" + "");
                }

                // update entity mappings:
                foreach (String EntityName in AppLogic.ro_SupportedEntities)
                {
                    XmlNodeList EntityList = node.SelectNodes(EntityName + "Ref");
                    foreach (XmlNode EntityNode in EntityList)
                    {
                        if (EntityNode.InnerText.Length != 0)
                        {

                            int EntityID = CheckForEntity(EntityName, EntityNode.InnerText, true);
                            int ProductIDCheck = 0;

                            // check if product exists in the entities specified
                            if (EntityName == "Manufacturer" || EntityName == "Distributor")
                            {
                                ProductIDCheck = CheckForProductID(EntityName, ProductID);
                            }
                            if (EntityID != 0)
                            {
                                try
                                {
                                    // add(new) or update(exisiting) entity mappings
                                    if (ProductIDCheck == 0)
                                    {
                                        RunCommand(StatusList, "insert product" + EntityName + "(productid," + EntityName + "id) values(" + ProductID + "," + EntityID + ")");
                                    }
                                    else
                                    {
                                        RunCommand(StatusList, "update product" + EntityName + " set " + EntityName + "id = " + EntityID + " where productid =" + ProductID);
                                    }
                                }
                                catch
                                {
                                    StatusList.Append("Ignoring bad/duplicate " + EntityName + "Ref, Name=" + HttpContext.Current.Server.HtmlEncode(EntityNode.InnerText) + "");
                                }
                            }
                            else
                            {
                                StatusList.Append("Ignoring bad " + EntityName + "Ref, Name=" + HttpContext.Current.Server.HtmlEncode(EntityNode.InnerText) + "");
                            }
                        }
                    }
                }

                //update store mappings
                XmlNode StoreNode = node.SelectSingleNode("StoreMappings");
                if (node.SelectSingleNode("StoreMappings") != null)
                {
                    XmlNodeList StoreList = node.SelectSingleNode("StoreMappings").SelectNodes("Store");
                    if (StoreList.Count > 0)
                    {
                        RunCommand(StatusList, String.Format("delete from productstore where productid = {0}", ProductID));
                        foreach (XmlNode storeNode in StoreList)
                        {
                            int nodeStoreID;
                            if (storeNode.Attributes["StoreId"] != null && int.TryParse(storeNode.Attributes["StoreId"].Value, out nodeStoreID))
                            {
                                RunCommand(StatusList, string.Format("insert into productstore (ProductID, StoreID) values ({0}, {1})", ProductID, nodeStoreID));
                            }
                        }
                    }
                }

                // add/update product variants:
                XmlNodeList VariantsList = node.SelectNodes(@"ProductVariant");
                foreach (XmlNode VariantNode in VariantsList)
                {
                    LoadProductVariant(StatusList, VariantNode, nodeName, ProductID, WasJustAddedProduct, TrackInventoryBySizeAndColor);
                }

                // add/update Kit Groups & Items:
                XmlNodeList KitGroupList = node.SelectNodes(@"KitGroup");
                foreach (XmlNode KitGroupNode in KitGroupList)
                {
                    LoadKitGroup(StatusList, KitGroupNode, nodeName, ProductID);
                }

                AppLogic.EnsureProductHasADefaultVariantSet(ProductID);

            }
        }

        private static void LoadProductVariant(StringBuilder StatusList, XmlNode node, String ProductName, int ProductID, bool WasJustAddedProduct, bool TrackInventoryBySizeAndColor)
        {
            CheckForRequiredFields(node, "Price");
            String nodeName = XmlCommon.XmlField(node, "Name");
            ProcessingNode = "ProductVariant:" + nodeName;
            StringBuilder sql = new StringBuilder(2500);

            decimal Price = XmlCommon.XmlFieldNativeDecimal(node, "Price");
            decimal SalePrice = XmlCommon.XmlFieldNativeDecimal(node, "SalePrice");
            decimal MSRP = XmlCommon.XmlFieldNativeDecimal(node, "MSRP");
            decimal Cost = XmlCommon.XmlFieldNativeDecimal(node, "Cost");

            int VariantID = 0;
            // check for existing variant match:
            if (!WasJustAddedProduct)
            {
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rsp = DB.GetRS("select VariantID from ProductVariant  with (NOLOCK)  where Deleted=0 and ProductID=" + ProductID.ToString() + " and Name=" + DB.SQuote(nodeName) + CommonLogic.IIF(XmlCommon.XmlField(node, "SKUSuffix").Length != 0, " and SKUSuffix=" + DB.SQuote(XmlCommon.XmlField(node, "SKUSuffix")), " and (SKUSuffix='' or SKUSuffix IS NULL)"), dbconn))
                    {
                        if (rsp.Read())
                        {
                            VariantID = DB.RSFieldInt(rsp, "VariantID");
                        }
                    }

                }

            }

            if (VariantID == 0)
            {
                // add the variant:
                StatusList.Append("Adding ProductVariant(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ", Product=" + HttpContext.Current.Server.HtmlEncode(ProductName) + ")" + "");

                String NewGUID = CommonLogic.GetNewGUID();
                sql.Append("insert productvariant(VariantGUID,IsImport,ProductID,Name,ImageFilenameOverride,IsDefault,DisplayOrder,Description,Price,SalePrice,MSRP,Cost,SKUSuffix,ManufacturerPartNumber,Weight,Dimensions,Inventory,Published,Colors,ColorSKUModifiers,Sizes,SizeSKUModifiers,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,ExtensionData,SEAltText,Condition,GTIN) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append("1,"); // Set IsImport Flag For later Undo of Import
                sql.Append(ProductID + ",");
                sql.Append(DB.SQuote(nodeName) + ",");
                if (XmlCommon.XmlField(node, "ImageFilenameOverride").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ImageFilenameOverride")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "IsDefault").Length == 0, 0, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsDefault"), 1, 0)) + ",");
                if (XmlCommon.XmlFieldUSInt(node, "IsDefault") == 1)
                {
                    DB.ExecuteSQL("UPDATE ProductVariant SET IsDefault=0 WHERE ProductID=" + ProductID);
                }
                sql.Append(CommonLogic.IIF(XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0, 1, XmlCommon.XmlFieldUSInt(node, "DisplayOrder")) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(Localization.DecimalStringForDB(Price) + ",");
                sql.Append(CommonLogic.IIF(SalePrice != 0.0M, Localization.DecimalStringForDB(SalePrice), "NULL") + ",");
                sql.Append(CommonLogic.IIF(MSRP != 0.0M, Localization.DecimalStringForDB(MSRP), "NULL") + ",");
                sql.Append(CommonLogic.IIF(Cost != 0.0M, Localization.DecimalStringForDB(Cost), "NULL") + ",");
                sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SKUSuffix")) + ",");
                if (XmlCommon.XmlField(node, "ManufacturerPartNumber").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ManufacturerPartNumber")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "Weight").Length == 0 || !CommonLogic.IsNumber(XmlCommon.XmlField(node, "Weight")), "NULL", Localization.DecimalStringForDB(Localization.ParseNativeDecimal(XmlCommon.XmlField(node, "Weight")))) + ",");
                if (XmlCommon.XmlField(node, "Dimensions").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Dimensions")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "Inventory").Length == 0, 1000000, XmlCommon.XmlFieldUSInt(node, "Inventory")) + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "Published").Length == 0, 1, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "Published"), 1, 0)) + ",");
                sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Colors").Replace(", ", ",").Replace(" ,", ",")) + ",");
                sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ColorSKUModifiers").Replace(", ", ",").Replace(" ,", ",")) + ",");
                sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Sizes").Replace(", ", ",").Replace(" ,", ",")) + ",");
                sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SizeSKUModifiers").Replace(", ", ",").Replace(" ,", ",")) + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "IsTaxable").Length == 0, 1, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsTaxable"), 1, 0)) + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "IsShipSeparately").Length == 0, 0, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsShipSeparately"), 1, 0)) + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "IsDownload").Length == 0, 0, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsDownload"), 1, 0)) + ",");
                if (XmlCommon.XmlField(node, "DownloadLocation").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "DownloadLocation")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (XmlCommon.XmlField(node, "ExtensionData").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "ExtensionData")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                if (XmlCommon.XmlField(node, "SEAltText").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "SEAltText")) + ",");
                }
                else
                {
                    sql.Append(DB.SQuote(CommonLogic.Left(nodeName, 400)) + ",");
                }
                sql.Append(CommonLogic.IIF(XmlCommon.XmlField(node, "Condition").Length == 0, 0, XmlCommon.XmlFieldNativeInt(node, "Condition")) + ",");
				sql.Append(DB.SQuote(XmlCommon.XmlField(node, "GTIN")));
                sql.Append(")");
                RunCommand(StatusList, sql.ToString());

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("Select VariantID from ProductVariant where VariantGUID=" + DB.SQuote(NewGUID), dbconn))
                    {
                        if (rs.Read())
                        {
                            VariantID = DB.RSFieldInt(rs, "VariantID");
                        }
                    }
                }

                StatusList.Append("  >> Added OK" + "");
            }
            else
            {
                // update the variant
                StatusList.Append("Updating ProductVariant(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ", Product=" + HttpContext.Current.Server.HtmlEncode(ProductName) + ")" + "");

                sql.Append("update productvariant set ");
                sql.Append("Name=" + DB.SQuote(nodeName) + ",");
                if (XmlCommon.XmlField(node, "ImageFilenameOverride").Length != 0)
                {
                    sql.Append("ImageFilenameOverride=" + DB.SQuote(XmlCommon.XmlField(node, "ImageFilenameOverride")) + ",");
                }
                else
                {
                    sql.Append("ImageFilenameOverride=NULL,");
                }
                sql.Append("DisplayOrder=" + CommonLogic.IIF(XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0, 1, XmlCommon.XmlFieldUSInt(node, "DisplayOrder")) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append("Description=" + DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("Description=NULL,");
                }
                sql.Append("Price=" + Localization.DecimalStringForDB(Price) + ",");
                sql.Append("SalePrice=" + CommonLogic.IIF(SalePrice != 0.0M, Localization.DecimalStringForDB(SalePrice), "NULL") + ",");
                sql.Append("MSRP=" + CommonLogic.IIF(MSRP != 0.0M, Localization.DecimalStringForDB(MSRP), "NULL") + ",");
                sql.Append("Cost=" + CommonLogic.IIF(Cost != 0.0M, Localization.DecimalStringForDB(Cost), "NULL") + ",");
                if (XmlCommon.XmlField(node, "SKUSuffix").Length != 0)
                {
                    sql.Append("SKUSuffix=" + DB.SQuote(XmlCommon.XmlField(node, "SKUSuffix")) + ",");
                }
                else
                {
                    sql.Append("SKUSuffix=NULL,");
                }
                if (XmlCommon.XmlField(node, "ManufacturerPartNumber").Length != 0)
                {
                    sql.Append("ManufacturerPartNumber=" + DB.SQuote(XmlCommon.XmlField(node, "ManufacturerPartNumber")) + ",");
                }
                else
                {
                    sql.Append("ManufacturerPartNumber=NULL,");
                }
                sql.Append("Weight=" + CommonLogic.IIF(XmlCommon.XmlField(node, "Weight").Length == 0 || !CommonLogic.IsNumber(XmlCommon.XmlField(node, "Weight")), "NULL", Localization.DecimalStringForDB(Localization.ParseNativeDecimal(XmlCommon.XmlField(node, "Weight")))) + ",");
                if (XmlCommon.XmlField(node, "Dimensions").Length != 0)
                {
                    sql.Append("Dimensions=" + DB.SQuote(XmlCommon.XmlField(node, "Dimensions")) + ",");
                }
                else
                {
                    sql.Append("Dimensions=NULL,");
                }
                sql.Append("Inventory=" + CommonLogic.IIF(XmlCommon.XmlField(node, "Inventory").Length == 0, 1000000, XmlCommon.XmlFieldUSInt(node, "Inventory")) + ",");
                sql.Append("Published=" + CommonLogic.IIF(XmlCommon.XmlField(node, "Published").Length == 0, 1, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "Published"), 1, 0)) + ",");
                sql.Append("Colors=" + DB.SQuote(XmlCommon.XmlField(node, "Colors").Replace(", ", ",").Replace(" ,", ",")) + ",");
                sql.Append("ColorSKUModifiers=" + DB.SQuote(XmlCommon.XmlField(node, "ColorSKUModifiers").Replace(", ", ",").Replace(" ,", ",")) + ",");
                sql.Append("Sizes=" + DB.SQuote(XmlCommon.XmlField(node, "Sizes").Replace(", ", ",").Replace(" ,", ",")) + ",");
                sql.Append("SizeSKUModifiers=" + DB.SQuote(XmlCommon.XmlField(node, "SizeSKUModifiers").Replace(", ", ",").Replace(" ,", ",")) + ",");
                sql.Append("IsTaxable=" + CommonLogic.IIF(XmlCommon.XmlField(node, "IsTaxable").Length == 0, 1, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsTaxable"), 1, 0)) + ",");
                sql.Append("IsShipSeparately=" + CommonLogic.IIF(XmlCommon.XmlField(node, "IsShipSeparately").Length == 0, 0, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsShipSeparately"), 1, 0)) + ",");
                sql.Append("IsDownload=" + CommonLogic.IIF(XmlCommon.XmlField(node, "IsDownload").Length == 0, 0, CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsDownload"), 1, 0)) + ",");
                if (XmlCommon.XmlField(node, "DownloadLocation").Length != 0)
                {
                    sql.Append("DownloadLocation=" + DB.SQuote(XmlCommon.XmlField(node, "DownloadLocation")) + ",");
                }
                else
                {
                    sql.Append("DownloadLocation=NULL,");
                }
                if (XmlCommon.XmlField(node, "ExtensionData").Length != 0)
                {
                    sql.Append("ExtensionData=" + DB.SQuote(XmlCommon.XmlField(node, "ExtensionData")) + ",");
                }
                else
                {
                    sql.Append("ExtensionData=NULL,");
                }
                if (XmlCommon.XmlField(node, "SEAltText").Length != 0)
                {
                    sql.Append("SEAltText=" + DB.SQuote(XmlCommon.XmlField(node, "SEAltText")) + ",");
                }
                else
                {
                    sql.Append("SEAltText=" + DB.SQuote(CommonLogic.Left(nodeName, 400)) + ",");
                }
                sql.Append("Condition=" + CommonLogic.IIF(XmlCommon.XmlField(node, "Condition").Length == 0, 0, XmlCommon.XmlFieldNativeInt(node, "Condition")) + ",");    
				
				sql.Append(string.Format("GTIN = {0}", DB.SQuote(XmlCommon.XmlField(node, "GTIN")))); 

                sql.Append(" where VariantID=" + VariantID.ToString());
                RunCommand(StatusList, sql.ToString());
                StatusList.Append("  >> Updated OK" + "");
            }

            if (TrackInventoryBySizeAndColor)
            {
                StatusList.Append("Updating Inventory For Variant(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ", Product=" + HttpContext.Current.Server.HtmlEncode(ProductName) + ")" + "");
                DB.ExecuteSQL("delete from Inventory where VariantID=" + VariantID.ToString());
                foreach (XmlNode n in node.SelectNodes("InventoryBySizeAndColor/Inventory"))
                {
					SqlParameter[] sqlParamaters = { new SqlParameter("@VariantId", VariantID), new SqlParameter("@Size", XmlCommon.XmlField(n, "Size").Trim()), new SqlParameter("@Color", XmlCommon.XmlField(n, "Color").Trim()), new SqlParameter("@Quantity", XmlCommon.XmlField(n, "Quantity").Trim()), new SqlParameter("@GTIN", XmlCommon.XmlField(n, "GTIN").Trim()) };

					DB.ExecuteSQL("insert into Inventory(VariantID,[Size],Color,Quan,GTIN) values(@VariantId,@Size,@Color,@Quantity,@GTIN)", sqlParamaters);
                }
            }
        }

        private static void LoadKitGroup(StringBuilder StatusList, XmlNode node, String ProductName, int ProductID)
        {
            CheckForRequiredFields(node, "Name,KitGroupTypeID,IsRequired");
            String nodeName = XmlCommon.XmlField(node, "Name");
            ProcessingNode = "KitGroup:" + nodeName;
            StringBuilder sql = new StringBuilder(2500);

            int KitGroupID = CheckForKitGroup(nodeName, ProductID);
            if (KitGroupID == 0)
            {
                // add the KitGroup:
                StatusList.Append("Adding KitGroup(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ", Product=" + HttpContext.Current.Server.HtmlEncode(ProductName) + ")" + "");

                String NewGUID = CommonLogic.GetNewGUID();
                sql.Append("insert KitGroup(KitGroupGUID,Name,Description,ProductID,KitGroupTypeID,IsRequired,DisplayOrder) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(nodeName) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(ProductID.ToString() + ",");
                sql.Append(XmlCommon.XmlFieldUSInt(node, "KitGroupTypeID").ToString() + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsRequired"), "1", "0") + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0, 1, XmlCommon.XmlFieldUSInt(node, "DisplayOrder")));
                sql.Append(")");
                RunCommand(StatusList, sql.ToString());

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select KitGroupID from KitGroup  with (NOLOCK)  where KitGroupGUID=" + DB.SQuote(NewGUID), dbconn))
                    {
                        rs.Read();
                        KitGroupID = DB.RSFieldInt(rs, "KitGroupID");

                    }

                }

                StatusList.Append("  >> Added OK" + "");
            }
            else
            {
                // update the KitGroup
                StatusList.Append("Updating KitGroup(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ", Product=" + HttpContext.Current.Server.HtmlEncode(ProductName) + ")" + "");

                sql.Append("update KitGroup set ");
                sql.Append("Name=" + DB.SQuote(nodeName) + ",");
                sql.Append("DisplayOrder=" + CommonLogic.IIF(XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0, 1, XmlCommon.XmlFieldUSInt(node, "DisplayOrder")) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append("Description=" + DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("Description=NULL,");
                }
                sql.Append("ProductID=" + ProductID.ToString() + ",");
                sql.Append("KitGroupTypeID=" + XmlCommon.XmlFieldUSInt(node, "KitGroupTypeID").ToString() + ",");
                sql.Append("IsRequired=" + CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsRequired"), "1", "0"));
                sql.Append(" where KitGroupID=" + KitGroupID.ToString());
                RunCommand(StatusList, sql.ToString());
                StatusList.Append("  >> Updated OK" + "");
            }

            // add/update Kit Items:
            XmlNodeList KitItemList = node.SelectNodes(@"KitItem");
            foreach (XmlNode KitItemNode in KitItemList)
            {
                LoadKitItem(StatusList, KitItemNode, nodeName, KitGroupID);
            }

        }

        private static void LoadKitItem(StringBuilder StatusList, XmlNode node, String KitGroupName, int KitGroupID)
        {
            CheckForRequiredFields(node, "Name");
            String nodeName = XmlCommon.XmlField(node, "Name");
            ProcessingNode = "KitItem:" + nodeName;
            StringBuilder sql = new StringBuilder(2500);

            decimal PriceDelta = XmlCommon.XmlFieldNativeDecimal(node, "PriceDelta");

            int KitItemID = CheckForKitItem(nodeName, KitGroupID);
            if (KitItemID == 0)
            {
                // add the KitItem:
                StatusList.Append("Adding KitItem(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ", KitGroup=" + HttpContext.Current.Server.HtmlEncode(KitGroupName) + ")" + "");

                String NewGUID = CommonLogic.GetNewGUID();
                sql.Append("insert KitItem(KitItemGUID,Name,Description,KitGroupID,IsDefault,PriceDelta,DisplayOrder) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(nodeName) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(KitGroupID.ToString() + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsDefault"), "1", "0") + ",");
                sql.Append(PriceDelta + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0, 1, XmlCommon.XmlFieldUSInt(node, "DisplayOrder")));
                sql.Append(")");
                RunCommand(StatusList, sql.ToString());
                StatusList.Append("  >> Added OK" + "");
            }
            else
            {
                // update the KitItem
                StatusList.Append("Updating KitItem(" + HttpContext.Current.Server.HtmlEncode(nodeName) + ", KitGroup=" + HttpContext.Current.Server.HtmlEncode(KitGroupName) + ")" + "");

                sql.Append("update KitItem set ");
                sql.Append("Name=" + DB.SQuote(nodeName) + ",");
                sql.Append("DisplayOrder=" + CommonLogic.IIF(XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0, 1, XmlCommon.XmlFieldUSInt(node, "DisplayOrder")) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append("Description=" + DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("Description=NULL,");
                }
                sql.Append("KitGroupID=" + KitGroupID.ToString() + ",");
                sql.Append("PriceDelta=" + PriceDelta + ",");
                sql.Append("IsDefault=" + CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsDefault"), "1", "0"));
                sql.Append(" where KitItemID=" + KitItemID.ToString());
                RunCommand(StatusList, sql.ToString());
                StatusList.Append("  >> Updated OK" + "");
            }

        }

        public static void DoIt(StringBuilder StatusList, String XMLContents, bool CleanOnError)
        {
            try
            {
                RelatedProductsToDoList.Length = 0;
                ProcessingNode = String.Empty;

                if (XMLContents.Length == 0)
                {
                    StatusList.Append("Nothing to Import!");
                    return;
                }
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(XMLContents);
                XmlNodeList nodeList = xmlDoc.SelectNodes(@"//Customer");
                foreach (XmlNode node in nodeList)
                {
                    LoadCustomer(StatusList, node);
                }
                nodeList = xmlDoc.SelectNodes(@"//ProductType");
                foreach (XmlNode node in nodeList)
                {
                    LoadProductType(StatusList, node);
                }

                foreach (String EntityName in AppLogic.ro_SupportedEntities)
                {
                    nodeList = xmlDoc.SelectNodes(@"//" + EntityName);
                    foreach (XmlNode node in nodeList)
                    {
                        LoadEntity(EntityName, StatusList, node);
                    }
                    nodeList = xmlDoc.SelectNodes(@"//" + EntityName + "Tree");
                    foreach (XmlNode node in nodeList)
                    {
                        LoadEntityTree(EntityName, StatusList, node);
                    }
                }

                bool productImportIsRestricted = false;
                nodeList = xmlDoc.SelectNodes(@"//Product");
                foreach (XmlNode node in nodeList)
                {
                    productImportIsRestricted = AppLogic.MaxProductsExceeded();
                    if (productImportIsRestricted)
                    {
                        StatusList.Append("<font class=\"errorMsg\">Importing Aborted!  Maximum Number of allowed products have been reached. ");
                        StatusList.Append("To add additional products, please delete some products or upgrade to a non-Express license.</font>");
                        break;
                    }

                    if (!productImportIsRestricted)
                    {
                        LoadProduct(StatusList, node);
                    }
                }
                // process relatedproducts (deferred):
                String[] ss = RelatedProductsToDoList.ToString().Split('|');
                int LastPID = 0;
                foreach (String s in ss)
                {
                    if (s.Length != 0)
                    {
                        String[] s2 = s.Split('~');
                        try
                        {
                            if (s2[0].Length != 0 && s2[1].Length != 0)
                            {
                                int ThisPID = Localization.ParseNativeInt(s2[0]);
                                int RelPID = CheckForProduct(s2[1]);
                                if (RelPID != 0 && ThisPID != RelPID)
                                {
                                    RunCommand(StatusList, "update product set relatedproducts=convert(nvarchar(4000),relatedproducts)+','+" + DB.SQuote(RelPID.ToString()) + " where productid=" + ThisPID);
                                }
                                else
                                {
                                    StatusList.Append("Ignoring unknown related product..." + "");
                                }
                                LastPID = ThisPID;
                            }
                        }
                        catch
                        {
                            StatusList.Append("Ignoring unknown related product..." + "");
                        }
                    }
                }

                StatusList.Append("**COMPLETED**" + "");
            }
            catch (Exception ae)
            {
                // something went wrong:
                StatusList.Append("Error on Node: " + HttpContext.Current.Server.HtmlEncode(ProcessingNode) + ": " + ae.Message.ToString() + "");
            }
            CommonLogic.WriteFile(CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "images/import.htm"), "<html><head><title>AspDotNet" + "Storefront Import LOG File: " + Localization.ToNativeDateTimeString(System.DateTime.Now) + "</title></head><body>" + StatusList.ToString().Replace("\n", "") + "</body></html>", false);
        }

        static public int CheckForCustomer(String EMail)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select CustomerID from Customer  with (NOLOCK)  where Deleted=0 and EMail=" + DB.SQuote(EMail.ToLowerInvariant()), dbconn))
                {
                    rs.Read();
                    ID = DB.RSFieldInt(rs, "CustomerID");
                }
            }
            return ID;
        }

        /// <summary>
        /// Returns Product ID to check if the product exists in the entity passed.
        /// </summary>
        /// <param name="EntityName"></param>
        /// <param name="ProductID"></param>
        /// <returns>"ProductID"</returns>
        static public int CheckForProductID(String EntityName, int ProductID)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select ProductID from Product" + EntityName + " where ProductID = " + ProductID, dbconn))
                {
                    rs.Read();
                    ID = DB.RSFieldInt(rs, "ProductID");
                }
            }
            return ID;
        }

        static public int CheckForManufacturer(String Name, bool AddIfNotFound)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select ManufacturerID from Manufacturer  with (NOLOCK)  where Deleted=0 and lower(name)=" + DB.SQuote(Name.ToLowerInvariant()), dbconn))
                {
                    rs.Read();
                    ID = DB.RSFieldInt(rs, "ManufacturerID");

                }
            }

            if (ID == 0 && AddIfNotFound)
            {
                StringBuilder sql = new StringBuilder(2500);
                String NewGUID = CommonLogic.GetNewGUID();
                sql.Append("insert Manufacturer(ManufacturerGUID,IsImport,Name,SEName) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append("1,"); // Set IsImport Flag For later Undo of Import
                sql.Append(DB.SQuote(CommonLogic.Left(Name, 100)) + ",");
                sql.Append(DB.SQuote(CommonLogic.Left(SE.MungeName(Name), 150)));
                sql.Append(")");
                DB.ExecuteSQL(sql.ToString());

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select ManufacturerID from Manufacturer  with (NOLOCK)  where Deleted=0 and Name=" + DB.SQuote(Name), dbconn))
                    {
                        rs.Read();
                        ID = DB.RSFieldInt(rs, "ManufacturerID");
                    }
                }

            }
            return ID;
        }

        static public int CheckForEntity(String EntityName, String Name, bool AutoAdd)
        {
            int ID = 0;
            int ParentEntityID = 0;
            if (Name.StartsWith("/") || Name.StartsWith("//")) // Entity tree ref or simple Entity?
            {
                // Entity tree ref:

                // strip leading & trailing / chars:
                if (Name.StartsWith("/"))
                {
                    Name = Name.Substring(1, Name.Length - 1);
                }
                if (Name.StartsWith("//"))
                {
                    Name = Name.Substring(2, Name.Length - 2);
                }
                if (Name.EndsWith("/"))
                {
                    Name = Name.Substring(0, Name.Length - 1);
                }
                Name = Name.Trim();

                bool entityImportIsRestricted = false;
                foreach (String s in Name.Split('/'))
                {
                    int PIC = CheckForEntity(EntityName, s.Trim(), ParentEntityID);
                    if (PIC != 0)
                    {
                        ParentEntityID = PIC;
                    }
                    else
                    {
                        entityImportIsRestricted = AppLogic.MaxEntitiesExceeded();
                        if (!entityImportIsRestricted)
                        {
                            StringBuilder sql = new StringBuilder(2500);
                            String NewGUID = CommonLogic.GetNewGUID();
                            sql.Append("insert ^(^GUID,IsImport,Name,SEName,Parent^ID) values(");
                            sql.Append(DB.SQuote(NewGUID) + ",");
                            sql.Append("1,"); // Set IsImport Flag For later Undo of Import
                            sql.Append(DB.SQuote(CommonLogic.Left(s.Trim(), 400)) + ",");
                            sql.Append(DB.SQuote(CommonLogic.Left(SE.MungeName(s.Trim()), 100)) + ",");
                            sql.Append(ParentEntityID.ToString());
                            sql.Append(")");
                            sql = sql.Replace("^", EntityName);
                            DB.ExecuteSQL(sql.ToString());

                            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                            {
                                dbconn.Open();
                                using (IDataReader rs = DB.GetRS("select * from " + EntityName + "  with (NOLOCK)  where Deleted=0 and Name=" + DB.SQuote(s) + " and Parent" + EntityName + "ID=" + ParentEntityID.ToString(), dbconn))
                                {
                                    rs.Read();
                                    ParentEntityID = DB.RSFieldInt(rs, EntityName + "ID");
                                }
                            }
                        }
                    }

                    ID = ParentEntityID;
                }
            }
            else
            {
                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("Select * from " + EntityName + "  with (NOLOCK)  where Parent" + EntityName + "ID=0 and Deleted=0 and lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()), dbconn))
                    {
                        if (rs.Read())
                        {
                            if (Name != DB.RSFieldByLocale(rs, EntityName, Localization.GetDefaultLocale()))
                            {
                                ID = DB.RSFieldInt(rs, EntityName + "ID");
                            }
                        }
                    }

                }

                bool entityImportIsRestricted = false;
                entityImportIsRestricted = AppLogic.MaxEntitiesExceeded();
                if (!entityImportIsRestricted)
                {
                    if (ID == 0 && AutoAdd)
                    {
                        StringBuilder sql = new StringBuilder(2500);
                        String NewGUID = CommonLogic.GetNewGUID();
                        sql.Append("insert " + EntityName + "(" + EntityName + "GUID,IsImport,Name,SEName) values(");
                        sql.Append(DB.SQuote(NewGUID) + ",");
                        sql.Append("1,"); // Set IsImport Flag For later Undo of Import
                        sql.Append(DB.SQuote(CommonLogic.Left(Name, 400)) + ",");
                        sql.Append(DB.SQuote(CommonLogic.Left(SE.MungeName(Name.Trim()), 100)));
                        sql.Append(")");
                        DB.ExecuteSQL(sql.ToString());

                        using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                        {
                            dbconn.Open();
                            using (IDataReader rs = DB.GetRS("select * from " + EntityName + "  with (NOLOCK)  where Deleted=0 and Name=" + DB.SQuote(Name), dbconn))
                            {
                                rs.Read();
                                if (Name != DB.RSFieldByLocale(rs, EntityName, Localization.GetDefaultLocale()))
                                {
                                    ID = DB.RSFieldInt(rs, EntityName + "ID");
                                }
                            }
                        }
                    }
                }
            }
            return ID;
        }


        static public int CheckForEntity(String EntityName, String Name, int ParentEntityID)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select * from " + EntityName + "  with (NOLOCK)  where Deleted=0 and Name LIKE '%" + Name.Replace("'", "''").Trim().ToLowerInvariant() + CommonLogic.IIF(ParentEntityID == 0, "%' and (Parent" + EntityName + "ID=0 or Parent" + EntityName + "ID IS NULL)", "%' and Parent" + EntityName + "ID=" + ParentEntityID.ToString()), dbconn))
                {
                    while (rs.Read())
                    {
                        if (Name == DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale()))
                        {
                            ID = DB.RSFieldInt(rs, EntityName + "ID");
                            if (ID != 0) { continue; }
                        }
                    }
                }
            }
            return ID;
        }

        static public int CheckForProductType(String Name)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select ProductTypeID from ProductType  with (NOLOCK)  where lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()), dbconn))
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "ProductTypeID");
                    }
                }
            }
            return ID;
        }

        static public int CheckForProduct(String Name)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select ProductID from Product  with (NOLOCK)  where Deleted=0 and lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()), dbconn))
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "ProductID");
                    }
                }
            }
            return ID;
        }

        static public int CheckForProductVariant(String Name, int ProductID)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select VariantID from ProductVariant  with (NOLOCK)  where Deleted=0 and lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()) + " and ProductID=" + ProductID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "VariantID");
                    }
                }
            }
            return ID;
        }

        static public int CheckForKitGroup(String Name, int ProductID)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select KitGroupID from KitGroup  with (NOLOCK)  where lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()) + " and ProductID=" + ProductID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "KitGroupID");
                    }
                }
            }
            return ID;
        }

        static public int CheckForKitItem(String Name, int KitGroupID)
        {
            int ID = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select KitItemID from KitItem  with (NOLOCK)  where lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()) + " and KitGroupID=" + KitGroupID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "KitItemID");
                    }
                }
            }

            return ID;
        }



        /*The following method is used to convert a pricing file in Excel format to XML*/
        static public string ConvertPricingFileToXml(string PricingFile)
        {
            ExcelToXml exf = new ExcelToXml(PricingFile);

            if (exf == null)
            {
                return "";
            }

            XmlDocument xmlDoc = exf.LoadSheet("Sheet1", "M", 100000, "A,B,I");
            return xmlDoc.InnerXml;

        }
    }
}
