-- ------------------------------------------------------------------------------------------
-- Copyright AspDotNetStorefront.com.  All Rights Reserved.
-- http://www.aspdotnetstorefront.com
-- For details on this license please visit our homepage at the URL above.
-- THE ABOVE NOTICE MUST REMAIN INTACT. 
--
-- Database Upgrade Script:
-- AspDotNetStorefront Version 8.0.1.2 to 9.0, Microsoft SQL Server 2005 Or higher
-- ------------------------------------------------------------------------------------------

/*********** ASPDOTNETSTOREFRONT v8.0.1.2 to v9.0 *****************/
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


/* ************************************************************** */
/* DROP ALL ASPDOTNETSTOREFRONT STORED PROCEDURES				  */
/* ************************************************************** */

DECLARE @sproc nvarchar(max);
DECLARE tmpcur CURSOR
	FOR SELECT [NAME]FROM sys.objects WHERE TYPE = 'p' AND [name] LIKE 'aspdnsf_%'

OPEN tmpcur

FETCH NEXT FROM tmpcur INTO @sproc
WHILE @@FETCH_STATUS = 0
BEGIN
	EXEC('DROP PROCEDURE ' + @sproc)
	FETCH NEXT FROM tmpcur INTO @sproc
END
		
CLOSE tmpcur
DEALLOCATE tmpcur
GO


/* ************************************************************** */
/* SYNCHRONIZE OBJECTS                                            */
/* ************************************************************** */

SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
GO
CREATE TABLE #tmpErrors (Error int)
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
PRINT N'Dropping constraints from [dbo].[RatingCommentHelpfulness]'
GO
ALTER TABLE [dbo].[RatingCommentHelpfulness] DROP CONSTRAINT [PK_Comment_Helpfulness]
GO


PRINT N'Dropping index [UIX_AppConfig_Name] from [dbo].[AppConfig]'
GO
DROP INDEX [UIX_AppConfig_Name] ON [dbo].[AppConfig]
GO


PRINT N'Dropping index [UIX_ZipTaxRate_ZipCode_TaxClassID] from [dbo].[ZipTaxRate]'
GO
DROP INDEX [UIX_ZipTaxRate_ZipCode_TaxClassID] ON [dbo].[ZipTaxRate]
GO


PRINT N'Creating [dbo].[aspdnsf_CreateIndexes]'
GO


create proc [dbo].[aspdnsf_CreateIndexes] 
  
as
BEGIN
CREATE UNIQUE INDEX [UIX_Address_AddressGUID] ON [Address] ([AddressGUID]);
CREATE INDEX [IX_Address_CustomerID] ON [Address]([CustomerID]) ;
CREATE UNIQUE INDEX [UIX_CustomerGiftRegistrySearches_CustomerID_GIftRegistryGUID] ON [CustomerGiftRegistrySearches] ([CustomerID],[GIftRegistryGUID]) ;
CREATE INDEX [IX_Address_Deleted] ON [Address]([Deleted]) ;
CREATE INDEX [IX_AffiliateActivity_AffiliateID] ON [AffiliateActivity]([AffiliateID]) ;
CREATE INDEX [IX_AffiliateActivity_AffiliateID_ActivityDate] ON [AffiliateActivity]([AffiliateID],[ActivityDate]) ;
CREATE INDEX [IX_AffiliateActivityReason_Name] ON [AffiliateActivityReason]([Name]) ;
CREATE INDEX [IX_AffiliateCommissions_RowGUID] ON [AffiliateCommissions]([RowGUID]) ;
CREATE INDEX [IX_AffiliateCommissions_LowValue] ON [AffiliateCommissions]([LowValue]) ;
CREATE INDEX [IX_AffiliateCommissions_HighValue] ON [AffiliateCommissions]([HighValue]) ;
CREATE UNIQUE INDEX [UIX_AppConfig_Name_StoreID] ON [AppConfig] ([Name], [StoreID]);
CREATE INDEX [IX_AppConfig_GroupName] ON [AppConfig]([GroupName]) ;
CREATE UNIQUE INDEX [UIX_StringResource_Name_LocaleSetting] ON [StringResource]([Name],[LocaleSetting]);
CREATE INDEX [IX_BadWord] ON [BadWord]([Word]) ;
CREATE INDEX [IX_Category_Name] ON [Category]([Name]) ;
CREATE INDEX [IX_Category_Deleted] ON [Category]([Deleted]) ;
CREATE INDEX [IX_Category_Published] ON [Category]([Published]) ;
CREATE INDEX [IX_Category_Wholesale] ON [Category]([Wholesale]) ;
CREATE INDEX [IX_Category_CategoryGUID] ON [Category]([CategoryGUID]) ;
CREATE INDEX [IX_Category_ParentCategoryID] ON [Category]([ParentCategoryID]) ;
CREATE INDEX [IX_Category_DisplayOrder] ON [Category]([DisplayOrder]) ;
CREATE INDEX [IX_Category_Deleted_Published] ON [Category]([Deleted],[Published]) ;
CREATE INDEX [IX_Category_Deleted_Wholesale] ON [Category]([Deleted],[Wholesale]) ;
CREATE INDEX [IX_ClickTrack_Name_CreatedOn] ON [ClickTrack]([Name],[CreatedOn]) ;
CREATE UNIQUE INDEX [UIX_Country_Name] ON [Country]([Name]);
CREATE INDEX [IX_Country_DisplayOrder_Name] ON [Country]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_Country_CountryGUID] ON [Country]([CountryGUID]) ;
CREATE INDEX [IX_Country_DisplayOrder] ON [Country]([DisplayOrder]) ;
CREATE UNIQUE INDEX [UIX_Coupon_CouponGUID] ON [Coupon]([CouponGUID]);
CREATE UNIQUE INDEX [UIX_Coupon_CouponCode] ON [Coupon]([CouponCode]);
CREATE INDEX [IX_Coupon_ExpirationDate] ON [Coupon]([ExpirationDate]) ;
CREATE INDEX [IX_Coupon_Deleted] ON [Coupon]([Deleted]) ;
CREATE INDEX [IX_CouponUsage_CustomerID] ON [CouponUsage]([CustomerID]) ;
CREATE INDEX [IX_CouponUsage_CouponCode] ON [CouponUsage]([CouponCode]) ;
CREATE INDEX [IX_CouponUsage_CreatedOn] ON [CouponUsage]([CreatedOn]) ;
CREATE UNIQUE INDEX [UIX_CreditCardTypes] ON [CreditCardType]([CardTypeGUID]);
CREATE INDEX [IX_CreditCardType_CardType] ON [CreditCardType]([CardType]) ;
CREATE UNIQUE INDEX [UIX_Currency_CurrencyGUID] ON [dbo].[Currency]([CurrencyGUID])
CREATE INDEX [IX_CustomCart_VariantID] ON [CustomCart]([VariantID]) ;
CREATE INDEX [IX_CustomCart_CustomerID_ShoppingCartRecID] ON [CustomCart]([CustomerID],[ShoppingCartRecID]) ;
CREATE INDEX [IX_CustomCart_CustomerID_ProductID_VariantID] ON [CustomCart]([CustomerID],[ProductID],[VariantID]) ;
CREATE INDEX [IX_CustomCart_ChosenColor] ON [CustomCart]([ChosenColor]) ;
CREATE INDEX [IX_CustomCart_ChosenSize] ON [CustomCart]([ChosenSize]) ;
CREATE INDEX [IX_CustomCart_ProductID] ON [CustomCart]([ProductID]) ;
CREATE INDEX [IX_CustomCart_CartType] ON [CustomCart]([CartType]) ;
CREATE INDEX [IX_CustomCart_CustomerID_CartType] ON [CustomCart]([CustomerID],[CartType]) ;
CREATE INDEX [IX_CustomCart_CustomerID] ON [CustomCart]([CustomerID]) ;
CREATE INDEX [IX_CustomCart_CustomerID_PackID] ON [CustomCart]([CustomerID],[PackID]) ;
CREATE INDEX [IX_CustomCart_CreatedOn] ON [CustomCart]([CreatedOn]) ;
CREATE UNIQUE INDEX [UIX_Customer_CustomerGUID] ON [Customer]([CustomerGUID]);
CREATE INDEX [IX_Customer_GiftRegistryGUID] ON [Customer]([GiftRegistryGUID]) ;
CREATE INDEX [IX_Customer_GiftRegistryNickName] ON [Customer]([GiftRegistryNickName]) ;
CREATE INDEX [IX_Customer_EMail] ON [Customer]([Email]) ;
CREATE INDEX [IX_Customer_Password] ON [Customer]([Password]) ;
CREATE INDEX [IX_Customer_CustomerLevelID] ON [Customer]([CustomerLevelID]) ;
CREATE INDEX [IX_Customer_IsAdmin] ON [Customer]([IsAdmin]) ;
CREATE INDEX [IX_Customer_OkToEMail] ON [Customer]([OkToEmail]) ;
CREATE INDEX [IX_Customer_Deleted] ON [Customer]([Deleted]) ;
CREATE INDEX [IX_Customer_AffiliateID] ON [Customer]([AffiliateID]) ;
CREATE INDEX [IX_Customer_CouponCode] ON [Customer]([CouponCode]) ;
CREATE INDEX [IX_Customer_CreatedOn] ON [Customer]([CreatedOn]) ;
CREATE INDEX [IX_CustomerLevel_Deleted] ON [CustomerLevel]([Deleted]) ;
CREATE INDEX [IX_CustomerLevel_Name] ON [CustomerLevel]([Name]) ;
CREATE INDEX [IX_CustomerLevel_DisplayOrder] ON [CustomerLevel]([DisplayOrder]) ;
CREATE INDEX [IX_CustomerLevel_DisplayOrder_Name] ON [CustomerLevel]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_CustomerSession_CustomerID] ON [CustomerSession]([CustomerID]) ;
CREATE UNIQUE INDEX [UIX_Distributor_DistributorGUID] ON [Distributor]([DistributorGUID]);
CREATE INDEX [IX_Distributor_DisplayOrder] ON [Distributor]([DisplayOrder]) ;
CREATE INDEX [IX_Distributor_Name] ON [Distributor]([Name]) ;
CREATE INDEX [IX_Distributor_DisplayOrder_Name] ON [Distributor]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_Document_DocumentGUID] ON [Document]([DocumentGUID]);
CREATE INDEX [IX_Document_Published] ON [Document]([Published]) ;
CREATE INDEX [IX_Document_Wholesale] ON [Document]([Wholesale]) ;
CREATE INDEX [IX_Document_Deleted] ON [Document]([Deleted]) ;
CREATE INDEX [IX_Document_DocumentTypeID] ON [Document]([DocumentTypeID]) ;
CREATE INDEX [IX_Document_Published_Deleted] ON [Document]([Published],[Deleted]) ;
CREATE INDEX [IX_Document_Wholesale_Deleted] ON [Document]([Wholesale],[Deleted]) ;
CREATE INDEX [IX_Document_Name] ON [Document]([Name]) ;
CREATE UNIQUE INDEX [UIX_DocumentLibrary_DocumentID_LibraryID] ON [DocumentLibrary]([DocumentID],[LibraryID]);
CREATE INDEX [IX_DocumentLIbrary_DocumentID] ON [DocumentLibrary]([DocumentID]) ;
CREATE INDEX [IX_DocumentLibrary_LibraryID] ON [DocumentLibrary]([LibraryID]) ;
CREATE INDEX [IX_DocumentType_DocumentTypeGUID] ON [DocumentType]([DocumentTypeGUID]) ;
CREATE INDEX [IX_DocumentType_Name] ON [DocumentType]([Name]) ;
CREATE INDEX [IX_DocumentType_DisplayOrder] ON [DocumentType]([DisplayOrder]) ;
CREATE INDEX [IX_DocumentType_DisplayOrder_Name] ON [DocumentType]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_EventhHandler] ON [EventHandler] ([EventName]);
CREATE UNIQUE INDEX [UIX_ExtendedPrice_2] ON [ExtendedPrice] ([ExtendedPriceGUID]);
CREATE INDEX [IX_ExtendedPrice_VariantID_CustomerLevelID] ON [ExtendedPrice]([VariantID],[CustomerLevelID]) ;
CREATE INDEX [IX_ExtendedPrice_VariantID] ON [ExtendedPrice]([VariantID]) ;
CREATE INDEX [IX_FailedTransaction_OrderDate] ON [FailedTransaction]([OrderDate]) ;
CREATE INDEX [IX_FailedTransaction_PaymentGateway] ON [FailedTransaction]([PaymentGateway]) ;
CREATE UNIQUE INDEX [UIX_Gallery_GalleryGUID] ON [Gallery] ([GalleryGUID]);
CREATE INDEX [IX_Gallery_DisplayOrder] ON [Gallery]([DisplayOrder]) ;
CREATE INDEX [IX_Gallery_DisplayOrder_Name] ON [Gallery]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_Gallery_Name] ON [Gallery]([Name]) ;
CREATE INDEX [IX_Gallery_Deleted] ON [Gallery]([Deleted]) ;
CREATE UNIQUE INDEX [UIX_Genre_GenreGUID] ON [Genre]([GenreGUID]);
CREATE INDEX [IX_Genre_Name] ON [Genre]([Name]) ;
CREATE INDEX [IX_Genre_DisplayOrder_Name] ON [Genre]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_Feed_FeedGUID] ON [Feed] ([FeedGUID]);
CREATE INDEX [IX_Feed_DisplayOrder] ON [Feed]([DisplayOrder]) ;
CREATE INDEX [IX_Feed_DisplayOrder_Name] ON [Feed]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_GiftCard_GiftCardGUID] ON [GiftCard] ([GiftCardGUID]);
CREATE INDEX [IX_GiftCard_SerialNumber] ON [GiftCard]([SerialNumber]) ;
CREATE INDEX [IX_GiftCard_ExpirationDate] ON [GiftCard]([ExpirationDate]) ;
CREATE INDEX [IX_GiftCard_CreatedOn] ON [GiftCard]([CreatedOn]) ;
CREATE INDEX [IX_GiftCard_PurchasedByCustomerID] ON [GiftCard]([PurchasedByCustomerID]) ;
CREATE UNIQUE INDEX [UIX_GiftCardUsage_GiftCardUsageGUID] ON [GiftCardUsage]([GiftCardUsageGUID]);
CREATE INDEX [IX_GiftCardUsage_GiftCardID] ON [GiftCardUsage]([GiftCardID]) ;
CREATE INDEX [IX_GiftCardUsage_UsedByCustomerID] ON [GiftCardUsage]([UsedByCustomerID]) ;
CREATE UNIQUE INDEX [UIX_Inventory_InventoryGUID] ON [Inventory] ([InventoryGUID]);
CREATE INDEX [IX_Inventory_VariantID_Color_Size] ON [Inventory]([VariantID],[Color],[Size]) ;
CREATE INDEX [IX_KitCart_CreatedOn] ON [KitCart]([CreatedOn]) ;
CREATE INDEX [IX_KitCart_ShoppingCartRecID] ON [KitCart]([ShoppingCartRecID]) ;
CREATE INDEX [IX_KitCart_CustomerID_ShoppingCartRecID] ON [KitCart]([CustomerID],[ShoppingCartRecID]) ;
CREATE INDEX [IX_KitCart_ProductID] ON [KitCart]([ProductID]) ;
CREATE INDEX [IX_KitCart_VariantID] ON [KitCart]([VariantID]) ;
CREATE INDEX [IX_KitCart_KitGroupID] ON [KitCart]([KitGroupID]) ;
CREATE INDEX [IX_KitCart_KitItemID] ON [KitCart]([KitItemID]) ;
CREATE UNIQUE INDEX [UIX_KitGroup_KitGroupGUID] ON [KitGroup] ([KitGroupGUID]);
CREATE INDEX [IX_KitGroup_ProductID] ON [KitGroup]([ProductID]) ;
CREATE INDEX [IX_KitGroup_ProductID_DisplayOrder] ON [KitGroup]([ProductID],[DisplayOrder]) ;
CREATE INDEX [IX_KitGroup_DisplayOrder] ON [KitGroup]([DisplayOrder]) ;
CREATE UNIQUE INDEX [UIX_KitGroupType_KitGroupTypeGUID] ON [KitGroupType] ([KitGroupTypeGUID]);
CREATE INDEX [IX_KitGroupType_DisplayOrder] ON [KitGroupType]([DisplayOrder]) ;
CREATE INDEX [IX_KitGroupType_DisplayOrder_Name] ON [KitGroupType]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_KitGroupType_Name] ON [KitGroupType]([Name]) ;
CREATE UNIQUE INDEX [UIX_KitItem_KitItemGUID] ON [KitItem] ([KitItemGUID]);
CREATE INDEX [IX_KitItem_KitGroupID] ON [KitItem]([KitGroupID]) ;
CREATE INDEX [IX_KitItem_KitGroupID_DisplayOrder] ON [KitItem]([KitGroupID],[DisplayOrder]) ;
CREATE INDEX [IX_KitItem_DisplayOrder] ON [KitItem]([DisplayOrder]) ;
CREATE INDEX [IX_KitItem_DisplayOrder_Name] ON [KitItem]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_KitItem_Name] ON [KitItem]([Name]) ;
CREATE INDEX [IX_Library_Deleted] ON [Library]([Deleted]) ;
CREATE INDEX [IX_Library_Published] ON [Library]([Published]) ;
CREATE INDEX [IX_Library_Wholesale] ON [Library]([Wholesale]) ;
CREATE INDEX [IX_Library_LibraryGUID] ON [Library]([LibraryGUID]) ;
CREATE INDEX [IX_Library_ParentLibraryID] ON [Library]([ParentLibraryID]) ;
CREATE INDEX [IX_Library_DisplayOrder] ON [Library]([DisplayOrder]) ;
CREATE INDEX [IX_Library_Deleted_Published] ON [Library]([Deleted],[Published]) ;
CREATE INDEX [IX_Library_Deleted_Wholesale] ON [Library]([Deleted],[Wholesale]) ;
CREATE INDEX [IX_Library_Name] ON [Library]([Name]) ;
CREATE INDEX [IX_Library_DisplayOrder_Name] ON [Library]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_Locale_LocaleSettingGUID] ON [LocaleSetting] ([LocaleSettingGUID]);
CREATE UNIQUE INDEX [UIX_Locale_Name] ON [LocaleSetting] ([Name]);
CREATE INDEX [IX_Locale_DisplayOrder_Name] ON [LocaleSetting]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_Locale_DisplayOrder] ON [LocaleSetting]([DisplayOrder]) ;
CREATE INDEX [IX_CustomerEvents_CustomerID] ON [LOG_CustomerEvent]([CustomerID]) ;
CREATE INDEX [IX_LOG_CustomerEvents_CustomerID_Timestamp] ON [LOG_CustomerEvent]([CustomerID],[Timestamp]) ;
CREATE INDEX [IX_CustomerEvents_Timestamp] ON [LOG_CustomerEvent]([Timestamp]) ;
CREATE INDEX [IX_CustomerEvents_EventID] ON [LOG_CustomerEvent]([EventID]) ;
CREATE UNIQUE INDEX [UIX_MailingMgrLog_MailingMgrLogGUID] ON [MailingMgrLog] ([MailingMgrLogGUID]);
CREATE INDEX [IX_MailingMgrLog_SentOn_ToEMail] ON [MailingMgrLog]([SentOn],[ToEmail]) ;
CREATE UNIQUE INDEX [UIX_Manufacturer_ManufacturerGUID] ON [Manufacturer] ([ManufacturerGUID]);
CREATE INDEX [IX_Manufacturer_Deleted] ON [Manufacturer]([Deleted]) ;
CREATE INDEX [IX_Manufacturer_DisplayOrder] ON [Manufacturer]([DisplayOrder]) ;
CREATE INDEX [IX_Manufacturer_Name] ON [Manufacturer]([Name]) ;
CREATE INDEX [IX_Manufacturer_DisplayOrder_Name] ON [Manufacturer]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_News_NewsGUID] ON [News] ([NewsGUID]);
CREATE INDEX [IX_News_ExpiresOn] ON [News]([ExpiresOn] DESC) ;
CREATE INDEX [IX_News_Deleted] ON [News]([Deleted]) ;
CREATE INDEX [IX_News_Published] ON [News]([Published]) ;
CREATE INDEX [IX_News_Wholesale] ON [News]([Wholesale]) ;
CREATE UNIQUE INDEX [UIX_OrderNumbers_OrderNumberGUID] ON [OrderNumbers] ([OrderNumberGUID]);
CREATE INDEX [IX_OrderNumbers_CreatedOn] ON [OrderNumbers]([CreatedOn]) ;
CREATE INDEX [IX_Orders_OrderNumber] ON [Orders]([OrderNumber]) ;
CREATE INDEX [IX_Orders_ParentOrderNumber] ON [Orders]([ParentOrderNumber]) ;
CREATE INDEX [IX_Orders_CustomerID] ON [Orders]([CustomerID]) ;
CREATE INDEX [IX_Orders_OrderNumber_CustomerID] ON [Orders]([OrderNumber],[CustomerID]) ;
CREATE INDEX [IX_Orders_AffiliateID] ON [Orders]([AffiliateID]) ;
CREATE INDEX [IX_Orders_OrderDate] ON [Orders]([OrderDate]) ;
CREATE INDEX [IX_Orders_OrderGUID] ON [Orders]([OrderGUID]) ;
CREATE INDEX [IX_Orders_EMail] ON [Orders]([EMail]) ;
CREATE INDEX [IX_Orders_IsNew] ON [Orders]([IsNew]) ;
CREATE INDEX [IX_Orders_CouponCode] ON [Orders]([CouponCode]) ;
CREATE INDEX [IX_Orders_TransactionState] ON [Orders]([TransactionState]) ;
CREATE CLUSTERED INDEX [IX_Orders_CustomCart_OrderNumber] ON [Orders_CustomCart]([OrderNumber]) ;
CREATE INDEX [IX_Orders_CustomCart_CustomCartRecID] ON [Orders_CustomCart]([CustomCartRecID]) ;
CREATE INDEX [IX_Orders_CustomCart_CustomerID] ON [Orders_CustomCart]([CustomerID]) ;
CREATE INDEX [IX_Orders_CustomCart_ShoppingCartRecID] ON [Orders_CustomCart]([ShoppingCartRecID]) ;
CREATE INDEX [IX_Orders_CustomCart_ProductID_VariantID_ChosenColor_ChosenSize] ON [Orders_CustomCart]([ProductID],[VariantID],[ChosenColor],[ChosenSize]) ;
CREATE INDEX [IX_Orders_CustomCart_CreatedOn] ON [Orders_CustomCart]([CreatedOn]) ;
CREATE INDEX [IX_Orders_CustomCart_ProductSKU] ON [Orders_CustomCart]([ProductSKU]) ;
CREATE CLUSTERED INDEX [IX_Orders_KitCart_OrderNumber] ON [Orders_KitCart]([OrderNumber]) ;
CREATE INDEX [IX_Orders_KitCart_ProductID_VariantID] ON [Orders_KitCart]([ProductID],[VariantID]) ;
CREATE INDEX [IX_Orders_KitCart_CreatedOn] ON [Orders_KitCart]([CreatedOn]) ;
CREATE INDEX [IX_Orders_KitCart_KitCartRecID] ON [Orders_KitCart]([KitCartRecID]) ;
CREATE INDEX [IX_Orders_KitCart_CustomerID] ON [Orders_KitCart]([CustomerID]) ;
CREATE INDEX [IX_Orders_KitCart_ShoppingCartRecID] ON [Orders_KitCart]([ShoppingCartRecID]) ;
CREATE INDEX [IX_Orders_KitCart_KitGroupID] ON [Orders_KitCart]([KitGroupID]) ;
CREATE INDEX [IX_Orders_ShoppingCart_OrderedProductSKU] ON [Orders_ShoppingCart]([OrderedProductSKU]) ;
CREATE INDEX [IX_Orders_ShoppingCart_CustomerID] ON [Orders_ShoppingCart]([CustomerID]) ;
CREATE CLUSTERED INDEX [IX_Orders_ShoppingCart_OrderNumber_CustomerID] ON [Orders_ShoppingCart]([OrderNumber],[CustomerID]) ;
CREATE INDEX [IX_Orders_ShoppingCart_ShoppingCartRecID] ON [Orders_ShoppingCart]([ShoppingCartRecID]) ;
CREATE INDEX [IX_Orders_ShoppingCart_ProductID] ON [Orders_ShoppingCart]([ProductID]) ;
CREATE INDEX [IX_Orders_ShoppingCart_ProductID_VariantID_ChosenColor_ChosenSize] ON [Orders_ShoppingCart]([ProductID],[VariantID],[ChosenColor],[ChosenSize]) ;
CREATE INDEX [IX_Orders_ShoppingCart_CreatedOn] ON [Orders_ShoppingCart]([CreatedOn]) ;
CREATE UNIQUE INDEX [UIX_Partner_PartnerGUID] ON [Partner] ([PartnerGUID]);
CREATE INDEX [IX_Partner_DisplayOrder] ON [Partner]([DisplayOrder]) ;
CREATE INDEX [IX_Partner_DisplayOrder_Name] ON [Partner]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_Partner_Name] ON [Partner]([Name]) ;
CREATE INDEX [IX_Partner_Published] ON [Partner]([Published]) ;
CREATE INDEX [IX_Partner_Wholesale] ON [Partner]([Wholesale]) ;
CREATE INDEX [IX_Partner_Deleted] ON [Partner]([Deleted]) ;
CREATE UNIQUE INDEX [UIX_Poll_PollGUID] ON [Poll] ([PollGUID]);
CREATE INDEX [IX_Poll_DisplayOrder] ON [Poll]([DisplayOrder]) ;
CREATE INDEX [IX_Poll_Name] ON [Poll]([Name]) ;
CREATE INDEX [IX_Poll_DisplayOrder_Name] ON [Poll]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_Poll_Published] ON [Poll]([Published]) ;
CREATE INDEX [IX_Poll_Wholesale] ON [Poll]([Wholesale]) ;
CREATE INDEX [IX_Poll_Deleted] ON [Poll]([Deleted]) ;
CREATE INDEX [IX_Poll_ExpiresOn] ON [Poll]([ExpiresOn]) ;
CREATE UNIQUE INDEX [UIX_PollAnswer_PollAnswerGUID] ON [PollAnswer] ([PollAnswerGUID]);
CREATE INDEX [IX_PollAnswers_PollID_DisplayOrder] ON [PollAnswer]([PollID],[DisplayOrder]) ;
CREATE INDEX [IX_PollAnswer_Deleted] ON [PollAnswer]([Deleted]) ;
CREATE INDEX [IX_PollAnswer_PollID] ON [PollAnswer]([PollID]) ;
CREATE INDEX [IX_PollAnswer_Name] ON [PollAnswer]([Name]) ;
CREATE UNIQUE INDEX [UIX_PollSortOrder_PollSortOrderGUID] ON [PollSortOrder] ([PollSortOrderGUID]);
CREATE INDEX [IX_PollSortOrder_DisplayOrder] ON [PollSortOrder]([DisplayOrder]) ;
CREATE INDEX [IX_PollSortOrder_Name] ON [PollSortOrder]([Name]) ;
CREATE INDEX [IX_PollSortOrder_DisplayOrder_Name] ON [PollSortOrder]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_PollVotingRecord_PollID_CustomerID] ON [PollVotingRecord] ([PollID],[CustomerID]);
CREATE UNIQUE INDEX [UIX_PollVotingRecord_PollVotingRecordGUID] ON [PollVotingRecord] ([PollVotingRecordGUID]);
CREATE INDEX [IX_PollVotingRecord_PollID] ON [PollVotingRecord]([PollID]) ;
CREATE INDEX [IX_PollVotingRecord_CreatedOn] ON [PollVotingRecord]([CreatedOn]) ;
CREATE UNIQUE INDEX [UIX_Product_ProductGUID] ON [Product] ([ProductGUID]);
CREATE INDEX [IX_Product_SKU] ON [Product]([SKU]) ;
CREATE INDEX [IX_Product_IsImport] ON [Product]([IsImport]) ;
CREATE INDEX [IX_Product_IsSystem] ON [Product]([IsSystem]) ;
CREATE INDEX [IX_Product_Published] ON [Product]([Published]) ;
CREATE INDEX [IX_Product_Wholesale] ON [Product]([Wholesale]) ;
CREATE INDEX [IX_Product_Deleted] ON [Product]([Deleted]) ;
CREATE INDEX [IX_Product_ProductTypeID] ON [Product]([ProductTypeID]) ;
CREATE INDEX [IX_Product_IsAPack] ON [Product]([IsAPack]) ;
CREATE INDEX [IX_Product_IsAKit] ON [Product]([IsAKit]) ;
CREATE INDEX [IX_Product_Name] ON [Product]([Name]) ;
CREATE INDEX [IX_Product_ManufacturerPartNumber] ON [Product]([ManufacturerPartNumber]) ;
CREATE INDEX [IX_Product_Published_Deleted] ON [Product]([Published],[Deleted]) ;
CREATE INDEX [IX_Product_Wholesale_Deleted] ON [Product]([Wholesale],[Deleted]) ;
CREATE INDEX [IX_Product_ProductID] ON [ProductCategory]([ProductID]) ;
CREATE INDEX [IX_Category_CategoryID] ON [ProductCategory]([CategoryID]) ;
CREATE INDEX [IX_ProductType_ProductTypeGUID] ON [ProductType]([ProductTypeGUID]) ;
CREATE INDEX [IX_ProductType_Name] ON [ProductType]([Name]) ;
CREATE INDEX [IX_ProductType_DisplayOrder] ON [ProductType]([DisplayOrder]) ;
CREATE INDEX [IX_ProductType_DisplayOrder_Name] ON [ProductType]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_ProductVariant_VariantID] ON [ProductVariant] ([VariantGUID]);
CREATE INDEX [IX_ProductVariant_ProductID] ON [ProductVariant]([ProductID]) ;
CREATE INDEX [IX_ProductVariant_SKUSuffix] ON [ProductVariant]([SKUSuffix]) ;
CREATE INDEX [IX_ProductVariant_ManufacturerPartNumber] ON [ProductVariant]([ManufacturerPartNumber]) ;
CREATE INDEX [IX_ProductVariant_Deleted] ON [ProductVariant]([Deleted]) ;
CREATE INDEX [IX_ProductVariant_Published] ON [ProductVariant]([Published]) ;
CREATE INDEX [IX_ProductVariant_Wholesale] ON [ProductVariant]([Wholesale]) ;
CREATE INDEX [IX_ProductVariant_Deleted_Published] ON [ProductVariant]([Deleted],[Published]) ;
CREATE INDEX [IX_ProductVariant_Deleted_Wholesale] ON [ProductVariant]([Deleted],[Wholesale]) ;
CREATE INDEX [IX_ProductVariant_IsDefault] ON [ProductVariant]([IsDefault]) ;
CREATE INDEX [IX_ProductVariant_DisplayOrder] ON [ProductVariant]([DisplayOrder]) ;
CREATE INDEX [IX_ProductVariant_Name] ON [ProductVariant]([Name]) ;
CREATE INDEX [IX_ProductVariant_DisplayOrder_Name] ON [ProductVariant]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_QuantityDiscount_QuantityDiscountGUID] ON [QuantityDiscount] ([QuantityDiscountGUID]);
CREATE INDEX [IX_QuantityDiscount_DisplayOrder] ON [QuantityDiscount]([DisplayOrder]) ;
CREATE INDEX [IX_QuantityDiscount_DisplayOrder_Name] ON [QuantityDiscount]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_QuantityDiscount_Name] ON [QuantityDiscount]([Name]) ;
CREATE UNIQUE INDEX [UIX_QuantityDiscountTable_QuantityDiscountTableGUID] ON [QuantityDiscountTable] ([QuantityDiscountTableGUID]);
CREATE INDEX [IX_QuantityDiscountTable_QuantityDiscountTableID] ON [QuantityDiscountTable]([QuantityDiscountID]) ;
CREATE INDEX [IX_QuantityDiscountTable_LowQuantity_HighQuantity] ON [QuantityDiscountTable]([LowQuantity],[HighQuantity]) ;
CREATE INDEX [IX_Rating_FoundNotHelpful] ON [Rating]([FoundNotHelpful]) ;
CREATE INDEX [IX_Rating_CreatedOn] ON [Rating]([CreatedOn]) ;
CREATE INDEX [IX_Rating] ON [Rating]([HasComment]) ;
CREATE INDEX [IX_Rating_ProductID] ON [Rating]([ProductID]) ;
CREATE INDEX [IX_Rating_CustomerID] ON [Rating]([CustomerID]) ;
CREATE INDEX [IX_Rating_IsROTD] ON [Rating]([IsROTD]) ;
CREATE INDEX [IX_Rating_FoundHelpful] ON [Rating]([FoundHelpful]) ;
CREATE INDEX [IX_Rating_IsFilthy] ON [Rating]([IsFilthy]) ;
CREATE INDEX [IX_RatingCommentHelpfulness_StoreID] ON [RatingCommentHelpfulness]([StoreID]) ;
CREATE INDEX [IX_RatingCommentHelpfulness_VotingCustomerID] ON [RatingCommentHelpfulness]([VotingCustomerID]) ;
CREATE INDEX [IX_RatingCommentHelpfulness_ProductID] ON [RatingCommentHelpfulness]([ProductID]) ;
CREATE INDEX [IX_RatingCommentHelpfulness_RatingCustomerID] ON [RatingCommentHelpfulness]([RatingCustomerID]) ;
CREATE INDEX [IX_RatingCommentHelpfulness_Helpful] ON [RatingCommentHelpfulness]([Helpful]) ;
CREATE UNIQUE INDEX [UIX_SkinPreview_Name] ON [SkinPreview] ([Name]);
CREATE INDEX [IX_SkinPreview_GroupName] ON [SkinPreview]([GroupName]) ;
CREATE INDEX [IX_SQLLog_ExecutedBy] ON [SQLLog]([ExecutedBy]) ;
CREATE INDEX [IX_SQLLog_ExecutedOn] ON [SQLLog]([ExecutedOn]) ;
CREATE UNIQUE INDEX [UIX_SalesPrompt_SalesPromptGUID] ON [SalesPrompt] ([SalesPromptGUID]);
CREATE INDEX [IX_SalesPrompt_Deleted] ON [SalesPrompt]([Deleted]) ;
CREATE INDEX [IX_SalesPrompt_Name] ON [SalesPrompt]([Name]) ;
CREATE UNIQUE INDEX [UIX_Section_SectionGUID] ON [Section] ([SectionGUID]);
CREATE INDEX [IX_Section_ParentSectionID] ON [Section]([ParentSectionID]) ;
CREATE INDEX [IX_Section_DisplayOrder] ON [Section]([DisplayOrder]) ;
CREATE INDEX [IX_Section_Name] ON [Section]([Name]) ;
CREATE INDEX [IX_Section_DisplayOrder_Name] ON [Section]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_Section_Published] ON [Section]([Published]) ;
CREATE INDEX [IX_Section_Wholesale] ON [Section]([Wholesale]) ;
CREATE INDEX [IX_Section_Deleted] ON [Section]([Deleted]) ;
CREATE INDEX [IX_Section_Deleted_Published] ON [Section]([Deleted],[Published]) ;
CREATE INDEX [IX_Section_Deleted_Wholesale] ON [Section]([Deleted],[Wholesale]) ;
CREATE INDEX [IX_ProductSection_SectionID_DisplayOrder] ON [ProductSection]([SectionID],[DisplayOrder]) ;
CREATE UNIQUE INDEX [UIX_ShippingByProduct_ShippingByProductGUID] ON [dbo].[ShippingByProduct]([ShippingByProductGUID])
CREATE INDEX [IX_ShippingByTotal_ShippingMethodID_LowValue_HighValue] ON [ShippingByTotal]([ShippingMethodID],[LowValue],[HighValue]) ;
CREATE INDEX [IX_ShippingByTotal_RowGUID] ON [ShippingByTotal]([RowGUID]) ;
CREATE INDEX [IX_ShippingByTotalByPercent_ShippingMethodID_LowValue_HighValue] ON [ShippingByTotalByPercent]([ShippingMethodID],[LowValue],[HighValue]) ;
CREATE INDEX [IX_ShippingByTotalByPercent_RowGUID] ON [ShippingByTotalByPercent]([RowGUID]) ;
CREATE INDEX [IX_ShippingByWeight_ShippingMethodID_LowValue_HighValue] ON [ShippingByWeight]([ShippingMethodID],[LowValue],[HighValue]) ;
CREATE INDEX [IX_ShippingByWeight_RowGUID] ON [ShippingByWeight]([RowGUID]) ;
CREATE INDEX [IX_ShippingWeightByZone_RowGUID] ON [ShippingWeightByZone]([RowGUID]) ;
CREATE INDEX [IX_ShippingWeightByZone_ShippingZoneID_LowValue_HighValue] ON [ShippingWeightByZone]([ShippingZoneID],[LowValue],[HighValue]) ;
CREATE INDEX [IX_ShippingTotalByZone_RowGUID] ON [ShippingTotalByZone]([RowGUID]) ;
CREATE INDEX [IX_ShippingTotalByZone_ShippingZoneID_LowValue_HighValue] ON [ShippingTotalByZone]([ShippingZoneID],[LowValue],[HighValue]) ;
CREATE UNIQUE INDEX [UIX_ShippingCalculation_ShippingCalculationID] ON [ShippingCalculation] ([ShippingCalculationGUID]);
CREATE INDEX [IX_ShippingCalculation_DisplayOrder] ON [ShippingCalculation]([DisplayOrder]) ;
CREATE INDEX [IX_ShippingCalculation_Name] ON [ShippingCalculation]([Name]) ;
CREATE INDEX [IX_ShippingCalculation_DisplayOrder_Name] ON [ShippingCalculation]([DisplayOrder],[Name]) ;
CREATE UNIQUE INDEX [UIX_ShippingMethod_ShippingMethodGUID] ON [ShippingMethod] ([ShippingMethodGUID]);
CREATE INDEX [IX_ShippingMethod_IsRTShipping] ON [ShippingMethod]([IsRTShipping]) ;
CREATE INDEX [IX_ShippingMethod_DisplayOrder] ON [ShippingMethod]([DisplayOrder]) ;
CREATE UNIQUE INDEX [UIX_ShippingZone_ShippingZoneGUID] ON [ShippingZone] ([ShippingZoneGUID]);
CREATE INDEX [IX_ShippingZone_DisplayOrder] ON [ShippingZone]([DisplayOrder]) ;
CREATE INDEX [IX_ShippingZone_Deleted] ON [ShippingZone]([Deleted]) ;
CREATE UNIQUE INDEX [UIX_ShoppingCart_ShoppingCartRecGUID] ON [ShoppingCart] ([ShoppingCartRecGUID]);
CREATE INDEX [IX_ShoppingCart_CustomerID] ON [ShoppingCart]([CustomerID]) ;
CREATE INDEX [IX_ShoppingCart_CustomerID_CartType] ON [ShoppingCart]([CustomerID],[CartType]) ;
CREATE INDEX [IX_ShoppingCart_ProductID] ON [ShoppingCart]([ProductID]) ;
CREATE INDEX [IX_ShoppingCart_VariantID] ON [ShoppingCart]([VariantID]) ;
CREATE INDEX [IX_ShoppingCart_ProductID_VariantID_ChosenColor_ChosenSize] ON [ShoppingCart]([ProductID],[VariantID],[ChosenColor],[ChosenSize]) ;
CREATE INDEX [IX_ShoppingCart_CreatedOn] ON [ShoppingCart]([CreatedOn]) ;
CREATE INDEX [IX_ShoppingCart_CartType] ON [ShoppingCart]([CartType]) ;
CREATE INDEX [IX_ShoppingCart_CartType_RecurringSubscriptionID] ON [ShoppingCart]([CartType], [RecurringSubscriptionID]) ;
CREATE INDEX [IX_ShoppingCart_NextRecurringShipDate] ON [ShoppingCart]([NextRecurringShipDate]) ;
CREATE INDEX [IX_ShoppingCart_RecurringIndex] ON [ShoppingCart]([RecurringIndex]) ;
CREATE UNIQUE INDEX [UIX_Staff_StaffGUID] ON [Staff] ([StaffGUID]);
CREATE INDEX [IX_Staff_Name] ON [Staff]([Name]) ;
CREATE INDEX [IX_Staff_DisplayOrder_Name] ON [Staff]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_Staff_DisplayOrder] ON [Staff]([DisplayOrder]) ;
CREATE INDEX [IX_Staff_Deleted] ON [Staff]([Deleted]) ;
CREATE INDEX [IX_Staff_Published] ON [Staff]([Published]) ;
CREATE INDEX [IX_Staff_Wholesale] ON [Staff]([Wholesale]) ;
CREATE UNIQUE INDEX [UIX_State_StateGUID] ON [State] ([StateGUID]);
CREATE UNIQUE INDEX [UIX_State_Country_Abbrv] ON [dbo].[State]([CountryID], [Abbreviation])
CREATE INDEX [IX_State_DisplayOrder] ON [State]([DisplayOrder]) ;
CREATE INDEX [IX_State_Name] ON [State]([Name]) ;
CREATE INDEX [IX_State_DisplayOrder_Name] ON [State]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_State_CountryID] ON [State]([CountryID]) ;
CREATE UNIQUE INDEX [UIX_Topic_TopicGUID] ON [Topic] ([TopicGUID]);
CREATE INDEX [IX_Topic_Deleted] ON [Topic]([Deleted]) ;
CREATE UNIQUE INDEX [UIX_Topic_Name] ON [Topic]([Name]) ;
CREATE INDEX [IX_Topic_ShowInSiteMap] ON [Topic]([ShowInSiteMap]) ;
CREATE CLUSTERED INDEX CIX_PasswordLog on dbo.PasswordLog (CustomerID, ChangeDt)
CREATE UNIQUE INDEX [UIX_ZipTaxRate_ZipCode_TaxClassID_CountryID] ON [ZipTaxRate] (ZipCode,TaxClassID,CountryID)
CREATE UNIQUE INDEX [UIX_StateTaxRate_StateID_TaxClassID] ON [StateTaxRate] (StateID,TaxClassID)
CREATE UNIQUE INDEX [UIX_CountryTaxRate_CountryID_TaxClassID] ON [CountryTaxRate] (CountryID,TaxClassID)
CREATE UNIQUE INDEX [UIX_Vector_VectorGUID] ON [Vector]([VectorGUID]);
CREATE INDEX [IX_Vector_Name] ON [Vector]([Name]) ;
CREATE INDEX [IX_Vector_DisplayOrder_Name] ON [Vector]([DisplayOrder],[Name]) ;
CREATE INDEX [IX_Profile_CustomerGuid] ON [Profile]	([CustomerGUID]) ;
END
GO
PRINT N'Creating [dbo].[Profile]'
GO
CREATE TABLE [dbo].[Profile]
(
[StoreID] [int] NULL,
[ProfileID] [int] NOT NULL IDENTITY(1, 1),
[CustomerID] [int] NULL,
[PropertyName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CustomerGUID] [uniqueidentifier] NULL,
[PropertyValueString] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UpdatedOn] [datetime] NULL
)
GO


PRINT N'Creating primary key [PK_Profile] on [dbo].[Profile]'
GO
ALTER TABLE [dbo].[Profile] ADD CONSTRAINT [PK_Profile] PRIMARY KEY CLUSTERED  ([ProfileID])
GO


PRINT N'Creating index [IX_Profile_CustomerGuid] on [dbo].[Profile]'
GO
CREATE NONCLUSTERED INDEX [IX_Profile_CustomerGuid] ON [dbo].[Profile] ([CustomerGUID])
GO


PRINT N'Creating [dbo].[aspdnsf_SetProfileProperties]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_SetProfileProperties]    
    @customerid				int,
    @CustomerGUID			UniqueIdentifier,
    @isAuthenticated        bit,
    @PropertyNames          nvarchar(256),
    @PropertyValuesString   nvarchar(256),
    @storeid				int
    
 
AS
BEGIN 
		IF EXISTS (SELECT * FROM profile with (NOLOCK) where CustomerGUID = @CustomerGUID and PropertyName = @PropertyNames and StoreID = @storeid)
		BEGIN 		
			Update profile Set				
				PropertyValueString = @PropertyValuesString,
				UpdatedOn = GETDATE()	
				Where CustomerGUID = @CustomerGUID and PropertyName = @PropertyNames and StoreID = @storeid
		END
		ELSE
		BEGIN
				Insert into profile
				(
				StoreID,
				CustomerID,
				PropertyName,
				CustomerGUID,
				PropertyValueString,
				UpdatedOn		
				)
				Values
				(
				@storeid,		
				@customerid,
				@PropertyNames,
				@CustomerGUID,
				@PropertyValuesString,
				GETDATE()
				)
		END
END

GO

PRINT N'Creating [dbo].[aspdnsf_ListAllIndexes]'
GO


create proc [dbo].[aspdnsf_ListAllIndexes] 
  
as
begin
    SELECT  
        TABLE_NAME = OBJECT_NAME(i.id), 
        INDEX_NAME = i.name, 
        COLUMN_LIST = dbo.GetIndexColumns(OBJECT_NAME(i.id), i.id, i.indid), 
        IS_CLUSTERED = INDEXPROPERTY(i.id, i.name, 'IsClustered'), 
        IS_UNIQUE = INDEXPROPERTY(i.id, i.name, 'IsUnique'), 
        FILE_GROUP = g.GroupName 
    INTO #AllIndexes
    FROM 
        dbo.sysindexes i 
    INNER join [dbo].
        dbo.sysfilegroups g 
    ON 
        i.groupid = g.groupid 
    WHERE 
        (i.indid BETWEEN 1 AND 254) 
        -- leave out AUTO_STATISTICS: 
        AND (i.Status & 64)=0 
        -- leave out system tables: 
        AND OBJECTPROPERTY(i.id, 'IsMsShipped') = 0 

    SELECT 
        v.*, 
        [PrimaryKey?] = CASE  
            WHEN T.TABLE_NAME IS NOT NULL THEN 1 
            ELSE 0 
        END 
    FROM 
        #AllIndexes v 
    LEFT OUTER join 
        INFORMATION_SCHEMA.TABLE_CONSTRAINTS T  
    ON 
        T.CONSTRAINT_NAME = v.INDEX_NAME 
        AND T.TABLE_NAME = v.TABLE_NAME  
        AND T.CONSTRAINT_TYPE = 'PRIMARY KEY'
    end
GO
PRINT N'Creating [dbo].[aspdnsf_ListAllNonPrimaryIndexes]'
GO

create proc [dbo].[aspdnsf_ListAllNonPrimaryIndexes] 
  
as
begin
    SELECT  
        TABLE_NAME = OBJECT_NAME(i.id), 
        INDEX_NAME = i.name, 
        COLUMN_LIST = dbo.GetIndexColumns(OBJECT_NAME(i.id), i.id, i.indid), 
        IS_CLUSTERED = INDEXPROPERTY(i.id, i.name, 'IsClustered'), 
        IS_UNIQUE = INDEXPROPERTY(i.id, i.name, 'IsUnique'), 
        FILE_GROUP = g.GroupName 
    INTO #AllIndexes
    FROM 
        dbo.sysindexes i 
    INNER join [dbo].
        dbo.sysfilegroups g 
    ON 
        i.groupid = g.groupid 
    WHERE 
        (i.indid BETWEEN 1 AND 254) 
        -- leave out AUTO_STATISTICS: 
        AND (i.Status & 64)=0 
        -- leave out system tables: 
        AND OBJECTPROPERTY(i.id, 'IsMsShipped') = 0 

    SELECT 
        v.*, 
        [PrimaryKey?] = CASE  
            WHEN T.TABLE_NAME IS NOT NULL THEN 1 
            ELSE 0 
        END 
    FROM 
        #AllIndexes v 
    LEFT OUTER join 
        INFORMATION_SCHEMA.TABLE_CONSTRAINTS T  
    ON 
        T.CONSTRAINT_NAME = v.INDEX_NAME 
        AND T.TABLE_NAME = v.TABLE_NAME  
        AND T.CONSTRAINT_TYPE = 'PRIMARY KEY'
    where INDEX_Name like 'IX_%'
    order by Index_Name

END
GO
PRINT N'Creating [dbo].[aspdnsf_GenerateCreatesForAllIndexes]'
GO

create proc [dbo].[aspdnsf_GenerateCreatesForAllIndexes] 
  
as
begin

    SELECT  
        TABLE_NAME = OBJECT_NAME(i.id), 
        INDEX_NAME = i.name, 
        COLUMN_LIST = dbo.GetIndexColumns(OBJECT_NAME(i.id), i.id, i.indid), 
        IS_CLUSTERED = INDEXPROPERTY(i.id, i.name, 'IsClustered'), 
        IS_UNIQUE = INDEXPROPERTY(i.id, i.name, 'IsUnique'), 
        FILE_GROUP = g.GroupName 
    INTO #AllIndexes
    FROM 
        dbo.sysindexes i 
    INNER join [dbo].
        dbo.sysfilegroups g 
    ON 
        i.groupid = g.groupid 
    WHERE 
        (i.indid BETWEEN 1 AND 254) 
        -- leave out AUTO_STATISTICS: 
        AND (i.Status & 64)=0 
        -- leave out system tables: 
        AND OBJECTPROPERTY(i.id, 'IsMsShipped') = 0 

    SELECT  
        CASE WHEN T.TABLE_NAME IS NULL THEN 
            'CREATE ' 
            + CASE IS_UNIQUE WHEN 1 THEN ' UNIQUE' ELSE '' END 
            + CASE IS_CLUSTERED WHEN 1 THEN ' CLUSTERED' ELSE '' END 
            + ' INDEX [' + INDEX_NAME + '] ON [' + v.TABLE_NAME + ']' 
            + ' (' + COLUMN_LIST + ') ON ' + FILE_GROUP 
        ELSE 
            'ALTER TABLE ['+T.TABLE_NAME+']' 
            +' ADD CONSTRAINT ['+INDEX_NAME+']' 
            +' PRIMARY KEY ' 
            + CASE IS_CLUSTERED WHEN 1 THEN ' CLUSTERED' ELSE '' END 
            + ' (' + COLUMN_LIST + ')' 
        END 
    FROM 
         v 
    LEFT OUTER join 
        INFORMATION_SCHEMA.TABLE_CONSTRAINTS T  
    ON 
        T.CONSTRAINT_NAME = v.INDEX_NAME 
        AND T.TABLE_NAME = v.TABLE_NAME  
        AND T.CONSTRAINT_TYPE = 'PRIMARY KEY' 
    where INDEX_Name like 'IX_%'
    ORDER BY 
        v.TABLE_NAME, 
        IS_CLUSTERED DESC


end

GO
PRINT N'Creating [dbo].[aspdnsf_updProductView]'
GO


CREATE PROC [dbo].[aspdnsf_updProductView]
	@OrgCustomerViewID nvarchar(50),
	@CustomerViewID nvarchar(50)

  
AS
SET NOCOUNT ON
BEGIN
	UPDATE dbo.ProductView SET CustomerViewID=@CustomerViewID WHERE CustomerViewID=@OrgCustomerViewID

	--Remove duplicate records.
	DELETE ProductView WHERE ViewID IN(SELECT MIN(ViewID) FROM ProductView WITH (NOLOCK) WHERE CustomerViewID=@CustomerViewID GROUP BY ProductID HAVING COUNT(ProductID) >1)
END
GO
PRINT N'Creating [dbo].[aspdnsf_insProductView]'
GO


CREATE PROC [dbo].[aspdnsf_insProductView]
	@CustomerViewID nvarchar(50),
	@ProductID int,
	@ViewDate datetime

  
AS
SET NOCOUNT ON
BEGIN
	IF EXISTS (SELECT * FROM ProductView with (NOLOCK) where CustomerViewID = @CustomerViewID and ProductID = @ProductID)
	BEGIN 		
		UPDATE ProductView set ViewDate = @ViewDate where CustomerViewID = @CustomerViewID and ProductID = @ProductID
	END
	ELSE
	BEGIN
		insert into ProductView(CustomerViewID,ProductID,ViewDate) values (@CustomerViewID,@ProductID,@ViewDate)
	END
END

GO

PRINT N'Creating [dbo].[ShippingCalculationStore]'
GO
CREATE TABLE [dbo].[ShippingCalculationStore]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[StoreId] [int] NOT NULL,
[ShippingCalculationId] [int] NOT NULL,
[CreatedOn] [datetime] NULL CONSTRAINT [DF_StoreShippingCalculation_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_StoreShippingCalculation] on [dbo].[ShippingCalculationStore]'
GO
ALTER TABLE [dbo].[ShippingCalculationStore] ADD CONSTRAINT [PK_StoreShippingCalculation] PRIMARY KEY CLUSTERED  ([StoreId], [ShippingCalculationId])

GO


PRINT N'Altering [dbo].[ShippingImportExport]'
GO
ALTER TABLE [dbo].[ShippingImportExport] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingImportExport_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[AppConfig]'
GO
ALTER TABLE [dbo].[AppConfig] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_AppConfig_StoreID] DEFAULT ((1)),
[ValueType] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[AllowableValues] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL

GO


PRINT N'Creating index [UIX_AppConfig_Name_StoreID] on [dbo].[AppConfig]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [UIX_AppConfig_Name_StoreID] ON [dbo].[AppConfig] ([Name], [StoreID])

GO


PRINT N'Creating [dbo].[aspdnsf_RemoveDuplicateAppConfigs]'
GO


create proc [dbo].[aspdnsf_RemoveDuplicateAppConfigs] 
as
BEGIN
    delete from [dbo].appconfig where appconfigid in (select max(AppConfigID) as AppConfigID from AppConfig WITH (NOLOCK) where name in (select name from appconfig WITH (NOLOCK) group by name having count(name) > 1)  group by name)
end
GO
PRINT N'Creating [dbo].[LayoutField]'
GO
CREATE TABLE [dbo].[LayoutField]
(
[LayoutFieldID] [int] NOT NULL IDENTITY(1, 1),
[LayoutFieldGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_LayoutField_LayoutFieldGUID] DEFAULT (newid()),
[LayoutID] [int] NULL,
[FieldType] [int] NULL,
[FieldID] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_LayoutField_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_LayoutField_LayoutFieldID] on [dbo].[LayoutField]'
GO
ALTER TABLE [dbo].[LayoutField] ADD CONSTRAINT [PK_LayoutField_LayoutFieldID] PRIMARY KEY CLUSTERED  ([LayoutFieldID])

GO


PRINT N'Creating [dbo].[aspdnsf_insLayoutField]'
GO

CREATE proc [dbo].[aspdnsf_insLayoutField]
	@LayoutID int,
    @FieldType int,
    @FieldID nvarchar(100),
    @LayoutFieldID int OUTPUT
  
AS
SET NOCOUNT ON 

if not exists(select * from dbo.LayoutField where LayoutID=@LayoutID and FieldID=@FieldID)
begin
    INSERT dbo.LayoutField(LayoutID, FieldType, FieldID)
    VALUES (@LayoutID, @FieldType, @FieldID)
    
    set @LayoutFieldID = @@identity
end

GO

PRINT N'Creating [dbo].[aspdnsf_ShowDuplicateAppConfigs]'
GO



create proc [dbo].[aspdnsf_ShowDuplicateAppConfigs] 
  
as
BEGIN
    select * from dbo.appconfig with (nolock) where name in (select name from dbo.appconfig with (nolock) group by name having count(name) > 1) order by name
end

GO
PRINT N'Creating [dbo].[PageType]'
GO
CREATE TABLE [dbo].[PageType]
(
[PageTypeID] [int] NOT NULL IDENTITY(1, 1),
[PageTypeGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_PageType_PageTypeGUID] DEFAULT (newid()),
[PageTypeName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)

GO


PRINT N'Creating primary key [PK_PageType_PageTypeID] on [dbo].[PageType]'
GO
ALTER TABLE [dbo].[PageType] ADD CONSTRAINT [PK_PageType_PageTypeID] PRIMARY KEY CLUSTERED  ([PageTypeID])

GO


PRINT N'Creating [dbo].[LayoutMap]'
GO
CREATE TABLE [dbo].[LayoutMap]
(
[LayoutMapID] [int] NOT NULL IDENTITY(1, 1),
[LayoutMapGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_LayoutMap_LayoutMapGUID] DEFAULT (newid()),
[LayoutID] [int] NOT NULL,
[PageTypeID] [int] NOT NULL,
[PageID] [int] NOT NULL,
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_LayoutMap_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_LayoutMap_LayoutMapID] on [dbo].[LayoutMap]'
GO
ALTER TABLE [dbo].[LayoutMap] ADD CONSTRAINT [PK_LayoutMap_LayoutMapID] PRIMARY KEY CLUSTERED  ([LayoutMapID])

GO


PRINT N'Creating [dbo].[aspdnsf_updAppconfig]'
GO


create proc dbo.aspdnsf_updAppconfig
    @AppConfigID int,
    @Description ntext = null,
    @ConfigValue nvarchar(1000) = null,
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
PRINT N'Creating [dbo].[Layout]'
GO
CREATE TABLE [dbo].[Layout]
(
[LayoutID] [int] NOT NULL IDENTITY(1, 1),
[LayoutGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Layouts_LayoutGUID] DEFAULT (newid()),
[Name] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Description] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[HTML] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Micro] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Icon] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Medium] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Large] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Version] [int] NOT NULL CONSTRAINT [DF_Layouts_LayoutVersion] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_Layouts_CreatedOn] DEFAULT (getdate()),
[UpdatedOn] [datetime] NOT NULL CONSTRAINT [DF_Layouts_UpdatedOn] DEFAULT (getdate()),
[DisplayOrder] [int] NOT NULL CONSTRAINT [DF_Layouts_DisplayOrder] DEFAULT ((1))
)

GO


PRINT N'Creating primary key [PK_Layout_LayoutID] on [dbo].[Layout]'
GO
ALTER TABLE [dbo].[Layout] ADD CONSTRAINT [PK_Layout_LayoutID] PRIMARY KEY CLUSTERED  ([LayoutID])

GO


PRINT N'Creating [dbo].[aspdnsf_insLayout]'
GO


CREATE proc [dbo].[aspdnsf_insLayout]
	@LayoutGUID uniqueidentifier,
    @Name nvarchar(100),
    @Description nvarchar(max),
    @HTML nvarchar(max),
    @Micro nvarchar(100),
    @Icon nvarchar(100),
    @Medium nvarchar(100),
    @Large nvarchar(100),
    @Version int,
    @CreatedOn datetime,
    @UpdatedOn datetime,
    @LayoutID int OUTPUT
  
AS
SET NOCOUNT ON 


    INSERT dbo.Layout(LayoutGUID, Name, Description, HTML, Micro, Icon, Medium, Large, Version, CreatedOn, UpdatedOn)
    VALUES (@LayoutGUID, @Name, @Description, @HTML, @Micro, @Icon, @Medium, @Large, @Version, @CreatedOn, @UpdatedOn)

    set @LayoutID = @@identity

GO


PRINT N'Creating [dbo].[aspdnsf_SysLog]'
GO
CREATE TABLE [dbo].[aspdnsf_SysLog]
(
[SysLogID] [int] NOT NULL IDENTITY(1, 1),
[SysLogGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_aspdnsf_SysLog_SysLogGUID] DEFAULT (newid()),
[Message] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Details] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Type] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Severity] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_aspdnsf_SysLog_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_aspdnsf_SysLog] on [dbo].[aspdnsf_SysLog]'
GO
ALTER TABLE [dbo].[aspdnsf_SysLog] ADD CONSTRAINT [PK_aspdnsf_SysLog] PRIMARY KEY CLUSTERED  ([SysLogID] DESC)

GO


PRINT N'Creating [dbo].[aspdnsf_insSysLog]'
GO


create proc dbo.aspdnsf_insSysLog
    @message nvarchar(max),
    @details nvarchar(max),
	@type nvarchar(100),
	@severity nvarchar(100)  
  
AS
SET NOCOUNT ON

INSERT [dbo].[aspdnsf_SysLog] (Message,Details,Type,Severity)
	VALUES(	@message,
			@details,
			@type,
			@severity)
GO
PRINT N'Creating [dbo].[aspdnsf_getSysLog]'
GO



create proc dbo.aspdnsf_getSysLog
	@type nvarchar(100),
	@severity nvarchar(100),
	@startDate DateTime,
	@endDate DateTime  
  
AS
SET NOCOUNT ON

select * from aspdnsf_SysLog WITH (NOLOCK)
where CreatedOn >= @startDate
	and CreatedOn <= @endDate
	and (@type = 'ALL' or [Type] = @type)
	and (@severity = 'ALL' or Severity = @Severity)
GO
	DECLARE @countryID int
	SELECT @countryID = CountryID FROM Country WHERE Name='United States'
	DECLARE @SQL nvarchar(max)
	SET @sql = 'ALTER TABLE [dbo].[ZipTaxRate] ADD
	[CountryID] [int] NOT NULL CONSTRAINT [DF_ZipTaxRate_CountryID] DEFAULT ((' + CAST(@countryID as nvarchar) + '))'
	EXEC(@sql)
GO


PRINT N'Creating index [UIX_ZipTaxRate_ZipCode_TaxClassID_CountryID] on [dbo].[ZipTaxRate]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [UIX_ZipTaxRate_ZipCode_TaxClassID_CountryID] ON [dbo].[ZipTaxRate] ([ZipCode], [TaxClassID], [CountryID])

GO


PRINT N'Creating [dbo].[aspdnsf_getTaxclass]'
GO


create proc [dbo].[aspdnsf_getTaxclass]
    @TaxClassID int = null
  
AS
SET NOCOUNT ON 

SELECT TaxClassID, TaxClassGUID, Name, TaxCode, DisplayOrder, CreatedOn
FROM dbo.Taxclass with (nolock) 
WHERE TaxClassID = COALESCE(@TaxClassID, TaxClassID)

GO
PRINT N'Creating [dbo].[aspdnsf_updTaxclass]'
GO


create proc [dbo].[aspdnsf_updTaxclass]
    @TaxClassID int,
    @TaxCode nvarchar(400),
    @DisplayOrder int
  
AS
SET NOCOUNT ON 

UPDATE dbo.Taxclass
SET 
    TaxCode = COALESCE(@TaxCode, TaxCode),
    DisplayOrder = COALESCE(@DisplayOrder, DisplayOrder)
WHERE TaxClassID = @TaxClassID
GO
PRINT N'Creating [dbo].[aspdnsf_insTaxclass]'
GO


create proc [dbo].[aspdnsf_insTaxclass]
    @Name nvarchar(400),
    @TaxCode nvarchar(400),
    @DisplayOrder int,
    @TaxClassID int OUTPUT
  
AS
SET NOCOUNT ON 

insert dbo.Taxclass(TaxClassGUID, Name, TaxCode, DisplayOrder, CreatedOn)
values (newid(), @Name, @TaxCode, @DisplayOrder, getdate())

set @TaxClassID = @@identity
GO
PRINT N'Creating [dbo].[aspdnsf_updZipTaxRate]'
GO


create proc [dbo].[aspdnsf_updZipTaxRate]
    @ZipTaxID int,
    @TaxRate money,
	@CountryID int
  
AS
SET NOCOUNT ON 
UPDATE dbo.ZipTaxRate
SET 
    TaxRate = COALESCE(@TaxRate, TaxRate)
WHERE ZipTaxID = @ZipTaxID AND CountryID = @CountryID

GO

PRINT N'Creating [dbo].[aspdnsf_UpdateZipTaxRateCountry]'
GO


create proc [dbo].[aspdnsf_UpdateZipTaxRateCountry]
    @ZipTaxID int,
	@ZipCode nvarchar (20),
    @TaxRate money,
	@CountryID int,
	@OriginalCountryID int
  
AS
SET NOCOUNT ON 


UPDATE dbo.ZipTaxRate
SET 
    TaxRate = COALESCE(@TaxRate, TaxRate), CountryID = (COALESCE(@CountryID, CountryID)), ZipCode = (COALESCE(@ZipCode, ZipCode))
WHERE ZipTaxID = @ZipTaxID AND CountryID = @OriginalCountryID
GO
PRINT N'Creating [dbo].[aspdnsf_insCountryTaxRate]'
GO


create proc [dbo].[aspdnsf_insCountryTaxRate]
    @CountryID int,
    @TaxClassID int,
    @TaxRate money,
    @CountryTaxID int OUTPUT
      
AS
SET NOCOUNT ON 


insert dbo.CountryTaxRate(CountryID, TaxClassID, TaxRate, CreatedOn)
values (@CountryID, @TaxClassID, @TaxRate, getdate())

set @CountryTaxID = @@identity
GO
PRINT N'Altering [dbo].[Country]'
GO
ALTER TABLE [dbo].[Country] ADD
[PostalCodeRequired] [tinyint] NOT NULL CONSTRAINT [DF_Country_PostalCodeRequired] DEFAULT ((1)),
[PostalCodeRegex] [nvarchar] (4000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PostalCodeExample] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL

GO


PRINT N'Creating [dbo].[aspdnsf_getCountryTaxRateByID]'
GO


create proc [dbo].[aspdnsf_getCountryTaxRateByID]
    @CountryTaxID int
  
AS
SET NOCOUNT ON 


SELECT ctr.CountryTaxID, ctr.CountryID, ctr.TaxClassID, ctr.TaxRate, ctr.CreatedOn, t.Name TaxClass, c.Name Country
FROM dbo.CountryTaxRate ctr with (nolock) join dbo.TaxClass t with (nolock) on ctr.TaxClassID = t.TaxClassID join dbo.Country c on c.CountryID = ctr.CountryID
WHERE ctr.CountryTaxID = @CountryTaxID
GO
PRINT N'Creating [dbo].[aspdnsf_updCountryTaxRate]'
GO


create proc [dbo].[aspdnsf_updCountryTaxRate]
    @CountryTaxID int,
    @TaxRate money
  
AS
SET NOCOUNT ON 


UPDATE dbo.CountryTaxRate
SET 
    TaxRate = COALESCE(@TaxRate, TaxRate)
WHERE CountryTaxID = @CountryTaxID
GO
PRINT N'Creating [dbo].[AffiliateStore]'
GO
CREATE TABLE [dbo].[AffiliateStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[AffiliateID] [int] NOT NULL CONSTRAINT [DF_AffiliateStore_AffiliateID] DEFAULT ((0)),
[StoreID] [int] NOT NULL CONSTRAINT [DF_AffiliateStore_StoreID] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_AffiliateStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_AffiliateStore] on [dbo].[AffiliateStore]'
GO
ALTER TABLE [dbo].[AffiliateStore] ADD CONSTRAINT [PK_AffiliateStore] PRIMARY KEY CLUSTERED  ([AffiliateID], [StoreID])

GO


PRINT N'Creating [dbo].[aspdnsf_getAffiliate]'
GO



CREATE PROC dbo.aspdnsf_getAffiliate
    @AffiliateID int = null
  
AS
SET NOCOUNT ON 


SELECT a.AffiliateID, AffiliateGUID, EMail, Password, DateOfBirth, Gender, Notes, IsOnline, FirstName, LastName, Name, Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, 
WebSiteDescription, URL, TrackingOnly, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SENoScript, SEAltText, SEKeywords, SEDescription, Published, Wholesale, Deleted, a.CreatedOn, SaltKey, StoreID
FROM dbo.Affiliate a with (nolock) left join AffiliateStore b with (nolock) on a.AffiliateID = b.AffiliateID
WHERE a.AffiliateID = COALESCE(@AffiliateID, a.AffiliateID)

GO

PRINT N'Creating [dbo].[aspdnsf_getStateTaxRateByID]'
GO


create proc [dbo].[aspdnsf_getStateTaxRateByID]
    @StateTaxID int
  
AS
SET NOCOUNT ON 


SELECT sr.StateTaxID, sr.StateID, sr.TaxClassID, sr.TaxRate, sr.CreatedOn, t.Name TaxClass, s.Name StateName
FROM dbo.StateTaxRate sr with (nolock) join dbo.TaxClass t with (nolock) on sr.TaxClassID = t.TaxClassID join dbo.State s on s.StateID = sr.StateID
WHERE StateTaxID = @StateTaxID
GO
PRINT N'Creating [dbo].[aspdnsf_updAffiliate]'
GO


create proc dbo.aspdnsf_updAffiliate
    @AffiliateID int,
    @EMail nvarchar(100) = null,
    @Password nvarchar(250) = null,
    @DateOfBirth datetime = null,
    @Gender nvarchar(1) = null,
    @Notes text = null,
    @IsOnline tinyint = null,
    @FirstName nvarchar(100) = null,
    @LastName nvarchar(100) = null,
    @Name nvarchar(100) = null,
    @Company nvarchar(100) = null,
    @Address1 nvarchar(100) = null,
    @Address2 nvarchar(100) = null,
    @Suite nvarchar(50) = null,
    @City nvarchar(100) = null,
    @State nvarchar(100) = null,
    @Zip nvarchar(10) = null,
    @Country nvarchar(100) = null,
    @Phone nvarchar(25) = null,
    @WebSiteName nvarchar(100) = null,
    @WebSiteDescription ntext = null,
    @URL ntext = null,
    @TrackingOnly tinyint = null,
    @DefaultSkinID int = null,
    @ParentAffiliateID int = null,
    @DisplayOrder int = null,
    @ExtensionData ntext = null,
    @SEName nvarchar(100) = null,
    @SETitle ntext = null,
    @SENoScript ntext = null,
    @SEAltText ntext = null,
    @SEKeywords ntext = null,
    @SEDescription ntext = null,
    @Published tinyint = null,
    @Wholesale tinyint = null,
    @Deleted tinyint = null,
    @SaltKey int = null
  
AS
SET NOCOUNT ON 

UPDATE dbo.Affiliate
SET 
    EMail = COALESCE(@EMail, EMail),
    Password = COALESCE(@Password, Password),
    DateOfBirth = COALESCE(@DateOfBirth, DateOfBirth),
    Gender = COALESCE(@Gender, Gender),
    Notes = COALESCE(@Notes, Notes),
    IsOnline = COALESCE(@IsOnline, IsOnline),
    FirstName = COALESCE(@FirstName, FirstName),
    LastName = COALESCE(@LastName, LastName),
    Name = COALESCE(@Name, Name),
    Company = COALESCE(@Company, Company),
    Address1 = COALESCE(@Address1, Address1),
    Address2 = COALESCE(@Address2, Address2),
    Suite = COALESCE(@Suite, Suite),
    City = COALESCE(@City, City),
    State = COALESCE(@State, State),
    Zip = COALESCE(@Zip, Zip),
    Country = COALESCE(@Country, Country),
    Phone = COALESCE(@Phone, Phone),
    WebSiteName = COALESCE(@WebSiteName, WebSiteName),
    WebSiteDescription = COALESCE(@WebSiteDescription, WebSiteDescription),
    URL = COALESCE(@URL, URL),
    TrackingOnly = COALESCE(@TrackingOnly, TrackingOnly),
    DefaultSkinID = COALESCE(@DefaultSkinID, DefaultSkinID),
    ParentAffiliateID = COALESCE(@ParentAffiliateID, ParentAffiliateID),
    DisplayOrder = COALESCE(@DisplayOrder, DisplayOrder),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData),
    SEName = COALESCE(@SEName, SEName),
    SETitle = COALESCE(@SETitle, SETitle),
    SENoScript = COALESCE(@SENoScript, SENoScript),
    SEAltText = COALESCE(@SEAltText, SEAltText),
    SEKeywords = COALESCE(@SEKeywords, SEKeywords),
    SEDescription = COALESCE(@SEDescription, SEDescription),
    Published = COALESCE(@Published, Published),
    Wholesale = COALESCE(@Wholesale, Wholesale),
    Deleted = COALESCE(@Deleted, Deleted),
    SaltKey = COALESCE(@SaltKey, SaltKey)
WHERE AffiliateID = @AffiliateID

GO
PRINT N'Creating [dbo].[aspdnsf_insStateTaxRate]'
GO


create proc [dbo].[aspdnsf_insStateTaxRate]
    @StateID int,
    @TaxClassID int,
    @TaxRate money,
    @StateTaxID int OUTPUT
  
AS
SET NOCOUNT ON 


insert dbo.StateTaxRate(StateID, TaxClassID, TaxRate, CreatedOn)
values (@StateID, @TaxClassID, @TaxRate, getdate())

set @StateTaxID = @@identity

GO
PRINT N'Creating [dbo].[aspdnsf_getAffiliateByEmail]'
GO



CREATE PROC dbo.aspdnsf_getAffiliateByEmail
    @AffiliateEmail nvarchar(100)
  
AS
SET NOCOUNT ON 


SELECT a.AffiliateID, AffiliateGUID, EMail, Password, DateOfBirth, Gender, Notes, IsOnline, FirstName, LastName, Name, Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL, TrackingOnly, 
DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SENoScript, SEAltText, SEKeywords, SEDescription, Published, Wholesale, Deleted, a.CreatedOn, SaltKey, StoreID
FROM dbo.Affiliate a with (nolock) left join AffiliateStore b with (nolock) on a.AffiliateID = b.AffiliateID
WHERE EMail = @AffiliateEmail

GO
PRINT N'Creating [dbo].[aspdnsf_updStateTaxRate]'
GO


create proc [dbo].[aspdnsf_updStateTaxRate]
    @StateTaxID int,
    @TaxRate money
  
AS
SET NOCOUNT ON 


UPDATE dbo.StateTaxRate
SET 
    TaxRate = COALESCE(@TaxRate, TaxRate)
WHERE StateTaxID = @StateTaxID

GO
PRINT N'Altering [dbo].[Feed]'
GO
ALTER TABLE [dbo].[Feed] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_Feed_StoreID] DEFAULT ((1))

GO


PRINT N'Creating [dbo].[aspdnsf_CreateFeed]'
GO


create proc [dbo].[aspdnsf_CreateFeed]
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


    
INSERT dbo.Feed(FeedGUID, Name, DisplayOrder, XmlPackage, CanAutoFTP, FTPUsername, FTPPassword, FTPServer, FTPPort, FTPFilename, ExtensionData, CreatedOn)
VALUES (newid(), @Name, isnull(@DisplayOrder,1), @XmlPackage, isnull(@CanAutoFTP,0), @FTPUsername, @FTPPassword, @FTPServer, @FTPPort, @FTPFilename, @ExtensionData, getdate())
set @FeedID = @@identity
END

GO

PRINT N'Creating [dbo].[aspdnsf_SelectZipTaxRatesAll]'
GO


CREATE PROCEDURE dbo.[aspdnsf_SelectZipTaxRatesAll]  
  
AS  
	  
SET NOCOUNT ON  
  
SELECT  
 ztr.[ZipCode],  
 ztr.[TaxRate],  
 ztr.[CreatedOn],  
 ztr.[ZipTaxID],  
 ztr.[TaxClassID],  
 ztr.[CountryId], 
 t.[Name] TaxClass  
FROM  
 dbo.[ZipTaxRate] ztr  
WITH (NOLOCK)
JOIN  
 dbo.[TaxClass] t ON ztr.TaxClassID = t.TaxClassID  

GO
PRINT N'Creating [dbo].[aspdnsf_DelFeed]'
GO


create proc [dbo].[aspdnsf_DelFeed]
    @FeedID int
  
AS
BEGIN
SET NOCOUNT ON 

    DELETE dbo.Feed WHERE FeedID = @FeedID

END
GO
PRINT N'Creating [dbo].[aspdnsf_getZipTaxRateByID]'
GO


create proc [dbo].[aspdnsf_getZipTaxRateByID]
    @ZipTaxID int
  
AS
SET NOCOUNT ON 


SELECT ztr.ZipTaxID, ztr.ZipCode, ztr.TaxClassID, ztr.TaxRate, ztr.CreatedOn, ztr.CountryId, t.Name TaxClass
FROM dbo.ZipTaxRate ztr with (nolock) join dbo.TaxClass t with (nolock) on ztr.TaxClassID = t.TaxClassID 
WHERE ztr.ZipTaxID = @ZipTaxID 
GO
PRINT N'Creating [dbo].[Store]'
GO
CREATE TABLE [dbo].[Store]
(
[StoreID] [int] NOT NULL IDENTITY(1, 1),
[StoreGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_Store_StoreGUID] DEFAULT (newid()),
[ProductionURI] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[StagingURI] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DevelopmentURI] [nvarchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Name] [nvarchar] (400) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Summary] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Description] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Published] [tinyint] NOT NULL CONSTRAINT [DF_Store_Published] DEFAULT ((0)),
[Deleted] [tinyint] NOT NULL CONSTRAINT [DF_Store_Deleted] DEFAULT ((0)),
[SkinID] [int] NOT NULL CONSTRAINT [DF_Store_SkinID] DEFAULT ((0)),
[IsDefault] [tinyint] NOT NULL CONSTRAINT [DF_Store_IsDefault] DEFAULT ((0)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_Store_Created] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_Store] on [dbo].[Store]'
GO
ALTER TABLE [dbo].[Store] ADD CONSTRAINT [PK_Store] PRIMARY KEY CLUSTERED  ([StoreID])

GO


PRINT N'Creating [dbo].[aspdnsf_GetFeed]'
GO


CREATE PROC [dbo].[aspdnsf_GetFeed]
    @FeedID int = null
  
AS
BEGIN
SET NOCOUNT ON 

SELECT FeedID, FeedGUID, a.StoreID, b.Name as StoreName, a.Name, DisplayOrder, XmlPackage, CanAutoFTP, FTPUsername, FTPPassword, FTPServer, FTPPort, FTPFilename, isnull(ExtensionData, '') ExtensionData, a.CreatedOn
FROM dbo.Feed a with (nolock) left join Store b with (nolock) on a.StoreID = b.StoreID
WHERE FeedID = COALESCE(@FeedID, FeedID) order by a.StoreID ASC

END
GO
PRINT N'Creating [dbo].[aspdnsf_getZipTaxRate]'
GO


create proc [dbo].[aspdnsf_getZipTaxRate]
    @ZipCode nvarchar(10) = null,
    @TaxClassID int = null
  
AS
SET NOCOUNT ON 


SELECT ztr.ZipTaxID, ztr.ZipCode, ztr.TaxClassID, ztr.TaxRate, ztr.CreatedOn, t.Name TaxClass
FROM dbo.ZipTaxRate ztr with (nolock) join dbo.TaxClass t with (nolock) on ztr.TaxClassID = t.TaxClassID 
WHERE ztr.ZipCode = COALESCE(@ZipCode, ztr.ZipCode) and ztr.TaxClassID = COALESCE(@TaxClassID, ztr.TaxClassID)

GO
PRINT N'Creating [dbo].[aspdnsf_UpdFeed]'
GO

create proc [dbo].[aspdnsf_UpdFeed]
    @FeedID int,
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
PRINT N'Creating [dbo].[aspdnsf_insZipTaxRate]'
GO


create proc [dbo].[aspdnsf_insZipTaxRate]
    @ZipCode nvarchar(10),
    @TaxClassID int,
    @TaxRate money,
    @CountryId int
  
AS
SET NOCOUNT ON 


insert dbo.ZipTaxRate(ZipCode, TaxClassID, TaxRate, CreatedOn, CountryId)
values (@ZipCode, @TaxClassID, @TaxRate, getdate(), @CountryId)

SELECT SCOPE_IDENTITY()

GO

PRINT N'Creating [dbo].[NewsStore]'
GO
CREATE TABLE [dbo].[NewsStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[NewsID] [int] NOT NULL CONSTRAINT [DF_NewsStore_NewsID] DEFAULT ((0)),
[StoreID] [int] NOT NULL CONSTRAINT [DF_NewsStore_StoreID] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_NewsStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_NewsStore] on [dbo].[NewsStore]'
GO
ALTER TABLE [dbo].[NewsStore] ADD CONSTRAINT [PK_NewsStore] PRIMARY KEY CLUSTERED  ([NewsID], [StoreID])

GO


PRINT N'Altering [dbo].[News]'
GO
ALTER TABLE [dbo].[News] ALTER COLUMN [Headline] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL

GO


PRINT N'Creating [dbo].[aspdnsf_DropAllNonPrimaryIndexes]'
GO


create proc [dbo].[aspdnsf_DropAllNonPrimaryIndexes] 
  
as
BEGIN
declare @sql varchar(8000)
    SELECT  
        TABLE_NAME = OBJECT_NAME(i.id), 
        INDEX_NAME = i.name, 
        COLUMN_LIST = dbo.GetIndexColumns(OBJECT_NAME(i.id), i.id, i.indid), 
        IS_CLUSTERED = INDEXPROPERTY(i.id, i.name, 'IsClustered'), 
        IS_UNIQUE = INDEXPROPERTY(i.id, i.name, 'IsUnique'), 
        FILE_GROUP = g.GroupName 
    INTO #AllIndexes
    FROM 
        dbo.sysindexes i 
    INNER join dbo.sysfilegroups g 
    ON 
        i.groupid = g.groupid 
    WHERE 
        (i.indid BETWEEN 1 AND 254) 
        -- leave out AUTO_STATISTICS: 
        AND (i.Status & 64)=0 
        -- leave out system tables: 
        AND OBJECTPROPERTY(i.id, 'IsMsShipped') = 0 

    DECLARE @MyCursor CURSOR   

    SET @MyCursor = CURSOR FAST_FORWARD 
    FOR 
    SELECT  
        CASE WHEN T.TABLE_NAME IS NULL THEN 
            CASE WHEN IS_UNIQUE=1 THEN
                'ALTER TABLE [' + v.TABLE_NAME + '] DROP CONSTRAINT ' + INDEX_NAME
            ELSE
                'DROP INDEX [' + v.TABLE_NAME + '].[' + INDEX_NAME + ']' 
            END
        END 
    FROM 
        #AllIndexes v 
    LEFT OUTER join 
        INFORMATION_SCHEMA.TABLE_CONSTRAINTS T  
    ON 
        T.CONSTRAINT_NAME = v.INDEX_NAME 
        AND T.TABLE_NAME = v.TABLE_NAME  
        AND T.CONSTRAINT_TYPE = 'PRIMARY KEY' 
    where INDEX_Name like 'IX_%'
    ORDER BY 
        v.TABLE_NAME, 
        IS_CLUSTERED DESC

    OPEN @MyCursor 
    FETCH NEXT FROM @MyCursor 
    INTO @sql

       WHILE @@FETCH_STATUS = 0 
       BEGIN 
          exec(@sql)
          FETCH NEXT FROM @MyCursor 
          INTO @sql
       END 

    CLOSE @MyCursor 
    DEALLOCATE @MyCursor 

END

GO
PRINT N'Creating [dbo].[aspdnsf_DropAndRecreateAllNonPrimaryIndexes]'
GO


create proc [dbo].[aspdnsf_DropAndRecreateAllNonPrimaryIndexes] 
  
as
BEGIN
    exec dbo.aspdnsf_DropAllNonPrimaryIndexes
    exec dbo.aspdnsf_CreateIndexes
END
GO
PRINT N'Altering [dbo].[Customer]'
GO
ALTER TABLE [dbo].[Customer] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_Customer_StoreID] DEFAULT ((1))

GO


PRINT N'Creating [dbo].[aspdnsf_insCustomer]'
GO


create proc [dbo].[aspdnsf_insCustomer]
    @Email nvarchar(100),
    @Password nvarchar(100),
    @SkinID int,
    @AffiliateID int,
    @Referrer ntext,
    @IsAdmin tinyint,
    @LastIPAddress varchar(40),
    @LocaleSetting nvarchar(10),
    @Over13Checked tinyint,
    @CurrencySetting nvarchar(10),
    @VATSetting int,
    @VATRegistrationID nvarchar(100),
    @CustomerLevelID int,
    @CustomerID int OUTPUT
  
AS
SET NOCOUNT ON
 

insert dbo.Customer(CustomerGUID, CustomerLevelID, RegisterDate, Email, Password, DateOfBirth, Gender, FirstName, LastName, Notes, SkinID, Phone, AffiliateID, Referrer, CouponCode, OkToEmail, IsAdmin, BillingEqualsShipping, LastIPAddress, OrderNotes, SubscriptionExpiresOn, RTShipRequest, RTShipResponse, OrderOptions, LocaleSetting, MicroPayBalance, RecurringShippingMethodID, RecurringShippingMethod, BillingAddressID, ShippingAddressID, GiftRegistryGUID, GiftRegistryIsAnonymous, GiftRegistryAllowSearchByOthers, GiftRegistryNickName, GiftRegistryHideShippingAddresses, CODCompanyCheckAllowed, CODNet30Allowed, ExtensionData, FinalizationData, Deleted, CreatedOn, Over13Checked, CurrencySetting, VATSetting, VATRegistrationID, StoreCCInDB, IsRegistered, LockedUntil, AdminCanViewCC, PwdChanged, BadLoginCount, LastBadLogin, Active, PwdChangeRequired, SaltKey)
values
(
    newid(),
    @CustomerLevelID,
    getdate(),
    isnull(@Email, ''),
    isnull(@Password, ''),
    null,
    null,
    null,
    null,
    null,
    isnull(@SkinID, 1),
    null,
    @AffiliateID,
    @Referrer,
    null,
    1,
    isnull(@IsAdmin, 0),
    0,
    @LastIPAddress,
    null,
    null,
    null,
    null,
    null,
    isnull(@LocaleSetting, ('en-US')),
    0.0,
    1,
    null,
    null,
    null,
    newid(),
    1,
    1,
    null,
    1,
    0,
    0,
    null,
    null,
    0,
    getdate(),
    @Over13Checked,
    @CurrencySetting,
    @VATSetting,
    @VATRegistrationID,
    1,
    0,
    null,
    1,
    getdate(),
    0,
    null,
    1,
    0,
    0
)
 
set @CustomerID = @@identity
 

GO

PRINT N'Altering [dbo].[ShoppingCart]'
GO
ALTER TABLE [dbo].[ShoppingCart] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShoppingCart_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[RatingCommentHelpfulness]'
GO
ALTER TABLE [dbo].[RatingCommentHelpfulness] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF__RatingCom__Store__4277DAAA] DEFAULT ((1))

GO


PRINT N'Creating primary key [PK_Comment_Helpfulness] on [dbo].[RatingCommentHelpfulness]'
GO
ALTER TABLE [dbo].[RatingCommentHelpfulness] ADD CONSTRAINT [PK_Comment_Helpfulness] PRIMARY KEY CLUSTERED  ([StoreID], [ProductID], [RatingCustomerID], [VotingCustomerID])

GO


PRINT N'Creating index [IX_RatingCommentHelpfulness_StoreID] on [dbo].[RatingCommentHelpfulness]'
GO
CREATE NONCLUSTERED INDEX [IX_RatingCommentHelpfulness_StoreID] ON [dbo].[RatingCommentHelpfulness] ([StoreID])

GO


PRINT N'Altering [dbo].[Rating]'
GO
ALTER TABLE [dbo].[Rating] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_Rating_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[ProductVariant]'
GO
if not exists ( select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='ProductVariant' and COLUMN_NAME='Condition' )
BEGIN
	ALTER TABLE [dbo].[ProductVariant] ADD
	[Condition] [tinyint] NOT NULL CONSTRAINT [DF_ProductVariant_Condition] DEFAULT ((0))
END

GO


PRINT N'Creating [dbo].[aspdnsf_GetCustomerByGUID]'
GO


create proc [dbo].[aspdnsf_GetCustomerByGUID]
    @CustomerGUID uniqueidentifier
  
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
              WHERE c.CustomerGUID = @CustomerGUID and s.LoggedOut is null and s.LastActivity >= dateadd(mi, -@intSessionTimeOut, getdate())) a on cs.CustomerSessionID = a.CustomerSessionID


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
    WHERE c.Deleted=0 and c.CustomerGUID = @CustomerGUID
END
GO
PRINT N'Altering [dbo].[Orders_ShoppingCart]'
GO
ALTER TABLE [dbo].[Orders_ShoppingCart] ALTER COLUMN [OrderedProductQuantityDiscountName] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL

GO


PRINT N'Creating [dbo].[aspdnsf_GetCustomerByEmail]'
GO


CREATE PROC [dbo].[aspdnsf_GetCustomerByEmail]
    @Email nvarchar(100),
    @filtercustomer bit,
    @StoreID int = 1
    
  
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
    WHERE c.Deleted=0 and c.Email = @Email and ((@filtercustomer = 0 or IsAdmin > 0) or c.StoreID = @StoreID)
END

GO

PRINT N'Altering [dbo].[ShippingMethod]'
GO
ALTER TABLE [dbo].[ShippingMethod] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingMethod_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[GiftCard]'
GO
ALTER TABLE [dbo].[GiftCard] ADD
[StartDate] [datetime] NOT NULL CONSTRAINT [DF_GiftCard_StartDate] DEFAULT (getdate())

GO


PRINT N'Altering [dbo].[Coupon]'
GO
ALTER TABLE [dbo].[Coupon] ADD
[StartDate] [datetime] NOT NULL CONSTRAINT [DF_Coupon_StartDate] DEFAULT (getdate())

GO


PRINT N'Creating [dbo].[aspdnsf_updCustomer]'
GO


CREATE proc [dbo].[aspdnsf_updCustomer]
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
    @StoreID	int =1
    
  
AS
SET NOCOUNT ON 

DECLARE @OldPwd nvarchar(100), @IsAdminCust tinyint, @OldSaltKey int

SELECT @OldPwd = Password, @IsAdminCust = IsAdmin, @OldSaltKey = Saltkey FROM dbo.Customer WITH (NOLOCK) WHERE CustomerID = @CustomerID


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
PRINT N'Creating [dbo].[EntityStore]'
GO
CREATE TABLE [dbo].[EntityStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[StoreID] [int] NOT NULL CONSTRAINT [DF_EntityStore_StoreID] DEFAULT ((0)),
[EntityID] [int] NOT NULL CONSTRAINT [DF_EntityStore_EntityID] DEFAULT ((0)),
[EntityType] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_EntityStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_EntityStore] on [dbo].[EntityStore]'
GO
ALTER TABLE [dbo].[EntityStore] ADD CONSTRAINT [PK_EntityStore] PRIMARY KEY CLUSTERED  ([StoreID], [EntityID], [EntityType])

GO


PRINT N'Creating [dbo].[aspdnsf_updCustomerByEmail]'
GO


CREATE proc [dbo].[aspdnsf_updCustomerByEmail]
    @Email nvarchar(100),
    @CustomerLevelID int = null,
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
    @BadLogin smallint = 0 , --only pass -1 = null, 0 = null, or 1: -1 clears the field = null, 0 does nothing = null, 1 increments the field by one
    @Active tinyint = null,
    @PwdChangeRequired tinyint = null,
    @RequestedPaymentMethod  nvarchar(100) = null
  
AS
SET NOCOUNT ON 

DECLARE @CustomerID int, @OldPwd nvarchar(100), @IsAdminCust tinyint, @OldSaltKey int

SELECT @CustomerID = CustomerID , @OldPwd = Password, @IsAdminCust = IsAdmin, @OldSaltKey = Saltkey FROM dbo.Customer WITH (NOLOCK) WHERE Email = @Email 


UPDATE dbo.Customer
SET 
    CustomerLevelID = COALESCE(@CustomerLevelID, CustomerLevelID),
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
    RequestedPaymentMethod = COALESCE(@RequestedPaymentMethod, RequestedPaymentMethod)
WHERE Email = @Email

IF @IsAdminCust = 1 and @OldPwd <> @Password
    INSERT dbo.PasswordLog (CustomerID, OldPwd, SaltKey, ChangeDt)
    VALUES (@CustomerID, @OldPwd, @OldSaltKey, getdate())
GO
PRINT N'Creating [dbo].[aspdnsf_SessionGetByCustomerID]'
GO


create proc [dbo].[aspdnsf_SessionGetByCustomerID]
    @CustomerID int
  
AS
SET NOCOUNT ON 

DECLARE @CustomerSessionID int, @SessionTimeOut varchar(10), @intSessionTimeOut int
SELECT @SessionTimeOut = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [Name] = 'SessionTimeoutInMinutes'

IF ISNUMERIC(@SessionTimeOut) = 1
    set @intSessionTimeOut = convert(int, @SessionTimeOut)
ELSE
    set @intSessionTimeOut = 60


select @CustomerSessionID = max(CustomerSessionID)
from dbo.Customersession with (nolock) 
WHERE CustomerID = @CustomerID and LoggedOut is null and LastActivity > dateadd(mi, -@intSessionTimeOut, getdate())

UPDATE dbo.Customersession 
SET LastActivity = getdate()
WHERE CustomerSessionID = @CustomerSessionID

SELECT cs.CustomerSessionID, cs.CustomerSessionGUID, cs.CustomerID, cs.SessionName, cs.SessionValue, cs.CreatedOn, cs.ExpiresOn, cs.ipaddr, cs.LastActivity, cs.LoggedOut
FROM dbo.Customersession cs with (nolock) 
WHERE CustomerSessionID = @CustomerSessionID

DELETE dbo.Customersession WHERE CustomerID = @CustomerID and CustomersessionID <> @CustomerSessionID 
GO
PRINT N'Creating [dbo].[aspdnsf_SessionGetByGUID]'
GO



create proc [dbo].[aspdnsf_SessionGetByGUID]
    @CustomerSessionGUID uniqueidentifier
  
AS
SET NOCOUNT ON 

DECLARE @CustomerSessionID int, @SessionTimeOut varchar(10), @intSessionTimeOut int
SELECT @SessionTimeOut = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [Name] = 'SessionTimeoutInMinutes'

IF ISNUMERIC(@SessionTimeOut) = 1
    set @intSessionTimeOut = convert(int, @SessionTimeOut)
ELSE
    set @intSessionTimeOut = 60

UPDATE dbo.Customersession 
SET LastActivity = getdate()
WHERE CustomerSessionGUID = @CustomerSessionGUID


SELECT CustomerSessionID, CustomerSessionGUID, CustomerID, SessionName, SessionValue, CreatedOn, ExpiresOn, ipaddr, LastActivity, LoggedOut
FROM dbo.Customersession WITH (NOLOCK)
WHERE CustomerSessionGUID = @CustomerSessionGUID and LoggedOut is null and LastActivity > dateadd(mi, -@intSessionTimeOut, getdate())

GO
PRINT N'Creating [dbo].[aspdnsf_CloneStore]'
GO


CREATE PROC [dbo].[aspdnsf_CloneStore]
	@StoreID INT,
	@NewStoreName nvarchar(400) = NULL,
	@NewStoreID int output
AS
BEGIN
	INSERT INTO Store (ProductionURI, StagingURI, DevelopmentURI, [Name], Description, SkinID)
	SELECT ProductionURI, StagingURI, DevelopmentURI, ISNULL(@NewStoreName, [Name]), Description, SkinID FROM Store WHERE StoreID = @StoreID
	
	select @NewStoreID=Max(StoreID) FROM Store 

	EXEC [aspdnsf_CloneStoreMappings] @StoreID, @NewStoreID
	
END
GO
PRINT N'Creating [dbo].[aspdnsf_SessionGetByID]'
GO




create proc [dbo].[aspdnsf_SessionGetByID]
    @CustomerSessionID int
  
AS
SET NOCOUNT ON 

DECLARE @SessionTimeOut varchar(10), @intSessionTimeOut int
SELECT @SessionTimeOut = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [Name] = 'SessionTimeoutInMinutes'

IF ISNUMERIC(@SessionTimeOut) = 1
    set @intSessionTimeOut = convert(int, @SessionTimeOut)
ELSE
    set @intSessionTimeOut = 60

UPDATE dbo.Customersession 
SET LastActivity = getdate()
WHERE CustomerSessionID = @CustomerSessionID


SELECT CustomerSessionID, CustomerSessionGUID, CustomerID, SessionName, SessionValue, CreatedOn, ExpiresOn, ipaddr, LastActivity, LoggedOut
FROM dbo.Customersession WITH (NOLOCK)
WHERE CustomerSessionID = @CustomerSessionID and LoggedOut is null and LastActivity > dateadd(mi, -@intSessionTimeOut, getdate())

GO
PRINT N'Creating [dbo].[aspdnsf_SessionInsert]'
GO


create proc [dbo].[aspdnsf_SessionInsert]
    @CustomerID int,
    @SessionValue ntext,
    @ipaddr varchar(15),
    @CustomerSessionID int OUTPUT
  
AS
SET NOCOUNT ON 

DECLARE @CustomerSessionGUID uniqueidentifier

set @CustomerSessionGUID = newid()
insert dbo.Customersession(CustomerID, SessionName, SessionValue, CreatedOn, ipaddr, LastActivity, CustomerSessionGUID)
values (@CustomerID, '', isnull(@SessionValue, ''), getdate(), @ipaddr, getdate(), @CustomerSessionGUID)

set @CustomerSessionID = @@identity

DELETE dbo.Customersession WHERE CustomerID = @CustomerID and CustomersessionID <> @CustomerSessionID 

GO
PRINT N'Creating [dbo].[aspdnsf_SessionAge]'
GO


create proc [dbo].[aspdnsf_SessionAge]
    @CustomerID int = null
  
AS
SET NOCOUNT ON 

DECLARE @SessionTimeOut varchar(10), @intSessionTimeOut int
SELECT @SessionTimeOut = ConfigValue FROM dbo.AppConfig with (nolock) WHERE [Name] = 'SessionTimeoutInMinutes'

IF ISNUMERIC(@SessionTimeOut) = 1
    set @intSessionTimeOut = convert(int, @SessionTimeOut)
ELSE
    set @intSessionTimeOut = 60

DELETE dbo.Customersession WHERE CustomerID = coalesce(@CustomerID, CustomerID) and  (LoggedOut is not null or LastActivity <= dateadd(mi, -@intSessionTimeOut, getdate()))
GO
PRINT N'Altering [dbo].[ShippingZone]'
GO
ALTER TABLE [dbo].[ShippingZone] ADD
[CountryID] [int] NULL

GO


PRINT N'Altering [dbo].[Orders]'
GO
ALTER TABLE [dbo].[Orders] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_Orders_StoreID] DEFAULT ((1))

GO


PRINT N'Creating [dbo].[aspdnsf_SessionUpdate]'
GO


CREATE proc [dbo].[aspdnsf_SessionUpdate]
    @CustomerSessionID int,
    @SessionName nvarchar(100),
    @SessionValue ntext,
    @ExpiresOn datetime,
    @LoggedOut datetime
  
AS
SET NOCOUNT ON 


UPDATE dbo.Customersession
SET 
    SessionName = COALESCE(@SessionName, SessionName),
    SessionValue = COALESCE(@SessionValue, SessionValue),
    ExpiresOn = COALESCE(@ExpiresOn, ExpiresOn),
    LastActivity = getdate(),
    LoggedOut = COALESCE(@LoggedOut, LoggedOut)
WHERE CustomerSessionID = @CustomerSessionID


SELECT CustomerSessionID, CustomerID, SessionName, SessionValue, CreatedOn, ExpiresOn, ipaddr, LastActivity, CustomerSessionGUID
FROM dbo.Customersession WITH (NOLOCK)
WHERE CustomerSessionID = @CustomerSessionID 
GO
PRINT N'Creating [dbo].[ProductStore]'
GO
CREATE TABLE [dbo].[ProductStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[ProductID] [int] NOT NULL CONSTRAINT [DF_ProductStore_ProductID] DEFAULT ((0)),
[StoreID] [int] NOT NULL CONSTRAINT [DF_ProductStore_StoreID] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_ProductStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_ProductStore] on [dbo].[ProductStore]'
GO
ALTER TABLE [dbo].[ProductStore] ADD CONSTRAINT [PK_ProductStore] PRIMARY KEY CLUSTERED  ([ProductID], [StoreID])

GO


PRINT N'Creating [dbo].[aspdnsf_GetCustomersRelatedProducts]'
GO



CREATE PROCEDURE [dbo].[aspdnsf_GetCustomersRelatedProducts]
	@CustomerViewID		nvarchar(50),
	@ProductID			int,
	@CustomerLevelID	int,
	@InvFilter			int,
	@affiliateID		int,
	@storeID			int = 1,
	@filterProduct		bit = 0
	
  
AS
SET NOCOUNT ON 

DECLARE 
	@custlevelcount int, 
	@CustomerLevelFilteringIsAscending bit, 
	@FilterProductsByCustomerLevel tinyint, 
	@relatedprods varchar(8000),
	@DynamicProductsEnabled varchar(10),
	@DynamicProductsDisplayed int,
	@FilterProductsByAffiliate tinyint,
	@affiliatecount int,
	@AffiliateExists int
	

SELECT @custlevelcount = si.rows from dbo.sysobjects so with (nolock) join dbo.sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductCustomerLevel') and si.indid < 2 and type = 'u'
SELECT @FilterProductsByCustomerLevel = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel'
SELECT @FilterProductsByAffiliate = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate'
SELECT @affiliatecount = count(*),  @AffiliateExists = sum(case when AffiliateID = @affiliateID then 1 else 0 end) from dbo.ProductAffiliate with (nolock) where ProductID = @ProductID


SET @CustomerLevelFilteringIsAscending  = 0
SELECT @CustomerLevelFilteringIsAscending  = case configvalue when 'true' then 1 else 0 end
FROM dbo.appConfig with (nolock)
WHERE name like 'FilterByCustomerLevelIsAscending'	

SELECT @DynamicProductsDisplayed = CAST(ConfigValue AS INT) from AppConfig with (NOLOCK) where Name = 'DynamicRelatedProducts.NumberDisplayed'
SELECT @DynamicProductsEnabled = CASE ConfigValue WHEN 'true' then 1 else 0 end from AppConfig with (NOLOCK) where Name = 'DynamicRelatedProducts.Enabled'
select @relatedprods = replace(cast(relatedproducts as varchar(8000)), ' ', '') from dbo.product with (NOLOCK) where productid = @productid

IF(@DynamicProductsEnabled = 1 and @DynamicProductsDisplayed > 0)
BEGIN
SELECT TOP (@DynamicProductsDisplayed) tp.ProductID, tp.ProductGUID, tp.ImageFilenameOverride, tp.SKU, tp.SEAltText, tp.Name, tp.Description  FROM Product tp with (NOLOCK) JOIN
(
SELECT p.ProductID, p.ProductGUID, p.ImageFilenameOverride, p.SKU, p.SEAltText, p.Name, p.Description  
from dbo.product p with (nolock) 
    join dbo.split(@relatedprods, ',') rp on p.productid = cast(rp.items as int) 
    left join dbo.productcustomerlevel pcl with (nolock) on p.productid = pcl.productid and @FilterProductsByCustomerLevel = 1
    join (select p.ProductID
          from dbo.product p  with (nolock)
              join dbo.split(@relatedprods, ',') rp on p.productid = cast(rp.items as int) 
              join (select ProductID, sum(Inventory) Inventory from dbo.productvariant with (nolock) group by ProductID) pv on p.ProductID = pv.ProductID
              left join (select ProductID, sum(quan) inventory from dbo.inventory i1 with (nolock) join dbo.productvariant pv1 with (nolock) on pv1.variantid = i1.variantid join dbo.split(@relatedprods, ',') rp1 on pv1.productid = cast(rp1.items as int) group by pv1.productid) i on i.productid = p.productid
          where case p.TrackInventoryBySizeAndColor when 1 then isnull(i.inventory, 0) else pv.inventory end >= @InvFilter
          ) tp on p.productid = tp.productid
where published = 1 and deleted = 0 and p.productid != @productid
	and GETDATE() BETWEEN ISNULL(AvailableStartDate, '1/1/1900') AND ISNULL(AvailableStopDate, '1/1/2999')
    and case 
         when @FilterProductsByCustomerLevel = 0 then 1
         when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID or pcl.CustomerLevelID is null then 1 
         when @CustomerLevelID=0 and pcl.CustomerLevelID is null then 1
         when @CustomerLevelID is null or @custlevelcount = 0 then 1 
         when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1 
         else 0
        end  = 1
UNION ALL	
SELECT pr.ProductID, pr.ProductGUID, pr.ImageFilenameOverride, pr.SKU, pr.SEAltText, pr.Name, pr.Description 
FROM Product pr WITH (NOLOCK)
WHERE pr.ProductID in (
	SELECT TOP 100 PERCENT p.ProductID FROM Product p with (NOLOCK) 
	JOIN 
	(
		SELECT ProductID FROM ProductView with (NOLOCK) WHERE CustomerViewID 
		IN 
		(
		SELECT CustomerViewID FROM ProductView with (NOLOCK)
		WHERE ProductID = @ProductID AND CustomerViewID <> @CustomerViewID		
		)
		AND ProductID <> @ProductID
		AND ProductID NOT 
		IN
		(
		select ProductID 
		from product with (NOLOCK) 
		join split(@relatedprods, ',') rp on productid = cast(rp.items as int) 
		group by ProductID  		
		)
	) a on p.ProductID = a.ProductID
	
	
	LEFT JOIN dbo.productcustomerlevel pcl with (NOLOCK) on p.productid = pcl.productid and @FilterProductsByCustomerLevel = 1
	left join dbo.ProductAffiliate     pa  with (nolock) on p.ProductID = pa.ProductID 	    
	WHERE 
	Published = 1 AND Deleted = 0
	and GETDATE() BETWEEN ISNULL(AvailableStartDate, '1/1/1900') AND ISNULL(AvailableStopDate, '1/1/2999')
	and case 
         when @FilterProductsByCustomerLevel = 0 then 1
         when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID or pcl.CustomerLevelID is null then 1 
         when @CustomerLevelID=0 and pcl.CustomerLevelID is null then 1
         when @CustomerLevelID is null or @custlevelcount = 0 then 1 
         when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1 
         else 0
        end  = 1 
    and (pa.AffiliateID = @affiliateID or pa.AffiliateID is null or @affiliatecount = 0 or @FilterProductsByAffiliate = 0)		
	group by p.ProductID
	order by COUNT(*) desc		
)
)prd on tp.ProductID = prd.ProductID
inner join (select distinct a.ProductID from Product a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) ps on tp.ProductID = ps.ProductID
END

IF(@DynamicProductsEnabled = 0 and @DynamicProductsDisplayed > 0)
BEGIN 
select TOP (@DynamicProductsDisplayed) p.ProductID, p.ProductGUID, p.ImageFilenameOverride, p.SKU, p.SEAltText, p.Name, p.Description  
from dbo.product p with (nolock) 
    join dbo.split(@relatedprods, ',') rp on p.productid = cast(rp.items as int) 
    left join dbo.productcustomerlevel pcl with (nolock) on p.productid = pcl.productid and @FilterProductsByCustomerLevel = 1
    join (select p.ProductID
          from dbo.product p  with (nolock)
              join dbo.split(@relatedprods, ',') rp on p.productid = cast(rp.items as int) 
              join (select ProductID, sum(Inventory) Inventory from dbo.productvariant with (nolock) group by ProductID) pv on p.ProductID = pv.ProductID
              left join (select ProductID, sum(quan) inventory from dbo.inventory i1 with (nolock) join dbo.productvariant pv1 with (nolock) on pv1.variantid = i1.variantid join dbo.split(@relatedprods, ',') rp1 on pv1.productid = cast(rp1.items as int) group by pv1.productid) i on i.productid = p.productid
          where case p.TrackInventoryBySizeAndColor when 1 then isnull(i.inventory, 0) else pv.inventory end >= @InvFilter
          ) tp on p.productid = tp.productid
	inner join (select distinct a.ProductID from Product a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) ps on p.ProductID = ps.ProductID
where published = 1 and deleted = 0 and p.productid != @productid
and GETDATE() BETWEEN ISNULL(AvailableStartDate, '1/1/1900') AND ISNULL(AvailableStopDate, '1/1/2999')
    and case 
         when @FilterProductsByCustomerLevel = 0 then 1
         when @CustomerLevelFilteringIsAscending = 1 and pcl.CustomerLevelID <= @CustomerLevelID or pcl.CustomerLevelID is null then 1 
         when @CustomerLevelID=0 and pcl.CustomerLevelID is null then 1
         when @CustomerLevelID is null or @custlevelcount = 0 then 1 
         when pcl.CustomerLevelID = @CustomerLevelID or pcl.CustomerLevelID is null then 1 
         else 0
        end  = 1
END
GO
PRINT N'Creating [dbo].[TopicMapping]'
GO
CREATE TABLE [dbo].[TopicMapping]
(
[TopicID] [int] NOT NULL,
[ParentTopicID] [int] NOT NULL,
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_TopicMapping_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating [dbo].[aspdnsf_SaveTopicMap]'
GO

create procedure [dbo].[aspdnsf_SaveTopicMap]
	@TopicID INT,
	@ParentTopicID INT,
	@Map BIT

AS
BEGIN
	-- Add Mapping Information
	if(@map = 1)
	begin
	    if not exists(select * from dbo.TopicMapping WITH (NOLOCK) where TopicID=@TopicID and ParentTopicID=@ParentTopicID and @TopicID<>@ParentTopicID)
	    begin
			INSERT INTO dbo.TopicMapping(TopicID, ParentTopicID) VALUES (@TopicID, @ParentTopicID)
		end
	end
	-- Remove Mapping Information if any
	else if (@map = 0)
	begin
		DELETE FROM dbo.TopicMapping WHERE TopicID=@TopicID AND ParentTopicID=@ParentTopicID
	end
END

GO

PRINT N'Creating [dbo].[aspdnsf_GetRecentlyViewedProducts]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_GetRecentlyViewedProducts]
	@productID							int, 
	@CustomerViewID						nvarchar(50),
	@invFilter							int,
	@recentlyViewedProductsNumToDisplay int,
	@storeID							int = 1,
	@filterProduct						bit = 0

      
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @HideProductsWithLessThanThisInventoryLevel int,
			@RecentlyViewedProductsEnabled varchar(10)
	
    -- Insert statements for procedure here
    SELECT @RecentlyViewedProductsEnabled = CASE ConfigValue WHEN 'true' THEN 1 ELSE 0 END FROM dbo.AppConfig WITH (NOLOCK) WHERE Name = 'RecentlyViewedProducts.Enabled'
    SELECT @HideProductsWithLessThanThisInventoryLevel = CONVERT(int, ConfigValue) FROM dbo.AppConfig WITH (NOLOCK) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' AND ISNUMERIC(ConfigValue) = 1
    
    SELECT @RecentlyViewedProductsEnabled = CASE configvalue WHEN 'true' then 1 else 0 end FROM dbo.AppConfig WITH (NOLOCK)WHERE Name = 'RecentlyViewedProducts.Enabled'

    IF (@HideProductsWithLessThanThisInventoryLevel > @invFilter or @HideProductsWithLessThanThisInventoryLevel = -1) and @invFilter <> -1
        SET @invFilter = @HideProductsWithLessThanThisInventoryLevel 
    
    if(@RecentlyViewedProductsEnabled = 1)
    BEGIN
	SELECT TOP (@recentlyViewedProductsNumToDisplay) pv.ProductID, pr.Name,pr.ImageFilenameOverride, pr.SEName,pr.SKU
	FROM ProductView pv WITH (NOLOCK) 
		 INNER JOIN Product pr WITH (NOLOCK) ON pv.ProductID = pr.ProductID 
		 INNER JOIN dbo.productvariant pvt WITH (NOLOCK) ON pv.ProductID = pvt.ProductID and pvt.IsDefault =1   
		 LEFT JOIN (SELECT variantid, SUM(quan) inventory FROM inventory WITH (NOLOCK) GROUP BY variantid) i ON pvt.variantid = i.variantid 	
		 inner join (select distinct a.ProductID from Product a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) ps on pv.ProductID = ps.ProductID
	WHERE @RecentlyViewedProductsEnabled = 1 AND
		  pr.Deleted = 0  AND 
		  pvt.Deleted = 0 AND 
		  pr.published = 1 AND 
		  pvt.published = 1 AND
		  pv.CustomerViewID = @CustomerViewID AND 
	      pv.ProductID <> @productID AND  
	      pr.ProductID <> @productID AND
		  pvt.ProductID <> @productID AND
		  pv.ViewDate <> GETDATE() AND 
		                 GETDATE() BETWEEN ISNULL(pr.AvailableStartDate, '1/1/1900') AND 
		                 ISNULL(pr.AvailableStopDate, '1/1/2999')AND  
		                 (CASE pr.TrackInventoryBySizeAndColor WHEN 1 THEN isnull(i.inventory, 0) 
																	  ELSE pvt.Inventory END >= @InvFilter OR @InvFilter = -1)	
	 
	ORDER BY pv.ViewDate DESC
	END
END
GO
PRINT N'Creating [dbo].[aspdnsf_FindCircularReference]'
GO


create proc [dbo].[aspdnsf_FindCircularReference]
  
as
SET NOCOUNT ON

DECLARE @categorypath table(catid int, parentID int)
SELECT c1.categoryid into #tmp FROM dbo.category c1 with (nolock) left join [dbo].category c2 with (nolock) on c1.categoryid = c2.ParentCategoryID WHERE c2.ParentCategoryID is null

DECLARE @StartCatID int, @loopCatID int, @prevloopCatID int, @parentCatID int, @catname nvarchar(200)

SELECT top 1 @StartCatID = categoryid from #tmp
WHILE @@rowcount > 0 BEGIN
    SELECT @loopCatID = ParentCategoryID, @catname = [name] from dbo.category with (nolock) WHERE categoryid = @StartCatID
    INSERT @categorypath values(@StartCatID, @loopCatID)
    WHILE @@rowcount > 0 begin
        IF exists (select * from @categorypath where catid = @loopCatID) BEGIN
            INSERT @categorypath select * from @categorypath where catid = @loopCatID
            PRINT 'circular reference found'
            SELECT cp.catid CategoryID, cp.parentID, c.name [Category Name] FROM @categorypath cp join [dbo].category c on cp.catid = c.CategoryID
            PRINT ''
            BREAK
        END

        IF @loopCatID = 0
            BREAK

        SET @prevloopCatID = @loopCatID 
        SELECT @loopCatID = ParentCategoryID, @catname = [name] FROM dbo.category with (nolock) WHERE categoryid = @loopCatID 
        INSERT @categorypath VALUES(@prevloopCatID, @loopCatID)
    end
    DELETE #tmp where categoryid = @StartCatID 
    DELETE @categorypath
    SELECT top 1 @StartCatID = categoryid from #tmp
END

drop table #tmp
GO
PRINT N'Creating [dbo].[aspdnsf_GetVariantsPaged]'
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
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductCategory WITH (NOLOCK) WHERE CategoryID = @EntityFilterID
	IF @EntityFilterType = 2
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductSection WITH (NOLOCK) WHERE SectionID = @EntityFilterID
	IF @EntityFilterType = 3
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductManufacturer WITH (NOLOCK) WHERE ManufacturerID = @EntityFilterID
	IF @EntityFilterType = 4
		INSERT INTO @Filter (productID) SELECT ProductID FROM ProductDistributor WITH (NOLOCK) WHERE DistributorID = @EntityFilterID
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


PRINT N'Creating [dbo].[aspdnsf_ClearAllImportFlags]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_ClearAllImportFlags]
  
as
BEGIN
    SET NOCOUNT ON 
    update [dbo].productvariant set IsImport=0;
    update [dbo].product set IsImport=0;
    update [dbo].manufacturer set IsImport=0;
    update [dbo].category set IsImport=0;
    update [dbo].section set IsImport=0;
    update [dbo].library set IsImport=0;
    update [dbo].Document set IsImport=0;
END
GO
PRINT N'Creating [dbo].[TopicStore]'
GO
CREATE TABLE [dbo].[TopicStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[TopicID] [int] NOT NULL CONSTRAINT [DF_TopicStore_TopicID] DEFAULT ((0)),
[StoreID] [int] NOT NULL CONSTRAINT [DF_TopicStore_StoreID] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_TopicStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_TopicStore] on [dbo].[TopicStore]'
GO
ALTER TABLE [dbo].[TopicStore] ADD CONSTRAINT [PK_TopicStore] PRIMARY KEY CLUSTERED  ([TopicID], [StoreID])

GO


PRINT N'Creating [dbo].[ShippingMethodStore]'
GO
CREATE TABLE [dbo].[ShippingMethodStore]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[StoreId] [int] NOT NULL,
[ShippingMethodId] [int] NOT NULL,
[CreatedOn] [datetime] NULL CONSTRAINT [DF_StoreShippingMethod_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_StoreShippingMethod] on [dbo].[ShippingMethodStore]'
GO
ALTER TABLE [dbo].[ShippingMethodStore] ADD CONSTRAINT [PK_StoreShippingMethod] PRIMARY KEY CLUSTERED  ([StoreId], [ShippingMethodId])

GO


PRINT N'Creating [dbo].[OrderOptionStore]'
GO
CREATE TABLE [dbo].[OrderOptionStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[OrderOptionID] [int] NOT NULL CONSTRAINT [DF_OrderOptionStore_OrderOptionID] DEFAULT ((0)),
[StoreID] [int] NOT NULL CONSTRAINT [DF_OrderOptionStore_StoreID] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_OrderOptionStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_OrderOptionStore] on [dbo].[OrderOptionStore]'
GO
ALTER TABLE [dbo].[OrderOptionStore] ADD CONSTRAINT [PK_OrderOptionStore] PRIMARY KEY CLUSTERED  ([OrderOptionID], [StoreID])

GO


PRINT N'Altering [dbo].[KitItem]'
GO
ALTER TABLE [dbo].[KitItem] ADD
[InventoryQuantityDelta] [int] NOT NULL CONSTRAINT [DF_KitItem_InventoryQuantityDelta] DEFAULT ((0))

GO


PRINT N'Altering [dbo].[KitGroup]'
GO
ALTER TABLE [dbo].[KitGroup] ADD
[Summary] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IsReadOnly] [tinyint] NOT NULL CONSTRAINT [DF_KitItem_IsReadOnly] DEFAULT ((0))

GO


PRINT N'Creating [dbo].[PollStore]'
GO
CREATE TABLE [dbo].[PollStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[PollID] [int] NOT NULL CONSTRAINT [DF_PollStore_PollID] DEFAULT ((0)),
[StoreID] [int] NOT NULL CONSTRAINT [DF_PollStore_StoreID] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_PollStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_PollStore] on [dbo].[PollStore]'
GO
ALTER TABLE [dbo].[PollStore] ADD CONSTRAINT [PK_PollStore] PRIMARY KEY CLUSTERED  ([PollID], [StoreID])

GO


PRINT N'Creating [dbo].[GiftCardStore]'
GO
CREATE TABLE [dbo].[GiftCardStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[GiftCardID] [int] NOT NULL CONSTRAINT [DF_GiftCardStore_GiftCardID] DEFAULT ((0)),
[StoreID] [int] NOT NULL CONSTRAINT [DF_GiftCardStore_StoreID] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_GiftCardStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_GiftCardStore] on [dbo].[GiftCardStore]'
GO
ALTER TABLE [dbo].[GiftCardStore] ADD CONSTRAINT [PK_GiftCardStore] PRIMARY KEY CLUSTERED  ([GiftCardID], [StoreID])

GO


PRINT N'Creating [dbo].[CouponStore]'
GO
CREATE TABLE [dbo].[CouponStore]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[CouponID] [int] NOT NULL CONSTRAINT [DF_CouponStore_CouponID] DEFAULT ((0)),
[StoreID] [int] NOT NULL CONSTRAINT [DF_CouponStore_StoreID] DEFAULT ((1)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_CouponStore_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_CouponStore] on [dbo].[CouponStore]'
GO
ALTER TABLE [dbo].[CouponStore] ADD CONSTRAINT [PK_CouponStore] PRIMARY KEY CLUSTERED  ([CouponID], [StoreID])

GO


PRINT N'Creating [dbo].[NewsletterMailList]'
GO
CREATE TABLE [dbo].[NewsletterMailList]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[GUID] [uniqueidentifier] NULL CONSTRAINT [DF_Newsletter_NewGUID] DEFAULT (newid()),
[EMailAddress] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[FirstName] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LastName] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[SubscriptionConfirmed] [bit] NULL CONSTRAINT [DF_Newsletter_Confirmed] DEFAULT ((1)),
[AddedOn] [datetime] NULL,
[SubscribedOn] [datetime] NULL,
[UnsubscribedOn] [datetime] NULL
)

GO


PRINT N'Creating [dbo].[aspdnsf_GetUpsellProducts]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_GetUpsellProducts]
	@productID			int, 
	@customerlevelID	int,
	@invFilter			int,
	@storeID			int = 1,
	@filterProduct		bit = 0	

      
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @UpsellProducts VARCHAR(8000), 
			@UpsellProductDiscPct MONEY

    SELECT @UpsellProducts = replace(cast(UpsellProducts as varchar(8000)), ' ', ''), @UpsellProductDiscPct = UpsellProductDiscountPercentage from dbo.product with (nolock) where productid = @productid
	
	SELECT 1-(@UpsellProductDiscPct/100) UpsellDiscMultiplier, p.*, pv.VariantID, pv.Price , isnull(pv.SalePrice, 0) SalePrice, isnull(pv.Points, 0) Points, sp.Name SalesPromptName, isnull(ep.price, 0) ExtendedPrice
            from dbo.product p  with (nolock) 
                join dbo.split(@UpsellProducts, ',') up on p.productid = cast(up.items as int)
                left join dbo.SalesPrompt sp  with (nolock) on sp.SalesPromptID = p.SalesPromptID
                join dbo.productvariant pv  with (nolock) on pv.productid = cast(up.items as int) and pv.isdefault = 1 and pv.Published = 1 and pv.Deleted = 0
                left join dbo.ExtendedPrice ep  with (nolock) on ep.VariantID = pv.VariantID and ep.CustomerLevelID = @CustomerLevelID
                join (select p.ProductID
                      from dbo.product p  with (nolock)
                          join dbo.split(@UpsellProducts, ',') rp on p.productid = cast(rp.items as int) 
                          join (select ProductID, sum(Inventory) Inventory from dbo.productvariant with (nolock) group by ProductID) pv on p.ProductID = pv.ProductID
                          left join (select ProductID, sum(quan) inventory from dbo.inventory i1 with (nolock) join dbo.productvariant pv1 with (nolock) on pv1.variantid = i1.variantid join dbo.split(@UpsellProducts, ',') rp1 on pv1.productid = cast(rp1.items as int) group by pv1.productid) i on i.productid = p.productid
                      where case p.TrackInventoryBySizeAndColor when 1 then isnull(i.inventory, 0) else pv.inventory end >= @InvFilter
                      ) tp on p.productid = tp.productid
				inner join (select distinct a.ProductID from Product a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) ps on p.ProductID = ps.ProductID
			where p.Published = 1 and p.Deleted = 0 and p.IsCallToOrder = 0 and p.productid != @productid
END
GO
PRINT N'Creating [dbo].[aspdnsf_insMailingMgrLog]'
GO




CREATE PROCEDURE [dbo].[aspdnsf_insMailingMgrLog]
(
	@SentOn DateTime = null,
	@ToEmail nvarchar(255),
	@FromEmail nvarchar(255),
	@Subject nvarchar(255),
	@Body nvarchar(max)
)
AS
BEGIN
SET NOCOUNT ON

INSERT INTO [MailingMgrLog]	([SentOn],
							[ToEmail],
							[FromEmail],
							[Subject],
							[Body])
			VALUES	(@SentOn,
					@ToEmail,
					@FromEmail,
					@Subject,
					@Body)

END

GO


PRINT N'Creating [dbo].[aspdnsf_getAddressesByCustomer]'
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
	FROM [Address] WITH (NOLOCK)
	WHERE [CustomerID] = @CustomerID
		AND [Deleted] = 0
END

GO


PRINT N'Altering [dbo].[Orders_KitCart]'
GO
ALTER TABLE [dbo].[Orders_KitCart] ADD
[KitGroupIsReadOnly] [tinyint] NOT NULL CONSTRAINT [DF_Orders_KitCart_KitGroupIsReadOnly] DEFAULT ((0)),
[KitItemInventoryQuantityDelta] [int] NOT NULL CONSTRAINT [DF_Orders_KitCart_KitItemInventoryQuantityDelta] DEFAULT ((0))

GO


PRINT N'Creating [dbo].[aspdnsf_delAddressByID]'
GO

CREATE PROCEDURE [dbo].[aspdnsf_delAddressByID]
(
	@AddressID int
)
AS
BEGIN
SET NOCOUNT ON
	DELETE FROM [Address]
	WHERE [AddressID] = @AddressID
END

GO


PRINT N'Creating [dbo].[aspdnsf_updAddress]'
GO

CREATE PROCEDURE [dbo].[aspdnsf_updAddress]
(
	@AddressID int,
	@NickName nvarchar(100) = null,
	@FirstName nvarchar(100) = null,
	@LastName nvarchar(100) = null,
	@Company nvarchar(100) = null,
	@Address1 nvarchar(100) = null,
	@Address2 nvarchar(100) = null,
	@Suite nvarchar(50) = null,
	@City nvarchar(100) = null,
	@State nvarchar(100) = null,
	@Zip nvarchar(100) = null,
	@Country nvarchar(100) = null,
	@ResidenceType int = null,
	@Phone nvarchar(25) = null,
	@Email nvarchar(25) = null
)
AS
BEGIN
SET NOCOUNT ON
UPDATE [Address]
SET
	[NickName]	= COALESCE(@NickName, [NickName]),
	[FirstName]	= COALESCE(@FirstName, [FirstName]),
	[LastName]	= COALESCE(@LastName, [LastName]),
	[Company]	= COALESCE(@Company, [Company]),
	[Address1]	= COALESCE(@Address1, [Address1]),
	[Address2]	= COALESCE(@Address2, [Address2]),
	[Suite]		= COALESCE(@Suite, [Suite]),
	[City]		= COALESCE(@City, [City]),
	[State]		= COALESCE(@State, [State]),
	[Zip]		= COALESCE(@Zip, [Zip]),
	[Country]	= COALESCE(@Country, [Country]),
	[ResidenceType] = COALESCE(@ResidenceType, [ResidenceType]),
	[Phone]		= COALESCE(@Phone, [Phone]),
	[Email]		= COALESCE(@Email, [Email])
WHERE [AddressID] = @AddressID
END

GO


PRINT N'Creating [dbo].[aspdnsf_insAddress]'
GO

CREATE PROCEDURE [dbo].[aspdnsf_insAddress]
(
	@CustomerID int,
	@NickName nvarchar(100)	= null,
	@FirstName nvarchar(100),
	@LastName nvarchar(100),
	@Company nvarchar(100)	= null,
	@Address1 nvarchar(100),
	@Address2 nvarchar(100)	= null,
	@Suite nvarchar(50)		= null,
	@City nvarchar(100),
	@State nvarchar(100),
	@Zip nvarchar(100),
	@Country nvarchar(100),
	@ResidenceType int		= null,
	@Phone nvarchar(25),
	@Email nvarchar(25)
)
AS
BEGIN
	INSERT INTO [Address]	([CustomerID],
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
				[Email])
		VALUES	(@CustomerID,
			@NickName,
			@FirstName,
			@LastName,
			@Company,
			@Address1,
			@Address2,
			@Suite,
			@City,
			@State,
			@Zip,
			@Country,
			@ResidenceType,
			@Phone,
			@Email)
END

GO


PRINT N'Creating [dbo].[aspdnsf_getAffiliateList]'
GO



CREATE PROCEDURE [dbo].[aspdnsf_getAffiliateList]
AS
BEGIN
	SET NOCOUNT ON
	SELECT AffiliateID, Name
	FROM Affiliate WITH (NOLOCK)
	WHERE Deleted = 0 AND Published = 1
END

GO


PRINT N'Creating [dbo].[aspdnsf_SaveKitGroup]'
GO



create procedure [dbo].[aspdnsf_SaveKitGroup] (
	@KitGroupID int,
	@Name nvarchar(400),
	@Description ntext,
	@Summary ntext,
	@ProductID int,
	@DisplayOrder int,
	@KitGroupTypeID int,
	@IsRequired bit,
	@IsReadOnly bit,
	@SavedID int OUTPUT)
 
AS
BEGIN
	IF(EXISTS(SELECT * FROM KitGroup WITH (NOLOCK) WHERE KitGroupID = @KitGroupID))
	BEGIN
		UPDATE KitGroup SET
			[Name] = @Name,
			Description = @Description,
			Summary = @Summary,
			ProductID = @ProductID,
			DisplayOrder = @DisplayOrder,
			KitGroupTypeID = @KitGroupTypeID,
			IsRequired = @IsRequired,
			IsReadOnly = @IsReadOnly
		WHERE KitGroupID = @KitGroupID
		
		SET @SavedID = @KitGroupID
		
	END
	ELSE
	BEGIN
		INSERT INTO KitGroup(
			KitGroupGUID,
			[Name],
			Description,
			Summary,
			ProductID,
			DisplayOrder,
			KitGroupTypeID,
			IsRequired,
			IsReadOnly,
			CreatedOn)
		VALUES (
			newid(),
			@Name,
			@Description,
			@Summary,
			@ProductID,
			@DisplayOrder,
			@KitGroupTypeID,
			@IsRequired,
			@IsReadOnly,
			getdate())

		SET @SavedID = @@Identity
	END
END
GO
PRINT N'Creating [dbo].[aspdnsf_getLocales]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_getLocales]
AS
BEGIN
	SELECT LocaleSettingID, Name
	FROM LocaleSetting WITH (NOLOCK)
END

GO


PRINT N'Creating [dbo].[aspdnsf_SaveKitItem]'
GO


create procedure [dbo].[aspdnsf_SaveKitItem](
	@KitItemID int,
	@KitGroupID int,
	@Name nvarchar(400),
	@Description ntext,	
	@PriceDelta money,
	@WeightDelta money,
	@IsDefault bit,
	@DisplayOrder int,
	@InventoryVariantID int,
	@InventoryQuantityDelta int,
	@InventoryVariantColor nvarchar(100),
	@InventoryVariantSize nvarchar(100),
	@SavedID int OUTPUT
)
 
AS
BEGIN

	IF EXISTS(SELECT * FROM KitItem WITH(NOLOCK) WHERE KitItemId = @KitItemId )
	BEGIN
		UPDATE KitItem SET
			KitGroupID = @KitGroupID,
			[Name] = @Name,
			Description = @Description,
			PriceDelta = @PriceDelta,
			WeightDelta = @WeightDelta,
			IsDefault = @IsDefault,
			DisplayOrder = @DisplayOrder,
			InventoryVariantID = @InventoryVariantID,
			InventoryQuantityDelta = @InventoryQuantityDelta,
			InventoryVariantColor = @InventoryVariantColor,
			InventoryVariantSize = @InventoryVariantSize
		WHERE KitItemID = @KitItemID

		SET @SavedID = @KitItemId		
	END
	ELSE
	BEGIN

		INSERT INTO KitItem( KitItemGUID,
			KitGroupID,
			[Name],
			Description,
			PriceDelta,
			WeightDelta,
			IsDefault,
			DisplayOrder,
			InventoryVariantID,
			InventoryQuantityDelta,
			InventoryVariantColor,
			InventoryVariantSize,
			CreatedOn)
		VALUES ( newid(),
			@KitGroupID,
			@Name,
			@Description,
			@PriceDelta,
			@WeightDelta,
			@IsDefault,
			@DisplayOrder,
			@InventoryVariantID,
			@InventoryQuantityDelta,
			@InventoryVariantColor,
			@InventoryVariantSize,
			getdate() )

		SET @SavedID = @@Identity

	END

END 

GO
PRINT N'Creating [dbo].[aspdnsf_getFailedTransactionCount]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_getFailedTransactionCount]
(
	@CustomerID int
)
AS
BEGIN
SET NOCOUNT ON
	SELECT COUNT(*) AS N
	FROM FailedTransaction WITH (NOLOCK)
	WHERE CustomerID = @CustomerID
END

GO


PRINT N'Creating [dbo].[aspdnsf_delFailedTransactionsByCustomer]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_delFailedTransactionsByCustomer]
(
	@CustomerID int
)
AS
BEGIN
SET NOCOUNT ON
	DELETE FROM FailedTransaction
	WHERE CustomerID = @CustomerID
END

GO


PRINT N'Creating [dbo].[aspdnsf_insRestrictedIP]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_insRestrictedIP]
(
	@IPAddress nvarchar(25)
)
AS
BEGIN
SET NOCOUNT ON
	IF NOT EXISTS(SELECT IPAddress FROM RestrictedIP WITH (NOLOCK) WHERE IPAddress = @IPAddress) BEGIN
		INSERT INTO RestrictedIP (IPAddress) VALUES(@IPAddress)
	END
END

GO


PRINT N'Altering [dbo].[StringResource]'
GO
ALTER TABLE [dbo].[StringResource] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF__StoreID] DEFAULT ((1))

GO


PRINT N'Creating [dbo].[aspdnsf_getStringresource]'
GO


create proc [dbo].[aspdnsf_getStringresource]
    @StringResourceID int = null
  
AS
SET NOCOUNT ON 


SELECT StoreId, StringResourceID, StringResourceGUID, Name, LocaleSetting, ConfigValue, CreatedOn, Modified
FROM dbo.Stringresource with (nolock) 
WHERE StringResourceID = COALESCE(@StringResourceID, StringResourceID)


GO
PRINT N'Creating [dbo].[aspdnsf_getIPIsRestricted]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_getIPIsRestricted]
(
	@IPAddress nvarchar(25)
)
AS
BEGIN
SET NOCOUNT ON
	SELECT COUNT(*) AS N FROM RestrictedIP WITH (NOLOCK)
	WHERE IPAddress = @IPAddress
END

GO


PRINT N'Creating [dbo].[aspdnsf_insStringresource]'
GO


create proc [dbo].[aspdnsf_insStringresource]
    @Name nvarchar(100),
    @LocaleSetting nvarchar(10),
    @ConfigValue nvarchar(2500),
	@StoreId int,
    @StringResourceID int OUTPUT
  
AS
SET NOCOUNT ON 

if exists (select * FROM dbo.Stringresource with (nolock) where StoreId = @StoreId and [Name] = @Name and LocaleSetting = @LocaleSetting) begin
    set @StringResourceID = -1
end
else begin
    insert dbo.Stringresource(StoreId, StringResourceGUID, Name, LocaleSetting, ConfigValue, CreatedOn, Modified)
    values (@StoreId, newid(), @Name, @LocaleSetting, @ConfigValue, getdate(), 0)

    set @StringResourceID = @@identity
end 

 

GO

PRINT N'Creating [dbo].[aspdnsf_delRestrictedIP]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_delRestrictedIP]
(
	@IPAddress nvarchar(25)
)
AS
BEGIN
SET NOCOUNT ON
	DELETE FROM RestrictedIP
	WHERE IPAddress = @IPAddress
END

GO


PRINT N'Creating [dbo].[aspdnsf_updStringresource]'
GO


create proc [dbo].[aspdnsf_updStringresource]
    @StringResourceID int,
	@StoreId int,
    @Name nvarchar(100),
    @LocaleSetting nvarchar(10),
    @ConfigValue nvarchar(2500)
  
AS
SET NOCOUNT ON 


UPDATE dbo.Stringresource
SET 
	StoreId = COALESCE(@StoreId, StoreId),
    Name = COALESCE(@Name, Name),
    LocaleSetting = COALESCE(@LocaleSetting, LocaleSetting),
    ConfigValue = COALESCE(@ConfigValue, ConfigValue),
    Modified = 1
WHERE StringResourceID = @StringResourceID

GO
PRINT N'Creating [dbo].[aspdnsf_getCustomerLevels]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_getCustomerLevels]
AS
BEGIN
SET NOCOUNT ON
	SELECT Name, CustomerLevelID
	FROM CustomerLevel WITH (NOLOCK)
	WHERE Deleted = 0
END

GO


PRINT N'Creating [dbo].[aspdnsf_SearchProductRatings]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_SearchProductRatings]
    @SearchTerm		varchar(1000) = '',
    @FilthyOnly		tinyint = 0,
    @Days			int = 7,
	@StoreID		int = 1,
	@FilterByStore	tinyint = 0

AS
BEGIN
    SET NOCOUNT ON
    
	SELECT
		r.RatingID,
		r.CreatedOn,
		p.Name AS ProductName,
		r.ProductID,
		c.FirstName + ' ' + c.LastName + ' (' + CAST(r.CustomerID AS nvarchar(255)) + ')' AS FullName,
		r.Rating,
		r.Comments,
		r.FoundHelpful,
		r.FoundNotHelpful,
		CAST(r.IsFilthy as BIT) AS IsFilthy
	FROM Rating r WITH (NOLOCK)
	JOIN Product p ON p.ProductID = r.ProductID
	JOIN Customer c ON c.CustomerID = r.CustomerID
	WHERE (@FilterByStore = 0 OR r.StoreID = @StoreID)
		AND (@FilthyOnly = 0 OR r.IsFilthy = 1)
		AND r.CreatedOn > DATEADD(day, (@Days * -1), GETDATE())
		AND ((@SearchTerm = '' OR r.Comments LIKE '%' + @SearchTerm + '%') 
			OR (@SearchTerm = '' OR CAST(r.ProductID as NVARCHAR(15)) LIKE '%' + @SearchTerm + '%'))
END

GO


PRINT N'Creating [dbo].[aspdnsf_UpdateCartItemQuantity]'
GO



CREATE PROCEDURE [dbo].[aspdnsf_UpdateCartItemQuantity]
	@ProductID int,
    @VariantID int,
	@ShoppingCartRecID INT,
	@Quantity INT,
	@NewQuantity INT OUTPUT
  
AS
BEGIN
	DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int

	SELECT @RestrictedQy = RestrictedQuantities 
	FROM dbo.ProductVariant with (nolock) 
	WHERE VariantID = @VariantID

	SELECT @CurrentCartQty = Quantity 
	FROM dbo.shoppingcart with (nolock) 
	WHERE ShoppingCartRecID = @ShoppingCartRecID

	DECLARE @RQty int
	IF isnull(rtrim(@RestrictedQy), '') = ''
		set @RQty = -1
	ELSE
		SELECT @RQty = cast(items as int) FROM dbo.Split(@RestrictedQy, ',') WHERE cast(items as int) <= isnull(@CurrentCartQty, 0) + @Quantity

	UPDATE dbo.ShoppingCart 
    SET Quantity = case @RQty when -1 then Quantity + @Quantity else isnull(@RQty, 0) end
    WHERE ShoppingCartRecID = @ShoppingCartRecID

	SELECT @NewQuantity = Quantity FROM dbo.ShoppingCart with (nolock) WHERE ShoppingCartRecID = @ShoppingCartRecID
END

GO
PRINT N'Creating [dbo].[aspdnsf_updProductRating]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_updProductRating]
	@RatingID	int,
	@Rating		int,
	@Comments	nvarchar(max),
	@IsFilthy	bit
AS
BEGIN
	SET NOCOUNT ON
	UPDATE [dbo].[Rating]
	SET	[Rating]	= @Rating,
		[Comments]	= @Comments,
		[IsFilthy]	= CAST(@IsFilthy AS tinyint)
	WHERE [RatingID] = @RatingID
END

GO


PRINT N'Creating [dbo].[aspdnsf_MoveToShoppingCart]'
GO


create proc [dbo].[aspdnsf_MoveToShoppingCart]
    @ShoppingCartRecId int,
    @CartType int
  
AS
SET NOCOUNT ON 

DECLARE @custid int, @pid int, @vid int, @isapack tinyint, @isakit tinyint, @color nvarchar(200), @size nvarchar(200), @text nvarchar(4000), @qty int

select @custid = s.Customerid, @pid = s.ProductID, @vid = variantid, @color = s.ChosenColor, @size = s.ChosenSize, @qty = s.Quantity, @text = convert(nvarchar(4000), TextOption), @isapack = p.IsAPack, @isakit = p.IsAKit
from dbo.shoppingcart s with (nolock) 
    join dbo.Product p with (nolock) on s.ProductID = p.ProductID 
where s.ShoppingCartRecId = @ShoppingCartRecId and s.CartType = @CartType

if @isapack = 0 and @isakit = 0 begin
    if exists (select * from dbo.shoppingcart with (nolock) where CustomerID=@custid and carttype = 0 and productid = @pid and variantid = @vid and ChosenColor = @color and ChosenSize = @size and convert(nvarchar(4000), TextOption) = @text) begin
        update dbo.shoppingcart set Quantity = Quantity + @qty,CreatedOn=getdate() where CustomerID=@custid and carttype = 0 and productid = @pid and variantid = @vid and ChosenColor = @color and ChosenSize = @size and convert(nvarchar(4000), TextOption) = @text 
        delete dbo.shoppingcart where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
    end
    else begin
        update dbo.ShoppingCart set CartType = 0,CreatedOn=getdate() where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
    end
end
else begin
    update dbo.ShoppingCart set CartType = 0,CreatedOn=getdate() where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
    update dbo.CustomCart set CartType = 0,CreatedOn=getdate() where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
    update dbo.KitCart set CartType = 0,CreatedOn=getdate() where ShoppingCartRecId = @ShoppingCartRecId and CartType = @CartType
end

GO
PRINT N'Creating [dbo].[aspdnsf_delProductRating]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_delProductRating]
	@RatingID	int
AS
BEGIN
	SET NOCOUNT ON
	DELETE FROM [dbo].[Rating] WHERE [RatingID] = @RatingID
END

GO


PRINT N'Creating [dbo].[aspdnsf_UpdateCartKitPrice]'
GO


create proc dbo.aspdnsf_UpdateCartKitPrice
    @ShoppingCartRecId int,
    @CustomerLevelID int = 0
  
AS
BEGIN
    SET NOCOUNT ON
    DECLARE @LevelDiscountsApplyToExtendedPrices tinyint, @LevelDiscountPercent money, @ProductPrice money

    SELECT @LevelDiscountsApplyToExtendedPrices = LevelDiscountsApplyToExtendedPrices, @LevelDiscountPercent = LevelDiscountPercent  
    FROM customerlevel WITH (NOLOCK)
    WHERE CustomerLevelID = @CustomerLevelID

    select @ProductPrice = case 
                            when isnull(@LevelDiscountsApplyToExtendedPrices, 0) = 1 then (100-isnull(@LevelDiscountPercent, 0))*(coalesce(ep.price, nullif(pv.saleprice, 0), pv.price) + kc.pricedelta)/100.0
                            when isnull(@LevelDiscountsApplyToExtendedPrices, 0) = 0 and ep.price is not null then ep.price + kc.pricedelta
                            else (100-isnull(@LevelDiscountPercent, 0))*((coalesce(nullif(pv.saleprice, 0), pv.price) + kc.pricedelta))/100.0
                           end 
    FROM ShoppingCart sc with (nolock) 
        join productvariant pv with (nolock) on sc.variantid = pv.variantid
        left join extendedprice ep with (nolock) on pv.variantid = ep.variantid and ep.CustomerLevelID = @CustomerLevelID
        join (select ShoppingCartRecId, sum(pricedelta) pricedelta
              from kitcart kc with (nolock) 
                join kititem ki with (nolock) on kc.kititemid = ki.kititemid
              where kc.ShoppingCartRecId = @ShoppingCartRecId
              group by ShoppingCartRecId 
             ) kc on sc.ShoppingCartRecId = kc.ShoppingCartRecId
    WHERE sc.ShoppingCartRecId = @ShoppingCartRecId

    UPDATE ShoppingCart
    SET ProductPrice = @ProductPrice
    WHERE ShoppingCartRecID = @ShoppingCartRecId
END
GO
PRINT N'Creating [dbo].[aspdnsf_updGridProductVariant]'
GO



CREATE PROCEDURE [dbo].[aspdnsf_updGridProductVariant]
(
	@variantID int,
	@name nvarchar(255) = NULL,
	@description nvarchar(max),
	@skuSuffix nvarchar(50) = NULL,
	@Price dec = NULL,
	@SalePrice dec = NULL,
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


PRINT N'Creating [dbo].[aspdnsf_SynchronizeCart]'
GO


CREATE PROC [dbo].[aspdnsf_SynchronizeCart](
    @CustomerID int,
    @CartType int
)
  
AS
SET NOCOUNT ON
BEGIN

	UPDATE sc 
	SET sc.ProductSKU = case when i.VendorFullSKU IS NULL OR rtrim(i.VendorFullSKU) = '' then isnull(p.sku, '') + isnull(pv.skusuffix, '') + isnull(sc.ChosenColorSKUModifier, '') + isnull(sc.ChosenSizeSKUModifier, '') else i.VendorFullSKU end
	FROM dbo.ShoppingCart sc WITH (NOLOCK)
		INNER JOIN  dbo.Product p with (NOLOCK)  ON p.ProductID = sc.ProductID AND sc.CustomerID = @CustomerID
		JOIN dbo.ProductVariant pv with (NOLOCK) on sc.VariantID = pv.VariantID 
		LEFT JOIN dbo.Inventory i with (NOLOCK) on pv.VariantID = i.VariantID AND i.size = sc.ChosenSize AND i.color = sc.ChosenColor		
    WHERE sc.CustomerID = @CustomerID and
          sc.CartType  = @CartType
END
GO
PRINT N'Creating [dbo].[GlobalConfig]'
GO
CREATE TABLE [dbo].[GlobalConfig]
(
[GlobalConfigID] [int] NOT NULL IDENTITY(1, 1),
[GlobalConfigGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_GlobalConfig_GlobalConfigGUID] DEFAULT (newid()),
[Name] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Description] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ConfigValue] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ValueType] [varchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[GroupName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EnumValues] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[SuperOnly] [tinyint] NOT NULL CONSTRAINT [DF_GlobalConfig_SuperOnly] DEFAULT ((1)),
[Hidden] [bit] NOT NULL CONSTRAINT [DF_GlobalConfig_Hidden] DEFAULT ((0)),
[CreatedOn] [datetime] NOT NULL CONSTRAINT [DF_GlobalConfig_CreatedOn] DEFAULT (getdate())
)

GO


PRINT N'Creating primary key [PK_GlobalConfig] on [dbo].[GlobalConfig]'
GO
ALTER TABLE [dbo].[GlobalConfig] ADD CONSTRAINT [PK_GlobalConfig] PRIMARY KEY CLUSTERED  ([GlobalConfigID])

GO


PRINT N'Creating [dbo].[aspdnsf_GetEntityMenu]'
GO



CREATE PROCEDURE [dbo].[aspdnsf_GetEntityMenu] 
	@EntityType			int,
		-- 0 = Product
		-- 1 = ProductVariant
		-- 2 = Category
		-- 3 = Section/Department
		-- 4 = Manufacturer
		-- 5 = Distributor
		-- 6 = Genre
		-- 7 = Vector
	@ParentID			int =	0,
	@ParentEntityType	int =	0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- PRODUCTS
	IF @EntityType = 0 BEGIN
		DECLARE @ProductEntity table (ProductID int, EntityID int)
		
		-- Build a temporary table to hold the mappings
		IF @ParentEntityType = 2 BEGIN
			INSERT @ProductEntity
			SELECT ProductID, CategoryID FROM ProductCategory WITH (NOLOCK)
				WHERE CategoryID = @ParentID
		END
		
		IF @ParentEntityType = 3 BEGIN
			INSERT @ProductEntity
				SELECT ProductID, SectionID FROM ProductSection
					WHERE SectionID = @ParentID
				
		END
		
		IF @ParentEntityType = 4 BEGIN
			INSERT @ProductEntity
				SELECT ProductID, ManufacturerID FROM ProductManufacturer
					WHERE ManufacturerID = @ParentID
		END
		
		IF @ParentEntityType = 5 BEGIN
			INSERT @ProductEntity
				SELECT ProductID, DistributorID FROM ProductDistributor
					WHERE DistributorID = @ParentID
		END
		
		IF @ParentEntityType = 6 BEGIN
			INSERT @ProductEntity
				SELECT ProductID, GenreID FROM ProductGenre
					WHERE GenreID = @ParentID
		END
		
		IF @ParentEntityType = 7 BEGIN
			INSERT @ProductEntity
				SELECT ProductID, VectorID FROM ProductVector
					WHERE VectorID = @ParentID
		END
		-- End build temp table
		
		-- Select the product info
		SELECT PE.ProductID as EntityID, P.Name as EntityName, P.Published
			FROM @ProductEntity PE 
			JOIN Product P on PE.ProductID = P.ProductID
			ORDER BY Name, Published
	END
		
	-- PRODUCT VARIANTS
	IF @EntityType = 1 BEGIN
		SELECT Name as EntityName, VariantID as EntityID, Published
			FROM ProductVariant
			WHERE ProductID = @ParentID
			ORDER BY DisplayOrder, Name, Published
	END
	
	-- CATEGORIES
	IF @EntityType = 2 BEGIN
		SELECT Name as EntityName, CategoryID as EntityID, Published
			FROM Category
			WHERE ParentCategoryID = @ParentID
			ORDER BY DisplayOrder, Name, Published
	END
	
	-- DEPARTMENTS
	IF @EntityType = 3 BEGIN
		SELECT Name as EntityName, SectionID as EntityID, Published
			FROM Section
			WHERE ParentSectionID = @ParentID
			ORDER BY DisplayOrder, Name, Published
	END
	
	-- MANUFACTURERS
	IF @EntityType = 4 BEGIN
		SELECT Name as EntityName, ManufacturerID as EntityID, Published
			FROM Manufacturer
			WHERE ParentManufacturerID = @ParentID
			ORDER BY DisplayOrder, Name, Published
	END
	
	-- DISTRIBUTORS
	IF @EntityType = 5 BEGIN
		SELECT Name as EntityName, DistributorID as EntityID, Published
			FROM Distributor
			WHERE ParentDistributorID = @ParentID
			ORDER BY DisplayOrder, Name, Published
	END
	
	-- GENRES
	IF @EntityType = 6 BEGIN
		SELECT Name as EntityName, GenreID as EntityID, Published
			FROM Genre
			WHERE ParentGenreID = @ParentID
			ORDER BY DisplayOrder, Name, Published
	END
	
	-- VECTORS
	IF @EntityType = 7 BEGIN
		SELECT Name as EntityName, VectorID as EntityID, Published
			FROM Vector
			WHERE ParentVectorID = @ParentID
			ORDER BY DisplayOrder, Name, Published
	END

END

GO

PRINT N'Creating [dbo].[aspdnsf_ReloadCart]'
GO


create proc [dbo].[aspdnsf_ReloadCart]
    @CartXML text
  
AS
BEGIN
    SET NOCOUNT ON  

    DECLARE @tmpShoppingCart TABLE ( [CustomerID] [int] NOT NULL ,[ProductSKU] [nvarchar] (100) NULL , [ProductPrice] [money] NULL ,[ProductWeight] [money] NULL , [ProductID] [int] NOT NULL , [VariantID] [int] NOT NULL , [Quantity] [int] NOT NULL , [ChosenColor] [nvarchar] (100)  NULL , [ChosenColorSKUModifier] [nvarchar] (50) NULL , [ChosenSize] [nvarchar] (100) NULL , [ChosenSizeSKUModifier] [nvarchar] (50) NULL , [IsTaxable] [tinyint] NOT NULL , [IsShipSeparately] [tinyint] NOT NULL , [IsDownload] [tinyint] NOT NULL , [DownloadLocation] [ntext] NULL , [CreatedOn] [datetime] NOT NULL , [ProductDimensions] [nvarchar] (100) NULL , [CartType] [int] NOT NULL , [IsSecureAttachment] [tinyint] NOT NULL , [TextOption] [ntext] NULL , [NextRecurringShipDate] [datetime] NULL , [RecurringIndex] [int] NOT NULL , [OriginalRecurringOrderNumber] [int] NULL , [RecurringSubscriptionID] [nvarchar](100) NOT NULL, [BillingAddressID] [int] NULL , [ShippingAddressID] [int] NULL , [ShippingMethodID] [int] NULL , [ShippingMethod] [nvarchar] (400) NULL , [DistributorID] [int] NULL , [SubscriptionInterval] [int] NULL , [SubscriptionIntervalType] [int] NULL, [Notes] [ntext] NULL , [IsUpsell] [tinyint] NOT NULL , [GiftRegistryForCustomerID] [int] NOT NULL , [RecurringInterval] [int] NOT NULL , [RecurringIntervalType] [int] NOT NULL , [ExtensionData] [ntext] NULL, [FreeShipping] [tinyint] NOT NULL, [CustomerEntersPrice] [tinyint] NOT NULL, [IsAKit] [tinyint] NOT NULL, [IsAPack] [tinyint] NOT NULL)
    DECLARE @tmpShoppingCart2 TABLE (oldCartID int not null,  [ShoppingCartRecGUID] [uniqueidentifier] NOT NULL, [CustomerID] [int] NOT NULL , [ProductSKU] [nvarchar] (100) NULL , [ProductPrice] [money] NULL , [ProductWeight] [money] NULL , [ProductID] [int] NOT NULL , [VariantID] [int] NOT NULL , [Quantity] [int] NOT NULL , [ChosenColor] [nvarchar] (100) NULL , [ChosenColorSKUModifier] [nvarchar] (50) NULL , [ChosenSize] [nvarchar] (100) NULL , [ChosenSizeSKUModifier] [nvarchar] (50) NULL , [IsTaxable] [tinyint] NOT NULL , [IsShipSeparately] [tinyint] NOT NULL , [IsDownload] [tinyint] NOT NULL , [DownloadLocation] [ntext] NULL , [CreatedOn] [datetime] NOT NULL , [ProductDimensions] [nvarchar] (100) NULL , [CartType] [int] NOT NULL , [IsSecureAttachment] [tinyint] NOT NULL , [TextOption] [ntext] NULL , [NextRecurringShipDate] [datetime] NULL , [RecurringIndex] [int] NOT NULL , [OriginalRecurringOrderNumber] [int] NULL , [RecurringSubscriptionID] [nvarchar](100) NOT NULL, [BillingAddressID] [int] NULL , [ShippingAddressID] [int] NULL , [ShippingMethodID] [int] NULL , [ShippingMethod] [nvarchar] (400) NULL , [DistributorID] [int] NULL , [SubscriptionInterval] [int] NULL , [SubscriptionIntervalType] [int] NULL , [Notes] [ntext] NULL , [IsUpsell] [tinyint] NOT NULL , [GiftRegistryForCustomerID] [int] NOT NULL , [RecurringInterval] [int] NOT NULL , [RecurringIntervalType] [int] NOT NULL , [ExtensionData] [ntext] NULL, [FreeShipping] [tinyint] NOT NULL, [CustomerEntersPrice] [tinyint] NOT NULL, [IsAKit] [tinyint] NOT NULL, [IsAPack] [tinyint] NOT NULL)
    DECLARE @tmpCart TABLE (cartid int not null, addressid int not null, qty  int not null)     
    DECLARE @tmp1 TABLE ( [CustomerID] [int] NOT NULL , [CartType] [int] NOT NULL , [ProductID] [int] NOT NULL , [VariantID] [int] NOT NULL , [ChosenColor] [nvarchar] (100) NULL , [ChosenSize] [nvarchar] (100) NULL ,[TextOption] [ntext] NULL , [ShippingAddressID] [int] NULL , [Qty] [int] NOT NULL )
    DECLARE @tmp2 TABLE ([cartid] [int] NOT NULL )

    DECLARE @hdoc int, @retcode int
    EXEC @retcode = sp_xml_preparedocument 
                        @hdoc OUTPUT,
                        @CartXML
 
    INSERT @tmpCart (cartid, addressid, qty)
    SELECT cartid, addressid, count(*)
    FROM OPENXML(@hdoc, '/root/row', 0) WITH (cartid int, addressid int)
    GROUP BY cartid, addressid
 
    DECLARE @custid int, @carttype int
 
    SELECT top 1 @custid = CustomerID, @carttype = CartType
    FROM dbo.ShoppingCart s with (nolock) 
            join @tmpCart c on s.ShoppingCartRecID = c.cartid
 

    --creates cart item/shipping address combinations
    INSERT @tmpShoppingCart
    SELECT CustomerID, ProductSKU, ProductPrice, ProductWeight, ProductID,VariantID, c.qty quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize,ChosenSizeSKUModifier, IsTaxable, IsShipSeparately, IsDownload,DownloadLocation, CreatedOn, ProductDimensions, CartType,IsSecureAttachment, TextOption, NextRecurringShipDate, RecurringIndex,OriginalRecurringOrderNumber, RecurringSubscriptionID, BillingAddressID,c.addressid ShippingAddressID, ShippingMethodID, ShippingMethod,DistributorID, SubscriptionInterval, SubscriptionIntervalType, Notes,IsUpsell, GiftRegistryForCustomerID, RecurringInterval,RecurringIntervalType, ExtensionData, s.FreeShipping, s.CustomerEntersPrice, s.IsAKit, s.IsAPack
    FROM dbo.ShoppingCart s 
        join @tmpCart c on s.ShoppingCartRecID = c.cartid
 
 
 
    -- combines like items based on the fields in the group by clause
    INSERT @tmp1
    SELECT customerid, carttype, productid, variantid, chosencolor, chosensize, convert(nvarchar(4000), textoption) textoption, shippingaddressid, sum(quantity)
    FROM @tmpShoppingCart 
    GROUP BY customerid,carttype,productid,variantid,chosencolor,chosensize,convert(nvarchar(4000), textoption),shippingaddressid
 

    -- gets original cartID for restricting new cart items
    INSERT @tmp2
    SELECT min(ShoppingCartRecID) cartid
    FROM dbo.ShoppingCart
    WHERE customerid = @custid and carttype = @carttype
    GROUP BY customerid, carttype, productid, variantid, chosencolor, chosensize, convert(nvarchar(4000), textoption)
 

    -- create new cart records
    INSERT @tmpShoppingCart2 (oldCartID, ShoppingCartRecGUID, CustomerID,ProductSKU, ProductPrice, ProductWeight, ProductID, VariantID, Quantity,ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier,IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, SubscriptionInterval,SubscriptionIntervalType, Notes, IsUpsell, GiftRegistryForCustomerID,RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit, IsAPack)
    SELECT c.cartid, newid(), s.CustomerID, s.ProductSKU, s.ProductPrice,s.ProductWeight, s.ProductID, s.VariantID, a.qty, s.ChosenColor,s.ChosenColorSKUModifier, s.ChosenSize, s.ChosenSizeSKUModifier,s.IsTaxable, s.IsShipSeparately, s.IsDownload, s.DownloadLocation,s.CreatedOn, s.ProductDimensions, s.CartType, s.IsSecureAttachment,s.TextOption, s.NextRecurringShipDate, s.RecurringIndex,s.OriginalRecurringOrderNumber, s.RecurringSubscriptionID,s.BillingAddressID, a.shippingaddressid, s.ShippingMethodID,s.ShippingMethod, s.DistributorID, s.SubscriptionInterval,s.SubscriptionIntervalType, '', s.IsUpsell, s.GiftRegistryForCustomerID,s.RecurringInterval, s.RecurringIntervalType, s.ExtensionData, s.FreeShipping, s.CustomerEntersPrice, s.IsAKit, s.IsAPack
    FROM dbo.ShoppingCart s 
        join @tmp1 a
            on s.customerid = a.customerid and
               s.carttype = a.carttype and 
               s.productid = a.productid and 
               s.variantid = a.variantid and 
               s.chosencolor = a.chosencolor and 
               s.chosensize = a.chosensize and 
               convert(nvarchar(4000), s.textoption)  = convert(nvarchar(4000), a.textoption) 
        join @tmp2 c on s.ShoppingCartRecID = c.cartid
 

    BEGIN TRAN
        INSERT [dbo].ShoppingCart (ShoppingCartRecGUID, CustomerID,ProductSKU, ProductPrice, ProductWeight, ProductID, VariantID, Quantity,ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier,IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, SubscriptionInterval,SubscriptionIntervalType, Notes, IsUpsell, GiftRegistryForCustomerID,RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit, IsAPack)
        SELECT ShoppingCartRecGUID, CustomerID, ProductSKU, ProductPrice,ProductWeight, ProductID, VariantID, Quantity, ChosenColor,ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, IsTaxable,IsShipSeparately, IsDownload, DownloadLocation, CreatedOn,ProductDimensions, CartType, IsSecureAttachment, TextOption,NextRecurringShipDate, RecurringIndex, OriginalRecurringOrderNumber,RecurringSubscriptionID, BillingAddressID, ShippingAddressID,ShippingMethodID, ShippingMethod, DistributorID, SubscriptionInterval,SubscriptionIntervalType, Notes, IsUpsell, GiftRegistryForCustomerID,RecurringInterval, RecurringIntervalType, ExtensionData, FreeShipping, CustomerEntersPrice, IsAKit, IsAPack
        FROM @tmpShoppingCart2
 
        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not add new shopping cart records', 16, 1)
            RETURN -1
        END
 
 
 
        INSERT [dbo].KitCart(CustomerID, ShoppingCartRecID, ProductID,VariantID, KitGroupID, KitGroupTypeID, KitItemID, TextOption, Quantity,CartType, OriginalRecurringOrderNumber, ExtensionData, InventoryVariantID, InventoryVariantColor,InventoryVariantSize, CreatedOn)        
        SELECT k.CustomerID, s.ShoppingCartRecID, k.ProductID, k.VariantID,k.KitGroupID, k.KitGroupTypeID, k.KitItemID, k.TextOption, k.Quantity,k.CartType, k.OriginalRecurringOrderNumber, k.ExtensionData, k.InventoryVariantID, k.InventoryVariantColor,k.InventoryVariantSize, k.CreatedOn
        FROM dbo.KitCart k 
            join @tmpShoppingCart2 c on k.ShoppingCartRecID = c.oldCartID 
            join [dbo].ShoppingCart s with (nolock) on s.ShoppingCartRecGUID = c.ShoppingCartRecGUID
 
        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not add new kit cart records', 16, 1)
            RETURN -2
        END
 

        INSERT [dbo].CustomCart (CustomerID, PackID, ShoppingCartRecID,ProductSKU, ProductWeight, ProductID, VariantID, Quantity, ChosenColor,ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, CartType,OriginalRecurringOrderNumber, ExtensionData,CreatedOn)        
        SELECT k.CustomerID, k.PackID, s.ShoppingCartRecID, k.ProductSKU,k.ProductWeight, k.ProductID, k.VariantID, k.Quantity, k.ChosenColor,k.ChosenColorSKUModifier, k.ChosenSize, k.ChosenSizeSKUModifier, k.CartType,k.OriginalRecurringOrderNumber, k.ExtensionData,k.CreatedOn
        FROM dbo.CustomCart k 
            join @tmpShoppingCart2 c on k.ShoppingCartRecID = c.oldCartID 
            join [dbo].ShoppingCart s with (nolock) on s.ShoppingCartRecGUID = c.ShoppingCartRecGUID
 
        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not add new custom cart records', 16, 1)
            RETURN -3
        END
 

        DELETE dbo.ShoppingCart
        FROM dbo.ShoppingCart s 
            join @tmpCart c on s.ShoppingCartRecID = c.cartid
 
        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not delete old shopping cart records', 16, 1)
            RETURN -4
        END
 
        DELETE dbo.KitCart
        FROM dbo.KitCart s
            join @tmpCart c on s.ShoppingCartRecID = c.cartid
 
        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not delete old kit cart records', 16, 1)
            RETURN -5
        END
 

        DELETE dbo.CustomCart
        FROM dbo.CustomCart s
            join @tmpCart c on s.ShoppingCartRecID = c.cartid
 
        IF @@Error <>0 BEGIN
            ROLLBACK TRAN
            RAISERROR('Could not delete old custom cart records', 16, 1)
            RETURN -6
        END
 
    COMMIT TRAN
  
    exec sp_xml_removedocument @hdoc
END


GO
PRINT N'Creating [dbo].[aspdnsf_MarkOrderAsFraud]'
GO


create proc dbo.aspdnsf_MarkOrderAsFraud
    @ordernum int,
    @fraudstate int
  
AS
SET NOCOUNT ON
BEGIN
    if @fraudstate=1
        update orders set TransactionState='FRAUD', IsNew=0 where OrderNumber=@ordernum
    else
        update orders 
        set TransactionState= case 
                                when AuthorizedOn > isnull(CapturedOn, '1/1/1900')   and AuthorizedOn > isnull(RefundedOn, '1/1/1900')   and AuthorizedOn > isnull(VoidedOn, '1/1/1900')     then 'AUTHORIZED'
                                when CapturedOn   > isnull(AuthorizedOn, '1/1/1900') and CapturedOn   > isnull(RefundedOn, '1/1/1900')   and CapturedOn   > isnull(VoidedOn, '1/1/1900')     then 'CAPTURED'
                                when RefundedOn   > isnull(CapturedOn, '1/1/1900')   and RefundedOn   > isnull(AuthorizedOn, '1/1/1900') and RefundedOn   > isnull(VoidedOn, '1/1/1900')     then 'REFUNDED'
                                when VoidedOn     > isnull(CapturedOn, '1/1/1900')   and VoidedOn     > isnull(RefundedOn, '1/1/1900')   and VoidedOn     > isnull(AuthorizedOn, '1/1/1900') then 'VOIDED'
                              end,
            IsNew=0 
        where OrderNumber=@ordernum
END

GO
PRINT N'Creating [dbo].[aspdnsf_OrderAvgSummary]'
GO


create proc [dbo].[aspdnsf_OrderAvgSummary]
@transactionstate nvarchar(100)
  
AS
BEGIN
SET NOCOUNT ON

SELECT case CountTodayOrders when 0 then 0 else SumTodayOrders/CountTodayOrders end Today,
case CountThisWeekOrders when 0 then 0 else SumThisWeekOrders/CountThisWeekOrders end ThisWeek,
case CountThisMonthOrders when 0 then 0 else SumThisMonthOrders/CountThisMonthOrders end ThisMonth,
case CountThisYearOrders when 0 then 0 else SumThisYearOrders/CountThisYearOrders end ThisYear,
AllTime
FROM
(

select sum(case when datediff(dy, OrderDate, getdate()) = 0 then OrderTotal else 0 end) SumTodayOrders,

sum(case when datediff(dy, OrderDate, getdate()) = 0 then 1 else 0 end) CountTodayOrders,
sum(case when datediff(wk, OrderDate, getdate()) = 0 then OrderTotal else 0 end) SumThisWeekOrders,
sum(case when datediff(wk, OrderDate, getdate()) = 0 then 1 else 0 end) CountThisWeekOrders,
sum(case when datediff(mm, OrderDate, getdate()) = 0 then OrderTotal else 0 end) SumThisMonthOrders,
sum(case when datediff(mm, OrderDate, getdate()) = 0 then 1 else 0 end) CountThisMonthOrders,
sum(case when datediff(yy, OrderDate, getdate()) = 0 then OrderTotal else 0 end) SumThisYearOrders,
sum(case when datediff(yy, OrderDate, getdate()) = 0 then 1 else 0 end) CountThisYearOrders,
avg(OrderTotal) AllTime

from dbo.Orders
where OrderTotal > 0 and TransactionState=@transactionstate

) a
END

GO

PRINT N'Creating [dbo].[aspdnsf_OrderSummary]'
GO


CREATE proc [dbo].[aspdnsf_OrderSummary]
    @transactionstate nvarchar(100)
  
AS
BEGIN
SET NOCOUNT ON

SELECT case CountTodayOrder when 0 then 0 else SumTodayOrders/CountTodayOrder end Today,
        case CountThisWeekOrders when 0 then 0 else SumThisWeekOrders/CountThisWeekOrders end ThisWeek,
        case CountThisMonthOrders when 0 then 0 else SumThisMonthOrders/CountThisMonthOrders end ThisMonth,
        case CountThisYearOrders when 0 then 0 else SumThisWeekOrders/SumThisYearOrders end ThisYear,
        AllTime
FROM
(
    select sum(case when datediff(dy, OrderDate, getdate()) = 0 then OrderTotal else 0 end) SumTodayOrders,
            sum(case when datediff(dy, OrderDate,getdate()) = 0 then 1 else 0 end) CountTodayOrder,
            sum(case when datediff(wk, OrderDate, getdate()) = 0 then OrderTotal else 0 end) SumThisWeekOrders,
            sum(case when datediff(wk, OrderDate,getdate()) = 0 then 1 else 0 end) CountThisWeekOrders,
            sum(case when datediff(mm, OrderDate, getdate()) = 0 then OrderTotal else 0 end) SumThisMonthOrders,
            sum(case when datediff(mm,OrderDate,getdate()) = 0 then 1 else 0 end) CountThisMonthOrders,
            sum(case when datediff(yy, OrderDate, getdate()) = 0 then OrderTotal else 0 end) SumThisYearOrders,
            sum(case when datediff(yy, OrderDate,getdate()) = 0 then 1 else 0 end) CountThisYearOrders,
            avg(OrderTotal) AllTime
    from Orders
    where TransactionState=@transactionstate
) a

END

GO
PRINT N'Creating [dbo].[aspdnsf_ImportOrderShipment_XML]'
GO


create PROC [dbo].[aspdnsf_ImportOrderShipment_XML]  
 @xmlorder ntext,
 @carrierName ntext  
  
AS  
  
BEGIN  
SET NOCOUNT ON  
  
CREATE TABLE #tmp (ReadyToShip bit, OrderNumber int, TrackingNumber nvarchar(100))  
  
DECLARE @hdoc int, @retcode int  
DECLARE @OrderState nvarchar(10)  
  
EXEC @retcode = sp_xml_preparedocument   
  @hdoc OUTPUT,  
  @xmlorder    
  
INSERT INTO #tmp  
SELECT *
FROM OPENXML(@hdoc, '/shipment/ordershipment',2)  
WITH (ReadyToShip bit, OrderNumber int, TrackingNumber nvarchar(100))  
   
  UPDATE dbo.Orders
  SET  
  ShippingTrackingNumber = t.TrackingNumber,  
  ReadyToShip = t.ReadyToShip,
  ShippedOn = getdate(),
  ShippedVia = @carrierName,
  IsNew = 0
  FROM dbo.Orders o  
  JOIN #tmp t  
  ON o.OrderNumber = t.OrderNumber
    
EXEC sp_xml_removedocument @hdoc  
DROP TABLE #tmp  

  
END  


GO
PRINT N'Creating [dbo].[aspdnsf_EditOrder]'
GO


create proc dbo.aspdnsf_EditOrder
    @OrderNumber int
  
AS
SET NOCOUNT ON

DECLARE @custid int, @custlvl int  
  
SELECT @custid = customerid from dbo.orders with (nolock) where OrderNumber = @OrderNumber  
SELECT @custlvl = CustomerLevelID FROM dbo.Customer with (nolock) WHERE customerid = @custid  
  
DELETE dbo.shoppingcart where customerid = @custid and carttype = 0  
DELETE dbo.CustomCart where customerid = @custid and carttype = 0  
DELETE dbo.KitCart where customerid = @custid and carttype = 0  
  
INSERT dbo.ShoppingCart(ShoppingCartRecGUID, CustomerID, ProductSKU, ProductPrice, ProductWeight, ProductID, VariantID, Quantity, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, DistributorID, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, CreatedOn, ProductDimensions, CartType, IsSecureAttachment, TextOption,ShippingAddressID, IsUpsell, CustomerEntersPrice, IsAKit, IsAPack, IsSystem, TaxClassID, TaxRate, RequiresCount)  
SELECT  newid(), os.CustomerID, os.OrderedProductSKU, case when isnull(pv.saleprice, 0) = 0 then  pv.Price else pv.saleprice end, pv.Weight, os.ProductID, os.VariantID, os.Quantity, os.ChosenColor, os.ChosenColorSKUModifier, os.ChosenSize, os.ChosenSizeSKUModifier, os.DistributorID, os.IsTaxable, os.IsShipSeparately, os.IsDownload, os.DownloadLocation, os.FreeShipping, getdate(), pv.Dimensions, 0, os.IsSecureAttachment, os.TextOption,os.ShippingAddressID, 0, os.CustomerEntersPrice, os.IsAKit, os.IsAPack, os.IsSystem, os.TaxClassID, os.TaxRate, ShoppingCartRecID  
FROM dbo.orders_shoppingcart os with (NOLOCK)  
    join dbo.product p with (NOLOCK) on os.productid = p.productid  
    join dbo.productvariant pv with (NOLOCK) on os.variantid = pv.variantid  
WHERE os.OrderNumber = @OrderNumber  
  
INSERT dbo.CustomCart(CartType, ChosenColor, ChosenColorSKUModifier, ChosenSize, ChosenSizeSKUModifier, CreatedOn, CustomerID, ExtensionData, PackID, ProductID, ProductSKU, ProductWeight, Quantity, ShoppingCartRecID, VariantID)  
SELECT 0, cc.ChosenColor, cc.ChosenColorSKUModifier, cc.ChosenSize, cc.ChosenSizeSKUModifier, getdate(), cc.CustomerID, cc.ExtensionData, cc.PackID, cc.ProductID, cc.ProductSKU, cc.ProductWeight, cc.Quantity, s.ShoppingCartRecID, cc.VariantID  
FROM dbo.orders_CustomCart cc with (NOLOCK)  
    join dbo.orders_shoppingcart os with (NOLOCK) on cc.ShoppingCartRecID = os.ShoppingCartRecID  
    join dbo.ShoppingCart s with (NOLOCK) on os.ShoppingCartRecID = s.RequiresCount  
WHERE os.OrderNumber = @OrderNumber  
  
INSERT dbo.KitCart(CartType, CreatedOn, CustomerID, ExtensionData, InventoryVariantColor, InventoryVariantID, InventoryVariantSize, KitGroupID, KitGroupTypeID, KitItemID, ProductID, Quantity, ShoppingCartRecID, TextOption, VariantID)  
SELECT 0, getdate(), kc.CustomerID, kc.ExtensionData, kc.InventoryVariantColor, kc.InventoryVariantID, kc.InventoryVariantSize, kc.KitGroupID, kc.KitGroupTypeID, kc.KitItemID, kc.ProductID, kc.Quantity, s.ShoppingCartRecID, kc.TextOption, kc.VariantID  
FROM dbo.orders_KitCart kc with (NOLOCK)   
    join dbo.orders_shoppingcart os with (NOLOCK) on kc.ShoppingCartRecID = os.ShoppingCartRecID  
    join dbo.ShoppingCart s with (NOLOCK) on os.ShoppingCartRecID = s.RequiresCount  
WHERE os.OrderNumber = @OrderNumber  
  
UPDATE ShoppingCart SET RequiresCount = 0 WHERE customerid = @custid and carttype = 0  

GO
PRINT N'Creating [dbo].[aspdnsf_getOrder]'
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
PRINT N'Creating [dbo].[aspdnsf_updOrders]'
GO


create proc dbo.aspdnsf_updOrders
    @OrderNumber int,
    @ParentOrderNumber int = null,
    @StoreVersion nvarchar(100) = null,
    @QuoteCheckout tinyint = null,
    @IsNew tinyint = null,
    @ShippedOn datetime = null,
    @CustomerID int = null,
    @CustomerGUID uniqueidentifier = null,
    @Referrer ntext = null,
    @SkinID int = null,
    @LastName nvarchar(100) = null,
    @FirstName nvarchar(100) = null,
    @Email nvarchar(100) = null,
    @Notes ntext = null,
    @BillingEqualsShipping tinyint = null,
    @BillingLastName nvarchar(100) = null,
    @BillingFirstName nvarchar(100) = null,
    @BillingCompany nvarchar(100) = null,
    @BillingAddress1 nvarchar(100) = null,
    @BillingAddress2 nvarchar(100) = null,
    @BillingSuite nvarchar(50) = null,
    @BillingCity nvarchar(100) = null,
    @BillingState nvarchar(100) = null,
    @BillingZip nvarchar(10) = null,
    @BillingCountry nvarchar(100) = null,
    @BillingPhone nvarchar(25) = null,
    @ShippingLastName nvarchar(100) = null,
    @ShippingFirstName nvarchar(100) = null,
    @ShippingCompany nvarchar(100) = null,
    @ShippingResidenceType int = null,
    @ShippingAddress1 nvarchar(100) = null,
    @ShippingAddress2 nvarchar(100) = null,
    @ShippingSuite nvarchar(50) = null,
    @ShippingCity nvarchar(100) = null,
    @ShippingState nvarchar(100) = null,
    @ShippingZip nvarchar(10) = null,
    @ShippingCountry nvarchar(100) = null,
    @ShippingMethodID int = null,
    @ShippingMethod ntext = null,
    @ShippingPhone nvarchar(25) = null,
    @ShippingCalculationID int = null,
    @Phone nvarchar(20) = null,
    @RegisterDate datetime = null,
    @AffiliateID int = null,
    @CouponCode nvarchar(50) = null,
    @CouponType int = null,
    @CouponDescription ntext = null,
    @CouponDiscountAmount money = null,
    @CouponDiscountPercent money = null,
    @CouponIncludesFreeShipping tinyint = null,
    @OkToEmail tinyint = null,
    @Deleted tinyint = null,
    @CardType nvarchar(20) = null,
    @CardName nvarchar(100) = null,
    @CardNumber ntext = null,
    @CardExpirationMonth nvarchar(10) = null,
    @CardExpirationYear nvarchar(10) = null,
    @OrderSubtotal money = null,
    @OrderTax money = null,
    @OrderShippingCosts money = null,
    @OrderTotal money = null,
    @PaymentGateway nvarchar(50) = null,
    @AuthorizationCode nvarchar(100) = null,
    @AuthorizationResult ntext = null,
    @AuthorizationPNREF nvarchar(100) = null,
    @TransactionCommand ntext = null,
    @OrderDate datetime = null,
    @LevelID int = null,
    @LevelName nvarchar(100) = null,
    @LevelDiscountPercent money = null,
    @LevelDiscountAmount money = null,
    @LevelHasFreeShipping tinyint = null,
    @LevelAllowsQuantityDiscounts tinyint = null,
    @LevelHasNoTax tinyint = null,
    @LevelAllowsCoupons tinyint = null,
    @LevelDiscountsApplyToExtendedPrices tinyint = null,
    @LastIPAddress varchar(40) = null,
    @PaymentMethod nvarchar(100) = null,
    @OrderNotes ntext = null,
    @PONumber nvarchar(50) = null,
    @DownloadEmailSentOn datetime = null,
    @ReceiptEmailSentOn datetime = null,
    @DistributorEmailSentOn datetime = null,
    @ShippingTrackingNumber nvarchar(100) = null,
    @ShippedVIA nvarchar(100) = null,
    @CustomerServiceNotes ntext = null,
    @RTShipRequest ntext = null,
    @RTShipResponse ntext = null,
    @TransactionState nvarchar(20) = null,
    @AVSResult nvarchar(50) = null,
    @CaptureTXCommand ntext = null,
    @CaptureTXResult ntext = null,
    @VoidTXCommand ntext = null,
    @VoidTXResult ntext = null,
    @RefundTXCommand ntext = null,
    @RefundTXResult ntext = null,
    @CardinalLookupResult ntext = null,
    @CardinalAuthenticateResult ntext = null,
    @CardinalGatewayParms ntext = null,
    @AffiliateCommissionRecorded tinyint = null,
    @OrderOptions ntext = null,
    @OrderWeight money = null,
    @eCheckBankABACode ntext = null,
    @eCheckBankAccountNumber ntext = null,
    @eCheckBankAccountType ntext = null,
    @eCheckBankName ntext = null,
    @eCheckBankAccountName ntext = null,
    @CarrierReportedRate ntext = null,
    @CarrierReportedWeight ntext = null,
    @LocaleSetting nvarchar(10) = null,
    @FinalizationData ntext = null,
    @ExtensionData ntext = null,
    @AlreadyConfirmed tinyint = null,
    @CartType int = null,
    @THUB_POSTED_TO_ACCOUNTING char(1) = null,
    @THUB_POSTED_DATE datetime = null,
    @THUB_ACCOUNTING_REF char(25) = null,
    @Last4 nvarchar(4) = null,
    @ReadyToShip tinyint = null,
    @IsPrinted tinyint = null,
    @AuthorizedOn datetime = null,
    @CapturedOn datetime = null,
    @RefundedOn datetime = null,
    @VoidedOn datetime = null,
    @InventoryWasReduced int = null,
    @MaxMindFraudScore decimal(5, 2) = null,
    @MaxMindDetails ntext = null,
    @CardStartDate nvarchar(20) = null,
    @CardIssueNumber nvarchar(25) = null,
    @TransactionType int = null,
    @Crypt int = null,
    @VATRegistrationID ntext = null,
    @FraudedOn tinyint = null,
    @RefundReason ntext = null
  
AS
SET NOCOUNT ON 


UPDATE dbo.Orders
SET 
    ParentOrderNumber = COALESCE(@ParentOrderNumber, ParentOrderNumber),
    StoreVersion = COALESCE(@StoreVersion, StoreVersion),
    QuoteCheckout = COALESCE(@QuoteCheckout, QuoteCheckout),
    IsNew = COALESCE(@IsNew, IsNew),
    ShippedOn = COALESCE(@ShippedOn, ShippedOn),
    CustomerID = COALESCE(@CustomerID, CustomerID),
    CustomerGUID = COALESCE(@CustomerGUID, CustomerGUID),
    Referrer = COALESCE(@Referrer, Referrer),
    SkinID = COALESCE(@SkinID, SkinID),
    LastName = COALESCE(@LastName, LastName),
    FirstName = COALESCE(@FirstName, FirstName),
    Email = COALESCE(@Email, Email),
    Notes = COALESCE(@Notes, Notes),
    BillingEqualsShipping = COALESCE(@BillingEqualsShipping, BillingEqualsShipping),
    BillingLastName = COALESCE(@BillingLastName, BillingLastName),
    BillingFirstName = COALESCE(@BillingFirstName, BillingFirstName),
    BillingCompany = COALESCE(@BillingCompany, BillingCompany),
    BillingAddress1 = COALESCE(@BillingAddress1, BillingAddress1),
    BillingAddress2 = COALESCE(@BillingAddress2, BillingAddress2),
    BillingSuite = COALESCE(@BillingSuite, BillingSuite),
    BillingCity = COALESCE(@BillingCity, BillingCity),
    BillingState = COALESCE(@BillingState, BillingState),
    BillingZip = COALESCE(@BillingZip, BillingZip),
    BillingCountry = COALESCE(@BillingCountry, BillingCountry),
    BillingPhone = COALESCE(@BillingPhone, BillingPhone),
    ShippingLastName = COALESCE(@ShippingLastName, ShippingLastName),
    ShippingFirstName = COALESCE(@ShippingFirstName, ShippingFirstName),
    ShippingCompany = COALESCE(@ShippingCompany, ShippingCompany),
    ShippingResidenceType = COALESCE(@ShippingResidenceType, ShippingResidenceType),
    ShippingAddress1 = COALESCE(@ShippingAddress1, ShippingAddress1),
    ShippingAddress2 = COALESCE(@ShippingAddress2, ShippingAddress2),
    ShippingSuite = COALESCE(@ShippingSuite, ShippingSuite),
    ShippingCity = COALESCE(@ShippingCity, ShippingCity),
    ShippingState = COALESCE(@ShippingState, ShippingState),
    ShippingZip = COALESCE(@ShippingZip, ShippingZip),
    ShippingCountry = COALESCE(@ShippingCountry, ShippingCountry),
    ShippingMethodID = COALESCE(@ShippingMethodID, ShippingMethodID),
    ShippingMethod = COALESCE(@ShippingMethod, ShippingMethod),
    ShippingPhone = COALESCE(@ShippingPhone, ShippingPhone),
    ShippingCalculationID = COALESCE(@ShippingCalculationID, ShippingCalculationID),
    Phone = COALESCE(@Phone, Phone),
    RegisterDate = COALESCE(@RegisterDate, RegisterDate),
    AffiliateID = COALESCE(@AffiliateID, AffiliateID),
    CouponCode = COALESCE(@CouponCode, CouponCode),
    CouponType = COALESCE(@CouponType, CouponType),
    CouponDescription = COALESCE(@CouponDescription, CouponDescription),
    CouponDiscountAmount = COALESCE(@CouponDiscountAmount, CouponDiscountAmount),
    CouponDiscountPercent = COALESCE(@CouponDiscountPercent, CouponDiscountPercent),
    CouponIncludesFreeShipping = COALESCE(@CouponIncludesFreeShipping, CouponIncludesFreeShipping),
    OkToEmail = COALESCE(@OkToEmail, OkToEmail),
    Deleted = COALESCE(@Deleted, Deleted),
    CardType = COALESCE(@CardType, CardType),
    CardName = COALESCE(@CardName, CardName),
    CardNumber = COALESCE(@CardNumber, CardNumber),
    CardExpirationMonth = COALESCE(@CardExpirationMonth, CardExpirationMonth),
    CardExpirationYear = COALESCE(@CardExpirationYear, CardExpirationYear),
    OrderSubtotal = COALESCE(@OrderSubtotal, OrderSubtotal),
    OrderTax = COALESCE(@OrderTax, OrderTax),
    OrderShippingCosts = COALESCE(@OrderShippingCosts, OrderShippingCosts),
    OrderTotal = COALESCE(@OrderTotal, OrderTotal),
    PaymentGateway = COALESCE(@PaymentGateway, PaymentGateway),
    AuthorizationCode = COALESCE(@AuthorizationCode, AuthorizationCode),
    AuthorizationResult = COALESCE(@AuthorizationResult, AuthorizationResult),
    AuthorizationPNREF = COALESCE(@AuthorizationPNREF, AuthorizationPNREF),
    TransactionCommand = COALESCE(@TransactionCommand, TransactionCommand),
    OrderDate = COALESCE(@OrderDate, OrderDate),
    LevelID = COALESCE(@LevelID, LevelID),
    LevelName = COALESCE(@LevelName, LevelName),
    LevelDiscountPercent = COALESCE(@LevelDiscountPercent, LevelDiscountPercent),
    LevelDiscountAmount = COALESCE(@LevelDiscountAmount, LevelDiscountAmount),
    LevelHasFreeShipping = COALESCE(@LevelHasFreeShipping, LevelHasFreeShipping),
    LevelAllowsQuantityDiscounts = COALESCE(@LevelAllowsQuantityDiscounts, LevelAllowsQuantityDiscounts),
    LevelHasNoTax = COALESCE(@LevelHasNoTax, LevelHasNoTax),
    LevelAllowsCoupons = COALESCE(@LevelAllowsCoupons, LevelAllowsCoupons),
    LevelDiscountsApplyToExtendedPrices = COALESCE(@LevelDiscountsApplyToExtendedPrices, LevelDiscountsApplyToExtendedPrices),
    LastIPAddress = COALESCE(@LastIPAddress, LastIPAddress),
    PaymentMethod = COALESCE(@PaymentMethod, PaymentMethod),
    OrderNotes = COALESCE(@OrderNotes, OrderNotes),
    PONumber = COALESCE(@PONumber, PONumber),
    DownloadEmailSentOn = COALESCE(@DownloadEmailSentOn, DownloadEmailSentOn),
    ReceiptEmailSentOn = COALESCE(@ReceiptEmailSentOn, ReceiptEmailSentOn),
    DistributorEmailSentOn = COALESCE(@DistributorEmailSentOn, DistributorEmailSentOn),
    ShippingTrackingNumber = COALESCE(@ShippingTrackingNumber, ShippingTrackingNumber),
    ShippedVIA = COALESCE(@ShippedVIA, ShippedVIA),
    CustomerServiceNotes = COALESCE(@CustomerServiceNotes, CustomerServiceNotes),
    RTShipRequest = COALESCE(@RTShipRequest, RTShipRequest),
    RTShipResponse = COALESCE(@RTShipResponse, RTShipResponse),
    TransactionState = COALESCE(@TransactionState, TransactionState),
    AVSResult = COALESCE(@AVSResult, AVSResult),
    CaptureTXCommand = COALESCE(@CaptureTXCommand, CaptureTXCommand),
    CaptureTXResult = COALESCE(@CaptureTXResult, CaptureTXResult),
    VoidTXCommand = COALESCE(@VoidTXCommand, VoidTXCommand),
    VoidTXResult = COALESCE(@VoidTXResult, VoidTXResult),
    RefundTXCommand = COALESCE(@RefundTXCommand, RefundTXCommand),
    RefundTXResult = COALESCE(@RefundTXResult, RefundTXResult),
    CardinalLookupResult = COALESCE(@CardinalLookupResult, CardinalLookupResult),
    CardinalAuthenticateResult = COALESCE(@CardinalAuthenticateResult, CardinalAuthenticateResult),
    CardinalGatewayParms = COALESCE(@CardinalGatewayParms, CardinalGatewayParms),
    AffiliateCommissionRecorded = COALESCE(@AffiliateCommissionRecorded, AffiliateCommissionRecorded),
    OrderOptions = COALESCE(@OrderOptions, OrderOptions),
    OrderWeight = COALESCE(@OrderWeight, OrderWeight),
    eCheckBankABACode = COALESCE(@eCheckBankABACode, eCheckBankABACode),
    eCheckBankAccountNumber = COALESCE(@eCheckBankAccountNumber, eCheckBankAccountNumber),
    eCheckBankAccountType = COALESCE(@eCheckBankAccountType, eCheckBankAccountType),
    eCheckBankName = COALESCE(@eCheckBankName, eCheckBankName),
    eCheckBankAccountName = COALESCE(@eCheckBankAccountName, eCheckBankAccountName),
    CarrierReportedRate = COALESCE(@CarrierReportedRate, CarrierReportedRate),
    CarrierReportedWeight = COALESCE(@CarrierReportedWeight, CarrierReportedWeight),
    LocaleSetting = COALESCE(@LocaleSetting, LocaleSetting),
    FinalizationData = COALESCE(@FinalizationData, FinalizationData),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData),
    AlreadyConfirmed = COALESCE(@AlreadyConfirmed, AlreadyConfirmed),
    CartType = COALESCE(@CartType, CartType),
    THUB_POSTED_TO_ACCOUNTING = COALESCE(@THUB_POSTED_TO_ACCOUNTING, THUB_POSTED_TO_ACCOUNTING),
    THUB_POSTED_DATE = COALESCE(@THUB_POSTED_DATE, THUB_POSTED_DATE),
    THUB_ACCOUNTING_REF = COALESCE(@THUB_ACCOUNTING_REF, THUB_ACCOUNTING_REF),
    Last4 = COALESCE(@Last4, Last4),
    ReadyToShip = COALESCE(@ReadyToShip, ReadyToShip),
    IsPrinted = COALESCE(@IsPrinted, IsPrinted),
    AuthorizedOn = COALESCE(@AuthorizedOn, AuthorizedOn),
    CapturedOn = COALESCE(@CapturedOn, CapturedOn),
    RefundedOn = COALESCE(@RefundedOn, RefundedOn),
    VoidedOn = COALESCE(@VoidedOn, VoidedOn),
    InventoryWasReduced = COALESCE(@InventoryWasReduced, InventoryWasReduced),
    MaxMindFraudScore = COALESCE(@MaxMindFraudScore, MaxMindFraudScore),
    MaxMindDetails = COALESCE(@MaxMindDetails, MaxMindDetails),
    CardStartDate = COALESCE(@CardStartDate, CardStartDate),
    CardIssueNumber = COALESCE(@CardIssueNumber, CardIssueNumber),
    TransactionType = COALESCE(@TransactionType, TransactionType),
    Crypt = COALESCE(@Crypt, Crypt),
    VATRegistrationID = COALESCE(@VATRegistrationID, VATRegistrationID),
    FraudedOn = COALESCE(@FraudedOn, FraudedOn),
    RefundReason = COALESCE(@RefundReason, RefundReason)
WHERE OrderNumber = @OrderNumber

GO
PRINT N'Creating [dbo].[aspdnsf_GetMobileEntities]'
GO

/****** Object:  StoredProcedure [dbo].[aspdnsf_GetMobileEntities]    ******/
create proc [dbo].[aspdnsf_GetMobileEntities]
	@entitytype		int, -- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector  
	@entityid		int,
	@pagesize		int,
	@pagenum		int,
	@numpages		int output
 
 
AS  
BEGIN  
  
 SET NOCOUNT ON   
  
	IF @entityid is null
		set @entityid = 0
		
	IF @pagenum is null or @pagenum = 0
		set @pagenum = 1
	
    -- get page size  
    IF (@pagesize is null or @pagesize = 0) and @entityid > 0 BEGIN  
        IF @entitytype = 1  
            SELECT @pagesize = PageSize FROM dbo.Category with (nolock) WHERE categoryID = @entityid  
        ELSE IF @entitytype = 2  
            SELECT @pagesize = PageSize FROM dbo.Section with (nolock) WHERE sectionID = @entityid  
        ELSE IF @entitytype = 3  
            SELECT @pagesize = PageSize FROM dbo.Manufacturer with (nolock) WHERE manufacturerID = @entityid  
        ELSE IF @entitytype = 4 
            SELECT @pagesize = PageSize FROM dbo.Distributor with (nolock) WHERE distributorID = @entityid  
        ELSE IF @entitytype = 5   
            SELECT @pagesize = PageSize FROM dbo.Genre with (nolock) WHERE genreID = @entityid  
        ELSE IF @entitytype = 6  
            SELECT @pagesize = PageSize FROM dbo.Vector with (nolock) WHERE vectorID = @entityid  
        ELSE   
            SELECT @pagesize = convert(int, ConfigValue) FROM dbo.AppConfig with (nolock) WHERE [Name] = 'Default_CategoryPageSize'  
    END  
  
    IF @pagesize is null or @pagesize = 0  
        SET @pagesize = 20  

	declare @lowbounds	int
	set @lowbounds = @pagesize * (@pagenum - 1)
	
	declare @highbounds	int 
	set @highbounds = @pagesize * @pagenum
	
	DECLARE @entitycount int
	
	-- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector
	IF @entitytype = 1
		BEGIN
		SELECT @entitycount = COUNT(CategoryID) from dbo.Category with(NOLOCK) where ParentCategoryID = @entityid and Published=1 and Deleted=0
		SELECT CategoryID, Name, SEName FROM
			( select CategoryID,
				Name, 
				SEName, 
				ROW_NUMBER() OVER(ORDER BY DisplayOrder, Name, CategoryID) as RowNum
				from dbo.Category with(NOLOCK)
				where ParentCategoryID = @entityid and Published=1 and Deleted=0
			) ent
			WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
    ELSE IF @entitytype = 2  
		BEGIN
		SELECT @entitycount = COUNT(SectionID) from dbo.Section with(NOLOCK) where ParentSectionID = @entityid and Published=1 and Deleted=0
        SELECT SectionID, Name, SEName FROM
		( select SectionID,
			Name, 
			SEName,
			ROW_NUMBER() OVER(ORDER BY DisplayOrder, Name, SectionID) as RowNum
			from dbo.Section with(NOLOCK)
			where ParentSectionID = @entityid and Published=1 and Deleted=0
		) ent
		WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
    ELSE IF @entitytype = 3  
        BEGIN
		SELECT @entitycount = COUNT(ManufacturerID) from dbo.Manufacturer with(NOLOCK) where ParentManufacturerID = @entityid and Published=1 and Deleted=0
        SELECT ManufacturerID, Name, SEName FROM
		( select ManufacturerID,
			Name, 
			SEName,
			ROW_NUMBER() OVER(ORDER BY DisplayOrder, Name, ManufacturerID) as RowNum
			from dbo.Manufacturer with(NOLOCK)
			where ParentManufacturerID = @entityid and Published=1 and Deleted=0
		) ent
		WHERE RowNum > @lowbounds and RowNum <= @highbounds 
		END
    ELSE IF @entitytype = 4 
        BEGIN
		SELECT @entitycount = COUNT(DistributorID) from dbo.Distributor with(NOLOCK) where ParentDistributorID = @entityid and Published=1 and Deleted=0
        SELECT DistributorID, Name, SEName FROM
		( select DistributorID,
			Name, 
			SEName,
			ROW_NUMBER() OVER(ORDER BY DisplayOrder, Name, DistributorID) as RowNum
			from dbo.Distributor with(NOLOCK)
			where ParentDistributorID = @entityid and Published=1 and Deleted=0
		) ent
		WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
    ELSE IF @entitytype = 5   
        BEGIN
		SELECT @entitycount = COUNT(GenreID) from dbo.Genre with(NOLOCK) where ParentGenreID = @entityid and Published=1 and Deleted=0
        SELECT GenreID, Name, SEName FROM
		( select GenreID,
			Name, 
			SEName,
			ROW_NUMBER() OVER(ORDER BY DisplayOrder, Name, GenreID) as RowNum
			from dbo.Genre with(NOLOCK)
			where ParentGenreID = @entityid and Published=1 and Deleted=0
		) ent
		WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END 
    ELSE IF @entitytype = 6  
        BEGIN
		SELECT @entitycount = COUNT(VectorID) from dbo.Vector with(NOLOCK) where ParentVectorID = @entityid and Published=1 and Deleted=0
        SELECT VectorID, Name, SEName FROM
		( select VectorID,
			Name, 
			SEName,
			ROW_NUMBER() OVER(ORDER BY DisplayOrder, Name, VectorID) as RowNum
			from dbo.Vector with(NOLOCK)
			where ParentVectorID = @entityid and Published=1 and Deleted=0
		) ent
		WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
	END  
	
set @numpages = case when (@entitycount % @pagesize = 0) then @entitycount / @pagesize else (@entitycount/@pagesize) + 1 end 
	
return @entitycount

GO

PRINT N'Creating [dbo].[aspdnsf_GetCartCategoryCountsByProduct]'
GO


create proc dbo.aspdnsf_GetCartCategoryCountsByProduct
    @CustomerID int
  
AS
BEGIN
    SELECT pc.productid, pc.categoryid, sum(a.Quantity) CategoryQty
    FROM dbo.productcategory pc with (nolock) 
        join
        (select pc1.categoryid, sum(Quantity) Quantity
         from dbo.shoppingcart sc with (nolock) 
         join [dbo].productcategory pc1 with (nolock) on sc.productid = pc1.productid
         where sc.customerid = @CustomerID
         group by pc1.categoryid
        ) a on pc.categoryid = a.categoryid
        join (select distinct productid from dbo.shoppingcart with (nolock) where customerid = @CustomerID) b on pc.productid = b.productid 
    GROUP BY pc.productid, pc.categoryid
    ORDER BY pc.productid, pc.categoryid
END

GO

PRINT N'Creating [dbo].[aspdnsf_RecurseMobileEntities]'
GO

/****** Object:  StoredProcedure [dbo].[aspdnsf_RecurseMobileEntities]    ******/
create proc [dbo].[aspdnsf_RecurseMobileEntities]
	@entitytype		int, -- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector  
	@entityid		int

AS  
BEGIN  
  
 SET NOCOUNT ON   
  
	declare @parentid int
	select @parentid = ParentCategoryID from dbo.Category where CategoryID=@entityid
	
	CREATE TABLE #tmp
	(
		IDEN int identity,
		ID int,
		Name nvarchar(400),
		SEName nvarchar(100)
	)
	
		IF @entitytype = 1
				BEGIN
					insert #tmp (ID, Name, SEName)
						select CategoryID, 
							Name, 
							SEName 
						from dbo.Category with(NOLOCK) 
						where CategoryID=@entityid
						
					select @parentid = ParentCategoryID from dbo.Category with(NOLOCK) where CategoryID=@entityid

				END
			ELSE IF @entitytype = 2  
				BEGIN
					insert #tmp (ID, Name, SEName)
						select SectionID, 
							Name, 
							SEName 
						from dbo.Section with(NOLOCK) 
						where SectionID=@entityid
					
					select @parentid = ParentSectionID from dbo.Section with(NOLOCK) where SectionID=@entityid
				END
			ELSE IF @entitytype = 3  
				BEGIN
					insert #tmp (ID, Name, SEName)
						select ManufacturerID, 
							Name, 
							SEName 
						from dbo.Manufacturer with(NOLOCK) 
						where ManufacturerID=@entityid
						
						select @parentid = ParentManufacturerID from dbo.Manufacturer with(NOLOCK) where ManufacturerID=@entityid
				
				END
			ELSE IF @entitytype = 4 
				BEGIN
					insert #tmp (ID, Name, SEName)
						select DistributorID, 
							Name, 
							SEName 
						from dbo.Distributor with(NOLOCK) 
						where DistributorID=@entityid
						
						select @parentid = ParentDistributorID from dbo.Distributor with(NOLOCK) where DistributorID=@entityid
				
				END
			ELSE IF @entitytype = 5   
				BEGIN
					insert #tmp (ID, Name, SEName)
						select GenreID, 
							Name, 
							SEName 
						from dbo.Genre with(NOLOCK) 
						where GenreID=@entityid
						
						select @parentid = ParentGenreID from dbo.Genre with(NOLOCK) where GenreID=@entityid

				END 
			ELSE IF @entitytype = 6  
				BEGIN
					insert #tmp (ID, Name, SEName)
						select VectorID, 
							Name, 
							SEName 
						from dbo.Vector with(NOLOCK) 
						where VectorID=@entityid
						
						select @parentid = ParentVectorID from dbo.Vector with(NOLOCK) where VectorID=@entityid
				END
	
	
	while @parentid > 0
		begin
			-- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector
			IF @entitytype = 1
				BEGIN
					insert #tmp (ID, Name, SEName)
						select CategoryID, 
							Name, 
							SEName 
						from dbo.Category with(NOLOCK) 
						where CategoryID=@parentid
						
					select @entityid = CategoryID, 
						@parentid = ParentCategoryID
					from dbo.Category with(NOLOCK) where CategoryID=@parentid
				END
			ELSE IF @entitytype = 2  
				BEGIN
					insert #tmp (ID, Name, SEName)
						select SectionID, 
							Name, 
							SEName 
						from dbo.Section with(NOLOCK) 
						where SectionID=@parentid
						
					select @entityid = SectionID, 
						@parentid = ParentSectionID
					from dbo.Section with(NOLOCK) where SectionID=@parentid	
				END
			ELSE IF @entitytype = 3  
				BEGIN
					insert #tmp (ID, Name, SEName)
						select ManufacturerID, 
							Name, 
							SEName 
						from dbo.Manufacturer with(NOLOCK) 
						where ManufacturerID=@parentid
						
					select @entityid = ManufacturerID, 
						@parentid = ParentManufacturerID
					from dbo.Manufacturer with(NOLOCK) where ManufacturerID=@parentid
				END
			ELSE IF @entitytype = 4 
				BEGIN
					insert #tmp (ID, Name, SEName)
						select DistributorID, 
							Name, 
							SEName 
						from dbo.Distributor with(NOLOCK) 
						where DistributorID=@parentid
						
					select @entityid = DistributorID, 
						@parentid = ParentDistributorID
					from dbo.Distributor with(NOLOCK) where DistributorID=@parentid
				END
			ELSE IF @entitytype = 5   
				BEGIN
					insert #tmp (ID, Name, SEName)
						select GenreID, 
							Name, 
							SEName 
						from dbo.Genre with(NOLOCK) 
						where GenreID=@parentid
						
					select @entityid = GenreID, 
						@parentid = ParentGenreID
					from dbo.Genre with(NOLOCK) where GenreID=@parentid
				END 
			ELSE IF @entitytype = 6  
				BEGIN
					insert #tmp (ID, Name, SEName)
						select VectorID, 
							Name, 
							SEName 
						from dbo.Vector with(NOLOCK) 
						where VectorID=@parentid
						
					select @entityid = VectorID, 
						@parentid = ParentVectorID
					from dbo.Vector with(NOLOCK) where VectorID=@parentid
				END
			END
		END 

	select * from #tmp order by IDEN DESC

	drop table #tmp
	

GO


PRINT N'Creating [dbo].[aspdnsf_CheckFilthy]'
GO


CREATE PROC dbo.aspdnsf_CheckFilthy
    @COMMENTTEXT ntext,
    @locale char(5)  
  
AS
SET NOCOUNT ON

    IF EXISTS (select * from Badword WHERE (@COMMENTTEXT LIKE '%' + WORD + ' %' OR @COMMENTTEXT LIKE '%' + WORD + ',%' OR @COMMENTTEXT LIKE '%' + WORD + '.%' OR @COMMENTTEXT LIKE '%' + WORD + '!%' OR @COMMENTTEXT LIKE '%' + WORD) AND LocaleSetting = @locale )
        SELECT 1 IsFilthy
    ELSE
        SELECT 0 IsFilthy
GO
PRINT N'Creating [dbo].[aspdnsf_SearchProductComments]'
GO


CREATE PROC dbo.aspdnsf_SearchProductComments
    @Search			varchar(1000) = '',
    @CustomerID		int = null,
    @FilthyOnly		tinyint = 0,
    @Sort			tinyint = 1,
    @days			int = 7,
	@storeID		int = 1,
	@FilterByStore	tinyint = 0
  
AS
BEGIN
    SET NOCOUNT ON 

    CREATE TABLE #tmpProductComments(
        [RatingID] [int] NOT NULL,
        [ProductID] [int] NOT NULL,
        [CustomerID] [int] NOT NULL,
        [Rating] [int] NOT NULL,
        [Comments] [ntext] NULL,
        [FoundHelpful] [int] NOT NULL,
        [FoundNotHelpful] [int] NOT NULL,
        [CreatedOn] [datetime] NOT NULL,
        [ProductName] [nvarchar](400) NULL,
        [FirstName] [nvarchar](100) NULL,
        [LastName] [nvarchar](100) NULL,
        [IsFilthy] [tinyint] NOT NULL
    ) 

    IF RTRIM(@Search) = '' and @CustomerID = null and @FilthyOnly = 0 BEGIN
        SELECT @days = convert(int, ConfigValue) FROM dbo.AppConfig with (nolock) WHERE Name = 'RecentCommentHistoryDays'
        
        IF @days = 0 
            SET @days = 7

        INSERT #tmpProductComments
        SELECT     
            r.RatingID, 
            r.ProductID, 
            r.CustomerID, 
            r.Rating, 
            r.Comments, 
            r.FoundHelpful, 
            r.FoundNotHelpful, 
            r.CreatedOn, 
            p.Name ProductName, 
            c.FirstName, 
            c.LastName, 
            r.IsFilthy
        FROM dbo.Rating r with (nolock) 
            left join dbo.Customer c with (nolock) ON r.CustomerID = c.CustomerID 
            left join dbo.Product p with (nolock) ON r.ProductID = p.ProductID 
        WHERE r.HasComment = 1
             AND p.Deleted = 0 
             AND p.Published = 1
             AND r.IsFilthy >= @FilthyOnly
             AND r.CustomerID = COALESCE(@CustomerID, r.CustomerID)
             AND r.CreatedOn >= dateadd(dy, -@days, getdate())
			 AND (0 = @FilterByStore OR r.StoreID = @storeID)
    END
    ELSE BEGIN
        SET @Search = '%' + @Search + '%'
        INSERT #tmpProductComments
        SELECT     
            r.RatingID, 
            r.ProductID, 
            r.CustomerID, 
            r.Rating, 
            r.Comments, 
            r.FoundHelpful, 
            r.FoundNotHelpful, 
            r.CreatedOn, 
            p.Name ProductName, 
            c.FirstName, 
            c.LastName, 
            r.IsFilthy
        FROM dbo.Rating r with (nolock) 
            left join dbo.Customer c with (nolock) ON r.CustomerID = c.CustomerID 
            left join dbo.Product p with (nolock) ON r.ProductID = p.ProductID 
        WHERE r.HasComment = 1
             AND p.Deleted = 0 
             AND p.Published = 1
             AND r.IsFilthy >= @FilthyOnly
             AND r.CustomerID = COALESCE(@CustomerID, r.CustomerID)
             AND isnull(r.Comments, '') like @Search
			 AND (0 = @FilterByStore OR r.StoreID = @storeID)
    END

    IF @Sort = 1 
        SELECT * FROM #tmpProductComments ORDER BY FoundHelpful desc, CreatedOn desc
    ELSE IF @Sort = 2
        SELECT * FROM #tmpProductComments ORDER BY FoundHelpful asc, CreatedOn desc 
    ELSE IF @Sort = 3
        SELECT * FROM #tmpProductComments ORDER BY CreatedOn desc
    ELSE IF @Sort = 4
        SELECT * FROM #tmpProductComments ORDER BY CreatedOn
    ELSE IF @Sort = 5
        SELECT * FROM #tmpProductComments ORDER BY Rating desc, CreatedOn desc
    ELSE IF @Sort = 6
        SELECT * FROM #tmpProductComments ORDER BY Rating asc, CreatedOn desc
END
GO
PRINT N'Creating [dbo].[aspdnsf_GetProductsForMobileEntity]'
GO

/****** Object:  StoredProcedure [dbo].[aspdnsf_GetProductsForMobileEntity]    ******/
CREATE proc [dbo].[aspdnsf_GetProductsForMobileEntity]
	@entitytype		int, -- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector  
	@entityid		int,
	@pagesize		int,
	@pagenum		int,
	@numpages		int output
 
 
AS  
BEGIN  
  
 SET NOCOUNT ON   
  
	IF @entityid is null
		set @entityid = 0
		
	IF @pagenum is null or @pagenum = 0
		set @pagenum = 1
	
    -- get page size  
    IF (@pagesize is null or @pagesize = 0) and @entityid > 0 BEGIN  
        IF @entitytype = 1  
            SELECT @pagesize = PageSize FROM dbo.Category with (nolock) WHERE categoryID = @entityid  
        ELSE IF @entitytype = 2  
            SELECT @pagesize = PageSize FROM dbo.Section with (nolock) WHERE sectionID = @entityid  
        ELSE IF @entitytype = 3  
            SELECT @pagesize = PageSize FROM dbo.Manufacturer with (nolock) WHERE manufacturerID = @entityid  
        ELSE IF @entitytype = 4 
            SELECT @pagesize = PageSize FROM dbo.Distributor with (nolock) WHERE distributorID = @entityid  
        ELSE IF @entitytype = 5   
            SELECT @pagesize = PageSize FROM dbo.Genre with (nolock) WHERE genreID = @entityid  
        ELSE IF @entitytype = 6  
            SELECT @pagesize = PageSize FROM dbo.Vector with (nolock) WHERE vectorID = @entityid  
        ELSE   
            SELECT @pagesize = convert(int, ConfigValue) FROM dbo.AppConfig with (nolock) WHERE [Name] = 'Default_CategoryPageSize'  
    END  
  
    IF @pagesize is null or @pagesize = 0  
        SET @pagesize = 5  

	declare @lowbounds	int
	set @lowbounds = @pagesize * (@pagenum - 1)
	
	declare @highbounds	int 
	set @highbounds = @pagesize * @pagenum
	
	DECLARE @productcount int
	
	-- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector
	IF @entitytype = 1
		BEGIN
		SELECT @productcount = COUNT(pc.ProductID) from dbo.ProductCategory pc with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pc.ProductID where p.Published=1 and p.Deleted=0 and pc.CategoryID = @entityid
	
		SELECT ProductID, Name, SEName, Description, Price, SalePrice, SalesPrompt, VariantID, TaxClassID FROM
			( select p.ProductID,
				p.Name, 
				p.SEName,
				p.Description,
				pv.Price,
				pv.SalePrice,
				pv.VariantID,
				p.TaxClassID, 
				sp.Name as SalesPrompt,
				ROW_NUMBER() OVER(ORDER BY pc.DisplayOrder, p.Name, p.ProductID) as RowNum
				from dbo.Product p with(NOLOCK)
				left join dbo.ProductCategory pc with(NOLOCK) on pc.ProductID = p.ProductID
				left join dbo.ProductVariant pv with(NOLOCK) on pv.ProductID = p.ProductID
				left join dbo.SalesPrompt sp with(NOLOCK) on sp.SalesPromptID = p.SalesPromptID
				where p.Published=1 and p.Deleted=0 and pc.CategoryID = @entityid and pv.IsDefault=1
			) ent
			WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
    ELSE IF @entitytype = 2  
		BEGIN
		SELECT @productcount = COUNT(ps.ProductID) from dbo.ProductSection ps with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = ps.ProductID where p.Published=1 and p.Deleted=0 and ps.SectionID = @entityid
	
		SELECT ProductID, Name, SEName, Description, Price, SalePrice, SalesPrompt, VariantID, TaxClassID FROM
			( select p.ProductID,
				p.Name, 
				p.SEName,
				p.Description, 
				pv.Price,
				pv.SalePrice, 
				pv.VariantID,
				p.TaxClassID, 
				sp.Name as SalesPrompt,
				ROW_NUMBER() OVER(ORDER BY ps.DisplayOrder, p.Name, p.ProductID) as RowNum
				from dbo.Product p with(NOLOCK)
				left join dbo.ProductSection ps with(NOLOCK) on ps.ProductID = p.ProductID
				left join dbo.ProductVariant pv with(NOLOCK) on pv.ProductID = p.ProductID
				left join dbo.SalesPrompt sp with(NOLOCK) on sp.SalesPromptID = p.SalesPromptID
				where p.Published=1 and p.Deleted=0 and ps.SectionID = @entityid and pv.IsDefault=1
			) ent
			WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
    ELSE IF @entitytype = 3  
        BEGIN
		SELECT @productcount = COUNT(pm.ProductID) from dbo.ProductManufacturer pm with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pm.ProductID where p.Published=1 and p.Deleted=0 and pm.ManufacturerID = @entityid
	
		SELECT ProductID, Name, SEName, Description, Price, SalePrice, SalesPrompt, VariantID, TaxClassID FROM
			( select p.ProductID,
				p.Name, 
				p.SEName,
				p.Description, 
				pv.Price,
				pv.SalePrice, 
				pv.VariantID,
				p.TaxClassID, 
				sp.Name as SalesPrompt,
				ROW_NUMBER() OVER(ORDER BY pm.DisplayOrder, p.Name, p.ProductID) as RowNum
				from dbo.Product p with(NOLOCK)
				left join dbo.ProductManufacturer pm with(NOLOCK) on pm.ProductID = p.ProductID
				left join dbo.ProductVariant pv with(NOLOCK) on pv.ProductID = p.ProductID
				left join dbo.SalesPrompt sp with(NOLOCK) on sp.SalesPromptID = p.SalesPromptID
				where p.Published=1 and p.Deleted=0 and pm.ManufacturerID = @entityid and pv.IsDefault=1
			) ent
			WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
    ELSE IF @entitytype = 4 
        BEGIN
		SELECT @productcount = COUNT(pd.ProductID) from dbo.ProductDistributor pd with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pd.ProductID where p.Published=1 and p.Deleted=0 and pd.DistributorID = @entityid
	
		SELECT ProductID, Name, SEName, Description, Price, SalePrice, SalesPrompt, VariantID, TaxClassID FROM
			( select p.ProductID,
				p.Name, 
				p.SEName, 
				p.Description,
				pv.Price,
				pv.SalePrice, 
				pv.VariantID,
				p.TaxClassID, 
				sp.Name as SalesPrompt,
				ROW_NUMBER() OVER(ORDER BY pd.DisplayOrder, p.Name, p.ProductID) as RowNum
				from dbo.Product p with(NOLOCK)
				left join dbo.ProductDistributor pd with(NOLOCK) on pd.ProductID = p.ProductID
				left join dbo.ProductVariant pv with(NOLOCK) on pv.ProductID = p.ProductID
				left join dbo.SalesPrompt sp with(NOLOCK) on sp.SalesPromptID = p.SalesPromptID
				where p.Published=1 and p.Deleted=0 and pd.DistributorID = @entityid and pv.IsDefault=1
			) ent
			WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
    ELSE IF @entitytype = 5   
        BEGIN
		SELECT @productcount = COUNT(pg.ProductID) from dbo.ProductGenre pg with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pg.ProductID where p.Published=1 and p.Deleted=0 and pg.GenreID = @entityid
	
		SELECT ProductID, Name, SEName, Description, Price, SalePrice, SalesPrompt, VariantID, TaxClassID FROM
			( select p.ProductID,
				p.Name, 
				p.SEName,
				p.Description, 
				pv.Price,
				pv.SalePrice, 
				pv.VariantID,
				p.TaxClassID, 
				sp.Name as SalesPrompt,
				ROW_NUMBER() OVER(ORDER BY pg.DisplayOrder, p.Name, p.ProductID) as RowNum
				from dbo.Product p with(NOLOCK)
				left join dbo.ProductGenre pg with(NOLOCK) on pg.ProductID = p.ProductID
				left join dbo.ProductVariant pv with(NOLOCK) on pv.ProductID = p.ProductID
				left join dbo.SalesPrompt sp with(NOLOCK) on sp.SalesPromptID = p.SalesPromptID
				where p.Published=1 and p.Deleted=0 and pg.GenreID = @entityid and pv.IsDefault=1
			) ent
			WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
    ELSE IF @entitytype = 6  
        BEGIN
		SELECT @productcount = COUNT(pv.ProductID) from dbo.ProductVector pv with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pv.ProductID where p.Published=1 and p.Deleted=0 and pv.VectorID = @entityid
	
		SELECT ProductID, Name, SEName, Description, Price, SalePrice, SalesPrompt, VariantID, TaxClassID FROM
			( select p.ProductID,
				p.Name, 
				p.SEName,
				p.Description, 
				pv.Price,
				pv.SalePrice,
				pv.VariantID,
				p.TaxClassID, 
				sp.Name as SalesPrompt,
				ROW_NUMBER() OVER(ORDER BY pvc.DisplayOrder, p.Name, p.ProductID) as RowNum
				from dbo.Product p with(NOLOCK)
				left join dbo.ProductVector pvc with(NOLOCK) on pvc.ProductID = p.ProductID
				left join dbo.ProductVariant pv with(NOLOCK) on pv.ProductID = p.ProductID
				left join dbo.SalesPrompt sp with(NOLOCK) on sp.SalesPromptID = p.SalesPromptID
				where p.Published=1 and p.Deleted=0 and pvc.VectorID = @entityid and pv.IsDefault=1
			) ent
			WHERE RowNum > @lowbounds and RowNum <= @highbounds
		END
	END
	
set @numpages = case when (@productcount % @pagesize = 0) then @productcount / @pagesize else (@productcount/@pagesize) + 1 end 
	
return @productcount

GO

PRINT N'Creating [dbo].[aspdnsf_getBadWord]'
GO


create proc [dbo].[aspdnsf_getBadWord]
    @BadWordID int   = null
  
AS
SET NOCOUNT ON 
SELECT [BadWordID],
      [LocaleSetting],
      [Word],
      [CreatedOn]
FROM [dbo].[BadWord] with (nolock) 
WHERE [BadWordID] = COALESCE(@BadWordID,[BadWordID])
ORDER BY BadWordID


GO
PRINT N'Creating [dbo].[aspdnsf_GetProductCount]'
GO

/****** Object:  StoredProcedure [dbo].[aspdnsf_GetProductCount]    ******/
CREATE proc [dbo].[aspdnsf_GetProductCount]
	@entitytype		int, -- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector  
	@entityid		int
 
 
AS  
BEGIN  
  
 SET NOCOUNT ON   
  
	IF @entityid is null
		set @entityid = 0
	
	DECLARE @productcount int
	
	-- 1 = category, 2 = section, 3 = manufacturer, 4 = distributor, 5= genre, 6 = vector
	IF @entitytype = 1
		SELECT @productcount = COUNT(pc.ProductID) from dbo.ProductCategory pc with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pc.ProductID where p.Published=1 and p.Deleted=0 and pc.CategoryID = @entityid
    ELSE IF @entitytype = 2  
		SELECT @productcount = COUNT(ps.ProductID) from dbo.ProductSection ps with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = ps.ProductID where p.Published=1 and p.Deleted=0 and ps.SectionID = @entityid
    ELSE IF @entitytype = 3  
        SELECT @productcount = COUNT(pm.ProductID) from dbo.ProductManufacturer pm with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pm.ProductID where p.Published=1 and p.Deleted=0 and pm.ManufacturerID = @entityid
    ELSE IF @entitytype = 4 
        SELECT @productcount = COUNT(pd.ProductID) from dbo.ProductDistributor pd with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pd.ProductID where p.Published=1 and p.Deleted=0 and pd.DistributorID = @entityid
    ELSE IF @entitytype = 5   
        SELECT @productcount = COUNT(pg.ProductID) from dbo.ProductGenre pg with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pg.ProductID where p.Published=1 and p.Deleted=0 and pg.GenreID = @entityid
    ELSE IF @entitytype = 6  
        SELECT @productcount = COUNT(pv.ProductID) from dbo.ProductVector pv with(NOLOCK) left join dbo.Product p with(NOLOCK) on p.ProductID = pv.ProductID where p.Published=1 and p.Deleted=0 and pv.VectorID = @entityid
	END

return @productcount

GO

PRINT N'Creating [dbo].[aspdnsf_insBadWord]'
GO


create proc [dbo].[aspdnsf_insBadWord]
    @LocaleSetting nvarchar(10),
    @Word nvarchar(100),
    @BadWordID int OUTPUT
      
AS
SET NOCOUNT ON 

INSERT INTO [dbo].[BadWord] (LocaleSetting, Word, CreatedOn)
VALUES(@LocaleSetting,@Word,getdate())

set @BadWordId = @@Identity

GO
PRINT N'Creating [dbo].[aspdnsf_updBadWord]'
GO


create proc [dbo].[aspdnsf_updBadWord]
    @BadWordID int,
    @LocaleSetting nvarchar(10)      = null,
    @Word nvarchar(100)              = null
  
AS    
UPDATE [dbo].[BadWord]
   SET [LocaleSetting]               = COALESCE(@LocaleSetting,[LocaleSetting]),
       [Word]                        = COALESCE(@Word,[Word])
 WHERE BadWordID = @BadWordID

GO
PRINT N'Creating [dbo].[aspdnsf_SearchCategories]'
GO


create proc [dbo].[aspdnsf_SearchCategories]
    @SearchTerm nvarchar(3000),
    @CategoryID int = null,
    @storeID	int = 1,	
    @filterEntity bit
    
  
AS
BEGIN
SET NOCOUNT ON

    SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
    SET @CategoryID = nullif(@CategoryID, 0)

    SELECT a.* 
    FROM dbo.Category a with (NOLOCK)
    inner join (SELECT DISTINCT CategoryID EntityID FROM Category A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.CategoryID = B.EntityID AND EntityType = 'Category' WHERE (@filterEntity = 0 or StoreID = @storeID)) B ON A.CategoryID = B.EntityID
    WHERE a.name like @SearchTerm 
        and Published <> 0 
        and Deleted = 0 
        and CategoryID = coalesce(@CategoryID, CategoryID)
        and CategoryID in (select distinct CategoryID 
                           from dbo.ProductCategory   with (NOLOCK)  
                           where ProductID in 
                                (select ProductID 
                                 from dbo.Product   with (NOLOCK)  
                                 where Deleted=0 and Published=1
                                )
                          )
    ORDER BY DisplayOrder, Name

END

GO

PRINT N'Creating [dbo].[aspdnsf_SearchSections]'
GO


create proc [dbo].[aspdnsf_SearchSections]
    @SearchTerm nvarchar(3000),
    @SectionID int = null,
    @storeID	int = 1,	
    @filterEntity bit	
    
  
AS
BEGIN
SET NOCOUNT ON

    SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
    SET @SectionID = nullif(@SectionID, 0)


    SELECT * 
    FROM dbo.[Section] a with (NOLOCK)
    inner join (SELECT DISTINCT SectionID EntityID FROM Section A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.SectionID = B.EntityID AND EntityType = 'Section' WHERE (@filterEntity = 0 or StoreID = @storeID)) B ON A.SectionID = B.EntityID
    WHERE Deleted=0 and Published=1 
        and SectionID = coalesce(@SectionID, SectionID)
        and a.Name like @SearchTerm
        and SectionID in (select distinct SectionID 
                          from dbo.ProductSection   with (NOLOCK)  
                          where ProductID in (select distinct ProductID 
                                              from dbo.Product   with (NOLOCK)  
                                              where Deleted=0 and Published=1
                                              )
                         ) 
    ORDER BY DisplayOrder, Name
END

GO
PRINT N'Creating [dbo].[aspdnsf_SearchManufacturers]'
GO


create proc [dbo].[aspdnsf_SearchManufacturers]
    @SearchTerm		nvarchar(3000),
    @ManufacturerID int = null,
    @storeID		int = 1,	
    @filterEntity bit
    
  
AS
BEGIN
SET NOCOUNT ON

    SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
    SET @ManufacturerID = nullif(@ManufacturerID, 0)

    SELECT * 
    FROM dbo.Manufacturer a with (NOLOCK)
    inner join (SELECT DISTINCT ManufacturerID EntityID FROM Manufacturer A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.ManufacturerID = B.EntityID AND EntityType = 'Manufacturer' WHERE (@filterEntity = 0 or StoreID = @storeID)) B ON A.ManufacturerID = B.EntityID
    WHERE deleted=0 
        and name like @SearchTerm 
        and ManufacturerID = coalesce(@ManufacturerID, ManufacturerID)
END
GO
PRINT N'Creating [dbo].[aspdnsf_SearchDistributors]'
GO



create proc [dbo].[aspdnsf_SearchDistributors]
    @SearchTerm		nvarchar(3000),
    @DistributorID	int = null,
    @storeID		int = 1,	
    @filterEntity bit
    
  
AS
BEGIN
SET NOCOUNT ON

    SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
    SET @DistributorID = nullif(@DistributorID, 0)

    SELECT * 
    FROM dbo.Distributor a with (NOLOCK)
    inner join (SELECT DISTINCT DistributorID EntityID FROM Distributor A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.DistributorID = B.EntityID AND EntityType = 'Distributor' WHERE (@filterEntity = 0 or StoreID = @storeID)) B ON A.DistributorID = B.EntityID
    WHERE deleted=0 
        and name like @SearchTerm 
        and DistributorID = coalesce(@DistributorID, DistributorID)
END
GO
PRINT N'Creating [dbo].[aspdnsf_SearchGenres]'
GO


create proc [dbo].[aspdnsf_SearchGenres]
    @SearchTerm nvarchar(3000),
    @GenreID int = null,
    @storeID		int = 1,	
    @filterEntity bit
    
  
AS
BEGIN
SET NOCOUNT ON

    SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
    SET @GenreID = nullif(@GenreID, 0)

    SELECT * 
    FROM dbo.Genre a with (NOLOCK) 
    inner join (SELECT DISTINCT GenreID EntityID FROM Genre A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.GenreID = B.EntityID AND EntityType = 'Genre' WHERE (@filterEntity = 0 or StoreID = @storeID)) B ON A.GenreID = B.EntityID
    WHERE deleted=0 
        and name like @SearchTerm 
        and GenreID = coalesce(@GenreID, GenreID)

END

GO
PRINT N'Creating [dbo].[aspdnsf_SearchVectors]'
GO



create proc [dbo].[aspdnsf_SearchVectors]
    @SearchTerm nvarchar(3000),
    @VectorID int = null,
    @storeID		int = 1,	
    @filterEntity bit
    
  
AS
BEGIN
SET NOCOUNT ON

    SET @SearchTerm = '%' + rtrim(ltrim(@SearchTerm)) + '%'
    SET @VectorID = nullif(@VectorID, 0)

    SELECT * 
    FROM dbo.vector a with (NOLOCK) 
    inner join (SELECT DISTINCT VectorID EntityID FROM Vector A WITH (NOLOCK) LEFT JOIN EntityStore B WITH (NOLOCK) ON A.VectorID = B.EntityID AND EntityType = 'Vector' WHERE (@filterEntity = 0 or StoreID = @storeID)) B ON A.VectorID = B.EntityID
    WHERE deleted=0 
        and name like @SearchTerm 
        and VectorID = coalesce(@VectorID, VectorID)
END

GO
PRINT N'Creating [dbo].[aspdnsf_CreateSubEntities]'
GO


create proc dbo.aspdnsf_CreateSubEntities
    @EntityName varchar(20),
    @EntityID int,
    @EntityList varchar(8000)
  
AS
BEGIN
SET NOCOUNT ON

IF RTRIM(@EntityList) = ''
    RETURN

IF @EntityName = 'category'
    INSERT INTO [dbo].[Category] (CategoryGUID, Name, ParentCategoryID, XmlPackage, SEName, ColWidth, PageSize)
    SELECT newid(), s.Items, @EntityID, e.XmlPackage, dbo.MakeSEName(s.Items), e.ColWidth, e.PageSize
    FROM [dbo].[Category] e with (nolock) cross join dbo.split(@EntityList, ',') s
    WHERE categoryid = @EntityID 

IF @EntityName = 'section'
    INSERT INTO [dbo].Section (SectionGUID, Name, ParentSectionID, XmlPackage, SEName, ColWidth, PageSize)
    SELECT newid(), s.Items, @EntityID, e.XmlPackage, dbo.MakeSEName(s.Items), e.ColWidth, e.PageSize
    FROM [dbo].[Section] e with (nolock) cross join dbo.split(@EntityList, ',') s
    WHERE Sectionid = @EntityID 

IF @EntityName = 'distributor'
    INSERT INTO [dbo].Distributor (DistributorGUID, Name, ParentDistributorID, XmlPackage, SEName, ColWidth, PageSize)
    SELECT newid(), s.Items, @EntityID, e.XmlPackage, dbo.MakeSEName(s.Items), e.ColWidth, e.PageSize
    FROM [dbo].Distributor e with (nolock) cross join dbo.split(@EntityList, ',') s
    WHERE Distributorid = @EntityID 

IF @EntityName = 'manufacturer'
    INSERT INTO [dbo].Manufacturer (ManufacturerGUID, Name, ParentManufacturerID, XmlPackage, SEName, ColWidth, PageSize)
    SELECT newid(), s.Items, @EntityID, e.XmlPackage, dbo.MakeSEName(s.Items), e.ColWidth, e.PageSize
    FROM [dbo].Manufacturer e with (nolock) cross join dbo.split(@EntityList, ',') s
    WHERE Manufacturerid = @EntityID 

IF @EntityName = 'affiliate'
    INSERT INTO [dbo].Affiliate (AffiliateGUID, Name, ParentAffiliateID, SEName)
    SELECT newid(), s.Items, @EntityID, dbo.MakeSEName(s.Items)
    FROM [dbo].Affiliate e with (nolock) cross join dbo.split(@EntityList, ',') s
    WHERE Affiliateid = @EntityID 

IF @EntityName = 'genre'
    INSERT INTO [dbo].Genre (GenreGUID, Name, ParentGenreID, XmlPackage, SEName, ColWidth, PageSize)
    SELECT newid(), s.Items, @EntityID, e.XmlPackage, dbo.MakeSEName(s.Items), e.ColWidth, e.PageSize
    FROM [dbo].Genre e with (nolock) cross join dbo.split(@EntityList, ',') s
    WHERE Genreid = @EntityID 

IF @EntityName = 'vector'
    INSERT INTO [dbo].Vector (VectorGUID, Name, ParentVectorID, XmlPackage, SEName, ColWidth, PageSize)
    SELECT newid(), s.Items, @EntityID, e.XmlPackage, dbo.MakeSEName(s.Items), e.ColWidth, e.PageSize
    FROM [dbo].Vector e with (nolock) cross join dbo.split(@EntityList, ',') s
    WHERE Vectorid = @EntityID 

END
GO
PRINT N'Creating [dbo].[aspdnsf_EntityMgr]'
GO


create proc [dbo].[aspdnsf_EntityMgr]
    @EntityName varchar(100),
    @PublishedOnly tinyint
    
  
AS
BEGIN
    SET NOCOUNT ON
    IF @EntityName = 'Category' BEGIN
        SELECT Entity.CategoryID EntityID, Entity.CategoryGUID EntityGuid, Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentCategoryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor
			   ,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Category Entity with (NOLOCK)
          left join (SELECT pc.CategoryID, COUNT(pc.ProductID) AS NumProducts
                     FROM  dbo.ProductCategory pc with (nolock)
                         join [dbo].Product p with (nolock) on pc.ProductID = p.ProductID  and p.deleted=0 and p.published=1
                     GROUP BY pc.CategoryID
                    ) a on Entity.CategoryID = a.CategoryID
        WHERE Published >= @PublishedOnly and Deleted=0
        ORDER BY ParentCategoryID,DisplayOrder,Name
    END
 
 
 
    IF @EntityName = 'Affiliate' BEGIN
        SELECT Entity.AffiliateID EntityID,Entity.AffiliateGUID EntityGuid, Name,4 as ColWidth,'' as Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentAffiliateID ParentEntityID,DisplayOrder,0 as SortByLooks,'' as XmlPackage,Published,'' as ContentsBGColor,'' as PageBGColor
			   ,'' as GraphicsColor,isnull(NumProducts, 0) NumObjects, 0 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
        FROM dbo.Affiliate Entity with (NOLOCK)
          left join (SELECT pa.AffiliateID, COUNT(pa.ProductID) AS NumProducts
                     FROM dbo.ProductAffiliate pa with (nolock) join [dbo].Product p with (nolock) on pa.ProductID = p.ProductID and p.deleted=0 and p.published=1
                     GROUP BY pa.AffiliateID
                    ) a on Entity.AffiliateID = a.AffiliateID
        WHERE Published >= @PublishedOnly and Deleted=0
        ORDER BY ParentAffiliateID, DisplayOrder,Name
    END
 
 
 
    IF @EntityName = 'Section' BEGIN
        SELECT Entity.SectionID EntityID,Entity.SectionGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentSectionID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor,isnull(NumProducts, 0) NumObjects
			   , PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Section Entity with (NOLOCK)
            left join (SELECT ps.SectionID, COUNT(ps.ProductID) AS NumProducts
                       FROM dbo.ProductSection ps with (nolock) join [dbo].Product p with (nolock) on ps.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY ps.SectionID
                      ) a on Entity.SectionID = a.SectionID
        WHERE Published >= @PublishedOnly and Deleted=0
        ORDER BY ParentSectionID,DisplayOrder,Name
    END
 
 
 
    IF @EntityName = 'Manufacturer' BEGIN
        SELECT Entity.ManufacturerID EntityID,Entity.ManufacturerGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentManufacturerID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor
			   ,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Manufacturer Entity with (NOLOCK)
        left join (SELECT pm.ManufacturerID, COUNT(pm.ProductID) AS NumProducts
                   FROM dbo.ProductManufacturer pm with (nolock) join [dbo].Product p with (nolock) on pm.ProductID = p.ProductID and p.deleted=0 and p.published=1
                   GROUP BY pm.ManufacturerID
                  ) a on Entity.ManufacturerID = a.ManufacturerID
        WHERE Published >= @PublishedOnly and Deleted=0
        ORDER BY ParentManufacturerID,DisplayOrder,Name
    END
 
 
 
    IF @EntityName = 'Library' BEGIN
        SELECT Entity.LibraryID EntityID,Entity.LibraryGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentLibraryID ParentEntityID,DisplayOrder,SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor
			   ,isnull(NumDocuments, 0) NumObjects, PageSize, 0 QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Library Entity with (NOLOCK)
        left join (SELECT dl.LibraryID, COUNT(dl.DocumentID) AS NumDocuments
                       FROM  dbo.DocumentLibrary dl with (nolock) 
                           join [dbo].[Document] d with (nolock) on d.DocumentID = dl.DocumentID and d.deleted=0 and d.published=1
                       GROUP BY dl.LibraryID
                      ) a on Entity.LibraryID = a.LibraryID
        WHERE Published >= @PublishedOnly and Deleted=0
        ORDER BY ParentLibraryID,DisplayOrder,Name
    END
 
 
 
    IF @EntityName = 'Distributor' BEGIN
        SELECT Entity.DistributorID EntityID,Entity.DistributorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentDistributorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor
			   ,GraphicsColor,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Distributor Entity with (NOLOCK)
        left join (SELECT pd.DistributorID, COUNT(pd.ProductID) AS NumProducts
                       FROM dbo.ProductDistributor pd with (nolock) join [dbo].Product p with (nolock) on pd.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY pd.DistributorID
                      ) a on Entity.DistributorID = a.DistributorID
        WHERE Published >= @PublishedOnly and Deleted=0
        ORDER BY ParentDistributorID,DisplayOrder,Name
    END
 
 
    IF @EntityName = 'Genre' BEGIN
        SELECT Entity.GenreID EntityID,Entity.GenreGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentGenreID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor
			   ,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Genre Entity with (NOLOCK)
        left join (SELECT px.GenreID, COUNT(px.ProductID) AS NumProducts
                       FROM dbo.ProductGenre px with (nolock) join [dbo].Product p with (nolock) on px.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY px.GenreID
                      ) a on Entity.GenreID = a.GenreID
        WHERE Published >= @PublishedOnly and Deleted=0
        ORDER BY ParentGenreID,DisplayOrder,Name
    END
 
    IF @EntityName = 'Vector' BEGIN
        SELECT Entity.VectorID EntityID,Entity.VectorGUID EntityGuid,Name,ColWidth,Description,SEName,SEKeywords,SEDescription,SETitle,SENoScript,SEAltText,ParentVectorID as ParentEntityID,DisplayOrder,0 as SortByLooks,XmlPackage,Published,ContentsBGColor,PageBGColor,GraphicsColor
			   ,isnull(NumProducts, 0) NumObjects, PageSize, QuantityDiscountID, Summary, SkinID, TemplateName
        FROM dbo.Vector Entity with (NOLOCK)
        left join (SELECT px2.VectorID, COUNT(px2.ProductID) AS NumProducts
                       FROM dbo.ProductVector px2 with (nolock) join [dbo].Product p with (nolock) on px2.ProductID = p.ProductID and p.deleted=0 and p.published=1
                       GROUP BY px2.VectorID
                      ) a on Entity.VectorID = a.VectorID
        WHERE Published >= @PublishedOnly and Deleted=0
        ORDER BY ParentVectorID,DisplayOrder,Name
    END
 
 
    IF @EntityName = 'Customerlevel' BEGIN
        SELECT Entity.CustomerLevelID EntityID,Entity.CustomerLevelGUID EntityGuid,Name, 4 ColWidth, '' Description,SEName, '' SEKeywords, '' SEDescription, '' SETitle, '' SENoScript,'' SEAltText,ParentCustomerLevelID ParentEntityID,DisplayOrder,0 SortByLooks, '' XmlPackage, 1 Published,'' ContentsBGColor
			   , '' PageBGColor, '' GraphicsColor,isnull(NumProducts, 0) NumObjects, 20 PageSize, 0 QuantityDiscountID, '' Summary, SkinID, TemplateName
        FROM dbo.CustomerLevel Entity with (NOLOCK)
          left join (SELECT pc.CustomerLevelID, COUNT(pc.ProductID) AS NumProducts
                     FROM  dbo.ProductCustomerLevel pc with (nolock)
                         join [dbo].Product p with (nolock) on pc.ProductID = p.ProductID  and p.deleted=0 and p.published=1
                     GROUP BY pc.CustomerLevelID
                    ) a on Entity.CustomerLevelID = a.CustomerLevelID
        WHERE Deleted=0
        ORDER BY ParentCustomerLevelID, DisplayOrder,Name
    END
END

GO

PRINT N'Creating [dbo].[aspdnsf_GetProductsEntity]'
GO


create proc [dbo].[aspdnsf_GetProductsEntity]
    @categoryID      int = 0,
    @sectionID       int = 0,
    @manufacturerID  int = 0,
    @distributorID   int = 0,
    @genreID   int = 0,
    @vectorID   int = 0,
    @affiliateID     int = 0,
    @ProductTypeID   int = 1,
    @ViewType        bit = 1, -- 0 = all variants, 1 = one variant
    @StatsFirst      tinyint = 1,
    @searchstr       nvarchar(4000) = '',
    @extSearch       tinyint = 1,
    @publishedonly   tinyint = 0,
    @OnSaleOnly      tinyint = 0,
    @SearchIndex     varchar(2) = '',
    @SortOrder       varchar(4) = 'ASC', -- ASC or DESC
    @SortBy          varchar(50) = 'Name' -- name to sort by
  
AS
BEGIN

    SET NOCOUNT ON 

    DECLARE @rcount int

    DECLARE @custlevelcount int, @sectioncount int, @affiliatecount int, @categorycount int, @distributorcount int, @genrecount int,  @vectorcount int, @manufacturercount int

    DECLARE @FilterProductsByAffiliate tinyint
    SELECT @FilterProductsByAffiliate = case ConfigValue when 'true' then 1 else 0 end FROM AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate'

    select @categorycount     = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productcategory') and si.indid < 2 and type = 'u'
    select @sectioncount      = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('productsection') and si.indid < 2 and type = 'u'
    select @affiliatecount    = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductAffiliate') and si.indid < 2 and type = 'u'
    select @distributorcount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductDistributor') and si.indid < 2 and type = 'u'
    select @genrecount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductGenre') and si.indid < 2 and type = 'u'
    select @vectorcount  = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductVector') and si.indid < 2 and type = 'u'
    select @manufacturercount = si.rows from sysobjects so with (nolock) join sysindexes si with (nolock) on so.id = si.id where so.id = object_id('ProductManufacturer') and si.indid < 2 and type = 'u'


    SET @searchstr = '%' + @searchstr + '%'
    SET @SearchIndex = @SearchIndex + '%'


    SET @rcount = @@rowcount
    IF @StatsFirst = 1
        SELECT cast(ceiling(@rcount*1.0/1) as int) pages, @rcount ProductCount

    DECLARE @sql nvarchar(4000)
    SET @sql = '
    SELECT 
        p.ProductID,
        p.Name,
        pv.VariantID,
        pv.Name AS VariantName,
        p.ProductGUID,
        p.Summary,
        p.Description,
        p.ProductTypeID,
        p.TaxClassID,
        p.SKU,
        p.ManufacturerPartNumber,
        p.XmlPackage,
        p.Published,
        p.Looks,
        p.Notes,
        p.IsAKit,
        p.ShowInProductBrowser,
        p.IsAPack,
        p.PackSize,
        p.IsSystem,
        p.Deleted,
        p.CreatedOn,
        p.ImageFileNameOverride,
        pv.VariantGUID,
        pv.Description AS VariantDescription,
        pv.SKUSuffix,
        pv.ManufacturerPartNumber AS VariantManufacturerPartNumber,
        pv.Price,
        pv.CustomerEntersPrice, 
        isnull(pv.SalePrice, 0) SalePrice,
        cast(isnull(pv.Weight,0) as decimal(10,1)) Weight,
        pv.MSRP,
        pv.Cost,
        case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end Inventory,
        pv.DisplayOrder as VariantDisplayOrder,
        pv.Notes AS VariantNotes,
        pv.IsTaxable,
        pv.IsShipSeparately,
        pv.IsDownload,
        pv.DownloadLocation,
        pv.Published AS VariantPublished,
        pv.SubscriptionInterval,
        pv.RestrictedQuantities,
        pv.MinimumQuantity,
        pv.Deleted AS VariantDeleted,
        pv.CreatedOn AS VariantCreatedOn,
        d.Name AS DistributorName,
        x.Name AS GenreName,
        x2.Name AS SHowName,
        d.DistributorID,
        x.GenreID,
        x2.VectorID,
        m.ManufacturerID,
        m.Name AS ManufacturerName'

    DECLARE @sql1 nvarchar(4000)
    SET @sql1 = '
    FROM Product p with (NOLOCK) 
        join 
        (
        SELECT distinct p.productid, pv.VariantID 
            FROM 
                product p with (nolock)
                left join ProductVariant pv             with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= '

    SET @sql1 = @sql1 +CONVERT(nvarchar,@ViewType)+'
                left join productcategory pc       with (nolock) on p.ProductID = pc.ProductID 
                left join productsection ps        with (nolock) on p.ProductID = ps.ProductID 
                left join ProductManufacturer pm   with (nolock) on p.ProductID = pm.ProductID 
                left join ProductDistributor pd    with (nolock) on p.ProductID = pd.ProductID 
                left join ProductGenre px    with (nolock) on p.ProductID = px.ProductID 
                left join ProductVector px2    with (nolock) on p.ProductID = px2.ProductID 
                left join ProductAffiliate pa      with (nolock) on p.ProductID = pa.ProductID 
            WHERE 
                  (pc.categoryid = '


    DECLARE @sql2 nvarchar(4000)

    SET @sql2 = ' ' + CONVERT(nvarchar,@categoryID) + ') or (ps.sectionid = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@sectionID) + ') or (pa.AffiliateID = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@affiliateID) + ') or (pm.manufacturerid = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@manufacturerID) + ') or (pd.DistributorID = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@distributorID) + ') or (px.genreID = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@genreID) + ') or (px2.vectorID = '

    SET @sql2 = @sql2 + CONVERT(nvarchar,@vectorID) + ')'

        DECLARE @sql3 nvarchar(4000)
        SET @sql3 = ' 
          and case when isnull(pv.saleprice,0) = 0 then 0 else 1 end >= '+CONVERT(nvarchar,@OnSaleOnly)+'
          and p.published >= '+CONVERT(nvarchar,@publishedonly)+'
          and pv.published >= '+CONVERT(nvarchar,@publishedonly)+'
          and p.Deleted = 0
          and pv.Deleted = 0
        )                              pf on p.ProductID = pf.ProductID  
        left join ProductVariant      pv  with (NOLOCK) on p.ProductID = pv.ProductID and pv.IsDefault >= '

    SET @sql3 = @sql3 + CONVERT(nvarchar,@ViewType)+'
        left join ProductManufacturer pm  with (NOLOCK) on p.ProductID = pm.ProductID 
        left join Manufacturer         m  with (NOLOCK) on pm.ManufacturerID = m.ManufacturerID 
        left join ProductDistributor  pd  with (NOLOCK) on p.ProductID = pd.ProductID
        left join ProductGenre  px  with (NOLOCK) on p.ProductID = px.ProductID
        left join ProductVector  px2  with (NOLOCK) on p.ProductID = px2.ProductID
        left join Distributor          d  with (NOLOCK) on pd.DistributorID = d.DistributorID
        left join Genre          x  with (NOLOCK) on px.GenreID = x.GenreID
        left join Vector          x2  with (NOLOCK) on px2.VectorID = x2.VectorID
        left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i  on pv.VariantID = i.VariantID
    WHERE
        (p.ProductTypeID = '+CONVERT(nvarchar,@ProductTypeID)+ ' or  '+CONVERT(nvarchar,@ProductTypeID)+ ' = 0) and 
        (
          p.Name LIKE '''+ @searchstr + ''' 
          or convert(nvarchar(20),p.productid) LIKE '''+ @searchstr + '''
          or pv.name LIKE '''+ @searchstr + ''' 
          or p.sku LIKE '''+ @searchstr + ''' 
          or p.manufacturerpartnumber LIKE '''+ @searchstr + ''' 
          or pv.manufacturerpartnumber LIKE '''+ @searchstr + ''' 
          or ('+CONVERT(nvarchar,@extSearch)+' = 1 AND p.Description LIKE '''+ @searchstr + ''')
          or ('+CONVERT(nvarchar,@extSearch)+' = 1 AND p.Summary LIKE '''+ @searchstr + ''')
        )
        and 
        p.Name LIKE '''+ @SearchIndex + ''' 
    ORDER BY '

DECLARE @sql4 nvarchar(4000)
    
    IF @SortBy = 'ProductID' 
        SET @sql4 = 'P.ProductID'
    ELSE IF @SortBy = 'SKU' 
        SET @sql4 = 'SKU'
    ELSE IF @SortBy = 'ManufacturerPartNumber' 
        SET @sql4 = 'P.ManufacturerPartNumber'
    ELSE IF @SortBy = 'Inventory' 
        SET @sql4 = 'Inventory'
    ELSE 
        SET @sql4 = 'P.[Name]'
    
    
    IF @SortOrder = 'DESC' 
        SET @sql4 = @sql4 + ' DESC'
    ELSE 
        SET @sql4 = @sql4 + ' ASC'
    
    SET @sql4 = @sql4 + ', pv.DisplayOrder'

    EXECUTE(@sql+' '+@sql1+' '+@sql2+' '+@sql3+' '+@sql4)

    IF @StatsFirst <> 1
        SELECT cast(ceiling(@rcount*1.0/1) as int) pages, @rcount ProductCount

END

GO
PRINT N'Creating [dbo].[aspdnsf_GetProducts]'
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
	@filterProduct	 bit = 0
	
  
AS  
BEGIN  
  
    SET NOCOUNT ON   
  
    DECLARE @rcount int  
    DECLARE @productfilter table (rownum int not null identity  primary key, productid int not null, displayorder int not null, VariantID int not null, VariantDisplayOrder int not null, ProductName nvarchar(400) null, VariantName nvarchar(400) null)  
    DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @DisplayOutOfStockProducts tinyint, @HideProductsWithLessThanThisInventoryLevel int  
    CREATE TABLE #displayorder ([name] nvarchar (800), productid int not null primary key, displayorder int not null)  
    CREATE TABLE #inventoryfilter (productid int not null, variantid int not null, InvQty int not null)  
    CREATE CLUSTERED INDEX tmp_inventoryfilter ON #inventoryfilter (productid, variantid)  
  
    DECLARE @custlevelcount int, @sectioncount int, @localecount int, @affiliatecount int, @categorycount int, @CustomerLevelFilteringIsAscending bit, @distributorcount int, @genrecount int, @vectorcount int, @manufacturercount int  
  
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
          and (  
                 @searchstr is null  
              or patindex(@searchstr, isnull(p.name, '')) > 0  
              or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0   
              or patindex(@searchstr, isnull(pv.name, '')) > 0  
              or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0  
              or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0  
              or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0  
              or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)  
              or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)  
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
          and (  
                 @searchstr is null  
              or patindex(@searchstr, isnull(p.name, '')) > 0  
              or patindex(@searchstr, isnull(convert(nvarchar(20),p.productid), '')) > 0   
              or patindex(@searchstr, isnull(pv.name, '')) > 0  
              or patindex(@searchstr, isnull(p.sku , '')+isnull(pv.skusuffix , '')) > 0  
              or patindex(@searchstr, isnull(p.manufacturerpartnumber, '')) > 0  
              or patindex(@searchstr, isnull(pv.manufacturerpartnumber, '')) > 0  
              or (patindex(@searchstr, isnull(p.Description, '')) > 0 and @extSearch = 1)  
              or (patindex(@searchstr, isnull(p.Summary, '')) > 0 and @extSearch = 1)  
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
        join @productfilter                pf                on pv.ProductID = pf.ProductID and pv.VariantID = pf.VariantID   
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
PRINT N'Creating [dbo].[aspdnsf_ProductInfo]'
GO



create proc [dbo].[aspdnsf_ProductInfo]
    @ProductID          int,  
    @CustomerLevelID    int,  
    @DefaultVariantOnly tinyint,  
    @InvFilter          int = 0,  
    @affiliateID        int = null,  
    @publishedonly      tinyint = 1  
  
AS  
SET NOCOUNT ON  
  
DECLARE @CustLevelExists int, @AffiliateExists  int, @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @CustomerLevelFilteringIsAscending tinyint,@DisplayOutOfStockProducts tinyint, @custlevelcount int, @affiliatecount int, @minproductcustlevel int, @HideProductsWithLessThanThisInventoryLevel int  
SELECT @FilterProductsByCustomerLevel     = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel'  
SELECT @CustomerLevelFilteringIsAscending = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterByCustomerLevelIsAscending'  
SELECT @FilterProductsByAffiliate         = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate'  
  
SET @minproductcustlevel = 0  
SET @CustLevelExists = 0  
SELECT @custlevelcount = count(*), @minproductcustlevel = min(CustomerLevelID), @CustLevelExists = sum(case when CustomerLevelID = @CustomerLevelID then 1 else 0 end) from dbo.ProductCustomerLevel with (nolock) where ProductID = @ProductID   
SELECT @affiliatecount = count(*),  @AffiliateExists = sum(case when AffiliateID = @affiliateID then 1 else 0 end) from dbo.ProductAffiliate with (nolock) where ProductID = @ProductID   
  
SELECT @DisplayOutOfStockProducts = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'DisplayOutOfStockProducts'
SELECT @HideProductsWithLessThanThisInventoryLevel = convert(int, ConfigValue) FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel' and isnumeric(ConfigValue) = 1  
  
 IF @DisplayOutOfStockProducts = 1 
 BEGIN
        SET @HideProductsWithLessThanThisInventoryLevel = 0
		SET @InvFilter = -1
 END

IF (@HideProductsWithLessThanThisInventoryLevel > @InvFilter or @HideProductsWithLessThanThisInventoryLevel = -1) and @InvFilter <> -1  
    SET @InvFilter = @HideProductsWithLessThanThisInventoryLevel   
  
  
if (@FilterProductsByCustomerLevel = 0 or @custlevelcount = 0 or (@CustomerLevelFilteringIsAscending = 1 and @minproductcustlevel <= @CustomerLevelID ) or @CustLevelExists > 0)  
    and (@FilterProductsByAffiliate = 0 or @affiliatecount = 0 or @AffiliateExists > 0)  
    SELECT   
           p.*,   
           pv.VariantID, pv.name VariantName, pv.Price, pv.Description VariantDescription, isnull(pv.SalePrice, 0) SalePrice, isnull(SkuSuffix, '') SkuSuffix, pv.Dimensions, pv.Weight, isnull(pv.Points, 0) Points, pv.Inventory, pv.ImageFilenameOverride VariantImageFilenameOverride,  pv.isdefault, pv.CustomerEntersPrice, isnull(pv.colors, '') Colors, isnull(pv.sizes, '') Sizes,  
           sp.name SalesPromptName,   
           case when pcl.productid is null then 0 else isnull(e.Price, 0) end ExtendedPrice  
    FROM dbo.Product p with (nolock) join dbo.productvariant            pv  with (NOLOCK) on p.ProductID = pv.ProductID     
        join dbo.SalesPrompt               sp  with (NOLOCK) on p.SalesPromptID = sp.SalesPromptID   
        left join dbo.ExtendedPrice        e   with (NOLOCK) on pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID  
        left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID  
        left join (select variantid, sum(quan) inventory from inventory group by variantid) i on pv.variantid = i.variantid  
    WHERE p.ProductID = @ProductID   
        and p.Deleted = 0   
        and pv.Deleted = 0   
        and p.published >= @publishedonly  
        and pv.published >= @publishedonly  
        and pv.isdefault >= @DefaultVariantOnly  
        and getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999')  
        and (case p.TrackInventoryBySizeAndColor when 1 then isnull(i.inventory, 0) else pv.inventory end >= @InvFilter or @InvFilter = -1)  
    ORDER BY pv.DisplayOrder, pv.name  

GO
PRINT N'Creating [dbo].[aspdnsf_EditOrderProduct]'
GO


create proc [dbo].[aspdnsf_EditOrderProduct]
    @ShoppingCartRecID int,
    @CustomerLevelID   int
  
AS
SET NOCOUNT ON 

    SELECT p.*, 
           pv.VariantID, pv.name VariantName, pv.Price, isnull(pv.SalePrice, 0) SalePrice, isnull(SkuSuffix, '') SkuSuffix, pv.Dimensions, pv.Weight, isnull(pv.Points, 0) Points, pv.Inventory, pv.ImageFilenameOverride VariantImageFilenameOverride,  pv.isdefault, pv.CustomerEntersPrice,
           sp.name SalesPromptName, 
           case when pcl.productid is null then 0 else isnull(e.Price, 0) end ExtendedPrice
    FROM dbo.ShoppingCart           sc  with (nolock) 
        join dbo.Product            p   with (NOLOCK) on p.ProductID = sc.ProductID   
        join dbo.productvariant     pv  with (NOLOCK) on sc.ProductID = pv.ProductID and sc.variantid = pv.variantid
        join dbo.SalesPrompt        sp  with (NOLOCK) on p.SalesPromptID = sp.SalesPromptID 
        left join dbo.ExtendedPrice e   with (NOLOCK) on pv.VariantID=e.VariantID and e.CustomerLevelID = @CustomerLevelID
        left join dbo.ProductCustomerLevel pcl with (NOLOCK) on p.ProductID = pcl.ProductID and pcl.CustomerLevelID = @CustomerLevelID
    WHERE sc.ShoppingCartRecID = @ShoppingCartRecID
GO
PRINT N'Creating [dbo].[aspdnsf_RecentAdditions]'
GO


create proc [dbo].[aspdnsf_RecentAdditions]
    @since			int = 180,  -- products added in the last "@since" number of days
    @return			int = 10,   -- returns the top "@returns" items
    @storeID		int = 1,
	@filterProduct	bit = 0
	
  
AS
BEGIN
SET NOCOUNT ON

DECLARE @cmd varchar(200)

CREATE TABLE #tmp (id int not null identity, ProductID int, VariantID int, SKU nvarchar(50), SKUSuffix nvarchar(50), ProductName nvarchar(400), SEName nvarchar(150), VariantName nvarchar(400),ImageFilenameOverride ntext)
INSERT #tmp (ProductID, VariantID, SKU, SKUSuffix, ProductName, SEName, VariantName,ImageFilenameOverride)
SELECT 
    p.ProductID, 
    pv.VariantID, 
    p.SKU, 
    isnull(pv.SKUSuffix, '') SKUSuffix, 
    p.Name, 
    p.SEName, 
    isnull(pv.Name, '') VariantName,
    ISNULL(p.ImageFilenameOverride, '') AS ImageFilenameOverride
FROM dbo.Product p with (nolock) 
    join [dbo].ProductVariant pv with (nolock) on pv.productid = p.productid
    inner join (select distinct a.ProductID from Product a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on p.ProductID = b.ProductID
WHERE p.CreatedOn >= dateadd(dy, -@since, getdate())
    and p.Deleted = 0 
    and p.Published = 1  
    and pv.Published = 1 
    and pv.Deleted = 0 
ORDER BY p.CreatedOn desc

SET @cmd = 'select top ' + convert(varchar(10), @return ) + ' * FROM #tmp order by id'

EXEC (@cmd)
END



GO
PRINT N'Creating [dbo].[aspdnsf_BestSellers]'
GO


create proc [dbo].[aspdnsf_BestSellers]
    @since			int = 180,  -- best sellers in the last "@since" number of days
    @return			int = 10,   -- returns the top "@returns" items
    @orderby		tinyint = 1, -- 1 = order by count of sales for each product, 2 = order by total dollars sales for each product
    @storeID		int = 1,
	@filterProduct	bit = 0	
 
  
AS
BEGIN
SET NOCOUNT ON

DECLARE @cmd varchar(200)

CREATE TABLE #tmp (id int not null identity, ProductID int, VariantID int, SKU nvarchar(50), SKUSuffix nvarchar(50), ProductName nvarchar(400), SEName nvarchar(150), VariantName nvarchar(400), SalesCount money, SalesDollars money,ImageFilenameOverride ntext, VariantImageFilenameOverride ntext, VariantCount int)
INSERT #tmp (ProductID, VariantID, SKU, SKUSuffix, ProductName, SEName, VariantName, SalesCount, SalesDollars,ImageFilenameOverride, VariantImageFilenameOverride,VariantCount)
SELECT 
    s.ProductID, 
    s.VariantID, 
    p.SKU, 
    isnull(pv.SKUSuffix, '') SKUSuffix, 
    p.Name, 
    p.SEName, 
    isnull(pv.Name, '') VariantName, 
    s.NumSales, 
    s.NumDollars,
    ISNULL(p.ImageFilenameOverride, '') AS ImageFilenameOverride, 
    ISNULL(pv.ImageFilenameOverride, '') AS VariantImageFilenameOverride,
    (SELECT count(VariantID) from ProductVariant WHERE ProductID = p.productid)
FROM (select ProductID, VariantID, SUM(Quantity) AS NumSales, SUM(OrderedProductPrice) AS NumDollars
      from dbo.Orders_ShoppingCart sc
            join [dbo].Orders o on sc.OrderNumber = o.OrderNumber and o.OrderDate >= dateadd(dy, -@since, getdate())
      group by ProductID, VariantID 
     ) s 
    join [dbo].Product p with (nolock) on s.productid = p.productid 
    join [dbo].ProductVariant pv with (nolock) on s.variantid = pv.variantid
    inner join (select distinct a.ProductID from Product a with (nolock) left join ProductStore b with (nolock) on a.ProductID = b.ProductID where (@filterProduct = 0 or StoreID = @storeID)) b on p.ProductID = b.ProductID
WHERE p.Deleted = 0 
    and p.Published = 1  
    and pv.Published = 1 
    and pv.Deleted = 0 
ORDER BY case @orderby when 1 then s.NumSales when 2 then s.NumDollars else s.NumSales end desc


SET @cmd = 'select top ' + convert(varchar(10), @return ) + ' * FROM #tmp order by id'

EXEC (@cmd)
END

GO
PRINT N'Creating [dbo].[aspdnsf_ProductStats]'
GO


CREATE proc [dbo].[aspdnsf_ProductStats]
    @ProductID	int,
	@storeID	int = 1
  
AS
BEGIN
    SET NOCOUNT ON

    SELECT * 
    FROM dbo.product p with (nolock)
        left join (SELECT ProductID,count(rating) as NumRatings, sum(rating) as SumRatings, convert(decimal(4,3), avg(rating*1.0)) AvgRating
                   FROM dbo.Rating with (nolock)
                   WHERE ProductID = @ProductID and StoreID = @storeID
                   GROUP BY ProductID) ps on p.productid = ps.productid 
    WHERE p.ProductID = @ProductID
END

GO
PRINT N'Creating [dbo].[aspdnsf_ImportProductPricing_XML]'
GO


create proc [dbo].[aspdnsf_ImportProductPricing_XML]
    @pricing ntext
  
AS
BEGIN
SET NOCOUNT ON

CREATE TABLE #tmp (ProductID int, VariantID int, KitItemID int, Name nvarchar(400), KitGroup nvarchar(800), SKU nvarchar(50), SKUSuffix nvarchar(50), ManufacturerPartNumber nvarchar(50), Cost money, MSRP money, Price money, SalePrice money, Inventory int)
DECLARE @hdoc int, @retcode int
EXEC @retcode = sp_xml_preparedocument 
                    @hdoc OUTPUT,
                    @pricing

INSERT #tmp
SELECT *
FROM OPENXML(@hdoc, '/productlist/productvariant', 2) 
        WITH (ProductID int, VariantID int, KitItemID int, Name nvarchar(400), KitGroup nvarchar(800), SKU nvarchar(50), SKUSuffix nvarchar(50), ManufacturerPartNumber nvarchar(50), Cost money, MSRP money, Price money, SalePrice money, Inventory int)


UPDATE dbo.ProductVariant
SET Price = t.Price,
    SalePrice = nullif(t.SalePrice,0),
    Inventory = t.Inventory,
    Cost = t.cost
FROM dbo.ProductVariant p 
    join #tmp t  on p.ProductID = t.ProductID and p.VariantID = t.VariantID
WHERE KitItemID = 0


UPDATE dbo.KitItem
SET PriceDelta = t.Price
FROM dbo.KitItem k
    join #tmp t  on k.KitItemID = t.KitItemID
WHERE t.KitItemID > 0



exec sp_xml_removedocument @hdoc

DROP TABLE #tmp
END
GO
PRINT N'Creating [dbo].[aspdnsf_CloneProduct]'
GO



create proc [dbo].[aspdnsf_CloneProduct]
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
    SELECT newid(), Name + ' - CLONED', Summary, Description, SEKeywords, SEDescription, SpecTitle, MiscText, SwatchImageMap, IsFeaturedTeaser, FroogleDescription, SETitle, SENoScript, SEAltText, SizeOptionPrompt, ColorOptionPrompt, TextOptionPrompt, ProductTypeID, TaxClassID, SKU, ManufacturerPartNumber, SalesPromptID, SpecCall, SpecsInline, IsFeatured, XmlPackage, ColWidth, Published, RequiresRegistration, 0, Notes, QuantityDiscountID, RelatedProducts, UpsellProducts, UpsellProductDiscountPercentage, RelatedDocuments, TrackInventoryBySizeAndColor, TrackInventoryBySize, TrackInventoryByColor, IsAKit, ShowInProductBrowser, IsAPack, PackSize, ShowBuyButton, RequiresProducts, HidePriceUntilCart, IsCalltoOrder, ExcludeFromPriceFeeds, RequiresTextOption, TextOptionMaxLength, SEName, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, getdate()
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
PRINT N'Creating [dbo].[aspdnsf_ExportProductList]'
GO


create proc [dbo].[aspdnsf_ExportProductList]
    @categoryID int = -1,
    @sectionID int = -1,
    @manufacturerID int = -1,
    @distributorID int = -1,
    @genreID int = -1,
    @vectorID int = -1
  
AS
BEGIN
    set nocount on 

    declare @productfilter table (productid int not null primary key)

    IF @categoryID+@sectionID+@manufacturerID+@distributorID+@genreID+@vectorID = -6

        SELECT p.ProductID, 
               pv.VariantID, 
               '' KitItemID,
               p.Name, 
               0 KitGroupID,
               '' KitGroup,
               isnull(p.SKU, '') SKU,
               isnull(p.ManufacturerPartNumber, '') ManufacturerPartNumber, 
               isnull(pv.SKUSuffix,'') SKUSuffix, 
               convert(varchar(20), isnull(convert(decimal(10,2), pv.Cost), 0)) Cost, 
               convert(varchar(20), isnull(convert(decimal(10,2), pv.MSRP), 0)) MSRP,
               convert(varchar(20), isnull(convert(decimal(10,2), pv.Price), 0)) Price, 
               convert(varchar(20), isnull(convert(decimal(10,2), pv.SalePrice), 0)) SalePrice, 
               convert(varchar(20), isnull(case p.TrackInventoryBySizeAndColor when 1 then i.quan else pv.Inventory end, 0)) Inventory
        FROM dbo.product p with (nolock) 
            join dbo.productvariant pv with (nolock) on pv.ProductID = p.ProductID
            left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i on i.VariantID = pv.VariantID
        WHERE p.deleted = 0 and pv.deleted = 0 
        UNION ALL
        SELECT p.ProductID, 
               pv.VariantID, 
               convert(varchar(10), KitItemID) KitItemID,
               k.Name, 
               kg.KitGroupID,
               kg.name,
               '',
               '', 
               '', 
               '', 
               '',
               convert(varchar(20), k.PriceDelta), 
               '', 
               ''
        FROM dbo.product p with (nolock) 
            join dbo.productvariant pv with (nolock) on pv.ProductID = p.ProductID
            join dbo.KitGroup kg with (nolock) on p.ProductID = kg.ProductID 
            join dbo.KitItem k with (nolock) on k.KitGroupID = kg.KitGroupID
        WHERE p.deleted = 0 and pv.deleted = 0 
        ORDER BY p.ProductID, pv.VariantID, KitGroupID

    ELSE BEGIN
        insert @productfilter 
        SELECT distinct p.ProductID FROM dbo.product p with (nolock) join [dbo].productcategory pc with (nolock) on p.ProductID = pc.ProductID WHERE pc.categoryID = @categoryID
        UNION
        SELECT distinct p.ProductID FROM dbo.product p with (nolock) join [dbo].productsection ps with (nolock) on p.ProductID = ps.ProductID WHERE ps.sectionid = @sectionID
        UNION
        SELECT distinct p.ProductID FROM dbo.product p with (nolock) join [dbo].productManufacturer pm with (nolock) on p.ProductID = pm.ProductID WHERE pm.manufacturerid = @manufacturerID
        UNION
        SELECT distinct p.ProductID FROM dbo.product p with (nolock) join [dbo].productDistributor pd with (nolock) on p.ProductID = pd.ProductID WHERE pd.Distributorid = @DistributorID
        UNION
        SELECT distinct p.ProductID FROM dbo.product p with (nolock) join [dbo].productGenre px with (nolock) on p.ProductID = px.ProductID WHERE px.Genreid = @GenreID
        UNION
        SELECT distinct p.ProductID FROM dbo.product p with (nolock) join [dbo].productvector px2 with (nolock) on p.ProductID = px2.ProductID WHERE px2.Vectorid = @VectorID

        SELECT p.ProductID, 
               pv.VariantID, 
               '' KitItemID,
               p.Name, 
               0 KitGroupID,
               '' KitGroup,
               isnull(p.SKU, '') SKU,
               isnull(p.ManufacturerPartNumber, '') ManufacturerPartNumber, 
               isnull(pv.SKUSuffix,'') SKUSuffix, 
               convert(varchar(20), isnull(convert(decimal(10,2), pv.Cost), 0)) Cost, 
               convert(varchar(20), isnull(convert(decimal(10,2), pv.MSRP), 0)) MSRP,
               convert(varchar(20), isnull(convert(decimal(10,2), pv.Price), 0)) Price,
               convert(varchar(20), isnull(convert(decimal(10,2), pv.SalePrice), 0)) SalePrice, 
               convert(varchar(20), isnull(case p.TrackInventoryBySizeAndColor when 1 then i.quan else pv.Inventory end, 0)) Inventory
        FROM dbo.product p with (nolock) 
            join dbo.productvariant pv with (nolock) on pv.ProductID = p.ProductID
            join @productfilter pf on p.ProductID = pf.ProductID
            left join (select VariantID, sum(quan) quan from dbo.Inventory with (nolock) group by VariantID) i on i.VariantID = pv.VariantID
        WHERE p.deleted = 0 and pv.deleted = 0 
        UNION ALL
        SELECT p.ProductID, 
               pv.VariantID, 
               convert(varchar(10), KitItemID) KitItemID,
               k.Name, 
               kg.KitGroupID,
               kg.name,
               '',
               '', 
               '', 
               '', 
               '',
               convert(varchar(20), k.PriceDelta), 
               '', 
               ''
        FROM dbo.product p with (nolock) 
            join dbo.productvariant pv with (nolock) on pv.ProductID = p.ProductID
            join @productfilter pf on p.ProductID = pf.ProductID
            join dbo.KitGroup kg with (nolock) on p.ProductID = kg.ProductID 
            join dbo.KitItem k with (nolock) on k.KitGroupID = kg.KitGroupID
        WHERE p.deleted = 0 and pv.deleted = 0 
        ORDER BY p.ProductID, pv.VariantID, KitGroupID

    END
END

GO
PRINT N'Creating [dbo].[aspdnsf_insProductType]'
GO


create proc [dbo].[aspdnsf_insProductType]
    @Name nvarchar(400),
    @ProductTypeID int OUTPUT
  
AS
SET NOCOUNT ON
 
if exists (select * FROM dbo.ProductType with (nolock) where [Name] = @Name) begin
 select @ProductTypeID=ProductTypeID FROM dbo.ProductType with (nolock) where [Name] = @Name
end
else begin
    insert dbo.ProductType(ProductTypeGUID, Name) values (newid(),@Name)
    set @ProductTypeID = @@identity
end

GO

PRINT N'Creating [dbo].[aspdnsf_CreateMissingVariants]'
GO


create proc [dbo].[aspdnsf_CreateMissingVariants] 
  
AS 
BEGIN
SET NOCOUNT ON
INSERT [dbo].ProductVariant (VariantGUID, IsDefault, Name, ProductID, Price, SalePrice, Inventory, 
                           DisplayOrder, IsTaxable, IsShipSeparately, IsDownload, FreeShipping, 
                           Published, Wholesale, IsSecureAttachment, IsRecurring, RecurringInterval, 
                           RecurringIntervalType, SEName, IsImport, Deleted, CreatedOn, 
                           SubscriptionIntervalType, CustomerEntersPrice) 
SELECT newid(), 1, '', p.ProductID, 0, 0, 100000000, 
       1, 0, 0, 0, 0, 
       1, 0, 0, 0, 0, 
       0, '', 0, 0, getdate(), 
       0, 0
FROM dbo.Product p with (nolock) 
    left join [dbo].ProductVariant pv with (nolock) on p.ProductID = pv.ProductID WHERE pv.ProductID is null
END


 

GO

PRINT N'Creating [dbo].[aspdnsf_ResetAllProductVariantDefaults]'
GO


create proc [dbo].[aspdnsf_ResetAllProductVariantDefaults] 
  
as
BEGIN
    SET NOCOUNT ON 

    update dbo.ProductVariant set IsDefault=0
    update dbo.ProductVariant 
    set IsDefault=1 
    from dbo.ProductVariant pv 
        join ( select distinct p.ProductID,pv.VariantID 
               from Product p
               join ProductVariant pv on p.ProductID=pv.ProductID
               where pv.VariantID in (SELECT top 1 VariantID from ProductVariant where ProductID=p.ProductID and ProductVariant.Deleted=0 order by DisplayOrder,Name)
               and p.Deleted=0
             ) a on pv.VariantID = a.VariantID

END

GO
PRINT N'Creating [dbo].[aspdnsf_GetProductWithVariants]'
GO


create procedure [dbo].[aspdnsf_GetProductWithVariants](
	@AlphaFilter nvarchar(30) = null,
	@SearchFilter nvarchar(30) = null,
	@pageSize int = null,
	@page int = null
)
as
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
		select	p.ProductId, 
				p.[Name], 
				p.Description, 
				p.Published, 
				p.Deleted 
		from Product p with (nolock)
		where	(@AlphaFilter IS NULL OR (p.[Name] like @AlphaFilter + '%')) and 
				(@SearchFilter IS NULL OR (p.[Name] like '%' + @SearchFilter + '%'))
	) p

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
	select	p.ProductId, 
			p.[Name], 
			p.Description, 
			p.Published, 
			p.Deleted 
	from
	(
		select	ROW_NUMBER() over(order by p.ProductId) as [Row_Number], 
				p.ProductId, 
				p.[Name], 
				p.Description, 
				p.Published, 
				p.Deleted 
		from Product p with (nolock)
		where	(@AlphaFilter IS NULL OR (p.[Name] like @AlphaFilter + '%')) and 
				(@SearchFilter IS NULL OR (p.[Name] like '%' + @SearchFilter + '%'))
	) p
	where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)

	-- next result set would be the variants that are tied to the previous products result set	
	select	pv.VariantId, 
			pv.ProductId, 
			pv.[Name], 
			pv.Description, 
			pv.Published, 
			pv.Deleted, 
			pv.IsDefault, 
			pv.Inventory, 
			pv.Price, 
			pv.SalePrice, 
			pv.Weight
	from ProductVariant pv with (nolock)
	inner join 	
	(
		select	p.ProductId
		from
		(
			select	ROW_NUMBER() over(order by p.ProductId) as [Row_Number], 
					p.ProductId, 
					p.[Name], 
					p.Description, 
					p.Published, 
					p.Deleted 
			from Product p with (nolock)
			where	(@AlphaFilter IS NULL OR (p.[Name] like @AlphaFilter + '%')) and 
					(@SearchFilter IS NULL OR (p.[Name] like '%' + @SearchFilter + '%'))
		) p
		where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)
	) p on p.ProductId = pv.ProductId	

end

GO


PRINT N'Creating [dbo].[aspdnsf_CloneVariant]'
GO

CREATE proc [dbo].[aspdnsf_CloneVariant]
    @VariantID int,
    @userid int = 0 
  
AS
BEGIN
    DECLARE @newvariantid int
    SET @newvariantid = 0

    INSERT [dbo].productvariant (VariantGUID, ProductID, IsDefault, Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, SubscriptionInterval, SubscriptionIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, CreatedOn)
    SELECT newid(), ProductID, 0, '(Cloned) ' + Name, Description, SEKeywords, SEDescription, Colors, ColorSKUModifiers, Sizes, SizeSKUModifiers, FroogleDescription, SKUSuffix, ManufacturerPartNumber, Price, SalePrice, Weight, MSRP, Cost, Points, Dimensions, Inventory, DisplayOrder, Notes, IsTaxable, IsShipSeparately, IsDownload, DownloadLocation, FreeShipping, Published, IsSecureAttachment, IsRecurring, RecurringInterval, RecurringIntervalType, SubscriptionInterval, SubscriptionIntervalType, RewardPoints, SEName, RestrictedQuantities, MinimumQuantity, ExtensionData, ExtensionData2, ExtensionData3, ExtensionData4, ExtensionData5, ContentsBGColor, PageBGColor, GraphicsColor, ImageFilenameOverride, IsImport, Deleted, getdate()
    FROM dbo.productvariant
    WHERE VariantID = @VariantID
    
    SELECT @newvariantid = @@IDENTITY 

    IF @@error <> 0 BEGIN
        raiserror('Variant not cloned', 1, 16)
        SELECT 0 VariantID
        RETURN
    END
    ELSE BEGIN
        INSERT [dbo].ExtendedPrice (ExtendedPriceGUID, VariantID, CustomerLevelID, Price, ExtensionData, CreatedOn)
        SELECT newid(), @newvariantid, CustomerLevelID, Price, ExtensionData, getdate()
        FROM dbo.ExtendedPrice
        WHERE VariantID = @VariantID

        INSERT [dbo].Inventory (InventoryGUID, VariantID, Color, Size, Quan, CreatedOn)
        SELECT newid(), @newvariantid, Color, Size, Quan, @userid
        FROM dbo.Inventory
        WHERE VariantID = @VariantID

        SELECT @newvariantid VariantID
    END
END

GO
PRINT N'Creating [dbo].[aspdnsf_CreateDefaultVariant]'
GO


create proc [dbo].[aspdnsf_CreateDefaultVariant] 
AS
SET NOCOUNT ON


INSERT [dbo].ProductVariant (VariantGUID, IsDefault, Name, ProductID, Price, SalePrice, Inventory, 
                       DisplayOrder, IsTaxable, IsShipSeparately, IsDownload, FreeShipping, 
                       Published, Wholesale, IsSecureAttachment, IsRecurring, RecurringInterval, 
                       RecurringIntervalType, SEName, IsImport, Deleted, CreatedOn, 
                       SubscriptionIntervalType, CustomerEntersPrice)
SELECT newid(), 1, '', p.ProductID, 0, 0, 100000000, 
       1, 0, 0, 0, 0, 
       1, 0, 0, 0, 0, 
       0, '', 0, 0, getdate(), 
       0, 0
FROM dbo.Product p with (nolock) 
    left join dbo.ProductVariant pv with (nolock) on p.ProductID = pv.ProductID
WHERE pv.ProductID is null

GO
PRINT N'Creating [dbo].[aspdnsf_CreateGiftCard]'
GO


CREATE proc [dbo].[aspdnsf_CreateGiftCard]
    @SerialNumber nvarchar(200),
    @PurchasedByCustomerID int,
    @OrderNumber int = null,
    @ShoppingCartRecID int,
    @ProductID int = null,
    @VariantID int = null,
    @InitialAmount money = null,
    @Balance money = null,
    @ExpirationDate datetime = null,
    @GiftCardTypeID int,
    @EMailName nvarchar(100) = null,
    @EMailTo nvarchar(100) = null,
    @EMailMessage ntext = null,
    @ValidForCustomers ntext = null,
    @ValidForProducts ntext = null,
    @ValidForManufacturers ntext = null,
    @ValidForCategories ntext = null,
    @ValidForSections ntext = null,
    @ExtensionData ntext = null,
    @GiftCardID int OUTPUT
  
AS
SET NOCOUNT ON 

IF @ExpirationDate is null
    set @ExpirationDate = dateadd(yy, 1, getdate())

insert dbo.GiftCard(GiftCardGUID, SerialNumber, PurchasedByCustomerID, OrderNumber, ShoppingCartRecID, ProductID, VariantID, InitialAmount, Balance, ExpirationDate, GiftCardTypeID, EMailName, EMailTo, EMailMessage, ValidForCustomers, ValidForProducts, ValidForManufacturers, ValidForCategories, ValidForSections, DisabledByAdministrator, ExtensionData, CreatedOn)
values 
(
    newid(), 
    @SerialNumber,
    @PurchasedByCustomerID, 
    isnull(@OrderNumber, 0),
    isnull(@ShoppingCartRecID, 0),
    isnull(@ProductID, 0),
    isnull(@VariantID, 0),
    isnull(@InitialAmount,0), 
    isnull(@Balance, 0),
    @ExpirationDate,
    @GiftCardTypeID,
    @EMailName, 
    @EMailTo, 
    @EMailMessage, 
    @ValidForCustomers, 
    @ValidForProducts, 
    @ValidForManufacturers, 
    @ValidForCategories, 
    @ValidForSections, 
    0, 
    @ExtensionData, 
    getdate()
)

set @GiftCardID = @@identity


GO
PRINT N'Creating [dbo].[aspdnsf_updGiftCard]'
GO


create proc [dbo].[aspdnsf_updGiftCard]
    @GiftCardID int,
    @SerialNumber nvarchar(100),
    @OrderNumber int,
    @InitialAmount money,
    @Balance money,
    @ExpirationDate datetime,
    @EMailName nvarchar(100),
    @EMailTo nvarchar(100),
    @EMailMessage ntext,
    @ValidForCustomers ntext,
    @ValidForProducts ntext,
    @ValidForManufacturers ntext,
    @ValidForCategories ntext,
    @ValidForSections ntext,
    @DisabledByAdministrator tinyint,
    @ExtensionData ntext
  
AS
SET NOCOUNT ON 


UPDATE dbo.GiftCard
SET 
    SerialNumber = COALESCE(@SerialNumber, SerialNumber),
    OrderNumber = COALESCE(@OrderNumber, OrderNumber),
    InitialAmount = COALESCE(@InitialAmount, InitialAmount),
    Balance = COALESCE(@Balance, Balance),
    ExpirationDate = COALESCE(@ExpirationDate, ExpirationDate),
    EMailName = COALESCE(@EMailName, EMailName),
    EMailTo = COALESCE(@EMailTo, EMailTo),
    EMailMessage = COALESCE(@EMailMessage, EMailMessage),
    ValidForCustomers = COALESCE(@ValidForCustomers, ValidForCustomers),
    ValidForProducts = COALESCE(@ValidForProducts, ValidForProducts),
    ValidForManufacturers = COALESCE(@ValidForManufacturers, ValidForManufacturers),
    ValidForCategories = COALESCE(@ValidForCategories, ValidForCategories),
    ValidForSections = COALESCE(@ValidForSections, ValidForSections),
    DisabledByAdministrator = COALESCE(@DisabledByAdministrator, DisabledByAdministrator),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData)
WHERE GiftCardID = @GiftCardID

GO
PRINT N'Creating [dbo].[aspdnsf_insGiftCardUsage]'
GO


create proc [dbo].[aspdnsf_insGiftCardUsage]
    @GiftCardID int,
    @UsageTypeID int,
    @UsedByCustomerID int,
    @OrderNumber int,
    @Amount money,
    @ExtensionData ntext = null,
    @GiftCardUsageID int OUTPUT
  
AS
BEGIN
    SET NOCOUNT ON 
    DECLARE @err int, @TotalUsage money, @Balance money
    
    select @Balance = Balance from dbo.GiftCard with (nolock) WHERE GiftCardID = @GiftCardID
    IF @UsageTypeID in (2, 4) and @Balance < @Amount BEGIN
        SET @Amount = @Balance 
    END

    BEGIN TRAN
        insert dbo.GiftCardUsage(GiftCardUsageGUID, GiftCardID, UsageTypeID, UsedByCustomerID, OrderNumber, Amount, ExtensionData, CreatedOn)
        values (newid(), @GiftCardID, @UsageTypeID, @UsedByCustomerID, @OrderNumber, @Amount, @ExtensionData, getdate())

        SELECT  @GiftCardUsageID = @@identity, @err = @@ERROR
        IF @err <> 0 BEGIN
            SET @GiftCardUsageID = -2
            RAISERROR('Could not enter gift card usage transaction', 16, 1)
            ROLLBACK TRAN
            RETURN
        END 

        SELECT @TotalUsage = sum(Amount*(case when UsageTypeID in (2, 4) then -1 else 1 end)) FROM dbo.GiftCardUsage with (nolock) WHERE GiftCardID = @GiftCardID
        UPDATE dbo.GiftCard
        SET Balance = InitialAmount + @TotalUsage 
        WHERE GiftCardID = @GiftCardID

        IF @err <> 0 BEGIN
            SET @GiftCardUsageID = -3
            RAISERROR('Could not update gift card balance', 16, 1)
            ROLLBACK TRAN
            RETURN
        END 

    COMMIT TRAN
END
GO
PRINT N'Creating [dbo].[aspdnsf_updGiftCardUsage]'
GO


create proc [dbo].[aspdnsf_updGiftCardUsage]
    @GiftCardUsageID int,
    @UsageTypeID int,
    @UsedByCustomerID int,
    @OrderNumber int,
    @Amount money,
    @ExtensionData ntext
  
AS
SET NOCOUNT ON 


UPDATE dbo.GiftCardUsage
SET 
    UsageTypeID = COALESCE(@UsageTypeID, UsageTypeID),
    UsedByCustomerID = COALESCE(@UsedByCustomerID, UsedByCustomerID),
    OrderNumber = COALESCE(@OrderNumber, OrderNumber),
    Amount = COALESCE(@Amount, Amount),
    ExtensionData = COALESCE(@ExtensionData, ExtensionData)
WHERE GiftCardUsageID = @GiftCardUsageID



GO
PRINT N'Creating [dbo].[aspdnsf_getGiftCardUsage]'
GO


create proc [dbo].[aspdnsf_getGiftCardUsage]
    @GiftCardUsageID int
  
AS
SET NOCOUNT ON 


SELECT GiftCardUsageID, GiftCardUsageGUID, GiftCardID, UsageTypeID, UsedByCustomerID, OrderNumber, Amount, ExtensionData, CreatedOn
FROM dbo.GiftCardUsage with (nolock) 
WHERE GiftCardUsageID = COALESCE(@GiftCardUsageID, GiftCardUsageID)


GO
PRINT N'Creating [dbo].[aspdnsf_getGiftCardUsageByGiftCard]'
GO


create proc [dbo].[aspdnsf_getGiftCardUsageByGiftCard]
    @GiftCardID int
  
AS
SET NOCOUNT ON 


SELECT GiftCardUsageID, GiftCardUsageGUID, GiftCardID, UsageTypeID, UsedByCustomerID, OrderNumber, Amount, ExtensionData, CreatedOn
FROM dbo.GiftCardUsage with (nolock) 
WHERE GiftCardID = @GiftCardID
GO
PRINT N'Creating [dbo].[aspdnsf_GetFeaturedProducts]'
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
	where pc.CategoryID=@FeaturedCategoryID and p.Deleted=0
	order by newid()

END
GO
PRINT N'Creating [dbo].[aspdnsf_WorldShipExport]'
GO


CREATE proc dbo.aspdnsf_WorldShipExport   
          
AS      
SET NOCOUNT ON
BEGIN

    SELECT CAST(o.OrderNumber AS varchar(10)) + '-' + CAST(a.ShippingAddressID AS varchar(10)) OrderNumber,    
         o.ShippingTrackingNumber, o.ShippedOn, o.ShippingMethod, o.CustomerID, o.FirstName + ' ' + o.LastName [Name],    
         o.Email, ad.FirstName + ' ' + ad.LastName AS ShippingName, o.ShippingCompany, ad.Address1 ShippingAddress1,
         ad.Address2 ShippingAddress2, ad.Suite ShippingSuite, ad.City ShippingCity, ad.State ShippingState, ad.Zip ShippingZip, ad.Country ShippingCountry,     
         ad.Phone ShippingPhone, b.AddressSubTotal OrderSubtotal,    
         o.OrderTax, o.OrderShippingCosts, o.OrderTotal, o.OrderDate, CASE WHEN (c.AddressCount = 1) THEN o.OrderWeight ELSE b.AddressWeightTotal END OrderWeight
    FROM dbo.Orders o    with (nolock) 
     JOIN (SELECT OrderNumber, ShippingAddressID FROM dbo.orders_shoppingcart with (nolock) GROUP BY OrderNumber, ShippingAddressID HAVING COUNT(DISTINCT ShippingAddressID) = 1 ) a ON O.OrderNumber = A.OrderNumber    
     JOIN (SELECT OrderNumber, ShippingAddressID, SUM(OrderedProductPrice * Quantity) AddressSubTotal,   SUM(PV.Weight * Quantity) AddressWeightTotal FROM dbo.orders_shoppingcart os with (nolock) JOIN productvariant pv with (nolock) on os.variantid = pv.variantid group by ordernumber, shippingaddressid )  b on b.ordernumber = a.ordernumber and b.ShippingAddressID = a.ShippingAddressID    
     JOIN (SELECT OrderNumber, count(ShippingAddressID) AddressCount FROM dbo.orders_shoppingcart with (nolock) group by ordernumber ) c on c.ordernumber = a.ordernumber
     JOIN dbo.Address ad on ad.addressid = b.shippingaddressid    

    WHERE o.ReadyToShip = 1 AND o.ShippedOn IS NULL AND TransactionState IN ('AUTHORIZED', 'CAPTURED')
    ORDER BY ordernumber

END

GO
PRINT N'Creating [dbo].[ShippingMethodStoreSummaryView]'
GO


create view [dbo].[ShippingMethodStoreSummaryView]
AS
SELECT	sm.ShippingMethodID, 
        SUM(CASE WHEN sms.StoreId IS NULL THEN 0 ELSE POWER(2, sms.StoreId) END) AS StoreBitMap
FROM ShippingMethod sm WITH (NOLOCK) 
LEFT OUTER JOIN ShippingMethodStore sms WITH (NOLOCK) ON sm.ShippingMethodID = sms.ShippingMethodID 
GROUP BY sm.ShippingMethodID
GO
PRINT N'Creating [dbo].[aspdnsf_StoreVersion]'
GO


create proc [dbo].[aspdnsf_StoreVersion] 
as
BEGIN
    select configvalue from dbo.appconfig with (nolock) where name='StoreVersion'
end
GO
PRINT N'Creating [dbo].[aspdnsf_CloneStoreMappings]'
GO


CREATE PROC [dbo].[aspdnsf_CloneStoreMappings]
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
PRINT N'Creating [dbo].[aspdnsf_NukeStoreMappings]'
GO


CREATE PROC [dbo].[aspdnsf_NukeStoreMappings]
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
PRINT N'Creating [dbo].[aspdnsf_NukeStoreOrder]'
GO


create procedure [dbo].[aspdnsf_NukeStoreOrder]
	@StoreID int
as
begin
	set nocount on;

	delete occ
	from Orders_CustomCart occ
	inner join Orders o on o.OrderNumber = occ.OrderNumber
	where o.StoreId = @StoreId
	
	delete okc
	from Orders_KitCart okc
	inner join Orders o on o.OrderNumber = okc.OrderNumber
	where o.StoreId = @StoreId
	
	delete osc
	from Orders_ShoppingCart osc
	inner join Orders o on o.OrderNumber = osc.OrderNumber
	where o.StoreId = @StoreId
	
	delete o
	from Orders o
	where o.StoreId = @StoreId
	
end

GO

PRINT N'Creating [dbo].[aspdnsf_MakeStoreDefault]'
GO


create procedure [dbo].[aspdnsf_MakeStoreDefault]
	@StoreID int
as
begin
	set nocount on;

	declare @prefDefID int
	select @prefDefID = StoreID from Store where IsDefault = 1
	
	update Store set IsDefault = Case when(StoreId = @StoreId) then 1 else 0 end

	-- synchronize appconfig and string resources
	update AppConfig set StoreID = @StoreID where StoreID = @prefDefID
	update StringResource set StoreID = @StoreID where StoreID = @prefDefID

end
GO
PRINT N'Creating [dbo].[aspdnsf_SaveMap]'
GO


create procedure [dbo].[aspdnsf_SaveMap]
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
			else IF @EntityType='Polls'
		begin
			DELETE FROM PollStore WHERE PollID = @EntityID AND StoreID = @StoreID
		end	
	end

END
GO
PRINT N'Creating [dbo].[aspdnsf_Map]'
GO


CREATE PROC [dbo].[aspdnsf_Map]
	@CreateMap BIT,
	@RemoveMap BIT,
	@StoreID INT,
	@EntityID INT,
	@EntityType VARCHAR(100)

AS
IF @EntityType IN ('Topic', 'News', 'Product', 'OrderOption')
BEGIN
		IF @CreateMap = 1
		BEGIN
			IF @EntityType = 'Topic'
			IF NOT EXISTS(SELECT * FROM TopicStore WHERE @StoreID = StoreID AND TopicID = @EntityID)
				INSERT INTO TopicStore (StoreID, TopicID)
				VALUES (@StoreID, @EntityID)
			IF @EntityType = 'News'
			IF NOT EXISTS (SELECT * FROM NewsStore WHERE StoreID = @StoreID AND NewsID = @EntityID)
				INSERT INTO NewsStore(StoreID, NewsID)
				VALUES (@StoreID, @EntityID)
			IF @EntityType='Product'
			IF NOT EXISTS (SELECT * FROM ProductStore WHERE StoreID = @StoreID AND ProductID = @EntityID)
				INSERT INTO ProductStore(StoreID, ProductID)
				VALUES (@StoreID, @EntityID)
			IF @EntityType='OrderOption'
			IF NOT EXISTS (SELECT * FROM OrderOptionStore WHERE StoreID = @StoreID AND OrderOptionID = @EntityID)
				INSERT INTO OrderOptionStore(StoreID, OrderOptionID)
				VALUES (@StoreID, @EntityID)
		END	
		IF @RemoveMap = 1
		BEGIN
			IF @EntityType = 'Topic'
				DELETE FROM TopicStore WHERE TopicID = @EntityID AND StoreID = @StoreID
			IF @EntityType = 'News'
				DELETE FROM NewsStore WHERE NewsID = @EntityID AND StoreID = @StoreID
			IF @EntityType='Product'
				DELETE FROM ProductStore WHERE ProductID = @EntityID AND StoreID = @StoreID
			IF @EntityType='OrderOption'
				DELETE FROM OrderOptionStore WHERE OrderOptionID = @EntityID AND StoreID = @StoreID
		END
END
ELSE
	BEGIN
		IF @CreateMap = 1
		BEGIN
		IF NOT EXISTS (SELECT * FROM EntityStore WHERE StoreID = @StoreID AND EntityID = @EntityID  AND EntityType = @EntityType)
			INSERT INTO EntityStore (StoreID, EntityID, EntityType) 
				VALUES (@StoreID, @EntityID, @EntityType)
				
		END
		IF @RemoveMap = 1
		BEGIN
			DELETE FROM EntityStore
				WHERE StoreID = @StoreID AND EntityID = @EntityID  AND EntityType = @EntityType
	END
END

GO
PRINT N'Creating [dbo].[aspdnsf_insAppconfig]'
GO



create proc dbo.aspdnsf_insAppconfig
    @Name nvarchar(100),
    @Description ntext,
    @ConfigValue nvarchar(1000),
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
PRINT N'Creating [dbo].[aspdnsf_getAppconfig]'
GO



create proc dbo.aspdnsf_getAppconfig
    @AppConfigID int = null
  
AS
SET NOCOUNT ON 

    SELECT StoreId, AppConfigID, AppConfigGUID, [Name], Description, ConfigValue, ValueType, AllowableValues, GroupName, SuperOnly, CreatedOn
    FROM dbo.Appconfig with (nolock) 
    WHERE AppConfigID = COALESCE(@AppConfigID, AppConfigID)
    ORDER BY [Name]

GO
PRINT N'Creating [dbo].[aspdnsf_SecurityLogInsert]'
GO


create proc [dbo].[aspdnsf_SecurityLogInsert]
    @SecurityAction nvarchar(100),
    @Description ntext,
    @CustomerUpdated int,
    @UpdatedBy int,
    @CustomerSessionID int,
    @logid bigint OUTPUT
  
AS
SET NOCOUNT ON 


insert dbo.SecurityLog(SecurityAction, Description, ActionDate, CustomerUpdated, UpdatedBy, CustomerSessionID)
values (@SecurityAction, @Description, getdate(), @CustomerUpdated, @UpdatedBy, @CustomerSessionID)

set @logid = @@identity
 

GO

PRINT N'Creating [dbo].[aspdnsf_PABPEraseCCInfo]'
GO



create proc [dbo].[aspdnsf_PABPEraseCCInfo]
    @CartType int
  
AS
SET NOCOUNT ON 
update dbo.orders set CardNumber='1111111111111111' where CardNumber is not null
update dbo.orders set CardNumber=NULL where CardNumber is not null

update dbo.address set CardNumber='1111111111111111' 
from dbo.address a left join (select distinct CustomerID from dbo.shoppingcart where CartType=@CartType) b on a.CustomerID = b.CustomerID
where CardNumber is not null and b.CustomerID is null

update dbo.address set CardNumber=NULL 
from dbo.address a left join (select distinct CustomerID from dbo.shoppingcart where CartType=@CartType) b on a.CustomerID = b.CustomerID
where CardNumber is not null and b.CustomerID is null
GO
PRINT N'Creating [dbo].[aspdnsf_getEventHandler]'
GO


create proc [dbo].[aspdnsf_getEventHandler]
    @EventID int = null
  
AS
SET NOCOUNT ON 

    SELECT EventID, [EventName], CalloutURL, XMLPackage, Active, Debug
    FROM dbo.EventHandler with (nolock) 
    WHERE EventID = COALESCE(@EventID, EventID)
    ORDER BY [EventName]

GO


PRINT N'Creating [dbo].[LayoutFieldAttribute]'
GO
CREATE TABLE [dbo].[LayoutFieldAttribute]
(
[LayoutFieldAttributeID] [int] NOT NULL IDENTITY(1, 1),
[LayoutFieldAttributeGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF_LayoutFieldAttribute_LayoutFieldAttributeGUID] DEFAULT (newid()),
[LayoutID] [int] NOT NULL,
[LayoutFieldID] [int] NOT NULL,
[Name] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Value] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)

GO


PRINT N'Creating primary key [PK_LayoutFieldAttribute_LayoutFieldAttributeID] on [dbo].[LayoutFieldAttribute]'
GO
ALTER TABLE [dbo].[LayoutFieldAttribute] ADD CONSTRAINT [PK_LayoutFieldAttribute_LayoutFieldAttributeID] PRIMARY KEY CLUSTERED  ([LayoutFieldAttributeID])

GO


PRINT N'Creating [dbo].[aspdnsf_updLayoutFieldAttribute]'
GO


CREATE proc [dbo].[aspdnsf_updLayoutFieldAttribute]
    @LayoutID int,
    @LayoutFieldID int,
    @Name nvarchar(100),
    @Value nvarchar(max)
  
AS
SET NOCOUNT ON 

if not exists(select * from dbo.LayoutFieldAttribute where LayoutFieldID=@LayoutFieldID and Name=@Name)
begin
	INSERT dbo.LayoutFieldAttribute(LayoutFieldAttributeGUID, LayoutID, LayoutFieldID, Name, Value)
	VALUES (NEWID(), @LayoutID, @LayoutFieldID, @Name, @Value)
end
else
begin
    UPDATE dbo.LayoutFieldAttribute
    SET 
        Value = COALESCE(@Value, Value)
		WHERE LayoutFieldID=@LayoutFieldID and Name=@Name
end

GO


PRINT N'Creating [dbo].[aspdnsf_updEventHandler]'
GO


create proc [dbo].[aspdnsf_updEventHandler]
    @EventID int,
    @EventName nvarchar(20) = null,
    @CalloutURL varchar(200) = null,
    @XmlPackage varchar(100) = null,
    @Active bit = null,
    @Debug bit =null
  
AS
SET NOCOUNT ON 


    UPDATE dbo.EventHandler
    SET 
        EventName = COALESCE(@EventName, EventName),
        CalloutURL = COALESCE(@CalloutURL, CalloutURL),
        XmlPackage = COALESCE(@XmlPackage, XmlPackage),
        Active = COALESCE(@Active, Active),
        Debug = COALESCE(@Debug, Debug)
    WHERE EventID = @EventID
GO
PRINT N'Creating [dbo].[aspdnsf_updLayout]'
GO

CREATE proc [dbo].[aspdnsf_updLayout]
    @LayoutID int,
    @Description nvarchar(max) = null,
    @HTML nvarchar(max) = null,
    @Version int = 1,
    @Micro nvarchar(100) = null,
    @Icon nvarchar(100) = null,
    @Medium nvarchar(100) = null,
    @Large nvarchar(100) = null,
    @UpdatedOn datetime
  
AS
SET NOCOUNT ON 

    UPDATE dbo.Layout
    SET 
        Description = COALESCE(@Description, Description),
        HTML = COALESCE(@HTML, HTML),
        Version =  COALESCE(@Version, Version),
        Micro = COALESCE(@Micro, Micro),
        Icon = COALESCE(@Icon, Icon),
        Medium = COALESCE(@Medium, Medium),
        Large = COALESCE(@Large, Large),
		@UpdatedOn =  COALESCE(@UpdatedOn, getdate())
    WHERE LayoutID = @LayoutID

GO


PRINT N'Creating [dbo].[aspdnsf_insEventHandler]'
GO


create proc [dbo].[aspdnsf_insEventHandler]
    @EventName nvarchar(20),
    @CalloutURL varchar(200),
    @XmlPackage varchar(100),
    @Active bit,
    @Debug bit,
    @EventID int OUTPUT
  
AS
SET NOCOUNT ON 


    if exists (select * from dbo.EventHandler with (nolock) where EventName = @EventName)
        set @EventID = -1
    else begin
        INSERT dbo.EventHandler(EventName, CalloutURL, XmlPackage, Active, Debug)
        VALUES (@EventName, @CalloutURL, @XmlPackage, @Active, @Debug)
        set @EventID = @@identity
    end 

GO

PRINT N'Creating [dbo].[aspdnsf_insLayoutFieldAttribute]'
GO

CREATE proc [dbo].[aspdnsf_insLayoutFieldAttribute]
	@LayoutID int,
	@LayoutFieldID int,
    @Name nvarchar(100),
    @Value nvarchar(250)
  
AS
SET NOCOUNT ON 

if not exists(select * from dbo.LayoutFieldAttribute where LayoutFieldID=@LayoutFieldID and Name=@Name)
begin
INSERT dbo.LayoutFieldAttribute (LayoutID, LayoutFieldID, Name, Value)
VALUES (@LayoutID, @LayoutFieldID, @Name, @Value)
end
else
begin
	UPDATE dbo.LayoutFieldAttribute SET Value=@Value WHERE LayoutFieldID=@LayoutFieldID and Name=@Name
end

GO


PRINT N'Altering [dbo].[ShippingByTotal]'
GO
ALTER TABLE [dbo].[ShippingByTotal] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingByTotal_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[ShippingByTotalByPercent]'
GO
ALTER TABLE [dbo].[ShippingByTotalByPercent] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingByTotalByPercent_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[ShippingByWeight]'
GO
ALTER TABLE [dbo].[ShippingByWeight] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingByWeight_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[ShippingMethodToCountryMap]'
GO
ALTER TABLE [dbo].[ShippingMethodToCountryMap] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingMethodToCountryMap_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[ShippingMethodToStateMap]'
GO
ALTER TABLE [dbo].[ShippingMethodToStateMap] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingMethodToStateMap_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[ShippingMethodToZoneMap]'
GO
ALTER TABLE [dbo].[ShippingMethodToZoneMap] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingMethodToZoneMap_StoreID] DEFAULT ((1))

GO


PRINT N'Altering [dbo].[ShippingTotalByZone]'
GO
ALTER TABLE [dbo].[ShippingTotalByZone] ADD
[StoreID] [int] NOT NULL CONSTRAINT [DF_ShippingTotalByZone_StoreID] DEFAULT ((1))

GO


PRINT N'Creating [dbo].[aspdnsf_insLayoutMap]'
GO

create proc [dbo].[aspdnsf_insLayoutMap]
	@layoutid int,				-- the id of the layout
	@pageid int,				-- the entity or object id if the page is a topic, entity, or product
	@pagetypename nvarchar(100)	-- the page name if the page is not a topic, entity, or product
  
AS
BEGIN
    SET NOCOUNT ON
    
    declare @pagetypeid int	-- the pagetypeid from dbo.PageType
	select @pagetypeid = PageTypeID from dbo.PageType where PageTypeName=@pagetypename
	
	IF(@pagetypeid > 0) -- the page type already exists, insert the mapping
		INSERT dbo.LayoutMap (LayoutMapGUID, LayoutID, PageTypeID, PageID, CreatedOn)
		values (
			NEWID(),
			@layoutid,
			@pagetypeid,
			@pageid,
			GETDATE())
	ELSE BEGIN -- the page type does not exist so add the page type then insert the mapping
		INSERT dbo.PageType (PageTypeGUID, PageTypeName)
		values (
			NEWID(),
			@pagetypename )
			
		select @pagetypeid = PageTypeID from dbo.PageType where PageTypeName=@pagetypename
		
		INSERT dbo.LayoutMap (LayoutMapGUID, LayoutID, PageTypeID, PageID, CreatedOn)
		values (
			NEWID(),
			@layoutid,
			@pagetypeid,
			@pageid,
			GETDATE())
	END	
    
END

GO


PRINT N'Creating [dbo].[aspdnsf_delTaxClass]'
GO


create proc [dbo].[aspdnsf_delTaxClass]
    @TaxClassID int
  
AS
SET NOCOUNT ON 

BEGIN TRAN

    DELETE dbo.StateTaxRate where TaxClassID = @TaxClassID
    IF @@ERROR <> 0 BEGIN
        ROLLBACK TRAN
        RAISERROR('Deleting TaxClass from StateTaxRate failed', 16, 1)
        RETURN
    END


    DELETE dbo.CountryTaxRate where TaxClassID = @TaxClassID
    IF @@ERROR <> 0 BEGIN
        ROLLBACK TRAN
        RAISERROR('Deleting TaxClass from CountryTaxRate failed', 16, 1)
        RETURN
    END


    DELETE dbo.ZipTaxRate where TaxClassID = @TaxClassID
    IF @@ERROR <> 0 BEGIN
        ROLLBACK TRAN
        RAISERROR('Deleting TaxClass from ZipTaxRate failed', 16, 1)
        RETURN
    END

    
    DELETE dbo.TaxClass where TaxClassID = @TaxClassID
    IF @@ERROR <> 0 BEGIN
        ROLLBACK TRAN
        RAISERROR('Deleting TaxClass failed', 16, 1)
        RETURN
    END

    
COMMIT TRAN
GO
PRINT N'Creating [dbo].[aspdnsf_getCountryTaxRate]'
GO


create proc [dbo].[aspdnsf_getCountryTaxRate]
    @CountryID int = null,
    @TaxClassID int = null
  
AS
SET NOCOUNT ON 


SELECT ctr.CountryTaxID, ctr.CountryID, ctr.TaxClassID, ctr.TaxRate, ctr.CreatedOn, t.Name TaxClass, c.Name Country
FROM dbo.CountryTaxRate ctr with (nolock) join dbo.TaxClass t with (nolock) on ctr.TaxClassID = t.TaxClassID join dbo.Country c on c.CountryID = ctr.CountryID
WHERE ctr.CountryID = COALESCE(@CountryID, ctr.CountryID) and ctr.TaxClassID = COALESCE(@TaxClassID, ctr.TaxClassID)
GO
PRINT N'Creating [dbo].[aspdnsf_insAffiliate]'
GO



CREATE PROC dbo.aspdnsf_insAffiliate
    @EMail nvarchar(100),
    @Password nvarchar(250),
    @DateOfBirth datetime,
    @Gender nvarchar(1),
    @Notes text,
    @IsOnline tinyint,
    @FirstName nvarchar(100),
    @LastName nvarchar(100),
    @Name nvarchar(100),
    @Company nvarchar(100),
    @Address1 nvarchar(100),
    @Address2 nvarchar(100),
    @Suite nvarchar(50),
    @City nvarchar(100),
    @State nvarchar(100),
    @Zip nvarchar(10),
    @Country nvarchar(100),
    @Phone nvarchar(25),
    @WebSiteName nvarchar(100),
    @WebSiteDescription ntext,
    @URL ntext,
    @TrackingOnly tinyint,
    @DefaultSkinID int,
    @ParentAffiliateID int,
    @DisplayOrder int,
    @ExtensionData ntext,
    @SEName nvarchar(100),
    @SETitle ntext,
    @SENoScript ntext,
    @SEAltText ntext,
    @SEKeywords ntext,
    @SEDescription ntext,
    @Wholesale tinyint,
    @SaltKey int,
    @StoreID int,
    @AffiliateID int OUTPUT
  
AS
SET NOCOUNT ON 


insert dbo.Affiliate(AffiliateGUID, EMail, Password, DateOfBirth, Gender, Notes, IsOnline, FirstName, LastName, [Name], Company, Address1, Address2, Suite, City, State, Zip, Country, Phone, WebSiteName, WebSiteDescription, URL, TrackingOnly, DefaultSkinID, ParentAffiliateID, DisplayOrder, ExtensionData, SEName, SETitle, SENoScript, SEAltText, SEKeywords, SEDescription, Published, Wholesale, CreatedOn, SaltKey)
values (newid(), @EMail, @Password, @DateOfBirth, @Gender, @Notes, @IsOnline, @FirstName, @LastName, @Name, @Company, @Address1, @Address2, @Suite, @City, @State, @Zip, @Country, @Phone, @WebSiteName, @WebSiteDescription, @URL, @TrackingOnly, @DefaultSkinID, @ParentAffiliateID, @DisplayOrder, @ExtensionData, @SEName, @SETitle, @SENoScript, @SEAltText, @SEKeywords, @SEDescription, 1, @Wholesale, getdate(), @SaltKey)

set @AffiliateID = @@identity

insert into AffiliateStore (AffiliateID, StoreID, CreatedOn) values (@AffiliateID, @StoreID, GETDATE())

GO
PRINT N'Creating [dbo].[aspdnsf_getStateTaxRate]'
GO


create proc [dbo].[aspdnsf_getStateTaxRate]
    @StateID int = null,
    @TaxClassID int = null
  
AS
SET NOCOUNT ON 


SELECT sr.StateTaxID, sr.StateID, sr.TaxClassID, sr.TaxRate, sr.CreatedOn, t.Name TaxClass, s.Name StateName
FROM dbo.StateTaxRate sr with (nolock) join dbo.TaxClass t with (nolock) on sr.TaxClassID = t.TaxClassID join dbo.State s on s.StateID = sr.StateID
WHERE sr.StateID = COALESCE(@StateID, sr.StateID) and sr.TaxClassID = COALESCE(@TaxClassID, sr.TaxClassID)
GO
PRINT N'Creating [dbo].[aspdnsf_GetNews]'
GO



CREATE PROCEDURE [dbo].[aspdnsf_GetNews] (
	@NumHomeNewsToDisplay	INT,
	@storeid				INT,
	@filternews				BIT
)
  
AS
BEGIN
	SET NOCOUNT ON

	select TOP(@NumHomeNewsToDisplay) a.* 
	from News a with (NOLOCK) inner join (select distinct a.NewsID from News a with (nolock) left join NewsStore b with (NOLOCK) on a.NewsID = b.NewsID where (@filternews = 0 or StoreID = @storeid)) b on a.NewsID = b.NewsID 
	where ExpiresOn > getdate() and Deleted = 0 and Published = 1 
	order by a.NewsId desc 
END
GO
PRINT N'Creating [dbo].[aspdnsf_DeleteAddress]'
GO


CREATE proc [dbo].[aspdnsf_DeleteAddress]
    @AddressID int,
    @CustomerID int
  
AS
BEGIN
    SET NOCOUNT ON 

    DECLARE @addrID int

    BEGIN TRAN
        DELETE dbo.ADDRESS WHERE AddressID = @AddressID and CustomerID = @CustomerID
        IF @@ERROR <> 0 BEGIN
            ROLLBACK TRAN
            RETURN
        END

        SELECT TOP 1 @addrID = AddressID FROM dbo.Address with (nolock) WHERE CustomerID = @CustomerID

        update [dbo].Customer SET ShippingAddressID = @addrID WHERE ShippingAddressID = @AddressID and CustomerID = @CustomerID
        IF @@ERROR <> 0 BEGIN
            ROLLBACK TRAN
            RETURN
        END

        update [dbo].Customer SET BillingAddressID = @addrID WHERE BillingAddressID = @AddressID and CustomerID = @CustomerID

        IF @@ERROR <> 0 BEGIN
            ROLLBACK TRAN
            RETURN
        END

    COMMIT TRAN

END

GO

PRINT N'Refreshing [dbo].[EntityMaster]'
GO
EXEC sp_refreshview N'[dbo].[EntityMaster]'

GO


PRINT N'Refreshing [dbo].[ProductEntity]'
GO
EXEC sp_refreshview N'[dbo].[ProductEntity]'

GO


PRINT N'Creating [dbo].[aspdnsf_CustomerConsistencyCheck]'
GO


create proc dbo.aspdnsf_CustomerConsistencyCheck
  
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
    delete from dbo.PollVotingRecord where CustomerID not in (select distinct CustomerID from Customer);
    delete from dbo.Address where CustomerID not in (select distinct CustomerID from Customer);
    delete from dbo.Rating where CustomerID not in (select distinct CustomerID from Customer);
END

GO


PRINT N'Creating [dbo].[aspdnsf_GetCustomerByID]'
GO


create proc [dbo].[aspdnsf_GetCustomerByID]
    @CustomerID int
  
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
              WHERE c.CustomerID = @CustomerID and s.LoggedOut is null and s.LastActivity >= dateadd(mi, -@intSessionTimeOut, getdate())) a on cs.CustomerSessionID = a.CustomerSessionID

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
            @CustomerSessionID CustomerSessionID,
            @LastActivity LastActivity, c.StoreID, d.Name StoreName
    FROM dbo.Customer c with (nolock) left join dbo.CustomerLevel cl with (nolock) on c.CustomerLevelID = cl.CustomerLevelID
    left join Store d with (nolock) on c.StoreID = d.StoreID
    WHERE c.Deleted=0 and c.CustomerID = @CustomerID
END
GO
PRINT N'Creating [dbo].[ObjectView]'
GO


CREATE VIEW [dbo].[ObjectView ]
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
GO
PRINT N'Creating [dbo].[aspdnsf_GetEntityTree]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_GetEntityTree]
	@entity nvarchar(30),
	@entityid int,
	@storeid int,
	@filterentity bit	
 	
AS
BEGIN
SET NOCOUNT ON

	SELECT	DISTINCT pe.EntityID as ID, 
			pe.Name as [Name], 
			pe.ParentEntityID as ParentID
	FROM EntityMaster pe WITH (NOLOCK)
	INNER JOIN EntityMaster te WITH (NOLOCK) on te.EntityID = @entityid and te.EntityType = @entity and pe.EntityType = @entity
	left join (select EntityID from EntityStore with (nolock) WHERE (0 = 0 or StoreID = 1)) et on pe.EntityID = et.EntityID
	inner join (select distinct a.EntityID from EntityMaster a with (nolock) left join EntityStore b with (nolock) on a.EntityID = b.EntityID WHERE (@filterentity = 0 or StoreID = @storeid)) et1 on pe.EntityID = et1.EntityID
	WHERE pe.EntityID = te.ParentEntityID 

	SELECT	DISTINCT se.EntityID as ID, 
			se.Name as [Name], 
			se.ParentEntityID as ParentID, se.DisplayOrder
	FROM EntityMaster se WITH (NOLOCK)
	inner join EntityMaster te WITH (NOLOCK) on te.EntityID = @entityid and te.EntityType = @entity and se.EntityType = @entity
	inner join (select distinct a.EntityID from EntityMaster a with (nolock) left join EntityStore b with (nolock) on a.EntityID = b.EntityID WHERE (@filterentity = 0 or StoreID = @storeid)) stre on se.EntityID = stre.EntityID
	WHERE se.ParentEntityID = te.ParentEntityID
	order by se.DisplayOrder, se.name asc
END
GO
PRINT N'Creating [dbo].[aspdnsf_GetSimpleObjectEntityList]'
GO


create proc [dbo].[aspdnsf_GetSimpleObjectEntityList]
    @entityname      varchar(100),
    @entityid        int = null,
    @affiliateid     int = null,
    @customerlevelid int = null,
    @AllowKits       tinyint = 1,
    @AllowPacks      tinyint = 1, 
    @PublishedOnly   tinyint = 0,
    @OrderByLooks    tinyint = 0
  
AS
SET NOCOUNT ON

DECLARE @FilterProductsByCustomerLevel tinyint, @CustomerLevelFilteringIsAscending bit

SELECT @FilterProductsByCustomerLevel = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel'

SET @CustomerLevelFilteringIsAscending  = 0
SELECT @CustomerLevelFilteringIsAscending  = case configvalue when 'true' then 1 else 0 end
FROM dbo.appConfig
WHERE name like 'FilterByCustomerLevelIsAscending'


IF @entityname not in ('category', 'section', 'manufacturer', 'library', 'distributor', 'genre', 'vector')
    RETURN

IF @entityname = 'library' BEGIN
    SELECT d.*, d.documentid objectid, pe.EntityID
    FROM dbo.[Document] d with (nolock) 
        left join [dbo].ProductEntity pe with (nolock) on d.documentid = pe.ProductID and pe.EntityType = @entityname and pe.EntityID = coalesce(@entityid, pe.EntityID)
        left join [dbo].DocumentAffiliate a with (nolock) on d.documentid = a.DocumentID
        left join [dbo].DocumentCustomerLevel dcl with (nolock) on d.documentid = dcl.DocumentID
    WHERE pe.EntityID = coalesce(@entityid, pe.EntityID)
        and (a.AffiliateID = coalesce(@affiliateid, a.AffiliateID) or @affiliateid is null or @affiliateid = 0)
        and (dcl.CustomerLevelID = coalesce(@customerlevelid, dcl.CustomerLevelID) or @customerlevelid is null)
        and case 
                when @FilterProductsByCustomerLevel = 0 then 1
                when @CustomerLevelFilteringIsAscending = 1 and (dcl.CustomerLevelID <= @CustomerLevelID or dcl.CustomerLevelID is null) then 1 
                when @CustomerLevelID=0 and dcl.CustomerLevelID is null then 1
                when dcl.CustomerLevelID = @CustomerLevelID  or dcl.CustomerLevelID is null then 1 
                else 0
            end  = 1
        and d.Published >= @PublishedOnly
        and d.deleted = 0
END
ELSE BEGIN

    SELECT p.*, p.productid ObjectID, pe.EntityID
    FROM dbo.Product p with (nolock) 
        join [dbo].ProductEntity pe with (nolock) on p.ProductID = pe.ProductID and pe.EntityType = @entityname and pe.EntityID = coalesce(@entityid, pe.EntityID)
        join (
                SELECT distinct p.productid 
                FROM dbo.Product p with (nolock) 
                     join [dbo].ProductEntity pe with (nolock) on p.ProductID = pe.ProductID and pe.EntityType = @entityname
                     left join (select distinct ProductID, EntityID from [dbo].ProductEntity with (nolock) where EntityType = 'affiliate') a on p.ProductID = a.ProductID
                     left join (select distinct ProductID, EntityID from [dbo].ProductEntity with (nolock) where EntityType = 'customerlevel') pcl on p.ProductID = pcl.ProductID 
                WHERE pe.EntityID = coalesce(@entityid, pe.EntityID)
                    and (a.EntityID = coalesce(@affiliateid, a.EntityID) or @affiliateid is null or @affiliateid = 0)
                    and case 
                            when @FilterProductsByCustomerLevel = 0 then 1
                            when @CustomerLevelFilteringIsAscending = 1 and (pcl.EntityID <= @CustomerLevelID or pcl.EntityID is null) then 1 
                            when @CustomerLevelID=0 and pcl.EntityID is null then 1
                            when pcl.EntityID = @CustomerLevelID  or pcl.EntityID is null then 1 
                            else 0
                        end  = 1
                    and p.IsAKit <= @AllowKits
                    and p.IsAPack <= @AllowPacks
                    and p.IsSystem = 0
                    and p.Published >= @PublishedOnly
                    and p.deleted = 0
            ) filter on p.productid = filter.productid
    ORDER BY pe.DisplayOrder
END
GO
PRINT N'Creating [dbo].[aspdnsf_ProductSequence]'
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
    @affiliateID     int = null
  
AS
BEGIN 
    SET NOCOUNT ON 

    
    DECLARE @id int, @row int
    DECLARE @affiliatecount int
    CREATE TABLE #sequence (row int identity not null, productid int not null)

    DECLARE @FilterProductsByAffiliate tinyint, @FilterProductsByCustomerLevel tinyint, @HideProductsWithLessThanThisInventoryLevel int, @CustomerLevelFilteringIsAscending bit

    SELECT @FilterProductsByCustomerLevel = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByCustomerLevel'
    SELECT @FilterProductsByAffiliate = case ConfigValue when 'true' then 1 else 0 end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'FilterProductsByAffiliate'
	SELECT @HideProductsWithLessThanThisInventoryLevel = case ConfigValue when 0 then 0 else ConfigValue end FROM dbo.AppConfig with (nolock) WHERE [Name] = 'HideProductsWithLessThanThisInventoryLevel'

    SET @CustomerLevelFilteringIsAscending  = 0
    SELECT @CustomerLevelFilteringIsAscending  = case configvalue when 'true' then 1 else 0 end FROM dbo.appConfig with (nolock) WHERE name like 'FilterByCustomerLevelIsAscending'

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
          and case p.TrackInventoryBySizeAndColor when 1 then isnull(i.quan, 0) else pv.inventory end >= @HideProductsWithLessThanThisInventoryLevel
		  and (getdate() between isnull(p.AvailableStartDate, '1/1/1900') and isnull(p.AvailableStopDate, '1/1/2999'))     
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
PRINT N'Creating [dbo].[StoreMappingView]'
GO


CREATE VIEW [dbo].[StoreMappingView]
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
PRINT N'Creating [dbo].[UniquelyMapped]'
GO


create function dbo.UniquelyMapped(
	@StoreId int,
	@EntityType nvarchar(30),
	@EntityId int = null
)
returns bit
begin

	declare @isUnique int
	set @isUnique = 0 -- unknown yet or default value if non found

	declare @c int
	select @c = count(*)
	FROM StoreMappingView smv 
	WHERE smv.EntityType = @EntityType and smv.EntityId = @EntityId

	if(@c > 0)
	begin
		if(@c > 1)
		begin
			set @isUnique = 0 -- more than 1 store is mapped to this entity
		end
		else
		begin
			declare @s int
	
			select @s = smv.StoreId 
			FROM StoreMappingView smv 
			WHERE smv.EntityType = @EntityType and smv.EntityId = @EntityId

			if(@StoreId = @s) 
			begin
				set @isUnique = 1 -- only 1 match
			end	
		end
	end	

	return @isUnique

end
GO
PRINT N'Creating [dbo].[GetStoreMap]'
GO


create function dbo.GetStoreMap(
	@StoreId int,
	@EntityType nvarchar(30),
	@EntityId int = null
)
RETURNS BIT
BEGIN
	DECLARE @IsMapped BIT

	IF EXISTS(SELECT * FROM StoreMappingView WHERE StoreId = @StoreId AND EntityType = @EntityType AND EntityId = ISNULL(@EntityId, EntityId))
	BEGIN
		SET @IsMapped = 1
	END
	ELSE
	BEGIN
		SET @IsMapped = 0
	END

	RETURN @IsMapped
END
GO
PRINT N'Creating [dbo].[aspdnsf_NukeProduct]'
GO


create proc [dbo].[aspdnsf_NukeProduct]
    @ProductID int
  
AS
SET NOCOUNT ON 
BEGIN


BEGIN TRAN
    DELETE dbo.ProductCategory WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductCategory could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.CustomCart WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('CustomCart could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ProductDistributor WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductDistributor could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ProductGenre WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductGenre could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.KitCart WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('KitCart could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.KitGroup WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('KitGroup could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ProductManufacturer WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductManufacturer could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ProductAffiliate WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductAffiliate could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ProductCategory WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductCategory could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ProductCustomerLevel WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductCustomerLevel could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ProductLocaleSetting WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductLocaleSetting could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ProductSection WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductSection could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.Rating WHERE productid = @productid 
    IF @@ERROR <> 0 BEGIN
        raiserror('Rating could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.RatingCommentHelpfulness WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('RatingCommentHelpfulness could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    DELETE dbo.ShoppingCart WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ShoppingCart could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.ExtendedPrice FROM dbo.ExtendedPrice with (nolock) join [dbo].productvariant pv with (nolock) on ExtendedPrice.variantid = pv.variantid where pv.productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ExtendedPrice could not be deleted', 1, 16)
        rollback tran
        return
    end
    
    DELETE dbo.Inventory FROM dbo.Inventory with (nolock) join [dbo].productvariant pv with (nolock) on Inventory.variantid = pv.variantid where pv.productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('Inventory could not be deleted', 1, 16)
        rollback tran
        return
    end

    DELETE dbo.ProductVariant WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('ProductVariant could not be deleted', 1, 16)
        rollback tran
        return
    END

    DELETE dbo.Product WHERE productid = @productid
    IF @@ERROR <> 0 BEGIN
        raiserror('Product could not be deleted', 1, 16)
        rollback tran
        return
    END
    
    
COMMIT TRAN

END
GO
PRINT N'Creating [dbo].[aspdnsf_GetMappedObjects]'
GO


create procedure [dbo].[aspdnsf_GetMappedObjects](
	@StoreId int,
	@EntityType nvarchar(30),
	@AlphaFilter nvarchar(30) = null,
	@SearchFilter nvarchar(30) = null,
	@pageSize int = null,
	@page int = null
)
as
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

GO

PRINT N'Creating [dbo].[aspdnsf_GetCustomersAlsoBoughtProducts]'
GO



CREATE PROC [dbo].[aspdnsf_GetCustomersAlsoBoughtProducts]  
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
				INNER JOIN (SELECT DISTINCT ProductID FROM ProductStore with (nolock) WHERE (@filterProduct = 0 or StoreID = @storeID)) ps on p.ProductID = ps.ProductID   
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
            GROUP BY os.productid   
            ORDER BY count(*) desc, sum(os.Quantity) desc  
        ) a on p.productid = a.productid  
GO
PRINT N'Creating [dbo].[aspdnsf_GetObjects]'
GO


create procedure [dbo].[aspdnsf_GetObjects](	
	@EntityType nvarchar(30),
	@AlphaFilter nvarchar(30) = null,
	@SearchFilter nvarchar(30) = null,
	@pageSize int = null,
	@page int = null
)
as
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
	select	ROW_NUMBER,
			ov.EntityType, 			
			ov.[ID],
			ov.[Name]			
	from
	(
		select	ROW_NUMBER() over(partition by o.EntityType order by id) as [Row_Number], 
				o.EntityType, 			
				o.[Id],
				o.[Name],
				o.Description
		from ObjectView o
		where	o.EntityType = COALESCE(@EntityType, o.EntityType) and 
				(@AlphaFilter IS NULL OR (o.[Name] like @AlphaFilter + '%')) and 
				(@SearchFilter IS NULL OR (o.[Name] like '%' + @SearchFilter + '%'))
	) ov
	where @doPage = 0 or (ROW_NUMBER BETWEEN @start AND @end)

end

GO

PRINT N'Creating [dbo].[aspdnsf_GetStoreShippingMethodMapping]'
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
PRINT N'Creating [dbo].[aspdnsf_NukeStoreAffiliate]'
GO


create procedure [dbo].[aspdnsf_NukeStoreAffiliate]
	@StoreID int
as
begin
	set nocount on;
	
	delete aar
	from AffiliateActivityReason aar
	inner join AffiliateActivity aa on aar.AffiliateActivityReasonID = aa.AffiliateActivityReasonID
	inner join Affiliate af on aa.AffiliateID = af.AffiliateID
	inner join AffiliateStore afs on afs.AffiliateID = af.AffiliateID
	where dbo.UniquelyMapped(@StoreID, 'Affiliate', af.AffiliateID) = 1 
	
	delete aa
	from AffiliateActivity aa
	inner join Affiliate af on aa.AffiliateID = af.AffiliateID
	inner join AffiliateStore afs on afs.AffiliateID = af.AffiliateID
	where dbo.UniquelyMapped(@StoreID, 'Affiliate', af.AffiliateID) = 1 
	
	delete pa
	from ProductAffiliate pa
	inner join Affiliate af on pa.AffiliateID = af.AffiliateID
	inner join AffiliateStore afs on afs.AffiliateID = af.AffiliateID
	where dbo.UniquelyMapped(@StoreID, 'Affiliate', af.AffiliateID) = 1 
	
	delete da
	from DocumentAffiliate da
	inner join Affiliate af on da.AffiliateID = af.AffiliateID
	inner join AffiliateStore afs on afs.AffiliateID = af.AffiliateID
	where dbo.UniquelyMapped(@StoreID, 'Affiliate', af.AffiliateID) = 1 
	
	delete af
	from Affiliate af
	inner join AffiliateStore afs on afs.AffiliateID = af.AffiliateID
	where dbo.UniquelyMapped(@StoreID, 'Affiliate', af.AffiliateID) = 1 

end

GO

PRINT N'Creating [dbo].[aspdnsf_GetMappedObject]'
GO


create procedure [dbo].[aspdnsf_GetMappedObject](
	@StoreId int,
	@EntityType nvarchar(30),
	@EntityID int)
as
begin

	set nocount on;

	select	ov.EntityType, 			
			ov.[ID],
			ov.[Name],
			dbo.GetStoreMap(@StoreId, ov.EntityType, ov.ID) as Mapped
	from ObjectView ov
	where	ov.EntityType = @EntityType and 
			ov.[ID] = @EntityID		

end
GO
PRINT N'Creating [dbo].[MappedObjects]'
GO


CREATE VIEW [dbo].[MappedObjects]
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
SELECT OO.OrderOptionID AS ID, 'OrderOption' AS EntityType,0 AS ParentID, OO.OrderOptionGUID AS GUID, OO.[Name], PS.StoreID AS StoreID
FROM OrderOption AS OO LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
ON OO.OrderOptionID = PS.EntityID AND PS.EntityType = 'OrderOption'
UNION ALL 
SELECT GC.GiftCardID AS ID, 'GiftCard' AS EntityType,0 AS ParentID, GC.GiftCardGUID AS GUID, GC.SerialNumber AS [Name], PS.StoreID AS StoreID
FROM GiftCard AS GC LEFT JOIN StoreMappingView AS PS WITH (NOLOCK)
ON GC.GiftCardID = PS.EntityID AND PS.EntityType = 'GiftCard'

GO
PRINT N'Creating [dbo].[aspdnsf_NukeStore]'
GO


create procedure [dbo].[aspdnsf_NukeStore]
	@StoreID int,
	@SummaryOnly bit = 0
as
begin
	/*
	 NOTE:
		this script doesn't protect against nuking a default store, 
		it should be prompted first before calling out this sproc.
		Only certain entities can be nuked:
			News, Topic, ShippingMethod, OrderOption, Affiliate, Customer and Orders
		but mapping for all entities are removed as well
	*/
	
	declare @MAPPED_TO_THIS_STORE int
	set @MAPPED_TO_THIS_STORE = 1

	/* summary table used for reporting */
	declare @NukeSummary Table
	(
		[Row] [int] IDENTITY(1,1) NOT NULL,
		[EntityType] nvarchar(100),
		[EntityID] int,
		[EntityName] nvarchar(500)
	)

	/* generic entities */
	insert into @NukeSummary(EntityType, EntityID, EntityName)
	select ov.EntityType, ov.[ID], ov.[Name]
	from ObjectView ov
	where	ov.EntityType in ('News', 'Topic', 'ShippingMethod', 'OrderOption', 'Affiliate') and 
			dbo.UniquelyMapped(@StoreID, ov.EntityType, ov.ID) = @MAPPED_TO_THIS_STORE

	if(@SummaryOnly = 0)
	begin				
		/* news */		
		delete n
		from News n
		inner join NewsStore ns on ns.NewsID = n.NewsID
		where dbo.UniquelyMapped(@StoreID, 'News', n.NewsID) = @MAPPED_TO_THIS_STORE

		/* Topic */		
		delete t
		from Topic t
		inner join TopicStore ts on ts.TopicID = t.TopicID
		where dbo.UniquelyMapped(@StoreID, 'Topic', t.TopicID) = @MAPPED_TO_THIS_STORE

		/* Order Option */		
		delete os
		from OrderOption os
		inner join OrderOptionStore oos on oos.OrderOptionID = os.OrderOptionID
		where dbo.UniquelyMapped(1, 'OrderOption', os.OrderOptionID) = @MAPPED_TO_THIS_STORE

		/* Shipping Method */		
		delete sm
		from ShippingMethod sm
		inner join ShippingMethodStore sms on sms.ShippingMethodID = sm.ShippingMethodID
		where dbo.UniquelyMapped(@StoreID, 'ShippingMethod', sm.ShippingMethodID) = @MAPPED_TO_THIS_STORE

		/* next delete ALL mappings */
		exec aspdnsf_NukeStoreMappings @StoreID

		/* nuking these entities requires some relationships to be handled first, hence encapsulated into sproc */
		exec [aspdnsf_NukeStoreAffiliate] @StoreID
		exec [aspdnsf_NukeStoreCustomer] @StoreID
		exec [aspdnsf_NukeStoreOrder] @StoreID

		delete s
		from Store s
		where s.StoreID = @StoreID
	end
	

	select * from @NukeSummary

end
GO
PRINT N'Creating [dbo].[aspdnsf_UndoImport]'
GO


create procEDURE [dbo].[aspdnsf_UndoImport]
  
as
BEGIN
    SET NOCOUNT ON 
    delete from [dbo].customcart where shoppingcartrecid in (select shoppingcartrecid from shoppingcart where productid in (select productid from product where IsImport=1 and IsSystem=0));
    delete from [dbo].kitcart where shoppingcartrecid in (select shoppingcartrecid from shoppingcart where productid in (select productid from product where IsImport=1 and IsSystem=0));
    delete from [dbo].shoppingcart where productid in (select productid from product where IsImport=1 and IsSystem=0);
    delete from [dbo].ProductCategory where productid in (select productid from product where IsImport=1 and IsSystem=0);
    delete from [dbo].ProductSection where productid in (select productid from product where IsImport=1 and IsSystem=0);
    delete from [dbo].ProductAffiliate where productid in (select productid from product where IsImport=1 and IsSystem=0);
    delete from [dbo].ProductCustomerLevel where productid in (select productid from product where IsImport=1 and IsSystem=0);
    delete from [dbo].ProductManufacturer where productid in (select productid from product where IsImport=1 and IsSystem=0);
    delete from [dbo].productvariant where productid in (select productid from product where IsImport=1 and IsSystem=0) or IsImport=1;
    delete from [dbo].kititem where kitgroupid in (select kitgroupid from kitgroup where productid in (select productid from product where IsImport=1 and IsSystem=0));
    delete from [dbo].kitgroup where productid in (select productid from product where IsImport=1 and IsSystem=0);
    delete from [dbo].product where IsImport=1 and IsSystem=0;
    delete from [dbo].manufacturer where IsImport=1;
    delete from [dbo].category where IsImport=1;
    delete from [dbo].section where IsImport=1;
    delete from [dbo].library where IsImport=1;
    delete from [dbo].Document where IsImport=1;
    delete from [dbo].Manufacturer where IsImport=1;
END

GO
PRINT N'Creating [dbo].[aspdnsf_GetEntityEntries]'
GO



CREATE PROC [dbo].[aspdnsf_GetEntityEntries]
	@EntityType VARCHAR(30),
	@StoreID INT = NULL,
	@NameLike Varchar(30) = NULL
 
AS

SELECT ID, EntityType, ParentID, GUID, [Name], dbo.GetStoreMap(@StoreID, @EntityType, ID) AS Mapped
FROM MappedObjects WHERE EntityType = @EntityType AND ([Name] LIKE @NameLike OR @NameLike IS NULL)

GO
PRINT N'Creating [dbo].[NukeStore]'
GO


CREATE PROC [dbo].[NukeStore] 
	@StoreID INT,
	@NukeNews BIT = 0,
	@NukeAffiliates BIT =0,
	@NukeTopics BIT = 0,
	@NukeProducts BIT = 0,
	@NukeCoupons BIT = 0,
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
PRINT N'Creating [dbo].[aspdnsf_getMailingList]'
GO



CREATE PROCEDURE [dbo].[aspdnsf_getMailingList]
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
			[LastName]
	FROM [NewsletterMailList]
	WHERE [SubscriptionConfirmed] = 1
		AND	([UnsubscribedOn] IS NULL  OR [UnsubscribedOn] > GETDATE()) 
END

END

GO


PRINT N'Creating [dbo].[aspdnsf_AdjustInventory]'
GO



create proc [dbo].[aspdnsf_AdjustInventory]
    @ordernumber int,
    @direction smallint -- 1 = add items to inventory, -1 = remove from inventory
  
AS
BEGIN
    SET NOCOUNT ON 

    IF @direction <> 1 and @direction <> -1 BEGIN
        RAISERROR('Invalid direction specified', 16, 1)
        RETURN
    END

    DECLARE @InventoryWasReduced int
    SELECT @InventoryWasReduced = InventoryWasReduced FROM dbo.orders with (nolock) WHERE ordernumber = @ordernumber

    IF (@direction = 1 and @InventoryWasReduced = 1) or (@direction = -1 and @InventoryWasReduced = 0) BEGIN

        BEGIN TRAN
            -- update [dbo].Non-Pack Products
            update dbo.Inventory
            SET Quan = Quan + (a.qty*@direction)
            FROM dbo.Inventory i 
                join (select o.variantid, 
                            case when o.ChosenColor is null then '' when charindex('[', o.ChosenColor)>0 then rtrim(left(o.ChosenColor, charindex('[', o.ChosenColor)-1)) else o.ChosenColor end ChosenColor, 
                            case when o.ChosenSize is null then '' when charindex('[', o.ChosenSize)>0 then rtrim(left(o.ChosenSize, charindex('[', o.ChosenSize)-1)) else o.ChosenSize end ChosenSize, 
                            sum(o.Quantity) qty 
                      from dbo.Orders_ShoppingCart o 
                          join dbo.product p on p.ProductID = o.ProductID 
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID 
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 1 
                      group by o.variantid, 
                            case when o.ChosenColor is null then '' when charindex('[', o.ChosenColor)>0 then rtrim(left(o.ChosenColor, charindex('[', o.ChosenColor)-1)) else o.ChosenColor end , 
                            case when o.ChosenSize is null then '' when charindex('[', o.ChosenSize)>0 then rtrim(left(o.ChosenSize, charindex('[', o.ChosenSize)-1)) else o.ChosenSize end 
                     ) a on i.variantid = a.variantid and isnull(i.size, '') = a.ChosenSize and isnull(i.Color, '') = a.ChosenColor

            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('Inventory update failed', 16, 1)
                RETURN
            END
            

            update dbo.ProductVariant
            SET Inventory = Inventory + (a.qty*@direction)
            FROM dbo.ProductVariant pv join [dbo].Product p on pv.productid = p.productid 
                join (select o.variantid, sum(o.Quantity) qty 
                      from dbo.Orders_ShoppingCart o 
                          join dbo.product p on p.ProductID = o.ProductID 
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID 
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 0
                      group by o.variantid
                     ) a on pv.variantid = a.variantid


            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('ProductVariant inventory update failed', 16, 1)
                RETURN
            END



            --update [dbo].Pack Products
            update dbo.Inventory
            SET Quan = Quan + (a.qty*@direction)
            FROM dbo.Inventory i
                join (select o.variantid, 
                            case when o.ChosenColor is null then '' when charindex('[', o.ChosenColor)>0 then rtrim(left(o.ChosenColor, charindex('[', o.ChosenColor)-1)) else o.ChosenColor end ChosenColor, 
                            case when o.ChosenSize is null then '' when charindex('[', o.ChosenSize)>0 then rtrim(left(o.ChosenSize, charindex('[', o.ChosenSize)-1)) else o.ChosenSize end ChosenSize, 
                            sum(o.Quantity) qty 
                      from dbo.Orders_CustomCart o 
                          join dbo.Orders_ShoppingCart sc on sc.ShoppingCartRecID = o.ShoppingCartRecID
                          join dbo.product p on p.ProductID = o.ProductID 
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID 
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 1 
                      group by o.variantid, 
                            case when o.ChosenColor is null then '' when charindex('[', o.ChosenColor)>0 then rtrim(left(o.ChosenColor, charindex('[', o.ChosenColor)-1)) else o.ChosenColor end , 
                            case when o.ChosenSize is null then '' when charindex('[', o.ChosenSize)>0 then rtrim(left(o.ChosenSize, charindex('[', o.ChosenSize)-1)) else o.ChosenSize end 
                     ) a on i.variantid = a.variantid and isnull(i.size, '') = a.ChosenSize and isnull(i.Color, '') = a.ChosenColor


            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('Inventory update failed', 16, 1)
                RETURN
            END


            update dbo.ProductVariant
            SET Inventory = Inventory + (a.qty*@direction)
            FROM dbo.ProductVariant pv join [dbo].Product p on pv.productid = p.productid 
                join (select o.variantid, sum(o.Quantity*sc.Quantity) qty 
             from dbo.Orders_CustomCart o 
                          join dbo.Orders_ShoppingCart sc on sc.ShoppingCartRecID = o.ShoppingCartRecID
                          join dbo.product p on p.ProductID = o.ProductID 
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID 
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 0
                      group by o.variantid
                     ) a on pv.variantid = a.variantid


            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('ProductVariant inventory update failed', 16, 1)
                RETURN
            END


            --Update Inventory of inventoryable kititems
            UPDATE dbo.Inventory
            SET Quan = Quan + (a.qty*@direction)
            FROM dbo.Inventory i
                join (select o.InventoryVariantID variantid, 
                            case when o.InventoryVariantColor is null then '' when charindex('[', o.InventoryVariantColor)>0 then rtrim(left(o.InventoryVariantColor, charindex('[', o.InventoryVariantColor)-1)) else o.InventoryVariantColor end ChosenColor, 
                            case when o.InventoryVariantSize is null then '' when charindex('[', o.InventoryVariantSize)>0 then rtrim(left(o.InventoryVariantSize, charindex('[', o.InventoryVariantSize)-1)) else o.InventoryVariantSize end ChosenSize, 
                            sum(o.Quantity) qty 
                      from dbo.Orders_KitCart o 
                          join dbo.Orders_ShoppingCart sc on sc.ShoppingCartRecID = o.ShoppingCartRecID
                          join dbo.product p on p.ProductID = o.ProductID 
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID 
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 1 
                      group by o.InventoryVariantID, 
                            case when o.InventoryVariantColor is null then '' when charindex('[', o.InventoryVariantColor)>0 then rtrim(left(o.InventoryVariantColor, charindex('[', o.InventoryVariantColor)-1)) else o.InventoryVariantColor end , 
                            case when o.InventoryVariantSize is null then '' when charindex('[', o.InventoryVariantSize)>0 then rtrim(left(o.InventoryVariantSize, charindex('[', o.InventoryVariantSize)-1)) else o.InventoryVariantSize end  
                     ) a on i.variantid = a.variantid and isnull(i.size, '') = a.ChosenSize and isnull(i.Color, '') = a.ChosenColor

            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('KitItem Inventory update failed', 16, 1)
                RETURN
            END


            update dbo.ProductVariant
            SET Inventory = Inventory + (a.qty*@direction)
            FROM dbo.ProductVariant pv join [dbo].Product p on pv.productid = p.productid 
                join (select o.InventoryVariantID variantid, sum(o.Quantity*sc.Quantity) qty 
                      from dbo.Orders_KitCart o 
                          join dbo.Orders_ShoppingCart sc on sc.ShoppingCartRecID = o.ShoppingCartRecID
                          join dbo.product p on p.ProductID = o.ProductID 
                          join dbo.productvariant pv on o.ProductID = pv.ProductID and o.VariantID = pv.VariantID 
                      where o.ordernumber = @ordernumber and p.TrackInventoryBySizeAndColor = 0
                      group by o.InventoryVariantID
                     ) a on pv.variantid = a.variantid


            IF @@ERROR <> 0 BEGIN
                ROLLBACK TRAN
                RAISERROR('KitItem ProductVariant inventory update failed', 16, 1)
                RETURN
            END

            UPDATE dbo.orders SET InventoryWasReduced = case @direction when 1 then 0 when -1 then 1 else InventoryWasReduced end WHERE ordernumber = @ordernumber

        COMMIT TRAN

    END

END
GO
PRINT N'Creating [dbo].[aspdnsf_getKitItems]'
GO



CREATE PROCEDURE [dbo].[aspdnsf_getKitItems] (
	@ProductID int,
    @CartRecID int,
	@CustomerID int)
  
AS
BEGIN
	SET NOCOUNT ON	
	SELECT
			p.[Name]		 AS ProductName,
			ki.KitItemID,
			kg.KitGroupID,
			kg.ProductID,
			ki.[Name]        AS ItemName,
			ki.[Description] AS ItemDescription,
			ki.PriceDelta    AS ItemPriceDelta,
			ki.WeightDelta   AS ItemWeightDelta, 
			ki.IsDefault,	
			ki.DisplayOrder,	
			ki.InventoryVariantID,
			ki.InventoryQuantityDelta,
			ki.InventoryVariantColor,
			ki.InventoryVariantSize,			
			CAST( (CASE WHEN ki.IsDefault = 1 THEN 1 ELSE 0 END ) AS BIT) AS IsSelected,
			CASE	WHEN   kc.KitItemID IS NOT NULL THEN kc.TextOption       			
					ELSE ''
			END AS TextOption,
			kg.[Name]		 AS GroupName,
			kg.[Description] AS GroupDescription,
			kg.Summary AS GroupSummary,
			kg.DisplayOrder	 AS GroupDisplayOrder,
			kg.IsRequired,
			kg.IsReadOnly,
			kgt.KitGroupTypeID AS SelectionControl
    FROM KitItem      ki  with (nolock) 
    JOIN KitGroup     kg  with (nolock) ON kg.KitGroupID=ki.KitGroupID 
    JOIN KitGroupType kgt with (nolock) ON kgt.KitGroupTypeID=kg.KitGroupTypeID
	JOIN Product	  p   with (nolock) ON p.IsAKit = 1 AND kg.ProductId = p.ProductId
    LEFT JOIN KitCart kc  with (nolock) ON ( kc.KitItemID = ki.KitItemID AND kc.KitGroupID = ki.KitGroupID 
                                                  AND kc.ProductID = kg.ProductID  
                                                  AND kc.CustomerID = @CustomerID 
                                                  AND kc.ShoppingCartrecID = @CartRecID AND kc.ShoppingCartrecID <> 0)  
    WHERE	p.ProductID=@ProductID 	
    ORDER BY kg.DisplayOrder ASC, ki.DisplayOrder ASC, ki.[Name]

END
GO
PRINT N'Creating [dbo].[aspdnsf_GetShoppingCart]'
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
PRINT N'Creating [dbo].[aspdnsf_NukeStoreCustomer]'
GO


create procedure [dbo].[aspdnsf_NukeStoreCustomer]
	@StoreID int,
	@IncludeAdmins BIT = 0
as
begin
	set nocount on;

	delete cu	
	from couponusage cu
	inner join Customer c on cu.CustomerID = c.CustomerID
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

PRINT N'Creating [dbo].[aspdnsf_AddItemToCart]'
GO


CREATE proc dbo.aspdnsf_AddItemToCart
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
    @StoreID int
    
  
AS
SET NOCOUNT ON


DECLARE @RestrictedQy varchar(8000), @CurrentCartQty int, @InventoryFullSKU nvarchar(50), @InventoryWeightDelta money, @AllowEmptySkuAddToCart varchar(10), @ShoppingCartrecid int, @IsAKit tinyint, @IsAPack tinyint
DECLARE @levelprice money, @LevelDiscountPercent money, @LevelDiscountsApplyToExtendedPrices tinyint, @CustEntersPrice tinyint

SELECT @CustEntersPrice = CustomerEntersPrice, @RestrictedQy = RestrictedQuantities FROM dbo.ProductVariant with (nolock) WHERE VariantID = @VariantID

SELECT	@IsAKit = IsAKit, @IsAPack = IsAPack FROM dbo.Product with (nolock) WHERE ProductID = @ProductID 


SELECT @CurrentCartQty = Quantity From dbo.shoppingcart with (nolock) where ShippingAddressID = @ShippingAddressID and ProductID = @ProductID and VariantID = @VariantID and ChosenColor = @ChosenColor and ChosenSize = @ChosenSize and TextOption like @TextOption and CustomerID = @CustomerID and GiftRegistryForCustomerID = @GiftRegistryForCustomerID and CartType = @CartType and StoreID = @StoreID

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
INSERT dbo.ShoppingCart(CartType,ShoppingCartRecGUID,CustomerID,ShippingAddressID,BillingAddressID,ProductID,SubscriptionInterval,SubscriptionIntervalType,VariantID,ProductSKU,ProductPrice,CustomerEntersPrice,ProductWeight,ProductDimensions,Quantity,RequiresCount,ChosenColor,ChosenColorSKUModifier,ChosenSize,ChosenSizeSKUModifier,TextOption,IsTaxable,IsShipSeparately,IsDownload,DownloadLocation,FreeShipping,DistributorID,RecurringInterval,RecurringIntervalType, IsSystem, IsAKit, IsAPack, TaxClassID, IsKit2, StoreID)
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
	@StoreID
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

PRINT N'Creating [dbo].[aspdnsf_MonthlyMaintenance]'
GO


CREATE proc [dbo].[aspdnsf_MonthlyMaintenance]
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
        delete dbo.kitcart where CartType=0 and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
        delete dbo.customcart where CartType=0 and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
        delete dbo.ShoppingCart where CartType=0 and CreatedOn < dateadd(d,-@CleanShoppingCartsOlderThan,getdate());
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
PRINT N'Creating [dbo].[aspdnsf_updOrderItemQuantityDiscount]'
GO


CREATE PROC [dbo].[aspdnsf_updOrderItemQuantityDiscount]
    @OrderNumber int   
     
  
AS
SET NOCOUNT ON

UPDATE dbo.orders_ShoppingCart
SET OrderedProductQuantityDiscountID = dbo.GetQtyDiscountID(sc.ProductID),
    OrderedProductQuantityDiscountPercent = COALESCE(NULLIF(dbo.GetQtyDiscount(sc.ProductID, a.ProductQuantity, 0), 0), NULLIF(dbo.GetQtyDiscount(sc.ProductID, a.ProductQuantity, 1), 0), 0)
FROM orders_ShoppingCart sc
    join (select ProductID, sum(Quantity) ProductQuantity from dbo.orders_ShoppingCart with (nolock) where OrderNumber = @OrderNumber group by ProductID) a on sc.ProductID = a.ProductID
WHERE OrderNumber = @OrderNumber 

UPDATE dbo.orders_ShoppingCart
SET OrderedProductQuantityDiscountName = qd.[name]
FROM dbo.orders_ShoppingCart sc join dbo.QuantityDiscount qd on sc.OrderedProductQuantityDiscountID = qd.QuantityDiscountID
WHERE OrderNumber = @OrderNumber 

GO
PRINT N'Creating [dbo].[MobileDevice]'
GO
CREATE TABLE [dbo].[MobileDevice]
(
[MobileDeviceID] [int] NOT NULL IDENTITY(1, 1),
[UserAgent] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Name] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)

GO


PRINT N'Creating primary key [PK_MobileDevice] on [dbo].[MobileDevice]'
GO
ALTER TABLE [dbo].[MobileDevice] ADD CONSTRAINT [PK_MobileDevice] PRIMARY KEY CLUSTERED  ([MobileDeviceID])

GO


PRINT N'Adding mobile devices to [dbo].[MobileDevice]'
GO
SET IDENTITY_INSERT [dbo].[MobileDevice] ON
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (2, N'acs-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (3, N'alav', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (4, N'alca', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (5, N'amoi', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (6, N'audi', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (7, N'avan', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (8, N'benq', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (9, N'bird', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (10, N'blac', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (11, N'blaz', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (12, N'brew', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (13, N'cell', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (14, N'cldc', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (15, N'cmd-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (16, N'dang', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (17, N'doco', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (18, N'eric', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (19, N'hipt', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (20, N'inno', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (21, N'ipaq', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (22, N'java', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (23, N'jigs', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (24, N'kddi', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (25, N'keji', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (26, N'leno', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (27, N'lg-c', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (28, N'lg-d', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (29, N'lge-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (30, N'maui', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (31, N'maxo', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (32, N'midp', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (33, N'mini', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (34, N'mits', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (35, N'mmef', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (36, N'mmp', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (37, N'mobi', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (38, N'mot-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (39, N'moto', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (40, N'mwbp', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (41, N'nec-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (42, N'newt', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (43, N'noki', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (45, N'palm', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (46, N'pana', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (47, N'pant', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (48, N'pda', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (49, N'phil', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (50, N'phone', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (51, N'play', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (52, N'port', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (53, N'prox', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (54, N'qwap', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (55, N'sage', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (56, N'sams', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (57, N'sany', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (58, N'sch-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (59, N'sec-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (60, N'send', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (61, N'seri', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (62, N'sgh-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (63, N'shar', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (64, N'sie-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (65, N'siem', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (66, N'smal', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (67, N'smar', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (68, N'smartphone', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (69, N'sony', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (70, N'sph-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (71, N'symb', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (72, N't-mo', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (73, N'teli', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (74, N'tim-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (76, N'tsm-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (77, N'up.browser', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (78, N'up.link', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (79, N'upg1', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (80, N'upsi', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (81, N'vk-v', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (82, N'voda', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (83, N'wap', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (84, N'wap-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (85, N'wapa', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (86, N'wapi', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (87, N'wapp', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (88, N'wapr', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (89, N'webc', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (90, N'windows ce', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (91, N'winw', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (92, N'xda', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (93, N'xda-', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (94, N'iphone', NULL)
INSERT [dbo].[MobileDevice] ([MobileDeviceID], [UserAgent], [Name]) VALUES (95, N'blackberry', NULL)
SET IDENTITY_INSERT [dbo].[MobileDevice] OFF
GO 

PRINT N'Creating [dbo].[aspdnsf_getProductVariants]'
GO




CREATE PROCEDURE [dbo].[aspdnsf_getProductVariants]
(
	@FilterEntityType int = 0,
	@FilterEntityID int = 0,
	@Deleted tinyint = 0
)
AS
BEGIN
SET NOCOUNT ON

-- This can return huge result sets potentially
-- Dynamic SQL actually performs better in this scenario than static Sql would

DECLARE @sql nvarchar(max)
DECLARE @whereClause nvarchar(max)
DECLARE @finalSql nvarchar(max)

SELECT @FilterEntityID = ISNULL(@FilterEntityID, 0)
SELECT @FilterEntityType = ISNULL(@FilterEntityType, 0)

SET @sql = 'SELECT	[pv].[VariantID],
					[pv].[Name],
					[pv].[Description],
					[pv].[Price],
					[pv].[SalePrice],
					[pv].[SKUSuffix],
					[pv].[Published],
					[pv].[Inventory],
					[p].[TrackInventoryBySizeAndColor],
					[pv].[ProductID],
					[p].[Name] as ProductName
			FROM [ProductVariant] pv WITH (NOLOCK)
			JOIN [Product] p ON [pv].[ProductID] = [p].[ProductID]'

IF (@FilterEntityType = 1) BEGIN
	SET @whereClause = 'JOIN [ProductCategory] m ON [pv].[ProductID] = [m].[ProductID]
						WHERE [m].[CategoryID] =' + CAST(@FilterEntityID as nvarchar(12))
	END
ELSE IF (@FilterEntityType = 2) BEGIN
	SET @whereClause = 'JOIN [ProductSection] m ON [pv].[ProductID] = [m].[ProductID] 
						WHERE [m].[SectionID] =' + CAST(@FilterEntityID as nvarchar(12))
	END
ELSE IF (@FilterEntityType = 3) BEGIN
	SET @whereClause = 'JOIN [ProductManufacturer] m ON [pv].[ProductID] = [m].[ProductID]
						WHERE [m].[ManufacturerID] =' + CAST(@FilterEntityID as nvarchar(12))
	END
ELSE IF (@FilterEntityType = 4) BEGIN
	SET @whereClause = 'JOIN [ProductDistributor] m ON [pv].[ProductID] = [m].[ProductID] 
						WHERE [m].[DistributorID] =' + CAST(@FilterEntityID as nvarchar(12))
	END

IF (@Deleted = 0 AND @FilterEntityType = 0)
	SET @whereClause = ' WHERE [pv].[Deleted] = 0'
ELSE IF (@Deleted = 0 AND @FilterEntityType > 0)
	SET @whereClause = ISNULL(@whereClause, '') + ' AND [pv].[Deleted] = 0'

SET @finalSql = @sql + ' ' + ISNULL(@whereClause, '') + 'ORDER BY [pv].[ProductID] ASC'

exec sp_executesql @FinalSql
		
END

GO


PRINT N'Creating [dbo].[aspdnsf_DropColumnWithDefaultConstraint]'
GO

create proc [dbo].[aspdnsf_DropColumnWithDefaultConstraint]
  @TableName varchar(100), 
  @ColumnName varchar(100) 
  
AS
BEGIN
    DECLARE @def varchar(255), @cmd VARCHAR(1000)
    select @def = so.name 
    From dbo.sysobjects so with (nolock)
        join dbo.syscolumns sc with (nolock)
                on so.id = sc.cdefault 
                    and sc.id = object_id(@TableName) 
                    and sc.name = @ColumnName

    IF @@rowcount = 1
        exec ('ALTER TABLE dbo.' + @TableName + ' DROP CONSTRAINT ' + @def)

    exec ('ALTER TABLE dbo.' + @TableName + ' DROP COLUMN ' + @ColumnName)

END

GO
PRINT N'Creating [dbo].[aspdnsf_WSIUpdateMappings]'
GO


create proc dbo.aspdnsf_WSIUpdateMappings
    @xml text
  
AS
SET NOCOUNT ON

    create table #tmp (id int not null, displayorder int)
    DECLARE @pid varchar(50), @pem varchar(50), @xpath varchar(8000), @counter int, @cmd varchar(8000)
    DECLARE @hdoc int, @retcode int, @AutoCleanup bit

    EXEC @retcode = sp_xml_preparedocument @hdoc OUTPUT, @xml                        

    SELECT @AutoCleanup = case AutoCleanup when 'true' then 1 else 0 end
    FROM OPENXML(@hdoc, '/Mappings', 0) WITH (AutoCleanup varchar(5))

    set @counter = 1
    select @xpath = '/Mappings/Product[' + convert(varchar(10), @counter) + ']'

    SELECT top 1 @pid = id, @pem = EntityName
    FROM OPENXML(@hdoc, @xpath, 0) WITH (id varchar(10), EntityName varchar(50))

    while @@rowcount > 0 begin
        if @pem in ('category', 'section', 'manufacturer', 'distributor', 'affiliate', 'vector', 'genre') and isnumeric(@pid) = 1 begin
            select @xpath = @xpath + '/Entity'

            truncate table #tmp
            insert #tmp 
            SELECT *
            FROM OPENXML(@hdoc, @xpath, 0) WITH (id int, displayorder int)

            -- Update display order for existing mappings
            set @cmd = 'update dbo.product' + @pem + ' set displayorder = isnull(t.displayorder, pe.displayorder) from dbo.product' + @pem + ' pe with (nolock) join #tmp t on pe.Productid = ' + @pid + ' and pe.' + @pem + 'id = t.id'
            exec (@cmd)

            -- Insert new mappings
            set @cmd = 'insert dbo.product' + @pem + '(ProductID, ' + @pem + 'id, displayorder, createdon) select ' + @pid + ', id, displayorder, getdate() from #tmp where not exists (select * from dbo.product' + @pem + ' with (nolock) where ProductID = ' + @pid + ' and ' + @pem + 'id = #tmp.id)'
            exec (@cmd)

            -- if auto clenaup then remove mapping that are not in the imput xml document
            if @AutoCleanup = 1 begin
                set @cmd = 'delete dbo.product' + @pem + ' from dbo.product' + @pem + ' pe with (nolock) left join #tmp t on pe.productid = ' + @pid + ' and pe.categoryid = t.id where t.id is null'
                exec (@cmd)
            end 

            set @counter = @counter + 1
            select @xpath = '/Mappings/Product[' + convert(varchar(10), @counter) + ']'

            SELECT top 1 @pid = id, @pem = EntityName
            FROM OPENXML(@hdoc, @xpath, 0) WITH (id varchar(10), EntityName varchar(50))
        end
    end

    exec sp_xml_removedocument @hdoc

    drop table #tmp 
GO
PRINT N'Creating [dbo].[aspdnsf_PageQuery]'
GO


CREATE PROCEDURE [dbo].[aspdnsf_PageQuery]
  @Select varchar(8000), 
  @OrderBy varchar(2000), 
  @PageNum int,
  @PageSize int,
  @StatsFirst int = 1
  
AS
SET NOCOUNT ON 

BEGIN

declare @ColList varchar(8000);
declare @Where varchar(8000);
declare @i int;  
declare @i2 int;
declare @tmp varchar(8000);
declare @dec varchar(8000);
declare @f varchar(100);
declare @d varchar(100);
declare @Symbol char(2);
declare @sTmp varchar(2000)
declare @SQL varchar(8000);
declare @Sort varchar(2000);

declare @StartRow int;
declare @EndRow int;
declare @Total int;

set @StartRow = ((@PageNum-1)* @PageSize)+1
set @EndRow = @StartRow + @PageSize - 1

--Print @Select


if ltrim(rtrim(@OrderBy))=''
begin
  Print @OrderBy
  set @i = charindex('order by',@Select)
  set @OrderBy = ltrim(rtrim(right(@Select,len(@Select)-@i-8)))
  Print @OrderBy
  set @Select = left(@Select,@i-1)
  Print @Select
end


create table #recCount(RecCount int)
exec('INSERT into #recCount (RecCount) select count(*) from ('+@Select+') a')
select @Total=RecCount from #recCount
drop table #recCount



set @Sort = @OrderBy + ', '
set @dec = ''
set @Where  = ''
set @SQL = ''

set @i = charindex(',' , @Sort)
while @i != 0
 begin
  set @tmp = left(@Sort,@i-1)
  set @i2 = charindex(' ', @tmp)

  set @f = case when @i2=0 then ltrim(rtrim(@tmp)) else ltrim(rtrim(left(@tmp,@i2-1))) end
  set @d = case when @i2=0 then '' else ltrim(rtrim(substring(@tmp,@i2+1,100))) end

  set @Sort = rtrim(ltrim(substring(@Sort,@i+1,100)))
  set @i = charindex(',', @Sort)
  set @symbol = case when @d = 'DESC' then '<' else '>' end + 
                case when @i=0 then '=' else '' end

  set @dec = @dec + 'declare @' + @f + ' sql_variant; '
  set @ColList = isnull(replace(replace(@colList,'>','='),'<','=') + ' and ','') +
                 @f + ' ' + @Symbol + ' @' + @f
  set @Where = @Where + ' OR (' + @ColList + ') '
  set @SQL = @SQL + ', @' + @f + '= ' + @f
 end

set @SQL = @dec + ' ' +
           'SELECT top ' + convert(varchar(10), @StartRow) + ' ' + substring(@SQL,3,7000) + ' from (' + @Select + ') a ORDER BY ' +
           @OrderBy + '; ' + 
           'select top ' + convert(varchar(10), 1 + @EndRow - @StartRow) + ' * from (' + @Select + ') a WHERE ' + 
           substring(@Where,4,7000) + ' ORDER BY ' + @OrderBy


IF @StatsFirst = 1 BEGIN
    SELECT @Total TotalRows, ceiling(@Total*1.0/@PageSize) Pages
    exec(@SQL)
END
ELSE BEGIN
    exec(@SQL)
    SELECT @Total TotalRows, ceiling(@Total*1.0/@PageSize) Pages
END

--PRINT @SQL
END

GO
PRINT N'Creating [dbo].[aspdnsf_GetRecentComments]'
GO


create proc [dbo].[aspdnsf_GetRecentComments]
    @votingcustomer int,
    @pagesize int = 20,
    @pagenum int = 1,
    @sort tinyint = 0,
    @categoryid int = 0
  
AS
BEGIN
    SET NOCOUNT ON

    CREATE TABLE #tmp (rownum int not null identity, RatingID int, ProductID int, CustomerID int, Rating int, Comments ntext, FoundHelpful tinyint, FoundNotHelpful tinyint, CreatedOn datetime, IsFilthy tinyint, ProductName nvarchar(400), ProductSEName nvarchar(150), ProductGuid uniqueidentifier, RatingCustomerName nvarchar(200), CommentHelpFul smallint, MyRating tinyint)


    DECLARE @totalcomments int, @cmd nvarchar(4000)

    SET @cmd = N'SELECT  r.RatingID, r.ProductID, r.CustomerID, r.Rating, r.Comments, r.FoundHelpful, 
            r.FoundNotHelpful, r.CreatedOn, r.IsFilthy, 
            p.Name, p.SEName, p.ProductGuid,
            c.FirstName + '' '' + c.LastName RatingCustomerName,
            isnull(convert(smallint, h.HelpFul), -1),
            isnull(r2.Rating, 0)
    FROM dbo.Rating r with (nolock) 
        join [dbo].Customer c with (nolock) ON r.CustomerID = c.CustomerID 
        join [dbo].Product p with (nolock) ON r.ProductID = p.ProductID 
        left join [dbo].RatingCommentHelpfulness h with (nolock) on h.productid = r.ProductID and h.RatingCustomerID = r.CustomerID and h.VotingCustomerID = @votingcustomerid
        left join [dbo].Rating r2 with (nolock) on r2.CustomerID = @votingcustomerid and r.ProductID = r2.ProductID 
    WHERE r.HasComment <> 0 AND p.Deleted = 0 AND p.Published <> 0 '
    + case when @categoryid > 0 then
    'AND r.ProductID in (select productid from [dbo].productcategory where categoryid=' + CAST(@categoryid AS nvarchar(10)) +')'
    else '' end
    + ' ORDER BY ' + case @sort when 0 then 'r.CreatedOn desc' when 1 then 'r.CreatedOn asc' when 2 then 'r.Rating desc, r.CreatedOn desc' when 3 then 'r.Rating asc, r.CreatedOn desc' end

    INSERT #tmp (RatingID, ProductID, CustomerID, Rating, Comments, FoundHelpful, FoundNotHelpful, CreatedOn, IsFilthy, ProductName, ProductSEName, ProductGuid, RatingCustomerName, CommentHelpFul, MyRating)
    EXEC sp_executesql @cmd, N'@votingcustomerid int', @votingcustomerid = @votingcustomer


    SET @totalcomments = @@rowcount

    SELECT * 
    FROM #tmp
    WHERE rownum >= @pagesize*(@pagenum-1)+1 and rownum <= @pagesize*(@pagenum)
    ORDER BY rownum

    SELECT @totalcomments totalcomments, ceiling(@totalcomments*1.0/@pagesize) pages

END

GO
PRINT N'Creating [dbo].[aspdnsf_GetProductComments]'
GO


CREATE proc [dbo].[aspdnsf_GetProductComments]
    @ProductID		int,
    @votingcustomer int,
    @pagesize		int = 20,
    @pagenum		int = 1,
    @sort			tinyint = 0,
	@storeID		int = 1
  
AS
BEGIN
    SET NOCOUNT ON

    CREATE TABLE #tmp (rownum int not null identity, RatingID int, ProductID int, CustomerID int, Rating int, 
                       Comments ntext, FoundHelpful tinyint, FoundNotHelpful tinyint, CreatedOn datetime, 
                       IsFilthy tinyint, ProductName nvarchar(400), ProductSEName nvarchar(150), ProductGuid uniqueidentifier, 
                       FirstName nvarchar(100), LastName nvarchar(100),
                       RatingCustomerName nvarchar(200), CommentHelpFul smallint, MyRating tinyint)


    DECLARE @totalcomments int, @cmd nvarchar(4000)

    SET @cmd = N'SELECT  r.RatingID, r.ProductID, r.CustomerID, r.Rating, r.Comments, r.FoundHelpful, 
            r.FoundNotHelpful, r.CreatedOn, r.IsFilthy, 
            p.Name, p.SEName, p.ProductGuid,
            c.FirstName, c.LastName, 
            c.FirstName + '' '' + c.LastName RatingCustomerName,
            isnull(convert(smallint, h.HelpFul), -1),
            isnull(r2.Rating, 0)
    FROM dbo.Rating r with (nolock) 
        join [dbo].Customer c with (nolock) ON r.CustomerID = c.CustomerID 
        join [dbo].Product p with (nolock) ON r.ProductID = p.ProductID 
        left join [dbo].RatingCommentHelpfulness h with (nolock) on h.productid = r.ProductID and h.RatingCustomerID = r.CustomerID and h.VotingCustomerID = @votingcustomerid and h.StoreID = r.StoreID
        left join [dbo].Rating r2 with (nolock) on r2.CustomerID = @votingcustomerid and r.ProductID = r2.ProductID and r2.StoreID = r.StoreID
    WHERE r.HasComment <> 0 AND p.Deleted = 0 AND p.Published <> 0 and r.ProductID = @ProdID and r.StoreID = @store AND r.IsFilthy = 0
    ORDER BY ' + case @sort 
                    when 1 then'r.FoundHelpful desc, r.CreatedOn desc' 
                    when 2 then'r.FoundHelpful asc, r.CreatedOn desc' 
                    when 3 then'r.CreatedOn desc' 
                    when 4 then'r.CreatedOn asc' 
                    when 5 then'r.Rating desc, r.CreatedOn desc' 
                    when 6 then'r.Rating asc, r.CreatedOn desc' 
                 end

    INSERT #tmp (RatingID, ProductID, CustomerID, Rating, Comments, FoundHelpful, FoundNotHelpful, CreatedOn, IsFilthy, ProductName, ProductSEName, ProductGuid, FirstName, LastName, RatingCustomerName, CommentHelpFul, MyRating)
    EXEC sp_executesql @cmd, N'@votingcustomerid int, @ProdID int, @store int', @votingcustomerid = @votingcustomer, @ProdID = @ProductID, @store = @storeID

    SET @totalcomments = @@rowcount

    SELECT @totalcomments totalcomments, ceiling(@totalcomments*1.0/@pagesize) pages

    SELECT * 
    FROM #tmp
    WHERE rownum >= @pagesize*(@pagenum-1)+1 and rownum <= @pagesize*(@pagenum)
    ORDER BY rownum
END


GO
PRINT N'Adding constraints to [dbo].[Store]'
GO
ALTER TABLE [dbo].[Store] ADD CONSTRAINT [CHK_Store_HasURI] CHECK ((NOT ([ProductionURI] IS NULL AND [StagingURI] IS NULL AND [DevelopmentURI] IS NULL)))

GO


PRINT N'Adding constraints to [dbo].[GlobalConfig]'
GO
ALTER TABLE [dbo].[GlobalConfig] ADD CONSTRAINT [UQ_GlobalConfig_Name] UNIQUE NONCLUSTERED  ([Name])
GO


/* ************************************************************** */
/* UPDATE CONFIG TABLES                                           */
/* ************************************************************** */


		
-- DELETE UNUSED APPCONFIGS
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'Admin_ShowDefaultContents'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AllowCustomerDuplicateEMailAddresses'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AllowZipChangeAgainInCart'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'DarkCellColor'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'Default_VectorColWidth'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'Default_VectorPageSize'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'IsFeaturedCategoryTeaser'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'LimitPlainCategoryListToOneLevel'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'LimitPlainSectionListToOneLevel'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'MaxCatMaps'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'MaxSkinID'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'QQMailDirectSend'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'QuantityDiscount'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'RecentCommentsHistoryDays'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'Search_ShowVectorsInResults'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'SearchAdv_ShowVector'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'SecureAttachment.DocumentFormat'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'SecureAttachment.EMailSubject'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'SecureAttachment.Password'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'SecureAttachment.UserAgent'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'SecureAttachment.UserName'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'SendEMailViaQQMail'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'ShippingTrackingRegEx.ParcelForce'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'ShippingTrackingURL.ParcelForce'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'ShowGatewayMsg'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'TELLUS_CM'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'TELLUS_ISOLanguage'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'TELLUS_LIVE_SERVER'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'TELLUS_ProcessingCode'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'TELLUS_TellusPayID'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'TELLUS_TerminalID'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'TELLUS_TEST_SERVER'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'TELLUS_TransactionType'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'WizardRun'

-- RENAME EXISTING APPCONFIGS
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUK.UseSimulator'		WHERE [Name] = 'ProtX.UseSimulator'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUK.Vendor'			WHERE [Name] = 'ProtX.Vendor'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Live.Abort'		WHERE [Name] = 'ProtXURL.Live.Abort'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Live.Callback' WHERE [Name] = 'ProtXURL.Live.Callback'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Live.Purchase' WHERE [Name] = 'ProtXURL.Live.Purchase'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Live.Refund'	WHERE [Name] = 'ProtXURL.Live.Refund'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Live.Release'	WHERE [Name] = 'ProtXURL.Live.Release'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Simulator.Abort'		WHERE [Name] = 'ProtXURL.Simulator.Abort'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Simulator.Callback'	WHERE [Name] = 'ProtXURL.Simulator.Callback'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Simulator.Purchase'	WHERE [Name] = 'ProtXURL.Simulator.Purchase'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Simulator.Refund'		WHERE [Name] = 'ProtXURL.Simulator.Refund'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Simulator.Release'		WHERE [Name] = 'ProtXURL.Simulator.Release'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Test.Abort'	WHERE [Name] = 'ProtXURL.Test.Abort'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Test.Callback' WHERE [Name] = 'ProtXURL.Test.Callback'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Test.Purchase' WHERE [Name] = 'ProtXURL.Test.Purchase'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Test.Refund'	WHERE [Name] = 'ProtXURL.Test.Refund'
UPDATE [dbo].[AppConfig] SET [Name] = N'SagePayUKURL.Test.Release'	WHERE [Name] = 'ProtXURL.Test.Release'

-- ADD TYPES
-- Update 16 rows in [dbo].[AppConfig]
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SagePayUK.UseSimulator'
UPDATE [dbo].[AppConfig] SET [Description]=N'Your Vendor name supplied by Sage Pay.', [ValueType]=N'string' WHERE [Name]=N'SagePayUK.Vendor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Live.Abort'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Live.Callback'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Live.Purchase'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Live.Refund'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Live.Release'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Simulator.Abort'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Simulator.Callback'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Simulator.Purchase'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Simulator.Refund'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Simulator.Release'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Test.Abort'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Test.Callback'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Test.Purchase'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Test.Refund'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayUKURL.Test.Release'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'2CHECKOUT_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'2CHECKOUT_VendorID'
UPDATE [dbo].[AppConfig] SET [Description]=N'Comma separated list of CreditCardTypeIDs that 3-D Secure transactions can be processed for with the currently active gateway. You need to get the desired card type ids from the Configuration -> Payment -> Credit Card Types page.' WHERE [Name]=N'3DSecure.CreditCardTypeIDs'
UPDATE [dbo].[AppConfig] SET [Description]=N'This value filters the likeness of words. The higher the value, the wider the range of results the 404 page will return.   Suggested value .60 to .70.', [ValueType]=N'double' WHERE [Name]=N'404.ComparisonDistance'
UPDATE [dbo].[AppConfig] SET [Description]=N'This appconfig sets the upper limit of the suggested links that will display on the 404 page.', [ValueType]=N'integer' WHERE [Name]=N'404.NumberOfSuggestedLinks'
UPDATE [dbo].[AppConfig] SET [Description]=N'Specifies what type(s) of pages to suggest on the 404 page (blank by default).  Allowed values are product, category, manufacturer, section, topic.', [ValueType]=N'multiselect', [AllowableValues]=N'product, category, manufacturer, section, topic' WHERE [Name]=N'404.VisibleSuggestions'
UPDATE [dbo].[AppConfig] SET [Description]=N'Enter the NAME of the quantity discount table that you want to be active for ALL products on the site. If you enter a discount table name here, it applies to all products. If a category has a discount table also specified, the category discount table overrides this one. If a product has a discount table specified, the product discount table overrides the category and the site one. Discount table order of precedence: 1. Product Level, 2. Category Level, and 3. Site Level.', [ValueType]=N'string' WHERE [Name]=N'ActiveQuantityDiscountTable'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AddressCCSaltField'
UPDATE [dbo].[AppConfig] SET [Description]=N'If this is set to STAY, then after an add to cart action the site will go back to the product page the customer was just on. If blank (the default), customers will be sent to the shopping cart page.', [ValueType]=N'string' WHERE [Name]=N'AddToCartAction'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin.EntityFrameMenuWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Admin.MultiGalleryImageWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Admin.ShowNewsFeed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Admin.ShowSecurityFeed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Admin.ShowSponsorFeed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin_DefaultInventory'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin_DefaultProductTypeID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin_DefaultSalesPromptID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin_DefaultTaxClassID'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, the order statistics on the admin home page will display as a chart.  If false, they will be in a table.', [ValueType]=N'boolean' WHERE [Name]=N'Admin_OrderStatisticIsChart'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin_ProductPageSize'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, the SQL used to generate admin reports will be shown.', [ValueType]=N'boolean' WHERE [Name]=N'Admin_ShowReportSQL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Admin_SpecsInlineByDefault'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin_TextAreaHeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin_TextAreaHeightSmall'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Admin_TextAreaWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AdminAlert.BackgroundColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AdminAlert.FontColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AdminAlert.Message'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AdminDir'
UPDATE [dbo].[AppConfig] SET [Description]=N'The number of days between password resets for admins users.', [ValueType]=N'integer' WHERE [Name]=N'AdminPwdChangeDays'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AffiliateEMailAddress'
UPDATE [dbo].[AppConfig] SET [Description]=N'Products are removed from customer carts after this many days.', [ValueType]=N'integer' WHERE [Name]=N'AgeCartDays'
UPDATE [dbo].[AppConfig] SET [Description]=N'Products are removed from customer wish lists after this many days.', [ValueType]=N'integer' WHERE [Name]=N'AgeWishListDays'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AllowAddressChangeOnCheckoutShipping'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, Products with no SKU''s whatsoever (for the product or variant) can be added to the cart. It is highly recommended that this value be set to false. The admin personnel must then assign a SKU to every product/variant. This keeps things orderly!', [ValueType]=N'boolean' WHERE [Name]=N'AllowEmptySkuAddToCart'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AllowGiftRegistryQuantities'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AllowMultipleShippingAddressPerOrder'
UPDATE [dbo].[AppConfig] SET [Description]=N'if true, then the admin user can edit the next ship date for recurring orders.', [ValueType]=N'boolean' WHERE [Name]=N'AllowRecurringIntervalEditing'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, then the customer can specify a shipping address which is different than their billing address. If false, then ONLY the billing address can be entered, and the shipping address will be set to match it.', [ValueType]=N'boolean' WHERE [Name]=N'AllowShipToDifferentThanBillTo'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, each shipping group on an order will have a notes field that customers can type in.', [ValueType]=N'boolean' WHERE [Name]=N'AllowShoppingCartItemNotes'
UPDATE [dbo].[AppConfig] SET [Description]=N'Determines the maximum number of AlsoBought products that will be displayed at the bottom of product pages.', [ValueType]=N'integer' WHERE [Name]=N'AlsoBoughtNumberToDisplay'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'enum', [AllowableValues]=N'Grid, Table' WHERE [Name]=N'AlsoBoughtProductsFormat'
UPDATE [dbo].[AppConfig] SET [Description]=N'If using grid layout for AlsoBought products (see the AlsoBoughtProductsFormat AppConfig), this is the # of cols in the grid.', [ValueType]=N'integer' WHERE [Name]=N'AlsoBoughtProductsGridColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AnonCheckoutReqEmail'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ApplyShippingHandlingExtraFeeToFreeShipping'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AuditLog.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AUTHORIZENET_Verify_Addresses'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_X_DELIM_CHAR'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AUTHORIZENET_X_DELIM_DATA'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_X_EMAIL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AUTHORIZENET_X_EMAIL_CUSTOMER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_X_ENCAP_CHAR'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_X_Login'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_X_METHOD'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AUTHORIZENET_X_RECURRING_BILLING'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'AUTHORIZENET_X_RELAY_RESPONSE'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_X_Tran_Key'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AUTHORIZENET_X_VERSION'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, the first choice in the size or color select lists on add to cart forms will be auto-selected.  If false, customers will be forced to choose a size or color before adding to the cart (recommended).', [ValueType]=N'boolean' WHERE [Name]=N'AutoSelectFirstSizeColorOption'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'AutoVariantFillColumnDelimiter'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'BadLoginLockTimeOut'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'BestSellersN'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'BestSellersShowPics'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BoxFrameStyle'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BreadcrumbSeparator'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, then when bulk importing shipments that are not voided, shipped notification emails will be sent to the customers of the orders whose shipments were imported.', [ValueType]=N'boolean' WHERE [Name]=N'BulkImportSendsShipmentNotifications'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'BuySafe.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.Hash'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.InvitationCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.JsUrl'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.MspId'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.RegUrl'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.SealSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.StoreToken'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'BuySafe.Username'
UPDATE [dbo].[AppConfig] SET [Description]=N'If the CacheMenus AppConfig is set to true, this is the duration (in MINUTES) between cache updates.  This should not be set to a value much lower than 60, or the cache will not have time to build up enough to be of any use.', [ValueType]=N'integer' WHERE [Name]=N'CacheDurationMinutes'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'CacheEntityPageHTML'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, then site menus and MANY MANY other navigational and dataset elements on the store and admin site are cached for faster performance. If cached, they update every CacheDurationMinutes, so changes made on the admin site may not take effect until the cache expires. To force the store to reload cache, restart the site through IIS or by ''touching'' the web.config. In production, CacheMenus should almost always be set to true for performance reasons.', [ValueType]=N'boolean' WHERE [Name]=N'CacheMenus'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Captcha.AllowedCharactersRegex'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Captcha.CaseSensitive'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Captcha.HorizontalColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Captcha.ImageBackColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Captcha.ImageForeColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Captcha.MaxAsciiValue'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Captcha.NumberOfCharacters'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Captcha.TextBackColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Captcha.TextForeColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Captcha.VerticalColor'
UPDATE [dbo].[AppConfig] SET [Description]=N'If false, the customer MUST enter their 3 or 4 digit card verification code. if true, they can enter it, but it is not required by the javascript form validation routines.  Note that this controls on-site validation only, your payment gateway may require that value regardless of this setting.', [ValueType]=N'boolean' WHERE [Name]=N'CardExtraCodeIsOptional'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardiaServices.Live.MerchantToken'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardiaServices.Live.UserToken'
UPDATE [dbo].[AppConfig] SET [Description]=N'Cardia Services gateway URL.  DO NOT change this unless instructed to do so!', [ValueType]=N'string' WHERE [Name]=N'CardiaServices.SOAPURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardiaServices.Test.MerchantToken'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardiaServices.Test.UserToken'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'CardinalCommerce.Centinel.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'CardinalCommerce.Centinel.IsLive'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'CardinalCommerce.Centinel.MapsTimeout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.MerchantID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.MsgType.Authenticate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.MsgType.Lookup'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'CardinalCommerce.Centinel.MyECheckMarkAsCaptured'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'CardinalCommerce.Centinel.NumRetries'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.ProcessorID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.TermURL'
UPDATE [dbo].[AppConfig] SET [Description]=N'Your Cardinal Centinel Assigned Transaction Password.', [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.TransactionPwd'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.TransactionUrl.Live'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.TransactionUrl.Test'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.Version.Authenticate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CardinalCommerce.Centinel.Version.Lookup'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'CartMinOrderAmount'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CategoryImg_icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CategoryImg_large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CategoryImg_medium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CENTRALPAYMENTS_AssociateName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CENTRALPAYMENTS_AssociatePassword'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Checkout.UseOnePageCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Checkout.UseOnePageCheckout.UseFinalReviewOrderPage'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, then the customer''s coupon code will be cleared in their customer record after each order. If false, then the customer''s coupon code will remain active in their customer record.  Generally best left set to true.', [ValueType]=N'boolean' WHERE [Name]=N'ClearCouponAfterOrdering'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ClearOldCartOnSignin'
UPDATE [dbo].[AppConfig] SET [Description]=N'If non-zero, this extra amount will be added to all order totals using the C.O.D. payment method, for orders where the shipping total is already non zero. This cost is NOT added to orders where the shipping total computes to 0.00!! This value should be a dollar amount, without leading $ character, e.g. 5.00', [ValueType]=N'decimal' WHERE [Name]=N'CODHandlingExtraFee'
UPDATE [dbo].[AppConfig] SET [Description]=N'If no background color is specified in a skin, this value will be used for the content area.', [ValueType]=N'string' WHERE [Name]=N'ContentsBGColorDefault'
UPDATE [dbo].[AppConfig] SET [Description]=N'This can be set to the relative path of a page on your site (ie c-1-myproducts.aspx, products.htm, etc).  Customers who click Continue Shopping on the shopping cart page will be sent there instead of back to the page they were on.', [ValueType]=N'string' WHERE [Name]=N'ContinueShoppingURL'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, then customer level 0 (all customers) can use purchase orders (assuming that payment method is enabled).', [ValueType]=N'boolean' WHERE [Name]=N'CustomerLevel0AllowsPOs'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CustomerPwdValidator'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.keyFilename'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.keysDirectory'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.LiveURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.merchantID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.paCountryCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.paMerchantName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.paMerchantURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.PITURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'CYBERSOURCE.TestURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'CYBERSOURCE.UsePIT'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, customer a''s gift registry desired quantity will be decreased by N if customer b purchased N products from customer a''s gift registry. If false, the customer''s gift registry doesn''t change unless the customer does it manually.', [ValueType]=N'boolean' WHERE [Name]=N'DecrementGiftRegistryOnOrder'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_CategoryColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_CategoryPageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_DistributorColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_DistributorPageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_DocumentColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_DocumentPageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_GenreColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_GenrePageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_LibraryColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_LibraryPageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_ManufacturerColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_ManufacturerPageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_ProductColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_ProductPageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_SectionColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Default_SectionPageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultAddToCartQuantity'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'DefaultCrop'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'DefaultCropHorizontal'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'DefaultCropVertical'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultCustomerLevelID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'DefaultFillColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultHeight_icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultHeight_large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultHeight_medium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultQuality'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultShippingCalculationID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultSkinID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'DefaultStretch'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultWidth_icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultWidth_large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DefaultWidth_medium'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, then the download e-mail will NOT be sent automatically.  You will have to send the order download to the customer via the Send Download Email button on the View/Manage Orders page. This is to allow for fraud screening before releasing digital products.', [ValueType]=N'boolean' WHERE [Name]=N'DelayedDownloads'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, then the distributor e-mail will NOT be sent automatically.  You will have to send the order download to the customer via the Send Distributor Email button on the View/Manage Orders page. This is to allow for fraud screening and order verification before shipping products.', [ValueType]=N'boolean' WHERE [Name]=N'DelayedDropShipNotifications'
UPDATE [dbo].[AppConfig] SET [Description]=N'If this is set to true, customers must be logged in to add to the wishlist.', [ValueType]=N'boolean' WHERE [Name]=N'DisallowAnonCustomerToCreateWishlist'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, coupon fields will be hidden.', [ValueType]=N'boolean' WHERE [Name]=N'DisallowCoupons'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, the order notes field on the shopping cart page will be hidden.', [ValueType]=N'boolean' WHERE [Name]=N'DisallowOrderNotes'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'DisallowShippingToPOBoxes'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Dispatch_MAX_SMS_MSG_LENGTH'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'Dispatch_OrderThreshold'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Dispatch_SiteName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Dispatch_ToPhoneNumber'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'DisplayOutOfStockOnEntityPages'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'DisplayOutOfStockOnProductPages'
UPDATE [dbo].[AppConfig] SET [Description]=N'This will show ''out of stock'' or ''In stock'' message depending on the inventory of product. If the inventory is less than OutOfStockThreshold value, it will display ''Out of stock'' message, otherwise ''In stock'' message. This can enable by setting the value to true and will automatically disable the HideProductsWithLessThanThisInventoryLevel. You can totaly configure it in Configuration -> Inventory Control page.', [ValueType]=N'boolean' WHERE [Name]=N'DisplayOutOfStockProducts'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'DistributorEMailCC'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'DistributorImg_icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'DistributorImg_large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'DistributorImg_medium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'DoNotShowNewsOnHomePage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'DumpDownloadInfo'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'DynamicRelatedProducts.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'DynamicRelatedProducts.NumberDisplayed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'EFSNET_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'EFSNET_LIVE_STOREID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'EFSNET_LIVE_STOREKEY'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'EFSNET_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'EFSNET_TEST_STOREID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'EFSNET_TEST_STOREKEY'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'EFSNET_VERIFY_ADDRESSES'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'EncryptIterations'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'EntitySelectLists.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'eProcessingNetwork_Verify_Addresses'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_X_DELIM_CHAR'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'eProcessingNetwork_X_DELIM_DATA'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_X_EMAIL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'eProcessingNetwork_X_EMAIL_CUSTOMER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_X_ENCAP_CHAR'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_X_Login'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_X_METHOD'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'eProcessingNetwork_X_RECURRING_BILLING'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'eProcessingNetwork_X_RELAY_RESPONSE'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_X_Tran_Key'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eProcessingNetwork_X_VERSION'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.api_token.Live'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.api_token.Test'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.country'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.crypt'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'eSelectPlus.includeAVS'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.store_id.Live'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.store_id.Test'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.URL.CA.Live'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.URL.CA.LiveMPI'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.URL.CA.Test'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.URL.CA.TestMPI'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.URL.US.Live'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.URL.US.LiveMPI'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.URL.US.Test'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eSelectPlus.URL.US.TestMPI'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, every user action is logged in the LOG_CustomerEvent table. WARNING: this table could get very large. This can be helpful to figure out what users are doing on the site, what pages they are visiting, etc. You need to manually update the [dbo].LOG_Event and the page code to track events you want on each page.', [ValueType]=N'boolean' WHERE [Name]=N'EventLoggingEnabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eWay.Live.CustomerID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eWay.Live.URL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eWay.Test.CustomerID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'eWay.Test.URL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'FedexShipManager.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'FilterByCustomerLevelIsAscending'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'FilterDocumentsByAffiliate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'FilterDocumentsByCustomerLevel'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'FilterOutShippingMethodsThatHave0Cost'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'FilterProductsByAffiliate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'FilterProductsByCustomerLevel'
UPDATE [dbo].[AppConfig] SET [Description]=N'If true, upon order completion, the user will be forcibly signed out of the site.', [ValueType]=N'boolean' WHERE [Name]=N'ForceSignoutOnOrderCompletion'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'FreeShippingAllowsRateSelection'
UPDATE [dbo].[AppConfig] SET [Description]=N'If set to a dollar amount (no $ sign) orders equal to or greater than this amount will have free shipping. A value of 0.00 or blank disables this feature.', [ValueType]=N'decimal' WHERE [Name]=N'FreeShippingThreshold'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'GatewayRetries'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GiftCard.CertificateProductTypeIDs'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GiftCard.EmailProductTypeIDs'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GiftCard.PhysicalProductTypeIDs'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoNonSecureAgain'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Google.AnalyticsAccount'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Google.EcomOrderTrackingEnabled'
UPDATE [dbo].[AppConfig] SET [Description]=N'This enables customers to check out with Google Checkout without registering on the store site.  When you set this to true you should also set the AllowCustomerDuplicateEMailAddresses AppConfig parmeter to true, or customers will only be able to purchase once.', [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.AllowAnonCheckout'
UPDATE [dbo].[AppConfig] SET [Description]=N'Set this to true to verify http basic authentication on the gcCallback.aspx file.  This should almost never be set to true!', [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.AuthenticateCallback'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.BaseUrl'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'GoogleCheckout.CarrierCalculatedDefaultPrice'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.CarrierCalculatedFreeOption'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.CarrierCalculatedPackage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.CarrierCalculatedShippingEnabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.CarrierCalculatedShippingOptions'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.ConversionParameters'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.ConversionURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.DefaultDomesticShipToCity'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.DefaultDomesticShipToCountry'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.DefaultDomesticShipToState'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.DefaultDomesticShipToZip'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.DefaultInternationalShipToCity'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.DefaultInternationalShipToCountry'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.DefaultInternationalShipToState'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.DefaultInternationalShipToZip'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'GoogleCheckout.DefaultShippingMarkup'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'double' WHERE [Name]=N'GoogleCheckout.DefaultTaxRate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.DiagnosticsOnly'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.LiveCheckoutButton'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.LogFileName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.LogMessages'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.MerchantId'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.MerchantKey'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.SandBoxCheckoutButton'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.SandBoxCheckoutURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.SandboxMerchantId'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleCheckout.SandboxMerchantKey'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.SendStoreReceipt'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.ShippingIsTaxed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.ShowOnCartPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.UseSandbox'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'GoogleCheckout.UseTaxTables'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleSiteMap.EntityChangeFreq'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleSiteMap.EntityPriority'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleSiteMap.ObjectChangeFreq'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleSiteMap.ObjectPriority'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleSiteMap.TopicChangeFreq'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GoogleSiteMap.TopicPriority'
UPDATE [dbo].[AppConfig] SET [Description]=N'Change to match Google requirements, if necessary. This should not be changed without full understanding of what you are doing!  All information on supported schemas must come from Google.', [ValueType]=N'string' WHERE [Name]=N'GoogleSiteMap.Xmlns'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GotOrderEMailFrom'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GotOrderEMailFromName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GotOrderEMailTo'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GraphicsColorDefault'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'GreyCellColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HashAlgorithm'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HeaderBGColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'HideKitPrice'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'HideKitQuantity'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'HidePasswordFieldDuringCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'HidePicsInTableCondensed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'HideProductNextPrevLinks'
UPDATE [dbo].[AppConfig] SET [Description]=N'If not -1, the site will filter out products that have less than the inventory # in stock that you specify here. Leave set to -1 to disable this filtering. For products with multiple variants, and sizes & colors, etc, the SUM of those inventory items is used to determine if to filter the product. So the sum of all variant inventory must be less than this number before it will be filtered out.', [ValueType]=N'integer' WHERE [Name]=N'HideProductsWithLessThanThisInventoryLevel'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HomeTemplate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'HomeTemplateAsIs'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.CcpaClientID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.CcpaURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.ClientAlias'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.ClientID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.ClientName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.ClientPassword'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.DocVersion'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.Live.Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.Mode.Live'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.Mode.Test'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.Pipeline'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'HSBC.Test.Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string', [GroupName]=N'GATEWAY' WHERE [Name]=N'IATS.AgentCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string', [GroupName]=N'GATEWAY' WHERE [Name]=N'IATS.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string', [GroupName]=N'GATEWAY' WHERE [Name]=N'IATS.URL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'IDEPOSIT_LIVE_CLERKID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'IDEPOSIT_LIVE_PASSWORD'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'IDEPOSIT_LIVE_URL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'IDEPOSIT_LIVE_USERNAME'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'IDEPOSIT_TEST_CLERKID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'IDEPOSIT_TEST_PASSWORD'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'IDEPOSIT_TEST_URL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'IDEPOSIT_TEST_USERNAME'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'IncludeGoogleTrackingCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'IncludeOvertureTrackingCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'InitializationVector'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Innova.XHTML'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'InternationalCheckout.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'InternationalCheckout.ForceForInternationalCustomers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'InternationalCheckout.StoreID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'InternationalCheckout.TestMode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Inventory.LimitCartToQuantityOnHand'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'IPAddress.MaxFailedTransactions'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'IPAddress.RefuseRestrictedIPsFromSite'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'IsFeaturedCategoryID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ITransact.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ITransact.Sale_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ITransact.Test_FirstName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ITransact.Vendor_ID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ITransact.Verify_Addresses'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ITransact.VoidRefund_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'JETPAY_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'JETPAY_MERCHANTID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'JETPAY_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'KeySize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'KitCategoryID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'LargeCreatesOthers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'LargeOverwritesOthers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'LightCellColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'LINKPOINT_CONFIGFILE'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'LINKPOINT_KEYFILE'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'LINKPOINT_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'LINKPOINT_PORT'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'LINKPOINT_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'LINKPOINT_Verify_Addresses'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'LinkToProductPageInCart'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'LiveServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Localization.CurrencyCacheMinutes'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Localization.CurrencyFeedBaseRateCurrencyCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Localization.CurrencyFeedUrl'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Localization.CurrencyFeedXmlPackage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Localization.StoreCurrency'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Localization.StoreCurrencyNumericCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Localization.WeightUnits'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MailCheckReminder'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MailingMgr.BlockSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'MailingMgr.SendEachEmailSeparately'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MailMe_FromAddress'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MailMe_FromName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MailMe_Port'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MailMe_Pwd'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MailMe_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MailMe_ToAddress'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MailMe_ToName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MailMe_User'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'MailMe_UseSSL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ManufacturerImg_icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ManufacturerImg_large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ManufacturerImg_medium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ManufacturersLinkToOurPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MaxBadLogins'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MaxMenuSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'MaxMind.DelayDownloadThreshold'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'MaxMind.DelayDropShipThreshold'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'MaxMind.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'MaxMind.FailScoreThreshold'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MaxMind.LicenseKey'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MaxMind.SOAPURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MaxSlides'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MediumCellColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'MicroPay.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Micropay.HideOnCartPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Micropay.ShowTotalOnTopOfCartPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'MicroStyle'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MinCartItemsBeforeCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MiniCartMaxIconHeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MiniCartMaxIconWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'MinOrderWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MinSearchStringLength'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'MultiColorMakesSwatchAndMap'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'MultiImage.UseProductIconPics'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'MultiMakesMicros'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'MultiShipMaxItemsAllowed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'NETAXEPT.Error.Setup'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NETAXEPT.Live_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NETAXEPT.Live_UI'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NETAXEPT.Merchant_Id'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NETAXEPT.Merchant_Token'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NETAXEPT.MerchantSettings.RedirectUrl'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NETAXEPT.Test_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NETAXEPT.Test_UI'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NetBilling.Account_ID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NetBilling.Live_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NetBilling.Pay_Type'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NetBilling.Site_Tag'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NetBilling.Test_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'NetBilling.Verify_Addresses'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NewPwdAllowedChars'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NewsTeaser'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'NextKeyChange'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'NumHomePageSpecials'
UPDATE [dbo].[AppConfig] SET [Description]=N'Prevents admin users from re-using any of the specified number of previously used passwords, the PA-DSS requirement is 4.', [ValueType]=N'integer' WHERE [Name]=N'NumPreviouslyUsedPwds'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.LivePostURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.LiveServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.LiveServerOrder'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.PSPID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.PSWD'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.SHASignature'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.TestPostURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.TestServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.TestServerOrder'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Ogone.Use3TierMode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Ogone.USERID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'OnSaleForTextColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'OrderEditingEnabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'OrdersCCSaltField'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'OrderShowCCPwd'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'OrderSummaryReportFields'
UPDATE [dbo].[AppConfig] SET [Description]=N'This is the flag use in DisplayOutOfStockProducts. To enable this you must first set DisplayOutOfStockProducts to true.', [ValueType]=N'integer' WHERE [Name]=N'OutOfStockThreshold'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PageBGColorDefault'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PasswordIsOptionalDuringCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFlowPro.LiveURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFlowPro.PARTNER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFlowPro.PWD'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'PayflowPro.RecurringMaxFailPayments'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayflowPro.Reporting.LiveURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayflowPro.Reporting.ReportName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayflowPro.Reporting.TestURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFlowPro.TestURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFlowPro.USER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFlowPro.VENDOR'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFuse.Alias'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFuse.Live_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFuse.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFuse.Test_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayFuse.UserID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYJUNCTION_HTTP_VERSION'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYJUNCTION_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYJUNCTION_LOGON'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYJUNCTION_PASSWORD'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYJUNCTION_SECURITY_DESCRIPTOR'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYJUNCTION_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYMENTECH_BIN'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYMENTECH_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYMENTECH_MERCHANT_ID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYMENTECH_MERCHANT_TERMINAL_ID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYMENTECH_MERCHANT_TZCODE'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PAYMENTECH_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PAYMENTECH_Verify_Addresses'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PaymentExpress.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PaymentExpress.Username'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'invoke', [AllowableValues]=N'AspDotNetStorefrontGateways.GatewayLoader,AspDotNetStorefrontGateways,GetAvailableGatewayNames' WHERE [Name]=N'PaymentGateway'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'multiselect', [AllowableValues]=N'Credit Card,PayPalExpress,PayPal,Request Quote,Purchase Order,Check By Mail,C.O.D.,ECheck,CardinalMyECheck,MicroPay,AmazonSimplePay' WHERE [Name]=N'PaymentMethods'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.API.LiveURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.API.MerchantEMailAddress'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.API.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.API.Signature'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.API.TestURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.API.Username'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.BusinessID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.DefaultLocaleCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PayPal.Express.AllowAnonCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.ButtonImageURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.CancelURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.HeaderBackColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.HeaderBorderColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.HeaderImage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.LiveURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.PageStyle'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.PayFlowColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.ReturnURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Express.SandboxURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PayPal.Express.ShowOnCartPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PayPal.ForceCapture'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.LiveServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.NotificationURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.PaymentIcon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Promo.BannerURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Promo.ButtonImageURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'PayPal.Promo.CartMaximum'
UPDATE [dbo].[AppConfig] SET [Description]=N'The minimum order amount that can be purchased. If an order is less than this amount, customers will not be able to proceed past the shopping cart page. Leave blank for no minimum. Enter ##.## values (no $).', [ValueType]=N'decimal' WHERE [Name]=N'PayPal.Promo.CartMinimum'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Promo.Codes'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PayPal.Promo.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.Promo.LearnMoreURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PayPal.RequireConfirmedAddress'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.ReturnCancelURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.ReturnOKURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PayPal.TestServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PayPal.UseInstantNotification'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PersistFilters'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PinnaclePayments.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PinnaclePayments.SOAPURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PinnaclePayments.UserName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PlugNPay_Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PlugNPay_URL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'PlugNPay_Username'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Polls.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'PreserveActiveCartOnSignin'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'ProductBrowserHeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ProductBrowserHoverColor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ProductImg_icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ProductImg_large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ProductImg_medium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ProductImg_swatch'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QBMERCHANTSERVICES_ApplicationID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QBMERCHANTSERVICES_ApplicationLogin'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QBMERCHANTSERVICES_ApplicationVersion'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QBMERCHANTSERVICES_ConnectionTicket'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QBMERCHANTSERVICES_InstallID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QBMERCHANTSERVICES_Language'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'QUICKCOMMERCE_Verify_Addresses'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_X_DELIM_CHAR'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'QUICKCOMMERCE_X_DELIM_DATA'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_X_EMAIL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'QUICKCOMMERCE_X_EMAIL_CUSTOMER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_X_ENCAP_CHAR'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_X_Login'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_X_METHOD'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'QUICKCOMMERCE_X_RECURRING_BILLING'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'QUICKCOMMERCE_X_RELAY_RESPONSE'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_X_Tran_Key'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'QUICKCOMMERCE_X_VERSION'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RatingsCanBeDoneByAnons'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RatingsCommentFrameVisibility'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RatingsEnabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RatingsPageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ReceiptEMailFrom'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ReceiptEMailFromName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ReceiptHideStoreVersion'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RecentlyViewedProducts.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RecentlyViewedProducts.NumberDisplayed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RecentlyViewedProducts.ProductsFormat'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RecentlyViewedProductsGridColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Recurring.AUTHORIZENET_AUTOBILL_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Recurring.AUTHORIZENET_AUTOBILL_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Recurring.ClearIsNewFlag'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Recurring.DefaultRecurringShippingMethod'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Recurring.DefaultRecurringShippingMethodID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Recurring.GatewayImportOffsetHours'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Recurring.GatewayLastImportedDate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Recurring.LimitCustomerToOneOrder'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Recurring.SendOrderEMailToCustomer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Recurring.SendShippedEMail'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Recurring.UseGatewayInternalBilling'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RedirectLiveToWWW'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RelatedProductsFormat'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RelatedProductsGridColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Reorder.ClearCartBeforeAddingReorderItems'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ReplaceImageURLFromAssetMgr'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RequireOver13Checked'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RequireTermsAndConditionsAtCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ResizableLargeImagePopup'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ResizeSlideWindow'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'multiselect', [AllowableValues]=N'UPS,UPS2,USPS,FEDEX,DHL,CANADAPOST,AUSPOST' WHERE [Name]=N'RTShipping.ActiveCarrier'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RTShipping.AllowLocalPickup'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.AusPost.DefaultPackageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.AusPost.DomesticServices'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.AusPost.IntlServices'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RTShipping.AusPost.MaxWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.CallForShippingPrompt'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.CanadaPost.DefaultPackageSize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.CanadaPost.Language'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RTShipping.CanadaPost.MaxWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.CanadaPost.MerchantID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.CanadaPost.Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RTShipping.CanadaPost.ServerPort'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'RTShipping.DefaultItemWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHL.AccountNumber'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHL.APISystemID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHL.APISystemPassword'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHL.Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RTShipping.DHL.ShipInDays'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHL.TestServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.BillingAccountNbr'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.BillingParty'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.Dutiable'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.DutyPayment'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.DutyPaymentAccountNbr'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RTShipping.DHLIntl.MaxWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.Overrides'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.Packaging'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.Services'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.DHLIntl.ShippingKey'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'multiselect', [AllowableValues]=N'UPS,UPS2,USPS,FEDEX,DHL,CANADAPOST,AUSPOST' WHERE [Name]=N'RTShipping.DomesticCarriers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RTShipping.DumpXmlOnCartPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.FedEx.AccountNumber'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.Fedex.CarrierCodes'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RTShipping.FedEx.MaxWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.FedEx.Meter'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.FedEx.Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.Fedex.ShipDate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RTShipping.Insured'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'multiselect', [AllowableValues]=N'UPS,UPS2,USPS,FEDEX,DHL,CANADAPOST,AUSPOST' WHERE [Name]=N'RTShipping.InternationalCarriers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'RTShipping.LocalPickupCost'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.LocalPickupRestrictionStates'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.LocalPickupRestrictionType'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.LocalPickupRestrictionZips'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.LocalPickupRestrictionZones'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'RTShipping.MarkupPercent'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RTShipping.MultiDistributorCalculation'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.OriginAddress'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.OriginAddress2'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.OriginCity'
UPDATE [dbo].[AppConfig] SET [Description]=N'The 2-character country abbreviation of where you are shipping from.', [ValueType]=N'string' WHERE [Name]=N'RTShipping.OriginCountry'
UPDATE [dbo].[AppConfig] SET [Description]=N'The state/province abbreviation of where you are shipping from', [ValueType]=N'string' WHERE [Name]=N'RTShipping.OriginState'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.OriginZip'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'RTShipping.PackageExtraWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.ShippingMethodsToPrevent'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RTShipping.ShowErrors'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RTShipping.SortByRate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.AccountNumber'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.AddressTypeBehavior'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.DeliveryConfirmation'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RTShipping.UPS.GetNegotiatedRates'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.License'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RTShipping.UPS.MaxWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.Services'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.TestServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.UPSPickupType'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.UPS.UserName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'RTShipping.UseTestRates'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'RTShipping.USPS.MaxWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.USPS.Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.USPS.Services'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.USPS.TestServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.USPS.UserName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'RTShipping.WeightUnits'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayments.MERCHANT_ID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayments.MERCHANT_KEY'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayments.ServiceURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SagePayments.ServiceURLTEST'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SE_MetaDescription'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SE_MetaKeywords'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SE_MetaNoScript'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SE_MetaTitle'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Search_ShowCategoriesInResults'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Search_ShowDistributorsInResults'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Search_ShowGenresInResults'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Search_ShowManufacturersInResults'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Search_ShowProductsInResults'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Search_ShowSectionsInResults'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SearchAdv_ShowCategory'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SearchAdv_ShowDistributor'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SearchAdv_ShowGenre'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SearchAdv_ShowManufacturer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SearchAdv_ShowPriceRange'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SearchAdv_ShowProductType'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SearchAdv_ShowSection'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SearchAdv_ShowSKU'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SectionImg_icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SectionImg_large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SectionImg_medium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SecureNet.ID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SecureNet.Key'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SecureNet.LiveURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SecureNet.TestURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SecurityCodeRequiredOnAdminLogin'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SecurityCodeRequiredOnCreateAccount'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SecurityCodeRequiredOnCreateAccountDuringCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SecurityCodeRequiredOnStoreLogin'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SendOrderEMailToCustomer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SendShippedEMailToCustomer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SendWelcomeEmail'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'SessionTimeoutInMinutes'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'ShippingCostWhenNoZoneMatch'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'ShippingHandlingExtraFee'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'ShippingMethodIDIfFreeShippingIsOn'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'ShippingMethodIDIfItemShippingCostsAreOn'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'ShippingTaxClassID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingCarriers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingRegEx.AusPost'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingRegEx.DHL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingRegEx.FedEx'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingRegEx.UPS'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingRegEx.USPS'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingURL.AusPost'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingURL.DHL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingURL.FedEx'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingURL.UPS'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShippingTrackingURL.USPS'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShipRush.DefaultTemplate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShipRush.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShipRush.LoginID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShipRush.ProvideExtraTemplateInfo'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShipRush.TrackRef'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Shipwire.Password'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Shipwire.Username'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'ShoppingCartItemNotesTextareaCols'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'ShoppingCartItemNotesTextareaRows'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Show404SuggestionLinks'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowBuyButtons'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowCardStartDateFields'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowCartDeleteItemButton'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowCustomerServiceNotesInReceipts'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowDescriptionInTableCondensed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowDimensionsInTableCondensed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowEditButtonInCartForKitProducts'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowEditButtonInCartForPackProducts'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowEditButtonInCartForRegularProducts'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowEMailProductToFriend'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowFullNameInRightBar'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowFullNameInTableCondensed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowFullNameInTableExpanded'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowFullNewsArticle'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowGiftRegistryButtons'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowInventoryTable'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowKitPics'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowManufacturerTree'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowMiniCart'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowPicsInCart'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowPicsInMiniCart'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowPreviousPurchase'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'ShowPriceRegularPrompt'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowQuantityDiscountTablesInline'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowQuantityOnProductPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowShippingAndTaxEstimate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowSpecialsPics'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowSubcatsInGrid'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowSummaryInTableOrderFormat'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowUpsellProductsOnCartPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowWeightInTableCondensed'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ShowWishButtons'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SiteDisclaimerAgreedPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SiteDisclaimerNotAgreedURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteDisclaimerRequired'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteMap.ShowCategories'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteMap.ShowCustomerService'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteMap.ShowDocuments'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteMap.ShowLibraries'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteMap.ShowManufacturers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteMap.ShowProducts'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteMap.ShowSections'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SiteMap.ShowTopics'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Skipjack.DeveloperSerialNumber'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Skipjack.ForceSettlement'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Skipjack.LiveChangeURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Skipjack.LiveSerialNumber'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Skipjack.LiveServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Skipjack.TestChangeURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Skipjack.TestSerialNumber'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Skipjack.TestServer'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SkipPaymentEntryOnZeroDollarCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SkipShippingOnCheckout'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'SlideShowInterval'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'StoreCCInDB'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'StoreName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'StoreVersion'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'double' WHERE [Name]=N'SubscriptionExpiredGracePeriod'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'SubscriptionExtensionOccursFromOrderDate'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Suscription.ExpiredMessageWhenViewingTopic'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'SwatchStyleAuto'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'TaxCalcMode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'TemplateSwitching.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'TRANSACTIONCENTRAL_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'TRANSACTIONCENTRAL_MerchantID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'TRANSACTIONCENTRAL_RegKey'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'TRANSACTIONCENTRAL_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'TRANSACTIONCENTRAL_VOID_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'enum', [AllowableValues]=N'AUTH, AUTH CAPTURE' WHERE [Name]=N'TransactionMode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Tree.CustomerServiceXml'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Tree.ShowCategories'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Tree.ShowCustomerService'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Tree.ShowGenres'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Tree.ShowLibraries'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Tree.ShowManufacturers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Tree.ShowSections'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Tree.ShowVectors'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'TurnOffHtmlEditorInAdminSite'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'TurnOffStoreAdminEMailNotifications'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'UPS.MinimumPackageWeight'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'UpsellProductsGridColWidth'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'UpsellProductsLimitNumberOnCart'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'USAePay.ConnectionError'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'USAePay.Declined'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'USAePay.Description'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'USAePay.Live.Pin'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'USAePay.Live.SourceKey'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'USAePay.Test.Pin'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'USAePay.Test.SourceKey'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'USAePay.TransactionError'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'USAePay.UseSandBox'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseImageResize'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseImagesForMultiNav'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseLiveTransactions'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseMappingShipToPayment'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseNameForSectionDescriptionName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseParserOnEntityDescriptions'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseParserOnObjectDescriptions'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseRolloverForMultiNav'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseSKUForProductDescriptionName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseSKUForProductImageName'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseSSL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'UseStrongPwd'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'ValidateCreditCardNumbers'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VariantImg_icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VariantImg_large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VariantImg_medium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'VAT.AllowCustomerToChooseSetting'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'VAT.CountryID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'VAT.DefaultSetting'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'VAT.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'VAT.HideTaxInOrderSummary'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'VAT.RoundPerItem'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VerifyAddressesProvider'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VerifyAddressesProvider.USPS.Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VerifyAddressesProvider.USPS.UserID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VIAKLIX_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VIAKLIX_SSL_MERCHANT_ID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VIAKLIX_SSL_PIN'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VIAKLIX_SSL_USER_ID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'VIAKLIX_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Watermark.CopyrightImage.Icon'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Watermark.CopyrightImage.Large'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Watermark.CopyrightImage.Medium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Watermark.CopyrightText'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Watermark.Enabled'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Watermark.OffsetFromBottomPercentage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'decimal' WHERE [Name]=N'Watermark.Opacity'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'Web.Config.EncryptionProvider'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'WholesaleOnlySite'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'WorldPay_FixContact'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'WorldPay_HideContact'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'WorldPay_InstallationID'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'WorldPay_LanguageLocale'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'WorldPay_Live_Server'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'WorldPay_OnCancelAutoRedirectToCart'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'WorldPay_RequireAVSMatch'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'WorldPay_ReturnURL'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'WorldPay_TestMode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'WorldPay_TestModeCode'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'WriteFileInUTF8'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.AffiliateSignupNotification'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.DefaultDistributorNotification'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'XmlPackage.DumpTransform'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.EmailGiftCardNotification'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.LostPassword'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.NewOrderAdminNotification'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.NewOrderAdminSMSNotification'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.OrderAsXml'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.OrderCanceledEmail'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.OrderConfirmationPage'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.OrderFinalization'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.OrderReceipt'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.OrderShipped'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.RefundEmail'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'XmlPackage.WelcomeEmail'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'YOURPAY_CONFIGFILE'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'YOURPAY_KEYFILE'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'YOURPAY_LIVE_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'YOURPAY_PORT'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'string' WHERE [Name]=N'YOURPAY_TEST_SERVER'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'YOURPAY_Verify_Addresses'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'ZipCodePrefixLength'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'ZoneIdForNoMatch'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Zoomify.Active'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Zoomify.GalleryMedium'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Zoomify.Large.Height'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Zoomify.Large.Width'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Zoomify.Medium.Height'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'integer' WHERE [Name]=N'Zoomify.Medium.Width'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Zoomify.ProductLarge'
UPDATE [dbo].[AppConfig] SET [ValueType]=N'boolean' WHERE [Name]=N'Zoomify.ProductMedium'

-- ADD NEW GLOBAL CONFIGS
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowProductFiltering', 'MULTISTORE', 'Indicator of whether or not to filter products by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowEntityFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Entities by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowCustomerFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Customers by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('DefaultRedirectURL', 'MULTISTORE', 'Default store to which to redirect', '', 'stringtype')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('UseSharedSSL', 'MULTISTORE', 'Indicator of whether or not to use a shared SSL cert', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('SSLRededirectURL', 'MULTISTORE', 'default redirect url if UseSharedSSL is turned on', '', 'stringtype')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('LicenseKey', 'SYSTEM', 'The license key', '', 'stringtype')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowNewsFiltering', 'MULTISTORE', 'Indicator of whether or not to filter News by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowTopicFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Topic by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowOrderOptionFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Order Option by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowCouponFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Coupon by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowShoppingcartFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Products added to Shoppingcart from different stores', 'true', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowCustomerDuplicateEMailAddresses', 'MULTISTORE', 'Indicator of whether or not to filter Email address globally or per store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowAffiliateFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Affiliate by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowShippingFiltering', 'MULTISTORE', 'Indicator of whether or not to filter shipping calculation mode and methods on a per store basis', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowGiftCardFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Gift Card by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowRatingFiltering', 'MULTISTORE', 'Indicator of whether or not to filter Ratings by store', 'false', 'boolean')
INSERT GlobalConfig([Name], [GroupName], [Description], [ConfigValue], ValueType) VALUES('AllowTemplateSwitchingByLocale', 'DISPLAY', 'Indicator of whether the site should attempt to load different masterpage skins based on the current locale of the browsing customer.  This should only be enabled if you have multiple locales and have created different masterpages for each of your locales (eg. template.en-us.master, template.en-gb.master, etc...).  Enabling this when you do not have multiple locales or when you have not created multiple masterpages that vary by locale can hinder the performance of your site.', 'false', 'boolean')


-- ADD NEW APPCONFIGS
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AddToCart.AddToCartButton', N'The image location for the add to cart button. This must be located in the /skins/skin_#/images folder. Specify just the filename i.e. addtocart.gif.', N'string', NULL, N'SITEDISPLAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AddToCart.AddToGiftRegistryButton', N'The image location for the add to gift registry button. This must be located in the /skins/skin_#/images folder. Specify just the filename i.e. addtogiftregistry.gif.', N'string', NULL, N'SITEDISPLAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AddToCart.AddToWishButton', N'The image location for the add to wishlist button. This must be located in the /skins/skin_#/images folder. Specify just the filename i.e. addwishlist.gif.', N'string', NULL, N'SITEDISPLAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AddToCart.UseImageButton', N'Use images for addtocart buttons', N'boolean', NULL, N'SITEDISPLAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.AccessKey', N'A string distributed by AWS that uniquely identifies an AWS developer. The value of this Id is included in every Amazon Simple Pay request.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.ButtonURL', N'The url of the amazon button that will appear on checkoutpayment.aspx.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.CollectShippingAddress', N'To enable amazon to return through form post the shipping address selected.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.Description', N'The description that shows if you used amazon simple pay as your payment method', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.FPS_LiveServer', N'The live server url of amazon flexible payment service.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.FPS_TestServer', N'The sandbox url of amazon flexible payment service.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.ImmediateReturn', N'After amazon accepts the payment the api returns you to the website immediately if true it displays status page of the payment.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.LiveServer', N'The live server url of amazon business account.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.SecretKey', N'A string distributed by AWS that uniquely identifies an AWS developer.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'AMAZON.TestServer', N'The sandbox url of amazon business account.', NULL, NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'KitInventory.DisableItemSelection', N'If a kit item is tied to a particular variant, setting this to true disables the KitItem selection if its variant goes out of stock. You can also configure this on the Configuration -> Inventory Control page.', NULL, NULL, N'OUTOFSTOCK', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'KitInventory.HideOutOfStock', N'If a kit item is tied to a particular variant, setting this to true hides the KitItem display entirely if its variant goes out of stock. You can also configure this on the Configuration -> Inventory Control page.', NULL, NULL, N'OUTOFSTOCK', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'KitInventory.ShowStockHint', N'If a kit item is tied to a particular variant, setting this to true shows stock notices for that kit item. You can also configure this on the Configuration -> Inventory Control page.', NULL, NULL, N'OUTOFSTOCK', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Layouts.Enabled', N'When enabled layouts can be used throughout the site and the quick edit navigation bar will be enabled.', N'boolean', NULL, N'LAYOUTS', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'MaxMenuLevel', N'The maximum number of menu levels to render for dynamic menu.', NULL, NULL, N'SITEDISPLAY', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Minicart.MaxLatestCartItemsCount', N'the ajax minicart maximum latest cart items count.', N'int', NULL, N'GENERAL', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Minicart.UseAjaxAddToCart', N'Use Ajaxified Minicart Control for adding product to cart', N'boolean', NULL, N'GENERAL', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Moneybookers.Language', N'2-letter code of the language used for Moneybookers'' pages (i.e. EN, DE, ES, etc).', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Moneybookers.LiveServer', N'Live mode URL for Moneybookers maintenance requests.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Moneybookers.MerchantEmail', N'Email address of the Merchant?s Moneybookers account.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Moneybookers.MerchantMD5Password', N'A cryptographic MD5 hash of merchant password. You may require Moneybookers assistance to generate an MD5 merchant password value.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Moneybookers.NotifyMerchant', N'Uses Moneybookers.MerchantEmail to which the transaction details will be sent after the payment process is made. This is optional.', N'boolean', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Moneybookers.RefundURL', N'Refund mode URL for Moneybookers maintenance requests.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Moneybookers.StoreLogoURL', N'The URL of the logo which you would like to appear at the top of the gateway. The logo must be accessible via HTTPS otherwise it will not be shown. For best result we recommend a dimension up to 200px width and 50px height.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Moneybookers.TestServer', N'Test mode URL for Moneybookers maintenance requests.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Netgiro.HostName', N'Should be either the IP of the SSLP host or the IP/DNS name of one of Netgiro?s data centers.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Netgiro.HostPort', N'Should be either the port that the SSLP is listening to or the port for one of Netgiro''s data centers.', N'int', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Netgiro.MerchantCertificateLocation', N'Directory location of Merchant certificate either .pfx or .cer (e.g. C:\cert\merchant.pfx). ', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Netgiro.MerchantCertificatePassword', N'Password for Merchant Certificate. Leave blank if certificate has no password.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Netgiro.MerchantID', N'Merchant ID provided by Netgiro.', N'int', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Netgiro.NetgiroCertificateLocation', N'Directory location of Netgiro certificate either .pfx or .cer (e.g. C:\cert\netgiro.cer).', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Netgiro.NetgiroCertificatePassword', N'Password for Netgiro Certificate. Leave blank if certificate has no password.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Netgiro.URL', N'Netgiro server url. Consult Netgiro for valid url values.', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Newsletter.CaptchaErrorDisplayLength', N'Amount of time to display error message on Newsletter control in seconds', NULL, NULL, N'Newsletter', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Newsletter.GetFirstAndLast', N'whether or not to retrieve a subscribers first and last name when subscribing to the newsletter', NULL, NULL, N'Newsletter', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Newsletter.OptInLevel', N'The level of validation that is required when subscribing.  Valid values are "single", "double", and "triple"', NULL, NULL, N'Newsletter', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Newsletter.OptOutLevel', N'The level of validation that is required when unsubscribing.  Valid values are "double", and "triple"', NULL, NULL, N'Newsletter', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Newsletter.UseCaptcha', N'whether or not the newsletter control protects against scripts with a captcha', NULL, NULL, N'Newsletter', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'PaymentGateway.BackupRetries', N'The number of times to try the backup gatway before failing the order.  Setting this value too high could cause the site to timeout for the user, or have an inordinate delay if the payment gateway does not respond.  It is recommended to set this value to 1 or 2.', N'integer', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'PaymentGateway.PrimaryRetries', N'The number of times to try the primary payment gateway before switching to the backup.  Setting this value too high could cause the site to timeout for the user, or have an inordinate delay if the payment gateway does not respond.  It is recommended to set this value to 1 or 2.', N'integer', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'PaymentGatewayBackup', N'The payment gateway to use as a backup.  Leave this value blank if you do not have a backup payment gateway', N'invoke', N'AspDotNetStorefrontGateways.GatewayLoader,AspDotNetStorefrontGateways,GetAvailableGatewayNames', N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'System.ErrorHandlingEnabled', N'Determines whether application error events trigger logging and handling functions.  This setting should only be disabled when debugging.', N'boolean', NULL, N'SYSTEM', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'System.LoggingEnabled', N'If error handling is enabled, determines whether the error is silently swallowed or logged for troubleshooting later', N'boolean', NULL, N'SYSTEM', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'System.LoggingLocation', N'If error logging is enabled, specifies where the log will be stored.  It is recommended to store logs in the database, however troubleshooting my necessitate storing the error log file elsewhere such as the file system (images/errors directory) or the system event log.  Email error logging is not recommended', N'multiselect', N'Database,File,EventLog,Email', N'SYSTEM', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'System.MaxLogDays', N'The maximum number of days to store log entries.  Entries older than this will be deleted on application start', N'integer', NULL, N'SYSTEM', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'System.MaxLogEntries', N'The maximum number of log entries to store.  If this value is exceeded, the oldest log entry will be deleted to make room for new entries.', N'integer', NULL, N'SYSTEM', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [Description], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'System.ShowFriendlyErrors', N'If error handling is enabled, determines whether a friendly error page will be shown if an error occurs, or if the user will be shown an ASP.NET exception page.', N'boolean', NULL, N'SYSTEM', 1, 0)
GO

-- Delete 12 rows from [dbo].[AppConfig]
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.AccessKey'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.ButtonURL'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.CollectShippingAddress'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.Description'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.FPS_LiveServer'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.FPS_TestServer'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.ImmediateReturn'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.LiveServer'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.SecretKey'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'AMAZON.TestServer'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'MailingMgr.BlockSize'
DELETE FROM [dbo].[AppConfig] WHERE [Name]=N'MailingMgr.SendEachEmailSeparately'

-- Update 69 rows in [dbo].[AppConfig]
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'2CHECKOUT_VendorID'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'AddToCart.AddToCartButton'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'AddToCart.AddToGiftRegistryButton'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'AddToCart.AddToWishButton'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'false' WHERE [Name]=N'AddToCart.UseImageButton'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'CardiaServices.Live.MerchantToken'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'CardiaServices.Live.UserToken'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'CardiaServices.Test.MerchantToken'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'CardiaServices.Test.UserToken'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'eWay.Live.CustomerID'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'eWay.Test.CustomerID'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'-1' WHERE [Name]=N'HideProductsWithLessThanThisInventoryLevel'
UPDATE [dbo].[AppConfig] SET [Description]=N'The Name of the home page template, if different than template.master (e.g. hometemplate.master). Include the .master extension also in what you enter.' WHERE [Name]=N'HomeTemplate'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'false' WHERE [Name]=N'KitInventory.DisableItemSelection'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'false' WHERE [Name]=N'KitInventory.HideOutOfStock'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'false' WHERE [Name]=N'KitInventory.ShowStockHint'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'false' WHERE [Name]=N'Layouts.Enabled'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'1' WHERE [Name]=N'MaxMenuLevel'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'8' WHERE [Name]=N'Minicart.MaxLatestCartItemsCount'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'false' WHERE [Name]=N'Minicart.UseAjaxAddToCart'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'EN' WHERE [Name]=N'Moneybookers.Language'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://www.moneybookers.com/app/payment.pl' WHERE [Name]=N'Moneybookers.LiveServer'
UPDATE [dbo].[AppConfig] SET [Description]=N'Email address of the Merchant''s Moneybookers account.', [ConfigValue]=N'' WHERE [Name]=N'Moneybookers.MerchantEmail'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Moneybookers.MerchantMD5Password'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'FALSE' WHERE [Name]=N'Moneybookers.NotifyMerchant'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://www.moneybookers.com/app/refund.pl' WHERE [Name]=N'Moneybookers.RefundURL'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Moneybookers.StoreLogoURL'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'http://www.moneybookers.com/app/payment.pl' WHERE [Name]=N'Moneybookers.TestServer'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://epayment.bbs.no/terminal/' WHERE [Name]=N'NETAXEPT.Live_UI'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://epayment-test.bbs.no/terminal/' WHERE [Name]=N'NETAXEPT.Test_UI'
UPDATE [dbo].[AppConfig] SET [Description]=N'Should be either the IP of the SSLP host or the IP/DNS name of one of Netgiro''s data centers.', [ConfigValue]=N'' WHERE [Name]=N'Netgiro.HostName'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Netgiro.HostPort'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Netgiro.MerchantCertificateLocation'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Netgiro.MerchantCertificatePassword'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Netgiro.MerchantID'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Netgiro.NetgiroCertificateLocation'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Netgiro.NetgiroCertificatePassword'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'Netgiro.URL'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'3.5' WHERE [Name]=N'Newsletter.CaptchaErrorDisplayLength'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'false' WHERE [Name]=N'Newsletter.GetFirstAndLast'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'double' WHERE [Name]=N'Newsletter.OptInLevel'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'double' WHERE [Name]=N'Newsletter.OptOutLevel'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'true' WHERE [Name]=N'Newsletter.UseCaptcha'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'2' WHERE [Name]=N'PaymentGateway.BackupRetries'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'2' WHERE [Name]=N'PaymentGateway.PrimaryRetries'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'' WHERE [Name]=N'PaymentGatewayBackup'
UPDATE [dbo].[AppConfig] SET [AllowableValues]=N'Credit Card,PayPalExpress,PayPal,Request Quote,Purchase Order,Check By Mail,C.O.D.,ECheck,CardinalMyECheck,MicroPay' WHERE [Name]=N'PaymentMethods'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://www.paypal.com/en_US/i/logo/PayPal_mark_50x34.gif' WHERE [Name]=N'PayPal.PaymentIcon'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'App_Themes/skin_1/images/paypal_promo.gif' WHERE [Name]=N'PayPal.Promo.BannerURL'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://live.sagepay.com/gateway/service/abort.vsp' WHERE [Name]=N'SagePayUKURL.Live.Abort'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://live.sagepay.com/gateway/service/direct3dcallback.vsp' WHERE [Name]=N'SagePayUKURL.Live.Callback'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://live.sagepay.com/gateway/service/vspdirect-register.vsp' WHERE [Name]=N'SagePayUKURL.Live.Purchase'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://live.sagepay.com/gateway/service/refund.vsp' WHERE [Name]=N'SagePayUKURL.Live.Refund'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://live.sagepay.com/gateway/service/release.vsp' WHERE [Name]=N'SagePayUKURL.Live.Release'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://test.sagepay.com/gateway/service/abort.vsp' WHERE [Name]=N'SagePayUKURL.Test.Abort'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://test.sagepay.com/gateway/service/direct3dcallback.vsp' WHERE [Name]=N'SagePayUKURL.Test.Callback'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://test.sagepay.com/gateway/service/vspdirect-register.vsp' WHERE [Name]=N'SagePayUKURL.Test.Purchase'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://test.sagepay.com/gateway/service/refund.vsp' WHERE [Name]=N'SagePayUKURL.Test.Refund'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://test.sagepay.com/gateway/service/release.vsp' WHERE [Name]=N'SagePayUKURL.Test.Release'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'https://certify.securenet.com/payment.asmx' WHERE [Name]=N'SecureNet.TestURL'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'9.0.1.2' WHERE [Name]=N'StoreVersion'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'TRUE' WHERE [Name]=N'System.ErrorHandlingEnabled'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'TRUE' WHERE [Name]=N'System.LoggingEnabled'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'DATABASE' WHERE [Name]=N'System.LoggingLocation'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'30' WHERE [Name]=N'System.MaxLogDays'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'250' WHERE [Name]=N'System.MaxLogEntries'
UPDATE [dbo].[AppConfig] SET [ConfigValue]=N'TRUE' WHERE [Name]=N'System.ShowFriendlyErrors'

-- Add 5 rows to [dbo].[AppConfig]
INSERT INTO [dbo].[AppConfig] ([Name], [StoreID], [Description], [ConfigValue], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'Mobile.EntityPageSize', 1, N'This will control the number of entities that display per page when the site is being browsed from a mobile device.', N'10', N'int', NULL, N'MOBILE', 0, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [StoreID], [Description], [ConfigValue], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'PayLeap.LIVE_URL', 1, N'Your PayLeap SmartPayment assigned gateway LIVE URL', N'https://secure.payleap.com/SmartPayments/transact.asmx', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [StoreID], [Description], [ConfigValue], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'PayLeap.Password', 1, N'Your PayLeap SmartPayment assigned account password', N'', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [StoreID], [Description], [ConfigValue], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'PayLeap.TEST_URL', 1, N'Your PayLeap SmartPayment assigned gateway TEST URL', N'http://test.payleap.com/SmartPayments/transact.asmx', N'string', NULL, N'GATEWAY', 1, 0)
INSERT INTO [dbo].[AppConfig] ([Name], [StoreID], [Description], [ConfigValue], [ValueType], [AllowableValues], [GroupName], [SuperOnly], [Hidden]) VALUES (N'PayLeap.UserName', 1, N'Your PayLeap SmartPayment assigned account Username', N'', N'string', NULL, N'GATEWAY', 1, 0)
GO

-- ADD TOPIC MAPPINGS
INSERT dbo.TopicStore (TopicID, StoreID)
	SELECT TopicID, 1 FROM dbo.Topic WHERE TopicID NOT IN (SELECT TopicID FROM TopicStore WHERE StoreID = 1)
GO

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

/************* TFS 552 **************************/
/*    This goes in the 8012-90 upgrade script   */
/************************************************/
PRINT N'Altering [dbo].[ZipTaxRate]'
GO
IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = object_id('ZipTaxRate') AND name = 'CountryID') 
	BEGIN
		DECLARE @countryID int
		SELECT @countryID = CountryID FROM Country WHERE Name='United States'
		DECLARE @SQL nvarchar(max)
		SET @sql = 'ALTER TABLE [dbo].[ZipTaxRate] ADD
		[CountryID] [int] NOT NULL CONSTRAINT [DF_ZipTaxRate_CountryID] DEFAULT ((' + CAST(@countryID as nvarchar) + '))'
		EXEC(@sql)
	END
GO

/************* END TFS 552 ****************/
