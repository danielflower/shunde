<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SelectObject.aspx.cs" Inherits="EditObjects_SelectObject" Title="Select an object" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>

	<script language="JavaScript" type="text/javascript">

		function selectObject( objId, objName ) {
			opener.<ASP:Literal id="functionName" runat="server" />( objId, objName );
			opener.focus();
			window.close();
		}

	</script>
</head>
<body>

<div>



<ul>
<ASP:Literal id="output" runat="server" />
</ul>
&nbsp;
<p align="center"><input type="button" value="Close Window" onclick="window.close();" /></p>

<br/><br/>&nbsp;


</div>
    
</body>
</html>
