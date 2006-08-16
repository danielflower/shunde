<%@ Page Language="C#" MasterPageFile="~/Template.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="SqlCreator_Default" Title="Shunde SQL Creator" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" Runat="Server">


<h3><a href="Default.aspx">Create Database Sql</a></h3>

<ASP:CheckBoxList id="assembliesCB" runat="server" TabIndex="1" RepeatDirection="Vertical" RepeatColumns="4" CellPadding="3" OnSelectedIndexChanged="clickAssembly" AutoPostBack="true" />

<br><br>

<b>Select the tables you would like to create:</b>
<br><br>

<ASP:ListBox id="listbox" runat="server" Rows="10" width="500px" SelectionMode="Multiple" TabIndex="1" />
<br><br>

<ASP:Checkbox runat="server" id="tablesAlreadyExist" TabIndex="1" Text="Tables already exist" Checked="false" />
&nbsp;&nbsp;

<ASP:Button ID="Button1" runat="server" TabIndex="1" Text="Create SQL" OnClick="createSql" />
&nbsp;&nbsp;


<input type="button" value="Copy to clipboard" OnClick="copySql(this.form);" TabIndex="1" />
<br><br>


<script language="javascript">

function copySql(f) {
	var txt = f.sqlBox.value;
	clipboardData.setData("Text", txt);
}

</script>
<textarea id="sqlBox" cols=100 rows=10><ASP:Literal id="sqlLiteral" runat="server" /></textarea>




</asp:Content>

