// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using AspDotNetStorefrontCore;

namespace Vortx.MobileFramework
{
    public enum MobileXmlPackageType
    {
        None,
        Product,
        Entity
    }

    public class MobileXmlPackageController
    {
        public static string XmlPackageHook(string xmlPackage, Customer thisCustomer)
        {
            if (xmlPackage == "")
            {
                xmlPackage = "entity.DNE.xml.config";
            }

            // check for the mobile skin, if not return the default xml package.
            if (!MobileHelper.isMobile())
                return xmlPackage;

            MobileXmlPackageType mobileType = MobileXmlPackageType.None;

            // Strip prefix from xml package name.
            if (xmlPackage.StartsWith(MobileXmlPackageType.Product.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                mobileType = MobileXmlPackageType.Product;
            }
            else if (xmlPackage.StartsWith(MobileXmlPackageType.Entity.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                mobileType = MobileXmlPackageType.Entity;
            }

            

            string mobileXmlPackage = string.Format("mobile.{0}", xmlPackage);
            string urlPath = string.Format("~/App_Templates/Skin_{0}/XmlPackages/{1}", thisCustomer.SkinID, mobileXmlPackage);            
            string fileName = HttpContext.Current.Server.MapPath(urlPath);
            // Test for existance of xml package.
            if (!CommonLogic.FileExists(fileName))
            {
                urlPath = string.Format("~/XmlPackages/{1}", thisCustomer.SkinID, mobileXmlPackage);
                fileName = HttpContext.Current.Server.MapPath(urlPath);
                if (!CommonLogic.FileExists(fileName))
                {
                    return DefaultXmlPackage(xmlPackage, mobileType);
                }
            }


            // we found a matching package, so return the name
            return mobileXmlPackage;
        }


        public static string DefaultXmlPackage(string xmlPackage, MobileXmlPackageType type)
        {
            switch (type)
            {
                case MobileXmlPackageType.Entity: 
                    return Vortx.Data.Config.MobilePlatform.DefaultXmlPackageEntity;
                case MobileXmlPackageType.Product:
                    return Vortx.Data.Config.MobilePlatform.DefaultXmlPackageProduct;
                default:
                    return xmlPackage;
            }
        }
    }
}
