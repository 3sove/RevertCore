<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FrameNetUI.ascx.cs" Inherits="LexViz.Controls.FrameNet.FrameNetUI" %>

<asp:Label runat="server" ID="lblSearch" Text="Word:"></asp:Label>
<asp:TextBox runat="server" ID="txtSearch" CssClass="border_MainTheme color_Text_Important"></asp:TextBox>
<asp:Button runat="server" ID="btnSearch" Text="Search"/>

<asp:PlaceHolder runat="server" ID="phResults"></asp:PlaceHolder>