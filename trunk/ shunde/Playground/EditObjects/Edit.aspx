<%@ Page Language="C#" MasterPageFile="~/Template.master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="EditObjects_Edit" Title="Untitled Page" %>

<%@ Register Assembly="Shunde" Namespace="Shunde.WebControls" TagPrefix="Shunde" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" Runat="Server">

<h1 runat="server" id="headerTag" />
	<asp:ValidationSummary ID="ValidationSummary1" runat="server" />
<Shunde:ObjectEditor ID="ObjectEditor" runat="server" NumberColumnWidth="500px" DateTextBoxWidth="70px" FileUploaderWidth="1000%" NumberTextBoxWidth="70px" />

</asp:Content>

