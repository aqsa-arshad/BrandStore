// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Reflection.Emit;
using AspDotNetStorefrontCore;


namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for showcategory.
    /// </summary>
    [PageType("category")]
    public partial class showcategory : SkinBase
    {
        ShowEntityPage m_EP;
        protected string CategoryTypeFlag = string.Empty;
        protected void Page_Load(object sender, System.EventArgs e)
        {            
            m_EP = new ShowEntityPage(EntityDefinitions.readonly_CategoryEntitySpecs, this);
            m_EP.Page_Load(sender, e);

            PayPalAd entityPageAd = new PayPalAd(PayPalAd.TargetPage.Entity);
            ((System.Web.UI.WebControls.Label)Master.FindControl("lblPageHeading")).Text = SEDescription;

            if (entityPageAd.Show)
            {
                ltPayPalAd.Text = entityPageAd.ImageScript;
            }

            litOutput.Text = m_EP.GetOutput();

            CategoryTypeFlag = litOutput.Text.Contains("entity.guidednavigationgrid.xml.config") ? "true" : "false";

            // check if the postback was caused by an addtocart button
            if (IsPostBack && m_EP.IsAddToCartPostBack)
            {
                HandleAddToCart();
                return;
            }
        }

        private void HandleAddToCart()
        {
            // extract the input parameters from the form post
            AddToCartInfo formInput = AddToCartInfo.FromForm(ThisCustomer);

            if (formInput != AddToCartInfo.INVALID_FORM_COMPOSITION)
            {
                bool success = ShoppingCart.AddToCart(ThisCustomer, formInput);
                AppLogic.eventHandler("AddToCart").CallEvent("&AddToCart=true&VariantID=" + formInput.VariantId.ToString() + "&ProductID=" + formInput.ProductId.ToString() + "&ChosenColor=" + formInput.ChosenColor.ToString() + "&ChosenSize=" + formInput.ChosenSize.ToString());
                if (success)
                {
                    bool stayOnThisPage = AppLogic.AppConfig("AddToCartAction").Equals("STAY", StringComparison.InvariantCultureIgnoreCase);
                    if (stayOnThisPage)
                    {
                        // some tokens like the shoppingcart qty may already be rendered
                        // we therefore need to re-display the page to display the correct qty
                        Response.Redirect(this.Request.Url.ToString());
                    }
                    else
                    {
                        string returnUrl = CommonLogic.GetThisPageName(false) + "?" + CommonLogic.ServerVariables("QUERY_STRING");

                        if (formInput.CartType == CartTypeEnum.WishCart)
                        {
                            Response.Redirect(ResolveClientUrl("~/wishlist.aspx?ReturnUrl=" + Security.UrlEncode(returnUrl)));
                        }
                        if (formInput.CartType == CartTypeEnum.GiftRegistryCart)
                        {
                            Response.Redirect(ResolveClientUrl("~/giftregistry.aspx?ReturnUrl=" + Security.UrlEncode(returnUrl)));
                        }
                        // default
                        Response.Redirect(ResolveClientUrl("~/ShoppingCart.aspx?add=true&ReturnUrl=" + Security.UrlEncode(returnUrl)));
                    }
                }
            }

            return;
        }

        public override bool IsEntityPage
        {
            get
            {
                return true;
            }
        }

        public override string EntityType
        {
            get
            {
                return "Category";
            }
        }

        public override int PageID
        {
            get
            {
                return m_EP.GetActiveEntityID;
            }
        }

        protected override string OverrideTemplate()
        {
            var masterHome = AppLogic.HomeTemplate();
            if (masterHome.Trim().Length == 0)
            {
                masterHome = "JeldWenTemplate";
            }
            if (masterHome.EndsWith(".ascx"))
            {
                masterHome = masterHome.Replace(".ascx", ".master");
            }
            if (!masterHome.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
            {
                masterHome = masterHome + ".master";
            }
            if (!CommonLogic.FileExists(CommonLogic.SafeMapPath("~/App_Templates/Skin_" + SkinID + "/" + masterHome)))
            {
                masterHome = "JeldWenTemplate";
            }
            return masterHome;
        }
    }
}
