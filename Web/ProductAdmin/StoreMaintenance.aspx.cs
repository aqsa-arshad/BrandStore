// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using AspDotNetStorefrontCore;
using Telerik.Web.UI;
using AspDotNetStorefrontControls;

#endregion

namespace AspDotNetStorefrontAdmin
{
    public partial class StoreMaintaince : AdminPageBase
	{
		#region Properties

		private IEnumerable<IGrouping<Int32, MappedObject>> Datasource { get; set; }

		private Int32 CurrentPage
		{
			get
			{
				if (ViewState["CurrentPage"] == null)
					ViewState["CurrentPage"] = 1;

				return (Int32)ViewState["CurrentPage"];
			}
			set
			{
				ViewState["CurrentPage"] = value;
			}
		}

		#endregion

		#region Event Handlers

		protected override void OnInit(EventArgs e)
        {
            scMain.ThisCustomer = ThisCustomer;

			ctrlSearch.PageSizes = new List<int>(new int[] { 10, 15, 20, 25, 30, 40, 50 });
			if (!this.IsPostBack)
			{
				ctrlSearch.CurrentPageSize = 10;
			}

            base.OnInit(e);
        }

		protected void Page_Load(Object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			if (IsPostBack)
				return;

			if (Request.QueryString.Count > 0 && Request.QueryString[0] == "Domains")
				tabPanel.ActiveTabIndex = 1;
		}

        protected void ctrlSearch_ContentCreated(object sender, FilterEventArgs e)
        {
			PagingInfo pagingInfo = BindData(ddlEntities.SelectedValue, ctrlSearch.CurrentFilterType, ctrlSearch.CurrentFilter, ctrlSearch.CurrentPageSize, CurrentPage);
			ctrlSearch.AllCount = pagingInfo.TotalCount;
			ctrlSearch.StartCount = pagingInfo.StartIndex;
			ctrlSearch.EndCount = pagingInfo.EndIndex;
			ctrlSearch.PageCount = pagingInfo.TotalPages;
			ctrlSearch.CurrentPage =
				CurrentPage = pagingInfo.CurrentPage;
        }

        protected void ctrlSearch_Filter(object sender, FilterEventArgs e)
        {
            ctrlSearch.CurrentFilter = e.Filter;
            ctrlSearch.CurrentFilterType = e.Type;
            ctrlSearch.CurrentPage = e.Page;

            PagingInfo pagingInfo = BindData(ddlEntities.SelectedValue, ctrlSearch.CurrentFilterType, ctrlSearch.CurrentFilter, ctrlSearch.CurrentPageSize, ctrlSearch.CurrentPage);
            ctrlSearch.AllCount = pagingInfo.TotalCount;
            ctrlSearch.StartCount = pagingInfo.StartIndex;
            ctrlSearch.EndCount = pagingInfo.EndIndex;
            ctrlSearch.PageCount = pagingInfo.TotalPages;
			ctrlSearch.CurrentPage =
				CurrentPage = pagingInfo.CurrentPage;
        }

        protected void ddlEntities_SelectedIndexChanged(Object sender, EventArgs e)
        {
			pnlEntityMap.Visible = !String.IsNullOrEmpty(ddlEntities.SelectedValue);

			ctrlSearch.ResetFilters();
			
            if (String.IsNullOrEmpty(ddlEntities.SelectedValue))
                return;

            Title = "Store Mapping - " + ddlEntities.SelectedValue;
            
            PagingInfo pagingInfo = BindData(ddlEntities.SelectedValue, ctrlSearch.CurrentFilterType, ctrlSearch.CurrentFilter, ctrlSearch.CurrentPageSize, ctrlSearch.CurrentPage);
            ctrlSearch.AllCount = pagingInfo.TotalCount;
            ctrlSearch.StartCount = pagingInfo.StartIndex;
            ctrlSearch.EndCount = pagingInfo.EndIndex;
            ctrlSearch.PageCount = pagingInfo.TotalPages;
            ctrlSearch.CurrentPage = pagingInfo.CurrentPage;
        }

