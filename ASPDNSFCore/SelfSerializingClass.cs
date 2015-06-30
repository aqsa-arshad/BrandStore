// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace AspDotNetStorefrontCore
{
   public abstract class SelfSerializer
    {
       
        private XmlSerializer serializer
        {
            get { return new XmlSerializer(this.GetType()); }
        }
        #region "Output Function"
        
        public string ToXMLString()
        {
            StreamReader xRdr = new StreamReader(ToStream());
            return xRdr.ReadToEnd();
        }
        
        public XmlDocument ToXMLDoc()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(ToStream());
            return xDoc;
        }
        
        public MemoryStream ToStream()
        {
            MemoryStream xStream = new MemoryStream();
            serializer.Serialize(xStream, this);
            xStream.Flush();
            xStream.Position = 0;
            return xStream;
        }
        public void ToFile(string file)
        {
            ToXMLDoc().Save(file);
        }
        #endregion

       }

    public static class SelfSerializerFactory<tType> where tType : SelfSerializer
    {
        private static XmlSerializer serializer
        {
            get {return new XmlSerializer(typeof(tType));}
        }

        public static tType FromXMLString(string xmlString)
        {
            MemoryStream memStream = new MemoryStream();
            StreamWriter xWri = new StreamWriter(memStream);
            xWri.AutoFlush = true;
            xWri.Write(xmlString);
            return FromStream(memStream);
        }
        public static tType FromXMLDoc(System.Xml.XmlDocument xmlDoc)
        {
            MemoryStream xStream = new MemoryStream();
            xmlDoc.Save(xStream);
            xStream.Flush();
            xStream.Position = 0;
            return FromStream(xStream);

        }
        public static tType FromStream(Stream stream)
        {
            stream.Position = 0;
            try
            {

                return (tType)serializer.Deserialize(stream);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static tType FromFile(string filePath)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filePath);
            return FromXMLDoc(xDoc);
        }
    }
}
