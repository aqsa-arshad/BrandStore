// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using System.ComponentModel;
using System.Drawing;

namespace AspDotNetStorefrontControls
{
    public class BaseUserControl<T> : UserControl where T : class
    {
        private const string HAS_ERRORS = "HasErrors";

        public event EventHandler UpdatedChanges;

        private T m_datasource;
        private Customer m_thiscustomer; 

        public T Datasource
        {
            get { return m_datasource; }
            set { m_datasource = value; }
        }

        public virtual bool UpdateChanges() { return true; }

        public Customer ThisCustomer
        {
            get { return m_thiscustomer; }
            set { m_thiscustomer = value; }
        }

        [Browsable(false)]
        public bool HasErrors
        {
            get
            {
                object booleanValue = ViewState[HAS_ERRORS];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[HAS_ERRORS] = value;
            }
        }

        //protected override void OnInit(EventArgs e)
        //{
        //    this.AppRelativeTemplateSourceDirectory = "~/";
        //    base.OnInit(e);
        //}

        protected override void OnLoad(EventArgs e)
        {
            if (this.Page != null)
            {
                this.AppRelativeTemplateSourceDirectory = Page.AppRelativeTemplateSourceDirectory;
            }

            base.OnLoad(e);
        }

        protected virtual void OnUpdatedChanges(EventArgs e)
        {
            if (UpdatedChanges != null)
            {
                UpdatedChanges(this, e);
            }
        }
    }
}
