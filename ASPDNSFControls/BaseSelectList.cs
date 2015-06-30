// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data;
using System.Drawing.Design;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Base class providing drop down display for business-logic related UI
    /// </summary>
    public class BaseSelectList : CompositeControl, IPostBackDataHandler
    {
        #region Variable Declaration
        
        private ListItemCollection _datasource = new ListItemCollection();
        private Orientation _orientation = Orientation.Horizontal;
        private const string THISCAPTION = "Caption";
        private const string DEFAULT_SELECTED = "DefaultSelected";
        public event EventHandler<CancelEventArgs> ValueChanging;

        #endregion

        /// <summary>
        /// Gets the current customer
        /// </summary>
        public Customer ThisCustomer
        {
            get { return Customer.Current; }
        }

        /// <summary>
        /// Gets or sets the Caption
        /// </summary>
        /// <value>The table style.</value>
        [Browsable(true), Category("Appearance")]
        public string Caption
        {
            get { return null == ViewState[THISCAPTION] ? string.Empty : (string)ViewState[THISCAPTION]; }
            set
            {
                ViewState[THISCAPTION] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the default selected value
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public string SelectedValue
        {
            get { return null == ViewState[DEFAULT_SELECTED] ? string.Empty : (string)ViewState[DEFAULT_SELECTED]; }
            set
            {
                ViewState[DEFAULT_SELECTED] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the orientation
        /// </summary>
        public Orientation Orientation
        {
            get { return this._orientation; }
            set { _orientation = value; }
        }

        /// <summary>
        /// Gets the datasource
        /// </summary>
        protected ListItemCollection DataSource
        {
            get { return _datasource; }
        }

        /// <summary>
        /// When overridden. Initialize the datasource
        /// </summary>
        /// <param name="datasource"></param>
        protected virtual void InitializeDataSource(ListItemCollection datasource)
        {
        }

        /// <summary>
        /// Overrides the OnInit event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            if (!this.DesignMode)
            {
                InitializeDataSource(_datasource);
            }

            base.OnInit(e);
        }

        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            // render only if valid
            // if design-mode, allow to render dummy html
            // otherwise at runtime, our ShouldRender flag will determine visibility
            if (this.DesignMode || this.ShouldRender)
            {
                writer.WriteBeginTag("div");
                writer.WriteAttribute("id", this.ClientID);
                writer.WriteAttribute("name", this.UniqueID);
                writer.WriteAttribute("class", this.CssClass);
                writer.Write(HtmlTextWriter.TagRightChar);

                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(this.Caption);
                writer.RenderEndTag();

                if (this.DesignMode)
                {
                    writer.Write("<select >");
                    writer.Write("<option >Bound</option>");
                }
                else
                {
                    writer.Write(string.Format("<select id=\"{0}\" name=\"{1}\" onchange=\"javascript:setTimeout('__doPostBack(\\'{1}\\')') \">", this.ClientID, this.UniqueID));
                    RenderInnerContents(writer);
                }
                writer.Write("</select>");
                writer.WriteEndTag("div");
            }
        }

        /// <summary>
        /// When overridden. Gets whether to render this control per business rule
        /// </summary>
        protected virtual bool ShouldRender
        {
            get { return true; }
        }

        /// <summary>
        /// Renders the inner options of the control
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void RenderInnerContents(HtmlTextWriter writer)
        {
            foreach (ListItem item in this.DataSource)
            {
                string selectedFlag = string.Empty;
                if (this.SelectedValue == item.Value)
                {
                    selectedFlag = "selected=\"selected\"";
                }

                writer.Write(string.Format("<option value=\"{0}\" {2}>{1}</option>", item.Value, item.Text, selectedFlag));
            }
        }

        /// <summary>
        /// Reconciles the postback data from the page to check if the value has changed
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns></returns>
        public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            bool thisTriggeredThePostBack = postCollection["__EVENTTARGET"] == this.UniqueID;
            if (thisTriggeredThePostBack && this.DataSource.Count > 0)
            {
                string oldValue = this.SelectedValue;
                string newValue = postCollection[postDataKey];

                this.SelectedValue = newValue;

                return oldValue.Equals(newValue) == false;
            }

            return false;
        }

        
        /// <summary>
        /// Raises the PostBack event routine
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            CancelEventArgs c = new CancelEventArgs();
            OnValueChanging(c);
            if (c.Cancel == false)
            {
                // we'll do the redirection here
                DoPostbackRoutine(this.SelectedValue);
            }
        }

        /// <summary>
        /// Raises the ValueChanging event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnValueChanging(CancelEventArgs e)
        {
            if(ValueChanging != null)
            {
                ValueChanging(this, e);
            }
        }

        /// <summary>
        /// When overridden. Performs the necessary action for this control per postback
        /// </summary>
        /// <param name="selectedValue"></param>
        protected virtual void DoPostbackRoutine(string selectedValue)
        {
        }

        /// <summary>
        /// Refreshes the current page so that the applied changes should take effect
        /// </summary>
        protected void RefreshPage()
        {
            HttpContext ctx = HttpContext.Current;
            ctx.Response.Redirect(ctx.Request.Url.ToString());
        }

    }

}
