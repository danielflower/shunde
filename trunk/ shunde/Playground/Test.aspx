<%@ Page Language="C#" MasterPageFile="~/Template.master" AutoEventWireup="true" CodeFile="Test.aspx.cs" Inherits="Test" Title="Untitled Page" %>
<%@ Register Assembly="Shunde" Namespace="Shunde.Web" TagPrefix="Shunde" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" Runat="Server">



<div style="position:absolute;left:200px;top:200px;">
one: <Shunde:ComboBox ID="comboBox1" runat="server" />
</div>

two: <Shunde:ComboBox ID="comboBox" runat="server" />

<asp:Button ID="goButton" Text="Go" runat="server" />

</asp:Content>

