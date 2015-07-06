<%@ Page Language="C#" AutoEventWireup="true" CodeFile="editAffiliates.aspx.cs" Inherits="AspDotNetStorefrontAdmin.editAffiliates" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<%@ OutputCache Duration="1" Location="none" %>
<asp:content runat="server" contentplaceholderid="bodyContentPlaceholder">

    <asp:Literal ID="ltScript1" runat="server"></asp:Literal>
    <div id="help">
        <div class="pageTitle">
            <asp:Literal id="ltAffiliate" runat="server" />
        </div>
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <asp:Panel ID="pnlAffiliateDetails" runat="server" DefaultButton="btnSubmit">
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.AffiliateDetails %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">                                        
                                        <div style="margin-top: 5px; margin-bottom: 15px;">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.RequiredFieldsPrompt %>" />
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.NickName %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtNickName" runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.FirstName %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtFirstName" runat="server"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ValidationGroup="signup" ErrorMessage="<%$Tokens:StringResource, admin.editAffiliates.ErrorMessageFirstName %>" ControlToValidate="txtFirstName" EnableClientScript="false" ID="RequiredFieldValidator4" SetFocusOnError="true" runat="server" Display="static"></asp:RequiredFieldValidator> 
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.LastName %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtLastName" runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.ParentAffiliate %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddParent" runat="server">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="top">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Email %>" /></font>
                                                </td>
                                                <td align="left" valign="top">
                                                    <asp:TextBox ID="txtEmail" runat="server" Width="269px"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="valReqEmail" runat="server" Display="static" EnableClientScript="false" ValidationGroup="signup" ErrorMessage="<%$Tokens:StringResource, admin.editAffiliates.EnterEmailAddress %>" ControlToValidate="txtEmail" SetFocusOnError="true" Enabled="false" />
                                                </td>
                                            </tr>
                                            <tr runat="server" id="ResetPasswordRow">
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.cst_account.ResetPassword %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:LinkButton ID="ResetPasswordLink" runat="server" OnClick="ResetPasswordLink_Click" Text="<%$Tokens:StringResource, admin.common.ResetPassword %>"></asp:LinkButton>
                                                    <asp:Label ID="ResetPasswordError" runat="server" Font-Bold="True" ForeColor="Red" Text="Label" Visible="False"></asp:Label>
                                                    <asp:Label ID="ResetPasswordOk" runat="server" Font-Bold="True" ForeColor="Black" Text="Label" Visible="False"></asp:Label>
                                                </td>
                                            </tr>
                                            <asp:Panel ID="CreatePasswordRow" runat="server">
                                            <tr>
                                                <td align="right"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.topic.password %>" /></td>
                                                <td>
                                                    <asp:TextBox ID="AffPassword" TextMode="Password" Columns="37" maxlength="100" runat="server" ValidationGroup="signup" CausesValidation="true"></asp:TextBox> <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.PasswordLength %>" />
                                                    <asp:RequiredFieldValidator ID="reqValPassword" ControlToValidate="AffPassword" runat="server" Display="static" EnableClientScript="false" ValidationGroup="signup" Enabled="false"></asp:RequiredFieldValidator>
                                                    <asp:CustomValidator ID="valPassword" runat="server" Display="static" EnableClientScript="false" ValidationGroup="signup" SetFocusOnError="true" OnServerValidate="ValidatePassword"></asp:CustomValidator>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td align="right" valign="middle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.RepeatPassword %>" /></td>

                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="AffPassword2" TextMode="Password" Columns="37" maxlength="100" runat="server" ValidationGroup="signup" CausesValidation="true"></asp:TextBox>
                                                </td>
                                            </tr>
                                            </asp:Panel>
                                             <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.DefaultSkin %>" /></font>
                                                </td>                                                
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtSkin" runat="server" CssClass="singleShortest" Width="72px">1</asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.order.Company %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtCompany" runat="server" Width="200px"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.Address1 %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtAddress1" runat="server" CssClass="singleNormal" Width="200px"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.Address2 %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtAddress2" runat="server" CssClass="singleNormal" Width="200px"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Suite %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtSuite" runat="server" CssClass="singleShortest" Width="85px"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.City %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtCity" runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.State %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddState" runat="server">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.Zip %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtZip" runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Country %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:DropDownList ID="ddCountry" runat="server">
                                                    </asp:DropDownList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.phoneorder.label.Phone %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtPhone" runat="server"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.cst_account.DateOfBirth %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtBirthdate" runat="server" CssClass="singleShorter"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.ForAdTrackingOnly %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:RadioButtonList ID="rblAdTracking" runat="server">
                                                        <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.False %>"></asp:ListItem>
                                                        <asp:ListItem Value="1" Selected="true" Text="<%$Tokens:StringResource, admin.common.True %>"></asp:ListItem>                                                            
                                                    </asp:RadioButtonList>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td valign="middle" align="right">
                                                    <asp:Panel ID="litStoreMapperHdr" runat="server">
                                                        <font class="subTitleSmall">
                                                        <asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" />
                                                        </font>
                                                    </asp:Panel>
                                                </td>
                                                <td>
                                                    <asp:Panel ID="litStoreMapper" runat="server" >
                                                        <aspdnsf:EntityToStore ID="etsMapper" runat="server" 
                                                            EntityType="Affiliate" 
                                                            Text=""
                                                            />
                                                    </asp:Panel>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <asp:ValidationSummary ValidationGroup="signup" ID="validationSummary" runat="server" EnableClientScript="false" ShowMessageBox="false" ShowSummary="false" Enabled="true" />
                                                </td>
                                            </tr>
                                        </table>
                                        <div style="width: 100%; text-align: center;">
                                            &nbsp;&nbsp;<asp:Button ValidationGroup="signup" ID="btnSubmit" runat="server" CssClass="normalButtons" OnClick="btnSubmit_Click" />
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlWebsiteInformation" runat="server" DefaultButton="btnSubmit1">
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteInformation %>" /></font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="*">
                                    <div class="wrapperLeft">                                        
                                        <div style="margin-top: 5px; margin-bottom: 15px;">
                                            <asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteNotification %>" />
                                        </div>
                                        <table width="100%" cellpadding="1" cellspacing="0" border="0">
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteName %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtWebName" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                </td>
                                            </tr>  
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteDescription %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtWebDescription" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                </td>
                                            </tr>  
                                            <tr>
                                                <td width="260" align="right" valign="middle">
                                                    <font class="subTitleSmall"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editAffiliates.WebSiteUrl %>" /></font>
                                                </td>
                                                <td align="left" valign="middle">
                                                    <asp:TextBox ID="txtWebURL" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                </td>
                                            </tr>                                         
                                            <tr>
                                                <td colspan="2">
                                                </td>
                                            </tr>
                                        </table>
                                        <div style="width: 100%; text-align: center;">
                                            &nbsp;&nbsp;<asp:Button ValidationGroup="signup" ID="btnSubmit1" runat="server" CssClass="normalButtons" OnClick="btnSubmit1_Click" />
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    </asp:Panel>
                    
                </td>
            </tr>
        </table>
    </div>
    <asp:Literal ID="ltScript" runat="server"></asp:Literal>
    </asp:content>
