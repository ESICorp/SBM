<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="Done.aspx.cs" Inherits="SBM.Web.Done" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

        <asp:GridView ID="GridDone" runat="server" AutoGenerateColumns="false"  Width="1200px" 
                  CssClass="Grid" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr" 
                    AllowPaging="true" PageSize="12" AllowCustomPaging="true" OnPageIndexChanging="GridDone_PageIndexChanging">
        <Columns>
            <asp:TemplateField HeaderText="Service">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("sbm_service.description") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Parameter">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("parameters") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Result">
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("result") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("sbm_done_status.description") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Started">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("started") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Ended">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("ended") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Requested">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("requested") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Owner">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("sbm_owner.description") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerSettings Mode="Numeric" />
    </asp:GridView>

</asp:Content>
