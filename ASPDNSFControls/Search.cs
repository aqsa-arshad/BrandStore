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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.ComponentModel;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// [DEPRRECATED] Custom control used for search functionality
    /// </summary>
    public class Search : CompositeControl
    {
        #region Variable Declaration

        private Panel _pnlContainer = new Panel();
        private Label _lblSearchCaption = new Label();
        private TextBox _txtSearch = new TextBox();
        private Button _btnDoSearch = new Button();
        private RegularExpressionValidator _valMinLength = new RegularExpressionValidator();
        private ValidationSummary _valSummary = new ValidationSummary();

        private int _minPrefixLength = 3;
        private bool _watchKeypress = false;

        public event EventHandler<CancelEventArgs> SearchInvoking;
        public event EventHandler SearchInvoked;

        private const string SETTINGS_CATEGORY = "ASPDNSF Settings";
        private const string LANDING_PAGE = "LandingPage";
        private const string SEARCH_TERM_KEYWORD = "SearchTermKeyWord";
        private const string SEARCH_TEXT_MIN_LENGTH = "SearchTextMinLength";
        private const string SEARCH_TEXT_INVALID_MIN_LENGTH_ERROR_MESSAGE = "SearchTextInvalidMinLengthErrorMessage";
        private const string VALIDATE_INPUT_LENGTH = "ValidateInputLength";
        private const string USE_LANDING_PAGE_BEHAVIOR = "UseLandingPageBehavior";
        private const string WILL_RENDER_IN_UPDATEPANEL = "WillRenderInUpdatePanel";

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the search control
        /// </summary>
        public Search()
        {
            // don't make this button a submit 
            // so that it won't be the default one fired 
            // when the user presses the enter key
            _btnDoSearch.UseSubmitBehavior = false;
			_btnDoSearch.ValidationGroup = "search";

            this.LandingPage = "search.aspx";
            this.SearchTermKeyWord = "searchterm";
            this.SearchTextMinLength = 3;
            this.SearchTextMinLengthInvalidErrorMessage = "Invalid min char length";

            // explicitly assign ids to the controls
            AssignClientReferenceIDs();

            // default is summary
            this.ShowValidationSummary = true;
            this.ShowValidationMessageBox = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The search button text
        /// </summary>
        [Browsable(true), Category(SETTINGS_CATEGORY), DefaultValue(""), Bindable(true)]
        public string SearchButtonCaption
        {
            get { return _btnDoSearch.Text; }
            set { _btnDoSearch.Text = value; }
        }

        /// <summary>
        /// The search label
        /// </summary>
        [Browsable(true), Category(SETTINGS_CATEGORY), DefaultValue(""), Bindable(true)]
        public string SearchCaption
        {
            get { return _lblSearchCaption.Text; }
            set { _lblSearchCaption.Text = value; }
        }

        /// <summary>
        /// The search text
        /// </summary>
        [Browsable(false)]
        public string SearchText
        {
            get { return _txtSearch.Text; }
            set { _txtSearch.Text = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to watch for keypress event
        /// </summary>
        [Browsable(true), DefaultValue(false)]
        public bool WatchKeyPress
        {
            get { return _watchKeypress; }
            set { _watchKeypress = value; }
        }

        /// <summary>
        /// Minimum length of text before the Search by Keypess initiates
        /// </summary>
        [Browsable(true), DefaultValue(3)]
        public int MinimumPrefixLength
        {
            get { return _minPrefixLength; }
            set { _minPrefixLength = value; }
        }

        /// <summary>
        /// Gets or sets the search button CSS.
        /// </summary>
        /// <value>The search button CSS.</value>
        [Browsable(true)]
        public string SearchButtonCSS
        {
            get { return _btnDoSearch.CssClass; }
            set { _btnDoSearch.CssClass = value; }
        }

        /// <summary>
        /// Gets or sets the landing page for the search query
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public string LandingPage
        {
            get { return null == ViewState[LANDING_PAGE] ? string.Empty : (string)ViewState[LANDING_PAGE]; }
            set
            {
                ViewState[LANDING_PAGE] = value;
            }
        }

        /// <summary>
        /// Gets or sets the querystring name for the search keyword
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public string SearchTermKeyWord
        {
            get { return null == ViewState[SEARCH_TERM_KEYWORD] ? string.Empty : (string)ViewState[SEARCH_TERM_KEYWORD]; }
            set
            {
                ViewState[SEARCH_TERM_KEYWORD] = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum character length that can be used to perform search
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public int SearchTextMinLength
        {
            get
            {
                object savedValue = ViewState[SEARCH_TEXT_MIN_LENGTH];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[SEARCH_TEXT_MIN_LENGTH] = value;
            }
        }

        /// <summary>
        /// Gets or sets the landing page for the search query
        /// </summary>
        [Browsable(true), Category("Appearance")]
        public string SearchTextMinLengthInvalidErrorMessage
        {
            get { return null == ViewState[SEARCH_TEXT_INVALID_MIN_LENGTH_ERROR_MESSAGE] ? string.Empty : (string)ViewState[SEARCH_TEXT_INVALID_MIN_LENGTH_ERROR_MESSAGE]; }
            set
            {
                ViewState[SEARCH_TEXT_INVALID_MIN_LENGTH_ERROR_MESSAGE] = value;
            }
        }
        

        [Browsable(true), Category("Appearance")]
        public bool ValidateInputLength
        {
            get
            {
                object booleanValue = ViewState[VALIDATE_INPUT_LENGTH];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[VALIDATE_INPUT_LENGTH] = value;
                // this property doesn't affect the ui
                //ChildControlsCreated = false;
            }
        }

        public bool ShowValidationSummary
        {
            get { return _valSummary.ShowSummary; }
            set { _valSummary.ShowSummary = value; }
        }

        public bool ShowValidationMessageBox
        {
            get { return _valSummary.ShowMessageBox; }
            set { _valSummary.ShowMessageBox = value; }
        }

        [Browsable(true), Category("Behavior")]
        public bool UseLandingPageBehavior
        {
            get
            {
                object booleanValue = ViewState[USE_LANDING_PAGE_BEHAVIOR];
                // default to true
                if (null == booleanValue) { return true; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[USE_LANDING_PAGE_BEHAVIOR] = value;
                // this property doesn't affect the ui
                //ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category("Appearance")]
        public bool WillRenderInUpdatePanel
        {
            get
            {
                object booleanValue = ViewState[WILL_RENDER_IN_UPDATEPANEL];
                if (null == booleanValue) { return false; }

                return booleanValue is bool && (bool)booleanValue;
            }
            set
            {
                ViewState[WILL_RENDER_IN_UPDATEPANEL] = value;
            }
        }


        #endregion

        #region Methods

        /// <summary>
        /// Sets the id suffix of the contained controls
        /// </summary>
        private void AssignClientReferenceIDs()
        {
            _pnlContainer.ID = "SearchPanel";
            _lblSearchCaption.ID = "SearchCaption";
            _txtSearch.ID = "SearchText";
            _btnDoSearch.ID = "SearchButton";
        }

        /// <summary>
        /// Overrides the OnInit methdo to attach the Click event handler of the button
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            _btnDoSearch.Click += new EventHandler(btnDoSearch_Click);
            base.OnInit(e);
        }

        /// <summary>
        /// The button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDoSearch_Click(object sender, EventArgs e)
        {            
            if (this.UseLandingPageBehavior)
            {
                CancelEventArgs c = new CancelEventArgs();
                OnSearchInvoking(c);

                if (c.Cancel == false)
                {
                    // automatically perform search using a landing page
                    // implementing search logic via search parameter specified
                    // on the query string, default landing page is search.aspx
                    DoSearch();
                }
                else
                {
                    // search wasn't canceled
                    // let's raise the SearchInvoked event
                    // to allow the listener of that event to handle the search keywords independently
                    OnSearchInvoked(EventArgs.Empty);
                }
            }
            else
            {
                // notify the handlers so they can perform search
                OnSearchInvoked(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Perform search
        /// </summary>
        private void DoSearch()
        {
            // use the landing page to redirect the user 
            // and pass the search text as querystring parameter
            string redirectUrl = string.Format("{0}?{1}={2}", this.LandingPage, this.SearchTermKeyWord, HttpUtility.UrlEncode(this.SearchText));
            HttpContext.Current.Response.Redirect("~/" + redirectUrl);
        }

        /// <summary>
        /// Method to raise the SearchInvoking event
        /// </summary>
        /// <param name="e"></param>
        protected void OnSearchInvoking(CancelEventArgs e)
        {
            // check if we have an event handler registered
            if (SearchInvoking != null)
            {
                // raise the event
                SearchInvoking(this, e);
            }
        }

        /// <summary>
        /// Method to raise the SearchInvoked event
        /// </summary>
        /// <param name="e"></param>
        protected void OnSearchInvoked(EventArgs e)
        {
            if (SearchInvoked != null)
            {
                SearchInvoked(this, e);
            }
        }

        /// <summary>
        /// Overrides the TagKey to render the html element container to div instead of the default which is span
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Span;
                //return HtmlTextWriterTag.Div;
            }
        }

        /// <summary>
        /// Creates the child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            _pnlContainer.Controls.Clear();

            Action<Control> addControl = (ctrl) => { _pnlContainer.Controls.Add(ctrl); };

            addControl(_lblSearchCaption);
            addControl(new LiteralControl("&nbsp;"));
            addControl(_txtSearch);
            addControl(new LiteralControl("&nbsp;"));
            addControl(_btnDoSearch);

            if (this.ValidateInputLength && !this.DesignMode)
            {             
                _valMinLength.ControlToValidate = _txtSearch.ID;
                _valMinLength.ErrorMessage = string.Format(this.SearchTextMinLengthInvalidErrorMessage, this.SearchTextMinLength);
                _valMinLength.ValidationExpression = ".{" + this.SearchTextMinLength.ToString() + ",}";
                _valMinLength.EnableClientScript = true;
                _valMinLength.Display = ValidatorDisplay.None; //.Dynamic;
                _valMinLength.CssClass = this.CssClass + "_error";
                _valMinLength.Font.Bold = false;
                _valMinLength.Font.Italic = false;
				_valMinLength.ValidationGroup = "search";

				_valSummary.ValidationGroup = "search";

                addControl(_valMinLength);
                addControl(_valSummary);
            }

            // assign the default button to trigger
            // once any Enter key was pressed inside the area of this container
            // which is usually after typing on the search text and hitting the enter key
            _pnlContainer.DefaultButton = _btnDoSearch.UniqueID;
            Controls.Add(_pnlContainer);           

            base.CreateChildControls();
        }

        public override bool HasControls()
        {
            EnsureChildControls();
            return base.HasControls();
        }

        #endregion

    }
}
