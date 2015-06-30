// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Provides a temporary data store object for storing Async processing results
    /// </summary>
    public class AsyncDataStore
    {
        private static Hashtable m_ht = new Hashtable();

        /// <summary>
        /// Instantiates a new instance of the AsyncDataStore class
        /// </summary>
        static AsyncDataStore()
        {
        }

        /// <summary>
        /// Retrieves an object from the data store using the provided key
        /// </summary>
        /// <param name="key">Data key (generally the Session ID)</param>
        /// <returns>Object corresponding to the provided data key</returns>
        public static object RetrieveRecord(string key)
        {
            lock (m_ht.SyncRoot)
            {
                return m_ht[key];
            }
        }

        /// <summary>
        /// Stores an object in the data store using the provided key
        /// </summary>
        /// <param name="key">Data key (generally the Session ID)</param>
        /// <param name="value">Object to store</param>
        public static void AddRecord(string key, object value)
        {
            lock (m_ht.SyncRoot)
            {
                m_ht.Add(key, value);
            }
        }

        /// <summary>
        /// Updates the object corresponding to the provided key
        /// </summary>
        /// <param name="key">Data key (generally the Session ID)</param>
        /// <param name="value">New object to store</param>
        public static void UpdateRecord(string key, object value)
        {
            lock (m_ht.SyncRoot)
            {
                if (m_ht[key] != null)
                {
                    m_ht[key] = value;
                }
            }
        }

        /// <summary>
        /// Removes an object from the data store using the corresponding data key
        /// </summary>
        /// <param name="key">Data key (generally the Session ID)</param>
        public static void RemoveRecord(string key)
        {
            lock (m_ht.SyncRoot)
            {
                m_ht.Remove(key);
            }
        }

        /// <summary>
        /// Clears all records from the data store
        /// </summary>
        public static void ClearAll()
        {
            lock (m_ht.SyncRoot)
            {
                m_ht.Clear();
            }
        }
    }
}
