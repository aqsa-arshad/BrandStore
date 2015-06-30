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

namespace AspDotNetStorefront
{
    public partial class QuickEdit : System.Web.UI.UserControl
    {
        private Customer m_thiscustomer;
        protected Customer ThisCustomer
        {
            get { return m_thiscustomer; }
            set { m_thiscustomer = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        private void HideModalPanelByDefault(Panel pnl)
        {
            // we can't set the style declaratively
            // and we need the container to be a panel
            // so that we can assign the DefaultButton property

            // hide the div by default so that upon first load there won't be a sudden
            // flicker by the hiding of the div on browser page load
            pnl.Style["display"] = "none";
        }

        protected int EditingID
        {
            get
            {
                return (Page as AspDotNetStorefront.SkinBase).PageID;
            }
        }

        protected bool IsTopicPage
        {
            get
            {
                return (Page as AspDotNetStorefront.SkinBase).IsTopicPage;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            ThisCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;

            HideModalPanelByDefault(pnlQuickTopic);
            HideModalPanelByDefault(pnlQuickEditTopic);
            HideModalPanelByDefault(pnlMapLayout);

            base.OnInit(e);
        }

        protected void btnSubmitQuickTopic_Click(object sender, EventArgs e)
        {
            if (quickTopic.UpdateChanges())
            {
                quickTopic.resetForm();
                HideModalPanelByDefault(pnlQuickTopic);
            }
        }
        protected void btnSubmitQuickEditTopic_Click(object sender, EventArgs e)
        {
            if (quickEditTopic.UpdateChanges())
            {
                quickEditTopic.resetForm();
                HideModalPanelByDefault(pnlQuickEditTopic);
            }
        }

        protected void btnCancelQuickTopic_Click(object sender, EventArgs e)
        {
            quickTopic.resetForm();
        }

        protected void btnCancelQuickEditTopic_Click(object sender, EventArgs e)
        {
            quickEditTopic.resetForm();
        }

        protected void btnSubmitMapLayout_Click(object sender, EventArgs e)
        {
            quickMapLayout.UpdateChanges();
            quickMapLayout.SetVisibility(true);
        }

        protected void btnCancelMapLayout_Click(object sender, EventArgs e)
        {

        }

    }
}
