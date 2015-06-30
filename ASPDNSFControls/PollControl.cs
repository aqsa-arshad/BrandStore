// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Custom control used for polls
    /// </summary>
    [ToolboxData("<{0}:PollControl runat=server></{0}:PollControl>")]
    public class PollControl : CompositeControl
    {
        #region private properties

        private string _headerBGColor = string.Empty;
        private string _pollClass = string.Empty;
        private string _pollTitle = string.Empty;      
        private int _pollID = 0;
      
        #endregion
        
        private Poll _dataSource = null;
        private Image _headerImage = new Image();
        private RadioButtonList _pollRadio = new RadioButtonList();
        private Button _pollVoteButton = new Button();

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PollControl"/> class.
        /// </summary>
        public PollControl()
        {
        }

        #endregion

        public event EventHandler PollVoteButtonClick;
        
        #region Public Properties

        /// <summary>
        /// Gets or sets the color of the header BG.
        /// </summary>
        /// <value>The color of the header BG.</value>
        public string HeaderBGColor
        {
            get { return this._headerBGColor; }
            set { this._headerBGColor = value; }
        }

        /// <summary>
        /// Gets or sets the button CSS class.
        /// </summary>
        /// <value>The button CSS class.</value>
        public string ButtonCssClass
        {
            get { return this._pollVoteButton.CssClass; }
            set { this._pollVoteButton.CssClass = value; }
        }

        /// <summary>
        /// Gets or sets the header image.
        /// </summary>
        /// <value>The header image.</value>
        [Browsable(true), Category("POLL_IMAGE_URL"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string HeaderImage
        {
            get { return this._headerImage.ImageUrl; }
            set { this._headerImage.ImageUrl = value; }
        }

        /// <summary>
        /// Gets or sets the poll ID.
        /// </summary>
        /// <value>The poll ID.</value>
        public int PollID
        {
            get { return this._pollID; }
            set { this._pollID = value; }
        }

        /// <summary>
        /// Gets or sets the poll class.
        /// </summary>
        /// <value>The poll class.</value>
        public string PollClass
        {
            get { return this._pollClass; }
            set { this._pollClass = value; }
        }

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public Poll DataSource
        {
            get
            {
                return this._dataSource;
            }
            
            set
            {
                this._dataSource = value;
                ChildControlsCreated = false;
            }        
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>The selected value.</value>
        public string SelectedValue
        {
            get { return this._pollRadio.SelectedValue; }
            set { this._pollRadio.SelectedValue = value; }
        }

        /// <summary>
        /// Gets or sets the poll title.
        /// </summary>
        /// <value>The poll title.</value>
        public string PollTitle
        {
            get { return this._pollTitle; }
            set { this._pollTitle = value; }
        }

        /// <summary>
        /// Gets or sets the poll button text.
        /// </summary>
        /// <value>The poll button text.</value>
        public string PollButtonText
        {
            get { return this._pollVoteButton.Text; }
            set { this._pollVoteButton.Text = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            if (this.DataSource != null)
            {
                // Assign pollanswer in radio button
                this.AssignDataContent(this.DataSource);

                // Create table
                this.Controls.Add(new LiteralControl("<table width=\"100%\" cellpadding=\"2\" cellspacing=\"0\" border=\"0\" style=\"border-style: solid; border-width: 0px; border-color: #" + this._headerBGColor + "\">"));
                this.Controls.Add(new LiteralControl("<tr><td align=\"left\" valign=\"top\">"));
                
                // Header Image
                if (this._headerImage.ImageUrl.Length != 0)
                {
                    this.Controls.Add(this._headerImage);
                }
                
                // Create table
                this.Controls.Add(new LiteralControl("<table width=\"100%\" cellpadding=\"4\" cellspacing=\"0\" border=\"1\" style=\"" + this._pollClass + "\">"));
                this.Controls.Add(new LiteralControl("<tr><td align=\"left\" valign=\"top\">"));
                this.Controls.Add(new LiteralControl("<span class=\"PollTitle\">"));
                this.Controls.Add(new LiteralControl(this._pollTitle + CommonLogic.IIF(this.DataSource.IsExpired(), " " + AppLogic.GetString("poll.cs.1", this.DataSource.SkinID, Thread.CurrentThread.CurrentUICulture.Name), string.Empty)));
                this.Controls.Add(new LiteralControl("</span>"));
                
                // Customer has already voted
                if (this.DataSource.HasCustomerVoted) 
                {
                    this.Controls.Add(new LiteralControl("<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">"));
                   
                    foreach (PollAnswer pa in this.DataSource.pollAnswers)
                    {
                        this.Controls.Add(new LiteralControl("<tr>"));
                        this.Controls.Add(new LiteralControl("<td width=\"40%\" align=\"right\" valign=\"middle\"><span class=\"PollAnswer\">"));
                        this.Controls.Add(new LiteralControl(pa.Value));
                        this.Controls.Add(new LiteralControl(":&nbsp;</span></td>"));
                        this.Controls.Add(new LiteralControl("<td width=\"60%\" align=\"left\" valign=\"middle\">"));
                        this.Controls.Add(new LiteralControl("<img src=\"" + AppLogic.LocateImageURL("App_Themes/skin_" + this.DataSource.SkinID.ToString() + "/images/pollimage.gif") + "\" align=\"absmiddle\" width=\"" + (pa.ThisPercent * 0.9).ToString() + "%\" height=\"10\" border=\"0\"><span class=\"PollAnswer\"> (" + pa.ThisPercent.ToString() + "%)"));
                        this.Controls.Add(new LiteralControl("</span></td>"));
                        this.Controls.Add(new LiteralControl("</tr>"));
                        this.Controls.Add(new LiteralControl("<tr><td colspan=\"2\"><img src=\"App_Themes/skin_" + this.DataSource.SkinID.ToString() + "/images/spacer.gif\" width=\"100%\" height=\"2\"></td></tr>"));                         
                    }

                    this.Controls.Add(new LiteralControl("</table>"));
                    this.Controls.Add(new LiteralControl("  <div align=\"center\"><span class=\"PollLink\">" + AppLogic.GetString("poll.cs.2", this.DataSource.SkinID, Thread.CurrentThread.CurrentUICulture.Name) + " " + this.DataSource.NumVotes().ToString() + "</span></div>"));
                }
                else 
                {
                    this.Controls.Add(new LiteralControl("<span class=\"PollAnswer\">"));
                    this.Controls.Add(this._pollRadio);
                    this.Controls.Add(new LiteralControl("</span>"));
                    this.Controls.Add(new LiteralControl("<div align=\"center\">"));
                   
                    // Vote button
                    if (this._pollVoteButton.Text != string.Empty)
                    {
                        this.Controls.Add(this._pollVoteButton);
                    }

                    this.Controls.Add(new LiteralControl("</div>"));
                }

                this.Controls.Add(new LiteralControl("</td></tr>"));
                this.Controls.Add(new LiteralControl("</table>"));
                this.Controls.Add(new LiteralControl("</td></tr>"));
                this.Controls.Add(new LiteralControl("</table>"));
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            this._pollVoteButton.Click += new EventHandler(this._pollVoteButton_Click);
            base.OnInit(e);
        }

        /// <summary>
        /// Raises the <see cref="E:PollVoteButtonClick"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnPollVoteButtonClick(EventArgs e)
        {
            if (this.PollVoteButtonClick != null)
            {
                // raise the event
                this.PollVoteButtonClick(this, e);
            }
        }

        /// <summary>
        /// Assigns the content of the data.
        /// </summary>
        /// <param name="pollObject">The poll object.</param>
        private void AssignDataContent(Poll pollObject)
        {
            this._pollID = pollObject.PollID;
            this._pollTitle = pollObject.Name;

            foreach (PollAnswer pa in pollObject.pollAnswers)
            {
                this._pollRadio.Items.Add(new ListItem(pa.Value, pa.ID.ToString()));
            }
        }

        /// <summary>
        /// Handles the Click event of the _pollVoteButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void _pollVoteButton_Click(object sender, EventArgs e)
        {
            this.OnPollVoteButtonClick(EventArgs.Empty);
        }

        #endregion
    }
}
