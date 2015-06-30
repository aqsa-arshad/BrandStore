// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Threading;
using System.Text;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefront
{
    public partial class MinicartControl : System.Web.UI.UserControl
    {
        ShoppingCart cart = null;
        Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)HttpContext.Current.User).ThisCustomer;

        private bool m_refreshed;
        public bool Refreshed
        {
            get { return m_refreshed; }
            set { m_refreshed = value; }
        }

        protected void ctrlMiniCart_OnItemDeleting(object sender, ItemEventArgs e)
        {
            ShoppingCartControl ctrlMiniCart = this.FindControl("ctrlMiniCart") as ShoppingCartControl;

            int crtType = 0;
            using (SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader reader = DB.GetRS(string.Format("select CartType from ShoppingCart where ShoppingCartRecID = {0}", e.ID), conn))
                {
                    if (reader.Read())
                    {
                        crtType = DB.RSFieldInt(reader, "cartType");
                    }
                }
            }

            cart.RemoveItem(e.ID, true);

            cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false, true);

            BindMiniCart();
        }

        protected void ctrlMiniCart_OnMiniCartItemUpdate(object sender, EventArgs e)
        {
            ShoppingCartControl ctrlMiniCart = this.FindControl("ctrlMiniCart") as ShoppingCartControl;

            int quantity = 0;
            int sRecID = 0;
            string itemNotes = string.Empty;
            CartItemCollection cartItemsCopy = null;

            for (int i = 0; i < ctrlMiniCart.Items.Count; i++)
            {
                quantity = ctrlMiniCart.Items[i].Quantity;
                sRecID = ctrlMiniCart.Items[i].ShoppingCartRecId;
                itemNotes = ctrlMiniCart.Items[i].ItemNotes;

                //prevent negative quantities
                if (quantity > 0)
                {
                    cart.SetItemQuantity(sRecID, quantity);
                    cart.SetItemNotes(sRecID, CommonLogic.CleanLevelOne(itemNotes));
                }
                else
                {
                    cart.SetItemQuantity(sRecID, 0);
                    cart.SetItemNotes(sRecID, CommonLogic.CleanLevelOne(itemNotes));
                }

                cart.CheckMinimumQuantities(ThisCustomer.CustomerID);
                cartItemsCopy = cart.CartItems;
            }

            cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false, true);

            foreach (var a in cartItemsCopy.Where(n => n.MinimumQuantityUdpated.Equals(true)))
            {
                for (int i = 0; i < cart.CartItems.Count; i++)
                {
                    if (cart.CartItems[i].ShoppingCartRecordID == a.ShoppingCartRecordID)
                    {
                        cart.CartItems[i].MinimumQuantityUdpated = a.MinimumQuantityUdpated;
                        break;
                    }
                }
            }
            BindMiniCart();
        }

        private void BindMiniCart()
        {
            if (cart.InventoryTrimmed)
                ctrlMiniCart.InventoryTrimmed = true;
            ctrlMiniCart.DataSource = cart.CartItems;
            ctrlMiniCart.DataBind();
        }


        protected override void OnPreRender(EventArgs e)
        {
            ctrlMiniCart.MinicartSummarySetting.UseInAjaxMiniCart = ThisCustomer.IsRegistered;

            BindMiniCart();
            lblItemCount.Text = string.Format("({0})", ShoppingCart.NumItems(ThisCustomer.CustomerID, CartTypeEnum.ShoppingCart));

            lnkRefresh.Attributes["style"] = "display:none";

            base.OnPreRender(e);
        }


        protected override void OnInit(EventArgs e)
        {
            Refreshed = false;
            InitializeDataSource();
            SetupPrerequisites();
            RegisterScripts();

            base.OnInit(e);
        }

        private void InitializeDataSource()
        {
            cart = new ShoppingCart(ThisCustomer.SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false, true);
            ShoppingCartControl ctrlMiniCart = this.FindControl("ctrlMiniCart") as ShoppingCartControl;
            ctrlMiniCart.DisplayMode = CartDisplayMode.MiniCart;

            List<CartItem> c = default(List<CartItem>);
            c = cart.CartItems.Where((CartItem n) => n.CartType.Equals((CartTypeEnum)101)).ToList();

            List<int> recItemToRemove = new List<int>();
            foreach (var ci in c)
            {
                recItemToRemove.Add(ci.ShoppingCartRecordID);
            }

            foreach (int a in recItemToRemove)
            {
                cart.SetItemQuantity(a, 0);
            }

            BindMiniCart();
        }

        private void SetupPrerequisites()
        {
            scrptProxy.Scripts.Add(new ScriptReference("~/jscripts/minicart.js"));            
        }

        protected void lnkRefresh_Click(object sender, EventArgs e)
        {
            Refreshed = true;
            extCollapseMinicart.Collapsed = false;
        }

        private void RegisterScripts()
        {
            StringBuilder script = new StringBuilder();

            script.AppendFormat("    var fMnc = function() {{\n");
            script.AppendFormat("        var minCart = $create(aspdnsf.Controls.MinicartControl, null, null, null, $get('{0}'));\n", pnlMiniCart.ClientID);

            // delegate method to trigger async postback in order to update the shoppingcart and the display
            script.AppendFormat("        minCart.setRefreshDelegate( function(){{ {0} }} );\n", Page.ClientScript.GetPostBackEventReference(lnkRefresh, string.Empty));

            // set the collapsible extender instance for reference
            script.AppendFormat("        minCart.setExtender( '{0}' );\n", extCollapseMinicart.ClientID);

            // we only need 1 instance of the minicart
            script.AppendFormat("        aspdnsf.Controls.Minicart.setInstance(minCart);\n");

            script.AppendFormat("        Sys.Application.remove_load(fMnc);\n");
            script.AppendFormat("    }}\n");

            script.AppendFormat("    Sys.Application.add_load(fMnc);\n");

            ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script.ToString(), true);
        }

    }
}

