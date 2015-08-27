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
using System.Web;
using System.Web.UI;
using System.IO;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontLayout
{
    public class BehaviorLayoutData
    {
        public BehaviorLayoutData()
        {
            Name = string.Empty;
            ContentControl = string.Empty;
            HasEditor = false;
            EditorControl = string.Empty;
        }

        public string Name { get; set; }
        public string ContentControl { get; set; }
        public bool HasEditor { get; set; }
        public string EditorControl { get; set; }
    }

    public enum LayoutMode
    {
        View,
        Edit,
        New
    }

    public static class BehaviorLayoutLoader
    {
        private static List<BehaviorLayoutData> layouts = new List<BehaviorLayoutData>();

        public static BehaviorLayoutData GetLayout(string name)
        {
            return layouts.Find(layout => layout.Name.EqualsIgnoreCase(name));
        }

        public static List<BehaviorLayoutData> GetLayouts()
        {
            if (layouts.Count == 0)
            {
                TryDiscoverLayouts();
            }

            return layouts;
        }


        private static void TryDiscoverLayouts()
        {
            layouts.Clear();

            var ctx = HttpContext.Current;
            if (ctx != null)
            {
                try
                {
                    // For the templating logic, we use a folder-filename convention
                    // to automatically discover the behaviors availble
                    // each behavior should be available in the {web}/controls/template/behaviors folder
                    // wherein each behavior should have their own folder
                    string layoutBehaviorsDir = ctx.Server.MapPath("~/controls/layout/behaviors");
                    string[] behaviorsDir = Directory.GetDirectories(layoutBehaviorsDir);

                    // get a reference to the controls folder
                    // which we'll base our relative paths to each individual controls                
                    string controlsDir = ctx.Server.MapPath("~/controls");
                    Uri controlsUri = new Uri(controlsDir);

                    // iterate through each found sub-directories
                    // which we'll assume as behaviors
                    foreach (var behaviorDir in behaviorsDir)
                    {
                        // Behaviors are usually composed of 2 parts and follows a file-name convention:
                        // content.ascx - contains the main display for that behavior
                        // editor.ascx - if applicable, will be used to provide editor interface to choose data display

                        // the least prerequisite is to have a "content.ascx" file
                        string contentControlPath = Path.Combine(behaviorDir, "content.ascx");
                        if (File.Exists(contentControlPath))
                        {
                            Uri contentUri = new Uri(contentControlPath);
                            Uri contentRelPath = controlsUri.MakeRelativeUri(contentUri);

                            BehaviorLayoutData layout = new BehaviorLayoutData() { ContentControl = contentRelPath.ToString() };
                            layout.Name = new DirectoryInfo(behaviorDir).Name;

                            // check to see if this behavior offers any edit mode
                            string editorControlPath = Path.Combine(behaviorDir, "editor.ascx");
                            layout.HasEditor = File.Exists(editorControlPath);
                            if (layout.HasEditor)
                            {
                                Uri editorUri = new Uri(editorControlPath);
                                Uri editorRelPath = controlsUri.MakeRelativeUri(editorUri);

                                layout.EditorControl = editorRelPath.ToString();
                            }

                            layouts.Add(layout);
                        }
                        
                    }
                }
                catch { }
            }
            
        }

    }
}
