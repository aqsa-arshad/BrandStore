<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BrandMerchandiseExample.ascx.cs" Inherits="controls_BrandMerchandiseExample" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>

<div class="beforeloginControl hide-element">
    <aspdnsf:Topic ID="Topic1" runat="server" TopicName="home.promotionalItemsInternal" />
</div>

<div class="content-box-02 thumbnail afterloginControl hide-element">
   <aspdnsf:Topic ID="PromotionalItemsTopicForInternalusers" runat="server" TopicName="home.promotionalItemsInternal" Visible="false" />
    <aspdnsf:Topic ID="PromotionalItemsTopicForDealers" runat="server" TopicName="home.promotionalItemsDealers"  Visible="false"/>
</div>
