<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityProductMap.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.EntityProductMap" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>

<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>

<script type="text/javascript" >
    function mapAll(map) {
        var chks = document.getElementsByTagName('input');
        for (var ctr = 0; ctr < chks.length; ctr++) {
            var chk = chks[ctr];
            if (chk.parentNode.className == 'map_check') {
                chk.checked = map;
            }
        }
    }
</script>

<div>
    <div style="text-indent: 50px;">
        <asp:CheckBox runat="server" ID="chkShowSelectedOnly" Checked="true" CssClass="IgnoreMapSelect" Text="<%$ Tokens: StringResource, admin.showselected %>" OnCheckedChanged="chkShowSelectedOnly_CheckedChanged" AutoPostBack="true" />
       </div>
    <aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch" 
        OnFilter="ctrlSearch_Filter"
        OnContentCreated="ctrlSearch_ContentCreated">
        
        <Search SearchButtonCaption="Go" 
			SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>"
            SearchTextMinLength="<%$ Tokens:AppConfigUSInt, MinSearchStringLength %>" 
            SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>"
            ValidateInputLength="false" 
            ShowValidationMessageBox="false" 
            ShowValidationSummary="false"
            UseLandingPageBehavior="false" 
            WillRenderInUpdatePanel="true" />
            
        <ContentTemplate>
        
			<div style="position:relative; float:right; top: -17px; width: 500px;">
				<asp:Label ID="lblHelp" runat="server" />
			</div>
			
            <telerik:RadGrid ID="grdMap" runat="server" GridLines="None" AllowSorting="false"
                OnItemCreated="grdMap_ItemCreated" 
                OnSortCommand="grdMap_SortCommand" 
                OnItemCommand="grdMap_ItemCommand">
                <HeaderContextMenu>
                    <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
                </HeaderContextMenu>
                <ClientSettings>
                    <Resizing AllowColumnResize="true" EnableRealTimeResize="true" ResizeGridOnColumnResize="true" ClipCellContentOnResize="false" />
                </ClientSettings>
                <MasterTableView AutoGenerateColumns="False" DataKeyNames="ProductID">
                    <RowIndicatorColumn>
                        <HeaderStyle Width="20px"></HeaderStyle>
                    </RowIndicatorColumn>
                    <ExpandCollapseColumn>
                        <HeaderStyle Width="20px"></HeaderStyle>
                    </ExpandCollapseColumn>
                    <AlternatingItemStyle CssClass="config_alternating_item" />
                    <Columns>
                        <telerik:GridTemplateColumn UniqueName="columnEdit" ItemStyle-Width="150px" HeaderStyle-HorizontalAlign="Center" HeaderStyle-VerticalAlign="Middle" HeaderStyle-Height="80px">
                            <HeaderTemplate>
                                <div class="map_header">
                                    Map: <a href="javascript:void(0);" onclick="javascript:mapAll(true);">All,</a>&nbsp;
                                        <a href="javascript:void(0);" onclick="javascript:mapAll(false);">None</a>&nbsp;|&nbsp;
                                        <asp:LinkButton ID="lnkSave" runat="server" CommandName="SaveMapping"><img id="Img1" runat="server" src="~/App_Themes/Admin_Default/images/save_small.gif" alt="save" style="border-style:none; margin: 0px 0px -3px 0;" /> Save</asp:LinkButton>
                                </div>
                            </HeaderTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                            <ItemTemplate>
                                <%--hidden field for reference later during update--%>
                                <asp:HiddenField ID="txtProductId" runat="server" Value='<%# DataItemAs<AspDotNetStorefrontAdmin.Controls.ProductEntityMapInfo>(Container).ProductId %>' />
                                <asp:CheckBox ID="chkIsMapped" runat="server" CssClass="map_check"  Checked='<%# DataItemAs<AspDotNetStorefrontAdmin.Controls.ProductEntityMapInfo>(Container).IsMapped %>' />
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="ID" UniqueName="TemplateColumn1" ItemStyle-Width="50px" SortExpression="Name">
                            <ItemTemplate>
                                <asp:Label ID="lblID" runat="server" Text='<%# DataItemAs<AspDotNetStorefrontAdmin.Controls.ProductEntityMapInfo>(Container).ProductId %>'></asp:Label>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Name" SortExpression="Name">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkName" runat="server" Text='<%# DataItemAs<AspDotNetStorefrontAdmin.Controls.ProductEntityMapInfo>(Container).Name %>'>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        
                         <telerik:GridTemplateColumn HeaderText="Clone" ItemStyle-Width="50px" HeaderStyle-Width="50px" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" CommandName="CloneProducts" ToolTip="Clone Product" OnClientClick="return confirm('Are you sure you want to clone this product?');"><img id="Img2" runat="server" src="~/App_Themes/Admin_Default/images/clone.png" alt="Clone Product" style="border-style:none; margin: 0px 0px -3px 0;" /></asp:LinkButton>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        
                        <telerik:GridTemplateColumn HeaderText="Soft Delete" ItemStyle-Width="50px" HeaderStyle-Width="50px" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteProducts" ToolTip="Delete Product" OnClientClick="return confirm('Are you sure you want to mark this product as deleted?');" Visible="false"><img id="Img3" runat="server" src="~/App_Themes/Admin_Default/images/delete.png" alt="Delete Product" style="border-style:none; margin: 0px 0px -3px 0;" /> </asp:LinkButton>
                                <asp:LinkButton ID="lnkUndelete" runat="server" CommandName="UndeleteProducts" ToolTip="Undelete Product" OnClientClick="return confirm('Are you sure you want to undelete this product?');" Visible="false"><img id="Img4" runat="server" src="~/App_Themes/Admin_Default/images/undelete.png" alt="Undelete Product" style="border-style:none; margin: 0px 0px -3px 0;" /> </asp:LinkButton>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        
                        <telerik:GridTemplateColumn HeaderText="Nuke" ItemStyle-Width="50px" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkNuke" runat="server" CommandName="NukeProducts" ToolTip="Nuke Product" OnClientClick="return confirm('Are you sure you want to nuke this product? This cannot be undone.');"><img id="Img5" runat="server" src="~/App_Themes/Admin_Default/images/delete.png" alt="Nuke Product" style="border-style:none; margin: 0px 0px -3px 0;" /></asp:LinkButton>
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

    <telerik:RadWindowManager ID="RadWindowManager1" runat="server" Width="1050px" Height="750px" KeepInScreenBounds="true" DestroyOnClose="true" ReloadOnShow="true"
        Behaviors="Close,Move,Resize" Modal="true">
        <Windows>
            <telerik:RadWindow ID="rdwEditProduct" runat="server" />
        </Windows>
    </telerik:RadWindowManager>
</div>
