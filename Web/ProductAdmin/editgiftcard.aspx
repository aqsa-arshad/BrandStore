<%@ Page Language="C#" AutoEventWireup="true" CodeFile="editgiftcard.aspx.cs" Inherits="AspDotNetStorefrontAdmin.editgiftcard"
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
	<div style="padding-left:10px;">
		<asp:Hyperlink ID="lnkGiftCardManagement" runat="server" Text="<%$Tokens:StringResource, admin.menu.GiftCards %>" NavigateUrl="~/Admin/giftcards.aspx" />
	</div>
    <asp:Literal runat="server" id="ltStyles"></asp:Literal>
        <div style="margin-bottom: 5px; margin-top: 5px; padding:3px; text-align:center;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <asp:Panel runat="server" CssClass="wrapper" DefaultButton="btnSubmit">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>                                
                                <td class="titleTable" width="100%">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.GiftCardDetails %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapper">
                                        <div id="divMain" runat="server">
											<asp:Label ID="lblSerial" runat="server" Text="Serial Number :" /><span style="color: Red;"><asp:Literal runat="server" id="ltCard"></asp:Literal></span>
											<br />
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.ClickHere %>" /> <asp:HyperLink ID="lnkUsage" runat="server" Text="here"></asp:HyperLink>.
                                            <br /><br />
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.RequiredFieldsPrompt %>" />
                                            <br /><br />
                                            <table cellpadding="1" cellspacing="0" border="0">
                                                <tr>
                                                    <td width="250" align="right">
                                                        <asp:Label ID="lblAction" runat="server" CssClass="subTitleSmall" Text="<%$Tokens:StringResource, admin.editgiftcard.Action %>"></asp:Label>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:RadioButtonList ID="rblAction" runat="server">
                                                            <asp:ListItem Value="0" Text="Enabled" />
                                                            <asp:ListItem Value="1" Text="Disabled" />
                                                        </asp:RadioButtonList>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width="250" align="right">
                                                        <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.SerialNumber %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:TextBox ValidationGroup="main" ID="txtSerial" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                        <asp:Image ID="imgSerial" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgSerial %>" runat="server" />
                                                        <asp:RequiredFieldValidator ValidationGroup="main" ID="RequiredFieldValidator" runat="server" ControlToValidate="txtSerial" SetFocusOnError="true" ErrorMessage="<%$Tokens:StringResource,admin.editgiftcard.FillSerialNumber %>"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width="250" align="right">
                                                        <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.coupons.ExpirationDate %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="top">
                                                        <asp:TextBox ValidationGroup="main" ID="txtDate" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                        <asp:Literal ID="ltDate" runat="server"></asp:Literal>
                                                        <asp:RequiredFieldValidator ValidationGroup="main" ErrorMessage="<%$Tokens:StringResource, admin.common.FillinExpirationDate %>" ControlToValidate="txtDate" ID="RequiredFieldValidator3" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 
                                                    </td>
                                                </tr>
                                                 <tr runat="server" id="PurchasedByCustomerIDTextRow">
                                                    <td width="250" align="right">
                                                        <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource,admin.editgiftcard.Purchaser %>" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgCustomer %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
														<asp:TextBox ID="txtCustomerEmail" runat="server" Width="200" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgCustomer %>" OnFocus="ClearCustomerEmailBackground()" OnBlur="CheckCustomerId()"></asp:TextBox>
														<asp:HiddenField ID="hdnCustomerId" runat="server" Value="" />
														<ajax:AutoCompleteExtender ID="autoCompleteCustomerId" runat="server"
															EnableCaching="true"
															MinimumPrefixLength="3"
															CompletionInterval="250"
															CompletionSetCount="15"
															TargetControlID="txtCustomerEmail"
															UseContextKey="True"
															ServiceMethod="GetCompletionList"
															OnClientItemSelected = "AutoCompleteItemSelected"
															CompletionListCssClass="autoCompleteResults"
															FirstRowSelected="true"
															/>
                                                        <asp:Image ID="imgCustomer" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgCustomer %>" runat="server" />
                                                        <asp:RequiredFieldValidator ValidationGroup="main" ErrorMessage="<%$Tokens:StringResource, admin.editgiftcard.Purchaser %>" ControlToValidate="txtCustomerEmail" ID="reqCustEmail" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 

														<script type="text/javascript">// <![CDATA[
															function AutoCompleteItemSelected(sender, e) { 
																var hdnCustomerId = document.getElementById('<%=hdnCustomerId.ClientID %>'); 
																hdnCustomerId.value = e.get_value();
															}
															function CheckCustomerId(sender, e) {
																var hdnCustomerId = document.getElementById('<%=hdnCustomerId.ClientID %>');
																var txtCustomerEmail = document.getElementById('<%=txtCustomerEmail.ClientID %>'); 

																if (hdnCustomerId.value.length === 0) {
																	txtCustomerEmail.style.background = "yellow";
																	txtCustomerEmail.style.opacity = ".5";
																	txtCustomerEmail.value = "Type to search for valid email...";
																}
															}
															function ClearCustomerEmailBackground(sender, e) {
																var txtCustomerEmail = document.getElementById('<%=txtCustomerEmail.ClientID %>');

																txtCustomerEmail.style.background = "#FFF";
																txtCustomerEmail.style.opacity = "1";
																txtCustomerEmail.value = "";
															}
														// ]]>
														</script>
													</td>
                                                </tr>                                                
                                                <tr runat="server" id="PurchasedByCustomerIDLiteralRow">
                                                    <td width="250" align="right">
                                                        <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.Purchaser %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                       <asp:Literal ID="ltCustomerID" runat="server"></asp:Literal>
                                                       [<asp:Literal ID="ltCustomer2" runat="server" Text='<%$Tokens:StringResource, admin.common.CustomerName %>'></asp:Literal>]
                                                      </td>
                                                </tr>   
                                                <tr runat="server" id="OrderNumberRow">
                                                    <td width="250" align="right">
                                                        <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.OriginalOrderNumber %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:Literal ID="txtOrder" runat="server"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr runat="server" id="InitialAmountTextRow">
                                                    <td width="250" align="right">
                                                        <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.InitialAmount %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:TextBox ValidationGroup="main" ID="txtAmount" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                        <asp:Literal ID="Literal1" runat="server"></asp:Literal>
                                                        <asp:RequiredFieldValidator ValidationGroup="main" ErrorMessage="<%$Tokens:StringResource, admin.editgiftcard.EnterValue %>" ControlToValidate="txtAmount" ID="RequiredFieldValidator2" SetFocusOnError="true" runat="server"></asp:RequiredFieldValidator> 
                                                </td>
                                                </tr>
                                                <tr runat="server" id="InitialAmountLiteralRow">
                                                    <td width="250" align="right">
                                                        <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.InitialAmount %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:Literal ID="ltAmount" runat="server"></asp:Literal>
                                                </td>
                                                </tr>
                                                <tr runat="server" id="RemainingBalanceRow">
                                                    <td width="250" align="right">
                                                        <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.RemainingBalance %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle">
                                                        <asp:Literal ID="ltCurrentBalance" runat="server"></asp:Literal>
                                                    </td>
                                                </tr>
                                               <tr runat="server" id="GiftCardTypeSelectRow">
                                                    <td width="250" align="right" style="padding-top: 10px;">
                                                        <font class="subTitleSmall">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.GiftCardType %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle" style="padding-top: 10px;">
                                                        <asp:DropDownList ValidationGroup="main" ID="ddType" runat="server" CssClass="default">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.editgiftcard.SelectType %>" />
                                                            <asp:ListItem Value="102" Text="<%$Tokens:StringResource, admin.editgiftcard.Certificate %>" />
                                                            <asp:ListItem Value="101" Text="<%$Tokens:StringResource, admin.common.E-Mail %>" />
                                                            <asp:ListItem Value="100" Text="<%$Tokens:StringResource, admin.editgiftcard.Physical %>" />
                                                        </asp:DropDownList>
                                                        <asp:Image ID="imgType" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgType %>" runat="server" />
                                                        <asp:RequiredFieldValidator ValidationGroup="main" ControlToValidate="ddType" InitialValue="0" ErrorMessage="<%$Tokens:StringResource, admin.editgiftcard.SelectGiftCardType %>" ID="RequiredFieldValidator5" runat="server" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr runat="server" id="GiftCardTypeDisplayRow">
                                                    <td width="250" align="right" style="padding-top: 10px;">
                                                        <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.GiftCardType %>" />:</font>
                                                    </td>
                                                    <td align="left" valign="middle" style="padding-top: 10px;">
                                                    <asp:Literal ID="ltGiftCardType" runat="server" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td valign="middle" align="right">
                                                        <asp:Panel ID="litStoreMapperHdr" runat="server">
                                                            <font class="subTitleSmall">
                                                            <asp:Literal ID="Literal2" runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" />
                                                            </font>
                                                        </asp:Panel>
                                                    </td>
                                                    <td>
                                                        <asp:Panel ID="litStoreMapper" runat="server" >
                                                            <aspdnsf:EntityToStore ID="etsMapper" runat="server" 
                                                                EntityType="GiftCard" 
                                                                Text=""
                                                                />
                                                        </asp:Panel>
                                                    </td>
                                                </tr>

                                                <tr id="trEmail" runat="server">
                                                    <td width="250" align="right">
                                                        &nbsp;
                                                    </td>                                                    
                                                    <td align="left" valign="top" style="background-color: #f2f2f2; padding: 5px 3px 2px 3px;">
                                                        <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.IfGiftCard %>" /><b> E-Mail type</b>:
                                                        <table cellpadding="1" cellspacing="0" border="0" style="margin-top: 10px;">
                                                            <tr>
                                                                <td align="right">
                                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.EmailSubject %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox ID="txtEmailName" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                    <asp:Image ID="imgEmailName" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgEmailName %>" runat="server" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="right">
                                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.EmailTo %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox ID="txtEmailTo" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                                    <asp:Image ID="imgEmailTo" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgEmailTo %>" runat="server" />
                                                                       <aspdnsf:EmailValidator id="RegularExpressionValidator1" runat="server" Display="None" ControlToValidate="txtEmailTo" EnableClientScript="false" ErrorMessage="<%$Tokens:StringResource, admin.editgiftcard.InvalidEmail %>" SetFocusOnError="true"></aspdnsf:EmailValidator>
                                                                </td>
                                                                     
                                                            </tr>
                                                            <tr>
                                                                <td align="right">
                                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editgiftcard.EmailBody %>" />:</font>
                                                                </td>
                                                                <td align="left" valign="middle">
                                                                    <asp:TextBox ID="txtEmailBody" runat="server" TextMode="MultiLine" CssClass="multiLong"></asp:TextBox>
                                                                    <asp:Image ID="imgEmailBody" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.editgiftcard.tooltip.imgEmailBody %>" runat="server" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width="250" align="right">
                                                        &nbsp;
                                                    </td> 
                                                    <td align="left" style="padding-top: 10px;">
                                                        <asp:Button ValidationGroup="main" ID="btnSubmit" runat="Server" CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Submit %>" OnClick="btnSubmit_Click" />
                                                    </td>
                                                </tr>
                                            </table>
                                            <asp:ValidationSummary ValidationGroup="main" ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
        </table>
    </div>
    <asp:Literal runat="server" id="ltScript"></asp:Literal>

</asp:Content>