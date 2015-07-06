<%@ Page language="c#" AutoEventWireup="true" Inherits="AspDotNetStorefrontAdmin.storewide" CodeFile="storewide.aspx.cs" MaintainScrollPositionOnPostback="true"
MasterPageFile="~/App_Templates/Admin_Default/AdminMaster.master" %>

<asp:Content runat="server" ContentPlaceHolderID="bodyContentPlaceholder">
    <asp:Literal ID="ltScript" runat="server"></asp:Literal> 
    <asp:Literal ID="ltValid" runat="server"></asp:Literal>
    <div id="">
        <div style="margin-bottom: 5px; margin-top: 5px;">
            <asp:Literal ID="ltError" runat="server"></asp:Literal>
        </div>
    </div>
    <div id="">
        <table border="0" cellpadding="1" cellspacing="0" class="outerTable" width="100%">
            <tr>
                <td>
                    <div class="wrapper">                       
                        <table border="0" cellpadding="0" cellspacing="0" class="innerTable" width="100%">
                            <tr>
                                <td class="titleTable">
                                    <font class="subTitle">Maintenance:</font>
                                </td>
                            </tr>
                            <tr>
                                <td class="contentTable" valign="top" width="100%">
                                    <div class="wrapper">
                                        This page helps you perform recommended store-wide maintenance.
                                        <br /><br />
                                        <div id="divMain" runat="server">
                                            <table cellpadding="5" cellspacing="0" border="0" width="100%">
                                                <tr>
                                                    <td align="right" valign="top" style="border-bottom: dashed 1px #666666;">
                                                        <font class="subTitleSmall">
                                                            Set 'On Sale' Prompt:
                                                        </font>
                                                    </td>
                                                    <td align="left" style="border-bottom: dashed 1px #666666;">
                                                    <asp:Panel runat="server" ID="pnlOnSalePrompt" DefaultButton="btnSubmit1">
                                                        <asp:DropDownList ID="ddOnSale" runat="server" CssClass="default"></asp:DropDownList>
                                                        <asp:RequiredFieldValidator InitialValue="0" ValidationGroup="group1" ErrorMessage="!!" ControlToValidate="ddOnSale" Display="dynamic" SetFocusOnError="true" runat="server" ID="RequiredFieldValidator"></asp:RequiredFieldValidator>
                                                        <br />
                                                        For Category: <asp:DropDownList ID="ddOnSaleCat" runat="server" CssClass="default"></asp:DropDownList>
                                                        <br />
                                                        For Department: <asp:DropDownList ID="ddOnSaleDep" runat="server" CssClass="default"></asp:DropDownList>
                                                        <br />
                                                        For Manufacturer: <asp:DropDownList ID="ddOnSaleManu" runat="server" CssClass="default"></asp:DropDownList>
                                                        <br />
                                                        <asp:Button ID="btnSubmit1" ValidationGroup="group1" CssClass="normalButtons" runat="server" Text="Submit" OnClick="btnSubmit1_Click" />
                                                    </asp:Panel>
                                                    </td>
                                                </tr>                                                   
                                                <tr>
                                                    <td align="right" valign="top" style="border-bottom: dashed 1px #666666;">
                                                        <font class="subTitleSmall">
                                                            Set Spec Title For All Products:
                                                        </font>
                                                    </td>
                                                    <td align="left" style="border-bottom: dashed 1px #666666;">
                                                    <asp:Panel runat="server" ID="pnlSpecTitle" DefaultButton="btnSubmit2">
                                                        <asp:TextBox ID="txtSpec" runat="server" CssClass="singleNormal"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ValidationGroup="group2" ErrorMessage="!!" ControlToValidate="txtSpec" Display="dynamic" SetFocusOnError="true" runat="server" ID="RequiredFieldValidator1"></asp:RequiredFieldValidator>
                                                        <br />
                                                        <asp:Button ID="btnSubmit2" ValidationGroup="group2" CssClass="normalButtons" runat="server" Text="Submit" OnClick="btnSubmit2_Click" />
                                                    </asp:Panel>
                                                    </td>
                                                </tr>                                                   
                                                <tr>
                                                    <td align="right" valign="top" style="border-bottom: dashed 1px #666666;">
                                                        <font class="subTitleSmall">
                                                            Set Specs Inline Flag For All Products:
                                                        </font>
                                                    </td>
                                                    <td align="left" style="border-bottom: dashed 1px #666666;">
                                                    <asp:Panel runat="server" ID="pnlSpecsInline" DefaultButton="btnSubmit3"> 
                                                        <asp:RadioButtonList ID="rblSpecsInline" runat="server">
                                                            <asp:ListItem Value="0">No</asp:ListItem>
                                                            <asp:ListItem Value="1">Yes</asp:ListItem>
                                                        </asp:RadioButtonList>
                                                        <br />
                                                        <asp:Button ID="btnSubmit3" ValidationGroup="group3" CssClass="normalButtons" runat="server" Text="Submit" OnClick="btnSubmit3_Click" />
                                                    </asp:Panel>
                                                    </td>
                                                </tr>                                                   
                                                <tr>
                                                    <td align="right" valign="top" style="border-bottom: dashed 1px #666666;">
                                                        <font class="subTitleSmall">
                                                            Set Quantity Discount Table to be<br />used for ALL Products & Variants:
                                                        </font>
                                                    </td>
                                                    <td align="left" style="border-bottom: dashed 1px #666666;">
                                                    <asp:Panel runat="server" ID="pnlSetQtyDiscount" DefaultButton="btnSubmit4"> 
                                                        <asp:DropDownList ID="ddDiscountTable" runat="server"></asp:DropDownList>
                                                        <br />
                                                        <asp:Button ID="btnSubmit4" ValidationGroup="group4" CssClass="normalButtons" runat="server" Text="Submit" OnClick="btnSubmit4_Click" />
                                                    </asp:Panel>
                                                    </td>
                                                </tr>                                                     
                                                <tr>
                                                    <td align="right" valign="top" style="border-bottom: dashed 1px #666666;">
                                                        <font class="subTitleSmall">
                                                            Set Sales Discount Percentage:
                                                        </font>
                                                    </td>
                                                    <td align="left" style="border-bottom: dashed 1px #666666;">
                                                    <asp:Panel runat="server" ID="pnlSalesDiscount" DefaultButton="btnSubmit5">
                                                        <asp:TextBox CssClass="singleShort" runat="server" ID="txtDiscountPercent"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ValidationGroup="group5" ErrorMessage="!!" ControlToValidate="txtDiscountPercent" Display="dynamic" SetFocusOnError="true" runat="server" ID="RequiredFieldValidator2"></asp:RequiredFieldValidator>
                                                        <br />
                                                        For Category: <asp:DropDownList ID="ddDiscountCate" runat="server" CssClass="default"></asp:DropDownList>
                                                        <br />
                                                        For Department: <asp:DropDownList ID="ddDiscountDep" runat="server" CssClass="default"></asp:DropDownList>
                                                        <br />
                                                        For Manufacturer: <asp:DropDownList ID="ddDiscountManu" runat="server" CssClass="default"></asp:DropDownList>
                                                        <br />
                                                        <asp:Button ID="btnSubmit5" ValidationGroup="group5" CssClass="normalButtons" runat="server" Text="Submit" OnClick="btnSubmit5_Click" />
                                                    </asp:Panel>
                                                    </td>
                                                </tr>                                                                                                    
                                                <tr>
                                                    <td align="right" valign="top" style="border-bottom: dashed 1px #666666;">
                                                        <font class="subTitleSmall">
                                                            Reset All Default Variants:
                                                        </font>
                                                    </td>
                                                    <td align="left" style="border-bottom: dashed 1px #666666;">
                                                    <asp:Panel runat="server" ID="pnlResetVariants" DefaultButton="btnSubmit6">
                                                        This cannot be undone!
                                                        <br />
                                                        Resets the default variant for each product to the first one by DisplayOrder,Name
                                                        <br />
                                                        <asp:Button ID="btnSubmit6" ValidationGroup="group6" CssClass="normalButtons" runat="server" Text="Submit" OnClick="btnSubmit6_Click" />
                                                    </asp:Panel>
                                                    </td>
                                                </tr>                                                
                                                <tr>
                                                    <td align="right" valign="top">
                                                        <font class="subTitleSmall">
                                                            Reset All product SENames:
                                                        </font>
                                                    </td>
                                                    <td align="left" >
                                                    <asp:Panel runat="server" ID="pnlResetSENames" DefaultButton="btnSubmit7">
                                                        This cannot be undone!
                                                        <br />
                                                        Sets the SEName field in the product table for ALL products, this may take a long time if you have a lot of products.
                                                        <br />
                                                        <asp:Button ID="btnSubmit7" ValidationGroup="group7" CssClass="normalButtons" runat="server" Text="Submit" OnClick="btnSubmit7_Click" />
                                                    </asp:Panel>
                                                    </td>
                                                </tr>                                              
                                            </table>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>                    
                </td>
            </tr>
        </table>
    </div>
</asp:Content>