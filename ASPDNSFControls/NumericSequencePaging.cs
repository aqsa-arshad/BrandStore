// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Custom control used to render numeric sequence paging. i.e. First, 2, 3...Last
    /// </summary>
    public class NumericSequencePaging : CompositeControl
    {
        private const string PAGE_COUNT = "PageCount";
        private const string CURRENT_PAGE = "CurrentPage";
        private const string PAGE_COMMAND = "PageCommand";

        [Browsable(true)]
        public int PageCount
        {
            get
            {
                object intValue = ViewState[PAGE_COUNT];
                if (null == intValue) { return 0; }

                return (int)intValue;
            }
            set
            {
                ViewState[PAGE_COUNT] = value;

                ChildControlsCreated = false;
            }
        }


        [Browsable(true)]
        public int CurrentPage
        {
            get
            {
                object intValue = ViewState[CURRENT_PAGE];
                if (null == intValue) { return 1; }

                return (int)intValue;
            }
            set
            {
                ViewState[CURRENT_PAGE] = value;

                ChildControlsCreated = false;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        //private delegate void SetupPage(int page, string displayText);

        private void SetupPage(int page, string displayText)
        {
            LinkButton lnkPage = new LinkButton();
            lnkPage.ID = "Page_" + displayText + "_" + page.ToString();
            lnkPage.CommandName = PAGE_COMMAND;
            lnkPage.CommandArgument = page.ToString();
            lnkPage.Text = displayText;

            //displaySelectableStyle(lnkPage);

            Controls.Add(lnkPage);
            // spacer
            Controls.Add(new LiteralControl("&nbsp;&nbsp;"));
        }

        protected override void CreateChildControls()
        {
            if (this.PageCount > 1)
            {
                //Action<WebControl> displaySelectedStyle = c => c.Font.Size = FontUnit.Small;
                //Action<WebControl> displaySelectableStyle = c =>
                //{
                //    c.Attributes["onmouseover"] = string.Format("this.style.fontSize = '{0}'", FontUnit.Small);
                //    c.Attributes["onmouseout"] = string.Format("this.style.fontSize = '{0}'", FontUnit.Small);
                //};

                //SetupPage doSetupPage = (int page, string displayText) =>
                //{
                //    //LinkButton lnkPage = new LinkButton();
                //    //lnkPage.ID = "Page_" + displayText + "_" + page.ToString();
                //    //lnkPage.CommandName = PAGE_COMMAND;
                //    //lnkPage.CommandArgument = page.ToString();
                //    //lnkPage.Text = displayText;

                //    ////displaySelectableStyle(lnkPage);

                //    //Controls.Add(lnkPage);
                //    //// spacer
                //    //Controls.Add(new LiteralControl("&nbsp;&nbsp;"));
                //};

                int lastPage = this.PageCount;

                int current = this.CurrentPage;

                // First Page
                if (current > 1)
                {
                    SetupPage(1, "First");
                    SetupPage(current - 1, "<<");
                }
                else
                {
                    SetupPage(1, "First");
                    SetupPage(current, "<<");
                }

                int limit = 5;

                int start = 1;
                int end = lastPage;

                if (lastPage > limit)
                {
                    if (current > limit)
                    {
                        start = current - limit;
                        end = start + limit;
                    }
                    else
                    {
                        end = limit;
                    }
                }

                if (start > current)
                {
                    SetupPage(start - 1, "...");
                }

                for (int page = start; page <= end; page++)
                {
                    if (this.CurrentPage == page)
                    {
                        Label lblPage = new Label();
                        lblPage.Text = page.ToString();

                        //displaySelectedStyle(lblPage);

                        Controls.Add(lblPage);

                        // spacer
                        Controls.Add(new LiteralControl("&nbsp;&nbsp;"));
                    }
                    else
                    {
                        SetupPage(page, page.ToString());
                    }
                }


                if (end < lastPage)
                {
                    SetupPage(end + 1, "...");
                }

                // last page
                if (current < lastPage)
                {
                    SetupPage(current + 1, ">>");
                    SetupPage(lastPage, "Last");
                }
                else
                {
                    SetupPage(current, ">>");
                    SetupPage(lastPage, "Last");
                }

            }
            else
            {
                // spacer
                Controls.Add(new LiteralControl("&nbsp;&nbsp;"));
            }

            base.CreateChildControls();
        }
    }
}
