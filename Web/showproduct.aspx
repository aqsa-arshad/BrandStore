<%@ Page Language="c#" Inherits="AspDotNetStorefront.showproduct" CodeFile="showproduct.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" EnableViewStateMac="false" %>

<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <%--Thankyou POP UP Start here --%>
    <div id="divThankyouPopUp" class="modal fade" tabindex="-3" role="dialog" aria-labelledby="myModalLabel">
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

    <asp:Label ID="hdnInventory" name="hdnInventory" runat="server" ClientIDMode="Static" Style="display: none" Text="0" />
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
    <asp:Label ID="hdnSelectedFundType" name="hdnSelectedFundType" EnableViewState="true" ViewStateMode="Enabled" Autopostbox="false" runat="server" ClientIDMode="Static" Style="display: none" Text="1" />
    <%--End Hidden Variables Region--%>

    <%-- Region Open Pop Up for bucckts--%>
    <div class="modal fade" id="myModa2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" data-keyboard="false" data-backdrop="static">
        <div class="modal-dialog modal-checkout" role="document">
            <div class="modal-content">
                <div class="modal-body">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close" id="btncancelforblubucks" ClientIDMode="Static">
                        <img src="App_Themes/Skin_3/images/close-popup.png" alt="Close" /></button>
                    <h4 class="text-uppercase-no">APPLY BLU™ BUCKS</h4>
                    <p runat="server" id="ppointscount">You have XXXXXX BLU™ Bucks you can use to purchase items.</p>
                    <p runat="server" id="ppercentage">You can pay for up to XX% of this item's cost with BLU™ Bucks.</p>

                    <div class="form-group">
                        <div class="col-xs-12 padding-none">
                            <label class="roman-black">BLU™ Bucks to be applied:</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="txtBluBuksUsed" MaxLength="10" onpaste="return false" AutoCompleteType="Disabled" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                        </div>
                        <div class="clearfix"></div>
                    </div>

                    <p class="label-text">
                        <span class="roman-black">Price using BLU™ Bucks:</span>
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
    <div class="modal fade" id="myModal1" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" data-keyboard="false" data-backdrop="static">
        <div class="modal-dialog modal-checkout" role="document">
            <div class="modal-content">
                <div class="modal-body">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close" id="btncancelforsof" ClientIDMode="Static">
                        <img src="App_Themes/Skin_3/images/close-popup.png" alt="Close" /></button>
                    <h4 class="text-uppercase-no">SPECIFY FUNDS </h4>
                    <p runat="server">
                        Apply General Funds(GF), Sales Operation Funds(SOF) or authorized Capital Expenditure Fund Code(CAPEX) to your item.</br>
                                     If not applicable select "NO FUNDS"</br>
                          <a class="underline-link" href="#"><span>About sales funds</span></a>
                    </p>

                    <div class="btn-funds-main">
                        <button class="btn btn-primary margin-top-none" id="btnGeneralFunds">GF</button>
                        <button class="btn btn-primary margin-top-none pull-right" id="btnSOFFunds">SOF</button>
                        <div class="clearfix"></div>
                        <button class="btn btn-primary" id="btnCapitalExpenditure">CAPEX</button>
                       <button class="btn btn-primary pull-right" id="btnNoFund">NO FUNDS</button>
                    </div>

                    <p id="pGeneralFunds" class="label-text hide-items">
                        Enter the quantity of general funds you'd like to apply to pay for this item:
                    </p>
                    <p id="pSOFFunds" class="label-text hide-items">
                        Enter the quantity of SOF LOREUM FUND you'd like to apply to pay for this item:
                    </p>
                    <p id="pCapitalExpenditure" class="label-text hide-items">
                        Enter a valid authentication code to mark this item as a capital expenditure.
                    </p>
                    <p id="pNoFund" class="label-text hide-items">
                        Enter the quantity of general funds you'd like to apply to pay for this item
                    </p>

                    <p class="label-text">
                        <span class="roman-black">Regular price:</span>
                        <span id="sppriceforsalesrep" runat="server" clientidmode="Static">$0,000.00 </span>
                    </p>
                    <div class="form-group hide-items" id="divSOFFunds">
                        <div class="col-xs-12 padding-none hide-items" id="divGeneralFunds">
                            <label class="roman-black">General Funds Used</label>
                        </div>
                        <div class="col-xs-12 padding-none hide-items" id="lblSOFunds">
                            <label class="roman-black">SOF Funds Used</label>
                        </div>
                        <p class="col-xs-6 padding-none">
                            <asp:TextBox ID="txtproductcategoryfundusedforsalesrep" onpaste="return false" AutoCompleteType="Disabled" MaxLength="7" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                        </p>
                        <div class="clearfix"></div>
                        <div class="col-xs-12 padding-none hide-items" id="deptCode">
                            <label class="roman-black">Department Code(3-digit)</label>
                        </div>
                        <p class="col-xs-6 padding-none hide-items" id="txtDept">
                            <asp:TextBox ID="txtSOFCode" MaxLength="3" onpaste="return false" AutoCompleteType="Disabled" ClientIDMode="Static" placeholder="000" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                        </p>
                        <div class="clearfix"></div>

                    </div>
                    <div class="form-group hide-items" id="divCapitalExpenditure">
                        <div class="col-xs-12 padding-none">
                            <label class="roman-black">Authentication code</label>
                        </div>
                        <p class="col-xs-6 padding-none">
                            <asp:TextBox ID="txtCAPEX" MaxLength="10" onpaste="return false" AutoCompleteType="Disabled" ClientIDMode="Static" placeholder="000" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                        </p>
                        <div class="clearfix"></div>
                    </div>
                    <div class="buttons-group trueblue-popup">
                        <asp:Button ID="btnaddtocartforsalesrep" ClientIDMode="Static" CssClass="btn btn-primary btn-block margin-top-none" Text="ADD TO CART" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%--End Region Open Pop Up For SOF Funds--%>
    <script type="text/javascript">
        $(document).ready(function () {

            $("#btnGeneralFunds").addClass("btn-funds");
            $("#btnSOFFunds").removeClass("btn-funds");
            $("#btnCapitalExpenditure").removeClass("btn-funds");
            $("#btnNoFund").removeClass("btn-funds");

            $("#divGeneralFunds").removeClass("hide-items");
            $("#pGeneralFunds").removeClass("hide-items");
            $("#divSOFFunds").removeClass("hide-items");
            $("#txtSOFCode").attr("autocomplete", "off");
            $("#txtCAPEX").attr("autocomplete", "off");

            var SOFCode = $("#hdnSelectedFundType").text();
            $.ajax({
                type: "post",
                url: "showproduct.aspx/SetSOFCodeChoice",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    "SOFCodeValue": SOFCode
                }),
                dataType: "json",
                async: false,
                success: function (result) {
                }
            });

        //New CHECK ADDED ON 3-3-2016 by tayyab to clear field in cross button click
            $("#btncancelforsof").click(function () {
            $("#txtproductcategoryfundusedforsalesrep").val(0);
            $("#txtproductcategoryfundusedforsalesrep").trigger("focusout");
            });

            $("#btncancelforblubucks").click(function () {
            $("#txtBluBuksUsed").val(0);
             $("#txtBluBuksUsed").trigger("focusout");
            });
        //End
             
            $("#btnGeneralFunds").click(function (e) {
                $("#btnGeneralFunds").addClass("btn-funds");
                $("#btnSOFFunds").removeClass("btn-funds");
                $("#btnCapitalExpenditure").removeClass("btn-funds");
                $("#btnNoFund").removeClass("btn-funds");

                function round(value, decimals) {
                    if (value == "" || isNaN(value))
                        return 0;

                    return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
                }

                $("#txtproductcategoryfundusedforsalesrep").val(round($("#hdnProductFundAmountUsed").text(), 2));
                $("#txtproductcategoryfundusedforsalesrep").trigger("focusout");

                $("#divGeneralFunds").removeClass("hide-items");
                $("#pGeneralFunds").removeClass("hide-items");
                $("#divSOFFunds").removeClass("hide-items");

                $("#pSOFFunds").addClass("hide-items");
                $("#lblSOFunds").addClass("hide-items");
                $("#deptCode").addClass("hide-items");
                $("#txtDept").addClass("hide-items");

                $("#pCapitalExpenditure").addClass("hide-items");
                $("#divCapitalExpenditure").addClass("hide-items");

                $("#hdnSelectedFundType").text("1");
                var SOFCode = $("#hdnSelectedFundType").text();
                $.ajax({
                    type: "post",
                    url: "showproduct.aspx/SetSOFCodeChoice",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        "SOFCodeValue": SOFCode
                    }),
                    dataType: "json",
                    async: false,
                    success: function (result) {
                    }
                });

                e.preventDefault();
            });
            $("#btnSOFFunds").click(function (e) {

                $("#btnGeneralFunds").removeClass("btn-funds");
                $("#btnSOFFunds").addClass("btn-funds");
                $("#btnCapitalExpenditure").removeClass("btn-funds");
                $("#btnNoFund").removeClass("btn-funds");

                function round(value, decimals) {
                    if (value == "" || isNaN(value))
                        return 0;

                    return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
                }

                $("#txtproductcategoryfundusedforsalesrep").val(round($("#hdnProductFundAmountUsed").text(),2));
                $("#txtproductcategoryfundusedforsalesrep").trigger("focusout");

                $("#pSOFFunds").removeClass("hide-items");
                $("#lblSOFunds").removeClass("hide-items");
                $("#deptCode").removeClass("hide-items");
                $("#txtDept").removeClass("hide-items");
                $("#divSOFFunds").removeClass("hide-items");

                $("#pGeneralFunds").addClass("hide-items");
                $("#divGeneralFunds").addClass("hide-items");

                $("#pCapitalExpenditure").addClass("hide-items");
                $("#divCapitalExpenditure").addClass("hide-items");

                $("#hdnSelectedFundType").text("2");
                var SOFCode = $("#hdnSelectedFundType").text();
                $.ajax({
                    type: "post",
                    url: "showproduct.aspx/SetSOFCodeChoice",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        "SOFCodeValue": SOFCode
                    }),
                    dataType: "json",
                    async: false,
                    success: function (result) {
                    }
                });
                e.preventDefault();
            });
            $("#btnCapitalExpenditure").click(function (e) {

                $("#btnGeneralFunds").removeClass("btn-funds");
                $("#btnSOFFunds").removeClass("btn-funds");
                $("#btnCapitalExpenditure").addClass("btn-funds");
                $("#btnNoFund").removeClass("btn-funds");

                function round(value, decimals) {
                    if (value == "" || isNaN(value))
                        return 0;

                    return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
                }
                // setting price for capital expense
                var ItemOriginalPrice = $("#hdnproductactualprice").text();
                var ItemQuantity = theForm.Quantity_1_1.value;
                $("#sppriceforsalesrep").text("$" + round(ItemQuantity * ItemOriginalPrice, 2));

                $("#pCapitalExpenditure").removeClass("hide-items");
                $("#divCapitalExpenditure").removeClass("hide-items");

                $("#pGeneralFunds").addClass("hide-items");

                $("#pSOFFunds").addClass("hide-items");
                $("#divSOFFunds").addClass("hide-items");

                $("#hdnSelectedFundType").text("3");
                var SOFCode = $("#hdnSelectedFundType").text();
                $.ajax({
                    type: "post",
                    url: "showproduct.aspx/SetSOFCodeChoice",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        "SOFCodeValue": SOFCode
                    }),
                    dataType: "json",
                    async: false,
                    success: function (result) {
                    }
                });
                e.preventDefault();
            });
            $("#btnNoFund").click(function (e) {

                $("#btnGeneralFunds").removeClass("btn-funds");
                $("#btnSOFFunds").removeClass("btn-funds");
                $("#btnCapitalExpenditure").removeClass("btn-funds");
                $("#btnNoFund").addClass("btn-funds");

                function round(value, decimals) {
                    if (value == "" || isNaN(value))
                        return 0;

                    return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
                }
                var ItemOriginalPrice = $("#hdnproductactualprice").text();
                var ItemQuantity = theForm.Quantity_1_1.value;
                $("#sppriceforsalesrep").text("$" + round(ItemQuantity * ItemOriginalPrice, 2));

                $("#pGeneralFunds").addClass("hide-items");

                $("#pSOFFunds").addClass("hide-items");
                $("#divSOFFunds").addClass("hide-items");

                $("#pCapitalExpenditure").addClass("hide-items");
                $("#divCapitalExpenditure").addClass("hide-items");

                $("#hdnSelectedFundType").text("4");
                var SOFCode = $("#hdnSelectedFundType").text();
                $.ajax({
                    type: "post",
                    url: "showproduct.aspx/SetSOFCodeChoice",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        "SOFCodeValue": SOFCode
                    }),
                    dataType: "json",
                    async: false,
                    success: function (result) {
                    }
                });
                e.preventDefault();
            });
        });
    </script>
    <script type="text/javascript">
        $(document).ready(function () {
            //$("#pInStock").hide();
            //$("#pOutofStock").hide();
            var inventoryArray = jQuery.parseJSON($("#hdnInventory").text());
            $("#txtBluBuksUsed").attr("autocomplete", "off");
            $("#txtproductcategoryfundusedforsalesrep").attr("autocomplete", "off");

            if (inventoryArray.length >= 1) {
                $("#divNotifyme").hide();
                $("#divNotifymepopUp").hide();
            }

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
                    $("#QtyDropDown").addClass("hide-element");
                }
                else {
                    $("#btnaddtocart").removeClass("hide-element");
                    $("#btnShoppingcart").addClass("hide-element");
                }
            }
            //end check

            $("#btnShoppingcart").click(function () {
                window.location.href = "ShoppingCart.aspx";

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

                if (EID == "" || EID == null || EID.length > 40) {
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

                var customerlevel = $("#hdncustomerlevel").text();
                if (customerlevel == 3 || customerlevel == 7) {
                    // aqsa arshad code block starts here : display price without applying fund 
                    var SOFChoice = $("#hdnSelectedFundType").text();
                    if (SOFChoice=="3" || SOFChoice == "4")
                    {
                        var PriceWithoutFund = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value);
                        $("#sppriceforsalesrep").text("$" + round(PriceWithoutFund.toFixed(2), 2));
                    }
                    // aqsa arshad code block starts here : display price without applying fund 
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
                if (applySOFValidationonselectedType()) {

                    if ($("#txtproductcategoryfundusedforsalesrep").trigger("focusout")) {
                        $(btnname).trigger("click");
                        return true;
                    }
                    else return false;
                }
                else {
                    return false;
                }

            });

            $("#txtproductcategoryfundusedforsalesrep").focusout(function () {

                var ItemOriginalPrice = $("#hdnproductactualprice").text();
                var ItemQuantity = theForm.Quantity_1_1.value;
                var newpricetotal = (ItemOriginalPrice * ItemQuantity);

                var spproductcategoryfund = round($("#hdnProductFundAmountUsed").text().replace("$", ""), 2);
                spproductcategoryfund = (spproductcategoryfund);

                $("#sppriceforsalesrep").text("$" + round(ItemQuantity * ItemOriginalPrice, 2));
                $("#spprice").text("$" + ItemQuantity * ItemOriginalPrice);
                newpricetotal = $("#sppriceforsalesrep").text().replace("$", "");
                var sofentered = ($("#txtproductcategoryfundusedforsalesrep").val());
                if (applySOFValidation(newpricetotal, sofentered, spproductcategoryfund)) {
                    $("#spprice").text("$" + (ItemQuantity) * (ItemOriginalPrice));
                    var updatedprice = round($("#spprice").text().replace("$", ""), 2) - round($("#txtproductcategoryfundusedforsalesrep").val(), 2);
                    $("#spprice").text("$" + updatedprice.toFixed(2));
                    $("#sppriceforsalesrep").text("$" + round(updatedprice.toFixed(2), 2));
                    var ProductCategoryFundUsed = $("#txtproductcategoryfundusedforsalesrep").val();
                    var BluBucksUsed = 0;
                    return true;
                    //PageMethods.SaveValuesInSession(ProductCategoryFundUsed, BluBucksUsed, currentrecordid, onSucceed, onError);// onSucceed, onError
                }
                else {

                    var updatedprice = round($("#spprice").text().replace("$", ""), 2) - round($("#txtproductcategoryfundusedforsalesrep").val(), 2);//((ItemOriginalPrice * ItemQuantity) - round($("#hdnProductFundAmountUsed").val(),2));//$("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val();               

                    $("#spprice").text("$" + round(updatedprice, 2));
                    $("#sppriceforsalesrep").text("$" + round(updatedprice, 2));
                    return false;

                }
            });
            function applySOFValidationonselectedType() {
                // aqsa arshad code block starts here 
                var SOFCode = "0";
                var SOFChoice = $("#hdnSelectedFundType").text();
                if (SOFChoice == "1") {
                    return true;
                }
                else if (SOFChoice == "2") {
                    var SOFCode = $("#txtSOFCode").val();
                    if (SOFCode == "") {
                        alert("Code Field can not be empty");
                        return false;
                    }
                    else if (SOFCode.length<3) {
                        alert("Code must be 3 digit");
                        return false;
                    }
                    else {
                        return true;
                    }
                }
                else if (SOFChoice == "3") {
                    var SOFCode = $("#txtCAPEX").val();
                    if (SOFCode == "") {
                        alert("Code Field can not be empty");
                        return false;
                    }
                    else {
                        return true;

                    }
                }
                else if (SOFChoice == "4") {
                    return true;
                }
                // aqsa arshad code block ends here

            }
            function applySOFValidation(newpricetotal, sofentered, spproductcategoryfund) {

                if (spproductcategoryfund <= 0) {
                    $("#txtproductcategoryfundusedforsalesrep").val("0.00");
                    return true;
                }
                if ($("#txtproductcategoryfundusedforsalesrep").val() == "" || isNaN($("#txtproductcategoryfundusedforsalesrep").val())) {
                    $("#txtproductcategoryfundusedforsalesrep").val("0.00");
                    return true;
                    // return false;
                }
                else if (round(sofentered, 2) > round(spproductcategoryfund, 2)) {
                    alert("You exceed available SOF");
                    $("#txtproductcategoryfundusedforsalesrep").val(round($("#hdnProductFundAmountUsed").text(), 2));
                    return false;
                }
                else if (round(sofentered, 2) > round(newpricetotal, 2)) {
                    alert("You exceed price limit");
                    $("#txtproductcategoryfundusedforsalesrep").val(round($("#hdnProductFundAmountUsed").text(), 2));
                    return false;
                }
                else {
                    return true;
                }
            }

            function round(value, decimals) {
                if (value == "" || isNaN(value))
                    return 0;

                return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
            }

            //end area for pop up for sales rep
            $("#btnaddtocart").click(function (e) {

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
                    if ((customerlevel == 13 || customerlevel == 4 || customerlevel == 5 || customerlevel == 6) && (round($("#spprice").text().replace("$", 0), 2) > 0 && round($("#hdnBluBucktsPoints").text(), 2) > 0)) {

                        var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                        $("#spprice").text("$" + updatedprice.toFixed(2));

                        $("#txtBluBuksUsed").val($("#spprice").text().replace("$", ""));
                        $("#spprice").text($("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val());
                        applyblubuksvalidation2();

                        $("#txtBluBuksUsed").trigger("focusout");

                        $("#btnaddtocart").attr("data-toggle", "modal");
                        $("#btnaddtocart").attr("data-target", "#myModa2");
                    }
                    else if ((customerlevel == 3 || customerlevel == 7)) {
                        if (round($("#hdnProductFundAmountUsed").text(), 2) > 0) {
                            var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - ($("#hdnProductFundAmountUsed").text());
                            $("#spprice").text("$" + updatedprice.toFixed(2));
                            $("#sppriceforsalesrep").text("$" + updatedprice.toFixed(2));

                            $("#txtproductcategoryfundusedforsalesrep").val(round(($("#hdnProductFundAmountUsed").text()), 2));

                            $("#btnaddtocart").attr("data-toggle", "modal");
                            $("#btnaddtocart").attr("data-target", "#myModal1");
                        }

                        else {
                            $("#btnaddtocart").removeAttr("data-toggle", "modal");
                            $("#btnaddtocart").removeAttr("data-target", "#myModa2");
                            $("#btnaddtocart").removeAttr("data-target", "#myModal1");

                            $("#btnaddtocartforsalesrep").trigger("click");

                        }

                    }
                    else {
                        $("#btnaddtocart").removeAttr("data-toggle", "modal");
                        $("#btnaddtocart").removeAttr("data-target", "#myModa2");
                        $("#btnaddtocart").removeAttr("data-target", "#myModal1");
                        $(btnname).trigger("click");
                    }
                }
                else {
                    $("#txtBluBuksUsed").val($("#spprice").text().replace("$", ""));
                    $("#spprice").text($("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val());
                    applyblubuksvalidation2();

                    $("#txtBluBuksUsed").trigger("focusout");
                }
            });

            // CallBack method when the page call success
            function onSucceed(results, currentContext, methodName) {

            }
            //CallBack method when the page call fails due to internal, server error 
            function onError(results, currentContext, methodName) {

            }
            function checkifproductalreadyexists() {
                var exist = false;
                var sel_size = '0';
                var sel_color = '0';
                if ($("#Size_1_1").length > 0) {
                    sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                    sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                }

                if ($("#Color_1_1").length > 0) {
                    sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                    sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                }

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
                    applyblubuksvalidation2();
                    var updatedprice = $("#spprice").text().replace("$", "") - $("#txtBluBuksUsed").val();
                    $("#spprice").text("$" + updatedprice.toFixed(2));
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
                ApplyValidation2(theForm);

            });
            $("#Quantity_1_1").focusout(function () {

                ApplyValidation2(theForm);
                $("#hdnquantity").text(theForm.Quantity_1_1.value);
                setpricewithquantitychange();
                applyblubuksvalidation2();
                $("#txtBluBuksUsed").trigger("focusout");

            });

            //Shehriyar's Code
            $("#Color_1_1").change(function () {
                debugger;
                if (inventoryArray.length > 1) {
                    //if ($("#Size_1_1").length > 0) {
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
                    else {
                        $("#pInStock").hide();
                        $("#pOutofStock").hide();
                    }
                    //}
                }
                ApplyValidation2(theForm);
            });
            //End

            $("#Size_1_1").change(function () {
                var customerlevel = $("#hdncustomerlevel").text();

                if (customerlevel == 1 || customerlevel == 8 || customerlevel == 0) {
                    $("#sppricewithfund").addClass("hide-element");

                }
                else {

                    $("#sppricewithfund").removeClass("hide-element");
                }

                //Shehriyar's Code

                if (inventoryArray.length > 1) {
                    var sel_color;
                    if ($("#Color_1_1").length <= 0) {
                        sel_color = "";
                    } else {
                        sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                        sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    }
                    var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                    sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');
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
                    } else {
                        $("#pInStock").hide();
                        $("#pOutofStock").hide();
                    }
                }
                ApplyValidation2(theForm);
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
                    regex = new RegExp("^[0-9\b]+$");

                    var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
                    if (regex !== "") {
                        if (regex.test(str)) {
                            return true;
                        }
                        else {
                            e.preventDefault();
                            return false;
                        }
                    }

                }

            });

            function setpricewithquantitychange() {

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
                if (updatedtotalprice < 0) {
                    updatedtotalprice = 0;
                }
                $("#hdnpricewithfund").text(updatedtotalprice);
                $("#spprice").text("$" + updatedtotalprice.toFixed(2));
                $("#sppriceforsalesrep").text("$" + updatedtotalprice.toFixed(2));
                $("#sppricewithfund").html("<font>Price with " + $("#hdnFundName").text() + " credit: $</font>" + updatedtotalprice.toFixed(2));

            }

            function applyblubuksvalidation() {

                var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                $("#spprice").text("$" + updatedprice.toFixed(2));
                var maxfundlimit = $("#spprice").text().replace("$", "") * (round($("#hdnBudgetPercentValue").text(), 2) / 100)
                if (round($("#spprice").text().replace("$", ""), 2) <= 0) {
                    $("#txtBluBuksUsed").val(0);
                    return true;
                }
                if ($("#txtBluBuksUsed").val() == "" || isNaN($("#txtBluBuksUsed").val())) {
                    $("#txtBluBuksUsed").val(0);
                    return true;
                    //return false;
                }
                else if (round($("#txtBluBuksUsed").val(), 2) > round($("#hdnBluBucktsPoints").text(), 2)) {
                    alert("BLU BUKS cannot be greater than allowed limit");
                    $("#txtBluBuksUsed").val($("#hdnBluBucktsPoints").text());
                    applyblubuksvalidation2();

                    return false;
                }
                else if (round($("#txtBluBuksUsed").val(), 2) > round(maxfundlimit, 2)) {
                    alert("BLU BUKS cannot be greater than allowed limit");
                    $("#txtBluBuksUsed").val(round(maxfundlimit));
                    applyblubuksvalidation2();
                    return false;
                }
                else if (round($("#txtBluBuksUsed").val(), 2) > round($("#spprice").text().replace("$", ""), 2)) {
                    //alert("BLU BUKS cannot be greater than allowed limit");
                    $("#txtBluBuksUsed").val($("#spprice").text().replace("$", "").toFixed(2));
                    applyblubuksvalidation2();
                    return false;
                }
                else
                    return true;
            }

            function applyblubuksvalidation2() {

                var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                $("#spprice").text("$" + round(updatedprice, 2).toFixed(2));
                var maxfundlimit = $("#spprice").text().replace("$", "") * (round($("#hdnBudgetPercentValue").text(), 2) / 100);

                if (($("#spprice").text().replace("$", "")) <= 0) {
                    $("#txtBluBuksUsed").val(0);
                    return true;
                }

                var min = Math.min(updatedprice, maxfundlimit, $("#hdnBluBucktsPoints").text());
                $("#txtBluBuksUsed").val(round(min, 2));

            }
            function applyproductcategoryfund() {
                $("#spprice").text("$" + round($("#hdnpricewithfund").text(), 2).toFixed(2));
                $("#sppriceforsalesrep").text("$" + round($("#hdnpricewithfund").text(), 2).toFixed(2));
                $("#sppricewithfund").html("<font>Price with" + $("#hdnFundName").text() + " credit:</font> $" + round($("#hdnpricewithfund").text(), 2).toFixed(2));
                $("#hdnproductactualprice").text($("meta[itemprop=price]").attr("content").replace("$", "").replace(",", "").replace(" ", ""));
                var customerlevel = $("#hdncustomerlevel").text();
                if (customerlevel == 1 || customerlevel == 8) {
                    $("#sppricewithfund").addClass("hide-element");

                }
                else {

                    if (round($("#hdnProductFundAmount").text(), 2) > 0)
                        $("#sppricewithfund").removeClass("hide-element");
                    else
                        $("#sppricewithfund").addClass("hide-element");
                }

            }

            function ApplyValidation(theForm) {

                if ($("#Quantity_1_1").length <= 0) {
                    submitenabled(theForm);
                    return (true);
                }

                submitonce(theForm);
                if ((theForm.Quantity_1_1.value * 1) < 1) {
                    alert("Please specify the quantity you want to add to your cart");
                    $("#btnaddtocart").removeAttr("data-toggle", "modal");
                    $("#btnaddtocart").removeAttr("data-target", "#myModa2");
                    $("#btnaddtocart").removeAttr("data-target", "#myModal1");
                    theForm.Quantity_1_1.focus();
                    submitenabled(theForm);
                    return (false);

                }
                if ($("#Size_1_1").length > 0) {
                    var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                    sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    if (theForm.Size_1_1.selectedIndex < 1) {
                        alert("Please select a size.");
                        $("#btnaddtocart").removeAttr("data-toggle", "modal");
                        $("#btnaddtocart").removeAttr("data-target", "#myModa2");
                        $("#btnaddtocart").removeAttr("data-target", "#myModal1");
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
                        $("#btnaddtocart").removeAttr("data-toggle", "modal");
                        $("#btnaddtocart").removeAttr("data-target", "#myModa2");
                        $("#btnaddtocart").removeAttr("data-target", "#myModal1");
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
                        //  $("#Quantity_1_1").trigger("focusout");                       
                        submitenabled(theForm);
                        return (false);
                    }
                } else {
                    //Shehriyar's Code Start                      
                    var sel_color;
                    if ($("#Color_1_1").length <= 0) {
                        sel_color = "";
                    } else {
                        sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                        sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    }
                    var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                    sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');

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
                            if (result.d.toString() != '0') {
                                $("#pInStock").show();
                                $("#lblInStock").text(result.d);

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
                                $("#pOutofStock").hide();
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

            function ApplyValidation2(theForm) {

                if ($("#Quantity_1_1").length <= 0) {
                    submitenabled(theForm);
                    return (true);
                }

                submitonce(theForm);
                if ((theForm.Quantity_1_1.value * 1) < 1) {
                    //  alert("Please specify the quantity you want to add to your cart");
                    $("#btnaddtocart").removeAttr("data-toggle", "modal");
                    $("#btnaddtocart").removeAttr("data-target", "#myModa2");
                    $("#btnaddtocart").removeAttr("data-target", "#myModal1");
                    theForm.Quantity_1_1.focus();
                    submitenabled(theForm);
                    return (false);
                }

                if (inventoryArray.length <= 1) {
                    if (theForm.Quantity_1_1.value > SelectedVariantInventory_1_1) {
                        alert("Your quantity exceeds stock on hand. The maximum quantity that can be added is " + SelectedVariantInventory_1_1 + ". Please contact us if you need more information.");
                        if ($("#Quantity_1_1").prop('type') == 'text') {
                            theForm.Quantity_1_1.value = SelectedVariantInventory_1_1;
                        }
                        else {
                            jQuery("#Quantity_1_1 option:first-child").attr("selected", true);
                        }

                        theForm.Quantity_1_1.focus();
                        $("#Quantity_1_1").trigger("focusout");
                        submitenabled(theForm);
                        return (false);
                    }
                } else {
                    //Shehriyar's Code Start                      
                    var sel_color;
                    if ($("#Color_1_1").length <= 0) {
                        sel_color = "";
                    } else {
                        sel_color = theForm.Color_1_1[theForm.Color_1_1.selectedIndex].value;
                        sel_color = sel_color.substring(0, sel_color.indexOf(',')).replace(new RegExp("'", 'gi'), '');
                    }
                    var sel_size = theForm.Size_1_1[theForm.Size_1_1.selectedIndex].value;
                    sel_size = sel_size.substring(0, sel_size.indexOf(',')).replace(new RegExp("'", 'gi'), '');

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
                            if (result.d.toString() != '0') {
                                $("#pInStock").show();
                                $("#lblInStock").text(result.d);

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
                                $("#pOutofStock").hide();
                            }
                        }
                    });
                }
                //End Code

                submitenabled(theForm);
                return (true);
            }
        });
    </script>
</asp:Content>

