<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomerGrid.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.CustomerGrid" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" TagName="editcustomer" Src="editcustomer.ascx" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="AspDotNetStorefrontAdmin" %>
<%@ Import Namespace="AspDotNetStorefrontAdmin.Controls" %>

<div id="pnlLoadingGeneral" runat="server" style="float: left; padding-left:45%; position: absolute; width:100%;" >
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>
            <img id="Img1" runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/waiting.gif" />
        </ProgressTemplate>
    </asp:UpdateProgress>
</div>

<asp:Panel ID="pnlAddCustomer" runat="server" CssClass="modal_popup" Height="80%" Width="725px" ScrollBars="None">
    <div class="modal_popup_Header" id="modaldiv" runat="server">
        <asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.customer.EditCustomer  %>" />
        <div style="float:right;position:absolute;top:9px;right:9px;">
            <asp:ImageButton ID="btnCancelAddCustomer" OnClick="btnCancelAddCustomer_Click" runat="server" src="../App_Themes/Admin_Default/images/delete.png" />
        </div>
    </div>
    <asp:Panel ID="pnlCustomerHolder" runat="server" ScrollBars="Vertical" Height="95%">
    <div style="border:solid 1px white;background:white;">
        <aspdnsf:editcustomer ID="ctrlAddCustomer" runat="server" CssClass="modal_popup_Content" />
        </div>
    </asp:Panel>
</asp:Panel>
<div style="display: none;">
    <asp:Button ID="btnHidden" runat="server" />
</div>
<div style="display: none;">
    <asp:Button ID="btnCancelHidden" runat="server" />    
</div>
<ajax:ModalPopupExtender ID="extCustomerPanel" runat="server" PopupControlID="pnlAddCustomer"
    TargetControlID="btnHidden" BackgroundCssClass="modal_popup_background" CancelControlID="btnCancelAddCustomer"
    PopupDragHandleControlID="modaldiv">
