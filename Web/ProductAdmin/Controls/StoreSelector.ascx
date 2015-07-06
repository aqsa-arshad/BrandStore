<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StoreSelector.ascx.cs" Inherits="StoreSelector" %>


<asp:Literal ID='lblText' runat="server" 
    Text="<%$ Tokens:StringResource, StoreSelector.Header %>" />
    
<asp:CheckBoxList ID="lstMultiSelect" runat="server" 
    DataTextField="Name" DataValueField="StoreID" 
    />
    


<asp:RadioButtonList ID="lstSingleSelect" runat="server" 
    onselectedindexchanged="lstSingleSelect_SelectedIndexChanged"
    DataTextField="Name" DataValueField="StoreID" 
    />
    
<asp:DropDownList ID="cmbSingleList" runat="server"
    onselectedindexchanged="lstSingleSelect_SelectedIndexChanged"
    DataTextField="Name" DataValueField="StoreID"
    />

