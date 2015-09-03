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
using System.Xml;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontCore
{
    public interface IConfigurationAtom
    {
        IEnumerable<AppConfigAtomInfo> AtomAppConfigs {get;}
        String HTMLHeader { get; }
        String Title { get; }
        IEnumerable<String> ConfigurationErrors(int storeId);
        Boolean IsConfigured(int storeId);
    }

    public class ConfigurationAtom : IConfigurationAtom
    {
        private const String _XmlDirectory = "ConfigurationAtoms/";
        public IEnumerable<AppConfigAtomInfo> AtomAppConfigs { get; protected set; }
        public String HTMLHeader { get; protected set; }
        public String Title { get; protected set; }

        public ConfigurationAtom(IEnumerable<AppConfigAtomInfo> atomAppConfigs, String htmlHeader, String title)
        {
            this.Init(atomAppConfigs, htmlHeader, title);
        }

        public ConfigurationAtom(String XMLFileName)
        {
            using (XmlTextReader reader = new XmlTextReader(CommonLogic.SafeMapPath(_XmlDirectory + XMLFileName)))
            {
                reader.WhitespaceHandling = WhitespaceHandling.None;
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                reader.Close();
                InitFromXML(doc);
            }
        }

        public ConfigurationAtom(XmlDocument xmlDoc)
        {
            InitFromXML(xmlDoc);
        }

        private void InitFromXML(XmlDocument xmlDoc)
        {
            XmlNode headerNode = xmlDoc.SelectSingleNode("/ConfigurationAtom/HtmlHeader");
            String header = string.Empty;
            if (headerNode != null && !String.IsNullOrEmpty(headerNode.InnerText))
                header = headerNode.InnerText;

            XmlNode titleNode = xmlDoc.SelectSingleNode("/ConfigurationAtom/Title");
            string title = string.Empty;
            if (titleNode != null && !string.IsNullOrEmpty(titleNode.InnerText))
                title = titleNode.InnerText;

            List<AppConfigAtomInfo> configs = new List<AppConfigAtomInfo>();
            foreach (XmlNode acNode in xmlDoc.SelectNodes("/ConfigurationAtom/AppConfig"))
            {
                String name = null;

                XmlNode nameNode = acNode.SelectSingleNode("Name");
                if (nameNode == null || string.IsNullOrEmpty(nameNode.InnerText))
                    throw new ArgumentException("Name node invalid.");
                
                name = nameNode.InnerText;

                AppConfig ac = AppConfigManager.GetAppConfig(0, nameNode.InnerText);
                Boolean hasCreateInfo = false;
                if (ac == null)
                {
                    XmlNode createNode = acNode.SelectSingleNode("CreateValues");
                    if (createNode != null)
                    {
                        String description = string.Empty;
                        String defaultValue = string.Empty;
                        String groupName = "CUSTOM";
                        Boolean superOnly = false;
                        String valueType = "string";
                        List<String> allowableValues = new List<string>();

                        XmlNode descriptionNode = createNode.SelectSingleNode("Description");
                        if (descriptionNode != null && !String.IsNullOrEmpty(descriptionNode.InnerText))
                            description = descriptionNode.InnerText;

                        XmlNode defaultVauleNode = createNode.SelectSingleNode("DefaultValue");
                        if (defaultVauleNode != null && !String.IsNullOrEmpty(defaultVauleNode.InnerText))
                            defaultValue = defaultVauleNode.InnerText;

                        XmlNode groupNameNode = createNode.SelectSingleNode("GroupName");
                        if (groupNameNode != null && !String.IsNullOrEmpty(groupNameNode.InnerText))
                            groupName = groupNameNode.InnerText.ToUpper();

                        XmlNode superOnlyNode = createNode.SelectSingleNode("SuperOnly");
                        if (superOnlyNode != null && !String.IsNullOrEmpty(superOnlyNode.InnerText))
                            superOnly = superOnlyNode.InnerText.ToBool();

                        XmlNode valueTypeNode = createNode.SelectSingleNode("ValueType");
                        if (valueTypeNode != null && !String.IsNullOrEmpty(valueTypeNode.InnerText))
                            valueType = valueTypeNode.InnerText;

                        XmlNode allowableValuesNode = createNode.SelectSingleNode("AllowableValues");
                        if (allowableValuesNode != null && !String.IsNullOrEmpty(allowableValuesNode.InnerText))
                            allowableValues = allowableValuesNode.InnerText.Split(',').ToList();


                        ac = new AppConfig(0, Guid.NewGuid(), name, description, defaultValue, groupName, superOnly, DateTime.Now, valueType, allowableValues);
                        ac.StoreId = 0;
                        hasCreateInfo = true;
                    }
                }
                
                AppConfigAtomInfo acai = new AppConfigAtomInfo(ac);
                acai.HasCreateInfo = hasCreateInfo;

                if (acNode.Attributes["Required"] != null)
                    acai.IsRequired = acNode.Attributes["Required"].InnerText.ToBool();

                if (acNode.Attributes["Advanced"] != null)
                    acai.IsAdvanced = acNode.Attributes["Advanced"].InnerText.ToBool();

                if (acNode.Attributes["FriendlyName"] != null)
                    acai.FriendlyName = acNode.Attributes["FriendlyName"].InnerText;

                XmlNode contextualDescriptionNode = acNode.SelectSingleNode("ContextualDescription");
                if (contextualDescriptionNode != null && !String.IsNullOrEmpty(contextualDescriptionNode.InnerText))
                    acai.ContextualDescription = contextualDescriptionNode.InnerText;

                configs.Add(acai);
            }

            this.Init(configs, header, title);
        }

        private void Init(IEnumerable<AppConfigAtomInfo> atomAppConfigs, String htmlHeader, String title)
        {
            this.AtomAppConfigs = atomAppConfigs;
            this.HTMLHeader = htmlHeader;
            this.Title = title;
            EnsureAppConfigsExist();
        }

        private void EnsureAppConfigsExist()
        {
            foreach (AppConfigAtomInfo acai in this.AtomAppConfigs)
            {
                if (!acai.HasCreateInfo || AppLogic.AppConfigExists(acai.Config.Name, 0))
                    break;

                AppConfigManager.CreateDBAndCacheAppConfig(acai.Config);
            }
        }

        public Boolean IsConfigured(int storeId)
        {
            return ConfigurationErrors(storeId).Count() == 0;
        }

        public IEnumerable<String> ConfigurationErrors(int storeId)
        {
            List<String> errors = new List<String>();
			foreach (AppConfigAtomInfo ac in AtomAppConfigs)
			{
				try
				{
					if (ac != null && ac.Config != null && ac.IsRequired && String.IsNullOrEmpty(AppLogic.AppConfig(ac.Config.Name, storeId, true)))
						errors.Add("Please configure the app config " + ac.Config.Name + ".");
				}
				catch { }
			}
            return errors;
        }
    }

    public class SearchConfigurationAtom : IConfigurationAtom
    {
        #region IConfigurationAtom Members
        public IEnumerable<AppConfigAtomInfo> AtomAppConfigs { get; protected set; }
        public string HTMLHeader { get; protected set; }
        public string Title { get; protected set; }
        #endregion

        public SearchConfigurationAtom(String searchTerm, String htmlHeader, String title)
        {
            List<AppConfigAtomInfo> acinfo = new List<AppConfigAtomInfo>();
            IEnumerable<AppConfig> acs = AppConfigManager.SearchAppConfigs(searchTerm, 0);

            foreach (AppConfig ac in acs)
                acinfo.Add(new AppConfigAtomInfo(ac));

            AtomAppConfigs = acinfo;
            HTMLHeader = htmlHeader;
            Title = title;
        }

        public IEnumerable<String> ConfigurationErrors(int StoreId)
        {
            return new List<String>();
        }

        public Boolean IsConfigured(int storeId)
        {
            return true;
        }
    }

    public class AppConfigAtomInfo
    {
        public AppConfig Config {get; protected set;}
        public Boolean IsRequired { get; set; }
        public Boolean IsAdvanced { get; set; }
        public String ContextualDescription { get; set; }
        public String FriendlyName { get; set; }
        public Boolean HasCreateInfo { get; set; }

        public AppConfigAtomInfo(AppConfig config) : this(config, false, false) {}
        public AppConfigAtomInfo(AppConfig config, bool isRequired, bool isAdvanced) : this(config, isAdvanced, isRequired, null) { }
        public AppConfigAtomInfo(AppConfig config, bool isRequired, bool isAdvanced, String contextualDescription) : this(config, isRequired, isAdvanced, contextualDescription, null) { }
        public AppConfigAtomInfo(AppConfig config, bool isRequired, bool isAdvanced, String contextualDescription, String friendlyName)
        {
            this.Config = config;
            this.IsRequired = isRequired;
            this.IsAdvanced = IsAdvanced;
            this.ContextualDescription = contextualDescription;
            this.FriendlyName = friendlyName;
        }
    }

}
