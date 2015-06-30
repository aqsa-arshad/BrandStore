// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontControls
{
    /// <summary>
	/// [DEPRECATED] Custom control used to display entities and its sub entities
    /// </summary>
    [ToolboxData("<{0}:EntityControl runat=server></{0}:EntityControl>")]
    public class EntityControl : CompositeControl
    {
        #region Variable Declarations

        private string _Header = string.Empty;
        private Image _imgHeader = new Image();
        private const string THISBULLET = "Bullet";
        private const string ENTITY_TYPE = "EntityType";
        private const string HEADER_TEXT = "HeaderText";
        private const string MAX_MENU_SIZE = "MaxMenuSize";
        private const int ID_NOT_DEFINED = 0;
        private int i = 0;

        #endregion

        #region Constructor

        public EntityControl()
        {
            this.MaxMenuSize = int.MaxValue;
            this.Bullet = ">>";
        }

        #endregion

        #region Properties

        [Browsable(true), Category("Appearance")]
        public string Bullet
        {
            get { return null == ViewState[THISBULLET] ? string.Empty : (string)ViewState[THISBULLET]; }
            set
            {
                ViewState[THISBULLET] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the table style.
        /// </summary>
        /// <value>The table style.</value>
        [Browsable(true), Category("Appearance")]
        public string Header
        {
            get { return null == ViewState[HEADER_TEXT] ? string.Empty : (string)ViewState[HEADER_TEXT]; }
            set
            {
                ViewState[HEADER_TEXT] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the table style.
        /// </summary>
        /// <value>The table style.</value>
        [Browsable(true), Category("Appearance")]
        public string EntityType
        {
            get { return null == ViewState[ENTITY_TYPE] ? string.Empty : (string)ViewState[ENTITY_TYPE]; }
            set
            {
                ViewState[ENTITY_TYPE] = value;
                ChildControlsCreated = false;
            }
        }

        /// <summary>
        /// Gets or sets the image header.
        /// </summary>
        /// <value>The image header.</value>
        [Browsable(true), Category("Appearance"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ImageHeader
        {
            get { return _imgHeader.ImageUrl; }
            set
            {
                _imgHeader.ImageUrl = value;
                ChildControlsCreated = false;
            }
        }

        [Browsable(true), Category("Appearance")]
        public int MaxMenuSize
        {
            get
            {
                object savedValue = ViewState[MAX_MENU_SIZE];

                if (null == savedValue || !(savedValue is int))
                {
                    return int.MaxValue;
                }

                return (int)savedValue;
            }
            set
            {
                ViewState[MAX_MENU_SIZE] = value;
            }
        }

        private LinkItemCollection m_datasource;
        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public LinkItemCollection DataSource
        {
            get { return m_datasource; }
            set { m_datasource = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl"/> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (this.DesignMode)
            {
                writer.Write("<div class=\"navHeader\">");
                writer.Write("Browse [" + this.Header + "]");
                writer.Write("</div>");
                writer.Write("<div class=\"leftNav\">");
                writer.Write("[Sample " + this.EntityType + " 1]<br/>");
                writer.Write("[Sample " + this.EntityType + " 2]<br/>");
                writer.Write("[Sample " + this.EntityType + " 3]<br/>");
                writer.Write("&nbsp;&nbsp;<span class=\"catMark\">" + this.Bullet + "</span>&nbsp;&nbsp;[Sample Sub " + this.EntityType + "  3-1]<br/>");
                writer.Write("&nbsp;&nbsp;<span class=\"catMark\">" + this.Bullet + "</span>&nbsp;&nbsp;[Sample Sub " + this.EntityType + " 3-2]<br/>");
                writer.Write("&nbsp;&nbsp;<span class=\"catMark\">" + this.Bullet + "</span>&nbsp;&nbsp;[Sample Sub " + this.EntityType + " 3-3]<br/>");
                writer.Write("[Sample Entity 4]<br/>");
                writer.Write("</div>");
            }
            else
            {
                if (this.DataSource != null)
                {
                    writer.Write("<div class=\"navHeader\">");
                    writer.Write(AppLogic.GetString("common.browse", Customer.Current.LocaleSetting) + " " + this.Header);
                    writer.Write("</div>");
                    writer.Write("<div class=\"leftNav\" id=\"" + this.Header + "\">");
                    RenderList(this.DataSource, writer, false, this.EntityType);
                    writer.Write("</div>");
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Renders the list.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="IsChild">if set to <c>true</c> [is child].</param>
        private void RenderList(LinkItemCollection nodes, HtmlTextWriter writer, bool IsChild, string Type)
        {
            int maxSize = this.MaxMenuSize;
            LinkItemCollection NewNodes = new LinkItemCollection();
            bool AllowEntityFiltering = AppLogic.GlobalConfigBool("AllowEntityfiltering");
            if (AllowEntityFiltering)
            {
                foreach (ILinkItem LI in nodes)
                {
                    if (!Entity.ExistsInEntityStore(AppLogic.StoreID(), LI.ID, Type))
                    { }
                    else
                    { NewNodes.Add(LI); }
                }
            }
            else
            {
                foreach (ILinkItem LI in nodes)
                {
                     NewNodes.Add(LI);
                }
            }

            writer.Write("<ul class=\"tame\">");
            for (int ctr = 0; ctr < NewNodes.Count; ctr++)
            {
                if (ctr == maxSize) break;

                ProductMappingLinkItem node = NewNodes[ctr] as ProductMappingLinkItem;
                RenderItem(node, writer, IsChild);
            }
            writer.Write("</ul>");
        }

        /// <summary>
        /// Renders the item.
        /// </summary>
        /// <param name="currentNode">The current node.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="IsChild">if set to <c>true</c> [is child].</param>
        private void RenderItem(ProductMappingLinkItem currentNode, HtmlTextWriter writer, bool IsChild)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            if (IsChild)
            {
                int j = 0;
                while (j < i)
                {
                    writer.Write("&nbsp;&nbsp;");
                    j++;
                }
                writer.Write("<span class=\"catMark\">" + this.Bullet + "</span>");
                writer.Write("&nbsp;&nbsp;");
            }
            HyperLink lnk = new HyperLink();
            lnk.Text = XmlCommon.GetLocaleEntry(currentNode.Name, Customer.Current.LocaleSetting, false);
            lnk.NavigateUrl = currentNode.Url;
            if (currentNode.Selected)
            {
                lnk.Style.Add("font-weight", "bold");
            }
            lnk.RenderControl(writer);

            if (currentNode.ChildItems.Count > 0)
            {
                i++;
                RenderList(currentNode.ChildItems, writer, true, this.EntityType);
                i--;
            }

            writer.RenderEndTag();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (string.IsNullOrEmpty(this.EntityType))
            {
                throw new ArgumentNullException("Entity Type not specified!!!");
            }

            InitializeDataSource();

            base.OnLoad(e);
        }

        private void InitializeDataSource()
        {
            // check the current entity if viewing on an entity page
            int id = ID_NOT_DEFINED;

            if (HasDefinedInQueryString(out id) ||
                HasDefinedInProfile(out id))
            {
                ProductMappingLinkItem selectedEntity = ProductMappingLinkItem.Find(id, this.EntityType);
                if (selectedEntity != null)
                {
                    selectedEntity.LoadChildren();
                    this.DataSource = LinkItemCollection.BuildFrom(selectedEntity, selectedEntity.Type);
                    return;
                }
            }

            // by default load the root level entities
            this.DataSource = LinkItemCollection.GetAllFirstLevel(this.EntityType, this.MaxMenuSize, Customer.Current.LocaleSetting);
        }

        private bool HasDefinedInQueryString(out int id)
        {
            id = ID_NOT_DEFINED;

            // check the current entity if viewing on an entity page
            id = CommonLogic.QueryStringUSInt(this.EntityType + "id");
            return id != ID_NOT_DEFINED;
        }

        private bool HasDefinedInProfile(out int id)
        {
            id = ID_NOT_DEFINED;

            if (HttpContext.Current.Profile != null &&
                "showproduct.aspx".EqualsIgnoreCase(CommonLogic.GetThisPageName(false)))
            {
                object objLastViewedInstanceId = HttpContext.Current.Profile.GetPropertyValue("LastViewedEntityInstanceID");
                object objLastViewedEntity = HttpContext.Current.Profile.GetPropertyValue("LastViewedEntityName");

                return objLastViewedInstanceId != null &&
                    objLastViewedEntity != null &&
                    objLastViewedInstanceId is string &&
                    objLastViewedEntity is string &&
                    this.EntityType.EqualsIgnoreCase((string)objLastViewedEntity) &&
                    int.TryParse((string)objLastViewedInstanceId, out id);
            }

            return false;
        }


        #endregion
    }
}
