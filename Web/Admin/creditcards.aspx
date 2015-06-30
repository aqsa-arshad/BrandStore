<%@ Page Language="C#" AutoEventWireup="true" CodeFile="creditcards.aspx.cs" Inherits="AspDotNetStorefrontAdmin.creditcards" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
        <div class="breadCrumb3">
    <asp:Literal ID="ltValid" runat="server" />
    </div>
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class=""><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.creditcards.CreditCards %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="" valign="top" width="100%">
                                    <div class="">
                                        <asp:Panel runat="server" id="pnlGrid">
                                        <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.AddNewUC %>" OnClick="btnInsert_Click" /><br />
                                            <br />
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="True" PageSize="15" AllowSorting="True" HorizontalAlign="Left" OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" OnRowUpdating="gMain_RowUpdating" OnRowEditing="gMain_RowEditing" CellPadding="0" GridLines="None" ShowFooter="True">
                                            <Columns>
                                                <asp:CommandField ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" >
                                                    <ItemStyle Width="60px" />
                                                </asp:CommandField>
                                                <asp:BoundField DataField="CardTypeID" HeaderText="<%$Tokens:StringResource, admin.common.ID %>" ReadOnly="True" SortExpression="CardTypeID" >
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.creditcards.CardType %>" SortExpression="CardType">
                                                    <ItemTemplate>
                                                        <asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "CardType") %>'></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtName" runat="Server" Text='<%# DataBinder.Eval(Container.DataItem, "CardType")%>' CssClass="singleNormal"></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>                                                
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("CardTypeID") %>' runat="Server" AlternateText="<%$Tokens:StringResource, admin.common.Delete %>" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" Width="25px" />
                                                </asp:TemplateField>
                                            </Columns>
                                            <PagerSettings FirstPageText="<%$Tokens:StringResource, admin.countries.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.countries.LastPage %>"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="gridEdit" />
                                            <PagerStyle CssClass="pagerGrid" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                        </asp:GridView>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlAdd" runat="Server" DefaultButton="btnSubmit">
                                        <div style="margin-top: 5px; margin-bottom: 15px;">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.RequiredFieldsPrompt %>" />
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.CreditCard %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtName" runat="server" CssClass="singleNormal" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinName %>" ControlToValidate="txtName" ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 
                                                    <asp:Image ID="imgInfo" runat="server" ToolTip="<%$Tokens:StringResource, admin.creditcards.tooltip.imgInfo %>" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" />                                                   
                                                </td>
                                            </tr>                                            
                                            <tr>
                                                <td colspan="2">
                                                    <asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                                </td>
                                            </tr>
                                        </table>
                                        <div style="width: 100%; text-align: center;">
                                            &nbsp;&nbsp;<asp:Button ValidationGroup="gAdd" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Submit %>" />
                                            &nbsp;&nbsp;<asp:Button ID="btnCancel" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Cancel %>" OnClick="btnCancel_Click" />
                                        </div>
                                        </asp:Panel>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>