<%@ Page Language="c#" Inherits="AspDotNetStorefront.showproduct" CodeFile="showproduct.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" EnableViewStateMac="false" %>

<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <%--Thankyou POP UP Start here --%>
    <div id="divThankyouPopUp" class="modal fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
        <div class="modal-dialog modal-checkout" role="document">
            <div class="modal-content">
                <div class="modal-body">
                    <button type="button" id="Closebutton" class="close" data-dismiss="modal" aria-label="Close">
                        <img src="App_Themes/Skin_3/images/close-popup.png" alt="Close"></button>
                    <h4 id="hNotification">Thank you!</h4>
                    <p id="pNotification">
                        You will receive an email when this item is back in stock.
                    </p>
                </div>
            </div>
        </div>
    </div>
    <button type="button" id="HiddenButton" class="btn btn-primary margin-none" data-toggle="modal" data-target="#divThankyouPopUp" style="display: none">Click me</button>
    <%--Thankyou POP UP Start here --%>
    <asp:Panel runat="server">
        <asp:Literal ID="litOutput" runat="server"></asp:Literal>
       
    </asp:Panel>
    <%--Hidden Variables Regions--%>

    <asp:Label ID="hdnProductFundID" name="hdnProductFundID" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnProductFundAmount" name="hdnProductFundAmount" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnProductFundAmountUsed" name="hdnProductFundAmountUsed" EnableViewState="true" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnBluBucktsPoints" name="hdnBluBucktsPoints" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnBudgetPercentValue" name="hdnBudgetPercentValue" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnProductCategoryID" name="hdnProductCategoryID" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnpricewithfund" name="hdnpricewithfund" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnproductprice" name="hdnprice" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnButtonName" name="hdnButtonName" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnproductactualprice" name="hdnproductactualprice" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdncustomerlevel" name="hdncustomerlevel" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnFundName" name="hdnFundName" runat="server" ClientIDMode="Static" Style="display: none" Text="" />
    <asp:Label ID="hdnIsProductExist" name="hdnIsProductExist" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <asp:Label ID="hdnquantity" name="hdnquantity" EnableViewState="true" ViewStateMode="Enabled" Autopostbox="false" runat="server" ClientIDMode="Static" Style="display: none" Text="1" />
    <asp:Label ID="hdnProductID" name="hdnProductID" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
     <asp:Label ID="hdnVariantID" name="hdnVariantID" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
     <asp:Label ID="hdnCustomerID" name="hdnCustomerID" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
    <%--End Hidden Variables Region--%>

    <%-- Region Open Pop Up for bucckts--%>
    <div class="modal fade" id="myModa2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
        <div class="modal-dialog modal-checkout" role="document">
            <div class="modal-content">
                <div class="modal-body">
                     <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <img src="App_Themes/Skin_3/images/close-popup.png" alt="Close"></button>
                    <h5 class="text-uppercase-no">True BLU(tm)</h5>
                    <p runat="server" id="ppointscount">You have XXXXXX BLU(tm) Bucks you can use to purchase items.</p>
                     <p runat="server" id="ppercentage">You can pay for up to XX% of this item's cost with BLU Bucks.</p>

                    <div class="form-group">
                        <div class="col-xs-12 padding-none">
                            <label class="roman-black">BLU Bucks to be applied:</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="txtBluBuksUsed" MaxLength="10" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>

                        </div>
                        <div class="clearfix"></div>
                    </div>

                    <p class="label-text">
                        <span class="roman-black">Price using BLU Bucks:</span>
                        <span id="spprice" runat="server" clientidmode="Static">$0,000.00</span>
                    </p>
                    <div class="buttons-group trueblue-popup">
                        <div>
                            <asp:Literal ID="LiteralCustom" runat="server"></asp:Literal>
                           <%-- <button type="button" data-dismiss="modal" class="btn btn-primary">Cancel</button>--%>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <%--End Region Open Pop Up for bucckts--%>

    <%-- Region Open PopUp for SOF Funds--%>

    <!-- Modal -->
    <div class="modal fade" id="myModal1" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
        <div class="modal-dialog modal-checkout" role="document">
            <div class="modal-content">
                <div class="modal-body">
                     <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <img src="App_Themes/Skin_3/images/close-popup.png" alt="Close"></button>
                    <h5 class="text-uppercase-no">Apply sales funds to this item</h5>
                    <p>Apply sales funds by entering a GL code and the amount of the funds you want to use below:</p>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-xs-6 col-sm-7">
                                <label class="roman-black">GL Code:</label>
                                <asp:TextBox ID="txtGLcode" MaxLength="12" ClientIDMode="Static" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                            </div>
                            <div class="col-xs-6 col-sm-5">
                                <label class="roman-black">Amount:</label>
                                <asp:TextBox ID="txtproductcategoryfundusedforsalesrep" MaxLength="7" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>

                            </div>
                        </div>

                    </div>

                    <p class="label-text">

                        <span class="roman-black">Total price using sales funds:</span>
                        <span id="sppriceforsalesrep" runat="server" clientidmode="Static">$0,000.00 </span>
                    </p>
                    <div class="buttons-group trueblue-popup">

                        <%-- <asp:Literal ID="LiteralCustom2" runat="server"></asp:Literal>--%>
                        <asp:Button ID="btnaddtocartforsalesrep" ClientIDMode="Static" CssClass="btn btn-primary btn-block" Text="<%$ Tokens:StringResource,AppConfig.CartButtonPrompt %>" runat="server" />

                        <%--<button type="button" data-dismiss="modal" class="btn btn-primary">Cancel</button>--%>
                    </div>
                </div>
            </div>
        </div>
    </div>


    <%--End Region Open Pop Up For SOF Funds--%>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#pInStock").hide();
            $("#pOutofStock").hide();

            var inventoryArray;
            $.ajax({
                type: "post",
                url: "showproduct.aspx/GetInventoryList",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                async: true,
                success: function (result) {
                    inventoryArray = $.parseJSON(result.d);
                    //aqsa code block starts here
                    if (inventoryArray.length >= 1) {
                        $("#divNotifyme").hide();
                        $("#divNotifymepopUp").hide();

                    }
                    //aqsa code block ends here
                }
            });

            //Check if product exist
            if (($("#Size_1_1").length > 0 || $("#Color_1_1").length > 0)) {
                        
                if ($("#hdnIsProductExist").text() == "1") {
                    
                }
                else {
                    $("#btnaddtocart").removeClass("hide-element");
                    $("#btnShoppingcart").addClass("hide-element");
                }                             
            }
            else {                
                if ($("#hdnIsProductExist").text() == "1") {                    
                    $("#btnaddtocart").addClass("hide-element");
                    $("#btnShoppingcart").removeClass("hide-element");
                    $("#palreadyexist").removeClass("hide-element");
                    
                }
                else {
                    $("#btnaddtocart").removeClass("hide-element");
                    $("#btnShoppingcart").addClass("hide-element");
                }
            }
            //end check

            $("#btnShoppingcart").click(function () {              
                window.location.href ="ShoppingCart.aspx";

            });

            if ('<%=parentCategoryID%>' == "1") {
                $("#MCCategory1").addClass("active");
            }
            else if ('<%=parentCategoryID%>' == "2") {
                $("#MCCategory2").addClass("active");
            }
            else if ('<%=parentCategoryID%>' == "3") {
                $("#MCCategory3").addClass("active");
            }
            else if ('<%=parentCategoryID%>' == "4") {
                $("#MCCategory4").addClass("active");
            }
            else if ('<%=parentCategoryID%>' == "5") {
                $("#MCCategory5").addClass("active");
            }
            else {
                $("#MCCategory6").addClass("active");
            }
            //aqsa arshad code block starts here
            $("#btnSubmit").click(function (e) {
                document.getElementById('lblErrorMsg').style.display = 'none';
                var IID = "-1";
                if (inventoryArray.length > 1) {
                    var psize = "";
                    var pcolor = "";
                    if ($("#Size_1_1").length > 0) {
                        psize = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                        psize = psize.substring(0, psize.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    }
                    if ($("#Color_1_1").length > 0) {
                        pcolor = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                        pcolor = pcolor.substring(0, pcolor.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    }
                    var varientID = $("#hdnVarientId").val();
                    if (psize != "-" && pcolor != "-") {
                        $.ajax({
                            type: "post",
                            url: "showproduct.aspx/GetInventoryID",
                            contentType: "application/json; charset=utf-8",
                            data: JSON.stringify({
                                "color": pcolor,
                                "size": psize,
                                "varientID": varientID
                            }),
                            dataType: "json",
                            async: false,
                            success: function (result) {     
                                IID = result.d;
                            }
                        });
                    }
                    else {
                        alert("please selct valid color and size");
                        e.preventDefault();
                        return false;
                    }
                }

                var EID = $("#txtOutOfStock").val();
                var PID = $("#hdnProductId").val();
                var VID = $("#hdnVarientId").val();
                
                if (EID == "" || EID == null) {
                    document.getElementById('lblErrorMsg').style.display = 'block';
                    e.preventDefault();
                    return false;
                }
                else {
                    var regex = /^[a-z][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*)?@[a-z][a-zA-Z-0-9]*\.[a-z]+(\.[a-z]+)?$/;
                    var flag = regex.test(EID);
                    if (flag == true) {
                        document.getElementById('lblErrorMsg').style.display = 'none';
                        $.ajax({
                            type: "post",
                            url: "showproduct.aspx/InsertCustomersToBeNotifiedInDB",
                            contentType: "application/json; charset=utf-8",
                            data: JSON.stringify({
                                "PId": PID,
                                "VId": VID,
                                "EId": EID,
                                "IId": IID
                            }),
                            dataType: "json",
                            async: true,
                            success: function (result) {
                                $("#HiddenButton").trigger("click");
                            },
                            error: function (result) {
                                alert('error occured');
                                alert(result.responseText);

                            },
                        });
                    }
                    else {
                        document.getElementById('lblErrorMsg').style.display = 'block';
                        e.preventDefault();
                        return false;

                    }
                }
            });
            //aqsa arshad code block ends here
            var btnname = "#" + $("#hdnButtonName").text();

            $(btnname).click(function () {
                debugger;
                var customerlevel = $("#hdncustomerlevel").text();
                if (customerlevel == 3 || customerlevel == 7) {
                    return true;
                }
                else {

                    $("#spprice").text("$" + $("#hdnpricewithfund").text());
                    if (applyblubuksvalidation()) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            });

            //Area for pop up for sales rep
            $("#btnaddtocartforsalesrep").click(function () {
                if ($("#txtGLcode").val() == "") {
                    alert("Please enter GL code.");
                    return false;
                }
                if ($("#txtproductcategoryfundusedforsalesrep").trigger("focusout")) {
                    $(btnname).trigger("click");
                    return true;
                }
                else return false;
            });

            $("#txtproductcategoryfundusedforsalesrep").focusout(function () {
                debugger;
                $("#spprice").text("$" + $("#hdnpricewithcategoryfundapplied").text());
                $("#hdncurrentrecordid").text();
                //  var currentrecordid = $("#hdncurrentrecordid").text();
                var ItemOriginalPrice = $("#hdnproductactualprice").text();
                // var quantityfieldid = "#" + $("#hdntoreplace").text() + "txtQuantity";
                var ItemQuantity = theForm.Quantity_1_1.value;
                var newpricetotal = $("#spprice").text().replace("$", "");// (ItemOriginalPrice * ItemQuantity) - $("#spregularprice_" + currentrecordid).text().replace("$", "").replace("Regular Price: ", "");
                // var ProductCategoryID = $("#spItemProductCategoryId_" + currentrecordid).text().replace("$", "");
                // var BluBucksPercentage = $("#spBluBucksPercentageUsed_" + currentrecordid).text().replace("$", "");

                var spproductcategoryfund = parseFloat($("#hdnProductFundAmountUsed").text().replace("$", ""));
                spproductcategoryfund = parseFloat($("#hdnsoffundamount").text()) + parseFloat(spproductcategoryfund)
                // $("#hdnsoffundamount").text(spproductcategoryfund);

                $("#spprice").text("$" + parseFloat(ItemQuantity) * parseFloat(ItemOriginalPrice));
                $("#sppriceforsalesrep").text("$" + parseFloat(ItemQuantity) * parseFloat(ItemOriginalPrice));
                newpricetotal = $("#spprice").text().replace("$", "");
                var sofentered = parseFloat($("#txtproductcategoryfundusedforsalesrep").val());

                if (applySOFValidation(newpricetotal, sofentered, spproductcategoryfund)) {
                    $("#spprice").text("$" + parseFloat(ItemQuantity) * parseFloat(ItemOriginalPrice));
                    var updatedprice = $("#spprice").text().replace("$", "") - $("#txtproductcategoryfundusedforsalesrep").val();
                    $("#spprice").text("$" + updatedprice.toFixed(2));
                    $("#sppriceforsalesrep").text("$" + updatedprice.toFixed(2));
                    var ProductCategoryFundUsed = $("#txtproductcategoryfundusedforsalesrep").val();
                    var BluBucksUsed = 0;
                    return true;
                    //PageMethods.SaveValuesInSession(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, onSucceed, onError);// onSucceed, onError
                }
                else {
                    return false;

                }
            });

            function applySOFValidation(newpricetotal, sofentered, spproductcategoryfund) {
                if ($("#txtproductcategoryfundusedforsalesrep").val() == "" || isNaN($("#txtproductcategoryfundusedforsalesrep").val())) {
                    return false;
                }
                else if (parseFloat(sofentered) > parseFloat(spproductcategoryfund)) {
                    alert("You exceed available SOF");
                    $("#txtproductcategoryfundusedforsalesrep").val("0.00");
                    return false;
                }
                else if (parseFloat(sofentered) > parseFloat(newpricetotal)) {
                    alert("You exceed price limit");
                    $("#txtproductcategoryfundusedforsalesrep").val("0.00");
                    return false;
                }
                else {
                    return true;
                }
            }
           
            //end area for pop up for sales rep
            $("#btnaddtocart").click(function (e) {
                debugger;
                if (checkifproductalreadyexists()) {
                    $("#palreadyexist").removeClass("hide-element");
                    $("#palreadyexist").html("<span class=\"notify\">Product with selected options already exists in cart,Please go to shopping cart and update quantity or select different option.</span>");
                    $("#palreadyexist").addClass("notify");
                    $("#palreadyexist").removeAttr("Style");
                    e.preventDefault();
                    return false;
                }
                else {
                    $("#palreadyexist").addClass("hide-element");
                }

                
                if (ApplyValidation(theForm)) {
                    var btnname = "#" + $("#hdnButtonName").text();
                    var customerlevel = $("#hdncustomerlevel").text();
                    if (customerlevel == 13 || customerlevel == 4 || customerlevel == 5 || customerlevel == 6) {

                        var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                        $("#spprice").text("$" + updatedprice.toFixed(2));

                        $("#btnaddtocart").attr("data-toggle", "modal");
                        $("#btnaddtocart").attr("data-target", "#myModa2");

                    }
                    else if (customerlevel == 3 || customerlevel == 7) {

                        var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                        $("#spprice").text("$" + updatedprice.toFixed(2));
                        $("#sppriceforsalesrep").text("$" + updatedprice.toFixed(2));
                        $("#txtproductcategoryfundusedforsalesrep").val($("#hdnProductFundAmountUsed").text());

                        $("#btnaddtocart").attr("data-toggle", "modal");
                        $("#btnaddtocart").attr("data-target", "#myModal1");
                      
                      
                    }
                    else {
                        $(btnname).trigger("click");
                    }
                }               
            });

            function checkifproductalreadyexists()
            {
                var exist = false;
                var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                var sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');               
                var ProductID = $("#hdnProductID").text();
                var VariantID = $("#hdnVariantID").text();
                var CustomerID = $("#hdnCustomerID").text();
                if (sel_size != "-" && sel_color != "-") {
                    $.ajax({
                        type: "post",
                        url: "showproduct.aspx/IsProductExist",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({
                            "PId": ProductID,
                            "VId": VariantID,
                            "SelectedColour": sel_color,
                            "SelectedSize": sel_size,
                            "CustomerID": CustomerID
                        }),
                        dataType: "json",
                        async: false,
                        success: function (result) {                          
                           
                            if (result.d.toString() == 'true') {
                               // alert("Product with selected options already exists,Please go to shopping cart and update quantity or select different option.");
                               // $("#btnaddtocart").addClass("hide-element");
                                $("#btnShoppingcart").removeClass("hide-element");                               
                                exist = true;
                               
                            } else {                               
                                $("#btnaddtocart").removeClass("hide-element");
                                $("#btnShoppingcart").addClass("hide-element");                               
                                exist = false;
                              
                            }
                        },
                        error: function (result) {                       

                    }
                    });
                }
               
                return exist;

            }

            //Set product price  to show on pupup
            applyproductcategoryfund();
            setpricewithquantitychange();
            $("#txtBluBuksUsed").focusout(function () {
                $("#spprice").text("$" + $("#hdnpricewithfund").text());
                if (applyblubuksvalidation()) {
                    var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                    $("#spprice").text("$" + updatedprice.toFixed(2));
                    var updatedprice = $("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val();
                    $("#spprice").text("$" + updatedprice.toFixed(2));
                }
                else {
                }
            });

            $("#txtBluBuksUsed").keypress(function (evt) {
                var charCode = (evt.which) ? evt.which : event.keyCode
                if (charCode == 46)
                    return true;
                else if (charCode > 31 && (charCode < 48 || charCode > 57))
                    return false;
                else
                    return true;
            });

            $("#Quantity_1_1").change(function () {
                debugger;
                $("#hdnquantity").text(theForm.Quantity_1_1.value);
                setpricewithquantitychange();
            });

            //Shehriyar's Code
            $("#Color_1_1").change(function () {
                debugger;
                if (inventoryArray.length > 1) {
                    if ($("#Size_1_1").length > 0) {
                        var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                        sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                        var sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                        sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');

                        if (sel_size != "-" && sel_color != "-") {
                            $.ajax({
                                type: "post",
                                url: "showproduct.aspx/GetQuantity",
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify({
                                    "color": sel_color,
                                    "size": sel_size,
                                    "lstInventories": inventoryArray
                                }),
                                dataType: "json",
                                async: true,
                                success: function (result) {
                                    console.log(result.d);
                                    if (result.d.toString() != '0') {
                                        $("#btnaddtocart").removeClass("hide-element");
                                        $("#pInStock").show();
                                        $("#lblInStock").text(result.d);
                                        $("#pOutofStock").hide();
                                        $("#QtyDropDown").show();
                                        $("#divNotifyme").hide();
                                        $("#divNotifymepopUp").hide();
                                    } else {
                                        $("#btnaddtocart").addClass("hide-element");
                                        $("#pInStock").hide();
                                        // $("#pOutofStock").show();       
                                        $("#QtyDropDown").hide();
                                        $("#divNotifyme").show();
                                        $("#divNotifymepopUp").show();

                                    }
                                }
                            });
                        }
                    }
                }
            });
            //End

            $("#Size_1_1").change(function () {
                debugger;
                var customerlevel = $("#hdncustomerlevel").text();
                if (customerlevel == 1 || customerlevel == 8) {
                    $("#sppricewithfund").addClass("hide-element");

                }
                else {
                    $("#sppricewithfund").removeClass("hide-element");
                }

                //Shehriyar's Code

                if (inventoryArray.length > 1) {
                    if ($("#Color_1_1").length > 0) {
                        var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                        sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                        var sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                        sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');

                        if (sel_size != "-" && sel_color != "-") {
                            $.ajax({
                                type: "post",
                                url: "showproduct.aspx/GetQuantity",
                                contentType: "application/json; charset=utf-8",
                                data: JSON.stringify({
                                    "color": sel_color,
                                    "size": sel_size,
                                    "lstInventories": inventoryArray
                                }),
                                dataType: "json",
                                async: true,
                                success: function (result) {
                                    console.log(result.d);
                                    if (result.d.toString() != '0') {
                                        $("#btnaddtocart").removeClass("hide-element");
                                        $("#pInStock").show();
                                        $("#lblInStock").text(result.d);
                                        $("#pOutofStock").hide();
                                        $("#QtyDropDown").show();
                                        $("#divNotifyme").hide();
                                        $("#divNotifymepopUp").hide();
                                    } else {
                                        $("#btnaddtocart").addClass("hide-element");
                                        $("#pInStock").hide();
                                        //$("#pOutofStock").show();
                                        $("#QtyDropDown").hide();
                                        $("#divNotifyme").show();
                                        $("#divNotifymepopUp").show();
                                    }
                                }
                            });
                        }
                    }
                }
                //End 
            });

            $('input').keypress(function (e) {
                var regex;
                if ($(this).attr('id') == "txtBluBuksUsed" || $(this).attr('id') == "txtproductcategoryfundusedforsalesrep") {
                    if ((event.which != 46 || $(this).val().indexOf('.') != -1) && ((event.which < 48 || event.which > 57) && (event.which != 0 && event.which != 8))) {
                      event.preventDefault();
                    }

                    var text = $(this).val();

                    if ((text.indexOf('.') != -1) && (text.substring(text.indexOf('.')).length > 2) && (event.which != 0 && event.which != 8) && ($(this)[0].selectionStart >= text.length - 2)) {
                        event.preventDefault();
                    }
                }
                else if ($(this).attr('id') == "Quantity_1_1") {
                    regex = new RegExp("^[0-9]+$");
                }
             
                var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
                if (regex.test(str)) {
                    return true;
               
            }

                e.preventDefault();
                return false;
            });

            function setpricewithquantitychange() {
                debugger;
                var updatedtotalprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value);
                var productfundamount = $("#hdnProductFundAmount").text();

                if (productfundamount < updatedtotalprice) {
                    updatedtotalprice = updatedtotalprice - productfundamount;
                    $("#hdnProductFundAmountUsed").text(productfundamount);
                }
                else {
                    productfundamount = productfundamount - updatedtotalprice;
                    $("#hdnProductFundAmountUsed").text(updatedtotalprice);
                    updatedtotalprice = 0;
                    $("#txtBluBuksUsed").text(updatedtotalprice);
                }
                $("#hdnpricewithfund").text(updatedtotalprice);
                $("#spprice").text("$" + updatedtotalprice.toFixed(2));
                $("#sppricewithfund").html("<font>Price with " + $("#hdnFundName").text() + " credit: $</font>" + updatedtotalprice.toFixed(2));

            }

            function applyblubuksvalidation() {

                var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                $("#spprice").text("$" + updatedprice.toFixed(2));
                var maxfundlimit = $("#spprice").text().replace("$", "") * (parseFloat($("#hdnBudgetPercentValue").text()) / 100)
                if ($("#txtBluBuksUsed").val() == "" || isNaN($("#txtBluBuksUsed").val())) {
                    return false;
                }
                else if (parseFloat($("#txtBluBuksUsed").val()) > parseFloat(maxfundlimit)) {
                    alert("BLU BUKS cannot be greater than allowed limit");
                    // $("#txtBluBuksUsed").val(maxfundlimit.toFixed(2));
                    $("#txtBluBuksUsed").val("0.00");
                    return false;
                }
                else if (parseFloat($("#txtBluBuksUsed").val()) > parseFloat($("#hdnBluBucktsPoints").text())) {
                    alert("You exceed available BLU BUKS");
                    // $("#txtBluBuksUsed").val(maxfundlimit.toFixed(2));
                    $("#txtBluBuksUsed").val("0.00");

                    return false;
                }
                else if (parseFloat($("#txtBluBuksUsed").val()) > parseFloat($("#spprice").text().replace("$", ""))) {
                    alert("BLU BUKS cannot be greater than product price");
                    // $("#txtBluBuksUsed").val(maxfundlimit.toFixed(2));
                    $("#txtBluBuksUsed").val("0.00");
                    return false;
                }
                else
                    return true;
            }
            function applyproductcategoryfund() {
                    $("#spprice").text("$" + parseFloat($("#hdnpricewithfund").text()).toFixed(2));
                    $("#sppricewithfund").html("<font>Price with" + $("#hdnFundName").text() + " credit:</font> $" + parseFloat($("#hdnpricewithfund").text()).toFixed(2));

                $("#hdnproductactualprice").text($("meta[itemprop=price]").attr("content").replace("$", "").replace(",", "").replace(" ", ""));

                var customerlevel = $("#hdncustomerlevel").text();
                if (customerlevel == 1 || customerlevel == 8) {
                    $("#sppricewithfund").addClass("hide-element");

                }
                else {
                    if (parseFloat($("#hdnProductFundAmount").text())>0)
                        $("#sppricewithfund").removeClass("hide-element");
                    else
                        $("#sppricewithfund").addClass("hide-element");

                }
            }

            function ApplyValidation(theForm) {
                debugger;
                if ($("#Quantity_1_1").length <= 0) {
                    submitenabled(theForm);
                    return (true);
                }
                submitonce(theForm);
                if ((theForm.Quantity_1_1.value * 1) < 1) {
                    alert("Please specify the quantity you want to add to your cart");
                    theForm.Quantity_1_1.focus();
                    submitenabled(theForm);
                    return (false);

                }
                if ($("#Size_1_1").length > 0) {
                    var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                    sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    if (theForm.Size_1_1.selectedIndex < 1) {
                        alert("Please select a size.");
                        theForm.Size_1_1.focus();
                        submitenabled(theForm);
                        return (false);
                    }
                }
                if ($("#Color_1_1").length > 0) {
                    var sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                    sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    if (theForm.Color_1_1.selectedIndex < 1) {
                        alert("Please select a color.");
                        theForm.Color_1_1.focus();
                        submitenabled(theForm);
                        return (false);
                    }
                }
                if (inventoryArray.length <= 1) {
                    if (theForm.Quantity_1_1.value > SelectedVariantInventory_1_1) {
                        alert("Your quantity exceeds stock on hand. The maximum quantity that can be added is " + SelectedVariantInventory_1_1 + ". Please contact us if you need more information.");
                        theForm.Quantity_1_1.value = SelectedVariantInventory_1_1;
                        theForm.Quantity_1_1.focus();
                        submitenabled(theForm);
                        return (false);
                    }
                } else {
                    //Shehriyar's Code Start                      
                    var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                    sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    var sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                    sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');

                    $.ajax({
                        type: "post",
                        url: "showproduct.aspx/GetQuantity",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({
                            "color": sel_color,
                            "size": sel_size,
                            "lstInventories": inventoryArray
                        }),
                        dataType: "json",
                        async: true,
                        success: function (result) {
                            if (result > 0) {
                                $("#spInStock").removeClass("hide-element");
                                $("#lblInStock").text = result;

                                if (typeof (sel_size) == 'undefined') sel_size = '';
                                if (typeof (sel_color) == 'undefined') sel_color = '';
                                var j = sel_size.indexOf("[");
                                if (j != -1) {
                                    sel_size = Trim(sel_size.substring(0, j));
                                }
                                var i = sel_color.indexOf("[");
                                if (i != -1) {
                                    sel_color = Trim(sel_color.substring(0, i));
                                }
                                var sel_size_master = sel_size;
                                var sel_color_master = sel_color;
                                var sel_qty = theForm.Quantity_1_1.value;
                                var sizecolorfound = 0;
                                for (i = 0; i < board_1_1.length; i++) {
                                    if (board_1_1[i][1] == sel_size_master && board_1_1[i][0] == sel_color_master) {
                                        sizecolorfound = 1;
                                        if (parseInt(sel_qty) > parseInt(board_1_1[i][2])) {
                                            if (parseInt(board_1_1[i][2]) == 0) {
                                                if (sel_color == '') sel_color = 'N/A';
                                                if (sel_size == '') sel_size = 'N/A';
                                                alert('Color: ' + sel_color + ', Size: ' + sel_size + ' is currently out of stock.\n\nPlease select another Color/Size combination.');
                                                theForm.Quantity_1_1.value = board_1_1[i][2];
                                                theForm.Quantity_1_1.focus();
                                            } else {
                                                if (sel_color == '') sel_color = 'N/A';
                                                if (sel_size == '') sel_size = 'N/A';
                                                alert('Your quantity exceeds our inventory on hand. The maximum quantity that can be added for Color: ' + sel_color + ', Size: ' + sel_size + ' is ' + board_1_1[i][2] + '.\n\nPlease reduce your quantity, or select another Color/Size combination.');
                                                theForm.Quantity_1_1.value = board_1_1[i][2];
                                                theForm.Quantity_1_1.focus();
                                            }
                                            submitenabled(theForm);
                                            return (false);
                                        }
                                    }
                                }
                                if (sizecolorfound == 0) {
                                    if (sel_color == '') sel_color = 'N/A';
                                    if (sel_size == '') sel_size = 'N/A';
                                    alert('Inventory Table Error - No Inventory Record Found For Color=[' + sel_color + '], Size=[' + sel_size + ']');
                                    submitenabled(theForm);
                                    return (false);
                                }
                            } else {
                                $("#spOutofStock").removeClass("hide-element");
                            }
                        }
                    });
                }
                //End Code

                submitenabled(theForm);
                return (true);
            }            function GetControlValue(id) {
                var CustomerLevelElemment;
                if (document.getElementById(id)) {
                    CustomerLevelElemment = document.getElementById(id);
                }
                else if (document.all) {
                    CustomerLevelElemment = document.all[id];
                }
                else {
                    CustomerLevelElemment = document.layers[id];
                }
                return CustomerLevelElemment.innerHTML;
            }
        });
    </script>
</asp:Content>

