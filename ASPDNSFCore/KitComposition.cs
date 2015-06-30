// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Drawing;
using System.IO;

namespace AspDotNetStorefrontCore
{
    public class KitComposition : IEnumerable<KitCartItem>
    {
        public KitComposition(int cartId)
        {
            this.CartID = cartId;
        }

        public int CartID;
        public List<KitCartItem> Compositions = new List<KitCartItem>();

        public bool Matches(KitComposition other)
        {
            if (this.Compositions.Count != other.Compositions.Count) return false;

            bool matchesAll = false;

            foreach (KitCartItem kitItem in Compositions)
            {
                matchesAll = (other.Compositions.Find(kitItem.Match) != null);

                if (!matchesAll)
                {
                    return false;
                }
            }

            return true;
        }

        public static KitComposition FromCart(Customer thisCustomer, CartTypeEnum cartType, int cartId)
        {
            KitComposition composition = new KitComposition(cartId);

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                string query = string.Format(
                                @"SELECT 
                                    kc.ShoppingCartRecID, 
                                    kc.ProductID, 
                                    kc.VariantID, 
                                    kc.KitGroupID, 
                                    kc.KitItemID,
                                    ki.Name,
                                    kc.TextOption,
                                    kc.Quantity
                                    FROM KitCart kc WITH (NOLOCK)
                                    INNER JOIN ShoppingCart sc WITH (NOLOCK) ON kc.ShoppingCartRecID = sc.ShoppingCartRecID 
                                    INNER JOIN KitItem ki ON ki.KitItemID = kc.KitItemID
                                    WHERE kc.CustomerID = {0} AND sc.CartType = {1}  AND sc.ShoppingCartRecID = {2}", 
                                    thisCustomer.CustomerID, (int)cartType, cartId);
                using (IDataReader reader = DB.GetRS(query,dbconn))
                {
                    while (reader.Read())
                    {
                        KitCartItem kit = new KitCartItem();
                        kit.CustomerID = thisCustomer.CustomerID;
                        kit.ProductID = DB.RSFieldInt(reader, "ProductID");
                        kit.VariantID = DB.RSFieldInt(reader, "VariantID");
                        kit.KitGroupID = DB.RSFieldInt(reader, "KitGroupID");
                        kit.KitItemID = DB.RSFieldInt(reader, "KitItemID");
                        kit.Name = DB.RSFieldByLocale(reader, "Name", thisCustomer.LocaleSetting);
                        kit.TextOption = DB.RSField(reader, "TextOption");
                        kit.Quantity = DB.RSFieldInt(reader, "Quantity");

                        composition.Compositions.Add(kit);
                    }
                }

            }
      
            return composition;
        }

        public static KitComposition FromOrder(Customer thisCustomer, int orderNumber, int cartId)
        {
            KitComposition composition = new KitComposition(cartId);

            string query = string.Format("SELECT okc.ProductID, okc.VariantID, okc.KitGroupID, okc.KitItemID, okc.TextOption, okc.Quantity FROM Orders_KitCart okc INNER JOIN Orders_ShoppingCart osc ON osc.ShoppingCartRecID = okc.ShoppingCartRecID INNER JOIN Orders o ON osc.OrderNumber = o.OrderNumber WHERE o.CustomerID = {0} AND o.OrderNumber = {1} AND osc.ShoppingCartRecID = {2}", thisCustomer.CustomerID, orderNumber, cartId);

            using (SqlConnection dbconn = new SqlConnection(DB.GetDBConn()))
            {
                dbconn.Open();
                using (IDataReader reader = DB.GetRS(query,dbconn))
                {
                    while (reader.Read())
                    {
                        KitCartItem kit = new KitCartItem();
                        kit.CustomerID = thisCustomer.CustomerID;
                        kit.ProductID = DB.RSFieldInt(reader, "ProductID");
                        kit.VariantID = DB.RSFieldInt(reader, "VariantID");
                        kit.KitGroupID = DB.RSFieldInt(reader, "KitGroupID");
                        kit.KitItemID = DB.RSFieldInt(reader, "KitItemID");
                        kit.TextOption = DB.RSField(reader, "TextOption");
                        kit.Quantity = DB.RSFieldInt(reader, "Quantity");

                        composition.Compositions.Add(kit);
                    }
                }
            
            }
            
            return composition;
        }

