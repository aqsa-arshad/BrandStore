// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using Telerik.Web.UI;
using System.Data.SqlClient;
using System.Data;

namespace AspDotNetStorefrontAdmin
{
	public partial class NewEntities : RadAjaxPage
	{
		public List<GridEntity> grdEntities;

		protected void Page_Load(object sender, EventArgs e)
		{
			Customer ThisCustomer = Customer.Current;

			int ProductID = CommonLogic.QueryStringUSInt("productid");

			if (ProductID > 0)
			{
				Response.Redirect("newproducts.aspx?productid=" + ProductID.ToString());
			}

			String EntityToLoad = "Category";
			int EntityID = 0;

			if (!IsPostBack)
			{
				if (CommonLogic.QueryStringUSInt("manufacturerid") > 0)
				{
					EntityID = CommonLogic.QueryStringUSInt("manufacturerid");
					EntityToLoad = "Manufacturer";
				}

				if (EntityID == 0)
				{
					if (CommonLogic.QueryStringUSInt("categoryid") > 0)
					{
						EntityID = CommonLogic.QueryStringUSInt("categoryid");
						EntityToLoad = "Category";
					}
				}

				if (EntityID == 0)
				{
					if (CommonLogic.QueryStringUSInt("sectionid") > 0)
					{
						EntityID = CommonLogic.QueryStringUSInt("sectionid");
						EntityToLoad = "Section";
					}
				}

				if (EntityID == 0)
				{
					if (CommonLogic.QueryStringUSInt("distributorid") > 0)
					{
						EntityID = CommonLogic.QueryStringUSInt("distributorid");
						EntityToLoad = "Distributor";
					}
				}
			}

			if (EntityID > 0)
			{
				List<GridEntity> tmplist = new List<GridEntity>();
				GridEntity parentEntity = new GridEntity(EntityID, EntityToLoad);
				tmplist.Add(parentEntity);

				while (parentEntity.ParentID != 0)
				{	
					parentEntity = new GridEntity(parentEntity.ParentID, EntityToLoad);
					tmplist.Add(parentEntity);
				}

				grdEntities = tmplist;
				grd.Datasource = grdEntities;
			}
			else
			{
				EntityToLoad = CommonLogic.QueryStringCanBeDangerousContent("entityname");

				grdEntities = FixedGetAllEntitiesOfType(EntityToLoad);
				grd.Datasource = grdEntities;
			}

			switch (EntityToLoad.ToLowerInvariant())
			{
				case "manufacturer":
					lblTitle.Text =
						this.Title = AppLogic.GetString("admin.menu.Manufacturers", ThisCustomer.LocaleSetting);
					break;
				case "distributor":
					lblTitle.Text =
						this.Title = AppLogic.GetString("admin.menu.Distributors", ThisCustomer.LocaleSetting);
					break;
				case "department":
				case "section":
					lblTitle.Text =
						this.Title = AppLogic.GetString("admin.menu.Sections", ThisCustomer.LocaleSetting);
					break;
				case "category":
				default:
					lblTitle.Text =
						this.Title = AppLogic.GetString("admin.menu.Categories", ThisCustomer.LocaleSetting);
					break;
			}
		}

		public List<GridEntity> FixedGetAllEntitiesOfType(String EntityToLoad)
		{
			String EntityType = Entity.LoadEntityType(EntityToLoad);

			List<GridEntity> eiList = new List<GridEntity>();

			using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();

				using (IDataReader rs = DB.GetRS("select * from dbo.{0} with(NOLOCK)".FormatWith(EntityType), conn))
				{
					while (rs.Read())
					{
						GridEntity e = new GridEntity(rs, EntityType);
						eiList.Add(e);
					}
				}
			}

			return eiList;
		}
	}
}
