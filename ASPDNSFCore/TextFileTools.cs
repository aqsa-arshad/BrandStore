// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Data;
using System.Collections;

namespace AspDotNetStorefrontCore 
{

	public class TextFileFormatClass
	{

		public class ImportFormat
		{
			public bool delimiterNamed;
			public int FirstLine;
			public string lineDelimiter;
			public string valueDelimiter;
			public string textDelimiter;
			public FileFormatEnum FileFormat;
			public formatColumn[] fmtCols;
			public enum FileFormatEnum : short
			{
				FlatFile = 0,
				Delimited = 1,
				XML = 2,
				ODBC = 3
			}

			public ImportFormat()
			{
			}

			public DataTable CreateTempTable()
			{
				if (fmtCols.Length > 0) 
				{
					DataTable tmpDt = new DataTable();
					foreach (formatColumn col in fmtCols) 
					{
						tmpDt.Columns.Add(col.ColumnName);
					}
					return tmpDt;
				} 
				else 
				{
					return null;
				}
			}
		}
		public class ExportFormat
		{
			public bool delimiterNamed;
			public int FirstLine;
			public string lineDelimiter;
			public string valueDelimiter;
			public string textDelimiter;
			public FileFormatEnum FileFormat;
			public string defaultFileName;
			public formatColumn[] fmtCols;
			public enum FileFormatEnum : short
			{
				FlatFile = 0,
				Delimited = 1,
				XML = 2,
				ODBC = 3
			}

			public ExportFormat()
			{
			}

			public DataTable CreateTempTable()
			{
				if (fmtCols.Length > 0) 
				{
					DataTable tmpDt = new DataTable();
					foreach (formatColumn fmtCol in fmtCols) 
					{
						DataColumn col = new DataColumn();
						col.ColumnName = fmtCol.refColumnName;
						System.Type dType;
						if (fmtCol.Format.Equals("string", StringComparison.InvariantCultureIgnoreCase) || 
                            fmtCol.Format.Equals("str", StringComparison.InvariantCultureIgnoreCase)) 
						{
							dType = typeof(string);
						} 
						else if (fmtCol.Format.Equals("double", StringComparison.InvariantCultureIgnoreCase) || 
                            fmtCol.Format.Equals("single", StringComparison.InvariantCultureIgnoreCase)) 
						{
							dType = typeof(double);
						} 
						else if (fmtCol.Format.Equals("boolean", StringComparison.InvariantCultureIgnoreCase) || 
                            fmtCol.Format.Equals("bool", StringComparison.InvariantCultureIgnoreCase) || 
                            fmtCol.Format.Equals("bit", StringComparison.InvariantCultureIgnoreCase)) 
						{
							dType = typeof(bool);
						} 
						else if (fmtCol.Format.Equals("date", StringComparison.InvariantCultureIgnoreCase) || 
                            fmtCol.Format.Equals("datetime", StringComparison.InvariantCultureIgnoreCase)) 
						{
							dType = typeof(DateTime);
						} 
						else if (fmtCol.Format.Equals("int", StringComparison.InvariantCultureIgnoreCase) || 
                            fmtCol.Format.Equals("integer", StringComparison.InvariantCultureIgnoreCase) || 
                            fmtCol.Format.Equals("long", StringComparison.InvariantCultureIgnoreCase) || 
                            fmtCol.Format.Equals("short", StringComparison.InvariantCultureIgnoreCase )) 
						{
							dType = typeof(int);
						} 
						else 
						{
							dType = typeof(string);
						}
						col.DataType = dType;
						tmpDt.Columns.Add(col);
					}
					return tmpDt;
				} 
				else 
				{
					return null;
				}
			}
		}
		public class formatColumn
		{
			public string ColumnName;
			public string refColumnName;
			public int ColumnNumber;
			public bool Enabled;
			public int StartPos;
			public int EndPos;
			public string Type;
			public string Format;
		}



		private DataSet FormatDS = new DataSet();
		private DataTable ImportFormatDT;
		private DataTable ExportFormatDT;

		public TextFileFormatClass(string xmlFilePath)
		{
			FormatDS.ReadXml(xmlFilePath);
			ImportFormatDT = FormatDS.Tables["ImportFormat"];
			ExportFormatDT = FormatDS.Tables["ExportFormat"];
		}

		public DataTable GetImportFormatList 
		{
			get 
			{
				return ImportFormatDT;
			}
		}

