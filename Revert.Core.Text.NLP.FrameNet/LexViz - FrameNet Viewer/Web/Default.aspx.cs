using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LexViz
{
    public partial class Default1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            phBody.Controls.Add(LexViz.Controls.FrameNet.FrameNetUI.Instance.CreateControl(this));
        }
    }
}