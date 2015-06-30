// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using AspDotNetStorefrontCore;
using ASPDNSFRadWrapper.Telerik.Web.UI;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefrontLayout
{
//    public abstract class PanelControlBase<contentType> : UserControl where contentType:Control
//    {
//        public PanelControlBase()
//        {
//            Index = -1;
//        }
//        protected override void OnInit(EventArgs e)
//        {
//            if (EditVisible)
//            {
//                BuildHeader();
//                object x = popupWindow;

//                if (! Page.ClientScript.IsStartupScriptRegistered(this.GetType(), "OverlayScript"))
//                {
//                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "OverlayScript", OverlayScriptText);
//                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "RadWindowScript", RadWindowScript);
//                this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID,

//                    string.Format(
//                    @"
//                    <script>
//                        posElement(document.getElementById('{0}'), document.getElementById('Edit_{1}'));
//                    </script>"
//                    , ClientControlID, ClientID)
                    
//                    );
//                }
//                /*
//                this.Page.RegisterStartupScript("OverlayScript",
//                    OverlayScriptText);

//                this.Page.RegisterStartupScript(
//                    "RadWindowScript",
//                    RadWindowScript);

//                this.Page.RegisterStartupScript(
//                    this.ClientID,
//                    string.Format(
//                    @"
//                    <script>
//                        posElement(document.getElementById('{0}'), document.getElementById('Edit_{1}'));
//                    </script>"
//                    , ClientControlID, ClientID));
                
//                */
//            }
//            base.OnInit(e);
//        }
//        protected virtual void Page_Load(object sender, EventArgs e)
//        {
            
//            if (Request["__EVENTARGUMENT"] == this.ClientID + "Save" && _editSaved != null)
//            {
//                //Save();
//                _editSaved(this, new EventArgs());
//            }
//            else if (Request["__EVENTARGUMENT"] == this.ClientID + "Cancel" && _editCanceled != null)
//                _editCanceled(this, new EventArgs());
//        }
//        protected void Page_UnLoad(object sender, EventArgs e)
//        {
            
//        }

//        private void BuildHeader() 
//        {
//            Panel hdrDiv = new Panel();
//            Table tbl = new Table();
//            tbl.Rows.Add(new TableRow());
//            tbl.Rows[0].Cells.Add(new TableCell());
//            tbl.Rows[0].Cells[0].Controls.Add(hdrDiv);

//            hdrDiv.Attributes.Add("class", "EditButton");
//            //hdrDiv.Attributes.Add("style", "border-style:outset;");
//            hdrDiv.Attributes.Add("id", string.Format("Edit_{0}", this.ClientID));
//            hdrDiv.Attributes.Add("onmouseover", "header_OnMouse(this, true);");
//            hdrDiv.Attributes.Add("onmouseout", "header_OnMouse(this,false);");
//            HeaderPanel.Controls.Add(tbl);

//            LinkButton cmdEdit = new LinkButton();
//            cmdEdit.ID = "cmdEdit";
//            cmdEdit.CausesValidation = false;
//            cmdEdit.CommandName = string.Empty;
//            cmdEdit.Text = "Edit";
//            hdrDiv.Controls.Add(cmdEdit);


//        }

//        #region Javascript
//        private string OverlayScriptText
//        {
//            get
//            {
//                return @"
//	<script>
//    function header_OnMouse(object, active)
//    {
//        if (object.timer != null)
//            clearTimeout(object.timer);
//        if (active)
//        {
//            FadeIn(object, 100);
//        }
//        else
//        {
//            FadeOut(object, 40);
//        }
//    }
//    
//    function FadeIn(object, max)
//    {
//        var curOp = getOpacity(object);
//        if (curOp < max)
//        {
//           setOpacity(object, curOp + 10);
//            var recFadeIn = 'FadeIn(document.getElementById(\'' + object.id + '\'), ' + max + ');';
//           object.timer=setTimeout(recFadeIn, 100);
//        }
//    }
//    function FadeOut(object, min)
//    {
//        var curOp = getOpacity(object);
//        if (curOp > min)
//        {
//           setOpacity(object, curOp - 10);
//            var recFadeIn = 'FadeOut(document.getElementById(\'' + object.id + '\'), ' + min + ');';
//           object.timer=setTimeout(recFadeIn, 100);
//        }
//    }
//
//    function getOpacity(object)
//    {
//        if (object.style.opacity != null)
//            return object.style.opacity * 100;
//        else
//            return object.filters.alpha.opacity;
//    }
//    function setOpacity(object, op)
//    {
//        try{object.style.opacity=op / 100.0 ;}  catch(err){}
//        try{object.filters.alpha.opacity=op;}   catch(err){}
//    }
//
//    function posElement(underlay, overlay)
//    {
//        try{
//	        overlay.style.top = '' + (getXY(underlay).y + 20) + 'px';
//            overlay.style.left= '' + (getXY(underlay).x + 20) + 'px';
//            
//        }
//        catch(err)
//        {
//            alert(err.message);
//        }
//
//    }
//
//	var getXY = function (obj) {
//		var left, top;
//		top = 0;
//		left= 0;
//		if (obj.offsetParent) {
//			do {
//				left += obj.offsetLeft;
//				top  += obj.offsetTop;
//			} while (obj = obj.offsetParent);
//		}
//		return {
//			x : left,
//			y : top
//		};
//	};
//	</script>
//	";
//            }
//        }
//        protected string RadWindowScript
//        {
//            get
//            {
//                return @"
//                <script>
//                    function GetRadWindow() 
//                    {
//                        var oWindow = null;
//                        if (window.radWindow)
//                            oWindow = window.radWindow;
//                        else if (window.frameElement.radWindow)
//                            oWindow = window.frameElement.radWindow;
//                        return oWindow;
//                    }
//                    function clientClose(sender, args) {
//                        if (sender.argument == null)
//                        {
//                            !CancelCallback!
//                        }
//                        else
//                        {
//                            !SaveCallback!
//                        }
//                    }
//                </script>"
//                    .Replace(
//                     "!CancelCallback!", this.Page.ClientScript.GetPostBackEventReference(this, this.ClientID + "Cancel"))
//                    .Replace(
//                     "!SaveCallback!", this.Page.ClientScript.GetPostBackEventReference(this, this.ClientID + "Save"))
//                    .Replace(
//                     "!RadWinCliID!", popupWindow.ClientID);
//            }
//        }
//        #endregion

//        #region Properties
        
//        public int Index
//        {get;set;}
//        private bool EditVisible
//        {
//            get { return true; }
//        }
//        protected virtual Panel HeaderPanel
//        {
//            get
//            {
//                return (Panel)FindControl("pnlEditHeader");
//            }
//        }
//        protected abstract string ClientControlID
//        { get; }
//        protected virtual string PopupOpenerID
//        {
//            get
//            {
//                return FindControl("cmdEdit").ClientID;
//            }
//        }
//        protected abstract string EditorPage
//        { get; }
//        protected abstract int WinHeight
//        { get; }
//        protected abstract int WinWidth
//        { get; }
//        protected contentType ContentControl
//        {
//            get
//            {
//                return (contentType)FindControl("ctrlContent");
//            }
//        }

//        private RadWindow _radWin;
//        private RadWindow popupWindow
//        {
//            get
//            {
//                if (_radWin == null)
//                {
//                    _radWin = (RadWindow)FindControl("radWindow");
//                    _radWin.VisibleOnPageLoad = false;
//                     _radWin.ShowContentDuringLoad=false;
//                     _radWin.NavigateUrl = EditorPage;
//                    _radWin.VisibleStatusbar = false;
//                    _radWin.Behaviors = WindowBehaviors.Close | WindowBehaviors.Move | WindowBehaviors.Resize;
//                    _radWin.OpenerElementID = PopupOpenerID;
//                    _radWin.Height= new Unit(string.Format("{0}px", WinHeight));
//                    _radWin.Width = new Unit(string.Format("{0}px", WinWidth));
                    
//                }
//                return _radWin;
//            }
//        }

//        #endregion

//        #region events
//        private event EventHandler _editSaved;
//        private event EventHandler _editCanceled;

//        public event EventHandler EditSaved
//        {
//            add
//            {
//                _editSaved += value;
//            }
//            remove
//            {
//                if (_editSaved != null)
//                    _editSaved -= value;
//            }
//        }
//        public event EventHandler EditCanceled
//        {
//            add
//            {
//                _editCanceled += value;
//            }
//            remove
//            {
//                if (_editCanceled != null)
//                    _editCanceled -= value;
//            }
//        }

//        #endregion

//    }
  
//    public class TextPanel : PanelControlBase<Literal>
//    {
//        protected override string EditorPage
//        {
//            get { return this.ResolveUrl("~/TextEdit.aspx"); }
//        }
//        protected override int WinHeight
//        {
//            get { return 200; }
//        }
//        protected override int WinWidth
//        {
//            get { return 200; }
//        }
//        public string CssClass
//        { get; set; }
//        public string CssClassTag
//        {
//            get
//            {
//                return "class='" + CssClass + "'";
//            }
//        }
//        public string TagType
//        { get; set; }

//        public TextPanelData DataSource
//        { get; set; }
//        public sealed override void DataBind()
//        {
//            ContentControl.Text = string.Format("<{0} {1} id='{2}'>{3}</{0}>",
//                new object[]{
//                TagType,
//                CssClassTag,
//                ClientControlID,
//                DataSource
//                }
//                );
//            HeaderPanel.Attributes.Add(CssClassTag.Split('=')[0], CssClassTag.Split('=')[1]);
//            base.DataBind();
//        }


//        protected override string ClientControlID
//        {
//            get { return this.ClientID; }
//        }
//    }

//    public class ImagePanel : PanelControlBase<Image>
//    {
//        protected override void OnInit(EventArgs e)
//        {
//            if (DataSource != null)
//                DataBind();
//            base.OnInit(e);
//        }
//        public sealed override void DataBind()
//        {
//            ContentControl.ImageUrl = DataSource.ImageSource;
//            ContentControl.Attributes.Add("id", ClientControlID);
//            ContentControl.Height= new Unit(this.Height);
//            ContentControl.Width = new Unit(this.Width);
//            base.DataBind();
//        }
//        public ImagePanelData DataSource
//        { get; set; }
       

//        protected override string EditorPage
//        {
//            get { return this.ResolveUrl("~/ImageEdit.aspx"); }
//        }
//        protected override int WinHeight
//        {
//            get { return 200; }
//        }
//        protected override int WinWidth
//        {
//            get { return 200; }
//        }
//        public string Height
//        { get; set; }
//        public string Width
//        { get; set; }

//        protected override string ClientControlID
//        {
//            get { return this.ContentControl.ClientID; }
//        }
//    }
    
//    public class LayoutPage : Page
//    {
//        protected override sealed void OnInit(EventArgs e)
//        {            
//            this.DataSource = new LayoutDataSource();
//            DataBind();
//            LayoutPanel.Controls.Add(LayoutObjects);
//            base.OnInit(e);
//        }

//        protected LayoutDataSource DataSource
//        {get;set;}
        
//        protected int ContextID
//        {
//            get
//            {
//                return CommonLogic.QueryStringNativeInt("Context");
//            }
//        }
//        protected string LayoutName
//        {
//            get
//            {
//                return CommonLogic.QueryStringCanBeDangerousContent("Layout");
//            }
//        }

//        List<ImagePanel> _ImagePanels;
        
//        protected List<ImagePanel> ImagePanels
//        {
//            get
//            {
//                if (_ImagePanels != null)
//                    return _ImagePanels;
//                _ImagePanels = new List<ImagePanel>();
//                foreach (Control item in LayoutObjects.Controls)
//                {
//                    if (item.GetType().IsSubclassOf(typeof(ImagePanel)))
//                        _ImagePanels.Add((ImagePanel)item);
//                }
//                return _ImagePanels;
//            }
//        }
//        List<TextPanel> _textPanels;
        
//        protected List<TextPanel> TextPanels
//        {
//            get
//            {
//                if (_textPanels != null)
//                    return _textPanels;
//                _textPanels = new List<TextPanel>();
//                foreach (Control item in LayoutObjects.Controls)
//                {
//                    if (item.GetType().IsSubclassOf(typeof(TextPanel)))
//                        _textPanels.Add((TextPanel)item);
//                }
//                return _textPanels;
//            }
//        }
        
//        private Control LayoutPanel
//        {
//            get
//            {
//                return FindControl("pnlLayout");
//            }
//        }

//        private Control _LayoutObjects;
//        private Control LayoutObjects
//        {
//            get
//            {
//                if (_LayoutObjects != null)
//                    return _LayoutObjects;
//                SqlCommand xCmd = new SqlCommand(
//                    "SELECT ControlMarkup FROM Layout WHERE [Name] = @Name"
//                    );
//                xCmd.Parameters.Add(new SqlParameter("@Name", LayoutName));
//                using (SqlConnection xcon = new SqlConnection(DB.GetDBConn()))
//                {
//                    xCmd.Connection = xcon;
//                    xcon.Open();
//                    _LayoutObjects = ParseControl((string)xCmd.ExecuteScalar());
//                }
//                return _LayoutObjects;
//            }
//        }

//        #region Databinding
//        private void BindImages()
//        {
//            foreach (ImagePanel panel in ImagePanels)
//            {
//                if (panel.ID != null && DataSource.HasData(DataType.ImagePanelData, panel.ID))
//                {
//                    panel.DataSource = DataSource.ImageData(panel.ID);
//                    panel.DataBind();
//                    continue;
//                }
//                if (panel.Index > -1 && DataSource.HasData(DataType.ImagePanelData, panel.Index))
//                {
//                    panel.DataSource = DataSource.ImageData(panel.Index);
//                    panel.DataBind();
//                    continue;
//                }

//            }
//        }
//        private void BindText()
//        {
//            foreach (TextPanel panel in TextPanels)
//            {
//                if (panel.ID != null && DataSource.HasData(DataType.TextPanelData, panel.ID))
//                {
//                    panel.DataSource = DataSource.TextData(panel.ID);
//                    panel.DataBind();
//                    continue;
//                }
//                if (panel.Index > -1 && DataSource.HasData(DataType.TextPanelData, panel.Index))
//                {
//                    panel.DataSource = DataSource.TextData(panel.Index);
//                    panel.DataBind();
//                    continue;
//                }

//            }
//        }
        
//        public override sealed void DataBind()
//        {
//            BindImages();
//            BindText();
//            base.DataBind();
//        }
//        #endregion
//    }
}
