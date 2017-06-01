<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SBM.Web.Default" %>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">
    
    <div class="table">
        
        <asp:Repeater ID="Repeater1" runat="server">

            <HeaderTemplate>
                <div class="th">
                    <div class="td">SERVICE</div>
                    <div class="td">TYPE</div>
                    <div class="td">ASSEMBLY</div>
                    <div class="td" title="SECS">TIMEOUT</div>
                    <div class="td">PUBLISHED</div>
                </div>
            </HeaderTemplate>
            
            <ItemTemplate>

                <div class="<%# "tr " + DataBinder.Eval(Container, "DataItem.ENABLED") %>">
                    <div class="td" title="<%# "ID# " + DataBinder.Eval(Container, "DataItem.ID_SERVICE") %>">
                        <%# DataBinder.Eval(Container, "DataItem.DESCRIPTION") %>
                    </div>
                    <div class="td" title="<%# "Version " + DataBinder.Eval(Container, "DataItem.VERSION") %>">
                        <%# DataBinder.Eval(Container, "DataItem.SBM_SERVICE_TYPE.DESCRIPTION")  %>
                    </div>
                    <div class="td" title="<%# @".\Repository\" + DataBinder.Eval(Container, "DataItem.ASSEMBLY_PATH") + @"\"  + DataBinder.Eval(Container, "DataItem.ASSEMBLY_FILE")  %>">
                        <%# DataBinder.Eval(Container, "DataItem.ASSEMBLY_FILE")  %>
                    </div>
                    <div class="td" title="seconds">
                        <%#  DataBinder.Eval(Container, "DataItem.MAX_TIME_RUN")  %>
                    </div>
                    <div class="td" title="<%# DataBinder.Eval(Container, "DataItem.PUBLISHED", "{0:HH:mm:ss z}") %>">
                        <%#  DataBinder.Eval(Container, "DataItem.PUBLISHED", "{0:dd-MM-yyyy}")  %>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

    </div>

</asp:Content>
