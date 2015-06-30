<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProductsGrid.ascx.cs"
    Inherits="AspDotNetStorefrontAdmin.Controls.ProductsGrid" %>
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
                <telerik:AjaxUpdatedControl ControlID="grdProducts" />
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="grdProducts">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="grdProducts" />
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<div id="pnlLoadingGeneral" runat="server" style="float: left; padding-left: 45%;
    position: absolute; width: 100%;">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>
            <img id="Img1" runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/waiting.gif" />
        </ProgressTemplate>
    </asp:UpdateProgress>
</div>

<asp:UpdatePanel runat="server" ID="updatePanelStatus" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Literal ID="litUpdateStatus" runat="server" Visible="false" />
    </ContentTemplate>
</asp:UpdatePanel>

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
         <div align="center" style="padding-top: 25px; padding-bottom: 5px; position: absolute; left: 1000px; top: 135px;">
             <asp:Button ID="btnAddnew" style="float:right" CssClass="normalButtons" runat="server" Text="<%$ Tokens: StringResource, common.cs.89 %>" OnClientClick="return ShowInsertForm();" />
        </div>

        <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch" OnFilter="ctrlSearch_Filter">
            <Search ID="Search1" runat="server" SearchButtonCaption="Go" SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>"
                SearchTextMinLength="3" SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>"
                ValidateInputLength="false" ShowValidationMessageBox="false" ShowValidationSummary="false"
                UseLandingPageBehavior="false" WillRenderInUpdatePanel="true" />
            <ContentTemplate>
                <telerik:RadGrid ID="grdProducts" runat="server" GridLines="None"
                    AllowSorting="true" OnItemCreated="grdProducts_ItemCreated" OnSortCommand="grdProducts_SortCommand"
                    OnItemCommand="grdProducts_ItemCommand" OnDetailTableDataBind="grdProducts_DetailTableDataBind"
                    OnNeedDataSource="grdProducts_NeedDataSource">
                    <HeaderContextMenu>
                        <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                    </HeaderContextMenu>
                    <ClientSettings>
                        <Resizing AllowColumnResize="true" EnableRealTimeResize="true" ResizeGridOnColumnResize="true" ClipCellContentOnResize="false" />
                    </ClientSettings>
                    <MasterTableView AutoGenerateColumns="False" HierarchyLoadMode="ServerOnDemand" DataKeyNames="ProductID"
                        Name="Master">
                        <RowIndicatorColumn>
                            <HeaderStyle Width="20px"></HeaderStyle>
                        </RowIndicatorColumn>
                        <ExpandCollapseColumn>
                            <HeaderStyle Width="20px"></HeaderStyle>
                        </ExpandCollapseColumn>
                        <AlternatingItemStyle CssClass="config_alternating_item" />
                        <DetailTables>
                            <telerik:GridTableView HierarchyLoadMode="ServerOnDemand" AutoGenerateColumns="False"
                                Name="ProductVariants" DataKeyNames="VariantID">
                                <RowIndicatorColumn>
                                    <HeaderStyle Width="20px"></HeaderStyle>
                                </RowIndicatorColumn>
                                <ExpandCollapseColumn>
                                    <HeaderStyle Width="20px"></HeaderStyle>
                                </ExpandCollapseColumn>
                                <ParentTableRelation>
                                    <telerik:GridRelationFields DetailKeyField="ProductID" MasterKeyField="ProductID" />
                                </ParentTableRelation>
                                <Columns>
                                    <telerik:GridTemplateColumn UniqueName="columnEdit">
                                        <ItemStyle HorizontalAlign="Center" Width="20px" />
                                        <HeaderStyle HorizontalAlign="Center" Width="20px" />
                                        <ItemTemplate>
                                            <asp:HyperLink runat="server" ID="lnkEditVariant" Text="Edit Variant" />
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn DataField="VariantID" HeaderText="VariantID" UniqueName="TemplateColumn4"
                                        SortExpression="VariantID">
                                        <HeaderStyle HorizontalAlign="Center" Width="25px" />
                                        <ItemStyle HorizontalAlign="Center" Width="25px" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="ProductID" Visible="false" HeaderText="ProductID"
                                        UniqueName="TemplateColumn3">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridTemplateColumn UniqueName="IsDefaultColumn" HeaderText="Default" SortExpression="IsDefault">
                                        <ItemStyle HorizontalAlign="Center" Width="30px" />
                                        <HeaderStyle HorizontalAlign="Center" Width="30px" />
                                        <ItemTemplate>
                                            <asp:LinkButton ID="cmdDefaultProductVariant" runat="server" CommandName="MakeDefaultProductVariant"
                                                CommandArgument='<%# DataItemAs<GridProductVariant>(Container).VariantID %>' />
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn HeaderText="Name" UniqueName="TemplateColumn1" SortExpression="LocaleName">
                                        <HeaderStyle HorizontalAlign="Left" Width="150px" />
                                        <ItemStyle HorizontalAlign="Left" Width="150px" />
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkName" runat="server" Text='<%# DataItemAs<GridProductVariant>(Container).LocaleName %>'>
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn DataField="SKUSuffix" HeaderText="SKU Suffix" UniqueName="SKUColumn"
                                        SortExpression="SKUSuffix">
                                        <HeaderStyle HorizontalAlign="Left" Width="75px" />
                                        <ItemStyle HorizontalAlign="Left" Width="75px" />
                                    </telerik:GridBoundColumn>
                                    <telerik:GridTemplateColumn HeaderText="Price" UniqueName="TemplateColumn1" SortExpression="Price">
                                        <HeaderStyle HorizontalAlign="Left" Width="75px" />
                                        <ItemStyle HorizontalAlign="Left" Width="75px" />
                                        <ItemTemplate>
                                            <asp:Literal ID="litPrice" runat="server" Text='<%# DataItemAs<GridProductVariant>(Container).Price %>'>
                                            </asp:Literal>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn HeaderText="SalePrice" UniqueName="TemplateColumn1" SortExpression="SalePrice">
                                        <HeaderStyle HorizontalAlign="Left" Width="75px" />
                                        <ItemStyle HorizontalAlign="Left" Width="75px" />
                                        <ItemTemplate>
                                            <asp:Literal ID="litSalePrice" runat="server" Text='<%# DataItemAs<GridProductVariant>(Container).SalePrice %>'>
                                            </asp:Literal>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="PublishedColumn" HeaderText="Published" SortExpression="Published">
                                        <ItemStyle HorizontalAlign="Center" Width="30px" />
                                        <HeaderStyle HorizontalAlign="Center" Width="30px" />
                                        <ItemTemplate>
                                            <asp:LinkButton ID="cmdPublishProductVariant" runat="server" CommandName="PublishProductVariant"
                                                CommandArgument='<%# DataItemAs<GridProductVariant>(Container).VariantID %>' />
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="DeleteColumn" HeaderText="Deleted">
                                        <ItemStyle HorizontalAlign="Center" Width="20px" />
                                        <HeaderStyle HorizontalAlign="Center" Width="20px" />
                                        <ItemTemplate>
                                            <asp:ImageButton ID="imgDeleteProductVariant" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/delete.png"
                                                CommandName="DeleteProductVariant" CommandArgument='<%# DataItemAs<GridProductVariant>(Container).VariantID %>'
                                                OnClientClick="return confirm('Are you sure you want to delete this product variant?');" />
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                </Columns>
                            </telerik:GridTableView>
                        </DetailTables>
                        <Columns>
                            <telerik:GridTemplateColumn UniqueName="columnEdit">
                                <ItemStyle HorizontalAlign="Center" Width="75px" />
                                <HeaderStyle HorizontalAlign="Center" Width="75px" />
                                <ItemTemplate>
                                        <asp:HyperLink runat="server" href="#" ID="lnkAddVariant" Text="Add Variant">
                                        </asp:HyperLink>
                                        &nbsp;
                                        <asp:HyperLink runat="server" ID="lnkEdit" Text="Edit">
                                        </asp:HyperLink>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="ProductID" HeaderText="ID" UniqueName="column"
                                SortExpression="ProductID">
                                <HeaderStyle HorizontalAlign="Center" Width="25px" />
                                <ItemStyle HorizontalAlign="Center" Width="25px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Name" UniqueName="TemplateColumn1" SortExpression="LocaleName">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkName" runat="server" Text='<%# DataItemAs<GridProduct>(Container).LocaleName %>' />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="SKU" HeaderText="SKU" UniqueName="SKUColumn"
                                SortExpression="SKU">
                                <HeaderStyle HorizontalAlign="Left" Width="75px" />
                                <ItemStyle HorizontalAlign="Left" Width="75px" />
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn UniqueName="PublishedColumn" HeaderText="Published">
                                <ItemStyle HorizontalAlign="Center" Width="30px" />
                                <HeaderStyle HorizontalAlign="Center" Width="30px" />
                                <ItemTemplate>
                                    <asp:LinkButton ID="cmdPublishProduct" runat="server" CommandName="PublishProduct"
                                        CommandArgument='<%# DataItemAs<GridProduct>(Container).ProductID %>' />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Clone" UniqueName="CloneColumn">
                                <ItemStyle HorizontalAlign="Center" Width="35px" />
                                <HeaderStyle HorizontalAlign="Center" Width="35px" />
                                <ItemTemplate>
                                    <asp:LinkButton ID="cmdCloneProduct" runat="server" CommandName="CloneProduct"
                                        CommandArgument='<%# DataItemAs<GridProduct>(Container).ProductID %>' Text="Clone" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Deleted" UniqueName="DeleteColumn">
                                <ItemStyle HorizontalAlign="Center" Width="35px" />
                                <HeaderStyle HorizontalAlign="Center" Width="35px" />
                                <ItemTemplate>
                                    <asp:ImageButton ID="imgDeleteProduct" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/delete.png"
                                        ToolTip="Delete Product" CommandName="DeleteProduct" CommandArgument='<%# DataItemAs<GridProduct>(Container).ProductID %>'
                                        OnClientClick="return confirm('Are you sure you want to delete this product?');" />
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Nuke" UniqueName="NukeColumn">
                                <ItemStyle HorizontalAlign="Center" Width="35px" />
                                <HeaderStyle HorizontalAlign="Center" Width="35px" />
                                <ItemTemplate>
                                    <asp:ImageButton ID="imgNukeProduct" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/bomb.png"
                                        ToolTip="Nuke Product" CommandName="NukeProduct" CommandArgument='<%# DataItemAs<GridProduct>(Container).ProductID %>'
                                        OnClientClick="return confirm('Are you sure you want to Nuke this product? You cannot undo this action.');" />
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
<telerik:RadWindowManager ID="RadWindowManager1" runat="server" Width="1050px" Height="750px" KeepInScreenBounds="true" DestroyOnClose="true" ReloadOnShow="true" 
    Behaviors="Close,Move,Resize" Modal="true">
    <Windows>
        <telerik:RadWindow ID="rdwEditProduct" runat="server" />
    </Windows>
</telerik:RadWindowManager>
