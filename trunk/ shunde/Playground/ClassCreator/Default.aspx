<%@ Page Language="C#" MasterPageFile="~/Template.master" EnableEventValidation="False" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="ClassCreator_Default" Title="Shunde Class Creator" %>

<%@ Register Assembly="Shunde" Namespace="Shunde.Web" TagPrefix="Shunde" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" Runat="Server">


<h1>Create C# Class File</h1>

	<asp:Label ID="infoLabel" runat="server" Text="" ForeColor="Red" Style="display:block;"></asp:Label>
	<asp:ValidationSummary ID="ValidationSummary1" runat="server" />

Create a class with <asp:TextBox ID="numberToCreateTB" runat="server" TabIndex="1" Columns="4" Text="15" /> 
	<asp:RangeValidator ID="RangeValidator1" runat="server" ControlToValidate="numberToCreateTB"
		ErrorMessage="Please enter an integer here" MinimumValue="0" Type="Integer" MaximumValue="100">*</asp:RangeValidator>fields. 
		<asp:Button ID="changeNumberButton" runat="server" TabIndex="1" Text="Go" CausesValidation="False" />&nbsp;<br />
	<br />
	&nbsp;Namespace<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server"
		ControlToValidate="namespaceTB" ErrorMessage="Please enter a namespace">*</asp:RequiredFieldValidator>:
	<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="namespaceTB"
		Display="Dynamic" ErrorMessage="Use Pascal notation for namespaces" ValidationExpression="[A-Z][A-Za-z0-9\.]*">*</asp:RegularExpressionValidator>
	<asp:TextBox ID="namespaceTB" runat="server" TabIndex="1"></asp:TextBox>&nbsp; Class
	name:<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="classNameTB"
		ErrorMessage="Please enter a class name">*</asp:RequiredFieldValidator>
<asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="classNameTB"
		Display="Dynamic" ErrorMessage="Use Pascal notation for class names" ValidationExpression="[A-Z][A-Za-z0-9]*">*</asp:RegularExpressionValidator>
	<asp:TextBox ID="classNameTB" runat="server" TabIndex="1"></asp:TextBox><br />
	<br />

<asp:Repeater ID="textboxRepeater" runat="server">
	<HeaderTemplate>
		<table border="1" cellpadding="3">
			<tr>
				<th>Field name</th>
				<th>Type</th>
				<th>Allow nulls?</th>
				<th>Min Allowed</th>
				<th>Max Allowed</th>
				<th>Description</th>
			</tr>
	</HeaderTemplate>
	<ItemTemplate>
			<tr>
				<td><asp:TextBox ID="fieldNameTB" runat="server" TabIndex="1" /></td>
				<td><Shunde:ComboBox ID="typeTB" runat="server" TabIndex="1" /></td>
				<td><asp:CheckBox ID="allowNullsCB" runat="server" TabIndex="1" /></td>
				<td><asp:TextBox ID="minAllowedTB" runat="server" TabIndex="1" style="width:100px;" /></td>
				<td><asp:TextBox ID="maxAllowedTB" runat="server" TabIndex="1" style="width:100px;" /></td>
				<td><asp:TextBox ID="summaryTB" runat="server" TabIndex="1" style="width:100px;" /></td>
			</tr>
	</ItemTemplate>
	<FooterTemplate>
			<tr>
				<td colspan="6">
					<asp:Button ID="submitButton" runat="server" Text="Create Class" OnClick="submitButton_Click" />
					<input type="button" onclick="copyCodeToClipboard();" value="Copy to Clipboard" tabindex="1" />
				</td>
			</tr>
		</table>
	</FooterTemplate>
</asp:Repeater>


<asp:TextBox ID="outputTB" runat="server" TextMode="MultiLine" Width="100%" Height="300px" />


<script language="javascript" type="text/javascript">

function copyCodeToClipboard() {
	var txt = document.getElementById( '<asp:Literal id="textareaId" runat="server" />' ).value;
	clipboardData.setData("Text", txt);
}

function typeChanged( fnId, typeId, anId, minId, maxId ) {

	var fieldTB = document.getElementById( fnId );
	var typeTB = document.getElementById( typeId );
	var nullCB = document.getElementById( anId );
	var minTB = document.getElementById( minId );
	var maxTB = document.getElementById( maxId );

	var t = typeTB.value;
	
	fieldTB.disabled = false;
	typeTB.disabled = false;
	nullCB.disabled = false;
	minTB.disabled = false;
	maxTB.disabled = false;
	
	if (t == 'bool') {
		nullCB.checked = false;
		nullCB.disabled = true;
		minTB.value = '';
		maxTB.value = '';
		minTB.disabled = true;
		maxTB.disabled = true;
	} else if (t == 'string') {
		nullCB.checked = false;
		nullCB.disabled = true;
	} else if (t == 'multiline') {
		nullCB.checked = true;
		nullCB.disabled = true;
		minTB.value = '';
		maxTB.value = '';
		minTB.disabled = true;
		maxTB.disabled = true;
	}

}

</script>


</asp:Content>

