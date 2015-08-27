-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT. 
--
-- Database Upgrade Script:
-- AspDotNetStorefront Version 9.0 to Latest, Microsoft SQL Server 2005 Or higher
-- ------------------------------------------------------------------------------------------

/*********** ASPDOTNETSTOREFRONT v9.0 to Latest *******************/
/*                                                                */
/*                                                                */
/* BACKUP YOUR EXISTING DATABASE BEFORE RUNNING THIS SCRIPT!!     */
/* BACKUP YOUR EXISTING DATABASE BEFORE RUNNING THIS SCRIPT!!     */
/* BACKUP YOUR EXISTING DATABASE BEFORE RUNNING THIS SCRIPT!!     */
/* BACKUP YOUR EXISTING DATABASE BEFORE RUNNING THIS SCRIPT!!     */
/*                                                                */
/*                                                                */
/*                                                                */
/* ************************************************************** */

SET NOCOUNT ON
GO

PRINT '*****Database Upgrade Started*****'


/* ************************************************************** */
/* SCHEMA UPDATES												  */
/* ************************************************************** */
PRINT 'Updating GlobalConfig Table...'
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('GlobalConfig') AND name = 'IsMultiStore') 
    ALTER TABLE dbo.GlobalConfig ADD IsMultiStore [bit] NOT NULL CONSTRAINT DF_GlobalConfig_IsMultiStore DEFAULT((1))
ELSE
	declare @xtype int
	select @xtype = system_type_id from sys.types where name = 'int'
	if exists (select * from syscolumns where id = object_id('GlobalConfig') and name = 'IsMultiStore' and xtype = @xtype)
		begin 
		ALTER TABLE dbo.GlobalConfig DROP CONSTRAINT DF_GlobalConfig_IsMultiStore
		alter table dbo.GlobalConfig alter column IsMultiStore bit 	
		ALTER TABLE dbo.GlobalConfig ADD CONSTRAINT DF_GlobalConfig_IsMultiStore DEFAULT ((1)) FOR IsMultiStore
		end
GO

/* ************************************************************** */
/* STORED PROCEDURE UPDATES										  */
/* ************************************************************** */
PRINT 'Updating Stored Procedures...'


IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetStoreShippingMethodMapping') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetStoreShippingMethodMapping]
GO

create procedure [dbo].[aspdnsf_GetStoreShippingMethodMapping](
	@StoreId int,
	@IsRTShipping int = 0,
	@OnlyMapped int = 0,
	@ExcludeNameLike NVARCHAR(2000) = NULL
)	
AS
BEGIN
	
	SET NOCOUNT ON;
	
	select	@StoreId as StoreId, 			
			dbo.GetStoreMap(@storeid, 'ShippingMethod', sm.ShippingMethodId) as Mapped,
			sm.* 
	from ShippingMethod sm WITH (NOLOCK)	
	WHERE	sm.IsRTShipping = @IsRTShipping AND 
			(@OnlyMapped = 0 or (dbo.GetStoreMap(@storeid, 'ShippingMethod', sm.ShippingMethodId) = 1)) AND
			(@ExcludeNameLike IS NULL OR (sm.[Name] NOT LIKE '%' + @ExcludeNameLike + '%'))
	order by sm.DisplayOrder ASC	

END


GO



IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetFeaturedProducts') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetFeaturedProducts]
GO
	
CREATE PROCEDURE [dbo].[aspdnsf_GetFeaturedProducts](
	@FeaturedCategoryID				INT,
	@NumHomePageFeaturedProducts	INT,
	@CustomerLevelID				INT,
	@storeID						INT = 1,
	@filterProduct					BIT = 0
)
  
AS
BEGIN
	SET NOCOUNT ON

	declare @numSpecialDisplay int 

	if @NumHomePageFeaturedProducts = 0 
		select @numSpecialDisplay = configvalue from appconfig where name = 'NumHomePageSpecials'		
	else
		select @numSpecialDisplay = @NumHomePageFeaturedProducts
		
	select top (@numSpecialDisplay) 
		p.ProductID,
		p.ImageFilenameOverride,
		p.SKU,
		p.SEName,
		p.Name,
		p.Description,
		p.TaxClassID,
		pv.VariantID, 
		p.HidePriceUntilCart,
		pv.name VariantName, 
		pv.Price, 
		pv.Description VariantDescription, 
		isnull(pv.SalePrice, 0) SalePrice, 
		isnull(SkuSuffix, '') SkuSuffix, 
		pv.Dimensions, 
		pv.Weight, 
		isnull(pv.Points, 0) Points, 
		pv.Inventory, 
		pv.ImageFilenameOverride VariantImageFilenameOverride,  
		pv.isdefault, 
		pv.CustomerEntersPrice, 
		isnull(pv.colors, '') Colors, 
		isnull(pv.sizes, '') Sizes, 
		sp.name SalesPromptName,
		case when pcl.productid is null then 0 else isnull(e.Price, 0) end ExtendedPrice		   
	from ProductCategory pc with (NOLOCK) 
	inner join Product p with (NOLOCK) on pc.ProductID = p.ProductID 
	inner join ProductVariant pv with (NOLOCK) on pv.ProductID = p.ProductID and pv.IsDefault = 1
	join dbo.SalesPrompt sp with (NOLOCK) on p.SalesPromptID = sp.SalesPromptID 
	left join dbo.ExtendedPrice e  with (NOLOCK) on pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID 
	left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID 
	INNER JOIN (SELECT DISTINCT a.ProductID FROM Product a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID WHERE (@filterProduct = 0 OR StoreID = @storeID)) ps on pc.ProductID = ps.ProductID
	where pc.CategoryID=@FeaturedCategoryID and p.Deleted=0 and p.published = 1
	order by newid()

END


GO


IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetShoppingCart') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetShoppingCart]
GO
create proc [dbo].[aspdnsf_GetShoppingCart]
    @CartType tinyint, -- ShoppingCart = 0, WishCart = 1, RecurringCart = 2, GiftRegistryCart = 3
    @CustomerID int,
    @OriginalRecurringOrderNumber int,
    @OnlyLoadRecurringItemsThatAreDue tinyint,
    @StoreID int = 1
    
  
AS
BEGIN
 
    SET NOCOUNT ON 
    declare @filtershoppingcart bit, @filterproduct bit
    SELECT TOP 1 @filtershoppingcart = ConfigValue FROM GlobalConfig WHERE Name='AllowShoppingcartFiltering'
    SELECT TOP 1 @filterproduct = ConfigValue FROM GlobalConfig WHERE Name='AllowProductFiltering'
    
    SELECT
        ShoppingCart.ProductSKU,
        ShoppingCart.IsUpsell,
        ShoppingCart.Notes,
        ShoppingCart.ExtensionData,
        ShoppingCart.CustomerEntersPrice,
        ShoppingCart.NextRecurringShipDate,
        ShoppingCart.RecurringIndex,
        ShoppingCart.OriginalRecurringOrderNumber,
        ShoppingCart.RecurringSubscriptionID,
        ShoppingCart.CartType,
        ShoppingCart.ProductPrice,
        ShoppingCart.ProductWeight,
        ShoppingCart.ProductDimensions,
        ShoppingCart.SubscriptionInterval,
        ShoppingCart.SubscriptionIntervalType,
        ShoppingCart.ShoppingCartRecID,
        ShoppingCart.ProductID,
        ShoppingCart.VariantID,
        ShoppingCart.Quantity,
        ShoppingCart.IsTaxable,
        ShoppingCart.TaxClassID,
        ShoppingCart.TaxRate,
        ShoppingCart.IsShipSeparately,
        ShoppingCart.ChosenColor,
        ShoppingCart.ChosenColorSKUModifier,
        ShoppingCart.ChosenSize,
        ShoppingCart.ChosenSizeSKUModifier,
        ShoppingCart.TextOption,
        ShoppingCart.IsDownload,
        ShoppingCart.FreeShipping,
        ShoppingCart.DistributorID,
        ShoppingCart.DownloadLocation,
        ShoppingCart.CreatedOn,
        ShoppingCart.BillingAddressID as ShoppingCartBillingAddressID,
        ShoppingCart.GiftRegistryForCustomerID,
        ShoppingCart.ShippingAddressID as ShoppingCartShippingAddressID,
        ShoppingCart.ShippingMethodID,
        ShoppingCart.ShippingMethod,
        ShoppingCart.RequiresCount,
        ShoppingCart.IsSystem,
        ShoppingCart.IsAKit,
        ShoppingCart.IsAPack,
        Customer.EMail,
        Customer.OrderOptions,
        Customer.OrderNotes,
        Customer.FinalizationData,
        Customer.CouponCode,
        Customer.ShippingAddressID as
        CustomerShippingAddressID,
        Customer.BillingAddressID as CustomerBillingAddressID,
        Product.Name as ProductName,
        Product.IsSystem,
        ProductVariant.name as VariantName,
        Product.TextOptionPrompt,
        Product.SizeOptionPrompt,
        Product.ColorOptionPrompt,
        ProductVariant.CustomerEntersPricePrompt,
        Product.ProductTypeId,
        Product.TaxClassId,
        Product.ManufacturerPartNumber,
        Product.ImageFileNameOverride,
        Product.SEName,
        Product.Deleted,
        ProductVariant.Weight,
        case @CartType when 2 then ShoppingCart.RecurringInterval else productvariant.RecurringInterval end RecurringInterval,
        case @CartType when 2 then ShoppingCart.RecurringIntervalType else productvariant.RecurringIntervalType end RecurringIntervalType
 
    FROM dbo.Customer with (NOLOCK)
        join dbo.ShoppingCart with (NOLOCK) ON Customer.CustomerID = ShoppingCart.CustomerID
        join dbo.Product with (NOLOCK) on ShoppingCart.ProductID=Product.ProductID
        left join dbo.ProductVariant with (NOLOCK) on ShoppingCart.VariantID=ProductVariant.VariantID
        left join dbo.Address with (NOLOCK) on Customer.ShippingAddressID=Address.AddressID
        left join dbo.country c on c.name = Address.country 
        left join dbo.State with (nolock) ON Address.State = State.Abbreviation and State.countryid = c.countryid
		inner join (select distinct a.ProductID,a.StoreID from ShoppingCart a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterproduct = 0 or a.StoreID = b.StoreID)) productstore
        on ShoppingCart.ProductID = productstore.ProductID and ShoppingCart.StoreID = productstore.StoreID
        
    WHERE ShoppingCart.CartType = @CartType
        and Product.Deleted in (0,2)
        and ProductVariant.Deleted = 0
        and Customer.customerid = @CustomerID
        and (@OriginalRecurringOrderNumber = 0 or ShoppingCart.OriginalRecurringOrderNumber = @OriginalRecurringOrderNumber)
        and (@OnlyLoadRecurringItemsThatAreDue = 0 or (@CartType = 2 and NextRecurringShipDate < dateadd(dy, 1, getdate())))
        and (@filtershoppingcart = 0 or ShoppingCart.StoreID = @StoreID)
		AND (AvailableStopDate IS NULL OR AvailableStopDate > GetDate())
     ORDER BY ShoppingCart.GiftRegistryForCustomerID,ShoppingCart.ShippingAddressID
 
END

GO


IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_GetProducts') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetProducts]
GO

create proc [dbo].[aspdnsf_GetProducts]
    @categoryID      int = null,  
    @sectionID       int = null,  
    @manufacturerID  int = null,  
    @distributorID   int = null,  
    @genreID         int = null,  
    @vectorID        int = null,  
    @localeID        int = null,  
    @CustomerLevelID int = null,  
    @affiliateID     int = null,  
    @ProductTypeID   int = null,  
    @ViewType        bit = 1, -- 0 = all variants, 1 = one variant  
    @sortEntity      int = 0, -- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector  
    @pagenum         int = 1,  
    @pagesize        int = null,  
    @StatsFirst      tinyint = 1,  
    @searchstr       nvarchar(4000) = null,  
    @extSearch       tinyint = 0,  
    @publishedonly   tinyint = 0,  
    @ExcludePacks    tinyint = 0,  
    @ExcludeKits     tinyint = 0,  
    @ExcludeSysProds tinyint = 0,  
    @InventoryFilter int = 0,  --  will only show products with an inventory level GREATER OR EQUAL TO than the number specified in this parameter, set to -1 to disable inventory filtering  
    @sortEntityName  varchar(20) = '', -- usely only when the entity id is provided, allowed values: category, section, manufacturer, distributor, genre, vector  
    @localeName      varchar(20) = '',  
    @OnSaleOnly      tinyint = 0,  
    @IncludeAll      bit = 0, -- Don't filter products that have a start date in the future or a stop date in the past  
	@storeID		 int = 1,
	@filterProduct	 bit = 0,
	@sortby			 varchar(10) = 'default',
	@since			 int = 180  -- best sellers in the last "@since" number of days
	
  
AS  
BEGIN  
  
    SET NOCOUNT ON   
  
    DECLARE @rcount int
    DECLARE @productfiltersort table (rownum int not null identity  primary key, productid int not null, price money null, saleprice money null, displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)
    DECLARE @productfilter table (rownum int not null identity  primary key, productid int not null, price money null, saleprice money null,  displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)
	DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @DisplayOutOfStockProducts tinyint, @HideProductsWithLessThanThisInventoryLevel int  
    CREATE TABLE #displayorder ([name] nvarchar (800), productid int not null primary key, displayorder int not null)  
    CREATE TABLE #inventoryfilter (productid int not null, variantid int not null, InvQty int not null)  
    CREATE CLUSTERED INDEX tmp_inventoryfilter ON #inventoryfilter (productid, variantid)  
  
    DECLARE @custlevelcount int, @sectioncount int, @localecount int, @affiliatecount int, @categorycount int, @CustomerLevelFilteringIsAscending bit, @distributorcount int, @genrecount int, @vectorcount int, @manufacturercount int  
  
	DECLARE @ftsenabled tinyint
	
	SET @ftsenabled = 0
	
	IF ((SELECT DATABASEPROPERTY(db_name(db_id()),'IsFulltextEnabled')) = 1 
		AND EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[KeyWordSearch]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
		AND EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetValidSearchString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')))
	BEGIN
		SET @ftsenabled = 1
	END
  
    SELECT @FilterProductsByAffiliate = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate'  
    SELECT @FilterProductsByCustomerLevel = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel'  
    SELECT @DisplayOutOfStockProducts = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'DisplayOutOfStockProducts'
    SELECT @HideProductsWithLessThanThisInventoryLevel = convert(int, ConfigValue) FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' and isnumeric(ConfigValue) = 1  
  
    IF @DisplayOutOfStockProducts = 1 
	BEGIN
		SET @HideProductsWithLessThanThisInventoryLevel = 0
		SET @InventoryFilter = -1
	END
    
    IF @InventoryFilter <> -1 and (@HideProductsWithLessThanThisInventoryLevel > @InventoryFilter or @HideProductsWithLessThanThisInventoryLevel  = -1)  
        SET @InventoryFilter  = @HideProductsWithLessThanThisInventoryLevel  
  
  
    SET @categoryID      = nullif(@categoryID, 0)  
    SET @sectionID       = nullif(@sectionID, 0)  
    SET @manufacturerID  = nullif(@manufacturerID, 0)  
    SET @distributorID   = nullif(@distributorID, 0)  
    SET @genreID         = nullif(@genreID, 0)  
    SET @vectorID        = nullif(@vectorID, 0)  
    SET @affiliateID     = nullif(@affiliateID, 0)  
    SET @ProductTypeID   = nullif(@ProductTypeID, 0)  
  
  
    SET @CustomerLevelFilteringIsAscending  = 0  
    SELECT @CustomerLevelFilteringIsAscending  = case configvalue when 'true' then 1 else 0 end  
    FROM dbo.AppConfig with (nolock)   
    WHERE name = 'FilterByCustomerLevelIsAscending'  
  
    IF @localeID is null and ltrim(rtrim(@localeName)) <> ''  
        SELECT @localeID = LocaleSettingID FROM dbo.LocaleSetting with (nolock) WHERE Name = ltrim(rtrim(@localeName))  
  
    select @categorycount     = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productcategory') and si.indid < 2 and type = 'u'  
    select @sectioncount      = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productsection') and si.indid < 2 and type = 'u'  
    select @localecount       = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductLocaleSetting') and si.indid < 2 and type = 'u'  
    select @custlevelcount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductCustomerLevel') and si.indid < 2 and type = 'u'  
    select @affiliatecount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductAffiliate') and si.indid < 2 and type = 'u'  
    select @distributorcount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductDistributor') and si.indid < 2 and type = 'u'  
    select @genrecount        = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductGenre') and si.indid < 2 and type = 'u'  
    select @vectorcount       = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductVector') and si.indid < 2 and type = 'u'  
    select @manufacturercount = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductManufacturer') and si.indid < 2 and type = 'u'  
  
  
    -- get page size  
    IF @pagesize is null or @pagesize = 0 BEGIN  
        IF @categoryID is not null  
            SELECT @pagesize = PageSize FROM dbo.Category with (nolock) WHERE categoryID = @categoryID  
        ELSE IF @sectionID is not null  
            SELECT @pagesize = PageSize FROM dbo.Section with (nolock) WHERE sectionID = @sectionID  
        ELSE IF @manufacturerID is not null  
            SELECT @pagesize = PageSize FROM dbo.Manufacturer with (nolock) WHERE manufacturerID = @manufacturerID  
        ELSE IF @distributorID is not null  
            SELECT @pagesize = PageSize FROM dbo.Distributor with (nolock) WHERE distributorID = @distributorID  
        ELSE IF @genreID is not null  
            SELECT @pagesize = PageSize FROM dbo.Genre with (nolock) WHERE genreID = @genreID  
        ELSE IF @vectorID is not null  
            SELECT @pagesize = PageSize FROM dbo.Vector with (nolock) WHERE vectorID = @vectorID  
        ELSE   
            SELECT @pagesize = convert(int, ConfigValue) FROM dbo.AppConfig with (nolock) WHERE [Name] = 'Default_CategoryPageSize'  
    END  
  
    IF @pagesize is null or @pagesize = 0  
        SET @pagesize = 20  
  
    -- get sort order  
    IF @sortEntity = 1 or @sortEntityName = 'category' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductCategory a with (nolock) inner join (select distinct a.ProductID from ProductCategory a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b  on a.ProductID = b.ProductID where categoryID = @categoryID 
    END  
    ELSE IF @sortEntity = 2 or @sortEntityName = 'section' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductSection a with (nolock) inner join (select distinct a.ProductID from ProductSection a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where sectionId = @sectionID
    END  
    ELSE IF @sortEntity = 3 or @sortEntityName = 'manufacturer' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductManufacturer a with (nolock) inner join (select distinct a.ProductID from ProductManufacturer a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where ManufacturerID = @manufacturerID
    END  
    ELSE IF @sortEntity = 4 or @sortEntityName = 'distributor' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductDistributor a with (nolock) inner join (select distinct a.ProductID from ProductDistributor a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where DistributorID = @distributorID
    END  
    ELSE IF @sortEntity = 5 or @sortEntityName = 'genre' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductGenre a with (nolock) inner join (select distinct a.ProductID from ProductGenre a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where GenreID = @genreID
    END  
    ELSE IF @sortEntity = 6 or @sortEntityName = 'vector' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductVector a with (nolock) inner join (select distinct a.ProductID from ProductVector a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where VectorID = @vectorID
    END  
    ELSE BEGIN  
        INSERT #displayorder select distinct [name], a.productid, 1 from dbo.Product a with (nolock) inner join (select distinct a.ProductID from Product a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID ORDER BY Name  
    END  

	IF (@ftsenabled = 1)
	BEGIN
		IF rtrim(isnull(@searchstr, '')) <> ''
		BEGIN
			DECLARE @tmpsrch nvarchar(4000)
			SET @tmpsrch = dbo.GetValidSearchString(@searchstr) 
			DELETE #displayorder from #displayorder d left join dbo.KeyWordSearch(@tmpsrch) k on d.productid = k.productid where k.productid is null  
		END
	END
	
	SET @searchstr = '%' + rtrim(ltrim(@searchstr)) + '%' 
 
    IF @InventoryFilter <> -1 BEGIN  
        IF @ViewType = 1 BEGIN  
            INSERT #inventoryfilter  
            SELECT p.productid, pv.VariantID, sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) invqty  
            FROM product p with (NOLOCK) join #displayorder d on p.ProductID = d.ProductID  
                join ProductVariant pv with (NOLOCK) on p.ProductID = pv.ProductID  and pv.IsDefault = 1  
                left join Inventory i with (NOLOCK) on pv.VariantID = i.VariantID  
            GROUP BY p.productid, pv.VariantID  
            HAVING sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) >= @InventoryFilter  
        END  
        ELSE  
            INSERT #inventoryfilter  
            SELECT p.productid, pv.VariantID, sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) invqty  
            FROM product p with (NOLOCK) join #displayorder d on p.ProductID = d.ProductID  
                join ProductVariant pv with (NOLOCK) on p.ProductID = pv.ProductID  
                left join Inventory i with (NOLOCK) on pv.VariantID = i.VariantID  
            GROUP BY p.productid, pv.VariantID  
            HAVING sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) >= @InventoryFilter  
  

        INSERT @productfilter (productid, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)  
        SELECT distinct p.productid, do.displayorder, pv.VariantID, pv.DisplayOrder, p.Name, pv.Name  
        FROM   
            product p with (nolock)  
            join #displayorder do on p.ProductID = do.ProductID   
            left join ProductVariant pv        with (NOLOCK) ON p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
            left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID   
            left join productsection ps        with (nolock) on p.ProductID = ps.ProductID   
            left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID   
            left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID   
            left join ProductGenre px          with (nolock) on p.ProductID = px.ProductID   
            left join ProductVector px2        with (nolock) on p.ProductID = px2.ProductID   
            left join ProductLocaleSetting pl  with (nolock) on p.ProductID = pl.ProductID   
            left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID   
            left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID
			   
            join #inventoryfilter i on pv.VariantID = i.VariantID  
        WHERE   
              (pc.categoryid = @categoryID or @categoryID is null or @categorycount = 0)  
          and (ps.sectionid = @sectionID or @sectionID is null or @sectioncount = 0)  
          and (pl.LocaleSettingID = @localeID or @localeID is null or @localecount = 0)  
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)  
          and (pm.manufacturerid = @manufacturerID or @manufacturerID is null or @manufacturercount = 0)  
          and (pd.DistributorID = @distributorID or @distributorID is null or @distributorcount = 0)  
          and (px.GenreID = @genreID or @genreID is null or @genrecount = 0)  
          and (px2.VectorID = @vectorID or @vectorID is null or @vectorcount = 0)  
          and p.ProductTypeID = coalesce(@ProductTypeID, p.ProductTypeID)  
          and (case   
                when @FilterProductsByCustomerLevel = 0 or @custlevelcount = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1  
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1   
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1   
                else 0  
               end  = 1  
              )  
          and (@ftsenabled = 1 or
				(@ftsenabled = 0 and
					(@searchstr is null  
					 or patindex(@searchstr, isnull(p.name, '')) > 0  
					 or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0   
					 or patindex(@searchstr, isnull(pv.name, '')) > 0  
					 or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0  
					 or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0  
					 or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0  
					 or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)  
					 or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)  
					)
				)
              )  
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= @OnSaleOnly  
          and p.published >= @publishedonly  
          and pv.published >= @publishedonly  
          and isnull(p.IsAPack, 0) <= 1-@ExcludePacks  
          and isnull(p.IsAKit, 0) <= 1-@ExcludeKits  
          and p.IsSystem <= 1-@ExcludeSysProds  
          and p.Deleted = 0  
          and pv.Deleted = 0  
          and ((@IncludeAll = 1) or (getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999')))    
        order by do.displayorder, p.Name, pv.DisplayOrder, pv.Name  
  
    END   
    ELSE BEGIN  
        INSERT @productfilter (productid, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)  
        SELECT distinct p.productid, do.displayorder, pv.VariantID, pv.DisplayOrder, p.Name, pv.Name  
        FROM   
            product p with (nolock)  
            join #displayorder do on p.ProductID = do.ProductID   
            join ProductVariant pv             with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
            left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID   
            left join productsection ps        with (nolock) on p.ProductID = ps.ProductID   
            left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID   
            left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID   
            left join ProductGenre px          with (nolock) on p.ProductID = px.ProductID   
            left join ProductVector px2        with (nolock) on p.ProductID = px2.ProductID   
            left join ProductLocaleSetting pl  with (nolock) on p.ProductID = pl.ProductID   
            left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID   
            left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID   
        WHERE   
              (pc.categoryid = @categoryID or @categoryID is null or @categorycount = 0)  
          and (ps.sectionid = @sectionID or @sectionID is null or @sectioncount = 0)  
          and (pl.LocaleSettingID = @localeID or @localeID is null or @localecount = 0)  
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)  
          and (pm.manufacturerid = @manufacturerID or @manufacturerID is null or @manufacturercount = 0)  
          and (pd.DistributorID = @distributorID or @distributorID is null or @distributorcount = 0)  
          and (px.GenreID = @genreID or @genreID is null or @genrecount = 0)  
          and (px2.VectorID = @vectorID or @vectorID is null or @vectorcount = 0)  
          and p.ProductTypeID = coalesce(@ProductTypeID, p.ProductTypeID)  
          and (case   
                when @FilterProductsByCustomerLevel = 0 or @custlevelcount = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1  
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1   
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1   
                else 0  
               end  = 1  
              )  
          and (@ftsenabled = 1 or
				(@ftsenabled = 0 and
					(@searchstr is null  
					or patindex(@searchstr, isnull(p.name, '')) > 0  
					or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0   
					or patindex(@searchstr, isnull(pv.name, '')) > 0  
					or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0  
					or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0  
					or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0  
					or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)  
					or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)  
					)
				)
              )  
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= @OnSaleOnly  
          and p.published >= @publishedonly  
          and pv.published >= @publishedonly  
          and isnull(p.IsAPack, 0) <= 1-@ExcludePacks  
          and isnull(p.IsAKit, 0) <= 1-@ExcludeKits  
          and p.IsSystem <= 1-@ExcludeSysProds  
          and p.Deleted = 0  
          and pv.Deleted = 0  
          and ((@IncludeAll = 1) or (getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999')))    
        order by do.displayorder, p.Name, pv.DisplayOrder, pv.Name  
    END  
	
    SET @rcount = @@rowcount  
    IF @StatsFirst = 1  
        SELECT cast(ceiling(@rcount*1.0/@pagesize) as int) pages, @rcount ProductCount  
  
  
  --Begin sorting
  	if @sortby = 'bestseller'
		begin
			insert @productfiltersort (productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName) 
			select pf.productid, pf.price, pf.saleprice, pf.displayorder, pf.VariantID, pf.VariantDisplayOrder, pf.ProductName, pf.VariantName
				from @productfilter pf
				left join (
					select ProductID, SUM(Quantity) AS NumSales
					  from dbo.Orders_ShoppingCart sc with (NOLOCK) 
							join [dbo].Orders o with (NOLOCK)  on sc.OrderNumber = o.OrderNumber and o.OrderDate >= dateadd(dy, -@since, getdate())
					  group by ProductID 
				) bsSort on pf.productid = bsSort.ProductID
				order by isnull(bsSort.NumSales, 0) DESC
		end
  	else --default
		begin
			insert @productfiltersort (productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName) 
			select productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName
			from @productfilter order by displayorder, productName, variantDisplayOrder, variantName
		end
    
    SELECT   
        p.ProductID,  
        p.Name,  
        pv.VariantID,  
        pv.Name AS VariantName,  
        p.ProductGUID,  
        p.Summary,  
        p.Description,  
        p.SEKeywords,  
        p.SEDescription,  
        p.SpecTitle,  
        p.MiscText,  
        p.SwatchImageMap,  
        p.IsFeaturedTeaser,  
        p.FroogleDescription,  
        p.SETitle,  
        p.SENoScript,  
        p.SEAltText,  
        p.SizeOptionPrompt,  
        p.ColorOptionPrompt,  
        p.TextOptionPrompt,  
        p.ProductTypeID,  
        p.TaxClassID,  
        p.SKU,  
        p.ManufacturerPartNumber,  
        p.SalesPromptID,  
        p.SpecCall,  
        p.SpecsInline,  
        p.IsFeatured,  
        p.XmlPackage,  
        p.ColWidth,  
        p.Published,  
        p.RequiresRegistration,  
        p.Looks,  
        p.Notes,  
        p.QuantityDiscountID,  
        p.RelatedProducts,  
        p.UpsellProducts,  
        p.UpsellProductDiscountPercentage,  
        p.RelatedDocuments,  
        p.TrackInventoryBySizeAndColor,  
        p.TrackInventoryBySize,  
        p.TrackInventoryByColor,  
        p.IsAKit,  
        p.ShowInProductBrowser,  
        p.IsAPack,  
        p.PackSize,  
        p.ShowBuyButton,  
        p.RequiresProducts,  
        p.HidePriceUntilCart,  
        p.IsCalltoOrder,  
        p.ExcludeFromPriceFeeds,  
        p.RequiresTextOption,  
        p.TextOptionMaxLength,  
        p.SEName,  
        p.Deleted,  
        p.CreatedOn,  
        p.ImageFileNameOverride,  
        pv.VariantGUID,  
        pv.Description AS VariantDescription,  
        pv.SEKeywords AS VariantSEKeywords,  
        pv.SEDescription AS VariantSEDescription,  
        pv.Colors,  
        pv.ColorSKUModifiers,  
        pv.Sizes,  
        pv.SizeSKUModifiers,  
        pv.FroogleDescription AS VariantFroogleDescription,  
        pv.SKUSuffix,  
        pv.ManufacturerPartNumber AS VariantManufacturerPartNumber,  
        pv.Price,  
        pv.CustomerEntersPrice,   
        pv.CustomerEntersPricePrompt,  
        isnull(pv.SalePrice, 0) SalePrice,  
        cast(isnull(pv.Weight,0) as decimal(10,1)) Weight,  
        pv.MSRP,  
        pv.Cost,  
        isnull(pv.Points,0) Points,  
        pv.Dimensions,  
        case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end Inventory,  
        pv.DisplayOrder as VariantDisplayOrder,  
        pv.Notes AS VariantNotes,  
        pv.IsTaxable,  
        pv.IsShipSeparately,  
        pv.IsDownload,  
        pv.DownloadLocation,  
        pv.Published AS VariantPublished,  
        pv.IsSecureAttachment,  
        pv.IsRecurring,  
        pv.RecurringInterval,  
        pv.RecurringIntervalType,  
        pv.SubscriptionInterval,  
        pv.SEName AS VariantSEName,  
        pv.RestrictedQuantities,  
        pv.MinimumQuantity,  
        pv.Deleted AS VariantDeleted,  
        pv.CreatedOn AS VariantCreatedOn,  
        d.Name AS DistributorName,  
        d.DistributorID,  
        d.SEName AS DistributorSEName,  
        m.ManufacturerID,  
        m.Name AS ManufacturerName,  
        m.SEName AS ManufacturerSEName,  
        s.Name AS SalesPromptName,  
        case when pcl.productid is null then 0 else isnull(ep.Price, 0) end ExtendedPrice  
    FROM dbo.Product p with (NOLOCK)   
        left join dbo.ProductVariant       pv  with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
        join @productfiltersort            pf                on pv.ProductID = pf.ProductID and pv.VariantID = pf.VariantID   
        left join dbo.SalesPrompt           s  with (NOLOCK) on p.SalesPromptID = s.SalesPromptID   
        left join dbo.ProductManufacturer  pm  with (NOLOCK) on p.ProductID = pm.ProductID   
        left join dbo.Manufacturer          m  with (NOLOCK) on pm.ManufacturerID = m.ManufacturerID   
        left join dbo.ProductDistributor   pd  with (NOLOCK) on p.ProductID = pd.ProductID  
        left join dbo.Distributor           d  with (NOLOCK) on pd.DistributorID = d.DistributorID  
        left join dbo.ExtendedPrice        ep  with (NOLOCK) on ep.VariantID = pv.VariantID and ep.CustomerLevelID = @CustomerLevelID  
        left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID  
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID  
    WHERE pf.rownum >= @pagesize*(@pagenum-1)+1 and pf.rownum <= @pagesize*(@pagenum)  
    ORDER BY pf.rownum  
  
    IF @StatsFirst <> 1  
        SELECT cast(ceiling(@rcount*1.0/@pagesize) as int) pages, @rcount ProductCount  
  
END  


GO

/* ************************************************************** */
/* DATA UPDATES													  */
/* ************************************************************** */

-- Update store version
PRINT 'Updating Store Version...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = '9.0.1.2' WHERE [Name] = 'StoreVersion'

-- Type some AppConfigs
PRINT 'Updating AppConfig Settings...'
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'Newsletter.UseCaptcha'
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'Newsletter.GetFirstAndLast'
UPDATE dbo.[AppConfig] SET ValueType = 'decimal' WHERE [Name] = 'Newsletter.CaptchaErrorDisplayLength'
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'Newsletter.OptInLevel'
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'Newsletter.OptOutLevel'
UPDATE dbo.[AppConfig] SET AllowableValues = 'single, double, triple' WHERE [Name] = 'Newsletter.OptInLevel'
UPDATE dbo.[AppConfig] SET AllowableValues = 'double, triple' WHERE [Name] = 'Newsletter.OptOutLevel'

-- Add the topic required for the new contact page
IF EXISTS (SELECT * FROM Topic WHERE Name='ContactEmail')
BEGIN
PRINT 'ContactEmail topic exists already'
END
ELSE
BEGIN
PRINT 'Adding ContactEmail topic'
INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('ContactEmail', 1, 0, 'ContactEmail', '<table style="width: 400px;"><tbody><tr><td align="right" style="width: 150px;">Customer Name: </td><td align="left">%NAME%</td></tr><tr><td align="right" style="width: 150px;">Customer Email: </td><td align="left">%EMAIL%</td></tr><tr><td align="right" style="width: 150px;">Customer Phone:</td><td align="left">%PHONE%</td></tr><tr><td colspan="2"> </td></tr><tr><td colspan="2"><b>%SUBJECT%</b></td></tr><tr><td colspan="2" style="padding-top: 3px;">%MESSAGE%</td></tr></tbody></table>')
END

-- Create a default record in the stores table if one does not exist
PRINT 'Creating Default Store Record...'
IF (SELECT COUNT(*) FROM dbo.Store) = 0
BEGIN
	INSERT INTO dbo.Store(	StoreGUID,
							ProductionURI,
							StagingURI,
							DevelopmentURI,
							[Name],
							Summary,
							Description,
							Published,
							Deleted,
							SkinID,
							IsDefault,
							CreatedOn)
			Values(	newid(),
					'www.mystore.com',
					'staging.mystore.com',
					'localhost',
					'Default Store',
					'',
					'',
					1,
					0,
					1,
					1,
					getdate())
END

GO

-- Remove invalid mobile device useragent
PRINT 'Updating Mobile Devices...'
DELETE [dbo].[MobileDevice] WHERE UserAgent=N'tosh'
DELETE [dbo].[MobileDevice] WHERE UserAgent=N'wc3'
DELETE [dbo].[MobileDevice] WHERE UserAgent=N'oper'
GO

PRINT 'Updating Global Configs...'
-- Create globalconfig parameter for switching masterpages by locale
IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'AllowTemplateSwitchingByLocale') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) VALUES('AllowTemplateSwitchingByLocale', 'DISPLAY', 'Indicator of whether the site should attempt to load different masterpage skins based on the current locale of the browsing customer.  This should only be enabled if you have multiple locales and have created different masterpages for each of your locales (eg. template.en-us.master, template.en-gb.master, etc...).  Enabling this when you do not have multiple locales or when you have not create multiple masterpages that vary by locale can hinder the performance of your site.', 'false', 'boolean', 'false')
END
GO

PRINT 'Updating Topics Table to work with Duplicate Topic Names and Delete Name Constraint'
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('Topic') AND name = 'StoreID')
    ALTER TABLE dbo.Topic ADD StoreID [int] NOT NULL CONSTRAINT DF_Topic_StoreID DEFAULT((0))
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Topic]') AND name = N'UIX_Topic_Name')
	DROP INDEX [UIX_Topic_Name] ON [dbo].[Topic] WITH ( ONLINE = OFF )
GO

-- Update AllowCustomerDuplicateEmailAddresses globalconfig parameter
UPDATE GlobalConfig SET [IsMultiStore] = 0 WHERE Name='AllowCustomerDuplicateEmailAddresses'
GO

-- Populate the ProductStore table
-- Insert deleted products as well in case they are undeleted
PRINT 'Populating Stores Table...'
INSERT INTO ProductStore (ProductID) SELECT ProductID FROM Product WHERE ProductID NOT IN (SELECT ProductID FROM ProductStore)
GO

--Update Croatia so that it works for real time shipping
UPDATE [dbo].Country set name = 'Croatia' where name = 'Croatia (local Name: Hrvatska)'
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.API.AcceleratedBoardingEmailAddress') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.API.AcceleratedBoardingEmailAddress','GATEWAY','If you enter your email address PayPal will allow you to start taking orders without yet having an account. You must register for an account to collect funds captured within 30 days. After registering you should enter your API creds into the appropriate app configs.','');
END
GO

UPDATE [dbo].AppConfig SET configvalue = 'http://www.sitemaps.org/schemas/sitemap/0.9' WHERE name = 'GoogleSiteMap.Xmlns' and configvalue = 'http://www.google.com/schemas/sitemap/1.0'
GO

UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPServerGateway.asp?service=VendorAbortTx' WHERE name = 'SagePayUKURL.Simulator.Abort' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorAbortTx'
UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPDirectCallback.asp' WHERE name = 'SagePayUKURL.Simulator.Callback' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPDirectCallback.asp'
UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPDirectGateway.asp' WHERE name = 'SagePayUKURL.Simulator.Purchase' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPDirectGateway.asp'
UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPServerGateway.asp?service=VendorRefundTx' WHERE name = 'SagePayUKURL.Simulator.Refund' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorRefundTx'
UPDATE [dbo].AppConfig SET configvalue = 'https://test.sagepay.com/simulator/VSPServerGateway.asp?service=VendorReleaseTx' WHERE name ='SagePayUKURL.Simulator.Release' and configvalue = 'https://ukvpstest.protx.com/VSPSimulator/VSPServerGateway.asp?service=VendorReleaseTx'
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.API.AcceleratedBoardingEmailAddress') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.API.AcceleratedBoardingEmailAddress','GATEWAY','If you enter your email address PayPal will allow you to start taking orders without yet having an account. You must register for an account to collect funds captured within 30 days. After registering you should enter your API creds into the appropriate app configs.','');
END

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_CreateFeed') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_CreateFeed]
GO

create proc [dbo].[aspdnsf_CreateFeed]
    @StoreID int,
    @Name nvarchar(100),
    @DisplayOrder int,
    @XmlPackage nvarchar(100),
    @CanAutoFTP tinyint,
    @FTPUsername nvarchar(100),
    @FTPPassword nvarchar(100),
    @FTPServer nvarchar(100),
    @FTPPort int,
    @FTPFilename nvarchar(100),
    @ExtensionData ntext,
    @FeedID int OUTPUT
  
AS
BEGIN
SET NOCOUNT ON 

IF isnull(@XmlPackage, '') = '' BEGIN
    RAISERROR('XmlPAckage is required', 16, 1)
    RETURN
END

IF @CanAutoFTP > 1  
    SET @CanAutoFTP = 1


    
INSERT dbo.Feed(FeedGUID, StoreID, Name, DisplayOrder, XmlPackage, CanAutoFTP, FTPUsername, FTPPassword, FTPServer, FTPPort, FTPFilename, ExtensionData, CreatedOn)
VALUES (newid(), @StoreID, @Name, isnull(@DisplayOrder,1), @XmlPackage, isnull(@CanAutoFTP,0), @FTPUsername, @FTPPassword, @FTPServer, @FTPPort, @FTPFilename, @ExtensionData, getdate())
set @FeedID = @@identity
END

GO

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].aspdnsf_UpdFeed') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_UpdFeed]
GO
create proc [dbo].[aspdnsf_UpdFeed]
    @FeedID int,
    @StoreID int,
    @Name nvarchar(100),
    @DisplayOrder int,
    @XmlPackage nvarchar(100),
    @CanAutoFTP tinyint,
    @FTPUsername nvarchar(100),
    @FTPPassword nvarchar(100),
    @FTPServer nvarchar(100),
    @FTPPort int,
    @FTPFilename nvarchar(100),
    @ExtensionData ntext
  
AS
SET NOCOUNT ON 


IF isnull(@XmlPackage, '') = '' BEGIN
    RAISERROR('XmlPAckage is required', 16, 1)
    RETURN
END
    
IF @CanAutoFTP > 1  
    SET @CanAutoFTP = 1

UPDATE dbo.Feed
SET 
    StoreID = COALESCE(@StoreID, StoreID),
    Name = COALESCE(@Name, Name),
    DisplayOrder = COALESCE(@DisplayOrder, DisplayOrder),
    XmlPackage = COALESCE(@XmlPackage, XmlPackage),
    CanAutoFTP = COALESCE(@CanAutoFTP, CanAutoFTP),
    FTPUsername = COALESCE(@FTPUsername, FTPUsername),
    FTPPassword = COALESCE(@FTPPassword, FTPPassword),
    FTPServer = COALESCE(@FTPServer, FTPServer),
    FTPPort = COALESCE(@FTPPort, FTPPort),
    FTPFilename = COALESCE(@FTPFilename, FTPFilename),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData)
WHERE FeedID = @FeedID





GO

/* ************************************************************** */
/* Admin Service Pack  											  */
/* ************************************************************** */
/****** Object:  StoredProcedure [dbo].[aspdnsf_GetMappedObjects]    Script Date: 11/17/2010 13:44:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[aspdnsf_GetMappedObjects](
	@StoreId int,
	@EntityType nvarchar(30),
	@AlphaFilter nvarchar(30) = null,
	@SearchFilter nvarchar(30) = null,
	@pageSize int = null,
	@page int = null,
	@IsLegacyCacheMechanism bit = 1.
)
as
begin

	-- In an effort to elliminate the insanly slow load time of the store entity caching mechanism, the default returns of this stored procedure returns bunk data.
	-- In admin, the entity object mapping controls will switch this off to work correctly.
	if (@IsLegacyCacheMechanism = 1)
		begin

			select	0 as TotalCount, 0 as PageSize, 0 as CurrentPage, 0 as TotalPages, 0 as StartIndex, 0 as EndIndex
			select	0 as StoreID, 0, '' as EntityType, 0 as [ID], '' as [Name], 0 as Mapped

		end
	else
		begin

		declare @count int
		declare @allPages int
		declare @start int
		declare @end int

		-- flag to determine if we should do paging
		declare @doPage bit
		set @doPage = case when @pageSize is null and @page is null then 0 else 1 end	

		-- execute query to fetch the count of all availalble data
		-- which we will use later on to get the paging information
		select @count = count(*)
		from
		(
			select	o.EntityType, 			
					o.[Id],
					o.[Name]
			from ObjectView o
			where	o.EntityType = COALESCE(@EntityType, o.EntityType) and 
					(@AlphaFilter IS NULL OR (o.[Name] like @AlphaFilter + '%')) and 
					(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
		) ov

		-- sanity check
		if(@count <= @pageSize) set @pageSize = @count

		-- determine start and end index	
		set @start = ((@page - 1) * @pageSize) + 1
		set @end = (@start + @pageSize) - 1
		if(@end > @count) set @end = @count

		-- compute the total number of pages
		if(@count > 0 ) 
		begin
			set @allPages = @count / @pageSize

			declare @rem int
			set @rem = @count % @pageSize -- residue
			if(@rem > 0) set @allPages = @allPages + 1
		end 
		else set @allPages = 0
		
		-- paging information
		select	@count as TotalCount, 
				@pageSize as PageSize, 
				@page as CurrentPage, 
				@allPages as TotalPages, 
				@start as StartIndex, 
				@end as [EndIndex]
			
		-- actual paged result set
		select	@StoreId as StoreID,
				ROW_NUMBER,
				ov.EntityType, 			
				ov.[ID],
				ov.[Name],
				dbo.GetStoreMap(@StoreId, ov.EntityType, ov.ID) as Mapped
		from
		(
			select	ROW_NUMBER() over(partition by o.EntityType order by id) as [Row_Number], 
					o.EntityType, 			
					o.[Id],
					o.[Name]
			from ObjectView o
			where	o.EntityType = COALESCE(@EntityType, o.EntityType) and 
					(@AlphaFilter IS NULL OR (o.[Name] like @AlphaFilter + '%')) and 
					(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
		) ov
		where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)
	end
end
go

ALTER PROCEDURE [dbo].[aspdnsf_updGridProductVariant]
(
	@variantID int,
	@name nvarchar(255) = NULL,
	@description nvarchar(max),
	@skuSuffix nvarchar(50) = NULL,
	@Price money = NULL,
	@SalePrice money = NULL,
	@Inventory int = NULL,
	@deleted tinyint = 0,
	@published tinyint = 1
)
AS
BEGIN
SET NOCOUNT ON

UPDATE [ProductVariant] SET
	[Name] = COALESCE(@name, [Name]),
	[Description] = COALESCE(@description, [Description]),
	[SKUSuffix] = COALESCE(@skuSuffix, [skuSuffix]),
	[Price] = COALESCE(@Price, [Price]),
	[SalePrice] = COALESCE(@SalePrice, [SalePrice]),
	[Inventory] = COALESCE(@Inventory,[Inventory]),
	[Deleted] = @deleted,
	[Published] = @published
WHERE [VariantID] = @VariantID

END


GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'DefaultWidth_micro') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(0,'DefaultWidth_micro','IMAGERESIZE','Default width of an micro image if no width attribute is specified in the other size-configs (i.e. width:50;).  This value should NOT be left blank.','50')
END

update AppConfig set ValueType = 'string' where Name = 'ShippingMethodIDIfFreeShippingIsOn'
GO

ALTER proc [dbo].[aspdnsf_CloneProduct]
    @productID int,
    @userid int = 0, 
    @cloneinventory int = 1
  
AS
BEGIN

SET NOCOUNT ON

DECLARE @tmpKitTbl TABLE(KitGroupID int not null)
DECLARE @tmpPVariantTbl TABLE(VariantID int not null)
DECLARE @newproductID int
DECLARE @err int, @newkitgroupid int

SET @newproductID = -1

BEGIN TRAN
    INSERT [dbo].product (ProductGUID, Name, Summary, Description, SEKeywords, SEDescription, SpecTitle, MiscText, SwatchImageMap, IsFeaturedTeaser, FroogleDescription, SETitle, SENoScript, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, SpecCall, SpecsInline, IsFeatured, XmlPackage, ColWidth, Published, RequiresRegistration, Looks, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, IsAPack, PackSize, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
    SELECT newid(), Name + ' - CLONED', Summary, Description, SEKeywords, SEDescription, SpecTitle, MiscText, SwatchImageMap, IsFeaturedTeaser, FroogleDescription, SETitle, SENoScript, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, SpecCall, SpecsInline, IsFeatured, XmlPackage, ColWidth, 0, RequiresRegistration, 0, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, IsAPack, PackSize, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, getdate()
    FROM dbo.product
    WHERE productid = @productID
    
    SELECT @newproductID = @@identity, @err = @@error

    IF @err <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -1
    END

    IF @cloneinventory = 1 BEGIN
        DECLARE @PrdVariantID int, @newvariantID int
        INSERT @tmpPVariantTbl SELECT VariantID FROM dbo.productvariant  WHERE productid = @productID
        SELECT top 1 @PrdVariantID = VariantID FROM @tmpPVariantTbl
        WHILE @@rowcount <> 0 BEGIN

            INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, SubscriptionInterval, SubscriptionIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
            SELECT newid(), @newproductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, SubscriptionInterval, SubscriptionIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, getdate()
            FROM dbo.productvariant
            WHERE VariantID = @PrdVariantID

            SELECT @newvariantID = @@identity, @err = @@error
        
            IF @err <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -2
            END


            INSERT [dbo].Inventory (InventoryGUID, VariantID, Color, Size, Quan, CreatedOn) 
            SELECT newid(), @newvariantID, Color, Size, Quan, getdate()
            FROM dbo.Inventory 
            WHERE VariantID = @PrdVariantID

            IF @@error <> 0 BEGIN
                raiserror('Product not cloned', 1, 16)
                rollback tran
                return -14
            END
        
            DELETE @tmpPVariantTbl where VariantID = @PrdVariantID
            SELECT top 1 @PrdVariantID = VariantID from @tmpPVariantTbl
        END
    END
    ELSE BEGIN

        INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, SubscriptionInterval, SubscriptionIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
        SELECT newid(), @newproductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, SubscriptionInterval, SubscriptionIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, getdate()
        FROM dbo.productvariant
        WHERE productid = @productID
        
        IF @@error <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -2
        END
    
    END


    DECLARE @kitgrpid int
    INSERT @tmpKitTbl select KitGroupID FROM kitgroup  where productid = @productID
    SELECT top 1 @kitgrpid = KitGroupID FROM @tmpKitTbl
    WHILE @@rowcount <> 0 BEGIN
        INSERT [dbo].kitgroup (KitGroupGUID, Name, Description, ProductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, CreatedOn)
        SELECT newid(), Name, Description, @newproductID, DisplayOrder, KitGroupTypeID, IsRequired, ExtensionData, getdate()
        FROM dbo.kitgroup
        WHERE KitGroupID = @kitgrpid
    
        SELECT @newkitgroupid = @@identity, @err = @@error
    
        IF @err <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -3
        END
    
        
        INSERT [dbo].kititem (KitItemGUID, KitGroupID, Name, Description, PriceDelta, IsDefault, DisplayOrder, TextOptionMaxLength, TextOptionWidth, TextOptionHeight, ExtensionData, InventoryVariantID, InventoryVariantColor, InventoryVariantSize, CreatedOn)
        SELECT newid(), @newkitgroupid, kititem.Name, kititem.Description, kititem.PriceDelta, kititem.IsDefault, kititem.DisplayOrder, kititem.TextOptionMaxLength, kititem.TextOptionWidth, kititem.TextOptionHeight, kititem.ExtensionData, kititem.InventoryVariantID, kititem.InventoryVariantColor, kititem.InventoryVariantSize, getdate()
        FROM dbo.kititem 
        WHERE KitGroupID = @kitgrpid
    
        IF @@error <> 0 BEGIN
            raiserror('Product not cloned', 1, 16)
            rollback tran
            return -6
        END
    
        DELETE @tmpKitTbl WHERE KitGroupID = @kitgrpid
        SELECT top 1 @kitgrpid = KitGroupID FROM @tmpKitTbl
    END
    
    INSERT [dbo].productcategory (ProductID, CategoryID, CreatedOn)
    SELECT @newproductID, CategoryID, getdate()
    FROM dbo.productcategory
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -5
    END
    
	
    
    INSERT [dbo].productsection (ProductID, SectionID, CreatedOn)
    SELECT @newproductID, SectionID, getdate()
    FROM dbo.productsection
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -7
    END
    
    
    INSERT [dbo].productaffiliate (ProductID, AffiliateID, CreatedOn)
    SELECT @newproductID, AffiliateID, getdate()
    FROM dbo.productaffiliate
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -9
    END
    
    INSERT [dbo].productcustomerlevel (ProductID, CustomerLevelID, CreatedOn)
    SELECT @newproductID, CustomerLevelID, getdate()
    FROM dbo.productcustomerlevel
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -10 
    END
    
    INSERT [dbo].productlocalesetting (ProductID, LocaleSettingID, CreatedOn)
    SELECT @newproductID, LocaleSettingID, getdate()
    FROM dbo.productlocalesetting
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -11 
    END
    
    INSERT [dbo].ProductManufacturer (ManufacturerID, ProductID, DisplayOrder, CreatedOn)
    SELECT ManufacturerID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productmanufacturer
    WHERE productid = @productID

    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -12
    END
    
	
    INSERT [dbo].ProductDistributor (DistributorID, ProductID, DisplayOrder, CreatedOn)
    SELECT DistributorID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productdistributor
    WHERE productid = @productID

    INSERT [dbo].ProductGenre (GenreID, ProductID, DisplayOrder, CreatedOn)
    SELECT GenreID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productgenre
    WHERE productid = @productID

    INSERT [dbo].ProductVector (VectorID, ProductID, DisplayOrder, CreatedOn)
    SELECT VectorID, @newproductID, DisplayOrder, getdate()
    FROM dbo.productvector
    WHERE productid = @productID
    
    INSERT [dbo].ProductStore (ProductId, StoreId, CreatedOn)
    SELECT @newproductID, StoreId, getdate()
    FROM dbo.ProductStore
    WHERE productid = @productID
    
    IF @@error <> 0 BEGIN
        raiserror('Product not cloned', 1, 16)
        rollback tran
        return -13
    END


    -- return one result row with new Product ID
    select @newproductID
    

COMMIT TRAN

END
GO

update appconfig set description = 'If you do not want to allow, or cannot allow (e.g. using FEDEX) shipping to PO Boxes, set this flag to true, and the store will TRY to prevent users from entering PO Box shipping addresses. It is NOT 100% guaranteed. Users enter all types of values.' where name = 'DisallowShippingToPOBoxes'
/* ************************************************************** */
/* Version 9.1       											  */
/* ************************************************************** */
--new default store id on app configs of 0
if not exists(select * from [dbo].[AppConfig] where storeid = 0)
BEGIN
update [dbo].[AppConfig] set StoreID = 0 where StoreID = (select StoreID from [dbo].[Store] where IsDefault = 1)
END

--new app configs will now be added with default storeid 0
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
ALTER TABLE [dbo].[AppConfig]
	DROP CONSTRAINT DF_AppConfig_StoreID
GO
ALTER TABLE [dbo].[AppConfig] ADD CONSTRAINT
	DF_AppConfig_StoreID DEFAULT ((0)) FOR StoreID
GO

--these changes still need to be merged into the create script
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - This app config has been left for backwards compatability, but, as of SP1, is no longer used by AspDotNetStorefront. Default Skin Id is now set per store under Configuration -> Store Maintenance -> Domains under store settings.' where [Name] = 'DefaultSkinID'
update [dbo].[AppConfig] set [Description] = 'The category ID that is considered to be ''Featured Products''. Products mapped to this category get a Featured status. This category ID provides an additional way for consumers to find the ''specials''. The ''is featured'' category Name typically has a Name like ''Specials'', ''On Sale Now'', ''Blowout Specials'', etc...' where [Name] = 'IsFeaturedCategoryID'

if not exists(select * from [dbo].[AppConfig] where [Name] = 'DefaultLocale')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) 
	VALUES (N'DefaultLocale', N'The default locale. If empty this will default to the value in the web.config. Note that changes to this app config will not take full effect until the site is restarted.', N'string', NULL, N'SETUP', 1, 0)
END

if not exists(select * from [dbo].[AppConfig] where [Name] = 'Paymentech.UseVerifiedByVisa')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
	VALUES (N'Paymentech.UseVerifiedByVisa', N'Whether or not the paymentech gateway should use verified by Visa.', N'boolean', NULL, N'GATEWAY', 1, 0, 'false')
END

if not exists(select * from [dbo].[AppConfig] where [Name] = 'Paging.ShowAllPageLinks')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
	VALUES (N'Paging.ShowAllPageLinks', N'If true all pages will be shown in paging links.', N'boolean', NULL, N'SITEDISPLAY', 0, 0, 'false')
END


if not exists(select * from [dbo].[AppConfig] where [Name] = 'Paging.PagesForward')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
	VALUES (N'Paging.PagesForward', N'The number of forward pages to show if Paging.ShowAllPageLinks is set to false.', N'integer', NULL, N'SITEDISPLAY', 0, 0, '3')
END


if not exists(select * from [dbo].[AppConfig] where [Name] = 'Paging.PagesBackward')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
	VALUES (N'Paging.PagesBackward', N'The number of backwards pages to show if Paging.ShowAllPageLinks is set to false.', N'integer', NULL, N'SITEDISPLAY', 0, 0, '3')
END


UPDATE [dbo].[AppConfig] set [ConfigValue] = N'MONERIS' WHERE [CONFIGVALUE] like N'ESELECTPLUS'
UPDATE [dbo].[AppConfig] set [ConfigValue] = N'PAYPAL' WHERE [CONFIGVALUE] like N'PAYPALPRO'
UPDATE [dbo].[AppConfig] set [ConfigValue] = N'SAGEPAYUK' WHERE [CONFIGVALUE] like N'PROTX'
UPDATE [dbo].[AppConfig] set [ConfigValue] = N'QBMERCHANTSERVICES' WHERE [CONFIGVALUE] like N'QUICKBOOKS'

UPDATE [dbo].[AppConfig] set [ConfigValue] = N'https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor/' WHERE [Name] = N'CYBERSOURCE.PITURL'

update [dbo].[Country] set PostalCodeRegex = N'^[0-9]{4,4}(-[0-9]{3,3}){0,1}$', PostalCodeExample = N'#### or ####-###' where Name = N'Portugal' and (PostalCodeRegex = N'' or PostalCodeRegex is null)

if not exists(select * from [dbo].[AppConfig] where [Name] = 'RTSHIPPING.FedEx.Key')
	INSERT INTO [dbo].[AppConfig] ([SuperOnly],[Name],[GroupName],[Description],[ConfigValue])
	values(0,'RTSHIPPING.FedEx.Key', 'RTSHIPPING','FedEx account key given to you from FedEx.', '')
go
	
if not exists(select * from [dbo].[AppConfig] where [Name] = 'RTSHIPPING.FedEx.Password')
	INSERT INTO [dbo].[AppConfig] ([SuperOnly],[Name],[GroupName],[Description],[ConfigValue])
 	values(0,'RTSHIPPING.FedEx.Password', 'RTSHIPPING','FedEx password given to you from FedEx. This is givent to you when you generate you key', '')
go

if exists(select * from [dbo].[AppConfig] where [Name] = 'RTSHIPPING.FedEx.Server')
	Delete [dbo].[AppConfig] where [Name] = 'RTSHIPPING.FedEx.Server'
 go
INSERT INTO [dbo].[AppConfig] ([SuperOnly],[Name],[GroupName],[Description],[ConfigValue])
values(0,'RTShipping.FedEx.Server','RTSHIPPING','Your FedEx Server Assigned by FedEX. The Server URl is CaSE SeNSitIVe!!! Your URL may DIfFeR frOm THis One!','https://gateway.fedex.com:443/web-services');
go


if not exists (select * From syscolumns where id = object_id('Orders') and name = 'ReceiptHtml') begin
	ALTER TABLE [dbo].[Orders] ADD ReceiptHtml ntext
end
go

 IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[aspdnsf_getOrder]') AND type in (N'P', N'PC'))
        DROP PROCEDURE [dbo].[aspdnsf_getOrder]
    GO

create proc dbo.aspdnsf_getOrder
    @ordernumber int
  
AS
SET NOCOUNT ON 

SELECT 
    o.OrderNumber,
    o.OrderGUID,
    o.ParentOrderNumber,
    o.StoreVersion,
    o.QuoteCheckout,
    o.IsNew,
    o.ShippedOn,
    o.CustomerID,
    o.CustomerGUID,
    o.Referrer,
    o.SkinID,
    o.LastName,
    o.FirstName,
    o.Email,
    o.Notes,
    o.BillingEqualsShipping,
    o.BillingLastName,
    o.BillingFirstName,
    o.BillingCompany,
    o.BillingAddress1,
    o.BillingAddress2,
    o.BillingSuite,
    o.BillingCity,
    o.BillingState,
    o.BillingZip,
    o.BillingCountry,
    o.BillingPhone,
    o.ShippingLastName,
    o.ShippingFirstName,
    o.ShippingCompany,
    o.ShippingResidenceType,
    o.ShippingAddress1,
    o.ShippingAddress2,
    o.ShippingSuite,
    o.ShippingCity,
    o.ShippingState,
    o.ShippingZip,
    o.ShippingCountry,
    o.ShippingMethodID,
    o.ShippingMethod,
    o.ShippingPhone,
    o.ShippingCalculationID,
    o.Phone,
    o.RegisterDate,
    o.AffiliateID,
    o.CouponCode,
    o.CouponType,
    o.CouponDescription,
    o.CouponDiscountAmount,
    o.CouponDiscountPercent,
    o.CouponIncludesFreeShipping,
    o.OkToEmail,
    o.Deleted,
    o.CardType,
    o.CardName,
    o.CardNumber,
    o.CardExpirationMonth,
    o.CardExpirationYear,
    o.OrderSubtotal,
    o.OrderTax,
    o.OrderShippingCosts,
    o.OrderTotal,
    o.PaymentGateway,
    o.AuthorizationCode,
    o.AuthorizationResult,
    o.AuthorizationPNREF,
    o.TransactionCommand,
    o.OrderDate,
    o.LevelID,
    o.LevelName,
    o.LevelDiscountPercent,
    o.LevelDiscountAmount,
    o.LevelHasFreeShipping,
    o.LevelAllowsQuantityDiscounts,
    o.LevelHasNoTax,
    o.LevelAllowsCoupons,
    o.LevelDiscountsApplyToExtendedPrices,
    o.LastIPAddress,
    o.PaymentMethod,
    o.OrderNotes,
    o.PONumber,
    o.DownloadEmailSentOn,
    o.ReceiptEmailSentOn,
    o.DistributorEmailSentOn,
    o.ShippingTrackingNumber,
    o.ShippedVIA,
    o.CustomerServiceNotes,
    o.RTShipRequest,
    o.RTShipResponse,
    o.TransactionState,
    o.AVSResult,
    o.CaptureTXCommand,
    o.CaptureTXResult,
    o.VoidTXCommand,
    o.VoidTXResult,
    o.RefundTXCommand,
    o.RefundTXResult,
    o.CardinalLookupResult,
    o.CardinalAuthenticateResult,
    o.CardinalGatewayParms,
    o.AffiliateCommissionRecorded,
    o.OrderOptions,
    o.OrderWeight,
    o.eCheckBankABACode,
    o.eCheckBankAccountNumber,
    o.eCheckBankAccountType,
    o.eCheckBankName,
    o.eCheckBankAccountName,
    o.CarrierReportedRate,
    o.CarrierReportedWeight,
    o.LocaleSetting,
    o.FinalizationData,
    o.ExtensionData,
    o.AlreadyConfirmed,
    o.CartType,
    o.THUB_POSTED_TO_ACCOUNTING,
    o.THUB_POSTED_DATE,
    o.THUB_ACCOUNTING_REF,
    o.Last4,
    o.ReadyToShip,
    o.IsPrinted,
    o.AuthorizedOn,
    o.CapturedOn,
    o.RefundedOn,
    o.VoidedOn,
    o.EditedOn,
    o.InventoryWasReduced,
    o.MaxMindFraudScore,
    o.MaxMindDetails,
    o.CardStartDate,
    o.CardIssueNumber,
    o.TransactionType,
    o.Crypt,
    o.VATRegistrationID,
    o.FraudedOn,
    o.RefundReason,
    o.AuthorizationPNREF as TransactionID,
    o.RecurringSubscriptionID,
    o.RelatedOrderNumber,
    o.ReceiptHtml,

    os.SubscriptionInterval,
    os.SubscriptionIntervalType,
    os.ShoppingCartRecID,
    os.IsTaxable,
    os.IsShipSeparately,
    os.IsDownload,
    os.DownloadLocation,
    os.FreeShipping,
    os.DistributorID,
    os.ShippingDetail,
    os.TaxClassID,
    os.TaxRate,
    os.Notes,
    os.CustomerEntersPrice,
    os.ProductID,
    os.VariantID,
    os.Quantity,
    os.ChosenColor,
    os.ChosenColorSKUModifier,
    os.ChosenSize,
    os.ChosenSizeSKUModifier,
    os.TextOption,
    os.SizeOptionPrompt,
    os.ColorOptionPrompt,
    os.TextOptionPrompt,
    os.CustomerEntersPricePrompt,
    os.OrderedProductQuantityDiscountID,
    os.OrderedProductQuantityDiscountName,
    os.OrderedProductQuantityDiscountPercent,
    os.OrderedProductName,
    os.OrderedProductVariantName,
    os.OrderedProductSKU,
    os.OrderedProductManufacturerPartNumber ,
    os.OrderedProductPrice,
    os.OrderedProductWeight,
    os.OrderedProductPrice,
    os.ShippingMethodID,
    os.GiftRegistryForCustomerID,
    os.ShippingAddressID,
    os.IsAKit,
    os.IsAPack
FROM Orders o with (nolock) 
    left join orders_ShoppingCart os with (nolock) ON os.OrderNumber = o.OrderNumber 
WHERE o.OrderNumber = @ordernumber 
ORDER BY os.GiftRegistryForCustomerID,os.ShippingAddressID


GO





delete from [dbo].[appconfig] where [Name] = 'RTShipping.Fedex.ShipDate'
delete from [dbo].[appconfig] where [Name] = 'RTShipping.Fedex.CarrierCodes'



/* ************************************************************** */
/* Version 9.2       											  */
/* ************************************************************** */

update [dbo].[AppConfig] set configvalue = '9.2.0.0' where name = 'StoreVersion'
GO

--buysafe global configs
delete from AppConfig where Name like 'BuySafe.%'

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.Enabled') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) 
	VALUES('BuySafe.Enabled', 'BUYSAFE', N'To enable BuySafe set this to true. If this is false, BuySafe will be disabled.', 'false', 'boolean', 'false')
END
GO


IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.EndPoint') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) 
	VALUES('BuySafe.EndPoint', 'BUYSAFE', N'The buySAFE API endpoint.', 'https://api.buysafe.com/BuySafeWS/RegistrationAPI.dll', 'string', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.UserName') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) 
	VALUES('BuySafe.UserName', 'BUYSAFE', N'Your BuySafe user name', '', 'string', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.Hash') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) 
	VALUES('BuySafe.Hash', 'BUYSAFE', N'The buySAFE Hash value is the unique identifier for your buySAFE account. Please contact buySAFE if you have any questions.', '', 'string', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.GMS') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) 
	VALUES('BuySafe.GMS', 'BUYSAFE', N'Your BuySafe GMS', '1000.00', 'decimal', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.TrialStartDate') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) 
	VALUES('BuySafe.TrialStartDate', 'BUYSAFE', N'The date on which your 30 BuySafe trial started', '', 'string', 'false')
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'BuySafe.RollOverJSLocation') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) 
	VALUES('BuySafe.RollOverJSLocation', 'BUYSAFE', N'Do not change this value.', 'https://seal.buysafe.com/private/rollover/rollover.js', 'string', 'false')
END
GO

if not exists(select * from [dbo].[AppConfig] where [Name] = 'BuySafe.DisableAddoToCartKicker')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
	VALUES (N'BuySafe.DisableAddoToCartKicker', N'If this is set to true the buySafe kicker will not be shown on product pages.', N'boolean', NULL, N'BUYSAFE', 1, 0, 'false')
END



if not exists(select * from [dbo].[AppConfig] where [Name] = 'BuySafe.KickerType')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
	VALUES (N'BuySafe.KickerType', N'The value of the request to buysafe that defines the type of kicker to show. Alternate types can be found here: http://www.buysafe.com/web/general/kickerpreview.aspx.', N'string', NULL, N'BUYSAFE', 1, 0, 'Kicker Guaranteed Ribbon 200x90')
END

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[StringResource]') AND name = N'UIX_StringResource_Name_LocaleSetting')
	DROP INDEX UIX_StringResource_Name_LocaleSetting ON [dbo].StringResource WITH ( ONLINE = OFF )
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[StringResource]') AND name = N'UIX_StringResource_Name_LocaleSetting_StoreId')
	CREATE UNIQUE INDEX [UIX_StringResource_Name_LocaleSetting_StoreId] ON [StringResource]([Name],[LocaleSetting],[StoreID]);
GO

ALTER proc [dbo].[aspdnsf_updCustomer]
    @CustomerID int,
    @CustomerLevelID int = null,
    @Email nvarchar(100) = null,
    @Password nvarchar(250) = null,
    @SaltKey int = null,
    @DateOfBirth datetime = null,
    @Gender nvarchar(1) = null,
    @FirstName nvarchar(100) = null,
    @LastName nvarchar(100) = null,
    @Notes ntext = null,
    @SkinID int = null,
    @Phone nvarchar(25) = null,
    @AffiliateID int = null,
    @Referrer ntext = null,
    @CouponCode nvarchar(50) = null,
    @OkToEmail tinyint = null,
    @IsAdmin tinyint = null,
    @BillingEqualsShipping tinyint = null,
    @LastIPAddress varchar(40) = null,
    @OrderNotes ntext = null,
    @SubscriptionExpiresOn datetime = null,
    @RTShipRequest ntext = null,
    @RTShipResponse ntext = null,
    @OrderOptions ntext = null,
    @LocaleSetting nvarchar(10) = null,
    @MicroPayBalance money = null,
    @RecurringShippingMethodID int = null,
    @RecurringShippingMethod nvarchar(100) = null,
    @BillingAddressID int = null,
    @ShippingAddressID int = null,
    @GiftRegistryGUID uniqueidentifier = null,
    @GiftRegistryIsAnonymous tinyint = null,
    @GiftRegistryAllowSearchByOthers tinyint = null,
    @GiftRegistryNickName nvarchar(100) = null,
    @GiftRegistryHideShippingAddresses tinyint = null,
    @CODCompanyCheckAllowed tinyint = null,
    @CODNet30Allowed tinyint = null,
    @ExtensionData ntext = null,
    @FinalizationData ntext = null,
    @Deleted tinyint = null,
    @Over13Checked tinyint = null,
    @CurrencySetting nvarchar(10) = null,
    @VATSetting int = null,
    @VATRegistrationID nvarchar(100) = null,
    @StoreCCInDB tinyint = null,
    @IsRegistered tinyint = null,
    @LockedUntil datetime = null,
    @AdminCanViewCC tinyint = null,
    @BadLogin smallint = 0, --only pass -1 = null, 0 = null, or 1: -1 clears the field = null, 0 does nothing = null, 1 increments the field by one
    @Active tinyint = null,
    @PwdChangeRequired tinyint = null,
    @RegisterDate datetime = null,
    @RequestedPaymentMethod  nvarchar(100) = null,
    @StoreID	int = null
    
  
AS
SET NOCOUNT ON 

DECLARE @OldPwd nvarchar(100), @IsAdminCust tinyint, @OldSaltKey int

SELECT @OldPwd = Password, @IsAdminCust = IsAdmin, @OldSaltKey = Saltkey FROM dbo.Customer WHERE CustomerID = @CustomerID


UPDATE dbo.Customer
SET 
    CustomerLevelID = COALESCE(@CustomerLevelID, CustomerLevelID),
    RegisterDate = COALESCE(@RegisterDate, RegisterDate),
    Email = COALESCE(@Email, Email),
    Password = COALESCE(@Password, Password),
    SaltKey = COALESCE(@SaltKey, SaltKey),
    DateOfBirth = COALESCE(@DateOfBirth, DateOfBirth),
    Gender = COALESCE(@Gender, Gender),
    FirstName = COALESCE(@FirstName, FirstName),
    LastName = COALESCE(@LastName, LastName),
    Notes = COALESCE(@Notes, Notes),
    SkinID = COALESCE(@SkinID, SkinID),
    Phone = COALESCE(@Phone, Phone),
    AffiliateID = COALESCE(@AffiliateID, AffiliateID),
    Referrer = COALESCE(@Referrer, Referrer),
    CouponCode = COALESCE(@CouponCode, CouponCode),
    OkToEmail = COALESCE(@OkToEmail, OkToEmail),
    IsAdmin = COALESCE(@IsAdmin, IsAdmin),
    BillingEqualsShipping = COALESCE(@BillingEqualsShipping, BillingEqualsShipping),
    LastIPAddress = COALESCE(@LastIPAddress, LastIPAddress),
    OrderNotes = COALESCE(@OrderNotes, OrderNotes),
    SubscriptionExpiresOn = COALESCE(@SubscriptionExpiresOn, SubscriptionExpiresOn),
    RTShipRequest = COALESCE(@RTShipRequest, RTShipRequest),
    RTShipResponse = COALESCE(@RTShipResponse, RTShipResponse),
    OrderOptions = COALESCE(@OrderOptions, OrderOptions),
    LocaleSetting = COALESCE(@LocaleSetting, LocaleSetting),
    MicroPayBalance = COALESCE(@MicroPayBalance, MicroPayBalance),
    RecurringShippingMethodID = COALESCE(@RecurringShippingMethodID, RecurringShippingMethodID),
    RecurringShippingMethod = COALESCE(@RecurringShippingMethod, RecurringShippingMethod),
    BillingAddressID = COALESCE(@BillingAddressID, BillingAddressID),
    ShippingAddressID = COALESCE(@ShippingAddressID, ShippingAddressID),
    GiftRegistryGUID = COALESCE(@GiftRegistryGUID, GiftRegistryGUID),
    GiftRegistryIsAnonymous = COALESCE(@GiftRegistryIsAnonymous, GiftRegistryIsAnonymous),
    GiftRegistryAllowSearchByOthers = COALESCE(@GiftRegistryAllowSearchByOthers, GiftRegistryAllowSearchByOthers),
    GiftRegistryNickName = COALESCE(@GiftRegistryNickName, GiftRegistryNickName),
    GiftRegistryHideShippingAddresses = COALESCE(@GiftRegistryHideShippingAddresses, GiftRegistryHideShippingAddresses),
    CODCompanyCheckAllowed = COALESCE(@CODCompanyCheckAllowed, CODCompanyCheckAllowed),
    CODNet30Allowed = COALESCE(@CODNet30Allowed, CODNet30Allowed),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData),
    FinalizationData = COALESCE(@FinalizationData, FinalizationData),
    Deleted = COALESCE(@Deleted, Deleted),
    Over13Checked = COALESCE(@Over13Checked, Over13Checked),
    CurrencySetting = COALESCE(@CurrencySetting, CurrencySetting),
    VATSetting = COALESCE(@VATSetting, VATSetting),
    VATRegistrationID = COALESCE(@VATRegistrationID, VATRegistrationID),
    StoreCCInDB = COALESCE(@StoreCCInDB, StoreCCInDB),
    IsRegistered = COALESCE(@IsRegistered, IsRegistered),
    LockedUntil = COALESCE(@LockedUntil, LockedUntil),
    AdminCanViewCC = COALESCE(@AdminCanViewCC, AdminCanViewCC),
    PwdChanged = case when @OldPwd <> @Password and @Password is not null then getdate() else PwdChanged end,
    BadLoginCount = case @BadLogin when -1 then 0 else BadLoginCount + @BadLogin end,
    LastBadLogin = case @BadLogin when -1 then null when 1 then getdate() else LastBadLogin end,
    Active = COALESCE(@Active, Active),
    PwdChangeRequired = COALESCE(@PwdChangeRequired, PwdChangeRequired),
    RequestedPaymentMethod = COALESCE(@RequestedPaymentMethod, RequestedPaymentMethod),
    StoreID = COALESCE(@StoreID, StoreID)
WHERE CustomerID = @CustomerID

IF @IsAdminCust > 0 and @OldPwd <> @Password
    INSERT dbo.PasswordLog (CustomerID, OldPwd, SaltKey, ChangeDt)
    VALUES (@CustomerID, @OldPwd, @OldSaltKey, getdate())
GO

IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter', 1, 0, '	Newsletter	', '	<HTML> <HEAD> <TITLE></TITLE> </HEAD> <BODY class=RadEContent> %NewsletterBody% <DIV id=NewsletterFooter>You have recieved this newsletter because you are subscribed to the %CompanyName% newsletter. Click <A href="%UnsubscribeURL%">here</A> to unsubscribe. </DIV> </BODY> </HTML> ')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='NewsletterEmail')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('NewsletterEmail', 1, 0, '	NewsletterEmail	', ' <HTML>%NewsletterBody%<div id="NewsletterFooter">You have recieved this newsletter because you are subscribed to the %CompanyName% newsletter.Click <a href="%UnsubscribeURL%">here to unsubscribe.</a></div></HTML>	')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='NewsletterOptInEmail')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('NewsletterOptInEmail', 1, 0, '	NewsletterOptInEmail	', '	<HTML>You have recieved this email because you requested to be subscribed to the %CompanyName% newsletter.Click <a href="%SubscribeURL%">here</a> to confirm your email address.</HTML>	')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter.OptInOutBadRequest')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter.OptInOutBadRequest', 1, 0, ' Newsletter.OptInOutBadRequest	', '	The address you''re attempting to confirm is invalid or has already been confirmed.	')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter.ConfirmOptOut')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter.ConfirmOptOut', 1, 0, '	Newsletter.ConfirmOptOut	', ' Are you sure you wish to be removed from our newsletter mailing list?	')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter.ConfirmOptIn')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter.ConfirmOptIn', 1, 0, '	Newsletter.ConfirmOptIn	', '	Click confirm to subscribe to our newsletter.	')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter.AddressErrorBlock')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter.AddressErrorBlock', 1, 0, '	Newsletter.AddressErrorBlock ', '	<table> <tr class="captchaBox"> <td align="center">The email address you entered was in an invalid format.</td> </tr> </table>	')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter.CaptchaErrorBlock')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter.CaptchaErrorBlock', 1, 0, '	Newsletter.CaptchaErrorBlock ', '	<table> <tr class="captchaBox"> <td align="center">The captcha characters you entered were incorrect.</td> </tr> </table>	')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter.SubscribeSuccessful')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter.SubscribeSuccessful', 1, 0, 'Newsletter.SubscribeSuccessful', 'Thank you, you''re now subscribed to our newsletter. ')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter.SubscribeConfirm')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter.SubscribeConfirm', 1, 0, 'Newsletter.SubscribeConfirm', 'Thank you. An email confirming your subscription is being sent.')
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Newsletter.UnsubscribeSuccessful')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Newsletter.UnsubscribeSuccessful', 1, 0, 'Newsletter.UnsubscribeSuccessful', 'You have now unsubscribed from our newsletter. ')
GO



if not exists(select * from [dbo].[AppConfig] where [Name] = 'DisablePasswordAutocomplete')
BEGIN
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
	VALUES (N'DisablePasswordAutocomplete', N'If this is true the attribute autocomplete="false" will be added to password text boxes. This will stop browsers from prepopulating login credentials.', N'boolean', NULL, N'SECURITY', 1, 0, 'false')
END
GO



if not exists (select * From sysobjects where id = object_id('ErrorMessage') and type = 'u')
BEGIN
    CREATE TABLE [dbo].[ErrorMessage](
		[MessageId] [int] IDENTITY(1,1) NOT NULL,
		[Message] [nvarchar](max) NOT NULL,
		[MessageGuid] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_ErrorMessage] PRIMARY KEY CLUSTERED 
	(
		[MessageId] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'RecentAdditionsShowPics'

/* ************************************************************** */
/* Version 9.1.0.2   											  */
/* ************************************************************** */

update [dbo].AppConfig set ConfigValue = 'https://www.paypal.com/en_US/i/bnr/bnr_paymentsBy_150x40.gif' where Name = 'PayPal.Express.ButtonImageURL'

/* ==== MONEYBOOKERS ==== */
-- Delete outdated AppConfigs
delete from AppConfig
where Name in (
	'Moneybookers.Language', 
	'Moneybookers.MerchantEmail', 
	'Moneybookers.MerchantMD5Password', 
	'Moneybookers.NotifyMerchant',
	'Moneybookers.RefundURL',
	'Moneybookers.StoreLogoURL')

-- Add new AppConfigs
if not exists (select * from AppConfig where Name = 'Moneybookers.SenderID')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.SenderID', 'The "sender ID" provided by Moneybookers.', '', 'string', 'null', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.ChannelID')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.ChannelID', 'The "channel ID" provided by Moneybookers.', '', 'string', 'null', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.UserLogin')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.UserLogin', 'The User Login provided by Moneybookers.', '', 'string', 'null', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.UserPassword')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.UserPassword', 'The User Password provided by Moneybookers.', '', 'string', 'null', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.Acquirer')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.Acquirer', 'The acquirer used by your merchant account. Setting this will populate the payment page with a list of supported credit cards. Setting this to "None" will use the standard credit card selection functionality.', 'None', 'enum', 'None,AIB,EMS,EPX', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.QuickCheckout.PaymentIcon')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.QuickCheckout.PaymentIcon', 'Path to the icon image for the Quick Checkout payment method.', '~/images/skrilllogo.jpg', 'string', 'null', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.QuickCheckout.HideLogin')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.QuickCheckout.HideLogin', 'Set to false to show the login form in the Quick Checkout iframe. Doing so will increase the required width of the frame.', 'true', 'boolean', 'null', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.Acquirer.AIB.AcceptedCardTypes')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.Acquirer.AIB.AcceptedCardTypes', 'A comma separated list of cards supported by the AIB acquierer. Determines which cards are allowed for payment. You should not need to change this value.', 'Visa,Mastercard,Laser,Maestro,Visa Debit,Visa Electron', 'string', 'null', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.Acquirer.EMS.AcceptedCardTypes')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.Acquirer.EMS.AcceptedCardTypes', 'A comma separated list of cards supported by the EMS acquierer. Determines which cards are allowed for payment. You should not need to change this value.', 'Visa,MasterCard,Maestro,Visa Debit,Visa Electron', 'string', 'null', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'Moneybookers.Acquirer.EPX.AcceptedCardTypes')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.Acquirer.EPX.AcceptedCardTypes', 'A comma separated list of cards supported by the EPX acquierer. Determines which cards are allowed for payment. You should not need to change this value.', 'Visa,MasterCard,Discover,Maestro,JCB,Diners,Amex', 'string', 'null', 'GATEWAY', 1, 0, getdate())

-- Update live and test server URL's
update AppConfig 
set ConfigValue = 'https://test.nextgenpay.com/payment/ctpe',
	Description = 'Test mode URL for Moneybookers maintenance requests. You should not need to change this value.'
where Name = 'Moneybookers.TestServer'
  and ConfigValue = 'http://www.moneybookers.com/app/payment.pl'

if not exists (select * from AppConfig where Name = 'Moneybookers.TestServer.3DSecure')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.TestServer.3DSecure', 'The base URL used to verify 3D Secure payments in test mode. You should not need to change this value.', 'https://test.nextgenpay.com', 'string', 'null', 'GATEWAY', 1, 0, getdate())

update AppConfig 
set ConfigValue = 'https://nextgenpay.com/payment/ctpe',
	Description = 'Live mode URL for Moneybookers maintenance requests. You should not need to change this value.'
where Name = 'Moneybookers.LiveServer'
  and ConfigValue = 'https://www.moneybookers.com/app/payment.pl'

if not exists (select * from AppConfig where Name = 'Moneybookers.LiveServer.3DSecure')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Moneybookers.LiveServer.3DSecure', 'The base URL used to verify 3D Secure payments in live mode. You should not need to change this value.', 'https://nextgenpay.com', 'string', 'null', 'GATEWAY', 1, 0, getdate())

-- Add moneybookers as a payment method option
if not exists (select * from AppConfig where Name = 'PaymentMethods' and AllowableValues like '%Moneybookers Quick Checkout%')
	update AppConfig 
	set AllowableValues = AllowableValues + ', Moneybookers Quick Checkout'
	where Name = 'PaymentMethods'
	
--SecureNetV411 App Configs
if not exists (select * from AppConfig where Name = 'SecureNetV4.LiveURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'SecureNetV4.LiveURL', 'Endpoint address for live SecureNet transactions', 'http://gateway.securenet.com/api/Gateway.svc/soap', 'string', '', 'GATEWAY', 1, 0, getdate())
	
if not exists (select * from AppConfig where Name = 'SecureNetV4.TestURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'SecureNetV4.TestURL', 'Endpoint address for test SecureNet transactions', 'http://certify.securenet.com/API/Gateway.svc/soap', 'string', '', 'GATEWAY', 1, 0, getdate())
	
if not exists (select * from AppConfig where Name = 'SecureNetV4.DataAPI.LiveURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'SecureNetV4.DataAPI.LiveURL', 'Endpoint address for live SecureNet data transactions', 'http://gateway.securenet.com/api/data/service.svc/soap', 'string', '', 'GATEWAY', 1, 0, getdate())
	
if not exists (select * from AppConfig where Name = 'SecureNetV4.DataAPI.TestURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'SecureNetV4.DataAPI.TestURL', 'Endpoint address for test SecureNet data transactions', 'http://certify.securenet.com/api/data/service.svc/soap', 'string', '', 'GATEWAY', 1, 0, getdate())
	
if not exists (select * from AppConfig where Name = 'SecureNetV4.UseTestMode')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'SecureNetV4.UseTestMode', 'If enabled the SecureNetV4 gateway will use soft test mode. Note that this is different than hitting the test endpoing; almost no validation will be performed.', 'false', 'boolean', '', 'GATEWAY', 1, 0, getdate())

if not exists (select * from AppConfig where Name = 'SecureNetV4.VaultEnabled')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'SecureNetV4.VaultEnabled', 'If enabled users will be able to store credit cards via the SecureNet Vault service.', 'false', 'boolean', '', 'GATEWAY', 1, 0, getdate())

-- Checkout Type
if not exists (select * from AppConfig where Name = 'Checkout.Type')
begin
	declare @checkoutTypeValue nvarchar(100)
	
	set @checkoutTypeValue = case 
		when exists (select * from AppConfig where Name like 'Vortx.OnePageCheckout.%') and exists (select * from AppConfig where Name = 'Checkout.UseOnePageCheckout' and (ConfigValue like 'true' or ConfigValue like '1' or ConfigValue like 'yes')) then 'SmartOPC'
		when exists (select * from AppCOnfig where Name = 'Checkout.UseOnePageCheckout' and (ConfigValue like 'true' or ConfigValue like '1' or ConfigValue like 'yes')) then 'BasicOPC'
		else 'Standard'
	end
	
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Checkout.Type', 'The Checkout Type.  Valid Values are Standard, SmartOPC, BasicOPC, or Other.', @checkoutTypeValue, 'enum', 'Standard,SmartOPC,BasicOPC,Other', 'CHECKOUT', 1, 0, getdate())
end
else
begin
	update AppConfig set 
		Description = 'The Checkout Type.  Valid Values are Standard, SmartOPC, BasicOPC, or Other.', 
		AllowableValues = 'Standard,SmartOPC,BasicOPC,Other' 
	where 
		Name = 'Checkout.Type' 
		and Description like 'The Checkout Type.  Valid Values are Standard,SmartOPC,DeprecatedOPC,or Other.' 
		and AllowableValues = 'Standard,SmartOPC,DeprecatedOPC,Other'
	
	update AppConfig set 
		ConfigValue = 'BasicOPC' 
	where 
		Name = 'Checkout.Type' 
		and ConfigValue = 'DeprecatedOPC'
end
go

ALTER PROC [dbo].[aspdnsf_GetCustomerByEmail]
    @Email nvarchar(100),
    @filtercustomer bit,
    @StoreID int = 1,
    @AdminOnly bit = 0
AS
BEGIN
    SET NOCOUNT ON

    DECLARE @CustomerSessionID int, @LastActivity datetime, @SessionTimeOut varchar(10), @intSessionTimeOut int

    SELECT @LastActivity = '1/1/1900', @CustomerSessionID = -1

    SELECT @SessionTimeOut = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [Name] = 'SessionTimeoutInMinutes'

    IF ISNUMERIC(@SessionTimeOut) = 1
        set @intSessionTimeOut = convert(int, @SessionTimeOut)
    ELSE
        set @intSessionTimeOut = 60

    SELECT  @CustomerSessionID  = cs.CustomerSessionID , @LastActivity = cs.LastActivity  
    FROM dbo.CustomerSession cs with (nolock) 
        join (SELECT max(CustomerSessionID) CustomerSessionID 
              FROM dbo.CustomerSession s with (nolock) join dbo.Customer c with (nolock) on s.CustomerID = c.CustomerID 
              WHERE c.Email = @Email and s.LoggedOut is null and s.LastActivity >= dateadd(mi, -@intSessionTimeOut, getdate())) a on cs.CustomerSessionID = a.CustomerSessionID

    SELECT top 1 
            c.CustomerID, c.CustomerGUID, c.CustomerLevelID, c.RegisterDate, c.Email, c.Password, c.SaltKey, c.DateOfBirth, c.Gender, 
            c.FirstName, c.LastName, c.Notes, c.SkinID, c.Phone, c.AffiliateID, c.Referrer, c.CouponCode, c.OkToEmail, 
            IsAdmin&1 IsAdmin, sign(IsAdmin&2) IsSuperAdmin, c.BillingEqualsShipping, c.LastIPAddress, 
            c.OrderNotes, c.SubscriptionExpiresOn, c.RTShipRequest, c.RTShipResponse, c.OrderOptions, c.LocaleSetting, 
            c.MicroPayBalance, c.RecurringShippingMethodID, c.RecurringShippingMethod, c.BillingAddressID, c.ShippingAddressID, 
            c.GiftRegistryGUID, c.GiftRegistryIsAnonymous, c.GiftRegistryAllowSearchByOthers, c.GiftRegistryNickName, 
            c.GiftRegistryHideShippingAddresses, c.CODCompanyCheckAllowed, c.CODNet30Allowed, c.ExtensionData, 
            c.FinalizationData, c.Deleted, c.CreatedOn, c.Over13Checked, c.CurrencySetting, 
            case when isnull(cl.CustomerLevelID, 0) > 0 and cl.LevelHasNoTax = 1 then 2 else c.VATSetting end VATSetting, 
            c.VATRegistrationID, c.StoreCCInDB, c.IsRegistered, c.LockedUntil, c.AdminCanViewCC, c.PwdChanged, c.BadLoginCount, 
            c.LastBadLogin, c.Active, c.PwdChangeRequired, c.SaltKey, isnull(cl.LevelDiscountPercent, 0) LevelDiscountPercent, 
            isnull(cl.LevelDiscountsApplyToExtendedPrices, 0) LevelDiscountsApplyToExtendedPrices, c.RequestedPaymentMethod,
            @CustomerSessionID CustomerSessionID, @LastActivity LastActivity, c.StoreID, d.Name StoreName
    FROM dbo.Customer c with (nolock) left join dbo.CustomerLevel cl with (nolock) on c.CustomerLevelID = cl.CustomerLevelID
    left join Store d with (nolock) on c.StoreID = d.StoreID
    WHERE c.Deleted=0 
		and c.Email = @Email 
		and ((@filtercustomer = 0 or IsAdmin > 0) or c.StoreID = @StoreID)
		and (@AdminOnly = 0 or c.IsAdmin > 0)
    ORDER BY c.IsRegistered desc, c.CreatedOn desc
END
GO

IF NOT EXISTS (SELECT * FROM [GlobalConfig] WHERE [Name] = 'Anonymous.AllowAlreadyRegisteredEmail') BEGIN
	INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], [ValueType], [IsMultiStore]) VALUES('Anonymous.AllowAlreadyRegisteredEmail', 'DISPLAY', 'If true anonymous users will be able to checkout with email addresses that are already in use. If AllowCustomerDuplicateEMailAddresses is true this has no effect.', 'false', 'boolean', 'false')
END
GO

ALTER proc [dbo].[aspdnsf_MonthlyMaintenance]
-- BACKUP YOUR DB BEFORE USING THIS SCRIPT!
    @InvalidateCustomerCookies      tinyint = 1,
    @PurgeAnonCustomers             tinyint = 1,
    @CleanShoppingCartsOlderThan    smallint = 30,  -- set to 0 to disable erasing
    @CleanWishListsOlderThan        smallint = 30,  -- set to 0 to disable erasing
    @CleanGiftRegistriesOlderThan   smallint = 30,  -- set to 0 to disable erasing
    @EraseCCFromAddresses           tinyint = 1,    -- except those used for recurring billing items!
    @EraseSQLLogOlderThan           smallint = 30,  -- set to 0 to disable erasing
    @ClearProductViewsOrderThan     smallint = 180, 
    @EraseCCFromOrdersOlderThan     smallint = 30,  -- set to 0 to disable erasing
    @DefragIndexes                  tinyint = 0,
    @PurgeDeletedRecords            tinyint = 0     -- Purges records in all tables with a deleted flag set to 1
  
AS
BEGIN
    set nocount on 
    DECLARE @cmd varchar(8000)

    -- clear out old stuff in failed transactions:
    delete from failedtransaction where orderdate < dateadd(mm,-2,getdate());

    -- clear out old tx info, not longer needed:
    update orders set TransactionCommand=NULL, AuthorizationResult=NULL, VoidTXCommand=NULL, VoidTXResult=NULL, CaptureTXCommand=NULL, CaptureTXResult=NULL, RefundTXCommand=NULL, RefundTXResult=NULL where orderdate < dateadd(mm,-2,getdate());

    -- invalidate all user cookies (forces re-login of all customers, for security safety):
    IF @InvalidateCustomerCookies = 1
    BEGIN
        update [dbo].customer set CustomerGUID=newid();
    END

    -- clean out RefundTXCommand, not needed anymore:
    update orders set RefundTXCommand=NULL;

    -- clean up all carts (don't touch recurring items, gift registry items, or wishlist items however):
    IF @CleanShoppingCartsOlderThan <> 0
    BEGIN 
        delete dbo.kitcart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
        delete dbo.customcart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
        delete dbo.ShoppingCart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
    END

    IF @CleanWishListsOlderThan <> 0
    BEGIN
        delete dbo.kitcart where CartType=1 and CreatedOn < dateadd(d,-@CleanWishListsOlderThan,getdate());
        delete dbo.customcart where CartType=1 and CreatedOn < dateadd(d,-@CleanWishListsOlderThan,getdate());
        delete dbo.ShoppingCart where CartType=1 and CreatedOn < dateadd(d,-@CleanWishListsOlderThan,getdate());
    END

    IF @CleanGiftRegistriesOlderThan = 1
    BEGIN
        delete dbo.kitcart where CartType=3 and CreatedOn < dateadd(d,-@CleanGiftRegistriesOlderThan,getdate());
        delete dbo.customcart where CartType=3 and CreatedOn < dateadd(d,-@CleanGiftRegistriesOlderThan,getdate());
        delete dbo.ShoppingCart where CartType=3 and CreatedOn < dateadd(d,-@CleanGiftRegistriesOlderThan,getdate());
    END

    -- purge anon customers:
    IF @PurgeAnonCustomers = 1
    BEGIN
        delete dbo.customer 
        where IsRegistered=0 and IsAdmin = 0
            and customerid not in (select customerid from dbo.ShoppingCart with (NOLOCK)) 
            and customerid not in (select customerid from dbo.kitcart with (NOLOCK)) 
            and customerid not in (select customerid from dbo.customcart with (NOLOCK)) 
            and customerid not in (select customerid from dbo.orders with (NOLOCK)) 
            and customerid not in (select customerid from dbo.rating with (NOLOCK)) 
            and customerid not in (select ratingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
            and customerid not in (select votingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
            and customerid not in (select customerid from dbo.pollvotingrecord with (NOLOCK))
    END
	
    -- clean any orphaned addresses
    delete dbo.Address where CustomerID Not in (select CustomerID from dbo.customer with (NOLOCK))

    -- clean addresses, except for those that have recurring orders
    IF @EraseCCFromAddresses = 1
    BEGIN
        IF exists (select * from dbo.sysobjects with (nolock) where type = 'u' and name = 'address')
            IF exists (select * from dbo.syscolumns with (nolock) where id = object_id('address') and name = 'CardExtraCode')
            BEGIN
                SET @cmd = 'update [dbo].address set CardNumber=''1111111111111111'', CardStartDate=''11/11'', CardIssueNumber=''111111111'', CardExtraCode=''1111'',CardExpirationMonth=''11'', CardExpirationYear=''1111'', eCheckBankABACode=''11111111'', eCheckBankAccountNumber=''11111111'' where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].address set CardNumber=NULL, CardStartDate=NULL, CardIssueNumber=NULL, CardExtraCode=NULL,CardExpirationMonth=NULL, CardExpirationYear=NULL, eCheckBankABACode=NULL, eCheckBankAccountNumber=NULL where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
            END 
            ELSE
            BEGIN
                SET @cmd = 'update [dbo].address set CardNumber=''1111111111111111'', CardStartDate=''11/11'', CardIssueNumber=''11111111'', CardExpirationMonth=''11'', CardExpirationYear=''1111'', eCheckBankABACode=''11111111'', eCheckBankAccountNumber=''11111111'' where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].address set CardNumber=NULL, CardStartDate=NULL, CardIssueNumber=NULL, CardExpirationMonth=NULL, CardExpirationYear=NULL, eCheckBankABACode=NULL, eCheckBankAccountNumber=NULL where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
            END 
        ELSE
        BEGIN
            -- erase credit card info from all customer records (recurring orders were not supported in these old versions)
            IF exists (select * From sysobjects so with (nolock) join [dbo].syscolumns sc on so.id = sc.id where so.type= 'u' and so.name = 'customer' and sc.name = 'CardNumber') BEGIN
                SET @cmd = 'update [dbo].Customer SET CardNumber = ''1111111111111111'''
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].Customer SET CardNumber = null'
                EXEC (@cmd)
                SET @cmd = ''
            END 
        END
    END

    -- erase credit cards from all orders older than N days:
    IF @EraseCCFromOrdersOlderThan <> 0
    BEGIN
        update [dbo].orders set CardNumber=NULL, eCheckBankABACode=NULL,eCheckBankAccountNumber=NULL WHERE OrderDate < dateadd(d,-@EraseCCFromOrdersOlderThan,getdate())
        IF exists (select * From dbo.sysobjects so with (nolock) join [dbo].syscolumns sc on so.id = sc.id where so.type= 'u' and so.name = 'orders' and sc.name = 'CardExtraCode') BEGIN
            SET @cmd = 'update [dbo].Orders SET CardExtraCode = ''1111'''
            EXEC (@cmd)
            SET @cmd = ''
            SET @cmd = 'update [dbo].Orders SET CardExtraCode = null'
            EXEC (@cmd)
            SET @cmd = ''
        END 
    END

    -- erase logged sql statements
    IF @EraseSQLLogOlderThan <> 0
    BEGIN
        DELETE dbo.SQLLog WHERE ExecutedOn < dateadd(d,-@EraseSQLLogOlderThan,getdate())
    END
    
    -- erase product views both for recently and dynamic
    IF @ClearProductViewsOrderThan <> 0
    BEGIN 
		DELETE dbo.ProductView WHERE ViewDate < dateadd(d,-@ClearProductViewsOrderThan,getdate())
	END

    truncate table CustomerSession

    exec aspdnsf_CustomerConsistencyCheck


    -- nuke all products that were used as temporary products for checkout (auction) purposes
    declare @productid int
    select productid into #tmpproduct from dbo.product with (nolock) where deleted = 2
    select top 1 @productid = productid from #tmpproduct 
    while @@rowcount > 0 begin
        exec dbo.aspdnsf_NukeProduct @productid
        delete #tmpproduct where productid = @productid
        select top 1 @productid = productid from #tmpproduct 
    end
    drop table #tmpproduct 

    IF @PurgeDeletedRecords = 1 BEGIN
        DELETE dbo.Address where deleted = 1
        DELETE dbo.Affiliate where deleted = 1
        DELETE dbo.Category where deleted = 1
        DELETE dbo.Coupon where deleted = 1
        DELETE dbo.Customer where deleted = 1
        DELETE dbo.CustomerLevel where deleted = 1
        DELETE dbo.Distributor where deleted = 1
        DELETE dbo.Document where deleted = 1
        DELETE dbo.FAQ where deleted = 1
        DELETE dbo.Gallery where deleted = 1
        DELETE dbo.Genre where deleted = 1
        DELETE dbo.Library where deleted = 1
        DELETE dbo.Manufacturer where deleted = 1
        DELETE dbo.News where deleted = 1
        DELETE dbo.Partner where deleted = 1
        DELETE dbo.Poll where deleted = 1
        DELETE dbo.PollAnswer where deleted = 1 or not exists (select * from dbo.Poll where PollID = PollAnswer.PollID)
        DELETE dbo.Product where deleted = 1
        DELETE dbo.ProductVariant where deleted = 1 or not exists (select * from dbo.Product where productid = ProductVariant.productid)
        DELETE dbo.SalesPrompt where deleted = 1
        DELETE dbo.Section where deleted = 1
        DELETE dbo.ShippingZone where deleted = 1
        DELETE dbo.Staff where deleted = 1
        DELETE dbo.Topic where deleted = 1
        DELETE dbo.Vector where deleted = 1

        DELETE dbo.ProductVector where not exists (select * from dbo.product where productid = ProductVector.productid) or not exists (select * from dbo.vector where vectorid = ProductVector.vectorid) 
        DELETE dbo.ProductAffiliate where not exists (select * from dbo.product where productid = ProductAffiliate.productid) or not exists (select * from dbo.Affiliate where Affiliateid = ProductAffiliate.Affiliateid) 
        DELETE dbo.ProductCategory where not exists (select * from dbo.product where productid = ProductCategory.productid) or not exists (select * from dbo.Category where Categoryid = ProductCategory.Categoryid) 
        DELETE dbo.ProductCustomerLevel where not exists (select * from dbo.product where productid = ProductCustomerLevel.productid) or not exists (select * from dbo.CustomerLevel where CustomerLevelid = ProductCustomerLevel.CustomerLevelid) 
        DELETE dbo.ProductDistributor where not exists (select * from dbo.product where productid = ProductDistributor.productid) or not exists (select * from dbo.Distributor where Distributorid = ProductDistributor.Distributorid) 
        DELETE dbo.ProductGenre where not exists (select * from dbo.product where productid = ProductGenre.productid) or not exists (select * from dbo.Genre where Genreid = ProductGenre.Genreid) 
        DELETE dbo.ProductLocaleSetting where not exists (select * from dbo.product where productid = ProductLocaleSetting.productid) or not exists (select * from dbo.LocaleSetting where LocaleSettingid = ProductLocaleSetting.LocaleSettingid) 
        DELETE dbo.ProductManufacturer where not exists (select * from dbo.product where productid = ProductManufacturer.productid) or not exists (select * from dbo.Manufacturer where Manufacturerid = ProductManufacturer.Manufacturerid) 
        DELETE dbo.ProductSection where not exists (select * from dbo.product where productid = ProductSection.productid) or not exists (select * from dbo.Section where Sectionid = ProductSection.Sectionid) 
        DELETE dbo.Address where not exists (select * from dbo.Customer where customerid = Address.customerid)
    END

    -- Defrag indexes
    IF @DefragIndexes = 1
    BEGIN
        CREATE TABLE #SHOWCONTIG (
           tblname VARCHAR (255),
           ObjectId INT,
           IndexName VARCHAR (255),
           IndexId INT,
           Lvl INT,
           CountPages INT,
           CountRows INT,
           MinRecSize INT,
           MaxRecSize INT,
           AvgRecSize INT,
           ForRecCount INT,
           Extents INT,
           ExtentSwitches INT,
           AvgFreeBytes INT,
           AvgPageDensity INT,
           ScanDensity DECIMAL,
           BestCount INT,
           ActualCount INT,
           LogicalFrag DECIMAL,
           ExtentFrag DECIMAL)

        SELECT [name] tblname into #tmp FROM dbo.sysobjects with (nolock) WHERE type = 'u' ORDER BY Name

        DECLARE @tblname varchar(255), @indexname varchar(255)
        SELECT top 1 @tblname = tblname FROM #tmp
        WHILE @@rowcount > 0 BEGIN
            SET @cmd = 'DBCC SHOWCONTIG (''' + @tblname + ''') with tableresults, ALL_INDEXES'
            INSERT #SHOWCONTIG
            EXEC (@cmd)
            DELETE #tmp WHERE tblname = @tblname 
            SELECT top 1 @tblname = tblname FROM #tmp
        END


        DELETE #SHOWCONTIG WHERE LogicalFrag < 5 or Extents = 1 or IndexId in (0, 255)


        SELECT top 1 @tblname = tblname, @indexname = IndexName FROM #SHOWCONTIG ORDER BY IndexId
        WHILE @@rowcount > 0 BEGIN
            SET @cmd = 'DBCC DBREINDEX (''' + @tblname + ''', ''' + @indexname + ''', 90)  '
            EXEC (@cmd)
            DELETE #SHOWCONTIG WHERE tblname = @tblname 
            SELECT top 1 @tblname = tblname, @indexname = IndexName FROM #SHOWCONTIG ORDER BY IndexId
        END
    END
END

GO
if not exists (select * from AppConfig where Name = 'QuantityDiscount.PercentDecimalPlaces')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'QuantityDiscount.PercentDecimalPlaces', 'The number of decimal places to show on percent quantity discounts', '0', 'integer', '', 'DISPLAY', 1, 0, getdate())
GO
if not exists (select * from AppConfig where Name = 'VAT.VATCheckServiceURL')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'VAT.VATCheckServiceURL', 'The endpoint for the VATCheck service.', 'http://ec.europa.eu/taxation_customs/vies/services/checkVatService', 'string', '', 'VAT', 1, 0, getdate())
GO
if not exists (select * from AppConfig where Name = 'ContactUs.UseCaptcha')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'ContactUs.UseCaptcha', 'Whether or not the contact us control protects against scripts with a captcha.', 'true', 'boolean', '', 'SECURITY', 1, 0, getdate())
GO
if not exists (select * from AppConfig where Name = 'PaymentExpress.Url')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'PaymentExpress.Url', 'The PaymentExpress endpoint.', 'https://www.paymentexpress.com/pxpost.aspx', 'string', '', 'GATEWAY', 1, 0, getdate())
GO

update appconfig set [description] = 'If you have multiple distributors, set this true to allow real time shipping rates to be calculated for the products based on the address of the distributor that its assigned to.' where name = 'RTShipping.MultiDistributorCalculation' and convert(nvarchar(max),[description]) = '*BETA* - If you have multiple distributors, set this true to allow real time shipping rates to be calculated for the products based on the address of the distributor that its assigned to.  Currently only available for UPS.'
GO

--==== AVALARA ====--
PRINT 'Creating MultiShipOrder_Shipment table for Avalara to work with Multiship functionality ...'
if not exists (select * from sysobjects where ID = object_id('MultiShipOrder_Shipment'))
	CREATE TABLE [dbo].[MultiShipOrder_Shipment](
		[MultiShipOrder_ShipmentId] [int] IDENTITY(1,1) NOT NULL,
		[MultiShipOrder_ShipmentGUID] [uniqueidentifier] NOT NULL,
		[OrderNumber] [int] NOT NULL,
		[DestinationAddress] [xml] NOT NULL,
		[ShippingAmount] [money] NOT NULL,
		[ShippingMethodId] [int] NOT NULL,
		[ShippingAddressId] [int] NOT NULL,
		[BillingAddressId] [int] NOT NULL
	) ON [PRIMARY]
GO

if not exists (select * from AppConfig where name like 'AvalaraTax.Enabled')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.Enabled', 'Set to true to use Avalara for tax calculations.', 'False', 'boolean', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.Account')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.Account', 'The account provided to you by Avalara.', '', 'string', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.License')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.License', 'The license provided to you by Avalara.', '', 'string', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.ServiceUrl')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.ServiceUrl', 'The service URL provided to you by Avalara.', '', 'string', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.CompanyCode')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.CompanyCode', 'The company code provided to you by Avalara.', '', 'string', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.TaxAddress')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.TaxAddress', 'Set to "Billing" to use the customer''s billing address for tax calculations. Set to "Shipping" to use the customer''s shipping address for tax calculations. This should not need to be changed.', 'Shipping', 'enum', 'Shipping,Billing', 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.TaxRefunds')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.TaxRefunds', 'Set to true to charge tax on order refunds. This should not need to be changed.', 'False', 'boolean', null, 'AVALARATAX', 0, 0, getdate())

if not exists (select * from AppConfig where name like 'AvalaraTax.DetailLevel')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.DetailLevel', 'The detail level used with Avalara Tax. Valid values are "summary", "line", "document", "diagnostic", and "tax" This should not need to be changed.', 'Tax', 'enum', 'Summary,Line,Document,Diagnostic,Tax', 'AVALARATAX', 0, 0, getdate())
go

--== HSBC Gateway URL Update ==--
update AppConfig 
set ConfigValue = 'https://www.uat.apixml.secureepayments.hsbc.com'
where Name in ('HSBC.Test.Server', 'HSBC.Live.Server') and ConfigValue = 'https://www.uat.apixml.netq.hsbc.com'
go

if not exists (select * from AppConfig where name like 'RTShipping.FedEx.SmartPost.IndiciaWeights')
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
VALUES (N'RTShipping.FedEx.SmartPost.IndiciaWeights', N'The Indicia weight breaks for Smart Post Shipments.  Format: IndiciaWeightRangeLow-IndiciaWeightRangeHigh:IndiciaType', N'string', NULL, N'RTSHIPPING', 1, 0, '0-0.99:PRESORTED_STANDARD,1-69.99:PARCEL_SELECT')

if not exists (select * from AppConfig where name like 'RTShipping.FedEx.SmartPost.AncillaryEndorsementType')
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
VALUES (N'RTShipping.FedEx.SmartPost.AncillaryEndorsementType', N'The Ancillary Endorsement Type for Smart Post Shipments.  Optional. 	Valid Values: "ADDRESS CORRECTION", "CARRIER LEAVE IF NO RESPONSE", "CHANGE SERVICE", "FORWARDING SERVICE", "RETURN SERVICE"', N'string', NULL, N'RTSHIPPING', 1, 0, '')

if not exists (select * from AppConfig where name like 'RTShipping.FedEx.SmartPost.HubId')
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
VALUES (N'RTShipping.FedEx.SmartPost.HubId', N'The HubId for your Smart Post Shipments.  See your FedEx account manager.', N'string', NULL, N'RTSHIPPING', 1, 0, '5531')

if not exists (select * from AppConfig where name like 'RTShipping.FedEx.SmartPost.Enabled')
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden], [ConfigValue]) 
VALUES (N'RTShipping.FedEx.SmartPost.Enabled', N'Enables the FedEx Smart Post service.  See your FedEx account manager for more information.', N'boolean', NULL, N'RTSHIPPING', 1, 0, 'false')
GO

--== Vortx OPC ==--
update AppConfig
set [Description] = 'This AppConfig has been deprecated. Use Checkout.Type instead.'
where Name = 'Checkout.UseOnePageCheckout'

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ShowEmailPreferencesOnCheckout')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.ShowEmailPreferencesOnCheckout', 'Set to true to allow customers to select email opt in/out on one page checkout.', 'true', 'boolean', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.UseZipcodeService')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.UseZipcodeService', 'Set to true to use automatic city/state lookup service based on zip-code.', 'true', 'boolean', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ShowCreateAccount')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.ShowCreateAccount', 'True to show the create account panel on SmartOPC, false to hide.', 'true', 'boolean', null, 'CHECKOUT', 1, 0, getdate())
end
		
if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.CustomerServiceTopic')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.CustomerServiceTopic', 'The name of the topic used for Customer Service panel on SmartOPC.', 'OPC.CustomerServicePanel', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.LicenseKey')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.LicenseKey', 'Enter your LicenseKey for the Smart OnePageCheckout.  Contact customer service if you do not have a LicenseKey.', '', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.AddressLocale')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.AddressLocale', 'Address Locale', 'US', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.OPCStyleSheetName')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.OPCStyleSheetName', 'Stylesheet used for SmartOPC.', 'onepagecheckout.css', 'string', null, 'CHECKOUT', 1, 0, getdate())
end
	
if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ZipCodeService.Timeout')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.ZipCodeService.Timeout', 'Do Not Change', '10', 'string', null, 'CHECKOUT', 1, 0, getdate())
end
		
if not exists (select * from AppConfig where Name = 'Vortx.OnePageCheckout.ScriptManagerId')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Vortx.OnePageCheckout.ScriptManagerId', 'Do Not Change', 'scrptMgr', 'string', null, 'CHECKOUT', 1, 0, getdate())
end

IF (NOT EXISTS (SELECT TopicID FROM Topic WHERE Name='OPC.CustomerServicePanel'))
	INSERT INTO Topic(Name,Title,Description,SkinID,ShowInSiteMap)
	VALUES('OPC.CustomerServicePanel','OPC.CustomerServicePanel', 
	'<div class="opc-container-header customer-service-header">
	<span class="opc-container-inner">
	Customer Service</span></div>
<div class="opc-container-body customer-service-body">
	<div class="page-links opc-container-inner customer-service-links">
		<a href="t-shipping.aspx" target="_blank">Shipping Info</a>
		<a href="t-returns.aspx" target="_blank">Return Policy</a>
		<a href="t-security.aspx" target="_blank">Security Policy</a>
		<a href="t-contact.aspx" target="_blank">Contact Us</a>
	</div>
	<div class="opc-container-inner customer-service-phone">
		Call Us At:<br />
		1.800.555.1234
	</div>
</div>', 0, 0);
GO

-- Add CheckoutByAmazon as a payment method option
if not exists (select * from AppConfig where Name = 'PaymentMethods' and AllowableValues like '%CheckoutByAmazon%')
	update AppConfig 
	set AllowableValues = AllowableValues + ', CheckoutByAmazon'
	where Name = 'PaymentMethods'
GO

update AppConfig set Description = 'Deprecated - VAT will be hidden and shown appropriately given the inclusive/exclusive display settings. ' + convert(nvarchar(max), [Description])
where Name ='vat.hidetaxinordersummary' and 
Description not like 'Deprecated - VAT will be hidden and shown appropriately given the inclusive/exclusive display settings. %'
GO

if not exists (select * from AppConfig where Name = 'Signin.SkinMaster')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Signin.SkinMaster', 'This config determines what skinid will be applied to a user after they sign in. Session: the skinid will be set based on the current session. Default: the skin id will be set to the site default. ', 'Default', 'enum', 'Session, Default', 'DISPLAY', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'QuantityDiscount.CombineQuantityByProduct')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'QuantityDiscount.CombineQuantityByProduct', 'If this is false then quantity discounts will be calculated per line item. If this is true then quantity discounts should be applied based on agregated quantities of product IDs.', 'false', 'boolean', '', 'DISPLAY', 1, 0, getdate())
end

--== Authorize.NET CIM ==--
if not exists (select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'Customer' and COLUMN_NAME = 'CIM_ProfileId')
	alter table Customer add CIM_ProfileId bigint

if not exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'CIM_AddressPaymentProfileMap')
	create table [dbo].[CIM_AddressPaymentProfileMap](
		[CIMId] int identity(1,1) not null primary key clustered,
		[AddressId] int not null foreign key([AddressId]) references [dbo].[Address] ([AddressID]),
		[CustomerId] int not null foreign key([CustomerId]) references [dbo].[Customer] ([CustomerID]),
		[AuthorizeNetProfileId] bigint not null,
		[ExpirationMonth] nvarchar(10) not null,
		[ExpirationYear] nvarchar(10) not null,
		[CardType] nvarchar(50) not null,
		[Primary] bit not null default (0)
	)
go

if not exists(select * from AppConfig Where Name='AUTHORIZENET_Cim_LiveServiceURL')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) values(newid(), 0, 'AUTHORIZENET_Cim_LiveServiceURL', 'CIM live Service URL. Do Not Change.', 'https://api.authorize.net/soap/v1/Service.asmx', 'string', null, 'ADMIN', 0, 0, getdate());

if not exists(select * from AppConfig Where Name='AUTHORIZENET_Cim_SandboxServiceURL')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) values(newid(), 0, 'AUTHORIZENET_Cim_SandboxServiceURL', 'CIM sandbox Service URL. Do Not Change.', 'https://apitest.authorize.net/soap/v1/Service.asmx', 'string', null, 'ADMIN', 0, 0, getdate());

if not exists(select * from AppConfig Where Name='AUTHORIZENET_Cim_UseSandbox')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) values(newid(), 0, 'AUTHORIZENET_Cim_UseSandbox', 'Set to true to use CIM in sandbox mode.', 'false', 'boolean', null, 'ADMIN', 0, 0, getdate());

if not exists(select * from AppConfig Where Name='AUTHORIZENET_Cim_Enabled')
	insert into [AppConfig] (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) values(newid(), 0, 'AUTHORIZENET_Cim_Enabled', 'Set to true to enable CIM.', 'true', 'boolean', null, 'ADMIN', 0, 0, getdate());

if not exists (select * from AppConfig where Name = 'Bongo.Extend.Script')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Bongo.Extend.Script', 'The location of your Bongo Extend script', 'https://s3bongous.s3.amazonaws.com/extend/loader/0deeb03b99e174cd366c6db87f6be71e.js', 'string', null, 'SHIPPING', 1, 0, getdate())
end

if not exists (select * from AppConfig where Name = 'Bongo.Extend.Enabled')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Bongo.Extend.Enabled', 'Set to true to enable Bongo Extend', 'false', 'boolean', null, 'SHIPPING', 1, 0, getdate())
end

--Start 9.2.x.x updates--
 IF  EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[ObjectView]') AND type = 'V')
        DROP VIEW [dbo].ObjectView
    GO

CREATE VIEW [dbo].[ObjectView]
AS 
SELECT	EM.EntityID AS ID,  
		EM.EntityType AS EntityType,
		EM.[Name],
		EM.Description
FROM EntityMaster AS EM WITH (NOLOCK) 

UNION ALL

SELECT	tp.TopicID AS ID, 
		'Topic' AS EntityType,
		tp.[Name],
		tp.Description
FROM Topic AS tp WITH (NOLOCK)

UNION ALL

SELECT	nw.NewsID AS ID,
		'News' AS EntityType,
		nw.Headline AS [Name],		
		'' AS Description
FROM News AS nw WITH(NOLOCK)

UNION ALL

SELECT	p.ProductID AS ID, 
		'Product' AS EntityType,
		p.[Name],
		p.Description
FROM Product AS p WITH(NOLOCK)

UNION ALL

SELECT	cp.CouponID AS ID, 
		'Coupon' AS EntityType,
		cp.[CouponCode] AS [Name],
		cp.Description
FROM Coupon AS cp WITH(NOLOCK)

UNION ALL

SELECT	oo.OrderOptionID AS ID, 
		'OrderOption' AS EntityType,
		oo.[Name],
		oo.Description
FROM OrderOption oo WITH(NOLOCK)

UNION ALL 

SELECT	gc.GiftCardID AS ID, 
		'GiftCard' AS EntityType,
		gc.SerialNumber AS [Name],
		'' AS Description
FROM GiftCard AS gc WITH(NOLOCK)

UNION ALL 

SELECT	sm.ShippingMethodID AS ID, 
		'ShippingMethod' AS EntityType,
		sm.[Name] AS [Name],
		'' AS Description
FROM ShippingMethod AS sm WITH(NOLOCK)  

UNION ALL

SELECT	po.PollID AS ID,
		'Polls' AS EntityType,
		po.Name AS [Name],
		'' AS Description
FROM Poll AS po WITH (NOLOCK)

GO

---------------- 9.3.0.0 -------------------------------------
-- Add PayPalPayments as a payment method option
if not exists (select * from AppConfig where Name = 'PaymentMethods' and AllowableValues like '%PayPal Payments Advanced%')
	update AppConfig 
	set AllowableValues = AllowableValues + ',PayPal Payments Advanced'
	where Name = 'PaymentMethods'
GO

if not exists (select * From sysobjects where id = object_id('OrderTransaction') and type = 'u')
BEGIN
	CREATE TABLE dbo.OrderTransaction
		(
		OrderTransactionID int NOT NULL IDENTITY (1, 1),
		OrderNumber int NOT NULL,
		TransactionType nvarchar(100) NOT NULL,
		TransactionCommand nvarchar(MAX) NULL,
		TransactionResult nvarchar(MAX) NULL,
		PNREF nvarchar(400) NULL,
		Code nvarchar(400) NULL,
		PaymentMethod nvarchar(100) NULL,
		PaymentGateway nvarchar(100) NULL,
		Amount money NULL
		)
	ALTER TABLE dbo.OrderTransaction ADD CONSTRAINT
		PK_OrderTransaction PRIMARY KEY CLUSTERED 
		(
		OrderTransactionID
		) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

/*********** 1st Pay Gateway *************/

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.AdminTransactionEmail.Enable')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.AdminTransactionEmail.Enable'
		,'This will enable/disable the 1stPay gateway to send out an email to the store admin for each transaction.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null	
	);
	
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.CustomerTransactionEmail.Enable')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.CustomerTransactionEmail.Enable'
		,'This will enable/disable the 1stPay gateway to send out an email to the customer when placing an order.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null	
	);
	
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.Level2.Enable')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.Level2.Enable'
		,'This will enable/disable the 1stPay gateway level 2 fields to be passed up on transactions. This must be enabled on the 1stPay account.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null	
	);
	
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.Cim.Enable')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.Cim.Enable'
		,'This will enable/disable the 1stPay gateway Cim feature. This must be enabled on the 1stPay account.'
		,'GATEWAY'
		,'false'
		,'boolean'
		, null	
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.TransactionCenterId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.TransactionCenterId'
		,'This is your unique Transaction Center number, NOT your 16 digit Merchant ID.'
		,'GATEWAY'
		,''
		,'integer'
		, null	
	);
	
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.ProcessorId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.ProcessorId'
		,'Processor Id it can be retrieved from the gateway options section in the 1stPay Transaction Center.'
		,'GATEWAY'
		,''
		,'string'
		, null	
	);

If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.GatewayId')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.GatewayId'
		,'This is your Alpha-numeric passphrase assigned by 1stPayGateway, it can be retrieved from the gateway options section in the 1stPay Transaction Center.'
		,'GATEWAY'
		,''
		,'string'
		, null	
	);		
	
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.EmailHeader')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.EmailHeader'
		,'This is an optional header that can be added to the emails which can be configured to send out for each transaction.  This should be short and plain text (like your company name and phone number).'
		,'GATEWAY'
		,''
		,'string'
		, null	
	);	
	
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.EmailFooter')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.EmailFooter'
		,'This is an optional footer that can be added to the emails which can be configured to send out for each transaction.  This should be short and plain text (like your company name and phone number).'
		,'GATEWAY'
		,''
		,'string'
		, null	
	);
	
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.PaymentModuleURL')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.PaymentModuleURL'
		,'This is the payment module url for the 1stPay gateway. This should not be changed'
		,'GATEWAY'
		,'https://secure.1stpaygateway.net/secure/gateway/pm.aspx'
		,'string'
		, null	
	);
	
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.XmlURL')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.XmlURL'
		,'This is the xml url for the 1stPay gateway. This should not be changed'
		,'GATEWAY'
		,'https://secure.1stpaygateway.net/secure/gateway/xmlgateway.aspx'
		,'string'
		, null	
	);
		
If Not Exists(Select * From dbo.AppConfig Where name = '1stPay.TestProccessor')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'1stPay.TestProccessor'
		,'This is the test processor for the 1stPay gateway. This should not be changed'
		,'GATEWAY'
		,'sandbox'
		,'string'
		, null	
	);

/*********** End 1st Pay Gateway *************/

/**************TFS 529*****************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_ProductSequence]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_ProductSequence]
GO

create proc [dbo].[aspdnsf_ProductSequence]
    @positioning varchar(10), -- allowed values: first, next, previous, last
    @ProductID int,
    @EntityName varchar(20),
    @EntityID int,
    @ProductTypeID int = null,
    @IncludeKits tinyint = 1 ,
    @IncludePacks tinyint = 1 ,
    @SortByLooks tinyint = 0, 
    @CustomerLevelID int = null,
    @affiliateID     int = null,
    @StoreID	int = null,
    @FilterProductsByStore tinyint = 0,
    @FilterOutOfStockProducts tinyint = 0
  
AS
BEGIN 
    SET NOCOUNT ON 

    
    DECLARE @id int, @row int
    DECLARE @affiliatecount int
    CREATE TABLE #sequence (row int identity not null, productid int not null)

    DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @HideProductsWithLessThanThisInventoryLevel int, @CustomerLevelFilteringIsAscending bit

    SET @FilterProductsByCustomerLevel = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @FilterProductsByAffiliate = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @HideProductsWithLessThanThisInventoryLevel = (SELECT TOP 1 case ConfigValue when -1 then 0 else ConfigValue end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    
    SET @CustomerLevelFilteringIsAscending  = 0
    SET @CustomerLevelFilteringIsAscending = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterByCustomerLevelIsAscending' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc) 
  
    select @affiliatecount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductAffiliate') and si.indid < 2 and type = 'u'

    IF @positioning not in ('first', 'next', 'previous', 'last')
        SET @positioning = 'first'

    insert #sequence (productid)     
    select pe.productid 
    from dbo.ProductEntity             pe  with (nolock)
        join [dbo].Product             p   with (nolock) on p.ProductID = pe.ProductID and pe.EntityType = @EntityName and pe.EntityID = @EntityID
        left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID 
        left join ProductAffiliate     pa  with (nolock) on p.ProductID = pa.ProductID 
		left join ProductVariant pv		   with (nolock) on p.ProductID = pv.ProductID  and pv.IsDefault = 1
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID
    where p.ProductTypeID = coalesce(nullif(@ProductTypeID, 0), p.ProductTypeID) and p.Deleted = 0 and p.Published = 1 and p.IsAKit <= @IncludeKits and p.IsAPack <= @IncludePacks 
          and (case 
                when @FilterProductsByCustomerLevel = 0 then 1
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID or pcl.CustomerLevelID is null then 1 
                when @CustomerLevelID=0 and pcl.CustomerLevelID is null then 1
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1 
                else 0
               end  = 1
              )
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)
          and ((case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end >= @HideProductsWithLessThanThisInventoryLevel) OR @FilterOutOfStockProducts = 0)
		  and (getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999'))     
		  and (p.ProductID IN (SELECT ProductID FROM ProductStore WHERE StoreID=@StoreID) OR @FilterProductsByStore = 0)
order by pe.DisplayOrder, p.Name



    SELECT @row = row FROM #sequence WHERE ProductID = @ProductID 

    IF @positioning = 'next' BEGIN
        SELECT top 1 @id = ProductID 
        FROM #sequence 
        WHERE row > @row 
        ORDER BY row

        IF @id is null
            SET @positioning = 'first'
    END


    IF @positioning = 'previous' BEGIN
        SELECT top 1 @id = ProductID 
        FROM #sequence 
        WHERE row < @row 
        ORDER BY row desc

        IF @id is null
            SET @positioning = 'last'
    END


    IF @positioning = 'first'
        SELECT top 1 @id = ProductID 
        FROM #sequence 
        ORDER BY row

    IF @positioning = 'last' 
        SELECT top 1 @id = ProductID 
        FROM #sequence
        ORDER BY row desc

    SELECT ProductID, SEName FROM dbo.Product with (nolock) WHERE ProductID = @id

END

GO
/**************End TFS 529*****************/

/************** TFS 597 - PayPal Icon url update *****************/

If Exists(Select * From dbo.AppConfig Where name = 'PayPal.Express.ButtonImageURL' And ConfigValue = 'https://www.paypal.com/en_US/i/bnr/bnr_paymentsBy_150x40.gif')
Begin
	Update AppConfig
	Set ConfigValue = 'https://www.paypalobjects.com/en_US/i/btn/btn_xpressCheckout.gif'
	Where Name = 'PayPal.Express.ButtonImageURL';
End

If Not Exists(Select * From dbo.AppConfig Where name = 'PayPal.Express.ButtonImageURL')
Begin

	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'PayPal.Express.ButtonImageURL'
		,'URL for Express Checkout Button Image'
		,'GATEWAY'
		,'https://www.paypalobjects.com/en_US/i/btn/btn_xpressCheckout.gif'
		,'string'
		, null	
	);

End

If Exists(Select * From dbo.AppConfig Where name = 'PayPal.PaymentIcon' And ConfigValue = 'https://www.paypal.com/en_US/i/logo/PayPal_mark_50x34.gif')
Begin
	Update AppConfig
	Set ConfigValue = 'https://www.paypalobjects.com/en_US/i/logo/PayPal_mark_50x34.gif'
	Where Name = 'PayPal.PaymentIcon';
End

If Not Exists(Select * From dbo.AppConfig Where name = 'PayPal.PaymentIcon')
Begin

	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'PayPal.PaymentIcon'
		,'Image URL for Paypal payment method icon.'
		,'GATEWAY'
		,'https://www.paypalobjects.com/en_US/i/logo/PayPal_mark_50x34.gif'
		,'string'
		, null	
	);

End

/**************TFS 347 & TFS 796 *********************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetProducts]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetProducts]
GO

CREATE proc [dbo].[aspdnsf_GetProducts]
    @categoryID      int = null,  
    @sectionID       int = null,  
    @manufacturerID  int = null,  
    @distributorID   int = null,  
    @genreID         int = null,  
    @vectorID        int = null,  
    @localeID        int = null,  
    @CustomerLevelID int = null,  
    @affiliateID     int = null,  
    @ProductTypeID   int = null,  
    @ViewType        bit = 1, -- 0 = all variants, 1 = one variant  
    @sortEntity      int = 0, -- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector  
    @pagenum         int = 1,  
    @pagesize        int = null,  
    @StatsFirst      tinyint = 1,  
    @searchstr       nvarchar(4000) = null,  
    @extSearch       tinyint = 0,  
    @publishedonly   tinyint = 0,  
    @ExcludePacks    tinyint = 0,  
    @ExcludeKits     tinyint = 0,  
    @ExcludeSysProds tinyint = 0,  
    @InventoryFilter int = 0,  --  will only show products with an inventory level GREATER OR EQUAL TO than the number specified in this parameter, set to -1 to disable inventory filtering  
    @sortEntityName  varchar(20) = '', -- usely only when the entity id is provided, allowed values: category, section, manufacturer, distributor, genre, vector  
    @localeName      varchar(20) = '',  
    @OnSaleOnly      tinyint = 0,  
    @IncludeAll      bit = 0, -- Don't filter products that have a start date in the future or a stop date in the past  
	@storeID		 int = 1,
	@filterProduct	 bit = 0,
	@sortby			 varchar(10) = 'default',
	@since			 int = 180  -- best sellers in the last "@since" number of days
	
  
AS  
BEGIN  
  
    SET NOCOUNT ON   
  
    DECLARE @rcount int
    DECLARE @productfiltersort table (rownum int not null identity  primary key, productid int not null, price money null, saleprice money null, displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)
    DECLARE @productfilter table (rownum int not null identity  primary key, productid int not null, price money null, saleprice money null,  displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)
	DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @HideProductsWithLessThanThisInventoryLevel int  
    CREATE TABLE #displayorder ([name] nvarchar (800), productid int not null primary key, displayorder int not null)  
    CREATE TABLE #inventoryfilter (productid int not null, variantid int not null, InvQty int not null)  
    CREATE CLUSTERED INDEX tmp_inventoryfilter ON #inventoryfilter (productid, variantid)  
  
    DECLARE @custlevelcount int, @sectioncount int, @localecount int, @affiliatecount int, @categorycount int, @CustomerLevelFilteringIsAscending bit, @distributorcount int, @genrecount int, @vectorcount int, @manufacturercount int  
  
	DECLARE @ftsenabled tinyint
	
	SET @ftsenabled = 0
	
	IF ((SELECT DATABASEPROPERTY(db_name(db_id()),'IsFulltextEnabled')) = 1 
		AND EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[KeyWordSearch]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
		AND EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetValidSearchString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')))
	BEGIN
		SET @ftsenabled = 1
	END
  
    SET @FilterProductsByAffiliate = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @FilterProductsByCustomerLevel = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @HideProductsWithLessThanThisInventoryLevel = (SELECT TOP 1 case ConfigValue when -1 then 0 else ConfigValue end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    
    IF @InventoryFilter <> -1 and (@HideProductsWithLessThanThisInventoryLevel > @InventoryFilter or @HideProductsWithLessThanThisInventoryLevel  = -1)  
        SET @InventoryFilter  = @HideProductsWithLessThanThisInventoryLevel  
  
    SET @categoryID      = nullif(@categoryID, 0)  
    SET @sectionID       = nullif(@sectionID, 0)  
    SET @manufacturerID  = nullif(@manufacturerID, 0)  
    SET @distributorID   = nullif(@distributorID, 0)  
    SET @genreID         = nullif(@genreID, 0)  
    SET @vectorID        = nullif(@vectorID, 0)  
    SET @affiliateID     = nullif(@affiliateID, 0)  
    SET @ProductTypeID   = nullif(@ProductTypeID, 0)  
  
  
    SET @CustomerLevelFilteringIsAscending  = 0  
    SET @CustomerLevelFilteringIsAscending = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterByCustomerLevelIsAscending' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc) 
  
    IF @localeID is null and ltrim(rtrim(@localeName)) <> ''  
        SELECT @localeID = LocaleSettingID FROM dbo.LocaleSetting with (nolock) WHERE Name = ltrim(rtrim(@localeName))  
  
    select @categorycount     = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productcategory') and si.indid < 2 and type = 'u'  
    select @sectioncount      = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productsection') and si.indid < 2 and type = 'u'  
    select @localecount       = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductLocaleSetting') and si.indid < 2 and type = 'u'  
    select @custlevelcount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductCustomerLevel') and si.indid < 2 and type = 'u'  
    select @affiliatecount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductAffiliate') and si.indid < 2 and type = 'u'  
    select @distributorcount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductDistributor') and si.indid < 2 and type = 'u'  
    select @genrecount        = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductGenre') and si.indid < 2 and type = 'u'  
    select @vectorcount       = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductVector') and si.indid < 2 and type = 'u'  
    select @manufacturercount = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductManufacturer') and si.indid < 2 and type = 'u'  
  
  
    -- get page size  
    IF @pagesize is null or @pagesize = 0 BEGIN  
        IF @categoryID is not null  
            SELECT @pagesize = PageSize FROM dbo.Category with (nolock) WHERE categoryID = @categoryID  
        ELSE IF @sectionID is not null  
            SELECT @pagesize = PageSize FROM dbo.Section with (nolock) WHERE sectionID = @sectionID  
        ELSE IF @manufacturerID is not null  
            SELECT @pagesize = PageSize FROM dbo.Manufacturer with (nolock) WHERE manufacturerID = @manufacturerID  
        ELSE IF @distributorID is not null  
            SELECT @pagesize = PageSize FROM dbo.Distributor with (nolock) WHERE distributorID = @distributorID  
        ELSE IF @genreID is not null  
            SELECT @pagesize = PageSize FROM dbo.Genre with (nolock) WHERE genreID = @genreID  
        ELSE IF @vectorID is not null  
            SELECT @pagesize = PageSize FROM dbo.Vector with (nolock) WHERE vectorID = @vectorID  
        ELSE   
            SELECT @pagesize = convert(int, ConfigValue) FROM dbo.AppConfig with (nolock) WHERE [Name] = 'Default_CategoryPageSize'  
    END  
  
    IF @pagesize is null or @pagesize = 0  
        SET @pagesize = 20  
  
    -- get sort order  
    IF @sortEntity = 1 or @sortEntityName = 'category' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductCategory a with (nolock) inner join (select distinct a.ProductID from ProductCategory a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b  on a.ProductID = b.ProductID where categoryID = @categoryID 
    END  
    ELSE IF @sortEntity = 2 or @sortEntityName = 'section' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductSection a with (nolock) inner join (select distinct a.ProductID from ProductSection a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where sectionId = @sectionID
    END  
    ELSE IF @sortEntity = 3 or @sortEntityName = 'manufacturer' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductManufacturer a with (nolock) inner join (select distinct a.ProductID from ProductManufacturer a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where ManufacturerID = @manufacturerID
    END  
    ELSE IF @sortEntity = 4 or @sortEntityName = 'distributor' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductDistributor a with (nolock) inner join (select distinct a.ProductID from ProductDistributor a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where DistributorID = @distributorID
    END  
    ELSE IF @sortEntity = 5 or @sortEntityName = 'genre' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductGenre a with (nolock) inner join (select distinct a.ProductID from ProductGenre a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where GenreID = @genreID
    END  
    ELSE IF @sortEntity = 6 or @sortEntityName = 'vector' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductVector a with (nolock) inner join (select distinct a.ProductID from ProductVector a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where VectorID = @vectorID
    END  
    ELSE BEGIN  
        INSERT #displayorder select distinct [name], a.productid, 1 from dbo.Product a with (nolock) inner join (select distinct a.ProductID from Product a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID ORDER BY Name  
    END  

	IF (@ftsenabled = 1)
	BEGIN
		IF rtrim(isnull(@searchstr, '')) <> ''
		BEGIN
			DECLARE @tmpsrch nvarchar(4000)
			SET @tmpsrch = dbo.GetValidSearchString(@searchstr) 
			DELETE #displayorder from #displayorder d left join dbo.KeyWordSearch(@tmpsrch) k on d.productid = k.productid where k.productid is null  
		END
	END
	
	SET @searchstr = '%' + rtrim(ltrim(@searchstr)) + '%' 
 
    IF @InventoryFilter <> -1 BEGIN  
        IF @ViewType = 1 BEGIN  
            INSERT #inventoryfilter  
            SELECT p.productid, pv.VariantID, sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) invqty  
            FROM product p with (NOLOCK) join #displayorder d on p.ProductID = d.ProductID  
                join ProductVariant pv with (NOLOCK) on p.ProductID = pv.ProductID  and pv.IsDefault = 1  
                left join Inventory i with (NOLOCK) on pv.VariantID = i.VariantID  
            GROUP BY p.productid, pv.VariantID  
            HAVING sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) >= @InventoryFilter  
        END  
        ELSE  
            INSERT #inventoryfilter  
            SELECT p.productid, pv.VariantID, sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) invqty  
            FROM product p with (NOLOCK) join #displayorder d on p.ProductID = d.ProductID  
                join ProductVariant pv with (NOLOCK) on p.ProductID = pv.ProductID  
                left join Inventory i with (NOLOCK) on pv.VariantID = i.VariantID  
            GROUP BY p.productid, pv.VariantID  
            HAVING sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) >= @InventoryFilter  
  

        INSERT @productfilter (productid, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)  
        SELECT distinct p.productid, do.displayorder, pv.VariantID, pv.DisplayOrder, p.Name, pv.Name  
        FROM   
            product p with (nolock)  
            join #displayorder do on p.ProductID = do.ProductID   
            left join ProductVariant pv        with (NOLOCK) ON p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
            left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID   
            left join productsection ps        with (nolock) on p.ProductID = ps.ProductID   
            left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID   
            left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID   
            left join ProductGenre px          with (nolock) on p.ProductID = px.ProductID   
            left join ProductVector px2        with (nolock) on p.ProductID = px2.ProductID   
            left join ProductLocaleSetting pl  with (nolock) on p.ProductID = pl.ProductID   
            left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID   
            left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID
			   
            join #inventoryfilter i on pv.VariantID = i.VariantID  
        WHERE   
              (pc.categoryid = @categoryID or @categoryID is null or @categorycount = 0)  
          and (ps.sectionid = @sectionID or @sectionID is null or @sectioncount = 0)  
          and (pl.LocaleSettingID = @localeID or @localeID is null or @localecount = 0)  
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)  
          and (pm.manufacturerid = @manufacturerID or @manufacturerID is null or @manufacturercount = 0)  
          and (pd.DistributorID = @distributorID or @distributorID is null or @distributorcount = 0)  
          and (px.GenreID = @genreID or @genreID is null or @genrecount = 0)  
          and (px2.VectorID = @vectorID or @vectorID is null or @vectorcount = 0)  
          and p.ProductTypeID = coalesce(@ProductTypeID, p.ProductTypeID)  
          and (case   
                when @FilterProductsByCustomerLevel = 0 or @custlevelcount = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1  
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1   
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1   
                else 0  
               end  = 1  
              )  
          and (@ftsenabled = 1 or
				(@ftsenabled = 0 and
					(@searchstr is null  
					 or patindex(@searchstr, isnull(p.name, '')) > 0  
					 or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0   
					 or patindex(@searchstr, isnull(pv.name, '')) > 0  
					 or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0  
					 or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0  
					 or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0  
					 or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)  
					 or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)  
					)
				)
              )  
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= @OnSaleOnly  
          and p.published >= @publishedonly  
          and pv.published >= @publishedonly  
          and isnull(p.IsAPack, 0) <= 1-@ExcludePacks  
          and isnull(p.IsAKit, 0) <= 1-@ExcludeKits  
          and p.IsSystem <= 1-@ExcludeSysProds  
          and p.Deleted = 0  
          and pv.Deleted = 0  
          and ((@IncludeAll = 1) or (getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999')))    
        order by do.displayorder, p.Name, pv.DisplayOrder, pv.Name  
  
    END   
    ELSE BEGIN  
        INSERT @productfilter (productid, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)  
        SELECT distinct p.productid, do.displayorder, pv.VariantID, pv.DisplayOrder, p.Name, pv.Name  
        FROM   
            product p with (nolock)  
            join #displayorder do on p.ProductID = do.ProductID   
            join ProductVariant pv             with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
            left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID   
            left join productsection ps        with (nolock) on p.ProductID = ps.ProductID   
            left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID   
            left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID   
            left join ProductGenre px          with (nolock) on p.ProductID = px.ProductID   
            left join ProductVector px2        with (nolock) on p.ProductID = px2.ProductID   
            left join ProductLocaleSetting pl  with (nolock) on p.ProductID = pl.ProductID   
            left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID   
            left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID   
        WHERE   
              (pc.categoryid = @categoryID or @categoryID is null or @categorycount = 0)  
          and (ps.sectionid = @sectionID or @sectionID is null or @sectioncount = 0)  
          and (pl.LocaleSettingID = @localeID or @localeID is null or @localecount = 0)  
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)  
          and (pm.manufacturerid = @manufacturerID or @manufacturerID is null or @manufacturercount = 0)  
          and (pd.DistributorID = @distributorID or @distributorID is null or @distributorcount = 0)  
          and (px.GenreID = @genreID or @genreID is null or @genrecount = 0)  
          and (px2.VectorID = @vectorID or @vectorID is null or @vectorcount = 0)  
          and p.ProductTypeID = coalesce(@ProductTypeID, p.ProductTypeID)  
          and (case   
                when @FilterProductsByCustomerLevel = 0 or @custlevelcount = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1  
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1   
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1   
                else 0  
               end  = 1  
              )  
          and (@ftsenabled = 1 or
				(@ftsenabled = 0 and
					(@searchstr is null  
					or patindex(@searchstr, isnull(p.name, '')) > 0  
					or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0   
					or patindex(@searchstr, isnull(pv.name, '')) > 0  
					or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0  
					or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0  
					or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0  
					or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)  
					or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)  
					)
				)
              )  
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= @OnSaleOnly  
          and p.published >= @publishedonly  
          and pv.published >= @publishedonly  
          and isnull(p.IsAPack, 0) <= 1-@ExcludePacks  
          and isnull(p.IsAKit, 0) <= 1-@ExcludeKits  
          and p.IsSystem <= 1-@ExcludeSysProds  
          and p.Deleted = 0  
          and pv.Deleted = 0  
          and ((@IncludeAll = 1) or (getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999')))    
        order by do.displayorder, p.Name, pv.DisplayOrder, pv.Name  
    END  
	
    SET @rcount = @@rowcount  
    IF @StatsFirst = 1  
        SELECT cast(ceiling(@rcount*1.0/@pagesize) as int) pages, @rcount ProductCount  
  
  
  --Begin sorting
  	if @sortby = 'bestseller'
		begin
			insert @productfiltersort (productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName) 
			select pf.productid, pf.price, pf.saleprice, pf.displayorder, pf.VariantID, pf.VariantDisplayOrder, pf.ProductName, pf.VariantName
				from @productfilter pf
				left join (
					select ProductID, SUM(Quantity) AS NumSales
					  from dbo.Orders_ShoppingCart sc with (NOLOCK) 
							join [dbo].Orders o with (NOLOCK)  on sc.OrderNumber = o.OrderNumber and o.OrderDate >= dateadd(dy, -@since, getdate())
					  group by ProductID 
				) bsSort on pf.productid = bsSort.ProductID
				order by isnull(bsSort.NumSales, 0) DESC
		end
  	else --default
		begin
			insert @productfiltersort (productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName) 
			select productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName
			from @productfilter order by displayorder, productName, variantDisplayOrder, variantName
		end
    
    SELECT   
        p.ProductID,  
        p.Name,  
        pv.VariantID,  
        pv.Name AS VariantName,  
        p.ProductGUID,  
        p.Summary,  
        p.Description,  
        p.SEKeywords,  
        p.SEDescription,  
        p.SpecTitle,  
        p.MiscText,  
        p.SwatchImageMap,  
        p.IsFeaturedTeaser,  
        p.FroogleDescription,  
        p.SETitle,  
        p.SENoScript,  
        p.SEAltText,  
        p.SizeOptionPrompt,  
        p.ColorOptionPrompt,  
        p.TextOptionPrompt,  
        p.ProductTypeID,  
        p.TaxClassID,  
        p.SKU,  
        p.ManufacturerPartNumber,  
        p.SalesPromptID,  
        p.SpecCall,  
        p.SpecsInline,  
        p.IsFeatured,  
        p.XmlPackage,  
        p.ColWidth,  
        p.Published,  
        p.RequiresRegistration,  
        p.Looks,  
        p.Notes,  
        p.QuantityDiscountID,  
        p.RelatedProducts,  
        p.UpsellProducts,  
        p.UpsellProductDiscountPercentage,  
        p.RelatedDocuments,  
        p.TrackInventoryBySizeAndColor,  
        p.TrackInventoryBySize,  
        p.TrackInventoryByColor,  
        p.IsAKit,  
        p.ShowInProductBrowser,  
        p.IsAPack,  
        p.PackSize,  
        p.ShowBuyButton,  
        p.RequiresProducts,  
        p.HidePriceUntilCart,  
        p.IsCalltoOrder,  
        p.ExcludeFromPriceFeeds,  
        p.RequiresTextOption,  
        p.TextOptionMaxLength,  
        p.SEName,  
        p.Deleted,  
        p.CreatedOn,  
        p.ImageFileNameOverride,  
        pv.VariantGUID,  
        pv.Description AS VariantDescription,  
        pv.SEKeywords AS VariantSEKeywords,  
        pv.SEDescription AS VariantSEDescription,  
        pv.Colors,  
        pv.ColorSKUModifiers,  
        pv.Sizes,  
        pv.SizeSKUModifiers,  
        pv.FroogleDescription AS VariantFroogleDescription,  
        pv.SKUSuffix,  
        pv.ManufacturerPartNumber AS VariantManufacturerPartNumber,  
        pv.Price,  
        pv.CustomerEntersPrice,   
        pv.CustomerEntersPricePrompt,  
        isnull(pv.SalePrice, 0) SalePrice,  
        cast(isnull(pv.Weight,0) as decimal(10,1)) Weight,  
        pv.MSRP,  
        pv.Cost,  
        isnull(pv.Points,0) Points,  
        pv.Dimensions,  
        case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end Inventory,  
        pv.DisplayOrder as VariantDisplayOrder,  
        pv.Notes AS VariantNotes,  
        pv.IsTaxable,  
        pv.IsShipSeparately,  
        pv.IsDownload,  
        pv.DownloadLocation,  
        pv.Published AS VariantPublished,  
        pv.IsSecureAttachment,  
        pv.IsRecurring,  
        pv.RecurringInterval,  
        pv.RecurringIntervalType,  
        pv.SubscriptionInterval,  
        pv.SEName AS VariantSEName,  
        pv.RestrictedQuantities,  
        pv.MinimumQuantity,  
        pv.Deleted AS VariantDeleted,  
        pv.CreatedOn AS VariantCreatedOn,  
        d.Name AS DistributorName,  
        d.DistributorID,  
        d.SEName AS DistributorSEName,  
        m.ManufacturerID,  
        m.Name AS ManufacturerName,  
        m.SEName AS ManufacturerSEName,  
        s.Name AS SalesPromptName,  
        case when pcl.productid is null then 0 else isnull(ep.Price, 0) end ExtendedPrice  
    FROM dbo.Product p with (NOLOCK)   
        left join dbo.ProductVariant       pv  with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
        join @productfiltersort            pf                on pv.ProductID = pf.ProductID and pv.VariantID = pf.VariantID   
        left join dbo.SalesPrompt           s  with (NOLOCK) on p.SalesPromptID = s.SalesPromptID   
        left join dbo.ProductManufacturer  pm  with (NOLOCK) on p.ProductID = pm.ProductID   
        left join dbo.Manufacturer          m  with (NOLOCK) on pm.ManufacturerID = m.ManufacturerID   
        left join dbo.ProductDistributor   pd  with (NOLOCK) on p.ProductID = pd.ProductID  
        left join dbo.Distributor           d  with (NOLOCK) on pd.DistributorID = d.DistributorID  
        left join dbo.ExtendedPrice        ep  with (NOLOCK) on ep.VariantID = pv.VariantID and ep.CustomerLevelID = @CustomerLevelID  
        left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID  
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID  
    WHERE pf.rownum >= @pagesize*(@pagenum-1)+1 and pf.rownum <= @pagesize*(@pagenum)  
    ORDER BY pf.rownum  
  
    IF @StatsFirst <> 1  
        SELECT cast(ceiling(@rcount*1.0/@pagesize) as int) pages, @rcount ProductCount  
  
END
GO
/**************END TFS 347*********************/


/************** TFS 600 *********************/

	IF NOT EXISTS (select * from [dbo].AppConfig where Name = 'PayFlowPro.Product')
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayFlowPro.Product','GATEWAY','If using PayPal PayFlow PRO merchant gateway, this is the specific product that uses the PayFlowPro gateway.','');	
	GO

/************** END TFS 600 *********************/

/**************TFS 234*****************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_GetVariantsPaged]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_GetVariantsPaged]
GO

CREATE PROCEDURE [dbo].[aspdnsf_GetVariantsPaged]
(
	@pageSize int,
	@startIndex int,
	@EntityFilterType int = 0,
	@EntityFilterID int = 0
)
AS
BEGIN
SET NOCOUNT ON

DECLARE @Filter TABLE (productID int)

IF (@EntityFilterID <> 0 AND @EntityFilterType <> 0) BEGIN
	IF @EntityFilterType = 1
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductCategory WHERE CategoryID = @EntityFilterID
	IF @EntityFilterType = 2
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductManufacturer WHERE ManufacturerID = @EntityFilterID
	IF @EntityFilterType = 3
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductSection WHERE SectionID = @EntityFilterID
	IF @EntityFilterType = 4
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductDistributor WHERE DistributorID = @EntityFilterID
END;

WITH OrderedVariants AS
(
	SELECT	pv.variantID,
			pv.IsDefault,
			pv.Name,
			pv.Description,
			pv.ProductID,
			pv.SkuSuffix,
			pv.Price,
			pv.SalePrice,
			pv.Inventory,
			pv.Published,
			pv.Deleted,
			p.TrackInventoryBySizeAndColor,
			p.Name as ProductName,
			ROW_NUMBER() OVER(ORDER BY pv.variantID) as RowNum
			FROM ProductVariant pv WITH (NOLOCK)
			JOIN Product p on p.ProductID = pv.ProductID
			WHERE pv.Deleted = 0
				AND ((@EntityFilterType = 0 OR @EntityFilterID = 0) OR
					pv.ProductID in (SELECT ProductID from @Filter))
)
SELECT	TOP (@pageSize) variantID,
		IsDefault,
		Name,
		Description,
		ProductID,
		SkuSuffix,
		Price,
		SalePrice,
		Inventory,
		Published,
		Deleted,
		TrackInventoryBySizeAndColor,
		ProductName
FROM OrderedVariants
WHERE RowNum > @startIndex

END
GO
/**************END TFS 234*****************/

/************* TFS XXX ********************/
update AppConfig set [Description] = 'Set to true to use Avalara for tax calculations.<br /><a  href="AvalaraConnectionTest.aspx" class="lightboxLink">Click here to test your AvaTax connection</a><br /><a href="https://admin-avatax.avalara.net/" target="_blank">Click here for your AvaTax Admin Console</a>' where Name = 'AvalaraTax.Enabled' and  [Description] like 'Set to true to use Avalara for tax calculations.'
update AppConfig set [Description] = 'Unused' where Name = 'AvalaraTax.DetailLevel'

/************* END TFS XXX ****************/

/************ TFS 626 *********************/

IF NOT EXISTS (SELECT * FROM Topic WHERE Name='SubscriptionToken.Subscribe')
Begin
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) 
	values('SubscriptionToken.Subscribe'
			, 1
			, 0
			, 'SubscriptionToken.Subscribe'
			, ''
			);
End
Go

/********************* End TFS 626 **********************/

/************* TFS 549 & TFS 796 ********************/
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_ProductInfo]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_ProductInfo]
GO

CREATE proc [dbo].[aspdnsf_ProductInfo]
    @ProductID          int,  
    @CustomerLevelID    int,  
    @DefaultVariantOnly tinyint,  
    @InvFilter          int = 0,  
    @AffiliateID        int = null,  
    @PublishedOnly      tinyint = 1,
    @IsAdmin			tinyint = 0,
    @StoreID			int = 0
AS BEGIN
	SET NOCOUNT ON  
	DECLARE 
		@CustLevelExists int, 
		@AffiliateExists int, 
		@FilterProductsByAffiliate tinyint, 
		@FilterProductsByCustomerLevel tinyint, 
		@CustomerLevelFilteringIsAscending tinyint,
		@CustomerLevelCount int, 
		@AffiliateCount int, 
		@MinProductCustomerLevel int, 
		@HideProductsWithLessThanThisInventoryLevel int  
		
	SELECT @FilterProductsByCustomerLevel		= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByCustomerLevel'		AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
	SELECT @CustomerLevelFilteringIsAscending	= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterByCustomerLevelIsAscending'	AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
	SELECT @FilterProductsByAffiliate			= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByAffiliate'			AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
	SELECT @HideProductsWithLessThanThisInventoryLevel	= CONVERT(INT, ConfigValue) FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'HideProductsWithLessThanThisInventoryLevel' AND ISNUMERIC(ConfigValue) = 1 AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
	SELECT @CustomerLevelCount = COUNT(*), @MinProductCustomerLevel = MIN(CustomerLevelID), @CustLevelExists = SUM(CASE WHEN CustomerLevelID = @CustomerLevelID THEN 1 ELSE 0 END) FROM dbo.ProductCustomerLevel WITH (NOLOCK) WHERE ProductID = @ProductID
	SELECT @AffiliateCount = COUNT(*), @AffiliateExists = SUM(CASE WHEN AffiliateID = @AffiliateID THEN 1 ELSE 0 END) FROM dbo.ProductAffiliate WITH (NOLOCK) WHERE ProductID = @ProductID   

	IF (@HideProductsWithLessThanThisInventoryLevel > @InvFilter or @HideProductsWithLessThanThisInventoryLevel = -1) and @InvFilter <> 0  
		SET @InvFilter = @HideProductsWithLessThanThisInventoryLevel   

	IF
	(
		(
			(
				@FilterProductsByCustomerLevel = 0 
				OR @CustomerLevelCount = 0 
				OR (
					@CustomerLevelFilteringIsAscending = 1 
					AND @MinProductCustomerLevel <= @CustomerLevelID) 
				OR @CustLevelExists > 0
			)  
			AND (
				@FilterProductsByAffiliate = 0 
				OR @AffiliateCount = 0 
				OR @AffiliateExists > 0)
		)
		OR @IsAdmin = 1
	)  
		SELECT   
			p.*,   
			pv.VariantID, pv.name VariantName, pv.Price, pv.Description VariantDescription, isnull(pv.SalePrice, 0) SalePrice, isnull(SkuSuffix, '') SkuSuffix, pv.Dimensions, pv.Weight, isnull(pv.Points, 0) Points, pv.Inventory, pv.ImageFilenameOverride VariantImageFilenameOverride,  pv.isdefault, pv.CustomerEntersPrice, isnull(pv.colors, '') Colors, isnull(pv.sizes, '') Sizes,  
			sp.name SalesPromptName,   
			case when pcl.productid is null then 0 else isnull(e.Price, 0) end ExtendedPrice  
		FROM dbo.Product p with (nolock) 
			join dbo.productvariant            pv  with (NOLOCK) on p.ProductID = pv.ProductID     
			join dbo.SalesPrompt               sp  with (NOLOCK) on p.SalesPromptID = sp.SalesPromptID   
			left join dbo.ExtendedPrice        e   with (NOLOCK) on pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID  
			left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID  
			left join (select variantid, sum(quan) inventory from inventory group by variantid) i on pv.variantid = i.variantid  
		WHERE p.ProductID = @ProductID   
			and p.Deleted = 0   
			and pv.Deleted = 0   
			and p.Published >= @PublishedOnly  
			and pv.Published >= @PublishedOnly  
			and pv.IsDefault >= @DefaultVariantOnly  
			and getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999')  
			and (case p.TrackInventoryBySizeAndColor when 1 then isnull(i.inventory, 0) else pv.inventory end >= @InvFilter or @InvFilter = 0)  
		ORDER BY pv.DisplayOrder, pv.name
END
GO
/************* END TFS 549 ****************/

/************* TFS 552 **************************/
/*    This goes in the 8012-90 upgrade script   */
/************************************************/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[ZipTaxRate]') AND type = 'D' And (Name Like 'DF__ZipTaxRat__Count__%' OR Name = 'DF_ZipTaxRate_CountryID'))
BEGIN
	DECLARE @constraintName nvarchar(max)
	SELECT @constraintName = Name FROM dbo.sysobjects WHERE parent_obj = OBJECT_ID(N'[ZipTaxRate]') AND type = 'D' And (Name Like 'DF__ZipTaxRat__Count__%' OR Name = 'DF_ZipTaxRate_CountryID')
	EXEC('ALTER TABLE [dbo].[ZipTaxRate] DROP CONSTRAINT ' + @constraintName)
END

DECLARE @countryID int
DECLARE @SQL nvarchar(max)
SELECT @countryID = CountryID FROM Country WHERE Name='United States'
SET @sql = 'ALTER TABLE [dbo].[ZipTaxRate] ADD CONSTRAINT [DF_ZipTaxRate_CountryID] DEFAULT ' + CAST(@countryID as nvarchar) + ' FOR CountryID'
EXEC(@sql)
GO
/************* END TFS 552 ****************/

/************* TFS 180 ****************/
If Not Exists(Select * From dbo.AppConfig Where name = 'SendLowStockWarnings')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'SendLowStockWarnings'
		,'If true, store admins will be sent an email when purchases take product inventory levels below the value specified in the SendLowStockWarningsThreshold AppConfig.'
		,'OUTOFSTOCK'
		,'false'
		,'boolean'
		, null	
	);

If Not Exists(Select * From dbo.AppConfig Where name = 'SendLowStockWarningsThreshold')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'SendLowStockWarningsThreshold'
		,'This sets the threshold at which to start notifying store admins that a product is running low on stock, if SendLowStockWarnings is enabled.'
		,'OUTOFSTOCK'
		,'1'
		,'string'
		, null	
	);

If Not Exists(Select * From dbo.AppConfig Where name = 'ShowAdminLowStockAudit')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values
	(
		'ShowAdminLowStockAudit'
		,'If this is set to true, a table of products with inventory levels lower than SendLowStockWarningsThreshold will be displayed on the admin home page.  NOTE: This may slow down the load time of the admin home page on very large sites.'
		,'OUTOFSTOCK'
		,'false'
		,'boolean'
		, null	
	);
/************* END TFS 180 ****************/

/************* TFS 578 - SecureNetV4 App Configs ****************/

/***********************************************/
/* These configs need to be in the create      */
/* script as well as the update.  The original */
/* live urls are incorrect and need to be      */
/* replaced with the https versions.           */
/*                                             */
/***********************************************/

if not exists (select * from AppConfig where Name = 'SecureNetV4.LiveURL')
Begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
		values (newid()
					, 0
					, 'SecureNetV4.LiveURL'
					, 'Endpoint address for live SecureNet transactions'
					, 'https://gateway.securenet.com/api/Gateway.svc/soap'
					, 'string'
					, ''
					, 'GATEWAY'
					, 1
					, 0
					, getdate()
				);
End
Else if exists (select * from AppConfig where Name = 'SecureNetV4.LiveURL' And ConfigValue = 'http://gateway.securenet.com/api/Gateway.svc/soap')
Begin
	Update AppConfig
	Set ConfigValue = 'https://gateway.securenet.com/api/Gateway.svc/soap'
	Where Name = 'SecureNetV4.LiveURL' 
	And ConfigValue = 'http://gateway.securenet.com/api/Gateway.svc/soap';
End
		
if not exists (select * from AppConfig where Name = 'SecureNetV4.DataAPI.LiveURL')
Begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid()
				, 0
				, 'SecureNetV4.DataAPI.LiveURL'
				, 'Endpoint address for live SecureNet data transactions'
				, 'https://gateway.securenet.com/api/data/service.svc/soap'
				, 'string'
				, ''
				, 'GATEWAY'
				, 1
				, 0
				, getdate()
			);
End
Else if exists (select * from AppConfig where Name = 'SecureNetV4.DataAPI.LiveURL' And ConfigValue = 'http://gateway.securenet.com/api/data/service.svc/soap')
Begin
	Update AppConfig
	Set ConfigValue = 'https://gateway.securenet.com/api/data/service.svc/soap'
	Where Name = 'SecureNetV4.DataAPI.LiveURL' 
	And ConfigValue = 'http://gateway.securenet.com/api/data/service.svc/soap';
End
Go
	
/************* END TFS 578 - SecureNetV4 App Configs ****************/

/************* TFS 615 ****************/
/* I've already added this to the     */
/* create script, with it set to      */
/* true.  Upgrades we'll set it to    */
/* false instead.                     */
/**************************************/
if not exists (select * from AppConfig where Name = 'NameImagesBySEName')
Begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
		values (newid()
					, 0
					, 'NameImagesBySEName'
					, 'If true, product images will be named by the product SEName.  If false, product ID is used.'
					, 'false'
					, 'boolean'
					, ''
					, 'GENERAL'
					, 0
					, 0
					, getdate()
				);
End
/************* END TFS 615 ****************/

/************* TFS 665: Promotions ****************/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Promotions]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[Promotions](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[PromotionGuid] [uniqueidentifier] NOT NULL CONSTRAINT DF_Promotion_PromotionGUID DEFAULT(newid()),
			[Name] [varchar](100) NOT NULL,
			[Description] [varchar](max) NOT NULL,
			[UsageText] varchar(max) null,
			EmailText varchar(max) null,
			[Code] [varchar](50) NOT NULL CONSTRAINT [DF_Promotions_Code]  DEFAULT (''),
			[Priority] [numeric](18, 0) NOT NULL,
			[Active] [bit] NOT NULL,
			[AutoAssigned] [bit] NOT NULL,
			CallToAction NVARCHAR(MAX) NULL,
			[PromotionRuleData] [xml] NULL,
			[PromotionDiscountData] [xml] NULL,
		 CONSTRAINT [PK_Promotions] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
	END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' and TABLE_NAME = 'Promotions' AND COLUMN_NAME = 'CallToAction')
BEGIN
   ALTER TABLE dbo.Promotions ADD CallToAction NVARCHAR(MAX) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PromotionUsage]') AND type in (N'U'))
	BEGIN
		CREATE TABLE [dbo].[PromotionUsage](
			[Id] [int] IDENTITY(1,1) NOT NULL,
			[PromotionId] [int] NOT NULL,
			[CustomerId] [int] NOT NULL,
			[OrderId] [int] NULL,
			[DateApplied] [datetime] NULL,
			ShippingDiscountAmount MONEY NULL,
			LineItemDiscountAmount MONEY NULL,
			OrderDiscountAmount MONEY NULL,
			[DiscountAmount] [money] NULL,
			[Complete] [bit] NOT NULL CONSTRAINT [DF_PromotionUsage_Complete]  DEFAULT ((0)),	
			Constraint FK_PromotionUsage_PromotionId Foreign Key (PromotionId) References Promotions(Id), 
		 CONSTRAINT [PK_PromotionUsage] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY]
	END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PromotionLineItem]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [dbo].[PromotionLineItem](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[PromotionUsageId][int] Not Null,
		[shoppingCartRecordId] [int] NOT NULL,
		[productId] [int] NOT NULL,
		[variantId] [int] NOT NULL,
		[sku] [nvarchar](150) NOT NULL,
		[quantity] [int] NOT NULL,
		[cartPrice] [money] NOT NULL,
		[subTotal] [money] NOT NULL,
		[isAGift] [bit] NOT NULL,
		[discountAmount] [money] NOT NULL,
		Constraint FK_PromotionLineItem_PromotionUsageId Foreign Key (PromotionUsageId) References PromotionUsage(Id) On Delete Cascade, 
	 CONSTRAINT [PK_[PromotionLineItem] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PromotionStore]') AND type in (N'U'))
	BEGIN
	CREATE TABLE [dbo].[PromotionStore]
	(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[PromotionID] [INT] NOT NULL,
		[StoreID] [INT] NOT NULL,
		[CreatedOn] [datetime] NOT NULL CONSTRAINT DF_PromotionStore_CreatedOn DEFAULT(getdate())
		, Constraint FK_PromotionStore_PromotionId Foreign Key (PromotionId) References Promotions(Id) On Delete Cascade 
		, Constraint FK_PromotionStore_StoreID Foreign Key (StoreId) References Store(StoreId) On Delete Cascade 
		, Constraint AK_PromotionStore Unique(PromotionId, StoreId),
		CONSTRAINT [PK_PromotionStore] Primary Key (Id)
	)
	END
GO

If Not Exists (select * from information_schema.columns where table_name = 'Orders_ShoppingCart' and column_name = 'IsGift')
Begin
	Alter Table Orders_ShoppingCart Add IsGift bit NOT NULL Constraint DF_Orders_ShoppingCart_IsGift Default(0);
End
GO

If Not Exists (select * from information_schema.columns where table_name = 'ShoppingCart' and column_name = 'IsGift')
Begin
	Alter Table ShoppingCart Add IsGift bit NOT NULL Constraint DF_ShoppingCart_IsGift Default(0);
End
GO

ALTER VIEW StoreMappingView
	AS
	SELECT ID, StoreID, EntityID, EntityType FROM EntityStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, ProductID EntityID, 'Product' EntityType FROM ProductStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, NewsID EntityID, 'News' EntityType FROM NewsStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, TopicID EntityID, 'Topic' EntityType FROM TopicStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, CouponID EntityID, 'Coupon' EntityType FROM CouponStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, PromotionID EntityID, 'Promotion' EntityType FROM PromotionStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, OrderOptionID EntityID, 'OrderOption' EntityType FROM OrderOptionStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, GiftCardID EntityID, 'GiftCard' EntityType FROM GiftCardStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, PollID EntityID, 'Polls' EntityType FROM PollStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, ShippingMethodID EntityID, 'ShippingMethod' EntityType FROM ShippingMethodStore WITH (NOLOCK)
	UNION ALL
	SELECT ID, StoreID, AffiliateID EntityID, 'Affiliate' EntityType FROM AffiliateStore WITH (NOLOCK)
GO

ALTER VIEW MappedObjects
	AS 
	SELECT 
		EM.EntityID AS ID,  
		EM.EntityType AS EntityType, 
		ParentEntityID AS ParentID, 
		EM.EntityGUID AS GUID, EM.[Name] , 
		ES.StoreID AS StoreID
	FROM EntityMaster AS EM WITH (NOLOCK) LEFT JOIN EntityStore AS ES WITH (NOLOCK) ON ES.EntityID = EM.EntityID AND ES.EntityType = EM.EntityType
	UNION ALL
	SELECT TP.TopicID AS ID, 'Topic' AS EntityType,0 AS ParentID, TP.TopicGUID AS GUID, TP.[Name], TS.StoreID AS StoreID
	FROM Topic AS TP WITH (NOLOCK) LEFT JOIN StoreMappingView AS TS WITH (NOLOCK)
	ON TS.EntityID = TP.TopicID AND TS.EntityType='Topic'
	UNION ALL
	SELECT NW.NewsID AS ID,'News' AS EntityType,0 AS ParentID, NW.NewsGUID AS GUID, NW.Headline AS [Name], NS.StoreID AS StoreID
	FROM News AS NW LEFT JOIN StoreMappingView AS NS WITH (NOLOCK)
	ON NS.EntityID = NW.NewsID AND NS.EntityType='News'
	UNION ALL
	SELECT PR.ProductID AS ID, 'Product' AS EntityType,0 AS ParentID, PR.ProductGUID AS GUID, PR.[Name], PS.StoreID AS StoreID
	FROM Product AS PR LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON PR.ProductID = PS.EntityID AND PS.EntityType='Product'
	UNION ALL
	SELECT CP.CouponID AS ID, 'Coupon' AS EntityType,0 AS ParentID, CP.CouponGUID AS GUID, CP.[CouponCode] AS [Name], PS.StoreID AS StoreID
	FROM Coupon AS CP LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON CP.CouponID = PS.EntityID AND PS.EntityType='Coupon'
	UNION ALL
	SELECT Promo.Id AS ID, 'Promotion' AS EntityType,0 AS ParentID, Promo.PromotionGUID AS GUID, Promo.[Code] AS [Name], PS.StoreID AS StoreID
	FROM Promotions AS Promo LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON Promo.Id = PS.EntityID AND PS.EntityType='Promotion'
	UNION ALL
	SELECT OO.OrderOptionID AS ID, 'OrderOption' AS EntityType,0 AS ParentID, OO.OrderOptionGUID AS GUID, OO.[Name], PS.StoreID AS StoreID
	FROM OrderOption AS OO LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON OO.OrderOptionID = PS.EntityID AND PS.EntityType = 'OrderOption'
	UNION ALL 
	SELECT GC.GiftCardID AS ID, 'GiftCard' AS EntityType,0 AS ParentID, GC.GiftCardGUID AS GUID, GC.SerialNumber AS [Name], PS.StoreID AS StoreID
	FROM GiftCard AS GC LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
	ON GC.GiftCardID = PS.EntityID AND PS.EntityType = 'GiftCard'
GO

ALTER PROC [dbo].[aspdnsf_CloneStoreMappings]
	@FromStoreID INT,	
	@ToStoreID int
	AS
	BEGIN	
		
		INSERT INTO EntityStore (StoreID, EntityID, EntityType)
		SELECT @ToStoreID AS [StoreID], EntityID, EntityType FROM EntityStore WHERE StoreID = @FromStoreID

		INSERT INTO AffiliateStore (StoreID, AffiliateID)
		SELECT @ToStoreID AS [StoreID], AffiliateID FROM AffiliateStore WHERE StoreID = @FromStoreID

		INSERT INTO NewsStore (StoreID, NewsID)
		SELECT @ToStoreID AS [StoreID], NewsID FROM NewsStore WHERE StoreID = @FromStoreID

		INSERT INTO ProductStore (StoreID, ProductID)
		SELECT @ToStoreID AS [StoreID], ProductID FROM ProductStore WHERE StoreID = @FromStoreID

		INSERT INTO TopicStore (StoreID, TopicID)
		SELECT @ToStoreID AS [StoreID], TopicID FROM TopicStore WHERE StoreID = @FromStoreID

		INSERT INTO PollStore (StoreID, PollID)
		SELECT @ToStoreID AS [StoreID], PollID FROM Pollstore WHERE StoreID = @FromStoreID

		INSERT INTO GiftCardStore (StoreID, GiftCardID)
		SELECT @ToStoreID AS [StoreID], GiftCardID FROM GiftCardStore WHERE StoreID = @FromStoreID

		INSERT INTO CouponStore (StoreID, CouponID)
		SELECT @ToStoreID AS [StoreID], CouponID FROM CouponStore WHERE StoreID = @FromStoreID

		INSERT INTO PromotionStore (StoreID, PromotionID)
		SELECT @ToStoreID AS [StoreID], PromotionID FROM PromotionStore WHERE StoreID = @FromStoreID
		
		INSERT INTO OrderOptionStore(StoreID, OrderOptionID)
		SELECT @ToStoreID AS [StoreID], OrderOptionID FROM OrderOptionStore WHERE StoreID = @FromStoreID

		INSERT INTO ShippingMethodStore(StoreID, ShippingMethodID)
		SELECT @ToStoreID AS [StoreID], ShippingMethodID FROM ShippingMethodStore WHERE StoreID = @FromStoreID

		-- only create additional configs/string resources for non-default stores
		declare @isDefault tinyint
		select @isDefault = IsDefault from Store WHERE StoreID = @FromStoreID
		if(@isDefault <> 1)
		begin
		   INSERT INTO StringResource(StringResourceGUID, StoreID, [Name], LocaleSetting, ConfigValue)
			SELECT newid(), @ToStoreID, [Name], LocaleSetting, ConfigValue FROM StringResource WHERE StoreID = @FromStoreID 

			INSERT INTO AppConfig(AppConfigGUID, StoreID, [Name], Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden)
			SELECT newid(), @ToStoreID, [Name], Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden FROM AppConfig WHERE StoreID = @FromStoreID 	
		end	
	END
GO

ALTER PROC [dbo].[aspdnsf_NukeStoreMappings]
	@StoreID INT
	AS
	BEGIN		
		DELETE FROM EntityStore WHERE StoreID = @StoreID
		DELETE FROM AffiliateStore WHERE StoreID = @StoreID	
		DELETE FROM NewsStore WHERE StoreID = @StoreID	
		DELETE FROM ProductStore WHERE StoreID = @StoreID
		DELETE FROM TopicStore WHERE StoreID = @StoreID	
		DELETE FROM Pollstore WHERE StoreID = @StoreID	
		DELETE FROM GiftCardStore WHERE StoreID = @StoreID	
		DELETE FROM CouponStore WHERE StoreID = @StoreID	
		DELETE FROM PromotionStore WHERE StoreID = @StoreID	
		DELETE FROM OrderOptionStore WHERE StoreID = @StoreID	
		DELETE FROM ShippingMethodStore WHERE StoreID = @StoreID

		-- only create additional configs/string resources for non-default stores
		declare @isDefault tinyint
		select @isDefault = IsDefault from Store WHERE StoreID = @StoreID
		if(@isDefault <> 1)
		begin	   
			DELETE FROM StringResource WHERE StoreID = @StoreID 		
			DELETE FROM AppConfig WHERE StoreID = @StoreID 	
		end	
	END
GO

ALTER PROC NukeStore 
	@StoreID INT,
	@NukeNews BIT = 0,
	@NukeAffiliates BIT =0,
	@NukeTopics BIT = 0,
	@NukeProducts BIT = 0,
	@NukeCoupons BIT = 0,
	@NukePromotions BIT = 0,
	@NukeOrderOptions BIT = 0,
	@NukeGiftCards BIT = 0,
	@NukeCategories BIT = 0,
	@NukeSections BIT = 0,
	@NukeManufacturers BIT = 0,
	@NukeDistributors BIT = 0,
	@NukeGenres BIT = 0,
	@NukeVectors BIT = 0
	AS
	CREATE TABLE #tmpEntities(
	GUID UniqueIdentifier
	)
	INSERT INTO #tmpEntities (GUID) (
	SELECT GUID FROM MappedObjects WHERE StoreID = @StoreID AND GUID NOT IN(
	SELECT GUID FROM MappedObjects WHERE StoreID <> @StoreID)
	)

	IF (@NukeNews = 1)			DELETE FROM News WHERE NewsGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeAffiliates = 1)	DELETE FROM Affiliate WHERE AffiliateGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeTopics = 1)		DELETE FROM Topic WHERE TopicGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeProducts = 1)		DELETE FROM Product WHERE ProductGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeCoupons = 1)		DELETE FROM Coupon WHERE CouponGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukePromotions = 1)	DELETE FROM Promotions WHERE PromotionGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeOrderOptions = 1)	DELETE FROM OrderOption WHERE OrderOptionGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeGiftCards = 1)		DELETE FROM GiftCard WHERE GiftCardGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeCategories = 1)	DELETE FROM Category WHERE CategoryGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeSections = 1)		DELETE FROM Section WHERE SectionGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeManufacturers = 1)	DELETE FROM Manufacturer WHERE ManufacturerGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeDistributors = 1)	DELETE FROM Distributor WHERE DistributorGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeGenres = 1)		DELETE FROM Genre WHERE GenreGUID IN (SELECT [GUID] FROM #tmpEntities)
	IF (@NukeVectors = 1)		DELETE FROM Vector WHERE VectorGUID IN (SELECT [GUID] FROM #tmpEntities)

	DELETE FROM EntityStore WHERE StoreID = @StoreID
	DELETE FROM AffiliateStore WHERE StoreID = @StoreID
	DELETE FROM NewsStore WHERE StoreID = @StoreID
	DELETE FROM ProductStore WHERE StoreID = @StoreID
	DELETE FROM TopicStore WHERE StoreID = @StoreID
	DELETE FROM PollStore WHERE StoreID = @StoreID
	DELETE FROM GiftCardStore WHERE StoreID = @StoreID
	DELETE FROM CouponStore WHERE StoreID = @StoreID
	DELETE FROM OrderOptionStore WHERE StoreID = @StoreID
	DELETE FROM Store WHERE StoreID = @StoreID

	DROP TABLE #tmpEntities
GO

alter procedure [dbo].[aspdnsf_SaveMap]
	@StoreID INT,
	@EntityID INT,
	@EntityType NVARCHAR(50),
	@Map BIT
	AS
	BEGIN
		-- Add Mapping Information
		if(@map = 1)
		begin

			IF @EntityType='Product'
			begin
				IF NOT EXISTS (SELECT * FROM ProductStore WHERE StoreID = @StoreID AND ProductID = @EntityID)
				begin
					INSERT INTO ProductStore(StoreID, ProductID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType in ('Category', 'Manufacturer', 'Section')
			begin
				IF NOT EXISTS (SELECT * FROM EntityStore WHERE StoreID = @StoreID AND EntityId = @EntityID and EntityType = @EntityType)
				begin
					INSERT INTO EntityStore(StoreID, EntityType, EntityId) VALUES (@StoreID, @EntityType, @EntityID)
				end
			end
			else IF @EntityType='ShippingMethod'
			begin
				IF NOT EXISTS (SELECT * FROM ShippingMethodStore WHERE StoreID = @StoreID AND ShippingMethodId = @EntityID)
				begin
					INSERT INTO ShippingMethodStore(StoreID, ShippingMethodId) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType = 'Topic'
			begin
				IF NOT EXISTS(SELECT * FROM TopicStore WHERE @StoreID = StoreID AND TopicID = @EntityID)
				begin
					INSERT INTO TopicStore (StoreID, TopicID) VALUES (@StoreID, @EntityID)
				end			
			end
			else IF @EntityType = 'News'
			begin
				IF NOT EXISTS (SELECT * FROM NewsStore WHERE StoreID = @StoreID AND NewsID = @EntityID)
				begin
					INSERT INTO NewsStore(StoreID, NewsID) VALUES (@StoreID, @EntityID)
				end		
			end			
			else IF @EntityType='OrderOption'
			begin
				IF NOT EXISTS (SELECT * FROM OrderOptionStore WHERE StoreID = @StoreID AND OrderOptionID = @EntityID)
				begin
					INSERT INTO OrderOptionStore(StoreID, OrderOptionID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='GiftCard'
			begin
				IF NOT EXISTS (SELECT * FROM GiftCardStore WHERE StoreID = @StoreID AND GiftCardId = @EntityID)
				begin
					INSERT INTO GiftCardStore(StoreID, GiftCardId) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='Affiliate'
			begin
				IF NOT EXISTS (SELECT * FROM AffiliateStore WHERE StoreID = @StoreID AND AffiliateID = @EntityID)
				begin
					INSERT INTO AffiliateStore(StoreID, AffiliateID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='Coupon'
			begin
				IF NOT EXISTS (SELECT * FROM CouponStore WHERE StoreID = @StoreID AND CouponID = @EntityID)
				begin
					INSERT INTO CouponStore(StoreID, CouponID) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='Promotion'
			begin
				IF NOT EXISTS (SELECT * FROM PromotionStore WHERE StoreID = @StoreID AND PromotionId = @EntityID)
				begin
					INSERT INTO PromotionStore(StoreID, PromotionId) VALUES (@StoreID, @EntityID)
				end
			end
			else IF @EntityType='Polls'
			begin
				IF NOT EXISTS (SELECT * FROM PollStore WHERE StoreID = @StoreID AND PollID = @EntityID)
				begin
					INSERT INTO PollStore(StoreID, PollID) VALUES (@StoreID, @EntityID)
				end
			end
		end
		-- Remove Mapping Information if any
		else if (@map = 0)
		begin

			IF @EntityType='Product'
			begin
				DELETE FROM ProductStore WHERE ProductID = @EntityID AND StoreID = @StoreID
			end 
			else IF @EntityType in ('Category', 'Manufacturer', 'Section')
			begin
				DELETE FROM EntityStore WHERE EntityId = @EntityID AND StoreID = @StoreID and EntityType = @EntityType
			end 
			else IF @EntityType = 'ShippingMethod'
			begin
				DELETE FROM ShippingMethodStore WHERE ShippingMethodID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType = 'Topic'
			begin
				DELETE FROM TopicStore WHERE TopicID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType = 'News'
			begin
				DELETE FROM NewsStore WHERE NewsID = @EntityID AND StoreID = @StoreID
			end		
			else IF @EntityType='OrderOption'
			begin
				DELETE FROM OrderOptionStore WHERE OrderOptionID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='GiftCard'
			begin
				DELETE FROM GiftCardStore WHERE GiftCardId = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='Affiliate'
			begin
				DELETE FROM AffiliateStore WHERE AffiliateId = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='Coupon'
			begin
				DELETE FROM CouponStore WHERE CouponID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='Promotion'
			begin
				DELETE FROM PromotionStore WHERE PromotionID = @EntityID AND StoreID = @StoreID
			end
			else IF @EntityType='Polls'
			begin
				DELETE FROM PollStore WHERE PollID = @EntityID AND StoreID = @StoreID
			end	
		end
	END
GO

ALTER PROC dbo.aspdnsf_CustomerConsistencyCheck  
	AS
	BEGIN
	SET NOCOUNT ON 
		delete from dbo.ShoppingCart where CustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.CustomCart where ShoppingCartRecID not in (select distinct ShoppingCartRecID from ShoppingCart);
		delete from dbo.KitCart where ShoppingCartRecID not in (select distinct ShoppingCartRecID from ShoppingCart);
		delete from dbo.CustomerSession where CustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.LOG_CustomerEvent where CustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.RatingCommentHelpfulness where RatingCustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.RatingCommentHelpfulness where VotingCustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.CouponUsage where CustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.PromotionUsage where CustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.PollVotingRecord where CustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.Address where CustomerID not in (select distinct CustomerID from Customer);
		delete from dbo.Rating where CustomerID not in (select distinct CustomerID from Customer);
	END
GO

alter procedure [dbo].[aspdnsf_NukeStoreCustomer]
		@StoreID int,
		@IncludeAdmins BIT = 0
	as
	begin
		set nocount on;

		delete cu	
		from couponusage cu
		inner join Customer c on cu.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete pu	
		from promotionusage pu
		inner join Customer c on pu.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))
		
		delete occ	
		from orders_customcart occ
		inner join Customer c on occ.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete okc	
		from orders_kitcart okc
		inner join Customer c on okc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete osc	
		from orders_ShoppingCart osc
		inner join Customer c on osc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete o	
		from orders o
		inner join Customer c on o.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete sc	
		from ShoppingCart sc
		inner join Customer c on sc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete ft	
		from failedtransaction ft
		inner join Customer c on ft.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete kc	
		from kitcart kc
		inner join Customer c on kc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete cc	
		from customcart cc
		inner join Customer c on cc.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))


		delete pvr	
		from pollvotingrecord pvr
		inner join Customer c on pvr.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete rch	
		from ratingcommenthelpfulness rch
		inner join Customer c on rch.RatingCustomerID = c.CustomerID or rch.VotingCustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete r	
		from rating r
		inner join Customer c on r.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete a	
		from Address a
		inner join Customer c on a.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete lce
		from LOG_CustomerEvent lce
		inner join Customer c on lce.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete cgrs
		from CustomerGiftRegistrySearches cgrs
		inner join Customer c on cgrs.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

		delete cs
		from CustomerSession cs
		inner join Customer c on cs.CustomerID = c.CustomerID
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))
		
		delete c
		from Customer c
		where c.StoreID = @StoreID and (@IncludeAdmins = 0 and c.IsAdmin = 0) or (@IncludeAdmins = 1 and c.IsAdmin in (0, 1, 3))

	end

GO

ALTER VIEW ObjectView 
	AS 
	SELECT	EM.EntityID AS ID,  
			EM.EntityType AS EntityType,
			EM.[Name],
			EM.Description
	FROM EntityMaster AS EM WITH (NOLOCK) 

	UNION ALL

	SELECT	tp.TopicID AS ID, 
			'Topic' AS EntityType,
			tp.[Name],
			tp.Description
	FROM Topic AS tp WITH (NOLOCK)

	UNION ALL

	SELECT	nw.NewsID AS ID,
			'News' AS EntityType,
			nw.Headline AS [Name],		
			'' AS Description
	FROM News AS nw WITH(NOLOCK)

	UNION ALL

	SELECT	p.ProductID AS ID, 
			'Product' AS EntityType,
			p.[Name],
			p.Description
	FROM Product AS p WITH(NOLOCK)

	UNION ALL

	SELECT	cp.CouponID AS ID, 
			'Coupon' AS EntityType,
			cp.[CouponCode] AS [Name],
			cp.Description
	FROM Coupon AS cp WITH(NOLOCK)

	UNION ALL

	SELECT	p.Id AS ID, 
			'Promotion' AS EntityType,
			p.[Code] AS [Name],
			p.Description
	FROM Promotions AS p WITH(NOLOCK)

	UNION ALL

	SELECT	oo.OrderOptionID AS ID, 
			'OrderOption' AS EntityType,
			oo.[Name],
			oo.Description
	FROM OrderOption oo WITH(NOLOCK)

	UNION ALL 

	SELECT	gc.GiftCardID AS ID, 
			'GiftCard' AS EntityType,
			gc.SerialNumber AS [Name],
			'' AS Description
	FROM GiftCard AS gc WITH(NOLOCK)

	UNION ALL 

	SELECT	sm.ShippingMethodID AS ID, 
			'ShippingMethod' AS EntityType,
			sm.[Name] AS [Name],
			'' AS Description
	FROM ShippingMethod AS sm WITH(NOLOCK)  

	UNION ALL

	SELECT	po.PollID AS ID,
			'Polls' AS EntityType,
			po.Name AS [Name],
			'' AS Description
	FROM Poll AS po WITH (NOLOCK)
GO

/************* End Promotions ****************/

/************* TFS 690: Mobile ********************/
IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[MobileLocaleMapping]')AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
    CREATE TABLE [dbo].[MobileLocaleMapping](
        [DesktopLocale] [nvarchar](50) NOT NULL,
        [MobileLocale] [nvarchar](50) NOT NULL,
		[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_MobileLocaleMapping_CreatedOn] DEFAULT (getdate())
    ) ON [PRIMARY]
END

If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.SkinId') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.SkinId' ,'For Developers Use Only - This value contains the skin id for your mobile skin. In standard mobile setups this AppConfig should be set to "2".','MOBILE','2','integer', null);
DECLARE @mobilelocale nvarchar(5)
SELECT @mobilelocale = ISNULL(configvalue, 'en-US') from appconfig where name = 'DefaultLocale' order by storeid desc
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.DefaultLocaleSetting') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.DefaultLocaleSetting' ,'The locale setting to use for the mobile platform.','MOBILE', @mobilelocale,'string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.IncludeEmailLinks') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.IncludeEmailLinks' ,'If this AppConfig is set to "true" the store will show a link to email the store administrator on every page. These emails will be sent to the email address listed in the built in AppConfig "GotOrderEMailFrom".','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.IsEnabled') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.IsEnabled' ,'Set this app config to false to turn off the mobile platform.','MOBILE','true','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.IncludePhoneLinks') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.IncludePhoneLinks' ,'If this is set to "true" the store will show a link to call your store’s phone number on every page. This phone number can be changed via the AppConfig "Mobile.ContactPhoneNumber".','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.TopicsShowImages') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.TopicsShowImages' ,'This feature is unsupported. Please leave set to "false".','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ContactPhoneNumber') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ContactPhoneNumber' ,'For Developers Use Only - This AppConfig allows the user to call directly from their mobile device. If this feature is enabled (via the "Mobile.IncludePhoneLinks" AppConfig) the call link will dial the phone number contained in this AppConfig.','MOBILE','1.800.555.1234','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.PageExceptions') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.PageExceptions' ,'For Developers Use Only - Pages listed in this appconfig will be excluded from the mobile platform.','MOBILE','','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.Entity.PageSize') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.Entity.PageSize' ,'This AppConfig sets the number of products to list on product listing pages.','MOBILE','3','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.Entity.ImageWidth') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.Entity.ImageWidth' ,'For Developers Use Only - This AppConfig defines the width of product image widths on entity pages. Note that a large value in this field might break the display of this field. This value does not necessarily need to match the width of your product images; they will be resized with CSS.','MOBILE','80','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ProductSlider.ImageWidth') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ProductSlider.ImageWidth' ,'For Developers Use Only - This AppConfig defines the widths of the images in the product slider. This value should space images appropriately on mobile devices when paired with the value in the "Mobile.ProductSlider.Width" AppConfig. The product slider is used for featured products, recently-viewed products, related products, upsell products, and "also bought" products.','MOBILE','60','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ProductSlider.MaxProducts') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ProductSlider.MaxProducts' ,'This AppConfig sets the maximum number of products to show in a product slider. The fewer products allowed in a product slider, the faster pages with product sliders will load. This number of products should be a evenly divisible by the value in “Mobile.ProductSlider.Width “.','MOBILE','15','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ProductSlider.Width') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ProductSlider.Width' ,'For Developers Use Only - This AppConfig defines the number of products each pane in the product slider2 should display.','MOBILE','3','integer', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ShowAlternateCheckouts') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ShowAlternateCheckouts' ,'If true then google checkout and paypal express will be shown on the shopping cart page.','MOBILE','true','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.DefaultXmlPackageEntity') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.DefaultXmlPackageEntity' ,'For Developers Use Only - This AppConfig defines the xml package that the Mobile Commerce Plug-in uses by default for entity pages. The default value for this AppConfig is "mobile.entity.default.xml.config". The associated xml package is included in the Mobile Skin.','MOBILE','mobile.entity.default.xml.config','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.DefaultXmlPackageProduct') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.DefaultXmlPackageProduct' ,'For Developers Use Only - This AppConfig defines the xml package that the Mobile Commerce Plug-in uses by default for product pages. The default value for this AppConfig is "mobile.entity.default.xml.config". The associated xml package is included in the Mobile Skin.','MOBILE','mobile.product.default.xml.config','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.UserAgentList') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.UserAgentList' ,'For Developers Use Only','MOBILE','up.browser, up.link, mmp, symbian, smartphone, midp, wap, phone, windows ce, pda, mobile, mini, palm, webos, android','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ShortUserAgentList') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ShortUserAgentList' ,'For Developers Use Only','MOBILE','w3c ,acs-,alav,alca,amoi,audi,avan,benq,bird,blac,blaz,brew,cell,cldc,cmd-,dang,doco,eric,hipt,inno,ipaq,java,jigs,kddi,keji,leno,lg-c,lg-d,lg-g,lge-,maui,maxo,midp,mits,mmef,mobi,mot-,moto,mwbp,nec-,newt,noki,palm,pana,pant,phil,play,port,prox,qwap,sage,sams,sany,sch-,sec-,send,seri,sgh-,shar,sie-,siem,smal,smar,sony,sph-,symb,t-mo,teli,tim-,tosh,tsm-,upg1,upsi,vk-v,voda,wap-,wapa,wapi,wapp,wapr,webc,winw,winw,xda,xda-','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.AllowAddressChangeOnCheckoutShipping') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.AllowAddressChangeOnCheckoutShipping' ,'Enables a dropdown box on the shipping selection page that allows the user to switch addresses.','MOBILE','true','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.AllowMultiShipOnCheckout') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.AllowMultiShipOnCheckout' ,'Not supported within the Mobile Commerce Plug-in – do not touch!','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ShowMobileOniPad') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ShowMobileOniPad' ,'If this is set to true the Mobile Platform will be showed to iPad users.','MOBILE','false','boolean', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.ThemeId') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.ThemeId' ,'The jQuery Mobile theme you would like to use. a, b, c, d, or e.','MOBILE','c','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.Action.ThemeId') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.Action.ThemeId' ,'The jQuery Mobile theme you would like to use for call-to-action buttons. a, b, c, d, or e.','MOBILE','e','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.Header.ThemeId') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.Header.ThemeId' ,'The jQuery Mobile theme you would like to use for the site header. a, b, c, d, or e.','MOBILE','b','string', null);
If Not Exists(Select * From dbo.AppConfig Where name = 'Mobile.PayPal.Express.ButtonImageURL') Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  Values ('Mobile.PayPal.Express.ButtonImageURL' ,'URL for Express Checkout Button Image used on Mobile','MOBILE','https://www.paypalobjects.com/en_US/i/btn/btn_xpressCheckout.gif','string', null);

IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Mobile.GlobalHeader') INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Mobile.GlobalHeader', 1, 0, 'Mobile.GlobalHeader', '');
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Mobile.GlobalFooter') INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Mobile.GlobalFooter', 1, 0, 'Mobile.GlobalFooter', '(!XmlPackage name="mobile.footer"!)');
IF NOT EXISTS (SELECT * FROM Topic WHERE Name='Mobile.9HomeTopIntro') INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Mobile.9HomeTopIntro', 1, 0, 'Mobile.9HomeTopIntro', '(!XmlPackage name="mobile.entitylist" entityname="section"!)<ul data-role="listview"><li class="group" data-role="list-divider"></li><li><a href="e-mobile.entitylist.aspx?entityname=category" class="fullwidth">Shop (!AppConfig name="StoreName"!)</a></li></ul>');
/************* END TFS 690: Mobile ****************/

/************* TFS 693: AvaTax Commits ****************/
if not exists (select * from AppConfig where Name = 'AvalaraTax.CommitTaxes') insert into AppConfig(Name, Description, GroupName, ConfigValue, ValueType, AllowableValues) values ('AvalaraTax.CommitTaxes', 'Set to true if AspDotNetStorefront should commit the tax document for orders. Set to false if order taxes are committed in an external system.', 'AVALARATAX', 'true', 'boolean', null);
if not exists (select * from AppConfig where Name = 'AvalaraTax.CommitRefunds') insert into AppConfig(Name, Description, GroupName, ConfigValue, ValueType, AllowableValues) values ('AvalaraTax.CommitRefunds', 'Set to true if AspDotNetStorefront should commit the tax document for refunds. Set to false if refund taxes are committed in an external system.', 'AVALARATAX', 'true', 'boolean', null);
/************* TFS 693: AvaTax Commits ****************/

update AppConfig set ValueType = 'boolean' where Name in ('KitInventory.DisableItemSelection', 'KitInventory.HideOutOfStock', 'KitInventory.ShowStockHint')

/************* TFS 751: Gift Promotions ****************/

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspdnsf_AddItemToCart]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[aspdnsf_AddItemToCart]
GO

CREATE proc [dbo].[aspdnsf_AddItemToCart]
    @CustomerID int,
    @ProductID int,
    @VariantID int,
    @Quantity int,
    @ShippingAddressID int,
    @BillingAddressID int,
    @ChosenColor nvarchar(100),
    @ChosenColorSKUModifier varchar(100),
    @ChosenSize nvarchar(100),
    @ChosenSizeSKUModifier varchar(100),
    @CleanColorOption nvarchar(100), 
    @CleanSizeOption nvarchar(100),
    @ColorAndSizePriceDelta money,
    @TextOption ntext,
    @CartType int,
    @GiftRegistryForCustomerID int,
    @CustomerEnteredPrice money,
    @CustomerLevelID int = 0,
    @RequiresCount int = 0, 		
	@IsKit2 tinyint = 0,
    @NewShoppingCartRecID int OUTPUT,
    @StoreID int, 
    @IsAGift bit = 0
AS
SET NOCOUNT ON
	DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int, @InventoryFullSKU nvarchar(50), @InventoryWeightDelta money, @AllowEmptySkuAddToCart varchar(10), @ShoppingCartrecid int, @IsAKit tinyint, @IsAPack tinyint
	DECLARE @levelprice money, @LevelDiscountPercent money, @LevelDiscountsApplyToExtendedPrices tinyint, @CustEntersPrice tinyint

	SELECT @CustEntersPrice = CustomerEntersPrice, @RestrictedQy = RestrictedQuantities FROM dbo.ProductVariant with (nolock) WHERE VariantID = @VariantID

	SELECT	@IsAKit = IsAKit, @IsAPack = IsAPack FROM dbo.Product with (nolock) WHERE ProductID = @ProductID 

	-- We are always going to ignore gift records, gift item code should be able to avoid duplicate records.
	SELECT @CurrentCartQty = Quantity From dbo.shoppingcart with (nolock) where ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and GiftRegistryForCustomerID = @GiftRegistryForCustomerID and CartType = @CartType and StoreID = @StoreID and (IsGift = 0 And @IsAGift = 0)

	DECLARE @RQty int
	IF isnull(rtrim(@RestrictedQy), '') = ''
		set @RQty = -1
	ELSE
		SELECT @RQty = cast(items as int) FROM dbo.Split(@RestrictedQy, ',') WHERE cast(items as int) <= isnull(@CurrentCartQty, 0) + @Quantity

	IF @CustomerLevelID = 0
		SELECT @LevelDiscountPercent = 0.0, @LevelDiscountsApplyToExtendedPrices = 0
	ELSE
		SELECT @LevelDiscountPercent = LevelDiscountPercent, @LevelDiscountsApplyToExtendedPrices = LevelDiscountsApplyToExtendedPrices FROM dbo.customerlevel with (nolock) WHERE customerlevelid = @CustomerLevelID    

	-- if item already exists in the cart update it's quantity
	IF @CurrentCartQty is not null and @IsAKit = 0 and @IsAPack = 0 and @CustEntersPrice = 0  BEGIN
		UPDATE dbo.ShoppingCart 
		SET Quantity = case @RQty when -1 then Quantity + @Quantity else isnull(@RQty, 0) end , 
			RequiresCount = RequiresCount + @RequiresCount 
		WHERE ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and GiftRegistryForCustomerID =
	 @GiftRegistryForCustomerID and CartType = @CartType

		SET @NewShoppingCartRecID = 0
		RETURN
	END


	SELECT @AllowEmptySkuAddToCart = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [name]= 'AllowEmptySkuAddToCart'


	--Insert item into ShoppingCart
	INSERT dbo.ShoppingCart(CartType,ShoppingCartRecGUID,CustomerID,ShippingAddressID,BillingAddressID,ProductID,SubscriptionInterval,SubscriptionIntervalType,VariantID,ProductSKU,ProductPrice,CustomerEntersPrice,ProductWeight,ProductDimensions,Quantity,RequiresCount,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,TextOption,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,FreeShipping,DistributorID,RecurringInterval,RecurringIntervalType, IsSystem, IsAKit, IsAPack, TaxClassID, IsKit2, StoreID, IsGift)
	SELECT 
		@CartType,
		newid(),
		@CustomerID,
		@ShippingAddressID,
		@BillingAddressID,
		@ProductID, 
		pv.SubscriptionInterval,
		pv.SubscriptionIntervalType,
		@VariantID,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(@ChosenColorSKUModifier, '') + isnull(@ChosenSizeSKUModifier, '') else i.VendorFullSKU end,
		case when isnull(@CustomerEnteredPrice, 0) > 0 then @CustomerEnteredPrice 
			 when p.IsAKit = 1 then dbo.GetCustomerLevelPrice(pv.VariantID, @CustomerLevelID)+((dbo.KitPriceDelta(@CustomerID, @ProductID, 0)*(100.0 - @LevelDiscountPercent))/100.0)
			 when p.IsAPack = 1 and p.PackSize = 0 then dbo.PackPriceDelta(@CustomerID, @CustomerLevelID, @ProductID, 0)+@ColorAndSizePriceDelta 
			 else dbo.GetCustomerLevelPrice(pv.VariantID, @CustomerLevelID)+@ColorAndSizePriceDelta 
		end,
		case when @CustomerEnteredPrice is not null and @CustomerEnteredPrice > 0 then 1 else 0 end,
		pv.Weight + case when p.IsAKit = 1 then dbo.KitWeightDelta(@CustomerID, @ProductID, 0) else isnull(i.WeightDelta, 0) end,
		pv.Dimensions,
		case @RQty when -1 then @Quantity else isnull(@RQty, 0) end,
		@RequiresCount,
		@ChosenColor,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then @ChosenColorSKUModifier else '' end,
		@ChosenSize,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then @ChosenSizeSKUModifier else '' end,
		@TextOption,
		pv.IsTaxable,
		pv.IsShipSeparately,
		pv.IsDownload,
		pv.DownloadLocation,
		pv.FreeShipping,
		isnull(pd.DistributorID, 0),
		case pv.RecurringInterval when 0 then 1 else pv.RecurringInterval end,
		case pv.RecurringIntervalType when 0 then -5 else pv.RecurringIntervalType end,
		p.IsSystem,
		p.IsAKit,
		p.IsAPack,
		p.TaxClassID,
		@IsKit2,
		@StoreID,
		@IsAGift
	FROM dbo.Product p with (NOLOCK) 
		join dbo.ProductVariant pv with (NOLOCK) on p.productid = pv.productid 
		left join dbo.Inventory i with (NOLOCK) on pv.VariantID = i.VariantID and i.size = @CleanSizeOption and i.color = @CleanColorOption
		left join dbo.ProductDistributor pd with (NOLOCK) on p.ProductID = pd.ProductID 
	WHERE p.ProductID = @ProductID 
		and pv.VariantID = @VariantID 
		and (@AllowEmptySkuAddToCart = 'true' or rtrim(case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(@ChosenColorSKUModifier, '') + isnull(@ChosenSizeSKUModifier, '') else i.VendorFullSKU end) <> '')

	SET @ShoppingCartrecid = @@IDENTITY

	--Update KitCart And CustomCart Tables if necessary
	IF (@IsAKit = 1 AND @IsKit2 = 0) BEGIN
		UPDATE KitCart SET ShoppingCartRecID = @ShoppingCartrecid WHERE ProductID = @ProductID and VariantID = @VariantID and ShoppingCartRecID = 0 and CustomerID = @CustomerID
	END

	IF @IsAPack = 1 BEGIN
		UPDATE CustomCart SET CartType = @CartType, ShoppingCartRecID = @ShoppingCartrecid WHERE PackID = @ProductID and ShoppingCartRecID = 0 and CustomerID = @CustomerID
	END

	SET @NewShoppingCartRecID = @ShoppingCartrecid
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspdnsf_GetShoppingCart]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[aspdnsf_GetShoppingCart]
GO

create proc [dbo].[aspdnsf_GetShoppingCart]
    @CartType tinyint, -- ShoppingCart = 0, WishCart = 1, RecurringCart = 2, GiftRegistryCart = 3
    @CustomerID int,
    @OriginalRecurringOrderNumber int,
    @OnlyLoadRecurringItemsThatAreDue tinyint,
    @StoreID int = 1
    
  
AS
BEGIN
 
    SET NOCOUNT ON 
    declare @filtershoppingcart bit, @filterproduct bit
    SELECT TOP 1 @filtershoppingcart = ConfigValue FROM GlobalConfig WHERE Name='AllowShoppingcartFiltering'
    SELECT TOP 1 @filterproduct = ConfigValue FROM GlobalConfig WHERE Name='AllowProductFiltering'
    
    SELECT
        ShoppingCart.ProductSKU,
        ShoppingCart.IsUpsell,
        ShoppingCart.Notes,
        ShoppingCart.ExtensionData,
        ShoppingCart.CustomerEntersPrice,
        ShoppingCart.NextRecurringShipDate,
        ShoppingCart.RecurringIndex,
        ShoppingCart.OriginalRecurringOrderNumber,
        ShoppingCart.RecurringSubscriptionID,
        ShoppingCart.CartType,
        ShoppingCart.ProductPrice,
        ShoppingCart.ProductWeight,
        ShoppingCart.ProductDimensions,
        ShoppingCart.SubscriptionInterval,
        ShoppingCart.SubscriptionIntervalType,
        ShoppingCart.ShoppingCartRecID,
        ShoppingCart.ProductID,
        ShoppingCart.VariantID,
        ShoppingCart.Quantity,
        ShoppingCart.IsTaxable,
        ShoppingCart.TaxClassID,
        ShoppingCart.TaxRate,
        ShoppingCart.IsShipSeparately,
        ShoppingCart.ChosenColor,
        ShoppingCart.ChosenColorSKUModifier,
        ShoppingCart.ChosenSize,
        ShoppingCart.ChosenSizeSKUModifier,
        ShoppingCart.TextOption,
        ShoppingCart.IsDownload,
        ShoppingCart.FreeShipping,
        ShoppingCart.DistributorID,
        ShoppingCart.DownloadLocation,
        ShoppingCart.CreatedOn,
        ShoppingCart.BillingAddressID as ShoppingCartBillingAddressID,
        ShoppingCart.GiftRegistryForCustomerID,
        ShoppingCart.ShippingAddressID as ShoppingCartShippingAddressID,
        ShoppingCart.ShippingMethodID,
        ShoppingCart.ShippingMethod,
        ShoppingCart.RequiresCount,
        ShoppingCart.IsSystem,
        ShoppingCart.IsAKit,
        ShoppingCart.IsAPack,
        ShoppingCart.IsGift,
        Customer.EMail,
        Customer.OrderOptions,
        Customer.OrderNotes,
        Customer.FinalizationData,
        Customer.CouponCode,
        Customer.ShippingAddressID as
        CustomerShippingAddressID,
        Customer.BillingAddressID as CustomerBillingAddressID,
        Product.Name as ProductName,
        Product.IsSystem,
        ProductVariant.name as VariantName,
        Product.TextOptionPrompt,
        Product.SizeOptionPrompt,
        Product.ColorOptionPrompt,
        ProductVariant.CustomerEntersPricePrompt,
        Product.ProductTypeId,
        Product.TaxClassId,
        Product.ManufacturerPartNumber,
        Product.ImageFileNameOverride,
        Product.SEName,
        Product.Deleted,
        ProductVariant.Weight,
        case @CartType when 2 then ShoppingCart.RecurringInterval else productvariant.RecurringInterval end RecurringInterval,
        case @CartType when 2 then ShoppingCart.RecurringIntervalType else productvariant.RecurringIntervalType end RecurringIntervalType
 
    FROM dbo.Customer with (NOLOCK)
        join dbo.ShoppingCart with (NOLOCK) ON Customer.CustomerID = ShoppingCart.CustomerID
        join dbo.Product with (NOLOCK) on ShoppingCart.ProductID=Product.ProductID
        left join dbo.ProductVariant with (NOLOCK) on ShoppingCart.VariantID=ProductVariant.VariantID
        left join dbo.Address with (NOLOCK) on Customer.ShippingAddressID=Address.AddressID
        left join dbo.country c on c.name = Address.country 
        left join dbo.State with (nolock) ON Address.State = State.Abbreviation and State.countryid = c.countryid
		inner join (select distinct a.ProductID,a.StoreID from ShoppingCart a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterproduct = 0 or a.StoreID = b.StoreID)) productstore
        on ShoppingCart.ProductID = productstore.ProductID and ShoppingCart.StoreID = productstore.StoreID
        
    WHERE ShoppingCart.CartType = @CartType
        and Product.Deleted in (0,2)
        and ProductVariant.Deleted = 0
        and Customer.customerid = @CustomerID
        and (@OriginalRecurringOrderNumber = 0 or ShoppingCart.OriginalRecurringOrderNumber = @OriginalRecurringOrderNumber)
        and (@OnlyLoadRecurringItemsThatAreDue = 0 or (@CartType = 2 and NextRecurringShipDate < dateadd(dy, 1, getdate())))
        and (@filtershoppingcart = 0 or ShoppingCart.StoreID = @StoreID)
		AND (AvailableStopDate IS NULL OR AvailableStopDate > GetDate())
     ORDER BY ShoppingCart.GiftRegistryForCustomerID,ShoppingCart.ShippingAddressID
 
END

GO

if not exists (select * from AppConfig where Name = 'Debug.DisplayOrderSummaryDiagnostics')
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  
		Values ('Debug.DisplayOrderSummaryDiagnostics' ,'Displays diagnostic subtotals in the order summary.','DEBUG', 'false','boolean', null);
GO

/************* TFS 751: Gift Promotions ****************/

/************* TFS 780                  ****************/
update [dbo].AppConfig set [Description] = '[DEPRECATED] if true, internationalcheckout.com button will be visible on your shopping cart page. You must sign up separately for an account from International Checkout if you want to use this feature.' 
	where Name like 'InternationalCheckout.Enabled'

update [dbo].AppConfig set [Description] = '[DEPRECATED] if true and InternationalCheckout is enabled, enter your InternationalCheckout.com assigned storeid here (e.g. store123)' 
	where Name like 'InternationalCheckout.StoreID'

update [dbo].AppConfig set [Description] = '[DEPRECATED] if true and InternationalCheckout is enabled, and the customer''s address is outside the U.S., their only checkout button will be the InternationalCheckout button. See AppConfig:InternationalCheckout.Enabled appconfig also.' 
	where Name like 'InternationalCheckout.ForceForInternationalCustomers'

update [dbo].AppConfig set [Description] = '[DEPRECATED] if true and InternationalCheckout is enabled, you can set this to true to see the form that is submitted to InternationalCheckout before the submission. This can help during debug/development mode' 
	where Name like 'InternationalCheckout.TestMode'
/************* TFS 780                  ****************/

/************* TFS 742: Promotions and Avalara ****************/
update AppConfig set Description = 'This AppConfig is no longer used.' where Name = 'AvalaraTax.TaxAddress'
/************* TFS 742: Promotions and Avalara ****************/

/************* TFS 821: Order Shipments ****************/
if not exists (select * From sysobjects where id = object_id('OrderShipment') and type = 'u')
	create table dbo.OrderShipment (
		OrderShipmentID int not null primary key identity(1, 1),
		OrderNumber int not null,
		AddressID int not null,
		ShippingTotal money not null
	)
/************* TFS 821: Order Shipments ****************/

if not exists (select * from AppConfig where Name = 'Debug.CouponMigrated')
	begin
	
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  
		Values ('Debug.CouponMigrated' ,'Flag to determine if coupons have been migrated over to promotions.','DEBUG', 'true','boolean', null);

	print ('Migrating coupons...');
	declare @CouponId int
	declare @CouponGuid uniqueidentifier
	declare @CouponCode nvarchar(50)
	declare @CouponDescription nvarchar(max)
	declare @CouponStartDate datetime
	declare @CouponExpirationDate datetime
	declare @CouponExpireAfterUseByAnyCustomer int
	declare @CouponExpireAfterUseByEachCustomer int
	declare @CouponExpireAfterNUses int
	declare @CouponRequiresMinimumOrderAmount money
	declare @CouponValidForCustomers varchar(max)
	declare @CouponValidForProducts varchar(max)
	declare @CouponValidForManufacturers varchar(max)
	declare @CouponValidForCategories varchar(max)
	declare @CouponValidForSections varchar(max)
	declare @CouponDiscountPercent money
	declare @CouponDiscountAmount money
	declare @CouponType int
	declare @CouponDiscountIncludesFreeShipping bit
	declare @CouponActive tinyint

	declare CouponCursor cursor for 
		select CouponId
			, CouponGuid
			, CouponCode
			, isnull(Description, '') 
			, StartDate
			, ExpirationDate 
			, DiscountPercent
			, DiscountAmount
			, CouponType
			, DiscountIncludesFreeShipping
			, ExpiresOnFirstUseByAnyCustomer
			, ExpiresAfterOneUsageByEachCustomer
			, ExpiresAfterNUses
			, isnull(RequiresMinimumOrderAmount, 0)
			, isnull(ValidForCustomers, '')
			, isnull(ValidForProducts, '')
			, isnull(ValidForManufacturers, '')
			, isnull(ValidForCategories, '')
			, isnull(ValidForSections, '')
			, case Deleted when 0 then 1 else 0 end
		from Coupon

	open CouponCursor

	fetch next from CouponCursor into 
		@CouponId
		, @CouponGuid 
		, @CouponCode
		, @CouponDescription 
		, @CouponStartDate
		, @CouponExpirationDate
		, @CouponDiscountPercent
		, @CouponDiscountAmount
		, @CouponType
		, @CouponDiscountIncludesFreeShipping
		, @CouponExpireAfterUseByAnyCustomer
		, @CouponExpireAfterUseByEachCustomer
		, @CouponExpireAfterNUses
		, @CouponRequiresMinimumOrderAmount
		, @CouponValidForCustomers
		, @CouponValidForProducts
		, @CouponValidForManufacturers
		, @CouponValidForCategories
		, @CouponValidForSections
		, @CouponActive
	
	while (@@fetch_status <> -1)
	begin

		-- Promotions
		declare @CouponRuleData xml
		declare @CouponDiscountData xml
		declare @PromotionId int
		declare @PromotionDiscountBaseType nvarchar(50);
	
		Set @PromotionDiscountBaseType = Case When @CouponType = 0 Then 'OrderPromotionDiscount' Else 'OrderItemPromotionDiscount' End;
	
		-- Build expiration per use xml
		declare @RuleExpirationUseXml varchar(max)
		set @RuleExpirationUseXml = ''
	
		if (@CouponExpireAfterUseByEachCustomer = 1)
			begin
			set @RuleExpirationUseXml = '<PromotionRuleBase xsi:type="ExpirationNumberOfUsesPerCustomerPromotionRule"><NumberOfUsesAllowed>1</NumberOfUsesAllowed></PromotionRuleBase>'
			end
		else if (@CouponExpireAfterUseByEachCustomer = 0 and @CouponExpireAfterUseByAnyCustomer = 1)
			begin
			set @CouponExpireAfterNUses = 1
			set @RuleExpirationUseXml = '<PromotionRuleBase xsi:type="ExpirationNumberOfUsesPromotionRule"><NumberOfUsesAllowed>' + convert(varchar, @CouponExpireAfterNUses) + '</NumberOfUsesAllowed></PromotionRuleBase>'
			end
		else if (@CouponExpireAfterUseByEachCustomer = 0 and @CouponExpireAfterUseByAnyCustomer = 0 and @CouponExpireAfterNUses > 0)
			begin
			set @CouponExpireAfterNUses = @CouponExpireAfterNUses
			set @RuleExpirationUseXml = '<PromotionRuleBase xsi:type="ExpirationNumberOfUsesPromotionRule"><NumberOfUsesAllowed>' + convert(varchar, @CouponExpireAfterNUses) + '</NumberOfUsesAllowed></PromotionRuleBase>'
			end

		-- Build minimum cart amount rule xml
		declare @RuleMinimumCartAmountXml varchar(max)
		set @RuleMinimumCartAmountXml = ''
	
		if (@CouponRequiresMinimumOrderAmount > 0)
			begin	
			set @RuleMinimumCartAmountXml = '<PromotionRuleBase xsi:type="MinimumCartAmountPromotionRule"><CartAmount>' + convert(varchar, @CouponRequiresMinimumOrderAmount) + '</CartAmount></PromotionRuleBase>'
			end
	
		-- Build email address rule xml	
  		declare @RuleEmailAddressRequiredXml varchar(max)
		set @RuleEmailAddressRequiredXml = ''
	
		if (len(@CouponValidForCustomers) > 0)
			begin
			declare @CustomerEmailString varchar(max)
			set @CustomerEmailString = ''
			select @CustomerEmailString = @CustomerEmailString + '<string>' + email + '</string>' from Customer where CustomerId in (select * from dbo.Split(@CouponValidForCustomers, ','))
			set @RuleEmailAddressRequiredXml = '<PromotionRuleBase xsi:type="EmailAddressPromotionRule"><EmailAddresses>' + @CustomerEmailString + '</EmailAddresses></PromotionRuleBase>'
			end
	
		-- Build valid for product xml
		declare @RuleValidForProductXml varchar(max)
		set @RuleValidForProductXml = ''
		if (len(@CouponValidForProducts) > 0)
			begin
				declare @ProductIdString varchar(max)
				set @ProductIdString = ''
				select @ProductIdString = @ProductIdString + '<int>' + Items + '</int>' from dbo.Split(@CouponValidForProducts, ',')
				set @RuleValidForProductXml = '<PromotionRuleBase xsi:type="ProductIdPromotionRule"><ProductIds>' + @ProductIdString + '</ProductIds><RequireQuantity>false</RequireQuantity><Quantity>1</Quantity><AndTogether>false</AndTogether></PromotionRuleBase>'
			end
  
		-- Build valid for category xml
		declare @RuleValidForCategoryXml varchar(max)
		set @RuleValidForCategoryXml = ''
		if (len(@CouponValidForCategories) > 0)
			begin
				declare @CategoryIdString varchar(max)
				set @CategoryIdString = ''
				select @CategoryIdString = @CategoryIdString + '<int>' + Items + '</int>' from dbo.Split(@CouponValidForCategories, ',')
				set @RuleValidForCategoryXml = '<PromotionRuleBase xsi:type="CategoryPromotionRule"><CategoryIds>' + @CategoryIdString + '</CategoryIds></PromotionRuleBase>'
			end
	
		-- Build valid for Manufacturers xml
		declare @RuleValidForManufacturersXml varchar(max)
		set @RuleValidForManufacturersXml = ''
		if (len(@CouponValidForManufacturers) > 0)
			begin
				declare @ManufacturerIdString varchar(max)
				set @ManufacturerIdString = ''
				select @ManufacturerIdString = @ManufacturerIdString + '<int>' + Items + '</int>' from dbo.Split(@CouponValidForManufacturers, ',')
				set @RuleValidForManufacturersXml = '<PromotionRuleBase xsi:type="ManufacturerPromotionRule"><ManufacturerIds>' + @ManufacturerIdString + '</ManufacturerIds></PromotionRuleBase>'
			end
	
		-- Build valid for Manufacturers xml
		declare @RuleValidForSectionsXml varchar(max)
		set @RuleValidForSectionsXml = ''
		if (len(@CouponValidForSections) > 0)
			begin
				declare @SectionIdString varchar(max)
				set @SectionIdString = ''
				select @SectionIdString = @SectionIdString + '<int>' + Items + '</int>' from dbo.Split(@CouponValidForSections, ',')
				set @RuleValidForSectionsXml = '<PromotionRuleBase xsi:type="SectionPromotionRule"><SectionIds>' + @SectionIdString + '</SectionIds></PromotionRuleBase>'
			end
		
		-- Build rule data xml
		set @CouponRuleData = cast('<ArrayOfPromotionRuleBase xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
		  <PromotionRuleBase xsi:type="StartDatePromotionRule">
			<StartDate>' + convert(varchar, @CouponStartDate, 126) + '</StartDate>
		  </PromotionRuleBase>
		  <PromotionRuleBase xsi:type="ExpirationDatePromotionRule">
			<ExpirationDate>' + convert(varchar, @CouponExpirationDate, 126) + '</ExpirationDate>
		  </PromotionRuleBase>
		  ' + isnull(@RuleExpirationUseXml, '') + '
		  ' + isnull(@RuleMinimumCartAmountXml, '') + '
		  ' + isnull(@RuleEmailAddressRequiredXml, '') + '
		  ' + isnull(@RuleValidForProductXml, '') + '
		  ' + isnull(@RuleValidForCategoryXml, '') + '
		  ' + isnull(@RuleValidForManufacturersXml, '') + '
		  ' + isnull(@RuleValidForSectionsXml, '') + '
		</ArrayOfPromotionRuleBase>' as xml)

		declare @DiscountIncludeFreeShippingXml varchar(max)
		set @DiscountIncludeFreeShippingXml = ''
		if (@CouponDiscountIncludesFreeShipping > 0)
			begin
				set @DiscountIncludeFreeShippingXml = '<PromotionDiscountBase xsi:type="ShippingPromotionDiscount"><DiscountType>Percentage</DiscountType><DiscountAmount>1</DiscountAmount></PromotionDiscountBase>'
			end
	
		if @CouponDiscountPercent > 0  
			set @CouponDiscountData = cast('<ArrayOfPromotionDiscountBase xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
			  <PromotionDiscountBase xsi:type="' + @PromotionDiscountBaseType + '">
				<DiscountType>Percentage</DiscountType>
				<DiscountAmount>' + convert(varchar, @CouponDiscountPercent/100) + '</DiscountAmount>
			  </PromotionDiscountBase>
			  ' + isnull(@DiscountIncludeFreeShippingXml, '') + '
			</ArrayOfPromotionDiscountBase>' as xml)
		else 
			set @CouponDiscountData = cast('<ArrayOfPromotionDiscountBase xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
			  <PromotionDiscountBase xsi:type="' + @PromotionDiscountBaseType + '">
				<DiscountType>Fixed</DiscountType>
				<DiscountAmount>' + convert(varchar, @CouponDiscountAmount) + '</DiscountAmount>
			  </PromotionDiscountBase>
			  ' + isnull(@DiscountIncludeFreeShippingXml, '') + '
			</ArrayOfPromotionDiscountBase>' as xml)
	
		insert into Promotions (PromotionGuid, Name, [Description], UsageText, EmailText, Code, [Priority], Active, AutoAssigned, PromotionRuleData, PromotionDiscountData)
			values (@CouponGuid, @CouponCode, @CouponDescription, @CouponDescription, @CouponDescription, @CouponCode, 1, @CouponActive, 0, @CouponRuleData, @CouponDiscountData)
		set @PromotionId = @@identity
		
		-- Promotion Usage
		insert into PromotionUsage (PromotionId, CustomerId, OrderId, DateApplied, DiscountAmount, Complete)
			select @PromotionId, CustomerId, 0, CreatedOn, 0, 1 from CouponUsage where CouponCode = @CouponCode
	
		-- Promotion store
		insert into PromotionStore (PromotionId, StoreId, CreatedOn)
			select @PromotionId, StoreId, createdOn from CouponStore where CouponId = @CouponId
	
		-- Expire the old coupon record
		if (@CouponExpirationDate > getdate())
			update Coupon set ExpirationDate = getdate() where CouponId = @CouponId
			
		fetch next from CouponCursor into 
			@CouponId
			, @CouponGuid
			, @CouponCode
			, @CouponDescription
			, @CouponStartDate
			, @CouponExpirationDate
			, @CouponDiscountPercent
			, @CouponDiscountAmount
			, @CouponType
			, @CouponDiscountIncludesFreeShipping
			, @CouponExpireAfterUseByAnyCustomer
			, @CouponExpireAfterUseByEachCustomer
			, @CouponExpireAfterNUses
			, @CouponRequiresMinimumOrderAmount
			, @CouponValidForCustomers
			, @CouponValidForProducts
			, @CouponValidForManufacturers
			, @CouponValidForCategories
			, @CouponValidForSections
			, @CouponActive		
	end

	close CouponCursor
	deallocate CouponCursor
	end
GO

UPDATE dbo.[AppConfig] SET ConfigValue = 'PayPal' WHERE [Name] = 'PayFlowPro.PARTNER';
GO


---------------- 9.3.1.0 -------------------------------------
IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[aspdnsf_getAddressesByCustomer]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_getAddressesByCustomer]
GO

CREATE PROCEDURE [dbo].[aspdnsf_getAddressesByCustomer]
(
	@CustomerID int
)
AS
BEGIN
SET NOCOUNT ON
	SELECT	[AddressID],
			[CustomerID],
			[NickName],
			[FirstName], 
			[LastName],
			[Company], 
			[Address1], 
			[Address2], 
			[Suite], 
			[City], 
			[State], 
			[Zip], 
			[Country], 
			[ResidenceType], 
			[Phone], 
			[Email]
	FROM [Address]
	WHERE [CustomerID] = @CustomerID
		AND [Deleted] = 0
		AND Address1 NOT LIKE '%Hidden By Amazon%'
END
GO

--New USPS Tracking URL--
UPDATE dbo.[AppConfig] SET ConfigValue = 'https://tools.usps.com/go/TrackConfirmAction_input?origTrackNum={0}' WHERE [Name] = 'ShippingTrackingURL.USPS' AND ConfigValue = 'http://trkcnfrm1.smi.usps.com/PTSInternetWeb/InterLabelInquiry.do?origTrackNum={0}';
GO

--Make topics publishable--
PRINT 'Updating Topic Table...'
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('Topic') AND name = 'Published') 
    ALTER TABLE dbo.Topic ADD Published [bit] NOT NULL CONSTRAINT DF_Topic_Published DEFAULT((1))

/*********************Google Trusted Stores Integration ********************/
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreProductSearchID')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreProductSearchID', 'Account ID from Product Search. This value should match the account ID you use to submit your product data feed you submit to Google Product Search. (If you have an MCA account, use the subaccount ID associated with that product feed.)', '', 'string', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreCountry')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreCountry', 'Account country from Product Search. This value should match the account country you use to submit your product data feed to Google Product Search.    The value of the country parameter should be a <a target="blank" style="text-decoration: underline;" href="http://www.iso.org/iso/country_codes/iso_3166_code_lists.htm">two-letter ISO 3166 country code</a>.', 'US', 'string', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreLanguage')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreLanguage', 'Account language from Product Search. This value should match the account language you use to submit your product data feed to Google Product Search.    The value of the language parameter should be a <a target="blank" style="text-decoration: underline;" href="http://www.loc.gov/standards/iso639-2/php/English_list.php">two-letter ISO 639-1 language code</a>.', 'en', 'string', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreID')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreID', 'This is your Google Trusted Store account ID, which can be gotten <a target="blank" style="text-decoration: underline;" href="http://www.google.com/trustedstores/sell/setupcode">here</a>.  Look for the code on this line:    gts.push(["id", "xxxxx"]);', '', 'string', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreEnabled')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreEnabled', 'If true, the Google Trusted Store javascript will be added to your orderconfirmation.aspx page.', 'false', 'boolean', null, 'Google Trusted Store', 0, 0, getdate());
if not exists(select * from AppConfig Where Name='GoogleTrustedStoreShippingLeadTime')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'GoogleTrustedStoreShippingLeadTime', 'Average estimated number of days before a new order ships (whole numbers only).  Be as accurate as possible without shorting this value - this factors into your store''s trust rating.', '', 'integer', null, 'Google Trusted Store', 0, 0, getdate());
/*********************Google Trusted Stores Integration ********************/

/*********************UpdatedOn and CreatedOn Columns & DB Triggers************************/
--Add the UpdatedOn column to tables that don't have it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Address' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Address ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Address_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Affiliate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Affiliate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Affiliate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivity' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AffiliateActivity ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivity_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivityReason' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AffiliateActivityReason ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivityReason_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateCommissions' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AffiliateCommissions ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateCommissions_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AffiliateStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AppConfig' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AppConfig ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AppConfig_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'aspdnsf_SysLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE aspdnsf_SysLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_aspdnsf_SysLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE AuditLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_AuditLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BadWord' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE BadWord ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_BadWord_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Category' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Category ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Category_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CIM_AddressPaymentProfileMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CIM_AddressPaymentProfileMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CIM_AddressPaymentProfileMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClickTrack' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ClickTrack ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ClickTrack_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Country' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Country ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Country_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CountryTaxRate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CountryTaxRate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CountryTaxRate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Coupon' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Coupon ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Coupon_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CouponStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CouponStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponUsage' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CouponUsage ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CouponUsage_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CreditCardType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CreditCardType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CreditCardType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Currency' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Currency ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Currency_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Customer ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Customer_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerGiftRegistrySearches' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CustomerGiftRegistrySearches ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerGiftRegistrySearches_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerLevel' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CustomerLevel ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerLevel_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerSession' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CustomerSession ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerSession_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomReport' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE CustomReport ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_CustomReport_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Distributor' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Distributor ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Distributor_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Document' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Document ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Document_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentAffiliate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE DocumentAffiliate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentAffiliate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentCustomerLevel' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE DocumentCustomerLevel ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentCustomerLevel_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentLibrary' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE DocumentLibrary ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentLibrary_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE DocumentType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EntityStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE EntityStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_EntityStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ErrorLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorMessage' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ErrorMessage ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorMessage_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EventHandler' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE EventHandler ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_EventHandler_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExtendedPrice' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ExtendedPrice ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ExtendedPrice_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FailedTransaction' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE FailedTransaction ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_FailedTransaction_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FAQ' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE FAQ ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_FAQ_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Feed' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Feed ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Feed_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Gallery' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Gallery ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Gallery_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Genre' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Genre ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Genre_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCard' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE GiftCard ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCard_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE GiftCardStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardUsage' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE GiftCardUsage ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardUsage_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GlobalConfig' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE GlobalConfig ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_GlobalConfig_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Inventory ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Inventory_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitCart' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE KitCart ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_KitCart_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroup' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE KitGroup ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroup_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroupType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE KitGroupType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroupType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitItem' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE KitItem ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_KitItem_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Layout' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Layout ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Layout_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutField' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE LayoutField ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutField_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutFieldAttribute' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE LayoutFieldAttribute ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutFieldAttribute_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE LayoutMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Library' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Library ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Library_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LocaleSetting' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE LocaleSetting ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_LocaleSetting_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LOG_CustomerEvent' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE LOG_CustomerEvent ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_LOG_CustomerEvent_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LOG_Event' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE LOG_Event ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_LOG_Event_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MailingMgrLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE MailingMgrLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_MailingMgrLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Manufacturer' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Manufacturer ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Manufacturer_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileDevice' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE MobileDevice ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_MobileDevice_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileLocaleMapping' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE MobileLocaleMapping ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_MobileLocaleMapping_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MultiShipOrder_Shipment' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE MultiShipOrder_Shipment ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_MultiShipOrder_Shipment_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'News' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE News ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_News_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsletterMailList' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE NewsletterMailList ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_NewsletterMailList_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE NewsStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_NewsStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderNumbers' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderNumbers ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderNumbers_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOption' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderOption ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOption_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOptionStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderOptionStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOptionStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Orders ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_KitCart' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Orders_KitCart ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_KitCart_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Orders_ShoppingCart ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_ShoppingCart_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderShipment' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderShipment ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderShipment_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderTransaction' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE OrderTransaction ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_OrderTransaction_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PageType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PageType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PageType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Partner' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Partner ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Partner_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PasswordLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PasswordLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PasswordLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Poll' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Poll ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Poll_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollAnswer' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PollAnswer ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PollAnswer_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollCategory' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PollCategory ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PollCategory_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollSection' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PollSection ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PollSection_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollSortOrder' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PollSortOrder ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PollSortOrder_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PollStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PollStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollVotingRecord' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PollVotingRecord ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PollVotingRecord_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Product ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Product_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductAffiliate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductAffiliate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductAffiliate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCategory' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductCategory ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCategory_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCustomerLevel' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductCustomerLevel ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCustomerLevel_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductDistributor' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductDistributor ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductDistributor_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductGenre' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductGenre ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductGenre_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductLocaleSetting' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductLocaleSetting ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductLocaleSetting_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductManufacturer' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductManufacturer ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductManufacturer_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductSection' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductSection ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductSection_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductType' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductType ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductType_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariant' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductVariant ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVariant_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVector' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductVector ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVector_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductView' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ProductView ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ProductView_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Profile' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Profile ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Profile_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionLineItem' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PromotionLineItem ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionLineItem_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Promotions' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Promotions ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Promotions_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PromotionStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionUsage' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE PromotionUsage ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionUsage_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscount' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE QuantityDiscount ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscount_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscountTable' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE QuantityDiscountTable ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscountTable_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rating' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Rating ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Rating_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RatingCommentHelpfulness' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE RatingCommentHelpfulness ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_RatingCommentHelpfulness_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RestrictedIP' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE RestrictedIP ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_RestrictedIP_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesPrompt' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE SalesPrompt ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_SalesPrompt_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SearchLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE SearchLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_SearchLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Section' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Section ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Section_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SecurityLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE SecurityLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_SecurityLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByProduct' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingByProduct ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByProduct_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotal' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingByTotal ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotal_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotalByPercent' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingByTotalByPercent ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotalByPercent_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByWeight' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingByWeight ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByWeight_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculation' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingCalculation ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculation_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculationStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingCalculationStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculationStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingImportExport' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingImportExport ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingImportExport_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethod' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethod ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethod_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethodStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToCountryMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethodToCountryMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToCountryMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToStateMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethodToStateMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToStateMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToZoneMap' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingMethodToZoneMap ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToZoneMap_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingTotalByZone' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingTotalByZone ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingTotalByZone_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingWeightByZone' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingWeightByZone ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingWeightByZone_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingZone' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShippingZone ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingZone_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShoppingCart' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ShoppingCart ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ShoppingCart_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SkinPreview' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE SkinPreview ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_SkinPreview_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SQLLog' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE SQLLog ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_SQLLog_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Staff' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Staff ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Staff_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'State' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE State ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_State_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StateTaxRate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE StateTaxRate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_StateTaxRate_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Store' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Store ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Store_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StringResource' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE StringResource ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_StringResource_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TaxClass' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE TaxClass ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_TaxClass_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Topic' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Topic ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Topic_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicMapping' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE TopicMapping ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_TopicMapping_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicStore' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE TopicStore ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_TopicStore_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vector' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE Vector ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_Vector_UpdatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ZipTaxRate' AND COLUMN_NAME = 'UpdatedOn')
	ALTER TABLE ZipTaxRate ADD UpdatedOn DATETIME NOT NULL CONSTRAINT DF_ZipTaxRate_UpdatedOn DEFAULT(getdate())
	GO
--Add the CreatedOn column to tables that don't have it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Address' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Address ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Address_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Affiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Affiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Affiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivity' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateActivity ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivity_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivityReason' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateActivityReason ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivityReason_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateCommissions' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateCommissions ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateCommissions_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AppConfig' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AppConfig ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AppConfig_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'aspdnsf_SysLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE aspdnsf_SysLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_aspdnsf_SysLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AuditLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AuditLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BadWord' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE BadWord ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_BadWord_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Category' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Category ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Category_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CIM_AddressPaymentProfileMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CIM_AddressPaymentProfileMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CIM_AddressPaymentProfileMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClickTrack' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ClickTrack ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ClickTrack_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Country' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Country ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Country_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CountryTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CountryTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CountryTaxRate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Coupon' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Coupon ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Coupon_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CouponStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CouponStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CouponUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CouponUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CreditCardType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CreditCardType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CreditCardType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Currency' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Currency ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Currency_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Customer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Customer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerGiftRegistrySearches' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerGiftRegistrySearches ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerGiftRegistrySearches_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerSession' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerSession ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerSession_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomReport' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomReport ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomReport_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Distributor' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Distributor ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Distributor_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Document' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Document ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Document_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentAffiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentAffiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentAffiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentCustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentCustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentCustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentLibrary' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentLibrary ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentLibrary_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EntityStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE EntityStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_EntityStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ErrorLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorMessage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ErrorMessage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorMessage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EventHandler' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE EventHandler ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_EventHandler_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExtendedPrice' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ExtendedPrice ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ExtendedPrice_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FailedTransaction' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE FailedTransaction ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_FailedTransaction_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FAQ' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE FAQ ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_FAQ_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Feed' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Feed ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Feed_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Gallery' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Gallery ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Gallery_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Genre' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Genre ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Genre_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCard' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCard ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCard_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCardStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCardUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GlobalConfig' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GlobalConfig ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GlobalConfig_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Inventory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Inventory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroup' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitGroup ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroup_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroupType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitGroupType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroupType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitItem' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitItem ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitItem_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Layout' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Layout ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Layout_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutField' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LayoutField ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutField_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutFieldAttribute' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LayoutFieldAttribute ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutFieldAttribute_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LayoutMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Library' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Library ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Library_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LocaleSetting' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LocaleSetting ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LocaleSetting_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LOG_CustomerEvent' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LOG_CustomerEvent ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LOG_CustomerEvent_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LOG_Event' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LOG_Event ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LOG_Event_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MailingMgrLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MailingMgrLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MailingMgrLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Manufacturer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Manufacturer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Manufacturer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileDevice' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MobileDevice ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MobileDevice_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileLocaleMapping' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MobileLocaleMapping ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MobileLocaleMapping_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MultiShipOrder_Shipment' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MultiShipOrder_Shipment ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MultiShipOrder_Shipment_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'News' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE News ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_News_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsletterMailList' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE NewsletterMailList ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_NewsletterMailList_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE NewsStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_NewsStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderNumbers' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderNumbers ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderNumbers_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOption' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderOption ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOption_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOptionStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderOptionStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOptionStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_KitCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders_KitCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_KitCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders_ShoppingCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_ShoppingCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderShipment' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderShipment ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderShipment_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderTransaction' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderTransaction ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderTransaction_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PageType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PageType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PageType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Partner' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Partner ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Partner_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PasswordLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PasswordLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PasswordLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Poll' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Poll ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Poll_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollAnswer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollAnswer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollAnswer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollCategory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollCategory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollCategory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollSection' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollSection ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollSection_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollSortOrder' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollSortOrder ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollSortOrder_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollVotingRecord' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollVotingRecord ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollVotingRecord_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Product ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Product_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductAffiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductAffiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductAffiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCategory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductCategory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCategory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductCustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductDistributor' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductDistributor ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductDistributor_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductGenre' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductGenre ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductGenre_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductLocaleSetting' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductLocaleSetting ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductLocaleSetting_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductManufacturer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductManufacturer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductManufacturer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductSection' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductSection ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductSection_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariant' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductVariant ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVariant_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVector' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductVector ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVector_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductView' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductView ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductView_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Profile' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Profile ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Profile_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionLineItem' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionLineItem ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionLineItem_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Promotions' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Promotions ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Promotions_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscount' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE QuantityDiscount ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscount_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscountTable' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE QuantityDiscountTable ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscountTable_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rating' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Rating ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Rating_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RatingCommentHelpfulness' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE RatingCommentHelpfulness ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_RatingCommentHelpfulness_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RestrictedIP' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE RestrictedIP ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_RestrictedIP_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesPrompt' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SalesPrompt ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SalesPrompt_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SearchLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SearchLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SearchLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Section' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Section ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Section_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SecurityLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SecurityLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SecurityLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByProduct' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByProduct ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByProduct_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotal' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByTotal ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotal_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotalByPercent' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByTotalByPercent ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotalByPercent_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByWeight' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByWeight ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByWeight_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculation' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingCalculation ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculation_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculationStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingCalculationStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculationStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingImportExport' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingImportExport ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingImportExport_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethod' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethod ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethod_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToCountryMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToCountryMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToCountryMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToStateMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToStateMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToStateMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToZoneMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToZoneMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToZoneMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingTotalByZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingTotalByZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingTotalByZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingWeightByZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingWeightByZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingWeightByZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShoppingCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShoppingCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShoppingCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SkinPreview' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SkinPreview ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SkinPreview_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SQLLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SQLLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SQLLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Staff' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Staff ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Staff_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'State' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE State ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_State_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StateTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE StateTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StateTaxRate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Store' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Store ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Store_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StringResource' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE StringResource ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StringResource_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TaxClass' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TaxClass ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TaxClass_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Topic' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Topic ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Topic_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicMapping' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TopicMapping ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TopicMapping_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TopicStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TopicStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vector' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Vector ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Vector_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ZipTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ZipTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ZipTaxRate_CreatedOn DEFAULT(getdate())
	GO
	
--Add the CreatedOn column to tables that don't have it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Address' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Address ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Address_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Affiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Affiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Affiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivity' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateActivity ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivity_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateActivityReason' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateActivityReason ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateActivityReason_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateCommissions' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateCommissions ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateCommissions_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AffiliateStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AffiliateStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AffiliateStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AppConfig' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AppConfig ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AppConfig_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'aspdnsf_SysLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE aspdnsf_SysLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_aspdnsf_SysLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AuditLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE AuditLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_AuditLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BadWord' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE BadWord ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_BadWord_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Category' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Category ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Category_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CIM_AddressPaymentProfileMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CIM_AddressPaymentProfileMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CIM_AddressPaymentProfileMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ClickTrack' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ClickTrack ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ClickTrack_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Country' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Country ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Country_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CountryTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CountryTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CountryTaxRate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Coupon' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Coupon ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Coupon_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CouponStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CouponStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CouponUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CouponUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CouponUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CreditCardType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CreditCardType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CreditCardType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Currency' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Currency ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Currency_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Customer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Customer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerGiftRegistrySearches' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerGiftRegistrySearches ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerGiftRegistrySearches_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomerSession' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomerSession ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomerSession_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CustomReport' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE CustomReport ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_CustomReport_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Distributor' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Distributor ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Distributor_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Document' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Document ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Document_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentAffiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentAffiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentAffiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentCustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentCustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentCustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentLibrary' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentLibrary ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentLibrary_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DocumentType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE DocumentType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_DocumentType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EntityStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE EntityStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_EntityStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ErrorLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ErrorMessage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ErrorMessage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ErrorMessage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'EventHandler' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE EventHandler ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_EventHandler_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExtendedPrice' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ExtendedPrice ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ExtendedPrice_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FailedTransaction' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE FailedTransaction ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_FailedTransaction_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FAQ' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE FAQ ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_FAQ_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Feed' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Feed ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Feed_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Gallery' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Gallery ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Gallery_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Genre' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Genre ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Genre_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCard' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCard ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCard_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCardStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GiftCardUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GiftCardUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GiftCardUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GlobalConfig' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE GlobalConfig ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_GlobalConfig_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Inventory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Inventory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Inventory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroup' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitGroup ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroup_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitGroupType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitGroupType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitGroupType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KitItem' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE KitItem ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_KitItem_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Layout' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Layout ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Layout_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutField' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LayoutField ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutField_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutFieldAttribute' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LayoutFieldAttribute ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutFieldAttribute_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LayoutMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LayoutMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LayoutMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Library' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Library ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Library_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LocaleSetting' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LocaleSetting ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LocaleSetting_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LOG_CustomerEvent' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LOG_CustomerEvent ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LOG_CustomerEvent_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LOG_Event' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE LOG_Event ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_LOG_Event_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MailingMgrLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MailingMgrLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MailingMgrLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Manufacturer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Manufacturer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Manufacturer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileDevice' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MobileDevice ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MobileDevice_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MobileLocaleMapping' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MobileLocaleMapping ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MobileLocaleMapping_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'MultiShipOrder_Shipment' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE MultiShipOrder_Shipment ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_MultiShipOrder_Shipment_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'News' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE News ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_News_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsletterMailList' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE NewsletterMailList ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_NewsletterMailList_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NewsStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE NewsStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_NewsStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderNumbers' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderNumbers ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderNumbers_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOption' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderOption ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOption_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderOptionStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderOptionStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderOptionStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_KitCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders_KitCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_KitCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Orders_ShoppingCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Orders_ShoppingCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderShipment' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderShipment ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderShipment_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderTransaction' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE OrderTransaction ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_OrderTransaction_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PageType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PageType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PageType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Partner' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Partner ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Partner_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PasswordLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PasswordLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PasswordLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Poll' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Poll ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Poll_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollAnswer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollAnswer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollAnswer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollCategory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollCategory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollCategory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollSection' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollSection ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollSection_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollSortOrder' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollSortOrder ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollSortOrder_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PollVotingRecord' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PollVotingRecord ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PollVotingRecord_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Product ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Product_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductAffiliate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductAffiliate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductAffiliate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCategory' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductCategory ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCategory_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductCustomerLevel' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductCustomerLevel ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductCustomerLevel_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductDistributor' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductDistributor ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductDistributor_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductGenre' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductGenre ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductGenre_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductLocaleSetting' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductLocaleSetting ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductLocaleSetting_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductManufacturer' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductManufacturer ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductManufacturer_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductSection' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductSection ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductSection_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductType' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductType ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductType_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariant' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductVariant ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVariant_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVector' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductVector ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductVector_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductView' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ProductView ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ProductView_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Profile' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Profile ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Profile_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionLineItem' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionLineItem ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionLineItem_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Promotions' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Promotions ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Promotions_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PromotionUsage' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE PromotionUsage ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_PromotionUsage_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscount' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE QuantityDiscount ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscount_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuantityDiscountTable' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE QuantityDiscountTable ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_QuantityDiscountTable_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Rating' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Rating ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Rating_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RatingCommentHelpfulness' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE RatingCommentHelpfulness ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_RatingCommentHelpfulness_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RestrictedIP' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE RestrictedIP ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_RestrictedIP_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SalesPrompt' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SalesPrompt ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SalesPrompt_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SearchLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SearchLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SearchLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Section' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Section ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Section_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SecurityLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SecurityLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SecurityLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByProduct' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByProduct ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByProduct_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotal' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByTotal ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotal_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByTotalByPercent' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByTotalByPercent ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByTotalByPercent_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingByWeight' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingByWeight ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingByWeight_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculation' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingCalculation ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculation_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingCalculationStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingCalculationStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingCalculationStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingImportExport' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingImportExport ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingImportExport_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethod' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethod ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethod_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToCountryMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToCountryMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToCountryMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToStateMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToStateMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToStateMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingMethodToZoneMap' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingMethodToZoneMap ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingMethodToZoneMap_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingTotalByZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingTotalByZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingTotalByZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingWeightByZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingWeightByZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingWeightByZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShippingZone' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShippingZone ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShippingZone_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ShoppingCart' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ShoppingCart ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ShoppingCart_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SkinPreview' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SkinPreview ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SkinPreview_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SQLLog' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE SQLLog ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_SQLLog_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Staff' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Staff ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Staff_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'State' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE State ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_State_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StateTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE StateTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StateTaxRate_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Store' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Store ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Store_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'StringResource' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE StringResource ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_StringResource_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TaxClass' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TaxClass ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TaxClass_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Topic' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Topic ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Topic_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicMapping' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TopicMapping ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TopicMapping_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TopicStore' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE TopicStore ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_TopicStore_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vector' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE Vector ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_Vector_CreatedOn DEFAULT(getdate())
	GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ZipTaxRate' AND COLUMN_NAME = 'CreatedOn')
	ALTER TABLE ZipTaxRate ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ZipTaxRate_CreatedOn DEFAULT(getdate())
	GO

--Add UpdatedOn triggers to all tables
--Add the UpdatedOn triggers
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Address_Updated'))
	DROP TRIGGER [dbo].[Address_Updated]
GO

CREATE TRIGGER [dbo].[Address_Updated]
	ON [Address]
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Address_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			[Address]
		SET
			[Address].UpdatedOn = current_timestamp
		FROM 
			[Address] [a]
			   INNER JOIN INSERTED i
			   ON [a].AddressID = i.AddressID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Affiliate_Updated'))
	DROP TRIGGER [dbo].[Affiliate_Updated]
GO

CREATE TRIGGER [dbo].[Affiliate_Updated]
	ON Affiliate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Affiliate_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Affiliate
		SET
			Affiliate.UpdatedOn = current_timestamp
		FROM 
			Affiliate [a]
			   INNER JOIN INSERTED i
			   ON [a].AffiliateID = i.AffiliateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AffiliateActivity_Updated'))
	DROP TRIGGER [dbo].[AffiliateActivity_Updated]
GO

CREATE TRIGGER [dbo].[AffiliateActivity_Updated]
	ON AffiliateActivity
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('AffiliateActivity_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			AffiliateActivity
		SET
			AffiliateActivity.UpdatedOn = current_timestamp
		FROM 
			AffiliateActivity [a]
			   INNER JOIN INSERTED i
			   ON [a].AffiliateActivityID = i.AffiliateActivityID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AffiliateActivityReason_Updated'))
	DROP TRIGGER [dbo].[AffiliateActivityReason_Updated]
GO

CREATE TRIGGER [dbo].[AffiliateActivityReason_Updated]
	ON AffiliateActivityReason
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('AffiliateActivityReason_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			AffiliateActivityReason
		SET
			AffiliateActivityReason.UpdatedOn = current_timestamp
		FROM 
			AffiliateActivityReason [a]
			   INNER JOIN INSERTED i
			   ON [a].AffiliateActivityReasonID = i.AffiliateActivityReasonID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AffiliateCommissions_Updated'))
	DROP TRIGGER [dbo].[AffiliateCommissions_Updated]
GO

CREATE TRIGGER [dbo].[AffiliateCommissions_Updated]
	ON AffiliateCommissions
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('AffiliateCommissions_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			AffiliateCommissions
		SET
			AffiliateCommissions.UpdatedOn = current_timestamp
		FROM 
			AffiliateCommissions [a]
			   INNER JOIN INSERTED i
			   ON [a].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AffiliateStore_Updated'))
	DROP TRIGGER [dbo].[AffiliateStore_Updated]
GO

CREATE TRIGGER [dbo].[AffiliateStore_Updated]
	ON AffiliateStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('AffiliateStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			AffiliateStore
		SET
			AffiliateStore.UpdatedOn = current_timestamp
		FROM 
			AffiliateStore [a]
			   INNER JOIN INSERTED i
			   ON [a].ID = i.ID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AppConfig_Updated'))
	DROP TRIGGER [dbo].[AppConfig_Updated]
GO

CREATE TRIGGER [dbo].[AppConfig_Updated]
	ON AppConfig
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('AppConfig_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			AppConfig
		SET
			AppConfig.UpdatedOn = current_timestamp
		FROM 
			AppConfig [a]
			   INNER JOIN INSERTED i
			   ON [a].AppConfigID = i.AppConfigID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'aspdnsf_Syslog_Updated'))
	DROP TRIGGER [dbo].[aspdnsf_Syslog_Updated]
GO

CREATE TRIGGER [dbo].[aspdnsf_Syslog_Updated]
	ON aspdnsf_Syslog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('aspdnsf_Syslog_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			aspdnsf_Syslog
		SET
			aspdnsf_Syslog.UpdatedOn = current_timestamp
		FROM 
			aspdnsf_Syslog [a]
			   INNER JOIN INSERTED i
			   ON [a].SyslogID = i.SyslogID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'AuditLog_Updated'))
	DROP TRIGGER [dbo].[AuditLog_Updated]
GO

CREATE TRIGGER [dbo].[AuditLog_Updated]
	ON AuditLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('AuditLog_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			AuditLog
		SET
			AuditLog.UpdatedOn = current_timestamp
		FROM 
			AuditLog [a]
			   INNER JOIN INSERTED i
			   ON [a].AuditLogID = i.AuditLogID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'BadWord_Updated'))
	DROP TRIGGER [dbo].[BadWord_Updated]
GO

CREATE TRIGGER [dbo].[BadWord_Updated]
	ON BadWord
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('BadWord_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			BadWord
		SET
			BadWord.UpdatedOn = current_timestamp
		FROM 
			BadWord [a]
			   INNER JOIN INSERTED i
			   ON [a].BadWordID = i.BadWordID
			   AND [a].LocaleSetting = i.LocaleSetting
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Category_Updated'))
	DROP TRIGGER [dbo].[Category_Updated]
GO

CREATE TRIGGER [dbo].[Category_Updated]
	ON Category
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Category_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Category
		SET
			Category.UpdatedOn = current_timestamp
		FROM 
			Category [c]
			   INNER JOIN INSERTED i
			   ON [c].CategoryID = i.CategoryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CIM_AddressPaymentProfileMap_Updated'))
	DROP TRIGGER [dbo].[CIM_AddressPaymentProfileMap_Updated]
GO

CREATE TRIGGER [dbo].[CIM_AddressPaymentProfileMap_Updated]
	ON CIM_AddressPaymentProfileMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CIM_AddressPaymentProfileMap_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CIM_AddressPaymentProfileMap
		SET
			CIM_AddressPaymentProfileMap.UpdatedOn = current_timestamp
		FROM 
			CIM_AddressPaymentProfileMap [c]
			   INNER JOIN INSERTED i
			   ON [c].CIMID = i.CIMID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ClickTrack_Updated'))
	DROP TRIGGER [dbo].[ClickTrack_Updated]
GO

CREATE TRIGGER [dbo].[ClickTrack_Updated]
	ON ClickTrack
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ClickTrack_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ClickTrack
		SET
			ClickTrack.UpdatedOn = current_timestamp
		FROM 
			ClickTrack [c]
			   INNER JOIN INSERTED i
			   ON [c].ClickTrackID = i.ClickTrackID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Country_Updated'))
	DROP TRIGGER [dbo].[Country_Updated]
GO

CREATE TRIGGER [dbo].[Country_Updated]
	ON Country
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Country_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Country
		SET
			Country.UpdatedOn = current_timestamp
		FROM 
			Country [c]
			   INNER JOIN INSERTED i
			   ON [c].CountryID = i.CountryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CountryTaxRate_Updated'))
	DROP TRIGGER [dbo].[CountryTaxRate_Updated]
GO

CREATE TRIGGER [dbo].[CountryTaxRate_Updated]
	ON CountryTaxRate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CountryTaxRate_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CountryTaxRate
		SET
			CountryTaxRate.UpdatedOn = current_timestamp
		FROM 
			CountryTaxRate [c]
			   INNER JOIN INSERTED i
			   ON [c].CountryTaxID = i.CountryTaxID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Coupon_Updated'))
	DROP TRIGGER [dbo].[Coupon_Updated]
GO

CREATE TRIGGER [dbo].[Coupon_Updated]
	ON Coupon
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Coupon_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Coupon
		SET
			Coupon.UpdatedOn = current_timestamp
		FROM 
			Coupon [c]
			   INNER JOIN INSERTED i
			   ON [c].CouponID = i.CouponID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CouponStore_Updated'))
	DROP TRIGGER [dbo].[CouponStore_Updated]
GO

CREATE TRIGGER [dbo].[CouponStore_Updated]
	ON CouponStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CouponStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CouponStore
		SET
			CouponStore.UpdatedOn = current_timestamp
		FROM 
			CouponStore [c]
			   INNER JOIN INSERTED i
			   ON [c].CouponID = i.CouponID
			   AND [c].StoreId = i.StoreId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CouponUsage_Updated'))
	DROP TRIGGER [dbo].[CouponUsage_Updated]
GO

CREATE TRIGGER [dbo].[CouponUsage_Updated]
	ON CouponUsage
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CouponUsage_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CouponUsage
		SET
			CouponUsage.UpdatedOn = current_timestamp
		FROM 
			CouponUsage [c]
			   INNER JOIN INSERTED i
			   ON [c].CouponUsageID = i.CouponUsageID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CreditCardType_Updated'))
	DROP TRIGGER [dbo].[CreditCardType_Updated]
GO

CREATE TRIGGER [dbo].[CreditCardType_Updated]
	ON CreditCardType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CreditCardType_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CreditCardType
		SET
			CreditCardType.UpdatedOn = current_timestamp
		FROM 
			CreditCardType [c]
			   INNER JOIN INSERTED i
			   ON [c].CardTypeID = i.CardTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Currency_Updated'))
	DROP TRIGGER [dbo].[Currency_Updated]
GO

CREATE TRIGGER [dbo].[Currency_Updated]
	ON Currency
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Currency_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Currency
		SET
			Currency.UpdatedOn = current_timestamp
		FROM 
			Currency [c]
			   INNER JOIN INSERTED i
			   ON [c].CurrencyID = i.CurrencyID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Customer_Updated'))
	DROP TRIGGER [dbo].[Customer_Updated]
GO

CREATE TRIGGER [dbo].[Customer_Updated]
	ON Customer
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Customer_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Customer
		SET
			Customer.UpdatedOn = current_timestamp
		FROM 
			Customer [c]
			   INNER JOIN INSERTED i
			   ON [c].CustomerID = i.CustomerID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CustomerGiftRegistrySearches_Updated'))
	DROP TRIGGER [dbo].[CustomerGiftRegistrySearches_Updated]
GO

CREATE TRIGGER [dbo].[CustomerGiftRegistrySearches_Updated]
	ON CustomerGiftRegistrySearches
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CustomerGiftRegistrySearches_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CustomerGiftRegistrySearches
		SET
			CustomerGiftRegistrySearches.UpdatedOn = current_timestamp
		FROM 
			CustomerGiftRegistrySearches [c]
			   INNER JOIN INSERTED i
			   ON [c].CustomerGiftRegistrySearchesID = i.CustomerGiftRegistrySearchesID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CustomerLevel_Updated'))
	DROP TRIGGER [dbo].[CustomerLevel_Updated]
GO

CREATE TRIGGER [dbo].[CustomerLevel_Updated]
	ON CustomerLevel
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CustomerLevel_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CustomerLevel
		SET
			CustomerLevel.UpdatedOn = current_timestamp
		FROM 
			CustomerLevel [cl]
			   INNER JOIN INSERTED i
			   ON [cl].CustomerLevelID = i.CustomerLevelID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CustomerSession_Updated'))
	DROP TRIGGER [dbo].[CustomerSession_Updated]
GO

CREATE TRIGGER [dbo].[CustomerSession_Updated]
	ON CustomerSession
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CustomerSession_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CustomerSession
		SET
			CustomerSession.UpdatedOn = current_timestamp
		FROM 
			CustomerSession [cl]
			   INNER JOIN INSERTED i
			   ON [cl].CustomerSessionID = i.CustomerSessionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'CustomReport_Updated'))
	DROP TRIGGER [dbo].[CustomReport_Updated]
GO

CREATE TRIGGER [dbo].[CustomReport_Updated]
	ON CustomReport
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('CustomReport_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			CustomReport
		SET
			CustomReport.UpdatedOn = current_timestamp
		FROM 
			CustomReport [cl]
			   INNER JOIN INSERTED i
			   ON [cl].CustomReportID = i.CustomReportID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Document_Updated'))
	DROP TRIGGER [dbo].[Document_Updated]
GO

CREATE TRIGGER [dbo].[Document_Updated]
	ON Document
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Document_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Document
		SET
			Document.UpdatedOn = current_timestamp
		FROM 
			Document [d]
			   INNER JOIN INSERTED i
			   ON [d].DocumentID = i.DocumentID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'DocumentAffiliate_Updated'))
	DROP TRIGGER [dbo].[DocumentAffiliate_Updated]
GO

CREATE TRIGGER [dbo].[DocumentAffiliate_Updated]
	ON DocumentAffiliate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('DocumentAffiliate_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			DocumentAffiliate
		SET
			DocumentAffiliate.UpdatedOn = current_timestamp
		FROM 
			DocumentAffiliate [d]
			   INNER JOIN INSERTED i
			   ON [d].DocumentID = i.DocumentID
			   AND [d].AffiliateID = i.AffiliateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'DocumentLibrary_Updated'))
	DROP TRIGGER [dbo].[DocumentLibrary_Updated]
GO

CREATE TRIGGER [dbo].[DocumentLibrary_Updated]
	ON DocumentLibrary
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('DocumentLibrary_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			DocumentLibrary
		SET
			DocumentLibrary.UpdatedOn = current_timestamp
		FROM 
			DocumentLibrary [d]
			   INNER JOIN INSERTED i
			   ON [d].DocumentID = i.DocumentID
			   AND [d].LibraryID = i.LibraryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'DocumentType_Updated'))
	DROP TRIGGER [dbo].[DocumentType_Updated]
GO

CREATE TRIGGER [dbo].[DocumentType_Updated]
	ON DocumentType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('DocumentType_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			DocumentType
		SET
			DocumentType.UpdatedOn = current_timestamp
		FROM 
			DocumentType [d]
			   INNER JOIN INSERTED i
			   ON [d].DocumentTypeID = i.DocumentTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'EntityStore_Updated'))
	DROP TRIGGER [dbo].[EntityStore_Updated]
GO

CREATE TRIGGER [dbo].[EntityStore_Updated]
	ON EntityStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('EntityStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			EntityStore
		SET
			EntityStore.UpdatedOn = current_timestamp
		FROM 
			EntityStore [es]
			   INNER JOIN INSERTED i
			   ON [es].EntityID = i.EntityID
			   AND [es].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ErrorLog_Updated'))
	DROP TRIGGER [dbo].[ErrorLog_Updated]
GO

CREATE TRIGGER [dbo].[ErrorLog_Updated]
	ON ErrorLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ErrorLog_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ErrorLog
		SET
			ErrorLog.UpdatedOn = current_timestamp
		FROM 
			ErrorLog [e]
			   INNER JOIN INSERTED i
			   ON [e].logid = i.logid
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ErrorMessage_Updated'))
	DROP TRIGGER [dbo].[ErrorMessage_Updated]
GO

CREATE TRIGGER [dbo].[ErrorMessage_Updated]
	ON ErrorMessage
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ErrorMessage_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ErrorMessage
		SET
			ErrorMessage.UpdatedOn = current_timestamp
		FROM 
			ErrorMessage [e]
			   INNER JOIN INSERTED i
			   ON [e].MessageId = i.MessageId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'EventHandler_Updated'))
	DROP TRIGGER [dbo].[EventHandler_Updated]
GO

CREATE TRIGGER [dbo].[EventHandler_Updated]
	ON EventHandler
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('EventHandler_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			EventHandler
		SET
			EventHandler.UpdatedOn = current_timestamp
		FROM 
			EventHandler [e]
			   INNER JOIN INSERTED i
			   ON [e].EventId = i.EventId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ExtendedPrice_Updated'))
	DROP TRIGGER [dbo].[ExtendedPrice_Updated]
GO

CREATE TRIGGER [dbo].[ExtendedPrice_Updated]
	ON ExtendedPrice
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ExtendedPrice_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ExtendedPrice
		SET
			ExtendedPrice.UpdatedOn = current_timestamp
		FROM 
			ExtendedPrice [e]
			   INNER JOIN INSERTED i
			   ON [e].ExtendedPriceId = i.ExtendedPriceId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'FailedTransaction_Updated'))
	DROP TRIGGER [dbo].[FailedTransaction_Updated]
GO

CREATE TRIGGER [dbo].[FailedTransaction_Updated]
	ON FailedTransaction
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('FailedTransaction_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			FailedTransaction
		SET
			FailedTransaction.UpdatedOn = current_timestamp
		FROM 
			FailedTransaction [f]
			   INNER JOIN INSERTED i
			   ON [f].DBRecNo = i.DBRecNo
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'FAQ_Updated'))
	DROP TRIGGER [dbo].[FAQ_Updated]
GO

CREATE TRIGGER [dbo].[FAQ_Updated]
	ON FAQ
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('FAQ_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			FAQ
		SET
			FAQ.UpdatedOn = current_timestamp
		FROM 
			FAQ [f]
			   INNER JOIN INSERTED i
			   ON [f].FAQID = i.FAQID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Feed_Updated'))
	DROP TRIGGER [dbo].[Feed_Updated]
GO

CREATE TRIGGER [dbo].[Feed_Updated]
	ON Feed
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Feed_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Feed
		SET
			Feed.UpdatedOn = current_timestamp
		FROM 
			Feed [f]
			   INNER JOIN INSERTED i
			   ON [f].FeedID = i.FeedID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Gallery_Updated'))
	DROP TRIGGER [dbo].[Gallery_Updated]
GO

CREATE TRIGGER [dbo].[Gallery_Updated]
	ON Gallery
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Gallery_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Gallery
		SET
			Gallery.UpdatedOn = current_timestamp
		FROM 
			Gallery [g]
			   INNER JOIN INSERTED i
			   ON [g].GalleryID = i.GalleryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Genre_Updated'))
	DROP TRIGGER [dbo].[Genre_Updated]
GO

CREATE TRIGGER [dbo].[Genre_Updated]
	ON Genre
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Genre_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Genre
		SET
			Genre.UpdatedOn = current_timestamp
		FROM 
			Genre [g]
			   INNER JOIN INSERTED i
			   ON [g].GenreID = i.GenreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'GiftCard_Updated'))
	DROP TRIGGER [dbo].[GiftCard_Updated]
GO

CREATE TRIGGER [dbo].[GiftCard_Updated]
	ON GiftCard
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('GiftCard_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			GiftCard
		SET
			GiftCard.UpdatedOn = current_timestamp
		FROM 
			GiftCard [g]
			   INNER JOIN INSERTED i
			   ON [g].GiftCardID = i.GiftCardID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'GiftCardStore_Updated'))
	DROP TRIGGER [dbo].[GiftCardStore_Updated]
GO

CREATE TRIGGER [dbo].[GiftCardStore_Updated]
	ON GiftCardStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('GiftCardStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			GiftCardStore
		SET
			GiftCardStore.UpdatedOn = current_timestamp
		FROM 
			GiftCardStore [g]
			   INNER JOIN INSERTED i
			   ON [g].GiftCardID = i.GiftCardID
			   AND [g].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'GiftCardUsage_Updated'))
	DROP TRIGGER [dbo].[GiftCardUsage_Updated]
GO

CREATE TRIGGER [dbo].[GiftCardUsage_Updated]
	ON GiftCardUsage
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('GiftCardUsage_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			GiftCardUsage
		SET
			GiftCardUsage.UpdatedOn = current_timestamp
		FROM 
			GiftCardUsage [g]
			   INNER JOIN INSERTED i
			   ON [g].GiftCardUsageID = i.GiftCardUsageID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'GlobalConfig_Updated'))
	DROP TRIGGER [dbo].[GlobalConfig_Updated]
GO

CREATE TRIGGER [dbo].[GlobalConfig_Updated]
	ON GlobalConfig
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('GlobalConfig_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			GlobalConfig
		SET
			GlobalConfig.UpdatedOn = current_timestamp
		FROM 
			GlobalConfig [g]
			   INNER JOIN INSERTED i
			   ON [g].GlobalConfigID = i.GlobalConfigID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Inventory_Updated'))
	DROP TRIGGER [dbo].[Inventory_Updated]
GO

CREATE TRIGGER [dbo].[Inventory_Updated]
	ON Inventory
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Inventory_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Inventory
		SET
			Inventory.UpdatedOn = current_timestamp
		FROM 
			Inventory [inv]
			   INNER JOIN INSERTED i
			   ON [inv].InventoryID = i.InventoryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'KitCart_Updated'))
	DROP TRIGGER [dbo].[KitCart_Updated]
GO

CREATE TRIGGER [dbo].[KitCart_Updated]
	ON KitCart
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('KitCart_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			KitCart
		SET
			KitCart.UpdatedOn = current_timestamp
		FROM 
			KitCart [k]
			   INNER JOIN INSERTED i
			   ON [k].KitCartRecID = i.KitCartRecID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'KitGroup_Updated'))
	DROP TRIGGER [dbo].[KitGroup_Updated]
GO

CREATE TRIGGER [dbo].[KitGroup_Updated]
	ON KitGroup
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('KitGroup_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			KitGroup
		SET
			KitGroup.UpdatedOn = current_timestamp
		FROM 
			KitGroup [k]
			   INNER JOIN INSERTED i
			   ON [k].KitGroupID = i.KitGroupID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'KitGroupType_Updated'))
	DROP TRIGGER [dbo].[KitGroupType_Updated]
GO

CREATE TRIGGER [dbo].[KitGroupType_Updated]
	ON KitGroupType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('KitGroupType_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			KitGroupType
		SET
			KitGroupType.UpdatedOn = current_timestamp
		FROM 
			KitGroupType [k]
			   INNER JOIN INSERTED i
			   ON [k].KitGroupTypeID = i.KitGroupTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'KitItem_Updated'))
	DROP TRIGGER [dbo].[KitItem_Updated]
GO

CREATE TRIGGER [dbo].[KitItem_Updated]
	ON KitItem
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('KitItem_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			KitItem
		SET
			KitItem.UpdatedOn = current_timestamp
		FROM 
			KitItem [k]
			   INNER JOIN INSERTED i
			   ON [k].KitItemID = i.KitItemID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Layout_Updated'))
	DROP TRIGGER [dbo].[Layout_Updated]
GO

CREATE TRIGGER [dbo].[Layout_Updated]
	ON Layout
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Layout_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Layout
		SET
			Layout.UpdatedOn = current_timestamp
		FROM 
			Layout [l]
			   INNER JOIN INSERTED i
			   ON [l].LayoutID = i.LayoutID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'LayoutField_Updated'))
	DROP TRIGGER [dbo].[LayoutField_Updated]
GO

CREATE TRIGGER [dbo].[LayoutField_Updated]
	ON LayoutField
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('LayoutField_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			LayoutField
		SET
			LayoutField.UpdatedOn = current_timestamp
		FROM 
			LayoutField [l]
			   INNER JOIN INSERTED i
			   ON [l].LayoutFieldID = i.LayoutFieldID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'LayoutFieldAttribute_Updated'))
	DROP TRIGGER [dbo].[LayoutFieldAttribute_Updated]
GO

CREATE TRIGGER [dbo].[LayoutFieldAttribute_Updated]
	ON LayoutFieldAttribute
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('LayoutFieldAttribute_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			LayoutFieldAttribute
		SET
			LayoutFieldAttribute.UpdatedOn = current_timestamp
		FROM 
			LayoutFieldAttribute [l]
			   INNER JOIN INSERTED i
			   ON [l].LayoutFieldAttributeID = i.LayoutFieldAttributeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'LayoutMap_Updated'))
	DROP TRIGGER [dbo].[LayoutMap_Updated]
GO

CREATE TRIGGER [dbo].[LayoutMap_Updated]
	ON LayoutMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('LayoutMap_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			LayoutMap
		SET
			LayoutMap.UpdatedOn = current_timestamp
		FROM 
			LayoutMap [l]
			   INNER JOIN INSERTED i
			   ON [l].LayoutMapID = i.LayoutMapID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Library_Updated'))
	DROP TRIGGER [dbo].[Library_Updated]
GO

CREATE TRIGGER [dbo].[Library_Updated]
	ON Library
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Library_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Library
		SET
			Library.UpdatedOn = current_timestamp
		FROM 
			Library [l]
			   INNER JOIN INSERTED i
			   ON [l].LibraryID = i.LibraryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'LocaleSetting_Updated'))
	DROP TRIGGER [dbo].[LocaleSetting_Updated]
GO

CREATE TRIGGER [dbo].[LocaleSetting_Updated]
	ON LocaleSetting
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('LocaleSetting_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			LocaleSetting
		SET
			LocaleSetting.UpdatedOn = current_timestamp
		FROM 
			LocaleSetting [l]
			   INNER JOIN INSERTED i
			   ON [l].LocaleSettingID = i.LocaleSettingID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'LOG_CustomerEvent_Updated'))
	DROP TRIGGER [dbo].[LOG_CustomerEvent_Updated]
GO

CREATE TRIGGER [dbo].[LOG_CustomerEvent_Updated]
	ON LOG_CustomerEvent
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('LOG_CustomerEvent_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			LOG_CustomerEvent
		SET
			LOG_CustomerEvent.UpdatedOn = current_timestamp
		FROM 
			LOG_CustomerEvent [l]
			   INNER JOIN INSERTED i
			   ON [l].DBRecNo = i.DBRecNo
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'LOG_Event_Updated'))
	DROP TRIGGER [dbo].[LOG_Event_Updated]
GO

CREATE TRIGGER [dbo].[LOG_Event_Updated]
	ON LOG_CustomerEvent
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('LOG_Event_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			LOG_Event
		SET
			LOG_Event.UpdatedOn = current_timestamp
		FROM 
			LOG_Event [l]
			   INNER JOIN INSERTED i
			   ON [l].EventID = i.EventID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'MailingMgrLog_Updated'))
	DROP TRIGGER [dbo].[MailingMgrLog_Updated]
GO

CREATE TRIGGER [dbo].[MailingMgrLog_Updated]
	ON MailingMgrLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('MailingMgrLog_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			MailingMgrLog
		SET
			MailingMgrLog.UpdatedOn = current_timestamp
		FROM 
			MailingMgrLog [m]
			   INNER JOIN INSERTED i
			   ON [m].MailingMgrLogID = i.MailingMgrLogID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Manufacturer_Updated'))
	DROP TRIGGER [dbo].[Manufacturer_Updated]
GO

CREATE TRIGGER [dbo].[Manufacturer_Updated]
	ON Manufacturer
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Manufacturer_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Manufacturer
		SET
			Manufacturer.UpdatedOn = current_timestamp
		FROM 
			Manufacturer [m]
			   INNER JOIN INSERTED i
			   ON [m].ManufacturerID = i.ManufacturerID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'MobileDevice_Updated'))
	DROP TRIGGER [dbo].[MobileDevice_Updated]
GO

CREATE TRIGGER [dbo].[MobileDevice_Updated]
	ON MobileDevice
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('MobileDevice_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			MobileDevice
		SET
			MobileDevice.UpdatedOn = current_timestamp
		FROM 
			MobileDevice [m]
			   INNER JOIN INSERTED i
			   ON [m].MobileDeviceID = i.MobileDeviceID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'MobileLocaleMapping_Updated'))
	DROP TRIGGER [dbo].[MobileLocaleMapping_Updated]
GO

CREATE TRIGGER [dbo].[MobileLocaleMapping_Updated]
	ON MobileLocaleMapping
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('MobileLocaleMapping_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			MobileLocaleMapping
		SET
			MobileLocaleMapping.UpdatedOn = current_timestamp
		FROM 
			MobileLocaleMapping [m]
			   INNER JOIN INSERTED i
			   ON [m].DesktopLocale = i.DesktopLocale
			   AND [m].MobileLocale = i.MobileLocale
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'MultiShipOrder_Shipment_Updated'))
	DROP TRIGGER [dbo].[MultiShipOrder_Shipment_Updated]
GO

CREATE TRIGGER [dbo].[MultiShipOrder_Shipment_Updated]
	ON MultiShipOrder_Shipment
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('MultiShipOrder_Shipment_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			MultiShipOrder_Shipment
		SET
			MultiShipOrder_Shipment.UpdatedOn = current_timestamp
		FROM 
			MultiShipOrder_Shipment [m]
			   INNER JOIN INSERTED i
			   ON [m].MultiShipOrder_ShipmentGUID = i.MultiShipOrder_ShipmentGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'News_Updated'))
	DROP TRIGGER [dbo].[News_Updated]
GO

CREATE TRIGGER [dbo].[News_Updated]
	ON News
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('News_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			News
		SET
			News.UpdatedOn = current_timestamp
		FROM 
			News [n]
			   INNER JOIN INSERTED i
			   ON [n].NewsID = i.NewsID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'NewsletterMailList_Updated'))
	DROP TRIGGER [dbo].[NewsletterMailList_Updated]
GO

CREATE TRIGGER [dbo].[NewsletterMailList_Updated]
	ON NewsletterMailList
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('NewsletterMailList_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			NewsletterMailList
		SET
			NewsletterMailList.UpdatedOn = current_timestamp
		FROM 
			NewsletterMailList [n]
			   INNER JOIN INSERTED i
			   ON [n].[GUID] = i.[GUID]
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'NewsStore_Updated'))
	DROP TRIGGER [dbo].[NewsStore_Updated]
GO

CREATE TRIGGER [dbo].[NewsStore_Updated]
	ON NewsStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('NewsStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			NewsStore
		SET
			NewsStore.UpdatedOn = current_timestamp
		FROM 
			NewsStore [n]
			   INNER JOIN INSERTED i
			   ON [n].NewsID = i.NewsID
			   AND [n].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderNumber_Updated'))
	DROP TRIGGER [dbo].[OrderNumber_Updated]
GO

CREATE TRIGGER [dbo].[OrderNumber_Updated]
	ON OrderNumbers
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('OrderNumber_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			OrderNumbers
		SET
			OrderNumbers.UpdatedOn = current_timestamp
		FROM 
			OrderNumbers [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderNumber = i.OrderNumber
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderOption_Updated'))
	DROP TRIGGER [dbo].[OrderOption_Updated]
GO

CREATE TRIGGER [dbo].[OrderOption_Updated]
	ON OrderOption
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('OrderOption_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			OrderOption
		SET
			OrderOption.UpdatedOn = current_timestamp
		FROM 
			OrderOption [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderOptionID = i.OrderOptionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderOptionStore_Updated'))
	DROP TRIGGER [dbo].[OrderOptionStore_Updated]
GO

CREATE TRIGGER [dbo].[OrderOptionStore_Updated]
	ON OrderOptionStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('OrderOptionStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			OrderOptionStore
		SET
			OrderOptionStore.UpdatedOn = current_timestamp
		FROM 
			OrderOptionStore [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderOptionID = i.OrderOptionID
			   AND [o].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Orders_Updated'))
	DROP TRIGGER [dbo].[Orders_Updated]
GO

CREATE TRIGGER [dbo].[Orders_Updated]
	ON Orders
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Orders_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Orders
		SET
			Orders.UpdatedOn = current_timestamp
		FROM 
			Orders [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderNumber = i.OrderNumber
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Orders_KitCart_Updated'))
	DROP TRIGGER [dbo].[Orders_KitCart_Updated]
GO

CREATE TRIGGER [dbo].[Orders_KitCart_Updated]
	ON Orders_KitCart
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Orders_KitCart_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Orders_KitCart
		SET
			Orders_KitCart.UpdatedOn = current_timestamp
		FROM 
			Orders_KitCart [o]
			   INNER JOIN INSERTED i
			   ON [o].KitCartRecID = i.KitCartRecID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Orders_ShoppingCart_Updated'))
	DROP TRIGGER [dbo].[Orders_ShoppingCart_Updated]
GO

CREATE TRIGGER [dbo].[Orders_ShoppingCart_Updated]
	ON Orders_ShoppingCart
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Orders_ShoppingCart_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Orders_ShoppingCart
		SET
			Orders_ShoppingCart.UpdatedOn = current_timestamp
		FROM 
			Orders_ShoppingCart [o]
			   INNER JOIN INSERTED i
			   ON [o].ShoppingCartRecID = i.ShoppingCartRecID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderShipment_Updated'))
	DROP TRIGGER [dbo].[OrderShipment_Updated]
GO

CREATE TRIGGER [dbo].[OrderShipment_Updated]
	ON OrderShipment
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('OrderShipment_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			OrderShipment
		SET
			OrderShipment.UpdatedOn = current_timestamp
		FROM 
			OrderShipment [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderShipmentID = i.OrderShipmentID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderTransaction_Updated'))
	DROP TRIGGER [dbo].[OrderTransaction_Updated]
GO

CREATE TRIGGER [dbo].[OrderTransaction_Updated]
	ON OrderTransaction
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('OrderTransaction_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			OrderTransaction
		SET
			OrderTransaction.UpdatedOn = current_timestamp
		FROM 
			OrderTransaction [o]
			   INNER JOIN INSERTED i
			   ON [o].OrderTransactionID = i.OrderTransactionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PageType_Updated'))
	DROP TRIGGER [dbo].[PageType_Updated]
GO

CREATE TRIGGER [dbo].[PageType_Updated]
	ON PageType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PageType_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PageType
		SET
			PageType.UpdatedOn = current_timestamp
		FROM 
			PageType [p]
			   INNER JOIN INSERTED i
			   ON [p].PageTypeID = i.PageTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Partner_Updated'))
	DROP TRIGGER [dbo].[Partner_Updated]
GO

CREATE TRIGGER [dbo].[Partner_Updated]
	ON [Partner]
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Partner_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			[Partner]
		SET
			[Partner].UpdatedOn = current_timestamp
		FROM 
			[Partner] [p]
			   INNER JOIN INSERTED i
			   ON [p].PartnerID = i.PartnerID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PasswordLog_Updated'))
	DROP TRIGGER [dbo].[PasswordLog_Updated]
GO

CREATE TRIGGER [dbo].[PasswordLog_Updated]
	ON PasswordLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PasswordLog_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PasswordLog
		SET
			PasswordLog.UpdatedOn = current_timestamp
		FROM 
			PasswordLog [p]
			   INNER JOIN INSERTED i
			   ON [p].CustomerID = i.CustomerID
			   AND [p].ChangeDt = i.ChangeDt
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Poll_Updated'))
	DROP TRIGGER [dbo].[Poll_Updated]
GO

CREATE TRIGGER [dbo].[Poll_Updated]
	ON Poll
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Poll_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Poll
		SET
			Poll.UpdatedOn = current_timestamp
		FROM 
			Poll [p]
			   INNER JOIN INSERTED i
			   ON [p].PollID = i.PollID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PollAnswer_Updated'))
	DROP TRIGGER [dbo].[PollAnswer_Updated]
GO

CREATE TRIGGER [dbo].[PollAnswer_Updated]
	ON PollAnswer
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PollAnswer_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PollAnswer
		SET
			PollAnswer.UpdatedOn = current_timestamp
		FROM 
			PollAnswer [p]
			   INNER JOIN INSERTED i
			   ON [p].PollAnswerID = i.PollAnswerID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PollCategory_Updated'))
	DROP TRIGGER [dbo].[PollCategory_Updated]
GO

CREATE TRIGGER [dbo].[PollCategory_Updated]
	ON PollCategory
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PollCategory_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PollCategory
		SET
			PollCategory.UpdatedOn = current_timestamp
		FROM 
			PollCategory [p]
			   INNER JOIN INSERTED i
			   ON [p].PollID = i.PollID
			   AND [p].CategoryID = i.CategoryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PollSection_Updated'))
	DROP TRIGGER [dbo].[PollSection_Updated]
GO

CREATE TRIGGER [dbo].[PollSection_Updated]
	ON PollSection
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PollSection_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PollSection
		SET
			PollSection.UpdatedOn = current_timestamp
		FROM 
			PollSection [p]
			   INNER JOIN INSERTED i
			   ON [p].PollID = i.PollID
			   AND [p].SectionID = i.SectionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PollSortOrder_Updated'))
	DROP TRIGGER [dbo].[PollSortOrder_Updated]
GO

CREATE TRIGGER [dbo].[PollSortOrder_Updated]
	ON PollSortOrder
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PollSortOrder_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PollSortOrder
		SET
			PollSortOrder.UpdatedOn = current_timestamp
		FROM 
			PollSortOrder [p]
			   INNER JOIN INSERTED i
			   ON [p].PollSortOrderID = i.PollSortOrderID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PollStore_Updated'))
	DROP TRIGGER [dbo].[PollStore_Updated]
GO

CREATE TRIGGER [dbo].[PollStore_Updated]
	ON PollStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PollStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PollStore
		SET
			PollStore.UpdatedOn = current_timestamp
		FROM 
			PollStore [p]
			   INNER JOIN INSERTED i
			   ON [p].PollID = i.PollID
			   AND [p].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PollVotingRecord_Updated'))
	DROP TRIGGER [dbo].[PollVotingRecord_Updated]
GO

CREATE TRIGGER [dbo].[PollVotingRecord_Updated]
	ON PollVotingRecord
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PollVotingRecord_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PollVotingRecord
		SET
			PollVotingRecord.UpdatedOn = current_timestamp
		FROM 
			PollVotingRecord [p]
			   INNER JOIN INSERTED i
			   ON [p].PollVotingRecordID = i.PollVotingRecordID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Product_Updated'))
	DROP TRIGGER [dbo].[Product_Updated]
GO

CREATE TRIGGER [dbo].[Product_Updated]
	ON Product
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Product_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Product
		SET
			Product.UpdatedOn = current_timestamp
		FROM 
			Product [p]
			   INNER JOIN INSERTED i
			   ON [p].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductAffiliate_Updated'))
	DROP TRIGGER [dbo].[ProductAffiliate_Updated]
GO

CREATE TRIGGER [dbo].[ProductAffiliate_Updated]
	ON ProductAffiliate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductAffiliate_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductAffiliate
		SET
			ProductAffiliate.UpdatedOn = current_timestamp
		FROM 
			ProductAffiliate [p]
			   INNER JOIN INSERTED i
			   ON [p].ProductID = i.ProductID
			   AND [p].AffiliateID = i.AffiliateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductVariant_Updated'))
	DROP TRIGGER [dbo].[ProductVariant_Updated]
GO

CREATE TRIGGER [dbo].[ProductVariant_Updated]
	ON ProductVariant
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductVariant_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductVariant
		SET
			ProductVariant.UpdatedOn = current_timestamp
		FROM 
			ProductVariant [pv]
			   INNER JOIN INSERTED i
			   ON [pv].VariantID = i.VariantID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductCategory_Updated'))
	DROP TRIGGER [dbo].[ProductCategory_Updated]
GO

CREATE TRIGGER [dbo].[ProductCategory_Updated]
	ON ProductCategory
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductCategory_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductCategory
		SET
			ProductCategory.UpdatedOn = current_timestamp
		FROM 
			ProductCategory [pc]
			   INNER JOIN INSERTED i
			   ON [pc].CategoryID = i.CategoryID
			   AND [pc].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductCustomerLevel_Updated'))
	DROP TRIGGER [dbo].[ProductCustomerLevel_Updated]
GO

CREATE TRIGGER [dbo].[ProductCustomerLevel_Updated]
	ON ProductCustomerLevel
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductCustomerLevel_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductCustomerLevel
		SET
			ProductCustomerLevel.UpdatedOn = current_timestamp
		FROM 
			ProductCustomerLevel [pcl]
			   INNER JOIN INSERTED i
			   ON [pcl].CustomerLevelID = i.CustomerLevelID
			   AND [pcl].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductDistributor_Updated'))
	DROP TRIGGER [dbo].[ProductDistributor_Updated]
GO

CREATE TRIGGER [dbo].[ProductDistributor_Updated]
	ON ProductDistributor
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductDistributor_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductDistributor
		SET
			ProductDistributor.UpdatedOn = current_timestamp
		FROM 
			ProductDistributor [pd]
			   INNER JOIN INSERTED i
			   ON [pd].DistributorID = i.DistributorID
			   AND [pd].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductGenre_Updated'))
	DROP TRIGGER [dbo].[ProductGenre_Updated]
GO

CREATE TRIGGER [dbo].[ProductGenre_Updated]
	ON ProductGenre
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductGenre_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductGenre
		SET
			ProductGenre.UpdatedOn = current_timestamp
		FROM 
			ProductGenre [pg]
			   INNER JOIN INSERTED i
			   ON [pg].GenreID = i.GenreID
			   AND [pg].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductLocaleSetting_Updated'))
	DROP TRIGGER [dbo].[ProductLocaleSetting_Updated]
GO

CREATE TRIGGER [dbo].[ProductLocaleSetting_Updated]
	ON ProductLocaleSetting
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductLocaleSetting_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductLocaleSetting
		SET
			ProductLocaleSetting.UpdatedOn = current_timestamp
		FROM 
			ProductLocaleSetting [pl]
			   INNER JOIN INSERTED i
			   ON [pl].LocaleSettingID = i.LocaleSettingID
			   AND [pl].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductManufacturer_Updated'))
	DROP TRIGGER [dbo].[ProductManufacturer_Updated]
GO

CREATE TRIGGER [dbo].[ProductManufacturer_Updated]
	ON ProductManufacturer
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductManufacturer_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductManufacturer
		SET
			ProductManufacturer.UpdatedOn = current_timestamp
		FROM 
			ProductManufacturer [pm]
			   INNER JOIN INSERTED i
			   ON [pm].ManufacturerID = i.ManufacturerID
			   AND [pm].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductSection_Updated'))
	DROP TRIGGER [dbo].[ProductSection_Updated]
GO

CREATE TRIGGER [dbo].[ProductSection_Updated]
	ON ProductSection
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductSection_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductSection
		SET
			ProductSection.UpdatedOn = current_timestamp
		FROM 
			ProductSection [ps]
			   INNER JOIN INSERTED i
			   ON [ps].SectionID = i.SectionID
			   AND [ps].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductStore_Updated'))
	DROP TRIGGER [dbo].[ProductStore_Updated]
GO

CREATE TRIGGER [dbo].[ProductStore_Updated]
	ON ProductStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductStore
		SET
			ProductStore.UpdatedOn = current_timestamp
		FROM 
			ProductStore [ps]
			   INNER JOIN INSERTED i
			   ON [ps].ID = i.ID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductType_Updated'))
	DROP TRIGGER [dbo].[ProductType_Updated]
GO

CREATE TRIGGER [dbo].[ProductType_Updated]
	ON ProductType
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductType_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductType
		SET
			ProductType.UpdatedOn = current_timestamp
		FROM 
			ProductType [p]
			   INNER JOIN INSERTED i
			   ON [p].ProductTypeID = i.ProductTypeID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductVariant_Updated'))
	DROP TRIGGER [dbo].[ProductVariant_Updated]
GO

CREATE TRIGGER [dbo].[ProductVariant_Updated]
	ON ProductVariant
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductVariant_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductVariant
		SET
			ProductVariant.UpdatedOn = current_timestamp
		FROM 
			ProductVariant [p]
			   INNER JOIN INSERTED i
			   ON [p].VariantID = i.VariantID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductVector_Updated'))
	DROP TRIGGER [dbo].[ProductVector_Updated]
GO

CREATE TRIGGER [dbo].[ProductVector_Updated]
	ON ProductVector
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductVector_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductVector
		SET
			ProductVector.UpdatedOn = current_timestamp
		FROM 
			ProductVector [pv]
			   INNER JOIN INSERTED i
			   ON [pv].VectorID = i.VectorID
			   AND [pv].ProductID = i.ProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ProductView_Updated'))
	DROP TRIGGER [dbo].[ProductView_Updated]
GO

CREATE TRIGGER [dbo].[ProductView_Updated]
	ON ProductView
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ProductView_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ProductView
		SET
			ProductView.UpdatedOn = current_timestamp
		FROM 
			ProductView [p]
			   INNER JOIN INSERTED i
			   ON [p].ViewID = i.ViewID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PromotionLineItem_Updated'))
	DROP TRIGGER [dbo].[PromotionLineItem_Updated]
GO

CREATE TRIGGER [dbo].[PromotionLineItem_Updated]
	ON PromotionLineItem
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PromotionLineItem_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PromotionLineItem
		SET
			PromotionLineItem.UpdatedOn = current_timestamp
		FROM 
			PromotionLineItem [p]
			   INNER JOIN INSERTED i
			   ON [p].id = i.id
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Promotions_Updated'))
	DROP TRIGGER [dbo].[Promotions_Updated]
GO

CREATE TRIGGER [dbo].[Promotions_Updated]
	ON Promotions
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Promotions_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Promotions
		SET
			Promotions.UpdatedOn = current_timestamp
		FROM 
			Promotions [p]
			   INNER JOIN INSERTED i
			   ON [p].Id = i.Id
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PromotionStore_Updated'))
	DROP TRIGGER [dbo].[PromotionStore_Updated]
GO

CREATE TRIGGER [dbo].[PromotionStore_Updated]
	ON PromotionStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PromotionStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PromotionStore
		SET
			PromotionStore.UpdatedOn = current_timestamp
		FROM 
			PromotionStore [p]
			   INNER JOIN INSERTED i
			   ON [p].ID = i.ID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'PromotionUsage_Updated'))
	DROP TRIGGER [dbo].[PromotionUsage_Updated]
GO

CREATE TRIGGER [dbo].[PromotionUsage_Updated]
	ON PromotionUsage
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('PromotionUsage_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			PromotionUsage
		SET
			PromotionUsage.UpdatedOn = current_timestamp
		FROM 
			PromotionUsage [p]
			   INNER JOIN INSERTED i
			   ON [p].Id = i.Id
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'QuantityDiscount_Updated'))
	DROP TRIGGER [dbo].[QuantityDiscount_Updated]
GO

CREATE TRIGGER [dbo].[QuantityDiscount_Updated]
	ON QuantityDiscount
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('QuantityDiscount_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			QuantityDiscount
		SET
			QuantityDiscount.UpdatedOn = current_timestamp
		FROM 
			QuantityDiscount [q]
			   INNER JOIN INSERTED i
			   ON [q].QuantityDiscountID = i.QuantityDiscountID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'QuantityDiscountTable_Updated'))
	DROP TRIGGER [dbo].[QuantityDiscountTable_Updated]
GO

CREATE TRIGGER [dbo].[QuantityDiscountTable_Updated]
	ON QuantityDiscountTable
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('QuantityDiscountTable_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			QuantityDiscountTable
		SET
			QuantityDiscountTable.UpdatedOn = current_timestamp
		FROM 
			QuantityDiscountTable [q]
			   INNER JOIN INSERTED i
			   ON [q].QuantityDiscountTableID = i.QuantityDiscountTableID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Rating_Updated'))
	DROP TRIGGER [dbo].[Rating_Updated]
GO

CREATE TRIGGER [dbo].[Rating_Updated]
	ON Rating
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Rating_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Rating
		SET
			Rating.UpdatedOn = current_timestamp
		FROM 
			Rating [r]
			   INNER JOIN INSERTED i
			   ON [r].RatingID = i.RatingID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'RatingCommentHelpfulness_Updated'))
	DROP TRIGGER [dbo].[RatingCommentHelpfulness_Updated]
GO

CREATE TRIGGER [dbo].[RatingCommentHelpfulness_Updated]
	ON RatingCommentHelpfulness
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('RatingCommentHelpfulness_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			RatingCommentHelpfulness
		SET
			RatingCommentHelpfulness.UpdatedOn = current_timestamp
		FROM 
			RatingCommentHelpfulness [r]
			   INNER JOIN INSERTED i
			   ON [r].StoreID = i.StoreID
			   AND [r].ProductID = i.ProductID
			   AND [r].RatingCustomerID = i.RatingCustomerID
			   AND [r].VotingCustomerID = i.VotingCustomerID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'RestrictedIP_Updated'))
	DROP TRIGGER [dbo].[RestrictedIP_Updated]
GO

CREATE TRIGGER [dbo].[RestrictedIP_Updated]
	ON RestrictedIP
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('RestrictedIP_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			RestrictedIP
		SET
			RestrictedIP.UpdatedOn = current_timestamp
		FROM 
			RestrictedIP [r]
			   INNER JOIN INSERTED i
			   ON [r].DBRecNo = i.DBRecNo
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'SalesPrompt_Updated'))
	DROP TRIGGER [dbo].[SalesPrompt_Updated]
GO

CREATE TRIGGER [dbo].[SalesPrompt_Updated]
	ON SalesPrompt
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('SalesPrompt_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			SalesPrompt
		SET
			SalesPrompt.UpdatedOn = current_timestamp
		FROM 
			SalesPrompt [s]
			   INNER JOIN INSERTED i
			   ON [s].SalesPromptID = i.SalesPromptID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'SearchLog_Updated'))
	DROP TRIGGER [dbo].[SearchLog_Updated]
GO

CREATE TRIGGER [dbo].[SearchLog_Updated]
	ON SearchLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('SearchLog_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			SearchLog
		SET
			SearchLog.UpdatedOn = current_timestamp
		FROM 
			SearchLog [s]
			   INNER JOIN INSERTED i
			   ON [s].SearchTerm = i.SearchTerm
			   AND [s].CustomerID = i.CustomerID
			   AND [s].CreatedOn = i.CreatedOn
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Section_Updated'))
	DROP TRIGGER [dbo].[Section_Updated]
GO

CREATE TRIGGER [dbo].[Section_Updated]
	ON Section
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Section_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Section
		SET
			Section.UpdatedOn = current_timestamp
		FROM 
			Section [s]
			   INNER JOIN INSERTED i
			   ON [s].SectionID = i.SectionID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'SecurityLog_Updated'))
	DROP TRIGGER [dbo].[SecurityLog_Updated]
GO

CREATE TRIGGER [dbo].[SecurityLog_Updated]
	ON SecurityLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('SecurityLog_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			SecurityLog
		SET
			SecurityLog.UpdatedOn = current_timestamp
		FROM 
			SecurityLog [s]
			   INNER JOIN INSERTED i
			   ON [s].logid = i.logid
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingByProduct_Updated'))
	DROP TRIGGER [dbo].[ShippingByProduct_Updated]
GO

CREATE TRIGGER [dbo].[ShippingByProduct_Updated]
	ON ShippingByProduct
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingByProduct_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingByProduct
		SET
			ShippingByProduct.UpdatedOn = current_timestamp
		FROM 
			ShippingByProduct [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingByProductID = i.ShippingByProductID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingByTotal_Updated'))
	DROP TRIGGER [dbo].[ShippingByTotal_Updated]
GO

CREATE TRIGGER [dbo].[ShippingByTotal_Updated]
	ON ShippingByTotal
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingByTotal_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingByTotal
		SET
			ShippingByTotal.UpdatedOn = current_timestamp
		FROM 
			ShippingByTotal [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingByTotalByPercent_Updated'))
	DROP TRIGGER [dbo].[ShippingByTotalByPercent_Updated]
GO

CREATE TRIGGER [dbo].[ShippingByTotalByPercent_Updated]
	ON ShippingByTotalByPercent
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingByTotalByPercent_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingByTotalByPercent
		SET
			ShippingByTotalByPercent.UpdatedOn = current_timestamp
		FROM 
			ShippingByTotalByPercent [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingByWeight_Updated'))
	DROP TRIGGER [dbo].[ShippingByWeight_Updated]
GO

CREATE TRIGGER [dbo].[ShippingByWeight_Updated]
	ON ShippingByWeight
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingByWeight_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingByWeight
		SET
			ShippingByWeight.UpdatedOn = current_timestamp
		FROM 
			ShippingByWeight [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingCalculation_Updated'))
	DROP TRIGGER [dbo].[ShippingCalculation_Updated]
GO

CREATE TRIGGER [dbo].[ShippingCalculation_Updated]
	ON ShippingCalculation
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingCalculation_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingCalculation
		SET
			ShippingCalculation.UpdatedOn = current_timestamp
		FROM 
			ShippingCalculation [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingCalculationID = i.ShippingCalculationID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingCalculationStore_Updated'))
	DROP TRIGGER [dbo].[ShippingCalculationStore_Updated]
GO

CREATE TRIGGER [dbo].[ShippingCalculationStore_Updated]
	ON ShippingCalculationStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingCalculationStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingCalculationStore
		SET
			ShippingCalculationStore.UpdatedOn = current_timestamp
		FROM 
			ShippingCalculationStore [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingCalculationID = i.ShippingCalculationID
			   AND [s].StoreId = i.StoreId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingImportExport_Updated'))
	DROP TRIGGER [dbo].[ShippingImportExport_Updated]
GO

CREATE TRIGGER [dbo].[ShippingImportExport_Updated]
	ON ShippingImportExport
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingImportExport_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingImportExport
		SET
			ShippingImportExport.UpdatedOn = current_timestamp
		FROM 
			ShippingImportExport [s]
			   INNER JOIN INSERTED i
			   ON [s].OrderNumber = i.OrderNumber
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethod_Updated'))
	DROP TRIGGER [dbo].[ShippingMethod_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethod_Updated]
	ON ShippingMethod
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingMethod_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingMethod
		SET
			ShippingMethod.UpdatedOn = current_timestamp
		FROM 
			ShippingMethod [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodStore_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodStore_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodStore_Updated]
	ON ShippingMethodStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingMethodStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingMethodStore
		SET
			ShippingMethodStore.UpdatedOn = current_timestamp
		FROM 
			ShippingMethodStore [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].StoreId = i.StoreId
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodToCountryMap_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodToCountryMap_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodToCountryMap_Updated]
	ON ShippingMethodToCountryMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingMethodToCountryMap_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingMethodToCountryMap
		SET
			ShippingMethodToCountryMap.UpdatedOn = current_timestamp
		FROM 
			ShippingMethodToCountryMap [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].CountryID = i.CountryID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodToStateMap_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodToStateMap_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodToStateMap_Updated]
	ON ShippingMethodToStateMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingMethodToStateMap_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingMethodToStateMap
		SET
			ShippingMethodToStateMap.UpdatedOn = current_timestamp
		FROM 
			ShippingMethodToStateMap [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].StateID = i.StateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodToZoneMap_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodToZoneMap_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodToZoneMap_Updated]
	ON ShippingMethodToZoneMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingMethodToZoneMap_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingMethodToZoneMap
		SET
			ShippingMethodToZoneMap.UpdatedOn = current_timestamp
		FROM 
			ShippingMethodToZoneMap [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].ShippingZoneID = i.ShippingZoneID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingMethodToZoneMap_Updated'))
	DROP TRIGGER [dbo].[ShippingMethodToZoneMap_Updated]
GO

CREATE TRIGGER [dbo].[ShippingMethodToZoneMap_Updated]
	ON ShippingMethodToZoneMap
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingMethodToZoneMap_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingMethodToZoneMap
		SET
			ShippingMethodToZoneMap.UpdatedOn = current_timestamp
		FROM 
			ShippingMethodToZoneMap [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingMethodID = i.ShippingMethodID
			   AND [s].ShippingZoneID = i.ShippingZoneID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingTotalByZone_Updated'))
	DROP TRIGGER [dbo].[ShippingTotalByZone_Updated]
GO

CREATE TRIGGER [dbo].[ShippingTotalByZone_Updated]
	ON ShippingTotalByZone
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingTotalByZone_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingTotalByZone
		SET
			ShippingTotalByZone.UpdatedOn = current_timestamp
		FROM 
			ShippingTotalByZone [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingWeightByZone_Updated'))
	DROP TRIGGER [dbo].[ShippingWeightByZone_Updated]
GO

CREATE TRIGGER [dbo].[ShippingWeightByZone_Updated]
	ON ShippingWeightByZone
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingWeightByZone_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingWeightByZone
		SET
			ShippingWeightByZone.UpdatedOn = current_timestamp
		FROM 
			ShippingWeightByZone [s]
			   INNER JOIN INSERTED i
			   ON [s].RowGUID = i.RowGUID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShippingZone_Updated'))
	DROP TRIGGER [dbo].[ShippingZone_Updated]
GO

CREATE TRIGGER [dbo].[ShippingZone_Updated]
	ON ShippingZone
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShippingZone_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShippingZone
		SET
			ShippingZone.UpdatedOn = current_timestamp
		FROM 
			ShippingZone [s]
			   INNER JOIN INSERTED i
			   ON [s].ShippingZoneID = i.ShippingZoneID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ShoppingCart_Updated'))
	DROP TRIGGER [dbo].[ShoppingCart_Updated]
GO

CREATE TRIGGER [dbo].[ShoppingCart_Updated]
	ON ShoppingCart
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ShoppingCart_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ShoppingCart
		SET
			ShoppingCart.UpdatedOn = current_timestamp
		FROM 
			ShoppingCart [s]
			   INNER JOIN INSERTED i
			   ON [s].ShoppingCartRecID = i.ShoppingCartRecID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'SkinPreview_Updated'))
	DROP TRIGGER [dbo].[SkinPreview_Updated]
GO

CREATE TRIGGER [dbo].[SkinPreview_Updated]
	ON SkinPreview
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('SkinPreview_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			SkinPreview
		SET
			SkinPreview.UpdatedOn = current_timestamp
		FROM 
			SkinPreview [s]
			   INNER JOIN INSERTED i
			   ON [s].SkinPreviewID = i.SkinPreviewID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'SQLLog_Updated'))
	DROP TRIGGER [dbo].[SQLLog_Updated]
GO

CREATE TRIGGER [dbo].[SQLLog_Updated]
	ON SQLLog
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('SQLLog_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			SQLLog
		SET
			SQLLog.UpdatedOn = current_timestamp
		FROM 
			SQLLog [s]
			   INNER JOIN INSERTED i
			   ON [s].SQLLogID = i.SQLLogID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Staff_Updated'))
	DROP TRIGGER [dbo].[Staff_Updated]
GO

CREATE TRIGGER [dbo].[Staff_Updated]
	ON Staff
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Staff_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Staff
		SET
			Staff.UpdatedOn = current_timestamp
		FROM 
			Staff [s]
			   INNER JOIN INSERTED i
			   ON [s].StaffID = i.StaffID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'State_Updated'))
	DROP TRIGGER [dbo].[State_Updated]
GO

CREATE TRIGGER [dbo].[State_Updated]
	ON [State]
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('State_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			[State]
		SET
			[State].UpdatedOn = current_timestamp
		FROM 
			[State] [s]
			   INNER JOIN INSERTED i
			   ON [s].StateID = i.StateID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'StateTaxRate_Updated'))
	DROP TRIGGER [dbo].[StateTaxRate_Updated]
GO

CREATE TRIGGER [dbo].[StateTaxRate_Updated]
	ON StateTaxRate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('StateTaxRate_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			StateTaxRate
		SET
			StateTaxRate.UpdatedOn = current_timestamp
		FROM 
			StateTaxRate [s]
			   INNER JOIN INSERTED i
			   ON [s].StateTaxID = i.StateTaxID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'StateTaxRate_Updated'))
	DROP TRIGGER [dbo].[StateTaxRate_Updated]
GO

CREATE TRIGGER [dbo].[StateTaxRate_Updated]
	ON StateTaxRate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('StateTaxRate_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			StateTaxRate
		SET
			StateTaxRate.UpdatedOn = current_timestamp
		FROM 
			StateTaxRate [s]
			   INNER JOIN INSERTED i
			   ON [s].StateTaxID = i.StateTaxID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Store_Updated'))
	DROP TRIGGER [dbo].[Store_Updated]
GO

CREATE TRIGGER [dbo].[Store_Updated]
	ON Store
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Store_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Store
		SET
			Store.UpdatedOn = current_timestamp
		FROM 
			Store [s]
			   INNER JOIN INSERTED i
			   ON [s].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'StringResource_Updated'))
	DROP TRIGGER [dbo].[StringResource_Updated]
GO

CREATE TRIGGER [dbo].[StringResource_Updated]
	ON StringResource
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('StringResource_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			StringResource
		SET
			StringResource.UpdatedOn = current_timestamp
		FROM 
			StringResource [s]
			   INNER JOIN INSERTED i
			   ON [s].StringResourceID = i.StringResourceID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'TaxClass_Updated'))
	DROP TRIGGER [dbo].[TaxClass_Updated]
GO

CREATE TRIGGER [dbo].[TaxClass_Updated]
	ON TaxClass
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('TaxClass_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			TaxClass
		SET
			TaxClass.UpdatedOn = current_timestamp
		FROM 
			TaxClass [s]
			   INNER JOIN INSERTED i
			   ON [s].TaxClassID = i.TaxClassID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Topic_Updated'))
	DROP TRIGGER [dbo].[Topic_Updated]
GO

CREATE TRIGGER [dbo].[Topic_Updated]
	ON Topic
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Topic_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Topic
		SET
			Topic.UpdatedOn = current_timestamp
		FROM 
			Topic [s]
			   INNER JOIN INSERTED i
			   ON [s].TopicID = i.TopicID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'TopicMapping_Updated'))
	DROP TRIGGER [dbo].[TopicMapping_Updated]
GO

CREATE TRIGGER [dbo].[TopicMapping_Updated]
	ON TopicMapping
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('TopicMapping_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			TopicMapping
		SET
			TopicMapping.UpdatedOn = current_timestamp
		FROM 
			TopicMapping [s]
			   INNER JOIN INSERTED i
			   ON [s].TopicID = i.TopicID
			   AND [s].ParentTopicID = i.ParentTopicID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'TopicStore_Updated'))
	DROP TRIGGER [dbo].[TopicStore_Updated]
GO

CREATE TRIGGER [dbo].[TopicStore_Updated]
	ON TopicStore
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('TopicStore_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			TopicStore
		SET
			TopicStore.UpdatedOn = current_timestamp
		FROM 
			TopicStore [s]
			   INNER JOIN INSERTED i
			   ON [s].TopicID = i.TopicID
			   AND [s].StoreID = i.StoreID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'Vector_Updated'))
	DROP TRIGGER [dbo].[Vector_Updated]
GO

CREATE TRIGGER [dbo].[Vector_Updated]
	ON Vector
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('Vector_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			Vector
		SET
			Vector.UpdatedOn = current_timestamp
		FROM 
			Vector [v]
			   INNER JOIN INSERTED i
			   ON [v].VectorID = i.VectorID
	END
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'ZipTaxRate_Updated'))
	DROP TRIGGER [dbo].[ZipTaxRate_Updated]
GO

CREATE TRIGGER [dbo].[ZipTaxRate_Updated]
	ON ZipTaxRate
	FOR UPDATE, INSERT
	AS
	BEGIN
		IF @@rowcount = 0 RETURN;
	    
		IF trigger_nestlevel(object_ID('ZipTaxRate_Updated')) > 1 RETURN;
	    
		SET NOCOUNT ON
	    
		UPDATE
			ZipTaxRate
		SET
			ZipTaxRate.UpdatedOn = current_timestamp
		FROM 
			ZipTaxRate [v]
			   INNER JOIN INSERTED i
			   ON [v].ZipTaxID = i.ZipTaxID
	END
GO
	

/*********************End UpdatedOn and CreatedOn Columns & DB Triggers************************/

UPDATE AppConfig SET Description = 'DEPRECATED - Support for this AppConfig has been removed.' WHERE Name = 'ApplyShippingHandlingExtraFeeToFreeShipping'

-- Update store version
PRINT 'Updating Store Version...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = '9.3.1.0' WHERE [Name] = 'StoreVersion'

/********************* Update PayPal Sandbox URL's **************************/
update AppConfig set ConfigValue = 'https://api-3t.sandbox.paypal.com/2.0/' where Name = 'PayPal.API.TestURL' and ConfigValue = 'https://api.sandbox.paypal.com/2.0/'
update AppConfig set Description = 'DEPRECATED - Support for this AppConfig has been removed.' where Name = 'PayPal.API.LiveAAURL'
update AppConfig set Description = 'DEPRECATED - Support for this AppConfig has been removed.' where Name = 'PayPal.API.TestAAURL'


/*********** Begin 9.4.0.0 Changes ***********************/

GO
--TFS 1311
ALTER proc [dbo].[aspdnsf_getOrder]
    @ordernumber int
  
AS
SET NOCOUNT ON 
SELECT 
    o.OrderNumber,
    o.OrderGUID,
    o.ParentOrderNumber,
    o.StoreVersion,
    o.QuoteCheckout,
    o.IsNew,
    o.ShippedOn,
    o.CustomerID,
    o.CustomerGUID,
    o.Referrer,
    o.SkinID,
    o.LastName,
    o.FirstName,
    o.Email,
    o.Notes,
    o.BillingEqualsShipping,
    o.BillingLastName,
    o.BillingFirstName,
    o.BillingCompany,
    o.BillingAddress1,
    o.BillingAddress2,
    o.BillingSuite,
    o.BillingCity,
    o.BillingState,
    o.BillingZip,
    o.BillingCountry,
    o.BillingPhone,
    o.ShippingLastName,
    o.ShippingFirstName,
    o.ShippingCompany,
    o.ShippingResidenceType,
    o.ShippingAddress1,
    o.ShippingAddress2,
    o.ShippingSuite,
    o.ShippingCity,
    o.ShippingState,
    o.ShippingZip,
    o.ShippingCountry,
    o.ShippingMethodID,
    o.ShippingMethod,
    o.ShippingPhone,
    o.ShippingCalculationID,
    o.Phone,
    o.RegisterDate,
    o.AffiliateID,
    o.CouponCode,
    o.CouponType,
    o.CouponDescription,
    o.CouponDiscountAmount,
    o.CouponDiscountPercent,
    o.CouponIncludesFreeShipping,
    o.OkToEmail,
    o.Deleted,
    o.CardType,
    o.CardName,
    o.CardNumber,
    o.CardExpirationMonth,
    o.CardExpirationYear,
    o.OrderSubtotal,
    o.OrderTax,
    o.OrderShippingCosts,
    o.OrderTotal,
    o.PaymentGateway,
    o.AuthorizationCode,
    o.AuthorizationResult,
    o.AuthorizationPNREF,
    o.TransactionCommand,
    o.OrderDate,
    o.LevelID,
    o.LevelName,
    o.LevelDiscountPercent,
    o.LevelDiscountAmount,
    o.LevelHasFreeShipping,
    o.LevelAllowsQuantityDiscounts,
    o.LevelHasNoTax,
    o.LevelAllowsCoupons,
    o.LevelDiscountsApplyToExtendedPrices,
    o.LastIPAddress,
    o.PaymentMethod,
    o.OrderNotes,
    o.PONumber,
    o.DownloadEmailSentOn,
    o.ReceiptEmailSentOn,
    o.DistributorEmailSentOn,
    o.ShippingTrackingNumber,
    o.ShippedVIA,
    o.CustomerServiceNotes,
    o.RTShipRequest,
    o.RTShipResponse,
    o.TransactionState,
    o.AVSResult,
    o.CaptureTXCommand,
    o.CaptureTXResult,
    o.VoidTXCommand,
    o.VoidTXResult,
    o.RefundTXCommand,
    o.RefundTXResult,
    o.CardinalLookupResult,
    o.CardinalAuthenticateResult,
    o.CardinalGatewayParms,
    o.AffiliateCommissionRecorded,
    o.OrderOptions,
    o.OrderWeight,
    o.eCheckBankABACode,
    o.eCheckBankAccountNumber,
    o.eCheckBankAccountType,
    o.eCheckBankName,
    o.eCheckBankAccountName,
    o.CarrierReportedRate,
    o.CarrierReportedWeight,
    o.LocaleSetting,
    o.FinalizationData,
    o.ExtensionData,
    o.AlreadyConfirmed,
    o.CartType,
    o.THUB_POSTED_TO_ACCOUNTING,
    o.THUB_POSTED_DATE,
    o.THUB_ACCOUNTING_REF,
    o.Last4,
    o.ReadyToShip,
    o.IsPrinted,
    o.AuthorizedOn,
    o.CapturedOn,
    o.RefundedOn,
    o.VoidedOn,
    o.EditedOn,
    o.InventoryWasReduced,
    o.MaxMindFraudScore,
    o.MaxMindDetails,
    o.CardStartDate,
    o.CardIssueNumber,
    o.TransactionType,
    o.Crypt,
    o.VATRegistrationID,
    o.FraudedOn,
    o.RefundReason,
    o.AuthorizationPNREF as TransactionID,
    o.RecurringSubscriptionID,
    o.RelatedOrderNumber,
    o.ReceiptHtml,

    os.SubscriptionInterval,
    os.SubscriptionIntervalType,
    os.ShoppingCartRecID,
    os.IsTaxable,
    os.IsShipSeparately,
    os.IsDownload,
    os.DownloadLocation,
    os.FreeShipping,
    os.DistributorID,
    os.ShippingDetail,
    os.TaxClassID,
    os.TaxRate,
    os.Notes,
    os.CustomerEntersPrice,
    os.ProductID,
    os.VariantID,
    os.Quantity,
    os.ChosenColor,
    os.ChosenColorSKUModifier,
    os.ChosenSize,
    os.ChosenSizeSKUModifier,
    os.TextOption,
    os.SizeOptionPrompt,
    os.ColorOptionPrompt,
    os.TextOptionPrompt,
    os.CustomerEntersPricePrompt,
    os.OrderedProductQuantityDiscountID,
    os.OrderedProductQuantityDiscountName,
    os.OrderedProductQuantityDiscountPercent,
    os.OrderedProductName,
    os.OrderedProductVariantName,
    os.OrderedProductSKU,
    os.OrderedProductManufacturerPartNumber ,
    os.OrderedProductPrice,
    os.OrderedProductWeight,
    os.OrderedProductPrice,
    os.ShippingMethodID,
    os.ShippingMethodID CartItemShippingMethodID,
    os.ShippingMethod CartItemShippingMethod,
    os.GiftRegistryForCustomerID,
    os.ShippingAddressID,
    os.IsAKit,
    os.IsAPack
FROM Orders o with (nolock) 
    left join orders_ShoppingCart os with (nolock) ON os.OrderNumber = o.OrderNumber 
WHERE o.OrderNumber = @ordernumber 
ORDER BY os.GiftRegistryForCustomerID,os.ShippingAddressID

GO

--insert topics for skin
if not exists(select name from topic where name = 'Template.Logo')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.Logo', 
	'Template.Logo', 
	1,
	0,
	'
	<a id="logo" class="logo" href="default.aspx" title="YourCompany.com" >
        <img src="App_Themes/Skin_(!SKINID!)/images/logo.gif" alt="YourCompany.com"/>
    </a>
	'
);

if not exists(select name from topic where name = 'Template.Header')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.Header', 
	'Template.Header', 
	1,
	0,
	'
	(!Topic Name="template.logo"!)
	<div class="user-links" id="user-links">
		(!USERNAME!)
		<a href="(!SIGNINOUT_LINK!)">(!SIGNINOUT_TEXT!)</a>
		<a href="account.aspx" class="account">Your Account</a>
		<a href="shoppingcart.aspx" class="cart">Shopping Cart ((!NUM_CART_ITEMS!))</a>
	</div>
	<div class="phone">1.800.555.1234</div>
	'
);

if not exists(select name from topic where name = 'Template.TopNavigation')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.TopNavigation', 
	'Template.TopNavigation', 
	1,
	0,
	'
	<ul class="top-nav-list">
	    <li><a href="default.aspx">Home</a></li>
	    <li><a href="t-about.aspx">About Us</a></li>
	    <li><a href="t-service.aspx">Customer Service</a></li>
	    <li><a href="t-faq.aspx">FAQs</a></li>
	    <li><a href="t-contact.aspx">Contact Us</a></li>
    </ul>
	'
);

if not exists(select name from topic where name = 'Template.VerticalNavigation')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.VerticalNavigation', 
	'Template.VerticalNavigation', 
	1,
	0,
	'
	<div class="nav-wrapper">
    	<h6 class="bar">
    		(!stringresource name="appconfig.categorypromptplural"!) 
    	</h6>
    	(!xmlpackage name="entitymenu.xml.config" entitytype="category" !)
    </div>
    <div class="nav-wrapper">
    	<h6 class="bar">
    		(!stringresource name="appconfig.sectionpromptplural"!) 
    	</h6>
    	(!xmlpackage name="entitymenu.xml.config" entitytype="section" !)
    </div>
    <div class="nav-wrapper">
    	<h6 class="bar">
    		(!stringresource name="appconfig.manufacturerpromptplural"!) 
    	</h6>
    	(!xmlpackage name="entitymenu.xml.config" entitytype="manufacturer" !)
    </div>
	'
);
if not exists(select name from topic where name = 'Template.Footer')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.Footer', 
	'Template.Footer', 
	1,
	0,
	'
	<ul class="footer-list">
	    <li><a href="default.aspx">Home</a></li>
	    <li><a href="t-about.aspx">About Us</a></li>
	    <li><a href="t-contact.aspx">Contact Us</a></li>
	    <li><a href="sitemap2.aspx">Site Map</a></li>
	    <li><a href="t-service.aspx">Customer Service</a></li>
	    <li><a href="wishlist.aspx">Wishlist</a></li>
	    <li><a href="t-security.aspx">Security</a></li>
	    <li><a href="t-privacy.aspx">Privacy Policy</a></li>
	    <li>(!XmlPackage Name="mobilelink"!)</li>
    </ul>
	'
);

if not exists(select name from topic where name = 'Template.SubFooter')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Template.SubFooter', 
	'Template.SubFooter', 
	1,
	0,
	'
	&copy; YourCompany.com 2013. All Rights Reserved. Powered by <a href="http://www.aspdotnetstorefront.com" target="_blank">AspDotNetStorefront</a>
	'
);

/* 9.4 added for Download Products */
if not exists (select * from AppConfig where Name = 'Download.ShowRelatedProducts')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'Download.ShowRelatedProducts','DOWNLOAD','If True, the product downloads page will display related items.','true', 'boolean');

if not exists (select * from AppConfig where Name = 'Download.ReleaseOnAction')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType, AllowableValues) values(0,'Download.ReleaseOnAction','DOWNLOAD','Valid configurations are (MANUAL, CAPTURE, AUTO).  Manual will require admin to release on the order page. CAPTURE will release on payment capture status.  AUTO will release the download without any requirements.','Manual', 'enum', 'Manual,Capture,Auto');

if not exists (select * from AppConfig where Name = 'Download.StreamFile')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'Download.StreamFile','DOWNLOAD','If true (recommended), the file will be streamed and delivered on a button click instead of providing a URL to the file location.','true', 'boolean');

if not exists (select * from AppConfig where Name = 'Download.CopyFileForEachOrder')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'Download.CopyFileForEachOrder','DOWNLOAD','If true (not recommended), the software will create a separate copy of each file that is purchased.  This configuration is ignored if you are using files on another server for your downloads.','false', 'boolean');

if not exists (select * from AppConfig where Name = 'Download.AllowMobileAccess')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'Download.AllowMobileAccess','DOWNLOAD','If true, authenticated customers on mobile devices will be able to access the downloads page.','true', 'boolean');

if not exists (select * from Topic where Name = 'Download.Information')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Download.Information', 1, 0, 'Download.Information', 'Note to <a href="http://www.aspdotnetstorefront.com/">AspDotNetStorefront</a> Administrators:<br/><br/>You can edit this placeholder text by editing the "Download.Information" topic within the Admin Console.')

if not exists (select * from Topic where Name = 'Download.EmailHeader')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Download.EmailHeader', 1, 0, 'Download.EmailHeader', '')

if not exists (select * from Topic where Name = 'Download.EmailFooter')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Download.EmailFooter', 1, 0, 'Download.EmailFooter', '')

if not exists (select * from Topic where Name = 'Download.MobilePageContent')
	INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap,Title, Description) values('Download.MobilePageContent', 1, 0, 'Download.MobilePageContent', 'Please visit the Downloads page on a non-mobile device to access your downloads.')

if exists (select * from Topic where Name = 'DownloadFooter' and datalength(Description) = 0)
	delete from Topic where name like 'DownloadFooter'

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' and COLUMN_NAME = 'DownloadStatus')
	ALTER TABLE Orders_ShoppingCart ADD DownloadStatus INT 
	GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' and COLUMN_NAME = 'DownloadValidDays')
	ALTER TABLE Orders_ShoppingCart ADD DownloadValidDays INT 
	GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' and COLUMN_NAME = 'DownloadCategory')
	ALTER TABLE Orders_ShoppingCart ADD DownloadCategory NTEXT 
	GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders_ShoppingCart' and COLUMN_NAME = 'DownloadReleasedOn')
	ALTER TABLE Orders_ShoppingCart ADD DownloadReleasedOn DATETIME 
	GO
	
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariant' and COLUMN_NAME = 'DownloadValidDays')
	ALTER TABLE ProductVariant ADD DownloadValidDays INT 
	GO

-- Add AppConfig for the new shoppingcart variant selector
If Not Exists(Select * From dbo.AppConfig Where name = 'AllowRecurringFrequencyChangeInCart') 
	Insert Into AppConfig (Name, [Description], GroupName, ConfigValue, ValueType, AllowableValues)  
	Values ('AllowRecurringFrequencyChangeInCart' ,'If true, customers will be able to change between recurring variants on the shopping cart page.','CHECKOUT','true','boolean', null);

-- Make gateway recurring billing on by default
PRINT 'Updating Recurring.UseGatewayInternalBilling...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = 'true' WHERE [Name] = 'Recurring.UseGatewayInternalBilling'

-- Add the new recurring informational topic
IF EXISTS (SELECT * FROM Topic WHERE Name='recurringpayments')
	BEGIN
		PRINT 'Recurringpayments topic exists already'
	END
ELSE
	BEGIN
		PRINT 'Adding recurringpayments topic'
		INSERT [dbo].Topic(Name, HTMLOK, ShowInSiteMap, Published, Title, Description) values('recurringpayments', 1, 0, 1, 'recurringpayments', 'Add the information about your recurring products that you want to appear on this page by editing the "recurringpayments" topic within the Admin Console.')
	END

/* 9.4 Convert CardNumber to NVARCHAR(300) for [aspdnsf_PABPEraseCCInfo] performance */
ALTER TABLE Orders ALTER COLUMN CardNumber NVARCHAR(300) null
UPDATE Orders SET CardNumber=CardNumber

ALTER TABLE [Address] ALTER COLUMN CardNumber NVARCHAR(300) null
UPDATE [Address] SET CardNumber=CardNumber

/* 9.4 Remove unused AppConfigs */
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ModelFactoryAssembly'
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ModelFactoryType'
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ConfigurationFactoryAssembly'
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ConfigurationFactoryType'
DELETE FROM AppConfig WHERE Name = 'Vortx.OnePageCheckout.ZipCodeService.Yahoo.LicenseId'	

/* 9.4 Create AppConfigs for SmartOPC changes */
if not exists(select * from AppConfig Where Name='Vortx.OnePageCheckout.DefaultShippingMethodId')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'Vortx.OnePageCheckout.DefaultShippingMethodId', 'If defined, the Smart One Page Checkout will automatically select this shipping method after the customer enters their shipping address information.  Set to blank to disable.', '', 'string', null, 'CHECKOUT', 0, 0, getdate());

if not exists(select * from AppConfig Where Name='Vortx.OnePageCheckout.AllowAlternativePaymentBillingAddressEdit')
	INSERT INTO AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	VALUES(newid(), 0, 'Vortx.OnePageCheckout.AllowAlternativePaymentBillingAddressEdit', 'If true, other payment methods besides credit card payment will allow collection of separate billing address on smart one page checkout.', 'false', 'boolean', null, 'CHECKOUT', 0, 0, getdate());

-- PayPal Integrated Express Checkout

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.IntegratedCheckout.LiveURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Express.IntegratedCheckout.LiveURL','GATEWAY','PayPal Express Integrated Checkout Live Site URL. Do not change unless you know what you are doing.','https://www.paypal.com/webapps/xo/webflow/sparta/xoflow');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.IntegratedCheckout.SandboxURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Express.IntegratedCheckout.SandboxURL','GATEWAY','PayPal Express Integrated Checkout Sandbox Site URL. Do not change unless you know what you are doing.','https://www.sandbox.paypal.com/webapps/xo/webflow/sparta/xoflow');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.UseIntegratedCheckout') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Express.UseIntegratedCheckout','GATEWAY','Use the PayPal Express integrated checkout.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Express.UseIntegratedCheckout'
END
GO

-- PayPal Bill Me Later Button
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.BillMeLaterButtonURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Hidden,Name,GroupName,Description,ConfigValue) values(1,1,'PayPal.Express.BillMeLaterButtonURL','GATEWAY','URL for Bill Me Later Button.','//www.paypalobjects.com/webstatic/en_US/btn/btn_bml_SM.png');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.ShowBillMeLaterButton') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'PayPal.Express.ShowBillMeLaterButton','GATEWAY','Show the Bill Me Later button on the shoppingcart page.','true', 'boolean');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.BillMeLaterMarketingMessage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Hidden,Name,GroupName,Description,ConfigValue) values(1,1,'PayPal.Express.BillMeLaterMarketingMessage','GATEWAY','Link & URL for Bill Me later Marketing Message to display beneath the Bill Me Later Button.','<a href="//www.securecheckout.billmelater.com/paycapture-content/fetch?hash=AU826TU8&content=/bmlweb/ppwpsiw.html" target="_blank"><img src="//www.paypalobjects.com/webstatic/en_US/btn/btn_bml_text.png" alt="Get 6 months to pay on $99+"/></a>');
END
GO


-- PayPal Banner Ads (Bill Me Later)
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.BannerEnrollmentService.SandboxURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.BannerEnrollmentService.SandboxURL','GATEWAY','Sandbox URL for Banner Enrollment Service.','https://api.financing-mint.paypal.com/finapi/v1/publishers/');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.BannerEnrollmentService.LiveURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.BannerEnrollmentService.LiveURL','GATEWAY','Live URL for Banner Enrollment Service.','https://api.financing.paypal.com/finapi/v1/publishers/');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.TermsAndConditionsAgreement') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.TermsAndConditionsAgreement','GATEWAY','I agree to the <a href="#" target="_blank">terms and conditions</a> for PayPal Banners.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.TermsAndConditionsAgreement'
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.PublisherId') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.PublisherId','GATEWAY','Your PayPal Publisher Id from the PayPal Media Network (PMN)','');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnProductPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ShowOnProductPage','GATEWAY','Show the bill me later ads on your product page.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.ShowOnProductPage'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ProductPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ProductPageDimensions','GATEWAY','PayPal ad dimensions for the product page.','120x90');
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'PayPal.Ads.ProductPageDimensions'
UPDATE dbo.[AppConfig] SET AllowableValues = '120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200' WHERE [Name] = 'PayPal.Ads.ProductPageDimensions'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnHomePage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ShowOnHomePage','GATEWAY','Show the bill me later ads on your home page.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.ShowOnHomePage'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.HomePageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.HomePageDimensions','GATEWAY','PayPal ad dimensions for the home page.','120x90');
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'PayPal.Ads.HomePageDimensions'
UPDATE dbo.[AppConfig] SET AllowableValues = '120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200' WHERE [Name] = 'PayPal.Ads.HomePageDimensions'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnCartPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ShowOnCartPage','GATEWAY','Show the bill me later ads on your Shopping Cart page.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.ShowOnCartPage'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.CartPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.CartPageDimensions','GATEWAY','PayPal ad dimensions for the product page.','120x90');
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'PayPal.Ads.CartPageDimensions'
UPDATE dbo.[AppConfig] SET AllowableValues = '120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200' WHERE [Name] = 'PayPal.Ads.CartPageDimensions'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnEntityPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.ShowOnEntityPage','GATEWAY','Show the bill me later ads on your entity pages.','false');
UPDATE dbo.[AppConfig] SET ValueType = 'boolean' WHERE [Name] = 'PayPal.Ads.ShowOnEntityPage'
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.EntityPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.EntityPageDimensions','GATEWAY','PayPal ad dimensions for the entity pages.','120x90');
UPDATE dbo.[AppConfig] SET ValueType = 'enum' WHERE [Name] = 'PayPal.Ads.EntityPageDimensions'
UPDATE dbo.[AppConfig] SET AllowableValues = '120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200' WHERE [Name] = 'PayPal.Ads.EntityPageDimensions'
END
GO


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'Topic' and COLUMN_NAME = 'IsFrequent')
	ALTER TABLE Topic ADD IsFrequent BIT DEFAULT 1 WITH VALUES
	GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Promotions.ApplyDiscountsBeforePromoApplied') BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'Promotions.ApplyDiscountsBeforePromoApplied','SETUP','If this is set to TRUE, promotions will be applied after quantity discounts.','true','boolean');
END
GO


ALTER PROCEDURE [dbo].[aspdnsf_GetCustomersAlsoBoughtProducts]  
    @NumProductToDisplay	int,  
    @productid				int,  
    @CustomerLevelID		int,  
    @InvFilter				int,  
    @affiliateID			int,
	@storeID				int = 1,
	@filterProduct			bit = 0		  

  
AS  
SET NOCOUNT ON   

DECLARE @custlevelcount int, @affiliatecount int, @AffiliateExists int, @FilterProductsByAffiliate tinyint, @CustomerLevelFilteringIsAscending bit, @FilterProductsByCustomerLevel tinyint, @DisplayOutOfStockProducts tinyint, @OutOfStockThreshold int  

SELECT @custlevelcount = si.rows from dbo.sysobjects so with (nolock) join dbo.sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductCustomerLevel') and si.indid < 2 and type = 'u'  
SELECT @FilterProductsByCustomerLevel = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel'  
SELECT @affiliatecount = count(*),  @AffiliateExists = sum(case when AffiliateID = @affiliateID then 1 else 0 end) from dbo.ProductAffiliate with (nolock) where ProductID = @ProductID   

SET @CustomerLevelFilteringIsAscending  = 0  
SELECT @CustomerLevelFilteringIsAscending  = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (NOLOCK) WHERE name = 'FilterByCustomerLevelIsAscending'  

SELECT @FilterProductsByAffiliate = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate'  


SELECT TOP (@NumProductToDisplay) p.ProductID,p.ImageFilenameOverride, p.Name,p.SKU, p.SEName, Description, a.CountOfOrders, UnitsOrdered, SEAltText
FROM dbo.product p with (nolock)   
    join (  
            SELECT top 8 os.productid, count(*) CountOfOrders, sum(os.Quantity) UnitsOrdered  
            from dbo.orders_shoppingcart os   
                join (SELECT distinct o.ordernumber  
                      from dbo.orders_shoppingcart os with (nolock)   
                           join dbo.orders o with (nolock) on os.ordernumber = o.ordernumber   
                      WHERE (o.TransactionState = 'CAPTURED' or o.TransactionState = 'AUTHORIZED' or o.TransactionState = 'AUTH' )and os.ProductID = @productid --and o.orderdate > dateadd(dy, -360, getdate())  
                     ) o on os.ordernumber = o.ordernumber   
                join dbo.product p with (nolock) on os.productid = p.productid  
                join dbo.productvariant pv with (nolock) on os.variantid = pv.variantid  
                left join (select variantid, sum(quan) inventory from inventory group by variantid) i on pv.variantid = i.variantid  
                left join dbo.productcustomerlevel pcl with (nolock) on os.productid = pcl.productid  
                left join dbo.ProductAffiliate     pa  with (nolock) on os.ProductID = pa.ProductID
				left JOIN dbo.ProductStore ps on p.ProductID = ps.ProductID   
            WHERE os.productid <> @productid  
                  and p.published = 1 AND p.deleted = 0   
                  and pv.published = 1 AND pv.deleted = 0   
                  and GETDATE() BETWEEN ISNULL(p.AvailableStartDate, '1/1/1900') AND ISNULL(p.AvailableStopDate, '1/1/2999')   
                  and pv.Inventory >= @InvFilter  
                  and (case   
                        when @FilterProductsByCustomerLevel = 0 or @custlevelcount = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1  
                        when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1   
                        when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1   
                        else 0  
                       end  = 1  
                      )  
                  and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)  
                  and (ps.StoreID = @storeID or @filterProduct = 0)  
            GROUP BY os.productid   
            ORDER BY count(*) desc, sum(os.Quantity) desc  
        ) a on p.productid = a.productid  
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'DefaultHeight_micro') BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(0,'DefaultHeight_micro','IMAGERESIZE','Default height of an micro image if no height attribute is specified in the other size-configs (i.e. width:50;). This value should NOT be left blank.','50','NULL');
END
GO

ALTER proc [dbo].[aspdnsf_MonthlyMaintenance]
-- BACKUP YOUR DB BEFORE USING THIS SCRIPT!
    @InvalidateCustomerCookies      tinyint = 1,
    @PurgeAnonCustomers             tinyint = 1,
    @CleanShoppingCartsOlderThan    smallint = 30,  -- set to 0 to disable erasing
    @CleanWishListsOlderThan        smallint = 30,  -- set to 0 to disable erasing
    @CleanGiftRegistriesOlderThan   smallint = 30,  -- set to 0 to disable erasing
    @EraseCCFromAddresses           tinyint = 1,    -- except those used for recurring billing items!
    @EraseSQLLogOlderThan           smallint = 30,  -- set to 0 to disable erasing
    @ClearProductViewsOrderThan     smallint = 180, 
    @EraseCCFromOrdersOlderThan     smallint = 30,  -- set to 0 to disable erasing
    @DefragIndexes                  tinyint = 0,
    @PurgeDeletedRecords            tinyint = 0,     -- Purges records in all tables with a deleted flag set to 1
	@RemoveRTShippingDataOlderThan  smallint = 30,    -- set to 0 to disable erasing
	@ClearSearchLogOlderThan		smallint = 30    -- set to 0 to disable erasing
AS
BEGIN
    set nocount on 
    DECLARE @cmd varchar(8000)

    -- clear out old stuff in failed transactions:
    delete from failedtransaction where orderdate < dateadd(mm,-2,getdate());

    -- clear out old tx info, not longer needed:
    update orders set TransactionCommand=NULL, AuthorizationResult=NULL, VoidTXCommand=NULL, VoidTXResult=NULL, CaptureTXCommand=NULL, CaptureTXResult=NULL, RefundTXCommand=NULL, RefundTXResult=NULL where orderdate < dateadd(mm,-2,getdate());

    -- invalidate all user cookies (forces re-login of all customers, for security safety):
    IF @InvalidateCustomerCookies = 1
    BEGIN
        update [dbo].customer set CustomerGUID=newid();
    END

    -- clean out RefundTXCommand, not needed anymore:
    update orders set RefundTXCommand=NULL;

    -- clean up all carts (don't touch recurring items, gift registry items, or wishlist items however):
    IF @CleanShoppingCartsOlderThan <> 0
    BEGIN 
        delete dbo.kitcart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
        delete dbo.customcart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
        delete dbo.ShoppingCart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
    END

    IF @CleanWishListsOlderThan <> 0
    BEGIN
        delete dbo.kitcart where CartType=1 and CreatedOn < dateadd(d,-@CleanWishListsOlderThan,getdate());
        delete dbo.customcart where CartType=1 and CreatedOn < dateadd(d,-@CleanWishListsOlderThan,getdate());
        delete dbo.ShoppingCart where CartType=1 and CreatedOn < dateadd(d,-@CleanWishListsOlderThan,getdate());
    END

    IF @CleanGiftRegistriesOlderThan = 1
    BEGIN
        delete dbo.kitcart where CartType=3 and CreatedOn < dateadd(d,-@CleanGiftRegistriesOlderThan,getdate());
        delete dbo.customcart where CartType=3 and CreatedOn < dateadd(d,-@CleanGiftRegistriesOlderThan,getdate());
        delete dbo.ShoppingCart where CartType=3 and CreatedOn < dateadd(d,-@CleanGiftRegistriesOlderThan,getdate());
    END

    -- purge anon customers:
    IF @PurgeAnonCustomers = 1
    BEGIN
        delete dbo.customer 
        where IsRegistered=0 and IsAdmin = 0
            and customerid not in (select customerid from dbo.ShoppingCart with (NOLOCK)) 
            and customerid not in (select customerid from dbo.kitcart with (NOLOCK)) 
            and customerid not in (select customerid from dbo.customcart with (NOLOCK)) 
            and customerid not in (select customerid from dbo.orders with (NOLOCK)) 
            and customerid not in (select customerid from dbo.rating with (NOLOCK)) 
            and customerid not in (select ratingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
            and customerid not in (select votingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
            and customerid not in (select customerid from dbo.pollvotingrecord with (NOLOCK))
    END
	
    -- clean any orphaned addresses
    delete dbo.Address where CustomerID Not in (select CustomerID from dbo.customer with (NOLOCK))

    -- clean addresses, except for those that have recurring orders
    IF @EraseCCFromAddresses = 1
    BEGIN
        IF exists (select * from dbo.sysobjects with (nolock) where type = 'u' and name = 'address')
            IF exists (select * from dbo.syscolumns with (nolock) where id = object_id('address') and name = 'CardExtraCode')
            BEGIN
                SET @cmd = 'update [dbo].address set CardNumber=''1111111111111111'', CardStartDate=''11/11'', CardIssueNumber=''111111111'', CardExtraCode=''1111'',CardExpirationMonth=''11'', CardExpirationYear=''1111'', eCheckBankABACode=''11111111'', eCheckBankAccountNumber=''11111111'' where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].address set CardNumber=NULL, CardStartDate=NULL, CardIssueNumber=NULL, CardExtraCode=NULL,CardExpirationMonth=NULL, CardExpirationYear=NULL, eCheckBankABACode=NULL, eCheckBankAccountNumber=NULL where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
            END 
            ELSE
            BEGIN
                SET @cmd = 'update [dbo].address set CardNumber=''1111111111111111'', CardStartDate=''11/11'', CardIssueNumber=''11111111'', CardExpirationMonth=''11'', CardExpirationYear=''1111'', eCheckBankABACode=''11111111'', eCheckBankAccountNumber=''11111111'' where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].address set CardNumber=NULL, CardStartDate=NULL, CardIssueNumber=NULL, CardExpirationMonth=NULL, CardExpirationYear=NULL, eCheckBankABACode=NULL, eCheckBankAccountNumber=NULL where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
            END 
        ELSE
        BEGIN
            -- erase credit card info from all customer records (recurring orders were not supported in these old versions)
            IF exists (select * From sysobjects so with (nolock) join [dbo].syscolumns sc on so.id = sc.id where so.type= 'u' and so.name = 'customer' and sc.name = 'CardNumber') BEGIN
                SET @cmd = 'update [dbo].Customer SET CardNumber = ''1111111111111111'''
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].Customer SET CardNumber = null'
                EXEC (@cmd)
                SET @cmd = ''
            END 
        END
    END

    -- erase credit cards from all orders older than N days:
    IF @EraseCCFromOrdersOlderThan <> 0
    BEGIN
        update [dbo].orders set CardNumber=NULL, eCheckBankABACode=NULL,eCheckBankAccountNumber=NULL WHERE OrderDate < dateadd(d,-@EraseCCFromOrdersOlderThan,getdate())
        IF exists (select * From dbo.sysobjects so with (nolock) join [dbo].syscolumns sc on so.id = sc.id where so.type= 'u' and so.name = 'orders' and sc.name = 'CardExtraCode') BEGIN
            SET @cmd = 'update [dbo].Orders SET CardExtraCode = ''1111'''
            EXEC (@cmd)
            SET @cmd = ''
            SET @cmd = 'update [dbo].Orders SET CardExtraCode = null'
            EXEC (@cmd)
            SET @cmd = ''
        END 
    END

    -- erase logged sql statements
    IF @EraseSQLLogOlderThan <> 0
    BEGIN
        DELETE dbo.SQLLog WHERE ExecutedOn < dateadd(d,-@EraseSQLLogOlderThan,getdate())
    END
    
    -- erase product views both for recently and dynamic
    IF @ClearProductViewsOrderThan <> 0
    BEGIN 
		DELETE dbo.ProductView WHERE ViewDate < dateadd(d,-@ClearProductViewsOrderThan,getdate())
	END

    truncate table CustomerSession

    exec aspdnsf_CustomerConsistencyCheck


    -- nuke all products that were used as temporary products for checkout (auction) purposes
    declare @productid int
    select productid into #tmpproduct from dbo.product with (nolock) where deleted = 2
    select top 1 @productid = productid from #tmpproduct 
    while @@rowcount > 0 begin
        exec dbo.aspdnsf_NukeProduct @productid
        delete #tmpproduct where productid = @productid
        select top 1 @productid = productid from #tmpproduct 
    end
    drop table #tmpproduct 

    IF @PurgeDeletedRecords = 1 BEGIN
        DELETE dbo.Address where deleted = 1
        DELETE dbo.Affiliate where deleted = 1
        DELETE dbo.Category where deleted = 1
        DELETE dbo.Coupon where deleted = 1
        DELETE dbo.Customer where deleted = 1
        DELETE dbo.CustomerLevel where deleted = 1
        DELETE dbo.Distributor where deleted = 1
        DELETE dbo.Document where deleted = 1
        DELETE dbo.FAQ where deleted = 1
        DELETE dbo.Gallery where deleted = 1
        DELETE dbo.Genre where deleted = 1
        DELETE dbo.Library where deleted = 1
        DELETE dbo.Manufacturer where deleted = 1
        DELETE dbo.News where deleted = 1
        DELETE dbo.Partner where deleted = 1
        DELETE dbo.Poll where deleted = 1
        DELETE dbo.PollAnswer where deleted = 1 or not exists (select * from dbo.Poll where PollID = PollAnswer.PollID)
        DELETE dbo.Product where deleted = 1
        DELETE dbo.ProductVariant where deleted = 1 or not exists (select * from dbo.Product where productid = ProductVariant.productid)
        DELETE dbo.SalesPrompt where deleted = 1
        DELETE dbo.Section where deleted = 1
        DELETE dbo.ShippingZone where deleted = 1
        DELETE dbo.Staff where deleted = 1
        DELETE dbo.Topic where deleted = 1
        DELETE dbo.Vector where deleted = 1

        DELETE dbo.ProductVector where not exists (select * from dbo.product where productid = ProductVector.productid) or not exists (select * from dbo.vector where vectorid = ProductVector.vectorid) 
        DELETE dbo.ProductAffiliate where not exists (select * from dbo.product where productid = ProductAffiliate.productid) or not exists (select * from dbo.Affiliate where Affiliateid = ProductAffiliate.Affiliateid) 
        DELETE dbo.ProductCategory where not exists (select * from dbo.product where productid = ProductCategory.productid) or not exists (select * from dbo.Category where Categoryid = ProductCategory.Categoryid) 
        DELETE dbo.ProductCustomerLevel where not exists (select * from dbo.product where productid = ProductCustomerLevel.productid) or not exists (select * from dbo.CustomerLevel where CustomerLevelid = ProductCustomerLevel.CustomerLevelid) 
        DELETE dbo.ProductDistributor where not exists (select * from dbo.product where productid = ProductDistributor.productid) or not exists (select * from dbo.Distributor where Distributorid = ProductDistributor.Distributorid) 
        DELETE dbo.ProductGenre where not exists (select * from dbo.product where productid = ProductGenre.productid) or not exists (select * from dbo.Genre where Genreid = ProductGenre.Genreid) 
        DELETE dbo.ProductLocaleSetting where not exists (select * from dbo.product where productid = ProductLocaleSetting.productid) or not exists (select * from dbo.LocaleSetting where LocaleSettingid = ProductLocaleSetting.LocaleSettingid) 
        DELETE dbo.ProductManufacturer where not exists (select * from dbo.product where productid = ProductManufacturer.productid) or not exists (select * from dbo.Manufacturer where Manufacturerid = ProductManufacturer.Manufacturerid) 
        DELETE dbo.ProductSection where not exists (select * from dbo.product where productid = ProductSection.productid) or not exists (select * from dbo.Section where Sectionid = ProductSection.Sectionid) 
        DELETE dbo.Address where not exists (select * from dbo.Customer where customerid = Address.customerid)
    END
    
    IF @RemoveRTShippingDataOlderThan <> 0
    BEGIN
		UPDATE dbo.Customer SET RTShipRequest = '', RTShipResponse = ''
		WHERE UpdatedOn < dateadd(d,-@RemoveRTShippingDataOlderThan,getdate())
		
		UPDATE dbo.Orders SET RTShipRequest = '', RTShipResponse = ''
		WHERE UpdatedOn < dateadd(d,-@RemoveRTShippingDataOlderThan,getdate())
    END
    
    --Search log
    IF @ClearSearchLogOlderThan <> 0
	BEGIN
		DELETE FROM dbo.SearchLog WHERE CreatedOn < dateadd(d,-@ClearSearchLogOlderThan,getdate())
	END

    -- Defrag indexes
    IF @DefragIndexes = 1
    BEGIN
        CREATE TABLE #SHOWCONTIG (
           tblname VARCHAR (255),
           ObjectId INT,
           IndexName VARCHAR (255),
           IndexId INT,
           Lvl INT,
           CountPages INT,
           CountRows INT,
           MinRecSize INT,
           MaxRecSize INT,
           AvgRecSize INT,
           ForRecCount INT,
           Extents INT,
           ExtentSwitches INT,
           AvgFreeBytes INT,
           AvgPageDensity INT,
           ScanDensity DECIMAL,
           BestCount INT,
           ActualCount INT,
           LogicalFrag DECIMAL,
           ExtentFrag DECIMAL)

        SELECT [name] tblname into #tmp FROM dbo.sysobjects with (nolock) WHERE type = 'u' ORDER BY Name

        DECLARE @tblname varchar(255), @indexname varchar(255)
        SELECT top 1 @tblname = tblname FROM #tmp
        WHILE @@rowcount > 0 BEGIN
            SET @cmd = 'DBCC SHOWCONTIG (''' + @tblname + ''') with tableresults, ALL_INDEXES'
            INSERT #SHOWCONTIG
            EXEC (@cmd)
            DELETE #tmp WHERE tblname = @tblname 
            SELECT top 1 @tblname = tblname FROM #tmp
        END


        DELETE #SHOWCONTIG WHERE LogicalFrag < 5 or Extents = 1 or IndexId in (0, 255)


        SELECT top 1 @tblname = tblname, @indexname = IndexName FROM #SHOWCONTIG ORDER BY IndexId
        WHILE @@rowcount > 0 BEGIN
            SET @cmd = 'DBCC DBREINDEX (''' + @tblname + ''', ''' + @indexname + ''', 90)  '
            EXEC (@cmd)
            DELETE #SHOWCONTIG WHERE tblname = @tblname 
            SELECT top 1 @tblname = tblname, @indexname = IndexName FROM #SHOWCONTIG ORDER BY IndexId
        END
    END
END
GO


IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('NewsletterMailList') AND name = 'StoreId') 
BEGIN
	ALTER TABLE [dbo].[NewsletterMailList] ADD StoreId INT
	ALTER TABLE [dbo].[NewsletterMailList] ADD  CONSTRAINT [DF_NewsletterMailList_StoreID]  DEFAULT ((1)) FOR [StoreID]
END
GO


ALTER PROCEDURE [dbo].[aspdnsf_getMailingList]
(
	@ListType tinyint,
		--	0 = Email Blast
		--	1 = NewsLetter
	@withOrdersOnly tinyint	= 0,
	@mailingSubject nvarchar(255) = ''
)
AS
BEGIN
SET NOCOUNT ON

/*SELECTING FROM THE CUSTOMER TABLE*/
IF (@ListType = 0) BEGIN

	/*RETRIEVE A LIST OF EMAIL ADDRESSES WHOM HAVE ALREADY RECEIVED THIS EMAIL*/
	DECLARE @EMailSentAlready TABLE (EMailAddress nvarchar(255))
	
	INSERT INTO @EMailSentAlready ([EMailAddress])	SELECT DISTINCT [ToEmail] 
													FROM [MailingMgrLog]
													WHERE (RTRIM(@mailingSubject) <> ''
														AND MONTH([SentOn]) = MONTH(GetDate())
														AND DAY([SentOn]) = DAY(GetDate())
														AND YEAR([SentOn]) = YEAR(GetDate())
														AND [Subject] = RTRIM(@mailingSubject))

	/*FIND ONLY UNIQUE EMAILS FROM THE CUSTOMER TABLE AND PERFORM FILTERING*/
	DECLARE @UniqueEMails TABLE	(EMailAddress nvarchar(255),
								 RecipientGuid UniqueIdentifier,
								 RecipientID int,
								 FirstName nvarchar(255),
								 LastName nvarchar(255))
	
	INSERT INTO @UniqueEMails 
	SELECT DISTINCT [EMail], null, null, null, null
		FROM [Customer]
		WHERE [OkToEmail] = 1
			AND [Deleted] = 0
			AND [Email] <> ''
			AND (@withOrdersOnly = 0 
					OR [CustomerID] IN (SELECT CustomerID FROM [Orders] WHERE [Deleted] = 0))
			AND [Email] NOT IN (Select [EMailAddress] FROM @EMailSentAlready)
			
	/*FILL THE REST OF THE DATA*/
	UPDATE @UniqueEMails SET	RecipientID =	(SELECT TOP 1 [CustomerID] FROM [Customer] WHERE [Email] = [EMailAddress]),
								RecipientGuid = (SELECT TOP 1 [CustomerGuid] FROM [Customer] WHERE [Email] = [EMailAddress]),
								FirstName =		(SELECT TOP 1 [FirstName] FROM [Customer] [c] WHERE [Email] = [EMailAddress]),
								LastName =		(SELECT TOP 1 [LastName] FROM [Customer] [c] WHERE [Email] = [EMailAddress])
													
	/*RETRIEVE REQUIRED DATA*/
	SELECT * FROM @UniqueEMails
	
	
END
/*RETRIEVING FROM THE NEWSLETTER TABLE*/
ELSE BEGIN
	SELECT	[ID]			AS RecipientID,
			[GUID]			AS RecipientGUID,
			[EMailAddress],
			[FirstName],
			[LastName],
			[SubscribedOn],
			[StoreId]
	FROM [NewsletterMailList]
	WHERE [SubscriptionConfirmed] = 1
		AND	([UnsubscribedOn] IS NULL  OR [UnsubscribedOn] > GETDATE()) 
END

END
GO

if not exists (select * from AppConfig where Name = 'Account.ShowBirthDateField')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Account.ShowBirthDateField', 'This will show the Birth Date field while creating customer account.', '', 'boolean', 'null', 'DISPLAY', 0, 0, getdate())
GO

IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = object_id(N'OrderNumber_Updated'))
	DROP TRIGGER [dbo].[OrderNumber_Updated]
GO

print 'Adding ShowDistributorNotificationPriceInfo appconfig'
if not exists (select * from AppConfig where Name = 'ShowDistributorNotificationPriceInfo')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'ShowDistributorNotificationPriceInfo', 'Show prices and order total information on the distributor notification emails.', 'false', 'boolean', 'null', 'MISC', 0, 0, getdate())
GO

print 'Increasing the max length of the appconfig value column'
if not exists(select CHARACTER_MAXIMUM_LENGTH  from INFORMATION_SCHEMA.COLUMNS where COLUMN_NAME = 'configvalue' and TABLE_NAME = 'appconfig' and CHARACTER_MAXIMUM_LENGTH = -1)
begin
	ALTER TABLE appconfig
	ALTER COLUMN configvalue nvarchar(max)
end

print 'Removing the basic OPC checkout type and related appconfigs'

update AppConfig set Description = 'The Checkout Type.  Valid Values are Standard, SmartOPC, or Other.', 
		AllowableValues = 'Standard,SmartOPC,Other' 
	where Name = 'Checkout.Type' 
	
update AppConfig set  ConfigValue = 'Standard' 
	where  Name = 'Checkout.Type' 
	and (ConfigValue = 'BasicOPC' or ConfigValue = 'DeprecatedOPC')

delete from appconfig where name = 'Checkout.UseOnePageCheckout'
delete from appconfig where name = 'Checkout.UseOnePageCheckout.UseFinalReviewOrderPage'


--Add GTIN for 9.4
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('Inventory') AND name = 'GTIN') 
BEGIN
	ALTER TABLE [dbo].[Inventory] ADD GTIN nvarchar(14) null
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ProductVariant') AND name = 'GTIN') 
BEGIN
	ALTER TABLE [dbo].[ProductVariant] ADD GTIN nvarchar(14) null
END

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ShoppingCart') AND name = 'GTIN') 
BEGIN
	ALTER TABLE [dbo].[ShoppingCart] ADD GTIN nvarchar(14) null
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('Orders_ShoppingCart') AND name = 'GTIN') 
BEGIN
	ALTER TABLE [dbo].[Orders_ShoppingCart] ADD GTIN nvarchar(14) null
END
GO

ALTER proc dbo.aspdnsf_AddItemToCart
    @CustomerID int,
    @ProductID int,
    @VariantID int,
    @Quantity int,
    @ShippingAddressID int,
    @BillingAddressID int,
    @ChosenColor nvarchar(100),
    @ChosenColorSKUModifier varchar(100),
    @ChosenSize nvarchar(100),
    @ChosenSizeSKUModifier varchar(100),
    @CleanColorOption nvarchar(100), 
    @CleanSizeOption nvarchar(100),
    @ColorAndSizePriceDelta money,
    @TextOption ntext,
    @CartType int,
    @GiftRegistryForCustomerID int,
    @CustomerEnteredPrice money,
    @CustomerLevelID int = 0,
    @RequiresCount int = 0, 		
	@IsKit2 tinyint = 0,
    @NewShoppingCartRecID int OUTPUT,
    @StoreID int, 
    @IsAGift bit = 0
AS
SET NOCOUNT ON
	DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int, @InventoryFullSKU nvarchar(50), @InventoryWeightDelta money, @AllowEmptySkuAddToCart varchar(10), @ShoppingCartrecid int, @IsAKit tinyint
	DECLARE @levelprice money, @LevelDiscountPercent money, @LevelDiscountsApplyToExtendedPrices tinyint, @CustEntersPrice tinyint

	SELECT @CustEntersPrice = CustomerEntersPrice, @RestrictedQy = RestrictedQuantities FROM dbo.ProductVariant with (nolock) WHERE VariantID = @VariantID

	SELECT	@IsAKit = IsAKit FROM dbo.Product with (nolock) WHERE ProductID = @ProductID 

	-- We are always going to ignore gift records, gift item code should be able to avoid duplicate records.
	SELECT @CurrentCartQty = Quantity From dbo.shoppingcart with (nolock) where ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and GiftRegistryForCustomerID = @GiftRegistryForCustomerID and CartType = @CartType and StoreID = @StoreID and (IsGift = 0 And @IsAGift = 0)

	DECLARE @RQty int
	IF isnull(rtrim(@RestrictedQy), '') = ''
		set @RQty = -1
	ELSE
		SELECT @RQty = cast(items as int) FROM dbo.Split(@RestrictedQy, ',') WHERE cast(items as int) <= isnull(@CurrentCartQty, 0) + @Quantity

	IF @CustomerLevelID = 0
		SELECT @LevelDiscountPercent = 0.0, @LevelDiscountsApplyToExtendedPrices = 0
	ELSE
		SELECT @LevelDiscountPercent = LevelDiscountPercent, @LevelDiscountsApplyToExtendedPrices = LevelDiscountsApplyToExtendedPrices FROM dbo.customerlevel with (nolock) WHERE customerlevelid = @CustomerLevelID    

	-- if item already exists in the cart update it's quantity
	IF @CurrentCartQty is not null and @IsAKit = 0 and @CustEntersPrice = 0  BEGIN
		UPDATE dbo.ShoppingCart 
		SET Quantity = case @RQty when -1 then Quantity + @Quantity else isnull(@RQty, 0) end , 
			RequiresCount = RequiresCount + @RequiresCount 
		WHERE ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and GiftRegistryForCustomerID =
	 @GiftRegistryForCustomerID and CartType = @CartType

		SET @NewShoppingCartRecID = 0
		RETURN
	END


	SELECT @AllowEmptySkuAddToCart = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [name]= 'AllowEmptySkuAddToCart'


	--Insert item into ShoppingCart
	INSERT dbo.ShoppingCart(CartType,ShoppingCartRecGUID,CustomerID,ShippingAddressID,BillingAddressID,ProductID,SubscriptionInterval,SubscriptionIntervalType,VariantID,ProductSKU,ProductPrice,CustomerEntersPrice,ProductWeight,ProductDimensions,Quantity,RequiresCount,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,TextOption,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,FreeShipping,DistributorID,RecurringInterval,RecurringIntervalType, IsSystem, IsAKit, TaxClassID, IsKit2, StoreID, IsGift, GTIN)
	SELECT 
		@CartType,
		newid(),
		@CustomerID,
		@ShippingAddressID,
		@BillingAddressID,
		@ProductID, 
		pv.SubscriptionInterval,
		pv.SubscriptionIntervalType,
		@VariantID,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(@ChosenColorSKUModifier, '') + isnull(@ChosenSizeSKUModifier, '') else i.VendorFullSKU end,
		case when isnull(@CustomerEnteredPrice, 0) > 0 then @CustomerEnteredPrice 
			 when p.IsAKit = 1 then dbo.GetCustomerLevelPrice(pv.VariantID, @CustomerLevelID)+((dbo.KitPriceDelta(@CustomerID, @ProductID, 0)*(100.0 - @LevelDiscountPercent))/100.0)
			 else dbo.GetCustomerLevelPrice(pv.VariantID, @CustomerLevelID)+@ColorAndSizePriceDelta 
		end,
		case when @CustomerEnteredPrice is not null and @CustomerEnteredPrice > 0 then 1 else 0 end,
		pv.Weight + case when p.IsAKit = 1 then dbo.KitWeightDelta(@CustomerID, @ProductID, 0) else isnull(i.WeightDelta, 0) end,
		pv.Dimensions,
		case @RQty when -1 then @Quantity else isnull(@RQty, 0) end,
		@RequiresCount,
		@ChosenColor,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then @ChosenColorSKUModifier else '' end,
		@ChosenSize,
		case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then @ChosenSizeSKUModifier else '' end,
		@TextOption,
		pv.IsTaxable,
		pv.IsShipSeparately,
		pv.IsDownload,
		pv.DownloadLocation,
		pv.FreeShipping,
		isnull(pd.DistributorID, 0),
		case pv.RecurringInterval when 0 then 1 else pv.RecurringInterval end,
		case pv.RecurringIntervalType when 0 then -5 else pv.RecurringIntervalType end,
		p.IsSystem,
		p.IsAKit,
		p.TaxClassID,
		@IsKit2,
		@StoreID,
		@IsAGift,
		case when p.TrackInventoryBySizeAndColor = 1 then i.GTIN else pv.GTIN end
	FROM dbo.Product p with (NOLOCK) 
		join dbo.ProductVariant pv with (NOLOCK) on p.productid = pv.productid 
		left join dbo.Inventory i with (NOLOCK) on pv.VariantID = i.VariantID and i.size = @CleanSizeOption and i.color = @CleanColorOption
		left join dbo.ProductDistributor pd with (NOLOCK) on p.ProductID = pd.ProductID 
	WHERE p.ProductID = @ProductID 
		and pv.VariantID = @VariantID 
		and (@AllowEmptySkuAddToCart = 'true' or rtrim(case when i.VendorFullSKU is null or rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(@ChosenColorSKUModifier, '') + isnull(@ChosenSizeSKUModifier, '') else i.VendorFullSKU end) <> '')

	SET @ShoppingCartrecid = @@IDENTITY

	--Update KitCart And CustomCart Tables if necessary
	IF (@IsAKit = 1 AND @IsKit2 = 0) BEGIN
		UPDATE KitCart SET ShoppingCartRecID = @ShoppingCartrecid WHERE ProductID = @ProductID and VariantID = @VariantID and ShoppingCartRecID = 0 and CustomerID = @CustomerID
	END

	SET @NewShoppingCartRecID = @ShoppingCartrecid
GO

print 'Updating appconfig stored procedures'
IF EXISTS (select * from dbo.sysobjects where id = object_id('aspdnsf_insAppconfig') and OBJECTPROPERTY(id, 'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_insAppconfig]
GO
Create proc [dbo].[aspdnsf_insAppconfig]
    @Name nvarchar(100),
    @Description ntext,
    @ConfigValue nvarchar(max),
    @GroupName nvarchar(100),
    @SuperOnly tinyint,
    @StoreID int,
    @ValueType nvarchar(100) = null,
    @AllowableValues nvarchar(max) = null,
    @AppConfigID int OUTPUT
  
AS
SET NOCOUNT ON 


    INSERT dbo.Appconfig(AppConfigGUID, Name, Description, ConfigValue, GroupName, SuperOnly, ValueType, AllowableValues, StoreID,CreatedOn)
    VALUES (newid(), @Name, @Description, @ConfigValue, @GroupName, @SuperOnly, @ValueType, @AllowableValues, @StoreID, getdate())

    set @AppConfigID = @@identity
GO	
	
IF EXISTS (select * from dbo.sysobjects where id = object_id('aspdnsf_updAppconfig') and OBJECTPROPERTY(id, 'IsProcedure') = 1)
    drop proc [dbo].[aspdnsf_updAppconfig]
GO
Create proc [dbo].[aspdnsf_updAppconfig]
    @AppConfigID int,
    @Description ntext = null,
    @ConfigValue nvarchar(max) = null,
    @GroupName nvarchar(100) = null,
    @SuperOnly tinyint = null,
    @StoreID int = null,
    @ValueType nvarchar(100) = null,
    @AllowableValues nvarchar(max) = null
  
AS
SET NOCOUNT ON 


    UPDATE dbo.Appconfig
    SET 
        Description = COALESCE(@Description, Description),
        ConfigValue = COALESCE(@ConfigValue, ConfigValue),
        GroupName = COALESCE(@GroupName, GroupName),
        SuperOnly = COALESCE(@SuperOnly, SuperOnly),
        StoreID =  COALESCE(@StoreID, StoreID),
	ValueType =  COALESCE(@ValueType, ValueType),
	AllowableValues = COALESCE(@AllowableValues, AllowableValues)
    WHERE AppConfigID = @AppConfigID
GO

print 'Installing and updating the maxmind appconfig parameters'
if not exists (select * from AppConfig where Name = 'MaxMind.ServiceType')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'MaxMind.ServiceType', 'This can be set to either "standard" or "premium". By default, we use the highest level of service available for your account. If you have both the premium and standard minFraud service, you can choose to use the standard service to save on costs.', 'premium', 'enum', 'standard,premium', 'GATEWAY', 0, 0, getdate())
GO
if not exists (select * from AppConfig where Name = 'MaxMind.ExplanationLink')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'MaxMind.ExplanationLink', 'The URL where admins can find more information about the maxmind riskScore. Used on the order management screen.', 'http://www.maxmind.com/en/riskscore', 'string', null, 'GATEWAY', 0, 0, getdate())
GO
UPDATE dbo.Appconfig set ConfigValue = 'http://minfraud.maxmind.com/app/minfraud_soap' where name = 'MaxMind.SOAPURL'
GO
UPDATE dbo.Appconfig 
	set Description = 'Threshold over which to fail orders. 0.10 is lowest risk. 100.0 is highest risk. By default, this setting (of 100.0) will NOT FAIL any order. You will have to set this threshold to your own liking for your own business. Every store will probably use different thresholds due to the nature of their business. Consult MaxMind.com for documentation.'
where name = 'MaxMind.FailScoreThreshold'
GO
UPDATE dbo.Appconfig 
	set  Description = 'Threshold over which to force delayed downloads on orders, regardless of the setting of AppConfig:DelayedDownloads is set to. 0.10 is lowest risk. 100.0 is highest risk. See AppConfig:MaxMind.ScoreThreshold also.'
where name = 'MaxMind.DelayDownloadThreshold'
GO
UPDATE dbo.Appconfig 
	set Description = 'Threshold over which to force delayed dropship notifications on orders, regardless of the setting of AppConfig:DelayedDropShipNotifications is set to. 0.10 is lowest risk. 100.0 is highest risk. See AppConfig:MaxMind.ScoreThreshold also.'
where name = 'MaxMind.DelayDropShipThreshold'
GO
UPDATE dbo.Appconfig 
	set Description = 'If true, the MaxMind fraud prevention score will be checked before a credit card is sent to the gateway. If the returned FraudScore exceeds AppLogic.MaxMind.FailScoreThreshold, the order will be failed. See MaxMind.com for more documentation. This feature uses MaxMind''s minFraud service version 1.3'
where name = 'MaxMind.Enabled'
GO

print 'Installing and updating the USAePay appconfig parameters'
if not exists (select * from AppConfig where Name = 'USAePay.EndpointLive')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'USAePay.EndpointLive','GATEWAY','WSDL Endpoint for USAePay SOAP API Live Transactions. Do not change.','https://www.usaepay.com/soap/gate/2E58E844', 'string');

if not exists (select * from AppConfig where Name = 'USAePay.EndpointSandbox')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'USAePay.EndpointSandbox','GATEWAY','WSDL Endpoint for USAePay SOAP API Sandbox Transactions. Do not change.','https://sandbox.usaepay.com/soap/gate/2E58E844', 'string');
GO

UPDATE dbo.Appconfig 
	set Description = 'For Developers Use Only - Non-mobile pages entered in this AppConfig will be displayed in the mobile skin when a user is viewing the mobile site.(example: manufacturers.aspx,sitemap.aspx)'
where name = 'Mobile.PageExceptions'
GO

if not exists (select * from AppConfig where Name = 'RequireEmailConfirmation')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'RequireEmailConfirmation','SETUP','If true, customers will be prompted to re-enter their email addresses for confirmation when registering or updating accounts.','false', 'boolean');
GO

if not exists (select * from AppConfig where Name = 'ShowInStorePickupInShippingEstimator')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'ShowInStorePickupInShippingEstimator','SHIPPING','If true, customers will see the ''In Store Pickup'' option (if it is enabled) in the shipping estimator on the shopping cart page.','false', 'boolean');
GO

if not exists (select * from AppConfig where Name = 'SearchDescriptionsByDefault')
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'SearchDescriptionsByDefault','SETUP','If true, searches on your site will include the description and summary fields by default. NOTE: This will put additional strain on your site''s resources and may not be advisable in some shared hosting environments.','true', 'boolean');
GO

UPDATE AppConfig SET Description = 'DEPRECATED: This Gateway is no longer supported and will be removed in a future version. ' + CAST(Description AS NVARCHAR(MAX)) WHERE Name LIKE '%PayLeap%'

print 'Installing Google Remarketing and Dynamic Remarketing configuration elements'
if not exists (select * from AppConfig where Name = 'Google.Remarketing.Enabled')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Google.Remarketing.Enabled', 'Enable Google Remarketing script on your site. This puts the contents of the Script.Google.Remarketing topic on every page of your site. You must make sure that the script.bodyclose xmlpackage is included in your template.', 'false', 'boolean', null, 'MISC', 0, 0, getdate())
GO

if not exists (select * from AppConfig where Name = 'Google.DynamicRemarketing.Enabled')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Google.DynamicRemarketing.Enabled', 'Enable Google Dynamic Remarketing script on your site. Google Remarketing must also be installed.', 'false', 'boolean', null, 'MISC', 0, 0, getdate())
GO

if not exists (select * from AppConfig where Name = 'Google.DynamicRemarketing.ProductIdentifierFormat')
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Google.DynamicRemarketing.ProductIdentifierFormat', 'This string allows you to specify the format of your google product identifiers. Valid tokens are {ProductID},{VariantID}, and {FullSKU}. These tokens are case sensitive.', '{ProductID}-{VariantID}--', 'string', null, 'MISC', 0, 0, getdate())
GO

if not exists(select name from topic where name = 'Script.Google.Remarketing')
Insert Into Topic (Name, Title, HtmlOk, ShowInSitemap, [Description]) Values
(
	'Script.Google.Remarketing', 
	'Script.Google.Remarketing', 
	1,
	0,
	''
);
GO

if not exists (select * from AppConfig where Name = 'ProductPageOutOfStockRedirect')
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue, ValueType) values(1,'ProductPageOutOfStockRedirect','GATEWAY','Will provide a 404 for the product URL if a product is hidden due to inventory.  Should only be set to true if you are not concerned about search engine page rankings.','false', 'boolean');
GO

print 'Updating google analytics'
declare @UseDeprecatedTokens varchar(10)
select @UseDeprecatedTokens = configvalue from AppConfig where Name = 'Google.EcomOrderTrackingEnabled' and storeid = 0

if not exists (select * from AppConfig where Name = 'Google.DeprecatedEcomTokens.Enabled')
begin
	insert into AppConfig(AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn) 
	values (newid(), 0, 'Google.DeprecatedEcomTokens.Enabled'
	, 'The GOOGLE_ECOM_TRACKING_ASYNCH and GOOGLE_ECOM_TRACKING_V2 tokens have been deprecated and will be removed in a future release. You can set the value of this appconfig to true to continue using the old tokens, but we recommend updating to the new method. Please see the manual for details.'
	, @UseDeprecatedTokens, 'boolean', null, 'MISC', 0, 0, getdate())
end
GO

UPDATE dbo.AppConfig 
	set Description = 'Determines whether the google ecommerce tracking code is fired on the order confirmation.  If this AppConfig is disabled, Analytics will still function, however order details will not be sent to Google.'
where name = 'Google.EcomOrderTrackingEnabled'
GO

print 'Add Avalara AppConfig'
if not exists (select * from AppConfig where name like 'AvalaraTax.PreventOrderIfAddressValidationFails')
	insert into AppConfig (AppConfigGUID, StoreID, Name, Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, Hidden, CreatedOn)
	values (newid(), 0, 'AvalaraTax.PreventOrderIfAddressValidationFails', 'If true, Avalara address validation errors will prevent checkout.', 'False', 'boolean', null, 'AVALARATAX', 0, 0, getdate())
GO

print 'Updating menu appconfigs'
if not exists(select name from appconfig where name = 'MaxMenuSize')
	begin
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) 
		values(0,'MaxMenuSize','SITEDISPLAY','The maximum number of items you want to allow in a top menu (e.g. manufacturers). If a menu is longer than this, it will display a "more" link. 0 will disable the limit altogether.','0');
	end
else
	begin
		--leave the value alone in case it is already set.
		update appconfig set description = 'The maximum number of items you want to allow in a top menu (e.g. manufacturers). If a menu is longer than this, it will display a "more" link. 0 will disable the limit altogether.' where name = 'MaxMenuSize'
	end
Go

if not exists(select name from appconfig where name = 'MaxMenuLevel')
	begin
		INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) 
		values(0,'MaxMenuLevel','SITEDISPLAY','The maximum number of menu levels to render for dynamic menu. A value of 0 will turn this limit off altogether.','0');
	end
else
	begin
		--okay to update because the appconfig is unused until now
		update appconfig set description = 'The maximum number of menu levels to render for dynamic menu. A value of 0 will turn this limit off altogether.', configvalue = '0' where name = 'MaxMenuSize'
	end
Go

print 'Add HasRecurring to aspdnsf_GetProducts'
GO
---- BEGIN Add HasRecurring to GetProducts   -----
ALTER proc [dbo].[aspdnsf_GetProducts]
    @categoryID      int = null,  
    @sectionID       int = null,  
    @manufacturerID  int = null,  
    @distributorID   int = null,  
    @genreID         int = null,  
    @vectorID        int = null,  
    @localeID        int = null,  
    @CustomerLevelID int = null,  
    @affiliateID     int = null,  
    @ProductTypeID   int = null,  
    @ViewType        bit = 1, -- 0 = all variants, 1 = one variant  
    @sortEntity      int = 0, -- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector  
    @pagenum         int = 1,  
    @pagesize        int = null,  
    @StatsFirst      tinyint = 1,  
    @searchstr       nvarchar(4000) = null,  
    @extSearch       tinyint = 0,  
    @publishedonly   tinyint = 0,  
    @ExcludePacks    tinyint = 0,  -- Deprecated
    @ExcludeKits     tinyint = 0,  
    @ExcludeSysProds tinyint = 0,  
    @InventoryFilter int = 0,  --  will only show products with an inventory level GREATER OR EQUAL TO than the number specified in this parameter, set to -1 to disable inventory filtering  
    @sortEntityName  varchar(20) = '', -- usely only when the entity id is provided, allowed values: category, section, manufacturer, distributor, genre, vector  
    @localeName      varchar(20) = '',  
    @OnSaleOnly      tinyint = 0,  
    @IncludeAll      bit = 0, -- Don't filter products that have a start date in the future or a stop date in the past  
	@storeID		 int = 1,
	@filterProduct	 bit = 0,
	@sortby			 varchar(10) = 'default',
	@since			 int = 180  -- best sellers in the last "@since" number of days
AS
BEGIN
	SET NOCOUNT ON   
  
    DECLARE @rcount int
    DECLARE @productfiltersort table (rownum int not null identity  primary key, productid int not null, price money null, saleprice money null, displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)
    DECLARE @productfilter table (rownum int not null identity  primary key, productid int not null, price money null, saleprice money null,  displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)
	DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @HideProductsWithLessThanThisInventoryLevel int  
    CREATE TABLE #displayorder ([name] nvarchar (800), productid int not null primary key, displayorder int not null)  
    CREATE TABLE #inventoryfilter (productid int not null, variantid int not null, InvQty int not null)  
    CREATE CLUSTERED INDEX tmp_inventoryfilter ON #inventoryfilter (productid, variantid)  
  
    DECLARE @custlevelcount int, @sectioncount int, @localecount int, @affiliatecount int, @categorycount int, @CustomerLevelFilteringIsAscending bit, @distributorcount int, @genrecount int, @vectorcount int, @manufacturercount int  
  
	DECLARE @ftsenabled tinyint
	
	SET @ftsenabled = 0
	
	IF ((SELECT DATABASEPROPERTY(db_name(db_id()),'IsFulltextEnabled')) = 1 
		AND EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[KeyWordSearch]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
		AND EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[dbo].[GetValidSearchString]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')))
	BEGIN
		SET @ftsenabled = 1
	END
  
    SET @FilterProductsByAffiliate = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @FilterProductsByCustomerLevel = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    SET @HideProductsWithLessThanThisInventoryLevel = (SELECT TOP 1 case ConfigValue when -1 then 0 else ConfigValue end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc)
    
    IF @InventoryFilter <> -1 and (@HideProductsWithLessThanThisInventoryLevel > @InventoryFilter or @HideProductsWithLessThanThisInventoryLevel  = -1)  
        SET @InventoryFilter  = @HideProductsWithLessThanThisInventoryLevel  
  
    SET @categoryID      = nullif(@categoryID, 0)  
    SET @sectionID       = nullif(@sectionID, 0)  
    SET @manufacturerID  = nullif(@manufacturerID, 0)  
    SET @distributorID   = nullif(@distributorID, 0)  
    SET @genreID         = nullif(@genreID, 0)  
    SET @vectorID        = nullif(@vectorID, 0)  
    SET @affiliateID     = nullif(@affiliateID, 0)  
    SET @ProductTypeID   = nullif(@ProductTypeID, 0)  
  
  
    SET @CustomerLevelFilteringIsAscending  = 0  
    SET @CustomerLevelFilteringIsAscending = (SELECT TOP 1 case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterByCustomerLevelIsAscending' AND (StoreID=@storeID OR StoreID=0) ORDER BY StoreID desc) 
  
    IF @localeID is null and ltrim(rtrim(@localeName)) <> ''  
        SELECT @localeID = LocaleSettingID FROM dbo.LocaleSetting with (nolock) WHERE Name = ltrim(rtrim(@localeName))  
  
    select @categorycount     = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productcategory') and si.indid < 2 and type = 'u'  
    select @sectioncount      = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productsection') and si.indid < 2 and type = 'u'  
    select @localecount       = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductLocaleSetting') and si.indid < 2 and type = 'u'  
    select @custlevelcount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductCustomerLevel') and si.indid < 2 and type = 'u'  
    select @affiliatecount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductAffiliate') and si.indid < 2 and type = 'u'  
    select @distributorcount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductDistributor') and si.indid < 2 and type = 'u'  
    select @genrecount        = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductGenre') and si.indid < 2 and type = 'u'  
    select @vectorcount       = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductVector') and si.indid < 2 and type = 'u'  
    select @manufacturercount = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductManufacturer') and si.indid < 2 and type = 'u'  
  
  
    -- get page size  
    IF @pagesize is null or @pagesize = 0 BEGIN  
        IF @categoryID is not null  
            SELECT @pagesize = PageSize FROM dbo.Category with (nolock) WHERE categoryID = @categoryID  
        ELSE IF @sectionID is not null  
            SELECT @pagesize = PageSize FROM dbo.Section with (nolock) WHERE sectionID = @sectionID  
        ELSE IF @manufacturerID is not null  
            SELECT @pagesize = PageSize FROM dbo.Manufacturer with (nolock) WHERE manufacturerID = @manufacturerID  
        ELSE IF @distributorID is not null  
            SELECT @pagesize = PageSize FROM dbo.Distributor with (nolock) WHERE distributorID = @distributorID  
        ELSE IF @genreID is not null  
            SELECT @pagesize = PageSize FROM dbo.Genre with (nolock) WHERE genreID = @genreID  
        ELSE IF @vectorID is not null  
            SELECT @pagesize = PageSize FROM dbo.Vector with (nolock) WHERE vectorID = @vectorID  
        ELSE   
            SELECT @pagesize = convert(int, ConfigValue) FROM dbo.AppConfig with (nolock) WHERE [Name] = 'Default_CategoryPageSize'  
    END  
  
    IF @pagesize is null or @pagesize = 0  
        SET @pagesize = 20  
  
    -- get sort order  
    IF @sortEntity = 1 or @sortEntityName = 'category' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductCategory a with (nolock) inner join (select distinct a.ProductID from ProductCategory a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b  on a.ProductID = b.ProductID where categoryID = @categoryID 
    END  
    ELSE IF @sortEntity = 2 or @sortEntityName = 'section' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductSection a with (nolock) inner join (select distinct a.ProductID from ProductSection a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where sectionId = @sectionID
    END  
    ELSE IF @sortEntity = 3 or @sortEntityName = 'manufacturer' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductManufacturer a with (nolock) inner join (select distinct a.ProductID from ProductManufacturer a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where ManufacturerID = @manufacturerID
    END  
    ELSE IF @sortEntity = 4 or @sortEntityName = 'distributor' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductDistributor a with (nolock) inner join (select distinct a.ProductID from ProductDistributor a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where DistributorID = @distributorID
    END  
    ELSE IF @sortEntity = 5 or @sortEntityName = 'genre' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductGenre a with (nolock) inner join (select distinct a.ProductID from ProductGenre a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where GenreID = @genreID
    END  
    ELSE IF @sortEntity = 6 or @sortEntityName = 'vector' BEGIN  
        INSERT #displayorder select distinct null as [name], a.productid, displayorder from dbo.ProductVector a with (nolock) inner join (select distinct a.ProductID from ProductVector a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID where VectorID = @vectorID
    END  
    ELSE BEGIN  
        INSERT #displayorder select distinct [name], a.productid, 1 from dbo.Product a with (nolock) inner join (select distinct a.ProductID from Product a with (nolock) 
        left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on a.ProductID = B.ProductID ORDER BY Name  
    END  

	IF (@ftsenabled = 1)
	BEGIN
		IF rtrim(isnull(@searchstr, '')) <> ''
		BEGIN
			DECLARE @tmpsrch nvarchar(4000)
			SET @tmpsrch = dbo.GetValidSearchString(@searchstr) 
			DELETE #displayorder from #displayorder d left join dbo.KeyWordSearch(@tmpsrch) k on d.productid = k.productid where k.productid is null  
		END
	END
	
	SET @searchstr = '%' + rtrim(ltrim(@searchstr)) + '%' 
 
    IF @InventoryFilter <> -1 BEGIN  
        IF @ViewType = 1 BEGIN  
            INSERT #inventoryfilter  
            SELECT p.productid, pv.VariantID, sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) invqty  
            FROM product p with (NOLOCK) join #displayorder d on p.ProductID = d.ProductID  
                join ProductVariant pv with (NOLOCK) on p.ProductID = pv.ProductID  and pv.IsDefault = 1  
                left join Inventory i with (NOLOCK) on pv.VariantID = i.VariantID  
            GROUP BY p.productid, pv.VariantID  
            HAVING sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) >= @InventoryFilter  
        END  
        ELSE  
            INSERT #inventoryfilter  
            SELECT p.productid, pv.VariantID, sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) invqty  
            FROM product p with (NOLOCK) join #displayorder d on p.ProductID = d.ProductID  
                join ProductVariant pv with (NOLOCK) on p.ProductID = pv.ProductID  
                left join Inventory i with (NOLOCK) on pv.VariantID = i.VariantID  
            GROUP BY p.productid, pv.VariantID  
            HAVING sum(case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end ) >= @InventoryFilter  
  

        INSERT @productfilter (productid, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)  
        SELECT distinct p.productid, do.displayorder, pv.VariantID, pv.DisplayOrder, p.Name, pv.Name  
        FROM   
            product p with (nolock)  
            join #displayorder do on p.ProductID = do.ProductID   
            left join ProductVariant pv        with (NOLOCK) ON p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
            left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID   
            left join productsection ps        with (nolock) on p.ProductID = ps.ProductID   
            left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID   
            left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID   
            left join ProductGenre px          with (nolock) on p.ProductID = px.ProductID   
            left join ProductVector px2        with (nolock) on p.ProductID = px2.ProductID   
            left join ProductLocaleSetting pl  with (nolock) on p.ProductID = pl.ProductID   
            left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID   
            left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID
			   
            join #inventoryfilter i on pv.VariantID = i.VariantID  
        WHERE   
              (pc.categoryid = @categoryID or @categoryID is null or @categorycount = 0)  
          and (ps.sectionid = @sectionID or @sectionID is null or @sectioncount = 0)  
          and (pl.LocaleSettingID = @localeID or @localeID is null or @localecount = 0)  
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)  
          and (pm.manufacturerid = @manufacturerID or @manufacturerID is null or @manufacturercount = 0)  
          and (pd.DistributorID = @distributorID or @distributorID is null or @distributorcount = 0)  
          and (px.GenreID = @genreID or @genreID is null or @genrecount = 0)  
          and (px2.VectorID = @vectorID or @vectorID is null or @vectorcount = 0)  
          and p.ProductTypeID = coalesce(@ProductTypeID, p.ProductTypeID)  
          and (case   
                when @FilterProductsByCustomerLevel = 0 or @custlevelcount = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1  
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1   
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1   
                else 0  
               end  = 1  
              )  
          and (@ftsenabled = 1 or
				(@ftsenabled = 0 and
					(@searchstr is null  
					 or patindex(@searchstr, isnull(p.name, '')) > 0  
					 or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0   
					 or patindex(@searchstr, isnull(pv.name, '')) > 0  
					 or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0  
					 or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0  
					 or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0  
					 or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)  
					 or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)  
					)
				)
              )  
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= @OnSaleOnly  
          and p.published >= @publishedonly  
          and pv.published >= @publishedonly  
          and isnull(p.IsAKit, 0) <= 1-@ExcludeKits  
          and p.IsSystem <= 1-@ExcludeSysProds  
          and p.Deleted = 0  
          and pv.Deleted = 0  
          and ((@IncludeAll = 1) or (getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999')))    
        order by do.displayorder, p.Name, pv.DisplayOrder, pv.Name  
  
    END   
    ELSE BEGIN  
        INSERT @productfilter (productid, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName)  
        SELECT distinct p.productid, do.displayorder, pv.VariantID, pv.DisplayOrder, p.Name, pv.Name  
        FROM   
            product p with (nolock)  
            join #displayorder do on p.ProductID = do.ProductID   
            join ProductVariant pv             with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
            left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID   
            left join productsection ps        with (nolock) on p.ProductID = ps.ProductID   
            left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID   
            left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID   
            left join ProductGenre px          with (nolock) on p.ProductID = px.ProductID   
            left join ProductVector px2        with (nolock) on p.ProductID = px2.ProductID   
            left join ProductLocaleSetting pl  with (nolock) on p.ProductID = pl.ProductID   
            left join ProductCustomerLevel pcl with (nolock) on p.ProductID = pcl.ProductID   
            left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID   
        WHERE   
              (pc.categoryid = @categoryID or @categoryID is null or @categorycount = 0)  
          and (ps.sectionid = @sectionID or @sectionID is null or @sectioncount = 0)  
          and (pl.LocaleSettingID = @localeID or @localeID is null or @localecount = 0)  
          and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)  
          and (pm.manufacturerid = @manufacturerID or @manufacturerID is null or @manufacturercount = 0)  
          and (pd.DistributorID = @distributorID or @distributorID is null or @distributorcount = 0)  
          and (px.GenreID = @genreID or @genreID is null or @genrecount = 0)  
          and (px2.VectorID = @vectorID or @vectorID is null or @vectorcount = 0)  
          and p.ProductTypeID = coalesce(@ProductTypeID, p.ProductTypeID)  
          and (case   
                when @FilterProductsByCustomerLevel = 0 or @custlevelcount = 0 or pcl.CustomerLevelID is null or @CustomerLevelID is null then 1  
                when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID then 1   
                when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1   
                else 0  
               end  = 1  
              )  
          and (@ftsenabled = 1 or
				(@ftsenabled = 0 and
					(@searchstr is null  
					or patindex(@searchstr, isnull(p.name, '')) > 0  
					or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0   
					or patindex(@searchstr, isnull(pv.name, '')) > 0  
					or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0  
					or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0  
					or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0  
					or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)  
					or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)  
					)
				)
              )  
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= @OnSaleOnly  
          and p.published >= @publishedonly  
          and pv.published >= @publishedonly  
          and isnull(p.IsAKit, 0) <= 1-@ExcludeKits  
          and p.IsSystem <= 1-@ExcludeSysProds  
          and p.Deleted = 0  
          and pv.Deleted = 0  
          and ((@IncludeAll = 1) or (getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999')))    
        order by do.displayorder, p.Name, pv.DisplayOrder, pv.Name  
    END  
	
    SET @rcount = @@rowcount  
    IF @StatsFirst = 1  
        SELECT cast(ceiling(@rcount*1.0/@pagesize) as int) pages, @rcount ProductCount  
  
  
  --Begin sorting
  	if @sortby = 'bestseller'
		begin
			insert @productfiltersort (productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName) 
			select pf.productid, pf.price, pf.saleprice, pf.displayorder, pf.VariantID, pf.VariantDisplayOrder, pf.ProductName, pf.VariantName
				from @productfilter pf
				left join (
					select ProductID, SUM(Quantity) AS NumSales
					  from dbo.Orders_ShoppingCart sc with (NOLOCK) 
							join [dbo].Orders o with (NOLOCK)  on sc.OrderNumber = o.OrderNumber and o.OrderDate >= dateadd(dy, -@since, getdate())
					  group by ProductID 
				) bsSort on pf.productid = bsSort.ProductID
				order by isnull(bsSort.NumSales, 0) DESC
		end
  	else --default
		begin
			insert @productfiltersort (productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName) 
			select productid, price, saleprice, displayorder, VariantID, VariantDisplayOrder, ProductName, VariantName
			from @productfilter order by displayorder, productName, variantDisplayOrder, variantName
		end
		
    ---- Check filtered products for recurring variants
    Declare @ProductResults Table
    (
		ProductID int
		, VariantID int
		, HasRecurring bit
		, RowNum int		
    );
    -- temp table based on filtered product result set
    Insert Into @ProductResults
    
    SELECT   Distinct
        p.ProductID,  
		pv.VariantID,
		0,
		pf.rownum
    FROM dbo.Product p with (NOLOCK)   
        left join dbo.ProductVariant       pv  with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
        join @productfiltersort            pf                on pv.ProductID = pf.ProductID and pv.VariantID = pf.VariantID           
    WHERE pf.rownum >= @pagesize*(@pagenum-1)+1 and pf.rownum <= @pagesize*(@pagenum)  
    ORDER BY pf.rownum  
    
    -- set HasRecurring
    Update pr
    Set HasRecurring = 1    
    From @ProductResults pr, (
    Select prs.ProductId
    From @ProductResults prs, ProductVariant pv
    Where prs.ProductID = pv.ProductID
    And pv.IsRecurring = 1
    And pv.Deleted = 0
    And pv.Published = 1
    Group By prs.ProductId
    Having Count(*) > 0) tmp
    Where pr.ProductId = tmp.ProductId
    ---- End Recurring
    
    SELECT   
        p.ProductID,  
        p.Name,  
        pv.VariantID,  
        pv.Name AS VariantName,  
        p.ProductGUID,  
        p.Summary,  
        p.Description,  
        p.SEKeywords,  
        p.SEDescription,  
        p.SpecTitle,  
        p.MiscText,  
        p.SwatchImageMap,  
        p.IsFeaturedTeaser,  
        p.FroogleDescription,  
        p.SETitle,  
        p.SENoScript,  
        p.SEAltText,  
        p.SizeOptionPrompt,  
        p.ColorOptionPrompt,  
        p.TextOptionPrompt,  
        p.ProductTypeID,  
        p.TaxClassID,  
        p.SKU,  
        p.ManufacturerPartNumber,  
        p.SalesPromptID,  
        p.SpecCall,  
        p.SpecsInline,  
        p.IsFeatured,  
        p.XmlPackage,  
        p.ColWidth,  
        p.Published,  
        p.RequiresRegistration,  
        p.Looks,  
        p.Notes,  
        p.QuantityDiscountID,  
        p.RelatedProducts,  
        p.UpsellProducts,  
        p.UpsellProductDiscountPercentage,  
        p.RelatedDocuments,  
        p.TrackInventoryBySizeAndColor,  
        p.TrackInventoryBySize,  
        p.TrackInventoryByColor,  
        p.IsAKit,  
        p.ShowInProductBrowser,  
        p.IsAPack,  
        p.PackSize,  
        p.ShowBuyButton,  
        p.RequiresProducts,  
        p.HidePriceUntilCart,  
        p.IsCalltoOrder,  
        p.ExcludeFromPriceFeeds,  
        p.RequiresTextOption,  
        p.TextOptionMaxLength,  
        p.SEName,  
        p.Deleted,  
        p.CreatedOn,  
        p.ImageFileNameOverride,  
        pv.VariantGUID,  
        pv.Description AS VariantDescription,  
        pv.SEKeywords AS VariantSEKeywords,  
        pv.SEDescription AS VariantSEDescription,  
        pv.Colors,  
        pv.ColorSKUModifiers,  
        pv.Sizes,  
        pv.SizeSKUModifiers,  
        pv.FroogleDescription AS VariantFroogleDescription,  
        pv.SKUSuffix,  
        pv.ManufacturerPartNumber AS VariantManufacturerPartNumber,  
        pv.Price,  
        pv.CustomerEntersPrice,   
        pv.CustomerEntersPricePrompt,  
        isnull(pv.SalePrice, 0) SalePrice,  
        cast(isnull(pv.Weight,0) as decimal(10,1)) Weight,  
        pv.MSRP,  
        pv.Cost,  
        isnull(pv.Points,0) Points,  
        pv.Dimensions,  
        case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end Inventory,  
        pv.DisplayOrder as VariantDisplayOrder,  
        pv.Notes AS VariantNotes,  
        pv.IsTaxable,  
        pv.IsShipSeparately,  
        pv.IsDownload,  
        pv.DownloadLocation,  
        pv.Published AS VariantPublished,  
        pv.IsSecureAttachment,  
        pv.IsRecurring,  
        pv.RecurringInterval,  
        pv.RecurringIntervalType,  
        pv.SubscriptionInterval,  
        pv.SEName AS VariantSEName,  
        pv.RestrictedQuantities,  
        pv.MinimumQuantity,  
        pv.Deleted AS VariantDeleted,  
        pv.CreatedOn AS VariantCreatedOn,  
        d.Name AS DistributorName,  
        d.DistributorID,  
        d.SEName AS DistributorSEName,  
        m.ManufacturerID,  
        m.Name AS ManufacturerName,  
        m.SEName AS ManufacturerSEName,  
        s.Name AS SalesPromptName,
        pf.HasRecurring,  
        case when pcl.productid is null then 0 else isnull(ep.Price, 0) end ExtendedPrice  
    FROM dbo.Product p with (NOLOCK)   
        left join dbo.ProductVariant       pv  with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= @ViewType  
        join @ProductResults			   pf                on pv.ProductID = pf.ProductID and pv.VariantID = pf.VariantID
        left join dbo.SalesPrompt           s  with (NOLOCK) on p.SalesPromptID = s.SalesPromptID   
        left join dbo.ProductManufacturer  pm  with (NOLOCK) on p.ProductID = pm.ProductID   
        left join dbo.Manufacturer          m  with (NOLOCK) on pm.ManufacturerID = m.ManufacturerID   
        left join dbo.ProductDistributor   pd  with (NOLOCK) on p.ProductID = pd.ProductID  
        left join dbo.Distributor           d  with (NOLOCK) on pd.DistributorID = d.DistributorID  
        left join dbo.ExtendedPrice        ep  with (NOLOCK) on ep.VariantID = pv.VariantID and ep.CustomerLevelID = @CustomerLevelID  
        left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID  
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID       
  
    IF @StatsFirst <> 1  
        SELECT cast(ceiling(@rcount*1.0/@pagesize) as int) pages, @rcount ProductCount  
END
---- END Add HasRecurring to GetProducts   -----

GO

print 'Updating PayPal Appconfig descriptions'
update appconfig set description = 'Set to true to use PayPal Instant Notification to capture payments' where name = 'PayPal.UseInstantNotification'
update appconfig set description = 'PayPal assigned API merchant e-mail address for your account. Consult PayPal documentation for more information. This is almost ALWAYS left blank!' where name = 'PayPal.API.MerchantEMailAddress'
update appconfig set description = 'PayPal assigned API username. Consult PayPal documentation for more information. You get this from the PayPal site.' where name = 'PayPal.API.Username'
update appconfig set description = 'PayPal assigned API password. Consult PayPal documentation for more information. You get this from the PayPal site.' where name = 'PayPal.API.Password'
update appconfig set description = 'PayPal assigned API signature. Consult PayPal documentation for more information. You get this from the PayPal site.' where name = 'PayPal.API.Signature'
update appconfig set description = 'This shows the PayPal Express checkout button on your shopping cart page. You will also need to configure your PayPal API credentials.' where name = 'PayPal.Express.ShowOnCartPage'
update appconfig set description = 'This enables customers to checkout using PayPal Express without being a registered customer (i.e. anonymous customer). When you set this to true you also need to set the AllowCustomerDuplicateEMailAddresses AppConfig parameter to true.' where name = 'PayPal.Express.AllowAnonCheckout'

/*********** End 9.4.0.0 Changes ***********************/



/*********** Begin 9.4.1.0 Changes *********************/

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'GiftCards.Enabled') BEGIN
	INSERT [dbo].[AppConfig] ([SuperOnly], [Name], [GroupName], [Description], [ValueType] ,[ConfigValue])
		values(1,'GiftCards.Enabled','SITEDISPLAY','Enables GiftCards to be used in the shopping cart', 'boolean', 'true');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Promotions.Enabled') BEGIN
	INSERT [dbo].[AppConfig] ([SuperOnly], [Name], [GroupName], [Description], [ValueType] ,[ConfigValue])
		values(1,'Promotions.Enabled','SITEDISPLAY','Enables Promotions to be used in the shopping cart', 'boolean', 'true');
END
GO

-- Disable promotions if they were disabled before
IF EXISTS (SELECT * FROM [dbo].[AppConfig] WHERE [Name] = 'DisallowCoupons') BEGIN
	DECLARE @PromosEnabled varchar(max);
	SELECT @PromosEnabled = ConfigValue FROM [dbo].[AppConfig] WHERE [Name] = 'DisallowCoupons';
	IF @PromosEnabled = 'true' BEGIN
		UPDATE [dbo].[AppConfig] SET [ConfigValue] = 'false' WHERE [Name] = 'Promotions.Enabled';
	END
END
GO

UPDATE [dbo].[AppConfig] SET [Description] = '*DEPRECATED* - Use AppConfigs GiftCards.Enabled or Promotions.Enabled to selectively enable the features DisallowCoupons was previously used for.'
	WHERE [Name] = 'DisallowCoupons';
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Express.AVSRequireConfirmedAddress') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Express.AVSRequireConfirmedAddress','GATEWAY','Require Confirmed Address. This is used to qualify for Seller Protection.  If set to true, shoppers who do not have an AVS Confirmed shipping address set in their PayPal account will not be able to check out with PayPal Express.','false', 'boolean');
END
GO

-- PayPal Banner Ads (Bill Me Later)
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.BannerEnrollmentService.SandboxURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.BannerEnrollmentService.SandboxURL','GATEWAY','Sandbox URL for Banner Enrollment Service.','https://api.financing-mint.paypal.com/finapi/v1/publishers/');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.BannerEnrollmentService.LiveURL') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.BannerEnrollmentService.LiveURL','GATEWAY','Live URL for Banner Enrollment Service.','https://api.financing.paypal.com/finapi/v1/publishers/');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.TermsAndConditionsAgreement') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.TermsAndConditionsAgreement','GATEWAY','I agree to the <a href="https://financing.paypal.com/ppfinportal/content/operatingAgmt" target="_blank">terms and conditions</a> for PayPal Banners.','false','boolean');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.PublisherId') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) values(1,'PayPal.Ads.PublisherId','GATEWAY','Your PayPal Publisher Id from the PayPal Media Network (PMN)','');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnProductPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.ShowOnProductPage','GATEWAY','Show the bill me later ads on your product page.','false','boolean');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ProductPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType,AllowableValues) values(1,'PayPal.Ads.ProductPageDimensions','GATEWAY','PayPal ad dimensions for the product page.','120x90','enum',
	'120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnHomePage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.ShowOnHomePage','GATEWAY','Show the bill me later ads on your home page.','false','boolean');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.HomePageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType,AllowableValues) values(1,'PayPal.Ads.HomePageDimensions','GATEWAY','PayPal ad dimensions for the home page.','120x90','enum',
	'120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnCartPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.ShowOnCartPage','GATEWAY','Show the bill me later ads on your Shopping Cart page.','false','boolean');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.CartPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType,AllowableValues) values(1,'PayPal.Ads.CartPageDimensions','GATEWAY','PayPal ad dimensions for the product page.','120x90','enum',
	'120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.ShowOnEntityPage') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) values(1,'PayPal.Ads.ShowOnEntityPage','GATEWAY','Show the bill me later ads on your entity pages.','false','boolean');
END
GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'PayPal.Ads.EntityPageDimensions') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType,AllowableValues) values(1,'PayPal.Ads.EntityPageDimensions','GATEWAY','PayPal ad dimensions for the entity pages.','120x90','enum',
	'120x90,150x100,170x100,190x100,234x60,120x240,250x250,468x60,728x90,800x66,120x600,234x400,280x280,300x250,336x280,540x200');
END
GO

GO
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Vortx.OnePageCheckout.AllowRTShipping.NoMethodSelected') BEGIN
INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue,ValueType) 
values(0,'Vortx.OnePageCheckout.AllowRTShipping.NoMethodSelected','CHECKOUT','This allows $0 shipping. Allows customer checkout when real time shipping returns zero methods, such as when order weight exceeds UPS, USPS, or FEDEX weight limits.','false','boolean');
END
GO


--Update the mobile user agent list to remove tosh so desktop Macs stop triggering the mobile skin
UPDATE [dbo].AppConfig SET ConfigValue = REPLACE(ConfigValue, ',tosh,', ',') WHERE Name = 'Mobile.ShortUserAgentList'
GO

--Remove Google Checkout as a payment option
DELETE FROM [dbo].AppConfig WHERE Name IN ('GoogleCheckout.ShowOnCartPage','GoogleCheckout.UseSandbox','GoogleCheckout.DefaultShippingMarkup','GoogleCheckout.MerchantId','GoogleCheckout.MerchantKey','GoogleCheckout.SandboxMerchantId','GoogleCheckout.SandboxMerchantKey','GoogleCheckout.LogMessages','GoogleCheckout.LogFileName','GoogleCheckout.BaseUrl','GoogleCheckout.DiagnosticsOnly','GoogleCheckout.DefaultTaxRate','GoogleCheckout.UseTaxTables','GoogleCheckout.ShippingIsTaxed','GoogleCheckout.SandBoxCheckoutButton','GoogleCheckout.LiveCheckoutButton','GoogleCheckout.SandBoxCheckoutURL','GoogleCheckout.AllowAnonCheckout','GoogleCheckout.DefaultDomesticShipToCity','GoogleCheckout.DefaultDomesticShipToState','GoogleCheckout.DefaultDomesticShipToZip','GoogleCheckout.DefaultDomesticShipToCountry','GoogleCheckout.DefaultInternationalShipToCity','GoogleCheckout.DefaultInternationalShipToState','GoogleCheckout.DefaultInternationalShipToZip','GoogleCheckout.DefaultInternationalShipToCountry','GoogleCheckout.CarrierCalculatedShippingEnabled','GoogleCheckout.CarrierCalculatedPackage','GoogleCheckout.CarrierCalculatedShippingOptions','GoogleCheckout.CarrierCalculatedDefaultPrice','GoogleCheckout.CarrierCalculatedFreeOption','GoogleCheckout.ConversionURL','GoogleCheckout.ConversionParameters','GoogleCheckout.SendStoreReceipt','GoogleCheckout.AuthenticateCallback','Mobile.GoogleCheckout.LiveCheckoutButton')
GO


IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'XmlPackage.SearchPage') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) 
	values(0,'XmlPackage.SearchPage','XMLPACKAGE','The XmlPackage used to display search results on the search page. "page.search.xml.config" is the default value.','page.search.xml.config');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'XmlPackage.SearchAdvPage') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) 
	values(0,'XmlPackage.SearchAdvPage','XMLPACKAGE','The XmlPackage used to display search results on the advanced search page. "page.searchadv.xml.config" is the default value.','page.searchadv.xml.config');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Search_NumberOfColumns') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) 
	values(0,'Search_NumberOfColumns','SITEDISPLAY','The number of columns on the search page grid. 4 is the default value.','4');
END
GO

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'FeaturedProductsNumberOfColumns') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ConfigValue) 
	values(0,'FeaturedProductsNumberOfColumns','SITEDISPLAY','The number of columns on the home page featured items. 4 is the default value.','4');
END
GO

update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'HidePicsInTableCondensed'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ShowFullNameInTableExpanded'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ShowDescriptionInTableCondensed'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ShowDimensionsInTableCondensed'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ShowWeightInTableCondensed'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'RelatedProductsFormat'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'RecentlyViewedProducts.ProductsFormat'
update [dbo].[AppConfig] set [Description] = 'DEPRECATED - this has AppConfig has been left for backwards compatibility, but is no longer being used and will be removed in a future release.' where [Name] = 'ResizeSlideWindow.ProductsFormat'

/*********** End 9.4.1.0 Changes ***********************/



/*********** Begin 9.4.2.0 Changes *********************/

--Update a confusing AppConfig description
if exists (select * from AppConfig where Name = 'QuantityDiscount.CombineQuantityByProduct')
begin
	UPDATE AppConfig SET Description = 'If this is false then quantity discounts will be calculated per line item. If true, all line items with the same product ID will be factored into quantity discount calculations, even if they have different variant, size, or color options.'
	WHERE Name = 'QuantityDiscount.CombineQuantityByProduct'
end
go

--Update monthly maintenance to handle CIM profiles from abandoned carts
ALTER proc [dbo].[aspdnsf_MonthlyMaintenance]
-- BACKUP YOUR DB BEFORE USING THIS SCRIPT!
    @InvalidateCustomerCookies      tinyint = 1,
    @PurgeAnonCustomers             tinyint = 1,
    @CleanShoppingCartsOlderThan    smallint = 30,  -- set to 0 to disable erasing
    @CleanWishListsOlderThan        smallint = 30,  -- set to 0 to disable erasing
    @CleanGiftRegistriesOlderThan   smallint = 30,  -- set to 0 to disable erasing
    @EraseCCFromAddresses           tinyint = 1,    -- except those used for recurring billing items!
    @EraseSQLLogOlderThan           smallint = 30,  -- set to 0 to disable erasing
    @ClearProductViewsOrderThan     smallint = 180, 
    @EraseCCFromOrdersOlderThan     smallint = 30,  -- set to 0 to disable erasing
    @DefragIndexes                  tinyint = 0,
    @PurgeDeletedRecords            tinyint = 0,     -- Purges records in all tables with a deleted flag set to 1
	@RemoveRTShippingDataOlderThan  smallint = 30,    -- set to 0 to disable erasing
	@ClearSearchLogOlderThan		smallint = 30    -- set to 0 to disable erasing
AS
BEGIN
    set nocount on 
    DECLARE @cmd varchar(8000)

    -- clear out old stuff in failed transactions:
    delete from failedtransaction where orderdate < dateadd(mm,-2,getdate());

    -- clear out old tx info, not longer needed:
    update orders set TransactionCommand=NULL, AuthorizationResult=NULL, VoidTXCommand=NULL, VoidTXResult=NULL, CaptureTXCommand=NULL, CaptureTXResult=NULL, RefundTXCommand=NULL, RefundTXResult=NULL where orderdate < dateadd(mm,-2,getdate());

    -- invalidate all user cookies (forces re-login of all customers, for security safety):
    IF @InvalidateCustomerCookies = 1
    BEGIN
        update [dbo].customer set CustomerGUID=newid();
    END

    -- clean out RefundTXCommand, not needed anymore:
    update orders set RefundTXCommand=NULL;

    -- clean up all carts (don't touch recurring items, gift registry items, or wishlist items however):
    IF @CleanShoppingCartsOlderThan <> 0
    BEGIN 
        delete dbo.kitcart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
        delete dbo.ShoppingCart where (CartType=0 or CartType=101) and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
    END

    IF @CleanWishListsOlderThan <> 0
    BEGIN
        delete dbo.kitcart where CartType=1 and CreatedOn < dateadd(d,-@CleanWishListsOlderThan,getdate());
        delete dbo.ShoppingCart where CartType=1 and CreatedOn < dateadd(d,-@CleanWishListsOlderThan,getdate());
    END

    IF @CleanGiftRegistriesOlderThan = 1
    BEGIN
        delete dbo.kitcart where CartType=3 and CreatedOn < dateadd(d,-@CleanGiftRegistriesOlderThan,getdate());
        delete dbo.ShoppingCart where CartType=3 and CreatedOn < dateadd(d,-@CleanGiftRegistriesOlderThan,getdate());
    END

    -- purge anon customers:
    IF @PurgeAnonCustomers = 1
    BEGIN
		-- clean out CIM profiles for orders that were not completed
		delete dbo.CIM_AddressPaymentProfileMap where customerid not in (select customerid from dbo.customer with (NOLOCK) where IsRegistered=1)
		
        delete dbo.customer 
        where IsRegistered=0 and IsAdmin = 0
            and customerid not in (select customerid from dbo.ShoppingCart with (NOLOCK)) 
            and customerid not in (select customerid from dbo.kitcart with (NOLOCK)) 
            and customerid not in (select customerid from dbo.orders with (NOLOCK)) 
            and customerid not in (select customerid from dbo.rating with (NOLOCK)) 
            and customerid not in (select ratingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
            and customerid not in (select votingcustomerid from dbo.ratingcommenthelpfulness with (NOLOCK)) 
            and customerid not in (select customerid from dbo.pollvotingrecord with (NOLOCK))
            and customerid not in (select customerid from dbo.CIM_AddressPaymentProfileMap with (NOLOCK))
    END
	
    -- clean any orphaned addresses
    delete dbo.Address where CustomerID Not in (select CustomerID from dbo.customer with (NOLOCK))

    -- clean addresses, except for those that have recurring orders
    IF @EraseCCFromAddresses = 1
    BEGIN
        IF exists (select * from dbo.sysobjects with (nolock) where type = 'u' and name = 'address')
            IF exists (select * from dbo.syscolumns with (nolock) where id = object_id('address') and name = 'CardExtraCode')
            BEGIN
                SET @cmd = 'update [dbo].address set CardNumber=''1111111111111111'', CardStartDate=''11/11'', CardIssueNumber=''111111111'', CardExtraCode=''1111'',CardExpirationMonth=''11'', CardExpirationYear=''1111'', eCheckBankABACode=''11111111'', eCheckBankAccountNumber=''11111111'' where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].address set CardNumber=NULL, CardStartDate=NULL, CardIssueNumber=NULL, CardExtraCode=NULL,CardExpirationMonth=NULL, CardExpirationYear=NULL, eCheckBankABACode=NULL, eCheckBankAccountNumber=NULL where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
            END 
            ELSE
            BEGIN
                SET @cmd = 'update [dbo].address set CardNumber=''1111111111111111'', CardStartDate=''11/11'', CardIssueNumber=''11111111'', CardExpirationMonth=''11'', CardExpirationYear=''1111'', eCheckBankABACode=''11111111'', eCheckBankAccountNumber=''11111111'' where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].address set CardNumber=NULL, CardStartDate=NULL, CardIssueNumber=NULL, CardExpirationMonth=NULL, CardExpirationYear=NULL, eCheckBankABACode=NULL, eCheckBankAccountNumber=NULL where CustomerID not in (select CustomerID from ShoppingCart where CartType=2)'
                EXEC (@cmd)
                SET @cmd = ''
            END 
        ELSE
        BEGIN
            -- erase credit card info from all customer records (recurring orders were not supported in these old versions)
            IF exists (select * From sysobjects so with (nolock) join [dbo].syscolumns sc on so.id = sc.id where so.type= 'u' and so.name = 'customer' and sc.name = 'CardNumber') BEGIN
                SET @cmd = 'update [dbo].Customer SET CardNumber = ''1111111111111111'''
                EXEC (@cmd)
                SET @cmd = ''
                SET @cmd = 'update [dbo].Customer SET CardNumber = null'
                EXEC (@cmd)
                SET @cmd = ''
            END 
        END
    END

    -- erase credit cards from all orders older than N days:
    IF @EraseCCFromOrdersOlderThan <> 0
    BEGIN
        update [dbo].orders set CardNumber=NULL, eCheckBankABACode=NULL,eCheckBankAccountNumber=NULL WHERE OrderDate < dateadd(d,-@EraseCCFromOrdersOlderThan,getdate())
        IF exists (select * From dbo.sysobjects so with (nolock) join [dbo].syscolumns sc on so.id = sc.id where so.type= 'u' and so.name = 'orders' and sc.name = 'CardExtraCode') BEGIN
            SET @cmd = 'update [dbo].Orders SET CardExtraCode = ''1111'''
            EXEC (@cmd)
            SET @cmd = ''
            SET @cmd = 'update [dbo].Orders SET CardExtraCode = null'
            EXEC (@cmd)
            SET @cmd = ''
        END 
    END

    -- erase logged sql statements
    IF @EraseSQLLogOlderThan <> 0
    BEGIN
        DELETE dbo.SQLLog WHERE ExecutedOn < dateadd(d,-@EraseSQLLogOlderThan,getdate())
    END
    
    -- erase product views both for recently and dynamic
    IF @ClearProductViewsOrderThan <> 0
    BEGIN 
		DELETE dbo.ProductView WHERE ViewDate < dateadd(d,-@ClearProductViewsOrderThan,getdate())
	END

    truncate table CustomerSession

    exec aspdnsf_CustomerConsistencyCheck


    -- nuke all products that were used as temporary products for checkout (auction) purposes
    declare @productid int
    select productid into #tmpproduct from dbo.product with (nolock) where deleted = 2
    select top 1 @productid = productid from #tmpproduct 
    while @@rowcount > 0 begin
        exec dbo.aspdnsf_NukeProduct @productid
        delete #tmpproduct where productid = @productid
        select top 1 @productid = productid from #tmpproduct 
    end
    drop table #tmpproduct 

    IF @PurgeDeletedRecords = 1 BEGIN
        DELETE dbo.Address where deleted = 1
        DELETE dbo.Affiliate where deleted = 1
        DELETE dbo.Category where deleted = 1
        DELETE dbo.Coupon where deleted = 1
        DELETE dbo.Customer where deleted = 1
        DELETE dbo.CustomerLevel where deleted = 1
        DELETE dbo.Distributor where deleted = 1
        DELETE dbo.Document where deleted = 1
        DELETE dbo.FAQ where deleted = 1
        DELETE dbo.Gallery where deleted = 1
        DELETE dbo.Genre where deleted = 1
        DELETE dbo.Library where deleted = 1
        DELETE dbo.Manufacturer where deleted = 1
        DELETE dbo.News where deleted = 1
        DELETE dbo.Partner where deleted = 1
        DELETE dbo.Poll where deleted = 1
        DELETE dbo.PollAnswer where deleted = 1 or not exists (select * from dbo.Poll where PollID = PollAnswer.PollID)
        DELETE dbo.Product where deleted = 1
        DELETE dbo.ProductVariant where deleted = 1 or not exists (select * from dbo.Product where productid = ProductVariant.productid)
        DELETE dbo.SalesPrompt where deleted = 1
        DELETE dbo.Section where deleted = 1
        DELETE dbo.ShippingZone where deleted = 1
        DELETE dbo.Staff where deleted = 1
        DELETE dbo.Topic where deleted = 1
        DELETE dbo.Vector where deleted = 1

        DELETE dbo.ProductVector where not exists (select * from dbo.product where productid = ProductVector.productid) or not exists (select * from dbo.vector where vectorid = ProductVector.vectorid) 
        DELETE dbo.ProductAffiliate where not exists (select * from dbo.product where productid = ProductAffiliate.productid) or not exists (select * from dbo.Affiliate where Affiliateid = ProductAffiliate.Affiliateid) 
        DELETE dbo.ProductCategory where not exists (select * from dbo.product where productid = ProductCategory.productid) or not exists (select * from dbo.Category where Categoryid = ProductCategory.Categoryid) 
        DELETE dbo.ProductCustomerLevel where not exists (select * from dbo.product where productid = ProductCustomerLevel.productid) or not exists (select * from dbo.CustomerLevel where CustomerLevelid = ProductCustomerLevel.CustomerLevelid) 
        DELETE dbo.ProductDistributor where not exists (select * from dbo.product where productid = ProductDistributor.productid) or not exists (select * from dbo.Distributor where Distributorid = ProductDistributor.Distributorid) 
        DELETE dbo.ProductGenre where not exists (select * from dbo.product where productid = ProductGenre.productid) or not exists (select * from dbo.Genre where Genreid = ProductGenre.Genreid) 
        DELETE dbo.ProductLocaleSetting where not exists (select * from dbo.product where productid = ProductLocaleSetting.productid) or not exists (select * from dbo.LocaleSetting where LocaleSettingid = ProductLocaleSetting.LocaleSettingid) 
        DELETE dbo.ProductManufacturer where not exists (select * from dbo.product where productid = ProductManufacturer.productid) or not exists (select * from dbo.Manufacturer where Manufacturerid = ProductManufacturer.Manufacturerid) 
        DELETE dbo.ProductSection where not exists (select * from dbo.product where productid = ProductSection.productid) or not exists (select * from dbo.Section where Sectionid = ProductSection.Sectionid) 
        DELETE dbo.Address where not exists (select * from dbo.Customer where customerid = Address.customerid)
    END
    
    -- Remove old RTShipping requests and responses
    IF @RemoveRTShippingDataOlderThan <> 0
    BEGIN
		UPDATE dbo.Customer SET RTShipRequest = '', RTShipResponse = ''
		WHERE CreatedOn < dateadd(d,-@RemoveRTShippingDataOlderThan,getdate())
		
		UPDATE dbo.Orders SET RTShipRequest = '', RTShipResponse = ''
		WHERE CreatedOn < dateadd(d,-@RemoveRTShippingDataOlderThan,getdate())
    END
    
    -- Search log
    IF @ClearSearchLogOlderThan <> 0
	BEGIN
		DELETE FROM dbo.SearchLog WHERE CreatedOn < dateadd(d,-@ClearSearchLogOlderThan,getdate())
	END

    -- Defrag indexes
    IF @DefragIndexes = 1
    BEGIN
        CREATE TABLE #SHOWCONTIG (
           tblname VARCHAR (255),
           ObjectId INT,
           IndexName VARCHAR (255),
           IndexId INT,
           Lvl INT,
           CountPages INT,
           CountRows INT,
           MinRecSize INT,
           MaxRecSize INT,
           AvgRecSize INT,
           ForRecCount INT,
           Extents INT,
           ExtentSwitches INT,
           AvgFreeBytes INT,
           AvgPageDensity INT,
           ScanDensity DECIMAL,
           BestCount INT,
           ActualCount INT,
           LogicalFrag DECIMAL,
           ExtentFrag DECIMAL)

        SELECT [name] tblname into #tmp FROM dbo.sysobjects with (nolock) WHERE type = 'u' ORDER BY Name

        DECLARE @tblname varchar(255), @indexname varchar(255)
        SELECT top 1 @tblname = tblname FROM #tmp
        WHILE @@rowcount > 0 BEGIN
            SET @cmd = 'DBCC SHOWCONTIG (''' + @tblname + ''') with tableresults, ALL_INDEXES'
            INSERT #SHOWCONTIG
            EXEC (@cmd)
            DELETE #tmp WHERE tblname = @tblname 
            SELECT top 1 @tblname = tblname FROM #tmp
        END


        DELETE #SHOWCONTIG WHERE LogicalFrag < 5 or Extents = 1 or IndexId in (0, 255)


        SELECT top 1 @tblname = tblname, @indexname = IndexName FROM #SHOWCONTIG ORDER BY IndexId
        WHILE @@rowcount > 0 BEGIN
            SET @cmd = 'DBCC DBREINDEX (''' + @tblname + ''', ''' + @indexname + ''', 90)  '
            EXEC (@cmd)
            DELETE #SHOWCONTIG WHERE tblname = @tblname 
            SELECT top 1 @tblname = tblname, @indexname = IndexName FROM #SHOWCONTIG ORDER BY IndexId
        END
    END
END
GO

-- New MaxCartItemsBeforeCheckout appconfig
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'MaxCartItemsBeforeCheckout') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue) 
	values(0,'MaxCartItemsBeforeCheckout','GATEWAY','Maximum # of line items a user may have in their cart before they can checkout.  Quantities do not matter, this looks at the number of separate items in the cart.','integer','300');
END
GO

-- New appconfig for re-ordering
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Reorder.Enabled') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue) 
	values(0,'Reorder.Enabled','MISC','If enabled, customers will see a grid of previous orders on the account.aspx page, each with a link to place the same order again.','boolean','true');
END
GO

-- New appconfig for google trusted stores
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'GoogleTrustedStoreProductIdentifierFormat') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue) 
	values(0,'GoogleTrustedStoreProductIdentifierFormat','Google Trusted Store','This string allows you to specify the format of your google product identifiers. Valid tokens are {ProductID}, {VariantID}, and {FullSKU}. These tokens are case sensitive.','string','{ProductID}-{VariantID}--');
END
GO

-- New appconfig for google trusted stores
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'GoogleTrustedStoreProductIdentifierEnabled') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue) 
	values(0,'GoogleTrustedStoreProductIdentifierEnabled','Google Trusted Store','This will add your product identifier to the google trusted stores script on product pages. Make sure to setup the appconfig titled GoogleTrustedStoreProductIdentifierFormat when you enable this feature.','boolean','false');
END
GO

-- New appconfig for google analytics
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Google.AnalyticsDisplayAdvertising.Enabled') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue) 
	values(0,'Google.AnalyticsDisplayAdvertising.Enabled','MISC','This will enable Google display advertising features within your Google Analytics tag. You may need to update your privacy policy if you turn this feature on. See Google''s display advertising policy requirements for more inforamtion.','boolean','false');
END
GO

--deprecate microstyle appconfig
if exists(select name from appconfig where name = 'MicroStyle')
begin 
	update appconfig set description = 'DEPRECATED - Attributes used for MultiMakesMicros. The cols colspacing, and rowspacing attributes are used to determine how many images can appear in each row and how much space (in pixels) is between each image while the width and height determine the resized micro height.'
	where name = 'MicroStyle'
end
go

if exists(select * from appconfig where name = 'MultiMakesMicros')
begin 
	update appconfig set description = 'If true this will create micro images resized by the width and height specified in DefaultWidth_micro and DefaultHeight_micro  and will save them in the images/product/micro folder whenever you are uploading multiple images in the medium multi-image manager.  If a product has multi-images and UseImagesForMultiNav is true then images will be shown instead of the number icons.' 
	where name = 'MultiMakesMicros'
end
go

-- add display name to the shipping methods table
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ShippingMethod') AND name = 'DisplayName')
    ALTER TABLE dbo.ShippingMethod ADD DisplayName nvarchar(400) NULL
GO

-- MicroPay cleanup
update [dbo].[AppConfig] set [Description] = 'If this is true and MicroPay is enabled as a payment method, the user''s current Micropay Balance will be shown at the top of the shopping cart page.' where [Name] = 'Micropay.ShowTotalOnTopOfCartPage'
update [dbo].[AppConfig] set [Description] = 'If this is true and MicroPay is enabled as a payment method, the "add $5 to your micropay" Product line item will NOT appear on the shopping cart page. This is helpful if the store administrator is controlling their micropay balance using some other means.' where [Name] = 'Micropay.HideOnCartPage'
DELETE FROM [dbo].[AppConfig] WHERE [Name] = 'MicroPay.Enabled'

-- Local pickup AppConfig description clarification
update [dbo].[AppConfig] set [Description] = 'State restrictions for the store-pickup option if the restriction type is state.  This should be a comma-separated list of the 2-character state abbreviations found on the Taxes->Edit State/Provinces page.' where [Name] = 'RTShipping.LocalPickupRestrictionStates'
update [dbo].[AppConfig] set [Description] = 'Zip Code restrictions for the store-pickup option if the restriction type is zip.  This should be a comma-separated list of 5-digit zip codes.' where [Name] = 'RTShipping.LocalPickupRestrictionZips'
update [dbo].[AppConfig] set [Description] = 'Zone restrictions for the store-pickup option if the restriction type is zone.  This should be a list of zone IDs from the Shipping->Shipping Zones Page.' where [Name] = 'RTShipping.LocalPickupRestrictionZones'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.aspdnsf_GetCustomersRelatedProducts') AND type IN ( N'P', N'PC' ) ) 
	begin
	 DROP PROCEDURE dbo.aspdnsf_GetCustomersRelatedProducts;
	end
GO
-- Added info for Schema.org attributes
CREATE PROCEDURE [dbo].[aspdnsf_GetCustomersRelatedProducts]
	@CustomerViewID		NVARCHAR(50),
	@ProductID			INT,
	@CustomerLevelID	INT,
	@InvFilter			INT,
	@affiliateID		INT,
	@storeID			INT = 1,
	@filterProduct		BIT = 0
	  
AS
SET NOCOUNT ON 

DECLARE 
	   @custlevelcount INT, 
	   @CustomerLevelFilteringIsAscending BIT, 
	   @FilterProductsByCustomerLevel TINYINT, 
	   @relatedprods VARCHAR(8000),
	   @DynamicProductsEnabled VARCHAR(10),
	   @ProductsDisplayed INT,
	   @FilterProductsByAffiliate TINYINT,
	   @affiliatecount INT,
	   @AffiliateExists INT

SELECT @custlevelcount = si.rows FROM dbo.sysobjects so WITH (NOLOCK) JOIN dbo.sysindexes si WITH (NOLOCK) ON so.id = si.id WHERE so.id = OBJECT_ID('ProductCustomerLevel') and si.indid < 2 AND type = 'u'
SELECT @FilterProductsByCustomerLevel = CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE [Name] = 'FilterProductsByCustomerLevel'
SELECT @FilterProductsByAffiliate = CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE [Name] = 'FilterProductsByAffiliate'
SELECT @affiliatecount = COUNT(*),  @AffiliateExists = SUM(CASE WHEN AffiliateID = @affiliateID THEN 1 ELSE 0 END) FROM dbo.ProductAffiliate WITH (NOLOCK) WHERE ProductID = @ProductID

   SET @CustomerLevelFilteringIsAscending  = 0
SELECT @CustomerLevelFilteringIsAscending  = CASE configvalue WHEN 'true' THEN 1 ELSE 0 END
  FROM dbo.appConfig WITH (NOLOCK)
 WHERE name like 'FilterByCustomerLevelIsAscending'	

SELECT @ProductsDisplayed = CAST(ConfigValue AS INT) from AppConfig with (NOLOCK) where Name = 'RelatedProducts.NumberDisplayed'
SELECT @DynamicProductsEnabled = CASE ConfigValue WHEN 'true' then 1 else 0 end from AppConfig with (NOLOCK) where Name = 'DynamicRelatedProducts.Enabled'
select @relatedprods = replace(cast(relatedproducts as varchar(8000)), ' ', '') from dbo.product with (NOLOCK) where productid = @productid

IF(@DynamicProductsEnabled = 1 and @ProductsDisplayed > 0)
BEGIN
	SELECT TOP (@ProductsDisplayed) 
	           tp.ProductID
			 , tp.ProductGUID
			 , tp.ImageFilenameOverride
			 , tp.SKU
			 , ISNULL(PRODUCTVARIANT.SkuSuffix, '') AS SkuSuffix
			 , ISNULL(PRODUCTVARIANT.ManufacturerPartNumber, '') AS VariantManufacturerPartNumber
			 , ISNULL(tp.ManufacturerPartNumber, '') AS ManufacturerPartNumber
		     , ISNULL(PRODUCTVARIANT.Dimensions, '') AS Dimensions
			 , PRODUCTVARIANT.Weight
			 , ISNULL(PRODUCTVARIANT.GTIN, '') AS GTIN
			 , PRODUCTVARIANT.VariantID
			 , PRODUCTVARIANT.Condition
			 , tp.SEAltText
			 , tp.Name
			 , tp.Description
			 , ProductManufacturer.ManufacturerID AS ProductManufacturerId
			 , Manufacturer.Name AS ProductManufacturerName
			 , Manufacturer.SEName AS ProductManufacturerSEName 
		FROM Product tp WITH (NOLOCK) 
		JOIN (SELECT p.ProductID
				   , p.ProductGUID
				   , p.ImageFilenameOverride
				   , p.SKU
				   , p.SEAltText
				   , p.Name
				   , p.Description  
				FROM dbo.product p WITH (NOLOCK) 
				JOIN dbo.split(@relatedprods, ',') rp ON p.productid = CAST(rp.items AS INT) 
		   LEFT JOIN dbo.productcustomerlevel pcl WITH (NOLOCK) ON p.productid = pcl.productid and @FilterProductsByCustomerLevel = 1
				JOIN (SELECT p.ProductID
						FROM dbo.product p WITH (NOLOCK)
						JOIN dbo.split(@relatedprods, ',') rp on p.productid = CAST(rp.items AS INT) 
						JOIN (SELECT ProductID
								   , SUM(Inventory) Inventory 
								FROM dbo.productvariant WITH (NOLOCK) GROUP BY ProductID) pv ON p.ProductID = pv.ProductID
						   LEFT JOIN (SELECT ProductID
										   , SUM(quan) inventory 
										FROM dbo.inventory i1 WITH (NOLOCK) 
										JOIN dbo.productvariant pv1 WITH (NOLOCK) ON pv1.variantid = i1.variantid 
										JOIN dbo.split(@relatedprods, ',') rp1 ON pv1.productid = CAST(rp1.items AS INT) GROUP BY pv1.productid) i ON i.productid = p.productid
								  WHERE CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.inventory, 0) ELSE pv.inventory END >= @InvFilter
					 ) tp on p.productid = tp.productid
			   WHERE published = 1 
				 AND deleted = 0 
				 AND p.productid != @productid
				 AND GETDATE() BETWEEN ISNULL(AvailableStartDate, '1/1/1900') AND ISNULL(AvailableStopDate, '1/1/2999')
				 AND CASE 
					 WHEN @FilterProductsByCustomerLevel = 0 THEN 1
					 WHEN @CustomerLevelFilteringIsAscending = 1 AND pcl.CustomerLevelID <= @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1 
					 WHEN @CustomerLevelID=0 AND pcl.CustomerLevelID IS NULL THEN 1
					 WHEN @CustomerLevelID IS NULL OR @custlevelcount = 0 THEN 1 
					 WHEN pcl.CustomerLevelID = @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1 
					 ELSE 0
					 END = 1
	UNION ALL	
	   SELECT pr.ProductID
			, pr.ProductGUID
			, pr.ImageFilenameOverride
			, pr.SKU
			, pr.SEAltText
			, pr.Name
			, pr.Description 
		 FROM Product pr WITH (NOLOCK)
		WHERE pr.ProductID IN (
		SELECT TOP 100 PERCENT p.ProductID 
		  FROM Product p WITH (NOLOCK) 
		  JOIN (SELECT ProductID 
				  FROM ProductView WITH (NOLOCK) WHERE CustomerViewID 
					IN (SELECT CustomerViewID 
						  FROM ProductView WITH (NOLOCK)
						 WHERE ProductID = @ProductID 
						   AND CustomerViewID <> @CustomerViewID		
					   )
				   AND ProductID <> @ProductID
				   AND ProductID NOT 
					IN (SELECT ProductID 
						  FROM product WITH (NOLOCK) 
						  JOIN split(@relatedprods, ',') rp ON productid = cast(rp.items AS INT) 
					  GROUP BY ProductID  		
					   )
				) a ON p.ProductID = a.ProductID	
	LEFT JOIN dbo.productcustomerlevel pcl WITH (NOLOCK) ON p.productid = pcl.productid and @FilterProductsByCustomerLevel = 1
	LEFT JOIN dbo.ProductAffiliate pa WITH (NOLOCK) ON p.ProductID = pa.ProductID 	    
		WHERE Published = 1 AND Deleted = 0
		 AND GETDATE() BETWEEN ISNULL(AvailableStartDate, '1/1/1900') AND ISNULL(AvailableStopDate, '1/1/2999')
		 AND CASE 
			 WHEN @FilterProductsByCustomerLevel = 0 THEN 1
			 WHEN @CustomerLevelFilteringIsAscending = 1 AND pcl.CustomerLevelID <= @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1 
			 WHEN @CustomerLevelID=0 AND pcl.CustomerLevelID IS NULL THEN 1
			 WHEN @CustomerLevelID IS NULL OR @custlevelcount = 0 THEN 1 
			 WHEN pcl.CustomerLevelID = @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1 
			 ELSE 0
			  END = 1 
		AND (pa.AffiliateID = @affiliateID OR pa.AffiliateID IS NULL OR @affiliatecount = 0 OR @FilterProductsByAffiliate = 0)		
	GROUP BY p.ProductID
	ORDER BY COUNT(*) DESC		
		)
	  )prd ON tp.ProductID = prd.ProductID
	 LEFT JOIN dbo.ProductManufacturer WITH (NOLOCK) ON tp.ProductID = ProductManufacturer.ProductID
	 LEFT JOIN dbo.Manufacturer WITH (NOLOCK) ON ProductManufacturer.ManufacturerID = Manufacturer.ManufacturerID
	      JOIN PRODUCTVARIANT WITH (NOLOCK) ON PRODUCTVARIANT.productid = CAST(tp.ProductID AS INT) AND PRODUCTVARIANT.isdefault = 1 AND PRODUCTVARIANT.Published = 1 AND PRODUCTVARIANT.Deleted = 0
	INNER JOIN (SELECT DISTINCT a.ProductID 
				  FROM Product a WITH (NOLOCK) 
			 LEFT JOIN ProductStore b WITH (NOLOCK) ON a.ProductID = b.ProductID 
				 WHERE (@filterProduct = 0 OR StoreID = @storeID)) ps ON tp.ProductID = ps.ProductID
END

IF(@DynamicProductsEnabled = 0 and @ProductsDisplayed > 0)
BEGIN 
	select TOP (@ProductsDisplayed) 
	           p.ProductID
			 , p.ProductGUID
			 , p.ImageFilenameOverride
			 , p.SKU
			 , ISNULL(PRODUCTVARIANT.SkuSuffix, '') AS SkuSuffix
			 , ISNULL(PRODUCTVARIANT.ManufacturerPartNumber, '') AS VariantManufacturerPartNumber
			 , ISNULL(p.ManufacturerPartNumber, '') AS ManufacturerPartNumber
		     , ISNULL(PRODUCTVARIANT.Dimensions, '') AS Dimensions
			 , PRODUCTVARIANT.Weight
			 , ISNULL(PRODUCTVARIANT.GTIN, '') AS GTIN
			 , PRODUCTVARIANT.VariantID
			 , PRODUCTVARIANT.Condition
			 , p.SEAltText
			 , p.Name
			 , p.Description
			 , ProductManufacturer.ManufacturerID AS ProductManufacturerId
			 , Manufacturer.Name AS ProductManufacturerName
			 , Manufacturer.SEName AS ProductManufacturerSEName   
		  FROM dbo.product p WITH (NOLOCK) 
	 LEFT JOIN dbo.ProductManufacturer WITH (NOLOCK) ON p.ProductID = ProductManufacturer.ProductID
	 LEFT JOIN dbo.Manufacturer WITH (NOLOCK) ON ProductManufacturer.ManufacturerID = Manufacturer.ManufacturerID
		  JOIN dbo.split(@relatedprods, ',') rp ON p.productid = CAST(rp.items AS INT) 
		  JOIN PRODUCTVARIANT WITH (NOLOCK) ON PRODUCTVARIANT.productid = CAST(rp.items AS INT) AND PRODUCTVARIANT.isdefault = 1 AND PRODUCTVARIANT.Published = 1 AND PRODUCTVARIANT.Deleted = 0
	 LEFT JOIN dbo.productcustomerlevel pcl WITH (NOLOCK) ON p.productid = pcl.productid AND @FilterProductsByCustomerLevel = 1
		  JOIN (SELECT p.ProductID
				  FROM dbo.product p WITH (NOLOCK)
				  JOIN dbo.split(@relatedprods, ',') rp on p.productid = cast(rp.items as int) 
				  JOIN (SELECT ProductID
							 , SUM(Inventory) Inventory 
						  FROM dbo.productvariant WITH (NOLOCK) 
					  GROUP BY ProductID) pv ON p.ProductID = pv.ProductID
					 LEFT JOIN (SELECT ProductID
									 , SUM(quan) inventory 
								  FROM dbo.inventory i1 WITH (NOLOCK) 
								  JOIN dbo.productvariant pv1 WITH (NOLOCK) ON pv1.variantid = i1.variantid 
								  JOIN dbo.split(@relatedprods, ',') rp1 ON pv1.productid = CAST(rp1.items AS INT) 
							  GROUP BY pv1.productid) i ON i.productid = p.productid
								 WHERE CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.inventory, 0) ELSE pv.inventory END >= @InvFilter
								) tp ON p.productid = tp.productid
					INNER JOIN (SELECT DISTINCT a.ProductID 
								  FROM Product a WITH (NOLOCK) 
							 LEFT JOIN ProductStore b WITH (NOLOCK) ON a.ProductID = b.ProductID 
								 WHERE (@filterProduct = 0 OR StoreID = @storeID)
								) ps ON p.ProductID = ps.ProductID		
						 WHERE p.published = 1 and p.deleted = 0 and p.productid != @productid
						   AND GETDATE() BETWEEN ISNULL(AvailableStartDate, '1/1/1900') AND ISNULL(AvailableStopDate, '1/1/2999')
						   AND CASE 
							   WHEN @FilterProductsByCustomerLevel = 0 THEN 1
							   WHEN @CustomerLevelFilteringIsAscending = 1 AND pcl.CustomerLevelID <= @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1 
							   WHEN @CustomerLevelID=0 AND pcl.CustomerLevelID IS NULL THEN 1
							   WHEN @CustomerLevelID IS NULL OR @custlevelcount = 0 THEN 1 
							   WHEN pcl.CustomerLevelID = @CustomerLevelID OR pcl.CustomerLevelID IS NULL THEN 1 
							   else 0
								end = 1
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.aspdnsf_GetUpsellProducts') AND type IN ( N'P', N'PC' ) ) 
	begin
	 DROP PROCEDURE dbo.aspdnsf_GetUpsellProducts;
	end
GO

-- Added info for Schema.org attributes
CREATE PROCEDURE [dbo].[aspdnsf_GetUpsellProducts]
	@productID			INT, 
	@customerlevelID	INT,
	@invFilter			INT,
	@storeID			INT = 1,
	@filterProduct		BIT = 0	

      
AS
BEGIN
	SET NOCOUNT ON;

   DECLARE @UpsellProducts VARCHAR(8000), 
		   @UpsellProductDiscPct MONEY

    SELECT @UpsellProducts = REPLACE(CAST(UpsellProducts AS VARCHAR(8000)), ' ', '')
	     , @UpsellProductDiscPct = UpsellProductDiscountPercentage 
	  FROM dbo.product WITH (NOLOCK) WHERE productid = @productid
	
	SELECT 1-(@UpsellProductDiscPct/100) UpsellDiscMultiplier
		 , p.*
		 , pv.VariantID
		 , pv.Price 
		 , ISNULL(pv.SalePrice, 0) SalePrice
		 , ISNULL(pv.SkuSuffix, '') AS SkuSuffix
		 , ISNULL(pv.ManufacturerPartNumber, '') AS VariantManufacturerPartNumber
		 , ISNULL(pv.Dimensions, '') AS Dimensions
		 , pv.Weight
		 , ISNULL(pv.GTIN, '') AS GTIN
		 , pv.Condition
		 , ISNULL(pv.Points, 0) Points
		 , sp.Name SalesPromptName
		 , ISNULL(ep.price, 0) ExtendedPrice
		 , ProductManufacturer.ManufacturerID AS ProductManufacturerId
		 , Manufacturer.Name AS ProductManufacturerName
		 , Manufacturer.SEName AS ProductManufacturerSEName 
      FROM dbo.product p WITH (NOLOCK) 
      JOIN dbo.split(@UpsellProducts, ',') up ON p.productid = CAST(up.items AS INT)
 LEFT JOIN dbo.SalesPrompt sp WITH (NOLOCK) ON sp.SalesPromptID = p.SalesPromptID
      JOIN dbo.productvariant pv WITH (NOLOCK) ON pv.productid = CAST(up.items AS INT) AND pv.isdefault = 1 AND pv.Published = 1 AND pv.Deleted = 0
 LEFT JOIN dbo.ExtendedPrice ep WITH (NOLOCK) ON ep.VariantID = pv.VariantID AND ep.CustomerLevelID = @CustomerLevelID
      JOIN (SELECT p.ProductID
              FROM dbo.product p WITH (NOLOCK)
              JOIN dbo.split(@UpsellProducts, ',') rp ON p.productid = CAST(rp.items AS INT) 
              JOIN (SELECT ProductID, SUM(Inventory) Inventory 
			          FROM dbo.productvariant WITH (NOLOCK) 
				  GROUP BY ProductID) pv ON p.ProductID = pv.ProductID
         LEFT JOIN (SELECT ProductID
				         , SUM(quan) inventory 
				      FROM dbo.inventory i1 WITH (NOLOCK) 
				      JOIN dbo.productvariant pv1 WITH (NOLOCK) ON pv1.variantid = i1.variantid
				      JOIN dbo.split(@UpsellProducts, ',') rp1 ON pv1.productid = CAST(rp1.items AS INT) 
			      GROUP BY pv1.productid) i ON i.productid = p.productid
                WHERE CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.inventory, 0) ELSE pv.inventory END >= @InvFilter
                   ) tp ON p.productid = tp.productid
		INNER JOIN (SELECT DISTINCT a.ProductID 
		              FROM Product a WITH (NOLOCK) 
				 LEFT JOIN ProductStore b WITH (NOLOCK) ON a.ProductID = b.ProductID WHERE (@filterProduct = 0 OR StoreID = @storeID)
				   ) ps ON p.ProductID = ps.ProductID
LEFT JOIN dbo.ProductManufacturer WITH (NOLOCK) ON tp.ProductID = ProductManufacturer.ProductID
LEFT JOIN dbo.Manufacturer WITH (NOLOCK) ON ProductManufacturer.ManufacturerID = Manufacturer.ManufacturerID
	WHERE p.Published = 1 
	  AND p.Deleted = 0 
	  AND p.IsCallToOrder = 0 
	  AND p.productid != @productid
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.aspdnsf_ProductInfo') AND type IN ( N'P', N'PC' ) ) 
	begin
	 DROP PROCEDURE dbo.aspdnsf_ProductInfo;
	end
GO

-- Added info for Schema.org attributes
CREATE proc [dbo].[aspdnsf_ProductInfo]
    @ProductID          INT,  
    @CustomerLevelID    INT,  
    @DefaultVariantOnly TINYINT,  
    @InvFilter          INT = 0,  
    @AffiliateID        INT = null,  
    @PublishedOnly      TINYINT = 1,
    @IsAdmin			TINYINT = 0,
    @StoreID			INT = 0
AS BEGIN
	SET NOCOUNT ON  
	DECLARE 
		@CustLevelExists INT, 
		@AffiliateExists INT, 
		@FilterProductsByAffiliate TINYINT, 
		@FilterProductsByCustomerLevel TINYINT, 
		@CustomerLevelFilteringIsAscending TINYINT,
		@CustomerLevelCount INT, 
		@AffiliateCount INT, 
		@MinProductCustomerLevel INT, 
		@HideProductsWithLessThanThisInventoryLevel INT  
		
		SELECT @FilterProductsByCustomerLevel		= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByCustomerLevel'		AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
		SELECT @CustomerLevelFilteringIsAscending	= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterByCustomerLevelIsAscending'	AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
		SELECT @FilterProductsByAffiliate			= CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'FilterProductsByAffiliate'			AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
		SELECT @HideProductsWithLessThanThisInventoryLevel	= CONVERT(INT, ConfigValue) FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'HideProductsWithLessThanThisInventoryLevel' AND ISNUMERIC(ConfigValue) = 1 AND (StoreID = @StoreID OR StoreID = 0) ORDER BY StoreID DESC
		SELECT @CustomerLevelCount = COUNT(*), @MinProductCustomerLevel = MIN(CustomerLevelID), @CustLevelExists = SUM(CASE WHEN CustomerLevelID = @CustomerLevelID THEN 1 ELSE 0 END) FROM dbo.ProductCustomerLevel WITH (NOLOCK) WHERE ProductID = @ProductID
		SELECT @AffiliateCount = COUNT(*), @AffiliateExists = SUM(CASE WHEN AffiliateID = @AffiliateID THEN 1 ELSE 0 END) FROM dbo.ProductAffiliate WITH (NOLOCK) WHERE ProductID = @ProductID   

		IF (@HideProductsWithLessThanThisInventoryLevel > @InvFilter or @HideProductsWithLessThanThisInventoryLevel = -1) and @InvFilter <> 0  
			SET @InvFilter = @HideProductsWithLessThanThisInventoryLevel   

		IF
		(
			(
				(
					@FilterProductsByCustomerLevel = 0 
					OR @CustomerLevelCount = 0 
					OR (
						@CustomerLevelFilteringIsAscending = 1 
						AND @MinProductCustomerLevel <= @CustomerLevelID) 
					OR @CustLevelExists > 0
				)  
				AND (
					@FilterProductsByAffiliate = 0 
					OR @AffiliateCount = 0 
					OR @AffiliateExists > 0)
			)
			OR @IsAdmin = 1
		)  
		SELECT   
			  p.*
			, pv.VariantID
			, pv.name VariantName
			, pv.Price
			, pv.Description VariantDescription
			, ISNULL(pv.SalePrice, 0) SalePrice
			, ISNULL(pv.SkuSuffix, '') AS SkuSuffix
			, ISNULL(pv.ManufacturerPartNumber, '') AS VariantManufacturerPartNumber
			, ISNULL(pv.Dimensions, '') AS Dimensions
			, pv.Weight
			, ISNULL(pv.GTIN, '') AS GTIN
			, pv.Condition			
			, ISNULL(pv.Points, 0) Points
			, pv.Inventory
			, pv.ImageFilenameOverride VariantImageFilenameOverride
			, pv.isdefault
			, pv.CustomerEntersPrice
			, ISNULL(pv.colors, '') Colors
			, ISNULL(pv.sizes, '') Sizes
			, sp.name SalesPromptName
			, CASE WHEN pcl.productid is null THEN 0 ELSE ISNULL(e.Price, 0) END ExtendedPrice
			, PRODUCTMANUFACTURER.ManufacturerID AS ProductManufacturerId
			, MANUFACTURER.Name AS ProductManufacturerName
			, MANUFACTURER.SEName AS ProductManufacturerSEName 
		 FROM dbo.Product p WITH (NOLOCK) 
		 JOIN dbo.productvariant pv WITH (NOLOCK) ON p.ProductID = pv.ProductID     
		 JOIN dbo.SalesPrompt sp WITH (NOLOCK) ON p.SalesPromptID = sp.SalesPromptID   
	LEFT JOIN dbo.ExtendedPrice e WITH (NOLOCK) ON pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID  
	LEFT JOIN dbo.ProductCustomerLevel pcl WITH (NOLOCK) ON p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID  
	LEFT JOIN (SELECT variantid, SUM(quan) inventory 
				 FROM inventory 
			 GROUP BY variantid) i on pv.variantid = i.variantid
	LEFT JOIN dbo.PRODUCTMANUFACTURER WITH (NOLOCK) ON p.ProductID = PRODUCTMANUFACTURER.ProductID
	LEFT JOIN dbo.MANUFACTURER WITH (NOLOCK) ON PRODUCTMANUFACTURER.ManufacturerID = MANUFACTURER.ManufacturerID
	    WHERE p.ProductID = @ProductID   
		  AND p.Deleted = 0   
		  AND pv.Deleted = 0   
		  AND p.Published >= @PublishedOnly  
		  AND pv.Published >= @PublishedOnly  
		  AND pv.IsDefault >= @DefaultVariantOnly  
		  AND GETDATE() between ISNULL(p.AvailableStartDate, '1/1/1900') and ISNULL(p.AvailableStopDate, '1/1/2999')  
		  AND (CASE p.TrackInventoryBySizeAndColor WHEN 1 THEN ISNULL(i.inventory, 0) ELSE pv.inventory END >= @InvFilter or @InvFilter = 0)  
     ORDER BY pv.DisplayOrder, pv.name
END
GO

-- New appconfig for dimension units
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Localization.DimensionUnits') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue) 
	values(0,'Localization.DimensionUnits','SETUP','Enter the prompt you want to use for Dimensions (e.g. inches (IN) or centimeters (CM))','string','IN');
END
GO

-- Create the CBA AppConfigs
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.CbaAccessKey') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.CbaAccessKey','GATEWAY','Checkout By Amazon Access Key','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.CbaSecretKey') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.CbaSecretKey','GATEWAY','Checkout By Amazon Secret Key','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MwsAccessKey') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.MwsAccessKey','GATEWAY','Amazon Marketplace Access Key','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MwsSecretKey') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.MwsSecretKey','GATEWAY','Amazon Marketplace Secret Key','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MerchantId') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.MerchantId','GATEWAY','Checkout By Amazon Merchant Id','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.Marketplace') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.Marketplace','GATEWAY','Checkout By Amazon Marketplace','string','',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.WidgetUrl') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.WidgetUrl','GATEWAY','The url used to render the widget scripts','string','https://static-na.payments-amazon.com/cba/js/us/PaymentWidgets.js',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.WidgetSandboxUrl') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.WidgetSandboxUrl','GATEWAY','The sandbox url used to render the widget scripts','string','https://static-na.payments-amazon.com/cba/js/us/sandbox/PaymentWidgets.js',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.CBAServiceUrl') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.CBAServiceUrl','GATEWAY','The url used to call the cba service','string','https://payments.amazon.com/cba/api/purchasecontract/',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.CBAServiceSandboxUrl') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.CBAServiceSandboxUrl','GATEWAY','The sandbox url used to call the cba service','string','https://payments-sandbox.amazon.com/cba/api/purchasecontract/',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MerchantServiceUrl') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.MerchantServiceUrl','GATEWAY','The url used to call the merchant service','string','https://mws.amazonservices.com',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.MerchantServiceSandboxUrl') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.MerchantServiceSandboxUrl','GATEWAY','The sandbox url used to call the merchant service','string','https://mws.amazonservices.com',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.UseSandbox') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'CheckoutByAmazon.UseSandbox','GATEWAY','Puts the Checkout By Amazon services in sandbox mode','boolean','true',0);
END
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'CheckoutByAmazon.OrderFulfillmentType') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,AllowableValues,StoreId) 
	values(0,'CheckoutByAmazon.OrderFulfillmentType','GATEWAY','Checkout By Amazon Order Fulfillment Type. Instant: Marks the order as shipped immediatly after getting the ready to ship notification. MarkedAsShipped - Notifies Amazon that the order has shipped when an admin marks the order as shipped. Never - Admins must manually adjust ship status at Amazon.','enum','Never','Instant,Never,MarkedAsShipped',0);
END
GO

-- New appconfig for displaying watermarks on icon sized product images
IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'Watermark.Icons.Enabled') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'Watermark.Icons.Enabled','MISC','When this is set to true and watermarks are enabled, icon sized product images will also contain watermarks.','boolean','true',0);
END
GO

-- Related products changes
UPDATE AppConfig SET Name = 'RelatedProducts.NumberDisplayed' WHERE Name = 'DynamicRelatedProducts.NumberDisplayed'

IF NOT EXISTS (SELECT * FROM [AppConfig] WHERE [Name] = 'RelatedProducts.NumberDisplayed') 
BEGIN
	INSERT [dbo].AppConfig(SuperOnly,Name,GroupName,Description,ValueType,ConfigValue,StoreId) 
	values(0,'RelatedProducts.NumberDisplayed','DISPLAY','Controls the maximum number of related products that will be displayed at the bottom of the Product pages.','integer','4',0);
END
GO

-- Update store version
PRINT 'Updating Store Version...'
UPDATE [dbo].[AppConfig] SET [ConfigValue] = '9.4.2.0' WHERE [Name] = 'StoreVersion'


print '*****Database Upgrade Completed*****'