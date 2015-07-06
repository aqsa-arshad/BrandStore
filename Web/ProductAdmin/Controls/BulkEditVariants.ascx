<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkEditVariants.ascx.cs"
    Inherits="AspDotNetStorefrontControls.BulkEditVariants" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="tcontrol" %>
<div align="center">
    <asp:Panel runat="server" ID="pnlFilterOptions">
        <asp:UpdatePanel runat="server" ID="updFilterOptions" UpdateMode="Conditional">
            <ContentTemplate>
                <div align="center">
                    <asp:Button ID="btnShowFilters" runat="server" Text="<%$ Tokens:StringResource, admin.prices.ShowFilters%>"
                        class="normalButtons" />
                    <br />
                    <br />
                </div>
                <cc1:ModalPopupExtender ID="ModalPopupExtender1" runat="server" PopupControlID="pnlFilterPopup"
                    TargetControlID="btnShowFilters" BackgroundCssClass="modal_popup_background">
                </cc1:ModalPopupExtender>
                <div style="display: none;">
                    <asp:Panel runat="server" ID="pnlFilterPopup" CssClass="modal_popup" Width="500px"
                        Height="350px">
                        <div class="modal_popup_Header" style="display: block;">
                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.prices.ChooseFilters%>" />
                        </div>
                        <div class="modal_popup_Content" align="center">
                            <table cellpadding="10">
                                <tbody>
                                    <tr>
                                        <td align="left" valign="top">
                                            <asp:Label runat="server" ID="lblFields" Text="Fields To Edit: " />
                                            <br />
                                            &nbsp;<input runat="server" id="chkName" type="checkbox" checked="Checked" />
                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Name%>" />
                                            <br />
                                            &nbsp;<input runat="server" id="chkSKU" type="checkbox" checked="Checked" />
                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.VariantSkuSuffix%>" />
                                            <br />
                                            &nbsp;<input runat="server" id="chkPrice" type="checkbox" checked="Checked" />
                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Price%>" />
                                            <br />
                                            &nbsp;<input runat="server" id="chkSalePrice" type="checkbox" checked="Checked" />
                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.SalePrice%>" />
                                            <br />
                                            &nbsp;<input runat="server" id="chkInventory" type="checkbox" checked="Checked" />
                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Inventory%>" />
                                            <br />
                                            &nbsp;<input runat="server" id="chkPublished" type="checkbox" checked="Checked" />
                                            <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.Published%>" />
                                            &nbsp;
                                        </td>
                                        <td align="left" valign="top">
                                            <asp:UpdatePanel runat="server" ID="updEntitySelectLists">
                                                <ContentTemplate>
                                                    <asp:Literal runat="server" Text="<%$ Tokens:StringResource, admin.common.EntityFilter%>" /><br />
                                                    <asp:DropDownList runat="server" ID="ddlEntityType" OnSelectedIndexChanged="ddlEntityType_OnSelectedIndexChanged"
                                                        AutoPostBack="true" Width="195px">
                                                        <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.ddSelectOne %>" Value="-1" />
                                                        <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.Category%>" Value="1" />
                                                        <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.Manufacturer%>" Value="2" />
                                                        <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.Section%>" Value="3" />
                                                        <asp:ListItem Text="<%$ Tokens:StringResource, admin.common.Distributor%>" Value="4" />
                                                    </asp:DropDownList>
                                                    <br />
                                                    <br />
                                                    <asp:DropDownList runat="server" ID="ddlEntityName" Width="195px" Enabled="false">
                                                    </asp:DropDownList>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            <div class="modal_popup_Footer" style="display: block; bottom: 0px;" align="center">
                                <asp:Button runat="server" ID="btnUpdateFilters" Text="<%$ Tokens:StringResource, admin.prices.UpdateFilters%>"
                                    class="normalButtons" OnClick="btnUpdateFilters_OnClick" />
                                &nbsp;
                                <asp:Button runat="server" ID="btnClose" Text="<%$ Tokens:StringResource, admin.common.Close%>"
                                    class="normalButtons" />
                            </div>
                    </asp:Panel>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
    <asp:UpdatePanel ID="updVariantGrid" runat="server">
        <ContentTemplate>
            <tcontrol:RadGrid ID="grdVariants" runat="server" AllowMultiRowEdit="True" AutoGenerateColumns="False"
                GridLines="None" PageSize="50" AllowCustomPaging="true" AllowPaging="True"
                ShowFooter="True" ShowStatusBar="true" OnItemCommand="grdVariants_OnItemCommand"
                OnItemDataBound="grdVariants_OnItemDataBound" OnPreRender="grdVariants_PreRender"
                OnItemCreated="grdVariants_OnItemCreated" OnNeedDatasource="grdVariants_OnNeedDatasource"
                ClientSettings-AllowKeyboardNavigation="true" PagerStyle-Mode="NumericPages">
                <HeaderContextMenu>
                    <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                </HeaderContextMenu>
                <MasterTableView GridLines="None" EditMode="InPlace" CommandItemDisplay="TopAndBottom"
                    AdditionalDataFieldNames="VariantID" DataKeyNames="VariantID,ProductID" Width="100%">
                    <RowIndicatorColumn>
                        <HeaderStyle Width="20px"></HeaderStyle>
                    </RowIndicatorColumn>
                    <ExpandCollapseColumn>
                        <HeaderStyle Width="20px"></HeaderStyle>
                    </ExpandCollapseColumn>
                    <Columns>
                        <tcontrol:GridBoundColumn DataField="VariantID" DataType="System.Int32" HeaderText="<%$ Tokens:StringResource, admin.common.VariantID%>"
                            SortExpression="VariantID" UniqueName="VariantID" ReadOnly="true" ForceExtractValue="Always"
                            ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Left">
                        </tcontrol:GridBoundColumn>
                        <tcontrol:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, admin.common.ProductName %>"
                            ItemStyle-HorizontalAlign="Left" ItemStyle-Wrap="true">
                            <EditItemTemplate>
                                <asp:Image runat="server" ID="imgProduct" Visible="false" Height="35px" />
                                &nbsp; <b>
                                    <asp:Label runat="server" ID="lblProductName" Text='<%# Eval("ProductName") %>' /></b>
                            </EditItemTemplate>
                        </tcontrol:GridTemplateColumn>
                        <tcontrol:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, admin.common.Name%>"
                            ItemStyle-HorizontalAlign="Left">
                            <EditItemTemplate>
                                <asp:TextBox runat="server" ID="txtName" Width="250" Text='<%# Bind("LocaleName") %>' />
                            </EditItemTemplate>
                        </tcontrol:GridTemplateColumn>
                        <tcontrol:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, admin.common.VariantSkuSuffix%>"
                            ItemStyle-HorizontalAlign="Left">
                            <EditItemTemplate>
                                <asp:TextBox runat="server" ID="txtSKUSuffix" Width="75" Text='<%# Bind("SKUSuffix") %>' />
                            </EditItemTemplate>
                        </tcontrol:GridTemplateColumn>
                        <tcontrol:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, admin.common.Price%>"
                            ItemStyle-HorizontalAlign="Left">
                            <EditItemTemplate>
                                <asp:TextBox runat="server" ID="txtPrice" Width="75" Text='<%# Bind("Price", "{0:n4}") %>' />
                            </EditItemTemplate>
                        </tcontrol:GridTemplateColumn>
                        <tcontrol:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, admin.common.SalePrice%>"
                            ItemStyle-HorizontalAlign="Left">
                            <EditItemTemplate>
                                <asp:TextBox runat="server" ID="txtSalePrice" Width="75" Text='<%# Bind("SalePrice", "{0:n4}") %>' />
                            </EditItemTemplate>
                        </tcontrol:GridTemplateColumn>
                        <tcontrol:GridTemplateColumn HeaderText="<%$ Tokens:StringResource, admin.common.Inventory%>"
                            ItemStyle-HorizontalAlign="Left">
                            <EditItemTemplate>
                                <asp:TextBox runat="server" ID="txtInventory" Width="100" Text='<%# Bind("Inventory") %>' />
                            </EditItemTemplate>
                        </tcontrol:GridTemplateColumn>
                        <tcontrol:GridCheckBoxColumn DataField="Published" DataType="System.Boolean" HeaderText="<%$ Tokens:StringResource, admin.common.Published%>"
                            SortExpression="Published" UniqueName="Published" ItemStyle-Width="45px" ItemStyle-HorizontalAlign="Left">
                        </tcontrol:GridCheckBoxColumn>
                        <tcontrol:GridEditCommandColumn UniqueName="EditCommandColumn" />
                    </Columns>
                    <EditFormSettings>
                        <EditColumn UniqueName="EditCommandColumn1">
                        </EditColumn>
                    </EditFormSettings>
                    <CommandItemTemplate>
                        <asp:Button runat="server" ID="btnUpdateAll" Text="Update All" CommandName="UpdateAll" />
                        <asp:Button runat="server" ID="btnReset" Text="<%$ Tokens:StringResource, admin.common.Reset%>"
                            CommandName="Reset" />
                    </CommandItemTemplate>
                    <PagerStyle Mode="NextPrevNumericAndAdvanced" AlwaysVisible="true" Position="TopAndBottom" />
                </MasterTableView>
                <ClientSettings>
                    <Scrolling AllowScroll="false" EnableVirtualScrollPaging="false" UseStaticHeaders="True"
                        SaveScrollPosition="false" />
                </ClientSettings>
                <FilterMenu>
                    <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                </FilterMenu>
            </tcontrol:RadGrid>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
