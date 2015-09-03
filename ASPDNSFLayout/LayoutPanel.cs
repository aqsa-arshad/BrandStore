// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspDotNetStorefrontLayout.Behaviors;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspDotNetStorefrontLayout
{
    public class LayoutPanel : UserControl
    {
        private IBehavior behavior = null;
        //protected Panel pnlBody = null;

        public LayoutHost Host { get; set; }
        public IBehavior Behavior 
        {
            get { return behavior; }
            set 
            { 
                behavior = value;
            }
        }

        public bool IsNew
        {
            get
            {
                object booleanValue = ViewState["IsNew"];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["IsNew"] = value;
            }
        }

        public string CurrentBehavior
        {
            get { return null == ViewState["CurrentBehavior"] ? string.Empty : (string)ViewState["CurrentBehavior"]; }
            set { ViewState["CurrentBehavior"] = value; }
        }
        
        public bool EditMode
        {
            get
            {
                object booleanValue = ViewState["EditMode"];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState["EditMode"] = value;
            }
        }

        public event EventHandler BehaviorChanged;
        public event EventHandler EditInvoked;


        public virtual void Initialize()
        {
            if (this.Behavior != null)
            {
                this.Behavior.Container = this;
                this.Behavior.Host = this.Host;
                this.Behavior.Initialize();
            }
            //this.Host.FileName 
        }

        protected void OnEditInvoked(EventArgs e)
        {
            if (EditInvoked != null)
            {
                EditInvoked(this, e);
            }
        }

        protected void OnBehaviorChanged(EventArgs e)
        {
            if (BehaviorChanged != null)
            {
                BehaviorChanged(this, e);
            }
        }

    }
}
