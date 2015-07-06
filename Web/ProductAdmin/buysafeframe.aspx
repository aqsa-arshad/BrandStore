<%@ Page Language="C#" AutoEventWireup="true" CodeFile="buysafeframe.aspx.cs" Inherits="Admin_buysafeframe" %>


<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
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
    </style>
</head>
<body>
    <div style="width:650px;" id="buysafeContent">
        <asp:Panel ID="pnlActivationSuccess" runat="server" Visible="false">
            <table style="height:330px;">
                <tr>
                    <td valign="middle" style="vertical-align:middle;">
                        <b>Your request has been submitted to buySAFE.</b><br /><br />
                        buySAFE will be contacting you shortly to complete the full buySAFE registration process.
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="pnlActivationForm" runat="server">
            <p></p><h3>Congratulations! Your 30-day free trial of buySAFE has been activated. You will now begin seeing the benefits of
            increased consumer confidence!</h3><p></p>

            <p></p><h1>For more information about the benefits of buySAFE or to upgrade to a full account, please call us at
            1-888-926-6333 or complete the form below.</h1><p></p>
            <br/>

            <form id="form1" method="post" action="https://www.buysafe.com/web/partner/ProcessPartnerData.aspx?msp=Vortx" >
                <input type="hidden" id="bsp.AccountHash" name="bsp.AccountHash" value='<asp:Literal id="litAccountHash" runat="server" />' />
                <input type="hidden" id="bsp.BusinessInformation.PromoCode" name="bsp.BusinessInformation.PromoCode" value='Vortx_2011_03' />
                <input type="hidden" id="bsp.ReturnURL" name="bsp.ReturnURL" value='<asp:Literal id="litReturnURL" runat="server" />' />
                <input type="hidden" id="bsp.StoreInformation.Url" name="bsp.StoreInformation.Url" value='<asp:Literal id="litStoreUrl" runat="server" />' />
                <input type="hidden" id="bsp.BusinessInformation.CompanyName" name="bsp.BusinessInformation.CompanyName" value='<asp:Literal id="litCompanyName" runat="server" />' />
                <div style="text-align:left; width:95%;">
                    <div id="err_msg" style="text-align:center; color:red;">&nbsp;</div>
                    <table style="margin-left:auto; margin-right:auto;" cellpadding="2" cellspacing="0" border="0">
                    <tbody><tr>
                        <td class="right">First Name:</td>
                        <td><input id="bsp.BusinessInformation.FirstName" maxlength="40" name="bsp.BusinessInformation.FirstName" size="20" type="text" />&nbsp;&nbsp;</td>
                        <td class="right">Last Name:</td>
                        <td><input id="bsp.BusinessInformation.LastName" maxlength="80" name="bsp.BusinessInformation.LastName" size="20" type="text" /></td>
                    </tr>
                    <tr>
                        <td class="right">Email:</td>
                        <td><input id="bsp.BusinessInformation.Email" maxlength="80" name="bsp.BusinessInformation.Email" size="20" type="text" />&nbsp;&nbsp;</td>
                        <td class="right">Phone:</td>
                        <td><input id="bsp.BusinessInformation.Phone.Number" maxlength="40" name="bsp.BusinessInformation.Phone.Number" size="20" type="text" /></td>
                    </tr>
                    <tr>
                        <td class="right" style="vertical-align:top;">Avg Monthly Sales $:</td>
                        <td style="vertical-align:top;"><input id="bsp.BusinessInformation.AverageMonthlyGrossSales" name="bsp.BusinessInformation.AverageMonthlyGrossSales" size="20" type="text" /></td>
                        <td class="right"></td>
                        <td align="right">
                        <input type="submit" class="" name="submit" value="Submit to buySAFE" />
                        </td>
                    </tr>
                    </tbody></table>
                </div>
            </form>

            <br/><br/>
            <p>NOTE: If you have already completed the upgrade process, you can login to the
            <a target="_blank" href="https://www.buysafe.com/web/Login/SellerLogin.aspx">buySAFE Merchant Service Center</a>.</p>
            <br/>
        </asp:Panel>
    </div>
</body>
</html>
