// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

#endregion

namespace AspDotNetStorefront.Promotions
{
	public static class SerializationController
	{
		#region Public Methods

		public static XElement SerializeObject<T> (T item)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(writer, item);
				stream.Position = 0;

				using (StreamReader reader = new StreamReader(stream))
				{
					return XElement.Parse(reader.ReadToEnd(), LoadOptions.None);
				}
			}
		}

		public static T DeserializeObject<T> (XElement element) where T : class
		{
			XmlReader reader = element.CreateReader();
			XmlSerializer serializer = new XmlSerializer(typeof(T));
			return serializer.Deserialize(reader) as T;
		}

		#endregion
	}
}
