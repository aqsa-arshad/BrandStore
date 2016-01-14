<%@ Page Language="c#" Inherits="AspDotNetStorefront.showproduct" CodeFile="showproduct.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" EnableViewStateMac="false" %>

<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
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
    <asp:Label ID="hdnquantity" name="hdnquantity" EnableViewState="true" ViewStateMode="Enabled" Autopostbox="false" runat="server" ClientIDMode="Static" Style="display: none" Text="1" />
    <%--End Hidden Variables Region--%>
    <%-- Region Open Pop Up for bucckts--%>
    <div class="modal fade" id="myModa2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
        <div class="modal-dialog modal-checkout" role="document">
            <div class="modal-content">
                <div class="modal-body">
                    <h5 class="text-uppercase-no">True BLU</h5>
                    <p runat="server" id="ppointscount">You have XXXXXX BLU Bucks you can use to purchase your items.</p>
                    <p>Decide hom many BLU Bucks you want to use to purchase this item.</p>

                    <div class="form-group">
                        <div class="col-xs-6 padding-none">
                            <label class="roman-black">BLU Bucks used:</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="txtBluBuksUsed" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>

                        </div>
                        <div class="clearfix"></div>
                    </div>

                    <p class="label-text">
                        <span class="roman-black">Total price using BLU Bucks:</span>
                        <span id="spprice" runat="server" clientidmode="Static">$0,000.00</span>
                    </p>
                    <div class="buttons-group trueblue-popup">
                        <div>
                            <asp:Literal ID="LiteralCustom" runat="server"></asp:Literal>
                            <button type="button" data-dismiss="modal" class="btn btn-primary">Cancel</button>

                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <%--End Region Open Pop Up for bucckts--%>
    <script type="text/javascript">
        $(document).ready(function () {
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

            var btnname = "#" + $("#hdnButtonName").text();

            $(btnname).click(function (e) {
              
                $("#spprice").text($("#hdnpricewithfund").text());
                if (applyblubuksvalidation()) {
                    return true;
                }
                else {
                    return false;

                }

            });
            $("#btnaddtocart").click(function (e) {
                if (ApplyValidation(theForm)) {
                    debugger;
                    var btnname = "#" + $("#hdnButtonName").text();
                    var customerlevel = $("#hdncustomerlevel").text();
                    if (customerlevel == 13 || customerlevel == 4 ||  customerlevel == 5 || customerlevel == 6) {
                        var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                        $("#spprice").text("$" + updatedprice.toFixed(2));
                        $("#btnaddtocart").attr("data-toggle", "modal");
                        $("#btnaddtocart").attr("data-target", "#myModa2");
                    }                   
                    else {                      
                        $(btnname).trigger("click");
                    }
                }

            });           
           


            //Set product price  to show on pupup
            applyproductcategoryfund();
            setpricewithquantitychange();
            $("#txtBluBuksUsed").focusout(function () {
                $("#spprice").text($("#hdnpricewithfund").text());
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
                $("#sppricewithfund").html("<font>Price with FUND credit: $</font>" + updatedtotalprice.toFixed(2));

            }

            function applyblubuksvalidation() {
                var updatedprice = ($("#hdnproductactualprice").text() * theForm.Quantity_1_1.value) - $("#hdnProductFundAmountUsed").text();
                $("#spprice").text("$" + updatedprice.toFixed(2));
                var maxfundlimit = $("#spprice").text().replace("$", "") * (Number.parseFloat($("#hdnBudgetPercentValue").text()) / 100)
                if ($("#txtBluBuksUsed").val() == "" || isNaN($("#txtBluBuksUsed").val())) {
                    return false;
                }
                else if (Number.parseInt($("#txtBluBuksUsed").val()) > Number.parseInt(maxfundlimit)) {
                    alert("BLU BUKS cannot be greater than allowed limit");
                    // $("#txtBluBuksUsed").val(maxfundlimit.toFixed(2));
                    $("#txtBluBuksUsed").val("0.00");
                    return false;
                }
                else if (Number.parseInt($("#txtBluBuksUsed").val()) > Number.parseInt($("#hdnBluBucktsPoints").text())) {
                    alert("You exceed available BLU BUKS");
                    // $("#txtBluBuksUsed").val(maxfundlimit.toFixed(2));
                    $("#txtBluBuksUsed").val("0.00");

                    return false;
                }
                else if (Number.parseInt($("#txtBluBuksUsed").val()) > Number.parseInt($("#spprice").text().replace("$", ""))) {
                    alert("BLU BUKS cannot be greater than product price");
                    // $("#txtBluBuksUsed").val(maxfundlimit.toFixed(2));
                    $("#txtBluBuksUsed").val("0.00");
                    return false;
                }
                else
                    return true;

            }
            function applyproductcategoryfund() {
                $("#spprice").text("$" + Number.parseFloat($("#hdnpricewithfund").text()).toFixed(2));
                $("#sppricewithfund").html("<font>Price with FUND credit:</font> $" + Number.parseFloat($("#hdnpricewithfund").text()).toFixed(2));
                $("#hdnproductactualprice").text($("meta[itemprop=price]").attr("content").replace("$", "").replace(",", "").replace(" ", ""));

                    var customerlevel = $("#hdncustomerlevel").text();
                   if (customerlevel == 1 ||  customerlevel == 8 )
                    {
                         $("#sppricewithfund").addClass("hide-element");
                   
                    }
                    else
                    {
                         $("#sppricewithfund").removeClass("hide-element");
                    }
            }

           $("#Size_1_1").change(function () {
                 var customerlevel = $("#hdncustomerlevel").text();
                   if (customerlevel == 1 ||   customerlevel == 8 )
                    {
                         $("#sppricewithfund").addClass("hide-element");
                   
                    }
                    else
                    {
                         $("#sppricewithfund").removeClass("hide-element");
                    }
            });
            function ApplyValidation(theForm) {
                debugger;
                if ($("#Quantity_1_1").length <= 0 || $("#Size_1_1").length <= 0 || $("#Color_1_1").length <= 0) {
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
                if (theForm.Size_1_1.selectedIndex < 1) {
                    alert("Please select a size.");
                    theForm.Size_1_1.focus();
                    submitenabled(theForm);
                    return (false);
                }
                if (theForm.Color_1_1.selectedIndex < 1) {
                    alert("Please select a color.");
                    theForm.Color_1_1.focus();
                    submitenabled(theForm);
                    return (false);
                }
                if (theForm.Quantity_1_1.value > SelectedVariantInventory_1_1) {
                    alert("Your quantity exceeds stock on hand. The maximum quantity that can be added is " + SelectedVariantInventory_561_561 + ". Please contact us if you need more information.");
                    theForm.Quantity_1_1.value = SelectedVariantInventory_1_1;
                    theForm.Quantity_1_1.focus();
                    submitenabled(theForm);
                    return (false);
                }
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

