// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Xsl;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for DB.
    /// </summary>
    public class DB
    {

        static CultureInfo USCulture = new CultureInfo("en-US");
        static CultureInfo SqlServerCulture = new CultureInfo(CommonLogic.Application("DBSQLServerLocaleSetting"));

        public DB() { }

        static private String _activeDBConn = SetDBConn();

        static private String SetDBConn()
        {
            String s = CommonLogic.Application("DBConn");
            return s;
        }

        static public String GetDBConn()
        {
            return _activeDBConn;
        }

        static public SqlConnection dbConn()
        {
            return new SqlConnection(DB.GetDBConn());
        }

        static public String GetTableIdentityField(String TableName)
        {
            if (TableName.Length == 0)
            {
                return String.Empty;
            }
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select name from syscolumns with (NOLOCK) where id = object_id(" + DB.SQuote(TableName) + ") and colstat & 1 = 1", con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSField(rs, "name");
                    }
                }
            }

            return tmpS;
        }

        static public String GetTableColumnDataType(String TableName, String ColumnName)
        {
            if (TableName.Length == 0 || ColumnName.Length == 0)
            {
                return String.Empty;
            }
            String tmpS = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS("select st.name from syscolumns sc with (NOLOCK) join systypes st with (NOLOCK) on sc.xtype = st.xtype where id = object_id(" + DB.SQuote(TableName) + ") and sc.name = " + DB.SQuote(ColumnName) + "", con))
                {
                    if (rs.Read())
                    {
                        tmpS = DB.RSField(rs, "name");
                    }
                }
            }

            return tmpS;
        }

        public static String SQuote(String s)
        {
            int len = s.Length + 25;
            StringBuilder tmpS = new StringBuilder(len); // hopefully only one alloc
            tmpS.Append("N'");
            tmpS.Append(s.Replace("'", "''"));
            tmpS.Append("'");
            return tmpS.ToString();
        }

        public static String SQuoteNotUnicode(String s)
        {
            int len = s.Length + 25;
            StringBuilder tmpS = new StringBuilder(len); // hopefully only one alloc
            tmpS.Append("'");
            tmpS.Append(s.Replace("'", "''"));
            tmpS.Append("'");
            return tmpS.ToString();
        }

        public static String DateQuote(String s)
        {
            int len = s.Length + 25;
            StringBuilder tmpS = new StringBuilder(len); // hopefully only one alloc
            tmpS.Append("'");
            tmpS.Append(s.Replace("'", "''"));
            tmpS.Append("'");
            return tmpS.ToString();
        }

        public static String DateQuote(DateTime dt)
        {
            return DateQuote(Localization.ToDBDateTimeString(dt));
        }

        /// <summary>
        /// Provides a controlled construct for for which to use a DataContext object
        /// </summary>
        /// <param name="action"></param>
        public static void UseDataReader(string query, Action<IDataReader> action)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();

                using (IDataReader rs = DB.GetRS(query, con))
                {
                    action(rs);
                }
            }
        }
        /// <summary>
        /// Provides a controlled construct for for which to use a DataContext object
        /// </summary>
        /// <param name="command">SQL command to execute</param>
        /// <param name="action"></param>
        public static void UseDataReader(SqlCommand command, Action<IDataReader> action)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                command.Connection = con;
                con.Open();
                using (IDataReader rs = command.ExecuteReader())
                {
                    action(rs);
                }
            }
        }
        public static void UseDataReader(string query, SqlParameter[] parameters, Action<IDataReader> action)
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                SqlCommand command = new SqlCommand(query, con);
                command.Parameters.AddRange(parameters);
                con.Open();
                using (IDataReader rs = command.ExecuteReader())
                {
                    action(rs);
                }
            }
        }

        ///// <summary>
        ///// Provides a controlled construct for for which to use a DataContext object
        ///// </summary>
        ///// <param name="action"></param>
        //public static void UseDataContext(Action<DataContext> action)
        //{
        //    using (DataContext db = new DataContext(DB.GetDBConn()))
        //    {
        //        // use the delegate to do whatever operation it needs to do
        //        // so that we can control the disposing here..
        //        action(db);
        //    }
        //}

        static public IDataReader GetRS(String Sql, SqlConnection dbconn)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            using (SqlCommand cmd = new SqlCommand(Sql, dbconn))
            {
                return cmd.ExecuteReader();
            }
        }

        static public IDataReader GetRS(String Sql, SqlParameter[] spa, SqlConnection dbconn)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }

            using (SqlCommand cmd = new SqlCommand(Sql, dbconn))
            {
                foreach (SqlParameter sp in spa)
                {
                    cmd.Parameters.Add(sp);
                }
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }

        static public IDataReader GetRS(String Sql, SqlTransaction trans)
        {
            if (trans == null)
            {
                // we can't use the other overloads for this
                // since one is obsolete and the other one requires an 
                // SqlConnection object, creating one here, there's no way to tell
                // when we can/should dispose it.
                throw new ArgumentNullException("Transaction cannot be null!!");
            }
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            using (SqlCommand cmd = new SqlCommand(Sql, trans.Connection, trans))
            {
                return cmd.ExecuteReader();
            }
        }
        
        public static SqlDataReader SPGetRS(String procCall, SqlConnection dbconn)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SP=" + procCall + "\n");
            }

            using (SqlCommand cmd = new SqlCommand(procCall, dbconn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }

        public static SqlDataReader ExecuteStoredProcReader(String StoredProcName, SqlParameter[] spa, SqlConnection dbconn)
        {
            using (SqlCommand dbCommand = new SqlCommand(StoredProcName, dbconn))
            {
                dbCommand.CommandType = CommandType.StoredProcedure;

                foreach (SqlParameter sp in spa)
                { dbCommand.Parameters.Add(sp); }

                SqlDataReader dbReader;
                dbReader = dbCommand.ExecuteReader(CommandBehavior.CloseConnection);
                // Always call Read before accessing data.
                return dbReader;
            }

        }

        public static int ExecuteStoredProcInt(String StoredProcName, SqlParameter[] spa)
        {
            bool Flag = false;
            string RetValName = String.Empty;
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = GetDBConn();
            dbconn.Open();

            SqlCommand dbCommand = new SqlCommand(StoredProcName, dbconn);
            dbCommand.CommandType = CommandType.StoredProcedure;

            foreach (SqlParameter sp in spa)
            {
                if (sp.Direction == ParameterDirection.Output)
                {
                    Flag = true; // There is one output parameter to read value from, return this value
                    RetValName = sp.ParameterName;
                }
                dbCommand.Parameters.Add(sp);
            }

            try
            {
                int retval = dbCommand.ExecuteNonQuery();
                if (Flag)
                {
                    retval = Int32.Parse(dbCommand.Parameters[RetValName].Value.ToString());
                }
                dbCommand.Dispose();
                dbconn.Close();
                dbconn.Dispose();

                return retval;
            }
            catch (Exception ex)
            {
                dbCommand.Dispose();
                dbconn.Close();
                dbconn.Dispose();

                throw (ex);
            }

        }

        public static int ExecuteStoredProcInt(String StoredProcName, SqlParameter[] spa, SqlTransaction trans)
        {
            bool Flag = false;
            string RetValName = String.Empty;

            if (trans == null)
            {
                return ExecuteStoredProcInt(StoredProcName, spa);
            }

            SqlCommand dbCommand = new SqlCommand(StoredProcName, trans.Connection, trans);
            dbCommand.CommandType = CommandType.StoredProcedure;

            foreach (SqlParameter sp in spa)
            {
                if (sp.Direction == ParameterDirection.Output)
                {
                    Flag = true; // There is one output parameter to read value from, return this value
                    RetValName = sp.ParameterName;
                }
                dbCommand.Parameters.Add(sp);
            }

            try
            {
                int retval = dbCommand.ExecuteNonQuery();
                if (Flag)
                {
                    retval = Int32.Parse(dbCommand.Parameters[RetValName].Value.ToString());
                }
                dbCommand.Dispose();

                return retval;
            }
            catch (Exception ex)
            {
                dbCommand.Dispose();
                throw (ex);
            }

        }


        public static void ExecuteStoredProcVoid(String StoredProcName, SqlParameter[] spa, SqlConnection conn)
        {

            SqlCommand dbCommand = new SqlCommand(StoredProcName, conn);
            dbCommand.CommandType = CommandType.StoredProcedure;

            dbCommand.Parameters.AddRange(spa);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                dbCommand.Dispose();
                SysLog.LogException(ex, MessageTypeEnum.DatabaseException, MessageSeverityEnum.Error);
                throw ex;
            }

        }


        public static SqlParameter SetValueDecimal(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = (decimal)value;
            }

            return sparam;
        }

        public static SqlParameter SetValueBool(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = (bool)value;
            }

            return sparam;
        }
        public static SqlParameter SetValueSmallInt(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = (Int16)value;
            }

            return sparam;
        }
        public static SqlParameter SetValueTinyInt(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = Convert.ToByte(Convert.ToInt32(value));
            }

            return sparam;
        }
        public static SqlParameter SetValueInt(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = (int)value;
            }

            return sparam;
        }
        public static SqlParameter SetValueBigInt(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = (Int64)value;
            }

            return sparam;
        }
        public static SqlParameter SetValueDateTime(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = (DateTime)value;
            }
            return sparam;
        }
        public static SqlParameter SetValueGUID(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = (Guid)value;
            }
            return sparam;
        }
        public static SqlParameter SetValue(SqlParameter sparam, object value)
        {
            if (value == null)
            {
                sparam.Value = DBNull.Value;
            }
            else
            {
                sparam.Value = value;
            }
            return sparam;
        }

        public static SqlParameter CreateSQLParameter(String ParameterName, SqlDbType ParamterType, int ParamterLength, object Value, ParameterDirection Direction)
        {
            SqlParameter sq = new SqlParameter(ParameterName, ParamterType, ParamterLength);
            sq.Direction = Direction;

            switch (ParamterType)
            {
                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.Float:
                case SqlDbType.Real:
                case SqlDbType.SmallMoney:
                    sq = SetValueDecimal(sq, Value);
                    break;
                case SqlDbType.Int:
                    sq = SetValueInt(sq, Value);
                    break;
                case SqlDbType.BigInt:
                    sq = SetValueBigInt(sq, Value);
                    break;
                case SqlDbType.TinyInt:
                    sq = SetValueTinyInt(sq, Value);
                    break;
                case SqlDbType.NVarChar:
                case SqlDbType.NChar:
                    sq = SetValue(sq, Value);
                    break;
                case SqlDbType.VarChar:
                case SqlDbType.Char:
                    sq = SetValue(sq, Value);
                    break;
                case SqlDbType.NText:
                case SqlDbType.Text:
                    sq = SetValue(sq, Value);
                    break;
                case SqlDbType.Xml:
                    sq = SetValue(sq, Value);
                    break;
                case SqlDbType.Bit:
                    sq = SetValueBool(sq, Value);
                    break;
                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    sq = SetValueDateTime(sq, Value);
                    break;
                case SqlDbType.SmallInt:
                    sq = SetValueSmallInt(sq, Value);
                    break;
                case SqlDbType.UniqueIdentifier:
                    sq = SetValueGUID(sq, Value);
                    break;


            }
            return sq;
        }

        public static SqlParameter[] CreateSQLParameterArray(SqlParameter[] spa, SqlParameter sp)
        {
            Array.Resize(ref spa, spa.Length +1);
            spa[spa.Length-1] = sp;
       
            return spa;
        }
       

        static public void ExecuteSQL(String Sql)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = DB.GetDBConn();
            dbconn.Open();
            SqlCommand cmd = new SqlCommand(Sql, dbconn);
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                dbconn.Close();
                dbconn.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                dbconn.Close();
                dbconn.Dispose();
                throw (ex);
            }
        }

        static public void ExecuteSQL(String Sql, SqlConnection dbconn)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlCommand cmd = new SqlCommand(Sql, dbconn);
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                throw (ex);
            }
        }

        static public void ExecuteSQL(String Sql, SqlTransaction trans)
        {
            if (trans == null)
            {
                ExecuteSQL(Sql);
                return;
            }
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlCommand cmd = new SqlCommand(Sql, trans.Connection, trans);
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                throw (ex);
            }
        }

        static public void ExecuteSQL(String Sql, SqlParameter[] spa, SqlTransaction trans)
        {
            if (trans == null)
            {
                ExecuteSQL(Sql, spa);
                return;
            }
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlCommand cmd = new SqlCommand(Sql, trans.Connection, trans);

            foreach (SqlParameter sp in spa)
            {
                cmd.Parameters.Add(sp);
            }

            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                throw (ex);
            }
        }

        public static void ExecuteSQL(String Sql, SqlParameter[] spa)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }

            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = GetDBConn();
            dbconn.Open();
            SqlCommand cmd = new SqlCommand(Sql, dbconn);
            cmd.Parameters.AddRange(spa);

            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                dbconn.Close();
                dbconn.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                dbconn.Close();
                dbconn.Dispose();
                throw (ex);
            }
        }

        public static void ExecuteSQL(SqlCommand cmd)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + cmd.CommandText + "\n");
            }
            using (SqlConnection xcon = new SqlConnection(DB.GetDBConn()))
            {
                cmd.Connection = xcon;
                xcon.Open();
                cmd.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Daemon for handling scalar SQL Queries
        /// </summary>
        /// <typeparam name="returnType">The type of value to return from the query</typeparam>
        public static class Scalar<returnType>
        {
            /// <summary>
            /// Executes a SQL Command and returns is Scalar Value
            /// </summary>
            /// <param name="cmd">the SQLCommand to execute</param>
            public static returnType ExecuteScalar(SqlCommand cmd)
            {
                using (SqlConnection conn = new SqlConnection(GetDBConn()))
                {
                    conn.Open();
                    cmd.Connection = conn;
                    return (returnType)cmd.ExecuteScalar();
                }
            }

        }

        // NOTE FOR DB ACCESSOR FUNCTIONS: AdminSite try/catch block is needed until
        // we convert to the new admin page styles. Our "old" db accessors handled empty
        // recordset conditions, so we need to preserve that for the admin site to add 
        // new products/categories/etc...
        //
        // We do not use try/catch on the store site for speed

        // ----------------------------------------------------------------
        //
        // SIMPLE ROW FIELD ROUTINES
        //
        // ----------------------------------------------------------------

        public static String RowField(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return String.Empty;
                    }
                    return Convert.ToString(row[fieldname]);
                }
                catch
                {
                    return String.Empty;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return String.Empty;
                }
                return Convert.ToString(row[fieldname]);
            }
        }

        public static String RowFieldByLocale(DataRow row, String fieldname, String LocaleSetting)
        {
            String tmpS = String.Empty;
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        tmpS = String.Empty;
                    }
                    else
                    {
                        tmpS = Convert.ToString(row[fieldname]);
                    }
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    tmpS = String.Empty;
                }
                else
                {
                    tmpS = Convert.ToString(row[fieldname]);
                }
            }

            return XmlCommon.GetLocaleEntry(tmpS, LocaleSetting, true);
        }

        // uses xpath spec to look into the field value and return a node innertext
        public static String RowFieldByXPath(DataRow row, String fieldname, String XPath)
        {
            String tmpS = String.Empty;
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        tmpS = String.Empty;
                    }
                    else
                    {
                        tmpS = Convert.ToString(row[fieldname]);
                    }
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    tmpS = String.Empty;
                }
                else
                {
                    tmpS = Convert.ToString(row[fieldname]);
                }
            }
            return XmlCommon.GetXPathEntry(tmpS, XPath);
        }

        public static bool RowFieldBool(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return false;
                    }

                    String s = row[fieldname].ToString();

                    return (s.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || 
                            s.Equals("YES", StringComparison.InvariantCultureIgnoreCase) || 
                            s.Equals("1", StringComparison.InvariantCultureIgnoreCase));
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return false;
                }

                String s = row[fieldname].ToString();

                return (s.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || 
                        s.Equals("YES", StringComparison.InvariantCultureIgnoreCase) || 
                        s.Equals("1", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public static Byte RowFieldByte(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return 0;
                    }
                    return Convert.ToByte(row[fieldname]);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return 0;
                }
                return Convert.ToByte(row[fieldname]);
            }
        }

        public static String RowFieldGUID(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return String.Empty;
                    }
                    return Convert.ToString(row[fieldname]);
                }
                catch
                {
                    return String.Empty;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return String.Empty;
                }
                return Convert.ToString(row[fieldname]);
            }
        }

        public static int RowFieldInt(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(row[fieldname]);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return 0;
                }
                return Convert.ToInt32(row[fieldname]);
            }
        }

        public static long RowFieldLong(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return 0;
                    }
                    return Convert.ToInt64(row[fieldname]);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return 0;
                }
                return Convert.ToInt64(row[fieldname]);
            }
        }

        public static Single RowFieldSingle(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return 0.0F;
                    }
                    return Convert.ToSingle(row[fieldname]);
                }
                catch
                {
                    return 0.0F;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return 0.0F;
                }
                return Convert.ToSingle(row[fieldname]);
            }
        }

        public static Double RowFieldDouble(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return 0.0F;
                    }
                    return Convert.ToDouble(row[fieldname]);
                }
                catch
                {
                    return 0.0F;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return 0.0F;
                }
                return Convert.ToDouble(row[fieldname]);
            }
        }

        public static Decimal RowFieldDecimal(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return System.Decimal.Zero;
                    }
                    return Convert.ToDecimal(row[fieldname]);
                }
                catch
                {
                    return System.Decimal.Zero;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return System.Decimal.Zero;
                }
                return Convert.ToDecimal(row[fieldname]);
            }
        }


        public static DateTime RowFieldDateTime(DataRow row, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    if (Convert.IsDBNull(row[fieldname]))
                    {
                        return System.DateTime.MinValue;
                    }
                    return Convert.ToDateTime(row[fieldname], SqlServerCulture);
                }
                catch
                {
                    return System.DateTime.MinValue;
                }
            }
            else
            {
                if (Convert.IsDBNull(row[fieldname]))
                {
                    return System.DateTime.MinValue;
                }
                return Convert.ToDateTime(row[fieldname], SqlServerCulture);
            }
        }

        // ----------------------------------------------------------------
        //
        // SIMPLE RS FIELD ROUTINES
        //
        // ----------------------------------------------------------------

        public static String RSField(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return String.Empty;
                    }
                    return rs.GetString(idx);
                }
                catch
                {
                    return String.Empty;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return String.Empty;
                }
                return rs.GetString(idx);
            }
        }

        public static String RSFieldByLocale(IDataReader rs, String fieldname, String LocaleSetting)
        {
            String tmpS = String.Empty;
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        tmpS = String.Empty;
                    }
                    else
                    {
                        tmpS = rs.GetString(idx);
                    }
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    tmpS = String.Empty;
                }
                else
                {
                    tmpS = rs.GetString(idx);
                }
            }
            if (LocaleSetting == Localization.ALL_LOCALES)
            {
                return tmpS;
            }
            else
            {
                return XmlCommon.GetLocaleEntry(tmpS, LocaleSetting, true);
            }
        }

        // uses xpath spec to look into the field value and return a node innertext
        public static String RSFieldByXPath(IDataReader rs, String fieldname, String XPath)
        {
            String tmpS = String.Empty;
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        tmpS = String.Empty;
                    }
                    else
                    {
                        tmpS = rs.GetString(idx);
                    }
                }
                catch
                {
                    tmpS = String.Empty;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    tmpS = String.Empty;
                }
                else
                {
                    tmpS = rs.GetString(idx);
                }
            }
            return XmlCommon.GetXPathEntry(tmpS, XPath);
        }

        public static bool RSFieldBool(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return false;
                    }

                    String s = rs[fieldname].ToString();

                    return (s.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || 
                            s.Equals("YES", StringComparison.InvariantCultureIgnoreCase) || 
                            s.Equals("1", StringComparison.InvariantCultureIgnoreCase));
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return false;
                }

                String s = rs[fieldname].ToString();

                return (s.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase) || 
                        s.Equals("YES", StringComparison.InvariantCultureIgnoreCase) || 
                        s.Equals("1", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public static String RSFieldGUID(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return String.Empty;
                    }
                    return rs.GetGuid(idx).ToString();
                }
                catch
                {
                    return String.Empty;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return String.Empty;
                }
                return rs.GetGuid(idx).ToString();
            }
        }

        public static Guid RSFieldGUID2(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return new Guid("00000000000000000000000000000000");
                    }
                    return rs.GetGuid(idx);
                }
                catch
                {
                    return new Guid("00000000000000000000000000000000");
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return new Guid("00000000000000000000000000000000");
                }
                return rs.GetGuid(idx);
            }
        }

        public static Byte RSFieldByte(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return 0;
                    }
                    return rs.GetByte(idx);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return 0;
                }
                return rs.GetByte(idx);
            }
        }

        public static int RSFieldInt(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return 0;
                    }
                    return rs.GetInt32(idx);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return 0;
                }
                return rs.GetInt32(idx);
            }
        }

        public static int RSFieldTinyInt(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return 0;
                    }
                    return Localization.ParseNativeInt(rs[idx].ToString());
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return 0;
                }
                return Localization.ParseNativeInt(rs[idx].ToString());
            }
        }

        public static long RSFieldLong(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return 0;
                    }
                    return rs.GetInt64(idx);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return 0;
                }
                return rs.GetInt64(idx);
            }
        }

        public static Single RSFieldSingle(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return 0.0F;
                    }
                    return (Single)rs.GetDouble(idx); // SQL server seems to fail the GetFloat calls, so we have to do this
                }
                catch
                {
                    return 0.0F;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return 0.0F;
                }
                return (Single)rs.GetDouble(idx); // SQL server seems to fail the GetFloat calls, so we have to do this
            }
        }

        public static Double RSFieldDouble(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return 0.0F;
                    }
                    return rs.GetDouble(idx);
                }
                catch
                {
                    return 0.0F;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return 0.0F;
                }
                return rs.GetDouble(idx);
            }
        }

        public static Decimal RSFieldDecimal(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return System.Decimal.Zero;
                    }
                    return rs.GetDecimal(idx);
                }
                catch
                {
                    return System.Decimal.Zero;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return System.Decimal.Zero;
                }
                return rs.GetDecimal(idx);
            }
        }

        public static DateTime RSFieldDateTime(IDataReader rs, String fieldname)
        {
            if (AppLogic.IsAdminSite)
            {
                try
                {
                    int idx = rs.GetOrdinal(fieldname);
                    if (rs.IsDBNull(idx))
                    {
                        return System.DateTime.MinValue;
                    }
                    return Convert.ToDateTime(rs[idx], SqlServerCulture);
                 
                }
                catch
                {
                    return System.DateTime.MinValue;
                }
            }
            else
            {
                int idx = rs.GetOrdinal(fieldname);
                if (rs.IsDBNull(idx))
                {
                    return System.DateTime.MinValue;
                }
                return Convert.ToDateTime(rs[idx], SqlServerCulture);
             
            }
        }

        public static DataSet GetTable(String tablename, String orderBy, String cacheName, bool useCache)
        {
            if (useCache)
            {
                DataSet cacheds = (DataSet)HttpContext.Current.Cache.Get(cacheName);
                if (cacheds != null)
                {
                    return cacheds;
                }
            }
            DataSet ds = new DataSet();
            String Sql = String.Empty;
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = DB.GetDBConn();
            dbconn.Open();
            Sql = "select * from " + tablename + " order by " + orderBy;
            SqlDataAdapter da = new SqlDataAdapter(Sql, dbconn);
            da.Fill(ds, tablename);
            dbconn.Close();
            if (useCache)
            {
                HttpContext.Current.Cache.Insert(cacheName, ds);
            }
            return ds;
        }

        /// <summary>
        /// Incrementally adds tables results to a dataset
        /// </summary>
        /// <param name="ds">Dataset to add the table to</param>
        /// <param name="tableName">Name of the table to be created in the DataSet</param>
        /// <param name="sqlQuery">Query to retrieve the data for the table.</param>
        static public int FillDataSet(DataSet ds, string tableName, string sqlQuery)
        {
            int n = 0;
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = DB.GetDBConn();
            dbconn.Open();
            SqlDataAdapter da = new SqlDataAdapter(sqlQuery, dbconn);
            n = da.Fill(ds, tableName);
            dbconn.Close();
            return n;
        }

        public static String GetNewGUID()
        {
            return System.Guid.NewGuid().ToString();
        }

        static public int GetSqlN(String Sql)
        {
            SqlParameter[] spa = new SqlParameter[0];
            return GetSqlN(Sql, spa);
        }

        static public int GetSqlN(String Sql, SqlParameter[] spa)
        {
            int N = 0;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, spa, con))
                {
                    if (rs.Read())
                    {
                        N = DB.RSFieldInt(rs, "N");
                    }
                }
            }

            return N;
        }


		static public long GetSqlNLong(String Sql)
		{
			long N = 0;

			using(SqlConnection con = new SqlConnection(DB.GetDBConn()))
			{
				con.Open();
				using(IDataReader rs = DB.GetRS(Sql, con))
				{
					if(rs.Read())
					{
						N = DB.RSFieldLong(rs, "N");
					}
				}
			}

			return N;
		}

		static public Single GetSqlNSingle(String Sql)
        {
            Single N = 0.0F;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, con))
                {
                    if (rs.Read())
                    {
                        N = DB.RSFieldSingle(rs, "N");
                    }
                }
            }

            return N;
        }

        static public decimal GetSqlNDecimal(String Sql)
        {
            decimal N = System.Decimal.Zero;
  
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, con))
                {
                    if (rs.Read())
                    {
                        N = DB.RSFieldDecimal(rs, "N");
                    }
                }
            }

            return N;
        }

        static public decimal GetSqlNDecimal(String Sql, SqlParameter[] spa)
        {
            decimal N = System.Decimal.Zero;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, spa, con))
                {
                    if (rs.Read())
                    {
                        N = DB.RSFieldDecimal(rs, "N");
                    }
                }
            }

            return N;
        }

        static public void ExecuteLongTimeSQL(String Sql, int TimeoutSecs)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = DB.GetDBConn();
            dbconn.Open();
            SqlCommand cmd = new SqlCommand(Sql, dbconn);
            cmd.CommandTimeout = TimeoutSecs;
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                dbconn.Close();
                dbconn.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                dbconn.Close();
                dbconn.Dispose();
                throw (ex);
            }
        }

        static public String GetSqlS(String Sql)
        {
            String S = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, con))
                {
                    if (rs.Read())
                    {
                        S = DB.RSFieldByLocale(rs, "S", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                        if (S.Equals(DBNull.Value))
                        {
                            S = String.Empty;
                        }
                    }
                }
            }

            return S;
        }

        static public int GetSqlN(String Sql, SqlConnection dbconn)
        {
            int N = 0;
            using (IDataReader rs = DB.GetRS(Sql, dbconn))
            {
                if (rs.Read())
                {
                    N = DB.RSFieldInt(rs, "N");
                }
            }
            
            return N;
        }

        static public int GetSqlN(String Sql, SqlTransaction trans)
        {
            if (trans == null)
            {
                return GetSqlN(Sql);
            }
            int N = 0;

            using (IDataReader rs = DB.GetRS(Sql, trans))
            {
                if (rs.Read())
                {
                    N = DB.RSFieldInt(rs, "N");
                }
            }
            
            return N;
        }

        static public Single GetSqlNSingle(String Sql, SqlConnection dbconn)
        {
            Single N = 0.0F;
            using (IDataReader rs = DB.GetRS(Sql, dbconn))
            {
                if (rs.Read())
                {
                    N = DB.RSFieldSingle(rs, "N");
                }
            }
            
            return N;
        }

        static public Single GetSqlNSingle(String Sql, SqlTransaction trans)
        {
            if (trans == null)
            {
                return GetSqlNSingle(Sql);
            }
            Single N = 0.0F;
            using (IDataReader rs = DB.GetRS(Sql, trans))
            {
                if (rs.Read())
                {
                    N = DB.RSFieldSingle(rs, "N");
                }
            }
            
            return N;
        }

        static public decimal GetSqlNDecimal(String Sql, SqlConnection dbconn)
        {
            decimal N = System.Decimal.Zero;

            using (IDataReader rs = DB.GetRS(Sql, dbconn))
            {
                if (rs.Read())
                {
                    N = DB.RSFieldDecimal(rs, "N");
                }
            }
            
            return N;
        }

        static public decimal GetSqlNDecimal(String Sql, SqlTransaction trans)
        {
            if (trans == null)
            {
                return GetSqlNDecimal(Sql);
            }

            decimal N = System.Decimal.Zero;

            using (IDataReader rs = DB.GetRS(Sql, trans))
            {
                if (rs.Read())
                {
                    N = DB.RSFieldDecimal(rs, "N");
                }
            }
            
            return N;
        }

        static public void ExecuteLongTimeSQL(String Sql, int TimeoutSecs, SqlConnection dbconn)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }

            SqlCommand cmd = new SqlCommand(Sql, dbconn);
            cmd.CommandTimeout = TimeoutSecs;
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                throw (ex);
            }
        }

        static public void ExecuteLongTimeSQL(String Sql, int TimeoutSecs, SqlTransaction trans)
        {
            if (trans == null)
            {
                ExecuteLongTimeSQL(Sql, TimeoutSecs);
                return;
            }
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlCommand cmd = new SqlCommand(Sql, trans.Connection, trans);
            cmd.CommandTimeout = TimeoutSecs;
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                throw (ex);
            }
        }

        static public String GetSqlS(String Sql, SqlConnection dbconn)
        {
            String S = String.Empty;
            using (IDataReader rs = DB.GetRS(Sql, dbconn))
            {
                if (rs.Read())
                {
                    S = DB.RSFieldByLocale(rs, "S", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                    if (S.Equals(DBNull.Value))
                    {
                        S = String.Empty;
                    }
                }
            }            

            return S;
        }

        static public String GetSqlS(String Sql, SqlTransaction trans)
        {
            if (trans == null)
            {
                return GetSqlS(Sql);
            }
            return GetSqlS(Sql, trans.Connection);
        }

        static public String GetSqlSAllLocales(String Sql)
        {
            String S = String.Empty;

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, con))
                {
                    if (rs.Read())
                    {
                        S = DB.RSField(rs, "S");
                        if (S.Equals(DBNull.Value))
                        {
                            S = String.Empty;
                        }
                    }
                }
            }

            return S;
        }

        static public string GetSqlXml(String Sql, string xslTranformFile)
        {
            if (Sql.ToUpper(CultureInfo.InvariantCulture).IndexOf("FOR XML") < 1)
            {
                Sql += " FOR XML AUTO";
            }
            StringBuilder s = new StringBuilder(4096);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, con))
                {
                    while (rs.Read())
                    {
                        s.Append(rs.GetString(0));
                    }
                }
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml("<root>" + s.ToString() + "</root>");
            if (xslTranformFile != null && xslTranformFile.Trim() != "")
            {
                XslCompiledTransform xsl = new XslCompiledTransform();
                xsl.Load(xslTranformFile);
                TextWriter tw = new StringWriter();
                xsl.Transform(xdoc, null, tw);
                return tw.ToString();
            }
            else
            {
                return xdoc.DocumentElement.InnerXml;
            }
        }

        static public XmlDocument GetSqlXmlFromProc(String ProcInvocation)
        {
            StringBuilder s = new StringBuilder(4096);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(ProcInvocation, con))
                {
                    while (rs.Read())
                    {
                        s.Append(rs.GetString(0));
                    }
                }
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(s.ToString());
            return xdoc;
        }

        static public XmlDocument GetSqlXmlDoc(String Sql, string xslTranformFile)
        {
            if (Sql.ToUpper(CultureInfo.InvariantCulture).IndexOf("FOR XML") < 1)
            {
                Sql += " FOR XML AUTO";
            }
            StringBuilder s = new StringBuilder(4096);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, con))
                {
                    while (rs.Read())
                    {
                        s.Append(rs.GetString(0));
                    }
                }
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml("<root>" + s.ToString() + "</root>");
            if (xslTranformFile != null && xslTranformFile.Trim() != "")
            {
                XslCompiledTransform xsl = new XslCompiledTransform();
                xsl.Load(xslTranformFile);
                TextWriter tw = new StringWriter();
                xsl.Transform(xdoc, null, tw);
                xdoc.LoadXml(tw.ToString());
            }
            return xdoc;
        }

        static public string GetSqlXml(String Sql, XslCompiledTransform xsl, XsltArgumentList xslArgs)
        {
            if (Sql.IndexOf("for xml", StringComparison.InvariantCultureIgnoreCase) < 1)
            {
                Sql += " for xml auto";
            }

            StringBuilder s = new StringBuilder(4096);

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, con))
                {
                    while (rs.Read())
                    {
                        s.Append(rs.GetString(0));
                    }
                }
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml("<root>" + s.ToString() + "</root>");
            if (xsl != null)
            {
                TextWriter tw = new StringWriter();
                xsl.Transform(xdoc, xslArgs, tw);
                return tw.ToString();
            }
            else
            {
                return xdoc.DocumentElement.InnerXml;
            }
        }

        static public int GetXml(IDataReader dr, string rootEl, string rowEl, bool IsPagingProc, ref StringBuilder Xml)
        {
            int rows = 0;
            if (rootEl.Length != 0)
            {
                Xml.Append("<");
                Xml.Append(rootEl);
                Xml.Append(">");
            }
            while (dr.Read())
            {
                ++rows;
                if (rowEl.Length == 0)
                {
                    Xml.Append("<row>");
                }
                else
                {
                    Xml.Append("<");
                    Xml.Append(rowEl);
                    Xml.Append(">");
                }
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string elname = dr.GetName(i).Replace(" ", "_");
                    if (dr.IsDBNull(i))
                    {
                        Xml.Append("<");
                        Xml.Append(elname);
                        Xml.Append("/>");
                    }
                    else
                    {
                        if (Convert.ToString(dr.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Xml.Append("<");
                            Xml.Append(elname);
                            Xml.Append(">");
                            Xml.Append(Convert.ToString(dr.GetValue(i)));
                            Xml.Append("</");
                            Xml.Append(elname);
                            Xml.Append(">");

                        }
                        else
                        {
                            if (dr.GetFieldType(i).Equals(DbType.DateTime))
                            {
                                Xml.Append("<");
                                Xml.Append(elname);
                                Xml.Append(">");
                                Xml.Append(Localization.ParseLocaleDateTime(dr.GetString(i), Thread.CurrentThread.CurrentUICulture.Name));
                                Xml.Append("</");
                                Xml.Append(elname);
                                Xml.Append(">");
                            }
                            else
                            {
                                Xml.Append("<");
                                Xml.Append(elname);
                                Xml.Append(">");
                                Xml.Append(XmlCommon.XmlEncodeAsIs(Convert.ToString(dr.GetValue(i))));
                                Xml.Append("</");
                                Xml.Append(elname);
                                Xml.Append(">");

                            }
                        }
                    }
                }
                if (rowEl.Length == 0)
                {
                    Xml.Append("</row>");
                }
                else
                {
                    Xml.Append("</");
                    Xml.Append(rowEl);
                    Xml.Append(">");
                }

            }
            if (rootEl.Length != 0)
            {
                Xml.Append("</");
                Xml.Append(rootEl);
                Xml.Append(">");
            }
            int ResultSet = 1;
            while (dr.NextResult())
            {
                ResultSet++;
                if (IsPagingProc)
                {
                    if (rootEl.Length != 0)
                    {
                        Xml.Append("<");
                        Xml.Append(rootEl);
                        Xml.Append("Paging>");
                    }
                    else
                    {
                        Xml.Append("<Paging>");
                    }
                }
                else
                {
                    if (rootEl.Length != 0)
                    {
                        Xml.Append("<");
                        Xml.Append(rootEl);
                        Xml.Append(ResultSet.ToString());
                        Xml.Append(">");
                    }
                }
                while (dr.Read())
                {
                    if (!IsPagingProc)
                    {
                        if (rowEl.Length == 0)
                        {
                            Xml.Append("<row>");
                        }
                        else
                        {
                            Xml.Append("<");
                            Xml.Append(rowEl);
                            Xml.Append(">");
                        }
                    }
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        string elname = dr.GetName(i).Replace(" ", "_");
                        if (dr.IsDBNull(i))
                        {
                            Xml.Append("<");
                            Xml.Append(elname);
                            Xml.Append("/>");
                        }
                        else
                        {
                            if (System.Convert.ToString(dr.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Xml.Append("<");
                                Xml.Append(elname);
                                Xml.Append(">");
                                Xml.Append(System.Convert.ToString(dr.GetValue(i)));
                                Xml.Append("</"); 
                                Xml.Append(elname);
                                Xml.Append(">");
                            }
                            else
                            {
                                Xml.Append("<");
                                Xml.Append(elname);
                                Xml.Append(">");
                                Xml.Append(XmlCommon.XmlEncodeAsIs(System.Convert.ToString(dr.GetValue(i))));
                                Xml.Append("</");
                                Xml.Append(elname);
                                Xml.Append(">");
                            }
                        }
                    }
                    if (!IsPagingProc)
                    {
                        if (rowEl.Length == 0)
                        {
                            Xml.Append("</row>");
                        }
                        else
                        {
                            Xml.Append("</");
                            Xml.Append(rowEl);
                            Xml.Append(">");
                        }
                    }
                }
                if (IsPagingProc)
                {
                    if (rootEl.Length != 0)
                    {
                        Xml.Append("</");
                        Xml.Append(rootEl);
                        Xml.Append("Paging>");
                    }
                    else
                    {
                        Xml.Append("</Paging>");
                    }
                }
                else
                {
                    if (rootEl.Length != 0)
                    {
                        Xml.Append("</");
                        Xml.Append(rootEl);
                        Xml.Append(ResultSet.ToString());
                        Xml.Append(">");
                    }
                }
            }
            dr.Close();
            return rows;
        }


        // assumes a 2nd result set is the PAGING INFO back from the aspdnsf_PageQuery proc!!!
        // looks for aspdnsf_PageQuery in the Sql input to determine this!
        static public int GetXml(string Sql, string rootEl, string rowEl, ref string Xml)
        {
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = DB.GetDBConn();
            dbconn.Open();
            int i = GetXml(Sql, rootEl, rowEl, ref Xml, dbconn);
            dbconn.Close();
            dbconn.Dispose();
            return i;
        }


        // assumes a 2nd result set is the PAGING INFO back from the aspdnsf_PageQuery proc!!!
        // looks for aspdnsf_PageQuery in the Sql input to determine this!
        static public int GetXml(string Sql, string rootEl, string rowEl, ref string Xml, SqlConnection dbconn)
        {
            bool IsPagingProc = (Sql.IndexOf("aspdnsf_PageQuery") != -1);
            StringBuilder s = new StringBuilder(4096);

            int rows = 0;
            if (rootEl.Length != 0)
            {
                s.Append("<");
                s.Append(rootEl);
                s.Append(">");
            }

            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader rs = DB.GetRS(Sql, con))
                {
                    while (rs.Read())
                    {
                        ++rows;
                        if (rowEl.Length == 0)
                        {
                            s.Append("<row>");
                        }
                        else
                        {
                            s.Append("<");
                            s.Append(rowEl);
                            s.Append(">");
                        }
                        for (int i = 0; i < rs.FieldCount; i++)
                        {
                            string elname = rs.GetName(i).Replace(" ", "_");
                            if (rs.IsDBNull(i))
                            {
                                s.Append("<");
                                s.Append(elname);
                                s.Append("/>");
                            }
                            else
                            {
                                if (System.Convert.ToString(rs.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    s.Append("<");
                                    s.Append(elname);
                                    s.Append(">");
                                    s.Append(System.Convert.ToString(rs.GetValue(i)));
                                    s.Append("</");
                                    s.Append(elname);
                                    s.Append(">");
                                }
                                else
                                {
                                    s.Append("<");
                                    s.Append(elname);
                                    s.Append(">");
                                    s.Append(System.Convert.ToString(rs.GetValue(i)).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;"));
                                    s.Append("</");
                                    s.Append(elname);
                                    s.Append(">");
                                }
                            }
                        }
                        if (rowEl.Length == 0)
                        {
                            s.Append("</row>");
                        }
                        else
                        {
                            s.Append("</");
                            s.Append(rowEl);
                            s.Append(">");
                        }

                    }
                    if (rootEl.Length != 0)
                    {
                        s.Append("</");
                        s.Append(rootEl);
                        s.Append(">");
                    }
                    int ResultSet = 1;
                    while (rs.NextResult())
                    {
                        ResultSet++;
                        if (IsPagingProc)
                        {
                            if (rootEl.Length != 0)
                            {
                                s.Append("<");
                                s.Append(rootEl);
                                s.Append("Paging>");
                            }
                            else
                            {
                                s.Append("<Paging>");
                            }
                        }
                        else
                        {
                            if (rootEl.Length != 0)
                            {
                                s.Append("<");
                                s.Append(rootEl);
                                s.Append(ResultSet.ToString());
                                s.Append(">");
                            }
                        }
                        while (rs.Read())
                        {
                            if (!IsPagingProc)
                            {
                                if (rowEl.Length == 0)
                                {
                                    s.Append("<row>");
                                }
                                else
                                {
                                    s.Append("<");
                                    s.Append(rowEl);
                                    s.Append(">");
                                }
                            }
                            for (int i = 0; i < rs.FieldCount; i++)
                            {
                                string elname = rs.GetName(i).Replace(" ", "_");
                                if (rs.IsDBNull(i))
                                {
                                    s.Append("<");
                                    s.Append(elname);
                                    s.Append("/>");
                                }
                                else
                                {
                                    if (System.Convert.ToString(rs.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        s.Append("<");
                                        s.Append(elname);
                                        s.Append(">");
                                        s.Append(System.Convert.ToString(rs.GetValue(i)));
                                        s.Append("</");
                                        s.Append(elname);
                                        s.Append(">");
                                    }
                                    else
                                    {
                                        s.Append("<");
                                        s.Append(elname);
                                        s.Append(">");
                                        s.Append(System.Convert.ToString(rs.GetValue(i)).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;"));
                                        s.Append("</");
                                        s.Append(elname);
                                        s.Append(">");
                                    }
                                }
                            }
                            if (!IsPagingProc)
                            {
                                if (rowEl.Length == 0)
                                {
                                    s.Append("</row>");
                                }
                                else
                                {
                                    s.Append("</");
                                    s.Append(rowEl);
                                    s.Append(">");
                                }
                            }
                        }
                        if (IsPagingProc)
                        {
                            if (rootEl.Length != 0)
                            {
                                s.Append("</");
                                s.Append(rootEl);
                                s.Append("Paging>");

                            }
                            else
                            {
                                s.Append("</Paging>");
                            }
                        }
                        else
                        {
                            if (rootEl.Length != 0)
                            {
                                s.Append("</");
                                s.Append(rootEl);
                                s.Append(ResultSet.ToString());
                                s.Append(">");
                            }
                        }
                    }
                }
            }

            Xml = s.ToString();
            return rows;
        }

        static public int GetXml(string Sql, string rootEl, string rowEl, ref string Xml, SqlTransaction trans)
        {
            if (trans == null)
            {
                return GetXml(Sql, rootEl, rowEl, ref Xml);
            }
            return GetXml(Sql, rootEl, rowEl, ref Xml, trans.Connection);
        }

        static public int GetENLocaleXml(SqlDataReader dr, string rootEl, string rowEl, ref StringBuilder Xml)
        {
            int rows = 0;
            if (rootEl.Length != 0)
            {
                Xml.Append("<");
                Xml.Append(rootEl);
                Xml.Append(">");
            }
            while (dr.Read())
            {
                ++rows;
                if (rowEl.Length == 0)
                {
                    Xml.Append("<row>");
                }
                else
                {
                    Xml.Append("<");
                    Xml.Append(rowEl);
                    Xml.Append(">");
                }
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string elname = dr.GetName(i).Replace(" ", "_");
                    if (dr.IsDBNull(i))
                    {
                        Xml.Append("<");
                        Xml.Append(elname);
                        Xml.Append("/>");
                    }
                    else
                    {
                        if (System.Convert.ToString(dr.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Xml.Append("<");
                                Xml.Append(elname);
                                Xml.Append(">");
                                Xml.Append(System.Convert.ToString(dr.GetValue(i)));
                                Xml.Append("</" );
                                Xml.Append(elname);
                                Xml.Append(">");
                        }
                        else
                        {
                            string FieldVal = string.Empty;
                            string FieldDataType = dr.GetDataTypeName(i);
                            if ((FieldDataType.Equals("decimal", StringComparison.InvariantCultureIgnoreCase) || 
                                FieldDataType.Equals("money", StringComparison.InvariantCultureIgnoreCase)) && 
                                CommonLogic.Application("DBSQLServerLocaleSetting") != "en-US")
                            {
                                FieldVal = Localization.ParseLocaleDecimal(dr.GetDecimal(i).ToString(), "en-US").ToString();
                            }
                            else if (dr.GetDataTypeName(i).Equals("datetime", StringComparison.InvariantCultureIgnoreCase) && 
                                CommonLogic.Application("DBSQLServerLocaleSetting") != "en-US")
                            {
                                FieldVal = Localization.ParseLocaleDateTime(dr.GetDateTime(i).ToString(), "en-US").ToString();
                            }
                            else
                            {
                                FieldVal = dr.GetValue(i).ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;");
                            }
                            Xml.Append("<");
                            Xml.Append(elname);
                            Xml.Append(">");
                            Xml.Append(FieldVal);
                            Xml.Append("</");
                            Xml.Append(elname);
                            Xml.Append(">");
                        }
                    }
                }
                if (rowEl.Length == 0)
                {
                    Xml.Append("</row>");
                }
                else
                {
                    Xml.Append("</");
                    Xml.Append(rowEl);
                    Xml.Append(">");
                }

            }
            if (rootEl.Length != 0)
            {
                Xml.Append("</");
                Xml.Append(rootEl);
                Xml.Append(">");
            }
            int ResultSet = 1;
            while (dr.NextResult())
            {
                ResultSet++;
                if (rootEl.Length != 0)
                {
                    Xml.Append("<");
                    Xml.Append(rootEl);
                    Xml.Append(ResultSet.ToString());
                    Xml.Append(">");
                }
                while (dr.Read())
                {
                    if (rowEl.Length == 0)
                    {
                        Xml.Append("<row>");
                    }
                    else
                    {
                        Xml.Append("<");
                        Xml.Append(rowEl);
                        Xml.Append(">");
                    }
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        string elname = dr.GetName(i).Replace(" ", "_");
                        if (dr.IsDBNull(i))
                        {
                            Xml.Append("<");
                            Xml.Append(elname);
                            Xml.Append("/>");
                        }
                        else
                        {
                            if (System.Convert.ToString(dr.GetValue(i)).StartsWith("<ml>", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Xml.Append("<");
                                Xml.Append(elname);
                                Xml.Append(">");
                                Xml.Append(System.Convert.ToString(dr.GetValue(i)));
                                Xml.Append("</");
                                Xml.Append(elname);
                                Xml.Append(">");
                            }
                            else
                            {
                                string FieldVal = string.Empty;
                                if (dr.GetDataTypeName(i).Equals("decimal", StringComparison.InvariantCultureIgnoreCase) && 
                                    CommonLogic.Application("DBSQLServerLocaleSetting") != "en-US")
                                {
                                    FieldVal = Localization.ParseLocaleDecimal(dr.GetDecimal(i).ToString(), "en-US").ToString();
                                }
                                else if (dr.GetDataTypeName(i).Equals("datetime", StringComparison.InvariantCultureIgnoreCase) && 
                                    CommonLogic.Application("DBSQLServerLocaleSetting") != "en-US")
                                {
                                    FieldVal = Localization.ParseLocaleDateTime(dr.GetDateTime(i).ToString(), "en-US").ToString();
                                }
                                else
                                {
                                    FieldVal = dr.GetValue(i).ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;");
                                }
                                Xml.Append("<");
                                Xml.Append(elname);
                                Xml.Append(">");
                                Xml.Append(System.Convert.ToString(dr.GetValue(i)).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;"));
                                Xml.Append("</");
                                Xml.Append(elname);
                                Xml.Append(">");
                            }
                        }
                    }
                    if (rowEl.Length == 0)
                    {
                        Xml.Append("</row>");
                    }
                    else
                    {
                        Xml.Append("</");
                        Xml.Append(rowEl);
                        Xml.Append(">");
                    }
                }
                if (rootEl.Length != 0)
                {
                    Xml.Append("</");
                    Xml.Append(rootEl);
                    Xml.Append(ResultSet.ToString());
                    Xml.Append(">");
                }
            }
            dr.Close();
            return rows;
        }

        static public void ExecuteLongTimeSQL(String Sql, String DBConnString, int TimeoutSecs)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlConnection dbconn = new SqlConnection();
            dbconn.ConnectionString = DB.GetDBConn();
            dbconn.Open();
            SqlCommand cmd = new SqlCommand(Sql, dbconn);
            cmd.CommandTimeout = TimeoutSecs;
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                dbconn.Close();
                dbconn.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                dbconn.Close();
                dbconn.Dispose();
                throw (ex);
            }
        }

        static public void ExecuteLongTimeSQL(String Sql, String DBConnString, int TimeoutSecs, SqlConnection dbconn)
        {
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlCommand cmd = new SqlCommand(Sql, dbconn);
            cmd.CommandTimeout = TimeoutSecs;
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                throw (ex);
            }
        }

        static public void ExecuteLongTimeSQL(String Sql, String DBConnString, int TimeoutSecs, SqlTransaction trans)
        {
            if (trans == null)
            {
                ExecuteLongTimeSQL(Sql, DBConnString, TimeoutSecs);
                return;
            }
            if (CommonLogic.ApplicationBool("DumpSQL"))
            {
                HttpContext.Current.Response.Write("SQL=" + Sql + "\n");
            }
            SqlCommand cmd = new SqlCommand(Sql, trans.Connection, trans);
            cmd.CommandTimeout = TimeoutSecs;
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                cmd.Dispose();
                throw (ex);
            }
        }


        static public void LogSql(string LogText)
        {
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            try
            {
                cn.Open();
            }
            catch { return; }
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "insert dbo.sqllog (SQLText, ExecutedBy, ExecutedOn) values(@logText, 1, getdate())";
            cmd.Parameters.Add(new SqlParameter("@logText", SqlDbType.NText, 1000000000));
            cmd.Parameters["@logText"].Value = LogText;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch { }

            cmd.Dispose();
            cn.Close();
            cn.Close();

        }

        public static String GetTimeSpanSql(String RangeType, DateTime SpecifiedStartDate, DateTime SpecifiedEndDate, String FieldToCompare, Boolean IncludeLeadingAnd, out DateTime ResultingStartDate, out DateTime ResultingEndDate)
        {
            try
            {
                DateRangeType parsedType;
                parsedType =(DateRangeType) Enum.Parse(typeof(DateRangeType), RangeType);
                return GetTimeSpanSql(parsedType, SpecifiedStartDate, SpecifiedEndDate, FieldToCompare, IncludeLeadingAnd, out ResultingStartDate, out ResultingEndDate);
            }
            catch (Exception)
            {
                throw new ArgumentException("RangeType is not valid.");
            }
        }

        public static String GetTimeSpanSql(DateRangeType RangeType, DateTime SpecifiedStartDate, DateTime SpecifiedEndDate, String FieldToCompare, Boolean IncludeLeadingAnd, out DateTime ResultingStartDate, out DateTime ResultingEndDate)
        {
            ResultingStartDate = DateTime.Today.AddDays(-7);
            ResultingEndDate = DateTime.Today.AddDays(1);
            switch (RangeType)
            {
                case DateRangeType.UseDatesAbove:
                    ResultingStartDate = SpecifiedStartDate;
                    ResultingEndDate = SpecifiedEndDate;
                    break;
                case DateRangeType.Today:
                    ResultingStartDate = DateTime.Today;
                    ResultingEndDate = DateTime.Today.AddDays(1);
                    break;
                case DateRangeType.Yesterday:
                    ResultingStartDate = System.DateTime.Today.AddDays(-1);
                    ResultingEndDate = System.DateTime.Today;
                    break;
                case DateRangeType.ThisWeek:
                    ResultingStartDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek));
                    ResultingEndDate = ResultingStartDate.AddDays(7);
                    break;
                case DateRangeType.LastWeek:
                    ResultingStartDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek) - 7);
                    ResultingEndDate = ResultingStartDate.AddDays(7);
                    break;
                case DateRangeType.ThisMonth:
                    ResultingStartDate = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                    ResultingEndDate = ResultingStartDate.AddMonths(1);
                    break;
                case DateRangeType.LastMonth:
                    ResultingStartDate = DateTime.Today.AddMonths(-1);
                    ResultingStartDate = ResultingStartDate.AddDays(1 - ResultingStartDate.Day);
                    ResultingEndDate = ResultingStartDate.AddMonths(1);
                    break;
                case DateRangeType.ThisYear:
                    ResultingStartDate = DateTime.Today.AddMonths(1 - DateTime.Today.Month);
                    ResultingStartDate = ResultingStartDate.AddDays(1 - ResultingStartDate.Day);
                    ResultingEndDate = ResultingStartDate.AddYears(1);
                    break;
                case DateRangeType.LastYear:
                    ResultingStartDate = DateTime.Today.AddYears(-1);
                    ResultingStartDate = ResultingStartDate.AddMonths(1 - ResultingStartDate.Month);
                    ResultingStartDate = ResultingStartDate.AddDays(1 - ResultingStartDate.Day);
                    ResultingEndDate = ResultingStartDate.AddYears(1);
                    break;
                default:
                    break;
            }
            return String.Format(CommonLogic.IIF(IncludeLeadingAnd, " and", "") + " (({2}>={0}) and ({2} < {1}))", DB.DateQuote(Localization.ToDBShortDateString(ResultingStartDate)), DB.DateQuote(Localization.ToDBShortDateString(ResultingEndDate)), FieldToCompare);
        }
    }

    // currently only supported for SQL Server:
    public class DBTransaction
    {
        private ArrayList sqlCommands = new ArrayList(10);

        public DBTransaction() { }

        public void AddCommand(String Sql)
        {
            sqlCommands.Add(Sql);
        }

        public void ClearCommands()
        {
            sqlCommands.Clear();
        }

        // returns true if no errors, or false if ANY Exception is found:
        public bool Commit()
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = DB.GetDBConn();
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            bool status = false;
            try
            {

                foreach (String s in sqlCommands)
                {
                    SqlCommand comm = new SqlCommand(s, conn);
                    comm.Transaction = trans;
                    comm.ExecuteNonQuery();
                }
                trans.Commit();
                status = true;
            }
            catch 
            {
                trans.Rollback();
                status = false;
            }
            finally
            {
                conn.Close();
            }
            return status;
        }

    }

    public enum DateRangeType
    {
        UseDatesAbove,
        Today,
        Yesterday,
        ThisWeek,
        LastWeek,
        ThisMonth,
        LastMonth,
        ThisYear,
        LastYear
    }
}
