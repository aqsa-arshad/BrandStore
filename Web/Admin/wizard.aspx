<%@ Page Language="C#" AutoEventWireup="true" Title="Wizard" CodeFile="wizard.aspx.cs" Inherits="AspDotNetStorefrontAdmin.wizard" MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsfc" %>
<%@ Register TagPrefix="aspdnsf" TagName="ModalConfigurationAtom" Src="Controls/ModalConfigurationAtom.ascx" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register TagPrefix="aspdnsf" TagName="editAppConfigAtom" Src="Controls/editAppConfigAtom.ascx" %>
<%@ Register TagPrefix="aspdnsf" TagName="GeneralInfo" Src="Controls/GeneralInfo.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src="Scripts/jquery.min.js"></script>
    <script type="text/javascript" src="Scripts/jqueryextensions.js"></script>

    <script type="text/javascript">
        function showGatewayDirections(description) {
            var instructionsHTML = "After successful download, unzip the file and review 'readme.txt' for installation instructions.<br />Your new payment gateway will appear here when you refresh the page.";
            if (description != null && description.length != 0) {
                instructionsHTML = description;
            }
            $("#GatewayInstructionsContent").html(instructionsHTML);

            $find('mpGatewayInstructions').show();
        }
        function pageLoad(sender, args) {
            if (!args.get_isPartialLoad()) {
                $addHandler(document, "keydown", onKeyDown);
            }
        }
        function onKeyDown(e) {
            if (e && e.keyCode == Sys.UI.Key.esc) {
                $('.atomModalClose').click();
            }
        }
    </script>

</asp:Content>

<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">

