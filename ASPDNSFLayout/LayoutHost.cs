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
using System.Web.UI;
using AspDotNetStorefrontLayout.Behaviors;
using AspDotNetStorefrontCore;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace AspDotNetStorefrontLayout
{
    public class LayoutHost : UserControl
    {
        public LayoutHost()
        {
            LayoutSettings = new List<Settings>();
        }

        public string FileName { get; set; }
        public int EntityID { get; set; }
        public string EntityType { get; set; }
        public List<Settings> LayoutSettings { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            DetermineFileName();
            LoadData();
            InitializePanels();
            base.OnLoad(e);
            
        }        

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        private void DetermineFileName()
        {
            if (this.TemplateControl != null)
            {
                this.FileName = Path.GetFileName(this.TemplateControl.AppRelativeVirtualPath);
            }
        }

        public void InitializePanels()
        {
            IEnumerable<LayoutPanel> panels = Controls
                        .Cast<Control>()
                        .Where(ctrl => ctrl is LayoutPanel)
                        .Select(ctrl => ctrl as LayoutPanel);

            foreach (LayoutPanel panel in panels)
            {
                panel.Host = this;

                if (panel.IsNew)
                {
                    var behaviorName = panel.CurrentBehavior;
                    var editBehavior = LoadBehaviorControl(behaviorName, true);
                    if (editBehavior != null)
                    {
                        SwitchToEditMode(panel, editBehavior);
                    }
                }
                else
                {
                    IBehavior ctrl = HydrateBehavior(panel.ID, panel.EditMode);
                    panel.Behavior = ctrl;

                    // if first time load
                    if (ctrl != null &&
                        !this.IsPostBack)
                    {
                        ctrl.IsFirstLoad = true;
                    }
                }

                panel.EditInvoked += new EventHandler(Panel_EditInvoked);
                panel.Initialize();
            }
        }

        //public void SetBehaviorAndSwitchToEditMode(LayoutPanel panel, string behaviorName)
        //{
        //    var templates = BehaviorTemplateLoader.GetTemplates();
        //    var editBehavior = templates.Find(template => template.Name.EqualsIgnoreCase(behaviorName));
        //    LoadBehaviorControl(
        //}

        protected virtual void Panel_EditInvoked(object sender, EventArgs e)
        {
            LayoutPanel panel = sender as LayoutPanel;
            SwitchToEditMode(panel);
        }

        public void SwitchToEditMode(LayoutPanel panel)
        {
            // re-initialize
            IBehavior editBehavior = HydrateBehavior(panel.ID, true);
            SwitchToEditMode(panel, editBehavior);
        }

        public void SwitchToEditMode(LayoutPanel panel, IBehavior editBehavior)
        {
            if (editBehavior != null)
            {
                // assure first time load
                editBehavior.IsFirstLoad = true;
                panel.Behavior = editBehavior;
                panel.EditMode = true;
                panel.Initialize();
            }
            else
            {
                throw new InvalidOperationException("No behavior yet defined");
            }            
        }

        public void RemoveBehavior(LayoutPanel container)
        {
            var sql = "DELETE EntityTemplate WHERE EntityID = {0} AND EntityType = {1} AND TemplateFile = {2} and PanelID = {3}".FormatWith(this.EntityID, 
                this.EntityType.DBQuote(), 
                this.FileName.DBQuote(), 
                container.ID.DBQuote());

            DB.ExecuteSQL(sql);
            container.Behavior = null;
            container.Initialize();
        }

        public void StopEditing(IBehavior behavior)
        {
            var container = behavior.Container;

            IBehavior ctrl = HydrateBehavior(container.ID, false);

            // assure first time load
            ctrl.IsFirstLoad = true;
            container.Behavior = ctrl;
            container.EditMode = false;
            container.IsNew = false;
            container.Initialize();
        }

        private IBehavior HydrateBehavior(string id)
        {
            return HydrateBehavior(id, false);
        }

        private IBehavior HydrateBehavior(string id, bool edit)
        {
            IBehavior behavior = null;
            // find the mapped setting if any
            Settings mappedSetting = LayoutSettings
                                .Where(setting => setting.PanelID.EqualsIgnoreCase(id))
                                .FirstOrDefault();

            if (mappedSetting != null)
            {
                behavior = LoadBehaviorControl(mappedSetting.BehaviorControl, edit);
                if (behavior != null && 
                    !string.IsNullOrEmpty(mappedSetting.BehaviorDataRaw))
                {
                    var data = TryHydrateData(behavior.GetType(), mappedSetting.BehaviorDataRaw);

                    // Although we're using generics to have strongly typed Data properties,
                    // Generics unfortunately doesn't support contra-variance for downcasting
                    // the defined template for T in IBehavior<T> eventhough the T will always
                    // a type inherited from IData, we therefore resort to reflection
                    var dataProperty = behavior
                                        .GetType()
                                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                        .Where(prop => prop.Name.EqualsIgnoreCase("Data"))
                                        .FirstOrDefault();
                    if (dataProperty != null)
                    {
                        dataProperty.SetValue(behavior, data, new object[] { });
                    }
                }
            }

            return behavior;
        }

        private IData TryHydrateData(Type type, string serialization)
        {
            IData data = null;
            //Type trueDataType = null;
            
            // we can't use the Data property as it's null at this point
            // so we'll just infer it through reflection
            var trueDataType = DiscoverTrueDataType(type);

            if (trueDataType != null)
            {
                data = JSONHelper.Deserialize(serialization, trueDataType) as IData;
            }

            return data;
        }

        private Type DiscoverTrueDataType(Type behaviorType)
        {
            // we can't use the Data property as it's null at this point
            // so we'll just infer it through reflection
            var trueDataType = behaviorType
                                .GetInterfaces()
                                .Where(iType => iType.IsGenericType && iType.FullName.ContainsIgnoreCase("IBehaviorData"))
                                .Select(iType => iType.GetGenericArguments().FirstOrDefault())
                                .FirstOrDefault();

            return trueDataType;
        }

        public IBehavior LoadBehaviorControl(string name)
        {
            return LoadBehaviorControl(name, false);
        }

        private const string LAYOUT_EDITOR_STANDARD_NAME = "editor";
        private const string LAYOUT_CONTENT_STANDARD_NAME = "content";

        public IBehavior LoadBehaviorControl(string name, bool edit)
        {
            var behaviorRel = "~/controls/layout/behaviors/{0}/{1}.ascx".FormatWith(name, edit ? LAYOUT_EDITOR_STANDARD_NAME : LAYOUT_CONTENT_STANDARD_NAME);
            var ctrlBehavior = LoadControl(behaviorRel);
            if (ctrlBehavior != null)
            {
                ctrlBehavior.AppRelativeTemplateSourceDirectory = this.Page.AppRelativeTemplateSourceDirectory; // "~/";
                ctrlBehavior.ID = "ctrlBehavior";
            }

            return ctrlBehavior as IBehavior;
        }

        //private IBehavior LoadBehaviorControlEdit(string name)
        //{
        //    var behaviorRel = "~/controls/template/behaviors/{0}/edit.ascx".FormatWith(name);
        //    var ctrlBehavior = LoadControl(behaviorRel);
        //    if (ctrlBehavior != null)
        //    {
        //        ctrlBehavior.AppRelativeTemplateSourceDirectory = this.Page.AppRelativeTemplateSourceDirectory; // "~/";
        //        ctrlBehavior.ID = "ctrlBehavior";
        //    }

        //    return ctrlBehavior as IBehavior;
        //}

        private void LoadData()
        {
            LayoutSettings = Settings.GetSettings(this.FileName, this.EntityType, this.EntityID);
        }

        public void SaveSettings(IBehavior behavior, IData data)
        {
            var panelSettings = Settings.GetPanelSettings(this.FileName, this.EntityType, this.EntityID, behavior.Container.ID);
            if (panelSettings == null)
            {
                panelSettings = new Settings();
                panelSettings.PanelID = behavior.Container.ID;
            }

            panelSettings.BehaviorControl = behavior.Name;
            // serialize will always base on the actual concrete datatype
            var serialized = JSONHelper.Serialize(data);
            panelSettings.BehaviorDataRaw = serialized;

            using(var con = DB.dbConn())
            {
                using (var aspdnsf_SaveLayout = new SqlCommand("aspdnsf_SaveLayout", con))
                {
                    aspdnsf_SaveLayout.CommandType = CommandType.StoredProcedure;
                    aspdnsf_SaveLayout.Parameters.Add(new SqlParameter("@EntityType", this.EntityType));
                    aspdnsf_SaveLayout.Parameters.Add(new SqlParameter("@EntityID", this.EntityID));
                    aspdnsf_SaveLayout.Parameters.Add(new SqlParameter("@LayoutFile", this.FileName));
                    aspdnsf_SaveLayout.Parameters.Add(new SqlParameter("@PanelID", panelSettings.PanelID));
                    aspdnsf_SaveLayout.Parameters.Add(new SqlParameter("@Behavior", panelSettings.BehaviorControl));
                    aspdnsf_SaveLayout.Parameters.Add(new SqlParameter("@BehaviorData", panelSettings.BehaviorDataRaw));

                    con.Open();
                    aspdnsf_SaveLayout.ExecuteNonQuery();
                }
            }

            // update mapped settings re-load data..
            LoadData();
            

        }

    }
}
