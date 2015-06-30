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
using AspDotNetStorefrontLayout;
using System.Drawing;
using AspDotNetStorefront;

namespace AspDotNetStorefront
{
    public partial class LayoutListItem : System.Web.UI.UserControl
    {
        private String m_layoutname;
        public String LayoutName
        {
            get { return m_layoutname; }
            set { m_layoutname = value; }
        }
        private int m_skinid;
        public new int SkinID
        {
            get { return m_skinid; }
            set { m_skinid = value; }
        }
        private String m_localesetting;
        public String LocaleSetting
        {
            get { return m_localesetting; }
            set { m_localesetting = value; }
        }
        private String m_layoutthumb;
        public String LayoutThumb
        {
            get { return m_layoutthumb; }
            set { m_layoutthumb = value; }
        }
        private String m_layoutlarge;
        public String LayoutLarge
        {
            get { return m_layoutlarge; }
            set { m_layoutlarge = value; }
        }
        private Color m_backcolor;
        public Color BackColor
        {
            get { return m_backcolor; }
            set { m_backcolor = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //lbtnMain.Attributes.Add("onClick", "return false;");

            if (!String.IsNullOrEmpty(LayoutThumb))
            {
                imgThumb.ImageUrl = LayoutThumb;
            }
            else
            {
                imgThumb.ImageUrl = AppLogic.NoPictureImageURL(true, SkinID, LocaleSetting);
            }

            if (!String.IsNullOrEmpty(LayoutLarge))
            {
                imgLarge.ImageUrl = LayoutLarge;
            }
            else
            {
                imgLarge.ImageUrl = AppLogic.NoPictureImageURL(false, SkinID, LocaleSetting);
            }

            if (BackColor != Color.Empty)
            {
                pnlMain.BackColor = BackColor;
            }

            litName.Text = LayoutName;
        }
    }
}
