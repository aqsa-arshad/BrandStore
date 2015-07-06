<%@ Page Language="C#" AutoEventWireup="true" CodeFile="localesettings.aspx.cs" Inherits="AspDotNetStorefrontAdmin.localesettings" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
        <div class="breadCrumb3">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal><asp:Literal ID="ltValid" runat="server"></asp:Literal></div>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.LocaleSettings %>" />:
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="">
                                        <asp:Panel runat="server" id="pnlGrid">
                                        <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.AddNewUC %>" OnClick="btnInsert_Click" /><br />
                                            <br />
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="True" PageSize="15" AllowSorting="True" HorizontalAlign="Left" OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" OnRowUpdating="gMain_RowUpdating" OnRowEditing="gMain_RowEditing" CellPadding="0" GridLines="None" BorderStyle="None" ShowFooter="True">
                                            <Columns>                                            
                                                <asp:CommandField ButtonType="Image" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" >
                                                </asp:CommandField>
                                                <asp:BoundField DataField="LocaleSettingID" HeaderText="ID" ReadOnly="True" SortExpression="LocaleSettingID" >
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Name %>" SortExpression="Name">
                                                    <ItemTemplate>
                                                        <asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox MaxLength="10" CssClass="singleNormal" runat="server" ID="txtName" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:TextBox>
                                                        ex: en-US
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidatorMessage %>" ControlToValidate="txtName"></asp:RequiredFieldValidator></EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>     
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.Description %>" SortExpression="Description">
                                                    <ItemTemplate>
                                                        <asp:Literal runat="server" ID="ltDescription" Text='<%# DataBinder.Eval(Container.DataItem, "Description") %>'></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox CssClass="singleNormal" runat="server" ID="txtDescription" Text='<%# DataBinder.Eval(Container.DataItem, "Description") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:TemplateField>   
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.localesettings.DefaultCurrency %>" SortExpression="DefaultCurrencyID">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "DefaultCurrencyID") %>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:DropDownList runat="server" ID="ddCurrency"></asp:DropDownList>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField> 
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.localesettings.StringResources %>">
                                                    <ItemTemplate>
                                                        <a href='stringresource.aspx?ShowLocaleSetting=<%# DataBinder.Eval(Container.DataItem, "Name") %>'><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.localesettings.EditUpload %>" /></a>
                                                    </ItemTemplate>
                                                </asp:TemplateField>    
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.common.DisplayOrder %>" SortExpression="DisplayOrder">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox CssClass="singleShortest" runat="server" ID="txtOrder" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:TemplateField>                                       
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("LocaleSettingID") %>' runat="Server" AlternateText="Delete" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" Width="25px" />
                                                </asp:TemplateField>
                                            </Columns>
                                            <PagerSettings FirstPageText="<%$Tokens:StringResource, admin.common.FirstPage %>" LastPageText="<%$Tokens:StringResource, admin.common.LastPage %>"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="tableDataCellGridEdit" />
                                            <PagerStyle CssClass="tablepagerGrid" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                        </asp:GridView>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlAdd" runat="Server" DefaultButton="btnSubmit">
                                        <div style="margin-top: 5px; margin-bottom: 15px;">
                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.RequiredFieldsPrompt %>" />
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Name %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtName" runat="server" CssClass="singleNormal" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.common.FillinName %>" ControlToValidate="txtName" ID="RequiredFieldValidator2" ValidationGroup="gAdd" Display="dynamic" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator>
                                                    <asp:RegularExpressionValidator ErrorMessage="Validate Name" ControlToValidate="txtName" id="RegularExpressionValidator" ValidationGroup="gAdd" Display="dynamic" SetFocusOnError="true" runat="server" ValidationExpression="^[a-z][a-z]-[A-Z][A-Z]$"></asp:RegularExpressionValidator>
                                                    &nbsp;<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.localesettings.Sample %>" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.Description %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtDescription" runat="server" CssClass="multiNormal" TextMode="multiline" ValidationGroup="gAdd"></asp:TextBox>
                                                </td>
                                            </tr>     
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.localesettings.DefaultCurrency %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddCurrency" runat="server" ValidationGroup="gAdd"></asp:DropDownList>
                                                    <asp:RequiredFieldValidator ErrorMessage="<%$Tokens:StringResource, admin.localesettings.SelectCurrency %>" InitialValue="0" ControlToValidate="ddCurrency" ID="RequiredFieldValidator1" ValidationGroup="gAdd" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 
                                                </td>
                                            </tr> 
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.common.DisplayOrder %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                     <asp:TextBox ID="txtOrder" runat="Server" CssClass="single3chars" ValidationGroup="gAdd">1</asp:TextBox>
                                                     <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="<%$Tokens:StringResource, admin.common.EnterDisplayOrder %>" ValidationGroup="gAdd" ControlToValidate="txtOrder"></asp:RequiredFieldValidator>
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
                                            &nbsp;&nbsp;<asp:Button ID="btnCancel" runat="server" CssClass="normalButtons" Text="Cancel" OnClick="btnCancel_Click" />
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