<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server" />
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>

    <div id="container">
        <aspdnsf:ModalConfigurationAtom runat="server" ShowConfigureLink="false"  ID="FileConfigurationAtom" />
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle">Wizard:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        This wizard can help you configure your store's primary configuration variables after first installation. 
                                        <br />
                                        <div id="divMain" class="configWizard" runat="server">
                                            <p>Fields marked with an asterisk (*) are required. All other fields are optional.</p>
                                            <table cellpadding="1" cellspacing="0" border="0" width="100%">
                                                <tr id="trSaveTop">
                                                    <td colspan="2">
														<div class="wizardButtonWrap">
															<asp:Button ID="btnSubmitTop" runat="Server" CssClass="defaultButton" Text="Submit" OnClick="btnSubmit_Click" />
														</div>
                                                    </td>
                                                </tr>
                                                
                                                <tr>
                                                	<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>Store Name:</b>
														</div>
													</td>
                                                	<td>
                                                		<aspdnsf:editAppConfigAtom runat="server" id="AtomStoreName" HideLabel="true" ShowSaveButton="false" AppConfig="StoreName" />
                                                	</td>
                                                </tr>
                                                <tr>
													<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>Live Server Domain:</b>
														</div>
													</td>
                                                	<td>
                                                		<aspdnsf:editAppConfigAtom runat="server" id="AtomLiveServer" HideLabel="true" ShowSaveButton="false" AppConfig="LiveServer" />
                                                	</td>
                                                </tr>
                                                <tr>
													<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>Shipping Origin Zip Code:</b>
														</div>
													</td>
                                                	<td>
                                                		<aspdnsf:editAppConfigAtom runat="server" id="AtomStoreZip" HideLabel="true" ShowSaveButton="false" AppConfig="RTShipping.OriginZip" />
                                                	</td>
                                                </tr>
                                                <tr>
                                                	<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>Store Currency:</b>
														</div>
													</td>
                                                	<td>
                                                		<aspdnsf:editAppConfigAtom runat="server" id="AtomStoreCurrency" HideLabel="true" ShowSaveButton="false" AppConfig="Localization.StoreCurrency" />
                                                	</td>
                                                </tr>
                                                <tr>
													<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>Store Currency Numeric Code:</b>
														</div>
													</td>
                                                	<td>
                                                		<aspdnsf:editAppConfigAtom runat="server" id="AtomStoreCurrencyNumeric" HideLabel="true" ShowSaveButton="false" AppConfig="Localization.StoreCurrencyNumericCode" />
                                                	</td>
                                                </tr>
                                                <tr>
													<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>Use Live Transactions:</b>
														</div>
													</td>
                                                	<td>
                                                		<aspdnsf:editAppConfigAtom runat="server" id="AtomStoreLiveTransactions" HideLabel="true" ShowSaveButton="false" AppConfig="UseLiveTransactions" />
                                                	</td>
                                                </tr>
                                                <tr>
													<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>Use SSL:</b>
														</div>
													</td>
                                                	<td>
                                                		<aspdnsf:editAppConfigAtom runat="server" id="AtomStoreUseSSL" HideLabel="true" ShowSaveButton="false" AppConfig="UseSSL" />
                                                	</td>
                                                </tr>

                                                <tr id="MachineKeyRow" runat="server">
													<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>
																<asp:Literal ID="ltStaticMachineKey" runat="server" />
															</b>
														</div>
													</td>
													<td class="atomInfoCell">
														<table>
															<tr>
																<td class="atomAppConfigEditCell">
																	<div class="atomAppConfigEditWrap">
																		<asp:RadioButtonList ID="rblStaticMachineKey" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" CellPadding="5" CellSpacing="0">
																			<asp:ListItem Value="false" Selected="true" Text="No" />
																			<asp:ListItem Value="true" Text="Yes" />
																		</asp:RadioButtonList>
																	</div>
																</td>
																<td>
																	<div class="atomDescriptionWrap">
																		<asp:Label ID="Label1" runat="server" Text="<%$Tokens:StringResource, admin.wizard.tooltip.imgStaticMachineKey %>" />
																	</div>
																</td>
															</tr>
														</table>
													</td>
												</tr>


												<tr id="EncryptWebConfigRow" runat="server">
													<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>
																Encrypt the Web.Config:
															</b>
														</div>
													</td>
													<td class="atomInfoCell">
														<table>
															<tr>
																<td class="atomAppConfigEditCell">
																	<div class="atomAppConfigEditWrap">
																		<asp:RadioButtonList ID="rblEncrypt" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" CellPadding="5" CellSpacing="0">
																			<asp:ListItem Value="false" Selected="true" Text="No" /> 
																			<asp:ListItem Value="true" Text="Yes" />                                                                                                                                                                                            
																		</asp:RadioButtonList>
																	</div>
																</td>
																<td>
																	<div class="atomDescriptionWrap">
																		<asp:Label ID="Label2" runat="server" Text="<%$Tokens:StringResource, admin.wizard.tooltip.imgEncryptKey %>" />
																	</div>
																</td>
															</tr>
														</table>
													</td>
												</tr>

                                                <tr id="trEmail">
                                                    <td align="right" class="textBoxLabel">
                                                        <div class="configTitle">
                                                        	<b>Email:</b>
                                                        </div>
                                                    </td>      
                                                    <td align="left">
														<table>
															<tr>
																<td class="atomAppConfigEditCell">
																	<div class="atomAppConfigEditWrap">
																		<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
																			<Triggers>
																				<asp:AsyncPostBackTrigger ControlID="btnConfigureEmail" EventName="Click" />
																			</Triggers>
																			<ContentTemplate>
																				<asp:LinkButton ID="btnConfigureEmail" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.Email.xml" Text="Configure Email" runat="server" />
																			</ContentTemplate>
																		</asp:UpdatePanel>
																	</div>
																</td>
																<td></td>
															</tr>
														</table>
														
                                                    </td>                                                                                                  
                                                </tr>
                                                
                                                <tr id="trSEO">
                                                    <td align="right" class="textBoxLabel">
                                                        <div class="configTitle">
                                                        	<b>Search Engine (Meta Tags):</b>
                                                        </div>
                                                    </td>      
                                                    <td align="left">
														<table>
															<tr>
																<td class="atomAppConfigEditCell">
																	<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
																		<Triggers>
																			<asp:AsyncPostBackTrigger ControlID="btnConfigureSEO" EventName="Click" />
																		</Triggers>
																		<ContentTemplate>
																			<asp:LinkButton ID="btnConfigureSEO" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.SEO.xml" Text="Configure SEO" runat="server" />
																		</ContentTemplate>
																	</asp:UpdatePanel>
																</td>
																<td></td>
															</tr>
														</table>
                                                    </td>                                                                                                  
                                                </tr>
                                                
												<tr id="trBuySafe">
													<td class="textBoxLabel" align="right">
														<div class="configTitle">
															<b>Increase sales with buySAFE:</b>
														</div>
													</td>
													<td class="atomInfoCell">
														<asp:Panel ID="pnlBuySafeInactive" runat="server">
															<table>
																<tr>
																	<td class="atomAppConfigEditCell">
																		<div class="atomAppConfigEditWrap">
																			<asp:RadioButtonList ID="rblBuySafeEnabled" RepeatDirection="Horizontal" runat="server">
																				<asp:ListItem Selected="True" Text="No" />
																				<asp:ListItem Text="Yes" />
																			</asp:RadioButtonList>
																		</div>
																	</td>
																	<td>
																		<div class="atomDescriptionWrap">
																			<a href="buysafeSetup.aspx" style="display:block;margin-left:5px;" target="_blank">buySAFE increases your site conversions by improving shopper confidence. Click here to learn more...</a>
																		</div>
																	</td>
																</tr>
															</table>
														</asp:Panel>
														<asp:Panel ID="pnlBuySafeActive" Visible="false" runat="server">
                                                            <asp:Literal ID="litBuySafeActiveMsg" runat="server" />
                                                        </asp:Panel>
													</td>
												</tr>


                                                <tr style="background-color:#19437D;" id="fraudSolutions">
                                                    <td></td>
                                                    <td style="font-weight:bold;color:#fff;">Configure Fraud Solutions</td>
                                                </tr>
                                                
                                                <tr id="trMaxMind">
                                                    <td align="right" class="textBoxLabel">
                                                        <div class="configTitle">
                                                        	<b>MaxMind:</b>
                                                        </div>
                                                    </td>      
                                                    <td align="left">
														<table>
															<tr>
																<td class="atomAppConfigEditCell">
																	<asp:UpdatePanel runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
																		<Triggers>
																			<asp:AsyncPostBackTrigger ControlID="btnConfigureMaxMind" EventName="Click" />
																		</Triggers>
																		<ContentTemplate>
																			<asp:LinkButton ID="btnConfigureMaxMind" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.MaxMind.xml" Text="Configure MaxMind" runat="server" />
																		</ContentTemplate>
																	</asp:UpdatePanel>
																</td>
																<td></td>
															</tr>
														</table>
                                                    </td>
                                                </tr>
                                                
                                                <tr style="background-color:#19437D;" id="country">
                                                    <td></td>
                                                    <td style="font-weight:bold;color:#fff;">Choose Your Country</td>
                                                </tr>
												<tr>
													<td align="right">
														<div class="configTitle">
															<b>Choose your company's country:</b>
														</div>
													</td>
													<td>
														<table>
                                                            <a id="payments"></a>
															<tr>
																<td class="atomAppConfigEditCell">
																	<asp:DropDownList ID="ddlCountries" runat="server" OnSelectedIndexChanged="ddlCountries_SelectedIndexChanged" AutoPostBack="true" />
																</td>
																<td></td>
															</tr>
														</table>
													</td>
												</tr>
                                                                                                
                                                <tr style="background-color:#19437D;" id="alternativePaymentMethods">
                                                    <td></td>
                                                    <td style="font-weight:bold;color:#fff;">Configure Alternative Payment Methods</td>
                                                </tr>

                                               <tr id="trCheckoutMethods">
                                                    <td align="right" class="textBoxLabel">
														<div class="configTitle">
															<b>Alternative Payment Methods:</b>
														</div>
                                                        <asp:UpdatePanel runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                                                            <Triggers>
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigureCheckoutMethodsPerStore" EventName="Click" />
                                                            </Triggers>
                                                            <ContentTemplate>
                                                                <span class="lineHeightFix">
                                                                    <asp:LinkButton ID="btnConfigureCheckoutMethodsPerStore" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.PaymentMethods.xml" Text="Configure Per Store" runat="server" />&nbsp;&nbsp;
                                                                </span>
                                                            </ContentTemplate>
                                                        </asp:UpdatePanel>
                                                    </td>
                                                    <td align="left">
                                                        <asp:UpdatePanel ID="UpdatePanel8" UpdateMode="Conditional" ChildrenAsTriggers="false" runat="server">
                                                            <Triggers>
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigureCheckoutByAmazon" EventName="Click" />
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigurePayPalExpress" EventName="Click" />
                                                            </Triggers>
                                                            <ContentTemplate>
                                                                <table class="alternaterows paymentOptionsTable" cellspacing="0" width="100%">
                                                                   <tr id="trPayPalExpress" runat="server">
                                                                        <td class="configSelect">                                                                            
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxPayPalExpress" runat="server"  Text="PayPal Express Checkout" />
                                                                        </td>
                                                                        <td class="configImage">
                                                                            <img id="Img3" runat="server" src="Images/PayPal_OnBoarding_ECPaymentIcon.gif" class="paymentIcon paypalCheckoutBtn"/>
                                                                        </td>
                                                                        <td>
                                                                            See <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalexpress&type=manual" target="_blank">Manual</a> and <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalexpress&type=demo" target="_blank">Demo</a>
                                                                        </td>
                                                                        <td class="paymentRightCell">
                                                                            <asp:LinkButton ID="btnConfigurePayPalExpress" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.PayPalExpress.xml" Text="configure" runat="server" />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">  
															                <asp:CheckBox CssClass="paymentCheckBox" ID="cbxCheckoutByAmazon" runat="server"  Text="Checkout By Amazon" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>
                                                                            <asp:Literal ID="litAmazonPrompt" runat="server" />
                                                                        </td>
                                                                        <td class="paymentRightCell">
                                                                            <asp:LinkButton ID="btnConfigureCheckoutByAmazon" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.CheckoutByAmazon.xml" Text="configure" runat="server" />
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </ContentTemplate>
                                                        </asp:UpdatePanel>
                                                    </td>
                                                </tr>


                                                <tr style="background-color:#19437D;" id="paymentMethods">
                                                    <td></td>
                                                    <td style="font-weight:bold;color:#fff;">Credit Cards & Other Payment Methods</td>
                                                </tr>
                                                
                                                <tr id="trPaymentMethods">
                                                    <td align="right" class="textBoxLabel">
														<div class="configTitle">
															<b>Payment Methods:</b>
														</div>
                                                        <asp:UpdatePanel ID="UpdatePanel3" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                                                            <Triggers>
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigurePaymentMethodsPerStore" EventName="Click" />
                                                            </Triggers>
                                                            <ContentTemplate>
                                                                <span class="lineHeightFix">
                                                                    <asp:LinkButton ID="btnConfigurePaymentMethodsPerStore" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.PaymentMethods.xml" Text="Configure Per Store" runat="server" />&nbsp;&nbsp;
                                                                </span>
                                                            </ContentTemplate>
                                                        </asp:UpdatePanel>
                                                    </td>
                                                    <td align="left">
                                                        <asp:UpdatePanel ID="UpdatePanel4" UpdateMode="Conditional" ChildrenAsTriggers="false" runat="server">
                                                            <Triggers>                                                                
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigureCreditCard" EventName="Click" />
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigurePayPalExpress" EventName="Click" />
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigurePayPal" EventName="Click" />
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigureCardinalEChecks" EventName="Click" />
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigureMicroPay" EventName="Click" />
                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigureMoneyBookersQuickCheckout" EventName="Click" />
                                                            </Triggers>
                                                            
                                                            <ContentTemplate>
                                                                <table class="alternaterows paymentOptionsTable" cellspacing="0" width="100%">
                                                                    <tr id="trPayPalWebsitePaymentsStandard" runat="server">
                                                                        <td class="configSelect">
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxPayPal" runat="server"  Text="PayPal Payments Standard" />
                                                                        </td>
                                                                        <td class="configImage">
                                                                            <img id="Img1" runat="server" src="Images/PayPal_PaymentsAccepted.gif" class="paymentIcon paypalGenericOptions"/>
                                                                        </td>
                                                                        <td>
                                                                            See <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalstandard&type=manual" target="_blank">Manual</a> and <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=paypalstandard&type=demo" target="_blank">Demo</a>
                                                                        </td>
                                                                        <td class="paymentRightCell">
                                                                            <asp:LinkButton ID="btnConfigurePayPal" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.PayPal.xml" Text="configure" runat="server" />
                                                                        </td>
                                                                    </tr>
                                                                     <tr>
                                                                        <td class="configSelect">  
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxCreditCard" runat="server" Text="Credit Card" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>&nbsp;</td>
                                                                        <td class="paymentRightCell">
                                                                            <asp:LinkButton ID="btnConfigureCreditCard" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.CreditCard.xml" Text="configure" runat="server" />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxRequestQuote" runat="server"  Text="Request For Quote" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>&nbsp;</td>
                                                                        <td class="paymentRightCell">&nbsp;</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxPurchaseOrder" runat="server"  Text="Purchase Orders" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>&nbsp;</td>
                                                                        <td class="paymentRightCell">&nbsp;</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxCheckByMail" runat="server"  Text="Checks" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>&nbsp;</td>
                                                                        <td class="paymentRightCell">&nbsp;</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxCOD" runat="server"  Text="C.O.D." />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>&nbsp;</td>
                                                                        <td class="paymentRightCell">&nbsp;</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxECheck" runat="server"  Text="E-Checks through supported gateway" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>
                                                                            See <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=echecks&type=manual" target="_blank">Manual</a> for supported gateways
                                                                        </td>
                                                                        <td class="paymentRightCell">&nbsp;</td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxCardinalMyCheck" runat="server"  Text="-or- Cardinal Centinel MyECheck" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>&nbsp;</td>
                                                                        <td class="paymentRightCell">
                                                                            <asp:LinkButton ID="btnConfigureCardinalEChecks" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.CardinalMyECheck.xml" Text="configure" runat="server" />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">
                                                                            <asp:CheckBox CssClass="paymentCheckBox" ID="cbxMicroPay" runat="server"  Text="MICROPAY" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>
                                                                            See <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=micropay&type=manual" target="_blank">Manual</a>
                                                                        </td>
                                                                        <td class="paymentRightCell">
                                                                            <asp:LinkButton ID="btnConfigureMicroPay" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.Micropay.xml" Text="configure" runat="server" />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="configSelect">
															                <asp:CheckBox CssClass="paymentCheckBox" ID="cbxMoneyBookers" runat="server" Value="Moneybookers Quick Checkout" Text="Skrill (Moneybookers) Quick Checkout" />
                                                                        </td>
																		<td class="configImage"></td>
                                                                        <td>
                                                                            Learn <a href="http://www.aspdotnetstorefront.com/linkmanager.aspx?topic=skrill&type=learnmore" target="_blank">more...</a>
                                                                        </td>

                                                                        <td class="paymentRightCell">
                                                                            <asp:LinkButton ID="btnConfigureMoneyBookersQuickCheckout" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="PaymentMethod.MoneyBookersQuickCheckout.xml" Text="configure" runat="server" />
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </ContentTemplate>
                                                        </asp:UpdatePanel>
                                                    </td>
                                                </tr>
                                                
                                                <tr style="background-color:#19437D;" id="paymentGateways">
                                                    <td></td>
                                                    <td style="font-weight:bold;color:#fff;">Payment Processing Solutions</td>
                                                </tr>

                                                <tr id="trGateways">
                                                    <td align="right" class="textBoxLabel">
                                                        <div class="configTitle">
                                                        	<b>Payment Gateway:</b>
                                                        </div>
                                                        <asp:UpdatePanel ID="UpdatePanel5" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
                                                            <Triggers>
                                                                <asp:AsyncPostBackTrigger ControlID="btnGatewayPerStoreAndBackup" EventName="Click" />
                                                            </Triggers>
                                                            <ContentTemplate>
                                                                <span class="lineHeightFix">
                                                                    <asp:LinkButton style="margin-right:5px;display:block;" ID="btnGatewayPerStoreAndBackup" OnClick="ShowModalAtomByXMLFile_Click" CommandArgument="General.Gateway.xml" runat="server" >Configure Per Store and<br />Backup Gateways</asp:LinkButton><br />
                                                                </span>
                                                            </ContentTemplate>
                                                        </asp:UpdatePanel>
                                                    </td>
                                                    <td align="left">
                                                        <asp:Repeater ID="repGateways" runat="server" OnItemDataBound="repGateways_DataBinding" OnItemCommand="repGateways_ItemCommand">
                                                            <HeaderTemplate>
                                                                <table width="100%" cellspacing="0" class="alternaterows paymentOptionsTable">
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                <tr id="trGateway" runat="server">
                                                                    <td class="configSelect">
                                                                        <asp:UpdatePanel ID="upGatewaySelect" runat="server" UpdateMode="Always" ChildrenAsTriggers="true">
                                                                            <Triggers>
                                                                                <asp:AsyncPostBackTrigger ControlID="repGateways" EventName="ItemCommand" />
                                                                                <asp:AsyncPostBackTrigger ControlID="btnConfigureGateway" EventName="Click" />
                                                                            </Triggers>
                                                                            <ContentTemplate>
                                                                                <aspdnsfc:GroupRadioButton CssClass="rbGateway" ID="rbGateway" GroupName="SelectedGateway" Enabled="false" runat="server" value='<%# Eval("GatewayIdentifier") %>' />
																				<span class="wizardRadioDisplayName">
																					<%# Eval("DisplayName") %>
																				</span>
                                                                            </ContentTemplate>
                                                                        </asp:UpdatePanel>
                                                                    </td>
                                                                    <td class="configImage">  
                                                                        <asp:Image ID="imgPayPal" CssClass="paymentIcon paypalGenericOptions" runat="server" ImageUrl="Images/PayPal_PaymentsAccepted.gif" Visible="false" />                                                          
                                                                    </td>
                                                                    <td>
                                                                        <div class="lineHeightFix">
                                                                            <%# Eval("AdministratorSetupPrompt")%>
                                                                        </div>
                                                                        <asp:HiddenField ID="hfGatewayIdentifier" Value='<%# Eval("GatewayIdentifier") %>' runat="server" />
                                                                        <asp:HiddenField ID="hfGatewayProductIdentifier" Value='<%# Eval("DisplayName") %>' runat="server" />
                                                                    </td>
                                                                    <td class="paymentRightCell">
                                                                        <asp:LinkButton runat="server" ID="btnConfigureGateway" Text="configure" CommandName="ShowConfiguration" CommandArgument='<%# Eval("GatewayIdentifier") %>' />
                                                                    </td>
                                                                </tr>
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                </table>
                                                            </FooterTemplate>
                                                        </asp:Repeater>
                                                        <aspdnsf:ModalConfigurationAtom runat="server" OnAtomSaved="GatewayConfigurationAtom_Saved" ShowConfigureLink="false"  ID="GatewayConfigurationAtom" ConfigureButtonCssClass="ConfigureGatewayButton" />
                                                    </td>
                                                </tr>

                                                <tr id="trSaveBottom">
                                                    <td colspan="2">
														<div class="wizardButtonWrap">
															<asp:Button ID="btnSubmitBottom" runat="Server" CssClass="defaultButton" Text="Submit" OnClick="btnSubmit_Click" />
														</div>
                                                    </td>
                                                </tr>
                                            </table>
                                            <asp:ValidationSummary ID="validationSummary" runat="server" EnableClientScript="true" ShowMessageBox="true" ShowSummary="false" Enabled="true" />
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
        
    <ajax:ModalPopupExtender 
        ID="mpGatewayInstructions" runat="server" 
        PopupControlID="pnlGatewayInstructions"
        TargetControlID="btnShowGatewayInstructions" 
        BackgroundCssClass="modal_popup_background" 
        CancelControlID="btnCancelConfiguration"
        BehaviorID="mpGatewayInstructions"
        >
    </ajax:ModalPopupExtender>
    
    <div style="display: none;">
        <asp:LinkButton ID="btnShowGatewayInstructions" runat="server" Text="configure" OnClientClick="showGatewayDirections();return false;" />
        <asp:Panel ID="pnlGatewayInstructions" runat="server" CssClass="modal_popup atom_modal_popup" Width="725px" ScrollBars="None">
            <div class="modal_popup_Header" id="modaldiv" runat="server">
                <asp:Literal ID="Literal1" runat="server" Text="Gateway Installation" />
                <div style="float:right;">
                    <asp:ImageButton ID="btnCancelConfiguration" runat="server" src="../App_Themes/Admin_Default/images/delete.png" />
                </div>
            </div>
            <asp:Panel ID="pnlConfigAtomContainer" runat="server" ScrollBars="None">
                <div style="padding:10px;">
                    <strong>Your download has started.</strong><br /><br />
                    <span id="GatewayInstructionsContent"></span>
                </div>
            </asp:Panel>
        </asp:Panel>
    </div>

</asp:Content>