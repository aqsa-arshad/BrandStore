<%@ Page Language="c#" Inherits="AspDotNetStorefront.rateit" CodeFile="rateit.aspx.cs" EnableTheming="false" StylesheetTheme="" %>

<html>
<head runat="server">
	<title>Rate Product</title>
	<asp:Literal ID="ltStylesheet" runat="server" />
</head>
<body>
	<div class="page-wrap ratings-page-wrap">
		<div class="list-item-inner">
			<form runat="server" id="RateItForm" onsubmit="return FormValidator(this)">
				<div class="group-header rating-header">
					<asp:Label ID="rateit_aspx_3" runat="server" CssClass="rateitlabel"></asp:Label>
					<asp:Label ID="lblProductName" runat="server"></asp:Label>
				</div>
				<div class="page-row bad-words-label">
					<asp:Label ID="rateit_aspx_4" runat="server" CssClass="rateitlabel" Visible="false"></asp:Label>
				</div>
				<div class="form rating-form">
					<div class="group-header form-header rating-header">
						<asp:Label ID="rateit_aspx_5" runat="server" CssClass="rateitlabel"></asp:Label>
					</div>

					<div class="page-row">
						<div class="one-fifth">
							<a href="javascript:void()" onclick="return newRatingEntered(1);">
								<asp:Image ID="Star1" Width="30" Height="30" hspace="2" runat="server" /></a>
						</div>
						<div class="one-fifth">
							<a href="javascript:void()" onclick="return newRatingEntered(2);">
								<asp:Image ID="Star2" Width="30" Height="30" hspace="2" runat="server" /></a>
						</div>
						<div class="one-fifth">
							<a href="javascript:void()" onclick="return newRatingEntered(3);">
								<asp:Image ID="Star3" Width="30" Height="30" hspace="2" runat="server" /></a>
						</div>
						<div class="one-fifth">
							<a href="javascript:void()" onclick="return newRatingEntered(4);">
								<asp:Image ID="Star4" Width="30" Height="30" hspace="2" runat="server" /></a>
						</div>
						<div class="one-fifth">
							<a href="javascript:void()" onclick="return newRatingEntered(5);">
								<asp:Image ID="Star5" Width="30" Height="30" hspace="2" runat="server" /></a>
						</div>
					</div>

					<div class="form-group">
						<asp:DropDownList ID="rating" class="form-control" runat="server" onchange="newRatingEntered(this.value)"></asp:DropDownList>
						<div class="form-text">
							<asp:Label ID="rateit_aspx_12" runat="server" class="rateittext"></asp:Label>
						</div>
					</div>
					<div class="form-group">
						<label>
							<asp:Label ID="rateit_aspx_13" runat="server" CssClass="rateitlabel"></asp:Label>
						</label>
						<asp:TextBox ID="Comments" CssClass="form-control" Rows="6" runat="server" TextMode="MultiLine"></asp:TextBox>
					</div>
					<div class="form-submit-wrap">
						<asp:Button ID="btnSubmit" CssClass="button call-to-action" runat="server" />
						<asp:Button ID="btnCancel" CssClass="button cancel-button" OnClientClick="javascript:document.getElementById('RateItForm').onsubmit = function(){};self.close();" runat="server" />
					</div>
				</div>
			</form>
		</div>
	</div>
</body>
</html>
