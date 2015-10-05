﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BrandMerchandiseExample.ascx.cs" Inherits="controls_BrandMerchandiseExample" %>
<div class="beforelogin">
    <h4>Brand Asset</h4>
    <div class="row">
        <div class="col-sm-6">
            <div class="primary-img-box">
                <img id="productimmage" runat="server" class="img-responsive" src="App_Themes/Skin_3/images/brand-ex-img.png">
            </div>
        </div>
    </div>
    <p id="productdescription" runat="server">Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut </p>
</div>

<div class="content-box-02 afterlogin">

    <h4>Brand Merchandise Overview headline goes here  </h4>
    <p id="productdescriptionAfterLogin" runat="server">
        Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore 
             
    </p>

    <div class="row">
        <div class="col-md-8">
            <div class="primary-img-box">
               <img id="productimmageAfterLogin" runat="server" class="img-responsive" src="App_Themes/Skin_3/images/brand-ex-img.png">
            </div>
           <div class="img-tag-line">branded Merchandise</div>
        </div>
    </div>
    <button class="btn btn-md btn-primary btn-block" type="submit">See All</button>
    <div class="clearfix"></div>

</div>