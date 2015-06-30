<%@ Page Language="c#" Inherits="AspDotNetStorefront.mobileemailproduct" CodeFile="mobileemailproduct.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent">
    <asp:Panel ID="pnlContent" runat="server" >
    <asp:UpdatePanel ID="upEmailProduct" runat="server" UpdateMode="Conditional">
<ContentTemplate>
<asp:Panel ID="pnlSuccess" runat="server" Visible="false">
    <div align="center">
        
        <asp:Label ID="emailproduct_aspx_8" Font-Bold="true" runat="server"></asp:Label> 
        
        <asp:HyperLink ID="ReturnToProduct" Font-Bold="true" runat="server"></asp:HyperLink>
    </div>
</asp:Panel>    
<asp:Panel ID="pnlRequireReg" runat="server" Visible="false">
    <asp:Literal ID="emailproduct_aspx_1" Mode="PassThrough" runat="server"></asp:Literal>
</asp:Panel>    
<asp:Panel ID="pnlEmailToFriend" runat="server" Visible="false" DefaultButton="btnSubmit">
    <ul data-role="listview">
        <li data-icon="arrow-l" class="back">
            <asp:HyperLink ID="ProductNavLink"  CssClass="ProductNavLink" runat="server"></asp:HyperLink>
        </li>
        <li>
            <table border="0" cellpadding="0" cellspacing="0" width="100%">
                <tr>
                    <td align="center" valign="top" width="40%">
                        <asp:Image ID="imgProduct" runat="server" Width="60" />
                    </td>
                    <td class="ProductNameText">
                        <asp:Label ID="emailproduct_aspx_4" runat="server"></asp:Label>
                        
                    </td>
                </tr>
            </table>
        </li>
        <li>
            <asp:Label ID="emailproduct_aspx_11" runat="server" Font-Bold="true"></asp:Label>
            <asp:TextBox ID="txtToAddress" runat="server" MaxLength="75" style="width:85%;" CausesValidation="true"></asp:TextBox>
            <asp:RequiredFieldValidator ID="reqToAddress" ControlToValidate="txtToAddress" runat="server"
                Display="Dynamic"></asp:RequiredFieldValidator>
            <aspdnsf:EmailValidator ID="regexToAddress" ControlToValidate="txtToAddress"
                Display="Dynamic" runat="server"></aspdnsf:EmailValidator>
            <asp:Label ID="emailproduct_aspx_22" runat="server" Font-Bold="true"></asp:Label>
            <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" style="width:85%;" Rows="7"></asp:TextBox>
            
            <asp:Label ID="emailproduct_aspx_15" runat="server" Font-Bold="true"></asp:Label>
            <asp:TextBox ID="txtFromAddress" MaxLength="75" runat="server" style="width:85%;" CausesValidation="true"></asp:TextBox>
            <asp:RequiredFieldValidator ID="reqFromAddress" ControlToValidate="txtFromAddress"
                runat="server" Display="Dynamic"></asp:RequiredFieldValidator>
            <aspdnsf:EmailValidator ID="regexFromAddress" ControlToValidate="txtFromAddress"
                Display="Dynamic" runat="server"></aspdnsf:EmailValidator>
            
            <asp:Button ID="btnSubmit" runat="server" CausesValidation="true" OnClick="btnSubmit_Click"  CssClass="fullwidthshortgreen" />
            <AjaxToolkit:TextBoxWatermarkExtender ID="tbweRecipient" runat="server" TargetControlID="txtToAddress"
                WatermarkCssClass="watermarked" WatermarkText='<%$ Tokens:StringResource, emailproduct.aspx.12 %>' />
            <AjaxToolkit:TextBoxWatermarkExtender ID="tbweMessage" runat="server" TargetControlID="txtMessage"
                WatermarkCssClass="watermarked" WatermarkText='<%$ Tokens:StringResource, emailproduct.aspx.23 %>' />
            <AjaxToolkit:TextBoxWatermarkExtender ID="tbweYourEmail" runat="server" TargetControlID="txtFromAddress"
                WatermarkCssClass="watermarked" WatermarkText='<%$ Tokens:StringResource, emailproduct.aspx.25 %>' />
        </li>
    </ul>
</asp:Panel> 
</ContentTemplate>
</asp:UpdatePanel>
    </asp:Panel>
</asp:Content>

