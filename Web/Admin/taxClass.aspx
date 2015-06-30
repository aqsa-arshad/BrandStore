<%@ Page language="c#" Inherits="AspDotNetStorefrontAdmin.taxClass" CodeFile="taxClass.aspx.cs" 
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
     <div class="breadCrumb3">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal><asp:Literal ID="ltValid" runat="server"></asp:Literal></div>
    <div id="help">
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
                                <td class="titleTable">
                                    <font class=""><asp:Literal ID="litHeader" runat="server" Text="<%$Tokens:StringResource, admin.taxclass.taxclassheader %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="">
                                        <asp:Panel runat="server" id="pnlGrid">
                                        <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.AddNewUC %>" OnClick="btnInsert_Click" /><br />
                                            <br />
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="True" PageSize="15" AllowSorting="True" HorizontalAlign="Left" OnRowCancelingEdit="gMain_RowCancelingEdit" OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" OnPageIndexChanging="gMain_PageIndexChanging" OnRowUpdating="gMain_RowUpdating" OnRowEditing="gMain_RowEditing" CellPadding="0" GridLines="None" ShowFooter="True">
                                            <Columns>
                                                <asp:CommandField ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" >
                                                    <ItemStyle Width="60px" />
                                                </asp:CommandField>
                                                <asp:BoundField DataField="TaxClassID" HeaderText="<%$Tokens:StringResource, admin.taxclass.ID %>" ReadOnly="True" SortExpression="TaxClassID" >
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:BoundField>
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.taxclass.name %>" SortExpression="Name">
                                                    <ItemTemplate>
                                                        <asp:Literal runat="server" ID="ltName" Text='<%# DataBinder.Eval(Container.DataItem, "Name") %>'></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <%# DataBinder.Eval(Container.DataItem, "EditName") %>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField> 
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.taxclass.taxcodeheader %>" SortExpression="TaxCode">
                                                    <ItemTemplate>
                                                        <asp:Literal runat="server" ID="ltTaxCode" Text='<%# DataBinder.Eval(Container.DataItem, "TaxCode") %>'></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtTaxCode" runat="server" CssClass="singleNormal" Text='<%# DataBinder.Eval(Container.DataItem, "TaxCode") %>'></asp:TextBox>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:TemplateField>    
                                                <asp:TemplateField HeaderText="<%$Tokens:StringResource, admin.taxclass.displayorderheader %>" SortExpression="DisplayOrder">
                                                    <ItemTemplate>
                                                        <asp:Literal runat="server" ID="ltDisplayOrder" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtDisplayOrder" runat="server" CssClass="singleNormal" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayOrder") %>'></asp:TextBox>
                                                        <asp:CompareValidator ID="cmpValTxtDisplayOrder" runat="server" ControlToValidate="txtDisplayOrder" ValueToCompare="0" Operator="GreaterThanEqual" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidIntegerNumberPrompt%>" Display="Dynamic" Type="Integer" />
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:TemplateField>                                             
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("TaxClassID") %>' runat="Server" AlternateText="Delete" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
                                                    </ItemTemplate>
                                                    <ItemStyle CssClass="selectData" Width="25px" />
                                                </asp:TemplateField>
                                                <asp:BoundField Visible="False" DataField="EditName" ReadOnly="True" />
                                            </Columns>
                                            <PagerSettings FirstPageText="&amp;lt;&amp;lt;First Page" LastPageText="Last Page&amp;gt;&amp;gt;"
                                                Mode="NumericFirstLast" PageButtonCount="15" Position="TopAndBottom" />
                                            <FooterStyle CssClass="gridFooter" />
                                            <RowStyle CssClass="gridRow" />
                                            <EditRowStyle CssClass="gridEdit2" />
                                            <PagerStyle CssClass="gridPager" HorizontalAlign="Left" />
                                            <HeaderStyle CssClass="gridHeader" />
                                            <AlternatingRowStyle CssClass="gridAlternatingRow" />
                                        </asp:GridView>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlAdd" runat="Server" DefaultButton="btnSubmit">
                                        <div style="margin-top: 5px; margin-bottom: 15px;">
                                            <asp:Literal ID="litReqField" Text="<%$Tokens:StringResource, admin.common.RequiredFieldsPrompt %>" runat="server" />
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Literal ID="litTaxClass" Text="<%$Tokens:StringResource, admin.taxclass.taxclass %>" runat="server" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:Literal ID="ltTaxClass" runat="server"></asp:Literal>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Literal ID="litTaxCode" Text="<%$Tokens:StringResource, admin.taxclass.taxcode %>" runat="server" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtTaxCode" runat="server" CssClass="singleShort" ValidationGroup="gAdd"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Literal ID="litDispOrder" Text="<%$Tokens:StringResource, admin.taxclass.displayorder %>" runat="server" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtDisplayOrder" runat="server" CssClass="singleShort" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:CompareValidator ID="cmpValTxtDisplayOrder" runat="server" ControlToValidate="txtDisplayOrder" ValueToCompare="0" Operator="GreaterThanEqual" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidIntegerNumberPrompt%>" Display="Dynamic" Type="Integer" />
                                                </td>
                                            </tr>                                            
                                            <tr>
                                                <td colspan="2">
                                                    <asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                                </td>
                                            </tr>
                                        </table>
                                        <div style="width: 100%; text-align: center;">
                                            &nbsp;&nbsp;<asp:Button ValidationGroup="gAdd" CausesValidation="true" ID="btnSubmit" CssClass="normalButtons" runat="server" OnClick="btnSubmit_Click" Text="Submit" />
                                            &nbsp;&nbsp;<asp:Button ID="btnCancel" CssClass="normalButtons" runat="server" Text="Cancel" OnClick="btnCancel_Click" />
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
