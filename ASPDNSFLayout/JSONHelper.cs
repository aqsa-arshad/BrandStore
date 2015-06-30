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
using System.Runtime.Serialization.Json;
using System.IO;

namespace AspDotNetStorefrontLayout
{
    public class JSONHelper
    {
        public static string Serialize(object graph)
        {
            var ser = new DataContractJsonSerializer(graph.GetType());
            
            string serialized = string.Empty;

            using (var strm = new MemoryStream())
            {
                ser.WriteObject(strm, graph);
                serialized = Encoding.UTF8.GetString(strm.ToArray());
            }
            
            return serialized;
        }

        public static T Deserialize<T>(string jsonSerialized) where T : class
        {
            return Deserialize(jsonSerialized, typeof(T)) as T;
        }

        public static object Deserialize(string jsonSerialized, Type type)
        {
            object deserialized = null;
            var ser = new DataContractJsonSerializer(type);

            var raw = Encoding.UTF8.GetBytes(jsonSerialized);
            using (var strm = new MemoryStream(raw))
            {
                //x = (T)(ser.ReadObject(strm));
                deserialized = ser.ReadObject(strm);
            }

            return deserialized;
        }

    }
}
