// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront.Kit
{
    public partial class GroupTemplate : BaseKitGroupControl
    {
        public override void DataBind()
        {
            base.DataBind();

            ITemplate template = null;

            if (KitGroup.IsReadOnly && KitGroup.HasNoEditableField)
            {
                template = LoadTemplate("controls/kit/ReadOnlyItems.ascx");
            }
            else
            {
                switch (KitGroup.SelectionControl)
                {
                    case KitGroupData.SINGLE_SELECT_DROPDOWN_LIST:
                        template = LoadTemplate("controls/kit/DropDownListItems.ascx");
                        break;

                    case KitGroupData.SINGLE_SELECT_RADIO_LIST:
                        template = LoadTemplate("controls/kit/RadioListItems.ascx");
                        break;

                    case KitGroupData.MULTI_SELECT_RADIO_LIST:
                        template = LoadTemplate("controls/kit/MultiSelectListItems.ascx");
                        break;

                    case KitGroupData.TEXT_AREA:
                        template = LoadTemplate("controls/kit/TextItems.ascx");
                        break;

                    case KitGroupData.TEXT_OPTION:
                        template = LoadTemplate("controls/kit/TextItems.ascx");
                        break;

                    case KitGroupData.FILE_OPTION:
                        template = LoadTemplate("controls/kit/FileUploadItems.ascx");
                        break;
                }
            }

            if (template != null)
            {
                template.InstantiateIn(plhKitItemTemplate);
                BaseKitGroupControl ctrlKitGroup = plhKitItemTemplate.Controls[0] as BaseKitGroupControl;
                ctrlKitGroup.ID = string.Format("ctrlGroup_{0}", KitGroup.Id);
                ctrlKitGroup.ThisCustomer = ThisCustomer;
                ctrlKitGroup.KitGroup = KitGroup;
                ctrlKitGroup.DataBind();
            }
        }

        public override void ResolveSelection()
        {
            if (plhKitItemTemplate.Controls.Count > 0)
            {
                (plhKitItemTemplate.Controls[0] as BaseKitGroupControl).ResolveSelection();
            }            
        }
    }
}

