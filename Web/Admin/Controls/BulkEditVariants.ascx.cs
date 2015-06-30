// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using Telerik.Web.UI;

namespace AspDotNetStorefrontControls
{
    public partial class BulkEditVariants : System.Web.UI.UserControl
    {
        #region Private variables

        private Customer ThisCustomer;

        #endregion


        #region Events

        /// <summary>
        /// Default page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            ThisCustomer = AppLogic.GetCurrentCustomer();

            if (!IsPostBack)
            {
                ViewState["EntityType"] = String.Empty;
                ViewState["EntityID"] = String.Empty;
            }
        }


        /// <summary>
        /// Handles the Product Filter entity type dropdown selected index changed event and
        /// populates the entity name dropdown based on the value selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlEntityType_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            List<GridEntity> entityList = new List<GridEntity>();

            #region Retrieve Entity List
            //No entity selected
            if (ddlEntityType.SelectedValue == "-1")
            {
                ddlEntityName.Items.Clear();
                ddlEntityName.Enabled = false;

                return;
            }
            else if (ddlEntityType.SelectedValue.Equals("1"))
            {
                if (HttpContext.Current.Cache["controlsBulkEditVariants_m_CategoryList"] == null)
                {
                    entityList = GridEntity.GetAllEntitiesOfType("Category");
                    entityList = entityList.OrderBy(el => el.Name).ToList();
                    ddlEntityName.Enabled = true;
                    CacheItem(entityList, "controlsBulkEditVariants_m_CategoryList"); 
                }
                else
                {
                    entityList = (List<GridEntity>)HttpContext.Current.Cache["controlsBulkEditVariants_m_CategoryList"];
                    ddlEntityName.Enabled = true;
                }
            }
            else if (ddlEntityType.SelectedValue.Equals("2"))
            {
                if (HttpContext.Current.Cache["controlsBulkEditVariants_m_manufacturerList"] == null)
                {
                    entityList = GridEntity.GetAllEntitiesOfType("Manufacturer");
                    entityList = entityList.OrderBy(el => el.Name).ToList();
                    ddlEntityName.Enabled = true;
                    CacheItem(entityList, "controlsBulkEditVariants_m_manufacturerList");
                }
                else
                {
                    entityList = (List<GridEntity>)HttpContext.Current.Cache["controlsBulkEditVariants_m_manufacturerList"];
                    ddlEntityName.Enabled = true;
                }
            }
            else if (ddlEntityType.SelectedValue.Equals("3"))
            {
                if (HttpContext.Current.Cache["controlsBulkEditVariants_m_sectionList"] == null)
                {
                    entityList = GridEntity.GetAllEntitiesOfType("Section");
                    entityList = entityList.OrderBy(el => el.Name).ToList();
                    ddlEntityName.Enabled = true;
                    CacheItem(entityList, "controlsBulkEditVariants_m_sectionList");
                }
                else
                {
                    entityList = (List<GridEntity>)HttpContext.Current.Cache["controlsBulkEditVariants_m_sectionList"];
                    ddlEntityName.Enabled = true;
                }
            }
            else if (ddlEntityType.SelectedValue.Equals("4"))
            {
                if (HttpContext.Current.Cache["controlsBulkEditVariants_m_distributorList"] == null)
                {
                    entityList = GridEntity.GetAllEntitiesOfType("Distributor");
                    entityList = entityList.OrderBy(el => el.Name).ToList();
                    ddlEntityName.Enabled = true;
                    CacheItem(entityList, "controlsBulkEditVariants_m_distributorList");
                }
                else
                {
                    entityList = (List<GridEntity>)HttpContext.Current.Cache["controlsBulkEditVariants_m_distributorList"];
                    ddlEntityName.Enabled = true;
                }
            }
            #endregion

            ddlEntityName.ClearSelection();
            ddlEntityName.Items.Clear();

            foreach (GridEntity ge in entityList)
            {
                ddlEntityName.Items.Add(new ListItem(ge.LocaleName, ge.ID.ToString()));
            }

