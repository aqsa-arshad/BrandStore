<%@ Page Language="C#" AutoEventWireup="true" CodeFile="editpolls.aspx.cs" Inherits="AspDotNetStorefrontAdmin.editpolls" 
MaintainScrollPositionOnPostback="true" MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>
<%@ Register TagPrefix="aspdnsf" TagName="EntityToStore" Src="controls/EntityToStoreMapper.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <div class="breadCrumb3">
    <asp:Literal ID="ltScript" runat="server" /> 
    <asp:Literal ID="ltValid" runat="server" />
    <asp:Literal runat="server" ID="ltStyles" />
    </div>
    <asp:Panel ID="plnError" runat="server">
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;" class="breadCrumb3">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    </asp:Panel>
     <div id="container">
        <asp:Literal ID="ltInfo" runat="server"></asp:Literal>
     <div style="margin-top: 5px; margin-bottom: 15px;">
        <p><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editpollanswer.PollInfo %>" /></p>
     </div>
     
     <table width="100%" cellpadding="4" cellspacing="0">
        <tr valign="middle">
            <td width="100%" colspan="2" align="left"></td>
        </tr>
        <tr>
            <td></td>
            <td align="left" valign="top">
            <br/>
            <asp:Button ID="btnSubmit" CssClass="normalButtons" runat="server" OnClick="btnSubmit_Click" />
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:Button ID="btnReset" CssClass="normalButtons" runat="server" Text="<%$Tokens:StringResource, admin.common.Reset %>" OnClick="btnReset_Click" />
            </td>
        </tr>
        <tr valign="middle">
            <td width="25%" align="right" valign="middle">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editpollanswer.PollName %>" />&nbsp;&nbsp;</td>
            <td align="left" valign="top">
                <asp:Literal ID="ltName" runat="server"></asp:Literal>
            </td>
        </tr>
        <tr>
            <td width="25%" align="right" valign="middle"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.common.ExpiresOn %>" />:</td>
            <td align="left" valign="top">
				<telerik:RadDatePicker ID="txtDate" runat="server" Style="z-index: 150000;">
					<Calendar UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x" />
					<DatePopupButton HoverImageUrl="" ImageUrl="" />
				</telerik:RadDatePicker>
            </td>
        </tr>  
        <tr valign="middle">  
            <td align="right" valign="middle">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editpollanswer.PollSortOrder %>" />:&nbsp;&nbsp;</td>
            <td align="left" valign="top">
                <asp:DropDownList ID="ddlSortOrder" runat="server"></asp:DropDownList>
            </td>
        </tr>
        <tr valign="middle">
            <td align="right" valign="middle">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.currencies.Published %>" />:&nbsp;&nbsp;</td>
            <td align="left">
                <asp:RadioButtonList ID="rbPublished" RepeatDirection="Horizontal" runat="server">
                <asp:ListItem Value="1" Selected="true" Text="<%$Tokens:StringResource, admin.common.Yes %>"></asp:ListItem>
                <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>"></asp:ListItem>                                                            
                </asp:RadioButtonList>
            </td>
        </tr>
        <tr valign="middle">
            <td align="right" valign="middle">*<asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editpollanswer.AnonsCanVote %>" />:&nbsp;&nbsp;</td>
            <td align="left" valign="top">
                <asp:RadioButtonList ID="rbAnon" RepeatDirection="Horizontal" runat="server">
                <asp:ListItem Value="1" Selected="true" Text="<%$Tokens:StringResource, admin.common.Yes %>"></asp:ListItem>
                <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.common.No %>"></asp:ListItem>                                                            
                </asp:RadioButtonList>
            </td>
        </tr>
        <tr>
            <td valign="middle" align="right">
                <asp:Panel ID="litStoreMapperHdr" runat="server">
                    <font class="subTitleSmall">
                    <asp:Literal ID="ltStore" runat="server" Text="<%$Tokens:StringResource, admin.topic.mapstores %>" />
                    </font>
                </asp:Panel>
            </td>
            <td>
                <asp:Panel ID="litStoreMapper" runat="server" >
                    <aspdnsf:EntityToStore ID="etsMapper" runat="server"
                        EntityType="Polls" 
                        Text=""
                        />
                </asp:Panel>
            </td>
        </tr>
        <tr valign="top">
            <td align="right" valign="top"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editpollanswer.Category %>" />:&nbsp;&nbsp;</td>
            <td align="left" valign="top">
                <table width="100%" cellpadding="0" cellspacing="0" border="0">
                    <tr>
                        <td align="left" valign="top"><asp:Literal ID="ltCategoryList" runat="server"></asp:Literal></td>
                        <td align="right" valign="top"><asp:Label runat="server" Text="<%$Tokens:StringResource, admin.editpollanswer.Section %>" />:&nbsp;&nbsp;</td>
                        <td align="left" valign="top"><asp:Literal ID="ltSectionList" runat="server"></asp:Literal></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td></td>
            <td align="left" valign="top">
                <br/>
                <asp:Button ID="btnSubmit1" CssClass="normalButtons" runat="server" OnClick="btnSubmit_Click" />
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:Button ID="btnReset1" CssClass="normalButtons" runat="server" Text="<%$Tokens:StringResource, admin.common.Reset %>" OnClick="btnReset_Click" />
            </td>
        </tr>
     </table>
    </div>
    <asp:Literal ID="ltScript1" runat="server"></asp:Literal>
</asp:Content>