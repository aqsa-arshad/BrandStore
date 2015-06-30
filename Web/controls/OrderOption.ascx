<%@ Control ClassName="OrderOptionControl" Language="C#" AutoEventWireup="true" CodeFile="OrderOption.ascx.cs" Inherits="AspDotNetStorefront.OrderOptionControl" %>
<%@ Register TagPrefix="aspdnsfc" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>


<div class="cart-row cart-header">
    <div class="cart-column cart-column-option-select">
        <asp:Label ID="lblNameHeader" CssClass="OrderOptionsRowHeader" Text="<%$ Tokens:StringResource,shoppingcart.cs.27 %>" runat="server"></asp:Label>
    </div>
    <div class="cart-column cart-column-option-price">
        <asp:Label ID="lblCostHeader" CssClass="OrderOptionsRowHeader" Text="<%$ Tokens:StringResource,shoppingcart.cs.28 %>"
            runat="server"></asp:Label>
    </div>
</div>

<asp:Repeater ID="rptOrderOptions" runat="server"
    OnItemDataBound="rptOrderOptions_ItemDataBound">
    <ItemTemplate>
        <div class="cart-row">
            <div class="cart-column cart-column-option-select">

                <%--shown as check image on checkout pages other than the shoppingcart--%>
                <asp:Image ID="imgSelected" runat="server"
                    ImageUrl='<%$ Tokens:SkinImage, selected.gif %>'
                    Visible="<%# EditMode == false %>" />

                <aspdnsfc:DataCheckBox ID="chkSelected" runat="server"
                    Data='<%# Container.DataItemAs<OrderOption>().UniqueID %>'
                    Checked="<%# DetermineSelected(Container.DataItemAs<OrderOption>()) %>"
                    Visible="<%# EditMode == true  %>" />
                <asp:Image ID="imgOption" runat="server" Visible="false" />
                <asp:Label ID="OrderOptionName" CssClass="OrderOptionsName" runat="server" Text='<%# Container.DataItemAs<OrderOption>().Name %>'></asp:Label>
                <asp:Image ID="helpcircle_gif" runat="server" AlternateText='<%# AspDotNetStorefrontCore.AppLogic.GetString("shoppingcart.cs.30",ThisCustomer.SkinID,ThisCustomer.LocaleSetting) %>' ImageUrl='<%$ Tokens:SkinImage, helpcircle.gif %>' />
            </div>
            <div class="cart-column cart-price cart-column-option-price">
                <asp:Label ID="OrderOptionPrice" CssClass="OrderOptionsPrice" runat="server" Text='<%# DisplayCost(Container.DataItemAs<OrderOption>()) %>'></asp:Label>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>

