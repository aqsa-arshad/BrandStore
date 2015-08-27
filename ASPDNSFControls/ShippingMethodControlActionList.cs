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
using System.ComponentModel.Design;

namespace AspDotNetStorefrontControls
{
    /// <summary>
    /// Defines a list of items used to create a smart tag panel
    /// </summary>
    class ShippingMethodControlActionList : DesignerActionList
    {
        #region Constructor

        private ShippingMethodControl ctrlShippingMethod;

        public ShippingMethodControlActionList(ShippingMethodControl ctrl)
            : base(ctrl) { ctrlShippingMethod = ctrl; }

        #endregion
       
        /// <summary>
        /// Returns the collection of DesignerActionItem objects contained in the list.
        /// </summary>
        /// <returns>A DesignerActionItem array that contains the items in this list</returns>
        public override System.ComponentModel.Design.DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection actionItems = new DesignerActionItemCollection();
            
            actionItems.Add(new DesignerActionHeaderItem("WWW.AspDotNetStoreFront.com"));

            return actionItems;
        }

    }
}
