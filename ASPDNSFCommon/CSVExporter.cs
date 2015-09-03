// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AspDotNetStorefrontCommon
{
    public static class CSVExporter
    {

        public static string ExportListToCSV(List<object> objList)
        {
            //Make sure we have something to do
            if (objList.Count < 1)
            {
                return String.Empty;
            }

            PropertyInfo[] propertyList = objList[0].GetType().GetProperties();

            StringBuilder sb = new StringBuilder();


            //Generate the headers
            foreach (PropertyInfo p in propertyList)
            {
                sb.Append(p.Name + ",");
            }

            //Replace the last comma with a line break
            sb.Replace(",", Environment.NewLine, sb.ToString().LastIndexOf(','), 1);

            //Export the items
            for (int i = 0; i < objList.Count; i++)
            {
                foreach (PropertyInfo p in propertyList)
                {
                    try
                    {
                        sb.Append(p.GetValue(objList[i], null).ToString().Replace(","," ") + ",");
                        //sb.Append(objList[i].GetType().GetProperty(p.Name).ToString() + ",");
                    }
                    catch
                    {
                        sb.Append(",");
                    }
                }

                sb.AppendLine();
            }

            //Replace the last comma with a line break
            sb.Replace(",", Environment.NewLine, sb.ToString().LastIndexOf(','), 1);


            return sb.ToString();
        }
    }
}
