// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.IO;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontAdmin.Controls;
using AjaxControlToolkit;

namespace AspDotNetStorefrontAdmin
{
    public partial class Admin_editkit2 : AdminPageBase
    {
        public KitProductData Kit { get; private set; }

        public List<KitGroupType> GroupTypes { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            int productId = Request.QueryStringNativeInt("productId");

            Customer ThisCustomer = Customer.Current;

            this.Kit = KitProductData.Find(productId, ThisCustomer);

            if (this.Kit!=null)
            {
                InitializeDataSources();
                BindData();

                if (!this.IsPostBack)
                {
                    InitializeLocales();
                }

                CreateBreadCrumb();
            }
            

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void CreateBreadCrumb()
        {
            ltProduct.Text = "<a href=\"entityEditProducts.aspx?iden=" + Kit.Id + "&entityName=" + CommonLogic.QueryStringCanBeDangerousContent("entityName") + "&entityFilterID=" + CommonLogic.QueryStringCanBeDangerousContent("entityFilterID") + "\">" + AppLogic.GetProductName(Kit.Id, ThisCustomer.LocaleSetting) + " (" + Kit.Id + ")</a>";
        }

        protected void dlKitGroups_ItemDataBound(object sender, DataListItemEventArgs e)
        {
        }

        protected void dlKitGroups_ItemCreated(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string confirmDeleteScript = "return confirm('Are you sure you want to delete this group?');";

                ImageButton cmdDelete1 = e.Item.FindControl<ImageButton>("cmdDelete1");
                cmdDelete1.Attributes["onclick"] = confirmDeleteScript;

                LinkButton cmdDelete2 = e.Item.FindControl<LinkButton>("cmdDelete2");
                cmdDelete2.Attributes["onclick"] = confirmDeleteScript;
            }
        }

        private const string SAVE_COMMAND_INVOKER = "SaveCommandInvoker";
        public string SaveCommandInvoker
        {
            get { return null == ViewState[SAVE_COMMAND_INVOKER] ? string.Empty : (string)ViewState[SAVE_COMMAND_INVOKER]; }
            set { ViewState[SAVE_COMMAND_INVOKER] = value; }
        }

        protected void dlKitGroups_ItemCommand(object source, DataListCommandEventArgs e)
        {
            //System.Threading.Thread.Sleep(5000);

            switch (e.CommandName)
            {
                case "Save":
                    // let's determine which button via the CommandArgument
                    ResolveKitGroupChanges(e.Item);
                    SaveCommandInvoker = e.CommandArgument.ToString();

                    UpdatePanel pnlUpdateKitGroup = e.Item.FindControl<UpdatePanel>("pnlUpdateKitGroup");                    
                    pnlUpdateKitGroup.Update();
                    
                    break;

                case "DeleteGroup":
                    DeleteKitGroup(e);                   
                    break;

                case "Delete_KitItem":
                    DeleteKitItem(e);                    
                    break;
            }

            // rebind
            BindData();
        }

        protected bool ShouldHighlightNotification(int updatedGroupId, string commandInvoker)
        {
            var updatedGroup = Kit.Groups.Find(group => group.Id == updatedGroupId && group.IsModified);
            if (updatedGroup != null)
            {
                return this.SaveCommandInvoker.EqualsIgnoreCase(commandInvoker);
            }

            return false;
        }

        private void DeleteKitGroup(DataListCommandEventArgs e)
        {
            var hdfGroupId = e.Item.FindControl<HiddenField>("hdfGroupId");
            int groupId = hdfGroupId.Value.ToNativeInt();
            KitGroupData kitGroup = this.Kit.GetGroup(groupId);
            Kit.DeleteGroup(kitGroup);

            // force the update of the whole groups            
            pnlUpdateAllGroups.Update();
        }

        private void DeleteKitItem(DataListCommandEventArgs e)
        {
            var hdfGroupId = e.Item.FindControl<HiddenField>("hdfGroupId");
            int groupId = hdfGroupId.Value.ToNativeInt();
            KitGroupData kitGroup = this.Kit.GetGroup(groupId);

            int id = e.CommandArgument.ToString().ToNativeInt();
            KitItemData toBeDeleted = kitGroup.GetItem(id);
            if (toBeDeleted != null)
            {
                kitGroup.DeleteItem(toBeDeleted);
            }
        }