		public DataTable GetExportFormatList 
		{
			get 
			{
				return ExportFormatDT;
			}
		}

		public ImportFormat GetImportFormat(int inputFormatID)
		{
			DataRow[] Fmt = ImportFormatDT.Select("ID = " + inputFormatID);
			DataRow fmtDr = Fmt[0];
			ImportFormat iFmt = new ImportFormat();
			iFmt.lineDelimiter = DB.RowField(fmtDr, "lineDelim");
			iFmt.valueDelimiter = DB.RowField(fmtDr, "valueDelim");
			iFmt.textDelimiter = DB.RowField(fmtDr, "textDelim");
			string FmtType = DB.RowField(fmtDr, "type");
			if (FmtType.Equals("named", StringComparison.InvariantCultureIgnoreCase)) 
			{
				iFmt.delimiterNamed = true;
				iFmt.FirstLine = 1;
			} 
			else 
			{
				iFmt.delimiterNamed = false;
				iFmt.FirstLine = 0;
			}
			string Format = DB.RowField(fmtDr, "format");
			if (Format.Equals("delimited", StringComparison.InvariantCultureIgnoreCase)) 
			{
				iFmt.FileFormat = ImportFormat.FileFormatEnum.Delimited;
			} 
			else if (Format.Equals("flat", StringComparison.InvariantCultureIgnoreCase)) 
			{
				iFmt.FileFormat = ImportFormat.FileFormatEnum.FlatFile;
			}
			DataRow[] formatColumns = fmtDr.GetChildRows("ImportFormat_Column");
			int fmtCount = formatColumns.Length; // - 1;
			// TODO: NotImplemented statement: ICSharpCode.SharpRefactory.Parser.AST.VB.ReDimStatement
			iFmt.fmtCols = new formatColumn[fmtCount];


			for (int z = 0; z <= fmtCount-1; z++) 
			{
				DataRow iFmtRow = formatColumns[z];
				formatColumn iFmtC = new formatColumn();
				iFmtC.ColumnName = DB.RowField(iFmtRow, "OrderCol");
				iFmtC.refColumnName = DB.RowField(iFmtRow, "LogCol");
				
				int ColNum = -1;
				if ( DB.RowField(iFmtRow, "ColNum") != "")
				{
					ColNum=DB.RowFieldInt(iFmtRow, "ColNum");
				}
				iFmtC.ColumnNumber = ColNum; //iFmtC.ColumnNumber = DB.RowFieldInt(iFmtRow, "ColNum");
				iFmtC.Enabled = DB.RowFieldBool(iFmtRow, "enabled");
				iFmtC.Type = DB.RowField(iFmtRow, "type");
				iFmtC.Format = DB.RowField(iFmtRow, "format");
				iFmtC.StartPos = DB.RowFieldInt(iFmtRow, "spos");
				iFmtC.EndPos = DB.RowFieldInt(iFmtRow, "epos");
				iFmt.fmtCols[z] = iFmtC;
			}
			return iFmt;
		}

