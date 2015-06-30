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
using System.IO;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefront
{
    public partial class ConfigureTextPanel : BaseUserControl<LayoutField>
    {
        #region Private Variables

        //private bool bUseHtmlEditor = true;

        private const string DEFAULT_TEXT =
@"This is default text for an ASPDNSFTextField. While logged in as an administrator click here to edit.Lorem ipsum dolor sit amet, 
consectetur adipiscing elit. Aliquam sagittis molestie turpis nec gravida. Duis vitae mauris at sem aliquet vestibulum vel a augue. 
Curabitur ullamcorper tincidunt diam tempus bibendum. Praesent auctor porta fermentum. Praesent tempus, mi ac venenatis vulputate, justo 
massa adipiscing enim, at commodo nunc nulla sed ipsum. Suspendisse eu massa sit amet lacus auctor cursus nec ac tellus.";

        #endregion

        #region Public Properties

        private string m_cssclass;
        public string CssClass
        {
            get { return m_cssclass; }
            set { m_cssclass = value; }
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

        public String Text
        {
            get
            {
                var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(
                    ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("text", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "defaulttext", Value = DEFAULT_TEXT });

                return lfa.Value;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            ThisCustomer = (Page as SkinBase).ThisCustomer;
            ThisField = this.Datasource;
            
            // Determine HTML editor configuration
            //bUseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
            //radText.Visible = bUseHtmlEditor;
            //litText.Visible = !bUseHtmlEditor;

            if (!IsPostBack)
            {
                InitializeControl();
            }
        }

        private void InitializeControl()
        {
            String cssclassvalue = CssClass;
            if (ThisField != null)
            {
                lblLayout.Text = ThisLayout.Name;
                lblField.Text = ThisField.FieldID;

                //if (bUseHtmlEditor)
                //{
                    //radText.Content = this.Text;
                //}
                //else
                //{
                    litText.Text = this.Text;
                //}
            }
        }

        public void SaveAttribute(String Name, String Value)
        {
            LayoutFieldAttribute lfa = new LayoutFieldAttribute();

            lfa.LayoutID = ThisField.LayoutID;
            lfa.LayoutFieldID = ThisField.LayoutFieldID;
            lfa.Name = Name;
            lfa.Value = Value;

            lfa.Update();
        }

        public override bool UpdateChanges()
        {
            ThisField = this.Datasource;

            //if (bUseHtmlEditor)
            //{
                //SaveAttribute("text", radText.Content);
            //}
            //else
            //{
                SaveAttribute("text", litText.Text);
            //}

            OnUpdatedChanges(EventArgs.Empty);

            return true;
        }
    }
}


