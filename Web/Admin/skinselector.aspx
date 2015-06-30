<%@ Page Language="C#" AutoEventWireup="true" CodeFile="skinselector.aspx.cs" Inherits="AspDotNetStorefrontAdmin.skinselector" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div class="skin-management-page page-wrap">
		<div class="page-row store-select-wrap">
			<div class="one-fourth">
				<label class="label">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.SkinSelector.StoreLabel %>" />
				</label>
				<asp:DropDownList runat="server" ID="StoreSelector" AutoPostBack="true" OnSelectedIndexChanged="StoreSelector_SelectedIndexChanged" CssClass="store-select-list"></asp:DropDownList>
				<asp:HiddenField runat="server" ID="SelectedStoreSkinName" />
			</div>
			<div class="three-fourths">
				<asp:Panel runat="server" ID="Message" Visible="false" CssClass="info-message">
					<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.SkinSelector.SkinSaved %>" />
				</asp:Panel>
			</div>
		</div>
		<div class="page-row">
			<div class="one-fourth">
				<div class="skin-navigation">
					<asp:Repeater runat="server" ID="SkinNavigationList" OnItemDataBound="SkinNavigation_ItemDataBound">
						<ItemTemplate>
							<a href='<%# "#" + Eval("Name") %>' class="skin-item-link" id='<%# "skin-item-link-" + Eval("Name") %>'>
								<asp:Label runat="server" ID="CurrentSkinIndicator" CssClass="skin-indicator" Visible="false"></asp:Label>
								<div class="skin-image-icon-wrap">
									<img id="SkinImageIcon" runat="server" src='<%# Eval("PreviewUrl") %>' alt='<%# Eval("Name") %>' class="skin-image-icon" />
								</div>
								<span class="skin-link-name">
									<asp:Literal runat="server" ID="DisplayName" Text='<%# Eval("DisplayName") %>' />
								</span>
								<div style="clear: both;"></div>
							</a>
						</ItemTemplate>
						<FooterTemplate>
							<div style="clear: both;"></div>
						</FooterTemplate>
					</asp:Repeater>
				</div>
			</div>
			<div class="three-fourths">
				<asp:Repeater runat="server" ID="SkinInfo" OnItemDataBound="SkinInfo_ItemDataBound">
					<ItemTemplate>
						<div id="<%# Eval("Name") %>" class="skin-info-item">
							<div class="action-bar">
								<asp:HyperLink runat="server" ID="PreviewSkin" CssClass="defaultButton" Text="Preview" Target="_blank" />
								<asp:Button runat="server" ID="SetSkin" OnCommand="SetSkin_Click" CssClass="emphasisButton" Text="Apply Skin" CommandArgument='<%# Eval("Name") %>' />
							</div>
							<div class="skin-description-wrap">
								<span class="skin-name">
									<asp:Literal runat="server" ID="DisplayName" Text='<%# Eval("DisplayName") %>' />
								</span>
								<span id="SkinDescriptionContainer" runat="server" class="skin-description">
									- <asp:Literal runat="server" ID="Description" Text='<%# Eval("Description") %>' />
								</span>
							</div>
							<div class="skin-image-wrap">
								<asp:Panel runat="server" ID="NoPreviewAvailable" Visible="false" CssClass="no-image-preview">
									<asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.SkinSelector.NoImage %>" />
								</asp:Panel>
								<img id="SkinImage" runat="server" src='<%# Eval("PreviewUrl") %>' alt='<%# Eval("Name") %>' class="skin-image" />
							</div>
						</div>
					</ItemTemplate>
					<FooterTemplate>
						<div style="clear: both;"></div>
					</FooterTemplate>
				</asp:Repeater>
			</div>
		</div>

	</div>
	<script src="Scripts/jquery.min.js"></script>
	<script type="text/javascript">
		var selectedStoreSkinName = $('input[id$="_SelectedStoreSkinName"]').val();
		setSkinView(selectedStoreSkinName);

		$('.skin-item-link').click(function (event) {
			var href = $(this).attr('href');
			setSkinView(href.substr(href.indexOf("#")+1));
			event.preventDefault();
		});

		function setSkinView(skin) {
			$('.skin-info-item').each(function () {
				if($(this).attr('id') === skin) {
					$(this).show();
				}
				else {
					$(this).hide();
				}
			});

			$('.skin-item-link').each(function () {
				if($(this).attr('id') === 'skin-item-link-' + skin) {
					$(this).addClass('selected');
				}
				else {
					$(this).removeClass('selected');
				}
			});
		}
	</script>
</asp:Content>
