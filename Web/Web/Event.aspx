<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="Event.aspx.cs" Inherits="SBM.Web.Event" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

        <asp:GridView ID="GridEvent" runat="server" AutoGenerateColumns="false"  Width="1000px" 
                  CssClass="Grid" AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr" 
                    AllowPaging="true" PageSize="12" AllowCustomPaging="true" OnPageIndexChanging="GridEvent_PageIndexChanging">
        <Columns>
            <asp:TemplateField HeaderText="Event">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("sbm_event.description") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Description">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("description") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Time">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Eval("time_stamp") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <PagerSettings Mode="Numeric" />
    </asp:GridView>

</asp:Content>
