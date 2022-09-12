using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace LexViz
{
    public abstract class AbstractUserControl : UserControl
    {
        public abstract string ControlPath { get; }
    }

    public abstract class AbstractUserControl<TDerivedControl> : AbstractUserControl
        where TDerivedControl : AbstractUserControl<TDerivedControl>, new()
    {
        private static TDerivedControl instance = new TDerivedControl();
        public static TDerivedControl Instance
        {
            get { return instance; }
        }

        public virtual TDerivedControl CreateControl(TemplateControl requestingControl)
        {
            return (TDerivedControl)requestingControl.LoadControl(ControlPath);
        }
    }

    public abstract class AbstractUserControl<TDerivedControl, TViewModel> : AbstractUserControl<TDerivedControl>
        where TDerivedControl : AbstractUserControl<TDerivedControl, TViewModel>, new()
    {
        public TViewModel ViewModel { get; protected set; }

        protected abstract void LoadData(TViewModel viewModel);

        public virtual TDerivedControl CreateControl(TemplateControl requestingControl, TViewModel model)
        {
            var derivedControl = (TDerivedControl)requestingControl.LoadControl(ControlPath);
            derivedControl.ViewModel = model;
            derivedControl.LoadData(model);
            return derivedControl;
        }
    }

    //public abstract class EncapsulatedUserControl<TControl, TViewModel> : AbstractUserControl<TControl> where TControl : EncapsulatedUserControl<TControl, TViewModel>, new()
    //{
    //    public TViewModel ViewModel { get; protected set; }

    //    protected abstract void LoadData(TViewModel viewModel);

    //    public virtual TControl CreateControl(TemplateControl requestingControl, TViewModel model)
    //    {
    //        var derivedControl = (TControl)requestingControl.LoadControl(ControlPath);
    //        derivedControl.ViewModel = model;
    //        derivedControl.LoadData(model);
    //        return derivedControl;
    //    }
    //}
}