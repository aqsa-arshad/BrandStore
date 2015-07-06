<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityGrid.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.EntityGrid" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="AspDotNetStorefrontLayout" %>
<%@ Import Namespace="AspDotNetStorefrontAdmin.Controls" %>
<%@ Import Namespace="System.Linq" %>
<telerik:RadAjaxManager ID="radAjaxMgr" runat="server" OnAjaxRequest="radAjaxMgr_OnAjaxRequest">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="radAjaxMgr">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="grdEntities" />
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="grdEntities">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="grdEntities" />
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>
<div id="pnlLoadingGeneral" runat="server" style="float: left; padding-left: 45%;
    position: absolute; width: 100%;">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>
            <img id="Img1" runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/waiting.gif" height="25" />
        </ProgressTemplate>
    </asp:UpdateProgress>
</div>
<div style="display: none;">
    <asp:Button ID="btnHidden" runat="server" />
</div>
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
        <div align="center" style="padding-top: 25px; padding-bottom: 5px; position: absolute; left: 280px; top: 135px;">
            <asp:Button ID="btnDisplayOrder" runat="server" OnClientClick="return ShowDisplayOrderForm();"
                class="normalButtons" Text="<%$ Tokens:StringResource, admin.newentities.EditDisplayOrder %>" />
        </div>
                 <div align="center" style="padding-top: 25px; padding-bottom: 5px; position: absolute; left: 500px; top: 135px;">
             <asp:Button ID="btnAddnew" style="float:right" CssClass="normalButtons" runat="server" Text="<%$ Tokens: StringResource, common.cs.90 %>" OnClientClick="return ShowInsertForm();" />
        </div>
        <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch" OnFilter="ctrlSearch_Filter">
            <Search ID="Search1" runat="server" SearchButtonCaption="Go" SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>"
                SearchTextMinLength="3" SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>"
                ValidateInputLength="false" ShowValidationMessageBox="false" ShowValidationSummary="false"
                UseLandingPageBehavior="false" WillRenderInUpdatePanel="true" />
            <ContentTemplate>
				<asp:CheckBox ID="chkShowDeleted" runat="server" Text="Show Deleted" AutoPostBack="true" OnCheckedChanged="chkShowDeleted_CheckChanged" />				
                <telerik:RadGrid ID="grdEntities" runat="server" GridLines="None"
                    AllowSorting="false" OnItemCreated="grdEntities_ItemCreated"
                    OnItemCommand="grdEntities_ItemCommand" OnDetailTableDataBind="grdEntities_DetailTableDataBind"
                    OnNeedDataSource="grdEntities_NeedDataSource">
                
				    <HeaderContextMenu>
                        <CollapseAnimation Type="OutQuint" Duration="200" />
                    </HeaderContextMenu>
                    <FilterMenu>
                        <CollapseAnimation Type="OutQuint" Duration="200" />
                    </FilterMenu>
                    <ClientSettings>
                        <Resizing AllowColumnResize="true" EnableRealTimeResize="true" ResizeGridOnColumnResize="false" ClipCellContentOnResize="false" />
                    </ClientSettings>

                    <MasterTableView AutoGenerateColumns="false" HierarchyLoadMode="ServerOnDemand" DataKeyNames="ID" 
					    Name="Master" SelfHierarchySettings-KeyName="ID" SelfHierarchySettings-ParentKeyName="ParentID"
                        FilterExpression="ParentID=0" ExpandCollapseColumn-Visible="true">

                        <RowIndicatorColumn>
                            <HeaderStyle Width="20" />
                        </RowIndicatorColumn>

                        <AlternatingItemStyle CssClass="config_alternating_item" />
                        <Columns>
                            <telerik:GridTemplateColumn UniqueName="Edit">
                                <HeaderStyle HorizontalAlign="Center" Width="75" />
                                <ItemStyle HorizontalAlign="Center" Width="75" />
                                <ItemTemplate>
                                    <asp:ImageButton ID="imgAdd" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/add.png"
                                        CommandName="AddEntity" CommandArgument='<%# DataItemAs<GridEntity>(Container).ID %>'
                                        OnDataBinding="btnAdd_DataBinding" />
                                    <asp:ImageButton ID="imgEdit" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/edit.png"
                                        CommandName="EditEntity" CommandArgument='<%# DataItemAs<GridEntity>(Container).ID %>'
                                        OnDataBinding="btnEdit_DataBinding" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="ID" HeaderText="ID" UniqueName="IDColumn" SortExpression="ID">
                                <HeaderStyle HorizontalAlign="Center" Width="25" />
                                <ItemStyle HorizontalAlign="Center" Width="25" />
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="ParentID" HeaderText="ParentID" UniqueName="ParentIDColumn" SortExpression="ParentID" Visible="false">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn UniqueName="LocaleName" HeaderText="Name" SortExpression="LocaleName">
                                <HeaderStyle HorizontalAlign="Left" />
                                <ItemStyle HorizontalAlign="Left" />
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkName" runat="server" Text='<%# DataItemAs<GridEntity>(Container).LocaleName %>'
                                        OnDataBinding="btnName_DataBinding" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn UniqueName="Published" HeaderText="Published" SortExpression="Published">
                                <HeaderStyle HorizontalAlign="Center" Width="30" />
                                <ItemStyle HorizontalAlign="Center" Width="30" />
                                <ItemTemplate>
                                    <asp:LinkButton ID="cmdPublish" runat="server"
                                        CommandArgument='<%# DataItemAs<GridEntity>(Container).ID %>' />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn UniqueName="Delete" HeaderText="Delete" SortExpression="Deleted">
                                <HeaderStyle HorizontalAlign="Center" Width="20" />
                                <ItemStyle HorizontalAlign="Center" Width="20" />
                                <ItemTemplate>
                                    <asp:ImageButton ID="imgDelete" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/delete.png"
                                        ToolTip="Delete Entity" CommandName="DeleteEntity" CommandArgument='<%# DataItemAs<GridEntity>(Container).ID %>'
                                        OnClientClick="return confirm('Are you sure you want to delete this entity?');" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                        </Columns>
                    </MasterTableView>

                </telerik:RadGrid>
                <asp:Panel ID="pnlGrid" runat="server">
                </asp:Panel>
            </ContentTemplate>
        </aspdnsf:SearcheableTemplate>
    </ContentTemplate>
</asp:UpdatePanel>
<telerik:RadWindowManager ID="RadWindowManager1" runat="server" Width="1050px" Height="750px"
    KeepInScreenBounds="true" DestroyOnClose="true" ReloadOnShow="true" Behaviors="Close,Move,Resize"
    Modal="true">
    <Windows>
        <telerik:RadWindow ID="rdwEditEntity" runat="server" />
    </Windows>
</telerik:RadWindowManager>
