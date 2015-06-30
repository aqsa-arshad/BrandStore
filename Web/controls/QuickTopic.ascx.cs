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
using AspDotNetStorefront;
using System.Text;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefront
{
    public partial class QuickTopic : BaseUserControl<Topic>
    {
        //bool bUseHtmlEditor = false;
        private bool m_addmode;
        public bool AddMode
        {
            get { return m_addmode; }
            set { m_addmode = value; }
        }

        private int TopicID
        {
            get { return (Page as SkinBase).PageID; }
        }

        private string pageLocale
        {
            get { return ThisCustomer.LocaleSetting; }
        }

        protected override void  OnPreRender(EventArgs e)
        {
            if (!AddMode)
            {
                DataBind(); 
            }
            
            base.OnPreRender(e);
        }

        public override void DataBind()
        {
            if (TopicID > 0)
            {
                Topic t = new Topic(TopicID, ThisCustomer.LocaleSetting);

                ltName.Text = XmlCommon.GetLocaleEntry(t.TopicName, pageLocale, true);
                txtDspOrdr.Text = t.DisplayOrder.ToString();
                ltTitle.Text = t.SectionTitle;

                //if (bUseHtmlEditor)
                //{
                    //radDescription.Content = XmlCommon.GetLocaleEntry(t.Contents, pageLocale, true); //AppLogic.FormLocaleXml(t.Contents, ThisCustomer.LocaleSetting);
                //}
                //else
                //{
                ltDescription.Text = "<div id=\"idDescription\" style=\"height: 1%;\">";
                ltDescription.Text += "<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\">";
                ltDescription.Text = XmlCommon.GetLocaleEntry(t.Contents, pageLocale, true);
                ltDescription.Text += "</textarea>\n";
                ltDescription.Text += "</div>";
                    
                //}

                rbDisclaimer.SelectedValue = CommonLogic.IIF(t.RequiresDisclaimer, "1", "0");
                rbPublish.SelectedValue = CommonLogic.IIF(t.ShowInSiteMap, "1", "0");
                ltSEKeywords.Text = t.SEKeywords;
                ltSEDescription.Text = t.SEDescription;
                ltSETitle.Text = t.SETitle;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (ThisCustomer == null)
            {
                ThisCustomer = (Page as SkinBase).ThisCustomer;
            }

            resetForm();

            //bUseHtmlEditor = !AppLogic.AppConfigBool("TurnOffHtmlEditorInAdminSite");
            //radDescription.Visible = bUseHtmlEditor;
        }

        public override bool UpdateChanges()
        {
            if (validateForm() && ThisCustomer.IsAdminUser)
            {
                bool bTopicNameExist = Topic.GetTopicID(ltName.Text, pageLocale, AppLogic.StoreID()) > 0;
                    
                if (AddMode)
                {
                    if (!bTopicNameExist)
                    {
                        AddTopic();
                    }
                    else
                    {
                        resetError("Topic Name already exists.", true);
                        return false;
                    }
                }
                else
                {
                    if (bTopicNameExist)
                    {
                        UpdateTopic();
                    }
                    else
                    {
                        resetError("Topic could not be found or does not exist.", true);
                        return false;
                    }
                }

                return base.UpdateChanges();
            }

            return false;
        }

        protected void resetError(string error, bool isError)
        {
            string str = "NOTICE:";
            if (isError)
            {
                str = "ERROR ";
            }

            if (error.Length > 0)
            {
                str += error + "";
            }
            else
            {
                str = "";
            }

            ltError.Text = str;
        }

        protected void AddTopic()
        {
            ltValid.Text = "";

            Topic t = new Topic();

            t.TopicName = ltName.Text;
            t.DisplayOrder = int.Parse(txtDspOrdr.Text);
            t.SectionTitle = ltTitle.Text;

            String desc = String.Empty;
            //if (bUseHtmlEditor)
            //{
                //desc = AppLogic.FormLocaleXml(radDescription.Content, pageLocale);
            //}
            //else
            //{
                desc = AppLogic.FormLocaleXml(CommonLogic.FormCanBeDangerousContent("Description"), pageLocale);
            //}

            t.Contents = desc;
            t.RequiresSubscription = false;
            t.HTMLOk = true;
            t.RequiresDisclaimer = CommonLogic.IIF(rbDisclaimer.SelectedValue.ToString().Equals("0"), false, true);
            t.ShowInSiteMap = CommonLogic.IIF(rbPublish.SelectedValue.ToString().Equals("0"), false, true);
            t.SEKeywords = ltSEKeywords.Text;
            t.SEDescription = ltSEDescription.Text;
            t.SETitle = ltSETitle.Text;

            try
            {
                t.Commit();
                resetError("Topic added.", false);
            }
            catch (Exception ex)
            {
                resetError(ex.Message, true);
                return;
            }
        }

        protected void UpdateTopic()
        {
            ltValid.Text = "";

            Topic t = new Topic(TopicID, pageLocale);

            t.TopicName = XmlCommon.GetLocaleEntry(t.TopicName, pageLocale, true);
            t.DisplayOrder = int.Parse(txtDspOrdr.Text);
            t.SectionTitle = ltTitle.Text;

            String desc = String.Empty;
            //if (bUseHtmlEditor)
            //{
                //desc = AppLogic.FormLocaleXml("Description", radDescription.Content, pageLocale, "topic", Convert.ToInt32(t.TopicID));
            //}
            //else
            //{
                desc = AppLogic.FormLocaleXmlEditor("Description", "Description", pageLocale, "topic", Convert.ToInt32(t.TopicID));
            //}

            t.Contents = desc;
            t.RequiresSubscription = false;
            t.HTMLOk = true;
            t.RequiresDisclaimer = CommonLogic.IIF(rbDisclaimer.SelectedValue.ToString().Equals("0"), false, true);
            t.ShowInSiteMap = CommonLogic.IIF(rbPublish.SelectedValue.ToString().Equals("0"), false, true);
            t.SEKeywords = ltSEKeywords.Text;
            t.SEDescription = ltSEDescription.Text;
            t.SETitle = ltSETitle.Text;

            try
            {
                t.Update();
                resetError("Topic updated.", false);
            }
            catch (Exception ex)
            {
                resetError(ex.Message, true);
            }
        }

        protected bool validateForm()
        {
            bool valid = true;
            string temp = "";

            if ((string.IsNullOrEmpty(ltName.Text) || (AppLogic.FormLocaleXml(ltName.Text, pageLocale).Equals("<ml></ml>"))))
            {
                valid = false;
                temp += "Please fill out the Name";
            }
            else if ((string.IsNullOrEmpty(ltTitle.Text) || (AppLogic.FormLocaleXml(ltTitle.Text, pageLocale).Equals("<ml></ml>"))))
            {
                valid = false;
                temp += "Please fill out the Title";

            }
            if (!valid)
            {
                ltName.Text = XmlCommon.GetLocaleEntry(AppLogic.FormLocaleXml(ltName.Text, pageLocale), pageLocale, true);
                ltTitle.Text = XmlCommon.GetLocaleEntry(AppLogic.FormLocaleXml(ltTitle.Text, pageLocale), pageLocale, true);
                ltValid.Text = string.Format("<script type=\"text/javascript\">alert('{0}');</script>", temp);
            }

            return valid;
        }

        public void resetForm()
        {
            rbDisclaimer.SelectedIndex = 0;
            rbPublish.SelectedIndex = 0;

            ltName.Text = "";
            ltTitle.Text = "";
            //if (bUseHtmlEditor)
            //{
                //radDescription.Content = "";
                //ltDescription.Text = "";
            //}
            //else
            //{
                ltDescription.Text = ("<div id=\"idDescription\" style=\"height: 1%;\">");
                ltDescription.Text += ("<textarea rows=\"" + AppLogic.AppConfigUSInt("Admin_TextareaHeight") + "\" cols=\"" + AppLogic.AppConfigUSInt("Admin_TextareaWidth") + "\" id=\"Description\" name=\"Description\"></textarea>\n");
                ltDescription.Text += ("</div>");
            //}

            ltSETitle.Text = "";
            ltSEKeywords.Text = "";
            ltSEDescription.Text = "";

            resetError("", false);

        }

    }
}
