// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data.OleDb;
using System.Data;

namespace AspDotNetStorefrontCommon
{
    /// <summary>
    /// Summary description for Excel.
    /// </summary>
    public class Excel
    {
        public Excel()
        { }

        static public DataSet GetDS(String ExcelFile, String SheetName)
        {
            String[] files = { ExcelFile };
            return GetDS(files, SheetName);
        }

        // returns dataset Table1 matching excel file:
        static public DataSet GetDS(String[] ExcelFiles, String SheetName)
        {
            try
            {

                // Create new DataSet to hold information from the worksheet.
                DataSet objDataset1 = new DataSet();

                foreach (string filename in ExcelFiles)
                {
                    // Create connection string variable. Modify the "Data Source"
                    // parameter as appropriate for your environment.
                    String sConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filename + ";" + "Extended Properties=\"Excel 8.0;HDR=No;IMEX=1;\"";

                    // Create connection object by using the preceding connection string.
                    OleDbConnection objConn = new OleDbConnection(sConnectionString);

                    // Open connection with the database.
                    objConn.Open();

                    // The code to follow uses a SQL SELECT command to display the data from the worksheet.
                    try
                    {
                        // Create new OleDbCommand to return data from worksheet.
                        OleDbCommand objCmdSelect = new OleDbCommand(String.Format("SELECT * FROM [{0}$]", SheetName), objConn);

                        // Create new OleDbDataAdapter that is used to build a DataSet
                        // based on the preceding SQL SELECT statement.
                        OleDbDataAdapter objAdapter1 = new OleDbDataAdapter();

                        // Pass the Select command to the adapter.
                        objAdapter1.SelectCommand = objCmdSelect;

                        // Fill the DataSet with the information from the worksheet.
                        objAdapter1.Fill(objDataset1, "Table1");

                        // Clean up objects.
                        objConn.Close();
                    }
                    catch
                    {
                        objConn.Close();
                    }

                }

                return objDataset1;

            }
            catch
            {
                return null;
            }
        }
    }
}

