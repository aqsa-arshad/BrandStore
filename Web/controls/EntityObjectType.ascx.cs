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
using AspDotNetStorefront;
using AjaxControlToolkit;
using Telerik.Web.UI;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefront.TemplateEditors
{
    public partial class EntityObjectTypeControl : BaseUserControl<DatabaseObjectCollection>
    {
        protected override void OnInit(EventArgs e)
        {
            EnsureEntityTypes();

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            //if (!this.IsPostBack)
            //{
            //    EnsureEntityTypes();
            //}
            //ProvideEntityTypes();
            //if (!this.IsPostBack)
            //{
            //    ProvideEntityTypes();
            //    EntityType = entityTypes[0];
            //}

            base.OnLoad(e);
        }

        private void EnsureEntityTypes()
        {
            if (cboEntityType.Items.Count == 0)
            {
                string[] entityTypes = new string[] { "Product", "Category", "Manufacturer", "Section" };

                foreach(string etype in entityTypes)
                {
                    cboEntityType.Items.Add(etype);
                }
            }
        }

        public string EntityType
        {
            get
            {
                return cboEntityType.SelectedValue;
            }
            set
            {
                var items = cboEntityType.Items.Cast<ListItem>();
                var selectedValue = items.FirstOrDefault(item => value.EqualsIgnoreCase(item.Value));
                if (selectedValue != null)
                {
                    // default all non-selected
                    items.ForEachItem(item => item.Selected = false);

                    selectedValue.Selected = true;
                    ctrlSelectEntity.EntityType = value;
                }
            }
        }

        protected void cboEntityType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ctrlSelectEntity.EntityType = cboEntityType.SelectedValue;
            //ctrlSelectEntity.Text = string.Empty;
        }

        public int EntityID
        {
            get { return ctrlSelectEntity.EntityID; }
            set { ctrlSelectEntity.EntityID = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ctrlSelectEntity.EntityType = this.EntityType;
            ctrlSelectEntity.EntityID = this.EntityID;
        }

    }
}
