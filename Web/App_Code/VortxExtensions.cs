using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Web;
using System.Globalization;
using System.Runtime.CompilerServices;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.IO;
using System.Collections;
using Vortx.VortxFramework;

/// <summary>
/// Receipt Xslt Extension Class
/// </summary>
public class VortxExtensions : XSLTExtensionBase
{
    #region Constructor

    /// <summary>
    /// VortxExtensions Constructor
    /// </summary>
    public VortxExtensions()
        : this(null, 1, null)
    {
    }

    public VortxExtensions(Customer cust, int SkinID, Dictionary<string, EntityHelper> EntityHelpers)
        : base(cust, SkinID)
    {
    }

    #endregion

    #region Methods

    public static XPathNodeIterator ProductImageCollectionXML(int ProductID, string ImageFileNameOverride, string SKU, string colors )
    {
        ProductImageCollection pic = new ProductImageCollection(ProductID, ImageFileNameOverride, SKU, 1, "en-US", colors);
        XmlDocument doc = pic.GetXMLBySize();
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator ret = nav.Select(".");

        return ret;
    }

    public static String ProductImageCollectionString(int ProductID, string ImageFileNameOverride, string SKU, string colors)
    {
        ProductImageCollection prodcutImageCollection = new ProductImageCollection(ProductID, ImageFileNameOverride, SKU, 1, "en-US", colors);
		XmlDocument doc = prodcutImageCollection.GetXMLBySize();
        return doc.InnerXml;
    }

	public static XPathNodeIterator VariantImageCollectionXML(int productId)
	{
		VariantImageCollection variantImageCollection = new VariantImageCollection(productId, 1, "en-US");
		XmlDocument doc = variantImageCollection.GetXMLBySize();
		XPathNavigator nav = doc.CreateNavigator();
		XPathNodeIterator ret = nav.Select(".");

		return ret;
	}

	public static String VariantImageCollectionString(int productId)
	{
		VariantImageCollection variantImageCollection = new VariantImageCollection(productId, 1, "en-US");
		XmlDocument doc = variantImageCollection.GetXMLBySize();
		return doc.InnerXml;
	}

    public static XPathNodeIterator StringToNode(string s)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(s);
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator ret = nav.Select(".");
        return ret;
    }

    #endregion

}
