<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="Service.aspx.cs" Inherits="SBM.Web.Service"  EnableEventValidation="false" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

    <asp:ImageButton ID="agregar" runat="server" ImageUrl="~/img/add.png" ToolTip="Add New Service" OnClick="Add_Click" />

    <asp:GridView ID="GridService" runat="server" AutoGenerateColumns="false"  Width="900px" 
                  CssClass="Grid" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr">
        <Columns>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:ImageButton ID="editar" runat="server" ImageUrl="~/img/edit.png" Visible='<%# Eval("enabled") %>' ToolTip='<%# "Edit Id " + Eval("id_service") %>' CommandArgument='<%# Eval("id_service") %>' OnClick="Edit_Click" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Service">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("description") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Version">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("version") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Type">
                <ItemTemplate>
                    <asp:Label  runat="server" Text='<%# Eval("service_type") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="File">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("assembly_file") %>' Font-Strikeout='<%# Eval("disabled") %>' ToolTip='<%# Eval("files") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Timeout">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("max_time_run") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="CPU">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("cpu") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:ImageButton ID="borrar" runat="server" ImageUrl="~/img/del.png" Visible='<%# Eval("enabled") %>' ToolTip='<%# "Deactivate Id " + Eval("id_service") %>' CommandArgument='<%# Eval("id_service") %>' OnClick="Deactivate_Click" OnClientClick="return confirm('Deactivate confirmation');"  />
                    <asp:ImageButton ID="agregar" runat="server" ImageUrl="~/img/add.png" Visible='<%# Eval("disabled") %>' ToolTip='<%# "Activate Id " + Eval("id_service") %>' CommandArgument='<%# Eval("id_service") %>' OnClick="Activate_Click" OnClientClick="return confirm('Activate confirmation');" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>