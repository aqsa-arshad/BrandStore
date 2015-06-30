<%@ Control Language="C#" AutoEventWireup="true" CodeFile="QuickTopic.ascx.cs" Inherits="AspDotNetStorefront.QuickTopic" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%--<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>--%>
<style type="text/css">
    .style1 {
        width: 234px;
    }
</style>
<asp:Literal ID="ltValid" runat="server"></asp:Literal>
<div id="">
    <div style="margin-bottom: 5px; margin-top: 5px;">
        <asp:Literal ID="ltError" runat="server"></asp:Literal>
    </div>
</div>
<div class="wrapperLeft">
    <asp:PlaceHolder ID="phMain" runat="server">
        <%--    <div style="width: 100% " runat ="server" id="div1" class="titleMessage">
         <asp:Literal runat="server" ID="ltMode"></asp:Literal> Topic
    </div>
        <p>
            Please enter the following information about this topic. Fields marked with an asterisk (*) are required. All other fields are optional.
        </p> --%>
        <table width="100%" cellpadding="1" cellspacing="0" border="0">
            <tr>
                <td width="260" align="right" valign="top">
                    <font class="subTitleSmall">*Topic Name:</font>
                </td>
                <td align="left" valign="middle">
                    <asp:TextBox runat="server" ID="ltName" Enabled='<%# AddMode %>'></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="right" valign="top">
                    <font class="subTitleSmall">*Topic Title:</font>
                </td>
                <td align="left" valign="middle">
                    <asp:TextBox runat="server" ID="ltTitle"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="right" valign="middle">
                    <font class="subTitleSmall">Display Order:</font>
                </td>
                <td align="left" valign="top">
                    <asp:TextBox ID="txtDspOrdr" runat="server" CssClass="singleShortest"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="right" valign="top" height="380">
                    <font class="subTitleSmall">Description:</font>
                </td>
                <td align="left" valign="top">
                    <asp:Literal ID="ltDescription" runat="server"></asp:Literal>
                    <%--<telerik:RadEditor runat="server" id="radDescription">
                        <ImageManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                        <DocumentManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                        <FlashManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                        <MediaManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                        <SilverlightManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                        <TemplateManager UploadPaths="~/Images" ViewPaths="~/Images" DeletePaths="~/Images" />
                    </telerik:RadEditor>--%>
                </td>
            </tr>
            <tr>
                <td align="right" valign="top">
                    <font class="subTitleSmall">Search Engine Page Title:</font>
                </td>
                <td align="left" valign="top">
                    <asp:TextBox runat="server" ID="ltSETitle"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="right" valign="top">
                    <font class="subTitleSmall">Search Engine Keywords:</font>
                </td>
                <td align="left" valign="top">
                    <asp:TextBox runat="server" ID="ltSEKeywords"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="right" valign="top">
                    <font class="subTitleSmall">Search Engine Description:</font>
                </td>
                <td align="left" valign="top">
                    <asp:TextBox runat="server" ID="ltSEDescription"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="right" valign="middle">
                    <font class="subTitleSmall">Requires Disclaimer:</font>
                </td>
                <td align="left" valign="middle">
                    <asp:RadioButtonList ID="rbDisclaimer" runat="server" RepeatDirection="horizontal">
                        <asp:ListItem Value="0" Selected="true">No</asp:ListItem>
                        <asp:ListItem Value="1">Yes</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
            </tr>
            <tr>
                <td align="right" valign="middle">
                    <font class="subTitleSmall">Publish In Site Map:</font>
                </td>
                <td align="left" valign="middle">
                    <asp:RadioButtonList ID="rbPublish" runat="server" RepeatDirection="horizontal">
                        <asp:ListItem Value="0" Selected="true">No</asp:ListItem>
                        <asp:ListItem Value="1">Yes</asp:ListItem>
                    </asp:RadioButtonList>
                </td>
            </tr>
            <%--<tr>
                <td align="right" valign="middle">
                    <font class="subTitleSmall">Page BG Color:</font>
                </td>
                <td align="left" valign="middle">
                    <asp:TextBox ID="txtPageBG" runat="server" CssClass="singleShorter"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="right" valign="middle">
                    <font class="subTitleSmall">Contents BG Color:</font>
                </td>
                <td align="left" valign="middle">
                    <asp:TextBox ID="txtContentsBG" runat="server" CssClass="singleShorter"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td align="right" valign="middle">
                    <font class="subTitleSmall">Skin Graphics Color:</font>
                </td>
                <td align="left" valign="middle">
                    <asp:TextBox ID="txtSkinColor" runat="server" CssClass="singleShorter"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td valign="middle" align="right">
                    <asp:Panel ID="litStoreMapperHdr" runat="server">
                        <font class="subTitleSmall">
                        <asp:Literal ID="Literal1" runat="server" Text="Map to Stores:" />
                        </font>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel ID="litStoreMapper" runat="server" >
                        <aspdnsf:EntityToStore ID="etsMapper" runat="server" 
                            EntityType="Topic" 
                            Text=""
                            />
                    </asp:Panel>
                </td>
            </tr>--%>
        </table>
        <%--<div style="width: 100%; text-align: center;">
            &nbsp;&nbsp;<asp:Button ID="btnSubmit" runat="server" CssClass="normalButtons" OnClick="btnSubmit_Click" Text="Add Topic" />
        </div>--%>
    </asp:PlaceHolder>
</div>
