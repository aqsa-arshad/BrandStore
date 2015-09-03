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
using AspDotNetStorefrontLayout.Behaviors;
using AspDotNetStorefrontCore;
using System.Data;
using System.Data.Sql;

namespace AspDotNetStorefrontLayout
{
    public class Settings
    {
        public Settings() 
        {
            PanelID = string.Empty;
            BehaviorControl = string.Empty;
            BehaviorDataRaw = string.Empty;
            //BehaviorData = null;
        }

        public string PanelID { get; set; }
        public string BehaviorControl { get; set; }
        public string BehaviorDataRaw { get; set; }
        //public IData BehaviorData { get; set; }


        public static List<Settings> GetSettings(string layoutFile, string entityType, int entityID)
        {
            var layoutSettings = new List<Settings>();

            var query = "SELECT PanelID, Behavior, BehaviorData FROM EntityLayout WITH (NOLOCK) WHERE Layoutfile = {0} AND EntityType = {1} AND EntityID = {2}"
                        .FormatWith(layoutFile.DBQuote(), entityType.DBQuote(), entityID);

            Action<IDataReader> readAction = (rs) =>
            {
                while (rs.Read())
                {
                    Settings setting = new Settings();
                    setting.PanelID = rs.Field("PanelID");
                    setting.BehaviorControl = rs.Field("Behavior");
                    setting.BehaviorDataRaw = rs.Field("BehaviorData");

                    layoutSettings.Add(setting);
                }
            };            

            DB.UseDataReader(query, readAction);

            return layoutSettings;
        }

        public static Settings GetPanelSettings(string layoutFile, string entityType, int entityID, string panelId)
        {
            Settings setting = null;

            var query = "SELECT PanelID, Behavior, BehaviorData FROM EntityLayout WITH (NOLOCK) WHERE Layoutfile = {0} AND EntityType = {1} AND EntityID = {2} AND PanelID = {3}"
                        .FormatWith(layoutFile.DBQuote(), 
                            entityType.DBQuote(), 
                            entityID, 
                            panelId.DBQuote());
            

            Action<IDataReader> readAction = (rs) =>
            {
                while (rs.Read())
                {
                    setting = new Settings();
                    setting.PanelID = rs.Field("PanelID");
                    setting.BehaviorControl = rs.Field("Behavior");
                    setting.BehaviorDataRaw = rs.Field("BehaviorData");
                }
            };

            DB.UseDataReader(query, readAction);

            return setting;
        }

    }
}
