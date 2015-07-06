// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontLayout;
using AspDotNetStorefrontControls;
using AjaxControlToolkit;
using AspDotNetStorefront;
using System.Reflection;
using System.Xml;
using System.IO;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class EditLayout : BaseUserControl<LayoutData>
    {
        private bool m_addmode;
        public bool AddMode
        {
            get { return m_addmode; }
            set { m_addmode = value; }
        }

        private string m_cssclass;
        public string CssClass
        {
            get { return m_cssclass; }
            set { m_cssclass = value; }
        }

        private const int DEFAULT_COLUMN_LENGTH = 35;
        private const int MAX_COLUMN_LENGTH = 75;
        private const int MAX_ROW_LENGTH = 4;

        private LayoutData m_thislayout;
        public LayoutData thisLayout
        {
            get { return m_thislayout; }
            set { m_thislayout = value; }
        }

        public override void DataBind()
        {
            if (this.AddMode)
            {
                txtName.Text = String.Empty;
                txtDescription.Text = String.Empty;
                txtLayout.Text = String.Empty;
            }
            else
            {
                if (thisLayout == null)
                {
                    thisLayout = this.Datasource;
                }

                if (thisLayout != null)
                {
                    txtName.Text = thisLayout.Name;
                    txtDescription.Text = thisLayout.Description;
                    txtLayout.Text = XmlCommon.PrettyPrintXml(thisLayout.HTML);

                    string tImage = "~/images/layouts/icon/" + thisLayout.Icon;
                    string lImage = "~/images/layouts/medium/" + thisLayout.Large;

                    if (CommonLogic.FileExists(CommonLogic.SafeMapPath(tImage)))
                    {
                        imgThumb.ImageUrl = tImage;
                        imgPopThumb.ImageUrl = tImage;
                    }

                    if (CommonLogic.FileExists(CommonLogic.SafeMapPath(lImage)))
                    {
                        imgLarge.ImageUrl = lImage;
                        imgPopLarge.ImageUrl = lImage;
                    }
                }
            }

            base.DataBind();
        }

        protected override void OnInit(EventArgs e)
        {
            AddMode = false;
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int baseSkinID = (Page as AdminPageBase).SkinID;
            String baseLocaleSetting = (Page as AdminPageBase).LocaleSetting;

            thisLayout = this.Datasource;

            imgLarge.ImageUrl = AppLogic.NoPictureImageURL(false, baseSkinID, baseLocaleSetting);
            imgThumb.ImageUrl = AppLogic.NoPictureImageURL(true, baseSkinID, baseLocaleSetting);

            if (!IsPostBack)
            {
                if (!this.AddMode)
                {
                    DataBind();
                }
            }
        }

        private void ClearErrors()
        {
            HasErrors = false;
            pnlError.Visible = false;
            lblError.Text = string.Empty;
        }

        public override bool UpdateChanges()
        {     
            var thisLayout = this.Datasource;

            if (this.AddMode)
            {
                thisLayout.Name = txtName.Text;
                thisLayout.Description = txtDescription.Text;
                
                if (string.IsNullOrEmpty(thisLayout.Name))
                {
                    this.HasErrors = true;
                    pnlError.Visible = true;
                    lblError.Text = "Please specify name";
                    return false;
                }

                bool allow = true;

                if (LayoutData.LayoutExists(thisLayout.Name))
                {
                    allow = false;

                    if (!allow)
                    {
                        this.HasErrors = true;
                        pnlError.Visible = true;
                        lblError.Text = "Layout already exists. Please specify a different name.";
                    }
                }

                //new add checking failed, don't allow
                if (!allow)
                {
                    return false;
                }

                if (fuLayoutFile.HasFile)
                {
                    try
                    {
                        XmlDocument xdoc = new XmlDocument();

                        using (StreamReader sr = new StreamReader(fuLayoutFile.FileContent))
                        {
                            string xmlstr = sr.ReadToEnd();

                            xmlstr = xmlstr
                                .Replace("&nbsp;", "#0160;")
                                .Replace("”", "")
                                .Replace("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>",String.Empty);

                            xdoc.LoadXml(xmlstr);
                            sr.Close();
                            sr.Dispose();
                        }

                        thisLayout.HTML = xdoc.OuterXml;

                    }
                    catch (Exception ex)
                    {
                        thisLayout.HTML = ex.Message;
                    }
                }
                else
                {
                    if (txtLayout.Text.Length > 0)
                    {
                        thisLayout.HTML = txtLayout.Text;
                    }
                }

                if (fuLayoutThumb.HasFile)
                {
                    // Upload image
                    thisLayout.Icon = fuLayoutThumb.FileName;

                    // TBD: Tie into image resizing
                    String image1 = String.Empty;
                    HttpPostedFile image1File = fuLayoutThumb.PostedFile;

                    if (image1File != null && image1File.ContentLength != 0)
                    {
                        image1 = CommonLogic.SafeMapPath("~/images/layouts/icon/" + fuLayoutThumb.FileName);
                        image1File.SaveAs(image1);
                    }
                }

                if (fuLayoutLarge.HasFile)
                {
                    // Upload image
                    thisLayout.Large = fuLayoutLarge.FileName;

                    // TBD: Tie into image resizing
                    String image1 = String.Empty;
                    HttpPostedFile image1File = fuLayoutLarge.PostedFile;

                    if (image1File != null && image1File.ContentLength != 0)
                    {
                        image1 = CommonLogic.SafeMapPath("~/images/layouts/medium/" + fuLayoutLarge.FileName);
                        image1File.SaveAs(image1);
                    }
                }

                thisLayout.Commit();
                thisLayout.Reload();
                this.Datasource = thisLayout;
                DataBind();

                ClearErrors(); // if any
            }
            else
            {
                UpdateCurrentLayout(thisLayout);
            }

            OnUpdatedChanges(EventArgs.Empty);

            return true;
        }

        private void UpdateCurrentLayout(LayoutData thisLayout)
        {
            if(fuLayoutFile.HasFile)
            {
                try
                {
                    XmlDocument xdoc = new XmlDocument();

                    using (StreamReader sr = new StreamReader(fuLayoutFile.FileContent))
                    {
                        string xmlstr = sr.ReadToEnd();

                        xmlstr = xmlstr.Replace("&nbsp;", "#0160;").Replace("”","")
                            .Replace("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>",String.Empty);

                        xdoc.LoadXml(xmlstr);
                        sr.Close();
                        sr.Dispose();
                    }

                    thisLayout.HTML = xdoc.OuterXml;

                }
                catch { }
            }
            else
            {
                if (txtLayout.Text.Length > 0)
                    {
                        thisLayout.HTML = txtLayout.Text;
                    }
            }

            if (fuLayoutLarge.HasFile)
            {
                // Upload image
                thisLayout.Large = fuLayoutLarge.FileName;

                // TBD: Tie into image resizing
                String image1 = String.Empty;
                HttpPostedFile image1File = fuLayoutLarge.PostedFile;

                if (image1File != null && image1File.ContentLength != 0)
                {
                    image1 = CommonLogic.SafeMapPath("~/images/layouts/medium/" + fuLayoutLarge.FileName);
                    image1File.SaveAs(image1);
                }
            }

            if (fuLayoutThumb.HasFile)
            {
                // Upload image
                thisLayout.Icon = fuLayoutThumb.FileName;

                // TBD: Tie into image resizing
                String image1 = String.Empty;
                HttpPostedFile image1File = fuLayoutThumb.PostedFile;

                if (image1File != null && image1File.ContentLength != 0)
                {
                    image1 = CommonLogic.SafeMapPath("~/images/layouts/icon/" + fuLayoutThumb.FileName);
                    image1File.SaveAs(image1);
                }
            }

            thisLayout.Description = txtDescription.Text;
            thisLayout.UpdatedOn = DateTime.Now;
            thisLayout.Update();

            thisLayout.Reload();
            this.Datasource = thisLayout;
            DataBind();
        }
    }
}


