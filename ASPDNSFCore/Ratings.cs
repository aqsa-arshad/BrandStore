// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
using System.Web;
using System.Configuration;
using System.Web.SessionState;
using System.Web.Caching;
using System.Threading;
using System.Collections.Generic;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for Ratings.
    /// </summary>
    public class Ratings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ratings"/> class.
        /// </summary>
        public Ratings() { }

        /// <summary>
        /// Gets the product rating.
        /// </summary>
        /// <param name="CustomerID">The CustomerID.</param>
        /// <param name="ProductID">The ProductID.</param>
        /// <returns>Returns the product rating.</returns>
        static public int GetProductRating(int CustomerID, int ProductID)
        {
            int uname = 0;
            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS("Select rating from Rating   with (NOLOCK)  where CustomerID=" + CustomerID.ToString() + " and ProductID=" + ProductID.ToString(), dbconn))
                {
                    if (rs.Read())
                    {
                        uname = DB.RSFieldInt(rs, "rating");
                    }
                }
            }
            return uname;
        }

        /// <summary>
        /// Determine if the string has bad words.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>Returns TRUE if the string has bad words otherwise FALSE.</returns>
        static public bool StringHasBadWords(String s)
        {
            if (s.Length == 0)
            {
                return false;
            }
            String sql = "aspdnsf_CheckFilthy " + DB.SQuote(s) + "," + DB.SQuote(Thread.CurrentThread.CurrentUICulture.Name);

            bool hasBad = false;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    rs.Read();
                    int IsFilthy = DB.RSFieldInt(rs, "IsFilthy");

                    if (IsFilthy == 1)
                        hasBad = true;

                }

            }

            return hasBad;
        }

        /// <summary>
        /// Deletes the rating.
        /// </summary>
        /// <param name="RatingID">The RatingID.</param>
        static public void DeleteRating(int RatingID)
        {
            if (RatingID != 0)
            {
                int CustID = 0;
                int ProdID = 0;

                using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
                {
                    dbconn.Open();
                    using (IDataReader rs = DB.GetRS("select * from Rating   with (NOLOCK)  where RatingID=" + RatingID.ToString(), dbconn))
                    {
                        if (rs.Read())
                        {
                            CustID = DB.RSFieldInt(rs, "CustomerID");
                            ProdID = DB.RSFieldInt(rs, "ProductID");
                        }
                    }
                }

                DB.ExecuteSQL("delete from Rating where RatingID=" + CommonLogic.QueryStringUSInt("DeleteID").ToString());
                if (CustID != 0 && ProdID != 0)
                {
                    DB.ExecuteSQL(string.Format("delete from RatingCommentHelpfulness where RatingCustomerID={0} and ProductID={1} and StoreID={2}", CustID, ProdID, AppLogic.StoreID()));
                }
            }
        }

        /// <summary>
        /// overload method the sets the enclosedtab paramter value to true and displays the product rating
        /// </summary>
        /// <param name="ThisCustomer">Customer object</param>
        /// <param name="ProductID">Product ID of the product rating to display</param>
        /// <param name="CategoryID">Category ID of the product rating to display</param>
        /// <param name="SectionID">Section ID of the product rating to display</param>
        /// <param name="ManufacturerID">Manufacturer ID of the product rating to display</param>
        /// <param name="SkinID">skin id of the page</param>
        /// <returns>returns string html to be rendered</returns>
        static public String Display(Customer ThisCustomer, int ProductID, int CategoryID, int SectionID, int ManufacturerID, int SkinID)
        {
            return Display(ThisCustomer, ProductID, CategoryID, SectionID, ManufacturerID, SkinID, true);
        }

        /// <summary>
        /// Displays the product rating
        /// </summary>
        /// <param name="ThisCustomer">Customer object</param>
        /// <param name="ProductID">Product ID of the product rating to display</param>
        /// <param name="CategoryID">Category ID of the product rating to display</param>
        /// <param name="SectionID">Section ID of the product rating to display</param>
        /// <param name="ManufacturerID">Manufacturer ID of the product rating to display</param>
        /// <param name="SkinID">skin id of the page</param>
        /// <param name="encloseInTab">set to true if not to be displayed in a tabUI</param>
        /// <returns>returns string html to be rendered</returns>
        static public String Display(Customer ThisCustomer, int ProductID, int CategoryID, int SectionID, int ManufacturerID, int SkinID, bool encloseInTab)
        {
            string productName = AppLogic.GetProductName(ProductID, ThisCustomer.LocaleSetting);
            StringBuilder tmpS = new StringBuilder(50000);

            if (!AppLogic.IsAdminSite)
            {
                tmpS.Append("<input type=\"hidden\" name=\"ProductID\" value=\"" + ProductID.ToString() + "\">");
                tmpS.Append("<input type=\"hidden\" name=\"CategoryID\" value=\"" + CategoryID.ToString() + "\">");
                tmpS.Append("<input type=\"hidden\" name=\"SectionID\" value=\"" + SectionID.ToString() + "\">");
                tmpS.Append("<input type=\"hidden\" name=\"ManufacturerID\" value=\"" + ManufacturerID.ToString() + "\">");
                if (!encloseInTab)
                {
                    tmpS.Append("<input type=\"hidden\" name=\"productTabs\" value=\"2\">");
                }
            }

            if (encloseInTab)
            {
				tmpS.Append("<div class=\"group-header rating-header\">" + AppLogic.GetString("Header.ProductRatings", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</div>");
            }

            // RATINGS BODY:
            string sql = string.Format("aspdnsf_ProductStats {0}, {1}", ProductID, AppLogic.StoreID());
            int ratingsCount = 0;
            decimal ratingsAverage = 0;

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(sql, dbconn))
                {
                    rs.Read();
                    ratingsCount = DB.RSFieldInt(rs, "NumRatings");
                    int SumRatings = DB.RSFieldInt(rs, "SumRatings");
                    ratingsAverage = DB.RSFieldDecimal(rs, "AvgRating");
                }
            }

            int[] ratingPercentages = new int[6]; // indexes 0-5, but we only use indexes 1-5

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                string query = string.Format("select Productid, rating, count(rating) as N from Rating with (NOLOCK) where Productid = {0} and StoreID = {1} group by Productid,rating order by rating", ProductID, AppLogic.StoreID());
                dbconn.Open();
                using (IDataReader rs = DB.GetRS(query, dbconn))
                {
                    while (rs.Read())
                    {
                        int NN = DB.RSFieldInt(rs, "N");
                        Decimal pp = ((Decimal)NN) / ratingsCount;
                        int pper = (int)(pp * 100.0M);
                        ratingPercentages[DB.RSFieldInt(rs, "Rating")] = pper;
                    }
                }

            }

            string sortDescription = AppLogic.GetString("ratings.cs.1", SkinID, Thread.CurrentThread.CurrentUICulture.Name);
            string filterDescription = string.Empty;
            string fieldSuffix = string.Empty;

            int orderIndex = 0;
            if ("OrderBy".Equals(CommonLogic.FormCanBeDangerousContent("__EVENTTARGET"), StringComparison.InvariantCultureIgnoreCase))
            {
                orderIndex = CommonLogic.FormNativeInt("OrderBy");
            }
            if (orderIndex == 0)
            {
                orderIndex = 3;
            }

            switch (orderIndex)
            {
                case 1:
                    sortDescription = AppLogic.GetString("ratings.cs.1", SkinID, Thread.CurrentThread.CurrentUICulture.Name);
                    break;
                case 2:
                    sortDescription = AppLogic.GetString("ratings.cs.2", SkinID, Thread.CurrentThread.CurrentUICulture.Name);
                    break;
                case 3:
                    sortDescription = AppLogic.GetString("ratings.cs.3", SkinID, Thread.CurrentThread.CurrentUICulture.Name);
                    break;
                case 4:
                    sortDescription = AppLogic.GetString("ratings.cs.4", SkinID, Thread.CurrentThread.CurrentUICulture.Name);
                    break;
                case 5:
                    sortDescription = AppLogic.GetString("ratings.cs.5", SkinID, Thread.CurrentThread.CurrentUICulture.Name);
                    break;
                case 6:
                    sortDescription = AppLogic.GetString("ratings.cs.6", SkinID, Thread.CurrentThread.CurrentUICulture.Name);
                    break;
            }

            int pageSize = AppLogic.AppConfigUSInt("RatingsPageSize");
            int pageNumber = CommonLogic.QueryStringUSInt("PageNum");
            if (pageNumber == 0)
            {
                pageNumber = 1;
            }
            if (pageSize == 0)
            {
                pageSize = 10;
            }
            if (CommonLogic.QueryStringCanBeDangerousContent("show") == "all")
            {
                pageSize = 1000000;
                pageNumber = 1;
            }

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = DB.GetDBConn();
            conn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "aspdnsf_GetProductComments";
            cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@votingcustomer", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@pagesize", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@pagenum", SqlDbType.Int));
            cmd.Parameters.Add(new SqlParameter("@sort", SqlDbType.TinyInt));
            cmd.Parameters.Add(new SqlParameter("@storeID", SqlDbType.Int));

            cmd.Parameters["@ProductID"].Value = ProductID;
            cmd.Parameters["@votingcustomer"].Value = ThisCustomer.CustomerID;
            cmd.Parameters["@pagesize"].Value = pageSize;
            cmd.Parameters["@pagenum"].Value = pageNumber;
            cmd.Parameters["@sort"].Value = orderIndex;
            cmd.Parameters["@storeID"].Value = AppLogic.StoreID();

            SqlDataReader dr = cmd.ExecuteReader();
            dr.Read();

            int rowsCount = Convert.ToInt32(dr["totalcomments"]);
            int pagesCount = Convert.ToInt32(dr["pages"]);
            dr.NextResult();

            if (pageNumber > pagesCount && pageNumber > 1 && rowsCount == 0)
            {
                dr.Close();
                HttpContext.Current.Response.Redirect("showProduct.aspx?ProductID=" + ProductID.ToString() + "&pagenum=" + (pageNumber - 1).ToString());
            }

            int StartRow = (pageSize * (pageNumber - 1)) + 1;
            int StopRow = CommonLogic.IIF((StartRow + pageSize - 1) > rowsCount, rowsCount, StartRow + pageSize - 1);

			if(ratingsCount > 0) {
                tmpS.AppendFormat("<span itemprop=\"aggregateRating\" itemscope itemtype=\"{0}://schema.org/AggregateRating\">{1}", HttpContext.Current.Request.Url.Scheme, Environment.NewLine);
                tmpS.AppendFormat("<meta itemprop=\"ratingValue\" content=\"{0}\"/>{1}", ratingsAverage, Environment.NewLine);
                tmpS.AppendFormat("<meta itemprop=\"reviewCount\" content=\"{0}\"/>{1}", ratingsCount, Environment.NewLine);
                tmpS.AppendFormat("<meta itemprop=\"bestRating\" content=\"5\"/>{0}", Environment.NewLine);
                tmpS.AppendFormat("<meta itemprop=\"worstRating\" content=\"1\"/>{0}", Environment.NewLine);
                tmpS.AppendFormat("</span>{0}", Environment.NewLine);
			}

            tmpS.Append("<div class=\"page-row total-rating-row\">");
            tmpS.Append("   <div class=\"rating-stars-wrap\">");
            tmpS.Append(CommonLogic.BuildStarsImage(ratingsAverage, SkinID) + "<span class=\"ratings-average-wrap\">(" + String.Format("{0:f}", ratingsAverage) + ")</span>");
            tmpS.Append("   </div>");
            tmpS.Append("   <div class=\"rating-count-wrap\">");
            tmpS.Append("       <span>" + AppLogic.GetString("ratings.cs.23", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span> " + ratingsCount.ToString());
            tmpS.Append("   </div>");
            tmpS.Append("</div>");

            string rateScript = "javascript:RateIt(" + ProductID.ToString() + ");";

            int productRating = Ratings.GetProductRating(ThisCustomer.CustomerID, ProductID);

            tmpS.Append("<div class=\"page-row rating-link-row\">");
            if (productRating != 0)
            {
                tmpS.Append("<div class=\"rating-link-wrap\">");
                tmpS.Append("   <span>" + AppLogic.GetString("ratings.cs.24", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " " + productRating.ToString() + "</span>");
                tmpS.Append("</div>");
                if (!AppLogic.IsAdminSite)
                {
                    tmpS.Append("<div class=\"rating-link-wrap\">");
                    tmpS.Append("   <a href=\"" + rateScript + "\">" + AppLogic.GetString("ratings.cs.25", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a> ");
                    tmpS.Append("	<span>" + AppLogic.GetString("ratings.cs.26", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span>");
                    tmpS.Append("</div>");
                }
            }
            else
            {
                if ((AppLogic.AppConfigBool("RatingsCanBeDoneByAnons") || ThisCustomer.IsRegistered) && !AppLogic.IsAdminSite)
                {
                    tmpS.Append("<div class=\"rating-link-wrap\">");
                    tmpS.Append("   <a href=\"" + rateScript + "\">" + AppLogic.GetString("image.altText.10", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a>");
                    tmpS.Append("</div>");
                    tmpS.Append("<div class=\"rating-link-wrap\">");
                    tmpS.Append("   <a href=\"" + rateScript + "\">" + AppLogic.GetString("ratings.cs.28", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a> ");
					tmpS.Append("	<span>" + AppLogic.GetString("ratings.cs.27", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span>");
                    tmpS.Append("</div>");
                }
                else
                {
                    tmpS.Append("<div class=\"rating-link-wrap\">");
                    tmpS.Append("   <span>" + AppLogic.GetString("ratings.cs.29", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span>");
                    tmpS.Append("</div>");
                }
            }
            tmpS.Append("</div>");

            if (rowsCount == 0)
            {
                tmpS.Append(AppLogic.GetString("ratings.cs.39", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
                if (AppLogic.AppConfigBool("RatingsCanBeDoneByAnons") || ThisCustomer.IsRegistered && !AppLogic.IsAdminSite)
                {
                    tmpS.Append(" <a href=\"" + rateScript + "\">" + AppLogic.GetString("ratings.cs.40", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a> " + AppLogic.GetString("ratings.cs.41", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a>");
                }
            }
            else
            {
                while (dr.Read())
                {
                    tmpS.AppendFormat("<div class=\"page-row rating-comment-row\" itemprop=\"review\" itemscope itemtype=\"{0}://schema.org/Review\">{1}", HttpContext.Current.Request.Url.Scheme, Environment.NewLine);
					tmpS.AppendFormat("<meta itemprop=\"datePublished\" content=\"{0}\"/>{1}", Convert.ToDateTime(dr["CreatedOn"]).ToString("yyyy-MM-dd"), Environment.NewLine);
					tmpS.AppendFormat("<meta itemprop=\"itemReviewed\" content=\"{0}\"/>{1}", productName, Environment.NewLine);                   
					tmpS.Append("	<div class=\"rating-author-wrap\">\n");
                    tmpS.Append("		<span class=\"rating-row-number\">" + dr["rownum"].ToString() + ". </span><span class=\"rating-row-author\" itemprop=\"author\">" + HttpContext.Current.Server.HtmlEncode(CommonLogic.IIF(dr["FirstName"].ToString().Length == 0, AppLogic.GetString("ratings.cs.14", SkinID, Thread.CurrentThread.CurrentUICulture.Name), dr["FirstName"].ToString())) + "</span> <span class=\"rating-row-said\">" + AppLogic.GetString("ratings.cs.15", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " " + Localization.ToThreadCultureShortDateString(Convert.ToDateTime(dr["CreatedOn"])) + ", " + AppLogic.GetString("ratings.cs.16", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " </span>");
                    tmpS.Append("	</div>");
                    tmpS.AppendFormat("<div class=\"rating-comment-stars\" itemprop=\"reviewRating\" itemscope itemtype=\"{0}://schema.org/Rating\">{1}", HttpContext.Current.Request.Url.Scheme, Environment.NewLine);
                    tmpS.AppendFormat("<meta itemprop=\"bestRating\" content=\"5\"/>{0}", Environment.NewLine);
                    tmpS.AppendFormat("<meta itemprop=\"worstRating\" content=\"1\"/>{0}", Environment.NewLine);
					tmpS.AppendFormat("<meta itemprop=\"ratingValue\" content=\"{0}\"/>{1}", Convert.ToDecimal(dr["Rating"]), Environment.NewLine);
					tmpS.Append(CommonLogic.BuildStarsImage(Convert.ToDecimal(dr["Rating"]), SkinID));
					tmpS.Append("	</div>");
                    tmpS.Append("	<div class=\"rating-comments\" itemprop=\"reviewBody\">\n");
                    tmpS.Append(HttpContext.Current.Server.HtmlEncode(dr["Comments"].ToString()));
					tmpS.Append("	</div>\n");
					tmpS.Append("</div>\n");
                    tmpS.Append("<div class=\"form rating-comment-helpfulness-wrap\">");
					tmpS.Append("	<div class=\"form-group\">");
                    if (ThisCustomer.CustomerID != Convert.ToInt32(dr["CustomerID"]))
                    {
                        if (!AppLogic.IsAdminSite)
                        {
                            tmpS.Append(AppLogic.GetString("ratings.cs.42", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
                            tmpS.Append("<input TYPE=\"RADIO\" NAME=\"helpful_" + ProductID.ToString() + "_" + dr["CustomerID"].ToString() + "\" onClick=\"return RateComment('" + ProductID.ToString() + "','" + ThisCustomer.CustomerID + "','Yes','" + dr["CustomerID"].ToString() + "');\" " + CommonLogic.IIF(Convert.ToInt16(dr["CommentHelpFul"]) == 1, " checked ", "") + ">\n");
                            tmpS.Append("<span>" + AppLogic.GetString("ratings.cs.43", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span> \n");
							tmpS.Append("<input TYPE=\"RADIO\" NAME=\"helpful_" + ProductID.ToString() + "_" + dr["CustomerID"].ToString() + "\" onClick=\"return RateComment('" + ProductID.ToString() + "','" + ThisCustomer.CustomerID + "','No','" + dr["CustomerID"].ToString() + "');\" " + CommonLogic.IIF(Convert.ToInt16(dr["CommentHelpFul"]) == 0, " checked ", "") + ">\n");
							tmpS.Append("<span>" + AppLogic.GetString("ratings.cs.44", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span> \n");
                        }
                        else
                        {
                            tmpS.Append(AppLogic.GetString("ratings.cs.42", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
							tmpS.Append("<input TYPE=\"RADIO\" NAME=\"helpful_" + ProductID.ToString() + "_" + dr["CustomerID"].ToString() + "\" " + CommonLogic.IIF(Convert.ToInt16(dr["CommentHelpFul"]) == 1, " checked ", "") + ">\n");
							tmpS.Append("<span>" + AppLogic.GetString("ratings.cs.43", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span>\n");
							tmpS.Append("<input TYPE=\"RADIO\" NAME=\"helpful_" + ProductID.ToString() + "_" + dr["CustomerID"].ToString() + "\" " + CommonLogic.IIF(Convert.ToInt16(dr["CommentHelpFul"]) == 0, " checked ", "") + ">\n");
							tmpS.Append("<span>" + AppLogic.GetString("ratings.cs.44", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</span>\n");
                        }
                    }
					tmpS.Append("	</div>\n");
					tmpS.Append("	<div class=\"form-text rating-helpfulness-text\">");
                    tmpS.Append("			(" + dr["FoundHelpful"].ToString() + " " + AppLogic.GetString("ratings.cs.17", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " " + CommonLogic.IIF(ThisCustomer.CustomerID != Convert.ToInt32(dr["CustomerID"]), AppLogic.GetString("ratings.cs.18", SkinID, Thread.CurrentThread.CurrentUICulture.Name), AppLogic.GetString("ratings.cs.19", SkinID, Thread.CurrentThread.CurrentUICulture.Name)) + " " + AppLogic.GetString("ratings.cs.20", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + ", " + dr["FoundNotHelpful"].ToString() + " " + AppLogic.GetString("ratings.cs.21", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + ")");
					tmpS.Append("	</div>\n");
					tmpS.Append("</div>\n");
                }
            }
            dr.Close();

            if (rowsCount > 0)
            {
				tmpS.Append("<div class=\"page-row comments-count-wrap\">");
                tmpS.Append(String.Format(AppLogic.GetString("ratings.cs.37", SkinID, Thread.CurrentThread.CurrentUICulture.Name), StartRow.ToString(), StopRow.ToString(), rowsCount.ToString()));
                if (pagesCount > 1)
                {
                    tmpS.Append(" (");
                    if (pageNumber > 1)
                    {
                        tmpS.Append("<a href=\"showProduct.aspx?ProductID=" + CommonLogic.QueryStringUSInt("ProductID").ToString() + "&OrderBy=" + orderIndex.ToString() + "&pagenum=" + (pageNumber - 1).ToString() + "\">" + AppLogic.GetString("ratings.cs.10", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " " + pageSize.ToString() + "</a>");
                    }
                    if (pageNumber > 1 && pageNumber < pagesCount)
                    {
                        tmpS.Append(" | ");
                    }
                    if (pageNumber < pagesCount)
                    {
                        tmpS.Append("<a href=\"showProduct.aspx?ProductID=" + CommonLogic.QueryStringUSInt("ProductID").ToString() + "&OrderBy=" + orderIndex.ToString() + "&pagenum=" + (pageNumber + 1).ToString() + "\">" + AppLogic.GetString("ratings.cs.11", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " " + pageSize.ToString() + "</a>");
                    }
                    tmpS.Append(")");
                }
				tmpS.Append("</div>\n");
				tmpS.Append("<div class=\"page-row comments-pager-wrap\">");
                if (pagesCount > 1)
                {
                    tmpS.Append("<a href=\"showProduct.aspx?ProductID=" + CommonLogic.QueryStringUSInt("ProductID").ToString() + "&show=all\">" + AppLogic.GetString("ratings.cs.28", SkinID, Thread.CurrentThread.CurrentUICulture.Name) + "</a> " + AppLogic.GetString("ratings.cs.38", SkinID, Thread.CurrentThread.CurrentUICulture.Name));
                }
				tmpS.Append("</div>\n");
            }

            // END RATINGS BODY:

            if (!AppLogic.IsAdminSite)
            {
                tmpS.Append("<div id=\"RateCommentDiv\" name=\"RateCommentDiv\" style=\"position:absolute; left:0px; top:0px; visibility:" + AppLogic.AppConfig("RatingsCommentFrameVisibility") + "; z-index:2000; \">\n");
                tmpS.Append("<iframe name=\"RateCommentFrm\" id=\"RateCommentFrm\" width=\"400\" height=\"100\" hspace=\"0\" vspace=\"0\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" noresize scrolling=\"yes\" src=\"" + AppLogic.LocateImageURL("empty.htm") + "\"></iframe>\n");
                tmpS.Append("</div>\n");
                tmpS.Append("<script type=\"text/javascript\">\n");
                tmpS.Append("function RateComment(ProductID,MyCustomerID,MyVote,RatersCustomerID)\n");
                tmpS.Append("	{\n");
                tmpS.Append("	RateCommentFrm.location = 'RateComment.aspx?Productid=' + ProductID + '&VotingCustomerID=' + MyCustomerID + '&MyVote=' + MyVote + '&CustomerID=' + RatersCustomerID\n");
                tmpS.Append("	}\n");
                tmpS.Append("</script>\n");

                tmpS.Append("<script type=\"text/javascript\">\n");
                tmpS.Append("	function RateIt(ProductID)\n");
                tmpS.Append("	{\n");
                tmpS.Append("		window.open('" + AppLogic.ResolveUrl("~/rateit.aspx") + "?Productid=' + ProductID + '&refresh=no&returnurl=" + HttpContext.Current.Server.UrlEncode(CommonLogic.PageInvocation()) + "','ASPDNSF_ML" + CommonLogic.GetRandomNumber(1, 100000).ToString() + "','height=550,width=400,top=10,left=20,status=no,toolbar=no,menubar=no,scrollbars=yes,location=no')\n");
                tmpS.Append("	}\n");
                tmpS.Append("</SCRIPT>\n");
            }

            return tmpS.ToString();
        }
    }
}
