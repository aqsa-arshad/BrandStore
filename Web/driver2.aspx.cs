// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;
using System.Text;
using System.Collections.Generic;

namespace AspDotNetStorefront
{

    // this provides a topic page that is NOT rendered inside the skin. The topic page itself should also contain the full <html>, not just the body.
	[PageType("topic")]
    public partial class driver2 : System.Web.UI.Page
    {
        Customer ThisCustomer;
        Topic m_T;

        protected void Page_Load(object sender, EventArgs e)
        {
            // set the Customer context, and set the SkinBase context, so meta tags will be set if they are not blank in the Topic results
            ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            String PN = CommonLogic.QueryStringCanBeDangerousContent("TopicName");
            if (PN.Length == 0)
            {
                PN = CommonLogic.QueryStringCanBeDangerousContent("Topic");
            }

            AppLogic.CheckForScriptTag(PN);

            m_T = new Topic(PN, ThisCustomer.LocaleSetting, ThisCustomer.SkinID, null, AppLogic.StoreID());

            if (m_T.HasChildren)
            {
                TopicContents.Text = LoadChildren();
            }

            TopicContents.Text += m_T.ContentsRAW;

            if (CommonLogic.FormCanBeDangerousContent("Password").Length != 0)
            {
                ThisCustomer.ThisCustomerSession["Topic" + PN] = Security.MungeString(CommonLogic.FormCanBeDangerousContent("Password"));
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        public List<int> Children
        {
            get { return m_T.Children; }
        }

        private String LoadChildren()
        {
            StringBuilder sbChildList = new StringBuilder();

            foreach (int child in Children)
            {
                Topic t = new Topic(child);

                sbChildList.Append("<p align=\"left\">");
                sbChildList.Append("&#0160;&#0160;&#0160;<img border=\"0\" src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + ThisCustomer.SkinID.ToString() + "/images/redarrow.gif", ThisCustomer.LocaleSetting) + "\"></img>&#0160;");
                sbChildList.Append("<a href=\"" + SE.MakeDriver2Link(XmlCommon.GetLocaleEntry(t.TopicName, ThisCustomer.LocaleSetting, true)) + "\">");
                sbChildList.Append(XmlCommon.GetLocaleEntry(t.SectionTitle, ThisCustomer.LocaleSetting, true));
                sbChildList.Append("</a>");
                sbChildList.Append("</p>");
            }

            return sbChildList.ToString();
        }
    }


}
