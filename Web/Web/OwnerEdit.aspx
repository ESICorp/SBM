<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="OwnerEdit.aspx.cs" Inherits="SBM.Web.OwnerEdit" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

        <div style="width: 900px;">
        <asp:HiddenField runat="server" ID="owner_id" />
        <fieldset>
            <legend>Owner</legend>

            <asp:Label runat="server" AssociatedControlID="description" Text="Description : " />
            <asp:TextBox runat="server" ID="description" MaxLength="100" TextMode="SingleLine" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="description" Text="&#10005;" />
            </span>
            <br />
            <asp:Label runat="server" AssociatedControlID="token" Text="Token : " />
            <asp:TextBox runat="server" ID="token" MaxLength="15" TextMode="SingleLine" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="token" Text="&#10005;" />
            </span>
        </fieldset>

    </div>

    <asp:ImageButton ID="guardar" runat="server" ImageUrl="~/img/save.png" ToolTip="Save Service" OnClick="Save_Click" OnClientClick="if (Page_ClientValidate()) return confirm('Save confirmation'); else return false;" />
    <asp:ImageButton ID="volver" runat="server" ImageUrl="~/img/back.png" ToolTip="Save Service" OnClick="Back_Click" OnClientClick="return confirm('Cancel confirmation');" />

    <asp:ValidationSummary runat="server" ShowSummary="false" />

</asp:Content>
