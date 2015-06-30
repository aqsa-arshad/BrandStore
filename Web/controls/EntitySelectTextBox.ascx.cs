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
using AspDotNetStorefrontCore;
using AspDotNetStorefront;
using AjaxControlToolkit;
using Telerik.Web.UI;

namespace AspDotNetStorefront
{
    public partial class EntitySelectTextBoxControl : System.Web.UI.UserControl, ITextControl
    {
        public string EntityType
        {
            get
            {
                object savedValue = ViewState["EntityType"];
                if (null == savedValue) { return string.Empty; }

                return savedValue.ToString();
            }
            set
            {
                ViewState["EntityType"] = value;

                AssignRadNavUrl();
            }
        }

        public int EntityID
        {
            get
            {
                return this.Text.ToNativeInt();
            }
            set
            {
                this.Text = value.ToString();
            }
        }

        public string Text
        {
            get
            {
                return txtEntityID.Text;
            }
            set
            {
                txtEntityID.Text = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.EntityType))
            {
                throw new InvalidOperationException("Entity type not specified!");
            }

            this.AppRelativeTemplateSourceDirectory = "~/";

            AssignRadNavUrl();
            RegisterScripts();
        }

        private void AssignRadNavUrl()
        {
            rwEntityList.NavigateUrl = "entityobjectlist.aspx?entitytype={0}&TextBoxClientID={1}".FormatWith(this.EntityType, this.txtEntityID.ClientID);
        }

        private void RegisterScripts()
        {
            var script = @"            

            var fn = function() {{ 
                var rwEntityList = $find('{0}');
                if (rwEntityList) {{                    
                    rwEntityList.set_openerElementID('{1}');
                    rwEntityList.set_offsetElementID('{1}');

                    var ctrl = new aspdnsf.Controls.EntityObjectList('{1}', '{0}');
                    aspdnsf.Controls.EntityObjectListManager.register(ctrl);
                }}

                Sys.Application.remove_load(fn);
            }}
            Sys.Application.add_load(fn);
            ".FormatWith(rwEntityList.ClientID, txtEntityID.ClientID);

            var scrptMgr = ScriptManager.GetCurrent(this.Page);
            if (scrptMgr != null)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(), script, true);
            }

            
        }
       

    }
}

