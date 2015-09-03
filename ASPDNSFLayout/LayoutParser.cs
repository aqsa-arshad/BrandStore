// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using System.Data;


namespace AspDotNetStorefrontLayout
{

    public class ImagePanelNode
    {
        #region Private Variables

        private XmlNode m_transnode;
        private XmlNode m_inputnode;

        #endregion

        #region Public Properties

        public XmlNode TransNode
        {
            get { return m_transnode; }
            set { m_transnode = value; }
        }

        public XmlNode InputNode
        {
            get { return m_inputnode; }
            set { m_inputnode = value; }
        }

        #endregion

        #region Node Properties

        private string baseXPath
        {
            get
            {
                return "."; 
            }
        }

        private XmlNode heightNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@height | {0}/@Height| {0}/@HEIGHT", baseXPath));
                }
            }
        }

        private XmlNode widthNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@width | {0}/@Width| {0}/@WIDTH", baseXPath));
                }
            }
        }

        private XmlNode styleNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@style | {0}/@Style| {0}/@STYLE", baseXPath));
                }
            }
        }

        private XmlNode classNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@class | {0}/@Class| {0}/@CLASS", baseXPath));
                }
            }
        }

        private XmlNode idNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@id | {0}/@Id | {0}/@ID", baseXPath));
                }
            }
        }

        private XmlNode altNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@alt | {0}/@Alt | {0}/@ALT", baseXPath));
                }
            }
        }

        private XmlNode sourceNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@source | {0}/@Source | {0}/@SOURCE | {0}/@src | {0}/@Src | {0}/@SRC", baseXPath));
                }
            }
        }

        #endregion

        private ImagePanelNode(XmlNode node)
        {
            m_inputnode = node;
            XmlDocument xDoc = node.OwnerDocument;
            
            m_transnode = xDoc.CreateElement("aspdnsf", "ImagePanel", string.Empty);
            m_transnode.Prefix = "aspdnsf";
            
            if (heightNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("Height"));
                m_transnode.Attributes["Height"].Value = heightNode.Value;
            }
            if (widthNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("Width"));
                m_transnode.Attributes["Width"].Value = widthNode.Value;
            }
            if (styleNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("Style"));
                m_transnode.Attributes["Style"].Value = styleNode.Value;
            }
            if (classNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("CssClass"));
                m_transnode.Attributes["CssClass"].Value = classNode.Value;
            }
            if (idNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("ID"));
                m_transnode.Attributes["ID"].Value = idNode.Value;
            }
            if (altNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("Alt"));
                m_transnode.Attributes["Alt"].Value = altNode.Value;
            }
            if (sourceNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("Source"));
                if (sourceNode.Value.IndexOf("/") != -1)
                {
                    m_transnode.Attributes["Source"].Value = sourceNode.Value.Substring(sourceNode.Value.IndexOf("/") + 1);
                }
                else
                {
                    m_transnode.Attributes["Source"].Value = sourceNode.Value;
                }
            }

            m_transnode.Attributes.Append(xDoc.CreateAttribute("runat"));
            m_transnode.Attributes["runat"].Value = "server";


        }

        public static explicit operator ImagePanelNode(XmlNode node)
        {
            return new ImagePanelNode(node);
        }
        
        public static implicit operator XmlNode(ImagePanelNode node)
        {
            return node.TransNode;
        }

    }
    
    public class TextPanelNode
    {
        #region Private Variables

        private XmlNode m_transnode;
        private XmlNode m_inputnode;

        #endregion

        #region Public Properties

        public XmlNode TransNode
        {
            get { return m_transnode; }
            set { m_transnode = value; }
        }

        public XmlNode InputNode
        {
            get { return m_inputnode; }
            set { m_inputnode = value; }
        }

        #endregion

        #region Node Properties

        private string baseXPath
        {
            get
            {
                return ".";
            }
        }

        private XmlNode styleNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@style | {0}/@Style| {0}/@STYLE", baseXPath));
                }
            }
        }

        private XmlNode classNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@class | {0}/@Class| {0}/@CLASS", baseXPath));
                }
            }
        }

        public XmlNode idNode
        {
            get
            {
                if (m_transnode == null)
                {
                    return null;
                }
                else
                {
                    return m_inputnode.SelectSingleNode(string.Format("{0}/@id | {0}/@Id| {0}/@ID", baseXPath));
                }
            }
        }


        #endregion

        public TextPanelNode(XmlNode node)
        {
            m_inputnode = node;
            XmlDocument xDoc = node.OwnerDocument;
            m_transnode = xDoc.CreateElement("TextPanel");

            if (styleNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("Style"));
                m_transnode.Attributes["Style"].Value = styleNode.Value;
            }
            if (classNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("CssClass"));
                m_transnode.Attributes["CssClass"].Value = classNode.Value;
            }
            if (idNode != null)
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("ID"));
                m_transnode.Attributes["ID"].Value = idNode.Value;
            }
            else
            {
                m_transnode.Attributes.Append(xDoc.CreateAttribute("ID"));
                m_transnode.Attributes["ID"].Value = String.Empty;
            }

            m_transnode.Attributes.Append(xDoc.CreateAttribute("TagType"));
            m_transnode.Attributes["TagType"].Value = node.Name;

            m_transnode.Attributes.Append(xDoc.CreateAttribute("runat"));
            m_transnode.Attributes["runat"].Value = "server";
        }

        public static explicit operator TextPanelNode(XmlNode node)
        {
            return new TextPanelNode(node);
        }

        public static implicit operator XmlNode(TextPanelNode node)
        {
            return node.TransNode;
        }
    }


    public class LayoutParser
    {
        private static string controlBase
        {
            get
            {
                return 
@"<%@ Register Src='~/controls/ImagePanel.ascx' TagName='imagepanel' TagPrefix='aspdnsf' %>
<%@ Register Src='~/controls/TextPanel.ascx' TagName='textpanel' TagPrefix='aspdnsf' %>\n";
            }
        }

        private XmlDocument doc;

        public LayoutParser(MemoryStream inputDocument)
        {
            inputDocument.Position = 0;
            doc = new XmlDocument();
            doc.Load(inputDocument);
        }

        public LayoutParser(XmlDocument inputDocument)
        {
            doc = new XmlDocument();
            doc = inputDocument;
        }

        public LayoutParser(String inputDocument)
        {
            doc = new XmlDocument();
            doc.LoadXml(inputDocument);
        }

        public void Parse(MemoryStream stream)
        {  
            
        }

        public string Parse()
        {
            return Parse(0);
        }

        public string Parse(int LayoutID)
        {
            XmlNodeList imgList = doc.SelectNodes("//node()[@type='ASPDNSFImageField']");
            XmlNodeList txtList = doc.SelectNodes("//node()[@type='ASPDNSFTextField']");

            int imgCount = imgList.Count;
            int txtCount = txtList.Count;

            foreach (XmlNode imgNode in imgList)
            {
                XmlNode newImgNode = imgNode.ParentNode.InsertAfter((ImagePanelNode)imgNode, imgNode);
                imgNode.ParentNode.RemoveChild(imgNode);
                
                WriteImageFieldToDB(newImgNode, LayoutID);
            }

            foreach (XmlNode txtNode in txtList)
            {
                if (txtNode.Attributes["ID"] == null)
                {
                    txtNode.Attributes.Append(doc.CreateAttribute("ID"));
                    txtNode.Attributes["ID"].Value = WriteTextFieldToDB(txtNode, LayoutID);
                }
                else if(txtNode.Attributes["ID"].Value.Length == 0)
                {
                    txtNode.Attributes["ID"].Value = WriteTextFieldToDB(txtNode, LayoutID);
                }

                txtNode.ParentNode.InsertAfter((TextPanelNode)txtNode, txtNode);
                txtNode.ParentNode.RemoveChild(txtNode);

            }

            string val = doc.OuterXml
                .Replace("<ImagePanel", "\n<aspdnsf:imagepanel")
                .Replace("<TextPanel", "\n<aspdnsf:textpanel");


            val = CommonLogic.ExtractBody(val);

            return controlBase + val;
            
        }

        private String WriteImageFieldToDB(XmlNode node, int LayoutID)
        {
            return WriteFieldToDB(LayoutFieldEnum.ASPDNSFImageField, node.Attributes, LayoutID);
        }

        private String WriteTextFieldToDB(XmlNode node, int LayoutID)
        {
            return WriteFieldToDB(LayoutFieldEnum.ASPDNSFTextField, node.Attributes, LayoutID);
        }

        private String WriteFieldToDB(LayoutFieldEnum lfe, XmlAttributeCollection xac, int LayoutID)
        {
            String fieldID = String.Empty;

            int LayoutFieldID = 0;

            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();

                using (SqlCommand scom = new SqlCommand())
                {
                    scom.CommandType = CommandType.StoredProcedure;
                    scom.Connection = conn;
                    scom.CommandText = "dbo.aspdnsf_insLayoutField";

                    scom.Parameters.Add(new SqlParameter("@LayoutID", SqlDbType.Int));
                    scom.Parameters.Add(new SqlParameter("@FieldType", SqlDbType.Int));
                    scom.Parameters.Add(new SqlParameter("@FieldID", SqlDbType.NVarChar));
                    scom.Parameters.Add(new SqlParameter("@LayoutFieldID", SqlDbType.Int, 4)).Direction = ParameterDirection.Output;

                    scom.Parameters["@LayoutID"].Value = LayoutID;
                    scom.Parameters["@FieldType"].Value = lfe;

                    fieldID = GetID(xac);

                    scom.Parameters["@FieldID"].Value = fieldID;
                    

                    try
                    {
                        scom.ExecuteNonQuery();
                        LayoutFieldID = Int32.Parse(scom.Parameters["@LayoutFieldID"].Value.ToString());

                        // they didn't specify an ID in the html that was uploaded or entered
                        // create one for them.
                        if (string.IsNullOrEmpty(fieldID) && LayoutFieldID != 0)
                        {
                            fieldID = GetID(xac, lfe, LayoutFieldID);

                            DB.ExecuteSQL("update dbo.LayoutField set FieldID=" + DB.SQuote(fieldID) + " where LayoutFieldID=" + LayoutFieldID.ToString());

                            //WriteLayoutFieldAttribute(conn, LayoutID, LayoutFieldID, "ID", fieldID);
                        }
                    }
                    catch { }
                }

                if (LayoutFieldID != 0)
                {
                    foreach(XmlAttribute xa in xac)
                    {
                        if (xa.Name.Equals("ID", StringComparison.OrdinalIgnoreCase) && (xa.Value.Length == 0 || !xa.Value.Equals(fieldID, StringComparison.OrdinalIgnoreCase)))
                        {
                            xa.Value = fieldID;
                        }

                        if(!xa.Name.Equals("type", StringComparison.OrdinalIgnoreCase))
                        {
                            WriteLayoutFieldAttribute(conn, LayoutID, LayoutFieldID, xa.Name, xa.Value);
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }

            return fieldID;
        }

        private void WriteLayoutFieldAttribute(SqlConnection conn, int LayoutID, int LayoutFieldID, String Name, String Value)
        {
            using (SqlCommand scom = new SqlCommand())
            {
                scom.Connection = conn;
                scom.CommandType = CommandType.StoredProcedure;
                scom.CommandText = "dbo.aspdnsf_insLayoutFieldAttribute";

                scom.Parameters.Add(new SqlParameter("@LayoutID", SqlDbType.Int));
                scom.Parameters.Add(new SqlParameter("@LayoutFieldID", SqlDbType.Int));
                scom.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar));
                scom.Parameters.Add(new SqlParameter("@Value", SqlDbType.NVarChar));

                scom.Parameters["@LayoutID"].Value = LayoutID;
                scom.Parameters["@LayoutFieldID"].Value = LayoutFieldID;
                scom.Parameters["@Name"].Value = Name;
                scom.Parameters["@Value"].Value = Value;

                try
                {
                    scom.ExecuteNonQuery();
                }
                catch { }
            }
        }

        private String GetID(XmlAttributeCollection xac)
        {
            return GetID(xac, LayoutFieldEnum.Unknown);
        }

        private String GetID(XmlAttributeCollection xac, LayoutFieldEnum lfe)
        {
            return GetID(xac, lfe, 0);
        }

        private String GetID(XmlAttributeCollection xac, LayoutFieldEnum lfe, int LayoutFieldID)
        {
            String xID = String.Empty;

            foreach (XmlAttribute xa in xac)
            {
                if (xa.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    xID = xa.Value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(xID) && LayoutFieldID > 0)
            {
                if (lfe == LayoutFieldEnum.ASPDNSFImageField)
                {
                    xID = "Image_" + LayoutFieldID.ToString();
                }
                else if (lfe == LayoutFieldEnum.ASPDNSFTextField)
                {
                    xID = "Text_" + LayoutFieldID.ToString();
                }
                else
                {
                    xID = "Unknown_" + LayoutFieldID.ToString();
                }
            }

            return xID;
        }

        private String GetAllAttributes(XmlAttributeCollection xac)
        {
            String allAttributes = String.Empty;

            foreach (XmlAttribute xa in xac)
            {
                if (!xa.Name.Equals("id", StringComparison.OrdinalIgnoreCase) && !xa.Name.Equals("type", StringComparison.OrdinalIgnoreCase))
                {
                    allAttributes += xa.Name + "=\"" + xa.Value + "\" ";
                }
            }

            return allAttributes;
        }
    }
}
