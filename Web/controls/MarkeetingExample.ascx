<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarkeetingExample.ascx.cs" Inherits="controls_MarkeetingExample" %>
<div class="beforeloginControl hide-element">
    <h4>Marketing Materials</h4>
    <div class="row">
        <div class="col-md-10">
            <div class="primary-img-box">
                <img runat="server" id="productimmage" class="img-responsive" src="App_Themes/Skin_3/images/market-ex-img.png">
            </div>
        </div>
    </div>
    <p id="productdescription" runat="server">Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut </p>
</div>

<div class="content-box-02 afterloginControl hide-element">
    <h4>Marketing Materials Overview headline here </h4>
    <p id="productdescriptionAfterLogin" runat="server">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore             
    </p>
    <div class="row">
        <div class="col-md-11">
            <div class="primary-img-box">
                <img id="productimmageAfterLogin" runat="server" class="img-responsive" src="App_Themes/Skin_3/images/brand-ex-img.png">
            </div>
            <div class="img-tag-line">brand Merchandise</div>
        </div>
    </div>
    <button class="btn btn-md btn-primary btn-block" type="submit">See All</button>
    <div class="clearfix"></div>
</div>
