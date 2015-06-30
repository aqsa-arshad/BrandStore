<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConfigureImagePanel.ascx.cs" Inherits="AspDotNetStorefront.ConfigureImagePanel" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>

<div class='<%# this.CssClass %>'>

    <asp:Panel ID="pnlError" runat="server" Visible='false' CssClass="config_error" >
        <asp:Label ID="lblError" runat="server" ></asp:Label>
    </asp:Panel>

            <table style="width:650px;border-style:none;" >
                <tr>
                    <td valign="top" align="left">
                        &nbsp;<b><asp:Label ID="Label1" Text="Layout:" runat="server"/></b>
                    </td>
                    <td valign="top" align="left">
                        &nbsp;<b><asp:Label ID="lblLayout" runat="server"/></b>
                    </td>
                </tr>
                <tr>
                    <td valign="top" align="left">
                        &nbsp;<b><asp:Label ID="Label10" Text="Field:" runat="server"/></b>
                    </td>
                    <td valign="top" align="left">
                        &nbsp;<b><asp:Label ID="lblField" runat="server"/></b>
                    </td>
                </tr>
                <tr>
                    <td valign="top" align="left">
                    &nbsp;<b><asp:Label ID="Label2" Text="Current Image:" runat="server"/></b>
                    </td>
                    <td valign="top" align="left" >
                        &nbsp;<asp:Image ID="imgCurrent" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td valign="top" align="left">
                    &nbsp;<b><asp:Label ID="Label5" Text="Image Details:" runat="server"/></b>
                    </td>
                    <td valign="top" align="left" >
                    &nbsp;<asp:Label ID="Label7" Text="Source:" runat="server" />&nbsp;<asp:Label ID="lblSource" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td valign="top" align="right" >
                    &nbsp;
                    </td>
                    <td valign="top" align="left" >
                    &nbsp;<asp:Label ID="Label8" Text="Alt:" runat="server" />&nbsp;<asp:Label ID="lblAlt" runat="server" />
                    </td>
                </tr>
                <tr runat="server" id="trWidth">
                    <td valign="top" align="right" >
                    &nbsp;
                    </td>
                    <td valign="top" align="left">
                    &nbsp;<asp:Literal ID="litWidth" Text="Width:" runat="server" />&nbsp;<asp:Label ID="lblWidth" runat="server" />
                    </td>
                </tr>
                <tr runat="server" id="trHeight">
                    <td valign="top" align="right" >
                    &nbsp;
                    </td>
                    <td valign="top" align="left" >
                    &nbsp;<asp:Literal ID="litHeight" Text="Height:" runat="server" />&nbsp;<asp:Label ID="lblHeight" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td valign="top" align="left">
                    &nbsp;<b><asp:Label ID="Label4" Text="Choose A New Image:" runat="server"/></b>
                    </td>
                    <td valign="top" align="left">
                        &nbsp;<asp:FileUpload ID="fuImageUpload" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td valign="top" align="left">
                    &nbsp;<b><asp:Label ID="Label12" Text="-or-" runat="server"/></b>
                    </td>
                    <td valign="top" align="left">
                        &nbsp;
                    </td>
                </tr>
                <tr>
                    <td valign="top" align="left">
                    &nbsp;<b><asp:Label ID="lblURL" Text="Enter Image URL" ToolTip="For images hosted off-site" runat="server"/></b>
                    </td>
                    <td valign="top" align="left">
                        &nbsp;<asp:TextBox ID="txtImageURL" Text="Currently Unavailable" Enabled="false" runat="server" />
                    </td>
                </tr>
                <%--<tr>
                    <td valign="top" align="left">
                        &nbsp;<b><asp:Label ID="Label9" Text="Use Image Resize:" runat="server"/></b>
                    </td>
                    <td valign="top" align="left">
                        <asp:CheckBox ID="cbUseImageResize" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td valign="top" align="left">
                        &nbsp;<b><asp:Label ID="Label11" Text="Alt Text:" runat="server"/></b>
                    </td>
                    <td valign="top" align="left">
                        &nbsp;<asp:TextBox ID="txtAlt" runat="server" />
                    </td>
                </tr>--%>
        <%--        <tr>
                    <td valign="top" align="right">Value:</td>
                    <td valign="top" align="left" >
                        <asp:PlaceHolder ID="plhConfigValueEditor" runat="server">                
                        </asp:PlaceHolder>
                    </td>
                </tr>--%>
                
                <tr id="trAdvanced" runat="server" visible="false">
                    
                    <td valign="top" align="left" >
                        &nbsp<a href="javascript:void(0);" onclick="$get('<%= pnlAdvanced.ClientID %>').style.display = $get('<%= pnlAdvanced.ClientID %>').style.display == 'none'? '':'none';" ><b>Advanced</b></a>
                    </td>
                    <td valign="top" align="right"></td>
                </tr>
                
            </table>
            
            <%--<asp:Panel ID="pnlAdvanced" runat="server" Visible="false" >--%>
            <div id="pnlAdvanced" runat="server" visible="false" class="modal_popup_Footer" style='display:none;'>
                <table style="width:500px;border-style:none;" >
                    <tr>
                        <td valign="top" align="left" width="200">
                            &nbsp;<b><asp:Label ID="Label9" AssociatedControlID="cbUseImageResize" Text="Use Image Resize:" runat="server"/></b>
                        </td>
                        <td valign="top" align="left">
                            <asp:CheckBox ID="cbUseImageResize" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" align="left" width="200">
                            &nbsp;<b><asp:Label AssociatedControlID="txtAlt" ID="Label11" Text="Alt Text:" runat="server"/></b>
                        </td>
                        <td valign="top" align="left">
                            &nbsp;<asp:TextBox ID="txtAlt" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" align="left" width="200">
                            &nbsp;<b><asp:Label ID="Label3" Text="Forcefully Constrain Image:" runat="server"/></b>
                        </td>
                        <td valign="top" align="left">
                            &nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" align="right" width="200" style="padding-right:100px;">
                            <asp:Literal ID="Literal1" Text="Width:" runat="server" />
                        </td>
                        <td valign="top" align="left">
                            &nbsp;<asp:TextBox ID="txtWidth" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td valign="top" align="right" width="200" style="padding-right:100px;">
                            <asp:Literal ID="Literal2" Text="Height:" runat="server" />
                        </td>
                        <td valign="top" align="left">
                            &nbsp;<asp:TextBox ID="txtHeight" runat="server" />
                        </td>
                    </tr>
                    <%--<tr>
                        <td valign="top" align="right">Super Only:</td>
                        <td valign="top" align="left" >
                            <asp:RadioButtonList ID="rbSuperOnly" runat="server" RepeatDirection="Horizontal" />
                            <%--Will be bound on the code-behind--
                        </td>
                    </tr>
                    
                    <tr>
                        <td valign="top" align="right">Value Type:</td>
                        <td valign="top" align="left" >
                            <%--Will be bound on the code-behind--
                            <asp:DropDownList ID="cboValueType" runat="server" />
                        </td>
                    </tr>
                    
                    <tr>
                        <td valign="top" align="right">Allowable Values:</td>
                        <td valign="top" align="left" >
                            <%--Will be bound on the code-behind--
                            <asp:TextBox ID="txtAllowableValues" runat="server" 
                                TextMode="MultiLine"
                                Columns="35" 
                                Rows="3">
                            </asp:TextBox>
                        </td>
                    </tr>--%>
                </table>
                
            </div>
</div>

