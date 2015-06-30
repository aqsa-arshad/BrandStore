// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for rateit.
	/// </summary>
	public partial class rateit : System.Web.UI.Page
	{
		int ProductID;
		String ReturnURL = String.Empty;
		int TheirCurrentRating = 0;
		String TheirCurrentComment = String.Empty;
		bool Editing = false;
		bool HasBadWords = false;
		Customer ThisCustomer;
		new int SkinID = 1;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			ThisCustomer.RequireCustomerRecord();

			if(ThisCustomer.SkinID > 0)
				SkinID = ThisCustomer.SkinID;

			ProductID = CommonLogic.QueryStringUSInt("ProductID");
			String ProductName = AppLogic.GetProductName(ProductID, ThisCustomer.LocaleSetting);
			String ReturnURL = CommonLogic.QueryStringCanBeDangerousContent("ReturnURL");
			AppLogic.CheckForScriptTag(ReturnURL);

			using(SqlConnection conn = DB.dbConn())
			{
				string query = string.Format("select * from Rating with (NOLOCK) where CustomerID = {0} and ProductID = {1} and StoreID = {2}", ThisCustomer.CustomerID, ProductID, AppLogic.StoreID());
				conn.Open();
				using(IDataReader rs = DB.GetRS(query, conn))
				{
					if(rs.Read())
					{
						TheirCurrentRating = DB.RSFieldInt(rs, "Rating");
						TheirCurrentComment = DB.RSField(rs, "Comments");
						Editing = true;
					}
				}
			}

			if(!IsPostBack)
			{
				InitializePageContent();
			}

		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnSubmit.Click += new EventHandler(btnSubmit_Click);
		}

		#endregion

		void btnSubmit_Click(object sender, EventArgs e)
		{
			StringBuilder sql = new StringBuilder(2500);
			String theCmts = CommonLogic.Left(Comments.Text, 5000);
			String theRating = rating.SelectedValue;

			ThisCustomer.ThisCustomerSession["LastCommentEntered"] = theCmts; // instead of passing via querystring due to length
			ThisCustomer.ThisCustomerSession["LastRatingEntered"] = theRating;

			HasBadWords = Ratings.StringHasBadWords(theCmts);

			if(!Editing)
			{
				sql.Append("insert into Rating(ProductID,IsFilthy,CustomerID,CreatedOn,Rating,HasComment,StoreID,Comments) values(");
				sql.Append(ProductID.ToString() + ",");
				sql.Append(CommonLogic.IIF(HasBadWords, "1", "0") + ",");
				sql.Append(ThisCustomer.CustomerID.ToString() + ",");
				sql.Append(DB.DateQuote(Localization.ToDBShortDateString(System.DateTime.Now)) + ",");
				sql.Append(theRating + ",");
				sql.Append(CommonLogic.IIF(theCmts.Length != 0, "1", "0") + ",");
				sql.Append(AppLogic.StoreID() + ",");
				if(theCmts.Length != 0)
				{

					sql.Append(DB.SQuote(theCmts));
				}
				else
				{
					sql.Append("NULL");
				}
				sql.Append(")");
				DB.ExecuteSQL(sql.ToString());
			}
			else
			{
				sql.Append("update Rating set ");
				sql.Append("IsFilthy=" + CommonLogic.IIF(HasBadWords, "1", "0") + ",");
				sql.Append("Rating=" + theRating + ",");
				sql.Append("CreatedOn=getdate(),");
				sql.Append("HasComment=" + CommonLogic.IIF(theCmts.Length != 0, "1", "0") + ",");
				if(theCmts.Length != 0)
				{

					sql.Append("Comments=" + DB.SQuote(theCmts));
				}
				else
				{
					sql.Append("Comments=NULL");
				}
				sql.Append(string.Format(" where ProductID = {0} and CustomerID = {1} and StoreID = {2}", ProductID.ToString(), ThisCustomer.CustomerID, AppLogic.StoreID()));
				DB.ExecuteSQL(sql.ToString());
			}

			TheirCurrentRating = Convert.ToInt32(theRating);
			TheirCurrentComment = theCmts;


			StringBuilder s = new StringBuilder("");

			s.Append("<script type=\"text/javascript\">\n");
			s.Append("opener.window.location.reload();");
			s.Append("self.close();");
			s.Append("</script>\n");
			ClientScript.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), s.ToString());

		}

		override protected void OnPreInit(EventArgs e)
		{

			ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			ThisCustomer.RequireCustomerRecord();

			if(ThisCustomer.SkinID > 0)
				SkinID = ThisCustomer.SkinID;

			string chosenTheme = "Skin_" + SkinID;

			if(CommonLogic.IsStringNullOrEmpty(this.Theme) ||
				false == this.Theme.Equals(chosenTheme, StringComparison.InvariantCultureIgnoreCase))
			{
				this.Theme = chosenTheme;
			}

			base.OnPreInit(e);
		}

		private void InitializePageContent()
		{
			rateit_aspx_4.Visible = HasBadWords;

			rateit_aspx_3.Text = AppLogic.GetString("rateit.aspx.3", 1, Localization.GetDefaultLocale());
			rateit_aspx_4.Text = AppLogic.GetString("rateit.aspx.4", 1, Localization.GetDefaultLocale());
			rateit_aspx_5.Text = AppLogic.GetString("rateit.aspx.5", 1, Localization.GetDefaultLocale());
			rateit_aspx_12.Text = AppLogic.GetString("rateit.aspx.12", 1, Localization.GetDefaultLocale());
			rateit_aspx_13.Text = AppLogic.GetString("rateit.aspx.13", 1, Localization.GetDefaultLocale());

			lblProductName.Text = AppLogic.GetProductName(ProductID, ThisCustomer.LocaleSetting);

			string img1 = AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/bigstar-whi.gif");
			string img2 = AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/bigstar-blu.gif");
			Star1.ImageUrl = CommonLogic.IIF(TheirCurrentRating >= 1, img2, img1);
			Star2.ImageUrl = CommonLogic.IIF(TheirCurrentRating >= 2, img2, img1);
			Star3.ImageUrl = CommonLogic.IIF(TheirCurrentRating >= 3, img2, img1);
			Star4.ImageUrl = CommonLogic.IIF(TheirCurrentRating >= 4, img2, img1);
			Star5.ImageUrl = CommonLogic.IIF(TheirCurrentRating >= 5, img2, img1);

			if(rating.Items.Count < 1)
			{
				rating.Items.Add(new ListItem(AppLogic.GetString("rateit.aspx.6", 1, Localization.GetDefaultLocale()), "0"));
				rating.Items.Add(new ListItem(AppLogic.GetString("rateit.aspx.7", 1, Localization.GetDefaultLocale()), "1"));
				rating.Items.Add(new ListItem(AppLogic.GetString("rateit.aspx.8", 1, Localization.GetDefaultLocale()), "2"));
				rating.Items.Add(new ListItem(AppLogic.GetString("rateit.aspx.9", 1, Localization.GetDefaultLocale()), "3"));
				rating.Items.Add(new ListItem(AppLogic.GetString("rateit.aspx.10", 1, Localization.GetDefaultLocale()), "4"));
				rating.Items.Add(new ListItem(AppLogic.GetString("rateit.aspx.11", 1, Localization.GetDefaultLocale()), "5"));
			}

			rating.SelectedValue = TheirCurrentRating.ToString();

			Comments.Text = TheirCurrentComment;

			btnSubmit.Text = AppLogic.GetString("rateit.aspx.14", 1, Localization.GetDefaultLocale());
			AppLogic.GetButtonDisable(btnSubmit);
			btnCancel.Text = AppLogic.GetString("rateit.aspx.15", 1, Localization.GetDefaultLocale());

			GetJSFunctions();
		}

		private void GetJSFunctions()
		{
			StringBuilder s = new StringBuilder("");
			s.Append("<script type=\"text/javascript\">\n");
			s.Append(" document.onreadystatechange=document_onreadystatechange;\n");
			s.Append("function FormValidator()\n");
			s.Append("	{\n");
			s.Append("	if (document.getElementById(\"rating\").selectedIndex < 1)\n");
			s.Append("	{\n");
			s.Append("		alert(\"" + AppLogic.GetString("rateit.aspx.1", 1, Localization.GetDefaultLocale()) + "\");\n");
			s.Append("		document.getElementById(\"rating\").focus();\n");
			s.Append("		return (false);\n");
			s.Append("    }\n");
			s.Append("	if (document.getElementById(\"Comments\").value.length > 5000)\n");
			s.Append("	{\n");
			s.Append("		alert(\"" + AppLogic.GetString("rateit.aspx.2", 1, Localization.GetDefaultLocale()) + "\");\n");
			s.Append("		document.getElementById(\"Comments\").focus();\n");
			s.Append("		return (false);\n");
			s.Append("    }\n");
			s.Append("	return (true);\n");
			s.Append("	}\n");
			s.Append("\n");
			s.Append("	var ImgArray = new Array(new Image(),new Image())\n");
			s.Append("	ImgArray[0].src = \"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/bigstar-blu.gif") + "\"\n");
			s.Append("	ImgArray[1].src = \"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/bigstar-whi.gif") + "\"\n");
			s.Append("	\n");
			s.Append("	function document_onreadystatechange()\n");
			s.Append("	{\n");
			s.Append("		newRatingEntered(document.getElementById(\"rating\").selectedIndex);\n");
			s.Append("	}\n");
			s.Append("	\n");
			s.Append("	function newRatingEntered(RV)\n");
			s.Append("	{\n");
			s.Append("		if (RV >= 1)\n");
			s.Append("			{document.getElementById(\"" + Star1.ClientID + "\").src = ImgArray[0].src}\n");
			s.Append("		else\n");
			s.Append("			{document.getElementById(\"" + Star1.ClientID + "\").src = ImgArray[1].src}\n");
			s.Append("		if (RV >= 2)\n");
			s.Append("			{document.getElementById(\"" + Star2.ClientID + "\").src = ImgArray[0].src}\n");
			s.Append("		else\n");
			s.Append("			{document.getElementById(\"" + Star2.ClientID + "\").src = \"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/bigstar-whi.gif") + "\"}\n");
			s.Append("		if (RV >= 3)\n");
			s.Append("			{document.getElementById(\"" + Star3.ClientID + "\").src = ImgArray[0].src}\n");
			s.Append("		else\n");
			s.Append("			{document.getElementById(\"" + Star3.ClientID + "\").src = \"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/bigstar-whi.gif") + "\"}\n");
			s.Append("		if (RV >= 4)\n");
			s.Append("			{document.getElementById(\"" + Star4.ClientID + "\").src = ImgArray[0].src}\n");
			s.Append("		else\n");
			s.Append("			{document.getElementById(\"" + Star4.ClientID + "\").src = \"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/bigstar-whi.gif") + "\"}\n");
			s.Append("		if (RV >= 5)\n");
			s.Append("			{document.getElementById(\"" + Star5.ClientID + "\").src = ImgArray[0].src}\n");
			s.Append("		else\n");
			s.Append("			{document.getElementById(\"" + Star5.ClientID + "\").src =\"" + AppLogic.LocateImageURL("App_Themes/skin_" + SkinID.ToString() + "/images/bigstar-whi.gif") + "\"}\n");
			s.Append("		document.getElementById(\"rating\").selectedIndex = RV;\n");
			s.Append("		return false;\n");
			s.Append("	}\n");
			s.Append("</script>\n");

			ClientScript.RegisterClientScriptBlock(this.GetType(), Guid.NewGuid().ToString(), s.ToString());

		}

	}
}
