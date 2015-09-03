// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using AspDotNetStorefrontCommon;

namespace AspDotNetStorefrontCore
{
    public class Feed
    {
        private int m_Feedid = 0;
        private String m_FeedGuid;
        private int m_storeid;
        private string m_Name;
        private int m_Displayorder;
        private string m_Xmlpackage;
        private bool m_Canautoftp;
        private string m_Ftpusername;
        private string m_Ftppassword;
        private string m_Ftpserver;
        private int m_Ftpport;
        private string m_Ftpfilename;
        private string m_Extensiondata;
        private DateTime m_Createdon;
        private SqlConnection m_SqlCn = new SqlConnection(DB.GetDBConn());
        private SqlCommand m_SqlCmd = new SqlCommand();

        public Feed(int FeedID)
        {
            m_SqlCn.Open();
            m_SqlCmd.Connection = m_SqlCn;
            m_SqlCmd.CommandText = "aspdnsf_GetFeed";
            m_SqlCmd.CommandType = CommandType.StoredProcedure;
            m_SqlCmd.Parameters.Add(new SqlParameter("@FeedID", SqlDbType.Int, 4));
            m_SqlCmd.Parameters["@FeedID"].Value = FeedID;
            try
            {
                SqlDataReader dr = m_SqlCmd.ExecuteReader();
                if (dr.Read())
                {
                    m_Feedid = FeedID;
                    m_FeedGuid = DB.RSFieldGUID(dr, "FeedGUID"); 
                    m_storeid = DB.RSFieldInt(dr, "StoreID"); 
                    m_Name = DB.RSField(dr, "Name"); 
                    m_Displayorder = DB.RSFieldInt(dr, "DisplayOrder");
                    m_Xmlpackage = DB.RSField(dr, "XmlPackage").ToLowerInvariant(); 
                    m_Canautoftp = DB.RSFieldBool(dr, "CanAutoFTP");
                    m_Ftpusername = DB.RSField(dr, "FTPUsername"); 
                    m_Ftppassword = DB.RSField(dr, "FTPPassword"); 
                    m_Ftpserver = DB.RSField(dr, "FTPServer"); 
                    m_Ftpport = DB.RSFieldInt(dr, "FTPPort");  
                    m_Ftpfilename = DB.RSField(dr, "FTPFilename");
                    m_Extensiondata = DB.RSField(dr, "ExtensionData");
                    m_Createdon = DB.RSFieldDateTime(dr, "CreatedOn");
                }
                dr.Close();
            }
            catch
            {
            }
            m_SqlCn.Close();
            m_SqlCmd.Parameters.Clear();
        }

