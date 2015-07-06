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
using AspDotNetStorefrontLayout;
using AspDotNetStorefrontControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin.Controls
{
    public partial class LayoutItem : BaseUserControl<LayoutData>
    {
        public LayoutData ThisLayout;

        public override void DataBind()
        {
            base.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ThisLayout = this.Datasource;
            ctrlEditLayout.Datasource = this.Datasource;

            //ImageButton cbtn = pnlLayoutItem.FindControl<ImageButton>("imgbtnClone");
            //ImageButton dbtn = pnlLayoutItem.FindControl<ImageButton>("imgbtnDelete");
            //Button sbtn = pnlLayoutItem.FindControl<Button>("btnSaveEditedLayout");

            // ScriptManager from layouts.aspx
            // TableCell.TableRow.Table.ControlSearch.UpdatePanel.SearchableTemplate.UpdatePanel.LayoutList.UpdatePanel.LayoutPage
            //ScriptManager sm = Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.FindControl<ScriptManager>("scrptMgr");

            //UpdatePanel up = Parent.Parent.Parent.Parent.Parent.Parent.FindControl<UpdatePanel>("upListItems");

            //PostBackTrigger pbt = new PostBackTrigger();
            //pbt.ControlID = cbtn.ClientID;
            //up.Triggers.Add(pbt);

            //pbt = new PostBackTrigger();
            //pbt.ControlID = dbtn.ClientID;
            //up.Triggers.Add(pbt);

            //pbt = new PostBackTrigger();
            //pbt.ControlID = sbtn.ClientID;
            //up.Triggers.Add(pbt);

            // register the buttons
            //sm.RegisterPostBackControl(cbtn);
            //sm.RegisterPostBackControl(dbtn);
            //sm.RegisterPostBackControl(sbtn);

            litLayoutID.Text = ThisLayout.LayoutID.ToString();
            litLayoutName.Text = ThisLayout.Name;
            litLayoutMapped.Text = CommonLogic.IIF(ThisLayout.IsMapped, "Yes", "No");

            if (string.IsNullOrEmpty(ThisLayout.Icon))
            {
                imgLayoutIcon.ImageUrl = AppLogic.NoPictureImageURL(true, 1, ThisCustomer.LocaleSetting);
            }
            else
            {
                String lfIcon = "~/images/layouts/icon/" + ThisLayout.Icon;

                if (CommonLogic.FileExists(CommonLogic.SafeMapPath(lfIcon)))
                {
                    imgLayoutIcon.ImageUrl = lfIcon;
                }
                else
                {
                    imgLayoutIcon.ImageUrl = AppLogic.NoPictureImageURL(true, 1, ThisCustomer.LocaleSetting);
                }
            }

            if (String.IsNullOrEmpty(ThisLayout.Large))
            {
                imgLarge.ImageUrl = AppLogic.NoPictureImageURL(true, 1, ThisCustomer.LocaleSetting);
            }
            else
            {
                imgLarge.ImageUrl = "~/images/layouts/medium/" + ThisLayout.Large;
            }

        }

        public override bool UpdateChanges()
        {
            if (ctrlEditLayout.UpdateChanges())
            {
                
            }

            return base.UpdateChanges();
        }

        protected void imgbtnDelete_Click(object sender, ImageClickEventArgs e)
        {
            ThisLayout.Remove();
            Page.DataBind();
            
            //TableCell.TableRow.Table.Panel.Search.LayoutItem.UpdatePanel.LayoutList
            //Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.DataBind();
        }

        protected void imgbtnClone_Click(object sender, ImageClickEventArgs e)
        {
            ThisLayout.Clone();
        }

        protected void btnSaveEditedLayout_Click(object sender, EventArgs e)
        {
            UpdateChanges();
        }

        private void HideModalPanelByDefault(Panel pnl)
        {
            // we can't set the style declaratively
            // and we need the container to be a panel
            // so that we can assign the DefaultButton property

            // hide the div by default so that upon first load there won't be a sudden
            // flicker by the hiding of the div on browser page load
            pnl.Style["display"] = "none";
        }

        protected override void OnInit(EventArgs e)
        {
            HideModalPanelByDefault(pnlEditLayout);

            ThisLayout = this.Datasource;
            ctrlEditLayout.Datasource = this.Datasource;
            ctrlEditLayout.DataBind();

            base.OnInit(e);
        }
    }
}
