// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontExcelWrapper;
using AspDotNetStorefrontGateways;
using AspDotNetStorefrontGateways.Processors;

namespace AspDotNetStorefrontWSI
{

    /// <summary>
    /// Summary description for WSI.
    /// </summary>
    public class WSI
    {

        public enum NodeActionEnum
        {
            Unknown, Add, Update, Delete, Nuke, Lookup, Get, Clear
        }

        public enum OrderManagementActionEnum
        {
            Unknown, Void, Capture, FullRefund, ForceFullRefund, MarkAsFraud, ClearFraud, BlockIP, AllowIP,
            SimulateDownloadNotification, SendDistributorNotification, ChangeOrderEMail, MarkAsReadyToShip, ClearNewStatus, ClearReadyToShip,
            MarkAsShipped, SetPrivateNotes, SetOrderWeight, SetCustomerServiceNotes, SendToShipRush, SendToFedexShippingMgr,
            GetReceipt, GetDistributorNotifications, MarkAsPrinted, SendReceipt, SetTracking, Delete
        }

        public enum RecurringOrderManagementActionEnum
        {
            Unknown, Declined, Approved, Cancel, FullRefund, UpdateBilling
        }

        public enum InventoryMatchKeyEnum
        {
            SizeColor, VendorFullSKU, VendorID, VariantID, VariantSKU
        }

        private enum MappingTypeEnum
        {
            StringField, IntegerField, DecimalField, BoolField, DateTimeField, GUIDField
        }

        WSIStatusWriter m_ResultWriter = null;
        XmlDocument m_XmlDoc = null;
        bool m_Verbose = false;
        String m_Version = String.Empty;
        bool m_SetImportFlag = false; // TBD not supported yet via WSI
        bool m_AutoLazyAdd = false;
        bool m_AutoCleanup = false;
        bool m_UseImplicitTransactions = true;
        Customer m_ThisCustomer = null;
        XmlDocument m_XmlTableSpecs = null;
        SqlTransaction m_DBTrans = null;

        public WSI() { }

        public WSI(Customer thisCustomer) 
        {
            m_ThisCustomer = thisCustomer;
        }

        #region LookupTableStuff
        private XmlDocument XmlTableSpecs
        {
            get
            {
                if (m_XmlTableSpecs == null)
                {
                    try
                    {
                        m_XmlTableSpecs = new XmlDocument();
                        m_XmlTableSpecs.Load(CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "ipx.xml"));
                    }
                    catch
                    {
                        throw new ArgumentException("Error loading ipx.xml Import Control Specification File!");
                    }
                }
                return m_XmlTableSpecs;
            }
        }

        private String LookupTableIDColumn(String TableName)
        {
            String tmpS = TableName + "ID";
            XmlNode n = XmlTableSpecs.SelectSingleNode("//Tables/Table[@Name='" + XmlCommon.XmlEncodeAttribute(TableName) + "']");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttribute(n, "IDColumn");
            }
            if (tmpS.Length == 0)
            {
                tmpS = DB.GetTableIdentityField(TableName);
            }
            return tmpS;
        }

        private String LookupTableGUIDColumn(String TableName)
        {
            String tmpS = TableName + "GUID";
            XmlNode n = XmlTableSpecs.SelectSingleNode("//Tables/Table[@Name='" + XmlCommon.XmlEncodeAttribute(TableName) + "']");
            if (n != null)
            {
                tmpS = XmlCommon.XmlAttribute(n, "GUIDColumn");
            }
            if (tmpS.Length == 0)
            {
                tmpS = DB.GetTableIdentityField(TableName);
            }
            return tmpS;
        }

        private MappingTypeEnum LookupTableFieldType(String TableName, String FieldName)
        {
            if (TableName.Length == 0 || FieldName.Length == 0)
            {
                return MappingTypeEnum.StringField;
            }
            //XmlNode n = XmlTableSpecs.SelectSingleNode("//Tables/Table[@Name='" + XmlCommon.XmlEncodeAttribute(TableName) + "']/Field[@Name='" + XmlCommon.XmlEncodeAttribute(FieldName) + "']");
            //if(n != null)
            //{
            //    return (MappingTypeEnum)Enum.Parse(typeof(MappingTypeEnum), XmlCommon.XmlAttribute(n,"Type"), true);
            //}
            String s = DB.GetTableColumnDataType(TableName, FieldName);
            MappingTypeEnum m = MappingTypeEnum.StringField;
            switch (s.ToLowerInvariant())
            {
                case "image":
                    m = MappingTypeEnum.StringField;
                    break;
                case "text":
                    m = MappingTypeEnum.StringField;
                    break;
                case "uniqueidentifier":
                    m = MappingTypeEnum.GUIDField;
                    break;
                case "tinyint":
                    m = MappingTypeEnum.BoolField;
                    break;
                case "smallint":
                    m = MappingTypeEnum.IntegerField;
                    break;
                case "int":
                    m = MappingTypeEnum.IntegerField;
                    break;
                case "smalldatetime":
                    m = MappingTypeEnum.DateTimeField;
                    break;
                case "real":
                    m = MappingTypeEnum.DecimalField;
                    break;
                case "money":
                    m = MappingTypeEnum.DecimalField;
                    break;
                case "datetime":
                    m = MappingTypeEnum.DateTimeField;
                    break;
                case "float":
                    m = MappingTypeEnum.DecimalField;
                    break;
                case "sql_variant":
                    m = MappingTypeEnum.StringField;
                    break;
                case "ntext":
                    m = MappingTypeEnum.StringField;
                    break;
                case "bit":
                    m = MappingTypeEnum.BoolField;
                    break;
                case "decimal":
                    m = MappingTypeEnum.DecimalField;
                    break;
                case "numeric":
                    m = MappingTypeEnum.DecimalField;
                    break;
                case "smallmoney":
                    m = MappingTypeEnum.DecimalField;
                    break;
                case "bigint":
                    m = MappingTypeEnum.IntegerField;
                    break;
                case "varbinary":
                    m = MappingTypeEnum.StringField;
                    break;
                case "varchar":
                    m = MappingTypeEnum.StringField;
                    break;
                case "binary":
                    m = MappingTypeEnum.IntegerField;
                    break;
                case "char":
                    m = MappingTypeEnum.StringField;
                    break;
                case "timestamp":
                    m = MappingTypeEnum.DateTimeField;
                    break;
                case "nvarchar":
                    m = MappingTypeEnum.StringField;
                    break;
                case "nchar":
                    m = MappingTypeEnum.StringField;
                    break;
                case "xml":
                    m = MappingTypeEnum.StringField;
                    break;
                case "sysname":
                    m = MappingTypeEnum.StringField;
                    break;
            }
            return m;
        }
        #endregion

        public void LoadFromString(String s)
        {
            m_XmlDoc = new XmlDocument();
            m_XmlDoc.LoadXml(s);
            ProcessHeaderInstructions();
        }

        public void LoadFromXml(XmlDocument d)
        {
            m_XmlDoc = d;
            ProcessHeaderInstructions();
        }

        public void LoadFromXmlFile(String XmlFilePath)
        {
            m_XmlDoc = new XmlDocument();
            m_XmlDoc.Load(XmlFilePath);
            ProcessHeaderInstructions();
        }


        // we're going to read Excel and convert into our Xml Format and then use the same processing code.
        // this will be a little less efficient than just doing the processing on the Excel file, but it will
        // allow the actual import processing logic to be centralized.
        public void LoadFromExcelFile(String ExcelFilePath)
        {
            StringBuilder tmpS = new StringBuilder(4096);

            // get excel file and convert to Xml Import Format:
            String XmlContents = ConvertExcelImportFileToXml(ExcelFilePath);

            // now save the Xml File for later review if necessary:
            String XmlFileName = "ExcelImport_" + Localization.ToThreadCultureShortDateString(System.DateTime.Now).Replace(" ", "").Replace("/", "").Replace(":", "").Replace(".", "");
            String SaveToXmlFilename = CommonLogic.SafeMapPath("../images" + "/" + XmlFileName + ".xml");
            CommonLogic.WriteFile(SaveToXmlFilename, XmlContents.ToString(), true);

            LoadFromString(XmlContents);
        }

        private void ProcessHeaderInstructions()
        {
            XmlNode n = m_XmlDoc.SelectSingleNode("/AspDotNetStorefrontImport");
            m_Verbose = XmlCommon.XmlAttributeBool(n, "Verbose");
            m_Version = XmlCommon.XmlAttribute(n, "Version");
            m_SetImportFlag = XmlCommon.XmlAttributeBool(n, "SetImportFlag");
            m_AutoLazyAdd = XmlCommon.XmlAttributeBool(n, "AutoLazyAdd");
            m_AutoCleanup = XmlCommon.XmlAttributeBool(n, "AutoCleanup");
            // transactions are defaulted to on
            if (XmlCommon.XmlAttribute(n, "UseImplicitTransactions").Length != 0)
            {
                m_UseImplicitTransactions = XmlCommon.XmlAttributeBool(n, "UseImplicitTransactions");
            }
        }

        // returns true on good authentication
        public bool AuthenticateRequest(String AuthenticationEMail, String AuthenticationPassword)
        {
            if (AuthenticationEMail.Length == 0)
            {
                throw new ArgumentException("AuthenticationEMail is required");
            }
            if (AuthenticationPassword.Length == 0)
            {
                throw new ArgumentException("AuthenticationPassword is required");
            }
            bool LoginOk = false;
            m_ThisCustomer = new Customer(null, AuthenticationEMail, true, false, true);
            
            if (m_ThisCustomer.IsRegistered && m_ThisCustomer.IsAdminUser)
            {
                if (m_ThisCustomer.BadLoginCount >= AppLogic.AppConfigNativeInt("MaxBadLogins") && m_ThisCustomer.LockedUntil > DateTime.Now)
                {
                    LoginOk = false;
                }
                else if (!m_ThisCustomer.Active)
                {
                    LoginOk = false;
                }
                else if (m_ThisCustomer.PwdChanged.AddDays(AppLogic.AppConfigUSDouble("AdminPwdChangeDays")) < DateTime.Now || m_ThisCustomer.PwdChangeRequired)
                {
                    LoginOk = false;
                }
                else
                {
                    LoginOk = m_ThisCustomer.CheckLogin(AuthenticationPassword);
                }
            }
            if (!LoginOk)
            {
                m_ThisCustomer = null;
            }
            return LoginOk;
        }

        public void DoIt()
        {
            DoIt(String.Empty);
        }

        // LogFileName will be written into /images, if LogFileName is not empty
        public void DoIt(String LogFileName)
        {
            Results.WriteVerboseEntry("Import Starting");
            try
            {
                int i = 1;
                int TransID = 1;
                // process nodes in the order they appear in the xml doc:
                SqlConnection dbconn = null; // only used in transaction mode
                XmlNode rootNode = m_XmlDoc.SelectSingleNode("/AspDotNetStorefrontImport");
                if (rootNode != null)
                {
                    foreach (XmlNode n in rootNode.ChildNodes)
                    {
                        String NodeName = n.Name;
                        if (NodeName == "Transaction")
                        {
                            String TransName = XmlCommon.XmlAttribute(n, "Name");
                            if (TransName.Length == 0)
                            {
                                TransName = "Transaction" + TransID.ToString();
                            }
                            if (m_UseImplicitTransactions)
                            {
                                Results.WriteXml("<Transaction Name=\"" + XmlCommon.XmlEncodeAttribute(TransName) + "\">");
                                // process all nodes in this transaction as a transaction group
                                // create an implicit tranasaction for this node:
                                dbconn = new SqlConnection();
                                dbconn.ConnectionString = DB.GetDBConn();
                                dbconn.Open();
                                m_DBTrans = dbconn.BeginTransaction(IsolationLevel.ReadCommitted, TransName);
                            }
                            else
                            {
                                m_DBTrans = null;
                                dbconn = null;
                            }
                            try
                            {
                                foreach (XmlNode n2 in n.ChildNodes)
                                {
                                    ProcessDoItNode(n2, ref i);
                                }
                                if (m_DBTrans != null)
                                {
                                    m_DBTrans.Commit();
                                    m_DBTrans.Dispose();
                                    m_DBTrans = null;
                                    Results.WriteXml("<Commit/>");
                                    Results.WriteXml("</Transaction>");
                                    dbconn.Close();
                                    dbconn.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {
                                if (m_DBTrans != null)
                                {
                                    m_DBTrans.Rollback();
                                    m_DBTrans.Dispose();
                                    m_DBTrans = null;
                                    Results.WriteXml("<Rollback/>");
                                    Results.WriteXml("</Transaction>");
                                    dbconn.Close();
                                    dbconn.Dispose();
                                }
                                throw (ex);
                            }
                            TransID++;
                        }
                        else
                        {
                            ProcessDoItNode(n, ref i);
                        }
                    }
                }
                else
                {
                    Results.WriteErrorEntry("Missing AspDotNetStorefrontImport Root Element!!");
                }
                Results.WriteVerboseEntry("Import Completed");
            }
            catch (Exception ex)
            {
                if (m_DBTrans != null)
                {
                    m_DBTrans.Rollback();
                    m_DBTrans.Dispose();
                    m_DBTrans = null;
                    Results.WriteXml("<Rollback/>");
                    Results.WriteXml("</Transaction>");
                }
                // something very bad went wrong:
                Results.WriteErrorEntry(String.Empty, String.Empty, String.Empty, String.Empty, ex.Message.ToString());
                Results.WriteVerboseEntry("Import Aborted");
            }

            if (LogFileName.Length != 0)
            {
                String s = GetResultsAsString().Replace("utf-16", "utf-8").Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", ""); ;
                CommonLogic.WriteFile(CommonLogic.SafeMapPath(CommonLogic.IIF(AppLogic.IsAdminSite, "../", "") + "images/" + LogFileName), s, true);
            }
        }

        private void ProcessDoItNode(XmlNode n, ref int idx)
        {
            String NodeName = n.Name;
            if (NodeName == "AppConfig")
            {
                ProcessAppConfig(n);
            }
            else if (NodeName == "ProductType")
            {
                ProcessProductType(n);
            }
            else if (NodeName == "Entity")
            {
                ProcessEntity(n, 0);
            }
            else if (NodeName == "Product")
            {
                ProcessProduct(n);
            }
            else if (NodeName == "InventoryUpdate")
            {
                ProcessInventoryUpdate(n, 0, 0);
            }
            else if (NodeName == "GetCustomer")
            {
                ProcessGetCustomer(n);
            }
            else if (NodeName == "GetOrder")
            {
                ProcessGetOrder(n);
            }
            else if (NodeName == "ResetPassword")
            {
                ProcessResetPassword(n);
            }
            else if (NodeName == "Customer")
            {
                ProcessCustomer(n, null);
            }
            else if (NodeName == "OrderManagement")
            {
                ProcessOrderManagement(n);
            }
            else if (NodeName == "Mapping")
            {
                ProcessMapping(n);
            }
            else if (NodeName == "GetGUID")
            {
                ProcessGetGUID(n);
            }
            else if (NodeName == "ClearMappings")
            {
                ProcessClearMappings(n);
            }
            else if (NodeName == "GetMappings")
            {
                ProcessGetMappings(n);
            }
            else if (NodeName == "GetEntityHelper")
            {
                ProcessGetEntityHelper(n);
            }
            else if (NodeName == "GetEntity")
            {
                ProcessGetEntity(n);
            }
            else if (NodeName == "ShoppingCart")
            {
                ProcessShoppingCart(n);
            }
            else if (NodeName == "GetProduct")
            {
                ProcessGetProduct(n);
            }
            else if (NodeName == "SetOrder")
            {
                ProcessSetOrder(n);
            }
            else if (NodeName == "Set")
            {
                idx++;
                ProcessSet(n, idx);
            }
            else if (NodeName == "Get")
            {
                idx++;
                ProcessGet(n, idx);
            }
            else if (NodeName == "ExecuteSQL")
            {
                idx++;
                ProcessExecuteSQL(n, idx);
            }
            else if (NodeName == "Query")
            {
                idx++;
                ProcessQuery(n, idx);
            }
            else if (NodeName == "XmlPackage")
            {
                idx++;
                ProcessXmlPackage(n, idx);
            }
            else if (NodeName == "ScanForXmlPackages")
            {
                ProcessScanForXmlPackages(n);
            }
            else if (NodeName == "ResetCache")
            {
                ProcessResetCache(n);
            }
            else if (NodeName == "StoreMapping")
            {
                ProcessStoreMapping(n);
            }
        }

        // XPathEntityName must be //x/y/z etc...
        private int LoadEntityTree(String EntityType, String XPathEntityName, int ParentEntityID)
        {
            return LoadEntityTree(EntityType, XPathEntityName, ParentEntityID, String.Empty, String.Empty);
        }

        // XPathEntityName must be //x/y/z etc...
        // Takes name of the last XPath entity (i.e. Cat-1-1 from /Cat/Cat-1/Cat-1-1) to be added and the entityGUID 
        // so that when adding a nested entity the GUID can still be set from the import
        private int LoadEntityTree(String EntityType, String XPathEntityName, int ParentEntityID, String EntityGUID, String OrigEntityName)
        {
            if (XPathEntityName.Length == 0)
            {
                return 0;
            }
            if (XPathEntityName.StartsWith("//"))
            {
                XPathEntityName = XPathEntityName.Substring(2, XPathEntityName.Length - 2);
            }
            if (XPathEntityName.StartsWith("/"))
            {
                XPathEntityName = XPathEntityName.Substring(1, XPathEntityName.Length - 1);
            }
            if (XPathEntityName.EndsWith("/"))
            {
                XPathEntityName = XPathEntityName.Substring(0, XPathEntityName.Length - 1);
            }
            XPathEntityName = XPathEntityName.Trim();
            foreach (String s in XPathEntityName.Split('/'))
            {
                int PIC = CheckForEntity(EntityType, s, ParentEntityID);
                if (PIC != 0)
                {
                    ParentEntityID = PIC;
                }
                else
                {
                    StringBuilder sql = new StringBuilder(1024);
                    String NewGUID = CommonLogic.GetNewGUID();

                    if (s.Equals(OrigEntityName, StringComparison.InvariantCultureIgnoreCase) &&
                        EntityGUID.Length > 0)
                    {
                        NewGUID = EntityGUID;
                    }
                    sql.Append("insert ^(^GUID,Name,SEName,Parent^ID) values(");
                    sql.Append(DB.SQuote(NewGUID) + ",");
                    sql.Append(DB.SQuote(s) + ",");
                    sql.Append(DB.SQuote(SE.MungeName(s)) + ",");
                    sql.Append(ParentEntityID.ToString());
                    sql.Append(")");
                    sql = sql.Replace("^", EntityType);
                    RunCommand(sql.ToString());

                    SqlConnection con = null;
                    IDataReader rs = null;
                    try
                    {
                        string query = "select * from " + EntityType + "  with (NOLOCK)  where Deleted=0 and Name=" + DB.SQuote(s) + " and Parent" + EntityType + "ID=" + ParentEntityID.ToString();
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            rs = DB.GetRS(query, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            con = new SqlConnection(DB.GetDBConn());
                            con.Open();
                            rs = DB.GetRS(query, con);
                        }

                        using (rs)
                        {
                            rs.Read();
                            ParentEntityID = DB.RSFieldInt(rs, EntityType + "ID");
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (con != null && m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            con.Dispose();
                        }

                        // make sure we won't reference this again in code
                        rs = null;
                        con = null;
                    }


                }
            }
            return ParentEntityID;
        }

        public void UndoLastImport()
        {
            RunCommand("aspdnsf_UndoImport");
        }

        // XPathEntityName must be //x/y/z etc...
        private int LookupEntityTree(String EntityType, String XPathEntityName)
        {
            if (XPathEntityName.Length == 0)
            {
                return 0;
            }
            int ParentEntityID = 0;
            if (XPathEntityName.StartsWith("//"))
            {
                XPathEntityName = XPathEntityName.Substring(2, XPathEntityName.Length - 2);
            }
            if (XPathEntityName.StartsWith("/"))
            {
                XPathEntityName = XPathEntityName.Substring(1, XPathEntityName.Length - 1);
            }
            if (XPathEntityName.EndsWith("/"))
            {
                XPathEntityName = XPathEntityName.Substring(0, XPathEntityName.Length - 1);
            }
            XPathEntityName = XPathEntityName.Trim();
            foreach (String s in XPathEntityName.Split('/'))
            {
                int PIC = CheckForEntity(EntityType, s, ParentEntityID);
                if (PIC == 0)
                {
                    return 0;
                }
                ParentEntityID = PIC;
            }
            return ParentEntityID;
        }

        public int LookupTableRecordByGUID(String TableName, String GUID)
        {
            int ID = 0;
            String IDColumnName = LookupTableIDColumn(TableName);
            String GUIDColumnName = LookupTableGUIDColumn(TableName);
            String sql = String.Format("select {0} from {1} with (NOLOCK) where {2}={3}", IDColumnName, TableName, GUIDColumnName, DB.SQuote(GUID));

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, IDColumnName);
                    }
                }
            }

            return ID;
        }

        public bool ExistsTableRecordByID(String TableName, int ID)
        {
            String IDColumnName = LookupTableIDColumn(TableName);
            String GUIDColumnName = LookupTableGUIDColumn(TableName);
            String sql = String.Format("select count({0}) as N from {1} with (NOLOCK) where {2}={3}", IDColumnName, TableName, IDColumnName, ID.ToString());
            return DB.GetSqlN(sql, m_DBTrans) > 0;
        }

        public int LookupTableRecordByName(String TableName, String Name, bool HasDeletedFlag)
        {
            int ID = 0;
            String IDColumnName = LookupTableIDColumn(TableName);
            String GUIDColumnName = LookupTableGUIDColumn(TableName);

            String sql = String.Format("select {0} from {1} with (NOLOCK) where {2} lower(Name)={3}", IDColumnName, TableName, CommonLogic.IIF(HasDeletedFlag, "deleted=0 and", ""), DB.SQuote(Name.Trim().ToLowerInvariant()));

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, IDColumnName);
                    }
                }
            }

            return ID;
        }

        // will add it by name if required!
        private int LoadSimpleObject(String TableName, int ID, String GUID, String Name, bool HasDeletedFlag)
        {
            if (ID != 0)
            {
                // validate product exists
                if (ExistsTableRecordByID(TableName, ID))
                {
                    return ID;
                }
                ID = 0;
            }
            if (GUID.Length != 0)
            {
                ID = LookupTableRecordByGUID(TableName, GUID);
            }
            if (ID != 0)
            {
                // validate product exists
                if (ExistsTableRecordByID(TableName, ID))
                {
                    return ID;
                }
                ID = 0;
            }
            if (Name.Length != 0)
            {
                ID = LookupTableRecordByName(TableName, Name, HasDeletedFlag);
                if (ID != 0)
                {
                    return ID;
                }
                String NewGUID = CommonLogic.GetNewGUID();
                String GUIDColumnName = LookupTableGUIDColumn(TableName);
                String sql = String.Format("insert {0} ({1},Name) values({2},{3})", TableName, GUIDColumnName, DB.SQuote(NewGUID), DB.SQuote(Name));
                RunCommand(sql.ToString());
                ID = LookupTableRecordByGUID(TableName, GUID);
            }
            return ID;
        }

        //LookupCustomerLevel methods
        public int LookupCustomerLevelByName(String CustomerLevelName)
        {
            if (CustomerLevelName.Length == 0)
            {
                throw new ArgumentException("CustomerLevelName is required");
            }
            int ID = 0;
            String sql = "Select CustomerLevelID from CustomerLevel  with (NOLOCK)  where Name=" + DB.SQuote(CustomerLevelName);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(sql, con))
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "CustomerLevelID");
                    }
                }
            }

            return ID;
        }

        public int LookupCustomerLevelByGUID(String CustomerLevelGUID)
        {
            int ID = 0;
            String sql = "Select CustomerLevelID from CustomerLevel  with (NOLOCK)  where CustomerLevelGUID=" + DB.SQuote(CustomerLevelGUID);

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "CustomerLevelID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }




            return ID;
        }

        public int LookupEntityByName(String EntityName, String Name, int ParentEntityID)
        {
            if (EntityName.Length == 0)
            {
                throw new ArgumentException("EntityName is required");
            }
            if (Name.Length == 0)
            {
                throw new ArgumentException("Name is required");
            }
            int ID = 0;
            String sql = "Select ^ID from ^  with (NOLOCK)  where Deleted=0 and Parent^ID=" + ParentEntityID.ToString() + " and lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant());
            sql = sql.Replace("^", EntityName);

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, EntityName + "ID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return ID;
        }

        public int LookupEntityByGUID(String EntityType, String EntityGUID)
        {
            if (EntityType.Length == 0)
            {
                throw new ArgumentException("EntityType is required");
            }
            int ID = 0;
            String sql = "Select ^ID from ^  with (NOLOCK)  where deleted=0 and ^GUID=" + DB.SQuote(EntityGUID);
            sql = sql.Replace("^", EntityType);

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, EntityType + "ID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }



            return ID;
        }

        public bool ExistsEntityByID(String EntityType, int ID)
        {
            if (EntityType.Length == 0)
            {
                throw new ArgumentException("EntityType is required");
            }
            String sql = "Select count(^ID) as N from ^  with (NOLOCK)  where deleted=0 and ^ID=" + ID.ToString();
            sql = sql.Replace("^", EntityType);
            return DB.GetSqlN(sql, m_DBTrans) > 0;
        }

        public int LookupEntityByXPath(String EntityType, String XPathEntityName)
        {
            if (EntityType.Length == 0)
            {
                Results.WriteErrorEntry(EntityType, "EntityType is required");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("EntityType is required");
                }
            }
            if (XPathEntityName.Length == 0)
            {
                Results.WriteErrorEntry(EntityType, "XPathEntityName is required");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("XPathEntityName is required");
                }
            }
            int ID = LookupEntityTree(EntityType, XPathEntityName);
            return ID;
        }

        public int CheckForKitGroup(String Name, int ProductID)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select KitGroupID from KitGroup  with (NOLOCK)  where lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()) + " and ProductID=" + ProductID.ToString();
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "KitGroupID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return ID;
        }

        public int CheckForKitItem(String Name, int KitGroupID)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select KitItemID from KitItem  with (NOLOCK)  where lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()) + " and KitGroupID=" + KitGroupID.ToString();
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "KitItemID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return ID;
        }

        private void RunCommand(String cmd)
        {
            Results.WriteVerboseEntry("Executing SQL: " + cmd);
            int TimeoutSecs = CommonLogic.ApplicationUSInt("SQLCommandTimeoutSecs");
            if (TimeoutSecs == 0)
            {
                TimeoutSecs = 60;
            }
            DB.ExecuteSQL(cmd, m_DBTrans);
        }

        private int SetupTableID(String TableName, int ID, String GUID)
        {
            int newid = ID;
            String sql = String.Empty;
            if (newid == 0 && GUID.Length == 0)
            {
                return 0;
            }
            String IDColumnName = LookupTableIDColumn(TableName);
            String GUIDColumnName = LookupTableGUIDColumn(TableName);
            if (GUID.Length != 0)
            {
                if (newid != 0)
                {
                    sql = String.Format("select {0} from {1} with (NOLOCK) where {2}={3} or {4}={5}", IDColumnName, TableName, IDColumnName, ID.ToString(), GUIDColumnName, DB.SQuote(GUID));
                }
                else
                {
                    sql = String.Format("select {0} from {1} with (NOLOCK) where {2}={3}", IDColumnName, TableName, GUIDColumnName, DB.SQuote(GUID));
                }
            }
            else
            {
                sql = String.Format("select {0} from {1} with (NOLOCK) where {2}={3}", IDColumnName, TableName, IDColumnName, ID.ToString());
            }

            newid = 0;
            GUID = String.Empty;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = sql;
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        newid = DB.RSFieldInt(rs, IDColumnName);
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return newid;
        }

        private int SetupProductID(int ProductID, String ProductGUID, String ProductName, String SKU)
        {
            int id = SetupTableID("Product", ProductID, ProductGUID);
            if (id == 0 && SKU.Length != 0)
            {
                // check for existing product by exact SKU match:
                SqlConnection con = null;
                IDataReader rsp = null;
                try
                {
                    string query = "select ProductID from Product  with (NOLOCK)  where Deleted<>1 and lower(name)=" + DB.SQuote(ProductName) + CommonLogic.IIF(SKU.Trim().Length != 0, " and SKU=" + DB.SQuote(SKU), "");
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rsp = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rsp = DB.GetRS(query, con);
                    }

                    using (rsp)
                    {
                        if (rsp.Read())
                        {
                            id = DB.RSFieldInt(rsp, "ProductID");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rsp = null;
                    con = null;
                }
            }

            return id;
        }

        private int SetupVariantID(int ProductID, int VariantID, String VariantGUID, String VariantName, String SKUSuffix)
        {
            int id = SetupTableID("ProductVariant", VariantID, VariantGUID);
            if (id == 0)
            {
                // check for existing Variant by exact SKU match:
                SqlConnection con = null;
                IDataReader rsp = null;
                try
                {
                    string query = "select VariantID from ProductVariant  with (NOLOCK)  where Deleted=0 and lower(name)=" + DB.SQuote(VariantName) + "and ProductID=" + ProductID + CommonLogic.IIF(SKUSuffix.Trim().Length != 0, " and SKUSuffix=" + DB.SQuote(SKUSuffix), "");
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rsp = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rsp = DB.GetRS(query, con);
                    }

                    using (rsp)
                    {
                        if (rsp.Read())
                        {
                            id = DB.RSFieldInt(rsp, "VariantID");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rsp = null;
                    con = null;
                }

            }
            if (id != 0)
            {
                // validate variant exists:
                if (DB.GetSqlN("select count(VariantID) as N from ProductVariant  with (NOLOCK)  where Deleted=0 and VariantID=" + id.ToString(), m_DBTrans) == 0)
                {
                    id = 0;
                }
            }
            return id;
        }


        private int SetupNodeID(int nodeID, String nodeGUID, String NodeType, String nodeName, NodeActionEnum NodeAction)
        {
            int id = nodeID;
            if (id == 0 && nodeGUID.Length == 0)
            {
                return 0;
            }
            if (id == 0)
            {
                id = LookupTableRecordByGUID(NodeType, nodeGUID);
            }
            if (id != 0)
            {

                // validate node exists:
                if (!ExistsTableRecordByID(NodeType, id))
                {
                    id = 0;
                }
            }
            return id;
        }

        private int SetupEntityNodeID(int nodeID, String nodeGUID, String NodeType, String entityName, String nodeName, NodeActionEnum NodeAction)
        {
            int id = nodeID;
            if (id == 0 && nodeGUID.Length == 0)
            {
                return 0;
            }
            if (id == 0)
            {
                id = LookupEntityByGUID(entityName, nodeGUID);
            }
            if (id != 0)
            {
                // validate node exists:
                if (!ExistsEntityByID(entityName, id))
                {
                    id = 0;
                }
            }
            return id;
        }

        private void ProcessGetGUID(XmlNode node)
        {
            String NodeType = node.Name;
            int NumGUIDs = XmlCommon.XmlAttributeNativeInt(node, "Count");
            NumGUIDs = CommonLogic.Min(NumGUIDs, 1000);

            Results.WriteVerboseEntry("Processing " + NodeType + ", Count=" + NumGUIDs.ToString());
            for (int i = 1; i <= NumGUIDs; i++)
            {
                Results.WriteGUIDOutputEntry(CommonLogic.GetNewGUID().ToUpperInvariant());
            }
            Results.WriteVerboseEntry(NodeType + " OK");
        }

        private int ProcessProductType(XmlNode node)
        {
            String NodeType = node.Name;
            String ProductTypeName = XmlCommon.XmlField(node, "Name");
            NodeActionEnum NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            String ProductTypeGUID = XmlCommon.XmlAttribute(node, "GUID");
            int ProductTypeID = XmlCommon.XmlAttributeNativeInt(node, "ID");

            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + ProductTypeName + ", Action=" + NodeAction + ", ID=" + ProductTypeID.ToString() + ", GUID=" + ProductTypeGUID);

            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Add:
                        {
                            if (CheckForProductType(ProductTypeName) != 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "ERROR", "Duplicate " + NodeType + " skipping...");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Duplicate " + NodeType);
                                }
                            }
                            else
                            {
                                StringBuilder sql = new StringBuilder(1024);
                                if (ProductTypeGUID.Length == 0)
                                {
                                    ProductTypeGUID = CommonLogic.GetNewGUID();
                                }
                                sql.Append("insert ProductType(ProductTypeGUID,Name) values(");
                                sql.Append(DB.SQuote(ProductTypeGUID) + ",");
                                sql.Append(DB.SQuote(ProductTypeName));
                                sql.Append(")");
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                ProductTypeID = LookupTableRecordByGUID("ProductType", ProductTypeGUID);
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Update:
                        {
                            ProductTypeID = SetupNodeID(ProductTypeID, ProductTypeGUID, NodeType, ProductTypeName, NodeAction);
                            if (ProductTypeID == 0 && ProductTypeGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                if (ProductTypeID != 0)
                                {
                                    StringBuilder sql = new StringBuilder(1024);
                                    sql.Append("update ProductType set ");
                                    sql.Append("Name=");
                                    sql.Append(DB.SQuote(ProductTypeName));
                                    if (ProductTypeGUID.Length != 0)
                                    {
                                        sql.Append(",ProductTypeGUID=");
                                        sql.Append(DB.SQuote(ProductTypeGUID));
                                    }
                                    sql.Append(" where ProductTypeID=");
                                    sql.Append(ProductTypeID.ToString());
                                    RunCommand(sql.ToString());
                                    Results.WriteVerboseEntry(NodeType + " OK");
                                    Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "OK", String.Empty);
                                }
                                else
                                {
                                    if (m_AutoLazyAdd)
                                    {
                                        Results.WriteVerboseEntry(NodeType + " LazyAdd being performed...");
                                        NodeAction = NodeActionEnum.Add;
                                        if (CheckForProductType(ProductTypeName) != 0)
                                        {
                                            Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "ERROR", "Duplicate " + NodeType + ", skipping...");
                                            if (m_DBTrans != null)
                                            {
                                                throw new ArgumentException("Duplicate " + NodeType);
                                            }
                                        }
                                        else
                                        {
                                            StringBuilder sql = new StringBuilder(1024);
                                            if (ProductTypeGUID.Length == 0)
                                            {
                                                ProductTypeGUID = CommonLogic.GetNewGUID();
                                            }
                                            sql.Append("insert ProductType(ProductTypeGUID,Name) values(");
                                            sql.Append(DB.SQuote(ProductTypeGUID));
                                            sql.Append(",");
                                            sql.Append(DB.SQuote(ProductTypeName));
                                            sql.Append(")");
                                            RunCommand(sql.ToString());
                                            Results.WriteVerboseEntry(NodeType + " OK");
                                            ProductTypeID = LookupTableRecordByGUID("ProductType", ProductTypeGUID);
                                            Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "OK", String.Empty);
                                        }
                                    }
                                    else
                                    {
                                        Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "ERROR", "No ID or GUID Specified");
                                        if (m_DBTrans != null)
                                        {
                                            throw new ArgumentException("No ID or GUID Specified");
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            Results.WriteVerboseEntry(NodeType + " does not support DELETE. NUKE being performed");
                            ProductTypeID = SetupNodeID(ProductTypeID, ProductTypeGUID, NodeType, ProductTypeName, NodeAction);
                            if (ProductTypeID == 0 && ProductTypeGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                StringBuilder sql = new StringBuilder(1024);
                                sql.Append("delete from ProductType where ProductTypeID=");
                                sql.Append(ProductTypeID.ToString());
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Nuke:
                        {
                            ProductTypeID = SetupNodeID(ProductTypeID, ProductTypeGUID, NodeType, ProductTypeName, NodeAction);
                            if (ProductTypeID == 0 && ProductTypeGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                StringBuilder sql = new StringBuilder(1024);
                                sql.Append("delete from ProductType where ProductTypeID=");
                                sql.Append(ProductTypeID.ToString());
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Lookup:
                        {
                            ProductTypeID = SetupNodeID(ProductTypeID, ProductTypeGUID, NodeType, ProductTypeName, NodeAction);
                            // if id and guid lookup failed, try it by name:
                            if (ProductTypeID == 0)
                            {
                                ProductTypeID = LookupTableRecordByGUID("ProductType", ProductTypeGUID);
                            }
                            if (ProductTypeID != 0)
                            {
                                SqlConnection con = null;
                                IDataReader rs = null;
                                try
                                {
                                    string query = "select * from ProductType  with (NOLOCK)  where ProductTypeID=" + ProductTypeID.ToString();
                                    if (m_DBTrans != null)
                                    {
                                        // if a transaction was passed, we should use the transaction objects connection
                                        rs = DB.GetRS(query, m_DBTrans);
                                    }
                                    else
                                    {
                                        // otherwise create it
                                        con = new SqlConnection(DB.GetDBConn());
                                        con.Open();
                                        rs = DB.GetRS(query, con);
                                    }

                                    using (rs)
                                    {
                                        if (rs.Read())
                                        {
                                            ProductTypeName = DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale());
                                            ProductTypeGUID = DB.RSFieldGUID(rs, "ProductTypeGUID");
                                        }
                                    }
                                }
                                catch { throw; }
                                finally
                                {
                                    // we can't dispose of the connection if it's part of a transaction
                                    if (con != null && m_DBTrans == null)
                                    {
                                        // here it's safe to dispose since we created the connection ourself
                                        con.Dispose();
                                    }

                                    // make sure we won't reference this again in code
                                    rs = null;
                                    con = null;
                                }


                            }
                            if (ProductTypeID != 0 && ProductTypeName.Length != 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                Results.WriteOutputEntry(NodeType, ProductTypeName, ProductTypeGUID, ProductTypeID, NodeAction, "ERROR", "Not Found");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Not Found");
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return ProductTypeID;
        }

        private int ProcessAppConfig(XmlNode node)
        {
            String NodeType = node.Name;
            String AppConfigName = XmlCommon.XmlAttribute(node, "Name");
            String AppConfigDescription = XmlCommon.XmlAttribute(node, "Description");
            String AppConfigValue = XmlCommon.XmlAttribute(node, "ConfigValue");
            bool AppConfigSuperOnly = XmlCommon.XmlAttributeBool(node, "SuperOnly");
            NodeActionEnum NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            String AppConfigGUID = XmlCommon.XmlAttribute(node, "GUID");
            int AppConfigID = XmlCommon.XmlAttributeNativeInt(node, "ID");

            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + AppConfigName + ", Action=" + NodeAction + ", ID=" + AppConfigID.ToString() + ", GUID=" + AppConfigGUID);

            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Add:
                        {
                            if (CheckForAppConfig(AppConfigName) != 0)
                            {
                                Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "ERROR", "Duplicate " + NodeType + " skipping...");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Duplicate " + NodeType);
                                }
                            }
                            else
                            {
                                StringBuilder sql = new StringBuilder(1024);
                                if (AppConfigGUID.Length == 0)
                                {
                                    AppConfigGUID = CommonLogic.GetNewGUID();
                                }
                                sql.Append("insert AppConfig(AppConfigGUID,Name,Description,ConfigValue,SuperOnly) values(");
                                sql.Append(DB.SQuote(AppConfigGUID) + ",");
                                sql.Append(DB.SQuote(AppConfigName) + ",");
                                sql.Append(DB.SQuote(AppConfigDescription) + ",");
                                sql.Append(DB.SQuote(AppConfigValue) + ",");
                                sql.Append(CommonLogic.IIF(AppConfigSuperOnly, "1", "0"));
                                sql.Append(")");
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                AppConfigID = LookupTableRecordByGUID("AppConfig", AppConfigGUID);
                                Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Update:
                        {
                            AppConfigID = SetupNodeID(AppConfigID, AppConfigGUID, NodeType, AppConfigName, NodeAction);
                            if (AppConfigID == 0 && AppConfigGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                if (AppConfigID != 0)
                                {
                                    SqlConnection conx = null;
                                    IDataReader rsx = null;
                                    try
                                    {
                                        string query = "select * from AppConfig where AppConfigID=" + AppConfigID.ToString();
                                        if (m_DBTrans != null)
                                        {
                                            // if a transaction was passed, we should use the transaction objects connection
                                            rsx = DB.GetRS(query, m_DBTrans);
                                        }
                                        else
                                        {
                                            // otherwise create it
                                            conx = new SqlConnection(DB.GetDBConn());
                                            conx.Open();
                                            rsx = DB.GetRS(query, conx);
                                        }

                                        using (rsx)
                                        {
                                            if (rsx.Read())
                                            {
                                                if (DB.RSFieldBool(rsx, "SuperOnly") && m_ThisCustomer.IsAdminSuperUser)
                                                {
                                                    if (m_DBTrans != null)
                                                    {
                                                        throw new ArgumentException("Permission Denied");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch { throw; }
                                    finally
                                    {
                                        // we can't dispose of the connection if it's part of a transaction
                                        if (conx != null && m_DBTrans == null)
                                        {
                                            // here it's safe to dispose since we created the connection ourself
                                            conx.Dispose();
                                        }

                                        // make sure we won't reference this again in code
                                        rsx = null;
                                        conx = null;
                                    }

                                    StringBuilder sql = new StringBuilder(1024);
                                    sql.Append("update AppConfig set ");
                                    sql.Append("Name=");
                                    sql.Append(DB.SQuote(AppConfigName));
                                    if (AppConfigGUID.Length != 0)
                                    {
                                        sql.Append(",AppConfigGUID=");
                                        sql.Append(DB.SQuote(AppConfigGUID));
                                    }
                                    sql.Append(",Description=");
                                    sql.Append(DB.SQuote(AppConfigDescription));
                                    sql.Append(",ConfigValue=");
                                    sql.Append(DB.SQuote(AppConfigValue));
                                    sql.Append(",SuperOnly=");
                                    sql.Append(CommonLogic.IIF(AppConfigSuperOnly, "1", "0"));
                                    sql.Append(" where AppConfigID=");
                                    sql.Append(AppConfigID.ToString());
                                    RunCommand(sql.ToString());
                                    Results.WriteVerboseEntry(NodeType + " OK");
                                    Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "OK", String.Empty);
                                }
                                else
                                {
                                    if (m_AutoLazyAdd)
                                    {
                                        Results.WriteVerboseEntry(NodeType + " LazyAdd being performed...");
                                        NodeAction = NodeActionEnum.Add;
                                        if (CheckForAppConfig(AppConfigName) != 0)
                                        {
                                            Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "ERROR", "Duplicate " + NodeType + ", skipping...");
                                            if (m_DBTrans != null)
                                            {
                                                throw new ArgumentException("Duplicate " + NodeType);
                                            }
                                        }
                                        else
                                        {
                                            StringBuilder sql = new StringBuilder(1024);
                                            if (AppConfigGUID.Length == 0)
                                            {
                                                AppConfigGUID = CommonLogic.GetNewGUID();
                                            }
                                            sql.Append("insert AppConfig(AppConfigGUID,Name,Description,ConfigValue,SuperOnly) values(");
                                            sql.Append(DB.SQuote(AppConfigGUID) + ",");
                                            sql.Append(DB.SQuote(AppConfigName) + ",");
                                            sql.Append(DB.SQuote(AppConfigDescription) + ",");
                                            sql.Append(DB.SQuote(AppConfigValue) + ",");
                                            sql.Append(CommonLogic.IIF(AppConfigSuperOnly, "1", "0"));
                                            sql.Append(")");
                                            RunCommand(sql.ToString());
                                            Results.WriteVerboseEntry(NodeType + " OK");
                                            AppConfigID = LookupTableRecordByGUID("AppConfig", AppConfigGUID);
                                            Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "OK", String.Empty);
                                        }
                                    }
                                    else
                                    {
                                        Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "ERROR", "No ID or GUID Specified");
                                        if (m_DBTrans != null)
                                        {
                                            throw new ArgumentException("No ID or GUID Specified");
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            Results.WriteVerboseEntry(NodeType + " does not support DELETE. NUKE being performed");
                            AppConfigID = SetupNodeID(AppConfigID, AppConfigGUID, NodeType, AppConfigName, NodeAction);
                            if (AppConfigID == 0 && AppConfigGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                SqlConnection conx = null;
                                IDataReader rsx = null;
                                try
                                {
                                    string query = "select * from AppConfig where AppConfigID=" + AppConfigID.ToString();
                                    if (m_DBTrans != null)
                                    {
                                        // if a transaction was passed, we should use the transaction objects connection
                                        rsx = DB.GetRS(query, m_DBTrans);
                                    }
                                    else
                                    {
                                        // otherwise create it
                                        conx = new SqlConnection(DB.GetDBConn());
                                        conx.Open();
                                        rsx = DB.GetRS(query, conx);
                                    }

                                    using (rsx)
                                    {
                                        if (rsx.Read())
                                        {
                                            if (DB.RSFieldBool(rsx, "SuperOnly") && m_ThisCustomer.IsAdminSuperUser)
                                            {
                                                if (m_DBTrans != null)
                                                {
                                                    throw new ArgumentException("Permission Denied");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch { throw; }
                                finally
                                {
                                    // we can't dispose of the connection if it's part of a transaction
                                    if (conx != null && m_DBTrans == null)
                                    {
                                        // here it's safe to dispose since we created the connection ourself
                                        conx.Dispose();
                                    }

                                    // make sure we won't reference this again in code
                                    rsx = null;
                                    conx = null;
                                }

                                StringBuilder sql = new StringBuilder(1024);
                                sql.Append("delete from AppConfig where AppConfigID=");
                                sql.Append(AppConfigID.ToString());
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Nuke:
                        {
                            AppConfigID = SetupNodeID(AppConfigID, AppConfigGUID, NodeType, AppConfigName, NodeAction);
                            if (AppConfigID == 0 && AppConfigGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                SqlConnection conx = null;
                                IDataReader rsx = null;
                                try
                                {
                                    string query = "select * from AppConfig where AppConfigID=" + AppConfigID.ToString();
                                    if (m_DBTrans != null)
                                    {
                                        // if a transaction was passed, we should use the transaction objects connection
                                        rsx = DB.GetRS(query, m_DBTrans);
                                    }
                                    else
                                    {
                                        // otherwise create it
                                        conx = new SqlConnection(DB.GetDBConn());
                                        conx.Open();
                                        rsx = DB.GetRS(query, conx);
                                    }

                                    using (rsx)
                                    {
                                        if (rsx.Read())
                                        {
                                            if (DB.RSFieldBool(rsx, "SuperOnly") && m_ThisCustomer.IsAdminSuperUser)
                                            {
                                                if (m_DBTrans != null)
                                                {
                                                    throw new ArgumentException("Permission Denied");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch { throw; }
                                finally
                                {
                                    // we can't dispose of the connection if it's part of a transaction
                                    if (conx != null && m_DBTrans == null)
                                    {
                                        // here it's safe to dispose since we created the connection ourself
                                        conx.Dispose();
                                    }

                                    // make sure we won't reference this again in code
                                    rsx = null;
                                    conx = null;
                                }

                                StringBuilder sql = new StringBuilder(1024);
                                sql.Append("delete from AppConfig where AppConfigID=");
                                sql.Append(AppConfigID.ToString());
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Lookup:
                        {
                            AppConfigID = SetupNodeID(AppConfigID, AppConfigGUID, NodeType, AppConfigName, NodeAction);
                            // if id and guid lookup failed, try it by name:
                            if (AppConfigID == 0)
                            {
                                AppConfigID = LookupTableRecordByGUID("AppConfig", AppConfigGUID);
                            }
                            if (AppConfigID != 0)
                            {
                                SqlConnection con = null;
                                IDataReader rs = null;
                                try
                                {
                                    string query = "select * from AppConfig  with (NOLOCK)  where AppConfigID=" + AppConfigID.ToString();
                                    if (m_DBTrans != null)
                                    {
                                        // if a transaction was passed, we should use the transaction objects connection
                                        rs = DB.GetRS(query, m_DBTrans);
                                    }
                                    else
                                    {
                                        // otherwise create it
                                        con = new SqlConnection(DB.GetDBConn());
                                        con.Open();
                                        rs = DB.GetRS(query, con);
                                    }

                                    using (rs)
                                    {
                                        if (rs.Read())
                                        {
                                            AppConfigName = DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale());
                                            AppConfigGUID = DB.RSFieldGUID(rs, "AppConfigGUID");
                                            AppConfigValue = DB.RSFieldGUID(rs, "ConfigValue");
                                        }
                                    }
                                }
                                catch { throw; }
                                finally
                                {
                                    // we can't dispose of the connection if it's part of a transaction
                                    if (con != null && m_DBTrans == null)
                                    {
                                        // here it's safe to dispose since we created the connection ourself
                                        con.Dispose();
                                    }

                                    // make sure we won't reference this again in code
                                    rs = null;
                                    con = null;
                                }


                            }
                            if (AppConfigID != 0 && AppConfigName.Length != 0)
                            {
                                Results.WriteAppConfigOutputEntry(NodeType, AppConfigName, AppConfigValue, AppConfigGUID, AppConfigID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                Results.WriteOutputEntry(NodeType, AppConfigName, AppConfigGUID, AppConfigID, NodeAction, "ERROR", "Not Found");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Not Found");
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return AppConfigID;
        }

        private int GetStoreIdByName(string StoreName)
        {
            List<Store> stores = Store.GetStoreList();
            foreach (Store s in stores)
                if (s.Name.EqualsIgnoreCase(StoreName))
                    return s.StoreID;

            return 0;
        }

        public Boolean ValidateStoreId(int StoreId)
        {
            Boolean found = false;
            List<Store> stores = Store.GetStoreList();
            foreach (Store s in stores)
                if (s.StoreID == StoreId)
                    found = true;
            return found;
        }

        public bool EntityStoreMappingExists(int StoreId, int EntityId, EntitySpecs Specs)
        {
            String sql = String.Format("select count(*) as N from EntityStore with (NOLOCK) where StoreId = {0} and EntityID= {1} and EntityType = {2}", StoreId, EntityId, DB.SQuote(Specs.m_EntityName.ToLower()));
            return DB.GetSqlN(sql, m_DBTrans) > 0;
        }

        public bool ProductStoreMappingExists(int StoreId, int ProductId)
        {
            String sql = String.Format("select count(*) as N from ProductStore with (NOLOCK) where ProductId={0} and StoreId = {1}", ProductId, StoreId);
            return DB.GetSqlN(sql, m_DBTrans) > 0;
        }

        public bool ProductEntityMappingExists(int StoreId, String EntityType, int EntityId)
        {
            String sql = String.Format("select count(*) as N from EntityStore with (NOLOCK) where EntityID={0} and EntityType={1} and StoreId={2}", EntityId, DB.SQuote(EntityType), StoreId);
            return DB.GetSqlN(sql, m_DBTrans) > 0;
        }

        private bool EntitySupported(String EntityType)
        {
            bool entitySupported = false;
            foreach (String entity in AppLogic.ro_SupportedEntities)
                if (entity.EqualsIgnoreCase(EntityType))
                    entitySupported = true;
            return entitySupported;
        }

        private bool ProcessProductStoreMapping(NodeActionEnum NodeAction, int StoreId, int ProductId)
        {
            bool exists = ProductStoreMappingExists(StoreId, ProductId);

            switch (NodeAction)
            {
                case NodeActionEnum.Add:
                    if (exists || !ValidateStoreId(StoreId))
                        return false;
                    RunCommand(String.Format("insert ProductStore(ProductID, StoreID) values({0}, {1})", ProductId, StoreId));
                    return true;
                case NodeActionEnum.Delete:
                    if (!exists)
                        return false;
                    RunCommand(String.Format("delete from ProductStore where ProductId = {0} and StoreId = {1}", ProductId, StoreId));
                    return true;
            }
            return false;
        }

        private bool ProcessEntityStoreMapping(NodeActionEnum NodeAction, int StoreId, String EntityType, int EntityId)
        {
            if (!EntitySupported(EntityType))
                    throw new ArgumentException("Invalid entity type: " + EntityType);

            bool exists = ProductEntityMappingExists(StoreId, EntityType, EntityId);
            switch (NodeAction)
            {
                case NodeActionEnum.Add:
                    if (exists || !ValidateStoreId(StoreId))
                        return false;
                    RunCommand(String.Format("insert EntityStore(StoreID, EntityID, EntityType) values({0}, {1}, {2})", StoreId, EntityId, DB.SQuote(EntityType.ToLower())));
                    return true;
                case NodeActionEnum.Delete:
                    if (!exists)
                        return false;
                    RunCommand(String.Format("delete from EntityStore where StoreID={0} and EntityID={1} and EntityType={2}", StoreId, EntityId, DB.SQuote(EntityType.ToLower())));
                    return true;
            }
            return false;
        }


        private void ProcessStoreMapping(XmlNode node)
        {
            String NodeType = node.Name;
            int StoreId = XmlCommon.XmlAttributeNativeInt(node, "StoreId");
            if (StoreId == 0)
                StoreId = XmlCommon.XmlAttributeNativeInt(node, "StoreID");
            String StoreName = XmlCommon.XmlAttribute(node, "StoreName");
            NodeActionEnum NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            String ObjectType = XmlCommon.XmlAttribute(node, "ObjectType");
            String EntityType = XmlCommon.XmlAttribute(node, "EntityType");
            int ObjectID = XmlCommon.XmlAttributeNativeInt(node, "ObjectId");
            if (ObjectID == 0)
                ObjectID = XmlCommon.XmlAttributeNativeInt(node, "ObjectID");

            Results.WriteVerboseEntry(String.Format("Processing {0}, StoreId = {1}, StoreName={2}, Action={3}, ObjectType={4}, EntityType={5}, ObjectId={6}", NodeType, StoreId, StoreName, NodeAction.ToString(), ObjectType, EntityType, ObjectID));

            if (StoreId == 0)
                StoreId = GetStoreIdByName(StoreName);

            if (StoreId == 0 || !ValidateStoreId(StoreId))
            {
                Results.WriteErrorEntry("Invalid StoreId (or StoreName did not match): " + StoreId);
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Invalid StoreId (or StoreName did not match): " + StoreId);
                }
                return;
            }

            if (ObjectType != "Product" && ObjectType != "Entity")
            {
                Results.WriteErrorEntry("Invalid ObjectType: " + ObjectType);
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Invalid ObjectType: " + ObjectType);
                }
                return;
            }

            try
            {

                if (ObjectType == "Product")
                {
                    ProcessProductStoreMapping(NodeAction, StoreId, ObjectID);
                }
                else if (ObjectType == "Entity")
                {
                    ProcessEntityStoreMapping(NodeAction, StoreId, EntityType, ObjectID);
                }
                Results.WriteVerboseEntry(NodeType + " OK");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessMapping(XmlNode node)
        {
            String NodeType = node.Name;
            NodeActionEnum NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            String EntityType = XmlCommon.XmlAttribute(node, "EntityType");
            EntitySpecs Specs = EntityDefinitions.LookupSpecs(EntityType);
            String ObjectType = XmlCommon.XmlAttribute(node, "ObjectType");
            String EntityGUID = XmlCommon.XmlAttribute(node, "EntityGUID");
            int EntityID = XmlCommon.XmlAttributeNativeInt(node, "EntityID");
            String ObjectGUID = XmlCommon.XmlAttribute(node, "ObjectGUID");
            int ObjectID = XmlCommon.XmlAttributeNativeInt(node, "ObjectID");
            int DisplayOrder = XmlCommon.XmlAttributeNativeInt(node, "DisplayOrder");

            Results.WriteVerboseEntry("Processing " + NodeType + ", EntityType=" + EntityType + ", EntityID=" + EntityID.ToString() + ", EntityGUID=" + EntityGUID + ", ObjectID=" + ObjectID.ToString() + ", ObjectGUID=" + ObjectGUID + ", DisplayOrder=" + DisplayOrder.ToString());

            if (ObjectType.Length == 0)
            {
                ObjectType = "Product";
            }
            if (ObjectType != "Product" && ObjectType != "Document")
            {
                Results.WriteErrorEntry("Invalid ObjectType: " + ObjectType);
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Invalid ObjectType: " + ObjectType);
                }
                return;
            }

            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Add:
                        {
                            EntityID = SetupEntityNodeID(EntityID, EntityGUID, EntityType, EntityType, String.Empty, NodeAction);
                            ObjectID = SetupNodeID(ObjectID, ObjectGUID, "Product", String.Empty, NodeAction);
                            if (EntityID == 0)
                            {
                                Results.WriteErrorEntry("Required EntityID or EntityGUID not provided or Invalid");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Required EntityID or EntityGUID not provided or Invalid");
                                }
                                return;
                            }
                            if (ObjectID == 0)
                            {
                                Results.WriteErrorEntry("Required ObjectID or ObjectGUID not provided or Invalid");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Required ObjectID or ObjectGUID not provided or Invalid");
                                }
                                return;
                            }
                            String sql = String.Empty;
                            if (Specs.m_EntityObjectMappingIs1to1)
                            {
                                sql = "delete from ~^ where ^ID=" + EntityID.ToString() + " and ~ID=" + ObjectID.ToString();
                                sql = sql.Replace("^", EntityType).Replace("~", ObjectType);
                                RunCommand(sql);
                            }
                            // ignore dups on the insert
                            try
                            {
                                sql = "insert ~^(^ID,~ID) values(" + EntityID.ToString() + "," + ObjectID.ToString() + ")";
                                sql = sql.Replace("^", EntityType).Replace("~", ObjectType);
                                RunCommand(sql);
                            }
                            catch { }
                            try
                            {
                                sql = "update ~^ set DisplayOrder=" + DisplayOrder.ToString() + " where ^ID=" + EntityID.ToString() + " and ~ID=" + ObjectID.ToString();
                                sql = sql.Replace("^", EntityType).Replace("~", ObjectType);
                                RunCommand(sql);
                            }
                            catch { }
                            Results.WriteVerboseEntry(NodeType + " OK");
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            EntityID = SetupEntityNodeID(EntityID, EntityGUID, EntityType, EntityType, String.Empty, NodeAction);
                            ObjectID = SetupNodeID(ObjectID, ObjectGUID, "Product", String.Empty, NodeAction);
                            if (EntityID == 0)
                            {
                                Results.WriteErrorEntry("Required EntityID or EntityGUID not provided or Invalid");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Required EntityID or EntityGUID not provided or Invalid");
                                }
                                return;
                            }
                            if (ObjectID == 0)
                            {
                                Results.WriteErrorEntry("Required ObjectID or ObjectGUID not provided or Invalid");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Required ObjectID or ObjectGUID not provided or Invalid");
                                }
                                return;
                            }
                            String sql = "delete from ~^ where ^ID=" + EntityID.ToString() + " and ~ID=" + ObjectID.ToString();
                            sql = sql.Replace("^", EntityType).Replace("~", ObjectType);
                            RunCommand(sql);
                            Results.WriteVerboseEntry(NodeType + " OK");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessGetMappings(XmlNode node)
        {
            String NodeType = node.Name;
            String EntityType = XmlCommon.XmlAttribute(node, "EntityType");
            EntitySpecs Specs = EntityDefinitions.LookupSpecs(EntityType);
            String EntityGUID = XmlCommon.XmlAttribute(node, "EntityGUID");
            int EntityID = XmlCommon.XmlAttributeNativeInt(node, "EntityID");

            Results.WriteVerboseEntry("Processing " + NodeType + ", EntityType=" + EntityType + ", EntityID=" + EntityID.ToString() + ", EntityGUID=" + EntityGUID);

            EntityID = SetupEntityNodeID(EntityID, EntityGUID, NodeType, EntityType, String.Empty, NodeActionEnum.Get);
            if (EntityID == 0)
            {
                Results.WriteErrorEntry("Required EntityID or EntityGUID not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required EntityID or EntityGUID not provided or Invalid");
                }
                return;
            }

            try
            {
                EntityID = SetupEntityNodeID(EntityID, EntityGUID, NodeType, EntityType, String.Empty, NodeActionEnum.Get);
                if (EntityID == 0)
                {
                    Results.WriteErrorEntry("Required EntityID or EntityGUID not provided or Invalid");
                    if (m_DBTrans != null)
                    {
                        throw new ArgumentException("Required EntityID or EntityGUID not provided or Invalid");
                    }
                    return;
                }
                String sql = "select ~^.~ID,DisplayOrder,~.~GUID from ~^  with (NOLOCK)  join ~  with (NOLOCK)  on ~^.~ID=~.~ID where ^ID=" + EntityID.ToString() + " order by DisplayOrder";
                sql = sql.Replace("^", EntityType).Replace("~", Specs.m_ObjectName);
                Results.WriteVerboseEntry("Executing SQL: " + sql);
                Results.WriteGetMappingsStartElement(EntityID, EntityGUID, EntityType);

                SqlConnection con = null;
                IDataReader rs = null;
                try
                {
                    string query = sql;
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        while (rs.Read())
                        {
                            Results.WriteMappingEntry(Specs.m_ObjectName, DB.RSFieldInt(rs, Specs.m_ObjectName + "ID"), DB.RSFieldGUID(rs, Specs.m_ObjectName + "GUID"), DB.RSFieldInt(rs, "DisplayOrder"));
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs = null;
                    con = null;
                }

                Results.WriteEndElement();
                Results.WriteVerboseEntry(NodeType + " OK");

            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void htpDBToXmlFieldString(StringBuilder tmpS, String XmlField, String DBField, IDataReader rs)
        {
            htpDBToXmlFieldString(tmpS, XmlField, DBField, rs, Localization.GetDefaultLocale());
        }

        private void htpDBToXmlFieldString(StringBuilder tmpS, String XmlField, String DBField, IDataReader rs, string locale)
        {
            tmpS.Append("<");
            tmpS.Append(XmlField);
            tmpS.Append(">");
            tmpS.Append(XmlCommon.XmlEncode(DB.RSFieldByLocale(rs, DBField, locale)));
            tmpS.Append("</");
            tmpS.Append(XmlField);
            tmpS.Append(">");
        }

        private void htpDBToXmlFieldInt(StringBuilder tmpS, String XmlField, String DBField, IDataReader rs)
        {
            tmpS.Append("<");
            tmpS.Append(XmlField);
            tmpS.Append(">");
            tmpS.Append(DB.RSFieldInt(rs, DBField).ToString());
            tmpS.Append("</");
            tmpS.Append(XmlField);
            tmpS.Append(">");
        }

        private void htpDBToXmlFieldBool(StringBuilder tmpS, String XmlField, String DBField, IDataReader rs)
        {
            tmpS.Append("<");
            tmpS.Append(XmlField);
            tmpS.Append(">");
            tmpS.Append(DB.RSFieldBool(rs, DBField).ToString());
            tmpS.Append("</");
            tmpS.Append(XmlField);
            tmpS.Append(">");
        }

        private void htpDBToXmlFieldTinyInt(StringBuilder tmpS, String XmlField, String DBField, IDataReader rs)
        {
            tmpS.Append("<");
            tmpS.Append(XmlField);
            tmpS.Append(">");
            tmpS.Append(DB.RSFieldTinyInt(rs, DBField).ToString());
            tmpS.Append("</");
            tmpS.Append(XmlField);
            tmpS.Append(">");
        }

        private void htpDBToXmlFieldDecimal(StringBuilder tmpS, String XmlField, String DBField, IDataReader rs)
        {
            tmpS.Append("<");
            tmpS.Append(XmlField);
            tmpS.Append(">");
            tmpS.Append(Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, DBField)));
            tmpS.Append("</");
            tmpS.Append(XmlField);
            tmpS.Append(">");
        }

        private void htpDBToXmlFieldDateTime(StringBuilder tmpS, String XmlField, String DBField, IDataReader rs)
        {
            tmpS.Append("<");
            tmpS.Append(XmlField);
            tmpS.Append(">");
            if (DB.RSFieldDateTime(rs, DBField) != System.DateTime.MinValue)
            {
                tmpS.Append(Localization.DateTimeStringForDB(DB.RSFieldDateTime(rs, DBField)));
            }
            tmpS.Append("</");
            tmpS.Append(XmlField);
            tmpS.Append(">");
        }

        private void GetImageData(StringBuilder tmpS, String EntityOrObjectName, String theSize, int ID)
        {
            String ImageUrl = AppLogic.LookupImage(EntityOrObjectName, ID, theSize, 1, Localization.GetDefaultLocale());
            String FN = String.Empty;
            if (ImageUrl.Length != 0)
            {
                FN = CommonLogic.SafeMapPath(ImageUrl);
            }
            tmpS.Append("<");
            tmpS.Append(theSize);
            if (ImageUrl.Length != 0 && ImageUrl.IndexOf("nopicture") == -1)
            {
                tmpS.Append(" Extension=\"");
                try
                {
                    tmpS.Append(Path.GetExtension(FN).ToLowerInvariant().Replace(".", ""));
                }
                catch { }
                tmpS.Append("\"");
                tmpS.Append(" URL=\"");
                tmpS.Append(XmlCommon.XmlEncodeAttribute(ImageUrl).ToLowerInvariant());
                tmpS.Append("\"");
            }
            tmpS.Append(">");
            if (ImageUrl.Length != 0 && ImageUrl.IndexOf("nopicture") == -1)
            {
                byte[] bdata = File.ReadAllBytes(FN);
                string s = Convert.ToBase64String(bdata, Base64FormattingOptions.InsertLineBreaks);
                tmpS.Append(s);
            }
            tmpS.Append("</");
            tmpS.Append(theSize);
            tmpS.Append(">");
        }

        private String hlpEntityXml(String EntityType, int EntityID, bool IncludeImages)
        {
            return hlpEntityXml(EntityType, EntityID, IncludeImages, Localization.GetDefaultLocale());
        }

        private String hlpEntityXml(String EntityType, int EntityID, bool IncludeImages, string locale)
        {
            EntitySpecs Specs = EntityDefinitions.LookupSpecs(EntityType);
            StringBuilder tmpS = new StringBuilder(4096);

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "select * from " + EntityType + "   with (NOLOCK)  where " + EntityType + "ID=" + EntityID.ToString();
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        htpDBToXmlFieldString(tmpS, "Name", "Name", rs, locale);
                        tmpS.Append("<XPath>");
                        tmpS.Append(XmlCommon.XmlEncode(Breadcrumb.GetEntityXPath(EntityID, EntityType, Localization.GetDefaultLocale())));
                        tmpS.Append("</XPath>");
                        tmpS.Append("<SE>");
                        htpDBToXmlFieldString(tmpS, "SEName", "SEName", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SETitle", "SETitle", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SEKeywords", "SEKeywords", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SEDescription", "SEDescription", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SENoScript", "SENoScript", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SEAltText", "SEAltText", rs, locale);
                        tmpS.Append("</SE>");
                        if (Specs.m_HasAddress)
                        {
                            htpDBToXmlFieldString(tmpS, "Address1", "Address1", rs, locale);
                            htpDBToXmlFieldString(tmpS, "Address2", "Address2", rs, locale);
                            htpDBToXmlFieldString(tmpS, "Suite", "Suite", rs, locale);
                            htpDBToXmlFieldString(tmpS, "City", "City", rs, locale);
                            htpDBToXmlFieldString(tmpS, "State", "State", rs, locale);
                            htpDBToXmlFieldString(tmpS, "ZipCode", "ZipCode", rs, locale);
                            htpDBToXmlFieldString(tmpS, "Country", "Country", rs, locale);
                            htpDBToXmlFieldString(tmpS, "Phone", "Phone", rs, locale);
                            htpDBToXmlFieldString(tmpS, "FAX", "FAX", rs, locale);
                            htpDBToXmlFieldString(tmpS, "URL", "URL", rs, locale);
                            htpDBToXmlFieldString(tmpS, "EMail", "EMail", rs, locale);
                        }
                        htpDBToXmlFieldString(tmpS, "Summary", "Summary", rs, locale);
                        htpDBToXmlFieldString(tmpS, "Description", "Description", rs, locale);
                        tmpS.Append("<Display>");
                        htpDBToXmlFieldString(tmpS, "XmlPackage", "XmlPackage", rs, locale);
                        htpDBToXmlFieldInt(tmpS, "ColWidth", "ColWidth", rs);
                        htpDBToXmlFieldInt(tmpS, "PageSize", "PageSize", rs);
                        htpDBToXmlFieldInt(tmpS, "SkinID", "SkinID", rs);
                        htpDBToXmlFieldString(tmpS, "TemplateName", "TemplateName", rs, locale);
                        tmpS.Append("</Display>");
                        tmpS.Append("<Images>");
                        if (IncludeImages)
                        {
                            GetImageData(tmpS, EntityType, "Icon", EntityID);
                            GetImageData(tmpS, EntityType, "Medium", EntityID);
                            GetImageData(tmpS, EntityType, "Large", EntityID);
                        }
                        htpDBToXmlFieldString(tmpS, "ImageFilenameOverride", "ImageFilenameOverride", rs, locale);
                        tmpS.Append("</Images>");
                        int QDisID = DB.RSFieldInt(rs, "QuantityDiscountID");

                        SqlConnection con2 = null;
                        IDataReader rs2 = null;
                        try
                        {
                            string query2 = "select * from QuantityDiscount  with (NOLOCK)  where QuantityDiscountID=" + QDisID.ToString();
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs2 = DB.GetRS(query2, m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con2 = new SqlConnection(DB.GetDBConn());
                                con2.Open();
                                rs2 = DB.GetRS(query2, con2);
                            }

                            using (rs2)
                            {
                                if (rs2.Read())
                                {
                                    if (locale == "AllLocales")
                                    {
                                        tmpS.Append("<QuantityDiscountID Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs2, "Name", Localization.GetDefaultLocale())) + "\" ID=\"" + QDisID + "\" GUID=\"" + DB.RSFieldGUID(rs2, "QuantityDiscountGUID") + "\" DisplayOrder=\"" + DB.RSFieldInt(rs2, "DisplayOrder") + "\" ExtensionData=\"" + DB.RSField(rs2, "ExtensionData") + "\" DiscountType=\"" + DB.RSFieldTinyInt(rs2, "DiscountType") + "\" CreatedOn=\"" + DB.RSFieldDateTime(rs2, "CreatedOn") + "\">");
                                        tmpS.Append("<name>" + XmlCommon.XmlEncodeAttribute(DB.RSField(rs2, "Name")) + "</name>");
                                        tmpS.Append("</QuantityDiscountID>");
                                    }
                                    else
                                    {
                                        tmpS.Append("<QuantityDiscount Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs2, "Name", Localization.GetDefaultLocale())) + "\" ID=\"" + QDisID + "\" GUID=\"" + DB.RSFieldGUID(rs2, "QuantityDiscountGUID") + "\"/>");
                                    }
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (con2 != null && m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con2.Dispose();
                            }

                            // make sure we won't reference this again in code
                            rs2 = null;
                            con2 = null;
                        }

                        htpDBToXmlFieldInt(tmpS, "DisplayOrder", "DisplayOrder", rs);
                        htpDBToXmlFieldBool(tmpS, "Published", "Published", rs);
                        htpDBToXmlFieldBool(tmpS, "Wholesale", "Wholesale", rs);
                        htpDBToXmlFieldString(tmpS, "ExtensionData", "ExtensionData", rs, locale);
                        tmpS.Append("<StoreMappings AutoCleanup=\"true\">");
                        using (SqlConnection StoreMappingConn = DB.dbConn())
                        {
                            StoreMappingConn.Open();
                            using (IDataReader StoreMappingRS = DB.GetRS(String.Format("select StoreID from EntityStore where EntityID = {0} and EntityType = {1}", EntityID, DB.SQuote(EntityType)), StoreMappingConn))
                            {
                                while (StoreMappingRS.Read())
                                {
                                    int sid = DB.RSFieldInt(StoreMappingRS, "StoreID");
                                    tmpS.Append(String.Format("<Store StoreId=\"{0}\" />", sid));
                                }
                            }
                        }
                        tmpS.Append("</StoreMappings>");

                        if (locale == "AllLocales" &&
                            EntityType.Trim().Equals("category", StringComparison.InvariantCultureIgnoreCase))
                        {
                            htpDBToXmlFieldInt(tmpS, "TaxClassID", "TaxClassID", rs);
                        }
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }



            return tmpS.ToString();
        }

        private String hlpGetEntity(String EntityType, EntityHelper h, int EntityID, bool Recursive, bool IncludeImages)
        {
            return hlpGetEntity(EntityType, h, EntityID, Recursive, IncludeImages, Localization.GetDefaultLocale());

        }

        private String hlpGetEntity(String EntityType, EntityHelper h, int EntityID, bool Recursive, bool IncludeImages, string locale)
        {
            StringBuilder tmpS = new StringBuilder(4096);
            XmlNode n;
            if (EntityID == 0)
            {
                n = h.m_TblMgr.ResetToRootNode();
            }
            else
            {
                n = h.m_TblMgr.SetContext(EntityID);
            }
            if (n != null)
            {
                int ThisID = h.m_TblMgr.CurrentID(n);
                String ThisGUID = h.m_TblMgr.CurrentGUID(n);

                if (ThisGUID.Length == 0)
                {
                    SqlConnection coni = null;
                    IDataReader rsi = null;
                    try
                    {
                        string query = "select ^GUID from ^ where ^ID=".Replace("^", EntityType) + EntityID.ToString();
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            rsi = DB.GetRS(query, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            coni = new SqlConnection(DB.GetDBConn());
                            coni.Open();
                            rsi = DB.GetRS(query, coni);
                        }

                        using (rsi)
                        {
                            if (rsi.Read())
                            {
                                ThisGUID = DB.RSFieldGUID(rsi, EntityType + "GUID");
                            }
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (coni != null && m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            coni.Dispose();
                        }

                        // make sure we won't reference this again in code
                        rsi = null;
                        coni = null;
                    }
                }

                String ThisName = h.m_TblMgr.CurrentName(n, Localization.GetDefaultLocale());
                tmpS.Append("<Entity EntityType=\"" + EntityType + "\" Action=\"Update\" ID=\"" + ThisID.ToString() + "\" GUID=\"" + ThisGUID + "\" Name=\"" + XmlCommon.XmlEncodeAttribute(ThisName) + "\">");
                tmpS.Append(hlpEntityXml(EntityType, ThisID, IncludeImages, locale));
                if (Recursive && h.m_TblMgr.HasChildren(n))
                {
                    n = h.m_TblMgr.MoveFirstChild(n);
                    while (n != null)
                    {
                        ThisID = h.m_TblMgr.CurrentID(n);
                        ThisGUID = h.m_TblMgr.CurrentGUID(n);
                        tmpS.Append(hlpGetEntity(EntityType, h, ThisID, Recursive, IncludeImages, locale));
                        n = h.m_TblMgr.MoveNextSibling(n, false);
                    }
                }
                tmpS.Append("</Entity>");
            }
            return tmpS.ToString();
        }

        private void hlpGetVariant(StringBuilder tmpS, int VariantID, IDataReader rs, bool IncludeImages, bool TracksInventoryBySizeAndColor)
        {
            hlpGetVariant(tmpS, VariantID, rs, IncludeImages, TracksInventoryBySizeAndColor, Localization.GetDefaultLocale());
        }

        private void hlpGetVariant(StringBuilder tmpS, int VariantID, IDataReader rs, bool IncludeImages, bool TracksInventoryBySizeAndColor, string locale)
        {
            tmpS.Append("<Variant Action=\"Update\" ID=\"" + VariantID.ToString() + "\" Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale())) + "\" SKUSuffix=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "SKUSuffix", Localization.GetDefaultLocale())) + "\" GUID=\"" + DB.RSFieldGUID(rs, "VariantGUID") + "\">");
            htpDBToXmlFieldBool(tmpS, "IsDefault", "IsDefault", rs);
            htpDBToXmlFieldString(tmpS, "Name", "Name", rs, locale);
            htpDBToXmlFieldString(tmpS, "Description", "Description", rs, locale);
            tmpS.Append("<SE>");
            htpDBToXmlFieldString(tmpS, "SEName", "SEName", rs, locale);
            htpDBToXmlFieldString(tmpS, "SEKeywords", "SEKeywords", rs, locale);
            htpDBToXmlFieldString(tmpS, "SEDescription", "SEDescription", rs, locale);
            tmpS.Append("</SE>");
            htpDBToXmlFieldString(tmpS, "FroogleDescription", "FroogleDescription", rs, locale);
            htpDBToXmlFieldInt(tmpS, "ProductID", "ProductID", rs);
            htpDBToXmlFieldString(tmpS, "SKUSuffix", "SKUSuffix", rs, locale);
            htpDBToXmlFieldString(tmpS, "ManufacturerPartNumber", "ManufacturerPartNumber", rs, locale);
			htpDBToXmlFieldString(tmpS, "GTIN", "GTIN", rs, locale);
            htpDBToXmlFieldDecimal(tmpS, "Price", "Price", rs);
            htpDBToXmlFieldDecimal(tmpS, "SalePrice", "SalePrice", rs);
            htpDBToXmlFieldDecimal(tmpS, "Weight", "Weight", rs);
            htpDBToXmlFieldDecimal(tmpS, "MSRP", "MSRP", rs);
            htpDBToXmlFieldDecimal(tmpS, "Cost", "Cost", rs);
            htpDBToXmlFieldInt(tmpS, "Points", "Points", rs);
            htpDBToXmlFieldString(tmpS, "Dimensions", "Dimensions", rs, locale);
            htpDBToXmlFieldInt(tmpS, "Inventory", "Inventory", rs);
            htpDBToXmlFieldInt(tmpS, "DisplayOrder", "DisplayOrder", rs);
            htpDBToXmlFieldString(tmpS, "Notes", "Notes", rs, locale);
            htpDBToXmlFieldBool(tmpS, "IsTaxable", "IsTaxable", rs);
            htpDBToXmlFieldBool(tmpS, "IsShipSeparately", "IsShipSeparately", rs);
            htpDBToXmlFieldBool(tmpS, "IsDownload", "IsDownload", rs);
            htpDBToXmlFieldString(tmpS, "DownloadLocation", "DownloadLocation", rs, locale);
            htpDBToXmlFieldTinyInt(tmpS, "FreeShipping", "FreeShipping", rs);
            htpDBToXmlFieldBool(tmpS, "Published", "Published", rs);
            htpDBToXmlFieldBool(tmpS, "Wholesale", "Wholesale", rs);
            htpDBToXmlFieldBool(tmpS, "IsRecurring", "IsRecurring", rs);
            htpDBToXmlFieldInt(tmpS, "RecurringInterval", "RecurringInterval", rs);
            htpDBToXmlFieldInt(tmpS, "RecurringIntervalType", "RecurringIntervalType", rs);
            htpDBToXmlFieldInt(tmpS, "SubscriptionInterval", "SubscriptionInterval", rs);
            htpDBToXmlFieldInt(tmpS, "SubscriptionIntervalType", "SubscriptionIntervalType", rs);
            htpDBToXmlFieldString(tmpS, "RestrictedQuantities", "RestrictedQuantities", rs, locale);
            htpDBToXmlFieldInt(tmpS, "MinimumQuantity", "MinimumQuantity", rs);
            tmpS.Append("<Images>");
            if (IncludeImages)
            {
                GetImageData(tmpS, "Variant", "Icon", VariantID);
                GetImageData(tmpS, "Variant", "Medium", VariantID);
                GetImageData(tmpS, "Variant", "Large", VariantID);
            }
            htpDBToXmlFieldString(tmpS, "ImageFilenameOverride", "ImageFilenameOverride", rs, locale);
            tmpS.Append("</Images>");
            htpDBToXmlFieldBool(tmpS, "CustomerEntersPrice", "CustomerEntersPrice", rs);
            htpDBToXmlFieldString(tmpS, "CustomerEntersPricePrompt", "CustomerEntersPricePrompt", rs, locale);
            htpDBToXmlFieldString(tmpS, "ExtensionData", "ExtensionData", rs, locale);
            htpDBToXmlFieldString(tmpS, "ExtensionData2", "ExtensionData2", rs, locale);
            htpDBToXmlFieldString(tmpS, "ExtensionData3", "ExtensionData3", rs, locale);
            htpDBToXmlFieldString(tmpS, "ExtensionData4", "ExtensionData4", rs, locale);
            htpDBToXmlFieldString(tmpS, "ExtensionData5", "ExtensionData5", rs, locale);
            tmpS.Append("<Sizes>");
            String SizesMaster = DB.RSFieldByLocale(rs, "Sizes", Localization.GetDefaultLocale()).Trim();
            if (SizesMaster.Length != 0)
            {
                String SizeSKUModifiers = DB.RSField(rs, "SizeSKUModifiers").Trim();
                String[] SizesMasterSplit = SizesMaster.Split(',');
                String[] SizeSKUsSplit = SizeSKUModifiers.Split(',');
                for (int i = SizesMasterSplit.GetLowerBound(0); i <= SizesMasterSplit.GetUpperBound(0); i++)
                {
                    String[] SizesX = SizesMasterSplit[i].Replace(']', '[').Split('[');
                    String SizeText = SizesX[0].Trim();
                    Decimal SizePriceDelta = System.Decimal.Zero;
                    String Modifier = String.Empty;
                    if (SizesX.GetUpperBound(0) >= 1)
                    {
                        SizePriceDelta = Localization.ParseNativeDecimal(SizesX[1].Trim());
                    }
                    try
                    {
                        Modifier = SizeSKUsSplit[i].Trim();
                    }
                    catch { }
                    tmpS.Append("<Size SKUModifier=\"" + XmlCommon.XmlEncodeAttribute(Modifier) + "\" PriceDelta=\"" + Localization.DecimalStringForDB(SizePriceDelta) + "\">" + XmlCommon.XmlEncodeAttribute(SizeText) + "</Size>");
                }
            }
            tmpS.Append("</Sizes>");
            tmpS.Append("<Colors>");
            String ColorsMaster = DB.RSFieldByLocale(rs, "Colors", Localization.GetDefaultLocale()).Trim();
            if (ColorsMaster.Length != 0)
            {
                String ColorSKUModifiers = DB.RSField(rs, "ColorSKUModifiers").Trim();
                String[] ColorsMasterSplit = ColorsMaster.Split(',');
                String[] ColorSKUsSplit = ColorSKUModifiers.Split(',');
                for (int i = ColorsMasterSplit.GetLowerBound(0); i <= ColorsMasterSplit.GetUpperBound(0); i++)
                {
                    String[] ColorsX = ColorsMasterSplit[i].Replace(']', '[').Split('[');
                    String ColorText = ColorsX[0].Trim();
                    Decimal ColorPriceDelta = System.Decimal.Zero;
                    String Modifier = String.Empty;
                    if (ColorsX.GetUpperBound(0) >= 1)
                    {
                        ColorPriceDelta = Localization.ParseNativeDecimal(ColorsX[1].Trim());
                    }
                    try
                    {
                        Modifier = ColorSKUsSplit[i].Trim();
                    }
                    catch { }
                    tmpS.Append("<Color SKUModifier=\"" + XmlCommon.XmlEncodeAttribute(Modifier) + "\" PriceDelta=\"" + Localization.DecimalStringForDB(ColorPriceDelta) + "\">" + XmlCommon.XmlEncodeAttribute(ColorText) + "</Color>");
                }
            }
            tmpS.Append("</Colors>");
            if (TracksInventoryBySizeAndColor)
            {
                bool first = true;

                SqlConnection coni = null;
                IDataReader rsi = null;
                try
                {
                    string query = "select * from Inventory  with (NOLOCK)  where VariantID=" + VariantID.ToString();
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rsi = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        coni = new SqlConnection(DB.GetDBConn());
                        coni.Open();
                        rsi = DB.GetRS(query, coni);
                    }

                    using (rsi)
                    {
                        while (rsi.Read())
                        {
                            if (first)
                            {
                                // look at first record, if it has VendorFullSKU, use that as MatchKey for all records, else if VendorID then use that, or else default to SizeColor match (default)
                                tmpS.Append("<InventoryBySizeAndColor MatchKey=\"");
                                if (DB.RSField(rsi, "VendorFullSKU").Trim().Length != 0)
                                {
                                    tmpS.Append("VendorFullSKU");
                                }
                                else if (DB.RSField(rsi, "VendorID").Trim().Length != 0)
                                {
                                    tmpS.Append("VendorID");
                                }
                                else
                                {
                                    tmpS.Append("SizeColor");
                                }
                                tmpS.Append("\">");
                            }
                            tmpS.Append("<Inv ");
                            tmpS.Append("Size=\"");
                            tmpS.Append(XmlCommon.XmlEncodeAttribute(DB.RSField(rsi, "Size")));
                            tmpS.Append("\" ");
                            tmpS.Append("Color=\"");
                            tmpS.Append(XmlCommon.XmlEncodeAttribute(DB.RSField(rsi, "Color")));
                            tmpS.Append("\" ");
                            tmpS.Append("Quantity=\"");
                            tmpS.Append(DB.RSFieldInt(rsi, "Quan").ToString());
                            tmpS.Append("\" ");
							tmpS.Append("GTIN=\"");
							tmpS.Append(DB.RSFieldInt(rsi, "GTIN").ToString());
							tmpS.Append("\" ");
                            tmpS.Append("VendorID=\"");
                            tmpS.Append(XmlCommon.XmlEncodeAttribute(DB.RSField(rsi, "VendorID")));
                            tmpS.Append("\" ");
                            tmpS.Append("VendorFullSKU=\"");
                            tmpS.Append(XmlCommon.XmlEncodeAttribute(DB.RSField(rsi, "VendorFullSKU")));
                            tmpS.Append("\" ");
                            tmpS.Append("WeightDelta=\"");
                            tmpS.Append(Localization.DecimalStringForDB(DB.RSFieldDecimal(rsi, "WeightDelta")));
                            tmpS.Append("\" ");
                            tmpS.Append("ExtensionData=\"");
                            tmpS.Append(XmlCommon.XmlEncodeAttribute(DB.RSField(rsi, "ExtensionData")));
                            tmpS.Append("\"");
                            tmpS.Append("/>");
                            first = false;
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (coni != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        coni.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rsi = null;
                    coni = null;
                }

                if (first)
                {
                    tmpS.Append("<InventoryBySizeAndColor>");
                }
                tmpS.Append("</InventoryBySizeAndColor>");
            }
            tmpS.Append("</Variant>");
        }

        private void hlpGetKitGroup(StringBuilder tmpS, int KitGroupID, IDataReader rs)
        {
            tmpS.Append("<KitGroup ID=\"" + KitGroupID.ToString() + "\" Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale())) + "\" GUID=\"" + DB.RSFieldGUID(rs, "KitGroupGUID") + "\">");
            htpDBToXmlFieldString(tmpS, "Name", "Name", rs);
            htpDBToXmlFieldString(tmpS, "Description", "Description", rs);
            htpDBToXmlFieldInt(tmpS, "DisplayOrder", "DisplayOrder", rs);
            htpDBToXmlFieldInt(tmpS, "KitGroupTypeID", "KitGroupTypeID", rs);
            htpDBToXmlFieldBool(tmpS, "IsRequired", "IsRequired", rs);
			htpDBToXmlFieldBool(tmpS, "IsReadOnly", "IsReadOnly", rs);

            SqlConnection conv = null;
            IDataReader rsv = null;
            try
            {
                string query = "select * from KitItem  with (NOLOCK)  where KitGroupID=" + KitGroupID.ToString() + " order by DisplayOrder,Name";
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rsv = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    conv = new SqlConnection(DB.GetDBConn());
                    conv.Open();
                    rsv = DB.GetRS(query, conv);
                }

                using (rsv)
                {
                    while (rsv.Read())
                    {
                        hlpGetKitItem(tmpS, DB.RSFieldInt(rsv, "KitItemID"), rsv);
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (conv != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    conv.Dispose();
                }

                // make sure we won't reference this again in code
                rsv = null;
                conv = null;
            }

            tmpS.Append("</KitGroup>");
        }

        private void hlpGetKitItem(StringBuilder tmpS, int KitItemID, IDataReader rs)
        {
            tmpS.Append("<KitItem ID=\"" + KitItemID.ToString() + "\" Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale())) + "\" GUID=\"" + DB.RSFieldGUID(rs, "KitItemGUID") + "\">");
            htpDBToXmlFieldString(tmpS, "Name", "Name", rs);
            htpDBToXmlFieldString(tmpS, "Description", "Description", rs);
            htpDBToXmlFieldDecimal(tmpS, "PriceDelta", "PriceDelta", rs);
            htpDBToXmlFieldDecimal(tmpS, "WeightDelta", "WeightDelta", rs);
            htpDBToXmlFieldInt(tmpS, "DisplayOrder", "DisplayOrder", rs);
            htpDBToXmlFieldBool(tmpS, "IsDefault", "IsDefault", rs);
            htpDBToXmlFieldInt(tmpS, "TextOptionMaxLength", "TextOptionMaxLength", rs);
            htpDBToXmlFieldInt(tmpS, "TextOptionHeight", "TextOptionHeight", rs);
            htpDBToXmlFieldInt(tmpS, "TextOptionWidth", "TextOptionWidth", rs);
			htpDBToXmlFieldInt(tmpS, "InventoryQuantityDelta", "InventoryQuantityDelta", rs);
            tmpS.Append("</KitItem>");
        }

        private String hlpGetProduct(int ProductID, bool IncludeVariants, bool IncludeImages)
        {
            return hlpGetProduct(ProductID, IncludeVariants, IncludeImages, Localization.GetDefaultLocale());
        }

        private String hlpGetProduct(int ProductID, bool IncludeVariants, bool IncludeImages, string locale)
        {
            StringBuilder tmpS = new StringBuilder(4096);

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "select * from Product  with (NOLOCK)  where Deleted<>1 and ProductID=" + ProductID.ToString();
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        tmpS.Append("<Product Action=\"Update\" ID=\"" + ProductID.ToString() + "\" Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "Name", Localization.GetDefaultLocale())) + "\" SKU=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs, "SKU", Localization.GetDefaultLocale())) + "\" GUID=\"" + DB.RSFieldGUID(rs, "ProductGUID") + "\">");
                        htpDBToXmlFieldString(tmpS, "Name", "Name", rs, locale);
                        htpDBToXmlFieldString(tmpS, "Summary", "Summary", rs, locale);
                        htpDBToXmlFieldString(tmpS, "Description", "Description", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SpecTitle", "SpecTitle", rs, locale);
                        htpDBToXmlFieldString(tmpS, "MiscText", "MiscText", rs, locale);
                        htpDBToXmlFieldString(tmpS, "Notes", "Notes", rs, locale);
                        htpDBToXmlFieldString(tmpS, "IsFeaturedTeaser", "IsFeaturedTeaser", rs, locale);
                        htpDBToXmlFieldString(tmpS, "FroogleDescription", "FroogleDescription", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SKU", "SKU", rs, locale);
                        htpDBToXmlFieldString(tmpS, "ManufacturerPartNumber", "ManufacturerPartNumber", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SwatchImageMap", "SwatchImageMap", rs, locale);
                        tmpS.Append("<SE>");
                        htpDBToXmlFieldString(tmpS, "SEName", "SEName", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SETitle", "SETitle", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SEKeywords", "SEKeywords", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SEDescription", "SEDescription", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SENoScript", "SENoScript", rs, locale);
                        htpDBToXmlFieldString(tmpS, "SEAltText", "SEAltText", rs, locale);
                        tmpS.Append("</SE>");
                        htpDBToXmlFieldString(tmpS, "SizeOptionPrompt", "SizeOptionPrompt", rs, locale);
                        htpDBToXmlFieldString(tmpS, "ColorOptionPrompt", "ColorOptionPrompt", rs, locale);
                        int ProductTypeID = DB.RSFieldInt(rs, "ProductTypeID");


                        using (SqlConnection con2 = DB.dbConn())
                        {
                            con2.Open();
                            using (IDataReader rs2 = DB.GetRS("select * from ProductType  with (NOLOCK)  where ProductTypeID=" + ProductTypeID.ToString(), con2))
                            {
                                if (rs2.Read())
                                {
                                    tmpS.Append("<ProductType Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs2, "Name", Localization.GetDefaultLocale())) + "\" ID=\"" + ProductTypeID.ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs2, "ProductTypeGUID") + "\"/>");
                                }
                            }
                        }


                        using (SqlConnection con3 = DB.dbConn())
                        {
                            con3.Open();

                            int TaxClassID = DB.RSFieldInt(rs, "TaxClassID");

                            using (IDataReader rs3 = DB.GetRS("select * from TaxClass  with (NOLOCK)  where TaxClassID=" + TaxClassID.ToString(), con3))
                            {
                                if (rs3.Read())
                                {
                                    if (locale == "AllLocales")
                                    {
                                        tmpS.Append("<TaxClass Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs3, "Name", Localization.GetDefaultLocale())) + "\" ID=\"" + TaxClassID.ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs3, "TaxClassGUID") + "\" TaxCode=\"" + DB.RSField(rs3, "TaxCode") + "\" DisplayOrder=\"" + DB.RSFieldInt(rs3, "DisplayOrder") + "\" >");
                                        tmpS.Append("<name>" + XmlCommon.XmlEncodeAttribute(DB.RSField(rs3, "Name")) + "</name>");
                                        tmpS.Append("</TaxClass>");
                                    }
                                    else
                                    {
                                        tmpS.Append("<TaxClass Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs3, "Name", Localization.GetDefaultLocale())) + "\" ID=\"" + TaxClassID.ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs3, "TaxClassGUID") + "\"/>");
                                    }
                                }
                            }
                        }


                        using (SqlConnection con4 = DB.dbConn())
                        {
                            con4.Open();

                            int SalesPromptID = DB.RSFieldInt(rs, "SalesPromptID");

                            using (IDataReader rs4 = DB.GetRS("select * from SalesPrompt  with (NOLOCK)  where Deleted=0 and SalesPromptID=" + SalesPromptID.ToString(), con4))
                            {
                                if (rs4.Read())
                                {
                                    tmpS.Append("<SalesPrompt Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs4, "Name", Localization.GetDefaultLocale())) + "\" ID=\"" + SalesPromptID.ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs4, "SalesPromptGUID") + "\"/>");
                                }
                            }
                        }
                        htpDBToXmlFieldString(tmpS, "SpecCall", "SpecCall", rs, locale);
                        htpDBToXmlFieldBool(tmpS, "SpecsInline", "SpecsInline", rs);
                        tmpS.Append("<Display>");
                        htpDBToXmlFieldString(tmpS, "XmlPackage", "XmlPackage", rs, locale);
                        htpDBToXmlFieldInt(tmpS, "ColWidth", "ColWidth", rs);
                        htpDBToXmlFieldInt(tmpS, "PageSize", "PageSize", rs);
                        htpDBToXmlFieldInt(tmpS, "SkinID", "SkinID", rs);
                        htpDBToXmlFieldString(tmpS, "TemplateName", "TemplateName", rs, locale);
                        tmpS.Append("</Display>");
                        tmpS.Append("<Images>");
                        if (IncludeImages)
                        {
                            GetImageData(tmpS, "Product", "Icon", ProductID);
                            GetImageData(tmpS, "Product", "Medium", ProductID);
                            GetImageData(tmpS, "Product", "Large", ProductID);

                            ProductImageGallery ig = new ProductImageGallery(ProductID, 1, Localization.GetDefaultLocale(), DB.RSField(rs, "SKU"));
                            String m_Colors = String.Empty;
                            String[] m_ColorsSplit = new String[1] { "" };


                            using (SqlConnection con5 = DB.dbConn())
                            {
                                con5.Open();
                                using (IDataReader rs5 = DB.GetRS("select Colors from ProductVariant  with (NOLOCK)  where IsDefault=1 and Deleted<>1 and ProductID=" + ProductID.ToString(), con5))
                                {
                                    if (rs5.Read())
                                    {
                                        m_Colors = DB.RSFieldByLocale(rs5, "Colors", Localization.GetDefaultLocale()); // remember to add "empty" color to front, for no color selected
                                        if (m_Colors.Length != 0)
                                        {
                                            m_ColorsSplit = ("," + m_Colors).Split(',');
                                        }
                                    }
                                }
                            }

                            if (m_Colors.Length != 0)
                            {
                                for (int i = m_ColorsSplit.GetLowerBound(0); i <= m_ColorsSplit.GetUpperBound(0); i++)
                                {
                                    String s2 = AppLogic.RemoveAttributePriceModifier(m_ColorsSplit[i]);
                                    m_ColorsSplit[i] = CommonLogic.MakeSafeFilesystemName(s2);
                                }
                            }
                            if (!ig.IsEmpty())
                            {
                                tmpS.Append("<MultiImage>");
                                foreach (String imagesize in "Icon,Medium,Large".Split(','))
                                {
                                    tmpS.Append("<" + imagesize + ">");
                                    for (int i = 1; i <= ig.MaxImageIndex; i++)
                                    {
                                        foreach (String c in m_ColorsSplit)
                                        {
                                            String FN = CommonLogic.SafeMapPath(ig.ImageUrl(i, c, imagesize).ToLowerInvariant());
                                            if (File.Exists(FN))
                                            {
                                                Byte[] bdata = File.ReadAllBytes(FN);
                                                String Base64Data = Convert.ToBase64String(bdata, Base64FormattingOptions.InsertLineBreaks);
                                                tmpS.Append(String.Format("<Img Index=\"{0}\" Color=\"{1}\" Extension=\"{2}\">{3}</Img>", i.ToString(), c, Path.GetExtension(FN).ToLowerInvariant().Replace(".", ""), Base64Data));
                                            }
                                        }
                                    }
                                    tmpS.Append("</" + imagesize + ">");
                                }
                                tmpS.Append("</MultiImage>");
                            }
                        }
                        htpDBToXmlFieldString(tmpS, "ImageFilenameOverride", "ImageFilenameOverride", rs, locale);
                        tmpS.Append("</Images>");


                        using (SqlConnection con6 = DB.dbConn())
                        {
                            con6.Open();

                            int QuantityDiscountID = DB.RSFieldInt(rs, "QuantityDiscountID");

                            using (IDataReader rs6 = DB.GetRS("select * from QuantityDiscount  with (NOLOCK)  where QuantityDiscountID=" + QuantityDiscountID.ToString(), con6))
                            {
                                if (rs6.Read())
                                {
                                    if (locale == "AllLocales")
                                    {
                                        tmpS.Append("<QuantityDiscountID Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs6, "Name", Localization.GetDefaultLocale())) + "\" ID=\"" + QuantityDiscountID.ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs6, "QuantityDiscountGUID") + "\" DisplayOrder=\"" + DB.RSFieldInt(rs6, "DisplayOrder") + "\" ExtensionData=\"" + DB.RSField(rs6, "ExtensionData") + "\" DiscountType=\"" + DB.RSFieldTinyInt(rs6, "DiscountType") + "\" CreatedOn=\"" + DB.RSFieldDateTime(rs6, "CreatedOn") + "\">");
                                        tmpS.Append("<name>" + XmlCommon.XmlEncodeAttribute(DB.RSField(rs6, "Name")) + "</name>");
                                        tmpS.Append("</QuantityDiscountID>");

                                    }
                                    else
                                    {
                                        tmpS.Append("<QuantityDiscount Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs6, "Name", Localization.GetDefaultLocale())) + "\" ID=\"" + QuantityDiscountID.ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs6, "QuantityDiscountGUID") + "\"/>");
                                    }
                                }
                            }
                        }

                        tmpS.Append("<RelatedProducts>");
                        if (DB.RSField(rs, "RelatedProducts").Trim().Length != 0)
                        {
                            foreach (String s in DB.RSField(rs, "RelatedProducts").Trim().Split(','))
                            {
                                try
                                {
                                    int PID = Localization.ParseNativeInt(s);

                                    using (SqlConnection con7 = DB.dbConn())
                                    {
                                        con7.Open();
                                        using (IDataReader rs7 = DB.GetRS("select ProductID,ProductGUID from Product  with (NOLOCK)  where Deleted<>1 and ProductID=" + PID.ToString(), con7))
                                        {
                                            if (rs7.Read())
                                            {
                                                tmpS.Append("<CX ID=\"" + DB.RSFieldInt(rs7, "ProductID").ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs7, "ProductGUID") + "\"/>");
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        tmpS.Append("</RelatedProducts>");
                        tmpS.Append("<UpsellProducts DiscountPercentage=\"" + Localization.DecimalStringForDB(DB.RSFieldDecimal(rs, "UpsellProductDiscountPercentage")) + "\">");
                        if (DB.RSField(rs, "UpsellProducts").Trim().Length != 0)
                        {
                            foreach (String s in DB.RSField(rs, "UpsellProducts").Trim().Split(','))
                            {
                                try
                                {
                                    int PID = Localization.ParseNativeInt(s);

                                    using (SqlConnection con8 = DB.dbConn())
                                    {
                                        con8.Open();
                                        using (IDataReader rs8 = DB.GetRS("select ProductID,ProductGUID from Product  with (NOLOCK)  where Deleted<>1 and ProductID=" + PID.ToString(), con8))
                                        {
                                            if (rs8.Read())
                                            {
                                                tmpS.Append("<CX ID=\"" + DB.RSFieldInt(rs8, "ProductID").ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs8, "ProductGUID") + "\"/>");
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        tmpS.Append("</UpsellProducts>");
                        tmpS.Append("<RequiresProducts>");
                        if (DB.RSField(rs, "RequiresProducts").Trim().Length != 0)
                        {
                            foreach (String s in DB.RSField(rs, "RequiresProducts").Trim().Split(','))
                            {
                                try
                                {
                                    int PID = Localization.ParseNativeInt(s);


                                    using (SqlConnection con9 = DB.dbConn())
                                    {
                                        con9.Open();
                                        using (IDataReader rs9 = DB.GetRS("select ProductID,ProductGUID from Product  with (NOLOCK)  where Deleted<>1 and ProductID=" + PID.ToString(), con9))
                                        {
                                            if (rs9.Read())
                                            {
                                                tmpS.Append("<CX ID=\"" + DB.RSFieldInt(rs9, "ProductID").ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs9, "ProductGUID") + "\"/>");
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        tmpS.Append("</RequiresProducts>");
                        tmpS.Append("<InventoryType>");
                        htpDBToXmlFieldBool(tmpS, "TrackInventoryBySizeAndColor", "TrackInventoryBySizeAndColor", rs);
                        htpDBToXmlFieldString(tmpS, "WarehouseLocation", "WarehouseLocation", rs, locale);
                        tmpS.Append("</InventoryType>");
                        htpDBToXmlFieldBool(tmpS, "IsFeatured", "IsFeatured", rs);
                        htpDBToXmlFieldBool(tmpS, "IsAKit", "IsAKit", rs);
                        htpDBToXmlFieldBool(tmpS, "IsSystem", "IsSystem", rs);
                        htpDBToXmlFieldBool(tmpS, "ShowBuyButton", "ShowBuyButton", rs);
                        htpDBToXmlFieldBool(tmpS, "Published", "Published", rs);
                        htpDBToXmlFieldBool(tmpS, "Wholesale", "Wholesale", rs);
                        htpDBToXmlFieldBool(tmpS, "RequiresRegistration", "RequiresRegistration", rs);
                        htpDBToXmlFieldBool(tmpS, "HidePriceUntilCart", "HidePriceUntilCart", rs);
                        htpDBToXmlFieldBool(tmpS, "IsCallToOrder", "IsCallToOrder", rs);
                        htpDBToXmlFieldBool(tmpS, "ExcludeFromPriceFeeds", "ExcludeFromPriceFeeds", rs);
                        htpDBToXmlFieldBool(tmpS, "RequiresTextOption", "RequiresTextOption", rs);
                        htpDBToXmlFieldInt(tmpS, "TextOptionMaxLength", "TextOptionMaxLength", rs);
                        htpDBToXmlFieldString(tmpS, "TextOptionPrompt", "TextOptionPrompt", rs, locale);
                        htpDBToXmlFieldDateTime(tmpS, "AvailableStartDate", "AvailableStartDate", rs);
                        htpDBToXmlFieldDateTime(tmpS, "AvailableStopDate", "AvailableStopDate", rs);
                        tmpS.Append("<StoreMappings AutoCleanup=\"true\">");
                        using (SqlConnection StoreMappingConn = DB.dbConn())
                        {
                            StoreMappingConn.Open();
                            using (IDataReader StoreMappingRS = DB.GetRS("select StoreID from ProductStore where ProductID = " + ProductID, StoreMappingConn))
                            {
                                while (StoreMappingRS.Read())
                                {
                                    int sid = DB.RSFieldInt(StoreMappingRS, "StoreID");
                                    tmpS.Append(String.Format("<Store StoreId=\"{0}\" />", sid));
                                }
                            }
                        }
                        tmpS.Append("</StoreMappings>");
                        tmpS.Append("<Mappings AutoCleanup=\"true\">");
                        foreach (String EntityType in AppLogic.ro_SupportedEntities)
                        {
                            String sqlx = "select ~^.*,^.Name, ^.^GUID from ~^  with (NOLOCK)  join ^  with (NOLOCK)  on ~^.^ID=^.^ID where ~ID=" + ProductID.ToString() + " order by DisplayOrder";
                            sqlx = sqlx.Replace("^", EntityType).Replace("~", EntityDefinitions.LookupSpecs(EntityType).m_ObjectName);

                            using (SqlConnection con10 = DB.dbConn())
                            {
                                con10.Open();
                                using (IDataReader rs10 = DB.GetRS(sqlx, con10))
                                {
                                    while (rs10.Read())
                                    {
                                        int eid = DB.RSFieldInt(rs10, EntityType + "ID");
                                        String EntityXPath = Breadcrumb.GetEntityXPath(eid, EntityType, Localization.GetDefaultLocale());
                                        tmpS.Append("<Entity EntityType=\"" + EntityType + "\" Name=\"" + XmlCommon.XmlEncodeAttribute(DB.RSFieldByLocale(rs10, "Name", Localization.GetDefaultLocale())) + "\" XPath=\"" + XmlCommon.XmlEncodeAttribute(EntityXPath) + "\" ID=\"" + eid.ToString() + "\" GUID=\"" + DB.RSFieldGUID(rs10, EntityType + "GUID") + "\" DisplayOrder=\"" + DB.RSFieldInt(rs10, "DisplayOrder").ToString() + "\"/>");
                                    }
                                }
                            }
                        }
                        tmpS.Append("</Mappings>");
                        htpDBToXmlFieldString(tmpS, "ExtensionData", "ExtensionData", rs);
                        htpDBToXmlFieldString(tmpS, "ExtensionData2", "ExtensionData2", rs);
                        htpDBToXmlFieldString(tmpS, "ExtensionData3", "ExtensionData3", rs);
                        htpDBToXmlFieldString(tmpS, "ExtensionData4", "ExtensionData4", rs);
                        htpDBToXmlFieldString(tmpS, "ExtensionData5", "ExtensionData5", rs);
                        if (DB.RSFieldBool(rs, "IsAKit"))
                        {
                            tmpS.Append("<Kit AutoCleanup=\"true\">");


                            using (SqlConnection con11 = DB.dbConn())
                            {
                                con11.Open();
                                using (IDataReader rs11 = DB.GetRS("select * from KitGroup  with (NOLOCK)  where ProductID=" + ProductID.ToString() + " order by DisplayOrder,Name", con11))
                                {
                                    while (rs11.Read())
                                    {
                                        hlpGetKitGroup(tmpS, DB.RSFieldInt(rs11, "KitGroupID"), rs11);
                                    }
                                }
                            }
                            tmpS.Append("</Kit>");
                        }
                        if (IncludeVariants)
                        {
                            tmpS.Append("<Variants AutoCleanup=\"true\">");


                            using (SqlConnection con12 = DB.dbConn())
                            {
                                con12.Open();
                                using (IDataReader rs12 = DB.GetRS("select pv.*, p.TrackInventoryBySizeAndColor from ProductVariant pv  with (NOLOCK) join Product p with (NOLOCK) on pv.ProductID = p.ProductID  where pv.Deleted<>1 and pv.ProductID=" + ProductID.ToString() + " order by pv.DisplayOrder,pv.Name", con12))
                                {
                                    while (rs12.Read())
                                    {
                                        hlpGetVariant(tmpS, DB.RSFieldInt(rs12, "VariantID"), rs12, IncludeImages, DB.RSFieldBool(rs12, "TrackInventoryBySizeAndColor"), locale);
                                    }
                                }
                            }
                            tmpS.Append("</Variants>");
                        }
                        tmpS.Append("</Product>");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return tmpS.ToString();
        }

        private void ProcessGetEntity(XmlNode node)
        {
            String NodeType = node.Name;
            String EntityType = XmlCommon.XmlAttribute(node, "EntityType");
            EntitySpecs Specs = EntityDefinitions.LookupSpecs(EntityType);
            int EntityID = XmlCommon.XmlAttributeNativeInt(node, "EntityID");
            String EntityGUID = XmlCommon.XmlAttribute(node, "EntityGUID");
            bool IncludeImages = XmlCommon.XmlAttributeBool(node, "IncludeImages");
            bool Recursive = XmlCommon.XmlAttributeBool(node, "Recursive");
            String XPathLookup = XmlCommon.XmlAttribute(node, "XPathLookup");
            bool getAllLocales = XmlCommon.XmlAttributeBool(node, "GetAllLocale");

            string preferredLocale = Localization.GetDefaultLocale();
            if (getAllLocales)
            {
                preferredLocale = "AllLocales";
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", EntityType=" + EntityType + ", EntityID = " + EntityID.ToString() + ", EntityGUID=" + EntityGUID + ", XPathLookup=" + XPathLookup + ", Recursive=" + Recursive.ToString());

            if (Specs == null)
            {
                Results.WriteErrorEntry("Required EntityType not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required EntityType not provided or Invalid");
                }
                return;
            }

            EntityHelper h = AppLogic.LookupHelper(EntityType, 0);
            if (h == null)
            {
                Results.WriteErrorEntry("Entity Helper not found!");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Entity Helper not found!");
                }
                return;
            }

            if (XPathLookup.Length != 0)
            {
                if (XPathLookup.StartsWith("/"))
                {
                    XPathLookup = XPathLookup.Substring(1, XPathLookup.Length - 1);
                }
            }

            if (XPathLookup.Length != 0)
            {
                EntityID = this.LookupEntityByXPath(EntityType, XPathLookup);
            }
            else
            {
                EntityID = SetupEntityNodeID(EntityID, EntityGUID, NodeType, EntityType, String.Empty, NodeActionEnum.Unknown);
            }

            if (EntityID != 0 && EntityGUID.Length == 0)
            {
                SqlConnection coni = null;
                IDataReader rsi = null;
                try
                {
                    string query = "select ^GUID from ^ where ^ID=".Replace("^", EntityType) + EntityID.ToString();
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rsi = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        coni = new SqlConnection(DB.GetDBConn());
                        coni.Open();
                        rsi = DB.GetRS(query, coni);
                    }

                    using (rsi)
                    {
                        if (rsi.Read())
                        {
                            EntityGUID = DB.RSFieldGUID(rsi, Specs.m_EntityName + "GUID");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (coni != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        coni.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rsi = null;
                    coni = null;
                }

            }

            try
            {
                Results.WriteXml("<GetEntity EntityID=\"" + EntityID.ToString() + "\" EntityGUID=\"" + EntityGUID + "\" " + CommonLogic.IIF(XPathLookup.Length != 0, "XPathLookup=\"" + XmlCommon.XmlEncodeAttribute(XPathLookup) + "\"", "") + ">");
                Results.WriteXml(hlpGetEntity(EntityType, h, EntityID, Recursive, IncludeImages, preferredLocale));
                Results.WriteXml("</GetEntity>");
                Results.WriteVerboseEntry(NodeType + " OK");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessGetCustomer(XmlNode node)
        {
            String NodeType = node.Name;
            int CustomerID = XmlCommon.XmlAttributeNativeInt(node, "ID");
            String CustomerGUID = XmlCommon.XmlAttribute(node, "GUID");
            String EMail = XmlCommon.XmlAttribute(node, "EMail");
            bool GetAll = XmlCommon.XmlAttributeBool(node, "GetAll");

            Results.WriteVerboseEntry("Processing " + NodeType + ", ID=" + CustomerID.ToString() + ", GUID=" + CustomerGUID + ", EMail=" + EMail + ", GetAll=" + GetAll.ToString());

            CustomerID = SetupTableID("Customer", CustomerID, CustomerGUID);
            if (CustomerID == 0 && EMail.Length != 0)
            {
                SqlConnection con = null;
                IDataReader rs = null;
                try
                {
                    string query = "select CustomerID,CustomerGUID from Customer  with (NOLOCK)  where lower(EMail)=" + DB.SQuote(EMail.ToLowerInvariant());
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        if (rs.Read())
                        {
                            CustomerID = DB.RSFieldInt(rs, "CustomerID");
                            CustomerGUID = DB.RSFieldGUID(rs, "CustomerGUID");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs = null;
                    con = null;
                }

            }
            if (!GetAll && CustomerID == 0)
            {
                Results.WriteErrorEntry("Required CustomerID, CustomerGUID or EMail not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required CustomerID, CustomerGUID or EMail not provided or Invalid");
                }
                return;
            }

            try
            {
                // retreive the specified Customer(s):
                String XmlDocS = "<root><Get Table=\"Customer\"><XmlPackage>DumpCustomer.xml.config</XmlPackage><DefaultWhereClause>" + CommonLogic.IIF(CustomerID == 0 && GetAll, "CustomerID>0", "CustomerID=" + CustomerID.ToString()) + "</DefaultWhereClause></Get></root>";
                XmlDocument d = new XmlDocument();
                d.LoadXml(XmlDocS);
                ProcessGet(d.SelectSingleNode("root/Get"), CustomerID);
                Results.WriteVerboseEntry(NodeType + " OK");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessGetOrder(XmlNode node)
        {
            String NodeType = node.Name;
            int OrderNumber = XmlCommon.XmlAttributeNativeInt(node, "OrderNumber");
            bool GetAll = XmlCommon.XmlAttributeBool(node, "GetAll");

            Results.WriteVerboseEntry("Processing " + NodeType + ", OrderNumber=" + OrderNumber.ToString() + ", GetAll=" + GetAll.ToString());

            if (!GetAll && OrderNumber == 0)
            {
                Results.WriteErrorEntry("Required OrderNumber not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required OrderNumber not provided or Invalid");
                }
                return;
            }

            try
            {
                // retreive the specified Order(s):
                String XmlDocS = "<root><Get Table=\"Orders\"><XmlPackage>DumpOrder.xml.config</XmlPackage><DefaultWhereClause>" + CommonLogic.IIF(OrderNumber == 0 && GetAll, "OrderNumber>0", "OrderNumber=" + OrderNumber.ToString()) + "</DefaultWhereClause></Get></root>";
                XmlDocument d = new XmlDocument();
                d.LoadXml(XmlDocS);
                ProcessGet(d.SelectSingleNode("root/Get"), OrderNumber);
                Results.WriteVerboseEntry(NodeType + " OK");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessGetProduct(XmlNode node)
        {
            String NodeType = node.Name;
            int ProductID = XmlCommon.XmlAttributeNativeInt(node, "ID");
            String ProductGUID = XmlCommon.XmlAttribute(node, "GUID");
            String ForEntityType = XmlCommon.XmlAttribute(node, "ForEntityType");
            EntitySpecs Specs = EntityDefinitions.LookupSpecs(ForEntityType);
            int ForEntityID = XmlCommon.XmlAttributeNativeInt(node, "ForEntityID");
            String ForEntityGUID = XmlCommon.XmlAttribute(node, "ForEntityGUID");
            bool IncludeVariants = XmlCommon.XmlAttributeBool(node, "IncludeVariants");
            bool IncludeImages = XmlCommon.XmlAttributeBool(node, "IncludeImages");
            bool GetAll = XmlCommon.XmlAttributeBool(node, "GetAll");
            bool getAllLocales = XmlCommon.XmlAttributeBool(node, "GetAllLocale");

            string preferredLocale = Localization.GetDefaultLocale();
            if (getAllLocales)
            {
                preferredLocale = "AllLocales";
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", ProductID=" + ProductID.ToString() + ", ProductGUID=" + ProductGUID + ", ForEntityType=" + ForEntityType + ", ForEntityID = " + ForEntityID.ToString() + ", ForEntityGUID=" + ForEntityGUID + ", IncludeVariants=" + IncludeVariants.ToString() + ", GetAll=" + GetAll.ToString());

            ProductID = SetupTableID("Product", ProductID, ProductGUID);
            if (ForEntityType.Length != 0)
            {
                ForEntityID = SetupEntityNodeID(ForEntityID, ForEntityGUID, NodeType, ForEntityType, String.Empty, NodeActionEnum.Unknown);
            }
            if ((ProductID == 0 && ForEntityID == 0) && !GetAll)
            {
                Results.WriteErrorEntry("Required ProductID, ProductGUID or EntityID or EntityGUID not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required ProductID, ProductGUID or EntityID or EntityGUID not provided or Invalid");
                }
                return;
            }

            try
            {
                if (GetAll)
                {
                    // retreive all products in the database:
                    String sql = "select productID from Product order by ProductID";
                    Results.WriteVerboseEntry("Executing SQL: " + sql);
                    Results.WriteXml("<GetProduct GetAll=\"true\">");

                    SqlConnection con = null;
                    IDataReader rs = null;
                    try
                    {
                        string query = sql;
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            rs = DB.GetRS(query, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            con = new SqlConnection(DB.GetDBConn());
                            con.Open();
                            rs = DB.GetRS(query, con);
                        }

                        using (rs)
                        {
                            while (rs.Read())
                            {
                                Results.WriteXml(hlpGetProduct(DB.RSFieldInt(rs, "ProductID"), IncludeVariants, IncludeImages, preferredLocale));
                            }
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (con != null && m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            con.Dispose();
                        }

                        // make sure we won't reference this again in code
                        rs = null;
                        con = null;
                    }

                    Results.WriteXml("</GetProduct>");
                    Results.WriteVerboseEntry(NodeType + " OK");
                }
                else if (ProductID == 0)
                {
                    // retreive all products for the Entity specified:
                    String sql = "select ~.~ID,DisplayOrder from ~^  with (NOLOCK)  join ~  with (NOLOCK)  on ~^.~ID=~.~ID where ^ID=" + ForEntityID.ToString() + " order by DisplayOrder";
                    sql = sql.Replace("^", Specs.m_EntityName).Replace("~", Specs.m_ObjectName);
                    Results.WriteVerboseEntry("Executing SQL: " + sql);
                    Results.WriteXml("<GetProduct ForEntityID=\"" + ForEntityID.ToString() + "\" ForEntityGUID=\"" + ForEntityGUID + "\">");

                    SqlConnection con = null;
                    IDataReader rs = null;
                    try
                    {
                        string query = sql;
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            rs = DB.GetRS(query, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            con = new SqlConnection(DB.GetDBConn());
                            con.Open();
                            rs = DB.GetRS(query, con);
                        }

                        using (rs)
                        {
                            while (rs.Read())
                            {
                                Results.WriteXml(hlpGetProduct(DB.RSFieldInt(rs, "ProductID"), IncludeVariants, IncludeImages, preferredLocale));
                            }
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (con != null && m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            con.Dispose();
                        }

                        // make sure we won't reference this again in code
                        rs = null;
                        con = null;
                    }

                    Results.WriteXml("</GetProduct>");
                    Results.WriteVerboseEntry(NodeType + " OK");
                }
                else
                {
                    // retreive the specified product:
                    Results.WriteXml("<GetProduct ID=\"" + ProductID.ToString() + "\" GUID=\"" + ProductGUID + "\">");
                    Results.WriteXml(hlpGetProduct(ProductID, IncludeVariants, IncludeImages, preferredLocale));
                    Results.WriteXml("</GetProduct>");
                    Results.WriteVerboseEntry(NodeType + " OK");
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessClearMappings(XmlNode node)
        {
            String NodeType = node.Name;
            String EntityType = XmlCommon.XmlAttribute(node, "EntityType");
            EntitySpecs Specs = EntityDefinitions.LookupSpecs(EntityType);
            String EntityGUID = XmlCommon.XmlAttribute(node, "EntityGUID");
            int EntityID = XmlCommon.XmlAttributeNativeInt(node, "EntityID");

            Results.WriteVerboseEntry("Processing " + NodeType + ", EntityType=" + EntityType + ", EntityID=" + EntityID.ToString() + ", EntityGUID=" + EntityGUID);

            EntityID = SetupEntityNodeID(EntityID, EntityGUID, NodeType, EntityType, String.Empty, NodeActionEnum.Get);
            if (EntityID == 0)
            {
                Results.WriteErrorEntry("Required EntityID or EntityGUID not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required EntityID or EntityGUID not provided or Invalid");
                }
                return;
            }

            try
            {
                String sql = "delete from ~^ where ^ID=" + EntityID.ToString();
                sql = sql.Replace("^", EntityType).Replace("~", "Product");
                RunCommand(sql);
                Results.WriteVerboseEntry(NodeType + " OK");

            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessGetEntityHelper(XmlNode node)
        {
            String NodeType = node.Name;
            String EntityType = XmlCommon.XmlAttribute(node, "EntityType");
            EntitySpecs Specs = EntityDefinitions.LookupSpecs(EntityType);

            Results.WriteVerboseEntry("Processing " + NodeType + ", EntityType=" + EntityType);

            try
            {
                EntityHelper hlp = AppLogic.LookupHelper(EntityType, 0);
                if (hlp == null)
                {
                    Results.WriteErrorEntry("Requested EntityHelper Is Invalid");
                    if (m_DBTrans != null)
                    {
                        throw new ArgumentException("Requested EntityHelper Is Invalid");
                    }
                    return;
                }
                Results.WriteGetEntityHelperStartElement(EntityType);
                Results.WriteXml(hlp.m_TblMgr.FinalXml);
                Results.WriteEndElement();
                Results.WriteVerboseEntry(NodeType + " OK");

            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private String AddXmlFieldToSQLWhereClauseString(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator, bool CDATAAllowed)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            String fldVal = XmlCommon.XmlField(node, XmlElementName);
            if (fldVal.Length == 0)
            {
                if (CDATAAllowed && n.NodeType == XmlNodeType.CDATA)
                {
                    fldVal = n.Value;
                }
            }
            if (fldVal.Length != 0)
            {
                sql.Append(Separator);
                sql.Append(DBFieldName);
                sql.Append("=");
                sql.Append(DB.SQuote(fldVal));
                Separator = ",";
            }
            return Separator;
        }

        private String AddXmlFieldToSQLWhereClauseBool(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            bool fldVal = XmlCommon.XmlFieldBool(node, XmlElementName);
            if (XmlCommon.XmlField(node, XmlElementName).Length != 0)
            {
                sql.Append(Separator);
                sql.Append(DBFieldName);
                sql.Append("=");
                sql.Append(CommonLogic.IIF(fldVal, "1", "0"));
                Separator = ",";
            }
            return Separator;
        }

        private String AddXmlFieldToSQLWhereClauseInt(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            int fldVal = XmlCommon.XmlFieldNativeInt(node, XmlElementName);
            if (XmlCommon.XmlField(node, XmlElementName).Length != 0)
            {
                sql.Append(Separator);
                sql.Append(DBFieldName);
                sql.Append("=");
                sql.Append(Localization.IntStringForDB(fldVal));
                Separator = ",";
            }
            return Separator;
        }

        private String AddXmlFieldToSQLWhereClauseDecimal(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            Decimal fldVal = XmlCommon.XmlFieldNativeDecimal(node, XmlElementName);
            if (XmlCommon.XmlField(node, XmlElementName).Length != 0)
            {
                sql.Append(Separator);
                sql.Append(DBFieldName);
                sql.Append("=");
                sql.Append(Localization.DecimalStringForDB(fldVal));
                Separator = ",";
            }
            return Separator;
        }

        private String AddXmlFieldToSQLWhereClauseDateTime(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            DateTime fldVal = XmlCommon.XmlFieldNativeDateTime(node, XmlElementName);
            if (XmlCommon.XmlField(node, XmlElementName).Length != 0)
            {
                sql.Append(Separator);
                sql.Append(DBFieldName);
                sql.Append("=");
                sql.Append(DB.SQuote(Localization.DateTimeStringForDB(fldVal)));
                Separator = ",";
            }
            return Separator;
        }

        private int hlpEvalBoolForDB(String tmpS)
        {
            if ("TRUE".Equals(tmpS) ||
                "YES".Equals(tmpS) ||
                "1".Equals(tmpS))
            {
                return 1;
            }
            return 0;
        }

        private String hlpProcessGetAttributes(String TableName, XmlNode node, StringBuilder sql, String Separator)
        {
            if (node == null)
            {
                return Separator;
            }
            foreach (XmlAttribute a in node.Attributes)
            {
                MappingTypeEnum ft = LookupTableFieldType(TableName, a.Name);
                switch (ft)
                {
                    case MappingTypeEnum.BoolField:
                        if (a.InnerText.Length != 0)
                        {
                            sql.Append(Separator);
                            sql.Append(a.Name + "=" + hlpEvalBoolForDB(a.InnerText));
                            Separator = " and ";
                        }
                        break;
                    case MappingTypeEnum.IntegerField:
                        if (a.InnerText.Length != 0)
                        {
                            sql.Append(Separator);
                            sql.Append(a.Name + "=" + a.InnerText);
                            Separator = " and ";
                        }
                        break;
                    case MappingTypeEnum.DateTimeField:
                        if (a.InnerText.Length != 0)
                        {
                            if (a.InnerText.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sql.Append(Separator);
                                sql.Append(a.Name + " is NULL");
                                Separator = " and ";
                            }
                            else
                            {
                                sql.Append(Separator);
                                sql.Append(a.Name + "=" + DB.SQuote(a.InnerText));
                                Separator = " and ";
                            }
                        }
                        break;
                    case MappingTypeEnum.DecimalField:
                        if (a.InnerText.Length != 0)
                        {
                            sql.Append(Separator);
                            sql.Append(a.Name + "=" + a.InnerText);
                            Separator = " and ";
                        }
                        break;
                    case MappingTypeEnum.GUIDField:
                        if (a.InnerText.Length != 0)
                        {
                            if (a.InnerText.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sql.Append(Separator);
                                sql.Append(a.Name + " is NULL");
                                Separator = " and ";
                            }
                            else
                            {
                                sql.Append(Separator);
                                sql.Append(a.Name + "=" + DB.SQuote(a.InnerText));
                                Separator = " and ";
                            }
                        }
                        break;
                    case MappingTypeEnum.StringField:
                        if (a.InnerText.Length != 0)
                        {
                            if (a.InnerText.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sql.Append(Separator);
                                sql.Append(a.Name + " is NULL");
                                Separator = " and ";
                            }
                            else
                            {
                                sql.Append(Separator);
                                sql.Append(a.Name + "=" + DB.SQuote(a.InnerText));
                                Separator = " and ";
                            }
                        }
                        break;
                }
            }
            return Separator;
        }

        private String hlpProcessSetElements(String TableName, XmlNode node, StringBuilder sql, String Separator)
        {
            if (node == null)
            {
                return Separator;
            }
            foreach (XmlNode n in node.ChildNodes)
            {
                MappingTypeEnum ft = LookupTableFieldType(TableName, n.Name);
                switch (ft)
                {
                    case MappingTypeEnum.BoolField:
                        sql.Append(Separator);
                        sql.Append(n.Name + "=" + hlpEvalBoolForDB(n.InnerText));
                        Separator = " , ";
                        break;
                    case MappingTypeEnum.IntegerField:
                        sql.Append(Separator);
                        sql.Append(n.Name + "=" + Localization.ParseNativeInt(n.InnerText).ToString());
                        Separator = " , ";
                        break;
                    case MappingTypeEnum.DateTimeField:
                        if (n.InnerText.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                        {
                            sql.Append(Separator);
                            sql.Append(n.Name + "=NULL");
                            Separator = " , ";
                        }
                        else
                        {
                            sql.Append(Separator);
                            sql.Append(n.Name + "=" + DB.SQuote(Localization.ToDBDateTimeString(Localization.ParseNativeDateTime(n.InnerText))));
                            Separator = " , ";
                        }
                        break;
                    case MappingTypeEnum.DecimalField:
                        sql.Append(Separator);
                        sql.Append(n.Name + "=" + DB.SQuote(Localization.DecimalStringForDB(Localization.ParseNativeDecimal(n.InnerText))));
                        Separator = " , ";
                        break;
                    case MappingTypeEnum.GUIDField:
                        if (n.InnerText.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                        {
                            sql.Append(Separator);
                            sql.Append(n.Name + "=NULL");
                            Separator = " , ";
                        }
                        else
                        {
                            sql.Append(Separator);
                            sql.Append(n.Name + "=" + DB.SQuote(n.InnerText));
                            Separator = " , ";
                        }
                        break;
                    case MappingTypeEnum.StringField:
                        if (n.InnerXml.Length != 0)
                        {
                            sql.Append(Separator);
                            sql.Append(n.Name + "=" + DB.SQuote(n.InnerXml));
                            Separator = " , ";
                        }
                        else
                        {
                            if (n.InnerText.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                            {
                                sql.Append(Separator);
                                sql.Append(n.Name + "=NULL");
                                Separator = " , ";
                            }
                            else
                            {
                                sql.Append(Separator);
                                sql.Append(n.Name + "=" + DB.SQuote(n.InnerText));
                                Separator = " , ";
                            }
                        }
                        break;
                }
            }
            return Separator;
        }

        private void ProcessSet(XmlNode node, int Idx)
        {
            String NodeType = node.Name;
            String SetName = XmlCommon.XmlAttribute(node, "Name");
            String TableName = XmlCommon.XmlAttribute(node, "Table");
            int ID = XmlCommon.XmlAttributeUSInt(node, "ID");
            String GUID = XmlCommon.XmlAttribute(node, "GUID");
            String IDColumn = XmlCommon.XmlField(node, "IDColumn");
            String GUIDColumn = XmlCommon.XmlField(node, "GUIDColumn");

            if (IDColumn.Length == 0)
            {
                IDColumn = LookupTableIDColumn(TableName);
            }

            if (GUIDColumn.Length == 0)
            {
                GUIDColumn = LookupTableGUIDColumn(TableName);
            }

            if (SetName.Length == 0)
            {
                SetName = "Set" + Idx.ToString();
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", Table=" + TableName + ", IDColumn=" + IDColumn + ", GUIDColumn=" + GUIDColumn);

            bool IDChecked = false;
            if (ID == 0 && GUID.Length != 0)
            {
                ID = LookupTableRecordByGUID(TableName, GUID);
                if (ID != 0)
                {
                    IDChecked = true;
                }
            }
            if (ID == 0 && GUID.Length == 0)
            {
                Results.WriteErrorEntry("Required ID or GUID not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required ID or GUID not provided or Invalid");
                }
            }
            if (!IDChecked && !ExistsTableRecordByID(TableName, ID))
            {
                Results.WriteErrorEntry("Specified record ID (" + ID.ToString() + ")  not found!");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Specified record ID (" + ID.ToString() + ") not found!");
                }
            }
            IDChecked = true;

            try
            {
                Results.WriteXml("<Set Table=\"" + TableName + "\" ID=\"" + ID.ToString() + "\" GUID=\"" + GUID + "\" Name=\"" + SetName + "\">");
                if (TableName.Length != 0 && IDColumn.Length != 0)
                {
                    String Separator = String.Empty;
                    StringBuilder sql = new StringBuilder(2500);
                    sql.Append("update ");
                    sql.Append(TableName);
                    sql.Append(" set ");
                    Separator = hlpProcessSetElements(TableName, node, sql, Separator);
                    sql.Append(" where ");
                    sql.Append(IDColumn);
                    sql.Append("=");
                    sql.Append(ID.ToString());
                    RunCommand(sql.ToString());
                }
                Results.WriteXml("</Set>");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                Results.WriteXml("</Set>");
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessGet(XmlNode node, int Idx)
        {
            String NodeType = node.Name;
            String GetName = XmlCommon.XmlAttribute(node, "Name");
            String TableName = XmlCommon.XmlAttribute(node, "Table");
            String IDColumn = XmlCommon.XmlField(node, "IDColumn");
            String XmlPackage = XmlCommon.XmlField(node, "XmlPackage");
            String DefaultWhereClause = XmlCommon.XmlField(node, "DefaultWhereClause");
            String OrderBy = XmlCommon.XmlField(node, "OrderBy");
            XmlNode Criteria = node.SelectSingleNode("Criteria");

            if (IDColumn.Length == 0)
            {
                IDColumn = LookupTableIDColumn(TableName);
            }

            if (GetName.Length == 0)
            {
                GetName = "Get" + Idx.ToString();
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", Table=" + TableName + ", IDColumn=" + IDColumn + ", XmlPackage=" + XmlPackage + ", DefaultWhereClause=" + DefaultWhereClause + ", OrderBy=" + OrderBy);

            try
            {
                Results.WriteXml("<Get Table=\"" + XmlCommon.XmlEncodeAttribute(TableName) + "\" Name=\"" + XmlCommon.XmlEncodeAttribute(GetName) + "\" XmlPackage=\"" + XmlCommon.XmlEncodeAttribute(XmlPackage) + "\" IDColumn=\"" + XmlCommon.XmlEncodeAttribute(IDColumn) + "\" DefaultWhereClause=\"" + XmlCommon.XmlEncodeAttribute(DefaultWhereClause) + "\" OrderBy=\"" + XmlCommon.XmlEncodeAttribute(OrderBy) + "\">");
                hlpWriteNodeEcho(Criteria);
                if (DefaultWhereClause.Length == 0)
                {
                    DefaultWhereClause = "1=1";
                }

                if (TableName.Length != 0 && IDColumn.Length != 0)
                {
                    if (XmlPackage.Length != 0)
                    {
                        String Separator = String.Empty;
                        StringBuilder sql = new StringBuilder(2500);
                        sql.Append("select ");
                        sql.Append(IDColumn);
                        sql.Append(" from  ");
                        sql.Append(TableName);
                        sql.Append(" with (NOLOCK) where ");
                        Separator = hlpProcessGetAttributes(TableName, Criteria, sql, Separator);
                        if (Separator.Length == 0 && DefaultWhereClause.Length != 0)
                        {
                            sql.Append(" ");
                            sql.Append(DefaultWhereClause);
                            sql.Append(" ");
                        }
                        if (OrderBy.Length != 0)
                        {
                            sql.Append(" order by ");
                            sql.Append(OrderBy);
                        }

                        Results.WriteVerboseEntry("SQL=" + sql.ToString());
                        String PN = CommonLogic.SafeMapPath(AppLogic.AppConfig("AdminDir") + "/XmlPackages/" + XmlPackage);

                        SqlConnection con = null;
                        IDataReader rs = null;
                        try
                        {
                            string query = sql.ToString();
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(query, m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(query, con);
                            }

                            using (rs)
                            {
                                while (rs.Read())
                                {
                                    int ThisID = DB.RSFieldInt(rs, IDColumn);
                                    XmlNode xpn = node.SelectSingleNode("XmlPackage");
                                    String xpnRuntimeParams = String.Empty;
                                    if (xpn != null)
                                    {
                                        xpnRuntimeParams = XmlCommon.XmlAttribute(xpn, "RuntimeParams");
                                    }
                                    if (xpnRuntimeParams.Length != 0)
                                    {
                                        xpnRuntimeParams = IDColumn + "=" + ThisID.ToString() + "&" + xpnRuntimeParams;
                                    }
                                    else
                                    {
                                        xpnRuntimeParams = IDColumn + "=" + ThisID.ToString();
                                    }
                                    String OutputXml = AppLogic.RunXmlPackage(PN, null, m_ThisCustomer, 1, "", xpnRuntimeParams, false, true);
                                    OutputXml = OutputXml.Replace("utf-16", "utf-8");
                                    Results.WriteXml(OutputXml);
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (con != null && m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }

                            // make sure we won't reference this again in code
                            rs = null;
                            con = null;
                        }

                    }
                    else
                    {
                        String Separator = String.Empty;
                        StringBuilder sql = new StringBuilder(2500);
                        sql.Append("select * from ");
                        sql.Append(TableName);
                        sql.Append(" with (NOLOCK) where ");
                        Separator = hlpProcessGetAttributes(TableName, Criteria, sql, Separator);
                        if (Separator.Length == 0 && DefaultWhereClause.Length != 0)
                        {
                            sql.Append(" ");
                            sql.Append(DefaultWhereClause);
                            sql.Append(" ");
                        }
                        if (OrderBy.Length != 0)
                        {
                            sql.Append(" order by ");
                            sql.Append(OrderBy);
                        }

                        Results.WriteVerboseEntry("SQL=" + sql.ToString());

                        StringBuilder OutputXml = new StringBuilder(4096);

                        SqlConnection con = null;
                        IDataReader rs = null;
                        try
                        {
                            string query = sql.ToString();
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(query, m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(query, con);
                            }

                            using (rs)
                            {
                                DB.GetXml(rs, String.Empty, TableName, false, ref OutputXml);
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (con != null && m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }

                            // make sure we won't reference this again in code
                            rs = null;
                            con = null;
                        }

                        Results.WriteXml(OutputXml.ToString());

                    }
                }
                Results.WriteXml("</" + NodeType + ">");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessQuery(XmlNode node, int Idx)
        {
            String NodeType = node.Name;
            String QueryName = XmlCommon.XmlAttribute(node, "Name");
            String RowName = XmlCommon.XmlAttribute(node, "RowName");
            String sql = XmlCommon.XmlField(node, "SQL");
            if (sql.Length == 0)
            {
                sql = XmlCommon.XmlField(node, "sql");
            }
            if (QueryName.Length == 0)
            {
                QueryName = "Query" + Idx.ToString();
            }
            if (RowName.Length == 0)
            {
                RowName = "row";
            }
            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + QueryName + ", RowName=" + RowName);

            try
            {
                Results.WriteXml("<Query Name=\"" + XmlCommon.XmlEncodeAsIs(QueryName) + "\">");
                if (sql.Length != 0)
                {
                    String s = sql.ToLowerInvariant();
                    Results.WriteVerboseEntry("SQL=" + s);

                    StringBuilder OutputXml = new StringBuilder(4096);

                    SqlConnection con = null;
                    IDataReader rs = null;
                    try
                    {
                        string query = s;
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            rs = DB.GetRS(query, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            con = new SqlConnection(DB.GetDBConn());
                            con.Open();
                            rs = DB.GetRS(query, con);
                        }

                        using (rs)
                        {
                            DB.GetXml(rs, String.Empty, RowName, false, ref OutputXml);
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (con != null && m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            con.Dispose();
                        }

                        // make sure we won't reference this again in code
                        rs = null;
                        con = null;
                    }

                    Results.WriteXml(OutputXml.ToString());
                }
                Results.WriteXml("</Query>");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                Results.WriteXml("</Query>");
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessExecuteSQL(XmlNode node, int Idx)
        {
            String NodeType = node.Name;
            String CmdName = XmlCommon.XmlAttribute(node, "Name");
            String sql = XmlCommon.XmlField(node, "SQL");
            if (sql.Length == 0)
            {
                sql = XmlCommon.XmlField(node, "sql");
            }
            if (CmdName.Length == 0)
            {
                CmdName = "ExecuteSQL" + Idx.ToString();
            }
            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + CmdName);

            try
            {
                Results.WriteXml("<ExecuteSQL Name=\"" + XmlCommon.XmlEncodeAsIs(CmdName) + "\">");
                if (sql.Length != 0)
                {
                    Results.WriteVerboseEntry("SQL=" + sql);
                    DB.ExecuteSQL(sql, m_DBTrans);

                }
                Results.WriteXml("</ExecuteSQL>");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }


        private void ProcessXmlPackage(XmlNode node, int Idx)
        {
            String NodeType = node.Name;
            String PackageName = XmlCommon.XmlAttribute(node, "Name");
            String RuntimeParams = XmlCommon.XmlAttribute(node, "RuntimeParams");
            String OutputType = XmlCommon.XmlAttribute(node, "OutputType").ToUpperInvariant();
            if (OutputType.Length == 0)
            {
                OutputType = "INLINE";
            }
            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + PackageName);

            try
            {
                Results.WriteXml("<XmlPackage Name=\"" + XmlCommon.XmlEncodeAsIs(PackageName) + "\">");
                if (PackageName.Length != 0)
                {
                    String s = AppLogic.RunXmlPackage(PackageName, null, null, 1, "", RuntimeParams, false, false); ;
                    if (OutputType == "INLINE")
                    {
                        Results.WriteXml(s);
                    }
                    else
                    {
                        Results.WriteXml("<![CDATA[");
                        Results.WriteXml(s);
                        Results.WriteXml("]]>");
                    }
                }
                Results.WriteXml("</XmlPackage>");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessScanForXmlPackages(XmlNode node)
        {
            String NodeType = node.Name;
            int SkinID = XmlCommon.XmlAttributeUSInt(node, "SkinID");
            if (SkinID == 0)
            {
                SkinID = AppLogic.GetStoreSkinID(AppLogic.StoreID());
            }
            String PackageTypePrefix = XmlCommon.XmlAttribute(node, "PackageTypePrefix");
            Results.WriteVerboseEntry("Processing " + NodeType + ", SkinID=" + SkinID.ToString() + ", PackageTypePrefix=" + PackageTypePrefix);

            try
            {
                ArrayList xmlPackages;
                Results.WriteXml(String.Format("<ScanForXmlPackages SkinID=\"{0}\" PackageTypePrefix=\"{1}\">", SkinID.ToString(), PackageTypePrefix));

                String[] PackageTypes = { "Entity", "Product", "Notification", "Feed", "Page", "Skin" };
                foreach (String s in PackageTypes)
                {
                    if (PackageTypePrefix.Length == 0 ||
                        PackageTypePrefix.Equals(s, StringComparison.InvariantCultureIgnoreCase))
                    {
                        xmlPackages = AppLogic.ReadXmlPackages(s.ToLowerInvariant(), SkinID);
                        foreach (String xp in xmlPackages)
                        {
                            Results.WriteXml("<" + s + " Name=\"" + XmlCommon.XmlEncodeAttribute(xp) + "\"/>");
                        }
                    }
                }
                Results.WriteXml("</ScanForXmlPackages>");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessResetCache(XmlNode node)
        {
            String NodeType = node.Name;
            bool Confirm = XmlCommon.XmlAttributeBool(node, "Confirm");
            Results.WriteVerboseEntry("Processing " + NodeType + ", Confirm=" + Confirm.ToString());

            try
            {
                if (Confirm)
                {
                    // make sure all products have a variant. this is a bit of a hack doing it here, but was required by tech support:
                    RunCommand("exec aspdnsf_CreateMissingVariants");
                    AppLogic.m_RestartApp();
                    Results.WriteXml("<ResetCache/>");
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessUndoLastImport(XmlNode node)
        {
            String NodeType = node.Name;
            bool Confirm = XmlCommon.XmlAttributeBool(node, "Confirm");
            Results.WriteVerboseEntry("Processing " + NodeType + ", Confirm=" + Confirm.ToString());

            try
            {
                if (Confirm)
                {
                    RunCommand("aspdnsf_UndoImport");
                    Results.WriteXml("<UndoLastImport/>");
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessSetOrder(XmlNode node)
        {
            String NodeType = node.Name;
            int OrderNumber = XmlCommon.XmlAttributeNativeInt(node, "OrderNumber");
            String OrderGUID = XmlCommon.XmlAttribute(node, "OrderGUID");

            Results.WriteVerboseEntry("Processing " + NodeType + ", OrderNumber=" + OrderNumber.ToString() + ", OrderGUID=" + OrderGUID);

            OrderNumber = SetupTableID("Orders", OrderNumber, OrderGUID);
            if (OrderNumber == 0)
            {
                Results.WriteErrorEntry("Required OrderNumber or OrderGUID not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required OrderNumber or OrderGUID not provided or Invalid");
                }
                return;
            }

            try
            {

            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }


        private String AddXmlFieldToSQLFieldString(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator, bool CDATAAllowed, String DefaultValueToUseIfEmptyNode)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            String fldVal = String.Empty;
            if (n.InnerXml.Length != 0 && !n.InnerXml.StartsWith("<![CDATA["))
            {
                fldVal = n.InnerXml;
            }
            else
            {
                fldVal = XmlCommon.XmlField(node, XmlElementName);
            }
            if (fldVal.Length == 0)
            {
                if (CDATAAllowed && n.NodeType == XmlNodeType.CDATA)
                {
                    fldVal = n.Value;
                }
            }
            if (fldVal.Length == 0)
            {
                fldVal = DefaultValueToUseIfEmptyNode;
            }
            sql.Append(Separator);
            sql.Append(DBFieldName);
            sql.Append("=");
            sql.Append(DB.SQuote(fldVal));
            Separator = ",";
            return Separator;
        }

        private String AddXmlFieldToSQLFieldBool(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator, bool DefaultValueToUseIfEmptyNode)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            bool fldVal = XmlCommon.XmlFieldBool(node, XmlElementName);
            if (XmlCommon.XmlField(node, XmlElementName).Length == 0)
            {
                fldVal = DefaultValueToUseIfEmptyNode;
            }
            sql.Append(Separator);
            sql.Append(DBFieldName);
            sql.Append("=");
            sql.Append(CommonLogic.IIF(fldVal, "1", "0"));
            Separator = ",";
            return Separator;
        }

        private String AddXmlFieldToSQLFieldInt(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator, int DefaultValueToUseIfEmptyNode)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            int fldVal = XmlCommon.XmlFieldNativeInt(node, XmlElementName);
            if (XmlCommon.XmlField(node, XmlElementName).Length == 0)
            {
                fldVal = DefaultValueToUseIfEmptyNode;
            }
            sql.Append(Separator);
            sql.Append(DBFieldName);
            sql.Append("=");
            sql.Append(Localization.IntStringForDB(fldVal));
            Separator = ",";
            return Separator;
        }

        private String AddXmlFieldToSQLFieldDecimal(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator, Decimal DefaultValueToUseIfEmptyNode)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            Decimal fldVal = XmlCommon.XmlFieldNativeDecimal(node, XmlElementName);
            if (XmlCommon.XmlField(node, XmlElementName).Length == 0)
            {
                fldVal = DefaultValueToUseIfEmptyNode;
            }
            sql.Append(Separator);
            sql.Append(DBFieldName);
            sql.Append("=");
            sql.Append(Localization.DecimalStringForDB(fldVal));
            Separator = ",";
            return Separator;
        }

        private String AddXmlFieldToSQLFieldDateTime(String XmlElementName, String DBFieldName, XmlNode node, StringBuilder sql, String Separator, DateTime DefaultValueToUseIfEmptyNode)
        {
            XmlNode n = node.SelectSingleNode(XmlElementName);
            if (n == null)
            {
                return Separator;
            }
            DateTime fldVal = XmlCommon.XmlFieldNativeDateTime(node, XmlElementName);
            if (XmlCommon.XmlField(node, XmlElementName).Length == 0)
            {
                fldVal = DefaultValueToUseIfEmptyNode;
            }
            sql.Append(Separator);
            sql.Append(DBFieldName);
            sql.Append("=");
            sql.Append(DB.SQuote(Localization.DateTimeStringForDB(fldVal)));
            Separator = ",";
            return Separator;
        }

        private void hlpEntityUpdate(EntitySpecs Specs, int EntityID, XmlNode node)
        {
            String Separator = String.Empty;
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("update ^ set ");
            Separator = AddXmlFieldToSQLFieldString("Name", "Name", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Summary", "Summary", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Description", "Description", node, sql, Separator, true, String.Empty);
            if (XmlCommon.XmlField(node, "SE/SEName").Length != 0)
            {
                sql.Append(Separator);
                sql.Append("SEName=");
                sql.Append(DB.SQuote(CommonLogic.Left(XmlCommon.XmlField(node, "SE/SEName"), 150)));
                Separator = ",";
            }
            else if (XmlCommon.XmlField(node, "Name").Length != 0)
            {
                sql.Append(Separator);
                sql.Append("SEName=");
                sql.Append(DB.SQuote(CommonLogic.Left(SE.MungeName(XmlCommon.XmlField(node, "Name")), 150)));
                Separator = ",";
            }
            Separator = AddXmlFieldToSQLFieldString("SE/SEKeywords", "SEKeywords", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SEDescription", "SEDescription", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SETitle", "SETitle", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SENoScript", "SENoScript", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SEAltText", "SEAltText", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldBool("Published", "Published", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldBool("Wholesale", "Wholesale", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldString("Display/XmlPackage", "XmlPackage", node, sql, Separator, false, String.Empty);
            Separator = AddXmlFieldToSQLFieldInt("Display/PageSize", "PageSize", node, sql, Separator, 32);
            Separator = AddXmlFieldToSQLFieldInt("Display/ColWidth", "ColWidth", node, sql, Separator, 4);

            XmlNode storeMappings = node.SelectSingleNode("StoreMappings");
            if (storeMappings != null && storeMappings.InnerXml.Length != 0)
            {
                bool AutoCleanupMappings = XmlCommon.XmlAttributeBool(storeMappings, "AutoCleanup");

                bool PreserveExistingRecords = false;
                Dictionary<string, string> PreserveColl = new Dictionary<string, string>();

                if (XmlCommon.NodeContainsAttribute(storeMappings, "PreserveExistingRecords"))
                {
                    PreserveExistingRecords = XmlCommon.XmlAttributeBool(storeMappings, "PreserveExistingRecords");
                }

                if (AutoCleanupMappings && !PreserveExistingRecords)
                {
                    RunCommand(String.Format("delete from entitystore where entityid = {0} and entitytype = {1}", EntityID.ToString(), DB.SQuote(Specs.m_EntityName.ToLower())));
                }
                foreach (XmlNode n in storeMappings.SelectNodes("Store"))
                {
                    int StoreId = XmlCommon.XmlAttributeNativeInt(n, "StoreId");
                    if (StoreId == 0)
                        StoreId = XmlCommon.XmlAttributeNativeInt(n, "StoreID");
                    if (StoreId == 0)
                    {
                        String StoreName = XmlCommon.XmlAttribute(n, "StoreName");
                        StoreId = GetStoreIdByName(StoreName);
                    }

                    ProcessEntityStoreMapping(NodeActionEnum.Add, StoreId, Specs.m_EntityName, EntityID);
                }
            }

            XmlNode ptn = node.SelectSingleNode("QuantityDiscount");
            if (ptn != null)
            {
                int QuantityDiscountID = LoadSimpleObject("QuantityDiscount", XmlCommon.XmlAttributeNativeInt(ptn, "ID"), XmlCommon.XmlAttribute(ptn, "GUID"), XmlCommon.XmlAttribute(ptn, "Name"), false);
                sql.Append(Separator);
                sql.Append("QuantityDiscountID=");
                sql.Append(QuantityDiscountID.ToString());
                Separator = ",";
            }

            Separator = AddXmlFieldToSQLFieldInt("DisplayOrder", "DisplayOrder", node, sql, Separator, 1);

            if (Specs.m_HasAddress)
            {
                Separator = AddXmlFieldToSQLFieldString("Address1", "Address1", node, sql, Separator, true, String.Empty);
                Separator = AddXmlFieldToSQLFieldString("Address2", "Address2", node, sql, Separator, true, String.Empty);
                Separator = AddXmlFieldToSQLFieldString("Suite", "Suite", node, sql, Separator, true, String.Empty);
                Separator = AddXmlFieldToSQLFieldString("City", "City", node, sql, Separator, true, String.Empty);
                Separator = AddXmlFieldToSQLFieldString("State", "State", node, sql, Separator, true, "--");
                Separator = AddXmlFieldToSQLFieldString("ZipCode", "ZipCode", node, sql, Separator, true, String.Empty);
                Separator = AddXmlFieldToSQLFieldString("Country", "Country", node, sql, Separator, true, String.Empty);
                Separator = AddXmlFieldToSQLFieldString("Phone", "Phone", node, sql, Separator, true, String.Empty);
                Separator = AddXmlFieldToSQLFieldString("FAX", "FAX", node, sql, Separator, true, String.Empty);
                if (XmlCommon.XmlField(node, "URL").Length != 0)
                {
                    String theUrl = XmlCommon.XmlField(node, "URL");
                    if (theUrl.IndexOf("http://") == -1 && theUrl.Length != 0)
                    {
                        theUrl = "http://" + theUrl;
                    }
                    sql.Append(Separator);
                    sql.Append("URL=");
                    sql.Append(DB.SQuote(theUrl));
                    Separator = ",";
                }
                Separator = AddXmlFieldToSQLFieldString("EMail", "EMail", node, sql, Separator, false, String.Empty);
            }
            Separator = AddXmlFieldToSQLFieldString("Images/ImageFilenameOverride", "ImageFilenameOverride", node, sql, Separator, false, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData", "ExtensionData", node, sql, Separator, true, String.Empty);
            sql.Append(" where ^Id=" + EntityID.ToString());
            sql = sql.Replace("^", Specs.m_EntityName);
            // was there anything to update:
            if (Separator != String.Empty)
            {
                RunCommand(sql.ToString());
            }

            XmlNode nnxImages = node.SelectSingleNode("Images");
            // process images, if any:
            foreach (String imagesize in "Icon,Medium,Large".Split(','))
            {
                String img_Params = String.Empty;
                Boolean img_UseAppConfigs = false;
                Boolean img_Resize = false;

                XmlNode nnx = node.SelectSingleNode("Images/" + imagesize);
                if (nnx != null)
                {
                    String ImageType = XmlCommon.XmlAttribute(nnx, "ImageType");
                    if (ImageType.Length == 0)
                    {
                        ImageType = XmlCommon.XmlAttribute(nnx, "Extension");
                    }
                    String ImageData = XmlCommon.XmlField(node, "Images/" + imagesize);
                    bool DeleteFlag = XmlCommon.XmlAttributeBool(nnx, "Delete");
                    String FN = EntityID.ToString();
                    if (XmlCommon.XmlField(node, "Images/ImageFilenameOverride").Trim().Length != 0)
                    {
                        FN = XmlCommon.XmlField(node, "Images/ImageFilenameOverride").Trim();
                    }
                    // delete any current image file first
                    if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + FN);
                        }
                        catch
                        { }
                    }
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + Path.GetFileNameWithoutExtension(FN) + ss);
                        }
                        catch
                        { }
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + EntityID.ToString() + ss);
                        }
                        catch
                        { }
                    }

                    if (ImageData.Length != 0 && !DeleteFlag)
                    {
                        String img_ContentType = String.Empty;
                        String TempImage = String.Empty;

                        //need to keep the original filename incase we are creating other sizes when uploading the large.
                        String eoID = FN;

                        byte[] imagedatabytes = Convert.FromBase64String(ImageData);
                        if (!FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        {
                            switch (ImageType.Replace(".", "").ToLowerInvariant())
                            {
                                case "gif":
                                    TempImage = AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + "tmp_" + FN + ".gif";
                                    FN = AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + FN + ".gif";
                                    img_ContentType = "image/gif";
                                    break;
                                case "png":
                                    TempImage = AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + "tmp_" + FN + ".png";
                                    FN = AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + FN + ".png";
                                    img_ContentType = "image/png";
                                    break;
                                case "jpg":
                                case "jpeg":
                                case "pjpeg":
                                    TempImage = AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + "tmp_" + FN + ".jpg";
                                    FN = AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + FN + ".jpg";
                                    img_ContentType = "image/jpeg";
                                    break;
                            }
                        }
                        else
                        {
                            TempImage = AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + "tmp_" + FN;
                            FN = AppLogic.GetImagePath(Specs.m_EntityName, imagesize, true) + FN;
                            img_ContentType = "image/jpeg";
                        }
                        try
                        {
                            img_UseAppConfigs = XmlCommon.XmlAttributeBool(nnxImages, "UseAppConfigs");

                            if (img_UseAppConfigs)
                            {
                                img_Resize = AppLogic.AppConfigBool("UseImageResize");
                            }
                            if (XmlCommon.XmlAttribute(nnx, "Resize").Length > 0)
                            {
                                img_Resize = XmlCommon.XmlAttributeBool(nnxImages, "Resize");
                            }


                            if (img_Resize)
                            {
                                img_Params = XmlCommon.XmlAttribute(nnx, "Params");

                                File.WriteAllBytes(TempImage, imagedatabytes);

                                AppLogic.ResizeEntityOrObject(Specs.m_EntityName, TempImage, FN, eoID, imagesize, img_ContentType, img_Params, img_UseAppConfigs);

                            }
                            else
                            {
                                File.WriteAllBytes(FN, imagedatabytes);
                            }
                        }
                        catch
                        {
                            Results.WriteErrorEntry("Could not write file: " + FN);
                        }
                        finally
                        {
                            AppLogic.DisposeOfTempImage(TempImage);
                        }
                    }
                }

            }
        }

        // NOTE: THIS DOES NOT UPDATE PRIMARY BILLING OR SHIPPING INFO!! 
        // That has to be handled AFTER this call is done, and any subsequent Address nodes were also processed!
        private void hlpCustomerUpdate(int CustomerID, XmlNode node)
        {
            hlpCustomerUpdate(CustomerID, node, true);
        }
        private void hlpCustomerUpdate(int CustomerID, XmlNode node, bool AddingCustomer)
        {
            String Separator = String.Empty;
            StringBuilder sql = new StringBuilder(2500);

            Customer c = new Customer(m_DBTrans, CustomerID, true);

            String EMailField = c.EMail;
            if (XmlCommon.XmlField(node, "EMail").Length != 0)
            {
                EMailField = XmlCommon.XmlField(node, "EMail").Trim().ToLowerInvariant();
            }
            bool NewEmailAllowed = Customer.NewEmailPassesDuplicationRules(EMailField, c.CustomerID, false);

            if (!NewEmailAllowed)
            {
                Results.WriteErrorEntry("EMail Is Already Used! " + EMailField);
                throw new ArgumentException("EMail Is Already Used! " + EMailField);
            }

            sql.Append("update Customer set ");
            Separator = AddXmlFieldToSQLFieldInt("CustomerLevelID", "CustomerLevelID", node, sql, Separator, 0);

            // use lowercase, for consistency:
            if (EMailField.Length != 0)
            {
                sql.Append(Separator);
                sql.Append("EMail=");
                sql.Append(DB.SQuote(EMailField));
                Separator = ",";
            }

            Separator = AddXmlFieldToSQLFieldString("FirstName", "FirstName", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("LastName", "LastName", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Phone", "Phone", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("FAX", "FAX", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("CouponCode", "CouponCode", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldBool("OkToEMail", "OkToEMail", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldInt("IsAdmin", "IsAdmin", node, sql, Separator, 0);
            Separator = AddXmlFieldToSQLFieldString("LocaleSetting", "LocaleSetting", node, sql, Separator, true, Localization.GetDefaultLocale());
            Separator = AddXmlFieldToSQLFieldString("CurrencySetting", "CurrencySetting", node, sql, Separator, true, Localization.GetPrimaryCurrency());
            Separator = AddXmlFieldToSQLFieldDecimal("MicroPayBalance", "MicroPayBalance", node, sql, Separator, 0.0M);
            Separator = AddXmlFieldToSQLFieldInt("RecurringShippingMethodID", "RecurringShippingMethodID", node, sql, Separator, 0);
            Separator = AddXmlFieldToSQLFieldString("GiftRegistryGUID", "GiftRegistryGUID", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldBool("GiftRegistryIsAnonymous", "GiftRegistryIsAnonymous", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("GiftRegistryAllowSearchByOthers", "GiftRegistryAllowSearchByOthers", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldBool("GiftRegistryHideShippingAddresses", "GiftRegistryHideShippingAddresses", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldBool("CODCompanyCheckAllowed", "CODCompanyCheckAllowed", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("CODNet30Allowed", "CODNet30Allowed", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("Over13Checked", "Over13Checked", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("StoreCCInDB", "StoreCCInDB", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldInt("StoreID", "StoreID", node, sql, Separator, 1);

            String PWD = XmlCommon.XmlField(node, "Password").Trim();
            if (PWD.Length != 0)
            {
                Password p = new Password(PWD);
                String newpwd = p.SaltedPassword;

                sql.Append(Separator);
                sql.Append("SaltKey=");
                sql.Append(p.Salt.ToString());
                Separator = ",";

                sql.Append(Separator);
                sql.Append("Password=");
                sql.Append(DB.SQuote(newpwd));
                Separator = ",";
            }

            if (AddingCustomer)
            {
                if (XmlCommon.XmlField(node, "EMail").Trim().Length > 0 && XmlCommon.XmlField(node, "FirstName").Trim().Length > 0 && XmlCommon.XmlField(node, "LastName").Trim().Length > 0 && XmlCommon.XmlField(node, "Phone").Trim().Length > 0 && PWD.Length > 0)
                {
                    Separator = AddXmlFieldToSQLFieldBool("IsRegistered", "IsRegistered", node, sql, Separator, false);
                }
                else
                {
                    sql.Append(Separator);
                    sql.Append("IsRegistered=0");
                    Separator = ",";
                }
            }
            else
            {
                if (XmlCommon.XmlField(node, "IsRegistered").Trim().Length > 0)
                {
                    sql.Append(Separator);
                    sql.Append("IsRegistered=" + XmlCommon.XmlFieldUSInt(node, "IsRegistered"));
                    Separator = ",";
                }
            }

            Separator = AddXmlFieldToSQLFieldDateTime("LockedUntil", "LockedUntil", node, sql, Separator, System.DateTime.Now.AddMinutes(-1));
            Separator = AddXmlFieldToSQLFieldBool("AdminCanViewCC", "AdminCanViewCC", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldInt("BadLoginCount", "BadLoginCount", node, sql, Separator, 0);
            Separator = AddXmlFieldToSQLFieldBool("Active", "Active", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldBool("PwdChangeRequired", "PwdChangeRequired", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldInt("VATSetting", "VATSetting", node, sql, Separator, 0);
            Separator = AddXmlFieldToSQLFieldString("VATRegistrationID", "VATRegistrationID", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Notes", "Notes", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData", "ExtensionData", node, sql, Separator, true, String.Empty);
            sql.Append(" where CustomerID=" + CustomerID.ToString());
            // was there anything to update:
            if (Separator != String.Empty)
            {
                RunCommand(sql.ToString());
            }
        }

        private void hlpAddressUpdate(int AddressID, XmlNode node)
        {
            String Separator = String.Empty;
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("update Address set ");
            Separator = AddXmlFieldToSQLFieldString("NickName", "NickName", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("FirstName", "FirstName", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("LastName", "LastName", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Company", "Company", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Address1", "Address1", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Address2", "Address2", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("City", "City", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("State", "State", node, sql, Separator, true, "--");
            Separator = AddXmlFieldToSQLFieldString("Zip", "Zip", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Country", "Country", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldInt("ResidenceType", "ResidenceType", node, sql, Separator, 0);
            Separator = AddXmlFieldToSQLFieldString("Phone", "Phone", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("EMail", "EMail", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData", "ExtensionData", node, sql, Separator, true, String.Empty);
            sql.Append(" where AddressID=" + AddressID.ToString());
            // was there anything to update:
            if (Separator != String.Empty)
            {
                RunCommand(sql.ToString());

                //SAFETY CHECK - Our Address tables do not operate off ISO Codes!

                String IsoCode = node.SelectSingleNode("Country").InnerText.ToString();
                String ctry = ParseCountryNameFromIso(IsoCode);

                if (ctry != IsoCode)
                {
                    DB.ExecuteSQL("UPDATE Address SET Country = " + DB.SQuote(ctry) + " WHERE AddressID = " + DB.SQuote(AddressID.ToString()), m_DBTrans);
                }

            }
        }

        public String ParseCountryNameFromIso(String s)
        {
            String country = String.Empty;
            if (s.Length == 2)
            {
                SqlConnection con = null;
                IDataReader rs = null;
                try
                {
                    string query = "select Name from country where TwoLetterISOCode=" + DB.SQuote(s);
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        if (rs.Read())
                        {
                            country = DB.RSField(rs, "Name");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs = null;
                    con = null;
                }
            }
            else if (s.Length == 3)
            {
                SqlConnection con = null;
                IDataReader rs = null;
                try
                {
                    string query = "select Name from country where ThreeLetterISOCode=" + DB.SQuote(s);
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        if (rs.Read())
                        {
                            country = DB.RSField(rs, "Name");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs = null;
                    con = null;
                }

            }
            if (country.Length == 0)
            {
                country = s;
            }
            return country;
        }

        private void hlpProductUpdate(int ProductID, XmlNode node)
        {
            String Separator = String.Empty;
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("update Product set ");
            Separator = AddXmlFieldToSQLFieldString("Name", "Name", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Summary", "Summary", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Description", "Description", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SpecTitle", "SpecTitle", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("MiscText", "MiscText", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Notes", "Notes", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("IsFeaturedTeaser", "IsFeaturedTeaser", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("FroogleDescription", "FroogleDescription", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SKU", "SKU", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ManufacturerPartNumber", "ManufacturerPartNumber", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SwatchImageMap", "SwatchImageMap", node, sql, Separator, true, String.Empty);
            if (XmlCommon.XmlField(node, "SE/SEName").Length != 0)
            {
                sql.Append(Separator);
                sql.Append("SEName=");
                sql.Append(DB.SQuote(CommonLogic.Left(XmlCommon.XmlField(node, "SE/SEName"), 150)));
                Separator = ",";
            }
            else if (XmlCommon.XmlField(node, "Name").Length != 0)
            {
                sql.Append(Separator);
                sql.Append("SEName=");
                sql.Append(DB.SQuote(CommonLogic.Left(SE.MungeName(XmlCommon.XmlField(node, "Name")), 150)));
                Separator = ",";
            }
            Separator = AddXmlFieldToSQLFieldString("SE/SEKeywords", "SEKeywords", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SEDescription", "SEDescription", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SETitle", "SETitle", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SENoScript", "SENoScript", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SEAltText", "SEAltText", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SizeOptionPrompt", "SizeOptionPrompt", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ColorOptionPrompt", "ColorOptionPrompt", node, sql, Separator, true, String.Empty);

            XmlNode ptn = node.SelectSingleNode("ProductType");
            if (ptn != null)
            {
                int ProductTypeID = LoadSimpleObject("ProductType", XmlCommon.XmlAttributeNativeInt(ptn, "ID"), XmlCommon.XmlAttribute(ptn, "GUID"), XmlCommon.XmlAttribute(ptn, "Name"), false);
                sql.Append(Separator);
                sql.Append("ProductTypeID=");
                sql.Append(ProductTypeID.ToString());
                Separator = ",";
            }

            ptn = node.SelectSingleNode("TaxClass");
            if (ptn != null)
            {
                int TaxClassID = LoadSimpleObject("TaxClass", XmlCommon.XmlAttributeNativeInt(ptn, "ID"), XmlCommon.XmlAttribute(ptn, "GUID"), XmlCommon.XmlAttribute(ptn, "Name"), false);
                sql.Append(Separator);
                sql.Append("TaxClassID=");
                sql.Append(TaxClassID.ToString());
                Separator = ",";
            }

            ptn = node.SelectSingleNode("SalesPrompt");
            if (ptn != null)
            {
                int SalesPromptID = LoadSimpleObject("SalesPrompt", XmlCommon.XmlAttributeNativeInt(ptn, "ID"), XmlCommon.XmlAttribute(ptn, "GUID"), XmlCommon.XmlAttribute(ptn, "Name"), false);
                sql.Append(Separator);
                sql.Append("SalesPromptID=");
                sql.Append(SalesPromptID.ToString());
                Separator = ",";
            }

            Separator = AddXmlFieldToSQLFieldString("SpecCall", "SpecCall", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldBool("SpecsInline", "SpecsInline", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldString("Display/XmlPackage", "XmlPackage", node, sql, Separator, false, String.Empty);
            Separator = AddXmlFieldToSQLFieldInt("Display/PageSize", "PageSize", node, sql, Separator, 32);
            Separator = AddXmlFieldToSQLFieldInt("Display/ColWidth", "ColWidth", node, sql, Separator, 4);

            ptn = node.SelectSingleNode("QuantityDiscount");
            if (ptn != null)
            {
                int QuantityDiscountID = LoadSimpleObject("QuantityDiscount", XmlCommon.XmlAttributeNativeInt(ptn, "ID"), XmlCommon.XmlAttribute(ptn, "GUID"), XmlCommon.XmlAttribute(ptn, "Name"), false);
                sql.Append(Separator);
                sql.Append("QuantityDiscountID=");
                sql.Append(QuantityDiscountID.ToString());
                Separator = ",";
            }

            Separator = AddXmlFieldToSQLFieldBool("InventoryType/TrackInventoryBySizeAndColor", "TrackInventoryBySizeAndColor", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("InventoryType/TrackInventoryBySizeAndColor", "TrackInventoryBySize", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("InventoryType/TrackInventoryBySizeAndColor", "TrackInventoryByColor", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldString("WarehouseLocation", "WarehouseLocation", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldBool("IsFeatured", "IsFeatured", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("IsAKit", "IsAKit", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("IsSystem", "IsSystem", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("ShowBuyButton", "ShowBuyButton", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldBool("Published", "Published", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldBool("Wholesale", "Wholesale", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("RequiresRegistration", "RequiresRegistration", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("HidePriceUntilCart", "HidePriceUntilCart", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("IsCallToOrder", "IsCallToOrder", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("ExcludeFromPriceFeeds", "ExcludeFromPriceFeeds", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("RequiresTextOption", "RequiresTextOption", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldInt("TextOptionMaxLength", "TextOptionMaxLength", node, sql, Separator, 32);
            Separator = AddXmlFieldToSQLFieldString("TextOptionPrompt", "TextOptionPrompt", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldDateTime("AvailableStartDate", "AvailableStartDate", node, sql, Separator, System.DateTime.Now);
            Separator = AddXmlFieldToSQLFieldDateTime("AvailableStopDate", "AvailableStopDate", node, sql, Separator, System.DateTime.Now.AddYears(100));
            XmlNode nn;
            nn = node.SelectSingleNode("RelatedProducts");
            if (nn != null)
            {
                String sx = String.Empty;
                StringBuilder RelatedProducts = new StringBuilder(512);
                if (nn.InnerXml.Length != 0)
                {
                    foreach (XmlNode n in nn.SelectNodes("CX"))
                    {
                        int PID = SetupTableID("Product", XmlCommon.XmlAttributeNativeInt(n, "ID"), XmlCommon.XmlAttribute(n, "GUID"));
                        if (PID != 0)
                        {
                            RelatedProducts.Append(sx);
                            RelatedProducts.Append(PID.ToString());
                            sx = ",";
                        }
                    }
                }
                sql.Append(Separator);
                sql.Append("RelatedProducts=");
                sql.Append(DB.SQuote(RelatedProducts.ToString()));
                Separator = ",";
            }
            nn = node.SelectSingleNode("UpsellProducts");
            if (nn != null)
            {
                String sx = String.Empty;
                StringBuilder UpsellProducts = new StringBuilder(512);
                if (nn.InnerXml.Length != 0)
                {
                    foreach (XmlNode n in nn.SelectNodes("CX"))
                    {
                        int PID = SetupTableID("Product", XmlCommon.XmlAttributeNativeInt(n, "ID"), XmlCommon.XmlAttribute(n, "GUID"));
                        if (PID != 0)
                        {
                            UpsellProducts.Append(sx);
                            UpsellProducts.Append(PID.ToString());
                            sx = ",";
                        }
                    }
                }
                sql.Append(Separator);
                sql.Append("UpsellProducts=");
                sql.Append(DB.SQuote(UpsellProducts.ToString()));
                Separator = ",";
            }
            nn = node.SelectSingleNode("RequiresProducts");
            if (nn != null && nn.InnerXml.Length != 0)
            {
                String sx = String.Empty;
                StringBuilder RequiresProducts = new StringBuilder(512);
                if (nn.InnerXml.Length != 0)
                {
                    foreach (XmlNode n in nn.SelectNodes("CX"))
                    {
                        int PID = SetupTableID("Product", XmlCommon.XmlAttributeNativeInt(n, "ID"), XmlCommon.XmlAttribute(n, "GUID"));
                        if (PID != 0)
                        {
                            RequiresProducts.Append(sx);
                            RequiresProducts.Append(PID.ToString());
                            sx = ",";
                        }
                    }
                }
                sql.Append(Separator);
                sql.Append("RequiresProducts=");
                sql.Append(DB.SQuote(RequiresProducts.ToString()));
                Separator = ",";
            }
            Separator = AddXmlFieldToSQLFieldString("ExtensionData", "ExtensionData", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData2", "ExtensionData2", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData3", "ExtensionData3", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData4", "ExtensionData4", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData5", "ExtensionData5", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Images/ImageFilenameOverride", "ImageFilenameOverride", node, sql, Separator, true, String.Empty);
            sql.Append(" where ProductId=" + ProductID.ToString());
            // was there anything to update:
            if (Separator != String.Empty)
            {
                RunCommand(sql.ToString());
            }

            nn = node.SelectSingleNode("StoreMappings");
            if (nn != null && nn.InnerXml.Length != 0)
            {
                bool AutoCleanupMappings = XmlCommon.XmlAttributeBool(nn, "AutoCleanup");

                bool PreserveExistingRecords = false;
                Dictionary<string, string> PreserveColl = new Dictionary<string, string>();

                if (XmlCommon.NodeContainsAttribute(nn, "PreserveExistingRecords"))
                {
                    PreserveExistingRecords = XmlCommon.XmlAttributeBool(nn, "PreserveExistingRecords");
                }

                if (AutoCleanupMappings && !PreserveExistingRecords)
                {
                    RunCommand(String.Format("delete from productstore where productid = {0}", ProductID.ToString()));
                }
                foreach (XmlNode n in nn.SelectNodes("Store"))
                {
                    int StoreId = XmlCommon.XmlAttributeNativeInt(n, "StoreId");
                    if (StoreId == 0)
                        StoreId = XmlCommon.XmlAttributeNativeInt(n, "StoreID");
                    if (StoreId == 0)
                    {
                        String StoreName = XmlCommon.XmlAttribute(n, "StoreName");
                        StoreId = GetStoreIdByName(StoreName);
                    }

                    ProcessProductStoreMapping(NodeActionEnum.Add, StoreId, ProductID);
                }
            }

            // process mappings for the product:
            nn = node.SelectSingleNode("Mappings");
            if (nn != null && nn.InnerXml.Length != 0)
            {

                bool AutoCleanupMappings = XmlCommon.XmlAttributeBool(nn, "AutoCleanup");

                bool PreserveExistingRecords = false;
                Dictionary<string, string> PreserveColl = new Dictionary<string, string>();

                if (XmlCommon.NodeContainsAttribute(nn, "PreserveExistingRecords"))
                {
                    PreserveExistingRecords = XmlCommon.XmlAttributeBool(nn, "PreserveExistingRecords");
                }

                if (AutoCleanupMappings && !PreserveExistingRecords)
                {
                    foreach (String eName in AppLogic.ro_SupportedEntities)
                    {
                        EntitySpecs Specs = EntityDefinitions.LookupSpecs(eName);
                        if (Specs.m_ObjectName == "Product")
                        {
                            String s = "delete from ~^ where ~ID=" + ProductID.ToString();
                            s = s.Replace("^", eName).Replace("~", Specs.m_ObjectName);
                            RunCommand(s);
                        }
                    }

                    using (SqlConnection conCL = new SqlConnection(DB.GetDBConn()))
                    {
                        conCL.Open();
                        using (IDataReader rsCL = DB.GetRS("select * from ProductCustomerLevel where ProductID=" + ProductID.ToString(), conCL))
                        {
                            while (rsCL.Read())
                            {
                                String t = "delete from ProductCustomerLevel where ProductID=" + ProductID.ToString();
                                RunCommand(t);
                            }
                        }
                    }
                }
                foreach (XmlNode n in nn.SelectNodes("Entity"))
                {
                    String EntityType = XmlCommon.XmlAttribute(n, "EntityType");
                    String EntityName = XmlCommon.XmlAttribute(n, "Name");
                    String EntityXPath = XmlCommon.XmlAttribute(n, "XPath");
                    int EntityID = XmlCommon.XmlAttributeNativeInt(n, "ID");
                    String EntityGUID = XmlCommon.XmlAttribute(n, "GUID");
                    int DisplayOrder = XmlCommon.XmlAttributeNativeInt(n, "DisplayOrder");

                    if (EntityID == 0 && EntityGUID.Length != 0)
                    {
                        EntityID = LookupEntityByGUID(EntityType, EntityGUID);
                    }
                    if (EntityID == 0 && EntityName.Length != 0)
                    {
                        EntityID = LookupEntityByName(EntityType, EntityName, 0);
                    }
                    if (EntityID == 0 && EntityXPath.Length != 0)
                    {
                        EntityID = this.LoadEntityTree(EntityType, EntityXPath, 0, EntityGUID, EntityName);
                    }
                    if (EntityID != 0)
                    {

                        String mapsql = String.Empty;
                        EntitySpecs Specs = EntityDefinitions.LookupSpecs(EntityType);
                        if (Specs.m_ObjectName == "Product")
                        {
                            if (PreserveExistingRecords)
                            {
                                if (PreserveColl.ContainsKey(EntityType))
                                {
                                    PreserveColl[EntityType] += "," + EntityID.ToString();
                                }
                                else
                                {
                                    PreserveColl.Add(EntityType, EntityID.ToString());
                                }
                            }
                            if (Specs.m_EntityObjectMappingIs1to1)
                            {
                                mapsql = "delete from ~^ where ^ID=" + EntityID.ToString() + " and ~ID=" + ProductID.ToString();
                                mapsql = mapsql.Replace("^", EntityType).Replace("~", Specs.m_ObjectName);
                                RunCommand(mapsql);
                            }
                            // ignore dups on the insert
                            try
                            {
                                mapsql = "insert ~^(^ID,~ID) values(" + EntityID.ToString() + "," + ProductID.ToString() + ")";
                                mapsql = mapsql.Replace("^", EntityType).Replace("~", Specs.m_ObjectName);
                                RunCommand(mapsql);
                            }
                            catch { }
                            try
                            {
                                if (XmlCommon.NodeContainsAttribute(n, "DisplayOrder"))
                                {
                                    mapsql = "update ~^ set DisplayOrder=" + DisplayOrder.ToString() + " where ^ID=" + EntityID.ToString() + " and ~ID=" + ProductID.ToString();
                                    mapsql = mapsql.Replace("^", EntityType).Replace("~", Specs.m_ObjectName);
                                    RunCommand(mapsql);
                                }
                            }
                            catch { }
                        }
                    }
                }

                //loop through each <CustomerLevel Name="" ID="" GUID="" /> node in <Mappings>
                foreach (XmlNode cl in nn.SelectNodes("CustomerLevel"))
                {
                    String CustomerLevelName = XmlCommon.XmlAttribute(cl, "Name");
                    int CustomerLevelID = XmlCommon.XmlAttributeNativeInt(cl, "ID");
                    String CustomerLevelGUID = XmlCommon.XmlAttribute(cl, "GUID");

                    if (CustomerLevelID == 0 && CustomerLevelGUID.Length != 0)
                    {
                        CustomerLevelID = LookupCustomerLevelByGUID(CustomerLevelGUID);
                    }
                    if (CustomerLevelID == 0 && CustomerLevelName.Length != 0)
                    {
                        CustomerLevelID = LookupCustomerLevelByName(CustomerLevelName);
                    }
                    if (CustomerLevelID != 0)
                    {
                        String mapsql = String.Empty;

                        if (PreserveExistingRecords)
                        {
                            if (PreserveColl.ContainsKey(CustomerLevelName))
                            {
                                PreserveColl[CustomerLevelName] += "," + CustomerLevelID.ToString();
                            }
                            else
                            {
                                PreserveColl.Add(CustomerLevelName, CustomerLevelID.ToString());
                            }
                        }

                        // ignore dups on the insert
                        try
                        {
                            mapsql = "insert ~^(^ID,~ID) values(" + CustomerLevelID.ToString() + "," + ProductID.ToString() + ")";
                            mapsql = mapsql.Replace("^", "CustomerLevel").Replace("~", "Product");
                            RunCommand(mapsql);
                        }
                        catch { }

                    }
                }


                //delete the mappings not needed anymore 
                if (AutoCleanupMappings && PreserveExistingRecords)
                {
                    String mapsql = String.Empty;
                    foreach (String eName in AppLogic.ro_SupportedEntities)
                    {
                        EntitySpecs Specs = EntityDefinitions.LookupSpecs(eName);
                        if (Specs.m_ObjectName == "Product")
                        {
                            if (PreserveColl.ContainsKey(eName))
                            {
                                //only delete the ones not found in the list in the call 
                                mapsql = "delete from ~^ where ^ID not in (" + PreserveColl[eName] + ") and ~ID=" + ProductID.ToString();
                                mapsql = mapsql.Replace("^", eName).Replace("~", Specs.m_ObjectName);
                                RunCommand(mapsql);
                            }
                            else
                            {
                                //no mappings were included in the call for this entity, so purge them all 
                                mapsql = "delete from ~^ where ~ID=" + ProductID.ToString();
                                mapsql = mapsql.Replace("^", eName).Replace("~", Specs.m_ObjectName);
                                RunCommand(mapsql);
                            }
                        }
                    }
                }

            }
            XmlNode nnxImages = node.SelectSingleNode("Images");
            String img_Params = String.Empty;
            Boolean img_UseAppConfigs = false;
            Boolean img_Resize = false;
            // process images, if any:
            foreach (String imagesize in "Icon,Medium,Large".Split(','))
            {
                XmlNode nnx = node.SelectSingleNode("Images/" + imagesize);
                if (nnx != null)
                {
                    String ImageType = XmlCommon.XmlAttribute(nnx, "ImageType");
                    if (ImageType.Length == 0)
                    {
                        ImageType = XmlCommon.XmlAttribute(nnx, "Extension");
                    }
                    String ImageData = XmlCommon.XmlField(node, "Images/" + imagesize);
                    bool DeleteFlag = XmlCommon.XmlAttributeBool(nnx, "Delete");
                    String FN = ProductID.ToString();
                    if (AppLogic.AppConfigBool("UseSKUForProductImageName"))
                    {
                        FN = XmlCommon.XmlField(node, "SKU").Trim();
                    }
                    if (XmlCommon.XmlField(node, "Images/ImageFilenameOverride").Trim().Length != 0)
                    {
                        FN = XmlCommon.XmlField(node, "Images/ImageFilenameOverride").Trim();
                    }
                    // delete any current image file first
                    if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath("Product", imagesize, true) + FN);
                        }
                        catch
                        { }
                    }
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath("Product", imagesize, true) + Path.GetFileNameWithoutExtension(FN) + ss);
                        }
                        catch
                        { }
                        try
                        {
                            System.IO.File.Delete(AppLogic.GetImagePath("Product", imagesize, true) + ProductID.ToString() + ss);
                        }
                        catch
                        { }
                    }

                    if (ImageData.Length != 0 && !DeleteFlag)
                    {
                        String img_ContentType = String.Empty;
                        String TempImage = String.Empty;

                        //need to keep the original filename incase we are creating other sizes when uploading the large.
                        String eoID = FN;

                        byte[] imagedatabytes = Convert.FromBase64String(ImageData);
                        if (!FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        {
                            switch (ImageType.Replace(".", "").ToLowerInvariant())
                            {
                                case "gif":
                                    TempImage = AppLogic.GetImagePath("Product", imagesize, true) + "tmp_" + FN + ".gif";
                                    FN = AppLogic.GetImagePath("Product", imagesize, true) + FN + ".gif";
                                    img_ContentType = "image/gif";
                                    break;
                                case "png":
                                    TempImage = AppLogic.GetImagePath("Product", imagesize, true) + "tmp_" + FN + ".png";
                                    FN = AppLogic.GetImagePath("Product", imagesize, true) + FN + ".png";
                                    img_ContentType = "image/png";
                                    break;
                                case "jpg":
                                case "jpeg":
                                case "pjpeg":
                                    TempImage = AppLogic.GetImagePath("Product", imagesize, true) + "tmp_" + FN + ".jpg";
                                    FN = AppLogic.GetImagePath("Product", imagesize, true) + FN + ".jpg";
                                    img_ContentType = "image/jpeg";
                                    break;
                            }
                        }
                        else
                        {
                            TempImage = AppLogic.GetImagePath("Product", imagesize, true) + "tmp_" + FN;
                            FN = AppLogic.GetImagePath("Product", imagesize, true) + FN;
                            img_ContentType = "image/jpeg";
                        }
                        try
                        {
                            img_UseAppConfigs = XmlCommon.XmlAttributeBool(nnxImages, "UseAppConfigs");

                            if (img_UseAppConfigs)
                            {
                                img_Resize = AppLogic.AppConfigBool("UseImageResize");
                            }
                            if (XmlCommon.XmlAttribute(nnx, "Resize").Length > 0)
                            {
                                img_Resize = XmlCommon.XmlAttributeBool(nnxImages, "Resize");
                            }


                            if (img_Resize)
                            {
                                img_Params = XmlCommon.XmlAttribute(nnx, "Params");

                                File.WriteAllBytes(TempImage, imagedatabytes);

                                AppLogic.ResizeEntityOrObject("Product", TempImage, FN, eoID, imagesize, img_ContentType, img_Params, img_UseAppConfigs);

                            }
                            else
                            {
                                File.WriteAllBytes(FN, imagedatabytes);
                            }
                        }
                        catch
                        {
                            Results.WriteErrorEntry("Could not write file: " + FN);
                        }
                        finally
                        {
                            AppLogic.DisposeOfTempImage(TempImage);
                        }
                    }
                }
            }

            // process multi-images:
            XmlNode mx = node.SelectSingleNode("Images/MultiImage");
            if (mx != null)
            {
                foreach (String imagesize in "Icon,Medium,Large".Split(','))
                {
                    // process images, if any:
                    foreach (XmlNode nnx in mx.SelectNodes(imagesize + "/Img"))
                    {
                        String ImageType = XmlCommon.XmlAttribute(nnx, "ImageType");
                        if (ImageType.Length == 0)
                        {
                            ImageType = XmlCommon.XmlAttribute(nnx, "Extension");
                        }
                        String ImageData = nnx.InnerText.Trim();
                        int Index = XmlCommon.XmlAttributeUSInt(nnx, "Index");
                        String Color = XmlCommon.XmlAttribute(nnx, "Color");
                        bool DeleteFlag = XmlCommon.XmlAttributeBool(nnx, "Delete");
                        String SafeColor = CommonLogic.MakeSafeFilesystemName(Color);

                        String FN = ProductID.ToString();
                        if (AppLogic.AppConfigBool("UseSKUForProductImageName"))
                        {
                            FN = XmlCommon.XmlField(node, "SKU").Trim();
                        }
                        if (XmlCommon.XmlField(nnx, "ImageFilenameOverride").Trim().Length != 0)
                        {
                            FN = XmlCommon.XmlField(node, "Images/ImageFilenameOverride").Trim();
                        }

                        FN = Path.GetFileNameWithoutExtension(FN) + "_" + Index.ToString() + "_" + SafeColor + "." + Path.GetExtension(FN).ToLowerInvariant().Replace(".", "");

                        // delete any current image file first
                        if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                System.IO.File.Delete(AppLogic.GetImagePath("Product", imagesize, true) + FN);
                            }
                            catch { }
                        }
                        foreach (String ss in CommonLogic.SupportedImageTypes)
                        {
                            try
                            {
                                System.IO.File.Delete(AppLogic.GetImagePath("Product", imagesize, true) + Path.GetFileNameWithoutExtension(FN) + ss);
                            }
                            catch { }
                        }

                        if (ImageData.Length != 0 && !DeleteFlag)
                        {
                            String strImage = String.Empty;
                            String TempImage = String.Empty;
                            String ContentType = String.Empty;

                            img_Params = String.Empty;

                            //need to keep the original filename incase we are creating other sizes when uploading the large.
                            String eoID = FN;

                            byte[] imagedatabytes = Convert.FromBase64String(ImageData);
                            if (!FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                            {
                                switch (ImageType.Replace(".", ""))
                                {
                                    case "gif":
                                        TempImage = AppLogic.GetImagePath("Product", imagesize, true) + "tmp_" + FN + "gif";
                                        FN = AppLogic.GetImagePath("Product", imagesize, true) + FN + "gif";
                                        ContentType = "image/gif";
                                        break;
                                    case "image/x-png":
                                    case "image/png":
                                        TempImage = AppLogic.GetImagePath("Product", imagesize, true) + "tmp_" + FN + "png";
                                        FN = AppLogic.GetImagePath("Product", imagesize, true) + FN + "png";
                                        ContentType = "image/png";
                                        break;
                                    case "jpg":
                                    case "jpeg":
                                    case "pjpeg":
                                        TempImage = AppLogic.GetImagePath("Product", imagesize, true) + "tmp_" + FN + "jpg";
                                        FN = AppLogic.GetImagePath("Product", imagesize, true) + FN + "jpg";
                                        ContentType = "image/jpeg";
                                        break;
                                }
                            }
                            else
                            {
                                TempImage = AppLogic.GetImagePath("Product", imagesize, true) + "tmp_" + FN;
                                FN = AppLogic.GetImagePath("Product", imagesize, true) + Path.GetFileNameWithoutExtension(FN);
                            }
                            try
                            {
                                Boolean MultiMakesMicros = false;

                                if (img_UseAppConfigs)
                                {
                                    img_Resize = AppLogic.AppConfigBool("UseImageResize");
                                    MultiMakesMicros = AppLogic.AppConfigBool("MultiMakesMicros");
                                }
                                if (XmlCommon.XmlAttribute(nnxImages, "Resize").Length > 0)
                                {
                                    img_Resize = XmlCommon.XmlAttributeBool(nnxImages, "Resize");
                                }
                                if (XmlCommon.XmlAttribute(nnxImages, "MultiMakesMicros").Length > 0)
                                {
                                    MultiMakesMicros = XmlCommon.XmlAttributeBool(nnxImages, "MultiMakesMicros");
                                }


                                if (img_Resize)
                                {
                                    img_Params = XmlCommon.XmlAttribute(nnx, "Params");

                                    File.WriteAllBytes(TempImage, imagedatabytes);

                                    AppLogic.ResizeEntityOrObject("Product", TempImage, FN, eoID, imagesize, ContentType, img_Params, img_UseAppConfigs);

                                }
                                else
                                {
                                    File.WriteAllBytes(FN, imagedatabytes);
                                }
                            }
                            catch
                            {
                                Results.WriteErrorEntry("Could not write file: " + FN);
                            }
                            finally
                            {
                                AppLogic.DisposeOfTempImage(TempImage);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessInventoryUpdate(XmlNode nn, int MasterProductID, int MasterVariantID)
        {
            // process inventory:
            if (nn != null && nn.InnerXml.Length != 0)
            {
                InventoryMatchKeyEnum MatchKey = InventoryMatchKeyEnum.SizeColor;
                if (XmlCommon.XmlAttribute(nn, "MatchKey").Length != 0)
                {
                    MatchKey = (InventoryMatchKeyEnum)Enum.Parse(typeof(InventoryMatchKeyEnum), XmlCommon.XmlAttribute(nn, "MatchKey"));
                }
                foreach (XmlNode n in nn.SelectNodes("Inv"))
                {
                    String Size = XmlCommon.XmlAttribute(n, "Size");
                    String Color = XmlCommon.XmlAttribute(n, "Color");
                    int Quantity = XmlCommon.XmlAttributeNativeInt(n, "Quantity");
					String GTIN = XmlCommon.XmlAttribute(n, "GTIN");
                    String VendorID = XmlCommon.XmlAttribute(n, "VendorID");
                    String WarehouseLocation = XmlCommon.XmlAttribute(n, "WarehouseLocation");
                    String VendorFullSKU = XmlCommon.XmlAttribute(n, "VendorFullSKU");
                    String VariantSKU = XmlCommon.XmlAttribute(n, "VariantSKU");
                    Decimal WeightDelta = XmlCommon.XmlAttributeNativeDecimal(n, "WeightDelta");
                    String ExtensionData = XmlCommon.XmlAttribute(n, "ExtensionData");
                    String CleanSize = AppLogic.CleanSizeColorOption(Size).ToLowerInvariant().Trim();
                    String CleanColor = AppLogic.CleanSizeColorOption(Color).ToLowerInvariant().Trim();
                    // To get the proper case of what the user inputs in both size and color
                    String CleanSizeProperCase = AppLogic.CleanSizeColorOption(Size).Trim();
                    String CleanColorProperCase = AppLogic.CleanSizeColorOption(Color).Trim();
                    int VariantID = MasterVariantID;
                    if (VariantID == 0 && XmlCommon.XmlAttributeNativeInt(n, "VariantID") != 0)
                    {
                        VariantID = XmlCommon.XmlAttributeNativeInt(n, "VariantID");
                    }
                    int ProductID = MasterProductID;
                    if (ProductID == 0)
                    {
                        SqlConnection con = null;
                        IDataReader rs1 = null;
                        try
                        {
                            string query = "select productid from productvariant   with (NOLOCK)  where deleted=0 and variantid=" + VariantID.ToString();
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs1 = DB.GetRS(query, m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs1 = DB.GetRS(query, con);
                            }

                            using (rs1)
                            {
                                if (rs1.Read())
                                {
                                    ProductID = DB.RSFieldInt(rs1, "ProductID");
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (con != null && m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }

                            // make sure we won't reference this again in code
                            rs1 = null;
                            con = null;
                        }
                    }

                    bool ProductTracksInventoryBySizeAndColor = false;

                    SqlConnection con2 = null;
                    IDataReader rs2 = null;
                    try
                    {
                        string query2 = "select TrackInventoryBySizeAndColor from Product  with (NOLOCK)  where Deleted<>1 and ProductID=" + ProductID.ToString();
                        if (m_DBTrans != null)
                        {
                            // if a transaction was passed, we should use the transaction objects connection
                            rs2 = DB.GetRS(query2, m_DBTrans);
                        }
                        else
                        {
                            // otherwise create it
                            con2 = new SqlConnection(DB.GetDBConn());
                            con2.Open();
                            rs2 = DB.GetRS(query2, con2);
                        }

                        using (rs2)
                        {
                            if (rs2.Read())
                            {
                                ProductTracksInventoryBySizeAndColor = DB.RSFieldBool(rs2, "TrackInventoryBySizeAndColor");
                            }
                        }
                    }
                    catch { throw; }
                    finally
                    {
                        // we can't dispose of the connection if it's part of a transaction
                        if (con2 != null && m_DBTrans == null)
                        {
                            // here it's safe to dispose since we created the connection ourself
                            con2.Dispose();
                        }

                        // make sure we won't reference this again in code
                        rs2 = null;
                        con2 = null;
                    }

                    if (!ProductTracksInventoryBySizeAndColor)
                    {
                        // simple inventory:

                        if (VariantID != 0)
                        {
                            RunCommand("update ProductVariant set Inventory=" + Quantity.ToString() + " where VariantID=" + VariantID.ToString());
                        }
                        else if (VariantSKU.Length != 0)
                        {
                            RunCommand("update ProductVariant set Inventory=" + Quantity.ToString() + " where SKUSuffix=" + DB.SQuote(VariantSKU));
                        }
                        else
                        {
                            Results.WriteErrorEntry("InventoryUpdate node found with no VariantID and no VariantSKU with simple inventory active on the product. This combination is not allowed. This inventory update record ignored.");
                        }
                    }
                    else
                    {
                        // inventory by size & color:

                        if (VariantID == 0 && MatchKey == InventoryMatchKeyEnum.SizeColor)
                        {
                            // not allowed
                            Results.WriteErrorEntry("InventoryUpdate node found with no VariantID and MatchKey=SizeColor. This combination is not allowed. This inventory update record ignored.");
                        }

                        bool InventoryRecordAlreadyExists = false;
                        switch (MatchKey)
                        {
                            case InventoryMatchKeyEnum.SizeColor:
                                InventoryRecordAlreadyExists = (DB.GetSqlN("select count(*) as N from Inventory where " + CommonLogic.IIF(VariantID != 0, "VariantID=" + VariantID.ToString() + " and ", "") + " lower(size)=" + DB.SQuote(CleanSize) + " and lower(color)=" + DB.SQuote(CleanColor), m_DBTrans) > 0);
                                break;
                            case InventoryMatchKeyEnum.VendorFullSKU:
                                InventoryRecordAlreadyExists = (DB.GetSqlN("select count(*) as N from Inventory where " + CommonLogic.IIF(VariantID != 0, "VariantID=" + VariantID.ToString() + " and ", "") + " VendorFullSKU=" + DB.SQuote(VendorFullSKU), m_DBTrans) > 0);
                                break;
                            case InventoryMatchKeyEnum.VendorID:
                                InventoryRecordAlreadyExists = (DB.GetSqlN("select count(*) as N from Inventory where " + CommonLogic.IIF(VariantID != 0, "VariantID=" + VariantID.ToString() + " and ", "") + " VendorID=" + DB.SQuote(VendorID), m_DBTrans) > 0);
                                break;
                        }
                        if (!InventoryRecordAlreadyExists && VariantID == 0)
                        {
                            // not allowed
                            Results.WriteErrorEntry("InventoryUpdate node with no VariantID and NO match was found in Inventory Table on " + MatchKey.ToString() + " field. Ignoring record.");
                        }
                        // Inserts the proper cases of color and size 
                        if (!InventoryRecordAlreadyExists)
                        {
                            String sqlI = String.Format("insert Inventory(VariantID,Size,Color,Quan,VendorID,WarehouseLocation,VendorFullSKU,WeightDelta,ExtensionData,GTIN) values({0},{1},{2},{3},{4},{5},{6},{7},{8},{9})",
                                                        VariantID.ToString(),
                                                        DB.SQuote(CleanSizeProperCase),
                                                        DB.SQuote(CleanColorProperCase),
                                                        Quantity.ToString(),
                                                        DB.SQuote(VendorID),
                                                        DB.SQuote(WarehouseLocation),
                                                        DB.SQuote(VendorFullSKU),
                                                        DB.SQuote(Localization.DecimalStringForDB(WeightDelta)),
                                                        DB.SQuote(ExtensionData),
														DB.SQuote(GTIN)
														);
                            RunCommand(sqlI);
                        }
                        else
                        {
                            bool anyFound = false;
                            StringBuilder sql2 = new StringBuilder(1024);
                            sql2.Append("update Inventory set ");
                            if (XmlCommon.NodeContainsAttribute(n, "Quantity"))
                            {
                                if (anyFound)
                                {
                                    sql2.Append(",");
                                }
                                sql2.Append("Quan=");
                                sql2.Append(Quantity.ToString());
                                anyFound = true;
                            }
							if (XmlCommon.NodeContainsAttribute(n, "GTIN"))
							{
								if (anyFound)
								{
									sql2.Append(",");
								}
								sql2.Append("GTIN=");
								sql2.Append(GTIN);
								anyFound = true;
							}
                            if (XmlCommon.NodeContainsAttribute(n, "VendorID"))
                            {
                                if (anyFound)
                                {
                                    sql2.Append(",");
                                }
                                sql2.Append("VendorID=");
                                sql2.Append(DB.SQuote(VendorID));
                                anyFound = true;
                            }
                            if (XmlCommon.NodeContainsAttribute(n, "WarehouseLocation"))
                            {
                                if (anyFound)
                                {
                                    sql2.Append(",");
                                }
                                sql2.Append("WarehouseLocation=");
                                sql2.Append(DB.SQuote(WarehouseLocation));
                                anyFound = true;
                            }
                            if (XmlCommon.NodeContainsAttribute(n, "VendorFullSKU"))
                            {
                                if (anyFound)
                                {
                                    sql2.Append(",");
                                }
                                sql2.Append("VendorFullSKU=");
                                sql2.Append(DB.SQuote(VendorFullSKU));
                                anyFound = true;
                            }
                            if (XmlCommon.NodeContainsAttribute(n, "WeightDelta"))
                            {
                                if (anyFound)
                                {
                                    sql2.Append(",");
                                }
                                sql2.Append("WeightDelta=");
                                sql2.Append(Localization.DecimalStringForDB(WeightDelta));
                                anyFound = true;
                            }
                            if (XmlCommon.NodeContainsAttribute(n, "ExtensionData"))
                            {
                                if (anyFound)
                                {
                                    sql2.Append(",");
                                }
                                sql2.Append("ExtensionData=");
                                sql2.Append(DB.SQuote(ExtensionData));
                                anyFound = true;
                            }
                            if (anyFound)
                            {
                                switch (MatchKey)
                                {
                                    case InventoryMatchKeyEnum.SizeColor:
                                        sql2.Append(" where " + CommonLogic.IIF(VariantID != 0, "VariantID=" + VariantID.ToString() + " and ", "") + " lower(size)=" + DB.SQuote(AppLogic.CleanSizeColorOption(Size).ToLowerInvariant()) + " and lower(color)=" + DB.SQuote(AppLogic.CleanSizeColorOption(Color).ToLowerInvariant()));
                                        break;
                                    case InventoryMatchKeyEnum.VendorFullSKU:
                                        sql2.Append(" where " + CommonLogic.IIF(VariantID != 0, "VariantID=" + VariantID.ToString() + " and ", "") + " VendorFullSKU=" + DB.SQuote(VendorFullSKU));
                                        break;
                                    case InventoryMatchKeyEnum.VendorID:
                                        sql2.Append(" where " + CommonLogic.IIF(VariantID != 0, "VariantID=" + VariantID.ToString() + " and ", "") + " VendorID=" + DB.SQuote(VendorID));
                                        break;
                                }
                                RunCommand(sql2.ToString());
                            }
                        }
                    }
                }
            }
        }

        private void hlpVariantUpdate(int ProductID, int VariantID, XmlNode node)
        {
            String Separator = String.Empty;
            StringBuilder sql = new StringBuilder(2500);
            sql.Append("update ProductVariant set ");
            Separator = AddXmlFieldToSQLFieldBool("IsDefault", "IsDefault", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldString("Name", "Name", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Description", "Description", node, sql, Separator, true, String.Empty);
            if (XmlCommon.XmlField(node, "SE/SEName").Length != 0)
            {
                sql.Append(Separator);
                sql.Append("SEName=");
                sql.Append(DB.SQuote(CommonLogic.Left(XmlCommon.XmlField(node, "SE/SEName"), 150)));
                Separator = ",";
            }
            else if (XmlCommon.XmlField(node, "Name").Length != 0)
            {
                sql.Append(Separator);
                sql.Append("SEName=");
                sql.Append(DB.SQuote(CommonLogic.Left(SE.MungeName(XmlCommon.XmlField(node, "Name")), 150)));
                Separator = ",";
            }
            Separator = AddXmlFieldToSQLFieldString("SE/SEKeywords", "SEKeywords", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SEDescription", "SEDescription", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("SE/SEAltText", "SEAltText", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("FroogleDescription", "FroogleDescription", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldInt("ProductID", "ProductID", node, sql, Separator, ProductID); // they just can't omit this one, we'll force it
            Separator = AddXmlFieldToSQLFieldString("SKUSuffix", "SKUSuffix", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ManufacturerPartNumber", "ManufacturerPartNumber", node, sql, Separator, true, String.Empty);
			Separator = AddXmlFieldToSQLFieldString("GTIN", "GTIN", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldDecimal("Price", "Price", node, sql, Separator, System.Decimal.Zero);
            Separator = AddXmlFieldToSQLFieldDecimal("SalePrice", "SalePrice", node, sql, Separator, System.Decimal.Zero);
            Separator = AddXmlFieldToSQLFieldDecimal("Weight", "Weight", node, sql, Separator, System.Decimal.Zero);
            Separator = AddXmlFieldToSQLFieldDecimal("MSRP", "MSRP", node, sql, Separator, System.Decimal.Zero);
            Separator = AddXmlFieldToSQLFieldDecimal("Cost", "Cost", node, sql, Separator, System.Decimal.Zero);
            Separator = AddXmlFieldToSQLFieldInt("Points", "Points", node, sql, Separator, 0);
            //Need to pull height, width, and length first and then update Dimensions
            if (XmlCommon.XmlField(node, "Dimensions").Length != 0)
            {
                sql.Append(Separator);
                sql.Append("Dimensions=");

                Decimal vHeight = XmlCommon.XmlFieldNativeDecimal(node, "Dimensions/Height");
                Decimal vWidth = XmlCommon.XmlFieldNativeDecimal(node, "Dimensions/Width");
                Decimal vLength = XmlCommon.XmlFieldNativeDecimal(node, "Dimensions/Length");

                sql.Append(DB.SQuote(vWidth.ToString() + "x" + vHeight.ToString()  + "x" + vLength.ToString()));
                Separator = ",";
            }
            Separator = AddXmlFieldToSQLFieldInt("Inventory", "Inventory", node, sql, Separator, 1000000);
            Separator = AddXmlFieldToSQLFieldInt("DisplayOrder", "DisplayOrder", node, sql, Separator, 1);
            Separator = AddXmlFieldToSQLFieldString("Notes", "Notes", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldBool("IsTaxable", "IsTaxable", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldBool("IsShipSeparately", "IsShipSeparately", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("IsDownload", "IsDownload", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldString("DownloadLocation", "DownloadLocation", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldInt("FreeShipping", "FreeShipping", node, sql, Separator, 0);
            Separator = AddXmlFieldToSQLFieldBool("Published", "Published", node, sql, Separator, true);
            Separator = AddXmlFieldToSQLFieldBool("Wholesale", "Wholesale", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldBool("IsRecurring", "IsRecurring", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldInt("RecurringInterval", "RecurringInterval", node, sql, Separator, 1);
            Separator = AddXmlFieldToSQLFieldInt("RecurringIntervalType", "RecurringIntervalType", node, sql, Separator, 3);
            Separator = AddXmlFieldToSQLFieldInt("SubscriptionInterval", "SubscriptionInterval", node, sql, Separator, 1);
            Separator = AddXmlFieldToSQLFieldInt("SubscriptionIntervalType", "SubscriptionIntervalType", node, sql, Separator, 3);
            Separator = AddXmlFieldToSQLFieldString("RestrictedQuantities", "RestrictedQuantities", node, sql, Separator, false, String.Empty);
            Separator = AddXmlFieldToSQLFieldInt("MinimumQuantity", "MinimumQuantity", node, sql, Separator, 0);
            Separator = AddXmlFieldToSQLFieldBool("CustomerEntersPrice", "CustomerEntersPrice", node, sql, Separator, false);
            Separator = AddXmlFieldToSQLFieldString("CustomerEntersPricePrompt", "CustomerEntersPricePrompt", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData", "ExtensionData", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData2", "ExtensionData2", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData3", "ExtensionData3", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData4", "ExtensionData4", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("ExtensionData5", "ExtensionData5", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldString("Images/ImageFilenameOverride", "ImageFilenameOverride", node, sql, Separator, true, String.Empty);
            Separator = AddXmlFieldToSQLFieldInt("Condition", "Condition", node, sql, Separator, 0);

            XmlNode nn;
            // process Sizes:
            nn = node.SelectSingleNode("Sizes");
            if (nn != null && nn.InnerXml.Length != 0)
            {
                String sx = String.Empty;
                StringBuilder Sizes = new StringBuilder(512);
                StringBuilder SizeSKUModifiers = new StringBuilder(512);
                foreach (XmlNode n in nn.SelectNodes("Size"))
                {
                    String theSize = n.InnerText;
                    String SKUModifier = XmlCommon.XmlAttribute(n, "SKUModifier");
                    Decimal PriceModifier = XmlCommon.XmlAttributeNativeDecimal(n, "PriceModifier");
                    if (theSize.Length != 0)
                    {
                        Sizes.Append(sx);
                        Sizes.Append(theSize);
                        if (PriceModifier != System.Decimal.Zero)
                        {
                            Sizes.Append("[");
                            Sizes.Append(Localization.CurrencyStringForGatewayWithoutExchangeRate(PriceModifier));
                            Sizes.Append("]");
                        }
                        SizeSKUModifiers.Append(sx);
                        SizeSKUModifiers.Append(SKUModifier);

                        sx = ",";
                    }
                }
                sql.Append(Separator);
                sql.Append("Sizes=");
                sql.Append(DB.SQuote(Sizes.ToString()));
                sql.Append(",");
                sql.Append("SizeSKUModifiers=");
                sql.Append(DB.SQuote(SizeSKUModifiers.ToString()));
                Separator = ",";
            }

            // process Colors:
            nn = node.SelectSingleNode("Colors");
            if (nn != null && nn.InnerXml.Length != 0)
            {
                String sx = String.Empty;
                StringBuilder Colors = new StringBuilder(512);
                StringBuilder ColorSKUModifiers = new StringBuilder(512);
                foreach (XmlNode n in nn.SelectNodes("Color"))
                {
                    String theColor = n.InnerText;
                    String SKUModifier = XmlCommon.XmlAttribute(n, "SKUModifier");
                    Decimal PriceModifier = XmlCommon.XmlAttributeNativeDecimal(n, "PriceModifier");
                    if (theColor.Length != 0)
                    {
                        Colors.Append(sx);
                        Colors.Append(theColor);
                        if (PriceModifier != System.Decimal.Zero)
                        {
                            Colors.Append("[");
                            Colors.Append(Localization.CurrencyStringForGatewayWithoutExchangeRate(PriceModifier));
                            Colors.Append("]");
                        }

                        ColorSKUModifiers.Append(sx);
                        ColorSKUModifiers.Append(SKUModifier);

                        sx = ",";
                    }
                }
                sql.Append(Separator);
                sql.Append("Colors=");
                sql.Append(DB.SQuote(Colors.ToString()));
                sql.Append(",");
                sql.Append("ColorSKUModifiers=");
                sql.Append(DB.SQuote(ColorSKUModifiers.ToString()));
                Separator = ",";
            }

            sql.Append(" where VariantId=" + VariantID.ToString());
            // was there anything to update:
            if (Separator != String.Empty)
            {
                RunCommand(sql.ToString());
            }

            nn = node.SelectSingleNode("InventoryBySizeAndColor");
            ProcessInventoryUpdate(nn, ProductID, VariantID);

            nn = node.SelectSingleNode("ShippingCosts");
            if (nn != null)
            {
                bool AutoCleanupSM = XmlCommon.XmlAttributeBool(nn, "AutoCleanup");
                if (AutoCleanupSM)
                {
                    RunCommand("delete from ShippingByProduct where VariantID=" + VariantID.ToString());
                }
                int PullFromVariantID = XmlCommon.XmlAttributeUSInt(nn, "PullFromVariantID");
                String PullFromVariantGUID = XmlCommon.XmlAttribute(nn, "PullFromVariantGUID");
                if (PullFromVariantID != 0 || PullFromVariantGUID.Length != 0)
                {
                    PullFromVariantID = SetupVariantID(ProductID, PullFromVariantID, PullFromVariantGUID, String.Empty, String.Empty);
                    RunCommand("insert ShippingByProduct(VariantID,ShippingMethodID,ShippingCost) select " + VariantID.ToString() + ",ShippingMethodID,ShippingCost from ShippingByProduct where VariantID=" + PullFromVariantID.ToString());
                }
                else
                {
                    foreach (XmlNode nx in nn.SelectNodes("ShippingMethod"))
                    {
                        int SMID = XmlCommon.XmlAttributeUSInt(nx, "ShippingMethodID");
                        String SMGUID = XmlCommon.XmlAttribute(nx, "ShippingMethodGUID");
                        SMID = SetupTableID("ShippingMethod", SMID, SMGUID);
                        if (SMID != 0)
                        {
                            if (DB.GetSqlN("select count(*) as N from ShippingByProduct where VariantID=" + VariantID.ToString() + " and ShippingMethodID=" + SMID.ToString(), m_DBTrans) == 0)
                            {
                                RunCommand("insert ShippingByProduct(VariantID,ShippingMethodID,ShippingCost) values(" + VariantID.ToString() + "," + SMID + "," + XmlCommon.XmlAttributeUSDecimal(nx, "Cost") + ")");
                            }
                            else
                            {
                                RunCommand("update ShippingByProduct set Cost=" + XmlCommon.XmlAttributeUSDecimal(nx, "Cost") + " where VariantID=" + VariantID.ToString() + " and ShippingMethodID=" + SMID.ToString());
                            }
                        }
                    }
                }
            }

            nn = node.SelectSingleNode("ExtendedPrices");
            if (nn != null)
            {
                bool AutoCleanupSM = XmlCommon.XmlAttributeBool(nn, "AutoCleanup");
                if (AutoCleanupSM)
                {
                    RunCommand("delete from ExtendedPrice where VariantID=" + VariantID.ToString());
                }
                int PullFromVariantID = XmlCommon.XmlAttributeUSInt(nn, "PullFromVariantID");
                String PullFromVariantGUID = XmlCommon.XmlAttribute(nn, "PullFromVariantGUID");
                if (PullFromVariantID != 0 || PullFromVariantGUID.Length != 0)
                {
                    PullFromVariantID = SetupVariantID(ProductID, PullFromVariantID, PullFromVariantGUID, String.Empty, String.Empty);
                    RunCommand("insert ExtendedPrice(VariantID,CustomerLevelID,Price) select " + VariantID.ToString() + ",CustomerLevelID,Price from ExtendedPrice where VariantID=" + PullFromVariantID.ToString());
                }
                else
                {
                    foreach (XmlNode nx in nn.SelectNodes("CustomerLevel"))
                    {
                        //changed CustomerLevelID to ID and CustomerLevelGUID to GUID to match documentation
                        int CLID = XmlCommon.XmlAttributeUSInt(nx, "ID");
                        String CLGUID = XmlCommon.XmlAttribute(nx, "GUID");

                        CLID = SetupTableID("CustomerLevel", CLID, CLGUID);
                        if (CLID != 0)
                        {
                            if (DB.GetSqlN("select count(*) as N from ExtendedPrice where VariantID=" + VariantID.ToString() + " and CustomerLevelID=" + CLID.ToString(), m_DBTrans) == 0)
                            {
                                RunCommand("insert ExtendedPrice(VariantID,CustomerLevelID,Price) values(" + VariantID.ToString() + "," + CLID + "," + XmlCommon.XmlAttributeUSDecimal(nx, "Price") + ")");
                            }
                            else
                            {
                                RunCommand("update ExtendedPrice set Price=" + XmlCommon.XmlAttributeUSDecimal(nx, "Price") + " where VariantID=" + VariantID.ToString() + " and CustomerLevelID=" + CLID.ToString());
                            }
                        }
                    }
                }
            }

            // process images:
            foreach (String imagesize in "Icon,Medium,Large".Split(','))
            {
                String img_Params = String.Empty;
                Boolean img_UseAppConfigs = false;
                Boolean img_Resize = false;

                XmlNode nnx = node.SelectSingleNode("Images/" + imagesize);
                if (nnx != null)
                {
                    String ImageType = XmlCommon.XmlAttribute(nnx, "ImageType");
                    if (ImageType.Length == 0)
                    {
                        ImageType = XmlCommon.XmlAttribute(nnx, "Extension");
                    }
                    String ImageData = XmlCommon.XmlField(node, "Images/" + imagesize);
                    bool DeleteFlag = XmlCommon.XmlAttributeBool(nnx, "Delete");
                    String FN = XmlCommon.XmlField(node, "Images/ImageFilenameOverride");
                    if (FN.Length == 0)
                    {
                        FN = VariantID.ToString();
                    }
                    // delete any current image file first
                    foreach (String ss in CommonLogic.SupportedImageTypes)
                    {
                        if (FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) || FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                System.IO.File.Delete(AppLogic.GetImagePath("Variant", imagesize, true) + FN);
                            }
                            catch
                            { }
                        }
                        else
                        {
                            try
                            {
                                System.IO.File.Delete(AppLogic.GetImagePath("Variant", imagesize, true) + FN + ss);
                            }
                            catch
                            { }
                        }
                    }

                    if (ImageData.Length != 0 && !DeleteFlag)
                    {
                        String img_ContentType = String.Empty;
                        String TempImage = String.Empty;

                        //need to keep the original filename incase we are creating other sizes when uploading the large.
                        String eoID = FN;

                        byte[] imagedatabytes = Convert.FromBase64String(ImageData);
                        if (!FN.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) && !FN.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        {
                            switch (ImageType)
                            {
                                case "gif":
                                    TempImage = AppLogic.GetImagePath("Variant", imagesize, true) + "tmp_" + FN + ".gif";
                                    FN = AppLogic.GetImagePath("Variant", imagesize, true) + FN + ".gif";
                                    img_ContentType = "image/gif";
                                    break;
                                case "x-png":
                                    FN = AppLogic.GetImagePath("Variant", imagesize, true) + FN + ".png";
                                    TempImage = AppLogic.GetImagePath("Variant", imagesize, true) + "tmp_" + FN + ".png";
                                    FN = AppLogic.GetImagePath("Variant", imagesize, true) + FN + ".png";
                                    img_ContentType = "image/png";
                                    break;
                                case "jpg":
                                case "jpeg":
                                case "pjpeg":
                                    TempImage = AppLogic.GetImagePath("Variant", imagesize, true) + "tmp_" + FN + ".jpg";
                                    FN = AppLogic.GetImagePath("Variant", imagesize, true) + FN + ".jpg";
                                    img_ContentType = "image/jpeg";
                                    break;
                            }
                        }
                        else
                        {
                            TempImage = AppLogic.GetImagePath("Variant", imagesize, true) + "tmp_" + FN;
                            FN = AppLogic.GetImagePath("Variant", imagesize, true) + FN;
                            img_ContentType = "image/jpeg";
                        }
                        try
                        {
                            img_UseAppConfigs = XmlCommon.XmlAttributeBool(nnx, "UseAppConfigs");

                            if (img_UseAppConfigs)
                            {
                                img_Resize = AppLogic.AppConfigBool("UseImageResize");
                            }
                            if (XmlCommon.XmlAttribute(nnx, "Resize").Length > 0)
                            {
                                img_Resize = XmlCommon.XmlAttributeBool(nnx, "Resize");
                            }


                            if (img_Resize)
                            {
                                img_Params = XmlCommon.XmlAttribute(nnx, "Params");

                                File.WriteAllBytes(TempImage, imagedatabytes);

                                AppLogic.ResizeEntityOrObject("Variant", TempImage, FN, eoID, imagesize, img_ContentType, img_Params, img_UseAppConfigs);

                            }
                            else
                            {
                                File.WriteAllBytes(FN, imagedatabytes);
                            }
                        }
                        catch
                        {
                            Results.WriteErrorEntry("Could not write file: " + FN);
                        }
                        finally
                        {
                            AppLogic.DisposeOfTempImage(TempImage);
                        }
                    }
                }
            }

            // Added to deal with electronic download files.  This is specific to DotNetNuke Marketplace
            XmlNode df = node.SelectSingleNode("DownloadFile");
            if (df != null)
            {
                String FileData = df.InnerText.Trim();
                String SKU = DB.GetSqlSAllLocales("Select p.SKU + ISNULL(pv.SKUSuffix, '') as S from Product p JOIN ProductVariant pv on pv.ProductID = p.ProductID WHERE pv.VariantID=" + VariantID.ToString());
                String PNAME = DB.GetSqlSAllLocales("Select p.Name + ' ' + ISNULL(pv.Name, '') as S from Product p JOIN ProductVariant pv on pv.ProductID = p.ProductID WHERE pv.VariantID=" + VariantID.ToString()).Trim();
                String VirtAppPath = HttpRuntime.AppDomainAppVirtualPath;
                String DFN = HttpContext.Current.Request.MapPath(String.Format("{0}/{1}/OrderDownloads.xml", VirtAppPath, AppLogic.AppConfig("DownloadRootDirectory")));
                String FN = HttpContext.Current.Request.MapPath(String.Format("{0}/{1}/{2}/Marketplace.UnzipMe.{2}.zip.resources", VirtAppPath, AppLogic.AppConfig("DownloadRootDirectory"), SKU));
                String DN = Path.GetDirectoryName(FN);

                // delete any current variant file first
                if (System.IO.File.Exists(FN))
                {
                    try
                    {
                        System.IO.File.Delete(FN);
                    }
                    catch { }
                }
                if (!System.IO.Directory.Exists(DN))
                {
                    System.IO.Directory.CreateDirectory(DN);
                }
                if (FileData.Length != 0)
                {

                    byte[] filedatabytes = Convert.FromBase64String(FileData);
                    try
                    {
                        File.WriteAllBytes(FN, filedatabytes);
                    }
                    catch
                    {
                        Results.WriteErrorEntry("Could not write file: " + FN);
                    }
                }

                try
                {
                    XmlDocument downloadsDoc = new XmlDocument();
                    downloadsDoc.Load(DFN);
                    XmlElement root = downloadsDoc.DocumentElement;

                    XmlNode files = root.SelectSingleNode("files[@rolename='" + SKU + "']");
                    if (files == null)
                    {
                        files = downloadsDoc.CreateElement("files");
                    }
                    else
                    {
                        files.RemoveAll();
                    }
                    XmlAttribute role = downloadsDoc.CreateAttribute("rolename");
                    role.Value = SKU;
                    files.Attributes.Append(role);

                    XmlAttribute productName = downloadsDoc.CreateAttribute("productname");
                    productName.Value = PNAME;
                    files.Attributes.Append(productName);

                    XmlElement file = downloadsDoc.CreateElement("file");
                    XmlAttribute name = downloadsDoc.CreateAttribute("name");
                    name.Value = PNAME;
                    file.Attributes.Append(name);

                    XmlAttribute location = downloadsDoc.CreateAttribute("location");
                    location.Value = "/" + Path.GetFileNameWithoutExtension(FN);
                    file.Attributes.Append(location);

                    files.AppendChild(file);
                    root.AppendChild(files);

                    downloadsDoc.Save(DFN);
                }
                catch
                {
                    // The only real error here would be a file lock issue.  Will look at this if it turns into a problem.
                }
                // End DotNetNuke MarketPlace
            }
        }


        private int ProcessEntity(XmlNode node, int ParentEntityID)
        {
            String NodeType = node.Name;
            String EntityName = XmlCommon.XmlField(node, "Name");
            String EntityXPath = XmlCommon.XmlField(node, "XPath");
            String XPathLookup = XmlCommon.XmlAttribute(node, "XPathLookup");
            String EntityType = XmlCommon.XmlAttribute(node, "EntityType");
            EntitySpecs Specs = EntityDefinitions.LookupSpecs(EntityType);
            NodeActionEnum NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            String EntityGUID = XmlCommon.XmlAttribute(node, "GUID");
            int EntityID = XmlCommon.XmlAttributeNativeInt(node, "ID");
            bool RemoveMappings = XmlCommon.XmlAttributeBool(node, "RemoveMappings");

            Results.WriteVerboseEntry("Processing " + NodeType + ", Type=" + EntityType + ", Name=" + EntityName + ", XPath=" + EntityXPath + ", Action=" + NodeAction + ", ID=" + EntityID.ToString() + ", GUID=" + EntityGUID + ", RemoveMappings=" + RemoveMappings.ToString());

            if (EntityXPath.Length != 0)
            {
                if (EntityXPath.StartsWith("/"))
                {
                    ParentEntityID = 0; // force it
                    EntityXPath = EntityXPath.Substring(1, EntityXPath.Length - 1);
                }
                int ix = EntityXPath.LastIndexOf("/");
                if (ix != -1)
                {
                    EntityName = EntityXPath.Substring(ix + 1);
                }
                else
                {
                    EntityName = EntityXPath;
                }
            }

            if (XPathLookup.Length != 0)
            {
                if (XPathLookup.StartsWith("/"))
                {
                    XPathLookup = XPathLookup.Substring(1, XPathLookup.Length - 1);
                }
            }

            if (Specs == null)
            {
                Results.WriteErrorEntry("Invalid Entity Type Specified");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Invalid Entity Type Specified");
                }
                return 0;
            }

            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Add:
                        {
                            if (EntityXPath.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, EntityXPath);
                            }
                            else
                            {
                                EntityID = CheckForEntity(Specs.m_EntityName, EntityName, ParentEntityID);
                            }
                            if (EntityID != 0)
                            {
                                if (EntityGUID.Length == 0)
                                {
                                    SqlConnection coni = null;
                                    IDataReader rsi = null;
                                    try
                                    {
                                        string query = "select ^GUID from ^ where ^ID=".Replace("^", Specs.m_EntityName) + EntityID.ToString();
                                        if (m_DBTrans != null)
                                        {
                                            // if a transaction was passed, we should use the transaction objects connection
                                            rsi = DB.GetRS(query, m_DBTrans);
                                        }
                                        else
                                        {
                                            // otherwise create it
                                            coni = new SqlConnection(DB.GetDBConn());
                                            coni.Open();
                                            rsi = DB.GetRS(query, coni);
                                        }

                                        using (rsi)
                                        {
                                            if (rsi.Read())
                                            {
                                                EntityGUID = DB.RSFieldGUID(rsi, Specs.m_EntityName + "GUID");
                                            }
                                        }
                                    }
                                    catch { throw; }
                                    finally
                                    {
                                        // we can't dispose of the connection if it's part of a transaction
                                        if (coni != null && m_DBTrans == null)
                                        {
                                            // here it's safe to dispose since we created the connection ourself
                                            coni.Dispose();
                                        }

                                        // make sure we won't reference this again in code
                                        rsi = null;
                                        coni = null;
                                    }
                                }

                                Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "ERROR", "Duplicate " + Specs.m_EntityName + ", skipping...");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Duplicate " + Specs.m_EntityName);
                                }
                                return 0;
                            }
                            else
                            {
                                if (EntityXPath.Length != 0)
                                {
                                    EntityID = LoadEntityTree(Specs.m_EntityName, EntityXPath, ParentEntityID, EntityGUID, EntityName);
                                    hlpEntityUpdate(Specs, EntityID, node);

                                    SqlConnection coni = null;
                                    IDataReader rsi = null;
                                    try
                                    {
                                        string query = "select ^GUID from ^ where ^ID=".Replace("^", Specs.m_EntityName) + EntityID.ToString();
                                        if (m_DBTrans != null)
                                        {
                                            // if a transaction was passed, we should use the transaction objects connection
                                            rsi = DB.GetRS(query, m_DBTrans);
                                        }
                                        else
                                        {
                                            // otherwise create it
                                            coni = new SqlConnection(DB.GetDBConn());
                                            coni.Open();
                                            rsi = DB.GetRS(query, coni);
                                        }

                                        using (rsi)
                                        {
                                            if (rsi.Read())
                                            {
                                                EntityGUID = DB.RSFieldGUID(rsi, Specs.m_EntityName + "GUID");
                                            }
                                        }
                                    }
                                    catch { throw; }
                                    finally
                                    {
                                        // we can't dispose of the connection if it's part of a transaction
                                        if (coni != null && m_DBTrans == null)
                                        {
                                            // here it's safe to dispose since we created the connection ourself
                                            coni.Dispose();
                                        }

                                        // make sure we won't reference this again in code
                                        rsi = null;
                                        coni = null;
                                    }

                                    Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "OK", String.Empty);
                                }
                                else
                                {
                                    if (EntityGUID.Length == 0)
                                    {
                                        EntityGUID = CommonLogic.GetNewGUID();
                                    }
                                    StringBuilder sql = new StringBuilder(1024);
                                    sql.Append("insert ^(^GUID,Name,SEName,Parent^ID) values(");
                                    sql.Append(DB.SQuote(EntityGUID) + ",");
                                    sql.Append(DB.SQuote(EntityName) + ",");
                                    sql.Append(DB.SQuote(SE.MungeName(EntityName)) + ",");
                                    sql.Append(ParentEntityID.ToString());
                                    sql.Append(")");
                                    sql = sql.Replace("^", Specs.m_EntityName);
                                    RunCommand(sql.ToString());
                                    Results.WriteVerboseEntry(NodeType + " OK");
                                    EntityID = LookupEntityByGUID(Specs.m_EntityName, EntityGUID);
                                    hlpEntityUpdate(Specs, EntityID, node);
                                    Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "OK", String.Empty);
                                }
                            }
                            break;
                        }
                    case NodeActionEnum.Update:
                        {
                            if (XPathLookup.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, XPathLookup);
                            }
                            else if (EntityXPath.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, EntityXPath);
                            }
                            else
                            {
                                EntityID = SetupEntityNodeID(EntityID, EntityGUID, NodeType, Specs.m_EntityName, EntityName, NodeAction);
                            }
                            if (EntityID == 0 && EntityGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                if (EntityID != 0)
                                {
                                    hlpEntityUpdate(Specs, EntityID, node);
                                    Results.WriteVerboseEntry(NodeType + " OK");
                                    Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "OK", String.Empty);
                                }
                                else
                                {
                                    if (m_AutoLazyAdd)
                                    {
                                        Results.WriteVerboseEntry(NodeType + " LazyAdd being performed...");
                                        NodeAction = NodeActionEnum.Add;
                                        EntityID = CheckForEntity(Specs.m_EntityName, EntityName, ParentEntityID);
                                        if (EntityID != 0)
                                        {
                                            Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "ERROR", "Duplicate " + Specs.m_EntityName + ", skipping...");
                                            if (m_DBTrans != null)
                                            {
                                                throw new ArgumentException("Duplicate " + Specs.m_EntityName);
                                            }
                                        }
                                        else
                                        {
                                            if (EntityGUID.Length == 0)
                                            {
                                                EntityGUID = CommonLogic.GetNewGUID();
                                            }
                                            StringBuilder sql = new StringBuilder(1024);
                                            sql.Append("insert ^(^GUID,Name,SEName,Parent^ID) values(");
                                            sql.Append(DB.SQuote(EntityGUID) + ",");
                                            sql.Append(DB.SQuote(EntityName) + ",");
                                            sql.Append(DB.SQuote(SE.MungeName(EntityName)) + ",");
                                            sql.Append(ParentEntityID.ToString());
                                            sql.Append(")");
                                            sql = sql.Replace("^", Specs.m_EntityName);
                                            RunCommand(sql.ToString());
                                            Results.WriteVerboseEntry(NodeType + " OK");
                                            EntityID = LookupEntityByGUID(Specs.m_EntityName, EntityGUID);
                                            hlpEntityUpdate(Specs, EntityID, node);
                                            Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "OK", String.Empty);
                                        }
                                    }
                                    else
                                    {
                                        Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "ERROR", "No ID or GUID Specified");
                                        if (m_DBTrans != null)
                                        {
                                            throw new ArgumentException("No ID or GUID Specified");
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            if (XPathLookup.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, XPathLookup);
                            }
                            else if (EntityXPath.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, EntityXPath);
                            }
                            else
                            {
                                EntityID = SetupEntityNodeID(EntityID, EntityGUID, NodeType, Specs.m_EntityName, EntityName, NodeAction);
                            }
                            if (EntityID == 0 && EntityGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                String sql = "update ^ set Deleted=1 where ^ID=" + EntityID.ToString();
                                sql = sql.Replace("^", Specs.m_EntityName);
                                RunCommand(sql.ToString());

                                if (RemoveMappings)
                                {
                                    // delete the mappings to this entity also on nuke:
                                    sql = "delete from ~^ where ^ID=" + EntityID.ToString();
                                    sql = sql.Replace("^", EntityType).Replace("~", Specs.m_ObjectName);
                                    RunCommand(sql);
                                }

                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Nuke:
                        {
                            if (XPathLookup.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, XPathLookup);
                            }
                            else if (EntityXPath.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, EntityXPath);
                            }
                            else
                            {
                                EntityID = SetupEntityNodeID(EntityID, EntityGUID, NodeType, Specs.m_EntityName, EntityName, NodeAction);
                            }
                            if (EntityID == 0 && EntityGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                String sql = "delete from ^ where ^ID=" + EntityID.ToString();
                                sql = sql.Replace("^", Specs.m_EntityName);
                                RunCommand(sql);

                                // delete the mappings to this entity also on nuke:
                                sql = "delete from ~^ where ^ID=" + EntityID.ToString();
                                sql = sql.Replace("^", EntityType).Replace("~", Specs.m_ObjectName);
                                RunCommand(sql);

                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Lookup:
                        {
                            if (XPathLookup.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, XPathLookup);
                            }
                            else if (EntityXPath.Length != 0)
                            {
                                EntityID = this.LookupEntityByXPath(Specs.m_EntityName, EntityXPath);
                            }
                            else
                            {
                                EntityID = SetupEntityNodeID(EntityID, EntityGUID, NodeType, Specs.m_EntityName, EntityName, NodeAction);
                            }
                            // if id and guid lookup failed, try it by name:
                            if (EntityID == 0)
                            {
                                EntityID = this.LookupEntityByName(Specs.m_EntityName, EntityName, ParentEntityID);
                            }
                            if (EntityID != 0)
                            {
                                String sql = "select * from ^  with (NOLOCK)  where ^ID=" + EntityID.ToString();
                                sql = sql.Replace("^", Specs.m_EntityName);

                                SqlConnection con = null;
                                IDataReader rs = null;
                                try
                                {
                                    string query = sql;
                                    if (m_DBTrans != null)
                                    {
                                        // if a transaction was passed, we should use the transaction objects connection
                                        rs = DB.GetRS(query, m_DBTrans);
                                    }
                                    else
                                    {
                                        // otherwise create it
                                        con = new SqlConnection(DB.GetDBConn());
                                        con.Open();
                                        rs = DB.GetRS(query, con);
                                    }

                                    using (rs)
                                    {
                                        if (rs.Read())
                                        {
                                            EntityName = DB.RSField(rs, "Name");
                                            EntityGUID = DB.RSFieldGUID(rs, Specs.m_EntityName + "GUID");
                                        }
                                    }
                                }
                                catch { throw; }
                                finally
                                {
                                    // we can't dispose of the connection if it's part of a transaction
                                    if (con != null && m_DBTrans == null)
                                    {
                                        // here it's safe to dispose since we created the connection ourself
                                        con.Dispose();
                                    }

                                    // make sure we won't reference this again in code
                                    rs = null;
                                    con = null;
                                }
                            }
                            if (EntityID != 0 && EntityName.Length != 0)
                            {
                                Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                Results.WriteOutputEntry(NodeType, EntityName, EntityGUID, EntityID, NodeAction, "ERROR", "Not Found");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Not Found");
                                }
                            }
                            break;
                        }
                }
                // recurse sub-entities, if any:
                XmlNodeList NodeList = node.SelectNodes("Entity");
                foreach (XmlNode n in NodeList)
                {
                    ProcessEntity(n, EntityID);
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return EntityID;
        }

        public void EnsureProductHasADefaultVariantSet(int ProductID)
        {
            if (DB.GetSqlN("select count(VariantID) as N from ProductVariant where Deleted=0 and Published=1 and ProductID=" + ProductID.ToString() + " and IsDefault=1", m_DBTrans) == 0)
            {
                // force a default variant, none was specified!
                RunCommand("update ProductVariant set IsDefault=1 where Deleted=0 and ProductID=" + ProductID.ToString() + " and VariantID in (SELECT top 1 VariantID from ProductVariant where Deleted=0 and Published=1 and ProductID=" + ProductID.ToString() + " order by DisplayOrder,Name)");
            }
            RunCommand("update ProductVariant set IsDefault=0 where Deleted<>0 or Published=0 and ProductID=" + ProductID.ToString());
        }

        private int ProcessProduct(XmlNode node)
        {
            String NodeType = node.Name;
            String ProductName = XmlCommon.XmlField(node, "Name");
            String ProductSKU = XmlCommon.XmlField(node, "SKU");
            NodeActionEnum NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            String ProductGUID = XmlCommon.XmlAttribute(node, "GUID");
            int ProductID = XmlCommon.XmlAttributeNativeInt(node, "ID");
            bool EnsureDefaultVariant = true; // default to true if not present
            if (XmlCommon.XmlAttribute(node, "EnsureDefaultVariant").Length != 0)
            {
                EnsureDefaultVariant = XmlCommon.XmlAttributeBool(node, "EnsureDefaultVariant");
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + ProductName + ", Action=" + NodeAction + ", ID=" + ProductID.ToString() + ", GUID=" + ProductGUID);

            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Add:
                        {
                            ProductID = SetupTableID("Product", ProductID, ProductGUID);
                            if (ProductID != 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "ERROR", "Duplicate Product, skipping...");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Duplicate Product");
                                }
                            }
                            else
                            {
                                if (ProductGUID.Length == 0)
                                {
                                    ProductGUID = CommonLogic.GetNewGUID();
                                }
                                StringBuilder sql = new StringBuilder(1024);
                                sql.Append("insert Product(ProductGUID,Name,SEName) values(");
                                sql.Append(DB.SQuote(ProductGUID) + ",");
                                sql.Append(DB.SQuote(ProductName) + ",");
                                sql.Append(DB.SQuote(SE.MungeName(ProductName)) + ")");
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                ProductID = LookupTableRecordByGUID("Product", ProductGUID);
                                hlpProductUpdate(ProductID, node);
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Update:
                        {
                            ProductID = SetupProductID(ProductID, ProductGUID, ProductName, ProductSKU);
                            if (ProductID != 0)
                            {
                                hlpProductUpdate(ProductID, node);
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                if (m_AutoLazyAdd)
                                {
                                    Results.WriteVerboseEntry(NodeType + " LazyAdd being performed...");
                                    NodeAction = NodeActionEnum.Add;
                                    if (ProductGUID.Length == 0)
                                    {
                                        ProductGUID = CommonLogic.GetNewGUID();
                                    }
                                    StringBuilder sql = new StringBuilder(1024);
                                    sql.Append("insert Product(ProductGUID,Name,SEName) values(");
                                    sql.Append(DB.SQuote(ProductGUID) + ",");
                                    sql.Append(DB.SQuote(CommonLogic.Left(ProductName, 400)) + ",");
                                    sql.Append(DB.SQuote(SE.MungeName(ProductName)) + ")");
                                    RunCommand(sql.ToString());
                                    Results.WriteVerboseEntry(NodeType + " OK");
                                    ProductID = LookupTableRecordByGUID("Product", ProductGUID);
                                    hlpProductUpdate(ProductID, node);
                                    Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "OK", String.Empty);
                                }
                                else
                                {
                                    Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "ERROR", "No ID or GUID Specified");
                                    if (m_DBTrans != null)
                                    {
                                        throw new ArgumentException("No ID or GUID Specified");
                                    }
                                }
                            }
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            ProductID = SetupTableID("Product", ProductID, ProductGUID);
                            if (ProductID == 0 && ProductGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                String sql = "update Product set Deleted=1 where ProductID=" + ProductID.ToString();
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Nuke:
                        {
                            ProductID = SetupTableID("Product", ProductID, ProductGUID);
                            if (ProductID == 0 && ProductGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                RunCommand("aspdnsf_NukeProduct " + ProductID.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Lookup:
                        {
                            ProductID = SetupProductID(ProductID, ProductGUID, ProductName, ProductSKU);
                            if (ProductID != 0)
                            {
                                String sql = "select * from Product  with (NOLOCK)  where deleted<>1 and ProductID=" + ProductID.ToString();

                                SqlConnection con = null;
                                IDataReader rs = null;
                                try
                                {
                                    string query = sql;
                                    if (m_DBTrans != null)
                                    {
                                        // if a transaction was passed, we should use the transaction objects connection
                                        rs = DB.GetRS(query, m_DBTrans);
                                    }
                                    else
                                    {
                                        // otherwise create it
                                        con = new SqlConnection(DB.GetDBConn());
                                        con.Open();
                                        rs = DB.GetRS(query, con);
                                    }

                                    using (rs)
                                    {
                                        if (rs.Read())
                                        {
                                            ProductName = DB.RSField(rs, "Name");
                                            ProductGUID = DB.RSFieldGUID(rs, "ProductGUID");
                                        }
                                    }
                                }
                                catch { throw; }
                                finally
                                {
                                    // we can't dispose of the connection if it's part of a transaction
                                    if (con != null && m_DBTrans == null)
                                    {
                                        // here it's safe to dispose since we created the connection ourself
                                        con.Dispose();
                                    }

                                    // make sure we won't reference this again in code
                                    rs = null;
                                    con = null;
                                }


                            }
                            if (ProductID != 0 && ProductName.Length != 0)
                            {
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                Results.WriteOutputEntry(NodeType, ProductName, ProductGUID, ProductID, NodeAction, "ERROR", "Not Found");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Not Found");
                                }
                            }
                            break;
                        }
                }
                if (ProductID != 0)
                {
                    // process variants, if any:
                    XmlNode vv = node.SelectSingleNode("Variants");
                    if (vv != null)
                    {
                        StringBuilder VariantList = new StringBuilder(128);
                        String Separator = String.Empty;
                        XmlNodeList NodeList = vv.SelectNodes("Variant");
                        foreach (XmlNode n in NodeList)
                        {
                            VariantList.Append(Separator);
                            VariantList.Append(ProcessVariant(n, ProductID).ToString());
                            Separator = ",";
                        }

                        // do autoclean of variants if specified
                        bool AutoCleanupVariants = XmlCommon.XmlAttributeBool(vv, "AutoCleanup");
                        if (AutoCleanupVariants && VariantList.Length != 0)
                        {
                            String sql = "delete from ProductVariant where ProductID=" + ProductID.ToString() + " and VariantID not in (" + VariantList.ToString() + ")";  //Cannot concat types String and StringBuilder in VB
                            RunCommand(sql);
                        }

                        //MakeSureProductHasAtLeastOneVariant(ProductID);
                        if (EnsureDefaultVariant)
                        {
                            EnsureProductHasADefaultVariantSet(ProductID);
                        }
                    }

                    if (XmlCommon.XmlFieldBool(node, "IsAKit"))
                    {
                        // process kit groups & itesm, if any:
                        XmlNode kn = node.SelectSingleNode("Kit");
                        if (kn != null)
                        {
                            StringBuilder GroupList = new StringBuilder(128);
                            String Separator = String.Empty;
                            XmlNodeList NodeList = kn.SelectNodes("KitGroup");
                            foreach (XmlNode n in NodeList)
                            {
                                GroupList.Append(Separator);
                                GroupList.Append(LoadKitGroup(n, ProductID).ToString());
                                Separator = ",";
                            }

                            // do autoclean of kit groups and items if specified
                            bool AutoCleanupKits = XmlCommon.XmlAttributeBool(kn, "AutoCleanup");
                            if (AutoCleanupKits && GroupList.Length != 0)
                            {
                                String sql = "delete from KitItem where KitGroupID not in (select distinct KitGroupID from KitGroup with (NOLOCK) where ProductID=" + ProductID.ToString() + " and KitGroupID in (" + GroupList.ToString() + "))";  //Cannot concat types String and StringBuilder in VB
                                RunCommand(sql);
                                sql = "delete from KitGroup where ProductID=" + ProductID.ToString() + " and KitGroupID not in (" + GroupList.ToString() + ")";  //Cannot concat types String and StringBuilder in VB
                                RunCommand(sql);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return ProductID;
        }

        private void ProcessShoppingCart(XmlNode node)
        {
            String NodeType = node.Name;
            NodeActionEnum NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            CartTypeEnum CartType = (CartTypeEnum)Enum.Parse(typeof(CartTypeEnum), XmlCommon.XmlAttribute(node, "CartType"));
            int CustomerID = XmlCommon.XmlAttributeNativeInt(node, "CustomerID");
            String CustomerGUID = XmlCommon.XmlAttribute(node, "CustomerGUID");
            String CustomerEMail = XmlCommon.XmlAttribute(node, "CustomerEMail");

            Results.WriteVerboseEntry("Processing " + NodeType + ", Action=" + NodeAction + ", CartType=" + CartType.ToString() + ", CustomerID=" + CustomerID.ToString() + ", CustomerGUID=" + CustomerGUID + ", CustomerEMail=" + CustomerEMail);

            CustomerID = SetupTableID("Customer", CustomerID, CustomerGUID);
            if (CustomerID == 0 && CustomerEMail.Length != 0)
            {
                SqlConnection con = null;
                IDataReader rs = null;
                try
                {
                    string query = "select CustomerID,CustomerGUID from Customer  with (NOLOCK)  where deleted=0 and lower(EMail)=" + DB.SQuote(CustomerEMail.ToLowerInvariant());
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        if (rs.Read())
                        {
                            CustomerID = DB.RSFieldInt(rs, "CustomerID");
                            CustomerGUID = DB.RSFieldGUID(rs, "CustomerGUID");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs = null;
                    con = null;
                }

            }
            if (CustomerID == 0)
            {
                Results.WriteErrorEntry("Required CustomerID, CustomerGUID or EMail not provided or Invalid");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("Required CustomerID, CustomerGUID or EMail not provided or Invalid");
                }
                return;
            }

            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Clear:
                        {
                            RunCommand("delete from KitCart where CartType=" + ((int)CartType).ToString() + " and (ShoppingCartRecID=0 or ShoppingCartRecID in (select ShoppingCartRecID from ShoppingCart where CustomerID=" + m_ThisCustomer.CustomerID.ToString() + " and CartType=" + ((int)CartType).ToString() + "))");
                            RunCommand("delete from ShoppingCart where CustomerID=" + m_ThisCustomer.CustomerID.ToString() + " and CartType=" + ((int)CartType).ToString());
                            Results.WriteOutputEntry(NodeType, String.Empty, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                            break;
                        }
                    case NodeActionEnum.Get:
                        {
                            // retreive the specified Cart:
                            Results.WriteXml(AppLogic.RunXmlPackage("DumpShoppingCart.xml.config", null, m_ThisCustomer, m_ThisCustomer.SkinID, String.Empty, "CustomerID=" + CustomerID.ToString() + "&CartType=" + ((int)CartType).ToString(), false, false));
                            Results.WriteVerboseEntry(NodeType + " OK");
                            break;
                        }
                    case NodeActionEnum.Add:
                        {
                            Results.WriteOutputEntry(NodeType, String.Empty, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            Results.WriteOutputEntry(NodeType, String.Empty, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return;
        }

        private int ProcessResetPassword(XmlNode node)
        {
            String NodeType = node.Name;
            int CustomerID = XmlCommon.XmlAttributeNativeInt(node, "CustomerID");
            String CustomerGUID = XmlCommon.XmlAttribute(node, "CustomerGUID");
            String EMail = XmlCommon.XmlAttribute(node, "CustomerEMail");
            String PasswordType = XmlCommon.XmlAttribute(node, "PasswordType");
            bool SendEMailToCustomer = XmlCommon.XmlAttributeBool(node, "SendEMailToCustomer");

            Results.WriteVerboseEntry("Processing " + NodeType + ", ID=" + CustomerID.ToString() + ", GUID=" + CustomerGUID + ", EMail=" + EMail + ", PasswordType=" + PasswordType);

            CustomerID = SetupTableID("Customer", CustomerID, CustomerGUID);
            if (CustomerID == 0 && EMail.Length != 0)
            {
                SqlConnection con = null;
                IDataReader rs = null;
                try
                {
                    string query = "select CustomerID,CustomerGUID from Customer  with (NOLOCK)  where deleted=0 and lower(EMail)=" + DB.SQuote(EMail.ToLowerInvariant());
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        if (rs.Read())
                        {
                            CustomerID = DB.RSFieldInt(rs, "CustomerID");
                            CustomerGUID = DB.RSFieldGUID(rs, "CustomerGUID");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs = null;
                    con = null;
                }

            }

            if (CustomerID == 0)
            {
                throw new ArgumentException("Invalid Customer ID, GUID, or EMail specified! Password NOT changed");
            }
            Customer TargetCustomer = new Customer(m_DBTrans, CustomerID, true);

            if (TargetCustomer.IsAdminUser || TargetCustomer.IsAdminSuperUser)
            {
                throw new ArgumentException("Permission Denied");
            }

            try
            {
                Password p = null;
                String NewPWD = node.InnerText;
                if (NewPWD.Length == 0 || PasswordType == "Random")
                {
                    // reset to new random pwd!
                    p = new RandomPassword();
                }
                else
                {
                    p = new Password(NewPWD);
                }
                String sql = String.Format("update Customer set Password={0}, SaltKey={1}, BadLoginCount=0, LockedUntil=NULL, PwdChangeRequired=0 where CustomerID={2}", DB.SQuote(p.SaltedPassword), p.Salt.ToString(), TargetCustomer.CustomerID.ToString());
                RunCommand(sql);

                if (SendEMailToCustomer)
                {
                    AppLogic.SendMail(AppLogic.AppConfig("StoreName") + " - " + AppLogic.GetString("cst_account_process.aspx.1", TargetCustomer.SkinID, TargetCustomer.LocaleSetting), AppLogic.RunXmlPackage("notification.lostpassword.xml.config", null, TargetCustomer, TargetCustomer.SkinID, "", "thiscustomerid=" + TargetCustomer.CustomerID.ToString() + "&newpwd=" + p.ClearPassword, false, false), true, AppLogic.AppConfig("MailMe_FromAddress"), AppLogic.AppConfig("MailMe_FromName"), TargetCustomer.EMail, TargetCustomer.FullName(), "", "", AppLogic.MailServer());
                }
                Security.LogEvent("Admin Reset Customer Password", "", TargetCustomer.CustomerID, m_ThisCustomer.CustomerID, Convert.ToInt32(m_ThisCustomer.CurrentSessionID));

                Results.WriteOutputEntry(NodeType, String.Empty, CustomerGUID, CustomerID, NodeActionEnum.Update, "OK", String.Empty);
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return CustomerID;
        }

        private void ProcessOrderManagement(XmlNode node)
        {
            String NodeType = node.Name;
            int OrderNumber = XmlCommon.XmlAttributeUSInt(node, "OrderNumber");
            OrderManagementActionEnum NodeAction = OrderManagementActionEnum.Unknown;
            if (XmlCommon.XmlAttribute(node, "Action").Trim().Length != 0)
            {
                NodeAction = (OrderManagementActionEnum)Enum.Parse(typeof(OrderManagementActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", OrderNumber=" + OrderNumber + ", Action=" + NodeAction);

            Order ord = new Order(OrderNumber, Localization.GetDefaultLocale());

            if (ord.OrderNumber == 0 || ord.IsEmpty)
            {
                Results.WriteXml("<OrderManagement OrderNumber=\"" + OrderNumber.ToString() + "\" Action=\"" + NodeAction.ToString() + "\" Status=\"ERROR: " + CommonLogic.IIF(XmlCommon.XmlAttribute(node, "OrderNumber").Length == 0, "No OrderNumber Specified", "Order Not Found") + "\" />");
                return;
            }

            try
            {
                Results.WriteXml("<OrderManagement OrderNumber=\"" + OrderNumber.ToString() + "\" Action=\"" + NodeAction.ToString() + "\" ");
                String Status = AppLogic.ro_OK;
                switch (NodeAction)
                {
                    case OrderManagementActionEnum.Unknown:
                        Results.WriteXml(" Status=\"ERROR: No Action Specified\">");
                        break;
                    case OrderManagementActionEnum.Void:
                        Status = Gateway.OrderManagement_DoVoid(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.Capture:
                        Status = Gateway.OrderManagement_DoCapture(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.FullRefund:
                        String RefundReason = XmlCommon.XmlAttribute(node, "RefundReason");
                        Status = Gateway.OrderManagement_DoFullRefund(ord, m_ThisCustomer.LocaleSetting, RefundReason);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.ForceFullRefund:
                        Status = Gateway.OrderManagement_DoForceFullRefund(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.MarkAsFraud:
                        Status = Gateway.OrderManagement_MarkAsFraud(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.SendReceipt:
                        Status = Gateway.OrderManagement_SendReceipt(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.ClearFraud:
                        Status = Gateway.OrderManagement_ClearFraud(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.BlockIP:
                        Status = Gateway.OrderManagement_BlockIP(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.AllowIP:
                        Status = Gateway.OrderManagement_AllowIP(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.SendDistributorNotification:
                        Status = Gateway.OrderManagement_SendDistributorNotification(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.ChangeOrderEMail:
                        Status = Gateway.OrderManagement_ChangeOrderEMail(ord, m_ThisCustomer.LocaleSetting, XmlCommon.XmlAttribute(node, "NewEMail"));
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.MarkAsReadyToShip:
                        Status = Gateway.OrderManagement_MarkAsReadyToShip(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.ClearReadyToShip:
                        Status = Gateway.OrderManagement_ClearReadyToShip(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.ClearNewStatus:
                        Status = Gateway.OrderManagement_ClearNewStatus(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.MarkAsShipped:
                        DateTime ShippedOn = System.DateTime.Now;
                        if (XmlCommon.XmlAttribute(node, "ShippedOn").Length != 0)
                        {
                            ShippedOn = XmlCommon.XmlAttributeNativeDateTime(node, "ShippedOn");
                        }
                        if (ShippedOn.Equals(System.DateTime.MinValue))
                        {
                            ShippedOn = System.DateTime.Now;
                        }
                        Status = Gateway.OrderManagement_MarkAsShipped(ord, m_ThisCustomer.LocaleSetting, XmlCommon.XmlAttribute(node, "ShippedCarrier"), XmlCommon.XmlAttribute(node, "TrackingNumber"), ShippedOn);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.SetTracking:
                        Status = Gateway.OrderManagement_SetTracking(ord, m_ThisCustomer.LocaleSetting, XmlCommon.XmlAttribute(node, "ShippedCarrier"), XmlCommon.XmlAttribute(node, "TrackingNumber"));
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.SetPrivateNotes:
                        Status = Gateway.OrderManagement_SetPrivateNotes(ord, m_ThisCustomer.LocaleSetting, XmlCommon.XmlAttribute(node, "Notes"));
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.SetCustomerServiceNotes:
                        Status = Gateway.OrderManagement_SetCustomerServiceNotes(ord, m_ThisCustomer.LocaleSetting, XmlCommon.XmlAttribute(node, "Notes"));
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.SendToShipRush:
                        Status = Gateway.OrderManagement_SendToShipRush(ord, m_ThisCustomer.LocaleSetting, m_ThisCustomer);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.SetOrderWeight:
                        Decimal NewWeight = XmlCommon.XmlAttributeNativeDecimal(node, "Weight");
                        Status = Gateway.OrderManagement_SetOrderWeight(ord, m_ThisCustomer.LocaleSetting, NewWeight);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.SendToFedexShippingMgr:
                        Status = Gateway.OrderManagement_SendToFedexShippingMgr(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.MarkAsPrinted:
                        Status = Gateway.OrderManagement_MarkAsPrinted(ord, m_ThisCustomer.LocaleSetting);
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case OrderManagementActionEnum.GetReceipt:
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        Results.WriteXml("<![CDATA[");
                        Results.WriteXml(ord.Receipt(m_ThisCustomer, false));
                        Results.WriteXml("]]>"); break;
                    case OrderManagementActionEnum.GetDistributorNotifications:

                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                }
                Results.WriteXml("</OrderManagement>");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private void ProcessRecurringAutoBillOrderManagement(XmlNode node)
        {
            String NodeType = node.Name;
            int OriginalRecurringOrderNumber = XmlCommon.XmlAttributeUSInt(node, "OriginalRecurringOrderNumber");
            String RecurringSubscriptionID = XmlCommon.XmlAttribute(node, "RecurringSubscriptionID");
            RecurringOrderManagementActionEnum NodeAction = RecurringOrderManagementActionEnum.Unknown;
            if (XmlCommon.XmlAttribute(node, "Action").Trim().Length != 0)
            {
                NodeAction = (RecurringOrderManagementActionEnum)Enum.Parse(typeof(RecurringOrderManagementActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", OriginalRecurringOrderNumber=" + OriginalRecurringOrderNumber + ", RecurringSubscriptionID=" + RecurringSubscriptionID + ", Action=" + NodeAction);

            if (OriginalRecurringOrderNumber == 0)
            {
                Results.WriteXml("<RecurringAutoBillOrderManagement OriginalRecurringOrderNumber=\"" + OriginalRecurringOrderNumber.ToString() + "\" Action=\"" + NodeAction.ToString() + "\" Status=\"ERROR: " + CommonLogic.IIF(XmlCommon.XmlAttribute(node, "OrderNumber").Length == 0, "No OrderNumber Specified", "Order Not Found") + "\" />");
                return;
            }

            try
            {
                Results.WriteXml("<RecurringAutoBillOrderManagement OriginalRecurringOrderNumber=\"" + OriginalRecurringOrderNumber.ToString() + "\" Action=\"" + NodeAction.ToString() + "\" ");
                String Status = AppLogic.ro_OK;
                RecurringOrderMgr rmgr = new RecurringOrderMgr(AppLogic.MakeEntityHelpers(), null);
                switch (NodeAction)
                {
                    case RecurringOrderManagementActionEnum.Unknown:
                        Results.WriteXml(" Status=\"ERROR: No Action Specified\">");
                        break;
                    case RecurringOrderManagementActionEnum.Approved:
                        int NewOrderNumber = 0;
                        Status = rmgr.ProcessAutoBillApproved(OriginalRecurringOrderNumber, String.Empty, DateTime.MinValue, out NewOrderNumber); // TBD include Transaction ID and Time if available
                        Results.WriteXml(String.Format(" Status=\"{0}\">", XmlCommon.XmlEncodeAttribute(Status)));
                        break;
                    case RecurringOrderManagementActionEnum.Declined:
                        String DeclinedReason = XmlCommon.XmlAttribute(node, "DeclinedReason");
                        Status = rmgr.ProcessAutoBillDeclined(OriginalRecurringOrderNumber, String.Empty, DateTime.MinValue, RecurringSubscriptionID, DeclinedReason);
                        break;
                    case RecurringOrderManagementActionEnum.Cancel:
                        Status = rmgr.ProcessAutoBillCancel(OriginalRecurringOrderNumber);
                        break;
                    case RecurringOrderManagementActionEnum.FullRefund:
                        Status = rmgr.ProcessAutoBillFullRefund(OriginalRecurringOrderNumber);
                        break;
                    case RecurringOrderManagementActionEnum.UpdateBilling:
                        Order ord = new Order(OriginalRecurringOrderNumber, Localization.GetDefaultLocale());
                        Address NewBillingAddress = new Address();
                        NewBillingAddress.LoadByCustomer(ord.CustomerID, AddressTypes.Billing);
                        NewBillingAddress.FirstName = XmlCommon.XmlField(node, "FirstName");
                        NewBillingAddress.LastName = XmlCommon.XmlField(node, "LastName");
                        NewBillingAddress.Address1 = XmlCommon.XmlField(node, "Address1");
                        NewBillingAddress.Address2 = XmlCommon.XmlField(node, "Address2");
                        NewBillingAddress.Suite = XmlCommon.XmlField(node, "Suite");
                        NewBillingAddress.City = XmlCommon.XmlField(node, "City");
                        NewBillingAddress.State = XmlCommon.XmlField(node, "StateOrProvince");
                        if (NewBillingAddress.State.Length == 0)
                        {
                            NewBillingAddress.State = "--";
                        }
                        NewBillingAddress.Zip = XmlCommon.XmlField(node, "ZipCode");
                        NewBillingAddress.Country = XmlCommon.XmlField(node, "Country");
                        NewBillingAddress.CardName = XmlCommon.XmlField(node, "NameOnCard");
                        NewBillingAddress.CardType = XmlCommon.XmlField(node, "CardType");
                        NewBillingAddress.CardNumber = XmlCommon.XmlField(node, "CardNumber");
                        NewBillingAddress.CardExpirationMonth = XmlCommon.XmlField(node, "CardExpirationMonth");
                        NewBillingAddress.CardExpirationYear = XmlCommon.XmlField(node, "CardExpirationYear");
                        Status = rmgr.ProcessAutoBillAddressUpdate(OriginalRecurringOrderNumber, NewBillingAddress);
                        NewBillingAddress.ClearCCInfo();
                        NewBillingAddress.UpdateDB();
                        break;
                }
                Results.WriteXml("</RecurringAutoBillOrderManagement>");
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
        }

        private int ProcessCustomer(XmlNode node, Nullable<NodeActionEnum> ForceAction)
        {
            String NodeType = node.Name;
            String CustomerName = XmlCommon.XmlField(node, "Name");
            String CustomerSKU = XmlCommon.XmlField(node, "SKU");
            NodeActionEnum NodeAction = NodeActionEnum.Unknown;
            if (XmlCommon.XmlAttribute(node, "Action").Trim().Length != 0)
            {
                NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            }
            String CustomerGUID = XmlCommon.XmlAttribute(node, "GUID");
            String EMail = XmlCommon.XmlAttribute(node, "EMail");
            int CustomerID = XmlCommon.XmlAttributeNativeInt(node, "ID");
            if (ForceAction != null && ForceAction != NodeActionEnum.Unknown)
            {
                NodeAction = (NodeActionEnum)ForceAction;
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + CustomerName + ", Action=" + NodeAction + ", ID=" + CustomerID.ToString() + ", GUID=" + CustomerGUID + ", EMail=" + EMail);

            CustomerID = SetupTableID("Customer", CustomerID, CustomerGUID);
            if (CustomerID == 0 && EMail.Length != 0)
            {
                SqlConnection con = null;
                IDataReader rs = null;
                try
                {
                    string query = "select CustomerID,CustomerGUID from Customer  with (NOLOCK)  where Deleted=0 and lower(EMail)=" + DB.SQuote(EMail.ToLowerInvariant());
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        if (rs.Read())
                        {
                            CustomerID = DB.RSFieldInt(rs, "CustomerID");
                            CustomerGUID = DB.RSFieldGUID(rs, "CustomerGUID");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs = null;
                    con = null;
                }


            }

            Customer c = null;
            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Add:
                        {
                            if (CustomerID != 0)
                            {
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "ERROR", "Duplicate Customer, skipping...");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Duplicate Customer");
                                }
                            }
                            else
                            {
                                if (CustomerGUID.Length == 0)
                                {
                                    CustomerGUID = CommonLogic.GetNewGUID();
                                }
                                if (XmlCommon.XmlField(node, "Password").Trim().Length == 0)
                                {
                                    throw new ArgumentException("Password is required to add a customer record");
                                }
                                StringBuilder sql = new StringBuilder(1024);
                                sql.Append("insert Customer(CustomerGUID,LocaleSetting,CurrencySetting,EMail) values(");
                                sql.Append(DB.SQuote(CustomerGUID) + ",");
                                sql.Append(DB.SQuote(Localization.GetDefaultLocale()) + ",");
                                sql.Append(DB.SQuote(Currency.GetDefaultCurrency()) + ",");
                                sql.Append(DB.SQuote(EMail) + ")");
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                CustomerID = LookupTableRecordByGUID("Customer", CustomerGUID);
                                hlpCustomerUpdate(CustomerID, node, true);
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Update:
                        {
                            if (CustomerID != 0)
                            {
                                c = new Customer(m_DBTrans, CustomerID, true);
                                if (c.IsAdminUser || c.IsAdminSuperUser)
                                {
                                    throw new ArgumentException("Permission Denied");
                                }
                                hlpCustomerUpdate(CustomerID, node, false);
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                if (m_AutoLazyAdd)
                                {
                                    Results.WriteVerboseEntry(NodeType + " LazyAdd being performed...");
                                    NodeAction = NodeActionEnum.Add;
                                    if (CustomerGUID.Length == 0)
                                    {
                                        CustomerGUID = CommonLogic.GetNewGUID();
                                    }
                                    if (XmlCommon.XmlField(node, "Password").Trim().Length == 0)
                                    {
                                        throw new ArgumentException("Password is required to add a customer record");
                                    }
                                    StringBuilder sql = new StringBuilder(1024);
                                    sql.Append("insert Customer(CustomerGUID,LocaleSetting,CurrencySetting,EMail) values(");
                                    sql.Append(DB.SQuote(CustomerGUID) + ",");
                                    sql.Append(DB.SQuote(Localization.GetDefaultLocale()) + ",");
                                    sql.Append(DB.SQuote(Currency.GetDefaultCurrency()) + ",");
                                    sql.Append(DB.SQuote(EMail) + ")");
                                    RunCommand(sql.ToString());
                                    Results.WriteVerboseEntry(NodeType + " OK");
                                    CustomerID = LookupTableRecordByGUID("Customer", CustomerGUID);
                                    hlpCustomerUpdate(CustomerID, node);
                                    Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                                }
                                else
                                {
                                    Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "ERROR", "No ID or GUID Specified");
                                    if (m_DBTrans != null)
                                    {
                                        throw new ArgumentException("No ID, GUID or EMail Specified");
                                    }
                                }
                            }
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            if (CustomerID == 0)
                            {
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "ERROR", "A valid ID, GUID or EMail must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A valid ID, GUID or EMail must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                c = new Customer(m_DBTrans, CustomerID, true);
                                if (c.IsAdminUser || c.IsAdminSuperUser)
                                {
                                    throw new ArgumentException("Permission Denied");
                                }
                                String sql = "update Customer set Deleted=1 where CustomerID=" + CustomerID.ToString();
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Nuke:
                        {
                            if (CustomerID == 0)
                            {
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A valid ID, GUID or EMail must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                c = new Customer(m_DBTrans, CustomerID, true);
                                if (c.IsAdminUser || c.IsAdminSuperUser)
                                {
                                    throw new ArgumentException("Permission Denied");
                                }
                                AppLogic.NukeCustomer(CustomerID, false);
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Lookup:
                        {
                            if (CustomerID != 0)
                            {
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                c = new Customer(m_DBTrans, CustomerID, true);
                                if (c.IsAdminUser || c.IsAdminSuperUser)
                                {
                                    throw new ArgumentException("Permission Denied");
                                }
                                Results.WriteOutputEntry(NodeType, CustomerName, CustomerGUID, CustomerID, NodeAction, "ERROR", "Not Found");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Not Found");
                                }
                            }
                            break;
                        }
                }
                if (CustomerID != 0)
                {
                    if (c == null)
                    {
                        c = new Customer(m_DBTrans, CustomerID, true);
                    }

                    // process addresses, if any:
                    XmlNode vv = node.SelectSingleNode("Addresses");
                    if (vv != null)
                    {
                        StringBuilder AddressList = new StringBuilder(128);
                        String Separator = String.Empty;
                        XmlNodeList NodeList = vv.SelectNodes("Address");
                        foreach (XmlNode n in NodeList)
                        {
                            AddressList.Append(Separator);
                            int AdxID = ProcessAddress(n, CustomerID, ForceAction);
                            AddressList.Append(AdxID);
                            Separator = ",";
                        }

                        // do autoclean of Addresses if specified
                        bool AutoCleanupAddresses = XmlCommon.XmlAttributeBool(vv, "AutoCleanup");
                        if (AutoCleanupAddresses && AddressList.Length != 0)
                        {
                            String sql = "delete from Address where CustomerID=" + CustomerID.ToString() + " and AddressID not in (" + AddressList.ToString() + ")"; //Cannot concat types String and StringBuilder in VB
                            RunCommand(sql);
                        }
                    }

                    int AN = DB.GetSqlN("select count(AddressID) as N from Address  with (NOLOCK)  where CustomerID=" + c.CustomerID.ToString(), m_DBTrans);
                    int OnlyAddressID = 0;
                    if (AN == 1)
                    {
                        SqlConnection con = null;
                        IDataReader rs = null;
                        try
                        {
                            string query = "Select AddressID from Address  with (NOLOCK)  where CustomerID=" + c.CustomerID.ToString();
                            if (m_DBTrans != null)
                            {
                                // if a transaction was passed, we should use the transaction objects connection
                                rs = DB.GetRS(query, m_DBTrans);
                            }
                            else
                            {
                                // otherwise create it
                                con = new SqlConnection(DB.GetDBConn());
                                con.Open();
                                rs = DB.GetRS(query, con);
                            }

                            using (rs)
                            {
                                if (rs.Read())
                                {
                                    OnlyAddressID = DB.RSFieldInt(rs, "AddressID");
                                }
                            }
                        }
                        catch { throw; }
                        finally
                        {
                            // we can't dispose of the connection if it's part of a transaction
                            if (con != null && m_DBTrans == null)
                            {
                                // here it's safe to dispose since we created the connection ourself
                                con.Dispose();
                            }

                            // make sure we won't reference this again in code
                            rs = null;
                            con = null;
                        }
                    }

                    // process primary shipping & billing settings here now that all customer & address records are available:
                    bool AllowShipToDifferentThanBillTo = AppLogic.AppConfigBool("AllowShipToDifferentThanBillTo") && !AppLogic.AppConfigBool("SkipShippingOnCheckout");
                    StringBuilder sqlC = new StringBuilder(1024);
                    String SeparatorC = String.Empty;
                    sqlC.Append("update Customer set ");
                    int BillingAddressID = c.PrimaryBillingAddressID;
                    int ShippingAddressID = c.PrimaryShippingAddressID;
                    XmlNode ptn = node.SelectSingleNode("BillingAddress");
                    if (ptn != null)
                    {
                        BillingAddressID = SetupTableID("Address", XmlCommon.XmlAttributeNativeInt(ptn, "ID"), XmlCommon.XmlAttribute(ptn, "GUID"));
                    }
                    else
                    {
                        BillingAddressID = OnlyAddressID;
                    }
                    sqlC.Append(SeparatorC);
                    sqlC.Append("BillingAddressID=");
                    sqlC.Append(BillingAddressID.ToString());
                    SeparatorC = ",";

                    ptn = node.SelectSingleNode("ShippingAddress");
                    if (ptn != null)
                    {
                        ShippingAddressID = SetupTableID("Address", XmlCommon.XmlAttributeNativeInt(ptn, "ID"), XmlCommon.XmlAttribute(ptn, "GUID"));
                    }
                    else
                    {
                        ShippingAddressID = OnlyAddressID;
                    }
                    sqlC.Append(SeparatorC);
                    sqlC.Append("ShippingAddressID=");
                    sqlC.Append(ShippingAddressID.ToString());
                    SeparatorC = ",";

                    if (!AllowShipToDifferentThanBillTo && BillingAddressID != ShippingAddressID)
                    {
                        Results.WriteErrorEntry("Store settings require that ShippingAddressID and BillingAddressID must be the same!");
                        if (m_DBTrans == null)
                        {
                            throw new ArgumentException("Store settings require that ShippingAddressID and BillingAddressID must be the same!");
                        }
                    }
                    sqlC.Append(" where CustomerID=" + CustomerID.ToString());
                    RunCommand(sqlC.ToString());
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return CustomerID;
        }

        private int ProcessAddress(XmlNode node, int CustomerID, Nullable<NodeActionEnum> ForceAction)
        {
            String NodeType = node.Name;
            NodeActionEnum NodeAction = NodeActionEnum.Unknown;
            if (XmlCommon.XmlAttribute(node, "Action").Trim().Length != 0)
            {
                NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            }
            int AddressID = XmlCommon.XmlAttributeNativeInt(node, "ID");
            String AddressGUID = XmlCommon.XmlAttribute(node, "GUID");
            if (ForceAction != null && ForceAction != NodeActionEnum.Unknown)
            {
                NodeAction = (NodeActionEnum)ForceAction;
            }

            if (CustomerID == 0)
            {
                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an Update operation");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                }
                return 0;
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", Action=" + NodeAction + ", ID=" + AddressID.ToString() + ", GUID=" + AddressGUID);

            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Add:
                        {
                            AddressID = SetupTableID("Address", AddressID, AddressGUID);
                            if (AddressID != 0)
                            {
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "ERROR", "Duplicate Address, skipping...");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Duplicate Address");
                                }
                            }
                            else
                            {
                                if (AddressGUID.Length == 0)
                                {
                                    AddressGUID = CommonLogic.GetNewGUID();
                                }
                                StringBuilder sql = new StringBuilder(1024);
                                sql.Append("insert Address(AddressGUID,CustomerID) values(");
                                sql.Append(DB.SQuote(AddressGUID) + ",");
                                sql.Append(CustomerID.ToString() + ")");
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                AddressID = LookupTableRecordByGUID("Address", AddressGUID);
                                hlpAddressUpdate(AddressID, node);
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Update:
                        {
                            AddressID = SetupTableID("Address", AddressID, AddressGUID);
                            if (AddressID != 0)
                            {
                                hlpAddressUpdate(AddressID, node);
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                if (m_AutoLazyAdd)
                                {
                                    Results.WriteVerboseEntry(NodeType + " LazyAdd being performed...");
                                    NodeAction = NodeActionEnum.Add;
                                    if (AddressGUID.Length == 0)
                                    {
                                        AddressGUID = CommonLogic.GetNewGUID();
                                    }
                                    StringBuilder sql = new StringBuilder(1024);
                                    sql.Append("insert Address(AddressGUID,CustomerID) values(");
                                    sql.Append(DB.SQuote(AddressGUID) + ",");
                                    sql.Append(CustomerID.ToString() + ")");
                                    RunCommand(sql.ToString());
                                    Results.WriteVerboseEntry(NodeType + " OK");
                                    AddressID = LookupTableRecordByGUID("Address", AddressGUID);
                                    hlpAddressUpdate(AddressID, node);
                                    Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "OK", String.Empty);
                                }
                                else
                                {
                                    Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "ERROR", "No ID or GUID Specified");
                                    if (m_DBTrans != null)
                                    {
                                        throw new ArgumentException("No ID or GUID Specified");
                                    }
                                }
                            }
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            AddressID = SetupTableID("Address", AddressID, AddressGUID);
                            if (AddressID == 0 && AddressGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                RunCommand("update Address set Deleted=1 where AddressID=" + AddressID.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Nuke:
                        {
                            AddressID = SetupTableID("Address", AddressID, AddressGUID);
                            if (AddressID == 0 && AddressGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                RunCommand("delete from Address where AddressID=" + AddressID.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Lookup:
                        {
                            AddressID = SetupTableID("Address", AddressID, AddressGUID);
                            if (AddressID != 0)
                            {
                                String sql = "select * from Address  with (NOLOCK)  where AddressID=" + AddressID.ToString();

                                SqlConnection con = null;
                                IDataReader rs = null;
                                try
                                {
                                    string query = sql;
                                    if (m_DBTrans != null)
                                    {
                                        // if a transaction was passed, we should use the transaction objects connection
                                        rs = DB.GetRS(query, m_DBTrans);
                                    }
                                    else
                                    {
                                        // otherwise create it
                                        con = new SqlConnection(DB.GetDBConn());
                                        con.Open();
                                        rs = DB.GetRS(query, con);
                                    }

                                    using (rs)
                                    {
                                        if (rs.Read())
                                        {
                                            AddressGUID = DB.RSFieldGUID(rs, "AddressGUID");
                                        }
                                    }
                                }
                                catch { throw; }
                                finally
                                {
                                    // we can't dispose of the connection if it's part of a transaction
                                    if (con != null && m_DBTrans == null)
                                    {
                                        // here it's safe to dispose since we created the connection ourself
                                        con.Dispose();
                                    }

                                    // make sure we won't reference this again in code
                                    rs = null;
                                    con = null;
                                }

                            }
                            if (AddressID != 0)
                            {
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                Results.WriteOutputEntry(NodeType, String.Empty, AddressGUID, AddressID, NodeAction, "ERROR", "Not Found");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Not Found");
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return AddressID;
        }

        private int ProcessVariant(XmlNode node, int ProductID)
        {
            String NodeType = node.Name;
            String VariantName = XmlCommon.XmlField(node, "Name");
            String SKUSuffix = XmlCommon.XmlField(node, "SKUSuffix");
            NodeActionEnum NodeAction = (NodeActionEnum)Enum.Parse(typeof(NodeActionEnum), XmlCommon.XmlAttribute(node, "Action"));
            String VariantGUID = XmlCommon.XmlAttribute(node, "GUID");
            int VariantID = XmlCommon.XmlAttributeNativeInt(node, "ID");

            if (ProductID == 0)
            {
                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an Update operation");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                }
                return 0;
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + VariantName + ", Action=" + NodeAction + ", ID=" + VariantID.ToString() + ", GUID=" + VariantGUID);

            try
            {
                switch (NodeAction)
                {
                    case NodeActionEnum.Add:
                        {
                            VariantID = SetupTableID("ProductVariant", VariantID, VariantGUID);
                            if (VariantID != 0)
                            {
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "ERROR", "Duplicate Variant, skipping...");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Duplicate Variant");
                                }
                            }
                            else
                            {
                                if (VariantGUID.Length == 0)
                                {
                                    VariantGUID = CommonLogic.GetNewGUID();
                                }
                                StringBuilder sql = new StringBuilder(1024);
                                sql.Append("insert ProductVariant(VariantGUID,ProductID,Name,SEName) values(");
                                sql.Append(DB.SQuote(VariantGUID) + ",");
                                sql.Append(ProductID.ToString() + ",");
                                sql.Append(DB.SQuote(VariantName) + ",");
                                sql.Append(DB.SQuote(SE.MungeName(VariantName)) + ")");
                                RunCommand(sql.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                VariantID = LookupTableRecordByGUID("ProductVariant", VariantGUID);
                                hlpVariantUpdate(ProductID, VariantID, node);
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Update:
                        {
                            VariantID = SetupVariantID(ProductID, VariantID, VariantGUID, VariantName, SKUSuffix);
                            if (VariantID != 0)
                            {
                                hlpVariantUpdate(ProductID, VariantID, node);
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                if (m_AutoLazyAdd)
                                {
                                    Results.WriteVerboseEntry(NodeType + " LazyAdd being performed...");
                                    NodeAction = NodeActionEnum.Add;
                                    if (VariantGUID.Length == 0)
                                    {
                                        VariantGUID = CommonLogic.GetNewGUID();
                                    }
                                    StringBuilder sql = new StringBuilder(1024);
                                    sql.Append("insert ProductVariant(VariantGUID,ProductID,Name,SEName) values(");
                                    sql.Append(DB.SQuote(VariantGUID) + ",");
                                    sql.Append(ProductID.ToString() + ",");
                                    sql.Append(DB.SQuote(VariantName) + ",");
                                    sql.Append(DB.SQuote(SE.MungeName(VariantName)) + ")");
                                    RunCommand(sql.ToString());
                                    Results.WriteVerboseEntry(NodeType + " OK");
                                    VariantID = LookupTableRecordByGUID("ProductVariant", VariantGUID);
                                    hlpVariantUpdate(ProductID, VariantID, node);
                                    Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "OK", String.Empty);
                                }
                                else
                                {
                                    Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "ERROR", "No ID or GUID Specified");
                                    if (m_DBTrans != null)
                                    {
                                        throw new ArgumentException("No ID or GUID Specified");
                                    }
                                }
                            }
                            break;
                        }
                    case NodeActionEnum.Delete:
                        {
                            VariantID = SetupTableID("ProductVariant", VariantID, VariantGUID);
                            if (VariantID == 0 && VariantGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                RunCommand("update ProductVariant set Deleted=1 where VariantID=" + VariantID.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Nuke:
                        {
                            VariantID = SetupTableID("ProductVariant", VariantID, VariantGUID);
                            if (VariantID == 0 && VariantGUID.Length == 0)
                            {
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "ERROR", "A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("A Valid ID or GUID must be specified for an " + NodeAction.ToString() + " operation");
                                }
                            }
                            else
                            {
                                RunCommand("delete from ExtendedPrice where VariantID=" + VariantID.ToString());
                                RunCommand("delete from ProductVariant where VariantID=" + VariantID.ToString());
                                RunCommand("delete from KitCart where VariantID=" + VariantID.ToString());
                                RunCommand("delete from ShoppingCart where VariantID=" + VariantID.ToString());
                                RunCommand("delete from ShippingByProduct where VariantID=" + VariantID.ToString());
                                RunCommand("delete from Inventory where VariantID=" + VariantID.ToString());
                                RunCommand("delete from KitItem where InventoryVariantID=" + VariantID.ToString());
                                Results.WriteVerboseEntry(NodeType + " OK");
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "OK", String.Empty);
                            }
                            break;
                        }
                    case NodeActionEnum.Lookup:
                        {
                            VariantID = SetupVariantID(ProductID, VariantID, VariantGUID, VariantName, SKUSuffix);
                            if (VariantID != 0)
                            {
                                String sql = "select * from ProductVariant  with (NOLOCK)  where Deleted=0 and VariantID=" + VariantID.ToString();

                                SqlConnection con = null;
                                IDataReader rs = null;
                                try
                                {
                                    string query = sql;
                                    if (m_DBTrans != null)
                                    {
                                        // if a transaction was passed, we should use the transaction objects connection
                                        rs = DB.GetRS(query, m_DBTrans);
                                    }
                                    else
                                    {
                                        // otherwise create it
                                        con = new SqlConnection(DB.GetDBConn());
                                        con.Open();
                                        rs = DB.GetRS(query, con);
                                    }

                                    using (rs)
                                    {
                                        if (rs.Read())
                                        {
                                            VariantName = DB.RSField(rs, "Name");
                                            VariantGUID = DB.RSFieldGUID(rs, "VariantGUID");
                                        }
                                    }
                                }
                                catch { throw; }
                                finally
                                {
                                    // we can't dispose of the connection if it's part of a transaction
                                    if (con != null && m_DBTrans == null)
                                    {
                                        // here it's safe to dispose since we created the connection ourself
                                        con.Dispose();
                                    }

                                    // make sure we won't reference this again in code
                                    rs = null;
                                    con = null;
                                }

                            }
                            if (VariantID != 0 && VariantName.Length != 0)
                            {
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "OK", String.Empty);
                            }
                            else
                            {
                                Results.WriteOutputEntry(NodeType, VariantName, VariantGUID, VariantID, NodeAction, "ERROR", "Not Found");
                                if (m_DBTrans != null)
                                {
                                    throw new ArgumentException("Not Found");
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Results.WriteErrorEntry("Exception, Message=" + ex.Message);
                if (m_DBTrans != null)
                {
                    throw (ex);
                }
            }
            return VariantID;
        }

        private int LoadKitGroup(XmlNode node, int ProductID)
        {
            String NodeType = node.Name;
            String KitGrouThisRowName = XmlCommon.XmlField(node, "Name");
            int KitGroupTypeID = XmlCommon.XmlFieldUSInt(node, "KitGroupTypeID");

            if (ProductID == 0 || KitGrouThisRowName.Length == 0 || KitGroupTypeID == 0)
            {
                Results.WriteErrorEntry("ERROR", "ProductID, KitGroup Name, and KitGroup TypeID must be specified for a KitGroup node");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("ProductID, KitGroup Name, and KitGroup TypeID must be specified for a KitGroup node");
                }
                return 0;
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", Name=" + KitGrouThisRowName + ", KitGroupTypeID=" + KitGroupTypeID.ToString());

            StringBuilder sql = new StringBuilder(1024);

            int KitGroupID = CheckForKitGroup(KitGrouThisRowName, ProductID);
            if (KitGroupID == 0)
            {
                // add the KitGroup:
                Results.WriteVerboseEntry("Adding KitGroup(" + KitGrouThisRowName + ", ProductID=" + ProductID.ToString() + ")");
                String NewGUID = CommonLogic.GetNewGUID();
                sql.Append("insert KitGroup(KitGroupGUID,Name,Description,ProductID,KitGroupTypeID,IsRequired,DisplayOrder,IsReadOnly) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(KitGrouThisRowName) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append(DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("NULL,");
                }
                sql.Append(ProductID.ToString() + ",");
                sql.Append(KitGroupTypeID.ToString() + ",");
                sql.Append(CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsRequired"), "1", "0") + ",");
                sql.Append((XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0 ? 1 : XmlCommon.XmlFieldUSInt(node, "DisplayOrder")) + ",");
				sql.Append(CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsReadOnly"), "1", "0"));
                sql.Append(")");
                RunCommand(sql.ToString());

                SqlConnection con = null;
                IDataReader rs = null;
                try
                {
                    string query = "select KitGroupID from KitGroup  with (NOLOCK)  where KitGroupGUID=" + DB.SQuote(NewGUID);
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con = new SqlConnection(DB.GetDBConn());
                        con.Open();
                        rs = DB.GetRS(query, con);
                    }

                    using (rs)
                    {
                        rs.Read();
                        KitGroupID = DB.RSFieldInt(rs, "KitGroupID");
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs = null;
                    con = null;
                }

                Results.WriteVerboseEntry("KitGroup Added OK");
            }
            else
            {
                // update the KitGroup
                Results.WriteVerboseEntry("Updating KitGroup(" + KitGrouThisRowName + ", ProductID=" + ProductID.ToString() + ")");

                sql.Append("update KitGroup set ");
                sql.Append("Name=" + DB.SQuote(KitGrouThisRowName) + ",");
                sql.Append("DisplayOrder=" + (XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0 ? 1 : XmlCommon.XmlFieldUSInt(node, "DisplayOrder")) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append("Description=" + DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("Description=NULL,");
                }
                sql.Append("ProductID=" + ProductID.ToString() + ",");
                sql.Append("KitGroupTypeID=" + KitGroupTypeID.ToString() + ",");
				sql.Append("IsRequired=" + CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsRequired"), "1", "0") + ",");
				sql.Append("IsReadOnly=" + CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsReadOnly"), "1", "0"));
				sql.Append(" where KitGroupID=" + KitGroupID.ToString());
                RunCommand(sql.ToString());
                Results.WriteVerboseEntry("KitGroup Updated OK");
            }

            // add/update Kit Items:
            XmlNodeList KitItemList = node.SelectNodes("KitItem");
            foreach (XmlNode KitItemNode in KitItemList)
            {
                LoadKitItem(KitItemNode, KitGrouThisRowName, KitGroupID);
            }
            return KitGroupID;
        }

        private void LoadKitItem(XmlNode node, String KitGrouThisRowName, int KitGroupID)
        {
            String NodeType = node.Name;
            String KitItemName = XmlCommon.XmlField(node, "Name");
            int KitItemTypeID = XmlCommon.XmlFieldUSInt(node, "KitItemTypeID");

            if (KitGroupID == 0 || KitItemName.Length == 0)
            {
                Results.WriteErrorEntry("ERROR", "KitGroupID and KitItem Name must be specified for a KitItem node");
                if (m_DBTrans != null)
                {
                    throw new ArgumentException("KitGroupID and KitItem Name must be specified for a KitItem node");
                }
                return;
            }

            Results.WriteVerboseEntry("Processing " + NodeType + ", KitGrouThisRowName=" + KitGrouThisRowName + ", KitItemName=" + KitItemName + ", KitGroupID=" + KitGroupID.ToString());

            StringBuilder sql = new StringBuilder(1024);

            decimal PriceDelta = XmlCommon.XmlFieldNativeDecimal(node, "PriceDelta");
            decimal WeightDelta = XmlCommon.XmlFieldNativeDecimal(node, "WeightDelta");

            int KitItemID = CheckForKitItem(KitItemName, KitGroupID);
            if (KitItemID == 0)
            {
                // add the KitItem:
                Results.WriteVerboseEntry("Adding KitItem(" + KitItemName + ", KitGroup=" + KitGrouThisRowName + ")");

                String NewGUID = CommonLogic.GetNewGUID();
				sql.Append("insert KitItem(KitItemGUID,Name,Description,KitGroupID,IsDefault,PriceDelta,WeightDelta,DisplayOrder,InventoryQuantityDelta) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append(DB.SQuote(KitItemName) + ",");
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
                sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(PriceDelta) + ",");
                sql.Append(Localization.CurrencyStringForDBWithoutExchangeRate(WeightDelta) + ",");
                sql.Append((XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0 ? 1 : XmlCommon.XmlFieldUSInt(node, "DisplayOrder")) + ",");
                sql.Append(XmlCommon.XmlFieldUSInt(node, "InventoryQuantityDelta"));
				sql.Append(")");
                RunCommand(sql.ToString());
                Results.WriteVerboseEntry("KitItem Added OK");
            }
            else
            {
                // update the KitItem
                Results.WriteVerboseEntry("Updating KitItem(" + KitItemName + ", KitGroup=" + KitGrouThisRowName + ")");

                sql.Append("update KitItem set ");
                sql.Append("Name=" + DB.SQuote(KitItemName) + ",");
                sql.Append("DisplayOrder=" + (XmlCommon.XmlFieldUSInt(node, "DisplayOrder") == 0 ? 1 : XmlCommon.XmlFieldUSInt(node, "DisplayOrder")) + ",");
                if (XmlCommon.XmlField(node, "Description").Length != 0)
                {
                    sql.Append("Description=" + DB.SQuote(XmlCommon.XmlField(node, "Description")) + ",");
                }
                else
                {
                    sql.Append("Description=NULL,");
                }
                sql.Append("KitGroupID=" + KitGroupID.ToString() + ",");
                sql.Append("PriceDelta=" + Localization.CurrencyStringForDBWithoutExchangeRate(PriceDelta) + ",");
                sql.Append("WeightDelta=" + Localization.CurrencyStringForDBWithoutExchangeRate(WeightDelta) + ",");
                sql.Append("IsDefault=" + CommonLogic.IIF(XmlCommon.XmlFieldBool(node, "IsDefault"), "1", "0") + ",");
				sql.Append("InventoryQuantityDelta=" + XmlCommon.XmlFieldUSInt(node, "InventoryQuantityDelta"));
				sql.Append(" where KitItemID=" + KitItemID.ToString());
                RunCommand(sql.ToString());
                Results.WriteVerboseEntry("KitItem Updated OK");
            }

        }


        private void hlpWriteNodeEcho(XmlNode node)
        {
            if (node == null)
            {
                return;
            }
            StringBuilder tmpS = new StringBuilder(1024);
            tmpS.Append("<");
            tmpS.Append(node.Name);
            foreach (XmlAttribute a in node.Attributes)
            {
                tmpS.Append(" ");
                tmpS.Append(a.Name);
                tmpS.Append("=\"");
                tmpS.Append(XmlCommon.XmlEncodeAttribute(a.InnerText));
                tmpS.Append("\"");
            }
            tmpS.Append("/>");
            Results.WriteXml(tmpS.ToString());
        }

        public int CheckForCustomer(String EMail)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select CustomerID from Customer  with (NOLOCK)  where Deleted=0 and EMail=" + DB.SQuote(EMail.ToLowerInvariant());
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return ID;
        }

        public int CheckForManufacturer(String Name, bool AddIfNotFound)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select ManufacturerID from Manufacturer  with (NOLOCK)  where Deleted=0 and lower(name)=" + DB.SQuote(Name.ToLowerInvariant());
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "ManufacturerID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            if (ID == 0 && AddIfNotFound)
            {
                StringBuilder sql = new StringBuilder(1024);
                String NewGUID = CommonLogic.GetNewGUID();
                sql.Append("insert Manufacturer(ManufacturerGUID,IsImport,Name,SEName) values(");
                sql.Append(DB.SQuote(NewGUID) + ",");
                sql.Append("1,"); // Set IsImport Flag For later Undo of Import
                sql.Append(DB.SQuote(Name) + ",");
                sql.Append(DB.SQuote(SE.MungeName(Name)));
                sql.Append(")");
                RunCommand(sql.ToString());

                SqlConnection con2 = null;
                IDataReader rs2 = null;
                try
                {
                    string query = "select ManufacturerID from Manufacturer  with (NOLOCK)  where Deleted=0 and Name=" + DB.SQuote(Name);
                    if (m_DBTrans != null)
                    {
                        // if a transaction was passed, we should use the transaction objects connection
                        rs2 = DB.GetRS(query, m_DBTrans);
                    }
                    else
                    {
                        // otherwise create it
                        con2 = new SqlConnection(DB.GetDBConn());
                        con2.Open();
                        rs2 = DB.GetRS(query, con2);
                    }

                    using (rs2)
                    {
                        if (rs2.Read())
                        {
                            ID = DB.RSFieldInt(rs2, "ManufacturerID");
                        }
                    }
                }
                catch { throw; }
                finally
                {
                    // we can't dispose of the connection if it's part of a transaction
                    if (con2 != null && m_DBTrans == null)
                    {
                        // here it's safe to dispose since we created the connection ourself
                        con2.Dispose();
                    }

                    // make sure we won't reference this again in code
                    rs2 = null;
                    con2 = null;
                }


            }
            return ID;
        }

        public int CheckForEntity(String EntityName, String Name, int ParentEntityID)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select " + EntityName + "ID from " + EntityName + "  with (NOLOCK)  where Deleted=0 and lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()) + (ParentEntityID == 0 ? "and (Parent" + EntityName + "ID=0 or Parent" + EntityName + "ID IS NULL)" : " and Parent" + EntityName + "ID=" + ParentEntityID.ToString());
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, EntityName + "ID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return ID;
        }


        public int CheckForProductType(String Name)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select ProductTypeID from ProductType  with (NOLOCK)  where lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant());
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "ProductTypeID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return ID;
        }

        public int CheckForAppConfig(String Name)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select AppConfigID from AppConfig  with (NOLOCK)  where lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant());
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "AppConfigID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return ID;
        }

        public int CheckForProduct(String Name)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select ProductID from Product  with (NOLOCK)  where Deleted<>1 and lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant());
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "ProductID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }



            return ID;
        }

        public int CheckForProductVariant(String Name, int ProductID)
        {
            int ID = 0;

            SqlConnection con = null;
            IDataReader rs = null;
            try
            {
                string query = "Select VariantID from ProductVariant  with (NOLOCK)  where Deleted=0 and lower(Name)=" + DB.SQuote(Name.Trim().ToLowerInvariant()) + " and ProductID=" + ProductID.ToString();
                if (m_DBTrans != null)
                {
                    // if a transaction was passed, we should use the transaction objects connection
                    rs = DB.GetRS(query, m_DBTrans);
                }
                else
                {
                    // otherwise create it
                    con = new SqlConnection(DB.GetDBConn());
                    con.Open();
                    rs = DB.GetRS(query, con);
                }

                using (rs)
                {
                    if (rs.Read())
                    {
                        ID = DB.RSFieldInt(rs, "VariantID");
                    }
                }
            }
            catch { throw; }
            finally
            {
                // we can't dispose of the connection if it's part of a transaction
                if (con != null && m_DBTrans == null)
                {
                    // here it's safe to dispose since we created the connection ourself
                    con.Dispose();
                }

                // make sure we won't reference this again in code
                rs = null;
                con = null;
            }

            return ID;
        }



        /*The following method is used to convert a pricing file in Excel format to XML*/
        public static string ConvertPricingFileToXml(string PricingFile)
        {
            ExcelToXml exf = new ExcelToXml(PricingFile);

            if (exf == null)
            {
                return "";
            }

            XmlDocument xmlDoc = exf.LoadSheet("Sheet1", "M", 100000, "A,B,I");
            return xmlDoc.InnerXml;
        }

        public static String ProcessExcelImportFileTrackingNumbers(String ExcelFile, System.Collections.Generic.Dictionary<string, EntityHelper> EntityHelpers, Parser GetParser)
        {
            StringBuilder tmpS = new StringBuilder(65536); // try to give it some room, to avoid reallocs

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
                                tmpS.Append("<tr><td><b>Order</b>: " + orderNumber + " = " + trackingNumber + "</td><td><b>Existing</b>: " + (existing.Length == 0 ? AppLogic.ro_NotApplicable : existing) + "</td><td><b>Updated</b>: <i>Duplicate - ignored</i></td></tr>");
                            }
                        }
                        else
                        {
                            Order.MarkOrderAsShipped(orderNumber, "", trackingNumber, DateTime.Now, false, EntityHelpers, GetParser, false);
                            tmpS.Append("<tr><td><b>Order</b>: " + orderNumber + " = " + trackingNumber + "</td><td><b>Existing</b>: " + (existing.Length == 0 ? AppLogic.ro_NotApplicable : existing) + "</td><td><b>Updated</b>: " + trackingNumber + "</td></tr>");
                        }
                    }
                    catch
                    {
                        tmpS.Append("<tr><td><b>Order</b>: " + orderNumber + " = " + trackingNumber + "</td><td><b>Existing</b>: " + (existing.Length == 0 ? AppLogic.ro_NotApplicable : existing) + "</td><td><b>Updated</b>: ERROR UPDATING</td></tr>");
                    }
                }
            }
            ds.Dispose();
            tmpS.Append("</table>");

            return tmpS.ToString();
        }

        // ColumnNumbers are 1 based here!! but 0 based in dataset!
        private String GetExcelColumnForXml(DataRow row, int ColumnNumber, bool XmlEncodeIt)
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
        private String ConvertExcelImportFileToXml(String ExcelFile)
        {
            StringBuilder tmpS = new StringBuilder(100000);
            ExcelToXml exf = new ExcelToXml(ExcelFile);
            int MaxRowsInSpreadsheet = AppLogic.AppConfigUSInt("ImportMaxRowsExcel");
            if (MaxRowsInSpreadsheet == 0)
            {
                MaxRowsInSpreadsheet = 10000;
            }
            // a row without cols A or AR having data terminates the spreadsheet!
            XmlDocument xmlDoc = exf.LoadSheet("Sheet1", "CA", MaxRowsInSpreadsheet, "A,AR");

            tmpS.Append("<AspDotNetStorefrontImport Notes=\"WSI BETA\" Version=\"7.1\" SetImportFlag=\"true\" AutoLazyAdd=\"true\" AutoCleanup=\"true\" Verbose=\"true\" TransactionsEnabled=\"true\">");
            tmpS.Append("<Transaction Name=\"ExcelImport\">");
            int rowI = 1;
            Decimal ImportFileVersion = 2.0M;
            String LastRowName = String.Empty;
            String FV = exf.GetCell(xmlDoc, 1, "E");
            if (FV.StartsWith("IMPORT FILE VERSION", StringComparison.InvariantCultureIgnoreCase))
            {
                ImportFileVersion = Localization.ParseNativeDecimal(exf.GetCell(xmlDoc, 1, "F"));
            }
            bool FirstKitGroup = true;
            bool FirstVariant = true;
            bool FirstProduct = true;
            foreach (XmlNode row in xmlDoc.SelectNodes("/excel/sheet/row"))
            {
                rowI = XmlCommon.XmlAttributeUSInt(row, "id");
                if (rowI > 3) // skip first 3 header rows!
                {
                    // if this line has no variant price, and is not a kit group or kit item, consider this line as EOF
                    String ThisRowName = exf.GetCell(row, "A").ToUpper(CultureInfo.InvariantCulture);
                    String s;
                    if (ThisRowName.Length != 0 && (ThisRowName != "KITITEM" && ThisRowName != "KITGROUP" && ThisRowName != "KITGROUPDEF"))
                    {
                        // we are on a product start row
                        FirstVariant = true;
                    }
                    if (ThisRowName == "KITGROUPDEF")
                    {
                        if (LastRowName == "KITITEM" || LastRowName == "KITGROUP")
                        {
                            tmpS.Append("</KitGroup>");
                        }
                    }
                    else if (ThisRowName == "KITGROUP")
                    {
                        // Process Kit Group Row
                        if (LastRowName == "KITITEM" || LastRowName == "KITGROUP")
                        {
                            tmpS.Append("</KitGroup>");
                        }
                        if (FirstKitGroup)
                        {
                            tmpS.Append("</Variants>");
                            tmpS.Append("<Kit>");
                            FirstKitGroup = false;
                        }
                        tmpS.Append("<KitGroup>");
                        tmpS.Append(String.Format("<Name>{0}</Name>", XmlCommon.XmlEncode(exf.GetCell(row, "B"))));
                        tmpS.Append(String.Format("<Description>{0}</Description>", XmlCommon.XmlEncode(exf.GetCell(row, "C"))));
                        tmpS.Append(String.Format("<DisplayOrder>{0}</DisplayOrder>", XmlCommon.XmlEncode(exf.GetCell(row, "D"))));
                        tmpS.Append(String.Format("<KitGroupTypeID>{0}</KitGroupTypeID>", XmlCommon.XmlEncode(exf.GetCell(row, "E"))));
                        tmpS.Append(String.Format("<IsRequired>{0}</IsRequired>", XmlCommon.XmlEncode(exf.GetCell(row, "F"))));
                    }
                    else if (ThisRowName == "KITITEM")
                    {
                        // Process Kit Item Row
                        if (exf.GetCell(row, "B").ToUpper(CultureInfo.InvariantCulture) != "NAME" && exf.GetCell(row, "C").ToUpper(CultureInfo.InvariantCulture) != "DESCRIPTION")
                        {
                            tmpS.Append("<KitItem>");
                            tmpS.Append(String.Format("<Name>{0}</Name>", XmlCommon.XmlEncode(exf.GetCell(row, "B"))));
                            tmpS.Append(String.Format("<Description>{0}</Description>", XmlCommon.XmlEncode(exf.GetCell(row, "C"))));
                            tmpS.Append(String.Format("<DisplayOrder>{0}</DisplayOrder>", XmlCommon.XmlEncode(exf.GetCell(row, "D"))));
                            tmpS.Append(String.Format("<PriceDelta>{0}</PriceDelta>", XmlCommon.XmlEncode(exf.GetCell(row, "E"))));
                            tmpS.Append(String.Format("<IsDefault>{0}</IsDefault>", XmlCommon.XmlEncode(exf.GetCell(row, "F"))));
                            tmpS.Append(String.Format("<TextOptionMaxLength>{0}</TextOptionMaxLength>", XmlCommon.XmlEncode(exf.GetCell(row, "G"))));
                            tmpS.Append(String.Format("<TextOptionWidth>{0}</TextOptionWidth>", XmlCommon.XmlEncode(exf.GetCell(row, "H"))));
                            tmpS.Append(String.Format("<TextOptionHeight>{0}</TextOptionHeight>", XmlCommon.XmlEncode(exf.GetCell(row, "I"))));
                            tmpS.Append(String.Format("<WeightDelta>{0}</WeightDelta>", XmlCommon.XmlEncode(exf.GetCell(row, "J"))));
                            tmpS.Append("</KitItem>");
                        }
                    }
                    else
                    {
                        if (ThisRowName.Length != 0)
                        {
                            if (LastRowName == "KITGROUP" || LastRowName == "KITITEM")
                            {
                                tmpS.Append("</KitGroup>");
                                tmpS.Append("</Kit>");
                            }
                            if (!FirstProduct)
                            {
                                if (LastRowName != "KITITEM" && LastRowName != "KITGROUP" && LastRowName != "KITGROUPDEF")
                                {
                                    tmpS.Append("</Variants>");
                                }
                                tmpS.Append("</Product>");
                            }
                            FirstKitGroup = true;
                            FirstProduct = false;
                            FirstVariant = true;
                            tmpS.Append("<Product Action=\"Update\">");
                            tmpS.Append(String.Format("<Name>{0}</Name>", XmlCommon.XmlEncode(exf.GetCell(row, "A"))));
                            tmpS.Append(String.Format("<ProductType Name=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(exf.GetCell(row, "B"))));
                            tmpS.Append("<Mappings AutoCleanup=\"true\">");
                            tmpS.Append(String.Format("<Entity EntityType=\"Manufacturer\" Name=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(exf.GetCell(row, "C"))));
                            tmpS.Append(String.Format("<Entity EntityType=\"Distributor\" Name=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(exf.GetCell(row, "D"))));

                            s = exf.GetCell(row, "E");
                            if (s.Length != 0 && !s.StartsWith("/"))
                            {
                                s = "/" + s;
                            }
                            tmpS.Append(String.Format("<Entity EntityType=\"Category\" XPath=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(s)));
                            s = exf.GetCell(row, "F");
                            if (s.Length != 0 && !s.StartsWith("/"))
                            {
                                s = "/" + s;
                            }
                            tmpS.Append(String.Format("<Entity EntityType=\"Category\" XPath=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(s)));
                            if (ImportFileVersion >= 3)
                            {
                                s = exf.GetCell(row, "G");
                                if (s.Length != 0 && !s.StartsWith("/"))
                                {
                                    s = "/" + s;
                                }
                                tmpS.Append(String.Format("<Entity EntityType=\"Category\" XPath=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(s)));
                                s = exf.GetCell(row, "H");
                                if (s.Length != 0 && !s.StartsWith("/"))
                                {
                                    s = "/" + s;
                                }
                                tmpS.Append(String.Format("<Entity EntityType=\"Category\" XPath=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(s)));
                            }
                            s = exf.GetCell(row, "I");
                            if (s.Length != 0 && !s.StartsWith("/"))
                            {
                                s = "/" + s;
                            }
                            tmpS.Append(String.Format("<Entity EntityType=\"Section\" XPath=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(s)));
                            s = exf.GetCell(row, "J");
                            if (s.Length != 0 && !s.StartsWith("/"))
                            {
                                s = "/" + s;
                            }
                            tmpS.Append(String.Format("<Entity EntityType=\"Section\" XPath=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(s)));
                            if (ImportFileVersion >= 3)
                            {
                                s = exf.GetCell(row, "K");
                                if (s.Length != 0 && !s.StartsWith("/"))
                                {
                                    s = "/" + s;
                                }
                                tmpS.Append(String.Format("<Entity EntityType=\"Section\" XPath=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(s)));
                                s = exf.GetCell(row, "L");
                                if (s.Length != 0 && !s.StartsWith("/"))
                                {
                                    s = "/" + s;
                                }
                                tmpS.Append(String.Format("<Entity EntityType=\"Section\" XPath=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(s)));
                            }
                            tmpS.Append("</Mappings>");
                            tmpS.Append(String.Format("<Summary>{0}</Summary>", XmlCommon.XmlEncode(exf.GetCell(row, "M"))));
                            tmpS.Append(String.Format("<Description>{0}</Description>", XmlCommon.XmlEncode(exf.GetCell(row, "N"))));
                            tmpS.Append("<SE>");
                            tmpS.Append(String.Format("<SEKeywords>{0}</SEKeywords>", XmlCommon.XmlEncode(exf.GetCell(row, "O"))));
                            tmpS.Append(String.Format("<SEDescription>{0}</SEDescription>", XmlCommon.XmlEncode(exf.GetCell(row, "P"))));
                            tmpS.Append(String.Format("<SETitle>{0}</SETitle>", XmlCommon.XmlEncode(exf.GetCell(row, "Q"))));
                            tmpS.Append("</SE>");
                            tmpS.Append(String.Format("<SKU>{0}</SKU>", XmlCommon.XmlEncode(exf.GetCell(row, "R"))));
                            tmpS.Append(String.Format("<ManufacturerPartNumber>{0}</ManufacturerPartNumber>", XmlCommon.XmlEncode(exf.GetCell(row, "S"))));
                            tmpS.Append(String.Format("<XmlPackage>{0}</XmlPackage>", XmlCommon.XmlEncode(exf.GetCell(row, "T"))));
                            tmpS.Append(String.Format("<ColWidth>{0}</ColWidth>", XmlCommon.XmlEncode(exf.GetCell(row, "U"))));
                            tmpS.Append(String.Format("<SalesPrompt ID=\"{0}\"/>", XmlCommon.XmlEncodeAttribute(exf.GetCell(row, "V"))));
                            tmpS.Append(String.Format("<Published>{0}</Published>", XmlCommon.XmlEncode(exf.GetCell(row, "W"))));
                            tmpS.Append(String.Format("<RequiresRegistration>{0}</RequiresRegistration>", XmlCommon.XmlEncode(exf.GetCell(row, "X"))));
                            tmpS.Append("<RelatedProducts>");
                            s = exf.GetCell(row, "Y");
                            foreach (String s2 in s.Split(','))
                            {
                                tmpS.Append(XmlCommon.XmlEncode(s2));
                                tmpS.Append(String.Format("<CX ID=\"{0}\"/>", XmlCommon.XmlEncode(s2)));
                            }
                            tmpS.Append("</RelatedProducts>");
                            tmpS.Append(String.Format("<MiscText>{0}</MiscText>", XmlCommon.XmlEncode(exf.GetCell(row, "Z"))));
                            tmpS.Append("<InventoryType>");
                            String tmpX = exf.GetCell(row, "AA");
                            tmpS.Append(String.Format("<TrackInventoryBySizeAndColor>{0}</TrackInventoryBySizeAndColor>", tmpX));
                            tmpS.Append("</InventoryType>");
                            if (ImportFileVersion >= 3)
                            {
                                tmpS.Append(String.Format("<IsAKit>{0}</IsAKit>", XmlCommon.XmlEncode(exf.GetCell(row, "AD"))));
                            }
                            tmpS.Append("<Images>");
                            tmpS.Append(String.Format("<ImageFilenameOverride>{0}</ImageFilenameOverride>", XmlCommon.XmlEncode(exf.GetCell(row, "AG"))));
                            tmpS.Append("</Images>");
                            tmpS.Append(String.Format("<ExtensionData>{0}</ExtensionData>", XmlCommon.XmlEncode(exf.GetCell(row, "AH"))));
                        }

                        // col AI is not used!

                        if (FirstVariant)
                        {
                            tmpS.Append("<Variants AutoCleanup=\"true\">");
                        }

                        tmpS.Append("<Variant Action=\"Update\">");
                        tmpS.Append(String.Format("<Name>{0}</Name>", XmlCommon.XmlEncode(exf.GetCell(row, "AJ"))));
                        tmpS.Append(String.Format("<IsDefault>{0}</IsDefault>", XmlCommon.XmlEncode(exf.GetCell(row, "AK"))));
                        tmpS.Append(String.Format("<SKUSuffix>{0}</SKUSuffix>", XmlCommon.XmlEncode(exf.GetCell(row, "AL"))));
                        tmpS.Append(String.Format("<ManufacturerPartNumber>{0}</ManufacturerPartNumber>", XmlCommon.XmlEncode(exf.GetCell(row, "AM"))));
                        tmpS.Append(String.Format("<Description>{0}</Description>", XmlCommon.XmlEncode(exf.GetCell(row, "AN"))));
                        tmpS.Append("<SE>");
                        tmpS.Append(String.Format("<SEKeywords>{0}</SEKeywords>", XmlCommon.XmlEncode(exf.GetCell(row, "AO"))));
                        tmpS.Append(String.Format("<SEDescription>{0}</SEDescription>", XmlCommon.XmlEncode(exf.GetCell(row, "AP"))));
                        tmpS.Append(String.Format("<SETitle>{0}</SETitle>", XmlCommon.XmlEncode(exf.GetCell(row, "AQ"))));
                        tmpS.Append("</SE>");
                        tmpS.Append(String.Format("<Price>{0}</Price>", XmlCommon.XmlEncode(exf.GetCell(row, "AR"))));
                        tmpS.Append(String.Format("<SalePrice>{0}</SalePrice>", XmlCommon.XmlEncode(exf.GetCell(row, "AS"))));
                        tmpS.Append(String.Format("<MSRP>{0}</MSRP>", XmlCommon.XmlEncode(exf.GetCell(row, "AT"))));
                        tmpS.Append(String.Format("<Cost>{0}</Cost>", XmlCommon.XmlEncode(exf.GetCell(row, "AU"))));
                        tmpS.Append(String.Format("<Weight>{0}</Weight>", XmlCommon.XmlEncode(exf.GetCell(row, "AV"))));
                        tmpS.Append(String.Format("<Dimensions>{0}</Dimensions>", XmlCommon.XmlEncode(exf.GetCell(row, "AW"))));
                        tmpS.Append(String.Format("<Inventory>{0}</Inventory>", XmlCommon.XmlEncode(exf.GetCell(row, "AX"))));
                        tmpS.Append(String.Format("<DisplayOrder>{0}</DisplayOrder>", XmlCommon.XmlEncode(exf.GetCell(row, "AY"))));

                        tmpS.Append("<Colors>");
                        String ColorsMaster = exf.GetCell(row, "AZ").Trim();
                        if (ColorsMaster.Length != 0)
                        {
                            String ColorSKUModifiers = exf.GetCell(row, "BA").Trim();
                            String[] ColorsMasterSplit = ColorsMaster.Split(',');
                            String[] ColorSKUsSplit = ColorSKUModifiers.Split(',');
                            for (int i = ColorsMasterSplit.GetLowerBound(0); i <= ColorsMasterSplit.GetUpperBound(0); i++)
                            {
                                String[] ColorsX = ColorsMasterSplit[i].Replace(']', '[').Split('[');
                                String ColorText = ColorsX[0].Trim();
                                Decimal ColorPriceDelta = System.Decimal.Zero;
                                String Modifier = String.Empty;
                                if (ColorsX.GetUpperBound(0) >= 1)
                                {
                                    ColorPriceDelta = Localization.ParseNativeDecimal(ColorsX[1].Trim());
                                }
                                try
                                {
                                    Modifier = ColorSKUsSplit[i].Trim();
                                }
                                catch { }
                                tmpS.Append("<Color SKUModifier=\"" + XmlCommon.XmlEncodeAttribute(Modifier) + "\" PriceDelta=\"" + Localization.DecimalStringForDB(ColorPriceDelta) + "\">" + XmlCommon.XmlEncodeAttribute(ColorText) + "</Color>");
                            }
                        }

                        tmpS.Append("</Colors>");


                        tmpS.Append("<Sizes>");
                        String SizesMaster = exf.GetCell(row, "BB").Trim();
                        if (SizesMaster.Length != 0)
                        {
                            String SizeSKUModifiers = exf.GetCell(row, "BC").Trim();
                            String[] SizesMasterSplit = SizesMaster.Split(',');
                            String[] SizeSKUsSplit = SizeSKUModifiers.Split(',');
                            for (int i = SizesMasterSplit.GetLowerBound(0); i <= SizesMasterSplit.GetUpperBound(0); i++)
                            {
                                String[] SizesX = SizesMasterSplit[i].Replace(']', '[').Split('[');
                                String SizeText = SizesX[0].Trim();
                                Decimal SizePriceDelta = System.Decimal.Zero;
                                String Modifier = String.Empty;
                                if (SizesX.GetUpperBound(0) >= 1)
                                {
                                    SizePriceDelta = Localization.ParseNativeDecimal(SizesX[1].Trim());
                                }
                                try
                                {
                                    Modifier = SizeSKUsSplit[i].Trim();
                                }
                                catch { }
                                tmpS.Append("<Size SKUModifier=\"" + XmlCommon.XmlEncodeAttribute(Modifier) + "\" PriceDelta=\"" + Localization.DecimalStringForDB(SizePriceDelta) + "\">" + XmlCommon.XmlEncodeAttribute(SizeText) + "</Size>");
                            }
                        }
                        tmpS.Append("</Sizes>");

                        tmpS.Append(String.Format("<IsTaxable>{0}</IsTaxable>", XmlCommon.XmlEncode(exf.GetCell(row, "BD"))));
                        tmpS.Append(String.Format("<IsShipSeparately>{0}</IsShipSeparately>", XmlCommon.XmlEncode(exf.GetCell(row, "BE"))));
                        tmpS.Append(String.Format("<IsDownload>{0}</IsDownload>", XmlCommon.XmlEncode(exf.GetCell(row, "BF"))));
                        tmpS.Append(String.Format("<DownloadLocation>{0}</DownloadLocation>", XmlCommon.XmlEncode(exf.GetCell(row, "BG"))));
                        tmpS.Append(String.Format("<Published>{0}</Published>", XmlCommon.XmlEncode(exf.GetCell(row, "BH"))));
                        tmpS.Append("<Images>");
                        tmpS.Append(String.Format("<ImageFilenameOverride>{0}</ImageFilenameOverride>", XmlCommon.XmlEncode(exf.GetCell(row, "BI"))));
                        tmpS.Append("</Images>");
                        tmpS.Append(String.Format("<ExtensionData>{0}</ExtensionData>", XmlCommon.XmlEncode(exf.GetCell(row, "BJ"))));
                        tmpS.Append("</Variant>");
                    }
                    FirstVariant = false;
                    LastRowName = ThisRowName;
                }
            }
            if (!FirstProduct)
            {
                if (LastRowName == "KITITEM" || LastRowName == "KITGROUP")
                {
                    tmpS.Append("</KitGroup>");
                    tmpS.Append("</Kit>");
                }
                else
                {
                    tmpS.Append("</Variants>");
                }
                tmpS.Append("</Product>");
            }
            tmpS.Append("</Transaction>");
            tmpS.Append("</AspDotNetStorefrontImport>");
            return tmpS.ToString();
        }

        public WSIStatusWriter Results
        {
            get
            {
                if (m_ResultWriter == null)
                {
                    m_ResultWriter = new WSIStatusWriter(m_Version, m_Verbose);
                }
                return m_ResultWriter;
            }
        }

        public XmlDocument GetResults()
        {
            if (m_ResultWriter == null)
            {
                m_ResultWriter = new WSIStatusWriter(m_Version, m_Verbose);
            }
            return m_ResultWriter.GetResults();
        }

        public String GetResultsAsString()
        {
            if (m_ResultWriter == null)
            {
                m_ResultWriter = new WSIStatusWriter(m_Version, m_Verbose);
            }
            return m_ResultWriter.GetResultsAsString();
        }
    }

    public class WSIStatusWriter
    {
        String m_Version = String.Empty;
        StringBuilder m_sb = null;
        StringWriter m_sw = null;
        XmlTextWriter m_writer = null;
        bool m_Verbose = false;

        public WSIStatusWriter(String Version, bool Verbose)
        {
            m_Version = Version;
            m_Verbose = Verbose;
            m_sb = new StringBuilder(4096);

            m_sw = new StringWriter(m_sb);
            m_writer = new XmlTextWriter(m_sw);

            m_writer.Formatting = Formatting.Indented;
            StartDocument();
        }

        private void StartDocument()
        {
            m_writer.WriteStartDocument();
            m_writer.WriteStartElement("AspDotNetStorefrontImportResult");
            m_writer.WriteAttributeString("Version", m_Version);
            m_writer.WriteAttributeString("DateTime", Security.HtmlEncode(Localization.ToNativeDateTimeString(System.DateTime.Now)));
        }

        private void EndDocument()
        {
            m_writer.WriteEndElement();
        }

        public void WriteVerboseEntry(String NodeName, String Message)
        {
            if (m_Verbose)
            {
                m_writer.WriteStartElement("Verbose");
                if (NodeName.Length != 0)
                {
                    m_writer.WriteAttributeString("NodeName", Security.HtmlEncode(NodeName));
                }
                m_writer.WriteAttributeString("Message", Security.HtmlEncode(Message));
                m_writer.WriteEndElement();
            }
        }

        public void WriteGetMappingsStartElement(int EntityID, String EntityGUID, String EntityType)
        {
            m_writer.WriteStartElement("GetMappings");
            m_writer.WriteAttributeString("EntityType", EntityType);
            m_writer.WriteAttributeString("ID", EntityID.ToString());
            m_writer.WriteAttributeString("GUID", EntityGUID);
        }

        public void WriteGetOrdersStartElement(int StartingOrderNumber, int EndingOrderNumber, bool JustReturnAllNewOrders)
        {
            m_writer.WriteStartElement("GetOrders");

        }

        public void WriteGetEntityHelperStartElement(String EntityType)
        {
            m_writer.WriteStartElement("GetEntityHelper");
            m_writer.WriteAttributeString("EntityType", EntityType);
        }

        public void WriteXml(String XmlFragment)
        {
            m_writer.WriteRaw(XmlFragment);
        }

        public void WriteStartElement(String ElementName)
        {
            m_writer.WriteStartElement(ElementName);
        }

        public void WriteEndElement()
        {
            m_writer.WriteEndElement();
        }

        public void WriteMappingEntry(String ObjectType, int ObjectID, String ObjectGUID, int DisplayOrder)
        {
            m_writer.WriteStartElement(ObjectType);
            m_writer.WriteAttributeString("ID", ObjectID.ToString());
            m_writer.WriteAttributeString("GUID", ObjectGUID);
            m_writer.WriteAttributeString("DisplayOrder", DisplayOrder.ToString());
            m_writer.WriteEndElement();
        }

        public void WriteVerboseEntry(String Message)
        {
            WriteVerboseEntry(String.Empty, Message);
        }

        public void WriteErrorEntry(String NodeType, String NodeName, String NodeGUID, String NodeID, String Message)
        {
            m_writer.WriteStartElement("Error");

            m_writer.WriteAttributeString("Message", Security.HtmlEncode(Message));
            m_writer.WriteEndElement();
        }

        public void WriteErrorEntry(String NodeName, String Message)
        {
            m_writer.WriteStartElement("Error");

            m_writer.WriteAttributeString("Message", Security.HtmlEncode(Message));
            m_writer.WriteEndElement();
        }

        public void WriteErrorEntry(String Message)
        {
            m_writer.WriteStartElement("Error");
            m_writer.WriteAttributeString("Message", Security.HtmlEncode(Message));
            m_writer.WriteEndElement();
        }

        public void WriteOutputEntry(String NodeType, String NodeName, String NodeGUID, int NodeID, WSI.NodeActionEnum ActionTaken, String Status, String Message)
        {
            m_writer.WriteStartElement("Item");
            m_writer.WriteAttributeString("NodeType", NodeType);
            m_writer.WriteAttributeString("Name", Security.HtmlEncode(NodeName));
            m_writer.WriteAttributeString("GUID", NodeGUID);
            m_writer.WriteAttributeString("ID", NodeID.ToString());
            m_writer.WriteAttributeString("ActionTaken", ActionTaken.ToString());
            m_writer.WriteAttributeString("Status", Security.HtmlEncode(Status));
            m_writer.WriteAttributeString("Message", Security.HtmlEncode(Message));
            m_writer.WriteEndElement();
        }

        public void WriteOutputEntry(String NodeType, String NodeName, String NodeGUID, int NodeID, WSI.OrderManagementActionEnum ActionTaken, String Status, String Message)
        {
            m_writer.WriteStartElement("Item");
            m_writer.WriteAttributeString("NodeType", NodeType);
            m_writer.WriteAttributeString("Name", Security.HtmlEncode(NodeName));
            m_writer.WriteAttributeString("GUID", NodeGUID);
            m_writer.WriteAttributeString("ID", NodeID.ToString());
            m_writer.WriteAttributeString("ActionTaken", ActionTaken.ToString());
            m_writer.WriteAttributeString("Status", Security.HtmlEncode(Status));
            m_writer.WriteAttributeString("Message", Security.HtmlEncode(Message));
            m_writer.WriteEndElement();
        }

        public void WriteAppConfigOutputEntry(String NodeType, String NodeName, String ConfigValue, String NodeGUID, int NodeID, WSI.NodeActionEnum ActionTaken, String Status, String Message)
        {
            m_writer.WriteStartElement("Item");
            m_writer.WriteAttributeString("NodeType", NodeType);
            m_writer.WriteAttributeString("Name", Security.HtmlEncode(NodeName));
            m_writer.WriteAttributeString("ConfigValue", Security.HtmlEncode(ConfigValue));
            m_writer.WriteAttributeString("GUID", NodeGUID);
            m_writer.WriteAttributeString("ID", NodeID.ToString());
            m_writer.WriteAttributeString("ActionTaken", ActionTaken.ToString());
            m_writer.WriteAttributeString("Status", Security.HtmlEncode(Status));
            m_writer.WriteAttributeString("Message", Security.HtmlEncode(Message));
            m_writer.WriteEndElement();
        }

        public void WriteGUIDOutputEntry(String theGUID)
        {
            m_writer.WriteStartElement("GUID");
            m_writer.WriteString(theGUID);
            m_writer.WriteEndElement();
        }

        public XmlDocument GetResults()
        {
            EndDocument();
            m_writer.Close();
            String xml = m_sb.ToString();
            XmlDocument d = new XmlDocument();
            if (!xml.StartsWith("<?xml"))
            {
                xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + xml;
            }
            if (xml.IndexOf("utf-16") != -1)
            {
                xml = xml.Replace("utf-16", "utf-8");
            }
            d.LoadXml(xml);
            return d;
        }

        // returns all raw results, so far, as string. NOTE: doc may NOT be closed or completely invalid!
        public String GetResultsAsString()
        {
            m_writer.Close();
            return m_sb.ToString();
        }

    }
}
