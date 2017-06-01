<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="Owner.aspx.cs" Inherits="SBM.Web.Owner" EnableEventValidation="false" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

    <asp:ImageButton ID="agregar" runat="server" ImageUrl="~/img/add.png" ToolTip="Add New Owner" OnClick="Add_Click" />

    <asp:GridView ID="GridOwner" runat="server" AutoGenerateColumns="false"  Width="600px" 
                  CssClass="Grid" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr">
        <Columns>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:ImageButton ID="editar" runat="server" ImageUrl="~/img/edit.png" Visible='<%# Eval("enabled") %>' ToolTip='<%# "Edit Id " + Eval("id_owner") %>' CommandArgument='<%# Eval("id_owner") %>' OnClick="Edit_Click" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Owner">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("description") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Token">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("token") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:ImageButton ID="borrar" runat="server" ImageUrl="~/img/del.png" Visible='<%# Eval("enabled") %>' ToolTip='<%# "Deactivate Id " + Eval("id_owner") %>' CommandArgument='<%# Eval("id_owner") %>' OnClick="Deactivate_Click" OnClientClick="return confirm('Deactivate confirmation');"  />
                    <asp:ImageButton ID="agregar" runat="server" ImageUrl="~/img/add.png" Visible='<%# Eval("disabled") %>' ToolTip='<%# "Activate Id " + Eval("id_owner") %>' CommandArgument='<%# Eval("id_owner") %>' OnClick="Activate_Click" OnClientClick="return confirm('Activate confirmation');" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