        public static KitComposition FromForm(Customer ThisCustomer, int ProductID, int VariantID)
        {
            KitComposition composition = new KitComposition(0);

            string kitContents = CommonLogic.FormCanBeDangerousContent("KitItems");
            if (!string.IsNullOrEmpty(kitContents))
            {
                string[] selectedItems = kitContents.Split(',');
              
                foreach (string kitGroup in selectedItems)
                {
                    string[] groups = kitGroup.Split('+');
                    int groupID = int.Parse(groups[0]);
                    int itemID = int.Parse(groups[1]);

                    KitCartItem selectedKitItem = new KitCartItem();
                    selectedKitItem.CustomerID = ThisCustomer.CustomerID;
                    selectedKitItem.ProductID = ProductID;
                    selectedKitItem.VariantID = VariantID;
                    selectedKitItem.KitGroupID = groupID;
                    selectedKitItem.KitItemID = itemID;

                    composition.Compositions.Add(selectedKitItem);
                }
            }

            // process text options
            HttpContext ctx = HttpContext.Current;
            foreach (KitCartItem item in composition.Compositions)
            {
                string id = string.Format("KitItemTextOption_{0}_{1}", item.KitGroupID, item.KitItemID);
                string text = CommonLogic.FormCanBeDangerousContent(id);
                if (!CommonLogic.IsStringNullOrEmpty(text))
                {
                    item.TextOption = text;
                }
            }

            // process file upload
            foreach (KitCartItem item in composition.Compositions)
            {
                // NOTE:
                //  We don't use the group id for file upload
                string id = string.Format("KitItemFileUpload_{0}", item.KitItemID);
                HttpPostedFile file = ctx.Request.Files[id];
                if (null != file)
                {
                    item.FileName = file.FileName;
                    item.ImageFile = file;
                }
                else
                {
                    // must be edit mode
                    id = string.Format("KitItemFile_{0}", item.KitItemID);
                    string savedFile = CommonLogic.FormCanBeDangerousContent(id);
                    if (!CommonLogic.IsStringNullOrEmpty(savedFile))
                    {
                        item.TextOption = savedFile;
                    }
                }
            }

            return composition;
        }
        #region IEnumerable<KitCartItem> Members

        public IEnumerator<KitCartItem> GetEnumerator()
        {
            return Compositions.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class KitCartItem
    {
        public int ProductID;
        public int VariantID;
        public int KitGroupID;
        public int KitItemID;
        public int CustomerID;
        public string TextOption = string.Empty;
        public string FileName = string.Empty;
        public HttpPostedFile ImageFile = null;
        public int Quantity = 1;

        public bool ContentIsImage
        {
            get 
            {                
                // check the extension
                return ValidImageExtension(TextOption) && File.Exists(HttpContext.Current.Request.MapPath(TextOption));
            }
        }

        private static string[] validImageExtensions = { ".jpg", ".gif", ".png" };

        private bool ValidImageExtension(string file)
        {
            try
            {
                string fileExt = Path.GetExtension(file);
                foreach (string ext in validImageExtensions)
                {
                    if (fileExt.Equals(ext))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            

            return false;
        }

        public string Name = string.Empty;        
        public bool Match(KitCartItem other)
        {
            return this.ProductID.Equals(other.ProductID) &&
                    this.VariantID.Equals(other.VariantID) &&
                    this.KitGroupID.Equals(other.KitGroupID) &&
                    this.KitItemID.Equals(other.KitItemID) &&
                    this.CustomerID.Equals(other.CustomerID) &&
                    this.TextOption == other.TextOption;
        }
    }
}
