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
using AspDotNetStorefrontLayout;
using AspDotNetStorefrontCore;
using AspDotNetStorefront;

namespace AspDotNetStorefront
{
    public partial class LayoutTextPanel : System.Web.UI.UserControl
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

        public const string DEFAULT_TEXT = "This is default text for an ASPDNSFTextField. While logged in as an administrator click here to edit.";
        
        public String Text
        {
            get
            {
                var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("text", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "text", Value = DEFAULT_TEXT });
                
                return lfa.Value;
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            HideModalPanelByDefault(pnlConfigureText);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LayoutField lfield = new LayoutField();

            ThisLayout = (Page as SkinBase).ThisLayout;
            ThisCustomer = (Page as SkinBase).ThisCustomer;
            //ThisParser = (Page as SkinBase).GetParser;

            if (ThisCustomer.IsAdminUser)
            {
                String tName = litTextPanelAdmin.NamingContainer.ID;

                try
                {
                    lfield = ThisLayout.LayoutFields.Single(lf => lf.FieldID.Equals(tName, StringComparison.OrdinalIgnoreCase) && lf.FieldType.Equals(LayoutFieldEnum.ASPDNSFTextField));
                    ThisField = lfield;
                }
                catch { }

                litTextPanel.Visible = false;
                litTextPanelAdmin.Visible = true;
                btnTextPanel.Visible = true;
                //lblTextPanel.Visible = true;

                //ibEditImage.Style["borderwidth"] = "3px";
                //imgEditImage.Attributes.Add("onMouseOver", "this.style='border: 1px solid;'");
                //imgEditImage.Attributes.Add("onMouseOut", "this.style='border: 0px none;'");
            }

            else
            {
                String tName = litTextPanel.NamingContainer.ID;

                try
                {
                    lfield = ThisLayout.LayoutFields.Single(lf => lf.FieldID.Equals(tName, StringComparison.OrdinalIgnoreCase) && lf.FieldType.Equals(LayoutFieldEnum.ASPDNSFTextField));
                    ThisField = lfield;
                }
                catch { }

                litTextPanel.Visible = true;
                litTextPanelAdmin.Visible = false;
                btnTextPanel.Visible = false;
            }

            ConfigureTextPanel ctp = (ConfigureTextPanel)FindControl("ctrlConfigureText");
            ctp.Datasource = ThisField;
            ctp.ThisLayout = ThisLayout;
            //ConfigureImagePanel cip = (ConfigureImagePanel)FindControl("ctrlConfigureImage");
            //cip.Datasource = ThisField;
            //cip.ThisLayout = ThisLayout;

            InitializeControls(lfield);
            //GetDefaultImage();
        }

        private void InitializeControls(LayoutField lf)
        {
            
            String s = this.Text;

            //if(!string.IsNullOrEmpty(s))
            //{
            //    s = ThisParser.ReplacePageDynamicTokens(s);
            //    s = ThisParser.ReplacePageStaticTokens(s);
            //}

            if (ThisCustomer.IsAdminUser)
            {
                // Parent ScriptManager
                ScriptManager sm = Page.Master.FindControl<ScriptManager>("scrptMgr");
                sm.RegisterPostBackControl(btnSaveText);

                litTextPanelAdmin.Text = s;
            }
            else
            {
                litTextPanel.Text = s;
            }

            //if (lf != null && lf.LayoutFieldID > 0)
            //{
            //    if (string.IsNullOrEmpty(Source))
            //    {
            //        if (ThisCustomer.IsAdminUser)
            //        {
            //            GetAdminDefaultImage();
            //        }
            //        else
            //        {
            //            GetDefaultImage();
            //        }
            //    }
            //    else
            //    {
            //        if (ThisCustomer.IsAdminUser)
            //        {
            //            //iImage.Visible = false;
            //            //ibEditImage.Visible = true;
            //            ibEditImage.Height = Height;
            //            ibEditImage.Width = Width;
            //            ibEditImage.ImageUrl = Source;
            //            ibEditImage.AlternateText = Alt;

            //            // Parent ScriptManager
            //            ScriptManager sm = Page.Master.FindControl<ScriptManager>("scrptMgr");
            //            sm.RegisterPostBackControl(ibEditImage);
            //            sm.RegisterPostBackControl(btnSaveImage);
            //        }
            //        else
            //        {
            //            //iImage.Visible = true;
            //            //ibEditImage.Visible = false;
            //            //ibEditImage.Enabled = false;
            //            iImage.Height = Height;
            //            iImage.Width = Width;
            //            iImage.ImageUrl = Source;
            //            iImage.AlternateText = Alt;
            //        }
            //    }
            //}
        }

        //protected void Page_Load(object sender, EventArgs e)
        //{
            //lblTextPanel.Text = "This is default text for an ASPDNSFTextField.  While logged in as an administrator click here to edit." + Environment.NewLine + Environment.NewLine + "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam sagittis molestie turpis nec gravida. Duis vitae mauris at sem aliquet vestibulum vel a augue. Curabitur ullamcorper tincidunt diam tempus bibendum. Praesent auctor porta fermentum. Praesent tempus, mi ac venenatis vulputate, justo massa adipiscing enim, at commodo nunc nulla sed ipsum. Suspendisse eu massa sit amet lacus auctor cursus nec ac tellus.";
        //}

        private void HideModalPanelByDefault(Panel pnl)
        {
            // we can't set the style declaratively
            // and we need the container to be a panel
            // so that we can assign the DefaultButton property

            // hide the div by default so that upon first load there won't be a sudden
            // flicker by the hiding of the div on browser page load
            pnl.Style["display"] = "none";
        }
        protected void btnSaveText_Click(object sender, EventArgs e)
        {
            if (ctrlConfigureText.UpdateChanges())
            {
                ThisLayout.Reload();
                ThisField.Reload();

                InitializeControls(ThisField);
            }
        }
}
}
