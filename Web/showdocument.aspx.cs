// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Globalization;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
    /// <summary>
    /// Summary description for showdocument.
    /// </summary>
	[PageType("document")]
    public partial class showdocument : SkinBase
    {
        protected void Page_Load(object sender, System.EventArgs e)
        {
        }

        protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write("not implemented yet");
        }

        public override bool IsEntityPage
        {
            get
            {
                return true;
            }
        }

        public override string EntityType
        {
            get
            {
                return "Document";
            }
        }

        public override int PageID
        {
            get
            {
                // not implemented
                return 0;
            }
        }

    }
}
