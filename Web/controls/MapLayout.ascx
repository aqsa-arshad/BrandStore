<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MapLayout.ascx.cs" Inherits="AspDotNetStorefront.MapLayout" %>
<%@ Register TagPrefix="aspdnsf" TagName="layoutlistitem" Src="LayoutListItem.ascx" %>

<asp:Panel ID="pnlMapLayout" runat="server" width="600" height="400">
<asp:UpdatePanel ID="upList" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
</Triggers>
<ContentTemplate>

<div id="div1" style="overflow-x: hidden; overflow-y: scroll; width:200px; height: 375px; float:left;" runat="server">
<asp:Repeater ID="rptrList" runat="server" OnItemDataBound="rptrList_ItemDataBound"  OnItemCommand="rptrList_ItemCommand">
<AlternatingItemTemplate>
<asp:LinkButton ID="lbtnA" runat="server" CommandName="Details">
<aspdnsf:layoutlistitem ID="lliA" runat="server" BackColor="LightGray" />
</asp:LinkButton>
</AlternatingItemTemplate>
<ItemTemplate>
<asp:LinkButton ID="lbtn" runat="server" CommandName="Details">
<aspdnsf:layoutlistitem ID="lli" runat="server" BackColor="WhiteSmoke" />
</asp:LinkButton>
</ItemTemplate>
</asp:Repeater>
</div>

</ContentTemplate>
</asp:UpdatePanel>
<asp:UpdatePanel ID="upDetails" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
<Triggers>
</Triggers>
<ContentTemplate>
<div style="margin-left:210px;margin-top:10px;height:375px;" runat="server">
<asp:Literal ID="ltdetails" runat="server" />
<asp:Literal ID="ltselectedidlabel" runat="server" Text='<%$ Tokens:StringResource, layouts.map.selectedid  %>' />&nbsp;<asp:Literal ID="ltselectedid" runat="server" />

<asp:Literal ID="ltselectednamelabel" runat="server" Text='<%$ Tokens:StringResource, layouts.map.selectedname  %>' />&nbsp;<asp:Literal ID="ltselectedname" runat="server" />

<asp:Literal ID="ltpreview" runat="server" Text='<%$ Tokens:StringResource, layouts.map.preview  %>' />&nbsp;

<asp:Literal ID="ltcurrentpagelabel" runat="server" Text='<%$ Tokens:StringResource, layouts.map.currentpage  %>' />&nbsp;<asp:Literal ID="ltcurrentpage" runat="server" />

<asp:Literal ID="ltobjecttypelabel" runat="server" Text='<%$ Tokens:StringResource, layouts.map.objecttype  %>' />&nbsp;<asp:Literal ID="ltobjecttype" runat="server" />

<asp:Literal ID="ltobjectidlabel" runat="server" Text='<%$ Tokens:StringResource, layouts.map.objectid  %>' />&nbsp;<asp:Literal ID="ltobjectid" runat="server" />

</div>

</ContentTemplate>
</asp:UpdatePanel>
</asp:Panel>
