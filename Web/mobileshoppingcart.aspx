<%@ Page Language="c#" Inherits="AspDotNetStorefront.MobileShoppingCartPage" CodeFile="mobileshoppingcart.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master"  %>

<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
        <ul id="MPShoppingCartUL" data-role="listview">
            <li class="group" data-role="list-divider">
                <asp:Panel ID="pnlCouponError" runat="Server" Visible="false">
                    <p>
                        <asp:Label ID="CouponError" CssClass="errorLg" runat="Server"></asp:Label></p>
                </asp:Panel>
                <asp:Panel ID="pnlErrorMsg" runat="Server" Visible="false">
                    <p>
                        <asp:Label ID="ErrorMsgLabel" CssClass="errorLg" runat="Server"></asp:Label></p>
                </asp:Panel>
                <asp:Panel ID="pnlInventoryTrimmedError" runat="Server" Visible="false">
                    <p>
                        <asp:Label ID="InventoryTrimmedError" CssClass="errorLg" runat="Server"></asp:Label></p>
                </asp:Panel>
                <asp:Panel ID="pnlRecurringScheduleConflictError" runat="Server" Visible="false">
                    <p>
                        <asp:Label ID="RecurringScheduleConflictError" CssClass="errorLg" runat="Server"></asp:Label></p>
                </asp:Panel>
                <asp:Panel ID="pnlMinimumQuantitiesUpdatedError" runat="Server" Visible="false">
                    <p>
                        <asp:Label ID="MinimumQuantitiesUpdatedError" CssClass="errorLg" runat="Server"></asp:Label></p>
                </asp:Panel>
                <asp:Panel ID="pnlMeetsMinimumOrderAmountError" runat="Server" Visible="false">
                    <p>
                        <asp:Label ID="MeetsMinimumOrderAmountError" CssClass="errorLg" runat="Server"></asp:Label></p>
                </asp:Panel>
                <asp:Panel ID="pnlMeetsMinimumOrderQuantityError" runat="Server" Visible="false">
                    <p>
                        <asp:Label ID="MeetsMinimumOrderQuantityError" CssClass="errorLg" runat="Server"></asp:Label></p>
                </asp:Panel>
                <asp:Panel ID="pnlExceedsMaximumOrderQuantityError" runat="Server" Visible="false">
                    <p>
                        <asp:Label ID="ExceedsMaximumOrderQuantityError" CssClass="errorLg" runat="Server"></asp:Label></p>
                </asp:Panel>
                <asp:Panel ID="pnlMicropay_EnabledError" runat="Server" Visible="false">
                    <asp:Literal ID="Micropay_EnabledError" runat="Server"></asp:Literal>
                </asp:Panel>
            </li>
            <li class="mpContinueCheckoutRow">
                        <asp:Button ID="btnContinueShoppingTop" data-icon="arrow-l" Text="<%$ Tokens:StringResource,shoppingcart.cs.62 %>" CssClass="ContinueShoppingButton leftbutton" data-inline="true" runat="server" />
                        <asp:Button ID="btnCheckOutNowTop" Text="<%$ Tokens:StringResource,shoppingcart.cs.111 %>" runat="server" CssClass="CheckoutNowButton rightbutton action" data-icon="check" data-iconpos="right" />
            </li>            
            
            <li runat="server" id="AlternativeCheckouts" class="mpAltCheckout">
                <span runat="server" id="PayPalExpressSpan" class="mpPayPalExpressSpan">
                    <asp:ImageButton ImageAlign="Top" ID="btnPayPalExpressCheckout" runat="server" OnCommand="btnPayPalExpressCheckout_Click" CommandArgument="ppec" CssClass="mpBtnPayPalExpressCheckout" />
					<asp:ImageButton ImageAlign="Top" ID="btnPayPalBillMeLaterCheckout" runat="server" OnCommand="btnPayPalExpressCheckout_Click" CommandArgument="ppbml" CssClass="mpBtnPayPalExpressCheckout" />
                </span>
                <div class="clear" ></div>
            </li>
            <li id="MPOrderSummary">
                <asp:Label ID="shoppingcartcs96" runat="server" Font-Bold="true" ></asp:Label>
                <%--<asp:Label ID="CartSubTotal" runat="server"></asp:Label>--%>
                <b>
                <aspdnsf:CartSummary ID="ctrlCartSummary" runat="server"
                             CalculateShippingDuringCheckout="true" 
                             CalculateTaxDuringCheckout="true"
                             CalculateShippingDuringCheckoutText='<%$Tokens:StringResource, shoppingcart.aspx.13 %>'
                             CalculateTaxDuringCheckoutText='<%$Tokens:StringResource, shoppingcart.aspx.15 %>'
                             SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>'
                             SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
                             ShippingCaption='<%$Tokens:StringResource, shoppingcart.aspx.12 %>'                             
                             SkipShippingOnCheckout='<%$Tokens:AppConfigBool, SkipShippingOnCheckout %>'                             
                             TaxCaption='<%$Tokens:StringResource, shoppingcart.aspx.14 %>'
                             LineItemDiscountCaption ="<%$Tokens:StringResource, shoppingcart.cs.200 %>" OrderDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.201 %>"
                             ShowTotal="False"
                             ShowTax="false"
                             ShowShipping="false"
                            /></b>
            </li>
            <li class="group" data-role="list-divider"></li>
            <li>
                <asp:Panel ID="pnlCartSummary" runat="server" HorizontalAlign="right" DefaultButton="btnUpdateCart1">
                    <aspdnsfc:ShoppingCartControl ID="ctrlShoppingCart" runat="server" 
                             AllowEdit="true"
                             ProductHeaderText='<%$ Tokens:StringResource, shoppingcart.product %>'
                             QuantityHeaderText='<%$ Tokens:StringResource, shoppingcart.quantity %>'
                             SubTotalHeaderText='<%$ Tokens:StringResource, shoppingcart.subtotal %>' 
                                onitemdeleting="ctrlShoppingCart_ItemDeleting" >
                                 <LineItemSettings LinkToProductPageInCart='<%$ Tokens:AppConfigBool, LinkToProductPageInCart %>' 
                                    ImageLabelText='<%$ Tokens:StringResource, shoppingcart.cs.1000 %>'
                                    SKUCaption='<%$ Tokens:StringResource, showproduct.aspx.21 %>'  
                                    GiftRegistryCaption='<%$ Tokens:StringResource, shoppingcart.cs.92 %>'
                                    ItemNotesCaption='<%$ Tokens:StringResource, shoppingcart.cs.86 %>'
                                    ItemNotesColumns='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaCols %>'
                                    ItemNotesRows='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaRows %>'
                                    AllowShoppingCartItemNotes='<%$ Tokens:AppConfigBool, AllowShoppingCartItemNotes %>'                                     
                                    ShowPicsInCart='<%$ Tokens:AppConfigBool, ShowPicsInCart %>' 
                                    ShowEditButtonInCartForKitProducts='<%$ Tokens:AppConfigBool, ShowEditButtonInCartForKitProducts %>' 
                                    ShowMultiShipAddressUnderItemDescription="true" 
                                  />
                            </aspdnsfc:ShoppingCartControl>
                    <div style="text-align:right;">
                        <asp:Button ID="btnUpdateCart1" CssClass="rightbutton updatecart" data-inline="true" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>" runat="server" data-icon="refresh" data-iconpos="notext" />
                    </div>
                    <div class="clear" ></div>
                </asp:Panel>
            </li>
            <li class="group" data-role="list-divider"></li>
        </ul>
        <div data-role="collapsible-set">
            <asp:PlaceHolder ID="pnlCoupon" runat="server" Visible="false">
	            <div data-role="collapsible">
	                <h3>
                        <asp:Literal ID="Literal1" Text="<%$ Tokens:StringResource,Mobile.ShoppingCart.Coupon %>" runat="server"/>
                    </h3>
                    <div class="accordion_child">
						<table cellpadding="4" cellspacing="0" border="0" style="width:100%;">
                            <tr>
                                <td align="left" valign="top">
									<asp:Label ID="shoppingcartcs31" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.117 %>" Font-Bold="true" />&#160;
									<asp:TextBox ID="CouponCode" Columns="30" MaxLength="50" runat="server" />
                                </td>
                                <td>
                                    <asp:LinkButton ID="btnRemoveCoupon" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.aspx.removecoupon %>" Visible="false" OnClick="btnRemoveCoupon_Click" />
                                </td>
                                <td style="text-align:right; vertical-align: bottom;">
                                    <asp:Button ID="btnUpdateCart3" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>" CssClass="rightbutton updatecart" data-inline="true" data-icon="refresh" data-iconpos="notext" />
                                </td>
                            </tr>
                            </table>
                        </div>
                    </div>
                </asp:PlaceHolder>
            <asp:PlaceHolder ID="pnlPromotion" runat="server" Visible="false">
                <div data-role="collapsible">
                    <h3>
                        <asp:Literal ID="Literal4" Text="<%$ Tokens:StringResource,Mobile.ShoppingCart.Promotion %>" runat="server"/>
                    </h3>
                    <div class="accordion_child">
                        <table cellpadding="4" cellspacing="0" border="0" style="width:100%;">
                            <tr>
                                <td align="left" valign="top" class="mobilepromotioncodeentrycell">
									<asp:Panel runat="server" DefaultButton="btnAddPromotion">
										<span class="mobilepromotioncodeentrydescription">
											<asp:Label ID="shoppingcartcs118" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.118 %>"></asp:Label>&#160;
										</span>
										<asp:TextBox ID="txtPromotionCode" Columns="30" MaxLength="50" runat="server" />
										<asp:RequiredFieldValidator runat="server" ControlToValidate="txtPromotionCode" ValidationGroup="AddPromotion" ErrorMessage="*" Display="Dynamic" />
									</asp:Panel>
                                </td>
                                <td></td>
                                <td align="left" valign="top" class="mobilepromotioncodeentrybutton">
                                    <asp:Button ID="btnAddPromotion" runat="server" Text="Add" OnClick="btnAddPromotion_Click" ValidationGroup="AddPromotion" CssClass="rightbutton updatecart" data-inline="true" data-icon="refresh" data-iconpos="notext" />
                                </td>
                            </tr>
                            <tr>
                                <td align="left" valign="top" colspan="3">
                                    <asp:Label ID="lblPromotionError" runat="Server" CssClass="mobilepromotionerror" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="3" align="left" valign="top" class="promotionlist">
                                    <asp:Repeater ID="repeatPromotions" runat="server" OnItemDataBound="repeatPromotions_ItemDataBound" OnItemCommand="repeatPromotions_ItemCommand">
                                        <ItemTemplate>
                                            <div class="promotionlistitem">
                                                <span class="promotionlistitemcode">
                                                    <%# Eval("Code") %>
                                                </span>
                                                <span class="promotionlistitemdescription">
                                                    <%# Eval("Description") %>
                                                </span>
                                                <span class="promotioncodeentrylink">
                                                    <asp:LinkButton ID="lnkRemovePromotion" runat="server" CommandArgument='<%# Eval("Code") %>' CommandName="RemovePromotion" Text="Remove" />
                                                </span>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </td>
                            </tr>
                        </table>
					</div>
				</div>
            </asp:PlaceHolder>
	
            <asp:PlaceHolder ID="pnlOrderOptions" runat="server" Visible="false">
	            <div data-role="collapsible">
	                <h3><asp:Literal ID="Literal2" Text="<%$ Tokens:StringResource,Mobile.ShoppingCart.Options %>" runat="server"/></h3>
	                <div id="MPOrderOptions3-content" class="clear">
                        <div class="accordion_child">
                            <table style="width:100%;">
                                <tr>
                                    <td>
                                        <aspdnsf:OrderOption id="ctrlOrderOption" runat="server" EditMode="true" />
                                    </td>
                                    <td style="text-align:right;">
                                        <asp:Button ID="btnUpdateCart2" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>" CssClass="rightbutton updatecart" data-inline="true" data-icon="refresh" data-iconpos="notext" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
	            </div>
            </asp:PlaceHolder>
	
            <asp:PlaceHolder ID="pnlOrderNotes" runat="server" Visible="false">
	            <div data-role="collapsible">
	                <h3><asp:Literal ID="Literal3" Text="<%$ Tokens:StringResource,Mobile.ShoppingCart.Notes %>" runat="server"/></h3>
	                <div id="MPOrderOptions4-content" class="clear">
                        <div class="accordion_child">
                            <table width="100%" cellpadding="4" cellspacing="0" border="0">
                                <tr>
                                    <td align="left" valign="top">
                                        <asp:Label ID="lblOrderNotes" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.66 %>" Font-Bold="true"></asp:Label>
                                        <asp:TextBox ID="OrderNotes" Style="width:95%;" Rows="4" TextMode="MultiLine" runat="server"></asp:TextBox>
                                    </td>
                                    <td style="text-align:right;">
                                        <asp:Button ID="btnUpdateCart4" runat="server" Text="<%$ Tokens:StringResource,shoppingcart.cs.110 %>"  CssClass="rightbutton updatecart" data-inline="true" data-icon="refresh" data-iconpos="notext"/>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
	            </div>
            </asp:PlaceHolder>
        </div>
        <asp:Literal ID="ValidationScript" runat="server"></asp:Literal>
        <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>
	    <style type="text/css">
		    #pnlCartSummary table tbody tr td a img
		    {
			    max-width:75px;
		    }
		    #ctl00_PageContent_ctrlShoppingCart_imgShoppingCartTab
		    {
		        display:none;
		    }
		    .shopping_cart *
		    {
		        text-align:left;
		    }
	    </style>
        <ul data-role="listview">
            <li class="mpContinueCheckoutRow">
                <asp:Button ID="btnCheckOutNowBottom" Text="<%$ Tokens:StringResource,shoppingcart.cs.111 %>" runat="server" CssClass="CheckoutNowButton rightbutton action" data-icon="check" data-iconpos="right" />
                <asp:Button ID="btnContinueShoppingBottom" data-icon="arrow-l" Text="<%$ Tokens:StringResource,shoppingcart.cs.62 %>" CssClass="ContinueShoppingButton leftbutton" runat="server" data-inline="true" />
            </li>
            <li runat="server" id="AlternativeCheckouts2" class="mpAltCheckout">
                <span runat="server" id="PayPalExpressSpan2" class="mpPayPalExpressSpan">
                    <asp:ImageButton ImageAlign="Top" ID="btnPayPalExpressCheckout2" CssClass="mpBtnPayPalExpressCheckout" runat="server" OnCommand="btnPayPalExpressCheckout_Click" CommandArgument="ppec" />
					<asp:ImageButton ImageAlign="Top" ID="btnPayPalBillMeLaterCheckout2" runat="server" CssClass="mpBtnPayPalExpressCheckout" OnCommand="btnPayPalExpressCheckout_Click" CommandArgument="ppbml"  />
				</span>
                <div class="clear" ></div>
            </li>

            <li class="group" data-role="list-divider"></li>
            <asp:PlaceHolder ID="ShippingInformation" runat="server">
                <li>
                    <a href="t-shipping.aspx"><asp:Literal ID="shoppingcartaspx8" runat="server"></asp:Literal></a>
                </li>
            </asp:PlaceHolder>
            <li>
                <a href="t-returns.aspx"><asp:Literal ID="shoppingcartaspx9" Text="<%$ Tokens:StringResource,shoppingcart.aspx.9 %>" runat="server"></asp:Literal></a>
            </li>
            <li>
                <a href="t-privacy.aspx"><asp:Literal ID="shoppingcartaspx10" Text="<%$ Tokens:StringResource,shoppingcart.aspx.10 %>" runat="server"></asp:Literal></a>
            </li>
            <asp:PlaceHolder ID="AddresBookLlink" runat="server">
                <li>
                    <a href="mobileaddress.aspx?returnurl=shoppingcart.aspx&AddressType=Billing&checkout=true"><asp:Literal Text="<%$ Tokens:StringResource,Mobile.ShoppingCart.EditBillingAddresses %>" runat="server"></asp:Literal></a>
                </li>
                <li>
                    <a href="mobileaddress.aspx?returnurl=shoppingcart.aspx&AddressType=Shipping&checkout=true"><asp:Literal Text="<%$ Tokens:StringResource,Mobile.ShoppingCart.EditShippingAddresses %>" runat="server"></asp:Literal></a>
                </li>
            </asp:PlaceHolder>
        </ul>
        <script type="text/javascript">

            $(document).ready(function () {
                var themeType = '<%= AspDotNetStorefrontCore.AppLogic.AppConfig("Mobile.Action.ThemeId") %>';
                var divClasses = 'ui-btn ui-btn-corner-all ui-shadow ui-btn-up-c ui-btn-up-' + themeType;
                $('.mpPayPalExpressSpan div').removeClass(divClasses);
                $('.mpPayPalExpressSpan span').removeClass('ui-btn-inner ui-btn-corner-all');
                $('.mpBtnPayPalExpressCheckout').removeClass('ui-btn-hidden');
            });
        </script>
</asp:Content>
