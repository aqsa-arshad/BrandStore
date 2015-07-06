<%@ Page Language="C#" AutoEventWireup="true" CodeFile="stringresource.aspx.cs" Inherits="AspDotNetStorefrontAdmin.stringresourcepage" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder" >
    <div id="">
        <div style="margin-bottom: 5px; margin-top: 5px;" class="breadCrumb3">
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
                                <td class="titleTable" width="130">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.StringSearch %>" /></font>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #a2a2a2;" />
                                <td style="width: 5px;" />
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal ID="Literal1" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.StringResources %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTableNP" valign="top" width="130">
                                    <asp:Panel runat="server" DefaultButton="btnSearch">
                                        <asp:TextBox ID="txtSearch" Width="130" runat="server"></asp:TextBox>
                                        <asp:Button runat="server" ID="btnSearch" CssClass="normalButtons" Text="Search"
                                            OnClick="btnSearch_Click" />
                                    </asp:Panel>
                                    <br /><br />
                                    <table id="tblLocale" width="100%" cellpadding="0" cellspacing="0" border="0" runat="server">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Literal ID="Literal2" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.Locale %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableAPL">
                                                <div>
                                                    <asp:DropDownList runat="server" ID="ddLocales" AutoPostBack="true" OnSelectedIndexChanged="ddLocales_SelectedIndexChanged">
                                                    </asp:DropDownList>                                                    
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <br />
                                    <table width="100%" cellpadding="0" cellspacing="0" border="0">
                                        <tr>
                                            <td class="titleTable">
                                                <font class="subTitle"><asp:Literal ID="Literal3" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.Index %>" /></font>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="contentTableAPL">
                                                <div>
                                                    <asp:TreeView ID="treeMain" runat="server" OnSelectedNodeChanged="treeMain_SelectedNodeChanged">
                                                    </asp:TreeView>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td style="width: 5px;" />
                                <td style="width: 1px; background-color: #a2a2a2;" />
                                <td style="width: 5px;" />
                                <td class="contentTable" valign="top" width="*">
                                
                                <asp:Panel ID="pnlMultiStore" runat="server" Visible="false">    
                                    Stores: <asp:DropDownList ID="cboStores" runat="server" 
                                                AutoPostBack="true" 
                                                DataSource='<%# BindableStores %>'                                                            
                                                DataTextField="Text"
                                                DataValueField="Value"
                                                Visible='<%# Stores.Count > 1 %>' 
                                                onselectedindexchanged="cboStores_SelectedIndexChanged" >
                                            </asp:DropDownList>
                                </asp:Panel>
                                    <div class="wrapperLeft">
                                        <asp:Panel runat="server" id="pnlGrid">
                                        <div id="divActions" runat="server" style="margin-top: 5px; margin-bottom: 5px;">
                                            <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="<%$ Tokens:StringResource, admin.stringresources.AddNewString %>" Width="95" OnClick="btnInsert_Click" />
                                            <asp:Literal ID="ltActions" runat="Server"></asp:Literal>
                                            <asp:Button runat="server" ID="btnLoadExcelServer" CssClass="normalButtons" Text="<%$ Tokens:StringResource, admin.stringresources.ReloadFromExcelOnServer %>" Width="180" OnClick="btnLoadExcelServer_Click" />
                                            <asp:Button runat="server" ID="btnUploadExcel" CssClass="normalButtons" Text="<%$ Tokens:StringResource, admin.stringresources.ReloadFromExcelOnPC %>" Width="190" OnClick="btnUploadExcel_Click" />
                                            <asp:Button runat="server" ID="btnShowMissing" CssClass="normalButtons" Text="<%$ Tokens:StringResource, admin.stringresources.ShowMissing %>" Width="120" OnClick="btnShowMissing_Click" />
                                            <asp:Button runat="server" ID="btnShowModified" CssClass="normalButtons" Text="<%$ Tokens:StringResource, admin.stringresources.ShowModified %>" Width="120" OnClick="btnShowModified_Click" />
                                            <asp:Button runat="server" ID="btnClearLocale" CssClass="normalButtons" Text="<%$ Tokens:StringResource, admin.stringresources.ClearLocale %>" Width="75" OnClick="btnClearLocale_Click" />
                                        </div>
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="True" PageSize="50" AllowSorting="True" CssClass="tableoverallGrid" HorizontalAlign="Left" OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" OnRowUpdating="gMain_RowUpdating" OnRowEditing="gMain_RowEditing" BorderStyle="None" CellPadding="0" GridLines="None" ShowFooter="True">
                                            <Columns>
                                                <asp:CommandField ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" />
                                                
                                                <asp:TemplateField HeaderText="StoreId" SortExpression="StoreId">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "StoreId")%>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <%--Display label if we only have 1 store(non multi-store)--%>
                                                        <asp:Label ID="lblStoreId" runat="server" 
                                                            Text='<%# DataBinder.Eval(Container.DataItem, "StoreId")%>' 
                                                            Visible='<%# Stores.Count == 1 %>' />
                                                            
                                                        <%--For multi-store we display dropdown to allow different selection--%>
                                                        <asp:DropDownList ID="cboEditStores" runat="server" 
                                                            DataSource='<%# BindableStores %>'                                                            
                                                            DataTextField="Text"
                                                            DataValueField="Value"
                                                            Visible='<%# Stores.Count > 1 %>' >
                                                        </asp:DropDownList>
                                                        
                                                        <%--Hidden field to store reference to the previous store id before selecting a different one when multi-store--%>
                                                        <asp:HiddenField ID="hdfPrevStoreId" runat="server" 
                                                            Value='<%# DataBinder.Eval(Container.DataItem, "StoreId")%>' />
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
                                                </asp:TemplateField>
                                                
                                                <asp:BoundField DataField="StringResourceID" HeaderText="ID" ReadOnly="True" SortExpression="StringResourceID" >
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="Name" SortExpression="Name">
                                                    <ItemTemplate>
                                                        <div class='<%# this.CssClassIf(CurrentRowIsDuplicate(Container.DataItem as System.Data.DataRowView) || CurrentRowWasDuplicatedFrom(Container.DataItem as System.Data.DataRowView), "duplicated_string") %>' >
                                                            <%# AspDotNetStorefrontCore.CommonLogic.IIF(DataBinder.Eval(Container.DataItem, "Modified").ToString() == "0", "","<font color=\"blue\"><b>") + DataBinder.Eval(Container.DataItem, "Name") + AspDotNetStorefrontCore.CommonLogic.IIF(DataBinder.Eval(Container.DataItem, "Modified").ToString() == "0", "","</b></font>") %>
                                                        </div>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtName" runat="Server" CssClass="singleAuto" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator" ControlToValidate="txtName" runat="server" ErrorMessage="!!"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Value" SortExpression="ConfigValue">
                                                    <ItemTemplate>
                                                        <div style="white-space: normal; overflow: visible; width: 225px;" class='<%# this.CssClassIf(CurrentRowIsDuplicate(Container.DataItem as System.Data.DataRowView) || CurrentRowWasDuplicatedFrom(Container.DataItem as System.Data.DataRowView), "duplicated_string") %>' >
							                                <%# AspDotNetStorefrontCore.CommonLogic.IIF(DataBinder.Eval(Container.DataItem, "Modified").ToString() == "0", "","<font color=\"blue\"><b>") + DataBinder.Eval(Container.DataItem, "ConfigValue") + AspDotNetStorefrontCore.CommonLogic.IIF(DataBinder.Eval(Container.DataItem, "Modified").ToString() == "0", "","</b></font>") %>
							                            </div>
						                            </ItemTemplate>
						                            <EditItemTemplate>
							                            <asp:TextBox Runat="server" ID="txtValue" CssClass="multiAuto" TextMode="MultiLine" Text='<%# DataBinder.Eval(Container.DataItem, "ConfigValue") %>'></asp:TextBox>
						                            </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Modified" SortExpression="Modified">
                                                    <ItemTemplate>
                                                        <%# AspDotNetStorefrontCore.CommonLogic.IIF(DataBinder.Eval(Container.DataItem, "Modified").ToString() == "0", "No","<font color=\"blue\"><b>Yes</b></font>") %>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Locale" SortExpression="LocaleSetting">
                                                    <ItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "LocaleSetting")%>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:DropDownList ID="ddLocale" runat="server"></asp:DropDownList>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lightData" />
                                                </asp:TemplateField>
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("StringResourceID") %>' runat="Server" AlternateText="Delete" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" />
                                                </asp:TemplateField>
                                            </Columns>
                                            <PagerSettings FirstPageText="&amp;lt;&amp;lt;First Page" LastPageText="Last Page&amp;gt;&amp;gt;"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="gridRowPlain" />
                                            <EditRowStyle CssClass="gridEdit2" />
                                            <PagerStyle CssClass="gridPager" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRowPlain" />
                                        </asp:GridView>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlAdd" runat="Server" DefaultButton="btnSubmit" >
                                        <div style="margin-top: 5px; margin-bottom: 15px;">
                                            <asp:Literal ID="Literal4" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.RequiredFields %>" />
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal ID="Literal6" runat="server" Text="<%$ Tokens:StringResource, admin.common.Name %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtName" runat="server" CssClass="singleNormal" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="Fill in Name" ControlToValidate="txtName" ID="RequiredFieldValidator2" ValidationGroup="gAdd" Display="dynamic" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*<asp:Literal ID="Literal7" runat="server" Text="<%$ Tokens:StringResource, admin.common.Value %>" />:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtDescription" runat="server" CssClass="multiNormal" TextMode="multiline" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="Fill in Description" ControlToValidate="txtDescription" ID="RequiredFieldValidator3" ValidationGroup="gAdd" Display="dynamic" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator>
                                                </td>
                                            </tr>     
                                            <tr id="trLocale" runat="server">
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Literal ID="Literal5" runat="server" Text="<%$ Tokens:StringResource, admin.stringresources.Locale %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddLocale" runat="server" ValidationGroup="gAdd"></asp:DropDownList>
                                                    <asp:RequiredFieldValidator ErrorMessage="Select Locale" InitialValue="0" ControlToValidate="ddLocale" ID="RequiredFieldValidator1" ValidationGroup="gAdd" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator> 
                                                </td>
                                            </tr>                                                                                         
                                            
                                            <tr id="trStore" runat="server" visible="false">
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">Store:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="cboStoreAddString" runat="server" ValidationGroup="gAdd"></asp:DropDownList>
                                                </td>
                                            </tr>
                                            
                                            <tr>
                                                <td colspan="2">
                                                    <asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                                </td>
                                            </tr>
                                        </table>
                                        <div style="width: 100%; text-align: center;">
                                            <asp:Button ValidationGroup="gAdd" ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" CssClass="normalButtons" Text="Submit" />
                                            <asp:Button ID="btnCancel" runat="server" CssClass="normalButtons" Text="Cancel" OnClick="btnCancel_Click" />
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