</ajax:ModalPopupExtender>
<asp:UpdatePanel runat="server" ID="updatePanelSearch" UpdateMode="Conditional">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_All" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_#" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_A" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_B" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_C" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_D" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_E" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_F" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_G" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_H" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_I" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_J" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_K" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_L" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_M" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_N" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_O" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_P" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Q" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_R" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_S" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_T" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_U" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_V" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_W" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_X" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Y" />
        <asp:AsyncPostBackTrigger ControlID="ctrlSearch$ctrlAlphaPaging$AlphaFilter_Z" />
    </Triggers>
    <ContentTemplate>
        <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch" OnFilter="ctrlSearch_Filter">
            <Search ID="Search1" runat="server" SearchButtonCaption="Go" SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>"
                SearchTextMinLength="3" SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>"
                ValidateInputLength="false" ShowValidationMessageBox="false" ShowValidationSummary="false"
                UseLandingPageBehavior="false" WillRenderInUpdatePanel="true" />
            <ContentTemplate>
                <telerik:RadGrid ID="grdCustomers" runat="server" GridLines="None"
                    AllowSorting="true" OnItemCreated="grdCustomers_ItemCreated" OnSortCommand="grdCustomers_SortCommand"
                    OnItemCommand="grdCustomers_ItemCommand"
                    OnNeedDataSource="grdCustomers_NeedDataSource">
                    <HeaderContextMenu>
                        <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                    </HeaderContextMenu>
                    <ClientSettings>
                        <Resizing AllowColumnResize="true" EnableRealTimeResize="true" ResizeGridOnColumnResize="true" ClipCellContentOnResize="false" />
                    </ClientSettings>
                    <MasterTableView AutoGenerateColumns="False" HierarchyLoadMode="ServerOnDemand" DataKeyNames="CustomerID"
                        Name="Master">
                        <RowIndicatorColumn>
                            <HeaderStyle Width="20px"></HeaderStyle>
                        </RowIndicatorColumn>
                        <ExpandCollapseColumn>
                            <HeaderStyle Width="20px"></HeaderStyle>
                        </ExpandCollapseColumn>
                        <AlternatingItemStyle CssClass="config_alternating_item" />
                        <Columns>
                            <telerik:GridTemplateColumn UniqueName="IDColumn" HeaderText="ID" SortExpression="CustomerID">
                                <ItemTemplate>
                                <asp:LinkButton ID="lbtnID" runat="server"
                                    CommandName="EditCustomer"
                                    CommandArgument='<%# DataItemAs<GridCustomer>(Container).CustomerID %>' />
                                <br />
                                <asp:Literal ID="ltCreatedOn" runat="server" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Name" UniqueName="NameColumn" SortExpression="Name">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbtnName" runat="server" 
                                    CommandName="EditCustomer" 
                                    CommandArgument='<%# DataItemAs<GridCustomer>(Container).CustomerID %>'/>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Order History" UniqueName="OrderColumn">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbtnOrderHistory" runat="server"/>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Admin" UniqueName="AdminColumn" SortExpression="Admin">
                                <ItemTemplate>
                                    <asp:Literal ID="ltIsAdmin" runat="server" />
                                    <br />
                                    <asp:Button ID="btnSetAdmin" CssClass="normalButtons" runat="server" Visible="false" />
                                    <br />
                                    <asp:Button ID="btnSetSuperAdmin" CssClass="normalButtons" runat="server" Visible="false" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="CustomerLevelID" HeaderText="Level" UniqueName="CustomerLevelColumn" SortExpression="CustomerLevelID">
                                <HeaderStyle HorizontalAlign="Center" Width="25px" />
                                <ItemStyle HorizontalAlign="Center" Width="25px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Subscription Expires On" UniqueName="SubscriptionColumn">
                                <ItemTemplate>
                                    <asp:Literal ID="ltSubscriptionExpiresOn" runat="server" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Email" UniqueName="EmailColumn" SortExpression="Email">
                                <ItemTemplate>
                                    <asp:Literal ID="ltEmailMailTo" runat="server" />
                                    <br />
                                    <asp:Literal ID="ltOkToEmail" runat="server" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Billing Address" UniqueName="AddressColumn">
                                <ItemTemplate>
                                    <asp:Literal ID="ltAddress" runat="server" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Poll Votes" UniqueName="DeletePollsColumn">
                                <ItemStyle HorizontalAlign="Center" />
                                <ItemTemplate>
                                    <asp:Button ID="btnDeletePolls" CssClass="normalButtons" Text="Delete Poll Votes" runat="server"
                                         CommandName="DeletePolls"
                                         CommandArgument='<%# DataItemAs<GridCustomer>(Container).CustomerID %>' />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Delete Customer" UniqueName="DeleteColumn">
                                <ItemStyle HorizontalAlign="Center" />
                                <ItemTemplate>
                                    <asp:Button ID="btnDelete" CssClass="normalButtons" Text="Delete" runat="server"
                                         CommandName="Delete"
                                         CommandArgument='<%# DataItemAs<GridCustomer>(Container).CustomerID %>' />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Nuke Customer" UniqueName="NukeColumn">
                                <ItemStyle HorizontalAlign="Center" />
                                <ItemTemplate>
                                    <asp:Button ID="btnNuke" CssClass="normalButtons" Text="Nuke" runat="server"
                                         CommandName="Nuke"
                                         CommandArgument='<%# DataItemAs<GridCustomer>(Container).CustomerID %>' />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn  HeaderText="Nuke and Ban Customer" UniqueName="NukeBanColumn">
                                <ItemStyle HorizontalAlign="Center" />
                                <ItemTemplate> 
                                    <asp:Button ID="btnNukeBan" CssClass="normalButtons" Text="Nuke and Ban" runat="server" 
                                         CommandName="NukeBan"
                                         CommandArgument='<%# DataItemAs<GridCustomer>(Container).CustomerID %>' />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                        </Columns>
                    </MasterTableView>
                    <FilterMenu>
                        <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                    </FilterMenu>
                </telerik:RadGrid>
            </ContentTemplate>
        </aspdnsf:SearcheableTemplate>
    </ContentTemplate>
</asp:UpdatePanel>