		public ExportFormat GetExportFormat(int outputFormatID)
		{
			DataRow[] Fmt = ExportFormatDT.Select("ID = " + outputFormatID);
			DataRow fmtDr = Fmt[0];
			ExportFormat xFmt = new ExportFormat();
			xFmt.lineDelimiter = DB.RowField(fmtDr, "lineDelim");
			xFmt.valueDelimiter = DB.RowField(fmtDr, "valueDelim");
			xFmt.textDelimiter = DB.RowField(fmtDr, "textDelim");
			xFmt.defaultFileName = DB.RowField(fmtDr, "defaultFileName");
			string FmtType = DB.RowField(fmtDr, "type");
			if (FmtType.Equals("named", StringComparison.InvariantCultureIgnoreCase)) 
			{
				xFmt.delimiterNamed = true;
				xFmt.FirstLine = 1;
			} 
			else 
			{
				xFmt.delimiterNamed = false;
				xFmt.FirstLine = 0;
			}
			string Format =DB.RowField(fmtDr, "format");
			if (Format.Equals("delimited", StringComparison.InvariantCultureIgnoreCase)) 
			{
				xFmt.FileFormat = ExportFormat.FileFormatEnum.Delimited;
			} 
			else if (Format.Equals("flat", StringComparison.InvariantCultureIgnoreCase)) 
			{
				xFmt.FileFormat = ExportFormat.FileFormatEnum.FlatFile;
			}
			DataRow[] formatColumns = fmtDr.GetChildRows("ExportFormat_xColumn");
			int fmtCount = formatColumns.Length; // - 1;
			// TODO: NotImplemented statement: ICSharpCode.SharpRefactory.Parser.AST.VB.ReDimStatement
			//ReDim xFmt.fmtCols(fmtCount)
			xFmt.fmtCols = new formatColumn[fmtCount];

			for (int z = 0; z <= fmtCount-1; z++) 
			{
				DataRow xFmtRow = formatColumns[z];
				formatColumn xFmtC = new formatColumn();
				xFmtC.ColumnName = DB.RowField(xFmtRow, "OrderCol");
				xFmtC.refColumnName = DB.RowField(xFmtRow, "LogCol");
				int ColNum = -1;
				if ( DB.RowField(xFmtRow, "ColNum") != "")
				{
					ColNum=DB.RowFieldInt(xFmtRow, "ColNum");
				}
				else
				{
					ColNum=z;
				}

				xFmtC.ColumnNumber = ColNum; //DB.RowFieldInt(xFmtRow, "ColNum");
				xFmtC.Enabled = DB.RowFieldBool(xFmtRow, "enabled");
				xFmtC.Type = DB.RowField(xFmtRow, "type");
				xFmtC.Format = DB.RowField(xFmtRow, "format");
				xFmtC.StartPos = DB.RowFieldInt(xFmtRow, "spos");
				xFmtC.EndPos = DB.RowFieldInt(xFmtRow, "epos");
				xFmt.fmtCols[z] = xFmtC;
			}
			return xFmt;
		}

		public DataTable ImportFile(string FileName, int ImportFormatID)
		{
			string FileContents = GetFileContents(FileName);
			DataTable tmpDt = null;
			ImportFormat ifmt = GetImportFormat(ImportFormatID);
			if (ifmt.FileFormat == ImportFormat.FileFormatEnum.Delimited) 
			{
				tmpDt = ImportDelimitedFile(FileContents, ifmt);
			} 
			else if (ifmt.FileFormat == ImportFormat.FileFormatEnum.FlatFile) 
			{
			}
			return tmpDt;
		}
	

		public void ExportToFile(DataTable SourceTable, string FileName, int ExportFormatID)
		{
			ExportFormat xFmt = GetExportFormat(ExportFormatID);
			DataTable tmp = BuildExportTable(SourceTable, xFmt);
			if (xFmt.FileFormat == ExportFormat.FileFormatEnum.Delimited) 
			{
				ExportDelimitedFile(tmp, FileName, xFmt);
			}
		}

		public void ExportToFile(IDataReader SourceReader, string FileName, int ExportFormatID)
		{
			DataTable tmp = Reader2Table(SourceReader, true);
			ExportToFile(tmp, FileName, ExportFormatID);
		}

		public string ExportToString(DataTable SourceTable, int ExportFormatID)
		{
			string outPut = null;
			ExportFormat xFmt = GetExportFormat(ExportFormatID);
			DataTable tmp = BuildExportTable(SourceTable, xFmt);
			if (xFmt.FileFormat == ExportFormat.FileFormatEnum.Delimited) 
			{
				outPut = BuildDelimitedFile(tmp, xFmt);
			}
			return outPut;
		}

		public string ExportToString(IDataReader SourceReader, int ExportFormatID)
		{
			DataTable tmp = Reader2Table(SourceReader,true);
			return ExportToString(tmp, ExportFormatID);
		}