        private void InitializeLocales()
        {            
            using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
            {
                conn.Open();
                using (IDataReader thisReader = DB.GetRS("select Name from LocaleSetting with (NOLOCK) order by DisplayOrder,Description", conn))
                {
                    while (thisReader.Read())
                    {
                        cboLocale.Items.Add(new ListItem(DB.RSField(thisReader, "Name"), DB.RSField(thisReader, "Name")));                        
                    }
                }
            }
            cboLocale.Items.FindByValue(Localization.GetDefaultLocale()).Selected = true;

            if (cboLocale.Items.Count < 2)
            {
                cboLocale.SelectedValue = Localization.GetDefaultLocale();
            }

            if (cboLocale.Items.Count == 1)
            {
                pnlLocale.Visible = false;                
            }
        }

        private string GetCurrentLocale()
        {
            string pageLocale = cboLocale.SelectedValue;
            if (pageLocale.Equals(string.Empty))
            {
                pageLocale = Localization.GetDefaultLocale();
            }

            return pageLocale;
        }

        private void ResolveKitGroupChanges(DataListItem dataItem)
        {
            var hdfGroupId = dataItem.FindControl<HiddenField>("hdfGroupId");
            int groupId = hdfGroupId.Value.ToNativeInt();
            KitGroupData kitGroup = this.Kit.GetGroup(groupId);

            if (kitGroup != null)
            {
                bool wasNew = kitGroup.IsNew;

                ResolveKitItemChanges(dataItem, kitGroup);

                KitItemData newItem = kitGroup.Items.Find(item => item.IsNew);

                // if name was provided allow name, otherwise don't save
                if (newItem != null &&
                    string.IsNullOrEmpty(newItem.Name))
                {
                    // kick it out
                    kitGroup.Items.Remove(newItem);
                }

                // validate this group and all it's items
                kitGroup.Validate();

                if (kitGroup.IsValid)
                {
                    string locale = GetCurrentLocale();
                    kitGroup.Save(locale);

                    kitGroup.IsModified = true;
                }

                kitGroup.ProvideNewKitItem();

                if (wasNew)
                {
                    // previous group was a new kit group
                    // allow for a new kit group for input
                    var newGroup = kitGroup.Kit.ProvideNewGroup();
                    newGroup.ProvideNewKitItem();

                    // force the update of the whole groups            
                    pnlUpdateAllGroups.Update();
                }
            }
        }

        private void ResolveKitItemChanges(DataListItem dataItem, KitGroupData kitGroup)
        {
            var ctrlKitGroup = dataItem.FindControl<Admin_controls_editkitgrouptemplate>("ctrlKitGroup");
            ctrlKitGroup.KitGroup = kitGroup;
            ctrlKitGroup.ReconcileChanges();
        }

        private void RebindGroup(DataListItem dataItem)
        {
            var hdfGroupId = dataItem.FindControl<HiddenField>("hdfGroupId");
            int groupId = hdfGroupId.Value.ToNativeInt();
            KitGroupData kitGroup = this.Kit.GetGroup(groupId);
            
            var ctrlKitGroup = dataItem.FindControl<Admin_controls_editkitgrouptemplate>("ctrlKitGroup");
            ctrlKitGroup.KitGroup = kitGroup;
            ctrlKitGroup.GroupTypes = this.GroupTypes;
            ctrlKitGroup.DataBind();
        }


        private void InitializeDataSources()
        {
            this.Kit.ProvideNewGroup();

            foreach (KitGroupData group in this.Kit.Groups)
            {
                group.ProvideNewKitItem();
            }

            InitializeGroupTypes();
        }

        private void InitializeGroupTypes()
        {
            GroupTypes = KitGroupType.GetAll();
        }

        private void BindData()
        {
            dlKitGroups.DataSource = Kit.Groups;
            dlKitGroups.DataBind();
        }

        protected T FindContainer<T>(Control container) where T : class
        {
            T lookFor = null;

            while (lookFor == null)
            {
                if(container is T)
                {
                    lookFor = container as T;
                    break;
                }

                container = container.Parent;
            }

            return lookFor;
        }

        protected void cboLocale_SelectedIndexChanged(object sender, EventArgs e)
        {
            string currentLocale = cboLocale.SelectedValue;
            Kit = KitProductData.Find(Kit.Id, ThisCustomer, currentLocale);
            InitializeDataSources();
            BindData();

            // force the update of the whole groups            
            pnlUpdateAllGroups.Update();
        }
    }
}

