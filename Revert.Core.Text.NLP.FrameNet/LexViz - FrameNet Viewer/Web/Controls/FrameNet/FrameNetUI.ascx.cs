using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LexViz.Controls.FrameNet
{
    public partial class FrameNetUI : AbstractUserControl<FrameNetUI>
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.btnSearch.Click += btnSearch_Click;
        }

        void btnSearch_Click(object sender, EventArgs e)
        {
            
        }

        public override string ControlPath
        {
            get { return "~/Controls/FrameNet/FrameNetUI.ascx"; }
        }
    }
}