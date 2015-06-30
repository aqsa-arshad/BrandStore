// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using System.Web;
using System.Xml;
using System.Globalization;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Web.Routing;
using System.Configuration;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for SE.
	/// </summary>
	public class SE
	{
		public SE()	{}

		static public String MakeEntityLink(String EntityName, int EntityID, String SEName)
		{
			if (SEName == null)
			{
                SEName = String.Empty;
			}
			String URL = String.Empty;
			if(!AppLogic.IsAdminSite)
			{
				String tmp = String.Empty;
				if(SEName.Length != 0)
				{
					tmp = MungeName(SEName);
				}
				else
				{
					tmp = SE.GetEntitySEName(EntityName,EntityID);
				}

                RouteValueDictionary dictionary = new RouteValueDictionary();
                dictionary.Add(EntityName + "ID", EntityID);
	            dictionary.Add("SEName", tmp);
	
                VirtualPathData pathInfo = RouteTable.Routes.GetVirtualPath(null, EntityName, dictionary);

                if (pathInfo != null)
                {
                    URL = pathInfo.VirtualPath;
                }
                //URL = String.Format("{0}-{1}-{2}.aspx",EntityName.Substring(0,1).ToLowerInvariant(),EntityID.ToString(),tmp);
			}
			else
			{
				URL = String.Format("show{0}.aspx?{1}ID={2}",EntityName,EntityName,EntityID.ToString());
			}
			return URL.ToLowerInvariant();
		}

		static public String MakeCategoryLink(int CategoryID, String SEName)
		{
			return MakeEntityLink("Category",CategoryID,SEName);
		}

		static public String MakeSectionLink(int SectionID, String SEName)
		{
			return MakeEntityLink("Section",SectionID,SEName);
		}

        static public String MakeManufacturerLink(int ManufacturerID, String SEName)
        {
            return MakeEntityLink("Manufacturer", ManufacturerID, SEName);
        }
        static public String MakeDistributorLink(int DistributorID, String SEName)
        {
            return MakeEntityLink("Distributor", DistributorID, SEName);
        }

        static public String MakeGenreLink(int GenreID, String SEName)
        {
            return MakeEntityLink("Genre", GenreID, SEName);
        }

        static public String MakeVectorLink(int VectorID, String SEName)
        {
            return MakeEntityLink("Vector", VectorID, SEName);
        }

        static public String MakeObjectLink(String ObjectName, int ObjectID, String SEName)
		{
            string objectprefix = ObjectName.Substring(0, 1).ToLowerInvariant();
			if (SEName == null)
			{
				SEName = String.Empty;
			}
			String URL = String.Empty;
			String tmp = String.Empty;
			if(SEName.Length != 0)
			{
				tmp = MungeName(SEName);
			}
			else
			{
				tmp = SE.GetObjectSEName(ObjectName,ObjectID);
			}

            RouteValueDictionary dictionary = new RouteValueDictionary();
            dictionary.Add(ObjectName + "ID", ObjectID);
            dictionary.Add("SEName", tmp);

            VirtualPathData pathInfo = RouteTable.Routes.GetVirtualPath(null, ObjectName, dictionary);

            if (pathInfo != null)
            {
                URL = pathInfo.VirtualPath;
            }
  	  	  		
  	  	  	//URL = String.Format("{0}-{1}-{2}.aspx",objectprefix,ObjectID.ToString(),tmp);
			return URL.ToLowerInvariant();
		}

		static public String MakeProductLink(int ProductID, String SEName)
		{
			return MakeObjectLink("Product",ProductID,SEName);
		}

		static public String MakeObjectAndEntityLink(String ObjectName, String EntityName, int ObjectID, int EntityID, String SEName)
		{
            return MakeObjectLink(ObjectName, ObjectID, SEName);
		}

		static public String MakeProductAndEntityLink(String EntityName, int ProductID, int EntityID, String SEName)
		{
			return MakeObjectAndEntityLink("Product",EntityName,ProductID,EntityID,SEName);
		}

		static public String MakeProductAndCategoryLink(int ProductID, int CategoryID, String SEName)
		{

			return MakeProductAndEntityLink("Category",ProductID,CategoryID,SEName);
		}

		static public String MakeProductAndSectionLink(int ProductID, int SectionID, String SEName)
		{
			return MakeProductAndEntityLink("Section",ProductID,SectionID,SEName);
		}

        static public String MakeDriverLink(String TopicName)
        {
            String TopicNameWithoutLocale = TopicName;
            if (TopicNameWithoutLocale.IndexOf('.') != -1) //File-based topic may be localized.  We need to strip the locale out of the filename
            {
                String Expression = ".[A-Za-z]{2}-[A-Za-z]{2}"; //match anything that looks like a valid locale, eg. ".en-US"
                TopicNameWithoutLocale = System.Text.RegularExpressions.Regex.Replace(TopicNameWithoutLocale, Expression, ""); //and nuke it. Non-alpha-numeric characters are not supported in file based topics
            }

            String URL = String.Empty;
            if (!AppLogic.IsAdminSite)
            {
                RouteValueDictionary dictionary = new RouteValueDictionary();
                dictionary.Add("SEName", TopicNameWithoutLocale);

                VirtualPathData pathInfo = RouteTable.Routes.GetVirtualPath(null, "Topic", dictionary);

                if (pathInfo != null)
                {
                    URL = pathInfo.VirtualPath;
                }
                else
                {
                    URL = String.Format("t-{0}.aspx", TopicNameWithoutLocale.Replace(" ", "-"));
                }
            }
            else
            {
                URL = "driver.aspx?topic=" + TopicNameWithoutLocale;
            }
            return URL.ToLowerInvariant();
        }

        static public String MakeDriver2Link(String TopicName)
        {
            String URL = String.Empty;
            if (!AppLogic.IsAdminSite)
            {
                RouteValueDictionary dictionary = new RouteValueDictionary();
                dictionary.Add("Topic", TopicName);

                VirtualPathData pathInfo = RouteTable.Routes.GetVirtualPath(null, "Topic", dictionary);

                if (pathInfo != null)
                {
                    URL = pathInfo.VirtualPath;
                }
                else
                {
                    URL = String.Format("t2-{0}.aspx", TopicName);
                }
            }
            else
            {
                URL = "driver2.aspx?topic=" + TopicName;
            }
            return URL.ToLowerInvariant();
        }

        static public String MakeDriverLink(int TopicID)
        {
            String URL = String.Empty;
            if (!AppLogic.IsAdminSite)
            {
                URL = String.Format("t-{0}.aspx", TopicID);
            }
            else
            {
                URL = "driver.aspx?topic=" + TopicID;
            }
            return URL.ToLowerInvariant();
        }

        static public String MakeDriver2Link(int TopicID)
        {
            String URL = String.Empty;
            if (!AppLogic.IsAdminSite)
            {
                URL = String.Format("t2-{0}.aspx", TopicID);
            }
            else
            {
                URL = "driver2.aspx?topic=" + TopicID;
            }
            return URL.ToLowerInvariant();
        }

        public static String GetEntitySEName(String EntityName, int EntityID)
		{
			String uname = String.Empty;
			if(EntityID != 0)
			{
				if(EntityName == "Product" || EntityName == "ProductVariant")
				{
                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rs = DB.GetRS("Select Name,SEName from [" + EntityName.Replace("]", "") + "]  with (NOLOCK)  where " + EntityName + "ID=" + EntityID.ToString(), con))
                        {
                            if(rs.Read())
					        {
						        uname = DB.RSField(rs,"SEName");
						        if (uname.Length == 0)
						        {
							        uname = DB.RSFieldByLocale(rs,"Name",Localization.GetUSLocale()); // SENames are ALWAYS from U.S locale
							        //update the SEName Field since it's empty
                                    DB.ExecuteSQL(String.Format("update [" + EntityName.Replace("]", "") + "] set SEName={0} where " + EntityName + "ID={1}", DB.SQuote(CommonLogic.Left(SE.MungeName(uname), 90)), EntityID));
						        }
					        }
                        }
                    }
				}
				else
				{
                    using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                    {
                        con.Open();
                        using (IDataReader rs = DB.GetRS("select SEName,Name from [" + EntityName.Replace("]", "") + "]  with (NOLOCK)  where " + EntityName + "ID=" + EntityID.ToString(), con))
                        {
                            if (rs.Read())
                            {
                                uname = DB.RSField(rs, "SEName");
                                if (uname.Length == 0)
                                {
                                    uname = DB.RSFieldByLocale(rs, "Name", Localization.GetUSLocale());
                                    //update the SEName Field since it's empty
                                    DB.ExecuteSQL(String.Format("update [" + EntityName.Replace("]", "") + "] set SEName={0} where " + EntityName + "ID={1}", DB.SQuote(CommonLogic.Left(SE.MungeName(uname), 90)), EntityID));
                                }
                            }
                        }
                    }
				}
			}
			return CommonLogic.Left(SE.MungeName(uname),90);
		}

		public static String GetCategorySEName(int CategoryID)
		{
			return GetEntitySEName("Category",CategoryID);
		}

		public static String GetManufacturerSEName(int ManufacturerID)
		{
			return GetEntitySEName("Manufacturer",ManufacturerID);
		}

		public static String GetSectionSEName(int SectionID)
		{
			return GetEntitySEName("Section",SectionID);
		}

		public static String GetObjectSEName(String ObjectName, int ObjectID)
		{
			return GetEntitySEName(ObjectName,ObjectID); // Object table may not technically an entity, but this works fine for this usage
		}

		public static String GetProductSEName(int ProductID)
		{
			return GetEntitySEName("Product",ProductID); // Product table is not technically an entity, but this works fine for this usage
		}

		public static String GetVariantSEName(int VariantID)
		{
			return GetEntitySEName("ProductVariant",VariantID); // ProductVariant table is not technically an entity, but this works fine for this usage
		}

		static public String MungeName(String s)
		{
			if(s.Length == 0)
			{
				return s;
			}
			String OKChars = "abcdefghijklmnopqrstuvwxyz1234567890 _-";
			s = s.Trim().ToLowerInvariant();
			StringBuilder tmpS = new StringBuilder(s.Length);
			for(int i=0; i<s.Length; i++) 
			{
				if(OKChars.IndexOf(s[i]) != -1)
				{
					tmpS.Append(s[i]);
				}
			}
			String s2 = tmpS.ToString();

            //trim leading dashes
            s2 = s2.TrimStart('-');

            // now fix up any repeated stuff:
			s2 = s2.Replace(" ","-");
            while (s2.IndexOf("--") != -1)
            {
                s2 = s2.Replace("--", "-");
            }
            while (s2.IndexOf("__") != -1)
            {
                s2 = s2.Replace("__", "_");
            }

            //trim trailing dashes
            s2 = s2.TrimEnd('-');
            return HttpContext.Current.Server.UrlEncode(s2);
		}


	}
}
