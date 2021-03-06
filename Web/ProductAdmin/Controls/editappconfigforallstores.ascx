﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="editappconfigforallstores.ascx.cs" Inherits="AspDotNetStorefrontAdmin.Controls.EditAppConfigForAllStores" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
    <script type="text/javascript" src="Scripts/jquery.min.js"></script>
<div class='<%# this.CssClass %>'>

    <asp:Panel ID="pnlError" runat="server" Visible='<%# HasErrors %>' CssClass="config_error" >
        <asp:Label ID="lblError" runat="server" ></asp:Label>
    </asp:Panel>
    
    <table style="width:500px;border-style:none;" >
        <tr>
            <td valign="top" align="right">Name:</td>
            <td valign="top" align="left" >
                <asp:Label ID="lblName" runat="server" 
                    Visible='true'
                    Text='<%# Datasource.Name %>'>
                </asp:Label>
            </td>
        </tr>
        <tr>
            <td valign="top" align="right">Description:</td>
            <td valign="top" align="left" >
            
                <%--<asp:Label ID="lblDescription" runat="server" 
                    Visible='<%# AddMode == false %>'
                    Text='<%# Datasource.Description %>'>
                </asp:Label>--%>
                
                <asp:TextBox ID="txtDescription" runat="server" 
                    TextMode="MultiLine" 
                    Rows='<%# DetermineProperTextRowLength(Datasource.Description) %>'
                    Columns='<%# DetermineProperTextColumnLength(Datasource.Description) %>'
                    Text='<%# Datasource.Description %>' >
                </asp:TextBox>
            </td>
        </tr>
        
         <%--<tr id="trStores" runat="server" visible='<%# Stores.Count > 1 %>'>
            <td valign="top" align="right">Store:</td>
            <td valign="top" align="left" >
                <asp:DropDownList ID="cboStores" runat="server" >
                </asp:DropDownList>
            </td>
        </tr>--%>
        <asp:PlaceHolder ID="phStoreValues" runat="server" />

        
        <tr>
            <td valign="top" align="right">Group:</td>
            <td valign="top" align="left" >
                <asp:DropDownList ID="cboGroupName" runat="server" 
                    DataSource='<%# ConfigGroups %>'>
                </asp:DropDownList>
            </td>
        </tr>
        
        <tr id="trAdvanced" runat="server" visible="false">
            <td valign="top" align="right"><br /></td>
            <td valign="top" align="left" >
                <a href="javascript:void(0);" onclick="$get('<%= pnlAdvanced.ClientID %>').style.display = $get('<%= pnlAdvanced.ClientID %>').style.display == 'none'? '':'none';" >Advanced</a>
            </td>
        </tr>
        
    </table>
    
    <%--<asp:Panel ID="pnlAdvanced" runat="server" Visible="false" >--%>
    <div id="pnlAdvanced" runat="server" visible="false" class="appconfig_adv" style='display:none;'>
    
        <table style="width:500px;border-style:none;" >
            <tr>
                <td valign="top" align="right">Super Only:</td>
                <td valign="top" align="left" >
                    <asp:RadioButtonList ID="rbSuperOnly" runat="server" RepeatDirection="Horizontal" />
                    <%--Will be bound on the code-behind--%>
                </td>
            </tr>
            
            <tr>
                <td valign="top" align="right">Value Type:</td>
                <td valign="top" align="left" >
                    <%--Will be bound on the code-behind--%>
                    <asp:DropDownList ID="cboValueType" runat="server" />
                </td>
            </tr>
            
            <tr>
                <td valign="top" align="right">Allowable Values:</td>
                <td valign="top" align="left" >
                    <%--Will be bound on the code-behind--%>
                    <asp:TextBox ID="txtAllowableValues" runat="server" 
                        TextMode="MultiLine"
                        Columns="35" 
                        Rows="3">
                    </asp:TextBox>
                </td>
            </tr>
        </table>
        
    </div>

</div>

