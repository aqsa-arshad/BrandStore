// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for HierarchicalTableMgr.
	/// Remember, manages tables with parent-child relationships as nested nodes, and each sibling level is ordered by DisplayOrder asc. DisplayOrder MAY NOT be unique!
	/// Requires table to meet certain requirements:
	/// Must have "ID" integer column, e.g. "CategoryID" (can be named differently)
	/// Must have "Name" field column, e.g. "Name" (can be named differently)
	/// Must have a ParentID integer column, e.g. "ParentCategoryID" (can be named differently)
	/// Must have a Published tinyint column, with value of 0 meaning non-published (and not included in the hierarchy)
	/// Must have a Deleted tinyint column. Values of deleted=0 mean they are not included in the hierarchy
	/// Parent Child relationships should be in the form Parent:Child of 1:N (e.g. this is not a N:N mgr!)
	/// Root level records should have ParentID = 0 (NOT NULL!!) This could be extended to work with NULL ID as the root level indicator, but it's not done currently
	/// The table can have as many other fields and types as you want.
	/// It is anticipated that the Name field is <ml>...</ml> encoded for Locales, but that is not required.
	/// </summary>
	public class HierarchicalTableMgr
	{
		private String m_DataSetXml = String.Empty;
		private String m_FinalXml = String.Empty;
		private XmlDocument m_XmlDoc;
		private bool m_FromCache = false;
		
		private String m_TableName = String.Empty;
		private String m_NodeName = String.Empty;
        private String m_IDColumnName = String.Empty;
        private String m_GUIDColumnName = String.Empty;
        private String m_NameColumnName = "Name";
		private String m_CacheName = String.Empty;
		private String m_XmlPackageName = "HierarchicalTableMgr";
		private bool m_OnlyPublishedEntitiesAndObjects;
		private int m_NumRootLevelNodes = 0;

        public HierarchicalTableMgr(String TableName, bool OnlyPublishedEntitiesAndObjects, int StoreID)
            : this(TableName, TableName, TableName + "ID", TableName + "GUID", "Name", TableName, AppLogic.CacheDurationMinutes(), 0, OnlyPublishedEntitiesAndObjects, StoreID) { }

        public HierarchicalTableMgr(int CacheMinutes, String TableName, bool OnlyPublishedEntitiesAndObjects, int StoreID)
            : this(TableName, TableName, TableName + "ID", TableName + "GUID", "Name", TableName, CacheMinutes, 0, OnlyPublishedEntitiesAndObjects, StoreID) { }

        public HierarchicalTableMgr(String TableName, int SetInitialContextToNodeID, bool OnlyPublishedEntitiesAndObjects, int StoreID)
            : this(TableName, TableName, TableName + "ID", TableName + "GUID", "Name", TableName, AppLogic.CacheDurationMinutes(), SetInitialContextToNodeID, OnlyPublishedEntitiesAndObjects, StoreID) { }

        public HierarchicalTableMgr(String TableName, int SetInitialContextToNodeID, int CacheMinutes, bool OnlyPublishedEntitiesAndObjects, int StoreID)
            : this(TableName, TableName, TableName + "ID", TableName + "GUID", "Name", TableName, CacheMinutes, SetInitialContextToNodeID, OnlyPublishedEntitiesAndObjects, StoreID) { }

		public HierarchicalTableMgr(String TableName, String NodeName, String IDColumnName, String GUIDColumnName, String NameColumnName, String XmlPackageName, int CacheMinutes, int SetInitialContextToNodeID, bool OnlyPublishedEntitiesAndObjects, int StoreID)
		{
			m_TableName = TableName;
			m_NodeName = NodeName;
            m_IDColumnName = IDColumnName;
            m_GUIDColumnName = GUIDColumnName;
            m_NameColumnName = NameColumnName;
			m_CacheName = String.Format("HTM_{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}",TableName,NodeName,IDColumnName,GUIDColumnName,NameColumnName,XmlPackageName,OnlyPublishedEntitiesAndObjects.ToString(),AppLogic.IsAdminSite.ToString(), StoreID.ToString());
			m_XmlPackageName = XmlPackageName;
			m_OnlyPublishedEntitiesAndObjects = OnlyPublishedEntitiesAndObjects;

			if(m_XmlDoc == null)
			{
                String RTParams = "EntityName=" + TableName + "&PublishedOnly=" + CommonLogic.IIF(m_OnlyPublishedEntitiesAndObjects, "1", "0");
                if (StoreID > 0)
                {
                    RTParams += "&FilterByStore=true&CurrentStoreID=" + StoreID.ToString();
                }
                using (XmlPackage2 p = new XmlPackage2(m_XmlPackageName, null, 1, String.Empty, RTParams, String.Empty, false))
                {
                    m_FinalXml = p.TransformString();
                    m_DataSetXml = XmlCommon.XmlDecode(p.XmlSystemData);
                    m_XmlDoc = new XmlDocument();
                    if (m_FinalXml.Length != 0)
                    {
                        using (StringReader sr = new StringReader(m_FinalXml))
                        {
                            using (XmlReader xr = XmlReader.Create(sr))
                            {
                                m_XmlDoc.Load(xr);
                            }
                        }
                    }
                }
            }
			m_NumRootLevelNodes = m_XmlDoc.SelectSingleNode("/root").ChildNodes.Count;
		}

		public int NumRootLevelNodes
		{
			get 
			{
				return m_NumRootLevelNodes;
			}
		}

		public String DataSetXml
		{
			get 
			{
				return m_DataSetXml;
			}
		}

		public String FinalXml
		{
			get 
			{
				return m_FinalXml;
			}
		}

		public bool FromCache
		{
			get 
			{
				return m_FromCache;
			}
		}

		public XmlDocument XmlDoc
		{
			get 
			{
				return m_XmlDoc;
			}
		}

		public String IDColumnName
		{
			get 
			{
				return m_IDColumnName;
			}
		}

        public String GUIDColumnName
        {
            get
            {
                return m_GUIDColumnName;
            }
        }

		public XmlNode ResetToRootNode()
		{
			XmlNode n = m_XmlDoc.SelectSingleNode("/root");
			return n;
		}

		// if empty, null is returned
		public XmlNode SetContextToFirstRootLevelNode()
		{
			XmlNode n = m_XmlDoc.SelectSingleNode("/root/" + m_NodeName);
			return n;
		}

		// if ToNodeID = 0 or ToNodeID doesn't exist, the current context will NOT be changed and null is returned
		public XmlNode SetContext(int ToNodeID)
		{
			XmlNode n;
			if(ToNodeID == 0)
			{
				return null;
			}
			else
			{
				// TBD need to handle <ml>...</ml> markups here too!
				String NodeSpec = String.Format(@"//{0}[./{1}={2}]",m_NodeName,m_IDColumnName,ToNodeID.ToString());
				n = m_XmlDoc.SelectSingleNode(NodeSpec);
			}
			return n;
		}

		// context unchanged if node not found
		public XmlNode SetContext(String NodeName)
		{
			XmlNode n;
			if(NodeName.Length == 0)
			{
				return null;
			}
			else
			{
				String NodeSpec = String.Format(@"//{0}[{1}/ml/locale[@name=/root/System/LocaleSetting]={2}]",m_NodeName,m_NameColumnName,DB.SQuote(XmlCommon.XmlEncode(NodeName)));
				n = m_XmlDoc.SelectSingleNode(NodeSpec);
				if(n == null)
				{
					// may not have <ml> markup on Name
					NodeSpec = String.Format(@"//{0}[Name={1}]",m_NodeName,DB.SQuote(XmlCommon.XmlEncode(NodeName)));
					n = m_XmlDoc.SelectSingleNode(NodeSpec);
				}
			}
			return n;
		}

        // returns the id of the currently active node
        public int CurrentID(XmlNode CurrentContext)
        {
            if (CurrentContext == null)
            {
                return 0;
            }
            return XmlCommon.XmlFieldUSInt(CurrentContext, m_IDColumnName);
        }

        // returns the GUID of the currently active node
        public String CurrentGUID(XmlNode CurrentContext)
        {
            if (CurrentContext == null)
            {
                return String.Empty;
            }
            return XmlCommon.XmlField(CurrentContext, m_GUIDColumnName);
        }

        // returns the name (locale specific) of the currently active node
		public String CurrentName(XmlNode CurrentContext, String LocaleSetting)
		{
            if (CurrentContext == null)
            {
                return String.Empty;
            } 
            return XmlCommon.XmlFieldByLocale(CurrentContext, m_NameColumnName, LocaleSetting);
		}

		// returns the <FieldName> element value of the currently active node
		public String CurrentField(XmlNode CurrentContext, String FieldName)
		{
            if (CurrentContext == null)
            {
                return String.Empty;
            } 
            return XmlCommon.XmlField(CurrentContext, FieldName);
		}

		// returns the <FieldName> element value of the currently active node as an integer
		public int CurrentFieldInt(XmlNode CurrentContext, String FieldName)
		{
            if (CurrentContext == null)
            {
                return 0;
            } 
            return Localization.ParseUSInt(XmlCommon.XmlField(CurrentContext, FieldName));
		}

		// returns the <FieldName> element value of the currently active node as an bool
		public bool CurrentFieldBool(XmlNode CurrentContext, String FieldName)
		{
            if (CurrentContext == null)
            {
                return false;
            } 
            String tmpS = CurrentField(CurrentContext, FieldName);
			if(tmpS == "true" || tmpS == "yes" || tmpS == "1")
			{
				return true;
			}
			return false;
		}

		// returns the <FieldName> element value of the currently active node as a long
		public long CurrentFieldLong(XmlNode CurrentContext, String FieldName)
		{
            if (CurrentContext == null)
            {
                return 0;
            } 
            return Localization.ParseUSLong(XmlCommon.XmlField(CurrentContext, FieldName));
		}

		// returns the <FieldName> element value of the currently active node as a Single
		public Single CurrentFieldSingle(XmlNode CurrentContext, String FieldName)
		{
            if (CurrentContext == null)
            {
                return 0.0F;
            } 
            return Localization.ParseNativeSingle(XmlCommon.XmlField(CurrentContext, FieldName));
		}

		// returns the <FieldName> element value of the currently active node as a Decimal
		public Decimal CurrentFieldDecimal(XmlNode CurrentContext, String FieldName)
		{
            if (CurrentContext == null)
            {
                return System.Decimal.Zero;
            } 
            return Localization.ParseNativeDecimal(XmlCommon.XmlField(CurrentContext, FieldName));
		}

		// returns the <FieldName> element value of the currently active node as a DateTime
		public DateTime CurrentFieldDateTime(XmlNode CurrentContext, String FieldName)
		{
            if (CurrentContext == null)
            {
                return System.DateTime.MinValue;
            } 
            return Localization.ParseNativeDateTime(XmlCommon.XmlField(CurrentContext, FieldName));
		}

		// returns the <FieldName> element value of the currently active node
		public String CurrentFieldByLocale(XmlNode CurrentContext, String FieldName, String LocaleSetting)
		{
            if (CurrentContext == null)
            {
                return String.Empty;
            } 
            return XmlCommon.XmlFieldByLocale(CurrentContext, FieldName, LocaleSetting);
		}

		// returns true if the currently active node is at the root level
		public bool IsRootLevel(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return true;
            }
            return (CurrentContext.ParentNode.Name == "root");
		}

		// returns true if the currently active node has any children nodes
		public bool HasChildren(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return false;
            } 
            XmlNode n = CurrentContext.SelectSingleNode(m_NodeName);
			return (n != null);
		}

		// returns true if the currently active node a child node (at any level down from the current node) with the ID specified
		// also returns true if NodeID == CurrentContext NodeID (i.e. the node is considered a child of itself).
		public bool ContainsChild(XmlNode CurrentContext, int NodeID)
		{
            if (CurrentContext == null)
            {
                return false;
            } 
            int ThisID = this.CurrentID(CurrentContext);
			if(NodeID == ThisID)
			{
				return true;
			}
			XmlNode n = SetContext(NodeID);
			if(n == null)
			{
				return false; // can't have a non-existant child
			}
			n = MoveParent(n);
			while(n != null)
			{
				if(ThisID == CurrentID(n))
				{
					return true;
				}
				n = this.MoveParent(n);
			}
			return false;
		}

		// returns number of direct children nodes for the currently active node
		public int NumChildren(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return 0;
            } 
            return GetChildrenList(CurrentContext).Count;
		}

		// returns true if this node has siblings on same level
		public bool HasSiblings(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return false;
            } 
            return SiblingList(CurrentContext).Count > 1;
		}

		public int NumSiblings(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return 0;
            } 
            return SiblingList(CurrentContext).Count;
		}

		// returns nesting level of the currently active node, root level = 1
		public int Level(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return 0;
            } 
            int nestingLevel = 0;
			XmlNode lNode = CurrentContext;
			while (lNode.ParentNode != null)
			{
                nestingLevel++;
				lNode = lNode.ParentNode;
			}
            return nestingLevel;
		}

		// returns xml node list of all categories at this same level
		public XmlNodeList SiblingList(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            return CurrentContext.ParentNode.SelectNodes("./" + m_NodeName);
		}

		// returns xml node list of all child nodes of this node (just next level down)
		public XmlNodeList GetChildrenList(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            return CurrentContext.SelectNodes("./" + m_NodeName); // not sure how to do this, because we only want child nodes of m_NodeName
		}

		// changes current context to the parent node of the currently active node
		public XmlNode MoveParent(XmlNode CurrentContext)
		{
			if(IsRootLevel(CurrentContext))
			{
				return null;
			}
			return CurrentContext.ParentNode;
		}

		// changes current context to the first child node of the currently active node
		public XmlNode MoveFirstChild(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            XmlNodeList children = CurrentContext.SelectNodes("./" + m_NodeName);
			if (children.Count == 0)
			{
				return null;
			}
			XmlNode n = children[0];
			return n;
		}

		// changes current context to the first sibling node on the same level as the currently active node
		public XmlNode MoveFirstSibling(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            XmlNode n = SiblingList(CurrentContext)[0];
			return n;
		}

		public bool IsFirstSibling(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return false;
            } 
            XmlNode prev = CurrentContext.PreviousSibling;
			while (prev != null && prev.LocalName != m_NodeName)
			{
				prev = prev.PreviousSibling;
			}
			return (prev == null);
		}

		public bool IsLastSibling(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return false;
            } 
            XmlNode next = CurrentContext.NextSibling;
			while (next != null && next.LocalName != m_NodeName)
			{
				next = next.NextSibling;
			}
			return (next == null);
		}

		// changes current context to the last sibling node on the same level as the currently active node
		public XmlNode MoveLastSibling(XmlNode CurrentContext)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            XmlNodeList l = SiblingList(CurrentContext);
			if(l.Count == 0)
			{
				return null;
			}
			XmlNode n = l[l.Count-1];
			return n;
		}

		// changes current context to the next sibling node on the same level as the currently active node
		public XmlNode MoveNextSibling(XmlNode CurrentContext, bool Circular)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            XmlNode next = CurrentContext.NextSibling;
			if(Circular && next == null)
			{
				next = MoveFirstSibling(CurrentContext);
			}
			return next;
		}

		// changes current context to the previous sibling node on the same level as the currently active node
		public XmlNode MovePreviousSibling(XmlNode CurrentContext, bool Circular)
		{
            if (CurrentContext == null)
            {
                return null;
            } 
            XmlNode prev = CurrentContext.PreviousSibling;
			if(Circular && prev == null)
			{
				prev = MoveLastSibling(CurrentContext);
			}
			return prev;
		}


	}
}
