<%@ Page Language="C#" AutoEventWireup="true" CodeFile="phoneorder.aspx.cs" Inherits="AspDotNetStorefrontAdmin.phoneorder" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="aspdnsf" Namespace="AspDotNetStorefrontControls.Validators" Assembly="AspDotNetStorefrontControls"  %>

<asp:Content ContentPlaceHolderID="bodyContentPlaceholder" runat="server">
<script type="JavaScript">
    function getDelete()
    {
        return 'Confirm Delete';
    }
</script>
    <asp:Literal ID="saveOrderNumber" runat="server" Visible="false" />
    <div id="helpphone" runat="server" visible="true">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="helporderedit" runat="server" visible="false">
            <table border="0" cellpadding="1" cellspacing="0" class="outerTable">
            <tr>
                <td>
                    <div class="wrapper">
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable">
                            <tr>
                                <td class="contentTable">
                                    <font class="title">&nbsp;&nbsp;<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.EditOrderNumber %>" /></font>&nbsp;<asp:Label ID="txtordernumber" runat="server" />&nbsp;&nbsp;<asp:HyperLink runat="server" ID="backtoorderlink"  Text="Cancel"/></td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="Literal1" runat="server"></asp:Literal>
        </div>
    </div>
    <asp:Panel runat="server" ID="TopPanel" DefaultButton="Button2">
    <div id="content" runat="server">
        <table border="0" cellpadding="0" cellspacing="0" class="" width="100%">
            <tr>
                <td>
                    <div><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.FindExistingCustomer %>" />
                        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TextBox1"
                            ErrorMessage="<%$Tokens:StringResource, admin.phoneorder.validator.RequiredMessage %>" SetFocusOnError="True" ValidationGroup="Search" Font-Bold="True"></asp:RequiredFieldValidator>
                        <asp:Button ID="Button2" runat="server" CssClass="normalButtons" Text="Search" OnClick="Button2_Click1" ValidationGroup="Search" />
                        &nbsp; &nbsp;&nbsp; or &nbsp; &nbsp;&nbsp; &nbsp;<asp:Button ID="Button1" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.CreateNewCustomer %>" OnClick="Button1_Click" /><br />
                        <small><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.CustomerSearch %>" /></small></div>
                </td>
            </tr>
        </table>
    </div>
    </asp:Panel>
        <asp:Panel ID="SearchCustomerPanel" runat="server" Width="100%" Visible="False">
        <asp:Literal runat="server" ID="SQLText" Visible="false"></asp:Literal><br />
            &nbsp;<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.MatchingCustomers %>" /><br />
            <asp:GridView ID="GridView1" runat="server" DataKeyNames="CustomerID" OnRowCommand="GridView1_RowCommand" AutoGenerateColumns="False" Width="50%">
                <Columns>
                    <asp:BoundField DataField="CustomerID" HeaderText="<%$Tokens:StringResource, admin.common.CustomerID %>" >
                        <HeaderStyle HorizontalAlign="Left" />
                    </asp:BoundField>
                    <asp:BoundField DataField="FirstName" HeaderText="<%$Tokens:StringResource, admin.phoneorder.header.FirstName %>" >
                        <HeaderStyle HorizontalAlign="Left" />
                    </asp:BoundField>
                    <asp:BoundField DataField="LastName" HeaderText="<%$Tokens:StringResource, admin.phoneorder.header.LastName %>" >
                        <HeaderStyle HorizontalAlign="Left" />
                    </asp:BoundField>
                    <asp:BoundField DataField="EMail" HeaderText="<%$Tokens:StringResource, admin.menu.MailingTest %>" >
                        <HeaderStyle HorizontalAlign="Left" />
                    </asp:BoundField>
                    <asp:ButtonField ButtonType="Button" CommandName="Select" Text="<%$Tokens:StringResource, admin.phoneorder.button.Select %>" ControlStyle-CssClass="normalButtons">
                        <ControlStyle Font-Size="X-Small" />
                        <ItemStyle HorizontalAlign="Center" />
                        <HeaderStyle HorizontalAlign="Center" />                        
                    </asp:ButtonField>
                </Columns>
                <HeaderStyle CssClass="table-header" />
                <AlternatingRowStyle CssClass="table-alternatingrow2" />
                <RowStyle CssClass="table-row2" />
            </asp:GridView>
        </asp:Panel>
        <asp:Panel ID="CreateNewCustomerPanel" runat="server" Width="100%" Visible="False">
            <table class="innerTable" cellpadding="0" width="100%">
                <tr>
                    <td>
                        <table border="0" cellpadding="0" cellspacing="0" width="100%">
                        <tr><td colspan="4" style="height: 19px"><b><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.CustomerSearch %>" /></b><div ID="CustomerIDPanel" style="display:inline;" runat="server" visible="false">&nbsp;&nbsp;<b><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.CustomerID %>" /><asp:Literal ID="CustomerID" runat="server" />)</b></div></td></tr>
                            <tr>
                                <td align="right" valign="top">
                                    <div style=" margin-top:7px;">
                                        <asp:Label ID="Label3" runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.FirstName %>" />
                                    </div>
                                </td>
                                <td align="left" valign="middle">
                                    <asp:TextBox TabIndex="1" ID="FirstName" runat="server" Width="200" autocomplete="off"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator123" runat="server" ControlToValidate="FirstName" ErrorMessage="First Name Required!" ></asp:RequiredFieldValidator>
                                </td>
                                <td align="right" valign="top">
                                    <div style=" margin-top:7px;">
                                        <asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.LastName %>" />
                                    </div>
                                </td>
                                <td align="left" valign="middle">
                                    <asp:TextBox TabIndex="2" ID="LastName" runat="server" Width="200" autocomplete="off"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="LastName" ErrorMessage="Last Name Required!" ></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td align="right" valign="top" style="height: 43px;">
                                <div style=" margin-top:7px;">
                                    <asp:Label ID="lblRequiredEmailAsterix" runat="server" Visible="false">*</asp:Label>
                                    <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Email %>" />
                                    </div>
                                </td>
                                <td align="left" valign="middle" style="height: 43px">
                                    <asp:TextBox TabIndex="3" ID="EMail" runat="server" Width="200" autocomplete="off"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredEmailValidator" Enabled="false" runat="server" ControlToValidate="EMail"><br />E-Mail Required!</asp:RequiredFieldValidator>
                                    <aspdnsf:EmailValidator ID="valRegExValEmail" ControlToValidate="EMail" runat="SERVER" ErrorMessage="<%$Tokens:StringResource, admin.common.ValidE-MailAddressPrompt %>" />
                                    <asp:Label ID="EMailAlreadyTaken" Text="<%$Tokens:StringResource, admin.phoneorder.label.EmailTaken %>" Visible="False" runat="server" Font-Bold="True" Font-Size="X-Small" ForeColor="Red"></asp:Label>
                                </td>
                                <td align="right" valign="top" style="height: 43px">
                                <div style=" margin-top:7px;">
                                *<asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Phone %>" /></div></td>
                                <td align="left" valign="middle" style="height: 43px">
                                <asp:TextBox TabIndex="4" ID="Phone" runat="server" Width="200" autocomplete="off"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator15" runat="server" ControlToValidate="Phone"><br />Phone Number Required!</asp:RequiredFieldValidator>
                                </td>
                             </tr>
                            <tr>
                                <td align="right" valign="middle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.OkToEmail %>" /></td>
                                <td align="left" valign="middle"><asp:RadioButtonList TabIndex="5" ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal">
                                        <asp:ListItem Selected="True" Value="Yes" Text="<%$Tokens:StringResource, admin.common.Yes %>"></asp:ListItem>
                                        <asp:ListItem Text="<%$Tokens:StringResource, admin.common.No %>"></asp:ListItem></asp:RadioButtonList>
                                        </td>
                                <td align="right" valign="middle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Over13Years %>" /></td>
                                <td align="left" valign="middle"><asp:CheckBox TabIndex="6" ID="Over13" runat="server" Checked="True" /></td>
                            </tr>
                            <tr>
                                <td align="right" valign="middle"><asp:Literal ID="AffiliatePrompt" Text="<%$Tokens:StringResource, admin.order.Affiliate %>" runat="server"></asp:Literal></td>
		                        <td width="25%"><asp:DropDownList TabIndex="7" ID="AffiliateList" runat="server" DataTextField="Name" DataValueField="AffiliateID"></asp:DropDownList></td>
                                <td align="right" valign="middle"><asp:Literal ID="CustomerLevelPrompt" Text="<%$Tokens:StringResource, admin.phoneorder.CustomerLevel %>" runat="server"></asp:Literal></td>
		                        <td width="25%"><asp:DropDownList TabIndex="8" ID="CustomerLevelList" runat="server" DataTextField="Name" DataValueField="CustomerLevelID"></asp:DropDownList></td>
                            </tr>
                            <tr>
                            <td colspan="2">
                                 <br /><b><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.BillingInfo %>" /><asp:Button TabIndex="9" ID="btnCopyAccount" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.CopyFromAccount %>" /></b><br /><br /></td><td colspan="2"><br />
                                <b><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.ShippingInfo %>" /><asp:Button TabIndex="10" ID="btnCopyBilling" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.CopyFromBilling %>" /></b><br /><br />
                                </td>
                            </tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.FirstName %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="11" runat="server" ID="BillingFirstName" maxLength="50" autocomplete="off"></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="BillingFirstName" ErrorMessage="First Name Required!" ></asp:RequiredFieldValidator>
		    </td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.FirstName %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="31" runat="server" ID="ShippingFirstName" maxLength="50" autocomplete="off"></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="ShippingFirstName" ErrorMessage="First Name Required!" ></asp:RequiredFieldValidator>
		</td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.LastName %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="12" runat="server" ID="BillingLastName" maxLength="50" autocomplete="off"></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="BillingLastName" ErrorMessage="Last Name Required!" ></asp:RequiredFieldValidator>
		</td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.LastName %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="32" runat="server" ID="ShippingLastName" maxLength="50" autocomplete="off"></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="ShippingLastName" ErrorMessage="Last Name Required!" ></asp:RequiredFieldValidator>
		</td>
	</tr>
	<tr>
		<td align="right">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Phone %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="13" runat="server" ID="BillingPhone" maxLength="25" autocomplete="off"></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="BillingPhone" ErrorMessage="Phone Number Required!" ></asp:RequiredFieldValidator>
		</td>
		<td align="right">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Phone %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="33" runat="server" ID="ShippingPhone" maxLength="25" autocomplete="off"></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator14" runat="server" ControlToValidate="ShippingPhone" ErrorMessage="Phone Number Required!" ></asp:RequiredFieldValidator>
		</td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.Company %>" /></td>
		<td width="25%"><asp:TextBox TabIndex="14" runat="server" ID="BillingCompany" maxLength="100" autocomplete="off" ></asp:TextBox></td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.Company %>" /></td>
		<td width="25%"><asp:TextBox TabIndex="34" runat="server" ID="ShippingCompany" maxLength="100" autocomplete="off" ></asp:TextBox></td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.AddressType %>" /></td>
		<td width="25%"><asp:DropDownList TabIndex="15" ID="BillingResidenceType" runat="server"><asp:ListItem Value="0">Unknown</asp:ListItem><asp:ListItem Value="1" selected=True>Residential</asp:ListItem><asp:ListItem Value="2">Commercial</asp:ListItem></asp:DropDownList></td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.AddressType %>" /></td>
		<td width="25%"><asp:DropDownList TabIndex="35" ID="ShippingResidenceType" runat="server"><asp:ListItem Value="0">Unknown</asp:ListItem><asp:ListItem Value="1" selected=True>Residential</asp:ListItem><asp:ListItem Value="2">Commercial</asp:ListItem></asp:DropDownList></td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Address1 %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="16" runat="server" ID="BillingAddress1" maxLength="100" autocomplete="off" ></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="BillingAddress1" ErrorMessage="Address 1 Required!" ></asp:RequiredFieldValidator>
		</td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Address1 %>" /></td>
		<td width="25%"><asp:TextBox TabIndex="36" runat="server" ID="ShippingAddress1" maxLength="100" autocomplete="off" ></asp:TextBox>
		   <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ControlToValidate="ShippingAddress1" ErrorMessage="Address 1 Required!" ></asp:RequiredFieldValidator>
		</td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Address2 %>" /></td>
		<td width="25%"><asp:TextBox TabIndex="17" runat="server" ID="BillingAddress2" maxLength="100" autocomplete="off" ></asp:TextBox></td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Address2 %>" /></td>
		<td width="25%"><asp:TextBox TabIndex="37" runat="server" ID="ShippingAddress2" maxLength="100" autocomplete="off" ></asp:TextBox></td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Suite %>" /></td>
		<td width="25%"><asp:TextBox TabIndex="18" runat="server" ID="BillingSuite" maxLength="50" autocomplete="off" ></asp:TextBox></td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Suite %>" /></td>
		<td width="25%"><asp:TextBox TabIndex="38" runat="server" ID="ShippingSuite" maxLength="50" autocomplete="off" ></asp:TextBox></td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.City %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="19" runat="server" ID="BillingCity" maxLength="50" autocomplete="off" ></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ControlToValidate="BillingCity" ErrorMessage="City Required!" ></asp:RequiredFieldValidator>
		</td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.City %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="39" runat="server" ID="ShippingCity" maxLength="50" autocomplete="off" ></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ControlToValidate="ShippingCity" ErrorMessage="City Required!" ></asp:RequiredFieldValidator>
		</td>
	</tr>
	<tr>
		<td align="right" style="height: 22px"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.State %>" /></td>
		<td style="height: 22px"><asp:DropDownList TabIndex="20" ID="BillingState" runat="server" DataTextField="Name" DataValueField="Abbreviation"></asp:DropDownList></td>
		<td align="right" style="height: 22px"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.State %>" /></td>
		<td style="height: 22px"><asp:DropDownList TabIndex="40" ID="ShippingState" runat="server" DataTextField="Name" DataValueField="Abbreviation"></asp:DropDownList></td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Zip %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="21" runat="server" ID="BillingZip" maxLength="10" autocomplete="off"></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ControlToValidate="BillingZip" ErrorMessage="Zip Required!" ></asp:RequiredFieldValidator>
		</td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Zip %>" /></td>
		<td width="25%">
		    <asp:TextBox TabIndex="41" runat="server" ID="ShippingZip" maxLength="10" autocomplete="off"></asp:TextBox>
		    <asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" ControlToValidate="ShippingZip" ErrorMessage="Zip Required!" ></asp:RequiredFieldValidator>
		</td>
	</tr>
	<tr>
	    <td align="right"></td>
		<td width="25%">
		    <aspdnsf:ZipCodeValidator ControlToValidate="BillingZip" ID="valBillingZip" Display="Dynamic" runat="server"></aspdnsf:ZipCodeValidator>
		</td>
		<td align="right"></td>
		<td width="25%">
		    <aspdnsf:ZipCodeValidator ControlToValidate="ShippingZip" ID="valShippingZip" Display="Dynamic" runat="server"></aspdnsf:ZipCodeValidator>
		</td>
	</tr>
	<tr>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Country %>" /></td>
		<td width="25%"><asp:DropDownList TabIndex="22" ID="BillingCountry" runat="server" DataTextField="Name" DataValueField="Name"></asp:DropDownList></td>
		<td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Country %>" /></td>
		<td width="25%"><asp:DropDownList TabIndex="42" ID="ShippingCountry" runat="server" DataTextField="Name" DataValueField="Name"></asp:DropDownList></td>
	</tr>
	<tr><td colspan="4" align="center">
        <br />
        <asp:Button ID="CreateCustomer" TabIndex="43" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.CreateCustomer %>" visible="false" OnClick="CreateCustomer_Click"/>&nbsp;
        <asp:Button ID="UpdateCustomer" TabIndex="44" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.UpdateCustomer %>" visible="false" OnClick="UpdateCustomer_Click"/>
        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;
        <asp:Button ID="UseCustomer" runat="server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.UseCustomer %>" visible="false" OnClick="UseCustomer_Click"/></td></tr>
                        </table>
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="CustomerStatsPanel" runat="server" visible="false"><b><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.CreatingOrderForCustomer %>" /><asp:Literal ID="UsingCustomerID" runat="server"></asp:Literal><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.UsingFirstName %>" /><asp:Literal ID="UsingFirstName" runat="server"></asp:Literal><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.UsingLastName %>" /><asp:Literal ID="UsingLastName" runat="server"></asp:Literal><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.UsingEmail %>" /><asp:Literal ID="UsingEMail" runat="server"></asp:Literal></b>
            <br />&nbsp;<asp:Button ID="Button12" runat="server" Font-Size="XX-Small" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.ReEditCustomer %>" OnClick="Button12_Click" />
            &nbsp;<asp:Button ID="Button13" runat="server" Font-Size="XX-Small" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.ReStartImpersonation %>" OnClick="Button13_Click" />
            &nbsp;<asp:Button ID="Button43" runat="server" Font-Size="XX-Small" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.phoneorder.button.ClearFailedTX %>" OnClick="Button43_Click" />
            &nbsp;<asp:Button ID="Button6"  runat="server" Font-Size="XX-Small" Text="<%$Tokens:StringResource, admin.phoneorder.button.CancelOrder %>"  CssClass="normalButtons"  OnClick="Button6_Click" />
            &nbsp;<asp:Button ID="Button88" CssClass="normalButtons" Font-Size="XX-Small" runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.button.DoneOrder %>" OnClick="Button88_Click" />
            &nbsp;<asp:Button ID="Button89" CssClass="normalButtons" Font-Size="XX-Small" runat="server" Visible="false" Text="<%$Tokens:StringResource, admin.phoneorder.button.EditResetToMatchOriginalOrder %>" OnClick="Button89_Click" />
            &nbsp;<asp:Button ID="Button90" CssClass="normalButtons" Font-Size="XX-Small" runat="server" Visible="false" Text="<%$Tokens:StringResource, admin.phoneorder.button.EditClearCart %>" OnClick="Button90_Click" />
            &nbsp;<asp:Button ID="Button91" CssClass="normalButtons" Font-Size="XX-Small" runat="server" Visible="false" Text="<%$Tokens:StringResource, admin.phoneorder.button.EditViewOldReceipt %>" OnClick="Button91_Click" />
        </asp:Panel>
        <asp:Panel ID="ImpersonationPanel" runat="Server" Visible="false" >
            <iframe id="ImpersonationFrame" frameborder="1" style="height: 600px; overflow:visible; width: 100%;" runat="server" scrolling="auto"></iframe>
    	</asp:Panel>
    	<asp:Panel ID="Panel3" runat="server" Visible="false">
    	<table width="100%" height="600" cellpadding="0" cellspacing="0" border="1">
    	<tr>
    	<td align="left" valign="top" height="100%" width="25%">
            <iframe id="LeftPanel3Frame" frameborder="1" width="100%" height="100%" runat="server" scrolling="auto"></iframe>
        </td>
    	<td align="left" valign="top" height="100%" width="25%">
            <iframe id="MiddlePanel3Frame" frameborder="1" width="100%" height="100%" runat="server" scrolling="auto"></iframe>
        </td>
    	<td align="left" valign="top" height="100%" width="50%">
            <iframe id="RightPanel3Frame" src="empty.htm" frameborder="1" width="100%" height="100%" runat="server" scrolling="auto"></iframe>
        </td>
        </tr>
        </table>
    	</asp:Panel>
    	<asp:Panel ID="Panel2" runat="server" Visible="false" Width="100%" Height="600">
    	<table width="100%" height="100%" cellpadding="0" cellspacing="0" border="1">
    	<tr>
    	<td align="left" valign="top" height="100%" width="40%">
            <iframe src="empty.htm" id="LeftPanel2Frame" name="LeftPanel2Frame" frameborder="1" width="100%" height="100%" runat="server" scrolling="auto"></iframe>
        </td>
    	<td align="left" valign="top" height="100%" width="60%">
            <iframe src="empty.htm" id="RightPanel2Frame" name="RightPanel2Frame" frameborder="1" width="100%" height="100%" runat="server" scrolling="auto"></iframe>
        </td>
        </tr>
        </table>
    	</asp:Panel>

</asp:Content>
