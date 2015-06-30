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
using System.Drawing;
using AspDotNetStorefrontCore;
using AspDotNetStorefront;
using AspDotNetStorefrontControls;

namespace AspDotNetStorefront
{
    public partial class MapLayout : BaseUserControl<LayoutMap>
    {
        private String LocaleSetting;
        private int m_skinID;

        protected void Page_Load(object sender, EventArgs e)
        {
            LocaleSetting = (Page as SkinBase).ThisCustomer.LocaleSetting;
            m_skinID = (Page as SkinBase).SkinID;

            if (!Page.IsPostBack)
            {
                SetVisibility(true);

                ltdetails.Text = AppLogic.GetString("layouts.map.defaultdetails", m_skinID, LocaleSetting);
            }
            else
            {
                SetVisibility(false);
            }

            BindData();
        }

        public void SetVisibility(Boolean firstLoad)
        {
            ltdetails.Visible = firstLoad;
            ltselectedidlabel.Visible = !firstLoad;
            ltselectedid.Visible = !firstLoad;
            ltselectednamelabel.Visible = !firstLoad;
            ltselectedname.Visible = !firstLoad;
            // TBD
            ltpreview.Visible = false;
            // END TBD
            ltcurrentpagelabel.Visible = !firstLoad;
            ltcurrentpage.Visible = !firstLoad;
            ltobjecttypelabel.Visible = !firstLoad;
            ltobjecttype.Visible = !firstLoad;
            ltobjectidlabel.Visible = !firstLoad;
            ltobjectid.Visible = !firstLoad;  
        }

        private void BindData()
        {
            var results = LayoutData.GetLayouts();

            rptrList.DataSource = results;
            rptrList.DataBind();
        }

        protected void rptrList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType.Equals(ListItemType.AlternatingItem) || e.Item.ItemType.Equals(ListItemType.Item))
            {
                LayoutData ld = (LayoutData)e.Item.DataItem;

                LayoutListItem lli;
                LinkButton lb;

                if (e.Item.ItemType.Equals(ListItemType.AlternatingItem))
                {
                    lli = (LayoutListItem)e.Item.FindControl("lliA");
                    lb = (LinkButton)e.Item.FindControl("lbtnA");
                }
                else
                {
                    lli = (LayoutListItem)e.Item.FindControl("lli");
                    lb = (LinkButton)e.Item.FindControl("lbtn");
                }

                lb.CommandArgument = ld.LayoutID.ToString();

                // Find the script manager and register the link button from the repeater
                ScriptManager sm = (ScriptManager)Page.Master.FindControl("scrptMgr");
                sm.RegisterAsyncPostBackControl(lb);

                // set the LayoutListItem properties
                lli.LocaleSetting = this.LocaleSetting;
                lli.SkinID = this.m_skinID;
                lli.LayoutName = ld.Name;
                lli.LayoutThumb = "~/images/layouts/icon/" + ld.Icon;
                lli.LayoutLarge = "~/images/layouts/medium/" + ld.Large;
            }
        }

        protected void rptrList_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            if (e.CommandName.Equals("Details", StringComparison.OrdinalIgnoreCase))
            {
                LoadDetailsPanel(int.Parse(e.CommandArgument.ToString()));
                upDetails.Update();
            }
        }

        private void ObjectOrEntityVisible(Boolean show)
        {
            ltobjecttype.Visible = show;
            ltobjecttypelabel.Visible = show;
            ltobjectid.Visible = show;
            ltobjectidlabel.Visible = show;
        }

        private void LoadDetailsPanel(int lID)
        {
            ObjectOrEntityVisible(false);

            LayoutData ld = new LayoutData(lID);

            bool IsTopic = (Page as SkinBase).IsTopicPage;
            bool IsEntity = (Page as SkinBase).IsEntityPage;
            bool IsProduct = (Page as SkinBase).IsProductPage;

            int ID = (Page as SkinBase).PageID;
            
            ltselectedid.Text = ld.LayoutID.ToString();
            ltselectedname.Text = ld.Name;

            ltcurrentpage.Text = CommonLogic.GetThisPageName(false);

            if (IsTopic)
            {
                ObjectOrEntityVisible(true);
                ltobjecttype.Text = AppLogic.GetString("layouts.map.topic", m_skinID, LocaleSetting);
                ltobjectid.Text = ID.ToString();
            }

            if (IsProduct)
            {
                ObjectOrEntityVisible(true);
                ltobjecttype.Text = AppLogic.GetString("common.cs.Product", m_skinID, LocaleSetting);
                ltobjectid.Text = ID.ToString();
            }

            if (IsEntity)
            {
                ObjectOrEntityVisible(true);
                string EType = (Page as SkinBase).EntityType;

                switch(EType.ToLowerInvariant())
                {
                    case "category":
                        ltobjecttype.Text = AppLogic.GetString("AppConfig.CategoryPromptSingular", m_skinID, LocaleSetting);
                        break;
                    case "manufacturer":
                        ltobjecttype.Text = AppLogic.GetString("AppConfig.ManufacturerPromptSingular", m_skinID, LocaleSetting);
                        break;
                    case "distributor":
                        ltobjecttype.Text = AppLogic.GetString("AppConfig.DistributorPromptSingular", m_skinID, LocaleSetting);
                        break;
                    case "section":
                        ltobjecttype.Text = AppLogic.GetString("AppConfig.SectionPromptSingular", m_skinID, LocaleSetting);
                        break;
                    case "genre":
                        ltobjecttype.Text = AppLogic.GetString("AppConfig.GenrePromptSingular", m_skinID, LocaleSetting);
                        break;
                    case "vector":
                        ltobjecttype.Text = AppLogic.GetString("AppConfig.VectorPromptSingular", m_skinID, LocaleSetting);
                        break;
                    case "library":
                        ltobjecttype.Text = AppLogic.GetString("AppConfig.LibraryPromptSingular", m_skinID, LocaleSetting);
                        break;
                    case "affiliate":
                        ltobjecttype.Text = AppLogic.GetString("AppConfig.AffiliatePromptSingular", m_skinID, LocaleSetting);
                        break;
                    default:
                        ltobjecttype.Text = AppLogic.GetString("common.cs.NA", m_skinID, LocaleSetting);
                        break;
                }

                ltobjectid.Text = ID.ToString();
            }
        }

        public override bool UpdateChanges()
        {
            int SelectedLayoutID = int.Parse(ltselectedid.Text);

            bool IsTopic = (Page as SkinBase).IsTopicPage;
            bool IsEntity = (Page as SkinBase).IsEntityPage;
            bool IsProduct = (Page as SkinBase).IsProductPage;

            int ID = (Page as SkinBase).PageID;

            LayoutMap lm = new LayoutMap();
            lm.LayoutID = SelectedLayoutID;
            lm.PageID = ID;

            if (IsEntity)
            {
                String EType = (Page as SkinBase).EntityType;

                lm.PageTypeName = EType.ToLowerInvariant();
            }
            else if (IsTopic)
            {
                lm.PageTypeName = "topic";
            }
            else if (IsProduct)
            {
                lm.PageTypeName = "product";
            }
            else
            {
                lm.PageTypeName = ltcurrentpage.Text;
            }

            lm.Commit();

            return base.UpdateChanges();
        }
    }
}
