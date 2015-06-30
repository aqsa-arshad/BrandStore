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

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EntityMenu : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Build the horizontal menu
                BuildMenu();
            }
        }

        /// <summary>
        /// Populates the horizontal navigation menu control
        /// </summary>
        private void BuildMenu()
        {
            //TODO:  Since this fires on every page, add caching logic
            String locale = Localization.GetDefaultLocale();
            mnuEntityNav.Orientation = Orientation.Horizontal;

            MenuItem[] items = {
                                GetMenuItem("admin.menu.Categories", "organizationCategories", "NewEntities.aspx?entityname=category"),
                                GetMenuItem("admin.menu.Sections", "organizationSections", "NewEntities.aspx?entityname=section"),
                                /* UNSUPPORTED ENTITES - GENRES AND VECTORS - UNCOMMENT TO ADD TO MENU
                                GetMenuItem("admin.menu.Genres", "organizationGenres", "/newentities.aspx?entityname=genre"),
                                GetMenuItem("admin.menu.Vectors", "organizationVectors", "/newentities.aspx?entityname=vector"),
                                */
                                GetMenuItem("admin.menu.Manufacturers", "organizationManufacturers", "NewEntities.aspx?entityname=manufacturer"),
                                GetMenuItem("admin.menu.Distributors", "organizationdistributor", "NewEntities.aspx?entityname=distributor"),
                                GetMenuItem("admin.menu.ProductMgr", "productManager", "newproducts.aspx")
                               };

            foreach (MenuItem m in items)
            {
                mnuEntityNav.Items.Add(m);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private MenuItem GetMenuItem(String key, String value)
        {
            return GetMenuItem(key, value, String.Empty, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="Url"></param>
        /// <returns></returns>
        private MenuItem GetMenuItem(String key, String value, String Url)
        {
            return GetMenuItem(key, value, Url, String.Empty);
        }

        /// <summary>
        /// Builds the root level menu item nodes
        /// </summary>
        /// <param name="node">String Resource Key of the node you wish to create a menu item for (eg. Orders)</param>
        /// <param name="Url">Url to navigate to if the menu item is clicked</param>
        /// <returns>MenuItem object populated with all values and child items</returns>
        private MenuItem GetMenuItem(String key, String value, String Url, String target)
        {
            String name = AppLogic.GetStringForDefaultLocale(key);

            MenuItem item = new MenuItem();
            item.Text = AppLogic.GetStringForDefaultLocale(key);
            item.Value = CommonLogic.IIF(String.IsNullOrEmpty(value), name, value);

            if (!String.IsNullOrEmpty(Url))
            {
                if (Url.StartsWithIgnoreCase("http"))
                {
                    // Fully qualified URL.  Don't change it
                    item.NavigateUrl = Url;
                }
                else
                {
                    // Relative URL.  Format it as a fully qualified URL
                    item.NavigateUrl = AppLogic.AdminLinkUrl(Url);
                }
            }

            if (!String.IsNullOrEmpty(target))
            {
                item.Target = target;
            }

            return item;
        }
    }
}
