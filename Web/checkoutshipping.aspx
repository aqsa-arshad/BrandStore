<%@ Page Language="c#" Inherits="AspDotNetStorefront.checkoutshipping" CodeFile="checkoutshipping.aspx.cs" MasterPageFile="~/App_Templates/skin_3/JeldWenTemplate.master" %>

<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsfc" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="XmlPackage" Src="~/Controls/XmlPackageControl.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="OrderOption" Src="~/controls/OrderOption.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="BuySafeKicker" Src="~/controls/BuySafeKicker.ascx" %>

<%@ Register Src="controls/CheckoutSteps.ascx" TagName="CheckoutSteps" TagPrefix="checkout" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Label ID="hdnRemainingFundValue" name="hdnRemainingFundValue" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnShipmentChargesPaid" name="hdnShipmentChargesPaid" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnShippingMethod" name="hdnShippingMethod" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />

    <asp:Panel ID="pnlContent" runat="server">
        <checkout:CheckoutSteps ID="CheckoutSteps" runat="server" />
        <div class="content-box-03">

            <%-- <h1>
                <asp:Literal ID="ltShippingOptions" Text="<%$ Tokens:StringResource,Header.ShippingOptions %>" runat="server" />
            </h1>--%>
            <asp:Literal ID="CheckoutValidationScript" runat="server" Mode="PassThrough"></asp:Literal>
            <asp:Literal ID="JSPopupRoutines" runat="server"></asp:Literal>

            <asp:Panel ID="pnlErrorMsg" runat="server" Visible="false">
                <div class="error-wrap">
                    <asp:Label ID="ErrorMsgLabel" CssClass="error-large" runat="server"></asp:Label>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlCBAAddressWidget" runat="server" Visible="false">
                <div class="page-row row-cba-address">
                    <asp:Literal ID="litCBAAddressWidget" runat="server" />
                </div>
                <div class="page-row row-cba-instructions">
                    <asp:Literal ID="litCBAAddressWidgetInstruction" runat="server" />
                </div>
            </asp:Panel>


            <asp:Panel runat="server" ID="pnlSelectShipping">

                <div class="page-row row-shipping-address">
                    <div class="form login-form">
                        <div class="group-header form-header signin-header">
                            <asp:Label ID="lblChooseShippingAddr" Text="<%$ Tokens:StringResource,checkoutshipping.aspx.20 %>" Font-Bold="true" runat="server"></asp:Label>
                        </div>
                        <div class="form-group">
                            <asp:DropDownList ID="ddlChooseShippingAddr" CssClass="form-control" runat="server" OnSelectedIndexChanged="ddlChooseShippingAddr_SelectedIndexChanged"></asp:DropDownList>
                        </div>
                        <asp:Panel runat="server" ID="pnlNewShipAddr" Visible="false">
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx55" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.55 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingFirstName" CssClass="form-control" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="shipping1" />
                                <asp:RequiredFieldValidator ID="valReqShipFName" ErrorMessage="<%$ Tokens:StringResource,address.cs.13 %>" ControlToValidate="ShippingFirstName" EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx57" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.57 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingLastName" CssClass="form-control" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="shipping1" />
                                <asp:RequiredFieldValidator ID="valReqShipLName" ErrorMessage="<%$ Tokens:StringResource,address.cs.14 %>" ControlToValidate="ShippingLastName" EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx59" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.59 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingPhone" CssClass="form-control" MaxLength="25" runat="server" CausesValidation="true" ValidationGroup="shipping1" />
                                <asp:RequiredFieldValidator ID="valReqShipPhone" ErrorMessage="<%$ Tokens:StringResource,address.cs.15 %>" ControlToValidate="ShippingPhone" EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx62" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.62 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingCompany" CssClass="form-control" MaxLength="100" runat="server" />
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="addresscs58_2" runat="server" Text="<%$ Tokens:StringResource,address.cs.58 %>"></asp:Literal></label>
                                <asp:DropDownList ID="ShippingResidenceType" CssClass="form-control" runat="server"></asp:DropDownList>
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx63" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.63 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingAddress1" CssClass="form-control" MaxLength="100" runat="server" CausesValidation="true" ValidationGroup="shipping1" />
                                <asp:RequiredFieldValidator ID="valReqShipAddr1" ErrorMessage="<%$ Tokens:StringResource,address.cs.16 %>" ControlToValidate="ShippingAddress1" EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx65" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.65 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingAddress2" CssClass="form-control" MaxLength="100" runat="server" />
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx66" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.66 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingSuite" CssClass="form-control" MaxLength="50" runat="server" />
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx67" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.67 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingCity" CssClass="form-control" MaxLength="50" runat="server" CausesValidation="true" ValidationGroup="shipping1" />
                                <asp:RequiredFieldValidator ID="valReqShipCity" ErrorMessage="<%$ Tokens:StringResource,address.cs.17 %>" ControlToValidate="ShippingCity" EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx73" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.73 %>"></asp:Literal></label>
                                <asp:DropDownList ID="ShippingCountry" CssClass="form-control" runat="server" OnSelectedIndexChanged="ShippingCountry_Change" AutoPostBack="True"></asp:DropDownList>
                                <asp:RequiredFieldValidator ID="valReqShipCountry" ErrorMessage="<%$ Tokens:StringResource,createaccount.aspx.11 %>" ControlToValidate="ShippingCountry" EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx69" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.69 %>"></asp:Literal></label>
                                <asp:DropDownList ID="ShippingState" CssClass="form-control" runat="server"></asp:DropDownList>
                                <asp:RequiredFieldValidator ID="valReqShipState" ErrorMessage="<%$ Tokens:StringResource,address.cs.1 %>" ControlToValidate="ShippingState" EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                            </div>
                            <div class="form-group">
                                <label>
                                    <asp:Literal ID="Checkout1aspx70" runat="server" Text="<%$ Tokens:StringResource,createaccount.aspx.70 %>"></asp:Literal></label>
                                <asp:TextBox ID="ShippingZip" CssClass="form-control" MaxLength="10" runat="server" CausesValidation="true" ValidationGroup="shipping1" />
                                <asp:RequiredFieldValidator ID="valReqShipZip" ErrorMessage="<%$ Tokens:StringResource,address.cs.18 %>" ControlToValidate="ShippingZip" EnableClientScript="false" runat="server" ValidationGroup="shipping1" Display="None"></asp:RequiredFieldValidator>
                            </div>
                            <div class="form-submit-wrap">
                                <asp:Button ID="btnNewShipAddr" runat="server" Text="<%$ Tokens:StringResource,checkoutshipping.aspx.21 %>" CssClass="button button-new-address" Visible="true" ValidationGroup="shipping1" OnClick="btnNewShipAddr_OnClick" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </asp:Panel>

            <aspdnsf:Topic runat="server" ID="HeaderMsg" TopicName="CheckoutShippingPageHeader" />
            <asp:Literal ID="XmlPackage_CheckoutShippingPageHeader" runat="server" Mode="PassThrough"></asp:Literal>

            <asp:Panel ID="pnlGetFreeShippingMsg" CssClass="FreeShippingThresholdPrompt" Visible="false" runat="server">
                <asp:Literal ID="GetFreeShippingMsg" runat="server" Mode="PassThrough"></asp:Literal>
            </asp:Panel>

            <asp:Panel ID="pnlFreeShippingMsg" CssClass="FreeShippingThresholdPrompt" runat="server">
                <asp:Label ID="FreeShippingMsg" Visible="false" runat="server"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlCartAllowsShippingMethodSelection" runat="server" CssClass="row">
                <div class="col-md-12 checkout-shipping">
                    <asp:Label ID="ShipSelectionMsg" runat="server"></asp:Label>
                    <asp:Label ID="lblMultiShipPrompt" runat="server" Visible="false" />
                    <h4 class="black-color margin-top-none">
                        <label id="Label1" runat="server" class="black-color margin-top-none">Select Shipping Method</label></h4>
                    <asp:Label ID="lblshippingmethodnote" runat="server" Text="<%$Tokens:StringResource, shippingmethodnote%>" />
                    <aspdnsfc:ShippingMethodControl ID="ctrlShippingMethods" runat="server" />

                    <div class="page-row row-buysafe hide-element">
                        <aspdnsf:BuySafeKicker ID="buySAFEKicker" WrapperClass="shippingKicker" runat="server" />
                    </div>
                </div>

                <div class="clearfix"></div>
                <div class="col-md-12 checkout-shipping">
                    <table id="ctl00_PageContent_ctrlShippingMethods_ctrlShippingMethods" border="0">
                        <tbody>
                            <tr>
                                <td width="20px">
                                    <input type="checkbox" clientidmode="Static" id="cbBluBucks" runat="server" visible="False" autopostback="false" />

                                    <input type="checkbox" clientidmode="Static" id="cbSOF" runat="server" visible="False" autopostback="false" />
                                </td>
                                <td>
                                    <asp:Label runat="server" ID="lblBluBucks" Text="Use your BLU Bucks™ to pay for shipping costs" Visible="False" ClientIDMode="Static"></asp:Label>
                                    <asp:Label runat="server" ID="lblSOF" Text="Use your Sales Funds to pay for shipping costs" Visible="False" ClientIDMode="Static"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="padding-none" width="20px"></td>
                                <td class="padding-none">
                                    <asp:Label runat="server" ID="lblRemainingSOFCaption" Text="Sales Funds remaining: " Visible="False" ClientIDMode="Static"></asp:Label>
                                    <asp:Label runat="server" ID="lblRemainingSOF" Visible="False" ClientIDMode="Static"></asp:Label>
                                    <asp:Label runat="server" ID="lblRemainingBluBucksCaption" Text="BLU BUCKS remaining: " Visible="False" ClientIDMode="Static"></asp:Label>
                                    <asp:Label runat="server" ID="lblRemainingBluBucks" Visible="False" ClientIDMode="Static"></asp:Label>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="col-md-12">
                    <div class="checkout-buttons">
                        <asp:Button type="button" ID="btnback" class="btn btn-primary btn-block btn-success" runat="server" OnClick="btnback_Click" Text="<< Back"></asp:Button>
                    </div>

                    <div class="checkout-buttons pull-right pull-sm-no">
                        <asp:Button ID="btnContinueCheckout" runat="server" 
                            Text="<%$ Tokens:StringResource,checkoutshipping.aspx.13 %>"
                            CssClass="btn btn-primary btn-block" Visible="false"
                            OnClientClick="saveShipmentChargesPaid()" OnClick="btnContinueCheckout_Click"/>
                    </div>
                </div>
                <div class="clearfix"></div>
            </asp:Panel>

            <%--  <asp:Button ID="btnContinueCheckout" runat="server"
                    Text="<%$ Tokens:StringResource,checkoutshipping.aspx.13 %>"
                    CssClass="btn btn-primary" Visible="false"
                    OnClick="btnContinueCheckout_Click" />--%>


            <div class="group-header checkout-header" style="display: none">
                <asp:Literal ID="Literal1" Text="<%$ Tokens:StringResource,Header.ShoppingCart %>" runat="server" />
            </div>
            <div class="page-row row-shopping-cart" style="display: none">

                <%--Shopping cart control--%>
                <aspdnsfc:ShoppingCartControl ID="ctrlShoppingCart"
                    ProductHeaderText='<%$ Tokens:StringResource, shoppingcart.product %>'
                    QuantityHeaderText='<%$ Tokens:StringResource, shoppingcart.quantity %>'
                    SubTotalHeaderText='<%$ Tokens:StringResource, shoppingcart.subtotal %>'
                    runat="server" AllowEdit="false">
                    <LineItemSettings
                        LinkToProductPageInCart='<%$ Tokens:AppConfigBool, LinkToProductPageInCart %>'
                        SKUCaption='<%$ Tokens:StringResource, showproduct.aspx.21 %>'
                        GiftRegistryCaption='<%$ Tokens:StringResource, shoppingcart.cs.92 %>'
                        ItemNotesCaption='<%$ Tokens:StringResource, shoppingcart.cs.86 %>'
                        ItemNotesColumns='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaCols %>'
                        ItemNotesRows='<%$ Tokens:AppConfigUSInt, ShoppingCartItemNotesTextareaRows %>'
                        AllowShoppingCartItemNotes="false"
                        ShowPicsInCart="true" />
                </aspdnsfc:ShoppingCartControl>

                <asp:Literal ID="CartSummary" runat="server"></asp:Literal>


                <aspdnsf:OrderOption ID="ctrlOrderOption" runat="server" EditMode="false" />

                <%--Total Summary--%>
                <aspdnsf:CartSummary ID="ctrlCartSummary" runat="server" ShowShipping="False" ShowTax="false"
                    ShowTotal="false"
                    SubTotalCaption='<%$Tokens:StringResource, shoppingcart.cs.96 %>'
                    SubTotalWithDiscountCaption='<%$Tokens:StringResource, shoppingcart.cs.97 %>'
                    IncludeTaxInSubtotal="false"
                    LineItemDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.200 %>" OrderDiscountCaption="<%$Tokens:StringResource, shoppingcart.cs.201 %>" />

                <aspdnsf:Topic runat="server" ID="CheckoutShippingPageFooter" TopicName="CheckoutShippingPageFooter" />
                <asp:Literal ID="XmlPackage_CheckoutShippingPageFooter" runat="server" Mode="PassThrough"></asp:Literal>

            </div>
            <asp:Panel runat="server">
                <div>
                    <asp:Literal ID="DebugInfo" runat="server" Mode="PassThrough" />
                </div>
            </asp:Panel>

            <asp:Literal ID="ltPayPalIntegratedCheckout" runat="server" />
        </div>

    </asp:Panel>

    <script>
        var previouslySelectedShippingMethodAmount = 0;
        var availableFundValue = 0;
        var isCheckBoxSelected = 0;
        $(document).ready(function () {
            debugger;
            if ('<%=isBluBucks%>' == 'True') {
                CalculateShippingCost('cbBluBucks');
            }
            else {
                CalculateShippingCost("cbSOF");
            }

            $('[id*=ctrlShippingMethods] input').unbind().click(function (e) {
                debugger;
                availableFundValue = (parseFloat($("#hdnRemainingFundValue").text()) + previouslySelectedShippingMethodAmount).toFixed(2);
                $("#hdnRemainingFundValue").text((parseFloat($("#hdnRemainingFundValue").text()) + previouslySelectedShippingMethodAmount).toFixed(2));
                if ('<%=isBluBucks%>' == 'True') {
                    CalculateShippingCost('cbBluBucks');
                }
                else {
                    CalculateShippingCost("cbSOF");
                }
            });
        });

        function CalculateShippingCost(checkBoxName) {
            debugger;
            checkBoxName = "#" + checkBoxName;            
            if ($(checkBoxName).is(':checked')) {
                isCheckBoxSelected = -1;
                var lstShippingMethod = $.parseJSON($("#hdnShippingMethod").text());
                var shippingVal = $('[id*=ctrlShippingMethods]').find('input:checked').val();
                var shippingMethodID = shippingVal.split('|')[0];
                for (var i = 0; i < lstShippingMethod.length; i++) {
                    var shippingMethod = lstShippingMethod[i];
                    if (shippingMethod.Id == shippingMethodID) {
                        previouslySelectedShippingMethodAmount = shippingMethod.Freight;
                        if (shippingMethod.Freight <= availableFundValue) {
                            availableFundValue = availableFundValue - shippingMethod.Freight;
                            $('#hdnShipmentChargesPaid').text(shippingMethod.Freight.toFixed(2));

                            if ('<%=isBluBucks%>' == 'True') {
                                $('#lblRemainingBluBucks').text(availableFundValue.toFixed(2));
                            } else {
                                $('#lblRemainingSOF').text(availableFundValue.toFixed(2));
                            }
                        } else {
                            $('#hdnShipmentChargesPaid').text(availableFundValue);
                            availableFundValue = availableFundValue - shippingMethod.Freight;
                            if ('<%=isBluBucks%>' == 'True') {
                                $('#lblRemainingBluBucks').text("0.00");
                            } else {
                                $('#lblRemainingSOF').text("0.00");
                            }
                        }
                        $('#hdnRemainingFundValue').text(availableFundValue.toFixed(2));
                    }
                }
            } else {
                if (isCheckBoxSelected == -1) {
                    $('#hdnShipmentChargesPaid').text(0);
                    previouslySelectedShippingMethodAmount = 0;
                    if ('<%=isBluBucks%>' == 'True') {
                        $('#lblRemainingBluBucks').text(availableFundValue);
                    } else {
                        $('#lblRemainingSOF').text(availableFundValue);
                    }
                }
            }
        }

        function saveShipmentChargesPaid() {
            $.ajax({
                type: "post",
                url: "checkoutshipping.aspx/SaveShipmentChargesPaid",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    "shipmentCharges": $('#hdnShipmentChargesPaid').text()                    
                }),
                dataType: "json",
                async: false,
                success: function (result) {                    
                },
                error: function (result) {                    
                },
            });
        }

    </script>
</asp:Content>
