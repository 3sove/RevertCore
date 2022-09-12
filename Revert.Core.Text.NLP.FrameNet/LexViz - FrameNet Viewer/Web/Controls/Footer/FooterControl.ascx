<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FooterControl.ascx.cs" Inherits="LexViz.Controls.Footer.FooterControl" %>

<div style="position: absolute; left: 0; bottom: 0; text-align: center; width:100%;" class="color_Text_MainTheme_5 color_Background_Grey_1 size_Text_Large1">
    <div style="margin-left:auto; margin-right:auto; width:100%; height:25px; line-height:25px; vertical-align:middle;">
        <asp:HyperLink runat="server" ID="hlHome" NavigateUrl="~/Default.aspx" Text="Home"></asp:HyperLink>
        <span class="margin_Left_Large margin_Right_Large color_Text_MainThemeMediumLight">|</span>
        <a href="mailto:adam.carstensen.ctr@rdnet.org">Contact</a>
    </div>
</div>
