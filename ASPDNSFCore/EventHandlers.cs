// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Web;
using System.Net;
using System.Threading;


namespace AspDotNetStorefrontCore
{
    public class AspdnsfEventHandler
    {
        #region private variables
        private int m_EventID;
        private string m_EventName;
        private string m_CalloutURL;
        private string m_XmlPackage;
        private bool m_Active;
        private bool m_Debug;
        #endregion

        #region contructors

        public AspdnsfEventHandler() { }

        public AspdnsfEventHandler(int EventID)
        {

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getEventHandler " + EventID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        m_EventID = DB.RSFieldInt(rs, "EventID");
                        m_EventName = DB.RSField(rs, "EventName").Trim();
                        m_CalloutURL = DB.RSField(rs, "CalloutURL").Trim();
                        m_XmlPackage = DB.RSField(rs, "XmlPackage").Trim();
                        m_Active = DB.RSFieldBool(rs, "Active");
                        m_Debug = DB.RSFieldBool(rs, "Debug");
                    }
                }
            }
        }

        public AspdnsfEventHandler(int EventID, string EventName, string CalloutURL, string XmlPackage, bool Active, bool Debug)
        {
            m_EventID = EventID;
            m_EventName = EventName;
            m_CalloutURL = CalloutURL;
            m_XmlPackage = XmlPackage;
            m_Active = Active;
            m_Debug = Debug;
        }

        #endregion

        #region static methods

        public static AspdnsfEventHandler Create(string EventName, string CalloutURL, string XmlPackage, bool Active, bool Debug)
        {
            int EventID = 0;

            if (EventName.Trim().Length == 0)
            {
                return null;
            }

            if (CalloutURL.Trim().Length == 0)
            {
                return null;
            }

            if (XmlPackage.Trim().Length == 0)
            {
                return null;
            }

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_insEventHandler";

            cmd.Parameters.Add(new SqlParameter("@EventName", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@CalloutURL", SqlDbType.VarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@XmlPackage", SqlDbType.VarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@Active", SqlDbType.Bit));
            cmd.Parameters.Add(new SqlParameter("@Debug", SqlDbType.Bit));
            cmd.Parameters.Add(new SqlParameter("@EventID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

            cmd.Parameters["@EventName"].Value = EventName;
            cmd.Parameters["@CalloutURL"].Value = CalloutURL;
            cmd.Parameters["@XmlPackage"].Value = XmlPackage;
            cmd.Parameters["@Active"].Value = Active;
            cmd.Parameters["@Debug"].Value = Debug;

            try
            {
                cmd.ExecuteNonQuery();
                EventID = Int32.Parse(cmd.Parameters["@EventID"].Value.ToString());
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            cn.Close();
            cmd.Dispose();
            cn.Dispose();

            if (EventID > 0)
            {
                AspdnsfEventHandler a = new AspdnsfEventHandler(EventID);
                return a;
            }
            return null;

        }

        public static string Update(int EventID, SqlParameter[] spa)
        {
            string err = String.Empty;

            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updEventHandler";

            SqlParameter sqlparam = new SqlParameter("@EventID", SqlDbType.Int, 4);
            sqlparam.Value = EventID;
            cmd.Parameters.Add(sqlparam);
            foreach (SqlParameter sp in spa)
            {
                cmd.Parameters.Add(sp);
            }
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

        public static string Update(int EventID, string EventName, string CalloutURL, string XMLPackage, object Active, object Debug)
        {

            string err = String.Empty;
            SqlConnection cn = new SqlConnection(DB.GetDBConn());
            cn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.aspdnsf_updEventHandler";

            cmd.Parameters.Add(new SqlParameter("@EventID", SqlDbType.Int, 4));
            cmd.Parameters.Add(new SqlParameter("@EventName", SqlDbType.NVarChar, 20));
            cmd.Parameters.Add(new SqlParameter("@CalloutURL", SqlDbType.VarChar, 200));
            cmd.Parameters.Add(new SqlParameter("@XMLPackage", SqlDbType.VarChar, 100));
            cmd.Parameters.Add(new SqlParameter("@Active", SqlDbType.Bit));
            cmd.Parameters.Add(new SqlParameter("@Debug", SqlDbType.Bit));
            cmd.Parameters["@EventID"].Value = EventID;

            if (EventName.Trim() == null)
                cmd.Parameters["@EventName"].Value = DBNull.Value;
            else
                cmd.Parameters["@EventName"].Value = EventName;

            if (CalloutURL == null)
                cmd.Parameters["@CalloutURL"].Value = DBNull.Value;
            else
                cmd.Parameters["@CalloutURL"].Value = CalloutURL;

            if (XMLPackage == null)
                cmd.Parameters["@XmlPackage"].Value = DBNull.Value;
            else
                cmd.Parameters["@XmlPackage"].Value = XMLPackage;

            if (Active == null)
                cmd.Parameters["@Active"].Value = DBNull.Value;
            else
                cmd.Parameters["@Active"].Value = Active;

            if (Debug == null)
                cmd.Parameters["@Debug"].Value = DBNull.Value;
            else
                cmd.Parameters["@Debug"].Value = Debug;

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

        #endregion


        #region AsycCall functions and Variables

        // For Asynchronous Call
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        const int BUFFER_SIZE = 1024;
        // End For Asynchronous Call
        private static void ScanTimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                RequestState reqState = (RequestState)state;
                if (reqState != null)
                    reqState.Request.Abort();
            }
        }

        private static void RespCallback(IAsyncResult ar)
        {
            RequestState rs = (RequestState)ar.AsyncState;
            try
            {
                // Get the RequestState object from the async result.

                // Get the WebRequest from RequestState.
                WebRequest req = rs.Request;

                // Call EndGetResponse, which produces the WebResponse object
                //  that came from the request issued above.
                WebResponse resp = req.EndGetResponse(ar);

                //  Start reading data from the response stream.
                Stream ResponseStream = resp.GetResponseStream();

                // Store the response stream in RequestState to read 
                // the stream asynchronously.
                rs.ResponseStream = ResponseStream;
                rs.Waitone.Set();

                if (rs.Debug)
                {
                    //  Pass rs.BufferRead to BeginRead. Read data into rs.BufferRead
                    IAsyncResult iarRead = ResponseStream.BeginRead(rs.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(AspdnsfEventHandler.ReadCallBack), rs);
                }
            }
            catch (Exception ex) 
            { 
                string err = ex.Message;
                ASCIIEncoding encoding = new ASCIIEncoding();
                string data = encoding.GetString(rs.DataToSend);
                DB.LogSql("Error Executing Event Handler call:" + ex.Message + "\nEvent Data:" + data);
                rs.Waitone.Set();
            }
        }
        private static void ReadCallbackRequest(IAsyncResult asynchronousResult)
        {
            RequestState rs = (RequestState)asynchronousResult.AsyncState;
            try
            {
                HttpWebRequest request = rs.Request;
                // End the operation.
                Stream postStream = request.EndGetRequestStream(asynchronousResult);

                // Write to the request stream.
                postStream.Write(rs.DataToSend, 0, rs.DataToSend.Length);
                postStream.Close();
            }
            catch (Exception ex)
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                string data = encoding.GetString(rs.DataToSend);
                DB.LogSql("Error Executing Event Handler call:" + ex.Message + "\nEvent Data:" + data);
            }
            allDone.Set();

        }

        private static void ReadCallBack(IAsyncResult asyncResult)
        {
            // Get the RequestState object from AsyncResult.
            RequestState rs = (RequestState)asyncResult.AsyncState;

            // Retrieve the ResponseStream that was set in RespCallback. 
            Stream responseStream = rs.ResponseStream;

            // Read rs.BufferRead to verify that it contains data. 
            int read = responseStream.EndRead(asyncResult);
            if (read > 0)
            {
                // Prepare a Char array buffer for converting to Unicode.
                Char[] charBuffer = new Char[BUFFER_SIZE];

                // Convert byte stream to Char array and then to String.
                // len contains the number of characters converted to Unicode.
                int len =
                   rs.StreamDecode.GetChars(rs.BufferRead, 0, read, charBuffer, 0);

                String str = new String(charBuffer, 0, len);

                // Append the recently read data to the RequestData stringbuilder
                // object contained in RequestState.
                rs.RequestData.Append(Encoding.ASCII.GetString(rs.BufferRead, 0, read));

                // Continue reading data until 
                // responseStream.EndRead returns ?.
                IAsyncResult ar = responseStream.BeginRead(rs.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(AspdnsfEventHandler.ReadCallBack), rs);
            }
            else
            {
                if (rs.RequestData.Length > 0)
                {
                    //  Display data to the console.
                    string strContent;
                    strContent = rs.RequestData.ToString();
                    DB.LogSql(strContent); //Log the data to the SQL Table, only if Debug is true
                }
                // Close down the response stream.
                responseStream.Close();
                // Set the ManualResetEvent so the main thread can exit.
                allDone.Set();
            }
            return;
        }
       

        #endregion


        #region Public Methods
        public string Update(SqlParameter[] spa)
        {
            return AspdnsfEventHandler.Update(this.EventID, spa);
        }

        public string Update(string EventName, string CalloutURL, string XmlPackage, object Active, object Debug)
        {
            string err = String.Empty;
            try
            {
                err = Update(this.m_EventID, EventName, CalloutURL, XmlPackage, Active,Debug);
                if (err == "")
                {
                    m_EventName = CommonLogic.IIF(EventName != null, EventName, m_EventName);
                    m_CalloutURL = CommonLogic.IIF(CalloutURL != null, CalloutURL, m_CalloutURL);
                    m_XmlPackage = CommonLogic.IIF(XmlPackage != null, XmlPackage, m_XmlPackage);
                    if (Active != null)
                    {
                        m_Active = (bool)Active;
                    }
                    if (Debug != null)
                    {
                        m_Debug = (bool)Debug;
                    }
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            return err;

        }

        public void CallEvent(string runtimeParams)
        {
            try
            {
                Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;

                if (this.CalloutURL.Trim() != "" && m_Active)
                {
                    String str = AppLogic.RunXmlPackage(this.m_XmlPackage, null, ThisCustomer, 1, "", runtimeParams, false, false);
                    byte[] data = Encoding.ASCII.GetBytes(str);

                    HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(this.m_CalloutURL); //Webrequest must be fully qualified to avoid compile warnings in VB
                    webRequest.Method = "POST";
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.ContentLength = data.Length;

                    // For Async Call
                    // Create the state object.
                    ManualResetEvent WaitHndle = new ManualResetEvent(false);
                    RequestState rs = new RequestState(WaitHndle,this.Debug);

                    // Put the request into the state object so it can be passed around.
                    rs.Request = webRequest;
                    rs.DataToSend = data;

                    webRequest.BeginGetRequestStream(new AsyncCallback(ReadCallbackRequest), rs);
                  
                    // Issue the async request.

                    IAsyncResult r = (IAsyncResult)webRequest.BeginGetResponse(new AsyncCallback(AspdnsfEventHandler.RespCallback), rs);

                    ThreadPool.RegisterWaitForSingleObject(r.AsyncWaitHandle, new WaitOrTimerCallback(AspdnsfEventHandler.ScanTimeoutCallback), rs, (30 * 1000), true); //30 sec timeout

                    allDone.WaitOne();
                    rs.ResponseStream.Close();
                    
                }
            }
            catch (WebException ex) { string str = ex.ToString(); }
            catch (Exception ex) { string sr = ex.ToString(); }
        }

        #endregion

        #region public properties

        public int EventID
        {
            get { return m_EventID; }
        }

        public string XmlPackage
        {
            get { return m_XmlPackage; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@XmlPackage", SqlDbType.VarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_XmlPackage = value;
                }
            }
        }

        public string CalloutURL
        {
            get { return m_CalloutURL; }
            set
            {

                SqlParameter sp1 = new SqlParameter("@CalloutURL", SqlDbType.VarChar, 200);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_CalloutURL = value;
                }
            }
        }

        public string EventName
        {
            get { return m_EventName; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@EventName", SqlDbType.NVarChar, 20);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_EventName = value;
                }
            }
        }

        public bool Active
        {
            get { return m_Active; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@Active", SqlDbType.Bit);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_Active = value;
                }
            }
        }

        public bool Debug
        {
            get { return m_Debug; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@Debug", SqlDbType.Bit);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_Debug = value;
                }
            }
        }

        #endregion

    }


    public class AspdnsfEventHandlers : IEnumerable
    {
        public SortedList m_EventHandlers;

        public AspdnsfEventHandlers()
        {
            m_EventHandlers = new SortedList();

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("aspdnsf_getEventHandler", dbconn))
                {
                    while (rs.Read())
                    {
                        m_EventHandlers.Add(DB.RSField(rs, "EventName").ToLowerInvariant(), new AspdnsfEventHandler(DB.RSFieldInt(rs, "EventID"), DB.RSField(rs, "EventName"), DB.RSField(rs, "CalloutURL"), DB.RSField(rs, "XmlPackage"), DB.RSFieldBool(rs, "Active"), DB.RSFieldBool(rs, "Debug")));
                    }
                }
            }
        }

        public AspdnsfEventHandler this[string eventname]
        {
            get
            {
                return (AspdnsfEventHandler)m_EventHandlers[eventname.ToLowerInvariant()];
            }
        }

        public AspdnsfEventHandler this[int eventid]
        {
            get
            {
                SortedList syncdSL = SortedList.Synchronized(m_EventHandlers);

                for (int i = 0; i < syncdSL.Count; i++)
                {
                    if (((AspdnsfEventHandler)syncdSL.GetByIndex(i)).EventID == eventid)
                    {
                        return (AspdnsfEventHandler)syncdSL.GetByIndex(i);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Adds an existing EventHandlerClass object to the collection
        /// </summary>
        public void Add(AspdnsfEventHandler eventhandler)
        {
            m_EventHandlers.Add(eventhandler.EventName.ToLowerInvariant(), eventhandler);
        }

        /// <summary>
        /// Creates a new EventHandler record and adds it to the collection
        /// </summary>
        public void Add(string EventName, string CalloutURL, string XmlPackage, bool Active,bool Debug)
        {
            this.Add(AspdnsfEventHandler.Create(EventName, CalloutURL.Trim(), XmlPackage.Trim(), Active,Debug));
        }

        /// <summary>
        /// Deletes the EventHandler record and removes the item from the collection
        /// </summary>
        public void Remove(string eventname)
        {
            try
            {
                DB.ExecuteSQL("delete dbo.eventhandler where eventid = " + this[eventname].EventID.ToString());
                m_EventHandlers.Remove(eventname);
            }
            catch { }
        }

        public int Count
        {
            get { return m_EventHandlers.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return new EventHandlersEnumerator(this);
        }

    }

    public class EventHandlersEnumerator : IEnumerator
    {
        private int position = -1;
        private AspdnsfEventHandlers m_eventhandlers;

        public EventHandlersEnumerator(AspdnsfEventHandlers eventhandlerscol)
        {
            this.m_eventhandlers = eventhandlerscol;
        }

        public bool MoveNext()
        {
            if (position < m_eventhandlers.m_EventHandlers.Count - 1)
            {
                position++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            position = -1;
        }

        public object Current
        {
            get
            {
                return m_eventhandlers.m_EventHandlers[position];
            }
        }
    }


    // The RequestState class passes data across async calls.
    public class RequestState
    {
        const int BufferSize = 1024;
        public byte[] DataToSend;
        public StringBuilder RequestData;
        public byte[] BufferRead;
        public HttpWebRequest Request;
        public Stream ResponseStream;
        public ManualResetEvent Waitone;
        public bool Debug;
        // Create Decoder for appropriate enconding type.
        public Decoder StreamDecode = Encoding.UTF8.GetDecoder();

        public RequestState(ManualResetEvent waitHandle,bool debug)
        {
            BufferRead = new byte[BufferSize];
            RequestData = new StringBuilder(String.Empty);
            Request = null;
            ResponseStream = null;
            Waitone = waitHandle;
            Debug = debug;
        }
     }


}

