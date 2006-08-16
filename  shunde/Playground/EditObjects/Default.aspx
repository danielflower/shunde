<%@ Page Language="C#" MasterPageFile="~/Template.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="EditObjects_Default" Title="Create & Edit Objects" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" Runat="Server">






<table cellpadding="5">
	<tr>
		<td valign="top">

<h3><a href="default.aspx">Create and edit Objects</a></h3>

<asp:Label ID="infoMessage" runat="server" Font-Bold="True" ForeColor="red" />

			<b>Select the assembly you would like to select an Object from:</b><br/><br/>

			<ASP:RadioButtonList id="assembliesCB" runat="server" TabIndex="1" RepeatDirection="Vertical" RepeatColumns="4" CellPadding="3" AutoPostBack="true" />

		</td>
		<td valign="top" rowspan="2">

			<b>Select the object type you would like to create or edit:</b>
			<br/><br/>

			<ASP:ListBox id="listbox" runat="server" Rows="10" width="500px" SelectionMode="Single" TabIndex="1" />
			<br/>


			<ASP:DropDownList id="dbDropDown" runat="server" TabIndex="1" />

			<ASP:Button ID="Button1" runat="server" TabIndex="1" Text="List Instances" OnClick="getInstances" />
			&nbsp;&nbsp;
			<ASP:Button ID="Button2" runat="server" TabIndex="1" Text="Create New Instance" OnClick="createInstance" />

		</td>
	</tr>
	<tr>
		<td valign="bottom">
			<ASP:CheckBox id="getDeleted" runat="server" TabIndex="1" Text="Get Deleted Objects" Checked="false" />
			&nbsp;&nbsp;&nbsp;&nbsp;
			Max number to get: <ASP:TextBox id="numToGet" runat="server" TabIndex="1" Columns="7" Text="1000" />		
		</td>
	</tr>
</table>
&nbsp;<br/>

<b>Select the specific object you would like to edit:</b>
<br/><br/>

<ASP:ListBox id="objectsBox" runat="server" Rows="24" width="800px" SelectionMode="Single" TabIndex="1" />
<br/><br/>

<ASP:Button ID="Button3" runat="server" TabIndex="1" Text="Edit Selected Object" OnClick="editInstance" />







</asp:Content>

