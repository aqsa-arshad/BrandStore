<%@ Page Language="c#" Inherits="AspDotNetStorefront._default" CodeFile="default.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/empty.master"%>
<%@ Register TagPrefix="aspdnsf" TagName="Topic" Src="~/Controls/TopicControl.ascx" %>
<%@ Register TagPrefix="custom" TagName="Search" Src="Controls/Search.ascx" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="PageContent"> 
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
<html lang="en"><head>
    <meta charset="utf-8">
    <meta content="IE=edge" http-equiv="X-UA-Compatible">
    <meta content="width=device-width, initial-scale=1" name="viewport">
    <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
    <meta content="" name="description">
    <meta content="" name="author">

    <title>Home</title>

    <!-- Bootstrap core CSS -->
    <link rel="stylesheet" href="css/app.css">

  </head>

  <body>
    
<div class="container">

      <!-- The justified navigation menu is meant for single line per list item.
           Multiple lines will require custom code not provided by Bootstrap. -->
  <div class="header clearfix">
    <div class="header-part">
        <a href="default.aspx" class="logo"><img src="App_Themes/Skin_3/images/logo.png"></a>
        <span class="head-tagline">Brand Store</span>
    </div>
    <nav>
      <ul class="nav nav-justified">

        <li>
          <a href="#" class="nav-merchandise">
            Branded Merchandise
            <span>Possible description goes here?</span>
          </a>
        </li>

      </ul>
    </nav>
    <div class="user-grid">
      <input type="text" value="Search Site"  onblur="if(this.value==''){ this.value='Search Site';}"
  onfocus="if (this.value == 'Search Site') { this.value = ''; }">      
    </div>
  </div>

  <div class="body-container">
    <div class="row">
      <!-- left Contect Area  -->
      <div class="col-md-8">

        <div class="left-side">
            <%--Calling topic defind through admin console.--%>
             <aspdnsf:Topic ID="Topic2" runat="server" TopicName="LandingPageTopic" />         
        <!-- Contect Box 02  -->
        <div class="content-box-02" >

          <div class="row">  
             <%--calling xml package thst bringd featured products--%>    
              <asp:Literal ID="FeaturedProducts" runat="server" Text='<%$ Tokens:XMLPACKAGE, featuredproducts.xml.config, featuredentityid=11&featuredentitytype=category&headertext=Featured Products&numberofitems=4&columns=4&showprice=true %>' />
         </div>

        </div>
      </div>



    </div>
      <div class="col-md-4">
        <div class="side-bar">
          <!-- Login Box -->
          <div class="login-box" id="divlogin" runat="server">
           <%-- <h4>Login</h4>
             <%--Login control goes here--%>
          </div>

          <!-- Separator Line -->
          <div class="separator-line"></div>

          <!-- Side Box -->
          <div class="side-bar-box" id="divMarkeetingExample" runat="server">
           <%--Markeeting Example Category control goes here--%>
          </div>

          <!-- Separator Line -->
          <div class="separator-line"></div>

          <!-- Side Box -->
          <div class="side-bar-box" id="divBrandAssetExample" runat="server">
          <%--Brand Asset Category control goes here--%>
          </div>
        </div>
      </div>
    </div>
  </div>
      <!-- Site footer -->
      <footer class="footer">
        <div class="row">
        <div class="col-sm-6 col-md-4 footer-nav"><a href="#">Support</a></div>
        <div class="col-sm-6 col-md-4 footer-nav"><a href="#">Terms & Conditions</a></div><%--About TrueBLU™--%>
        <%--<div class="col-sm-6 col-md-4 footer-nav"><a href="#">Terms & Conditions</a></div>--%>
      </div>

      <div class="row">
           
        <div class="col-sm-2 col-md-2"><a href="default.aspx"><img src="App_Themes/Skin_3/images/bottom-logo.png"></a></div>
        <div class="col-sm-6 col-md-4 footer-privacy-link">©2016 JELD-WEN, Inc.  <span>|</span>   <a href="#">Privacy Policy</a></div>
      </div>

      </footer>

    </div>


    <!-- Bootstrap core JavaScript
    ================================================== -->
    <!-- Placed at the end of the document so the pages load faster -->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js"></script>
    <script src="../../dist/js/bootstrap.min.js"></script>

    <!-- IE10 viewport hack for Surface/desktop Windows 8 bug -->
    <script src="../../assets/js/ie10-viewport-bug-workaround.js"></script>

    <script src="offcanvas.js"></script>
  

</body></html>
    </asp:Content>


