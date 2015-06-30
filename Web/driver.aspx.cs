// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefront
{
	[PageType("topic")]
    public partial class driver : SkinBase
    {
        private int m_topicid = 0;

        private static Dictionary<string, string> _dctRedirectPages;
        private Dictionary<string, string> RedirectPages
        {
            get
            {
                if (_dctRedirectPages == null)
                {
                    _dctRedirectPages = new Dictionary<string, string>();
                    _dctRedirectPages.Add("contact", "~/ContactUs.aspx");
                }
                return _dctRedirectPages;
            }
        }

        public override bool IsTopicPage
        {
            get
            {
                return true;
            }
        }

        public override int PageID
        {
            get
            {
                String PN = CommonLogic.QueryStringCanBeDangerousContent("SEName");
                return m_topicid;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
            // set the Customer context, and set the SkinBase context, so meta tags will be set if they are not blank in the Topic results
            if (Topic1.TopicName.Length == 0)
            {
                String PN = CommonLogic.QueryStringCanBeDangerousContent("SEName");
                if (PN.Length == 0)
                {
                    PN = CommonLogic.QueryStringCanBeDangerousContent("Topic");
                }
                AppLogic.CheckForScriptTag(PN);

                Topic1.TopicName = PN;
                CheckForRedirect(Topic1.TopicName);

                m_topicid = Topic.GetTopicID(PN, AppLogic.StoreID());

                if (m_topicid > 0)
                {
                    Topic1.TopicID = m_topicid;
                    using (SqlConnection conn = DB.dbConn())
                    {
                        conn.Open();
                        using (IDataReader rs = DB.GetRS(string.Format("select t.Name from Topic t with (NOLOCK) inner join (select distinct a.TopicID from Topic a with (nolock) left join TopicStore b with (NOLOCK) " +
                            "on a.TopicID = b.TopicID where ({0} = 0 or b.StoreID = {1})) b on t.TopicID = b.TopicID where t.Deleted=0 and t.Published=1 and t.TopicID={2}", CommonLogic.IIF(AppLogic.GlobalConfigBool("AllowTopicFiltering") == true, 1, 0), AppLogic.StoreID(), m_topicid), conn))
                        {
                            if (rs.Read())
                            {
                                Topic1.TopicName = DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting);
                            }
                        }
                    }
                }
            }

            if (CommonLogic.IsInteger(Topic1.TopicName))
            {
                Topic1.TopicID = System.Int32.Parse(Topic1.TopicName);
            }
            if (CommonLogic.FormCanBeDangerousContent("Password").Length != 0)
            {
                ThisCustomer.ThisCustomerSession["Topic" + Topic1.TopicName] = Security.MungeString(CommonLogic.FormCanBeDangerousContent("Password"));
            }
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            if (Topic1.Topic == null || (Topic1.Topic.FromDB == false && Topic1.Topic.Contents.Length < 1))
            {
                HttpContext.Current.Server.Transfer("pagenotfound.aspx");
            }
            base.OnLoadComplete(e);
        }

        private void CheckForRedirect(string topic)
        {
            if (RedirectPages.ContainsKey(topic))
            {
                Server.Transfer(RedirectPages[topic]);
            }
        }
    
    }
}
