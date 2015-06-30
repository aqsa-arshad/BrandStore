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
using System.Drawing;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontLayout;
using AspDotNetStorefront;

namespace AspDotNetStorefront
{
    public partial class ImagePanel : System.Web.UI.UserControl
    {
        #region Public Properties

        private Customer m_thiscustomer;
        public Customer ThisCustomer
        {
            get { return m_thiscustomer; }
            set { m_thiscustomer = value; }
        }
        private LayoutData m_thislayout;
        public LayoutData ThisLayout
        {
            get { return m_thislayout; }
            set { m_thislayout = value; }
        }
        private LayoutField m_thisfield;
        public LayoutField ThisField
        {
            get { return m_thisfield; }
            set { m_thisfield = value; }
        }
        //public Parser ThisParser { get; set; }

        public int Width
        {
            get
            {
                var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("width", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "width", Value = "250" });

                return int.Parse(lfa.Value);
            }
        }

        public int Height
        {
            get
            {
                var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("height", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "height", Value = "250" });

                return int.Parse(lfa.Value);
            }
        }
        public String Source
        {
            get
            {
                var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("source", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "defaultsource", Value = "~/images/grey.png" });

                if (lfa.Name.Equals("defaultsource"))
                {
                    return lfa.Value;
                }

                return "~/images/layouts/" + lfa.LayoutID.ToString() + "/" + lfa.Value;
            }
        }

        public String Alt
        {
            get
            {
                var lfa =
                    (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("alt", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "alt", Value = "alt text" });

                return lfa.Value;
            }
        }

        public String Style
        {
            get
            {
                var lfa =
                    (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("style", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "style", Value = "" });

                return lfa.Value;
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            HideModalPanelByDefault(pnlConfigureImage);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LayoutField lfield = new LayoutField();

            ThisLayout = (Page as SkinBase).ThisLayout;
            ThisCustomer = (Page as SkinBase).ThisCustomer;
            //ThisParser = (Page as SkinBase).GetParser;

            if (ThisCustomer.IsAdminUser)
            {
                String iName = ibEditImage.NamingContainer.ID;

                try
                {
                    lfield = ThisLayout.LayoutFields.Single(lf => lf.FieldID.Equals(iName, StringComparison.OrdinalIgnoreCase) && lf.FieldType.Equals(LayoutFieldEnum.ASPDNSFImageField));
                    ThisField = lfield;
                }
                catch { }
            
                iImage.Visible = false;
                ibEditImage.Visible = true;
            }

            else
            {
                String iName = iImage.NamingContainer.ID;

                try
                {
                    lfield = ThisLayout.LayoutFields.Single(lf => lf.FieldID.Equals(iName, StringComparison.OrdinalIgnoreCase) && lf.FieldType.Equals(LayoutFieldEnum.ASPDNSFImageField));
                    ThisField = lfield;
                }
                catch { }

                iImage.Visible = true;
                ibEditImage.Visible = false;
            }

            ConfigureImagePanel cip = (ConfigureImagePanel)FindControl("ctrlConfigureImage");
            cip.Datasource = ThisField;
            cip.ThisLayout = ThisLayout;

            InitializeControls(lfield);
        }

        private void InitializeControls(LayoutField lf)
        {
            //if (!string.IsNullOrEmpty(src))
            //{
            //    src = ThisParser.ReplacePageStaticTokens(src);
            //    src = ThisParser.ReplacePageDynamicTokens(src);
            //}

            //if (!string.IsNullOrEmpty(alttxt))
            //{
            //    alttxt = ThisParser.ReplacePageStaticTokens(alttxt);
            //    alttxt = ThisParser.ReplacePageDynamicTokens(alttxt);
            //}

            if (lf != null && lf.LayoutFieldID > 0)
            {
                if (string.IsNullOrEmpty(this.Source) || !CommonLogic.FileExists(this.Source))
                {
                    if (ThisCustomer.IsAdminUser)
                    {
                        GetAdminDefaultImage();
                    }
                    else
                    {
                        GetDefaultImage();
                    }
                }
                else
                {
                    if (ThisCustomer.IsAdminUser)
                    {
                        //if (this.Height > 0)
                        //{
                        //    ibEditImage.Height = this.Height;
                        //}
                        //if (this.Width > 0)
                        //{
                        //    ibEditImage.Width = this.Width;
                        //}

                        ibEditImage.ImageUrl = "~/layoutimage.ashx?layoutfieldid=" + lf.LayoutFieldID.ToString();
                        ibEditImage.AlternateText = this.Alt;

                        // Parent ScriptManager
                        ScriptManager sm = Page.Master.FindControl<ScriptManager>("scrptMgr");
                        sm.RegisterPostBackControl(ibEditImage);
                        sm.RegisterPostBackControl(btnSaveImage);
                    }
                    else
                    {
                        //if (this.Height > 0)
                        //{
                        //    iImage.Height = this.Height;
                        //}
                        //if (this.Width > 0)
                        //{
                        //    iImage.Width = this.Width;
                        //}
                        iImage.ImageUrl = this.Source;
                        iImage.AlternateText = this.Alt;
                    }
                }
            }
        }

        private void GetAdminDefaultImage()
        {
            ibEditImage.ImageUrl = "~/layoutimage.ashx?layoutfieldid=" + ThisField.LayoutFieldID.ToString();
            //ibEditImage.ImageUrl = "~/images/grey.gif";
            //ibEditImage.Width = CommonLogic.IIF(this.Width > 0, this.Width, 100);
            //ibEditImage.Height = CommonLogic.IIF(this.Height > 0, this.Height, 100);

            // Parent ScriptManager
            ScriptManager sm = Page.Master.FindControl<ScriptManager>("scrptMgr");
            sm.RegisterPostBackControl(ibEditImage);
        }

        private void GetDefaultImage()
        {
            //iImage.ImageUrl = "~/layoutimage.ashx?layoutfieldid=" + ThisField.LayoutFieldID.ToString();
            iImage.ImageUrl = "~/images/grey.png";
            iImage.Width = this.Width;
            iImage.Height = this.Height;
            //iImage.Width = CommonLogic.IIF(this.Width > 0, this.Width, 100);
            //iImage.Height = CommonLogic.IIF(this.Height > 0, this.Height, 100);
        }

        protected void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (ctrlConfigureImage.UpdateChanges())
            {
                ThisField.Reload();
                InitializeControls(ThisField);
            }
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
    }
}
