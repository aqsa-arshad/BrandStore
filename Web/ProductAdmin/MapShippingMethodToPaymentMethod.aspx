<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MapShippingMethodToPaymentMethod.aspx.cs" Inherits="AspDotNetStorefrontAdmin.MapShippingMethodToPaymentMethod" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div id="help">
        <div>
            <table width="100%"  border="0" cellpadding="0" cellspacing="0" class="toppage">
                <tr>
                    <td align="left" valign="middle">
                       <div class="wrapper">
                       </div>
                    </td>
                </tr>
            </table>
        </div>
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>

        <br />
        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.Info %>" />
        <br />
     <br />
     <div id="content">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable">
            <tr>
                <td style="width: 669px">
                    <div class="wrapper">                        
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable">
                            <tr>
                                <td class="tablenormal">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.AvailableShippingMethods %>" /></font>
                                </td>
              
                                <td class="tablenormal">              
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.AvailablePaymentMethods %>" /></font>    
        
                                </td>
                                <td class="tablenormal" style="width: 65px">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Move %>" /></font>
                                 </td>
                                <td class="tablenormal">              
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.SelectedPaymentMethods %>" /></font>    
                                 </td>
                                <td class="tablenormal" style="width: 196px" align="center">              
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.Prioritize %>" /></font></td>
                            <tr>
                                <td>
                                    <asp:ListBox ID="ListBoxAvailShippingMethods" runat="server" Height="150px" Width="225px" OnSelectedIndexChanged="ListBoxAvailShippingMethods_SelectedIndexChanged" AutoPostBack="True"></asp:ListBox>
                                </td>
                                <td>
                                    <asp:ListBox ID="ListBoxAvailPaymentMethods" runat="server" Height="150px" SelectionMode="Single" Width="225px"></asp:ListBox>
                                </td>
                                  <TD vAlign="middle" align="center" style="width: 53px"><asp:button id="BtnSelectOne" CssClass="normalButtons" runat="server" Width="23" Height="18" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.SelectOne %>" OnClick="BtnSelectOne_Click"></asp:button><br/>
									<asp:button id="BtnDeSelectOne" runat="server" Width="23" Height="18" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.DeselectOne %>" OnClick="BtnDeSelectOne_Click"></asp:button><br/>
									<asp:button id="BtnSelectAll" runat="server" Width="23" Height="18" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.SelectAll %>" OnClick="BtnSelectAll_Click"></asp:button><br/>
									<asp:button id="BtnDeSelectALL" runat="server" Width="23" Height="18" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.DeselectAll %>" OnClick="BtnDeSelectALL_Click"></asp:button></TD>
                                   <td><asp:ListBox ID="ListBoxSelectedPaymentMethods" runat="server" Height="150px" SelectionMode="Multiple" Width="225px">
                                   </asp:ListBox>
                                   <br />
                                 </td>
                                <td style="width: 196px">
                                    &nbsp;<asp:Button ID="btnMovePaymentUp" runat="server" CssClass="normalButtons" OnClick="btnMovePaymentUp_Click"
                                        Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.MoveUp %>" /></td>
                            </tr>
                           
                        </table>
                    </div>
                </td>
            </tr>
        </table>
       </div>
        <br />
        <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.InfoToSave %>:" /></font><br />
                                   <asp:TextBox ID="txtSaveToDBInfo" runat="server" Width="453px" ReadOnly="true"></asp:TextBox>
        <br /> <br />
        <asp:Button ID="btnUpdateShippingToPaymentMethod" runat="server" CssClass="normalButtons" OnClick="btnUpdateShippingToPaymentMethod_Click"
            Text="<%$Tokens:StringResource, admin.MapShippingMethodToPaymentMethod.Update %>" Width="455px" />
      
</asp:Content>