            ddlEntityName.Enabled = true;
        }


        /// <summary>
        /// Repopulates the grid control using a new GridProductVariant list based on the
        /// selected filter criteria
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdateFilters_OnClick(object sender, EventArgs e)
        {
            if (ddlEntityType.SelectedValue != null && ddlEntityType.SelectedValue != "-1")
            {
                ViewState["EntityType"] = ddlEntityType.SelectedValue;
                ViewState["EntityID"] = ddlEntityName.SelectedValue;
            }
            else if (ddlEntityType.SelectedValue != null)
            {
                ViewState["EntityType"] = String.Empty;
                ViewState["EntityID"] = String.Empty;
            }

            grdVariants.Rebind();

            //Send them back to page 1
            grdVariants.CurrentPageIndex = 0;
        }


        protected void grdVariants_OnNeedDatasource(object sender, EventArgs e)
        {
            int pageSize = grdVariants.PageSize;
            int startIndex = (grdVariants.PageSize * grdVariants.CurrentPageIndex);
            int entityType = 0;
            int entityID = 0;

            if (!String.IsNullOrEmpty(ViewState["EntityType"].ToString()) && !String.IsNullOrEmpty(ViewState["EntityID"].ToString()))
            {
                entityType = Convert.ToInt32(ViewState["EntityType"]);
                entityID = Convert.ToInt32(ViewState["EntityID"]);
            }

            grdVariants.DataSource = GridProductVariant.GetVariantsPaged(pageSize, startIndex, entityType, entityID);

            grdVariants.VirtualItemCount = GridProductVariant.GetVariantCount(entityType, entityID);
        }


        /// <summary>
        /// Event that fires when a grid item is created.  Logic for populating template fields and
        /// images is contained here.  Replaces .NET Gridview OnRowCreated event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void grdVariants_OnItemCreated(object sender, GridItemEventArgs e)
        {
            int productID = 0;

            if (e.Item is GridDataItem)
            {
                GridProductVariant gpv = (GridProductVariant)e.Item.DataItem;

                GridDataItem di = (GridDataItem)e.Item;

                if (gpv != null)
                {
                    productID = gpv.ProductID;
                }

                if (di != null)
                {
                    //This has to be done manually since editing does not support alternate row styles
                    bool isAlternate = (di.RowIndex % 2 != 0);

                    if (isAlternate)
                    {
                        di.BackColor = System.Drawing.Color.LightSteelBlue;
                    }
                    else
                    {
                        di.BackColor = System.Drawing.Color.White;
                    }
                }
            }

            if (e.Item is GridEditableItem && e.Item.IsInEditMode)
            {
                TextBox txtName = (TextBox)e.Item.FindControl("txtName");
                TextBox txtSKU = (TextBox)e.Item.FindControl("txtSKUSuffix");
                TextBox txtPrice = (TextBox)e.Item.FindControl("txtPrice");
                TextBox txtSalePrice = (TextBox)e.Item.FindControl("txtSalePrice");
                TextBox txtInventory = (TextBox)e.Item.FindControl("txtInventory");
                Image imgProduct = (Image)e.Item.FindControl("imgProduct");


                String strImageUrl = String.Empty;

                if (productID != 0)
                {
                    strImageUrl = AppLogic.LookupImage("product", productID, "icon", 1, ThisCustomer.LocaleSetting);
                }


                if ((e.Item as GridEditableItem)["Published"] != null)
                {
                    (e.Item as GridEditableItem)["Published"].Enabled = chkPublished.Checked;
                }

                if (txtName != null)
                {
                    txtName.Enabled = chkName.Checked;

                    if (!txtName.Enabled)
                    {
                        txtName.BackColor = System.Drawing.Color.Gainsboro;
                    }
                }

                if (txtSKU != null)
                {
                    txtSKU.Enabled = chkSKU.Checked;

                    if (!txtSKU.Enabled)
                    {
                        txtSKU.BackColor = System.Drawing.Color.Gainsboro;
                    }
                }

                if (txtPrice != null)
                {
                    txtPrice.Enabled = chkPrice.Checked;

                    if (!txtPrice.Enabled)
                    {
                        txtPrice.BackColor = System.Drawing.Color.Gainsboro;
                    }
                }

                if (txtSalePrice != null)
                {
                    txtSalePrice.Enabled = chkSalePrice.Checked;

                    if (!txtSalePrice.Enabled)
                    {
                        txtSalePrice.BackColor = System.Drawing.Color.Gainsboro;
                    }
                }

                if (txtInventory != null)
                {
                    txtInventory.Enabled = chkInventory.Checked;

                    if (!txtInventory.Enabled)
                    {
                        txtInventory.BackColor = System.Drawing.Color.Gainsboro;
                    }
                }

                if (imgProduct != null && !String.IsNullOrEmpty(strImageUrl))
                {
                    imgProduct.ImageUrl = strImageUrl;
                    imgProduct.Visible = true;
                }

            }
        }


        /// <summary>
        /// Event that fires whenever a grid item is bound to the datasource
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void grdVariants_OnItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item is GridDataItem && e.Item.IsInEditMode)
            {
                GridDataItem dataItem = (GridDataItem)e.Item;

                //Hides the Update and Cancel buttons for each edit form
                dataItem["EditCommandColumn"].Visible = false;
            }
        }


        /// <summary>
        /// Event that fires when the entire grid control has been bound to the datasource
        /// Used to setup paging logic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void grdVariants_OnDataBound(object sender, EventArgs e)
        {
            
        }


        /// <summary>
        /// Event that fires whenever an item command is executed by the grid
        /// such as clicking the Update All button and passes the resultant data
        /// to the proper routine for processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void grdVariants_OnItemCommand(object sender, GridCommandEventArgs e)
        {
            if (e.CommandName.EqualsIgnoreCase("UpdateAll"))
            {
                foreach (GridEditableItem item in grdVariants.EditItems)
                {
                    Hashtable ht = new Hashtable();

                    e.Item.OwnerTableView.ExtractValuesFromItem(ht, item);

                    UpdateItem(ht);
                }

                grdVariants.Rebind();
            }
            else if (e.CommandName.EqualsIgnoreCase("Reset"))
            {
                grdVariants.DataBind();
            }
        }



        /// <summary>
        /// Event that fires before the grid control is rendered to the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void grdVariants_PreRender(object sender, EventArgs e)
        {
            foreach (GridItem gi in grdVariants.MasterTableView.Items)
            {
                if (gi is GridEditableItem)
                {
                    GridEditableItem ge = (GridDataItem)gi;

                    ge.Edit = true;
                }

            }

            grdVariants.Rebind();
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Performs the actual update to the variant record after the Update All command is issued
        /// </summary>
        /// <param name="ht">Hash tabled containing the values extracted from each grid data row</param>
        private void UpdateItem(Hashtable ht)
        {
            int variantID = Convert.ToInt32(ht["VariantID"]);
            String name = String.Empty;
            String skuSuffix = String.Empty;
            Decimal price = Convert.ToDecimal(ht["Price"]);
            Decimal salePrice = Convert.ToDecimal(ht["SalePrice"]);
            int inventory = Convert.ToInt32(ht["Inventory"]);
            bool isPublished = Convert.ToBoolean(ht["Published"]);

            if (ht["LocaleName"] != null)
            {
                name = AppLogic.FormLocaleXml(ht["LocaleName"].ToString(), Localization.GetDefaultLocale());
            }

            if (ht["SKUSuffix"] != null)
            {
                skuSuffix = ht["SKUSuffix"].ToString();
            }

            GridProductVariant pv = new GridProductVariant(variantID);

            if (pv != null)
            {
                pv.Name = name;
                pv.Price = price;
                pv.SalePrice = salePrice;
                pv.SKUSuffix = skuSuffix;
                pv.Inventory = inventory;
                pv.Published = isPublished;

                pv.Commit();
            }

        }


        /// <summary>
        /// Inserts an item into the cache using the selected key
        /// </summary>
        /// <param name="itemToCache">The object to insert into the cache</param>
        /// <param name="key">The key used to retrieve the item from the cache</param>
        private void CacheItem(object itemToCache, string key)
        {
            HttpContext.Current.Cache.Insert(key, itemToCache, null, System.DateTime.Now.AddMinutes(AppLogic.CacheDurationMinutes()), TimeSpan.Zero);
        }

        #endregion

    }
}
