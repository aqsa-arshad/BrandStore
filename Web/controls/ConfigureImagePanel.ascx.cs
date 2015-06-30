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
using AspDotNetStorefrontControls;
using System.IO;

namespace AspDotNetStorefront
{
    public partial class ConfigureImagePanel : BaseUserControl<LayoutField>
    {
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
        
        public int Width
        {
            get 
            {
                var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(
                    ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("width", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "width", Value = "0" });

                return int.Parse(lfa.Value);
            }
        }

        public int Height
        {
            get
            {
                var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(
                    ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("height", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name="height", Value="0" });

                return int.Parse(lfa.Value);
            }
        }

        public String Source
        {
            get
            {
                var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(
                    ThisField.LayoutFieldAttributes
                    .SingleOrDefault(lf => lf.Name.Equals("source", StringComparison.OrdinalIgnoreCase)),
                    new LayoutFieldAttribute { Name = "defaultsource", Value = "~/images/grey.gif" });

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

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            ThisCustomer = (Page as SkinBase).ThisCustomer;
            ThisField = this.Datasource;
            
            trAdvanced.Visible = pnlAdvanced.Visible = ThisCustomer.IsAdminUser;

            if (!IsPostBack)
            {
                InitializeControl();
            }
        }

        private void InitializeAdvancedControl()
        {
            if (ThisField != null)
            {
                txtAlt.Text = this.Alt;
            }
        }

        private void InitializeControl()
        {
            String cssclassvalue = CssClass;
            if (ThisField != null)
            {
                lblLayout.Text = ThisLayout.Name;
                lblField.Text = ThisField.FieldID;
                
                imgCurrent.AlternateText = txtAlt.Text = lblAlt.Text = this.Alt;
                imgCurrent.ImageUrl = lblSource.Text = this.Source;

                if (this.Width > 0)
                {
                    trWidth.Visible = true;
                    litWidth.Visible = true;
                    lblWidth.Visible = true;
                    imgCurrent.Width = this.Width;
                    txtWidth.Text = this.Width.ToString();
                    lblWidth.Text = this.Width.ToString() + "px";
                }
                else
                {
                    trWidth.Visible = false;
                    litWidth.Visible = false;
                    lblWidth.Visible = false;
                }
                if (this.Height > 0)
                {
                    trHeight.Visible = true;
                    litHeight.Visible = true;
                    lblHeight.Visible = true;
                    imgCurrent.Height = this.Height;
                    txtHeight.Text = this.Height.ToString();
                    lblHeight.Text = this.Height.ToString() + "px";
                }
                else
                {
                    trHeight.Visible = false;
                    litHeight.Visible = false;
                    lblHeight.Visible = false;
                }
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

            if (fuImageUpload.HasFile)
            {
                String origImageName = fuImageUpload.FileName;
                String image1 = String.Empty;
                String tempImage1 = String.Empty;
                HttpPostedFile image1File = fuImageUpload.PostedFile;
                String imageContentType = "image/gif";

                String layoutImagesDir = "images/layouts/" + ThisField.LayoutID.ToString();

                bool exists = Directory.Exists(layoutImagesDir);

                if (!exists)
                {
                    Directory.CreateDirectory(CommonLogic.SafeMapPath(layoutImagesDir));
                }

                if (image1File != null && image1File.ContentLength != 0)
                {
                    switch (image1File.ContentType)
                    {
                        case "image/gif":
                            //imageExtension = ".gif";
                            imageContentType = "image/gif";
                            break;
                        case "image/x-png":
                        case "image/png":
                            //imageExtension = ".png";
                            imageContentType = "image/png";
                            break;
                        case "image/jpg":
                        case "image/jpeg":
                        case "image/pjpeg":
                            //imageExtension = ".jpg";
                            imageContentType = "image/jpeg";
                            break;
                    }
                }

                if (cbUseImageResize.Checked)
                {
                    tempImage1 = AppLogic.GetImagePath("layouts", ThisField.LayoutID.ToString(), true) + "tmp_" + origImageName;
                    image1 = AppLogic.GetImagePath("layouts", ThisField.LayoutID.ToString(), true) + origImageName;
                    image1File.SaveAs(tempImage1);
                    AppLogic.ResizeEntityOrObject("layouts", tempImage1, image1, "medium", imageContentType);
                    AppLogic.DisposeOfTempImage(tempImage1);
                }
                else
                {
                    image1 = AppLogic.GetImagePath("layouts", ThisField.LayoutID.ToString(), true) + origImageName;
                    image1File.SaveAs(image1);
                }

                SaveAttribute("source", origImageName);
            }

            SaveAttribute("alt", txtAlt.Text);
            
            if (txtWidth.Text.Length > 0)
            {
                if (CommonLogic.IsInteger(txtWidth.Text))
                {
                    SaveAttribute("width", txtWidth.Text);
                }
            }

            if (txtHeight.Text.Length > 0)
            {
                if (CommonLogic.IsInteger(txtHeight.Text))
                {
                    SaveAttribute("height", txtHeight.Text);
                }
            }

            ThisField.Reload();
            this.Datasource = ThisField;
            InitializeControl();

            OnUpdatedChanges(EventArgs.Empty);

            return true;
        }

    }
}


