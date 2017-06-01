<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="ServiceEdit.aspx.cs" Inherits="SBM.Web.ServiceEdit" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

    <div style="width: 900px;">
        <asp:HiddenField runat="server" ID="service_id" />
        <fieldset>
            <legend>Service</legend>

            <asp:Label runat="server" AssociatedControlID="description" Text="Description : " />
            <asp:TextBox runat="server" ID="description" MaxLength="100" TextMode="SingleLine" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="description" Text="&#10005;" />
            </span>
            <br />
            <asp:Label runat="server" AssociatedControlID="service_type" Text="Type : " />
            <asp:DropDownList runat="server" ID="service_type" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="service_type" Text="&#10005;" />
            </span>
            <br />
            <asp:Label runat="server" AssociatedControlID="version" Text="Version : " />
            <asp:TextBox runat="server" ID="version" MaxLength="5" TextMode="SingleLine" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="version" Text="&#10005;" />
            </span>
            <br />
            <asp:Label runat="server" AssociatedControlID="assembly_file" Text="File : " />
            <asp:TextBox runat="server" ID="assembly_file" MaxLength="60" TextMode="SingleLine" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="assembly_file" Text="&#10005;" />
            </span>
            <br />
            <asp:FileUpload runat="server" ID="upload_file" AllowMultiple="true" Width="95%" />
            <br />
            <br />
            <asp:Label runat="server" AssociatedControlID="assembly_path" Text="Server Relative Path: " />
            <asp:TextBox runat="server" ID="assembly_path" MaxLength="250" TextMode="SingleLine" />
            <span class="invalid">
                <asp:RequiredFieldValidator runat="server" ControlToValidate="assembly_path" Text="&#10005;" />
            </span>
            <br />
            <asp:Label runat="server" AssociatedControlID="service_parent" Text="Parent : " />
            <asp:DropDownList runat="server" ID="service_parent" />
        </fieldset>

        <fieldset>
            <legend>Environment</legend>

            <asp:RadioButton ID="x32" runat="server" Text="32 bit" GroupName="CPU" />
            <asp:RadioButton ID="x64" runat="server" Text="64 bit" GroupName="CPU" />
            <br />
            <asp:Label runat="server" AssociatedControlID="max_time_run" Text="Max Time Run : " />
            <asp:TextBox runat="server" ID="max_time_run" TextMode="Number" />
            <span class="invalid">
                <%-- 
                <asp:RangeValidator runat="server" ControlToValidate="max_time_run" MinimumValue="0" MaximumValue="32767" Text="&#10005;" />
                 --%>
            </span>
            <br />
            <asp:CheckBox runat="server" ID="single_exec" Text="Single Exec" />
            <br />
            <asp:Label runat="server" AssociatedControlID="domain" Text="Domain : " />
            <asp:TextBox runat="server" ID="domain" MaxLength="80" TextMode="SingleLine" />
            <br />
            <asp:Label runat="server" AssociatedControlID="user" Text="User : " />
            <asp:TextBox runat="server" ID="user" MaxLength="80" TextMode="SingleLine" />
            <br />
            <asp:Label runat="server" AssociatedControlID="password" Text="Password : " />
            <asp:TextBox runat="server" ID="password" MaxLength="80" TextMode="Password" />
        </fieldset>
    </div>

    <asp:ImageButton ID="guardar" runat="server" ImageUrl="~/img/save.png" ToolTip="Save Service" OnClick="Save_Click" OnClientClick="if (Page_ClientValidate()) return confirm('Save confirmation'); else return false;" />
    <asp:ImageButton ID="volver" runat="server" ImageUrl="~/img/back.png" ToolTip="Save Service" OnClick="Back_Click" OnClientClick="return confirm('Cancel confirmation');" />

    <asp:ValidationSummary runat="server" ShowSummary="false" />

</asp:Content>
