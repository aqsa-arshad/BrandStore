// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using System.Linq;

namespace AspDotNetStorefrontControls
{
    public partial class EntityToStoreMapper : System.Web.UI.UserControl
    {
        private List<AspDotNetStorefront.CachelessStore> m_stores;
        public List<AspDotNetStorefront.CachelessStore> Stores
        {
            get { return m_stores; }
            set { m_stores = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            Stores = AspDotNetStorefront.CachelessStore.GetStoreList();

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        public override void DataBind()
        {
            if (EntityType == null)
            {
                throw new InvalidOperationException("Cannot bind without setting EntityType");
            }

            var stores = AspDotNetStorefront.CachelessStore.GetStoreList();
            var mappedStoreIds = stores
                                .Where(store => store.IsMapped(this.EntityType, this.ObjectID))
                                .Select(store => store.StoreID)
                                .ToArray();

            ssMain.SelectedStoreIDs = mappedStoreIds;
        }


        public void Save()
        {
            foreach (var storeId in ssMain.SelectedStoreIDs)
            {
                var store = Stores.Find(s => s.StoreID == storeId);
                store.UpdateMapping(this.EntityType, this.ObjectID, true);
            }

            foreach (var storeId in ssMain.UnSelectedStoreIDs)
            {
                var store = Stores.Find(s => s.StoreID == storeId);
                store.UpdateMapping(this.EntityType, this.ObjectID, false);
            }
        }

#region "properties"
        /// <summary>
        /// The type of entity for which to map against
        /// </summary>
        public string EntityType
        {
            get
            {
                if (ViewState["EntityType"] != null)
                {
                    return (string)ViewState["EntityType"];
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["EntityType"] = value;
            }
        }

        public bool ShowText
        {
            get
            {
                return ssMain.ShowText;
            }
            set
            {
                ssMain.ShowText = value;
            }
        }

        /// <summary>
        /// The ID of the object to map against
        /// </summary>
        public int ObjectID
        {
            get
            {
                if (ViewState["ObjectID"] != null)
                {
                    return (int)ViewState["ObjectID"];
                }
                else
                {
                    return -1;
                }
            }
            set
            {
                ViewState["ObjectID"] = value;
            }
        }

        /// <summary>
        /// Number of stores retrieved
        /// </summary>
        public int StoreCount
        {
            get
            {
                return ssMain.StoreCount;
            }
        }

        public string Text
        {
            get
            {
                return ssMain.Text;
            }
            set
            {
                ssMain.Text = value;
            }
        }

#endregion
    }    
}
