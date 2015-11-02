<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="controls_Search" %>

<asp:Panel runat="server" DefaultButton="SearchButton">

    <div class="form-group">
        <asp:TextBox ID="SearchBox" runat="server"
            CssClass="search-feild" MaxLength="80" />

        <asp:Button runat="server"
            ID="SearchButton"
            OnClick="SearchButton_Click"
            OnClientClick="javascript: searchTrim();"
            UseSubmitBehavior="false"
            Style="display: none" />
    </div>

    <script type="text/javascript">
        function searchTrim() {
            var controlId = GetClientID("SearchBox").attr("id");
            var value = document.getElementById(controlId).value;
            var rex = /(<([^>]+)>)/ig;

            if (rex.test(value) == true) {
                alert('Please provide legal input for search.');
                document.getElementById(controlId).value = "";
                e.preventDefault();

            }
        }
        function GetClientID(id, context) {
            var el = $("#" + id, context);
            if (el.length < 1)
                el = $("[id$=_" + id + "]", context);
            return el;
        }
    </script>

</asp:Panel>
