<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BrandAssetExample.ascx.cs" Inherits="controls_BrandAssetExample" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<div class="beforeloginControl hide-element">
     <aspdnsf:Topic ID="Topic2" runat="server" TopicName="Home.DisplayAndSignage" />
</div>

<div class="content-box-02 thumbnail afterloginControl hide-element">
     <aspdnsf:Topic ID="Topic1" runat="server" TopicName="Home.DisplayAndSignage" />   
</div>
