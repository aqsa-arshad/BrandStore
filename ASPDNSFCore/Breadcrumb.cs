// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Web;

namespace AspDotNetStorefrontCore
{
    public class Breadcrumb
    {

        private static Hashtable m_ProductHT = new Hashtable();
        private static Hashtable m_EntityHT = new Hashtable();
        private static Hashtable m_EntityBC = new Hashtable();

        public Breadcrumb() { }

        public static String GetProductBreadcrumb(int ProductID, String SourceEntity, int SourceEntityID, Customer ThisCustomer)
        {
            return GetProductBreadcrumb(ProductID, AppLogic.GetProductName(ProductID,ThisCustomer.LocaleSetting), SourceEntity, SourceEntityID, ThisCustomer);
        }

        public static String GetProductBreadcrumb(int ProductID, String ProductName, String SourceEntity, int SourceEntityID, Customer ThisCustomer)
        {
            String CacheName = String.Empty;
            if (AppLogic.CachingOn)
            {
                CacheName = String.Format("p_{0}_{1}_{2}_{3}", ProductID.ToString(), SourceEntity, SourceEntityID.ToString(), ThisCustomer.LocaleSetting);
                if (m_ProductHT.ContainsKey(CacheName))
                {
                    return m_ProductHT[CacheName].ToString();
                }
            }
            String separator = AppLogic.AppConfig("BreadcrumbSeparator");
            if (separator.Length == 0)
            {
                separator = "&rarr;";
            }
            StringBuilder st = new StringBuilder(2048);
            if (SourceEntityID != 0)
            {
                EntityHelper hlp = AppLogic.LookupHelper(SourceEntity, 0);
                int ThisID = SourceEntityID;
                while (ThisID != 0)
                {
                    st.Insert(0, String.Format("<a class=\"SectionTitleText\" href=\"{0}\">{1}</a> {2} ", SE.MakeEntityLink(SourceEntity, ThisID, ""), hlp.GetEntityName(ThisID, ThisCustomer.LocaleSetting), separator));
                    ThisID = hlp.GetParentEntity(ThisID);
                }
            }
            st.Append(ProductName);
            st.Insert(0, "<span class=\"SectionTitleText\">");
            st.Append("</span>");
            String s = st.ToString();
            if (AppLogic.CachingOn)
            {
                // possible race condition... check again...
                if (m_ProductHT.ContainsKey(CacheName) == false)
                {
                    m_ProductHT.Add(CacheName, s);
                }
            }
            return s;
        }

        public static String GetEntityBreadcrumb(int EntityID, String EntityInstanceName, String SourceEntity, Customer ThisCustomer)
        {
            String CacheName = String.Empty;
            if (AppLogic.CachingOn)
            {
                CacheName = String.Format("e_{0}_{1}_{2}_{3}", EntityID.ToString(), EntityInstanceName, SourceEntity, ThisCustomer.LocaleSetting);
                if (m_EntityHT.ContainsKey(CacheName))
                {
                    return m_EntityHT[CacheName].ToString();
                }
            }

            String separator = AppLogic.AppConfig("BreadcrumbSeparator");
            if (separator.Length == 0)
            {
                separator = "&rarr;";
            }
            EntityHelper hlp = AppLogic.LookupHelper(SourceEntity, 0);
            StringBuilder st = new StringBuilder(1024);
            int ParentID = hlp.GetParentEntity(EntityID);
            while (ParentID != 0)
            {
                st.Insert(0, String.Format("<a class=\"SectionTitleText\" href=\"{0}\">{1}</a> {2} ", SE.MakeEntityLink(SourceEntity, ParentID, ""), hlp.GetEntityName(ParentID, ThisCustomer.LocaleSetting),separator));
                ParentID = hlp.GetParentEntity(ParentID);
            }
            st.Append(EntityInstanceName);
            st.Insert(0, "<span class=\"SectionTitleText\">");
            st.Append("</span>");
            String s = st.ToString();
            
            if (AppLogic.CachingOn)
            {
                m_EntityHT.Add(CacheName, s);
            }
            return s;
        }

        public static String GetEntityXPath(int EntityID, String SourceEntity, String LocaleSetting)
        {
            String CacheName = String.Empty;
            if (AppLogic.CachingOn)
            {
                CacheName = String.Format("e_{0}_{1}_{2}", EntityID.ToString(), SourceEntity, LocaleSetting);
                if (m_EntityBC.ContainsKey(CacheName))
                {
                    return m_EntityBC[CacheName].ToString();
                }
            }

            String separator = "/";

            EntityHelper hlp = AppLogic.LookupHelper(SourceEntity, 0);
            StringBuilder st = new StringBuilder(1024);
            int ParentID = hlp.GetParentEntity(EntityID);
            while (ParentID != 0)
            {
                st.Insert(0, String.Format("{0}{1}", hlp.GetEntityName(ParentID, LocaleSetting), separator));
                ParentID = hlp.GetParentEntity(ParentID);
            }
            st.Append(hlp.GetEntityName(EntityID, LocaleSetting));
            st.Insert(0, "/");
            String s = st.ToString();

            if (AppLogic.CachingOn)
            {
                m_EntityBC.Add(CacheName, s);
            }
            return s;
        }

        public static void ClearCache()
        {
            m_ProductHT.Clear();
            m_EntityHT.Clear();
            m_EntityBC.Clear();
        }

    }

}
