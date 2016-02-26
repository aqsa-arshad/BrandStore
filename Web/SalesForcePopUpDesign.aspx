<%@ Page Language="c#" Inherits="AspDotNetStorefront.SalesForcePopUpDesign" CodeFile="SalesForcePopUpDesign.aspx.cs" MasterPageFile="~/App_Templates/Skin_1/template.master" EnableViewStateMac="false" %>

<%@ Register Assembly="AspDotNetStorefrontControls" Namespace="AspDotNetStorefrontControls" TagPrefix="aspdnsf" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content runat="server" ContentPlaceHolderID="PageContent">
    <link href="App_Themes/Skin_3/app.css" rel="stylesheet" />

    <%-- Region Open Pop Up for bucckts--%>
    <div class="modal fade" id="salefForceModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
        <div class="modal-dialog" role="document" style="width:450px">
            <div class="modal-content">
                <div class="modal-body">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <img src="App_Themes/Skin_3/images/close-popup.png" alt="Close" /></button>
                    <h4 class="text-uppercase-no">APPLY FUNDS OR CREDIT</h4>
                    <p runat="server">Apply general funds , SOF funds or authorized capital expenditure to your item.
                        <a class="underline-link" href="#"><span>About sales funds</span></a></p>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-6">
                                <button class="btn btn-primary btn-block margin-none btn-fund" id="btnGeneralFunds" >General Funds</button>
                            </div>
                            <div class="col-md-6">
                                <button class="btn btn-primary btn-block margin-none btn-fund" id="btnSOFFunds" >Sales Operations </br> Funds</button>
                            </div>
                        </div>
                        <div class="clearfix"></div>
                        <div class="row">
                            <div class="col-md-6">
                                <button class="btn btn-primary btn-block btn-fund" id="btnCapitalExpenditure" >Capital Expenditure</button>
                            </div>
                            <div class="col-md-6">
                                <button class="btn btn-primary btn-block btn-fund" id="btnNoFund" >Apply no funds </br> to item</button>
                            </div>
                        </div>
                        <div class="clearfix"></div>
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
                        <span class="roman-black">Regular price: $0,000.00</span>
                    </p>

                    <div class="form-group hide-items" id="divGeneralFunds">
                        <div class="col-xs-12 padding-none">
                            <label class="roman-black">General Funds Used</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="txtBluBuksUsed" MaxLength="10" onpaste="return false" AutoCompleteType="Disabled" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                        </div>
                        <div class="clearfix"></div>
                    </div>
                    <div class="form-group hide-items" id="divSOFFunds">
                        <div class="col-xs-12 padding-none">
                            <label class="roman-black">SOF Funds Used</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="TextBox1" MaxLength="10" onpaste="return false" AutoCompleteType="Disabled" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                        </div>
                        <div class="clearfix"></div>
                        <div class="col-xs-12 padding-none">
                            <label class="roman-black">Department Code</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="TextBox3" MaxLength="10" onpaste="return false" AutoCompleteType="Disabled" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                        </div>
                        <div class="clearfix"></div>

                    </div>
                    <div class="form-group hide-items" id="divCapitalExpenditure">
                        <div class="col-xs-12 padding-none">
                            <label class="roman-black">Authentication code</label>
                        </div>
                        <div class="col-xs-6 padding-none">
                            <asp:TextBox ID="TextBox2" MaxLength="10" onpaste="return false" AutoCompleteType="Disabled" ClientIDMode="Static" placeholder="0.00" class="form-control" EnableViewState="false" runat="server"></asp:TextBox>
                        </div>
                        <div class="clearfix"></div>
                    </div>
                    <div class="buttons-group trueblue-popup">
                            <asp:Button ClientIDMode="Static" CssClass="btn btn-primary btn-block" Text="add to cart" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <%--End Region Open Pop Up for bucckts--%>

    <button type="button" class="btn btn-primary" data-toggle="modal" data-target="#salefForceModal">Click here</button>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#pGeneralFunds").removeClass("hide-items");
            $("#divGeneralFunds").removeClass("hide-items");

            $("#btnGeneralFunds").click(function (e) {
                $("#pGeneralFunds").removeClass("hide-items");
                $("#divGeneralFunds").removeClass("hide-items");

                $("#pSOFFunds").addClass("hide-items");
                $("#divSOFFunds").addClass("hide-items");

                $("#pCapitalExpenditure").addClass("hide-items");
                $("#divCapitalExpenditure").addClass("hide-items");

                e.preventDefault();
            });
            $("#btnSOFFunds").click(function (e) {
                $("#pSOFFunds").removeClass("hide-items");
                $("#divSOFFunds").removeClass("hide-items");

                $("#pGeneralFunds").addClass("hide-items");
                $("#divGeneralFunds").addClass("hide-items");

                $("#pCapitalExpenditure").addClass("hide-items");
                $("#divCapitalExpenditure").addClass("hide-items");
                e.preventDefault();
            });
            $("#btnCapitalExpenditure").click(function (e) {
                $("#pCapitalExpenditure").removeClass("hide-items");
                $("#divCapitalExpenditure").removeClass("hide-items");

                $("#pGeneralFunds").addClass("hide-items");
                $("#divGeneralFunds").addClass("hide-items");

                $("#pSOFFunds").addClass("hide-items");
                $("#divSOFFunds").addClass("hide-items");
                e.preventDefault();
            });
            $("#btnNoFund").click(function (e) {
                $("#pGeneralFunds").addClass("hide-items");
                $("#divGeneralFunds").addClass("hide-items");

                $("#pSOFFunds").addClass("hide-items");
                $("#divSOFFunds").addClass("hide-items");

                $("#pCapitalExpenditure").addClass("hide-items");
                $("#divCapitalExpenditure").addClass("hide-items");

                e.preventDefault();
            });
        });
    </script>


</asp:Content>
