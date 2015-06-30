// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.Design.WebControls;
using System.Web.UI.WebControls;
using System.Linq;
using System.Text;


namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Extends design-time behavior for controls.
    /// </summary>
    public class ShippingMethodControlDesigner : CompositeControlDesigner
    {
        private DesignerActionListCollection m_ActionList;

        /// <summary>
        /// Overridden. Initializes the designer with the specified IComponent object. 
        /// </summary>
        /// <param name="component">The IComponent, which is the control associated with this designer.</param>
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            SetViewFlags(ViewFlags.TemplateEditing, true);
        }

        /// <summary>
        /// Gets the action list collection for the control designer.(Inherited from ControlDesigner.)
        /// </summary>
        public override DesignerActionListCollection ActionLists
        {
            get
            {                
                m_ActionList = new DesignerActionListCollection();
                ShippingMethodControl ctrlShippingMethod = (ShippingMethodControl)Component;
                m_ActionList.Add(new ShippingMethodControlActionList(ctrlShippingMethod));
               
                return m_ActionList;
            }
        }

        /// <summary>
        /// Gets the collection of predefined automatic formatting schemes to display in the Auto Format dialog box for the associated control at design time. (Inherited from ControlDesigner.)
        /// </summary>
        public override DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                DesignerAutoFormatCollection m_autoformat = new DesignerAutoFormatCollection();
                m_autoformat.Add(new NoFormat());

                return m_autoformat;
            }
        }
    }

    /// <summary>
    /// A designer autoformat object that defines no format 
    /// </summary>
    public class NoFormat : DesignerAutoFormat
    {
        public NoFormat() : base("No Format") { }

        /// <summary>
        /// Applies the associated formatting to the specified control. 
        /// </summary>
        /// <param name="control"></param>
        public override void Apply(Control control)
        {
            ShippingMethodControl ctrlShippingMethod = (ShippingMethodControl)control;

        }

    }


}
