<%@ Control Language="C#" AutoEventWireup="true" CodeFile="appconfiglist.ascx.cs"
    Inherits="AspDotNetStorefrontAdmin.Controls.AppConfigList" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls" Assembly="AspDotNetStorefrontControls" %>
<%@ Register TagPrefix="aspdnsf" TagName="EditAppConfigForAllStores" Src="editappconfigforallstores.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="AddAppConfig" Src="addappconfig.ascx" %>
<%@ Import Namespace="AspDotNetStorefrontCore" %>
<%@ Import Namespace="System.Linq" %>

<link type="text/css" href="css/redmond/jquery-ui-1.8.17.custom.css" rel="stylesheet" />

<%--
      We enclose the progress bar inside a div panel 
      so that we can control it's visibility
      since by default, the progress template will trigger upon
      every asynchronous postback request. Although there are 2
      async scenarios that this panel should be hidden, that is through
      1. Add AppConfig
      2. Save Appconfig
      Which we use animation extenders to render the progress template
      since they react faster to save button clicks and more suited
      to those specialized scenarios. Therefore for those 2 requests
      We hide the generic progress template
--%>
<div id="pnlLoadingGeneral" runat="server" style="float: left; margin-left: 400px;
    position: absolute;">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>
            <img runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/waiting.gif" />
        </ProgressTemplate>
    </asp:UpdateProgress>
</div>
<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.appconfig.ConfigGroups %>" />
<asp:DropDownList ID="cboConfigGroups" runat="server" AutoPostBack="true" OnSelectedIndexChanged="cboConfigGroups_SelectedIndexChanged">
</asp:DropDownList>
<asp:Button ID="btnAddConfig" runat="server" Text="Add AppConfig" />
<asp:Panel ID="pnlAddAppConfig" runat="server" DefaultButton="btnSaveNewConfig" CssClass="modal_popup">
    <div class="modal_popup_Header">
        <asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.appconfig.AddAppConfig %>" /></div>
    <aspdnsf:AddAppConfig ID="ctrlAddAppConfig" runat="server" CssClass="modal_popup_Content" AddMode="true" />
    <div align="center" class="modal_popup_Footer">
        <asp:Button ID="btnSaveNewConfig" runat="server" Text="Save" OnClick="btnSaveNewConfig_Click" />
        <asp:Button ID="btnCancelAddConfig" OnClick="btnCancelAddConfig_Click" runat="server"
            Text="Cancel" />
        <div id="divProgressAdd" runat="server" style="display: none">
            <p style="color: #3DDB0B; font-style: italic;">
                <asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.appconfig.SaveInProgress %>" /><img
                    runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/waiting.gif" />
            </p>
        </div>
    </div>
</asp:Panel>
<div style="display: none;">
    <asp:Button ID="btnHidden" runat="server" />
</div>
<ajax:ModalPopupExtender ID="extAppConfigPanel" runat="server" PopupControlID="pnlAddAppConfig"
    TargetControlID="btnAddConfig" BackgroundCssClass="modal_popup_background" CancelControlID="btnHidden">
</ajax:ModalPopupExtender>
<asp:Panel ID="pnlMultiStore" runat="server" Visible="false">
    Stores:
    <asp:DropDownList ID="cboStores" runat="server" AutoPostBack="true" OnSelectedIndexChanged="cboStores_SelectedIndexChanged">
    </asp:DropDownList>
