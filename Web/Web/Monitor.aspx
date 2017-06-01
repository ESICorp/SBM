<%@ Page Title="" Language="C#" MasterPageFile="~/Default.Master" AutoEventWireup="true" CodeBehind="Monitor.aspx.cs" Inherits="SBM.Web.Monitor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <% #if DEBUG %>
    <script src="Scripts/jquery-3.1.0.js" type="text/javascript"></script>
    <script src="Scripts/jquery.signalR-2.2.1.js"  type="text/javascript"></script>
    <% #else %>
    <script src="Scripts/jquery-3.1.0.min.js" type="text/javascript"></script>
    <script src="Scripts/jquery.signalR-2.2.1.min.js"  type="text/javascript"></script>
    <% #endif %>
    <script src="signalr/hubs"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">

     <script>
         $(function () {
             // Declare a proxy to reference the hub. 
             var proxy = $.connection.debugHub;
             var max = 80;

             // Create a function that the hub can call to broadcast messages.
             proxy.client.broadcast = function (time, pid, text) {

                 var filter = $('#filter');
                 try {
                     var m = text.match(filter.val());
                     filter.css('background-color','');
                     if (!m) return;
                 } catch (e) {
                     filter.css('background-color', 'red');
                     return;
                 }

                 var log = $('#log');

                 // Add the message to the page. 
                 log.append('<li>' +
                     $('<div />').text(time).html() + '&nbsp;<strong>[' +
                     $('<div />').text(pid).html() + ']</strong>&nbsp;' + 
                     $('<div />').text(text).html() + '</li>');

                 if (log.children('li').length > max) log.children('li').first().remove();
             };

             $.connection.hub.start();
         });
    </script>

    Filter <input id="filter" type="text" name="filter" value="" />
    <div id="log" style="font-size: small; margin-left : 10px; min-width:900px;"></div>
</asp:Content>
