<%@ Page Language="c#" Inherits="AspDotNetStorefront.showproduct" CodeFile="showproduct.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" EnableViewStateMac="false" %>

<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />
    <asp:Panel runat="server">
        <asp:Literal ID="litOutput" runat="server"></asp:Literal>
    </asp:Panel>
    <%-- Region Open Pop Up for bucckts--%>
    <div class="modal fade" id="myModa2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
        <div class="modal-dialog modal-checkout" role="document">
            <div class="modal-content">
                <div class="modal-body">
                    <h5 class="text-uppercase-no">TrueBLU</h5>
                    <p runat="server" id="ppointscount">You have XXXXXX BLU Bucks you can use to purchase your items.</p>
                    <p>Decide hom many BLU Bucks you want to use to purchase this item.</p>

                    <div class="form-group">
                        <div class="col-xs-6 padding-none">
                            <label class="roman-black">BLU Bucks used:</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="txtBluBuksUsed" class="form-control" runat="server" Text="0"></asp:TextBox>

                        </div>
                        <div class="clearfix"></div>
                    </div>

                    <p class="label-text">
                        <span class="roman-black">Total price using BLU Bucks:</span>
                        $X,XXX.XX
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
            $("#btnaddtocart").click(function (e) {
                if (ApplyValidation(theForm)) {                   
                    $("#btnaddtocart").attr("data-toggle", "modal");
                    $("#btnaddtocart").attr("data-target", "#myModa2");
                }

            });

            function ApplyValidation(theForm) {
                debugger;

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