</asp:Panel>
<aspdnsf:SearcheableTemplate runat="server" ID="ctrlSearch" OnFilter="ctrlSearch_Filter"
    OnContentCreated="ctrlSearch_ContentCreated">
    <Search SearchButtonCaption="Go" SearchCaption="<%$ Tokens: StringResource, common.cs.82 %>"
        SearchTextMinLength="<%$ Tokens:AppConfigUSInt, MinSearchStringLength %>" SearchTextMinLengthInvalidErrorMessage="<%$ Tokens: StringResource, search.aspx.2 %>"
        ValidateInputLength="false" ShowValidationMessageBox="false" ShowValidationSummary="false"
        UseLandingPageBehavior="false" WillRenderInUpdatePanel="true" OnSearchInvoked="ctrlSearch_SearchInvoked" />
    <ContentTemplate>
        <telerik:RadGrid ID="grdAppConfigs" runat="server" GridLines="None"
            AllowSorting="true" OnItemCreated="grdAppConfigs_ItemCreated" OnSortCommand="grdAppConfigs_SortCommand"
            OnItemCommand="grdAppConfigs_ItemCommand">
            <HeaderContextMenu>
                <CollapseAnimation Type="OutQuint" Duration="200"></CollapseAnimation>
            </HeaderContextMenu>
            <ClientSettings>
                <Resizing AllowColumnResize="true" EnableRealTimeResize="true" ResizeGridOnColumnResize="true" ClipCellContentOnResize="false" />
            </ClientSettings>
            <MasterTableView AutoGenerateColumns="False">
                <RowIndicatorColumn>
                    <HeaderStyle Width="20px"></HeaderStyle>
                </RowIndicatorColumn>
                <ExpandCollapseColumn>
                    <HeaderStyle Width="20px"></HeaderStyle>
                </ExpandCollapseColumn>
                <AlternatingItemStyle CssClass="config_alternating_item" />
                <Columns>
                    <telerik:GridTemplateColumn UniqueName="columnEdit" ItemStyle-Width="35px">
                        <ItemStyle HorizontalAlign="Center" />
                        <ItemTemplate>
                            <asp:ImageButton ID="imgEdit" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/edit.png" />
                            <asp:ImageButton Visible="<%# DataItemAs<AppConfig>(Container).StoreId != 0 %>" ID="cmdDelete" runat="server" ImageUrl="~/App_Themes/Admin_Default/images/delete.png"
                                CommandName="DeleteAppConfig" CommandArgument='<%# DataItemAs<AppConfig>(Container).AppConfigID %>'
                                OnClientClick="return confirm('Are you sure you want to delete this appconfig?');" />
                            <ajax:ModalPopupExtender ID="extAppConfigPanelEdit" runat="server" PopupControlID="pnlAppConfig"
                                TargetControlID="imgEdit" BackgroundCssClass="modal_popup_background" CancelControlID="btnCancel">
                            </ajax:ModalPopupExtender>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Store" UniqueName="ConfigIDColumn" ItemStyle-Width="60px" SortExpression="StoreId">
                        <HeaderStyle HorizontalAlign="Center" />
                        <ItemStyle HorizontalAlign="Center" />
                        <ItemTemplate>
                            <%# CommonLogic.IIF(DataItemAs<AppConfig>(Container).StoreId == 0, "<b>Default</b>", Store.GetStoreName(DataItemAs<AppConfig>(Container).StoreId))%>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridBoundColumn DataField="AppConfigID" HeaderText="Id" UniqueName="column"
                        ItemStyle-Width="40px" SortExpression="AppConfigID">
                        <HeaderStyle HorizontalAlign="Center" />
                        <ItemStyle HorizontalAlign="Center" />
                    </telerik:GridBoundColumn>
                    <telerik:GridTemplateColumn HeaderText="Name" UniqueName="TemplateColumn1" ItemStyle-Width="60px"
                        SortExpression="Name">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkName" runat="server" Text='<%# DataItemAs<AppConfig>(Container).Name %>'>
                            </asp:LinkButton>
                            <ajax:ModalPopupExtender ID="extAppConfigPanelName" runat="server" PopupControlID="pnlAppConfig"
                                TargetControlID="lnkName" BackgroundCssClass="modal_popup_background" CancelControlID="btnCancel">
                            </ajax:ModalPopupExtender>
                            <asp:Panel ID="pnlAppConfig" runat="server" DefaultButton="btnSave" CssClass="modal_popup">
                                <div class="modal_popup_Header">
                                            Edit AppConfig</div>
                                        <aspdnsf:EditAppConfigForAllStores ID="ctrlAppConfig" runat="server" CssClass="modal_popup_Content"
                                            ThisCustomer='<%# ThisCustomer %>' ConfigGroups='<%# ConfigGroups %>' Stores='<%# Stores %>'
                                            Datasource='<%# DataItemAs<AppConfig>(Container) %>' />
                                        <div align="center" class="modal_popup_Footer">
                                            <asp:Button ID="btnSave" runat="server" Text="Save" CommandName="UpdateAppConfig"
                                                CommandArgument='<%# DataItemAs<AppConfig>(Container).AppConfigID %>' />
                                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" />
                                            <%--
                                                            We'll use animation extender as update progress notification instead
                                                            since it reacts faster than the update progress and we can control
                                                            which particular sections we want to display the progress template 
                                                            based on which button clicked
                                                        --%>
                                            <div id="divProgressGrid" runat="server" style="display: none">
                                                <p style="color: #3DDB0B; font-style: italic;">
                                                    <asp:Literal ID="Literal3" runat="server" Text="<%$Tokens:StringResource, admin.appconfig.SaveInProgress %>" /><img
                                                        runat="server" alt="saving" src="~/App_Themes/Admin_Default/images/waiting.gif" />
                                                </p>
                                            </div>
                                        </div>
                            </asp:Panel>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Description" UniqueName="TemplateColumn2"
                        ItemStyle-Width="250px" SortExpression="Description">
                        <ItemTemplate>
                            <asp:Label ID="lblDescription" runat="server" Text='<%# DataItemAs<AppConfig>(Container).Description %>'></asp:Label>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Value" UniqueName="TemplateColumn3" ItemStyle-Width="150px"
                        SortExpression="ConfigValue">
                        <ItemTemplate>
                            <div id="test" runat="server" style="width: 100%;">
                                <asp:Label ID="lblConfigValue" runat="server" Text='<%# DataItemAs<AppConfig>(Container).ConfigValue %>'></asp:Label>
                            </div>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="GroupName" UniqueName="TemplateColumn" ItemStyle-Width="100px"
                        SortExpression="GroupName">
                        <ItemTemplate>
                            <asp:Label ID="lblGroupName" runat="server" Text='<%# DataItemAs<AppConfig>(Container).GroupName %>'></asp:Label>
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

<div id="lightboxDialog" title="AvaTax Connection Test"></div>

<script type="text/javascript" src="Scripts\jquery-ui-1.8.17.custom.min.js"></script>
<script type="text/javascript">
	$(document).ready(function () {

		$("#lightboxDialog").dialog({
			autoOpen: false,
			modal: true,
			height: 400,
			width: 700
		});

		$(".lightboxLink").click(function (event) {
			$("#lightboxDialog")
				.html("<iframe id='modalIframeId' width='100%' height='100%' marginWidth='0' marginHeight='0' frameBorder='0' scrolling='auto' />")
				.dialog("open");

			$("#modalIframeId")
				.attr("src", $(this).attr('href'));

			event.preventDefault();
		});

	});
</script>
