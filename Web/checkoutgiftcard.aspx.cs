// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.Sql;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
    public partial class checkoutgiftcard : SkinBase
    {
        ShoppingCart cart = null;
        bool ContainsEmailGiftCards = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = -1;
            Response.AddHeader("pragma", "no-cache");

            RequireSecurePage();

            SectionTitle = AppLogic.GetString("checkoutpayment.aspx.1", SkinID, ThisCustomer.LocaleSetting);

            cart = new ShoppingCart(SkinID, ThisCustomer, CartTypeEnum.ShoppingCart, 0, false);

            // -----------------------------------------------------------------------------------------------
            // NOTE ON PAGE LOAD LOGIC:
            // We are checking here for required elements to allowing the customer to stay on this page.
            // Many of these checks may be redundant, and they DO add a bit of overhead in terms of db calls, but ANYTHING really
            // could have changed since the customer was on the last page. Remember, the web is completely stateless. Assume this
            // page was executed by ANYONE at ANYTIME (even someone trying to break the cart). 
            // It could have been yesterday, or 1 second ago, and other customers could have purchased limitied inventory products, 
            // coupons may no longer be valid, etc, etc, etc...
            // -----------------------------------------------------------------------------------------------
            ErrorMessage err;
            if ((!ThisCustomer.IsRegistered && !AppLogic.AppConfigBool("PasswordIsOptionalDuringCheckout")) || cart.HasRecurringComponents())
            {
                Response.Redirect("createaccount.aspx?checkout=true");
            }
            if (ThisCustomer.PrimaryBillingAddressID == 0 || ThisCustomer.PrimaryShippingAddressID == 0)
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("checkoutpayment.aspx.2", SkinID, ThisCustomer.LocaleSetting)));
                Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }


            // re-validate all shipping info, as ANYTHING could have changed since last page:
            if (!cart.ShippingIsAllValid())
            {
                err = new ErrorMessage(Server.HtmlEncode(AppLogic.GetString("shoppingcart.cs.95", ThisCustomer.SkinID, ThisCustomer.LocaleSetting)));
                HttpContext.Current.Response.Redirect("shoppingcart.aspx?resetlinkback=1&errormsg=" + err.MessageId);
            }

            if (!IsPostBack)
            {
                CreateGiftCards(cart);
                InitializePageContent();
                if (!ContainsEmailGiftCards)
                {
                    Response.Redirect("checkoutpayment.aspx");
                }
            }

        }


        private void InitializePageContent()
        {
            JSPopupRoutines.Text = AppLogic.GetJSPopupRoutines();

            pnlError.Visible = lblErrMsg.Text.Length != 0;

            string sql = "select p.name productname, case when isnull(pv.name, '')='' then '' else ' - ' + pv.name end variantname, g.* from GiftCard g join ShoppingCart s on g.ShoppingCartRecID = s.ShoppingCartRecID join product p on s.productid = p.productid join productvariant pv on s.variantid = pv.variantid where g.GiftCardTypeID in (" + AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").TrimEnd(',').TrimStart(',') + ")";
        
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                con.Open();
                using (IDataReader dr = DB.GetRS(sql, con))
                {
                    rptrEmailGiftCards.DataSource = dr;
                    rptrEmailGiftCards.DataBind();
                }
            }

        }

        private void CreateGiftCards(ShoppingCart cart)
        {
            for (int i = 0; i < cart.CartItems.Count; i++)
            {
                CartItem c = (CartItem)cart.CartItems[i];
                string ProdTypeIDs = AppLogic.AppConfig("GiftCard.CertificateProductTypeIDs").TrimEnd(',').TrimStart(',') + "," + AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").TrimEnd(',').TrimStart(',') + "," + AppLogic.AppConfig("GiftCard.PhysicalProductTypeIDs").TrimEnd(',').TrimStart(',');
                string[] ProdTypeIDArray = ProdTypeIDs.Split(',');
                string[] FindProductTypeID = {c.ProductTypeId.ToString()};
                if (ArrayContainsValue(ProdTypeIDArray, FindProductTypeID))
                {
                    //Check the number of certificate records in the GiftCard table for this item.
                    int CardCnt = DB.GetSqlN("select count(*) as N from GiftCard where " +
                                              "ShoppingCartRecID=" + c.ShoppingCartRecordID.ToString());
                    //Add records if not enough
                    if (CardCnt < c.Quantity)
                    {
                        for (int j = 1; j <= c.Quantity - CardCnt; j++)
                        {
                            GiftCard.CreateGiftCard(ThisCustomer.CustomerID, null, null, c.ShoppingCartRecordID, null, null, c.Price, null, c.Price, c.ProductTypeId, null, null, null, null, null, null, null, null, null);
                        }
                    }
                    //Delete records if there are too many. Delete from the end of the list just because we have to delete something.
                    if (CardCnt > c.Quantity)
                    {
                        int DeleteCnt = CardCnt - c.Quantity;
                        string sql = "delete from GiftCard where GiftCardID in (select TOP " + DeleteCnt.ToString() + " GiftCardID from GiftCard where ShoppingCartRecID=" + c.ShoppingCartRecordID.ToString() + " order by GiftCardID DESC)";
                        DB.ExecuteSQL(sql);
                    }

                    //GiftCard.DeleteGiftCardsInCart(c.m_ShoppingCartRecordID);
                }

                if (ArrayContainsValue(AppLogic.AppConfig("GiftCard.EmailProductTypeIDs").Split(','), FindProductTypeID)) ContainsEmailGiftCards = true;
            }
        }

        protected void btnContinue_Click(object sender, EventArgs e)
        {
            int completed = 0;
            string retval = string.Empty;
            foreach (RepeaterItem ri in rptrEmailGiftCards.Items)
            {
                string recipientname = ((TextBox)ri.FindControl("EmailName")).Text;
                string emailto = ((TextBox)ri.FindControl("EmailTo")).Text;
                string emailmsg = ((TextBox)ri.FindControl("EmailMessage")).Text;
                string giftcardid = ((TextBox)ri.FindControl("giftcardid")).Text;

                retval = GiftCard.UpdateCard(Int32.Parse(giftcardid), null, null, null, null, null, null, recipientname, emailto, emailmsg, null, null, null, null, null, null);

                if (recipientname.Trim().Length > 0 && emailto.Trim().Length > 0 && retval == "")
                {
                    completed++;
                }
                retval = string.Empty;
            }

            if (completed == rptrEmailGiftCards.Items.Count)
            {
                Response.Redirect("checkoutpayment.aspx?checkout=true");
            }
            else
            {
                lblErrMsg.Text = "Blank address fields! All fields must be completed to insure proper delivery";
                InitializePageContent();
            }

        }

        bool ArrayContainsValue(string[] Source, string[] FindItems)
        {
            for (int i = 0; i < FindItems.Length; i++)
            {
                for (int j = 0; j < Source.Length; j++)
                {
                    if (Source[j] == FindItems[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
