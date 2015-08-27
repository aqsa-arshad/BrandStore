// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AspDotNetStorefrontCore
{
	public interface ISkinProvider
	{
		Skin GetSkinById(int id);
		Skin GetSkinByName(string name);
		IEnumerable<Skin> GetSkins();
	}

	public class SkinProvider : ISkinProvider
	{

		public Skin GetSkinById(int id)
		{
			return GetSkinByName(String.Format("Skin_{0}", id));
		}

		public Skin GetSkinByName(string name)
		{
			
			var lastIndexOfUnderscore = name.LastIndexOf('_') + 1;
			if(lastIndexOfUnderscore == 0)
				return null;

			var idString = name.Substring(lastIndexOfUnderscore, name.Length - lastIndexOfUnderscore);
			int id;
			if(!int.TryParse(idString, out id))
				return null;

			//get skin data from the data file if we have one.
			var dataFilePath = HostingEnvironment.MapPath(String.Format("~/App_Templates/{0}/SkinInfo/skininfo.xml", name));
			string displayName = null;
			string description = null;
			var isMobile = false;
			if(File.Exists(dataFilePath))
			{
				var skinDataFile = File.ReadAllText(dataFilePath);
				var xElement = XElement.Parse(skinDataFile);
				displayName = xElement.XPathSelectElement("/DisplayName") == null ? String.Empty : xElement.XPathSelectElement("/DisplayName").Value;
				description = xElement.XPathSelectElement("/Description") == null ? String.Empty : xElement.XPathSelectElement("/Description").Value;
				isMobile = xElement.XPathSelectElement("/MobileOnly") == null ? false : (xElement.XPathSelectElement("/MobileOnly").Value.ToLower() == "true");
			}

			//get the preview image
			string previewUrl = null;
			if(File.Exists(HostingEnvironment.MapPath(String.Format("~/App_Templates/{0}/SkinInfo/preview.jpg", name))))
				previewUrl = String.Format("~/App_Templates/{0}/SkinInfo/preview.jpg", name);
			else if(File.Exists(HostingEnvironment.MapPath(String.Format("~/App_Templates/{0}/SkinInfo/preview.png", name))))
				previewUrl = String.Format("~/App_Templates/{0}/SkinInfo/preview.png", name);
			else if(File.Exists(HostingEnvironment.MapPath(String.Format("~/App_Templates/{0}/SkinInfo/preview.gif", name))))
				previewUrl = String.Format("~/App_Templates/{0}/SkinInfo/preview.gif", name);

			return new Skin(id, name, displayName, description, previewUrl, isMobile);
		}

		public IEnumerable<Skin> GetSkins()
		{
			var skins = new List<Skin>();
			var directoryInfo = new DirectoryInfo(HostingEnvironment.MapPath("~/App_Templates"));
			foreach(DirectoryInfo directory in directoryInfo.GetDirectories("SKIN*"))
			{
				skins.Add(GetSkinByName(directory.Name));
			}
			return skins;
		}
	}
}