        protected void ctrlSearch_SearchInvoked(object sender, EventArgs e)
        {
            PagingInfo pagingInfo = BindData(ddlEntities.SelectedValue, ctrlSearch.CurrentFilterType, ctrlSearch.CurrentFilter, ctrlSearch.CurrentPageSize, ctrlSearch.CurrentPage);
            ctrlSearch.AllCount = pagingInfo.TotalCount;
            ctrlSearch.StartCount = pagingInfo.StartIndex;
            ctrlSearch.EndCount = pagingInfo.EndIndex;
            ctrlSearch.PageCount = pagingInfo.TotalPages;
            ctrlSearch.CurrentPage =
                CurrentPage = pagingInfo.CurrentPage;
        }
		protected void repeatMap_ItemCreated(Object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Header)
			{
				var phStoreHeadTableData = e.Item.FindControl<PlaceHolder>("phStoreHeadTableData");
				foreach (Store store in Store.GetStoreList())
					phStoreHeadTableData.Controls.Add(new LiteralControl("<td  class='rgHeader'  align='center'>{0}</td>".FormatWith(Trim(store.Name, 10))));
			}
		}

		protected void repeatMap_ItemDataBound(Object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
				return;

			var dataItem = e.Item.DataItem as IGrouping<Int32, MappedObject>;
            if (dataItem == null)
                return;
			
			var phStoreRowTableData = e.Item.FindControl<PlaceHolder>("phStoreRowTableData");

			foreach (Store store in Store.GetStoreList())
			{
				CheckBox chkStore = new CheckBox { ID = "chkStore{0}".FormatWith(store.StoreID) };

				if (dataItem.Any(mo => mo.StoreID == store.StoreID && mo.IsMapped))
					chkStore.Checked = true;

				phStoreRowTableData.Controls.Add(new LiteralControl("<td align='center'>"));
				phStoreRowTableData.Controls.Add(chkStore);
				phStoreRowTableData.Controls.Add(new LiteralControl("</td>"));
			}
		}

		protected void repeatMap_ItemCommand (Object sender, RepeaterCommandEventArgs e)
		{
			if (e.CommandName == "SaveMapping")
				SaveMappping();
		}

        #endregion

        #region Private Methods

        public PagingInfo BindData(String entityType, FilterType filterType, String filter, Int32 pageSize, Int32 currentPage)
        {
            string alphaFilter = filterType.Equals(FilterType.AlphaFilter) ? filter : string.Empty;
            string searchFilter = filterType.Equals(FilterType.Search) ? filter : string.Empty;

			var repeatMap = ctrlSearch.FindControl<Repeater>("repeatMap");

            AspDotNetStorefront.MappedObjectCollection firstMO = null;

            IList<MappedObject> mappedObjects = new List<MappedObject>();
            foreach (Store store in Store.GetStoreList())
            {
                firstMO = AspDotNetStorefront.MappedObjectCollection.GetObjects(store.StoreID, entityType, alphaFilter, searchFilter, pageSize, currentPage);
                foreach (MappedObject mappedObject in AspDotNetStorefront.MappedObjectCollection.GetObjects(store.StoreID, entityType, alphaFilter, searchFilter, pageSize, currentPage))
                    mappedObjects.Add(mappedObject);
            }

            var mappedObjectGroups = mappedObjects.GroupBy(m => m.ID);
            repeatMap.DataSource =
				this.Datasource = mappedObjectGroups;
            repeatMap.DataBind();
            repeatMap.Visible = true;

            PagingInfo pagingInfo = new PagingInfo();
            if (firstMO != null)
                pagingInfo = firstMO.PageInfo;

            return pagingInfo;
        }

		private void SaveMappping()
		{
			var repeatMap = ctrlSearch.FindControl<Repeater>("repeatMap");
			var editItems = repeatMap.Items.Cast<RepeaterItem>().Where(item => item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem);
			foreach (var item in editItems)
			{
				var lblID = item.FindControl<Label>("lblID");
				int entityId = lblID.Text.ToNativeInt();
				
				var dataItems = this.Datasource.Where(g => g.Key == entityId).SelectMany(mo => mo.Select(m => m));
				foreach (var mappedObject in dataItems)
				{
					CheckBox chkStore = item.FindControl<CheckBox>("chkStore{0}".FormatWith(mappedObject.StoreID));

					mappedObject.IsMapped = chkStore.Checked;
					mappedObject.Save();
				}
			}
		}

		private String Trim(String text, Int32 count)
		{
			if (text.Length <= count)
				return text;

			return text.Substring(0, count) + "...";
		}

        #endregion

		#region Protected Methods

		protected String ML_Localize(String text)
		{
			String localeSetting = Localization.GetDefaultLocale();
			if (ThisCustomer != null)
				localeSetting = ThisCustomer.LocaleSetting;

			return XmlCommon.GetLocaleEntry(text, localeSetting, false);
		}

		protected T DataItemAs<T>(RepeaterItem item) where T : class
		{
			return item.DataItem as T;
		}

		#endregion
}
}
