<%@ Page language="c#" Inherits="AspDotNetStorefrontAdmin.taxzips" CodeFile="taxzips.aspx.cs"
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ OutputCache  Duration="1"  Location="none" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle">Zip Code Taxes:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div>
                                    Enter only ZipCodes for which you want to charge sales tax. For those ZipCodes, enter the tax rate for your ZipCode (city, county, state, etc.)
                                    <br /><br />
                                    <asp:Panel runat="server" ID="pnlGrid">
                                        <asp:Button runat="server" ID="btnInsert" CssClass="normalButtons" Text="ADD NEW" OnClick="btnInsert_Click" />
                                        <asp:Button runat="server" ID="btnUpdateOrder" CssClass="normalButtons" Text="Update Tax Rates" OnClick="btnUpdateOrder_Click" /><br />
                                        <br />
                                        <asp:GridView Width="100%" ID="gMain" runat="server" PagerStyle-HorizontalAlign="left" 
                                            PagerSettings-Position="TopAndBottom" AutoGenerateColumns="False" AllowPaging="True" 
                                            PageSize="100" AllowSorting="True" HorizontalAlign="Left" OnRowCancelingEdit="gMain_RowCancelingEdit" 
                                            OnRowCommand="gMain_RowCommand" OnRowDataBound="gMain_RowDataBound" OnSorting="gMain_Sorting" 
                                            OnPageIndexChanging="gMain_PageIndexChanging" OnRowUpdating="gMain_RowUpdating" 
                                            OnRowEditing="gMain_RowEditing" CellPadding="0" GridLines="None" ShowFooter="True">
                                            <Columns>
                                                <asp:CommandField ButtonType="Image" CancelImageUrl="~/App_Themes/Admin_Default/images/cancel.gif" EditImageUrl="~/App_Themes/Admin_Default/images/edit.gif" ShowEditButton="True" UpdateImageUrl="~/App_Themes/Admin_Default/images/update.gif" >
                                                    <ItemStyle Width="60px" />
                                                </asp:CommandField>
                                                
                                                <asp:TemplateField HeaderText="Zip Code" SortExpression="ZipCode">
                                                    <ItemTemplate>
                                                        <%# Eval("ZipCode") %>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtZip" runat="Server" CssClass="singleShortest" MaxLength="5" Text='<%# DataBinder.Eval(Container.DataItem, "ZipCode") %>'></asp:TextBox>
                                                        Ex: 85259
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="!!" ControlToValidate="txtZip"></asp:RequiredFieldValidator>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="normalData" />
                                                </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="Country" SortExpression="Country">
                                                            <ItemTemplate>
                                                                <%# Eval("Name") %>
                                                                <asp:HiddenField ID="hdfCountryID" runat="server" Value='<%# Eval("CountryID") %>' />
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:DropDownList ID="ddlCountry" runat="server" />
                                                            </EditItemTemplate>
                                                        </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Tax Rate">
                                                    <ItemTemplate>
                                                        <asp:Literal ID="ltTaxRate" runat="server"></asp:Literal>
                                                    </ItemTemplate>
                                                    <EditItemTemplate>
                                                        <asp:Literal ID="ltTaxRate" runat="server"></asp:Literal>
                                                    </EditItemTemplate>
                                                    <ItemStyle CssClass="lighterData" />
                                                </asp:TemplateField>  
                                                
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <a name='a<%# Eval("ZipCode") %>'></a>
                                                                <asp:ImageButton ID="imgDelete" CommandName="DeleteItem" CommandArgument='<%# Eval("ZipCode").ToString() + "|" + Eval("Name").ToString() %>'
                                                        runat="Server" AlternateText="Delete" ImageUrl="~/App_Themes/Admin_Default/images/delete2.gif" />                                                        
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
                                    <asp:Panel ID="pnlAdd" runat="server" DefaultButton="btnSubmit">
                                    <div style="margin-top: 5px; margin-bottom: 15px;">
                                            Fields marked with an asterisk (*) are required. All other fields are optional.
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0">
                                            <tr>
                                                <td align="right" valign="middle" width="30%">
                                                    <font class="subTitleSmall">*Country:</font>
                                                </td>
                                                <td align="left" valign="middle" width="70%">
                                                    <asp:DropDownList ID="ddlCountry" runat="server" AutoPostBack="true" />
                                                </td>
                                            </tr>
                                            <tr id="trZipCode" runat="server">
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*Zip Code:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtZip" runat="server" CssClass="singleShorter" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="Fill in Zip Code" ControlToValidate="txtZip"
                                                        ID="RequiredFieldValidator2" ValidationGroup="gAdd" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator>
                                                    <asp:Image ID="imgZip" runat="server" Tooltip="<%$Tokens:StringResource,admin.taxzips.tooltip.imgZip %>" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" />
                                                </td>
                                            </tr>
                                            <tr id="trTaxRate" runat="server">
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall">*Tax Rate:</font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtTax" runat="server" CssClass="singleShort" ValidationGroup="gAdd"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ErrorMessage="Fill in Tax Rate" ControlToValidate="txtTax"
                                                        ID="RequiredFieldValidator3" ValidationGroup="gAdd" SetFocusOnError="true" runat="server">!!</asp:RequiredFieldValidator>
                                                    <asp:Image ID="imgTax" runat="server" ToolTip="<%$Tokens:StringResource,admin.taxzips.tooltip.imgTax %>" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <asp:ValidationSummary ValidationGroup="gAdd" ID="validationSummary" runat="server"
                                                        EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                                </td>
                                            </tr>
                                        </table>
                                        <div style="width: 100%; text-align: center;">
                                            &nbsp;&nbsp;<asp:Button ValidationGroup="gAdd" ID="btnSubmit" CssClass="normalButtons" 
                                            runat="server" OnClick="btnSubmit_Click" Text="Submit" />
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
    <asp:Literal ID="ltMiscellaneous" runat="server"></asp:Literal>
</asp:Content>