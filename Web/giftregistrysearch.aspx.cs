// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for giftregistrysearch.
    /// </summary>
    public partial class giftregistrysearch : SkinBase
    {
        ShoppingCart cart;
        String GiftRegistryGUID;
        int RegistryOwnerCustomerID;
                
        protected void Page_Load(object sender, System.EventArgs e)
        {
            SearchResults.Text = String.Empty;

            SectionTitle = AppLogic.GetString("giftregistrysearch.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            GiftRegistryGUID = CommonLogic.QueryStringCanBeDangerousContent("giftregistryid");

            if (GiftRegistryGUID.Trim().Length > 0)
            {
                RegistryOwnerCustomerID = AppLogic.GiftRegistryOwnerID(GiftRegistryGUID);
            }

            InitializePageContent();
            
        }

        public void btnSearchForName_Click(object sender, EventArgs e)
        {
            string sql = "select top 1 CustomerID,GiftRegistryGUID from Customer where ltrim(rtrim(lower(FirstName + ' ' + LastName)))=" + DB.SQuote(txtSearchForName.Text.ToLowerInvariant()) + " and GiftRegistryIsAnonymous=0 and ShippingAddressID IS NOT NULL and ShippingAddressID<>0 and GiftRegistryAllowSearchByOthers=1";
            DoSearch(sql);
        }
        public void btnSearchForNickName_Click(object sender, EventArgs e)
        {
            string sql = "select top 1 CustomerID,GiftRegistryGUID from Customer where lower(GiftRegistryNickName)=" + DB.SQuote(txtSearchForNickName.Text.ToLowerInvariant()) + " and ShippingAddressID IS NOT NULL and ShippingAddressID<>0 and GiftRegistryAllowSearchByOthers=1";
            DoSearch(sql);
        }
        public void btnSearchForEMail_Click(object sender, EventArgs e)
        {
            string sql = "select top 1 CustomerID,GiftRegistryGUID from Customer where email=" + DB.SQuote(txtSearchForEMail.Text.ToLowerInvariant()) + " and GiftRegistryIsAnonymous=0 and ShippingAddressID IS NOT NULL and ShippingAddressID<>0 and GiftRegistryAllowSearchByOthers=1";
            DoSearch(sql);
        }
        public void btnSaveButton_Click(object sender, EventArgs e)
        {
            try // ignore dups
            {
                string sql = "insert CustomerGiftRegistrySearches(CustomerID,GiftRegistryGUID) values(" + ThisCustomer.CustomerID.ToString() + "," + DB.SQuote(((Button)sender).CommandArgument.ToString()) + ")";
                DB.ExecuteSQL(sql);
            }
            catch { }
            Response.Redirect("giftregistry.aspx");
        }


        private void InitializePageContent()
        {      
            giftregistrysearch_aspx_3.Text = AppLogic.GetString("giftregistrysearch.aspx.3", SkinID, ThisCustomer.LocaleSetting);
            btnSearchForName.Text = AppLogic.GetString("giftregistrysearch.aspx.6", SkinID, ThisCustomer.LocaleSetting);
            txtSearchForName.Attributes.Add("onkeydown", "if(event.which || event.keyCode){if ((event.which == 13) || (event.keyCode == 13)) {document.getElementById('" + btnSearchForName.ClientID + "').click();return false;}} else {return true}; ");

            giftregistrysearch_aspx_4.Text = AppLogic.GetString("giftregistrysearch.aspx.4", SkinID, ThisCustomer.LocaleSetting);
            btnSearchForNickName.Text = AppLogic.GetString("giftregistrysearch.aspx.6", SkinID, ThisCustomer.LocaleSetting);
            txtSearchForNickName.Attributes.Add("onkeydown", "if(event.which || event.keyCode){if ((event.which == 13) || (event.keyCode == 13)) {document.getElementById('" + btnSearchForNickName.ClientID + "').click();return false;}} else {return true}; ");

            giftregistrysearch_aspx_5.Text = AppLogic.GetString("giftregistrysearch.aspx.5", SkinID, ThisCustomer.LocaleSetting);
            btnSearchForEMail.Text = AppLogic.GetString("giftregistrysearch.aspx.6", SkinID, ThisCustomer.LocaleSetting);
            txtSearchForEMail.Attributes.Add("onkeydown", "if(event.which || event.keyCode){if ((event.which == 13) || (event.keyCode == 13)) {document.getElementById('" + btnSearchForEMail.ClientID + "').click();return false;}} else {return true}; ");

            if (GiftRegistryGUID.Length != 0 && RegistryOwnerCustomerID != 0)
            {
                if (RegistryOwnerCustomerID == ThisCustomer.CustomerID)
                {
                    Response.Redirect("giftregistry.aspx"); // they are viewing their OWN registry!
                }
                pnlSearchResults.Visible = true;
                Customer RegistryOwnerCustomer = new Customer(RegistryOwnerCustomerID, true);

                cart = new ShoppingCart(SkinID, RegistryOwnerCustomer, CartTypeEnum.GiftRegistryCart, 0, false);

                BindDataTheirRegistry();

                String DisplayName = AppLogic.GiftRegistryDisplayName(RegistryOwnerCustomer.CustomerID, false, SkinID, ThisCustomer.LocaleSetting);
                giftregistry_aspx_16.Text = "<p align=\"left\">" + String.Format(AppLogic.GetString("giftregistry.aspx.16", SkinID, ThisCustomer.LocaleSetting), DisplayName);
                if (ThisCustomer.IsRegistered && ThisCustomer.HasCustomerRecord)
                {
                    giftregistry_aspx_16.Text += "&nbsp;&nbsp;";
                    btnSaveButton.CommandArgument = GiftRegistryGUID;
                    btnSaveButton.Visible = true;
                    btnSaveButton.Text = AppLogic.GetString("giftregistrysearch.aspx.12", SkinID, ThisCustomer.LocaleSetting);
                }
                else
                {
                    btnSaveButton.Visible = false;
                }
            }
            

        }
        private void DoSearch(string sql)
        {
            GiftRegistryGUID = String.Empty;
            RegistryOwnerCustomerID = 0;

            using(SqlConnection conn = DB.dbConn())
            {
                conn.Open();
                using (IDataReader rs = DB.GetRS(sql, conn))
                {
                    if (rs.Read())
                    {
                        GiftRegistryGUID = DB.RSFieldGUID(rs, "GiftRegistryGUID");
                        RegistryOwnerCustomerID = DB.RSFieldInt(rs, "CustomerID");
                    }
                }
            }

            if (GiftRegistryGUID.Length != 0 && RegistryOwnerCustomerID != 0)
            {
                Response.Redirect("~/giftregistrysearch.aspx?giftregistryid=" + GiftRegistryGUID);
            }
            else
            {
                // show that nothing was found
                SearchResults.Text = "" + AppLogic.GetString("giftregistrysearch.aspx.10", SkinID, ThisCustomer.LocaleSetting) + "";
            }
        }

        private void BindDataTheirRegistry()
        {
            ctrlTheirRegistry.DataSource = cart.CartItems;
            ctrlTheirRegistry.DataBind();
        }

        protected void ctrlTheirRegistry_MoveToShoppingCartInvoked(object sender, ItemEventArgs e)
        {
            // move is a misnomer, we're really "copying" the gift registry item, we don't remove it from the gift registry until THIS 
            // customer actually purchases it and the payment has cleared and only then if AppConfig:DecrementGiftRegistryOnOrder=true
            int MoveID = e.ID;
            if (GiftRegistryGUID.Length != 0 && MoveID != 0)
            {
                ThisCustomer.RequireCustomerRecord();
                String NewGUID = DB.GetNewGUID();
                int RegistryOwnerCustomerID = AppLogic.GiftRegistryOwnerID(GiftRegistryGUID);
                int ExistingShoppingCartRecID = 0;
                if (RegistryOwnerCustomerID != 0)
                {
                    // increment their cart if they already have this item in there for this gift recipient:
                    bool TheyHaveInCartAlready = false;
                    int ProductID = 0;
                    int VariantID = 0;
                    string ChosenColor = string.Empty;
                    string ChosenSize = string.Empty;
                    string TextOption = string.Empty;
                    string sqlx = string.Empty;

                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS("select * from ShoppingCart where ShoppingCartRecID=" + MoveID.ToString(), conn))
                        {
                            if (rs.Read())
                            {
                                ProductID = DB.RSFieldInt(rs, "ProductID");
                                VariantID = DB.RSFieldInt(rs, "VariantID");
                                ChosenColor = DB.RSField(rs, "ChosenColor");
                                ChosenSize = DB.RSField(rs, "ChosenSize");
                                TextOption = DB.RSField(rs, "TextOption");
                            }
                        }
                    }

                    sqlx = String.Format("select ShoppingCartRecID from shoppingcart where ProductID={0} and VariantID={1} and ChosenColor like {2} and ChosenSize like {3} and TextOption like {4} and GiftRegistryForCustomerID={5} and CustomerID={6}", ProductID.ToString(), VariantID.ToString(), DB.SQuote("%" + ChosenColor + "%"), DB.SQuote("%" + ChosenSize + "%"), DB.SQuote("%" + TextOption + "%"), RegistryOwnerCustomerID.ToString(), ThisCustomer.CustomerID.ToString());
                    using (SqlConnection conn2 = DB.dbConn())
                    {
                        conn2.Open();
                        using (IDataReader rsx = DB.GetRS(sqlx, conn2))
                        {
                            if (rsx.Read())
                            {
                                ExistingShoppingCartRecID = DB.RSFieldInt(rsx, "ShoppingCartRecID");
                            }
                        }
                    }
                    TheyHaveInCartAlready = (ExistingShoppingCartRecID != 0);

                    if (TheyHaveInCartAlready)
                    {
                        DB.ExecuteSQL("update ShoppingCart set Quantity=Quantity+1 where ShoppingCartRecID=" + ExistingShoppingCartRecID.ToString());
                    }
                    else
                    {
                        int GiftShippingAddressID = Customer.GetCustomerPrimaryShippingAddressID(RegistryOwnerCustomerID);
                        String sql = "insert into shoppingcart(ShoppingCartRecGUID,CustomerID,ProductSKU,ProductPrice,ProductWeight,ProductID,VariantID,Quantity,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,ProductDimensions,CartType,TextOption,NextRecurringShipDate,RecurringIndex,OriginalRecurringOrderNumber,BillingAddressID,ShippingAddressID,DistributorID,SubscriptionInterval,SubscriptionIntervalType,Notes,IsUpsell,GiftRegistryForCustomerID,RecurringInterval,RecurringIntervalType,ExtensionData, IsAKit, IsAPack) ";
                        sql += " select " + DB.SQuote(NewGUID) + "," + ThisCustomer.CustomerID.ToString() + ",ProductSKU,ProductPrice,ProductWeight,ProductID,VariantID,Quantity,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,ProductDimensions," + ((int)CartTypeEnum.ShoppingCart).ToString() + ",TextOption,NextRecurringShipDate,RecurringIndex,OriginalRecurringOrderNumber,BillingAddressID," + GiftShippingAddressID.ToString() + ",DistributorID,SubscriptionInterval,SubscriptionIntervalType,Notes,IsUpsell," + RegistryOwnerCustomerID.ToString() + ",RecurringInterval,RecurringIntervalType,ExtensionData, IsAKit, IsAPack";
                        sql += " from ShoppingCart where ShoppingCartRecID=" + MoveID.ToString();
                        DB.ExecuteSQL(sql);

                        // get new ShoppingCartRecID:
                        int NewShoppingCartRecID = 0;
                        using (SqlConnection conn3 = DB.dbConn())
                        {
                            conn3.Open();
                            using (IDataReader rs = DB.GetRS("Select ShoppingCartRecID from ShoppingCart  with (NOLOCK)  where ShoppingCartRecGUID=" + DB.SQuote(NewGUID), conn3))
                            {
                                if (rs.Read())
                                {
                                    NewShoppingCartRecID = DB.RSFieldInt(rs, "ShoppingCartRecID");
                                }
                            }
                        }

                        String sql2 = "insert into kitcart(CustomerID,ShoppingCartRecID,ProductID,VariantID,KitGroupID,KitItemID,Quantity,CartType,OriginalRecurringOrderNumber,ExtensionData, KitGroupTypeID) ";
                        sql2 += " select " + ThisCustomer.CustomerID.ToString() + "," + NewShoppingCartRecID.ToString() + ",ProductID,VariantID,KitGroupID,KitItemID,Quantity," + ((int)CartTypeEnum.ShoppingCart).ToString() + ",OriginalRecurringOrderNumber,ExtensionData, KitGroupTypeID";
                        sql2 += " from kitcart where ShoppingCartRecID=" + MoveID.ToString();
                        DB.ExecuteSQL(sql2);
                    }
                    Response.Redirect("shoppingcart.aspx");
                }
            }
        }
    }
}
