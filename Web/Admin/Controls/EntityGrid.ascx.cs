// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;
using System.Data;
using AjaxControlToolkit;
using Telerik.Web.UI;

#endregion

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EntityGrid : BaseUserControl<IEnumerable<GridEntity>>
    {
		#region Fields
		
		private const string CURRENT_PAGE = "CurrentPage";
		private const string SORT_COLUMN = "SortColumn";
		private const string SORT_COLUMN_DIRECTION = "SortColumnDirection";
		private const string CURRENT_FILTER = "CurrentFilter";
		private const string CURRENT_FILTER_TYPE = "CurrentFilterType";
		private const int PAGE_SIZE = 9999999;
		private string EntityType = "category";

		#endregion      

		#region Properties

		public string CurrentFilter
		{
			get
			{
				object savedValue = ViewState[CURRENT_FILTER];
				if (null == savedValue) { return string.Empty; }

				return savedValue.ToString();
			}
			set
			{
				ViewState[CURRENT_FILTER] = value;
			}
		}

		public FilterType CurrentFilterType
		{
			get
			{
				object intValue = ViewState[CURRENT_FILTER_TYPE];
				if (null == intValue) { return FilterType.Search; }

				return (FilterType)((int)intValue);
			}
			set
			{
				ViewState[CURRENT_FILTER_TYPE] = (int)value;
			}
		}

		public int CurrentPage
		{
			get { return null == ViewState[CURRENT_PAGE] ? 1 : (int)ViewState[CURRENT_PAGE]; }
			set { ViewState[CURRENT_PAGE] = value; }
		}

		public string SortColumn
		{
			get { return null == ViewState[SORT_COLUMN] ? string.Empty : (string)ViewState[SORT_COLUMN]; }
			set { ViewState[SORT_COLUMN] = value; }
		}

		public GridSortOrder SortColumnDirection
		{
			get { return null == ViewState[SORT_COLUMN_DIRECTION] ? GridSortOrder.Ascending : (GridSortOrder)ViewState[SORT_COLUMN_DIRECTION]; }
			set { ViewState[SORT_COLUMN_DIRECTION] = value; }
		}

		#endregion

		#region Event Handlers

		protected void Page_Load(object sender, EventArgs e)
		{
			EntityType = CommonLogic.QueryStringCanBeDangerousContent("entityname");

			if (!IsPostBack)
			{
				SortColumn = "LocaleName";
				CurrentPage = 1;
				CurrentFilter = string.Empty;
				CurrentFilterType = FilterType.None;
			}

			AddRootAddButton();

			//Build and register the RadWindow Script block
			string gridID = ctrlSearch.FindControl("grdEntities").ClientID;
			string ajxID = radAjaxMgr.ClientID;

			StringBuilder sb = new StringBuilder();

			#region RadWindow Javascript Functions
			sb.Append("<script type=\"text/javascript\">                                                                                        \r\n");
			/*Instantiates a new modale window with the Edit Entity page in edit mode*/
			sb.Append("function ShowEditForm(id)                                                                                                \r\n");
			sb.Append("{                                                                                                                        \r\n");
			sb.Append("     window.radopen(\"entityEdit.aspx?entityID=\" + id + \"&entityName=" + EntityType + "\", \"rdwEditEntity\");         \r\n");
			sb.Append("     return false;                                                                                                       \r\n");
			sb.Append("}                                                                                                                        \r\n");
			/*Instantiates a new modal window with the Edit Entity page in insert mode*/
			sb.Append("function ShowInsertForm(id)                                                                                              \r\n");
			sb.Append("{                                                                                                                        \r\n");
			sb.Append("     window.radopen(\"entityEdit.aspx?entityName=" + EntityType + "&entityParent=\" + id, \"rdwEditEntity\");            \r\n");
			sb.Append("     return false;                                                                                                       \r\n");
			sb.Append("}                                                                                                                        \r\n");
			/*Loads the Bulkd Display Order button from root entities*/
			/*Instantiates a new modal window with the Edit Entity page in insert mode*/
			sb.Append("function ShowDisplayOrderForm()                                                                                          \r\n");
			sb.Append("{                                                                                                                        \r\n");
			sb.Append("     window.radopen(\"entityBulkDisplayOrder.aspx?entityName=" + EntityType + "\", \"rdwEditEntity\");                     \r\n");
			sb.Append("     return false;                                                                                                       \r\n");
			sb.Append("}                                                                                                                        \r\n");
			/*Accepts a callback from the modal window to update the grid*/
			sb.Append("function refreshGrid(arg)                                                                                                \r\n");
			sb.Append("{                                                                                                                        \r\n");
			sb.Append("     if(!arg)                                                                                                            \r\n");
			sb.Append("     {                                                                                                                   \r\n");
			sb.Append("             $find(\"" + ajxID + "\").ajaxRequest(arg);                                                                  \r\n");
			sb.Append("     }                                                                                                                   \r\n");
			sb.Append("     else                                                                                                                \r\n");
			sb.Append("     {                                                                                                                   \r\n");
			sb.Append("             $find(\"" + ajxID + "\").ajaxRequest(arg);                                                                  \r\n");
			sb.Append("     }                                                                                                                   \r\n");
			sb.Append("}                                                                                                                        \r\n");

			sb.Append("</script>                                                                                                                \r\n");
			#endregion

			ClientScriptManager cs = Page.ClientScript;

			cs.RegisterClientScriptBlock(this.Page.GetType(), Guid.NewGuid().ToString(), sb.ToString());
		}

		/// <summary>
		/// Method to catch bubbled event from the entity control when a new entity is added
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Entity_Added(object sender, EventArgs e)
		{
			int eID = 0;

			if (CommonLogic.IsInteger(sender.ToString()))
			{
				eID = int.Parse(sender.ToString());
			}

			if (eID != 0)
			{
				GridEntity ge = new GridEntity(eID, EntityType);

				if (ge.ParentID == 0)
				{
					(base.Datasource as List<GridEntity>).Add(ge);
				}

				BindData(false);

				updatePanelSearch.Update();
			}
		}
		
		/// <summary>
		/// Method to catch bubbled event from the entity control when an entity is updated
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Entity_Updated(object sender, EventArgs e)
		{
			int eID = 0;

			if (CommonLogic.IsInteger(sender.ToString()))
			{
				eID = int.Parse(sender.ToString());
			}

			if (eID != 0)
			{
				int itemToUpdate = (base.Datasource as List<GridEntity>).FindIndex(ent => ent.ID.Equals(eID));

				(base.Datasource as List<GridEntity>)[itemToUpdate] = new GridEntity(eID, EntityType);

				BindData(false);

				updatePanelSearch.Update();
			}
		}

		protected void ctrlSearch_Filter(object sender, FilterEventArgs e)
        {
            CurrentFilter = e.Filter;
            CurrentFilterType = e.Type;
            CurrentPage = e.Page;

            AddRootAddButton();

            BindData(false);
        }

		/// <summary>
		/// Processes callbacks initiated from client script in the Edit Entity window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void radAjaxMgr_OnAjaxRequest(object sender, AjaxRequestEventArgs e)
		{
			//Parse the return arguments to get the entity ID
			int entityID = 0;

			try
			{
				entityID = int.Parse(e.Argument);
			}
			catch
			{
				//Nothing to do
				return;
			}

			if (entityID != 0)
			{
				GridEntity ent = new GridEntity(entityID, EntityType);

				try
				{
					//Updating an existing entity
					int entIndex = (base.Datasource as List<GridEntity>).FindIndex(enToUpdate => enToUpdate.ID.Equals(entityID));
					(base.Datasource as List<GridEntity>)[entIndex] = ent;
				}
				catch
				{
					//Entity does not exist, so add it here
					(base.Datasource as List<GridEntity>).Add(ent);
				}
			}

			BindData(false);

			//updatePanelSearch.Update();
		}

		protected void grdEntities_ItemCommand(object source, GridCommandEventArgs e)
		{
			String commandName = e.CommandName.ToLower();

			if (commandName != "expandcollapse" &&
				commandName != "publish" &&
				commandName != "unpublish" &&
				commandName != "deleteentity" &&
				commandName != "undeleteentity")
				return;

			if (commandName == "expandcollapse")
			{
				AddRootAddButton();
				return;
			}

			int eID = int.Parse(e.CommandArgument.ToString());
			GridEntity ent = new GridEntity(eID, EntityType);

			if (commandName == "publish")
			{
				ent.Publish();
				UpdatePublishedColumn(e.Item, true);
			}
			else if (commandName == "unpublish")
			{
				ent.UnPublish();
				UpdatePublishedColumn(e.Item, false);
			}
			else if (commandName == "deleteentity")
			{
				ent.Delete();
				UpdateDeletedColumn(e.Item, true);
			}
			else if (commandName == "undeleteentity")
			{
				ent.UnDelete();
				UpdateDeletedColumn(e.Item, false);
			}

			int entIndex = (base.Datasource as List<GridEntity>).FindIndex(enToUpdate => enToUpdate.ID.Equals(eID));
			(base.Datasource as List<GridEntity>)[entIndex] = ent;
		}

		protected void grdEntities_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
		{
			if (!e.IsFromDetailTable)
			{
				BindData(true);
			}
		}

		protected void grdEntities_DetailTableDataBind(object source, GridDetailTableDataBindEventArgs e)
		{
			RadGrid grdEntities = ctrlSearch.FindControl<RadGrid>("grdEntities");
			List<GridEntity> allEntities = (base.Datasource as List<GridEntity>).OrderBy(ent => ent.DisplayOrder).ThenBy(ent => ent.LocaleName).ToList();

			GridDataItem dataItem = (GridDataItem)e.DetailTableView.ParentItem;

			int parentID = int.Parse(dataItem.GetDataKeyValue("ID").ToString());

			IList<GridEntity> allChildEntities = GridEntity.GetChildrenEntities(EntityType, parentID);

			var chkShowDeleted = ctrlSearch.FindControl<CheckBox>("chkShowDeleted");
			if (chkShowDeleted != null && !chkShowDeleted.Checked)
				allChildEntities = allChildEntities.Where(ent => !ent.Deleted).ToList();

			var filterString = this.CurrentFilter;
			if (!String.IsNullOrEmpty(filterString))
				allChildEntities = allChildEntities.Where(item => IsFilterSearchMatch(CurrentFilterType, filterString, item, allEntities)).ToList();

			e.DetailTableView.DataSource = allChildEntities;
		}

		protected void grdEntities_ItemCreated(object sender, GridItemEventArgs e)
		{
			GridItem gridItem = e.Item;

			var chkShowDeleted = ctrlSearch.FindControl<CheckBox>("chkShowDeleted");
			bool showDeleted = chkShowDeleted != null && chkShowDeleted.Checked;

			if (gridItem.ItemType != GridItemType.Item && gridItem.ItemType != GridItemType.AlternatingItem)
				return;

			GridEntity gridEntity = gridItem.DataItem as GridEntity;

			if(gridEntity == null)
				return;

			UpdatePublishedColumn(gridItem, gridEntity.Published);
			UpdateDeletedColumn(gridItem, gridEntity.Deleted);

			// find the expand/collapse button
			Button ctrl = gridItem.FindControl<Button>("GECBtnExpandColumn");
			if(ctrl == null)
				return;

			// Hide the expand button if the entity has no children or only has deleted children and Show Deleted is not checked.
			// The ordering of operands is to make the SQL call at the end run only if everything else is false.
            bool hideExpandButton = !gridEntity.HasChildren || (!showDeleted && Entity.GetChildrenEntities(gridEntity.EntityType, gridEntity.ID).All(entity => entity.Deleted));

			if(hideExpandButton)
				ctrl.Visible = false;
		}

		protected void chkShowDeleted_CheckChanged(Object sender, EventArgs e)
		{
			BindData(false);
		}

		/// <summary>
		/// The eventhandler for databinding of the add button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnAdd_DataBinding(object sender, EventArgs e)
		{
			ImageButton imgbtnAdd = (ImageButton)sender;
			GridDataItem gdi = (GridDataItem)imgbtnAdd.NamingContainer;
			//imgbtnAdd.CommandArgument = (gdi.DataItem as GridEntity).ID.ToString();
			imgbtnAdd.Attributes["onclick"] = String.Format("ShowInsertForm('{0}')", (gdi.DataItem as GridEntity).ID.ToString());
		}

		/// <summary>
		/// The eventhandler for databinding of the edit button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnEdit_DataBinding(object sender, EventArgs e)
		{
			ImageButton imgbtnEdit = (ImageButton)sender;
			GridDataItem gdi = (GridDataItem)imgbtnEdit.NamingContainer;
			GridEntity ge = (GridEntity)gdi.DataItem;

			imgbtnEdit.Attributes["onclick"] = String.Format("ShowEditForm('{0}')", (gdi.DataItem as GridEntity).ID.ToString());
		}

		/// <summary>
		/// The eventhandler for databinding of the LinkButton for the Name column
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnName_DataBinding(object sender, EventArgs e)
		{
			LinkButton lbtnName = (LinkButton)sender;
			GridDataItem gdi = (GridDataItem)lbtnName.NamingContainer;
			lbtnName.Text = (gdi.DataItem as GridEntity).LocaleName;
			lbtnName.Attributes["onclick"] = String.Format("return ShowEditForm('{0}');", (gdi.DataItem as GridEntity).ID.ToString());
		}

		#endregion

		#region Protected Methods

		protected void AddRootAddButton()
		{
			AlphaPaging ap = ctrlSearch.FindControl<AlphaPaging>("ctrlAlphaPaging");

			if (ap.FindControl<ImageButton>("imgbtnAddRoot") == null)
			{
				ImageButton imgbtnAdd = new ImageButton();
				imgbtnAdd.ID = "imgbtnAddRoot";
				imgbtnAdd.ImageUrl = "~/App_Themes/Admin_Default/images/add.png";
				//imgbtnAdd.Click += new ImageClickEventHandler(this.imgbtnAdd_Click);
				imgbtnAdd.OnClientClick = "return ShowInsertForm();";
				imgbtnAdd.ToolTip = AppLogic.GetString("admin.common.Add", ThisCustomer.LocaleSetting) + " " + EntityType;

				ap.Controls.Add(imgbtnAdd);
			}
		}

		protected T DataItemAs<T>(GridItem item) where T : class
		{
			return item.DataItem as T;
		}

		#endregion

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			if (ThisCustomer == null)
				ThisCustomer = AppLogic.GetCurrentCustomer();

			base.OnInit(e);
		}

		public override void DataBind()
		{
			BindData(false);
			base.DataBind();
		}

		#endregion

		#region Private Methods

		private PaginatedList<GridEntity> GetDatasource()
		{
			List<GridEntity> allEntities = (base.Datasource as List<GridEntity>).OrderBy(ent => ent.DisplayOrder).ThenBy(ent => ent.LocaleName).ToList();

			var chkShowDeleted = ctrlSearch.FindControl<CheckBox>("chkShowDeleted");
			if (chkShowDeleted != null && !chkShowDeleted.Checked)
				allEntities = allEntities.Where(e => !e.Deleted).ToList();

			PagedSearchTemplate<GridEntity> pagedSearch = new PagedSearchTemplate<GridEntity>();

			var filterString = this.CurrentFilter;
			if (!String.IsNullOrEmpty(filterString))
				allEntities = allEntities.Where(item => IsFilterSearchMatch(CurrentFilterType, filterString, item, allEntities)).ToList();

			return pagedSearch.Search(allEntities, PAGE_SIZE, CurrentPage);
		}

		/// <summary>
		/// Determines the appropriate action and text for the published column of the entity RadGrid
		/// </summary>
		/// <param name="gi">The GridItem of the current row in the RadGrid</param>
		/// <param name="Published">A boolean value indicating the published status of the entity in the current row</param>
		private void UpdatePublishedColumn(GridItem gi, bool Published)
		{
			LinkButton lb = gi.FindControl<LinkButton>("cmdPublish");

			if (lb != null)
			{
				if (Published)
				{
					lb.Text = AppLogic.GetString("admin.entitygrid.unpublish", ThisCustomer.LocaleSetting);
					lb.CommandName = "UnPublish";
				}
				else
				{
					lb.Text = AppLogic.GetString("admin.entitygrid.publish", ThisCustomer.LocaleSetting);
					lb.CommandName = "Publish";
				}
			}
		}

		private void UpdateDeletedColumn(GridItem gi, bool Deleted)
		{
			ImageButton imgbtn = gi.FindControl<ImageButton>("imgDelete");

			if (Deleted)
			{
				imgbtn.ImageUrl = "~/App_Themes/Admin_Default/images/undelete.png";
				imgbtn.ToolTip = "UnDelete Entity";
				imgbtn.CommandName = "UnDeleteEntity";
				imgbtn.OnClientClick = "return confirm('Are you sure you want to undelete this entity?');";
			}
			else
			{
				imgbtn.ImageUrl = "~/App_Themes/Admin_Default/images/delete.png";
				imgbtn.ToolTip = "Delete Entity";
				imgbtn.CommandName = "DeleteEntity";
				imgbtn.OnClientClick = "return confirm('Are you sure you want to delete this entity?');";
			}
		}

		private void BindData(bool fromNeedDataSource)
		{
			PaginatedList<GridEntity> results = GetDatasource();

			ctrlSearch.AllCount = results.TotalCount;
			ctrlSearch.StartCount = results.StartIndex;
			ctrlSearch.EndCount = results.EndIndex;
			ctrlSearch.PageCount = results.TotalPages;
			ctrlSearch.CurrentPage = results.CurrentPage;

			RadGrid grdEntities = ctrlSearch.FindControl<RadGrid>("grdEntities");
			grdEntities.DataSource = results;

			if (!fromNeedDataSource)
			{
				grdEntities.DataBind();
			}
		}

		private Boolean IsFilterSearchMatch(FilterType filterType, String filter, GridEntity currentItem, IList<GridEntity> items)
		{
			if (currentItem == null)
				return false;

			if (CurrentFilterType.Equals(FilterType.Search))
				return currentItem.LocaleName.ContainsIgnoreCase(filter) || currentItem.ID == filter.ToNativeInt() || items.Where(child => child.ParentID == currentItem.ID).Any(child => IsFilterSearchMatch(filterType, filter, child, items));

			if (CurrentFilterType.Equals(FilterType.AlphaFilter))
			{
				if (filter == "[0-9]")
					return currentItem.LocaleName.StartsWithNumbers() || items.Where(child => child.ParentID == currentItem.ID).Any(child => IsFilterSearchMatch(filterType, filter, child, items));
				else
					return currentItem.LocaleName.StartsWithIgnoreCase(filter) || items.Where(child => child.ParentID == currentItem.ID).Any(child => IsFilterSearchMatch(filterType, filter, child, items));
			}

			return false;
		}

		#endregion
    }
}