        /// <summary>
        /// Creates a new feed record and returns a feed object for the new feed
        /// </summary>
        /// <param name="FeedName">Feed name, required</param>
        /// <param name="DisplayOrder">Display Order</param>
        /// <param name="XmlPackage">XmlPackage that defines the feed data and format</param>
        /// <param name="CanAutoFTP"></param>
        /// <param name="FTPUsername">Username for FTP site to which the feed is uploaded</param>
        /// <param name="FTPPassword">Password for FTP site to which the feed is uploaded</param>
        /// <param name="FTPServer">URL for FTP site to which the feed is uploaded</param>
        /// <param name="FTPPort">FTP port for for FTP site to which the feed is uploaded, the standard FTP port is 21</param>
        /// <param name="FTPFilename"></param>
        /// <param name="ExtensionData">Custom defined data</param>
        /// <returns>A new feed object if successfull, null otherwise</returns>
        static public Feed CreateFeed(int StoreID, string FeedName, int DisplayOrder, string XmlPackage, bool CanAutoFTP, string FTPUsername, string FTPPassword, string FTPServer, int FTPPort, string FTPFilename, string ExtensionData)
        {
            char[] TrimChars = {' ', '/'};
            FTPServer = FTPServer.ToLowerInvariant().Replace("ftp://", "").TrimEnd(TrimChars);
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            SqlCommand cmd = new SqlCommand();
            cn.Open();
            cmd.Connection = cn;
            cmd.CommandText = "aspdnsf_CreateFeed";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@StoreID",        SqlDbType.Int,           4));
            cmd.Parameters.Add(new SqlParameter("@Name",           SqlDbType.NVarChar   , 200));
            cmd.Parameters.Add(new SqlParameter("@DisplayOrder",   SqlDbType.Int          , 4));
            cmd.Parameters.Add(new SqlParameter("@XmlPackage",     SqlDbType.NVarChar   , 200));
            cmd.Parameters.Add(new SqlParameter("@CanAutoFTP",     SqlDbType.TinyInt      , 1));
            cmd.Parameters.Add(new SqlParameter("@FTPUsername",    SqlDbType.NVarChar   , 200));
            cmd.Parameters.Add(new SqlParameter("@FTPPassword",    SqlDbType.NVarChar   , 200));
            cmd.Parameters.Add(new SqlParameter("@FTPServer",      SqlDbType.NVarChar   , 200));
            cmd.Parameters.Add(new SqlParameter("@FTPPort",        SqlDbType.Int          , 4));
            cmd.Parameters.Add(new SqlParameter("@FTPFilename",    SqlDbType.NVarChar   , 200));
            cmd.Parameters.Add(new SqlParameter("@ExtensionData",  SqlDbType.NText));
            cmd.Parameters.Add(new SqlParameter("@FeedID",         SqlDbType.Int       , 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@StoreID"].Value = StoreID;
            cmd.Parameters["@Name"].Value = FeedName;
            cmd.Parameters["@DisplayOrder"].Value = DisplayOrder;
            cmd.Parameters["@XmlPackage"].Value = XmlPackage;
            cmd.Parameters["@CanAutoFTP"].Value = CanAutoFTP;
            cmd.Parameters["@FTPUsername"].Value = FTPUsername.Trim();
            cmd.Parameters["@FTPPassword"].Value = FTPPassword.Trim();
            cmd.Parameters["@FTPServer"].Value = FTPServer;
            cmd.Parameters["@FTPPort"].Value = FTPPort;
            cmd.Parameters["@FTPFilename"].Value = FTPFilename.Trim();
            cmd.Parameters["@ExtensionData"].Value = ExtensionData;

            try
            {
                cmd.ExecuteNonQuery();
                int feedid = (int)cmd.Parameters["@FeedID"].Value;
                Feed f = new Feed(feedid);
                cn.Close();
                cmd.Dispose();
                cn.Dispose();
                return f;
            }
            catch
            {
                cn.Close();
                cmd.Dispose();
                cn.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Deletes a feed record
        /// </summary>
        /// <returns>Returns an empty string if successful otherwise it returns an error message</returns>
        static public string DeleteFeed(int FeedId)
        {
            string err = string.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            SqlCommand cmd = new SqlCommand();
            cn.Open();
            cmd.Connection = cn;
            cmd.CommandText = "aspdnsf_DelFeed";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@FeedID", SqlDbType.Int, 4));
            cmd.Parameters["@FeedID"].Value = FeedId;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();
            return err;
        }

        ~Feed()
        {
            m_SqlCmd.Dispose();
            m_SqlCn.Dispose();
        }


        /// <summary>
        /// Updates the current feed object
        /// </summary>
        /// <param name="FeedName">Feed name, set to null to leave unchanged</param>
        /// <param name="DisplayOrder">Feed display order on admin pages, set to -1 to leave unchanged</param>
        /// <param name="XmlPackage">XmlPackage used to create the feed output, set to null to leave unchanged</param>
        /// <param name="CanAutoFTP">set to -1 to leave unchanged</param>
        /// <param name="FTPUsername">Username for FTP site to which the feed is uploaded, set to null to leave unchanged</param>
        /// <param name="FTPPassword">Password for FTP site to which the feed is uploaded, set to null to leave unchanged</param>
        /// <param name="FTPServer">URL for FTP site to which the feed is uploaded, set to null to leave unchanged</param>
        /// <param name="FTPPort">FTP port for for FTP site to which the feed is uploaded, the standard FTP port is 21, set to -1 to leave unchanged</param>
        /// <param name="FTPFilename">, set to null to leave unchanged</param>
        /// <param name="ExtensionData">, set to null to leave unchanged</param>
        /// <returns>If the update is successful an empty string is returned otherwise the error message is returned</returns>
        public string UpdateFeed(int StoreID, string FeedName, int DisplayOrder, string XmlPackage, short CanAutoFTP, string FTPUsername, string FTPPassword, string FTPServer, int FTPPort, string FTPFilename, string ExtensionData)
        {
            char[] TrimChars = { ' ', '/' };
            FTPServer = FTPServer.ToLowerInvariant().Replace("ftp://", "").TrimEnd(TrimChars);
            string err = string.Empty;
            if (m_Feedid > 0)
            {
                m_SqlCn.Open();
                m_SqlCmd.Connection = m_SqlCn;
                m_SqlCmd.CommandText = "aspdnsf_UpdFeed";
                m_SqlCmd.Parameters.Add(new SqlParameter("@FeedID", SqlDbType.Int, 4));
                m_SqlCmd.Parameters.Add(new SqlParameter("@StoreID", SqlDbType.Int, 4));
                m_SqlCmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200));
                m_SqlCmd.Parameters.Add(new SqlParameter("@DisplayOrder", SqlDbType.Int, 4));
                m_SqlCmd.Parameters.Add(new SqlParameter("@XmlPackage", SqlDbType.NVarChar, 200));
                m_SqlCmd.Parameters.Add(new SqlParameter("@CanAutoFTP", SqlDbType.TinyInt, 1));
                m_SqlCmd.Parameters.Add(new SqlParameter("@FTPUsername", SqlDbType.NVarChar, 200));
                m_SqlCmd.Parameters.Add(new SqlParameter("@FTPPassword", SqlDbType.NVarChar, 200));
                m_SqlCmd.Parameters.Add(new SqlParameter("@FTPServer", SqlDbType.NVarChar, 200));
                m_SqlCmd.Parameters.Add(new SqlParameter("@FTPPort", SqlDbType.Int, 4));
                m_SqlCmd.Parameters.Add(new SqlParameter("@FTPFilename", SqlDbType.NVarChar, 200));
                m_SqlCmd.Parameters.Add(new SqlParameter("@ExtensionData", SqlDbType.NText));


                m_SqlCmd.Parameters["@FeedID"].Value = m_Feedid;

                if (StoreID == -1) m_SqlCmd.Parameters["@StoreID"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@StoreID"].Value = StoreID;

                if (FeedName == null) m_SqlCmd.Parameters["@Name"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@Name"].Value = FeedName;               

                if (DisplayOrder == -1) m_SqlCmd.Parameters["@DisplayOrder"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@DisplayOrder"].Value = DisplayOrder;

                if (XmlPackage == null) m_SqlCmd.Parameters["@XmlPackage"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@XmlPackage"].Value = XmlPackage;

                if (CanAutoFTP == -1) m_SqlCmd.Parameters["@CanAutoFTP"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@CanAutoFTP"].Value = CanAutoFTP;

                if (FTPUsername == null) m_SqlCmd.Parameters["@FTPUsername"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@FTPUsername"].Value = FTPUsername.Trim();

                if (FTPPassword == null) m_SqlCmd.Parameters["@FTPPassword"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@FTPPassword"].Value = FTPPassword.Trim();

                if (FTPServer == null) m_SqlCmd.Parameters["@FTPServer"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@FTPServer"].Value = FTPServer;

                if (FTPPort == -1) m_SqlCmd.Parameters["@FTPPort"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@FTPPort"].Value = FTPPort;

                if (FTPFilename == null) m_SqlCmd.Parameters["@FTPFilename"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@FTPFilename"].Value = FTPFilename.Trim();

                if (ExtensionData == null) m_SqlCmd.Parameters["@ExtensionData"].Value = DBNull.Value;
                else m_SqlCmd.Parameters["@ExtensionData"].Value = ExtensionData;

                try
                {
                    m_SqlCmd.ExecuteNonQuery();
                    if (StoreID != -1) m_storeid = StoreID;
                    if (Name != null) m_Name = FeedName;                    
                    if (DisplayOrder != -1) m_Displayorder = DisplayOrder;
                    if (XmlPackage != null) m_Xmlpackage = XmlPackage;
                    if (CanAutoFTP != -1) m_Canautoftp = (CanAutoFTP==1);
                    if (FTPUsername != null) m_Ftpusername = FTPUsername.Trim();
                    if (FTPPassword != null) m_Ftppassword = FTPPassword.Trim();
                    if (FTPServer != null) m_Ftpserver = FTPServer;
                    if (FTPPort != -1) m_Ftpport = FTPPort;
                    if (FTPFilename != null) m_Ftpfilename = FTPFilename.Trim();
                    if (ExtensionData != null) m_Extensiondata = ExtensionData;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
                m_SqlCn.Close();
                m_SqlCmd.Parameters.Clear();

            }
            else
            {
                err = "Feed object has not been initialized";
            }
            return err;
        }

        public String ExecuteFeed(Customer ThisCustomer)
        {
            return ExecuteFeed(ThisCustomer, String.Empty);
        }

        
        public string ExecuteFeed(Customer ThisCustomer, String RuntimeParams)
        {
            string folderpath = CommonLogic.IIF(AppLogic.IsAdminSite, CommonLogic.SafeMapPath("../images/"), CommonLogic.SafeMapPath("images"));
            string retval = string.Empty;
            try
            {
                string filename = String.Empty;
                if (FTPFilename.Length == 0)
                {
                    FTPFilename = XmlPackage + ".txt";
                }
                //Strip all of the remote pathing stuff before setting the local filename
                if (FTPFilename.IndexOf("/") != -1)
                {
                    filename = FTPFilename.Substring(FTPFilename.LastIndexOf("/")).Trim('/');
                }
                else
                {
                    filename = FTPFilename;
                }
                filename = Path.Combine(folderpath, filename);
                
                String[] Files;
                    Files = Directory.GetFileSystemEntries(folderpath);
                    foreach (string Element in Files)
                    {
                        try
                        {
                            if (Element.Substring(Element.LastIndexOf("\\")).Trim('\\').Substring(0, FTPFilename.Substring(0, FTPFilename.LastIndexOf(".")).Length) == FTPFilename.Substring(0, FTPFilename.LastIndexOf(".")))
                            {
                                File.Delete(Element);
                            }
                        }
                        catch { }
                    }
                
                string HideProductsWithLessThanThisInventoryLevel = AppLogic.AppConfig("HideProductsWithLessThanThisInventoryLevel");
                if (HideProductsWithLessThanThisInventoryLevel == null || HideProductsWithLessThanThisInventoryLevel == "")
                    HideProductsWithLessThanThisInventoryLevel = "0";

                // SELECT query for getting the total number of rows
                StringBuilder SqlQuery = new StringBuilder(10000);
                SqlQuery.Append("SELECT COUNT(*) FROM (select p.productid, p.name, isnull(pv.name, '') VariantName, p.description, p.sename, p.ImageFileNameOverride, p.SKU, isnull(p.FroogleDescription, '') ProductFroogleDescription, p.SEKeywords, ");
                SqlQuery.Append("p.ManufacturerPartNumber, pv.price, isnull(pv.saleprice, 0) saleprice, isnull(pv.FroogleDescription, '') VariantFroogleDescription, isnull(pv.description, '') VariantDescr from dbo.product p ");
                SqlQuery.Append("join dbo.productvariant pv on p.productid = pv.productid left join (select variantid, sum(quan) inventory from dbo.inventory group by variantid) i on pv.variantid = i.variantid where p.IsSystem=0 and p.deleted = 0 ");
                SqlQuery.Append("and p.published = 1 and p.ExcludeFromPriceFeeds = 0 and pv.isdefault = 1 and case p.TrackInventoryBySizeAndColor when 1 then isnull(i.inventory, 0) else pv.inventory end >= " + HideProductsWithLessThanThisInventoryLevel + ") AS tmp");
                
                // Code for query execution
                string err;
                object objReturn;
                Int32 RowCount;
                SqlConnection cn = new SqlConnection(DB.GetDBConn());
                SqlCommand cmd = new SqlCommand();
                cn.Open();
                cmd.Connection = cn;
                cmd.CommandText = SqlQuery.ToString();
                cmd.CommandType = CommandType.Text;
                try
                {
                    // Get the result of the query
                    objReturn = cmd.ExecuteScalar();

                    // Check if result is null
                    if (objReturn != null)
                    {
                        RowCount = (Int32)objReturn;

                        string newFileNames = string.Empty;

                        // The lower and upper bounds of the records to be retrieved
                        Int32 LowerBound = 1;
                        Int32 UpperBound = (LowerBound - 1) + 20000;

                        // Looping for processing of records by 20000
                        while (RowCount > 0)
                        {
                            if (RowCount < 20000)
                            {
                                UpperBound = (LowerBound - 1) + RowCount;
                                RowCount = 0;
                            }
                            else
                                RowCount -= 20000;

                            // Add parameters to feed.googlebase.xml.config
                            RuntimeParams += String.Format("LowerBound={0}", LowerBound);
                            RuntimeParams += String.Format("&UpperBound={0}", UpperBound);

                            // Same as the old code
                            string feeddocument = AppLogic.RunXmlPackage(m_Xmlpackage, null, ThisCustomer, 1, "", RuntimeParams, false, false);
                            feeddocument = feeddocument.Replace("encoding=\"utf-16\"", "");

                            // Construction of the new filename. New filename will be old filename concatenated to the record numbers being processed, e.g. googlefeed1_20000
                            string filenamePrefix = LowerBound.ToString() + "_" + UpperBound.ToString();

                            string newFileName;
                            if (filename.LastIndexOf(".") < 1)
                                newFileName = filename + filenamePrefix;
                            else
                                newFileName = filename.Insert(filename.LastIndexOf("."), filenamePrefix);

                            if (newFileNames == string.Empty)
                            {
                                newFileNames = newFileName;
                            }
                            else
                            {
                                newFileNames = string.Concat(newFileNames, ",", newFileName);
                            }
                            

                            using (StreamWriter sw = new StreamWriter(newFileName, false))
                            {
                                sw.Write(feeddocument);
                                sw.Close();
                            }
                            if (this.CanAutoFTP)
                            {
                                FtpClient ftp = new FtpClient(this.FTPServer + ":" + this.FTPPort.ToString(), this.FTPUsername, this.FTPPassword);
                                //retval = ftp.Upload(filename, this.FTPFilename);
                                retval = ftp.Upload(newFileName.Trim(), this.FTPFilename);
                            }
                            else
                            {
                                //retval = "The file " + this.FTPFilename + " has been created in the /images folder of your website";
                                retval = "The file(s) " + newFileNames.Trim() + " has been created in the /images folder of your website";
                            }

                            LowerBound += 20000;
                            UpperBound += 20000;

                            filenamePrefix = "";
                            feeddocument = "";
                            RuntimeParams = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                    return err;
                }

                cn.Close();
                cmd.Dispose();
                cn.Dispose();

                return retval;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        


        public int FeedID
        {
            get { return m_Feedid; }
        }
        public String FeedGUID
        {
            get { return m_FeedGuid; }
        }

        public int StoreID
        {
            get { return m_storeid; }
        }
        public string Name
        {
            get { return m_Name; }
        }
        public int DisplayOrder
        {
            get { return m_Displayorder; }
        }
        public string XmlPackage
        {
            get { return m_Xmlpackage; }
        }
        public bool CanAutoFTP
        {
            get { return m_Canautoftp; }
        }
        public string FTPUsername
        {
            get { return m_Ftpusername; }
        }
        public string FTPPassword
        {
            get { return m_Ftppassword; }
        }
        public string FTPServer
        {
            get { return m_Ftpserver; }
        }
        public int FTPPort
        {
            get { return m_Ftpport; }
        }
        public string FTPFilename
        {
            get { return m_Ftpfilename; }
            set { m_Ftpfilename = value; }
        }
        public string ExtensionData
        {
            get { return m_Extensiondata; }
        }
        public DateTime CreatedOn
        {
            get { return m_Createdon; }
        }
     


    }


}
