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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class entityEditInventory : System.Web.UI.Page
	{
		public int VariantId { get; private set; }
		public int ProductId { get; private set; }

		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			SetProductVariantIds();

			if (!Page.IsPostBack)
			{
				StringBuilder str = new StringBuilder();
				Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

				Product product = new Product(ProductId);
				ProductVariant variant = new ProductVariant(VariantId);

				string sizeOptionPrompt = (product.SizeOptionPrompt != null && product.SizeOptionPrompt.Length > 0) ? product.SizeOptionPrompt : AppLogic.GetString("AppConfig.SizeOptionPrompt", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				string colorOptionPrompt = (product.ColorOptionPrompt != null && product.ColorOptionPrompt.Length > 0) ? product.ColorOptionPrompt : AppLogic.GetString("AppConfig.ColorOptionPrompt", ThisCustomer.SkinID, ThisCustomer.LocaleSetting);
				string productName = product.Name ?? string.Empty;
				string variantName = variant.Name ?? string.Empty;

				string entityName = CommonLogic.QueryStringCanBeDangerousContent("entityname");
				int entityId = CommonLogic.QueryStringNativeInt("entityid");

				List<InventoryItem> sizeColorCombos = GetSizeColorCombos(variant);

				DeleteNonExistingSizeColorCombos(sizeColorCombos);
				LoadInventoryGridView(sizeColorCombos);

				lnkEditingInventoryFor.Text = string.Format("{0} {1}", productName, variantName);
				lnkEditingInventoryFor.NavigateUrl = string.Format("entityEditProducts.aspx?iden={0}&entityName={1}&EntityID={2}", ProductId, entityName, entityId);
			}
		}

		protected void btnUpdate_Click(object sender, EventArgs e)
		{
			SaveInventoryRows();
		}

		private void SetProductVariantIds()
		{
			int productId = CommonLogic.QueryStringUSInt("ProductID");
			int variantId = CommonLogic.QueryStringUSInt("VariantID");

			if (variantId == 0)
			{
				ArgumentException ex = new ArgumentException("Variant ID is required.");
				SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
				throw ex;
			}

			if (productId == 0)
				productId = AppLogic.GetVariantProductID(variantId);

			VariantId = variantId;
			ProductId = productId;
		}

		private void LoadInventoryGridView(List<InventoryItem> sizeColorCombos)
		{
			grdInventory.DataSource = sizeColorCombos;
			grdInventory.DataBind();	
		}

		private void DeleteNonExistingSizeColorCombos(List<InventoryItem> sizeColorCombos)
		{
			Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

			//Build a list of Size/Color combinations that exist in the Inventory table now
			using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
			{
				conn.Open();
				using (IDataReader rs2 = DB.GetRS("select InventoryId, Color, Size from Inventory with (NOLOCK) where VariantId = @VariantId", new SqlParameter[] { new SqlParameter("@VariantID", VariantId) }, conn))
				{
					while (rs2.Read())
					{
						int inventoryId = DB.RSFieldInt(rs2, "InventoryId");
						string size = DB.RSFieldByLocale(rs2, "Size", ThisCustomer.LocaleSetting);
						string color = DB.RSFieldByLocale(rs2, "Color", ThisCustomer.LocaleSetting);
						string sizeColorCombo = size + color;

						if (!sizeColorCombos.Where(w => w.Color.Equals(color) && w.Size.Equals(size)).Any())
							DB.ExecuteSQL("delete from Inventory where InventoryId = @InventoryId", new SqlParameter[] { new SqlParameter("@InventoryId", inventoryId) });
					}
				}
			}
		}

		private List<InventoryItem> GetSizeColorCombos(ProductVariant variant)
		{
			string sizes = variant.Sizes ?? string.Empty;
			string colors = variant.Colors ?? string.Empty;

			string[] sizesList = sizes.Split(',');
			string[] colorsList = colors.Split(',');

			List<InventoryItem> sizecolorCombos = new List<InventoryItem>();

			for (int sizeIndex = sizesList.GetLowerBound(0); sizeIndex <= sizesList.GetUpperBound(0); sizeIndex++)
			{
				for (int colorIndex = colorsList.GetLowerBound(0); colorIndex <= colorsList.GetUpperBound(0); colorIndex++)
				{
					InventoryItem sizeColorCombo = new InventoryItem(AppLogic.CleanSizeColorOption(sizesList[sizeIndex]), AppLogic.CleanSizeColorOption(colorsList[colorIndex]), ProductId, VariantId);
					sizecolorCombos.Add(sizeColorCombo);
				}
			}

			return sizecolorCombos;
		}

		private void SaveInventoryRows()
		{
			foreach (GridViewRow row in grdInventory.Rows)
			{
				HiddenField hdnVariantId = (HiddenField)row.FindControl("VariantId");
				Literal ltSize = (Literal)row.FindControl("ltSize");
				Literal ltColor = (Literal)row.FindControl("ltColor");
				TextBox txtInventory = (TextBox)row.FindControl("txtInventory");
				TextBox txtGTIN = (TextBox)row.FindControl("txtGTIN");
				TextBox txtWarehouseLocation = (TextBox)row.FindControl("txtWarehouseLocation");
				TextBox txtVendorId = (TextBox)row.FindControl("txtVendorId");
				TextBox txtFullVendorSku = (TextBox)row.FindControl("txtFullVendorSku");
				TextBox txtWeightDelta = (TextBox)row.FindControl("txtWeightDelta");

				int inventory = 0;
				int.TryParse(txtInventory.Text, out inventory);

				decimal weightDelta = decimal.Zero;
				decimal.TryParse(txtWeightDelta.Text, out weightDelta);

				string size = AppLogic.CleanSizeColorOption(ltSize.Text);
				string color = AppLogic.CleanSizeColorOption(ltColor.Text);
				string gtin = txtGTIN.Text;
				string warehouseLocation = txtWarehouseLocation.Text;
				string fullSku = txtFullVendorSku.Text;
				string vendorId = txtVendorId.Text;
			
				if (DB.GetSqlN("select count(*) as N from Inventory where VariantID= @VariantId and lower([size]) = @Size and lower(color) = @Color", new SqlParameter[] { new SqlParameter("@VariantId", VariantId), new SqlParameter("@Size", size), new SqlParameter("@Color", color) }) == 0)
				{
					DB.ExecuteSQL(@"insert into Inventory(InventoryGUID,VariantID,[Size],Color,Quan,WarehouseLocation,VendorFullSKU,VendorID,WeightDelta,GTIN) 
									values(@InventoryGUID,@VariantId,@Size,@Color,@Inventory,@WarehouseLocation,@FullSku,@VendorId,@WeightDelta,@GTIN)", new SqlParameter[] {	
										new SqlParameter("@InventoryGUID", DB.GetNewGUID()), 
										new SqlParameter("@VariantId", VariantId), 
										new SqlParameter("@Size", size), 
										new SqlParameter("@Color", color), 
										new SqlParameter("@Inventory", inventory),
										new SqlParameter("@WarehouseLocation", warehouseLocation), 
										new SqlParameter("@FullSku", fullSku), 
										new SqlParameter("@VendorId", vendorId),
										new SqlParameter("@WeightDelta", weightDelta),
										new SqlParameter("@GTIN", gtin)});
				}
				else
				{
					DB.ExecuteSQL(@"update Inventory set 
									Quan = @Inventory,
									WarehouseLocation = @WarehouseLocation,
									VendorFullSKU = @FullSku,
									VendorID = @VendorId,
									WeightDelta = @WeightDelta,
									GTIN=@GTIN
									where VariantID = @VariantId and lower([size]) = @Size and lower(color) = @Color", new SqlParameter[] {	
										new SqlParameter("@InventoryGUID", DB.GetNewGUID()), 
										new SqlParameter("@VariantId", VariantId), 
										new SqlParameter("@Size", size), 
										new SqlParameter("@Color", color), 
										new SqlParameter("@Inventory", inventory),
										new SqlParameter("@WarehouseLocation", warehouseLocation), 
										new SqlParameter("@FullSku", fullSku), 
										new SqlParameter("@VendorId", vendorId),
										new SqlParameter("@WeightDelta", weightDelta),
										new SqlParameter("@GTIN", gtin)});
				}
			}
			ProductVariant variant = new ProductVariant(VariantId);
			List<InventoryItem> sizeColorCombos = GetSizeColorCombos(variant);
			LoadInventoryGridView(sizeColorCombos);
		}
}

}
