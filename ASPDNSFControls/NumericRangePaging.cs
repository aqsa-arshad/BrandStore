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
    /// Custom control that renders numeric range paging. i.e. Showing 1-x of N
    /// </summary>
    public class NumericRangePaging : CompositeControl
    {
        private const string ALL_COUNT = "AllCount";        
        private const string START_COUNT = "StartCount";
        private const string END_COUNT = "EndCount";

        [Browsable(true)]
        public int AllCount
        {
            get
            {
                object intValue = ViewState[ALL_COUNT];
                if (null == intValue) { return 0; }

                return (int)intValue;
            }
            set
            {
                ViewState[ALL_COUNT] = value;

                ChildControlsCreated = false;
            }
        }



        [Browsable(true)]
        public int StartCount
        {
            get
            {
                object intValue = ViewState[START_COUNT];
                if (null == intValue) { return 0; }

                return (int)intValue;
            }
            set
            {
                ViewState[START_COUNT] = value;

                ChildControlsCreated = false;
            }
        }

        [Browsable(true)]
        public int EndCount
        {
            get
            {
                object intValue = ViewState[END_COUNT];
                if (null == intValue) { return 0; }

                return (int)intValue;
            }
            set
            {
                ViewState[END_COUNT] = value;

                ChildControlsCreated = false;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div ;
            }
        }

        protected override void CreateChildControls()
        {
            // page info
            if (this.AllCount > 0)
            {
                int displayEndCount = this.EndCount;

                if (this.EndCount > this.AllCount)
                {
                    displayEndCount = this.AllCount;
                }

                string pageText = string.Format("Showing {0}-{1} of {2}", this.StartCount, displayEndCount, this.AllCount);
                Controls.Add(new LiteralControl(pageText));
            }
            else
            {
                string pageText = string.Format("Showing {0} of {0}", "0");
                Controls.Add(new LiteralControl(pageText));
            }

            base.CreateChildControls();
        }
    }

}
