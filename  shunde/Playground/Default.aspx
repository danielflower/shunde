<%@ Page Language="C#" MasterPageFile="~/Template.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" Title="Untitled Page" %>

<%@ Register Assembly="Shunde" Namespace="Shunde.Web" TagPrefix="Shunde" %>
<asp:Content ID="Content1" ContentPlaceHolderID="PageBody" Runat="Server">

<h1>Shunde Playground</h1>


<a href="EditObjects/Default.aspx">Edit Objects</a>

<br /><br />

<a href="SqlCreator/Default.aspx">Create Database Sql</a>

<br /><br />

<a href="ClassCreator/Default.aspx">[Create Classes]</a> |

<Shunde:ColorPicker ID="colorPicker" runat="server" SelectedColor="255, 192, 128" OnClientSelectedColorChanged="alert('You selected ' + color);" />


</asp:Content>

