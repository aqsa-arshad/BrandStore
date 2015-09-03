// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
    #region enums

    /// <summary>
    /// Used by the database log provider to convey the severity of the message
    /// 0 = Unknown     Default value.  Developers should never explicitly use this value.
    /// 1 = Message     Informational message, such as application start completion
    /// 2 = Alert       Medium severity message, such as a potential misconfiguration
    /// 3 = Error       An unhandled exception
    /// </summary>
    public enum MessageSeverityEnum : int
    {
        Unknown = 0,
        Message = 1,
        Alert = 2,
        Error = 3
    }

    /// <summary>
    /// Used by the database log provider to convey the type of message being logged
    /// 0 = Unknown             Default value.  Developers should never explicitly use this.
    /// 1 = Informational       An informational message or alert
    /// 2 = GeneralException    A general (generic) unhandled exception
    /// 3 = DatabaseException   An exception reading from or writing to the database
    /// 4 = XMLPackageException An exception that occurred within an XMLPackage ExtensionFunction
    /// </summary>
    public enum MessageTypeEnum : int
    {
        Unknown = 0,
        Informational = 1,
        GeneralException = 2,
        DatabaseException = 3,
        XmlPackageException = 4
    }

    #endregion

    /// <summary>
    /// Class for logging informational alerts and exceptions to the database
    /// This class is NOT for use logging PA-DSS related information, such as admin login attempts
    /// </summary>
    public class SysLog
    {
        #region private variables

        private int m_SysLogID = 0;
        private int m_maxEntries = AppLogic.AppConfigNativeInt("System.MaxLogEntries");
        private int m_maxDays = AppLogic.AppConfigNativeInt("System.MaxLogDays");
        private string m_message = String.Empty;
        private string m_details = String.Empty;
        private MessageTypeEnum m_messageType = MessageTypeEnum.Unknown;
        private MessageSeverityEnum m_messageSeverity = MessageSeverityEnum.Unknown;
        private DateTime m_createdOn = System.DateTime.MinValue;


        #endregion

        #region constructors

        /// <summary>
        /// Instantiates a new instance of the SysLog
        /// </summary>
        public SysLog()
        {}
        
        #endregion

        #region non-static methods

        /// <summary>
        /// Ages and prunes the log based on the System.MaxLogEntries and System.MaxLogDays AppConfigs
        /// </summary>
        private void Age()
        {
            int currentErrors = DB.GetSqlN("select count(*) as N from aspdnsf_SysLog");

            DateTime earliestDayToKeep = System.DateTime.Now.AddDays((this.MaxLogDays * -1));
            DB.ExecuteSQL("delete from aspdnsf_syslog where createdon < " + DB.SQuote(Localization.DateStringForDB(earliestDayToKeep)));

            //Make space for new entries
            int entriesToDelete = currentErrors - this.MaxLogEntries;
            if (entriesToDelete > 0)
            {
                DB.ExecuteSQL("delete from aspdnsf_syslog where SysLogID in (select top " + (entriesToDelete + 5) + " SysLogID from aspdnsf_syslog order by createdon)");
            }
        }

        /// <summary>
        /// Commits the log entry to the database
        /// </summary>
        private void Commit()
        {
            try // Never let logging crash the site!
            {
                this.Age();

                SqlParameter[] spa = {  DB.CreateSQLParameter("@message", SqlDbType.Text, this.Message.Length, this.Message, ParameterDirection.Input),
                                    DB.CreateSQLParameter("@details", SqlDbType.Text, this.Details.Length, this.Details, ParameterDirection.Input),
                                    DB.CreateSQLParameter("@type", SqlDbType.NVarChar, 100, this.MessageType.ToString(), ParameterDirection.Input),
                                    DB.CreateSQLParameter("@severity", SqlDbType.NVarChar, 100, this.MessageSeverity.ToString(), ParameterDirection.Input)
                                 };

                int i = DB.ExecuteStoredProcInt("aspdnsf_insSysLog", spa);

                return;
            }
            catch(Exception)
            {
                return;
            }

        }

        #endregion
        
        #region static methods

        /// <summary>
        /// Loads the specified log entry from the database
        /// </summary>
        /// <param name="sysLogID">The ID of the log entry to load</param>
        public static void LoadFromDB(int sysLogID)
        {
            //TBD
        }

        /// <summary>
        /// Deletes the entire contents of the event log
        /// </summary>
        public static void Clear()
        {
            DB.ExecuteSQL("DELETE from aspdnsf_SysLog");
        }

        /// <summary>
        /// Takes an exception and, if logging is enabled, logs it to the database
        /// </summary>
        /// <param name="ex">The exception object</param>
        /// <param name="messageType">The type of message being logged</param>
        /// <param name="messageSeverity">The severity of the message being logged</param>
        public static void LogException(Exception ex, MessageTypeEnum messageType, MessageSeverityEnum messageSeverity)
        {
            try // Never let logging crash the site!
            {
                String details = FormatExceptionForLog(ex);

                LogMessage(ex.Message, details, messageType, messageSeverity);
            }
            catch
            {
                return;
            }

        }

        /// <summary>
        /// Takes a message or alert, and if logging is enabled, logs it to the database
        /// For logging exceptions, SysLog.LogException should be used to capture the details of the exception
        /// </summary>
        /// <param name="message">A general description of the message being logged</param>
        /// <param name="details">Detailed information about the message</param>
        /// <param name="messageType">The type of message being logged</param>
        /// <param name="messageSeverity">The severity of the message being logged</param>
        public static void LogMessage(string message, string details, MessageTypeEnum messageType, MessageSeverityEnum messageSeverity)
        {
            try // Never let logging crash the site!
            {
                string loggingLocation = AppLogic.AppConfig("System.LoggingLocation");
                bool loggingEnabled = AppLogic.AppConfigBool("System.LoggingEnabled");

                if (loggingEnabled && CommonLogic.StringInCommaDelimitedStringList("database", loggingLocation))
                {
                    SysLog logEntry = new SysLog();

                    logEntry.Message = Security.HtmlEncode(Security.ScrubCCNumbers(message));
                    logEntry.Details = Security.HtmlEncode(Security.ScrubCCNumbers(details));
                    logEntry.MessageType = messageType;
                    logEntry.MessageSeverity = messageSeverity;

                    logEntry.Commit();
                }
                else
                {
                    return;
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Formats and extracts details from an exception for log entry
        /// </summary>
        /// <param name="ex">The exception to format</param>
        /// <returns></returns>
        public static string FormatExceptionForLog(Exception ex)
        {
            try  // Never let logging crash the site!
            {
                StringBuilder details = new StringBuilder();

                try // In case returning the page name fails, still log the rest of the data
                {
                    details.Append(AppLogic.GetString("admin.systemlog.PageURL", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + Security.ScrubCCNumbers(CommonLogic.GetThisPageName(true)) + "\r\n");
                }
                catch { }

                if (ex.InnerException != null)
                {
                    details.Append(AppLogic.GetString("admin.systemlog.Source", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + ex.InnerException.Source + "\r\n");
                    details.Append(AppLogic.GetString("admin.systemlog.Message", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + ex.InnerException.Message + "\r\n");
                    details.Append(AppLogic.GetString("admin.systemlog.StackTrace", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + "\r\n" + ex.InnerException.StackTrace + "\r\n");
                }
                else
                {
                    details.Append(AppLogic.GetString("admin.systemlog.Source", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + ex.Source + "\r\n");
                    details.Append(AppLogic.GetString("admin.systemlog.Message", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + ex.Message + "\r\n");
                    details.Append(AppLogic.GetString("admin.systemlog.StackTrace", AppLogic.GetStoreSkinID(AppLogic.StoreID()), Localization.GetDefaultLocale()) + "\r\n" + ex.StackTrace + "\r\n");
                }
                return details.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Returns the SysLogID of this instance.  This property is read-only.
        /// </summary>
        public int SysLogID
        {
            get { return m_SysLogID; }
        }

        /// <summary>
        /// Returns the max number of log entries to store.  This property is read-only.
        /// </summary>
        public int MaxLogEntries
        {
            get { return m_maxEntries; }
        }

        /// <summary>
        /// Returns the max number of days to store log entries.  This property is read-only.
        /// </summary>
        public int MaxLogDays
        {
            get { return m_maxDays; }
        }

        /// <summary>
        /// Gets or sets the message for this log entry.
        /// </summary>
        public string Message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        /// <summary>
        /// Gets or sets the details for this log entry
        /// </summary>
        public string Details
        {
            get {return m_details;}
            set { m_details = value; }
        }

        /// <summary>
        /// Gets or sets the message type for this log entry.
        /// </summary>
        public MessageTypeEnum MessageType
        {
            get { return m_messageType; }
            set { m_messageType = value; }
        }

        /// <summary>
        /// Returns the severity of this log entry.
        /// </summary>
        public MessageSeverityEnum MessageSeverity
        {
            get { return m_messageSeverity; }
            set { m_messageSeverity = value; }
        }

        /// <summary>
        /// Returns the date/time this log entry was created.  This property is read-only.
        /// </summary>
        public DateTime CreatedOn
        {
            get { return m_createdOn; }
        }


        #endregion

    }
}