		private DataTable ImportDelimitedFile(string FileContents, ImportFormat iFmt)
		{
			string[] fileLines = FileContents.Split(iFmt.lineDelimiter.ToCharArray());
			DataTable tmpDt = iFmt.CreateTempTable();
			string[] columnHeader = fileLines[0].Split(iFmt.valueDelimiter.ToCharArray());
			if (iFmt.delimiterNamed) 
			{ 

				for (int columnNumber = 0; columnNumber <= columnHeader.Length - 1; columnNumber++) 
				{
					string columnName = columnHeader[columnNumber];
					foreach (formatColumn r in iFmt.fmtCols) 
					{
						if (columnName == r.refColumnName) 
						{
							r.ColumnNumber = columnNumber;
						}
					}
				}
			}

		//validate
			foreach (formatColumn r in iFmt.fmtCols) 
			{
				if (r.Enabled && (r.ColumnNumber < 0)) 
				{
					//if any enabled rows dont have a colnum, then the import cant continue
					throw(new Exception("Not all required columns found in header... (" + r.ColumnName + ")"));
				}
			}

			for (int line = iFmt.FirstLine; line <= fileLines.Length - 1; line++) 
			{
				string[] Values = fileLines[line].Split(iFmt.valueDelimiter.ToCharArray());
				DataRow nr = tmpDt.NewRow();
				foreach (formatColumn fc in iFmt.fmtCols) 
				{
					if (fc.ColumnNumber < Values.Length) 
					{
						string value = Values[fc.ColumnNumber];
						if (iFmt.textDelimiter.Length > 0) 
						{
							value = value.Replace(iFmt.textDelimiter, "");
						}
						if (fc.Type.Equals("date", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("datetime", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = txtFieldDate(value, fc.Format);
						} 
						else if (fc.Type.Equals("string", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("str", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = value;
						} 
						else if (fc.Type.Equals("double", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("single", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = txtFieldDouble(value);
						} 
						else if (fc.Type.Equals("int", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("integer", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("long", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("short", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = txtFieldInt(value);
						} 
						else if (fc.Type.Equals("bool", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("boolean", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("bit", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = txtFieldBool(value, fc.Format);
						} 
						else 
						{
							nr[fc.ColumnName] = value;
						}
					}
				}
				if (Values.Length == columnHeader.Length) 
				{
					tmpDt.Rows.Add(nr);
				}
			}
			return tmpDt;
		}

		private DataTable ImportFlatFile(string FileContents, ImportFormat iFmt)
		{
			string[] fileLines = FileContents.Split(iFmt.lineDelimiter.ToCharArray());
			DataTable tmpDt = iFmt.CreateTempTable();
			for (int line = iFmt.FirstLine; line <= fileLines.Length - 1; line++) 
			{
				DataRow nr = tmpDt.NewRow();
				foreach (formatColumn fc in iFmt.fmtCols) 
				{
					if (fc.Enabled) 
					{
						string value = fileLines[line].Substring(fc.StartPos, fc.EndPos);
						if (fc.Type.Equals("date", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("datetime", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = txtFieldDate(value, fc.Format);
						} 
						else if (fc.Type.Equals("string", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("str", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = value;
						} 
						else if (fc.Type.Equals("double", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("single", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = txtFieldDouble(value);
						} 
						else if (fc.Type.Equals("int", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("integer", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("long", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("short", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = txtFieldInt(value);
						} 
						else if (fc.Type.Equals("bool", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("boolean", StringComparison.InvariantCultureIgnoreCase) || 
                            fc.Type.Equals("bit", StringComparison.InvariantCultureIgnoreCase)) 
						{
							nr[fc.ColumnName] = txtFieldBool(value, fc.Format);
						} 
						else 
						{
							nr[fc.ColumnName] = value;
						}
					}
				}
				tmpDt.Rows.Add(nr);
			}
			return tmpDt;
		}

		private void ExportDelimitedFile(DataTable SourceTable, string FileName, ExportFormat xFmt)
		{
			string df = BuildDelimitedFile(SourceTable, xFmt);
			SaveTextToFile(df, FileName);
		}

		private string BuildDelimitedFile(DataTable SourceTable, ExportFormat xFmt)
		{
			StringBuilder expStr = new StringBuilder(4096);
			if (xFmt.delimiterNamed) 
			{
				foreach (DataColumn col in SourceTable.Columns) 
				{
					expStr.Append(col.ColumnName + xFmt.valueDelimiter);
				}
				expStr.Append(xFmt.lineDelimiter);
			}
			foreach (DataRow row in SourceTable.Rows) 
			{
				for (int x = 0; x <= SourceTable.Columns.Count - 1; x++) 
				{
					string d;
					try 
					{
						d = row[x].ToString();
					} 
					catch 
					{
						d = "";
					}
					if (SourceTable.Columns[x].DataType == typeof(string)) 
					{
						if (xFmt.textDelimiter.Length > 0) 
						{
							d = xFmt.textDelimiter + d + xFmt.textDelimiter;
						}
					}
					if (x != SourceTable.Columns.Count - 1) 
					{
						d = d + xFmt.valueDelimiter;
					}
					expStr.Append(d);
				}
				expStr.Append(xFmt.lineDelimiter);
			}
			return expStr.ToString();
		}

		private DataTable BuildExportTable(DataTable SourceTable, ExportFormat xFmt)
		{
			ArrayList cols = new ArrayList();
			ArrayList validColumns = new ArrayList();
			foreach (formatColumn fRow in xFmt.fmtCols) 
			{
				if (fRow.Enabled) 
				{
					string colName = fRow.ColumnName;
					if (SourceTable.Columns.IndexOf(colName) > -1) 
					{
						cols.Add(colName);
						validColumns.Add(fRow);
					}
				}
			}
			DataTable tmp = xFmt.CreateTempTable();
			foreach (DataRow sRow in SourceTable.Rows) 
			{
				DataRow nr = tmp.NewRow();
				foreach (formatColumn col in validColumns) 
				{
					nr[col.refColumnName] = DB.RowField(sRow, col.ColumnName);
				}
				tmp.Rows.Add(nr);
			}
			return tmp;
		}

		public string ReaderToHTML(IDataReader rdr, bool header)
		{
			DataTable tbl = Reader2Table(rdr, false);
			return TableToHTML(tbl, header);
		}

		public string TableToHTML(DataTable tbl, bool header)
		{
			StringBuilder t2sb = new StringBuilder(4096);
			t2sb.Append("<table>");
			if (header) 
			{
				t2sb.Append("<tr><td><b>");
				t2sb.Append(tbl.TableName);
				t2sb.Append("<b></td></tr>");
				t2sb.Append("\n");
				t2sb.Append("<tr>");
				foreach (DataColumn col in tbl.Columns) 
				{
					t2sb.Append("<td>");
					t2sb.Append(col.ColumnName + "\t");
					t2sb.Append("</td>");
				}
				t2sb.Append("</tr>");
				t2sb.Append("\n");
			}
			foreach (DataRow row in tbl.Rows) 
			{
				t2sb.Append("<tr>");
				foreach (DataColumn col in row.Table.Columns) 
				{
					t2sb.Append("<td>");
					t2sb.Append(row[col.ColumnName].ToString() + "\t");
					t2sb.Append("</td>");
				}
				t2sb.Append("</tr>");
				t2sb.Append("\n");
			}
			if (header) 
			{
				t2sb.Append("<tr><td>");
				t2sb.Append(tbl.Rows.Count + " Rows" + "\n");
				t2sb.Append("</td></tr>");
				t2sb.Append("\n");
			}
			t2sb.Append("</table>");
			return t2sb.ToString();
		}

		public string TableToString(DataTable tbl)
		{
			StringBuilder t2sb = new StringBuilder(4096);
			t2sb.Append("Table: " + tbl.TableName + "\n");
			foreach (DataColumn col in tbl.Columns) 
			{
				t2sb.Append(col.ColumnName + "\t");
			}
			t2sb.Append("\n");
			foreach (DataRow row in tbl.Rows) 
			{
				foreach (DataColumn col in row.Table.Columns) 
				{
					t2sb.Append(row[col.ColumnName].ToString() + "\t");
				}
				t2sb.Append("\n");
			}
			t2sb.Append(tbl.Rows.Count + " Rows" + "\n");
			return t2sb.ToString();
		}

		public string GetFileContents(string FullPath)
		{
			string strContents;
			StreamReader objReader;
			objReader = new StreamReader(FullPath);
			strContents = objReader.ReadToEnd();
			objReader.Close();
			return strContents;
		}

		public bool SaveTextToFile(string strData, string FullPath)
		{
			bool bAns = false;
			StreamWriter objReader;
			objReader = new StreamWriter(FullPath);
			objReader.Write(strData);
			objReader.Close();
			bAns = true;
			return bAns;
		}

		public System.DateTime txtFieldDate(string dateValue)
		{
			return txtFieldDate(dateValue,"");
		}

		public System.DateTime txtFieldDate(string dateValue, string format)
		{
			System.DateTime myDate = System.DateTime.MinValue;
			if (dateValue.Trim() != "")
			{
				try 
				{
					if (format == "") 
					{
						myDate = System.DateTime.Parse(dateValue);
					} 
					else 
					{
						myDate = DateTime.ParseExact(dateValue, format, new System.Globalization.CultureInfo("en-US"));
					}
				} 
				catch 
				{
				}
				if (myDate.Equals(System.DateTime.MinValue)) 
				{
					int Year=0;
					int Month=0;
					int Day=0;
					int Hour=0;
					int Min=0;
					int Sec=0;

					try
					{
						int Ypos = format.IndexOf("yyyy");
						int Mpos = format.IndexOf("MM");
						int Dpos = format.IndexOf("dd");
						Year = int.Parse(dateValue.Substring(Ypos, 4));
						Month = int.Parse(dateValue.Substring(Mpos, 2));
						Day = int.Parse(dateValue.Substring(Dpos, 2));
						myDate = new System.DateTime(Year, Month, Day);
					}
					catch{}
					try 
					{
						int Hpos = format.IndexOf("HH");
						int Npos = format.IndexOf("mm");
						int Spos = format.IndexOf("ss");
						Hour = int.Parse(dateValue.Substring(Hpos, 2));
						Min = int.Parse(dateValue.Substring(Npos, 2));
						Sec = int.Parse(dateValue.Substring(Spos, 2));
						myDate = new System.DateTime(Year, Month, Day, Hour, Min, Sec);
					} 
					catch {}
				}
			}
			return myDate;
		}

		public int txtFieldInt(string value)
		{
			int intValue;
			try 
			{
				intValue = int.Parse(value);
				return intValue;
			} 
			catch 
			{
				return 0;
			}
		}

		public int txtFieldInt(string value, int DefaultValue)
		{
			int intValue;
			try 
			{
				intValue = int.Parse(value);
				return intValue;
			} 
			catch 
			{
				return DefaultValue;
			}
		}

		public double txtFieldDouble(string value)
		{
			double numValue;
			try 
			{
				numValue = double.Parse(value);
				return numValue;
			} 
			catch 
			{
				return 0;
			}
		}

		public double txtFieldDouble(string value, double DefaultValue)
		{
			double numValue;
			try 
			{
				numValue = double.Parse(value);
				return numValue;
			} 
			catch 
			{
				return DefaultValue;
			}
		}

		public bool txtFieldBool(string value)
		{
			try 
			{
				string s = value.ToString().ToUpperInvariant();
				return (s == "TRUE" || s == "YES" || s == "1");
			} 
			catch 
			{
				return false;
			}
		}

		public bool txtFieldBool(string value, string format)
		{
			return txtFieldBool(value, format, false);
		}

		public bool txtFieldBool(string value, string format, bool caseSensitive)
		{
			try 
			{
				if (!(caseSensitive)) 
				{
					value = value.ToUpperInvariant();
					format = format.ToUpperInvariant();
				}
				if (format == "") 
				{
					return txtFieldBool(value);
				} 
				else 
				{
					string[] fmt = format.Split("|".ToCharArray());
					if (value == fmt[0]) 
					{
						return true;
					} 
					else 
					{
						return false;
					}
				}
			} 
			catch 
			{
				return false;
			}
		}

		public System.Data.DataTable Reader2Table(IDataReader _reader, bool simple)
		{
			System.Data.DataTable _table = _reader.GetSchemaTable();
			System.Data.DataTable _dt = new System.Data.DataTable();
			System.Data.DataColumn _dc;
			System.Data.DataRow _row;
			System.Collections.ArrayList _al = new System.Collections.ArrayList();
			for (int i = 0; i <= _table.Rows.Count - 1; i++) 
			{
				_dc = new System.Data.DataColumn();
				if (!(_dt.Columns.Contains(_table.Rows[i]["ColumnName"].ToString()))) 
				{
					_dc.ColumnName = _table.Rows[i]["ColumnName"].ToString();
					if (!(simple)) 
					{
						_dc.Unique = Convert.ToBoolean(_table.Rows[i]["IsUnique"]);
						_dc.AllowDBNull = Convert.ToBoolean(_table.Rows[i]["AllowDBNull"]);
						_dc.ReadOnly = Convert.ToBoolean(_table.Rows[i]["IsReadOnly"]);
					}
					_al.Add(_dc.ColumnName);
					_dt.Columns.Add(_dc);
				}
			}
			while (_reader.Read()) 
			{
				_row = _dt.NewRow();
				for (int i = 0; i <= _al.Count - 1; i++) 
				{
					_row[System.Convert.ToString(_al[i])] = _reader[System.Convert.ToString(_al[i])];
				}
				_dt.Rows.Add(_row);
			}
			return _dt;
		}
	}

}
