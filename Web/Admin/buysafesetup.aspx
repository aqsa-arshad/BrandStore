<%@ Page Language="c#" Inherits="AspDotNetStorefrontAdmin.buysafesetup" CodeFile="buysafesetup.aspx.cs" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master"  %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        #buySafeSignUp
        {
            padding:0 10px;
        }
        td.tdLabel
        {
            text-align:right;
        }
        td.tdHeader
        {
            text-align:left;
            font-size:12px;
            font-weight:bold;
        }
        .error .message, .nonerror .message
        {
            font-weight:bold;
            color:Red;
            font-size:12px;
        }
        .nonerror .message
        {
            color:Green;
        }
        .breadCrumb1{display:none;}
        #mainContentBody
        {
                background-color:#f1f1f1;
        }
        #buySAFEUL li
        {
            list-style-type:disc;
            list-style-position:inside;
            margin-left:20px;
        }
        #buysafeContent .container { 	margin: 0 auto; 	width: 900px; 	} 
        #buysafeContent .content {  	background-color:#FFFFFF; 	width:900px; 	overflow:hidden; 	margin:0; 	padding:0; 	height: auto; 	} 
        #buysafeContent .dottedline_blue{ border-bottom: 1px dotted  #00305E; margin:0; padding:0; height:1px; overflow:hidden; width:98%;	}
        #buysafeContent .dottedline{ 	border-bottom: 1px dotted  #65696E; 	margin:0; 	padding:0; 	height:1px; 	overflow:hidden; 	width:98%; 	}	 	
        #buysafeContent .footerText, #buysafeContent .footerText a { color: #fff; font-size: 11px; }
        #buysafeContent .imgBorder { border: 5px solid #e2e2e2; }
        #buysafeContent .maincontent_home { 	margin: auto; 	padding:0; 	width:900px; 	height:266px; 	overflow:hidden; 	position:static; 	margin-top: 0px; 	margin-bottom: 12px; 	background: #FFDB0A; }	 
        #buysafeContent .mainhighlight { 	width:854px; 	margin:0 23px 0 23px; 	padding:0; 	position: middle; 	background-color: #F4F6F0; 	} 	
        #buysafeContent .mainhighlight2 { 	width:690px; 	margin:0 0px 0 0px; 	padding:0; 	position: middle; 	background-color: #00305e; 	}
        #buysafeContent .ProductNameText {     color: #2393d1;     font-family: Arial, Helvetica, sans-serif;     font-style: italic;     font-size: 15pt;     font-weight: bold;     margin-top: 10px; 	margin-bottom: 10px;    }
        #buysafeContent .repeaterTitle { color: #00305e; font-size: 14px; font-weight: bold; padding-bottom:5px; margin-top:16px; margin-left:8px; margin-right:8px; margin-bottom: 8px; }
        #buysafeContent .right { 	text-align:right; 	}
        #buysafeContent .sideTitle { color: #000; font-size: 14px; font-weight: bold; padding-bottom:5px; margin-bottom: 10px; border-bottom: 1px solid #a3a3a3; }
        #buysafeContent .tocLink a, #buysafeContent .sideText { font-size: 11px; }
        #buysafeContent .topNote a { color: #3d3d3d; }
        #buysafeContent .topNote { font-size: 10px; color: #656565; padding: 10px 0 8px 0; }
        #buysafeContent a { text-decoration: underline; color: #00305e; }
        #buysafeContent a:hover { text-decoration: none; }
        #buysafeContent blockquote { 	font-family: verdana; 	padding-left:24px; 	font-size:10px; 	margin:0px; 	text-align:left; 	line-height:14px; 	font-weight:normal; 	color:#565a5c; 	background-image:url(http://www.buysafe.com/images/blockquote.gif); 	background-repeat:no-repeat; 	} 	
        #buysafeContent, #buysafeContent td { color: #333; font-size: 12px; line-height: 14px; font-family: Verdana, Arial, Helvetica, sans-serif; }
        #buysafeContent H1.callout { 	font-size:30px; 	line-height:30px; 	letter-spacing:-2px; 	word-spacing:2px; 	color:#009933; 	padding-top:2px; 	margin-top:2px; 	margin-bottom:2px; 	padding-bottom:2px; 	} 	
        #buysafeContent H1{ 	font-size:14px; 	line-height:20px; 	color:#00305E; 	padding-top:2px; 	margin-top:2px; 	margin-bottom:2px; 	padding-bottom:2px; 	} 	
        #buysafeContent H2.callout { 	font-size:10px; 	line-height:12px; 	color:#565a5c; 	font-weight: normal;	 	padding:2px 0px 0px 0px; 	margin:0px 2px 2px 5px; 	} 
        #buysafeContent H3 { 	font-size:12px; 	line-height:18px; 	color:#565a5c; 	padding:5px 0px 0px 0px; 	margin:5px 0px 0px 0px; 	font-weight:normal;  	} 	
        #buysafeContent H4 { 	font-size:14px; 	line-height:18px; 	color:#565a5c; 	padding:5px 0px 1px 0px; 	margin:5px 0px 1px 0px; 	} 
        #buysafeContent H4.orange { 	font-size:14px; 	line-height:16px; 	color:#f79621; }
        #buysafeContent H6 { 	font-size:20px; 	line-height:22px; 	color:#00305E; 	padding:5px 0px 3px 0px; 	margin:5px 0px 3px 0px; 	letter-spacing:-2px; 	word-spacing: 2px; 	}	 
        #buysafeContent p { font-size: 12px; margin-top : 5px; margin-bottom : 10px; line-height: 15px; font-family: Verdana, Arial, Helvetica, sans-serif; color: #333; }
        .btnBuySafeSignUp {border: medium none ; margin: 14px 2px 0pt 0pt;}
    </style>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div id="buysafeContent">
        <table width="100%" bgcolor="#f1f1f1" cellpadding="0" cellspacing="0">
            <tbody>
                <tr>
                    <td valign="top" align="center">
                        <!-- Header table -->
                        <table align="center" width="690" cellspacing="0" cellpadding="0" border="0" bgcolor="#ffffff">
	                    <tbody><tr>
		                    <td><img src="../app_themes/admin_default/images/buysafe/header06.gif" alt="buySAFE for AspDotNetStorefront" /></td>
	                    </tr>
	                    </tbody></table>
                        <table width="690" align="center" bgcolor="#ffffff" border="0" cellpadding="0" cellspacing="0">
                            <tbody>
                                <tr>
                                    <td width="690" bgcolor="#ffffff">
                                        <asp:Panel runat="server" Visible="false" ID="toppnlNotEnabledFreeTrialAvailable">
                                            <asp:Panel ID="pnlMessage" runat="server" Visible="false">
                                                <br />
                                                <div class="message">
                                                    <asp:Literal runat="server" ID="litErrorMessage" />
                                                </div>
                                                <br />
                                            </asp:Panel>
                                            <asp:Panel id="pnlBuySafeSignUp" runat="server">
		                                                        <p class="repeaterTitle">AspDotNetStorefront is proud to provide buySAFE via 1-click activation</p>
		                                                        <p style="margin: 4px 8px 2px 10px;">After extensive testing, we’ve proven that buySAFE significantly increases website conversions and customer satisfaction. We’ve negotiated a way for you to <strong>try buySAFE for 30 days, at no cost</strong>. This risk-free trial will allow you to see the impact that buySAFE will have on your business.<br/><br/>Simply click the <strong>“Enable Free Trial”</strong> button below to automatically enable buySAFE on your site.</p>
		                                                        <br/><br/><p class="repeaterTitle">buySAFE provides a comprehensive consumer confidence solution that merchants leverage to build buyer confidence.
		                                                        buySAFE provides three key benefits:
                                                          </p><ul id="buySAFEUL"><li>increased  customer satisfaction</li>
		                                                        <li>higher website conversions, sales and site profitability</li>
		                                                        <li>more repeat buyers</li></ul><p></p>
                                                        <p class="repeaterTitle">How it works:</p>
                                                        <div style="text-align:center;"><img src="../app_themes/admin_default/images/buysafe/buysafeworks.gif" border="0" /></div>
                                                        <br/><br/>
				
				                                                        <div class="mainhighlight2" style="padding:6px 0; text-align:right; color:#FFFFFF; background-image: url(../app_themes/admin_default/images/buysafe/CTAmerch_trial.gif); background-repeat: no-repeat; background-position: left;">
                                                                            <asp:ImageButton ImageUrl="../app_themes/admin_default/images/buysafe/enabletrial.gif" OnClick="btnBuySafeSignUp_Click" CssClass="btnBuySafeSignUp" runat="server" />
                                                                            <br/><br/><div class="footertext" style="margin-right:10px;">By enabling the Trial,you agree to<br/> the <a target="_blank" href="http://buysafe.com/legal_notices/terms_and_conditions.html"  style="color:white">buySAFE terms &amp; conditions</a>
                                                                        </div> 
		                                                          </div>
		  
                                                        <p class="repeaterTitle">Who is using buySAFE:</p>
                                                        <p style="margin: 4px 8px 2px 10px;">buySAFE is now used by nearly 10% of the Top 1000 online merchants and has guaranteed over 20,000,000 online purchases. It is a solution proven to increase customer satisfaction and overall sales by 5-15%.</p><br/><br/>
                                                        <p align="center"><img src="../app_themes/admin_default/images/buysafe/logos_landing.gif" /></p><br/><br/>	
                                            </asp:Panel>
                                        </asp:Panel>
                                        <asp:Panel runat="server" Visible="false" ID="toppnlNotEnabledFreeTrialExpired">
                                            <h1>Your free trial offer has expired.</h1>
                                            <p>Please contact buySafe to enable register.</p><br />
                                            Call 1.888.926.6333 | Email: <a href="mailto:customersupport@buysafe.com">customersupport@buysafe.com</a>
                                            | Visit: <a target="_blank" href="http://www.buysafe.com/">buysafe.com</a><br />
                                        </asp:Panel>
                                        <asp:Panel runat="server" Visible="false" ID="toppnlWorking">
                                            <br />
                                            <asp:Panel runat="server" Visible="false" ID="pnlFreeTrialStatus">
                                                <b><asp:Literal ID="litFreeDaysRemaining" runat="server" /></b><br /><br />
                                            </asp:Panel>
                                            <asp:Panel runat="server" ID="pnlRegisteredStores">
                                                <asp:Panel ID="pnlActivationForm" runat="server" Visible="false">
                                                    <iframe style="border:0" src="buysafeframe.aspx" height="345" width="100%" ></iframe>
                                                </asp:Panel>
                                                <asp:Panel ID="pnlActivated" runat="server" Visible="false">
                                                    <b>
                                                        You have successfully registered with buySAFE.<br /><br /><a href="https://www.buysafe.com/web/Login/SellerLogin.aspx" target="_blank">Click here to login to the buySAFE Merchant Dashboard.</a>
                                                    </b>
                                                </asp:Panel>
                                                
                                                <br />
                                                <br />
                                                <asp:Literal ID="litBuySafeUserName" runat="server" />
                                                <h1>buySAFE is enabled and working for the following stores:</h1>
                                                <asp:Repeater runat="server" ID="repRegisteredStores">
                                                    <HeaderTemplate>
                                                        <ul>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <li>
                                                            <%# (Container.DataItem as AspDotNetStorefrontCore.Store).Name %> (<%# (Container.DataItem as AspDotNetStorefrontCore.Store).ProductionURI %>)
                                                        </li>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        </ul>
                                                    </FooterTemplate>
                                                </asp:Repeater><br /><br />
                                                
                                            </asp:Panel>
                                            <asp:Panel runat="server" ID="pnlUnregisteredStores">
                                                <p>The following domains are not registered with buySafe:</p>
                                                <asp:Repeater runat="server" ID="repUnregisteredStores">
                                                    <HeaderTemplate>
                                                        <ul>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <li>
                                                            <%# (Container.DataItem as AspDotNetStorefrontCore.Store).Name %> (<%# (Container.DataItem as AspDotNetStorefrontCore.Store).ProductionURI %>)
                                                            <asp:Button runat="server" ID="btnRegisterStore" OnClick="btnRegisterStore_Click" CommandArgument="<%# (Container.DataItem as AspDotNetStorefrontCore.Store).StoreID %>" Text="Register Store" />
                                                        </li>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        </ul>
                                                    </FooterTemplate>
                                                </asp:Repeater>
                                            </asp:Panel>
                                        </asp:Panel>
                                        
                                        <asp:Panel runat="server" Visible="false" ID="toppnlEnabledFreeTrialExpired">
                                            <h1>Your free trial offer has expired.</h1>
                                            <p>Please contact buySafe to reenable register.</p>
                                        </asp:Panel>
                                        <asp:Panel runat="server" Visible="false" ID="toppnlError">
                                            <br />
                                            <strong>There has been an error registering with buySAFE.<br /> Please contact <a href="mailto:customersupport@buysafe.com">customersupport@buysafe.com</a> to sign up.</strong>
                                            <br />
                                            <br />
                                        </asp:Panel>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <!-- Unsubscribe -->
                        <table width="690" align="center" bgcolor="#00305e" border="0" cellpadding="00" cellspacing="0">
                            <tbody>
                                <tr>
                                    <td width="10">
                                        <img src="../App_Themes/Admin_Default/images/buySafe/spacer.gif" alt="space" width="10" border="0" height="1" />
                                    </td>
                                    <td>
                                        <span class="footerText"><strong>Learn more about buySAFE:</strong><br />
                                            <br />
                                            Call 1.888.926.6333 | Email: <a href="mailto:customersupport@buysafe.com">customersupport@buysafe.com</a>
                                            | Visit: <a target="_blank" href="http://www.buysafe.com/">buysafe.com</a><br />
                                            <br />
                                        </span>
                                    </td>
                                    <td width="10">
                                        <img src="../App_Themes/Admin_Default/images/buySafe/spacer.gif" alt="space" width="10" border="0" height="1" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <br />
                        <br />
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</asp:Content>