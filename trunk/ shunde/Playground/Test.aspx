<%@ Page Language="C#" MasterPageFile="~/Template.master" AutoEventWireup="true" CodeFile="Test.aspx.cs" Inherits="Test" Title="Untitled Page" %>
<%@ Register Assembly="Shunde" Namespace="Shunde.Web" TagPrefix="Shunde" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" Runat="Server">

	<asp:ValidationSummary ID="ValidationSummary1" runat="server" />

<div style="position:absolute;left:400px;top:200px;">
one: <Shunde:ComboBox ID="comboBox1" runat="server" />
	<Shunde:ColorPicker ID="colorPicker1" runat="server" Width="200px" Height="50px" />

</div>

two: <Shunde:ComboBox ID="comboBox" runat="server" />
	<Shunde:ColorPicker ID="colorPicker" runat="server" Width="200px" Height="50px" SelectedColor="Red" />

	<asp:RequiredFieldValidator ControlToValidate="colorPicker" runat="server" ErrorMessage="Pick a value for the colour" Display="Dynamic">*</asp:RequiredFieldValidator>


<asp:Button ID="goButton" Text="Go" runat="server" />

<p>
Selected text: <asp:Literal ID="selectedText" runat="server" /><br />
Selected value: <asp:Literal ID="selectedValue" runat="server" /><br />
Selected index: <asp:Literal ID="selectedIndex" runat="server" /><br />
Selected item: <asp:Literal ID="selectedItem" runat="server" /><br />
</p>

<p>Changed to: 
<asp:Literal ID="colorChangeLit" runat="server" EnableViewState="false" />
</p>

<p>Date picker:
<Shunde:DateTimePicker ID="datePicker" runat="server" />
</p>

</asp:Content>

