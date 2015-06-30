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
    public class AlphaPaging : CompositeControl
    {
        private const string CURRENT_FILTER = "CurrentFilter";
        private const string THISHIGHLIGHT = "Highlight";
        public const string ALPHA_FILTER_COMMAND = "AlphaFilter";
        private List<KeyValuePair<string, string>> alphaFilters = new List<KeyValuePair<string, string>>();

        public AlphaPaging()
        {
            //InitializeAlphaFilters();
        }

        private void CreateAlphaFilters()
        {
            alphaFilters.Clear();

            alphaFilters.Add(new KeyValuePair<string, string>("All", ""));
            alphaFilters.Add(new KeyValuePair<string, string>("#", "[0-9]"));

            List<string> alpha = new List<string>();
            alpha.Add("A");
            alpha.Add("B");
            alpha.Add("C");
            alpha.Add("D");
            alpha.Add("E");
            alpha.Add("F");
            alpha.Add("G");
            alpha.Add("H");
            alpha.Add("I");
            alpha.Add("J");
            alpha.Add("K");
            alpha.Add("L");
            alpha.Add("M");
            alpha.Add("N");
            alpha.Add("O");
            alpha.Add("P");
            alpha.Add("Q");
            alpha.Add("R");
            alpha.Add("S");
            alpha.Add("T");
            alpha.Add("U");
            alpha.Add("V");
            alpha.Add("W");
            alpha.Add("X");
            alpha.Add("Y");
            alpha.Add("Z");

            if (this.AlphaGrouping > 1)
            {
                int current = 0;
                int gpx = (this.AlphaGrouping - 1);

                while ((current + gpx) < alpha.Count)
                {
                    int start = current;
                    int end = current + gpx;

                    alphaFilters.Add(new KeyValuePair<string, string>(string.Format("{0}-{1}", alpha[start], alpha[end]), string.Format("[{0}-{1}]", alpha[start], alpha[end])));

                    current += this.AlphaGrouping;
                }

                if (current < alpha.Count)
                {
                    int residue = (alpha.Count - 1) - current;
                    int start = current;
                    int end = current + residue;

                    if (start == end) // 1 char residue
                    {
                        alphaFilters.Add(new KeyValuePair<string, string>(alpha[start], alpha[start]));
                    }
                    else
                    {
                        alphaFilters.Add(new KeyValuePair<string, string>(string.Format("{0}-{1}", alpha[start], alpha[end]), string.Format("[{0}-{1}]", alpha[start], alpha[end])));
                    }
                }
            }
            else
            {
                foreach (String chr in alpha)
                {
                    alphaFilters.Add(new KeyValuePair<string, string>(chr, chr));
                }
            }
        }

        [Browsable(true)]
        public int AlphaGrouping
        {
            get
            {
                object intValue = ViewState["AlphaGrouping"];
                if (null == intValue) { return 1; }

                return (int)intValue;
            }
            set
            {
                ViewState["AlphaGrouping"] = value;

                ChildControlsCreated = false;
            }
        }

        public string CurrentFilter
        {
            get
            {
                object savedValue = ViewState[CURRENT_FILTER];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState[CURRENT_FILTER] = value;
                ChildControlsCreated = false;
            }
        }

        [Browsable(true)]
        public bool Highlight
        {
            get
            {
                object booleanValue = ViewState[THISHIGHLIGHT];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[THISHIGHLIGHT] = value;
                ChildControlsCreated = false;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            //Action<WebControl> displaySelectedStyle = c => c.Font.Size = FontUnit.XLarge;
            //Action<WebControl> displaySelectableStyle = c =>
            //{
            //    c.Attributes["onmouseover"] = string.Format("this.style.fontSize = '{0}'", FontUnit.XLarge);
            //    c.Attributes["onmouseout"] = string.Format("this.style.fontSize = '{0}'", FontUnit.Empty);
            //};

            CreateAlphaFilters();

            // alpha paging        
            foreach (KeyValuePair<string, string> filter in alphaFilters)
            {
                Control alphaFilterControl = null;

                //if (this.Highlight && 
                //    filter.Value == this.CurrentFilter)
                //{
                //    Label lblFilter = new Label();
                //    lblFilter.Text = filter.Key;
                //    lblFilter.Font.Bold = true;

                //    displaySelectedStyle(lblFilter);

                //    alphaFilterControl = lblFilter;
                //}
                //else
                //{
                    LinkButton lnkFilter = new LinkButton();
                    lnkFilter.ID = "AlphaFilter_" + filter.Key;
                    lnkFilter.CommandName = ALPHA_FILTER_COMMAND;
                    lnkFilter.CommandArgument = filter.Value;
                    lnkFilter.Text = filter.Key;
                    
                    if (this.Highlight && filter.Value == this.CurrentFilter)
                    {
                        lnkFilter.Font.Bold = true;
                    }

                    //displaySelectableStyle(lnkFilter);

                    alphaFilterControl = lnkFilter;
                //}

                Controls.Add(alphaFilterControl);

                // new line
                Controls.Add(new LiteralControl("<br/>"));
            }
            Controls.Add(new LiteralControl("            </div>\n"));
        }
    }
}
