// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Custom control used to handle address address information
    /// </summary>
    [ToolboxData("<{0}:Token runat=server></{0}:Token>")]
    public class Token : Literal
    { 

        #region Variable Declaration

        public enum TokenType
        {
            GOOGLE_ECOM_TRACKING_V2 = 0
            , COUNTRYDIVVISIBILITY
            , COUNTRYSELECTLIST
            , CURRENCYDIVVISIBILITY
            , CURRENCYSELECTLIST
            , VATDIVVISIBILITY
            , VATSELECTLIST
            , PAGEINFO
            , USERNAME
            , SIGNINOUT_TEXT
            , SIGNINOUT_LINK
            , METATITLE
            , METADESCRIPTION
            , METAKEYWORDS
            , CURRENCY_LOCALE_ROBOTS_TAG
            , CARTPROMPT
            , SKINID
            , NUM_CART_ITEMS
            , STRINGRESOURCE
            , STRINGRESOURCEFORMAT
            , APPCONFIG
            , APPCONFIGUSINT
            , APPCONFIGBOOL
            , TOPIC
            , TOPICTITLE
            , TOPICLINK
            , USER_MENU_NAME
            , STRINGFORMAT
            , XMLPACKAGE
            , CUSTOMERID
            , SKINIMAGEDIR
            , SKINIMAGE
            , ADMINLINK
            , BuySafeSeal
            , BongoExtend
            , EMPTY
        }
        public enum DisplayType
        {
            Static = 0
            , Dynamic
        }

        private DisplayType? display;
        private TokenType type;
        public TokenType Type
        {
            get
            {
                return (TokenType)type;
            }
            set
            {
                type = value;
            }
        }
        public string Expression { get; set; }
        public DisplayType Display
        {
            get
            {
                if (display == null)
                    display = DisplayType.Static;
                return (DisplayType)display;
            }
            set
            {
                display = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Account"/> class.
        /// </summary>
        public Token() { }

        #endregion

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            LoadControl();
            if (Type == TokenType.NUM_CART_ITEMS)
                Display = DisplayType.Dynamic;
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Display == DisplayType.Dynamic)
            {
                LoadControl();
            }
        }

        public void LoadControl()
        {
            string results = "";
            object evalResults;

            if (Expression == null)
                Expression = "";

            //Support entering in the whole token string into expression rather than using type + expression
            if (Type == TokenType.EMPTY && Expression.Length > 0)
            {                
                Expression = Expression.Replace("Tokens:", "").Trim();
                string[] strAry = Expression.Split(',');

                if(strAry != null && strAry.Length > 0)
                {
                    foreach (TokenType tType in Enum.GetValues(typeof(TokenType)))
                    {
                        if (tType.ToString().ToUpper() == strAry[0].ToUpper())
                        {
                            Type = tType;
                            Expression = Expression.Replace(strAry[0], "").TrimStart(',').Trim();
                        }
                    }
                }
            }

            //if we have no type then don't display anything
            if (Type != TokenType.EMPTY)
            {

                Expression = Type.ToString() + (string.IsNullOrEmpty(Expression) ? "" : ", " + Expression.Trim(','));

                Tokens token = new Tokens();

                evalResults = Tokens.GetEvalData(Expression, typeof(Token), "Text");
                results = evalResults as string;

                if (string.IsNullOrEmpty(results))
                    results = evalResults.ToString();
            }

            Text = results; 
        }

        #endregion

    }

}
