<%@ Page language="c#" AutoEventWireup="true" CodeFile="monthlymaintenance.aspx.cs" Inherits="AspDotNetStorefrontAdmin.monthlymaintenance" MaintainScrollPositionOnPostback="true"
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %> 

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">         
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="help">
        <div style="margin-bottom: 5px; margin-top: 5px;" class="breadCrumb3">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div runat="server" id="container">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.menu.Maintenance %>" />:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.PageInfo %>" />
                                        <br /><br />
                                        <div id="divMain" runat="server">
                                            <table cellpadding="1" cellspacing="0" border="0">
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAllShoppingCarts %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:DropDownList ID="ClearAllShoppingCarts" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Selected="True" Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />                                                        
                                                            </asp:DropDownList>
                                                        <asp:Image runat="server" id="imgClearAllShoppingCarts" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgClearAllShoppingCarts %>" />
                                                    </td>
                                                </tr> 
                                                <tr runat="server" id="trWishList">
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAllWishlists %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:DropDownList ID="ClearAllWishLists" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Selected="True" Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />                                                        
                                                        </asp:DropDownList>
                                                        <asp:Image runat="server" id="imgClearAllWishLists" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" border="0" style="cursor: normal;" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgClearAllWishLists %>"/>
                                                    </td>
                                                </tr> 
                                                <tr runat="server" id="trGR">
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAllGiftRegistries %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:DropDownList ID="ClearAllGiftRegistries" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Selected="True" Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />                                                        
                                                        </asp:DropDownList>
                                                        <asp:Image runat="server" id="imgClearAllGiftRegistries" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgClearAllGiftRegistries %>" />
                                                    </td>
                                                </tr> 
                                                <tr id="trEraseCC" runat="server">
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.EraseCCInfo %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:DropDownList ID="EraseOrderCreditCards" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Selected="True" Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />                                                        
                                                        </asp:DropDownList>
                                                        <asp:Image runat="server" id="imgEraseOrderCreditCards" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgEraseOrderCreditCards %>" />
                                                        </td>
                                                </tr> 
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.EraseSQLLog %>" />
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:DropDownList ID="EraseSQLLog" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Selected="True" Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />                                                        
                                                        </asp:DropDownList>
                                                        <asp:Image runat="server" id="imgEraseSQLLog" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgEraseSQLLog %>" />
                                                    </td>
                                                </tr>    
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class ="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearProfiles %>" />
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                          <asp:DropDownList ID="EraseProfileLog" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Selected="True" Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />                                                        
                                                        </asp:DropDownList>
                                                        <asp:Image runat="server" id="imgProfile" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" Tooltip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgProfile %>" />
                                                    </td> 
                                                 </tr>
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class ="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearViews %>" />
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                          <asp:DropDownList ID="ClearProductViewsOlderThan" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Selected="True" Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />
                                                        </asp:DropDownList>
                                                        <asp:Image runat="server" id="imgClearProductViewsOlderThan" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgClearProductViewsOlderThan %>" />
                                                    </td> 
                                                 </tr>   
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class ="subTitleSmall">
                                                            <asp:Literal ID="Literal1" runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearRTShippingData %>" />
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:DropDownList ID="dlClearRTShippingData" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Selected="True" Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />                                                        
                                                        </asp:DropDownList>
                                                        <asp:Image runat="server" id="Image1" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource, admin.monthlymaintenance.tooltip.imgClearRTShippingDataOlderThan %>" />
                                                    </td> 
                                                 </tr>   
                                                <tr>
                                                    <td align="right" style="width: 300px"><font class="subTitleSmall">
                                                        <asp:Literal ID="litsearchText" runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearSearchLog %>" />
                                                    </font></td>
                                                    <td align="left">
                                                        <asp:DropDownList ID="dlClearSearchData" CssClass="default" runat="server">
                                                            <asp:ListItem Value="0" Text="<%$Tokens:StringResource, admin.monthlymaintenance.LeaveUnchanged %>" />
                                                            <asp:ListItem Value="-1" Text="<%$Tokens:StringResource, admin.monthlymaintenance.ClearAll %>" />
                                                            <asp:ListItem Selected="True" Value="30" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan30 %>" />
                                                            <asp:ListItem Value="60" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan60 %>" />
                                                            <asp:ListItem Value="90" Text="<%$Tokens:StringResource, admin.monthlymaintenance.90 %>" />
                                                            <asp:ListItem Value="120" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan120 %>" />
                                                            <asp:ListItem Value="150" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan150 %>" />
                                                            <asp:ListItem Value="180" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MoreThan180 %>" />
                                                        </asp:DropDownList>
                                                        <asp:Image runat="server" ID="Image2" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource, admin.monthlymaintenance.tooltip.imgClearSearchLogOlderThan %>" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.InvalidateAllLogins %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:CheckBox ID="InvalidateUserLogins" runat="server" />
                                                        <asp:Image runat="server" id="imgInvalidateUserLogins" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgInvalidateUserLogins %>" />
                                                    </td>
                                                </tr>   
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.PurgeAnon %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:CheckBox ID="PurgeAnonUsers" runat="server" Checked="True" />
                                                        <asp:Image runat="server" id="imgPurgeAnonUsers" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgPurgeAnonUsers %>" />
                                                    </td>
                                                </tr> 
                                                <tr id="trEraseCC2" runat="server">
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.EraseCCInfoFromAddress %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:CheckBox ID="EraseAddressCreditCards" runat="server" Checked="True" />
                                                        <asp:Image runat="server" id="imgEraseAddressCreditCards" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgEraseAddressCreditCards %>" />
                                                    </td>
                                                </tr>                                                       
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.PurgeAll %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:CheckBox ID="PurgeDeletedRecords" runat="server" Checked="False" />
                                                        <asp:Image runat="server" id="imgPurgeDeletedRecords" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgPurgeDeletedRecords %>" />
                                                    </td>
                                                </tr>                                                       
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.TuneIndexes %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:CheckBox ID="TuneIndexes" runat="server" Checked="True" />
                                                        <asp:Image runat="server" id="imgTuneIndexes" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgTuneIndexes %>" />
                                                    </td>
                                                </tr>          
                                                <tr>
                                                    <td align="right" style="width: 300px">
                                                        <font class="subTitleSmall">
                                                            <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.SaveSettings %>" />:
                                                        </font>
                                                    </td>
                                                    <td align="left">
                                                        <asp:CheckBox ID="SaveSettings" runat="server" Checked="True" />
                                                        <asp:Image runat="server" id="imgSaveSettings" ImageUrl="~/App_Themes/Admin_Default/images/info.gif" ToolTip="<%$Tokens:StringResource,admin.monthlymaintenance.tooltip.imgSaveSettings %>" />
                                                    </td>
                                                </tr>                                           
                                                <tr>
                                                    <td>&nbsp;</td>
                                                    <td align="left" style="padding-top: 5px;">                                                        
                                                        <br />
                                                        <asp:Button ID="GOButton" runat="server" 
                                                            CssClass="normalButtons" Text="<%$Tokens:StringResource, admin.common.Go %>" OnClick="GOButton_Click"
                                                              />                                                        
                                                                                                                    
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="wrapper">
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle"><asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.Other %>" /></font>:
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        <asp:Literal runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.Reminder %>" />
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <asp:UpdatePanel ID="pnlTimer" runat="server">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="tmrMain" />
        </Triggers>
        <ContentTemplate>
        <asp:Timer ID="tmrMain" runat="server" OnTick="ShowTime_Tick" Interval='1000' Enabled="false" />
        
        <asp:Label ID="lblNotice" runat="server" Font-Bold="true" ForeColor="Red" Text="<%$Tokens:StringResource, admin.monthlymaintenance.Info %>"></asp:Label><br />
        <div runat="server" id="divRunning" class="dlgBox" >
            <div style="background:transparent url(images/kit/variantListHeader_background.jpg) repeat scroll 0 0; padding:10px; "> 
                <asp:Label ID="lblRunning" runat="server" Text="<%$Tokens:StringResource, admin.monthlymaintenance.MaintenanceRunning %>" Font-Size="X-Large" ForeColor="LightGray" />
            </div>
            <div style="padding-top:20px; padding-bottom:50px; padding-left:20px; padding-right:20px;">
                <asp:Label ID="lblStatus" runat="server" Visible="false" Font-Size="XX-Large" Text="<%$Tokens:StringResource, admin.monthlymaintenance.Processing %>"  /><br />
                <asp:Label ID="lblMessage" runat="server" Font-Size="Larger" Text="<%$Tokens:StringResource, admin.monthlymaintenance.Finished %>" />
            </div>
        </div>                                                  
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>