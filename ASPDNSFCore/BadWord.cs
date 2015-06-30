// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace AspDotNetStorefrontCore
{
    public class BadWord
    {

        #region Private Variables

        private int m_Badwordid;
        private string m_Word;
        private string m_Localesetting;
        private DateTime m_Createdon;

        #endregion

        #region contructors
        public BadWord() { }

        public BadWord(int BadWordID)
        {                       
            SqlParameter[] spa = {DB.CreateSQLParameter("@BadWordID", SqlDbType.Int, 4, BadWordID, ParameterDirection.Input)};

            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (SqlDataReader rs = DB.ExecuteStoredProcReader("aspdnsf_getBadWord", spa, conn))
                {
                    if (rs.Read())
                    {
                        m_Badwordid = DB.RSFieldInt(rs, "BadWordID");
                        m_Localesetting = DB.RSField(rs, "LocaleSetting");
                        m_Word = DB.RSField(rs, "Word");
                        m_Createdon = DB.RSFieldDateTime(rs, "CreatedOn");
                    }
                }
            }
        }

        public BadWord(int BadWordID, String LocaleSetting, String Word, DateTime Createdon)
        {
            m_Word = Word;
            m_Badwordid = BadWordID;
            m_Createdon = Createdon;
            m_Localesetting = LocaleSetting;
        }

        #endregion


        #region Static Method

        public static BadWord Create(string Word, string LocaleSetting)
        {
            int BadWordID = 0;
            string err = String.Empty;

            SqlParameter[] spa ={DB.CreateSQLParameter("@Word", SqlDbType.NVarChar, 100, Word, ParameterDirection.Input)};
            spa = DB.CreateSQLParameterArray(spa,DB.CreateSQLParameter("@LocaleSetting", SqlDbType.NVarChar,10,LocaleSetting,ParameterDirection.Input));
            spa = DB.CreateSQLParameterArray(spa,DB.CreateSQLParameter("@BadWordID", SqlDbType.Int, 4, BadWordID, ParameterDirection.Output));
                 

            try
            {
                int ret = DB.ExecuteStoredProcInt("dbo.aspdnsf_insBadWord", spa);
                BadWordID = ret;
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            if (BadWordID > 0)
            {
                BadWord a = new BadWord(BadWordID);
                return a;
            }
            return null;

        }

        public static string Update(int BadWordID, String Word, String LocaleSetting)
        {
            string err = String.Empty;
            SqlParameter[] spa = { DB.CreateSQLParameter("@BadWordID", SqlDbType.Int, 4, BadWordID, ParameterDirection.Input)};
            spa = DB.CreateSQLParameterArray(spa, DB.CreateSQLParameter("@Word", SqlDbType.NVarChar, 100, Word, ParameterDirection.Input));
            spa = DB.CreateSQLParameterArray(spa, DB.CreateSQLParameter("@LocaleSetting", SqlDbType.NVarChar, 10, LocaleSetting, ParameterDirection.Input));

            try
            {
                DB.ExecuteStoredProcInt("dbo.aspdnsf_updBadWord", spa);
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }


            return err;

        }

        public static string Update(int BadWordID, SqlParameter[] spa)
        {
            string err = String.Empty;
            try
            {
                spa = DB.CreateSQLParameterArray(spa, DB.CreateSQLParameter("@BadWordID", SqlDbType.Int, 4, BadWordID, ParameterDirection.Input));
                DB.ExecuteStoredProcInt("dbo.aspdnsf_updBadWord", spa);
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            return err;

        }
        #endregion

        #region Public Methods
        public string Update(SqlParameter[] spa)
        {
            string err = BadWord.Update(this.BadWordID, spa);
            return err;
        }

        public string Update(string Word, String LocaleSetting)
        {
            string err = String.Empty;
            try
            {
                err = Update(this.m_Badwordid, Word, LocaleSetting);
                if (err == "")
                {
                    m_Word = CommonLogic.IIF(Word != null, Word, m_Word);
                    m_Localesetting = CommonLogic.IIF(LocaleSetting != null, LocaleSetting, m_Localesetting);
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            return err;

        }

        #endregion

        #region public properties

        public DateTime CreatedOn
        {
            get { return m_Createdon; }

        }

        public int BadWordID
        {
            get { return m_Badwordid; }
        }

        public String LocaleSetting
        {
            get { return m_Localesetting; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@LocaleSetting", SqlDbType.NVarChar, 10);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_Localesetting = value;
                }
            }
        }

        public string Word
        {
            get
            { return m_Word; }
            set
            {
                SqlParameter sp1 = new SqlParameter("@Word", SqlDbType.NVarChar, 100);
                sp1.Value = value;
                SqlParameter[] spa = { sp1 };
                string retval = this.Update(spa);
                if (retval == string.Empty)
                {
                    m_Word = value;
                }
            }

        }


        #endregion
    }

    public class BadWords : IEnumerable
    {
        public SortedList m_Badwords;

        public BadWords()
        {
            m_Badwords = new SortedList();

            SqlParameter[] spa = { DB.CreateSQLParameter("@BadWordID", SqlDbType.Int, 4, null, ParameterDirection.Input) };
            using(SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (SqlDataReader rs = DB.ExecuteStoredProcReader("aspdnsf_getBadWord", spa, conn))
                {
                    while (rs.Read())
                    {
                        try
                        {
                            m_Badwords.Add(DB.RSField(rs, "Word"), new BadWord(DB.RSFieldInt(rs, "BadWordID"), DB.RSField(rs, "LocaleSetting"), DB.RSField(rs, "Word"), DB.RSFieldDateTime(rs, "CreatedOn")));
                        }
                        catch (Exception ex)
                        {
                            SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
                        }
                    }
                }
            }
        }
        
        public BadWord this[string word]
        {  get
            {
                return (BadWord)m_Badwords[word.ToLowerInvariant()];
            }
        }

        public BadWord this[int Badwordid]
        {
            get
            {
                SortedList syncdSL = SortedList.Synchronized(m_Badwords);

                for (int i = 0; i < syncdSL.Count; i++)
                {
                    if (((BadWord)syncdSL.GetByIndex(i)).BadWordID == Badwordid)
                    {
                        return (BadWord)syncdSL.GetByIndex(i);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Adds an existing Word object to teh collection
        /// </summary>
        public void Add(BadWord BadWord)
        {
            m_Badwords.Add(BadWord.Word, BadWord);
        }

        /// <summary>
        /// Creates a new Word record and adds it to the collection
        /// </summary>
        public void Add(string Word, String LocaleSetting)
        {
            this.Add(BadWord.Create(Word, LocaleSetting));

        }

        /// <summary>
        /// Deletes the Word record and removes the item from the collection
        /// </summary>
        public void Remove(int BadWordID)
        {
            try
            {
                DB.ExecuteSQL("delete dbo.BadWord where Badwordid = " + BadWordID);
                m_Badwords.Remove(BadWordID);
            }
            catch { }
        }

        public int Count
        {
            get { return m_Badwords.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return new BadWordsEnumerator(this);
        }

    }

    public class BadWordsEnumerator : IEnumerator
    {
        private int position = -1;
        private BadWords m_Badwords;

        public BadWordsEnumerator(BadWords BadWordscol)
        {
            this.m_Badwords = BadWordscol;
        }

        public bool MoveNext()
        {
            if (position < m_Badwords.m_Badwords.Count - 1)
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
                return m_Badwords.m_Badwords[position];
            }
        }
    }
}
