using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LexViz.Controls.Footer;
using LexViz.Controls.Header;

namespace LexViz
{
    public partial class Default : System.Web.UI.MasterPage
    {
        private const string NewUserRelativePath = "~/Registration/NewUser.aspx";

        public const string PageTitle = "Exploitation Dashboard 0.0.1";

        private string appPoolName = string.Empty;
        public string AppPoolName
        {
            get
            {
                if (appPoolName == string.Empty) appPoolName = System.Configuration.ConfigurationManager.AppSettings["App_Pool_Name"];
                return appPoolName;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.Title = PageTitle;

            //Web.Cache.UserCache.GetCurrentUserCache().OnPageLoad();
            //if (Page.AppRelativeVirtualPath != NewUserRelativePath && Environment.UserName != AppPoolName)
            //{
            //    userID = Modules.Users.UserModule.Instance.Authenticate();
            //    if (userID == Guid.Empty) Response.Redirect(NewUserRelativePath, true);
            //}
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            PageHeader.Controls.Add(HeaderControl.Instance.CreateControl(this));
            PageFooter.Controls.Add(FooterControl.Instance.CreateControl(this, new FooterViewModel()));
            //ShowDialogs();
        }
    }
}