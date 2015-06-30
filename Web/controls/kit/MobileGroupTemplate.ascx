<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileGroupTemplate.ascx.cs" Inherits="AspDotNetStorefront.Kit.MobileGroupTemplate" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
        <asp:HiddenField ID="hdfGroupID" runat="server" Value='<%# KitGroup.Id %>' />
<ul data-role="listview" class="MobileGroupTemplate">
    <li class="group" data-role="list-divider">
		<%# KitGroup.Name %>
    </li>
    <asp:PlaceHolder ID="pnlGroupDescription" runat="server"  Visible='<%# KitGroup.HasDescription || KitGroup.HasImage %>' >
            <li>
                <asp:Image ID="imgGroup" runat="server" CssClass="kit-group-image"  Visible='<%# KitGroup.HasImage %>' ImageUrl='<%# KitGroup.ImagePath %>' />
                <asp:Literal ID="Literal2" runat="server" Text='<%# KitGroup.Description %>' ></asp:Literal>                
            </li>
    </asp:PlaceHolder>
    <asp:PlaceHolder ID="phGroupSummary" runat="server" >
        <li>
            <asp:Panel ID="pnlGroupSummary" runat="server">
                <asp:Literal ID="litGroupSummary" runat="server" Text='<%# KitGroup.Summary %>'></asp:Literal>
            </asp:Panel>
        </li>
    </asp:PlaceHolder>
    <li>
        <asp:UpdatePanel ID="pnlUpdateKitItems" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true" >
            <ContentTemplate>
                <asp:PlaceHolder ID="plhKitItemTemplate" runat="server">
                </asp:PlaceHolder>
            </ContentTemplate>
        </asp:UpdatePanel>
    </li>
</ul>

        
            
            
              
        
            
            
        

