<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="TimerEdit.aspx.cs" Inherits="SBM.Web.TimerEdit" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

    <div style="width: 900px;">

        <fieldset>
            <legend>Service</legend>
            <asp:HiddenField ID="id_service_timer" runat="server" />
            <asp:Label runat="server" AssociatedControlID="service" Text="Service : " />
            <asp:DropDownList runat="server" ID="service" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="service" Text="&#10005;" />
            </span>
            <br />
            <asp:Label runat="server" AssociatedControlID="owner" Text="Owner : " />
            <asp:DropDownList runat="server" ID="owner" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="owner" Text="&#10005;" />
            </span>
            <br />
            <asp:Label runat="server" AssociatedControlID="id_private" Text="Private : " />
            <asp:TextBox runat="server" ID="id_private" MaxLength="40" TextMode="SingleLine" />
            <br />
            <asp:Label runat="server" AssociatedControlID="parameters" Text="Parameter : " />
            <asp:TextBox runat="server" ID="parameters" MaxLength="4000" TextMode="SingleLine" />
        </fieldset>

        <fieldset>
            <legend>Timer</legend>

            <asp:Label runat="server" AssociatedControlID="run_interval" Text="Run Interval: " />
            <asp:TextBox runat="server" ID="run_interval" TextMode="Number" />
            <span class="invalid">
                <asp:RangeValidator runat="server" ControlToValidate="run_interval" MinimumValue="0" MaximumValue="32767" Text="&#10005;" />
            </span>
            <br />
            <asp:Label runat="server" AssociatedControlID="crontab" Text="Crontab : " />
            <asp:TextBox runat="server" ID="crontab" MaxLength="50" TextMode="SingleLine" />
            <br />
            minute(0-59) hour(0-23) day month(1-31) month(1-12) day week(0-6)<br />
        </fieldset>
    </div>

    <asp:ImageButton ID="guardar" runat="server" ImageUrl="~/img/save.png" ToolTip="Save Service" OnClick="Save_Click" OnClientClick="if (Page_ClientValidate()) return confirm('Save confirmation'); else return false;" />
    <asp:ImageButton ID="volver" runat="server" ImageUrl="~/img/back.png" ToolTip="Save Service" OnClick="Back_Click" OnClientClick="return confirm('Cancel confirmation');" />

    <asp:ValidationSummary runat="server" ShowSummary="false" />

</asp:Content>
