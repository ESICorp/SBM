<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="Timer.aspx.cs" Inherits="SBM.Web.Scheduler" EnableEventValidation="false" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

    <asp:ImageButton ID="agregar" runat="server" ImageUrl="~/img/add.png" ToolTip="Add New Timer" OnClick="Add_Click" />

    <asp:GridView ID="GridTimer" runat="server" AutoGenerateColumns="false"  Width="900px" 
                  CssClass="Grid" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr">
        <Columns>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:ImageButton ID="editar" runat="server" ImageUrl="~/img/edit.png" Visible='<%# Eval("enabled") %>' ToolTip='<%# "Edit Id " + Eval("id_service_timer") %>' CommandArgument='<%# Eval("id_service_timer") %>' OnClick="Edit_Click" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Service">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("sbm_service.description") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Owner">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("sbm_owner.description") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Parameter">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("parameters") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Last">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("last_time_run") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Next">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("next_time_run") %>' Font-Strikeout='<%# Eval("disabled") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Interval" >
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("run_interval") %>' Font-Strikeout='<%# Eval("disabled") %>' Visible='<%# string.IsNullOrEmpty(Eval("crontab") as string) %>' />
                    <asp:Label runat="server" Text='<%# Eval("crontab") %>' Font-Strikeout='<%# Eval("disabled") %>' Visible='<%# !string.IsNullOrEmpty(Eval("crontab") as string) %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:ImageButton ID="borrar" runat="server" ImageUrl="~/img/del.png" Visible='<%# Eval("enabled") %>' ToolTip='<%# "Deactivate Id " + Eval("id_service") %>' CommandArgument='<%# Eval("id_service_timer")  %>' OnClick="Deactivate_Click" OnClientClick="return confirm('Deactivate confirmation');"  />
                    <asp:ImageButton ID="agregar" runat="server" ImageUrl="~/img/add.png" Visible='<%# Eval("disabled") %>' ToolTip='<%# "Activate Id " + Eval("id_service") %>' CommandArgument='<%# Eval("id_service_timer") %>' OnClick="Activate_Click" OnClientClick="return confirm('Activate confirmation');" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
