using System;

namespace LexViz.Controls.Header
{
    public partial class HeaderControl : AbstractUserControl<HeaderControl, HeaderViewModel>
    {
        public override string ControlPath
        {
            get { return "~/Controls/Header/HeaderControl.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void LoadData(HeaderViewModel viewModel)
        {
        }

        //Header Content
        //<span class="margin_Left_Medium margin_Right_Medium color_Text_Grey_5 weight_Light">|</span>
        //<asp:LinkButton runat="server" ID="btnUpload" Text="Upload Dummy Data" CssClass="color_Text_MainTheme_4"></asp:LinkButton>
    }
}