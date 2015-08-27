// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontCore
{
    public enum CouponTypeEnum
    {
        OrderCoupon = 0,
        ProductCoupon = 1,
        GiftCard = 2
    }

    public class CouponObject
    {
        internal int m_couponid = 0;
        internal string m_couponguid = String.Empty;
        internal string m_couponcode = String.Empty;
        internal string m_description = String.Empty;
        internal DateTime m_expirationdate = System.DateTime.MinValue;
        internal decimal m_discountpercent = System.Decimal.Zero;
        internal decimal m_discountamount = System.Decimal.Zero;
        internal bool m_discountincludesfreeshipping = false;
        internal bool m_expiresonfirstusebyanycustomer = false;
        internal bool m_expiresafteroneusagebyeachcustomer = false;
        internal int m_expiresafternuses = 0;
        internal decimal m_requiresminimumorderamount = 0;
        internal List<int> m_validforcustomers = new List<int>();
        internal List<int> m_validforproducts = new List<int>();
        internal List<int> m_validforcategories = new List<int>();
        internal List<int> m_validforsections = new List<int>();
        internal List<int> m_validformanufacturers = new List<int>();
        internal List<int> m_notvalidforcustomers = new List<int>();
        internal List<int> m_notvalidforproducts = new List<int>();
        internal List<int> m_notvalidforcategories = new List<int>();
        internal List<int> m_notvalidforsections = new List<int>();
        internal List<int> m_notvalidformanufacturers = new List<int>();
        internal List<int> m_validforproductsexpanded = new List<int>();
        internal List<int> m_validforcategoriesexpanded = new List<int>();
        internal List<int> m_validforsectionsexpanded = new List<int>();
        internal List<int> m_validformanufacturersexpanded = new List<int>();
        internal DateTime m_startdate = System.DateTime.MaxValue;
        internal CouponTypeEnum m_coupontype;
        internal int m_numuses = 0;
        internal string m_extensiondata = String.Empty;
        internal bool m_deleted = false;
        internal DateTime m_createdon = DateTime.MinValue;
        internal bool m_couponset = false;

        public CouponObject()
        {
            m_validforcustomers = new List<int>();
            m_validforproducts = new List<int>();
            m_validforcategories = new List<int>();
            m_validforsections = new List<int>();
            m_validformanufacturers = new List<int>();
            m_notvalidforcustomers = new List<int>();
            m_notvalidforproducts = new List<int>();
            m_notvalidforcategories = new List<int>();
            m_notvalidforsections = new List<int>();
            m_notvalidformanufacturers = new List<int>();
            m_validforproductsexpanded = new List<int>();
            m_validforcategoriesexpanded = new List<int>();
            m_validforsectionsexpanded = new List<int>();
            m_validformanufacturersexpanded = new List<int>();
        }

        public List<CouponObject> InitCoupon(String couponCode, Customer ThisCustomer)
        {
            List<CouponObject> cList = new List<CouponObject>();

            using (IDataReader rs = DB.GetRS("select * from coupon where CouponCode=" + DB.SQuote(couponCode), new SqlConnection(DB.GetDBConn())))
            {
                while (rs.Read())
                {
                    CouponObject co = new CouponObject();

                    co.m_couponcode = DB.RSField(rs, "CouponCode");
                    co.m_coupontype = (CouponTypeEnum)DB.RSFieldInt(rs, "CouponType");
                    co.m_description = DB.RSField(rs, "Description");
                    co.m_startdate = DB.RSFieldDateTime(rs, "StartDate");
                    co.m_expirationdate = DB.RSFieldDateTime(rs, "ExpirationDate");
                    co.m_discountamount = DB.RSFieldDecimal(rs, "DiscountAmount");
                    co.m_discountpercent = DB.RSFieldDecimal(rs, "DiscountPercent");
                    co.m_discountincludesfreeshipping = DB.RSFieldBool(rs, "DiscountIncludesFreeShipping");
                    co.m_expiresonfirstusebyanycustomer = DB.RSFieldBool(rs, "ExpiresOnFirstUseByAnyCustomer");
                    co.m_expiresafteroneusagebyeachcustomer = DB.RSFieldBool(rs, "ExpiresAfterOneUsageByEachCustomer");
                    co.m_expiresafternuses = DB.RSFieldInt(rs, "ExpiresAfterNUses");
                    co.m_requiresminimumorderamount = DB.RSFieldDecimal(rs, "RequiresMinimumOrderAmount");

                    co.m_validforcustomers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForCustomers"), "\\s+", "", RegexOptions.Compiled));
                    co.m_validforproducts = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForProducts"), "\\s+", "", RegexOptions.Compiled));
                    co.m_validforcategories = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForCategories"), "\\s+", "", RegexOptions.Compiled));
                    co.m_validforsections = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForSections"), "\\s+", "", RegexOptions.Compiled));
                    co.m_validformanufacturers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rs, "ValidForManufacturers"), "\\s+", "", RegexOptions.Compiled));

                    co.m_validforproductsexpanded = new List<int>();
                    co.m_validforcategoriesexpanded = new List<int>();
                    co.m_validforsectionsexpanded = new List<int>();
                    co.m_validformanufacturersexpanded = new List<int>();

                    if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validforcategories.Count > 0)
                    {
                        co.m_validforcategoriesexpanded = AppLogic.LookupHelper("Category", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validforcategories), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                        List<int> pList = AppLogic.LookupHelper("Category", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validforcategoriesexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                        co.m_validforproductsexpanded.AddRange(pList);
                    }
                    if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validforsections.Count > 0)
                    {
                        co.m_validforsectionsexpanded = AppLogic.LookupHelper("Section", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validforsections), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                        List<int> pList = AppLogic.LookupHelper("Section", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validforsectionsexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                        co.m_validforproductsexpanded.AddRange(pList);
                    }
                    if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validformanufacturers.Count > 0)
                    {
                        co.m_validformanufacturersexpanded = AppLogic.LookupHelper("Manufacturer", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validformanufacturers), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                        List<int> pList = AppLogic.LookupHelper("Manufacturer", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validformanufacturersexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                        co.m_validforproductsexpanded.AddRange(pList);
                    }

                    co.m_numuses = DB.RSFieldInt(rs, "NumUses");

                    cList.Add(co);
                }
            }

            return cList;
        }

        public string CouponCode
        {
            get { return m_couponcode; }
        }
        public int CouponID
        {
            get { return m_couponid; }
        }
        public string CouponGUID
        {
            get { return m_couponguid; }
        }
        public string Description
        {
            get { return m_description; }
        }
        public DateTime StartDate
        {
            get { return m_startdate; }
        }
        public DateTime ExpirationDate
        {
            get { return m_expirationdate; }
        }
        public decimal DiscountPercent
        {
            get { return m_discountpercent; }
        }
        public decimal DiscountAmount
        {
            get { return m_discountamount; }
        }
        public bool DiscountIncludesFreeShipping
        {
            get { return m_discountincludesfreeshipping; }
        }
        public bool ExpiresOnFirstUseByAnyCustomer
        {
            get { return m_expiresonfirstusebyanycustomer; }
        }
        public bool ExpiresAfterOneUsageByEachCustomer
        {
            get { return m_expiresafteroneusagebyeachcustomer; }
        }
        public int ExpiresAfterNUses
        {
            get { return m_expiresafternuses; }
        }
        public decimal RequiresMinimumOrderAmount
        {
            get { return m_requiresminimumorderamount; }
        }
        public List<int> ValidForCustomers
        {
            get { return m_validforcustomers; }
        }
        public List<int> ValidForProducts
        {
            get { return m_validforproducts; }
        }
        public List<int> ValidForCategories
        {
            get { return m_validforcategories; }
        }
        public List<int> ValidForSections
        {
            get { return m_validforsections; }
        }
        public List<int> ValidForManufacturers
        {
            get { return m_validformanufacturers; }
        }
        public List<int> NotValidForCustomers
        {
            get { return m_notvalidforcustomers; }
        }
        public List<int> NotValidForProducts
        {
            get { return m_notvalidforproducts; }
        }
        public List<int> NotValidForCategories
        {
            get { return m_notvalidforcategories; }
        }
        public List<int> NotValidForSections
        {
            get { return m_notvalidforsections; }
        }
        public List<int> NotValidForManufacturers
        {
            get { return m_notvalidformanufacturers; }
        }
        public List<int> ValidForProductsExpanded
        {
            get { return m_validforproductsexpanded; }
        }
        public List<int> ValidForCategoriesExpanded
        {
            get { return m_validforcategoriesexpanded; }
        }
        public List<int> ValidForSectionsExpanded
        {
            get { return m_validforsectionsexpanded; }
        }
        public List<int> ValidForManufacturersExpanded
        {
            get { return m_validformanufacturersexpanded; }
        }
        public CouponTypeEnum CouponType
        {
            get { return m_coupontype; }
        }
        public int NumUses
        {
            get { return m_numuses; }
        }
        public string ExtensionData
        {
            get { return m_extensiondata; }
        }
        public bool Deleted
        {
            get { return m_deleted; }
        }
        public DateTime CreatedOn
        {
            get { return m_createdon; }
        }
        public bool CouponSet
        {
            get { return m_couponset; }
        }
    }
    
    public class Coupons
    {
        public static CouponObject GetCoupon(SqlTransaction DBTrans, Customer ThisCustomer)
        {
            CouponObject co = new CouponObject();
            co.m_couponset = false;

            using (SqlConnection couponCon = new SqlConnection(DB.GetDBConn()))
            {
                couponCon.Open();

                string query = "select * from coupon  with (NOLOCK)  where lower(couponcode)=" + DB.SQuote(ThisCustomer.CouponCode.ToLowerInvariant());
                using (IDataReader rscoup = DB.GetRS(query, couponCon))
                {
                    if (rscoup.Read())
                    {
                        co.m_couponset = true;

                        // either consumer level, or this level allows coupons, so load it if there are any:
                        co.m_couponcode = DB.RSField(rscoup, "CouponCode");
                        co.m_coupontype = (CouponTypeEnum)DB.RSFieldInt(rscoup, "CouponType");
                        co.m_description = DB.RSField(rscoup, "Description");
                        co.m_startdate = DB.RSFieldDateTime(rscoup, "StartDate");
                        co.m_expirationdate = DB.RSFieldDateTime(rscoup, "ExpirationDate");
                        co.m_discountamount = DB.RSFieldDecimal(rscoup, "DiscountAmount");
                        co.m_discountpercent = DB.RSFieldDecimal(rscoup, "DiscountPercent");
                        co.m_discountincludesfreeshipping = DB.RSFieldBool(rscoup, "DiscountIncludesFreeShipping");
                        co.m_expiresonfirstusebyanycustomer = DB.RSFieldBool(rscoup, "ExpiresOnFirstUseByAnyCustomer");
                        co.m_expiresafteroneusagebyeachcustomer = DB.RSFieldBool(rscoup, "ExpiresAfterOneUsageByEachCustomer");
                        co.m_expiresafternuses = DB.RSFieldInt(rscoup, "ExpiresAfterNUses");
                        co.m_requiresminimumorderamount = DB.RSFieldDecimal(rscoup, "RequiresMinimumOrderAmount");

                        co.m_validforcustomers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rscoup, "ValidForCustomers"), "\\s+", "", RegexOptions.Compiled));
                        co.m_validforproducts = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rscoup, "ValidForProducts"), "\\s+", "", RegexOptions.Compiled));
                        co.m_validforcategories = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rscoup, "ValidForCategories"), "\\s+", "", RegexOptions.Compiled));
                        co.m_validforsections = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rscoup, "ValidForSections"), "\\s+", "", RegexOptions.Compiled));
                        co.m_validformanufacturers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(rscoup, "ValidForManufacturers"), "\\s+", "", RegexOptions.Compiled));

                        co.m_validforproductsexpanded = new List<int>();
                        co.m_validforcategoriesexpanded = new List<int>();
                        co.m_validforsectionsexpanded = new List<int>();
                        co.m_validformanufacturersexpanded = new List<int>();

                        co.m_deleted = DB.RSFieldBool(rscoup, "Deleted");

                        if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validforcategories.Count() > 0)
                        {
                            co.m_validforcategoriesexpanded = AppLogic.LookupHelper("Category", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validforcategories), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                            List<int> pList = AppLogic.LookupHelper("Category", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validforcategoriesexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);
                            
                            foreach (int p in pList)
                            {
                                co.m_validforproductsexpanded.Add(p);
                            }
                        }
                        if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validforsections.Count() > 0)
                        {
                            co.m_validforsectionsexpanded = AppLogic.LookupHelper("Section", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validforsections), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                            List<int> pList = AppLogic.LookupHelper("Section", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validforsectionsexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                            foreach (int p in pList)
                            {
                                co.m_validforproductsexpanded.Add(p);
                            }
                        }
                        if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validformanufacturers.Count() != 0)
                        {
                            co.m_validformanufacturersexpanded = AppLogic.LookupHelper("Manufacturer", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validformanufacturers), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                            List<int> pList = AppLogic.LookupHelper("Manufacturer", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validformanufacturersexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                            foreach (int p in pList)
                            {
                                co.m_validforproductsexpanded.Add(p);
                            }
                        }

                        if (co.m_validforproducts.Count() > 0)
                        {
                            foreach (int p in co.m_validforproducts)
                            {
                                co.m_validforproductsexpanded.Add(p);
                            }
                        }

                        co.m_numuses = DB.RSFieldInt(rscoup, "NumUses");
                    }
                    else
                    {
                        using (SqlConnection gfCon = new SqlConnection(DB.GetDBConn()))
                        {
                            gfCon.Open();
                            //Check if the item is giftcardemail or not. 
                            GiftCardTypes GiftCardTypeID = (GiftCardTypes)DB.GetSqlN(String.Format("select GiftCardTypeID N from GiftCard where lower(serialnumber)={0}", DB.SQuote(ThisCustomer.CouponCode.ToLowerInvariant())));
                            string giftCardQuery = string.Empty;

                            giftCardQuery = String.Format("select * from GiftCard  with (NOLOCK)  where StartDate<=getdate() and ExpirationDate>=getdate() and DisabledByAdministrator=0 and Balance>0 and SerialNumber={0}", DB.SQuote(ThisCustomer.CouponCode));

                            if (giftCardQuery != "")
                            {
                                using (IDataReader dr = DB.GetRS(giftCardQuery, gfCon))
                                {
                                    if (dr.Read())
                                    {
                                        co.m_couponset = true;
                                        co.m_couponcode = DB.RSField(dr, "SerialNumber");
                                        co.m_coupontype = CouponTypeEnum.GiftCard;
                                        co.m_description = "";
                                        co.m_startdate = DB.RSFieldDateTime(dr, "StartDate");
                                        co.m_expirationdate = DB.RSFieldDateTime(dr, "ExpirationDate");
                                        co.m_discountamount = DB.RSFieldDecimal(dr, "Balance");
                                        co.m_discountpercent = 0;
                                        co.m_discountincludesfreeshipping = false;
                                        co.m_expiresonfirstusebyanycustomer = false;
                                        co.m_expiresafteroneusagebyeachcustomer = false;
                                        co.m_expiresafternuses = 0;
                                        co.m_requiresminimumorderamount = System.Decimal.Zero;

                                        co.m_validforcustomers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(dr, "ValidForCustomers"), "\\s+", "", RegexOptions.Compiled));
                                        co.m_validforproducts = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(dr, "ValidForProducts"), "\\s+", "", RegexOptions.Compiled));
                                        co.m_validforcategories = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(dr, "ValidForCategories"), "\\s+", "", RegexOptions.Compiled));
                                        co.m_validforsections = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(dr, "ValidForSections"), "\\s+", "", RegexOptions.Compiled));
                                        co.m_validformanufacturers = CommonLogic.BuildListFromCommaString(Regex.Replace(DB.RSField(dr, "ValidForManufacturers"), "\\s+", "", RegexOptions.Compiled));

                                        co.m_validforproductsexpanded = new List<int>();
                                        co.m_validforcategoriesexpanded = new List<int>();
                                        co.m_validforsectionsexpanded = new List<int>();
                                        co.m_validformanufacturersexpanded = new List<int>();

                                        if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validforcategories.Count() > 0)
                                        {
                                            co.m_validforcategoriesexpanded = AppLogic.LookupHelper("Category", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validforcategories), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                            List<int> pList = AppLogic.LookupHelper("Category", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validforcategoriesexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                            foreach (int p in pList)
                                            {
                                                co.m_validforproductsexpanded.Add(p);
                                            }
                                        }
                                        if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validforsections.Count() > 0)
                                        {
                                            co.m_validforsectionsexpanded = AppLogic.LookupHelper("Section", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validforsections), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                            List<int> pList = AppLogic.LookupHelper("Section", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validforsectionsexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                            foreach (int p in pList)
                                            {
                                                co.m_validforproductsexpanded.Add(p);
                                            }
                                        }
                                        if (co.m_coupontype == CouponTypeEnum.ProductCoupon && co.m_validformanufacturers.Count() != 0)
                                        {
                                            co.m_validformanufacturersexpanded = AppLogic.LookupHelper("Manufacturer", 0).GetEntityList(CommonLogic.BuildCommaStringFromList(co.m_validformanufacturers), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                            List<int> pList = AppLogic.LookupHelper("Manufacturer", 0).GetProductList(CommonLogic.BuildCommaStringFromList(co.m_validformanufacturersexpanded), ThisCustomer.AffiliateID, ThisCustomer.CustomerLevelID);

                                            foreach (int p in pList)
                                            {
                                                co.m_validforproductsexpanded.Add(p);
                                            }
                                        }

                                        co.m_numuses = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return co;
        }

        public static String CheckIfCouponIsValidForProduct(Customer ThisCustomer, CouponObject co, CartItem ci)
        {
            String status = AppLogic.ro_OK;

            if (co.m_couponcode.Length != 0)
            {
                // we found a valid match for that coupon code with an expiration date greater than or equal to now, so check additional conditions on the coupon:
                // just return first reason for it not being valid, going from most obvious to least obvious:
                if (co.m_expirationdate == System.DateTime.MinValue || co.m_deleted)
                {
                    status = AppLogic.GetString("shoppingcart.cs.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }

                if (status == AppLogic.ro_OK)
                {
                    if (co.m_startdate > System.DateTime.Now)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }

                if (status == AppLogic.ro_OK)
                {
                    if (co.m_expirationdate < System.DateTime.Now)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.69", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_expiresonfirstusebyanycustomer && AppLogic.AnyCustomerHasUsedCoupon(co.m_couponcode))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.70", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_expiresafteroneusagebyeachcustomer && Customer.HasUsedCoupon(ThisCustomer.CustomerID, ThisCustomer.CouponCode))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.71", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_expiresafternuses > 0 && AppLogic.GetNumberOfCouponUses(ThisCustomer.CouponCode) > co.m_expiresafternuses)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.72", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_validforcustomers.Count() > 0 && !co.m_validforcustomers.Contains(ThisCustomer.CustomerID))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.74", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }

                if (status == AppLogic.ro_OK)
                {
                    try
                    {
                        if (co.m_validforproductsexpanded.Count() > 0)
                        {
                            if (!co.m_validforproductsexpanded.Contains(ci.ProductID))
                            {
                                status = AppLogic.GetString("shoppingcart.cs.75", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                            }
                        }
                        else if (co.m_validforproducts.Count() > 0)
                        {
                            if (!co.m_validforproducts.Contains(ci.ProductID))
                            {
                                status = AppLogic.GetString("shoppingcart.cs.75", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                            }
                        }
                    }
                    catch
                    {
                        status = AppLogic.GetString("shoppingcart.cs.76", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_startdate > System.DateTime.Now)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
            }
            else
            {
                try
                {
                    string query = "select * from GiftCard  with (NOLOCK)  where StartDate<=getdate() and ExpirationDate>=getdate() and DisabledByAdministrator=0 and Balance>0 and SerialNumber=" + DB.SQuote(co.m_couponcode);

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();

                        using (IDataReader rs = DB.GetRS(query, conn))
                        {
                            if (rs.Read())
                            {
                                status = AppLogic.ro_OK;
                            }
                            else
                            {
                                status = AppLogic.GetString("shoppingcart.cs.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                            }
                        }
                    }
                }
                catch { }
            }

            return status;
        }

        /// <summary>
        /// Determines if a coupon is valid for an order based on customer, customer level, coupon parameters, products, and order
        /// </summary>
        /// <param name="ThisCustomer">Customer object representing the customer making the purchase</param>
        /// <param name="co">Coupon object representing the coupon and all of its settings</param>
        /// <param name="subTotal">Subtotal of items in the shopping cart</param>
        /// <param name="subTotal">Subtotal of items in the shopping cart before any discounts have been applied</param>
        /// <returns>String 'AppLogic.ro_OK' if coupon is valid or there is no coupon, else returns reason why coupon is not valid</returns>
        public static String CheckIfCouponIsValidForOrder(Customer ThisCustomer, CouponObject co, Decimal subTotal, Decimal subTotalBeforeDiscounts)
        {
            String status = AppLogic.ro_OK;

            if (co.m_couponcode.Length != 0)
            {
                // we found a valid match for that coupon code with an expiration date greater than or equal to now, so check additional conditions on the coupon:
                // just return first reason for it not being valid, going from most obvious to least obvious:
                if (co.m_expirationdate == System.DateTime.MinValue || co.m_deleted)
                {
                    status = AppLogic.GetString("shoppingcart.cs.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                }

                if (status == AppLogic.ro_OK)
                {
                    if (co.m_startdate > System.DateTime.Now)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }

                if (status == AppLogic.ro_OK)
                {
                    if (co.m_expirationdate < System.DateTime.Now)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.69", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_expiresonfirstusebyanycustomer && AppLogic.AnyCustomerHasUsedCoupon(co.m_couponcode))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.70", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_expiresafteroneusagebyeachcustomer && Customer.HasUsedCoupon(ThisCustomer.CustomerID, ThisCustomer.CouponCode))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.71", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_expiresafternuses > 0 && AppLogic.GetNumberOfCouponUses(ThisCustomer.CouponCode) > co.m_expiresafternuses)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.72", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_requiresminimumorderamount > System.Decimal.Zero && subTotalBeforeDiscounts < co.m_requiresminimumorderamount)
                    {
                        status = String.Format(AppLogic.GetString("shoppingcart.cs.73", ThisCustomer.SkinID, ThisCustomer.LocaleSetting), ThisCustomer.CurrencyString(co.m_requiresminimumorderamount));
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_validforcustomers.Count() > 0 && !co.m_validforcustomers.Contains(ThisCustomer.CustomerID))
                    {
                        status = AppLogic.GetString("shoppingcart.cs.74", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }

                if (status == AppLogic.ro_OK)
                {
                    try
                    {
                        if (co.m_validforproductsexpanded.Count() > 0 && DB.GetSqlN("select count(productid) as N from ShoppingCart   with (NOLOCK)  where productid in (" + CommonLogic.BuildCommaStringFromList(co.m_validforproductsexpanded) + ") and CartType=" + ((int)CartTypeEnum.ShoppingCart).ToString() + " and customerid=" + ThisCustomer.CustomerID.ToString()) == 0)
                        {
                            status = AppLogic.GetString("shoppingcart.cs.75", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                        }
                    }
                    catch
                    {
                        status = AppLogic.GetString("shoppingcart.cs.76", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
                if (status == AppLogic.ro_OK)
                {
                    if (co.m_startdate > System.DateTime.Now)
                    {
                        status = AppLogic.GetString("shoppingcart.cs.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                    }
                }
            }
            else
            {
                try
                {
                    string query = "select * from GiftCard  with (NOLOCK)  where StartDate<=getdate() and ExpirationDate>=getdate() and DisabledByAdministrator=0 and Balance>0 and SerialNumber=" + DB.SQuote(co.m_couponcode);

                    using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
                    {
                        conn.Open();

                        using (IDataReader rs = DB.GetRS(query, conn))
                        {
                            if (rs.Read())
                            {
                                status = AppLogic.ro_OK;
                            }
                            else
                            {
                                status = AppLogic.GetString("shoppingcart.cs.79", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
                            }
                        }
                    }
                }
                catch {}
            }
            return status;
        }
    }
}
