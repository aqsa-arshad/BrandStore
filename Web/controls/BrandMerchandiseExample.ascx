<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BrandMerchandiseExample.ascx.cs" Inherits="controls_BrandMerchandiseExample" %>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>

<div class="beforeloginControl hide-element">
    <h4>Promotional Items</h4>
    <div class="row">
        <div class="col-md-10">
            <div class="primary-img-box">
                <img id="productimmage" runat="server" class="img-responsive" src="App_Themes/Skin_3/images/brand-ex-img.png">
            </div>
        </div>
    </div>
    <p id="productdescription" runat="server">Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut </p>
</div>

<div class="content-box-02 thumbnail afterloginControl hide-element">
   <aspdnsf:Topic ID="PromotionalItemsTopicForInternalusers" runat="server" TopicName="home.promotionalItemsInternal" Visible="false" />
    <aspdnsf:Topic ID="PromotionalItemsTopicForDealers" runat="server" TopicName="home.promotionalItemsDealers"  Visible="false"/>
</div>
