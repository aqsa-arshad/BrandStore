<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityToStoreMapper.ascx.cs" Inherits="AspDotNetStorefrontControls.EntityToStoreMapper" %>
<%@ Register TagPrefix="aspdnsf" TagName="StoreSelector" Src="StoreSelector.ascx" %>


<asp:Literal ID="litWarning" runat="server" />

<aspdnsf:StoreSelector ID="ssMain" runat="server" 
    SelectMode="MultiCheckList" 
    AutoPostBack="false" 
    ListRepeatDirection="Vertical"
    />

