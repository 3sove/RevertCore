<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HeaderControl.ascx.cs" Inherits="LexViz.Controls.Header.HeaderControl" %>

<div class="color_Background_UNCLASSIFIED weight_Bold size_Text_Medium color_Text_White size_Width_Full behavior_Centered">
    UNCLASSIFIED
</div>

<div style="width: 100%; height: 45px; line-height: 45px; vertical-align: middle;" class="color_Text_MainTheme_5 color_Background_Grey_1 size_Text_Large2">
    <span style="font-weight:normal;">
        <asp:HyperLink ID="HyperLink1" runat="server" CssClass="size_Text_Large3 color_Text_White margin_Left_Small" Text="Lexical Comprehension" NavigateUrl="~/Default.aspx"></asp:HyperLink>
        <asp:HyperLink ID="HyperLink2" runat="server" CssClass="size_Text_Medium color_Text_Grey_7" Text="Visualizer" NavigateUrl="~/Default.aspx"></asp:HyperLink>
    </span>
    <a runat="server" href="~/Default.aspx" class="margin_Left_VeryLarge color_Text_MainTheme_4">Home</a>
</div>
